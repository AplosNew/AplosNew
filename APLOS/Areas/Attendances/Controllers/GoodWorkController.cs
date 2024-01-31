#region Using

using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Setups;
using Library.OrderManagement.Sales;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class GoodWorkController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly AccountVoucherReportService _accountVoucherReportService;
        clsSales clsSales = new clsSales();
        public GoodWorkController(ISqlRepository R, AccountVoucherReportService accountVoucherReportService)
        {
            _sqlRepository = R;
            _accountVoucherReportService = accountVoucherReportService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult PCAAC()
        {
            return View();
        }
        public ActionResult GoodWorkCheck()
        {
            return View();
        }
        public ActionResult GoodWorkApprove()
        {
            return View();
        }
        public ActionResult GWPaymnetDisburse()
        {
            return View();
        }
        //Load Employee
        [HttpPost]
        public ActionResult LoadEmployeelist(Dictionary<string, string> parameters, string userGroupId, string shiftId, string workDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            var ec = ""; var dep = ""; var sec = ""; var subsec = ""; var des = ""; var userGr = "";
            if (parameters["EmpCategoryId"] != "null")
            {
                ec = "and EC.Id in (" + parameters["EmpCategoryId"] + @")";
            }
            if (parameters["DepartmentId"] != "null")
            {
                dep = "and DP.Id in (" + parameters["DepartmentId"] + @")";
            }
            if (parameters["SectionId"] != "null")
            {
                sec = "and EI.SectionId in (" + parameters["SectionId"] + @")";
            }
            if (parameters["SubSectionId"] != "null")
            {
                subsec = "and EI.SubSectionId in (" + parameters["SubSectionId"] + @")";
            }
            if (parameters["DesignationId"] != "null")
            {
                des = "and EI.LegalDesignationId in (" + parameters["DesignationId"] + @")";
            }
            if (parameters["UserReportGroupId"] != "null")
            {
                userGr = "and isnull(PR.Id,'') in (" + parameters["UserReportGroupId"] + @")";
            }
            try
            {
                sql = @"SELECT '' Id,0 CheckBoxSelect, EI.SystemId
                         ,EI.EmployeeCode
                         ,EI.EmployeeName,ei.EmployeeCodePreFix,ei.EmployeeCodeNumeric
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         ,S.UserName Section,SS.UserName SubSection, DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         ,EI.EmployeeStatus,APD.OverStay,APD.DayStatus
						 ,OTTitle = case when EI.ExcludeOT=0 then 'Yes' else 'No' end
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId	
                         LEFT join MST.DesignationMaster DM on DM.DesignationId=EI.GivenDesignationId
						 LEFT join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
                         LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                         LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                         LEFT JOIN GoodWorkDetail GWD on GWD.EmpSystemId=EI.SystemId
                         left join GoodWork GW on GW.Id=GWD.GoodWorkId
						 left join dbo.AttdnProcessData APD on APD.EmpSystemID=EI.SystemId and APD.WorkDate='" + workDate + @"'

                         WHERE  EI.PlantId='" + identity.PlantId + @"'  " + ec + @"  " + dep + @"  " + sec + @"   " + subsec + @"   " + des + @" " + userGr + @"
                         and ei.SystemId in (select EmpSystemID from EmployeeShiftAssign where FixSystemID='" + shiftId + @"')  
                        and EI.EmployeeStatus='Active' and EI.BudgetCode in (SELECT BudgetId FROM dbo.GoodWorkBudgetSetup where GoodWorkSetUpId = '" + userGroupId + @"') 
                         ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpGet, Authorize]
        public ActionResult GetAllActiveEmpData()
        {
            JsonResult json = Json(clsSales.GetAllEmployeeData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeCategoryList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsDiscreteAllowanceReport ep = new clsDiscreteAllowanceReport();
                return Json(ep.GetEmployeeCategoryList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        //Load Employee

        //Good Work

        public ActionResult DeleteChildUrl(string Id)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.GoodWorkDetail where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CreateGoodWork(Dictionary<string, object> data, List<Dictionary<string, object>> goodWorkDetail)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail;
                DataSet dsDD = null;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from GoodWork where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("GoodWork", out _Id);
                    }
                    data["Id"] = _Id;
                    data["CheckedStatus"] = "To Be Checked";
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update

                #region Good Work Detail

                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from GoodWorkDetail where GoodWorkId='" + _MasterId + "'", out dsDetail, false, "1");
                con.OpenDataSetThroughAdapter("select count(Id) countId from GoodWorkDetail where GoodWorkId='" + _MasterId + "'", out dsDD, false, "1");
                int ccount = Convert.ToInt32(dsDD.Tables[0].Rows[0]["countId"].ToString());
                if (goodWorkDetail != null)
                {
                    foreach (var item in goodWorkDetail)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            item["Id"] = detailid;
                            item["goodWorkId"] = _MasterId;
                            item["EmpSystemId"] = item["SystemId"];
                            item["FromTime"] = item["FromTime"];
                            item["ToTime"] = item["ToTime"];
                            item["Purpose"] = item["Purpose"];
                            item["PurposeCategory"] = item["PurposeCategory"];
                            item["Minute"] = item["Minute"];
                            item["Remark"] = item["Remark"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["FromTime"] = item["FromTime"];
                            drmo["ToTime"] = item["ToTime"];
                            drmo["Minute"] = item["Minute"];
                            drmo["Purpose"] = item["Purpose"];
                            drmo["PurposeCategory"] = item["PurposeCategory"];
                            drmo["Remark"] = item["Remark"];
                            drmo.EndEdit();
                        }
                    }
                }
                #endregion Good Work Detail
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.GoodWorkDetail where GoodWorkId='" + Id + "'");
                con.executeQuery("delete from dbo.GoodWork where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkList(string workDate)
        {
            string sql = @"select GW.Id,format(GW.WorkDate,'dd-MMM-yyyy') WorkDate,gw.ShiftId,S.UserName Shift,GW.Remarks,GWS.UserName UserGroup,GWS.Id UserGroupId,gw.Reason
                                    ,format(GW.FromTime,'hh:mm') FromTime,format(GW.ToTime,'hh:mm') ToTime,gw.Minute,gw.CheckedBy,gw.CheckedStatus,GW.ApprovedStatus
                                    ,cast(case when format(GW.WorkDate,'dd-MMM-yyyy')=format(GETDATE(),'dd-MMM-yyyy') 
									or format(GW.WorkDate,'dd-MMM-yyyy')=format(dateadd(day,-1,getdate()),'dd-MMM-yyyy')
									then 1 else 0 end as bit) WD
                                    from GoodWork GW
                                    left join ShiftDefination S on S.SystemId=GW.ShiftId
									left join [dbo].[GoodWorkSetup] GWS on GWS.Id=GW.UserGroupId
									left join EmployeeInformation ei on ei.SystemId=gw.CheckedBy
                                    where GW.WorkDate= '" + workDate + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetMinute(GoodWorkTransaction data)
        {
            var tst = data.ToTime.Subtract(data.FromTime);
            int f = (int)tst.TotalMinutes;
            if (f < 0)
            {

                DateTime FromDt = Convert.ToDateTime(data.FromTime);
                DateTime ToDt = Convert.ToDateTime(data.ToTime);
                //TimeSpan t = ToDt.Subtract(FromDt);
                //int N = t.Days;

                TimeSpan ts;
                DateTime date1 = Convert.ToDateTime(data.FromTime);
                DateTime date2 = Convert.ToDateTime(data.ToTime);
                //DateTime NextDayDate = date2.AddDays(N);
                if (FromDt == ToDt)
                {
                    ts = date2 - date1;
                }
                else
                {
                    DateTime NextDayDate2 = date2.AddDays(1);
                    ts = NextDayDate2 - date1;
                }
                f = (int)ts.TotalMinutes;
            }
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllActiveEmployeeData()
        {
            JsonResult json = Json(clsSales.GetAllGoodWorkEmployeeData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        public ActionResult GetGoodWorkDetailCenter(string goodWorkId)
        {
            string str = @"select GWD.Id,EI.SystemId,EI.EmployeeCode,EI.EmployeeName,format(GWD.FromTime,'hh:mm') FromTime,format(GWD.ToTime,'hh:mm') ToTime
,GWD.Minute CalculatedTime
							,GWD.Purpose,GWD.PurposeCategory,ec.Id EmployeeCategoryId,EC.UserName EmployeeCategory,GWD.[Minute],GWD.Remark
							,PR.GoodWorkPositionCodeId UserGroupId,PR1.UserReportGroup UserGroup,ei.GivenDesignationId DesignationId,D.UserName Designation,S.Id SectionId,S.UserName Section,SS.Id SubSectionId,SS.UserName SubSection,DEPT.Id DepartmentId,DEPT.UserName Department
                            ,APD.OverStay,APD.DayStatus
                            from GoodworkDetail GWD 
                            left join EmployeeInformation EI on EI.SystemId=GWD.EmpSystemId 
							LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            LEFT JOIN ORG.Department DEPT ON EI.DepartmentId=DEPT.Id
							LEFT join MST.DesignationMaster DM on DM.DesignationId=EI.GivenDesignationId
							LEFT join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
							LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
							LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
							LEFT JOIN ORG.Position PR1 ON PR1.Id=PR.GoodWorkPositionCodeId
							left join hkp.Designation D on D.Id=ei.GivenDesignationId
                            left join GoodWork GW on GW.Id=GWD.GoodWorkId
							left join dbo.AttdnProcessData APD on APD.EmpSystemID=EI.SystemId and APD.WorkDate=GW.WorkDate
                            where GWD.GoodWorkId in ('" + goodWorkId + "')";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getUserGroupData()
        {
            try
            {
                string strSQL = @"select distinct UserReportGroup from org.position";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult getFiltersData(string userGroupId, string shiftId)
        {
            try
            {
                var sql = @"SELECT EC.Id EmpCategoryId,EC.UserName EmployeeCategory
                         ,DG.Id DesignationId ,DG.UserName LegalDesignation
                         ,S.Id SectionId,S.UserName Section,SS.Id SubSectionId,SS.UserName SubSection,DP.Id DepartmentId,DP.UserName Department
                         ,PR.Id UserReportGroupId,PR.UserReportGroup 
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId	
                         LEFT join MST.DesignationMaster DM on DM.DesignationId=EI.GivenDesignationId
						 LEFT join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
                         LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                         LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
						 where EI.EmployeeStatus='Active' and ei.SystemId in (select EmpSystemID from EmployeeShiftAssign where FixSystemID='" + shiftId + @"') and EI.BudgetCode in (SELECT BudgetId FROM dbo.GoodWorkBudgetSetup where GoodWorkSetUpId= '" + userGroupId + @"')";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public class GoodWorkTransaction
        {
            #region Scalar Properties

            public DateTime WorkDate { get; set; }
            public DateTime YesterDay { get; set; }
            public DateTime FromTime { get; set; }
            public DateTime ToTime { get; set; }
            public int Minute { get; set; }

            #endregion Scalar Properties

            #region Audit Properties

            /// <summary>
            ///This is  AddedBy.Who add data keep track by AddedBy.
            /// </summary>
            [NeverUpdate]
            public string AddedBy { get; set; }

            /// <summary>
            ///This is  AddedDate.Added date keep track by AddedDate.
            /// </summary>
            [NeverUpdate]
            public DateTime AddedDate { get; set; }

            /// <summary>
            /// Record insert by user from IP address.
            /// </summary>
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            /// <summary>
            /// Record updated user name.
            /// </summary>
            public string UpdatedBy { get; set; }

            /// <summary>
            /// Record updated by user date and time.
            /// </summary>
            public DateTime? UpdatedDate { get; set; }

            /// <summary>
            /// Record updated by user IP address.
            /// </summary>
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }


        [HttpPost, Authorize]
        public ActionResult GetGoodWorkReport(string reportFileName, string workDate)
        {
            try
            {
                string fileName = "";
                fileName = GoodWorkReportxlx("", reportFileName, workDate);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GoodWorkReportxlx(string ReportHeader, string reportFileName, string workDate)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Good Work Report";
                sheet = workbook.Worksheets[0];
                int ROW = 5; int COL = 1;
                DataTable data = getGWReportData(workDate);

                #region columns
                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColEmployeeCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Work Date";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColWorkDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Over Time";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColOverTime = COL;
                COL++;

                sheet[ROW, COL].Text = "Over Stay";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColOverStay = COL;
                COL++;

                sheet[ROW, COL].Text = "Day Status";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColDayStatus = COL;

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColWorkDate].Text = data.Rows[i]["WorkDate"].ToString();
                    sheet[ROW, ColOverTime].Number = clsStaticInfo.dbl(data.Rows[i]["OverTime"].ToString());
                    sheet[ROW, ColOverTime].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColOverStay].Number = clsStaticInfo.dbl(data.Rows[i]["OverStay"].ToString());
                    sheet[ROW, ColOverStay].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColDayStatus].Text = data.Rows[i]["DayStatus"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Good Work Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable getGWReportData(string workDate)
        {
            try
            {
                var sql = @"select format(GW.WorkDate,'dd-MMM-yyyy') WorkDate,EI.EmployeeCode,EI.EmployeeName,EC.UserName EmployeeCategory,S.UserName Section
							,DEPT.UserName Department,GWD.Minute OverTime,APD.OverStay,APD.DayStatus
                            from GoodWork GW 
                            left join GoodworkDetail GWD on GW.Id=GWD.GoodWorkId
                            left join EmployeeInformation EI on EI.SystemId=GWD.EmpSystemId 
							LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            LEFT JOIN ORG.Department DEPT ON EI.DepartmentId=DEPT.Id
							LEFT join MST.DesignationMaster DM on DM.DesignationId=EI.GivenDesignationId
							LEFT join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
							LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
							LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
							LEFT JOIN ORG.Position PR1 ON PR1.Id=PR.GoodWorkPositionCodeId
							left join hkp.Designation D on D.Id=ei.GivenDesignationId
							left join dbo.AttdnProcessData APD on APD.EmpSystemID=EI.SystemId and APD.WorkDate=GW.WorkDate
                            where GW.WorkDate ='" + workDate + "' and APD.DayStatus <> 'A'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        #region Payable Creation and Worker Advance

        [HttpGet, Authorize]
        public ActionResult GetWorkerAdvanceList()
        {
            string sql = @"select wa.Id,FORMAT(FromDate,'dd-MMM-yy')FromDate,FORMAT(ToDate,'dd-MMM-yy')ToDate,UserRef,wa.YearNo,wa.MonthNo
						        ,wa.PayDaysType,NoOfDays,Percentage,wa.Remarks
                                ,ei.SystemId PreparedById,ei.EmployeeName PreparedBy
						        ,ei2.SystemId CheckedById,ei2.EmployeeName CheckedBy
						        ,ei3.SystemId ApprovedById,ei3.EmployeeName ApprovedBy
                                  from [dbo].[WorkerAdvance] wa
                                  LEFT JOIN dbo.EmployeeInformation AS ei ON ei.SystemId=wa.PreparedById
                                  LEFT JOIN dbo.EmployeeInformation AS ei2 ON ei2.SystemId=wa.CheckedById
                                  LEFT JOIN dbo.EmployeeInformation AS ei3 ON ei3.SystemId=wa.ApprovedById";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWorkerAdvanceDetailCenter(string workAdvanceId)
        {
            string str = @"select ei.SystemId EmpSystemId,ei.EmployeeCode,ei.EmployeeName,s.UserName Section,ss.UserName SubSection,wad.Id
							,wa.Id workAdvanceId,gwpd.Id GoodWorkPaymentDetailId,d.UserName Department,wad.PayDays,wad.Amount,x.[Basic]
                            ,wad.RatePerDay,wad.RatePerHour,wad.AdvanceGiven,wad.NetPayable
                            from [dbo].[WorkerAdvanceDetail] wad
                            left join [dbo].[WorkerAdvance] wa on wa.Id=wad.WorkerAdvanceId
                            left join EmployeeInformation ei on ei.SystemId=wad.EmpSystemId
                            left join org.Section AS s ON s.Id=ei.SectionId
                            left join org.SubSection AS ss ON ss.Id=ei.SubSectionId
                            left join org.Department d on d.Id=ei.DepartmentId
                            LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = ei.SystemId
                            left join GoodWorkPaymentDetail gwpd on gwpd.WorkerAdvanceId=wa.Id and gwpd.EmpSystemId=wad.EmpSystemId
                            LEFT JOIN (SELECT SID.DefineAmount Basic,SH.SalaryHead,SID.SalaryID
                                      FROM SalaryInfoDefine SID 
                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                      WHERE SH.HeadCategory='Basic')x ON x.SalaryID = SIDM.SystemID
                            where wad.WorkerAdvanceId in ('" + workAdvanceId + "')";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetIssueSlipCheckByCbo()
        {
            var sql = @"select E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text from dbo.GoodWorkCheckBySetUp  A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.CheckById 
                          where E.EmployeeStatus='Active'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetApprovedByCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text from dbo.GoodWorkAuthoritySetUp A 
                         Inner JOin dbo.EmployeeInformation E On E.systemId=A.AuthorityId 
                         where   E.EmployeeStatus='Active'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateWorkerAdvance(Dictionary<string, object> data, List<Dictionary<string, object>> workerAdvanceDetail)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[WorkerAdvance] where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                string _Id = "";

                #region data update Worker Advance
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("WorkerAdvance", out _Id);
                    }
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update  Worker Advance

                #region  Worker Advance Detail

                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from [dbo].[WorkerAdvanceDetail] where  WorkerAdvanceId='" + _MasterId + "'", out dsDetail, false, "1");
                int ccount = 0;
                if (workerAdvanceDetail != null)
                {
                    foreach (var item in workerAdvanceDetail)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailId = materialCommonService.MakePK(_MasterId, ccount, 2);

                            item["Id"] = detailId;
                            item["WorkerAdvanceId"] = _MasterId;
                            item["EmpSystemId"] = item["SystemId"];
                            item["GoodWorkPaymentDetailId"] = item["GoodWorkPaymentDetailId"];
                            item["PayDays"] = item["PayDays"];
                            //item["Amount"] = item["Amount"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            if (item["PayDays"] == null)
                            {
                                item["PayDays"] = 0;
                            }
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();

                            drmo["WorkerAdvanceId"] = _MasterId;
                            drmo["EmpSystemId"] = item["EmpSystemId"];
                            item["GoodWorkPaymentDetailId"] = item["GoodWorkPaymentDetailId"];
                            drmo["PayDays"] = item["PayDays"];
                            drmo["Amount"] = item["Amount"];

                            drmo.EndEdit();
                        }
                    }
                }

                #endregion  Worker Advance Detail
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost]
        public ActionResult LoadPCAACEmployeelist(string fromDate, string toDate, string payDaysType)
        {
            string sql = string.Empty;
            try
            {
                string pDays = null;
                if (payDaysType == "ExtraOT")
                {
                    pDays = @"LEFT JOIN(select isnull(SUM(PresentValue),0) PayDays,isnull(SUM(StandardOT),0) StandardOT,isnull(SUM(AdditionalOT),0) AdditionalOT,0 GoodWork,EmpSystemID,OverStay    
							from AttdnProcessData 
                            where WorkDate between '" + fromDate + @"' and '" + toDate + @"'
                            GROUP BY EmpSystemID,OverStay)y on y.EmpSystemID = EI.SystemId ";
                }
                else
                {
                    pDays = @"LEFT JOIN(select (SUM(cast(gwd.Minute as decimal)/1440)) PayDays,0 AdditionalOT,0 StandardOT,isnull(sum(gwd.Minute),0) GoodWork,EmpSystemID  
							from GoodWorkDetail gwd
							LEFT JOIN GoodWork AS gw ON gw.Id=gwd.GoodWorkId
                            where gw.WorkDate between '" + fromDate + @"' and '" + toDate + @"' and gw.ApprovedStatus='Approved'
                            GROUP BY EmpSystemID)y on y.EmpSystemID = EI.SystemId ";
                }

                sql = @"SELECT '' Id,0 CheckBoxSelect, EI.SystemId
                         ,EI.EmployeeCode
                         ,EI.EmployeeName,ei.EmployeeCodePreFix,ei.EmployeeCodeNumeric
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         ,S.UserName Section,SS.UserName SubSection, DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         ,EI.EmployeeStatus
						 ,OTTitle = case when EI.ExcludeOT=0 then 'Yes' else 'No' END
						,convert(numeric(10,2),x.DefineAmount) Basic,x.SalaryHead,convert(numeric(10,2),y.PayDays)PayDays,y.StandardOT,y.AdditionalOT,y.GoodWork
                         ,convert(numeric(10,2),g.RatePerHour)RatePerHour,convert(numeric(10,2),g.RatePerDay)RatePerDay,0 AdvanceGiven,'' GoodWorkPaymentDetailId
						 ,convert(numeric(10,2),g.Gross) Gross
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId	
                         LEFT join MST.DesignationMaster DM on DM.DesignationId=EI.GivenDesignationId
						 LEFT join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
                         LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                         LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                         LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
                         LEFT JOIN (SELECT SID.DefineAmount,SH.SalaryHead,SID.SalaryID
                                      FROM SalaryInfoDefine SID 
                         LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                    WHERE SH.HeadCategory='Basic')x ON x.SalaryID = SIDM.SystemID
                        LEFT JOIN(SELECT ((SID.DefineAmount/208)*2) RatePerHour,(((SID.DefineAmount/208)*2)*2) RatePerDay,SH.SalaryHead,SID.SalaryID,SID.DefineAmount Gross
                                      FROM SalaryInfoDefine SID 
                         LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                    WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
                         " + pDays + @"
                         where EI.employeeCode<>''  and EI.EmployeeStatus='Active' and y.PayDays<>0
                         and isnull(ei.SystemId,'') not in (select wad.EmpSystemId 
						 from WorkerAdvance wa
						 left join WorkerAdvanceDetail wad on wad.WorkerAdvanceId=wa.Id
						 where wa.FromDate between '" + fromDate + @"' and '" + toDate + @"'
						 and wa.ToDate between '" + fromDate + @"' and '" + toDate + @"'
						 )
                         ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public ActionResult DeleteWorkerAdvanceChildUrl(string Id)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.WorkerAdvanceDetail where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadPCEmployeelist(string fromDate, string toDate, string tabName)
        {
            string sql = string.Empty;
            try
            {
                if (tabName == "GoodWork")
                {
                    sql = @"select '' Id,ei.SystemId EmpSystemId,ei.EmployeeCode,ei.EmployeeName,sum(gw.Minute) Minute,(sum(gw.Minute)/60) Hour
                                    ,format(g.Gross,'N2') Gross,format(g.RatePerHour,'N2') Rate,Amount=format((sum(gw.Minute)/60)*g.RatePerHour,'N2')
                                     from [dbo].[GoodWork] gw
                                     left join  GoodWorkDetail GWD on GWD.GoodWorkId=gw.Id 
                                     left join EmployeeInformation ei on ei.SystemId=GWD.EmpSystemId  
                                     LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
                                     LEFT JOIN(SELECT SID.DefineAmount Basic,((SID.DefineAmount/208)*2) RatePerHour,SH.SalaryHead
								                                      ,SID.SalaryID,SID.DefineAmount Gross
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
                                     where gw.WorkDate between '" + fromDate + @"' and '" + toDate + @"' and gw.Minute<>0 and g.Gross<>0  and SIDM.IsApproved=1
                                     and gwd.EmpSystemId not in (select EmpSystemId
									                                    from GoodWorkPaymentAdvise gwpa
									                                    left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
									                                    where convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"') 
                                     group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross,g.RatePerHour";
                }
                else if(tabName == "ExtraOT")
                {
                    sql = @"select ei.SystemId EmpSystemId,'' Id,ei.EmployeeCode,ei.EmployeeName,format(sum(apd.OverStay),'N2') Minute
                                ,format((sum(apd.OverStay)/60),'N2') Hour
                                ,format(g.Gross,'N2') Gross
                                ,format(g.RatePerHour,'N2') Rate 
                                ,Amount=format(g.RatePerHour*(sum(apd.OverStay)/60),'N2'),apd.GWPaymentAdviseId 

                                from [dbo].[AttdnProcessData] apd 
                                left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID  
                                LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
								                                  LEFT JOIN(SELECT ((SID.DefineAmount/208)*2) RatePerHour,SH.SalaryHead
								                                  ,SID.SalaryID,SID.DefineAmount Gross
                                                                      FROM SalaryInfoDefine SID 
								                                  LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                    WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
                                where apd.WorkDate between '" + fromDate + @"' and '" + toDate + @"' AND DayStatus IN('P','W','L') AND OverStay<>0 AND apd.IsOTEntitled=1 and SIDM.IsApproved=1
                                and apd.EmpSystemID not in (select EmpSystemId
									                                from GoodWorkPaymentAdvise gwpa
									                                left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
									                                where convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
									                                )
                                --and apd.EmpSystemID=2010692
                                group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross,g.RatePerHour,apd.GWPaymentAdviseId ";
                }
                else
                {
                    sql = @"select ei.SystemId EmpSystemId,'' Id,ei.EmployeeCode,ei.EmployeeName,format(sum(apd.OverStay),'N2') Minute
                                ,format((sum(apd.OverStay)/60),'N2') Hour
                                ,format(g.Gross,'N2') Gross
                                ,format(g.RatePerHour,'N2') Rate 
                                ,Amount=format(g.RatePerHour*(sum(apd.OverStay)/60),'N2'),apd.GWPaymentAdviseId 
                                ,gwpa.Remarks
                                from [dbo].[AttdnProcessData] apd 
                                left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID
                                left join GoodWorkPaymentAdvise gwpa on gwpa.Id=apd.GWPaymentAdviseId
                                LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
								                                  LEFT JOIN(SELECT ((SID.DefineAmount/208)*2) RatePerHour,SH.SalaryHead
								                                  ,SID.SalaryID,SID.DefineAmount Gross
                                                                      FROM SalaryInfoDefine SID 
								                                  LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                    WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
                                where apd.WorkDate between '" + fromDate + @"' and '" + toDate + @"' AND DayStatus IN('P','W','L') AND OverStay<>0 AND apd.IsOTEntitled=1 and SIDM.IsApproved=1 
                                and apd.EmpSystemID in (select EmpSystemId
									                                from GoodWorkPaymentAdvise gwpa
									                                left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
									                                where convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"' and gwpad.IsDisburse<>1
									                                )
                                group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross,g.RatePerHour,apd.GWPaymentAdviseId,gwpa.Remarks";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost]
        public JsonResult CreatePayableCreationWorkerAdvance(Dictionary<string, object> data, List<Dictionary<string, object>> goodWorkPaymentDetail)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail, dsAdvance;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkPayment] where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                string _Id = "";

                #region data update Good Work Payment
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("GoodWorkPayment", out _Id);
                    }
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update Good Work Payment

                #region  Good Work Payment Detail

                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkPaymentDetail] where  GoodWorkPaymentId='" + _MasterId + "'", out dsDetail, false, "1");

                string id = "";
                foreach (var item in goodWorkPaymentDetail)
                {
                    if (id == "")
                        id = "'" + item["WorkerAdvanceDetailId"] + "'";
                    else
                        id = id + ",'" + item["WorkerAdvanceDetailId"] + "'";
                }

                con.OpenDataSetThroughAdapter("select * from [dbo].[WorkerAdvanceDetail] where Id in (" + id + ")", out dsAdvance, false, "1");
                int ccount = 0;

                if (goodWorkPaymentDetail != null)
                {
                    foreach (var item in goodWorkPaymentDetail)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailId = materialCommonService.MakePK(_MasterId, ccount, 2);

                            item["Id"] = detailId;
                            item["GoodWorkPaymentId"] = _MasterId;
                            item["EmpSystemId"] = item["EmpSystemId"];
                            item["WorkerAdvanceId"] = item["WorkerAdvanceId"];
                            item["Amount"] = item["Amount"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        else
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();

                            //drmo["WorkerAdvanceId"] = _MasterId;
                            //drmo["EmpSystemId"] = item["EmpSystemId"];
                            //drmo["PayDays"] = item["PayDays"];
                            //drmo["Amount"] = item["Amount"];

                            drmo.EndEdit();
                        }


                        DataView dvAdvance = new DataView(dsAdvance.Tables[0]);
                        dvAdvance.RowFilter = "Id='" + item["WorkerAdvanceDetailId"] + "' and EmpSystemId = '" + item["EmpSystemId"] + "'";

                        if (dvAdvance.Count > 0)
                        {
                            DataRow drAdvance = dvAdvance[0].Row;
                            drAdvance.BeginEdit();

                            drAdvance["GoodWorkPaymentDetailId"] = item["Id"];

                            drAdvance.EndEdit();
                        }

                    }
                }

                #endregion  Good Work Payment Detail
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail, dsAdvance);

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CreateGoodWorkPayableCreation(Dictionary<string, object> data, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail, string tabName)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster, dsDetail;
                DataSet dsGWPayable = null;
                DataSet dsExtraOT = null;
                DataSet dsDD = null;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkPaymentAdvise] where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region data update Good Work Payment Advise
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("GoodWorkPaymentAdvise", out _Id);
                    }
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update Good Work Payment Advise

                #region  Good Work Payment Advise Detail
                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from [dbo].[GoodWorkPaymentAdvisedetail] where PaymentAdviseId='" + _MasterId + "'", out dsDetail, false, "1");
                con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[GoodWorkPaymentAdvisedetail] where PaymentAdviseId='" + _MasterId + "'", out dsDD, false, "1");

                int ccount = Convert.ToInt32(dsDD.Tables[0].Rows[0]["countId"].ToString());
                if (goodWorkPaymentAdviseDetail != null)
                {
                    foreach (var item in goodWorkPaymentAdviseDetail)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailId = materialCommonService.MakePK(_MasterId, ccount, 2);

                            item["Id"] = detailId;
                            item["PaymentAdviseId"] = _MasterId;
                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        else
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["Remarks"] = item["Remarks"];
                            drmo.EndEdit();
                        }
                    }
                }
                #endregion  Good Work Payment Advise Detail

                if (tabName == "GoodWork")
                {
                    con.OpenDataSetThroughAdapter(@"select * from GoodWorkDetail gwd
                                                where gwd.GoodWorkId in (select Id from GoodWork gw where gw.WorkDate between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"')", out dsGWPayable, false, "1");

                    for (int i = 0; i < dsGWPayable.Tables[0].Rows.Count; i++)
                    {
                        dsGWPayable.Tables[0].DefaultView.RowFilter = "Id='" + dsGWPayable.Tables[0].Rows[i]["Id"] + "'";

                        if (dsGWPayable.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow drGW = dsGWPayable.Tables[0].DefaultView[0].Row;
                            drGW.BeginEdit();

                            drGW["GWPaymentAdviseId"] = _MasterId;
                            drGW.EndEdit();
                        }
                    }
                }
                else
                {
                    con.OpenDataSetThroughAdapter(@"select * from [dbo].[AttdnProcessData] apd 
                        where apd.WorkDate between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"' AND apd.DayStatus IN('P','W','L') AND apd.OverStay<>0 
                        and apd.EmpSystemID not in (select EmpSystemId                              
                        from GoodWorkPaymentAdvise gwpa                              
                        left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id                              
                        where convert( DateTime, gwpa.FromDate) between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"' and convert( DateTime, gwpa.ToDate) between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"'
                        )
                        AND apd.IsOTEntitled in(select D.IsOTEntitled
                        from [dbo].[AttdnProcessData] apd 
                        left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID  
                        left join (SELECT C.IsOTEntitled,D.Id FROM SCS.DesignationMasterConfiguration C
                                                            LEFT JOIN MST.DesignationMaster M ON M.Id=C.DesignationMasterId
                                                            LEFT JOIN HKP.Designation D ON D.Id=M.DesignationId
									                        where C.IsOTEntitled=1
							                                )D on D.Id=ei.GivenDesignationId
                        )", out dsExtraOT, false, "1");

                    for (int i = 0; i < dsExtraOT.Tables[0].Rows.Count; i++)
                    {
                        dsExtraOT.Tables[0].DefaultView.RowFilter = "EmpSystemID='" + dsExtraOT.Tables[0].Rows[i]["EmpSystemID"] + "' and WorkDate='" + dsExtraOT.Tables[0].Rows[i]["WorkDate"] + "'";

                        if (dsExtraOT.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow drGW = dsExtraOT.Tables[0].DefaultView[0].Row;
                            drGW.BeginEdit();

                            drGW["GWPaymentAdviseId"] = _MasterId;
                            drGW.EndEdit();
                        }
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail, dsGWPayable, dsExtraOT);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentList(string paymentSource)
        {
            string sql = @"select ei.EmployeeCode,ei.EmployeeName ByWhom,gwp.Id,FORMAT(gwp.FromDate,'dd-MMM-yyy') FromDate,FORMAT(gwp.ToDate,'dd-MMM-yyy')ToDate
						,gwp.UserRef,FORMAT(gwp.PaymentDate,'dd-MMM-yyy') PaymentDate,gwp.Remarks
						from GoodWorkPaymentAdvise gwp 
						left join EmployeeInformation ei on ei.SystemId=gwp.ByWhomId
                        where gwp.PaymentSource='" + paymentSource + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdviseDetailList(string paymentAdviseId)
        {
            string sql = @"select gwpad.Id,gwpad.PaymentAdviseId,ei.EmployeeCode,ei.EmployeeName,gwpad.Hour,gwpad.Hour*60 Minute,gwpad.Rate,gwpad.Amount,gwpad.Remarks
                            from GoodWorkPaymentAdviseDetail gwpad
                            left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
							left join GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
                            where gwpa.Id='" + paymentAdviseId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdviseOTDetailList(string paymentAdviseId)
        {
            string sql = @"select gwpad.Id,ei.EmployeeCode,ei.EmployeeName,gwpad.Hour,gwpad.Hour*60 Minute,gwpad.Rate,gwpad.Amount,gwpad.Remarks
                            from GoodWorkPaymentAdviseDetail gwpad
                            left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
							left join GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
                            left join (SELECT C.IsOTEntitled,D.Id FROM SCS.DesignationMasterConfiguration C
                            LEFT JOIN MST.DesignationMaster M ON M.Id=C.DesignationMasterId
                            LEFT JOIN HKP.Designation D ON D.Id=M.DesignationId
							)D on D.Id=ei.GivenDesignationId
                            where gwpa.Id='" + paymentAdviseId + "' and D.IsOTEntitled=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetPayableCreationEmployeeData()
        {
            JsonResult json = Json(clsSales.GetPayableCreationEmployeeData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [Authorize, HttpGet]
        public JsonResult GetGoodWorkCheckByCbo()
        {
            var sql = @"select distinct E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text  
                          from dbo.GoodWorkCheckBySetUp A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.CheckById 
                          where E.EmployeeStatus='Active' ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGoodWorkApprovedByCbo()
        {
            var sql = @"select distinct E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text  
                          from dbo.GoodWorkAuthoritySetUp A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.AuthorityId 
                          where E.EmployeeStatus='Active'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #region Good work check
        [HttpGet, Authorize]
        public ActionResult GetUncheckedData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsSales.GetUncheckedGoodWorkData(identity.EmployeeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetGoodWorkApproveByCboList()
        {
            return Json(clsSales.GetGoodWorkApproveByCboList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetcheckedData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsSales.GetcheckedGoodWorkData(identity.EmployeeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetApproveBycheckedData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsSales.GetApproveBycheckedGoodWorkData(identity.EmployeeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetcheckedDataList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsSales.GetcheckedGoodWorkDataList(identity.EmployeeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateCheckBy(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from GoodWork where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    data["CheckedStatus"] = "Checked";
                    data["ApprovedStatus"] = "To Be Approved";
                    data["ApprovedBy"] = data["ApproveBy"];
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateApproveBy(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from GoodWork where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    data["ApprovedStatus"] = "Approved";
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public JsonResult GetUserGrData()
        {
            var sql = @"select Id As Value,UserName as Text from [dbo].[GoodWorkSetup]";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateGoodWorkChecked(Dictionary<string, object> data, List<Dictionary<string, object>> goodWorkDetail)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from GoodWork where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    data["CheckedStatus"] = "Checked";
                    data["ApprovedStatus"] = "To Be Approved";
                    data["ApprovedBy"] = data["ApprovedBy"];
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update

                #region Good Work Detail

                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from GoodWorkDetail where GoodWorkId='" + _MasterId + "'", out dsDetail, false, "1");
                int ccount = 0;
                if (goodWorkDetail != null)
                {
                    foreach (var item in goodWorkDetail)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            item["Id"] = detailid;
                            item["goodWorkId"] = _MasterId;
                            item["EmpSystemId"] = item["SystemId"];
                            item["FromTime"] = item["FromTime"];
                            item["ToTime"] = item["ToTime"];
                            item["Purpose"] = item["Purpose"];
                            item["PurposeCategory"] = item["PurposeCategory"];
                            item["Minute"] = item["Minute"];
                            item["Remark"] = item["Remark"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["Id"] = detailid;
                            drmo["FromTime"] = item["FromTime"];
                            drmo["ToTime"] = item["ToTime"];
                            drmo["Minute"] = item["Minute"];
                            drmo["Purpose"] = item["Purpose"];
                            drmo["PurposeCategory"] = item["PurposeCategory"];
                            drmo["Remark"] = item["Remark"];
                            drmo.EndEdit();

                        }
                    }
                }

                #endregion Good Work Detail
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);

                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult CreateGoodWorkApproved(Dictionary<string, object> data, List<Dictionary<string, object>> goodWorkDetail)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from GoodWork where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    data["ApprovedStatus"] = "Approved";
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                }

                #endregion data update

                #region Good Work Detail

                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from GoodWorkDetail where GoodWorkId='" + _MasterId + "'", out dsDetail, false, "1");
                int ccount = 0;
                if (goodWorkDetail != null)
                {
                    foreach (var item in goodWorkDetail)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            item["Id"] = detailid;
                            item["goodWorkId"] = _MasterId;
                            item["EmpSystemId"] = item["SystemId"];
                            item["FromTime"] = item["FromTime"];
                            item["ToTime"] = item["ToTime"];
                            item["Purpose"] = item["Purpose"];
                            item["PurposeCategory"] = item["PurposeCategory"];
                            item["Minute"] = item["Minute"];
                            item["Remark"] = item["Remark"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["Id"] = detailid;
                            drmo["FromTime"] = item["FromTime"];
                            drmo["ToTime"] = item["ToTime"];
                            drmo["Minute"] = item["Minute"];
                            drmo["Purpose"] = item["Purpose"];
                            drmo["PurposeCategory"] = item["PurposeCategory"];
                            drmo["Remark"] = item["Remark"];
                            drmo.EndEdit();

                        }
                    }
                }

                #endregion Good Work Detail
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);

                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkCheckedDataList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select GW.Id,format(GW.WorkDate,'dd-MMM-yyyy') WorkDate,S.UserName Shift,GW.Remarks,GWS.UserName UserGroup,GWS.Id UserGroupId,gw.Reason
                                    ,format(GW.FromTime,'hh:mm') FromTime,format(GW.ToTime,'hh:mm') ToTime,gw.Minute,gw.CheckedBy,gw.ApprovedBy
                                    from GoodWork GW
                                    left join ShiftDefination S on S.SystemId=GW.ShiftId
									left join [dbo].[GoodWorkSetup] GWS on GWS.Id=GW.UserGroupId
									left join EmployeeInformation ei on ei.SystemId=gw.CheckedBy
                                    where GW.CheckedBy='" + identity.EmployeeId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkApprovedDataList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select GW.Id,format(GW.WorkDate,'dd-MMM-yyyy') WorkDate,S.UserName Shift,GW.Remarks,GWS.UserName UserGroup,GWS.Id UserGroupId,gw.Reason
                                    ,format(GW.FromTime,'hh:mm') FromTime,format(GW.ToTime,'hh:mm') ToTime,gw.Minute,gw.CheckedBy,gw.ApprovedBy
                                    from GoodWork GW
                                    left join ShiftDefination S on S.SystemId=GW.ShiftId
									left join [dbo].[GoodWorkSetup] GWS on GWS.Id=GW.UserGroupId
									left join EmployeeInformation ei on ei.SystemId=gw.CheckedBy
                                    where GW.ApprovedBy='" + identity.EmployeeId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateGoodWorkPaymentDisburse(string Id)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsGWPDisburse = null;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(@"select * from GoodWorkPaymentAdviseDetail where EmpSystemId in (" + Id + ")", out dsGWPDisburse, false, "1");

                for (int i = 0; i < dsGWPDisburse.Tables[0].Rows.Count; i++)
                {
                    dsGWPDisburse.Tables[0].DefaultView.RowFilter = "Id='" + dsGWPDisburse.Tables[0].Rows[i]["Id"] + "'";

                    if (dsGWPDisburse.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow drGW = dsGWPDisburse.Tables[0].DefaultView[0].Row;
                        drGW.BeginEdit();

                        drGW["IsDisburse"] = 1;
                        drGW.EndEdit();
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsGWPDisburse);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdviseUnDisburseOTDetailList()
        {
            string sql = @"select gwpad.Id,ei.EmployeeCode,ei.EmployeeName,gwpad.Hour,gwpad.Hour*60 Minute,gwpad.Rate,gwpad.Amount,gwpad.Remarks
                            from GoodWorkPaymentAdviseDetail gwpad
                            left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
							left join GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
                            left join (SELECT C.IsOTEntitled,D.Id FROM SCS.DesignationMasterConfiguration C
                            LEFT JOIN MST.DesignationMaster M ON M.Id=C.DesignationMasterId
                            LEFT JOIN HKP.Designation D ON D.Id=M.DesignationId
							)D on D.Id=ei.GivenDesignationId
                            where D.IsOTEntitled=1 and isdisburse<>1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdviseDisburseOTDetailList(string fromDate,string toDate)
        {
            string sql = @"select gwpad.Id,ei.EmployeeCode,ei.EmployeeName,gwpad.Hour,gwpad.Hour*60 Minute,gwpad.Rate,gwpad.Amount,gwpad.Remarks
                            from GoodWorkPaymentAdviseDetail gwpad
                            left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
							left join GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
                            left join (SELECT C.IsOTEntitled,D.Id FROM SCS.DesignationMasterConfiguration C
                            LEFT JOIN MST.DesignationMaster M ON M.Id=C.DesignationMasterId
                            LEFT JOIN HKP.Designation D ON D.Id=M.DesignationId
							)D on D.Id=ei.GivenDesignationId
                            where gwpa.FromDate='"+ fromDate + "' and gwpa.ToDate='" + toDate + "' and D.IsOTEntitled=1 and isdisburse<>0";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetGoodWorkPaymentUndisburseReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                string fileName = "";
                fileName = _accountVoucherReportService.GoodWorkPaymentUndisburseReportxlx(data, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        [HttpPost, Authorize]
        public ActionResult PCOTEmployeeDisburseList(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                string fileName = "";
                fileName = _accountVoucherReportService.GoodWorkPaymentDisburseReportxlx(data, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Good work check
        public class WorkerAdvanceTransaction
        {
            #region Scalar Properties
            public int Percentage { get; set; }

            #endregion Scalar Properties

            #region Audit Properties

            /// <summary>
            ///This is  AddedBy.Who add data keep track by AddedBy.
            /// </summary>
            [NeverUpdate]
            public string AddedBy { get; set; }

            /// <summary>
            ///This is  AddedDate.Added date keep track by AddedDate.
            /// </summary>
            [NeverUpdate]
            public DateTime AddedDate { get; set; }

            /// <summary>
            /// Record insert by user from IP address.
            /// </summary>
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            /// <summary>
            /// Record updated user name.
            /// </summary>
            public string UpdatedBy { get; set; }

            /// <summary>
            /// Record updated by user date and time.
            /// </summary>
            public DateTime? UpdatedDate { get; set; }

            /// <summary>
            /// Record updated by user IP address.
            /// </summary>
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }
        public class WorkerAdvanceDetailTransaction
        {
            #region Scalar Properties
            public double Basic { get; set; }
            public int PayDays { get; set; }

            #endregion Scalar Properties

            #region Audit Properties

            /// <summary>
            ///This is  AddedBy.Who add data keep track by AddedBy.
            /// </summary>
            [NeverUpdate]
            public string AddedBy { get; set; }

            /// <summary>
            ///This is  AddedDate.Added date keep track by AddedDate.
            /// </summary>
            [NeverUpdate]
            public DateTime AddedDate { get; set; }

            /// <summary>
            /// Record insert by user from IP address.
            /// </summary>
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            /// <summary>
            /// Record updated user name.
            /// </summary>
            public string UpdatedBy { get; set; }

            /// <summary>
            /// Record updated by user date and time.
            /// </summary>
            public DateTime? UpdatedDate { get; set; }

            /// <summary>
            /// Record updated by user IP address.
            /// </summary>
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        #endregion Payable Creation and Worker Advance
    }
}
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
using Library.Model.Enums;
using Library.Model.Setups;
using Library.OrderManagement.Sales;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.SalaryDisbursement;
using Library.Service.Setups;
using Library.ViewModel.Vouchers;
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
        private readonly ISalaryDisbursementService _salaryDisbursementService;
        clsSales clsSales = new clsSales();
        public GoodWorkController(ISqlRepository R, AccountVoucherReportService accountVoucherReportService, ISalaryDisbursementService salaryDisbursementService)
        {
            _sqlRepository = R;
            _accountVoucherReportService = accountVoucherReportService;
            _salaryDisbursementService = salaryDisbursementService;
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
        public ActionResult EmployeeMultipleAdvance()
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
        public ActionResult GWReport()
        {
            return View();
        }
        //Load Employee
        [HttpPost, Authorize]
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
						 ,CONVERT(varchar(15),CAST(APD.Intime AS TIME),100) InTime
						 ,CONVERT(varchar(15),CAST(APD.OutTime AS TIME),100) OutTime
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
						 left join dbo.AttdnProcessData APD on APD.EmpSystemID=EI.SystemId and APD.WorkDate='" + workDate + @"'
                         WHERE  EI.PlantId='" + identity.PlantId + @"'  " + ec + @"  " + dep + @"  " + sec + @"   " + subsec + @"   " + des + @" " + userGr + @"
                         and ei.SystemId in (select EmpSystemID from dbo.AttdnProcessData where ShiftSystemId='" + shiftId + @"' and WorkDate='" + workDate + @"')  
                        AND ei.SystemId IN(Select EmployeeId From [dbo].[ExceptionGoodWorkEmployee] where GoodWorkSetUpId = '" + userGroupId + @"')
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

        [Authorize, HttpGet]
        public ActionResult GetShift(string setupId, string date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string xstr = @"select SD.SystemID ShiftId,P.Id PlantId,P.UserName Plant,SD.ShiftDefinationDescription,SD.UserName ShiftDefination 
						,CONVERT(varchar(5),SD.InTime,108) InTime,CONVERT(VARCHAR(5), SD.InTime, 108) OutTime						
						from ShiftDefination SD
						left join ORG.Plant P on P.Id=SD.PlantID
                        Where SD.SystemID IN(	Select distinct MB.ShiftDefinationId from dbo.GoodWorkBudgetSetUp BS
						left join MST.ManpowerBudget MB ON MB.Id=BS.BudgetId
						Where GoodWorkSetUpId='" + setupId + "')";

            string str = @"SELECT SD.SystemID ShiftId,P.Id PlantId,P.UserName Plant,SD.ShiftDefinationDescription,SD.UserName ShiftDefination 
,CONVERT(varchar(5),SD.InTime,108) InTime,CONVERT(VARCHAR(5), SD.InTime, 108) OutTime	from ShiftDefination SD
left join ORG.Plant P on P.Id=SD.PlantID
WHERE SD.SystemId IN(select distinct p.ShiftSystemId from  AttdnProcessData p Where p.WorkDate='" + date + "')";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

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
                            //item["Minute"] = item["Minute"];
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

        [HttpPost, Authorize]
        public ActionResult GetGoodWorkDetailCenter(string goodWorkId)
        {
            string str = @"select GWD.Id,EI.SystemId,EI.EmployeeCode,EI.EmployeeName,format(GWD.FromTime,'hh:mm') FromTime,format(GWD.ToTime,'hh:mm') ToTime
,GWD.Minute CalculatedTime
							,GWD.Purpose,GWD.PurposeCategory,ec.Id EmployeeCategoryId,EC.UserName EmployeeCategory,GWD.[Minute],GWD.Remark
							,PR.GoodWorkPositionCodeId UserGroupId,PR1.UserReportGroup UserGroup,ei.GivenDesignationId DesignationId,D.UserName Designation,S.Id SectionId,S.UserName Section,SS.Id SubSectionId,SS.UserName SubSection,DEPT.Id DepartmentId,DEPT.UserName Department
                            ,APD.OverStay,APD.DayStatus,GWD.GoodWorkId,CONVERT(varchar(15),CAST(APD.Intime AS TIME),100) InTime
						 ,CONVERT(varchar(15),CAST(APD.OutTime AS TIME),100) OutTime
                            ,LateDuration=CASE WHEN APD.DayStatus='L' THEN cast(DATEDIFF(MINUTE, cast(SD.InTime as time ), cast(APD.InTime as time ))as float) ElSE 0 END
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
							LEFT JOIN  dbo.ShiftDefination SD ON SD.SystemID=APD.ShiftSystemID
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
        public ActionResult getFiltersData(string userGroupId, string shiftId, string date)
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
						 where EI.EmployeeStatus='Active' and ei.SystemId in (select EmpSystemID from dbo.AttdnProcessData where WorkDate='" + date + @"') and EI.BudgetCode in (SELECT BudgetId FROM dbo.GoodWorkBudgetSetup where GoodWorkSetUpId= '" + userGroupId + @"')";
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

        [HttpPost, Authorize]
        public ActionResult GetGoodWorkReportInDateRange(string reportFileName, string fromDate, string toDate)
        {
            try
            {
                string fileName = "";
                fileName = GetGoodWorkReportInDateRangexlx("", reportFileName, fromDate, toDate);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetGoodWorkReportInDateRangexlx(string ReportHeader, string reportFileName, string fromDate, string toDate)
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
                DataTable data = GetGWReportDataInDateRange(fromDate, toDate);

                #region columns
                sheet[ROW, COL].Text = "Employee Code"; sheet[ROW, COL].ColumnWidth = 15; int ColEmployeeCode = COL; COL++;
                sheet[ROW, COL].Text = "Employee Name"; sheet[ROW, COL].ColumnWidth = 20; int ColEmployeeName = COL; COL++;
                sheet[ROW, COL].Text = "Good Work Group Name"; sheet[ROW, COL].ColumnWidth = 20; int ColGWGN = COL; COL++;
                sheet[ROW, COL].Text = "Responsible Person Name"; sheet[ROW, COL].ColumnWidth = 20; int ColRP = COL; COL++;
                sheet[ROW, COL].Text = "Entity"; sheet[ROW, COL].ColumnWidth = 15; int ColEntity = COL; COL++;
                sheet[ROW, COL].Text = "Good Work Entry Date"; sheet[ROW, COL].ColumnWidth = 10; int ColGWED = COL; COL++;
                sheet[ROW, COL].Text = "Checked Status"; sheet[ROW, COL].ColumnWidth = 10; int ColCheckedStatus = COL; COL++;
                sheet[ROW, COL].Text = "Approved Status"; sheet[ROW, COL].ColumnWidth = 10; int ColApprovedStatus = COL; COL++;
                sheet[ROW, COL].Text = "Payment Creation Date"; sheet[ROW, COL].ColumnWidth = 10; int ColPCD = COL; COL++;
                sheet[ROW, COL].Text = "Payment ApprovedBy"; sheet[ROW, COL].ColumnWidth = 20; int ColPAB = COL; COL++;
                sheet[ROW, COL].Text = "Employee Category"; sheet[ROW, COL].ColumnWidth = 10; int ColEC = COL; COL++;
                sheet[ROW, COL].Text = "Legal Designation"; sheet[ROW, COL].ColumnWidth = 20; int ColLD = COL; COL++;
                sheet[ROW, COL].Text = "Section"; sheet[ROW, COL].ColumnWidth = 12; int ColSection = COL; COL++;
                sheet[ROW, COL].Text = "Department"; sheet[ROW, COL].ColumnWidth = 12; int ColDepartment = COL; COL++;
                sheet[ROW, COL].Text = "Work Date"; sheet[ROW, COL].ColumnWidth = 8; int ColWorkDate = COL; COL++;
                sheet[ROW, COL].Text = "Over Time"; sheet[ROW, COL].ColumnWidth = 10; int ColOverTime = COL; COL++;
                sheet[ROW, COL].Text = "Over Stay"; sheet[ROW, COL].ColumnWidth = 10; int ColOverStay = COL; COL++;
                sheet[ROW, COL].Text = "Day Status"; sheet[ROW, COL].ColumnWidth = 10; int ColDayStatus = COL;
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
                    sheet[ROW, ColGWGN].Text = data.Rows[i]["GoodWorkGroupName"].ToString();
                    sheet[ROW, ColRP].Text = data.Rows[i]["ResponsiblePersonName"].ToString();
                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, ColGWED].Text = data.Rows[i]["GoodWorkEntryDate"].ToString();
                    sheet[ROW, ColCheckedStatus].Text = data.Rows[i]["CheckedStatus"].ToString();
                    sheet[ROW, ColApprovedStatus].Text = data.Rows[i]["ApprovedStatus"].ToString();
                    sheet[ROW, ColPCD].Text = data.Rows[i]["PaymentCreationDate"].ToString();
                    sheet[ROW, ColPAB].Text = data.Rows[i]["PaymentApprovedBy"].ToString();
                    sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColLD].Text = data.Rows[i]["LegalDesignation"].ToString();
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

        public DataTable GetGWReportDataInDateRange(string fromDate, string toDate)
        {
            try
            {
                var sql = @"select EI.EmployeeCode,EI.EmployeeName,GWS.UserName GoodWorkGroupName,R.EmployeeName ResponsiblePersonName,EN.UserName Entity
,FORMAT(GW.AddedDate,'dd-MMM-yyyy')GoodWorkEntryDate,GW.CheckedStatus,GW.ApprovedStatus,FORMAT(GPA.AddedDate,'dd-MMM-yyyy')PaymentCreationDate
,GPAP.EmployeeName PaymentApprovedBy,EC.UserName EmployeeCategory,D.UserName LegalDesignation,S.UserName Section,DEPT.UserName Department
,format(GW.WorkDate,'dd-MMM-yyyy') WorkDate,GWD.Minute OverTime,APD.OverStay,APD.DayStatus
from GoodWork GW 
left join GoodworkDetail GWD on GW.Id=GWD.GoodWorkId
left join EmployeeInformation EI on EI.SystemId=GWD.EmpSystemId 
LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
LEFT JOIN ORG.Department DEPT ON EI.DepartmentId=DEPT.Id
LEFT join MST.DesignationMaster DM on DM.DesignationId=EI.GivenDesignationId
LEFT join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
left join hkp.LegalDesignation D on D.Id=ei.LegalDesignationId
left join dbo.AttdnProcessData APD on APD.EmpSystemID=EI.SystemId and APD.WorkDate=GW.WorkDate
LEFT JOIN dbo.GoodWorkSetup GWS ON GWS.Id=GW.UserGroupId
LEFT JOIN dbo.EmployeeInformation R ON GWS.ResponsiblePersonId=R.SystemId
LEFT JOIN [dbo].[GoodWorkPaymentAdvise] GPA ON GPA.Id=GWD.GWPaymentAdviseId
LEFT JOIN dbo.EmployeeInformation GPAP ON GPAP.SystemId=GPA.ApprovedById
LEFT JOIN ORG.Entity EN ON EN.Id=PMB.EntityId
where GW.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' and APD.DayStatus <> 'A'
Order By GW.WorkDate";
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
            string sql = @"select wa.Id,FORMAT(FromDate,'dd-MMM-yy')FromDate,FORMAT(ToDate,'dd-MMM-yy')ToDate,UserRef,UserName,wa.YearNo,wa.MonthNo
						        ,wa.PayDaysType,wa.Percentage,wa.Remarks,wa.ApprovedById,wa.Multiple,wa.MinimumPresentDay,wa.IsPayDay
                                ,wa.IsStandardOT,wa.IsAdditionalOT,wa.ApprovedStatus,wa.PaymentsStatus
                                ,ei.SystemId PreparedById,ei.EmployeeName ByWhom
                                  from [TRN].[EmployeeAdvance] wa
                                  LEFT JOIN dbo.EmployeeInformation AS ei ON ei.SystemId=wa.PreparedById
                                  WHERE wa.ApprovedStatus='TobeApproved' AND wa.SourceType='MultipleEmployeeAdvance' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetWorkerAdvancePendingforApprovalList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select wa.Id,FORMAT(FromDate,'dd-MMM-yy')FromDate,FORMAT(ToDate,'dd-MMM-yy')ToDate,UserRef,UserName,wa.YearNo,wa.MonthNo
						        ,wa.PayDaysType,wa.Percentage,wa.Remarks,wa.ApprovedById,wa.Multiple,wa.MinimumPresentDay,wa.IsPayDay
                                ,wa.IsStandardOT,wa.IsAdditionalOT,wa.ApprovedStatus,wa.PaymentsStatus
                                ,ei.SystemId PreparedById,ei.EmployeeName ByWhom
                                from [TRN].[EmployeeAdvance] wa
                                LEFT JOIN dbo.EmployeeInformation AS ei ON ei.SystemId=wa.PreparedById
                                WHERE wa.ApprovedStatus='TobeApproved' AND wa.SourceType='MultipleEmployeeAdvance' AND wa.ApprovedById='" + identity.EmployeeId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetWorkerAdvanceDetailCenter(string workAdvanceId)
        {
            string str = @"select ei.SystemId EmpSystemId,ei.EmployeeCode,ei.EmployeeName,s.UserName Section,ss.UserName SubSection,wad.Id
							,wa.Id workAdvanceId,d.UserName Department,wad.PayDays,wad.StandardOTHour,wad.AdditionalOTHour,wad.Rate,wad.Amount,wad.AdvanceAmount,wad.Remarks 
                            from [TRN].[EmployeeAdvanceDetail] wad
                            left join [TRN].[EmployeeAdvance] wa on wa.Id=wad.EmployeeAdvanceId
                            left join EmployeeInformation ei on ei.SystemId=wad.EmpSystemId
                            left join org.Section AS s ON s.Id=ei.SectionId
                            left join org.SubSection AS ss ON ss.Id=ei.SubSectionId
                            left join org.Department d on d.Id=ei.DepartmentId
                            where wad.EmployeeAdvanceId in ('" + workAdvanceId + "') AND wa.SourceType='MultipleEmployeeAdvance' AND wad.IsApprove IS NULL";
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
        public ActionResult ApproveWorkerAdvance(string workerAdvanceId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("UPDATE [TRN].[EmployeeAdvance] SET ApprovedById='" + identity.EmployeeId + "' ,ApprovedStatus='Approved'  where Id='" + workerAdvanceId + "' ");
                con.CommitTransaction();

                return Json(new { Message = "Approved Successfully." });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult ApproveEmployeeMultipleAdvance(Dictionary<string, object> data, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail)
        {
            try
            {
                string goodWorkPaymentAdviseDetailIds = "";
                if (goodWorkPaymentAdviseDetail != null)
                {
                    foreach (var item in goodWorkPaymentAdviseDetail)
                    {
                        if (goodWorkPaymentAdviseDetailIds == "")
                        {
                            goodWorkPaymentAdviseDetailIds = "'" + item["Id"] + "'"; ;
                        }
                        else
                        {
                            goodWorkPaymentAdviseDetailIds += ",'" + item["Id"] + "'";

                        }
                    }
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("UPDATE [TRN].[EmployeeAdvance] SET ApprovedStatus='Approved'  where Id='" + data["Id"] + "' ");
                con.executeQuery("UPDATE [TRN].[EmployeeAdvanceDetail] SET IsApprove=1  where Id in (" + goodWorkPaymentAdviseDetailIds + ") ");
                con.CommitTransaction();


                return Json(new { Error = false, Data = data, Message = "Approved Successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CreateWorkerAdvance(Dictionary<string, object> data, List<Dictionary<string, object>> workerAdvanceDetail)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvance] where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                string _Id = "";

                #region data update Worker Advance
                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    if (_Id == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("EmployeeAdvance", out _Id);
                    }
                    data["Id"] = _Id;
                    data["CompanyGroupId"] = identity.CompanyGroupId;
                    data["CompanyId"] = identity.CompanyId;
                    data["PlantId"] = identity.PlantId;
                    data["SourceType"] = SourceType.MultipleEmployeeAdvance.ToString();
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
                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceDetail] where  EmployeeAdvanceId='" + _MasterId + "'", out dsDetail, false, "1");
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
                            item["EmployeeAdvanceId"] = _MasterId;
                            item["EmpSystemId"] = item["EmpSystemId"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        if (dv.Count > 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();

                            drmo["EmployeeAdvanceId"] = _MasterId;
                            drmo["EmpSystemId"] = item["EmpSystemId"];
                            drmo["PayDays"] = item["PayDays"];
                            drmo["AdvanceAmount"] = item["AdvanceAmount"];
                            drmo["Remarks"] = item["Remarks"];

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


        [HttpPost, Authorize]
        public ActionResult LoadPCAACEmployeelist(string fromDate, string toDate, string payDaysType)
        {
            string sql = string.Empty;
            try
            {
                sql = @"select '' Id,gwpa.Id GoodWorkPaymentAdviseId,gwpad.Id GoodWorkPaymentAdviseDetailId,gwpa.PaymentSource,(gwpad.Hour*60) Minute,gwpad.Hour,gwpad.Rate,gwpad.Amount
						,0 CheckBoxSelect,EI.SystemId,EI.EmployeeCode,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(EI.DOS,'dd-MMM-yyyy') DOS,DG.UserName LegalDesignation
						,DP.UserName Department,S.UserName Section,SS.UserName SubSection,EI.EmployeeStatus,OTTitle = case when EI.ExcludeOT=0 then 'Yes' else 'No' END
						 
						from GoodWorkPaymentAdvise gwpa
						left join GoodWorkPaymentAdviseDetail gwpad on gwpa.Id=gwpad.PaymentAdviseId
						left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
						LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
						LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
						LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                        LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                        where gwpa.FromDate between '" + fromDate + @"' and '" + toDate + @"' and gwpa.ToDate between '" + fromDate + @"' and '" + toDate + @"'
                        and gwpa.PaymentSource='" + payDaysType + @"'
                        and gwpa.Id not in(select GoodWorkPaymentAdviseId from [TRN].[EmployeeAdvanceDetail]) ";
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
                con.executeQuery("delete from [TRN].[EmployeeAdvanceDetail] where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetGoodWorkEmployeelist(string fromDate, string toDate, string tabName, string saveUpdate)
        {
            string sql = string.Empty;
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("BasicSalaryHeadID");
            dtValue.Columns.Add("Basic");
            dtValue.Columns.Add("Rate");
            string sFormulaResult = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (saveUpdate == "Save")
                {
                    if (tabName == "GoodWork")
                    {
                        sql = @"SELECT * FROM 
(select CheckBoxSelect=cast(case when z.Id is null then 0 else 1 end as bit)
,z.Id,ei.SystemId EmpSystemId,ei.EmployeeCode,ei.EmployeeName,sum(gwd.Minute)*OLS.OTreductionFactor Minute,(sum(gwd.Minute)/60)*OLS.OTreductionFactor Hour
                                    ,format(g.Gross,'N2') Gross,0.00 Rate,0.00 Amount
									,onw.FormulaDesID,B.Basic,B.BasicSalaryHeadID,G.GrossSalaryHeadID,Department.UserName Department,Section.UserName Section,SubSection.UserName SubSection,EI.EmployeeStatus,EI.PaymentMode
                                     from [dbo].[GoodWork] gw
                                     left join  GoodWorkDetail GWD on GWD.GoodWorkId=gw.Id 
                                     left join EmployeeInformation ei on ei.SystemId=GWD.EmpSystemId
                                    left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						            left join ORG.Position PO on PO.Id=MPB.PositionId
                                    LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						            LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						            LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						            LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    left join (Select top 1* from [dbo].[OTLimitSetting])OLS ON OLS.PlantID=ei.PlantId
                                     LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
                                     LEFT JOIN(SELECT SID.SalaryID,SID.DefineAmount Gross,SH.SalaryHeadID GrossSalaryHeadID
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
left  join (SELECT SID.DefineAmount Basic,SH.SalaryHeadID BasicSalaryHeadID,SID.SalaryID
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Basic') B ON B.SalaryID=SIDM.SystemID

                                    left join (select gwpad.EmpSystemId,gwpad.Id
									                                from GoodWorkPaymentAdvise gwpa
									                                left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
									                                where convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
									                                )z on z.EmpSystemId=GWD.EmpSystemId

left join mst.DesignationMaster dml on dml.DesignationId=ei.GivenDesignationId
												inner join (select DesignationMasterId,OverTimePmtPolicyMasterID,IsOTEntitled ,PlantId
                                                            from scs.DesignationMasterConfiguration where PlantId in ('" + identity.PlantId + @"') and IsOTEntitled=1) dc 
                                                            on dc.DesignationMasterId=dml.Id and ei.PlantId = dc.PlantId
												left join OverTimePmtPolicyMaster otpm on otpm.ID=dc.OverTimePmtPolicyMasterID and otpm.PlantID in ('" + identity.PlantId + @"')
												left join OverTimePmtPolicyDetails oNW on oNW.OverTimePmtPolicyID=otpm.ID and onw.OverTimeDayType='Working Day'

                                     where gw.WorkDate between '" + fromDate + @"' and '" + toDate + @"' and gwd.Minute<>0 and g.Gross<>0  and SIDM.IsApproved=1 AND gw.ApprovedStatus='Approved' AND GWD.GWPaymentAdviseId IS NULL
                                     group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross
									 ,z.Id,onw.FormulaDesID,B.Basic,B.BasicSalaryHeadID,G.GrossSalaryHeadID,OLS.OTreductionFactor,Department.UserName,Section.UserName,SubSection.UserName ,EI.EmployeeStatus,EI.PaymentMode
                                    )T WHERE T.CheckBoxSelect=0
								order by T.EmployeeCode ";

                    }
                    else
                    {
                        sql = @"SELECT * FROM 
(select CheckBoxSelect=cast(case when z.Id is null then 0 else 1 end as bit)
,CheckBoxSelectAD=cast(case when za.Id is null then 0 else 1 end as bit)
,ei.SystemId EmpSystemId,z.Id,ei.EmployeeCode,ei.EmployeeName
,format(sum(ISNULL(apd.AdditionalOT,0)),'N2') Minute
                                ,format((sum(ISNULL(apd.AdditionalOT,0))/60),'N2') Hour
                                ,format(g.Gross,'N2') Gross
                                ,0.00 Rate,0.00 Amount
								,apd.GWPaymentAdviseId 
								,onw.FormulaDesID
                                ,Department.UserName Department,Section.UserName Section,SubSection.UserName SubSection
								,B.Basic,B.BasicSalaryHeadID
                                ,format(sum(apd.OverStay),'N2') OverStayMinute
                                ,format((sum(apd.OverStay)/60),'N2') OverStayHour
                                ,COUNT(apd.RowId)OTProcessDays
                                ,EI.EmployeeStatus,EI.PaymentMode
                                from [dbo].[AttdnProcessData] apd 
                                left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID 
                                left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						        left join ORG.Position PO on PO.Id=MPB.PositionId
                                LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						        LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						        LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						        LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
								                                  LEFT JOIN(SELECT ((SID.DefineAmount/208)*2) RatePerHour,SH.SalaryHead
								                                  ,SID.SalaryID,SID.DefineAmount Gross
                                                                      FROM SalaryInfoDefine SID 
								                                  LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                    WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
								left join (select gwpad.EmpSystemId,gwpad.Id
									                                from GoodWorkPaymentAdvise gwpa
									                                left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
									                                where convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
									                                )z on z.EmpSystemId=apd.EmpSystemID
                                left join (select gwpad.EmpSystemId,gwpad.Id,gwpad.AdvanceAmount
									                                from [TRN].[EmployeeAdvance] gwpa
									                                left join [TRN].[EmployeeAdvanceDetail] gwpad on gwpad.EmployeeAdvanceId=gwpa.Id
									                                where convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
									                                )za on za.EmpSystemId=apd.EmpSystemID

                                left join mst.DesignationMaster dml on dml.DesignationId=ei.GivenDesignationId
								inner join (select DesignationMasterId,OverTimePmtPolicyMasterID,IsOTEntitled ,PlantId
                                                            from scs.DesignationMasterConfiguration where PlantId in ('" + identity.PlantId + @"') and IsOTEntitled=1) dc 
                                                            on dc.DesignationMasterId=dml.Id and ei.PlantId = dc.PlantId
												left join OverTimePmtPolicyMaster otpm on otpm.ID=dc.OverTimePmtPolicyMasterID and otpm.PlantID in ('" + identity.PlantId + @"')
												left join OverTimePmtPolicyDetails oNW on oNW.OverTimePmtPolicyID=otpm.ID and onw.OverTimeDayType='Working Day'
								left  join (SELECT SID.DefineAmount Basic,SH.SalaryHeadID BasicSalaryHeadID,SID.SalaryID
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Basic') B ON B.SalaryID=SIDM.SystemID
                                where apd.WorkDate between '" + fromDate + @"' and '" + toDate + @"' AND DayStatus NOT  IN ('A') AND apd.IsOTEntitled=1 and SIDM.IsApproved=1 AND apd.GWPaymentAdviseId IS NULL and ISNULL(apd.AdditionalOT,0)>0
                                group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross,g.RatePerHour,apd.GWPaymentAdviseId,z.Id,Department.UserName,Section.UserName,SubSection.UserName
								,onw.FormulaDesID,B.Basic,B.BasicSalaryHeadID,za.Id,EI.EmployeeStatus,EI.PaymentMode
                                )T WHERE T.CheckBoxSelect=0 and T.CheckBoxSelectAD=0
								order by T.EmployeeCode ";
                    }
                }
                else
                {
                    if (tabName == "GoodWork")
                    {
                        sql = @"SELECT * FROM 
(select CheckBoxSelect=cast(case when z.Id is null then 0 else 1 end as bit)
,z.Id,ei.SystemId EmpSystemId,ei.EmployeeCode,ei.EmployeeName,sum(gwd.Minute)*OLS.OTreductionFactor Minute,(sum(gwd.Minute)/60)*OLS.OTreductionFactor Hour
                                    ,format(g.Gross,'N2') Gross,0.00 Rate,0.00 Amount
									,onw.FormulaDesID,B.Basic,B.BasicSalaryHeadID,G.GrossSalaryHeadID,Department.UserName Department,Section.UserName Section,SubSection.UserName SubSection,EI.EmployeeStatus,EI.PaymentMode
                                     from [dbo].[GoodWork] gw
                                     left join  GoodWorkDetail GWD on GWD.GoodWorkId=gw.Id 
                                     left join EmployeeInformation ei on ei.SystemId=GWD.EmpSystemId
                                    left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						            left join ORG.Position PO on PO.Id=MPB.PositionId
                                    LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						            LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						            LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						            LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    left join (Select top 1* from [dbo].[OTLimitSetting])OLS ON OLS.PlantID=ei.PlantId
                                     LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
                                     LEFT JOIN(SELECT SID.SalaryID,SID.DefineAmount Gross,SH.SalaryHeadID GrossSalaryHeadID
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
left  join (SELECT SID.DefineAmount Basic,SH.SalaryHeadID BasicSalaryHeadID,SID.SalaryID
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Basic') B ON B.SalaryID=SIDM.SystemID

                                    left join (select gwpad.EmpSystemId,gwpad.Id
									                                from GoodWorkPaymentAdvise gwpa
									                                left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
									                                where gwpa.PaymentSource='GoodWork' AND gwpad.IsCheck IS NULL AND convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
									                                )z on z.EmpSystemId=GWD.EmpSystemId

                                    left join mst.DesignationMaster dml on dml.DesignationId=ei.GivenDesignationId
												inner join (select DesignationMasterId,OverTimePmtPolicyMasterID,IsOTEntitled ,PlantId
                                                            from scs.DesignationMasterConfiguration where PlantId in ('" + identity.PlantId + @"') and IsOTEntitled=1) dc 
                                                            on dc.DesignationMasterId=dml.Id and ei.PlantId = dc.PlantId
												left join OverTimePmtPolicyMaster otpm on otpm.ID=dc.OverTimePmtPolicyMasterID and otpm.PlantID in ('" + identity.PlantId + @"')
												left join OverTimePmtPolicyDetails oNW on oNW.OverTimePmtPolicyID=otpm.ID and onw.OverTimeDayType='Working Day'

                                     where gw.WorkDate between '" + fromDate + @"' and '" + toDate + @"' and gwd.Minute<>0 and g.Gross<>0  and SIDM.IsApproved=1 AND gw.ApprovedStatus='Approved' 
                                     group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross
									 ,z.Id,onw.FormulaDesID,B.Basic,B.BasicSalaryHeadID,G.GrossSalaryHeadID,OLS.OTreductionFactor,Department.UserName,Section.UserName,SubSection.UserName ,EI.EmployeeStatus,EI.PaymentMode
                                    )T --WHERE T.CheckBoxSelect=0
								order by T.EmployeeCode ";

                    }
                    else
                    {
                        sql = @"SELECT * FROM 
(select CheckBoxSelect=cast(case when z.Id is null then 0 else 1 end as bit)
,CheckBoxSelectAD=cast(case when za.Id is null then 0 else 1 end as bit)
,ei.SystemId EmpSystemId,z.Id,ei.EmployeeCode,ei.EmployeeName
,format(sum(ISNULL(apd.AdditionalOT,0)),'N2') Minute
                                ,format((sum(ISNULL(apd.AdditionalOT,0))/60),'N2') Hour
                                ,format(g.Gross,'N2') Gross
                                ,0.00 Rate,0.00 Amount
								,apd.GWPaymentAdviseId 
								,onw.FormulaDesID
                                ,Department.UserName Department,Section.UserName Section,SubSection.UserName SubSection
								,B.Basic,B.BasicSalaryHeadID
                                ,format(sum(apd.OverStay),'N2') OverStayMinute
                                ,format((sum(apd.OverStay)/60),'N2') OverStayHour
                                ,COUNT(apd.RowId)OTProcessDays
                                ,EI.EmployeeStatus,EI.PaymentMode
                                from [dbo].[AttdnProcessData] apd 
                                left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID 
                                left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						        left join ORG.Position PO on PO.Id=MPB.PositionId
                                LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						        LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						        LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						        LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
								                                  LEFT JOIN(SELECT ((SID.DefineAmount/208)*2) RatePerHour,SH.SalaryHead
								                                  ,SID.SalaryID,SID.DefineAmount Gross
                                                                      FROM SalaryInfoDefine SID 
								                                  LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                    WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
								left join (select gwpad.EmpSystemId,gwpad.Id
									                                from GoodWorkPaymentAdvise gwpa
									                                left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
									                                where gwpa.PaymentSource='Attendance' AND convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
									                                )z on z.EmpSystemId=apd.EmpSystemID
                                left join (select gwpad.EmpSystemId,gwpad.Id,gwpad.AdvanceAmount
									                                from [TRN].[EmployeeAdvance] gwpa
									                                left join [TRN].[EmployeeAdvanceDetail] gwpad on gwpad.EmployeeAdvanceId=gwpa.Id
									                                where convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
									                                )za on za.EmpSystemId=apd.EmpSystemID
                                left join mst.DesignationMaster dml on dml.DesignationId=ei.GivenDesignationId
								inner join (select DesignationMasterId,OverTimePmtPolicyMasterID,IsOTEntitled ,PlantId
                                                            from scs.DesignationMasterConfiguration where PlantId in ('" + identity.PlantId + @"') and IsOTEntitled=1) dc 
                                                            on dc.DesignationMasterId=dml.Id and ei.PlantId = dc.PlantId
												left join OverTimePmtPolicyMaster otpm on otpm.ID=dc.OverTimePmtPolicyMasterID and otpm.PlantID in ('" + identity.PlantId + @"')
												left join OverTimePmtPolicyDetails oNW on oNW.OverTimePmtPolicyID=otpm.ID and onw.OverTimeDayType='Working Day'
								left  join (SELECT SID.DefineAmount Basic,SH.SalaryHeadID BasicSalaryHeadID,SID.SalaryID
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Basic') B ON B.SalaryID=SIDM.SystemID
                                where apd.WorkDate between '" + fromDate + @"' and '" + toDate + @"' AND DayStatus NOT  IN ('A')  AND apd.IsOTEntitled=1 and SIDM.IsApproved=1 and ISNULL(apd.AdditionalOT,0)>0
                                group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross,g.RatePerHour,apd.GWPaymentAdviseId,z.Id,Department.UserName,Section.UserName,SubSection.UserName
								,onw.FormulaDesID,B.Basic,B.BasicSalaryHeadID,za.Id,EI.EmployeeStatus,EI.PaymentMode
                                )T WHERE T.CheckBoxSelectAD=0
								order by T.EmployeeCode ";
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            DataTable dtData = _sqlRepository.GetDataTable(sql);
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                //if (i == 0)
                //{
                DataRow dtValueRow = dtValue.NewRow();

                dtValueRow["BasicSalaryHeadID"] = dtData.Rows[i]["BasicSalaryHeadID"].ToString().Trim();
                dtValueRow["Basic"] = dtData.Rows[i]["Basic"].ToString().Trim();

                dtValue.Rows.Add(dtValueRow);
                
                if (!string.IsNullOrEmpty(dtData.Rows[i]["FormulaDesID"].ToString()))
                {
                    ReLoadFormulaWithValue(dtData.Rows[i]["FormulaDesID"].ToString(), ref dtValue, out string _formulaValue);
                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("##,##0.00");

                    //DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["BasicSalaryHeadID"] = dtData.Rows[i]["BasicSalaryHeadID"].ToString().Trim();
                    dtValueRow["Rate"] = Convert.ToDecimal(sFormulaResult);

                    //dtValue.Rows.Add(dtValueRow);

                    DataView dv = new DataView(dtData);
                    dv.RowFilter = "EmpSystemId='" + dtData.Rows[i]["EmpSystemId"].ToString() + "'";
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;
                        
                            drmo.BeginEdit();
                            drmo["Rate"] = Convert.ToDecimal(sFormulaResult);
                            //drmo["Amount"] = Convert.ToDecimal(Convert.ToDecimal(sFormulaResult) * Convert.ToDecimal(dtData.Rows[i]["Hour"].ToString())).ToString("##,##0.00");
                            drmo["Amount"] = Math.Round(Convert.ToDecimal(sFormulaResult) * Convert.ToDecimal(dtData.Rows[i]["Hour"].ToString()),0);
                            drmo.EndEdit();

                    }
                    dtValue = new DataTable();
                    dtValue.TableName = "TempTable";
                    dtValue.Columns.Add("BasicSalaryHeadID");
                    dtValue.Columns.Add("Basic");
                    dtValue.Columns.Add("Rate");
                }
            }


            List<Dictionary<string, object>> data = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtData);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeMultipleAdvanceEmployeelist(string fromDate, string toDate, string saveUpdate,decimal percentage, decimal multiple, decimal minimumPresentDay, bool isPayDay, bool isStandardOT, bool isAdditionalOT,string employeeAdvanceId)
        {
            string sql = string.Empty;
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("BasicSalaryHeadID");
            dtValue.Columns.Add("Basic");
            dtValue.Columns.Add("Rate");
            string sFormulaResult = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (saveUpdate == "Save")
                {
                    sql = @"SELECT T.*,ISNULL(WRTobeApproved.TobeApprovedAmount,0)TobeApprovedAmount
								,ISNULL(WRApproved.ApprovedAmount,0)ApprovedAmount
								,ISNULL(WRPaid.PaidAmount,0)PaidAmount FROM 
(select CheckBoxSelect=cast(0 as bit)
,'' Id,ei.SystemId EmpSystemId,ei.EmployeeCode,ei.EmployeeName
,format(sum(ISNULL(apd.AdditionalOT,0)),'N2') Minute
                                ,format(sum(ISNULL(apd.PayDayValue,0)),'N2') PayDays
                                ,format((sum(ISNULL(apd.StandardOT,0))/60),'N2') StandardOTHour
                                ,format((sum(ISNULL(apd.AdditionalOT,0))/60),'N2') AdditionalOTHour
                                ,format(sum(ISNULL(apd.PresentValue,0)),'N2') PresentDays
                                ,format(g.Gross,'N2') Gross
                                ,0.00 Rate,0.00 Amount,0.00 AdvanceAmount
								,apd.GWPaymentAdviseId 
								,onw.FormulaDesID
                                ,EN.UserName Entity,Department.UserName Department,Section.UserName Section,SubSection.UserName SubSection
								,B.Basic,B.BasicSalaryHeadID
                                ,format(sum(apd.OverStay),'N2') OverStayMinute
                                ,format((sum(apd.OverStay)/60),'N2') OverStayHour
                                ,DesM.UserName GivenDesignation,DG.UserName LegalDesignation,EI.PaymentMode PaymentSource
                                from [dbo].[AttdnProcessData] apd 
                                left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID 
                                left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						        left join ORG.Position PO on PO.Id=MPB.PositionId
                                LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						        LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						        LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                                LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						        LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
								                                  LEFT JOIN(SELECT ((SID.DefineAmount/208)*2) RatePerHour,SH.SalaryHead
								                                  ,SID.SalaryID,SID.DefineAmount Gross
                                                                      FROM SalaryInfoDefine SID 
								                                  LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                    WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
								
                                left join mst.DesignationMaster dml on dml.DesignationId=ei.GivenDesignationId
								inner join (select DesignationMasterId,OverTimePmtPolicyMasterID,IsOTEntitled ,PlantId
                                                            from scs.DesignationMasterConfiguration where PlantId in ('" + identity.PlantId + @"') and IsOTEntitled=1) dc 
                                                            on dc.DesignationMasterId=dml.Id and ei.PlantId = dc.PlantId
												left join OverTimePmtPolicyMaster otpm on otpm.ID=dc.OverTimePmtPolicyMasterID and otpm.PlantID in ('" + identity.PlantId + @"')
												left join OverTimePmtPolicyDetails oNW on oNW.OverTimePmtPolicyID=otpm.ID and onw.OverTimeDayType='Working Day'
								left  join (SELECT SID.DefineAmount Basic,SH.SalaryHeadID BasicSalaryHeadID,SID.SalaryID
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Basic') B ON B.SalaryID=SIDM.SystemID
                                where apd.WorkDate between '" + fromDate + @"' and '" + toDate + @"' AND DayStatus IN('P','W','L') AND apd.GWPaymentAdviseId is null AND apd.IsOTEntitled=1 and SIDM.IsApproved=1 and EI.EmployeeStatus='Active' and (ISNULL(apd.StandardOT,0)>0 or ISNULL(apd.AdditionalOT,0)>0)
                                group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross,g.RatePerHour,apd.GWPaymentAdviseId,Department.UserName,Section.UserName,SubSection.UserName
								,onw.FormulaDesID,B.Basic,B.BasicSalaryHeadID,EN.UserName,DesM.UserName,DG.UserName,EI.PaymentMode
                                )T 
                                left join (select wad.EmpSystemId,SUM(wad.AdvanceAmount)TobeApprovedAmount
									                                from [TRN].[EmployeeAdvance] wa
									                                left join [TRN].[EmployeeAdvanceDetail] wad on wad.EmployeeAdvanceId=wa.Id
									                                where convert( DateTime, wa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, wa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
																	AND wa.ApprovedStatus='TobeApproved' AND wa.SourceType='MultipleEmployeeAdvance' GROUP BY wad.EmpSystemId
									                                )WRTobeApproved on WRTobeApproved.EmpSystemId=T.EmpSystemID
								left join (select wad.EmpSystemId,SUM(wad.AdvanceAmount)ApprovedAmount
									                                from [TRN].[EmployeeAdvance] wa
									                                left join [TRN].[EmployeeAdvanceDetail] wad on wad.EmployeeAdvanceId=wa.Id
									                                where convert( DateTime, wa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, wa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
																	AND wa.ApprovedStatus IN ('Approved','Paid') AND wa.SourceType='MultipleEmployeeAdvance' AND wad.VoucherId IS NULL GROUP BY wad.EmpSystemId
									                                )WRApproved on WRApproved.EmpSystemId=T.EmpSystemID
								left join (select wad.EmpSystemId,SUM(wad.AdvanceAmount)PaidAmount
									                                from [TRN].[EmployeeAdvance] wa
									                                left join [TRN].[EmployeeAdvanceDetail] wad on wad.EmployeeAdvanceId=wa.Id
									                                where convert( DateTime, wa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, wa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
																	AND wa.ApprovedStatus='Paid' AND wa.SourceType='MultipleEmployeeAdvance' AND wad.VoucherId IS NOT NULL GROUP BY wad.EmpSystemId
									                                )WRPaid on WRPaid.EmpSystemId=T.EmpSystemID
                                WHERE T.PresentDays >= '" + minimumPresentDay + @"' 
								order by T.EmployeeCode ";

                    }
                    else
                    {
                        sql = @"SELECT CheckBoxSelect=cast(case when WRUpdate.Id is null then 0 else 1 end as bit)
,WRUpdate.Id,T.*,ISNULL(WRTobeApproved.TobeApprovedAmount,0)TobeApprovedAmount
								,ISNULL(WRApproved.ApprovedAmount,0)ApprovedAmount
								,ISNULL(WRPaid.PaidAmount,0)PaidAmount 
								,ISNULL(WRUpdate.AdvanceAmount,0)AdvanceAmount FROM  
(select ei.SystemId EmpSystemId,ei.EmployeeCode,ei.EmployeeName
,format(sum(ISNULL(apd.AdditionalOT,0)),'N2') Minute
                                ,format(sum(ISNULL(apd.PayDayValue,0)),'N2') PayDays
                                ,format((sum(ISNULL(apd.StandardOT,0))/60),'N2') StandardOTHour
                                ,format((sum(ISNULL(apd.AdditionalOT,0))/60),'N2') AdditionalOTHour
                                ,format(sum(ISNULL(apd.PresentValue,0)),'N2') PresentDays
                                ,format(g.Gross,'N2') Gross
                                ,0.00 Rate,0.00 Amount
								,apd.GWPaymentAdviseId 
								,onw.FormulaDesID
                                ,EN.UserName Entity,Department.UserName Department,Section.UserName Section,SubSection.UserName SubSection
								,B.Basic,B.BasicSalaryHeadID
                                ,format(sum(apd.OverStay),'N2') OverStayMinute
                                ,format((sum(apd.OverStay)/60),'N2') OverStayHour
                                ,DesM.UserName GivenDesignation,DG.UserName LegalDesignation,EI.PaymentMode PaymentSource
                                from [dbo].[AttdnProcessData] apd 
                                left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID 
                                left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						        left join ORG.Position PO on PO.Id=MPB.PositionId
                                LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						        LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						        LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                                LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						        LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
								                                  LEFT JOIN(SELECT ((SID.DefineAmount/208)*2) RatePerHour,SH.SalaryHead
								                                  ,SID.SalaryID,SID.DefineAmount Gross
                                                                      FROM SalaryInfoDefine SID 
								                                  LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                    WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
								
                                left join mst.DesignationMaster dml on dml.DesignationId=ei.GivenDesignationId
								inner join (select DesignationMasterId,OverTimePmtPolicyMasterID,IsOTEntitled ,PlantId
                                                            from scs.DesignationMasterConfiguration where PlantId in ('" + identity.PlantId + @"') and IsOTEntitled=1) dc 
                                                            on dc.DesignationMasterId=dml.Id and ei.PlantId = dc.PlantId
												left join OverTimePmtPolicyMaster otpm on otpm.ID=dc.OverTimePmtPolicyMasterID and otpm.PlantID in ('" + identity.PlantId + @"')
												left join OverTimePmtPolicyDetails oNW on oNW.OverTimePmtPolicyID=otpm.ID and onw.OverTimeDayType='Working Day'
								left  join (SELECT SID.DefineAmount Basic,SH.SalaryHeadID BasicSalaryHeadID,SID.SalaryID
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Basic') B ON B.SalaryID=SIDM.SystemID
                                where apd.WorkDate between '" + fromDate + @"' and '" + toDate + @"' AND DayStatus IN('P','W','L') AND apd.GWPaymentAdviseId is null AND apd.IsOTEntitled=1 and SIDM.IsApproved=1 and EI.EmployeeStatus='Active' and (ISNULL(apd.StandardOT,0)>0 or ISNULL(apd.AdditionalOT,0)>0)
                                group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross,g.RatePerHour,apd.GWPaymentAdviseId,Department.UserName,Section.UserName,SubSection.UserName
								,onw.FormulaDesID,B.Basic,B.BasicSalaryHeadID,EN.UserName,DesM.UserName,DG.UserName,EI.PaymentMode
                                )T 
                                left join (select wad.EmpSystemId,SUM(wad.AdvanceAmount)TobeApprovedAmount
									                                from [TRN].[EmployeeAdvance] wa
									                                left join [TRN].[EmployeeAdvanceDetail] wad on wad.EmployeeAdvanceId=wa.Id
									                                where convert( DateTime, wa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, wa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
																	AND wa.ApprovedStatus='TobeApproved' AND wa.SourceType='MultipleEmployeeAdvance' AND wad.EmployeeAdvanceId<>'" + employeeAdvanceId + @"' GROUP BY wad.EmpSystemId
									                                )WRTobeApproved on WRTobeApproved.EmpSystemId=T.EmpSystemID
								left join (select wad.EmpSystemId,SUM(wad.AdvanceAmount)ApprovedAmount
									                                from [TRN].[EmployeeAdvance] wa
									                                left join [TRN].[EmployeeAdvanceDetail] wad on wad.EmployeeAdvanceId=wa.Id
									                                where convert( DateTime, wa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, wa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
																	AND wa.ApprovedStatus IN ('Approved','Paid') AND wa.SourceType='MultipleEmployeeAdvance' AND wad.VoucherId IS NULL GROUP BY wad.EmpSystemId
									                                )WRApproved on WRApproved.EmpSystemId=T.EmpSystemID
								left join (select wad.EmpSystemId,SUM(wad.AdvanceAmount)PaidAmount
									                                from [TRN].[EmployeeAdvance] wa
									                                left join [TRN].[EmployeeAdvanceDetail] wad on wad.EmployeeAdvanceId=wa.Id
									                                where convert( DateTime, wa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, wa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
																	AND wa.ApprovedStatus='Paid' AND wa.SourceType='MultipleEmployeeAdvance' AND wad.VoucherId IS NOT NULL GROUP BY wad.EmpSystemId
									                                )WRPaid on WRPaid.EmpSystemId=T.EmpSystemID
                                left join (select wad.Id,wad.EmpSystemId,wad.AdvanceAmount
									                                from [TRN].[EmployeeAdvance] wa
									                                left join [TRN].[EmployeeAdvanceDetail] wad on wad.EmployeeAdvanceId=wa.Id
									                                where wad.EmployeeAdvanceId='" + employeeAdvanceId + @"'
																	AND wa.ApprovedStatus='TobeApproved' AND wa.SourceType='MultipleEmployeeAdvance'
									                                )WRUpdate on WRUpdate.EmpSystemId=T.EmpSystemID
                                WHERE T.PresentDays >= '" + minimumPresentDay + @"' 
								order by T.EmployeeCode ";

                    }
                    

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            DataTable dtData = _sqlRepository.GetDataTable(sql);
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                DataRow dtValueRow = dtValue.NewRow();

                dtValueRow["BasicSalaryHeadID"] = dtData.Rows[i]["BasicSalaryHeadID"].ToString().Trim();
                dtValueRow["Basic"] = dtData.Rows[i]["Basic"].ToString().Trim();

                dtValue.Rows.Add(dtValueRow);
                
                if (!string.IsNullOrEmpty(dtData.Rows[i]["FormulaDesID"].ToString()))
                {
                    ReLoadFormulaWithValue(dtData.Rows[i]["FormulaDesID"].ToString(), ref dtValue, out string _formulaValue);
                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("##,##0.00");

                    dtValueRow["BasicSalaryHeadID"] = dtData.Rows[i]["BasicSalaryHeadID"].ToString().Trim();
                    dtValueRow["Rate"] = Convert.ToDecimal(sFormulaResult);

                    DataView dv = new DataView(dtData);
                    dv.RowFilter = "EmpSystemId='" + dtData.Rows[i]["EmpSystemId"].ToString() + "'";
                    var basic = 0.0M;
                    var payDays = 0.0M;
                    var standardOTHour = 0.0M;
                    var additionalOTHour = 0.0M;
                    var tobeApprovedAmount = 0.0M;
                    var approvedAmount = 0.0M;
                    var paidAmount = 0.0M;
                    var advanceAmount = 0.0M;
                    var rate = 0.0M;
                    basic = Convert.ToDecimal(dtData.Rows[i]["Basic"].ToString());
                    rate = Convert.ToDecimal(sFormulaResult);

                    if (isPayDay== true)
                    {
                        payDays = Convert.ToDecimal(dtData.Rows[i]["PayDays"].ToString());
                    }
                    if (isStandardOT == true)
                    {
                        standardOTHour = Convert.ToDecimal(dtData.Rows[i]["StandardOTHour"].ToString());
                    }
                    if (isAdditionalOT == true)
                    {
                        additionalOTHour = Convert.ToDecimal(dtData.Rows[i]["AdditionalOTHour"].ToString());
                    }
                    tobeApprovedAmount = Convert.ToDecimal(dtData.Rows[i]["TobeApprovedAmount"].ToString());
                    approvedAmount = Convert.ToDecimal(dtData.Rows[i]["ApprovedAmount"].ToString());
                    paidAmount = Convert.ToDecimal(dtData.Rows[i]["PaidAmount"].ToString());
                    advanceAmount = Convert.ToDecimal(dtData.Rows[i]["AdvanceAmount"].ToString());
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        
                            drmo.BeginEdit();
                            drmo["Rate"] = Convert.ToDecimal(sFormulaResult);
                            drmo["Amount"] = Math.Floor((((((basic/30)*payDays) + ((standardOTHour + additionalOTHour) * rate)) * percentage/100)/multiple)) * multiple - tobeApprovedAmount- approvedAmount- paidAmount;
                            if (saveUpdate == "Save")
                            {
                                drmo["AdvanceAmount"] = Math.Floor((((((basic / 30) * payDays) + ((standardOTHour + additionalOTHour) * rate)) * percentage / 100) / multiple)) * multiple - tobeApprovedAmount - approvedAmount - paidAmount;
                            }
                            else
                            {
                                if(advanceAmount>0)
                                {
                                    drmo["AdvanceAmount"] = advanceAmount;
                                }
                                else
                                {
                                    drmo["AdvanceAmount"] = Math.Floor((((((basic / 30) * payDays) + ((standardOTHour + additionalOTHour) * rate)) * percentage / 100) / multiple)) * multiple - tobeApprovedAmount - approvedAmount - paidAmount;
                                }
                            }

                            drmo.EndEdit();

                    }
                    dtValue = new DataTable();
                    dtValue.TableName = "TempTable";
                    dtValue.Columns.Add("BasicSalaryHeadID");
                    dtValue.Columns.Add("Basic");
                    dtValue.Columns.Add("Rate");
                }
            }


            List<Dictionary<string, object>> data = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtData);
            JsonResult json = Json(data.Where(r => Convert.ToDecimal(r["Amount"].ToString()) > 0), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public void ReLoadFormulaWithValue(string strFormulaID, ref DataTable dtValue, out string lblFormulaValue/*, ref DataTable dtSlrHd*/)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;

            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView();
                        dvLocal.Table = dtValue;

                        dvLocal.RowFilter = "BasicSalaryHeadID = '" + strTemp.Trim() + "'";
                        if (dvLocal.Count > 0)
                        {
                            strTemp = dvLocal[0]["Basic"].ToString().Trim();
                        }
                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End 

        [HttpPost, Authorize]
        public ActionResult LoadPCEmployeelist(string fromDate, string toDate, string tabName)
        {
            string sql = string.Empty;
            try
            {
                if (tabName == "GoodWork")
                {
                    sql = @"select CheckBoxSelect=cast(case when z.Id is null then 0 else 1 end as bit),z.Id,ei.SystemId EmpSystemId,ei.EmployeeCode,ei.EmployeeName,sum(gwd.Minute) Minute,(sum(gwd.Minute)/60) Hour
                                    ,format(g.Gross,'N2') Gross,format(g.RatePerHour,'N2') Rate,Amount=format((sum(gwd.Minute)/60)*g.RatePerHour,'N2'),Department.UserName Department,Section.UserName Section,SubSection.UserName SubSection
                                     from [dbo].[GoodWork] gw
                                     left join  GoodWorkDetail GWD on GWD.GoodWorkId=gw.Id 
                                     left join EmployeeInformation ei on ei.SystemId=GWD.EmpSystemId 
                                    left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						            left join ORG.Position PO on PO.Id=MPB.PositionId
                                    LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						            LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						            LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						            LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                     LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
                                     LEFT JOIN(SELECT SID.DefineAmount Basic,((SID.DefineAmount/208)*2) RatePerHour,SH.SalaryHead
								                                      ,SID.SalaryID,SID.DefineAmount Gross
                                                                          FROM SalaryInfoDefine SID 
								                                      LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                        WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
                                    left join (select gwpad.EmpSystemId,gwpad.Id
									                                from GoodWorkPaymentAdvise gwpa
									                                left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
									                                where convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
									                                )z on z.EmpSystemId=GWD.EmpSystemId
                                     where gw.WorkDate between '" + fromDate + @"' and '" + toDate + @"' and gwd.Minute<>0 and g.Gross<>0  and SIDM.IsApproved=1 AND gw.ApprovedStatus='Approved'
                                     group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross,g.RatePerHour,z.Id,Department.UserName,Section.UserName,SubSection.UserName
                                    order by ei.EmployeeCode";

                }
                else
                {
                    sql = @"select CheckBoxSelect=cast(case when z.Id is null then 0 else 1 end as bit),ei.SystemId EmpSystemId,z.Id,ei.EmployeeCode,ei.EmployeeName,format(sum(apd.OverStay),'N2') Minute
                                ,format((sum(apd.OverStay)/60),'N2') Hour
                                ,format(g.Gross,'N2') Gross
                                ,format(g.RatePerHour,'N2') Rate 
                                ,Amount=format(g.RatePerHour*(sum(apd.OverStay)/60),'N2'),apd.GWPaymentAdviseId 
                                ,Department.UserName Department,Section.UserName Section,SubSection.UserName SubSection
                                from [dbo].[AttdnProcessData] apd 
                                left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID 
                                left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
						        left join ORG.Position PO on PO.Id=MPB.PositionId
                                LEFT OUTER JOIN ORG.Entity EN ON MPB.EntityId=EN.Id
						        LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
						        LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = ei.GivenDesignationId
                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						        LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = EI.SystemId
								                                  LEFT JOIN(SELECT ((SID.DefineAmount/208)*2) RatePerHour,SH.SalaryHead
								                                  ,SID.SalaryID,SID.DefineAmount Gross
                                                                      FROM SalaryInfoDefine SID 
								                                  LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
                                                                    WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
								left join (select gwpad.EmpSystemId,gwpad.Id
									                                from GoodWorkPaymentAdvise gwpa
									                                left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id
									                                where convert( DateTime, gwpa.FromDate) between '" + fromDate + @"' and '" + toDate + @"' and convert( DateTime, gwpa.ToDate) between '" + fromDate + @"' and '" + toDate + @"'
									                                )z on z.EmpSystemId=apd.EmpSystemID
                                where apd.WorkDate between '" + fromDate + @"' and '" + toDate + @"' AND DayStatus IN('P','W','L') AND OverStay<>0 AND apd.IsOTEntitled=1 and SIDM.IsApproved=1 
                                group by ei.SystemId,ei.EmployeeCode,ei.EmployeeName,g.Gross,g.RatePerHour,apd.GWPaymentAdviseId,z.Id,Department.UserName,Section.UserName,SubSection.UserName
                                order by ei.EmployeeCode";
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
                        id = "'" + item["EmployeeAdvanceDetailId"] + "'";
                    else
                        id = id + ",'" + item["EmployeeAdvanceDetailId"] + "'";
                }

                con.OpenDataSetThroughAdapter("select * from [TRN].[EmployeeAdvanceDetail] where Id in (" + id + ")", out dsAdvance, false, "1");
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
                            item["EmployeeAdvanceId"] = item["EmployeeAdvanceId"];
                            item["Amount"] = item["Amount"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);
                        }
                        else
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo.EndEdit();
                        }


                        DataView dvAdvance = new DataView(dsAdvance.Tables[0]);
                        dvAdvance.RowFilter = "Id='" + item["EmployeeAdvanceDetailId"] + "' and EmpSystemId = '" + item["EmpSystemId"] + "'";

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

                //if (tabName == "GoodWork")
                //{
                //    con.OpenDataSetThroughAdapter(@"select * from GoodWorkDetail gwd
                //                                where gwd.GoodWorkId in (select Id from GoodWork gw where gw.WorkDate between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"')", out dsGWPayable, false, "1");

                //    for (int i = 0; i < dsGWPayable.Tables[0].Rows.Count; i++)
                //    {
                //        dsGWPayable.Tables[0].DefaultView.RowFilter = "Id='" + dsGWPayable.Tables[0].Rows[i]["Id"] + "'";

                //        if (dsGWPayable.Tables[0].DefaultView.Count > 0)
                //        {
                //            DataRow drGW = dsGWPayable.Tables[0].DefaultView[0].Row;
                //            drGW.BeginEdit();

                //            drGW["GWPaymentAdviseId"] = _MasterId;
                //            drGW.EndEdit();
                //        }
                //    }
                //}
                //else
                //{
                //    con.OpenDataSetThroughAdapter(@"select * from [dbo].[AttdnProcessData] apd 
                //        where apd.WorkDate between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"' AND apd.DayStatus IN('P','W','L') AND apd.OverStay<>0 
                //        and apd.EmpSystemID not in (select EmpSystemId                              
                //        from GoodWorkPaymentAdvise gwpa                              
                //        left join GoodWorkPaymentAdviseDetail gwpad on gwpad.PaymentAdviseId=gwpa.Id                              
                //        where convert( DateTime, gwpa.FromDate) between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"' and convert( DateTime, gwpa.ToDate) between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"'
                //        )
                //        AND apd.IsOTEntitled in(select D.IsOTEntitled
                //        from [dbo].[AttdnProcessData] apd 
                //        left join EmployeeInformation ei on ei.SystemId=apd.EmpSystemID  
                //        left join (SELECT C.IsOTEntitled,D.Id FROM SCS.DesignationMasterConfiguration C
                //                                            LEFT JOIN MST.DesignationMaster M ON M.Id=C.DesignationMasterId
                //                                            LEFT JOIN HKP.Designation D ON D.Id=M.DesignationId
                //                 where C.IsOTEntitled=1
                //                       )D on D.Id=ei.GivenDesignationId
                //        )", out dsExtraOT, false, "1");

                //    for (int i = 0; i < dsExtraOT.Tables[0].Rows.Count; i++)
                //    {
                //        dsExtraOT.Tables[0].DefaultView.RowFilter = "EmpSystemID='" + dsExtraOT.Tables[0].Rows[i]["EmpSystemID"] + "' and WorkDate='" + dsExtraOT.Tables[0].Rows[i]["WorkDate"] + "'";

                //        if (dsExtraOT.Tables[0].DefaultView.Count > 0)
                //        {
                //            DataRow drGW = dsExtraOT.Tables[0].DefaultView[0].Row;
                //            drGW.BeginEdit();

                //            drGW["GWPaymentAdviseId"] = _MasterId;
                //            drGW.EndEdit();
                //        }
                //    }
                //}

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);
                string detailIds = "";
                if (goodWorkPaymentAdviseDetail != null)
                {
                    foreach (var item in goodWorkPaymentAdviseDetail)
                    {
                        if (detailIds == "")
                        {
                            detailIds = "'" + item["EmpSystemId"] + "'"; ;
                        }
                        else
                        {
                            detailIds += ",'" + item["EmpSystemId"] + "'";

                        }
                    }
                }
                ConnectionManager.clsConnection conUpdate = new ConnectionManager.clsConnection();
                conUpdate.BeginTransaction();
                if (tabName == "GoodWork")
                {
                    conUpdate.executeQuery("UPDATE GWD SET  GWD.GWPaymentAdviseId='" + _MasterId + "' from [dbo].[GoodWork] gw inner join  GoodWorkDetail GWD on GWD.GoodWorkId = gw.Id   where GWD.EmpSystemId in (" + detailIds + ") and gw.WorkDate between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"' and gwd.Minute<>0   AND gw.ApprovedStatus='Approved' AND GWD.GWPaymentAdviseId IS NULL");
                }
                else
                {
                    conUpdate.executeQuery("UPDATE [dbo].[AttdnProcessData] SET GWPaymentAdviseId='" + _MasterId + "'  where EmpSystemId in (" + detailIds + ") and WorkDate between '" + data["FromDate"] + @"' and '" + data["ToDate"] + @"' AND DayStatus NOT  IN ('A') AND ISNULL(AdditionalOT,0)<>0 AND GWPaymentAdviseId IS NULL ");
                }
                conUpdate.CommitTransaction();
                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });
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
						,gwp.UserRef,FORMAT(gwp.PaymentDate,'dd-MMM-yyy') PaymentDate,gwp.Remarks,gwp.ApprovedById
						from GoodWorkPaymentAdvise gwp 
						left join EmployeeInformation ei on ei.SystemId=gwp.ByWhomId
                        where gwp.ApprovedStatus is null AND gwp.PaymentSource='" + paymentSource + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdvisePendingPaymentList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select ei.EmployeeCode,ei.EmployeeName ByWhom,gwp.Id,FORMAT(gwp.FromDate,'dd-MMM-yyy') FromDate,FORMAT(gwp.ToDate,'dd-MMM-yyy')ToDate
						,gwp.UserRef,FORMAT(gwp.PaymentDate,'dd-MMM-yyy') PaymentDate,gwp.Remarks,gwp.PaymentSource
						from GoodWorkPaymentAdvise gwp 
						left join EmployeeInformation ei on ei.SystemId=gwp.ByWhomId
                        where gwp.ApprovedStatus is null AND gwp.ApprovedById='" + identity.EmployeeId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveGoodWorkPaymentAdvisePendingPayment(Dictionary<string, object> data, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail)
        {
            try
            {
                string goodWorkPaymentAdviseDetailIds = "";
                if (goodWorkPaymentAdviseDetail != null)
                {
                    foreach (var item in goodWorkPaymentAdviseDetail)
                    {
                        if (goodWorkPaymentAdviseDetailIds == "")
                        {
                            goodWorkPaymentAdviseDetailIds = "'" + item["Id"] + "'"; ;
                        }
                        else
                        {
                            goodWorkPaymentAdviseDetailIds += ",'" + item["Id"] + "'";

                        }
                    }
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("UPDATE [dbo].[GoodWorkPaymentAdvise] SET PaymentCreationById='" + identity.EmployeeId + "' ,ApprovedStatus='PaymentApproved'  where Id='" + data["Id"] + "' ");
                con.executeQuery("UPDATE [dbo].[GoodWorkPaymentAdvisedetail] SET IsCheck=1  where Id in (" + goodWorkPaymentAdviseDetailIds + ") ");
                con.CommitTransaction();


                return Json(new { Error = false, Data = data, Message = "Approved Successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdvisePendingApprovalList()
        {
            string sql = @"select ei.EmployeeCode,ei.EmployeeName ByWhom,gwp.Id,FORMAT(gwp.FromDate,'dd-MMM-yyy') FromDate,FORMAT(gwp.ToDate,'dd-MMM-yyy')ToDate
						,gwp.UserRef,FORMAT(gwp.PaymentDate,'dd-MMM-yyy') PaymentDate,gwp.Remarks,gwp.PaymentSource
						from GoodWorkPaymentAdvise gwp 
						left join EmployeeInformation ei on ei.SystemId=gwp.ByWhomId
                        where gwp.ApprovedStatus ='PaymentCreation' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveGoodWorkPaymentAdvisePendingApproval(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("UPDATE [dbo].[GoodWorkPaymentAdvise] SET ApprovedById='" + identity.EmployeeId + "' ,ApprovedStatus='PaymentApproved'  where Id='" + data["Id"] + "' ");
                con.CommitTransaction();
                return Json(new { Error = false, Data = data, Message = "Approved Successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdviseDetailList(string paymentAdviseId)
        {
            string sql = @"select gwpad.Id,gwpad.PaymentAdviseId,gwpad.EmpSystemId,ei.EmployeeCode,ei.EmployeeName,gwpad.Hour,gwpad.Hour*60 Minute,gwpad.Rate,gwpad.Amount,gwpad.Remarks
                            from GoodWorkPaymentAdviseDetail gwpad
                            left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
							left join GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
                            where gwpa.Id='" + paymentAdviseId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdviseDetailForApproveList(string paymentAdviseId)
        {
            string sql = @"select isSelected = Convert(bit, 'True'),CheckBoxSelect=Convert(bit, 'True'),gwpad.Id,gwpad.PaymentAdviseId,gwpad.EmpSystemId,ei.EmployeeCode,ei.EmployeeName,gwpad.Hour,gwpad.Hour*60 Minute,gwpad.Rate,gwpad.Amount,gwpad.Remarks
                            ,gwpad.IsCheck,isnull(gwpad.IsDisburse,0)IsDisburse
                            from GoodWorkPaymentAdviseDetail gwpad
                            left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
							left join GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
                            where gwpa.Id='" + paymentAdviseId + "' and gwpad.IsCheck IS NULL AND gwpad.IsDisburse IS NULL AND gwpad.DisbursementVoucherId IS NULL ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdviseDetailCheckedList(string paymentAdviseId, string paymentMode)
        {
            string sql = @" DECLARE @paymentMode nvarchar(50)='" + paymentMode + @"'
select isSelected = Convert(bit, 'True'),CheckBoxSelect=Convert(bit, 'True'),gwpad.Id,gwpad.PaymentAdviseId,gwpad.EmpSystemId,ei.EmployeeCode,ei.EmployeeName,gwpad.Hour,gwpad.Hour*60 Minute,gwpad.Rate,gwpad.Amount,gwpad.Remarks
                            ,gwpad.IsCheck,isnull(gwpad.IsDisburse,0)IsDisburse,ISNULL(gwpad.PaymentMode,'') PaymentMode
                            from GoodWorkPaymentAdviseDetail gwpad
                            left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
							left join GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
                            where gwpa.Id='" + paymentAdviseId + @"' and gwpad.IsCheck=1 AND ISNULL(gwpad.IsDisburse,0)=0 AND gwpad.DisbursementVoucherId IS NULL 
                            AND ISNULL(gwpad.PaymentMode,'')=CASE WHEN @paymentMode<>'' THEN @paymentMode ELSE ISNULL(gwpad.PaymentMode,'') END ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult GetGoodWorkPaymentAdviseDisbursementJVDataList(string disbursementAdviceId, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail , string paymentMode)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string goodWorkPaymentAdviseDetailIds = "";
            if (goodWorkPaymentAdviseDetail != null)
            {
                foreach (var item in goodWorkPaymentAdviseDetail)
                {
                    if (goodWorkPaymentAdviseDetailIds == "")
                    {
                        goodWorkPaymentAdviseDetailIds = "'" + item["Id"] + "'"; ;
                    }
                    else
                    {
                        goodWorkPaymentAdviseDetailIds += ",'" + item["Id"] + "'";

                    }
                }
            }


            string sql = null;
            sql = @"DECLARE @paymentMode nvarchar(50)='" + paymentMode + @"'
SELECT X.GLName,X.BudgetName,X.ActivityName, SUM(X.DrAmount) DrAmount,SUM(X.CrAmount) CrAmount,SUM(X.Amount) Amount,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                FROM
                (
                SELECT  'GoodWorkPayment' AS OtherName, 'Dr' AS TrnType
                , gwpad.Amount DrAmount 
                , 0 CrAmount 
                , gwpad.Amount
                ,GAD.GLGeneralInfoId  ,GAD.BudgetMasterId,GAD.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName 
				FROM GoodWorkPaymentAdviseDetail gwpad
                left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
				LEFT JOIN GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
				LEFT JOIN HKP.GeneralAccountDeterminate GAD ON  GAD.Id='GoodWorkPayment'
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
				LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
				WHERE gwpa.PaymentSource='GoodWork' and gwpad.IsCheck=1 AND ISNULL(gwpad.IsDisburse,0)=0 AND gwpad.DisbursementVoucherId IS NULL 
                AND ISNULL(gwpad.PaymentMode,'')=CASE WHEN @paymentMode<>'' THEN @paymentMode ELSE ISNULL(gwpad.PaymentMode,'') END
                AND gwpad.PaymentAdviseId='" + disbursementAdviceId + @"' AND gwpad.Id in (" + goodWorkPaymentAdviseDetailIds + @")

                Union All
                SELECT  'ExtraOTPayment' AS OtherName, 'Dr' AS TrnType
                , gwpad.Amount DrAmount 
                , 0 CrAmount 
                , gwpad.Amount
                ,GAD.GLGeneralInfoId  ,GAD.BudgetMasterId,GAD.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName 
				FROM GoodWorkPaymentAdviseDetail gwpad
                left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
				LEFT JOIN GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
				LEFT JOIN HKP.GeneralAccountDeterminate GAD ON  GAD.Id='ExtraOTPayment'
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
				LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
				WHERE gwpa.PaymentSource='Attendance' and gwpad.IsCheck=1 AND ISNULL(gwpad.IsDisburse,0)=0 AND gwpad.DisbursementVoucherId IS NULL 
                AND ISNULL(gwpad.PaymentMode,'')=CASE WHEN @paymentMode<>'' THEN @paymentMode ELSE ISNULL(gwpad.PaymentMode,'') END
                AND gwpad.PaymentAdviseId='" + disbursementAdviceId + @"' AND gwpad.Id in (" + goodWorkPaymentAdviseDetailIds + @")
                        
                )X
                GROUP BY

                X.GLName,X.BudgetName,X.ActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId
                ORDER BY 5";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetGoodWorkPaymentAdviseApprovedList()
        {
            string sql = @"select ei.EmployeeCode,ei.EmployeeName ByWhom,gwp.Id,FORMAT(gwp.FromDate,'dd-MMM-yyy') FromDate,FORMAT(gwp.ToDate,'dd-MMM-yyy')ToDate
						,gwp.UserRef,FORMAT(gwp.PaymentDate,'dd-MMM-yyy') PaymentDate,gwp.Remarks,gwp.PaymentSource,ISNULL(gwp.PaymentsStatus,'Active') PaymentsStatus
                        ,(select SUM(gwpad.Amount)DisbursementAmount
                                from GoodWorkPaymentAdviseDetail gwpad
                                where gwpad.PaymentAdviseId=gwp.Id and gwpad.IsCheck=1 AND ISNULL(gwpad.IsDisburse,0)=0  AND gwpad.DisbursementVoucherId IS NULL)DisbursementAmount
						from GoodWorkPaymentAdvise gwp 
						left join EmployeeInformation ei on ei.SystemId=gwp.ByWhomId
                        where gwp.ApprovedStatus ='PaymentApproved' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeMultipleAdvanceApprovedList()
        {
            string sql = @"SELECT * FROM 
                            (select ei.EmployeeCode,ei.EmployeeName ByWhom,gwp.Id,FORMAT(gwp.FromDate,'dd-MMM-yyy') FromDate,FORMAT(gwp.ToDate,'dd-MMM-yyy')ToDate
                        ,gwp.UserRef,gwp.UserName,gwp.Remarks,ISNULL(gwp.PaymentsStatus,'Active') PaymentsStatus
                        ,(select SUM(gwpad.AdvanceAmount)DisbursementAmount
                        from [TRN].[EmployeeAdvanceDetail] gwpad
                        where gwpad.EmployeeAdvanceId=gwp.Id and gwpad.IsApprove=1 AND ISNULL(gwpad.IsDisburse,0)=0  AND gwpad.VoucherId IS NULL)DisbursementAmount
						,(SELECT COUNT(Id)CountEmployee FROM [TRN].[EmployeeAdvanceDetail] WHERE VoucherId IS NULL AND EmployeeAdvanceId=gwp.Id and IsApprove=1 AND ISNULL(IsDisburse,0)=0)CountEmployee
                        from [TRN].[EmployeeAdvance] gwp 
                        left join EmployeeInformation ei on ei.SystemId=gwp.PreparedById
                        where gwp.ApprovedStatus  IN ('Approved','Paid' ) AND gwp.SourceType='MultipleEmployeeAdvance' )T 
                        WHERE T.CountEmployee>0 ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeMultipleAdvanceDetailCheckedList(string paymentAdviseId, string paymentMode)
        {
            string sql = @" DECLARE @paymentMode nvarchar(50)='" + paymentMode + @"'
                            select isSelected = Convert(bit, 'True'),CheckBoxSelect=Convert(bit, 'True'),gwpad.Id,gwpad.EmployeeAdvanceId,gwpad.EmpSystemId,ei.EmployeeCode,ei.EmployeeName,gwpad.PayDays,gwpad.StandardOTHour,gwpad.AdditionalOTHour,gwpad.Rate,gwpad.Amount,gwpad.AdvanceAmount,gwpad.Remarks
                            ,gwpad.IsApprove,isnull(gwpad.IsDisburse,0)IsDisburse,gwpa.YearNo,gwpa.MonthNo,ISNULL(gwpad.PaymentSource,'') PaymentMode
                            from [TRN].[EmployeeAdvanceDetail] gwpad
                            left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
							left join [TRN].[EmployeeAdvance] gwpa on gwpa.Id=gwpad.EmployeeAdvanceId
                            where gwpa.Id='" + paymentAdviseId + @"' AND gwpa.SourceType='MultipleEmployeeAdvance' and gwpad.IsApprove=1 AND ISNULL(gwpad.IsDisburse,0)=0 AND gwpad.VoucherId IS NULL 
                            AND ISNULL(gwpad.PaymentSource,'')=CASE WHEN @paymentMode<>'' THEN @paymentMode ELSE ISNULL(gwpad.PaymentSource,'') END ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult GetEmployeeMultipleAdvanceDisbursementJVDataList(string disbursementAdviceId, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail, string paymentMode, VoucherViewModel voucherVM)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string goodWorkPaymentAdviseDetailIds = "";
            if (goodWorkPaymentAdviseDetail != null)
            {
                foreach (var item in goodWorkPaymentAdviseDetail)
                {
                    if (goodWorkPaymentAdviseDetailIds == "")
                    {
                        goodWorkPaymentAdviseDetailIds = "'" + item["Id"] + "'"; ;
                    }
                    else
                    {
                        goodWorkPaymentAdviseDetailIds += ",'" + item["Id"] + "'";

                    }
                }
            }


            string sql = null;
            sql = @" DECLARE @paymentMode nvarchar(50)='" + paymentMode + @"'
                SELECT X.GLName,X.BudgetName,X.ActivityName, SUM(X.DrAmount) DrAmount,SUM(X.CrAmount) CrAmount,SUM(X.Amount) Amount,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId,x.BudgetMasterActivityId
                FROM
                ( SELECT  'ExtraOTPayment' AS OtherName, 'Dr' AS TrnType
                , gwpad.AdvanceAmount DrAmount 
                , 0 CrAmount 
                , gwpad.AdvanceAmount Amount
                ,'" + voucherVM.GLGeneralInfoId + @"' GLGeneralInfoId ,'" + voucherVM.BudgetMasterId + @"' BudgetMasterId,'" + voucherVM.ActivityId + @"' ActivityId
                , '" + voucherVM.GLGeneralInfoName + @"' GLName , '" + voucherVM.BudgetName + @"' BudgetName,'" + voucherVM.ActivityName + @"' ActivityName,'" + voucherVM.DrBudgetMasterId + @"' BudgetMasterActivityId 
				FROM [TRN].[EmployeeAdvanceDetail] gwpad
				LEFT JOIN [TRN].[EmployeeAdvance] gwpa on gwpa.Id=gwpad.EmployeeAdvanceId
				WHERE gwpa.SourceType='MultipleEmployeeAdvance' AND gwpad.IsApprove=1 AND ISNULL(gwpad.IsDisburse,0)=0 AND gwpad.VoucherId IS NULL AND gwpad.EmployeeAdvanceId='" + disbursementAdviceId + @"' AND gwpad.Id in (" + goodWorkPaymentAdviseDetailIds + @")
                AND ISNULL(gwpad.PaymentSource,'')=CASE WHEN @paymentMode<>'' THEN @paymentMode ELSE ISNULL(gwpad.PaymentSource,'') END 
                        
                )X
                GROUP BY

                X.GLName,X.BudgetName,X.ActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId,x.BudgetMasterActivityId
                ORDER BY 5";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEmployeeMultipleAdvanceDisbursementVoucherList(GridParameter parameters)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetEmployeeMultipleAdvanceDisbursementVoucherList(parameters,SourceType.MultipleEmployeeAdvance), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ParkEmployeeMultipleAdvanceDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.Amount = directJVList.Sum(r => r.CrAmount);
            voucherVM.SourceType = SourceType.EmployeeAdvance.ToString();


            string goodWorkPaymentAdviseDetailIds = "";
            if (goodWorkPaymentAdviseDetail != null)
            {
                foreach (var item in goodWorkPaymentAdviseDetail)
                {
                    if (goodWorkPaymentAdviseDetailIds == "")
                    {
                        goodWorkPaymentAdviseDetailIds = "'" + item["Id"] + "'"; ;
                    }
                    else
                    {
                        goodWorkPaymentAdviseDetailIds += ",'" + item["Id"] + "'";

                    }
                }
            }

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _salaryDisbursementService.ParkEmployeeMultipleAdvanceDisbursement(voucherVM, directJVList, disbursementAdviceId, goodWorkPaymentAdviseDetailIds, goodWorkPaymentAdviseDetail)) });
        }
        [HttpPost]
        public JsonResult PostEmployeeMultipleAdvanceDisbursement(string voucherId)
        {
            _salaryDisbursementService.PostEmployeeMultipleAdvanceDisbursement(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpPost]
        public ActionResult DeleteEmployeeMultipleAdvanceDisbursement(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salaryDisbursementService.DeleteEmployeeMultipleAdvanceDisbursement(identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public JsonResult SaveGoodWorkPaymentAdvisePayments(Dictionary<string, object> data, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail)
        {
            try
            {
                string goodWorkPaymentAdviseDetailIds = "";
                if (goodWorkPaymentAdviseDetail != null)
                {
                    foreach (var item in goodWorkPaymentAdviseDetail)
                    {
                        if (goodWorkPaymentAdviseDetailIds == "")
                        {
                            goodWorkPaymentAdviseDetailIds = "'" + item["Id"] + "'"; ;
                        }
                        else
                        {
                            goodWorkPaymentAdviseDetailIds += ",'" + item["Id"] + "'";

                        }
                    }
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("UPDATE [dbo].[GoodWorkPaymentAdvisedetail] SET IsDisburse=1,PaymentsDate=GETDATE(), PaymentsById='" + identity.EmployeeId + "'  where Id in (" + goodWorkPaymentAdviseDetailIds + ") ");
                con.executeQuery("UPDATE [dbo].[GoodWorkPaymentAdvise] SET PaymentsStatus=CASE WHEN (SELECT COUNT(Id)Id FROM [dbo].[GoodWorkPaymentAdvisedetail]   where PaymentAdviseId= '" + data["Id"] + "' AND ISNULL(IsCheck,0)=1 AND ISNULL(IsDisburse,0)=0)>0 THEN 'Partial Payments' ELSE 'Full Payments' END  where Id='" + data["Id"] + "' ");
                con.CommitTransaction();


                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GoodWorkPaymentAdvisePaymentsReports(ReportFormat reportFormat, string goodWorkPaymentAdviseId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableReportService accountsInventoryPayableReportService = new AccountsInventoryPayableReportService(_sqlRepository);
            var reportFileName = "Good Work Payment Advise Payments Reports";
            var workbook = accountsInventoryPayableReportService.GoodWorkPaymentAdvisePaymentsReports(reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, goodWorkPaymentAdviseId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetGoodWorkPaymentAdviseDisbursementVoucherList(GridParameter parameters)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetGoodWorkPaymentAdviseDisbursementVoucherList(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ParkGoodWorkPaymentAdviseDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.Amount = directJVList.Sum(r => r.CrAmount);
            voucherVM.SourceType = SourceType.GoodWorkDisbursement.ToString();

            string goodWorkPaymentAdviseDetailIds = "";
            if (goodWorkPaymentAdviseDetail != null)
            {
                foreach (var item in goodWorkPaymentAdviseDetail)
                {
                    if (goodWorkPaymentAdviseDetailIds == "")
                    {
                        goodWorkPaymentAdviseDetailIds = "'" + item["Id"] + "'"; ;
                    }
                    else
                    {
                        goodWorkPaymentAdviseDetailIds += ",'" + item["Id"] + "'";

                    }
                }
            }

            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _salaryDisbursementService.ParkGoodWorkPaymentAdviseDisbursement(voucherVM, directJVList, disbursementAdviceId, goodWorkPaymentAdviseDetailIds)) });
        }
        [HttpPost]
        public JsonResult PostGoodWorkPaymentAdviseDisbursement(string voucherId)
        {
            _salaryDisbursementService.PostSalarydisbursement(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpPost]
        public ActionResult DeleteGoodWorkPaymentAdviseDisbursement(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salaryDisbursementService.DeleteGoodWorkPaymentAdviseDisbursement(identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
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
        public JsonResult GetGoodWorkCheckByCbo(string setupId)
        {
            var sql = @"select distinct E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text  
                          from dbo.GoodWorkCheckBySetUp A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.CheckById 
                          where E.EmployeeStatus='Active' AND A.GoodWorkSetUpId='" + setupId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGoodWorkApprovedByCbo(string setupId)
        {
            var sql = @"select distinct E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text  
                          from dbo.GoodWorkAuthoritySetUp A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.AuthorityId 
                          where E.EmployeeStatus='Active' AND A.GoodWorkSetUpId='" + setupId + "'";
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
        public JsonResult GetGoodWorkPaymentApproveByCboList()
        {
            return Json(clsSales.GetGoodWorkPaymentApproveByCboList(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeMultipleAdvanceApproveByCboList()
        {
            return Json(clsSales.GetEmployeeMultipleAdvanceApproveByCboList(), JsonRequestBehavior.AllowGet);
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
                        else
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
                    //data["ApprovedStatus"] = "Approved";
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
                        else
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
                _info.SaveDataSets(dsDetail);

                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult UpdateAll(List<Dictionary<string, object>> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster=null;
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                #region GW 
                objCon = new ConnectionManager.DAL.ConManager("1");
                string ids = "";
                foreach (var itm in data)
                {
                    if (ids.Length == 0)
                    {
                        ids = "'" + itm["Id"] + "'";
                    }
                    else
                    {
                        ids += ",'" + itm["Id"] + "'";
                    }
                }
                objCon.OpenDataSetThroughAdapter("select * from GoodWork where Id IN("+ ids + ")", out dsMaster, false, "1");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count > 0)
                        {
                            item["ApprovedStatus"] = "Approved";
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult UpdateCheckedAll(List<Dictionary<string, object>> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster = null;
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                #region GW 
                objCon = new ConnectionManager.DAL.ConManager("1");
                string ids = "";
                foreach (var itm in data)
                {
                    if (ids.Length == 0)
                    {
                        ids = "'" + itm["Id"] + "'";
                    }
                    else
                    {
                        ids += ",'" + itm["Id"] + "'";
                    }
                }
                objCon.OpenDataSetThroughAdapter("select * from GoodWork where Id IN(" + ids + ")", out dsMaster, false, "1");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count > 0)
                        {
                            item["CheckedStatus"] = "Checked";
                            item["ApprovedStatus"] = "To Be Approved";
                            item["ApprovedBy"] = item["ApproveBy"];
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkCheckedDataList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select GW.Id,format(GW.WorkDate,'dd-MMM-yyyy') WorkDate,S.UserName Shift,GW.Remarks,GWS.UserName UserGroup,GWS.Id UserGroupId,gw.Reason
                                    ,format(GW.FromTime,'hh:mm') FromTime,format(GW.ToTime,'hh:mm') ToTime,gw.Minute,gw.CheckedBy,gw.CheckedStatus,gw.ApprovedBy,gw.ApprovedStatus
                                    from GoodWork GW
                                    left join ShiftDefination S on S.SystemId=GW.ShiftId
									left join [dbo].[GoodWorkSetup] GWS on GWS.Id=GW.UserGroupId
									left join EmployeeInformation ei on ei.SystemId=gw.CheckedBy
                                    where GW.CheckedBy='" + identity.EmployeeId + "' AND CheckedStatus<>'Checked'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetGoodWorkApprovedDataList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select GW.Id,format(GW.WorkDate,'dd-MMM-yyyy') WorkDate,S.UserName Shift,GW.Remarks,GWS.UserName UserGroup,GWS.Id UserGroupId,gw.Reason
                                    ,format(GW.FromTime,'hh:mm') FromTime,format(GW.ToTime,'hh:mm') ToTime,gw.Minute,gw.CheckedBy,gw.CheckedStatus,gw.ApprovedBy,gw.ApprovedStatus
                                    from GoodWork GW
                                    left join ShiftDefination S on S.SystemId=GW.ShiftId
									left join [dbo].[GoodWorkSetup] GWS on GWS.Id=GW.UserGroupId
									left join EmployeeInformation ei on ei.SystemId=gw.CheckedBy
                                    where GW.ApprovedBy='" + identity.EmployeeId + "'AND CheckedStatus='Checked'  AND ApprovedStatus<>'Approved'";

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
        public ActionResult GetGoodWorkPaymentAdviseDisburseOTDetailList(string fromDate, string toDate)
        {
            string sql = @"select gwpad.Id,ei.EmployeeCode,ei.EmployeeName,gwpad.Hour,gwpad.Hour*60 Minute,gwpad.Rate,gwpad.Amount,gwpad.Remarks
                            from GoodWorkPaymentAdviseDetail gwpad
                            left join EmployeeInformation ei on ei.SystemId=gwpad.EmpSystemId
							left join GoodWorkPaymentAdvise gwpa on gwpa.Id=gwpad.PaymentAdviseId
                            left join (SELECT C.IsOTEntitled,D.Id FROM SCS.DesignationMasterConfiguration C
                            LEFT JOIN MST.DesignationMaster M ON M.Id=C.DesignationMasterId
                            LEFT JOIN HKP.Designation D ON D.Id=M.DesignationId
							)D on D.Id=ei.GivenDesignationId
                            where gwpa.FromDate='" + fromDate + "' and gwpa.ToDate='" + toDate + "' and D.IsOTEntitled=1 and isdisburse<>0";

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
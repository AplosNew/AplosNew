#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Payrolls;
using Library.Service.Helpers;
using Library.Service.Payrolls;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.HumanResource.Controllers
{
    public class PreallocatedOTController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public PreallocatedOTController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult Preallocatedotreport()
        {
            return View();
        }


        #endregion

        #region -- Operations


        [HttpGet]
        public ActionResult GetEmployeeBySectionAndWorkDate(string SectionId, DateTime workDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Active=CASE WHEN POT.EmpSystemID IS NOT NULL AND POT.WorkDate='" + workDate + @"' THEN 1 ELSE 0 END
                                        ,Emp.SystemID EmpSystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line, ATN.DayStatus, POT.PreallocatedOTHr, DT.Category,PR.SectionId,PR.DepartmentId
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
										LEFT JOIN dbo.EmployeeOTEntitle OT on OT.EmpSystemID=EMP.SystemId
										LEFT OUTER JOIN
										    (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId,dm.plantid
                                        ,dg.UserName GivenDesignationGroup,DM.IsOTEntitled
                                        from ( SELECT DC.SalaryRuleMasterId,dc.plantid,dm.*,DC.IsOTEntitled FROM MST.DesignationMaster DM
				                        		 LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                   ON DM.Id=DC.DesignationMasterId
                                                   )  dm
                                        LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
                                        ) egdsggso on egdsggso.DesignationId=EMP.GivenDesignationId and egdsggso.PlantId=e.PlantId

				                        LEFT JOIN (SELECT* FROM AttdnProcessData Where WorkDate='" + workDate + @"')ATN ON ATN.EmpSystemID=EMP.SystemId
                                        LEFT JOIN DayType DT ON DT.DayType=ATN.DayStatus 
                                        LEFT JOIN [dbo].[PreallocatedOT] POT ON POT.EmpSystemID=EMP.SystemId AND POT.WorkDate='" + workDate + @"'
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and (EMP.EmployeeStatus='Active' OR DOS>'" + workDate + @"') and PR.SectionId='" + SectionId + @"' 
				                        AND (egdsggso.IsOTEntitled=1 or OT.IsOTEntitle=1) ORDER BY EMP.EmployeeCodeNumeric";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private DataSet PlantWiseLock(string plantId, string workDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT FORMAT(LockedDate,'dd-MMM-yyyy') LockedDate FROM PlantWiseAttendanceLock where PlantId='" + plantId + "' And LockedDate='" + workDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<PreallocatedOT> entities,string WorkDate)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantLock = PlantWiseLock(identity.PlantId, WorkDate);
                if (plantLock.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Attendance is locked on " + plantLock.Tables[0].Rows[0]["LockedDate"] + "");
                }

                if (entities == null)
                {
                    throw new Exception("Select Employees.");
                }
                SaveData(entities);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }

        

        private void SaveData(IEnumerable<PreallocatedOT> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
               
                foreach (var item in data)
                {
                    if (item.PreallocatedOTHr == 0)
                    {
                        DeleteData(item.WorkDate, item.EmpSystemID);
                    }
                }

                foreach (var item in data)
                {
                    if (item.PreallocatedOTHr != 0)
                    {
                        string sql = "SELECT * FROM [dbo].[PreallocatedOT] WHERE EmpSystemID='" + item.EmpSystemID + "' AND WorkDate='" + item.WorkDate + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();


                            dr["EmpSystemID"] = item.EmpSystemID;
                            dr["WorkDate"] = item.WorkDate;
                            dr["ExtendTheDayLimit"] = item.ExtendTheDayLimit;
                            dr["PreallocatedOTHr"] = item.PreallocatedOTHr;
                            dr["GroupID"] = identity.CompanyGroupId;
                            dr["PlantID"] = identity.PlantId;
                            dr["GroupID"] = identity.CompanyGroupId;
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = DateTime.Now;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr["EmpSystemID"] = item.EmpSystemID;
                            dr["WorkDate"] = item.WorkDate;
                            dr["PreallocatedOTHr"] = item.PreallocatedOTHr;
                            dr["ExtendTheDayLimit"] = item.ExtendTheDayLimit;
                            dr["GroupID"] = identity.CompanyGroupId;
                            dr["PlantID"] = identity.PlantId;
                            dr["GroupID"] = identity.CompanyGroupId;

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = DateTime.Now;

                            dr.EndEdit();
                        }

                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }

            catch (Exception ex)
            {

                throw (ex);
            }
        }

        //public void DeleteData(DateTime workDate, string sectionId, string departmentId, string empSystemID)
        public void DeleteData(DateTime workDate, string empSystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                //strSQL = "DELETE FROM  [dbo].[PreallocatedOT] WHERE WorkDate='" + workDate + "' AND EmpSystemID IN (SELECT SystemID FROM EmployeeInformation WHERE SectionId='" + sectionId + "' AND DepartmentId='" + departmentId + "')";
                strSQL = "DELETE FROM  [dbo].[PreallocatedOT] WHERE WorkDate='" + workDate + "' AND EmpSystemID = '" + empSystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        #endregion

        #region OT Planning Matrix

        [HttpGet, Authorize]
        public ActionResult GetOTPlanningMatrixReport(ReportFormat reportFormat, string WorkDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = GetPreAllocatedReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, WorkDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "OT Planning Matrix";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        class ColHour
        {
            public int ColIndex { get; set; }
            public string ColValue { get; set; }
        }
        public IWorkbook GetPreAllocatedReport(string username, string plantId, string companyId, string companyGroupId, string plantName, string WorkDate)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();

            DataTable dtBioDvAC = null;
            DataTable dtTotalOt = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsBioDvAC = null;
            DataSet dsTotalPre = null;
            DataView dvPayDays, dvHr = null;
            // DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string FactoryAddress = string.Empty;
            string OTConsiderOn = string.Empty;
            #endregion

            try
            {
                //objStatic.GetPlantWiseHRMSSetting(companyGroupId, plantId, out dsLocalHRMSSetting);
                ReportUtility ru = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var totalOT = 0.00;
                var totalpot = 0.00;
                var totalcmpcou = 0.00;
                #region Validation


                #endregion Validation

                objRpt = new clsReport();

                dvPayDays = new DataView();


                //string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region DataSet

                GetPreallocatedOTReport(WorkDate, plantId, out dsBioDvAC);
                dtBioDvAC = dsBioDvAC.Tables[0];

                if (dtBioDvAC.Rows.Count == 0)
                {
                    throw new Exception("No Data found ...");
                }

                var dvMain = new DataView(dtBioDvAC);
                dvHr = new DataView(dtBioDvAC)
                {
                    Sort = "OTHR"
                };

                List<ColHour> listHours = new List<ColHour>();
                var dtHr = dvHr.ToTable(true, "OTHR");

                var dtMain = dvMain.ToTable(true, "Department", "Section", "SubSection");

                for (int i = 0; i < dtHr.Rows.Count; i++)
                {
                    ColHour colHour = new ColHour
                    {
                        ColIndex = i,
                        ColValue = dtHr.Rows[i]["OTHR"].ToString()
                    };
                    listHours.Add(colHour);
                }

                //objRpt.GetPreallocatedOTReportDepartmentSection(WorkDate, plantId, companyId, companyGroupId, out dsTotalPre);
                //dtTotalOt = dsTotalPre.Tables[0];

                #endregion DataSet

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
                objRpt.SelectedPlant(plantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                var iEmployeeCode = 0;
                var iDesignation = 0;

                var iEmployeeName = 0;
                var iSection = 0;
                var iSubSection = 0;
                var iDepartment = 0;
                var iAddedBy = 0;
                var iPreallocatedOTHr = 0;
                var isl = 0;
                var iPreallocated = 0;
                var iTotalCount = 0;
                var SLNo = 1;

                #region Individual OT

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 5;
                xlsCol = 1;
                #region ------------------Column Header------------------
                xlsRow++;

                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iSection = xlsCol;
                sheet1.Range[xlsRow, iSection].Text = "Section";
                sheet1.Range[xlsRow, iSection].ColumnWidth = 18;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet1.Range[xlsRow, iSubSection].Text = "SubSection";
                sheet1.Range[xlsRow, iSubSection].ColumnWidth = 18;

                sheet1.Range[xlsRow - 1, iSubSection + 1].Text = "OT Hours";
                sheet1.Range[xlsRow - 1, iSubSection + 1].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow - 1, iSubSection + 1, xlsRow - 1, iSubSection + 1 + listHours.Count].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, iSubSection + 1, xlsRow - 1, iSubSection + 1 + listHours.Count].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, iSubSection + 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, iSubSection + 1, xlsRow - 1, iSubSection + 1 + listHours.Count].Merge();
                //ot
                sheet1.Range[xlsRow, iSubSection + 1 + listHours.Count].Text = "Total OT Hour";
                sheet1.Range[xlsRow, iSubSection + 1 + listHours.Count].ColumnWidth = 15;
                //emp
                sheet1.Range[xlsRow, iSubSection + 2 + listHours.Count].Text = "Total Employees";
                sheet1.Range[xlsRow, iSubSection + 2 + listHours.Count].ColumnWidth = 15;

                xlsCol += 1;
                iPreallocatedOTHr = xlsCol;
                foreach (var item in listHours)
                {
                    sheet1.Range[xlsRow, item.ColIndex + iSubSection + 1].Text = item.ColValue;
                    sheet1.Range[xlsRow, item.ColIndex + iSubSection + 1].ColumnWidth = 7;
                }
                xlsCol += listHours.Count + 1;

                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                xlsRow++;
                endXlsCol = xlsCol;

                #endregion ------------------Column Header------------------

                if (dtBioDvAC.Rows.Count > 0)
                {
                    int _firstRow = 0;
                    for (int i = 0; i < dtMain.Rows.Count; i++)
                    {
                        #region ----------------------Data-----------------------

                        if (i == 0)
                        {
                            _firstRow = xlsRow;
                        }
                        sheet1.Range[xlsRow, iDepartment].Text = dtMain.Rows[i]["Department"].ToString();

                        sheet1.Range[xlsRow, iSection].Text = dtMain.Rows[i]["Section"].ToString();

                        sheet1.Range[xlsRow, iSubSection].Text = dtMain.Rows[i]["SubSection"].ToString();
                        double _mpower = 0;
                        double _ot = 0;
                        foreach (var item in listHours)
                        {
                            DataView dataView = new DataView(dtBioDvAC)
                            {
                                RowFilter = "Department='" + dtMain.Rows[i]["Department"].ToString() + "' AND Section='" + dtMain.Rows[i]["Section"].ToString() + "' AND SubSection='" + dtMain.Rows[i]["SubSection"].ToString() + "' AND OTHR='" + item.ColValue + "'"
                            };

                            if (dataView.Count > 0)
                            {
                                var mp = Convert.ToInt32(dataView[0]["EmpCount"].ToString());
                                sheet1.Range[xlsRow, iSubSection + 1 + item.ColIndex].Number = mp;
                                sheet1.Range[xlsRow, iSubSection + 1 + item.ColIndex].NumberFormat = "###0;";
                                _ot += mp * Convert.ToDouble(item.ColValue);
                                _mpower += mp;
                            }
                        }
                        //ot
                        sheet1.Range[xlsRow, iSubSection + 1 + listHours.Count].Number = _ot;
                        sheet1.Range[xlsRow, iSubSection + 1 + listHours.Count].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet1.Range[xlsRow, iSubSection + 1 + listHours.Count].CellStyle.Font.Bold = true;
                        //emp
                        sheet1.Range[xlsRow, iSubSection + 2 + listHours.Count].Number = _mpower;
                        sheet1.Range[xlsRow, iSubSection + 2 + listHours.Count].NumberFormat = "###0;";
                        sheet1.Range[xlsRow, iSubSection + 2 + listHours.Count].CellStyle.Font.Bold = true;
                        xlsRow++;
                        #endregion ----------------------Data-----------------------
                    }
                    int _lastRow = xlsRow - 1;
                    //ot
                    sheet1.Range[xlsRow, iSubSection].Text = "Total OT Hour:-";
                    sheet1.Range[xlsRow, iSubSection].CellStyle.Font.Bold = true;
                    //emp
                    sheet1.Range[xlsRow + 1, iSubSection].Text = "Total Employees:-";
                    sheet1.Range[xlsRow + 1, iSubSection].CellStyle.Font.Bold = true;
                    for (int i = 0; i < listHours.Count; i++)
                    {
                        var ob = listHours[i];
                        var col = ru.GetColumnNameForXls(ob.ColIndex + iSubSection + 1);
                        //GetColumnNameForXls
                        sheet1.Range[xlsRow, iSubSection + 1 + i].Formula = "=sum(" + col + _firstRow + ":" + col + _lastRow + ")*" + Convert.ToDouble(ob.ColValue) + "";
                        sheet1.Range[xlsRow, iSubSection + 1 + i].CellStyle.Font.Bold = true;
                        //emp
                        sheet1.Range[xlsRow + 1, iSubSection + 1 + i].Formula = "=sum(" + col + _firstRow + ":" + col + _lastRow + ")";
                        sheet1.Range[xlsRow + 1, iSubSection + 1 + i].CellStyle.Font.Bold = true;
                    }

                    //Grand OT
                    var col2 = ru.GetColumnNameForXls(listHours.Count + iSubSection + 1);
                    sheet1.Range[xlsRow, iSubSection + 1 + listHours.Count].Formula = "=sum(" + col2 + _firstRow + ":" + col2 + _lastRow + ")";
                    sheet1.Range[xlsRow, iSubSection + 1 + listHours.Count].CellStyle.Font.Bold = true;
                    //Grand emp
                    var col3 = ru.GetColumnNameForXls(listHours.Count + iSubSection + 2);
                    sheet1.Range[xlsRow + 1, iSubSection + 2 + listHours.Count].Formula = "=sum(" + col3 + _firstRow + ":" + col3 + _lastRow + ")";
                    sheet1.Range[xlsRow + 1, iSubSection + 2 + listHours.Count].CellStyle.Font.Bold = true;

                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                }


                #region ******************Report Header******************
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                    }


                }
                catch (Exception)
                {


                }
                xlsRow = 1;
                xlsCol = 1;

                FactoryName = string.Empty;

                //string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "OT Planning Matrix- " + WorkDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.IsGridLinesVisible = false;

                sheet1.Name = "OT Planning Matrix";


                #endregion Page Setup


                #endregion  ManualOutTime



                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetPreallocatedOTReport(string WorkDate, string plantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT 
                                DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                                ,count(emp.systemid) EmpCount,pot.PreallocatedOTHr OTHR	
                                FROM EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id	
                                LEFT JOIN [dbo].[PreallocatedOT] POT ON POT.EmpSystemID=EMP.SystemId 
                                WHERE emp.PlantID='" + plantId + @"' AND POT.WorkDate ='" + WorkDate + @"'	
                                group by pot.PreallocatedOTHr,DEPT.UserName ,S.UserName ,SS.UserName 
                                ORDER BY DEPT.UserName ,S.UserName ,SS.UserName";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        #endregion
    }

    public class PreallocatedOT : BaseModel
    {
        public string EmpSystemID { get; set; }
        public DateTime WorkDate { get; set; }
        public double PreallocatedOTHr { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public bool ExtendTheDayLimit { get; set; }
        public string SectionId { get; set; }
        public string DepartmentId { get; set; }

        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }

    }

}
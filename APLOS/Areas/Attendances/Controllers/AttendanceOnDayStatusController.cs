using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Library.ViewModel.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class AttendanceOnDayStatusController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMonthlyAttendanceInformation _monthlyAttendanceInformation;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;
        private object workbook;
        private object objRpt;
        private object excelEngine;
        private object application;

        public AttendanceOnDayStatusController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository,
            IMonthlyAttendanceInformation monthlyAttendanceInformation
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _monthlyAttendanceInformation = monthlyAttendanceInformation;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages


        #region -- Operations
        [Authorize]
        public ActionResult GetEmpInfo(string fromDate, string toDate,string DateStatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var DayTypeAndCatagory = "";
                if (DateStatus == "TotalPresent")
                {
                    //DayTypeAndCatagory = "AND  DT.DayType IN ('W','H','WP','HP','WA','HA')";
                    DayTypeAndCatagory = "AND  DT.Category in( 'Present','Late') ";
                }
                if (DateStatus == "Absent")
                {
                    DayTypeAndCatagory = "AND  DT.Category = 'Absent'";
                }
                if (DateStatus == "LatePresent")
                {
                    DayTypeAndCatagory = "AND  DT.Category = 'Late'";
                }
                if (DateStatus == "Leave")
                {
                    DayTypeAndCatagory = "AND  DT.Category = 'Leave'";
                }
                if (DateStatus == "OnTimePresent")
                {
                    DayTypeAndCatagory = "AND  DT.Category = 'Present'";
                }
                var cmdText = @"SELECT e.SystemId as EmpSystemId,DT.DayType,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
								    ,format( APD.WorkDate,'dd-MMM-yyyy') as WorkDate 
                                    ,ld.UserName LegalDesignation
									,Section.UserName as Section
									,Subsection.UserName as Subsection
									,Line.UserName as Line
									,edept.UserName as Department
									,LT.UserName LeaveType
                                    ,Division.UserName as Division
			                        ,format(APD.InTime,'dd-MMM-yyyy hh:mm tt')as InTime
									,format(APD.OutTime,'dd-MMM-yyyy hh:mm tt')as OutTime
                                    ,eu.UserName AS Unit
                                    ,EmpC.UserName as EmployeeCategory
                                        , ShiftOutTime= CASE                                   
                                   WHEN cs.OutTime IS NULL
                                   THEN CONVERT(varchar(15),CAST(S.OutTime AS TIME),100)
                                   ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                                   END
                                       , ShiftInTime= CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(S.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END

                                    FROM EmployeeInformation e
								    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT OUTER JOIN ORG.Department edept on edept.id=PO.DepartmentId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId
									LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
									LEFT JOIN [ORG].[Subsection] ON Subsection.Id = PO.SubsectionId
									LEFT JOIN [ORG].[Line] ON Line.Id = MPB.LineId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=PO.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=PO.SubDivisionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=EN.UnitId
									LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    

                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId                                
                                    LEFT OUTER JOIN HKP.EmployeeLocation ELoc on mpb.EmployeeLocationId=ELoc.Id
			                        LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
									LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
									LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    --LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                    --LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
			                        LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId                                    
									LEFT OUTER JOIN LeaveType LT ON LT.Id = APD.LTSystemID
									LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus

                                    LEFT JOIN ShiftDefination s on s.SystemID=APD.ShiftSystemID  
									LEFT JOIN EmpDateWiseShiftAssign ES on es.EmpSystemID = E.SystemId AND APD.WorkDate = ES.WorkDate
									LEFT JOIN(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime ,m.BreakStratTime,m.BreakEndTime
								 FROM[ShiftTimeChgMaster] m
                                left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = es.ShiftSystemID and cs.ShiftDate = APD.WorkDate

                                    WHERE 
									  E.GroupID='" + identity.CompanyGroupId+@"' 
									 AND E.PlantId='"+identity.PlantId+ @"' 
									 " + DayTypeAndCatagory + @"
									 AND CONVERT(DATE, APD.WorkDate) 
									 between CONVERT(DATE,'" + fromDate+@"') 
									 and CONVERT(DATE,'"+toDate+@"') 
									ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ASC ";
                JsonResult json = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost,Authorize]
        public ActionResult XlsAttendanceOnDayStatusReport(string fromDate, string toDate, string DateStatus,string Description, string[] employeeId)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsAttendanceOnDayStatus = null;
            DataTable dtAttendanceOnDayStatus = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            #endregion

            try
            {

                string EmpIdLoop = "";
                foreach (string item in employeeId)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "'" + item + "'"; ;
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item + "'";
                    }
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();                
                objRpt.GetAttendanceOnDayStatus(fromDate, toDate, DateStatus, EmpIdLoop, out dsAttendanceOnDayStatus);
                dtAttendanceOnDayStatus = dsAttendanceOnDayStatus.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                string companyId = identity.CompanyId;

                var iName = 0;
                var iEmployeeCode = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iDOJ = 0;
                var iDOS = 0;
                var iLine = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var iInTime = 0;
                var iOutTime = 0;
                var iLeaveType = 0;
                var iWorkDate = 0;
                var iDayType = 0;
                var iShiftInTime = 0;
                var iShiftOutTime = 0;
                var iRemarks = 0;
                var iShiftName = 0;

                var isl = 0;
                var SLNo = 1;

                if (dsAttendanceOnDayStatus.Tables[0].Rows.Count == 0)
                {
                    throw new CustomException("No Data Found....");
                }
                #region Hourly Ot

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 5;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "Work Date";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 10;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Employee Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;
                
                //xlsCol += 1;
                //iDayType = xlsCol;
                //sheet1.Range[xlsRow, iDayType].Text = "Day Type";
                //sheet1.Range[xlsRow, iDayType].ColumnWidth = 10;
                
                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Employee Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 25;

          
                //xlsCol += 1;
                //iDepartment = xlsCol;
                //sheet1.Range[xlsRow, iDepartment].Text = "Department";
                //sheet1.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 25;

                //xlsCol += 1;
                //iSection = xlsCol;
                //sheet1.Range[xlsRow, iSection].Text = "Section";
                //sheet1.Range[xlsRow, iSection].ColumnWidth = 14;

                //xlsCol += 1;
                //iSubSection = xlsCol;
                //sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                //sheet1.Range[xlsRow, iSubSection].ColumnWidth = 16;

                //xlsCol += 1;
                //iLine = xlsCol;
                //sheet1.Range[xlsRow, iLine].Text = "Line";
                //sheet1.Range[xlsRow, iLine].ColumnWidth = 12;

                //xlsCol += 1;
                //iDOJ = xlsCol;
                //sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                //sheet1.Range[xlsRow, iDOJ].ColumnWidth = 12;

                //xlsCol += 1;
                //iDOS = xlsCol;
                //sheet1.Range[xlsRow, iDOS].Text = "DOS";
                //sheet1.Range[xlsRow, iDOS].ColumnWidth = 12;

                if (DateStatus == "TotalPresent" || DateStatus == "OnTimePresent" || DateStatus == "LatePresent")
                {
                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet1.Range[xlsRow, iShiftInTime].Text = "Shift InTime";
                    sheet1.Range[xlsRow, iShiftInTime].ColumnWidth = 15;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet1.Range[xlsRow, iShiftOutTime].Text = "Shift OutTime";
                    sheet1.Range[xlsRow, iShiftOutTime].ColumnWidth = 15;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet1.Range[xlsRow, iInTime].Text = "InTime";
                    sheet1.Range[xlsRow, iInTime].ColumnWidth = 20;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet1.Range[xlsRow, iOutTime].Text = "OutTime";
                    sheet1.Range[xlsRow, iOutTime].ColumnWidth = 20;

                    xlsCol += 1;
                    iRemarks = xlsCol;
                    sheet1.Range[xlsRow, iRemarks].Text = "Remarks";
                    sheet1.Range[xlsRow, iRemarks].ColumnWidth = 20;

                }

                if (DateStatus == "Leave")
                {
                    xlsCol += 1;
                    iLeaveType = xlsCol;
                    sheet1.Range[xlsRow, iLeaveType].Text = "LeaveType";
                    sheet1.Range[xlsRow, iLeaveType].ColumnWidth = 17;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet1.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet1.Range[xlsRow, iShiftName].ColumnWidth = 17;
                }

                if (DateStatus == "Absent")
                {
                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet1.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet1.Range[xlsRow, iShiftName].ColumnWidth = 17;
                }

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;                
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                xlsRow++;

                #endregion ------------------Column Header------------------

                for (int i = 0; i < dtAttendanceOnDayStatus.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------   
                    
                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, iName].Text = dtAttendanceOnDayStatus.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtAttendanceOnDayStatus.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iDesignation].Text = dtAttendanceOnDayStatus.Rows[i]["LegalDesignation"].ToString();

                    //sheet1.Range[xlsRow, iDepartment].Text = dtAttendanceOnDayStatus.Rows[i]["Department"].ToString();
                    //sheet1.Range[xlsRow, iSection].Text = dtAttendanceOnDayStatus.Rows[i]["Section"].ToString();
                    //sheet1.Range[xlsRow, iSubSection].Text = dtAttendanceOnDayStatus.Rows[i]["SubSection"].ToString();
                    //sheet1.Range[xlsRow, iLine].Text = dtAttendanceOnDayStatus.Rows[i]["Line"].ToString();
                    //sheet1.Range[xlsRow, iDOJ].Text = dtAttendanceOnDayStatus.Rows[i]["DOJ"].ToString();
                    //sheet1.Range[xlsRow, iDOS].Text = dtAttendanceOnDayStatus.Rows[i]["DOS"].ToString();
                    //sheet1.Range[xlsRow, iDayType].Text = dtAttendanceOnDayStatus.Rows[i]["DayType"].ToString();

                    //sheet1.Range[xlsRow, iRemarks].Text = dtAttendanceOnDayStatus.Rows[i][""].ToString();
                    sheet1.Range[xlsRow, iWorkDate].Text = dtAttendanceOnDayStatus.Rows[i]["WorkDate"].ToString();

                    if (DateStatus == "TotalPresent" || DateStatus == "OnTimePresent" || DateStatus == "LatePresent")
                    {
                        sheet1.Range[xlsRow, iInTime].Text = dtAttendanceOnDayStatus.Rows[i]["InTime"].ToString();
                        sheet1.Range[xlsRow, iOutTime].Text = dtAttendanceOnDayStatus.Rows[i]["OutTime"].ToString();
                        sheet1.Range[xlsRow, iShiftInTime].Text = dtAttendanceOnDayStatus.Rows[i]["ShiftInTime"].ToString();
                        sheet1.Range[xlsRow, iShiftOutTime].Text = dtAttendanceOnDayStatus.Rows[i]["ShiftOutTime"].ToString();
                    }
            

                    if (DateStatus == "Leave")
                    {
                        sheet1.Range[xlsRow, iLeaveType].Text = dtAttendanceOnDayStatus.Rows[i]["LeaveType"].ToString();
                        sheet1.Range[xlsRow, iShiftName].Text = dtAttendanceOnDayStatus.Rows[i]["ShiftDefinationName"].ToString();
                    }

                    if (DateStatus == "Absent")
                    {
                        sheet1.Range[xlsRow, iShiftName].Text = dtAttendanceOnDayStatus.Rows[i]["ShiftDefinationName"].ToString();
                    }

                    xlsRow++;
                    SLNo++;
                }
                
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes
                
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

                string FactoryAddress = string.Empty;

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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 16;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
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
                if (DateStatus == "TotalPresent")
                {
                    sheet1.Range[xlsRow, 3].Text = "Total Present Report - " + Description;
                }
                if (DateStatus == "Absent")
                {
                    sheet1.Range[xlsRow, 3].Text = "Absent Report - " + Description;
                }
                if (DateStatus == "LatePresent")
                {
                    sheet1.Range[xlsRow, 3].Text = "Late Present - " + Description;
                }
                if (DateStatus == "Leave")
                {
                    sheet1.Range[xlsRow, 3].Text = "Leave - " + Description;
                }
                if (DateStatus == "OnTimePresent")
                {
                    sheet1.Range[xlsRow, 3].Text = "On Time Present - " + Description;
                }

                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************
                
                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Attendance On Daily Status";
                #endregion Page Setup
                
                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = "AttendanceOnDailyStatus.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw ex;
            }
        }
        
        #endregion -- Operations  
    }
}
#endregion  
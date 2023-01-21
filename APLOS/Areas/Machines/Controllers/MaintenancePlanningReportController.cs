using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Aplos.Controllers;
using Library.Service.Employees;
using Library.Model.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Syncfusion.DocIO.DLS;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Library.HumanResource.NewAttendanceProcess;
using Library.Data;

namespace Aplos.Areas.Machines.Controllers
{
    public class MaintenancePlanningReportController : Controller
    {
        #region Constructor


        private readonly IAttendanceManagementService _AttendanceManagementService;
        ResudeceStatusReportService rsr = new ResudeceStatusReportService();
        private readonly ISqlRepository _sqlRepository;

        public MaintenancePlanningReportController(IAttendanceManagementService AttendanceManagementService, ISqlRepository R)
        {
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetFromDateList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Top 1
 (Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id ASC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id ASC)),'dd-MMM-yyyy')end) FromDate
 from TRN.Maintenancescheduling MS
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id
 left Join  [TRN].[MachineAssetPlannedDetails] MPD ON MPD.AssetId=MMA.Id
 Order By MPD.Id ASC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMaintenancePlanningReportList(string ToDate,string FromDate,string Status)
        {
            string Filter = string.Empty;

            if (Status == "All")
            {
                Filter = " and (MPD.ActualDate is not null or MPD.ActualDate is null) and MPD.PlannedDate is not null";
            }
            else if (Status == "Completed")
            {
                Filter = " and MPD.ActualDate is not null and MPD.PlannedDate is not null order by MPD.ActualDate desc";
            }
            else
            {
                Filter = " and MPD.ActualDate is null and MPD.PlannedDate is not null order by MPD.PlannedDate";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select E.UserName Entity,D.UserName Department,P.UserName as Process,WC.UserName WorkCenter,WC.Code WCCode,MA.AssetName,MA.AssetCode,MA.AssetReference,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.UserName ScheduleName,MS.ScheduleCode,Reverse(stuff(Reverse((select EmployeeName+',' from EmployeeInformation where SystemId in (select ResponsiblePersonId from TRN.ResponsiblePlannedDetails AP where AP.PlannedId=MPD.Id and AP.IsActive=1) for xml path(''))),1,1,'')) ActionablePerson,
MS.ScheduleDays,Format(MPD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,isnull(Format(MPD.ActualDate,'dd-MMM-yyyy'),'') as ActualDate,DATEDIFF(Day,MPD.PlannedDate,MPD.ActualDate) DaysDifference,
Case when isnull(MPD.ActualDate,'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+MPD.ActualDate),'dd-MMM-yyyy') end CMD,MPD.AddedBy PlannedBy,MPD.UpdatedBy CompletedBy,MPD.Remarks
 from TRN.Maintenancescheduling MS
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 left join HKP.Process P ON WC.ProcessId=P.Id
 left Join ORG.Department D ON D.Id=MS.DepartmentId
 left join TRN.MachineAssetPlannedDetails MPD ON MPD.AssetId=MMA.Id
where
MS.IsActive=1 and MMA.Id is not null and
Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MPD.ActualDate) end between '" + FromDate+"' and '"+ ToDate + "' " + Filter + @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult XlsMaintenancePlanningReport(string todate, string fromDate, string Status)
        {
            try
            {
                var workbook = MaintenancePlanningReport(todate,fromDate,Status);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "MaintenancePlanningReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [Authorize, HttpPost]
        private IWorkbook MaintenancePlanningReport(string todate, string fromDate, string Status)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = rsr.MaintenancePlanningReport(todate, fromDate, Status);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Maintenance Planning Report";

            

            int ROW = 1;
            int endCol = 1;
            int COL = 1;
            int COLHeader = 0;

            report.SetHeaderText(ref sheet, ROW, COLHeader+6, "Maintenance Planning Report :", 15, ExcelHAlign.HAlignCenter);
            sheet.Range[ROW, COLHeader + 6, ROW, COLHeader + 7].Merge();
            ROW++;
           
            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Process", 12, ExcelHAlign.HAlignCenter);
            int ColProcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Work Center", 12, ExcelHAlign.HAlignCenter);
            int ColWorkCenter = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "WorkCenter Code", 12, ExcelHAlign.HAlignCenter);
            int ColWCCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Asset Name", 12, ExcelHAlign.HAlignCenter);
            int ColAssetName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Asset Code", 12, ExcelHAlign.HAlignCenter);
            int ColAssetCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Make", 15, ExcelHAlign.HAlignCenter);
            int ColMake = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Model", 15, ExcelHAlign.HAlignCenter);
            int ColModel = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Schedule Name", 12, ExcelHAlign.HAlignCenter);
            int ColScheduleName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Schedule Code", 12, ExcelHAlign.HAlignCenter);
            int ColScheduleCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Planned Date", 12, ExcelHAlign.HAlignCenter);
            int ColPlannedDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Last Maintenance Date", 12, ExcelHAlign.HAlignCenter);
            int ColLMD = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Current Maintenance Date", 12, ExcelHAlign.HAlignCenter);
            int ColCMD = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 12, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            
            ROW++;
            endCol = COL;
            #endregion Headers

            //string MaintenanceEntity = "";
            //string MaintenanceScheduleName = "";

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            //int MaintenanceEntityRow = 0;
            //int MaintenanceScheduleNameRow = 0;


            //double[] arr = new double[4];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                //if (MaintenanceEntity != data.Rows[i]["Entity"].ToString())
                //{
                //    MaintenanceEntity = data.Rows[i]["Entity"].ToString();

                //    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();

                //    if (i != 0 && MaintenanceEntityRow != (ROW - 1))
                //    {
                //        sheet.Range[MaintenanceEntityRow, ColEntity, ROW - 1, ColEntity].Merge();
                //        sheet.Range[MaintenanceEntityRow, ColEntity, ROW - 1, ColEntity].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    }
                //    MaintenanceEntityRow = ROW;
                //}

                //if (MaintenanceScheduleName != data.Rows[i]["ScheduleName"].ToString())
                //{
                //    MaintenanceScheduleName = data.Rows[i]["ScheduleName"].ToString();
                //    sheet[ROW, ColScheduleName].Text = data.Rows[i]["ScheduleName"].ToString();

                //    if (i != 0 && MaintenanceScheduleNameRow != (ROW - 1))
                //    {
                //        sheet.Range[MaintenanceScheduleNameRow, ColScheduleName, ROW - 1, ColScheduleName].Merge();
                //        sheet.Range[MaintenanceScheduleNameRow, ColScheduleName, ROW - 1, ColScheduleName].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    }
                //    MaintenanceScheduleNameRow = ROW;
                //}

                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();
                sheet[ROW, ColWCCode].Text = data.Rows[i]["WCCode"].ToString();
                sheet[ROW, ColAssetName].Text = data.Rows[i]["AssetName"].ToString();
                sheet[ROW, ColAssetCode].Text = data.Rows[i]["AssetCode"].ToString();
                sheet[ROW, ColMake].Text = data.Rows[i]["Make"].ToString();
                sheet[ROW, ColModel].Text = data.Rows[i]["Model"].ToString();
                sheet[ROW, ColScheduleName].Text = data.Rows[i]["ScheduleName"].ToString();
                sheet[ROW, ColScheduleCode].Text = data.Rows[i]["ScheduleCode"].ToString();
                sheet[ROW, ColPlannedDate].Text = data.Rows[i]["PlannedDate"].ToString();
                sheet[ROW, ColLMD].Text = data.Rows[i]["LMD"].ToString();
                sheet[ROW, ColCMD].Text = data.Rows[i]["CMD"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();

                ROW++;

            }

            ROW++;


            sheet.Range[ROW, ColProcess, ROW, endCol].CellStyle.Font.Bold = true;
            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, startRow, endCol];

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion -- Operations
    }
}
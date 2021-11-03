#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Helpers;
using Library.Service.IssueTracker;
using Library.Service.TaskManagement;
using Syncfusion.XlsIO;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IssueTracker.Controllers
{
    public class IssueGroupController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public IssueGroupController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

  
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations


        [HttpGet, Authorize]
        public ActionResult GetIssueGroups()
        {
            string sql = @"SELECT DISTINCT IG.*, E.EmployeeName AS ResponsiblePerson FROM [dbo].[IssueGroup] IG 
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId = IG.ResponsiblePersonId
							LEFT JOIN IssueTransaction IST ON IST.IssueGroupId = IG.Id
							WHERE IST.IssueGroupId IS NOT NULL";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetIssueGroupReport(ReportFormat reportFormat, string issueGroupId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "ExpensesBooking " + issueGroupId + "";
            var workbook = GetIssueGroupReportData(issueGroupId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        //private DataTable GetIssueTransactionReportDataByIssueGroupId(string issueGroupId)
        //{
        //            var sql = @"select ig.Name AS IssueGraoup,CONVERT(bit,0) IsCheck, e.EmployeeName AS ResponsiblePerson, ist.*,  tc.UserName as TaskCategory, tsc.UserName TaskSubCategory
        //            ,imc.UserName IssueImportance, Uaudit.EmployeeName AS UpdateAudit,Iaudit.EmployeeName AS InternalAudit,Eaudit.EmployeeName AS ExternalAudit, Faudit.EmployeeName as FollowUpAudit,aBy.EmployeeName as AssignBy, aTo.EmployeeName as AssignTo
        //            from IssueTransaction ist
        //            left outer join hkp.TaskCategory AS tc ON tc.Id = ist.TaskCategoryId
        //            left outer join hkp.TaskSubCategory AS tsc ON tsc.Id = ist.TaskSubCategoryId
        //            left outer join IssueImportance AS imc ON imc.Id = ist.IssueImportanceId
        //            left outer join IssueGroup AS ig ON ig.Id = ist.IssueGroupId
        //            left outer join EmployeeInformation AS e ON e.SystemId = ig.ResponsiblePersonId
        //            left outer join EmployeeInformation AS Uaudit ON Uaudit.SystemId = ist.UpdateResponsiblePersonId
        //            left outer join EmployeeInformation AS Iaudit ON Iaudit.SystemId = ist.InternalResponsiblePersonId
        //            left outer join EmployeeInformation AS Eaudit ON Eaudit.SystemId = ist.ExternalResponsiblePersonId
        //            left outer join EmployeeInformation AS Faudit ON Faudit.SystemId = ist.FollowUpResponsiblePersonId
        //            left outer join EmployeeInformation AS aBy ON aBy.SystemId = ist.AssignById
        //            left outer join EmployeeInformation AS aTo ON aTo.SystemId = ist.AssignToId
        //            where IssueGroupId = '" + issueGroupId + "' ";
        //    return _sqlRepository.GetDataTable(sql);
        //}


        private DataTable GetIssueTransactionReportDataByIssueGroupId(string issueGroupId)
        {
            var sql = @"select UT.UDueDate, IT.IDueDate, ET.EDueDate,FT.FDueDate , ig.Name AS IssueGraoup,CONVERT(bit,0) IsCheck, e.EmployeeName AS ResponsiblePerson, ist.*,  tc.UserName as TaskCategory, tsc.UserName TaskSubCategory
                    ,imc.UserName IssueImportance, Uaudit.EmployeeName AS UpdateAudit,Iaudit.EmployeeName AS InternalAudit,Eaudit.EmployeeName AS ExternalAudit
					,Faudit.EmployeeName as FollowUpAudit,aBy.EmployeeName as AssignBy, aTo.EmployeeName as AssignTo
                    from IssueTransaction ist
                    left outer join hkp.TaskCategory AS tc ON tc.Id = ist.TaskCategoryId
                    left outer join hkp.TaskSubCategory AS tsc ON tsc.Id = ist.TaskSubCategoryId
                    left outer join IssueImportance AS imc ON imc.Id = ist.IssueImportanceId
                    left outer join IssueGroup AS ig ON ig.Id = ist.IssueGroupId
                    left outer join EmployeeInformation AS e ON e.SystemId = ig.ResponsiblePersonId
                    left outer join EmployeeInformation AS Uaudit ON Uaudit.SystemId = ist.UpdateResponsiblePersonId
                    left outer join EmployeeInformation AS Iaudit ON Iaudit.SystemId = ist.InternalResponsiblePersonId
                    left outer join EmployeeInformation AS Eaudit ON Eaudit.SystemId = ist.ExternalResponsiblePersonId
                    left outer join EmployeeInformation AS Faudit ON Faudit.SystemId = ist.FollowUpResponsiblePersonId
                    left outer join EmployeeInformation AS aBy ON aBy.SystemId = ist.AssignById
                    left outer join EmployeeInformation AS aTo ON aTo.SystemId = ist.AssignToId

					left outer join (select TM.IssueTransactionId,max(ta.DueDate) AS UDueDate from TaskManagerMaster TM
					inner join TaskAudit TA on tm.id=ta.TaskManagerMasterId 
					where TaskType='UpdateAudit' and TA.AuthorizationType='AssignTo'
					group by TM.IssueTransactionId) UT ON ist.Id = UT.IssueTransactionId

					left outer join (select TM.IssueTransactionId,max(ta.DueDate) AS IDueDate from TaskManagerMaster TM
					inner join TaskAudit TA on tm.id=ta.TaskManagerMasterId 
					where TaskType='InternalAudit' and TA.AuthorizationType='AssignTo'
					group by TM.IssueTransactionId) IT ON ist.Id = IT.IssueTransactionId

					left outer join (select TM.IssueTransactionId,max(ta.DueDate) AS EDueDate from TaskManagerMaster TM
					inner join TaskAudit TA on tm.id=ta.TaskManagerMasterId 
					where TaskType='ExternalAudit' and TA.AuthorizationType='AssignTo'
					group by TM.IssueTransactionId) ET ON ist.Id = ET.IssueTransactionId

					left outer join (select TM.IssueTransactionId,max(ta.DueDate) AS FDueDate from TaskManagerMaster TM
					inner join TaskAudit TA on tm.id=ta.TaskManagerMasterId 
					where TaskType='FollowUpAudit' and TA.AuthorizationType='AssignTo'
					group by TM.IssueTransactionId) FT ON ist.Id = FT.IssueTransactionId
                    where IssueGroupId = '" + issueGroupId + "' ";
            return _sqlRepository.GetDataTable(sql);
        }

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;
            //sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }
        private void SetHeaderText(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Thin);
        }

        public IWorkbook GetIssueGroupReportData(string issueGroupId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Requisition";



            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            int IgNameCol = 1;
            int IgResponsiblePersonCol = 1;
            int IgNameRow = ROW;
            ROW++;
            int IgResponsiblePersonRow = ROW;
            int updateAuditDueDate = ROW;
            int internalAuditDueDate = ROW;
            int externalAuditDueDate = ROW;
            int followUpAuditDueDate = ROW;
            ROW++;


            DataTable data = GetIssueTransactionReportDataByIssueGroupId(issueGroupId);


            report.SetHeaderText(ref sheet, ROW, COL, "Issue", 20, ExcelHAlign.HAlignLeft);
            int ColIssue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Issue Detail", 20, ExcelHAlign.HAlignLeft);
            int ColIssueDetail = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Issue Type", 10, ExcelHAlign.HAlignLeft);
            int ColIssueType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Issue Category", 14, ExcelHAlign.HAlignLeft);
            int ColIssueCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Issue sub Category", 18, ExcelHAlign.HAlignLeft);
            int ColIssueSubCategory = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Issue Buyer", 20, ExcelHAlign.HAlignLeft);
            //int ColIssueBuyer = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Issue Importance", 16, ExcelHAlign.HAlignLeft);
            int ColIssueImportance = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Final Status", 11, ExcelHAlign.HAlignLeft);
            int ColFinalStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Issue Date", 10, ExcelHAlign.HAlignLeft);
            int ColIssueCreationDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Due Date", 10, ExcelHAlign.HAlignLeft);
            int ColDueDate = COL;
            COL++;



            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 20, ExcelHAlign.HAlignLeft);
            int ColUpdateAudit = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Due Date", 20, ExcelHAlign.HAlignLeft);
            int ColUDueDate = COL;

            sheet[ROW - 1, COL - 1].Text = "Update Audit";
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            COL++;

           
            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 20, ExcelHAlign.HAlignLeft);
            int ColInternalAudit = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Due Date", 20, ExcelHAlign.HAlignLeft);
            int ColIDueDate = COL;


            sheet[ROW - 1, COL - 1].Text = "Internal Audit";
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 20, ExcelHAlign.HAlignLeft);
            int ColExternalAudit = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Due Date", 20, ExcelHAlign.HAlignLeft);

            sheet[ROW - 1, COL - 1].Text = "External Audit";
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            int ColEDueDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 20, ExcelHAlign.HAlignLeft);
            int ColFollowUpAudit = COL;
            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Due Date", 20, ExcelHAlign.HAlignLeft);
            int ColFDueDate = COL;

            sheet[ROW - 1, COL - 1].Text = "Followup Audit";
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].Merge();
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW - 1, COL - 1, ROW - 1, COL].CellStyle.Font.Color = ExcelKnownColors.White;

            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Created By", 20, ExcelHAlign.HAlignLeft);
            int ColCreatedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Assign To", 20, ExcelHAlign.HAlignLeft);
            int ColAssignTo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 20, ExcelHAlign.HAlignLeft);
            int ColRemarks = COL;

            endCol = COL;

            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9;
            //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_blue;
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

            ROW++;
            int startRow = ROW;

            if (data.Rows.Count > 0)
            {
                SetHeaderTextTop(ref sheet, IgNameRow, IgNameCol, "Issue Group :", 20, ExcelHAlign.HAlignLeft);
                IgNameCol++;
                sheet[IgNameRow, IgNameCol].Text = data.Rows[0]["IssueGraoup"].ToString();
                IgNameRow++;

                SetHeaderTextTop(ref sheet, IgResponsiblePersonRow, IgResponsiblePersonCol, "ResponsiblePerson :", 20, ExcelHAlign.HAlignLeft);
                IgResponsiblePersonCol++;
                sheet[IgResponsiblePersonRow, IgResponsiblePersonCol].Text = data.Rows[0]["ResponsiblePerson"].ToString();
                IgResponsiblePersonRow++;
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                try
                {
                    //var IssueTransactionId = data.Rows[i]["IssueTransactionId"].ToString();

                    sheet[ROW, ColIssueType].Text = data.Rows[i]["IssueType"].ToString();
                    sheet[ROW, ColIssue].Text = data.Rows[i]["Issue"].ToString();
                    sheet[ROW, ColIssueDetail].Text = data.Rows[i]["IssueDetail"].ToString();
                    sheet[ROW, ColIssueCategory].Text = data.Rows[i]["TaskCategory"].ToString();
                    sheet[ROW, ColIssueSubCategory].Text = data.Rows[i]["TaskSubCategory"].ToString();
                    //sheet[ROW, ColIssueBuyer].Text = data.Rows[i]["Buyer"].ToString();
                    sheet[ROW, ColIssueImportance].Text = data.Rows[i]["IssueImportance"].ToString();
                    sheet[ROW, ColFinalStatus].Text = data.Rows[i]["FinalStatus"].ToString();
                    sheet[ROW, ColIssueCreationDate].Text = Convert.ToDateTime(data.Rows[i]["IssueDate"].ToString()).ToString("dd-MMM-yyyy");
                    sheet[ROW, ColDueDate].Text = Convert.ToDateTime(data.Rows[i]["RequiredDate"].ToString()).ToString("dd-MMM-yyyy");

                    var uAuditDueDate = string.IsNullOrEmpty(data.Rows[i]["UDueDate"].ToString()) ? "" : Convert.ToDateTime(data.Rows[i]["UDueDate"].ToString()).ToString("dd-MMM-yyyy");
                    sheet[ROW, ColUDueDate].Text = uAuditDueDate;
                    var iAuditDueDate = string.IsNullOrEmpty(data.Rows[i]["IDueDate"].ToString()) ? "" : Convert.ToDateTime(data.Rows[i]["IDueDate"].ToString()).ToString("dd-MMM-yyyy");
                    sheet[ROW, ColIDueDate].Text = iAuditDueDate;
                    var eAuditDueDate = string.IsNullOrEmpty(data.Rows[i]["EDueDate"].ToString()) ? "" : Convert.ToDateTime(data.Rows[i]["EDueDate"].ToString()).ToString("dd-MMM-yyyy");
                    sheet[ROW, ColEDueDate].Text = eAuditDueDate;
                    var fAuditDueDate = string.IsNullOrEmpty(data.Rows[i]["FDueDate"].ToString()) ? "" : Convert.ToDateTime(data.Rows[i]["FDueDate"].ToString()).ToString("dd-MMM-yyyy");
                    sheet[ROW, ColFDueDate].Text = fAuditDueDate;

                    //sheet[ROW, ColUDueDate].Text = Convert.ToDateTime(data.Rows[i]["UDueDate"].ToString()).ToString("dd-MMM-yyyy");
                    //sheet[ROW, ColIDueDate].Text = Convert.ToDateTime(data.Rows[i]["IDueDate"].ToString()).ToString("dd-MMM-yyyy");
                    //sheet[ROW, ColEDueDate].Text = Convert.ToDateTime(data.Rows[i]["EDueDate"].ToString()).ToString("dd-MMM-yyyy");
                    //sheet[ROW, ColFDueDate].Text = Convert.ToDateTime(data.Rows[i]["FDueDate"].ToString()).ToString("dd-MMM-yyyy");


                    sheet[ROW, ColUpdateAudit].Text = data.Rows[i]["UpdateAudit"].ToString();
                    sheet[ROW, ColInternalAudit].Text = data.Rows[i]["InternalAudit"].ToString();
                    sheet[ROW, ColExternalAudit].Text = data.Rows[i]["ExternalAudit"].ToString();
                    sheet[ROW, ColFollowUpAudit].Text = data.Rows[i]["FollowUpAudit"].ToString();

                    sheet[ROW, ColCreatedBy].Text = data.Rows[i]["AssignBy"].ToString();
                    sheet[ROW, ColAssignTo].Text = data.Rows[i]["AssignTo"].ToString();

                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();

                    //sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                    //sheet[ROW, ColAmount].Text = data.Rows[i]["Amount"].ToString();
                    //sheet[ROW, ColAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    ROW++;
                }
                catch (Exception ex)
                {

                    throw new Exception(ex.Message);
                }
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, endCol, "Issue Recap", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion -- Operations
    }
}
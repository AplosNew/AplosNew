#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.MeetingManagement.Controllers
{
    public class MeetingReportsController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public MeetingReportsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        
        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ReportView()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            try
            {
                var sql = @"SELECT * FROM (select MT.Id MeetingTypeId,MIH.Id MeetingId,MT.UserName MeetingType,MIH.IssueStatus,MIH.IssueCritically Criticality,D.Id DepartmentId,D.UserName Department
							,EI.SystemId CreatedById,EI.EmployeeName CreatedBy,MIH.ItemTitle,MIH.IssueCritically Critically,ActionApplicable=case when MIH.ActionApplicable=1 then 'Yes' else 'No' End 
			                ,DecisionApplicable=case when MIH.DecisionApplicable=1 then 'Yes' else 'No' End,MIH.IssueStatus [Status],EI.SystemId ByWhomId,EI.EmployeeName ByWhom
							,format((MIH.AddedDate),'dd-MMM-yyyy') TargetFromDate,format((MIH.AddedDate),'dd-MMM-yyyy') TargetToDate
							,MA.MeetingName,format((MA.Date),'dd-MMM-yyyy') MeetingDate,EINFO.EmployeeName ChairedBy,EINF.EmployeeName OrganizedBy
							,format((MIH.AddedDate),'dd-MMM-yyyy') TargetDate,MTP.Id TalkingPointId,MTP.TalkingPoint,MS.Id SuggestionId,MS.Suggestion
							,MAP.Id ActionToBeTakenId,MAP.ActionToBeTaken ActionalPoint,MD.Id DecisionId,MD.Decision,MIH.Remarks
                            from MeetingItemHeader MIH
                            left join EmployeeInformation EI on EI.SystemId=MIH.ByWhomId
							left join MeetingAgendaItem MAI on MAI.MeetingItemHeaderId=MIH.Id
							left join MeetingAgenda MA on MA.Id=MAI. MeetingAgendaId
							left join EmployeeInformation EINF on EINF.SystemId=MA.MeetingOrganizedById
							left join EmployeeInformation EINFO on EINFO.SystemId=MA.ChairedById
							left join ORG.Department D on D.Id=MIH.DepartmentId
                            left join MeetingType MT on MT.Id=MIH.MeetingTypeId
							left join MeetingTalkingPoint MTP on MTP.MeetingItemHeaderId=MIH.Id
							left join MeetingSuggestion MS on MS.MeetingItemHeaderId=MIH.Id
							left join MeetingActionablePoints MAP on MAP.MeetingItemHeaderId=MIH.Id
							left join MeetingDecision MD on MD.MeetingItemHeaderId=MIH.Id) AS KK	";
               
                //return _sqlRepository.GetDataCollection(sql);
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetMeetingReport(Dictionary<string, string> parameters)
        {
            try
            {
                string fileName = "";
                fileName = MeetingReport(parameters, "MeetingReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        
        public string MeetingReport(Dictionary<string, string> parameters, string SheetName)
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
                workbook.Worksheets[0].Name = "MeetingReports";
                sheet = workbook.Worksheets[0];
                DataTable data;
                MeetingReportSQL(parameters, out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Meeting Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMeetingId = COL;
                COL++;

                sheet[ROW, COL].Text = "Meeting Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMeetingDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Meeting Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMeetingName = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDepartment = COL;
                COL++;
                sheet[ROW, COL].Text = "Created By";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCreatedBy = COL;
                COL++;
                sheet[ROW, COL].Text = "Meeting Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMeetingType = COL;
                COL++;
                sheet[ROW, COL].Text = "Item Title";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColItemTitle = COL;
                COL++;
                sheet[ROW, COL].Text = "Critically";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCritically = COL;
                COL++;
                sheet[ROW, COL].Text = "Action Applicable";
                sheet[ROW, COL].ColumnWidth = 22;
                int ColActionApplicable = COL;
                COL++;
                sheet[ROW, COL].Text = "Decision Applicable";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDecisionApplicable = COL;
                COL++;
                sheet[ROW, COL].Text = "Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "By Whom";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColByWhom = COL;
                COL++;
                sheet[ROW, COL].Text = "Target Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColTargetDate = COL;
                COL++;
                
                sheet[ROW, COL].Text = "Chaired By";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColChairedBy = COL;
                COL++;
                sheet[ROW, COL].Text = "Actional Point";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColActionalPoint = COL;
                COL++;
                sheet[ROW, COL].Text = "Talking Point";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColTalkingPoint = COL;
                COL++;
                sheet[ROW, COL].Text = "Suggestion";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColSuggestion = COL;
                COL++;
                sheet[ROW, COL].Text = "Meeting Decision";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColMeetingDecision = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColRemarks = COL;
                

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
                    sheet[ROW, ColMeetingId].Text = data.Rows[i]["MeetingId"].ToString();
                    sheet[ROW, ColMeetingDate].Text = clsStaticInfo.GetDate(data.Rows[i]["MeetingDate"].ToString());
                    sheet[ROW, ColMeetingName].Text = data.Rows[i]["MeetingName"].ToString();
                    sheet[ROW, ColChairedBy].Text = data.Rows[i]["ChairedBy"].ToString();
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColCreatedBy].Text = data.Rows[i]["CreatedBy"].ToString();
                    sheet[ROW, ColMeetingType].Text = data.Rows[i]["MeetingType"].ToString();
                    sheet[ROW, ColItemTitle].Text = data.Rows[i]["ItemTitle"].ToString();
                    sheet[ROW, ColCritically].Text = data.Rows[i]["Critically"].ToString();
                    sheet[ROW, ColActionApplicable].Text = data.Rows[i]["ActionApplicable"].ToString();
                    sheet[ROW, ColDecisionApplicable].Text = data.Rows[i]["DecisionApplicable"].ToString();
                    sheet[ROW, ColStatus].Text = data.Rows[i]["Status"].ToString();
                    sheet[ROW, ColByWhom].Text = data.Rows[i]["ByWhom"].ToString();

                    sheet[ROW, ColTargetDate].Text = clsStaticInfo.GetDate(data.Rows[i]["TargetDate"].ToString());

                    sheet[ROW, ColTalkingPoint].Text = data.Rows[i]["TalkingPoint"].ToString();
                    sheet[ROW, ColSuggestion].Text = data.Rows[i]["Suggestion"].ToString();
                    sheet[ROW, ColActionalPoint].Text = data.Rows[i]["ActionToBeTaken"].ToString();
                    sheet[ROW, ColMeetingDecision].Text = data.Rows[i]["Decision"].ToString();
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Meeting Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;



               
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
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

        public void MeetingReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {


                string strSQL = @"select MA.Id MeetingId,format((MA.Date),'dd-MMM-yyyy') MeetingDate,MA.MeetingName,EINFO.EmployeeName ChairedBy,MT.Id MeetingTypeId,MT.UserName MeetingType,MIH.IssueStatus,MIH.IssueCritically Critically
							,D.Id DepartmentId,D.UserName Department
							,EI.SystemId CreatedById,EI.EmployeeName CreatedBy,MIH.ItemTitle,ActionApplicable=case when MIH.ActionApplicable=1 then 'Yes' else 'No' End 
			                ,DecisionApplicable=case when MIH.DecisionApplicable=1 then 'Yes' else 'No' End,MIH.IssueStatus [Status],EI.EmployeeName ByWhom
							,format((MIH.AddedDate),'dd-MMM-yyyy') TargetDate
							,MTP.TalkingPoint,MS.Suggestion,MAP.ActionToBeTaken,MD.Decision,MIH.Remarks

                            from MeetingItemHeader MIH
                            left join EmployeeInformation EI on EI.SystemId=MIH.ByWhomId
							left join ORG.Department D on D.Id=MIH.DepartmentId
                            left join MeetingType MT on MT.Id=MIH.MeetingTypeId
							left join MeetingAgendaItem MAI on MAI.MeetingItemHeaderId=MIH.Id
							left join MeetingAgenda MA on MA.Id=MAI. MeetingAgendaId
							left join EmployeeInformation EINFO on EINFO.SystemId=MA.ChairedById
							left join MeetingTalkingPoint MTP on MTP.MeetingItemHeaderId=MIH.Id
							left join MeetingSuggestion MS on MS.MeetingItemHeaderId=MIH.Id
							left join MeetingActionablePoints MAP on MAP.MeetingItemHeaderId=MIH.Id
							left join MeetingDecision MD on MD.MeetingItemHeaderId=MIH.Id
										
                            where MIH.DepartmentId in(" + parameters["DepartmentId"] + @")
                            AND MIH.ByWhomId in(" + parameters["ByWhomId"] + @")
                            AND MIH.MeetingTypeId in(" + parameters["MeetingTypeId"] + @")
                            AND MIH.IssueStatus in(" + parameters["Status"] + @")
                            AND MIH.Id in(" + parameters["MeetingId"] + @")";
                            
                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpGet, Authorize,AllowAnonymous]
        public ActionResult DownloadUsingFullPath(string FullPath, string fileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(FullPath);
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }

    }
}
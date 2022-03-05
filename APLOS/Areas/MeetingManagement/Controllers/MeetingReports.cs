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


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

                    
        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            try
            {
                var sql = @"SELECT * FROM (select MT.Id MeetingTypeId,MIH.Id MeetingId,MT.UserName MeetingType,MIH.IssueStatus,MIH.IssueCritically,D.Id DepartmentId,D.UserName Department
							,EI.SystemId AttendeeId,EI.EmployeeName ByWhom,MIH.ItemTitle ItemType,EI.EmployeeName Importance,ActionApplicable=case when MIH.ActionApplicable=1 then 'Yes' else 'No' End 
			                ,DecisionApplicable=case when MIH.DecisionApplicable=1 then 'Yes' else 'No' End,MIH.IssueStatus [Status],EI.EmployeeName ResponsiblePerson
							,format((MIH.AddedDate),'dd-MMM-yyyy') TargetFromDate,format((MIH.AddedDate),'dd-MMM-yyyy') TargetToDate,EI.EmployeeName UserName,EI.EmployeeName CharedBy
							,format((MIH.AddedDate),'dd-MMM-yyyy') TargetDate,format((MIH.AddedDate),'dd-MMM-yyyy') MeetingDate,MTP.Id TalkingPointId,MTP.TalkingPoint,MS.Id SuggestionId,MS.Suggestion
							,MAP.Id ActionToBeTakenId,MAP.ActionToBeTaken ActionalPoint,MD.Id DecisionId,MD.Decision,MIH.Remarks
                            from MeetingItemHeader MIH
                            left join EmployeeInformation EI on EI.SystemId=MIH.ByWhomId
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
                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDepartment = COL;
                COL++;
                sheet[ROW, COL].Text = "By Whom";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColByWhom = COL;
                COL++;
                sheet[ROW, COL].Text = "Meeting Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMeetingType = COL;
                COL++;
                sheet[ROW, COL].Text = "Item Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColItemType = COL;
                COL++;
                sheet[ROW, COL].Text = "Importance";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColImportance = COL;
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
                sheet[ROW, COL].Text = "Responsible person";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColResponsiblePerson = COL;
                COL++;
                sheet[ROW, COL].Text = "Target Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColTargetDate = COL;
                COL++;
                sheet[ROW, COL].Text = "MeetingId";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColMeetingId = COL;
                COL++;
                sheet[ROW, COL].Text = "Meeting Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColMeetingDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Chared By";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColCharedBy = COL;
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
                    sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColByWhom].Text = data.Rows[i]["ByWhom"].ToString();
                    sheet[ROW, ColMeetingType].Text = data.Rows[i]["MeetingType"].ToString();
                    sheet[ROW, ColItemType].Text = data.Rows[i]["ItemType"].ToString();
                    sheet[ROW, ColImportance].Text = data.Rows[i]["Importance"].ToString();
                    sheet[ROW, ColActionApplicable].Text = data.Rows[i]["ActionApplicable"].ToString();

                    sheet[ROW, ColDecisionApplicable].Text = data.Rows[i]["DecisionApplicable"].ToString();
                    sheet[ROW, ColStatus].Text = data.Rows[i]["Status"].ToString();
                    sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["ResponsiblePerson"].ToString();

                    sheet[ROW, ColTargetDate].Text = clsStaticInfo.GetDate(data.Rows[i]["TargetDate"].ToString());
                    sheet[ROW, ColMeetingId].Text = data.Rows[i]["MeetingId"].ToString();
                    sheet[ROW, ColMeetingDate].Text = clsStaticInfo.GetDate(data.Rows[i]["MeetingDate"].ToString());

                    sheet[ROW, ColCharedBy].Text = data.Rows[i]["CharedBy"].ToString();
                    sheet[ROW, ColTalkingPoint].Text = data.Rows[i]["TalkingPoint"].ToString();
                    sheet[ROW, ColSuggestion].Text = data.Rows[i]["Suggestion"].ToString();
                    sheet[ROW, ColActionalPoint].Text = data.Rows[i]["ActionalPoint"].ToString();
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


                string strSQL = @"select MT.Id MeetingTypeId,MT.UserName MeetingType,MIH.IssueStatus,MIH.IssueCritically,D.Id DepartmentId,D.UserName Department
							,EI.SystemId ByWhomId,EI.EmployeeName ByWhom,MIH.ItemTitle ItemType,EI.EmployeeName Importance,ActionApplicable=case when MIH.ActionApplicable=1 then 'Yes' else 'No' End 
			                ,DecisionApplicable=case when MIH.DecisionApplicable=1 then 'Yes' else 'No' End,MIH.IssueStatus [Status],EI.EmployeeName ResponsiblePerson
							,format((MIH.AddedDate),'dd-MMM-yyyy') TargetDate,MIH.Id MeetingId,format((MIH.AddedDate),'dd-MMM-yyyy') MeetingDate
							
							,CharedBy=STUFF((select distinct ','+EMI.EmployeeName CharedBy
							  from MeetingExpectedPerson XMOIC
							  left join EmployeeInformation EMI on EMI.SystemId=XMOIC.ExpectedPersonId
							 where XMOIC.MeetingItemHeaderId=MIH.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

							,TalkingPoint=STUFF((select distinct ','+XMOI.TalkingPoint 
							  from MeetingTalkingPoint XMOI 	  
							 where XMOI.MeetingItemHeaderId=MIH.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

							 ,Suggestion=STUFF((select distinct ','+XMOIS.Suggestion
							  from MeetingSuggestion XMOIS 	  
							 where XMOIS.MeetingItemHeaderId=MIH.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

							 ,ActionalPoint=STUFF((select distinct ','+XMOIA.ActionToBeTaken
							  from MeetingActionablePoints XMOIA 	  
							 where XMOIA.MeetingItemHeaderId=MIH.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

							 ,Decision=STUFF((select distinct ','+XMOIM.Decision
							  from MeetingDecision XMOIM 	  
							 where XMOIM.MeetingItemHeaderId=MIH.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,MIH.Remarks

                            from MeetingItemHeader MIH
                            left join EmployeeInformation EI on EI.SystemId=MIH.ByWhomId
							left join ORG.Department D on D.Id=MIH.DepartmentId
                            left join MeetingType MT on MT.Id=MIH.MeetingTypeId
										
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

        [HttpGet, Authorize]
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
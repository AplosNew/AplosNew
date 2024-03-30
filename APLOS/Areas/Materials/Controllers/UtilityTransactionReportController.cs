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

namespace Aplos.Areas.Materials.Controllers
{
    public class UtilityTransactionReportController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public UtilityTransactionReportController(ISqlRepository R)
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

       // [HttpGet, Authorize]
       // public ActionResult getFilters()
       // {
       //     try
       //     {
       //         var sql = @"select UT.Id,FORMAT(UT.Date,'dd-MMM-yyyy') [Date],MAX(CONVERT(varchar(5),UT.AddedDate,108)) [Time],UM.UtilityGroup [Group],UM.UtilitySubGroup SubGroup,UM.UtilityCategory Category
							//,UM.UtilitySubCategory SubCategory,UM.Item,EI.EmployeeName ResponsiblePerson 
							//,format(UT.AddedDate,'dd-MMM-yyyy')AddedDate,UT.Quantity,UT.Reading,UT.Remarks
							//from UtilityTransaction UT
							//left join UtilityMaster UM on UM.Id=UT.UtilityMasterId
							//left join EmployeeInformation EI on EI.SystemId=UM.ResponsiblePersonId
							//group by UT.Id,UT.Date,UT.AddedDate,UM.UtilityGroup,UM.UtilitySubGroup,UM.UtilityCategory,UM.UtilitySubCategory
							//,UM.Item,EI.EmployeeName,UT.Quantity,UT.Reading,UT.Remarks";

       //         return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
       //     }
       //     catch (Exception e)
       //     {
       //         throw e;
       //     }
       // }

        [Authorize, HttpPost]
        public ActionResult getUtilityTransactionData(string ToDate, string FromDate)
        {
            var str = @"select UT.Id,FORMAT(UT.Date,'dd-MMM-yyyy') [Date],MAX(CONVERT(varchar(5),UT.AddedDate,108)) [Time],UG.UserName [Group],UM.UtilitySubGroup SubGroup,UM.UtilityCategory Category,UM.Item
							,UM.UtilitySubCategory SubCategory,UM.Item,EI.EmployeeName ResponsiblePerson 
							,format(UT.AddedDate,'dd-MMM-yyyy')AddedDate,UT.Quantity,UT.Reading,isnull(UT.Remarks,'')Remarks
                            ,Amount=isnull(UT.Quantity*(SELECT TOP(1) Rate FROM dbo.UtilityDetail  WHERE EffectiveDate between '" + FromDate + @"' and '" + ToDate + @"' AND UtilityMasterId=UT.UtilityMasterId ORDER BY EffectiveDate),0)
                            ,isnull(UT.MultiplyingFactor,0) MultiplyingFactor
							,FinalQuantity=isnull(UT.MultiplyingFactor,0)*UT.Quantity
							,ET.UserName Entity , UOM.UserName UOM 
                            ,case when  UD.Rate is null then 0 else UD.Rate end Rate
							,case when  UD.EffectiveDate is null then '' else format(UD.EffectiveDate,'dd-MMM-yyyy') end EffectiveDate
							,PT.UserName PartyName , RS.EmployeeName ResponsiblePerson , AD.EmployeeName [Admin] , UM.EntryLegDays , 
							case when um.Active = 1 then 'Active' else 'InActive' end Status
							from UtilityTransaction UT
							left join UtilityMaster UM on UM.Id=UT.UtilityMasterId
							left join EmployeeInformation EI on EI.SystemId=UM.ResponsiblePersonId
                            left join HKP.UtilityGroup UG on UG.Id=UM.UtilityGroupId
							left join org.Entity ET on ET.Id = UM.EntityId
							left join [SCS].[UnitOfMeasurement] UOM on UOM.Id = UM.UoMId
							left join UtilityDetail UD on UD.UtilityMasterId = UM.Id
							left join hkp.Party PT on PT.Id = UM.PartyId
							left join EmployeeInformation RS on RS.SystemId = UM.ResponsiblePersonId
							left join EmployeeInformation AD on AD.SystemId = UM.AdminId
                             where UT.Date between '" + FromDate + @"' and '" + ToDate + @"'
                             group by UT.Id,UT.Date,UT.AddedDate,UM.UserName,UM.UtilitySubGroup,UM.UtilityCategory,UM.UtilitySubCategory
							,UM.Item,EI.EmployeeName,UT.Quantity,UT.Reading,UT.Remarks,UG.UserName,UT.MultiplyingFactor,UT.UtilityMasterId , ET.UserName , UOM.UserName
							,UD.Rate , UD.EffectiveDate ,PT.UserName , PT.UserName , RS.EmployeeName ,  AD.EmployeeName , UM.EntryLegDays
							,um.Active";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

            
        [HttpPost, Authorize]
        public ActionResult GetUtilityTransactionReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            { 
                string fileName = "";
                fileName = UtilityTransactionReportxlx(data, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string UtilityTransactionReportxlx(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
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
                workbook.Worksheets[0].Name = "UtilityTransactionReport";
                sheet = workbook.Worksheets[0];
                //DataTable data;
                //UtilityTransactionReportSQL(ToDate,FromDate, out data);
                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Time";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColTime = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColSubCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Item";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColItem = COL;
                COL++;

                sheet[ROW, COL].Text = "Group";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "SubGroup";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColSubGroup = COL;
                COL++;

               

               
                sheet[ROW, COL].Text = "PartyName";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColPartyName = COL;
                COL++;
                sheet[ROW, COL].Text = "Reading";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColReading = COL;
                COL++;
                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "Quantity";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColQuantity = COL;
                COL++;

                sheet[ROW, COL].Text = "Multiplying Factor";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColMultiplyingFactor = COL;
                COL++;

                sheet[ROW, COL].Text = "Final Quantity";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColFinalQuantity = COL;
                COL++;

               
                sheet[ROW, COL].Text = "EffectiveDate";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColEffectiveDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColRate = COL;
                COL++;
                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "EntryLegDays";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColEntryLegDays = COL;
                COL++;



                sheet[ROW, COL].Text = "ResponsiblePerson";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColResponsiblePerson = COL;
                COL++;

                sheet[ROW, COL].Text = "Admin";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColAdmin = COL;
                COL++;

               
                sheet[ROW, COL].Text = "Status";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 15;
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

                for (int i = 0; i < data.Count; i++)
                {
                    sheet[ROW, ColDate].Text = data[i]["Date"].ToString(); 
                    sheet[ROW, ColTime].Text = data[i]["Time"].ToString(); 
                    sheet[ROW, ColCategory].Text = data[i]["Category"].ToString();
                    sheet[ROW, ColSubCategory].Text = data[i]["SubCategory"].ToString();
                    sheet[ROW, ColItem].Text = data[i]["Item"].ToString();
                    sheet[ROW, ColGroup].Text = data[i]["Group"].ToString();
                    sheet[ROW, ColSubGroup].Text = data[i]["SubGroup"].ToString();
                    sheet[ROW, ColQuantity].Number = clsStaticInfo.dbl(data[i]["Quantity"].ToString());
                    sheet[ROW, ColQuantity].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColFinalQuantity].Number = clsStaticInfo.dbl(data[i]["FinalQuantity"].ToString());
                    sheet[ROW, ColFinalQuantity].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColMultiplyingFactor].Number = clsStaticInfo.dbl(data[i]["MultiplyingFactor"].ToString());
                    sheet[ROW, ColMultiplyingFactor].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColReading].Number = clsStaticInfo.dbl(data[i]["Reading"].ToString());
                    sheet[ROW, ColReading].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    //if (reading == 0)
                    //{
                    //    reading= clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());
                    //    sheet[ROW, ColReading].Number = reading;
                    //}
                    //else
                    //{
                    //    reading = clsStaticInfo.dbl(reading) + clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString()); 
                    //    sheet[ROW, ColReading].Number = reading;
                    //}
                                       
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data[i]["Amount"].ToString());
                    sheet[ROW, ColAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColEntity].Text = data[i]["Entity"].ToString();
                    sheet[ROW, ColUOM].Text = data[i]["UOM"].ToString();
                    sheet[ROW, ColEffectiveDate].Text = data[i]["EffectiveDate"].ToString();
                    sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data[i]["Rate"].ToString());
                    sheet[ROW, ColRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColPartyName].Text = data[i]["PartyName"].ToString();
                    sheet[ROW, ColResponsiblePerson].Text = data[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, ColAdmin].Text = data[i]["Admin"].ToString();
                    sheet[ROW, ColEntryLegDays].Text = data[i]["EntryLegDays"].ToString();
                    sheet[ROW, ColStatus].Text = data[i]["Status"].ToString();
                    sheet[ROW, ColRemarks].Text = data[i]["Remarks"].ToString();
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
                reportUtility.PlantHeader(ref sheet, endCol, "Utility Transaction Report", identity.PlantId);
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

        public void UtilityTransactionReportSQL(string ToDate, string FromDate, out DataTable data)
        {
            try
            {
                string strSQL = @"select UT.Id,FORMAT(UT.Date,'dd-MMM-yyyy') [Date],MAX(CONVERT(varchar(5),UT.AddedDate,108)) [Time],UG.UserName [Group],UM.UtilitySubGroup SubGroup,UM.UtilityCategory Category,UM.Item
							,UM.UtilitySubCategory SubCategory,UM.Item,EI.EmployeeName ResponsiblePerson 
							,format(UT.AddedDate,'dd-MMM-yyyy')AddedDate,UT.Quantity,UT.Reading,isnull(UT.Remarks,'')Remarks
                            ,Amount=isnull(UT.Quantity*(SELECT TOP(1) Rate FROM dbo.UtilityDetail  WHERE EffectiveDate between '" + FromDate + @"' and '" + ToDate + @"' AND UtilityMasterId=UT.UtilityMasterId ORDER BY EffectiveDate),0)
                            ,isnull(UT.MultiplyingFactor,0) MultiplyingFactor
							,FinalQuantity=isnull(UT.MultiplyingFactor,0)*UT.Quantity
							,ET.UserName Entity , UOM.UserName UOM 
                            ,case when  UD.Rate is null then 0 else UD.Rate end Rate
							,case when  UD.EffectiveDate is null then '' else format(UD.EffectiveDate,'dd-MMM-yyyy') end EffectiveDate
							,PT.UserName PartyName , RS.EmployeeName ResponsiblePerson , AD.EmployeeName [Admin] , UM.EntryLegDays , 
							case when um.Active = 1 then 'Active' else 'InActive' end Status
							from UtilityTransaction UT
							left join UtilityMaster UM on UM.Id=UT.UtilityMasterId
							left join EmployeeInformation EI on EI.SystemId=UM.ResponsiblePersonId
                            left join HKP.UtilityGroup UG on UG.Id=UM.UtilityGroupId
							left join org.Entity ET on ET.Id = UM.EntityId
							left join [SCS].[UnitOfMeasurement] UOM on UOM.Id = UM.UoMId
							left join UtilityDetail UD on UD.UtilityMasterId = UM.Id
							left join hkp.Party PT on PT.Id = UM.PartyId
							left join EmployeeInformation RS on RS.SystemId = UM.ResponsiblePersonId
							left join EmployeeInformation AD on AD.SystemId = UM.AdminId
                             where UT.Date between '" + FromDate + @"' and '" + ToDate + @"'
                             group by UT.Id,UT.Date,UT.AddedDate,UM.UserName,UM.UtilitySubGroup,UM.UtilityCategory,UM.UtilitySubCategory
							,UM.Item,EI.EmployeeName,UT.Quantity,UT.Reading,UT.Remarks,UG.UserName,UT.MultiplyingFactor,UT.UtilityMasterId , ET.UserName , UOM.UserName
							,UD.Rate , format(UD.EffectiveDate,'dd-MMM-yyyy') ,PT.UserName , PT.UserName , RS.EmployeeName ,  AD.EmployeeName , UM.EntryLegDays
							,um.Active";

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
using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.Service.Administration.Contract;
using Aplos.Properties;
using Library.Security.Core;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using System.IO;
using System.Data;
using Library.Model.Enums;
using Syncfusion.Pdf;
using Syncfusion.ExcelToPdfConverter;
using Library.MaterialManagement.Material;

namespace Aplos.Areas.Administration.Controllers
{
    public class GeneralContractCheckedController : BaseController
    {
        GeneralContractCheckService gc = new GeneralContractCheckService();
       private readonly SqlRepository _sqlRepository ; 
        #region CONSTRUCTOR
        public GeneralContractCheckedController()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion CONSTRUCTOR

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }

        
        #endregion Page

        #region GETFUNCTION
        public ActionResult GetUncheckedData()
        {
            try
            {
                var sql = @"select  GCE.Id, FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, EI.EmployeeName, GC.UserName Contract
                            from TRN.GeneralContractEntry GCE
                            LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
                            LEFT JOIN MST.GeneralContract GC ON GC.Id = GCE.GeneralContractId
                            left join EmployeeInformation EI on EI.SystemId = GCE.CheckBySystemId
                            where GCE.CheckedByStatus='To Be Check'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult GetcheckedData()
        {
            try
            {
                var sql = @"select  GCE.Id, FORMAT([GCE].[Date], 'dd-MMM-yyyy')[Date], E.UserName Entity, EI.EmployeeName, GC.UserName Contract,
                            GCE.CheckedByStatus, GCE.CheckedReason
                            from TRN.GeneralContractEntry GCE
                            LEFT JOIN ORG.Entity E on E.Id = GCE.EntityId
                            LEFT JOIN MST.GeneralContract GC ON GC.Id = GCE.GeneralContractId
                            left join EmployeeInformation EI on EI.SystemId = GCE.ApprovedById
                            where GCE.CheckedByStatus = 'Checked'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetAllCheckBy()
        {
            try
            {
                var sql = @"select EI.SystemId Value, EI.EmployeeName Text
                            from MST.GeneralContractApproveBy GCA
                            left join  MST.GeneralContract GC on GC.Id = GCA.GeneralContractId
                            left join EmployeeInformation EI on EI.SystemId = GCA.SystemId";



                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public ActionResult GetChildList()
        {
            try
            {
                var sql = @"select CIE.*, GCI.UserName from TRN.ContractItemEntry CIE
                left join TRN.GeneralContractEntry GCE on GCE.Id = CIE.GeneralContractEntryId
                left join HKP.GeneralContractItemMaster GCI on GCI.Id = CIE.ContractMasterId";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion GETFUNCTION

        #region SAVE
        [HttpPost]
        public ActionResult GeneralContractChecked (string headerId, string CheckedStataus, string AuthorizedById, string CheckedReason)
        {
            try
            {
                gc.GeneralContractChecked(headerId, CheckedStataus, AuthorizedById, CheckedReason);
                return Json(new { Message = "General Contract  Checked " + AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE

        #region report
        [HttpGet, Authorize]
        public ActionResult GetGeneralContractReport(ReportFormat reportFormat, string ContractId)
        {
            try
            {
                string fileName = "";
                IWorkbook workbook = GetGeneralContractWorkbook("General Contract", ContractId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "General Contract Report";
                // return RenderReportAsPdf(workbook, reportFileName);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;

                    case ReportFormat.PdfView:
                        PdfDocument document1 = new PdfDocument();
                        ExcelToPdfConverterSettings settings1 = new ExcelToPdfConverterSettings();
                        settings1.TemplateDocument = document1;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document1 = converter1.Convert(settings1);
                        }
                        document1.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                        //return RenderReportAsPdf(document1, reportFileName);
                        return RenderReportAsPdf(workbook, reportFileName);
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IWorkbook GetGeneralContractWorkbook(string SheetName, string ContractId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
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
                workbook.Worksheets[0].Name = "General Contract";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder,dtDetail;
                gc.GetGeneralContractCheckedHeaderData(ContractId, out dtOrder);
                gc.GetGeneralContractCheckedDetailsData(ContractId, out dtDetail);
 
                int ROW = 5; int COL = 1;
                sheet.Range[ROW, COL].Text = "Contract Name";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet.Range[ROW, COL + 1].Text = dtOrder.Rows[0]["Contract"].ToString();
                sheet[ROW, COL+1].ColumnWidth = 25;

                sheet.Range[ROW, COL + 2].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet.Range[ROW, COL + 3].Text = dtOrder.Rows[0]["Entity"].ToString();
                 
                sheet.Range[ROW, 1, ROW , COL + 3].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW , COL + 3].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW , COL + 3].BorderInside(ExcelLineStyle.Hair);


                ROW = 6; COL = 1;
                sheet.Range[ROW, COL].Text = "Date";
                sheet.Range[ROW, COL + 1].Text = dtOrder.Rows[0]["Date"].ToString();

                sheet.Range[ROW, COL + 2].Text = "Check By";
                sheet.Range[ROW, COL + 3].Text = dtOrder.Rows[0]["EmployeeName"].ToString();

                sheet.Range[ROW, 1, ROW , COL + 3].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW , COL + 3].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW , COL + 3].BorderInside(ExcelLineStyle.Hair);

                ROW = 7; COL = 1;
                sheet.Range[ROW, COL].Text = "Contract Status";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet.Range[ROW, COL + 1].Text = dtOrder.Rows[0]["CheckedByStatus"].ToString();

                sheet.Range[ROW, COL + 2].Text = "Approved By";
                sheet.Range[ROW, COL + 3].Text = dtOrder.Rows[0]["ApprovedBy"].ToString();

                sheet.Range[ROW, 1, ROW, COL + 3].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, COL + 3].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, COL + 3].BorderInside(ExcelLineStyle.Hair);

                #region ColumnsHeader

                ROW = 9; COL = 1;
                sheet[ROW, COL].Text = "Item"; 
                sheet[ROW, COL].ColumnWidth = 15; 
                int colItem = COL; 
                COL++;

                sheet[ROW, COL].Text = "Avg Qty"; 
                //sheet[ROW, COL].ColumnWidth = 16; 
                int colAvgQty = COL; 
                COL++;

                sheet[ROW, COL].Text = "Transaction Qty"; 
                sheet[ROW, COL].ColumnWidth = 16; 
                int colTransactionQty = COL; 
                COL++;

                sheet[ROW, COL].Text = "Rate"; 
                sheet[ROW, COL].ColumnWidth = 25; 
                int colRate = COL; 
                COL++;

                sheet[ROW, COL].Text = "Amount"; 
                sheet[ROW, COL].ColumnWidth = 15; 
                int colAmount = COL; 
                COL++;
                 
                sheet[ROW, COL].Text = "Remarks"; 
                sheet[ROW, COL].ColumnWidth = 10; 
                int colRemarks = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                #endregion columns

                ROW++;
                int startRow = ROW;

                #region DataPlot
                for (int i = 0; i < dtDetail.Rows.Count; i++)
                {
                    sheet[ROW, colItem].Text = dtDetail.Rows[i]["Item"].ToString();
                    sheet[ROW, colAvgQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtDetail.Rows[i]["AvgQty"].ToString());
                    sheet[ROW, colTransactionQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtDetail.Rows[i]["TransactionQuantity"].ToString());
                    sheet[ROW, colRate].Number = Library.Service.Extension.clsStaticInfo.dbl(dtDetail.Rows[i]["Rate"].ToString());
                    sheet[ROW, colAmount].Number = Library.Service.Extension.clsStaticInfo.dbl(dtDetail.Rows[i]["Amount"].ToString());
                    sheet[ROW, colRemarks].Text = dtDetail.Rows[i]["Remarks"].ToString();
                     
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                #endregion
                int edCRow = ROW;
                sheet.Range[edCRow, 1].Text = "TOTAL";
                sheet.Range[edCRow, 1].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 2].Number = OTSBD.clsStaticInfo.dbl(dtDetail.Compute("SUM(AvgQty)", null));
                sheet.Range[edCRow, 2].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 2].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 2, edCRow, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 2, edCRow, 6].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 3].Number = OTSBD.clsStaticInfo.dbl(dtDetail.Compute("SUM(TransactionQuantity)", null));
                sheet.Range[edCRow, 3].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 3].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 3, edCRow, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 3, edCRow, 7].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 5].Number = OTSBD.clsStaticInfo.dbl(dtDetail.Compute("SUM(Amount)", null));
                sheet.Range[edCRow, 5].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 5].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 5, edCRow, 9].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 5, edCRow, 9].HorizontalAlignment = ExcelHAlign.HAlignRight;
                 
                sheet.Range[edCRow, 1, edCRow, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[edCRow, 1, edCRow, endCol].BorderInside(ExcelLineStyle.Hair);

                edCRow++;
                edCRow++;
                edCRow++;
                edCRow++;
                edCRow++;
                edCRow++;

                sheet.Range[edCRow - 1, 1].Text = dtOrder.Rows[0]["AddedBy"].ToString();
                sheet.Range[edCRow, 1].Text = "PareparedBy";
                sheet.Range[edCRow - 1, 3].Text = dtOrder.Rows[0]["EmployeeName"].ToString();
                sheet.Range[edCRow, 3].Text = "CheckedBy";
                sheet.Range[edCRow - 1, 5].Text = dtOrder.Rows[0]["ApprovedBy"].ToString();
                sheet.Range[edCRow, 5].Text = "Approved By";

                #region ReportHeader
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "General Contract Report", identity.PlantId);
                //reportUtility.CompanyHeader(ref sheet, endCol, "Material Issue Report", identity.CompanyId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;
                #endregion


                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion report
    }


}
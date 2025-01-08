#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using Library.OrderManagement.Production;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.Data;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Aplos.Areas.Commercial.Controllers;
using System.Drawing;
using System.Collections;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class FinishedGoodsPackingReportController : BaseController
    {
        PackingData det = new PackingData();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public FinishedGoodsPackingReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetReport(string fromDate, string toDate, string PurposeId)
        {
            try
            {
                string fileName = "";
                fileName = GetFinishedGoodsPackingReport(fromDate, toDate, PurposeId, "GetFinishedGoodsPackingReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GetFinishedGoodsPackingReport(string fromDate, string toDate, string PurposeId, string SheetName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                IdentityParameter para = new IdentityParameter
                {
                    CompanyGroupId = identity.CompanyGroupId,
                    CompanyId = identity.CompanyId,
                    PlantId = identity.PlantId,
                    AddedBy = identity.Name,
                    AddedDate = DateTime.Now,
                    AddedFromIP = identity.IPAddress,
                    UpdatedBy = identity.Name,
                    UpdatedDate = DateTime.Now,
                    UpdatedFromIP = identity.IPAddress
                };
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "Data";
                sheet = workbook.Worksheets[1];
                DataTable dtOrder;
                det.GetFinishedGoodsPackingReportData(fromDate, toDate, PurposeId, out dtOrder);

                //if (dtOrder.Rows.Count > 0)
                //{
                //    det.SaveScandataToBooking(fromDate, toDate, PurposeId, para);
                //}
                //else
                //{
                //    throw new CustomException("No Data found.");
                //}
                if (dtOrder.Rows.Count == 0)
                {
                    throw new CustomException("No Data found.");
                }

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "PROD_TYPE";
                sheet[ROW, COL].ColumnWidth = 16;
                int colId = COL;
                COL++;
                sheet[ROW, COL].Text = "ProductCode";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "POId";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOId = COL;
                COL++;
                sheet[ROW, COL].Text = "LotNo";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLotNo = COL;
                COL++;
                sheet[ROW, COL].Text = "RefNo";
                sheet[ROW, COL].ColumnWidth = 16;
                int colRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Cones";
                sheet[ROW, COL].ColumnWidth = 22;
                int colCones = COL;
                COL++;
                sheet[ROW, COL].Text = "NetWeight";
                sheet[ROW, COL].ColumnWidth = 12;
                int colNetWeight = COL;
                COL++;
                sheet[ROW, COL].Text = "GWeight";
                sheet[ROW, COL].ColumnWidth = 12;
                int colGWeight = COL;
                COL++;
                sheet[ROW, COL].Text = "PackedBy";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPackedBy = COL;
                COL++;
                sheet[ROW, COL].Text = "Shade";
                sheet[ROW, COL].ColumnWidth = 16;
                int colShade = COL;
                COL++;
                sheet[ROW, COL].Text = "AddedBy";
                sheet[ROW, COL].ColumnWidth = 12;
                int colAddedBy = COL;
                COL++;
                sheet[ROW, COL].Text = "WorkDate";
                sheet[ROW, COL].ColumnWidth = 12;
                int colWorkDate = COL;
                COL++;
                sheet[ROW, COL].Text = "AddedDate";
                sheet[ROW, COL].ColumnWidth = 12;
                int colAddedDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 12;
                int colStandardName = COL;
                COL++;
                sheet[ROW, COL].Text = "FromLocation";
                sheet[ROW, COL].ColumnWidth = 30;
                int colFromLocation = COL;
                COL++;
                sheet[ROW, COL].Text = "ToLocation";
                sheet[ROW, COL].ColumnWidth = 30;
                int colToLocation = COL;

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

                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    sheet[ROW, colId].Text = dtOrder.Rows[i]["PROD_TYPE"].ToString();
                    sheet[ROW, colPlant].Text = dtOrder.Rows[i]["ProductCode"].ToString();
                    sheet[ROW, colPOId].Text = dtOrder.Rows[i]["POId"].ToString();
                    sheet[ROW, colLotNo].Text = dtOrder.Rows[i]["LotNo"].ToString();
                    sheet[ROW, colRefNo].Text = dtOrder.Rows[i]["RefNo"].ToString();
                    sheet[ROW, colCones].Text = dtOrder.Rows[i]["Cones"].ToString();
                    sheet[ROW, colNetWeight].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["NetWeight"].ToString());
                    sheet[ROW, colGWeight].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["GWeight"].ToString());
                    sheet[ROW, colPackedBy].Text = dtOrder.Rows[i]["PackedBy"].ToString();
                    sheet[ROW, colShade].Text = dtOrder.Rows[i]["Shade"].ToString();
                    sheet[ROW, colAddedBy].Text = dtOrder.Rows[i]["AddedBy"].ToString();
                    sheet[ROW, colWorkDate].Text = dtOrder.Rows[i]["WorkDate"].ToString();
                    sheet[ROW, colAddedDate].Text = dtOrder.Rows[i]["AddedDate"].ToString();
                    sheet[ROW, colStandardName].Text = dtOrder.Rows[i]["Article"].ToString();
                    sheet[ROW, colFromLocation].Text = dtOrder.Rows[i]["FromLocation"].ToString();
                    sheet[ROW, colToLocation].Text = dtOrder.Rows[i]["ToLocation"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


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



                #region Pivot

                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "FinishedGoodsPackingReport" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "Report";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[colWorkDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colStandardName - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colShade - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colLotNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colNetWeight - 1].Axis = PivotAxisTypes.Row;


                IPivotField field = pivotTable.Fields[colRefNo - 1];
                IPivotField fieldNW = pivotTable.Fields[colNetWeight - 1];
                IPivotField fieldGW = pivotTable.Fields[colGWeight - 1];
                fieldNW.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                fieldGW.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "RefNo", PivotSubtotalTypes.Count);
                pivotTable.DataFields.Add(fieldNW, "NetWeight", PivotSubtotalTypes.Sum);
                pivotTable.DataFields.Add(fieldGW, "GWeight", PivotSubtotalTypes.Sum);

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colId || i == colStandardName || i == colShade || i == colLotNo || i == colRefNo)
                    {
                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                    }
                    else if (i == colWorkDate)
                    {
                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.Sum;
                    }
                    else
                    {
                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                    }
                }

                //  pivotTable.Fields[colWorkDate].Subtotals = PivotSubtotalTypes.Sum;


                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Finished Goods Packing Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                workbook.Worksheets[0].UsedRange["A7"].FreezePanes();


                #endregion Buyer Summary
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




    }
}
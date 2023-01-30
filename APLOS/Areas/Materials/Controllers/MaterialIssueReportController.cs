#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.MaterialManagement.Material;
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
    public class MaterialIssueReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        clsMaterial clsM = new clsMaterial();
        public MaterialIssueReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult getFiltersData()
        {
            try
            {
                return Json(clsM.getFiltersData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetTransactionData()
        {
            return Json(clsM.GetTransactionData(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetReport(Dictionary<string, string> parameters)
        {
            try
            {
                string fileName = "";
                fileName = GetTransactionReport(parameters, "Transaction Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GetTransactionReport(Dictionary<string, string> parameters, string SheetName)
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
                workbook.Worksheets[0].Name = "TransactionReport";
                sheet = workbook.Worksheets[0];
                DataTable data;
                clsM.GetTransactionReportSQL(parameters, out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "POStatus"; sheet[ROW, COL].ColumnWidth = 16; int ColPOStatus = COL; COL++;
                sheet[ROW, COL].Text = "PONo"; sheet[ROW, COL].ColumnWidth = 16; int ColPOId = COL; COL++;
                sheet[ROW, COL].Text = "SONo"; sheet[ROW, COL].ColumnWidth = 16; int ColSONo = COL; COL++;
                sheet[ROW, COL].Text = "Material"; sheet[ROW, COL].ColumnWidth = 16; int ColMaterial = COL; COL++;
                sheet[ROW, COL].Text = "Article"; sheet[ROW, COL].ColumnWidth = 16; int ColArticle = COL; COL++;
                sheet[ROW, COL].Text = "NetConsumptionPerUnit"; sheet[ROW, COL].ColumnWidth = 16; int ColNetConsumptionPerUnit = COL; COL++;
                sheet[ROW, COL].Text = "ValueLoss"; sheet[ROW, COL].ColumnWidth = 16; int ColValueLoss = COL; COL++;
                sheet[ROW, COL].Text = "GrossConsumption"; sheet[ROW, COL].ColumnWidth = 16; int ColGrossConsumption = COL; COL++;
                sheet[ROW, COL].Text = "TotalConsumption"; sheet[ROW, COL].ColumnWidth = 16; int ColTotalConsumption = COL; COL++;
                sheet[ROW, COL].Text = "PlanConsumption"; sheet[ROW, COL].ColumnWidth = 16; int ColPlanConsumption = COL; COL++;
                sheet[ROW, COL].Text = "Rate"; sheet[ROW, COL].ColumnWidth = 16; int ColRate = COL; COL++;
                sheet[ROW, COL].Text = "TotaPlanlAmount"; sheet[ROW, COL].ColumnWidth = 16; int ColTotaPlanlAmount = COL; COL++;
                sheet[ROW, COL].Text = "RequestedQty"; sheet[ROW, COL].ColumnWidth = 16; int ColRequestedQty = COL; COL++;
                sheet[ROW, COL].Text = "IssueQty"; sheet[ROW, COL].ColumnWidth = 16; int ColIssueQty = COL; COL++;
                sheet[ROW, COL].Text = "Balance"; sheet[ROW, COL].ColumnWidth = 16; int ColBalance = COL;
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
                    sheet[ROW, ColPOStatus].Text = data.Rows[i]["POStatus"].ToString();
                    sheet[ROW, ColPOId].Text = data.Rows[i]["POId"].ToString();
                    sheet[ROW, ColSONo].Text = data.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, ColMaterial].Text = data.Rows[i]["MaterialMaster"].ToString();
                    sheet[ROW, ColArticle].Text = data.Rows[i]["QBOQArticle"].ToString();
                    sheet[ROW, ColNetConsumptionPerUnit].Text = data.Rows[i]["NetConsumptionPerUnit"].ToString();
                    sheet[ROW, ColValueLoss].Text = data.Rows[i]["ValueLoss"].ToString();
                    sheet[ROW, ColGrossConsumption].Text = data.Rows[i]["GrossConsumption"].ToString();
                    sheet[ROW, ColTotalConsumption].Text = data.Rows[i]["TotalConsumption"].ToString();
                    sheet[ROW, ColPlanConsumption].Number = clsStaticInfo.dbl(data.Rows[i]["PlanConsumption"].ToString());
                    sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rate"].ToString());
                    sheet[ROW, ColRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColTotaPlanlAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotaPlanlAmount"].ToString());
                    sheet[ROW, ColTotaPlanlAmount].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColRequestedQty].Number = clsStaticInfo.dbl(data.Rows[i]["RequestedQty"].ToString());
                    sheet[ROW, ColRequestedQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColIssueQty].Text = data.Rows[i]["IssueQty"].ToString();
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
                reportUtility.PlantHeader(ref sheet, endCol, "Material Issue Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;



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
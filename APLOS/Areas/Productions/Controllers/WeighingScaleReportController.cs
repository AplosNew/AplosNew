using Aplos.Controllers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Syncfusion.XlsIO;
using Library.Service.Productions;
using Library.Service.Helpers;
using System.IO;

namespace Aplos.Areas.Productions.Controllers
{
    public class WeighingScaleReportController : BaseController
    {
       WeighingScaleData rep = new WeighingScaleData();

        public WeighingScaleReportController()
        {
            rep = new WeighingScaleData();
        }

        public ActionResult Aplos()
        {
            return View();
        }
             

        [HttpGet, Authorize]
        public JsonResult GetStatus()
        {
            try
            {
                return Json(rep.GetStatus(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error =true,Message= ex.Message },JsonRequestBehavior.AllowGet);
               
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetData(string Status,string purp)
        {
            try
            {
                var data = rep.GetData(out List<string> ExtraColumns, Status,purp);
                var jsondata = Json(new { Error = false, DATA =data , Columns = ExtraColumns }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string Status,string SO, string ProductCode, string PO, string Product, string Material, string LotNo,string MaterialCode, string Customer,string MasterOrderNo,string purp)
        {
            try
            {
                var workbook = GetFilterData(Status, SO, ProductCode, PO, Product, Material, LotNo, MaterialCode, Customer, MasterOrderNo,purp);
                string strFileName = "";
                if (purp == "For User")
                {
                    strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "For-Users.xlsx";
                }
                else
                {
                    strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "Weighing-scale.xlsx";                    
                }

                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private IWorkbook GetFilterData(string Status,string SO, string ProductCode, string PO, string Product, string Material, string LotNo, string MaterialCode, string Customer, string MasterOrderNo,string purp)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            if (purp == "For User")
            {
                sheet.Name = "For Users Report";
            }
            else
            {
                sheet.Name = "Weighing Scale Report";
            }

            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            DataTable data = rep.GetReportData(out List<string> ExtraColumns, Status,  SO,  ProductCode,  PO,  Product,  Material,  LotNo, MaterialCode,  Customer,  MasterOrderNo,purp);

            if (purp == "For User")
            {
                #region Headers

               

                report.SetHeaderText(ref sheet, ROW, COL, "PO", 13, ExcelHAlign.HAlignCenter);
                int ColPO = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "LotNo", 13, ExcelHAlign.HAlignCenter);
                int ColLotNo = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "ProductCode", 13, ExcelHAlign.HAlignCenter);
                int ColProductCode = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "SOQty", 13, ExcelHAlign.HAlignCenter);
                int ColSOQty = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "DeliveryDate", 13, ExcelHAlign.HAlignCenter);
                int Coldate = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "SO", 13, ExcelHAlign.HAlignCenter);
                int ColSO = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "Product", 13, ExcelHAlign.HAlignCenter);
                int ColProduct = COL;
                COL++;
               
                int ColP=0; 
                foreach (var p in ExtraColumns)
                {
                   
                    report.SetHeaderText(ref sheet, ROW, COL, p, 14, ExcelHAlign.HAlignCenter);
                    ColP = COL;                    
                    COL++;
                }

                report.SetHeaderText(ref sheet, ROW, COL, "Material", 13, ExcelHAlign.HAlignCenter);
                int ColMaterial = COL;
                COL++;

                report.SetHeaderText(ref sheet, ROW, COL, "MaterialCode", 13, ExcelHAlign.HAlignCenter);
                int ColMaterialCode = COL;
                COL++; 
                
                report.SetHeaderText(ref sheet, ROW, COL, "MasterOrderNo", 13, ExcelHAlign.HAlignCenter);
                int ColMasterOrderNo = COL;
                COL++; 
                
                report.SetHeaderText(ref sheet, ROW, COL, "Customer", 13, ExcelHAlign.HAlignCenter);
                int ColCustomer = COL;
                COL++; 
                
                report.SetHeaderText(ref sheet, ROW, COL, "Production Status", 13, ExcelHAlign.HAlignCenter);
                int ColOrderStatus = COL;
                COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 13, ExcelHAlign.HAlignCenter);
                int ColRemarks = COL;
                ROW++;
                endCol = COL;


                #endregion Headers


                var startRow = 0;
                var endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, Coldate].Text = data.Rows[i]["DeliveryDate"].ToString();
                    sheet[ROW, ColSO].Text = data.Rows[i]["SO"].ToString();
                    sheet[ROW, ColProductCode].Text = data.Rows[i]["ProductCode"].ToString();
                    sheet[ROW, ColPO].Text = data.Rows[i]["PO"].ToString();
                    sheet[ROW, ColLotNo].Text = data.Rows[i]["LotNo"].ToString();
                    sheet[ROW, ColProduct].Text = data.Rows[i]["Product"].ToString();
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString(); 
                    sheet[ROW, ColMaterial].Text = data.Rows[i]["Material"].ToString(); 
                    sheet[ROW, ColMaterialCode].Text = data.Rows[i]["MaterialCode"].ToString(); 
                    sheet[ROW, ColMasterOrderNo].Text = data.Rows[i]["MasterOrderNo"].ToString();
                    sheet[ROW, ColCustomer].Text = data.Rows[i]["Customer"].ToString(); 
                    sheet[ROW, ColOrderStatus].Text = data.Rows[i]["OrderStatus"].ToString();
                    sheet[ROW, ColSOQty].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["SOQty"].ToString());


                    ColP = 8 ;
                    foreach (var p in ExtraColumns)
                    {
                        
                        sheet[ROW, ColP].Text = data.Rows[i][p].ToString();
                        ColP = ColP + 1;
                    }
                    
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                    ROW++;

                }
                sheet.Range[startRow, ColSOQty, ROW, ColSOQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                endRow = ROW - 1;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "For Users", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);

            }
            else {
                #region Headers

                

                //report.SetHeaderText(ref sheet, ROW, COL, "PO", 13, ExcelHAlign.HAlignCenter);
                //int ColPO = COL;
                //COL++;

                //report.SetHeaderText(ref sheet, ROW, COL, "LotNo", 13, ExcelHAlign.HAlignCenter);
                //int ColLotNo = COL;
                //COL++;


                report.SetHeaderText(ref sheet, ROW, COL, "ProductCode", 14, ExcelHAlign.HAlignCenter);
                int ColProductCode = COL;
                COL++;

                int ColP = 0;
                foreach (var p in ExtraColumns)
                {
                    report.SetHeaderText(ref sheet, ROW, COL, p, 18, ExcelHAlign.HAlignCenter);
                    ColP = COL;
                    COL++;
                }

                report.SetHeaderText(ref sheet, ROW, COL, "Production Status", 14, ExcelHAlign.HAlignCenter);
                int ColOrderStatus = COL;
                
                //report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 13, ExcelHAlign.HAlignCenter);
                //int ColRemarks = COL;
                ROW++;
                endCol = COL;


                #endregion Headers


                var startRow = 0;
                var endRow = 0;
                int RowIndex = ROW;
                startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColProductCode].Text = data.Rows[i]["ProductCode"].ToString();
                    //sheet[ROW, ColPO].Text = data.Rows[i]["PO"].ToString();
                    //sheet[ROW, ColLotNo].Text = data.Rows[i]["LotNo"].ToString();
                    ColP = 2;
                    foreach (var p in ExtraColumns)
                    {
                        sheet[ROW, ColP].Text = data.Rows[i][p].ToString();
                        ColP = ColP + 1;
                    }

                    sheet[ROW, ColOrderStatus].Text = data.Rows[i]["OrderStatus"].ToString();
                    //  sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                    ROW++;

                }
                endRow = ROW - 1;


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Weighing Scale", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                                
            }
            return workbook;
        }

    }
}
 
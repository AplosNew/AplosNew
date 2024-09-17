#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.OrderManagement.Sales;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.SalesManagements.Controllers
{
    public class SalesChalanController : BaseController
    {

        string TableName = "dbo.SalesChalan";

        #region Constructor
        clsSales clsSales = new clsSales();
        private readonly ISqlRepository _sqlRepository;
        public SalesChalanController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult SalesChalanCheck()
        {
            return View();
        }
        public ActionResult DispatchConfirmation()
        {
            return View();
        }



        [HttpGet, Authorize]
        public ActionResult GetSalesChalanReportPdf(ReportFormat reportFormat, string masterId)
        {
            try
            {
                string fileName = "";

                IWorkbook workbook = GetSalesChalanWorkbook("MaterialIssue", masterId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "SalesChalanReport";
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

        public IWorkbook GetSalesChalanWorkbook(string SheetName, string masterId)
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
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder;
                clsSales.GetSalesChalanReportData(masterId, out dtOrder);

                if (dtOrder.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }

                DataTable uniq_Cols = dtOrder.DefaultView.ToTable(true, "InvoiceId");
                var NoOfInv = uniq_Cols.Rows.Count;

                int ROW = 6; int COL = 1;
                sheet.Range[ROW, COL].Text = "VechileNo.:";
                sheet.Range[ROW, 2].Text = dtOrder.Rows[0]["VechileNo"].ToString();
                sheet.Range[ROW, 2].ColumnWidth = 30;
                sheet.Range[ROW, 2].WrapText = true;
                sheet.Range[ROW, 3].Text = "GatePassDate: ";
                sheet.Range[ROW, 4].Text = dtOrder.Rows[0]["GatePassDate"].ToString();

                sheet.Range[ROW, 5].Text = "GatePassNo:";
                sheet.Range[ROW, 6].Text = dtOrder.Rows[0]["UserRef"].ToString();

                ROW = 7; COL = 1;
                sheet.Range[ROW, COL].Text = "Checked Status:";
                sheet.Range[ROW, 2].Text = dtOrder.Rows[0]["CheckedStatus"].ToString();

                sheet.Range[ROW, 3].Text = "Approve Status:";
                sheet.Range[ROW, 4].Text = dtOrder.Rows[0]["ApprovedStatus"].ToString();

                ROW++;
                ROW++;
                #region ColumnsHeader

                sheet[ROW, COL].Text = "InvoiceNo"; sheet[ROW, COL].ColumnWidth = 14; int colIN = COL; COL++;
                sheet[ROW, COL].Text = "Customer Name"; sheet[ROW, COL].ColumnWidth = 30; int colCN = COL; COL++;
                sheet[ROW, COL].Text = "No.of Bag"; sheet[ROW, COL].ColumnWidth = 15; int colPackage = COL; COL++;
                sheet[ROW, COL].Text = "Net Weight"; sheet[ROW, COL].ColumnWidth = 15; int colNW = COL; COL++;
                sheet[ROW, COL].Text = "Gross Weight"; sheet[ROW, COL].ColumnWidth = 15; int colGW = COL; COL++;
                sheet[ROW, COL].Text = "Invoice Date"; sheet[ROW, COL].ColumnWidth = 14; int colID = COL; COL++;
                sheet[ROW, COL].Text = "Destination"; sheet[ROW, COL].ColumnWidth = 19; int colDN = COL;

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
                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    sheet[ROW, colIN].Text = dtOrder.Rows[i]["InvoiceId"].ToString();
                    sheet[ROW, colCN].Text = dtOrder.Rows[i]["Customer"].ToString();
                    sheet[ROW, colPackage].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["NoOfPackage"].ToString());
                    sheet[ROW, colNW].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["NetWeight"].ToString());
                    sheet[ROW, colGW].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["GrossWeight"].ToString());

                    sheet[ROW, colID].Text = dtOrder.Rows[i]["InvoiceDate"].ToString();
                    sheet[ROW, colDN].Text = dtOrder.Rows[i]["Destination"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }


                #endregion
                int edCRow = ROW;
                sheet.Range[edCRow, 2].Text = "No Of Invoice: " + NoOfInv;
                sheet.Range[edCRow, 2].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 1, edCRow, 2].Merge();

                sheet.Range[edCRow, 3].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(NoOfPackage)", null));
                sheet.Range[edCRow, 3].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 3].CellStyle.Font.Bold = true;

                sheet.Range[edCRow, 3].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(NoOfPackage)", null));
                sheet.Range[edCRow, 3].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 3].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 3, edCRow, 3].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 3, edCRow, 3].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 4].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(NetWeight)", null));
                sheet.Range[edCRow, 4].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 4].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 4, edCRow, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 4, edCRow, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 5].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(GrossWeight)", null));
                sheet.Range[edCRow, 5].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 5].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 5, edCRow, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 5, edCRow, 5].HorizontalAlignment = ExcelHAlign.HAlignRight; ;

                sheet.Range[edCRow, 1, edCRow, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[edCRow, 1, edCRow, endCol].BorderInside(ExcelLineStyle.Hair);

                edCRow++;
                edCRow++;
                edCRow++;
                edCRow++;
                edCRow++;
                edCRow++;

                //sheet.Range[edCRow, 7].Text = "Authorized Signatory";


                sheet.Range[edCRow - 1, 1].Text = dtOrder.Rows[0]["AddedBy"].ToString();
                sheet.Range[edCRow, 1].Text = "Parepared By";
                if (dtOrder.Rows[0]["CheckedStatus"].ToString() == "Checked")
                {
                    sheet.Range[edCRow - 1, 3].Text = dtOrder.Rows[0]["CheckedBy"].ToString();
                }
                sheet.Range[edCRow, 3].Text = "Checked By";
                if (dtOrder.Rows[0]["ApprovedStatus"].ToString() == "Approved")
                {
                    sheet.Range[edCRow - 1, 5].Text = dtOrder.Rows[0]["ApprovedBy"].ToString();
                }
                sheet.Range[edCRow, 5].Text = "Approved By";

                if (Convert.ToBoolean(dtOrder.Rows[0]["IsDispatchConfirmation"].ToString()) == true)
                {
                    sheet.Range[edCRow - 1, 7].Text = dtOrder.Rows[0]["DispatchConfirmationBy"].ToString();
                }
                sheet.Range[edCRow, 7].Text = "Dispatch Confirmed By";

                #region ReportHeader

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "GATE PASS", identity.PlantId);

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);

                sheet.IsGridLinesVisible = false;
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



        [HttpGet, Authorize]
        public ActionResult GetVehicleNoCbo(string fromDate, string toDate)
        {
            try
            {
                return Json(clsSales.GetVehicleNoCbo(fromDate, toDate), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTransportDriverNo(string TransportVehicleNo)
        {
            try
            {
                return Json(clsSales.GetTransportDriverNo(TransportVehicleNo), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            JsonResult json = Json(clsSales.GetSalesChalan(column, value), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetInvoiceData(string fromDate, string toDate, string vehicleno)
        {
            try
            {
                JsonResult json = Json(clsSales.GetInvoiceData(fromDate, toDate, vehicleno), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetInvoiceDataByChalan(string masterId)
        {
            try
            {
                return Json(clsSales.GetInvoiceDataByChalan(masterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json("GP-" + DateTime.Now.ToString("yy") + GetSequence(), JsonRequestBehavior.AllowGet);
        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT COUNT(Id)C from[dbo].[SalesChalan]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["C"].ToString()) + 1;

            return 1;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> details)
        {
            try
            {
                DataSet dsMaster, dsChild;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                string _cId = "";
                bplib.clsGenID genid = new bplib.clsGenID();
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
                    data["CheckedStatus"] = "To Be Check";
                    data["IsDispatchConfirmation"] = 0;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                #region SalesChalanDetail 

                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalesChalanDetail WHERE  SalesChalanId='" + data["Id"] + "'", out dsChild, false, "1");

                if (details != null)
                {
                    genid.GenID("SalesChalanDetail", out _cId);
                    int c = 0;
                    foreach (var item in details)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            c++;
                            item["Id"] = _cId + " - " + c;
                            item["SalesChalanId"] = data["Id"];

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsChild);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateCheckBy(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    // data["CheckedByStatus"] = "Checked";
                    data["ApprovedStatus"] = "To Be Approve";
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateApproveBy(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    data["ApprovedStatus"] = "Approved";
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            string sql = @"select * from '" + TableName + "' where Id = '" + id + "'";

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpGet, Authorize]
        public ActionResult GetUncheckedData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsSales.GetUncheckedSalesChalanData(identity.EmployeeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetcheckedData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsSales.GetcheckedSalesChalanData(identity.EmployeeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetApproveBycheckedData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsSales.GetApproveBycheckedSalesChalanData(identity.EmployeeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetcheckedDataList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsSales.GetcheckedSalesChalanDataList(identity.EmployeeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesChalanCheckedByCboList()
        {
            return Json(clsSales.GetSalesChalanCheckedByCboList(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSalesChalanApproveByCboList()
        {
            return Json(clsSales.GetSalesChalanApproveByCboList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetApproveByDataForDispatchConfirmation()
        {
            return Json(clsSales.GetApproveByDataForDispatchConfirmation(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetApproveByDataForDispatchConfirmed()
        {
            return Json(clsSales.GetApproveByDataForDispatchConfirmed(), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateDispatchConfirmData(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {

                    data["IsDispatchConfirmation"] = 1;
                    data["DispatchConfirmationBy"] = identity.Name;
                    data["DispatchConfirmationDate"] = System.DateTime.Now.ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

    }
}
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
using Library.OrderManagement.Sales;
using Library.ViewModel.Vouchers;
using Library.ViewModel.SalesManagements;
using Library.Model.SalesManagements;
using Library.Service.SalesManagements;
using Library.Model.Inventory;
using Library.Model.Enums;
using Syncfusion.ExcelToPdfConverter;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class PackingInvoiceController : BaseController
    {
        private readonly ISalesService _salesService;
        PackingData det = new PackingData();
        clsSales clsSales = new clsSales();
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PackingInvoiceController(ISalesService salesService, ISqlRepository R)
        {
            _salesService = salesService;
            _sqlRepository = R;
            det = new PackingData();
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsSales.GetPackingSalesList(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesMaterialData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsSales.GetPackingSalesMaterialData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPackingSOData(string PackingId)
        {
            return Json(clsSales.GetPackingSOData(PackingId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPackingData()
        {
            return Json(det.GetPackingData(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSalesPackingData(string salesId)
        {
            return Json(clsSales.GetSalesPackingData(salesId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedPackingList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            DataSet dsDetail;
            DataSet dsHistory, dsScanData, dsItemScanData;
            if (salesMaterialVMList != null)
            {
                foreach (var item in salesMaterialVMList)
                {
                    if (item.MaterialMasterId == null)
                        throw new CustomException("Please Select Material !");
                    if (item.TransactionAmount == 0)
                        throw new CustomException("Please Input Amount !");
                    if (item.TransactionQty == 0)
                        throw new CustomException("Please Input Quantity !");
                }
            }
            if (salesServiceVMList != null)
            {
                foreach (var item in salesServiceVMList)
                {
                    if (item.ServiceMasterId == null)
                        throw new CustomException("Please Select Service !");
                    if (item.Amount == 0)
                        throw new CustomException("Please Input Service Amount !");
                }
            }
            string PackingId = "";
            if (selectedPackingList != null)
            {
                foreach (var item in selectedPackingList)
                {
                    var data = clsSales.GetQtyAmountByPackingId(item.PackingId);
                    item.Qty = Convert.ToDecimal(data["Qty"].ToString());
                    item.Amount = Convert.ToDecimal(data["Amount"].ToString());
                    item.ProductLibraryId = data["ProductLibraryId"].ToString();

                    if (PackingId == "")
                    {
                        PackingId = "'" + item.PackingId + "'";
                    }
                    else
                    {
                        PackingId += ",'" + item.PackingId + "'";
                    }
                }
            }
            GetIssueDetail(PackingId, out dsDetail);
            GetIssueHistory(PackingId, out dsHistory);
            GetItemScanChildData(PackingId, out dsItemScanData);


            _salesService.PackingInvoiceInsert(voucherVM, salesMaterialVMList, selectedPackingList, salesServiceVMList, dsDetail, dsHistory, dsItemScanData);



            return Json(new { Data = voucherVM, Message = AplosMessage.Insert + "Invoice No: " + voucherVM.Id + "" });
        }

        public void GetIssueHistory(string packingid, out DataSet dsRef)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                string sql = @"select RD.Id InventoryReceiveDetailId,RD.TransactionQty Qty,RD.MaterialTranRate,RD.TotalMaterialTranAmount TotalAmount,RD.BooksCurrencyBaseRate,RD.TotalMaterialBooksCurrencyAmount
								,PLI.PackingId,RD.MaterialTranRate
								from TRN.InventoryReceiveDetail RD
								left join(Select distinct InventoryReceiveDetailId,PackingId from dbo.ItemScanChild) ISC ON ISC.InventoryReceiveDetailId=RD.Id
								 JOIN TRN.POLotReference POR ON ISC.PackingId=POR.Id
								 JOIN TRN.PackingLineItem PLI ON POR.PackingLineItemId=PLI.PackingLineItemId
								Where PLI.PackingId IN(" + packingid + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetItemScanChildData(string packingid, out DataSet dsRef)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                string sql = @"Select * from trn.PackingLineItem  PLI
LEFT JOIN 
(							
Select SC.Id,SC.MasterId,ISNULL(sc.NetWeight,0) Qty,PackingLineItemId from trn.POLotReference po
left join dbo.ItemScanChild sc on sc.PackingId = po.Id AND Booked = 1 
Where SC.Id<>''
)POLR ON POLR.PackingLineItemId=PLI.PackingLineItemId
								Where PLI.PackingId IN(" + packingid + ") AND Id<>''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetIssueDetail(string packingid, out DataSet dsRef)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                string sql = @"select RD.InventoryMaterialId,SUM(RD.TransactionQty)TransactionQty,PolicyRate=SUM(RD.TotalMaterialTranAmount)/SUM(RD.TransactionQty),PolicyAmount=SUM(RD.TotalMaterialTranAmount)
                                    ,PLI.PackingId,RD.TransactionUoMId,RD.BaseUOMId
                                    from TRN.InventoryReceiveDetail RD
                                    left join(Select distinct InventoryReceiveDetailId,PackingId from dbo.ItemScanChild) ISC ON ISC.InventoryReceiveDetailId=RD.Id
                                    LEFT JOIN TRN.POLotReference POR ON ISC.PackingId=POR.Id
                                    LEFT JOIN TRN.PackingLineItem PLI ON POR.PackingLineItemId=PLI.PackingLineItemId
								Where PLI.PackingId IN(" + packingid + ") GROUP BY RD.InventoryMaterialId,PLI.PackingId,RD.TransactionUoMId,RD.BaseUOMId";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Edit(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedPackingList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            string PackingId = "";
            DataSet dsItemScanData;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            if (salesMaterialVMList != null)
            {
                foreach (var item in salesMaterialVMList)
                {
                    if (item.MaterialMasterId == null)
                        throw new CustomException("Please Select Material !");
                    if (item.TransactionAmount == 0)
                        throw new CustomException("Please Input Amount !");
                    if (item.TransactionQty == 0)
                        throw new CustomException("Please Input Quantity !");
                }
            }
            if (salesServiceVMList != null)
            {
                foreach (var item in salesServiceVMList)
                {
                    if (item.ServiceMasterId == null)
                        throw new CustomException("Please Select Service !");
                    if (item.Amount == 0)
                        throw new CustomException("Please Input  Service Amount !");
                }
            }

            if (selectedPackingList != null)
            {
                foreach (var item in selectedPackingList)
                {
                    var data = clsSales.GetQtyAmountByPackingId(item.PackingId);
                    item.Qty = Convert.ToDecimal(data["Qty"].ToString());
                    item.Amount = Convert.ToDecimal(data["Amount"].ToString());
                    item.ProductLibraryId = data["ProductLibraryId"].ToString();

                    if (PackingId == "")
                    {
                        PackingId = "'" + item.PackingId + "'";
                    }
                    else
                    {
                        PackingId += ",'" + item.PackingId + "'";
                    }
                }
            }
            GetItemScanChildData(PackingId, out dsItemScanData);
            _salesService.PackingInvoiceUpdate(voucherVM, salesMaterialVMList, selectedPackingList, salesServiceVMList, dsItemScanData);
            return Json(new { Data = voucherVM, Message = AplosMessage.Updated + "Invoice No: " + voucherVM.Id + "" });
        }

        [HttpPost]
        public JsonResult Delete(string Id)
        {
            DeleteData(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string id)
        {
            string strSQL, strPSQL, strBSQL, strOSQL, strSSQL, strASQL, strPISQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strOSQL = "DELETE FROM TRN.SalesTax WHERE SalesId='" + id + "'";
                strASQL = "DELETE FROM TRN.SalesAdditionalTax WHERE SalesId='" + id + "'";
                strSSQL = "DELETE FROM TRN.SalesService WHERE SalesId='" + id + "'";
                strPSQL = "DELETE FROM dbo.SalesPacking WHERE SalesId='" + id + "'";
                strBSQL = "DELETE FROM TRN.SalesMaterial WHERE SalesId='" + id + "'";
                strPISQL = "DELETE FROM [dbo].[PostSalesInvoice] WHERE SalesId='" + id + "'";
                strSQL = "DELETE FROM TRN.Sales WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strASQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strPSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strPISQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost]
        public JsonResult DeleteTaxRow(string Id)
        {
            _salesService.DeleteTaxRow(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public JsonResult DeleteServiceTaxRow(string Id)
        {
            _salesService.DeleteServiceTaxRow(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteSalesMaterial(string Id)
        {
            _salesService.DeleteSalesMaterial(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteSalesService(string Id)
        {
            _salesService.DeleteSalesService(Id);

            return Json(new { Message = AplosMessage.Deleted });
        }

        private string GetAdditionalInfoPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ContractTermsAndConditions", out sID);
            return sID;
        }

        [HttpGet, Authorize]
        public ActionResult GetAdditionalInfoList(string salesId)
        {
            string sql = @"SELECT CT.*,TC.Sequence,TC.Code,TC.ShortName,TC.StandardName,TC.UserName,TC.Description  FROM [dbo].[CommercialInvoiceAdditionalInfo] CT
                            LEFT JOIN dbo.CommercialAdditionalInfo TC ON TC.Id=CT.AdditionalInfoId
                            WHERE CT.SalesId='" + salesId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateAdditionalInfo(List<Dictionary<string, object>> data, string salesId)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.CommercialInvoiceAdditionalInfo where  SalesId='" + salesId + "'", out dsChild, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetAdditionalInfoPK();

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
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

        [Authorize, HttpPost]
        public ActionResult DeleteCommercialInvoiceAdditionalInfo(string id)
        {
            DeleteCommercialInvoiceAdditionalInfoData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteCommercialInvoiceAdditionalInfoData(string id)
        {
            string strSQL, strDSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[CommercialInvoiceAdditionalInfo] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function


        [HttpGet, Authorize]
        public ActionResult GetLotWiseSalesReportPdf(ReportFormat reportFormat, string masterId)
        {
            try
            {
                string fileName = "";

                IWorkbook workbook = GetMaterialIssueWorkbook("MaterialIssue", masterId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "MaterialIssueReport";
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

        public IWorkbook GetMaterialIssueWorkbook(string SheetName, string masterId)
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
                DataTable dtOrder = null;
                GetLotWiseSalesReportData(masterId, out dtOrder);

                if (dtOrder.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }
                int ROW = 6; int COL = 1;
                sheet.Range[ROW, COL].Text = "SlipNo. :";
                sheet.Range[ROW, COL + 1].Text = dtOrder.Rows[0]["IssueSlipId"].ToString();
                sheet.Range[ROW, COL + 2].Text = "Date" + ": " + dtOrder.Rows[0]["AddedDate"].ToString();
                sheet.Range[ROW, COL + 2].ColumnWidth = 14;
                sheet.Range[ROW, COL + 3].Text = "Customer: " + dtOrder.Rows[0]["Customer"].ToString();
                sheet.Range[ROW, COL + 3, ROW, COL + 5].Merge();

                sheet.Range[ROW, COL + 6].Text = "P.Code." + ": " + dtOrder.Rows[0]["Code"].ToString();

                sheet.Range[ROW, COL + 7].Text = "Checked Status" + ": " + dtOrder.Rows[0]["CheckedByStatus"].ToString();
                sheet.Range[ROW, COL + 7, ROW, COL + 11].Merge();

                sheet.Range[ROW, 1, ROW + 1, 11].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW + 1, 11].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW + 1, 11].BorderInside(ExcelLineStyle.Hair);


                ROW = 7; COL = 1;
                sheet.Range[ROW, COL].Text = "PO No. :";
                sheet.Range[ROW, COL + 1].Text = dtOrder.Rows[0]["POId"].ToString();
                sheet.Range[ROW, COL + 2].Text = "Cost Center" + ": " + dtOrder.Rows[0]["CostCenter"].ToString();
                sheet.Range[ROW, COL + 3].Text = "Order Qty" + ": " + dtOrder.Rows[0]["SOQty"].ToString() + " " + dtOrder.Rows[0]["UoM"].ToString();
                sheet.Range[ROW, COL + 4].Text = "Plan %" + ": " + dtOrder.Rows[0]["PlanPercentage"].ToString() + "%";
                sheet.Range[ROW, COL + 4, ROW, COL + 5].Merge();
                sheet.Range[ROW, COL + 6].Text = "Shade" + ": " + dtOrder.Rows[0]["Shade"].ToString();
                sheet.Range[ROW, COL + 6].ColumnWidth = 14;
                sheet.Range[ROW, COL + 7].Text = "Approved Status: " + dtOrder.Rows[0]["AuthorizedByStatus"].ToString();
                sheet.Range[ROW, COL + 7, ROW, COL + 11].Merge();

                sheet.Range[ROW, 1, ROW + 1, 11].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW + 1, 11].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW + 1, 11].BorderInside(ExcelLineStyle.Hair);

                sheet.Range[8, 1, 8, COL + 11].Merge();
                ROW = 8; COL = 1;
                ROW++;
                #region ColumnsHeader

                sheet[ROW, COL].Text = "SL"; sheet[ROW, COL].ColumnWidth = 8; int colSL = COL; COL++;
                sheet[ROW, COL].Text = "Description"; sheet[ROW, COL].ColumnWidth = 16; int colDescription = COL; COL++;
                sheet[ROW, COL].Text = "Packing Type"; sheet[ROW, COL].ColumnWidth = 16; int colPackingType = COL; COL++;
                sheet[ROW, COL].Text = "Master Order Item"; sheet[ROW, COL].ColumnWidth = 25; int colMOI = COL; COL++;
                sheet[ROW, COL].Text = "Article"; sheet[ROW, COL].ColumnWidth = 30; int colArticle = COL; COL++;
                sheet[ROW, COL].Text = "%Age"; sheet[ROW, COL].ColumnWidth = 14; int colAge = COL; COL++;
                sheet[ROW, COL].Text = "Value Loss"; sheet[ROW, COL].ColumnWidth = 19; int colVL = COL; COL++;
                sheet[ROW, COL].Text = "UOM"; sheet[ROW, COL].ColumnWidth = 8; int colUoM = COL; COL++;
                sheet[ROW, COL].Text = "Total Qty"; sheet[ROW, COL].ColumnWidth = 8; int colTQ = COL; COL++;
                sheet[ROW, COL].Text = "Plan Qty"; sheet[ROW, COL].ColumnWidth = 8; int colPQ = COL; COL++;
                sheet[ROW, COL].Text = "Issued Qty"; sheet[ROW, COL].ColumnWidth = 8; int colIQ = COL; COL++;
                sheet[ROW, COL].Text = "Balance Qty"; sheet[ROW, COL].ColumnWidth = 8; int colBQ = COL;

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
                    sheet[ROW, colSL].Text = dtOrder.Rows[i]["SrNo"].ToString();
                    sheet[ROW, colDescription].Text = dtOrder.Rows[i]["Remarks"].ToString();
                    sheet[ROW, colPackingType].Text = dtOrder.Rows[i]["PackingType"].ToString();
                    sheet[ROW, colMOI].Text = dtOrder.Rows[i]["MaterialMaster"].ToString();
                    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["QBOQArticle"].ToString();
                    sheet[ROW, colArticle].RowHeight = 20;
                    sheet[ROW, colAge].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["GrossConsumption"].ToString());
                    sheet.Range[ROW, colAge].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[ROW, colAge].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet[ROW, colVL].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ValueLoss"].ToString());
                    sheet[ROW, colUoM].Text = dtOrder.Rows[i]["UOM"].ToString();
                    sheet[ROW, colTQ].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["TotalConsumption"].ToString());
                    sheet.Range[ROW, colTQ].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[ROW, colTQ].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet[ROW, colPQ].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["IssueQty"].ToString());
                    sheet.Range[ROW, colPQ].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[ROW, colPQ].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet[ROW, colIQ].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ActualIssue"].ToString());
                    sheet.Range[ROW, colIQ].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[ROW, colIQ].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet[ROW, colBQ].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["Balance"].ToString());
                    sheet.Range[ROW, colBQ].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[ROW, colBQ].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                #endregion
                int edCRow = ROW;
                sheet.Range[edCRow, 5].Text = "TOTAL";
                sheet.Range[edCRow, 5].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 1, edCRow, 5].Merge();
                sheet.Range[edCRow, 6].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(GrossConsumption)", null));
                sheet.Range[edCRow, 6].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 6].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 6, edCRow, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 6, edCRow, 6].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 7].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(ValueLoss)", null));
                sheet.Range[edCRow, 7].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 7].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 7, edCRow, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 7, edCRow, 7].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 9].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(TotalConsumption)", null));
                sheet.Range[edCRow, 9].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 9].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 9, edCRow, 9].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 9, edCRow, 9].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 10].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(IssueQty)", null));
                sheet.Range[edCRow, 10].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 10].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 10, edCRow, 10].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 10, edCRow, 10].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 11].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(ActualIssue)", null));
                sheet.Range[edCRow, 11].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 11].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 11, edCRow, 11].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 11, edCRow, 11].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 12].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Compute("SUM(Balance)", null));
                sheet.Range[edCRow, 12].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[edCRow, 12].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 12, edCRow, 12].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[edCRow, 12, edCRow, 12].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[edCRow, 1, edCRow, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[edCRow, 1, edCRow, endCol].BorderInside(ExcelLineStyle.Hair);

                edCRow++;
                edCRow++;
                edCRow++;
                edCRow++;
                edCRow++;
                edCRow++;

                sheet.Range[edCRow - 1, 3].Text = dtOrder.Rows[0]["AddedBy"].ToString();
                sheet.Range[edCRow, 3].Text = "PareparedBy";
                sheet.Range[edCRow - 1, 5].Text = dtOrder.Rows[0]["CheckedBy"].ToString();
                sheet.Range[edCRow, 5].Text = "CheckedBy";
                sheet.Range[edCRow - 1, 7].Text = dtOrder.Rows[0]["AuthorizedBy"].ToString();
                sheet.Range[edCRow, 7].Text = "AuthorizedBy";

                #region ReportHeader
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Material Issue Report", identity.PlantId);
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

        public void GetLotWiseSalesReportData(string SalesId, out DataTable dtOrder)
        {
            try
            {
                string strSql = string.Empty;
                strSql = @"SELECT IR.Id CustomerNo
   
    ,CRNC.Code
    ,p.UserName Customer
    ,P.UserName Buyer
 
    ,P.TINNO CustomerGSTNo
    ,p.VATResistrationNo AS CustomerPANNo
    ,Addres.Address1 VendorAddress
    ,ISNULL(HSNC.Code,MHSN.Code) HSNCode
    ,Plant.GSTIN
    ,Plant.VATResistrationNo AS PlantPANNo
    ,DPARTYPL.GSTIN ShipGSTIN
    ,INVPARTYPL.GSTIN BillGSTIN
    ,IR.DocRefNo
    ,IR.InvoiceNo
    ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
    ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
    ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
    ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
   
    ,INVPARTYPL.UserName InvoiceParty
    ,INVPARTYPL.UserName InvoiceParty2
    ,IR.InvoicingByAddress AS ConsigneeAddress
    ,IR.DeliveryByAddress
    ,DPARTYPL.UserName DeliveryParty
 
    ,PSI.PreCarriageBy
    ,PSI.PlaceOfReceiptByPreCarriage
    ,PSI.CNFContainerNo
    ,PSI.CNFVesselName
    ,PSI.CNFVesselTrackingNo
    ,D.UserName AS FinalDestination
    ,PL.UserName AS PortOfLanding
    ,PD.UserName AS PortOfDischarge
    ,PoD.UserName AS PortOfDelivery
    ,CRNC.Code AS CurrencyName
    ,IR.ToCurrencyRate
    ,BASECRNC.Code AS BaseCurrencyName
    ,PayTerm.UserName PaymentTerm
    ,MM.UserName MaterialMaster
    ,MGM.UserName MaterialGroupMaster
    ,MMA.StandardName Article
    ,FC.UserName FirstChar
    ,FCV.UserName AS FirstCharacteristicsValue
    ,SCV.UserName AS SecondCharacteristicsValue
    ,TCV.UserName AS ThirdCharacteristicsValue
    ,SC.UserName SecondChar
    ,TC.UserName ThirdChar
	,SCN.NetWeight POTransactionQty
    ,ROUND(IRD.TransactionRate, 4) TransactionRate
	,ROUND((SCN.NetWeight * IRD.TransactionRate), 2) AS TrnAmount
	  ,ROUND((SCN.NetWeight * IRD.TransactionRate), 2) AS BaseAmount
   
    ,TUoM.UserName AS TransactionUoM
    ,PONumber = REPLACE(REPLACE(STUFF((
                    SELECT DISTINCT ', ' + CPO.PONumber
                    FROM TRN.SalesMaterial SM
                    JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId
                    JOIN TRN.CustomerPO CPO ON CPO.id = SO.CustomerPOId
                    WHERE IR.Id = SM.SalesId
                    FOR XML path('')
                        ,TYPE
                    ).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
    ,OurOrderRefNo = REPLACE(REPLACE(STUFF((
                    SELECT DISTINCT ', ' + MO.OwnReferenceNo
                    FROM TRN.SalesOrderItem SOI
                    JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                    WHERE IR.Id = SOI.SalesId
                    FOR XML path('')
                        ,TYPE
                    ).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
    ,YourOrderRefNo = REPLACE(REPLACE(STUFF((
                    SELECT DISTINCT ', ' + MOI.BuyerReferenceNo
                    FROM TRN.SalesOrderItem SOI
                    --JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                    JOIN TRN.MasterOrderItem MOI ON MOI.Id = SOI.MasterOrderItemId
                    WHERE IR.Id = SOI.SalesId
                    FOR XML path('')
                        ,TYPE
                    ).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
    ,AddedDate = REPLACE(REPLACE(STUFF((
                    SELECT DISTINCT ', ' + FORMAT(MO.AddedDate, 'dd-MMM-yyyy')
                    FROM TRN.SalesOrderItem SOI
                    JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                    WHERE IR.Id = SOI.SalesId
                    FOR XML path('')
                        ,TYPE
                    ).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
    ,IR.ComercialInvoiceNo
    ,IR.BLNumber
    ,FORMAT(IR.BLDate, 'dd-MMM-yyyy') BLDate
    ,IR.EXPFromNo
    ,FORMAT(IR.EXPDate, 'dd-MMM-yyyy') EXPDate
    ,IR.ItemDescription
    ,PSI.TransportVehicleNo
    ,PSI.TransportDriverName
    ,PPSI.UserName TransporterName
    ,BM.AccountTitle
    ,BM.AccountNumber
    ,BMA.Address1
    ,PSI.TransportDocRefNo
    ,FORMAT(PSI.TransportDocDate, 'dd-MMM-yyyy') CNFBLAWBDate
    ,B.UserName AS Bank
    ,BB.UserName AS BankBranch
	  ,ROUND((SCN.NetWeight * IRD.TransactionRate*IR.ToCurrencyRate), 2) AS BooksCurrencyTransactionAmount
    ,IRD.BooksCurrencyTransactionAmount
    ,IRD.BooksCurrencyTaxAmount
    ,IRD.BooksCurrencyBaseRate
    ,(
        SELECT Stuff((
                    SELECT ',' + pla.AttributeValue
                    FROM dbo.ProductLibraryAttribute pla
                    WHERE pla.ProductLibraryId = MOI.ProductLibraryId
                    FOR XML PATH('')
                    ), 1, 1, '')
        ) AS ProdDetails,IR.AddedBy CreatedBy
        , SCN.Bags, SCN.LotNo, SCN.GWeight, MO.Type
FROM TRN.Sales IR
LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = IR.Id
LEFT JOIN MST.[Port] AS PL ON PL.Id = PSI.PortOfLoadingId
LEFT JOIN MST.[Port] AS PD ON PD.Id = PSI.PortOfDischargeId
LEFT JOIN MST.[Port] AS PoD ON PoD.Id = PSI.PortOfDelivaryId
LEFT JOIN MST.Destination AS D ON D.Id = PSI.FinalDestinationId
LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
LEFT JOIN [MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id

left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
left join ProductLibrary PLA on PLA.Id = MOI.ProductLibraryId
LEFT JOIN (Select SalesId,SalesMaterialId, ProductCode, LotNo, COUNT(RefNo) Bags, 
            SUM(NetWeight)NetWeight,SUM(GWeight)GWeight from ItemScanChild group by SalesId ,SalesMaterialId, ProductCode, LotNo) SCN on SCN.SalesId = IR.Id AND SCN.SalesMaterialId=IRD.Id and SCN.ProductCode = PLA.Code

LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
LEFT JOIN [HKP].[HSNCode] AS MHSN ON MHSN.ID = MM.HSNCodeId
LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID = MMA.HSNCodeId
LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
LEFT JOIN HKP.Party PPSI ON PPSI.Id = PSI.TransportAgentId
LEFT JOIN MST.BankMaster BM ON BM.Id = PSI.BankMasterId
LEFT JOIN HKP.Bank B ON B.Id = BM.BankId
LEFT JOIN HKP.BankBranch BB ON BB.BankId = BM.BankId
    AND BB.Id = BM.BankBranchId
LEFT JOIN [MST].[AddressMaster] BMA ON BMA.Id = BB.AddressMasterId
WHERE IR.Id ='" + SalesId + "'";

                dtOrder = _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function


    }
}
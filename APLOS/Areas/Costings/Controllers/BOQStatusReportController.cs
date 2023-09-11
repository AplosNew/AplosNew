#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Costings;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


#endregion Using

namespace Aplos.Areas.Costings.Controllers
{
    public class BOQStatusReportController : BaseController
    {


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public BOQStatusReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult getBOQFilters()
        
        {
            try
            {
                var sql = @"  select distinct MO.Id MasterOrderId,BOM.CustomerId PartyId,PC.UserName Customer
							  ,PONo=STUFF((SELECT distinct ','+  CPO.PONumber
										from [TRN].[CustomerPO] CPO
										left join [TRN].[SalesOrder] AS SO on SO.CustomerPOId=CPO.Id
										where SO.CostingBOQMasterId=BOM.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,SOId=STUFF((SELECT distinct ','+  XITM.Id
								from trn.SalesOrder AS XITM
								where XITM.CostingBOQMasterId=BOM.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,BuyerReferenceNo=STUFF((SELECT distinct ','+  XITM.BuyerReferenceNo
								from TRN.MasterOrderItem AS XITM
								left join TRN.MasterOrder MOX on MOX.Id=XITM.MasterOrderId
								where MOX.Id=MO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,OwnReferenceNo=STUFF((SELECT distinct ','+  XITM.OwnReferenceNo
								from TRN.MasterOrderItem AS XITM
								left join TRN.MasterOrder MOX on MOX.Id=XITM.MasterOrderId
								where MOX.Id=MO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LineItemId=STUFF((SELECT distinct ','+  XITM.Id
								from TRN.MasterOrderItem AS XITM
								left join TRN.MasterOrder MOX on MOX.Id=XITM.MasterOrderId
								where MOX.Id=MO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
							  from BOQ boq 
							  left join costingboqmaster BOM on BOM.Id=boq.CostingBOQMasterId
							  left join TRN.MasterOrderItem AS moi on boq.MasterOrderItemId=moi.Id
							  left join TRN.MasterOrder MO on MO.Id=moi.MasterOrderId
							  left join HKP.Party PC on PC.Id=BOM.CustomerId

							  where BOM.CustomerId <>'' and moi.OrderCostingMasterTemplateId<>''";

                JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
                //return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Authorize, HttpPost]
        public ActionResult getBOQStatusData(Dictionary<string, string> parameters)
        {
            try
            {
                List<string> BR = parameters["BuyerReferenceNo"].Split(',').ToList();
                var ids = "";
                for (var i = 0; i < BR.Count; i++)
                {
                    if (ids == "")
                    {
                        ids =  "" + BR[i] + "";
                    }
                    else
                    {
                        ids += ",'" + BR[i] + "'";
                    }
                }

                string OwnRef = "'" + parameters["OwnReferenceNo"].Replace(" ", "','") + "'";//replaced with ""
                string BuyerRef = "" + parameters["BuyerReferenceNo"].Replace(" ", "','") + "";//replaced with ""
                string MasterOrderId = "" + parameters["MasterOrderId"].Replace(" ", "','") + ""; 
                string LineItemId = "" + parameters["LineItemId"].Replace(" ", "','") + "";//replaced with ""
                string SOId = "'" + parameters["SOId"].Replace(" ", "','") + "'";//replaced with ""
                string PONo = "'" + parameters["PONo"].Replace(" ", "','") + "'";//replaced with ""

                var sql = @"SELECT distinct boq.Id RowId,boq.[Sequence],boq.ItemRefNo,ci.UserName AS CostingItem,boq.BOQCriteria,c.Code AS Currency,p.UserName AS Vendor,mm.UserName AS Material,mma.StandardName AS Article,BOM.Id BOMId
                ,isnull(cv1.UserName,'') SKU1,isnull(cv2.UserName,'') SKU2,boq.SKUDesc,isnull(boq.POCriteria,'') POCriteria
				--,boq.Consumption
				,isnull(OPCD.GrossConsumption,0) Consumption
                ,boq.BOMQty,UOM.UserName BOQUOM,boq.BOMQtyBase,boq.RequiredQty
				,boq.Rate*BOQ.BOMQty AS BOMAmount
				,isnull(poboq.POBOQQty,0) POBOQQty,isnull(poboq.POUOM,'') POUOM,isnull(poboq.POTrnBOQQty,0) POTrnBOQQty
                ,isnull(poboq.POAmount,0) POAmount,BalanceBOQ=isnull(boq.BOMQtyBase-poboq.POBOQQty,0)
                ,isnull(grnboq.GRNBaseQty,0) GRNBaseQty
                ,isnull(grnboq.GRNAmount,0) GRNAmount
                ,isnull(grnboq.GRNUOM,'') GRNUOM
                , BalancePOQty=isnull(poboq.POBOQQty-grnboq.GRNBaseQty,0)
                ,isnull(issueboq.IssueBaseQty,0) IssueBaseQty
                ,isnull(issueboq.IssueAmount,0) IssueAmount
                , BalanceGRNQty=isnull(grnboq.GRNBaseQty-issueboq.IssueBaseQty,0)
                ,PC.Id PartyId,PC.UserName Customer
				,MO.Id MasterOrderId,MO.BuyerReferenceNo,MO.OwnReferenceNo,moi.Id LineItemId
                FROM BOQ  boq
                LEFT JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=boq.UoMId
                LEFT JOIN hkp.CostingItem AS ci ON ci.Id=boq.CostingItemId
                LEFT JOIN scs.Currency AS c ON c.Id=boq.CurrencyId
                LEFT JOIN hkp.Party AS p ON p.Id=boq.VendorId
                LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=boq.MaterialMasterId
                LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=boq.ArticleId
                LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=boq.FGFirstCharacteristicsValueId
                LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=boq.FGSecondCharacteristicsValueId
                left join costingboqmaster BOM on BOM.Id=boq.CostingBOQMasterId
                left join TRN.MasterOrderItem AS moi on boq.MasterOrderItemId = moi.Id
                left join TRN.MasterOrder MO on MO.Id = moi.MasterOrderId
                left join HKP.Party PC on PC.Id = MO.PartyId
               left join TRN.SalesOrder SO on SO.CostingBOQMasterId = BOM.Id
			   left join [TRN].[CustomerPO] CPO on CPO.MasterOrderId = MO.Id and CPO.Id=SO.CustomerPOId
			   --new add
			   LEFT JOIN (Select DISTINCT SalesOrderId,CostingBOQMasterId,CostingItemId,OrderProcurementCostingDirectMaterialId from CostingBOQItems )CBI on CBI.CostingBOQMasterId=boq.CostingBOQMasterId AND CBI.CostingItemId=boq.CostingItemId --AND so.Id=CBI.SalesOrderId
				LEFT JOIN OrderProcurementCostingDirectMaterial OPCD on OPCD.Id=CBI.OrderProcurementCostingDirectMaterialId AND CBI.CostingItemId=OPCD.CostingItemId AND boq.CostingItemId=OPCD.CostingItemId

                left join(SELECT pomap.BOQDetailId,sum(pomap.POBOQQty) POBOQQty,sum(pomap.TransactionQty) POTrnBOQQty,UOM.UserName POUOM,SUM(pod.BaseAmount) POAmount 
                			FROM  trn.POBOQMAP pomap 
                			JOIN trn.PurchaseOrderDetail pod on pod.Id=pomap.PODetailId
                			LEFT JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=pod.TransactionUoMId
                			GROUP BY pomap.BOQDetailId,UOM.UserName
                			) poboq ON poboq.BOQDetailId=boq.Id

                left join (SELECT gpa.BOQDetailId,sum(gpa.TransactionQty) GRNBaseQty,UOM.UserName GRNUOM,sum(IRD.TotalMaterialTranAmount ) GRNAmount
                				FROM trn.GRNPORequisitionAllocation gpa 
                				JOIN trn.InventoryReceiveDetail IRD ON gpa.InventoryReceiveDetailId=IRD.Id
                				LEFT JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=IRD.TransactionUoMId
                				GROUP BY gpa.BOQDetailId,UOM.UserName
                			) grnboq ON grnboq.BOQDetailId=poboq.BOQDetailId

                left join (SELECT iihb.BOQDetailId,sum(iihb.Qty) IssueBaseQty ,sum(iihb.Qty*iih.Rate) IssueAmount
                			FROM trn.InventoryIssueHistoryBOQ iihb 
                			join TRN.InventoryIssueHistory iih on iihb.InventoryIssueHistoryId=iih.Id
                			GROUP BY iihb.BOQDetailId

                ) issueboq ON issueboq.BOQDetailId=poboq.BOQDetailId


                                              where PC.Id in(" + parameters["PartyId"] + @")
                                              AND moi.BuyerReferenceNo in(" + BuyerRef + @")
                                              AND moi.OwnReferenceNo in(" + OwnRef + @")
                                              AND MO.Id in(" + MasterOrderId + @")
                                              AND moi.Id in(" + LineItemId + @")
                                              AND SO.Id in(" + SOId + @")                                       
                                        AND CPO.PONumber in(" + PONo + @")";

                JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
                //return _sqlRepository.GetDataTable(sql);
                //return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetBOQStatusReport(List<Dictionary<string, object>> data, Dictionary<string, string> parameters, string reportFileName)
        {

            try
            {
                string fileName = "";
                fileName = GetBOQStatusReportForm(data, parameters, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private string GetBOQStatusReportForm(List<Dictionary<string, object>> data, Dictionary<string, string> parameters,string ReportHeader, string reportFileName)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var headerData = getBOQStatusReportHeaderSql(parameters);

            //var data = getBOQStatusReportSql(parameters);

            var sheet = workbook.Worksheets[0];



            int ROW = 5; int COL = 1;

            #region Header
            report.SetMasterHeaderText(ref sheet, ROW, 1, "Master Order");
            sheet[ROW, 1].ColumnWidth = 20;
            sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            report.SetText(ref sheet, ROW, 2, headerData["MasterOrderId"].ToString());
            sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
            sheet[ROW, 2].ColumnWidth = 20;
            sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            report.SetMasterHeaderText(ref sheet, ROW, 4, "Customer");
            sheet[ROW, 4].ColumnWidth = 25;
            sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            report.SetText(ref sheet, ROW, 5, headerData["PartyName"].ToString());
            sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
            sheet[ROW, 5].ColumnWidth = 30;
            sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
            ROW++;

            report.SetMasterHeaderText(ref sheet, ROW, 1, "Buyer Ref No");
            sheet[ROW, 1].ColumnWidth = 20;
            sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            report.SetText(ref sheet, ROW, 2, headerData["BuyerReferenceNo"].ToString());
            sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
            sheet[ROW, 2].ColumnWidth = 20;
            sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            report.SetMasterHeaderText(ref sheet, ROW, 4, "Buyer");
            sheet[ROW, 4].ColumnWidth = 25;
            sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            report.SetText(ref sheet, ROW, 5, headerData["BuyerName"].ToString());
            sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
            sheet[ROW, 5].ColumnWidth = 30;
            sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
            ROW++;

            report.SetMasterHeaderText(ref sheet, ROW, 1, "Contract No");
            sheet[ROW, 1].ColumnWidth = 20;
            sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            report.SetText(ref sheet, ROW, 2, headerData["OwnReferenceNo"].ToString());
            sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
            sheet[ROW, 2].ColumnWidth = 20;
            sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            report.SetMasterHeaderText(ref sheet, ROW, 4, "Product");
            sheet[ROW, 4].ColumnWidth = 25;
            sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            report.SetText(ref sheet, ROW, 5, headerData["Product"].ToString());
            sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
            //sheet[ROW, 5].ColumnWidth = 25;
            sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
            ROW++;

            report.SetMasterHeaderText(ref sheet, ROW, 1, "SO Id");
            //sheet[ROW, 1].ColumnWidth = 20;
            sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            report.SetText(ref sheet, ROW, 2, headerData["SOId"].ToString());
            sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
            sheet[ROW, 2].ColumnWidth = 20;
            sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            report.SetMasterHeaderText(ref sheet, ROW, 4, "PO No");
            //sheet[ROW, 4].ColumnWidth = 25;
            sheet.Range[ROW, 4].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            report.SetText(ref sheet, ROW, 5, headerData["PONumber"].ToString());
            sheet[report.GetColumnNameForXls(5) + ROW + ":" + report.GetColumnNameForXls(6) + ROW].Merge();
            //sheet[ROW, 5].ColumnWidth = 25;
            sheet.Range[ROW, 5].VerticalAlignment = ExcelVAlign.VAlignTop;
            ROW++;

            report.SetMasterHeaderText(ref sheet, ROW, 1, "SO Quantity");
            //sheet[ROW, 1].ColumnWidth = 20;
            sheet.Range[ROW, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            report.SetText(ref sheet, ROW, 2, headerData["SOQty"].ToString());
            sheet[report.GetColumnNameForXls(2) + ROW + ":" + report.GetColumnNameForXls(3) + ROW].Merge();
            sheet[ROW, 2].ColumnWidth = 20;
            sheet.Range[ROW, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            ROW++;
            ROW++;
            #endregion


            #region columns
            sheet[ROW, COL].Text = "Row Id";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColRowId = COL;
            COL++;

            sheet[ROW, COL].Text = "Sequence";
            sheet[ROW, COL].ColumnWidth = 8;
            int ColSequence = COL;
            COL++;

            sheet[ROW, COL].Text = "BOM Id";
            sheet[ROW, COL].ColumnWidth = 8;
            int ColBOMId = COL;
            COL++;

            sheet[ROW, COL].Text = "Item Ref No";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColItemRefNo = COL;
            COL++;

            sheet[ROW, COL].Text = "Costing Item";
            sheet[ROW, COL].ColumnWidth = 15;
            int ColCostingItem = COL;
            COL++;

            sheet[ROW, COL].Text = "BOQ Criteria";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColBOQCriteria = COL;
            COL++;

            sheet[ROW, COL].Text = "Currency";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColCurrency = COL;
            COL++;

            sheet[ROW, COL].Text = "Vendor";
            sheet[ROW, COL].ColumnWidth = 15;
            int ColVendor = COL;
            COL++;

            sheet[ROW, COL].Text = "Material";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColMaterial = COL;
            COL++;

            sheet[ROW, COL].Text = "Article";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColArticle = COL;
            COL++;

            sheet[ROW, COL].Text = "SKU1";
            sheet[ROW, COL].ColumnWidth = 10;
            int ColSKU1 = COL;
            COL++;

            sheet[ROW, COL].Text = "SKU2";
            sheet[ROW, COL].ColumnWidth = 10;
            int ColSKU2 = COL;
            COL++;

            sheet[ROW, COL].Text = "SKU Description";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColSKUDescription = COL;
            COL++;

            sheet[ROW, COL].Text = "PO Criteria";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColPOCriteria = COL;
            COL++;

            sheet[ROW, COL].Text = "Consumption";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColConsumption = COL;
            COL++;

            sheet[ROW, COL].Text = "BOM Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColBOMQty = COL;
            COL++;
            sheet[ROW, COL].Text = "BOQ UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int ColBOQUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "BOM Qty Base";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColBOMQtyBase = COL;
            COL++;
            sheet[ROW, COL].Text = "Required Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColRequiredQty = COL;
            COL++;

            sheet[ROW, COL].Text = "BOM Amount";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColBOMAmount = COL;
            COL++;

            sheet[ROW, COL].Text = "PO BOQ Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColPOBOQQty = COL;
            COL++;
            sheet[ROW, COL].Text = "PO UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int ColPOUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "PO Trn BO QQty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColPOTrnBOQQty = COL;
            COL++;

            sheet[ROW, COL].Text = "PO Amount";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColPOAmount = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance BOQ";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColBalanceBOQ = COL;
            COL++;

            sheet[ROW, COL].Text = "GRN Base Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColGRNBaseQty = COL;
            COL++;

            sheet[ROW, COL].Text = "GRN Amount";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColGRNAmount = COL;
            COL++;
            sheet[ROW, COL].Text = "GRN UOM";
            sheet[ROW, COL].ColumnWidth = 8;
            int ColGRNUOM = COL;
            COL++;
            sheet[ROW, COL].Text = "Balance PO Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColBalancePOQty = COL;
            COL++;

            sheet[ROW, COL].Text = "Issue Base Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColIssueBaseQty = COL;
            COL++;

            sheet[ROW, COL].Text = "Issue Amount";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColIssueAmount = COL;
            COL++;

            sheet[ROW, COL].Text = "Balance GRN Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColBalanceGRNQty = COL;


            #endregion columns

            int endCol = COL;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

            ROW++;

            int startRow = ROW;

            for (int i = 0; i < data.Count; i++)
            {
                sheet[ROW, ColRowId].Text = data[i]["RowId"].ToString();
                sheet[ROW, ColSequence].Text = data[i]["Sequence"].ToString();
                sheet[ROW, ColBOMId].Text = data[i]["BOMId"].ToString();
                sheet[ROW, ColItemRefNo].Text = data[i]["ItemRefNo"].ToString();
                sheet[ROW, ColCostingItem].Text = data[i]["CostingItem"].ToString();
                sheet[ROW, ColBOQCriteria].Text = data[i]["BOQCriteria"].ToString();
                sheet[ROW, ColCurrency].Text = data[i]["Currency"].ToString();
                sheet[ROW, ColVendor].Text = data[i]["Vendor"].ToString();
                sheet[ROW, ColMaterial].Text = data[i]["Material"].ToString();
                sheet[ROW, ColArticle].Text = data[i]["Article"].ToString();
                sheet[ROW, ColSKU1].Text = data[i]["SKU1"].ToString();
                sheet[ROW, ColSKU2].Text = data[i]["SKU2"].ToString();
                sheet[ROW, ColSKUDescription].Text = data[i]["SKUDesc"].ToString();
                sheet[ROW, ColPOCriteria].Text = data[i]["POCriteria"].ToString();
                sheet[ROW, ColConsumption].Number = clsStaticInfo.dbl(data[i]["Consumption"].ToString());
                sheet[ROW, ColBOMAmount].Number = clsStaticInfo.dbl(data[i]["BOMAmount"].ToString());
                sheet[ROW, ColBOMQty].Number = clsStaticInfo.dbl(data[i]["BOMQty"].ToString());
                sheet[ROW, ColBOQUOM].Text = data[i]["BOQUOM"].ToString();
                sheet[ROW, ColBOMQtyBase].Number = clsStaticInfo.dbl(data[i]["BOMQtyBase"].ToString());
                sheet[ROW, ColRequiredQty].Number = clsStaticInfo.dbl(data[i]["RequiredQty"].ToString());
                sheet[ROW, ColPOBOQQty].Number = clsStaticInfo.dbl(data[i]["POBOQQty"].ToString());
                sheet[ROW, ColPOUOM].Text = data[i]["POUOM"].ToString();
                sheet[ROW, ColPOTrnBOQQty].Number = clsStaticInfo.dbl(data[i]["POTrnBOQQty"].ToString());
                sheet[ROW, ColPOAmount].Number = clsStaticInfo.dbl(data[i]["POAmount"].ToString());
                sheet[ROW, ColBalanceBOQ].Number = clsStaticInfo.dbl(data[i]["BalanceBOQ"].ToString());
                sheet[ROW, ColGRNBaseQty].Number = clsStaticInfo.dbl(data[i]["GRNBaseQty"].ToString());
                sheet[ROW, ColGRNAmount].Number = clsStaticInfo.dbl(data[i]["GRNAmount"].ToString());
                sheet[ROW, ColGRNUOM].Text = data[i]["GRNUOM"].ToString();
                sheet[ROW, ColBalancePOQty].Number = clsStaticInfo.dbl(data[i]["BalancePOQty"].ToString());
                sheet[ROW, ColIssueBaseQty].Number = clsStaticInfo.dbl(data[i]["IssueBaseQty"].ToString());

                sheet[ROW, ColIssueAmount].Number = clsStaticInfo.dbl(data[i]["IssueAmount"].ToString());
                sheet[ROW, ColBalanceGRNQty].Number = clsStaticInfo.dbl(data[i]["BalanceGRNQty"].ToString());

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                ROW++;

            }

            //Total Start
            var endRow = ROW++;

            sheet.Range[endRow, ColRowId].Text = "Total";
            sheet.Range[endRow, ColRowId, endRow, ColPOCriteria].Merge();
            sheet.Range[endRow, ColRowId].CellStyle.Font.Bold = true;

            sheet[endRow, ColConsumption].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColConsumption) + startRow + ":" + clsStaticInfo.GetxlsCol(ColConsumption) + (ROW - 2).ToString() + ")";
            //sheet.Range[endRow, ColConsumption].Number = clsStaticInfo.dbl(data.Compute("SUM(Consumption)", null));
            sheet.Range[endRow, ColConsumption].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[endRow, ColConsumption].CellStyle.Font.Bold = true;
            sheet.Range[endRow, ColConsumption, endRow, ColConsumption].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[endRow, ColConsumption, endRow, ColConsumption].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[endRow, ColBOMQty, endRow, ColRequiredQty].Merge();

            sheet[endRow, ColBOMAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBOMAmount) + startRow + ":" + clsStaticInfo.GetxlsCol(ColBOMAmount) + (ROW - 2).ToString() + ")";
            //sheet.Range[endRow, ColBOMAmount].Number = clsStaticInfo.dbl(data.Compute("SUM(BOMAmount)", null));
            sheet.Range[endRow, ColBOMAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[endRow, ColBOMAmount].CellStyle.Font.Bold = true;
            sheet.Range[endRow, ColBOMAmount, endRow, ColBOMAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[endRow, ColBOMAmount, endRow, ColBOMAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[endRow, ColBOMAmount].CellStyle.Font.Bold = true;
            sheet.Range[endRow, ColPOBOQQty, endRow, ColPOTrnBOQQty].Merge();

            sheet[endRow, ColPOAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColPOAmount) + startRow + ":" + clsStaticInfo.GetxlsCol(ColPOAmount) + (ROW - 2).ToString() + ")";
            //sheet.Range[endRow, ColPOAmount].Number = clsStaticInfo.dbl(data.Compute("SUM(POAmount)", null));
            sheet.Range[endRow, ColPOAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[endRow, ColPOAmount].CellStyle.Font.Bold = true;
            sheet.Range[endRow, ColPOAmount, endRow, ColPOAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[endRow, ColPOAmount, endRow, ColPOAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[endRow, ColPOAmount].CellStyle.Font.Bold = true;
            sheet.Range[endRow, ColBalanceBOQ, endRow, ColGRNBaseQty].Merge();

            sheet[endRow, ColGRNAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColGRNAmount) + startRow + ":" + clsStaticInfo.GetxlsCol(ColGRNAmount) + (ROW - 2).ToString() + ")";
            //sheet.Range[endRow, ColGRNAmount].Number = clsStaticInfo.dbl(data.Compute("SUM(GRNAmount)", null));
            sheet.Range[endRow, ColGRNAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[endRow, ColGRNAmount].CellStyle.Font.Bold = true;
            sheet.Range[endRow, ColGRNAmount, endRow, ColGRNAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[endRow, ColGRNAmount, endRow, ColGRNAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[endRow, ColGRNAmount].CellStyle.Font.Bold = true;
            sheet.Range[endRow, ColGRNUOM, endRow, ColIssueBaseQty].Merge();

            sheet[endRow, ColIssueAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColIssueAmount) + startRow + ":" + clsStaticInfo.GetxlsCol(ColIssueAmount) + (ROW - 2).ToString() + ")";
            //sheet.Range[endRow, ColIssueAmount].Number = clsStaticInfo.dbl(data.Compute("SUM(IssueAmount)", null));
            sheet.Range[endRow, ColIssueAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[endRow, ColIssueAmount].CellStyle.Font.Bold = true;
            sheet.Range[endRow, ColIssueAmount, endRow, ColIssueAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[endRow, ColIssueAmount, endRow, ColIssueAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[endRow, ColIssueAmount].CellStyle.Font.Bold = true;

            sheet.Range[endRow, 1, endRow, endCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[endRow, 1, endRow, endCol].BorderInside(ExcelLineStyle.Hair);

            endRow++;
            endRow++;

            //Total End
            //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
            //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
            sheet["A" + startRow.ToString()].FreezePanes();

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "BOQ Status Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
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

            //return workbook;
            var filePath = "";
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;
        }

        public DataTable getBOQStatusReportSql(Dictionary<string, string> parameters)
        {
            try
            {
                var sql = @"SELECT distinct boq.Id RowId,boq.[Sequence],boq.ItemRefNo,ci.UserName AS CostingItem,boq.BOQCriteria,c.Code AS Currency,p.UserName AS Vendor,mm.UserName AS Material,mma.StandardName AS Article,BOM.Id BOMId
                ,cv1.UserName AS SKU1,isnull(cv2.UserName,'') SKU2,boq.SKUDesc,boq.POCriteria
				--,boq.Consumption
				,isnull(OPCD.GrossConsumption,0) Consumption
                ,boq.BOMQty,UOM.UserName BOQUOM,boq.BOMQtyBase,boq.RequiredQty
				,boq.Rate*BOQ.BOMQty AS BOMAmount
				, poboq.POBOQQty,poboq.POUOM,poboq.POTrnBOQQty,poboq.POAmount,BalanceBOQ=boq.BOMQtyBase-poboq.POBOQQty
                , grnboq.GRNBaseQty
                , grnboq.GRNAmount
                , grnboq.GRNUOM
                , BalancePOQty=poboq.POBOQQty-grnboq.GRNBaseQty
                , issueboq.IssueBaseQty
                , issueboq.IssueAmount
                , BalanceGRNQty=grnboq.GRNBaseQty-issueboq.IssueBaseQty
                ,PC.Id PartyId,PC.UserName Customer
				,MO.Id MasterOrderId,MO.BuyerReferenceNo,MO.OwnReferenceNo,moi.Id LineItemId
                FROM BOQ  boq
                LEFT JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=boq.UoMId
                LEFT JOIN hkp.CostingItem AS ci ON ci.Id=boq.CostingItemId
                LEFT JOIN scs.Currency AS c ON c.Id=boq.CurrencyId
                LEFT JOIN hkp.Party AS p ON p.Id=boq.VendorId
                LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=boq.MaterialMasterId
                LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=boq.ArticleId
                LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=boq.FGFirstCharacteristicsValueId
                LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=boq.FGSecondCharacteristicsValueId
                left join costingboqmaster BOM on BOM.Id=boq.CostingBOQMasterId
                left join TRN.MasterOrderItem AS moi on boq.MasterOrderItemId = moi.Id
                left join TRN.MasterOrder MO on MO.Id = moi.MasterOrderId
                left join HKP.Party PC on PC.Id = MO.PartyId
               left join TRN.SalesOrder SO on SO.CostingBOQMasterId = BOM.Id
			   left join [TRN].[CustomerPO] CPO on CPO.MasterOrderId = MO.Id and CPO.Id=SO.CustomerPOId
			   --new add
			   LEFT JOIN (Select DISTINCT SalesOrderId,CostingBOQMasterId,CostingItemId,OrderProcurementCostingDirectMaterialId from CostingBOQItems )CBI on CBI.CostingBOQMasterId=boq.CostingBOQMasterId AND CBI.CostingItemId=boq.CostingItemId --AND so.Id=CBI.SalesOrderId
				LEFT JOIN OrderProcurementCostingDirectMaterial OPCD on OPCD.Id=CBI.OrderProcurementCostingDirectMaterialId AND CBI.CostingItemId=OPCD.CostingItemId AND boq.CostingItemId=OPCD.CostingItemId

                left join(SELECT pomap.BOQDetailId,sum(pomap.POBOQQty) POBOQQty,sum(pomap.TransactionQty) POTrnBOQQty,UOM.UserName POUOM,SUM(pod.BaseAmount) POAmount 
                			FROM  trn.POBOQMAP pomap 
                			JOIN trn.PurchaseOrderDetail pod on pod.Id=pomap.PODetailId
                			LEFT JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=pod.TransactionUoMId
                			GROUP BY pomap.BOQDetailId,UOM.UserName
                			) poboq ON poboq.BOQDetailId=boq.Id

                left join (SELECT gpa.BOQDetailId,sum(gpa.TransactionQty) GRNBaseQty,UOM.UserName GRNUOM,sum(IRD.TotalMaterialTranAmount ) GRNAmount
                				FROM trn.GRNPORequisitionAllocation gpa 
                				JOIN trn.InventoryReceiveDetail IRD ON gpa.InventoryReceiveDetailId=IRD.Id
                				LEFT JOIN SCS.UnitOfMeasurement UOM ON UOM.Id=IRD.TransactionUoMId
                				GROUP BY gpa.BOQDetailId,UOM.UserName
                			) grnboq ON grnboq.BOQDetailId=poboq.BOQDetailId

                left join (SELECT iihb.BOQDetailId,sum(iihb.Qty) IssueBaseQty ,sum(iihb.Qty*iih.Rate) IssueAmount
                			FROM trn.InventoryIssueHistoryBOQ iihb 
                			join TRN.InventoryIssueHistory iih on iihb.InventoryIssueHistoryId=iih.Id
                			GROUP BY iihb.BOQDetailId

                ) issueboq ON issueboq.BOQDetailId=poboq.BOQDetailId


                                              where PC.Id in(" + parameters["PartyId"] + @")
                                              AND moi.BuyerReferenceNo in(" + parameters["BuyerReferenceNo"] + @")
                                              AND moi.OwnReferenceNo in(" + parameters["OwnReferenceNo"] + @")
                                              AND MO.Id in(" + parameters["MasterOrderId"] + @")
                                              AND moi.Id in(" + parameters["LineItemId"] + @")
                                              AND SO.Id in(" + parameters["SOId"] + @")                                       
                                        AND CPO.PONumber in(" + parameters["PONo"] + @")";

                
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string,object> getBOQStatusReportHeaderSql(Dictionary<string, string> parameters)
        {
            try
            {
                //      var sql = @"SELECT boq.Id RowId,boq.[Sequence],boq.ItemRefNo,ci.UserName AS CostingItem,boq.BOQCriteria,mma.StandardName AS Article
                //,boq.SKUDesc,boq.POCriteria,boq.Consumption
                //,boq.BOMQty,boq.BOMQtyBase,boq.RequiredQty,MO.Id MasterOrderId
                //,CPO.PONumber,SO.Id SOId
                //,moi.BuyerReferenceNo,moi.OwnReferenceNo,moi.Id LineItemId
                //,PAR.UserName PartyName,B.UserName BuyerName,mma.StandardName Product
                //FROM BOQ  boq
                //LEFT JOIN hkp.CostingItem AS ci ON ci.Id=boq.CostingItemId
                //left join costingboqmaster BOM on BOM.Id=boq.CostingBOQMasterId
                //left join CostingBOQItems BOMI on BOMI.CostingBOQMasterId=BOM.Id
                //left join TRN.SalesOrder SO on SO.Id=BOMI.SalesOrderId
                //   left join TRN.MasterOrderItem AS moi on SO.MasterOrderItemId=moi.Id 
                //   left join TRN.MasterOrder MO on MO.Id=moi.MasterOrderId
                //LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId 
                //                              left  join [TRN].[CustomerPO] CPO on CPO.MasterOrderId=MO.Id
                //left join HKP.Party PAR on PAR.Id=MO.PartyId
                //left join HKP.Buyer B on B.Id=MO.BuyerId

                //                              where PAR.Id in(" + parameters["PartyId"] + @")
                //                              AND moi.BuyerReferenceNo in(" + parameters["BuyerReferenceNo"] + @")
                //                              AND moi.OwnReferenceNo in(" + parameters["OwnReferenceNo"] + @")
                //                              AND MO.Id in(" + parameters["MasterOrderId"] + @")
                //                              AND moi.Id in(" + parameters["LineItemId"] + @")
                //                              AND SO.Id in(" + parameters["SOId"] + @")                                       
                //                              AND CPO.PONumber in(" + parameters["PONo"] + @")";

                var sql = @"SELECT distinct boq.Id RowId, boq.[Sequence],boq.ItemRefNo,ci.UserName AS CostingItem,boq.BOQCriteria,mma.StandardName AS Article
										,boq.SKUDesc,boq.POCriteria,boq.Consumption
										,boq.BOMQty,boq.BOMQtyBase,boq.RequiredQty
										,MO.Id MasterOrderId
                                        , moi.BuyerReferenceNo,moi.OwnReferenceNo,moi.Id LineItemId
                                         , PAR.UserName PartyName, B.UserName BuyerName, mma.StandardName Product
                                        --,CPO.PONumber,SO.Id SOId
										
										,PONumber=STUFF((SELECT distinct ','+  CPO.PONumber
										from [TRN].[CustomerPO] CPO
										left join [TRN].[SalesOrder] AS SO on SO.CustomerPOId=CPO.Id
										where SO.CostingBOQMasterId=BOM.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

										,SOId=STUFF((SELECT distinct ','+  XITM.Id
										from trn.SalesOrder AS XITM
										where XITM.CostingBOQMasterId=BOM.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,SO.SOQty 
                                    

                                        FROM BOQ  boq
                                        LEFT JOIN hkp.CostingItem AS ci ON ci.Id = boq.CostingItemId
                                        left join costingboqmaster BOM on BOM.Id = boq.CostingBOQMasterId
                                        left join CostingBOQItems BOMI on BOMI.CostingBOQMasterId = BOM.Id
                                        --left join TRN.SalesOrder SO on SO.Id = BOMI.SalesOrderId
										left join (SELECT Id,sum(Qty) SOQty,CostingBOQMasterId,MasterOrderItemId from trn.SalesOrder group by Id,Qty,CostingBOQMasterId,MasterOrderItemId) SO on SO.CostingBOQMasterId=BOM.Id
                                        left join TRN.MasterOrderItem AS moi on SO.MasterOrderItemId = moi.Id
                                        left join TRN.MasterOrder MO on MO.Id = moi.MasterOrderId
                                        LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id = moi.ArticleId
                                        left join[TRN].[CustomerPO] CPO on CPO.MasterOrderId = MO.Id
                                        left join HKP.Party PAR on PAR.Id = MO.PartyId
                                        left join HKP.Buyer B on B.Id = MO.BuyerId
										--left join (SELECT sum(Qty) SOQty,CostingBOQMasterId from trn.SalesOrder group by CostingBOQMasterId) SOI on SOI.CostingBOQMasterId=BOM.Id
                                    
                                        where PAR.Id in(" + parameters["PartyId"] + @")
                                        AND moi.BuyerReferenceNo in(" + parameters["BuyerReferenceNo"] + @")
                                        AND moi.OwnReferenceNo in(" + parameters["OwnReferenceNo"] + @")
                                        AND MO.Id in(" + parameters["MasterOrderId"] + @")
                                        AND moi.Id in(" + parameters["LineItemId"] + @")
                                        AND SO.Id in(" + parameters["SOId"] + @")                                       
                                        AND CPO.PONumber in(" + parameters["PONo"] + @")";

                return _sqlRepository.GetData(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

}
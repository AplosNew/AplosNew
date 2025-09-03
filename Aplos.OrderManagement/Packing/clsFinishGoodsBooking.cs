using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;
using Library.Service.Systems;
using Library.Data;
using Library.Service.Logs;
using System.Reflection;
using Library.Service.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;

namespace Library.OrderManagement.Packing
{
    public class clsFinishGoodsBooking
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        #region Constructor
        public clsFinishGoodsBooking()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        #endregion Constructor

        public IEnumerable<object> GetItemDetailListData(string productionOrderId)
        {
            try
            {
                string sql = @"SELECT PO.Id ProductionOrderId,MOI.Id ItemId,MOI.MasterOrderId,P.UserName Customer,PL.Code ProductCode,SUM(SO.Qty) SOQty
                                    ,SONo = STUFF((SELECT DISTINCT ',' + XSO.Id
							                                    FROM trn.SalesOrder XSO
							                                    WHERE XSO.Id = POD.SalesOrderId
							                                    FOR XML path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    FROM TRN.ProductionOrder PO
                                    LEFT JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId = PO.Id
                                    LEFT JOIN  TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
                                    LEFT JOIN TRN.MasterOrderItem MOI ON moi.Id = so.MasterOrderItemId
                                    LEFT JOIN TRN.MasterOrder MO ON mo.Id = MOI.MasterOrderId
                                    LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
                                    LEFT JOIN HKP.Party P ON P.Id = MO.PartyId
                                    WHERE PO.Id='" + productionOrderId + "' GROUP BY PO.Id,MOI.Id,MOI.MasterOrderId,POD.SalesOrderId,P.UserName,PL.Code";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetCurrentQtyBreakDownData(string productionOrderId, string productCode, string entityId, string fromDate, string toDate)
        {
            try
            {
                string sql = @"Select Qty=ROUND(CAST(SUM(CASE WHEN ISC.IsDespatch=0 THEN ISC.NetWeight ELSE 0 END) AS DECIMAL(18,2)), 2),ISC.POId,ISC.ProductCode,FORMAT(ISM.WorkDate,'dd-MMM-yyyy') WorkDate
					from dbo.ItemScanChild ISC 
					LEFT JOIN dbo.ItemScan ISM ON ISM.Id=ISC.MasterId 
					WHERE ISM.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(ISC.InventoryReceiveDetailId,'')='' AND ISNULL(ISC.PackingId,'')='' AND ISC.POId IN (Select Id from TRN.ProductionOrder Where EntityId='" + entityId + @"') AND ISC.POId ='" + productionOrderId + @"' AND ISC.ProductCode='" + productCode + @"'
                    GROUP BY ISC.PoId,ISC.ProductCode,ISM.WorkDate";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetUnBookedQtyBreakDownData(string productionOrderId, string productCode, string entityId, string fromDate, string toDate)
        {
            try
            {
                string sql = @"SELECT Qty=ROUND(CAST(SUM(CASE WHEN ISC.IsDespatch=0 THEN ISC.NetWeight ELSE 0 END) AS DECIMAL(18,2)), 2),ISC.POId,ISC.ProductCode,FORMAT(ISM.WorkDate,'dd-MMM-yyyy') WorkDate
	                        FROM dbo.ItemScanChild ISC 
	                        LEFT JOIN dbo.ItemScan ISM ON ISM.Id=ISC.MasterId 
	                        WHERE ISNULL(ISC.InventoryReceiveDetailId,'')='' AND ISNULL(ISC.PackingId,'')='' AND ISC.POId IN (Select Id from TRN.ProductionOrder Where EntityId='" + entityId + @"')
                            AND ISC.PoId='" + productionOrderId + @"' AND ISC.ProductCode='" + productCode + @"'  AND ISM.WorkDate NOT IN(Select FORMAT(WorkDate,'dd-MMM-yyyy') from dbo.ItemScan Where WorkDate between '" + fromDate + @"' AND '" + toDate + @"') 
	                        GROUP BY ISC.PoId,ISC.ProductCode,ISM.WorkDate";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetBookedQtyBreakDownData(string productionOrderId, string productCode, string entityId)
        {
            try
            {
                string sql = @"SELECT Qty=ROUND(CAST(SUM(CASE WHEN ISC.IsDespatch=0 THEN ISC.NetWeight ELSE 0 END) AS DECIMAL(18,2)), 2),ISC.POId,ISC.ProductCode,FORMAT(ISM.WorkDate,'dd-MMM-yyyy') WorkDate
	FROM dbo.ItemScanChild ISC 
	LEFT JOIN dbo.ItemScan ISM ON ISM.Id=ISC.MasterId 
	WHERE ISNULL(ISC.InventoryReceiveDetailId,'')<>'' AND ISNULL(ISC.PackingId,'')='' AND ISC.POId IN (Select Id from TRN.ProductionOrder Where EntityId='" + entityId + @"')
	AND ISC.POId ='" + productionOrderId + @"' AND ISC.ProductCode='" + productCode + @"'
	GROUP BY ISC.PoId,ISC.ProductCode,ISM.WorkDate";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetListByPacking()
        {
            try
            {
                string sql = @"Select E.UserName ProductionEntity, P.UserName Process,FORMAT(FGB.FromDate,'dd-MMM-yyyy') FDate,FORMAT(FGB.ToDate,'dd-MMM-yyyy') TDate
                                ,MS.UserName MaterialStorage,C.Code Currency,FGB.* 
                                ,GRNNo= STUFF((select distinct ','+IR.Id from 
                                TRN.InventoryReceive IR 
                                JOIN [dbo].[FinishGoodsBooking] FG ON IR.FinishGoodsBookingId=FG.Id		       
                                where FG.Id=FGB.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                from [dbo].[FinishGoodsBooking] FGB
                                LEFT JOIN ORG.Entity E ON E.Id=FGB.ProductionEntityId
                                LEFT JOIN HKP.Process P ON P.Id=FGB.ProcessId 
                                LEFT JOIN HKP.MaterialStorage MS ON MS.Id=FGB.MaterialStorageId
                                LEFT JOIN SCS.Currency C ON C.Id=FGB.CurrencyId
                                Where FGB.SourceType='Packing' ORDER BY FGB.AddedDate DESC";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetListByProductionBooking()
        {
            try
            {
                string sql = @"Select E.UserName ProductionEntity, P.UserName Process,FORMAT(FGB.FromDate,'dd-MMM-yyyy') FDate,FORMAT(FGB.ToDate,'dd-MMM-yyyy') TDate
                                ,MS.UserName MaterialStorage,C.Code Currency,FGB.* 
                                ,GRNNo= STUFF((select distinct ','+IR.Id from 
                                TRN.InventoryReceive IR 
                                JOIN [dbo].[FinishGoodsBooking] FG ON IR.FinishGoodsBookingId=FG.Id		       
                                where FG.Id=FGB.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                from [dbo].[FinishGoodsBooking] FGB
                                LEFT JOIN ORG.Entity E ON E.Id=FGB.ProductionEntityId
                                LEFT JOIN HKP.Process P ON P.Id=FGB.ProcessId 
                                LEFT JOIN HKP.MaterialStorage MS ON MS.Id=FGB.MaterialStorageId
                                LEFT JOIN SCS.Currency C ON C.Id=FGB.CurrencyId
                                Where FGB.SourceType='ProductionBooking' ORDER BY FGB.AddedDate DESC";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetDetailList(string masterId, string entityId, string processId, string productionOrderId)
        {
            try
            {
                string sql = @"SELECT DISTINCT mo.MasterOrderNo,FGBD.Id
									,ISNULL(so.Id,'') SalesOrderId
									,PO.Id ProductionOrderId
									,mm.Id MaterialMasterId
									,mm.UserName MaterialMaster
									,ISNULL(mma.Id, '') ArticleId
									,ISNULL(mma.StandardName, '') Article
									,moi.ProductLibraryId
									,ISNULL(PL.Code,'') ProductCode
									,ISNULL(POD.ProductionOrderId, '') POId
									,BU.UserName Buyer
									,mo.TotalQty OrderQty
									,CEILING(SO.PlannedQty) PlannedQty,FB.Qty OtherBookedQty,FGBD.Qty,ISNULL(B.Rate,0) Rate,B.OrderCostingMasterTemplateId,B.CostingItem
									,ISNULL(MO.BuyerReferenceNo,'') BuyerOrder,ISNULL(MO.OwnReferenceNo,'') OwnOrder,ISNULL(moi.BuyerReferenceNo,'') BuyerItem,ISNULL(moi.OwnReferenceNo,'') OwnItem
									FROM TRN.ProductionOrderDetail POD
									LEFT JOIN (
									SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
									,s.Id,s.MasterOrderItemId
									FROM trn.SalesOrder AS s
									INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
									GROUP BY S.Id,s.MasterOrderItemId
									) so ON POD.SalesOrderId = SO.Id
									LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
									LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
									LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
									LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
									LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
									LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
									INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
									) OS ON OS.ProductionOrderId = POD.ProductionOrderId
									LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
									LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
									LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
									LEFT JOIN dbo.ProductLibrary PL ON PL.Id=moi.ProductLibraryId
									LEFT JOIN (
									select MOI.Id,A.OrderCostingMasterTemplateId, SUM(A.Rate) Rate,CI.UserName CostingItem
									from TRN.MasterOrderItem MOI
									LEFT JOIN [dbo].[OrderCostingMasterTemplate] OCMT ON OCMT.Id=MOI.OrderCostingMasterTemplateId
									LEFT JOIN
									( 
									Select DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate from [dbo].[OrderProcurementCostingDirectMaterial] DM
									UNION
									Select DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate from [dbo].[OrderProcurementCostingDirectProcess] DP
									UNION
									Select OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate from [dbo].[OrderProcurementCostingOperation] OP
									UNION
									Select P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate from [dbo].[OrderProcurementCostingProfit] P
									UNION
									Select SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate from [dbo].[OrderProcurementCostingSalesExpense] SE
									UNION
									Select VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate from [dbo].[OrderProcurementCostingValueLoss] VL
									) 
									A ON A.OrderCostingMasterTemplateId=OCMT.OrderCostingMasterTemplateId
									JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
									JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
									WHERE ISNULL(ConsiderForFGValuation,0)=1 
									GROUP BY MOI.Id,A.OrderCostingMasterTemplateId,CI.UserName
									) B ON B.Id=moi.Id AND B.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId
									LEFT JOIN (
									SELECT SUM(B.Qty) Qty,A.ProductionOrderId,B.SalesOrderId
									FROM dbo.[FinishGoodsBooking] A 
									JOIN dbo.[FinishGoodsBookingDetail] B ON A.Id=B.FinishGoodsBookingId
									WHERE ISNULL(B.ProductLibraryId,'')<>'' AND ISNULL(A.Id,'') <>'" + masterId + @"'
									GROUP BY A.ProductionOrderId,B.SalesOrderId
									) FB ON FB.ProductionOrderId=PO.Id AND FB.SalesOrderId=SO.Id
									LEFT JOIN [dbo].[FinishGoodsBookingDetail] FGBD ON FGBD.ProductionOrderId=PO.Id AND FGBD.SalesOrderId=SO.Id 
									WHERE  POSP.ProcessId = '" + processId + "' AND PO.EntityId='" + entityId + "'  AND PO.Id='" + productionOrderId + "' AND FGBD.FinishGoodsBookingId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetDateWiseConsumptionData(string entityId, string fromDate, string toDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = @"SELECT FORMAT(A.WorkDate,'dd-MMM-yyyy')WorkDate,A.ProductCode,A.POId
                             FROM (
                            SELECT ISN.WorkDate,SC.ProductCode,sc.POId
                            FROM dbo.ItemScanChild SC 
                            LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
                            LEFT JOIN dbo.ItemScan ISN ON ISN.Id=SC.MasterId						
                            WHERE ISN.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(SC.InventoryReceiveDetailId,'')='' AND SC.POId IN (Select Id from TRN.ProductionOrder Where EntityId='" + entityId + @"')
                            ) A Group By 
                            A.ProductCode,A.POId,A.WorkDate";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetConsumptionByCostingData(string costingId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = @"SELECT A.Id InPutCostingItemId,CT.Id CostingId, SUM(A.GrossAmount) GrossAmount,A.GrossConsumption,CI.UserName CostingItem, CC.UserName CostingComponent
							FROM dbo.OrderCostingMasterTemplate CT
							LEFT JOIN
								( 
								SELECT DM.Id,DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount,DM.GrossConsumption FROM [dbo].OrderProcurementCostingDirectMaterial DM
								) 
							A ON A.OrderCostingMasterTemplateId=CT.Id
							LEFT JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
							LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
							WHERE CT.Id='" + costingId + @"' AND ISNULL(CC.ConsiderForFGValuation,0)=1 
							GROUP BY A.Id,CT.Id,A.GrossConsumption,CI.UserName, CC.UserName";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> FGList, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string pOId = null;
                string productCode = null;
                string MaterialMasterId = null;
                string ArticleId = null;

                bplib.clsGenID objGenID = new bplib.clsGenID();

                foreach (var item in FGList)
                {
                    if (Convert.ToBoolean(item["Flag"].ToString()) == true)
                    {
                        if (pOId == null)
                        {
                            pOId = "'" + item["ProductionOrderId"].ToString() + "'";
                        }
                        else
                        {
                            pOId += ",'" + item["ProductionOrderId"].ToString() + "'";
                        }

                        if (productCode == null)
                        {
                            productCode = "'" + item["ProductCode"].ToString() + "'";

                        }
                        else
                        {
                            productCode += ",'" + item["ProductCode"].ToString() + "'";
                        }


                        if (MaterialMasterId == null)
                        {
                            MaterialMasterId = "'" + item["MaterialMasterId"].ToString() + "'";
                        }
                        else
                        {
                            MaterialMasterId += ",'" + item["MaterialMasterId"].ToString() + "'";
                        }
                        if (ArticleId == null)
                        {
                            ArticleId = "'" + item["ArticleId"].ToString() + "'";
                        }
                        else
                        {
                            ArticleId += ",'" + item["ArticleId"].ToString() + "'";
                        }

                    }

                }

                DataSet dsMaster, dsItemScanChild, dsFGDetail, dsConsumptionByCosting, dsFromConsumptionByCosting, dsProductionSummary, dsInventoryReceive, dsInventoryReceiveDetail, dsInventoryMaterial;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[FinishGoodsBooking] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[FinishGoodsBookingDetail] WHERE FinishGoodsBookingId='" + data["Id"] + "'", out dsFGDetail, false, "1");

                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.InventoryReceive WHERE 1 = 2", out dsInventoryReceive, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.InventoryReceiveDetail WHERE 1 = 2", out dsInventoryReceiveDetail, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.ProductionSummary WHERE 1 = 2", out dsProductionSummary, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ConsumptionByCosting WHERE 1 = 2", out dsConsumptionByCosting, false, "1");

                con.OpenDataSetThroughAdapter(@"SELECT * FROM dbo.ItemScanChild WHERE MasterId IN (Select Id from dbo.ItemScan ISN WHERE ISN.WorkDate between '" + data["FromDate"] + "' AND '" + data["ToDate"] + "') AND POId IN (" + pOId + @") AND ProductCode IN (" + productCode + @") AND ISNULL(InventoryReceiveDetailId,'')=''", out dsItemScanChild, false, "1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[InventoryMaterial] where MaterialMasterId IN(" + MaterialMasterId + ") and ArticleId IN(" + ArticleId + ")  and CompanyId='" + identity.CompanyId + "' and PlantId='" + identity.PlantId + "'", out dsInventoryMaterial, false, "1");

                string _Id = null, masterId = null, detailId = null, iID = null, inventoryMaterialId = null;

                #region FinishGoodsBooking & Detail

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FinishGoodsBooking", out _Id);
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                int count = 0;
                if (FGList != null)

                {
                    foreach (var item in FGList)

                    {

                        count++;
                        DataView dv = new DataView(dsFGDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = masterId + "" + count;
                            item["FinishGoodsBookingId"] = masterId;
                            AddNewRow(dsFGDetail.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }

                    }
                }

                #endregion

                #region InventoryReceive
                int detailIdCount = 0;
                foreach (var item in FGList)
                {
                    detailIdCount++;
                    #region InventoryReceive

                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceive", out iID);

                    DataRow drInventoryReceive = dsInventoryReceive.Tables[0].NewRow();
                    drInventoryReceive["Id"] = iID;
                    drInventoryReceive["CompanyGroupId"] = identity.CompanyGroupId;
                    drInventoryReceive["CompanyId"] = identity.CompanyId;
                    drInventoryReceive["PlantId"] = identity.PlantId;
                    drInventoryReceive["DocRefNo"] = iID;
                    drInventoryReceive["CurrencyId"] = data["CompanyCurrencyId"]; //companyCurrency
                    drInventoryReceive["MaterialStorageId"] = data["MaterialStorageId"];
                    drInventoryReceive["ToCurrencyRate"] = 1;
                    drInventoryReceive["FixedAssetOrInventory"] = "Inventory";
                    drInventoryReceive["GRNType"] = "FG";
                    drInventoryReceive["EntityId"] = data["ProductionEntityId"].ToString();
                    drInventoryReceive["GRNDate"] = ToDate;
                    drInventoryReceive["DocDate"] = DBNull.Value;
                    drInventoryReceive["EntryDate"] = DateTime.Now;
                    drInventoryReceive["PODepended"] = false;
                    drInventoryReceive["AlongwithInvoice"] = false;
                    drInventoryReceive["IsNonCreditable"] = false;
                    drInventoryReceive["IsTaxApplicable"] = false;
                    drInventoryReceive["IsApproved"] = true;
                    drInventoryReceive["IsPaymentHold"] = false;
                    drInventoryReceive["IsNonVendor"] = false;
                    drInventoryReceive["IsFOC"] = false;
                    drInventoryReceive["IsInvoice"] = false;

                    drInventoryReceive["FinishGoodsBookingId"] = masterId;
                    drInventoryReceive["ProductionOrderId"] = item["ProductionOrderId"];

                    drInventoryReceive["AddedBy"] = identity.Name;
                    drInventoryReceive["AddedDate"] = DateTime.Now;
                    drInventoryReceive["AddedFromIP"] = identity.IPAddress;
                    dsInventoryReceive.Tables[0].Rows.Add(drInventoryReceive);

                    #endregion

                    #region InventoryMaterial

                    //if (dsInventoryMaterial.Tables[0].Rows.Count > 0)
                    //{
                    dsInventoryMaterial.Tables[0].DefaultView.RowFilter = "MaterialMasterId='" + item["MaterialMasterId"].ToString() + "' AND ArticleId = '" + item["ArticleId"].ToString() + "'";

                    if (dsInventoryMaterial.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = dsInventoryMaterial.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        inventoryMaterialId = dr["Id"].ToString();
                        dr["TotalQty"] = Convert.ToDecimal(dr["TotalQty"].ToString()) + Convert.ToDecimal(item["Qty"].ToString());

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr.EndEdit();
                    }
                    else
                    {
                        objGenID.GenerateIDAuto("InventoryMaterial", out inventoryMaterialId);

                        DataRow drInventoryMaterial = dsInventoryMaterial.Tables[0].NewRow();
                        drInventoryMaterial["Id"] = inventoryMaterialId;
                        drInventoryMaterial["CompanyGroupId"] = identity.CompanyGroupId;
                        drInventoryMaterial["CompanyId"] = identity.CompanyId;
                        drInventoryMaterial["PlantId"] = identity.PlantId;
                        drInventoryMaterial["MaterialMasterId"] = item["MaterialMasterId"];
                        drInventoryMaterial["ArticleId"] = item["ArticleId"];
                        drInventoryMaterial["TotalQty"] = item["Qty"];
                        drInventoryMaterial["AvgRate"] = 0;

                        drInventoryMaterial["AddedBy"] = identity.Name;
                        drInventoryMaterial["AddedDate"] = DateTime.Now;
                        drInventoryMaterial["AddedFromIP"] = identity.IPAddress;

                        dsInventoryMaterial.Tables[0].Rows.Add(drInventoryMaterial);
                    }
                    //}


                    #endregion

                    #region InventoryReceiveDetail

                    DataRow drInventoryReceiveDetail = dsInventoryReceiveDetail.Tables[0].NewRow();
                    drInventoryReceiveDetail["Id"] = dsInventoryReceive.Tables[0].Rows[0]["Id"].ToString() + "-" + detailIdCount;
                    detailId = dsInventoryReceive.Tables[0].Rows[0]["Id"].ToString() + "-" + detailIdCount;
                    drInventoryReceiveDetail["InventoryReceiveId"] = dsInventoryReceive.Tables[0].Rows[0]["Id"].ToString();
                    drInventoryReceiveDetail["InventoryMaterialId"] = inventoryMaterialId;
                    drInventoryReceiveDetail["TransactionQty"] = item["Qty"];

                    drInventoryReceiveDetail["BaseQty"] = item["Qty"];
                    drInventoryReceiveDetail["GRNQty"] = item["Qty"];
                    drInventoryReceiveDetail["MaterialTranRate"] = Math.Round(Convert.ToDecimal(item["Rate"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 4);
                    drInventoryReceiveDetail["MaterialTranAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                    drInventoryReceiveDetail["TotalMaterialTranAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                    drInventoryReceiveDetail["TotalMaterialBooksCurrencyAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                    drInventoryReceiveDetail["GRNTotalAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                    drInventoryReceiveDetail["GrossAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                    drInventoryReceiveDetail["BooksCurrencyBaseRate"] = Math.Round(Convert.ToDecimal(item["Rate"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 4);
                    drInventoryReceiveDetail["TrnCurrencyBaseRate"] = Math.Round(Convert.ToDecimal(item["Rate"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 4);
                    drInventoryReceiveDetail["MaterialStorageId"] = data["MaterialStorageId"];
                    drInventoryReceiveDetail["DiscountAmount"] = 0;
                    drInventoryReceiveDetail["BaseUoMFactor"] = 1;
                    drInventoryReceiveDetail["TotalTaxAmount"] = 0;
                    drInventoryReceiveDetail["ChargesTranAmount"] = 0;
                    drInventoryReceiveDetail["ChargesTaxTranAmount"] = 0;

                    drInventoryReceiveDetail["BaseIssueQty"] = 0;

                    drInventoryReceiveDetail["ShortageQty"] = 0;
                    drInventoryReceiveDetail["RejectionQty"] = 0;
                    drInventoryReceiveDetail["ApprovedQty"] = 0;
                    drInventoryReceiveDetail["ShortageRatePercent"] = 0;
                    drInventoryReceiveDetail["ShortageValue"] = 0;
                    drInventoryReceiveDetail["RejectRatePercent"] = 0;
                    drInventoryReceiveDetail["RejectValue"] = 0;
                    drInventoryReceiveDetail["RejectClamPercent"] = 0;
                    drInventoryReceiveDetail["ShortRejFlag"] = 0;

                    drInventoryReceiveDetail["PostDrGLGeneralInfoId"] = null;
                    drInventoryReceiveDetail["PostDrBudgetMasterId"] = null;
                    drInventoryReceiveDetail["PostCRGLGeneralInfoId"] = null;
                    drInventoryReceiveDetail["PostCRBudgetMasterId"] = null;
                    drInventoryReceiveDetail["PostCRActivityId"] = null;
                    drInventoryReceiveDetail["CapitalizeVoucherDetailId"] = null;
                    drInventoryReceiveDetail["IsAsset"] = item["IsAsset"];
                    drInventoryReceiveDetail["TransactionUoMId"] = item["UOM"];
                    drInventoryReceiveDetail["BaseUOMId"] = item["UOM"];

                    drInventoryReceiveDetail["AddedBy"] = identity.Name;
                    drInventoryReceiveDetail["AddedDate"] = DateTime.Now;
                    drInventoryReceiveDetail["AddedFromIP"] = identity.IPAddress;
                    dsInventoryReceiveDetail.Tables[0].Rows.Add(drInventoryReceiveDetail);
                    #endregion

                    #region ProductionSummary

                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionSummary", out string sID);
                    DataRow drProductionSummary = dsProductionSummary.Tables[0].NewRow();
                    drProductionSummary["Id"] = sID;
                    drProductionSummary["PlantId"] = identity.PlantId;
                    drProductionSummary["EntityId"] = data["ProductionEntityId"].ToString();
                    drProductionSummary["ProcessId"] = data["ProcessId"].ToString();
                    drProductionSummary["ProductionDate"] = ToDate;
                    drProductionSummary["MaterialMasterId"] = item["MaterialMasterId"].ToString();
                    drProductionSummary["ArticleId"] = item["ArticleId"].ToString();
                    drProductionSummary["Quantity"] = item["Qty"];
                    drProductionSummary["ProductionOrderId"] = item["ProductionOrderId"];
                    drProductionSummary["FinishGoodsBookingId"] = masterId;
                    drProductionSummary["AddedBy"] = identity.Name;
                    drProductionSummary["AddedDate"] = DateTime.Now;
                    drProductionSummary["AddedFromIP"] = identity.IPAddress;
                    dsProductionSummary.Tables[0].Rows.Add(drProductionSummary);
                    #endregion

                    #region ConsumptionByCosting

                    if (item.ContainsKey("CostingMasterTemplateId"))
                    {
                        GetConsumptionByCostingData(item["CostingMasterTemplateId"].ToString(), out dsFromConsumptionByCosting);
                        dsFromConsumptionByCosting.Tables[0].DefaultView.RowFilter = "CostingId='" + item["CostingMasterTemplateId"].ToString() + "'";
                        for (int l = 0; l < dsFromConsumptionByCosting.Tables[0].DefaultView.Count; l++)
                        {
                            DataRow drConsumptionByCosting = dsConsumptionByCosting.Tables[0].NewRow();
                            CopyRow(dsFromConsumptionByCosting.Tables[0].DefaultView[l].Row, ref drConsumptionByCosting);
                            drConsumptionByCosting["Id"] = detailId + (l + 1);
                            drConsumptionByCosting["InventoryReceiveDetailId"] = detailId;
                            drConsumptionByCosting["GrossConsumption"] = dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossConsumption"].ToString();
                            drConsumptionByCosting["GrossAmount"] = dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossAmount"].ToString();
                            drConsumptionByCosting["InPutCostingItemId"] = dsFromConsumptionByCosting.Tables[0].DefaultView[l]["InPutCostingItemId"].ToString();
                            drConsumptionByCosting["TotalInputConsumption"] = Convert.ToDecimal(item["Qty"]) * Convert.ToDecimal(dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossConsumption"].ToString());

                            drConsumptionByCosting["TotalInputStandardCost"] = Convert.ToDecimal(dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossAmount"].ToString()) * Convert.ToDecimal(dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossConsumption"].ToString());

                            dsConsumptionByCosting.Tables[0].Rows.Add(drConsumptionByCosting);
                        }

                    }
                    #endregion

                    #region ItemScanChild
                    if (dsItemScanChild.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < dsItemScanChild.Tables[0].Rows.Count; j++)
                        {
                            dsItemScanChild.Tables[0].DefaultView.RowFilter = "Id='" + dsItemScanChild.Tables[0].Rows[j]["Id"].ToString() + "' AND POId = '" + item["ProductionOrderId"] + "' AND ProductCode = '" + item["ProductCode"] + "'";

                            if (dsItemScanChild.Tables[0].DefaultView.Count > 0)
                            {
                                //edit
                                DataRow dr = dsItemScanChild.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["InventoryReceiveDetailId"] = detailId;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr.EndEdit();
                            }
                        }
                    }
                    #endregion
                }

                #endregion


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsFGDetail, dsInventoryReceive, dsInventoryMaterial, dsInventoryReceiveDetail, dsConsumptionByCosting, dsItemScanChild, dsProductionSummary);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void OldforFGInventorySaveData(Dictionary<string, object> data, List<Dictionary<string, object>> WorkDayList, List<Dictionary<string, object>> FinishGoodsBookingDetailList, List<Dictionary<string, object>> FGList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string pOId = null;
                string productCode = null;
                string MaterialMasterId = null;
                string ArticleId = null;

                bplib.clsGenID objGenID = new bplib.clsGenID();

                //foreach (var item in FinishGoodsBookingDetailList)
                foreach (var item in FGList)
                {
                    if (Convert.ToBoolean(item["Flag"].ToString()) == true)
                    {
                        if (pOId == null)
                        {
                            pOId = "'" + item["ProductionOrderId"].ToString() + "'";
                        }
                        else
                        {
                            pOId += ",'" + item["ProductionOrderId"].ToString() + "'";
                        }

                        if (productCode == null)
                        {
                            productCode = "'" + item["ProductCode"].ToString() + "'";

                        }
                        else
                        {
                            productCode += ",'" + item["ProductCode"].ToString() + "'";
                        }


                        if (MaterialMasterId == null)
                        {
                            MaterialMasterId = "'" + item["MaterialMasterId"].ToString() + "'";
                        }
                        else
                        {
                            MaterialMasterId += ",'" + item["MaterialMasterId"].ToString() + "'";
                        }
                        if (ArticleId == null)
                        {
                            ArticleId = "'" + item["ArticleId"].ToString() + "'";
                        }
                        else
                        {
                            ArticleId += ",'" + item["ArticleId"].ToString() + "'";
                        }

                    }

                }

                DataSet dsMaster, dsItemScanChild, dsFGDetail, dsConsumptionByCosting, dsFromConsumptionByCosting, dsProductionSummary, dsInventoryReceive, dsInventoryReceiveDetail, dsInventoryMaterial;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                //GetDateWiseConsumptionData(data["ProductionEntityId"].ToString(), data["FromDate"].ToString(), data["ToDate"].ToString(), out dsFromDateWiseConsumption);
                //GetDateWiseDetailDataData(data["ProductionEntityId"].ToString(), data["FromDate"].ToString(), data["ToDate"].ToString(), out dsFromFinishGoodsBookingDetail);

                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[FinishGoodsBooking] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[FinishGoodsBookingDetail] WHERE FinishGoodsBookingId='" + data["Id"] + "'", out dsFGDetail, false, "1");

                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.InventoryReceive WHERE 1 = 2", out dsInventoryReceive, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.InventoryReceiveDetail WHERE 1 = 2", out dsInventoryReceiveDetail, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.ProductionSummary WHERE 1 = 2", out dsProductionSummary, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ConsumptionByCosting WHERE 1 = 2", out dsConsumptionByCosting, false, "1");

                con.OpenDataSetThroughAdapter(@"SELECT * FROM dbo.ItemScanChild WHERE MasterId IN (Select Id from dbo.ItemScan ISN WHERE ISN.WorkDate between '" + data["FromDate"] + "' AND '" + data["ToDate"] + "') AND POId IN (" + pOId + @") AND ProductCode IN (" + productCode + @") AND ISNULL(InventoryReceiveDetailId,'')=''", out dsItemScanChild, false, "1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[InventoryMaterial] where MaterialMasterId IN(" + MaterialMasterId + ") and ArticleId IN(" + ArticleId + ")  and CompanyId='" + identity.CompanyId + "' and PlantId='" + identity.PlantId + "'", out dsInventoryMaterial, false, "1");

                string _Id = null, masterId = null, detailId = null, iID = null, inventoryMaterialId = null;

                #region FinishGoodsBooking & Detail

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FinishGoodsBooking", out _Id);
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                int count = 0;
                if (FinishGoodsBookingDetailList != null)

                {
                    foreach (var item in FinishGoodsBookingDetailList)

                    {

                        count++;
                        DataView dv = new DataView(dsFGDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = masterId + "" + count;
                            item["FinishGoodsBookingId"] = masterId;
                            AddNewRow(dsFGDetail.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }

                    }
                }

                #endregion

                #region InventoryReceive

                foreach (var item in WorkDayList)
                {
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceive", out iID);

                    //dsFromDateWiseConsumption.Tables[0].DefaultView.RowFilter = "WorkDate=#" + Convert.ToDateTime(item["WorkDate"].ToString()) + "# AND POId = '" + item["ProductionOrderId"] + "' AND ProductCode = '" + item["ProductCode"] + "'";

                    DataRow drInventoryReceive = dsInventoryReceive.Tables[0].NewRow();
                    drInventoryReceive["Id"] = iID;
                    drInventoryReceive["CompanyGroupId"] = identity.CompanyGroupId;
                    drInventoryReceive["CompanyId"] = identity.CompanyId;
                    drInventoryReceive["PlantId"] = identity.PlantId;
                    drInventoryReceive["DocRefNo"] = iID;
                    drInventoryReceive["CurrencyId"] = data["CompanyCurrencyId"]; //companyCurrency
                    drInventoryReceive["MaterialStorageId"] = data["MaterialStorageId"];
                    drInventoryReceive["ToCurrencyRate"] = 1;
                    drInventoryReceive["FixedAssetOrInventory"] = "Inventory";
                    drInventoryReceive["GRNType"] = "FG";
                    drInventoryReceive["EntityId"] = data["ProductionEntityId"].ToString();
                    drInventoryReceive["GRNDate"] = item["WorkDate"];
                    drInventoryReceive["DocDate"] = DBNull.Value;
                    drInventoryReceive["EntryDate"] = DateTime.Now;
                    drInventoryReceive["PODepended"] = false;
                    drInventoryReceive["AlongwithInvoice"] = false;
                    drInventoryReceive["IsNonCreditable"] = false;
                    drInventoryReceive["IsTaxApplicable"] = false;
                    drInventoryReceive["IsApproved"] = true;
                    drInventoryReceive["IsPaymentHold"] = false;
                    drInventoryReceive["IsNonVendor"] = false;
                    drInventoryReceive["IsFOC"] = false;
                    drInventoryReceive["IsInvoice"] = false;

                    drInventoryReceive["FinishGoodsBookingId"] = masterId;
                    drInventoryReceive["ProductionOrderId"] = item["ProductionOrderId"];

                    drInventoryReceive["AddedBy"] = identity.Name;
                    drInventoryReceive["AddedDate"] = DateTime.Now;
                    drInventoryReceive["AddedFromIP"] = identity.IPAddress;
                    dsInventoryReceive.Tables[0].Rows.Add(drInventoryReceive);
                }

                #endregion

                for (int i = 0; i < dsInventoryReceive.Tables[0].Rows.Count; i++)
                {
                    int detailIdCount = 0;
                    var detailData = FinishGoodsBookingDetailList.Where(xx => Convert.ToDateTime(xx["WorkDate"].ToString()) == Convert.ToDateTime(dsInventoryReceive.Tables[0].Rows[i]["GRNDate"].ToString())).ToList();
                    if (detailData != null)
                    {
                        foreach (var item in detailData)
                        {
                            detailIdCount++;
                            #region InventoryMaterial

                            //if (dsInventoryMaterial.Tables[0].Rows.Count > 0)
                            //{
                            dsInventoryMaterial.Tables[0].DefaultView.RowFilter = "MaterialMasterId='" + item["MaterialMasterId"].ToString() + "' AND ArticleId = '" + item["ArticleId"].ToString() + "'";

                            if (dsInventoryMaterial.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsInventoryMaterial.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                inventoryMaterialId = dr["Id"].ToString();
                                dr["TotalQty"] = Convert.ToDecimal(dr["TotalQty"].ToString()) + Convert.ToDecimal(item["Qty"].ToString());

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr.EndEdit();
                            }
                            else
                            {
                                objGenID.GenerateIDAuto("InventoryMaterial", out inventoryMaterialId);

                                DataRow drInventoryMaterial = dsInventoryMaterial.Tables[0].NewRow();
                                drInventoryMaterial["Id"] = inventoryMaterialId;
                                drInventoryMaterial["CompanyGroupId"] = identity.CompanyGroupId;
                                drInventoryMaterial["CompanyId"] = identity.CompanyId;
                                drInventoryMaterial["PlantId"] = identity.PlantId;
                                drInventoryMaterial["MaterialMasterId"] = item["MaterialMasterId"];
                                drInventoryMaterial["ArticleId"] = item["ArticleId"];
                                drInventoryMaterial["TotalQty"] = item["Qty"];
                                drInventoryMaterial["AvgRate"] = 0;

                                drInventoryMaterial["AddedBy"] = identity.Name;
                                drInventoryMaterial["AddedDate"] = DateTime.Now;
                                drInventoryMaterial["AddedFromIP"] = identity.IPAddress;

                                dsInventoryMaterial.Tables[0].Rows.Add(drInventoryMaterial);
                            }
                            //}


                            #endregion

                            #region InventoryReceiveDetail

                            DataRow drInventoryReceiveDetail = dsInventoryReceiveDetail.Tables[0].NewRow();
                            drInventoryReceiveDetail["Id"] = dsInventoryReceive.Tables[0].Rows[i]["Id"].ToString() + "-" + detailIdCount;
                            detailId = dsInventoryReceive.Tables[0].Rows[i]["Id"].ToString() + "-" + detailIdCount;
                            drInventoryReceiveDetail["InventoryReceiveId"] = dsInventoryReceive.Tables[0].Rows[i]["Id"].ToString();
                            drInventoryReceiveDetail["InventoryMaterialId"] = inventoryMaterialId;
                            drInventoryReceiveDetail["TransactionQty"] = item["Qty"];

                            drInventoryReceiveDetail["BaseQty"] = item["Qty"];
                            drInventoryReceiveDetail["GRNQty"] = item["Qty"];
                            drInventoryReceiveDetail["MaterialTranRate"] = Math.Round(Convert.ToDecimal(item["Rate"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 4);
                            drInventoryReceiveDetail["MaterialTranAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["TotalMaterialTranAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["TotalMaterialBooksCurrencyAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["GRNTotalAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["GrossAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["BooksCurrencyBaseRate"] = Math.Round(Convert.ToDecimal(item["Rate"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 4);
                            drInventoryReceiveDetail["TrnCurrencyBaseRate"] = Math.Round(Convert.ToDecimal(item["Rate"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 4);
                            drInventoryReceiveDetail["MaterialStorageId"] = data["MaterialStorageId"];
                            drInventoryReceiveDetail["DiscountAmount"] = 0;
                            drInventoryReceiveDetail["BaseUoMFactor"] = 1;
                            drInventoryReceiveDetail["TotalTaxAmount"] = 0;
                            drInventoryReceiveDetail["ChargesTranAmount"] = 0;
                            drInventoryReceiveDetail["ChargesTaxTranAmount"] = 0;

                            drInventoryReceiveDetail["BaseIssueQty"] = 0;

                            drInventoryReceiveDetail["ShortageQty"] = 0;
                            drInventoryReceiveDetail["RejectionQty"] = 0;
                            drInventoryReceiveDetail["ApprovedQty"] = 0;
                            drInventoryReceiveDetail["ShortageRatePercent"] = 0;
                            drInventoryReceiveDetail["ShortageValue"] = 0;
                            drInventoryReceiveDetail["RejectRatePercent"] = 0;
                            drInventoryReceiveDetail["RejectValue"] = 0;
                            drInventoryReceiveDetail["RejectClamPercent"] = 0;
                            drInventoryReceiveDetail["ShortRejFlag"] = 0;

                            drInventoryReceiveDetail["PostDrGLGeneralInfoId"] = null;
                            drInventoryReceiveDetail["PostDrBudgetMasterId"] = null;
                            drInventoryReceiveDetail["PostCRGLGeneralInfoId"] = null;
                            drInventoryReceiveDetail["PostCRBudgetMasterId"] = null;
                            drInventoryReceiveDetail["PostCRActivityId"] = null;
                            drInventoryReceiveDetail["CapitalizeVoucherDetailId"] = null;
                            drInventoryReceiveDetail["IsAsset"] = item["IsAsset"];
                            drInventoryReceiveDetail["TransactionUoMId"] = item["UOM"];
                            drInventoryReceiveDetail["BaseUOMId"] = item["UOM"];

                            drInventoryReceiveDetail["AddedBy"] = identity.Name;
                            drInventoryReceiveDetail["AddedDate"] = DateTime.Now;
                            drInventoryReceiveDetail["AddedFromIP"] = identity.IPAddress;
                            dsInventoryReceiveDetail.Tables[0].Rows.Add(drInventoryReceiveDetail);
                            #endregion

                            #region ProductionSummary

                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionSummary", out string sID);
                            DataRow drProductionSummary = dsProductionSummary.Tables[0].NewRow();
                            drProductionSummary["Id"] = sID;
                            drProductionSummary["PlantId"] = identity.PlantId;
                            drProductionSummary["EntityId"] = data["ProductionEntityId"].ToString();
                            drProductionSummary["ProcessId"] = data["ProcessId"].ToString();
                            drProductionSummary["ProductionDate"] = Convert.ToDateTime(dsInventoryReceive.Tables[0].Rows[i]["GRNDate"].ToString()).ToString("dd-MMM-yyyy");
                            drProductionSummary["MaterialMasterId"] = item["MaterialMasterId"].ToString();
                            drProductionSummary["ArticleId"] = item["ArticleId"].ToString();
                            drProductionSummary["Quantity"] = item["Qty"];
                            drProductionSummary["ProductionOrderId"] = item["ProductionOrderId"];
                            drProductionSummary["FinishGoodsBookingId"] = masterId;
                            drProductionSummary["AddedBy"] = identity.Name;
                            drProductionSummary["AddedDate"] = DateTime.Now;
                            drProductionSummary["AddedFromIP"] = identity.IPAddress;
                            dsProductionSummary.Tables[0].Rows.Add(drProductionSummary);
                            #endregion

                            #region ConsumptionByCosting

                            if (item.ContainsKey("CostingMasterTemplateId"))
                            {
                                GetConsumptionByCostingData(item["CostingMasterTemplateId"].ToString(), out dsFromConsumptionByCosting);
                                dsFromConsumptionByCosting.Tables[0].DefaultView.RowFilter = "CostingId='" + item["CostingMasterTemplateId"].ToString() + "'";
                                for (int l = 0; l < dsFromConsumptionByCosting.Tables[0].DefaultView.Count; l++)
                                {
                                    DataRow drConsumptionByCosting = dsConsumptionByCosting.Tables[0].NewRow();
                                    CopyRow(dsFromConsumptionByCosting.Tables[0].DefaultView[l].Row, ref drConsumptionByCosting);
                                    drConsumptionByCosting["Id"] = detailId + (l + 1);
                                    drConsumptionByCosting["InventoryReceiveDetailId"] = detailId;
                                    drConsumptionByCosting["GrossConsumption"] = dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossConsumption"].ToString();
                                    drConsumptionByCosting["GrossAmount"] = dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossAmount"].ToString();
                                    drConsumptionByCosting["InPutCostingItemId"] = dsFromConsumptionByCosting.Tables[0].DefaultView[l]["InPutCostingItemId"].ToString();
                                    drConsumptionByCosting["TotalInputConsumption"] = Convert.ToDecimal(item["Qty"]) * Convert.ToDecimal(dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossConsumption"].ToString());

                                    drConsumptionByCosting["TotalInputStandardCost"] = Convert.ToDecimal(dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossAmount"].ToString()) * Convert.ToDecimal(dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossConsumption"].ToString());

                                    dsConsumptionByCosting.Tables[0].Rows.Add(drConsumptionByCosting);
                                }

                            }
                            #endregion

                            #region ItemScanChild
                            if (dsItemScanChild.Tables[0].Rows.Count > 0)
                            {
                                for (int j = 0; j < dsItemScanChild.Tables[0].Rows.Count; j++)
                                {
                                    dsItemScanChild.Tables[0].DefaultView.RowFilter = "Id='" + dsItemScanChild.Tables[0].Rows[j]["Id"].ToString() + "' AND POId = '" + item["ProductionOrderId"] + "' AND ProductCode = '" + item["ProductCode"] + "'";

                                    if (dsItemScanChild.Tables[0].DefaultView.Count > 0)
                                    {
                                        //edit
                                        DataRow dr = dsItemScanChild.Tables[0].DefaultView[0].Row;
                                        dr.BeginEdit();
                                        dr["IsDespatch"] = true;
                                        dr["InventoryReceiveDetailId"] = detailId;
                                        dr["UpdatedBy"] = identity.Name;
                                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                        dr.EndEdit();
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsFGDetail, dsInventoryReceive, dsInventoryMaterial, dsInventoryReceiveDetail, dsConsumptionByCosting, dsItemScanChild, dsProductionSummary);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void SaveFinishGoodsBookData(Dictionary<string, object> data, List<Dictionary<string, object>> WorkDayList, List<Dictionary<string, object>> FinishGoodsBookingDetailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string pOId = null;
                string MaterialMasterId = null;
                string ArticleId = null;

                bplib.clsGenID objGenID = new bplib.clsGenID();

                foreach (var item in FinishGoodsBookingDetailList)
                {
                    if (pOId == null)
                    {
                        pOId = "'" + item["ProductionOrderId"].ToString() + "'";
                    }
                    else
                    {
                        pOId += ",'" + item["ProductionOrderId"].ToString() + "'";
                    }

                    if (MaterialMasterId == null)
                    {
                        MaterialMasterId = "'" + item["MaterialMasterId"].ToString() + "'";
                    }
                    else
                    {
                        MaterialMasterId += ",'" + item["MaterialMasterId"].ToString() + "'";
                    }
                    if (ArticleId == null)
                    {
                        ArticleId = "'" + item["ArticleId"].ToString() + "'";
                    }
                    else
                    {
                        ArticleId += ",'" + item["ArticleId"].ToString() + "'";
                    }

                }

                DataSet dsMaster, dsFGDetail, dsProductionSummary, dsInventoryReceive, dsInventoryReceiveDetail, dsInventoryMaterial, dsConsumptionByCosting, dsFromConsumptionByCosting;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[FinishGoodsBooking] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[FinishGoodsBookingDetail] WHERE FinishGoodsBookingId='" + data["Id"] + "'", out dsFGDetail, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.InventoryReceive WHERE 1 = 2", out dsInventoryReceive, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.InventoryReceiveDetail WHERE 1 = 2", out dsInventoryReceiveDetail, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.ProductionSummary WHERE ProductionDate between '" + data["FromDate"] + @"' AND '" + data["ToDate"] + @"' AND ISNULL(FinishGoodsBookingId,'')='' AND EntityId='" + data["ProductionEntityId"] + @"' AND ProcessId='" + data["ProcessId"] + "' AND ISNULL(MaterialMasterId,'')<>''", out dsProductionSummary, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ConsumptionByCosting WHERE 1 = 2", out dsConsumptionByCosting, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[InventoryMaterial] where MaterialMasterId IN(" + MaterialMasterId + ") and ArticleId IN(" + ArticleId + ")  and CompanyId='" + identity.CompanyId + "' and PlantId='" + identity.PlantId + "'", out dsInventoryMaterial, false, "1");

                string _Id = null, masterId = null, detailId = null, iID = null, inventoryMaterialId = null;

                #region FinishGoodsBooking

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FinishGoodsBooking", out _Id);
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                int count = 0;
                if (FinishGoodsBookingDetailList != null)
                {
                    foreach (var item in FinishGoodsBookingDetailList)
                    {
                        count++;
                        DataView dv = new DataView(dsFGDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = masterId + "" + count;
                            item["FinishGoodsBookingId"] = masterId;
                            AddNewRow(dsFGDetail.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion

                #region InventoryReceive

                foreach (var item in WorkDayList)
                {
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceive", out iID);

                    DataRow drInventoryReceive = dsInventoryReceive.Tables[0].NewRow();
                    drInventoryReceive["Id"] = iID;
                    drInventoryReceive["CompanyGroupId"] = identity.CompanyGroupId;
                    drInventoryReceive["CompanyId"] = identity.CompanyId;
                    drInventoryReceive["PlantId"] = identity.PlantId;
                    drInventoryReceive["DocRefNo"] = iID;
                    drInventoryReceive["CurrencyId"] = data["CompanyCurrencyId"];//
                    drInventoryReceive["MaterialStorageId"] = data["MaterialStorageId"];
                    drInventoryReceive["ToCurrencyRate"] = 1;
                    drInventoryReceive["FixedAssetOrInventory"] = "Inventory";
                    drInventoryReceive["GRNType"] = "FG";
                    drInventoryReceive["EntityId"] = data["ProductionEntityId"].ToString();
                    drInventoryReceive["GRNDate"] = item["WorkDate"];
                    drInventoryReceive["DocDate"] = DBNull.Value;
                    drInventoryReceive["EntryDate"] = DateTime.Now;
                    drInventoryReceive["PODepended"] = false;
                    drInventoryReceive["AlongwithInvoice"] = false;
                    drInventoryReceive["IsNonCreditable"] = false;
                    drInventoryReceive["IsTaxApplicable"] = false;
                    drInventoryReceive["IsApproved"] = true;
                    drInventoryReceive["IsPaymentHold"] = false;
                    drInventoryReceive["IsNonVendor"] = false;
                    drInventoryReceive["IsFOC"] = false;
                    drInventoryReceive["IsInvoice"] = false;

                    drInventoryReceive["FinishGoodsBookingId"] = masterId;
                    drInventoryReceive["ProductionOrderId"] = item["ProductionOrderId"];
                    drInventoryReceive["AddedBy"] = identity.Name;
                    drInventoryReceive["AddedDate"] = DateTime.Now;
                    drInventoryReceive["AddedFromIP"] = identity.IPAddress;
                    dsInventoryReceive.Tables[0].Rows.Add(drInventoryReceive);
                }

                #endregion

                for (int i = 0; i < dsInventoryReceive.Tables[0].Rows.Count; i++)
                {
                    int detailIdCount = 0;
                    var detailData = FinishGoodsBookingDetailList.Where(xx => Convert.ToDateTime(xx["WorkDate"].ToString()) == Convert.ToDateTime(dsInventoryReceive.Tables[0].Rows[i]["GRNDate"].ToString())).ToList();
                    if (detailData != null)
                    {
                        foreach (var item in detailData)
                        {
                            detailIdCount++;
                            #region InventoryMaterial

                            //if (dsInventoryMaterial.Tables[0].Rows.Count > 0)
                            //{
                            dsInventoryMaterial.Tables[0].DefaultView.RowFilter = "MaterialMasterId='" + item["MaterialMasterId"].ToString() + "' AND ArticleId = '" + item["ArticleId"] + "' AND FirstCharacteristicsValueId = '" + item["FirstCharacteristicsValueId"] + "' AND SecondCharacteristicsValueId = '" + item["SecondCharacteristicsValueId"] + "'";

                            if (dsInventoryMaterial.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsInventoryMaterial.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                inventoryMaterialId = dr["Id"].ToString();
                                dr["TotalQty"] = Convert.ToDecimal(dr["TotalQty"].ToString()) + Convert.ToDecimal(item["Qty"].ToString());

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr.EndEdit();
                            }
                            else
                            {
                                objGenID.GenerateIDAuto("InventoryMaterial", out inventoryMaterialId);

                                DataRow drInventoryMaterial = dsInventoryMaterial.Tables[0].NewRow();
                                drInventoryMaterial["Id"] = inventoryMaterialId;
                                drInventoryMaterial["CompanyGroupId"] = identity.CompanyGroupId;
                                drInventoryMaterial["CompanyId"] = identity.CompanyId;
                                drInventoryMaterial["PlantId"] = identity.PlantId;
                                drInventoryMaterial["MaterialMasterId"] = item["MaterialMasterId"];
                                drInventoryMaterial["ArticleId"] = item["ArticleId"];

                                drInventoryMaterial["FirstCharacteristicsId"] = item["FirstCharacteristicsId"];
                                drInventoryMaterial["FirstCharacteristicsValueId"] = item["FirstCharacteristicsValueId"];
                                drInventoryMaterial["SecondCharacteristicsId"] = item["SecondCharacteristicsId"];
                                drInventoryMaterial["SecondCharacteristicsValueId"] = item["SecondCharacteristicsValueId"];

                                drInventoryMaterial["TotalQty"] = item["Qty"];
                                drInventoryMaterial["AvgRate"] = 0;

                                drInventoryMaterial["AddedBy"] = identity.Name;
                                drInventoryMaterial["AddedDate"] = DateTime.Now;
                                drInventoryMaterial["AddedFromIP"] = identity.IPAddress;

                                dsInventoryMaterial.Tables[0].Rows.Add(drInventoryMaterial);
                            }
                            //}


                            #endregion

                            #region InventoryReceiveDetail

                            DataRow drInventoryReceiveDetail = dsInventoryReceiveDetail.Tables[0].NewRow();
                            drInventoryReceiveDetail["Id"] = dsInventoryReceive.Tables[0].Rows[i]["Id"].ToString() + "-" + detailIdCount;
                            detailId = dsInventoryReceive.Tables[0].Rows[i]["Id"].ToString() + "-" + detailIdCount;
                            drInventoryReceiveDetail["InventoryReceiveId"] = dsInventoryReceive.Tables[0].Rows[i]["Id"].ToString();
                            drInventoryReceiveDetail["InventoryMaterialId"] = inventoryMaterialId;
                            drInventoryReceiveDetail["TransactionQty"] = item["Qty"];

                            drInventoryReceiveDetail["BaseQty"] = item["Qty"];
                            drInventoryReceiveDetail["GRNQty"] = item["Qty"];
                            drInventoryReceiveDetail["MaterialTranRate"] = Math.Round(Convert.ToDecimal(item["Rate"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 4);
                            drInventoryReceiveDetail["MaterialTranAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["TotalMaterialTranAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["TotalMaterialBooksCurrencyAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["GRNTotalAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["GrossAmount"] = Math.Round(Convert.ToDecimal(item["Amount"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 2);
                            drInventoryReceiveDetail["DiscountAmount"] = 0;
                            drInventoryReceiveDetail["BooksCurrencyBaseRate"] = Math.Round(Convert.ToDecimal(item["Rate"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 4);
                            drInventoryReceiveDetail["TrnCurrencyBaseRate"] = Math.Round(Convert.ToDecimal(item["Rate"]) * Convert.ToDecimal(data["ToCurrencyRate"]), 4);
                            drInventoryReceiveDetail["MaterialStorageId"] = data["MaterialStorageId"];

                            drInventoryReceiveDetail["BaseUoMFactor"] = 1;
                            drInventoryReceiveDetail["TotalTaxAmount"] = 0;
                            drInventoryReceiveDetail["ChargesTranAmount"] = 0;
                            drInventoryReceiveDetail["ChargesTaxTranAmount"] = 0;

                            drInventoryReceiveDetail["BaseIssueQty"] = 0;

                            drInventoryReceiveDetail["ShortageQty"] = 0;
                            drInventoryReceiveDetail["RejectionQty"] = 0;
                            drInventoryReceiveDetail["ApprovedQty"] = 0;
                            drInventoryReceiveDetail["ShortageRatePercent"] = 0;
                            drInventoryReceiveDetail["ShortageValue"] = 0;
                            drInventoryReceiveDetail["RejectRatePercent"] = 0;
                            drInventoryReceiveDetail["RejectValue"] = 0;
                            drInventoryReceiveDetail["RejectClamPercent"] = 0;
                            drInventoryReceiveDetail["ShortRejFlag"] = 0;

                            drInventoryReceiveDetail["PostDrGLGeneralInfoId"] = null;
                            drInventoryReceiveDetail["PostDrBudgetMasterId"] = null;
                            drInventoryReceiveDetail["PostCRGLGeneralInfoId"] = null;
                            drInventoryReceiveDetail["PostCRBudgetMasterId"] = null;
                            drInventoryReceiveDetail["PostCRActivityId"] = null;
                            drInventoryReceiveDetail["CapitalizeVoucherDetailId"] = null;
                            drInventoryReceiveDetail["IsAsset"] = item["IsAsset"];
                            drInventoryReceiveDetail["TransactionUoMId"] = item["UOM"];
                            drInventoryReceiveDetail["BaseUOMId"] = item["UOM"];

                            drInventoryReceiveDetail["AddedBy"] = identity.Name;
                            drInventoryReceiveDetail["AddedDate"] = DateTime.Now;
                            drInventoryReceiveDetail["AddedFromIP"] = identity.IPAddress;
                            dsInventoryReceiveDetail.Tables[0].Rows.Add(drInventoryReceiveDetail);
                            #endregion

                            #region ConsumptionByCosting

                            if (item["OrderCostingMasterTemplateId"] != null)
                            {
                                GetConsumptionByCostingData(item["OrderCostingMasterTemplateId"].ToString(), out dsFromConsumptionByCosting);
                                dsFromConsumptionByCosting.Tables[0].DefaultView.RowFilter = "CostingId='" + item["OrderCostingMasterTemplateId"].ToString() + "'";
                                for (int l = 0; l < dsFromConsumptionByCosting.Tables[0].DefaultView.Count; l++)
                                {
                                    DataRow drConsumptionByCosting = dsConsumptionByCosting.Tables[0].NewRow();
                                    CopyRow(dsFromConsumptionByCosting.Tables[0].DefaultView[l].Row, ref drConsumptionByCosting);
                                    drConsumptionByCosting["Id"] = detailId + (l + 1);
                                    drConsumptionByCosting["InventoryReceiveDetailId"] = detailId;
                                    drConsumptionByCosting["GrossConsumption"] = dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossConsumption"].ToString();
                                    drConsumptionByCosting["GrossAmount"] = dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossAmount"].ToString();
                                    drConsumptionByCosting["InPutCostingItemId"] = dsFromConsumptionByCosting.Tables[0].DefaultView[l]["InPutCostingItemId"].ToString();
                                    drConsumptionByCosting["TotalInputConsumption"] = Convert.ToDecimal(item["Qty"]) * Convert.ToDecimal(dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossConsumption"].ToString());

                                    drConsumptionByCosting["TotalInputStandardCost"] = Convert.ToDecimal(dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossAmount"].ToString()) * Convert.ToDecimal(dsFromConsumptionByCosting.Tables[0].DefaultView[l]["GrossConsumption"].ToString());

                                    dsConsumptionByCosting.Tables[0].Rows.Add(drConsumptionByCosting);
                                }

                            }
                            #endregion
                        }
                    }
                }

                #region ProductionSummary
                if (dsProductionSummary.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < dsProductionSummary.Tables[0].Rows.Count; j++)
                    {
                        dsProductionSummary.Tables[0].DefaultView.RowFilter = "Id='" + dsProductionSummary.Tables[0].Rows[j]["Id"].ToString() + "'";

                        if (dsProductionSummary.Tables[0].DefaultView.Count > 0)
                        {
                            //edit
                            DataRow dr = dsProductionSummary.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr["FinishGoodsBookingId"] = masterId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr.EndEdit();
                        }
                    }
                }
                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsFGDetail, dsInventoryReceive, dsInventoryMaterial, dsInventoryReceiveDetail, dsProductionSummary, dsConsumptionByCosting);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }

        private string GetFinishGoodsBookingDetailPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FinishGoodsBookingDetail", out sID);
            return sID;
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

        public IEnumerable<object> GetProcessCbo(string entityId)
        {
            try
            {
                var sql = @"SELECT DISTINCT P.Id AS Value,P.UserName AS Text FROM dbo.EntityConfig EC
							JOIN HKP.Process P ON P.Id=EC.ConsumptionProcessId
							WHERE EC.EntityId='" + entityId + "' AND P.Active=1";
                return _sqlRepository.GetCombo(sql, "Value", "Text");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetProductionOrderDataList(string entityId, string processId)
        {

            try
            {
                string sql = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, PD.Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,SO.Qty
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
									LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = PD.ProductionOrderId
								   WHERE  E.Id='" + entityId + "' AND POSP.ProcessId = '" + processId + "' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetLineItemData(string entityId, string processId, string productionOrderId, string masterId)
        {
            try
            {
                var sql = @"SELECT DISTINCT mo.MasterOrderNo
                                    ,'' Id
                                    ,''ProductCodeList
									,ISNULL(so.Id,'') SalesOrderId
									,PO.Id ProductionOrderId
									,mm.Id MaterialMasterId
									,mm.UserName MaterialMaster
									,ISNULL(mma.Id, '') ArticleId
									,ISNULL(mma.StandardName, '') Article
									,moi.ProductLibraryId
									,ISNULL(PL.Code,'') ProductCode
									,ISNULL(POD.ProductionOrderId, '') POId
									,BU.UserName Buyer
									,mo.TotalQty OrderQty
									,CEILING(SO.PlannedQty) PlannedQty,FB.Qty OtherBookedQty,0 Qty,ISNULL(B.Rate,0) Rate,B.OrderCostingMasterTemplateId,B.CostingItem
									,ISNULL(MO.BuyerReferenceNo,'') BuyerOrder,ISNULL(MO.OwnReferenceNo,'') OwnOrder,ISNULL(moi.BuyerReferenceNo,'') BuyerItem,ISNULL(moi.OwnReferenceNo,'') OwnItem
									FROM TRN.ProductionOrderDetail POD
									LEFT JOIN (
									SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
									,s.Id,s.MasterOrderItemId
									FROM trn.SalesOrder AS s
									INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
									GROUP BY S.Id,s.MasterOrderItemId
									) so ON POD.SalesOrderId = SO.Id
									LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
									LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
									LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
									LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
									LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
									LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
									INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
									) OS ON OS.ProductionOrderId = POD.ProductionOrderId
									LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
									LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
									LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
									LEFT JOIN dbo.ProductLibrary PL ON PL.Id=moi.ProductLibraryId
									LEFT JOIN (
									select MOI.Id,A.OrderCostingMasterTemplateId, SUM(A.Rate) Rate,CI.UserName CostingItem
									from TRN.MasterOrderItem MOI
									LEFT JOIN [dbo].[OrderCostingMasterTemplate] OCMT ON OCMT.Id=MOI.OrderCostingMasterTemplateId
									LEFT JOIN
									( 
									Select DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate from [dbo].[OrderProcurementCostingDirectMaterial] DM
									UNION
									Select DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate from [dbo].[OrderProcurementCostingDirectProcess] DP
									UNION
									Select OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate from [dbo].[OrderProcurementCostingOperation] OP
									UNION
									Select P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate from [dbo].[OrderProcurementCostingProfit] P
									UNION
									Select SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate from [dbo].[OrderProcurementCostingSalesExpense] SE
									UNION
									Select VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate from [dbo].[OrderProcurementCostingValueLoss] VL
									) 
									A ON A.OrderCostingMasterTemplateId=OCMT.OrderCostingMasterTemplateId
									JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
									JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
									WHERE ISNULL(ConsiderForFGValuation,0)=1 
									GROUP BY MOI.Id,A.OrderCostingMasterTemplateId,CI.UserName
									) B ON B.Id=moi.Id AND B.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId
									LEFT JOIN (
									SELECT SUM(B.Qty) Qty,A.ProductionOrderId,B.SalesOrderId
									FROM dbo.[FinishGoodsBooking] A 
									JOIN dbo.[FinishGoodsBookingDetail] B ON A.Id=B.FinishGoodsBookingId
									WHERE ISNULL(B.ProductLibraryId,'')<>'' AND ISNULL(A.Id,'') <>'" + masterId + @"'
									GROUP BY A.ProductionOrderId,B.SalesOrderId
									) FB ON FB.ProductionOrderId=PO.Id AND FB.SalesOrderId=SO.Id
									WHERE  POSP.ProcessId = '" + processId + "' AND PO.EntityId='" + entityId + "'  AND PO.Id='" + productionOrderId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetScanPackingData(string fromDate, string toDate)
        {
            try
            {
                string sql = @"SELECT CONVERT (bit,0) Active,NULL AS COSTList, '' Qty,sc.ProductCode, PL.Id ProductLibraryId , PO.Id ProductionOrderId,FGQty=SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END)
								,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,MasterOrderItemNo=STUFF((select distinct ','+MOI.Id from 
								                                 TRN.MasterOrderItem MOI
                                                                 JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=MOI.Id
                                                                 JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                ,MaterialMaster=STUFF((select distinct ','+MM.UserName from 
									                            MST.MaterialMaster MM
								                                JOIN TRN.MasterOrderItem MI ON MI.MaterialMasterId=MM.Id                                                                 
                                                                 JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=MI.Id
                                                                 JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                        
							    ,Article=STUFF((select distinct ','+MMA.StandardName from 
									                            MST.MaterialMasterArticle MMA
								                                JOIN TRN.MasterOrderItem MI ON MI.ArticleId=MMA.Id                                                                 
                                                                 JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=MI.Id
                                                                 JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')     
	                        ,ISNULL(B.Rate,0) Rate,B.OrderCostingMasterTemplateId
							FROM dbo.ItemScanChild sc
	                        LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
							LEFT JOIN dbo.ItemScan ISN ON ISN.Id=SC.MasterId
	    
							LEFT JOIN TRN.ProductionOrder PO ON PO.Id=SC.POId
							LEFT JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=PO.Id													
							
							LEFT JOIN 
							(
								SELECT DISTINCT COST.ProductionOrderId, COST.OrderCostingMasterTemplateId,COST.Rate FROM (select po.ProductionOrderId, moi.Id,a.OrderCostingMasterTemplateId,sum(a.rate) AS Rate from trn.MasterOrderItem moi 
								 JOIN
									( 
									Select DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate from [dbo].[OrderProcurementCostingDirectMaterial] DM
									UNION
									Select DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate from [dbo].[OrderProcurementCostingDirectProcess] DP
									UNION
									Select OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate from [dbo].[OrderProcurementCostingOperation] OP
									UNION
									Select P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate from [dbo].[OrderProcurementCostingProfit] P
									UNION
									Select SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate from [dbo].[OrderProcurementCostingSalesExpense] SE
									UNION
									Select VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate from [dbo].[OrderProcurementCostingValueLoss] VL
									)  AS 	A ON A.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId

									left JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
									left JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
									join (select POD.ProductionOrderId,SO.MasterOrderItemId from TRN.ProductionOrderDetail POD
									join trn.salesOrder SO ON SO.id=pod.SalesOrderId group by POD.ProductionOrderId,SO.MasterOrderItemId) AS PO on PO.MasterOrderItemId=MOI.Id
									WHERE ISNULL(ConsiderForFGValuation,0)=1 AND moi.OrderCostingMasterTemplateId<>''
								group by  po.ProductionOrderId,moi.Id,a.OrderCostingMasterTemplateId) AS COST
							) B ON B.ProductionOrderId=PO.Id

	                        WHERE ISN.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(SC.FinishGoodsBookingId,'')=''
							GROUP BY SC.ProductCode, PO.Id, PL.Id,B.Rate,B.OrderCostingMasterTemplateId";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetCostingItemDetailData(string costingId)
        {
            try
            {
                string sql = @"SELECT A.GrossConsumption, A.Rate,SUM(A.GrossAmount)GrossAmount,CI.UserName CostingItem, CC.UserName CostingComponent,CC.CostingSegment,ConsiderForFGValuation=CASE WHEN CC.ConsiderForFGValuation=1 THEN 'Yes' ELSE 'No'  END
								FROM dbo.OrderCostingMasterTemplate CT
								LEFT JOIN
									( 
									SELECT DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossConsumption,DM.Rate,DM.GrossAmount FROM [dbo].OrderPreCostingDirectMaterial DM
									UNION
									SELECT DP.CostingItemId,DP.OrderCostingMasterTemplateId,0 GrossConsumption,0 Rate,DP.Amount GrossAmount FROM [dbo].OrderPreCostingDirectProcess DP
									UNION
									SELECT OP.CostingItemId,OP.OrderCostingMasterTemplateId,0 GrossConsumption,0 Rate,OP.[Value] GrossAmount FROM [dbo].OrderPreCostingOperation OP
									UNION
									SELECT P.CostingItemId,P.OrderCostingMasterTemplateId,0 GrossConsumption,0 Rate,P.[Value] GrossAmount FROM [dbo].OrderPreCostingProfit P
									UNION
									SELECT SE.CostingItemId,SE.OrderCostingMasterTemplateId,0 GrossConsumption,0 Rate,SE.[Value] GrossAmount FROM [dbo].OrderPreCostingSalesExpense SE
									UNION
									SELECT VL.CostingItemId,VL.OrderCostingMasterTemplateId,0 GrossConsumption,0 Rate,VL.[Value] GrossAmount FROM [dbo].OrderPreCostingValueLoss VL
									) 
								A ON A.OrderCostingMasterTemplateId=CT.Id
								left JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
								left JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
								WHERE CT.Id='" + costingId + @"'
								GROUP BY A.GrossConsumption, CC.UserName,CI.UserName,A.Rate,CC.ConsiderForFGValuation,CC.CostingSegment";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetCostingItemData(string productionOrderId)
        {
            try
            {
                string sql = @"select distinct COST.ProductionOrderId, COST.OrderCostingMasterTemplateId,COST.Rate,COST.UserName FROM (select po.ProductionOrderId, moi.Id,a.OrderCostingMasterTemplateId,sum(a.rate) AS Rate,OCMT.UserName 
from trn.MasterOrderItem moi 
 JOIN
	( 
	Select DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate from [dbo].[OrderProcurementCostingDirectMaterial] DM
	UNION
	Select DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate from [dbo].[OrderProcurementCostingDirectProcess] DP
	UNION
	Select OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate from [dbo].[OrderProcurementCostingOperation] OP
	UNION
	Select P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate from [dbo].[OrderProcurementCostingProfit] P
	UNION
	Select SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate from [dbo].[OrderProcurementCostingSalesExpense] SE
	UNION
	Select VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate from [dbo].[OrderProcurementCostingValueLoss] VL
	)  AS 	A ON A.OrderCostingMasterTemplateId=moi.OrderCostingMasterTemplateId

	left JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
	left JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
	left JOIN [dbo].[OrderCostingMasterTemplate] OCMT ON OCMT.Id=A.OrderCostingMasterTemplateId
	join (select POD.ProductionOrderId,SO.MasterOrderItemId from TRN.ProductionOrderDetail POD
	join trn.salesOrder SO ON SO.id=pod.SalesOrderId group by POD.ProductionOrderId,SO.MasterOrderItemId) AS PO on PO.MasterOrderItemId=MOI.Id
	WHERE ISNULL(ConsiderForFGValuation,0)=1 AND PO.ProductionOrderId='" + productionOrderId + @"'
AND moi.OrderCostingMasterTemplateId<>''
group by  po.ProductionOrderId,moi.Id,a.OrderCostingMasterTemplateId,OCMT.UserName) AS COST";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetBookedAndBalancedData(string productionOrderId)
        {
            try
            {
                string sql = @"SELECT sc.ProductCode, PL.Id ProductLibraryId , sc.POId,FGQty=SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END),ISNULL(FB.Qty,0) BookedQty
                            , Balance=(SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END)-ISNULL(FB.Qty,0))
	                        FROM dbo.ItemScanChild sc
	                        LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
	                        LEFT JOIN (
	                        SELECT SUM(B.Qty) Qty,A.ProductionOrderId ,PL.Code
	                        FROM dbo.[FinishGoodsBooking] A 
	                        JOIN dbo.[FinishGoodsBookingDetail] B ON A.Id=B.FinishGoodsBookingId
	                        JOIN dbo.ProductLibrary PL ON PL.Id=B.ProductLibraryId
	                        WHERE ISNULL(B.ProductLibraryId,'')<>''
	                        GROUP BY A.ProductionOrderId,PL.Code
	                        ) FB ON FB.ProductionOrderId=SC.POId AND FB.Code=SC.ProductCode
	                        WHERE sc.POId='" + productionOrderId + @"'
							GROUP BY SC.ProductCode, SC.POId, PL.Id,FB.Qty
							--Having (SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END)-ISNULL(FB.Qty,0))>0
							";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedBookedAndBalancedData(string productionOrderId)
        {
            try
            {
                string sql = @"SELECT sc.ProductCode, PL.Id ProductLibraryId , sc.POId,FGQty=SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END),ISNULL(FB.Qty,0) BookedQty, Balance=(SUM(CASE WHEN SC.IsDespatch=0 THEN sc.NetWeight ELSE 0 END)-ISNULL(FB.Qty,0))
	                        FROM dbo.ItemScanChild sc
	                        LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
	                        LEFT JOIN (
	                        SELECT SUM(B.Qty) Qty,A.Id,A.ProductionOrderId ,PL.Code
	                        FROM dbo.[FinishGoodsBooking] A 
	                        JOIN dbo.[FinishGoodsBookingDetail] B ON A.Id=B.FinishGoodsBookingId
	                        JOIN dbo.ProductLibrary PL ON PL.Id=B.ProductLibraryId
	                        WHERE ISNULL(B.ProductLibraryId,'')<>''
	                        GROUP BY A.Id,A.ProductionOrderId,PL.Code
	                        ) FB ON FB.ProductionOrderId=SC.POId AND FB.Code=SC.ProductCode
	                        WHERE sc.POId='" + productionOrderId + "' GROUP BY SC.ProductCode, SC.POId, PL.Id,FB.Qty";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetItemScanChildData(string entityId, string fromDate, string toDate, string level)
        {
            try
            {
                string sql = "";
                if (level == "Costing")
                {
                    sql = @"SELECT CAST(0 AS bit) Flag,'' Id,PD.ProductionOrderId,PD.ProductCode,PD.MasterOrderItemId,SONo = STUFF((SELECT DISTINCT ',' + XSO.Id
							FROM trn.SalesOrder XSO
							WHERE XSO.Id = PD.SalesOrderId
							FOR XML path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PD.MaterialMasterId,PD.ArticleId,PD.MaterialMaster,PD.Article,PD.OrderCostingMasterTemplateId
							,Qty=CONVERT(decimal(18,2),ISNULL(A.Qty,0.00)),Rate=CONVERT(decimal(18,4),ISNULL(PD.Rate,0.0000))
							,Amount=CONVERT(decimal(18,2),(ISNULL(A.Qty,0.00)*ISNULL(PD.Rate,0.0000)))
							,PD.IsAsset,PD.UOM,PD.Buyer,PD.BuyerReferenceNo,PD.TotalQty ItemQty,ISNULL(B.UnBookedQty,0)UnBookedQty,ISNULL(C.BookedQty,0)BookedQty,Balance=ISNULL((PD.TotalQty-C.BookedQty),0)
FROM dbo.ItemScanChild SC 						
LEFT JOIN dbo.ItemScan ISN ON ISN.Id=SC.MasterId
LEFT JOIN (
	SELECT DISTINCT POD.ProductionOrderId,ISNULL(B.Rate,0)Rate,MOI.Id MasterOrderItemId,B.OrderCostingMasterTemplateId
	,MM.Id MaterialMasterId,MMA.Id ArticleId,MM.UserName MaterialMaster,MMA.StandardName Article,MM.IsAsset,U.Id UOM,PL.Code ProductCode,BR.UserName Buyer,SUM(SO.Qty)TotalQty,MOI.BuyerReferenceNo,POD.SalesOrderId
FROM TRN.ProductionOrder PO
LEFT JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId = PO.Id
LEFT JOIN  TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
LEFT JOIN TRN.MasterOrderItem MOI ON moi.Id = so.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON mo.Id = MOI.MasterOrderId
LEFT JOIN HKP.Buyer BR ON BR.Id=MO.BuyerId
LEFT JOIN dbo.ProductLibrary PL ON MOI.ProductLibraryId=PL.Id
LEFT JOIN MST.MaterialMaster MM ON MM.Id=moi.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=moi.ArticleId
LEFT JOIN SCS.UnitOfMeasurement U ON MM.BaseUoMId = U.Id					
LEFT JOIN dbo.OrderCostingMasterTemplate AS CT ON CT.Id = MOI.OrderCostingMasterTemplateId					
LEFT JOIN (
	SELECT DISTINCT COST.OrderCostingMasterTemplateId,COST.Rate
	FROM (
		SELECT A.OrderCostingMasterTemplateId,sum(ISNULL(A.rate,0)) AS Rate
		FROM OrderCostingMasterTemplate CMT
		JOIN (
			SELECT DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate
			FROM [dbo].OrderProcurementCostingDirectMaterial DM				
			UNION				
			SELECT DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate
			FROM [dbo].OrderProcurementCostingDirectProcess DP				
			UNION				
			SELECT OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate
			FROM [dbo].OrderProcurementCostingOperation OP
			UNION
			SELECT P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate
			FROM [dbo].OrderProcurementCostingProfit P
			UNION
			SELECT SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate
			FROM [dbo].OrderProcurementCostingSalesExpense SE
			UNION
			SELECT VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate
			FROM [dbo].OrderProcurementCostingValueLoss VL
			) AS A ON A.OrderCostingMasterTemplateId = CMT.Id 
		LEFT JOIN [HKP].[CostingItem] CI ON CI.Id = A.CostingItemId
		LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id = CI.CostingComponentId
		Where CC.ConsiderForFGValuation=1
		GROUP BY a.OrderCostingMasterTemplateId
		) AS COST
	) B ON B.OrderCostingMasterTemplateId = CT.Id
GROUP BY  POD.ProductionOrderId,B.Rate
,MOI.Id,B.OrderCostingMasterTemplateId
	,MM.Id,MMA.Id,MM.UserName,MMA.StandardName,MM.IsAsset,U.Id,PL.Code,BR.UserName,MOI.BuyerReferenceNo,POD.SalesOrderId
) PD ON PD.ProductionOrderId = SC.POId

INNER JOIN(
Select Qty=SUM(CASE WHEN ISC.IsDespatch=0 THEN ISC.NetWeight ELSE 0 END),ISC.POId,ISC.ProductCode 
	from dbo.ItemScanChild ISC 
	INNER JOIN dbo.ItemScan ISM ON ISM.Id=ISC.MasterId 
	LEFT JOIN TRN.ProductionOrder PO ON PO.Id=ISC.POId
	WHERE ISM.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(ISC.InventoryReceiveDetailId,'')='' AND ISNULL(ISC.PackingId,'')=''  
    AND  PO.EntityId='" + entityId + @"' 
	GROUP BY ISC.PoId,ISC.ProductCode
) A ON A.POId= PD.ProductionOrderId AND PD.ProductCode=A.ProductCode

LEFT JOIN(
Select UnBookedQty=SUM(CASE WHEN ISC.IsDespatch=0 THEN ISC.NetWeight ELSE 0 END),ISC.POId,ISC.ProductCode 
	from dbo.ItemScanChild ISC 
	INNER JOIN dbo.ItemScan ISM ON ISM.Id=ISC.MasterId 
	LEFT JOIN TRN.ProductionOrder PO ON PO.Id=ISC.POId
	WHERE ISNULL(ISC.InventoryReceiveDetailId,'')='' AND ISNULL(ISC.PackingId,'')=''  AND  PO.EntityId='" + entityId + @"'
    AND ISM.WorkDate NOT IN(Select FORMAT(WorkDate,'dd-MMM-yyyy') from dbo.ItemScan Where WorkDate between '" + fromDate + @"' AND '" + toDate + @"')
	GROUP BY ISC.PoId,ISC.ProductCode
) B ON B.POId= PD.ProductionOrderId AND PD.ProductCode=B.ProductCode

LEFT JOIN(
Select BookedQty=SUM(CASE WHEN ISC.IsDespatch=0 THEN ISC.NetWeight ELSE 0 END),ISC.POId,ISC.ProductCode 
	from dbo.ItemScanChild ISC 
	INNER JOIN dbo.ItemScan ISM ON ISM.Id=ISC.MasterId 
	LEFT JOIN TRN.ProductionOrder PO ON PO.Id=ISC.POId 
	WHERE ISNULL(ISC.InventoryReceiveDetailId,'')<>'' AND ISNULL(ISC.PackingId,'')=''  AND  PO.EntityId='" + entityId + @"'
	GROUP BY ISC.PoId,ISC.ProductCode
) C ON C.POId= PD.ProductionOrderId AND PD.ProductCode=C.ProductCode

WHERE ISN.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(SC.InventoryReceiveDetailId,'')='' AND ISNULL(SC.PackingId,'')='' 
--AND  PO.EntityId='" + entityId + @"'
GROUP BY PD.ProductionOrderId,PD.ProductCode,PD.OrderCostingMasterTemplateId,PD.Rate,PD.MaterialMaster,PD.MaterialMasterId,PD.Article,PD.ArticleId,PD.IsAsset,PD.UOM,PD.TotalQty,PD.Buyer,PD.MasterOrderItemId,A.Qty,B.UnBookedQty,C.BookedQty,PD.BuyerReferenceNo,PD.SalesOrderId
UNION ALL
SELECT CAST(0 AS bit) Flag,'' Id, ProductionOrderId,ProductCode,''MasterOrderItemId,''SONo,''MaterialMasterId,''ArticleId,'' MaterialMaster,''Article,'' OrderCostingMasterTemplateId,Qty=CONVERT(decimal(18,2),Qty),Rate=CONVERT(decimal(18,4),0), Amount=CONVERT(decimal(18,2),0)
,0 IsAsset,''UOM,''Buyer,'' BuyerReferenceNo,0 ItemQty,0 UnBookedQty,0 BookedQty,0 Balance from 
(
	Select Qty=SUM(CASE WHEN ISC.IsDespatch=0 THEN ISC.NetWeight ELSE 0 END),ISC.POId ProductionOrderId,ISC.ProductCode 
		from dbo.ItemScanChild ISC 
		INNER JOIN dbo.ItemScan ISM ON ISM.Id=ISC.MasterId 
	LEFT JOIN TRN.ProductionOrder PO ON PO.Id=ISC.POId 
		WHERE ISM.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(ISC.InventoryReceiveDetailId,'')='' AND ISNULL(ISC.PackingId,'')='' AND  PO.EntityId='" + entityId + @"'
		GROUP BY ISC.PoId,ISC.ProductCode
) A
where not exists
(
	select * from
	(	
		SELECT DISTINCT POD.ProductionOrderId,ISNULL(PL.Code,'') ProductCode
		FROM TRN.ProductionOrder PO
		LEFT JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId = PO.Id
		LEFT JOIN TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
		LEFT JOIN TRN.MasterOrderItem MOI ON moi.Id = so.MasterOrderItemId
		LEFT JOIN dbo.ProductLibrary PL ON MOI.ProductLibraryId=PL.Id
	) B where A.ProductionOrderId=B.ProductionOrderId and A.ProductCode=B.ProductCode
)";
                }
                else if (level == "QBOQ")
                {
                    sql = @" SELECT CAST(0 AS bit) Flag,'' Id,PD.ProductionOrderId,PD.ProductCode,PD.MasterOrderItemId,SONo = STUFF((SELECT DISTINCT ',' + XSO.Id

                            FROM trn.SalesOrder XSO

                            WHERE XSO.Id = PD.SalesOrderId

                            FOR XML path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PD.MaterialMasterId,PD.ArticleId,PD.MaterialMaster,PD.Article,NULL OrderCostingMasterTemplateId
                            , Qty = CONVERT(decimal(18, 2), ISNULL(A.Qty, 0.00)), Rate = CONVERT(decimal(18, 4), ISNULL(PD.Rate, 0.0000))
                            , Amount = CONVERT(decimal(18, 2), (ISNULL(A.Qty, 0.00) * ISNULL(PD.Rate, 0.0000)))
                            , PD.IsAsset,PD.UOM,PD.Buyer,PD.BuyerReferenceNo,PD.TotalQty ItemQty, ISNULL(B.UnBookedQty, 0)UnBookedQty,ISNULL(C.BookedQty, 0)BookedQty,Balance = ISNULL((PD.TotalQty - C.BookedQty), 0)
FROM dbo.ItemScanChild SC
LEFT JOIN dbo.ItemScan ISN ON ISN.Id = SC.MasterId
LEFT JOIN(
    SELECT DISTINCT POD.ProductionOrderId, ISNULL(QB.Rate,0)Rate,MOI.Id MasterOrderItemId
     , MM.Id MaterialMasterId, MMA.Id ArticleId, MM.UserName MaterialMaster, MMA.StandardName Article, MM.IsAsset,U.Id UOM, PL.Code ProductCode, BR.UserName Buyer, SUM(SO.Qty)TotalQty,MOI.BuyerReferenceNo,POD.SalesOrderId
         FROM TRN.ProductionOrder PO
LEFT JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId = PO.Id
LEFT JOIN  TRN.SalesOrder SO ON SO.Id = POD.SalesOrderId
LEFT JOIN TRN.MasterOrderItem MOI ON moi.Id = so.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON mo.Id = MOI.MasterOrderId
LEFT JOIN HKP.Buyer BR ON BR.Id = MO.BuyerId
LEFT JOIN dbo.ProductLibrary PL ON MOI.ProductLibraryId = PL.Id
LEFT JOIN MST.MaterialMaster MM ON MM.Id = moi.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = moi.ArticleId
LEFT JOIN(select MasterOrderItemId, sum(grossconsumption) Rate from dbo.QuickBOQ group by MasterOrderItemId) QB ON QB.MasterOrderItemId = MOI.Id
LEFT JOIN SCS.UnitOfMeasurement U ON MM.BaseUoMId = U.Id


GROUP BY  POD.ProductionOrderId
,MOI.Id
	,MM.Id,MMA.Id,MM.UserName,MMA.StandardName,MM.IsAsset,U.Id,PL.Code,BR.UserName,MOI.BuyerReferenceNo,POD.SalesOrderId,QB.Rate
) PD ON PD.ProductionOrderId = SC.POId

INNER JOIN(
Select Qty = SUM(CASE WHEN ISC.IsDespatch = 0 THEN ISC.NetWeight ELSE 0 END), ISC.POId, ISC.ProductCode
    from dbo.ItemScanChild ISC

    INNER JOIN dbo.ItemScan ISM ON ISM.Id= ISC.MasterId

    LEFT JOIN TRN.ProductionOrder PO ON PO.Id= ISC.POId

    WHERE ISM.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(ISC.InventoryReceiveDetailId,'')= '' AND ISNULL(ISC.PackingId,'')= ''
    AND PO.EntityId = '" + entityId + @"'

    GROUP BY ISC.PoId,ISC.ProductCode
) A ON A.POId = PD.ProductionOrderId AND PD.ProductCode = A.ProductCode

LEFT JOIN(
Select UnBookedQty = SUM(CASE WHEN ISC.IsDespatch = 0 THEN ISC.NetWeight ELSE 0 END), ISC.POId, ISC.ProductCode
    from dbo.ItemScanChild ISC

    INNER JOIN dbo.ItemScan ISM ON ISM.Id= ISC.MasterId

    LEFT JOIN TRN.ProductionOrder PO ON PO.Id= ISC.POId

    WHERE ISNULL(ISC.InventoryReceiveDetailId,'')= '' AND ISNULL(ISC.PackingId,'')= ''  AND PO.EntityId = '" + entityId + @"'
    AND ISM.WorkDate NOT IN(Select FORMAT(WorkDate, 'dd-MMM-yyyy') from dbo.ItemScan Where WorkDate between '" + fromDate + @"' AND '" + toDate + @"')

    GROUP BY ISC.PoId,ISC.ProductCode
) B ON B.POId = PD.ProductionOrderId AND PD.ProductCode = B.ProductCode

LEFT JOIN(
Select BookedQty = SUM(CASE WHEN ISC.IsDespatch = 0 THEN ISC.NetWeight ELSE 0 END), ISC.POId, ISC.ProductCode
    from dbo.ItemScanChild ISC

    INNER JOIN dbo.ItemScan ISM ON ISM.Id= ISC.MasterId

    LEFT JOIN TRN.ProductionOrder PO ON PO.Id= ISC.POId

    WHERE ISNULL(ISC.InventoryReceiveDetailId,'')<> '' AND ISNULL(ISC.PackingId,'')= ''  AND PO.EntityId = '" + entityId + @"'

    GROUP BY ISC.PoId,ISC.ProductCode
) C ON C.POId = PD.ProductionOrderId AND PD.ProductCode = C.ProductCode

WHERE ISN.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(SC.InventoryReceiveDetailId,'')= '' AND ISNULL(SC.PackingId,'')= ''
 --AND PO.EntityId = '" + entityId + @"'
GROUP BY PD.ProductionOrderId,PD.ProductCode ,PD.Rate,PD.MaterialMaster,PD.MaterialMasterId,PD.Article,PD.ArticleId,PD.IsAsset,PD.UOM,PD.TotalQty,PD.Buyer,PD.MasterOrderItemId,A.Qty,B.UnBookedQty,C.BookedQty,PD.BuyerReferenceNo,PD.SalesOrderId
UNION ALL
SELECT CAST(0 AS bit) Flag,'' Id, ProductionOrderId,ProductCode,''MasterOrderItemId,''SONo,''MaterialMasterId,''ArticleId,'' MaterialMaster,''Article,'' OrderCostingMasterTemplateId,Qty = CONVERT(decimal(18, 2), Qty),Rate = CONVERT(decimal(18, 4), 0), Amount = CONVERT(decimal(18, 2), 0)
,0 IsAsset,''UOM,''Buyer,'' BuyerReferenceNo,0 ItemQty,0 UnBookedQty,0 BookedQty,0 Balance from
(
    Select Qty = SUM(CASE WHEN ISC.IsDespatch = 0 THEN ISC.NetWeight ELSE 0 END), ISC.POId ProductionOrderId, ISC.ProductCode
        from dbo.ItemScanChild ISC

        INNER JOIN dbo.ItemScan ISM ON ISM.Id= ISC.MasterId

    LEFT JOIN TRN.ProductionOrder PO ON PO.Id= ISC.POId

        WHERE ISM.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(ISC.InventoryReceiveDetailId,'')= '' AND ISNULL(ISC.PackingId,'')= '' AND PO.EntityId = '" + entityId + @"'

        GROUP BY ISC.PoId,ISC.ProductCode
) A
where not exists
(
    select* from

    (
        SELECT DISTINCT POD.ProductionOrderId, ISNULL(PL.Code,'') ProductCode,PL.*
         FROM TRN.ProductionOrder PO

        LEFT JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId = PO.Id

        LEFT JOIN TRN.SalesOrder SO ON SO.Id = POD.SalesOrderId

        LEFT JOIN TRN.MasterOrderItem MOI ON moi.Id = so.MasterOrderItemId

        LEFT JOIN dbo.ProductLibrary PL ON MOI.ProductLibraryId = PL.Id
	) B where A.ProductionOrderId = B.ProductionOrderId and A.ProductCode = B.ProductCode
)";
                }

                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetDateWiseDetailDataData(string EntityId, string fromDate, string toDate, string POId, string ProductCode)
        {

            try
            {

                string sql = @"SELECT PD.ProductionOrderId,PD.ProductCode,PD.MasterOrderItemId
							,PD.MaterialMasterId,PD.ArticleId,PD.MaterialMaster,PD.Article,PD.OrderCostingMasterTemplateId
							,A.Qty,ISNULL(PD.Rate,0)Rate,Amount=FORMAT((A.Qty*ISNULL(PD.Rate,0)),'N2')
							,PD.IsAsset,PD.UOM,FORMAT(A.WorkDate,'dd-MMM-yyyy')WorkDate
						FROM dbo.ItemScanChild SC 						
						LEFT JOIN dbo.ItemScan ISN ON ISN.Id=SC.MasterId
						LEFT JOIN (
					SELECT DISTINCT POD.ProductionOrderId,ISNULL(B.Rate, 0) Rate,MOI.Id MasterOrderItemId,B.OrderCostingMasterTemplateId
					 ,MM.Id MaterialMasterId,MMA.Id ArticleId,MM.UserName MaterialMaster,MMA.StandardName Article,MM.IsAsset,U.Id UOM,PL.Code ProductCode
					FROM TRN.ProductionOrder PO
					LEFT JOIN TRN.ProductionOrderDetail POD ON POD.ProductionOrderId = PO.Id
					LEFT JOIN  TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
					LEFT JOIN TRN.MasterOrderItem MOI ON moi.Id = so.MasterOrderItemId
					LEFT JOIN dbo.ProductLibrary PL ON MOI.ProductLibraryId=PL.Id
					LEFT JOIN MST.MaterialMaster MM ON MM.Id=moi.MaterialMasterId
					LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=moi.ArticleId
                    LEFT JOIN SCS.UnitOfMeasurement U ON MM.BaseUoMId = U.Id
					LEFT JOIN dbo.OrderCostingMasterTemplate AS CT ON CT.Id = MOI.OrderCostingMasterTemplateId
					LEFT JOIN (
						SELECT DISTINCT COST.OrderCostingMasterTemplateId,COST.Rate
						FROM (
							SELECT A.OrderCostingMasterTemplateId,sum(A.rate) AS Rate
							FROM OrderCostingMasterTemplate CMT
							JOIN (
								SELECT DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate
								FROM [dbo].OrderProcurementCostingDirectMaterial DM				
								UNION				
								SELECT DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate
								FROM [dbo].OrderProcurementCostingDirectProcess DP				
								UNION				
								SELECT OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate
								FROM [dbo].OrderProcurementCostingOperation OP
								UNION
								SELECT P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate
								FROM [dbo].OrderProcurementCostingProfit P
								UNION
								SELECT SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate
								FROM [dbo].OrderProcurementCostingSalesExpense SE
								UNION
								SELECT VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate
								FROM [dbo].OrderProcurementCostingValueLoss VL
								) AS A ON A.OrderCostingMasterTemplateId = CMT.Id 
							LEFT JOIN [HKP].[CostingItem] CI ON CI.Id = A.CostingItemId
							LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id = CI.CostingComponentId
							Where CC.ConsiderForFGValuation=1
							GROUP BY a.OrderCostingMasterTemplateId
							) AS COST
						) B ON B.OrderCostingMasterTemplateId = CT.Id
					) PD ON PD.ProductionOrderId = SC.POId					
					INNER JOIN
					(Select Qty=ROUND(CAST(SUM(CASE WHEN ISC.IsDespatch=0 THEN ISC.NetWeight ELSE 0 END) AS DECIMAL(18,2)), 2),ISC.POId,ISC.ProductCode,ISM.WorkDate 
					from dbo.ItemScanChild ISC 
					LEFT JOIN dbo.ItemScan ISM ON ISM.Id=ISC.MasterId 
					WHERE ISM.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(ISC.InventoryReceiveDetailId,'')='' AND ISNULL(ISC.PackingId,'')='' AND ISC.POId IN (Select Id from TRN.ProductionOrder Where EntityId='" + EntityId + @"') AND ISC.POId " + POId + @" AND ISC.ProductCode " + ProductCode + @"
                    GROUP BY ISC.PoId,ISC.ProductCode,ISM.WorkDate
					) A ON A.POId= PD.ProductionOrderId AND PD.ProductCode=A.ProductCode
						WHERE ISN.WorkDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(SC.InventoryReceiveDetailId,'')='' AND ISNULL(SC.PackingId,'')='' AND SC.POId IN (Select Id from TRN.ProductionOrder Where EntityId='" + EntityId + @"') AND SC.POId " + POId + @" AND SC.ProductCode " + ProductCode + @"
						GROUP BY PD.ProductionOrderId,PD.ProductCode,PD.OrderCostingMasterTemplateId,PD.Rate,PD.MaterialMaster,PD.MaterialMasterId,PD.Article,PD.ArticleId,PD.IsAsset,PD.UOM,PD.MasterOrderItemId,A.Qty,A.WorkDate";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetItemDetailData(string masterId)
        {
            try
            {
                string sql = @"SELECT FGD.Id,SC.POId ProductionOrderId,sc.ProductCode, PL.Id ProductLibraryId, PL.CostingMasterTemplateId, CT.UserName CostingMasterTemplate,MM.UserName MaterialMaster,MMA.StandardName Article
							 ,Qty=SUM(CASE WHEN SC.IsDespatch=0 THEN SC.NetWeight ELSE 0 END),ISNULL(B.Rate,0)Rate, Amount=SUM(CASE WHEN SC.IsDespatch=0 THEN SC.NetWeight ELSE 0 END) * ISNULL(B.Rate,0),Active=Convert(bit,1)
							FROM dbo.ItemScanChild SC 
							LEFT JOIN dbo.ProductLibrary PL ON PL.Code=SC.ProductCode
							LEFT JOIN dbo.ItemScan ISN ON ISN.Id=SC.MasterId
							LEFT JOIN dbo.CostingMasterTemplate AS CT ON CT.Id=PL.CostingMasterTemplateId
							LEFT JOIN 
							(
							SELECT DISTINCT COST.CostingMasterTemplateId,COST.Rate 
							FROM (select A.CostingMasterTemplateId,sum(A.rate) AS Rate from CostingMasterTemplate CMT 
								JOIN
								( 
								SELECT DM.CostingItemId,DM.CostingMasterTemplateId,DM.GrossAmount Rate from [dbo].PreCostingDirectMaterial DM
								UNION
								SELECT DP.CostingItemId,DP.CostingMasterTemplateId,DP.Amount Rate from [dbo].PreCostingDirectProcess DP
								UNION
								SELECT OP.CostingItemId,OP.CostingMasterTemplateId,OP.[Value] Rate from [dbo].PreCostingOperation OP
								UNION
								SELECT P.CostingItemId,P.CostingMasterTemplateId,P.[Value] Rate from [dbo].PreCostingProfit P
								UNION
								SELECT SE.CostingItemId,SE.CostingMasterTemplateId,SE.[Value] Rate from [dbo].PreCostingSalesExpense SE
								UNION
								SELECT VL.CostingItemId,VL.CostingMasterTemplateId,VL.[Value] Rate from [dbo].PreCostingValueLoss VL
								)  AS 	A ON A.CostingMasterTemplateId=CMT.Id
								LEFT JOIN [HKP].[CostingItem] CI ON CI.Id=A.CostingItemId 
								LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
							GROUP BY a.CostingMasterTemplateId) AS COST
							) B ON B.CostingMasterTemplateId=CT.Id
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=PL.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=PL.ArticleId
							LEFT JOIN FinishGoodsBookingDetail FGD ON SC.FinishGoodsBookingDetailId=FGD.Id
							LEFT JOIN FinishGoodsBooking FG ON FG.Id=FGD.FinishGoodsBookingId
	                        WHERE FG.Id='" + masterId + @"'
							GROUP BY FGD.Id,SC.POId,SC.ProductCode,PL.Id,PL.CostingMasterTemplateId,B.Rate,CT.UserName,MM.UserName,MMA.StandardName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetNonPostedProductionSummeryData(string entityId, string processId, string fromDate, string toDate)
        {
            try
            {
                string sql = @"SELECT '' Id,PS.ProductionOrderId,PD.ProductCode,MM.Id MaterialMasterId,MMA.Id ArticleId,MM.UserName MaterialMaster,MMA.StandardName Article
					,SUM(PS.Quantity) Qty,FORMAT(PD.Rate, 'N4') Rate,FORMAT((SUM(PS.Quantity) * PD.Rate), 'N2') Amount
					,U.Id UOM,PD.MasterOrderItemId,PD.OrderCostingMasterTemplateId
					,SONO = STUFF((
							SELECT DISTINCT ',' + XSO.Id
							FROM trn.SalesOrder XSO
							WHERE XSO.MasterOrderItemId = PD.MasterOrderItemId
							FOR XML path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				FROM [TRN].ProductionSummary PS
				
				LEFT JOIN (
					SELECT DISTINCT POD.ProductionOrderId,ISNULL(B.Rate, 0) Rate,MOI.Id MasterOrderItemId,B.OrderCostingMasterTemplateId,PL.Code ProductCode
					FROM TRN.SalesOrder SO
					LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId = SO.Id
					LEFT JOIN TRN.MasterOrderItem MOI ON moi.Id = so.MasterOrderItemId
					LEFT JOIN dbo.ProductLibrary PL ON MOI.ProductLibraryId=PL.Id
					LEFT JOIN dbo.OrderCostingMasterTemplate AS CT ON CT.Id = MOI.OrderCostingMasterTemplateId
					LEFT JOIN (
						SELECT DISTINCT COST.OrderCostingMasterTemplateId,COST.Rate
						FROM (
							SELECT A.OrderCostingMasterTemplateId,sum(A.rate) AS Rate
							FROM OrderCostingMasterTemplate CMT
							JOIN (
								SELECT DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate
								FROM [dbo].OrderProcurementCostingDirectMaterial DM				
								UNION				
								SELECT DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate
								FROM [dbo].OrderProcurementCostingDirectProcess DP				
								UNION				
								SELECT OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate
								FROM [dbo].OrderProcurementCostingOperation OP
								UNION
								SELECT P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate
								FROM [dbo].OrderProcurementCostingProfit P
								UNION
								SELECT SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate
								FROM [dbo].OrderProcurementCostingSalesExpense SE
								UNION
								SELECT VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate
								FROM [dbo].OrderProcurementCostingValueLoss VL
								) AS A ON A.OrderCostingMasterTemplateId = CMT.Id
							LEFT JOIN [HKP].[CostingItem] CI ON CI.Id = A.CostingItemId
							LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id = CI.CostingComponentId 
							Where CC.ConsiderForFGValuation=1
							GROUP BY a.OrderCostingMasterTemplateId
							) AS COST
						) B ON B.OrderCostingMasterTemplateId = CT.Id
					) PD ON PD.ProductionOrderId = PS.ProductionOrderId
				LEFT JOIN MST.MaterialMaster MM ON MM.Id = PS.MaterialMasterId
				LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = PS.ArticleId
				LEFT JOIN SCS.UnitOfMeasurement U ON MM.BaseUoMId = U.Id
				WHERE PS.ProductionDate between '" + fromDate + @"' AND '" + toDate + @"' AND ISNULL(PS.FinishGoodsBookingId,'')='' AND PS.EntityId='" + entityId + @"' AND PS.ProcessId='" + processId + @"' AND ISNULL(PS.MaterialMasterId,'')<>''
				GROUP BY PS.ProductionOrderId,MM.Id,MMA.Id,MM.UserName,MMA.StandardName,U.Id,PD.Rate,PD.MasterOrderItemId,PD.OrderCostingMasterTemplateId,PD.ProductCode";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetDatewiseNonPostedProductionSummeryData(string entityId, string processId, string fromDate, string toDate)
        {
            try
            {

                string sql = @"SELECT  '' Id,PS.Id ProductionSummaryId,PS.ProductionOrderId,MM.Id MaterialMasterId,MMA.Id ArticleId,MM.UserName MaterialMaster,MMA.StandardName Article
								,Qty= CASE WHEN PSD.Id IS NULL THEN PS.Quantity ELSE PSD.Qty END
								, 0 Rate,0 Amount,U.Id UOM,FORMAT(PS.ProductionDate ,'dd-MMM-yyyy') WorkDate,MM.IsAsset,PD.OrderCostingMasterTemplateId
								,PSD.Characteristics1Id FirstCharacteristicsId,PSD.Characteristics1ValueId FirstCharacteristicsValueId,PSD.Characteristics2Id SecondCharacteristicsId,PSD.Characteristics2ValueId SecondCharacteristicsValueId
								FROM [TRN].ProductionSummary PS
								LEFT JOIN TRN.ProductionSummaryDetail PSD ON PS.Id = PSD.ProductionSummaryId
								LEFT JOIN [HKP].[CharacteristicsValue] AS FCHV ON FCHV.Id = PSD.Characteristics1ValueId
								LEFT JOIN [HKP].[CharacteristicsValue] AS SCHV ON SCHV.Id = PSD.Characteristics2ValueId
								LEFT JOIN (
								SELECT DISTINCT POD.ProductionOrderId,ISNULL(B.Rate, 0) Rate,MOI.Id MasterOrderItemId,B.OrderCostingMasterTemplateId
								FROM TRN.SalesOrder SO
								LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId = SO.Id
								LEFT JOIN TRN.MasterOrderItem MOI ON moi.Id = so.MasterOrderItemId
								LEFT JOIN dbo.OrderCostingMasterTemplate AS CT ON CT.Id = MOI.OrderCostingMasterTemplateId
								LEFT JOIN (
								SELECT DISTINCT COST.OrderCostingMasterTemplateId,COST.Rate
								FROM (
								SELECT A.OrderCostingMasterTemplateId,sum(A.rate) AS Rate
								FROM OrderCostingMasterTemplate CMT
								JOIN (
								SELECT DM.CostingItemId,DM.OrderCostingMasterTemplateId,DM.GrossAmount Rate
								FROM [dbo].OrderProcurementCostingDirectMaterial DM				
								UNION				
								SELECT DP.CostingItemId,DP.OrderCostingMasterTemplateId,DP.Amount Rate
								FROM [dbo].OrderProcurementCostingDirectProcess DP				
								UNION				
								SELECT OP.CostingItemId,OP.OrderCostingMasterTemplateId,OP.[Value] Rate
								FROM [dbo].OrderProcurementCostingOperation OP
								UNION
								SELECT P.CostingItemId,P.OrderCostingMasterTemplateId,P.[Value] Rate
								FROM [dbo].OrderProcurementCostingProfit P
								UNION
								SELECT SE.CostingItemId,SE.OrderCostingMasterTemplateId,SE.[Value] Rate
								FROM [dbo].OrderProcurementCostingSalesExpense SE
								UNION
								SELECT VL.CostingItemId,VL.OrderCostingMasterTemplateId,VL.[Value] Rate
								FROM [dbo].OrderProcurementCostingValueLoss VL
								) AS A ON A.OrderCostingMasterTemplateId = CMT.Id
								LEFT JOIN [HKP].[CostingItem] CI ON CI.Id = A.CostingItemId
								LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id = CI.CostingComponentId 
								Where CC.ConsiderForFGValuation=1
								GROUP BY a.OrderCostingMasterTemplateId
								) AS COST
								) B ON B.OrderCostingMasterTemplateId = CT.Id
								) PD ON PD.ProductionOrderId = PS.ProductionOrderId
								LEFT JOIN MST.MaterialMaster MM ON MM.Id=PS.MaterialMasterId
								LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=PS.ArticleId
								LEFT JOIN SCS.UnitOfMeasurement U ON MM.BaseUoMId = U.Id
                            where PS.ProductionDate between '" + fromDate + "' AND '" + toDate + "' AND ISNULL(PS.FinishGoodsBookingId,'')='' AND PS.EntityId='" + entityId + "' AND PS.ProcessId='" + processId + "' AND ISNULL(PS.MaterialMasterId,'')<>''";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object GetFromDate()
        {
            string sql = "";
            try
            {
                sql = @"Select FORMAT(MIN(A.WorkDate),'dd-MMM-yyyy') FromDate,FORMAT(MAX(A.WorkDate),'dd-MMM-yyyy') ToDate
						from dbo.ItemScan A
						JOIN dbo.ItemScanChild B oN A.Id=B.MasterId
						Where B.IsDespatch=0 AND ISNULL(B.InventoryReceiveDetailId,'')=''";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object GetProductionBookFromToDate(string PlantId)
        {
            string sql = "";
            try
            {
                sql = @"SELECT FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') FromDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') ToDate FROM [TRN].ProductionSummary where ISNULL(FinishGoodsBookingId,'')='' AND PlantId='" + PlantId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetListForFinishGoodsBookingPost(string plantId)
        {
            var sql = @"SELECT FG.Id,IR.Id InventoryReceiveId,IR.GRNDate BookingDate,IR.GRNDate PostingDate,ird.Qty,ird.Amount
                        ,FG.ProcessId,P.UserName ProcessName,FG.[Description],FG.ProductionEntityId EntityId
                        ,E.UserName Entity,IR.FinishGoodsBookingId,FG.FromDate,FG.ToDate,FG.SourceType
						,C.Code CurrencyCode,IR.CurrencyId,IR.ToCurrencyRate CompanyCurrencyRate,MS.UserName MaterialStorageName
					FROM  TRN.InventoryReceive IR
					LEFT JOIN dbo.[FinishGoodsBooking] AS FG  ON IR.FinishGoodsBookingId=FG.Id
                     LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS Qty, SUM(ROUND(A.TotalMaterialTranAmount,2)) AS Amount
					 FROM TRN.InventoryReceiveDetail AS A  GROUP BY A.InventoryReceiveId) AS  IRD ON IRD.InventoryReceiveId=IR.Id
					 LEFT JOIN ORG.Entity E ON E.Id=FG.ProductionEntityId
					 LEFT JOIN HKP.Process P ON P.Id=FG.ProcessId
					 LEFT JOIN SCS.Currency C ON C.Id=IR.CurrencyId
					 LEFT JOIN HKP.MaterialStorage MS ON MS.Id=IR.MaterialStorageId
					WHERE IR.VoucherId IS NULL  AND E.PlantId='" + plantId + @"'";
            return _sqlRepository.GetDataCollection(sql);
        }
        public IEnumerable<object> GetFGInventoryGLBudgetActivity(string receiveId, string companyId, string plantId)
        {
            var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

                            SELECT distinct IR.Id,IRD.Id AS InventoryReceiveDetailId, 'FGInventory' AS OtherName, 'Dr' AS TrnType ,MM.MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGGL.InventoryBudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id

                        LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id

						WHERE IRD.InventoryReceiveId=@receiveId";
            return _sqlRepository.GetDataCollection(sql);
        }
        private Dictionary<string, object> GetCompanyPartyGroup(string partyId, string plantId)
        {
            var cmdText = @"select PartyAccountGroupId FROM HKP.CompanyParty where PartyId = '" + partyId + "' AND PlantId='" + plantId + @"' and PartyType='Vendor'";
            return _sqlRepository.GetData(cmdText);
        }
        public IEnumerable<object> GetFGJournal(string companyId, string dateWiseConsumptionId)
        {
            var sql = @"DECLARE @inventoryReceiveId varchar(10)='" + dateWiseConsumptionId + @"',  @companyId varchar(10)='" + companyId + @"'
					
						SELECT  'FGInventory' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId
							,GLGeneralInfoId=MGGL.InventoryGLId
							,GLGeneralInfoCode = GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId = MGGL.InventoryBudgetMasterId
							,BudgetCode =B.Code 
							,BudgetName = B.UserName
							,ActivityId =MGGL.InventoryActivityId 
							,ActivityCode =A.Code 
							,ActivityName =A.UserName
							, SUM(IRD.TotalMaterialTranAmount) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialTranAmount) AS Amount
                            ,IRD.Id AS  FinishGoodsBookingDetailId
						FROM TRN.InventoryReceive IR 
						LEFT JOIN TRN.InventoryReceiveDetail AS IRD ON IR.Id=IRD.InventoryReceiveId
						LEFT JOIN dbo.[FinishGoodsBooking] AS FG ON IR.FinishGoodsBookingId=FG.Id
						LEFT JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						
						WHERE IR.Id=@inventoryReceiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,IRD.Id
                   
				   UNION
				   SELECT  'WIPSFG' AS OtherName, 'Cr' AS TrnType,NULL MaterialGroupMasterId
							,GLGeneralInfoId=GAD.GLGeneralInfoId
							,GLGeneralInfoCode = GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId = GAD.BudgetMasterId
							,BudgetCode =B.Code 
							,BudgetName = B.UserName
							,ActivityId =GAD.ActivityId 
							,ActivityCode =A.Code 
							,ActivityName =A.UserName
							, NULL Dr, SUM(IRD.TotalMaterialTranAmount) AS Cr
							, SUM(IRD.TotalMaterialTranAmount) AS Amount
                            ,NULL FinishGoodsBookingDetailId
						FROM TRN.InventoryReceive IR
						LEFT JOIN TRN.InventoryReceiveDetail AS IRD ON IR.Id=IRD.InventoryReceiveId
						LEFT JOIN dbo.[FinishGoodsBooking] AS FG ON IR.FinishGoodsBookingId=IR.Id
						LEFT JOIN ORG.Company CO ON CO.Id=IR.CompanyId
						LEFT JOIN HKP.GeneralAccountDeterminate GAD ON GAD.COAId=CO.COAId AND GAD.Id='IssueOfRawMaterialToAnOrder'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
						WHERE IR.Id=@inventoryReceiveId
						GROUP BY  GAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, GAD.BudgetMasterId, B.Code, B.UserName, GAD.ActivityId, A.Code, A.UserName
					     
					ORDER BY TrnType DESC 
";
            return _sqlRepository.GetDataCollection(sql);

        }

        public GridModel GetFGMaterialDetail(GridParameter parameters, string dateWiseConsumptionId)
        {

            parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + dateWiseConsumptionId + @"'
                        SELECT  FGD.Id AS FinishGoodsBookingDetailId
                            , MGM.UserName AS MaterialGroupMasterName
                            , PL.MaterialMasterId, MM.UserName
                            , PL.ArticleId, ART.StandardName
                            , FGD.MaterialTranRate AS TransactionRate
                            , CU.Code AS CurrencyName, 1 ToCurrencyRate
                            , FGD.TotalMaterialTranAmount AS TrnAmount
                             ,FGD.TransactionQty AS TransactionQty
                            
					  from TRN.InventoryReceive IR
                        LEFT JOIN TRN.InventoryReceiveDetail AS FGD ON IR.Id=FGD.InventoryReceiveId
						LEFT JOIN dbo.[FinishGoodsBooking] AS FG ON Ir.FinishGoodsBookingId=FG.Id
						LEFT JOIN trn.InventoryMaterial AS PL ON FGD.InventoryMaterialId=PL.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON PL.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON PL.ArticleId=ART.Id
						LEFT JOIN ORG.Entity E ON E.Id=FG.ProductionEntityId
						LEFT JOIN ORG.Company CO ON CO.Id=E.CompanyId
						LEFT JOIN SCS.Currency CU ON CU.Id=CO.BaseCurrencyId
                        WHERE IR.Id=@inventoryReceiveId";
            return _sqlRepository.GetDifferentGridData(parameters);
        }



        #region FG Inventory Register Report

        public IEnumerable<object> GetPurchaseRegister(string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";

                sql = @"Select * from (SELECT   --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
						   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
							,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
							,IR.Id As GRNId
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
									else '' end
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   --,p.Id
                            ,p.UserName AS PartyName
						   ,EI.EmployeeName FirstName						   
						   ,IRD.Id As GrnDetailId
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   --,IR.InvoiceNo
						   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,MT.UserName MaterialType
						  ,MGM.UserName AS MaterialGroupMasterName
						  ,IM.MaterialMasterId
						  ,MM.UserName MaterialMasterName
					   -- , IM.ArticleId
						, ART.StandardName ArticleName
                        ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
                        ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
						--, IM.FirstCharacteristicsId
						--, FC.UserName AS FirstCharacteristics
						--, IM.FirstCharacteristicsValueId
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						--, IM.SecondCharacteristicsId
						--, SC.UserName AS SecondCharacteristics
						--, IM.SecondCharacteristicsValueId
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						--, IM.ThirdCharacteristicsId
						--, TC.UserName AS ThirdCharacteristics
						--, IM.ThirdCharacteristicsValueId
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,TUoM.UserName AS UOM
						,IRD.TransactionQty
						,IRD.ShortageQty
						,IRD.ShortageRatePercent
						,IRD.ShortageValue
						,IRD.RejectionQty
						,IRD.RejectRatePercent
						,IRD.RejectValue
						,IRD.RejectClamPercent
						,IRD.ApprovedQty
						,IR.IsNonCreditable
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
						,ROUND(Isnull(IRD.MaterialTranAmount,0),2) MaterialTranAmount
						,ROUND(Isnull(IRD.TrnCurrencyBaseRate,0),2) TrnCurrencyBaseRate,ROUND(Isnull(IRD.BooksCurrencyBaseRate,0),2) BooksCurrencyBaseRate
						,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)

						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

                        --, IRD.ChargesTranAmount AS ChargesAmount	   
						--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
						--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
						--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
                         --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
                         --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						,IRD.ChargesTranAmount ServiceCharge
						,IRD.ChargesTaxTranAmount ServiceTax
						,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
						--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
						--,Case When IR.IsNonCreditable = 1 
							--then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--when IR.IsNonCreditable = 0
							--then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--end TotalMaterialTranAmount
                       ,ROUND(Isnull(IRD.TotalMaterialBooksCurrencyAmount,0),2) TotalMaterialBaseAmount ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
							

								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
						,IGL.UserName AS GL
						,IGL.AccountCode GLCode
						,IA.Id ActivityId
						,IA.UserName Activity
						,IA.Code ActivityCode
						,IBM.RefNo BudgetrefNo
						,B.UserName AS Budget
                        ,IGL1.UserName AS CGL
						,IGL1.AccountCode CGLCode
						,IA1.Id CActivityId
						,IA1.UserName AS CActivity
						,IA1.Code CActivityCode
						,IBM1.RefNo CBudgetrefNo
						,B1.UserName AS CBUdget
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
                        ,IRD.POId
						,IRD.PODetailsId AS PORowId
                        ,MS.UserName as StorageLocation--,V.VoucherNo

						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						--,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
                        --,isnull(p.TINNO,'') GSTINNo
						,isnull(PP.GSTIN,'') GSTINNo
						,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,IRD.LotNo , IRD.QualityStatus , IRD.GrossAmount ,IRD.DiscountAmount--,Isnull(C.ContractNo,'') ContractNo
						,ISNULL(PID.RefferenceNo,'') RefferenceNo
						--,isnull(PO.POId,'') POId
						,isnull(PO.PurchaseLCId,'') PurchaseLCId
						,isnull(PO.ContractId,'') ContractId						
						,ISNull(po.ContractNo,'') ContractNo
						,isnull(PO.LCANo,'') LCANo
						,isnull(PO.LCDate,'') LCDate
						,ISNULL(IRD.IssueQty,0) IssueQty
						,ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						,ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty
						,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty
						,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty
						,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty
						,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty	 				
						,ISNULL(IRD.InventoryTransferQty,0) InventoryTransferQty,IRD.BaseQty,BUoM.UserName BaseUoM
						,ISNULL( IR.FinishGoodsBookingId,'')FinishGoodsBookingId
						,IR.ProductionOrderId
						,CU.Code Currency

					from TRN.InventoryMaterial AS IM
					JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					--LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
					left join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveDetailId=IRD.Id
				    --Left JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId
					left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id	
					left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					--left join trn.Voucher V on V.Id=I.VoucherId
					left join trn.Voucher V on V.Id=IR.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
                    LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
					LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
					Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
					LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
					Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
	               LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
								FROM [TRN].[InventoryReceiveTax] A
								LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
								left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
								WHERE B.Code='CGST' and A.InventoryServiceId IS NULL
								--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 								
							) TAxInfo	ON TAxInfo.InventoryReceiveDetailId=IRD.Id 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.InventoryServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo1	ON TAxInfo1.InventoryReceiveDetailId=IRD.Id 
							  		 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.InventoryServiceId IS NULL 
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo2	ON TAxInfo2.InventoryReceiveDetailId=IRD.Id 

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TDS' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
									) TAxInfo3	ON TAxInfo3.InventoryReceiveDetailId=IRD.Id 


							
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='VAT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo4 ON TAxInfo4.InventoryReceiveDetailId=IRD.Id

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='AIT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
							
						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IRD.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo6 ON TAxInfo6.InventoryReceiveDetailId=IRD.Id
	               
						LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
						LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
							--left join  trn.POGGRNMap POGGRNMap ON POGGRNMap.GRNId=IR.Id
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
						LEFT JOIN(
							   SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
							    ,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCRef from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							 where  IR.PlantId='" + identity.PlantId + "'  AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
							AND IR.GRNType ='FG' 

							UNION ALL

						SELECT 	--ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
						   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
							,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
							,IR.Id As GRNId
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
									else '' end --HSNC.Code HSNCode
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   --,p.Id
                            ,p.UserName AS PartyName
						   ,EI.EmployeeName FirstName						   
						   ,Null As GrnDetailId
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   --,IR.InvoiceNo
						   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,NULL MaterialType
						  ,NULL MaterialGroupMasterName
						  ,NULL MaterialMasterId
						    ,SM.UserName MaterialMasterName
					   -- , IM.ArticleId
						, SM.UserName ArticleName
                        ,'No' IsAsset
                        ,'No' GRNAsset
						--, IM.FirstCharacteristicsId
						--, FC.UserName AS FirstCharacteristics
						--, IM.FirstCharacteristicsValueId
						, NULL FirstCharacteristicsValue
						--, IM.SecondCharacteristicsId
						--, SC.UserName AS SecondCharacteristics
						--, IM.SecondCharacteristicsValueId
						, NULL SecondCharacteristicsValue
						--, IM.ThirdCharacteristicsId
						--, TC.UserName AS ThirdCharacteristics
						--, IM.ThirdCharacteristicsValueId
						, NULL ThirdCharacteristicsValue 
						,NULL AS UOM
						,0 TransactionQty
						,0 ShortageQty
						,0 ShortageRatePercent
						,0 ShortageValue
						,0 RejectionQty
						,0 RejectRatePercent
						,0 RejectValue
						,0 RejectClamPercent
						,0 ApprovedQty
						,IsNULL(IR.IsNonCreditable,0) IsNonCreditable
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,0 MaterialTranRate
						,ISs.Amount MaterialTranAmount
						,0 TrnCurrencyBaseRate
						,0 BooksCurrencyBaseRate
						, 0 TaxAmount
						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo5.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

                        --, IRD.ChargesTranAmount AS ChargesAmount	   
						--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
						--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
						--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
                         --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
                         --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						,0 ServiceCharge
						,0 ServiceTax
						--,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
						--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
						--,Case When IR.IsNonCreditable = 1 
						--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--when IR.IsNonCreditable = 0
						--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--end TotalMaterialTranAmount
						,0 TotalMaterialTranAmount
                       ,0 TotalMaterialBaseAmount ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
							

								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
						,Null AS GL
						,Null GLCode
						,Null ActivityId
						,Null Activity
						,Null ActivityCode
						,Null BudgetrefNo
						,Null AS Budget
                        ,Null AS CGL
						,Null CGLCode
						,Null CActivityId
						,Null AS CActivity
						,Null CActivityCode
						,Null CBudgetrefNo
						,NULL AS CBUdget
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
                        ,IR.POId
						,NULL AS PORowId
                        ,MS.UserName as StorageLocation--,V.VoucherNo

						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						--,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
                       -- ,isnull(p.TINNO,'') GSTINNo
					,isnull(PP.GSTIN,'') GSTINNo
					,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
					,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
					,Null LotNo , Null QualityStatus , Null GrossAmount ,Null DiscountAmount--,Isnull(C.ContractNo,'') ContractNo
					,'' RefferenceNo
					,'' PurchaseLCId
					,'' ContractId						
					,'' ContractNo
					,'' LCANo
					,'' LCDate
					,0 IssueQty
					,0 BaseIssueQty
					,0 PurchaseReturnQty
					,0 IssueReturnQty
					,0 ReductionByAdjustmentQty
					,0 InventorySalesQty
					,0 InventoryScrapQty						
					,0 InventoryTransferQty,0 BaseQty,'' BaseUoM
					,ISNULL( IR.FinishGoodsBookingId,'')FinishGoodsBookingId
					,IR.ProductionOrderId
					,null Currency

			from trn.InventoryService AS ISs
			LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
			left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			--left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
			LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
			LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
			LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
			LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
			LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
			LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
			left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
			--left join trn.Voucher V on V.Id=I.VoucherId
			left join trn.Voucher V on V.Id=IR.VoucherId
			left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
			left join trn.Voucher V1 on V1.Id=ep.VoucherId
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
						,A.TaxAmount TaxAmount,HS.Code HSCode 
						FROM  [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='CGST'  
						--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
						) TAxInfo	ON TAxInfo.InventoryServiceId=ISs.Id AND TAxInfo.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,HS.Code HSCode 
			FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='IGST' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

						) TAxInfo1	ON TAxInfo1.InventoryServiceId=ISs.Id AND TAxInfo1.InventoryServiceId IS NOT NULL 
							  		 
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,HS.Code HSCode 
			FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='SGST' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								

						) TAxInfo2	ON TAxInfo2.InventoryServiceId=ISs.Id AND TAxInfo2.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						WHERE B.Code='TDS' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo3	ON TAxInfo3.InventoryServiceId=ISs.Id AND TAxInfo3.InventoryServiceId IS NOT NULL


							
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='VAT' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
			) TAxInfo4 ON TAxInfo4.InventoryServiceId=ISs.Id AND TAxInfo4.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='AIT' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
							
			) TAxInfo5 ON TAxInfo5.InventoryServiceId=ISs.Id AND TAxInfo5.InventoryServiceId IS NOT NULL
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
			) TAxInfo6 ON TAxInfo6.InventoryServiceId=ISs.Id AND TAxInfo6.InventoryServiceId IS NOT NULL
	               
			LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
			LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
			--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
			where  IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
			--AND IRT.InventoryServiceId is not null
			AND IR.GRNType = 'FG' 
			--AND IR.GRNType<>'GRNBYPO'
			)x
			Order By X.GRNEntryDate ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IWorkbook CreatePurchaseRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {

                var excelEngine = new ExcelEngine();
                var report = new Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var Head = "FG Inventory Register Report";// + " " + fromDate + " " + "To" + " " + toDate ;
                CreatePurchaseRegisterReportSheets(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        private void CreatePurchaseRegisterReportSheets(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {


            var cmdText = "";
            //cmdText = @"Select * from (SELECT   --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
            //			   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
            //				,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
            //				,IR.Id As GRNId
            //				,HSNC.Code HSNCode
            //			   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
            //			   --,p.Id
            //                         ,p.UserName AS PartyName
            //			   ,EI.EmployeeName FirstName						   
            //			   ,IRD.Id As GrnDetailId
            //			   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
            //			   --,IR.InvoiceNo
            //			   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
            //			   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
            //			   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
            //			  ,MT.UserName MaterialType
            //			  ,MGM.UserName AS MaterialGroupMasterName
            //			  ,IM.MaterialMasterId
            //			  ,MM.UserName MaterialMasterName
            //		   -- , IM.ArticleId
            //			, ART.StandardName ArticleName
            //                     ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
            //                     ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
            //			--, IM.FirstCharacteristicsId
            //			--, FC.UserName AS FirstCharacteristics
            //			--, IM.FirstCharacteristicsValueId
            //			, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
            //			--, IM.SecondCharacteristicsId
            //			--, SC.UserName AS SecondCharacteristics
            //			--, IM.SecondCharacteristicsValueId
            //			, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
            //			--, IM.ThirdCharacteristicsId
            //			--, TC.UserName AS ThirdCharacteristics
            //			--, IM.ThirdCharacteristicsValueId
            //			, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
            //			,TUoM.UserName AS UOM
            //			,IRD.TransactionQty
            //			,IRD.ShortageQty
            //			,IRD.ShortageRatePercent
            //			,IRD.ShortageValue
            //			,IRD.RejectionQty
            //			,IRD.RejectRatePercent
            //			,IRD.RejectValue
            //			,IRD.RejectClamPercent
            //			,IRD.ApprovedQty
            //			,IR.IsNonCreditable
            //			,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
            //			,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
            //			,ROUND(Isnull(IRD.MaterialTranAmount,0),2) MaterialTranAmount
            //			,ROUND(Isnull(IRD.TrnCurrencyBaseRate,0),2) TrnCurrencyBaseRate,ROUND(Isnull(IRD.BooksCurrencyBaseRate,0),2) BooksCurrencyBaseRate
            //			,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)

            //			,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
            //			,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
            //			,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
            //			,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
            //			,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

            //                     --, IRD.ChargesTranAmount AS ChargesAmount	   
            //			--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
            //			--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
            //			--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
            //                      --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
            //                      --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
            //			,IRD.ChargesTranAmount ServiceCharge
            //			,IRD.ChargesTaxTranAmount ServiceTax
            //			--,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
            //			--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
            //			,Case When IR.IsNonCreditable = 1 
            //				then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
            //			when IR.IsNonCreditable = 0
            //				then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
            //			end TotalMaterialTranAmount
            //                    ,ROUND(Isnull(IRD.TotalMaterialBooksCurrencyAmount,0),2) TotalMaterialBaseAmount ,IR.AddedBy
            //                    ,CASE 
            //		        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
            //					WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
            //					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'


            //					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
            //					WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
            //                             WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
            //					WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
            //					END GRNCheckStatus
            //			,IGL.UserName AS GL
            //			,IGL.AccountCode GLCode
            //			,IA.Id ActivityId
            //			,IA.UserName Activity
            //			,IA.Code ActivityCode
            //			,IBM.RefNo BudgetrefNo
            //			,B.UserName AS Budget
            //                     ,IGL1.UserName AS CGL
            //			,IGL1.AccountCode CGLCode
            //			,IA1.Id CActivityId
            //			,IA1.UserName AS CActivity
            //			,IA1.Code CActivityCode
            //			,IBM1.RefNo CBudgetrefNo
            //			,B1.UserName AS CBUdget
            //                     ,EI1.EmployeeName CheckedBY
            //			,EI2.EmployeeName AuthorizedBy
            //                     ,IR.POId
            //			,IRD.PODetailsId AS PORowId
            //                     ,MS.UserName as StorageLocation--,V.VoucherNo

            //			,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
            //			,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
            //			,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
            //			,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
            //                     --,isnull(p.TINNO,'') GSTINNo
            //			,isnull(PP.GSTIN,'') GSTINNo
            //			,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
            //			,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
            //		from TRN.InventoryMaterial AS IM
            //		JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
            //		--LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
            //		LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
            //		LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
            //		LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
            //		LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
            //		LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
            //		LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
            //		LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
            //		LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
            //		left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
            //		left join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveDetailId=IRD.Id
            //	     --JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId
            //		Left JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId
            //		left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
            //		left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
            //		left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id				
            //		left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
            //		LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
            //		LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
            //		LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
            //		LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
            //		LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
            //                 LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
            //		LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
            //		LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
            //                 left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
            //		left join trn.Voucher V on V.Id=I.VoucherId
            //                 left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
            //		left join trn.Voucher V1 on V1.Id=ep.VoucherId
            //                 LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
            //		LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
            //		LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
            //		Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
            //                 LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
            //		LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
            //		LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
            //		Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
            //             LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
            //	--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //	A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
            //WHERE B.Code='CGST' and A.InventoryServiceId IS NULL
            //--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 


            //) TAxInfo	ON TAxInfo.InventoryReceiveDetailId=IRD.Id 
            //			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
            //			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
            //						WHERE B.Code='IGST' and A.InventoryServiceId IS NULL 
            //		--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 


            //						) TAxInfo1	ON TAxInfo1.InventoryReceiveDetailId=IRD.Id 

            //			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
            //		--,sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //		A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
            //						WHERE B.Code='SGST' and A.InventoryServiceId IS NULL
            //		--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 


            //						) TAxInfo2	ON TAxInfo2.InventoryReceiveDetailId=IRD.Id 

            //			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
            //		--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
            //						WHERE B.Code='TDS' and A.InventoryServiceId IS NULL 
            //			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

            //						) TAxInfo3	ON TAxInfo3.InventoryReceiveDetailId=IRD.Id 



            //			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
            //			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
            //						WHERE B.Code='VAT' and A.InventoryServiceId IS NULL 
            //			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

            //			) TAxInfo4 ON TAxInfo4.InventoryReceiveDetailId=IRD.Id

            //			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
            //					--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //					A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
            //						WHERE B.Code='AIT' and A.InventoryServiceId IS NULL 
            //			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

            //			) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IRD.Id
            //			LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
            //					--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //					A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
            //						WHERE B.Code='TCS' and A.InventoryServiceId IS NULL 
            //					--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

            //			) TAxInfo6 ON TAxInfo6.InventoryReceiveDetailId=IRD.Id

            //			LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
            //			LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
            //			 where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --ORDER BY IR.GRNDate ASC

            //				UNION ALL
            //			SELECT 	--ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
            //			   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
            //				,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
            //				,IR.Id As GRNId
            //				,HSNC.Code HSNCode
            //			   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
            //			   --,p.Id
            //                         ,p.UserName AS PartyName
            //			   ,EI.EmployeeName FirstName						   
            //			   ,IRD.Id As GrnDetailId
            //			   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
            //			   --,IR.InvoiceNo
            //			   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
            //			   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
            //			   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
            //			  ,'' MaterialType
            //			  ,'' MaterialGroupMasterName
            //			  ,'' MaterialMasterId
            //			    ,SM.UserName MaterialMasterName
            //		   -- , IM.ArticleId
            //			, '' ArticleName
            //                     ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
            //                     ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
            //			--, IM.FirstCharacteristicsId
            //			--, FC.UserName AS FirstCharacteristics
            //			--, IM.FirstCharacteristicsValueId
            //			, '' FirstCharacteristicsValue
            //			--, IM.SecondCharacteristicsId
            //			--, SC.UserName AS SecondCharacteristics
            //			--, IM.SecondCharacteristicsValueId
            //			, '' SecondCharacteristicsValue
            //			--, IM.ThirdCharacteristicsId
            //			--, TC.UserName AS ThirdCharacteristics
            //			--, IM.ThirdCharacteristicsValueId
            //			, '' ThirdCharacteristicsValue 
            //			,TUoM.UserName AS UOM
            //			,0 TransactionQty
            //			,0 ShortageQty
            //			,0 ShortageRatePercent
            //			,0 ShortageValue
            //			,0 RejectionQty
            //			,0 RejectRatePercent
            //			,0 RejectValue
            //			,0 RejectClamPercent
            //			,0 ApprovedQty
            //			,IsNULL(IR.IsNonCreditable,'') IsNonCreditable
            //			,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
            //			,0 MaterialTranRate
            //			,IRD.ChargesTranAmount MaterialTranAmount
            //			,0 TrnCurrencyBaseRate
            //			,0 BooksCurrencyBaseRate
            //			,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IRD.InventoryReceiveId AND InventoryReceiveDetailId is null)

            //			,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
            //			,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
            //			,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
            //			,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
            //			,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
            //			--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

            //                     --, IRD.ChargesTranAmount AS ChargesAmount	   
            //			--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
            //			--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
            //			--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
            //                      --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
            //                      --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
            //			,0 ServiceCharge
            //			,0 ServiceTax
            //			--,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
            //			--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
            //			--,Case When IR.IsNonCreditable = 1 
            //			--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
            //			--when IR.IsNonCreditable = 0
            //			--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
            //			--end TotalMaterialTranAmount
            //			,0 TotalMaterialTranAmount
            //                    ,0 TotalMaterialBaseAmount ,IR.AddedBy
            //                    ,CASE 
            //		        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
            //					WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
            //					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'


            //					WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
            //					WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
            //                             WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
            //					WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
            //					END GRNCheckStatus
            //			,IGL.UserName AS GL
            //			,IGL.AccountCode GLCode
            //			,IA.Id ActivityId
            //			,IA.UserName Activity
            //			,IA.Code ActivityCode
            //			,IBM.RefNo BudgetrefNo
            //			,B.UserName AS Budget
            //                     ,IGL1.UserName AS CGL
            //			,IGL1.AccountCode CGLCode
            //			,IA1.Id CActivityId
            //			,IA1.UserName AS CActivity
            //			,IA1.Code CActivityCode
            //			,IBM1.RefNo CBudgetrefNo
            //			,B1.UserName AS CBUdget
            //                     ,EI1.EmployeeName CheckedBY
            //			,EI2.EmployeeName AuthorizedBy
            //                     ,IR.POId
            //			,IRD.PODetailsId AS PORowId
            //                     ,MS.UserName as StorageLocation--,V.VoucherNo

            //			,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
            //			,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
            //			,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
            //			,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
            //                    -- ,isnull(p.TINNO,'') GSTINNo
            //		,isnull(PP.GSTIN,'') GSTINNo
            //		,IR.PartyId ,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
            //			,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
            //from TRN.InventoryMaterial AS IM
            //JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
            //LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
            //LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
            //LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
            //LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
            //LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
            //LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
            //LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
            //LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
            //left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
            //left join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId=IRD.InventoryReceiveId 
            //  --JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId 
            //Left JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId 
            //left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
            //left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
            //left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id				
            //left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
            //LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				

            //LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
            //LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
            //LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
            //LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
            //LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
            //LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
            //LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy

            //left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
            //left join trn.Voucher V on V.Id=I.VoucherId

            //left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
            //left join trn.Voucher V1 on V1.Id=ep.VoucherId


            //LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
            //LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
            //LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
            //Left JOIN hkp.Budget B On B.Id=IBM.BudgetId


            //LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
            //LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
            //LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
            //Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
            //left JOIN trn.InventoryService ISs ON ISS.InventoryReceiveId=IR.Id
            //left JOin [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
            //LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
            //			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
            //			WHERE B.Code='CGST' and A.InventoryServiceId IS NOT NULL
            //			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 


            //			) TAxInfo	ON TAxInfo.InventoryReceiveId=IRD.InventoryReceiveId 
            //LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
            //			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
            //			WHERE B.Code='IGST' and A.InventoryServiceId IS NOT NULL 
            //		--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 


            //			) TAxInfo1	ON TAxInfo1.InventoryReceiveId=IRD.InventoryReceiveId 

            //LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
            //			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
            //			WHERE B.Code='SGST' and A.InventoryServiceId IS NOT NULL 
            //			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 


            //			) TAxInfo2	ON TAxInfo2.InventoryReceiveId=IRD.InventoryReceiveId 

            //LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,
            //			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
            //			WHERE B.Code='TDS' and A.InventoryServiceId IS NOT NULL 
            //			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

            //			) TAxInfo3	ON TAxInfo3.InventoryReceiveId=IRD.Id 



            //LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
            //			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
            //			WHERE B.Code='VAT' and A.InventoryServiceId IS NOT NULL 
            //			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

            //) TAxInfo4 ON TAxInfo4.InventoryReceiveId=IRD.Id

            //LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
            //	--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
            //			WHERE B.Code='AIT' and A.InventoryServiceId IS NOT NULL 
            //			--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

            //) TAxInfo5 ON TAxInfo5.InventoryReceiveId=IRD.Id
            //LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,
            //			--sum(A.TaxAmount) TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
            //			LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
            //			WHERE B.Code='TCS' and A.InventoryServiceId IS NOT NULL 
            //                    --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

            //) TAxInfo6 ON TAxInfo6.InventoryReceiveId=IRD.Id

            //LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
            //LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
            //where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IRT.InventoryServiceId is not null
            //)x
            //Order By X.GRNEntryDate ASC";


            cmdText = @"Select * from (SELECT   --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
						   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
							,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
							,IR.Id As GRNId
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
									else '' end
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   --,p.Id
                            ,p.UserName AS PartyName
						   ,EI.EmployeeName FirstName						   
						   ,IRD.Id As GrnDetailId
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   --,IR.InvoiceNo
						   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,MT.UserName MaterialType
						  ,MGM.UserName AS MaterialGroupMasterName
						  ,IM.MaterialMasterId
						  ,MM.UserName MaterialMasterName
					   -- , IM.ArticleId
						, ART.StandardName ArticleName
                        ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
                        ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
						--, IM.FirstCharacteristicsId
						--, FC.UserName AS FirstCharacteristics
						--, IM.FirstCharacteristicsValueId
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						--, IM.SecondCharacteristicsId
						--, SC.UserName AS SecondCharacteristics
						--, IM.SecondCharacteristicsValueId
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						--, IM.ThirdCharacteristicsId
						--, TC.UserName AS ThirdCharacteristics
						--, IM.ThirdCharacteristicsValueId
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,TUoM.UserName AS UOM
						,IRD.TransactionQty
						,IRD.ShortageQty
						,IRD.ShortageRatePercent
						,IRD.ShortageValue
						,IRD.RejectionQty
						,IRD.RejectRatePercent
						,IRD.RejectValue
						,IRD.RejectClamPercent
						,IRD.ApprovedQty
						,IR.IsNonCreditable
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
						,ROUND(Isnull(IRD.MaterialTranAmount,0),2) MaterialTranAmount
						,ROUND(Isnull(IRD.TrnCurrencyBaseRate,0),2) TrnCurrencyBaseRate,ROUND(Isnull(IRD.BooksCurrencyBaseRate,0),2) BooksCurrencyBaseRate
						,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)

						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) MaterialTCS,TAxInfo6.Percentage MaterialTCSTaxPercentage
                        ,round(isnull(TAxInfo7.TaxAmount,0),2) GRNTCS,TAxInfo7.Percentage GRNTCSTaxPercentage
						,round(isnull(TAxInfo8.TaxAmount,0),2) MandiTax,TAxInfo8.Percentage MandiTaxPercentage
						,round(isnull(TAxInfo9.TaxAmount,0),2) NirasritTax,TAxInfo9.Percentage NirasritTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

                        --, IRD.ChargesTranAmount AS ChargesAmount	   
						--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
						--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
						--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
                         --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
                         --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						,IRD.ChargesTranAmount ServiceCharge
						,IRD.ChargesTaxTranAmount ServiceTax
						,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
						--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
						--,Case When IR.IsNonCreditable = 1 
							--then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--when IR.IsNonCreditable = 0
							--then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--end TotalMaterialTranAmount
                       ,ROUND(Isnull(IRD.TotalMaterialBooksCurrencyAmount,0),2) TotalMaterialBaseAmount ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
							

								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
						,IGL.UserName AS GL
						,IGL.AccountCode GLCode
						,IA.Id ActivityId
						,IA.UserName Activity
						,IA.Code ActivityCode
						,IBM.RefNo BudgetrefNo
						,B.UserName AS Budget
                        ,IGL1.UserName AS CGL
						,IGL1.AccountCode CGLCode
						,IA1.Id CActivityId
						,IA1.UserName AS CActivity
						,IA1.Code CActivityCode
						,IBM1.RefNo CBudgetrefNo
						,B1.UserName AS CBUdget
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
                        ,IRD.POId
						,IRD.PODetailsId AS PORowId
                        ,MS.UserName as StorageLocation--,V.VoucherNo

						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
                        --,isnull(p.TINNO,'') GSTINNo
						,isnull(PP.GSTIN,'') GSTINNo
						,IR.PartyId ,P.Code,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,IRD.LotNo , IRD.QualityStatus , IRD.GrossAmount ,IRD.DiscountAmount--,Isnull(C.ContractNo,'') ContractNo
						,ISNULL(PID.RefferenceNo,'') RefferenceNo
						--,isnull(PO.POId,'') POId
						,isnull(PO.PurchaseLCId,'') PurchaseLCId
						,isnull(PO.ContractId,'') ContractId						
						,ISNull(po.ContractNo,'') ContractNo
						,isnull(PO.LCANo,'') LCANo
						,isnull(PO.LCDate,'') LCDate
						,IRD.IssueQty
						,IRD.BaseIssueQty
						,IRD.PurchaseReturnQty
						,IRD.IssueReturnQty
						
						,IRD.ReductionByAdjustmentQty
						,IRD.InventorySalesQty
						,IRD.InventoryScrapQty						
						,IRD.InventoryTransferQty,IRD.BaseQty,BUoM.UserName BaseUoM
					from TRN.InventoryMaterial AS IM
					JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					--LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
					left join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveDetailId=IRD.Id
				    --Left JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId
					left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id	
					left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					--left join trn.Voucher V on V.Id=I.VoucherId
					left join trn.Voucher V on V.Id=IR.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
                    LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
					LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
					Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
					LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
					Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
	               LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
				   FROM [TRN].[InventoryReceiveTax] A
			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
			WHERE B.Code='CGST' and A.InventoryServiceId IS NULL
			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

								
			) TAxInfo	ON TAxInfo.InventoryReceiveDetailId=IRD.Id 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.InventoryServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo1	ON TAxInfo1.InventoryReceiveDetailId=IRD.Id 
							  		 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.InventoryServiceId IS NULL 
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo2	ON TAxInfo2.InventoryReceiveDetailId=IRD.Id 

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TDS' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
									) TAxInfo3	ON TAxInfo3.InventoryReceiveDetailId=IRD.Id 


							
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='VAT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo4 ON TAxInfo4.InventoryReceiveDetailId=IRD.Id

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='AIT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
							
						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IRD.Id
						--LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						--			LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						--			WHERE B.Code='TCS' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						--) TAxInfo6 ON TAxInfo6.InventoryReceiveDetailId=IRD.Id
	                    LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
						           FROM trn.InventoryReceiveAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' --and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo6 ON TAxInfo6.InventoryReceiveId=IR.Id

                        LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
						           FROM trn.InventoryReceiveAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' --and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo7 ON TAxInfo7.InventoryReceiveId=IR.Id


						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Mandi Tax' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo8 ON TAxInfo8.InventoryReceiveDetailId=IRD.Id


						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Nirasrit T' and A.InventoryServiceId IS NULL 
						) TAxInfo9 ON TAxInfo9.InventoryReceiveDetailId=IRD.Id
						LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
						LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
						--left join  trn.POGGRNMap POGGRNMap ON POGGRNMap.GRNId=IR.Id
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
						LEFT JOIN(
							   SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
							    ,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCRef from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						 --where  IR.PlantId='20181'  AND convert(Date,IR.GRNDate) BETWEEN  '01-OCT-2020' AND '31-OCT-2020' --ORDER BY IR.GRNDate ASC
						 where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
                        
						AND IR.GRNType ='FG' 

							
			)x
			Order By X.GRNEntryDate ASC";


            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            var colTransactionQtyTotal = 0.00;
            var colTransactionAmountTotal = 0.00;
            var colTotalMaterialTranAmountTotal = 0.00;
            var colTaxAmountTotal = 0.00;
            var colTotalMaterialBooksCurrencyAmountTotal = 0.00;
            var colTrnCurrencyBaseRateTotal = 0.00;
            var colBooksCurrencyBaseRateTotal = 0.00;
            var colShortageQtyTotal = 0.00;
            var colRejectionQtyTotal = 0.00;
            var colApprovedQtyTotal = 0.00;

            var colCGSTTotal = 0.00;
            var colSGSTTotal = 0.00;
            var colIGSTTotal = 0.00;
            var colTDSTotal = 0.00;
            var colTCSTotal = 0.00;

            var colCGSTTotal1 = 0.00;
            var colSGSTTotal1 = 0.00;
            var colIGSTTotal1 = 0.00;
            var colTDSTotal1 = 0.00;
            var colTCSTotal1 = 0.00;


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _rowd = 4;

            if (fromDate != "" && toDate != "")
            {


                sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;
                sheet1[_rowd, 4].CellStyle.Font.Size = 8;
                sheet1[_rowd, 4].CellStyle.Font.Bold = false;
                sheet1.Range[_rowd, 3, _rowd, 4].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            }

            var _rows = 5;
            sheet1[_rows, 5].Text = "Report Ref No: ";
            sheet1[_rows, 5].CellStyle.Font.Size = 8;
            sheet1[_rows, 5].CellStyle.Font.Bold = false;
            sheet1.Range[_rows, 3, _rows, 6].Merge();

            var _row = 6;

            //sheet1[_row, 69].Text = "Posted (Dr.)";
            //sheet1[_row, 69].CellStyle.Font.Size = 10;
            //sheet1[_row, 69].CellStyle.Font.Bold = true;
            //sheet1.UsedRange.WrapText = true;
            //sheet1[_row, 69].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1[_row, 69].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_row, 69, _row, 75].BorderAround(ExcelLineStyle.Hair);
            //sheet1.Range[_row, 69, _row, 75].BorderInside(ExcelLineStyle.Hair);
            //sheet1.Range[_row, 69, _row, 75].Merge();
            //sheet1.Range[_row, 69, _row, 75].CellStyle.FillBackground = ExcelKnownColors.Tan;

            //sheet1[_row, 76].Text = "Posted (Cr.)";
            //sheet1[_row, 76].CellStyle.Font.Size = 10;
            //sheet1[_row, 76].CellStyle.Font.Bold = true;
            //sheet1.UsedRange.WrapText = true;
            //sheet1[_row, 76].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1[_row, 76].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_row, 76, _row, 82].BorderAround(ExcelLineStyle.Hair);
            //sheet1.Range[_row, 76, _row, 82].BorderInside(ExcelLineStyle.Hair);
            //sheet1.Range[_row, 76, _row, 82].Merge();
            //sheet1.Range[_row, 76, _row, 82].CellStyle.FillBackground = ExcelKnownColors.Tan;
            //sheet1[_row, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            var _rowL = _row;
            var row = _row + 1;
            //var xlsCol = 0;
            //var Article = 0;
            //var xlsRow = 0;

            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;

            _rowL += 1;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN No");
            ////wTable.Rows[ROW].Cells[sheet1headreColIndex].Width = 60;
            //sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Party");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "PartyId";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "PartyCode";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "InvoicingPartyPlantId";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "InvoicingPartyPlant";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "DeliveryPartyPlantId";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "DeliveryPartyPlant";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            ////report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GSTIN No");
            ////sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "GSTIN No";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;


            ////report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Employee");
            ////sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Employee";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Gate Entry No");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gate Entry No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Gate Name");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gate Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref No");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Doc Ref Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Grn Doc Date Difference");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Grn Doc Date Difference";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTransactionQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UoM");
            //sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TrnUoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTransactionQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "BaseUoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Rate");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

            sheet1headreColIndex++;



            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Lot No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;



            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Quality Status";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;




            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gross Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;



            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Discount Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Amount");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Taxable Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTransactionAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Total Material Tran Amount");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TotalMaterialTranAmount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTotalMaterialTranAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "TotalMaterialBooksCurrencyAmount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTotalMaterialBooksCurrencyAmountTotal = sheet1headreColIndex; 
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Credtible Status");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Credtible Status";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Tax Amount");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "RCM";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTaxAmountTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Tax Amount";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTaxAmountTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colCGSTTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST Tax (%)";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colCGSTTotal1 = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colSGSTTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST Tax (%)";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colSGSTTotal1 = sheet1headreColIndex;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colIGSTTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST Tax (%)";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colIGSTTotal1 = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTDSTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS Tax (%)";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTDSTotal1 = sheet1headreColIndex;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "MaterialTCS";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTCSTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "MaterialTCS Tax (%)";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTCSTotal1 = sheet1headreColIndex;
            //sheet1headreColIndex++;




            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRNTCS";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTCSTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRNTCS Tax (%)";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTCSTotal1 = sheet1headreColIndex;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "MandiTax";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTCSTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "MandiTax Tax (%)";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTCSTotal1 = sheet1headreColIndex;
            //sheet1headreColIndex++;



            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "NirasritTax";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTCSTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "NirasritTax Tax (%)";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTCSTotal1 = sheet1headreColIndex;
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "AIT";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTaxAmountTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "AIT Tax (%)";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTaxAmountTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Service Charge");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Charge";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Service Tax");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Service Tax";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Total Material Books Currency Amount");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Total Material Books Currency Amount";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //colTotalMaterialBooksCurrencyAmountTotal = sheet1headreColIndex;
            //sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Trn Currency Base Rate");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn Currency Base Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTrnCurrencyBaseRateTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Books Currency Base Rate");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Books Currency Base Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colBooksCurrencyBaseRateTotal = sheet1headreColIndex;
            sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "MMIsAsset");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "MMIsAsset";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRNIsAsset");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRNIsAsset";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "PO Id");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Id";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Storage Location");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Storage Location";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Shortage Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Shortage Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colShortageQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ShortageRatePercent");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "ShortageRatePercent";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ShortageValuet");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "ShortageValuet";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rejection Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rejection Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colRejectionQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Reject Rate Per");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Reject Rate Per";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "RejectionValue");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "RejectionValue";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "RejectionClam");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "RejectionClam";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ApprovedQty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Approved Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colApprovedQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Row ID");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Row ID";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN No");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Prepared By");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Prepared By";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Status");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Status";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Checking Name");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Checking Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Approving Name");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Approving Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Posted");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posted";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Posted By");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posted By";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Voucher No");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Voucher No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Contract No";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Posting Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posting Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            //sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL Code";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Budget");
            //sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget Code";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;




            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity ID";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity Code";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            //sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL Code";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "BUdget");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget Code";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;



            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");
            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity ID";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity Code";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "POREfference";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "LCRef";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "ContractNo";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //----------------------------
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "IssueQty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "BaseIssueQty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PurchaseReturnQty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "IssueReturnQty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "ReductionByAdjustmentQty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "InventorySalesQty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "InventoryScrapQty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "InventoryTransferQty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;








            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;
            //sheet1headreColIndex++;

            var Row_Total_Start = _rowL + 1;
            //List<string> list = new List<string>();
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                //var rcvid = inventoryMaterialList.Rows[n]["GRNId"].ToString();
                //if (list.Contains(rcvid))
                //{

                //}
                //else
                //{
                //	list.Add(rcvid);
                int COL = 1;
                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["GRNId"].ToString());

                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["GRNEntryDate"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["GRNType"].ToString());

                //report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["PartyName"].ToString());
                //report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["PartyId"].ToString());

                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["Code"].ToString());
                //report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["InvoicingPartyPlantId"].ToString());
                //report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["InvoicingPartyPlant"].ToString());
                //report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["DeliveryPartyPlantId"].ToString());
                //report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["DeliveryPartyPlant"].ToString());
                //report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["GSTINNo"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["FirstName"].ToString());
                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["GateEntryNo"].ToString());
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["GateName"].ToString());
                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["DocRefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["DocDate"].ToString());
                report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["GrnInvoiceDateDifference"].ToString());
                report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 20, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 21, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 22, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 23, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 24, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 25, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
                report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 27, inventoryMaterialList.Rows[n]["UOM"].ToString());

                report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 29, inventoryMaterialList.Rows[n]["BaseUoM"].ToString());
                report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 31, inventoryMaterialList.Rows[n]["LotNo"].ToString());
                report.SetText(ref sheet1, _rowL, 32, inventoryMaterialList.Rows[n]["QualityStatus"].ToString());
                report.SetText(ref sheet1, _rowL, 33, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GrossAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 34, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["DiscountAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 35, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 36, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 37, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialBaseAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 38, inventoryMaterialList.Rows[n]["CredtibleStatus"].ToString());
                //report.SetText(ref sheet1, _rowL, 39, inventoryMaterialList.Rows[n]["RCM"].ToString());
                //report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TaxAmount"].ToString()));
                //report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGSTTaxPercentage"].ToString()));
                //report.SetText(ref sheet1, _rowL, 40, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGST"].ToString()));
                //report.SetText(ref sheet1, _rowL, 41, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGSTTaxPercentage"].ToString()));
                //report.SetText(ref sheet1, _rowL, 42, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGST"].ToString()));
                //report.SetText(ref sheet1, _rowL, 43, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGSTTaxPercentage"].ToString()));
                //report.SetText(ref sheet1, _rowL, 44, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGST"].ToString()));
                //report.SetText(ref sheet1, _rowL, 45, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGSTTaxPercentage"].ToString()));
                //report.SetText(ref sheet1, _rowL, 46, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDS"].ToString()));
                //report.SetText(ref sheet1, _rowL, 47, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDSTaxPercentage"].ToString()));
                //report.SetText(ref sheet1, _rowL, 44, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCS"].ToString()));
                ////report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
                //report.SetText(ref sheet1, _rowL, 45, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TCSTaxPercentage"].ToString()));

                //report.SetText(ref sheet1, _rowL, 48, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTCS"].ToString()));
                //report.SetText(ref sheet1, _rowL, 44, rcvid);


                //report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
                //report.SetText(ref sheet1, _rowL, 49, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTCSTaxPercentage"].ToString()));

                //report.SetText(ref sheet1, _rowL, 50, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNTCS"].ToString()));
                //report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
                //report.SetText(ref sheet1, _rowL, 51, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNTCSTaxPercentage"].ToString()));

                //report.SetText(ref sheet1, _rowL, 52, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MandiTax"].ToString()));
                //report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
                //report.SetText(ref sheet1, _rowL, 53, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MandiTaxPercentage"].ToString()));

                //report.SetText(ref sheet1, _rowL, 54, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["NirasritTax"].ToString()));
                //report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["TCS"].ToString());
                //report.SetText(ref sheet1, _rowL, 55, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["NirasritTaxPercentage"].ToString()));

                report.SetText(ref sheet1, _rowL, 56, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TrnCurrencyBaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 57, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BooksCurrencyBaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 58, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                report.SetText(ref sheet1, _rowL, 59, inventoryMaterialList.Rows[n]["GRNAsset"].ToString());
                //report.SetText(ref sheet1, _rowL, 60, inventoryMaterialList.Rows[n]["POId"].ToString());
                report.SetText(ref sheet1, _rowL, 61, inventoryMaterialList.Rows[n]["StorageLocation"].ToString());
                report.SetText(ref sheet1, _rowL, 62, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ShortageQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 63, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ShortageRatePercent"].ToString()));
                report.SetText(ref sheet1, _rowL, 64, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ShortageValue"].ToString()));
                report.SetText(ref sheet1, _rowL, 65, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectionQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 66, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectRatePercent"].ToString()));
                report.SetText(ref sheet1, _rowL, 67, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectValue"].ToString()));
                report.SetText(ref sheet1, _rowL, 68, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectClamPercent"].ToString()));
                report.SetText(ref sheet1, _rowL, 69, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ApprovedQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 70, inventoryMaterialList.Rows[n]["GrnDetailId"].ToString());
                report.SetText(ref sheet1, _rowL, 71, inventoryMaterialList.Rows[n]["GRNId"].ToString());
                report.SetText(ref sheet1, _rowL, 72, inventoryMaterialList.Rows[n]["AddedBy"].ToString());
                report.SetText(ref sheet1, _rowL, 73, inventoryMaterialList.Rows[n]["GRNCheckStatus"].ToString());
                report.SetText(ref sheet1, _rowL, 74, inventoryMaterialList.Rows[n]["CheckedBY"].ToString());
                report.SetText(ref sheet1, _rowL, 75, inventoryMaterialList.Rows[n]["AuthorizedBy"].ToString());
                report.SetText(ref sheet1, _rowL, 76, inventoryMaterialList.Rows[n]["Posted"].ToString());
                report.SetText(ref sheet1, _rowL, 77, inventoryMaterialList.Rows[n]["PostedBy"].ToString());
                report.SetText(ref sheet1, _rowL, 78, inventoryMaterialList.Rows[n]["VoucherNo"].ToString());
                //report.SetText(ref sheet1, _rowL, 68, inventoryMaterialList.Rows[n]["ContractNo"].ToString());
                report.SetText(ref sheet1, _rowL, 79, inventoryMaterialList.Rows[n]["PostingDate"].ToString());
                report.SetText(ref sheet1, _rowL, 80, inventoryMaterialList.Rows[n]["GLCode"].ToString());
                report.SetText(ref sheet1, _rowL, 81, inventoryMaterialList.Rows[n]["GL"].ToString());
                report.SetText(ref sheet1, _rowL, 82, inventoryMaterialList.Rows[n]["BudgetrefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 83, inventoryMaterialList.Rows[n]["Budget"].ToString());
                //report.SetText(ref sheet1, _rowL, 84, inventoryMaterialList.Rows[n]["ActivityId"].ToString());
                report.SetText(ref sheet1, _rowL, 85, inventoryMaterialList.Rows[n]["ActivityCode"].ToString());
                report.SetText(ref sheet1, _rowL, 86, inventoryMaterialList.Rows[n]["Activity"].ToString());
                report.SetText(ref sheet1, _rowL, 87, inventoryMaterialList.Rows[n]["CGLCode"].ToString());
                report.SetText(ref sheet1, _rowL, 88, inventoryMaterialList.Rows[n]["CGL"].ToString());
                report.SetText(ref sheet1, _rowL, 89, inventoryMaterialList.Rows[n]["CBudgetrefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 90, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
                //report.SetText(ref sheet1, _rowL, 91, inventoryMaterialList.Rows[n]["CActivityId"].ToString());
                report.SetText(ref sheet1, _rowL, 92, inventoryMaterialList.Rows[n]["CActivityCode"].ToString());
                report.SetText(ref sheet1, _rowL, 93, inventoryMaterialList.Rows[n]["CActivity"].ToString());

                report.SetText(ref sheet1, _rowL, 94, inventoryMaterialList.Rows[n]["RefferenceNo"].ToString());
                report.SetText(ref sheet1, _rowL, 95, inventoryMaterialList.Rows[n]["LCANo"].ToString());
                report.SetText(ref sheet1, _rowL, 96, inventoryMaterialList.Rows[n]["ContractNo"].ToString());

                report.SetText(ref sheet1, _rowL, 97, inventoryMaterialList.Rows[n]["IssueQty"].ToString());
                report.SetText(ref sheet1, _rowL, 98, inventoryMaterialList.Rows[n]["BaseIssueQty"].ToString());
                report.SetText(ref sheet1, _rowL, 99, inventoryMaterialList.Rows[n]["PurchaseReturnQty"].ToString());
                report.SetText(ref sheet1, _rowL, 100, inventoryMaterialList.Rows[n]["IssueReturnQty"].ToString());
                report.SetText(ref sheet1, _rowL, 101, inventoryMaterialList.Rows[n]["ReductionByAdjustmentQty"].ToString());
                report.SetText(ref sheet1, _rowL, 102, inventoryMaterialList.Rows[n]["InventorySalesQty"].ToString());
                report.SetText(ref sheet1, _rowL, 103, inventoryMaterialList.Rows[n]["InventoryScrapQty"].ToString());
                report.SetText(ref sheet1, _rowL, 104, inventoryMaterialList.Rows[n]["InventoryTransferQty"].ToString());

                //}
            }
            _rowL++;

            if (fromDate != "" && toDate != "")
            {


                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, "Total");
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal) - 1].CellStyle.Font.Bold = true;
                //sheet1.Range[1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, _rowL].Merge();
                object sumObject;
                sumObject = inventoryMaterialList.Compute("Sum(MaterialTranAmount)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionAmountTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = inventoryMaterialList.Compute("Sum(CGST)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colCGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sumObject = inventoryMaterialList.Compute("Sum(SGST)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colSGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sumObject = inventoryMaterialList.Compute("Sum(IGST)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colIGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = inventoryMaterialList.Compute("Sum(TDS)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTDSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = inventoryMaterialList.Compute("Sum(MaterialTCS)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTCSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(ShortageQty)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colShortageQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(RejectionQty)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colRejectionQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(ApprovedQty)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colApprovedQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
            }

            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);
            //_rowL++;

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


        }


        public IWorkbook CreatePurchaseRegisterReportSheetExcel(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {

                var excelEngine = new ExcelEngine();
                var report = new Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var Head = "FG Inventory Register Report";// + " " + fromDate + " " + "To" + " " + toDate ;
                CreatePurchaseRegisterReportSheetsExcel(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void CreatePurchaseRegisterReportSheetsExcel(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {


            var cmdText = "";

            cmdText = @"Select * from (SELECT   --ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
						   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
							,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
							,IR.Id As GRNId
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
									else '' end
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   --,p.Id
                            ,p.UserName AS PartyName
						   ,EI.EmployeeName FirstName						   
						   ,IRD.Id As GrnDetailId
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   --,IR.InvoiceNo
						   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,MT.UserName MaterialType
						  ,MGM.UserName AS MaterialGroupMasterName
						  ,IM.MaterialMasterId
						  ,MM.UserName MaterialMasterName
					   -- , IM.ArticleId
						, ART.StandardName ArticleName
                        ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
                        ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
						--, IM.FirstCharacteristicsId
						--, FC.UserName AS FirstCharacteristics
						--, IM.FirstCharacteristicsValueId
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						--, IM.SecondCharacteristicsId
						--, SC.UserName AS SecondCharacteristics
						--, IM.SecondCharacteristicsValueId
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						--, IM.ThirdCharacteristicsId
						--, TC.UserName AS ThirdCharacteristics
						--, IM.ThirdCharacteristicsValueId
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						,TUoM.UserName AS UOM
						,IRD.TransactionQty
						,IRD.ShortageQty
						,IRD.ShortageRatePercent
						,IRD.ShortageValue
						,IRD.RejectionQty
						,IRD.RejectRatePercent
						,IRD.RejectValue
						,IRD.RejectClamPercent
						,IRD.ApprovedQty
						,IR.IsNonCreditable
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
						,ROUND(Isnull(IRD.MaterialTranAmount,0),2) MaterialTranAmount
						,ROUND(Isnull(IRD.TrnCurrencyBaseRate,0),2) TrnCurrencyBaseRate,ROUND(Isnull(IRD.BooksCurrencyBaseRate,0),2) BooksCurrencyBaseRate
						,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)

						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) MaterialTCS,TAxInfo6.Percentage MaterialTCSTaxPercentage
                        ,round(isnull(TAxInfo7.TaxAmount,0),2) GRNTCS,TAxInfo7.Percentage GRNTCSTaxPercentage
						,round(isnull(TAxInfo8.TaxAmount,0),2) MandiTax,TAxInfo8.Percentage MandiTaxPercentage
						,round(isnull(TAxInfo9.TaxAmount,0),2) NirasritTax,TAxInfo9.Percentage NirasritTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

                        --, IRD.ChargesTranAmount AS ChargesAmount	   
						--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
						--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
						--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
                         --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
                         --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						,IRD.ChargesTranAmount ServiceCharge
						,IRD.ChargesTaxTranAmount ServiceTax
						,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
						--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
						--,Case When IR.IsNonCreditable = 1 
							--then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--when IR.IsNonCreditable = 0
							--then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--end TotalMaterialTranAmount
                       ,ROUND(Isnull(IRD.TotalMaterialBooksCurrencyAmount,0),2) TotalMaterialBaseAmount ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
							

								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
						,IGL.UserName AS GL
						,IGL.AccountCode GLCode
						,IA.Id ActivityId
						,IA.UserName Activity
						,IA.Code ActivityCode
						,IBM.RefNo BudgetrefNo
						,B.UserName AS Budget
                        ,IGL1.UserName AS CGL
						,IGL1.AccountCode CGLCode
						,IA1.Id CActivityId
						,IA1.UserName AS CActivity
						,IA1.Code CActivityCode
						,IBM1.RefNo CBudgetrefNo
						,B1.UserName AS CBUdget
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
                        ,IRD.POId
						,IRD.PODetailsId AS PORowId
                        ,MS.UserName as StorageLocation--,V.VoucherNo

						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						--,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
                        --,isnull(p.TINNO,'') GSTINNo
						,isnull(PP.GSTIN,'') GSTINNo
						,IR.PartyId ,P.Code,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,IRD.LotNo , IRD.QualityStatus 
						,cu.Code Currency
						, IRD.GrossAmount ,IRD.DiscountAmount--,Isnull(C.ContractNo,'') ContractNo
						,ISNULL(PID.RefferenceNo,'') RefferenceNo
						--,isnull(PO.POId,'') POId
						,isnull(PO.PurchaseLCId,'') PurchaseLCId
						,isnull(PO.ContractId,'') ContractId						
						,ISNull(po.ContractNo,'') ContractNo
						,isnull(PO.LCANo,'') LCANo
						,isnull(PO.LCDate,'') LCDate
						,IRD.IssueQty
						,IRD.BaseIssueQty
						,IRD.PurchaseReturnQty
						,IRD.IssueReturnQty
						
						,IRD.ReductionByAdjustmentQty
						,IRD.InventorySalesQty
						,IRD.InventoryScrapQty						
						,IRD.InventoryTransferQty,IRD.BaseQty,BUoM.UserName BaseUoM
							,IR.ProductionOrderId
					from TRN.InventoryMaterial AS IM
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id
					--LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
					left join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveDetailId=IRD.Id
				    --Left JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId
					left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id	
					left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					--left join trn.Voucher V on V.Id=I.VoucherId
					left join trn.Voucher V on V.Id=IR.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
                    LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
					LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
					Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
					LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
					Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
	               LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
				   FROM [TRN].[InventoryReceiveTax] A
			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
			WHERE B.Code='CGST' and A.InventoryServiceId IS NULL
			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

								
			) TAxInfo	ON TAxInfo.InventoryReceiveDetailId=IRD.Id 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.InventoryServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo1	ON TAxInfo1.InventoryReceiveDetailId=IRD.Id 
							  		 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.InventoryServiceId IS NULL 
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo2	ON TAxInfo2.InventoryReceiveDetailId=IRD.Id 

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TDS' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
									) TAxInfo3	ON TAxInfo3.InventoryReceiveDetailId=IRD.Id 


							
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='VAT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo4 ON TAxInfo4.InventoryReceiveDetailId=IRD.Id

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='AIT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
							
						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IRD.Id
						--LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						--			LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						--			WHERE B.Code='TCS' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						--) TAxInfo6 ON TAxInfo6.InventoryReceiveDetailId=IRD.Id
	                    LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
						           FROM trn.InventoryReceiveAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' --and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo6 ON TAxInfo6.InventoryReceiveId=IR.Id

                        LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
						           FROM trn.InventoryReceiveAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' --and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo7 ON TAxInfo7.InventoryReceiveId=IR.Id


						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Mandi Tax' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo8 ON TAxInfo8.InventoryReceiveDetailId=IRD.Id


						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Nirasrit T' and A.InventoryServiceId IS NULL 
						) TAxInfo9 ON TAxInfo9.InventoryReceiveDetailId=IRD.Id
						LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
						LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
						--left join  trn.POGGRNMap POGGRNMap ON POGGRNMap.GRNId=IR.Id
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
						LEFT JOIN(
							   SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
							    ,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCRef from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						 --where  IR.PlantId='20181'  AND convert(Date,IR.GRNDate) BETWEEN  '01-OCT-2020' AND '31-OCT-2020' --ORDER BY IR.GRNDate ASC
						 where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
                        
						AND IR.GRNType ='FG' 

							UNION ALL

						SELECT 	--ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo                           
						   Distinct REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
							,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
							,IR.Id As GRNId
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
									else '' end --HSNC.Code HSNCode
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   --,p.Id
                            ,p.UserName AS PartyName
						   ,EI.EmployeeName FirstName						   
						   ,Null As GrnDetailId
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   --,IR.InvoiceNo
						   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,NULL MaterialType
						  ,NULL MaterialGroupMasterName
						  ,NULL MaterialMasterId
						    ,SM.UserName MaterialMasterName
					   -- , IM.ArticleId
						, SM.UserName ArticleName
                        ,'No' IsAsset
                        ,'No' GRNAsset
						--, IM.FirstCharacteristicsId
						--, FC.UserName AS FirstCharacteristics
						--, IM.FirstCharacteristicsValueId
						, NULL FirstCharacteristicsValue
						--, IM.SecondCharacteristicsId
						--, SC.UserName AS SecondCharacteristics
						--, IM.SecondCharacteristicsValueId
						, NULL SecondCharacteristicsValue
						--, IM.ThirdCharacteristicsId
						--, TC.UserName AS ThirdCharacteristics
						--, IM.ThirdCharacteristicsValueId
						, NULL ThirdCharacteristicsValue 
						,NULL AS UOM
						,0 TransactionQty
						,0 ShortageQty
						,0 ShortageRatePercent
						,0 ShortageValue
						,0 RejectionQty
						,0 RejectRatePercent
						,0 RejectValue
						,0 RejectClamPercent
						,0 ApprovedQty
						,IsNULL(IR.IsNonCreditable,0) IsNonCreditable
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,0 MaterialTranRate
						,ISs.Amount MaterialTranAmount
						,0 TrnCurrencyBaseRate
						,0 BooksCurrencyBaseRate
						, 0 TaxAmount
						--,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						--,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						--,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						--,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						--,round(isnull(TAxInfo5.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
							,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
							,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
							,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
							,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
							,round(isnull(TAxInfo6.TaxAmount,0),2) MaterialTCS,TAxInfo6.Percentage MaterialTCSTaxPercentage					
							,round(isnull(TAxInfo7.TaxAmount,0),2) GRNTCS,TAxInfo7.Percentage GRNTCSTaxPercentage
							,round(isnull(TAxInfo8.TaxAmount,0),2) MandiTax,TAxInfo8.Percentage MandiTaxPercentage
							,round(isnull(TAxInfo9.TaxAmount,0),2) NirasritTax,TAxInfo9.Percentage NirasritTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo.Percentage,0))/100) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo2.Percentage,0))/100) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo1.Percentage,0))/100) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo3.Percentage,0))/100) TDS,TAxInfo3.Percentage TDSTaxPercentage
						--,((Isnull((SELECT SUM(MaterialTranAmount) FROM [TRN].[InventoryReceiveDetail] WHERE Id=IRD.Id),0) * isnull(TAxInfo6.Percentage,0))/100) TCS,TAxInfo6.Percentage TCSTaxPercentage

                        --, IRD.ChargesTranAmount AS ChargesAmount	   
						--,totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id)
						--,totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)
						--,totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')
                         --,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
                         --,ServiceTax=((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						,0 ServiceCharge
						,0 ServiceTax
						--,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
						--,ROUND(Isnull(IRD.TotalMaterialBaseAmount,0),2) TotalMaterialBaseAmount
						--,Case When IR.IsNonCreditable = 1 
						--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2) + (SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id) + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount +((SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=IR.Id AND InventoryServiceId<>'')/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--when IR.IsNonCreditable = 0
						--	then ROUND(Isnull(IRD.MaterialTranAmount,0),2)  + ((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=IR.Id)/ISNULL(NULLIF((SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=IR.Id),0), 1))*IRD.MaterialTranAmount
						--end TotalMaterialTranAmount
						,0 TotalMaterialTranAmount
                       ,0 TotalMaterialBaseAmount ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
							

								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
						,Null AS GL
						,Null GLCode
						,Null ActivityId
						,Null Activity
						,Null ActivityCode
						,Null BudgetrefNo
						,Null AS Budget
                        ,Null AS CGL
						,Null CGLCode
						,Null CActivityId
						,Null AS CActivity
						,Null CActivityCode
						,Null CBudgetrefNo
						,NULL AS CBUdget
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
                        ,IR.POId
						,NULL AS PORowId
                        ,MS.UserName as StorageLocation--,V.VoucherNo

						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						--,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
                       -- ,isnull(p.TINNO,'') GSTINNo
					,isnull(PP.GSTIN,'') GSTINNo
					,IR.PartyId ,P.Code,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,Null LotNo , Null QualityStatus 
						,Null Currency
						, Null GrossAmount ,Null DiscountAmount--,Isnull(C.ContractNo,'') ContractNo
						,'' RefferenceNo
					,'' PurchaseLCId
					,'' ContractId						
					,'' ContractNo
					,'' LCANo
					,'' LCDate
					,0 IssueQty
					,0 BaseIssueQty
					,0 PurchaseReturnQty
					,0 IssueReturnQty
					
					,0 ReductionByAdjustmentQty
					,0 InventorySalesQty
					,0 InventoryScrapQty						
					,0 InventoryTransferQty,0 BaseQty,'' BaseUoM
					,null ProductionOrderId

			from trn.InventoryService AS ISs
			LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
			left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			--left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
			LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
			LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
			LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
			LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
			LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
			LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
			left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
			--left join trn.Voucher V on V.Id=I.VoucherId
			left join trn.Voucher V on V.Id=IR.VoucherId
			left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
			left join trn.Voucher V1 on V1.Id=ep.VoucherId
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
						,A.TaxAmount TaxAmount,HS.Code HSCode 
						FROM  [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='CGST'  
						--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
						) TAxInfo	ON TAxInfo.InventoryServiceId=ISs.Id AND TAxInfo.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,HS.Code HSCode 
			FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='IGST' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

						) TAxInfo1	ON TAxInfo1.InventoryServiceId=ISs.Id AND TAxInfo1.InventoryServiceId IS NOT NULL 
							  		 
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,HS.Code HSCode 
			FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='SGST' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								

						) TAxInfo2	ON TAxInfo2.InventoryServiceId=ISs.Id AND TAxInfo2.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						WHERE B.Code='TDS' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo3	ON TAxInfo3.InventoryServiceId=ISs.Id AND TAxInfo3.InventoryServiceId IS NOT NULL


							
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='VAT' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
			) TAxInfo4 ON TAxInfo4.InventoryServiceId=ISs.Id AND TAxInfo4.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='AIT' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
							
			) TAxInfo5 ON TAxInfo5.InventoryServiceId=ISs.Id AND TAxInfo5.InventoryServiceId IS NOT NULL
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
			) TAxInfo6 ON TAxInfo6.InventoryServiceId=ISs.Id AND TAxInfo6.InventoryServiceId IS NOT NULL
          
            LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS' and A.InventoryServiceId IS NOT NULL  
								
			) TAxInfo7 ON TAxInfo7.InventoryServiceId=ISs.Id AND TAxInfo7.InventoryServiceId IS NOT NULL
			 LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='Mandi Tax' and A.InventoryServiceId IS NOT NULL  
								
			) TAxInfo8 ON TAxInfo8.InventoryServiceId=ISs.Id AND TAxInfo8.InventoryServiceId IS NOT NULL
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='Nirasrit T' and A.InventoryServiceId IS NOT NULL 
								
			) TAxInfo9 ON TAxInfo9.InventoryServiceId=ISs.Id AND TAxInfo9.InventoryServiceId IS NOT NULL
	               
			LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
			LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
			--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
			where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'  --and IR.Id='20211740'
			--AND IRT.InventoryServiceId is not null
			AND IR.GRNType = 'FG' 
			--AND IR.GRNType<>'GRNBYPO'
			)x
			Order By X.GRNEntryDate ASC";


            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            //var colTransactionQtyTotal = 0.00;
            var colTransactionAmountTotal = 0.00;
            var colTotalMaterialTranAmountTotal = 0.00;
            var colTaxAmountTotal = 0.00;
            var colTotalMaterialBooksCurrencyAmountTotal = 0.00;
            var colTrnCurrencyBaseRateTotal = 0.00;
            var colBooksCurrencyBaseRateTotal = 0.00;
            var colShortageQtyTotal = 0.00;
            var colRejectionQtyTotal = 0.00;
            var colApprovedQtyTotal = 0.00;

            var colCGSTTotal = 0.00;
            var colSGSTTotal = 0.00;
            var colIGSTTotal = 0.00;
            var colTDSTotal = 0.00;
            var colTCSTotal = 0.00;

            var colCGSTTotal1 = 0.00;
            var colSGSTTotal1 = 0.00;
            var colIGSTTotal1 = 0.00;
            var colTDSTotal1 = 0.00;
            var colTCSTotal1 = 0.00;


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _rowd = 4;

            if (fromDate != "" && toDate != "")
            {
                sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;
                sheet1[_rowd, 4].CellStyle.Font.Size = 8;
                sheet1[_rowd, 4].CellStyle.Font.Bold = false;
                sheet1.Range[_rowd, 3, _rowd, 4].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter
            }

            var _rows = 5;
            sheet1[_rows, 5].Text = "Report Ref No: ";
            sheet1[_rows, 5].CellStyle.Font.Size = 8;
            sheet1[_rows, 5].CellStyle.Font.Bold = false;
            sheet1.Range[_rows, 3, _rows, 6].Merge();

            var _row = 6;
            var _rowL = _row;
            var row = _row + 1;
            //var xlsCol = 0;
            //var Article = 0;
            //var xlsRow = 0;

            //var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;

            _rowL += 1;

            int COL = 1;
            //int ROW = 5;
            int startCol = COL;

            //worksheet[ROW, COL].Text = "SL. No";
            //worksheet[ROW, COL].ColumnWidth = 5;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;


            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN No");
            ////wTable.Rows[ROW].Cells[sheet1headreColIndex].Width = 60;
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, COL].Text = "FG Ref No";
            int colGRNId = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Date";
            int colGRNDate = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            //sheet1.Range[_rowL, COL].Text = "Gate Entry No";
            //int colGateEntryNo = COL;
            //sheet1.Range[_rowL, COL].ColumnWidth = 15;
            //sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //COL++;

            //sheet1.Range[_rowL, COL].Text = "Gate Name";
            //int colGateName = COL;
            //sheet1.Range[_rowL, COL].ColumnWidth = 15;
            //sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //COL++;


            sheet1.Range[_rowL, COL].Text = "PrO";
            int colProductionId = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Doc Ref No";
            int colDocRefNo = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Doc Ref Date";
            int colDocRefDate = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            //sheet1.Range[_rowL, COL].Text = "Grn Doc Date Difference";
            //int colGRnDocDateDifference = COL;
            //sheet1.Range[_rowL, COL].ColumnWidth = 25;
            //sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //COL++;

            sheet1.Range[_rowL, COL].Text = "Material Type";
            int colMaterialType = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 25;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Material Group";
            int colMaterialGroup = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 25;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;


            sheet1.Range[_rowL, COL].Text = "Material";
            int colMaterial = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 20;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Article";
            int colArticle = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 30;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "SKU1";
            int colSKU1 = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "SKU2";
            int colSKU2 = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;


            sheet1.Range[_rowL, COL].Text = "SKU3";
            int colSKU3 = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "HSN No";
            int colHSNNo = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Transaction Qty";
            int colTransactionQty = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            //sheet1.Range[_rowL, COL].Text = "TrnUoM";
            //int colTrnUOM = COL;
            //sheet1.Range[_rowL, COL].ColumnWidth = 8;
            //sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //COL++;

            sheet1.Range[_rowL, COL].Text = "Base Qty";
            int colBaseQty = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //colTransactionQtyTotal = sheet1headreColIndex;
            COL++;

            //sheet1.Range[_rowL, COL].Text = "BaseUoM";
            //int colBaseUoM = COL;
            //sheet1.Range[_rowL, COL].ColumnWidth = 8;
            //sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //COL++;

            sheet1.Range[_rowL, COL].Text = "Transaction Rate";
            int colTransactionRate = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 20;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Lot No";
            int colLotNo = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            //sheet1.Range[_rowL, COL].Text = "Quality Status";
            //int colQualityStatus = COL;
            //sheet1.Range[_rowL, COL].ColumnWidth = 15;
            //sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //COL++;

            sheet1.Range[_rowL, COL].Text = "Currency";
            int colCurrency = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Gross Amount";
            int colGrossAmount = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Discount Amount";
            int colDiscountAmount = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Taxable Amount";
            int colTaxableAmount = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 20;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //colTransactionAmountTotal = sheet1headreColIndex;
            COL++;

            sheet1.Range[_rowL, COL].Text = "TotalMaterialTranAmount";
            int colTotalMaterialTranAmount = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 25;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "TotalMaterialBooksCurrencyAmount";
            int colTotalMaterialBooksCurrencyAmount = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 25;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //colTotalMaterialBooksCurrencyAmountTotal = sheet1headreColIndex; 
            COL++;


            //sheet1.Range[_rowL, COL].Text = "Credtible Status";
            //int colCredtibleStatus = COL;
            //sheet1.Range[_rowL, COL].ColumnWidth = 15;
            //sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //COL++;

            sheet1.Range[_rowL, COL].Text = "Trn Currency Base Rate";
            int colTrnCurrencyBaseRate = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 25;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //colTrnCurrencyBaseRateTotal = sheet1headreColIndex;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Books Currency Base Rate";
            int colBooksCurrencyBaseRate = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 25;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            colBooksCurrencyBaseRateTotal = COL;
            COL++;

            sheet1.Range[_rowL, COL].Text = "MMIsAsset";
            int colMMIsAsset = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "GRNIsAsset";
            int colGRNIsAsset = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Storage Location";
            int colStorageLocation = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 20;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "GRN Row ID";
            int colGRNRowId = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Prepared By";
            int colPreparedBY = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;


            //sheet1.Range[_rowL, COL].Text = "Status";
            //sheet1.Range[_rowL, COL].ColumnWidth = 15;
            //sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //COL++;


            sheet1.Range[_rowL, COL].Text = "Posted";
            int colPosted = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;


            sheet1.Range[_rowL, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;


            sheet1.Range[_rowL, COL].Text = "Posting Date";
            int colPostingDate = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;


            sheet1.Range[_rowL, COL].Text = "GL Code";
            int colGLCode = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "GL";
            int colGL = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Budget Code";
            int colBudgetCode = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Budget";
            int colBudget = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Activity Code";
            int colActivityCode = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Activity";
            int colActivity = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "CR GL Code";
            int colCRGLCode = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "CR GL";
            int colCRGL = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "CR Budget Code";
            int colCRBudgetCode = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "CR Budget";
            int colCRBudget = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;


            sheet1.Range[_rowL, COL].Text = "CR Activity Code";
            int colCRActivityCode = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "CR Activity";
            int colCRActivity = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 10;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "LC Ref";
            int colLCRef = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Contract No";
            int colContractNo = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Issue Qty";
            int colIssueQty = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Base Issue Qty";
            int colBaseIssueQty = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "Purchase Return Qty";
            int colPurchaseReturnQty = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "IssueReturnQty";
            int colIssueReturnQty = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            //sheet1.Range[_rowL, COL].Text = "ReductionByAdjustmentQty";
            //int colReductionByAdjustmentQty = COL;
            //sheet1.Range[_rowL, COL].ColumnWidth = 15;
            //sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            //COL++;

            sheet1.Range[_rowL, COL].Text = "InventorySalesQty";
            int colInventorySalesQty = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "InventoryScrapQty";
            int colInventoryScrapQty = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;
            COL++;

            sheet1.Range[_rowL, COL].Text = "InventoryTransferQty";
            int colInventoryTransferQty = COL;
            sheet1.Range[_rowL, COL].ColumnWidth = 15;
            sheet1.Range[_rowL, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1.Range[_rowL, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, COL].CellStyle.Font.Bold = true;

            sheet1.Range[_rowL, 1, _rowL, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, COL].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, COL].RowHeight = 22;
            //sheet1headreColIndex++;	  

            var Row_Total_Start = _rowL + 1;
            //List<string> list = new List<string>();
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                //var rcvid = inventoryMaterialList.Rows[n]["GRNId"].ToString();
                //if (list.Contains(rcvid))
                //{

                //}
                //else
                //{
                //	list.Add(rcvid);
                //int COL = 1;
                report.SetText(ref sheet1, _rowL, colGRNId, inventoryMaterialList.Rows[n]["GRNId"].ToString());

                report.SetText(ref sheet1, _rowL, colGRNDate, inventoryMaterialList.Rows[n]["GRNEntryDate"].ToString());
                //report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["GRNType"].ToString());

                //report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["PartyName"].ToString());
                //report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["PartyId"].ToString());

                //report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["Code"].ToString());
                //report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["InvoicingPartyPlantId"].ToString());
                //report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["InvoicingPartyPlant"].ToString());
                //report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["DeliveryPartyPlantId"].ToString());
                //report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["DeliveryPartyPlant"].ToString());

                //report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["GSTINNo"].ToString());
                report.SetText(ref sheet1, _rowL, colProductionId, inventoryMaterialList.Rows[n]["ProductionOrderId"].ToString());
                //report.SetText(ref sheet1, _rowL, colGateEntryNo, inventoryMaterialList.Rows[n]["GateEntryNo"].ToString());
                //report.SetText(ref sheet1, _rowL, colGateName, inventoryMaterialList.Rows[n]["GateName"].ToString());
                report.SetText(ref sheet1, _rowL, colDocRefNo, inventoryMaterialList.Rows[n]["DocRefNo"].ToString());
                report.SetText(ref sheet1, _rowL, colDocRefDate, inventoryMaterialList.Rows[n]["DocDate"].ToString());
                //report.SetText(ref sheet1, _rowL, colGRnDocDateDifference, inventoryMaterialList.Rows[n]["GrnInvoiceDateDifference"].ToString());
                report.SetText(ref sheet1, _rowL, colMaterialType, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                report.SetText(ref sheet1, _rowL, colMaterialGroup, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, colMaterial, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, colArticle, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, colSKU1, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, colSKU2, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, colSKU3, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, colHSNNo, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
                report.SetText(ref sheet1, _rowL, colTransactionQty, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()));
                //report.SetText(ref sheet1, _rowL, colTrnUOM, inventoryMaterialList.Rows[n]["UOM"].ToString());
                report.SetText(ref sheet1, _rowL, colCurrency, inventoryMaterialList.Rows[n]["Currency"].ToString());

                report.SetText(ref sheet1, _rowL, colBaseQty, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseQty"].ToString()));
                //report.SetText(ref sheet1, _rowL, colBaseUoM, inventoryMaterialList.Rows[n]["BaseUoM"].ToString());
                report.SetText(ref sheet1, _rowL, colTransactionRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranRate"].ToString()));
                report.SetText(ref sheet1, _rowL, colLotNo, inventoryMaterialList.Rows[n]["LotNo"].ToString());
                //report.SetText(ref sheet1, _rowL, colQualityStatus, inventoryMaterialList.Rows[n]["QualityStatus"].ToString());
                report.SetText(ref sheet1, _rowL, colGrossAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GrossAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, colDiscountAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["DiscountAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, colTaxableAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, colTotalMaterialTranAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialTranAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, colTotalMaterialBooksCurrencyAmount, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialBaseAmount"].ToString()));
                //report.SetText(ref sheet1, _rowL, colCredtibleStatus, inventoryMaterialList.Rows[n]["CredtibleStatus"].ToString());


                report.SetText(ref sheet1, _rowL, colTrnCurrencyBaseRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TrnCurrencyBaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, colBooksCurrencyBaseRate, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BooksCurrencyBaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, colMMIsAsset, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
                report.SetText(ref sheet1, _rowL, colGRNIsAsset, inventoryMaterialList.Rows[n]["GRNAsset"].ToString());
                report.SetText(ref sheet1, _rowL, colStorageLocation, inventoryMaterialList.Rows[n]["StorageLocation"].ToString());

                report.SetText(ref sheet1, _rowL, colGRNRowId, inventoryMaterialList.Rows[n]["GrnDetailId"].ToString());
                //report.SetText(ref sheet1, _rowL, 43, inventoryMaterialList.Rows[n]["GRNId"].ToString());
                report.SetText(ref sheet1, _rowL, colPreparedBY, inventoryMaterialList.Rows[n]["AddedBy"].ToString());


                report.SetText(ref sheet1, _rowL, colPosted, inventoryMaterialList.Rows[n]["Posted"].ToString());
                //report.SetText(ref sheet1, _rowL, 48, inventoryMaterialList.Rows[n]["PostedBy"].ToString());
                report.SetText(ref sheet1, _rowL, colVoucherNo, inventoryMaterialList.Rows[n]["VoucherNo"].ToString());
                report.SetText(ref sheet1, _rowL, colPostingDate, inventoryMaterialList.Rows[n]["PostingDate"].ToString());
                report.SetText(ref sheet1, _rowL, colGLCode, inventoryMaterialList.Rows[n]["GLCode"].ToString());
                report.SetText(ref sheet1, _rowL, colGL, inventoryMaterialList.Rows[n]["GL"].ToString());
                //report.SetText(ref sheet1, _rowL, col, inventoryMaterialList.Rows[n]["BudgetrefNo"].ToString());
                report.SetText(ref sheet1, _rowL, colBudget, inventoryMaterialList.Rows[n]["Budget"].ToString());
                //report.SetText(ref sheet1, _rowL, 56, inventoryMaterialList.Rows[n]["ActivityId"].ToString());
                report.SetText(ref sheet1, _rowL, colActivityCode, inventoryMaterialList.Rows[n]["ActivityCode"].ToString());
                report.SetText(ref sheet1, _rowL, colActivity, inventoryMaterialList.Rows[n]["Activity"].ToString());

                report.SetText(ref sheet1, _rowL, colCRGLCode, inventoryMaterialList.Rows[n]["CGLCode"].ToString());
                report.SetText(ref sheet1, _rowL, colCRGL, inventoryMaterialList.Rows[n]["CGL"].ToString());
                //report.SetText(ref sheet1, _rowL, colcrbud, inventoryMaterialList.Rows[n]["CBudgetrefNo"].ToString());
                report.SetText(ref sheet1, _rowL, colCRBudget, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
                //report.SetText(ref sheet1, _rowL, 63, inventoryMaterialList.Rows[n]["CActivityId"].ToString());
                report.SetText(ref sheet1, _rowL, colCRActivityCode, inventoryMaterialList.Rows[n]["CActivityCode"].ToString());
                report.SetText(ref sheet1, _rowL, colCRActivity, inventoryMaterialList.Rows[n]["CActivity"].ToString());

                //report.SetText(ref sheet1, _rowL, 64, inventoryMaterialList.Rows[n]["RefferenceNo"].ToString());
                report.SetText(ref sheet1, _rowL, colLCRef, inventoryMaterialList.Rows[n]["LCANo"].ToString());
                report.SetText(ref sheet1, _rowL, colContractNo, inventoryMaterialList.Rows[n]["ContractNo"].ToString());

                report.SetText(ref sheet1, _rowL, colIssueQty, inventoryMaterialList.Rows[n]["IssueQty"].ToString());
                report.SetText(ref sheet1, _rowL, colBaseIssueQty, inventoryMaterialList.Rows[n]["BaseIssueQty"].ToString());
                report.SetText(ref sheet1, _rowL, colPurchaseReturnQty, inventoryMaterialList.Rows[n]["PurchaseReturnQty"].ToString());
                report.SetText(ref sheet1, _rowL, colIssueReturnQty, inventoryMaterialList.Rows[n]["IssueReturnQty"].ToString());
                //report.SetText(ref sheet1, _rowL, colReductionByAdjustmentQty, inventoryMaterialList.Rows[n]["ReductionByAdjustmentQty"].ToString());
                report.SetText(ref sheet1, _rowL, colInventorySalesQty, inventoryMaterialList.Rows[n]["InventorySalesQty"].ToString());
                report.SetText(ref sheet1, _rowL, colInventoryScrapQty, inventoryMaterialList.Rows[n]["InventoryScrapQty"].ToString());
                report.SetText(ref sheet1, _rowL, colInventoryTransferQty, inventoryMaterialList.Rows[n]["InventoryTransferQty"].ToString());

                //}
            }
            _rowL++;

            if (fromDate != "" && toDate != "")
            {


                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionQty) - 1, "Total");
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQty) - 1].CellStyle.Font.Bold = true;
                //sheet1.Range[1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, _rowL].Merge();
                object sumObject;
                sumObject = inventoryMaterialList.Compute("Sum(MaterialTranAmount)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTaxableAmount)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTaxableAmount), Convert.ToDouble(sumObject).ToString("0.##"));
                sheet1.Range[_rowL, Convert.ToInt32(colTaxableAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTaxableAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = inventoryMaterialList.Compute("Sum(CGST)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colCGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sumObject = inventoryMaterialList.Compute("Sum(SGST)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colSGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sumObject = inventoryMaterialList.Compute("Sum(IGST)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colIGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = inventoryMaterialList.Compute("Sum(TDS)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTDSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = inventoryMaterialList.Compute("Sum(MaterialTCS)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTCSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = inventoryMaterialList.Compute("Sum(ShortageQty)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colShortageQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = inventoryMaterialList.Compute("Sum(RejectionQty)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colRejectionQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = inventoryMaterialList.Compute("Sum(ApprovedQty)", "");
                //sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet1, _rowL, Convert.ToInt32(colApprovedQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
            }

            sheet1.Range[(Row_Total_Start), 1, _rowL, COL].CellStyle.Font.Size = 8;

            sheet1.Range[(row), 1, _rowL, COL].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, COL].BorderAround(ExcelLineStyle.Hair);
            //_rowL++;

            sheet1.Range[(row), 1, _rowL, COL].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, COL].BorderAround(ExcelLineStyle.Hair);

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, COL, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


        }


        public List<Dictionary<string, object>> GetFGInventoryRegisterPoPUpListData(string companyGroupId, string companyId, string plantId, string finishGoodsBookingId)
        {
            try
            {
                var sql = @"SELECT Id,FORMAT(GRNDate,'dd-MMM-yyyy')GRNDate,DocRefNo,FORMAT(EntryDate,'dd-MMM-yyyy')EntryDate,FixedAssetOrInventory,[Status],GRNType FROM TRN.InventoryReceive WHERE FinishGoodsBookingId='" + finishGoodsBookingId + @"' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #endregion
    }

}

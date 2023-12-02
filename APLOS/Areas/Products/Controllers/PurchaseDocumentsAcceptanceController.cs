
using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Security.Core;
using Library.MaterialManagement.Inventory;
using Library.Service.Invoices;
using Library.ViewModel.Inventory;
using Library.ViewModel.Invoices;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Products.Controllers
{
    public class PurchaseDocumentsAcceptanceController : BaseController
    {
        #region Constructor


        private readonly IPurchaseDocumentAcceptanceService _purchaseDocumentAcceptance;
        private readonly IInventoryPayableService _inventoryPayableService;
        private readonly IInvoiceReportService _invoiceReportService;
        private readonly ISqlRepository _sqlRepository;

        public PurchaseDocumentsAcceptanceController(
            IPurchaseDocumentAcceptanceService purchaseDocumentAcceptance,
            IInventoryPayableService inventoryPayableService
            , IInvoiceReportService invoiceReportService
            , ISqlRepository sqlRepository
           )

        {
            _purchaseDocumentAcceptance = purchaseDocumentAcceptance;
            _inventoryPayableService = inventoryPayableService;
            _invoiceReportService = invoiceReportService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Aplos

        public ActionResult PurchaseDocAcceptance()
        {
            return View();
        }


        public ActionResult PurchaseDocAcceptancePost()
        {
            return View();
        }
        #endregion Aplos

        [Authorize, HttpGet]
        public JsonResult GetPOWithLCList(string PoType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.GetPOWithLCList(identity.PlantId, PoType), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetLCWisePOList(string PoType, string PurchaseLCNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.GetLCWisePOList(identity.PlantId, PoType, PurchaseLCNo), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGRNList(string purchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.GetGRNList(identity.PlantId, purchaseLCId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSavedGRNList(string purchaseLCId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var Sql = @"SELECT Convert(bit,0) Active,IR.Id,RD.TotalMaterialTranAmount,PO.Id POId, PO.DocRefNo PODocRefNo,IR.DocRefNo
                //            ,P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                //            ,IR.GateEntryNo,C.Code Currency,POD.TransactionAmount
                //            FROM [TRN].[InventoryReceive] AS IR 
                //            JOIN (SELECT SUM(TotalMaterialTranAmount) TotalMaterialTranAmount,InventoryReceiveId,POId FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,POId) RD ON RD.InventoryReceiveId=IR.Id
                //            LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=RD.POId
                //            JOIN(SELECT SUM(TransactionAmount) TransactionAmount,InventoryReceiveId FROM [TRN].[PurchaseOrderDetail] GROUP BY InventoryReceiveId)POD ON POD.InventoryReceiveId=PO.Id
                //            JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                //            JOIN [SCS].[Currency] C ON C.Id=IR.CurrencyId
                //            WHERE IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.VoucherId,'')<>'' AND IR.[Status]='Posting' AND IR.IsApproved=1 AND PO.PurchaseLCId='" + purchaseLCId + @"' 
                //            AND IR.Id IN (SELECT GRNId FROM [TRN].[GRNAcceptanceMap] WHERE PurchaseDocumentAcceptanceId IS NOT NULL)";

                string Sql = @"SELECT Convert(bit,0) Active,IR.Id,SUM(RD.TotalMaterialTranAmount) TotalMaterialTranAmount
                            ,IR.DocRefNo,P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                            ,IR.GateEntryNo,C.Code Currency
                            ,PODocRefNo= STUFF((select distinct ','+PO.DocRefNo
			                            from TRN.POGGRNMap PG 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			                            where PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,RD.POId,RD.PODetailsId
                            FROM [TRN].[InventoryReceive] AS IR 
                            LEFT JOIN [TRN].[InventoryReceiveDetail] RD ON RD.InventoryReceiveId=IR.Id
                            LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN [SCS].[Currency] C ON C.Id=IR.CurrencyId
                            WHERE IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.VoucherId,'')<>'' 
                            AND IR.[Status]='Posting' AND IR.IsApproved=1 AND  RD.POId IN (SELECT Id From TRN.PurchaseOrder Where PurchaseLCId='" + purchaseLCId + @"')
                            AND IR.Id IN (SELECT GRNId FROM [TRN].[GRNAcceptanceMap] WHERE PurchaseDocumentAcceptanceId IS NOT NULL)
                            GROUP BY IR.Id,IR.DocRefNo,P.UserName,IR.DocDate,IR.GateEntryNo,C.Code,RD.POId,RD.PODetailsId";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetPrePurchaseInvoiceList(string lcId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT P.Id,P.InvoiceNo,REPLACE(Convert(VARCHAR(11), P.InvoiceDate, 106), ' ', '-') InvoiceDate
                        ,PLC.LCRef PurchaseLCNo,P.BLAWBNo,REPLACE(Convert(VARCHAR(11), P.BLAWBDate, 106), ' ', '-') BLAWBDate
                        ,P.ShipmentModeId,P.PackingDescription,P.VesselTrackingNo,SM.UserName ShipmentMode,P.PurchaseLCId
                        FROM dbo.PrePurchaseInvoice P
					    LEFT JOIN [MST].[ShipMode] SM ON SM.Id=P.ShipmentModeId
					    LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=P.PurchaseLCId WHERE PurchaseLCId='" + lcId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListByOnlyPO(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.QueryOnlyPO(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGRNDetailData(GridParameter parameters, string inveReveiveId, string PurchaseDocAcceptanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.GetGRNDetailData(parameters, inveReveiveId, PurchaseDocAcceptanceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAcceptanceCharges()
        {
            return Json(_purchaseDocumentAcceptance.GetAcceptanceCharges(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetIsAccepptanceFirstData(string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var IsAccepptanceFirst = _purchaseDocumentAcceptance.GetIsAccepptanceFirstData(masterId, identity.PlantId);
            return Json(new { IsAccepptanceFirst }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail
            //, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceTax, IEnumerable<PurchaseDocAcceptanceChargesViewModel> AcceptancechargesList
            //, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptancechargesTax
            //, IEnumerable<PurchaseDocAcceptanceService> purchaseDocAcceptanceService, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax
            )
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            entity.EntryDate = entity.AcceptanceDate;
            if (PurchaseDocAcceptanceDetail == null)
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            //if (AcceptancechargesList != null)
            //{
            //    foreach (var item in AcceptancechargesList)
            //    {
            //        //if (!item.check)
            //        //{
            //        //    throw new CustomException("Please Select Materials !");
            //        //}
            //        //else if (item.Amount.ToString() == "0")
            //        //{
            //        //    throw new CustomException("Please Input  Amount !");
            //        //}

            //    }
            //}
            _purchaseDocumentAcceptance.InsertOrUpdateGraphNew(entity, PurchaseDocAcceptanceDetail/*, purchaseDocAcceptanceTax, AcceptancechargesList, purchaseDocAcceptancechargesTax, purchaseDocAcceptanceService, purchaseDocAcceptanceServiceTax*/);
            return Json(new { entity, Message = AplosMessage.Success + " Purchase Document Acceptance no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public JsonResult Update(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail
            , IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceServiceDetail
            , IEnumerable<PurchaseDocAcceptanceService> purchaseDocAcceptanceService, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax
            )
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;

            _purchaseDocumentAcceptance.InsertOrUpdate(entity, PurchaseDocAcceptanceDetail, PurchaseDocAcceptanceServiceDetail, purchaseDocAcceptanceService, purchaseDocAcceptanceServiceTax);
            return Json(new { entity, Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult SaveMaterialTax(IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceTax, string PurchaseDocAcceptanceId)
        {
            _purchaseDocumentAcceptance.SaveMaterialTax(purchaseDocAcceptanceTax, PurchaseDocAcceptanceId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult SaveOrUpdateServiceTax(IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax, string PurchaseDocAcceptanceId, string PurchaseDocAcceptanceServiceId)
        {
            _purchaseDocumentAcceptance.SaveOrUpdateServiceTax(purchaseDocAcceptanceServiceTax, PurchaseDocAcceptanceId, PurchaseDocAcceptanceServiceId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult SaveServiceAndServiceTax(IEnumerable<PurchaseDocAcceptanceService> purchaseDocAcceptanceService, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptanceServiceTax, string PurchaseDocAcceptanceId)
        {
            _purchaseDocumentAcceptance.SaveServiceAndServiceTax(purchaseDocAcceptanceService, purchaseDocAcceptanceServiceTax, PurchaseDocAcceptanceId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult SaveServiceChargesAndChargesTax(IEnumerable<PurchaseDocAcceptanceChargesViewModel> AcceptancechargesList, IEnumerable<PurchaseDocAcceptanceTax> purchaseDocAcceptancechargesTax, PurchaseDocAcceptance entity)
        {
            _purchaseDocumentAcceptance.SaveServiceChargesAndChargesTax(AcceptancechargesList, purchaseDocAcceptancechargesTax, entity);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult ServiceChargesCreate(PurchaseDocAcceptanceService entity, IEnumerable<PurchaseDocAcceptanceTax> taxCategoryList)
        {
            _purchaseDocumentAcceptance.InsertOrUpdatePurchaseDocAcceptanceService(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceChargeList(string purchaseDocAcceptanceId)
        {
            return Json(_purchaseDocumentAcceptance.GetPurchaseDocAcceptanceService(purchaseDocAcceptanceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceTaxList(string serviceId)
        {
            return Json(_purchaseDocumentAcceptance.GetServiceTaxList(serviceId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAcceptanceList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_purchaseDocumentAcceptance.GetAcceptanceList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAcceptanceDetailList(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_purchaseDocumentAcceptance.GetAcceptanceDetailList(Id, identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetMaterialById(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_purchaseDocumentAcceptance.GetMaterialById(Id, identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAcceptanceServiceList(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_purchaseDocumentAcceptance.GetAcceptanceServiceList(identity.PlantId, Id), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [Authorize, HttpGet]
        public JsonResult GetAcceptanceChargesTaxList(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_purchaseDocumentAcceptance.GetAcceptanceChargesTaxList(identity.PlantId, Id), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.GetTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetRecordDoubleClickMaster(string Id, string PoType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.GetRecordDoubleClickMaster(identity.PlantId, Id, PoType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetRecordDoubleClickDetail(string Id, string PoType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.GetRecordDoubleClickDetail(identity.PlantId, Id, PoType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetRecordDoubleClickGRNDetail(string Id, string PoType)
        {

            try
            {
                string sql = @"SELECT  PAD.Id ,PAD.POId,PAD.PODetailId, RD.InventoryReceiveId, MGM.UserName AS MaterialGroupMasterName, MM.Id MaterialMasterId, MM.UserName, PAD.ArticleId, ART.StandardName, PAD.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                    , PAD.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue, PAD.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                    , PAD.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue, PAD.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                    , PAD.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                                    , PD.TransactionQty POQty, RD.TransactionQty CurrentGRNQty, RD.TransactionQty AS GRNRcvQty
                                    , 0 AS PreviousRcvQty,ISNULL(PAD.TransactionQty, 0) AS TransactionQty ,0 Otherqty,(RD.TransactionQty - PAD.TransactionQty) As Balance                       , TotalGRN=RD.TransactionQty
                                    , PD.TransactionUoMId, TUoM.UserName AS TransactionUoM, PAD.MaterialTranRate TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
                                    ,PAD.MaterialTranAmount AS TrnAmount,0 AS BaseTaxAmount, 0 AS ChargesAmount,0 AS ServiceCharge, 0 AS ServiceTax ,'True' enableid
                                    ,null POMaterialTaxList, IR.InvoicingByAddress,IR.DeliveryByAddress,PD.Description MaterialDetail
                                    ,ISNULL(PAD.TotalMaterialTranAmount,0) TotalMaterialTranAmount,ISNULL(PAD.TaxAmount,0) TaxAmount,ISNULL(PAD.ChargesTranAmount,0) ChargesTranAmount,ISNULL(PAD.ChargesTaxTranAmount,0) ChargesTaxTranAmount,''TaxList
                                    ,[Active]=CAST (CASE WHEN PAD.Id IS NULL THEN 0 ELSE 1 END AS bit)

                                    FROM TRN.PurchaseDocAcceptanceDetail PAD
                                    JOIN TRN.[PurchaseOrderDetail] PD ON PD.InventoryReceiveId=PAD.POId AND PD.Id=PAD.PODetailId
                                    JOIN TRN.[InventoryReceiveDetail] RD ON RD.PurchaseDocumentAcceptanceId=PAD.PurchaseDocAcceptanceId AND RD.PurchaseDocumentAcceptanceDetailId=PAD.Id
                                    LEFT JOIN MST.MaterialMaster AS MM ON PAD.MaterialMasterId = MM.Id
                                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                                    LEFT JOIN MST.MaterialMasterArticle AS ART ON PAD.ArticleId = ART.Id
                                    LEFT JOIN HKP.Characteristics AS FC ON PAD.FirstCharacteristicsId = FC.Id
                                    LEFT JOIN HKP.Characteristics AS SC ON PAD.SecondCharacteristicsId = SC.Id
                                    LEFT JOIN HKP.Characteristics AS TC ON PAD.ThirdCharacteristicsId = TC.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON PAD.FirstCharacteristicsValueId = FCV.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON PAD.SecondCharacteristicsValueId = SCV.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON PAD.ThirdCharacteristicsValueId = TCV.Id
                                    LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON PAD.TransactionUoMId=TUoM.Id
                                    LEFT JOIN[TRN].[PurchaseOrder] AS IR ON PAD.POId= IR.Id
                                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId= CU.Id
                                    WHERE PAD.PurchaseDocAcceptanceId='" + Id + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetPurchaseDocAcceptanceTax(string Id)
        {
            return Json(_purchaseDocumentAcceptance.GetPurchaseDocAcceptanceTax(Id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPurchaseDocAcceptanceServiceTax(string Id)
        {
            return Json(_purchaseDocumentAcceptance.GetPurchaseDocAcceptanceServiceTax(Id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteLineItem(string id, string POID, string PODetailsID, decimal Qty)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _purchaseDocumentAcceptance.Delete(id, POID, PODetailsID, Qty);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [Authorize, HttpGet]
        public JsonResult LCDetails(string LCID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.LCDetails(identity.PlantId, LCID), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult SavedPOList(string AcceptanceID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Sql = @"SELECT
                            PDAPOMap.Id PDAPOMapId
                            ,PDAPOMap.PurchaseDocAcceptanceId
                            ,PDAPOMap.POId Id
                            , REPLACE(CONVERT(CHAR(11), PO.DocDate, 106),' ','-') AS DocDate
                            , REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate
                            , REPLACE(CONVERT(CHAR(11), PO.DocRefNo, 106),' ','-') AS DocRefNo
                            , REPLACE(CONVERT(CHAR(11), PO.DocRefNo, 106),' ','-') AS DocRefNo
                            --,PO.Id
                            ,P.UserName PartyName
                            ,PDAPOMap.AddedBy
                            ,PDAPOMap.AddedDate
                            ,PDAPOMap.AddedFromIP
                            ,PDAPOMap.UpdatedBy
                            ,PDAPOMap.UpdatedDate
                            ,PDAPOMap.UpdatedFromIP
                            FROM TRN.PurchaseDocAcceptancePOMap PDAPOMap
                            LEFT JOIN [TRN].[PurchaseDocAcceptance] PDAcc ON PDAPOMap.PurchaseDocAcceptanceId=PDAcc.Id
                            LEFT JOIN [TRN].[PurchaseOrder] PO ON PO.Id = PDAPOMap.POId
                            left join hkp.Party p On p.Id=PO.PartyId
                    Where PDAcc.Id='" + AcceptanceID + "'";
            return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteACPOmapTabledata(string id, string POID, string PODetailsID, string Qty)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _purchaseDocumentAcceptance.DeleteACPOmapTabledata(id, POID, PODetailsID, Qty);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteCharge(string id)
        {
            DeleteChargeData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        public void DeleteChargeData(string Id)
        {
            string strSQL, strTSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strTSQL = "DELETE FROM [TRN].[PurchaseDocAcceptanceTax] Where PurchaseDocAcceptanceChargesId='" + Id + "'";
                strSQL = "DELETE FROM TRN.PurchaseDocAcceptanceCharges WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strTSQL, true, "1");
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

        [HttpPost, Authorize]
        public ActionResult DeleteServiceCharge(string id)
        {
            DeleteServiceChargesData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteServiceChargesData(string Id)
        {
            string strSQL, strTSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strTSQL = "DELETE FROM [TRN].[PurchaseDocAcceptanceTax] Where PurchaseDocAcceptanceServiceId='" + Id + "'";
                strSQL = "DELETE FROM TRN.PurchaseDocAcceptanceService WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strTSQL, true, "1");
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

        [HttpPost, Authorize]
        public JsonResult DeleteTax(string id)
        {
            DeleteTaxData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteTaxData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [TRN].[PurchaseDocAcceptanceTax] WHERE Id='" + Id + "'";
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

        #region Document Acceptance Post
        [Authorize, HttpGet]
        public JsonResult GetAcceptanceNonPostedList()
        {
            AccountsDocAcceptanceService accountsDocAcceptanceService = new AccountsDocAcceptanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsDocAcceptanceService.GetAcceptanceNonPostedList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAcceptancePostedList()
        {
            AccountsDocAcceptanceService accountsDocAcceptanceService = new AccountsDocAcceptanceService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsDocAcceptanceService.GetAcceptancePostedList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAcceptanceChargesNonPostedList()
        {
            AccountsDocAcceptanceService accountsDocAcceptanceService = new AccountsDocAcceptanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsDocAcceptanceService.GetAcceptanceChargesNonPostedList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAcceptanceChargesPostedList()
        {
            AccountsDocAcceptanceService accountsDocAcceptanceService = new AccountsDocAcceptanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsDocAcceptanceService.GetAcceptanceChargesPostedList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAcceptancePOServiceNonPostedList()
        {
            AccountsDocAcceptanceService accountsDocAcceptanceService = new AccountsDocAcceptanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsDocAcceptanceService.GetAcceptancePOServiceNonPostedList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAcceptancePOServicePostedList()
        {
            AccountsDocAcceptanceService accountsDocAcceptanceService = new AccountsDocAcceptanceService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(accountsDocAcceptanceService.GetAcceptancePOServicePostedList(identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        [Authorize, HttpGet]
        public JsonResult GetAcceptanceDetailForPost(string Id, string PoType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.GetAcceptanceDetailForPost(identity.CompanyId, identity.PlantId, Id, PoType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetGRNAcceptanceDetailForPost(string PurchaseDocAcceptanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseDocumentAcceptance.GetGRNAcceptanceDetailForPost(PurchaseDocAcceptanceId,identity.CompanyId,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DocumentAcceptancePost(PurchaseDocAcceptanceViewModel voucherRows
            , IEnumerable<PurchaseDocAcceptanceDetailViewModel> docAcceptanceDetails, IEnumerable<PurchaseDocAcceptanceDetailViewModel> rowDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (voucherRows != null)
            {
                var voucherVM = new VoucherViewModel
                {
                    CompanyGroupId = identity.CompanyGroupId,
                    CompanyId = identity.CompanyId,
                    Id = voucherRows.Id,
                    CurrencyId = voucherRows.CurrencyId,
                    ToCurrencyRate = voucherRows.ToCurrencyRate,
                    PlantId = identity.PlantId,
                    VoucherDate = DateTime.Now,
                    SourceType = SourceType.PurchaseDocAcceptance.ToString(),
                    VoucherTypeId = voucherRows.VoucherTypeId,
                    Narration = voucherRows.Remarks,
                    BaseOnDueDate = voucherRows.AcceptanceDate,
                    BaseNoOfDays = voucherRows.Tenure,
                    PostingDate = voucherRows.AcceptanceDate,
                    DocDate = voucherRows.AcceptanceDate,
                    Amount = voucherRows.AcceptanceAmount,
                    MatureDate = voucherRows.DueDate
                };

                foreach (var item in docAcceptanceDetails)
                {
                    if (item.GLGeneralInfoId == null && item.BudgetMasterId == null && item.ActivityId == null)
                        throw new CustomException("Payable GL not found in Material Group of " + item.MaterialGroupMasterName);
                    if (item.ClearingAccountGLId == null && item.ClearingAccountBudgetMasterId == null && item.ClearingAccountActivityId == null)
                        throw new CustomException("Inventory in Trnasit GL not found in Material Group of " + item.MaterialGroupMasterName);
                }
                _inventoryPayableService.PostDocumentAcceptance(voucherVM, docAcceptanceDetails, rowDetails, voucherRows.IsNonCreditable);

            }
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public ActionResult DocumentAcceptanceVoucher(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _invoiceReportService.DocumentAcceptanceVoucher(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }


        [Authorize, HttpGet]
        public JsonResult GetAcceptanceServiceListForPost(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_purchaseDocumentAcceptance.GetAcceptanceServiceListForPost(identity.PlantId, Id), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [Authorize, HttpGet]
        public JsonResult GetAcceptanceServiceDeailsListForPost(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_purchaseDocumentAcceptance.GetAcceptanceServiceDeailsListForPost(identity.PlantId, Id), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
        [HttpPost]
        public JsonResult DocumentAcceptanceChargesPost(VoucherViewModel voucherRow, IEnumerable<PurchaseDocAcceptanceViewModel> voucherRows
            , IEnumerable<PurchaseDocAcceptanceChargesViewModel> AcceptancechargesList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (voucherRow.VoucherTypeId == null)
                throw new CustomException("LC Charges Voucher Type not found.");
            foreach (var item in AcceptancechargesList)
            {
                if (item.GLGeneralInfoId == null && item.BudgetMasterId == null && item.ActivityId == null)
                    throw new CustomException("Bank GL not found in Bank Master of " + item.OpeningBankMaster);
                if (item.ExpensesGLId == null && item.ExpensesBudgetMasterId == null && item.ExpensesActivityId == null)
                    throw new CustomException("Services  GL not found in Acceptance Services  of " + item.ChargeName);
            }
            if (voucherRows != null)
            {
                var voucherVM = new VoucherViewModel
                {
                    CompanyGroupId = identity.CompanyGroupId,
                    CompanyId = identity.CompanyId,
                    PlantId = identity.PlantId,
                    VoucherDate = DateTime.Now,
                    ToCurrencyRate= voucherRow.ToCurrencyRate,
                    SourceType = SourceType.PurchaseDocAcceptance.ToString(),
                    VoucherTypeId = voucherRow.VoucherTypeId,
                    AddedBy=identity.Name,
                    AddedDate=DateTime.Now,
                    AddedFromIP=identity.IPAddress
                };
                _inventoryPayableService.PostDocumentAcceptanceService(voucherVM, voucherRows, AcceptancechargesList, taxDetailVMList);
            }
            return Json(new { Message = AplosMessage.Posted });
        }
        #endregion

        private Dictionary<string, object> GetDetailId(string id)
        {
            var cmdText = @"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceDetail]  WHERE PurchaseDocAcceptanceId ='" + id + "'";
            return _sqlRepository.GetData(cmdText);
        }

        [HttpPost]
        public JsonResult XCreateGRNAcceptance(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail)
        {
            string acptDetailId = "";
            DataSet dsMaster, detailDestination, taxDestination, servicetaxDestination, dsdetailGRN, dsdetailPO;
            DataView dvdetailDestination, dvdetailGRN, dvdetailPO = null;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

            string inventoryReceiveId = "";
            string PoId = "";
            foreach (var item in PurchaseDocAcceptanceDetail)
            {
                if (string.IsNullOrEmpty(inventoryReceiveId))
                {
                    inventoryReceiveId += "''," + item.Id;
                }
                else
                {
                    inventoryReceiveId += "," + item.Id;
                }
                if (string.IsNullOrEmpty(PoId))
                {
                    PoId += "''," + item.POId;
                }
                else
                {
                    PoId += "," + item.POId;
                }
            }

            SaveData(entity, out dsMaster, out string masterId);
            entity.Id = masterId;

            con.OpenDataSetThroughAdapter(@"SELECT * FROM TRN.PurchaseDocAcceptanceDetail WHERE PODetailId IN (Select  id From TRN.InventoryReceiveDetail Where InventoryReceiveId In (" + inventoryReceiveId + "))", out detailDestination, false, "1");

            con.OpenDataSetThroughAdapter(@"SELECT * FROM TRN.[InventoryReceiveDetail] WHERE InventoryReceiveId IN (" + inventoryReceiveId + ")", out dsdetailGRN, false, "1");
            con.OpenDataSetThroughAdapter(@"SELECT * FROM TRN.[PurchaseOrderDetail] WHERE InventoryReceiveId IN (" + PoId + ")", out dsdetailPO, false, "1");

            //con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[PurchaseDocAcceptanceTax] WHERE PurchaseDocAcceptanceDetailId IN (SELECT Id FROM TRN.PurchaseDocAcceptanceDetail Where InventoryReceiveDetailId IN (Select  id From TRN.InventoryReceiveDetail Where InventoryReceiveId In (" + inventoryReceiveId + ")))", out taxDestination, false, "1");
            //con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[PurchaseDocAcceptanceService] WHERE PurchaseDocAcceptanceId='" + masterId + "'", out servicetaxDestination, false, "1");

            DataTable acceptanceDetailSource = _sqlRepository.GetDataTable(@"SELECT IRD.*,IM.MaterialMasterId,IM.ArticleId,IM.FirstCharacteristicsId,IM.FirstCharacteristicsValueId
                                                                    ,IM.SecondCharacteristicsId,IM.SecondCharacteristicsValueId,IM.ThirdCharacteristicsId,IM.ThirdCharacteristicsValueId,IR.ToCurrencyRate
                                                                    FROM [TRN].[InventoryMaterial] IM
                                                                    JOIN[TRN].[InventoryReceiveDetail] IRD ON IRD.InventoryMaterialId=IM.Id
                                                                    JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId Where IR.Id IN(" + inventoryReceiveId + ")");

            //DataTable inventoryTaxSource = _sqlRepository.GetDataTable(@"Select * from [TRN].[InventoryReceiveTax] Where InventoryReceiveDetailId IN (SELECT Id FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId IN (" + inventoryReceiveId + @") AND 
            //InventoryReceiveDetailId NOT IN (Select D.InventoryReceiveDetailId from  [TRN].[PurchaseDocAcceptanceTax] T
            //JOIN [TRN].[PurchaseDocAcceptanceDetail] D ON D.Id=T.PurchaseDocAcceptanceDetailId 
            //Where ISNULL(D.InventoryReceiveDetailId,'')<>''))");
            //DataTable serviceTaxSource = _sqlRepository.GetDataTable("Select * from [TRN].[InventoryService] Where  InventoryReceiveId IN (" + inventoryReceiveId + ")");

            // DataRow drDestination = detail.Tables[0].NewRow();
            var IdCount = 0;
            var Iddata = GetDetailId(entity.Id);
            IdCount = Convert.ToInt32(Iddata["Id"]);
            for (int K = 0; K < acceptanceDetailSource.Rows.Count; K++)
            {
                dvdetailDestination = new DataView(detailDestination.Tables[0]);
                dvdetailDestination.RowFilter = "PODetailId='" + acceptanceDetailSource.Rows[K]["Id"].ToString() + "'";
                if (dvdetailDestination.Count == 0)
                {
                    IdCount++;

                    DataRow drDetail = detailDestination.Tables[0].NewRow();
                    CopyRow(acceptanceDetailSource.Rows[K], ref drDetail);
                    drDetail["Id"] = masterId + "-" + IdCount;
                    acptDetailId = masterId + "-" + IdCount;
                    drDetail["PurchasedocAcceptanceId"] = masterId;


                    drDetail["MaterialMasterId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["MaterialMasterId"].ToString());
                    drDetail["ArticleId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["ArticleId"].ToString());
                    drDetail["FirstCharacteristicsId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["FirstCharacteristicsId"].ToString());
                    drDetail["FirstCharacteristicsValueId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["FirstCharacteristicsValueId"].ToString());
                    drDetail["SecondCharacteristicsId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["SecondCharacteristicsId"].ToString());
                    drDetail["SecondCharacteristicsValueId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["SecondCharacteristicsValueId"].ToString());
                    drDetail["ThirdCharacteristicsId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["ThirdCharacteristicsId"].ToString());
                    drDetail["ThirdCharacteristicsValueId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["ThirdCharacteristicsValueId"].ToString());
                    drDetail["AcceptanceRate"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["ToCurrencyRate"].ToString());

                    //drDetail["PODetailId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["Id"].ToString());
                    drDetail["PODetailId"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["PODetailsId"].ToString());
                    drDetail["TaxAmount"] = bplib.clsWebLib.RetValidLen(acceptanceDetailSource.Rows[K]["TotalTaxAmount"].ToString());

                    detailDestination.Tables[0].Rows.Add(drDetail);

                    // dvBp.RowFilter = " TaxPolicyID='" + item.TaxPolicyID + "' and plantID='" + item.PlantId + "' ";

                    //inventoryTaxSource.DefaultView.RowFilter = "InventoryReceiveDetailId='" + acceptanceDetailSource.Rows[K]["Id"].ToString() + "'";
                    //for (int i = 0; i < inventoryTaxSource.DefaultView.Count; i++)
                    //{

                    //    DataRow drTax = taxDestination.Tables[0].NewRow();
                    //    CopyRow(inventoryTaxSource.DefaultView[i].Row, ref drTax);
                    //    drTax["Id"] = masterId + "-" + drDetail["Id"] + (i + 1);
                    //    drTax["PurchasedocAcceptanceId"] = masterId;
                    //    drTax["PurchaseDocAcceptanceDetailId"] = drDetail["Id"];

                    //    taxDestination.Tables[0].Rows.Add(drTax);
                    //}

                    ////serviceTax.DefaultView.RowFilter = "InventoryReceiveId='" + "Id" + "'";
                    //for (int i = 0; i < serviceTaxSource.DefaultView.Count; i++)
                    //{

                    //    DataRow drserviceTax = servicetaxDestination.Tables[0].NewRow();
                    //    CopyRow(inventoryTaxSource.DefaultView[i].Row, ref drserviceTax);
                    //    drserviceTax["Id"] = masterId + "-" + drDetail["Id"] + (i + 1);
                    //    drserviceTax["PurchasedocAcceptanceId"] = masterId;
                    //    drserviceTax["PurchaseDocAcceptanceDetailId"] = drDetail["Id"];

                    //    servicetaxDestination.Tables[0].Rows.Add(drserviceTax);
                    //}

                }
                else
                {

                }

                dvdetailGRN = new DataView(dsdetailGRN.Tables[0]);
                dvdetailGRN.RowFilter = "Id='" + acceptanceDetailSource.Rows[K]["Id"].ToString() + "' AND InventoryReceiveId='" + acceptanceDetailSource.Rows[K]["InventoryReceiveId"].ToString() + "'";
                if (dvdetailGRN.Count > 0)
                {
                    DataRow drGRN = dvdetailGRN[0].Row;
                    drGRN.BeginEdit();

                    drGRN["PurchaseDocumentAcceptanceId"] = masterId;
                    drGRN["PurchaseDocumentAcceptanceDetailId"] = acptDetailId;

                    drGRN.EndEdit();
                }

                dvdetailPO = new DataView(dsdetailPO.Tables[0]);
                dvdetailPO.RowFilter = "Id='" + acceptanceDetailSource.Rows[K]["PODetailsId"].ToString() + "'";
                if (dvdetailPO.Count > 0)
                {
                    DataRow drPO = dvdetailPO[0].Row;
                    drPO.BeginEdit();

                    drPO["AcceptanceRcvQty"] = acceptanceDetailSource.Rows[K]["TransactionQty"].ToString();
                    drPO.EndEdit();
                }

            }

            SaveGRNAcceptanceMapData(PurchaseDocAcceptanceDetail, masterId, out DataSet dsGRNAcceptanceMap);

            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster, detailDestination, dsGRNAcceptanceMap, dsdetailGRN, dsdetailPO/*, taxDestination,  servicetaxDestination*/);

            return Json(new { entity, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult CreateGRNAcceptance(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail, List<Dictionary<string, object>> PurchaseDocAcceptanceDetails)
        {
            ConnectionManager.DAL.ConManager objCon;
            objCon = new ConnectionManager.DAL.ConManager("1");
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string inventoryReceiveId = "";
                string PoId = "";
                foreach (var item in PurchaseDocAcceptanceDetail)
                {
                    if (string.IsNullOrEmpty(inventoryReceiveId))
                    {
                        inventoryReceiveId += "''," + item.Id;
                    }
                    else
                    {
                        inventoryReceiveId += "," + item.Id;
                    }
                    if (string.IsNullOrEmpty(PoId))
                    {
                        PoId += "''," + item.POId;
                    }
                    else
                    {
                        PoId += "," + item.POId;
                    }
                }

                DataSet dsMaster, dsDetail, dsGRNService, dsdetailPO, dsAcptService, dsSerId;
                DataView dvAcptService, dvdetailPO = null;
                DataRow drAcptService = null;

                string acptDetailId = null;
                SaveData(entity, out dsMaster, out string masterId);
                entity.Id = masterId;
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[PurchaseDocAcceptanceDetail] Where PurchaseDocAcceptanceId='" + masterId + "'", out dsDetail, false, "1");
                objCon.OpenDataSetThroughAdapter(@"SELECT * FROM TRN.[PurchaseOrderDetail] WHERE InventoryReceiveId IN (" + PoId + ")", out dsdetailPO, false, "1");
                objCon.OpenDataSetThroughAdapter(@"SELECT * FROM TRN.InventoryService WHERE InventoryReceiveId IN (" + inventoryReceiveId + ")", out dsGRNService, false, "1");
                objCon.OpenDataSetThroughAdapter(@"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseDocAcceptanceService] WHERE PurchaseDocAcceptanceId='" + masterId + "'", out dsSerId, false, "1");

                objCon.OpenDataSetThroughAdapter(@"SELECT * FROM TRN.PurchaseDocAcceptanceService WHERE PurchaseDocAcceptanceId='" + masterId + "'", out dsAcptService, false, "1");

                int servicecurrentId = 0;
                if (dsSerId.Tables[0].Rows.Count>0)
                {
                    servicecurrentId = Convert.ToInt32(dsSerId.Tables[0].Rows[0]["Id"].ToString());
                }

                int IdCount = 0;
                if (PurchaseDocAcceptanceDetails != null)
                {
                    foreach (var item in PurchaseDocAcceptanceDetails)
                    {
                        IdCount++;
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = masterId + "-" + IdCount;
                            acptDetailId = masterId + "-" + IdCount;
                            item["PurchaseDocAcceptanceId"] = masterId;
                            item["AcceptanceRate"] = 0.0;
                            item["MaterialTranAmount"] = item["TotalMaterialTranAmount"];
                            AddNewRow(dsDetail.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }

                        //dvdetailGRN = new DataView(dsdetailGRN.Tables[0]);
                        //dvdetailGRN.RowFilter = "Id='" + item["InventoryReceiveDetailId"] + "'";
                        //if (dvdetailGRN.Count > 0)
                        //{
                        //    DataRow drGRN = dvdetailGRN[0].Row;
                        //    drGRN.BeginEdit();

                        //    drGRN["PurchaseDocumentAcceptanceId"] = masterId;
                        //    drGRN["PurchaseDocumentAcceptanceDetailId"] = acptDetailId;

                        //    drGRN.EndEdit();
                        //}

                        dvdetailPO = new DataView(dsdetailPO.Tables[0]);
                        dvdetailPO.RowFilter = "Id='" + item["PODetailId"].ToString() + "'";
                        if (dvdetailPO.Count > 0)
                        {
                            DataRow drPO = dvdetailPO[0].Row;
                            drPO.BeginEdit();

                            drPO["AcceptanceRcvQty"] = Convert.ToDecimal(drPO["AcceptanceRcvQty"].ToString()) + Convert.ToDecimal(item["TransactionQty"]);
                            drPO.EndEdit();
                        }
                    }
                }

                SaveGRNAcceptanceMapData(PurchaseDocAcceptanceDetail, masterId, out DataSet dsGRNAcceptanceMap);


                if (dsGRNService.Tables[0].Rows.Count>0)
                {
                    for (int i = 0; i < dsGRNService.Tables[0].Rows.Count; i++)
                    {
                        dvAcptService = new DataView(dsAcptService.Tables[0]);

                        dvAcptService.RowFilter = "PurchaseDocAcceptanceId='" + masterId + "'";

                        if (dvAcptService.Count == 0)
                        {
                            servicecurrentId++;

                            drAcptService = dsAcptService.Tables[0].NewRow();
                            drAcptService["Id"] = MakePK(entity.Id + 2, servicecurrentId, 2);
                            drAcptService["PurchaseDocAcceptanceId"] = masterId;
                            drAcptService["ServiceMasterId"] = dsGRNService.Tables[0].Rows[i]["ServiceMasterId"].ToString();
                            drAcptService["Amount"] = dsGRNService.Tables[0].Rows[i]["Amount"].ToString();
                            drAcptService["TotalTaxAmount"] = dsGRNService.Tables[0].Rows[i]["TotalTaxAmount"].ToString();
                            drAcptService["State"] = "GRN";
                            drAcptService["BankAmount"] = 0;
                            drAcptService["Rate"] = 0;

                            drAcptService["AddedBy"] = identity.Name;
                            drAcptService["AddedDate"] = DateTime.Now;
                            drAcptService["AddedFromIP"] = identity.IPAddress;

                            dsAcptService.Tables[0].Rows.Add(drAcptService);
                        }
                    } 
                }



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail, dsGRNAcceptanceMap, dsdetailPO, dsAcptService);
                return Json(new { entity, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static string MakePK(string masterId, int currentId, int padLeft)
        {
            return masterId + currentId.ToString().PadLeft(padLeft, '0');
        }

        [HttpPost]
        public JsonResult UpdateGRNAcceptance(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail, List<Dictionary<string, object>> PurchaseDocAcceptanceDetails)
        {
            ConnectionManager.DAL.ConManager objCon;
            objCon = new ConnectionManager.DAL.ConManager("1");
            try
            {
                string inventoryReceiveId = "";
                string PoId = "";
                foreach (var item in PurchaseDocAcceptanceDetail)
                {
                    if (string.IsNullOrEmpty(inventoryReceiveId))
                    {
                        inventoryReceiveId += "''," + item.Id;
                    }
                    else
                    {
                        inventoryReceiveId += "," + item.Id;
                    }
                    if (string.IsNullOrEmpty(PoId))
                    {
                        PoId += "''," + item.POId;
                    }
                    else
                    {
                        PoId += "," + item.POId;
                    }
                }

                DataSet dsMaster, dsDetail, dsdetailGRN, dsdetailPO;
                DataView dvdetailGRN, dvdetailPO = null;
                string acptDetailId = null;
                SaveData(entity, out dsMaster, out string masterId);
                entity.Id = masterId;
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[PurchaseDocAcceptanceDetail] Where PurchaseDocAcceptanceId='" + masterId + "'", out dsDetail, false, "1");

                //objCon.OpenDataSetThroughAdapter(@"SELECT * FROM TRN.[InventoryReceiveDetail] WHERE InventoryReceiveId IN (" + inventoryReceiveId + ")", out dsdetailGRN, false, "1");
                objCon.OpenDataSetThroughAdapter(@"SELECT * FROM TRN.[PurchaseOrderDetail] WHERE InventoryReceiveId IN (" + PoId + ")", out dsdetailPO, false, "1");

                int IdCount = 0;
                if (PurchaseDocAcceptanceDetails != null)
                {
                    foreach (var item in PurchaseDocAcceptanceDetails)
                    {
                        IdCount++;
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = masterId + "-" + IdCount;
                            acptDetailId = masterId + "-" + IdCount;
                            item["PurchaseDocAcceptanceId"] = masterId;
                            item["AcceptanceRate"] = 0.0;
                            item["MaterialTranAmount"] = item["TotalMaterialTranAmount"];
                            AddNewRow(dsDetail.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }

                        //dvdetailGRN = new DataView(dsdetailGRN.Tables[0]);
                        //dvdetailGRN.RowFilter = "Id='" + item["InventoryReceiveDetailId"] + "'";
                        //if (dvdetailGRN.Count > 0)
                        //{
                        //    DataRow drGRN = dvdetailGRN[0].Row;
                        //    drGRN.BeginEdit();

                        //    drGRN["PurchaseDocumentAcceptanceId"] = masterId;
                        //    drGRN["PurchaseDocumentAcceptanceDetailId"] = acptDetailId;

                        //    drGRN.EndEdit();
                        //}

                        dvdetailPO = new DataView(dsdetailPO.Tables[0]);
                        dvdetailPO.RowFilter = "Id='" + item["PODetailId"].ToString() + "'";
                        if (dvdetailPO.Count > 0)
                        {
                            DataRow drPO = dvdetailPO[0].Row;
                            drPO.BeginEdit();

                            drPO["AcceptanceRcvQty"] = Convert.ToDecimal(drPO["AcceptanceRcvQty"].ToString()) + Convert.ToDecimal(item["TransactionQty"]);
                            drPO.EndEdit();
                        }
                    }
                }

                SaveGRNAcceptanceMapData(PurchaseDocAcceptanceDetail, masterId, out DataSet dsGRNAcceptanceMap);


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail, dsGRNAcceptanceMap, dsdetailPO);
                return Json(new { entity, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
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

        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception)
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
                catch (Exception)
                {
                }
            }

        }
        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseDocAcceptance), out sID);
            return sID;
        }

        private void SaveData(PurchaseDocAcceptance data, out DataSet dsMaster, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            dsMaster = null;
            string id = string.Empty;
            try
            {
                string sql = "SELECT * FROM [TRN].[PurchaseDocAcceptance] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetPK();

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;
                    dr["AcceptanceNo"] = data.AcceptanceNo;
                    dr["AcceptanceDate"] = data.AcceptanceDate;
                    dr["Remarks"] = data.Remarks;
                    dr["PurchaseLCId"] = data.PurchaseLCId;
                    dr["AcceptancePaymentSource"] = data.AcceptancePaymentSource;
                    dr["DueDate"] = data.DueDate;
                    dr["InvoiceDate"] = data.InvoiceDate;
                    dr["VoucherId"] = data.VoucherId;
                    dr["PartyId"] = data.PartyId;
                    dr["PartyPlantId"] = data.PartyPlantId;
                    dr["AcceptanceRate"] = data.AcceptanceRate;
                    dr["IsNonCreditable"] = data.IsNonCreditable;
                    dr["InvoiceNo"] = data.InvoiceNo;
                    dr["PrePurchaseInvoiceId"] = data.PrePurchaseInvoiceId;
                    dr["EntryDate"] = data.EntryDate;
                    dr["AcceptanceAmount"] = data.AcceptanceAmount;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);

                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;
                    dr["AcceptanceNo"] = data.AcceptanceNo;
                    dr["AcceptanceDate"] = data.AcceptanceDate;
                    dr["Remarks"] = data.Remarks;
                    dr["PurchaseLCId"] = data.PurchaseLCId;
                    dr["AcceptancePaymentSource"] = data.AcceptancePaymentSource;
                    dr["DueDate"] = data.DueDate;
                    dr["InvoiceDate"] = data.InvoiceDate;
                    dr["VoucherId"] = data.VoucherId;
                    dr["PartyId"] = data.PartyId;
                    dr["PartyPlantId"] = data.PartyPlantId;
                    dr["AcceptanceRate"] = data.AcceptanceRate;
                    dr["IsNonCreditable"] = data.IsNonCreditable;
                    dr["InvoiceNo"] = data.InvoiceNo;
                    dr["PrePurchaseInvoiceId"] = data.PrePurchaseInvoiceId;
                    dr["EntryDate"] = data.EntryDate;
                    dr["AcceptanceAmount"] = data.AcceptanceAmount;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void SaveGRNAcceptanceMapData(IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail, string masterId, out DataSet dsGRNAcceptanceMap)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                dsGRNAcceptanceMap = null;



                DataTable dtGRNAcceptanceMap = null;
                DataView dvGRNAcceptanceMap = null;
                DataRow drGRNAcceptanceMap = null;
                string BPId = string.Empty;
                string sql = "SELECT * FROM TRN.GRNAcceptanceMap ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsGRNAcceptanceMap, false, "1");

                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();

                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(GRNAcceptanceMap), out string sID);
                int count = 0;

                objCon.OpenDataSetThroughAdapter(sql, out dsGRNAcceptanceMap, false, "1");

                foreach (var item in PurchaseDocAcceptanceDetail)
                {
                    dvGRNAcceptanceMap = new DataView(dsGRNAcceptanceMap.Tables[0]);

                    dvGRNAcceptanceMap.RowFilter = " GRNId='" + item.Id + "' and PurchaseDocumentAcceptanceId='" + masterId + "' ";

                    if (dvGRNAcceptanceMap.Count == 0)
                    {
                        count++;
                        string pk = "A" + sID + "_" + count;
                        drGRNAcceptanceMap = dsGRNAcceptanceMap.Tables[0].NewRow();
                        drGRNAcceptanceMap["Id"] = pk;
                        drGRNAcceptanceMap["GRNId"] = item.Id;
                        drGRNAcceptanceMap["PurchaseDocumentAcceptanceId"] = masterId;

                        drGRNAcceptanceMap["AddedBy"] = identity.Name;
                        drGRNAcceptanceMap["AddedDate"] = DateTime.Now;
                        drGRNAcceptanceMap["AddedFromIP"] = identity.IPAddress;

                        dsGRNAcceptanceMap.Tables[0].Rows.Add(drGRNAcceptanceMap);
                    }
                }

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetOtherAcptQtyValue(string POId, string PurchaseDocAcceptanceId)
        {
            return Json(_purchaseDocumentAcceptance.GetOtherAcptQtyValue(POId, PurchaseDocAcceptanceId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL, strDSQL, strPOSQL, strGRNSQL, strTSQL, strSVSQL, strCSQL, strGRNACPTSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            DataSet dsPO = null;
            DataSet dsPAD = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            decimal Tqty = 0;
            try
            {


                strGRNACPTSQL = @"update TRN.[InventoryReceiveDetail] set PurchaseDocumentAcceptanceId=NULL, PurchaseDocumentAcceptanceDetailId=NULL  where PurchaseDocumentAcceptanceId='" + Id + "'";
                strGRNSQL = @"delete from TRN.GRNAcceptanceMap Where PurchaseDocumentAcceptanceId ='" + Id + "'";
                strPOSQL = @"delete from TRN.PurchaseDocAcceptancePOMap Where PurchaseDocAcceptanceId ='" + Id + "'";
                strTSQL = @"delete from TRN.[PurchaseDocAcceptanceTax] Where PurchaseDocAcceptanceId ='" + Id + "'";
                strDSQL = @"delete from TRN.PurchaseDocAcceptanceDetail Where PurchaseDocAcceptanceId ='" + Id + "'";
                strSVSQL = @"delete from TRN.[PurchaseDocAcceptanceService] Where PurchaseDocAcceptanceId ='" + Id + "'";
                strCSQL = @"delete from TRN.[PurchaseDocAcceptanceCharges] Where PurchaseDocAcceptanceId ='" + Id + "'";
                strSQL = @"delete from TRN.PurchaseDocAcceptance  Where Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();


                objCon.OpenDataSetThroughAdapter(@"Select Id,InventoryReceiveId,AcceptanceRcvQty,AcceptanceRcvStatusQty from TRN.PurchaseOrderDetail 
                    Where Id IN (Select  PODetailId FROM TRN.PurchaseDocAcceptanceDetail Where PurchaseDocAcceptanceId = '" + Id + "')", out dsPO, false, "1");

                if (dsPO.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsPO.Tables[0].Rows.Count; i++)
                    {
                        objCon.OpenDataSetThroughAdapter(@"Select TransactionQty  FROM TRN.PurchaseDocAcceptanceDetail Where PODetailId='" + dsPO.Tables[0].Rows[i]["Id"].ToString() + "'", out dsPAD, false, "1");

                        if (dsPAD.Tables[0].Rows.Count > 0)
                        {
                            Tqty = Convert.ToDecimal(dsPAD.Tables[0].Rows[0]["TransactionQty"].ToString());
                        }

                        DataView dv = new DataView(dsPO.Tables[0]);
                        dv.RowFilter = "Id='" + dsPO.Tables[0].Rows[i]["Id"].ToString() + "'";

                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;

                            drmo.BeginEdit();


                            if (!string.IsNullOrEmpty(dsPO.Tables[0].Rows[i]["AcceptanceRcvQty"].ToString()))
                            {
                                drmo["AcceptanceRcvQty"] = Convert.ToDecimal(dsPO.Tables[0].Rows[i]["AcceptanceRcvQty"].ToString()) - Tqty;
                            }
                            else
                            {
                                drmo["AcceptanceRcvQty"] = Convert.ToDecimal(dsPO.Tables[0].Rows[i]["AcceptanceRcvQty"].ToString());
                            }
                            drmo["AcceptanceRcvStatusQty"] = 0;

                            drmo.EndEdit();

                        }
                    }
                }

                objCon.ExecuteNonQueryWrapper(strGRNACPTSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strGRNSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strPOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strTSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strDSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSVSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsPO);

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

        #region ServicePO Acceptance

        public IEnumerable<object> getServicePOAckTax(string Id)
        {
            try
            {
                var _sql = @"SELECT SAT.Id, SAT.ServicePODetailId, SAT.TaxCategoryId, SAT.HSNCodeId, SAT.Percentage, SAT.TaxAmount 
                            FROM trn.PurchaseDocAcceptanceTax SAT
                            Left JOIN MST.TaxCategory TC ON TC.Id= SAT.TaxCategoryId
                            WHERE SAT.PurchaseDocAcceptanceId='" + Id + "'";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceListByServicePO(string servicepoid)
        {

            string paramter = "";
            if (servicepoid != "")
            {
                if (paramter == "")
                    paramter += "A.ServicePOMasterId in(" + servicepoid + ")";
                else
                    paramter += " AND A.ServicePOMasterId in(" + servicepoid + ")";
            }
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"Select 
                a.Id ServicePODetailId,a.ServicePOMasterId
                ,b.Id ServiceMasterId
                ,b.UserName ServiceMasterName
                , a.Amount 
                ,c.TaxAmount TotalTaxAmount
                ,0 [check]
                ,d.IsNonCreditable
                ,TotalAmount=CASE WHEN d.IsNonCreditable=1 then (a.Amount + c.TaxAmount) Else a.Amount  END
				,a.Qty
				,a.Rate
				,UOM.Username UoM,null CurrentQty,A.TransactionUoMId,Mapdata.Qty OtherReceived,Balance=Isnull(a.Qty,0)-ISNULL(Mapdata.Qty,0)
                FROM TRN.ServicePODetail a
                LEFT JOIN TRN.ServicePOMaster d on d.id=a.ServicePOMasterId
                Left JOIN HKP.ServiceMaster b on a.ServiceMasterId=b.id
                LEFT JOIN(SELECT ServicePODetailId,sum(TaxAmount) TaxAmount from trn.ServicePOTax group by ServicePODetailId)c On c.ServicePODetailId=a.id
				LEFT JOIN SCS.UnitOfMeasurement UOM ON A.TransactionUoMId=UOM.Id
				LEFT JOIN(SELECT ServicePODetailId,sum(Qty) Qty from trn.ServivePOAcknowledgementMap group by ServicePODetailId)Mapdata On Mapdata.ServicePODetailId=a.id 
                where " + paramter + @"";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult CreateServicePOAcceptance(PurchaseDocAcceptance entity, IEnumerable<PurchaseDocAcceptanceDetailViewModel> PurchaseDocAcceptanceDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            entity.EntryDate = entity.AcceptanceDate;
            if (PurchaseDocAcceptanceDetail == null)
            {
                throw new CustomException("Please Select details !");
            }

            _purchaseDocumentAcceptance.InsertOrUpdateServicePOAcceptance(entity, PurchaseDocAcceptanceDetail);
            return Json(new { entity, Message = AplosMessage.Success + " Purchase Document Acceptance no <b>" + entity.Id + "</b>" });
        }
        [Authorize, HttpGet]
        public JsonResult GetSavedServicePOList(string acceptanceID)
        {
            try
            {
                var Sql = @"Select 
                PDAD.Id,
                a.Id ServicePODetailId,a.ServicePOMasterId
                ,b.Id ServiceMasterId
                ,b.UserName ServiceMasterName
                , a.Amount 
                ,c.TaxAmount TotalTaxAmount
                ,0 [check]
                ,d.IsNonCreditable
                ,TotalAmount=CASE WHEN d.IsNonCreditable=1 then (a.Amount + c.TaxAmount) Else a.Amount  END
				,a.Qty
				,a.Rate
				,UOM.Username UoM,PDAD.TransactionQty  CurrentQty,A.TransactionUoMId,Mapdata.Qty OtherReceived,Balance=Isnull(a.Qty,0)-ISNULL(PDAD.TransactionQty,0)
                ,PDAD.TransactionQty,PDAD.MaterialTranRate,PDAD.MaterialTranRate TransactionRate,PDAD.MaterialTranAmount,PDAD.TotalMaterialTranAmount,PDAD.AcceptanceRate
				 FROM TRN.PurchaseDocAcceptanceDetail PDAD
                LEFT JOIN trn.ServicePODetail a ON A.Id=PDAD.ServicePODetailId AND A.ServicePOMasterId=PDAD.ServicePOMasterId
                LEFT join trn.ServicePOMaster d on d.id=a.ServicePOMasterId
                Left join hkp.ServiceMaster b on a.ServiceMasterId=b.id
                left join(select ServicePODetailId,sum(TaxAmount) TaxAmount from trn.ServicePOTax group by ServicePODetailId)c On c.ServicePODetailId=a.id
				left join scs.UnitOfMeasurement UOM ON A.TransactionUoMId=UOM.Id
				left join(select ServicePODetailId,sum(Qty) Qty from trn.ServivePOAcknowledgementMap group by ServicePODetailId)Mapdata On Mapdata.ServicePODetailId=a.id 
                where PDAD.PurchaseDocAcceptanceId='" + acceptanceID + "' AND ISNULL(PDAD.ServicePOMasterId,'')<>'' AND ISNULL(PDAD.ServicePODetailId,'')<>''";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [Authorize, HttpPost]
        public JsonResult DeleteServicePOItem(string Id)
        {
            try
            {
                DeleteDeleteServicePOItemData(Id);
                return Json(new { Message = AplosMessage.Deleted });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public void DeleteDeleteServicePOItemData(string Id)
        {
            string strSQL, strDTSQL, strAMQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                //strDTSQL = @"Update TRN.PurchaseDocAcceptancePOMap Set ServicePOMasterId=NULL WHERE PurchasedocAcceptanceDetail='" + Id + "'";
                strDTSQL = @"DELETE FROM TRN.PurchaseDocAcceptanceTax WHERE PurchasedocAcceptanceDetailId='" + Id + "'";
                strSQL = @"DELETE FROM TRN.PurchasedocAcceptanceDetail where Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strDTSQL, true, "1");
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

        #endregion

        [HttpPost]
        public ActionResult DeletePurchaseDocAcceptance(string pdocAccpId, string voucherId)
        {
            _purchaseDocumentAcceptance.DeletePurchaseDocAcceptancePost(pdocAccpId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }





    }


}



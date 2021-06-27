
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Model.Products;
using Library.Service.Helpers;
using Library.Service.Inventory;
using Library.Service.Products;
using Library.Service.Reports;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;  
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Products.Controllers
{
    public class MaterialRequsitionMasterController : Controller
    {
        #region Constructor

        private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IPurchaseOrderDetailService _inventoryDetailService;
        private readonly IPOMaterialService _inventoryMaterialService;
        private readonly IPurchaseOrderServiceService _inventoryService;
        private readonly IInventoryReceiveReportService _inventoryReportService;
        private readonly ISqlRepository _sqlRepository;
       // private readonly IMaterialRequsitionMasterService _materialRequsitionMasterService;

        public MaterialRequsitionMasterController(
            IPurchaseOrderService inventoryReveiveService
            , IPurchaseOrderDetailService inventoryDetailService
            , IPOMaterialService inventoryMaterialService
            , IInventoryReceiveReportService inventoryReportService
            , IPurchaseOrderServiceService inventoryService
            
            , ISqlRepository sqlRepository)
        {
            _inventoryReveiveService = inventoryReveiveService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryService = inventoryService;
            _inventoryReportService = inventoryReportService;
            _sqlRepository = sqlRepository;
            
        }

        #endregion Constructor

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        //[Authorize]
        //public ActionResult Requisition()
        //{
        //    return View();
        //}

        #endregion Aplos

        #region Operations / Methods

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.Query(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPostingList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetPostingList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// using inventory payable
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Authorize, HttpGet]
        public JsonResult GetListForInvPayable(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForInvPayable(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PurchaseOrder entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            entity.IsApproved = false;
            entity.IsClosed=false;
            entity.POType = "PO";
            entity.MasterOrderId = null;  
            //entity.CheckedBy = "";
            entity.AuthorizedBy = null;
            entity.CheckedByStatus = "Pending";
            _inventoryReveiveService.Insert(entity);
            return Json(new { entity, Message = AplosMessage.Success + " PO no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public JsonResult CreateFGMasterOrder(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> Materialentity, IEnumerable<PurchaseOrderTax> taxCategoryList, IEnumerable<InventoryMaterialViewModel> ServiceEntity, IEnumerable<PurchaseOrderTax> ServicetaxCategoryList) 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            entity.IsApproved = false;
            entity.IsClosed = false;
            entity.POType = "PO";
            
           // _inventoryReveiveService.Insert(entity);
            DetailCreateFGMasterOrder(entity, Materialentity, taxCategoryList, ServiceEntity, ServicetaxCategoryList);
            return Json(new { entity, Message = AplosMessage.Success + " PO no <b>" + entity.Id + "</b>" });
        }
        [HttpPost, ChaildAction(ParentActionName = nameof(Create))] 
        public JsonResult DetailCreateFGMasterOrder(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> Materialentity, IEnumerable<PurchaseOrderTax> taxCategoryList, IEnumerable<InventoryMaterialViewModel> ServiceEntity,  IEnumerable<PurchaseOrderTax> ServicetaxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           // Materialentity.CompanyGroupId = identity.CompanyGroupId;
            //Materialentity.CompanyId = identity.CompanyId;
            //Materialentity.PlantId = identity.PlantId;
            _inventoryDetailService.InsertOrUpdateGraphFGForMasterOrder(entity,Materialentity, taxCategoryList,ServiceEntity, ServicetaxCategoryList);
            ServiceChargesCreateFG(ServiceEntity, ServicetaxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreateFG(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            //if(entity != null)            {

            //        if (entity.Amount == 0)
            //            throw new CustomException("Enter Service Amount!");                  

            //}
            _inventoryService.InsertGraphFG(entity, taxCategoryList);
            return Json(new {  Message = AplosMessage.Success });//entity.Id,
        }

        [HttpPost]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult UpdateMaterial(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var ip = identity.IPAddress;
            var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
            var UpdatedBy = identity.Name;
            if (entity != null)
            {
                foreach (var item in entity)
                {
                    if (item.TransactionQty == 0)
                        throw new CustomException("Enter Qty!");
                    if (item.TrnAmount == 0)
                        throw new CustomException("Enter Amount!");
                    //if (item.TransactionQty == 0)
                    //    throw new CustomException("Please Input   Quantity !");
                }
            }
            //entity.CompanyGroupId = identity.CompanyGroupId;
            //entity.CompanyId = identity.CompanyId;
            //entity.PlantId = identity.PlantId;
            _inventoryDetailService.UpdateMaterial(entity, receiveTaxList);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult UpdateServiceAndTax(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var ip = identity.IPAddress;
            var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
            var UpdatedBy = identity.Name;
            if(entity != null)
            {
                foreach (var item in entity)                {
                    
                    if (item.Amount == 0)
                        throw new CustomException("Enter Service Amount!");
                   
                }
            }

            _inventoryDetailService.UpdateServiceAndTax(entity, receiveTaxList);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(PurchaseOrder entity)
        {
            _inventoryReveiveService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _inventoryReveiveService.Delete(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #region Inventory Detail

        [Authorize, HttpGet]
        public JsonResult GetToCurrencyRate(string currencyId, string baseCurrencyId, string docDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetToCurrencyRate(currencyId, baseCurrencyId, Convert.ToDateTime(docDate), identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailCreate(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            //if(entity.BudgetType == "New")
            //{

            //}
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            _inventoryDetailService.InsertOrUpdateGraph(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }
        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            _inventoryDetailService.InsertExtraTax(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        public JsonResult InsertserviceTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList, string ServiceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            _inventoryDetailService.InsertserviceTax(entity, taxCategoryList, ServiceId);
            return Json(new { entity.Id, Message = AplosMessage.Updated });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DetailDelete(string receiveDetailId)
        {
            _inventoryDetailService.Delete(receiveDetailId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Inventory Detail

        #region InventorymatrialAdd

        public JsonResult GetMaterialDetails(string MaretialDetailsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetMaterialDetails(MaretialDetailsId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStateByInvoicingPartyPlantId(string InvoicingPartyPlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetStateByInvoicingPartyPlantId(InvoicingPartyPlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Inventory Receive Tax

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId,string PODate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId, PODate), JsonRequestBehavior.AllowGet);
        }
        

        [Authorize, HttpGet]
        public JsonResult GetReceiveTaxList(string receiveDetailId)
        {
            return Json(_inventoryReveiveService.GetReceiveTaxList(receiveDetailId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTotalReceiveTaxList(string receiveId)
        {
            return Json(_inventoryReveiveService.GetTotalReceiveTaxList(receiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceTaxList(string serviceId)
        {
            return Json(_inventoryReveiveService.GetServiceTaxList(serviceId), JsonRequestBehavior.AllowGet);
        }

        #endregion Inventory Receive Tax

        #region Inventory Material

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialList(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.QueryForPurchaseOrderDetail(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialPayable(string inveReveiveId, string employeeId, bool isReversCharge)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(employeeId) && employeeId != "null")
                return Json(_inventoryMaterialService.GetInventoryMaterialForImprestPayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
            else
            {
                if (isReversCharge)
                    return Json(_inventoryMaterialService.GetInventoryMaterialReversChargePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
                else
                    return Json(_inventoryMaterialService.GetInventoryMaterialWithoutReversChargePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Inventory Material

        #region Service Charges

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreate(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            //if(entity != null)            {
                
            //        if (entity.Amount == 0)
            //            throw new CustomException("Enter Service Amount!");                  
               
            //}
            _inventoryService.InsertGraph(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult ServiceChargesDelete(string serviceId)
        {
            _inventoryService.Delete(serviceId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceChargeList(string receiveId)
        {
            return Json(_inventoryService.Query(receiveId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTerms(string id)
        {
            return Json(_inventoryService.GetTerms(id), JsonRequestBehavior.AllowGet);
        }
       
        #endregion Service Charges

        //[HttpGet, Authorize]
        //public ActionResult Report(ReportFormat reportFormat, string inventoryReceiveId, string plantId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var reportFileName = "Inventory Receive " + inventoryReceiveId + "";
        //    var workbook = _inventoryReportService.GetInventoryReceiveReport(identity.CompanyId, plantId, inventoryReceiveId);
        //    switch (reportFormat)
        //    {
        //        case ReportFormat.Pdf:
        //            return RenderReportAsPdf(workbook, reportFileName);

        //        case ReportFormat.Excel:
        //            return RenderReportAsExcel(workbook, reportFileName);

        //        default:
        //            return View();
        //    }
        //}

        #region Employee Purchase

        public ActionResult EmployeePurchase()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeePurchaseList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetEmployeePurchaseList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #endregion Employee Purchase

        #region GRN Approved

        [HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult Approved(IEnumerable<PurchaseOrder> entities)
        {
            _inventoryReveiveService.GRNApproved(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion GRN Approved

        #region PaymentHold
        //IEnumerable<object> GetListForHold(string plantId)
        [Authorize, HttpGet]
        public JsonResult GetListForHold()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForHold(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPOMasterById(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetPOMasterById(identity.PlantId, id), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetListForHold()
        //{
        //    CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_inventoryReveiveService.GetListForHold(identity.PlantId), JsonRequestBehavior.AllowGet);
        //}


        [HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult PaymentHold(IEnumerable<PurchaseOrder> entities)
        {
            _inventoryReveiveService.PaymentHold(entities);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpGet, Authorize]
        public JsonResult GetListByParty()

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_inventoryReveiveService.GetListByParty(identity.CompanyId, PartyType.Vendor.ToString()), JsonRequestBehavior.AllowGet);
            //return Json(POList.GetListByParty(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPartyPlantCbo(string partyId, string Id)
        {
            return Json(_inventoryReveiveService.GetPartyPlantCbo(partyId, Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteMaterialTax(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _inventoryReveiveService.DeleteMaterialTax(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion PaymentHold

        #endregion -- Operations

        #region Purchase Order Report 
        [HttpGet, Authorize]
        public ActionResult GePurchaseOrderReport(string purchaseOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.GePurchaseOrderReport(identity.CompanyGroupId, identity.CompanyId,  identity.PlantId, identity.UserId, purchaseOrderId);

            return null;

        }
        #endregion


        #region PO Approval Shahazan Shahid     

        [Authorize, HttpGet]
        public JsonResult GetListForPOApproval()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForHold(identity.PlantId), JsonRequestBehavior.AllowGet);
        }     
        [HttpPost]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        //public JsonResult PoApproved(string PoId, string PoValue)
        //{
        //    _inventoryReveiveService.PoApproved(PoId, PoValue);
        //    return Json(new { Message = "PO Approved" + AplosMessage.Success });
        //}


        #endregion
        #region   POUnapproved 
        [Authorize, HttpGet]
        public JsonResult GetListForPOApproval1()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForHold1(identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult PoApproved1(string PoId, string PoValue)
        {
            _inventoryReveiveService.PoApproved1(PoId, PoValue);
            return Json(new { Message = "PO UN Approved" + AplosMessage.Success });
        }

        #endregion

        #region PO Closed Kazi Taufik
        [Authorize, HttpGet]

        public JsonResult GetListForPOClose()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForPOClose(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult POClose(string PoId, string PoValue)
        {
            _inventoryReveiveService.POClose(PoId, PoValue);
            return Json(new { Message = AplosMessage.Success + " PO Closed " });
        }
        #endregion

        #region PO UnClosed Taufik
        [Authorize, HttpGet]

        public JsonResult GetListForPOUnClose()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForPOUnClose(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult POUnClose(string PoId, string PoValue)
        {
            _inventoryReveiveService.POUnClose(PoId, PoValue);
            return Json(new { Message = AplosMessage.Success + " PO Closed " });
        }
        #endregion

        #region PO Approval Shahazan Shahid     

        [Authorize, HttpGet]
        public JsonResult GetListForAllPOList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForAllPOList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        //[HttpPost] 

        //Here use same function  of po close



        #endregion


        #region FG PO For Master Order 22-Jun-2019
        [Authorize, HttpGet]
        public ActionResult GetListForMasterOrder()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;           
            return Json(_inventoryReveiveService.GetListForMasterOrder(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMasterItemList(string masterOrderId)
        {
            return Json(_inventoryReveiveService.GetMasterItemList(masterOrderId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryListForFGService(string partyPlantId, string hsnCodeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.getTaxCategoryListForFGService(identity.CompanyGroupId,  identity.PlantId, hsnCodeId, partyPlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion


        //public JsonResult GetSupervisorCbo()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_materialRequsitionMasterService.EntityCbo(), JsonRequestBehavior.AllowGet);
        //}





         //public JsonResult EntityCbo()
         //   {
         //       var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
         //       return Json(_materialRequsitionMasterService.EntityCbo(), JsonRequestBehavior.AllowGet);
         //   }

    }

   
}
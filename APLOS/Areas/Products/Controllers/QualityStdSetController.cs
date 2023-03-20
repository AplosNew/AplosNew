
using Aplos.Controllers;
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
using Library.MaterialManagement.Inventory;
using Library.Service.Products;
using Library.MaterialManagement.Reports;
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
    public class QualityStdSetController : BaseController
    {
        #region Constructor

        private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IMaterialRequsitionDetailsServiceService _materialRequsitionDetailsServiceService;
        private readonly IMaterialRequsitionMasterServiceService _materialRequsitionMasterServiceService;
        private readonly IPurchaseOrderGroupMasterService _purchaseOrderGroupMasterService;
        private readonly IPurchaseOrderGroupDetailsService _purchaseOrderGroupDetailsService;
        private readonly IPOGVendorService _pOGVendorService;
        private readonly IQualityStdSetService _qualityStdSetService;
        private readonly IPurchaseOrderDetailService _inventoryDetailService;
        private readonly IPOMaterialService _inventoryMaterialService;
        private readonly IPurchaseOrderServiceService _inventoryService;
        private readonly IInventoryReceiveReportService _inventoryReportService;
        private readonly ISqlRepository _sqlRepository;

        public QualityStdSetController(
        
            IPurchaseOrderService inventoryReveiveService
            , IMaterialRequsitionDetailsServiceService materialRequsitionDetailsServiceService
            , IPurchaseOrderDetailService inventoryDetailService
            , IPOMaterialService inventoryMaterialService
            , IInventoryReceiveReportService inventoryReportService
            , IPurchaseOrderServiceService inventoryService
            , IMaterialRequsitionMasterServiceService materialRequsitionMasterServiceService
            , IPurchaseOrderGroupMasterService purchaseOrderGroupMasterService
            , IPurchaseOrderGroupDetailsService purchaseOrderGroupDetailsService
            ,IPOGVendorService pOGVendorService
            , IQualityStdSetService qualityStdSetService
            , ISqlRepository sqlRepository)

        {
            _pOGVendorService = pOGVendorService;
            _inventoryReveiveService = inventoryReveiveService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryService = inventoryService;
            _inventoryReportService = inventoryReportService;
            _sqlRepository = sqlRepository;
            _materialRequsitionDetailsServiceService = materialRequsitionDetailsServiceService;
            _materialRequsitionMasterServiceService = materialRequsitionMasterServiceService;
            _purchaseOrderGroupMasterService = purchaseOrderGroupMasterService;
            _qualityStdSetService = qualityStdSetService;
            _purchaseOrderGroupDetailsService = purchaseOrderGroupDetailsService;
        }

        #endregion Constructor

        #region Aplos
     
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Aplos
        #region  Operations / QualityStdSet

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
        public JsonResult Create(QualityStdSet entity)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;

            entity.PlantId = identity.PlantId;

            _qualityStdSetService.Insert(entity);
            return Json(new { entity, Message = AplosMessage.Success + " PO Group No <b>" + entity.Id + "</b>" });
        }
        [HttpPost]
        public JsonResult EditQualityStdSet(QualityStdSet entity)
        {
            _qualityStdSetService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]

        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _qualityStdSetService.DeleteQStd(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_qualityStdSetService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize,  HttpPost]
        public JsonResult UpdateMaterial(IEnumerable<PurchaseOrderGroupDetails> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var ip = identity.IPAddress;
            var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
            var UpdatedBy = identity.Name;
            

            _purchaseOrderGroupMasterService.UpdateMaterial(entity, receiveTaxList);
            return Json(new { Message = AplosMessage.Updated });
        }

        
 #endregion


        [Authorize, HttpPost]
        public JsonResult UpdateServiceAndTax(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var ip = identity.IPAddress;
            var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
            var UpdatedBy = identity.Name;
            if (entity != null)
            {
                foreach (var item in entity)
                {

                    if (item.Amount == 0)
                        throw new CustomException("Enter Service Amount!");

                }
            }

            _inventoryDetailService.UpdateServiceAndTax(entity, receiveTaxList);
            return Json(new { Message = AplosMessage.Success });
        }

    



        #region PurchaseOrdergroupDetails

        [Authorize, HttpGet]
        public JsonResult GetToCurrencyRate(string currencyId, string baseCurrencyId, string docDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderGroupMasterService.GetToCurrencyRate(currencyId, baseCurrencyId, Convert.ToDateTime(docDate), identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult DetailCreate(IEnumerable<PurchaseOrderGroupDetailsViewModel>  entity ,string id, string Gname)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            _purchaseOrderGroupDetailsService.InsertOrUpdateGraph(entity, id, Gname);
            return Json(new { PurchaseOrderGroupDetail = entity, Message = AplosMessage.Insert });
        }

        [Authorize, HttpPost]
        public JsonResult POGVendorCreate(POGVendor entity, string id)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           
            _pOGVendorService.InsertOrUpdateGraphPOGVendor(entity, id);
            return Json(new { POGVendor = entity, entity.Id, Message = AplosMessage.Insert });
        }


        [Authorize, HttpPost]
        public JsonResult DetailEdit(PurchaseOrderGroupDetails entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _purchaseOrderGroupDetailsService.InsertOrUpdateGraphEdit(entity);
            return Json(new { entity.Id, Message = AplosMessage.Updated });
        }


        [Authorize,HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DetailDelete(string id)
        {
            _purchaseOrderGroupDetailsService.DeletePOGDetails(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize,HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult POGVendorDelete(string id)
        {
            _pOGVendorService.POGVendorDelete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [Authorize,HttpPost]

        public JsonResult InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            _inventoryDetailService.InsertExtraTax(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [Authorize, HttpPost]
        public JsonResult InsertserviceTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList, string ServiceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            _inventoryDetailService.InsertserviceTax(entity, taxCategoryList, ServiceId);
            return Json(new { entity.Id, Message = AplosMessage.Updated });
        }



        #endregion Inventory Detail

        //#region InventorymatrialAdd
        //[Authorize, HttpGet]
        //public JsonResult GetMaterialDetails(string MaretialDetailsId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_inventoryReveiveService.GetMaterialDetails(MaretialDetailsId), JsonRequestBehavior.AllowGet);
        //}
        //[Authorize, HttpGet]
        //public JsonResult GetStateByInvoicingPartyPlantId(string InvoicingPartyPlantId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_inventoryReveiveService.GetStateByInvoicingPartyPlantId(InvoicingPartyPlantId), JsonRequestBehavior.AllowGet);
        //}
        //#endregion

        //#region Inventory Receive Tax

        //[Authorize, HttpGet]
        //public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_inventoryReveiveService.GetTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId), JsonRequestBehavior.AllowGet);
        //}


        //[Authorize, HttpGet]
        //public JsonResult GetReceiveTaxList(string receiveDetailId)
        //{
        //    return Json(_inventoryReveiveService.GetReceiveTaxList(receiveDetailId), JsonRequestBehavior.AllowGet);
        //}

        //[Authorize, HttpGet]
        //public JsonResult GetTotalReceiveTaxList(string receiveId)
        //{
        //    return Json(_inventoryReveiveService.GetTotalReceiveTaxList(receiveId), JsonRequestBehavior.AllowGet);
        //}

        //[Authorize, HttpGet]
        //public JsonResult GetServiceTaxList(string serviceId)
        //{
        //    return Json(_inventoryReveiveService.GetServiceTaxList(serviceId), JsonRequestBehavior.AllowGet);
        //}

        //#endregion Inventory Receive Tax

        //#region Inventory Material



        //[Authorize, HttpGet]
        //public JsonResult GetInventoryMaterialPayable(string inveReveiveId, string employeeId, bool isReversCharge)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    if (!string.IsNullOrEmpty(employeeId) && employeeId != "null")
        //        return Json(_inventoryMaterialService.GetInventoryMaterialForImprestPayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
        //    else
        //    {
        //        if (isReversCharge)
        //            return Json(_inventoryMaterialService.GetInventoryMaterialReversChargePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
        //        else
        //            return Json(_inventoryMaterialService.GetInventoryMaterialWithoutReversChargePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
        //    }
        //}

        //#endregion Inventory Material

        //#region Service Charges
        //[Authorize, HttpPost]

        //public JsonResult ServiceChargesCreate(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        //{

        //    _inventoryService.InsertGraph(entity, taxCategoryList);
        //    return Json(new { entity.Id, Message = AplosMessage.Success });
        //}

        //[HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        //public JsonResult ServiceChargesDelete(string serviceId)
        //{
        //    _inventoryService.Delete(serviceId);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}

        //[Authorize, HttpGet]
        //public JsonResult GetServiceChargeList(string receiveId)
        //{
        //    return Json(_inventoryService.Query(receiveId), JsonRequestBehavior.AllowGet);
        //}

        //#endregion Service Charges

        //#region Employee Purchase

        //public ActionResult EmployeePurchase()
        //{
        //    return View();
        //}

        //[Authorize, HttpGet]
        //public JsonResult GetEmployeePurchaseList(GridParameter parameters)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_inventoryReveiveService.GetEmployeePurchaseList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        //}

        //#endregion Employee Purchase

       

        //#region PaymentHold
       
        //[Authorize, HttpGet]
        //public JsonResult GetListForHold()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_inventoryReveiveService.GetListForHold(identity.PlantId), JsonRequestBehavior.AllowGet);
        //}

        //[Authorize, HttpGet]
        //public JsonResult GetPOMasterById(string id)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_inventoryReveiveService.GetPOMasterById(identity.PlantId, id), JsonRequestBehavior.AllowGet);
        //}



        //[HttpPost, ChaildAction(ParentActionName = "Edit")]
        //public JsonResult PaymentHold(IEnumerable<PurchaseOrder> entities)
        //{
        //    _inventoryReveiveService.PaymentHold(entities);
        //    return Json(new { Message = AplosMessage.Insert });
        //}
        //[HttpGet, Authorize]
        //public JsonResult GetListByParty()

        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //    return Json(_inventoryReveiveService.GetListByParty(identity.CompanyId, PartyType.Vendor.ToString()), JsonRequestBehavior.AllowGet);
        //    //return Json(POList.GetListByParty(), JsonRequestBehavior.AllowGet);
        //}
        //[HttpGet, Authorize]
        //public JsonResult GetPartyPlantCbo(string partyId, string Id)
        //{
        //    return Json(_inventoryReveiveService.GetPartyPlantCbo(partyId, Id), JsonRequestBehavior.AllowGet);
        //}


        //[HttpGet, Authorize]
        //public JsonResult GetVendorCbo(string partyId, string Id)
        //{
        //    return Json(_purchaseOrderGroupMasterService.GetVendorCbo(partyId, Id), JsonRequestBehavior.AllowGet);
        //}
        //[HttpPost, Authorize]
        //public ActionResult DeleteMaterialTax(string id)
        //{
        //    if (!string.IsNullOrEmpty(id))
        //    {
        //        _inventoryReveiveService.DeleteMaterialTax(id);
        //        return Json(new { Message = AplosMessage.Success });
        //    }
        //    else
        //        throw new CustomException(Resources.IdNotFound);
        //}

        //#endregion PaymentHold



        //[Authorize, HttpGet]
        //public JsonResult GetSupervisorCbo()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_materialRequsitionMasterServiceService.GetSupervisorCbo(), JsonRequestBehavior.AllowGet);
        //}
        //[Authorize, HttpGet]
        //public JsonResult GetEntity()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_materialRequsitionMasterServiceService.GetEntity(), JsonRequestBehavior.AllowGet);
        //}
        //[Authorize, HttpGet]
        //public JsonResult GetEmployee()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_materialRequsitionMasterServiceService.GetEmployee(), JsonRequestBehavior.AllowGet);
        //}

        [Authorize, HttpGet]

        public JsonResult GetQualityStdSetGridData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_qualityStdSetService.GetQualityStdSetGridData(), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]

        public JsonResult GetMaterialGridData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderGroupDetailsService.GetMaterialGridData(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAllPurchaseOrderGroupDetails(string Id) //string ReqDetailId
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderGroupMasterService.GetAllPurchaseOrderGroupDetails(Id), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetAllPOGVendor(string Id) //string ReqDetailId
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderGroupMasterService.GetAllPOGVendor(Id), JsonRequestBehavior.AllowGet);
        }

       
      
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialList(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialRequsitionMasterServiceService.QueryForPurchaseOrderDetail(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListById(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialRequsitionMasterServiceService.GetInventoryMaterialListById(inveReveiveId), JsonRequestBehavior.AllowGet);
        }
     
    
    }


}
   


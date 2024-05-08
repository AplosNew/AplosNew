
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.Service.Logs;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.OrderManagement.ShipmentControl;
using Library.OrderManagement.TermsAndConditions;
using Library.MaterialManagement.InventoryManagements;
using Aplos.MaterialManagement.MaterialQuery;
using System.Linq;

namespace Aplos.Areas.Products.Controllers
{
    public class PurchaseOrderController : Controller
    {
        Library.General.Conversions.UOMConversion conversion = new Library.General.Conversions.UOMConversion();
        #region Constructor

        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPurchaseOrderDetailService _purchseOrderDetailService;
        private readonly IPOMaterialService _inventoryMaterialService;
        private readonly IPurchaseOrderServiceService _inventoryService;
        private readonly IInventoryReceiveReportService _inventoryReportService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IServiceRequsitionMasterService _serviceRequsitionMasterService;


        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService
            , IPurchaseOrderDetailService purchaseOrderDetailService
            , IPOMaterialService inventoryMaterialService
            , IInventoryReceiveReportService inventoryReportService
            , IPurchaseOrderServiceService inventoryService
            , IServiceRequsitionMasterService serviceRequsitionMasterService
            , ISqlRepository sqlRepository)
        {
            _purchaseOrderService = purchaseOrderService;
            _purchseOrderDetailService = purchaseOrderDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryService = inventoryService;
            _inventoryReportService = inventoryReportService;
            _sqlRepository = sqlRepository;
            _serviceRequsitionMasterService = serviceRequsitionMasterService;
        }

        #endregion Constructor
        ShipmentControl control = new ShipmentControl();
        TermsAndConditionsService tg = new TermsAndConditionsService();

        bplib.clsGenID objGenID = new bplib.clsGenID();
        #region Aplos

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult POBOQ()
        {
            return View();
        }
        [Authorize]
        public ActionResult GRNApproved()
        {
            return View();
        }
        [Authorize]
        public ActionResult PaymentHold()
        {
            return View();
        }
        [Authorize]
        public ActionResult POChecke()
        {
            return View();
        }
        [Authorize]
        public ActionResult POUnApproval()
        {
            return View();
        }
        [Authorize]
        public ActionResult POClosed()
        {
            return View();
        }
        [Authorize]
        public ActionResult FGForMasterOrder()
        {
            return View();
        }
        [Authorize]
        public ActionResult POApprove()
        {
            return View();
        }


        public ActionResult PurchaseOrderByRequisition()
        {
            return View();
        }



        public ActionResult POLCMap()
        {
            return View();
        }



        public ActionResult ServicePOByRequisition()
        {
            return View();
        }

        [Authorize]
        public ActionResult ServicePOCheck()
        {
            return View();
        }

        [Authorize]
        public ActionResult ServicePOApproval()
        {
            return View();
        }

        public ActionResult ServicePoAcknowledgement()
        {
            return View();
        }
        [Authorize]
        public ActionResult ServiceAcknowledgementChecked()
        {
            return View();
        }

        [Authorize]
        public ActionResult ServiceAcknowledgementApproved()
        {
            return View();
        }

        public ActionResult POUncheckedAndUnApproved()
        {
            return View();
        }
        public ActionResult ServicePOIndependent()
        {
            return View();
        }
        public ActionResult PORollBack()
        {
            return View();
        }
        public ActionResult ServiceAcknowledgement()
        {
            return View();
        }

        #endregion Aplos

        #region PO-without-requisition
        [HttpPost]
        public JsonResult Create(PurchaseOrder entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
            {
                CheckedByStatusForNoti = "False";
                ApprovedByStatusForNoti = "False";
            }

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.CompanyId = identity.CompanyId;
                entity.PlantId = identity.PlantId;
                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {
                    entity.AuthorizedBy = entity.CheckedBy;
                    entity.AuthorizedByStatus = "For Approval";
                    entity.CheckedBy = null;
                    entity.CheckedByStatus = null;
                    entity.POType = "PO";
                    entity.IsApproved = false;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    entity.CheckedByStatus = null;
                    entity.AuthorizedByStatus = null;
                    entity.CheckedBy = null;
                    entity.AuthorizedBy = null;
                    entity.POType = "PO";
                    entity.IsApproved = true;
                }
                else
                {
                    entity.CheckedBy = entity.CheckedBy;
                    entity.CheckedByStatus = "Pending";
                    entity.AuthorizedBy = null;
                    entity.AuthorizedByStatus = null;
                    entity.POType = "PO";
                    entity.IsApproved = false;
                }

                entity.IsClosed = false;
                entity.MasterOrderId = null;
                //entity.CheckedBy = "";
                entity.AddedBy = null;
                entity.EmployeeId = identity.EmployeeId;
                //entity.AuthorizedBy = null;               
                _purchaseOrderService.Insert(entity);
                return Json(new { entity, Message = AplosMessage.Success + " PO no <b>" + entity.Id + "</b>" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailCreate(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            if (entity.TransactionUoMId == "" || entity.TransactionUoMId == null)
            {
                throw new CustomException("Please select UOM!");
            }
            else if (entity.DeliveryDate == null)
            {
                throw new CustomException("Please select Delivery Date!");
            }
            else if (entity.MaterialMasterId == null || entity.MaterialMasterId == "")
            {
                if (entity.Description == "" || entity.Description == null)
                {
                    throw new CustomException("Enter Material Descrition!");
                }
                if (entity.DeliveryDate == null)
                {
                    throw new CustomException("Select Delivery date!");
                }

            }
            _purchseOrderDetailService.InsertOrUpdateGraph(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DetailDelete(string receiveDetailId, string OrderSpecific)
        {
            _purchseOrderDetailService.Delete(receiveDetailId, OrderSpecific);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [Authorize, HttpPost]
        public JsonResult UpdateMaterial(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var ip = identity.IPAddress;
            var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
            var UpdatedBy = identity.Name;
            _purchseOrderDetailService.UpdateMaterial(entity, receiveTaxList);
            return Json(new { Message = AplosMessage.Updated });
        }


        #endregion  PO-without-requisition

        #region Purchase-Order-By-Requisition Action functionEditPOByReq
        [HttpPost]
        public JsonResult CreatePOByReq(PurchaseOrder entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.CompanyId = identity.CompanyId;
                entity.PlantId = identity.PlantId;
                if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
                {
                    CheckedByStatusForNoti = "False";
                    ApprovedByStatusForNoti = "False";
                }

                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {

                    entity.AuthorizedBy = entity.CheckedBy;
                    entity.AuthorizedByStatus = "For Approval";
                    entity.CheckedBy = null;
                    entity.CheckedByStatus = null;
                    entity.POType = "POByReq";
                    entity.IsApproved = false;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    entity.CheckedByStatus = null;
                    entity.AuthorizedByStatus = null;
                    entity.CheckedBy = null;
                    entity.AuthorizedBy = null;
                    entity.POType = "POByReq";
                    entity.IsApproved = true;
                }
                else
                {
                    entity.CheckedBy = entity.CheckedBy;
                    entity.CheckedByStatus = "Pending";
                    entity.AuthorizedBy = null;
                    entity.AuthorizedByStatus = null;
                    entity.POType = "POByReq";
                    entity.IsApproved = false;

                }


                entity.IsClosed = false;
                entity.MasterOrderId = null;
                //entity.CheckedBy = "";
                entity.AddedBy = null;
                entity.EmployeeId = identity.EmployeeId;
                _purchaseOrderService.Insert(entity);
                return Json(new { entity, Message = AplosMessage.Success + " PO no <b>" + entity.Id + "</b>" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult DeletePOByReq(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _purchaseOrderService.Delete(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _purchaseOrderService.Delete(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailCreatePOByReq(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (entity == null)
            {
                throw new CustomException("Please select Items");
            }
            else
            {
                _purchseOrderDetailService.InsertOrUpdateGraphPoByReq(entity, groupList, taxCategoryList, PoId);
            }
            return Json(new { entity, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DetailDeletePOByReq(string receiveDetailId)
        {
            _purchseOrderDetailService.DeletePOByReq(receiveDetailId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion Purchase-Order-By-Requisition Action function

        #region Operations / Methods

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.Query(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPostingList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetPostingList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
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
            return Json(_purchaseOrderService.GetListForInvPayable(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
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
        [HttpPost, Authorize, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailCreateFGMasterOrder(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> Materialentity, IEnumerable<PurchaseOrderTax> taxCategoryList, IEnumerable<InventoryMaterialViewModel> ServiceEntity, IEnumerable<PurchaseOrderTax> ServicetaxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _purchseOrderDetailService.InsertOrUpdateGraphFGForMasterOrder(entity, Materialentity, taxCategoryList, ServiceEntity, ServicetaxCategoryList);
            ServiceChargesCreateFG(ServiceEntity, ServicetaxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreateFG(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            _inventoryService.InsertGraphFG(entity, taxCategoryList);
            return Json(new { Message = AplosMessage.Success });//entity.Id,
        }

        [HttpPost, Authorize]
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

            _purchseOrderDetailService.UpdateServiceAndTax(entity, receiveTaxList);
            return Json(new { Message = AplosMessage.Success });
        }

        [Authorize, HttpPost]
        public JsonResult Edit(PurchaseOrder entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
            {
                CheckedByStatusForNoti = "False";
                ApprovedByStatusForNoti = "False";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            if (identity.EmployeeId == entity.CheckedBy)
            {
                throw new CustomException("Please select another employee for Check by.");
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
            {
                entity.AuthorizedBy = entity.CheckedBy;
                entity.AuthorizedByStatus = "For Approval";
                entity.CheckedBy = null;
                entity.CheckedByStatus = null;
                entity.POType = "PO";
                entity.IsApproved = false;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.POType = "PO";
                entity.IsApproved = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "Pending";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.POType = "PO";
                entity.IsApproved = false;
            }


            entity.IsClosed = false;
            entity.MasterOrderId = null;
            //entity.CheckedBy = "";
            entity.AddedBy = null;
            entity.EmployeeId = identity.EmployeeId;
            //entity.AuthorizedBy = null;

            _purchaseOrderService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }
        #region Inventory Detail

        [Authorize, HttpGet]
        public JsonResult GetToCurrencyRate(string currencyId, string baseCurrencyId, string docDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetToCurrencyRate(currencyId, baseCurrencyId, Convert.ToDateTime(docDate), identity.CompanyId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            _purchseOrderDetailService.InsertExtraTax(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [Authorize, HttpPost]
        public JsonResult InsertserviceTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList, string ServiceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            _purchseOrderDetailService.InsertserviceTax(entity, taxCategoryList, ServiceId);
            return Json(new { entity.Id, Message = AplosMessage.Updated });
        }

        #endregion Inventory Detail

        #region InventorymatrialAdd
        [Authorize, HttpGet]
        public JsonResult GetMaterialDetails(string MaretialDetailsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetMaterialDetails(MaretialDetailsId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetStateByInvoicingPartyPlantId(string InvoicingPartyPlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetStateByInvoicingPartyPlantId(InvoicingPartyPlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Inventory Receive Tax

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId, string PODate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId, PODate), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult getserviceTaxByTaxCategoryList(string receiveId, string hsnCodeId, string PODate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.getserviceTaxByTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId, PODate), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryListForSalesService(string receiveId, string hsnCodeId, string InventorySalesDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetTaxCategoryListForSalesService(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId, InventorySalesDate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetReceiveTaxList(string receiveDetailId)
        {
            return Json(_purchaseOrderService.GetReceiveTaxList(receiveDetailId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTotalReceiveTaxList(string receiveId)
        {
            return Json(_purchaseOrderService.GetTotalReceiveTaxList(receiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceTaxList(string serviceId)
        {
            return Json(_purchaseOrderService.GetServiceTaxList(serviceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceTaxListForTaxDetail(string serviceId)
        {
            return Json(_purchaseOrderService.GetServiceTaxListForTax(serviceId), JsonRequestBehavior.AllowGet);
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
        public JsonResult GetPOTaxListForUpdate( string poId)
        {
            return Json(_inventoryMaterialService.GetPOTaxUpdateList(poId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetPOBOQMAPList(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.GetPOBOQMAPList(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
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

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreate(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            _inventoryService.InsertGraph(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [Authorize, HttpPost]
        public JsonResult ServiceChargesCreates(InventoryMaterialViewModel entity, IEnumerable<ServicePOAckTax> taxCategoryList)
        {
            _purchaseOrderService.InsertGraphCharge(entity, taxCategoryList);
            return Json(new { entity.ServiceAcknowledgementMasterId, Message = AplosMessage.Success });
        }
        [Authorize, HttpPost]
        public JsonResult ServiceChargesUpdate(InventoryMaterialViewModel entity, List<ServicePOAckTax> taxCategoryList)
        {
            _purchaseOrderService.UpdateGraphCharge(entity, taxCategoryList);
            return Json(new { entity.ServiceAcknowledgementMasterId, Message = AplosMessage.Success });
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceChargeListForCharge(string MasterId)
        {
            return Json(_purchaseOrderService.QueryForCharges(MasterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
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
        #region Employee Purchase
        [Authorize]
        public ActionResult EmployeePurchase()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeePurchaseList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetEmployeePurchaseList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #endregion Employee Purchase

        #region GRN Approved

        [Authorize, HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult Approved(IEnumerable<PurchaseOrder> entities)
        {
            _purchaseOrderService.GRNApproved(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion GRN Approved

        #region PaymentHold
        //IEnumerable<object> GetListForHold(string plantId)

        #region PO Index UI all Function
        [Authorize, HttpGet]
        public JsonResult GetPOTypeList(string POTypeStatus, string poType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetPOTypeList(identity.PlantId, POTypeStatus, poType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetIndependentPOListByStatus(string ApproveRejectHold)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_purchaseOrderService.GetIndependentPOListByStatus(identity.PlantId, ApproveRejectHold), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpGet]
        public JsonResult GetListForHold11BOQ(string ApproveRejectHold,string poType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var jsondata = Json(_purchaseOrderService.GetListForHold11BOQ(identity.PlantId, ApproveRejectHold, poType), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion
        [Authorize, HttpGet]
        public JsonResult GetPOMasterById(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetPOMasterById(identity.PlantId, id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult PaymentHold(IEnumerable<PurchaseOrder> entities)
        {
            _purchaseOrderService.PaymentHold(entities);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpGet, Authorize]
        public JsonResult GetListByParty()

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_purchaseOrderService.GetListByParty(identity.CompanyId, PartyType.Vendor.ToString()), JsonRequestBehavior.AllowGet);
            //return Json(POList.GetListByParty(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPartyPlantCbo(string partyId, string Id)
        {
            return Json(_purchaseOrderService.GetPartyPlantCbo(partyId, Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteMaterialTax(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _purchaseOrderService.DeleteMaterialTax(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion PaymentHold

        #endregion -- Operations

        #region PurchaseOrder Report 
        [HttpGet, Authorize]
        public ActionResult GePurchaseOrderReport(string purchaseOrderId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            _purchaseOrderService.GePurchaseOrderReport(identity.CompanyGroupId, identity.CompanyId, plantId, identity.UserId, purchaseOrderId);

            return null;

        }
        #endregion

        #region PurchaseOrderBOQ Report 
        [HttpGet, Authorize]
        public ActionResult GePurchaseOrderBOQReportWithTax(string purchaseOrderBOQId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            Library.MaterialManagement.InventoryManagements.POBOQReportService Report = new Library.MaterialManagement.InventoryManagements.POBOQReportService();
            Report.GePurchaseOrderBOQReportWithTax(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, purchaseOrderBOQId);

            return null;
        }
        [HttpGet, Authorize]
        public ActionResult GePurchaseOrderBOQReportWithoutTax(string purchaseOrderBOQId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            Library.MaterialManagement.InventoryManagements.POBOQReportService Report = new Library.MaterialManagement.InventoryManagements.POBOQReportService();
            Report.GePurchaseOrderBOQReportWithoutTax(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, purchaseOrderBOQId);

            return null;
        }
        #endregion


        #region PurchaseAcceptance Report 
        [HttpGet, Authorize]
        public ActionResult PurchaseAcceptanceReport(string PDACId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _purchaseOrderService.GetPurchaseAcceptanceReport(identity.CompanyGroupId, identity.PlantId, PDACId);

            return null;

        }
        #endregion
        #region PO Approval Shahazan Shahid     
        [HttpPost, Authorize]
        public JsonResult PoApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string CheckedRejectReason)
        {
            if (CheckedStataus == "" || CheckedStataus == null)
            {
                throw new CustomException("Please Select Checked By Status!");
            }
            _purchaseOrderService.PoApproved(PoId, PoValue, CheckedStataus, AuthorizedBy, CheckedRejectReason);
            return Json(new { Message = "PO Approved" + AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult PoUnApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            _purchaseOrderService.PoUnApproved(PoId, PoValue, CheckedStataus, AuthorizedBy);
            return Json(new { Message = "PO Approved" + AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult PoApprovedAuth(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string ApproveRejectReason)
        {
            if (CheckedStataus == "" || CheckedStataus == null)
            {
                throw new CustomException("Please Select Checked By Status!");
            }
            _purchaseOrderService.PoApprovedAuth(PoId, PoValue, CheckedStataus, AuthorizedBy, ApproveRejectReason);
            return Json(new { Message = "PO Approved" + AplosMessage.Success });
        }
        #endregion
        #region   POUnapproved 
        [Authorize, HttpGet]
        public JsonResult GetListForPOApproval1UnApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForPOApproval1UnApproved(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult PoApproved1Auth(string PoId, string PoValue)
        {
            _purchaseOrderService.PoApproved1(PoId, PoValue);
            return Json(new { Message = "PO UN Approved" + AplosMessage.Success });
        }
        #endregion

        #region PO Closed Kazi Taufik
        [Authorize, HttpGet]
        public JsonResult GetListForPOClose()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForPOClose(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult POClose(string PoId, string PoValue)
        {
            _purchaseOrderService.POClose(PoId, PoValue);
            return Json(new { Message = AplosMessage.Success + " PO Closed " });
        }
        #endregion

        #region PO UnClosed Taufik
        [Authorize, HttpGet]
        public JsonResult GetListForPOUnClose()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForPOUnClose(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult POUnClose(string PoId, string PoValue)
        {
            _purchaseOrderService.POUnClose(PoId, PoValue);
            return Json(new { Message = AplosMessage.Success + " PO Closed " });
        }
        #endregion

        #region PO Approval Shahazan Shahid     
        [Authorize, HttpGet]
        public JsonResult GetListForAllPOList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForAllPOList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        //[HttpPost] 

        //Here use same function  of po close
        #endregion
        #region FG PO For Master Order 22-Jun-2019
        [Authorize, HttpGet]
        public ActionResult GetListForMasterOrder()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForMasterOrder(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMasterItemList(string masterOrderId)
        {
            return Json(_purchaseOrderService.GetMasterItemList(masterOrderId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryListForFGService(string partyPlantId, string hsnCodeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.getTaxCategoryListForFGService(identity.CompanyGroupId, identity.PlantId, hsnCodeId, partyPlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        [Authorize, HttpGet]
        public JsonResult GetSupervisorCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetSupervisorCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetIssueSlipCheckByCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetIssueSlipCheckByCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSupervisorCboApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetSupervisorCboApproved(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSupervisorCboApproved1()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetSupervisorCboApproved1(), JsonRequestBehavior.AllowGet);
        }

        #region Purchase Order By Requisition

        [Authorize, HttpGet]
        public JsonResult GetListForPOBYReq(string POTypeStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForPOBYReq(identity.PlantId, POTypeStatus), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetListForPOBYReq1(string ApproveRejectHold)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForPOBYReq1(identity.PlantId, ApproveRejectHold), JsonRequestBehavior.AllowGet);
        }



        [Authorize, HttpPost]
        public JsonResult GetApprovedListForPOBYReq(string column, string value)
        {
            PurchaseOrderQueryService purchaseOrderService = new PurchaseOrderQueryService();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(purchaseOrderService.GetApprovedListForPOBYReq(identity.PlantId, column, value), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetListForRequisition()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(_purchaseOrderService.GetListForRequisition(identity.CompanyGroupId));
            var jsondata = Json(new { NewData, Message = AplosMessage.Success });
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [Authorize, HttpGet]
        public ActionResult GetListForRequisition1()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForRequisition1(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetRequisitionList(string RequisitionId)
        {
            return Json(_purchaseOrderService.GetRequisitionList(RequisitionId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult EditPOByReq(PurchaseOrder entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            try
            {
                if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
                {
                    CheckedByStatusForNoti = "False";
                    ApprovedByStatusForNoti = "False";
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.CompanyId = identity.CompanyId;
                entity.PlantId = identity.PlantId;
                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }

                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {
                    entity.AuthorizedBy = entity.CheckedBy;
                    entity.AuthorizedByStatus = "For Approval";
                    entity.CheckedBy = null;
                    entity.CheckedByStatus = null;
                    entity.POType = "POByReq";
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    entity.CheckedByStatus = null;
                    entity.AuthorizedByStatus = null;
                    entity.CheckedBy = null;
                    entity.AuthorizedBy = null;
                    entity.POType = "POByReq";
                }
                else
                {
                    entity.CheckedBy = entity.CheckedBy;
                    entity.CheckedByStatus = "Pending";
                    entity.AuthorizedBy = null;
                    entity.AuthorizedByStatus = null;
                    entity.POType = "POByReq";
                }

                entity.IsApproved = false;
                entity.IsClosed = false;
                // entity.POType = "POByReq";
                entity.MasterOrderId = null;
                //entity.CheckedBy = "";
                entity.AddedBy = null;
                entity.RequisitionId = null;
                entity.EmployeeId = identity.EmployeeId;
                _purchaseOrderService.Update(entity);
            }
            catch (Exception)
            {

            }

            return Json(new { Message = AplosMessage.Updated });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailUpdatePOByReq(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (entity == null)
            {
                throw new CustomException("Please select Items");
            }
            else
            {
                _purchseOrderDetailService.InsertOrUpdateGraphPoUpdateByReq(entity, groupList, taxCategoryList, PoId);
            }
            return Json(new { entity, Message = AplosMessage.Success });
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListPoByReq(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.GetInventoryMaterialListPoByReq(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        #region Service Charges

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreatePOByReq(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            _inventoryService.InsertGraphPOByReq(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }


        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult ServiceChargesDeletePOByReq(string serviceId)
        {
            _inventoryService.Delete(serviceId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceChargeListPOByReq(string receiveId)
        {
            return Json(_inventoryService.Query(receiveId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTermsPOByReq(string id)
        {
            return Json(_inventoryService.GetTerms(id), JsonRequestBehavior.AllowGet);
        }

        #endregion Service Charges

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListForPOUpdate(string inveReveiveId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.GetInventoryMaterialListForPOUpdate(inveReveiveId, MaterialMasterId, ArticleId, FirstCharacteristicsValueId, SecondCharacteristicsValueId, ThirdCharacteristicsValueId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryListPO(string receiveDetailId)
        {
            return Json(_purchaseOrderService.GetTaxCategoryListPOBYReq(receiveDetailId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GePurchaseOrderReportByReq(string purchaseOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _purchaseOrderService.GePurchaseOrderReportByReq(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, purchaseOrderId);

            return null;

        }

        #endregion
        #region Purchaser LC Intregrated to PurchaseOrder

        [Authorize, HttpGet]
        public JsonResult GetLCContractList(bool isProcurementOnBom)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetLCContractList(isProcurementOnBom, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        //[Authorize, HttpGet]
        //public JsonResult GetLCContractListByPartyId(bool isProcurementOnBom, string partyId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_purchaseOrderService.GetLCContractListByPartyId(isProcurementOnBom, identity.PlantId, partyId), JsonRequestBehavior.AllowGet);
        //}


        [Authorize, HttpGet]
        public JsonResult GetalldataPOWithLCMap()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetalldataPOWithLCMap(identity.PlantId), JsonRequestBehavior.AllowGet);
        }



        [Authorize, HttpGet]
        public JsonResult GetalldataPOWithoutLCMap()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetalldataPOWithoutLCMap(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetLCListByContract(string ContractId, string VendorId, string CurrencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetLCListByContract(ContractId, VendorId, CurrencyId), JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult UpdatePOforLC(string POId, string PurchaseLCId, string flag)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _purchaseOrderService.UpdatePOforLC(POId, PurchaseLCId, flag);
            return Json(new { Message = AplosMessage.Updated });
        }
        #endregion

        #region Service-PO-ByRequisition


        [HttpPost]
        public JsonResult CreateServicePOByReq(ServicePOMaster entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
                {
                    CheckedByStatusForNoti = "False";
                    ApprovedByStatusForNoti = "False";
                }

                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }

                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.CompanyId = identity.CompanyId;
                entity.PlantId = identity.PlantId;
                entity.IsApproved = false;
                entity.IsClosed = false;
                entity.POType = entity.POType; //"ServicePOByReq";
                entity.MasterOrderId = null;
                entity.AddedBy = null;
                entity.EmployeeId = identity.EmployeeId;

                //entity.CheckedByStatus = "ForChecked";
                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {

                    entity.ApprovedBy = entity.CheckedBy;
                    entity.ApprovedByStatus = "For Approval";
                    entity.CheckedBy = null;
                    entity.CheckedByStatus = null;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    entity.CheckedByStatus = null;
                    entity.ApprovedByStatus = null;
                    entity.CheckedBy = null;
                    entity.ApprovedBy = null;
                }
                else
                {
                    entity.CheckedBy = entity.CheckedBy;
                    entity.CheckedByStatus = "For Checking";
                    entity.ApprovedBy = null;
                    entity.ApprovedByStatus = null;

                }


                _purchaseOrderService.InsertServicePOByReq(entity);
                return Json(new { entity, Message = AplosMessage.Success + " Requisition No <b>" + entity.Id + "</b>" });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public JsonResult EditServicePOByReq(ServicePOMaster entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
                {
                    CheckedByStatusForNoti = "False";
                    ApprovedByStatusForNoti = "False";
                }

                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }

                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.CompanyId = identity.CompanyId;
                entity.PlantId = identity.PlantId;
                entity.IsApproved = false;
                entity.IsClosed = false;
                entity.POType = entity.POType; //"ServicePOByReq";
                entity.MasterOrderId = null;
                entity.AddedBy = null;
                entity.EmployeeId = identity.EmployeeId;

                //entity.CheckedByStatus = "ForChecked";
                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {

                    entity.ApprovedBy = entity.CheckedBy;
                    entity.ApprovedByStatus = "For Approval";
                    entity.CheckedBy = null;
                    entity.CheckedByStatus = null;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    entity.CheckedByStatus = null;
                    entity.ApprovedByStatus = null;
                    entity.CheckedBy = null;
                    entity.ApprovedBy = null;
                }
                else
                {
                    entity.CheckedBy = entity.CheckedBy;
                    entity.CheckedByStatus = "For Checking";
                    entity.ApprovedBy = null;
                    entity.ApprovedByStatus = null;

                }
                _purchaseOrderService.Update(entity);
                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception)
            {

            }

            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult DeleteServicePOByReq(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _purchaseOrderService.DeleteServicePOReq(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [Authorize, HttpGet]
        public JsonResult GetListForServicePOBYReq(string POTypeStatus, string POType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForServicePOBYReq(identity.PlantId, POTypeStatus, POType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListForServicePOBYReqHR(string ApproveRejectHold, string POType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetListForServicePOBYReqHR(identity.PlantId, ApproveRejectHold, POType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetListForServiceRequisition(string Id)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_inventoryReveiveService.GetListForServiceRequisition(Id), JsonRequestBehavior.AllowGet);
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.GetListForServiceRequisition(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult CreateServicePODetailByReq(IEnumerable<ServicePODetailsViewModel> entity, string ServicePoMasterId, IEnumerable<ServicePOTax> taxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (entity == null)
            {
                throw new CustomException("Please select Items");
            }
            else
            {
                _purchseOrderDetailService.InsertServicePODetailByReq(entity, ServicePoMasterId, taxCategoryList);
            }
            return Json(new { entity, Message = AplosMessage.Success });
        }
        [HttpPost]
        public JsonResult CreateServicePODetail(ServicePODetail entity, string ServicePoMasterId, IEnumerable<ServicePOTax> taxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (entity == null)
            {
                throw new CustomException("Please select Items");
            }
            else
            {
                _purchseOrderDetailService.InsertServicePODetail(entity, ServicePoMasterId, taxCategoryList);
            }
            return Json(new { entity, Message = AplosMessage.Success });
        }

        #region Ado wise delete

        [HttpPost]
        public ActionResult ServicePODetailDelete(string SPODetailid)
        {
            DeleteServicePODetail(SPODetailid);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteServicePODetail(string SPODetailid)
        {
            string strSQL;
            string strSQL1;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [TRN].[ServicePOTax] WHERE ServicePODetailId = '" + SPODetailid + "'";
                strSQL1 = "DELETE FROM [TRN].[ServicePODetail] WHERE Id = '" + SPODetailid + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL1, true, "1");
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
        }

        #endregion Ado wise delete


        #endregion Service-PO-ByRequisition

        #region Survice PurchaseOrder Report 
        [HttpGet, Authorize]
        public ActionResult ServicePurchaseOrderReport(string purchaseOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _purchaseOrderService.ServicePurchaseOrderReport(identity.CompanyGroupId, identity.PlantId, purchaseOrderId);

            return null;

        }

        #endregion
        [Authorize, HttpGet]
        public JsonResult GetServicePOTerms(string id)
        {
            return Json(_inventoryService.GetServicePOTerms(id), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceChargePOServiceList(string id)
        {
            return Json(_inventoryService.GetServiceChargePOServiceList(id), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetServicePOByReqSupervisorCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetServicePOByReqSupervisorCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]

        public JsonResult GetUpdateServicePOTax(IEnumerable<ServicePOTaxViewModel> receiveTaxList, string ServicePODetailId, string servicePOid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (receiveTaxList == null)
            {
                throw new CustomException("Please select Items");
            }
            else
            {
                _purchseOrderDetailService.GetUpdateServicePOTax(receiveTaxList, ServicePODetailId, servicePOid);
            }
            return Json(new { receiveTaxList, Message = AplosMessage.Success });
        }
        [Authorize, HttpGet]
        public JsonResult LoadServicePoDetails(string id)
        {
            return Json(_inventoryService.LoadServicePoDetails(id), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult LoadTaxById(string id)
        {
            return Json(_inventoryService.LoadTaxById(id), JsonRequestBehavior.AllowGet);
        }




        #region Service PO Checked And Approved
        [HttpGet, Authorize]
        public JsonResult GetCheckedApprovedList(string tabType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                if (tabType == "UnCheckedList")
                {
                    sql = @"
                            SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
	                        , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
	                        , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
	                        , CP.UserName AS PartyAccountGroupName
	                        , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
	                        , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
	                        , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
	                        , SPOM.FixedAssetOrInventory, SPOM.PODepended
	                        , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
	                        ,SPOM.ToCurrencyRate
	                        , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
	                        , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
	                        ,pgl.CtnId
	                        ,SPOM.AddedBy
	                     --   ,SPOM.CheckedByStatus AS CheckedByStatus
	                        ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy
	                        ,IRD.Amount
	                        ,IRD.TotalTaxAmount
                            ,SPOM.ApprovedHoldRejectReason
                            ,SPOM.CheckedHoldRejectReason
                        FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                        FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                        ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                        left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                        LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                        left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id 
						WHERE B.PlantId='" + identity.PlantId + @"'
                        GROUP BY A.ServicePOMasterId
                        ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                              Where SPOM.CheckedBy='" + identity.EmployeeId + @"' 
							  AND SPOM.CheckedByStatus ='For Checking'";
                }
                else if (tabType == "HoldRejectCheckedList")
                {
                    sql = @"
                        SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
	                        , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
	                        , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
	                        , CP.UserName AS PartyAccountGroupName
	                        , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
	                        , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
	                        , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
	                        , SPOM.FixedAssetOrInventory, SPOM.PODepended
	                        , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
	                        ,SPOM.ToCurrencyRate
	                        , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
	                        , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
	                        ,pgl.CtnId
	                        ,SPOM.AddedBy
	                        ,SPOM.CheckedByStatus AS CheckedByStatus
	                        ,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy
	                        ,IRD.Amount
	                        ,IRD.TotalTaxAmount
                            ,SPOM.ApprovedHoldRejectReason
                            ,SPOM.CheckedHoldRejectReason
                        FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                        FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                        ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                        left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                        LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                        left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"'
                        GROUP BY A.ServicePOMasterId
                        ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                          Where SPOM.CheckedBy='" + identity.EmployeeId + @"' 
                          AND SPOM.CheckedBy Is NOT NULL 
                          And SPOM.[CheckedByStatus]='Hold' OR SPOM.[CheckedByStatus]='Reject'
                          AND SPOM.[ApprovedBy] IS NULL OR SPOM.ApprovedBy = ''
                          AND SPOM.[ApprovedByStatus] Is NULL";
                }
                else if (tabType == "CheckedList")
                {
                    sql = @"
                        SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
	                        , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
	                        , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
	                        , CP.UserName AS PartyAccountGroupName
	                        , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
	                        , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
	                        , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
	                        , SPOM.FixedAssetOrInventory, SPOM.PODepended
	                        , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
	                        ,SPOM.ToCurrencyRate
	                        , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
	                        , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
	                        ,pgl.CtnId
	                        ,SPOM.AddedBy
	                        ,SPOM.CheckedByStatus AS CheckedByStatus
                            ,SPOM.ApprovedByStatus 
	                        --,SPOM.ApprovedByStatus AS ApprovedByStatus,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy
	                        ,IRD.Amount
	                        ,IRD.TotalTaxAmount
                            ,SPOM.ApprovedHoldRejectReason
                            ,SPOM.CheckedHoldRejectReason
                            ,eI.EmployeeName AS CheckedBy
							,eI1.EmployeeName AS ApprovedBy
                        FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                        FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                        ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                        left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                        LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                        left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"'
                        GROUP BY A.ServicePOMasterId
                        ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                              Where SPOM.CheckedBy='" + identity.EmployeeId + @"' 
                          And SPOM.[CheckedByStatus]='Checked' 
                         ";
                }
                else if (tabType == "UnApprovedList")
                {
                    sql = @"
                SELECT * from (       SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
	                        , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
	                        , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
	                        , CP.UserName AS PartyAccountGroupName
	                        , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
	                        , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
	                        , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
	                        , SPOM.FixedAssetOrInventory, SPOM.PODepended
	                        , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
	                        ,SPOM.ToCurrencyRate
	                        , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
	                        , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
	                        ,pgl.CtnId
	                        ,SPOM.AddedBy
	                        ,SPOM.CheckedByStatus AS CheckedByStatus
                            ,SPOM.ApprovedByStatus 
	                        --,ApprovedByStatus=CASE WHEN SPOM.ApprovedByStatus='Approval' THEN 'Approved' else '' END 
                           ,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy
	                        ,IRD.Amount
	                        ,IRD.TotalTaxAmount
                            ,SPOM.ApprovedHoldRejectReason
                            ,SPOM.CheckedHoldRejectReason
                        FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                        FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                        ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                        left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                        LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                        left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"'
                        GROUP BY A.ServicePOMasterId
                        ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                          Where SPOM.ApprovedBy='" + identity.EmployeeId + @"'  
                          And SPOM.[CheckedByStatus]='Checked' 
                          AND SPOM.[ApprovedByStatus]='For Approval'
                 UNION ALL
SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
	                        , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
	                        , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
	                        , CP.UserName AS PartyAccountGroupName
	                        , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
	                        , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
	                        , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
	                        , SPOM.FixedAssetOrInventory, SPOM.PODepended
	                        , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
	                        ,SPOM.ToCurrencyRate
	                        , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
	                        , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
	                        ,pgl.CtnId
	                        ,SPOM.AddedBy
	                        ,SPOM.CheckedByStatus AS CheckedByStatus
                            ,SPOM.ApprovedByStatus 
	                        --,ApprovedByStatus=CASE WHEN SPOM.ApprovedByStatus='Approval' THEN 'Approved' else '' END 
                           ,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy
	                        ,IRD.Amount
	                        ,IRD.TotalTaxAmount
                            ,SPOM.ApprovedHoldRejectReason
                            ,SPOM.CheckedHoldRejectReason
                        FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                        FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                        ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                        left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                        LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                        left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"'
                        GROUP BY A.ServicePOMasterId
                        ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                          Where SPOM.ApprovedBy='" + identity.EmployeeId + @"'  
                          And SPOM.[CheckedByStatus] Is NULL
                          AND SPOM.[ApprovedByStatus] ='For Approval' )X ";
                }
                else if (tabType == "HoldRejectApprovedList")
                {
                    sql = @"
                        SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
	                        , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
	                        , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
	                        , CP.UserName AS PartyAccountGroupName
	                        , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
	                        , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
	                        , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
	                        , SPOM.FixedAssetOrInventory, SPOM.PODepended
	                        , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
	                        ,SPOM.ToCurrencyRate
	                        , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
	                        , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
	                        ,pgl.CtnId
	                        ,SPOM.AddedBy
	                        ,SPOM.CheckedByStatus AS CheckedByStatus
                            ,SPOM.ApprovedByStatus 
	                        --,ApprovedByStatus=CASE WHEN SPOM.ApprovedByStatus='Approval' THEN 'Approved' else '' END 
                           ,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy
	                        ,IRD.Amount
	                        ,IRD.TotalTaxAmount
                            ,SPOM.ApprovedHoldRejectReason
                            ,SPOM.CheckedHoldRejectReason
                        FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                        FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                        ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                        left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                        LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                        left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"'
                        GROUP BY A.ServicePOMasterId
                        ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                          Where SPOM.ApprovedBy='" + identity.EmployeeId + @"' 
                          AND SPOM.CheckedBy Is NOT NULL 
                          And SPOM.[CheckedByStatus]='Checked' 
                          AND SPOM.[ApprovedBy] IS NOT NULL 
                          AND SPOM.[ApprovedByStatus]='Hold' Or  SPOM.[ApprovedByStatus]='Reject'
                          ";
                }
                else if (tabType == "ApprovedList")
                {
                    sql = @"
                        SELECT * from (SELECT ROW_NUMBER()  OVER (ORDER BY  SPOM.Id) AS SiNo,SPOM.Id
	                        , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106),' ','-') AS PODate
	                        , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
	                        , CP.UserName AS PartyAccountGroupName
	                        , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106),' ','-') AS DocDate
	                        , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
	                        , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106),' ','-') AS MatureDate
	                        , SPOM.FixedAssetOrInventory, SPOM.PODepended
	                        , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
	                        ,SPOM.ToCurrencyRate
	                        , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
	                        , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
	                        ,pgl.CtnId
	                        ,SPOM.AddedBy
	                        ,SPOM.CheckedByStatus AS CheckedByStatus
                            ,SPOM.ApprovedByStatus 
	                        --,ApprovedByStatus=CASE WHEN SPOM.ApprovedByStatus='Approval' THEN 'Approved' else '' END 
                            ,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy
	                        ,IRD.Amount
	                        ,IRD.TotalTaxAmount
                            ,SPOM.ApprovedHoldRejectReason
                            ,SPOM.CheckedHoldRejectReason
                        FROM [TRN].[ServicePOMaster] AS SPOM JOIN [HKP].[Party] AS P ON SPOM.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable 
                        FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
                        ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId=SPOM.PlantId
                        LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=SPOM.CheckedBy
                        LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=SPOM.ApprovedBy
                        left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN [ORG].Plant PL ON PL.Id=SPOM.PlantId
                        LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                        LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                        LEFT JOIN (SELECT A.ServicePOMasterId,sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM [TRN].[ServicePODetail] AS A
                        left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"'
                        GROUP BY A.ServicePOMasterId
                        ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                        LEFT JOIN (Select count(Id) as CtnId,POID from TRN.ServicePurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=SPOM.Id
                          Where SPOM.ApprovedBy='" + identity.EmployeeId + @"' 
                          And SPOM.[CheckedByStatus]='Checked' 
                          AND SPOM.[ApprovedByStatus]='Approved'
UNION All

SELECT ROW_NUMBER()  OVER(ORDER BY  SPOM.Id) AS SiNo, SPOM.Id
	                        , REPLACE(CONVERT(CHAR(11), SPOM.PODate, 106), ' ', '-') AS PODate
                              , SPOM.CompanyGroupId, SPOM.CompanyId, SPOM.PlantId, SPOM.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
	                        , CP.UserName AS PartyAccountGroupName
	                        , SPOM.MaterialStorageId, SPOM.DocRefNo, REPLACE(CONVERT(CHAR(11), SPOM.DocDate, 106), ' ', '-') AS DocDate
                              , SPOM.CurrencyId, CU.Code AS CurrencyCode, SPOM.BaseCurrencyId, SPOM.PaymentTermId, SPOM.BaseNoOfDays
	                        , REPLACE(CONVERT(CHAR(11), SPOM.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), SPOM.MatureDate, 106), ' ', '-') AS MatureDate
                                , SPOM.FixedAssetOrInventory, SPOM.PODepended
	                        , SPOM.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, SPOM.InvoicingByAddress, SPOM.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, SPOM.DeliveryByAddress, SPOM.IsNonCreditable
	                        ,SPOM.ToCurrencyRate
	                        , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, SPOM.IsTaxApplicable
	                        , SPOM.IsApproved, SPOM.IsPaymentHold, SP.Id AS PlantStateId
	                        ,pgl.CtnId
	                        ,SPOM.AddedBy
	                        ,SPOM.CheckedByStatus AS CheckedByStatus
                            ,SPOM.ApprovedByStatus 
	                        --,ApprovedByStatus = CASE WHEN SPOM.ApprovedByStatus = 'Approval' THEN 'Approved' else '' END
                            ,eI.EmployeeName AS CheckedBy,eI1.EmployeeName AS ApprovedBy
	                        ,IRD.Amount
	                        ,IRD.TotalTaxAmount
                            ,SPOM.ApprovedHoldRejectReason
                            ,SPOM.CheckedHoldRejectReason
                        FROM[TRN].[ServicePOMaster] AS SPOM JOIN[HKP].[Party] AS P ON SPOM.PartyId=P.Id
                      LEFT JOIN (SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable
                      FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG

                      ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor'
                      ) AS CP ON CP.PartyId=SPOM.PartyId AND CP.PlantId= SPOM.PlantId

                      LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId= SPOM.CheckedBy

                      LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId= SPOM.ApprovedBy

                      left JOIN [SCS].[Currency] AS CU ON SPOM.CurrencyId= CU.Id

                      left JOIN [MST].[PaymentTerm] AS PT ON SPOM.PaymentTermId= PT.Id

                      LEFT JOIN [HKP].[PartyPlant] AS IPP ON SPOM.InvoicingPartyPlantId= IPP.Id

                      LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id

                      LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id

                      LEFT JOIN [HKP].[PartyPlant] AS DPP ON SPOM.DeliveryPartyPlantId= DPP.Id

                      LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id

                      LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id

                      LEFT JOIN [ORG].Plant PL ON PL.Id= SPOM.PlantId

                      LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id= PL.AddressMasterId

                      LEFT JOIN [SCS].[State] AS SP ON SP.Id= AMP.StateId

                      LEFT JOIN (SELECT A.ServicePOMasterId, sum(A.Amount) Amount, sum(A.TotalTaxAmount) TotalTaxAmount FROM[TRN].[ServicePODetail] AS A

                      left JOIN  [TRN].[ServicePOMaster] AS B ON A.ServicePOMasterId= B.Id WHERE B.PlantId= '" + identity.PlantId + @"'

                      GROUP BY A.ServicePOMasterId

                      ) AS IRD ON IRD.ServicePOMasterId=SPOM.Id
                      LEFT JOIN (Select count(Id) as CtnId, POID from TRN.ServicePurchaseOrderApprovalLog where Status= 'Approval' group by POID) as pgl on pgl.POID=SPOM.Id
                         Where SPOM.ApprovedBy='" + identity.EmployeeId + @"' 
                          And SPOM.[CheckedByStatus] Is Null
                          AND SPOM.[ApprovedByStatus]= 'Approved' ) X ";
                }


                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            // return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public void ServicePOCheckedAndApproved(string Id, string PoValue, string CheckedApprovedStataus, string CheckedApprovedBy, string RejectReason, string UIType)
        {
            var ApprovedById = "";
            if (UIType == "Service-PO-Checking")
            {
                try
                {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    var AuthorizedById = "";
                    var ApprovedByByStatus = "";

                    PoValue = "0";
                    //  var Id = GetPK();
                    if (CheckedApprovedStataus == "Checked")
                    {
                        CheckedApprovedStataus = "Checked";
                        if (CheckedApprovedBy == null || CheckedApprovedBy == "")
                        {
                            throw new CustomException("Select Approved By");
                        }
                        else if (identity.EmployeeId == CheckedApprovedBy)
                        {
                            throw new CustomException("You can't select same user");
                        }
                        ApprovedById = CheckedApprovedBy;
                        ApprovedByByStatus = "For Approval";
                        //DailySendMailRequisitionApproved(RequisitionType, RequirmentType, CheckedBy, AuthorizedById, PoId, PreparedBY);

                    }
                    else
                    {
                        ApprovedById = null;

                    }
                    var Status = CheckedApprovedStataus;
                    var UpdatedBy = "";
                    //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    var ip = identity.IPAddress;
                    var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                    var AddedBy = identity.Name;
                    var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                    var CompanyGroupId = identity.CompanyGroupId;
                    var CompanyId = identity.CompanyId;
                    var PlantId = identity.PlantId;
                    string _sql = "Update TRN.ServicePOMaster set CheckedByStatus='" + Status + "',ApprovedBy='" + ApprovedById + "',CheckedHoldRejectReason='" + RejectReason + "',ApprovedByStatus='" + ApprovedByByStatus + "' where id='" + Id + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                    string _sql1 = "Insert into TRN.ServicePurchaseOrderApprovalLog(" +
                    "CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,POValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,POID) " +
                    "values ('" + CompanyGroupId + "'," +
                    "'" + CompanyId + "'," +
                    "'" + PlantId + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + PoValue + "'," +
                    "'" + Status + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + ip + "'," +
                    "'" + UpdatedBy + "'," +
                    "'" + updatedDate + "', " +
                    "'" + ip + "','" + Id + "')";
                    _sqlRepository.ExecuteSqlCommand(_sql1);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
                }

            }
            else if (UIType == "Service-PO-Approval")
            {
                try
                {
                    var IsApproved = 0;

                    PoValue = "0";
                    //  var Id = GetPK();
                    if (CheckedApprovedStataus == "Approve")
                    {
                        CheckedApprovedStataus = "Approval";
                        IsApproved = 1;
                        ApprovedById = CheckedApprovedBy;
                    }
                    else
                    {
                        IsApproved = 0;
                        ApprovedById = null;
                    }
                    var Status = CheckedApprovedStataus;
                    var UpdatedBy = "";
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    var ip = identity.IPAddress;
                    var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                    var AddedBy = identity.Name;
                    var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                    var CompanyGroupId = identity.CompanyGroupId;
                    var CompanyId = identity.CompanyId;
                    var PlantId = identity.PlantId;
                    string _sql = "Update TRN.ServicePOMaster set ApprovedByStatus='" + Status + "',ApprovedHoldRejectReason='" + RejectReason + "' where id='" + Id + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                    string _sql1 = "Insert into TRN.ServicePurchaseOrderApprovalLog(" +
                    "CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,POValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,POID) " +
                    "values ('" + CompanyGroupId + "'," +
                    "'" + CompanyId + "'," +
                    "'" + PlantId + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + PoValue + "'," +
                    "'" + Status + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + ip + "'," +
                    "'" + UpdatedBy + "'," +
                    "'" + updatedDate + "', " +
                    "'" + ip + "','" + Id + "')";
                    _sqlRepository.ExecuteSqlCommand(_sql1);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
                }
            }

        }
        [Authorize, HttpGet]
        public JsonResult GetGatePassCheckedBy()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseOrderCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetGatePassApproveddBy()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, E.EmployeeCode+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='ServicePOApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetGatePassTobeApproveddSecurityBy()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseOrderApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion

        #region Service Acknowledgement Coding Start Here

        [HttpPost]
        public JsonResult CreateServiceAcknowledge(ServiceAcknowledgementMaster entity, IEnumerable<ServiceAcknowledgementViewModel> DetailList, string Status, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<ServicePOAckTax> ServicePOAndAckTax)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                if (Status == "Save")
                {
                    entity.Id = null;
                }
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.CompanyId = identity.CompanyId;
                entity.PlantId = identity.PlantId;
                entity.IsApproved = false;
                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if ((CheckedByStatusForNoti == "True" && ApprovedByStatusForNoti == "True") && string.IsNullOrEmpty(entity.CheckedBy))
                {
                    throw new CustomException("Please Set Check By and Approve by Name.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {
                    entity.ApprovedBy = entity.CheckedBy;
                    entity.ApprovedByStatus = "For Approval";
                    entity.CheckedBy = null;
                    entity.CheckedByStatus = null;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    entity.CheckedByStatus = null;
                    entity.ApprovedByStatus = null;
                    entity.CheckedBy = null;
                    entity.ApprovedBy = null;
                }
                else
                {
                    entity.CheckedBy = entity.CheckedBy;
                    entity.CheckedByStatus = "For Checking";
                    entity.ApprovedBy = null;
                    entity.ApprovedByStatus = null;
                }
                _purchaseOrderService.InsertServiceAck(entity, DetailList, ServicePOAndAckTax);
                return Json(new { entity, Message = AplosMessage.Success + " PO no <b>" + entity.Id + "</b>" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       
        [HttpPost]
        public ActionResult DeleteServiceAck(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _purchaseOrderService.DeleteServiceAck(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        [Authorize, HttpGet]
        public JsonResult GetListServiceAcknowledgementData(string plantId, string tabType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";


                if (tabType == "ForChecking")
                {
                    sql = @"
                            
                            Select * from (SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                               -- , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                        WHERE IR.CheckedByStatus='For Checking' AND IR.ApprovedByStatus IS NULL AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'
                   UNION ALL



                   SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                               -- , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                        WHERE IR.CheckedByStatus IS NULL AND IR.ApprovedByStatus ='For Approval'  AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'


                UNION ALL
                SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id  
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                               -- , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                        WHERE IR.CheckedByStatus IS NULL AND IR.ApprovedByStatus IS NULL AND IR.Id not in( Select ServicePOMasterId from trn.ServicePODetail where ServicePOMasterId IS NOT NULL) AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting') X";
                }
                else if (tabType == "CheckedHoldReject")
                {
                    sql = @"
                        
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                      WHERE IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' AND IR.ApprovedByStatus Is Null AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' ";


                }
                else if (tabType == "Checked")
                {
                    sql = @"
                        
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                       WHERE  IR.CheckedByStatus='Checked' AND IR.ApprovedByStatus= 'For Approval' AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' ";
                }
                else if (tabType == "ApprovedHoldReject")
                {
                    sql = @"
                       
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                      WHERE IR.ApprovedByStatus='Hold' OR IR.ApprovedByStatus='Reject' AND IR.CheckedByStatus='Checked'  AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' ";

                }
                else if (tabType == "Approved")
                {
                    sql = @"
                        
                        SELECT * from (   SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                --, IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                      WHERE IR.ApprovedByStatus='Approved'  AND IR.CheckedByStatus ='Checked' AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' 


UNION ALL

    SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                --, IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                      WHERE IR.ApprovedByStatus='Approved' AND IR.CheckedByStatus IS NULL  AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' 


             UNION ALL


     SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id 
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                --, IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                      WHERE IR.ApprovedByStatus IS NULL  AND IR.CheckedByStatus IS NULL    AND IR.Id  in( Select ServicePOMasterId from trn.ServicePODetail where ServicePOMasterId IS NOT NULL) AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' 
                  )X ";
                }
                else if (tabType == "Posted")
                {
                    sql = @"
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									,IR.CheckedByStatus
									, IR.NoteForAccounts,IRD.Amount,CheckedBy,IR.ApprovedByStatus,EI2.EmployeeName ApprovedBy,IR.ApprovedBy ApprovedById,IR.CheckedBy CheckedById
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.PreparedBy
LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                      WHERE IR.ApprovedByStatus='Approved' AND IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')='Posting'";

                }


                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            // return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceApprovedPO()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
              
                var sql = @"Select 
                             a.id
                            ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate 
                            ,P.Id PartyId
                            ,p.UserName PartyName 
                            ,a.DocRefNo
                            ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') DocDate
                            ,C.Code
                            ,b.Amount ,a.IsNonCreditable,0 Active
                            ,C.Id CurrencyId
                            ,a.ToCurrencyRate
                            ,a.BaseNoOfDays
                            ,a.BaseOnDueDate
                            ,a.BaseCurrencyId
                            ,a.MatureDate
                            , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                             ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                            FROM TRN.ServicePOMaster a
                            LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                            LEFT JOIN hkp.Party p On P.id=a.PartyId
                            LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
                            LEFT JOIN TRN.ServiceAcknowledgementMaster SAMS ON  SAMS.ServicePOId=a.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.ID=a.PaymentTermId
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
                            LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                            LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
                            Where a.CheckedByStatus='Checked' AND a.ApprovedByStatus='Approved'
                            AND IsNull (PT.PaymentMode,'') <>'LC' 
                            --AND a.Id NOT IN (Select ServicePOMasterId Id from TRN.ServiceAcknowledgementDetail where ServicePOMasterId is not null)

                            UNION All
                            Select 
                             a.id
                            ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate 
                            ,P.Id PartyId
                            ,p.UserName PartyName 
                            ,a.DocRefNo
                            ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') DocDate
                            ,C.Code
                            ,b.Amount ,a.IsNonCreditable,0 Active
                            ,C.Id CurrencyId
                            ,a.ToCurrencyRate
                            ,a.BaseNoOfDays
                            ,a.BaseOnDueDate
                            ,a.BaseCurrencyId
                            ,a.MatureDate
                            , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                             ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                            FROM TRN.ServicePOMaster a
                            LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                            LEFT JOIN hkp.Party p On P.id=a.PartyId
                            LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
                            LEFT JOIN TRN.ServiceAcknowledgementMaster SAMS ON  SAMS.ServicePOId=a.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.ID=a.PaymentTermId
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
                            LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                            LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
                            Where a.CheckedByStatus Is null AND a.ApprovedByStatus='Approved'
                            AND IsNull (PT.PaymentMode,'') <>'LC' 
                            --AND a.Id NOT IN (Select ServicePOMasterId Id from TRN.ServiceAcknowledgementDetail where ServicePOMasterId is not null)
                            UNION All
                            Select 
                             a.id
                            ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate 
                            ,P.Id PartyId
                            ,p.UserName PartyName 
                            ,a.DocRefNo
                            ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') DocDate
                            ,C.Code
                            ,b.Amount ,a.IsNonCreditable,0 Active
                            ,C.Id CurrencyId
                            ,a.ToCurrencyRate
                            ,a.BaseNoOfDays
                            ,a.BaseOnDueDate
                            ,a.BaseCurrencyId
                            ,a.MatureDate
                            , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                             ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                            FROM TRN.ServicePOMaster a
                            LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                            LEFT JOIN hkp.Party p On P.id=a.PartyId
                            LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
                            LEFT JOIN TRN.ServiceAcknowledgementMaster SAMS ON  SAMS.ServicePOId=a.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.ID=a.PaymentTermId
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
                            LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                            LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
                            Where a.CheckedByStatus Is null AND a.ApprovedByStatus IS NULL
                            AND IsNull (PT.PaymentMode,'') <>'LC' 
                            --AND a.Id NOT IN (Select ServicePOMasterId Id from TRN.ServiceAcknowledgementDetail where ServicePOMasterId is not null)


                            UNION ALL
                            Select 
                             a.id
                            ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate 
                            ,P.Id PartyId
                            ,p.UserName PartyName 
                            ,a.DocRefNo
                            ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') DocDate
                            ,C.Code
                            ,b.Amount ,a.IsNonCreditable,0 Active
                            ,C.Id CurrencyId
                            ,a.ToCurrencyRate
                            ,a.BaseNoOfDays
                            ,a.BaseOnDueDate
                            ,a.BaseCurrencyId
                            ,a.MatureDate
                            , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                             ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                            FROM TRN.ServicePOMaster a
                            LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                            LEFT JOIN hkp.Party p On P.id=a.PartyId
                            LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
                            LEFT JOIN TRN.ServiceAcknowledgementMaster SAMS ON  SAMS.ServicePOId=a.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.ID=a.PaymentTermId
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=a.PurchaseLCId  
                            LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                            LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
                            Where a.CheckedByStatus='Checked' AND a.ApprovedByStatus='Approved'
                            AND isnull(PT.PaymentMode,'') = 'LC' and PLC.IsAccepptanceFirst=0
                            --AND a.Id NOT IN (Select ServicePOMasterId Id from TRN.ServiceAcknowledgementDetail where ServicePOMasterId is not null)

				
                            UNION ALL
                            Select 
                             a.id
                            ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate 
                            ,P.Id PartyId
                            ,p.UserName PartyName 
                            ,a.DocRefNo
                            ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') DocDate
                            ,C.Code
                            ,b.Amount ,a.IsNonCreditable,0 Active
                            ,C.Id CurrencyId
                            ,a.ToCurrencyRate
                            ,a.BaseNoOfDays
                            ,a.BaseOnDueDate
                            ,a.BaseCurrencyId
                            ,a.MatureDate
                            , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                             ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                            FROM TRN.ServicePOMaster a
                            LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                            LEFT JOIN hkp.Party p On P.id=a.PartyId
                            LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
                            LEFT JOIN TRN.ServiceAcknowledgementMaster SAMS ON  SAMS.ServicePOId=a.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.ID=a.PaymentTermId
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=a.PurchaseLCId  
                            LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                            LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
                            Where a.CheckedByStatus Is null AND a.ApprovedByStatus='Approved'
                            AND isnull(PT.PaymentMode,'') = 'LC' and PLC.IsAccepptanceFirst=0
                            --AND a.Id NOT IN (Select ServicePOMasterId Id from TRN.ServiceAcknowledgementDetail where ServicePOMasterId is not null)
				
                            UNION ALL
                            Select 
                             a.id
                            ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate 
                            ,P.Id PartyId
                            ,p.UserName PartyName 
                            ,a.DocRefNo
                            ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') DocDate
                            ,C.Code
                            ,b.Amount ,a.IsNonCreditable,0 Active
                            ,C.Id CurrencyId
                            ,a.ToCurrencyRate
                            ,a.BaseNoOfDays
                            ,a.BaseOnDueDate
                            ,a.BaseCurrencyId
                            ,a.MatureDate
                            , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                             ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                            FROM TRN.ServicePOMaster a
                            LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                            LEFT JOIN hkp.Party p On P.id=a.PartyId
                            LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
                            LEFT JOIN TRN.ServiceAcknowledgementMaster SAMS ON  SAMS.ServicePOId=a.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.ID=a.PaymentTermId
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=a.PurchaseLCId  
                            LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                            LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
                            Where a.CheckedByStatus Is null AND a.ApprovedByStatus IS NULL
                            AND isnull(PT.PaymentMode,'') = 'LC' and PLC.IsAccepptanceFirst=0
                            --AND a.Id NOT IN (Select ServicePOMasterId Id from TRN.ServiceAcknowledgementDetail where ServicePOMasterId is not null)

							UNION ALL
                            Select 
                             a.id
                            ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate 
                            ,P.Id PartyId
                            ,p.UserName PartyName 
                            ,a.DocRefNo
                            ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') DocDate
                            ,C.Code
                            ,b.Amount ,a.IsNonCreditable,0 Active
                            ,C.Id CurrencyId
                            ,a.ToCurrencyRate
                            ,a.BaseNoOfDays
                            ,a.BaseOnDueDate
                            ,a.BaseCurrencyId
                            ,a.MatureDate
                            , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                             ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                            FROM TRN.ServicePOMaster a
                            LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                            LEFT JOIN hkp.Party p On P.id=a.PartyId
                            LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
                            LEFT JOIN TRN.ServiceAcknowledgementMaster SAMS ON  SAMS.ServicePOId=a.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.ID=a.PaymentTermId
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=a.PurchaseLCId  
                            LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                            LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
                            Where a.CheckedByStatus Is null AND a.ApprovedByStatus IS NULL
                            AND isnull(PT.PaymentMode,'') = 'LC' 
                            --AND a.Id NOT IN (Select ServicePOMasterId Id from TRN.ServiceAcknowledgementDetail where ServicePOMasterId is not null)


							UNION ALL
                            Select 
                             a.id
                            ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate 
                            ,P.Id PartyId
                            ,p.UserName PartyName 
                            ,a.DocRefNo
                            ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') DocDate
                            ,C.Code
                            ,b.Amount ,a.IsNonCreditable,0 Active
                            ,C.Id CurrencyId
                            ,a.ToCurrencyRate
                            ,a.BaseNoOfDays
                            ,a.BaseOnDueDate
                            ,a.BaseCurrencyId
                            ,a.MatureDate
                            , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                             ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                            FROM TRN.ServicePOMaster a
                            LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                            LEFT JOIN hkp.Party p On P.id=a.PartyId
                            LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
                            LEFT JOIN TRN.ServiceAcknowledgementMaster SAMS ON  SAMS.ServicePOId=a.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.ID=a.PaymentTermId
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=a.PurchaseLCId  
                            LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                            LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
                            Where a.CheckedByStatus Is null AND a.ApprovedByStatus='Approved'
                            AND isnull(PT.PaymentMode,'') = 'LC' 
                            --AND a.Id NOT IN (Select ServicePOMasterId Id from TRN.ServiceAcknowledgementDetail where ServicePOMasterId is not null)


							UNION ALL
                            Select 
                             a.id
                            ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate 
                            ,P.Id PartyId
                            ,p.UserName PartyName 
                            ,a.DocRefNo
                            ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') DocDate
                            ,C.Code
                            ,b.Amount ,a.IsNonCreditable,0 Active
                            ,C.Id CurrencyId
                            ,a.ToCurrencyRate
                            ,a.BaseNoOfDays
                            ,a.BaseOnDueDate
                            ,a.BaseCurrencyId
                            ,a.MatureDate
                            , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                             ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                            FROM TRN.ServicePOMaster a
                            LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                            LEFT JOIN hkp.Party p On P.id=a.PartyId
                            LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
                            LEFT JOIN TRN.ServiceAcknowledgementMaster SAMS ON  SAMS.ServicePOId=a.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.ID=a.PaymentTermId
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId= AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId= S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId= AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId= S2.Id
                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=a.PurchaseLCId  
                            LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                            LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
                            Where a.CheckedByStatus='Checked' AND a.ApprovedByStatus='Approved'
                            AND isnull(PT.PaymentMode,'') = 'LC' 
                            --AND a.Id NOT IN (Select ServicePOMasterId Id from TRN.ServiceAcknowledgementDetail where ServicePOMasterId is not null)
							ORDER BY a.id DESC";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion
        #region Independent Service Aknowledge
        [Authorize, HttpGet]
        public JsonResult GetListIndependentServiceAcknowledgementData(string tabType)
        {
            return Json(_serviceRequsitionMasterService.GetListIndependentServiceAcknowledgementData(tabType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateIndependentServiceAcknowledge(ServiceAcknowledgementMaster entity, string Status, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                if (Status == "Save")
                {
                    entity.Id = null;
                }
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.CompanyId = identity.CompanyId;
                entity.PlantId = identity.PlantId;
                entity.IsApproved = false;
                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if ((CheckedByStatusForNoti == "True" && ApprovedByStatusForNoti == "True") && string.IsNullOrEmpty(entity.CheckedBy))
                {
                    throw new CustomException("Please Set Check By and Approve by Name.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {
                    entity.ApprovedBy = entity.CheckedBy;
                    entity.ApprovedByStatus = "For Approval";
                    entity.CheckedBy = null;
                    entity.CheckedByStatus = null;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    entity.CheckedByStatus = null;
                    entity.ApprovedByStatus = null;
                    entity.CheckedBy = null;
                    entity.ApprovedBy = null;
                }
                else
                {
                    entity.CheckedBy = entity.CheckedBy;
                    entity.CheckedByStatus = "For Checking";
                    entity.ApprovedBy = null;
                    entity.ApprovedByStatus = null;
                }
                _purchaseOrderService.InsertIndependentServiceAck(entity);
                return Json(new { entity, Message = AplosMessage.Success + " Service No <b>" + entity.Id + "</b>" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Survice Acknowledgement Report 
        [HttpGet, Authorize]
        public ActionResult ServiceAcknowledgementReport(string SurviceAckId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _purchaseOrderService.ServiceAcknowledgementReport(identity.CompanyGroupId, identity.PlantId, SurviceAckId);

            return null;

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
                FROM trn. ServicePODetail a
                LEFT join trn.ServicePOMaster d on d.id=a.ServicePOMasterId
                Left join hkp.ServiceMaster b on a.ServiceMasterId=b.id
                left join(select ServicePODetailId,sum(TaxAmount) TaxAmount from trn.ServicePOTax group by ServicePODetailId)c On c.ServicePODetailId=a.id
				left join scs.UnitOfMeasurement UOM ON A.TransactionUoMId=UOM.Id
				left join(select ServicePODetailId,sum(Qty) Qty from trn.ServivePOAcknowledgementMap group by ServicePODetailId)Mapdata On Mapdata.ServicePODetailId=a.id 
                where " + paramter + @"";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        [Authorize, HttpGet]
        public JsonResult ServicePOAcknowledgementCheckedBy()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='ServicePOAcknowledgementCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        [Authorize, HttpGet]
        public JsonResult ServicePOAcknowledgementApproveBy()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, E.EmployeeCode+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='ServiceAcknowledgementApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion


        [Authorize, HttpGet]
        public JsonResult GetSavedPOList1(string AckId)
        {
            var Sql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                Sql = @"Select 
                 distinct a.id
                ,REPLACE(CONVERT(CHAR(11), a.PODate, 106),' ','-') AS PODate
                ,P.Id PartyId
                ,p.UserName PartyName 
                ,a.DocRefNo
                ,REPLACE(CONVERT(CHAR(11), a.DocDate, 106),' ','-') AS DocDate                            
                ,C.Code
                ,b.Amount ,a.IsNonCreditable,0 Active
                ,C.Id CurrencyId
                ,a.ToCurrencyRate
                ,a.BaseNoOfDays
				,a.BaseOnDueDate
				,a.BaseCurrencyId
				,a.MatureDate
                , a.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, a.InvoicingByAddress, a.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, a.DeliveryByAddress, a.IsNonCreditable
                ,con.ContractNo, P.UserName AS CustomerName,MLC.LCRef LCNo,IPP.GSTIN,C.Code CurrencyName,CON.UDNo
                FROM TRN.ServicePOMaster a
                LEFT join (Select ServicePOMasterId,Sum(Amount) Amount from TRN.ServicePODetail group by ServicePOMasterId) b On b.ServicePOMasterId=a.Id
                LEFT JOIN hkp.Party p On P.id=a.PartyId
                LEFT JOIn [SCS].[Currency] C on C.Id=a.CurrencyId 
				LEFT JOIN trn.ServiceAcknowledgementDetail sad on sad.ServicePOMasterId=a.id
				LEFT JOIN trn.ServiceAcknowledgementMaster sad1 on sad1.Id=sad.ServiceAcknowledgementMasterId
				LEFT JOIN [dbo].[Contract] CON on CON.Id= a.ContractId
                LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId
			    LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=CON.MasterLCId--MLC ON MLC.ContractId=C.Id
			    LEFT JOIN [HKP].[PartyPlant] AS IPP ON a.InvoicingPartyPlantId= IPP.Id
				   LEFT JOIN [HKP].[PartyPlant] AS DPP ON a.DeliveryPartyPlantId= DPP.Id
                Where --a.ApprovedByStatus='Approval' And a.Id Not in(select ServicePOMasterId from trn.ServiceAcknowledgementDetail)  And 
				sad1.PlantId='" + identity.PlantId + @"' and sad.ServiceAcknowledgementMasterId='" + AckId + @"'";

                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceLisrByAckid(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"Select 
                a.Id ServicePODetailId,a.ServicePOMasterId
                ,b.Id ServiceMasterId
                ,b.UserName ServiceMasterName
                , a.Amount 
                ,Tax.TaxAmount TotalTaxAmount
                ,0 [check]
                ,d.IsNonCreditable
                ,a.TotalAmount,a.Qty CurrentQty,a.Rate,a.TransactionUoMId,UOM.UserName UoM
				,SPO.Qty,mapData.Qty OtherReceived,mapData1.MapId MapId,SPO.Id ServicePoDelId,Balance=(Isnull(SPO.Qty,0)-(isnull(mapData.Qty,0)+isnull(a.Qty,0)))
                FROM trn. ServiceAcknowledgementDetail a
                LEFT join trn.ServiceAcknowledgementMaster d on d.id=a.ServiceAcknowledgementMasterId
                Left join hkp.ServiceMaster b on a.ServiceMasterId=b.id
                LEFT JOIN (select ServiceAcknowledgementDetailId, sum(TaxAmount) TaxAmount from trn.ServicePOAckTax Group By ServiceAcknowledgementDetailId
                
				) Tax ON Tax.ServiceAcknowledgementDetailId=a.Id
				left JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.TransactionUoMId
				left JOIN trn.ServicePODetail SPO ON SPO.Id=a.ServicePODetailId
				left JOIN (Select ServicePoDetailId,sum(Qty) Qty from trn.ServivePOAcknowledgementMap where ServiceAckId<>'" + Id + @"' Group by ServicePoDetailId) mapData on mapData.ServicePoDetailId=SPO.Id
				left JOIN (Select Id MapId,ServicePoDetailId, sum(Qty) Qty from trn.ServivePOAcknowledgementMap where ServiceAckId='" + Id + @"' Group by Id,ServicePoDetailId ) mapData1 on mapData1.ServicePoDetailId=SPO.Id
				where d.Id='" + Id + @"'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [Authorize, HttpPost]
        public JsonResult DeleteServiceAckRow(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var rdBuilder = new System.Text.StringBuilder();
                var voucherSql = @"delete from trn.ServicePOAckTax where ServiceAcknowledgementDetailId='" + Id + "'";
                var bankJournalSql = @"delete from trn.ServiceAcknowledgementDetail where id='" + Id + "'";
                rdBuilder.Append(voucherSql);
                rdBuilder.Append(bankJournalSql);
                return Json(_sqlRepository.ExecuteSqlCommand(rdBuilder.ToString()), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [Authorize, HttpPost]
        public JsonResult DeleteServiceAckChargesRow(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var rdBuilder = new System.Text.StringBuilder();
                var voucherSql = @"delete from trn.ServicePOAckTax where ServiceAcknowledgementChargeId='" + Id + "'";
                var bankJournalSql = @"delete from trn.ServiceAcknowledgementCharge where id='" + Id + "'";
                rdBuilder.Append(voucherSql);
                rdBuilder.Append(bankJournalSql);
                return Json(_sqlRepository.ExecuteSqlCommand(rdBuilder.ToString()), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [Authorize, HttpGet]
        public JsonResult LoadAllAckServicesData(string id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var Sql = @"Select 
                        sad.Id
	                    ,s.UserName ServiceName
	                    ,sad.Amount
	                    ,c.Code
	                    ,sad.TotalTaxAmount
	                    ,sad.TotalAmount,sad.ServiceAcknowledgementMasterId,sad.Qty,sad.Rate,sad.TransactionUoMId,UOM.UserName UoM
                    FROM TRN.ServiceAcknowledgementDetail sad
                    LEFT JOIN trn.ServiceAcknowledgementMaster sad1 on sad1.Id=sad.ServiceAcknowledgementMasterId
                    LEFT JOIn [SCS].[Currency] C on C.Id=sad1.CurrencyId 
                    LEFT JOIn hkp.ServiceMaster s On s.id=sad.ServiceMasterId
					left JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=sad.TransactionUoMId";

                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        [HttpGet, Authorize]
        public JsonResult GetCheckedApprovedListserviceack(string tabType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                if (tabType == "UnCheckedList")
                {
                    sql = @"
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									, IR.NoteForAccounts,IRD.Amount
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason

	                        ,IR.ApprovedByStatus AS ApprovedByStatus
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                              Where IR.CheckedBy='" + identity.EmployeeId + @"' 
                              --AND IR.CheckedBy Is NOT NULL 
                              And IR.[CheckedByStatus]='For Checking' 
                             ";

                }
                else if (tabType == "HoldRejectCheckedList")
                {
                    sql = @"
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									, IR.NoteForAccounts,IRD.Amount	   
                                  	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy ,IR.CheckedHoldRejectReason
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                          Where IR.CheckedBy='" + identity.EmployeeId + @"' 
                          AND IR.CheckedBy Is NOT NULL 
                          And IR.[CheckedByStatus]='Hold' OR IR.[CheckedByStatus]='Reject'
                          AND IR.[ApprovedBy] IS NULL OR IR.ApprovedBy = ''
                          AND IR.[ApprovedByStatus] Is NULL";
                }
                else if (tabType == "CheckedList")
                {
                    sql = @"
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									, IR.NoteForAccounts,IRD.Amount	
									,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy ,IR.CheckedHoldRejectReason

                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                              Where IR.CheckedBy='" + identity.EmployeeId + @"' 
                          
                          And IR.[CheckedByStatus]='Checked' 
                          ";
                }
                else if (tabType == "UnApprovedList")
                {
                    sql = @"
                      Select * from (SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                --, IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									, IR.NoteForAccounts,IRD.Amount
							        ,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy ,IR.ApprovedHoldRejectReason


                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                          Where IR.ApprovedBy='" + identity.EmployeeId + @"' 
                          And IR.[CheckedByStatus]='Checked' 
                          AND IR.[ApprovedByStatus]='For Approval' 
						  UNION ALL

						  SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                --, IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									, IR.NoteForAccounts,IRD.Amount
							        ,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy ,IR.ApprovedHoldRejectReason


                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                          Where IR.ApprovedBy='" + identity.EmployeeId + @"' 
                          And IR.[CheckedByStatus] is null 
                          AND IR.[ApprovedByStatus]='For Approval'
						  )X";
                }
                else if (tabType == "HoldRejectApprovedList")
                {
                    sql = @"
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									, IR.NoteForAccounts,IRD.Amount
		                            ,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy ,IR.ApprovedHoldRejectReason
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                          Where IR.ApprovedBy='" + identity.EmployeeId + @"' 
                          AND IR.CheckedBy Is NOT NULL 
                          And IR.[CheckedByStatus]='Checked' 
                          AND IR.[ApprovedBy] IS NOT NULL 
                          AND IR.[ApprovedByStatus]='Hold' Or  IR.[ApprovedByStatus]='Reject'
                          ";
                }
                else if (tabType == "ApprovedList")
                {
                    sql = @"
                 Select * from (   SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                --, IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									, IR.NoteForAccounts,IRD.Amount
							        ,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy ,IR.ApprovedHoldRejectReason
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                          Where IR.ApprovedBy='" + identity.EmployeeId + @"' 
                          And IR.[CheckedByStatus]='Checked' 
                          AND IR.[ApprovedByStatus]='Approved'
                        UNION ALL
                  SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.AcknowledgementDate, 106),' ','-') AS AcknowledgementDate
                                    -- ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                --, IR.AcknowledgementDate
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
									, IR.PODepended
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
									, IR.NoteForAccounts,IRD.Amount
							        ,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy ,IR.ApprovedHoldRejectReason
                        FROM [TRN].ServiceAcknowledgementMaster AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, sum(A.Amount) Amount
						 FROM [TRN].ServiceAcknowledgementDetail AS A
		                            JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                        LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId FROM [TRN].ServiceAcknowledgementDetail AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.ServiceAcknowledgementMasterId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.ServiceAcknowledgementMasterId  HAVING COUNT(A.ServiceAcknowledgementMasterId)> 
									COUNT(A.ServiceMasterId)) 
									AS TU ON TU.ServiceAcknowledgementMasterId=IR.Id
                          Where IR.ApprovedBy='" + identity.EmployeeId + @"' 
                          
                          And IR.[CheckedByStatus] Is Null
                          AND IR.[ApprovedByStatus]='Approved')X";
                }
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [HttpPost, Authorize]
        public void ServiceAckCheckedAndApproved(string Id, string PoValue, string CheckedApprovedStataus, string CheckedApprovedBy, string RejectReason, string UIType)
        {
            var ApprovedById = "";
            if (UIType == "Service-Ack-Checked")
            {
                try
                {
                    var ApprovedByStatus = "";
                    PoValue = "0";
                    if (CheckedApprovedStataus == "Checked")
                    {
                        if (CheckedApprovedBy == null || CheckedApprovedBy == "")
                        {
                            throw new CustomException("Select Approved By");
                        }
                        ApprovedById = CheckedApprovedBy;
                        ApprovedByStatus = "For Approval";
                    }
                    else
                    {
                        ApprovedById = null;
                    }
                    var Status = CheckedApprovedStataus;
                    var UpdatedBy = "";
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    var ip = identity.IPAddress;
                    var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                    var AddedBy = identity.Name;
                    var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                    var CompanyGroupId = identity.CompanyGroupId;
                    var CompanyId = identity.CompanyId;
                    var PlantId = identity.PlantId;
                    string _sql = "Update TRN.ServiceAcknowledgementMaster set CheckedByStatus='" + Status + "',ApprovedBy='" + ApprovedById + "',CheckedHoldRejectReason='" + RejectReason + "',ApprovedByStatus='" + ApprovedByStatus + "' where id='" + Id + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                    string _sql1 = "Insert into TRN.ServiceAckApprovalLog(" +
                    "CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,POValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,POid) " +
                    "values ('" + CompanyGroupId + "'," +
                    "'" + CompanyId + "'," +
                    "'" + PlantId + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + PoValue + "'," +
                    "'" + Status + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + ip + "'," +
                    "'" + UpdatedBy + "'," +
                    "'" + updatedDate + "', " +
                    "'" + ip + "','" + Id + "')";
                    _sqlRepository.ExecuteSqlCommand(_sql1);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
                }
            }
            else if (UIType == "Service-Ack-Approval")
            {
                try
                {
                    var IsApproved = 0;
                    PoValue = "0";
                    //  var Id = GetPK();
                    if (CheckedApprovedStataus == "Approval")
                    {
                        IsApproved = 1;
                        ApprovedById = CheckedApprovedBy;
                    }
                    else
                    {
                        IsApproved = 0;
                        ApprovedById = null;
                    }
                    var Status = CheckedApprovedStataus;
                    var UpdatedBy = "";
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    var ip = identity.IPAddress;
                    var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                    var AddedBy = identity.Name;
                    var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                    var CompanyGroupId = identity.CompanyGroupId;
                    var CompanyId = identity.CompanyId;
                    var PlantId = identity.PlantId;
                    string _sql = "Update TRN.ServiceAcknowledgementMaster set ApprovedByStatus='" + Status + "',ApprovedHoldRejectReason='" + RejectReason + "' where id='" + Id + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                    string _sql1 = "Insert into TRN.ServiceAckApprovalLog(" +
                    "CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,POValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,POId) " +
                    "values ('" + CompanyGroupId + "'," +
                    "'" + CompanyId + "'," +
                    "'" + PlantId + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + PoValue + "'," +
                    "'" + Status + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + ip + "'," +
                    "'" + UpdatedBy + "'," +
                    "'" + updatedDate + "', " +
                    "'" + ip + "','" + Id + "')";
                    _sqlRepository.ExecuteSqlCommand(_sql1);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
                }
            }
        }

        [Authorize, HttpGet]
        public JsonResult PaymentModeByPaymentTerm(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string _sql = "select * from [MST].[PaymentTerm] where Id='" + Id + "'";
                //_sqlRepository.ExecuteSqlCommand(_sql);

                return Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public ActionResult DeleteTitle(string id)
        {
            try
            {

                string ret = tg.DeletePOTitle(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }
        string TableName = "hkp.TermsAndConditions";
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }
        [Authorize, HttpGet]
        public JsonResult TermsAndConditions()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string _sql = "select Id,Description,UserName TermsAndConditions from HKP.TermsAndConditions where Type='PO' And CompanyId='" + identity.CompanyId + @"'";
                //_sqlRepository.ExecuteSqlCommand(_sql);

                return Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        [HttpGet, Authorize]
        public JsonResult GetPopUp(string TermsAndConditionsPODetailId)
        {
            try
            {
                return Json(control.GetTermsAndConditionPOPopUp(TermsAndConditionsPODetailId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetTermsAndConditionsList(string TermsAndConditionMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select TCM.Id TermsAndConditionMasterId,TC.Id TermsAndConditionChildId,TC.Id,TC.Title,TCM.Description ,TCM.Code  from TermsAndConditionsChild TC 
left outer join HKP.TermsAndConditions TCM on TCM.Id=TC.TermsAndConditionsMasterId 
where TC.TermsAndConditionsMasterId='" + TermsAndConditionMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public ActionResult GetTermsAndConditionsPOList(string TermsAndConditionMasterId, string POId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select TC.Id TermsAndConditionPOChildId,TC.Id,TC.Title
from TermsAndConditionsPOChild TC
WHERE TC.POId='" + POId + @"' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetTermsAndConditionsDetailList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select TCD.Id,TC.Id TermsAndConditionChildId,TCD.HeaderCaption ,TCD.Description  from TermsAndConditionsDetails TCD 
left outer join TermsAndConditionsChild TC on TC.Id=TCD.TermsAndConditionsChildId";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetTermsAndConditionsPODetailList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select TCD.Id,TC.Id TermsAndConditionPOChildId,TCD.HeaderCaption ,TCD.Description  from TermsAndConditionsPODetails TCD 
left outer join TermsAndConditionsPOChild TC on TC.Id=TCD.TermsAndConditionsPOChildId ORDER BY TCD.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult UpdateMaterialSequence(List<string> data)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data changed!!!");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                for (int i = 0; i < data.Count; i++)
                {
                    con.executeQuery("UPDATE TermsAndConditionsPODetails SET Sequence=" + (i + 1) + " where id='" + data[i] + "'");
                }

                con.CommitTransaction();

                return Json(new { Error = false, Message = "Sequence updated successfully" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveTitle(Dictionary<string, object> TitleData, string TitleId, List<Dictionary<string, object>> TermsAndConditionGridList)
        {
            try
            {
                DataView dvGrid = null;
                DataRow drGrid = null;
                DataTable dtGrid = null;
                ConnectionManager.DAL.ConManager conTitle = new ConnectionManager.DAL.ConManager("1");
                conTitle.OpenDataSetThroughAdapter("select * from dbo.TermsAndConditionsPOChild where 1=2 ", out DataSet dsTitle, false, "1");
                dtGrid = dsTitle.Tables[0];
                string _Id = "";
                foreach (var item in TermsAndConditionGridList)
                {
                    dvGrid = new DataView(dsTitle.Tables[0]);
                    dvGrid.RowFilter = "TermsAndConditionChildId= '" + item["TermsAndConditionChildId"] + "'";
                    if (dvGrid.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.TermsAndConditionsPOChild", out _Id);
                        _Id = "TC" + _Id;

                        drGrid = dtGrid.NewRow();

                        drGrid["Id"] = _Id;
                        drGrid["Title"] = item[""].ToString(); ;
                        dtGrid.Rows.Add(drGrid);
                        //AddNewRow(dsTitle.Tables[0], TitleData);
                    }
                    else
                    {
                        drGrid = dvGrid[0].Row;
                        drGrid.BeginEdit();

                        drGrid["Title"] = item[""].ToString(); ;

                        drGrid.EndEdit();
                    }
                }
                #region data update
                //if (dsTitle.Tables[0].Rows.Count == 0)
                //{

                //	bplib.clsGenID genid = new bplib.clsGenID();
                //	genid.GenID("dbo.TermsAndConditionsPOChild", out _Id);
                //	_Id = "TC" + _Id;
                //	TitleData["Id"] = _Id;
                //	TitleData["Title"] = TermsAndConditionGridList[].ToString(); ;

                //	AddNewRow(dsTitle.Tables[0], TitleData);
                //}
                //else
                //{
                //	_Id = TitleData["Id"].ToString();
                //	EditRow(dsTitle.Tables[0].Rows[0], TitleData);
                //}
                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTitle);
                //_info.SaveDataSets(dsTitle);

                return Json(new { Error = false, Data = TitleData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost]
        public ActionResult SaveTermsDetail(string TitleId, string POId)
        {
            DataSet dsToSalesOrder;
            DataSet dsToFirstCharacteristics;
            try
            {

                //if (TitleId == null)
                //{
                //	throw new Exception("Please select Terms and Condition..");
                //}


                string Id = "";
                DataSet dsSOId;
                //GetSOId(MasterId, out dsSOId);
                //string NewId = dsSOId.Tables[0].Rows[0]["Id"].ToString();
                string NewSoId = string.Empty;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM TermsAndConditionsPOChild WHERE 1=2", out dsToSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM TermsAndConditionsPODetails WHERE 1=2", out dsToFirstCharacteristics, false, "1");

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM  TermsAndConditionsChild WHERE TermsAndConditionsMasterId='" + TitleId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TermsAndConditionsDetails Where TermsAndConditionsChildId IN(Select Id from TermsAndConditionsChild Where TermsAndConditionsMasterId='" + TitleId + "')");

                int SCount = 0;
                objGenID.GenerateIDAuto("dbo.TermsAndConditionsPOChild", out Id);

                for (int m = 0; m < dtFromMaster.Rows.Count; m++)
                {
                    SCount++;
                    DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                    CopyRow(dtFromMaster.Rows[m], ref drSalesOrder);
                    drSalesOrder["Id"] = TitleId + Convert.ToInt32(Id) + SCount;
                    NewSoId = drSalesOrder["Id"].ToString();
                    drSalesOrder["TermsAndConditionsMasterId"] = TitleId;
                    drSalesOrder["POId"] = POId;
                    dsToSalesOrder.Tables[0].Rows.Add(drSalesOrder);

                    dtFromFirstCharacteristics.DefaultView.RowFilter = "TermsAndConditionsChildId='" + dtFromMaster.Rows[m]["Id"].ToString() + "'";
                    for (int i = 0; i < dtFromFirstCharacteristics.DefaultView.Count; i++)
                    {
                        DataRow drFirstCharacteristics = dsToFirstCharacteristics.Tables[0].NewRow();
                        CopyRow(dtFromFirstCharacteristics.DefaultView[i].Row, ref drFirstCharacteristics);
                        drFirstCharacteristics["Id"] = NewSoId + (i + 1);
                        drFirstCharacteristics["TermsAndConditionsPOChildId"] = NewSoId;

                        dsToFirstCharacteristics.Tables[0].Rows.Add(drFirstCharacteristics);
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsToSalesOrder, dsToFirstCharacteristics);
                return Json(new { Error = false, Message = AplosMessage.Insert });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult DeletePOMaster(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var rdBuilder = new System.Text.StringBuilder();
                var poDetailSql = _purchseOrderDetailService.Query(r=>r.InventoryReceiveId== Id).Select().FirstOrDefault();
                if (poDetailSql == null)
                {
                    var PoSql = @"delete from trn.PurchaseOrder where Id='" + Id + "'";
                    rdBuilder.Append(PoSql);
                }
                else
                {
                    throw new Exception("Detail have to Delete First !!!");
                }
               
                return Json(_sqlRepository.ExecuteSqlCommand(rdBuilder.ToString()), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public ActionResult DeletePODetailPOPup(string id)
        {
            try
            {

                string ret = tg.DeletePODetailPopUp(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false/*, Sequence = GetSequence()*/, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

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


        [HttpPost]
        public JsonResult SaveData(Dictionary<string, object> GridData, string titleId)
        {
            try
            {

                DataSet dsGrid;

                ConnectionManager.DAL.ConManager conBin = new ConnectionManager.DAL.ConManager("1");
                conBin.OpenDataSetThroughAdapter("select top 1 Sequence from dbo.TermsAndConditionsPODetails where TermsAndConditionsPOChildId='" + titleId + "' order by AddedDate desc", out DataSet dsGridSeq, false, "1");
                conBin.OpenDataSetThroughAdapter("select * from dbo.TermsAndConditionsPODetails where TermsAndConditionsPOChildId='" + titleId + "'", out dsGrid, false, "1");
                string DetailId = "";
                int count = 0;
                DataView dv = new DataView(dsGrid.Tables[0]);
                dv.RowFilter = "Id='" + GridData["Id"] + "'";

                if (dv.Count == 0)
                {
                    if (DetailId == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.TermsAndConditionsPODetails", out DetailId);
                    }
                    if (dsGridSeq.Tables[0].Rows.Count == 0)
                    {
                        count++;
                    }
                    else
                    {
                        count = (int)clsStaticInfo.dbl(dsGridSeq.Tables[0].Rows[0]["Sequence"].ToString()) + 1;
                    }
                    DataRow dr = dsGrid.Tables[0].NewRow();

                    GridData["Id"] = "TD-" + DetailId;
                    GridData["TermsAndConditionsPOChildId"] = titleId;
                    GridData["Sequence"] = count;

                    AddNewRow(dsGrid.Tables[0], GridData);
                }
                else
                {
                    DataRow drmo = dv[0].Row;
                    EditRow(drmo, GridData);
                }




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsGrid);


                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

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
        public JsonResult NotificationSetting()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='MaterialPurchaseOrder' and plantId='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetCheckedByAndApprovedBY(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCheckedByAndApprovedBYForOurSource(string CheckedBy, string ApprovedBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetCheckedByAndApprovedBYOutSource(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }

        #region Notification Seting for Service Requisition  PO
        [HttpGet, Authorize]
        public JsonResult ServicePORequisitionNotificationSetting()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='ServicePurchaseOrder' and plantId='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetCheckedByAndApprovedBYServicePORequisition(string CheckedBy, string ApprovedBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetCheckedByAndApprovedBYServicePORequisition(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }
        #endregion  Controller
        #region Notification Seting for service po acknowledgement
        [HttpGet, Authorize]
        public JsonResult ServicePOAcknowledgementNotificationSetting()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='ServiceAcknowledgement' and plantId='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetCheckedByAndApprovedBYServicePOAcknowledgement(string CheckedBy, string ApprovedBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.GetCheckedByAndApprovedBYServicePOAcknowledgement(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }
        #endregion  Controller

        #region All function for PO Check UI
        [Authorize, HttpGet]
        public JsonResult getPendingList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_inventoryReveiveService.getPendingList(identity.PlantId), JsonRequestBehavior.AllowGet);
            PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
            //obj.getPendingList(identity.PlantId);
            return Json(obj.getPendingList(identity.PlantId), JsonRequestBehavior.AllowGet);
            //return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult getCheckedHoldReject()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.getCheckedHoldReject(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult getCheckedList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_inventoryReveiveService.getCheckedList(identity.PlantId), JsonRequestBehavior.AllowGet);
            PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
            //obj.getCheckedList(identity.PlantId);
            //return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            return Json(obj.getCheckedList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region This code for PO Check and aporove UI
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListPoByReqDetail(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_inventoryMaterialService.GetInventoryMaterialListPoByReqDetail(inveReveiveId), JsonRequestBehavior.AllowGet);

            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public JsonResult PoApproved1(string PoId, string PoValue)
        {
            _purchaseOrderService.PoApproved1(PoId, PoValue);
            return Json(new { Message = "PO UN Approved" + AplosMessage.Success });
        }
        #endregion

        #region All Function for PO Approve UI 
        [HttpGet, Authorize]
        public JsonResult getUNApprovalList(string POTypeApprovalStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.getUNApprovalList(identity.PlantId, POTypeApprovalStatus), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult getApprovedHoldReject()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.getApprovedHoldReject(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Order Specific PO Material Details
        [Authorize, HttpGet]
        public JsonResult GetBOQItems(string ContractId, string VendorId, string IsOwnVendor, string inveReveiveMasterId, bool istradingPO)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();

                var jsondata = Json(obj.GetBOQItems(ContractId, VendorId, IsOwnVendor, inveReveiveMasterId, istradingPO), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult GetBOQItemsDetailsData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();

                var jsondata = Json(obj.GetBOQItemsDetailsData(), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        [HttpPost, Authorize]
        public JsonResult detailPOSaveForBOQ(string entity, string groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (entity == null)
            {
                throw new CustomException("Please select Items");
            }
            else
            {

                List<InventoryMaterialViewModel> entityDetailVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entity);
                List<InventoryMaterialViewModel> groupListDetailVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(groupList);
                _purchseOrderDetailService.InsertOrUpdateGraphPoForBOQItem(entityDetailVM, groupListDetailVM, taxCategoryList, PoId);
            }
            return Json(new { entity, Message = AplosMessage.Success });
        }


        [Authorize, HttpGet]
        public JsonResult GetBOQItemsListForUpdate(string ContractId, string VendorId, string inveReveiveId, string inveReveiveMasterId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();

                var jsondata = Json(obj.GetBOQItemsListForUpdate(ContractId, VendorId, inveReveiveId, inveReveiveMasterId, MaterialMasterId, ArticleId, FirstCharacteristicsValueId, SecondCharacteristicsValueId, ThirdCharacteristicsValueId), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [Authorize, HttpGet]
        public JsonResult GetPOBOQMapListForUpdate(string poId, string poDatailId)
        {
            BOQQueryService bOQQueryService = new BOQQueryService(_sqlRepository);
            return Json(bOQQueryService.GetPOBOQMapListForUpdate(poId, poDatailId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetPOBOQMapListForUpdateS(string poId)
        {
            BOQQueryService bOQQueryService = new BOQQueryService(_sqlRepository);
            return Json(bOQQueryService.GetPOBOQMapListForUpdateS(poId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        //public JsonResult detailPOUpdateForBOQ(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        public JsonResult detailPOUpdateForBOQ(PurchaseOrder entity, string groupList, string boqmapList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            if (identity.EmployeeId == entity.CheckedBy)
            {
                throw new CustomException("Please select another employee for Check by.");
            }

            entity.CheckedBy = entity.CheckedBy;
            entity.CheckedByStatus = "Pending";
            entity.AuthorizedBy = null;
            entity.AuthorizedByStatus = null;
            entity.POType = "POBOQ";
            entity.IsApproved = false;

            entity.IsClosed = false;
            entity.MasterOrderId = null;
            //entity.CheckedBy = "";
            entity.AddedBy = null;
            entity.EmployeeId = identity.EmployeeId;
            if (groupList == null)
            {
                throw new CustomException("Please select Items");
            }
            else
            {

                List<InventoryMaterialViewModel> groupListDetailVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(groupList);
                List<InventoryMaterialViewModel> boqmapListVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(boqmapList);
                _purchseOrderDetailService.POBoqUpdate(entity, groupListDetailVM, boqmapListVM, taxCategoryList, PoId);
            }
            return Json(new { entity, Message = AplosMessage.Success });
        }
        [Authorize, HttpPost]
        public JsonResult ConverttedBOQUOMData(Dictionary<string, object> data)//sk
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                data["RequiredQtyPO"] = conversion.Convert(data["MaterialMasterId"].ToString(), data["FromPoUomId"].ToString(), data["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(data["RequiredQtyPOOrginal"].ToString())).ToString("F2");
                data["OtherPOQty"] = conversion.Convert(data["MaterialMasterId"].ToString(), data["FromPoUomId"].ToString(), data["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(data["OtherPOQtyOrginal"].ToString())).ToString("F2");
                //data["OtherPOQty"] = conversion.Convert(data["MaterialMasterId"].ToString(), data["FromPoUomId"].ToString(), data["TransactionUoMId"].ToString(), OTSBD.clsStaticInfo.dbl(data["OtherPOQty"].ToString())).ToString("F2"); 
                return Json(new { data, Message = AplosMessage.Success });
            }
            catch (global::System.Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });


            }


        }

        public JsonResult ContractWiseData(string ContractId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.ContractWiseData(ContractId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }








        #endregion
        #region Documents Upload
        [HttpPost, Authorize]
        public JsonResult PODocCreate(FormCollection form, string POId)
        {
            var PODocumentMap = new JavaScriptSerializer().Deserialize<PODocumentMap>(form["PODocumentMap"]);

            var directory = ResourcesPathReader.GetPurchaseOrderPath();
            var path = Path.Combine(directory);

            if (PODocumentMap.UserFilename.IsNotNull())
            {
                ResourcesPathReader.IsValidFileExtention(Path.GetExtension(PODocumentMap.UserFilename));
            }

            var fileId = "";
            var fileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PODocumentMap.CompanyGroupId = identity.CompanyGroupId;


            _purchaseOrderService.InsertPODocMap(PODocumentMap, POId, out string Id);

            var file = Request.Files["file"];

            if (PODocumentMap.UserFilename.IsNotNull())
            {

                if (System.IO.File.Exists(path + PODocumentMap.POId))
                    System.IO.File.Delete(path + Id + Path.GetExtension(PODocumentMap.UserFilename));
                file.SaveAs(path + Id + Path.GetExtension(PODocumentMap.UserFilename));
            }
            return Json(new { PODocumentMap = PODocumentMap, Message = AplosMessage.Insert });
        }
        public JsonResult PODocumentMapData(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.PODocumentMapData(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Authorize, HttpPost]
        public ActionResult POImageDelete(string Id)
        {
            var fileId = "";
            var fileName = "";
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();

                var directory = ResourcesPathReader.GetPurchaseOrderPath();
                var path = Path.Combine(directory);
                var data = GetFile(Id);
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                    !string.IsNullOrEmpty(data["UserFilename"].ToString()))
                        fileId = data["Id"].ToString();
                    fileName = data["UserFilename"].ToString();
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }
                obj.GRNImageDelete(Id);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }
        public Dictionary<string, object> GetFile(string systemId)
        {
            try
            {
                var sql = @"Select Id, UserFilename From [TRN].[PODocumentMap] Where Id='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public JsonResult PODocumentMapDataAll(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.PODocumentMapDataAll(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Documents Upload Service PO
        [HttpPost, Authorize]
        public JsonResult ServicePODocCreate(FormCollection form, string POId)
        {
            var ServicePODocumentMap = new JavaScriptSerializer().Deserialize<ServicePODocumentMap>(form["PODocumentMap"]);

            var directory = ResourcesPathReader.GetServicePOPath();
            var path = Path.Combine(directory);

            if (ServicePODocumentMap.UserFilename.IsNotNull())
            {
                ResourcesPathReader.IsValidFileExtention(Path.GetExtension(ServicePODocumentMap.UserFilename));
            }

            var fileId = "";
            var fileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ServicePODocumentMap.CompanyGroupId = identity.CompanyGroupId;


            _purchaseOrderService.InsertServicePODocMap(ServicePODocumentMap, POId, out string Id);

            var file = Request.Files["file"];

            if (ServicePODocumentMap.UserFilename.IsNotNull())
            {

                if (System.IO.File.Exists(path + ServicePODocumentMap.POId))
                    System.IO.File.Delete(path + Id + Path.GetExtension(ServicePODocumentMap.UserFilename));
                file.SaveAs(path + Id + Path.GetExtension(ServicePODocumentMap.UserFilename));
            }
            return Json(new { PODocumentMap = ServicePODocumentMap, Message = AplosMessage.Insert });
        }
        public JsonResult ServicePODocumentMapData(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.ServicePODocumentMap(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Authorize, HttpPost]
        public ActionResult ServicePOImageDelete(string Id)
        {
            var fileId = "";
            var fileName = "";
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();

                var directory = ResourcesPathReader.GetServicePOPath();
                var path = Path.Combine(directory);
                var data = ServiceGetFile(Id);
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                    !string.IsNullOrEmpty(data["UserFilename"].ToString()))
                        fileId = data["Id"].ToString();
                    fileName = data["UserFilename"].ToString();
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }
                obj.ServicePOImageDelete(Id);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }
        public Dictionary<string, object> ServiceGetFile(string systemId)
        {
            try
            {
                var sql = @"Select Id, UserFilename From [TRN].[ServicePODocumentMap] Where Id='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public JsonResult ServicePODocumentMapDataAll(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.ServicePODocumentMapDataAll(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        #endregion

        #region Documents Upload Service PO ACK
        [HttpPost, Authorize]
        public JsonResult ServicePOACKDocCreate(FormCollection form, string POId)
        {
            var ServicePOAckDocumentMap = new JavaScriptSerializer().Deserialize<ServicePOAckDocumentMap>(form["PODocumentMap"]);

            var directory = ResourcesPathReader.GetServicePOAckPath();
            var path = Path.Combine(directory);

            if (ServicePOAckDocumentMap.UserFilename.IsNotNull())
            {
                ResourcesPathReader.IsValidFileExtention(Path.GetExtension(ServicePOAckDocumentMap.UserFilename));
            }

            var fileId = "";
            var fileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ServicePOAckDocumentMap.CompanyGroupId = identity.CompanyGroupId;


            _purchaseOrderService.InsertServicePOAckDocMap(ServicePOAckDocumentMap, POId, out string Id);

            var file = Request.Files["file"];

            if (ServicePOAckDocumentMap.UserFilename.IsNotNull())
            {

                if (System.IO.File.Exists(path + ServicePOAckDocumentMap.POId))
                    System.IO.File.Delete(path + Id + Path.GetExtension(ServicePOAckDocumentMap.UserFilename));
                file.SaveAs(path + Id + Path.GetExtension(ServicePOAckDocumentMap.UserFilename));
            }
            return Json(new { ServicePOAckDocumentMap = ServicePOAckDocumentMap, Message = AplosMessage.Insert });
        }
        public JsonResult ServicePOACKDocumentMapData(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.ServicePOAckDocumentMap(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Authorize, HttpPost]
        public ActionResult ServicePOACKImageDelete(string Id)
        {
            var fileId = "";
            var fileName = "";
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();

                var directory = ResourcesPathReader.GetServicePOAckPath();
                var path = Path.Combine(directory);
                var data = ServiceACKGetFile(Id);
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                    !string.IsNullOrEmpty(data["UserFilename"].ToString()))
                        fileId = data["Id"].ToString();
                    fileName = data["UserFilename"].ToString();
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }
                obj.ServicePOAckImageDelete(Id);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }
        public Dictionary<string, object> ServiceACKGetFile(string systemId)
        {
            try
            {
                var sql = @"Select Id, UserFilename From [TRN].[ServicePOAckDocumentMap] Where Id='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public JsonResult ServicePOACKDocumentMapDataAll(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.ServicePOAckDocumentMapDataAll(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        [Authorize, HttpGet]
        public JsonResult getServicePOTaxForAckSave(string POID)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.getServicePOTaxForAckSave(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [Authorize, HttpGet]
        public JsonResult getServicePOAckTax(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.getServicePOAckTax(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult UpdateServicePOAckTax(string ServiceAcknowledgementMasterId, List<Dictionary<string, object>> UserSendData)
        {
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                obj.UpdateServicePOAckTax(ServiceAcknowledgementMasterId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }


        #region Service Acknowledgement Additional Tax
        [Authorize, HttpPost]//
        public ActionResult SaveServiceAcknowledgementAdditionalTax(string InventoryReceiveId, List<Dictionary<string, object>> UserSendData)
        {
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                obj.ServiceAcknowledgementAdditionalTax(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult GetServiceAcknowledgementAdditionalTaxInfo(string ServicePOAckMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.GetServiceAcknowledgementAdditionalTaxInfo(ServicePOAckMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult ServiceAcknowledgementAdditionalTaxInfoDelete(string Id)
        {

            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                obj.ServiceAcknowledgementAdditionalTaxDelete(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        #endregion


        #region PO Uncheck and Un Approved
        [HttpGet, Authorize]
        public JsonResult getPOCheckedListData()
        {


            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.getPOCheckedListData(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public JsonResult getPOApprovedListData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.getPOApprovedListData(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion  PO Uncheck and Un Approved 
        #region PO UncheckAnd UnApproved Update
        [HttpPost]
        public ActionResult POUncheckUpdate(string InventoryReceiveId, Dictionary<string, object> UserSendData)
        {
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                obj.POUncheckUpdate(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult POUnapprovedUpdate(string InventoryReceiveId, Dictionary<string, object> UserSendData)
        {
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                obj.POUnapprovedUpdate(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion  PO Uncheck and Un Approved Update

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryListForSalesMaterial(string companyGroupId, string plantId, string partyPlantId, string hsnCodeId, string InventorySalesDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
            return Json(obj.GetTaxCategoryListForSalesMaterial(identity.CompanyGroupId, identity.PlantId, partyPlantId, hsnCodeId, InventorySalesDate), JsonRequestBehavior.AllowGet);
        }
        #region -- P O  ROLL BACK BY SAAD
        [Authorize, HttpGet]
        public JsonResult GetPOCheckedRollBack()
        {
            string POTypeStatus = "Checked";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrderService.POCheckedRollBack(identity.PlantId, POTypeStatus), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetPORollBackAproved()
        {
            string ApproveRejectHold = "Approved";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var res = _purchaseOrderService.PORollBackApproved(identity.PlantId, ApproveRejectHold);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost]
        public ActionResult PORollBackChecked(string InventoryReceiveId, Dictionary<string, object> UserSendData)
        {
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                obj.PORollBackChecked(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult PORollBackApproved(string InventoryReceiveId, Dictionary<string, object> UserSendData)
        {
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                obj.PORollBackUnApproved(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpGet]
        public JsonResult ServicePOApproveBy()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='ServicePOApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion

        #region PO BOQ

        [HttpPost, Authorize]
        public JsonResult GetCompanyBOQPartyDataListNew(string column, string value, string partyType)
        {
            BOQQueryService purchaseOrderBOQQueryService = new BOQQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = purchaseOrderBOQQueryService.GetCompanyBOQPartyListNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpGet]
        public JsonResult GetPOBOQItems(string ContractId, string VendorId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();

                var jsondata = Json(obj.GetPOBOQItems(ContractId, VendorId), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public JsonResult POBoqInsertUpdate(PurchaseOrder entity, string groupList, string boqmapList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            if (identity.EmployeeId == entity.CheckedBy)
            {
                throw new CustomException("Please select another employee for Check by.");
            }

            entity.CheckedBy = entity.CheckedBy;
            entity.CheckedByStatus = "Pending";
            entity.AuthorizedBy = null;
            entity.AuthorizedByStatus = null;
            entity.POType = "POBOQ";
            entity.IsApproved = false;

            entity.IsClosed = false;
            entity.MasterOrderId = null;
            //entity.CheckedBy = "";
            entity.AddedBy = null;
            entity.EmployeeId = identity.EmployeeId;
            if (groupList == null)
            {
                throw new CustomException("Please select Items");
            }
            else
            {

                List<InventoryMaterialViewModel> groupListDetailVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(groupList);
                List<InventoryMaterialViewModel> boqmapListVM = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(boqmapList);
                _purchseOrderDetailService.POBoqInsertUpdate(entity, groupListDetailVM, boqmapListVM, taxCategoryList, PoId);
            }
            return Json(new { entity, Message = AplosMessage.Success + " PO no <b>" + entity.Id + "</b>" });
        }
        [HttpPost, Authorize]
        public JsonResult POBOQSave(List<Dictionary<string, object>> updatePOBOQList, Dictionary<string,object> poBoqItemListNew)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster, dsBOq;
                DataRow drMSave = null;
                DataRow drSave = null;
                if (poBoqItemListNew == null)
                {
                    throw new CustomException("Please select Items");
                }
                //PO BOQ Save Not Done
                string DetailsId = string.Empty;
                string sql = "SELECT * FROM trn.PurchaseOrderDetail WHERE Id='" + poBoqItemListNew["Id"] + "' ";
                string sql1 = "SELECT * FROM trn.POBOQMAP WHERE PODetailId='" + poBoqItemListNew["Id"] + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsBOq, false, "1");

                double Total = 0;
                dsMaster.Tables[0].DefaultView.RowFilter = "Id = '" + poBoqItemListNew["Id"] + "' ";
                if (dsBOq.Tables[0].DefaultView.Count > 0)
                {
                    for (int i = 0; i < updatePOBOQList.Count; i++)
                    {
                        dsBOq.Tables[0].DefaultView.RowFilter = "Id = '" + updatePOBOQList[i]["Id"] + "' ";
                        if (dsBOq.Tables[0].DefaultView.Count>0)
                        {
                            drMSave = dsBOq.Tables[0].DefaultView[0].Row;
                            drMSave.BeginEdit();
                            drMSave["TransactionQty"] = clsStaticInfo.dbl(updatePOBOQList[i]["TransactionQty"].ToString());
                            drMSave["POBOQQty"] = clsStaticInfo.dbl(updatePOBOQList[i]["TransactionQty"].ToString());
                            drMSave["BaseQty"] = clsStaticInfo.dbl(updatePOBOQList[i]["TransactionQty"].ToString()) * clsStaticInfo.dbl(dsMaster.Tables[0].Rows[0]["BaseUoMFactor"].ToString());
                            Total = Total + clsStaticInfo.dbl(updatePOBOQList[i]["TransactionQty"].ToString());

                            drMSave["UpdatedBy"] = identity.Name;
                            drMSave["UpdatedDate"] = DateTime.Now;
                            drMSave["UpdatedFromIP"] = identity.IPAddress;

                            drMSave.EndEdit();
                        }
                    }
                }
                
                if (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    drSave = dsMaster.Tables[0].DefaultView[0].Row;
                    drSave.BeginEdit();
                    drSave["TransactionQty"] = clsStaticInfo.dbl(Total.ToString());
                    drSave["BaseQty"] = clsStaticInfo.dbl(Total.ToString()) * clsStaticInfo.dbl(dsMaster.Tables[0].Rows[0]["BaseUoMFactor"].ToString());
                    drSave["TransactionAmount"] = clsStaticInfo.dbl(Total.ToString()) * clsStaticInfo.dbl(dsMaster.Tables[0].Rows[0]["TransactionRate"].ToString());
                    drSave["BaseAmount"] = clsStaticInfo.dbl(drSave["BaseQty"].ToString()) * clsStaticInfo.dbl(dsMaster.Tables[0].Rows[0]["TransactionRate"].ToString());

                    drSave["UpdatedBy"] = identity.Name;
                    drSave["UpdatedDate"] = DateTime.Now;
                    drSave["UpdatedFromIP"] = identity.IPAddress;
                    drSave.EndEdit();
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsBOq,dsMaster);
                return Json(new { updatePOBOQList, Message = AplosMessage.Success + " BOQ no <b>" + poBoqItemListNew["Id"] + "</b>" });
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message });
            }
            
        }
        #endregion
        #region POBOQ Excel Report
        [HttpGet, Authorize]
        public ActionResult POBOQReport(string POID)
        {
            try
            {
                Library.MaterialManagement.InventoryManagements.POBOQReportService Report = new Library.MaterialManagement.InventoryManagements.POBOQReportService();
                Report.POBOQReport(POID);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion

        #region GRN BOQ PO Documents Upload 
        [HttpPost, Authorize]
        public JsonResult GRNPODocCreate(FormCollection form, string POId)
        {
            var PODocumentMap = new JavaScriptSerializer().Deserialize<PODocumentMap>(form["PODocumentMap"]);

            var directory = ResourcesPathReader.GetGRNPOPath();
            var path = Path.Combine(directory);

            if (PODocumentMap.UserFilename.IsNotNull())
            {
                ResourcesPathReader.IsValidFileExtention(Path.GetExtension(PODocumentMap.UserFilename));
            }

            var fileId = "";
            var fileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PODocumentMap.CompanyGroupId = identity.CompanyGroupId;


            _purchaseOrderService.InsertPODocMap(PODocumentMap, POId, out string Id);

            var file = Request.Files["file"];

            if (PODocumentMap.UserFilename.IsNotNull())
            {

                if (System.IO.File.Exists(path + PODocumentMap.POId))
                    System.IO.File.Delete(path + Id + Path.GetExtension(PODocumentMap.UserFilename));
                file.SaveAs(path + Id + Path.GetExtension(PODocumentMap.UserFilename));
            }
            return Json(new { PODocumentMap = PODocumentMap, Message = AplosMessage.Insert });
        }
        public JsonResult GRNPODocumentMapData(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.PODocumentMapData(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Authorize, HttpPost]
        public ActionResult GRNPOImageDelete(string Id)
        {
            var fileId = "";
            var fileName = "";
            try
            {
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();

                var directory = ResourcesPathReader.GetGRNPOPath();
                var path = Path.Combine(directory);
                var data = GetFile(Id);
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                    !string.IsNullOrEmpty(data["UserFilename"].ToString()))
                        fileId = data["Id"].ToString();
                    fileName = data["UserFilename"].ToString();
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }
                obj.GRNImageDelete(Id);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        [HttpGet, Authorize]
        public JsonResult GRNPODocumentMapDataAll(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PurchaseOrderQueryService obj = new PurchaseOrderQueryService();
                return Json(obj.PODocumentMapDataAll(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}


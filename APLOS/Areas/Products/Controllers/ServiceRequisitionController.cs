
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
    public class ServiceRequisitionController : BaseController
    {
        #region Constructor

        private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IMaterialRequsitionDetailsServiceService _materialRequsitionDetailsServiceService;
        private readonly IMaterialRequsitionMasterServiceService _materialRequsitionMasterServiceService;
        private readonly IServiceRequsitionMasterService _serviceRequsitionMasterService;
        private readonly IPurchaseOrderDetailService _inventoryDetailService;
        private readonly IPOMaterialService _inventoryMaterialService;
        private readonly IPurchaseOrderServiceService _inventoryService;
        private readonly IInventoryReceiveReportService _inventoryReportService;
        private readonly ISqlRepository _sqlRepository;

        public ServiceRequisitionController(
             IPurchaseOrderService inventoryReveiveService
            , IMaterialRequsitionDetailsServiceService materialRequsitionDetailsServiceService
            , IPurchaseOrderDetailService inventoryDetailService
            , IPOMaterialService inventoryMaterialService
            , IInventoryReceiveReportService inventoryReportService
            , IPurchaseOrderServiceService inventoryService
            , IMaterialRequsitionMasterServiceService materialRequsitionMasterServiceService
            , IServiceRequsitionMasterService serviceRequsitionMasterService
            , ISqlRepository sqlRepository)
        {
            _inventoryReveiveService = inventoryReveiveService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryService = inventoryService;
            _inventoryReportService = inventoryReportService;
            _sqlRepository = sqlRepository;
            _materialRequsitionDetailsServiceService = materialRequsitionDetailsServiceService;
            _materialRequsitionMasterServiceService = materialRequsitionMasterServiceService;
            _serviceRequsitionMasterService = serviceRequsitionMasterService;
        }

        #endregion Constructor

        #region Aplos
        [Authorize]
        public ActionResult ServiceReqCreation()
        {
            return View();
        }

        [Authorize]
        public ActionResult ServiceReqCheck()
        {
            return View();
        }


        [Authorize]
        public ActionResult ServiceReqApprove()
        {
            return View();
        }


        #endregion Aplos


        #region Requisition Order Report 
        [HttpGet, Authorize]
        public ActionResult RequisitionReportby(string RequisitionId, string startDate, string endDate, string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _materialRequsitionMasterServiceService.RequisitionReportby(identity.CompanyGroupId, identity.PlantId, RequisitionId,  startDate,  endDate, identity.EmployeeId);

            return null;

        }
        #endregion



        #region Purchase Order Report 
        [HttpGet, Authorize]
        public ActionResult GePurchaseOrderReport(string purchaseOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.GePurchaseOrderReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, purchaseOrderId);

            return null;

        }
        #endregion


        #region PO Approval Shahazan Shahid     

        [Authorize, HttpGet]
        public JsonResult GetListForPOApproval()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForPOApproval(identity.PlantId), JsonRequestBehavior.AllowGet);
        }



        [HttpPost, Authorize]
        public JsonResult PoUnApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            _inventoryReveiveService.PoUnApproved(PoId, PoValue, CheckedStataus, AuthorizedBy);
            return Json(new { Message = "PO Approved" + AplosMessage.Success });
        }


        #endregion
        #region   POUnapproved 
        [Authorize, HttpGet]
        public JsonResult GetListForPOApproval1()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForHold1(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListForPOApproval1UnApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForPOApproval1UnApproved(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListForPOApproval1Auth()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForPOApproval1Auth(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]

        public JsonResult PoApproved1(string PoId, string PoValue)
        {
            _inventoryReveiveService.PoApproved1(PoId, PoValue);
            return Json(new { Message = "PO UN Approved" + AplosMessage.Success });
        }

        [HttpPost, Authorize]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult PoApproved1Auth(string PoId, string PoValue)
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
            return Json(_inventoryReveiveService.getTaxCategoryListForFGService(identity.CompanyGroupId, identity.PlantId, hsnCodeId, partyPlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion




        #region  Operations / Methods

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

        [HttpPost, Authorize]
        //public JsonResult Create(MaterialRequsitionMaster entity)
        //{

        //	try
        //	{


        //		var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //		entity.CompanyGroupId = identity.CompanyGroupId;
        //		//entity.CompanyId = identity.CompanyId;
        //		//entity.PlantId = identity.PlantId;
        //		entity.CheckedBy = entity.CheckedBy;
        //		entity.CheckedByStatus = "For Checking";
        //		entity.AuthorizedBy = null;
        //		entity.AuthorizedByStatus = null;
        //		entity.RequisitionStatus = null;
        //		entity.ReqEmpId = identity.EmployeeId; 

        //		if (entity != null)
        //		{
        //			if (entity.RequirmentType == "Critical")
        //			{
        //				if (entity.ReasonWhyItIsNotPlanEarlier == null || entity.ReasonWhyItIsNotPlanEarlier == "")
        //				{
        //					throw new CustomException("Input Reason Why It Is Not Plan Earlier!");
        //				}
        //			}
        //			else if (entity.RequirmentType == "Urgent")
        //			{
        //				if (entity.ReasonWhyItIsNotPlanEarlier == null || entity.ReasonWhyItIsNotPlanEarlier == "")
        //				{
        //					throw new CustomException("Input Reason Why It Is Not Plan Earlier!");
        //				}
        //			}
        //		}
        //		_materialRequsitionMasterServiceService.Insert1(entity);
        //	}
        //	catch (Exception ex)
        //	{
        //		throw new CustomException("I am Bug!" + ex);
        //	}
        //	return Json(new { entity, Message = AplosMessage.Success + " Requisition No <b>" + entity.Id + "</b>" });
        //}

      
        public JsonResult Create(ServiceRequsitionMaster entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (identity.EmployeeId == entity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }

                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.CompanyId = identity.CompanyId;
                entity.PlantId = identity.PlantId;
                //entity.CheckedBy = entity.CheckedBy;
                //entity.CheckedByStatus = "ForChecking";
                //entity.AuthorizedBy = null;
                //entity.AuthorizedByStatus = null;
                entity.RequisitionStatus = null;
                entity.ReqEmpId = identity.EmployeeId;

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
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    entity.CheckedByStatus = null;
                    entity.AuthorizedByStatus = null;
                    entity.CheckedBy = null;
                    entity.AuthorizedBy = null;
                }
                else
                {
                    entity.CheckedBy = entity.CheckedBy;
                    entity.CheckedByStatus = "For Checking";
                    entity.AuthorizedByStatus = "For Approval";
                    entity.AuthorizedBy = null;
                    entity.AuthorizedByStatus = null;

                }
              

                if (entity != null)
                {
                    if (entity.RequirmentType == "Critical")
                    {
                        if (entity.ReasonWhyItIsNotPlanEarlier == null || entity.ReasonWhyItIsNotPlanEarlier == "")
                        {
                            throw new CustomException("Input Reason Why It Is Not Plan Earlier!");
                        }
                    }
                    else if (entity.RequirmentType == "Urgent")
                    {
                        if (entity.ReasonWhyItIsNotPlanEarlier == null || entity.ReasonWhyItIsNotPlanEarlier == "")
                        {
                            throw new CustomException("Input Reason Why It Is Not Plan Earlier!");
                        }
                    }
                }
                _serviceRequsitionMasterService.Insert1(entity);
                return Json(new { entity, Message = AplosMessage.Success + " Requisition No <b>" + entity.Id + "</b>" });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [Authorize, HttpPost]

        public JsonResult CreateSreviceReqDetail(ServiceRequsitionDetail entity)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.TotalServiceTranAmount = entity.TotalServiceTranAmount;
                entity.TotalServiceBooksCurrencyAmount = entity.TotalServiceTranAmount * entity.Rate;
                entity.Description = entity.Description;
                entity.RefferenceNo = entity.RefferenceNo;
                _serviceRequsitionMasterService.InsertSerReqDetail(entity);
                return Json(new { entity, Message = AplosMessage.Success + " Requisition No <b>" + entity.Id + "</b>" });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
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
        public JsonResult DetailCreateFGMasterOrder(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> Materialentity, IEnumerable<PurchaseOrderTax> taxCategoryList, IEnumerable<InventoryMaterialViewModel> ServiceEntity, IEnumerable<PurchaseOrderTax> ServicetaxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // Materialentity.CompanyGroupId = identity.CompanyGroupId;
            //Materialentity.CompanyId = identity.CompanyId;
            //Materialentity.PlantId = identity.PlantId;
            _inventoryDetailService.InsertOrUpdateGraphFGForMasterOrder(entity, Materialentity, taxCategoryList, ServiceEntity, ServicetaxCategoryList);
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
            return Json(new { Message = AplosMessage.Success });//entity.Id,
        }


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

        [HttpPost, Authorize]
        public JsonResult Edit(ServiceRequsitionMaster entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            entity.CheckedBy = entity.CheckedBy;
            entity.CheckedByStatus = "For Checking";
            entity.AuthorizedBy = null;
            entity.AuthorizedByStatus = null;
            entity.RequisitionStatus = null;
            entity.ReqEmpId = identity.EmployeeId;

            if (entity != null)
            {
                if (entity.RequirmentType == "Critical")
                {
                    if (entity.ReasonWhyItIsNotPlanEarlier == null || entity.ReasonWhyItIsNotPlanEarlier == "")
                    {
                        throw new CustomException("Input Reason Why It Is Not Plan Earlier!");
                    }
                }
                else if (entity.RequirmentType == "Urgent")
                {
                    if (entity.ReasonWhyItIsNotPlanEarlier == null || entity.ReasonWhyItIsNotPlanEarlier == "")
                    {
                        throw new CustomException("Input Reason Why It Is Not Plan Earlier!");
                    }
                }
            }
            _serviceRequsitionMasterService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }



        #region Inventory Detail

        [Authorize, HttpGet]
        public JsonResult GetToCurrencyRate(string currencyId, string baseCurrencyId, string docDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialRequsitionMasterServiceService.GetToCurrencyRate(currencyId, baseCurrencyId, Convert.ToDateTime(docDate), identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult DetailCreate(MaterialRequisitionDetailViewModel entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            if (entity != null)
            {
                if (entity.BudgetType == "Overbudget")
                {
                    if (entity.Reason == null || entity.Reason == "")
                    {
                        throw new CustomException("Input Reason!");
                    }
                }
                else if (entity.BudgetType == "New")
                {
                    if (entity.MaterialDetail == null || entity.MaterialDetail == "")
                    {
                        throw new CustomException("Input Material Detail!");
                    }
                    else if (entity.TransactionUoMId == null || entity.TransactionUoMId == "")
                    {
                        throw new CustomException("Please select UOM!");
                    }
                }

            }
            //entity.CompanyId = identity.CompanyId;
            //entity.PlantId = identity.PlantId;
            _materialRequsitionDetailsServiceService.InsertOrUpdateGraph(entity);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [Authorize, HttpPost]
        public JsonResult UpdateApprovedQty(IEnumerable<MaterialRequisitionDetailViewModel> entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _materialRequsitionDetailsServiceService.InsertOrUpdateGraphApprovedQty(entity);
            return Json(new { Message = AplosMessage.Updated });
        }



        [Authorize, HttpPost]
        public JsonResult DetailEdit(MaterialRequisitionDetailViewModel entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            if (entity != null)
            {
                if (entity.BudgetType == "Overbudget")
                {
                    if (entity.Reason == null || entity.Reason == "")
                    {
                        throw new CustomException("Input Reason!");
                    }
                }

            }
            //entity.CompanyId = identity.CompanyId;
            //entity.PlantId = identity.PlantId;
            _materialRequsitionDetailsServiceService.InsertOrUpdateGraphEdit(entity);
            return Json(new { entity.Id, Message = AplosMessage.Updated });
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
        [HttpPost]
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

        #region InventorymatrialAdd
        [Authorize, HttpGet]
        public JsonResult GetMaterialDetails(string MaretialDetailsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetMaterialDetails(MaretialDetailsId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetStateByInvoicingPartyPlantId(string InvoicingPartyPlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetStateByInvoicingPartyPlantId(InvoicingPartyPlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Inventory Receive Tax

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceRequsitionMasterService.GetTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId), JsonRequestBehavior.AllowGet);
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
            return Json(_serviceRequsitionMasterService.GetServiceTaxList(serviceId), JsonRequestBehavior.AllowGet);
        }

        #endregion Inventory Receive Tax

        #region Inventory Material

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

        [Authorize, HttpPost]
        public JsonResult ServiceChargesDelete(string id)
        {
            _serviceRequsitionMasterService.DeleteServiceCharge(id);
            return Json(new { Message = AplosMessage.Deleted });
        }




        [Authorize, HttpGet]
        public JsonResult GetServiceChargeList(string receiveId)
        {
            return Json(_serviceRequsitionMasterService.Query(receiveId), JsonRequestBehavior.AllowGet);
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
        [Authorize]
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
            return Json(_serviceRequsitionMasterService.GetListForHold(identity.PlantId), JsonRequestBehavior.AllowGet);
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

        [HttpPost, Authorize]
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

        [Authorize, HttpGet]
        public JsonResult GetSupervisorCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceRequsitionMasterService.GetSupervisorCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceRequsitionMasterService.GetEntity(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialRequsitionMasterServiceService.GetEmployee(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAllReqdata(string ReqStatus)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_serviceRequsitionMasterService.GetAllReqdata(ReqStatus), JsonRequestBehavior.AllowGet);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.MaterialManagement.InventoryManagements.RequisitionService obj = new Library.MaterialManagement.InventoryManagements.RequisitionService();
            return Json(obj.GetAllReqdata(ReqStatus), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetAllReqdataDetails() //string ReqDetailId
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceRequsitionMasterService.GetAllReqdataDetails(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetAllReqdataDetailsById(string Id) //string ReqDetailId
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialRequsitionMasterServiceService.GetAllReqdataDetailsById(Id), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetAllReqdata1(string ReqStatusApproval)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceRequsitionMasterService.GetAllReqdata1(ReqStatusApproval), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetReqMaster(string id)
        {
            //_materialRequsitionMasterServiceService
            return Json(_serviceRequsitionMasterService.GetReqMaster(id), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialList(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceRequsitionMasterService.QueryForPurchaseOrderDetail(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListById(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialRequsitionMasterServiceService.GetInventoryMaterialListById(inveReveiveId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult UpdateMaterial(IEnumerable<MaterialRequisitionDetailViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
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
                    if (item.TotalAmount == 0)
                        throw new CustomException("Enter Amount!");

                }
            }

            _materialRequsitionMasterServiceService.UpdateMaterial(entity, receiveTaxList);
            return Json(new { Message = AplosMessage.Updated });
        }


        [Authorize, HttpGet]
        public JsonResult GetAllServiceReqdataDetails() //string ReqDetailId
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceRequsitionMasterService.GetAllServiceReqdataDetails(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult DetailDelete(string id)
        {
            _materialRequsitionMasterServiceService.DeleteReqDetails(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _serviceRequsitionMasterService.DeleteReq(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #region ServicrRequisition Order Report 
        [HttpGet, Authorize]
        public ActionResult ServiceRequisitionReportby(string RequisitionId, string startDate, string endDate, string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _serviceRequsitionMasterService.ServiceRequisitionReportby(identity.CompanyGroupId, identity.PlantId, RequisitionId,  startDate,  endDate,  identity.EmployeeId);
            return null;

        }
        #endregion


        #region Notification Seting for Service Requisition Creation 
        [HttpGet, Authorize]
        public JsonResult ServiceRequisitionCreationNotificationSetting()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {

                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='ServiceRequistion' and plantId='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Authorize, HttpGet]
        public JsonResult GetCheckedByAndApprovedBYServiceRequisitionCreation(string CheckedBy, string ApprovedBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_serviceRequsitionMasterService.GetCheckedByAndApprovedBYServiceRequisitionCreation(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }


        #endregion  Controller

        [Authorize, HttpGet]
        public JsonResult GetFiscalYear(string formattedDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.MaterialManagement.InventoryManagements.RequisitionService obj = new Library.MaterialManagement.InventoryManagements.RequisitionService();
            return Json(obj.GetFiscalYear(formattedDate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult LoadServiceRequisitionMasterTotalEmpWise1(string MaterialMasterId, string startDate, string endDate, string empId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.MaterialManagement.InventoryManagements.RequisitionService obj = new Library.MaterialManagement.InventoryManagements.RequisitionService();
            return Json(obj.LoadServiceRequisitionMasterTotalEmpWise1(MaterialMasterId, startDate, endDate, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult ServiceRequisitionByEmpInMonth(string MaterialMasterId, string startDate, string endDate, string empId)
        {
            //var myDate = Convert.ToDateTime("06-Jun-2021");
            var myDate = Convert.ToDateTime(startDate);
            var startOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.MaterialManagement.InventoryManagements.RequisitionService obj = new Library.MaterialManagement.InventoryManagements.RequisitionService();
            return Json(obj.ServiceRequisitionByEmpInMonth(MaterialMasterId, startOfMonth, endOfMonth, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }


    }


}
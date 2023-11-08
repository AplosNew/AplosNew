
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
using Library.MaterialManagement.InventoryManagements;

namespace Aplos.Areas.Products.Controllers
{
	public class RequisitionController : BaseController
	{
		#region Constructor

		private readonly IPurchaseOrderService _inventoryReveiveService;
		private readonly IMaterialRequsitionDetailsServiceService _materialRequsitionDetailsServiceService;
		private readonly IMaterialRequsitionMasterServiceService _materialRequsitionMasterServiceService;
		private readonly IPurchaseOrderDetailService _inventoryDetailService;
		private readonly IPOMaterialService _inventoryMaterialService;
		private readonly IPurchaseOrderServiceService _inventoryService;
		private readonly IInventoryReceiveReportService _inventoryReportService;
		private readonly ISqlRepository _sqlRepository;

		public RequisitionController(
			 IPurchaseOrderService inventoryReveiveService
			, IMaterialRequsitionDetailsServiceService materialRequsitionDetailsServiceService
			, IPurchaseOrderDetailService inventoryDetailService
			, IPOMaterialService inventoryMaterialService
			, IInventoryReceiveReportService inventoryReportService
			, IPurchaseOrderServiceService inventoryService
			, IMaterialRequsitionMasterServiceService materialRequsitionMasterServiceService
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
		}

		#endregion Constructor

		#region Aplos
		[Authorize]
		public ActionResult Aplos()
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
		public ActionResult POApproval()
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
		public ActionResult POAuthorized()
		{
			return View();
		}



		#endregion Aplos


		#region Requisition Order Report 
		[HttpGet, Authorize]
		public ActionResult RequisitionReportby(string RequisitionId, string startDate, string endDate, string empId,string PreparedBy,string FromCheckedUI)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var EmployeeId = "";
			if (FromCheckedUI== "FromCheckedUI")
			{
				EmployeeId = PreparedBy;
			}
			else
			{
				EmployeeId = identity.EmployeeId;

			}

			_materialRequsitionMasterServiceService.RequisitionReportby(identity.CompanyGroupId, identity.PlantId, RequisitionId,  startDate,  endDate, EmployeeId);

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

        //[Authorize, HttpGet]
        //public JsonResult GetListForPOApprovalAuthorized()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_inventoryReveiveService.GetListForPOApprovalAuthorized(identity.PlantId), JsonRequestBehavior.AllowGet);
        //}



        //[HttpPost, Authorize]
        ////string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        //public JsonResult PoApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        //{
        //	_inventoryReveiveService.PoApproved(PoId, PoValue, CheckedStataus, AuthorizedBy);
        //	return Json(new { Message = "PO Approved" + AplosMessage.Success });
        //}

        [HttpPost, Authorize]
		//string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
		public JsonResult PoUnApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
		{
			_inventoryReveiveService.PoUnApproved(PoId, PoValue, CheckedStataus, AuthorizedBy);
			return Json(new { Message = "PO Approved" + AplosMessage.Success });
		}

		//[HttpPost, Authorize]
		////string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
		//public JsonResult PoApprovedAuth(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
		//{
		//	_inventoryReveiveService.PoApprovedAuth(PoId, PoValue, CheckedStataus, AuthorizedBy);
		//	return Json(new { Message = "PO Approved" + AplosMessage.Success });
		//}

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
       

        public JsonResult Create(MaterialRequsitionMaster entity,string CheckedByStatusForNoti,string ApprovedByStatusForNoti)
        {

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
				else if(CheckedByStatusForNoti=="False" && ApprovedByStatusForNoti=="True")
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
					entity.AuthorizedBy = null;
					entity.AuthorizedByStatus = null;
					
				}
				entity.RequisitionStatus = null;
				entity.ReqEmpId = identity.EmployeeId;
				entity.InActive = false;


				if (entity != null)
                {
					if(entity.RequirmentType == null)
					{
						throw new CustomException("Please Select Requirment Type!");
					}
					else if(entity.RequisitionType==null)
					{
						throw new CustomException("Please Select Requisition Type!");

					}
					else if (entity.EntityId == null)
					{
						throw new CustomException("Please Select Entity!");

					}
					//else if (entity.CheckedBy == null)
					//{
					//	throw new CustomException("Please Select Checked By!");

					//}
					
					else
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
					_materialRequsitionMasterServiceService.InsertRequsition(entity);

				}
                    
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
        public JsonResult Edit(MaterialRequsitionMaster entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			//entity.CheckedBy = entity.CheckedBy;
			//entity.CheckedByStatus = "For Checking";
			//entity.AuthorizedBy = null;
			//entity.AuthorizedByStatus = null;
			//entity.RequisitionStatus = null;			
			//entity.ReqEmpId = identity.EmployeeId;

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
				entity.AuthorizedBy = null;
				entity.AuthorizedByStatus = null;

			}
			entity.RequisitionStatus = null;
			entity.ReqEmpId = identity.EmployeeId;
			entity.InActive = false;

			if (entity != null)
			{
				if (entity.RequirmentType == null)
				{
					throw new CustomException("Please Select Requirment Type!");
				}
				else if (entity.RequisitionType == null)
				{
					throw new CustomException("Please Select Requisition Type!");

				}
				else if (entity.EntityId == null)
				{
					throw new CustomException("Please Select Entity!");

				}
				else
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
				_materialRequsitionMasterServiceService.Update(entity);
			}
			return Json(new { Message = AplosMessage.Updated });
		}



		#region Inventory Detail

		[Authorize, HttpGet]
		public JsonResult GetToCurrencyRate(string currencyId, string baseCurrencyId, string docDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialRequsitionMasterServiceService.GetToCurrencyRate(currencyId, baseCurrencyId, Convert.ToDateTime(docDate), identity.CompanyId), JsonRequestBehavior.AllowGet);
		}

		[Authorize,HttpPost]
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



		[Authorize,HttpPost]
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
        public JsonResult GetEntity() 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialRequsitionMasterServiceService.GetEntity(), JsonRequestBehavior.AllowGet);
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
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialRequsitionMasterServiceService.GetAllReqdata(ReqStatus), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetAllReqdataDetails() //string ReqDetailId
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata=Json(_materialRequsitionMasterServiceService.GetAllReqdataDetails(), JsonRequestBehavior.AllowGet);
			//var jsondata = Json(_skillMatrixService.GetSkillMasterDetail(queryString, queryStringProcess, queryStringSkill, queryStringOperationCode, queryStringGrouping, queryStringMachineCategory, queryStringMachineSubCategoryCode, queryStringCaption, queryStringOperationCategoryId, queryStringOnRoll, queryStringTotalPresent, queryStringOnRollShort, queryStringOnRollExcess, queryStringPresentShort, queryStringPresentExcess), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;


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
            return Json(_materialRequsitionMasterServiceService.GetAllReqdata1(ReqStatusApproval), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetReqMaster(string id)
        {
            //_materialRequsitionMasterServiceService
            return Json(_materialRequsitionMasterServiceService.GetReqMaster(id), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialList(GridParameter parameters,string inveReveiveId)
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

		[HttpGet, Authorize]
		public JsonResult GetRequisitionStockBalance(string requisitionDate, string materialMasterId, string articleId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			InventoryStockReportService inventoryStockReportService = new InventoryStockReportService();
			return Json(inventoryStockReportService.GetRequisitionStockBalance(identity.PlantId, requisitionDate, materialMasterId, articleId), JsonRequestBehavior.AllowGet);
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
                _materialRequsitionMasterServiceService.DeleteReq(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #region MailSend Function

        [HttpPost, Authorize]
        public JsonResult DailySendMailRequisitionCheck()
        {
            _materialRequsitionMasterServiceService.DailySendMailRequisitionCheck("TS", "TS", "10215","", "", "", "");
            return Json(new { Message = AplosMessage.Success });
        }


		#endregion

		[HttpGet, Authorize]
		public JsonResult GetMasterOrderList(string contractId) 
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				 var sql = @"SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId	
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' )
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus
                                    ,A.OwnReferenceNo [BuyerOrd],A.BuyerReferenceNo [OwnOrd]
                                    ,[BuyerItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,[ContractNo]=STUFF((select distinct ','+CO.ContractNo from 
																			dbo.Contract CO
															left join trn.SalesOrder SO on SO.ContractId=CO.Id
															left join [TRN].[MasterOrderItem] MOI ON MOI.Id=SO.MasterOrderItemId
							                                where MOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,[MasterLCNo]=STUFF((select distinct ','+MLC.LCRef from dbo.MasterLC MLC
															left join dbo.Contract CO on CO.MasterLCId=MLC.Id
															left join trn.SalesOrder SO on SO.ContractId=CO.Id
															left join [TRN].[MasterOrderItem] MOI ON MOI.Id=SO.MasterOrderItemId
							                                where MOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            FROM [TRN].[MasterOrder] AS A
                            left JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            WHERE A.CompanyId='" + identity.CompanyId + "' AND OrderType='ExternalOrder'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		[HttpGet, Authorize]
		public JsonResult GetMasterOrderDetailsList(string MasterOrderId)  
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;				
				var sql = @"SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId	
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' )
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage,A.BuyerDepartmentId
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage,PM.UserName ProductMaster,OS.UserName OrderStatus
                                    ,A.OwnReferenceNo [BuyerOrd],A.BuyerReferenceNo [OwnOrd]
                                    ,[BuyerItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
							                                where XMOI.MasterOrderId=A.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    ,A.ContractId ContractNo,MLC.Id MasterLCNo
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                            LEFT JOIN HKP.OrderStatus OS ON OS.Id=A.OrderStatusId
                            LEFT JOIN dbo.Contract CNT ON CNT.Id=A.ContractId
							LEFT JOIN dbo.MasterLC MLC ON MLC.Id=CNT.MasterLCId
                            WHERE A.CompanyId='" + identity.CompanyId + "' AND OrderType='ExternalOrder' AND A.Id='"+ MasterOrderId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		[HttpGet, Authorize]
		public JsonResult NotificationSetting() 
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		
			try
			{
				
				var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='MaterialRequistion' and plantId='"+identity.PlantId+"'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		[Authorize, HttpGet]
		public JsonResult GetSupervisorCbo()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialRequsitionMasterServiceService.GetSupervisorCbo(), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialRequsitionMasterServiceService.GetCheckedByAndApprovedBY(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetFiscalYear(string formattedDate)
		{
			
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			Library.MaterialManagement.InventoryManagements.RequisitionService obj = new Library.MaterialManagement.InventoryManagements.RequisitionService();
			return Json(obj.GetFiscalYear(formattedDate), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult LoadRequisitionMasterTotalEmpWise1(string MaterialMasterId, string startDate, string endDate, string empId)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			Library.MaterialManagement.InventoryManagements.RequisitionService obj = new Library.MaterialManagement.InventoryManagements.RequisitionService();
			return Json(obj.LoadRequisitionMasterTotalEmpWise1( MaterialMasterId, startDate, endDate, identity.EmployeeId), JsonRequestBehavior.AllowGet);
		}




		[Authorize, HttpGet]
		public JsonResult RequisitionByEmpInFixsal(string startDate, string endDate)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			Library.MaterialManagement.InventoryManagements.RequisitionService obj = new Library.MaterialManagement.InventoryManagements.RequisitionService();
			return Json(obj.RequisitionByEmpInFixsal(startDate, endDate), JsonRequestBehavior.AllowGet);
		}


		[Authorize, HttpGet]
		public JsonResult RequisitionByEmpInMonth(string MaterialMasterId, string startDate, string endDate, string empId)
		{
			//var myDate = Convert.ToDateTime("06-Jun-2021");
			var myDate = Convert.ToDateTime(startDate);
			var startOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
			var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			Library.MaterialManagement.InventoryManagements.RequisitionService obj = new Library.MaterialManagement.InventoryManagements.RequisitionService();
			return Json(obj.RequisitionByEmpInMonth(MaterialMasterId, startOfMonth, endOfMonth, identity.EmployeeId), JsonRequestBehavior.AllowGet);
		}

	}

}
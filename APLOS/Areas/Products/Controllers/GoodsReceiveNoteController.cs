using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Model.Products;
using Library.Service.Enums;
using Library.MaterialManagement.Inventory;
using Library.Service.Logs;
using Library.Service.Products;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using Library.MaterialManagement.Products;
using System.Data;
using Library.Security.Core;
using Library.MaterialManagement.JobWork;
using Aplos.MaterialManagement;
using Aplos.MaterialManagement.MaterialQuery;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using System.Web;

namespace Aplos.Areas.Products.Controllers
{
    public class GoodsReceiveNoteController : BaseController
    {
        #region Constructor



        private readonly IInventoryReceiveService _inventoryReveiveService;
        private readonly IInventoryIssueService _inventoryIssueService;
        private readonly IInventoryReceiveDetailService _inventoryDetailService;
        private readonly IGRNPORequisitionAllocationService _gRNPORequisitionAllocationService;
        private readonly IInventoryMaterialService _inventoryMaterialService;
        private readonly IInventoryServiceService _inventoryService;
        private readonly IInventoryReceiveReportService _inventoryReportService;
        private readonly IIssueRequestService _issueRequestService;
        private readonly IIssueRequestMasterService _issueRequestMasterService;
        private readonly ISqlRepository _sqlRepository;
        // private readonly IRepositoryAsync<InventoryReceiveDetail> _receiveDetailRepository;
        private readonly IRepositoryAsync<PurchaseReturnDetail> _PurchaseReturnDetailRepository;

        public GoodsReceiveNoteController(IInventoryReceiveService inventoryReveiveService
            , IInventoryReceiveDetailService inventoryDetailService
            , IInventoryMaterialService inventoryMaterialService
            , IInventoryReceiveReportService inventoryReportService
            , IInventoryServiceService inventoryService
            , IGRNPORequisitionAllocationService gRNPORequisitionAllocationService
            , IIssueRequestService issueRequestService
            , IIssueRequestMasterService issueRequestMasterService
            , IInventoryIssueService inventoryIssueService
            , ISqlRepository sqlRepository
            //, IRepositoryAsync<InventoryReceiveDetail> receiveDetailRepository
            , IRepositoryAsync<PurchaseReturnDetail> PurchaseReturnDetailRepository
            )

        {
            _inventoryReveiveService = inventoryReveiveService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryService = inventoryService;
            _inventoryReportService = inventoryReportService;
            _gRNPORequisitionAllocationService = gRNPORequisitionAllocationService;
            _issueRequestService = issueRequestService;
            _issueRequestMasterService = issueRequestMasterService;
            _inventoryIssueService = inventoryIssueService;
            _sqlRepository = sqlRepository;
            //_receiveDetailRepository = receiveDetailRepository;
            _PurchaseReturnDetailRepository = PurchaseReturnDetailRepository;

        }

        #endregion Constructor

        #region Aplos

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult IssueSlipIssue()
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
        public ActionResult GRNApproval()
        {
            return View();
        }

        [Authorize]
        public ActionResult GRNCheck()
        {
            return View();
        }

        //RequisitionWise Issue Slip
        [Authorize]
        public ActionResult IssueSlip()
        {
            return View();
        }

        //RequisitionWise Issue Slip
        [Authorize]
        public ActionResult MaterialWiseIssueSlip()
        {
            return View();
        }

        [Authorize]
        public ActionResult IssueUI()
        {
            return View();
        }


        [Authorize]
        public ActionResult IssueSlipCheck()
        {
            return View();
        }

        [Authorize]

        public ActionResult ApprovingIssueSlip()
        {
            return View();
        }

        public ActionResult GRNByPO()
        {
            return View();
        }

        public ActionResult GRNBOQPO()
        {
            return View();
        }

        [Authorize]

        public ActionResult MaterialIssueSlip()
        {
            return View();
        }

        [Authorize]
        public ActionResult AssetIssueSlip()
        {
            return View();
        }


        public ActionResult PurchaseReturn()
        {
            return View();
        }

        [Authorize]
        public ActionResult PurchaseReturnChecked()
        {
            return View();
        }

        [Authorize]
        public ActionResult PurchaseReturnApprove()
        {
            return View();
        }
        [Authorize]
        public ActionResult GRNUncheckedAndUnApproved()
        {
            return View();
        }

        [Authorize]
        public ActionResult GRNRequitionSOAllocation()
        {
            return View();
        }

        
        public ActionResult AllBinWiseGRN()
        {
            return View();
        }

        public ActionResult IndependentServiceGRN()
        {
            return View();
        }

        #endregion Aplos

        #region GRN-By-PO
        [HttpPost]
        public JsonResult CreateGRNBYPO(InventoryReceive entity, string entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList
            , IEnumerable<GRNPORequisitionMap> requisitionDetailList, string GRNType, string AcceptanceId, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<GRNBinAllocationMap> grnBinAllocationMap)
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
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            //IEnumerable<InventoryMaterialViewModel>
            List<InventoryMaterialViewModel> entityMatAndImat1 = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entityMatAndImat, settings);
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
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.IsApproved = true;
                entity.RequiredPosting = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            if (entityMatAndImat1 != null)
            {
                foreach (var item in entityMatAndImat1)
                {

                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");

                    }
                    else if (item.TransactionQty.ToString() == "0")
                    {
                        throw new CustomException("Please Input The Current Qty !");
                    }
                    else if (string.IsNullOrEmpty(item.ArticleId))
                    {
                        throw new CustomException("Please Input AritcleId  !!");
                    }

                }
            }
            else
            {
                throw new CustomException("Please Select Atleast One Materials !!.");
            }
            if (chargesListPO != null)
            {
                foreach (var item in chargesListPO)
                {
                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");
                    }
                    else if (item.Amount.ToString() == "0")
                    {
                        throw new CustomException("Please Input  Amount !");
                    }

                }
            }
            bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            if (_returnRes == true)
            {
                throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            }

            DetailCreate(entity, entityMatAndImat1, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType, requisitionDetailList, grnBinAllocationMap);
            ServiceChargesCreateNew(chargesListPO, POServiceTaxList, entity.Id, AcceptanceId);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public JsonResult UpdateGRNBYPO(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.IsApproved = true;
                entity.RequiredPosting = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            if (entityMatAndImat != null)
            {
                foreach (var item in entityMatAndImat)
                {

                    if (!item.check)
                        throw new CustomException("Please Select Materials !");

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            if (chargesListPO != null)
            {
                foreach (var item in chargesListPO)
                {
                    if (!item.check)
                        throw new CustomException("Please Select Materials !");

                }
            }
            DetailEdits(entity, entityMatAndImat, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType);
            ServiceChargesCreateNewEdit(chargesListPO, POServiceTaxList, entity.Id);

            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public JsonResult UpdateGRNBYPOMaster(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.IsApproved = true;
                entity.RequiredPosting = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
           
            _inventoryDetailService.UpdateGRNBYPOMaster(entity, GRNType);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public ActionResult DeleteGRNBYPO(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _inventoryReveiveService.Delete(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

       
        #endregion GRN-By-PO

        #region GRN-BOQ-PO

        [HttpPost, Authorize]
        public JsonResult GetGRNBOQPartyListNew(string column, string value, string partyType)
        {
            InventoryReceiveQueryService purchaseOrderBOQQueryService = new InventoryReceiveQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = purchaseOrderBOQQueryService.GetGRNBOQPartyListNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public JsonResult GetItemListByVendor(string vendorId)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = inventoryReceiveQueryService.GetItemListByVendor(identity.PlantId, vendorId);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public JsonResult CreateGRNBOQPO(InventoryReceive entity, string entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string AcceptanceId, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            //IEnumerable<InventoryMaterialViewModel>
            List<InventoryMaterialViewModel> entityMatAndImat1 = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entityMatAndImat, settings);
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
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.IsApproved = true;
                entity.RequiredPosting = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            if (entityMatAndImat1 != null)
            {
                foreach (var item in entityMatAndImat1)
                {

                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");

                    }
                    else if (item.TransactionQty.ToString() == "0")
                    {
                        throw new CustomException("Please Input The Current Qty !");
                    }

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            if (chargesListPO != null)
            {
                foreach (var item in chargesListPO)
                {
                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");
                    }
                    else if (item.Amount.ToString() == "0")
                    {
                        throw new CustomException("Please Input  Amount !");
                    }

                }
            }
            bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            if (_returnRes == true)
            {
                throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            }

            DetailCreate(entity, entityMatAndImat1, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType,null,null);
            ServiceChargesCreateNew(chargesListPO, POServiceTaxList, entity.Id, AcceptanceId);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public JsonResult UpdateGRNBOQPO(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.IsApproved = true;
                entity.RequiredPosting = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            if (entityMatAndImat != null)
            {
                foreach (var item in entityMatAndImat)
                {

                    if (!item.check)
                        throw new CustomException("Please Select Materials !");

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            if (chargesListPO != null)
            {
                foreach (var item in chargesListPO)
                {
                    if (!item.check)
                        throw new CustomException("Please Select Materials !");

                }
            }
            _inventoryDetailService.UpdateGRNBYPOMaster(entity, GRNType);
            //DetailEdits(entity, entityMatAndImat, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType);
            //ServiceChargesCreateNewEdit(chargesListPO, POServiceTaxList, entity.Id);

            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public ActionResult DeleteGRNBOQPO(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _inventoryReveiveService.Delete(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpPost]
        public ActionResult UpdateGRNBOQTax(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            _inventoryDetailService.UpdateGRNBOQTax(entity, taxCategoryList);
            return Json(new { Message = AplosMessage.Success });
        }
        #endregion GRN-By-PO

        #region purchase-return
        [HttpPost]
        public JsonResult CreatePurchaseReturn(PurchaseReturn entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<PurchaseReturnTax> receiveTaxList, IEnumerable<GRNPORequisitionAllocation> grnBoqList
            , IEnumerable<InventoryMaterialViewModel> chargesList, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string AcceptanceId, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<PurchaseReturnTax> ServicetaxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.PlantId = identity.PlantId;

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
                entity.IsApproved = false;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.ApprovedByStatus = null;
                entity.CheckedBy = null;
                entity.ApprovedBy = null;
                entity.IsApproved = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "For Checking";
                entity.ApprovedBy = null;
                entity.ApprovedByStatus = null;
                entity.IsApproved = false;

            }
            //entity.IsApproved = false;
            if (entityMatAndImat != null)
            {
                foreach (var item in entityMatAndImat)
                {

                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");

                    }
                    else if (string.IsNullOrEmpty(item.TransactionQty.ToString()) || item.TransactionQty.ToString() == "0")
                    {
                        throw new CustomException("Please Input The Current Qty !");
                    }

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            if (chargesList != null)
            {
                foreach (var item in chargesList)
                {
                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");
                    }
                    else if (item.Amount.ToString() == "0")
                    {
                        throw new CustomException("Please Input  Amount !");
                    }

                }
            }
            _inventoryDetailService.InsertOrUpdateGraphForPurchaseReturn(entity, entityMatAndImat, receiveTaxList, grnBoqList, entity.Id, entity.MaterialStorageId, GRNType, chargesList, ServicetaxCategoryList);

            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public ActionResult DeletePurchaseReturnfinal(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _inventoryReveiveService.DeletePurchaseReturnfinal(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion purchase-return

        #region GRN Through Requisition
        [HttpPost]
        public JsonResult Create(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string AcceptanceId, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.IsApproved = true;
                entity.RequiredPosting = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.IsApproved = false;
                entity.RequiredPosting = true;

            }
            //entity.EmployeeId = identity.EmployeeId;
            if (entityMatAndImat != null)
            {
                foreach (var item in entityMatAndImat)
                {

                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");

                    }
                    else if (item.TransactionQty.ToString() == "0")
                    {
                        throw new CustomException("Please Input The Current Qty !");
                    }

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            if (chargesListPO != null)
            {
                foreach (var item in chargesListPO)
                {
                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");
                    }
                    else if (item.Amount.ToString() == "0")
                    {
                        throw new CustomException("Please Input  Amount !");
                    }

                }
            }
            DetailCreate(entity, entityMatAndImat, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType,null,null);
            ServiceChargesCreateNew(chargesListPO, POServiceTaxList, entity.Id, AcceptanceId);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public JsonResult UpdareGRN(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;

            }
            entity.IsApproved = false;

            if (entityMatAndImat != null)
            {
                foreach (var item in entityMatAndImat)
                {

                    if (!item.check)
                        throw new CustomException("Please Select Materials !");

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            if (chargesListPO != null)
            {
                foreach (var item in chargesListPO)
                {
                    if (!item.check)
                        throw new CustomException("Please Select Materials !");

                }
            }
            DetailEdits(entity, entityMatAndImat, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType);
            ServiceChargesCreateNewEdit(chargesListPO, POServiceTaxList, entity.Id);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
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

        #endregion GRN Through Requisition

        #region -- Operations
        [Authorize, HttpGet]
        public JsonResult GetListForGRNSaveData(string GRNWithReqPOCheckStatus)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.QueryGetListForGRNSaveData(identity.PlantId, GRNWithReqPOCheckStatus), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetListOfPO(string PoType, string Status,string vendorId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForHold(identity.PlantId, PoType, Status, vendorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPOListForAdvance(string PoType, string Status, string vendorId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForHold(identity.PlantId, PoType, Status, vendorId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult LoadAcceptanceDetails(string AcceptanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.LoadAcceptanceDetails(AcceptanceId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetSavedPOList(string GRNId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetSavedPOList(GRNId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSavedPOList1(string GRNId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetSavedPOList1(GRNId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListForREqPOGRN(string PoType, string Status)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForREqPOGRN(identity.PlantId, PoType, Status), JsonRequestBehavior.AllowGet);
        }
        
        [Authorize, HttpGet]
        public JsonResult GetListOfPOGateEntry(string partyCode)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListOfPOGateEntry(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyCode), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.Query(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListForGRNBYPO(string GRNbyPOCheckStatus,string grnType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                //return Json(obj.QueryGetListGRNMasterData(identity.PlantId, GRNbyPOCheckStatus, grnType), JsonRequestBehavior.AllowGet);
                var jsondata = Json(obj.QueryGetListGRNMasterData(identity.PlantId, GRNbyPOCheckStatus, grnType), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetListForMasterData2(string GRNbyPOApprovedStatus)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_inventoryReveiveService.QueryGetListForMasterData2(identity.PlantId, GRNbyPOApprovedStatus), JsonRequestBehavior.AllowGet);
            var jsondata = Json(inventoryReceiveQueryService.QueryGetListForMasterData2(identity.PlantId, GRNbyPOApprovedStatus), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public JsonResult GetSearchPostedGRNPOList(string column, string value)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetSearchPostedGRNPOList(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetListForGrnByPoReq(string GRNWithReqPOApprovedStatus)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetListForGrnByPoReq(identity.PlantId, GRNWithReqPOApprovedStatus), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListByGrnno(GridParameter parameters, int GRN)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetListByGrnno(parameters, identity.PlantId, GRN), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPostingList(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetPostingList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// using inventory payable
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Authorize, HttpGet]
        public JsonResult GetListForInvPayable(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetListForInvPayable(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult InsertGRN(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMaterial)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            entity.FixedAssetOrInventory = "Inventory";
            if (entityMaterial != null)
            {
            }
            _inventoryReveiveService.Insert(entity, entityMaterial);
            return Json(new { Message = AplosMessage.Insert });
        }
        [Authorize, HttpPost]
        public JsonResult Edit(InventoryReceive entity)
        {
            _inventoryReveiveService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }
        #endregion -- Operations

        #region Inventory Detail
        [Authorize, HttpGet]
        public JsonResult GetToCurrencyRate(string currencyId, string baseCurrencyId, string docDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetToCurrencyRate(currencyId, baseCurrencyId, Convert.ToDateTime(docDate), identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailCreate(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id
            , string MaterialStorageId, string GRNType, IEnumerable<GRNPORequisitionMap> requisitionDetailList, IEnumerable<GRNBinAllocationMap> grnBinAllocationMap)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryDetailService.InsertOrUpdateGraphNew(entity, entityMat, taxCategoryList, id, MaterialStorageId, GRNType, requisitionDetailList, grnBinAllocationMap);
            return Json(new { Message = AplosMessage.Success });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult GrnRequisitionAllocationSave(IEnumerable<InventoryMaterialViewModel> entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _gRNPORequisitionAllocationService.InsertOrUpdateGraphNewGRNAllocation(entity);
            return Json(new { Message = AplosMessage.Success });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailEdits(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           _inventoryDetailService.InsertOrUpdateGraphNewEdits(entity, entityMatAndImat, taxCategoryList, id, MaterialStorageId, GRNType);
           
            return Json(new { Message = AplosMessage.Success });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Edit))]
        public JsonResult DetailEdit(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryDetailService.InsertOrUpdateGraphNewEdits(entity, entityMat, taxCategoryList, id, MaterialStorageId, GRNType);
            return Json(new { Message = AplosMessage.Success });
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DetailDelete(string receiveDetailId)
        {
            _inventoryDetailService.Delete(receiveDetailId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion Inventory Detail

        #region Inventory Receive Tax

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId, string GRNDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId, GRNDate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryOtherVendorList(string receiveId, string hsnCodeId, string GRNDate,string OtherPartyPlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetTaxCategoryOtherVendorList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId, GRNDate, OtherPartyPlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetReceiveTaxList(string receiveDetailId)
        {
            return Json(_inventoryReveiveService.GetReceiveTaxList(receiveDetailId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetReceiveTaxListPO(string receiveDetailId)
        {
            return Json(_inventoryReveiveService.GetReceiveTaxListPO(receiveDetailId), JsonRequestBehavior.AllowGet);
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

        [Authorize, HttpGet]
        public JsonResult GetServiceTaxListPR(string serviceId)
        {
            return Json(_inventoryReveiveService.GetServiceTaxListPR(serviceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceTaxListPO(string serviceId)
        {
            return Json(_inventoryReveiveService.GetServiceTaxListPO(serviceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryListByPartyPlant(string partyPlantId, string hsnCodeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetTaxCategoryListByPartyPlant(identity.CompanyGroupId, partyPlantId, identity.PlantId, hsnCodeId), JsonRequestBehavior.AllowGet);
        }
        #endregion Inventory Receive Tax

        #region Inventory Material

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialList(GridParameter parameters, string inveReveiveId, string POID, string AcceptanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.Query(parameters, inveReveiveId, POID, AcceptanceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GRNDetailsData(string inveReveiveId, string POID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_inventoryMaterialService.GRNDetailsData(inveReveiveId, POID), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }


        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialPayableList(GridParameter parameters, string inveReveiveId)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(accountsInventoryPayableService.GetPayableMaterial(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryShortageMaterialPayableList(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.GetPayableShortageMaterial(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryRejectMaterialPayableList(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.GetPayableRejectMaterial(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }
        //shakawats
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListByPO(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.Query1(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListByOnlyPO(GridParameter parameters, string inveReveiveId, string AcceptanceId)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            return Json(inventoryReceiveQueryService.QueryOnlyPO(parameters, inveReveiveId, AcceptanceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetRequsitionQtyListByPO(GridParameter parameters, string poIds)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            return Json(inventoryReceiveQueryService.GetRequsitionQtyListByPO(parameters, poIds), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialPayable(string inveReveiveId, string employeeId, bool isReversCharge)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(employeeId) && employeeId != "null")
                return Json(accountsInventoryPayableService.GetInventoryMaterialForImprestPayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
            else
            {
                if (isReversCharge)
                    return Json(accountsInventoryPayableService.GetInventoryMaterialReversChargePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
                else
                    return Json(accountsInventoryPayableService.GetInventoryMaterialWithoutReversChargePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Inventory Material

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListForPOUpdate(string inveReveiveId, string InventoryReceiveId, string MaterialMasterId, string InventoryReceiveDetailId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.GetInventoryMaterialListForPOUpdate(inveReveiveId, InventoryReceiveId, MaterialMasterId, InventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
        }

        #region Service Charges

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreate(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            _inventoryService.InsertGraph(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreateNew(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string Id, string AcceptanceId)
        {
            _inventoryService.InsertGraphNew(chargesListPO, POServiceTaxList, Id, AcceptanceId);
            return Json(new { Message = AplosMessage.Success });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreateNewEdit(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string Id)
        {
            _inventoryService.InsertGraphNewEdit(chargesListPO, POServiceTaxList, Id);
            return Json(new { Message = AplosMessage.Success });
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
        public JsonResult GetPurchaseReturnServiceChargeList(string receiveId)
        {
            return Json(_inventoryService.QueryPurchaseReturnCharges(receiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceChargeListPO(string receiveId, string AcceptanceId)
        {
            return Json(_inventoryService.Query1(receiveId, AcceptanceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult getTCSData(string receiveId)
        {
            return Json(_inventoryService.getTCSData(receiveId), JsonRequestBehavior.AllowGet);
        }
        #endregion Service Charges

        [HttpGet, Authorize]
        public ActionResult Report(ReportFormat reportFormat, string inventoryReceiveId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Inventory Receive " + inventoryReceiveId + "";
            var workbook = _inventoryReportService.GetInventoryReceiveReport(identity.CompanyId, plantId, inventoryReceiveId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        #region Employee Purchase
        [Authorize]
        public ActionResult EmployeePurchase()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeePurchaseList(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetEmployeePurchaseList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #endregion Employee Purchase

        #region GRN Approved

        [HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult Approved(IEnumerable<InventoryReceive> entities, string GRNStatus)
        {
            _inventoryReveiveService.GRNApproved(entities, GRNStatus);
            return Json(new { Message = AplosMessage.Insert });
        }
        #endregion GRN Approved



        #region Grn Approved and UnApproved Taufik 
        [HttpPost, Authorize]
        public JsonResult Approved1(IEnumerable<InventoryReceive> entities, string GRNStatus, string GRNNo, string AuthorizedByStatus, string RejectApprovedReason)
        {
            _inventoryReveiveService.GRNApproved1(entities, GRNStatus, GRNNo, AuthorizedByStatus, RejectApprovedReason);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion GRN Approved and Unapproved Taufik


        //#region GRN Approval

        //[HttpPost, ChaildAction(ParentActionName = "Edit")]
        //public JsonResult Approval(IEnumerable<InventoryReceive> entities, string GRNStatus)
        //{
        //    _inventoryReveiveService.GRNApproval(entities, GRNStatus);
        //    return Json(new { Message = AplosMessage.Insert });
        //}

        //#endregion GRN Approved

        #region PaymentHold

        [Authorize, HttpGet]
        public JsonResult GetListForHold(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetListForHold(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult PaymentHold(IEnumerable<InventoryReceive> entities)
        {
            _inventoryReveiveService.PaymentHold(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion PaymentHold


        #region INVENTORY RECEIPT  Report  

        [Authorize, HttpGet]
        public ActionResult GRNReport(string grnId, string plantId)

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            _inventoryReveiveService.InventoryReceive(identity.CompanyGroupId, identity.CompanyId, plantId, identity.UserId, grnId);

            return null;
        }

        #endregion

        #region GRN BOQ PO  Report  

        [Authorize, HttpGet]
        public ActionResult GRNBOQPOReport(string grnBOQPOId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.GrnBOQPORep(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnBOQPOId);

            return null;
        }

        #endregion

        #region FG INVENTORY Register  Report  

        [Authorize, HttpGet]
        public ActionResult FGGRNReport(string grnId)

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.FGInventoryReceive(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

            return null;
        }

        #endregion




        #region GRN Approval Bye Taufik 25-6-2019
        [HttpGet, Authorize]
        public JsonResult getListForGRNUnchecked()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.getListForGRNUnchecked(identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult getListForGRNChecked()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.getListForGRNChecked(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getListForGRNRejectHoldList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.getListForGRNRejectHoldList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetRecentApprovedData(string grnId)
        {
            return Json(_inventoryReveiveService.GetRecentApprovedData(grnId), JsonRequestBehavior.AllowGet);
        }

        
        [HttpGet, Authorize]
        public JsonResult GetListForGRNApproval()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForGRNAp(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListForGRNApprovalHoldReject()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForGRNApprovalHoldReject(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult PoApproved(string PoId, string PoValue)
        {
            _inventoryReveiveService.PoApproved(PoId, PoValue);
            return Json(new { Message = "PO Approved" + AplosMessage.Success });
        }


        #endregion


        #region    Unapproved 
        [Authorize, HttpGet]
        public JsonResult GetListForGRNUNApproval()
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetListForGRNUNApproval(identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult PoApproved1(string PoId, string PoValue)
        {
            _inventoryReveiveService.PoApproved1(PoId, PoValue);
            return Json(new { Message = "PO UN Approved" + AplosMessage.Success });
        }

        #endregion


        [HttpPost, Authorize]

        public JsonResult GRNChecked(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy, string CheckedRejectReason)
        {

            _inventoryReveiveService.GRNChecked(PoId, PoValue, CheckedStataus, AuthorizedBy, CheckedRejectReason);
            return Json(new { Message = "GRN Checked " + AplosMessage.Success });
        }

        #region Grn IssueSlip 
        [Authorize, HttpGet]
        public JsonResult IssueSlipFilter()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.IssueSlipFilter(), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        //public JsonResult IssueSlipCreate(IssueRequestMaster Issentity, IEnumerable<IssueRequestViewModel> entity, string CheckedBy, string IssueSlipType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<IssueRequestViewModel> SOListSelectedNew, IEnumerable<IssueRequestViewModel> MaterialColorListNew, string ProcessId)
        public JsonResult IssueSlipCreate(IssueRequestMaster Issentity, string entity, string entityGroupData, string CheckedBy, string IssueSlipType
                , string CheckedByStatusForNoti, string ApprovedByStatusForNoti, string SOListSelectedNew, string MaterialColorListNew
                , string ProcessId, string OrderSpecific,List<Dictionary<string,object>> machinepopUpDataList)

        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Issentity.CheckedBy = CheckedBy;
            Issentity.CompanyGroupId = identity.CompanyGroupId;
            Issentity.CompanyId = identity.CompanyId;
            Issentity.PlantId = identity.PlantId;
            Issentity.Orderspecific = OrderSpecific;
            List<IssueRequestViewModel> entityDetailVM = JsonConvert.DeserializeObject<List<IssueRequestViewModel>>(entity);
            List<IssueRequestViewModel> entityGroupDataVM = JsonConvert.DeserializeObject<List<IssueRequestViewModel>>(entityGroupData);
            List<IssueRequestViewModel> SOListSelectedNewDetailVM = JsonConvert.DeserializeObject<List<IssueRequestViewModel>>(SOListSelectedNew);
            List<IssueRequestViewModel> MaterialColorListNewDetailVM = JsonConvert.DeserializeObject<List<IssueRequestViewModel>>(MaterialColorListNew);

            _issueRequestService.InsertOrUpdateGraphIssueSlipCreate(Issentity, entityDetailVM, entityGroupDataVM, IssueSlipType, CheckedByStatusForNoti, ApprovedByStatusForNoti, SOListSelectedNewDetailVM, MaterialColorListNewDetailVM, ProcessId, machinepopUpDataList);
            //DetailCreate(entity, entityMatAndImat, receiveTaxList, entity.Id, entity.MaterialStorageId);

            return Json(new { Issentity, Message = "Issue Request " + AplosMessage.Success + "Id=" + Issentity.Id });
            //return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }


        #endregion

        #region Grn IssueRequest Report 
        [Authorize, HttpGet]
        public ActionResult IssueRequestReport(string issueId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.IssueRequestReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, issueId);

            return null;
        }

        #endregion


        #region Grn IssueUI or IssueWithReqPOGRN Report 
        [Authorize, HttpGet]
        public ActionResult IssueWithReqPOGRNReport(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.IssueWithReqPOGRNReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, Id);

            return View();
        }

        #endregion


        [HttpGet, Authorize]
        public JsonResult IssueListData(string IssueStatus, string IssueSlipType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.IssueListData(IssueStatus, IssueSlipType), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult AssetIssueListData(string IssueStatus, string IssueSlipType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.AssetIssueListData(IssueStatus, IssueSlipType), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult ApprovedIssueSlipGridData(string IssueStatusApproval, string IssueSlipType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.ApprovedIssueSlipGridData(IssueStatusApproval, IssueSlipType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult IssueListById(GridParameter parameters, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.IssueListById(parameters, Id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult IssueSlipDetail(string slipstatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.IssueSlipDetail(slipstatus, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult IssueSlipUpdate(IssueRequestMaster Issentity, string entity, string Id, string CheckedBy, string IssueSlipType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, string SOListSelectedNew, string MaterialColorListNew, string OrderSpecific)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Issentity.CheckedBy = CheckedBy;
            Issentity.CompanyGroupId = identity.CompanyGroupId;
            Issentity.CompanyId = identity.CompanyId;
            Issentity.PlantId = identity.PlantId;

            List<IssueRequestViewModel> entityDetailVM = JsonConvert.DeserializeObject<List<IssueRequestViewModel>>(entity);
            _issueRequestService.InsertOrUpdateGraphIssueSlipUpdate(Issentity, entityDetailVM, Id, IssueSlipType, CheckedByStatusForNoti, ApprovedByStatusForNoti);
            return Json(new { Message = "Issue Request " + AplosMessage.Updated });
        }
        [Authorize, HttpGet]
        public JsonResult IssueFilter()
        {
            return Json(_inventoryReveiveService.IssueFilter(), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public JsonResult IssueListDataByProudctionOrder(string IssueStatus, string IssueSlipType,string productionOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.IssueListDataByProudctionOrder(IssueStatus, IssueSlipType, productionOrderId), JsonRequestBehavior.AllowGet);
        }

        #region Requisition Inventory IssueUI Or Request

        [Authorize, HttpGet]
        public JsonResult GetRequisitionIssueDetail(string issueId)
        {
            return Json(_inventoryReveiveService.GetRequisitionIssueDetail(issueId), JsonRequestBehavior.AllowGet);

        }


        [Authorize, HttpGet]
        public JsonResult IssueDetailData(string status)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.IssueDetailData(status,identity.EmployeeId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult RequisitionIssueListData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.RequisitionIssueListData(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RequisitionIssueInsert(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
            , IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var inventoryIssue = new InventoryIssue();
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.RequisitionIssueInsert(entities, specificStockList, inventoryIssue, requisitionIssueDetails);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RequisitionIssueUpdate(string issueId, IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
           , IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var inventoryIssue = new InventoryIssue();
            inventoryIssue.Id = issueId;
            _inventoryIssueService.RequisitionIssueUpdate(entities, specificStockList, inventoryIssue, requisitionIssueDetails);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region  IssueSlipChecked and Approval
        [HttpGet, Authorize]
        public JsonResult IssueSlipUnChecked(string IssuStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.IssueSlipUnChecked(IssuStatus), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult IssueSlipChecked()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.IssueSlipChecked(), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult IssueSlipToChecked(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //if(identity.EmployeeId== AuthorizedBy)
            //{

            //}
            _issueRequestService.IssueSlipToChecked(PoId, PoValue, CheckedStataus, AuthorizedBy);
            return Json(new { Message = "Issue Slip Checked" + AplosMessage.Success });
        }

        #endregion


        #region  ApprovingIssueSlip 


        [HttpGet, Authorize]
        public JsonResult IssueSlipUnApproved(string IssuAppStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.IssueSlipUnApproved(IssuAppStatus), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult IssueSlipApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_issueRequestService.IssueSlipApproved(), JsonRequestBehavior.AllowGet);
        }



        [HttpPost, Authorize]
        //string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
        public JsonResult IssueSlipToApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            _issueRequestService.IssueSlipToApproved(PoId, PoValue, CheckedStataus, AuthorizedBy);
            return Json(new { Message = "Issue Slip Approved" + AplosMessage.Success });
        }

        #endregion

        #region MaterialWiseIssueSlip 2 Step 
        [Authorize, HttpPost]
        public JsonResult GetIssueSlipFilterData(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetIssueSlipFilterData(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpPost]
        public JsonResult GetStockForMaterialIssue(string materialMasterId,string articleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetStockForMaterialIssue(identity.PlantId, materialMasterId, articleId), JsonRequestBehavior.AllowGet);

        }
        #endregion


        #region AssetIssueWiseIssueSlip  
        [Authorize, HttpGet]
        public JsonResult GetAssetIssueSlipFilterData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetAssetIssueSlipFilterData(), JsonRequestBehavior.AllowGet);

        }
        #endregion


        #region Purchase Return Code Start Here
        [HttpGet, Authorize]
        public JsonResult GetListPurchaseReturnData(string plantId, string tabType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                if (tabType == "ForChecking")
                {
                    sql = @"select * from(SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE  IR.PlantId='" + identity.PlantId + @"'
						 and IR.CheckedByStatus='For Checking'
						  and IR. ApprovedbyStatus Is Null

			  UNION ALL
			      SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE  IR.PlantId='" + identity.PlantId + @"'
						and IR.CheckedByStatus Is Null 
						and IR. ApprovedbyStatus='For Approval'

			  UNION All 


			  SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE   IR.PlantId='" + identity.PlantId + @"' 
					and IR.CheckedByStatus Is Null 
					and IR.ApprovedbyStatus Is Null
					)x
              order by POReturnDate  ASC";
                }
                else if (tabType == "CheckedHoldReject")
                {
                    sql = @"SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + identity.PlantId + @"'
					AND	IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' 
					and IR.ApprovedbyStatus IS NULL
			   order by IR.POReturnDate  ASC";


                }
                else if (tabType == "Checked")
                {
                    sql = @"        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                          WHERE IR.PlantId='" + identity.PlantId + @"'
						AND IR.CheckedByStatus='Checked'
						AND IR.ApprovedByStatus='For Approval'
                order by IR.POReturnDate  ASC";



                }
                else if (tabType == "ApprovedHoldReject")
                {
                    sql = @"
                       
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + identity.PlantId + @"'
						AND IR.ApprovedByStatus='Hold' OR IR.ApprovedByStatus='Reject'
                        AND IR.CheckedByStatus='Checked'
                        order by IR.POReturnDate  ASC";

                }
                else if (tabType == "Approved")
                {
                    sql = @"Select * from(
                        
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                       WHERE IR.PlantId='" + identity.PlantId + @"'  
						AND  IR.ApprovedByStatus='Approved'
						 AND IR.CheckedByStatus='Checked'

						 UNION ALL 

						  SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                       WHERE IR.PlantId='" + identity.PlantId + @"' 
						AND IR.CheckedByStatus IS NULL
                      AND  IR.ApprovedByStatus='Approved'
                  

				   UNION ALL 

				    SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                       WHERE IR.PlantId='" + identity.PlantId + @"'
						AND IR.CheckedByStatus IS NULL
                      AND  IR.ApprovedByStatus IS NULL
					  ) X
					 order by POReturnDate  ASC";



                }
                else if (tabType == "Posted")
                {
                    sql = @"
                             SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,
						            IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate, 106),' ','-') AS GRNDate1
                                     ,IR.POReturnDate 
                                    , IR.CompanyGroupId,  IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved,
									IR.CheckedByStatus,IR.ApprovedByStatus
	                                , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName
									, IR.NoteForAccounts,IR.CheckedBy,IR.InventoryReceiveId
                        FROM [TRN].[PurchaseReturn] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                     
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                        LEFT JOIN (SELECT A.PurchaseReturnId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId, A.TransactionUoMId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId, A.TransactionUoMId HAVING COUNT(A.PurchaseReturnId)> COUNT(A.TransactionUoMId)) AS TU ON TU.PurchaseReturnId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    WHERE  IR.ApprovedByStatus='Posted'
						AND IR.PlantId='" + identity.PlantId + @"' 
						AND ISNULL(IR.[Status],'')<>'Posting' 
						AND IR.OpeningBalanceId IS NULL 
						AND IR.EmployeeId IS NULL 
						And IR.IsApproved = 0 
						--and IR.GRNType='GRNBYREQPO' 
                    order by IR.POReturnDate  ASC";


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
        [HttpGet, Authorize]
        public JsonResult PurchaseReturnApprovedCeackList(string tabType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";


                if (tabType == "UnCheckedList")
                {
                    sql = @"
                            
                            
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate , 106),' ','-') AS POReturnDate 
                                    , IR.CompanyGroupId
									, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState,
									CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved
									--, IR.NoteForAccounts--,IRD.BaseAmount
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason
                                 ,sum(IRD.TransactionQty)  TransactionQty
									,Sum(IRD.BaseAmount) BaseAmount
	                        ,IR.ApprovedByStatus AS ApprovedByStatus
                        FROM [TRN].[PurchaseReturn] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                         LEFT JOIN (SELECT A.PurchaseReturnId,Sum(A.TransactionQty) TransactionQty, sum(A.TotalMaterialBooksCurrencyAmount) BaseAmount
						 FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId,A.TransactionQty ) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"'GROUP BY A.PurchaseReturnId  HAVING COUNT(A.PurchaseReturnId)> 
									COUNT(A.MaterialMasterId)) 
									AS TU ON TU.PurchaseReturnId=IR.Id
                              Where IR.CheckedBy='" + identity.EmployeeId + @"' 
                              AND IR.CheckedBy Is NOT NULL 
                              And IR.[CheckedByStatus]='For Checking'  
                              AND IR.[ApprovedBy] IS NULL 
                              AND IR.[ApprovedByStatus] Is NULL
							Group BY  IR.Id
                                    , IR.POReturnDate
                                    , IR.CompanyGroupId
									, IR.PlantId
									, IR.PartyId
									, P.Code
									, P.UserName
			                        , CP.UserName
									,IR.DocDate
									, IR.CurrencyId
									, CU.Code
									, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId
									, IPP.UserName
									, IR.InvoicingByAddress
									, IR.DeliveryPartyPlantId
									, DPP.UserName
									, IR.DeliveryByAddress
									, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName
									, S2.UserName
									,CP.TaxApplicable
									, CP.IsTaxApplicableChangeable
									, IR.IsTaxApplicable
									, IR.IsApproved
									--, isnull(IR.NoteForAccounts,'')
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName 
									,EI1.EmployeeName 
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason                                   
	                                ,IR.ApprovedByStatus , IR.DocRefNo";
                }
                else if (tabType == "HoldRejectCheckedList")
                {
                    sql = @"
                       SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate , 106),' ','-') AS POReturnDate 
                                    , IR.CompanyGroupId
									, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState,
									CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved
									--, IR.NoteForAccounts--,IRD.BaseAmount
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason
                                ,sum(IRD.TransactionQty)  TransactionQty
									,Sum(IRD.BaseAmount) BaseAmount
	                        ,IR.ApprovedByStatus AS ApprovedByStatus
                        FROM [TRN].[PurchaseReturn] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                         LEFT JOIN (SELECT A.PurchaseReturnId,Sum(A.TransactionQty) TransactionQty, sum(A.TotalMaterialBooksCurrencyAmount) BaseAmount
						 FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId,A.TransactionQty ) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"'GROUP BY A.PurchaseReturnId  HAVING COUNT(A.PurchaseReturnId)> 
									COUNT(A.MaterialMasterId)) 
									AS TU ON TU.PurchaseReturnId=IR.Id
                          Where IR.CheckedBy='" + identity.EmployeeId + @"' 
                          AND IR.CheckedBy Is NOT NULL 
                          And IR.[CheckedByStatus]='Hold' OR IR.[CheckedByStatus]='Reject'
                          AND IR.[ApprovedBy] IS NULL OR IR.ApprovedBy = ''
                          AND IR.[ApprovedByStatus] Is NULL
							Group BY  IR.Id
                                    , IR.POReturnDate
                                    , IR.CompanyGroupId
									, IR.PlantId
									, IR.PartyId
									, P.Code
									, P.UserName
			                        , CP.UserName
									,IR.DocDate
									, IR.CurrencyId
									, CU.Code
									, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId
									, IPP.UserName
									, IR.InvoicingByAddress
									, IR.DeliveryPartyPlantId
									, DPP.UserName
									, IR.DeliveryByAddress
									, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName
									, S2.UserName
									,CP.TaxApplicable
									, CP.IsTaxApplicableChangeable
									, IR.IsTaxApplicable
									, IR.IsApproved
									--, isnull(IR.NoteForAccounts,'')
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName 
									,EI1.EmployeeName 
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason                                   
	                                ,IR.ApprovedByStatus , IR.DocRefNo";
                }
                else if (tabType == "CheckedList")
                {
                    sql = @"
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate , 106),' ','-') AS POReturnDate 
                                    , IR.CompanyGroupId
									, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState,
									CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved
									--, IR.NoteForAccounts--,IRD.BaseAmount
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason
                                    ,sum(IRD.TransactionQty)  TransactionQty
									,Sum(IRD.BaseAmount) BaseAmount
	                        ,IR.ApprovedByStatus AS ApprovedByStatus
                        FROM [TRN].[PurchaseReturn] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                         LEFT JOIN (SELECT A.PurchaseReturnId,Sum(A.TransactionQty) TransactionQty, sum(A.TotalMaterialBooksCurrencyAmount) BaseAmount
						 FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId,A.TransactionQty ) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"'GROUP BY A.PurchaseReturnId  HAVING COUNT(A.PurchaseReturnId)> 
									COUNT(A.MaterialMasterId)) 
									AS TU ON TU.PurchaseReturnId=IR.Id
                              Where IR.CheckedBy='" + identity.EmployeeId + @"' 
                          AND IR.CheckedBy Is NOT NULL 
                          And IR.[CheckedByStatus]='Checked' 
                          AND IR.[ApprovedBy] IS NOT NULL 
                          AND IR.[ApprovedByStatus]='For Approval'
						Group BY  IR.Id
                                    , IR.POReturnDate
                                    , IR.CompanyGroupId
									, IR.PlantId
									, IR.PartyId
									, P.Code
									, P.UserName
			                        , CP.UserName
									,IR.DocDate
									, IR.CurrencyId
									, CU.Code
									, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId
									, IPP.UserName
									, IR.InvoicingByAddress
									, IR.DeliveryPartyPlantId
									, DPP.UserName
									, IR.DeliveryByAddress
									, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName
									, S2.UserName
									,CP.TaxApplicable
									, CP.IsTaxApplicableChangeable
									, IR.IsTaxApplicable
									, IR.IsApproved
									--, isnull(IR.NoteForAccounts,'')
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName 
									,EI1.EmployeeName 
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason                                   
	                                ,IR.ApprovedByStatus , IR.DocRefNo";
                }
                else if (tabType == "UnApprovedList")
                {
                    sql = @"
                       SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate , 106),' ','-') AS POReturnDate 
                                    , IR.CompanyGroupId
									, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState,
									CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved
									--, IR.NoteForAccounts--,IRD.BaseAmount
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason
                                ,sum(IRD.TransactionQty)  TransactionQty
									,Sum(IRD.BaseAmount) BaseAmount
	                        ,IR.ApprovedByStatus AS ApprovedByStatus
                        FROM [TRN].[PurchaseReturn] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                         LEFT JOIN (SELECT A.PurchaseReturnId,Sum(A.TransactionQty) TransactionQty, sum(A.TotalMaterialBooksCurrencyAmount) BaseAmount
						 FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId,A.TransactionQty ) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"'GROUP BY A.PurchaseReturnId  HAVING COUNT(A.PurchaseReturnId)> 
									COUNT(A.MaterialMasterId)) 
									AS TU ON TU.PurchaseReturnId=IR.Id
                          Where IR.ApprovedBy='" + identity.EmployeeId + @"'  
                          AND IR.CheckedBy Is NOT NULL 
                          And IR.[CheckedByStatus]='Checked' 
                          AND IR.[ApprovedBy] IS NOT NULL 
                          AND IR.[ApprovedByStatus]='For Approval'
						Group BY  IR.Id
                                    , IR.POReturnDate
                                    , IR.CompanyGroupId
									, IR.PlantId
									, IR.PartyId
									, P.Code
									, P.UserName
			                        , CP.UserName
									,IR.DocDate
									, IR.CurrencyId
									, CU.Code
									, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId
									, IPP.UserName
									, IR.InvoicingByAddress
									, IR.DeliveryPartyPlantId
									, DPP.UserName
									, IR.DeliveryByAddress
									, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName
									, S2.UserName
									,CP.TaxApplicable
									, CP.IsTaxApplicableChangeable
									, IR.IsTaxApplicable
									, IR.IsApproved
									--, isnull(IR.NoteForAccounts,'')
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName 
									,EI1.EmployeeName 
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason                                   
	                                ,IR.ApprovedByStatus , IR.DocRefNo
UNION ALL
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate , 106),' ','-') AS POReturnDate 
                                    , IR.CompanyGroupId
									, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState,
									CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved
									--, IR.NoteForAccounts--,IRD.BaseAmount
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason
                                    ,sum(IRD.TransactionQty)  TransactionQty
									,Sum(IRD.BaseAmount) BaseAmount 
	                        ,IR.ApprovedByStatus AS ApprovedByStatus
                        FROM [TRN].[PurchaseReturn] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                         LEFT JOIN (SELECT A.PurchaseReturnId,Sum(A.TransactionQty) TransactionQty, sum(A.TotalMaterialBooksCurrencyAmount) BaseAmount
						 FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId,A.TransactionQty ) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"'GROUP BY A.PurchaseReturnId  HAVING COUNT(A.PurchaseReturnId)> 
									COUNT(A.MaterialMasterId)) 
									AS TU ON TU.PurchaseReturnId=IR.Id
                          Where IR.ApprovedBy= '" + identity.EmployeeId + @"'
                          AND IR.CheckedBy Is  NULL
                          And IR.[CheckedByStatus]  Is NULL
                          AND IR.[ApprovedBy] IS NOT NULL
                          AND IR.[ApprovedByStatus]= 'For Approval'
						Group BY  IR.Id
                                    , IR.POReturnDate
                                    , IR.CompanyGroupId
									, IR.PlantId
									, IR.PartyId
									, P.Code
									, P.UserName
			                        , CP.UserName
									,IR.DocDate
									, IR.CurrencyId
									, CU.Code
									, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId
									, IPP.UserName
									, IR.InvoicingByAddress
									, IR.DeliveryPartyPlantId
									, DPP.UserName
									, IR.DeliveryByAddress
									, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName
									, S2.UserName
									,CP.TaxApplicable
									, CP.IsTaxApplicableChangeable
									, IR.IsTaxApplicable
									, IR.IsApproved
									--, isnull(IR.NoteForAccounts,'')
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName 
									,EI1.EmployeeName 
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason                                   
	                                ,IR.ApprovedByStatus , IR.DocRefNo";


                }
                else if (tabType == "HoldRejectApprovedList")
                {
                    sql = @"
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate , 106),' ','-') AS POReturnDate 
                                    , IR.CompanyGroupId
									, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState,
									CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved
									--, IR.NoteForAccounts--,IRD.BaseAmount
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason
                                    ,sum(IRD.TransactionQty)  TransactionQty
									,Sum(IRD.BaseAmount) BaseAmount
	                        ,IR.ApprovedByStatus AS ApprovedByStatus
                        FROM [TRN].[PurchaseReturn] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                         LEFT JOIN (SELECT A.PurchaseReturnId,Sum(A.TransactionQty) TransactionQty, sum(A.TotalMaterialBooksCurrencyAmount) BaseAmount
						 FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId,A.TransactionQty ) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"'GROUP BY A.PurchaseReturnId  HAVING COUNT(A.PurchaseReturnId)> 
									COUNT(A.MaterialMasterId)) 
									AS TU ON TU.PurchaseReturnId=IR.Id
                          Where IR.ApprovedBy='" + identity.EmployeeId + @"' 
                          AND IR.CheckedBy Is NOT NULL 
                          And IR.[CheckedByStatus]='Checked' 
                          AND IR.[ApprovedBy] IS NOT NULL 
                          AND IR.[ApprovedByStatus]='Hold' Or  IR.[ApprovedByStatus]='Reject'
                          Group BY  IR.Id
                                    , IR.POReturnDate
                                    , IR.CompanyGroupId
									, IR.PlantId
									, IR.PartyId
									, P.Code
									, P.UserName
			                        , CP.UserName
									,IR.DocDate
									, IR.CurrencyId
									, CU.Code
									, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId
									, IPP.UserName
									, IR.InvoicingByAddress
									, IR.DeliveryPartyPlantId
									, DPP.UserName
									, IR.DeliveryByAddress
									, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName
									, S2.UserName
									,CP.TaxApplicable
									, CP.IsTaxApplicableChangeable
									, IR.IsTaxApplicable
									, IR.IsApproved
									--, isnull(IR.NoteForAccounts,'')
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName 
									,EI1.EmployeeName 
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason                                   
	                                ,IR.ApprovedByStatus , IR.DocRefNo";
                }
                else if (tabType == "ApprovedList")
                {
                    sql = @"
                       SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.POReturnDate , 106),' ','-') AS POReturnDate 
                                    , IR.CompanyGroupId
									, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
									, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
									, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState,
									CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved
									--, IR.NoteForAccounts--,IRD.BaseAmount
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName CheckedBy
									,EI1.EmployeeName ApprovedBy
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason
                                ,sum(IRD.TransactionQty)  TransactionQty
									,Sum(IRD.BaseAmount) BaseAmount
	                        ,IR.ApprovedByStatus AS ApprovedByStatus
                        FROM [TRN].[PurchaseReturn] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.ApprovedBy
                         LEFT JOIN (SELECT A.PurchaseReturnId,Sum(A.TransactionQty) TransactionQty, sum(A.TotalMaterialBooksCurrencyAmount) BaseAmount
						 FROM [TRN].[PurchaseReturnDetail] AS A
		                            JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.PurchaseReturnId,A.TransactionQty ) AS IRD ON IRD.PurchaseReturnId=IR.Id
                        LEFT JOIN (SELECT A.PurchaseReturnId FROM [TRN].[PurchaseReturnDetail] AS A JOIN [TRN].ServiceAcknowledgementMaster AS B ON A.PurchaseReturnId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"'GROUP BY A.PurchaseReturnId  HAVING COUNT(A.PurchaseReturnId)> 
									COUNT(A.MaterialMasterId)) 
									AS TU ON TU.PurchaseReturnId=IR.Id
                          Where IR.ApprovedBy='" + identity.EmployeeId + @"' 
                          AND IR.CheckedBy Is NOT NULL 
                          And IR.[CheckedByStatus]='Checked' 
                          AND IR.[ApprovedBy] IS NOT NULL 
                          AND IR.[ApprovedByStatus]='Approved'
						  Group BY  IR.Id
                                    , IR.POReturnDate
                                    , IR.CompanyGroupId
									, IR.PlantId
									, IR.PartyId
									, P.Code
									, P.UserName
			                        , CP.UserName
									,IR.DocDate
									, IR.CurrencyId
									, CU.Code
									, IR.BaseCurrencyId
	                                , IR.InvoicingPartyPlantId
									, IPP.UserName
									, IR.InvoicingByAddress
									, IR.DeliveryPartyPlantId
									, DPP.UserName
									, IR.DeliveryByAddress
									, IR.IsNonCreditable
									, IR.ToCurrencyRate
                                    , S1.UserName
									, S2.UserName
									,CP.TaxApplicable
									, CP.IsTaxApplicableChangeable
									, IR.IsTaxApplicable
									, IR.IsApproved
									--, isnull(IR.NoteForAccounts,'')
                                   	,IR.CheckedByStatus
									,IR.ApprovedByStatus
									,EI.EmployeeName 
									,EI1.EmployeeName 
									,IR.AddedBy 
	                                ,IR.CheckedHoldRejectReason                                   
	                                ,IR.ApprovedByStatus , IR.DocRefNo";
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


        //string PoValue,
        [HttpPost, Authorize]
        public void PurchaseReturnCheckedAndApproved(string Id, string PurchaseReturnValue, string CheckedApprovedStataus, string CheckedApprovedBy, string RejectReason, string UIType)
        {
            var ApprovedById = "";
            var ApprovedByStatus = "";
            if (UIType == "Purchase-Return-Checked")
            {
                try
                {



                    PurchaseReturnValue = "0";
                    //  var Id = GetPK();
                    if (CheckedApprovedStataus == "Checked")
                    {
                        if (CheckedApprovedBy == null || CheckedApprovedBy == "")
                        {
                            throw new CustomException("Select Approved By");
                        }
                        ApprovedById = CheckedApprovedBy;
                        ApprovedByStatus = "For Approval";

                        //DailySendMailRequisitionApproved(RequisitionType, RequirmentType, CheckedBy, AuthorizedById, PoId, PreparedBY);

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


                    string _sql = "Update TRN.PurchaseReturn set CheckedByStatus='" + Status + "',ApprovedBy='" + ApprovedById + "',CheckedHoldRejectReason='" + RejectReason + "',ApprovedByStatus='" + ApprovedByStatus + "',IsApproved='" + 1 + "' where id='" + Id + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                    string _sql1 = "Insert into TRN.PurchaseReturnpprovalLogTbl(" +
                    "CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,PurchaseReturnValue,Status,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIp,PurchaseReturnID) " +
                    "values ('" + CompanyGroupId + "', " +
                    "'" + CompanyId + "'," +
                    "'" + PlantId + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + PurchaseReturnValue + "'," +
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
            else if (UIType == "Purchase-Return-Approved")
            {
                try
                {
                    var IsApproved = 0;

                    PurchaseReturnValue = "0";
                    //  var Id = GetPK();
                    if (CheckedApprovedStataus == "Approved")
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
                    string _sql = "Update TRN.PurchaseReturn set ApprovedByStatus='" + Status + "',ApprovedHoldRejectReason='" + RejectReason + "' where id='" + Id + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                    string _sql1 = "Insert into TRN.PurchaseReturnpprovalLogTbl(" +
                    "CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,PurchaseReturnValue,Status,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIp,PurchaseReturnID) " +
                    "values ('" + CompanyGroupId + "', " +
                    "'" + CompanyId + "'," +
                    "'" + PlantId + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + PurchaseReturnValue + "'," +
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
        [Authorize, HttpPost]
        public JsonResult PostedGRNListForPurchaseReturn(string column, string value, string plantId)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var Sql = @"select top 300 * from (SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,isnull(IR.POID,'') POID,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName, IR.NoteForAccounts
                                    ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END,IR.GRNType
                        FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						--left join trn.POGGRNMap map on map.GRNId=ir.id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						WHERE IR.Status='Posting' AND IR.GRNType<>'FG' AND IR.PlantId='" + identity.PlantId + @"'
                        UNION ALL
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.DocRefNo InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,isnull(IR.POID,'') POID,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName, IR.NoteForAccounts
                                    ,IsOpeningBalance=CASE WHEN IR.OpeningBalanceId IS NOT NULL THEN 'Yes' ELSE 'No' END,IR.GRNType
                        FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
						--left join trn.POGGRNMap map on map.GRNId=ir.id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='202034' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						WHERE   IR.GRNType<>'FG' AND IR.PlantId='" + identity.PlantId + @"' AND (CheckedByStatus='Reject' OR AuthorizedByStatus='Reject') AND ir.IsApproved=0
                        ) AS TEMP WHERE " + strkey + " Order by GRNDate  DESC";
                var res = _sqlRepository.GetDataCollection(Sql);
                var jsondata = Json(res, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

                //return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [Authorize, HttpPost]
        public JsonResult GetGRNBOQListForPurchaseReturn( string InventoryreceiveDetailId)
        {
            try
            {
                var Sql = @"select   IM.MaterialMasterId, MM.UserName MaterialName
                                        , IM.ArticleId, MMA.StandardName ArticleName
                                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue 
										, GRNBOQ.TransactionQty BOQQty,TUoM.UserName TUOM 
                                        , GRNBOQ.*
										FROM [TRN].[GRNPORequisitionAllocation] GRNBOQ
										LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=GRNBOQ.InventoryReceiveDetailId
										LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
										LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
										LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=IM.ArticleId
										LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
										LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
										LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
										LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
										LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON GRNBOQ.TransactionUoMId=TUoM.Id
										WHERE GRNBOQ.InventoryreceiveDetailId='" + InventoryreceiveDetailId + "'";
                var res = _sqlRepository.GetDataCollection(Sql);
                var jsondata = Json(res, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

                //return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListForPurchaseReturn(GridParameter parameters, string inveReveiveId, string POID, string AcceptanceId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                var Sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                                     , @totalReceiveAmount DECIMAL(18, 4)=0
                                                     , @totalServiceAmount DECIMAL(18, 4)=0
                                                     , @totalSvcTaxAmount DECIMAL(18, 4)=0
                                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                  SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                                      ,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106),' ','-') AS AddedDate
                                        , MGM.UserName AS MaterialGroupMasterName
                                        , IM.MaterialMasterId, MM.UserName
                                        , IM.ArticleId, ART.StandardName
                                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                        , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                        , IRD.MaterialTranRate AS TransactionRate
                                        , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                        --, (IRD.TransactionQty*IRD.MaterialTranRate) AS TrnAmount
                                        --, (((((ISNULl(IRD.TransactionQty,0)-ISNULL(IRD.BaseIssueQty,0))-Isnull(IRD.PurchaseReturnQty,0))+Isnull(IRD.IssueReturnQty,0))-Isnull(IRD.ReductionByAdjustmentQty,0))*IRD.MaterialTranRate) AS TrnAmount
                                         ,0 AS TrnAmount
                                        , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                        --, IRD.TotalTaxAmount AS BaseTaxAmount
                                        ,0 AS BaseTaxAmount
                                        , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
                                        , IRD.ChargesTranAmount/IRD.TransactionQty AS ServiceChargeGRN
										, IRD.ChargesTaxTranAmount AS ServiceTaxGRN	 
										,IRD.TotalTaxAmount	 BaseTaxAmountGRN                      
                                        --, ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                        --, ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
                                         , 0 ServiceCharge
                                         , 0 ServiceTax
                                        , IRD.CountryId
                                        , PID.TransactionQty AS POQty
                                        , ISNULL(Pre.OtherReceived,0) OtherReceived	                       
                                        , IRD.TransactionQty GRNReceived   
                                         , BaseIssueQty=isnull(IRD.BaseIssueQty,0)-Isnull(IRD.IssueReturnQty,0)
                                        
                                        ,Isnull(IRD.PurchaseReturnQty,0) OtherReturned
                                       ,0 TransactionQty
                                        , ((((((ISNULl(IRD.TransactionQty,0)-ISNULL(IRD.BaseIssueQty,0))-Isnull(IRD.PurchaseReturnQty,0))+Isnull(IRD.IssueReturnQty,0))-Isnull(IRD.ReductionByAdjustmentQty,0))-Isnull(IRD.InventorySalesQty,0))-Isnull(IRD.InventoryScrapQty,0)) AS Balance
                                        --, ((((ISNULl(IRD.TransactionQty,0)-ISNULL(IRD.BaseIssueQty,0))-Isnull(IRD.PurchaseReturnQty,0))+Isnull(IRD.IssueReturnQty,0))-Isnull(IRD.ReductionByAdjustmentQty,0)) TransactionQty
                                        , IRD.TransactionUoMId
                                        , IRD.BaseUOMId   
                                        , IRD.MaterialTranRate
                                        --, IRD.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount
                                        , 0 TotalMaterialTranAmount
                                        , 0  AS TotalMaterialBaseAmount,IRD.BaseUoMFactor
                                        ,IRD.InventoryMaterialId
                                        ,IRD.PurchaseDocumentAcceptanceDetailId
										,IRD.PurchaseDocumentAcceptanceId
                                        , IRD.ShortageQty
                                        , IRD.RejectionQty
                                        , IRD.ApprovedQty
                                        , IRD.TransactionQty AS PreviousQty
                                        , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName,MS.UserName MaterialStorage,MS.Id MaterialStorageId
                                        , IRD.ShortageRatePercent AS ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent AS RejectionRate,IRD.RejectValue AS RejectionValue
                                        ,IRD.RejectClamPercent RejectionClamRate,IR.CheckedBy,PID.Description MaterialDetail
                                        ,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty
										,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty
										,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty
                                        ,ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty
                                        ,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty
                                        ,isnull(IRD.InventoryTransferQty,0) InventoryTransferQty
                                        ,Format(IRD.TrnCurrencyBaseRate,'N4') TrnCurrencyBaseRate
										,Format(IRD.BooksCurrencyBaseRate,'N4') BooksCurrencyBaseRate,TotalMatSum=IRD.MaterialTranRate*IRD.TransactionQty
                                  from TRN.InventoryMaterial AS IM
                                  JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                  LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + @"'
                                  LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
                                  LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
                                  from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + "') --AND POid='" + POID + @"'
                                  Group By PODetailsId) AS Pre on pre.PODetailsId=IRD.PODetailsId
                                JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                                JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
								LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                                Left join [HKP].[MaterialStorage] MS on MS.id=IRD.MaterialStorageId
                                --left join (select InventoryReceiveDetailId, sum(TransactionQty) OtherReturn from trn.PurchaseReturnDetail group BY InventoryReceiveDetailId) res on res.InventoryReceiveDetailId=IRD.Id
                                WHERE IRD.InventoryReceiveId=@inventoryReceiveId and IM.MaterialMasterId IS not null";

                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        [Authorize, HttpGet]
        public JsonResult GetPurchaseReturnCheckedBy()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseReturnCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
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
        public JsonResult GetReceiveTaxListPO1(string receiveDetailId)
        {
            return Json(_inventoryReveiveService.GetReceiveTaxListPO(receiveDetailId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetReceiveTaxListGRN(string receiveDetailId)
        {
            string paramter = "";
            //if (receiveDetailId != "")
            //{
            //    if (paramter == "")
            //        paramter += "A.InventoryReceiveId in(" + receiveDetailId + ")";
            //    else
            //        paramter += " AND A.InventoryReceiveId in(" + receiveDetailId + ")";
            //}
            try
            {
                var sql = @"SELECT A.Id,A.InventoryReceiveDetailId
	                        ,A.InventoryReceiveId
	                        , A.TaxCategoryId
	                        , TC.UserName AS TaxCategory
	                        , A.HSNCodeId
	                        , HN.Code AS HSNCode
	                        , A.[Percentage]
	                        , A.TaxAmount
	                        ,d.id As PODetailId
                        FROM [TRN].[InventoryReceiveTax] AS A 
                        JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                        LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                        left join TRN.InventoryReceiveDetail d on d.id= A.InventoryReceiveDetailId
                        WHERE A.InventoryServiceId IS NULL AND A.InventoryReceiveId='" + receiveDetailId + "' ORDER BY TC.[Sequence]";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        [Authorize, HttpGet]
        public JsonResult PurchaseReturnDetailsData(string PurchaseReturnId, string POID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_inventoryMaterialService.PurchaseReturnDetailsData(PurchaseReturnId, POID), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }


        [Authorize, HttpGet]
        public ActionResult PurchaseReturnReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.PurchaseReturnReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

            return null;
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListForPurchaseReturnModify(GridParameter parameters, string inveReveiveId, string POID, string AcceptanceId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                var Sql = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
                                                     , @totalReceiveAmount DECIMAL(18, 4)=0
                                                     , @totalServiceAmount DECIMAL(18, 4)=0
                                                     , @totalSvcTaxAmount DECIMAL(18, 4)=0
                                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                                   SELECT IM.Id InventoryReceiveDetailId,IM.PurchaseReturnId
                                        --,REPLACE(CONVERT(CHAR(11), PID.AddedDate, 106),' ','-') AS AddedDate
                                        , MGM.UserName AS MaterialGroupMasterName
                                        , IM.MaterialMasterId, MM.UserName
                                        , IM.ArticleId, ART.StandardName
                                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                                        , IM.TransactionUoMId, TUoM.UserName AS TransactionUoM
                                        , IM.MaterialTranRate AS TransactionRate
                                        , CU.Code AS CurrencyName, IR.ToCurrencyRate
                                        , (IM.TransactionQty*IM.MaterialTranRate) AS TrnAmount
                                        , IM.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                        , IM.TotalTaxAmount AS BaseTaxAmount
                                        , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IM.Id)
                                        , IM.ChargesTranAmount AS ChargesAmount	                      
                                         ,IM.ChargesTranAmount ServiceCharge
                                        ,IM.ChargesTaxTranAmount ServiceTax
                                        , IM.CountryId
                                       -- , PID.TransactionQty AS POQty
                                        --, ISNULL(Pre.OtherReceived,0) OtherReceived	 
                                        ,IM.MaterialTranRate TransactionRate
                                        , ISNULl(res1.Received,0) GRNReceived   
                                        , isnull(res1.BaseIssueQty,0) BaseIssueQty
										 ,isnull(res.OtherReturn,0) OtherReturned
                                          --,0 OtherReturned
                                        ,(ISNULl(IM.TransactionQty,0)) oldReturnQty
                                        , (ISNULl(IM.TransactionQty,0)) TransactionQty
                                        ,(ISNULl(res1.Received,0)-((ISNULL(res1.BaseIssueQty,0)+isnull(res.OtherReturn,0)+isnull(res1.PurchaseReturnQty,0)+isnull(res1.ReductionByAdjustmentQty,0)+isnull(res1.InventorySalesQty,0)+isnull(res1.InventoryScrapQty,0))+isnull(res1.IssueReturnQty,0))) AS Balance
                                        , IM.TransactionUoMId
                                        , IM.BaseUOMId   
                                        , IM.TotalMaterialTranAmount
                                        , IM.TotalMaterialBooksCurrencyAmount AS TotalMaterialBaseAmount
                                       
                                        --,IM.PurchaseDocumentAcceptanceDetailId
										--,IM.PurchaseDocumentAcceptanceId
                                        --, IM.ShortageQty
                                        ----, IM.RejectionQty
                                        --, IM.ApprovedQty
                                        , IM.TransactionQty AS PreviousQty
                                        , EI.EmployeeName CheckedByName
										, EI1.EmployeeName ApprovedByName,MS.UserName MaterialStorage,MS.Id MaterialStorageId
                                        --, IM.ShortageRatePercent AS ShortageRate,IM.ShortageValue,IM.RejectRatePercent AS RejectionRate,IM.RejectValue AS RejectionValue,IM.RejectClamPercent RejectionClamRate
										,IR.CheckedBy
										,IM.Description MaterialDetail,IM.InventoryMaterialId
                                       ,IM.InventoryReceiveDetailId InventoryServiceId
										,IM.InventoryReceiveId
                                       ,isnull(res1.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty
										  ,isnull(res1.InventorySalesQty,0) InventorySalesQty
										  ,isNULL(res1.InventoryScrapQty,0) InventoryScrapQty 
                                          ,isNULL(res1.PurchaseReturnQty,0) PurchaseReturnQty 
                                          ,isNULL(res1.IssueReturnQty,0) IssueReturnQty 
										  ,isNULL(res1.InventoryTransferQty,0) InventoryTransferQty
                                  from TRN.PurchaseReturnDetail AS IM
                                  left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                  --LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='19123'
                                  --LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
             --                     LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
											  --from trn.InventoryReceiveDetail where InventoryReceiveId not in('19123') --AND POid='null'
											  --Group By PODetailsId
											  --) AS Pre on pre.PODetailsId=IRD.PODetailsId
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
                                LEFT JOIN [TRN].[InventoryReceive] AS IR ON IM.InventoryReceiveId=IR.Id
                                LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                --LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.ID=PID.RequisitionDetailId
                                LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
								LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                                Left join [HKP].[MaterialStorage] MS on MS.id=IM.MaterialStorageId
                                left join (select InventoryReceiveDetailId,Id,sum(TransactionQty) OtherReturn from trn.PurchaseReturnDetail group BY InventoryReceiveDetailId,Id) res on res.Id=IM.Id ANd  res.Id <> IM.Id
								left join (select Id, sum(TransactionQty) Received,sum(BaseIssueQty) BaseIssueQty 
                                          ,sum(isnull(ReductionByAdjustmentQty,0)) ReductionByAdjustmentQty
										  ,sum(isnull(InventorySalesQty,0)) InventorySalesQty
										  ,sum(isNULL(InventoryScrapQty,0)) InventoryScrapQty 
                                          ,sum(isNULL(PurchaseReturnQty,0)) PurchaseReturnQty 
                                          ,sum(isNULL(IssueReturnQty,0)) IssueReturnQty,sum(isnull(InventoryTransferQty,0)) InventoryTransferQty
                                           FROM trn.InventoryReceiveDetail group BY Id
                                          ) res1 on res1.Id=IM.InventoryReceiveDetailId

                                WHERE IM.PurchaseReturnId=@inventoryReceiveId and IM.MaterialMasterId IS not null";

                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        [Authorize, HttpGet]
        public JsonResult GetReceiveTaxListGRNPurchaseReturnModify(string receiveDetailId)
        {
            string paramter = "";

            try
            {
                var sql = @"SELECT A.Id,A.PurchaseReturnDetailId
	                        ,A.PurchaseReturnId
	                        , A.TaxCategoryId
	                        , TC.UserName AS TaxCategory
	                        , A.HSNCodeId
	                        , HN.Code AS HSNCode
	                        , A.[Percentage]
	                        , A.TaxAmount
	                        ,d.id As PODetailId
                        FROM [TRN].[PurchaseReturnTax] AS A 
                        JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                        LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                        left join TRN.PurchaseReturnDetail d on d.id= A.PurchaseReturnDetailId
                        WHERE A.InventoryServiceId IS NULL 
						AND A.PurchaseReturnId='" + receiveDetailId + "' ORDER BY TC.[Sequence]";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetReceiveTaxListGRNPurchaseReturnModifyshowtax(string receiveDetailId)
        {
            string paramter = "";

            try
            {
                var sql = @"SELECT A.Id,A.PurchaseReturnDetailId
	                        ,A.PurchaseReturnId
	                        , A.TaxCategoryId
	                        , TC.UserName AS TaxCategory
	                        , A.HSNCodeId
	                        , HN.Code AS HSNCode
	                        , A.[Percentage]
	                        , A.TaxAmount
	                        ,d.id As PODetailId
                        FROM [TRN].[PurchaseReturnTax] AS A 
                        JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                        LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                        left join TRN.PurchaseReturnDetail d on d.id= A.PurchaseReturnDetailId
                        WHERE A.InventoryServiceId IS NULL 
						AND A.PurchaseReturnDetailId='" + receiveDetailId + "' ORDER BY TC.[Sequence]";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetReceiveTaxListGRNPurchaseReturnSaveshowtax(string receiveDetailId)
        {
            string paramter = "";

            try
            {
                var sql = @"SELECT A.Id,A.InventoryReceiveDetailId
	                        ,A.InventoryReceiveId
	                        , A.TaxCategoryId
	                        , TC.UserName AS TaxCategory
	                        , A.HSNCodeId
	                        , HN.Code AS HSNCode
	                        , A.[Percentage]
	                        , A.TaxAmount
	                        ,d.id As PODetailId
                        FROM [TRN].[InventoryReceiveTax] AS A 
                        JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                        LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                        left join TRN.InventoryReceiveDetail d on d.id= A.InventoryReceiveDetailId
                        WHERE A.InventoryServiceId IS NULL 
						AND A.InventoryReceiveDetailId='" + receiveDetailId + "' ORDER BY TC.[Sequence]";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }




        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DeletePurchaseReturnRow1(string PurchaseReturnDetailId, string inventoryReceiveDetailId, string InventoryMaterial, decimal Trasantionqty)
        {
            _inventoryDetailService.DeletePurchaseReturnRow1(PurchaseReturnDetailId, inventoryReceiveDetailId, InventoryMaterial, Trasantionqty);
            return Json(new { Message = AplosMessage.Deleted });
        }


        #endregion
        [HttpGet, Authorize]
        public JsonResult NotificationSetting()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='IssueSlip' and plantId='" + identity.PlantId + "'";
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
            return Json(_inventoryDetailService.GetCheckedByAndApprovedBY(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }


        #region notification setting Purchase Return

        [HttpGet, Authorize]
        public JsonResult NotificationSettingForPurchaserReturn()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {

                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='MaterialPurchaseReturn' and plantId='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Authorize, HttpGet]
        public JsonResult GetCheckedByAndApprovedBYForPurchaserReturn(string CheckedBy, string ApprovedBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryDetailService.GetCheckedByAndApprovedBYForPurchaserReturn(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }

        #endregion
        [Authorize, HttpPost]
        public ActionResult UpdateShortageRejectionValueMap(string InventoryReceiveId, List<Dictionary<string, object>> UserSendData)
        {
            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                obj.UpdateShortageRejectionValueMap(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }
        [Authorize, HttpPost]
        public bool GetDocRef(string UserDocRefNo, string PartyId, string DocDate, string Id)
        {
            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                bool res = obj.GetDocRef(UserDocRefNo, PartyId, DocDate, Id);
                if (res == true)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {

                return false;
            }
        }



        [Authorize, HttpGet]
        public JsonResult GetGRNDetailsForSoAllocation(string InventoryReceiveDetailId, string PODetailId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetGRNDetailsForSoAllocation(InventoryReceiveDetailId, PODetailId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet, Authorize]
        public JsonResult GetProductionRecipeMaterialList(string productionOrderId)
        {
            return Json(_inventoryDetailService.GetProductionRecipeMaterialList(productionOrderId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProcessByProductionOrder(string productionOrderId)
        {
            return Json(_inventoryDetailService.GetProcessByProductionOrder(productionOrderId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductionList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;//
            string sql = @"select top 100 * from (SELECT PO.*,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName, 
            isnull(PS.UserName,'') AS ProductionStatusName,ISNULL(so.Qty,0) AS SOQuantity           
                    FROM [TRN].[ProductionOrder] AS PO
                JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                LEFT OUTER  JOIN (SELECT pod.ProductionOrderId,SUM(so.Qty) AS Qty
                                    FROM trn.SalesOrder AS so
                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                  
                                  GROUP BY pod.ProductionOrderId

                ) AS SO ON so.ProductionOrderId=po.Id
                LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId

                            WHERE PO.PlantId='" + identity.PlantId + "' OR EN.PlantId='" + identity.PlantId + "') AS TEMP WHERE " + strkey;


            sql = @"select top 100 * from ( " + ProductionOrderList() + @"
                            WHERE PO.PlantId='" + identity.PlantId + "' OR EN.PlantId='" + identity.PlantId + "') AS TEMP WHERE " + strkey + " ORDER BY UpdatedDate DESC";


            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        private string ProductionOrderList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT 
                    case when PO.PlantId='" + identity.PlantId + @"' AND PO.PlantId=EN.PlantId then 'OWN' else 
                     case when PO.PlantId='" + identity.PlantId + @"' and EN.PlantId<>PO.PlantId then 'OUT' ELSE
                    case when PO.PlantId<>'" + identity.PlantId + @"' AND EN.PlantId='" + identity.PlantId + @"' THEN 'IN' ELSE '' END END END AS Owner,
                    PO.*,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName,                                                                    
                    isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                                FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                                                    -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                                                    sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                                                    MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                     
                                            from 
 
 
                                                     trn.SalesOrder SO 
                                                      JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                    left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                    left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                                                    group by pod.ProductionOrderId,mm.userName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId";

        }

        [HttpGet, Authorize]
        public ActionResult GetMaterialWithSKU(string processId, string parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string paramter = "";
                if (parameters != "")
                {
                    if (paramter == "")
                        paramter += "SO.Id in(" + parameters + ")";
                    else
                        paramter += " AND SO.Id in(" + parameters + ")";
                }


                var sql = @"SELECT Concat(SO.Id,'-',ISNULL(FCS.CharacteristicsValueId,''),'-',ISNULL(SCS.CharacteristicsValueId,''),'-',ISNULL(TCS.CharacteristicsValueId,'')) SOMATART
					,MOI.MaterialMasterId ,MM.UserName MaterialName ,MOI.ArticleId ,Article.StandardName ArticleName 
					,FCS.CharacteristicsValueId  FirstCharacteristicsValueId ,IsNULL(V1.UserName, '') AS FirstCharacteristicsValue
					,FC.Id FirstCharacteristicsId ,IsNULL(v2.UserName, '') AS SecondCharacteristicsValue
					,SCS.CharacteristicsValueId SecondCharacteristicsValueId ,SC.Id SecondCharacteristicsId ,IsNULL(v3.UserName, '') AS ThirdCharacteristicsValue
					,TCS.CharacteristicsValueId ThirdCharacteristicsValueId ,TC.Id ThirdCharacteristicsId ,null Active ,SO.Id SalesOrderId ,SCS.Qty OrderQty	
					,SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlanOrderQty
					,D.UserName Destination ,CPO.PONumber ,CPO.PODate,null RequisitionForQty
					,Concat(SO.Id,'-',ISNULL(FCS.CharacteristicsValueId,''),'-',ISNULL(SCS.CharacteristicsValueId,''),'-',ISNULL(TCS.CharacteristicsValueId,'')) SOFSTId
					FROM trn.SalesOrder SO 
					left JOIN trn.MasterOrderItem MOI ON  SO.MasterOrderItemId=MOI.Id					
					LEFT  JOIN MST.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
					LEFT JOIN mst.MaterialMasterArticle Article ON Article.Id=MOI.ArticleId
					left join [TRN].[FirstCharacteristics] FCS ON FCS.SalesOrderId=so.Id
					left join [TRN].[SecondCharacteristics] SCS ON SCS.FirstCharacteristicsId=FCS.Id
					Left join [TRN].[ThirdCharacteristics] TCS ON TCS.SecondCharacteristicsId=SCS.Id
					LEFT OUTER JOIN[HKP].[CharacteristicsValue] V1 ON v1.Id = FCS.CharacteristicsValueId
					LEFT OUTER JOIN[HKP].[CharacteristicsValue] V2 ON v2.Id = SCS.CharacteristicsValueId
					LEFT OUTER JOIN[HKP].[CharacteristicsValue] V3 ON v3.Id = TCS.CharacteristicsValueId
					LEFT JOIN HKP.Characteristics AS FC ON FC.Id = V1.CharacteristicsId
					LEFT JOIN HKP.Characteristics AS SC ON SC.Id = V2.CharacteristicsId
					LEFT JOIN HKP.Characteristics AS TC ON TC.Id = V3.CharacteristicsId
					LEFT JOIN [MST].[Destination] AS D ON D.Id=SO.DestinationId
					LEFT JOIN [TRN].[CustomerPO] AS CPO ON CPO.Id=SO.CustomerPOId
				     where  " + paramter + @"
					group by
					 MOI.MaterialMasterId
					,MM.UserName 
					,MOI.ArticleId
					,Article.StandardName  
					,FCS.CharacteristicsValueId  
					,IsNULL(V1.UserName, '')
					,FC.Id 
					,IsNULL(v2.UserName, '') 
					,SCS.CharacteristicsValueId
					,SC.Id 
					,IsNULL(v3.UserName, '') 
					,TCS.CharacteristicsValueId
					,TC.Id 
					,SO.Id 
					,SCS.Qty
					,D.UserName 
					,CPO.PONumber
					,CPO.PODate";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [Authorize, HttpGet]
        public JsonResult GetMaterialListForProductionReq(string Material, string Article, string Skuvalue1, string Skuvalue2, string Skuvalue3, string processId, string parameters, string SOMATART, string queryString)

        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetMaterialListForProductionReq(Material, Article, Skuvalue1, Skuvalue2, Skuvalue3, processId, parameters, SOMATART, queryString), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult GRNDocumentMapDataAll(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GRNDocumentMapDataAll(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region GRN Uncheck and Un Approved
        [HttpGet, Authorize]
        public JsonResult getGRNCheckedListData()
        {


            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.getGRNCheckedListData(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public JsonResult getGRNApprovedListData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.getGRNApprovedListData(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region GRN UncheckAnd UnApproved
        [HttpPost]
        public ActionResult GRNUncheckUpdate(string InventoryReceiveId, Dictionary<string, object> UserSendData)
        {
            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                obj.GRNUncheckUpdate(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GRNUnapprovedUpdate(string InventoryReceiveId, Dictionary<string, object> UserSendData)
        {
            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                obj.GRNUnapprovedUpdate(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion
        [Authorize, HttpGet]
        public JsonResult GetSalesOrderInfobyIssueSlipId(string IssueSlipId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryIssueService obj = new Library.MaterialManagement.InventoryManagements.InventoryIssueService();
                return Json(obj.GetSalesOrderInfobyIssueSlipId(IssueSlipId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetProductionOrderBYSalesOrder(string ProductionOrderId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryIssueService obj = new Library.MaterialManagement.InventoryManagements.InventoryIssueService();
                return Json(obj.GetProductionOrderBYSalesOrder(ProductionOrderId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetIssueWiseSKU(string IssueId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryIssueService obj = new Library.MaterialManagement.InventoryManagements.InventoryIssueService();
                return Json(obj.GetIssueWiseSKU(IssueId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region GRN-By-Outsource
        [HttpPost]
        public JsonResult CreateOSReceiptGRN(InventoryReceive entity, string entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string AcceptanceId, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, string entityMatByProduct)
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

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            //IEnumerable<InventoryMaterialViewModel>
            List<InventoryMaterialViewModel> entityMatAndImat1 = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entityMatAndImat, settings);
            List<InventoryMaterialViewModel> entityMatByProduct1 = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entityMatByProduct, settings);
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
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.IsApproved = true;
                entity.RequiredPosting = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            if (entityMatAndImat1 != null)
            {
                foreach (var item in entityMatAndImat1)
                {

                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");

                    }
                    else if (item.TransactionQty.ToString() == "0")
                    {
                        throw new CustomException("Please Input The Current Qty !");
                    }

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            if (chargesListPO != null)
            {
                foreach (var item in chargesListPO)
                {
                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");
                    }
                    else if (item.Amount.ToString() == "0")
                    {
                        throw new CustomException("Please Input  Amount !");
                    }

                }
            }
            bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            if (_returnRes == true)
            {
                throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            }
            if (entityMatAndImat1 != null)
            {
                foreach (var item in entityMatAndImat1)
                {

                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");

                    }
                    else if (item.TransactionQty.ToString() == "0")
                    {
                        throw new CustomException("Please Input The Current Qty !");
                    }

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }


            //JWDetailCreate(entity, entityMatAndImat1, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType, entityMatByProduct1);
            _inventoryDetailService.OSReceiptGRNInsertOrUpdateGraphNew(entity, entityMatAndImat1, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType, entityMatByProduct1);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }
        public JsonResult JWDetailCreate(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> entityMatByProduct)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryDetailService.OSReceiptGRNInsertOrUpdateGraphNew(entity, entityMat, taxCategoryList, id, MaterialStorageId, GRNType, entityMatByProduct);
            return Json(new { Message = AplosMessage.Success });
        }


        // Job Work Receipt

        [HttpPost]
        public JsonResult SaveJobWorkGRN(InventoryReceive entity, string entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string AcceptanceId, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, string entityMatByProduct)
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

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            //IEnumerable<InventoryMaterialViewModel>
            List<InventoryMaterialViewModel> entityMatAndImat1 = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entityMatAndImat, settings);
            List<InventoryMaterialViewModel> entityMatByProduct1 = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entityMatByProduct, settings);
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
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.IsApproved = true;
                entity.RequiredPosting = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            if (entityMatAndImat1 != null)
            {
                foreach (var item in entityMatAndImat1)
                {

                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");

                    }
                    else if (item.TransactionQty.ToString() == "0")
                    {
                        throw new CustomException("Please Input The Current Qty !");
                    }

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            if (chargesListPO != null)
            {
                foreach (var item in chargesListPO)
                {
                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");
                    }
                    else if (item.Amount.ToString() == "0")
                    {
                        throw new CustomException("Please Input  Amount !");
                    }

                }
            }
            bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            if (_returnRes == true)
            {
                throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            }
            if (entityMatAndImat1 != null)
            {
                foreach (var item in entityMatAndImat1)
                {

                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");

                    }
                    else if (item.TransactionQty.ToString() == "0")
                    {
                        throw new CustomException("Please Input The Current Qty !");
                    }

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }


            SaveJobWorkDetail(entity, entityMatAndImat1, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType, entityMatByProduct1);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }
        public JsonResult SaveJobWorkDetail(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> entityMatByProduct)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryDetailService.JobWorkInsertOrUpdateNew(entity, entityMat, taxCategoryList, id, MaterialStorageId, GRNType, entityMatByProduct);
            return Json(new { Message = AplosMessage.Success });
        }




        //[HttpPost]
        //public JsonResult UpdateJWGRN(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        //{
        //	if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
        //	{
        //		CheckedByStatusForNoti = "False";
        //		ApprovedByStatusForNoti = "False";
        //	}
        //	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //	entity.CompanyGroupId = identity.CompanyGroupId;
        //	entity.CompanyId = identity.CompanyId;
        //	entity.PlantId = identity.PlantId;

        //	if (identity.EmployeeId == entity.CheckedBy)
        //	{
        //		throw new CustomException("Please select another employee for Check by.");
        //	}
        //	else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
        //	{

        //		entity.AuthorizedBy = entity.CheckedBy;
        //		entity.AuthorizedByStatus = "For Approval";
        //		entity.CheckedBy = null;
        //		entity.CheckedByStatus = null;
        //		entity.IsApproved = false;
        //		entity.RequiredPosting = true;
        //	}
        //	else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
        //	{
        //		entity.CheckedByStatus = null;
        //		entity.AuthorizedByStatus = null;
        //		entity.CheckedBy = null;
        //		entity.AuthorizedBy = null;
        //		entity.IsApproved = true;
        //		entity.RequiredPosting = true;
        //	}
        //	else
        //	{
        //		entity.CheckedBy = entity.CheckedBy;
        //		entity.CheckedByStatus = "ForChecked";
        //		entity.AuthorizedBy = null;
        //		entity.AuthorizedByStatus = null;
        //		entity.IsApproved = false;
        //		entity.RequiredPosting = true;
        //	}
        //	if (entityMatAndImat != null)
        //	{
        //		foreach (var item in entityMatAndImat)
        //		{

        //			if (!item.check)
        //				throw new CustomException("Please Select Materials !");

        //		}
        //	}
        //	else
        //	{
        //		throw new CustomException("Please Select atlest one Materials !");
        //	}
        //	if (chargesListPO != null)
        //	{
        //		foreach (var item in chargesListPO)
        //		{
        //			if (!item.check)
        //				throw new CustomException("Please Select Materials !");

        //		}
        //	}
        //	DetailEdits(entity, entityMatAndImat, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType);
        //	ServiceChargesCreateNewEdit(chargesListPO, POServiceTaxList, entity.Id);

        //	return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        //}

        //[HttpPost]
        //public ActionResult DeleteJWGRN(string id) 
        //{
        //	if (!string.IsNullOrEmpty(id))
        //	{
        //		_inventoryReveiveService.Delete(id);
        //		return Json(new { Message = AplosMessage.Success });
        //	}
        //	else
        //		throw new CustomException(Resources.IdNotFound);
        //}
        [Authorize, HttpGet]
        public JsonResult GetJWGRNDataChecking(string GRNbyPOCheckStatus, string POId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_inventoryReveiveService.QueryGetListForMasterData(identity.PlantId, GRNbyPOCheckStatus), JsonRequestBehavior.AllowGet);
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetJWGRNDataChecking(identity.PlantId, GRNbyPOCheckStatus, POId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetJobWorkGRNDataChecking(string GRNbyPOCheckStatus, string POId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_inventoryReveiveService.QueryGetListForMasterData(identity.PlantId, GRNbyPOCheckStatus), JsonRequestBehavior.AllowGet);
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetJobWorkGRNDataChecking(identity.PlantId, GRNbyPOCheckStatus, POId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Authorize, HttpGet]
        public JsonResult GetJWApproving(string GRNbyPOApprovedStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetJWApproving(identity.PlantId, GRNbyPOApprovedStatus), JsonRequestBehavior.AllowGet);
        }

        // job work

        [Authorize, HttpGet]
        public JsonResult GetJobWorkApproving(string GRNbyPOApprovedStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetJobWorkApproving(identity.PlantId, GRNbyPOApprovedStatus), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetJWOutPutInventoryMaterialList(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.JWOutPutQuery(inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetJobWorkOutPutInventoryMaterialList(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.JobWorkOutPutQuery(inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetJWByProductInventoryMaterialList(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.JWByProductQuery(inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetJobWorkByProductInventoryMaterialList(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.JobWorkByProductQuery(inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult JWDetailDelete(string receiveDetailId)
        {
            _inventoryDetailService.JWDelete(receiveDetailId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult JWDeleteGRN(string Id)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                _inventoryReveiveService.JWDelete(Id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [Authorize, HttpGet]
        public JsonResult JWGRNDetailsData(string inveReveiveId, string POID)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
            return Json(obj.JWGRNDetailsData(inveReveiveId, POID), JsonRequestBehavior.AllowGet);

        }
        #endregion GRN-By-JW

        [Authorize, HttpGet]
        public JsonResult JobWorkGRNDetailsData(string inveReveiveId, string POID)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
            return Json(obj.JobWorkGRNDetailsData(inveReveiveId, POID), JsonRequestBehavior.AllowGet);

        }



        [Authorize, HttpGet]
        public JsonResult GetSOWiseMaterialStock(string Material, string Article, string Skuvalue1, string Skuvalue2, string Skuvalue3, string processId, string parameters, string SOMATART, string SalesOrderId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetSOWiseMaterialStock(Material, Article, Skuvalue1, Skuvalue2, Skuvalue3, processId, parameters, SOMATART, SalesOrderId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult IssueSlipDelete(string issueslipDetailId)
        {
            _inventoryDetailService.IssueSlipDelete(issueslipDetailId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult IssueSlipDeleteAll(string issueslipDetailId)
        {
            _inventoryDetailService.IssueSlipDeleteFn(issueslipDetailId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize, HttpGet]
        public JsonResult GetOutSourceReceiptDataForAllocation()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            OutsourceReceiveAllocationService outsourceReceiveAllocationService = new OutsourceReceiveAllocationService();
            //Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
            return Json(outsourceReceiveAllocationService.GetOutSourceReceiptDataForAllocation(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetOutSourceReceiptAllocatedData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            OutsourceReceiveAllocationService outsourceReceiveAllocationService = new OutsourceReceiveAllocationService();
            //Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
            return Json(outsourceReceiveAllocationService.GetOutSourceReceiptAllocatedData(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetOutSourceReceiptDetailDataForAllocation(string inventoryReceiveDetailId)
        {
            OutsourceReceiveAllocationService outsourceReceiveAllocationService = new OutsourceReceiveAllocationService();
            //Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
            return Json(outsourceReceiveAllocationService.GetOutSourceReceiptDetailDataForAllocation(inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateJWSOAllocation(IList<Dictionary<string, object>> Data)
        {
            try
            {
                DataRow dr = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsRack = null;
                if (Data == null)
                {
                    throw new Exception("Nothing to update..");
                }
                string Ids = "";
                string InventoryReceiveDetailIds = "";
                for (int i = 0; i < Data.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Data[i]["Id"].ToString()))
                    {
                        if (Ids == "")
                            Ids = "'" + Data[i]["Id"] + "'";
                        else
                            Ids += ",'" + Data[i]["Id"] + "'";
                    }
                    //if (!string.IsNullOrEmpty(Data[i]["InventoryReceiveDetailId"].ToString()))
                    //{
                    //    if (InventoryReceiveDetailIds == "")
                    //        InventoryReceiveDetailIds = "'" + Data[i]["InventoryReceiveDetailId"] + "'";
                    //    else
                    //        InventoryReceiveDetailIds += ",'" + Data[i]["InventoryReceiveDetailId"] + "'";
                    //}
                }
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                if (Ids != "")
                {
                    conRack.OpenDataSetThroughAdapter("select * from trn.GRNPORequisitionAllocation where Id in (" + Ids + ")", out dsRack, false, "1");
                }
                else
                {
                    conRack.OpenDataSetThroughAdapter("select * from trn.GRNPORequisitionAllocation where 1=2", out dsRack, false, "1");
                }

                foreach (var item in Data)
                {
                    string _Id = "";

                    #region data update

                    //DataView dv = new DataView(dsRack.Tables[0]);
                    dsRack.Tables[0].DefaultView.RowFilter = "Id='" + item["Id"] + "'";
                    if (dsRack.Tables[0].DefaultView.Count == 0)
                    {
                        if (item["SalesOrderId"] == null && item["OSPOBOQMAPId"] == null)
                        {
                            GetSOIds(item["InventoryReceiveDetailId"].ToString(), out DataSet dsDetails);
                            item["OSPOBOQMAPId"] = dsDetails.Tables[0].Rows[0]["OSPOBOQMAPId"];
                            item["SalesOrderId"] = dsDetails.Tables[0].Rows[0]["SalesOrderId"];
                        }
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("trn.GRNPORequisitionAllocation", out _Id);
                        dr = dsRack.Tables[0].NewRow();
                        _Id = "OS" + _Id;

                        dr["Id"] = _Id;
                        dr["InventoryReceiveDetailId"] = item["InventoryReceiveDetailId"];
                        dr["POBOQMapId"] = item["POBOQMapId"];
                        dr["POReqDetailsID"] = DBNull.Value;
                        dr["TransactionQty"] = item["TransactionQty"];
                        dr["TransactionUoMId"] = item["TransactionUoMId"];
                        dr["BaseQty"] = item["BaseQty"];
                        dr["BaseUoMId"] = item["BaseUoMId"];
                        dr["POBOQQty"] = item["POBOQQty"];
                        dr["POUoMId"] = item["POUoMId"];
                        dr["SalesOrderId"] = item["SalesOrderId"];
                        dr["OSPOBOQMAPId"] = item["OSPOBOQMAPId"];
                        dr["RejectQty"] = DBNull.Value;
                        dr["RejectBaseQty"] = DBNull.Value;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsRack.Tables[0].Rows.Add(dr);
                    }
                    //else
                    //{
                    //    _Id = item["Id"].ToString();
                    //    dr = dsRack.Tables[0].DefaultView[0].Row;
                    //    dr.BeginEdit();
                    //    dr["InventoryReceiveDetailId"] = item["InventoryReceiveDetailId"];
                    //    dr["POBOQMapId"] = item["POBOQMapId"];
                    //    dr["POReqDetailsID"] = item["POReqDetailsID"];
                    //    dr["TransactionQty"] = item["TransactionQty"];
                    //    dr["TransactionUoMId"] = item["TransactionUoMId"];
                    //    dr["BaseQty"] = item["BaseQty"];
                    //    dr["BaseUoMId"] = item["BaseUoMId"];
                    //    dr["POBOQQty"] = item["POBOQQty"];
                    //    dr["POUoMId"] = item["POUoMId"];
                    //    dr["SalesOrderId"] = item["SalesOrderId"];
                    //    dr["OSPOBOQMAPId "] = item["OSPOBOQMAPId "];
                    //    dr["RejectQty"] = item["RejectQty"];
                    //    dr["RejectBaseQty"] = item["RejectBaseQty"];
                    //    dr["UpdatedBy"] = identity.Name;
                    //    dr["UpdatedDate"] = DateTime.Now;
                    //    dr["UpdatedFromIP"] = identity.IPAddress;
                    //    dr.EndEdit();
                    //}
                    #endregion data update 
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRack);

                return Json(new { Error = false, Data = Data/*, Sequence = GetSequence()*/, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        public void GetSOIds(string DetailsId, out DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select OSBOQMAP.Id OSPOBOQMAPId,Boq.SalesOrderId SalesOrderId
                            from trn.InventoryReceive IR
                            Left JOIN TRN.InventoryReceiveDetail IRD on IR.Id = IRD.InventoryReceiveId
                            left join[dbo].OSPOBOQMAP OSBOQMAP ON OSBOQMAP.OSTransformationPODetailId = IRD.OSTransformationPODetailId
                            left join BOQ Boq on Boq.Id = OSBOQMAP.BOQDetailId
                            where IRD.Id = '" + DetailsId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
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

        #region GRN BOQ -- Saad

        [HttpPost, Authorize]
        public JsonResult GetItemListDetailsByList(string MaterialIds, string ArticleIds, string VendorRefNos, string CustomerRefNos, string OwnReferenceNo, string PartyId)
        {
            BOQQueryService bOQQueryService = new BOQQueryService(_sqlRepository);
            return Json(bOQQueryService.GetItemListDetailsByList(MaterialIds, ArticleIds, VendorRefNos, CustomerRefNos, OwnReferenceNo, PartyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedItemListDetailsByList(string POId, string ContractId, string masterOrderitemId, string SalesOrderId, string MaterialMasterId, string ArticleId)
        {

            BOQQueryService bOQQueryService = new BOQQueryService(_sqlRepository);
            return Json(bOQQueryService.GetSelectedItemListDetailsByList(POId, ContractId, masterOrderitemId, SalesOrderId, MaterialMasterId, ArticleId), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public JsonResult GetPOBOQItemForGRN(string POId, string ContractId, string masterOrderitemId, string SalesOrderId, string MaterialMasterId, string ArticleId)
        {
            BOQQueryService bOQQueryService = new BOQQueryService(_sqlRepository);
            return Json(bOQQueryService.GetPOBOQItemForGRN(POId, ContractId, masterOrderitemId, SalesOrderId, MaterialMasterId, ArticleId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreatePOGRNBOQ(InventoryReceive entity, string entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string gRNType, string AcceptanceId, string CheckedByStatusForNoti, string ApprovedByStatusForNoti,string BOQAllocation)
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
            gRNType = GRNType.GRNBYBOQ.ToString();
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            //IEnumerable<InventoryMaterialViewModel>
            List<InventoryMaterialViewModel> entityMatAndImat1 = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entityMatAndImat, settings);
            IEnumerable<InventoryMaterialViewModel> BOQAllocationSave = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(BOQAllocation, settings);
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
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
            {
                entity.CheckedByStatus = null;
                entity.AuthorizedByStatus = null;
                entity.CheckedBy = null;
                entity.AuthorizedBy = null;
                entity.IsApproved = true;
                entity.RequiredPosting = true;
            }
            else
            {
                entity.CheckedBy = entity.CheckedBy;
                entity.CheckedByStatus = "ForChecked";
                entity.AuthorizedBy = null;
                entity.AuthorizedByStatus = null;
                entity.IsApproved = false;
                entity.RequiredPosting = true;
            }
            if (entityMatAndImat1.Count>0)
            {
                foreach (var item in entityMatAndImat1)
                {

                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");

                    }
                    else if (item.TransactionQty.ToString() == "0")
                    {
                        throw new CustomException("Please Input The Current Qty !");
                    }

                }
            }
            else
            {
                throw new CustomException("Please Select atlest one Materials !");
            }
            if (chargesListPO != null)
            {
                foreach (var item in chargesListPO)
                {
                    if (!item.check)
                    {
                        throw new CustomException("Please Select Materials !");
                    }
                    else if (item.Amount.ToString() == "0")
                    {
                        throw new CustomException("Please Input  Amount !");
                    }

                }
            }
            if (string.IsNullOrEmpty(entity.Id))
            {
                bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
                if (_returnRes == true)
                {
                    throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
                }
            }


            DetailCreateGRNBOQ(entity, entityMatAndImat1, receiveTaxList, entity.Id, entity.MaterialStorageId, gRNType, BOQAllocationSave);
            ServiceChargesCreateNewSaad(chargesListPO, POServiceTaxList, entity.Id, AcceptanceId);
            //_gRNPORequisitionAllocationService.InsertOrUpdateGraphNewGRNAllocationBOQ(BOQAllocationSave);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }
        public JsonResult DetailCreateGRNBOQ(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string gRNType, IEnumerable<InventoryMaterialViewModel> BOQAllocationSave)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           
            _inventoryDetailService.InsertOrUpdateGraphNewGRNBOQ(entity, entityMat, taxCategoryList, id, MaterialStorageId, gRNType, BOQAllocationSave);
            return Json(new { Message = AplosMessage.Success });
        }
        public JsonResult ServiceChargesCreateNewSaad(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string Id, string AcceptanceId)
        {
            _inventoryService.InsertGraphNewBOQ(chargesListPO, POServiceTaxList, Id, AcceptanceId);
            return Json(new { Message = AplosMessage.Success });
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListBOQ(GridParameter parameters, string inveReveiveId, string POID, string AcceptanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.QueryBOQ(parameters, inveReveiveId, POID, AcceptanceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetReceiveTaxListBOQ(string receiveDetailId)
        {
            return Json(_inventoryReveiveService.GetReceiveTaxListBOQ(receiveDetailId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceChargeListBOQ(string receiveId)
        {
            return Json(_inventoryService.QueryBOQ(receiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetServiceTaxListBOQ(string serviceId)
        {
            return Json(_inventoryReveiveService.GetServiceTaxListBOQ(serviceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetAdvanceTaxInfoBOQ(string InventoryReceiveId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetAdvanceTaxInfoBOQ(InventoryReceiveId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult GRNDocumentMapDataBOQ(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();
                return Json(obj.GRNDocumentMapDataBOQ(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetCheckedByAndApprovedBYBOQ(string CheckedBy, string ApprovedBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetCheckedByAndApprovedBYBOQ(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult LoadAcceptanceDetailsBOQ(string AcceptanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.LoadAcceptanceDetailsBOQ(AcceptanceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialListByOnlyPOBOQ(GridParameter parameters, string inveReveiveId, string AcceptanceId)
        {
            BOQQueryService bOQQueryService = new BOQQueryService(_sqlRepository);
            return Json(bOQQueryService.QueryOnlyPOBOQ(parameters, inveReveiveId, AcceptanceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceChargeListPOBOQ(string receiveId, string AcceptanceId)
        {
            return Json(_inventoryService.Query1BOQ(receiveId, AcceptanceId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetSavedPOListBOQ(string GRNId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetSavedPOListBOQ(GRNId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetGRNDetailsForSoAllocationBOQ(string InventoryReceiveDetailId, string PODetailId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetGRNDetailsForSoAllocationBOQ(InventoryReceiveDetailId, PODetailId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult GrnRequisitionAllocationSaveBOQ(IEnumerable<InventoryMaterialViewModel> entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _gRNPORequisitionAllocationService.InsertOrUpdateGraphNewGRNAllocationBOQ(entity);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult GRNBOQDetailDelete(string receiveId, string receiveDetailId)
        {
            _inventoryDetailService.GRNBOQDetailDelete(receiveId,receiveDetailId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion

        #region BinWiseGReport -- Nitesh
        public ActionResult BinWiseGRN(string grnId, out DataTable data)
        {
            try
            {
                var sql = @"SELECT 
ROW_NUMBER() OVER(ORDER BY MT.Id) SrNo
,MT.UserName MaterialType    
,MM.UserName Material
,MMA.StandardName Article 
,HSNC.Code HSNNo
,TUoM.UserName UOM
,IsAsset = CASE WHEN IRD.IsAsset = 0 then 'Revenue' ELSE 'Asset' END
,ISNULL(IR.Id,0) grnNumber
,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS GRNDate  
,MS.UserName StorageLocation
,SBM.UserName BinLocation
,SBM.BinCode BinNumber
,CEILING(IRD.TransactionQty)  QTY
,TaxAmount = (
                            SELECT CEILING(SUM(TaxAmount))
                            FROM [TRN].[InventoryReceiveTax]
                            WHERE InventoryReceiveDetailId = IRD.Id
                            )
                             FROM trn.inventoryReceiveDetail IRD 
                             LEFT JOIN [TRN].[GRNBinAllocationMap] GAM ON IRD.Id = GAM.InventoryReceiveDetailId
							LEFT JOIN MST.StorageBinMaster SBM on SBM.Id = GAM.StorageBinMasterId
							LEFT JOIN TRN.BinAllocation BA on BA.StorageBinMasterId = SBM.Id
							LEFT JOIN TRN.InventoryReceive IR ON ir.Id = IRD.InventoryReceiveId
                            LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                            LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                            LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId                       
                            LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId
                            LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                             LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
							LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                            LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId                            
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id                           														
							LEFT JOIN HKP.MaterialType MT on MT.Id = MGM.MaterialTypeId
							LEFT JOIN [HKP].[MaterialStorage] MS on MS.Id = IR.MaterialStorageId                          
                            WHERE IR.Id ='" + grnId+"' and IOM.MaterialMasterId is not NULL";
                data = _sqlRepository.GetDataTable(sql);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        

        [Authorize, HttpPost]
        public ActionResult XlsBinWiseGRNReport(string grnId)
        {
            try
            {

                string fileName = "";
                fileName = BinWiseGRNExcel(grnId,"BinWiseGRNReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string BinWiseGRNExcel(string grnId, string SheetName)
        {
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
                workbook.Worksheets[0].Name = "Bin Wise GRN";
                sheet = workbook.Worksheets[0];
                DataTable data;
                BinWiseGRN(grnId, out data);

                sheet[4, 3].Text = "GRN Number : " + data.Rows[0]["grnNumber"];
                sheet[4, 3].ColumnWidth = 16;

                sheet[4, 5].Text = "GRN Date : " + data.Rows[0]["GRNDate"].ToString();
                sheet[4, 5].ColumnWidth = 16;

                int ROW = 6; int COL = 1;

                #region Columns
                sheet[ROW, COL].Text = "SR No";
                sheet[ROW, COL].ColumnWidth = 8;
                int ColSRNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMaterialType = COL;
                COL++;

                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMaterial = COL;
                COL++;

                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "HSN No";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColHSNNo = COL;
                COL++;

              

                

                sheet[ROW, COL].Text = "Revenue / Asset";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColGRNType= COL;
                COL++;


                sheet[ROW, COL].Text = "Storage Location";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColStorageLoc = COL;
                COL++;

                sheet[ROW, COL].Text = "Bin Location";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBinLocation = COL;
                COL++;

                sheet[ROW, COL].Text = "Bin No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBinNo = COL;
                COL++;

                

                sheet[ROW, COL].Text = "QTY";
                sheet[ROW, COL].ColumnWidth = 8;
                int ColQTY = COL;
                COL++;

                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColUOM = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColAmount = COL;
                //COL++;

                //COL++;
                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColSRNo].Number = clsStaticInfo.dbl(data.Rows[i]["SrNo"].ToString());
                    sheet[ROW, ColMaterialType].Text = data.Rows[i]["MaterialType"].ToString();
                    sheet[ROW, ColMaterial].Text = data.Rows[i]["Material"].ToString();
                    sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
                    sheet[ROW, ColHSNNo].Number = clsStaticInfo.dbl(data.Rows[i]["HSNNo"].ToString());
                    sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                    sheet[ROW, ColGRNType].Text = data.Rows[i]["IsAsset"].ToString();
                    //sheet[ROW, ColGRNNo].Number = clsStaticInfo.dbl(data.Rows[i]["grnNumber"].ToString());
                    //sheet[ROW, ColGRNDate].Text = data.Rows[i]["GRNDate"].ToString();
                    sheet[ROW, ColStorageLoc].Text = data.Rows[i]["StorageLocation"].ToString();
                    sheet[ROW, ColBinLocation].Text = data.Rows[i]["BinLocation"].ToString();
                    sheet[ROW, ColBinNo].Number = clsStaticInfo.dbl(data.Rows[i]["BinNumber"].ToString());
                    sheet[ROW, ColQTY].Number = clsStaticInfo.dbl(data.Rows[i]["QTY"].ToString());
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TaxAmount"].ToString());

                   

                    ROW++;
                }

               
                
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Bin Wise GRN", identity.PlantId);
                
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
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


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public ActionResult AllBinGRN(string from, string to, string materialtype, out DataTable data)
        {
            try
            {
                string MaterialType = "'" + materialtype.Replace(",", "','") + "'";//replaced with ""
                var sql = @"SELECT 
ROW_NUMBER() OVER(ORDER BY MT.Id) SrNo
,MT.UserName MaterialType    
,MM.UserName Material
,MMA.StandardName Article 
,HSNC.Code HSNNo
,TUoM.UserName UOM
,IsAsset = CASE WHEN IRD.IsAsset = 0 then 'Revenue' ELSE 'Asset' END
,ISNULL(IR.Id,0) grnNumber
,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS GRNDate  
,MS.UserName StorageLocation
,SBM.UserName BinLocation
,SBM.BinCode BinNumber
,CEILING(IRD.TransactionQty)  QTY
,TaxAmount = (
                            SELECT CEILING(SUM(TaxAmount))
                            FROM [TRN].[InventoryReceiveTax]
                            WHERE InventoryReceiveDetailId = IRD.Id
                            )
                            FROM  [TRN].[GRNBinAllocationMap] GAM 
							LEFT JOIN MST.StorageBinMaster SBM on SBM.Id = GAM.StorageBinMasterId
							LEFT JOIN TRN.BinAllocation BA on BA.StorageBinMasterId = SBM.Id
                            LEFT JOIN trn.inventoryReceiveDetail IRD ON IRD.Id = GAM.InventoryReceiveDetailId
							LEFT JOIN TRN.InventoryReceive IR ON ir.Id = IRD.InventoryReceiveId
                            LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                            LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                            LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId                       
                            LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId
                            LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId      
                             LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = MMA.MaterialMasterId
							LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId                      
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id                           														
							LEFT JOIN HKP.MaterialType MT on MT.Id = MGM.MaterialTypeId
							LEFT JOIN [HKP].[MaterialStorage] MS on MS.Id = IR.MaterialStorageId                          
                            WHERE IR.GRNDate between '" + from+"' and '"+to+ "' and MT.Id in ("+ MaterialType + ") and IOM.MaterialMasterId is not NULL";
                data = _sqlRepository.GetDataTable(sql);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [Authorize, HttpPost]
        public ActionResult XlsAllBinWiseGRNReport(string from, string to,string materialtype)
        {
            try
            {

                string fileName = "";
                fileName = AllBinWiseGRNExcel(from, to, materialtype, "AllBinWiseGRNReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string AllBinWiseGRNExcel(string from, string to, string materialtype, string SheetName)
        {
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
                workbook.Worksheets[0].Name = "All Bin Wise GRN";
                sheet = workbook.Worksheets[0];
                DataTable data;
                AllBinGRN(from, to, materialtype, out data);

                
                int ROW = 6; int COL = 1;

                #region Columns
                sheet[ROW, COL].Text = "SR No";
                sheet[ROW, COL].ColumnWidth = 8;
                int ColSRNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMaterialType = COL;
                COL++;

                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMaterial = COL;
                COL++;

                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "HSN No";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColHSNNo = COL;
                COL++;

               



                sheet[ROW, COL].Text = "Revenue / Asset";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColGRNType = COL;
                COL++;

                sheet[ROW, COL].Text = "GRN No";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGRNNo = COL;
                COL++;

                sheet[ROW, COL].Text = "GRN Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGRNDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Storage Location";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColStorageLoc = COL;
                COL++;

                sheet[ROW, COL].Text = "Bin Location";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBinLocation = COL;
                COL++;

                sheet[ROW, COL].Text = "Bin No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBinNo = COL;
                COL++;



                sheet[ROW, COL].Text = "QTY";
                sheet[ROW, COL].ColumnWidth = 8;
                int ColQTY = COL;
                COL++;

                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColUOM = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColAmount = COL;
                //COL++;

                //COL++;
                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColSRNo].Number = clsStaticInfo.dbl(data.Rows[i]["SrNo"].ToString());
                    sheet[ROW, ColMaterialType].Text = data.Rows[i]["MaterialType"].ToString();
                    sheet[ROW, ColMaterial].Text = data.Rows[i]["Material"].ToString();
                    sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
                    sheet[ROW, ColHSNNo].Number = clsStaticInfo.dbl(data.Rows[i]["HSNNo"].ToString());
                    sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                    sheet[ROW, ColGRNType].Text = data.Rows[i]["IsAsset"].ToString();
                    sheet[ROW, ColGRNNo].Number = clsStaticInfo.dbl(data.Rows[i]["grnNumber"].ToString());
                    sheet[ROW, ColGRNDate].DateTime = Convert.ToDateTime(data.Rows[i]["GRNDate"].ToString());
                    sheet[ROW, ColStorageLoc].Text = data.Rows[i]["StorageLocation"].ToString();
                    sheet[ROW, ColBinLocation].Text = data.Rows[i]["BinLocation"].ToString();
                    sheet[ROW, ColBinNo].Number = clsStaticInfo.dbl(data.Rows[i]["BinNumber"].ToString());
                    sheet[ROW, ColQTY].Number = clsStaticInfo.dbl(data.Rows[i]["QTY"].ToString());
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TaxAmount"].ToString());



                    ROW++;
                }



                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "All Bin Wise GRN", identity.PlantId);

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
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


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public ActionResult GetMaterialType()
        {
            try
            {
                var sql = @"select Id Value, StandardName Text from HKP.MaterialType";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion BinWiseGReport -- Nitesh

        [Authorize, HttpGet]
        public ActionResult GetWorkCenterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT WCM.Id AS WorkCenterMasterId,e.UserName AS Entity,p.UserName AS Plant
	                             , WCM.EntityId, WCM.Code, WCM.UserName
                            FROM SCS.WorkCenterMaster AS WCM
                            INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                            WHERE WCM.PlantId='" + identity.PlantId + "' order by p.userName, e.UserName,WCM.sequence";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetGRNAdditionalInfoData(string grnId)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            return Json(inventoryReceiveQueryService.GetGRNAdditionalInfoData(grnId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateGRNAdditionalInfo(List<Dictionary<string, object>> data, string grnId)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[SalesAdditionalInfo] where  InventoryReceiveId='" + grnId + "'", out dsChild, false, "1");
                int count = 0;
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            count++;
                            item["Id"] = grnId + "-" + count;
                            item["InventoryReceiveId"] = grnId;
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



    }
}
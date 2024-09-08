using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Aplos.MaterialManagement;
using Newtonsoft.Json;
using System.Linq;

namespace Aplos.Areas.Products.Controllers
{
    public class InventoryReceiveController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IInventoryReceiveService _inventoryReveiveService;
        private readonly IInventoryReceiveDetailService _inventoryDetailService;
        private readonly IInventoryMaterialService _inventoryMaterialService;
        private readonly IInventoryServiceService _inventoryService;
        private readonly IInventoryReceiveReportService _inventoryReportService;

        public InventoryReceiveController(IInventoryReceiveService inventoryReveiveService
            , IInventoryReceiveDetailService inventoryDetailService
            , IInventoryMaterialService inventoryMaterialService
            , IInventoryReceiveReportService inventoryReportService
            , ISqlRepository sqlRepository
            , IInventoryServiceService inventoryService)
        {
            _sqlRepository = sqlRepository;
            _inventoryReveiveService = inventoryReveiveService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryService = inventoryService;
            _inventoryReportService = inventoryReportService;
        }

        #endregion Constructor

        #region Aplos

        //public ActionResult Aplos()
        //{
        //	return View();
        //}

        public ActionResult VendorGRN()
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


        public ActionResult EmployeePurchaseGRN()
        {
            return View();
        }
        [Authorize]
        public ActionResult EmployeePurchase()
        {
            return View();
        }

        public ActionResult FOC()
        {
            return View();
        }
       
        #endregion Aplos



        #region Employee-GRN
        [HttpPost]
        public JsonResult CreateEMPGRN(InventoryReceive entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
            entity.IsApproved = false;
            if (entity.EmployeeId == null || entity.EmployeeId == "")
            {
                entity.GRNType = "GRN";
            }
            else
            {
                entity.GRNType = "EMPGRN";
            }
            if (entity.IsNonVendor == true)
            {
                entity.IsNonVendor = true;
            }
            else
            {
                entity.IsNonVendor = false;
            }
            if (entity.IsNonVendor == true)
            {
                if (entity.Reason == null || entity.Reason == "")
                {
                    throw new CustomException("Enter Reason!");
                }
            }
            else
            {
                if (entity.PartyId == null || entity.PartyId == "")
                {
                    throw new CustomException("Please select vendor!");
                }
            }
            if (entity.AlongwithInvoice == true)
            {
                if (entity.DocRefNo == null || entity.DocRefNo == "")
                {
                    throw new CustomException("Please Enter the RefNo!");
                }
                if (entity.DocDate == null)
                {
                    throw new CustomException("Please Enter the DocDate!");
                }
            }
            bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            if (_returnRes == true)
            {
                throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            }
            _inventoryReveiveService.Insert(entity);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public ActionResult DeleteEMPGRN(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _inventoryReveiveService.Delete(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailCreate(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList, IEnumerable<GRNBinAllocationMap> gRNBinAllocationMapList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            entity.ShortRejFlag = false;
            if (entity != null)
            {
                if (entity.TransactionUoMId == null || entity.TransactionUoMId == "")
                {
                    throw new CustomException("Please select UOM!");
                }
                else if (entity.MaterialMasterId == null || entity.MaterialMasterId == "")
                {
                    if (entity.Description == null || entity.Description == "")
                    {
                        throw new CustomException("Enter The Material Description!");
                    }


                }
            }
            _inventoryDetailService.InsertOrUpdateGraph(entity, taxCategoryList, gRNBinAllocationMapList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DetailDelete(string receiveDetailId)
        {
            _inventoryDetailService.Delete(receiveDetailId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion Employee-GRN

        #region GRN-without-PO --Vendor GRN

        [HttpPost]
        public JsonResult Create(InventoryReceive entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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


            if (entity.EmployeeId == null || entity.EmployeeId == "")
            {
                entity.GRNType = "GRN";

            }
            else
            {
                entity.GRNType = "EMPGRN";
            }
            if (entity.IsNonVendor == true)
            {

                entity.IsNonVendor = true;
            }
            else
            {
                entity.IsNonVendor = false;
            }
            if (entity.IsNonVendor == true)
            {
                if (entity.Reason == null || entity.Reason == "")
                {
                    throw new CustomException("Enter Reason!");
                }
            }
            else
            {
                if (entity.PartyId == null || entity.PartyId == "")
                {
                    throw new CustomException("Please select vendor!");
                }


            }
            if (entity.AlongwithInvoice == true)
            {
                if (entity.DocRefNo == null || entity.DocRefNo == "")
                {
                    throw new CustomException("Please Enter the RefNo!");
                }
                if (entity.DocDate == null)
                {
                    throw new CustomException("Please Enter the DocDate!");
                }
            }
            bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            if (_returnRes == true)
            {
                throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            }
            _inventoryReveiveService.Insert(entity);
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
        #endregion GRN-without-PO --Vendor GRN
        #region GRN FOC
        [Authorize, HttpGet]
        public JsonResult GetListFOCGRN(string status)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            return Json(inventoryReceiveQueryService.GetListFOCGRN(status), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GRNBYFOC(InventoryReceive entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
            entity.IsFOC = true;

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


            if (entity.EmployeeId == null || entity.EmployeeId == "")
            {
                entity.GRNType = "GRN";

            }
            else
            {
                entity.GRNType = "EMPGRN";
            }
            if (entity.IsNonVendor == true)
            {

                entity.IsNonVendor = true;
            }
            else
            {
                entity.IsNonVendor = false;
            }
            if (entity.IsNonVendor == true)
            {
                if (entity.Reason == null || entity.Reason == "")
                {
                    throw new CustomException("Enter Reason!");
                }
            }
            else
            {
                if (entity.PartyId == null || entity.PartyId == "")
                {
                    throw new CustomException("Please select vendor!");
                }


            }
            if (entity.AlongwithInvoice == true)
            {
                if (entity.DocRefNo == null || entity.DocRefNo == "")
                {
                    throw new CustomException("Please Enter the RefNo!");
                }
                if (entity.DocDate == null)
                {
                    throw new CustomException("Please Enter the DocDate!");
                }
            }
            bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            if (_returnRes == true)
            {
                throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            }
            _inventoryReveiveService.Insert(entity);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public JsonResult CreateGRNBYFOC(InventoryReceive entity, string entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string AcceptanceId, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
            //bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            //if (_returnRes == true)
            //{
            //	throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            //}

            DetailFOCCreate(entity, entityMatAndImat1, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType, entityMatAndImat);
            ServiceChargesFOCCreate(chargesListPO, POServiceTaxList, entity.Id, AcceptanceId);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailFOCCreate(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType,string entityMatAndImat)
        {
            #region Duplicate Remove
            List<InventoryMaterialViewModel> List = new List<InventoryMaterialViewModel>();
            
            decimal addnew = 0;
            foreach (var item in entityMat)
            {
                addnew = 0;
                if (List.Count == 0)
                {
                    List.Add(item);

                }
                else
                {
                    for (int i = 0; i < List.Count; i++)
                    {
                        if (List[i].MaterialMasterId == item.MaterialMasterId && List[i].ArticleId == item.ArticleId && List[i].FirstCharacteristicsValueId == item.FirstCharacteristicsValueId && List[i].SecondCharacteristicsValueId == item.SecondCharacteristicsValueId)
                        {
                            if (item.NetQty != List[i].NetQty)
                            {
                                List[i].Qty = List[i].Qty + item.Qty;
                                List[i].BaseIssueQty = List[i].BaseIssueQty + item.BaseIssueQty;
                                List[i].BaseQty = List[i].BaseQty + item.BaseQty;
                                List[i].NetQty = List[i].NetQty + item.NetQty;
                                List[i].TransactionQty = List[i].TransactionQty + item.TransactionQty;
                                List[i].GRNTotalAmount = List[i].GRNTotalAmount + item.GRNTotalAmount;
                                addnew = 1;
                            }
                            else
                            {
                                addnew = 1;
                            }

                        }
                    }
                    if (addnew == 0)
                    {
                        List.Add(item);
                    }
                }
            }

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<InventoryMaterialViewModel> entityMatAndImat1 = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entityMatAndImat, settings);

            #endregion


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryDetailService.InsertFOCDetail(entity, entityMatAndImat1, taxCategoryList, id, MaterialStorageId, GRNType, List);
            return Json(new { Message = AplosMessage.Success });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesFOCCreate(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string Id, string AcceptanceId)
        {
            _inventoryService.InsertGraphNew(chargesListPO, POServiceTaxList, Id, AcceptanceId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult UpdateGRNBYFOC(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
            DetailFOCEdits(entity, entityMatAndImat, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType);
            ServiceChargesCreateNewEdit(chargesListPO, POServiceTaxList, entity.Id);

            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult DetailFOCEdits(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _inventoryDetailService.UpdateFOCDetail(entity, entityMatAndImat, taxCategoryList, id, MaterialStorageId, GRNType);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult FOCMaterialInsert(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            entity.ShortRejFlag = false;
            entity.BaseRate = 0;
            entity.TotalMaterialBooksCurrencyAmount = 0;
            entity.TrnCurrencyBaseRate = 0;
            if (entity != null)
            {
                if (entity.TransactionUoMId == null || entity.TransactionUoMId == "")
                {
                    throw new CustomException("Please select UOM!");
                }
                else if (entity.MaterialMasterId == null || entity.MaterialMasterId == "")
                {
                    if (entity.Description == null || entity.Description == "")
                    {
                        throw new CustomException("Enter The Material Description!");
                    }


                }
            }
            _inventoryDetailService.InsertFOCMaterial(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreateNewEdit(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string Id)
        {
            _inventoryService.InsertGraphNewEdit(chargesListPO, POServiceTaxList, Id);
            return Json(new { Message = AplosMessage.Success });
        }
        #endregion
        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.Query(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #region Only GRN Index Page All Tab

        [Authorize, HttpGet]
        public JsonResult GetListGRN(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            return Json(inventoryReceiveQueryService.GetListGRN(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult CheckedHoldReject(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            return Json(inventoryReceiveQueryService.CheckedHoldReject(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult NotApproveChecked(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            return Json(inventoryReceiveQueryService.NotApproveChecked(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult ApprovedHoldChecked(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            return Json(inventoryReceiveQueryService.ApprovedHoldChecked(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult ApprovedNotPost(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            return Json(inventoryReceiveQueryService.ApprovedNotPost(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult Posted(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var res = _inventoryReveiveService.Posted();
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_gateEntryService.GetAllReqdata(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);

            //return Json(_inventoryReveiveService.Posted(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        #endregion

        public JsonResult GetListEmpGrn(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.QueryEmpGrn(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #region Employee GRN Index Page All Tab
        [Authorize, HttpGet]
        public JsonResult GetListEmployeePurchase(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            return Json(inventoryReceiveQueryService.GetListEmployeePurchase(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetListEmpCheckedHoldReject(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            return Json(inventoryReceiveQueryService.GetListEmpCheckedHoldReject(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetListEmpNotApproveChecked(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            return Json(inventoryReceiveQueryService.GetListEmpNotApproveChecked(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListEmpApprovedHoldReject(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);

            return Json(inventoryReceiveQueryService.GetListEmpApprovedHoldReject(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListEmpApprovedNotPost(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_inventoryReveiveService.GetListEmpApprovedNotPost(), JsonRequestBehavior.AllowGet);
            try
            {

                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetListEmpApprovedNotPost(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [Authorize, HttpGet]
        public JsonResult GetListEmpPosted(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_inventoryReveiveService.GetListEmpPosted(), JsonRequestBehavior.AllowGet);
            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                var res = obj.GetListEmpPosted();
                var jsondata = Json(res, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion



        [Authorize, HttpPost]
        public JsonResult Edit(InventoryReceive entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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
            //entity.IsApproved = false;
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

            if (entity.EmployeeId == null || entity.EmployeeId == "")
            {
                entity.GRNType = "GRN";

            }
            else
            {
                entity.GRNType = "EMPGRN";
            }
            if (entity.IsNonVendor == true)
            {

                entity.IsNonVendor = true;
            }
            else
            {
                entity.IsNonVendor = false;
            }
            if (entity.IsNonVendor == true)
            {
                if (entity.Reason == null || entity.Reason == "")
                {
                    throw new CustomException("Enter Reason!");
                }
            }
            //bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            //if (_returnRes == true)
            //{
            //	throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            //}
            _inventoryReveiveService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }
        [Authorize, HttpPost]
        public JsonResult EditEMPGRN(InventoryReceive entity, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            if (string.IsNullOrEmpty(CheckedByStatusForNoti) && string.IsNullOrEmpty(ApprovedByStatusForNoti))
            {
                CheckedByStatusForNoti = "False";
                ApprovedByStatusForNoti = "False";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
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
            if (entity.EmployeeId == null || entity.EmployeeId == "")
            {
                entity.GRNType = "GRN";

            }
            else
            {
                entity.GRNType = "EMPGRN";
            }
            if (entity.IsNonVendor == true)
            {

                entity.IsNonVendor = true;
            }
            else
            {
                entity.IsNonVendor = false;
            }
            if (entity.IsNonVendor == true)
            {
                if (entity.Reason == null || entity.Reason == "")
                {
                    throw new CustomException("Enter Reason!");
                }
            }
            bool _returnRes = GetDocRef(entity.DocRefNo, entity.PartyId, entity.DocDate.ToString(), entity.Id);
            if (_returnRes == true)
            {
                throw new CustomException("Vendor / Docref / Docdate cannot duplicate!");
            }
            _inventoryReveiveService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }


        [Authorize, HttpGet]
        public JsonResult GetListOfPOGateEntryEmployee(string EmployeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListOfPOGateEntryEmployee(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmployeeId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetListForInvShortagePayable()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForInvShortagePayable(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetListForInvRejectPayable()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetListForInvRejectPayable(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations

        #region Inventory Detail

        [Authorize, HttpGet]
        public JsonResult GetToCurrencyRate(string currencyId, string baseCurrencyId, string docDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetToCurrencyRate(currencyId, baseCurrencyId, Convert.ToDateTime(docDate), identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetToCurrencyRateForJWR(string currencyId, string baseCurrencyId, string docDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetToCurrencyRate(currencyId, baseCurrencyId, Convert.ToDateTime(docDate), identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult UpdareGRN(IEnumerable<InventoryMaterialViewModel> entityMatAndImat, string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryDetailService.InsertOrUpdateGraphNewEditsOnlyGRN(entityMatAndImat, Id);

            return Json(new { Message = AplosMessage.Updated });
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
        public JsonResult GetReceiveTaxList(string receiveDetailId)
        {
            return Json(_inventoryReveiveService.GetReceiveTaxList(receiveDetailId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            entity.CompanyId = identity.CompanyId;
            entity.PlantId = identity.PlantId;
            _inventoryDetailService.InsertExtraTax(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
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
        public JsonResult GetInventoryMaterialListwithoutpo(GridParameter parameters, string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.Querywithoutpo(parameters, inveReveiveId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialList(GridParameter parameters, string inveReveiveId, string POID, string AcceptanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.Query(parameters, inveReveiveId, POID, AcceptanceId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialPayable(string inveReveiveId, string employeeId, bool isReversCharge, string foc)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            if (foc == "NO")
            {
                if (!string.IsNullOrEmpty(employeeId) && employeeId != "null")
                    return Json(_accountsInventoryPayableService.GetInventoryMaterialForImprestPayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
                else
                {
                    if (isReversCharge)
                        return Json(_accountsInventoryPayableService.GetInventoryMaterialReversChargePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
                    else
                        return Json(_accountsInventoryPayableService.GetInventoryMaterialWithoutReversChargePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                AccountsInvoiceService _accountsInvoiceService = new AccountsInvoiceService(_sqlRepository);
                return Json(_accountsInvoiceService.GetInventoryPayableFOC(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);

            }
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryTaxList(string inveReveiveId)
        {
            return Json(_inventoryMaterialService.GetInventoryTaxList(inveReveiveId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetVendorPayableGLBudgetActivity(string inveReveiveId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryMaterialService.GetVendorPayableGLBudgetActivity(inveReveiveId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetOtherVendorChargesPayable(string inveReveiveId,string otherPartyId,bool rcmApplicable)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsInventoryPayableService _accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            return Json(_accountsInventoryPayableService.GetOtherVendorChargesPayableData(identity.CompanyId,  identity.PlantId, inveReveiveId, otherPartyId, rcmApplicable), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialShortagePayable(string inveReveiveId, string employeeId, bool isReversCharge)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(employeeId) && employeeId != "null")
                return Json(accountsInventoryPayableService.GetInventoryMaterialShortagePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
            else
            {
                if (isReversCharge)
                    return Json(accountsInventoryPayableService.GetInventoryMaterialShortagePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
                else
                    return Json(accountsInventoryPayableService.GetInventoryMaterialShortagePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryMaterialRejectPayable(string inveReveiveId, string employeeId, bool isReversCharge)
        {
            AccountsInventoryPayableService accountsInventoryPayableService = new AccountsInventoryPayableService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (!string.IsNullOrEmpty(employeeId) && employeeId != "null")
                return Json(accountsInventoryPayableService.GetInventoryMaterialShortagePayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
            else
            {
                if (isReversCharge)
                    return Json(accountsInventoryPayableService.GetInventoryMaterialRejectPayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
                else
                    return Json(accountsInventoryPayableService.GetInventoryMaterialRejectPayable(identity.CompanyId, identity.PlantId, inveReveiveId), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Inventory Material

        #region Service Charges

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult ServiceChargesCreate(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            _inventoryService.InsertGraph(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult ServiceChargesUpdate(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            _inventoryService.InsertGraphUpdate(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }
        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult ServiceChargesDelete(string serviceId)
        {
            _inventoryService.Delete(serviceId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult OtherVendorServiceChargesCreate(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            if(string.IsNullOrEmpty(entity.OtherPartyDocRefNo)) throw new CustomException("Please Input Other Party DocRefNo !!");
            if (entity.OtherPartyRCMApplicable)
            {
                foreach (var item in taxCategoryList)
                {
                    if (item.TaxAmount == 0)
                    {
                        item.TaxAmount = Math.Round( Convert.ToDecimal(entity.TransactionAmount * 5 / 100),2);
                    }
                }
            }
            _inventoryService.OtherVendorInsertGraph(entity, taxCategoryList);
            return Json(new { entity.Id, Message = AplosMessage.Success });
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceChargeList(string receiveId)
        {
            return Json(_inventoryService.Query(receiveId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceOtherVendorChargeList(string receiveId)
        {
            return Json(_inventoryService.OtherVendorChargesQuery(receiveId), JsonRequestBehavior.AllowGet);
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
        [Authorize, HttpGet]
        public JsonResult GetEmployeePurchaseList(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetEmployeePurchaseList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #endregion Employee Purchase

        #region GRN Approved

        [Authorize, HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult Approved(IEnumerable<InventoryReceive> entities, string GRNStatus)
        {
            _inventoryReveiveService.GRNApproved(entities, GRNStatus);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion GRN Approved

        #region PaymentHold

        [Authorize, HttpGet]
        public JsonResult GetListForHold(GridParameter parameters)
        {
            InventoryReceiveQueryService inventoryReceiveQueryService = new InventoryReceiveQueryService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(inventoryReceiveQueryService.GetListForHold(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult PaymentHold(IEnumerable<InventoryReceive> entities)
        {
            _inventoryReveiveService.PaymentHold(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion PaymentHold

        [Authorize, HttpGet]
        public JsonResult GetSupervisorCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetSupervisorCbo(), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetSupervisorCboApproved()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetSupervisorCboApproved(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult NotificationSetting()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {

                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='MaterialGoodsReceiptNote' and plantId='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult JWNotificationSetting()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {

                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='JobWorkPurchaseOrder' and plantId='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public JsonResult JWNotificationSettingReceipt()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {

                var sql = @"select * from dbo.NotificationSetting  where BusinessFlow='OutSourceGoodsReceiptNote' and plantId='" + identity.PlantId + "'";
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
            return Json(_inventoryReveiveService.GetCheckedByAndApprovedBY(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetJWCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetJWCheckedByAndApprovedBY(CheckedBy, ApprovedBy), JsonRequestBehavior.AllowGet);
        }




        [Authorize, HttpGet]
        public JsonResult GetACCCutOffDate()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetACCCutOffDate(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetShortageRejectionValue(string InventoryReceiveId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetShortageRejectionValue(InventoryReceiveId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpPost]
        public ActionResult UpdateShortageRejectionValue(string InventoryReceiveId, List<Dictionary<string, object>> UserSendData)
        {
            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                obj.UpdateShortageRejectionValue(InventoryReceiveId, UserSendData);
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
            catch (Exception)
            {

                return false;
            }


        }

        #region Documents Upload
        [HttpPost, Authorize]
        public JsonResult GRNDocCreate(FormCollection form, string POId)
        {
            var GRNDocumentMap = new JavaScriptSerializer().Deserialize<GRNDocumentMap>(form["GRNDocumentMap"]);

            var directory = ResourcesPathReader.GetGRNPath();
            var path = Path.Combine(directory);

            if (GRNDocumentMap.UserFilename.IsNotNull())
            {
                ResourcesPathReader.IsValidFileExtention(Path.GetExtension(GRNDocumentMap.UserFilename));
            }

            //var fileId = "";
            //var fileName = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GRNDocumentMap.CompanyGroupId = identity.CompanyGroupId;


            _inventoryReveiveService.InsertPODocMap(GRNDocumentMap, POId, out string Id);

            var file = Request.Files["file"];

            if (GRNDocumentMap.UserFilename.IsNotNull())
            {

                if (System.IO.File.Exists(path + GRNDocumentMap.POId))
                    System.IO.File.Delete(path + Id + Path.GetExtension(GRNDocumentMap.UserFilename));
                file.SaveAs(path + Id + Path.GetExtension(GRNDocumentMap.UserFilename));
            }
            return Json(new { GRNDocumentMap = GRNDocumentMap, Message = AplosMessage.Insert });
        }
        public JsonResult GRNDocumentMapData(string POID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();
                return Json(obj.GRNDocumentMapData(POID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Authorize, HttpPost]
        public ActionResult GRNImageDelete(string Id)
        {
            var fileId = "";
            var fileName = "";
            try
            {
                Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService obj = new Library.MaterialManagement.InventoryManagements.PurchaseOrderQueryService();

                var directory = ResourcesPathReader.GetGRNPath();
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
                var sql = @"Select Id, UserFilename From [TRN].[GRNDocumentMap] Where Id='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Additional Tax
        [Authorize, HttpPost]//
        public ActionResult SaveAdditinalTaxInGRN(string InventoryReceiveId, List<Dictionary<string, object>> UserSendData)
        {
            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                obj.SaveAdditinalTaxInGRN(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult GetAdvanceTaxInfo(string InventoryReceiveId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetAdvanceTaxInfo(InventoryReceiveId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult AdditionalTaxDelete(string Id)
        {

            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                obj.AdditionalTaxDelete(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        #endregion
        #region Additional Tax purchase return
        [Authorize, HttpPost]//
        public ActionResult SaveAdditinalTaxInPurchaseReturn(string InventoryReceiveId, List<Dictionary<string, object>> UserSendData)
        {
            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                obj.SaveAdditinalTaxInPurchaseReturn(InventoryReceiveId, UserSendData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult GetAdvanceTaxInfoPurchaseReturn(string PurchaseReturnId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetAdvanceTaxInfoPurchaseReturn(PurchaseReturnId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Authorize, HttpPost]
        public ActionResult AdditionalTaxDeletePurchaseReturn(string Id)
        {

            try
            {
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                obj.AdditionalTaxDeletePurchaseReturn(Id);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        #endregion

        [Authorize, HttpGet]
        public ActionResult FOCReport(string grnId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _inventoryReveiveService.GetFocReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

            return null;
        }

        [Authorize, HttpPost]
        public JsonResult CreateGRNBYBOQ(InventoryReceive entity, string entityMatAndImat, IEnumerable<InventoryReceiveTax> receiveTaxList, IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string GRNType, string AcceptanceId, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
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

            BOQDetailCreate(entity, entityMatAndImat1, receiveTaxList, entity.Id, entity.MaterialStorageId, GRNType, entityMatAndImat);
            BOQServiceChargesCreateNew(chargesListPO, POServiceTaxList, entity.Id, AcceptanceId);
            return Json(new { entity, Message = AplosMessage.Success + " GRN no <b>" + entity.Id + "</b>" });
        }
        public JsonResult BOQDetailCreate(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType,string entityMatAndImat)
        {
            try
            {
                #region Validation For R A T E
                foreach (var itemDetail in entityMat)
                {
                    var xy = entityMat.Where(q => q.MaterialMasterId == itemDetail.MaterialMasterId && q.ArticleId == itemDetail.ArticleId && q.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && q.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && q.MaterialTranRate != itemDetail.MaterialTranRate).ToList();
                    if (xy.Count > 1)
                    {
                        throw new Exception("Material transaction rate should be same!");
                    }
                }


                #endregion
                #region Duplicate Remove
                List<InventoryMaterialViewModel> List = new List<InventoryMaterialViewModel>();

                decimal addnew = 0;
                foreach (var item in entityMat)
                {
                    addnew = 0;
                    if (List.Count == 0)
                    {
                        List.Add(item);

                    }
                    else
                    {
                        for (int i = 0; i < List.Count; i++)
                        {
                            if (List[i].MaterialMasterId == item.MaterialMasterId && List[i].ArticleId == item.ArticleId && List[i].FirstCharacteristicsValueId == item.FirstCharacteristicsValueId && List[i].SecondCharacteristicsValueId == item.SecondCharacteristicsValueId)
                            {
                                if (item.NetQty != List[i].NetQty)
                                {
                                    List[i].Qty = List[i].Qty + item.Qty;
                                    List[i].BaseIssueQty = List[i].BaseIssueQty + item.BaseIssueQty;
                                    List[i].BaseQty = List[i].BaseQty + item.BaseQty;
                                    List[i].NetQty = List[i].NetQty + item.NetQty;
                                    List[i].TransactionQty = List[i].TransactionQty + item.TransactionQty;
                                    List[i].GRNTotalAmount = List[i].GRNTotalAmount + item.GRNTotalAmount;
                                    addnew = 1;
                                }
                                else
                                {
                                    addnew = 1;
                                }

                            }
                        }
                        if (addnew == 0)
                        {
                            List.Add(item);
                        }
                    }
                }

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                List<InventoryMaterialViewModel> entityMatAndImat1 = JsonConvert.DeserializeObject<List<InventoryMaterialViewModel>>(entityMatAndImat, settings);

                #endregion
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _inventoryDetailService.BOQInsertOrUpdateGraphNew(entity, entityMatAndImat1, taxCategoryList, id, MaterialStorageId, GRNType, List);
                return Json(new { Message = AplosMessage.Success });
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message });
            }
            
        }
        public JsonResult BOQServiceChargesCreateNew(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string Id, string AcceptanceId)
        {
            _inventoryService.InsertGraphNew(chargesListPO, POServiceTaxList, Id, AcceptanceId);
            return Json(new { Message = AplosMessage.Success });
        }
    }


}
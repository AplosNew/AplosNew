#region Using
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class InquiryController : Controller
    {
        #region Constructor
        private readonly IInquiryService _inquiryService;
        public InquiryController(IInquiryService inquiryService)
        {
            _inquiryService = inquiryService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(identity.EmployeeId))
                ViewBag.flag = false;
            else
                ViewBag.flag = true;
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inquiryService.Query(parameters, entityId,identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEntityCboWithProductionProcessGroup( string productionProcessGroupId)
        {
            return Json(_inquiryService.GetEntityCboWithProductionProcessGroup( productionProcessGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEntityWithInternal(GridParameter parameters, string companyGroupId, string productionProcessGroupId)
        {
            return Json(_inquiryService.EntityWithInternal(parameters, companyGroupId, productionProcessGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetCommitmentInquiryList(string inquiryId)
        {
            return Json(_inquiryService.QueryForCommitmentInquiry(inquiryId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductInquiryList(string inquiryId)
        {
            return Json(_inquiryService.QueryForProductInquiry(inquiryId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult QueryForIsPreCostingInquiry(GridParameter parameters)
        {
            return Json(_inquiryService.QueryForIsPreCostingInquiry(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductInquiryWithEntity(GridParameter parameters, string entityId)
        {
            return Json(_inquiryService.GetProductInquiryWithEntity(parameters,entityId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetIntermediateItemWithEntity(GridParameter parameters, string entityId)
        {
            return Json(_inquiryService.GetIntermediateItemWithEntity(parameters, entityId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductProcessGroupWithNotId(GridParameter parameters, string processProductionGroupId)
        {
            return Json(_inquiryService.GetProductProcessGroupWithNotId(parameters, processProductionGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductInquiryDetailList(string productInquiryId)
        {
            return Json(_inquiryService.QueryForProductInquiryDetailList(productInquiryId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult Create(Inquiry inquiry)
        {
            _inquiryService.InsertAndUpdate(inquiry);
            return Json(new { inquiry, Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult Edit(Inquiry inquiry)
        {
            _inquiryService.InsertAndUpdate(inquiry);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public JsonResult CommitmentInquiryCreate(IEnumerable<CommitmentInquiry> commitmentInquiry)
        {
            _inquiryService.InsertCommitmentInquiry(commitmentInquiry);
            return Json(new { commitmentInquiry, Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult ProductInquiryCreate(IEnumerable<ProductInquiry> productInquiry)
        {
            _inquiryService.InsertProductInquiry(productInquiry);
            return Json(new { productInquiry, Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult ProductInquiryDetailCreate(IEnumerable<ProductInquiryDetail> productInquiryDetail)
        {
            _inquiryService.InsertProductInquiryDetail(productInquiryDetail);
            return Json(new { productInquiryDetail, Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _inquiryService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpGet, Authorize]
        public ActionResult GetActivityWithBuyerMasterCbo(string buyermasterId)
        {
            return Json(_inquiryService.GetActivityWithBuyerMasterCbo(buyermasterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetQueryForResponsible(GridParameter parameters, string entityId, string buyerMasterId, string buyerActivityId)
        {
            return Json(_inquiryService.QueryForResponsible(parameters, entityId, buyerMasterId, buyerActivityId), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
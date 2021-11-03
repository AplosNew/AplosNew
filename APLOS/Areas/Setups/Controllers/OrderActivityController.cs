using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class OrderActivityController : BaseController
    {
        #region Constructor

        private readonly IOrderActivityService _buyerActivityService;

        public OrderActivityController(IOrderActivityService buyerActivityService)
        {
            _buyerActivityService = buyerActivityService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult BuyerActivity()
        {
            return View();
        }

        [Authorize]
        public ActionResult InquiryActivity()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string companyGroupId, string activityType)
        {
            return Json(_buyerActivityService.Query(parameters, companyGroupId, activityType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo(string activityType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerActivityService.GetCbo(identity.CompanyGroupId, activityType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult CreateBuyerActivity(OrderActivity entity)
        {
            _buyerActivityService.InsertBuyerActivity(entity);
            return Json(new { BuyerActivity = entity, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult EditBuyerActivity(OrderActivity entity)
        {
            _buyerActivityService.UpdateBuyerActivity(entity);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult CreateInquiryActivity(OrderActivity entity)
        {
            _buyerActivityService.InsertInquiryActivity(entity);
            return Json(new { BuyerActivity = entity, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult EditInquiryActivity(OrderActivity entity)
        {
            _buyerActivityService.UpdateInquiryActivity(entity);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _buyerActivityService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}
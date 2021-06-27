using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Materials.Controllers
{
    public class BuyerStyleController : BaseController
    {
        #region -- Constructor

        private readonly IBuyerStyleService _buyerStyleService;

        public BuyerStyleController(IBuyerStyleService buyerStyleService)
        {
            _buyerStyleService = buyerStyleService;
        }

        #endregion -- Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string buyerId)
        {
            return Json(_buyerStyleService.Query(parameters, buyerId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// cboService name getBuyerStyleCboByBuyer(buyerId)
        /// use : bulletin,
        /// </summary>
        /// <param name="buyerid"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult GetCbo(string buyerid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerStyleService.GetCbo(buyerid, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_buyerStyleService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBuyerStyle()
        {
            return Json(_buyerStyleService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BuyerStyle buyerStyle)
        {
            _buyerStyleService.Insert(buyerStyle);
            return Json(new { BuyerStyle = buyerStyle, Sequence = _buyerStyleService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BuyerStyle buyerStyle)
        {
            _buyerStyleService.Update(buyerStyle);
            return Json(new { Sequence = _buyerStyleService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _buyerStyleService.Delete(id);
            return Json(new { Sequence = _buyerStyleService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}
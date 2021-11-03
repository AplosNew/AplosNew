using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class BuyerCategoryController : BaseController
    {
        private readonly IBuyerCategoryService _buyerCategoryService;
        private readonly ICompanyGroupBuyerCategoryService _companyGroupBuyerCategoryService;

        public BuyerCategoryController(
            IBuyerCategoryService buyerCategoryService
            , ICompanyGroupBuyerCategoryService companyGroupBuyerCategoryService)
        {
            _buyerCategoryService = buyerCategoryService;
            _companyGroupBuyerCategoryService = companyGroupBuyerCategoryService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Parties/Views/BuyerCategory.cshtml");
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupBuyerCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupBuyerCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_buyerCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BuyerCategory buyerCategory)
        {
            _buyerCategoryService.Insert(buyerCategory);
            return Json(new { BuyerCategory = buyerCategory, Sequence = _buyerCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(BuyerCategory buyerCategory)
        {
            _buyerCategoryService.Update(buyerCategory);
            return Json(new { Sequence = _buyerCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _buyerCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _buyerCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
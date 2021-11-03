using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Banks;
using Library.Service.Banks;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankSubCategoryController : BaseController
    {
        private readonly IBankSubCategoryService _bankSubCategoryService;

        public BankSubCategoryController(IBankSubCategoryService bankSubCategoryService)
        {
            _bankSubCategoryService = bankSubCategoryService;
        }

        [Authorize, HttpGet]
        public ActionResult BankSubCategory()
        {
            return View("~/Areas/Banks/Views/BankSubCategory.cshtml");
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_bankSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetBankSubCategoryListCbo()
        {
            return Json(new SelectList(_bankSubCategoryService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_bankSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBankSubCategory()
        {
            return Json(_bankSubCategoryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBankSubCategoryById(string id)
        {
            return Json(_bankSubCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BankSubCategory bankSubCategory)
        {
            if (ModelState.IsValid)
            {
                _bankSubCategoryService.Insert(bankSubCategory);
                return Json(new { BankSubCategory = bankSubCategory, Sequence = _bankSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(BankSubCategory bankSubCategory)
        {
            if (ModelState.IsValid)
            {
                _bankSubCategoryService.Update(bankSubCategory);
                return Json(new { Sequence = _bankSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _bankSubCategoryService.Delete(id);
                return Json(new { Sequence = _bankSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}
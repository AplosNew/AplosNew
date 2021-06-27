using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Banks;
using Library.Service.Banks;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankCategoryController : BaseController
    {
        private readonly IBankCategoryService _bankCategoryService;

        public BankCategoryController(IBankCategoryService bankCategoryService)
        {
            _bankCategoryService = bankCategoryService;
        }

        [HttpGet]
        public ActionResult BankCategory()
        {
            return View("~/Areas/Banks/Views/BankCategory.cshtml");
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_bankCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCategoryListCbo()
        {
            return Json(new SelectList(_bankCategoryService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_bankCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBankCategory()
        {
            return Json(_bankCategoryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetBankCategoryById(string id)
        {
            return Json(_bankCategoryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BankCategory bankCategory)
        {
            if (ModelState.IsValid)
            {
                _bankCategoryService.Insert(bankCategory);
                return Json(new { BankCategory = bankCategory, Sequence = _bankCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(BankCategory bankCategory)
        {
            if (ModelState.IsValid)
            {
                _bankCategoryService.Update(bankCategory);
                return Json(new { Sequence = _bankCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _bankCategoryService.Delete(id);
                return Json(new { Sequence = _bankCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}
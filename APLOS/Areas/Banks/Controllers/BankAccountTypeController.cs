using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Banks;
using Library.Service.Banks;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankAccountTypeController : BaseController
    {
        private readonly IBankAccountTypeService _bankAccountTypeService;

        public BankAccountTypeController(IBankAccountTypeService bankAccountTypeService)
        {
            _bankAccountTypeService = bankAccountTypeService;
        }

        [HttpGet]
        public ActionResult BankAccountType()
        {
            return View("~/Areas/Banks/Views/BankAccountType.cshtml");
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_bankAccountTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBankAccountTypeListCbo()
        {
            return Json(new SelectList(_bankAccountTypeService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_bankAccountTypeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetbankAccountType()
        {
            return Json(_bankAccountTypeService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetbankAccountTypeById(string id)
        {
            return Json(_bankAccountTypeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BankAccountType bankAccountType)
        {
            if (ModelState.IsValid)
            {
                _bankAccountTypeService.Insert(bankAccountType);
                return Json(new { BankAccountType = bankAccountType, Sequence = _bankAccountTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(BankAccountType bankAccountType)
        {
            if (ModelState.IsValid)
            {
                _bankAccountTypeService.Update(bankAccountType);
                return Json(new { Sequence = _bankAccountTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _bankAccountTypeService.Delete(id);
                return Json(new { Sequence = _bankAccountTypeService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}
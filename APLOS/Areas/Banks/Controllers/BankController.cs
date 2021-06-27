using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Addresses;
using Library.Model.Banks;
using Library.Service.Banks;
using System.Web.Mvc;

namespace Aplos.Areas.Banks.Controllers
{
    public class BankController : BaseController
    {
        private readonly IBankService _bankService;

        public BankController(IBankService bankService)
        {
            _bankService = bankService;
        }

        [HttpGet]
        public ActionResult Bank()
        {
            return View("~/Areas/Banks/Views/Bank.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetBankListCbo()
        {
            return Json(new SelectList(_bankService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_bankService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_bankService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetbankById(string id)
        {
            return Json(_bankService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Bank bank, AddressMaster addressMaster, ContactMaster contactMaster)
        {
            _bankService.Insert(bank, addressMaster, contactMaster);
            return Json(new { Bank = bank, Sequence = _bankService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Bank bank, AddressMaster addressMaster, ContactMaster contactMaster)
        {
            _bankService.Update(bank, addressMaster, contactMaster);
            return Json(new { Sequence = _bankService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _bankService.Delete(id);
                return Json(new { Sequence = _bankService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}
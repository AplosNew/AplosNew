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
    public class BankBranchController : BaseController
    {
        private readonly IBankBranchService _bankBranchService;

        public BankBranchController(IBankBranchService bankBranchService)
        {
            _bankBranchService = bankBranchService;
        }

        [HttpGet]
        public ActionResult BankBranch()
        {
            return View("~/Areas/Banks/Views/BankBranch.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetBankBranchListCbo()
        {
            return Json(new SelectList(_bankBranchService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByBankId(string bankid)
        {
            return Json(new SelectList(_bankBranchService.GetCboList(bankid), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_bankBranchService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string bankId)
        {
            return Json(_bankBranchService.Query(parameters, bankId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetbankBranchById(string id)
        {
            return Json(_bankBranchService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BankBranch bankBranch, AddressMaster addressMaster, ContactMaster contactMaster)
        {
            _bankBranchService.Insert(bankBranch, addressMaster, contactMaster);
            return Json(new { BankBranch = bankBranch, Sequence = _bankBranchService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BankBranch bankBranch, AddressMaster addressMaster, ContactMaster contactMaster)
        {
            _bankBranchService.Update(bankBranch, addressMaster, contactMaster);
            return Json(new { BankBranch = bankBranch, Sequence = _bankBranchService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _bankBranchService.Delete(id);
                return Json(new { Sequence = _bankBranchService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class AccountTypeController : BaseController
    {
        private readonly IAccountTypeService _accountTypeService;

        public AccountTypeController(IAccountTypeService accountTypeService)
        {
            _accountTypeService = accountTypeService;
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/AccountType.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_accountTypeService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAccountTypeList(GridParameter parameters)
        {
            return Json(_accountTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckIdUse(string id)
        {
            return Json(_accountTypeService.CheckUsing(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AccountType accountType)
        {
            _accountTypeService.Insert(accountType);
            return Json(new { AccountType = accountType, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(AccountType accountType)
        {
            _accountTypeService.Update(accountType);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _accountTypeService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}
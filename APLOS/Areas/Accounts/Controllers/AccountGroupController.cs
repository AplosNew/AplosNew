using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class AccountGroupController : BaseController
    {
        private readonly IAccountGroupService _accountGroupService;

        public AccountGroupController(IAccountGroupService accountGroupService)
        {
            _accountGroupService = accountGroupService;
        }

        [HttpGet, Authorize]
        public JsonResult GetAccountGroupListCbo()
        {
            return Json(_accountGroupService.GetCboAccountGroupList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAccountGroupCOAWiseListCbo(string coaId)
        {
            return Json(_accountGroupService.GetCboAccountGroupCOAWiseList(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/AccountGroup.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string coaId)
        {
            return Json(_accountGroupService.GetAutoSequence(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMaxNumberRange(string coaId)
        {
            return Json(_accountGroupService.GetMaxNumberRange(coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAccountGroupList(GridParameter parameters, string coaId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountGroupService.Query(parameters, identity.CompanyGroupId, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAccountGroupNumberChange(GridParameter parameters, string accountGroupId)
        {
            return Json(_accountGroupService.GetAccountGroupNumberRange(parameters, accountGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAccountGroupById(string id)
        {
            return Json(_accountGroupService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AccountGroup accountGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            accountGroup.CompanyGroupId = identity.CompanyGroupId;
            _accountGroupService.Insert(accountGroup);
            return Json(new { AccountGroup = accountGroup, Sequence = _accountGroupService.GetAutoSequence(accountGroup.COAId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(AccountGroup accountGroup)
        {
            _accountGroupService.Update(accountGroup);
            return Json(new { AccountGroup = accountGroup, Sequence = _accountGroupService.GetAutoSequence(accountGroup.COAId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _accountGroupService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}
#region Using

using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Service.Accounts;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Accounts.Controllers
{
    public class AccountDashboardController : BaseController
    {
        private readonly IAccountDashboardService _accDashBoardService;

        public AccountDashboardController(IAccountDashboardService accDashBoardService)
        {
            _accDashBoardService = accDashBoardService;
        }

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetOverAllReceivableWithPartyCurrency(string partyId, string currencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accDashBoardService.OverAllReceivableWithPartyCurrency(identity.CompanyId, partyId, currencyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOverAllPayableWithPartyCurrency(string partyId, string currencyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accDashBoardService.OverAllPayableWithPartyCurrency(identity.CompanyId, partyId, currencyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOverDueReceivableModal(string partyId, string currencyId, string matureDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accDashBoardService.OverDueReceivableModal(identity.CompanyId, partyId, currencyId, matureDate), JsonRequestBehavior.AllowGet);
        }
    }
}
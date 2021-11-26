using Aplos.Controllers;
using Library.Core;
using Library.Service.Logs;
using System.Web.Mvc;

namespace Aplos.Areas.Securities.Controllers
{
    /// <summary>
    /// AuthTokenLockLogController
    /// </summary>
    public class AuthTokenLockLogController : BaseController
    {
        #region Constructor

        private readonly IAuthTokenLockLogService _authTokenLockLogService;

        public AuthTokenLockLogController(IAuthTokenLockLogService authTokenLockLogService)
        {
            _authTokenLockLogService = authTokenLockLogService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult AuthTokenLockDateDetailsWithoutSyAdmin(GridParameter parameters, string id)
        {
            return Json(_authTokenLockLogService.AuthTokenLockDateDetails(parameters, id), JsonRequestBehavior.AllowGet);
        }
    }
}
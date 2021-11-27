using Aplos.Controllers;
using Library.Core;
using Library.Service.Logs;
using System.Web.Mvc;

namespace Aplos.Areas.Securities.Controllers
{
    /// <summary>
    /// UserLockLog Controller
    /// </summary>
    public class UserLockLogController : BaseController
    {
        #region Constructor

        private readonly IUserLockLogService _userLockLogService;

        public UserLockLogController(IUserLockLogService userLockLogService)
        {
            _userLockLogService = userLockLogService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult UserLockDateDetailsWithoutSyAdmin(GridParameter parameters, string id)
        {
            return Json(_userLockLogService.UserLockDateDetails(parameters, id), JsonRequestBehavior.AllowGet);
        }
    }
}
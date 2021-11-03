#region Using
using Aplos.Controllers;
using Library.Core;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Logs.Controllers
{
    public class MailLogController : BaseController
    {
        #region Constructor
        private readonly IMailLogService _MailLogService;
        public MailLogController(IMailLogService MailLogService)
        {
			_MailLogService = MailLogService;
        }
        #endregion

        public ActionResult Aplos()
        {
            return View();
        }

		[HttpGet, Authorize]
		public ActionResult GetMailLogList(GridParameter parameters, string fromDate, string toDate)
		{
			return Json(_MailLogService.MailLogList(parameters,fromDate, toDate), JsonRequestBehavior.AllowGet);
		}
	}
}
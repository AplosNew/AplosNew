#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Service.Logs;
using Library.Data;
using Library.Core;
using System;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Logs.Controllers
{
    public class ActionLogController : BaseController
    {
        #region Constructor
        private readonly IActionLogService _actionLogService;
        public ActionLogController(IActionLogService actionLogService)
        {
            _actionLogService = actionLogService;
        }
        #endregion

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Get(GridParameter parameters, string fromDate, string toDate)
        {
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                return Json(_actionLogService.Query(parameters, Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate)), JsonRequestBehavior.AllowGet);
            else
                throw new CustomException("Please select date from and date to.");
        }

        [HttpPost]
        public ActionResult Delete(DateTime fromDate, DateTime toDate)
        {
            _actionLogService.Delete(fromDate, toDate);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}
#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Service.Logs;
using Library.Core;
using Library.Data;
using System;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Logs.Controllers
{
    public class AccessLogController : BaseController
    {
        #region Constructor
        private readonly IAccessLogService _accessLogService;
        public AccessLogController(IAccessLogService accessLogService)
        {
            _accessLogService = accessLogService;
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
                return Json(_accessLogService.Query(parameters, Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate)), JsonRequestBehavior.AllowGet);
            else
                throw new CustomException("Please select date from and date to.");
        }

        [HttpPost]
        public ActionResult Delete(string fromDate, string toDate)
        {
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                _accessLogService.Delete(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate));
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException("Please select date from and date to.");
        }
    }
}
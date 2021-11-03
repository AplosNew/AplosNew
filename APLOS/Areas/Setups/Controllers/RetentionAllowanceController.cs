#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Payrolls;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class RetentionAllowanceController : BaseController
    {
        #region Constructor

        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly IRetentionAllowanceService _retentionAllowanceService;

        public RetentionAllowanceController(IRetentionAllowanceService retentionAllowanceService
            )
        {
            this._retentionAllowanceService = retentionAllowanceService;
        }

        #endregion Constructor

        #region Aplos

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Aplos

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_retentionAllowanceService.Query(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(GridParameter parameters, string masterId)
        {
            return Json(_retentionAllowanceService.QueryWithMaster(parameters, masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(RetentionAllowanceMaster model, IEnumerable<RetentionAllowanceDetail> entities)
        {
            _retentionAllowanceService.InsertUpdate(model, entities);
            return Json(new { RetentionAllowance = model, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _retentionAllowanceService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}
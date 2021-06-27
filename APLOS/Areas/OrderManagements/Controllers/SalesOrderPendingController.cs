using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Materials;
using Library.Service.Productions;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class SalesOrderPendingController : BaseController
    {
        #region Constructor
        private readonly IMaterialMasterService _mms;

        public SalesOrderPendingController(
            IMaterialMasterService mms
            )
        {
            _mms = mms;
        }
        #endregion

        #region -- Pages
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        [Authorize]
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string entityid)
        {
            //return Json(_somms.LoadPendingSOMM(parameters, entityid), JsonRequestBehavior.AllowGet);
            return Json(new { });
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<SalesOrderMaterialMaster> list)
        {
            //_somms.PendingSaveChange(list);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion
    }
}
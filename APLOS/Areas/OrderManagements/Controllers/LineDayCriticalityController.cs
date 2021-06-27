#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class LineDayCriticalityController : BaseController
    {
        #region Constructor
        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly ILineDayCriticalityService _buyerDepartmentService;

        public LineDayCriticalityController(ILineDayCriticalityService buyerDepartmentService
            )
        {
            this._buyerDepartmentService = buyerDepartmentService;
        }
        #endregion


        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerDepartmentService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLineDayCriticalById(string id)
        {
            return Json(_buyerDepartmentService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<LineDayCriticality> lineDayCriticality)
        {
            _buyerDepartmentService.InsertOrUpdate(lineDayCriticality);
            return Json(new { LineDayCritical = lineDayCriticality, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(LineDayCriticality buyerDepartment)
        {
            _buyerDepartmentService.Update(buyerDepartment);
            return Json(new {Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string worKday)
        {
            _buyerDepartmentService.DeleteGraph(worKday);
            return Json(new {Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
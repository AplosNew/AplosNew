#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class UnitOfMeasurementController : BaseController
    {
        #region Constructor

        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly IUnitOfMeasurementService _unitOfMeasurementService;

        public UnitOfMeasurementController(IUnitOfMeasurementService unitOfMeasurementService
            )
        {
            this._unitOfMeasurementService = unitOfMeasurementService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_unitOfMeasurementService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        #region Aplos

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Aplos

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence(string uomDId)
        {
            return Json(_unitOfMeasurementService.GetAutoSequence(uomDId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetUnitOfMeasurementCbo()
        {
            return Json(_unitOfMeasurementService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetUnitOfMeasurementList(GridParameter parameters, string UOMDId)
        {
            return Json(_unitOfMeasurementService.Query(parameters, UOMDId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUnitOfMeasurementById(string id)
        {
            return Json(_unitOfMeasurementService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(UnitOfMeasurement unitOfMeasurement)
        {
            _unitOfMeasurementService.Insert(unitOfMeasurement);
            return Json(new { UnitOfMeasurement = unitOfMeasurement, Sequence = _unitOfMeasurementService.GetAutoSequence(unitOfMeasurement.UOMDId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(UnitOfMeasurement unitOfMeasurement)
        {
            _unitOfMeasurementService.Update(unitOfMeasurement);
            return Json(new { Sequence = _unitOfMeasurementService.GetAutoSequence(unitOfMeasurement.UOMDId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var unitOfMeasurement = _unitOfMeasurementService.Find(id);
            _unitOfMeasurementService.DeleteGraph(id);
            return Json(new { Sequence = _unitOfMeasurementService.GetAutoSequence(unitOfMeasurement.UOMDId), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}
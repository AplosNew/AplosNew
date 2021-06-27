#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionBookingPeriodController : BaseController
    {
        #region Constructor
        /// <summary>   The ProductionBookingPeriodService service. </summary>
        private readonly IProductionBookingPeriodService _ProductionBookingPeriodService;

        public ProductionBookingPeriodController(IProductionBookingPeriodService ProductionBookingPeriodService)
        {
            _ProductionBookingPeriodService = ProductionBookingPeriodService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_ProductionBookingPeriodService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_ProductionBookingPeriodService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_ProductionBookingPeriodService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProductionBookingPeriod productionBookingPeriod)
        {
            _ProductionBookingPeriodService.Insert(productionBookingPeriod);
            return Json(new { ProductionBookingPeriod= productionBookingPeriod, Sequence=_ProductionBookingPeriodService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ProductionBookingPeriod productionBookingPeriod)
        {
            _ProductionBookingPeriodService.Update(productionBookingPeriod);
            return Json(new { Sequence = _ProductionBookingPeriodService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _ProductionBookingPeriodService.DeleteGraph(id);
            return Json(new { Sequence = _ProductionBookingPeriodService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
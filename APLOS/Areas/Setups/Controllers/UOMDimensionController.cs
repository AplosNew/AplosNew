using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class UOMDimensionController : BaseController
    {
        #region Constructor

        private readonly IUOMDimensionService _uOMDimensionService;

        public UOMDimensionController(IUOMDimensionService uOMDimensionService)
        {
            _uOMDimensionService = uOMDimensionService;
        }

        #endregion Constructor

        [Authorize, HttpGet]
        public JsonResult GetUOMDimensionCbo()
        {
            return Json(new SelectList(_uOMDimensionService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_uOMDimensionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_uOMDimensionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(UOMDimension uOMDimension)
        {
            _uOMDimensionService.Insert(uOMDimension);
            return Json(new { UOMDimension = uOMDimension, Sequence = uOMDimension.Sequence + 1, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(UOMDimension uOMDimension)
        {
            _uOMDimensionService.Update(uOMDimension);
            return Json(new { Sequence = _uOMDimensionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _uOMDimensionService.Delete(id);
            return Json(new { Sequence = _uOMDimensionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
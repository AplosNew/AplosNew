#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ShipModeController : BaseController
    {
        #region Constructor
        /// <summary>   The skillCategoryService service. </summary>
        private readonly IShipModeService _skillCategoryService;
        private readonly ICompanyGroupShipModeService _companyGroupShipModeService;

        public ShipModeController(IShipModeService skillCategoryService, ICompanyGroupShipModeService companyGroupShipModeService)
        {
            _skillCategoryService = skillCategoryService;
            _companyGroupShipModeService = companyGroupShipModeService;
        }
        #endregion

        #region -- Pages
     
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyGroupShipModeService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GeShipModeCbo(string portid)
        {
            return Json(_companyGroupShipModeService.GeShipModeCbo(portid), JsonRequestBehavior.AllowGet);
           // return Json(new SelectList(_companyGroupShipModeService.GeShipModeCbo(portid), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupShipModeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_skillCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ShipMode skillCategory)
        {
            _skillCategoryService.Insert(skillCategory);
            return Json(new { ShipMode= skillCategory, Sequence=_skillCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ShipMode skillCategory)
        {
            _skillCategoryService.Update(skillCategory);
            return Json(new { Sequence = _skillCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _skillCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _skillCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
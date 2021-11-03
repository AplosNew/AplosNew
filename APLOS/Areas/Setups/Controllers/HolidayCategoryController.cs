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
    public class HolidayCategoryController : BaseController
    {
        #region Constructor

        private readonly IHolidayCategoryService _holidayCategoryService;

        public HolidayCategoryController(
              IHolidayCategoryService holidayCategoryService
            )
        {
            _holidayCategoryService = holidayCategoryService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(_holidayCategoryService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string companyGroupId)
        {
            return Json(_holidayCategoryService.QueryGraph(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_holidayCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(HolidayCategory model)
        {
            _holidayCategoryService.Insert(model);
            return Json(new { HolidayCategory = model, Sequence = _holidayCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(HolidayCategory model)
        {
            _holidayCategoryService.Update(model);
            return Json(new { Sequence = _holidayCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _holidayCategoryService.Delete(id);
            return Json(new { Sequence = _holidayCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}
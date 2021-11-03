using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Web.Mvc;
using Library.Data;
using Aplos.Controllers;
using Aplos.Properties;

namespace Aplos.Areas.OrderManagements.Controllers
{
     public class SeasonsController : BaseController
    {
        #region -- Constructor
        private readonly ISeasonsService _SeasonsService;

        public SeasonsController(ISeasonsService SeasonsService)
        {
            this._SeasonsService = SeasonsService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_SeasonsService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_SeasonsService.GetSeasonsCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_SeasonsService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetSeasons()
        {
            return Json(_SeasonsService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetSeasonsById(string id)
        {
            return Json(_SeasonsService.Find(id), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(Seasons Seasons)
        {
            _SeasonsService.Insert(Seasons);
            return Json(new {Seasons, Sequence = _SeasonsService.GetAutoSequence(), Message = AplosMessage.Insert });
        }


        [HttpPost]
        public JsonResult Edit(Seasons Seasons)
        {
            _SeasonsService.Update(Seasons);
            return Json(new { Sequence = _SeasonsService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _SeasonsService.Delete(id);
                return Json(new { Sequence = _SeasonsService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}
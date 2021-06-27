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
    public class BrandController : BaseController
    {
        #region Constructor

        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_brandService.GetCboList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_brandService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_brandService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Brand brand)
        {
            _brandService.Insert(brand);
            return Json(new { Brand = brand, Sequence = _brandService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Brand brand)
        {
            _brandService.Update(brand);
            return Json(new { Sequence = _brandService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _brandService.Archive(id);
            return Json(new { Sequence = _brandService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class SpecialTaxController : BaseController
    {
        #region -- Constructor

        private readonly ISpecialTaxService _SpecialTaxService;

        public SpecialTaxController(ISpecialTaxService SpecialTaxService)
        {
            this._SpecialTaxService = SpecialTaxService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters, string countryId)
        {
            return Json(_SpecialTaxService.Query(parameters, countryId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo(string plantId)
        {
            return Json(_SpecialTaxService.GetCbo(plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_SpecialTaxService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSpecialTax()
        {
            return Json(_SpecialTaxService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SpecialTax SpecialTax)
        {
            _SpecialTaxService.Insert(SpecialTax);
            return Json(new { SpecialTax = SpecialTax, Sequence = _SpecialTaxService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SpecialTax SpecialTax)
        {
            _SpecialTaxService.Update(SpecialTax);
            return Json(new { Sequence = _SpecialTaxService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _SpecialTaxService.Delete(id);
            return Json(new { Sequence = _SpecialTaxService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}
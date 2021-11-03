#region using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.Machines;
using Library.Model.Machines;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class ProductionSystemController : BaseController
    {
        #region Constructor
        private readonly IProductionSystemService _productionSystemService;

        public ProductionSystemController(IProductionSystemService productionSystemService)
        {
            _productionSystemService = productionSystemService;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_productionSystemService.GetCbo(idntity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productionSystemService.Query(parameters, idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productionSystemService.GetAutoSequence(idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProductionSystem entity)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = idntity.CompanyGroupId;
            if (string.IsNullOrEmpty(entity.PlantId))
                entity.PlantId = idntity.PlantId;
            _productionSystemService.Insert(entity);
            return Json(new { entity, Sequence = _productionSystemService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ProductionSystem entity)
        {
            _productionSystemService.Update(entity);
            return Json(new { Sequence = _productionSystemService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var entity = _productionSystemService.Find(id);
            _productionSystemService.Delete(entity);
            return Json(new { Sequence = _productionSystemService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        #endregion
    }
}
#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductionResourcesController : BaseController
    {
        #region Constructor

        private readonly IProductionResourcesService _ProductionResources;

        public ProductionResourcesController(
              IProductionResourcesService ProductionResourcesService
            )
        {
            _ProductionResources = ProductionResourcesService;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet, Authorize]
        public ActionResult GetList(string PlantId)
        {
            return Json(_ProductionResources.Query(PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(ProductionResources productionResources)
        {
            _ProductionResources.Insert(productionResources);
            return Json(new { Resources = productionResources, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult Edit(ProductionResources productionResources)
        {
            _ProductionResources.Update(productionResources);
            return Json(new {Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _ProductionResources.Delete(id);
            return Json(new {Message = AplosMessage.Deleted });
        }
    }
}
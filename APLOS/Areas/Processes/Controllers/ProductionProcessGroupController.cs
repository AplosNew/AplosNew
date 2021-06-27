#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Processes;
using Library.Service.Processes;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class ProductionProcessGroupController : BaseController
    {
        #region Constructor
        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly IProductionProcessGroupService _productionProcessGroupService;

        public ProductionProcessGroupController(IProductionProcessGroupService productionProcessGroupService
            )
        {
            this._productionProcessGroupService = productionProcessGroupService;
        }
        #endregion

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(_productionProcessGroupService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_productionProcessGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productionProcessGroupService.Query(parameters,identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductionProcessGroupById(string id)
        {
            return Json(_productionProcessGroupService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProductionProcessGroup productionProcessGroup)
        {
            _productionProcessGroupService.Insert(productionProcessGroup);
            return Json(new { ProductionProcessGroup = productionProcessGroup, Sequence = _productionProcessGroupService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ProductionProcessGroup productionProcessGroup)
        {
            _productionProcessGroupService.Update(productionProcessGroup);
            return Json(new { Sequence = _productionProcessGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _productionProcessGroupService.DeleteGraph(id);
            return Json(new { Sequence = _productionProcessGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
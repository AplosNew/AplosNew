#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionStatusController : BaseController
    {
        #region Constructor
        /// <summary>   The ProductionStatusService service. </summary>
        private readonly IProductionStatusService _productionStatusService;
        private readonly ICompanyGroupProductionStatusService _companyGroupProductionStatusService;

        public ProductionStatusController(IProductionStatusService ProductionStatusService, ICompanyGroupProductionStatusService companyGroupProductionStatusService)
        {
            _productionStatusService = ProductionStatusService;
            _companyGroupProductionStatusService = companyGroupProductionStatusService;
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
        //[Authorize]
        //public JsonResult GetCbo()
        //{
        //    return Json(new SelectList(_companyGroupProductionStatusService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}
        [Authorize]
        public JsonResult GetCbo()
        {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupProductionStatusService.GetStatusCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupProductionStatusService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_productionStatusService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProductionStatus productionStatus)
        {
            _productionStatusService.Insert(productionStatus);
            return Json(new { ProductionStatus= productionStatus, PlanningGroupPriority = _productionStatusService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ProductionStatus productionStatus)
        {
            _productionStatusService.Update(productionStatus);
            return Json(new { PlanningGroupPriority = _productionStatusService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _productionStatusService.DeleteGraph(id);
            return Json(new { PlanningGroupPriority = _productionStatusService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
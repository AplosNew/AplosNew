#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Aplos.Controllers;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class DestinationController : BaseController
    {
        #region Constructor
        /// <summary>   The skillCategoryService service. </summary>
        private readonly IDestinationService _skillCategoryService;
        private readonly ICompanyGroupDestinationService _companyGroupDestinationService;

        public DestinationController(IDestinationService skillCategoryService, ICompanyGroupDestinationService companyGroupDestinationService)
        {
            _skillCategoryService = skillCategoryService;
            _companyGroupDestinationService = companyGroupDestinationService;
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
        public JsonResult GetCbo()//
        {
            return Json(new SelectList(_companyGroupDestinationService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GeDestinationCbo(string portid)//GeDestinationCbo
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupDestinationService.GeDestinationCbo(portid, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GeDestinationCbobyCountry(string CountryId)//GeDestinationCbo
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupDestinationService.GeDestinationCbobyCountry(CountryId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupDestinationService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_skillCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Destination skillCategory)
        {
            _skillCategoryService.Insert(skillCategory);
            return Json(new { Destination= skillCategory, Sequence=_skillCategoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(Destination skillCategory)
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
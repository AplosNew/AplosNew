using Aplos.Controllers;
using Aplos.Properties;
using Library.Model.Menus;
using Library.Service.Menus;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Menus.Controllers
{
    public class CompanyGroupMenuMasterController : BaseController
    {
        private readonly ICompanyGroupMenuMasterService _companyGroupMenuMasterService;

        public CompanyGroupMenuMasterController(
            ICompanyGroupMenuMasterService companyGroupMenuMasterService)
        {
            _companyGroupMenuMasterService = companyGroupMenuMasterService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Menus/Views/CompanyGroupMenuMaster.cshtml");
        }

        [HttpGet]
        public JsonResult GetList(string companyGroupId, string moduleId, string menuFrameId)
        {
            return Json(_companyGroupMenuMasterService.Query(companyGroupId, moduleId, menuFrameId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyGroupMenuMaster> entities)
        {
            _companyGroupMenuMasterService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}
#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using System.Web;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.IO;
using Library.Model.Setups;

#endregion

namespace Aplos.Areas.Setups.Controllers
{
    public class RptConfigTemplateController : BaseController
    {
        #region Constructor
        private readonly IRptConfigTemplateService _rptConfigTemplateService;

        public RptConfigTemplateController(
              IRptConfigTemplateService rptConfigTemplateService
            )
        {
            _rptConfigTemplateService = rptConfigTemplateService;
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
        //    return Json(new SelectList(_companyGroupSOPCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetPlantCbo()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_rptConfigTemplateService.GetPlantCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLanguageCbo()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_rptConfigTemplateService.GetLanguageCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_rptConfigTemplateService.GetConfigTemplate(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(RptConfigTemplate rptConfigTemplate)
        {
            _rptConfigTemplateService.Insert(rptConfigTemplate);
            return Json(new { RptConfigTemplate = rptConfigTemplate, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(RptConfigTemplate rptConfigTemplate)
        {
            _rptConfigTemplateService.Update(rptConfigTemplate);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _rptConfigTemplateService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion
    }
}
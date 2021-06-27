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
    public class LanguageController : BaseController
    {
        #region Constructor

        private readonly ILanguageService _languageService;

        public LanguageController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_languageService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_languageService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Language language)
        {
            _languageService.Insert(language);
            return Json(new { Language = language, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Language language)
        {
            _languageService.Update(language);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _languageService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}
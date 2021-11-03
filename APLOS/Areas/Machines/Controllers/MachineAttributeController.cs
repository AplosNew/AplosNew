#region Using
using Aplos.Controllers;
using Library.Model.Machines;
using Aplos.Properties;
using Library.Service.Machines;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Materials;
using System.Collections.Generic;
using Library.Model.Materials;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class MachineAttributeController : BaseController
    {
        #region -- Constrator
        private readonly IMaterialMasterArticleService _articleService;
        public MachineAttributeController(IMaterialMasterArticleService articleService)
        {
            _articleService = articleService;
        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- MachineClasss

        [HttpGet, Authorize]
        public JsonResult GetList(string materialMasterId)
        {
            return Json(_articleService.Query(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(IEnumerable<MaterialMasterArticle> entities)
        {
            _articleService.UpdateGraph(entities);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion
    }
}
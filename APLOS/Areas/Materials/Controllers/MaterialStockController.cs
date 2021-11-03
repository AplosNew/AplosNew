using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialStockController : BaseController
    {
        #region -- Constructor
        private readonly IMaterialStockService _baseService;

        public MaterialStockController(IMaterialStockService baseService)
        {
            _baseService = baseService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region Operations
        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterList(GridParameter parameters,string paramList)
        {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_baseService.GetMaterialMasterList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(paramList)), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string tempParam)
        {
            return Json(_baseService.Query(parameters,new JavaScriptSerializer().Deserialize<string[]>(tempParam)), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(MaterialStock entity)
        {
            _baseService.Insert(entity);
            return Json(new { Entity = entity, Message = AplosMessage.Insert });
        }
        [HttpPost]
        public JsonResult Edit(MaterialStock MaterialStock)
        {
            _baseService.Update(MaterialStock);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]
        public JsonResult Delete(string id)
        {
            _baseService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
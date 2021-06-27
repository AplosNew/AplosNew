#region using
using Aplos.Properties;
using Library.Data;
using Aplos.Controllers;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Web.Mvc;
#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialMasterAlternativeUOMController : BaseController
    {
        #region Constructor
        private readonly IMaterialMasterAlternativeUOMService _materialMasterAlternativeUOMService;

        public MaterialMasterAlternativeUOMController(IMaterialMasterAlternativeUOMService materialMasterAlternativeUOMService)
        {
            this._materialMasterAlternativeUOMService = materialMasterAlternativeUOMService;
        }
        #endregion

        #region --pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        //[HttpGet]
        //public ActionResult GetList(GridParameter parameters)
        //{
        //    return Json(_materialMasterAlternativeUOMService.GetSearchData(parameters), JsonRequestBehavior.AllowGet);
        //}
        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterAlternativeUOM(string id)
        {
            return Json(_materialMasterAlternativeUOMService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MaterialMasterAlternativeUOM materialMasterAlternativeUOM)
        {
            if (ModelState.IsValid)
            {
                _materialMasterAlternativeUOMService.Insert(materialMasterAlternativeUOM);
                return Json(new { Id = materialMasterAlternativeUOM.Id, Message = AplosMessage.Insert });
                //return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(MaterialMasterAlternativeUOM materialMasterAlternativeUOM
            )
        {
            if (ModelState.IsValid)
            {
                _materialMasterAlternativeUOMService.Update(materialMasterAlternativeUOM);
                return Json(new { Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        //public ActionResult Delete(string id)
        //{
        //    if (!string.IsNullOrEmpty(id))
        //    {
        //        _materialMasterAlternativeUOMService.Archive(id);
        //        return Json(new { Message = AplosMessage.Delete });
        //    }
        //    else
        //        throw new CustomException(Resources.IdNotFound);
        //}
        #endregion
    }
}
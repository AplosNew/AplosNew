#region using
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Service.Materials;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class DefectCodeDetailController : BaseController
    {
        #region -- Constructor
        private readonly IDefectCodeDetailService _defectCodeDetailService;

        public DefectCodeDetailController(IDefectCodeDetailService defectCodeDetailService)
        {
            this._defectCodeDetailService = defectCodeDetailService;
        }
        #endregion

        #region Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        //[HttpGet, Authorize]
        //public JsonResult GetList(GridParameter parameters)
        //{
        //    return Json(_defectCodeDetailService.Query(parameters), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetDefectCodeDetail()
        {
            return Json(_defectCodeDetailService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDefectCodeDetailById(string id)
        {
            return Json(_defectCodeDetailService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(DefectCodeDetail defectCodeDetail)
        {
            _defectCodeDetailService.Insert(defectCodeDetail);
            return Json(new { DefectCodeDetail = defectCodeDetail, Message = AplosMessage.Insert });
        }


        [HttpPost]
        public JsonResult Edit(DefectCodeDetail defectCodeDetail)
        {
            _defectCodeDetailService.Update(defectCodeDetail);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _defectCodeDetailService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
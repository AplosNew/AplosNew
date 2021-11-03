#region using
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Service.Materials;
using Library.Core;
using System.Web.Mvc;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class DefectCodeController : BaseController
    {
        #region -- Constructor
        private readonly IDefectCodeService _defectCodeService;
        private readonly IDefectCodeDetailService _defectCodeDetailService;

        public DefectCodeController(
            IDefectCodeService defectCodeService,
            IDefectCodeDetailService defectCodeDetailService)
        {
            this._defectCodeService = defectCodeService;
            this._defectCodeDetailService = defectCodeDetailService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string processId)
        {
            return Json(_defectCodeService.Query(parameters, processId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetDefectCodeDetailList(GridParameter parameters, string defectCodeId)
        {
            return Json(_defectCodeDetailService.Query(parameters, defectCodeId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetDefectCode()
        {
            return Json(_defectCodeService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDefectCodeById(string id)
        {
            return Json(_defectCodeService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(DefectCode defectCode, IEnumerable<DefectCodeDetail> defectCodeDetail)
        {
            _defectCodeService.Insert(defectCode, defectCodeDetail);
            return Json(new { DefectCode = defectCode, Message = AplosMessage.Insert });
        }
        
        [HttpPost]
        public JsonResult Edit(DefectCode defectCode, IEnumerable<DefectCodeDetail> defectCodeDetail, string[] deletedItems)
        {
            _defectCodeService.Update(defectCode, defectCodeDetail, deletedItems);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _defectCodeService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
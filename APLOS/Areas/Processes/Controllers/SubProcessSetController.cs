#region Using
using Aplos.Controllers;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class SubProcessSetController : BaseController
    {
        #region --Constructor
        private readonly ISubProcessSetService _subprocessSetService;
        private readonly ISubProcessSetDetailService _subprocessSetDetailService;

        public SubProcessSetController(
             ISubProcessSetService subprocessSetService
           , ISubProcessSetDetailService subprocessSetDetailService
            )
        {
            _subprocessSetService = subprocessSetService;
            _subprocessSetDetailService = subprocessSetDetailService;
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

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string entityId)
        {
            return Json(_subprocessSetService.Query(parameters, entityId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Process set detail by process set id.
        /// </summary>
        /// <param name="subprocessSetId"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult GetSubProcessSetDetailList(string subprocessSetId)
        {
            return Json(_subprocessSetDetailService.Query(subprocessSetId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(SubProcessSet subProcessSet, IEnumerable<SubProcessSetDetail> subProcessSetDetail)
        {
            _subprocessSetService.InsertGraph(subProcessSet, subProcessSetDetail);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SubProcessSet subProcessSet, IEnumerable<SubProcessSetDetail> subProcessSetDetail)
        {
            _subprocessSetService.UpdateGraph(subProcessSet, subProcessSetDetail);
            return Json(new { Message = AplosMessage.Updated });
        }
        public ActionResult Delete(string id)
        {
            _subprocessSetService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
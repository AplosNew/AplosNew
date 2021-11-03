#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.WorkCenters;
using Library.Service.WorkCenters;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.WorkCenters.Controllers
{
    public class WorkCenterBuyerTagController : BaseController
    {
        #region Constructor

        private readonly IWorkCenterBuyerTagService _workCenterBuyerTagService;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        /// <param name="workCenterBuyerTagService">    The work center category service. </param>
        /// <param name="companyService">               The company service. </param>
        ///-------------------------------------------------------------------------------------------------

        public WorkCenterBuyerTagController(IWorkCenterBuyerTagService workCenterBuyerTagService)
        {
            this._workCenterBuyerTagService = workCenterBuyerTagService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string plantId, string unitId)
        {
            return Json(_workCenterBuyerTagService.Query(parameters, plantId, unitId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetWorkCenterBuyerTagById(string id)
        {
            return Json(_workCenterBuyerTagService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(WorkCenterBuyerTag workCenterBuyerTag)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            workCenterBuyerTag.CompanyGroupId = identity.CompanyGroupId;
            _workCenterBuyerTagService.Insert(workCenterBuyerTag);
            return Json(new { WorkCenterBuyerTag = workCenterBuyerTag, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(WorkCenterBuyerTag workCenterBuyerTag)
        {
            _workCenterBuyerTagService.Update(workCenterBuyerTag);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _workCenterBuyerTagService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}
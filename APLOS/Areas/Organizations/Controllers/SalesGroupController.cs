#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class SalesGroupController : BaseController
    {
        #region Constructor

        private readonly ISalesGroupService _salesGroupService;

        public SalesGroupController(ISalesGroupService salesGroupService)
        {
            _salesGroupService = salesGroupService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string salesorganisationid)
        {
            return Json(_salesGroupService.GetCbo(salesorganisationid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_salesGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string salesOrganizationId)
        {
            return Json(_salesGroupService.Query(parameters, salesOrganizationId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult SearchSalesGroupList(GridParameter parameters, string salesOrganizationId, string salesGroupIds)
        {
            return Json(_salesGroupService.SearchSalesGroupList(parameters, salesOrganizationId, new JavaScriptSerializer().Deserialize<string[]>(salesGroupIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SalesGroup salesGroup)
        {
            _salesGroupService.Insert(salesGroup);
            return Json(new { SalesGroup = salesGroup, Sequence = _salesGroupService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SalesGroup salesGroup)
        {
            _salesGroupService.Update(salesGroup);
            return Json(new { Sequence = _salesGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _salesGroupService.Delete(id);
            return Json(new { Sequence = _salesGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
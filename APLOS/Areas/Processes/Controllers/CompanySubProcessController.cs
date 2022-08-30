#region Using
using Aplos.Controllers;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class CompanySubProcessController : BaseController
    {
        #region Constructor
        private readonly ICompanySubProcessService _companySubProcessService;
        public CompanySubProcessController(ICompanySubProcessService companySubProcessService)
        {
            _companySubProcessService = companySubProcessService;
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

        [Authorize]
        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string companyId, string processId)
        {
            return Json(_companySubProcessService.Query(parameters, companyId, processId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetCbo(string processid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companySubProcessService.GetCbo(processid,identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbobyprocessid(string processid,string companyId)
        {
            return Json(_companySubProcessService.GetCbo(processid, companyId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetSubProcessList(GridParameter parameters, string companyId, string processId)
        {
            return Json(_companySubProcessService.Query(parameters, companyId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanySubProcess> companySubProcess, string ids)
        {
            _companySubProcessService.Insert(companySubProcess, new JavaScriptSerializer().Deserialize<string[]>(ids));
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _companySubProcessService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
#region Using
using Aplos.Controllers;
using Library.Core;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;

using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
	public class SubProcessController : BaseController
	{
		#region --Constructor
		private readonly ISubProcessService _subProcessService;

		public SubProcessController(ISubProcessService subProcessService)
		{
			this._subProcessService = subProcessService;
		}
		#endregion

		#region dll

		[Authorize]
		public JsonResult GetCbo(string processId)
		{
			return Json(_subProcessService.GetCbo(processId), JsonRequestBehavior.AllowGet);
		}

		#endregion

		#region -- Pages
		public ActionResult Aplos()
		{
			return View();
		}
		#endregion

		#region -- Operations
		[HttpGet]
		public JsonResult GetAutoSequence(string processId)
		{
			return Json(_subProcessService.GetAutoSequence(processId), JsonRequestBehavior.AllowGet);
		}
		[Authorize]
		[HttpGet]
		public JsonResult GetList(GridParameter parameters, string processId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_subProcessService.Query(parameters, processId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetListForCompanySubProcess(GridParameter parameters, string companyId, string processId, string subProcessIds)
		{
			return Json(_subProcessService.GetListForCompanySubProcess(parameters, companyId, processId, new JavaScriptSerializer().Deserialize<string[]>(subProcessIds)), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult GetListSubProcess(GridParameter parameters, string companyId, string processId, string subProcessIds)
		{
			return Json(_subProcessService.GetListSubProcess(parameters, companyId, processId, new JavaScriptSerializer().Deserialize<string[]>(subProcessIds)), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetSubProcessListByProductionProcess(GridParameter parameters, string processId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_subProcessService.GetSubProcessListByProductionProcess(parameters, identity.CompanyGroupId, processId), JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult Create(SubProcess subProcess)
		{
			_subProcessService.Insert(subProcess);
			return Json(new { SubProcess = subProcess, Sequence = _subProcessService.GetAutoSequence(subProcess.ProcessId), Message = AplosMessage.Insert });
		}
		[HttpPost]
		public JsonResult Edit(SubProcess subProcess)
		{
			_subProcessService.Update(subProcess);
			return Json(new { Sequence = _subProcessService.GetAutoSequence(subProcess.ProcessId), Message = AplosMessage.Updated });
		}

		public ActionResult Delete(string id)
		{
			var data = _subProcessService.Find(id);
			_subProcessService.Delete(id);
			return Json(new { Sequence = _subProcessService.GetAutoSequence(data.ProcessId), Message = AplosMessage.Deleted });
		}
		#endregion
	}
}
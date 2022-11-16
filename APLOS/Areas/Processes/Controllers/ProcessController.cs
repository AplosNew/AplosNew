#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Processes;
using Library.Service.Processes;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Processes.Controllers
{
	public class ProcessController : BaseController
	{
		#region --Constructor

		private readonly IProcessService _processService;

		public ProcessController(IProcessService processService)
		{
			_processService = processService;
		}

		#endregion --Constructor

		#region dll

		[Authorize, HttpGet]
		public JsonResult GetCboByIsValueAdded()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_processService.GetCboByIsValueAdded(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetCbo()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_processService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetProductionProcessCbo()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_processService.GetProductionProcessCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}

		#endregion dll

		#region  Pages

		public ActionResult Aplos()
		{
			return View();
		}

		#endregion -- Pages

		#region -- Operations

		[HttpGet, Authorize]
		public JsonResult GetAutoSequence()
		{
			return Json(_processService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult GetLoadProcessWithSubProcess(GridParameter parameters, string companyGroupId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_processService.GetLoadProcessWithSubProcess(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetList(GridParameter parameters, string processId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_processService.Query(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(processId)), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]

		public JsonResult GetProductionProcessList(GridParameter parameters, string productionOrderId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_processService.GetProductionProcessList(parameters, identity.CompanyGroupId, identity.CompanyId, productionOrderId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(Process process)
		{
			_processService.Insert(process);
			return Json(new { Process = process, Sequence = _processService.GetAutoSequence(), Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult Edit(Process process)
		{
			_processService.Update(process);
			return Json(new { Sequence = _processService.GetAutoSequence(), Message = AplosMessage.Updated });
		}

		[HttpPost]
		public ActionResult Delete(string id)
		{
			_processService.DeleteGraph(id);
			return Json(new { Sequence = _processService.GetAutoSequence(), Message = AplosMessage.Deleted });
		}

		#endregion -- Operations
	}
}
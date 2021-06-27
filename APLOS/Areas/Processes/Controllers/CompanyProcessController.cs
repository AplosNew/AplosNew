using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Processes;
using Library.Service.Processes;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Processes.Controllers
{
	public class CompanyProcessController : BaseController
	{
		#region Constructor

		private readonly ICompanyProcessService _companyProcessService;

		public CompanyProcessController(ICompanyProcessService companyProcessService)
		{
			_companyProcessService = companyProcessService;
		}

		#endregion Constructor


		[Authorize]
		public ActionResult Aplos()
		{
			return View();
		}

		#region -- Operations

		[Authorize, HttpGet]
		public JsonResult GetCompanyProductionProcessCbo(string companyId)
		{
			return Json(_companyProcessService.GetCompanyProductionProcessCbo(companyId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetCompanyProcessCbo(string companyId)
		{
			if (string.IsNullOrEmpty(companyId))
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				companyId = identity.CompanyId;
			}
			return Json(_companyProcessService.GetCompanyProcessCbo(companyId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetList(GridParameter parameters, string companyId)
		{
			return Json(_companyProcessService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// use in bulletin, productDefinition
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="processIds"></param>
		/// <returns></returns>
		[HttpGet, Authorize]
		public ActionResult GetCompanyProductionProcessList(GridParameter parameters, string processIds)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_companyProcessService.GetCompanyProductionProcessList(parameters, identity.CompanyGroupId, identity.CompanyId, new JavaScriptSerializer().Deserialize<string[]>(processIds)), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetCompanyProcessList(GridParameter parameters, string companyId, string processIds)
		{
			if (string.IsNullOrEmpty(companyId))
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				companyId = identity.CompanyId;
			}
			return Json(_companyProcessService.GetCompanyProcessList(parameters, companyId, new JavaScriptSerializer().Deserialize<string[]>(processIds)), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(IEnumerable<CompanyProcess> entities)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			_companyProcessService.InsertUpdateOrDeleteGraph(entities, identity.CompanyGroupId);
			return Json(new { Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult Delete(string id)
		{
			_companyProcessService.Delete(id);
			return Json(new { Message = AplosMessage.Deleted });
		}

		#endregion -- Operations
	}
}
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using System.Threading;
using System.Web.Mvc;
using System.Collections.Generic;
using Library.HumanResource.Payroll.Tax;

namespace Aplos.Areas.HumanResource.Controllers
{
	public class CompliedShiftGroupingController : BaseController
	{
		#region Constructor

		private readonly ICompliedShiftGroupingService _compliedShiftGroupingService;

		public CompliedShiftGroupingController(
			  ICompliedShiftGroupingService compliedShiftGroupingService
			)
		{
			_compliedShiftGroupingService = compliedShiftGroupingService;
		}

		#endregion Constructor

		#region -- Pages


		public ActionResult Aplos()
		{
			return View();
		}

		#endregion -- Pages

		#region -- Operations

		[AllowAnonymous]
		public JsonResult GetCbo(string plantId)
		{
			if (string.IsNullOrEmpty(plantId))
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				plantId = identity.PlantId;
			}
			return Json(new SelectList(_compliedShiftGroupingService.GetCbo(plantId), "Value", "Text"), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetDetailList(string compliedShiftGroupId)
		{
			return Json(_compliedShiftGroupingService.QueryDetail(compliedShiftGroupId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetList(GridParameter parameters, string companyGroupId, string plantId)
		{
			return Json(_compliedShiftGroupingService.Query(parameters, companyGroupId, plantId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public ActionResult QueryshiftDefination(GridParameter parameters, string groupId, string plantId)
		{
			return Json(_compliedShiftGroupingService.QueryshiftDefination(parameters, groupId, plantId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(CompliedShiftGrouping compliedShiftGrouping, IEnumerable<CompliedShiftGroupDetail> details)
		{
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //cIncomeTaxCalculation cIncomeTaxCalculation = new cIncomeTaxCalculation("1", identity.PlantId);
            //cIncomeTaxCalculation.Calculate();
            _compliedShiftGroupingService.InsertOrUpdateGraph(compliedShiftGrouping, details);
            return Json(new { CompliedShiftGrouping = compliedShiftGrouping, Message = AplosMessage.Success });
		}

		[HttpPost]
		public JsonResult Edit(CompliedShiftGrouping compliedShiftGrouping)
		{
			_compliedShiftGroupingService.Update(compliedShiftGrouping);
			return Json(new { Message = AplosMessage.Updated });
		}

		public ActionResult Delete(string id)
		{
			_compliedShiftGroupingService.DeleteGraph(id);
			return Json(new { Message = AplosMessage.Deleted });
		}
		public ActionResult DeleteDetails(string id)
		{
			_compliedShiftGroupingService.DeleteDetail(id);
			return Json(new { Message = AplosMessage.Deleted });
		}
		#endregion -- Operations
	}
}
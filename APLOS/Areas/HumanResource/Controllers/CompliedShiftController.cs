using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
	public class CompliedShiftController : BaseController
	{
		#region Constructor

		private readonly ICompliedShiftService _compliedShiftService;

		public CompliedShiftController(
			  ICompliedShiftService compliedShiftService
			)
		{
			_compliedShiftService = compliedShiftService;
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
			return Json(new SelectList(_compliedShiftService.GetCbo(plantId), "Value", "Text"), JsonRequestBehavior.AllowGet);
		}

		[HttpGet,Authorize]
		public ActionResult GetList(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_compliedShiftService.Query(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(CompliedShift entity)
		{
			_compliedShiftService.Insert(entity);
			return Json(new { entity, Message = AplosMessage.Success });
		}

		[HttpPost]
		public JsonResult Edit(CompliedShift entity)
		{
			_compliedShiftService.Update(entity);
			return Json(new { Message = AplosMessage.Updated });
		}

		public ActionResult Delete(string id)
		{
			_compliedShiftService.Delete(id);
			return Json(new { Message = AplosMessage.Deleted });
		}

		#endregion -- Operations
	}
}
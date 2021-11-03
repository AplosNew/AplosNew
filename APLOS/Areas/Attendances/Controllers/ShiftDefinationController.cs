using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Service.Attendances;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
	public class ShiftDefinationController : BaseController
	{
		#region Constructor

		private readonly IShiftDefinationService _shiftDefinationService;

		public ShiftDefinationController(IShiftDefinationService shiftDefinationService)
		{
			_shiftDefinationService = shiftDefinationService;
		}

		#endregion Constructor

		[HttpGet, Authorize]
		public JsonResult GetCbo(string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_shiftDefinationService.GetCboByPlant(plantId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetCboByPlant(string plantId)
		{
			return Json(_shiftDefinationService.GetCboByPlant(plantId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetEntityPlantShiftCbo(string entityId)
		{
			return Json(_shiftDefinationService.GetCboByEntity(entityId), JsonRequestBehavior.AllowGet);
		}
	}
}
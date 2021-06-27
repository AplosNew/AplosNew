#region Using
using Library.Core;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Service.Materials;

using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
	public class CharacteristicsController : Controller
	{
		#region Constructor
		/// <summary>   The skillCategoryService service. </summary>
		private readonly ICharacteristicsService _characteristicsService;

		public CharacteristicsController(ICharacteristicsService characteristicsservice)
		{
			this._characteristicsService = characteristicsservice;
		}
		#endregion

		#region -- Pages
		/// <summary>
		/// Indexes this instance.
		/// </summary>
		public ActionResult Aplos()
		{
			return View();
		}
		#endregion

		#region -- Operations
		[HttpGet, Authorize]
		public JsonResult GetAutoSequence()
		{
			return Json(_characteristicsService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
		}
		[Authorize]
		public JsonResult GetCbo(string valueAssignment)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_characteristicsService.GetCbo(valueAssignment, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetForCharacteristicsValue(string characteristicsId)
		{
			return Json(_characteristicsService.GetForCharacteristicsValue(characteristicsId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetList(GridParameter parameters)
		{
			return Json(_characteristicsService.GetSearchData(parameters), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetCharacteristicsSearch(GridParameter parameters)
		{
			return Json(_characteristicsService.GetCharacteristicsSearch(parameters), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult GetCharacteristics(string id)
		{
			return Json(_characteristicsService.Find(id), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(Characteristics characteristics)
		{
			_characteristicsService.Insert(characteristics);
			return Json(new { Characteristics = characteristics, Sequence = _characteristicsService.GetAutoSequence(), Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult Edit(Characteristics characteristics)
		{
			_characteristicsService.Update(characteristics);
			return Json(new { Sequence = _characteristicsService.GetAutoSequence(), Message = AplosMessage.Updated });
		}

		public ActionResult Delete(string id)
		{
			_characteristicsService.Archive(id);
			return Json(new { Sequence = _characteristicsService.GetAutoSequence(), Message = AplosMessage.Deleted });
		}
		#endregion
	}
}
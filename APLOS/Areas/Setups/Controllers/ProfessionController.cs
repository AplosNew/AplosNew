#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
	public class ProfessionController : BaseController
	{
		private readonly IProfessionService _professionService;

		public ProfessionController(IProfessionService professionService)
		{
			_professionService = professionService;
		}

		[HttpGet]
		public ActionResult Aplos()
		{
			return View();
		}

		[Authorize]
		public JsonResult GetProfessionCbo()
		{
			return Json(new SelectList(_professionService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult GetAllProfession()
		{
			return Json(_professionService.Query().Select(), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult GetProfessionList(GridParameter parameters)
		{
			return Json(_professionService.Query(parameters), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult GetProfession(string id)
		{
			return Json(_professionService.Find(id), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult GetAutoSequence()
		{
			return Json(_professionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(Profession continent, IEnumerable<LocalLanguage> localLanguages)
		{
			_professionService.Insert(continent, localLanguages);
			return Json(new { Profession = continent, Sequence = _professionService.GetAutoSequence(), Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult Edit(Profession continent, IEnumerable<LocalLanguage> localLanguages)
		{
			_professionService.Update(continent, localLanguages);
			return Json(new { Sequence = _professionService.GetAutoSequence(), Message = AplosMessage.Updated });
		}

		[HttpPost]
		public ActionResult Delete(string id)
		{
			_professionService.Delete(id);
			return Json(new { Sequence = _professionService.GetAutoSequence(), Message = AplosMessage.Deleted });
		}
	}
}
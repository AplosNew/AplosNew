#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
	public class RelationshipController : BaseController
	{
		private readonly IRelationshipService _relationshipService;

		public RelationshipController(IRelationshipService relationshipService)
		{
			_relationshipService = relationshipService;
		}

		[HttpGet]
		public ActionResult Aplos()
		{
			return View();
		}
		[Authorize]
		public JsonResult GetRelationshipCbo()
		{
			return Json(new SelectList(_relationshipService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult GetAllRelationship()
		{
			return Json(_relationshipService.Query().Select(), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult GetRelationshipList(GridParameter parameters)
		{
			return Json(_relationshipService.Query(parameters), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult GetRelationship(string id)
		{
			return Json(_relationshipService.Find(id), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult GetAutoSequence()
		{
			return Json(_relationshipService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(Relationship continent, IEnumerable<LocalLanguage> localLanguages)
		{
			_relationshipService.Insert(continent, localLanguages);
			return Json(new { Relationship = continent, Sequence = _relationshipService.GetAutoSequence(), Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult Edit(Relationship continent, IEnumerable<LocalLanguage> localLanguages)
		{
			_relationshipService.Update(continent, localLanguages);
			return Json(new { Sequence = _relationshipService.GetAutoSequence(), Message = AplosMessage.Updated });
		}

		[HttpPost]
		public ActionResult Delete(string id)
		{
			_relationshipService.Delete(id);
			return Json(new { Sequence = _relationshipService.GetAutoSequence(), Message = AplosMessage.Deleted });
		}
	}
}
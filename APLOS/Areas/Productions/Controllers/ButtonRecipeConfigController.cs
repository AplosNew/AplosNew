#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
	public class ButtonRecipeConfigController : BaseController
	{
		#region Constructor
		private readonly IButtonRecipeConfigService _btnRecipeConfigService;

		public ButtonRecipeConfigController(IButtonRecipeConfigService btnRecipeConfigService)
		{
			_btnRecipeConfigService = btnRecipeConfigService;
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

		[HttpGet, Authorize]
		public ActionResult GetList(GridParameter parameters, string plantId)
		{
			return Json(_btnRecipeConfigService.Query(parameters, plantId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(ButtonRecipeConfig entity)
		{
			_btnRecipeConfigService.Insert(entity);
			return Json(new { entity, Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult Edit(ButtonRecipeConfig entity)
		{
			_btnRecipeConfigService.Update(entity);
			return Json(new { Message = AplosMessage.Updated });
		}

		public ActionResult Delete(string id)
		{
			_btnRecipeConfigService.Delete(id);
			return Json(new { Message = AplosMessage.Deleted });
		}
		#endregion
	}
}
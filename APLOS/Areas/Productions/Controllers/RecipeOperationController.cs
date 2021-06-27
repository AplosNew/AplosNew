using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Productions;
using Library.Service.Productions;
using System.Web.Mvc;

namespace Aplos.Areas.Productions.Controllers
{
    public class RecipeOperationController : BaseController
    {
        #region Constructor

        private readonly IRecipeOperationService _recipeOperationService;

        public RecipeOperationController(
              IRecipeOperationService recipeOperationService
            )
        {
            _recipeOperationService = recipeOperationService;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_recipeOperationService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_recipeOperationService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(RecipeOperation model)
        {
            _recipeOperationService.Insert(model);
            return Json(new { RecipeOperation = model, Sequence = _recipeOperationService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(RecipeOperation model)
        {
            _recipeOperationService.Update(model);
            return Json(new { Sequence = _recipeOperationService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _recipeOperationService.Delete(id);
            return Json(new { Sequence = _recipeOperationService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
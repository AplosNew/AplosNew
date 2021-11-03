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
    public class RecipeProductionOrderController : BaseController
    {
        #region Constructor

        private readonly IRecipeProductionOrderService _baseService;

        public RecipeProductionOrderController(IRecipeProductionOrderService baseService)
        {
            _baseService = baseService;
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

        //[HttpGet, Authorize]
        //public ActionResult GetList(GridParameter parameters)
        //{
        //    return Json(_companyGroupRecipeProductionOrderService.Query(parameters), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult Create(RecipeProductionOrder dMM)
        {
            _baseService.Insert(dMM);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(RecipeProductionOrder dMM)
        {
            _baseService.Update(dMM);
            return Json(new { Message = AplosMessage.Success });
        }

        public ActionResult Delete(string id)
        {
            _baseService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion
    }
}
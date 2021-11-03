#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class PlanningTypesController : BaseController
    {
        #region Constructor
        /// <summary>   The PlanningTypesService service. </summary>
        private readonly IPlanningTypesService _planningTypesService;
        private readonly ISqlRepository _sqlRepository;
        public PlanningTypesController(IPlanningTypesService planningTypesService, ISqlRepository R)
        {
            _planningTypesService = planningTypesService;
            _sqlRepository = R;
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

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT PlanningType AS [Value], UserName AS [Text] FROM [dbo].[PlanningTypes]"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_planningTypesService.Query(parameters), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(PlanningTypes planningTypes)
        {
            _planningTypesService.Insert(planningTypes);
            return Json(new { PlanningTypes= planningTypes, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PlanningTypes planningTypes)
        {
            _planningTypesService.Update(planningTypes);
            return Json(new {Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _planningTypesService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
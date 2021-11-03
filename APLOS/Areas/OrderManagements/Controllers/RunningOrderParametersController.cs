#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.OrderManagements;
using Library.Model.Setups;
using Library.Service.OrderManagements;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class RunningOrderParametersController : BaseController
    {
        #region Constructor

        private readonly IRunningOrderParametersService _RunningOrderParametersService;
        private object runningOrderParameters;

        public RunningOrderParametersController(
              IRunningOrderParametersService RunningOrderParametersService
            )
        {
            _RunningOrderParametersService = RunningOrderParametersService;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        
        [HttpGet, Authorize]
        public ActionResult GetList(string PlantId)
        {
            return Json(_RunningOrderParametersService.Query(PlantId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult Create(RunningOrderParameter runningOrderParameters)
        {
            _RunningOrderParametersService.Insert(runningOrderParameters);
            return Json(new { RunningOrderParameter = runningOrderParameters, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult Edit(RunningOrderParameter RunningOrderParameters)
        {
            _RunningOrderParametersService.Update(RunningOrderParameters);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _RunningOrderParametersService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}
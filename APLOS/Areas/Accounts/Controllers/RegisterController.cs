using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class RegisterController : BaseController
    {
        private readonly IRegisterService _registerService;

        public RegisterController(
            IRegisterService registerService)
        {
            _registerService = registerService;
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_registerService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/Register.cshtml");
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_registerService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_registerService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_registerService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Register entity)
        {
            _registerService.Insert(entity);
            return Json(new { ModelData = entity, Sequence = _registerService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Register entity)
        {
            _registerService.Update(entity);
            return Json(new { Sequence = _registerService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _registerService.Delete(id);
            return Json(new { Sequence = _registerService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
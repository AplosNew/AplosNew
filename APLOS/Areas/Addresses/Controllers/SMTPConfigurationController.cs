using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Addresses;
using Library.Service.Addresses;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class SMTPConfigurationController : BaseController
    {
        private readonly ISMTPConfigurationService _smtpConfigurationService;

        public SMTPConfigurationController(ISMTPConfigurationService smtpConfigurationService)
        {
            _smtpConfigurationService = smtpConfigurationService;
        }

        [HttpGet]
        public ActionResult SMTPConfiguration()
        {
            return View("~/Areas/Addresses/Views/SMTPConfiguration.cshtml");
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters, string companyGroupId)
        {
            return Json(_smtpConfigurationService.Query(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SMTPConfiguration entity)
        {
            _smtpConfigurationService.Insert(entity);
            return Json(new { SMTPConfiguration = entity, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SMTPConfiguration entity)
        {
            _smtpConfigurationService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _smtpConfigurationService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}
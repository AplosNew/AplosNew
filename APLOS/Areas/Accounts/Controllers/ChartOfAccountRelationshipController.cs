using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class ChartOfAccountRelationshipController : BaseController
    {
        private readonly IChartOfAccountRelationshipService _chartOfAccountRelationshipService;

        public ChartOfAccountRelationshipController(IChartOfAccountRelationshipService chartOfAccountRelationshipService)
        {
            _chartOfAccountRelationshipService = chartOfAccountRelationshipService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        public JsonResult GetChartOfAccountRelationshipCbo()
        {
            return Json(new SelectList(_chartOfAccountRelationshipService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetChartOfAccountRelationshipList(GridParameter parameters, string coaid)
        {
            return Json(_chartOfAccountRelationshipService.Query(parameters, coaid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChartOfAccountRelationship(string id)
        {
            return Json(_chartOfAccountRelationshipService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence(string caoid)
        {
            return Json(_chartOfAccountRelationshipService.GetAutoSequence(caoid), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ChartOfAccountRelationship chartOfAccountRelationship)
        {
            _chartOfAccountRelationshipService.Insert(chartOfAccountRelationship);
            return Json(new { ChartOfAccountRelationship = chartOfAccountRelationship, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ChartOfAccountRelationship chartOfAccountLevel4)
        {
            _chartOfAccountRelationshipService.Update(chartOfAccountLevel4);
            return Json(new { Message = AplosMessage.Success });
        }

        public ActionResult Delete(string id)
        {
            _chartOfAccountRelationshipService.Archive(id);
            return Json(new { Message = AplosMessage.Success });
        }

        public ActionResult GetFormData(GridParameter parameters, string coaid)
        {
            return Json(_chartOfAccountRelationshipService.Query(parameters, coaid), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFormData1(GridParameter parameters)
        {
            return Json(_chartOfAccountRelationshipService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
    }
}
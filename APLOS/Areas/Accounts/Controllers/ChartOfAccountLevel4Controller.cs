using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class ChartOfAccountLevel4Controller : BaseController
    {
        private readonly IChartOfAccountLevel4Service _chartOfAccountLevel4Service;

        public ChartOfAccountLevel4Controller(IChartOfAccountLevel4Service chartOfAccountLevel4Service)
        {
            _chartOfAccountLevel4Service = chartOfAccountLevel4Service;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/ChartOfAccountLevel4.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_chartOfAccountLevel4Service.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChartOfAccountLevel4List(GridParameter parameters)
        {
            return Json(_chartOfAccountLevel4Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckIdUse(string id)
        {
            return Json(_chartOfAccountLevel4Service.CheckUsing(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChartOfAccountLevel4(string id)
        {
            return Json(_chartOfAccountLevel4Service.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_chartOfAccountLevel4Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ChartOfAccountLevel4 chartOfAccountLevel4)
        {
            _chartOfAccountLevel4Service.Insert(chartOfAccountLevel4);
            return Json(new { ChartOfAccountLevel4 = chartOfAccountLevel4, Sequence = _chartOfAccountLevel4Service.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ChartOfAccountLevel4 chartOfAccountLevel4)
        {
            _chartOfAccountLevel4Service.Update(chartOfAccountLevel4);
            return Json(new { Sequence = _chartOfAccountLevel4Service.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _chartOfAccountLevel4Service.Delete(id);
            return Json(new { Sequence = _chartOfAccountLevel4Service.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
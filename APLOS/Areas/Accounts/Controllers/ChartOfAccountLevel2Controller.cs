using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class ChartOfAccountLevel2Controller : BaseController
    {
        private readonly IChartOfAccountLevel2Service _chartOfAccountLevel2Service;

        public ChartOfAccountLevel2Controller(IChartOfAccountLevel2Service chartOfAccountLevel2Service)
        {
            _chartOfAccountLevel2Service = chartOfAccountLevel2Service;
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/ChartOfAccountLevel2.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_chartOfAccountLevel2Service.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetChartOfAccountLevel2List(GridParameter parameters)
        {
            return Json(_chartOfAccountLevel2Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckIdUse(string id)
        {
            return Json(_chartOfAccountLevel2Service.CheckUsing(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChartOfAccountLevel2(string id)
        {
            return Json(_chartOfAccountLevel2Service.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_chartOfAccountLevel2Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ChartOfAccountLevel2 chartOfAccountLevel2)
        {
            _chartOfAccountLevel2Service.Insert(chartOfAccountLevel2);
            return Json(new { ChartOfAccountLevel2 = chartOfAccountLevel2, Sequence = _chartOfAccountLevel2Service.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ChartOfAccountLevel2 chartOfAccountLevel2)
        {
            _chartOfAccountLevel2Service.Update(chartOfAccountLevel2);
            return Json(new { Sequence = _chartOfAccountLevel2Service.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _chartOfAccountLevel2Service.Delete(id);
            return Json(new { Sequence = _chartOfAccountLevel2Service.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
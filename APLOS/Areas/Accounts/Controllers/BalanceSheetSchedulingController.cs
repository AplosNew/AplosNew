using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class BalanceSheetSchedulingController : BaseController
    {
        private readonly IChartOfAccountLevel1Service _chartOfAccountLevel1Service;

        public BalanceSheetSchedulingController(IChartOfAccountLevel1Service chartOfAccountLevel1Service)
        {
            _chartOfAccountLevel1Service = chartOfAccountLevel1Service;
        }

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_chartOfAccountLevel1Service.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetChartOfAccountLevel1List(GridParameter parameters)
        {
            return Json(_chartOfAccountLevel1Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckIdUse(string id)
        {
            return Json(_chartOfAccountLevel1Service.CheckUsing(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_chartOfAccountLevel1Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ChartOfAccountLevel1 chartOfAccountLevel1)
        {
            _chartOfAccountLevel1Service.Insert(chartOfAccountLevel1);
            return Json(new { ChartOfAccountLevel1 = chartOfAccountLevel1, Sequence = _chartOfAccountLevel1Service.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ChartOfAccountLevel1 chartOfAccountLevel1)
        {
            _chartOfAccountLevel1Service.Update(chartOfAccountLevel1);
            return Json(new { Sequence = _chartOfAccountLevel1Service.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _chartOfAccountLevel1Service.Delete(id);
            return Json(new { Sequence = _chartOfAccountLevel1Service.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
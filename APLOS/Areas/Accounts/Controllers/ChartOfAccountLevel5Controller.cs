using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class ChartOfAccountLevel5Controller : BaseController
    {
        private readonly IChartOfAccountLevel5Service _chartOfAccountLevel5Service;

        public ChartOfAccountLevel5Controller(IChartOfAccountLevel5Service chartOfAccountLevel5Service)
        {
            _chartOfAccountLevel5Service = chartOfAccountLevel5Service;
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/ChartOfAccountLevel5.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_chartOfAccountLevel5Service.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetChartOfAccountLevel5List(GridParameter parameters)
        {
            return Json(_chartOfAccountLevel5Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckIdUse(string id)
        {
            return Json(_chartOfAccountLevel5Service.CheckUsing(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChartOfAccountLevel5(string id)
        {
            return Json(_chartOfAccountLevel5Service.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_chartOfAccountLevel5Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ChartOfAccountLevel5 chartOfAccountLevel5)
        {
            _chartOfAccountLevel5Service.Insert(chartOfAccountLevel5);
            return Json(new { ChartOfAccountLevel5 = chartOfAccountLevel5, Sequence = _chartOfAccountLevel5Service.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ChartOfAccountLevel5 chartOfAccountLevel5)
        {
            _chartOfAccountLevel5Service.Update(chartOfAccountLevel5);
            return Json(new { Sequence = _chartOfAccountLevel5Service.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _chartOfAccountLevel5Service.Delete(id);
            return Json(new { Sequence = _chartOfAccountLevel5Service.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
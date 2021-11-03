using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class ChartOfAccountLevel6Controller : BaseController
    {
        private readonly IChartOfAccountLevel6Service _chartOfAccountLevel6Service;

        public ChartOfAccountLevel6Controller(IChartOfAccountLevel6Service chartOfAccountLevel6Service)
        {
            _chartOfAccountLevel6Service = chartOfAccountLevel6Service;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/ChartOfAccountLevel6.cshtml");
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_chartOfAccountLevel6Service.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetChartOfAccountLevel6List(GridParameter parameters)
        {
            return Json(_chartOfAccountLevel6Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckIdUse(string id)
        {
            return Json(_chartOfAccountLevel6Service.CheckUsing(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChartOfAccountLevel6(string id)
        {
            return Json(_chartOfAccountLevel6Service.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_chartOfAccountLevel6Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ChartOfAccountLevel6 chartOfAccountLevel6)
        {
            _chartOfAccountLevel6Service.Insert(chartOfAccountLevel6);
            return Json(new { ChartOfAccountLevel6 = chartOfAccountLevel6, Sequence = _chartOfAccountLevel6Service.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ChartOfAccountLevel6 chartOfAccountLevel6)
        {
            _chartOfAccountLevel6Service.Update(chartOfAccountLevel6);
            return Json(new { Sequence = _chartOfAccountLevel6Service.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _chartOfAccountLevel6Service.Delete(id);
            return Json(new { Sequence = _chartOfAccountLevel6Service.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
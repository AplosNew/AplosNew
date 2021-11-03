using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class ChartOfAccountLevel3Controller : BaseController
    {
        private readonly IChartOfAccountLevel3Service _chartOfAccountLevel3Service;

        public ChartOfAccountLevel3Controller(IChartOfAccountLevel3Service chartOfAccountLevel3Service)
        {
            _chartOfAccountLevel3Service = chartOfAccountLevel3Service;
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/ChartOfAccountLevel3.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_chartOfAccountLevel3Service.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetChartOfAccountLevel3List(GridParameter parameters)
        {
            return Json(_chartOfAccountLevel3Service.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetChartOfAccountLevel3(string id)
        {
            return Json(_chartOfAccountLevel3Service.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CheckIdUse(string id)
        {
            return Json(_chartOfAccountLevel3Service.CheckUsing(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_chartOfAccountLevel3Service.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ChartOfAccountLevel3 chartOfAccountLevel3)
        {
            _chartOfAccountLevel3Service.Insert(chartOfAccountLevel3);
            return Json(new { ChartOfAccountLevel3 = chartOfAccountLevel3, Sequence = _chartOfAccountLevel3Service.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ChartOfAccountLevel3 chartOfAccountLevel3)
        {
            _chartOfAccountLevel3Service.Update(chartOfAccountLevel3);
            return Json(new { Sequence = _chartOfAccountLevel3Service.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _chartOfAccountLevel3Service.Delete(id);
            return Json(new { Sequence = _chartOfAccountLevel3Service.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
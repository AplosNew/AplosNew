using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.Organizations;
using Library.ViewModel.Accounts;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class NewAttdnDashboardController : BaseController
    {
       // private readonly IManpowerBudgetDashboardService na;


        NewAttdnDashboardService na = new NewAttdnDashboardService();

        public NewAttdnDashboardController()
        {
            
        }

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetGroupWiseCompanyList(string date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = na.GroupWiseCompanyList(identity.CompanyGroupId, date);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDrillDownListJSON(string CompanyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(na.DrillDownList(identity.CompanyGroupId,CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCompanyDrillDownListJSON(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(na.CompanyWiseDrillDownList(identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDetailDrillDownTable(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date ,  Dictionary<string,string> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(na.DetailDrillDownTable(ChartColumnList, seq, date, identity.CompanyGroupId , data), JsonRequestBehavior.AllowGet);
        }

    }
}
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

        //public ManpowerBudgetDashboardController(IManpowerBudgetDashboardService hrDashboardService)
        //{
        //    na = hrDashboardService;
        //}
            
       
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

        [HttpPost, Authorize]
        public ActionResult ModalEmployeeSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string status, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(na.ModalGroupWiseEmlpoyeeList(identity.CompanyGroupId, ChartColumnList, seq, status, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult ModalEmployeeDetail(IEnumerable<ChartColumnList> chartColumnList, string companyId, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(na.ModalEmlpoyeeListDetail(chartColumnList, identity.CompanyGroupId, companyId, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(na.ModalEmlpoyeeListDetail(chartColumnList, identity.CompanyGroupId, companyId, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalBudgetSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            JsonResult jsondata =  Json(na.ModalBudgetSummary(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

            //return Json(na.ModalBudgetSummary(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalBudgetDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var jsondata = Json(na.ModalBudgetDetail(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters,identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(na.ModalBudgetDetail(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalExcessSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status,string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(na.ModalExcessSummary(ChartColumnList, identity.CompanyGroupId, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalExcessDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(na.ModalExcessDetail(ChartColumnList, identity.CompanyGroupId, seq, date, status, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalShortSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(na.ModalShortSummary(ChartColumnList, identity.CompanyGroupId, seq, date, status, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalShortDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(na.ModalShortDetail(ChartColumnList, identity.CompanyGroupId, seq, date, status, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult BudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string budgetCode, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(na.BudgetCodeWiseEmpList(ChartColumnList, identity.CompanyGroupId, budgetCode, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult WPBudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string budgetCode)
        {
            return Json(na.WpBudgetCodeWiseEmpList(ChartColumnList, budgetCode), JsonRequestBehavior.AllowGet);
        }

    }
}
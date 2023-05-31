using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.Organizations;
using Library.ViewModel.Accounts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ManpowerBudgetDashboardController : BaseController
    {
        // private readonly IManpowerBudgetDashboardService _hrDashboardService;

        private readonly Library.HumanResource.Dashboard.HRDashboardService _HRDashboard;
        private readonly Library.HumanResource.Dashboard.ManPowerBudgetDashboardService _hrDashboardService;

        public ManpowerBudgetDashboardController()
        {
            _hrDashboardService = new Library.HumanResource.Dashboard.ManPowerBudgetDashboardService();
            _HRDashboard = new Library.HumanResource.Dashboard.HRDashboardService();
        }

        //public ManpowerBudgetDashboardController(IManpowerBudgetDashboardService hrDashboardService)
        //{
        //    _hrDashboardService = hrDashboardService;
        //}
            
       
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetGroupWiseCompanyList(string date, string status, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_hrDashboardService.GroupWiseCompanyList(identity.CompanyGroupId,date, status, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDrillDownListJSON(string CompanyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_HRDashboard.OrgStructureListColList(identity.CompanyGroupId,CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCompanyDrillDownListJSON(string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.CompanyWiseDrillDownList(identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDetailDrillDownTable(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_hrDashboardService.DetailDrillDownTable(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalEmployeeSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string status, string EmplyeeTypeOrCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_hrDashboardService.ModalGroupWiseEmlpoyeeList(identity.CompanyGroupId, ChartColumnList, seq, status, EmplyeeTypeOrCategoryId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult ModalEmployeeDetail(IEnumerable<ChartColumnList> chartColumnList, string companyId, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_hrDashboardService.ModalEmlpoyeeListDetail(chartColumnList, identity.CompanyGroupId, companyId, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_hrDashboardService.ModalEmlpoyeeListDetail(chartColumnList, identity.CompanyGroupId, companyId, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalBudgetSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            JsonResult jsondata =  Json(_hrDashboardService.ModalBudgetSummary(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

            //return Json(_hrDashboardService.ModalBudgetSummary(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalBudgetDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var jsondata = Json(_hrDashboardService.ModalBudgetDetail(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters,identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
            //return Json(_hrDashboardService.ModalBudgetDetail(ChartColumnList, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalExcessSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status,string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.ModalExcessSummary(ChartColumnList, identity.CompanyGroupId, seq, date, status, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalExcessDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.ModalExcessDetail(ChartColumnList, identity.CompanyGroupId, seq, date, status, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalShortSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.ModalShortSummary(ChartColumnList, identity.CompanyGroupId, seq, date, status, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ModalShortDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.ModalShortDetail(ChartColumnList, identity.CompanyGroupId, seq, date, status, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult BudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string budgetCode, string EmplyeeTypeOrCategoryId, GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_hrDashboardService.BudgetCodeWiseEmpList(ChartColumnList, identity.CompanyGroupId, budgetCode, EmplyeeTypeOrCategoryId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult WPBudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string budgetCode)
        {
            return Json(_hrDashboardService.WpBudgetCodeWiseEmpList(ChartColumnList, budgetCode), JsonRequestBehavior.AllowGet);
        }

        public ActionResult OnRoleEmployeeReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;
                    dt.Columns.Add(item);
                }
                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;
                        dr[item] = data[i][item];
                    }
                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = _hrDashboardService.CreateOnRoleEmployeeReportSheet(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult BudgetEmployeeReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;
                    dt.Columns.Add(item);
                }
                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;
                        dr[item] = data[i][item];
                    }
                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = _hrDashboardService.CreateBudgetEmployeeReportReportSheet(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult ShortEmployeeReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;
                    dt.Columns.Add(item);
                }
                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;
                        dr[item] = data[i][item];
                    }
                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = _hrDashboardService.CreateShortEmployeeReportReportSheet(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult ExcessEmployeeReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;
                    dt.Columns.Add(item);
                }
                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;
                        dr[item] = data[i][item];
                    }
                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = _hrDashboardService.CreateExcessEmployeeReportReportSheet(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
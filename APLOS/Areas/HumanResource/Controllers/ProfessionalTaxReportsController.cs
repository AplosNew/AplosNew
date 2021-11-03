using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using Library.HumanResource.Payroll.Report;
using Newtonsoft.Json;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ProfessionalTaxReportsController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public ProfessionalTaxReportsController(
              IAttendanceManagementService AttendanceManagementService, IEmployeeProfileService employeeProfileService, ISqlRepository R
            )
        {
            _AttendanceManagementService = AttendanceManagementService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region Get Professional Tax Reports
        [HttpPost, Authorize]
        public ActionResult GetProfessionalTaxReports(string yearId,string dateRange, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                ProfessionalTaxReport pft = new ProfessionalTaxReport();
                IWorkbook workbook = pft.GetProfessionalTaxReport(yearId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name,fromDate,toDate);

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "ProfessionalTaxReport.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetMonthCbo(string yearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {


                string strsql2 = @"SELECT *      
                                      FROM [SCS].[TaxYear] where TaxYearName = '" + yearId + @"'";
                DataTable dtMonth = _sqlRepository.GetDataTable(strsql2);

                DateTime startDate = Convert.ToDateTime(dtMonth.Rows[0]["StartDate"].ToString());
                DateTime endDate = Convert.ToDateTime(dtMonth.Rows[0]["EndDate"].ToString());
                DataTable dtNew = new DataTable();
                dtNew.Columns.Add("MonthName", typeof(string));
                dtNew.Columns.Add("MonthNo", typeof(string));
                dtNew.Columns.Add("Year", typeof(string));
                dtNew.Columns.Add("StartDate", typeof(string));

                DataRow dr = null;

                for(int i =0; i <12; i ++)
                {
                    dr = dtNew.NewRow();
                    dr["MonthName"] = startDate.ToString("MMMM") +"-"+ startDate.ToString("yy"); 
                    dr["MonthNo"] = startDate.ToString("MM");
                    dr["Year"] = startDate.ToString("yyyy");
                    dr["StartDate"] = "1-"+ startDate.ToString("MMM")+ "-"+startDate.ToString("yyyy");


                    dtNew.Rows.Add(dr);
                    startDate = startDate.AddMonths(1);
                }            
                    return Json(JsonConvert.SerializeObject(dtNew), JsonRequestBehavior.AllowGet);
            }


            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }



        [HttpGet, Authorize]
        public ActionResult GetFromToDateCbo(string yearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {                
                string strsql2 = @"SELECT Format(Convert(date,StartDate),'dd-MMM-yyyy') StartDate ,Format(Convert(date,EndDate),'dd-MMM-yyyy') EndDate         
                                      FROM [SCS].[TaxYear] where TaxYearName = '" + yearId + @"'";
               
                return Json(_sqlRepository.GetDataCollection(strsql2), JsonRequestBehavior.AllowGet);
            }


            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        #endregion



    }
}


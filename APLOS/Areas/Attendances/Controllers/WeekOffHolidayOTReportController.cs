using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Report.OT;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Library.HumanResource.Report.OT;
namespace Aplos.Areas.Attendances.Controllers
{
    public class WeekOffHolidayOTReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMonthlyAttendanceInformation _monthlyAttendanceInformation;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;
        private object workbook;
        private object objRpt;
        private object excelEngine;
        private object application;

        public WeekOffHolidayOTReportController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository,
            IMonthlyAttendanceInformation monthlyAttendanceInformation
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _monthlyAttendanceInformation = monthlyAttendanceInformation;
        }

        #endregion Constructor

        #region -- Pages
        
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult HolidayOT()
        {
            return View();
        }



        #endregion -- Pages


        #region -- Operations






        [HttpPost, Authorize]
        public ActionResult GetMonthWiseWeekExtraOTReport(string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                WeekOFFandHolidayOT clsWeekOFFOTReport = new WeekOFFandHolidayOT();
                var fileName = "WeekOFFOT" + DateTime.Now.ToString("yyMMdd") + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                //GetWeekOFFExtraOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
                var workbook = clsWeekOFFOTReport.GetWeekOFFExtraOT(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, Month, Year,  parameters,  isActive,  isSeperated,  isMaternity);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }

            catch (Exception ex)
            {
                throw ex;
            }

        }


        [HttpPost, Authorize]
        public ActionResult GetMonthWiseHolidayExtraOTReport(string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                WeekOFFandHolidayOT clsWeekOFFOTReport = new WeekOFFandHolidayOT();
                var fileName = "HolidayOT" + DateTime.Now.ToString("yyMMdd") + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                //GetWeekOFFExtraOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
                var workbook = clsWeekOFFOTReport.GetholidayExtraOT(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, Month, Year, parameters, isActive, isSeperated, isMaternity);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }

            catch (Exception ex)
            {
                throw ex;
            }

        }



        [HttpPost, Authorize]
        public ActionResult GetEmpInfoSalaryPorcessed(string effectiveDate, string month ,string year, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            WeekOFFandHolidayOT clsWeekOFFOTReport = new WeekOFFandHolidayOT();
            var jsondata = Json(clsWeekOFFOTReport.GetEmpInfoDateRange(identity.CompanyGroupId, identity.PlantId, month, year,  identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }



        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,YearNo FROM YearlyCalendar WHERE PlantId='" + identity.PlantId + "'  ORDER BY YearNo DESC ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        class OTReport
        {
            public decimal TotalOTHr { get; set; }
            public string EmployeeCode { get; set; }
            public DateTime workdate { get; set; }

        }

       
        


        #endregion -- Operations  
    }

  
}
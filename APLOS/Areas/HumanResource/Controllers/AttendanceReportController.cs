using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.EmployeeServices;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AttendanceReportController : BaseController
    {
        #region Constructor

        private readonly IAttdnProcessDataService _AttendanceProcessDataService;
        AnnualLeaveProcessingService alp = new AnnualLeaveProcessingService();
        public AttendanceReportController(
              IAttdnProcessDataService workGroupService
            )
        {
            _AttendanceProcessDataService = workGroupService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Attend()
        {
            return View();
        }
        public ActionResult Report()
        {
            return View();
        }
        #endregion -- Pages


        //[HttpGet, Authorize]
        //public ActionResult AttndReport( string fromDate, string toDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var fileName = DateTime.Now.ToString("yy-MM-dd") + " " + "Employee Attendance Report";
        //    var workbook = _AttendanceProcessDataService.AttndReport(fromDate, toDate, identity.CompanyGroupId ,identity.CompanyId ,identity.PlantId);
        //    workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
        //    return null;
        //}


        [HttpGet, Authorize]
        public ActionResult GetEmployeeSingleData(string fromdate, string todate, string empId)
        {
            var jsondata = Json(alp.GetEmployeeSingleData(fromdate, todate, empId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult EmployeeAttendanceDataXls(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                        continue;

                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        if (item.ToUpper().Contains("ID") || item.ToUpper().Contains("PK") || item.ToUpper().Contains("EJVALUE"))
                            continue;

                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }
                string fileName = "";
                fileName = alp.EmployeeAttendanceReport(dt, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
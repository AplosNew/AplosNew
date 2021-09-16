using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Syncfusion.DocIO.DLS;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class NewAttendanceProcessAuditReportController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public NewAttendanceProcessAuditReportController(
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

      
        #region Audit Report
        [HttpGet, Authorize]
        public ActionResult GetManualOutTimeDateWiseReport(ReportFormat reportFormat, string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetManualOutTimeDateWiseReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId,identity.PlantName,FromDate,ToDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Attendance Audit Data";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcelx(workbook, reportFileName);


                    default:
                        return RenderReportAsExcelx(workbook, reportFileName);

                }
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
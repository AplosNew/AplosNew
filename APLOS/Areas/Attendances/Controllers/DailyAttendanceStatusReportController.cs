#region Using
using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class DailyAttendanceStatusReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        private readonly IMailSenderService _mailSenderService;
        public DailyAttendanceStatusReportController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository,
              IMailSenderService mailSenderService
            )
        {
            _stoppageService = stoppageService;
            _sqlRepository = sqlRepository;
            _mailSenderService = mailSenderService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        //public string GetDailyAttendanceEmpInfoList(string companyGroupId, string plantId, string SheetHeader, string SheetName)

        [HttpPost]
        public JsonResult GetDailyAttendanceStatusReport(string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec)
        {
            

            string fileName = "";
            shift = "'" + shift.Replace(" ", "','") + "'";//replaced with ""
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            fileName = _mailSenderService.GetDailyAttendanceEmpInfoList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Daily Attendance Status", "", workDate, shift, Entity, Dept, Ydate, Sec, SSec,"","","","");
            return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDailyAttendanceStatusReportView(string workDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _mailSenderService.GetDailyAttendanceDataForView(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Daily Attendance Status", "", workDate);
            //workbook.Version = ExcelVersion.Excel2016;
            //workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);

            return RenderReportAsPdf(workbook, "DailyAttendance");
        }


        [HttpGet , Authorize]
        public JsonResult GetGrid()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var str = @"Select distinct isnull(E.UserName, '') as Entity, dep.UserName as Department, sec.UserName as Section, ssec.UserName as SubSection,
                        E.Id as EntityId , dep.Id as DepId , sec.Id as SecId , ssec.Id as SubSecId
                        from org.Position p
                        left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
                        left join org.Entity e on e.Id = mpb.EntityId
                        left join org.Section sec on sec.id = p.SectionId
                        left join org.SubSection ssec on ssec.Id = p.SubSectionId
                        left join org.Department dep on dep.Id = p.DepartmentId
                        where e.PlantId='"+identity.PlantId+@"'";
            return Json(_sqlRepository.GetDataCollection(str),JsonRequestBehavior.AllowGet);
        }

    }
}
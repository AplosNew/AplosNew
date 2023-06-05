using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Biometrics;
using Library.Model.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class MaternityLeaveTransactionController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeaveTransactionService _maternityLeaveTransactionService;
        private object companyGroupId;

        public MaternityLeaveTransactionController(
              IMaternityLeaveTransactionService maternityLeaveTransactionService
            , ISqlRepository sqlRepository

            )
        {
            _maternityLeaveTransactionService = maternityLeaveTransactionService;
            _sqlRepository = sqlRepository;

        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize]
        public ActionResult Leave()
        {
            return View();
        }
        [Authorize]
        public ActionResult lvEncash()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations


        [HttpGet, Authorize]
        public JsonResult GetFemaleEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_maternityLeaveTransactionService.GetFemaleEmployee(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(LeaveTransaction maternityLeaveTransaction)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            maternityLeaveTransaction.AddedBy = identity.Name;
            maternityLeaveTransaction.GroupID = identity.CompanyGroupId;
            maternityLeaveTransaction.PlantID = identity.PlantId;
            maternityLeaveTransaction.FirstApprovingStatus = true;
            _maternityLeaveTransactionService.Save(maternityLeaveTransaction);
            return Json(new { MaternityLeaveTransaction = maternityLeaveTransaction, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _maternityLeaveTransactionService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetleaveByEmpId(string empId)
        {
            return Json(_maternityLeaveTransactionService.Query(empId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getChildNo(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_maternityLeaveTransactionService.getChildNo(Id, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations

        #region Report
        [HttpGet, Authorize]
        public ActionResult LeaveReport(string fromDate, string toDate, string plantId, string employeeCodeString)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId) || plantId == "undefined")
            {
                plantId = identity.PlantId;
            }
            var fileName = DateTime.Now.ToString("yy-MM-dd") + " " + "Employee Leave Report";
            var workbook = _maternityLeaveTransactionService.LeaveReport(fromDate, toDate, plantId, employeeCodeString, identity.CompanyGroupId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult ShortLeaveReport(string date, string plantId, string employeeCodeString)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId) || plantId == "undefined")
            {
                plantId = identity.PlantId;
            }
            var fileName = DateTime.Now.ToString("yy-MM-dd") + " " + "Employee Short Leave Report";
            var workbook = _maternityLeaveTransactionService.ShortLeaveReport(date, identity.CompanyGroupId, plantId, employeeCodeString);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
        


        [HttpGet, Authorize]
        public ActionResult EmpEncashReport(string year, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId) || plantId == "undefined")
            {
                plantId = identity.PlantId;
            }
            DataTable dtYear =  _sqlRepository.GetDataTable(@"select Id,YearNo From YearlyCalendar where PlantId='" + identity.PlantId + "' and Id = '" + year + "' ");

            year = "2020";//= dtYear.Rows[0]["YearNo"].ToString();
            var fileName = DateTime.Now.ToString("yy-MM-dd") + " " + "Employee Leave Encashment Report";
            var workbook = _maternityLeaveTransactionService.EmpEncashReport(year, plantId, identity.CompanyGroupId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult EmpEncashReportOld(string fromDate, string toDate, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId) || plantId == "undefined")
            {
                plantId = identity.PlantId;
            }


            var fileName = DateTime.Now.ToString("yy-MM-dd") + " " + "Employee Leave Encashment Report";
            var workbook = _maternityLeaveTransactionService.EmpEncashReportOld(fromDate, toDate, plantId, identity.CompanyGroupId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }


        [HttpGet, Authorize]
        public JsonResult GetClanderYear()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = _maternityLeaveTransactionService.GetClanderYear(identity.PlantId);
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        [HttpGet, Authorize]
        public JsonResult GetPolicyData(string EffectiveDate, String plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_maternityLeaveTransactionService.GetPolicyData(EffectiveDate, identity.PlantId), JsonRequestBehavior.AllowGet);

        }
        #region Maternity Leave Reports
        [HttpGet, Authorize]
        public ActionResult MaternityLeaveReport(ReportFormat reportFormat, string SystemId,string LanguageId,string UserName,string LeaveTransactionId, string fromDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _maternityLeaveTransactionService.CreateMaternityLeaveReportSheet(identity.CompanyId, SystemId, LanguageId,identity.PlantId, UserName, LeaveTransactionId,  fromDate);//, strPathHindi, strPathEnglish, strPathBangla);

            }

            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }
        
        #endregion






    }
}
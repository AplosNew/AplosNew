#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Model.External;
using Library.Service.Employees;
using Library.Service.Properties;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class ResignationApprovalMultipleController : BaseController
    {
        #region Constructor
        private readonly IResignationService _ResignationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;

        public ResignationApprovalMultipleController(
              IResignationService ResignationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService)

        {
            _ResignationService = ResignationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
        }
        #endregion

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
      

        [HttpGet,Authorize]
        public ActionResult MultipleResignationAppliedList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ResignationService.MultipleResignationAppliedList(identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }
     

        [HttpGet, Authorize]
        public ActionResult MultipleResignationPendingList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           return Json(_ResignationService.MultipleResignationAppliedPendingList(identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyGroupId, identity.CompanyId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEntityByEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "ResignationApproval", identity.EmployeeId);
            return Json(entity, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetExperience(string EmpId)
        {
            _ResignationService.GetExperience(EmpId, out int tYear, out int tMonth);
            return Json(new { DurationY = tYear, DurationM = tMonth, JsonRequestBehavior.AllowGet });
        }

        [HttpPost, Authorize]
        public JsonResult Create(IEnumerable<Resignation> ResignationList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ResignationService.ApprovalUpdate(ResignationList,identity.Name,identity.IPAddress,identity.CompanyGroupId,identity.CompanyId);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult UpdateApprovalStatus(List<Dictionary<string, string>> ResignationList)
        {
          
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ResignationService.UpdateApprovalStatusUpdate(ResignationList, identity.Name, identity.IPAddress, identity.CompanyGroupId, identity.CompanyId);
            return Json(new { Message = AplosMessage.Success });
        }
        [HttpGet, Authorize]
        public JsonResult GetSeparationType()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ResignationService.GetCboSeparationType(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult Edit(Resignation model)
        {
            _ResignationService.Update(model);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _ResignationService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion

        [HttpGet, Authorize]
        public ActionResult ReportEmployeeInfo()
        {
            ReportParam param = new ReportParam();
            string fileName = "EmployeeInfo " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            IWorkbook workbook = _ResignationService.ReportEmployeeInfo(param);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult GetResignationList(GridParameter parameters, string plantId)
        {
            return Json(_ResignationService.ResignationApprovalQueryByPlantId(parameters, plantId), JsonRequestBehavior.AllowGet);
        }
    }
}
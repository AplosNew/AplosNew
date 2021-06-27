using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Organizations;
using Library.Service.Organizations;
using Library.Service.Reports;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class PositionController : BaseController
    {
        #region Constructor

        private readonly IPositionService _positionService;
        private readonly IPositionReportService _positionReportService;
        private readonly IPositionJobDescriptionService _positionJobDescriptionService;
        private readonly IPositionResponsiblePersonService _positionResponsiblePersonService;

        public PositionController(
              IPositionService positionService
            , IPositionJobDescriptionService positionJobDescriptionService
            , IPositionResponsiblePersonService positionResponsiblePersonService
            , IPositionReportService positionReportService
            )
        {
            _positionService = positionService;
            _positionReportService = positionReportService;
            _positionJobDescriptionService = positionJobDescriptionService;
            _positionResponsiblePersonService = positionResponsiblePersonService;
        }

        #endregion Constructor

        #region PositionResponsiblePerson

        #region BudgetMaster

        [HttpGet, Authorize]
        public ActionResult BudgetMaster()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult BudgetMasterResponsiblePerson(GridParameter parameters, string positionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_positionResponsiblePersonService.QueryBudgetMaster(parameters, identity.CompanyGroupId, positionId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveBudgetMaster(PositionResponsiblePerson entity)
        {
            _positionResponsiblePersonService.SaveBudgetMaster(entity);
            return Json(new { Message = AplosMessage.Success });
        }

        #endregion BudgetMaster

        #region BudgetMasterActivity

        [HttpGet, Authorize]
        public ActionResult BudgetMasterActivity()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult BudgetMasterActivityResponsiblePerson(GridParameter parameters, string positionId, string budgetMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_positionResponsiblePersonService.QueryBudgetMasterActivity(parameters, identity.CompanyGroupId, positionId, budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveBudgetMasterActivity(PositionResponsiblePerson entity)
        {
            _positionResponsiblePersonService.SaveBudgetMasterActivity(entity);
            return Json(new { Message = AplosMessage.Success });
        }

        #endregion BudgetMasterActivity

        #endregion PositionResponsiblePerson

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult PositionRelationship()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCboList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_positionService.GetCboByCompanyGroup(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByEntity(string entityId)
        {
            return Json(new SelectList(_positionService.GetCboByEntity(entityId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(new SelectList(_positionService.GetCboByCompanyGroup(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_positionService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult QueryByEntityId(GridParameter parameters, string entityId)
        {
            return Json(_positionService.QueryByEntityId(parameters, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPositionJobDescriptionList(GridParameter parameters, string positionId)
        {
            return Json(_positionJobDescriptionService.Query(parameters, positionId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCompanyStructureSetupList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_positionService.GetForResponsiblePerson(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(Position positionStructureSetup, IEnumerable<PositionJobDescription> positionJobDescription)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            positionStructureSetup.CompanyGroupId = identity.CompanyGroupId;
            _positionService.Update(positionStructureSetup, positionJobDescription);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _positionService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(Position positionStructureSetup, IEnumerable<PositionJobDescription> positionJobDescription)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            positionStructureSetup.CompanyGroupId = identity.CompanyGroupId;
            _positionService.Insert(positionStructureSetup, positionJobDescription);
            return Json(new { Message = AplosMessage.Insert });
        }

        #region Allowance

        [HttpGet, Authorize]
        public ActionResult Allowance()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateAllowance(PositionAllowance positionAllowance)
        {
            _positionService.InsertAllowance(positionAllowance);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditAllowance(PositionAllowance positionAllowance)
        {
            _positionService.UpdateAllowance(positionAllowance);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet]
        public JsonResult QueryAllowance(GridParameter parameters, string positionId)
        {
            return Json(_positionService.QueryAllowance(positionId, parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllowance(string id)
        {
            return Json(_positionService.GetPositionAllowance(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllowanceForEffectiveDate(string id, DateTime date)
        {
            return Json(_positionService.GetPositionAllowance(id, date), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteAllowance(string id)
        {
            _positionService.DeleteAllowance(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Allowance

        // Specific column
        [HttpGet]
        public JsonResult Get(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_positionService.GetData(identity.CompanyGroupId, id), JsonRequestBehavior.AllowGet);
        }

        // All column
        [HttpGet]
        public JsonResult GetById(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_positionService.Get(identity.CompanyGroupId, id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult PositionReport()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Position Report " + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _positionReportService.PositionReport(identity.CompanyGroupId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
    }
}
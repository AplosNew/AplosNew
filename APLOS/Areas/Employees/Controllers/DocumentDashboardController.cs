#region Using

using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Employees;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class DocumentDashboardController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IDocumentDashboardService _documentDashboardService;
        private readonly ITempDocDashboardService _tempDocDashboardService;
        private readonly IEmployeeDocumentService _employeeDocumentService;
        private readonly IPreRecruitmentDocumentService _preRecruitementDocumentService;

        public DocumentDashboardController(
            IDocumentDashboardService documentDashboardService,
            ITempDocDashboardService tempDocDashboardService,
            IEmployeeDocumentService employeeDocumentService,
            IPreRecruitmentDocumentService preRecruitementDocumentService,
            ISqlRepository sqlRepository
            )
        {
            _documentDashboardService = documentDashboardService;
            _tempDocDashboardService = tempDocDashboardService;
            _employeeDocumentService = employeeDocumentService;
            _preRecruitementDocumentService = preRecruitementDocumentService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult ExcelReport()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult DailyOverDueStatus(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.DailyOverDueStatus(identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult PieChart(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.PieChart(identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult PendingDocuments(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.PendingDocuments(identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult PreEmp(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            return Json(_documentDashboardService.PreEmp(parameter, employmentstage, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult PreEmp1(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            return Json(_documentDashboardService.PreEmp1(parameter, employmentstage, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult PreEmp2(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            return Json(_documentDashboardService.PreEmp2(parameter, employmentstage, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult PreEmp3(GridParameter parameter, string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            return Json(_documentDashboardService.PreEmp3(parameter, employmentstage, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Doc(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            return Json(_documentDashboardService.Doc(employmentstage, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Doc1(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            return Json(_documentDashboardService.Doc1(employmentstage, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Doc2(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            return Json(_documentDashboardService.Doc2(employmentstage, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Doc3(string employmentstage, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            return Json(_documentDashboardService.Doc3(employmentstage, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult EmpWiseDocOpt(string employmentStage, string segment, string preRecEmployeeId, string employeeId, string DocumentCategoryId, string DocumentSubCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.EmpWiseDocOpt(employmentStage, segment, preRecEmployeeId, employeeId, identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult EmpWiseDocMandt(string employmentStage, string segment, string preRecEmployeeId, string employeeId, string DocumentCategoryId, string DocumentSubCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.EmpWiseDocMandt(employmentStage, segment, preRecEmployeeId, employeeId, identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DocWiseEmp(GridParameter parameter, string employmentStage, string segment, string CompDocumentId, string EmplyeeTypeOrCategoryId, string DocumentCategoryId, string DocumentSubCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.DocWiseEmp(parameter, employmentStage, segment, CompDocumentId, EmplyeeTypeOrCategoryId, identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult CompletdDocWiseEmp(GridParameter parameter, string CompDocumentId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.CompletdDocWiseEmp(parameter, CompDocumentId, identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult OthersDocWiseEmp(GridParameter parameter, string CompDocumentId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.OthersDocWiseEmp(parameter, CompDocumentId, identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult OverDueWiseEmp(GridParameter parameter, string CompDocumentId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.OverDueWiseEmp(parameter, CompDocumentId, identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DueWiseEmp(GridParameter parameter, string CompDocumentId, string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.DueWiseEmp(parameter, CompDocumentId, identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult OverDueStatus(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            return Json(_documentDashboardService.OverDueStatus(DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetCbo()
        //{
        //	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //	return Json(_documentDashboardService.GetCbo(identity.CompanyGroupId).Rows, JsonRequestBehavior.AllowGet);
        //} GetComplianceDocumentDetail(string compnayGroupId, string complianceDocumentId);

        [HttpGet, Authorize]
        public JsonResult GetComplianceDocumentDetail(string complianceDocumentId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.GetComplianceDocumentDetail(identity.CompanyGroupId, complianceDocumentId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetComplianceDocumentCbo(string ComplianceDocumentCategoryId, string ComplianceDocumentSubCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.GetComplianceDocumentCbo(identity.CompanyGroupId, ComplianceDocumentCategoryId, ComplianceDocumentSubCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCascadingComplianceDocumentCategoryCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.GetCascadingComplianceDocumentCategoryCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCascadingComplianceDocumentSubCategoryCbo(string documentCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.GetCascadingComplianceDocumentSubCategoryCbo(identity.CompanyGroupId, documentCategoryId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetResponsiblePersonCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.GetResponsiblePersonCbo(identity.CompanyGroupId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult PieCompletedDoc(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.PieCompletedDoc(identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult PieOthersDoc(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.PieOthersDoc(identity.CompanyGroupId, DocumentCategoryId, EmplyeeTypeOrCategoryId, DocumentSubCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult PieDueDoc(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.PieDueDoc(identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult PieOverDueDoc(string DocumentCategoryId, string DocumentSubCategoryId, string EmplyeeTypeOrCategoryId, string ComplianceDocumentId, string DocumentationBy, string ResponsiblePersonId, string Importance, string OptionalOrMandatory, string DocumentType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_documentDashboardService.PieOverDueDoc(identity.CompanyGroupId, DocumentCategoryId, DocumentSubCategoryId, EmplyeeTypeOrCategoryId, ComplianceDocumentId, DocumentationBy, ResponsiblePersonId, Importance, OptionalOrMandatory, DocumentType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult DocDashboardSync()
        {
            _preRecruitementDocumentService.ProcessDocumentDailyOverDue(DateTime.Today, "Refresh Button", "TS");
            _employeeDocumentService.ProcessDocumentDailyOverDue(DateTime.Today, "Refresh Button", "TS");
            _tempDocDashboardService.DataInsertInTemTable();
            return Json("Data Sync Succesfully", JsonRequestBehavior.AllowGet);
        }

        //-----------------------------------Report-----------------------------------------//
        [HttpGet, Authorize]
        public JsonResult IndividualExcelReport()
        {
            return Json("Data Sync Succesfully", JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]

        public ActionResult EmployeeDocumentReport()
        {
            return View();
        }
        [HttpGet, Authorize]
        public ActionResult PreRecruitmentDocumentReport()
        {
            var fileName = "Candidate Document Report " + DateTime.Now.ToString("dd-MMM-yyyy") + "";
            var workbook = _documentDashboardService.GetEmployeeDocumentReport();
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        #region Syncfusion Document Dashboard
        string baseQueryd = @"SELECT ed.Id AS EmployeeDocumentID
                            ,cd.Id AS DocumentID,cdc.Id DocumentCategoryId, cdsc.Id DocumentSubCategoryId,EC.Id EmpCatgId,EC.UserName EmpCategory
                            ,cd.UserName AS DocumentUserName,cd.ShortName AS DocumentShortName,cd.DocumentType,cd.DocumentationBy,cd.Importance,ei.SystemID as EmployeeID,
                                    ei.EmployeeCode,ei.EmployeeName, cdc.UserName AS DocumentCategoryName,cdsc.UserName AS DocumentSubCategoryName,cdsd.OptionalOrMandatory,
                                    CASE WHEN isnull(ed.FileId,'')<>'' AND isnull(ed.[FileName],'')<>'' THEN 'Completed' ELSE 
		                                    --due or overdue
                                        CASE WHEN DATEDIFF(DAY, ed.DueDate, GETDATE()) >0 THEN 'Overdue' ELSE 'Due' END
                                          END AS DocumentStatus,

                                     CASE WHEN isnull(ed.FileId,'')<>'' AND isnull(ed.[FileName],'')<>'' THEN
                                         CASE WHEN DATEDIFF(DAY, ed.AddedDate, ed.DueDate)>=0 THEN 'OnTime' ELSE 'LATE' END
                                    ELSE '' END AS CompletionStatus,

                                    CASE WHEN isnull(ed.FileId,'')='' OR isnull(ed.[FileName],'')='' THEN
                                        DATEDIFF(DAY, ed.DueDate, GETDATE())
                                    ELSE 0 END AS DueDays,
                                    FORMAT(GETDATE(),'dd-MMM-yyyy') AS Today,
                                    FORMAT(ed.DueDate, 'dd-MMM-yyyy') AS DueDate,
                                    FORMAT(ed.AddedDate, 'dd-MMM-yyyy') AS AddedDate
                                     FROM EmployeeDocument AS ed
                                    LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=ed.EmpSystemID
									LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
                                                (
                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												)EC ON EC.DesignationId=EI.GivenDesignationId
                                    LEFT OUTER JOIN hkp.ComplianceDocument AS cd ON cd.Id= ed.ComplianceDocumentId
                                    --LEFT OUTER JOIN hkp.ComplianceDocumentSet AS cds ON cds.Id= ed.ComplianceDocumentSetId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSetDetail AS cdsd ON ed.ComplianceDocumentSetId= cdsd.ComplianceDocumentSetId AND ed.ComplianceDocumentId= cdsd.ComplianceDocumentId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentCategory AS cdc ON cdc.Id= cd.ComplianceDocumentCategoryId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSubCategory AS cdsc ON cdsc.Id= cd.ComplianceDocumentSubCategoryId
                                    WHERE ISNULL(ed.DueDate,'')<>''";
        private string baseQuery = @"SELECT '' CandidateID,ed.Id AS EmployeeDocumentID
                            ,cd.Id AS DocumentID,cdc.Id DocumentCategoryId, cdsc.Id DocumentSubCategoryId,EC.Id EmpCatgId,ISNULL(EC.UserName,'') EmpCategory
                            ,cd.UserName AS DocumentUserName,cd.ShortName AS DocumentShortName,cd.DocumentType,cd.DocumentationBy,cd.Importance,ei.SystemID as EmployeeID,
                                    ei.EmployeeCode,ei.EmployeeName, cdc.UserName AS DocumentCategoryName,cdsc.UserName AS DocumentSubCategoryName,cdsd.OptionalOrMandatory,
                                    CASE WHEN isnull(ed.FileId,'')<>'' AND isnull(ed.[FileName],'')<>'' THEN 'Completed' ELSE 
		                                    --due or overdue
                                        CASE WHEN DATEDIFF(DAY, ed.DueDate, GETDATE()) > 0 THEN 'Overdue' WHEN ISNULL(ed.DueDate,'') = '' THEN '' ELSE 'Due' END
                                          END AS DocumentStatus,

                                     CASE WHEN ISNULL(ed.FileId,'')<>'' AND ISNULL(ed.[FileName],'')<>'' THEN
                                         CASE WHEN DATEDIFF(DAY, ed.AddedDate, ed.DueDate)>=0 THEN 'OnTime' ELSE 'LATE' END
                                    ELSE '' END AS CompletionStatus,

                                    CASE WHEN isnull(ed.FileId,'')='' OR isnull(ed.[FileName],'')='' THEN
                                        DATEDIFF(DAY, ed.DueDate, GETDATE())
                                    ELSE 0 END AS DueDays,
                                    FORMAT(GETDATE(),'dd-MMM-yyyy') AS Today,
                                    FORMAT(ed.DueDate, 'dd-MMM-yyyy') AS DueDate,
                                    FORMAT(ed.AddedDate, 'dd-MMM-yyyy') AS AddedDate
                                     FROM EmployeeDocument AS ed
                                    LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=ed.EmpSystemID
									LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
                                                (
                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												)EC ON EC.DesignationId=EI.GivenDesignationId
                                    LEFT OUTER JOIN hkp.ComplianceDocument AS cd ON cd.Id= ed.ComplianceDocumentId
                                    --LEFT OUTER JOIN hkp.ComplianceDocumentSet AS cds ON cds.Id= ed.ComplianceDocumentSetId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSetDetail AS cdsd ON ed.ComplianceDocumentSetId= cdsd.ComplianceDocumentSetId AND ed.ComplianceDocumentId= cdsd.ComplianceDocumentId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentCategory AS cdc ON cdc.Id= cd.ComplianceDocumentCategoryId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSubCategory AS cdsc ON cdsc.Id= cd.ComplianceDocumentSubCategoryId
                                    WHERE EI.EmployeeStatus <> 'Separated'
									UNION
									SELECT PREEMP.Id AS CandidateID,''
                                    ,cd.Id AS DocumentID,cdc.Id DocumentCategoryId, cdsc.Id DocumentSubCategoryId,EC.Id EmpCatgId,ISNULL(EC.UserName,'') EmpCategory
                                    ,cd.UserName AS DocumentUserName,cd.ShortName AS DocumentShortName,cd.DocumentType,cd.DocumentationBy,cd.Importance,'' EmployeeID,
                                   '',PREEMP.EmployeeName, cdc.UserName AS DocumentCategoryName,cdsc.UserName AS DocumentSubCategoryName,cdsd.OptionalOrMandatory,
                                    CASE WHEN isnull(PRD.FileId,'')<>'' AND isnull(PRD.[FileName],'')<>'' THEN 'Completed' ELSE 
		                                    --due or overdue
                                        CASE WHEN DATEDIFF(DAY, PRD.DueDate, GETDATE()) >0 THEN 'Overdue' WHEN ISNULL(PRD.DueDate,'') = '' THEN '' ELSE 'Due' END
                                          END AS DocumentStatus,
                                     CASE WHEN isnull(PRD.FileId,'')<>'' AND isnull(PRD.[FileName],'')<>'' THEN
                                         CASE WHEN DATEDIFF(DAY, PRD.AddedDate, PRD.DueDate)>=0 THEN 'OnTime' ELSE 'LATE' END
                                    ELSE '' END AS CompletionStatus,

                                    CASE WHEN isnull(PRD.FileId,'')='' OR isnull(PRD.[FileName],'')='' THEN
                                        DATEDIFF(DAY, PRD.DueDate, GETDATE())
                                    ELSE 0 END AS DueDays,
                                    FORMAT(GETDATE(),'dd-MMM-yyyy') AS Today,
                                    FORMAT(PRD.DueDate, 'dd-MMM-yyyy') AS DueDate,
                                    FORMAT(PRD.AddedDate, 'dd-MMM-yyyy') AS AddedDate
                                     FROM PreRecruitmentDocument AS PRD
                                    LEFT OUTER JOIN PreRecruitmentEmployee AS PREEMP ON PREEMP.Id = PRD.PreRecruitmentEmployeeId
									LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
                                                (
                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												)EC ON EC.DesignationId=PREEMP.GivenDesignationId
                                    LEFT OUTER JOIN hkp.ComplianceDocument AS cd ON cd.Id= PRD.ComplianceDocumentId
                                    --LEFT OUTER JOIN hkp.ComplianceDocumentSet AS cds ON cds.Id= ed.ComplianceDocumentSetId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSetDetail AS cdsd ON PRD.ComplianceDocumentSetId= cdsd.ComplianceDocumentSetId AND PRD.ComplianceDocumentId= cdsd.ComplianceDocumentId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentCategory AS cdc ON cdc.Id= cd.ComplianceDocumentCategoryId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSubCategory AS cdsc ON cdsc.Id= cd.ComplianceDocumentSubCategoryId
                                    WHERE  PRD.IsCopied = 0";
        private string baseQueryForEmployee = @"SELECT '' CandidateID,ei.SystemID as EmployeeID,
                                    ei.EmployeeCode,ei.EmployeeName,ED.Id AS EmployeeDocumentID,MBudget.Code BudgetCode,Desg.UserName GivenDesignation
                                    ,CD.Id AS DocumentID,CDC.Id DocumentCategoryId, CDSC.Id DocumentSubCategoryId,EC.Id EmpCatgId,EC.UserName EmpCategory,EI.DOJ,Company.UserName CompanyName,Plant.UserName PlantName,Company.Id CompanyId,Plant.Id PlantId
                                    ,CD.UserName AS DocumentUserName,CD.ShortName AS DocumentShortName,cd.DocumentType,cd.DocumentationBy,cd.Importance, cdc.UserName AS DocumentCategoryName,cdsc.UserName AS DocumentSubCategoryName,cdsd.OptionalOrMandatory,
                                    CASE WHEN isnull(ed.FileId,'')<>'' AND isnull(ed.[FileName],'')<>'' THEN 'Completed' ELSE 
		                                    --due or overdue
                                        CASE WHEN DATEDIFF(DAY, ED.DueDate, GETDATE()) >0 THEN 'Overdue'  WHEN ISNULL(ED.DueDate,'') = '' THEN '' ELSE 'Due' END
                                          END AS DocumentStatus,

                                     CASE WHEN isnull(ed.FileId,'')<>'' AND isnull(ed.[FileName],'')<>'' THEN
                                         CASE WHEN DATEDIFF(DAY, ed.AddedDate, ed.DueDate)>=0 THEN 'OnTime' ELSE 'LATE' END
                                    ELSE '' END AS CompletionStatus,

                                    CASE WHEN isnull(ed.FileId,'')='' OR isnull(ed.[FileName],'')='' THEN
                                        DATEDIFF(DAY, ed.DueDate, GETDATE())
                                    ELSE 0 END AS DueDays,
                                    FORMAT(GETDATE(),'dd-MMM-yyyy') AS Today,
                                    FORMAT(ed.DueDate, 'dd-MMM-yyyy') AS DueDate,
                                    FORMAT(ed.AddedDate, 'dd-MMM-yyyy') AS AddedDate
                                     FROM EmployeeDocument AS ed
                                    LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=ed.EmpSystemID
                                    LEFT OUTER JOIN ORG.Company AS Company ON ei.CompanyId=Company.Id
									LEFT JOIN ORG.Plant AS Plant ON Plant.Id = ei.PlantId
									LEFT JOIN MST.ManpowerBudget AS MBudget ON MBudget.Id = ei.BudgetCode
									LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
                                                (
                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												)EC ON EC.DesignationId=EI.GivenDesignationId
									LEFT OUTER JOIN HKP.Designation AS Desg ON Desg.Id = ei.GivenDesignationId
                                    LEFT OUTER JOIN hkp.ComplianceDocument AS cd ON cd.Id= ed.ComplianceDocumentId
                                    --LEFT OUTER JOIN hkp.ComplianceDocumentSet AS cds ON cds.Id= ed.ComplianceDocumentSetId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSetDetail AS cdsd ON ed.ComplianceDocumentSetId= cdsd.ComplianceDocumentSetId AND ed.ComplianceDocumentId= cdsd.ComplianceDocumentId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentCategory AS cdc ON cdc.Id= cd.ComplianceDocumentCategoryId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSubCategory AS cdsc ON cdsc.Id= cd.ComplianceDocumentSubCategoryId
                                     WHERE EI.EmployeeStatus <> 'Separated'
                                    UNION
									SELECT  ISNULL(PREEMP.Id,'') AS CandidateID,'','',PREEMP.EmployeeName,prd.Id EmployeeDocumentID,'',Desg.UserName GivenDesignation
                            ,cd.Id AS DocumentID,cdc.Id DocumentCategoryId, cdsc.Id DocumentSubCategoryId,EC.Id EmpCatgId,EC.UserName EmpCategory,''DOJ,Company.UserName CompanyName,Plant.UserName PlantName,Company.Id CompanyId,Plant.Id PlantId
                            ,cd.UserName AS DocumentUserName,cd.ShortName AS DocumentShortName,cd.DocumentType,cd.DocumentationBy,cd.Importance,
                                  cdc.UserName AS DocumentCategoryName,cdsc.UserName AS DocumentSubCategoryName,cdsd.OptionalOrMandatory,
                                    CASE WHEN isnull(PRD.FileId,'')<>'' AND isnull(PRD.[FileName],'')<>'' THEN 'Completed' ELSE 
		                                    --due or overdue
                                        CASE WHEN DATEDIFF(DAY, PRD.DueDate, GETDATE()) >0 THEN 'Overdue'  WHEN ISNULL(PRD.DueDate,'') = '' THEN '' ELSE 'Due' END
                                          END AS DocumentStatus,

                                     CASE WHEN isnull(PRD.FileId,'')<>'' AND isnull(PRD.[FileName],'')<>'' THEN
                                         CASE WHEN DATEDIFF(DAY, PRD.AddedDate, PRD.DueDate)>=0 THEN 'OnTime' ELSE 'LATE' END
                                    ELSE '' END AS CompletionStatus,

                                    CASE WHEN isnull(PRD.FileId,'')='' OR isnull(PRD.[FileName],'')='' THEN
                                        DATEDIFF(DAY, PRD.DueDate, GETDATE())
                                    ELSE 0 END AS DueDays,
                                    FORMAT(GETDATE(),'dd-MMM-yyyy') AS Today,
                                    FORMAT(PRD.DueDate, 'dd-MMM-yyyy') AS DueDate,
                                    FORMAT(PRD.AddedDate, 'dd-MMM-yyyy') AS AddedDate
                                     FROM PreRecruitmentDocument AS PRD
                                    LEFT OUTER JOIN PreRecruitmentEmployee AS PREEMP ON PREEMP.Id = PRD.PreRecruitmentEmployeeId
									 LEFT OUTER JOIN ORG.Company AS Company ON PREEMP.CompanyId=Company.Id
									LEFT JOIN ORG.Plant AS Plant ON Plant.Id = PREEMP.PlantId
									LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
                                                (
                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												)EC ON EC.DesignationId=PREEMP.GivenDesignationId
                                    LEFT OUTER JOIN hkp.ComplianceDocument AS cd ON cd.Id= PRD.ComplianceDocumentId
									LEFT OUTER JOIN HKP.Designation AS Desg ON Desg.Id = PREEMP.GivenDesignationId
                                    --LEFT OUTER JOIN hkp.ComplianceDocumentSet AS cds ON cds.Id= ed.ComplianceDocumentSetId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSetDetail AS cdsd ON PRD.ComplianceDocumentSetId= cdsd.ComplianceDocumentSetId AND PRD.ComplianceDocumentId= cdsd.ComplianceDocumentId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentCategory AS cdc ON cdc.Id= cd.ComplianceDocumentCategoryId
                                    LEFT OUTER JOIN hkp.ComplianceDocumentSubCategory AS cdsc ON cdsc.Id= cd.ComplianceDocumentSubCategoryId
                                    WHERE  PRD.IsCopied = 0";

        private DataTable GetEmployeeData(string desiredOutputString, string prameterString, string documentType, string segment, string orderBy)
        {
            try
            {
                string sqlText = desiredOutputString + baseQueryForEmployee + ") AS dd " + prameterString + segment + orderBy;

                return _sqlRepository.GetDataTable(sqlText);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetMasterFilterationData()
        {
            try
            {
                string sqlText = @"SELECT Distinct K.EmpCatgId,K.EmpCategory, K.DocumentCategoryName,k.DocumentSubCategoryName,k.DocumentUserName,K.DocumentType,K.DocumentationBy,K.Importance,K.OptionalOrMandatory
                                    ,K.DocumentID,K.DocumentCategoryId,K.DocumentSubCategoryId,K.EmpCatgId
                                  FROM ("
                                    + baseQuery +
                                    ") AS K ";
                return Json(_sqlRepository.GetDataCollection(sqlText, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetMasterSegmentedData(string parameterString)
        {
            try
            {
                string orderBy = "Order by DocumentType,EmployeeID,DocumentID";
                string desiredOutputString = @"SELECT DD.EmployeeID, DD.EmpCatgId,DD.EmpCategory, DD.DocumentCategoryName,DD.DocumentSubCategoryName,DD.DocumentUserName,DD.DocumentType,DD.DocumentationBy,DD.Importance,DD.OptionalOrMandatory
                                    , DD.DocumentID,DD.DocumentCategoryId,DD.DocumentSubCategoryId,DD.DocumentStatus,DD.CompletionStatus,DD.DueDays
                                  FROM(";
                DataTable dtBaseData = GetMasterDataSql(desiredOutputString, parameterString, "", "", orderBy);

                var dtDocumentType = _sqlRepository.GetDataTable("SELECT DISTINCT DocumentType FROM HKP.ComplianceDocument");

                List<docDashBoardModel> DataList = new List<docDashBoardModel>();
                for (int i = 0; i < dtDocumentType.Rows.Count; i++)
                {
                    docDashBoardModel _data = new docDashBoardModel();
                    _data.DocumentType = dtDocumentType.Rows[i]["DocumentType"].ToString();

                    DataList.Add(_data);
                }

                StringCollection str = new StringCollection();
                StringCollection sl1 = new StringCollection();
                StringCollection sl2 = new StringCollection();
                StringCollection sl3 = new StringCollection();

                string documentType = "";

                docDashBoardModel model = null;
                for (int i = 0; i < dtBaseData.Rows.Count; i++)
                {
                    if (documentType != dtBaseData.Rows[i]["DocumentType"].ToString())
                    {
                        sl1 = new StringCollection();
                        sl2 = new StringCollection();
                        sl3 = new StringCollection();
                        str = new StringCollection();
                        model = DataList.SingleOrDefault(aa => aa.DocumentType == dtBaseData.Rows[i]["DocumentType"].ToString());
                        documentType = dtBaseData.Rows[i]["DocumentType"].ToString();
                    }

                    if (dtBaseData.Rows[i]["DocumentStatus"].ToString().ToUpper() == "COMPLETED" && dtBaseData.Rows[i]["CompletionStatus"].ToString().ToUpper() != "")
                        model.TotalCompleted++;

                    if (dtBaseData.Rows[i]["DocumentStatus"].ToString().ToUpper() == "COMPLETED" && dtBaseData.Rows[i]["CompletionStatus"].ToString().ToUpper() == "ONTIME")
                    {
                        model.TotalOntimeCompleted++;
                    }
                    if (dtBaseData.Rows[i]["DocumentStatus"].ToString().ToUpper() == "DUE" && dtBaseData.Rows[i]["CompletionStatus"].ToString().ToUpper() == "")
                    {
                        model.TotalDue++;
                    }
                    if (dtBaseData.Rows[i]["DocumentStatus"].ToString().ToUpper() == "OVERDUE" && dtBaseData.Rows[i]["CompletionStatus"].ToString().ToUpper() == "")
                    {
                        if (Convert.ToDouble(bplib.clsWebLib.GetNumData(dtBaseData.Rows[i]["DueDays"].ToString())) > 0)
                        {
                            model.OverAllDueDoc++;

                            if (str.Contains(dtBaseData.Rows[i]["EmployeeID"].ToString()) == false)
                            {
                                model.OverAllDueEmp++;
                                str.Add(dtBaseData.Rows[i]["EmployeeID"].ToString());
                            }
                            if (Convert.ToDouble(bplib.clsWebLib.GetNumData(dtBaseData.Rows[i]["DueDays"].ToString())) > 0 &&
                                Convert.ToDouble(bplib.clsWebLib.GetNumData(dtBaseData.Rows[i]["DueDays"].ToString())) < 10)
                            {
                                model.Seg1Doc++;

                                if (sl1.Contains(dtBaseData.Rows[i]["EmployeeID"].ToString()) == false)
                                {
                                    model.Seg1Emp++;
                                    sl1.Add(dtBaseData.Rows[i]["EmployeeID"].ToString());
                                }
                            }
                            else if (Convert.ToDouble(bplib.clsWebLib.GetNumData(dtBaseData.Rows[i]["DueDays"].ToString())) >= 10 &&
                               Convert.ToDouble(bplib.clsWebLib.GetNumData(dtBaseData.Rows[i]["DueDays"].ToString())) < 30)
                            {
                                model.Seg2Doc++;
                                if (sl2.Contains(dtBaseData.Rows[i]["EmployeeID"].ToString()) == false)
                                {
                                    model.Seg2Emp++;
                                    sl2.Add(dtBaseData.Rows[i]["EmployeeID"].ToString());
                                }
                            }
                            else if (Convert.ToDouble(bplib.clsWebLib.GetNumData(dtBaseData.Rows[i]["DueDays"].ToString())) >= 30)
                            {
                                model.Seg3Doc++;
                                if (sl3.Contains(dtBaseData.Rows[i]["EmployeeID"].ToString()) == false)
                                {
                                    model.Seg3Emp++;
                                    sl3.Add(dtBaseData.Rows[i]["EmployeeID"].ToString());
                                }
                            }
                        }

                    }

                }

                foreach (docDashBoardModel item in DataList)
                {
                    if (item.TotalCompleted > 0)
                    {
                        item.TotalOntimeCompletionPercentage = Convert.ToDouble(((item.TotalOntimeCompleted / item.TotalCompleted) * 100).ToString("F2"));
                    }
                }

                //for
                return Json(DataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetBarChartInfo(string parameterString)
        {
            try
            {
                string orderBy = "ORDER BY DocumentID";
                string desiredOutputString = @"SELECT DocumentType,DocumentID,DocumentShortName,DocumentUserName
                                               ,DocumentCategoryId,DocumentSubCategoryId,DocumentSubCategoryName,dd.DocumentCategoryName,dd.EmployeeID,dd.CompletionStatus
                                            ,dd.DocumentationBy,dd.Importance,dd.DocumentStatus,dd.OptionalOrMandatory,dd.DueDays
                                  FROM(";
                var dtBarChart = GetMasterDataSql(desiredOutputString, parameterString, "", "", orderBy);
                List<DocBarChartStatus> DataList = new List<DocBarChartStatus>();
                StringCollection str = new StringCollection();
                DataView dvDocumnet = new DataView(dtBarChart.DefaultView.ToTable(true, "DocumentID", "DocumentUserName", "DocumentShortName"));

                for (int i = 0; i < dvDocumnet.Count; i++)
                {
                    DocBarChartStatus _data = new DocBarChartStatus();
                    _data.ComplianceDocumentId = dvDocumnet[i]["DocumentID"].ToString();
                    _data.ComplianceDocumentUserName = dvDocumnet[i]["DocumentUserName"].ToString();
                    _data.ComplianceDocumentShortName = dvDocumnet[i]["DocumentShortName"].ToString();

                    DataList.Add(_data);
                }
                DocBarChartStatus barChartModel = null;
                string documentId = "";
                for (int i = 0; i < dtBarChart.Rows.Count; i++)
                {
                    if (documentId != dtBarChart.Rows[i]["DocumentID"].ToString())
                    {
                        str = new StringCollection();
                        barChartModel = DataList.SingleOrDefault(aa => aa.ComplianceDocumentId == dtBarChart.Rows[i]["DocumentID"].ToString());

                    }

                    if (dtBarChart.Rows[i]["DocumentStatus"].ToString().ToUpper() == "DUE" && dtBarChart.Rows[i]["CompletionStatus"].ToString().ToUpper() == "")
                    {
                        barChartModel.TotalDueDocument++;
                    }
                    if (dtBarChart.Rows[i]["DocumentStatus"].ToString().ToUpper() == "OVERDUE" && dtBarChart.Rows[i]["CompletionStatus"].ToString().ToUpper() == "")
                    {
                        barChartModel.TotalOverDueDocument++;
                    }
                }

                return Json(DataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPieChartDataModalInfo(string parameterString)
        {
            try
            {
                string orderBy = "ORDER BY DocumentID";
                string desiredOutputString = @"SELECT dd.DocumentType,dd.DocumentID,dd.DocumentShortName,dd.DocumentUserName
                                               ,DD.DocumentCategoryId,DD.DocumentSubCategoryId,dd.DocumentSubCategoryName,dd.DocumentCategoryName,dd.EmployeeID,dd.CompletionStatus
                                            ,dd.DocumentationBy,dd.Importance,dd.DocumentStatus,dd.OptionalOrMandatory,dd.DueDays
                                  FROM(";
                var dtBarChart = GetMasterDataSql(desiredOutputString, parameterString, "", "", orderBy);
                List<DocPieCharDocumentStatus> DataList = new List<DocPieCharDocumentStatus>();
                StringCollection str = new StringCollection();
                DataView dvDocumnet = new DataView(dtBarChart.DefaultView.ToTable(true, "DocumentID", "DocumentUserName", "DocumentShortName", "DocumentCategoryName", "DocumentSubCategoryName", "DocumentationBy", "Importance", "OptionalOrMandatory", "DocumentType"));

                for (int i = 0; i < dvDocumnet.Count; i++)
                {
                    DocPieCharDocumentStatus _data = new DocPieCharDocumentStatus();
                    _data.ComplianceDocumentId = dvDocumnet[i]["DocumentID"].ToString();
                    _data.ComplianceDocumentUserName = dvDocumnet[i]["DocumentUserName"].ToString();
                    _data.ComplianceDocumentShortName = dvDocumnet[i]["DocumentShortName"].ToString();
                    _data.ComplianceDocumentCategory = dvDocumnet[i]["DocumentCategoryName"].ToString();
                    _data.ComplianceDocumentSubCategory = dvDocumnet[i]["DocumentSubCategoryName"].ToString();
                    _data.ComplianceDocumentType = dvDocumnet[i]["DocumentType"].ToString();
                    _data.ComplianceDocumentionBy = dvDocumnet[i]["DocumentationBy"].ToString();
                    _data.ComplianceDocumentResponsiblePerson = "";
                    _data.ComplianceDocumentImportance = dvDocumnet[i]["Importance"].ToString();
                    _data.ComplianceDocumentOptionalOrMandatory = dvDocumnet[i]["OptionalOrMandatory"].ToString();

                    DataList.Add(_data);
                }
                DocPieCharDocumentStatus pieChartModel = null;
                string documentId = "";
                for (int i = 0; i < dtBarChart.Rows.Count; i++)
                {
                    if (documentId != dtBarChart.Rows[i]["DocumentID"].ToString())
                    {
                        str = new StringCollection();
                        pieChartModel = DataList.SingleOrDefault(aa => aa.ComplianceDocumentId == dtBarChart.Rows[i]["DocumentID"].ToString()
                                                                       &&
                                                                       aa.ComplianceDocumentCategory == dtBarChart.Rows[i]["DocumentCategoryName"].ToString()
                                                                       &&
                                                                       aa.ComplianceDocumentSubCategory == dtBarChart.Rows[i]["DocumentSubCategoryName"].ToString()
                                                                       &&
                                                                       aa.ComplianceDocumentType == dtBarChart.Rows[i]["DocumentType"].ToString()
                                                                       &&
                                                                       aa.ComplianceDocumentionBy == dtBarChart.Rows[i]["DocumentationBy"].ToString()
                                                                       &&
                                                                       aa.ComplianceDocumentImportance == dtBarChart.Rows[i]["Importance"].ToString()
                                                                       &&
                                                                       aa.ComplianceDocumentOptionalOrMandatory == dtBarChart.Rows[i]["OptionalOrMandatory"].ToString()
                                                                    );

                    }

                    if (dtBarChart.Rows[i]["DocumentStatus"].ToString().ToUpper() == "DUE" && dtBarChart.Rows[i]["CompletionStatus"].ToString().ToUpper() == "")
                    {
                        pieChartModel.TotalDueDocument++;
                    }
                    if (dtBarChart.Rows[i]["DocumentStatus"].ToString().ToUpper() == "OVERDUE" && dtBarChart.Rows[i]["CompletionStatus"].ToString().ToUpper() == "")
                    {
                        pieChartModel.TotalOverDueDocument++;
                    }
                    if (dtBarChart.Rows[i]["DocumentStatus"].ToString().ToUpper() == "COMPLETED" && dtBarChart.Rows[i]["CompletionStatus"].ToString().ToUpper() != "")
                    {
                        pieChartModel.TotalCompletedDocument++;
                    }
                }

                return Json(DataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetSegmentedOverDueDocDataModalInfo(string parameterString, string segment, string documentType)
        {
            try
            {
                var segmentParameters = "";
                var and = "";
                if (parameterString == "")
                {
                    and = "WHERE ";
                }
                if (parameterString != "")
                {
                    and = "AND ";
                }
                string orderBy = "";
                string desiredOutputString = @"SELECT Count(dd.DocumentID) TotalOverDueDocument,dd.DocumentID, dd.DocumentType,dd.DocumentShortName,dd.DocumentUserName
                                               ,DD.DocumentCategoryId,DD.DocumentSubCategoryId,dd.DocumentSubCategoryName,dd.DocumentCategoryName,dd.CompletionStatus
                                            ,dd.DocumentationBy,dd.Importance,dd.DocumentStatus,dd.OptionalOrMandatory
                                  FROM(";
                if (segment.ToUpper() == "SEGMENT1")
                {
                    segmentParameters = and + @"DueDays > 0 and DueDays < 10 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' ";
                }
                if (segment.ToUpper() == "SEGMENT2")
                {
                    segmentParameters = and + @" DueDays >= 10 and DueDays < 30 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' ";
                }
                if (segment.ToUpper() == "SEGMENT3")
                {
                    segmentParameters = and + @" DueDays >= 30 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' ";
                }
                if (segment.ToUpper() == "OVERALL")
                {
                    segmentParameters = and + @"DueDays > 0 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' ";
                }
                var GroupBy = @"GROUP BY
                                    dd.DocumentType,dd.DocumentShortName,dd.DocumentUserName,dd.DocumentID
                                               ,DD.DocumentCategoryId,DD.DocumentSubCategoryId,dd.DocumentSubCategoryName,dd.DocumentCategoryName,dd.CompletionStatus
                                            ,dd.DocumentationBy,dd.Importance,dd.DocumentStatus,dd.OptionalOrMandatory";
                var DtList = GetMasterDataSqlList(desiredOutputString, parameterString, segmentParameters, GroupBy, orderBy);
                List<DocPieCharDocumentStatus> DataList = new List<DocPieCharDocumentStatus>();
                StringCollection str = new StringCollection();


                return Json(DtList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetOverAllOverDueEmployeeList(string parameterString, string documentType, string segment)
        {
            try
            {
                string segmentParameters = "";
                string and = "";
                if (parameterString == "")
                {
                    and = "WHERE ";
                }
                if (parameterString != "")
                {
                    and = "AND ";
                }
                if (segment.ToUpper() == "SEGMENT1")
                {
                    segmentParameters = and + @" DueDays > 0 and DueDays < 10 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' ";
                }
                if (segment.ToUpper() == "SEGMENT2")
                {
                    segmentParameters = and + @" DueDays >= 10 and DueDays < 30 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' ";
                }
                if (segment.ToUpper() == "SEGMENT3")
                {
                    segmentParameters = and + @" DueDays >= 30 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' ";
                }
                if (segment.ToUpper() == "OVERALL")
                {
                    segmentParameters = and + @" DueDays > 0 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' ";
                }
                string orderBy = "Order by EmployeeID";
                string desiredOutputString = @"SELECT DD.CandidateID,DD.BudgetCode,DD.EmployeeID,REPLACE(CONVERT(VARCHAR(11), DD.DOJ, 106), ' ', '-') AS DOJ, DD.EmpCatgId,DD.EmpCategory,DD.GivenDesignation
                                                ,DD.EmployeeCode,DD.EmployeeName,DD.CompanyId,DD.CompanyName,DD.PlantId,DD.PlantName
									, DD.DocumentCategoryName,DD.DocumentSubCategoryName,DD.DocumentUserName,DD.DocumentType,DD.DocumentationBy,DD.Importance,DD.OptionalOrMandatory
                                    , DD.DocumentID,DD.DocumentCategoryId,DD.DocumentSubCategoryId,DD.DocumentStatus,DD.CompletionStatus,DD.DueDays
								  FROM(";
                var dtEmployeeInfo = GetEmployeeData(desiredOutputString, parameterString, documentType, segmentParameters, orderBy);
                var dtDocumentType = _sqlRepository.GetDataTable("SELECT DISTINCT DocumentType FROM HKP.ComplianceDocument");
                StringCollection str = new StringCollection();
                List<EmployeeModel> DataList = new List<EmployeeModel>();

                string employeeId = "";
                string candidateId = "";


                employeeId = "";

                EmployeeModel model = null;
                for (int i = 0; i < dtEmployeeInfo.Rows.Count; i++)
                {
                    if (employeeId != dtEmployeeInfo.Rows[i]["EmployeeID"].ToString() || candidateId != dtEmployeeInfo.Rows[i]["CandidateID"].ToString() && dtEmployeeInfo.Rows[i]["DocumentStatus"].ToString().ToUpper() == "OVERDUE")
                    {
                        str = new StringCollection();

                        model = new EmployeeModel();
                        model.EmployeeId = dtEmployeeInfo.Rows[i]["EmployeeID"].ToString();
                        model.CandiateId = dtEmployeeInfo.Rows[i]["CandidateID"].ToString();
                        model.EmployeeName = dtEmployeeInfo.Rows[i]["EmployeeName"].ToString();
                        model.EmployeeCode = dtEmployeeInfo.Rows[i]["EmployeeCode"].ToString();
                        model.BudgetCode = dtEmployeeInfo.Rows[i]["BudgetCode"].ToString();
                        model.EmployeeCategory = dtEmployeeInfo.Rows[i]["EmpCategory"].ToString();
                        model.CompanyId = dtEmployeeInfo.Rows[i]["CompanyId"].ToString();
                        model.CompanyName = dtEmployeeInfo.Rows[i]["CompanyName"].ToString();
                        model.PlantId = dtEmployeeInfo.Rows[i]["PlantId"].ToString();
                        model.PlantName = dtEmployeeInfo.Rows[i]["PlantName"].ToString();
                        model.Designation = dtEmployeeInfo.Rows[i]["GivenDesignation"].ToString();
                        model.DOJ = dtEmployeeInfo.Rows[i]["DOJ"].ToString();
                        model.DocumentType = dtEmployeeInfo.Rows[i]["DocumentType"].ToString();

                        DataList.Add(model);

                        employeeId = dtEmployeeInfo.Rows[i]["EmployeeID"].ToString();
                        candidateId = dtEmployeeInfo.Rows[i]["CandidateID"].ToString();
                    }
                    if (dtEmployeeInfo.Rows[i]["DocumentStatus"].ToString().ToUpper() == "OVERDUE")
                    {
                        if (Convert.ToDouble(bplib.clsWebLib.GetNumData(dtEmployeeInfo.Rows[i]["DueDays"].ToString())) > 0)
                        {
                            model.TotalOverDueDocument++;
                            if (dtEmployeeInfo.Rows[i]["OptionalOrMandatory"].ToString().ToUpper() == "MANDATORY")
                                model.TotalOverDueMandatory++;
                            if (dtEmployeeInfo.Rows[i]["OptionalOrMandatory"].ToString().ToUpper() == "OPTIONAL")
                                model.TotalOverDueOptional++;

                        }

                    }
                }

                return Json(DataList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public class docDashBoardModel : BaseModel
        {
            public string DocumentType { get; set; } = "";
            public double TotalCompleted { get; set; } = 0;
            public double TotalOntimeCompletionPercentage { get; set; } = 0;
            public double TotalOntimeCompleted { get; set; } = 0;


            public double OverAllDueDoc { get; set; } = 0;
            public double OverAllDueEmp { get; set; } = 0;

            public double Seg1Doc { get; set; } = 0;//OverDued DocNumber between 1 to 10 days
            public double Seg1Emp { get; set; } = 0;//OverDued EmpNumber between 1 to 10 days

            public double Seg2Doc { get; set; } = 0;//OverDued DocNumber between 10 to 30 days
            public double Seg2Emp { get; set; } = 0;//OverDued EmpNumer between 10 to 30 days

            public double Seg3Doc { get; set; } = 0;//OverDued DocNumber between 30 to 40 days
            public double Seg3Emp { get; set; } = 0;//OverDued EmpNumber between 30 to 40 days

            public double TotalDue { get; set; } = 0;// Dued DocNumber

        }

        public class DocBarChartStatus : BaseModel
        {
            public string ComplianceDocumentId { get; set; }
            public string ComplianceDocumentUserName { get; set; }
            public string ComplianceDocumentShortName { get; set; }
            public string ComplianceDocumentCategory { get; set; }
            public string ComplianceDocumentSubCategory { get; set; }
            public string ComplianceDocumentType { get; set; }
            public string ComplianceDocumentionBy { get; set; }
            public string ComplianceDocumentResponsiblePerson { get; set; }
            public string ComplianceDocumentImportance { get; set; }
            public string ComplianceDocumentOptionalOrMandatory { get; set; }

            public double TotalOverDueDocument { get; set; } = 0;
            public double TotalDueDocument { get; set; } = 0;

        }

        public class DocPieCharDocumentStatus : BaseModel
        {
            public string ComplianceDocumentId { get; set; }
            public string ComplianceDocumentUserName { get; set; }
            public string ComplianceDocumentShortName { get; set; }
            public string ComplianceDocumentCategory { get; set; }
            public string ComplianceDocumentSubCategory { get; set; }
            public string ComplianceDocumentType { get; set; }
            public string ComplianceDocumentionBy { get; set; }
            public string ComplianceDocumentResponsiblePerson { get; set; }
            public string ComplianceDocumentImportance { get; set; }
            public string ComplianceDocumentOptionalOrMandatory { get; set; }
            public string DueDays { get; set; }
            public double TotalOverDueDocument { get; set; } = 0;
            public double TotalDueDocument { get; set; } = 0;
            public double TotalCompletedDocument { get; set; } = 0;


        }

        public class EmployeeModel : BaseModel
        {
            public string DocumentType { get; set; }

            public string CompanyId { get; set; }
            public string CompanyName { get; set; }

            public string PlantId { get; set; }
            public string PlantName { get; set; }

            public string CandiateId { get; set; }//PreRecruiteMent Id

            public string EmployeeId { get; set; }
            public string CandidateId { get; set; }
            public string EmployeeCode { get; set; }
            public string EmployeeName { get; set; }
            public string BudgetCode { get; set; }
            public string EmployeeCategory { get; set; }
            public string Designation { get; set; }
            public string DOJ { get; set; }
            public int TotalOverDueDocument { get; set; } = 0;
            public int TotalOverDueMandatory { get; set; } = 0;

            public int TotalOverDueOptional { get; set; } = 0;
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeWiseOptionalOrMandatoryDocumentList(string parameterString, string employeeId, string documentType, string OptionalOrMandatory, string segment)
        {
            try
            {
                string segmentParameters = "";
                var and = "";
                if (parameterString == "")
                {
                    and = "WHERE";
                }
                if (parameterString != "")
                {
                    and = "AND";
                }
                if (segment.ToUpper() == "SEGMENT1")
                {
                    segmentParameters = and + @" DueDays > 0 and DueDays < 10 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' AND EmployeeID ='" + employeeId + "'  AND OptionalOrMandatory = '" + OptionalOrMandatory + "'";
                }
                if (segment.ToUpper() == "SEGMENT2")
                {
                    segmentParameters = and + @" DueDays >= 10 and DueDays < 30 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' AND EmployeeID ='" + employeeId + "'  AND OptionalOrMandatory = '" + OptionalOrMandatory + "' ";
                }
                if (segment.ToUpper() == "SEGMENT3")
                {
                    segmentParameters = and + @" DueDays >= 30 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' AND EmployeeID ='" + employeeId + "'  AND OptionalOrMandatory = '" + OptionalOrMandatory + "' ";
                }
                if (segment.ToUpper() == "OVERALL")
                {
                    if (parameterString != "")
                    {
                        segmentParameters = @"AND DueDays > 0 and DocumentStatus = 'OverDue'
                                        AND DocumentType = '" + documentType + "' AND EmployeeID ='" + employeeId + "' AND OptionalOrMandatory = '" + OptionalOrMandatory + "' ";

                    }
                    else
                    {
                        segmentParameters = @"WHERE DueDays > 0 and DocumentStatus = 'OverDue'
                                        AND DocumentType = '" + documentType + "' AND EmployeeID ='" + employeeId + "' AND OptionalOrMandatory = '" + OptionalOrMandatory + "' ";

                    }

                }
                string orderBy = " Order by DocumentID";
                string desiredOutputString = @"SELECT DD.BudgetCode,DD.EmployeeID,REPLACE(CONVERT(VARCHAR(11), DD.DOJ, 106), ' ', '-') AS DOJ, DD.EmpCatgId,DD.EmpCategory,DD.GivenDesignation
                                                ,DD.EmployeeCode,DD.EmployeeName,DD.CompanyId,DD.CompanyName,DD.PlantId,DD.PlantName
									, DD.DocumentCategoryName,DD.DocumentSubCategoryName,DD.DocumentUserName,DD.DocumentType,DD.DocumentationBy,DD.Importance,DD.OptionalOrMandatory
                                    , DD.DocumentID,DD.DocumentCategoryId,DD.DocumentSubCategoryId,DD.DocumentStatus,DD.CompletionStatus,DD.DueDays
								  FROM(";
                string sqlText = desiredOutputString + baseQueryForEmployee + ") AS dd " + parameterString + segmentParameters + orderBy;

                return Json(_sqlRepository.GetDataCollection(sqlText, null), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public DataTable GetMasterDataSql(string desiredOutputString, string prameterString, string segmentParameters, string GroupBy, string orderBy)
        {
            string sqlText = desiredOutputString + baseQuery +
                                    ") AS dd " + prameterString + segmentParameters + GroupBy + orderBy + " ";
            return _sqlRepository.GetDataTable(sqlText);
        }
        public IEnumerable<object> GetMasterDataSqlList(string desiredOutputString, string prameterString, string segmentParameters, string GroupBy, string orderBy)
        {
            string sqlText = desiredOutputString + baseQuery +
                                    ") AS dd " + prameterString + segmentParameters + GroupBy + orderBy + " ";
            return _sqlRepository.GetDataCollection(sqlText, null);
        }

        [HttpPost, Authorize]
        public ActionResult GetDocumentWiseEmployeeList(string parameterString, string documentId, string segment, string documentType, string OptionalOrMandatory)
        {
            try
            {
                string segmentParameters = "";
                string and = "";
                if (parameterString == "")
                {
                    and = "WHERE ";
                }
                if (parameterString != "")
                {
                    and = "AND ";
                }
                if (segment.ToUpper() == "SEGMENT1")
                {
                    segmentParameters = and + @" DueDays > 0 and DueDays < 10 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "' AND DocumentID = '" + documentId + "' AND OptionalOrMandatory='" + OptionalOrMandatory + "' ";
                }
                if (segment.ToUpper() == "SEGMENT2")
                {
                    segmentParameters = and + @" DueDays >= 10 and DueDays < 30 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "'  AND DocumentID = '" + documentId + "' AND OptionalOrMandatory='" + OptionalOrMandatory + "'";
                }
                if (segment.ToUpper() == "SEGMENT3")
                {
                    segmentParameters = and + @" DueDays >= 30 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "'  AND DocumentID = '" + documentId + "' AND OptionalOrMandatory='" + OptionalOrMandatory + "'";
                }
                if (segment.ToUpper() == "OVERALL")
                {
                    segmentParameters = and + @" DueDays > 0 and DocumentStatus = 'OverDue' AND DocumentType = '" + documentType + "'  AND DocumentID = '" + documentId + "' AND OptionalOrMandatory='" + OptionalOrMandatory + "'";
                }
                string orderBy = "Order by EmployeeID";
                string desiredOutputString = @"SELECT Distinct DD.CandidateID, DD.BudgetCode,DD.EmployeeID,REPLACE(CONVERT(VARCHAR(11), DD.DOJ, 106), ' ', '-') AS DOJ,
                                                    DD.EmpCatgId,DD.EmpCategory,DD.GivenDesignation
                                                ,DD.EmployeeCode,DD.EmployeeName,DD.CompanyId,DD.CompanyName,DD.PlantId,DD.PlantName

								  FROM(";
                string sqlText = desiredOutputString + baseQueryForEmployee + ") AS dd " + parameterString + segmentParameters + orderBy;

                return Json(_sqlRepository.GetDataCollection(sqlText, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetCompleteAndDueDocumentWiseEmployeeList(string parameterString, string documentId, string segment, string documentType, string OptionalOrMandatory)
        {
            try
            {
                string segmentParameters = "";
                string and = "";
                if (parameterString == "")
                {
                    and = "WHERE ";
                }
                if (parameterString != "")
                {
                    and = "AND ";
                }

                if (segment.ToUpper() == "COMPLETE")
                {
                    segmentParameters = and + @"ISNULL(CompletionStatus,'')<>''  AND ISNULL(DocumentType,'') = '" + documentType + "' AND ISNULL(DocumentID,'') = '" + documentId + "' AND ISNULL(OptionalOrMandatory,'') = '" + OptionalOrMandatory + "' ";
                }
                if (segment.ToUpper() == "DUE")
                {
                    segmentParameters = and + @"DocumentStatus = 'Due' AND ISNULL(DocumentType,'') = '" + documentType + "'  AND ISNULL(DocumentID,'') = '" + documentId + "' AND ISNULL(OptionalOrMandatory,'') ='" + OptionalOrMandatory + "'";
                }
                if (segment.ToUpper() == "OVERDUE")
                {
                    segmentParameters = and + @"DocumentStatus = 'OverDue' AND ISNULL(DocumentType,'') = '" + documentType + "'  AND ISNULL(DocumentID,'') = '" + documentId + "' AND ISNULL(OptionalOrMandatory,'') ='" + OptionalOrMandatory + "'";
                }
                string orderBy = "Order by EmployeeID";
                string desiredOutputString = @"SELECT Distinct DD.BudgetCode,DD.EmployeeID,REPLACE(CONVERT(VARCHAR(11), DD.DOJ, 106), ' ', '-') AS DOJ,
                                                    DD.EmpCatgId,DD.EmpCategory,DD.GivenDesignation
                                                ,DD.EmployeeCode,DD.EmployeeName,DD.CompanyId,DD.CompanyName,DD.PlantId,DD.PlantName

								  FROM(";
                string sqlText = desiredOutputString + baseQueryForEmployee + ") AS dd " + parameterString + segmentParameters + orderBy;

                return Json(_sqlRepository.GetDataCollection(sqlText, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion
    }
}
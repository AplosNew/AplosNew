using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeProfileApprovalController : BaseController
    {
        #region Constructor

        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeResponsiblePersonService _employeeResponsiblePersonService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly IEmpReferenceInformationService _empReferenceInformationService;
        private readonly IEmpAcademicQualificationInformationService _empAcademicQualificationInformationService;
        private readonly IEmpExperienceInformationService _empExperienceInformationService;
        private readonly IEmpTrainingInformationService _empTrainingInformationService;
        private readonly IEmployeeDocumentService _employeeDocumentService;

        public EmployeeProfileApprovalController(
              IEmployeeInformationService employeeInformationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeResponsiblePersonService employeeResponsiblePersonService
            , IEmployeeProfileService employeeProfileService
            , IEmpReferenceInformationService empReferenceInformationService
            , IEmpAcademicQualificationInformationService empAcademicQualificationInformationService
            , IEmpExperienceInformationService empExperienceInformationService
            , IEmpTrainingInformationService empTrainingInformationService
            , IEmployeeDocumentService employeeDocumentService)
        {
            _employeeInformationService = employeeInformationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _employeeResponsiblePersonService = employeeResponsiblePersonService;
            _employeeProfileService = employeeProfileService;
            _empReferenceInformationService = empReferenceInformationService;
            _empAcademicQualificationInformationService = empAcademicQualificationInformationService;
            _empExperienceInformationService = empExperienceInformationService;
            _empTrainingInformationService = empTrainingInformationService;
            _employeeDocumentService = employeeDocumentService;
        }

        #endregion Constructor
        EmployeeProfile employeeProfile = new EmployeeProfile();

        #region Pages

        [Authorize]
        public ActionResult EmployeeProfileApproval()
        {
            return View();
        }

   
        #endregion Pages

        #region EmployeeResponsiblePerson

        #region BudgetMaster

        //[HttpGet, Authorize]
        //public ActionResult BudgetMaster()
        //{
        //    return View();
        //}

        //[HttpGet, Authorize]
        //public JsonResult BudgetMasterResponsiblePerson(string employeeId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_employeeResponsiblePersonService.QueryBudgetMaster(identity.CompanyGroupId, employeeId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public JsonResult QueryBudgetMasterPopUp(GridParameter parameters, string employeeId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_employeeResponsiblePersonService.QueryBudgetMaster(parameters, identity.CompanyGroupId, employeeId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public JsonResult QueryBudgetMasterResponsiblePerson(GridParameter parameters, string employeeId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_employeeResponsiblePersonService.QueryBudgetMasterResponsiblePerson(parameters, identity.CompanyGroupId, employeeId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost, Authorize]
        //public JsonResult SaveBudgetMaster(IEnumerable<EmployeeResponsiblePerson> entityList, IEnumerable<EmployeeResponsiblePerson> activityList)
        //{
        //    _employeeResponsiblePersonService.SaveBudgetMaster(entityList, activityList);
        //    return Json(new { Message = AplosMessage.Success });
        //}

        #endregion BudgetMaster

        #region BudgetMasterActivity

        //[HttpGet, Authorize]
        //public ActionResult BudgetMasterActivity()
        //{
        //    return View();
        //}

        //[HttpGet, Authorize]
        //public JsonResult BudgetMasterActivityResponsiblePerson(string employeeId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_employeeResponsiblePersonService.QueryBudgetMasterActivity(identity.CompanyGroupId, employeeId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public JsonResult BudgetMasterActivityResponsiblePersonPopUp(GridParameter parameters, string employeeId, string budgetMasterId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_employeeResponsiblePersonService.QueryBudgetMasterActivity(parameters, identity.CompanyGroupId, employeeId, budgetMasterId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost, Authorize]
        //public JsonResult SaveBudgetMasterActivity(EmployeeResponsiblePerson entity)
        //{
        //    _employeeResponsiblePersonService.SaveBudgetMasterActivity(entity);
        //    return Json(new { Message = AplosMessage.Success });
        //}

        #endregion BudgetMasterActivity

        #region BudgetMasterActivityPhone

        //[HttpGet, Authorize]
        //public ActionResult BudgetMasterActivityPhone()
        //{
        //    return View();
        //}

        //[HttpGet, Authorize]
        //public JsonResult BudgetMasterActivityPhoneResponsiblePerson(GridParameter parameters, string employeeId, string budgetMasterId, string activityId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_employeeResponsiblePersonService.QueryBudgetMasterActivityPhone(parameters, identity.CompanyGroupId, employeeId, budgetMasterId, activityId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost, Authorize]
        //public JsonResult SaveBudgetMasterActivityPhone(EmployeeResponsiblePerson entity)
        //{
        //    _employeeResponsiblePersonService.SaveBudgetMasterActivityPhone(entity);
        //    return Json(new { Message = AplosMessage.Success });
        //}

        #endregion BudgetMasterActivityPhone

        #endregion EmployeeResponsiblePerson

        #region EmployeeInformation

        //[HttpPost]
        //public JsonResult Create(FormCollection form)
        //{
        //    var pre = form["employeeInformation"];
        //    var settings = new JsonSerializerSettings
        //    {
        //        NullValueHandling = NullValueHandling.Ignore,
        //        MissingMemberHandling = MissingMemberHandling.Ignore
        //    };
        //    var employee = JsonConvert.DeserializeObject<EmployeeInformation>(pre, settings);
        //    var file = Request.Files["file"];
        //    if (file != null)
        //    {
        //        var extension = Path.GetExtension(file.FileName);
        //        if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
        //        {
        //            employee.EmpPicPath = extension;
        //            if (!string.IsNullOrEmpty(employee.EmpPicPath))
        //                employee.EmpPicPath = employee.SystemId + employee.EmpPicPath;
        //        }
        //        else
        //            throw new CustomException(Resources.ImageUploadError);
        //    }
        //    _employeeProfileService.UpdateMaster(employee);
        //    if (file != null)
        //    {
        //        var path = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeImage, typeof(string)).ToString())*/, employee.EmpPicPath);
        //        if (System.IO.File.Exists(path))
        //        {
        //            System.IO.File.Delete(path);
        //            file.SaveAs(path);
        //        }
        //        else
        //        {
        //            file.SaveAs(path);
        //        }
        //    }
        //    return Json(new { EmployeeInformation = employee, Message = AplosMessage.Success });
        //}

        //[HttpPost]
        //public JsonResult CreatePersonal(EmployeeInformation employeeInformation)
        //{
        //    _employeeProfileService.UpdatePersonal(employeeInformation);
        //    return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Success });
        //}

        //[HttpPost]
        //public JsonResult CreateAddress(EmployeeInformation employeeInformation)
        //{
        //    _employeeProfileService.UpdateAddress(employeeInformation);
        //    return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Success });
        //}

        //[HttpPost]
        //public JsonResult CreateAdvanceInfo(EmployeeInformation employeeInformation)
        //{
        //    _employeeProfileService.UpdateAdvanceInfo(employeeInformation);
        //    return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Success });
        //}

        //[HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        //public JsonResult CreateReference(EmpReferenceInformation empReferenceInformation)
        //{
        //    _empReferenceInformationService.InsertOrUpdate(empReferenceInformation);
        //    return Json(new { EmpReferenceInformation = empReferenceInformation, Message = AplosMessage.Success });
        //}

        //[HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        //public JsonResult CreateQualification(FormCollection form, HttpPostedFileBase[] file)
        //{
        //    var empAcademicQualificationInformation = new JavaScriptSerializer().Deserialize<EmpAcademicQualificationInformation>(form["empAcademicQualificationInformation"]);

        //    var directory = ResourcesPathReader.GetQualificationDestinationPath();
        //    var path = Path.Combine(directory);

        //    if (file.IsNotNull())
        //    {
        //        for (int i = 0; i < file.Length; i++)
        //        {
        //            ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
        //        }
        //    }

        //    var fileId = "";
        //    var fileName = "";
        //    var filedata = _empAcademicQualificationInformationService.GetQualificationFile(empAcademicQualificationInformation.SystemID);
        //    if (filedata.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
        //            !string.IsNullOrEmpty(filedata["FileName"].ToString()))
        //            fileId = filedata["FileId"].ToString();
        //        fileName = filedata["FileName"].ToString();

        //        if (fileName != empAcademicQualificationInformation.FileName)
        //            if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //                System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }

        //    _empAcademicQualificationInformationService.InsertORUpdateMaster(empAcademicQualificationInformation);
        //    if (file.IsNotNull())
        //    {
        //        foreach (var item in file)
        //        {
        //            if (item != null)
        //            {
        //                if (System.IO.File.Exists(path + item.FileName))
        //                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
        //                item.SaveAs(path + empAcademicQualificationInformation.SystemID + Path.GetExtension(item.FileName));
        //            }
        //        }
        //    }
        //    return Json(new { EmpAcademicQualificationInformation = empAcademicQualificationInformation, Message = AplosMessage.Success });
        //}

        //[HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        //public JsonResult CreateExperience(FormCollection form, HttpPostedFileBase[] file)
        //{
        //    var settings = new JsonSerializerSettings
        //    {
        //        NullValueHandling = NullValueHandling.Ignore,
        //        MissingMemberHandling = MissingMemberHandling.Ignore
        //    };
        //    var empExperienceInformation = JsonConvert.DeserializeObject<EmpExperienceInformation>(form["empExperienceInformation"], settings);

        //    var directory = ResourcesPathReader.GetExperienceDestinationPath();
        //    var path = Path.Combine(directory);
        //    if (file.IsNotNull())
        //    {
        //        for (int i = 0; i < file.Length; i++)
        //        {
        //            ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
        //        }
        //    }
        //    var fileId = "";
        //    var fileName = "";
        //    var filedata = _empExperienceInformationService.GetExperienceFile(empExperienceInformation.SystemID);
        //    if (filedata.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
        //            !string.IsNullOrEmpty(filedata["FileName"].ToString()))
        //            fileId = filedata["FileId"].ToString();
        //        fileName = filedata["FileName"].ToString();

        //        if (fileName != empExperienceInformation.FileName)
        //            if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //                System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }

        //    _empExperienceInformationService.InsertORUpdateMaster(empExperienceInformation);
        //    if (file.IsNotNull())
        //    {
        //        foreach (var item in file)
        //        {
        //            if (item != null)
        //            {
        //                if (System.IO.File.Exists(path + item.FileName))
        //                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
        //                item.SaveAs(path + empExperienceInformation.SystemID + Path.GetExtension(item.FileName));
        //            }
        //        }
        //    }
        //    return Json(new { EmpExperienceInformation = empExperienceInformation, Message = AplosMessage.Success });
        //}

        //[HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        //public JsonResult CreateTraining(FormCollection form, HttpPostedFileBase[] file)
        //{
        //    var empTrainingInformation = new JavaScriptSerializer().Deserialize<EmpTrainingInformation>(form["empTrainingInformation"]);

        //    var directory = ResourcesPathReader.GetTrainingDestinationPath();
        //    var path = Path.Combine(directory);
        //    if (file.IsNotNull())
        //    {
        //        for (int i = 0; i < file.Length; i++)
        //        {
        //            ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
        //        }
        //    }
        //    var fileId = "";
        //    var fileName = "";
        //    var filedata = _empTrainingInformationService.GetTrainingFile(empTrainingInformation.SystemID);
        //    if (filedata.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
        //            !string.IsNullOrEmpty(filedata["FileName"].ToString()))
        //            fileId = filedata["FileId"].ToString();
        //        fileName = filedata["FileName"].ToString();

        //        if (fileName != empTrainingInformation.FileName)
        //            if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //                System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }

        //    _empTrainingInformationService.InsertORUpdateMaster(empTrainingInformation);

        //    if (file.IsNotNull())
        //    {
        //        foreach (var item in file)
        //        {
        //            if (item != null)
        //            {
        //                if (System.IO.File.Exists(path + item.FileName))
        //                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
        //                item.SaveAs(path + empTrainingInformation.SystemID + Path.GetExtension(item.FileName));
        //            }
        //        }
        //    }
        //    return Json(new { EmpTrainingInformation = empTrainingInformation, Message = AplosMessage.Success });
        //}

        //public JsonResult DeleteQualification(string id)
        //{
        //    var directory = ResourcesPathReader.GetQualificationDestinationPath();
        //    var path = Path.Combine(directory);
        //    var fileId = "";
        //    var fileName = "";
        //    var data = _empAcademicQualificationInformationService.GetQualificationFile(id);
        //    if (data.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
        //        !string.IsNullOrEmpty(data["FileName"].ToString()))
        //            fileId = data["FileId"].ToString();
        //        fileName = data["FileName"].ToString();
        //        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }
        //    _empAcademicQualificationInformationService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}

        //public JsonResult DeleteExperience(string id)
        //{
        //    var directory = ResourcesPathReader.GetExperienceDestinationPath();
        //    var path = Path.Combine(directory);
        //    var fileId = "";
        //    var fileName = "";
        //    var data = _empExperienceInformationService.GetExperienceFile(id);
        //    if (data.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
        //    !string.IsNullOrEmpty(data["FileName"].ToString()))
        //            fileId = data["FileId"].ToString();
        //        fileName = data["FileName"].ToString();
        //        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }
        //    _empExperienceInformationService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}

        //public JsonResult DeleteTraining(string id)
        //{
        //    var directory = ResourcesPathReader.GetTrainingDestinationPath();
        //    var path = Path.Combine(directory);
        //    var fileId = "";
        //    var fileName = "";
        //    var data = _empTrainingInformationService.GetTrainingFile(id);
        //    if (data.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
        //    !string.IsNullOrEmpty(data["FileName"].ToString()))
        //            fileId = data["FileId"].ToString();
        //        fileName = data["FileName"].ToString();
        //        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }
        //    _empTrainingInformationService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}

        //public JsonResult DeleteDocument(string id)
        //{
        //    var directory = ResourcesPathReader.GetDocumentDestinationPath();
        //    var path = Path.Combine(directory);
        //    var fileId = "";
        //    var fileName = "";
        //    var data = _employeeDocumentService.GetDocFile(id);
        //    if (data.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
        //        !string.IsNullOrEmpty(data["FileName"].ToString()))
        //            fileId = data["FileId"].ToString();
        //        fileName = data["FileName"].ToString();

        //        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }
        //    _employeeDocumentService.UpdateEmployeeDocument(id);

        //    return Json(new { Message = "File detach successfully." });
        //}

        //public JsonResult DeleteSingleDocument(string id)
        //{
        //    var directory = ResourcesPathReader.GetDocumentDestinationPath();
        //    var path = Path.Combine(directory);
        //    var fileId = "";
        //    var fileName = "";
        //    var data = _employeeDocumentService.GetDocFile(id);
        //    var fName = data["FileName"].ToString();
        //    if (!string.IsNullOrEmpty(fName))
        //    {
        //        throw new CustomException("This document cannot be deleted.");
        //    }
        //    else
        //    {
        //        if (data.Count > 0)
        //        {
        //            if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
        //            !string.IsNullOrEmpty(data["FileName"].ToString()))
        //                fileId = data["FileId"].ToString();
        //            fileName = data["FileName"].ToString();

        //            if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //                System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //        }
        //        _employeeDocumentService.DeleteEmployeeDocument(id);
        //    }
        //    return Json(new { Message = AplosMessage.Deleted });
        //}

        //[HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        //public JsonResult CreateDocument(FormCollection form, HttpPostedFileBase[] file)
        //{
        //    var employeeDocument = new JavaScriptSerializer().Deserialize<EmployeeDocument>(form["employeeDocument"]);

        //    var directory = ResourcesPathReader.GetDocumentDestinationPath();
        //    var path = Path.Combine(directory);
        //    if (file.IsNotNull())
        //    {
        //        for (int i = 0; i < file.Length; i++)
        //        {
        //            ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
        //        }
        //    }
        //    var fileId = "";
        //    var fileName = "";
        //    var filedata = _employeeDocumentService.GetDocFile(employeeDocument.Id);
        //    if (filedata.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
        //            !string.IsNullOrEmpty(filedata["FileName"].ToString()))
        //            fileId = filedata["FileId"].ToString();
        //        fileName = filedata["FileName"].ToString();

        //        if (fileName != employeeDocument.FileName)
        //            if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //                System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }

        //    _employeeDocumentService.InsertORUpdateMaster(employeeDocument);
        //    if (file.IsNotNull())
        //    {
        //        foreach (var item in file)
        //        {
        //            if (item != null)
        //            {
        //                if (System.IO.File.Exists(path + item.FileName))
        //                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
        //                item.SaveAs(path + employeeDocument.Id + Path.GetExtension(item.FileName));
        //            }
        //        }
        //    }
        //    return Json(new { EmployeeDocument = employeeDocument, Message = AplosMessage.Success });
        //}

        //private static string GetDoc(List<EmployeeDocument> doc, string fileName)
        //{
        //    return doc.Find(r => r.FileName == fileName).ComplianceDocumentId;
        //}

        //private static string GetFileId(IEnumerable<EmployeeDocument> list, string fileName)
        //{
        //    foreach (var item in list)
        //    {
        //        if (item.FileName == fileName)
        //        {
        //            return item.Id;
        //        }
        //    }
        //    return "";
        //}

        //private static string GetFileName(IEnumerable<PreRecruitmentDocument> list, string fileid)
        //{
        //    foreach (var item in list)
        //    {
        //        if (item.FileId == fileid)
        //        {
        //            return item.FileName;
        //        }
        //    }
        //    return "";
        //}

        //[HttpGet, Authorize]
        //public JsonResult GetEmployeeCbo()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_employeeProfileService.GetEmployeeCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        //}

        #endregion EmployeeInformation

        #region Report

        //[HttpGet, Authorize]
        //public ActionResult JobCardReport(string fromDate, string toDate)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var fileName = "Job Card" + DateTime.Now.ToString("ddMMMyyyy") + "";
        //    var workbook = _employeeProfileService.JobCard_Report(identity.EmployeeId, fromDate, toDate, identity.CompanyGroupId);
        //    workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
        //    return null;
        //}

        //[HttpGet, Authorize]
        //public ActionResult EmployeeInfoReport(string employeeId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var fileName = "Employee Information" + DateTime.Now.ToString("ddMMMyyyy") + "";
        //    var workbook = _employeeProfileService.EmpInfoReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, employeeId);
        //    workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
        //    return null;
        //}

        //[HttpGet, Authorize]
        //public ActionResult EmpRegisterInfo()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var fileName = "Employee Register Information" + DateTime.Now.ToString("ddMMMyyyy") + "";
        //    var workbook = _employeeProfileService.EmpRegisterReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
        //    workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
        //    return null;
        //}

        //[HttpGet, Authorize]
        //public JsonResult GetClanderYear()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var data = _employeeProfileService.GetClanderYear(identity.PlantId);
        //    return Json(new { data }, JsonRequestBehavior.AllowGet);
        //}

        #endregion Report

        #region Mediasoft Fair Shop Data Export
        //public ActionResult MediasoftFairShopEmpDataExport()
        //{
        //    //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //  DataTable data=  _employeeProfileService.MediasoftFairShopDataExport();//, strPathHindi, strPathEnglish, strPathBangla);

        //    //LetterType.
        //    // WriteDataTableToCSV(data, "FairShop");
        //    var excelEngine = new ExcelEngine();
        //    var application = excelEngine.Excel;
        //    var workbook = application.Workbooks.Create(3);
        //    var sheet1 = workbook.Worksheets[0];

        //    sheet1.ImportDataTable(data,true, 1, 1);
            
        //    workbook.Version = ExcelVersion.Excel2013;


        //    workbook.SaveAs(DateTime.Now.ToString("yyMMdd") + " FairShop.csv", ",", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog,ExcelHttpContentType.CSV);


        //    return null;
        //}


        public static void WriteDataTableToCSV(DataTable dt, string fileName)
        {
            WriteOutCSVResponseHeaders(fileName);
            WriteOutDataTable(dt);
            System.Web.HttpContext.Current.Response.End();
        }


        /// <summary>
        /// Writes out the response headers needed for outputting a CSV file.
        /// </summary>
        /// <param name="fileName">File name for the outputted file</param>
        public static void WriteOutCSVResponseHeaders(string fileName)
        {
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.ClearHeaders();
            System.Web.HttpContext.Current.Response.ClearContent();
            System.Web.HttpContext.Current.Response.AddHeader("content-disposition", string.Format("attachment; filename={0}-{1}.csv", fileName, DateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss")));
            System.Web.HttpContext.Current.Response.AddHeader("Pragma", "public");
            System.Web.HttpContext.Current.Response.ContentType = "text/csv";
            System.Web.HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
        }


        /// <summary>
        /// Writes out the header row and data rows from a data table.
        /// </summary>
        /// <param name="dt">DataTable which holds the data</param>
        public static void WriteOutDataTable(DataTable dt)
        {
            WriteOutHeaderRow(dt, dt.Columns.Count);
            WriteOutDataRows(dt, dt.Columns.Count, dt.Rows.Count);
        }

        /// <summary>
        /// Writes the header row from a datatable as Http Response
        /// </summary>
        /// <param name="dt">DataTable which holds the data</param>
        /// <param name="colCount">Number of columns</param>
        private static void WriteOutHeaderRow(DataTable dt, int colCount)
        {
            string CSVHeaderRow = string.Empty;
            for (int col = 0; col <= colCount - 1; col++)
            {
                CSVHeaderRow = string.Format("{0}\"{1}\",", CSVHeaderRow, dt.Columns[col].ColumnName);
            }
            WriteRow(CSVHeaderRow);
        }

        /// <summary>
        /// Writes the data rows of a datatable as Http Responses
        /// </summary>
        /// <param name="dt">DataTable which holds the data</param>
        /// <param name="colCount">Number of columns</param>
        /// <param name="rowCount">Number of columns</param>
        private static void WriteOutDataRows(DataTable dt, int colCount, int rowCount)
        {
            string CSVDataRow = string.Empty;
            for (int row = 0; row <= rowCount - 1; row++)
            {
                var dataRow = dt.Rows[row];
                CSVDataRow = string.Empty;
                for (int col = 0; col <= colCount - 1; col++)
                {
                    CSVDataRow = string.Format("{0}\"{1}\",", CSVDataRow, dataRow[col]);
                }
                WriteRow(CSVDataRow);
            }
        }

        /// <summary>
        /// Write out a row as an Http Response.
        /// </summary>
        /// <param name="row">The data row to write out</param>
        private static void WriteRow(string row)
        {
            //System.Web.HttpContext.Current.Response.Write(row.TrimEnd(","));
            System.Web.HttpContext.Current.Response.Write(row.TrimEnd(','));
            System.Web.HttpContext.Current.Response.Write(Environment.NewLine);
        }

        #endregion

        #region Lock and Un-Lock
        //[HttpPost, Authorize]
        //public JsonResult CreateLockData( string lockDate)
        //{
            
        //    _employeeProfileService.CreateLockData( lockDate);
        //    return Json(new { Message = AplosMessage.Success });
        //}
        #endregion

        #region Employee Approval

        [HttpGet]
        public JsonResult GetUnApprovedEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //JsonResult json = Json(_employeeProfileService.GetUnApprovedEmployeeList(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(employeeProfile.GetUnApprovedEmployeeList(identity.CompanyGroupId, identity.PlantId,identity.IsSysAdmin,identity.UserId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpPost]
        public ActionResult SaveApprovedEmployee( IEnumerable<ParaEmployeeInformation> employeeInformation)
        {
            DataSet dsEmployeeOTInformation = Library.Service.Helpers.DataTableExtensions.ToDataSet<ParaEmployeeInformation>(employeeInformation);
            //DataSet dsEmployeeOTInformation = null;
            _employeeProfileService.SaveApprovedEmployeeData(dsEmployeeOTInformation);
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
    public class ParaEmployeeInformation
    {
        public bool CheckBoxSelect { get; set; }
        public string SystemId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DOBs { get; set; }
        public string DOJs { get; set; }
        public string Department { get; set; }
        public string PositionName { get; set; }
        public string EntityName { get; set; }
        public string Designation { get; set; }
        public string Section { get; set; }
        public string LegalDesignation { get; set; }
        public string SubSection { get; set; }
        public string Code { get; set; }
      
    }
}
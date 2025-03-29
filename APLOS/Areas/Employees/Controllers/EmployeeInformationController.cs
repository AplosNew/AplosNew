using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Aplos.HumanResource;
using bplib;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeInformationController : BaseController
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
        private readonly ISqlRepository _sqlRepository;
        public EmployeeInformationController(
              IEmployeeInformationService employeeInformationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeResponsiblePersonService employeeResponsiblePersonService
            , IEmployeeProfileService employeeProfileService
            , IEmpReferenceInformationService empReferenceInformationService
            , IEmpAcademicQualificationInformationService empAcademicQualificationInformationService
            , IEmpExperienceInformationService empExperienceInformationService
            , IEmpTrainingInformationService empTrainingInformationService
            , IEmployeeDocumentService employeeDocumentService
            , ISqlRepository R)
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
            _sqlRepository = R;
        }

        #endregion Constructor

        EmployeeProfile employeeProfile = new EmployeeProfile();

        #region Pages

        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet]
        public ActionResult IndividualComplianceReport()
        {
            return View();
        }


        [Authorize, HttpGet]
        public ActionResult ProfileView()
        {
            return View();
        }
        [Authorize]
        public ActionResult Aplos1()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult JobCard()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult EmpInfo()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult EmpRegister()
        {
            return View();
        }


        public ActionResult EmployeeDocumentAddRemove()
        {
            return View();
        }



        public ActionResult MediasoftFairShopDataExport()
        {
            return View();
        }

        [Authorize]
        public ActionResult EmployeeLockAndUnLock()
        {
            return View();
        }
        #endregion Pages

        #region EmployeeResponsiblePerson

        #region BudgetMaster

        [HttpGet, Authorize]
        public ActionResult BudgetMaster()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult BudgetMasterResponsiblePerson(string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeResponsiblePersonService.QueryBudgetMaster(identity.CompanyGroupId, employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult QueryBudgetMasterPopUp(GridParameter parameters, string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeResponsiblePersonService.QueryBudgetMaster(parameters, identity.CompanyGroupId, employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult QueryBudgetMasterResponsiblePerson(GridParameter parameters, string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeResponsiblePersonService.QueryBudgetMasterResponsiblePerson(parameters, identity.CompanyGroupId, employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveBudgetMaster(string entityList, string activityList)
        {

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<EmployeeResponsiblePerson> budget = JsonConvert.DeserializeObject<List<EmployeeResponsiblePerson>>(entityList, settings);
            List<EmployeeResponsiblePerson> activity = JsonConvert.DeserializeObject<List<EmployeeResponsiblePerson>>(activityList, settings);

            _employeeResponsiblePersonService.SaveBudgetMaster(budget, activity);
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
        public JsonResult BudgetMasterActivityResponsiblePerson(string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeResponsiblePersonService.QueryBudgetMasterActivity(identity.CompanyGroupId, employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult BudgetMasterActivityResponsiblePersonPopUp(GridParameter parameters, string employeeId, string budgetMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeResponsiblePersonService.QueryBudgetMasterActivity(parameters, identity.CompanyGroupId, employeeId, budgetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveBudgetMasterActivity(EmployeeResponsiblePerson entity)
        {
            _employeeResponsiblePersonService.SaveBudgetMasterActivity(entity);
            return Json(new { Message = AplosMessage.Success });
        }

        #endregion BudgetMasterActivity

        #region BudgetMasterActivityPhone

        [HttpGet, Authorize]
        public ActionResult BudgetMasterActivityPhone()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult BudgetMasterActivityPhoneResponsiblePerson(GridParameter parameters, string employeeId, string budgetMasterId, string activityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeResponsiblePersonService.QueryBudgetMasterActivityPhone(parameters, identity.CompanyGroupId, employeeId, budgetMasterId, activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveBudgetMasterActivityPhone(EmployeeResponsiblePerson entity)
        {
            _employeeResponsiblePersonService.SaveBudgetMasterActivityPhone(entity);
            return Json(new { Message = AplosMessage.Success });
        }

        #endregion BudgetMasterActivityPhone

        [HttpGet, Authorize]
        public ActionResult GetInactiveEmployeeList(string col, string val)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_preRecruitmentEmployeeService.inactiveEmps(col, identity.CompanyGroupId, val), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion EmployeeResponsiblePerson


        [HttpGet, Authorize]
        public JsonResult GetAvailableBudgetCode(string budgetCode)
        {
            return Json(employeeProfile.GetAvailableBudgetCode(budgetCode), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetLocalLanguageLabel(string plantId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_employeeProfileService.GetLocalLanguageLabel(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTemplateCbo(string type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            IEnumerable<ComboModel> gcomboModels;
            gcomboModels = _employeeProfileService.GetTemplateCbo(identity.PlantId, type);

            if (((List<ComboModel>)gcomboModels).Count == 0)
            {
                gcomboModels = _employeeProfileService.GetDefaultCbo(identity.CompanyGroupId, identity.PlantId);
            }

            return Json(gcomboModels, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            IEnumerable<ComboModel> gcomboModels;
            gcomboModels = _employeeProfileService.GetCbo(identity.PlantId);

            if (((List<ComboModel>)gcomboModels).Count == 0)
            {
                gcomboModels = _employeeProfileService.GetDefaultCbo(identity.CompanyGroupId, identity.PlantId);
            }

            return Json(gcomboModels, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDefaultCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProfileService.GetDefaultCbo(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRelationCbo()
        {
            return Json(_employeeProfileService.GetRelationCbo(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProfessionCbo()
        {
            return Json(_employeeProfileService.GetProfessionCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetOperationVariationCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProfileService.GetOperationVariationCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetOperationMaster(/*string empSystemId*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProfileService.GetOperationMaster(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetOperationVariation(/*string empSystemId*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProfileService.GetOperationVariation(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult EmployeeAppointmentLetterLocal_backupExcel(string empId, string tempId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Appointment Letter-" + empId + "" + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _employeeProfileService.EmployeeAppointmentLetterLocal(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            return RenderReportAsExcel(workbook, fileName);
        }


        [HttpGet, Authorize]
        public ActionResult EmployeeAppointmentLetterLocal(string empId, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _employeeProfileService.EmployeeAppointmentLetterInMSWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View();
        }



        [HttpGet, Authorize]
        public ActionResult EmployeeFixationForm(string empId, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _employeeProfileService.EmployeeFixationFormInWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View();
        }



        [HttpGet, Authorize]
        public ActionResult EmployeeServiceBookInWord(string empId, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _employeeProfileService.EmployeeServiceBookInMSWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception)
            {

                //throw ex;
            }
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult EmployeeNomineeInMSWord(string empId, string reportType, string tempId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.EmployeeNomineeInMSWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            return View();
        }
        [HttpGet, Authorize]
        public ActionResult EmployeeJoiningLetterInMSWord(string empId, string reportType, string tempId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.EmployeeJoiningLetterInMSWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            return View();
        }
        [HttpGet, Authorize]
        public ActionResult LeaveRegister(string CalanderYearId, string FromDate, string ToDate, string empId, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _employeeProfileService.generateReport(CalanderYearId, FromDate, ToDate, identity.PlantId, empId, reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }
        [HttpGet, Authorize]
        public ActionResult EmployeeAcknowledgementInMSWord(string empId, string reportType, string tempId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.EmployeeAcknowledgementInMSWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            //LetterType.


            return View();
        }

        [HttpGet, Authorize]
        public ActionResult ConfirmationletterInMSWord(string empId, string reportType, string tempId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.ConfirmationletterInMSWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);
            return View();

        }

        [HttpGet, Authorize]
        public JsonResult GetWithoutUserEmployeeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeInformationService.GetWithoutUserEmployeeList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPlantEmployeeList(GridParameter parameters, string plantId, string employeeIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId)) plantId = identity.PlantId;
            return Json(_employeeInformationService.Query(parameters, identity.CompanyGroupId, plantId, new JavaScriptSerializer().Deserialize<string[]>(employeeIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeListByCompanyGroup(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeInformationService.EmployeeListByCompanyGroup(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeListByCompany(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyId) || companyId == "null")
                companyId = identity.CompanyId;
            return Json(_employeeInformationService.EmployeeListByCompany(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeListByPlant(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeInformationService.EmployeeListByPlant(parameters, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult EmployeeSearchByEntity(GridParameter parameters, string entityId)
        {
            return Json(_employeeInformationService.EmployeeListByEntity(parameters, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult EmployeeByEmployeeId(string employeeId)
        {
            return Json(_employeeInformationService.EmployeeByEmployeeId(employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployeeService.Query(parameters, identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetEmployeeDataList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(employeeProfile.Query(column, value, identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            var jsondata = Json(employeeProfile.Query(column, value, identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public JsonResult GetIsOTEntitled(string designationId)
        {
            clsEmployeeLoad clsEmployee = new clsEmployeeLoad();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(employeeProfile.GetIsOTEntitled(identity.PlantId, designationId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult CheckDuplicateEmployeeCode(string systemId, string employeeCode, string EmployeeCodeCheckLevel)
        //{
        //    clsEmployeeLoad clsEmployee = new clsEmployeeLoad();
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(clsEmployee.DuplicateEmployeeCode(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, systemId, employeeCode, EmployeeCodeCheckLevel), JsonRequestBehavior.AllowGet);
        //}
        [HttpGet, Authorize]
        public JsonResult CheckDuplicateEmployeeCode(string systemId, string employeeCode, string EmployeeCodeTypeId)
        {
            clsEmployeeLoad clsEmployee = new clsEmployeeLoad();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(clsEmployee.DuplicateEmployeeCodeWithInGroup(identity.PlantId, systemId, employeeCode, EmployeeCodeTypeId), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// For Document Dashboard
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="plantId"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult GetEmployeeListDocDashboard(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployeeService.Query(parameters, identity.CompanyGroupId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeListWithPlant(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployeeService.GetEmployeeWithPlant(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetJobData(string empid)
        {
            return Json(employeeProfile.GetJobData(empid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpProfileData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(employeeProfile.GetData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetReferenceData(string empid)
        {
            return Json(_empReferenceInformationService.GetData(empid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQualificationData(string empid)
        {
            return Json(_empAcademicQualificationInformationService.GetData(empid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetExperienceData(string empid)
        {
            return Json(_empExperienceInformationService.GetData(empid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTrainingData(string empid)
        {
            return Json(_empTrainingInformationService.GetData(empid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpDocumentDataList(string companyGroupId, string pId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(employeeProfile.GetEmpDocumentDataList(identity.CompanyGroupId, pId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpAllDocumentDataList(string companyGroupId, string pId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(employeeProfile.GetEmpAllDocumentDataList(identity.CompanyGroupId, pId, plantId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult SaveEmployeeComplianceDocument(string empId, string plantId, string givenDesignationId, string budgetId, string empType)
        {
            employeeProfile.GetEmployeeComplianceDocument(empId, plantId, givenDesignationId, budgetId, empType);
            return Json(new { Message = "Document Generate Successfully." });

        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeNomineeInfo(string empId)
        {
            return Json(employeeProfile.GetEmployeeNomineeInfo(empId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmployeeDependantInfo(string empId)
        {
            return Json(employeeProfile.GetEmployeeDependantInfo(empId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeLandLoardInfo(string empId)
        {
            return Json(employeeProfile.GetEmployeeLandLoardInfo(empId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult CreateDocumentList(string plantId, string empType, string budgetCode, string givenDesignationId)
        {
            return Json(_employeeDocumentService.GetDocumentList(plantId, empType, budgetCode, givenDesignationId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult CreateNewDOcument(IEnumerable<EmployeeDocument> employeeDocument, string empId)
        {
            _employeeDocumentService.CreateNewDOcument(employeeDocument, empId);
            return Json(new { Message = AplosMessage.Success });
        }

        #region EmployeeInformation

       
        

        [HttpPost]
        public JsonResult CreateNew(EmployeeInformation entity, string EmployeeCodeCheckLevel, EmpReferenceInformation empRef, Dictionary<string, object> empBank)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.EmployeeCodeTypeId))
                {
                    throw new Exception("Employee Code Type is required.");
                }
                // , Dictionary<string, object> WeekOff, Dictionary<string, object> OT
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IdentityParameter para = new IdentityParameter
                {
                    CompanyGroupId = identity.CompanyGroupId,
                    CompanyId = identity.CompanyId,
                    PlantId = identity.PlantId,
                    AddedBy = identity.Name,
                    AddedDate = DateTime.Now,
                    AddedFromIP = identity.IPAddress,
                    UpdatedBy = identity.Name,
                    UpdatedDate = DateTime.Now,
                    UpdatedFromIP = identity.IPAddress
                };

                var empNid = CkeckEmployeeDuplicateNID(entity.SystemId, entity.NationalID);
                if (empNid.Tables[0].Rows.Count > 0)
                {

                    throw new Exception("This National Id: " + entity.NationalID + " is exists for Employee (Code: " + empNid.Tables[0].Rows[0]["EmployeeCode"] + ", Name: " + empNid.Tables[0].Rows[0]["EmployeeName"] + ", Designation: " + empNid.Tables[0].Rows[0]["LegalDesignation"] + ", Department: " + empNid.Tables[0].Rows[0]["Department"] + ")-" + empNid.Tables[0].Rows[0]["EmployeeStatus"] + "");
                }

                else
                {
                    var BlackListData = GetBlackListData(entity.NationalID);
                    if (BlackListData.Tables[0].Rows.Count > 0)
                    {
                        if (BlackListData.Tables[0].Rows[0]["CompanyEmployeeOutsider"].ToString() == "CompanyEmp")
                        {
                            var BlackListEmpData = GetBlackListEmpData(entity.NationalID);
                            if (BlackListEmpData.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("This National Id: " + entity.NationalID + " is exists for Employee (Code: " + BlackListEmpData.Tables[0].Rows[0]["EmployeeCode"] + ", Name: " + BlackListEmpData.Tables[0].Rows[0]["EmployeeName"] + ", Designation: " + BlackListEmpData.Tables[0].Rows[0]["LegalDesignation"] + ", Department: " + BlackListEmpData.Tables[0].Rows[0]["Department"] + ")-" + BlackListEmpData.Tables[0].Rows[0]["EmployeeStatus"] + "");
                            }
                        }
                        else
                        {
                            var BlackListOutData = GetBlackListOutData(entity.NationalID);
                            if (BlackListOutData.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("This National Id: " + entity.NationalID + " is exists for Outsider (Name: " + BlackListOutData.Tables[0].Rows[0]["OutsiderName"] + ", Outsider Father Name: " + BlackListOutData.Tables[0].Rows[0]["OutsiderFatherName"] + ").");
                            }
                        }
                    }
                }
                employeeProfile.SaveData(entity, para, EmployeeCodeCheckLevel, empRef,empBank); //, WeekOff, OT

                return Json(new { EmployeeInformation = entity, Message = AplosMessage.Insert + "Employee Code: " + entity.EmployeeCode + "" });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }

        private DataSet CkeckEmployeeDuplicateNID(string strSystemID, string strNID)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT E.EmployeeCode,E.EmployeeName,LD.UserName LegalDesignation,DP.UserName Department,E.EmployeeStatus
                            FROM EmployeeInformation E
                            LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
                            LEFT JOIN HKP.LegalDesignation LD ON LD.Id=E.LegalDesignationId
                            LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
                            WHERE (E.SystemID <>'" + strSystemID + @"') AND (E.NationalID = '" + strNID + "') AND E.EmployeeStatus='Active'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet GetBlackListData(string strNID)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"Select * FROM [dbo].[BlackList] WHERE AadharNumber = '" + strNID + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet GetBlackListEmpData(string strNID)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT E.EmployeeCode,E.EmployeeName,LD.UserName LegalDesignation,DP.UserName Department,E.EmployeeStatus
                                    FROM [dbo].[BlackList] BL
                                    LEFT JOIN EmployeeInformation E ON BL.EmpSystemId=E.SystemId
                                    LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
                                    LEFT JOIN HKP.LegalDesignation LD ON LD.Id=E.LegalDesignationId
                                    LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
                                    WHERE BL.AadharNumber = '" + strNID + "' AND CompanyEmployeeOutsider='CompanyEmp'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }


        private DataSet GetBlackListOutData(string strNID)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"Select OutsiderName, OutsiderFatherName FROM [dbo].[BlackList] WHERE AadharNumber = '" + strNID + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }


        [HttpPost, Authorize]
        public JsonResult Create(FormCollection form)
        {
            var pre = form["employeeInformation"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var employee = JsonConvert.DeserializeObject<EmployeeInformation>(pre, settings);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            employee.AddedBy = identity.Name;
            employee.DateAdded = DateTime.Now;
            employee.GroupID = identity.CompanyGroupId;
            employee.CompanyId = identity.CompanyId;
            employee.PlantID = identity.PlantId
                ;
            _employeeProfileService.InsetOrUpdateMaster(employee);

            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                {
                    employee.EmpPicPath = extension;
                    if (!string.IsNullOrEmpty(employee.EmpPicPath))
                        employee.EmpPicPath = employee.SystemId + employee.EmpPicPath;
                }
                else
                    throw new CustomException(Resources.ImageUploadError);
            }

            if (file != null)
            {
                var path = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeImage, typeof(string)).ToString())*/, employee.EmpPicPath);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                {
                    file.SaveAs(path);
                }
            }
            return Json(new { EmployeeInformation = employee, Message = AplosMessage.Success });
        }


        [HttpPost, Authorize]
        public JsonResult Edit(FormCollection form)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var pre = form["employeeInformation"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var employee = JsonConvert.DeserializeObject<EmployeeInformation>(pre, settings);
            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png" || extension.ToLower() == ".jpeg")
                {
                    employee.EmpPicPath = extension;
                    if (!string.IsNullOrEmpty(employee.EmpPicPath))
                        employee.EmpPicPath = employee.SystemId + employee.EmpPicPath;
                }
                else
                    throw new CustomException(Resources.ImageUploadError);
            }
            _employeeProfileService.UpdateMaster(employee, identity.Name);
            if (file != null)
            {
                var path = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeImage, typeof(string)).ToString())*/, employee.EmpPicPath);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                {
                    file.SaveAs(path);
                }
            }
            return Json(new { EmployeeInformation = employee, Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult CreatePersonal(EmployeeInformation employeeInformation)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.UpdatePersonal(employeeInformation, identity.Name);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult CreateAddress(EmployeeInformation employeeInformation)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.UpdateAddress(employeeInformation, identity.Name);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult CreateLocalInfo(EmployeeInformation employeeInformation)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.UpdateLocalInfo(employeeInformation, identity.Name);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult CreateRelativeInfo(EmployeeInformation employeeInformation)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.UpdateRelativeInfo(employeeInformation, identity.Name);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }


        [HttpPost, Authorize]
        public JsonResult CreateEmployment(EmployeeInformation employeeInformation)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.UpdateEmployment(employeeInformation, identity.Name);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult CreateAdvanceInfo(EmployeeInformation employeeInformation)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeProfileService.UpdateAdvanceInfo(employeeInformation, identity.Name, identity.IPAddress);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult CreateReference(EmpReferenceInformation empReferenceInformation)
        {
            _empReferenceInformationService.InsertOrUpdate(empReferenceInformation);
            return Json(new { EmpReferenceInformation = empReferenceInformation, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create)), Authorize]
        public JsonResult CreateQualification(FormCollection form, HttpPostedFileBase[] file)
        {
            var empAcademicQualificationInformation = new JavaScriptSerializer().Deserialize<EmpAcademicQualificationInformation>(form["empAcademicQualificationInformation"]);

            _empAcademicQualificationInformationService.InsertORUpdateMaster(empAcademicQualificationInformation);

            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetQualificationDestinationPath();
                var path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = _empAcademicQualificationInformationService.GetQualificationFile(empAcademicQualificationInformation.SystemID);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["FileId"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != empAcademicQualificationInformation.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + empAcademicQualificationInformation.SystemID + Path.GetExtension(item.FileName));
                    }
                }

            }
            return Json(new { EmpAcademicQualificationInformation = empAcademicQualificationInformation, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create)), Authorize]
        public JsonResult CreateExperience(FormCollection form, HttpPostedFileBase[] file)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var empExperienceInformation = JsonConvert.DeserializeObject<EmpExperienceInformation>(form["empExperienceInformation"], settings);

            _empExperienceInformationService.InsertORUpdateMaster(empExperienceInformation);

            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetExperienceDestinationPath();
                var path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = _empExperienceInformationService.GetExperienceFile(empExperienceInformation.SystemID);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["FileId"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != empExperienceInformation.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + empExperienceInformation.SystemID + Path.GetExtension(item.FileName));
                    }
                }

            }

            return Json(new { EmpExperienceInformation = empExperienceInformation, Message = AplosMessage.Success });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create)), Authorize]
        public JsonResult CreateTraining(FormCollection form, HttpPostedFileBase[] file)
        {
            var empTrainingInformation = new JavaScriptSerializer().Deserialize<EmpTrainingInformation>(form["empTrainingInformation"]);


            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetTrainingDestinationPath();
                var path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = _empTrainingInformationService.GetTrainingFile(empTrainingInformation.SystemID);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["FileId"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != empTrainingInformation.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + empTrainingInformation.SystemID + Path.GetExtension(item.FileName));
                    }
                }
            }

            _empTrainingInformationService.InsertORUpdateMaster(empTrainingInformation);

            return Json(new { EmpTrainingInformation = empTrainingInformation, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult CreateNomineeInfo(EmployeeNomineeInfo nomineeInfo)
        {
            _employeeProfileService.InsertOrUpdate(nomineeInfo);
            return Json(new { EmployeeNomineeInfo = nomineeInfo, Message = AplosMessage.Success });
        }


        [HttpPost, Authorize]
        public JsonResult CreateDependantInfo(EmployeeDependantInfo dependantInfo)
        {
            _employeeProfileService.InsertOrUpdatedependantInfo(dependantInfo);
            return Json(new { EmployeeDependantInfo = dependantInfo, Message = AplosMessage.Success });
        }


        [HttpPost, Authorize]
        public JsonResult CreateLandLordInfo(EmployeeLandLordInfo LandLordInfo)
        {
            _employeeProfileService.InsertOrUpdateLandLordInfo(LandLordInfo);
            return Json(new { EmployeeLandLordInfo = LandLordInfo, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteQualification(string id)
        {
            var directory = ResourcesPathReader.GetQualificationDestinationPath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _empAcademicQualificationInformationService.GetQualificationFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _empAcademicQualificationInformationService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public JsonResult DeleteExperience(string id)
        {
            var directory = ResourcesPathReader.GetExperienceDestinationPath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _empExperienceInformationService.GetExperienceFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
            !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _empExperienceInformationService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public JsonResult DeleteTraining(string id)
        {
            var directory = ResourcesPathReader.GetTrainingDestinationPath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _empTrainingInformationService.GetTrainingFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
            !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _empTrainingInformationService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public JsonResult DeleteDocument(string id)
        {
            var directory = ResourcesPathReader.GetDocumentDestinationPath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _employeeDocumentService.GetDocFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();

                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _employeeDocumentService.UpdateEmployeeDocument(id);

            return Json(new { Message = "File detach successfully." });
        }
        [HttpPost]
        public JsonResult DeleteSingleDocument(string id)
        {
            var directory = ResourcesPathReader.GetDocumentDestinationPath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _employeeDocumentService.GetDocFile(id);
            var fName = data["FileName"].ToString();
            if (!string.IsNullOrEmpty(fName))
            {
                throw new CustomException("This document cannot be deleted.");
            }
            else
            {
                if (data.Count > 0)
                {
                    if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                    !string.IsNullOrEmpty(data["FileName"].ToString()))
                        fileId = data["FileId"].ToString();
                    fileName = data["FileName"].ToString();

                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }
                _employeeDocumentService.DeleteEmployeeDocument(id);
            }
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create)), Authorize]
        public JsonResult CreateDocument(FormCollection form, HttpPostedFileBase[] file)
        {
            var employeeDocument = new JavaScriptSerializer().Deserialize<EmployeeDocument>(form["employeeDocument"]);

            _employeeDocumentService.InsertORUpdateMaster(employeeDocument);

            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetDocumentDestinationPath();
                var path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = _employeeDocumentService.GetDocFile(employeeDocument.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["FileId"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != employeeDocument.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + employeeDocument.Id + Path.GetExtension(item.FileName));
                    }
                }

            }

            return Json(new { EmployeeDocument = employeeDocument, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteNominee(string id)
        {
            _employeeProfileService.DeleteNominee(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        [HttpPost, Authorize]
        public JsonResult DeleteDependant(string id)
        {
            _employeeProfileService.DeleteDependant(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteLandLoard(string id)
        {
            _employeeProfileService.DeleteLandLoard(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        private static string GetDoc(List<EmployeeDocument> doc, string fileName)
        {
            return doc.Find(r => r.FileName == fileName).ComplianceDocumentId;
        }

        private static string GetFileId(IEnumerable<EmployeeDocument> list, string fileName)
        {
            foreach (var item in list)
            {
                if (item.FileName == fileName)
                {
                    return item.Id;
                }
            }
            return "";
        }

        private static string GetFileName(IEnumerable<PreRecruitmentDocument> list, string fileid)
        {
            foreach (var item in list)
            {
                if (item.FileId == fileid)
                {
                    return item.FileName;
                }
            }
            return "";
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(employeeProfile.GetEmployeeCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJobLocationCbo(string flag)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(employeeProfile.GetJobLocationCbo(flag, identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetAllJobLocationCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(employeeProfile.GetAllJobLocationCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetPlantWiseHRMSSetting()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(employeeProfile.GetPlantWiseHRMSSetting(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetOnRollByBudget(string budgetId)
        {
            try
            {
                return Json(employeeProfile.GetOnRollByBudget(budgetId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetEmpCodeGenSetting(string employeeCodeTypeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(employeeProfile.GetEmpCodeGenSetting(identity.PlantId, employeeCodeTypeId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult SetDOC()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(employeeProfile.GetPlantWiseHRMSSetting(identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetCboShiftDefinationByPlant(string plantId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT SystemID,UserName FROM ShiftDefination WHERE PlantId ='" + plantId + @"' ORDER BY UserName";
                return Json(_sqlRepository.GetCombo(strSQL, "SystemID", "UserName"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        #endregion EmployeeInformation

        //[HttpPost, Authorize]
        //public JsonResult Delete(string id)
        //{
        //    _employeeInformationService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}

        [HttpGet, Authorize]
        public JsonResult CboList()
        {
            return Json(_preRecruitmentEmployeeService.CboList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEmployeeIndex(string employeeName)
        {
            var path = ResourcesPathReader.GetVirtualFolderName() + "/EmployeeProfiles/EmpPic/";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { results = _employeeInformationService.GetEmployeeIndex(identity.CompanyGroupId, employeeName, path) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeById(string employeeId, string employeementType)
        {
            return Json(_employeeProfileService.GetEmployeeById(employeeId, employeementType), JsonRequestBehavior.AllowGet);
        }

        #region Report

        [HttpGet, Authorize]
        public ActionResult JobCardReport(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Job Card" + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _employeeProfileService.JobCard_Report(identity.EmployeeId, fromDate, toDate, identity.CompanyGroupId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult EmployeeInfoReport(string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Employee Information" + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _employeeProfileService.EmpInfoReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, employeeId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet]
        public ActionResult EmpRegisterInfo(string radioValue)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Employee Register Information" + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _employeeProfileService.EmpRegisterReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, radioValue);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public JsonResult GetClanderYear()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = _employeeProfileService.GetClanderYear(identity.PlantId);
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        #endregion Report

        #region Mediasoft Fair Shop Data Export
        [Authorize]
        public ActionResult MediasoftFairShopEmpDataExport()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataTable data = _employeeProfileService.MediasoftFairShopDataExport();//, strPathHindi, strPathEnglish, strPathBangla);

            //LetterType.
            // WriteDataTableToCSV(data, "FairShop");
            var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            var workbook = application.Workbooks.Create(3);
            var sheet1 = workbook.Worksheets[0];

            sheet1.ImportDataTable(data, true, 1, 1);

            workbook.Version = ExcelVersion.Excel2013;


            workbook.SaveAs(DateTime.Now.ToString("yyMMdd") + " FairShop.csv", ",", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog, ExcelHttpContentType.CSV);


            return null;
        }


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
        [HttpPost]
        public JsonResult CreateLockData(string lockDate)
        {

            _employeeProfileService.CreateLockData(lockDate);
            return Json(new { Message = AplosMessage.Success });
        }
        #endregion

        #region IDCard Issue

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string empSystemId)
        {
            return Json(_employeeProfileService.GetAutoSequence(empSystemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIssueIdCardByEmployee(string employeeId)
        {
            return Json(_employeeProfileService.GetIssueIdCardByEmployee(employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWarningLetterByEmployee(string employeeId)
        {
            return Json(_employeeProfileService.GetWarningLetterByEmployee(employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateEmployeeIdCardIssue(EmployeeIdCardIssue employeeIdCardIssue)
        {
            InsertOrUpdateEmployeeIdCardIssue(employeeIdCardIssue);
            return Json(new { Message = AplosMessage.Success });
        }

        private string GeIDIssuePK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(EmployeeIdCardIssue), out sID);
            return sID;
        }

        private void InsertOrUpdateEmployeeIdCardIssue(EmployeeIdCardIssue data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            try
            {

                string sql = "SELECT * FROM [dbo].[EmployeeIdCardIssue] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GeIDIssuePK();
                    dr["Sequence"] = data.Sequence;
                    dr["EmpSystemId"] = data.EmpSystemId;
                    dr["EmployeeWorkTypeId"] = data.EmployeeWorkTypeId;
                    dr["IssueDate"] = data.IssueDate;

                    if (String.IsNullOrEmpty(data.ExpiryDate.ToString()))
                    {
                        dr["ExpiryDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ExpiryDate"] = data.ExpiryDate;
                    }


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["Sequence"] = data.Sequence;
                    dr["EmpSystemId"] = data.EmpSystemId;
                    dr["EmployeeWorkTypeId"] = data.EmployeeWorkTypeId;
                    dr["IssueDate"] = data.IssueDate;
                    if (String.IsNullOrEmpty(data.ExpiryDate.ToString()))
                    {
                        dr["ExpiryDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ExpiryDate"] = data.ExpiryDate;
                    }


                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        #endregion

        #region  EmployeeOperation     

        [HttpGet, Authorize]
        public ActionResult GetSavedOperationData(string empsystemId)
        {
            return Json(employeeProfile.GetSavedOperationData(empsystemId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveOperation(List<EmployeeOperation> data, string EmpSystemId)
        {
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                IdentityParameter para = new IdentityParameter
                {
                    AddedBy = identity.Name,
                    AddedDate = DateTime.Now,
                    AddedFromIP = identity.IPAddress,
                    UpdatedBy = identity.Name,
                    UpdatedDate = DateTime.Now,
                    UpdatedFromIP = identity.IPAddress
                };

                employeeProfile.SaveOperation(data, para, EmpSystemId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }


        [HttpPost, Authorize]
        public JsonResult DeleteOperation(string id)
        {
            employeeProfile.DeleteEmployeeOperation(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion

        [HttpPost, Authorize]
        public ActionResult SaveSignature(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save Employee first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var destinationPath = Path.Combine(ResourcesPathReader.GetCardHolderSignaturePath(), fileName);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetCardHolderSignaturePath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetCardHolderSignaturePath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from EmployeeInformation where SystemId='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["EmpSignature"] = fileName;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);

                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id)
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("SELECT EmpSignature FROM dbo.EmployeeInformation WHERE SystemId ='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet, Authorize]
        public ActionResult IncrementHistory(string empId, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _employeeProfileService.EmployeeIncrementHistory(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult ExitInterview(string empId, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _employeeProfileService.EmployeeExitInterview(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId, "Permanent", reportType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetApprovalAuthority()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(employeeProfile.GetApprovalAuthority(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        // To add the WeekOff And teh Non Eligible OT
        //[HttpGet, Authorize]
        //public ActionResult getWeekOff()
        //{
        //    return Json(employeeProfile.getWeekOff(), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost, Authorize]
        public ActionResult getNonEligibleOT(string DesgId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(employeeProfile.getNonEligibleOT(DesgId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetPrintData(string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            employeeProfile.GetAPPLICATIONFORMFORRECRUITMENT(empId);

            return View();
        }
    }
}
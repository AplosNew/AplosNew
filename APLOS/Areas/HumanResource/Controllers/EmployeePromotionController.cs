using System;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Model.Employees;
using Library.Model.HumanResources;
using Library.Model.Payrolls;
using Library.Service.Employees;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Library.Model.External;
using Library.Service.HumanResources;
using OTSBD;
using Library.Service.Helpers;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeePromotionController : BaseController
    {
        #region Constructor

        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly EmployeePromotionService _employeePromotionService;
        public EmployeePromotionController(
            IEmployeeInformationService employeeInformationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeProfileService employeeProfileService
            , EmployeePromotionService employeePromotionService
        )
        {
            _employeeInformationService = employeeInformationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _employeeProfileService = employeeProfileService;
            _employeePromotionService = employeePromotionService;
        }



        //private readonly IEmployeeProfileService _employeeProfileService;

        //public EmployeePromotionController(
        //      IEmployeeProfileService employeeProfileService
        //    )
        //{
        //    _employeeProfileService = employeeProfileService;
        //}

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

       [Authorize]
        public ActionResult Promotion()
        {
            return View();
        }


        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public JsonResult Save(List<XLUploadDetail> List)
        {
            _employeeProfileService.Insert(List);
            return Json(new { Message = "Data Uploaded Successfully" });

        }

        #region Load Employee
        [HttpGet]
        public JsonResult GetSalaryStrcApprovedEmployeeById(string EmpSystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = _employeePromotionService.GetSalaryStrcApprovedEmployeeById(EmpSystemId, identity.CompanyGroupId, identity.PlantId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetSalaryStrcApprovedEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            //return Json(_employeePromotionService.GetSalaryStrcApprovedEmployee(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);


            JsonResult json = Json(_employeePromotionService.GetSalaryStrcApprovedEmployee(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        //public JsonResult xGetSalaryStrcApprovedEmployeeList(GridParameter parameters)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_employeePromotionService.xGetSalaryStrcApprovedEmployee(parameters, identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        //}


        [HttpGet]
        public JsonResult GetSalaryStrcUnApprovedEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_employeePromotionService.GetSalaryStrcUnApprovedEmployee(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(_employeePromotionService.GetSalaryStrcUnApprovedEmployee(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        //public JsonResult xGetSalaryStrcUnApprovedEmployeeList(GridParameter parameters)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    return Json(_employeePromotionService.GetSalaryStrcUnApprovedEmployee(parameters, identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        //}
        #endregion

        [HttpPost]
        public JsonResult Update(EmployeeInformation employeeInformation, IncrementHistoryModel incrementHistory)
        {
            clsSalaryStructureAplos ob = new clsSalaryStructureAplos();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (incrementHistory.IsPromotion == true)
            {
                incrementHistory.AddedBy = identity.UserId;
                incrementHistory.UpdatedBy = identity.UserId;
                incrementHistory.AddedFromIP = identity.IPAddress;
                incrementHistory.UpdatedFromIP = identity.IPAddress;
                ob.SaveIncrementHistoryData(incrementHistory);
            }

            _employeeProfileService.UpdateBudgetCode(employeeInformation);

            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }




        public JsonResult UpdateSalaryStracture(EmployeeInformation employeeInformation, EmpSalaryInfoModel EmpSalaryInfo, CustomParaPFSetting PFSettingModel, IEnumerable<EmpSalaryInfoDefineModel> EmpSalaryInfoDefineNew, IncrementHistoryModel incrementHistory)
        {
            _employeePromotionService.UpdateSalaryStractureForIncrement(employeeInformation, PFSettingModel, EmpSalaryInfo, EmpSalaryInfoDefineNew, incrementHistory);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }
        public JsonResult CalculateSalary(EmployeeInformation employeeInformation, string IsbuttonPFClicked, bool IsPFEntitle, string SalaryRuleMasterSystemID, IEnumerable<OpenHeadModel> EmpSalaryOpenHeadNew)
        {
            string newGross = string.Empty;
            string newCTC = string.Empty;
            string newFormula_Desc = string.Empty;
            DataTable dsLocal = null;
            IEnumerable<EmpSalaryInfoDefineModel> EmpSalaryInfoDefine = null;
            _employeePromotionService.CalculateSalary(employeeInformation, IsbuttonPFClicked, IsPFEntitle, SalaryRuleMasterSystemID, EmpSalaryOpenHeadNew, out EmpSalaryInfoDefine, out newGross, out newCTC, out newFormula_Desc);

            //var ResultData = dsLocal.AsEnumerable().Select(
            //    dataRow => new
            //    {
            //        IsSelectSlrHd = dataRow.Field<bool>("IsSelectSlrHd"),
            //        SlrInfoDefSystemID = dataRow.Field<string>("SlrInfoDefSystemID"),                       //1
            //        CurrencyRuleChildSystemID = dataRow.Field<string>("CurrencyRuleChildSystemID"),          //2
            //        SalaryHeadID = dataRow.Field<string>("SalaryHeadID"),                                    //3
            //        SalaryHead = dataRow.Field<string>("SalaryHead"),                                        //4
            //        HeadType = dataRow.Field<string>("HeadType"),                                           //5
            //        FormulaDesID = dataRow.Field<string>("FormulaDesID"),                                   //6
            //        FixedValue = dataRow.Field<string>("FixedValue"),                                       //7
            //        IsOpen = dataRow.Field<string>("IsOpen"),                                              //8
            //        EntryCurrencyID = dataRow.Field<string>("EntryCurrencyID"),                            //9
            //        EntryCurrency = dataRow.Field<string>("EntryCurrency"),                                //10
            //        DefinitionCurrencyID = dataRow.Field<string>("DefinitionCurrencyID"),                  //11
            //        DefinitionCurrency = dataRow.Field<string>("DefinitionCurrency"),                      //12
            //        EntryAmount = dataRow.Field<string>("EntryAmount"),                                    //13
            //        DefineAmount = dataRow.Field<string>("DefineAmount"),                                  //14
            //        TagAndUnTag = dataRow.Field<string>("TagAndUnTag"),                                    //15
            //        MonthPeriod = dataRow.Field<string>("MonthPeriod"),                                    //16
            //        IsNA = dataRow.Field<string>("IsNA"),                                                 //17
            //        HeadCategory = dataRow.Field<string>("HeadCategory"),                                 //18
            //        SalaryHdSequence = dataRow.Field<string>("SalaryHdSequence"),
            //        SalaryCategory = dataRow.Field<string>("SalaryCategory")

            //    }).ToList();


            return Json(new { EmpSalaryInfoDefine, newGross, newCTC, newFormula_Desc, Message = AplosMessage.Updated });
        }


        public JsonResult GetSalaryFormulaDetails(EmployeeInformation employeeInformation, string IsbuttonPFClicked, bool IsPFEntitle, string SalaryRuleMasterSystemID, IEnumerable<OpenHeadModel> EmpSalaryOpenHeadNew)
        {
            string newGross = string.Empty;
            string newCTC = string.Empty;
            string newFormula_Desc = string.Empty;

            IEnumerable<EmpSalaryInfoDefineModel> EmpSalaryInfoDefine = null;
            _employeePromotionService.CalculateSalary(employeeInformation, IsbuttonPFClicked, IsPFEntitle, SalaryRuleMasterSystemID, EmpSalaryOpenHeadNew, out EmpSalaryInfoDefine, out newGross, out newCTC, out newFormula_Desc);
            //var ResultData= _employeePromotionService.ConvertEmpSalaryInfoDefineToList(dsLocal);
            //var ResultData = dsLocal.AsEnumerable().Select(
            //    dataRow => new
            //    {
            //        IsSelectSlrHd = dataRow.Field<bool>("IsSelectSlrHd"),
            //        SlrInfoDefSystemID = dataRow.Field<string>("SlrInfoDefSystemID"),                       //1
            //        CurrencyRuleChildSystemID = dataRow.Field<string>("CurrencyRuleChildSystemID"),          //2
            //        SalaryHeadID = dataRow.Field<string>("SalaryHeadID"),                                    //3
            //        SalaryHead = dataRow.Field<string>("SalaryHead"),                                        //4
            //        HeadType = dataRow.Field<string>("HeadType"),                                           //5
            //        FormulaDesID = dataRow.Field<string>("FormulaDesID"),                                   //6
            //        FixedValue = dataRow.Field<string>("FixedValue"),                                       //7
            //        IsOpen = dataRow.Field<string>("IsOpen"),                                              //8
            //        EntryCurrencyID = dataRow.Field<string>("EntryCurrencyID"),                            //9
            //        EntryCurrency = dataRow.Field<string>("EntryCurrency"),                                //10
            //        DefinitionCurrencyID = dataRow.Field<string>("DefinitionCurrencyID"),                  //11
            //        DefinitionCurrency = dataRow.Field<string>("DefinitionCurrency"),                      //12
            //        EntryAmount = dataRow.Field<string>("EntryAmount"),                                    //13
            //        DefineAmount = dataRow.Field<string>("DefineAmount"),                                  //14
            //        TagAndUnTag = dataRow.Field<string>("TagAndUnTag"),                                    //15
            //        MonthPeriod = dataRow.Field<string>("MonthPeriod"),                                    //16
            //        IsNA = dataRow.Field<string>("IsNA"),                                                 //17
            //        HeadCategory = dataRow.Field<string>("HeadCategory"),                                 //18
            //        SalaryHdSequence = dataRow.Field<string>("SalaryHdSequence"),
            //        SalaryCategory = dataRow.Field<string>("SalaryCategory")

            //    }).ToList();


            return Json(new { EmpSalaryInfoDefine, newGross, newCTC, newFormula_Desc, Message = AplosMessage.Updated });
        }
        [HttpGet]
        public JsonResult GetEmpSalaryInfoDefineData(string EmpSystemId)
        {

            return Json(_employeePromotionService.Query(EmpSystemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GivenDesignationChange(string GivenDesignationId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataSet dsResult = _employeePromotionService.GivenDesignationChange(GivenDesignationId, identity.CompanyGroupId, identity.PlantId);
            var ResultData = dsResult.Tables[0].AsEnumerable().Select(
               dataRow => new
               {
                   SalaryRuleMasterId = dataRow.Field<string>("SalaryRuleMasterId"),

               }).ToList();
            return Json(ResultData, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult LoadEmpSalaryInfoDefineData(string EmpSystemId)
        {
            CustomOutPara outPara = null;//new CustomOutPara();
            IEnumerable<EmpSalaryInfoDefineModel> EmpSalaryInfoDefine = null;
            IEnumerable<EmpSalaryInfoDefineModel> EmpApprovedSalaryInfoDefine = null;
            //IEnumerable<object> EmpApprovedSalaryInfoDefine = null;
            IEnumerable<SalaryRuleModel> ResultSalaryRule = null;
            IEnumerable<SalaryRuleModel> ResultSelectedSalaryRule = null;
            IEnumerable<OpenHeadModel> ResultOpenHead = null;
            IEnumerable<OpenHeadModel> ResultApprovedOpenHead = null;
            string ResultMinWage = null;
            string ApprovalStatus = string.Empty;
            string ApprovedEffectiveDate = string.Empty;
            string ApprovedNextDueDate = string.Empty;
            string ResultEffectiveDate = string.Empty;
            string ResultGross = string.Empty;
            string ResultNetCTC = string.Empty;
            bool IsSalaryRuleEditableEmployee = false;
            string NewFormula_Desc = string.Empty;
            string ApprovedFormula_Desc = string.Empty;
            string UnApprovedNextDueDate = string.Empty;
            try
            {
                _employeePromotionService.LoadEmpSalaryInfoDefineData(EmpSystemId, out EmpSalaryInfoDefine, out EmpApprovedSalaryInfoDefine, out ResultSalaryRule, out ResultSelectedSalaryRule, out ResultOpenHead, out ResultApprovedOpenHead, out outPara);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            ResultMinWage = outPara.ResultMinWage;
            ApprovalStatus = outPara.ApprovalStatus;
            ApprovedEffectiveDate = outPara.ApprovedEffectiveDate;
            ApprovedNextDueDate = outPara.ApprovedNextDueDate;
            ResultEffectiveDate = outPara.ResultEffectiveDate;
            ResultGross = outPara.ResultGross;
            ResultNetCTC = outPara.ResultNetCTC;
            IsSalaryRuleEditableEmployee = outPara.IsSalaryRuleEditableEmployee;

            NewFormula_Desc = outPara.NewFormula_Desc;
            ApprovedFormula_Desc = outPara.ApprovedFormula_Desc;
            UnApprovedNextDueDate = outPara.UnApprovedNextDueDate;










            return Json(new { EmpSalaryInfoDefine, EmpApprovedSalaryInfoDefine, ResultMinWage, ResultSalaryRule, ResultSelectedSalaryRule, ResultOpenHead, ResultApprovedOpenHead, ResultGross, ResultNetCTC, IsSalaryRuleEditableEmployee, ApprovalStatus, ApprovedEffectiveDate, ApprovedNextDueDate, ResultEffectiveDate, NewFormula_Desc, ApprovedFormula_Desc, UnApprovedNextDueDate, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult SalaryRuleChange(string EmpSystemId, string SalaryRuleId)
        {

            IEnumerable<OpenHeadModel> ResultOpenHead = null;
            string newFormula_Desc = string.Empty;
            _employeePromotionService.SalaryRuleChange(EmpSystemId, SalaryRuleId, out ResultOpenHead);
            _employeePromotionService.GetFomulaDetails(EmpSystemId, SalaryRuleId, out newFormula_Desc);
            return Json(new { ResultOpenHead, newFormula_Desc, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        public JsonResult ShowPFSetting(string EmpSystemId, CustomParaPFSetting PFSettingModel)
        {
            return Json(new { PFCheckAndUnCheck = _employeePromotionService.PFCheckAndUnCheck(EmpSystemId, PFSettingModel), Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }
        //[HttpGet, Authorize]
        //public JsonResult PFCheckAndUnCheck(string EmpSystemId, string IsbuttonPFClicked)
        //{
        //    return Json(new { PFCheckAndUnCheck = _employeePromotionService.PFCheckAndUnCheck(EmpSystemId, IsbuttonPFClicked), Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        //}
        [HttpGet]
        public JsonResult PFCheckAndUnCheckDone(string EmpSystemId, bool IsPFEntitle, string PFEffectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeePromotionService.PFCheckAndUnCheckDone(EmpSystemId, IsPFEntitle, PFEffectiveDate, false, identity.Name);
            return Json(new { Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }











        #endregion -- Operations
    }
}
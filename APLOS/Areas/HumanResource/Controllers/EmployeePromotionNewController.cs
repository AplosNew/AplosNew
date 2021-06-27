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
    public class EmployeePromotionNewController : BaseController
    {
        #region Constructor

        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly EmployeePromotionNewService _employeePromotionService;
        public EmployeePromotionNewController(
            IEmployeeInformationService employeeInformationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeProfileService employeeProfileService
            , EmployeePromotionNewService employeePromotionService
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

        [HttpPost,Authorize]
        public JsonResult Save(List<XLUploadDetail> List)
        {
            _employeeProfileService.Insert(List);
            return Json(new { Message = "Data Uploaded Successfully" });

        }

        #region Load Employee
        [HttpGet, Authorize]
        public JsonResult GetSalaryStrcApprovedEmployeeById(string EmpSystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = _employeePromotionService.GetSalaryStrcApprovedEmployeeById(EmpSystemId, identity.CompanyGroupId, identity.PlantId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
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


        [HttpGet, Authorize]
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

        [HttpPost, Authorize]
        public JsonResult Update(EmployeeInformation employeeInformation, IncrementHistoryModel incrementHistory)
        {
            DataSet dsEmp = null;
            GetEmployeeInfo(employeeInformation.SystemId, out dsEmp);
            clsSalaryStructureAplos ob = new clsSalaryStructureAplos();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (incrementHistory.IsPromotion == true)
            {
                incrementHistory.AddedBy = identity.Name;
                incrementHistory.UpdatedBy = identity.Name;
                incrementHistory.AddedFromIP = identity.IPAddress;
                incrementHistory.UpdatedFromIP = identity.IPAddress;

                incrementHistory.ToGivenDesignationId = employeeInformation.GivenDesignationId;
                incrementHistory.ToLegalDesignationId = employeeInformation.LegalDesignationId;
                incrementHistory.ToBudgetCode = employeeInformation.BudgetCode;

                if (dsEmp.Tables[0].Rows.Count>0)
                {
                    incrementHistory.FromGivenDesignationId = dsEmp.Tables[0].Rows[0]["GivenDesignationId"].ToString();
                    incrementHistory.FromLegalDesignationId = dsEmp.Tables[0].Rows[0]["LegalDesignationId"].ToString();
                    incrementHistory.FromBudgetCode = dsEmp.Tables[0].Rows[0]["BudgetCode"].ToString();
                }
               


                ob.SaveIncrementHistoryData(incrementHistory);
            }

            _employeeProfileService.UpdateBudgetCode(employeeInformation);

            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }

        public void GetEmployeeInfo(string strEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM EmployeeInformation where SystemId = '" + strEmpSystemID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function 

        [HttpPost]
        public JsonResult UpdateSalaryStracture(EmployeeInformation employeeInformation, EmpSalaryInfoModel EmpSalaryInfo, List<PFSettingTemp> AdditionalPolicySettingModel, IEnumerable<EmpSalaryInfoDefineModel> EmpSalaryInfoDefineNew, IncrementHistoryModel incrementHistory)
        {
            List<PFEmployeeVoluntaryValueTemp> oPFEmployeeVoluntaryValue = new List<PFEmployeeVoluntaryValueTemp>();
            List<EmployeeEligibleForSalaryHeadEnum> oEmployeeEligibleForSalaryHeadEnum = new List<EmployeeEligibleForSalaryHeadEnum>();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (AdditionalPolicySettingModel != null)
            {
                foreach (PFSettingTemp item in AdditionalPolicySettingModel)
                {
                    PFEmployeeVoluntaryValueTemp ovpf = new PFEmployeeVoluntaryValueTemp();
                    EmployeeEligibleForSalaryHeadEnum o = new EmployeeEligibleForSalaryHeadEnum();
                    o.Id = GetDPK();
                    o.SalaryHeadEnum = item.SalaryHeadEnum;
                    //o.SalaryStructureId = item.IsEntitle;
                    o.IsEligible = item.IsEntitle;
                    o.EmpSystemId = employeeInformation.SystemId;
                    o.CompanyGroupId = identity.CompanyGroupId;
                    o.PlantId = identity.PlantId;
                    o.AddedBy = identity.Name;
                    o.AddedDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    o.AddedFromIp = identity.IPAddress;
                    oEmployeeEligibleForSalaryHeadEnum.Add(o);

                    if (item.SalaryHeadEnum.ToUpper() == "VPF".ToUpper())
                    {
                        if (item.IsEntitle == true)
                        {
                            ovpf.Id = GetVPFPK();
                            ovpf.EmpSystemId = employeeInformation.SystemId;
                            if (item.IsEntitle == true)
                            {
                                ovpf.VoluntaryPFValue = item.Percentage;
                            }
                            else
                            {
                                ovpf.VoluntaryPFValue = "0";
                            }
                            ovpf.EffectiveDate = item.EffectiveDate;
                            ovpf.AddedBy = identity.Name;
                            ovpf.AddedDate = DateTime.Now.ToString("dd-MMM-yyyy");
                            ovpf.AddedFromIP = identity.IPAddress;
                            oPFEmployeeVoluntaryValue.Add(ovpf);
                        }

                    }



                }

            }


            _employeePromotionService.UpdateSalaryStractureForIncrement(employeeInformation
                , oEmployeeEligibleForSalaryHeadEnum, oPFEmployeeVoluntaryValue
                , EmpSalaryInfo
                , EmpSalaryInfoDefineNew
                , incrementHistory);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public JsonResult ReCalculateSalaryStracture(IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefineOld)
        {

            IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefineNew = null;
            _employeePromotionService.ReCalculateSalaryStracture(EmpSalaryInfoDefineOld, out EmpSalaryInfoDefineNew);
            return Json(new { data = EmpSalaryInfoDefineNew, Message = AplosMessage.Updated });
        }
        private string GetDPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EmpEligibleSHE", out idFromDB);
            systemID = "EESH-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }
        private string GetVPFPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EmpEligibleSHE", out idFromDB);
            systemID = idFromDB;
            sID = systemID.Trim();
            return sID;

        }


        [Authorize]
        public JsonResult CalculateSalary(EmployeeInformation employeeInformation
            , string IsbuttonPFClicked
            , bool IsPFEntitle
            , bool IsVPFEntitle
            , string VPFPescentage
            , bool IsESICEntitle, bool IsBonusEntitle
            , string SalaryRuleMasterSystemID
            , IEnumerable<OpenHeadModelNew> EmpSalaryOpenHeadNew, string EffectiveDate)
        {
            CustomParaAdditionalPolicySetting para;
            string newGross = string.Empty;
            string newCTC = string.Empty;
            string newFormula_Desc = string.Empty;

            bool IsPFEntitleNew = false;
            bool IsPFMandatoryNew = false;
            bool IsPFPolicyDefined = false;
            bool IsVPFMandatoryNew = false;
            bool IsVPFEntitleNew = false;
            bool IsPFOptionalNew = false;

            bool IsESICMandatoryNew = false;
            bool IsESICEntitleNew = false;
            bool IsESICPolicyDefined = false;
            bool IsESICOptionalNew = false;


            bool IsBonusEntitleNew = false;
            bool IsBonusMandatoryNew = false;
            bool IsBonusPolicyDefined = false;

            DataTable dsLocal = null;
            IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefine = null;
            _employeePromotionService.CalculateSalary(employeeInformation
                , IsbuttonPFClicked
                , IsPFEntitle
                , IsVPFEntitle, VPFPescentage
                , IsESICEntitle, IsBonusEntitle
                , SalaryRuleMasterSystemID
                , EmpSalaryOpenHeadNew, EffectiveDate
                , out EmpSalaryInfoDefine
                , out newGross
                , out newCTC
                , out newFormula_Desc
                , out para);

            if (para != null)
            {

                IsPFEntitleNew = para.IsPFEntitle;
                IsPFMandatoryNew = para.IsPFMandatory;
                IsPFPolicyDefined = para.IsPFPolicyDefined;
                //IsPFOptionalNew = para.IsPFOptionalNew;
                IsVPFMandatoryNew = para.IsVoluntaryPFEntitle;
                IsVPFEntitleNew = para.IsVoluntaryPFEntitle;

                IsESICMandatoryNew = para.IsESICMandatory;
                IsESICEntitleNew = para.IsESICEntitle;
                IsESICPolicyDefined = para.IsESICPolicyDefined;

                //IsESICOptionalNew = para.IsESICOptionalNew;
                IsBonusMandatoryNew = para.IsBonusRtnMandatory;
                IsBonusEntitleNew = para.IsBonusRtnEntitle;
                IsBonusPolicyDefined = para.IsBonusRtnPolicyDefined;


            }


            return Json(new
            {
                EmpSalaryInfoDefine
                ,
                newGross
                ,
                newCTC
                ,
                newFormula_Desc





                ,
                IsPFEntitleNew,
                IsPFMandatoryNew,
                IsPFOptionalNew
                ,
                IsVPFEntitleNew,
                IsVPFMandatoryNew,
                IsPFPolicyDefined
                ,
                IsESICMandatoryNew

                ,
                IsESICEntitleNew
                ,
                IsESICPolicyDefined

                ,
                IsBonusMandatoryNew
                ,
                IsBonusEntitleNew
                ,
                IsBonusPolicyDefined


                ,
                Message = AplosMessage.Updated
            });
        }

        [Authorize]
        public JsonResult GetSalaryFormulaDetails(EmployeeInformation employeeInformation
            , string IsbuttonPFClicked
            , bool IsPFEntitle
            , bool IsVPFEntitle, string VPFPersentage
            , bool IsESICEntitle
            , bool IsBonusEntitle
            , string SalaryRuleMasterSystemID,string EffectiveDate
            , IEnumerable<OpenHeadModelNew> EmpSalaryOpenHeadNew)
        {
            CustomParaAdditionalPolicySetting para = null;
            string newGross = string.Empty;
            string newCTC = string.Empty;
            string newFormula_Desc = string.Empty;

            IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefine = null;
            _employeePromotionService.CalculateSalary(employeeInformation
                , IsbuttonPFClicked
                , IsPFEntitle
                , IsVPFEntitle, VPFPersentage
                , IsESICEntitle
                , IsBonusEntitle
                , SalaryRuleMasterSystemID
                , EmpSalaryOpenHeadNew, EffectiveDate
                , out EmpSalaryInfoDefine
                , out newGross
                , out newCTC
                , out newFormula_Desc
                , out para);


            return Json(new { EmpSalaryInfoDefine, newGross, newCTC, newFormula_Desc, Message = AplosMessage.Updated });
        }
        [HttpGet, Authorize]
        public JsonResult GetEmpSalaryInfoDefineData(string EmpSystemId)
        {

            return Json(_employeePromotionService.Query(EmpSystemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
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
            CustomOutParaNew outPara = null;//new CustomOutPara();
            IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefine = null;
            IEnumerable<EmpSalaryInfoDefineModelNew> EmpApprovedSalaryInfoDefine = null;
            //IEnumerable<object> EmpApprovedSalaryInfoDefine = null;
            IEnumerable<SalaryRuleModelNew> ResultSalaryRule = null;
            IEnumerable<SalaryRuleModelNew> ResultSelectedSalaryRule = null;
            IEnumerable<OpenHeadModelNew> ResultOpenHead = null;
            IEnumerable<OpenHeadModelNew> ResultApprovedOpenHead = null;
            string ResultMinWage = null;
            string ApprovalStatus = string.Empty;
            string ApprovedEffectiveDate = string.Empty;
            string ApprovedNextDueDate = string.Empty;
            string ResultEffectiveDate = string.Empty;
            string ResultGross = string.Empty;
            string ResultNetCTC = string.Empty;
            bool IsSalaryRuleEditableEmployee = false;
            //bool IsPFMandatory = false;
            //bool IsESICMandatory = false;
            //bool IsVPFMandatory = false;
            //bool IsVPFEntitle = false;
            //bool IsBonusEntitle = false;
            bool IsFreshEntry = false;
            string NewFormula_Desc = string.Empty;
            string ApprovedFormula_Desc = string.Empty;
            string UnApprovedNextDueDate = string.Empty;
            string VPFPersentage = string.Empty;
            string VPFEffectiveDate = string.Empty;



            bool IsPFEntitle = false;
            bool IsPFMandatory = false;
            bool IsPFPolicyDefined = false;

            bool IsVPFMandatory = false;
            bool IsVPFEntitle = false;

            bool IsESICMandatory = false;
            bool IsESICEntitle = false;
            bool IsESICPolicyDefined = false;


            bool IsBonusMandatory = false;
            bool IsBonusEntitle = false;
            bool IsBonusPolicyDefined = false;


            try
            {
                _employeePromotionService.LoadEmpSalaryInfoDefineData(EmpSystemId
                    , out EmpSalaryInfoDefine
                    , out EmpApprovedSalaryInfoDefine
                    , out ResultSalaryRule
                    , out ResultSelectedSalaryRule
                    , out ResultOpenHead
                    , out ResultApprovedOpenHead
                    , out outPara, out IsFreshEntry);
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




            //IsPFMandatory = outPara.IsPFMandatory;
            //IsESICMandatory = outPara.IsESICMandatory;
            //IsVPFMandatory = outPara.IsVPFMandatory;
            //IsVPFEntitle = outPara.IsVPFEntitle;
            //IsBonusEntitle = outPara.IsBonusRtnEntitle;
            VPFPersentage = outPara.VPFPersentage;
            VPFEffectiveDate = outPara.VPFEffectiveDate;


            IsPFEntitle = outPara.IsPFEntitle;
            IsPFMandatory = outPara.IsPFMandatory;
            IsPFPolicyDefined = outPara.IsPFPolicyDefined;

            //IsVPFMandatory = outPara.IsVoluntaryPFEntitle;
            //IsVPFEntitle = outPara.IsVoluntaryPFEntitle;

            IsVPFMandatory = outPara.IsVPFMandatory;
            IsVPFEntitle = outPara.IsVPFEntitle;

            IsESICMandatory = outPara.IsESICMandatory;
            IsESICEntitle = outPara.IsESICEntitle;
            IsESICPolicyDefined = outPara.IsESICPolicyDefined;


            IsBonusMandatory = outPara.IsBonusRtnMandatory;
            IsBonusEntitle = outPara.IsBonusRtnEntitle;
            IsBonusPolicyDefined = outPara.IsBonusRtnPolicyDefined;


            return Json(new
            {
                EmpSalaryInfoDefine
                ,
                EmpApprovedSalaryInfoDefine
                ,
                ResultMinWage
                ,
                ResultSalaryRule
                ,
                ResultSelectedSalaryRule
                ,
                ResultOpenHead
                ,
                ResultApprovedOpenHead
                ,
                ResultGross
                ,
                ResultNetCTC
                ,
                IsSalaryRuleEditableEmployee
                ,
                ApprovalStatus
                ,
                ApprovedEffectiveDate
                ,
                ApprovedNextDueDate
                ,
                ResultEffectiveDate
                ,
                NewFormula_Desc
                ,
                ApprovedFormula_Desc
                ,
                UnApprovedNextDueDate
                ,
                IsPFEntitle
                ,
                IsPFMandatory
                ,
                IsPFPolicyDefined

                ,
                IsVPFMandatory
                ,
                IsVPFEntitle

                ,
                IsESICMandatory
                ,
                IsESICEntitle
                ,
                IsESICPolicyDefined


                ,
                IsBonusMandatory
                ,
                IsBonusEntitle
                ,
                IsBonusPolicyDefined
                ,
                VPFPersentage
                ,
                VPFEffectiveDate
                ,
                IsFreshEntry
                ,
                Message = AplosMessage.Updated
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult SalaryRuleChange(string EmpSystemId, string SalaryRuleId)
        {

            IEnumerable<OpenHeadModel> ResultOpenHead = null;
            string newFormula_Desc = string.Empty;
            _employeePromotionService.SalaryRuleChange(EmpSystemId, SalaryRuleId, out ResultOpenHead);
            _employeePromotionService.GetFomulaDetails(EmpSystemId, SalaryRuleId, out newFormula_Desc);
            return Json(new { ResultOpenHead, newFormula_Desc, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSettingsByRule(string SalaryRuleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { data = _employeePromotionService.GetSettingsByRule(SalaryRuleId, identity.PlantId), Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult ShowPFSetting(string EmpSystemId, CustomParaPFSetting PFSettingModel)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { PFCheckAndUnCheck = _employeePromotionService.PFTagUnTagEmpList(EmpSystemId, identity.PlantId), Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }
        //[HttpPost, Authorize]
        //public JsonResult xShowPFSetting(string EmpSystemId, CustomParaPFSetting PFSettingModel)
        //{
        //    return Json(new { PFCheckAndUnCheck = _employeePromotionService.PFCheckAndUnCheck(EmpSystemId, PFSettingModel), Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        //}
        //[HttpGet, Authorize]
        //public JsonResult PFCheckAndUnCheck(string EmpSystemId, string IsbuttonPFClicked)
        //{
        //    return Json(new { PFCheckAndUnCheck = _employeePromotionService.PFCheckAndUnCheck(EmpSystemId, IsbuttonPFClicked), Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        //}
        [HttpGet, Authorize]
        public JsonResult PFCheckAndUnCheckDone(string EmpSystemId, bool IsPFEntitle, string PFEffectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeePromotionService.PFCheckAndUnCheckDone(EmpSystemId, IsPFEntitle, PFEffectiveDate, false, identity.Name);
            return Json(new { Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public JsonResult GetLegalDesignation()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = _employeePromotionService.GetLegalDesignation();
            return Json(data, JsonRequestBehavior.AllowGet);
        }







        #endregion -- Operations
    }
}
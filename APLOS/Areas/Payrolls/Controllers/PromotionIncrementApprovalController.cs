#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Payrolls;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.Employees;
using Library.Service.Properties;
using System.Linq;
using System.Web.Mvc;
using Library.Data;
using Library.Service.HumanResources;
using System;

#endregion

namespace Aplos.Areas.Payrolls.Controllers
{
    public class PromotionIncrementApprovalController : BaseController
    {
        #region Constructor 
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly EmployeePromotionNewService _employeePromotionService;
        public PromotionIncrementApprovalController(
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

        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region -- Increment



        [HttpGet, Authorize]
        public JsonResult GetEmployeeListForSalaryStrcApproval()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_employeePromotionService.GetSalaryStrcUnApprovedEmployee(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(_employeePromotionService.GetEmployeeListForSalaryStrcApproval(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }



        [HttpGet]
        public JsonResult LoadEmpSalaryInfoDataForApproval(string EmpSystemId)
        {
            CustomOutPara outPara = null;//new CustomOutPara();
            IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefine = null;
            IEnumerable<EmpSalaryInfoDefineModelNew> EmpApprovedSalaryInfoDefine = null;
            IEnumerable<SalaryRuleModel> ResultSalaryRule = null;
            string ResultSelectedSalaryRule = null;
            IEnumerable<OpenHeadModel> ResultOpenHead = null;
            IEnumerable<OpenHeadModel> ResultApprovedOpenHead = null;
            string ResultMinWage = null;
            string ApprovalStatus = string.Empty;
            string ApprovedEffectiveDate = string.Empty;
            string ApprovedNextDueDate = string.Empty;
            string ResultEffectiveDate = string.Empty;
            string ResultTatalGross = string.Empty;
            string ResultCTC = string.Empty;
            string ResultNetpay = string.Empty;
            bool IsSalaryRuleEditableEmployee = false;
            string NewFormula_Desc = string.Empty;
            string ApprovedFormula_Desc = string.Empty;
            string UnApprovedNextDueDate = string.Empty;
            try
            {
                _employeePromotionService.LoadEmpSalaryInfoDataForapproval(EmpSystemId, out EmpSalaryInfoDefine,  out ResultSelectedSalaryRule, out outPara);
            }
            catch (Exception ex)
            {

                throw ex;
            }

            if (EmpSalaryInfoDefine.Count()>0)
            {
               

                foreach (EmpSalaryInfoDefineModelNew item in EmpSalaryInfoDefine)
                {
                    if (item.HeadCategory.ToString().ToUpper() == "TOTAL GROSS")
                    {
                        ResultTatalGross =item.EntryAmount.ToString();
                    }
                    if (item.HeadCategory.ToString().ToUpper() == "CTC")
                    {
                        ResultCTC = item.EntryAmount.ToString();
                    }
                    if (item.HeadCategory.ToString().ToUpper() == "NET PAYABLE")
                    {
                        ResultNetpay = item.EntryAmount.ToString();
                    }
                }
            }
         


            ResultMinWage = outPara.ResultMinWage;
            ApprovalStatus = outPara.ApprovalStatus;
            ApprovedEffectiveDate = outPara.ApprovedEffectiveDate;
            ApprovedNextDueDate = outPara.ApprovedNextDueDate;
            ResultEffectiveDate = outPara.ResultEffectiveDate;
          
            IsSalaryRuleEditableEmployee = outPara.IsSalaryRuleEditableEmployee;
            NewFormula_Desc = outPara.NewFormula_Desc;
            ApprovedFormula_Desc = outPara.ApprovedFormula_Desc;
            UnApprovedNextDueDate = outPara.UnApprovedNextDueDate;
            return Json(new { EmpSalaryInfoDefine, EmpApprovedSalaryInfoDefine, ResultMinWage, ResultSalaryRule, ResultSelectedSalaryRule, ResultOpenHead, ResultApprovedOpenHead, ResultTatalGross, ResultCTC, ResultNetpay, IsSalaryRuleEditableEmployee, ApprovalStatus, ApprovedEffectiveDate, ApprovedNextDueDate, ResultEffectiveDate, NewFormula_Desc, ApprovedFormula_Desc, UnApprovedNextDueDate, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }







        [HttpPost]
        public JsonResult SaveSalaryStructureApprovalData(string EmpSystemId)
        {
            CustomParaNew para = new CustomParaNew();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            para.EmployeeId = EmpSystemId;
            para.PlantId = identity.PlantId;
            para.CompanyId = identity.CompanyId;
            para.CompanyGroupId = identity.CompanyGroupId;
            para.User = identity.Name;

            _employeePromotionService.SaveSalaryStructureApprovalData(para);
            return Json(new {  Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        #endregion



        #region -- Promotion



        [HttpGet, Authorize]
        public JsonResult GetEmployeeListForPromotionApproval()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_employeePromotionService.GetSalaryStrcUnApprovedEmployee(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(_employeePromotionService.GetEmployeeListForPromotionApproval(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }



       






        [HttpPost]
        public JsonResult SavePromotionApprovalData(string EmpSystemId)
        {
            CustomParaNew para = new CustomParaNew();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            para.EmployeeId = EmpSystemId;
            para.PlantId = identity.PlantId;
            para.CompanyId = identity.CompanyId;
            para.CompanyGroupId = identity.CompanyGroupId;
            para.User = identity.Name;

            _employeePromotionService.SaveSalaryStructureApprovalData(para);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
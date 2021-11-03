#region Using
using System;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Syncfusion.XlsIO;
using Library.Data.Sql;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class BudgetCodeChangeController : BaseController
    {
        #region Constructor

        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public BudgetCodeChangeController(
              IEmployeeInformationService employeeInformationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeProfileService employeeProfileService
            , ISqlRepository sqlRepository
           )
        {
            _employeeInformationService = employeeInformationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Pages

        [HttpGet, Authorize]
        public JsonResult GetEmployeeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployeeService.Query(parameters, identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Update(EmployeeInformation employeeInformation)
        {
            _employeeProfileService.UpdateBudgetCode(employeeInformation);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public JsonResult GetGivenDesignationByLegalDesignationCbo(string legalDesignationId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT B.DesignationId, C.UserName FROM [MST].[DesignationMasterLegalDesignation] A
                            INNER JOIN  [MST].[DesignationMaster] B ON B.Id=A.DesignationMasterId
                            INNER JOIN HKP.Designation C ON C.Id=B.DesignationId
                            WHERE A.LegalDesignationId='" + legalDesignationId + "'";

                return Json(_sqlRepository.GetCombo(strSQL, "DesignationId", "UserName"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }//End Function

        [HttpGet, Authorize]
        public JsonResult GetInActiveLegalDesignaion(string legalDesignationId)
        {
            string sql;
          
            try
            {
                sql = @"SELECT Active FROM  [HKP].[LegalDesignation] WHERE id='" + legalDesignationId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetLegalSalaryGradeDesignation(string legalDesignationId)
        {
            string sql;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                sql = @"SELECT LegalDesignationId FROM [MST].[LegalSalaryGradeDesignation] WHERE PlantId='" + identity.PlantId + "' AND LegalDesignationId='" + legalDesignationId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SyncGivenDesignation()
        {
            _employeeProfileService.UpdateGivenDesignation();
            return Json(new { Message = AplosMessage.Updated });
        }

        

    }
}
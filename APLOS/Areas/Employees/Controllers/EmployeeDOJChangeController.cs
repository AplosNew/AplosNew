using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;

namespace Aplos.Areas.Employees.Controllers
{

    public class EmployeeDOJChangeController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;
        public EmployeeDOJChangeController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion



        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations


        [HttpGet, Authorize]
        public ActionResult GetSeparationTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,UserName FROM hkp.[SeparationType] ORDER BY Sequence";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet,Authorize]
        public ActionResult GetEmployeeFinalSettlementlist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT  EI.SystemId
                          ,EI.EmployeeCode
                         ,EI.EmployeeName
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         , DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         , E.UserName EntityName
                         , FS.Id, FS.SeparationTypeId,st.UserName SeparationTypeName, FORMAT(FS.FinalSettlementDate,'dd-MMM-yyyy') FinalSettlementDate, FS.FormulaDes, FS.PolicyYearNo, FS.PolicyDayNo, FS.SeparationTypeAmount, FS.GratuityAmount, FS.LvEncashmentAmount, FS.OthersAmount, FS.DeductionAmount, FS.TenureDayNo, FS.TenureMonthNo, FS.TenureYearNo, FS.Remarks
                         FROM [dbo].[EmployeeFinalSettlement] AS FS
                         LEFT JOIN HKP.SeparationType AS st ON st.Id = FS.SeparationTypeId
                         LEFT JOIN dbo.Employeeinformation EI ON EI.SystemId = FS.EmpSystemId
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id 
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId	
                         WHERE FS.PlantId='" + identity.PlantId + @"'  ORDER BY  CONVERT(DATETIME,FS.FinalSettlementDate) DESC";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult LoadEmployeelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"SELECT  EI.SystemId
                         ,EI.EmployeeCode
                         ,EI.EmployeeName
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         , DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         , E.UserName EntityName
                        
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId				
                         WHERE   EI.EmployeeStatus='Active' AND  EI.PlantId='" + identity.PlantId + @"' ORDER BY ei.EmployeeCode";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult SeparationTypeSelectedChange(string EmpSystemId)
        {
            string DOS = string.Empty;
            DataSet dsSalary = null;
            clsFinalSettlement ob = new clsFinalSettlement();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            EmployeeFinalSettlement data = ob.CalculateFinalSettlementValue(EmpSystemId, identity.PlantId,out DOS);
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //string sql = @"SELECT * FROM hkp.[SeparationType] Where Id='"+ SeparationTypeId+"'";

            //var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SaveDOJ(string EmpId,string NewDOJ)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string YearlyCalendarId = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsEmployeeFinalSettlement;
            try
            {
                string sql = @" UPDATE EmployeeInformation SET DOJ = '" + NewDOJ + @"'   WHERE SystemId='" + EmpId + @"'  
                                                             
                                UPDATE EmpDateWiseJobLocation SET EffectiveDate = '" + NewDOJ + @"'  WHERE EmpSystemID='" + EmpId + @"' 
						                            AND  SystemID=( SELECT TOP 1 SystemID FROM  EmpDateWiseJobLocation WHERE EmpSystemID='" + EmpId + @"' ORDER BY EffectiveDate) 
                                DELETE  FROM EmpDateWiseJobLocation WHERE EffectiveDate<'" + NewDOJ + @"'  AND EmpSystemID='" + EmpId + @"'   
   
                                UPDATE EmployeeShiftAssign SET EffectiveDate = '" + NewDOJ + @"'  WHERE EmpSystemID='" + EmpId + @"' 
						                            AND  SystemID=( SELECT TOP 1 SystemID FROM  EmployeeShiftAssign WHERE EmpSystemID='" + EmpId + @"' ORDER BY EffectiveDate) 
                                DELETE  FROM EmployeeShiftAssign WHERE EffectiveDate<'" + NewDOJ + @"'  AND EmpSystemID='" + EmpId + @"'    
   
                                UPDATE EmployeeWeekOffByDay SET EffectiveDate = '" + NewDOJ + @"'  WHERE EmpSystemID='" + EmpId + @"' 
			                            AND  SystemID=( SELECT TOP 1 SystemID FROM  EmployeeWeekOffByDay WHERE EmpSystemID='" + EmpId + @"' ORDER BY EffectiveDate)  
                                DELETE FROM EmployeeWeekOffByDay WHERE EffectiveDate<'" + NewDOJ + @"'  AND EmpSystemID='" + EmpId + @"'  

                                DELETE FROM EmpDateWiseShiftAssign WHERE WorkDate<'" + NewDOJ + @"'  AND EmpSystemID='" + EmpId + @"' 
                                DELETE FROM dbo.ManualEntryRemarks where RowId IN(Select RowId FROM AttdnProcessData WHERE WorkDate<'" + NewDOJ + @"'  AND EmpSystemID='" + EmpId + @"') 
                                DELETE FROM AttdnProcessData WHERE WorkDate<'" + NewDOJ + @"'  AND EmpSystemID='" + EmpId + @"' 
";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsEmployeeFinalSettlement, false, "1");



             
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetDataForEdit(string Id)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string FinalSettlementSql = @"SELECT FS.*,ST.UserName SeparationTypeName FROM [dbo].[EmployeeFinalSettlement] AS FS
                                          LEFT JOIN [HKP].[SeparationType] AS ST ON ST.Id = FS.SeparationTypeId
                                          WHERE FS.PlantId='" + identity.PlantId + @"' AND FS.Id='" + Id + @"'";
            string EmployeeInfosql = @"SELECT  EI.SystemId
                                                 ,EI.EmployeeCode
                                                 ,EI.EmployeeName
                                                 , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                                                 , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                                                 , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                                                 , DG.UserName LegalDesignation
                                                 , DP.UserName Department
                                                 , PMB.Code,PR.UserName PositionName
                                                 , E.UserName EntityName
                        
                                                 FROM dbo.Employeeinformation EI
                                                 LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                                                 LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                                                 LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                                                 LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                                 LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                                 LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                                                 LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                                                 LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId				
                                                      WHERE EI.SystemId IN (SELECT EmpSystemId FROM [dbo].[EmployeeFinalSettlement] WHERE id='" + Id + @"') AND 
                                                            EI.PlantId='" + identity.PlantId + @"' ORDER BY CONVERT(INT, ei.EmployeeCode) ";

            var FinalSettlement = _sqlRepository.GetDataCollection(FinalSettlementSql);
            var EmployeeInfo = _sqlRepository.GetDataCollection(EmployeeInfosql);

            return Json(new { FinalSettlement, EmployeeInfo }, JsonRequestBehavior.AllowGet);
        }



        //[HttpGet, Authorize]
        //public ActionResult xGetSeparationTypelist()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    string sql = @"SELECT Id
        //                        , Sequence
        //                        , Code
        //                        , ShortName
        //                        , StandardName
        //                        , UserName
        //                        , [Description]
        //                        , Remarks
        //                        , FormulaDes	
        //                        , FormulaDesID	
        //                        , PlantID	
        //                        , IsGratuityApplicable
        //                        , IsActive      
        //                         FROM [HKP].[SeparationType]
        //                         WHERE PlantID='" + identity.PlantId + @"'";


        //    var data = _sqlRepository.GetDataCollection(sql);

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public ActionResult GetEmploymentTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT UserName  FROM EmploymentTypeEnum  ";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet,Authorize]
        public JsonResult GetAutoSequence()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT max(Sequence)+1 Sequence FROM hkp.[SeparationType] WHERE PlantID='" + identity.PlantId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        public void Caculate()
        {

        }
        #endregion
    }
   
}
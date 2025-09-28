using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Biometrics;
using Library.Model.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeDeleteController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeaveTransactionService _maternityLeaveTransactionService;
        private object companyGroupId;

        public EmployeeDeleteController(
              IMaternityLeaveTransactionService maternityLeaveTransactionService
            , ISqlRepository sqlRepository

            )
        {
            _maternityLeaveTransactionService = maternityLeaveTransactionService;
            _sqlRepository = sqlRepository;

        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        
        #endregion -- Pages

        #region -- Operations


        [HttpGet,Authorize]
        public JsonResult getemployeeDelete()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(_maternityLeaveTransactionService.getemployeeDelete(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet,Authorize]
        public JsonResult getFixedOTemployee( string YearNo,string MonthNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_maternityLeaveTransactionService.getFixedOTemployee(YearNo, MonthNo,identity.PlantId,identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(string empSystemID)
        {

            DataSet CheckValidation = null;
             ValidationEmpDelete(empSystemID,out CheckValidation);
            if (CheckValidation.Tables[0].Rows.Count > 0)
            {
                throw new CustomException("This Employee Tag With User.");
            }

            DataSet CheckValidationstructure = null;
            ValidationSalaryStructureEmpDelete(empSystemID, out CheckValidationstructure);
            if (CheckValidationstructure.Tables[0].Rows.Count > 0)
            {
                //throw new CustomException("This Employee Already Added In Salary Structure.");
            }


            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"             DELETE FROM TaxGroupTagWithEmployee where EmpInfoSystemID='" + empSystemID + @"'
                                            DELETE FROM AttdnManualData where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM hkp.EmployeeMobileAppsAuthorization where EmployeeId='" + empSystemID + @"'
                                            DELETE FROM EmployeeBankInfo where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM SalaryInfoDefine where SalaryID IN (select SystemID FROM SalaryInfoDefineMaster where EmpInfoSystemID='" + empSystemID + @"')
                                            DELETE FROM SalaryInfoDefineMaster where EmpInfoSystemID='" + empSystemID + @"'
                                            DELETE FROM SalaryProcChild where EmpInfoSystemID='" + empSystemID + @"'
                                            DELETE FROM TRN.EmployeeLeaveSummary where EmployeeId='" + empSystemID + @"'
                                            DELETE FROM SalaryProceAttdnData where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM LeaveTransactionDetails where LvTrnsSystemID in (select SystemID from LeaveTransaction where EmpSystemID='" + empSystemID + @"')
                                            DELETE FROM LeaveTransaction where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM PFMonthlyDistributionEmployer where PFMntEmpWiseCalID in (select id from PFMonthlyEmpWiseCalculation where PFEligibleEmpID in (select id from PFEligibleEmployee where EmpSystemID='" + empSystemID + @"'))
                                            DELETE FROM PFMonthlyEmpWiseCalculation where PFEligibleEmpID in (select id from PFEligibleEmployee where EmpSystemID='" + empSystemID + @"')
                                            DELETE FROM PFEligibleEmployee where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM BonusPolicyMonthlyRetainEligibleEmployee where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM BonusPolicyMonthlyRetainDistributionPmt where BnsPlyMntRetainID in (select ID from BonusPolicyMonthlyRetainEmpWiseCalculation where EmpSystemID='" + empSystemID + @"')
                                            DELETE FROM BonusPolicyMonthlyRetainEmpWiseCalculation where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM ESICMonthlyEmpWiseCalculation where ESICEligibleEmpID in (select id from ESICEligibleEmployee where EmpSystemID='" + empSystemID + @"')
                                            DELETE FROM ESICEligibleEmployee where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.EmpDateWiseShiftAssign where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.EmployeeShiftAssign where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.SalaryIncrementNextDueDate Where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.EmployeeWeekOffByDay Where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM EmployeeDocument Where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.SalaryInfoDefineMaster where EmpInfoSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.SalaryProcChild where EmpInfoSystemID='" + empSystemID + @"'
                                            DELETE FROM TRN.EmployeeLeaveSummary where EmployeeId='" + empSystemID + @"'
                                            DELETE FROM dbo.AttdnDataMonthlySummary where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM TRN.EmployeeProbationalPeriod where EmployeeId='" + empSystemID + @"'
                                            DELETE FROM dbo.EmpDateWiseJobLocation where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM TRN.AdvanceDetail Where EmployeeId='" + empSystemID + @"'
                                            DELETE FROM dbo.EmployeeBudgetCodeHistory where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.BonusPolicyMonthlyRetainDistributionStrcPmt where  BnsPlyMntRetainID IN (select ID from dbo.BonusPolicyMonthlyRetainStrcEmpWiseCalculation where EmpSystemID = '" + empSystemID + @"') 
                                            DELETE FROM dbo.BonusPolicyMonthlyRetainStrcEmpWiseCalculation where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.EmpReferenceInformation where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.AttdnProcessData where EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.AccessControllerEmployeeTag WHERE EmpInfoSystemID= '" + empSystemID + @"'
                                            DELETE FROM MST.PayrollGroupMaster WHERE EmployeeId='" + empSystemID + @"'
                                            DELETE FROM dbo.SalaryInfoBack WHERE SalaryID IN (SELECT SystemID FROM dbo.SalaryInfoBackMaster WHERE EmpInfoSystemID='" + empSystemID + @"')
                                            DELETE FROM dbo.SalaryInfoBackMaster WHERE EmpInfoSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.EmployeeFPInformation WHERE EmpSystemId= '" + empSystemID + @"'
                                            DELETE FROM dbo.BonusPaymentActual WHERE EmpSystemID = '" + empSystemID + @"'
                                            DELETE FROM dbo.AccessControllerDeleteRequest WHERE EmpInfoSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.AttdnManualDataBackUp WHERE EmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.EmpReportingPerson WHERE RptEmpSystemID='" + empSystemID + @"'
                                            DELETE FROM dbo.CompliedEmployeeRoster WHERE EmpSystemId ='" + empSystemID + @"'
                                            DELETE FROM dbo.AttendanceRestDetail WHERE EmpSystemId ='" + empSystemID + @"'
                                            DELETE FROM dbo.CompliedShiftAssignment WHERE EmpSystemId ='" + empSystemID + @"'
                                            DELETE FROM dbo.AttdnProcessFinalData WHERE EmpSystemId ='" + empSystemID + @"'                                           
                                            DELETE FROM dbo.EmployeeOnDutyDetails where OnDutyId in(select Id from dbo.EmployeeOnDuty where EmpSystemId = '" + empSystemID + @"')
                                            DELETE FROM dbo.EmployeeOnDuty where EmpSystemId = '" + empSystemID + @"'
                                            DELETE FROM dbo.AttdnRawDataFromApp WHERE EmployeeId ='" + empSystemID + @"'
                                            DELETE FROM AttdnRawDataFromApp WHERE EmployeeId ='" + empSystemID + @"'
                                            DELETE FROM EmployeeOTEntitle where EmpSystemID ='" + empSystemID + @"'
                                            DELETE FROM dbo.EmployeeBankInfoBackUp where EmpSystemID ='" + empSystemID + @"'
                                            DELETE FROM MST.PaidHoursEmployeeAssign where EmployeeId ='" + empSystemID + @"'
                                            DELETE FROM dbo.TaxOpeningBalance where EmpInfoSystemID ='" + empSystemID + @"'
                                            DELETE FROM dbo.TaxOpeningBalance where EmpInfoSystemID ='" + empSystemID + @"'
                                            DELETE FROM SEC.[User] where EmployeeId = '" + empSystemID + @"'
                                            DELETE FROM SEC.PasswordHistory where UserId =(select Id from SEC.[USER] where EmployeeId ='" + empSystemID + @"')
                                            DELETE FROM DBO.TaxableYearlyActualIncomeSalaryHeadWise where EmpInfoSystemID = '" + empSystemID + @"'
                                            DELETE FROM dbo.TaxDeductionInfoMonthWise where EmpInfoSystemID = '" + empSystemID + @"' 
                                            DELETE FROM dbo.TaxableIncomeSalaryHeadWise where EmpInfoSystemID = '" + empSystemID + @"'                                            
                                            DELETE FROM dbo.TaxableIncomeSalaryHeadWise where EmpInfoSystemID = '" + empSystemID + @"' 
                                            DELETE FROM dbo.TaxDefineMaster where EmpInfoSystemID = '" + empSystemID + @"'
                                            DELETE FROM MST.EmployeeResponsiblePerson where EmployeeId = '" + empSystemID + @"'

	                                        -----added by shahazan

											 DELETE FROM dbo.EmployeeAttendanceGroup where EmployeeId='" + empSystemID + @"'
											 DELETE FROM dbo.FinalOT where EmpSystemID='" + empSystemID + @"'
											 DELETE from  dbo.MonthWiseExtraSalaryAmtChild where MWESAMasterSystemID in (select SystemID from  dbo.MonthWiseExtraSalaryAmtMaster where EmpInfoSystemID='" + empSystemID + @"')
											 DELETE FROM dbo.MonthWiseExtraSalaryAmtMaster where EmpInfoSystemID='" + empSystemID + @"'
											 DELETE FROM MST.CompensatoryOffEmpList where EmpSystemID='" + empSystemID + @"'
                                             DELETE from ExceptionEmployee Where EmpSystemId='" + empSystemID + @"'
                                             DELETE FROM TRN.Resignation where EmployeeId='" + empSystemID + @"'
                                             DELETE from [dbo].[EmployeeIdCardIssue] where EmpSystemId='" + empSystemID + @"'
                                             DELETE FROM EmployeeNomineeInfo where EmpSystemId='"+empSystemID+ @"'
                                             DELETE FROM [dbo].[EmployeeLandLordInfo] where EmpSystemId='" + empSystemID + @"'

											 ----added by shahazan 

                                            DELETE FROM  dbo.EmployeeDependantInfo where EmpSystemId='" + empSystemID + @"'
                                            DELETE FROM  dbo.SalaryLock where EmpSystemId='" + empSystemID + @"'
                                            DELETE FROM  dbo.EmployeeEligibleForSalaryHeadEnum where EmpSystemId='" + empSystemID + @"'
                                            DELETE FROM TRN.HolidayAbsentismAssignment where EmpSystemID='" + empSystemID + @"'


                                            DELETE FROM EmployeeInformation where systemid='" + empSystemID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations

        public void ValidationEmpDelete(string empSystemID,out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strSQL = @"select EmployeeId,CompanyGroupId from SEC.[User] where EmployeeId='"+ empSystemID + @"' and CompanyGroupId='"+identity.CompanyGroupId+"'";

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

        public void ValidationSalaryStructureEmpDelete(string empSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strSQL = @"select EmpInfoSystemID FROM (
                            select EmpInfoSystemID from SalaryInfoBackMaster  sibm
                            union 
                            SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster sidm) X
                            
                            where X.EmpInfoSystemID='"+ empSystemID + @"'";

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
    }
}
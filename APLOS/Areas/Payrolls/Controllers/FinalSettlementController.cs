using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.SalaryDisbursement;
using Library.ViewModel.Vouchers;
using OTSBD;
using Syncfusion.DocIO.DLS;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class FinalSettlementController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;
        private readonly ISalaryDisbursementService _salaryDisbursementService;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        public FinalSettlementController(ISqlRepository sqlRepository, ISalaryDisbursementService salaryDisbursementService, IAttendanceManagementService AttendanceManagementService)
        {
            _sqlRepository = sqlRepository;
            _salaryDisbursementService = salaryDisbursementService;
            _AttendanceManagementService = AttendanceManagementService;
        }
        #endregion

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult FinalSettle()
        {
            return View();
        }

        public ActionResult Approve()
        {
            return View();
        }

        public ActionResult Payment()
        {
            return View();
        }
        public ActionResult Report()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult GetEmployeeSeperationItemFormulaData(string EmpSystemId)
        {
            try
            {
                string sql = @"SELECT A.Id,A.EmpSystemId,OL.Id EmployeeSeperationItemId,OL.SandardName,OL.UserName,OL.Formula,OL.FormulaId,A.Value,OL.EntryState
,FieldDisable=CAST(CASE WHEN OL.EntryState IN('Auto','Calculate') AND OL.UserName='EarnLeave' THEN 0 WHEN OL.EntryState IN('Auto','Calculate') THEN 1 ELSE 0 END AS BIT),A.Remarks
                            FROM EmployeeSeperationItem AS OL
                            OUTER APPLY (SELECT * FROM dbo.EmployeeFullAndFinalSettlementItem WHERE EmployeeSeperationItemId=OL.Id AND ISNULL(EmpSystemId,'" + EmpSystemId + @"')='" + EmpSystemId + @"') A
							Where OL.EmployeeSeperationSetupId=(select EmployeeSeperationSetupId from [dbo].[EmpSeperationDesignationGroup] where DesignationGroupId=(select DM.DesignationGroupId from [dbo].EmployeeInformation EI
							LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId Where SystemId='" + EmpSystemId + @"'))
                            ORDER BY OL.Sequence";
                var SeperationItem = _sqlRepository.GetDataCollection(sql);

                string sqlundisbursed = @"SELECT sl.Id,sl.YearNo,sl.MonthNo
,[MonthName]=CASE WHEN sl.MonthNo=1 THEN 'Jan' WHEN sl.MonthNo=2 THEN 'Feb' WHEN sl.MonthNo=3 THEN 'Mar'
WHEN sl.MonthNo=4 THEN 'Apr' WHEN sl.MonthNo=5 THEN 'May' WHEN sl.MonthNo=6 THEN 'Jun'
WHEN sl.MonthNo=7 THEN 'Jul' WHEN sl.MonthNo=8 THEN 'Aug' WHEN sl.MonthNo=9 THEN 'Sep'
WHEN sl.MonthNo=10 THEN 'Oct' WHEN sl.MonthNo=11 THEN 'Nov' ELSE 'Dec' END
,spc.DisbusmentAmount FROM SalaryProcChild AS spc
LEFT JOIN SalaryProcMaster AS spm ON spm.SystemID = spc.SlrProcMstSystemID 
LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = spc.SalaryHeadID
LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
WHERE  spc.EmpInfoSystemID= '" + EmpSystemId + @"' AND PayableVoucherId<>'' AND sl.DisbursementVoucherId IS NULL AND sh.SalaryHead='Net Pay' AND PastDisbursed IS NULL";
                var FinalSettlementUndisbursedEarning = _sqlRepository.GetDataCollection(sqlundisbursed);

                string sqlundisbursedbonus = @"select sl.YearNo
,[MonthName]=CASE WHEN sl.MonthNo=1 THEN 'Jan' WHEN sl.MonthNo=2 THEN 'Feb' WHEN sl.MonthNo=3 THEN 'Mar'
WHEN sl.MonthNo=4 THEN 'Apr' WHEN sl.MonthNo=5 THEN 'May' WHEN sl.MonthNo=6 THEN 'Jun'
WHEN sl.MonthNo=7 THEN 'Jul' WHEN sl.MonthNo=8 THEN 'Aug' WHEN sl.MonthNo=9 THEN 'Sep'
WHEN sl.MonthNo=10 THEN 'Oct' WHEN sl.MonthNo=11 THEN 'Nov' ELSE 'Dec' END
,spc.DisbusmentAmount from SalaryProcChild SPC
left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID
Left join SalaryLock sl on sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=SPM.YearNo AND sl.MonthNo=SPM.MonthNo
LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Annual Bonus' and vd.SalaryHeadId=SPC.SalaryHeadID and vd.CrAmount>0 AND VD.AccountsGroupId=SL.AccountsGroupId 
Where HeadCategory IN('Annual Bonus Retain') AND ISNULL(SPC.DisbusmentAmount,0)!=0
AND ISNULL(sl.PayableVoucherId,'')<>'' and sl.islocked=1 AND sl.BonusDisbursementVoucherId IS NULL AND ISNULL(sl.PastBonusDisbursed,0) = 0 AND BonusDisbursementAdviceId<>'' AND SPC.EmpInfoSystemID=" + EmpSystemId + "";
                var FinalSettlementUndisbursedBonus = _sqlRepository.GetDataCollection(sqlundisbursedbonus);

                return Json(new { SeperationItem, FinalSettlementUndisbursedEarning, FinalSettlementUndisbursedBonus }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSeparationTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,UserName FROM hkp.[SeparationType] ORDER BY Sequence";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
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
                         , FS.Id, FS.SeparationTypeId
                            , st.UserName SeparationTypeName
                            , FORMAT(FS.FinalSettlementDate,'dd-MMM-yyyy') FinalSettlementDate
                            , FS.FormulaDes, FS.PolicyYearNo
                            , FS.PolicyDayNo
                            , FS.SeparationTypeAmount
                            , FS.GratuityAmount
                            , FS.LvEncashmentAmount
                            ---, FS.OthersAmount
                           --- , FS.DeductionAmount
                            , FS.TenureDayNo
                            , FS.TenureMonthNo
                            , FS.TenureYearNo
                            , FS.Remarks
                         FROM [dbo].[EmployeeFinalSettlement] AS FS
                         LEFT JOIN HKP.SeparationType AS st ON st.Id = FS.SeparationTypeId
                         INNER JOIN dbo.Employeeinformation EI ON EI.SystemId = FS.EmpSystemId
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id 
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId	
                         WHERE EI.PlantId='" + identity.PlantId + @"'  ORDER BY  CONVERT(DATETIME,FS.FinalSettlementDate) DESC";

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
                        ,SepType=STUFF((select distinct ','+ST.UserName from [HKP].[SeparationType] ST	  
											    LEFT JOIN [TRN].[Resignation] R ON R.SeparationTypeId=ST.Id
												AND R.Id=(SELECT TOP 1 Id FROM [TRN].[Resignation] MR WHERE MR.EmployeeId=R.EmployeeId ORDER BY MR.UpdatedDate DESC)
							                    where EI.SystemId=R.EmployeeId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId				
                              WHERE EI.SystemId IN (SELECT EmployeeId FROM TRN.Resignation WHERE ApprovalStatus='Approved' ) AND EI.SystemId NOT IN (SELECT EmpSystemId FROM EmployeeFinalSettlement ) AND
                                    EI.PlantId='" + identity.PlantId + @"' and isnull(DOSDate,'')<>'' ORDER BY  ei.DOS DESC";

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
        public ActionResult GetEmployeelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"SELECT Flag=CAST(0 AS bit), EI.SystemId
                         ,EI.EmployeeCode
                         ,EI.EmployeeName
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOC,'dd-MMM-yyyy') DOC
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
						 ,ResignationDate=FORMAT((SELECT TOP 1 ResignationDate FROM [TRN].[Resignation] MR WHERE MR.EmployeeId=EI.SystemId ORDER BY MR.UpdatedDate DESC),'dd-MMM-yyyy')
                         , DG.UserName LegalDesignation
                         , EDG.UserName DesignationGroup
                         , DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         , E.UserName EntityName
                        ,SepType=STUFF((select distinct ','+ST.UserName from [HKP].[SeparationType] ST	  
											    LEFT JOIN [TRN].[Resignation] R ON R.SeparationTypeId=ST.Id
												AND R.Id=(SELECT TOP 1 Id FROM [TRN].[Resignation] MR WHERE MR.EmployeeId=R.EmployeeId ORDER BY MR.UpdatedDate DESC)
							                    where EI.SystemId=R.EmployeeId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),EC.UserName EmployeeCategory
                         FROM dbo.Employeeinformation EI
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                         LEFT JOIN HKP.DesignationGroup EDG ON  EDG.Id=DM.DesignationGroupId
                              WHERE EI.SystemId IN (SELECT EmployeeId FROM TRN.Resignation WHERE ApprovalStatus='Approved' ) 
							  AND EI.SystemId NOT IN (SELECT EmpSystemId FROM EmployeeFullAndFinalSettlement) AND
                                    EI.PlantId='" + identity.PlantId + @"' and isnull(DOSDate,'')<>'' 
									ORDER BY  ei.DOS DESC";

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
            EmployeeFinalSettlement data = ob.CalculateFinalSettlementValue(EmpSystemId, identity.PlantId, out DOS);
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [Id]
                          ,[Sequence]
                         -- ,[Code]
                         -- ,[ShortName]
                         -- ,[StandardName]
                          ,[UserName]
                         -- ,[Description]
                         -- ,[Remarks]
                         -- ,[Active]
                          ,[DeductionAmount]=0.0
                           FROM [dbo].[FinalSettlementDeductionHead] WHERE Active = 1 And Category='Deduction' ORDER BY Sequence";

            var FinalSettlementDeduction = _sqlRepository.GetDataCollection(sql);


            string sqle = @"SELECT [Id]
                          ,[Sequence]
                         -- ,[Code]
                         -- ,[ShortName]
                         -- ,[StandardName]
                          ,[UserName]
                         -- ,[Description]
                         -- ,[Remarks]
                         -- ,[Active]
                          ,[Amount]=0.0
                           FROM [dbo].[FinalSettlementDeductionHead] WHERE Active = 1 And Category='Earning' ORDER BY Sequence";

            var FinalSettlementEarning = _sqlRepository.GetDataCollection(sqle);
            //var FinalSettlementDeduction = ob.GetFinalSettlementDeduction();
            string sqlRetained = @" SELECT    spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID                         
                            ,SUM(spc.DisbusmentAmount) DisbusmentAmount,'PreviousYear' as status
                           FROM  SalaryProcChild spc
                             LEFT JOIN SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                             LEFT JOIN SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                             LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID							 
                             LEFT JOIN SalaryDisbursementInAcc sd on sd.MonthNo=spm.MonthNo and sd.YearNo=spm.YearNo and sd.SalaryHeadId=spc.SalaryHeadID and sd.EmpSystemId=spc.EmpInfoSystemID
						     left join  SCS.TaxYear TC on '" + DOS + @"' between TC.startdate and TC.enddate

                             join scs.TaxYear TP on TP.EndDate < tc.StartDate and datefromparts(spm.YearNo, spm.MonthNo, 1) between TP.startdate and TP.enddate
                             WHERE  sl.IsLocked = 1 and sh.IsRetained = 1  AND

                           spc.DisbusmentAmount > 0 AND spc.EmpInfoSystemID = '" + EmpSystemId + @"'
                            and spc.PlantID = '" + identity.PlantId + @"'
                            and ISNULL(sd.Id,'')= ''
                            group by  spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID
                            union all
                            SELECT    spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID
                            ,SUM(spc.DisbusmentAmount) DisbusmentAmount,'CurrentYear' as status
                             FROM SalaryProcChild spc
                            LEFT JOIN SalaryProcMaster spm on spm.SystemID = spc.SlrProcMstSystemID
                             LEFT JOIN SalaryLock sl on sl.YearNo = spm.YearNo and sl.MonthNo = spm.MonthNo and sl.EmpSystemId = spc.EmpInfoSystemID
                             LEFT JOIN SalaryHead sh on sh.SalaryHeadID = spc.SalaryHeadID
                             LEFT JOIN SalaryDisbursementInAcc sd on sd.MonthNo = spm.MonthNo and sd.YearNo = spm.YearNo and sd.SalaryHeadId = spc.SalaryHeadID and sd.EmpSystemId = spc.EmpInfoSystemID

                             join SCS.TaxYear TC on datefromparts(spm.YearNo, spm.MonthNo, 1) between TC.startdate and TC.enddate
                          WHERE  sl.IsLocked = 1 and sh.IsRetained = 1  AND

                           spc.DisbusmentAmount > 0 AND spc.EmpInfoSystemID = '" + EmpSystemId + @"'

                           and '" + DOS + @"'  between TC.startdate and TC.enddate
                            and spc.PlantID = '" + identity.PlantId + @"'

                            and ISNULL(sd.Id,'')= ''
                            group by TC.Id,tc.StartDate,tc.EndDate,spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID";
            var FinalSettlementRetainedHead = _sqlRepository.GetDataCollection(sqlRetained);

            string sqlundisbursed = @"SELECT sl.Id,sl.YearNo,sl.MonthNo
,[MonthName]=CASE WHEN sl.MonthNo=1 THEN 'Jan' WHEN sl.MonthNo=2 THEN 'Feb' WHEN sl.MonthNo=3 THEN 'Mar'
WHEN sl.MonthNo=4 THEN 'Apr' WHEN sl.MonthNo=5 THEN 'May' WHEN sl.MonthNo=6 THEN 'Jun'
WHEN sl.MonthNo=7 THEN 'Jul' WHEN sl.MonthNo=8 THEN 'Aug' WHEN sl.MonthNo=9 THEN 'Sep'
WHEN sl.MonthNo=10 THEN 'Oct' WHEN sl.MonthNo=11 THEN 'Nov' ELSE 'Dec' END
,spc.DisbusmentAmount FROM SalaryProcChild AS spc
LEFT JOIN SalaryProcMaster AS spm ON spm.SystemID = spc.SlrProcMstSystemID 
LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = spc.SalaryHeadID
LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
WHERE  spc.EmpInfoSystemID= '" + EmpSystemId + @"' AND PayableVoucherId<>'' AND ISNULL(sl.IsDisbursed,0)=0 AND sl.DisbursementVoucherId IS NULL AND sh.SalaryHead='Net Pay'";
            var FinalSettlementUndisbursedEarning = _sqlRepository.GetDataCollection(sqlundisbursed);

            string sqlavdance = @"SELECT * FROM 
(SELECT AD.CompanyId, AD.PlantId,AD.CurrencyId, C.Code AS CurrencyCode, (select TOP 1 GLGeneralInfoId from [TRN].[AdvanceDetail] 
							        where EmployeeId=AD.EmployeeId) GLGeneralInfoId, AD.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , DP.UserName Department,LD.UserName Designation
								,  (select TOP 1 BudgetMasterId from [TRN].[AdvanceDetail] 
							        where EmployeeId=AD.EmployeeId)BudgetMasterId,  (select TOP 1 ActivityId from [TRN].[AdvanceDetail] 
							        where EmployeeId=AD.EmployeeId)ActivityId
								, SUM(AD.Amount) AS Receivable, ISNULL((select SUM(Amount)WrittenOffAmount 
								from TRN.EmployeeSubsequentTransaction where SourceType='EmployeeAdvanceWriteOff' AND  EmployeeId=AD.EmployeeId AND ISNULL(JournalType,'')<>'Salary'),0)  AS Received
                                , SUM(AD.Amount)-ISNULL((select SUM(Amount)WrittenOffAmount 
								from TRN.EmployeeSubsequentTransaction where SourceType='EmployeeAdvanceWriteOff' AND  EmployeeId=AD.EmployeeId AND ISNULL(JournalType,'')<>'Salary'),0) AS Balance
                                FROM TRN.EmployeeSubsequentTransaction AS AD
                                INNER JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=AD.EmployeeId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
								LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
								LEFT JOIN [HKP].[LegalDesignation] LD ON LD.Id=EI.LegalDesignationId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AD.CurrencyId
                                WHERE    AD.EmployeeId<>'' AND ISNULL(AD.AdvanceId,'') <>'' 
                                AND AD.SourceType in ('EmployeeAdvance', 'InterTransaction') AND AD.EmployeeId='" + EmpSystemId + @"' AND AD.PlantId='" + identity.PlantId + @"'
                                GROUP BY AD.CompanyId, AD.PlantId, AD.CurrencyId, C.Code , AD.EmployeeId, EI.EmployeeCode, EI.EmployeeName, DP.UserName,LD.UserName)X
                                WHERE X.Balance > 0";



            var avdanceData = _sqlRepository.GetDataCollection(sqlavdance);

            return Json(new { data, FinalSettlementDeduction, FinalSettlementEarning, FinalSettlementRetainedHead, FinalSettlementUndisbursedEarning, avdanceData }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult SeparationTypeSelectedChangeNew(string EmpSystemId)
        {
            string DOS = string.Empty;
            DataSet dsSalary = null;
            clsFinalSettlement ob = new clsFinalSettlement();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            EmployeeFinalSettlement data = ob.CalculateFinalSettlementValueNew(EmpSystemId, identity.PlantId, out DOS);
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [Id]
                          ,[Sequence]
                         -- ,[Code]
                         -- ,[ShortName]
                         -- ,[StandardName]
                          ,[UserName]
                         -- ,[Description]
                         -- ,[Remarks]
                         -- ,[Active]
                          ,[DeductionAmount]=0.0
                           FROM [dbo].[FinalSettlementDeductionHead] WHERE Active = 1 And Category='Deduction' ORDER BY Sequence";

            var FinalSettlementDeduction = _sqlRepository.GetDataCollection(sql);


            string sqle = @"SELECT [Id]
                          ,[Sequence]
                         -- ,[Code]
                         -- ,[ShortName]
                         -- ,[StandardName]
                          ,[UserName]
                         -- ,[Description]
                         -- ,[Remarks]
                         -- ,[Active]
                          ,[Amount]=0.0
                           FROM [dbo].[FinalSettlementDeductionHead] WHERE Active = 1 And Category='Earning' ORDER BY Sequence";

            var FinalSettlementEarning = _sqlRepository.GetDataCollection(sqle);
            //var FinalSettlementDeduction = ob.GetFinalSettlementDeduction();
            string sqlRetained = @" SELECT    spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID                         
                            ,SUM(spc.DisbusmentAmount) DisbusmentAmount,'PreviousYear' as status
                           FROM  SalaryProcChild spc
                             LEFT JOIN SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                             LEFT JOIN SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                             LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID							 
                             LEFT JOIN SalaryDisbursementInAcc sd on sd.MonthNo=spm.MonthNo and sd.YearNo=spm.YearNo and sd.SalaryHeadId=spc.SalaryHeadID and sd.EmpSystemId=spc.EmpInfoSystemID
						     left join  SCS.TaxYear TC on '" + DOS + @"' between TC.startdate and TC.enddate

                             join scs.TaxYear TP on TP.EndDate < tc.StartDate and datefromparts(spm.YearNo, spm.MonthNo, 1) between TP.startdate and TP.enddate
                             WHERE  sl.IsLocked = 1 and sh.IsRetained = 1  AND

                           spc.DisbusmentAmount > 0 AND spc.EmpInfoSystemID = '" + EmpSystemId + @"'
                            and spc.PlantID = '" + identity.PlantId + @"'
                            and ISNULL(sd.Id,'')= ''
                            group by  spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID
                            union all
                            SELECT    spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID
                            ,SUM(spc.DisbusmentAmount) DisbusmentAmount,'CurrentYear' as status
                             FROM SalaryProcChild spc
                            LEFT JOIN SalaryProcMaster spm on spm.SystemID = spc.SlrProcMstSystemID
                             LEFT JOIN SalaryLock sl on sl.YearNo = spm.YearNo and sl.MonthNo = spm.MonthNo and sl.EmpSystemId = spc.EmpInfoSystemID
                             LEFT JOIN SalaryHead sh on sh.SalaryHeadID = spc.SalaryHeadID
                             LEFT JOIN SalaryDisbursementInAcc sd on sd.MonthNo = spm.MonthNo and sd.YearNo = spm.YearNo and sd.SalaryHeadId = spc.SalaryHeadID and sd.EmpSystemId = spc.EmpInfoSystemID

                             join SCS.TaxYear TC on datefromparts(spm.YearNo, spm.MonthNo, 1) between TC.startdate and TC.enddate
                          WHERE  sl.IsLocked = 1 and sh.IsRetained = 1  AND

                           spc.DisbusmentAmount > 0 AND spc.EmpInfoSystemID = '" + EmpSystemId + @"'

                           and '" + DOS + @"'  between TC.startdate and TC.enddate
                            and spc.PlantID = '" + identity.PlantId + @"'

                            and ISNULL(sd.Id,'')= ''
                            group by TC.Id,tc.StartDate,tc.EndDate,spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID";

            var FinalSettlementRetainedHead = _sqlRepository.GetDataCollection(sqlRetained);


            return Json(new { data, FinalSettlementDeduction, FinalSettlementEarning, FinalSettlementRetainedHead }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveFinalSettlement(EmployeeFinalSettlement FinalSettlementData, List<DeductionModel> DeductionData, List<DeductionModel> EarningData, List<FinalSettlementRetainedHeadModel> FinalSettlementRetainedHead)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string YearlyCalendarId = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsEmployeeFinalSettlement = null; ;
            DataSet dsleaveEncashment = null;
            DataSet dsLvDetails = null;
            DataSet dsFinalSettlementDeductionDetails = null;
            string FinalSettlementId = string.Empty;


            List<DeductionModel> FinalSettlementDeductionDetailsData = new List<DeductionModel>();
            List<FinalSettlementRetainedHeadModel> FinalSettlementRetainedHeadDetailsData = new List<FinalSettlementRetainedHeadModel>();


            //clsLeaveEncashment olv = new clsLeaveEncashment();
            try
            {
                if (Convert.ToDateTime(FinalSettlementData.EmpDOS) > Convert.ToDateTime(FinalSettlementData.FinalSettlementDate))
                {
                    throw new Exception("Final Settlement Date cannot be less thab DOS");
                }

                if (EarningData != null)
                {
                    if (EarningData.Count > 0)
                    {
                        foreach (var item in EarningData)
                        {
                            if (item.EarningAmount > 0)
                            {
                                DeductionModel o = new DeductionModel();
                                o.Id = item.Id;
                                o.Sequence = item.Sequence;
                                o.UserName = item.UserName;
                                o.Amount = item.EarningAmount;
                                FinalSettlementDeductionDetailsData.Add(o);

                            }
                        }
                    }
                }
                if (DeductionData != null)
                {
                    if (DeductionData.Count > 0)
                    {
                        foreach (var item in DeductionData)
                        {
                            if (item.DeductionAmount > 0)
                            {
                                DeductionModel o = new DeductionModel();
                                o.Id = item.Id;
                                o.Sequence = item.Sequence;
                                o.UserName = item.UserName;
                                o.Amount = item.DeductionAmount;
                                FinalSettlementDeductionDetailsData.Add(o);

                            }
                        }
                    }
                }

                if (FinalSettlementRetainedHead != null)
                {
                    if (FinalSettlementRetainedHead.Count > 0)
                    {
                        foreach (var item in FinalSettlementRetainedHead)
                        {
                            if (item.DisbusmentAmount > 0)
                            {
                                FinalSettlementRetainedHeadModel o = new FinalSettlementRetainedHeadModel();
                                o.EmpInfoSystemID = item.EmpInfoSystemID;
                                o.SalaryHeadID = item.SalaryHeadID;
                                o.SalaryHead = item.SalaryHead;
                                o.DisbusmentAmount = item.DisbusmentAmount;
                                o.status = item.status;
                                FinalSettlementRetainedHeadDetailsData.Add(o);

                            }
                        }
                    }
                }




                string sql = @"SELECT * FROM [dbo].[EmployeeFinalSettlement] WHERE  EmpSystemId='" + FinalSettlementData.EmpSystemId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsEmployeeFinalSettlement, false, "1");



                DataView dvEmployeeFinalSettlement = new DataView(dsEmployeeFinalSettlement.Tables[0]);
                dvEmployeeFinalSettlement.RowFilter = "EmpSystemId='" + FinalSettlementData.EmpSystemId + @"'";

                if (dvEmployeeFinalSettlement.Count == 0)
                {
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EmployeeFinalSettlement", out sID);
                    DataRow dr = dsEmployeeFinalSettlement.Tables[0].NewRow();
                    FinalSettlementId = "FS" + DateTime.Now.ToString("yy") + sID;
                    dr["Id"] = FinalSettlementId;
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                    dr["SeparationTypeId"] = FinalSettlementData.SeparationTypeId;
                    dr["FinalSettlementDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                    dr["PolicyYearNo"] = FinalSettlementData.PolicyYearNo;
                    dr["PolicyDayNo"] = FinalSettlementData.PolicyDayNo;
                    dr["SeparationTypeAmount"] = FinalSettlementData.SeparationTypeAmount;

                    dr["GratuityAmount"] = FinalSettlementData.GratuityAmount;
                    dr["LvEncashmentAmount"] = FinalSettlementData.LvEncashmentAmount;
                    dr["EarningAmount"] = FinalSettlementData.EarningAmount;
                    //dr["DeductionAmount"] = FinalSettlementData.DeductionAmount;
                    dr["TenureDayNo"] = FinalSettlementData.TenureDayNo;
                    dr["TenureMonthNo"] = FinalSettlementData.TenureMonthNo;
                    dr["TenureYearNo"] = FinalSettlementData.TenureYearNo;
                    dr["Remarks"] = FinalSettlementData.Remarks;
                    dr["FormulaDes"] = FinalSettlementData.FormulaDes;
                    dr["GrossAmount"] = FinalSettlementData.GrossAmount;
                    dr["BasicAmount"] = FinalSettlementData.BasicAmount;
                    dr["SalaryRate"] = FinalSettlementData.SalaryRate;
                    dr["OTRate"] = FinalSettlementData.OTRate;

                    dr["PolicyFixedDayNo"] = FinalSettlementData.PolicyFixedDayNo;
                    dr["FixedDayAmount"] = FinalSettlementData.FixedDayAmount;
                    dr["LvEncashmentDayNo"] = FinalSettlementData.LvEncashmentDayNo;
                    dr["LvEncashmentRateAmount"] = FinalSettlementData.LvEncashmentRate;
                    dr["LastMonthProcDay"] = FinalSettlementData.LastMonthProcDay;

                    dr["LastMonthAbsentDay"] = FinalSettlementData.LastMonthAbsentDay;
                    dr["LastMonthOTHour"] = FinalSettlementData.LastMonthOTHour;
                    //dr["StampAmount"] = FinalSettlementData.StampAmount;
                    dr["LastMonthGrossAmount"] = FinalSettlementData.LastMonthGrossAmount;
                    dr["LastMonthAbsenteeismAmount"] = FinalSettlementData.LastMonthAbsenteeismAmount;
                    dr["LastMonthOTAmount"] = FinalSettlementData.LastMonthOTAmount;
                    dr["TotalPayableAmount"] = FinalSettlementData.TotalPayableAmount + FinalSettlementData.GratuityAmount;
                    dr["TotalDeductionAmount"] = FinalSettlementData.TotalDeductionAmount;
                    dr["NetPayAmount"] = FinalSettlementData.TotalNetPayAmount;
                    dr["LastMonthNetPayAmount"] = FinalSettlementData.LastMonthNetPayAmount;

                    dr["EarnLvDeductionDayNo"] = FinalSettlementData.EarnLvDeductionDayNo;
                    dr["EarnLvDeductionAmount"] = FinalSettlementData.EarnLvDeductionAmount;
                    dr["TotalRetainedAmount"] = FinalSettlementData.TotalRetainedAmount;
                    dr["NoticePeriodDayNo"] = FinalSettlementData.NoticePeriodDayNo;
                    dr["NoticePeriodAmount"] = FinalSettlementData.NoticePeriodAmount;
                    dr["NoticePeriodRate"] = FinalSettlementData.NoticePeriodRate;
                    dr["NoticePeriodType"] = FinalSettlementData.NoticePeriodType;

                    dr["GratuityDayOrYear"] = FinalSettlementData.GratuityDaysOrYear;
                    dr["GratuityNoOfDaysOrYear"] = FinalSettlementData.GratuityEligibleYearOrDays;
                    dr["GratuityRate"] = FinalSettlementData.GratuityRate;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEmployeeFinalSettlement.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dvEmployeeFinalSettlement[0].Row;
                    dr.BeginEdit();
                    FinalSettlementId = dr["Id"].ToString();
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                    dr["SeparationTypeId"] = FinalSettlementData.SeparationTypeId;
                    dr["FinalSettlementDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                    dr["PolicyYearNo"] = FinalSettlementData.PolicyYearNo;
                    dr["PolicyDayNo"] = FinalSettlementData.PolicyDayNo;
                    dr["SeparationTypeAmount"] = FinalSettlementData.SeparationTypeAmount;

                    dr["GratuityAmount"] = FinalSettlementData.GratuityAmount;
                    dr["LvEncashmentAmount"] = FinalSettlementData.LvEncashmentAmount;
                    dr["EarningAmount"] = FinalSettlementData.EarningAmount;
                    //dr["DeductionAmount"] = FinalSettlementData.DeductionAmount;
                    dr["TenureDayNo"] = FinalSettlementData.TenureDayNo;
                    dr["TenureMonthNo"] = FinalSettlementData.TenureMonthNo;
                    dr["TenureYearNo"] = FinalSettlementData.TenureYearNo;
                    dr["Remarks"] = FinalSettlementData.Remarks;
                    dr["FormulaDes"] = FinalSettlementData.FormulaDes;
                    dr["GrossAmount"] = FinalSettlementData.GrossAmount;
                    dr["BasicAmount"] = FinalSettlementData.BasicAmount;
                    dr["SalaryRate"] = FinalSettlementData.SalaryRate;
                    dr["OTRate"] = FinalSettlementData.OTRate;
                    dr["PolicyFixedDayNo"] = FinalSettlementData.PolicyFixedDayNo;
                    dr["FixedDayAmount"] = FinalSettlementData.FixedDayAmount;
                    dr["LvEncashmentDayNo"] = FinalSettlementData.LvEncashmentDayNo;
                    dr["LvEncashmentRateAmount"] = FinalSettlementData.LvEncashmentRate;
                    dr["LastMonthProcDay"] = FinalSettlementData.LastMonthProcDay;

                    dr["LastMonthAbsentDay"] = FinalSettlementData.LastMonthAbsentDay;
                    dr["LastMonthOTHour"] = FinalSettlementData.LastMonthOTHour;
                    //dr["StampAmount"] = FinalSettlementData.StampAmount;
                    dr["LastMonthGrossAmount"] = FinalSettlementData.LastMonthGrossAmount;
                    dr["LastMonthAbsenteeismAmount"] = FinalSettlementData.LastMonthAbsenteeismAmount;
                    dr["LastMonthOTAmount"] = FinalSettlementData.LastMonthOTAmount;
                    dr["TotalPayableAmount"] = FinalSettlementData.TotalPayableAmount + FinalSettlementData.GratuityAmount;
                    dr["TotalDeductionAmount"] = FinalSettlementData.TotalDeductionAmount;
                    dr["NetPayAmount"] = FinalSettlementData.TotalNetPayAmount;
                    dr["LastMonthNetPayAmount"] = FinalSettlementData.LastMonthNetPayAmount;


                    dr["EarnLvDeductionDayNo"] = FinalSettlementData.EarnLvDeductionDayNo;
                    dr["EarnLvDeductionAmount"] = FinalSettlementData.EarnLvDeductionAmount;
                    dr["TotalRetainedAmount"] = FinalSettlementData.TotalRetainedAmount;
                    dr["NoticePeriodDayNo"] = FinalSettlementData.NoticePeriodDayNo;
                    dr["NoticePeriodAmount"] = FinalSettlementData.NoticePeriodAmount;
                    dr["NoticePeriodRate"] = FinalSettlementData.NoticePeriodRate;
                    dr["NoticePeriodType"] = FinalSettlementData.NoticePeriodType;

                    dr["GratuityDaysOrYear"] = FinalSettlementData.GratuityDaysOrYear;
                    dr["GratuityNoOfDaysOrYear"] = FinalSettlementData.GratuityEligibleYearOrDays;
                    dr["GratuityRate"] = FinalSettlementData.GratuityRate;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }


                dvEmployeeFinalSettlement.RowFilter = null;


                //Leave Encashment
                if (FinalSettlementData.LvEncashmentDayNo > 0)
                {
                    clsFinalSettlement ob = new clsFinalSettlement();
                    DataSet dsYearlyCalendar = null;
                    ob.GetYearlyCalendarIdByDOS(FinalSettlementData.EmpDOS, identity.PlantId, out dsYearlyCalendar);
                    if (dsYearlyCalendar.Tables[0].Rows.Count > 0)
                    {
                        YearlyCalendarId = dsYearlyCalendar.Tables[0].Rows[0]["Id"].ToString();
                    }


                    string sqll = @"SELECT * FROM LeaveEncashmentTransaction 
                                            WHERE ---PlantId='" + identity.PlantId + @"' AND
                                             EmpSystemId='" + FinalSettlementData.EmpSystemId + @"' AND EncashmentDate='" + Convert.ToDateTime(FinalSettlementData.FinalSettlementDate).ToString("dd-MMM-yyyy") + @"' 
                                            AND LeaveEncashmentType='Final Settlement Encashment' AND YearlyCalendarId='" + YearlyCalendarId + @"'";

                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sqll, out dsleaveEncashment, false, "1");





                    GetLeaveBalance(FinalSettlementData.EmpSystemId, out dsLvDetails);
                    DataView dvleaveEncashment = new DataView(dsleaveEncashment.Tables[0]);


                    dvleaveEncashment.RowFilter = "EmpSystemId='" + FinalSettlementData.EmpSystemId + @"' AND YearlyCalendarId='" + YearlyCalendarId + "'  AND EncashmentDate='" + Convert.ToDateTime(FinalSettlementData.FinalSettlementDate).ToString("dd-MMM-yyyy") + @"'  AND LeaveEncashmentType='Final Settlement Encashment'";

                    if (dvleaveEncashment.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MultipleaveEncashment", out sID);
                        DataRow dr = dsleaveEncashment.Tables[0].NewRow();
                        dr["Id"] = "LE" + sID;
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                        dr["LeaveEncashmentType"] = "Final Settlement Encashment";
                        dr["EncashmentDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = FinalSettlementData.LvEncashmentDayNo;
                        //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                        dr["Rate"] = FinalSettlementData.LvEncashmentRate;
                        dr["LeaveTypeSystemId"] = FinalSettlementData.LeaveTypeId;



                        dr["BasicAmmount"] = FinalSettlementData.BasicAmount;
                        dr["GrossAmmount"] = FinalSettlementData.GrossAmount;


                        //dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;




                        if (dsLvDetails.Tables[0].Rows.Count > 0)
                        {



                            dr["LegalDesignationId"] = dsLvDetails.Tables[0].Rows[0]["LegalDesignationId"].ToString();


                            dr["BroughtForward"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["BroughtForward"].ToString());
                            dr["DaysCanBeSanctioned"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString());
                            dr["CarryForward"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["CarryForward"].ToString());
                            dr["AvailedLeave"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["AvailedLeave"].ToString());

                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString()))
                            {
                                dr["PaymentMode"] = dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString()))
                            {
                                dr["BankSystemID"] = dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString()))
                            {
                                dr["BankBranchId"] = dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString()))
                            {
                                dr["BankAccNo"] = dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString();
                            }


                        }
                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;




                        dsleaveEncashment.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvleaveEncashment[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                        dr["LeaveEncashmentType"] = "Final Settlement Encashment";
                        dr["EncashmentDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = FinalSettlementData.LvEncashmentDayNo;
                        //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                        dr["Rate"] = FinalSettlementData.LvEncashmentRate;
                        dr["LeaveTypeSystemId"] = FinalSettlementData.LeaveTypeId;
                        //dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                        dr["BasicAmmount"] = FinalSettlementData.BasicAmount;
                        dr["GrossAmmount"] = FinalSettlementData.GrossAmount;

                        //dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                        //dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                        //dr["BankAccNo"] = leaveEncashment[i].BankAccNo;

                        //dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;



                        if (dsLvDetails.Tables[0].Rows.Count > 0)
                        {
                            dr["LegalDesignationId"] = dsLvDetails.Tables[0].Rows[0]["LegalDesignationId"].ToString();


                            dr["BroughtForward"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["BroughtForward"].ToString());
                            dr["DaysCanBeSanctioned"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString());
                            dr["CarryForward"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["CarryForward"].ToString());
                            dr["AvailedLeave"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["AvailedLeave"].ToString());

                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString()))
                            {
                                dr["PaymentMode"] = dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString()))
                            {
                                dr["BankSystemID"] = dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString()))
                            {
                                dr["BankBranchId"] = dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString()))
                            {
                                dr["BankAccNo"] = dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString();
                            }
                        }

                        //dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        //dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        //dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        //dr["CarryForward"] = leaveEncashment[i].CarryForward;



                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                    }
                    dvleaveEncashment.RowFilter = null;

                }

                ///deduction

                string sqld = @"select * from FinalSettlementDeductionDetails where EmployeeFinalSettlementId='" + FinalSettlementId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqld, out dsFinalSettlementDeductionDetails, false, "1");



                DataView dvFinalSettlementDeductionDetails = new DataView(dsFinalSettlementDeductionDetails.Tables[0]);
                if (FinalSettlementDeductionDetailsData.Count > 0)
                {
                    foreach (var item in FinalSettlementDeductionDetailsData)
                    {
                        dvFinalSettlementDeductionDetails.RowFilter = "EmployeeFinalSettlementId='" + FinalSettlementId + @"' and FinalSettlementDeductionHeadId='" + item.Id + @"'";

                        if (dvFinalSettlementDeductionDetails.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "FinalSettlementDeductionDetails", out sID);
                            DataRow dr = dsFinalSettlementDeductionDetails.Tables[0].NewRow();

                            dr["Id"] = "FD" + DateTime.Now.ToString("yy") + sID;
                            dr["FinalSettlementDeductionHeadId"] = item.Id.ToString();
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.Amount;



                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsFinalSettlementDeductionDetails.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            DataRow dr = dvEmployeeFinalSettlement[0].Row;
                            dr.BeginEdit();

                            dr["FinalSettlementDeductionHeadId"] = item.Id;
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.Amount;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();




                        }

                        dvFinalSettlementDeductionDetails.RowFilter = null;
                    }

                }







                ///Retained
                DataSet dsFinalSettlementRetainedDetails = null;
                string sqlRetained = @"select * from [dbo].[FinalSettlementRetainedDetails] where EmployeeFinalSettlementId='" + FinalSettlementId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlRetained, out dsFinalSettlementRetainedDetails, false, "1");



                DataView dvFinalSettlementRetainedDetails = new DataView(dsFinalSettlementRetainedDetails.Tables[0]);
                if (FinalSettlementRetainedHeadDetailsData.Count > 0)
                {
                    foreach (var item in FinalSettlementRetainedHeadDetailsData)
                    {
                        dvFinalSettlementRetainedDetails.RowFilter = "EmployeeFinalSettlementId='" + FinalSettlementId + @"' and SalaryHeadId='" + item.SalaryHeadID + @"' and Amount = '" + item.DisbusmentAmount + "'";

                        if (dvFinalSettlementRetainedDetails.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "FinalSettlementRetainedDetails", out sID);
                            DataRow dr = dsFinalSettlementRetainedDetails.Tables[0].NewRow();

                            dr["Id"] = "FD" + DateTime.Now.ToString("yy") + sID;
                            dr["SalaryHeadId"] = item.SalaryHeadID.ToString();
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.DisbusmentAmount;
                            dr["YearStatus"] = item.status;



                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsFinalSettlementRetainedDetails.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            DataRow dr = dvFinalSettlementRetainedDetails[0].Row;
                            dr.BeginEdit();

                            dr["SalaryHeadId"] = item.SalaryHeadID;
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.DisbusmentAmount;
                            dr["YearStatus"] = item.status;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();




                        }

                        dvFinalSettlementRetainedDetails.RowFilter = null;
                    }

                }

                clsStaticInfo obj = new clsStaticInfo();


                if (FinalSettlementData.LvEncashmentDayNo > 0)
                {
                    obj.SaveDataSets(dsEmployeeFinalSettlement, dsleaveEncashment, dsFinalSettlementDeductionDetails, dsFinalSettlementRetainedDetails);
                }
                else
                {
                    obj.SaveDataSets(dsEmployeeFinalSettlement, dsFinalSettlementDeductionDetails, dsFinalSettlementRetainedDetails);
                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult SaveFinalSettlementNew(EmployeeFinalSettlement FinalSettlementData, List<DeductionModel> DeductionData, List<DeductionModel> EarningData, List<FinalSettlementRetainedHeadModel> FinalSettlementRetainedHead, List<Dictionary<string, object>> UndisbursedEarningList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string YearlyCalendarId = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsEmployeeFinalSettlement = null; ;
            DataSet dsleaveEncashment = null;
            DataSet dsLvDetails = null;
            DataSet dsFinalSettlementDeductionDetails = null;
            string FinalSettlementId = string.Empty;


            List<DeductionModel> FinalSettlementDeductionDetailsData = new List<DeductionModel>();
            List<FinalSettlementRetainedHeadModel> FinalSettlementRetainedHeadDetailsData = new List<FinalSettlementRetainedHeadModel>();


            //clsLeaveEncashment olv = new clsLeaveEncashment();
            try
            {
                if (Convert.ToDateTime(FinalSettlementData.EmpDOS) > Convert.ToDateTime(FinalSettlementData.FinalSettlementDate))
                {
                    throw new Exception("Final Settlement Date cannot be less thab DOS");
                }

                if (EarningData != null)
                {
                    if (EarningData.Count > 0)
                    {
                        foreach (var item in EarningData)
                        {
                            if (item.EarningAmount > 0)
                            {
                                DeductionModel o = new DeductionModel();
                                o.Id = item.Id;
                                o.Sequence = item.Sequence;
                                o.UserName = item.UserName;
                                o.Amount = item.EarningAmount;
                                FinalSettlementDeductionDetailsData.Add(o);

                            }
                        }
                    }
                }
                if (DeductionData != null)
                {
                    if (DeductionData.Count > 0)
                    {
                        foreach (var item in DeductionData)
                        {
                            if (item.DeductionAmount > 0)
                            {
                                DeductionModel o = new DeductionModel();
                                o.Id = item.Id;
                                o.Sequence = item.Sequence;
                                o.UserName = item.UserName;
                                o.Amount = item.DeductionAmount;
                                FinalSettlementDeductionDetailsData.Add(o);

                            }
                        }
                    }
                }

                if (FinalSettlementRetainedHead != null)
                {
                    if (FinalSettlementRetainedHead.Count > 0)
                    {
                        foreach (var item in FinalSettlementRetainedHead)
                        {
                            if (item.DisbusmentAmount > 0)
                            {
                                FinalSettlementRetainedHeadModel o = new FinalSettlementRetainedHeadModel();
                                o.EmpInfoSystemID = item.EmpInfoSystemID;
                                o.SalaryHeadID = item.SalaryHeadID;
                                o.SalaryHead = item.SalaryHead;
                                o.DisbusmentAmount = item.DisbusmentAmount;
                                o.status = item.status;
                                FinalSettlementRetainedHeadDetailsData.Add(o);

                            }
                        }
                    }
                }




                string sql = @"SELECT * FROM [dbo].[EmployeeFinalSettlement] WHERE  EmpSystemId='" + FinalSettlementData.EmpSystemId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsEmployeeFinalSettlement, false, "1");



                DataView dvEmployeeFinalSettlement = new DataView(dsEmployeeFinalSettlement.Tables[0]);
                dvEmployeeFinalSettlement.RowFilter = "EmpSystemId='" + FinalSettlementData.EmpSystemId + @"'";

                if (dvEmployeeFinalSettlement.Count == 0)
                {
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EmployeeFinalSettlement", out sID);
                    DataRow dr = dsEmployeeFinalSettlement.Tables[0].NewRow();
                    FinalSettlementId = "FS" + DateTime.Now.ToString("yy") + sID;
                    dr["Id"] = FinalSettlementId;
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                    dr["SeparationTypeId"] = FinalSettlementData.SeparationTypeId;
                    dr["FinalSettlementDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                    dr["PolicyYearNo"] = FinalSettlementData.PolicyYearNo;
                    dr["PolicyDayNo"] = FinalSettlementData.PolicyDayNo;
                    dr["SeparationTypeAmount"] = FinalSettlementData.SeparationTypeAmount;

                    dr["GratuityAmount"] = FinalSettlementData.GratuityAmount;
                    dr["LvEncashmentAmount"] = FinalSettlementData.LvEncashmentAmount;
                    dr["EarningAmount"] = FinalSettlementData.EarningAmount;
                    //dr["DeductionAmount"] = FinalSettlementData.DeductionAmount;
                    dr["TenureDayNo"] = FinalSettlementData.TenureDayNo;
                    dr["TenureMonthNo"] = FinalSettlementData.TenureMonthNo;
                    dr["TenureYearNo"] = FinalSettlementData.TenureYearNo;
                    dr["Remarks"] = FinalSettlementData.Remarks;
                    dr["FormulaDes"] = FinalSettlementData.FormulaDes;
                    dr["GrossAmount"] = FinalSettlementData.GrossAmount;
                    dr["BasicAmount"] = FinalSettlementData.BasicAmount;
                    dr["SalaryRate"] = FinalSettlementData.SalaryRate;
                    dr["OTRate"] = FinalSettlementData.OTRate;

                    dr["PolicyFixedDayNo"] = FinalSettlementData.PolicyFixedDayNo;
                    dr["FixedDayAmount"] = FinalSettlementData.FixedDayAmount;
                    dr["LvEncashmentDayNo"] = FinalSettlementData.LvEncashmentDayNo;
                    dr["LvEncashmentRateAmount"] = FinalSettlementData.LvEncashmentRate;
                    dr["LastMonthProcDay"] = FinalSettlementData.LastMonthProcDay;

                    dr["LastMonthAbsentDay"] = FinalSettlementData.LastMonthAbsentDay;
                    dr["LastMonthOTHour"] = FinalSettlementData.LastMonthOTHour;
                    //dr["StampAmount"] = FinalSettlementData.StampAmount;
                    dr["LastMonthGrossAmount"] = FinalSettlementData.LastMonthGrossAmount;
                    dr["LastMonthAbsenteeismAmount"] = FinalSettlementData.LastMonthAbsenteeismAmount;
                    dr["LastMonthOTAmount"] = FinalSettlementData.LastMonthOTAmount;
                    dr["TotalPayableAmount"] = FinalSettlementData.TotalPayableAmount + FinalSettlementData.GratuityAmount;
                    dr["TotalDeductionAmount"] = FinalSettlementData.TotalDeductionAmount;
                    dr["NetPayAmount"] = FinalSettlementData.TotalNetPayAmount;
                    dr["LastMonthNetPayAmount"] = FinalSettlementData.LastMonthNetPayAmount;

                    dr["EarnLvDeductionDayNo"] = FinalSettlementData.EarnLvDeductionDayNo;
                    dr["EarnLvDeductionAmount"] = FinalSettlementData.EarnLvDeductionAmount;
                    dr["TotalRetainedAmount"] = FinalSettlementData.TotalRetainedAmount;
                    dr["NoticePeriodDayNo"] = FinalSettlementData.NoticePeriodDayNo;
                    dr["NoticePeriodAmount"] = FinalSettlementData.NoticePeriodAmount;
                    dr["NoticePeriodRate"] = FinalSettlementData.NoticePeriodRate;
                    dr["NoticePeriodType"] = FinalSettlementData.NoticePeriodType;

                    dr["GratuityDayOrYear"] = FinalSettlementData.GratuityDaysOrYear;
                    dr["GratuityNoOfDaysOrYear"] = FinalSettlementData.GratuityEligibleYearOrDays;
                    dr["GratuityRate"] = FinalSettlementData.GratuityRate;
                    dr["AdvanceAmount"] = FinalSettlementData.AdvanceAmount;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEmployeeFinalSettlement.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dvEmployeeFinalSettlement[0].Row;
                    dr.BeginEdit();
                    FinalSettlementId = dr["Id"].ToString();
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                    dr["SeparationTypeId"] = FinalSettlementData.SeparationTypeId;
                    dr["FinalSettlementDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                    dr["PolicyYearNo"] = FinalSettlementData.PolicyYearNo;
                    dr["PolicyDayNo"] = FinalSettlementData.PolicyDayNo;
                    dr["SeparationTypeAmount"] = FinalSettlementData.SeparationTypeAmount;

                    dr["GratuityAmount"] = FinalSettlementData.GratuityAmount;
                    dr["LvEncashmentAmount"] = FinalSettlementData.LvEncashmentAmount;
                    dr["EarningAmount"] = FinalSettlementData.EarningAmount;
                    //dr["DeductionAmount"] = FinalSettlementData.DeductionAmount;
                    dr["TenureDayNo"] = FinalSettlementData.TenureDayNo;
                    dr["TenureMonthNo"] = FinalSettlementData.TenureMonthNo;
                    dr["TenureYearNo"] = FinalSettlementData.TenureYearNo;
                    dr["Remarks"] = FinalSettlementData.Remarks;
                    dr["FormulaDes"] = FinalSettlementData.FormulaDes;
                    dr["GrossAmount"] = FinalSettlementData.GrossAmount;
                    dr["BasicAmount"] = FinalSettlementData.BasicAmount;
                    dr["SalaryRate"] = FinalSettlementData.SalaryRate;
                    dr["OTRate"] = FinalSettlementData.OTRate;
                    dr["PolicyFixedDayNo"] = FinalSettlementData.PolicyFixedDayNo;
                    dr["FixedDayAmount"] = FinalSettlementData.FixedDayAmount;
                    dr["LvEncashmentDayNo"] = FinalSettlementData.LvEncashmentDayNo;
                    dr["LvEncashmentRateAmount"] = FinalSettlementData.LvEncashmentRate;
                    dr["LastMonthProcDay"] = FinalSettlementData.LastMonthProcDay;

                    dr["LastMonthAbsentDay"] = FinalSettlementData.LastMonthAbsentDay;
                    dr["LastMonthOTHour"] = FinalSettlementData.LastMonthOTHour;
                    //dr["StampAmount"] = FinalSettlementData.StampAmount;
                    dr["LastMonthGrossAmount"] = FinalSettlementData.LastMonthGrossAmount;
                    dr["LastMonthAbsenteeismAmount"] = FinalSettlementData.LastMonthAbsenteeismAmount;
                    dr["LastMonthOTAmount"] = FinalSettlementData.LastMonthOTAmount;
                    dr["TotalPayableAmount"] = FinalSettlementData.TotalPayableAmount + FinalSettlementData.GratuityAmount;
                    dr["TotalDeductionAmount"] = FinalSettlementData.TotalDeductionAmount;
                    dr["NetPayAmount"] = FinalSettlementData.TotalNetPayAmount;
                    dr["LastMonthNetPayAmount"] = FinalSettlementData.LastMonthNetPayAmount;


                    dr["EarnLvDeductionDayNo"] = FinalSettlementData.EarnLvDeductionDayNo;
                    dr["EarnLvDeductionAmount"] = FinalSettlementData.EarnLvDeductionAmount;
                    dr["TotalRetainedAmount"] = FinalSettlementData.TotalRetainedAmount;
                    dr["NoticePeriodDayNo"] = FinalSettlementData.NoticePeriodDayNo;
                    dr["NoticePeriodAmount"] = FinalSettlementData.NoticePeriodAmount;
                    dr["NoticePeriodRate"] = FinalSettlementData.NoticePeriodRate;
                    dr["NoticePeriodType"] = FinalSettlementData.NoticePeriodType;

                    dr["GratuityDaysOrYear"] = FinalSettlementData.GratuityDaysOrYear;
                    dr["GratuityNoOfDaysOrYear"] = FinalSettlementData.GratuityEligibleYearOrDays;
                    dr["GratuityRate"] = FinalSettlementData.GratuityRate;
                    dr["AdvanceAmount"] = FinalSettlementData.AdvanceAmount;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }


                dvEmployeeFinalSettlement.RowFilter = null;


                //Leave Encashment
                if (FinalSettlementData.LvEncashmentDayNo > 0)
                {
                    clsFinalSettlement ob = new clsFinalSettlement();
                    DataSet dsYearlyCalendar = null;
                    ob.GetYearlyCalendarIdByDOS(FinalSettlementData.EmpDOS, identity.PlantId, out dsYearlyCalendar);
                    if (dsYearlyCalendar.Tables[0].Rows.Count > 0)
                    {
                        YearlyCalendarId = dsYearlyCalendar.Tables[0].Rows[0]["Id"].ToString();
                    }


                    string sqll = @"SELECT * FROM LeaveEncashmentTransaction 
                                            WHERE ---PlantId='" + identity.PlantId + @"' AND
                                             EmpSystemId='" + FinalSettlementData.EmpSystemId + @"' AND EncashmentDate='" + Convert.ToDateTime(FinalSettlementData.FinalSettlementDate).ToString("dd-MMM-yyyy") + @"' 
                                            AND LeaveEncashmentType='Final Settlement Encashment' AND YearlyCalendarId='" + YearlyCalendarId + @"'";

                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sqll, out dsleaveEncashment, false, "1");





                    GetLeaveBalanceNew(FinalSettlementData.EmpSystemId, out dsLvDetails);
                    DataView dvleaveEncashment = new DataView(dsleaveEncashment.Tables[0]);


                    dvleaveEncashment.RowFilter = "EmpSystemId='" + FinalSettlementData.EmpSystemId + @"' AND YearlyCalendarId='" + YearlyCalendarId + "'  AND EncashmentDate='" + Convert.ToDateTime(FinalSettlementData.FinalSettlementDate).ToString("dd-MMM-yyyy") + @"'  AND LeaveEncashmentType='Final Settlement Encashment'";

                    if (dvleaveEncashment.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MultipleaveEncashment", out sID);
                        DataRow dr = dsleaveEncashment.Tables[0].NewRow();
                        dr["Id"] = "LE" + sID;
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                        dr["LeaveEncashmentType"] = "Final Settlement Encashment";
                        dr["EncashmentDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = FinalSettlementData.LvEncashmentDayNo;
                        //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                        dr["Rate"] = FinalSettlementData.LvEncashmentRate;
                        dr["LeaveTypeSystemId"] = FinalSettlementData.LeaveTypeId;



                        dr["BasicAmmount"] = FinalSettlementData.BasicAmount;
                        dr["GrossAmmount"] = FinalSettlementData.GrossAmount;


                        //dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;




                        if (dsLvDetails.Tables[0].Rows.Count > 0)
                        {



                            dr["LegalDesignationId"] = dsLvDetails.Tables[0].Rows[0]["LegalDesignationId"].ToString();


                            dr["BroughtForward"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["BroughtForward"].ToString());
                            dr["DaysCanBeSanctioned"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString());
                            dr["CarryForward"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["CarryForward"].ToString());
                            dr["AvailedLeave"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["AvailedLeave"].ToString());

                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString()))
                            {
                                dr["PaymentMode"] = dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString()))
                            {
                                dr["BankSystemID"] = dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString()))
                            {
                                dr["BankBranchId"] = dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString()))
                            {
                                dr["BankAccNo"] = dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString();
                            }


                        }
                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;




                        dsleaveEncashment.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvleaveEncashment[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                        dr["LeaveEncashmentType"] = "Final Settlement Encashment";
                        dr["EncashmentDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = FinalSettlementData.LvEncashmentDayNo;
                        //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                        dr["Rate"] = FinalSettlementData.LvEncashmentRate;
                        dr["LeaveTypeSystemId"] = FinalSettlementData.LeaveTypeId;
                        //dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                        dr["BasicAmmount"] = FinalSettlementData.BasicAmount;
                        dr["GrossAmmount"] = FinalSettlementData.GrossAmount;

                        //dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                        //dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                        //dr["BankAccNo"] = leaveEncashment[i].BankAccNo;

                        //dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;



                        if (dsLvDetails.Tables[0].Rows.Count > 0)
                        {
                            dr["LegalDesignationId"] = dsLvDetails.Tables[0].Rows[0]["LegalDesignationId"].ToString();


                            dr["BroughtForward"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["BroughtForward"].ToString());
                            dr["DaysCanBeSanctioned"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString());
                            dr["CarryForward"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["CarryForward"].ToString());
                            dr["AvailedLeave"] = clsStaticInfo.dbl(dsLvDetails.Tables[0].Rows[0]["AvailedLeave"].ToString());

                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString()))
                            {
                                dr["PaymentMode"] = dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString()))
                            {
                                dr["BankSystemID"] = dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString()))
                            {
                                dr["BankBranchId"] = dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString()))
                            {
                                dr["BankAccNo"] = dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString();
                            }
                        }

                        //dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        //dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        //dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        //dr["CarryForward"] = leaveEncashment[i].CarryForward;



                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                    }
                    dvleaveEncashment.RowFilter = null;

                }

                ///deduction

                string sqld = @"select * from FinalSettlementDeductionDetails where EmployeeFinalSettlementId='" + FinalSettlementId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqld, out dsFinalSettlementDeductionDetails, false, "1");



                DataView dvFinalSettlementDeductionDetails = new DataView(dsFinalSettlementDeductionDetails.Tables[0]);
                if (FinalSettlementDeductionDetailsData.Count > 0)
                {
                    foreach (var item in FinalSettlementDeductionDetailsData)
                    {
                        dvFinalSettlementDeductionDetails.RowFilter = "EmployeeFinalSettlementId='" + FinalSettlementId + @"' and FinalSettlementDeductionHeadId='" + item.Id + @"'";

                        if (dvFinalSettlementDeductionDetails.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "FinalSettlementDeductionDetails", out sID);
                            DataRow dr = dsFinalSettlementDeductionDetails.Tables[0].NewRow();

                            dr["Id"] = "FD" + DateTime.Now.ToString("yy") + sID;
                            dr["FinalSettlementDeductionHeadId"] = item.Id.ToString();
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.Amount;



                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsFinalSettlementDeductionDetails.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            DataRow dr = dvEmployeeFinalSettlement[0].Row;
                            dr.BeginEdit();

                            dr["FinalSettlementDeductionHeadId"] = item.Id;
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.Amount;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();




                        }

                        dvFinalSettlementDeductionDetails.RowFilter = null;
                    }

                }







                ///Retained
                DataSet dsFinalSettlementRetainedDetails = null;
                string sqlRetained = @"select * from [dbo].[FinalSettlementRetainedDetails] where EmployeeFinalSettlementId='" + FinalSettlementId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlRetained, out dsFinalSettlementRetainedDetails, false, "1");



                DataView dvFinalSettlementRetainedDetails = new DataView(dsFinalSettlementRetainedDetails.Tables[0]);
                if (FinalSettlementRetainedHeadDetailsData.Count > 0)
                {
                    foreach (var item in FinalSettlementRetainedHeadDetailsData)
                    {
                        dvFinalSettlementRetainedDetails.RowFilter = "EmployeeFinalSettlementId='" + FinalSettlementId + @"' and SalaryHeadId='" + item.SalaryHeadID + @"' and Amount = '" + item.DisbusmentAmount + "'";

                        if (dvFinalSettlementRetainedDetails.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "FinalSettlementRetainedDetails", out sID);
                            DataRow dr = dsFinalSettlementRetainedDetails.Tables[0].NewRow();

                            dr["Id"] = "FD" + DateTime.Now.ToString("yy") + sID;
                            dr["SalaryHeadId"] = item.SalaryHeadID.ToString();
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.DisbusmentAmount;
                            dr["YearStatus"] = item.status;



                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsFinalSettlementRetainedDetails.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            DataRow dr = dvFinalSettlementRetainedDetails[0].Row;
                            dr.BeginEdit();

                            dr["SalaryHeadId"] = item.SalaryHeadID;
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.DisbusmentAmount;
                            dr["YearStatus"] = item.status;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();




                        }

                        dvFinalSettlementRetainedDetails.RowFilter = null;
                    }

                }
                string lid = string.Empty;
                DataSet dsSL = null;
                if (UndisbursedEarningList != null)
                {
                    foreach (var item in UndisbursedEarningList)
                    {
                        if (lid == "")
                            lid = "'" + item["Id"] + "'";
                        else
                            lid = lid + ",'" + item["Id"] + "'";
                    }

                    string mosql = "SELECT * FROM dbo.SalaryLock WHERE Id IN (" + lid + ")";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(mosql, out dsSL, false, "1");
                    foreach (var item in UndisbursedEarningList)
                    {
                        DataView dv = new DataView(dsSL.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;

                            drmo.BeginEdit();

                            drmo["EmployeeFinalSettlementId"] = FinalSettlementId;
                            drmo["UpdatedBy"] = identity.Name;
                            drmo["UpdatedDate"] = DateTime.Now.ToString();
                            drmo["UpdatedFromIP"] = identity.IPAddress;

                            drmo.EndEdit();

                        }

                    }
                }


                clsStaticInfo obj = new clsStaticInfo();


                if (FinalSettlementData.LvEncashmentDayNo > 0)
                {
                    obj.SaveDataSets(dsEmployeeFinalSettlement, dsleaveEncashment, dsFinalSettlementDeductionDetails, dsFinalSettlementRetainedDetails, dsSL);
                }
                else
                {
                    obj.SaveDataSets(dsEmployeeFinalSettlement, dsFinalSettlementDeductionDetails, dsFinalSettlementRetainedDetails, dsSL);
                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }

        public void GetLeaveBalance(string EmpSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType,s.LeaveTypeId,e.LegalDesignationId,    BI.BankSystemID,  BI.BankBranchId,  BI.BankAccNo
                            ,e.GivenDesignationId,e.PaymentMode,e.LegalDesignationId,e.BudgetCode
                            ,s.BroughtForward,s.CarryForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed,s.EncashedInbetween ,s.YearEndEncash
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,ISNULL( (SELECT sum(d.LeaveDuration) FROM [dbo].[LeaveTransaction] m 
                                        INNER JOIN  [dbo].[LeaveTransactionDetails] D ON d.LvTrnsSystemID=m.SystemID 
                                        where D.WorkDate BETWEEN S.FromDate and S.ToDate
                                AND m.EmpSystemID=S.EmployeeId AND m.LTSystemID=S.LeaveTypeId ),0) AS AvailedLeave

            
            				,Balance=ISNULL(s.CurrentYearAllocation,0)+ISNULL(s.BroughtForward,0)+ISNULL(s.CarryForwardOpeningBalance,0)
                            from trn.EmployeeLeaveSummary s 
                            INNER join LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            join EmployeeInformation e on e.SystemId=s.EmployeeId AND s.PlantId=e.PlantId
                            LEFT JOIN EmployeeBankInfo  AS BI ON BI.EmpSystemID=e.SystemId
                            
						   --------------------------------------------------------------------------
                            where  E.SystemId ='" + EmpSystemId + @"' AND e.DOS BETWEEN s.FromDate AND s.ToDate
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                            ";

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
        public void GetLeaveBalanceNew(string EmpSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType,s.LeaveTypeId,e.LegalDesignationId,    BI.BankSystemID,  BI.BankBranchId,  BI.BankAccNo
                            ,e.GivenDesignationId,e.PaymentMode,e.LegalDesignationId,e.BudgetCode
                            ,s.BroughtForward,s.CarryForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed,s.EncashedInbetween ,s.YearEndEncash
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                           ,ISNULL( (SELECT sum(m.LvValue) FROM AttdnProcessData m 
                                        where m.WorkDate BETWEEN S.FromDate and S.ToDate
                                AND m.EmpSystemID=S.EmployeeId AND m.LTSystemID=S.LeaveTypeId ),0) AS AvailedLeave

            
            				,Balance=ISNULL(s.CurrentYearAllocation,0)+ISNULL(s.BroughtForward,0)+ISNULL(s.CarryForwardOpeningBalance,0)
                            from trn.EmployeeLeaveSummary s 
                            INNER join LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            join EmployeeInformation e on e.SystemId=s.EmployeeId AND s.PlantId=e.PlantId
                            LEFT JOIN EmployeeBankInfo  AS BI ON BI.EmpSystemID=e.SystemId
                            
						   --------------------------------------------------------------------------
                            where  E.SystemId ='" + EmpSystemId + @"' AND e.DOS BETWEEN s.FromDate AND s.ToDate
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                            ";

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
                                                 LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId				
                                                      WHERE EI.SystemId IN (SELECT EmpSystemId FROM [dbo].[EmployeeFinalSettlement] WHERE id='" + Id + @"') AND 
                                                            EI.PlantId='" + identity.PlantId + @"' ORDER BY CONVERT(INT, ei.EmployeeCode) ";

            var FinalSettlement = _sqlRepository.GetDataCollection(FinalSettlementSql);
            var EmployeeInfo = _sqlRepository.GetDataCollection(EmployeeInfosql);

            return Json(new { FinalSettlement, EmployeeInfo }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmploymentTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT UserName  FROM EmploymentTypeEnum  ";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFNFMasterData()
        {
            string sql = @"SELECT M.Id,M.FinalSettlementName,FORMAT(M.FinalSettlementDate,'dd-MMM-yyyy')FinalSettlementDate,M.ApproveById,E.EmployeeName ApproveBy,ApproveStatus= CASE WHEN M.IsApproved=1 THEN 'Yes' ELSE 'No' END,M.IsApproved
FROM dbo.EmployeeFullAndFinalSettlementMaster M
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=M.ApproveById
Where M.IsApproved=0
Order By M.AddedDate DESC";
            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetApprovedData()
        {
            string sql = @"SELECT M.Id,M.FinalSettlementName,FORMAT(M.FinalSettlementDate,'dd-MMM-yyyy')FinalSettlementDate,M.ApproveById,E.EmployeeName ApproveBy,ApproveStatus= CASE WHEN M.IsApproved=1 THEN 'Yes' ELSE 'No' END,M.IsApproved
FROM dbo.EmployeeFullAndFinalSettlementMaster M
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=M.ApproveById
Where M.IsApproved=1
Order By M.AddedDate DESC";
            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetFNFMasterDataForApprove()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT M.Id,M.FinalSettlementName,FORMAT(M.FinalSettlementDate,'dd-MMM-yyyy')FinalSettlementDate,M.ApproveById,E.EmployeeName ApproveBy,ApproveStatus= CASE WHEN M.IsApproved=1 THEN 'Yes' ELSE 'No' END,M.IsApproved
FROM dbo.EmployeeFullAndFinalSettlementMaster M
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=M.ApproveById
Where ISNULL(M.IsApproved,0)=0 AND M.ApproveById='" + identity.EmployeeId + "'";
            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFNFApprovedMasterData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM 
                            (SELECT M.Id,M.FinalSettlementName,FORMAT(M.FinalSettlementDate,'dd-MMM-yyyy')FinalSettlementDate,M.ApproveById,E.EmployeeName ApproveBy,ApproveStatus= CASE WHEN M.IsApproved=1 THEN 'Yes' ELSE 'No' END,M.IsApproved
                            ,(SELECT COUNT(Id)CountEmployee FROM EmployeeFullAndFinalSettlement WHERE VoucherId IS NULL AND FinalSettlementId=M.Id)CountEmployee
                            FROM dbo.EmployeeFullAndFinalSettlementMaster M
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=M.ApproveById
                            Where ISNULL(M.IsApproved,0)=1)T 
                            WHERE T.CountEmployee>0 ";
            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeFNFDataByMaster(string masterId)
        {
            string sql = @"select E.*,EI.EmployeeCode,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(EI.DOS,'dd-MMM-yyyy')DOS,LD.UserName LegalDesignation,D.UserName Department, EDG.UserName DesignationGroup,EC.UserName EmployeeCategory
from EmployeeFullAndFinalSettlement  E
LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=E.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = EI.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
LEFT JOIN HKP.LegalDesignation LD ON LD.Id=EI.LegalDesignationId
LEFT JOIN ORG.Department D ON D.Id=PR.DepartmentId
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
LEFT JOIN HKP.DesignationGroup EDG ON  EDG.Id=DM.DesignationGroupId
where FinalSettlementId='" + masterId + "'";
            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeFNFMasterData(string masterId)
        {
            string sql = @"select isSelected = Convert(bit, 'True'),E.*,EI.EmployeeCode,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(EI.DOS,'dd-MMM-yyyy')DOS,LD.UserName LegalDesignation,D.UserName Department,ISNULL(EI.PaymentMode,'') PaymentMode
from EmployeeFullAndFinalSettlement  E
LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=E.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = EI.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
LEFT JOIN HKP.LegalDesignation LD ON LD.Id=EI.LegalDesignationId
LEFT JOIN ORG.Department D ON D.Id=PR.DepartmentId
where E.VoucherId IS NULL AND FinalSettlementId='" + masterId + "'";
            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteEmp(string empId)
        {
            DeleteEmployeeSepItemData(empId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteEmployeeSepItemData(string empId)
        {
            string strSQL, strSQLItem;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[EmployeeFullAndFinalSettlement] WHERE EmpSystemId = '" + empId + "'";
                strSQLItem = "DELETE FROM [dbo].[EmployeeFullAndFinalSettlementItem] WHERE EmpSystemId = '" + empId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQLItem, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT max(Sequence)+1 Sequence FROM hkp.[SeparationType] WHERE PlantID='" + identity.PlantId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetApprovedByCbo()
        {
            var sql = @"SELECT distinct E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text  
FROM dbo.AuthorizationConfig A 
INNER JOIN dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
WHERE E.EmployeeStatus='Active' AND A.ActionStatus='FullAndFinalApproveBy'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public void ReLoadFormulaWithValue(string strFormulaID, ref DataTable dtValue, out string lblFormulaValue)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;

            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView();
                        dvLocal.Table = dtValue;

                        dvLocal.RowFilter = "EmployeeSeperationItemId = '" + strTemp.Trim() + "'";
                        if (dvLocal.Count > 0)
                        {
                            strTemp = dvLocal[0]["Value"].ToString().Trim();
                        }
                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End 

        public DataTable GetDataTable(string empId)
        {
            try
            {
               
                string year = DateTime.Now.Year.ToString();

                string sql = @"SELECT A.Id,A.FinalSettlementId,E.SystemId EmpSystemId,OL.Id EmployeeSeperationItemId,OL.UserName,OL.Formula,OL.FormulaId
,Value= ISNULL(CASE WHEN OL.UserName='JoiningDate' THEN FORMAT(E.DOJ,'dd-MMM-yyyy')
			 WHEN OL.UserName='ConfirmationDate' THEN FORMAT(E.DOC,'dd-MMM-yyyy')
			 WHEN OL.UserName='ResignDate' THEN FORMAT(R.ResignationDate,'dd-MMM-yyyy')
			 WHEN OL.UserName='SeparationDate' THEN FORMAT(E.DOS,'dd-MMM-yyyy')
			 WHEN OL.UserName='EarnLeave' THEN CAST(CEILING(LV.Balance) AS varchar(100))
			 WHEN OL.SalaryHeadID<>'' THEN CAST(cast(SID.DefineAmount AS decimal(18,0)) AS varchar(100))
			 WHEN OL.UserName='NoticePeriod' THEN CAST(LV.NoticePeriod AS varchar(100))

WHEN OL.UserName='AdvanceSalary' THEN CAST((
			 cast((SELECT SUM(AD.Amount)-ISNULL((select SUM(Amount)WrittenOffAmount 
from TRN.EmployeeSubsequentTransaction where SourceType in('EmployeeAdvanceWriteOff','SalaryPayable') AND  EmployeeId=AD.EmployeeId AND JournalType in('Salary')),0) AS Balance
FROM TRN.EmployeeSubsequentTransaction AS AD
LEFT JOIN TRN.Voucher V ON V.Id=AD.VoucherId
WHERE    AD.EmployeeId<>''  AND AD.JournalType in('Salary') AND V.IsPark=0
AND AD.SourceType in ('EmployeeAdvance', 'InterTransaction') AND AD.EmployeeId='" + empId + @"'
GROUP BY AD.EmployeeId) AS decimal(18,0))) AS varchar(100))
			
WHEN OL.UserName='AdvanceLoan' THEN CAST((
			 cast((SELECT SUM(AD.Amount)-ISNULL(SUM(AWD.Amount),0) Balance
FROM TRN.Advance AS AD
LEFT JOIN (select AdvanceId,Sum(Amount) Amount from TRN.AdvanceWriteOffDetail group by AdvanceId) AWD ON AWD.AdvanceId=AD.Id
WHERE    AD.EmployeeId<>'' AND AD.IsPark=0 AND AD.IsWrittenOff=0
AND AD.SourceType in ('EmployeeAdvance') AND AD.EmployeeId='" + empId + @"' and AD.JournalType='General'
GROUP BY AD.EmployeeId) AS decimal(18,0))) AS varchar(100))
	
WHEN OL.UserName='ExpensesPayable' THEN CAST((
			 cast((SELECT ISNULL(SUM(AD.NetAmount)-SUM(AD.WrittenOffAmount),0) AS Balance
FROM trn.EmployeePayable AS AD
 WHERE AD.Archive=0 AND AD.IsPark=0 AND AD.IsWrittenOff=0 AND AD.IsWrittenOff=0
 AND AD.SourceType IN ('EmployeePayable')
 AND AD.EmployeeId='" + empId + @"' AND (AD.NetAmount-AD.WrittenOffAmount)>0) AS decimal(18,0))) AS varchar(100))
	

WHEN OL.UserName='UnPaidSalary' THEN CAST((
			 SELECT cast(SUM(spc.DisbusmentAmount)AS decimal(18,0))DisbusmentAmount FROM SalaryProcChild AS spc
LEFT JOIN SalaryProcMaster AS spm ON spm.SystemID = spc.SlrProcMstSystemID 
LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = spc.SalaryHeadID
LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId AND V.IsPark=0
WHERE  spc.EmpInfoSystemID= '" + empId + @"' AND PayableVoucherId<>'' and sl.islocked=1 AND sl.DisbursementVoucherId IS NULL  AND sh.SalaryHead='Net Pay' AND PastDisbursed IS NULL
			 ) AS varchar(100))

			 WHEN OL.UserName='Bonus' THEN CAST((
			 Select SUM(BonusAmount)BonusAmount from(
select BonusAmount=  cast(SUM(spc.DisbusmentAmount)AS decimal(18,0)) from SalaryProcChild SPC
left join dbo.SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID
Left join SalaryLock sl on sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=SPM.YearNo AND sl.MonthNo=SPM.MonthNo
LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId  AND V.IsPark=0
left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Annual Bonus' and vd.SalaryHeadId=SPC.SalaryHeadID and vd.CrAmount>0 AND VD.AccountsGroupId=SL.AccountsGroupId 
Where HeadCategory IN('Annual Bonus Retain') AND ISNULL(SPC.DisbusmentAmount,0)!=0
AND ISNULL(sl.PayableVoucherId,'')<>'' and sl.islocked=1 AND sl.BonusDisbursementVoucherId IS NULL AND ISNULL(sl.PastBonusDisbursed,0) = 0 AND BonusDisbursementAdviceId<>'' 
AND SPC.EmpInfoSystemID='" + empId + @"')A
			 ) AS varchar(100))
           
WHEN OL.Formula='SeparationDate - ResignDate' THEN CAST(DATEDIFF(Day,
			 (Select FORMAT(R.ResignationDate,'dd-MMM-yyyy') from [TRN].[Resignation] R Where R.EmployeeId='" + empId + @"'
AND R.Id=(SELECT TOP 1 Id FROM [TRN].[Resignation] MR WHERE MR.EmployeeId=R.EmployeeId ORDER BY MR.UpdatedDate DESC)),
(Select FORMAT(DOS,'dd-MMM-yyyy') from dbo.EmployeeInformation Where SystemId='" + empId + @"')
			 ) AS varchar(100))

WHEN OL.UserName='GoodWork' THEN CAST((
Select cast(((sum(gd.Minute)/60)*(B.Basic/104)) AS decimal(18,0)) from dbo.GoodWorkDetail GD
left join EmployeeInformation ei on ei.SystemId=GD.EmpSystemId
left join (Select top 1* from [dbo].[OTLimitSetting])OLS ON OLS.PlantID=ei.PlantId
LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = GD.EmpSystemId
LEFT JOIN(SELECT SID.SalaryID,SID.DefineAmount Gross,SH.SalaryHeadID GrossSalaryHeadID
FROM SalaryInfoDefine SID 
LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
left  join (SELECT SID.DefineAmount Basic,SH.SalaryHeadID BasicSalaryHeadID,SID.SalaryID
FROM SalaryInfoDefine SID 
LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
WHERE SH.HeadCategory='Basic') B ON B.SalaryID=SIDM.SystemID
Where GWPaymentAdviseId IN(select PaymentAdviseId from [dbo].[GoodWorkPaymentAdviseDetail] Where PaymentAdviseId in (select Id from [dbo].[GoodWorkPaymentAdvise] Where EmpSystemId ='" + empId + @"') and DisbursementVoucherId IS NULL) AND EmpSystemId='" + empId + @"' AND GD.Minute<>0
Group By OLS.OTreductionFactor,B.Basic
 ) AS varchar(100))

 WHEN OL.UserName='OverTime' THEN CAST((
Select CAST(((sum(gd.AdditionalOT)/60)*(B.Basic/104)) AS decimal(18,0)) from dbo.AttdnProcessData GD
left join EmployeeInformation ei on ei.SystemId=GD.EmpSystemId
left join (Select top 1* from [dbo].[OTLimitSetting])OLS ON OLS.PlantID=ei.PlantId
LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = GD.EmpSystemId
LEFT JOIN(SELECT SID.SalaryID,SID.DefineAmount Gross,SH.SalaryHeadID GrossSalaryHeadID
FROM SalaryInfoDefine SID 
LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
WHERE SH.HeadCategory='Gross')g ON g.SalaryID=SIDM.SystemID
left  join (SELECT SID.DefineAmount Basic,SH.SalaryHeadID BasicSalaryHeadID,SID.SalaryID
FROM SalaryInfoDefine SID 
LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SID.SalaryHeadID
WHERE SH.HeadCategory='Basic') B ON B.SalaryID=SIDM.SystemID
Where GD.GWPaymentAdviseId  IN(select PaymentAdviseId from [dbo].[GoodWorkPaymentAdviseDetail] Where PaymentAdviseId in (select Id from [dbo].[GoodWorkPaymentAdvise] Where EmpSystemId ='" + empId + @"') and DisbursementVoucherId IS NULL) AND GD.EmpSystemId='" + empId + @"' 
AND GD.EmpSystemID NOT IN(Select EmployeeId from dbo.ExceptionGoodWorkEmployee) AND GD.AdditionalOT<>0 AND ISNULL(PastOTDisbursed,0)=0
Group By OLS.OTreductionFactor,B.Basic
 ) AS varchar(100))

ElSE CAST(A.Value as varchar(100)) END,0)
,OL.EntryState
FROM EmployeeSeperationItem AS OL
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId='" + empId + @"'
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationId
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
LEFT JOIN [TRN].[Resignation] R ON R.EmployeeId=E.SystemId
AND R.Id=(SELECT TOP 1 Id FROM [TRN].[Resignation] MR WHERE MR.EmployeeId=R.EmployeeId ORDER BY MR.UpdatedDate DESC)
LEFT JOIN(
Select Balance=ISNULL((CONVERT(NUMERIC(10,0),CONVERT(NUMERIC(10,2),ISNULL(P.PayDays,0))/CONVERT(NUMERIC(10,2),dp.EncashWorkingDaysQty))+ISNULL(S.BroughtForward,0)-ISNULL(B.Availed,0)),0),C.NoticePeriod,E.SystemId
FROM EmployeeInformation E
 LEFT JOIN (SELECT SUM(ISNULL(TotalPresent,0) + ISNULL(TotalLate,0))PayDays, EmpSystemID
 FROM [dbo].[SalaryProceAttdnData] WHERE EmpSystemID='" + empId + @"' and YearNo='"+ year + @"' Group By EmpSystemID
) P ON P.EmpSystemID=E.SystemId
left join mst.DesignationMasterLegalDesignation d on d.LegalDesignationId=e.LegalDesignationId
left join SCS.DesignationMasterConfiguration c on c.DesignationMasterId=d.DesignationMasterId and c.PlantId=e.PlantId
left join LeavePolicyDetail dp on dp.LPMSystemID=c.LeavePolicyMasterId
LEFT JOIN(
Select COUNT(a.EmpSystemID)Availed,a.EmpSystemID from AttdnProcessData a  
LEFT JOIN LeaveType t on t.Id=a.LTSystemID 
where a.WorkDate between '01-JAN-" + year + @"' AND  '31-DEC-" + year + @"' AND EmpSystemID='" + empId + @"' AND t.LeaveType='Earn'
Group By a.EmpSystemID
) B ON E.SystemID=B.EmpSystemID
LEFT JOIN(
select top(1) BroughtForward=CASE WHEN A.Closing>A.CarryForward THEN A.Closing ELSE A.CarryForward END,A.EmployeeId,0 EncashedInbetween from dbo.AnnualLeaveDataPast A
left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
Where EmployeeId='" + empId + @"' AND (A.Closing<>0 OR A.CarryForward<>0) order by A.AddedDate desc
) S ON E.SystemID=S.EmployeeId
where e.SystemID='" + empId + @"' AND dp.EncashWorkingDaysQty<>0
) LV ON LV.SystemID=E.SystemId
 LEFT JOIN SalaryInfoDefineMaster SIDM ON SIDM.EmpInfoSystemID = E.SystemId
 LEFT JOIN SalaryInfoDefine SID ON SID.SalaryID=SIDM.SystemID AND OL.SalaryHeadID = SID.SalaryHeadID
OUTER APPLY (SELECT * FROM dbo.EmployeeFullAndFinalSettlementItem WHERE EmployeeSeperationItemId=OL.Id 
AND ISNULL(EmpSystemId,'" + empId + @"')='" + empId + @"') A
Where OL.EmployeeSeperationSetupId=
(select EmployeeSeperationSetupId from [dbo].[EmpSeperationDesignationGroup] where DesignationGroupId=
(select DM.DesignationGroupId from [dbo].EmployeeInformation EI
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId Where EI.SystemId='" + empId + @"'))
ORDER BY OL.Sequence";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult ApproveFNF(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster = null;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from EmployeeFullAndFinalSettlementMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data master
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    data["IsApproved"] = true;
                    data["ApproveDateTime"] = DateTime.Now;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
                #endregion data update
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteCurrentData(string empIds)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[EmployeeFullAndFinalSettlementItem] WHERE EmpSystemId IN(" + empIds + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost]
        public JsonResult Process(Dictionary<string, object> data, List<Dictionary<string, object>> datalist)
        {
            try
            {

                DataSet dsMaster, dsID, dsEmpID = null;
                DataSet dsEmpMaster = null;
                DataSet dsEmpSL = null;
                DataSet dsEmpGW = null;
                DataSet dsEmpAT = null;
                DataSet dsEmpBN = null;
                DataSet dsSalaryData = null;
                DataSet dsProcSalaryData = null;
                DataSet dsFNFEmpMaster = null;
                string esql, elocksql, elockBNsql = "";
                var empIds = "' '";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (identity.EmployeeId == data["ApproveById"].ToString())
                {
                    throw new Exception("Creation and Approving person can't be same.");
                }
                clsFinalSettlement clsFS = new clsFinalSettlement();
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from EmployeeFullAndFinalSettlementMaster where FinalSettlementName='" + data["FinalSettlementName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");

                con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[EmployeeFullAndFinalSettlementItem] where FinalSettlementId='" + data["Id"] + "'", out dsID, false, "1");
                int ccount = Convert.ToInt32(dsID.Tables[0].Rows[0]["countId"].ToString());
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Final Settlement Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from EmployeeFullAndFinalSettlementMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data master
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("EmployeeFullAndFinalSettlementMaster", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update
                #region data Detail
                con.OpenDataSetThroughAdapter("select * from EmployeeFullAndFinalSettlement where FinalSettlementId='" + data["Id"] + "'", out dsFNFEmpMaster, false, "1");
                con.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[EmployeeFullAndFinalSettlementItem] where FinalSettlementId='" + data["Id"] + "'", out dsEmpID, false, "1");
                int empcount = Convert.ToInt32(dsEmpID.Tables[0].Rows[0]["countId"].ToString());
                
                foreach (var item in datalist)
                {
                    empIds += ",'" + item["EmpSystemId"].ToString() + "' ";

                    clsFS.GetSalaryDataEmpWise(item["EmpSystemId"].ToString(), Convert.ToDateTime(item["DOS"]).ToString("dd-MMM-yyyy"), out dsSalaryData);
                    if (dsSalaryData.Tables[0].Rows.Count == 0)
                    {
                        throw new Exception("This Employee " + item["EmpSystemId"].ToString() + " has no Approved Salary Structure.");
                    }


                    clsFS.GetLastMonthSalaryInfoByEmpId(item["EmpSystemId"].ToString(), Convert.ToDateTime(item["DOS"]).ToString("dd-MMM-yyyy"), out dsProcSalaryData);

                    if (dsProcSalaryData.Tables[0].Rows.Count > 0)
                    {
                        DataView dvSPAprovedData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPAprovedData.RowFilter = "IsLocked=" + true;
                        if (dvSPAprovedData.Count == 0)
                        {
                            throw new Exception("Salary of [" + Convert.ToDateTime(item["DOS"]).ToString("MMMM") + "] is not Locked for " + item["EmpSystemId"].ToString() + ".");
                        }
                    }
                    else
                    {
                        throw new Exception("Salary [ of " + Convert.ToDateTime(item["DOS"]).ToString("MMMM") + "] is not processed for " + item["EmpSystemId"].ToString() + ". ");
                    }

                }

                esql = "select * from EmployeeFullAndFinalSettlementItem where EmpSystemId IN(" + empIds + ")";
                con.OpenDataSetThroughAdapter(esql, out dsEmpMaster, false, "1");

                elocksql = @"Select * from  dbo.SalaryLock where EmpSystemId IN(" + empIds + ") AND PayableVoucherId<>'' AND PastDisbursed  IS NULL AND DisbursementVoucherId IS NULL";
                con.OpenDataSetThroughAdapter(elocksql, out dsEmpSL, false, "1");


                for (int i = 0; i < dsEmpSL.Tables[0].Rows.Count; i++)
                {
                    DataView empsldv = new DataView(dsEmpSL.Tables[0]);
                    empsldv.RowFilter = "EmpSystemId='" + dsEmpSL.Tables[0].Rows[i]["EmpSystemId"] + "' AND Id='" + dsEmpSL.Tables[0].Rows[i]["Id"] + "'";

                    if (empsldv.Count > 0)
                    {
                        DataRow drsl = empsldv[0].Row;

                        drsl.BeginEdit();
                        drsl["EmployeeFinalSettlementId"] = _Id;
                        drsl["UpdatedBy"] = identity.Name;
                        drsl["UpdatedDate"] = DateTime.Now.ToString();
                        drsl["UpdatedFromIP"] = identity.IPAddress;
                        drsl.EndEdit();

                    }
                }
               

                foreach (var item in datalist)
                {
                    string empId = item["EmpSystemId"].ToString();

                    string sql = string.Empty;
                    DataTable dtValue = new DataTable();
                    dtValue.TableName = "TempTable";
                    dtValue.Columns.Add("EmployeeSeperationItemId");
                    dtValue.Columns.Add("Value");
                    string sFormulaResult = null;

                    DataView empdv = new DataView(dsFNFEmpMaster.Tables[0]);
                    empdv.RowFilter = "EmpSystemId='" + item["EmpSystemId"] + "'";

                    if (empdv.Count == 0)
                    {
                        empcount++;
                        item["Id"] = materialCommonService.MakePK(_Id, empcount, 2);
                        item["FinalSettlementId"] = _Id;

                        AddNewRow(dsFNFEmpMaster.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = empdv[0].Row;
                        EditRow(drmo, item);
                    }

                    DataTable dtData = GetDataTable(empId);
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            DataRow dtValueRow = dtValue.NewRow();

                            dtValueRow["EmployeeSeperationItemId"] = dtData.Rows[i]["EmployeeSeperationItemId"].ToString().Trim();
                            dtValueRow["Value"] = dtData.Rows[i]["Value"].ToString().Trim();

                            dtValue.Rows.Add(dtValueRow);
                        }
                        else if (i > 0 && string.IsNullOrEmpty(dtData.Rows[i]["FormulaId"].ToString()))
                        {
                            DataRow dtValueRow = dtValue.NewRow();

                            dtValueRow["EmployeeSeperationItemId"] = dtData.Rows[i]["EmployeeSeperationItemId"].ToString().Trim();
                            dtValueRow["Value"] = dtData.Rows[i]["Value"].ToString().Trim();

                            dtValue.Rows.Add(dtValueRow);
                        }
                        else if (dtData.Rows[i]["Formula"].ToString() == "SeparationDate - ResignDate")
                        {
                            DataRow dtValueRow = dtValue.NewRow();

                            dtValueRow["EmployeeSeperationItemId"] = dtData.Rows[i]["EmployeeSeperationItemId"].ToString().Trim();
                            dtValueRow["Value"] = dtData.Rows[i]["Value"].ToString().Trim();

                            dtValue.Rows.Add(dtValueRow);
                        }
                        if (!string.IsNullOrEmpty(dtData.Rows[i]["FormulaId"].ToString()) && dtData.Rows[i]["Formula"].ToString() != "SeparationDate - ResignDate")
                        {
                            ReLoadFormulaWithValue(dtData.Rows[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                            sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("###0");
                            if (dtData.Rows[i]["Formula"].ToString() == "NoticePeriod - ServedNoticePeriod")
                            {
                                if (Convert.ToInt32(sFormulaResult.ToString()) < 0)
                                {
                                    sFormulaResult = "0";
                                }
                            }
                            DataRow dtValueRow = dtValue.NewRow();

                            dtValueRow["EmployeeSeperationItemId"] = dtData.Rows[i]["EmployeeSeperationItemId"].ToString().Trim();
                            dtValueRow["Value"] = sFormulaResult;

                            dtValue.Rows.Add(dtValueRow);

                            DataView dtv = new DataView(dtData);
                            dtv.RowFilter = "EmployeeSeperationItemId='" + dtData.Rows[i]["EmployeeSeperationItemId"].ToString() + "'";
                            if (dtv.Count > 0)
                            {
                                DataRow drmo = dtv[0].Row;

                                drmo.BeginEdit();
                                drmo["Value"] = sFormulaResult;
                                drmo.EndEdit();

                            }
                        }



                    }

                    List<Dictionary<string, object>> itemdata = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtData);

                    foreach (var itm in itemdata)
                    {
                        DataView dv = new DataView(dsEmpMaster.Tables[0]);
                        dv.RowFilter = "Id='" + itm["Id"] + "' AND EmployeeSeperationItemId = '" + itm["EmployeeSeperationItemId"] + "' AND EmpSystemId = '" + itm["EmpSystemId"] + "'";

                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;

                            drmo.BeginEdit();

                            drmo["UserName"] = itm["UserName"];
                            drmo["Value"] = itm["Value"];
                            drmo["UpdatedBy"] = identity.Name;
                            drmo["UpdatedDate"] = DateTime.Now.ToString();
                            drmo["UpdatedFromIP"] = identity.IPAddress;

                            drmo.EndEdit();

                        }
                        else
                        {
                            ccount++;
                            itm["Id"] = materialCommonService.MakePK(_Id, ccount, 2);
                            itm["FinalSettlementId"] = _Id;
                            AddNewRow(dsEmpMaster.Tables[0], itm);
                        }
                    }


                }

               string gwsql = @"Select * from dbo.GoodWorkDetail Where GWPaymentAdviseId IS NULL AND EmpSystemId IN(" + empIds + ")  AND Minute<>0";
                con.OpenDataSetThroughAdapter(gwsql, out dsEmpGW, false, "1");


                for (int i = 0; i < dsEmpGW.Tables[0].Rows.Count; i++)
                {
                    DataView empgwdv = new DataView(dsEmpGW.Tables[0]);
                    empgwdv.RowFilter = "EmpSystemId='" + dsEmpGW.Tables[0].Rows[i]["EmpSystemId"] + "' AND Id='" + dsEmpGW.Tables[0].Rows[i]["Id"] + "'";

                    if (empgwdv.Count > 0)
                    {
                        DataRow drgw = empgwdv[0].Row;

                        drgw.BeginEdit();
                        drgw["EmployeeFinalSettlementId"] = _Id;
                        drgw["UpdatedBy"] = identity.Name;
                        drgw["UpdatedDate"] = DateTime.Now.ToString();
                        drgw["UpdatedFromIP"] = identity.IPAddress;
                        drgw.EndEdit();

                    }
                }

                string atsql = @"Select * from dbo.AttdnProcessData Where GWPaymentAdviseId IN(select PaymentAdviseId from [dbo].[GoodWorkPaymentAdviseDetail] Where PaymentAdviseId in (select Id from [dbo].[GoodWorkPaymentAdvise] Where EmpSystemId IN(" + empIds + ")) and DisbursementVoucherId IS NULL) AND EmpSystemId IN(" + empIds + ") AND EmpSystemID NOT IN(Select EmployeeId from dbo.ExceptionGoodWorkEmployee) AND AdditionalOT<>0 AND ISNULL(PastOTDisbursed,0)=0";
                con.OpenDataSetThroughAdapter(atsql, out dsEmpAT, false, "1");


                for (int i = 0; i < dsEmpAT.Tables[0].Rows.Count; i++)
                {
                    DataView empatdv = new DataView(dsEmpAT.Tables[0]);
                    empatdv.RowFilter = "EmpSystemId='" + dsEmpAT.Tables[0].Rows[i]["EmpSystemId"] + "' AND RowId='" + dsEmpAT.Tables[0].Rows[i]["RowId"] + "'";

                    if (empatdv.Count > 0)
                    {
                        DataRow drat = empatdv[0].Row;

                        drat.BeginEdit();
                        drat["EmployeeFinalSettlementId"] = _Id;
                        //drat["PastOTDisbursed"] = true;
                        drat["UpdatedBy"] = identity.Name;
                        drat["DateUpdated"] = DateTime.Now.ToString();
                        drat.EndEdit();

                    }
                }

                #endregion data update 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsEmpMaster, dsFNFEmpMaster, dsEmpSL, dsEmpGW, dsEmpAT);

                elockBNsql = @"SELECT * FROM SalaryLock 
Where EmpSystemId IN(
(Select  spc.EmpInfoSystemID  from SalaryProcChild AS spc
LEFT JOIN SalaryProcMaster AS spm ON spm.SystemID = spc.SlrProcMstSystemID 
LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = spc.SalaryHeadID
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId IN (" + empIds + @")
LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationId
LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
Left join SalaryLock sl on sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=SPM.YearNo AND sl.MonthNo=SPM.MonthNo
LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
left join trn.VoucherDetail vd on vd.VoucherId=v.Id and vd.TrnNature ='Annual Bonus' and vd.SalaryHeadId=SPC.SalaryHeadID and vd.CrAmount>0 AND VD.AccountsGroupId=SL.AccountsGroupId 
WHERE  spc.EmpInfoSystemID IN (" + empIds + @") AND sh.HeadCategory IN('Annual Bonus Retain') AND ISNULL(SPC.DisbusmentAmount,0)!=0))
AND PayableVoucherId<>'' AND BonusDisbursementVoucherId IS NULL AND ISNULL(PastBonusDisbursed,0)=0 AND BonusDisbursementAdviceId<>''";
                con.OpenDataSetThroughAdapter(elockBNsql, out dsEmpBN, false, "1");


                for (int i = 0; i < dsEmpBN.Tables[0].Rows.Count; i++)
                {
                    DataView empsldv = new DataView(dsEmpBN.Tables[0]);
                    empsldv.RowFilter = "EmpSystemId='" + dsEmpBN.Tables[0].Rows[i]["EmpSystemId"] + "' AND Id='" + dsEmpBN.Tables[0].Rows[i]["Id"] + "'";

                    if (empsldv.Count > 0)
                    {
                        DataRow drsl = empsldv[0].Row;

                        drsl.BeginEdit();
                        drsl["EmployeeFinalSettlementId"] = _Id;
                        drsl["UpdatedBy"] = identity.Name;
                        drsl["UpdatedDate"] = DateTime.Now.ToString();
                        drsl["UpdatedFromIP"] = identity.IPAddress;
                        drsl.EndEdit();

                    }
                }
                _info.SaveDataSets(dsEmpBN);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult UpdateItemData(IEnumerable<OpenHeadModelNew> datalist)
        {
            DataSet dsEmpMaster = null;
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("EmployeeSeperationItemId");
            dtValue.Columns.Add("Value");
            string sFormulaResult = null;
            string esql = null;
            string empIds = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                foreach (var item in datalist)
                {
                    empIds = item.EmpSystemId;
                    break;
                }

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                esql = "select * from EmployeeFullAndFinalSettlementItem where EmpSystemId IN(" + empIds + ")";
                con.OpenDataSetThroughAdapter(esql, out dsEmpMaster, false, "1");
                DataSet dtData = Library.Service.Helpers.DataTableExtensions.ToDataSet<OpenHeadModelNew>(datalist);

                for (int i = 0; i < dtData.Tables[0].Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        DataRow dtValueRow = dtValue.NewRow();

                        dtValueRow["EmployeeSeperationItemId"] = dtData.Tables[0].Rows[i]["EmployeeSeperationItemId"].ToString().Trim();
                        dtValueRow["Value"] = dtData.Tables[0].Rows[i]["Value"].ToString().Trim();

                        dtValue.Rows.Add(dtValueRow);
                    }
                    else if (i > 0 && string.IsNullOrEmpty(dtData.Tables[0].Rows[i]["FormulaId"].ToString()))
                    {
                        DataRow dtValueRow = dtValue.NewRow();

                        dtValueRow["EmployeeSeperationItemId"] = dtData.Tables[0].Rows[i]["EmployeeSeperationItemId"].ToString().Trim();
                        dtValueRow["Value"] = dtData.Tables[0].Rows[i]["Value"].ToString().Trim();

                        dtValue.Rows.Add(dtValueRow);
                    }
                    else if (dtData.Tables[0].Rows[i]["Formula"].ToString() == "SeparationDate - ResignDate")
                    {
                        DataRow dtValueRow = dtValue.NewRow();

                        dtValueRow["EmployeeSeperationItemId"] = dtData.Tables[0].Rows[i]["EmployeeSeperationItemId"].ToString().Trim();
                        dtValueRow["Value"] = dtData.Tables[0].Rows[i]["Value"].ToString().Trim();

                        dtValue.Rows.Add(dtValueRow);
                    }
                    if (!string.IsNullOrEmpty(dtData.Tables[0].Rows[i]["FormulaId"].ToString()) && dtData.Tables[0].Rows[i]["Formula"].ToString() != "SeparationDate - ResignDate")
                    {
                        ReLoadFormulaWithValue(dtData.Tables[0].Rows[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                        sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("###0");

                        DataRow dtValueRow = dtValue.NewRow();

                        dtValueRow["EmployeeSeperationItemId"] = dtData.Tables[0].Rows[i]["EmployeeSeperationItemId"].ToString().Trim();
                        dtValueRow["Value"] = sFormulaResult;

                        dtValue.Rows.Add(dtValueRow);

                        DataView dtv = new DataView(dtData.Tables[0]);
                        dtv.RowFilter = "EmployeeSeperationItemId='" + dtData.Tables[0].Rows[i]["EmployeeSeperationItemId"].ToString() + "'";
                        if (dtv.Count > 0)
                        {
                            DataRow drmo = dtv[0].Row;

                            drmo.BeginEdit();
                            drmo["Value"] = sFormulaResult;
                            drmo.EndEdit();

                        }
                    }
                }

                List<Dictionary<string, object>> data = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtData.Tables[0]);

                foreach (var item in data)
                {
                    DataView dv = new DataView(dsEmpMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "' AND EmployeeSeperationItemId = '" + item["EmployeeSeperationItemId"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["Value"] = item["Value"];
                        drmo["Remarks"] = item["Remarks"];
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpMaster);

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [Authorize, HttpPost]
        public JsonResult GetFinalSettlementDisbursementJVDataList(string disbursementAdviceId, VoucherViewModel voucherVM, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string empSystemIds = "";
            if (goodWorkPaymentAdviseDetail != null)
            {
                foreach (var item in goodWorkPaymentAdviseDetail)
                {
                    if (empSystemIds == "")
                    {
                        empSystemIds = "'" + item["EmpSystemId"] + "'"; ;
                    }
                    else
                    {
                        empSystemIds += ",'" + item["EmpSystemId"] + "'";

                    }
                }
            }


            string sql = null;
            sql = @"SELECT x.OtherName,X.TrnType,X.GLName,X.BudgetName,X.ActivityName, SUM(X.DrAmount) DrAmount,SUM(X.CrAmount) CrAmount,SUM(X.Amount) Amount,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId,X.Active
                FROM
                ( SELECT  'NetPay' AS OtherName, 'Dr' AS TrnType
                , CAST(EI.Value AS decimal(18,2)) DrAmount 
                , 0 CrAmount 
                , CAST(EI.Value AS decimal(18,2)) Amount
                ,vd.GLGeneralInfoId  ,vd.BudgetMasterId,vd.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='NetPay'
				LEFT JOIN [dbo].[EmployeeSeperationItem] ESI ON  ESI.Id=EI.EmployeeSeperationItemId
				LEFT JOIN ( SELECT vd.GLGeneralInfoId, vd.BudgetMasterId,vd.ActivityId,sl.EmployeeFinalSettlementId FROM [dbo].[SalaryLock] sl   
				left join trn.VoucherDetail vd on vd.VoucherId=sl.PayableVoucherId and vd.TrnNature ='Net Pay' and Vd.AccountsGroupId=sl.AccountsGroupId WHERE sl.EmployeeFinalSettlementId IS NOT NULL AND vd.ActivityId IS NOT NULL GROUP BY vd.GLGeneralInfoId, vd.BudgetMasterId,vd.ActivityId,sl.EmployeeFinalSettlementId) AS vd ON vd.EmployeeFinalSettlementId=E.FinalSettlementId
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON vd.GLGeneralInfoId=GL.Id
				LEFT JOIN[MST].[BudgetMaster] AS BM ON vd.BudgetMasterId= BM.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON vd.ActivityId= A.Id
                LEFT JOIN[MST].[BudgetMasterActivity] AS BMA ON BMA.BudgetMasterId= BM.Id AND BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND E.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")

                Union All
				SELECT TOP 1 A.* FROM (SELECT  'Bonus' AS OtherName, 'Dr' AS TrnType
                , CAST(EI.Value AS decimal(18,2)) DrAmount 
                , 0 CrAmount 
                , CAST(EI.Value AS decimal(18,2)) Amount
                ,vd.GLGeneralInfoId  ,vd.BudgetMasterId,vd.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='Bonus'
				LEFT JOIN [dbo].[EmployeeSeperationItem] ESI ON  ESI.Id=EI.EmployeeSeperationItemId
				LEFT JOIN ( SELECT vd.GLGeneralInfoId, vd.BudgetMasterId,vd.ActivityId,sl.EmployeeFinalSettlementId FROM [dbo].[SalaryLock] sl   
				left join trn.VoucherDetail vd on vd.VoucherId=sl.PayableVoucherId and vd.TrnNature ='Annual Bonus' and Vd.AccountsGroupId=sl.AccountsGroupId WHERE sl.EmployeeFinalSettlementId IS NOT NULL AND vd.ActivityId IS NOT NULL AND VD.CrAmount>0 GROUP BY vd.GLGeneralInfoId, vd.BudgetMasterId,vd.ActivityId,sl.EmployeeFinalSettlementId) AS vd ON vd.EmployeeFinalSettlementId=E.FinalSettlementId
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON vd.GLGeneralInfoId=GL.Id
				LEFT JOIN[MST].[BudgetMaster] AS BM ON vd.BudgetMasterId= BM.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON vd.ActivityId= A.Id
                LEFT JOIN[MST].[BudgetMasterActivity] AS BMA ON BMA.BudgetMasterId= BM.Id AND BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND E.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @") )A

                Union All
				SELECT  'LeaveEncashment' AS OtherName, 'Dr' AS TrnType
                , CAST(EI.Value AS decimal(18,2)) DrAmount 
                , 0 CrAmount 
                , CAST(EI.Value AS decimal(18,2)) Amount
                ,BM.GLGeneralInfoId ,BMA.BudgetMasterId,BMA.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='LeaveEncashment'
				LEFT JOIN [dbo].[EmployeeSeperationItem] ESI ON  ESI.Id=EI.EmployeeSeperationItemId
				LEFT JOIN [MST].[BudgetMasterActivity] BMA ON  BMA.Id=ESI.DrBudgetMasterActivityId
				LEFT JOIN[MST].[BudgetMaster] AS BM ON BMA.BudgetMasterId= BM.Id
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND EI.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")
                
                Union All
				SELECT  'ExpensesPayable' AS OtherName, 'Dr' AS TrnType
                , CAST(EI.Value AS decimal(18,2)) DrAmount 
                , 0 CrAmount 
                , CAST(EI.Value AS decimal(18,2)) Amount
                ,BM.GLGeneralInfoId ,EP.BudgetMasterId,EP.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='ExpensesPayable'
				LEFT JOIN (SELECT EP.EmployeeId,EPD.GLGeneralInfoId, EPD.BudgetMasterId,EPD.ActivityId FROM trn.EmployeePayable AS EP 
									LEFT JOIN trn.EmployeePayableDetail AS EPD ON EPD.EmployeePayableId=EP.Id
									WHERE EP.EmployeeId<>'' and EPD.IsWrittenOff=0 AND EP.SourceType in ('EmployeePayable')
									GROUP BY EP.EmployeeId,EPD.GLGeneralInfoId, EPD.BudgetMasterId,EPD.ActivityId) AS EP ON EP.EmployeeId=E.EmpSystemId
				LEFT JOIN[MST].[BudgetMaster] AS BM ON EP.BudgetMasterId= BM.Id
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON EP.ActivityId= A.Id
                LEFT JOIN[MST].[BudgetMasterActivity] AS BMA ON BMA.BudgetMasterId= BM.Id AND BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND EI.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")

                Union All
				SELECT  'GoodWork' AS OtherName, 'Dr' AS TrnType
                , ABS(CAST(EI.Value AS decimal(18,2)))  DrAmount 
                , 0  CrAmount 
                , ABS(CAST(EI.Value AS decimal(18,2))) Amount
                ,BM.GLGeneralInfoId  ,BMA.BudgetMasterId,BMA.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='GoodWork'
				LEFT JOIN [dbo].[EmployeeSeperationItem] ESI ON  ESI.Id=EI.EmployeeSeperationItemId
				LEFT JOIN [MST].[BudgetMasterActivity] BMA ON  BMA.Id=ESI.CrBudgetMasterActivityId
				LEFT JOIN[MST].[BudgetMaster] AS BM ON BMA.BudgetMasterId= BM.Id
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND E.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")

				Union All
				SELECT  'OverTime' AS OtherName, 'Dr' AS TrnType
                , ABS(CAST(EI.Value AS decimal(18,2)))  DrAmount 
                , 0  CrAmount 
                , ABS(CAST(EI.Value AS decimal(18,2))) Amount
                ,BM.GLGeneralInfoId  ,BMA.BudgetMasterId,BMA.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='OverTime'
				LEFT JOIN [dbo].[EmployeeSeperationItem] ESI ON  ESI.Id=EI.EmployeeSeperationItemId
				LEFT JOIN [MST].[BudgetMasterActivity] BMA ON  BMA.Id=ESI.CrBudgetMasterActivityId
				LEFT JOIN[MST].[BudgetMaster] AS BM ON BMA.BudgetMasterId= BM.Id
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND E.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")
				Union All
				SELECT  'AdvanceLoan' AS OtherName, 'Cr' AS TrnType
                , 0 DrAmount 
                , CAST(EP.Amount AS decimal(18,2)) CrAmount 
                , CAST(EP.Amount AS decimal(18,2)) Amount
                ,BM.GLGeneralInfoId ,EP.BudgetMasterId,EP.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='AdvanceLoan'
				LEFT JOIN (SELECT EP.EmployeeId,EPD.GLGeneralInfoId, EPD.BudgetMasterId,EPD.ActivityId,(SUM(EP.Amount)-SUM(EP.WrittenOffAmount))Amount FROM trn.Advance AS EP 
									LEFT JOIN trn.AdvanceDetail AS EPD ON EPD.AdvanceId=EP.Id
									WHERE EP.EmployeeId<>'' and EPD.IsWrittenOff=0 AND EP.SourceType in ('EmployeeAdvance') and EP.JournalType='General'
									GROUP BY EP.EmployeeId,EPD.GLGeneralInfoId, EPD.BudgetMasterId,EPD.ActivityId) AS EP ON EP.EmployeeId=E.EmpSystemId
				LEFT JOIN[MST].[BudgetMaster] AS BM ON EP.BudgetMasterId= BM.Id
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON EP.ActivityId= A.Id
                LEFT JOIN[MST].[BudgetMasterActivity] AS BMA ON BMA.BudgetMasterId= BM.Id AND BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND EI.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")

				Union All
				SELECT  'AdvanceSalary' AS OtherName, 'Cr' AS TrnType
                , 0 DrAmount 
                , CAST(EI.Value AS decimal(18,2)) CrAmount 
                , CAST(EI.Value AS decimal(18,2)) Amount
                ,BM.GLGeneralInfoId ,EP.BudgetMasterId,EP.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='AdvanceSalary'
				LEFT JOIN (SELECT EP.EmployeeId,EPD.GLGeneralInfoId, EPD.BudgetMasterId,EPD.ActivityId FROM trn.EmployeeSubsequentTransaction AS EP 
									LEFT JOIN trn.VoucherDetail AS EPD ON EPD.Id=EP.VoucherDetailId
									WHERE EP.EmployeeId<>''  AND EP.SourceType  in ('EmployeeAdvance', 'InterTransaction') and EP.JournalType='Salary'
									GROUP BY EP.EmployeeId,EPD.GLGeneralInfoId, EPD.BudgetMasterId,EPD.ActivityId) AS EP ON EP.EmployeeId=E.EmpSystemId
				LEFT JOIN[MST].[BudgetMaster] AS BM ON EP.BudgetMasterId= BM.Id
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON EP.ActivityId= A.Id
                LEFT JOIN[MST].[BudgetMasterActivity] AS BMA ON BMA.BudgetMasterId= BM.Id AND BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND EI.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")

                Union All
				SELECT  'NetPay' AS OtherName, 'Cr' AS TrnType
                , 0 DrAmount 
                , ABS(CAST(EI.Value AS decimal(18,2)))  CrAmount 
                , ABS(CAST(EI.Value AS decimal(18,2))) Amount
                ,BM.GLGeneralInfoId  ,BMA.BudgetMasterId,BMA.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='NetPay'
				LEFT JOIN [dbo].[EmployeeSeperationItem] ESI ON  ESI.Id=EI.EmployeeSeperationItemId
				LEFT JOIN [MST].[BudgetMasterActivity] BMA ON  BMA.Id=ESI.CrBudgetMasterActivityId
				LEFT JOIN[MST].[BudgetMaster] AS BM ON BMA.BudgetMasterId= BM.Id
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))<0 AND E.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")

                Union All
				SELECT  'ShortNoticePeriodDeduction' AS OtherName, 'Cr' AS TrnType
                , 0 DrAmount 
                , ABS(CAST(EI.Value AS decimal(18,2)))  CrAmount 
                , ABS(CAST(EI.Value AS decimal(18,2))) Amount
                ,BM.GLGeneralInfoId  ,BMA.BudgetMasterId,BMA.ActivityId, GL.AccountCode + ' - ' + GL.UserName GLName
                , B.UserName BudgetName,A.UserName ActivityName ,BMA.Active
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='ShortNoticePeriodDeduction'
				LEFT JOIN [dbo].[EmployeeSeperationItem] ESI ON  ESI.Id=EI.EmployeeSeperationItemId
				LEFT JOIN [MST].[BudgetMasterActivity] BMA ON  BMA.Id=ESI.CrBudgetMasterActivityId
				LEFT JOIN[MST].[BudgetMaster] AS BM ON BMA.BudgetMasterId= BM.Id
				LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
				LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
				LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND E.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")

                Union All
				SELECT  'Bank/Cash' AS OtherName, 'Cr' AS TrnType
                , 0 DrAmount 
                , ABS(CAST(EI.Value AS decimal(18,2)))  CrAmount 
                , ABS(CAST(EI.Value AS decimal(18,2))) Amount
                ,'" + voucherVM.GLGeneralInfoId + @"' GLGeneralInfoId ,'" + voucherVM.BudgetMasterId + @"' BudgetMasterId,'" + voucherVM.ActivityId + @"' ActivityId
                , '" + voucherVM.GLGeneralInfoName + @"' GLName , '" + voucherVM.BudgetName + @"' BudgetName,'" + voucherVM.ActivityName + @"' ActivityName , Convert(bit, 'True') Active 
				FROM EmployeeFullAndFinalSettlement  E
				LEFT JOIN EmployeeFullAndFinalSettlementItem EI on EI.FinalSettlementId=E.FinalSettlementId AND EI.EmpSystemId=E.EmpSystemId AND EI.UserName='NetPayable'
				WHERE  E.VoucherId IS NULL AND CAST(EI.Value AS decimal(18,2))>0 AND E.FinalSettlementId='" + disbursementAdviceId + @"' AND E.EmpSystemId in (" + empSystemIds + @")

                )X
                GROUP BY

                X.OtherName,X.TrnType,X.GLName,X.BudgetName,X.ActivityName,X.GLGeneralInfoId,X.BudgetMasterId,X.ActivityId,X.Active
                ORDER BY X.TrnType DESC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFinalSettlementDisbursementVoucherList(GridParameter parameters)
        {
            AccountsSalaryPayableService accountsSalaryPayableService = new AccountsSalaryPayableService(_sqlRepository);
            return Json(accountsSalaryPayableService.GetFinalSettlementDisbursementVoucherList(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ParkFinalSettlementDisbursement(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> directJVList, string disbursementAdviceId, List<Dictionary<string, object>> goodWorkPaymentAdviseDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            voucherVM.CompanyGroupId = identity.CompanyGroupId;
            voucherVM.CompanyId = identity.CompanyId;
            voucherVM.PlantId = identity.PlantId;
            voucherVM.IsPark = true;
            voucherVM.Amount = directJVList.Sum(r => r.DrAmount);
            voucherVM.SourceType = SourceType.FinalSettlementJournal.ToString();
            string goodWorkPaymentAdviseDetailIds = "";
            if (goodWorkPaymentAdviseDetail != null)
            {
                foreach (var item in goodWorkPaymentAdviseDetail)
                {
                    if (goodWorkPaymentAdviseDetailIds == "")
                    {
                        goodWorkPaymentAdviseDetailIds = "'" + item["EmpSystemId"] + "'"; ;
                    }
                    else
                    {
                        goodWorkPaymentAdviseDetailIds += ",'" + item["EmpSystemId"] + "'";

                    }
                }
            }
            return Json(new { Message = string.Format(AplosMessage.VoucherSave, _salaryDisbursementService.ParkFinalSettlementDisbursement(voucherVM, directJVList, disbursementAdviceId, goodWorkPaymentAdviseDetailIds, goodWorkPaymentAdviseDetail)) });
        }
        [HttpPost]
        public JsonResult PostFinalSettlementdisbursement(string voucherId)
        {
            _salaryDisbursementService.PostFinalSettlementdisbursement(voucherId);
            return Json(new { Message = AplosMessage.Posted });
        }
        [HttpPost]
        public ActionResult DeleteFinalSettlementDisbursementVoucher(string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            _salaryDisbursementService.DeleteFinalSettlementDisbursementVoucher(identity.PlantId, voucherId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpSepItemReportPdf(ReportFormat reportFormat, string empId)
        {
            try
            {
                string fileName = "";

                IWorkbook workbook = GetEmpSepItemWorkbook("Item", empId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "EmpSepItemReport";
                // return RenderReportAsPdf(workbook, reportFileName);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;

                    case ReportFormat.PdfView:
                        PdfDocument document1 = new PdfDocument();
                        ExcelToPdfConverterSettings settings1 = new ExcelToPdfConverterSettings();
                        settings1.TemplateDocument = document1;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document1 = converter1.Convert(settings1);
                        }
                        document1.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                        //return RenderReportAsPdf(document1, reportFileName);
                        return RenderReportAsPdf(workbook, reportFileName);
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IWorkbook GetEmpSepItemWorkbook(string SheetName, string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder = null;

                string sql = @"SELECT EI.EmpSystemId,EM.EmployeeCode,EM.EmployeeName,FORMAT(EM.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(EM.DOS,'dd-MMM-yyyy')DOS,FORMAT(R.ResignationDate,'dd-MMM-yyyy')ResignationDate,ESI.SandardName ItemName,EI.Value,EI.Remarks,ESI.EntryState 
,EM.FatherName,DP.UserName Department,S.UserName Section,LD.UserName Designation,EI.AddedBy,AEM.EmployeeName ApproveBy,EM.PaymentMode,ApproveStatus=CASE WHEN  M.IsApproved=1 THEN 'Approved' ELSE 'Pending' END, M.IsApproved,EB.BankAccNo,B.UserName Bank,EB.IFSCCode,C.UserName Company
FROM dbo.EmployeeFullAndFinalSettlementItem  EI
LEFT JOIN dbo.EmployeeSeperationItem ESI ON ESI.Id=EI.EmployeeSeperationItemId
LEFT JOIN dbo.EmployeeInformation EM ON EM.SystemId=EI.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = EM.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
LEFT JOIN dbo.EmployeeBankInfo EB ON EB.EmpSystemID=EM.SystemId
LEFT JOIN HKP.Bank B ON B.Id=EB.BankSystemID
LEFT JOIN ORG.Department DP ON DP.Id=PR.DepartmentId
LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
LEFT JOIN HKP.LegalDesignation LD ON LD.Id=EM.LegalDesignationId
LEFT JOIN EmployeeFullAndFinalSettlementMaster M ON M.Id=EI.FinalSettlementId
LEFT JOIN dbo.EmployeeInformation AEM ON AEM.SystemId=M.ApproveById
LEFT JOIN ORG.Company C ON C.Id=AEM.CompanyId
LEFT JOIN [TRN].[Resignation] R ON R.EmployeeId=EM.SystemId
AND R.Id=(SELECT TOP 1 Id FROM [TRN].[Resignation] MR WHERE MR.EmployeeId=R.EmployeeId ORDER BY MR.UpdatedDate DESC)
Where EI.EmpSystemId='" + empId + @"' AND ESI.IsReportItem=1
Order By ESI.Sequence";
                dtOrder = _sqlRepository.GetDataTable(sql);


                if (dtOrder.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }
                ReportUtility reportUtility = new ReportUtility();

                int ROW = 6; int COL = 1;
                sheet.Range[ROW, COL].Text = "Employee Code :" + dtOrder.Rows[0]["EmployeeCode"].ToString() + "";
                sheet[ROW, COL].ColumnWidth = 30;
                sheet.Range[ROW, 2].Text = "Employee Name :" + dtOrder.Rows[0]["EmployeeName"].ToString() + "";
                sheet[ROW, 2].ColumnWidth = 35;
                sheet.Range[ROW, 3].Text = "Father Name :" + dtOrder.Rows[0]["FatherName"].ToString() + "";
                sheet[ROW, 3].ColumnWidth = 35;
                ROW++; COL = 1;
                sheet.Range[ROW, COL].Text = "Department :" + dtOrder.Rows[0]["Department"].ToString() + "";
                sheet.Range[ROW, 2].Text = "Section :" + dtOrder.Rows[0]["Section"].ToString() + "";
                sheet.Range[ROW, 3].Text = "Designation :" + dtOrder.Rows[0]["Designation"].ToString() + "";
                ROW++; COL = 1;
                sheet.Range[ROW, COL].Text = "Joining Date :" + dtOrder.Rows[0]["DOJ"].ToString() + "";
                sheet.Range[ROW, 2].Text = "Resign Date :" + dtOrder.Rows[0]["ResignationDate"].ToString() + "";
                sheet.Range[ROW, 3].Text = "Separation Date :" + dtOrder.Rows[0]["DOS"].ToString() + "";
                ROW++; COL = 1;
                sheet.Range[ROW, COL].Text = "Payment Mode :" + dtOrder.Rows[0]["PaymentMode"].ToString() + "";
                sheet.Range[ROW, 2].Text = "Approve Status :" + dtOrder.Rows[0]["ApproveStatus"].ToString() + "";
                sheet.Range[ROW, 3].Text = "Approve By :" + dtOrder.Rows[0]["ApproveBy"].ToString() + "";
                ROW++; COL = 1;
                sheet.Range[ROW, COL].Text = "BankAccNo :" + dtOrder.Rows[0]["BankAccNo"].ToString() + "";
                sheet.Range[ROW, 2].Text = "Bank :" + dtOrder.Rows[0]["Bank"].ToString() + "";
                sheet.Range[ROW, 3].Text = "IFSC Code :" + dtOrder.Rows[0]["IFSCCode"].ToString() + "";
                int HROW = ROW;
                sheet.Range[6, 1, HROW, 3].BorderAround(ExcelLineStyle.Hair);


                ROW++;
                ROW++;
                #region ColumnsHeader

                sheet[ROW, COL].Text = "SNo"; int colSL = COL; COL++;
                sheet[ROW, COL].Text = "Item Name"; int colIN = COL; COL++;
                sheet[ROW, COL].Text = "Value"; int colPackingType = COL;


                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                #endregion columns

                ROW++;
                int startRow = ROW;
                int cnt = 0;
                #region DataPlot
                double NetPayable = 0;
                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    if (dtOrder.Rows[i]["ItemName"].ToString() == "Net Payable")
                    {
                        NetPayable = Convert.ToDouble(dtOrder.Rows[i]["Value"].ToString());
                    }
                    cnt++;
                    sheet[ROW, colSL].Number = Library.Service.Extension.clsStaticInfo.dbl(cnt.ToString());
                    sheet[ROW, colIN].Text = dtOrder.Rows[i]["ItemName"].ToString();
                    if (!string.IsNullOrEmpty(dtOrder.Rows[i]["Remarks"].ToString()))
                    {
                        sheet[ROW, colPackingType].Text = dtOrder.Rows[i]["Value"].ToString() + " - " + dtOrder.Rows[i]["Remarks"].ToString(); 
                    }
                    else
                    {
                        sheet[ROW, colPackingType].Text = dtOrder.Rows[i]["Value"].ToString();
                    }
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                #endregion
                int edCRow = ROW;

                edCRow++;
                edCRow++;
                edCRow++;


                sheet.Range[edCRow - 1, 1].Text = dtOrder.Rows[0]["AddedBy"].ToString();
                sheet.Range[edCRow, 1].Text = "Created By";
                if (Convert.ToBoolean(dtOrder.Rows[0]["IsApproved"].ToString()) == true)
                {
                    sheet.Range[edCRow - 1, 2].Text = dtOrder.Rows[0]["ApproveBy"].ToString();
                }
                else
                {
                    sheet.Range[edCRow - 1, 2].Text = "";
                }
                sheet.Range[edCRow, 2].Text = "Approved By";
                sheet.Range[edCRow - 1, 3].Text = "";
                sheet.Range[edCRow, 3].Text = "Authorized By";

                edCRow++;
                edCRow++;


                sheet.Range[edCRow, 2].Text = "FinalClearance – Cum – Acceptance Receipt";
                sheet.Range[edCRow, 2].CellStyle.Font.Bold = true;
                sheet.Range[edCRow, 2].CellStyle.Font.Size = 10f;
                sheet.Range[edCRow, 2].CellStyle.Font.Underline = (ExcelUnderline)7;
                edCRow++;
                edCRow++;
                string inWord = reportUtility.InWord(NetPayable, null);
                ROW = edCRow; COL = 1;
                sheet.Range[ROW, COL, ROW, COL + 2].Text = "I have received a sum of Rs. " + NetPayable + ", Rupees. " + inWord + " towards full and final settlement of all my dues ";
                sheet.Range[ROW, COL, ROW, COL + 2].Merge();
                ROW++;

                sheet.Range[ROW, COL, ROW, COL + 2].Text = "from " + dtOrder.Rows[0]["Company"].ToString() + " and have no other claim, whatsoever, against the company.";
                sheet.Range[ROW, COL, ROW, COL + 2].Merge();
                ROW++;
               
                ROW++;
                ROW++;
                ROW++;


                sheet.Range[ROW, 1].Text = "Date: _________________  ";
                sheet.Range[ROW, 3].Text = "Signature of the Employee";
                #region ReportHeader
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();


                reportUtility.CompanyHeader(ref sheet, 3, "Employee Full & Final Report", identity.CompanyId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);

                sheet.PageSetup.CenterHorizontally = true;
                #endregion


                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
        [HttpPost, Authorize]
        public ActionResult GetFNFReport(string reportFileName, string fromDate, string toDate)
        {
            try
            {
                string fileName = "";
                fileName = GetFNFWorkbook("", reportFileName, fromDate, toDate);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetFNFWorkbook(string ReportHeader, string reportFileName, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder = null;
               
                string sql = @"Select M.Id DocRefNo,EI.EmployeeCode,EI.EmployeeName,DP.UserName Department,S.UserName Section,SS.UserName SubSection,LD.UserName LegalDesignation,FORMAT(M.AddedDate,'dd-MMM-yyyy') EntryDate,M.AddedBy EntryBy,FORMAT(M.ApproveDateTime,'dd-MMM-yyyy') ApprovalDate
,AE.EmployeeName ApprovalBy,E.VoucherId,V.VoucherNo,FORMAT(V.PostedDate,'dd-MMM-yyyy')PostedDate,V.PostedBy,V.Narration,IT.Value NetPayable,EI.PaymentMode,BN.UserName PaymentBank,EB.BankAccNo AccountNumber,SPD.IFSCCode
from dbo.EmployeeFullAndFinalSettlementMaster M
LEFT JOIN dbo.EmployeeFullAndFinalSettlement E ON E.FinalSettlementId=M.Id
LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=E.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
LEFT JOIN ORG.Department DP ON DP.Id=PR.DepartmentId
LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
LEFT JOIN ORG.SubSection SS ON S.Id=PR.SubSectionId
LEFT JOIN HKP.LegalDesignation LD ON LD.ID=EI.LegalDesignationId
LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=M.ApproveById
LEFT JOIN trn.Voucher V ON V.Id=E.VoucherId
LEFT JOIN dbo.EmployeeFullAndFinalSettlementItem IT ON IT.FinalSettlementId=M.Id AND IT.EmpSystemId=E.EmpSystemId AND IT.UserName='NetPayable'
LEFT JOIN [dbo].[EmployeeBankInfo] EB ON EB.EmpSystemID=E.EmpSystemId
LEFT JOIN HKP.Bank BN ON BN.Id=EB.BankSystemID
LEFT JOIN dbo.SalaryProcessLogDetail SPD ON SPD.EmpSystemId=E.EmpSystemId
AND SPD.Id=(Select top(1)Id from dbo.SalaryProcessLogDetail where EmpSystemId=SPD.EmpSystemId Order  By AddedDate DeSC)

where M.AddedDate between '" + fromDate+@"' AND '"+toDate+"'";
                dtOrder = _sqlRepository.GetDataTable(sql);


                if (dtOrder.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }
                ReportUtility reportUtility = new ReportUtility();

                int ROW = 6; int COL = 1;
                
                #region ColumnsHeader

                sheet[ROW, COL].Text = "Sr. No."; int colSL = COL; COL++;
                sheet[ROW, COL].Text = "Doc Ref No."; int colD = COL; COL++;
                sheet[ROW, COL].Text = "Emp Code"; int colEC = COL; COL++;
                sheet[ROW, colEC].ColumnWidth = 10;
                sheet[ROW, COL].Text = "Emp Name"; int colEN = COL; COL++;
                sheet[ROW, colEN].ColumnWidth = 20;
                sheet[ROW, COL].Text = "Department"; int colDP = COL; COL++;
                sheet[ROW, colDP].ColumnWidth = 20;
                sheet[ROW, COL].Text = "Section"; int colSection = COL; COL++;
                sheet[ROW, COL].Text = "SubSection"; int colSS = COL; COL++;
                sheet[ROW, COL].Text = "Legal Designation"; int colLD = COL; COL++;
                sheet[ROW, colLD].ColumnWidth = 20;
                sheet[ROW, COL].Text = "Entry Date"; int colED = COL; COL++;
                sheet[ROW, COL].Text = "Entry By"; int colEB = COL; COL++;
                sheet[ROW, COL].Text = "Approval Date"; int colAD = COL; COL++;
                sheet[ROW, colAD].ColumnWidth = 10;
                sheet[ROW, COL].Text = "Approval By"; int colAB = COL; COL++;
                sheet[ROW, colAB].ColumnWidth = 20;
                sheet[ROW, COL].Text = "VoucherNo"; int colVN = COL; COL++;
                sheet[ROW, colVN].ColumnWidth = 15;
                sheet[ROW, COL].Text = "Posted Date"; int colPD = COL; COL++;
                sheet[ROW, COL].Text = "PostedBy"; int colPB = COL; COL++;
                sheet[ROW, colPB].ColumnWidth = 10;
                sheet[ROW, COL].Text = "Narration"; int colN = COL; COL++;
                sheet[ROW, COL].Text = "Net Payable"; int colNP = COL; COL++;
                sheet[ROW, COL].Text = "Payment Mode"; int colPM = COL; COL++;
                sheet[ROW, colPM].ColumnWidth = 12;
                sheet[ROW, COL].Text = "Bank"; int colBN = COL; COL++;
                sheet[ROW, colBN].ColumnWidth = 20;
                sheet[ROW, COL].Text = "A/C No."; int colAC = COL; COL++;
                sheet[ROW, colAC].ColumnWidth = 20;
                sheet[ROW, COL].Text = "IFSC"; int colIFSC = COL;


                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                #endregion columns

                ROW++;
                int startRow = ROW;
                int cnt = 0;
                #region DataPlot
                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                                        cnt++;
                    sheet[ROW, colSL].Number = Library.Service.Extension.clsStaticInfo.dbl(cnt.ToString());
                    sheet[ROW, colD].Text = dtOrder.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, colEC].Text = dtOrder.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, colEN].Text = dtOrder.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, colDP].Text = dtOrder.Rows[i]["Department"].ToString();
                    sheet[ROW, colSection].Text = dtOrder.Rows[i]["Section"].ToString();
                    sheet[ROW, colSS].Text = dtOrder.Rows[i]["SubSection"].ToString();
                    sheet[ROW, colLD].Text = dtOrder.Rows[i]["LegalDesignation"].ToString();
                    sheet[ROW, colED].Text = dtOrder.Rows[i]["EntryDate"].ToString();
                    sheet[ROW, colEB].Text = dtOrder.Rows[i]["EntryBy"].ToString();
                    sheet[ROW, colAD].Text = dtOrder.Rows[i]["ApprovalDate"].ToString();
                    sheet[ROW, colAB].Text = dtOrder.Rows[i]["ApprovalBy"].ToString();
                    sheet[ROW, colVN].Text = dtOrder.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, colPD].Text = dtOrder.Rows[i]["PostedDate"].ToString();
                    sheet[ROW, colPB].Text = dtOrder.Rows[i]["PostedBy"].ToString();
                    sheet[ROW, colN].Text = dtOrder.Rows[i]["Narration"].ToString();
                    sheet[ROW, colNP].Number =clsStaticInfo.dbl(dtOrder.Rows[i]["NetPayable"].ToString());
                    sheet[ROW, colPM].Text = dtOrder.Rows[i]["PaymentMode"].ToString();
                    sheet[ROW, colBN].Text = dtOrder.Rows[i]["PaymentBank"].ToString();
                    sheet[ROW, colAC].Text = dtOrder.Rows[i]["AccountNumber"].ToString();
                    sheet[ROW, colIFSC].Text = dtOrder.Rows[i]["IFSCCode"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                ROW++;
                #endregion
                int edCRow = ROW;

              
                #region ReportHeader
                //sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();


                reportUtility.CompanyHeader(ref sheet, endCol, "Employee Full & Final Report", identity.CompanyId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);

                sheet.PageSetup.CenterHorizontally = true;
                #endregion


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet,Authorize]
        public ActionResult EmployeeSattlementReport(string empSystemId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _AttendanceManagementService.EmployeeSattlementReport(empSystemId,identity.PlantId);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View();
        }

       

        #endregion

    }
    public class OpenHeadModelNew
    {
        public string Id { get; set; }
        public string EmpSystemId { get; set; }
        public string EmployeeSeperationItemId { get; set; }
        public string UserName { get; set; }
        public string Formula { get; set; }
        public string FormulaId { get; set; }
        public string Value { get; set; }
        public string EntryState { get; set; }
        public string Remarks { get; set; }

    }



}
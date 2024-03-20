using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Employees;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class FinalSettlementController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;
        public FinalSettlementController(ISqlRepository sqlRepository)
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
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId	
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
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId				
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
WHERE  spc.EmpInfoSystemID= '" + EmpSystemId + @"' AND PayableVoucherId<>'' AND ISNULL(sl.IsDisbursed,0)=0  AND sh.SalaryHead='Net Pay'";
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
                                AND AD.SourceType in ('EmployeeAdvance', 'InterTransaction') AND AD.EmployeeId='" + EmpSystemId + @"' AND AD.PlantId='" + identity.PlantId+@"'
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
                if (UndisbursedEarningList!=null)
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
                            drmo["IsDisbursed"] = true;
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
                                                 LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId				
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
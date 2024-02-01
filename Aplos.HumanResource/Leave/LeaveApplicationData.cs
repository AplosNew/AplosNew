using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Core;
using Library.ViewModel.HR;
using System.Reflection;
using Library.Model.Biometrics;
using Library.Service.Setups;
using Library.Data;
using Library.Service.Extension.HumanResource.Leave;
using Library.Data.UnitOfWorks;
using Library.Service.Biometrics;
using Library.Service.Leave;
using Library.Crosscutting.Security;
using System.Threading;
using Newtonsoft.Json;
using bplib;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;

namespace Library.Service.EmployeeServices
{
    public class LeaveApplicationData
    {

        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailSenderService _mailSenderService;
        private readonly ILeaveTransactionDetailsService _leaveTransactionDetailsService;


        public LeaveApplicationData(IMailSenderService mailSenderService, IUnitOfWork unitOfWork,
            ILeaveTransactionDetailsService leaveTransactionDetailsService)
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

            _mailSenderService = mailSenderService;
            _leaveTransactionDetailsService = leaveTransactionDetailsService;
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<object> GetLeaveType(string PlantId, string EmpId, string GroupId)
        {
            try
            {
                var sql = @"SELECT LT.ID as Value , LT.UserName as Text FROM LeaveType LT
                                    LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                    LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
                                    LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId='" + PlantId + @"') DM ON DM.LeavePolicyMasterId=LPM.SystemID
                                    LEFT JOIN EmployeeInformation EI ON EI.GivenDesignationId=DM.DesignationId
                                    LEFT JOIN ESICEligibleEmployee EE ON EE.EmpSystemID=EI.SystemId
                                    WHERE EI.SystemID='" + EmpId + @"' AND EI.GroupID='" + GroupId + @"' AND EI.PlantID='" + PlantId + @"' AND LT.IsGeneral = 1 AND LT.LeaveType <>'Maternity'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetCalYearInfo(string CalYearId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select * from YearlyCalendar WHERE ID='" + CalYearId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetESICEligibleEmployeeFromEnum(string empSystemId, string FromDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT IsEligible,SalaryStructureId,EmpSystemId,m.EffectiveDate
                                  FROM [dbo].[EmployeeEligibleForSalaryHeadEnum] n
                                  left join (select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster
                                  union
                                  select SystemID,EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster
                                  )
                                   mm on mm.SystemID=n.SalaryStructureId
                                  inner join (
                                  select MAX(EffectiveDate)EffectiveDate,EmpInfoSystemID from (
                                  select EffectiveDate,EmpInfoSystemID from SalaryInfoDefineMaster where IsApproved=1 
                                  union
                                   select EffectiveDate,EmpInfoSystemID from SalaryInfoBackMaster where IsApproved=1 
                                   ) x 
                                   group by EmpInfoSystemID
                                  )m on mm.EffectiveDate=m.EffectiveDate and m.EmpInfoSystemID=mm.EmpInfoSystemID
                                  where SalaryHeadEnum='ESIC' and mm.EmpInfoSystemID='" + empSystemId + @"'  and IsEligible=1
                                 "
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public IEnumerable<object> GetEmpInfo(string EmpId)
        {
            try
            {
                DataTable dtEMPBudgetCode = _sqlRepository.GetDataTable(@"SELECT * FROM EmployeeInformation EEI  where SystemId = '" + EmpId + @"'");
                DataTable dtRPBudgetCode = _sqlRepository.GetDataTable(@"SELECT * FROM MST.ManpowerBudget WHERE Id = '" + dtEMPBudgetCode.Rows[0]["BudgetCode"] + @"' ");


                string Sql = @"SELECT EEI.SystemId as RespersonId,EEI.EmailId,EEI.EmployeeName as ResPersonName,EEI.BudgetCode  FROM EmployeeInformation EEI
                    --LEFT JOIN MST.ManpowerBudget MB ON EEI.BudgetCode = MB.ROBudgetCode
                    WHERE  EEI.BudgetCode = (select Id from MST.ManpowerBudget where Id ='" + dtRPBudgetCode.Rows[0]["ROBudgetCode"].ToString() + @"')   AND
                    EEI.DOJ = (SELECT MIN(DOJ) FROM EmployeeInformation WHERE EmployeeStatus = 'Active' and BudgetCode = (select Id from MST.ManpowerBudget where Id ='" + dtRPBudgetCode.Rows[0]["ROBudgetCode"].ToString() + @"'))";


                return _sqlRepository.GetDataCollection(Sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetCalender(string PlantId)
        {
            try
            {
                var sql = @"SELECT distinct Id as Value, YearNo as Text,FromDate,ToDate FROM dbo.YearlyCalendar WHERE PlantId='" + PlantId + "' ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetRestEmployee(string empSystemId, string fromDate, string toDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT D.EmpSystemId,R.AttendanceRestDate FROM AttendanceRest R  
                            LEFT JOIN AttendanceRestDetail D ON D.AttendanceRestId=R.Id 
                            WHERE D.EmpSystemId='" + empSystemId + "' AND R.AttendanceRestDate BETWEEN '" + fromDate + @"' and '" + toDate + "' "
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetEmpODData(string empSystemId, string fromDate, string toDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT ODD.Id FROM EmployeeOnDuty OD
							LEFT JOIN EmployeeOnDutyDetails ODD ON ODD.OnDutyId=OD.Id
							WHERE OD.EmpSystemId='" + empSystemId + @"' AND  ODD.WorkDate BETWEEN '" + fromDate + @"' and '" + toDate + "' "
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetEmpPreviousData(string empSystemId, string fromDate, string toDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"
                            SELECT LTD.* from LeaveTransaction LT
                            LEFT JOIN  LeaveTransactionDetails LTD ON LTD.LvTrnsSystemID=LT.SystemID
                            WHERE EmpSystemID='" + empSystemId + @"' AND WorkDate between '" + fromDate + @"' AND '" + toDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public void CheckMaxLeaveataTime(IEnumerable<LeaveData> DataToSave, string leavepolicymasterId, decimal leaveDays, string yearId = "")
        {
            List<LeaveData> master = DataToSave.ToList();
            DataSet dsLeavePolicyMaster = null;
            clsOffDayList _odl = null;
            try
            {
                DateTime doc = new DateTime();
                DateTime doj = new DateTime();
                DateTime dateAfterAllow = new DateTime();
                string plantId = master[0].PlantID;
                string companyGroupId = master[0].GroupID;
                string leaveTypeId = master[0].LTSystemID;
                string EmpSystemID = master[0].EmpSystemID;
                DateTime fromDate = Convert.ToDateTime(master[0].FromDate);
                DateTime applyDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                DateTime toDate = Convert.ToDateTime(master[0].ToDate);
                _odl = new clsOffDayList();

                _odl._getLeavePolicyMaster(EmpSystemID, plantId, out dsLeavePolicyMaster);
                if (dsLeavePolicyMaster.Tables[0].Rows.Count > 0)
                {
                    leavepolicymasterId = dsLeavePolicyMaster.Tables[0].Rows[0]["LeavePolicyMasterId"].ToString();
                }
                else
                {
                    throw new Exception("#Leave policy is not configured...#");
                }

                var leaveData = GetMaxLeaveAtaTime(companyGroupId, plantId, leavepolicymasterId, leaveTypeId);

                var _sql = @"select DOC,DOJ from dbo.EmployeeInformation where SystemId='" + EmpSystemID + "'";
                DataTable dtx = _sqlRepository.GetDataTable(_sql);

                string DOC = "", DOJ = "";
                if (dtx.Rows.Count > 0)
                {
                    DOC = dtx.Rows[0]["DOC"].ToString();
                    DOJ = dtx.Rows[0]["DOJ"].ToString();
                }

                if (leaveData.Tables[0].Rows.Count > 0)
                {
                    int maxDayAllowed = (int)(leaveData.Tables[0].Rows[0]["MaxAllocationLimit"]);
                    bool isMaxAtaRowExp = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsExcessAllow"].ToString());
                    bool subToApproval = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsSubjectToApproval"].ToString());

                    bool IsAllowedInProbationalPeriod = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsAllowed"].ToString());
                    bool isAllowedInSpecialCase = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsAllowedonspecialappeal"].ToString());
                    int allowAfterDays = Convert.ToInt32(leaveData.Tables[0].Rows[0]["AllowedAfterDays"].ToString());
                    bool isPostAppAllow = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsPostApplicationAllowed"].ToString());//
                    bool _IsAvailExceptionAllowedOnSpecialAppeal = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsAvailExceptionAllowedOnSpecialAppeal"].ToString());//IsAvailExceptionAllowedOnSpecialAppeal

                    decimal applied = leaveDays;
                    if (applied > maxDayAllowed && isMaxAtaRowExp == false)
                    {
                        throw new Exception("#Max leave at a time cannot be greater then " + maxDayAllowed + "  day#");
                    }
                    if (applied > maxDayAllowed && isMaxAtaRowExp == true && subToApproval == false)
                    {
                        throw new Exception("#Max leave at a time cannot be greater then " + maxDayAllowed + "  day#");
                    }

                    string stDoc = DOC.ToString();
                    if (!string.IsNullOrEmpty(DOC.ToString()) && !string.IsNullOrEmpty(DOJ.ToString()))
                    {
                        if (stDoc != null && stDoc != "")
                        {
                            doc = Convert.ToDateTime(stDoc);
                        }
                        if (DOJ.ToString() != null)
                        {
                            doj = Convert.ToDateTime(DOJ.ToString());
                        }
                        dateAfterAllow = doj.AddDays(allowAfterDays);
                    }

                    #region check post applicatiton allowed

                    if (isPostAppAllow == false && Convert.ToDateTime(applyDate.ToString().Trim()) > Convert.ToDateTime(toDate.ToString("dd-MMM-yyyy")))
                    {
                        throw new Exception("#Post Leave application is not allowed for this Leave Policy#");
                    }

                    #endregion check post applicatiton allowed

                    if (stDoc != null && stDoc != "")
                    {
                        doc = Convert.ToDateTime(stDoc);

                        if (IsAllowedInProbationalPeriod == true && Convert.ToDateTime(fromDate.ToString("dd-MMM-yyyy")) < doc && isAllowedInSpecialCase != true)
                        {
                            throw new Exception("#Leave application is not allowed for this employee before DOC#");
                        }
                    }
                    if (IsAllowedInProbationalPeriod == false && (stDoc == null || stDoc == "") && isAllowedInSpecialCase == false)
                    {
                        throw new Exception("#Leave application is not allowed for this employee before DOC#");
                    }
                    if (Convert.ToDateTime(fromDate.ToString("dd-MMM-yyyy")) < doj)
                    {
                        throw new Exception("#Leave application is not allowed Before DOJ#");
                    }
                    if (IsAllowedInProbationalPeriod == true && Convert.ToDateTime(fromDate.ToString("dd-MMM-yyyy")) < dateAfterAllow)
                    {
                        throw new Exception("#Leave application is not allowed for this employee before " + dateAfterAllow.ToString("dd-MMM-yyyy") + "#");
                    }

                    //check balance
                    if (yearId.Length > 0)
                    {
                        List<LeaveTransactionVM> list_balance = (List<LeaveTransactionVM>)LoadGrdAllocatedLvDetails(companyGroupId, plantId, EmpSystemID, yearId);
                        foreach (var item in list_balance)
                        {
                            if (item.LTSystemID == leaveTypeId)
                            {
                                if (_IsAvailExceptionAllowedOnSpecialAppeal == false)
                                {
                                    if (string.IsNullOrEmpty(master[0].SystemID) == false)//edit
                                    {
                                        item.Balance += master[0].LeaveDays;
                                    }

                                    if (item.Balance < leaveDays)
                                    {
                                        throw new Exception("#Can't apply more than Balance...#");
                                    }
                                }
                            }//leave type
                        }//foreach
                    }//yearid
                    //balance check

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetMaxLeaveAtaTime(string sGroupID, string sPlantID, string strLPMSystemID, string strLvTypeId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT ISNULL(IsExcessAllow,0) IsExcessAllow, ISNULL(IsSubjectToApproval,0)IsSubjectToApproval, ISNULL(MaxAllocationLimit,0)MaxAllocationLimit
                            ,LvAvailedOnDOC IsAllowed,IsAllowedonspecialappeal,AllowedAfterDays,IsPostApplicationAllowed,isnull(IsAvailExceptionAllowedOnSpecialAppeal,0) IsAvailExceptionAllowedOnSpecialAppeal
                            FROM dbo.LeavePolicyDetail
                            WHERE  (LPMSystemID = '" + strLPMSystemID + @"') AND (LTSystemID = '" + strLvTypeId + @"')"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetValidations(string sGroupID, string sPlantID, string EmpSystemID, string calYearId)
        {

            try
            {
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;

                // var esic = GetESICEligibleEmployee(EmpSystemID);
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = Convert.ToDateTime(dsCalYear.Tables[0].Rows[0]["FromDate"]).ToString("dd-MMM-yyyy");
                    _ToDate = Convert.ToDateTime(dsCalYear.Tables[0].Rows[0]["ToDate"]).ToString("dd-MMM-yyyy");

                }
                else
                {
                    throw new Exception("#No Year found...#");
                }
                var esic = GetESICEligibleEmployeeFromEnum(EmpSystemID, _FromDate);

                if (esic.Tables[0].Rows.Count > 0)
                {
                    GridParameter parameters = null;
                    parameters = new GridParameter
                    {
                        ExportType = "DATASET",
                        CmdText = @"SELECT	els.CalanderYearID,ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         --ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear
                                        ---,els.DaysCanBeSanctioned
										 ,DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end
 ,CurrentAllocationDCBS=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end

                                        ,els.EncashedInbetween
                                        ,ltd.IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                        ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType

-----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN  
,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
	CASE WHEN
	-----------------------------------DOJorDOC start -----------------------------------------------------------
								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
                                       END
---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
	> GETDATE() then 
		    CONVERT(BIT,0)------No
        ELSE  CONVERT(BIT,1) END---Yes
ELSE CONVERT(BIT,0) END  ---No

----------------------------------------------------------------------------------------------------------------------



                                          FROM (select * from trn.EmployeeLeaveSummary where CalanderYearId='" + calYearId + @"' and EmployeeId ='" + EmpSystemID + @"' ) els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
                                        LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m 
                                           where  (FromDate between '" + _FromDate + @"' and '" + _ToDate + @"') and (ToDate between '" + _FromDate + @"' and '" + _ToDate + @"')
                                           group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																			select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1  and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                            group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE els.EmployeeID = '" + EmpSystemID + @"'                                             
                                              AND CalanderYearID = '" + calYearId + @"'
                                             AND els.LeaveTypeId IN ( --IN


                                            SELECT LT.ID FROM dbo.ESICPolicyLeaveType AS EPLT
                  LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                  WHERE
                  EPLT.LeaveTypeID IN
                   (
                     SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                  LEFT JOIN  (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId WHERE DC.PlantId='" + sPlantID + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                  LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                  WHERE EI.SystemID='" + EmpSystemID + @"' AND EI.GroupID='" + sGroupID + @"' AND EI.PlantID='" + sPlantID + @"'
                   )
                AND
                EPLT.ESICPolicyMasterID IN (
                 SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
WHERE DC.PlantId='" + sPlantID + @"') DM
                 WHERE DM.DesignationId IN (
                  SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + EmpSystemID + @"'
                  )
                )

                                            				)--IN"
                    };
                    return _sqlRepository.GetGridData(parameters).Source;
                }
                else
                {
                    GridParameter parameters = null;
                    parameters = new GridParameter
                    {
                        ExportType = "DATASET",
                        CmdText = @"SELECT	els.CalanderYearID, ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID
                                         --ISNULL(ltd.IsProrataPreviousyear,0)IsProrataPreviousyear,
                                         ,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
                                        
                                         ---,els.DaysCanBeSanctioned
										 ,DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end
 ,CurrentAllocationDCBS=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) end

                                        ,ISNULL(ltd.IsAvailExceptionAllowedOnSpecialAppeal,0)IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                        ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                         --ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,els.EncashedInbetween,
										 ISNULL(acApl.ldays,0) ldays,lt.LeaveType


-----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
---,IsBroughtForwardAdd=CASE WHEN LT.LeaveType='Earn' THEN
,IsBroughtForwardAdd=CASE WHEN 1=1 THEN  
	CASE WHEN
	-----------------------------------DOJorDOC start -----------------------------------------------------------
								CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            										 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  emp.DOJ )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  emp.DOJ ) END
										   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   							 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	emp.DOC  )
																		  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	emp.DOC  )
																	      WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	emp.DOC  )
										   						END
                                       END
---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
	> GETDATE() then 
		    CONVERT(BIT,0)------No
        ELSE  CONVERT(BIT,1) END---Yes
ELSE CONVERT(BIT,0) END  ---No

----------------------------------------------------------------------------------------------------------------------
                                          FROM (select * from trn.EmployeeLeaveSummary where CalanderYearId='" + calYearId + @"' and EmployeeId ='" + EmpSystemID + @"' ) els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
                            where  (FromDate between '" + _FromDate + @"' and '" + _ToDate + @"') and (ToDate between '" + _FromDate + @"' and '" + _ToDate + @"')
                                                    group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																				SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																										FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC 
																													ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId
                                                WHERE els.EmployeeID = '" + EmpSystemID + @"'                                             
                                              AND CalanderYearID = '" + calYearId + @"'
                                              AND els.LeaveTypeId not IN 
                                            (select id from LeaveType where IsESIC=1 and IsGeneral=0) AND lt.LeaveType <>'Maternity'"
                    };
                    parameters.sort = "LeaveName";
                    parameters.order = "ASC";
                    return _sqlRepository.GetGridData(parameters).Source;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function

        public IEnumerable<object> LoadGrdAllocatedLvDetails(string companyGroupId, string plantId, string employeeId, string calanderYearId)
        {
            DataRow drLocal = null;
            DataView dvLocal = null;
            try
            {
                var dsLvAllo = GetValidations(companyGroupId, plantId, employeeId, calanderYearId);

                dvLocal = new DataView();
                dvLocal.Table = dsLvAllo.Tables[0];
                bool proDataCurrentYear = false;
                bool isAvailExceptionAllowed = false;
                List<object> ss = new List<object>();

                object ob = new object { };

                for (int i = 0; i < dsLvAllo.Tables[0].Rows.Count; i++)
                {
                    dvLocal.RowFilter = "LvPolDetailsSystemID = '" + dsLvAllo.Tables[0].Rows[i]["LvPolDetailsSystemID"].ToString().Trim() + "'";
                    if (dvLocal.Count == 1)
                    {
                        drLocal = dvLocal[0].Row;
                        drLocal.BeginEdit();
                        proDataCurrentYear = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsProratacurrentyear"].ToString());
                        isAvailExceptionAllowed = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsAvailExceptionAllowedOnSpecialAppeal"].ToString());

                        drLocal["Applied"] = dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim();
                        drLocal["Availed"] = dsLvAllo.Tables[0].Rows[i]["Availed"].ToString().Trim();
                        drLocal["BroughtForward"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["BroughtForward"].ToString().Trim());
                        decimal DaysCanBeSanctioned = 0;

                        decimal BroughtForward = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["BroughtForward"].ToString().Trim());
                        DaysCanBeSanctioned = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["DaysCanBeSanctioned"].ToString().Trim());

                        decimal EncashedInbetween = 0;
                        if (!string.IsNullOrEmpty(dsLvAllo.Tables[0].Rows[i]["EncashedInbetween"].ToString()))
                        {
                            EncashedInbetween = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["EncashedInbetween"].ToString().Trim());
                        }
                        drLocal["EncashedInbetween"] = EncashedInbetween;
                        bool IsBroughtForwardAdd = true;
                        IsBroughtForwardAdd = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsBroughtForwardAdd"].ToString());
                        decimal TotalEarn = 0;
                        if (IsBroughtForwardAdd)
                        {
                            TotalEarn = BroughtForward + DaysCanBeSanctioned;
                        }
                        else
                        {
                            TotalEarn = DaysCanBeSanctioned;
                        }


                        if (dsLvAllo.Tables[0].Rows[i]["LeaveType"].ToString().Trim().ToUpper() != "EARN")
                        {
                            if (proDataCurrentYear == false)
                            {
                                #region 01

                                drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim());
                                drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());

                                #endregion
                            }
                            else
                            {
                                #region 02

                                drLocal["LeaveDays"] = TotalEarn;
                                drLocal["Balance"] = TotalEarn - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());

                                #endregion
                            }
                        }
                        else
                        {
                            drLocal["LeaveDays"] = TotalEarn;
                            drLocal["Balance"] = TotalEarn - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim()) - EncashedInbetween;

                        }



                        drLocal.EndEdit();

                    }
                }

                var list = new List<LeaveTransactionVM>();
                list = ConvertDataTable<LeaveTransactionVM>(dsLvAllo.Tables[0]);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            try
            {
                var data = new List<T>();
                foreach (DataRow row in dt.Rows)
                {
                    var item = GetItem<T>(row);
                    data.Add(item);
                }
                return data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static T GetItem<T>(DataRow dr)
        {
            var temp = typeof(T);
            var obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        if (dr[column.ColumnName] == DBNull.Value)
                            dr[column.ColumnName] = "";
                        pro.SetValue(obj, dr[column.ColumnName], null);
                        break;
                    }
                }
            }
            return obj;
        }

        public string Create(IEnumerable<LeaveData> DataToSave)
        {
            try
            {
                DataSet dsMaster, dsApprove, dsCancel;
                string TableName = "dbo.LeaveTransaction";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<LeaveData> items = DataToSave.ToList();

                //AttendanceProcessAplos ob = new AttendanceProcessAplos();
                //ob.LockValidation(items[0].PlantID, items[0].FromDate.ToString("dd-MMM-yyyy"), Convert.ToDateTime(items[0].ToDate).ToString("dd-MMM-yyyy"), items[0].EmpSystemID);

                if (!string.IsNullOrEmpty(items[0].SystemID))
                {
                    con.OpenDataSetThroughAdapter("select IsApproved from " + TableName + " where SystemID='" + items[0].SystemID + "'", out dsApprove, false, "1");

                    if (bplib.clsWebLib.GetBoolData(dsApprove.Tables[0].Rows[0]["IsApproved"]) == false)
                    {
                        _sqlRepository.ExecuteSqlCommand(@"DELETE FROM [dbo].LeaveTransactionDetails WHERE LvTrnsSystemID ='" + items[0].SystemID + "'");
                    }
                    else
                    {
                        throw new CustomException("#Approved Data can not be updated.#");
                    }


                    con.OpenDataSetThroughAdapter("select IsCancel from " + TableName + " where SystemID='" + items[0].SystemID + "'", out dsCancel, false, "1");

                    if (bplib.clsWebLib.GetBoolData(dsCancel.Tables[0].Rows[0]["IsCancel"]) == true)
                    {
                        throw new CustomException("#Rejected Leave cann't be modified...#");
                    }
                }

                //  Validations 
                var restEmployee = GetRestEmployee(items[0].EmpSystemID, items[0].FromDate.ToString("dd-MMM-yyyy"), items[0].ToDate.ToString());

                if (restEmployee.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("#The Employee is on rest.#");
                }

                var getOdData = GetEmpODData(items[0].EmpSystemID, items[0].FromDate.ToString("dd-MMM-yyyy"), items[0].ToDate.ToString());
                if (getOdData.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("#The Employee is on duty.#");
                }

                //for sandwich W/H

                PolicySandwichVM _sandwichVM = new PolicySandwichVM();
                clsOffDayList _clsOffDayList = new clsOffDayList();
                List<string> _list_W = new List<string>();
                List<string> _list_H = new List<string>();
                createOffDayList(items[0].PlantID, DataToSave, _list_H, _list_W);

                //for sandwich W/H

                string LVPolicyMasterSystemID = "";
                var sql = @"select LVPolicyMasterSystemID from dbo.EmployeeInformation where SystemId='" + items[0].SystemID + "'";
                DataTable dtx = _sqlRepository.GetDataTable(sql);
                if (dtx.Rows.Count > 0)
                {
                    LVPolicyMasterSystemID = dtx.Rows[0]["LVPolicyMasterSystemID"].ToString();
                }



                decimal leaveDays_ = items[0].LeaveDays;
                decimal _leave_days = items[0].LeaveDays;
                if (items[0].LeaveDayType == "FirstHalfDay" || items[0].LeaveDayType == "SecondHalfDay")
                {
                    leaveDays_ = 0.5m;
                }
                else
                {
                    clsEmpWiseLeavePolicyInfo _obj_POD = new clsEmpWiseLeavePolicyInfo(items[0].PlantID);
                    _obj_POD.GetLeaveCount(items[0].EmpSystemID, items[0].LTSystemID, _list_H.Count, _list_W.Count, ref leaveDays_, out _sandwichVM);

                    if (leaveDays_ <= 0)
                    {
                        throw new Exception("#Define OffDay setting for this employee#");
                    }
                }


                CheckMaxLeaveataTime(DataToSave, LVPolicyMasterSystemID, items[0].LeaveDays);


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where SystemID='" + items[0].SystemID + "'", out dsMaster, false, "1");
                string _Id = "";


                foreach (LeaveData item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].SystemID == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["SystemID"] = "Lt" + _Id;
                        dr["PlantID"] = item.PlantID;
                        dr["EmpSystemID"] = item.EmpSystemID;
                        dr["LTSystemID"] = item.LTSystemID;
                        dr["GroupID"] = item.GroupID;
                        dr["FromDate"] = item.FromDate;
                        dr["ToDate"] = item.ToDate;
                        dr["LeaveDays"] = item.LeaveDays;
                        dr["LvReason"] = item.LvReason;
                        dr["LeaveDayType"] = item.LeaveDayType;
                        dr["LeaveDay"] = item.LeaveDay;
                        dr["LeaveStatus"] = item.LeaveStatus;
                        dr["ExceptionLeave"] = item.ExceptionLeave;
                        dr["MaternityLeavePolicyId"] = item.MaternityLeavePolicyId;
                        dr["IsApproved"] = item.IsApproved;
                        dr["CompanyId"] = item.CompanyId;
                        dr["IsPostApplied"] = item.IsPostApplied;
                        dr["IsAdminApproved"] = item.IsAdminApproved;
                        dr["AppliedBy"] = item.AppliedBy;
                        dr["LeaveDayType"] = item.LeaveDayType;
                        dr["ApprovalPerson"] = item.ApprovalPerson;
                        dr["AddedBy"] = item.AddedBy;
                        dr["DateAdded"] = DateTime.Now.ToString();
                        dr["AppliedDate"] = DateTime.Now.ToString();
                        dr["LeaveDay"] = item.LeaveDay;
                        dr["LeaveStatus"] = item.LeaveStatus;
                        dr["IsCancel"] = item.IsCancel;
                        dr["FirstApprovingAuthority"] = item.FirstApprovingAuthority;
                        dr["FirstApprovingStatus"] = item.FirstApprovingStatus;
                        dr["MaternityLeavePolicyId"] = item.MaternityLeavePolicyId;
                        dr["BabyNo"] = item.BabyNo;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                }

                var employeePreviousData = GetEmpPreviousData(items[0].EmpSystemID, items[0].FromDate.ToString("dd-MMM-yyyy"), items[0].ToDate.ToString());

                if (employeePreviousData.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("#Another leave has been already applied.#");
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["SystemId"].ToString();

                _unitOfWork.BeginTransaction();

                LeaveTransactionDetails details = new LeaveTransactionDetails
                {
                    LvTrnsSystemID = MasterId,
                    AddedBy = items[0].AddedBy,
                    DateAdded = DateTime.Now,
                };

                decimal duration = 0.0m;
                var halfDay = false;

                if (items[0].LeaveDayType == "FullDay")
                {
                    duration = 1m;
                    halfDay = false;
                }
                else if (items[0].LeaveDayType == "FirstHalfDay")
                {
                    duration = 0.5m;
                    halfDay = true;
                }
                else if(items[0].LeaveDayType == "SecondHalfDay")
                {
                    duration = 0.5m;
                    halfDay = true;
                }
                else
                {
                    duration = 0.5m;
                    halfDay = false;
                }

                _leaveTransactionDetailsService.InsertGraph(_sandwichVM, _list_H, _list_W, details, items[0].FromDate, Convert.ToDateTime(items[0].ToDate), duration, halfDay);

                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();


                #region Email Sender Service

                if (!string.IsNullOrEmpty(items[0].FirstApprovingAuthority))
                {
                    string mailMessage = "";
                    DataTable dtEmpInfo = _sqlRepository.GetDataTable(@"select * from EmployeeInformation where SystemId = '" + items[0].EmpSystemID + @"'");

                    var dtFmDate = Convert.ToDateTime(items[0].FromDate);
                    var dtToDate = Convert.ToDateTime(items[0].ToDate);

                    TimeSpan difference = dtToDate - dtFmDate;
                    var leaveDays = Convert.ToInt32(difference.Days + 1);
                    items[0].LeaveStatus = LeaveStatus.Pending.ToString();
                    if (items[0].LeaveDayType == "FirstHalfDay" || items[0].LeaveDayType == "SecondHalfDay")
                    {
                        items[0].LeaveDays = 0.5m;
                        if (items[0].LeaveDayType == "FirstHalfDay")
                        {
                            mailMessage = @"Dear " + items[0].ResPersonName + "<br> <br> <br>" +
                            " You have a Leave Approval request of " + dtEmpInfo.Rows[0]["EmployeeName"].ToString() + "(" + dtEmpInfo.Rows[0]["EmployeeCode"].ToString() + ") For First Half Dated On" + items[0].FromDate +
                            ". Please go to the portal for Approving." +
                            "<br> <br> <br>" +
                            "Thank you ";
                        }
                        if (items[0].LeaveDayType == "SecondHalfDay")
                        {
                            mailMessage = @"Dear " + items[0].ResPersonName + "<br> <br> <br>" +
                            " You have a Leave Approval request of " + dtEmpInfo.Rows[0]["EmployeeName"].ToString() + "(" + dtEmpInfo.Rows[0]["EmployeeCode"].ToString() + ") For Second Half Dated On" + items[0].FromDate +
                            ". Please go to the portal for Approving." +
                            "<br> <br> <br>" +
                            "Thank you";
                        }
                    }
                    else
                    {
                        string dt = "";
                        dt = items[0].ToDate != null ? items[0].ToDate.Value.ToString("dd-MMM-yyyy") : "";

                        mailMessage = @"Dear " + items[0].ResPersonName + "<br> <br> <br>" +
                       " You have a Leave Approval request of " + dtEmpInfo.Rows[0]["EmployeeName"].ToString() + "(" + dtEmpInfo.Rows[0]["EmployeeCode"].ToString() + ") For " + leaveDays + " Days, Dated From " + items[0].FromDate + " To " + dt +
                       ". Please go to the portal for Approving." +
                       "<br> <br> <br>" +
                       "Thank you";
                    }
                    _mailSenderService.SendFirstLeaveApproveRequestMail(items[0].FirstApprovingAuthority, items[0].PlantID, mailMessage, items[0].EmailId, items[0].ResPersonName, items[0].EmpSystemID, dtEmpInfo.Rows[0]["EmployeeName"].ToString(), dtEmpInfo.Rows[0]["EmployeeCode"].ToString());
                }
                #endregion
                return MasterId;


            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public void createOffDayList(string plantid, IEnumerable<LeaveData> DataToSave, List<string> listH, List<string> listW)
        {
            List<LeaveData> master = DataToSave.ToList();

            DataSet dsLeavePolicy = null;
            DataSet dsLeavePolicyMaster = null;
            string fromDate = string.Empty;
            string toDate = string.Empty;
            string LVPolicyMasterSystemID = string.Empty;
            try
            {
                List<string> list_W_Total = new List<string>();
                List<string> list_H_Total = new List<string>();
                fromDate = master[0].FromDate.ToString("dd-MMM-yyyy");
                DateTime _dtTD = Convert.ToDateTime(master[0].ToDate);
                toDate = _dtTD.ToString("dd-MMM-yyyy");

                clsOffDayList _clsOffDayList = new clsOffDayList();

                _clsOffDayList._getLeavePolicyMaster(master[0].EmpSystemID, plantid, out dsLeavePolicyMaster);

                if (dsLeavePolicyMaster.Tables[0].Rows.Count > 0)
                {
                    LVPolicyMasterSystemID = dsLeavePolicyMaster.Tables[0].Rows[0]["LeavePolicyMasterId"].ToString();
                }
                else
                {
                    throw new Exception("#Leave policy is not configured...#");
                }

                _clsOffDayList._getLeavePolicy(LVPolicyMasterSystemID, master[0].LTSystemID, out dsLeavePolicy);

                _clsOffDayList._getHolidays(master[0].EmpSystemID, plantid, fromDate, toDate, list_H_Total);

                _clsOffDayList._getWeekOffdate(master[0].EmpSystemID, plantid, fromDate, toDate, list_W_Total);

                clsOffDayListGenerate _odl = new clsOffDayListGenerate(plantid, fromDate, toDate, list_W_Total, list_H_Total);
                _odl.GenerateList(listW, listH);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmp(string EmpId)
        {
            try
            {
                var Sql = @"SELECT EEI.SystemId as EmpId,EEI.EmployeeName as EmpName,EEI.BudgetCode,
               EEI.PlantId,EEI.CompanyId  FROM EmployeeInformation EEI where SystemId='" + EmpId + "'";
                return _sqlRepository.GetDataCollection(Sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> Query(string companyGroupId, string companyId, string plantId, string employeeId)
        {
            try
            {
                var newyear = DateTime.Now.Year;
                var fromDate = "01-Jan-" + newyear;
                var toDate = "31-Dec-" + newyear;
                var sql = @"SELECT LT.SystemID,LT.FromDate,LT.ToDate,LT.IsApproved,LT.LvReason,LT.LeaveDays,
               e.SectionId ,LT.LeaveDayType, L.UserName  AS leaveTypeName,e.EmployeeName,e.EmployeeCode,FORMAT(DOJ,'dd-MMM-yyyy')DOJ,format(DOC,'dd-MMM-yyyy')DOC
                                    FROM [dbo].[LeaveTransaction] AS LT
                                    LEFT JOIN [dbo].[LeaveType] AS L ON L.Id=LT.LTSystemID
                                    LEFT JOIN EmployeeInformation E ON E.SystemId=LT.EmpSystemID
                                    WHERE LT.GroupID='" + companyGroupId + @"' AND E.CompanyId='" + companyId + @"' -- AND LT.PlantID='" + plantId + @"'
                                    AND LT.EmpSystemID='" + employeeId + @"'
                                    AND L.LeaveType<>'Maternity' AND (LT.FromDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
                                    OR LT.ToDate BETWEEN '" + fromDate + @"' AND '" + toDate + "') Order by LT.FromDate DESC";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApprovalList(string plantId, bool isControlAdmin, bool isSysAdmin, string employeeId, string companyId, string FirstApprovingAuthority)
        {
            string strSql = string.Empty;
            try
            {
                var str = "";
                str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                                            where LeaveApproval='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

                strSql = @"SELECT 0 CheckBoxSelect, emp.SystemId EmployeeID,emp.PlantId,emp.CompanyId,emp.GroupID,Lvt.SystemID LvTrnMsID,LT.Code AS LeaveStatus, emp.EmployeeCode,emp.BudgetCode,emp.EmployeeName,emp.EmpType,emp.NationalID,Dsgg.UserName GivenDesignation,E.UserName as Entity,
                             REPLACE(CONVERT(VARCHAR(11), emp.DOJ, 113), ' ', '-') DOJ,
							 LT.UserName LeaveName, LT.Description LeaveDescription,
                             REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate,
                             REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate, LvT.LeaveDays, LvT.LvReason AS Reason, LvT.ComAssignLvSystemID,LVT.LTSystemID,LVT.SystemID LvTransSystemID
                        ,(SELECT YearlyCalendar.Id
                                 FROM YearlyCalendar WHERE LvT.FromDate BETWEEN FromDate AND ToDate AND PlantId='" + plantId + @"' ) CalanderYearID
                             FROM
							 dbo.EmployeeInformation emp
							 LEFT outer JOIN dbo.LeaveTransaction LvT on LvT.EmpSystemID = emp.SystemId
                             LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
							 LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                             LEFT OUTER JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
                             LEFT outer JOIN dbo.LeaveType LT ON LvT.LTSystemID = LT.Id
							 LEFT outer JOIN [HKP].Designation AS Dsg ON Dsg.ID = Emp.DesignationSystemID
							 LEFT outer JOIN [HKP].Designation AS Dsgg ON Dsgg.ID = Emp.GivenDesignationID
                             WHERE  IsNull(Lvt.IsApproved,0) = 0
							 AND ISNULL(LvT.SystemID,'')<> ''
                             AND LvT.IsCancel=0
					         AND FirstApprovingAuthority = '" + FirstApprovingAuthority + @"' AND LvT.FirstApprovingStatus = 0 ";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SaveLeaveReject(IEnumerable<LeaveVM> DataToSave,string Reason)
        {
            try
            {
                clsLeaveApproval objLvTrsEmpWise;
                objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
                foreach (LeaveVM item in DataToSave)
                {
                    LeaveCustomPara obj = new LeaveCustomPara();
                    obj.EmpSystemId = item.EmployeeID;
                    obj.FromDate = Convert.ToDateTime(item.FromDate);
                    obj.ToDate = Convert.ToDateTime(item.ToDate);
                    obj.LvTransSystemID = item.LvTransSystemID;
                    obj.LTSystemID = item.LTSystemID;
                    obj.CalanderYearID = item.CalanderYearID;
                    obj.CancelationReason = Reason;


                    obj.PlantId = item.PlantId;
                    obj.CompanyId = item.CompanyId;
                    obj.GroupId = item.GroupID;
                    obj.UserId = item.UserId;
                    objLvTrsEmpWise.Reject(obj);

                    if (!string.IsNullOrEmpty(item.EmployeeID))
                    {
                        string responsiblePersonName = "";
                        string responsiblePersonId = "";
                        string responsiplePersonEmail = "";
                        string mailMessage = "";
                        DataTable dtEmpInfo = _sqlRepository.GetDataTable(@"SELECT * FROM EmployeeInformation WHERE SystemId = '" + item.EmployeeID + @"'");

                        var dtFmDate = Convert.ToDateTime(item.FromDate);
                        var dtToDate = Convert.ToDateTime(item.ToDate);

                        TimeSpan difference = dtToDate - dtFmDate;
                        var leaveDays = Convert.ToInt32(difference.Days + 1);
                       
                        responsiblePersonName = dtEmpInfo.Rows[0]["EmployeeName"].ToString();
                        responsiblePersonId = dtEmpInfo.Rows[0]["SystemId"].ToString();
                        responsiplePersonEmail = dtEmpInfo.Rows[0]["EmailId"].ToString();
                        string EmpPlant = dtEmpInfo.Rows[0]["PlantId"].ToString();


                        string dt = "";
                        dt = item.ToDate != null ? item.ToDate : "";

                        mailMessage = @"Dear " + responsiblePersonName + "<br> <br> <br>" +
                                            " Your leave request has been rejected for " + leaveDays + " Day(s),  Dated From " + item.FromDate + " To " + dt +
                                            ". Please contact to concern HOD." +
                                            "<br> <br> <br>" +
                                            "Thank you";

                        _mailSenderService.SendFirstLeaveApproveRequestMail(responsiblePersonId, EmpPlant, mailMessage, responsiplePersonEmail, responsiblePersonName, item.EmployeeID, dtEmpInfo.Rows[0]["EmployeeName"].ToString(), dtEmpInfo.Rows[0]["EmployeeCode"].ToString());
                    }
                }
                return "true";                
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string SaveLeaveApproval(IEnumerable<LeaveVM> DataToSave)
        {
            try
            {
                string LvtrnsId = "(' '";
                foreach (LeaveVM item in DataToSave)
                {
                    LvtrnsId += ",'" + item.LvTransSystemID + "'";

                    if (!string.IsNullOrEmpty(item.EmployeeID))
                    {
                        string responsiblePersonName = "";
                        string responsiblePersonId = "";
                        string responsiplePersonEmail = "";
                        string mailMessage = "";
                        DataTable dtEmpInfo = _sqlRepository.GetDataTable(@"SELECT * FROM EmployeeInformation WHERE SystemId = '" + item.EmployeeID + @"'");

                        var dtFmDate = Convert.ToDateTime(item.FromDate);
                        var dtToDate = Convert.ToDateTime(item.ToDate);

                        TimeSpan difference = dtToDate - dtFmDate;
                        var leaveDays = Convert.ToInt32(difference.Days + 1);

                        responsiblePersonName = dtEmpInfo.Rows[0]["EmployeeName"].ToString();
                        responsiblePersonId = dtEmpInfo.Rows[0]["SystemId"].ToString();
                        responsiplePersonEmail = dtEmpInfo.Rows[0]["EmailId"].ToString();
                        string EmpPlant = dtEmpInfo.Rows[0]["PlantId"].ToString();
                   

                        string dt = "";
                        dt = item.ToDate != null ? item.ToDate : "";

                        mailMessage = @"Dear " + responsiblePersonName + "<br> <br> <br>" +
                                            " Your leave request has been Accepted for " + leaveDays + " Day(s),  Dated From " + item.FromDate + " To " + dt +
                                            ". If any discrepancy, Please contact to concern HOD." +
                                            "<br> <br> <br>" +
                                            "Thank you";

                        _mailSenderService.SendFirstLeaveApproveRequestMail(responsiblePersonId, EmpPlant, mailMessage, responsiplePersonEmail, responsiblePersonName, item.EmployeeID, dtEmpInfo.Rows[0]["EmployeeName"].ToString(), dtEmpInfo.Rows[0]["EmployeeCode"].ToString());
                    }
                }
                LvtrnsId += ")";


                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                string strSql = @"Update LeaveTransaction set FirstApprovingStatus = 1,FirstApprovingDate = '" + DateTime.Now + "' where SystemID IN " + LvtrnsId + "";
                connection.executeQuery(strSql);

                connection.CommitTransaction();

                return "true";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }

    public class LeaveData
    {

        #region Scalar Properties

        public string SystemID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string LTSystemID { get; set; }
        public string EmpSystemID { get; set; }
        public string PlantID { get; set; }
        public string GroupID { get; set; }
        public string IsApproved { get; set; }
        public decimal LeaveDays { get; set; }
        public string EmailId { get; set; }
        public string ResPersonName { get; set; }
        public string LvReason { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        public string AddedBy { get; set; }
        public DateTime? DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? AppliedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? CancelationDate { get; set; }
        public DateTime? ExpectedDelivaryDate { get; set; }
        public DateTime? FirstApprovingDate { get; set; }

        #endregion Audit Properties

        #region Navigation

        public string CompanyId { get; set; }
        public string IsPostApplied { get; set; }
        public string IsAdminApproved { get; set; }
        public string CancelationReason { get; set; }
        public string AppliedBy { get; set; }
        public string LeaveDay { get; set; }
        public string LeaveStatus { get; set; }
        public string LeaveDayType { get; set; }
        public string IsCancel { get; set; }
        public string ExceptionLeave { get; set; }
        public string ApprovalPerson { get; set; }
        public string MaternityLeavePolicyId { get; set; }
        public decimal BabyNo { get; set; }
        public string FirstApprovingAuthority { get; set; }
        public string FirstApprovingStatus { get; set; }

        #endregion Navigation

    }
    
    public class LeaveYearDefinationService
    {
        ISqlRepository _sqlRepository;
        public LeaveYearDefinationService()
        {
            _sqlRepository = new SqlRepository();
        }
               
        public IEnumerable<object> GetCbo()
        {
            try
            {
                string TableName = "LeaveYearDefination";
                string sql = "SELECT Id as Value,UserName AS Text FROM " + TableName + "";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }       

        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
              
                string sql = @"select distinct top 100 * from (select ld.Id,c.Id as CompanyId,ld.Sequence,
                ld.Code,ld.ShortName,ld.StandardName,
                ld.UserName,Format(ld.FromDate,'dd-MMM-yyyy')FromDate,Format(ld.ToDate,'dd-MMM-yyyy')ToDate,
                Format(ld.ProcessingDate,'dd-MMM-yyyy')ProcessingDate,
				ld.RespersonId,ld.Remarks,e.EmployeeName 
                as responsiblePerson, STUFF((
                            SELECT ',' + px.UserName
                            FROM dbo.LeaveYearDefinationPlantChild pp
							left join org.Plant px on px.Id=pp.PlantId
                            where pp.LeaveYearDefinationId = ld.Id
                            FOR XML PATH('')
                            ),1,1,'') AS Plants,
							STUFF((
                            SELECT ',' + pp.PlantId
                            FROM dbo.LeaveYearDefinationPlantChild pp
							where pp.LeaveYearDefinationId = ld.Id
                            FOR XML PATH('')
                            ),1,1,'') AS PlantIds
                from dbo.LeaveYearDefination ld left join LeaveYearDefinationPlantChild 
				ldp on ldp.LeaveYearDefinationId=ld.Id
				left join org.Plant p on p.Id=ldp.PlantId
				left join org.Company c on c.Id=p.CompanyId              
				left join EmployeeInformation e on 
				e.SystemId=ld.RespersonId
                ) AS TEMP WHERE " + strkey+ "  order by sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }       
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmps(string CompId)
        {
            try
            {
                var str = @"Select * from EmployeeInformation where
                EmployeeStatus='Active' and CompanyId='"+CompId+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Create(Dictionary<string, object> data,List<string> DataList)
        {
            try
            {
                string TableName = "LeaveYearDefination";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where FromDate='" + data["FromDate"] + "' AND  Id<>'" + data["Id"] + "'AND ToDate='" + data["ToDate"]+"'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same From & To Date already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "LY" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsChild;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from dbo.LeaveYearDefinationPlantChild where LeaveYearDefinationId ='" + MasterId + "'", out dsChild, false, "1");

                while (dsChild.Tables[0].DefaultView.Count > 0)
                {
                    dsChild.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < DataList.Count; i++)
                {
                    DataRow dr = dsChild.Tables[0].NewRow();
                    dr["Id"] = MasterId + i.ToString();
                    dr["LeaveYearDefinationId"] =MasterId.ToString();
                    dr["PlantId"] = DataList[i].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsChild.Tables[0].Rows.Add(dr);
                }
                _info.SaveDataSets(dsChild);
                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }


        public string Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from LeaveYearDefinationPlantChild where LeaveYearDefinationId ='" + id + "'");
                con.executeQuery("delete from LeaveYearDefination where id='" + id + "'");
                con.CommitTransaction();
                return "Success";

            }
            catch (Exception ex)
            {
                return ex.Message;
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
            dr["AddedDate"] = DateTime.Now.ToString();
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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

    }
   
    public class LeaveOpeningUploadService
    {
        ISqlRepository _sqlRepository;
        public LeaveOpeningUploadService()
        {
            _sqlRepository = new SqlRepository();
        }

        public DataTable getSampleFile(string PlantId,string LvId)
        {
            try
            {
                var sql = @"select distinct e.SystemId as EmpId,e.EmployeeCode,ld.Id as 
                LeaveYearId,ld.UserName as LeaveYear,p.UserName as Plant,
                lt.UserName as LeaveType,lt.Id as LeaveTypeId
                ,isnull(ad.Earned,'0')Earned,
                isnull(ad.RegularEncashment,'0')RegularEncashment,
                isnull(ad.Availed,'0')Availed,isnull(ad.Adjustment,'0')Adjustment
                from LeaveYearDefination ld 
                left join LeaveYearDefinationPlantChild pc on 
				pc.LeaveYearDefinationId=ld.Id and pc.PlantId='"+PlantId+@"'
                left join org.Plant p on p.Id=pc.PlantId
				left join org.Company c on c.Id=p.CompanyId
                left join org.CompanyGroup cg on cg.Id=c.CompanyGroupId
                left join LeaveType lt on lt.CompanyGroupId=cg.Id 
                left join EmployeeInformation e on e.PlantId=p.Id
                left join ManualLeaveData ad on ad.EmployeeId=e.SystemId
				and ad.LeaveYearId=ld.Id and 
				ad.LeaveTypeId=lt.Id and ad.PlantId='"+PlantId+@"'
                where p.Id='"+PlantId+"' and ld.Id='"+LvId+@"' and
                e.EmployeeStatus='Active' order by e.SystemId,lt.Id";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getCompany()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select Username as Text , Id as Value from ORG.Company 
                where companygroupid='"+identity.CompanyGroupId+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getPlants(string cmp)
        {
            try
            {
                var str = @"Select Username as Text , Id as Value from ORG.Plant where CompanyId = '" + cmp + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> getLeaveYear(string PlantId)
        {
            try
            {
                var str = @"select ld.Username as Text ,ld.Id as Value,format(ld.fromdate,'dd-MMM-yyyy')FromDate,format(ld.Todate,'dd-MMM-yyyy')ToDate from 
                leaveyeardefination ld 
				left join LeaveYearDefinationPlantChild lpc on lpc.LeaveYearDefinationId=ld.Id
                where lpc.plantid='" + PlantId+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getCurrentList(string plantId,string YearId)
        {
            try
            {
                var str = @"Select a.*,lt.LeaveType from dbo.ManualLeaveData a 
                left join LeaveYearDefination ld on ld.Id=a.LeaveYearId
                left join LeaveType lt on lt.Id=a.LeaveTypeId
                where a.PlantId = '" + plantId+"' and a.LeaveYearId='"+ YearId+"'";
                return (_sqlRepository.GetDataCollection(str));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SaveFileList(List<Dictionary<string, object>> data, string PlantId,string YearId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = DateTime.Now.ToString();
                string TableName = "dbo.ManualLeaveData";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1 = 2", out dsMaster, false, "1");

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    int index = 0;
                    for (int i = 0; i < data.Count; i++)
                    {
                        Dictionary<string, object> jj = data[i];                        
                        jj["Id"] = "ML"+_Id + index;
                        index++;
                        AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate,PlantId);
                    }
                }

                //var sqls = @"Delete from dbo.ManualLeaveData                                 
                //                where plantId = '" + PlantId + @"' and LeaveYearId='"+YearId+"'";

                //ConnectionManager.DAL.ConManager objCone = null;
                //objCone = new ConnectionManager.DAL.ConManager("1");
                //objCone.OpenConnection("1");
                //objCone.BeginTransaction();

                //objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                //objCone.CommitTransaction();

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData, string addedname, string addeddate,string PlantId)
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
           
            dr["PlantId"] = PlantId;
            dr["AddedBy"] = addedname;
            dr["AddedDate"] = addeddate;
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }

    }

    public class AnnualLeaveProcessingService
    {
        ISqlRepository _sqlRepository;
        public AnnualLeaveProcessingService()
        {
            _sqlRepository = new SqlRepository();
        }
        
        public IEnumerable<object> GetEmpCategory()
        {
            try
            {
                var str = @"select Id as Value,UserName AS Text from 
                hkp.EmployeeCategory";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public IEnumerable<object> GetLeaveType()
        {
            try
            {
                var str = @"select Id as Value,UserName AS Text from LeaveType";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
              
        public IEnumerable<object> GetNewLvYear(string PlantId,string PrevLvYear)
        {
            try
            {
                var str = @"select ld.Id as Value,ld.UserName AS Text from 
                LeaveYearDefination ld left join 
                LeaveYearDefinationPlantChild lc on lc.LeaveYearDefinationId=ld.Id
                where lc.PlantId='" + PlantId + "' and ld.Id<>'"+PrevLvYear+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public IEnumerable<object> LoadData(string PlantId, string LvYearId, List<string> DataList,List<string> EmpCategoryList)
        {
            try
            {
                #region Supporting Variables Finding 

                DataTable DateTbl;
                var str = @"select FromDate,ToDate from LeaveYearDefination where id='"+LvYearId+"'";
                DateTbl=_sqlRepository.GetDataTable(str);
                string From = "", To = "";
                if(DateTbl.Rows.Count>0)
                {
                   From= DateTbl.Rows[0]["FromDate"].ToString();
                   To = DateTbl.Rows[0]["ToDate"].ToString();
                }
                string LTypeId = "''", EmpCategoryId = "''";

                for(int i = 0; i < DataList.Count; i++)
                {
                    LTypeId += ",'" + DataList[i].ToString() + "'";
                }               

                for (int i = 0; i < EmpCategoryList.Count; i++)
                {
                    EmpCategoryId += ",'" + EmpCategoryList[i].ToString() + "'";
                }

                #endregion

                #region DataSet Finding

                var sql = @"select xx.*,ROUND((xx.opening+ xx.Earned-xx.Availed-xx.RegularEncashment+xx.PAdjustment),0)as Closing 
                    
			    from (select dd.*,
				ROUND(case when lpd.EncashWorkingDaysQty >0 then dd.EarningDays/lpd.EncashWorkingDaysQty else 0 END,0) as Earned,ISNULL(ame.PAdjustment,0)PAdjustment		
                from (select e.SystemId as EmpId,e.EmployeeCode,ld.Id as 
                LeaveYearId,ld.UserName as LeaveYear,p.UserName as Plant,
                lt.UserName as LeaveType,lt.Id as LeaveTypeId,lt.Code
                ,isnull(ac.Opening,'0')Opening,isnull(Masterx.EarnDays,'0')+ isnull(md.Earned,'0')EarningDays,
                (isnull(md.RegularEncashment,'0')+ISNULL(ac.RegularEncashment,'0')) 
				RegularEncashment,
				Availed= (isnull(Info.AvailedLeave,'0')+isnull(md.Availed,'0')),
			    (isnull(md.Adjustment,'0') +isnull(ac.Adjustment,'0'))Adjustment,				
                ISNULL(Info.EmpTypeId,EDM.EmployeeCategoryId)EmpTypeId,ISNULL(Info.LeavePolicyMasterId,edmc.LeavePolicyMasterId)LeavePolicyMasterId
                from LeaveYearDefination ld 
                left join LeaveYearDefinationPlantChild pc on 
				pc.LeaveYearDefinationId=ld.Id and pc.PlantId='" + PlantId+ @"'
                left join org.Plant p on p.Id=pc.PlantId
				left join org.Company c on c.Id=p.CompanyId
                left join org.CompanyGroup cg on cg.Id=c.CompanyGroupId
                left join LeaveType lt on lt.CompanyGroupId=cg.Id 
                left join EmployeeInformation e on e.PlantId=p.Id
                left join ManualLeaveData md on md.EmployeeId=e.SystemId and md.LeaveYearId=ld.Id and 
				md.LeaveTypeId=lt.Id and md.PlantId='" + PlantId + @"'
                --left join (Select m.EmployeeId,m.Adjustment,m.RegularEncashment,m.Earned,m.Availed from ManualLeaveData m Where m.Id =
				--(Select top(1) d.Id from  ManualLeaveData d
				--JOIN LeaveType T ON t.Id=d.LeaveTypeId AND t.LeaveType='Earn' Where d.EmployeeId=m.EmployeeId order by d.addeddate desc
				--)) md on md.EmployeeId=e.SystemId
                left join AnnualLeaveDataCurrent ac on ac.EmployeeId=e.SystemId
				and ac.LeaveYearId=ld.Id and ac.LeaveTypeId=lt.Id and ac.PlantId='" + PlantId+ @"'
								left join
				(
select distinct a.EmpSystemID,LT.AvailedLeave---,A.DayStatus
,a.PlantID,dc.EmpTypeId,
				dxc.LeavePolicyMasterId
				from AttdnProcessData a 
				left join EmployeeInformation ei on a.EmpSystemID=ei.SystemId
				LEFT JOIN(
				Select Sum(LTD.LeaveDuration) AvailedLeave,LT.EmpSystemID,LT.LTSystemID from LeaveTransaction LT
				Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
				Where WorkDate between '" + From + @"' and '" + To + @"' AND LTSystemID in (" + LTypeId + @")
				group by LT.EmpSystemID,LT.LTSystemID
				) LT ON LT.EmpSystemID=a.EmpSystemID 
				left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = ei.LegalDesignationId
				left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
				left join scs.DesignationMasterConfiguration dxc on dxc.DesignationMasterId=dm.Id and dxc.PlantId=ei.PlantId
				left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId and dc.PlantId=ei.PlantId
				left join DayStatusHeader dh on dh.Id=dc.headerId
				left join DayTypeWithValues dt on dt.HeaderId=dh.Id and dt.DayType=a.DayStatus				
				where dt.HeaderId is not null --and a.LvValue<>0 
				and ei.EmployeeStatus='Active' AND A.LTSystemID in (" + LTypeId + @")
				and a.workdate between '" + From + @"' and '" + To + @"' and ei.PlantId='" + PlantId + @"'
				) as Info
				on Info.EmpSystemID=e.SystemId and Info.PlantID=e.PlantId --and Info.DayStatus=lt.Code
                left join (SELECT EmpSystemID,SUM(l.EarnValue)EarnDays,T.Id as LeaveId,ei.PlantId
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + From+@"' and '"+To+@"'
                and EI.PlantID='"+PlantId+ @"' and t.LeaveType='Earn'
                group by EmpSystemID,t.Id,ei.plantid
                ) as Masterx on Masterx.EmpSystemID=e.SystemId and e.PlantId=Masterx.PlantId and Masterx.LeaveId=lt.Id       
				LEFT JOIN mst.DesignationMaster EDM ON EDM.DesignationId=E.GivenDesignationId
				LEFT JOIN scs.DesignationMasterConfiguration AS edmc ON edmc.DesignationMasterId=EDM.Id AND edmc.PlantId=e.PlantId
                where p.Id='" + PlantId+@"' and ld.Id='"+LvYearId+@"' and
				lt.Id in ("+LTypeId+ @") and
                e.EmployeeStatus='Active') as dd
				left join LeavePolicyDetail lpd on lpd.LPMSystemID=dd.LeavePolicyMasterId and lpd.LTSystemID=dd.LeaveTypeId
                left join (Select m.EmployeeId,m.Adjustment PAdjustment from ManualLeaveData m Where m.Id =
				(Select top(1) md.Id from  ManualLeaveData md
				JOIN LeaveType T ON t.Id=md.LeaveTypeId AND t.LeaveType='Earn' Where md.EmployeeId=m.EmployeeId order by md.addeddate desc
				)) ame on ame.EmployeeId=dd.EmpId
			    where dd.EmpTypeId In(" + EmpCategoryId+@")
				) as xx
				order by xx.EmpId,xx.LeaveTypeId";
                return _sqlRepository.GetDataCollection(sql);

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetOldData(string PlantId, string LvId,string LeaveTypeId)
        {
            try
            {
                var sql = @"select EmployeeId,LeaveYearId,LeaveTypeId,PlantId,Closing CarryForward
				  from AnnualLeaveDataPast where
                  LeaveYearId='" + LvId+"' and PlantId='"+PlantId+"'and LeaveTypeId in("+LeaveTypeId+")";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ProcessData(string Data, string PlantId, string CurrentLvYearId,decimal MaxCarryForward,
            decimal MaxEncash,decimal MaxLapse,string NewLeaveYear, List<string> LeaveTypeList)
        {
            try
            {
                
                #region Annual Leave Data Past Processing
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsStaticInfo info = new clsStaticInfo();

                string LTypeId = "''";

                for (int i = 0; i < LeaveTypeList.Count; i++)
                {
                    LTypeId += ",'" + LeaveTypeList[i].ToString() + "'";
                }

                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                var sqlx = @"select * from AnnualLeaveDataPast where PlantId='" + PlantId + "' and LeaveYearId='"+CurrentLvYearId+"'";
                objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                var sqly = @"select * from LeaveEncashmentHistory where
                    LeaveYearId='" + CurrentLvYearId + "' and PlantId='" + PlantId + "'and LeaveTypeId in(" + LTypeId + ")";
                objCon.OpenDataSetThroughAdapter(sqly, out DataSet dsSave, false, false, "", "1");


                List<Dictionary<string, object>> _objects = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Data);
                DataTable Table = ToDataTable(_objects);
                if (Table.Rows.Count > 0)
                {
                    for (int i = 0; i < Table.Rows.Count; i++)
                    {
                        string EmpId = Table.Rows[i][@"EmpId"].ToString();
                        string LeaveTypeId = Table.Rows[i][@"LeaveTypeId"].ToString();
                        decimal Opening = Convert.ToDecimal(Table.Rows[i][@"Opening"].ToString());
                        decimal Earned = Convert.ToDecimal(Table.Rows[i][@"Earned"].ToString());
                        decimal Availed = Convert.ToDecimal(Table.Rows[i][@"Availed"].ToString());
                        decimal Adjustment = Convert.ToDecimal(Table.Rows[i][@"Adjustment"].ToString());
                        decimal RegularEncashment = Convert.ToDecimal(Table.Rows[i][@"RegularEncashment"].ToString());
                        decimal Closing = Convert.ToDecimal(Table.Rows[i][@"Closing"].ToString());
                        decimal Carryforward = 0;
                        if (Closing > MaxCarryForward)
                        {
                            Carryforward = MaxCarryForward;
                        }
                        else
                        {
                            Carryforward = Closing;
                        }

                        decimal Balance = Closing - Carryforward;
                        decimal AnnualEncash = 0;
                        if (Balance > MaxEncash)
                        {
                            AnnualEncash = MaxEncash;
                        }
                        else
                        {
                            AnnualEncash = Balance;
                        }

                        decimal LapseBalance = Balance - AnnualEncash;
                        decimal ResultingLapse = 0;
                        if (LapseBalance > MaxLapse)
                        {
                            ResultingLapse = MaxLapse;
                        }
                        else
                        {
                            ResultingLapse = LapseBalance;
                        }


                        clsGenID genid = new clsGenID();
                        genid.GenID("AnnualLeaveDataPast", out string _Id);
                        genid.GenID("LeaveEncashmentHistory", out string _Idy);

                        #region For Past Table

                        dsRef.Tables[0].DefaultView.RowFilter = @"EmployeeId='" + EmpId + "' AND LeaveTypeId='" + LeaveTypeId + "'";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["Opening"] = Opening;
                            dr["Earned"] = Earned;
                            dr["Availed"] = Availed;
                            dr["RegularEncashment"] = RegularEncashment;
                            dr["Adjustment"] = Adjustment;
                            dr["Closing"] = Closing;
                            dr["CarryForward"] = Carryforward;
                            dr["AnnualEncashment"] = AnnualEncash;
                            dr["Lapse"] = ResultingLapse;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = Convert.ToDateTime(DateTime.Now);
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();
                        }
                        else
                        {
                            DataRow dr = dsRef.Tables[0].NewRow();
                            dr["Id"] = "AP" + _Id + "-" + i;
                            dr["EmployeeId"] = EmpId;
                            dr["LeaveYearId"] = CurrentLvYearId;
                            dr["PlantId"] = PlantId;
                            dr["LeaveTypeId"] = LeaveTypeId;
                            dr["Opening"] = Opening;
                            dr["Earned"] = Earned;
                            dr["Availed"] = Availed;
                            dr["RegularEncashment"] = RegularEncashment;
                            dr["Adjustment"] = Adjustment;
                            dr["Closing"] = Closing;
                            dr["CarryForward"] = Carryforward;
                            dr["AnnualEncashment"] = AnnualEncash;
                            dr["Lapse"] = ResultingLapse;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = Convert.ToDateTime(DateTime.Now);
                            dr["AddedFromIP"] = identity.IPAddress;
                            dsRef.Tables[0].Rows.Add(dr);
                        }

                        #endregion

                        #region For Encashment Table

                        dsSave.Tables[0].DefaultView.RowFilter = @"EmployeeId='" + EmpId + "' " +
                            "AND LeaveTypeId='" + LeaveTypeId + "'";
                        if (dsSave.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dry = dsSave.Tables[0].DefaultView[0].Row;
                            dry.BeginEdit();
                            dry["AnnualEncashedLv"] = AnnualEncash;
                            dry["UpdatedBy"] = identity.Name;
                            dry["UpdatedDate"] = Convert.ToDateTime(DateTime.Now);
                            dry["UpdatedFromIP"] = identity.IPAddress;
                            dry.EndEdit();
                        }
                        else
                        {
                            DataRow dry = dsSave.Tables[0].NewRow();
                            dry["Id"] = "LH" + _Idy + "-" + i;
                            dry["EmployeeId"] = EmpId;
                            dry["LeaveYearId"] = CurrentLvYearId;
                            dry["PlantId"] = PlantId;
                            dry["LeaveTypeId"] = LeaveTypeId;
                            dry["AnnualEncashedLv"] = AnnualEncash;
                            dry["AddedBy"] = identity.Name;
                            dry["AddedDate"] = Convert.ToDateTime(DateTime.Now);
                            dry["AddedFromIP"] = identity.IPAddress;
                            dsSave.Tables[0].Rows.Add(dry);
                        }
                       
                        #endregion
                    }

                    info.SaveDataSets(dsRef,dsSave);
                }
                #endregion

                #region Current Table Processing  
                
               
                var sql = @"select * from AnnualLeaveDataCurrent where
                    LeaveYearId='"+NewLeaveYear+"' and PlantId='"+PlantId+"'and LeaveTypeId in("+LTypeId+")";
                    objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, false, "", "1");

                DataTable SourceData = GetOldData(PlantId, CurrentLvYearId, LTypeId);
                if(SourceData.Rows.Count>0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID("AnnualLeaveDataCurrent", out string _Idx);

                    for (int x = 0; x < SourceData.Rows.Count; x++)
                    {
                        string EmpId = SourceData.Rows[x][@"EmployeeId"].ToString();
                        string LeaveTypeId = SourceData.Rows[x][@"LeaveTypeId"].ToString();
                        string LeaveYearId = SourceData.Rows[x][@"LeaveYearId"].ToString();
                        decimal Opening_for_NewYear = Convert.ToDecimal(SourceData.Rows[x][@"CarryForward"].ToString());

                        dsMaster.Tables[0].DefaultView.RowFilter = @"EmployeeId='" + EmpId + "' AND LeaveTypeId='" + LeaveTypeId + "'";
                        if (dsMaster.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow drx = dsMaster.Tables[0].DefaultView[0].Row;
                            drx.BeginEdit();
                            drx["Opening"] = Opening_for_NewYear;
                            drx["UpdatedBy"] = identity.Name;
                            drx["UpdatedDate"] = Convert.ToDateTime(DateTime.Now);
                            drx["UpdatedFromIP"] = identity.IPAddress;
                            drx.EndEdit();
                        }
                        else
                        {
                            DataRow drx = dsMaster.Tables[0].NewRow();
                            drx["Id"] = "AC" + _Idx + "-" + x;
                            drx["EmployeeId"] = EmpId;
                            drx["LeaveYearId"] = NewLeaveYear;
                            drx["PlantId"] = PlantId;
                            drx["LeaveTypeId"] = LeaveTypeId;
                            drx["Opening"] = Opening_for_NewYear;
                            drx["AddedBy"] = identity.Name;
                            drx["AddedDate"] = Convert.ToDateTime(DateTime.Now);
                            drx["AddedFromIP"] = identity.IPAddress;
                            dsMaster.Tables[0].Rows.Add(drx);
                        }
                    }
                    info.SaveDataSets(dsMaster);
                }
                #endregion

               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static DataTable ToDataTable(List<Dictionary<string, object>> list)
        {
            DataTable result = new DataTable();
            if (list.Count == 0)
                return result;

            result.Columns.AddRange(
                list.First().Select(r => new DataColumn(r.Key)).ToArray()
            );

            list.ForEach(r => result.Rows.Add(r.Select(c => c.Value).Cast<object>().ToArray()));

            return result;
        }

        public IEnumerable<object> GetEmpYearEarnAvailData(string fromdate, string todate,string empId)
        {
            try
            {
                string sql = @"SELECT ei.EmployeeCode,APD.EmpSystemID,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(ei.DOS,'dd-MMM-yyyy')DOS,FORMAT(apd.WorkDate,'dd-MMM-yyyy')[Date]
,apd.DayStatus,apd.LTSystemID LeaveId,T.LeaveType
,ds.TotalWorkingDay,ds.ActualWorkingDay,ds.PayDay,ds.NonPayDay,ds.PresentValuePD,ds.LeaveValueLP,ds.AbsentValueAB,ds.WeeklyOffWO,ds.HolidayH
,ds.Other,ds.Other,ds.OTApplicable,ds.CompensatoryApplicable,ds.GoodWorkApplicable,ds.SandwichStatusFlag,ds.ToAudit
,l.EarnValue,l.AvailedValue
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId AND T.LeaveType='Earn'
                where apd.workdate between '" + fromdate + @"' and '"+todate+ @"' and APD.EmpSystemID='" + empId + @"' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public List<Dictionary<string, object>> GetEmpYearEarnAvailSummaryData(string fromdate, string todate, string PlantID)
        {
            try
            {
                string sql = @"SELECT ei.EmployeeCode,APD.EmpSystemID,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(ei.DOS,'dd-MMM-yyyy')DOS,T.LeaveType,COUNT(apd.DayStatus)DayStatus,SUM(l.EarnValue)EarnValue,SUM(l.AvailedValue)AvailedValue
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId AND T.LeaveType='Earn'
                where apd.workdate between '" + fromdate + @"' and '" + todate + @"' and EI.PlantID='"+ PlantID + @"' 
                group by EmpSystemID,t.Id,ei.plantid,ei.EmployeeCode,ei.EmployeeName,ei.DOJ,EI.DOS,T.LeaveType";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetEmployeeSingleData(string fromdate, string todate, string empId)
        {
            try
            {
                string sql = @"SELECT ei.EmployeeCode,APD.EmpSystemID,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(ei.DOS,'dd-MMM-yyyy')DOS,FORMAT(apd.WorkDate,'dd-MMM-yyyy')[Date]
				,apd.DayStatus,apd.LTSystemID LeaveId,T.LeaveType,ds.TotalWorkingDay,ds.ActualWorkingDay,ds.PayDay,ds.NonPayDay,ds.PresentValuePD,ds.LeaveValueLP
				,ds.AbsentValueAB,ds.WeeklyOffWO,ds.HolidayH,ds.Other,ds.Other,ds.OTApplicable,ds.CompensatoryApplicable,ds.GoodWorkApplicable,ds.SandwichStatusFlag
				,ds.ToAudit,l.EarnValue,l.AvailedValue
				,D.UserName GivenDesignation,SS.UserName SubSection,S.UserName Section,Dep.UserName Department,apd.ProcessIntime,apd.ProcessOuttime,apd.Duration
				,apd.OTHr,apd.WorkingDayValue,apd.ActualWorkingDayValue,apd.PresentValue,apd.AbsentValue,T.Code Leave
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                LEFT JOIN HKP.Designation D on D.Id=ei.GivenDesignationId
                LEFT JOIN ORG.SubSection SS on SS.Id=ei.SubSectionId
                LEFT JOIN ORG.Section S on S.Id=ei.SectionId
                LEFT JOIN ORG.Department Dep on Dep.Id=ei.DepartmentId
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + fromdate + @"' and '" + todate + @"' and APD.EmpSystemID='" + empId + @"' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string EmployeeAttendanceReport(DataTable data, string ReportHeader, string reportFileName)
        {
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
                workbook.Worksheets[0].Name = "Employee Attendance Data";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEmployeeCode = COL;
                COL++;

                //sheet[ROW, COL].Text = "Emp SystemID";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int colEmpSystemID = COL;
                //COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "DOS";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDOS = COL;
                COL++;

                sheet[ROW, COL].Text = "Given Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                int colGivenDesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSubSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 41;
                int colDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Process Intime";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcessIntime = COL;
                COL++;

                sheet[ROW, COL].Text = "Process Outtime";
                sheet[ROW, COL].ColumnWidth = 28;
                int colProcessOuttime = COL;
                COL++;

                sheet[ROW, COL].Text = "Duration";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDuration = COL;
                COL++;

                sheet[ROW, COL].Text = "OT Hr";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colOTHr = COL;
                COL++;
                        
                sheet[ROW, COL].Text = "Day Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDayStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Working Day Value";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWorkingDayValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Actual Working Day Value";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualWorkingDayValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Present Value";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPresentValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Absent Value";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAbsentValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Leave";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLeave = COL;
                COL++;

                sheet[ROW, COL].Text = "Leave Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLeaveType = COL;
                COL++;

                sheet[ROW, COL].Text = "Earn Value";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEarnValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Availed Value";
                sheet[ROW, COL].ColumnWidth = 16;
                int colAvailedValue = COL;
                
                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, colEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                    //sheet[ROW, colEmpSystemID].Text = data.Rows[i]["EmpSystemID"].ToString();
                    sheet[ROW, colDOJ].Text = data.Rows[i]["DOJ"].ToString();
                    sheet[ROW, colDOS].Text = data.Rows[i]["DOS"].ToString();
                    sheet[ROW, colGivenDesignation].Text = data.Rows[i]["GivenDesignation"].ToString();
                    sheet[ROW, colSubSection].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, colSection].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, colDepartment].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, colDate].Text = data.Rows[i]["Date"].ToString();
                    sheet[ROW, colProcessIntime].Text = data.Rows[i]["ProcessIntime"].ToString();
                    sheet[ROW, colProcessOuttime].Text = data.Rows[i]["ProcessOuttime"].ToString();
                    sheet[ROW, colDuration].Number = clsStaticInfo.dbl(data.Rows[i]["Duration"].ToString());
                    sheet[ROW, colOTHr].Number = clsStaticInfo.dbl(data.Rows[i]["OTHr"].ToString());
                    sheet[ROW, colDayStatus].Text = data.Rows[i]["DayStatus"].ToString();
                    sheet[ROW, colWorkingDayValue].Number = clsStaticInfo.dbl(data.Rows[i]["WorkingDayValue"].ToString());
                    sheet[ROW, colActualWorkingDayValue].Number = clsStaticInfo.dbl(data.Rows[i]["ActualWorkingDayValue"].ToString());

                    sheet[ROW, colPresentValue].Number = clsStaticInfo.dbl(data.Rows[i]["PresentValue"].ToString());
                    sheet[ROW, colAbsentValue].Number = clsStaticInfo.dbl(data.Rows[i]["AbsentValue"].ToString());
                    sheet[ROW, colLeave].Text = data.Rows[i]["Leave"].ToString();
                    sheet[ROW, colLeaveType].Text = data.Rows[i]["LeaveType"].ToString();
                    sheet[ROW, colEarnValue].Number = clsStaticInfo.dbl(data.Rows[i]["EarnValue"].ToString());
                    sheet[ROW, colAvailedValue].Number = clsStaticInfo.dbl(data.Rows[i]["AvailedValue"].ToString());

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Employee Attendance Data Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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

        public IEnumerable<object> GetEmployeeSummaryData(string fromdate, string todate,string empId)
        {
            try
            {
                string sql = @"SELECT ei.EmployeeCode,APD.EmpSystemID,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy')DOJ,ISNULL(FORMAT(ei.DOS,'dd-MMM-yyyy'),'')DOS
				,apd.DayStatus,apd.LTSystemID LeaveId,T.LeaveType,ds.TotalWorkingDay,ds.ActualWorkingDay
				,l.EarnValue,l.AvailedValue,D.UserName GivenDesignation,SS.UserName SubSection,S.UserName Section,Dep.UserName Department
                ,ISNULL(apd.OTHr,0)OTHr,ISNULL(apd.WorkingDayValue,0)WorkingDayValue
				,ISNULL(apd.ActualWorkingDayValue,0)ActualWorkingDayValue,ISNULL(apd.PresentValue,0)PresentValue,ISNULL(apd.AbsentValue,0)AbsentValue,ISNULL(T.Code,'') Leave

				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                LEFT JOIN HKP.Designation D on D.Id=ei.GivenDesignationId
                LEFT JOIN ORG.SubSection SS on SS.Id=ei.SubSectionId
                LEFT JOIN ORG.Section S on S.Id=ei.SectionId
                LEFT JOIN ORG.Department Dep on Dep.Id=ei.DepartmentId
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + fromdate + @"' and '" + todate + @"'
                and APD.EmpSystemID "+empId+" ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IEnumerable<object> GetEmployeeList(string fromdate, string todate)
        {
            try
            {
                string CmdText = @"SELECT ei.EmployeeCode,APD.EmpSystemID,ei.EmployeeName,FORMAT(ei.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(ei.DOS,'dd-MMM-yyyy')DOS,D.UserName GivenDesignation
				,SS.UserName SubSection,S.UserName Section,Dep.UserName Department,apd.OTHr,apd.DayStatus
				,apd.WorkingDayValue,apd.ActualWorkingDayValue,apd.PresentValue,apd.AbsentValue,T.Code Leave,T.LeaveType,l.EarnValue,l.AvailedValue
				
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                LEFT JOIN HKP.Designation D on D.Id=ei.GivenDesignationId
                LEFT JOIN ORG.SubSection SS on SS.Id=ei.SubSectionId
                LEFT JOIN ORG.Section S on S.Id=ei.SectionId
                LEFT JOIN ORG.Department Dep on Dep.Id=ei.DepartmentId
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                 where apd.workdate between '" + fromdate + @"' and '" + todate + @"'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string EmployeeAttendanceSummaryReport(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
        {
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
                workbook.Worksheets[0].Name = "Daily Planning & Production Report";
                sheet = workbook.Worksheets[0];
                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEmployeeCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "DOS";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDOS = COL;
                COL++;

                sheet[ROW, COL].Text = "Given Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                int colGivenDesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSubSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSection = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 41;
                int colDepartment = COL;
                COL++;

                sheet[ROW, COL].Text = "OTHr";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colOTHr = COL;
                COL++;

                sheet[ROW, COL].Text = "Day Status";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDayStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Working Day Value";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colWorkingDayValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Actual Working Day Value";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colActualWorkingDayValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Present Value";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPresentValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Absent Value";
                sheet[ROW, COL].ColumnWidth = 16;
                int colAbsentValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Leave";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLeave = COL;
                COL++;

                sheet[ROW, COL].Text = "Leave Type";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLeaveType = COL;
                COL++;

                sheet[ROW, COL].Text = "Earn Value";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEarnValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Availed Value";
                sheet[ROW, COL].ColumnWidth = 16;
                int colAvailedValue = COL;
                

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                int LastRow = ROW + (data.Count - 1);

                for (int i = 0; i < data.Count; i++)
                {
                    sheet[ROW, colEmployeeCode].Text = data[i]["EmployeeCode"].ToString();
                    sheet[ROW, colEmployeeName].Text = data[i]["EmployeeName"].ToString();
                    sheet[ROW, colDOJ].Text = data[i]["DOJ"].ToString();
                    sheet[ROW, colDOS].Text = data[i]["DOS"].ToString();
                    sheet[ROW, colGivenDesignation].Text = data[i]["GivenDesignation"].ToString();
                    sheet[ROW, colSubSection].Text = data[i]["SubSection"].ToString();
                    sheet[ROW, colSection].Text = data[i]["Section"].ToString();
                    sheet[ROW, colDepartment].Text = data[i]["Department"].ToString();
                    sheet[ROW, colOTHr].Number = clsStaticInfo.dbl(data[i]["OTHr"].ToString());
                    sheet[ROW, colDayStatus].Text = data[i]["DayStatus"].ToString();
                    sheet[ROW, colWorkingDayValue].Number = clsStaticInfo.dbl(data[i]["WorkingDayValue"].ToString());
                    sheet[ROW, colActualWorkingDayValue].Number = clsStaticInfo.dbl(data[i]["ActualWorkingDayValue"].ToString());
                    sheet[ROW, colPresentValue].Number = clsStaticInfo.dbl(data[i]["PresentValue"].ToString());
                    sheet[ROW, colPresentValue].Number = clsStaticInfo.dbl(data[i]["PresentValue"].ToString());
                    sheet[ROW, colAbsentValue].Number = clsStaticInfo.dbl(data[i]["AbsentValue"].ToString());
                    sheet[ROW, colLeaveType].Text = data[i]["LeaveType"].ToString();

                    sheet[ROW, colEarnValue].Number = clsStaticInfo.dbl(data[i]["EarnValue"].ToString());
                    sheet[ROW, colAvailedValue].Number = clsStaticInfo.dbl(data[i]["AvailedValue"].ToString());
                   
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Employee Attendance Summary Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

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

    }

    public class RegularEncashmentService
    {
        ISqlRepository _sqlRepository;
        public RegularEncashmentService()
        {
            _sqlRepository = new SqlRepository();
        }
        static DataTable ToDataTable(List<Dictionary<string, object>> list)
        {
            DataTable result = new DataTable();
            if (list.Count == 0)
                return result;

            result.Columns.AddRange(
                list.First().Select(r => new DataColumn(r.Key)).ToArray()
            );

            list.ForEach(r => result.Rows.Add(r.Select(c => c.Value).Cast<object>().ToArray()));

            return result;
        }
       
        public IEnumerable<object> GetEmpInfo(string PlantId,string From,string To,string Year)
        {
            try
            {
                var str = @"select e.EmployeeCode,e.SystemId as EmpId,e.EmployeeName,
                s.UserName as Section,
                d.UserName as Department,leh.LeaveTypeId,leh.LeaveTypeId,lt.UserName as LeaveType,
                ss.UserName as SubSection
                from employeeinformation e left join org.Department d on d.Id=e.DepartmentId
                left join org.Section s on s.Id=e.SectionId
                left join org.SubSection ss on ss.Id=e.SubSectionId
                left join LeaveEncashmentHistory leh on leh.EmployeeId=e.SystemId
                left join LeaveType lt on lt.Id=leh.LeaveTypeId
                where doj between '"+From+"' and '"+To+@"' and leh.LeaveYearId='"+Year+@"'
                and e.PlantId='"+PlantId+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public void ProcessRegData(string Data, string PlantId, string CurrentLvYearId,
           decimal MaxEncash, List<string> LeaveTypeList)
        {
            try
            {

                #region Reg Encashment Processing
             
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsStaticInfo info = new clsStaticInfo();

                string LTypeId = "''";

                for (int i = 0; i < LeaveTypeList.Count; i++)
                {
                    LTypeId += ",'" + LeaveTypeList[i].ToString() + "'";
                }

                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
              
                var sqly = @"select * from LeaveEncashmentHistory where
                    LeaveYearId='" + CurrentLvYearId + "' and PlantId='" + PlantId + "'and LeaveTypeId in(" + LTypeId + ")";
                objCon.OpenDataSetThroughAdapter(sqly, out DataSet dsSave, false, false, "", "1");


                List<Dictionary<string, object>> _objects = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Data);
                DataTable Table =ToDataTable(_objects);
                if (Table.Rows.Count > 0)
                {
                    for (int i = 0; i < Table.Rows.Count; i++)
                    {
                        string EmpId = Table.Rows[i][@"EmpId"].ToString();
                        string LeaveTypeId = Table.Rows[i][@"LeaveTypeId"].ToString();
                      
                        #region For Encashment Table

                        dsSave.Tables[0].DefaultView.RowFilter = @"EmployeeId='" + EmpId + "' " +
                            "AND LeaveTypeId='" + LeaveTypeId + "'";
                        if (dsSave.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dry = dsSave.Tables[0].DefaultView[0].Row;
                            dry.BeginEdit();
                            dry["RegularEncashedLv"] = MaxEncash;
                            dry["UpdatedBy"] = identity.Name;
                            dry["UpdatedDate"] = Convert.ToDateTime(DateTime.Now);
                            dry["UpdatedFromIP"] = identity.IPAddress;
                            dry.EndEdit();
                        }                       

                        #endregion
                    }

                    info.SaveDataSets(dsSave);
                }
                #endregion
                             
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}

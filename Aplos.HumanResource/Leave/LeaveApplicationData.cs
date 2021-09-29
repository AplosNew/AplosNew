using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Core;
using Library.ViewModel.HR;
using System.Reflection;
using Library.Model.Biometrics;
using Library.Service.Setups;
using Library.Data;
using clsAttendance;
using Library.Service.Extension.HumanResource.Leave;
using Library.Data.UnitOfWorks;
using Library.Service.Biometrics;

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

        public IEnumerable<object> GetLeaveBalanceType(string sGroupID, string sPlantID, string EmpSystemID, string calYearId)
        {

            try
            {
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;

                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = Convert.ToDateTime(dsCalYear.Tables[0].Rows[0]["FromDate"]).ToString("dd-MMM-yyyy");
                    _ToDate = Convert.ToDateTime(dsCalYear.Tables[0].Rows[0]["ToDate"]).ToString("dd-MMM-yyyy");
                }
                else
                {
                    throw new Exception("No Year found...");
                }
                var esic = GetESICEligibleEmployeeFromEnum(EmpSystemID, _FromDate);

                if (esic.Tables[0].Rows.Count > 0)
                {
                    var sql = @"SELECT	els.CalanderYearID,ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed,
										 els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 ltd.SystemID LvPolDetailsSystemID,
                                         --ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear,
                                         els.DaysCanBeSanctioned, els.EncashedInbetween as Encashed,
                                         ltd.IsAvailExceptionAllowedOnSpecialAppeal,
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

                                            				)--IN";

                    return _sqlRepository.GetDataCollection(sql, null);
                }
                else
                {
                    var sql = @"SELECT	els.CalanderYearID, ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed,
										 els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 ltd.SystemID LvPolDetailsSystemID
                                         --ISNULL(ltd.IsProrataPreviousyear,0)IsProrataPreviousyear,
                                         ,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear,
                                         els.DaysCanBeSanctioned,
                                          ISNULL(ltd.IsAvailExceptionAllowedOnSpecialAppeal,0)IsAvailExceptionAllowedOnSpecialAppeal,
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
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,els.EncashedInbetween as Encashed,
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
                                            (select id from LeaveType where IsESIC=1 and IsGeneral=0) AND lt.LeaveType <>'Maternity'";

                    return _sqlRepository.GetDataCollection(sql, null);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                    WHERE  EEI.EmployeeStatus = 'Active' AND EEI.BudgetCode = (select Id from MST.ManpowerBudget where Code ='" + dtRPBudgetCode.Rows[0]["ROBudgetCode"].ToString() + @"')   AND
                    EEI.DOJ = (SELECT MIN(DOJ) FROM EmployeeInformation WHERE BudgetCode = (select Id from MST.ManpowerBudget where Code ='" + dtRPBudgetCode.Rows[0]["ROBudgetCode"].ToString() + @"'))";


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


                AttendanceProcessAplos ob = new AttendanceProcessAplos();
                ob.LockValidation(items[0].PlantID, items[0].FromDate.ToString("dd-MMM-yyyy"), Convert.ToDateTime(items[0].ToDate).ToString("dd-MMM-yyyy"), items[0].EmpSystemID);


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

                if (items[0].LeaveDayType == "Full Day")
                {
                    duration = 1m;
                    halfDay = false;
                }
                else if (items[0].LeaveDayType == "First Half Day")
                {
                    duration = 0.5m;
                    halfDay = true;
                }
                else if(items[0].LeaveDayType == "Second Half Day")
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
                                    OR LT.ToDate BETWEEN '" + fromDate + @"' AND '" + toDate + "')";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
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

}

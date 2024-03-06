using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.HumanResource.Leave;
using Library.Model.Biometrics;
using Library.Service.Biometrics;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Extension.HumanResource.Leave;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Setups;
using Library.Service.Systems;
using Library.ViewModel.HR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class LeaveTransactionNewService : Service<LeaveTransaction>, ILeaveTransactionNewService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;
        private readonly IPlantService _plantService;
        private readonly ISignatureService _signatrueService;
        private readonly ILeaveTransactionDetailsService _leaveTransactionDetailsService;
        private readonly IEmployeeInformationService _employeeinformationService;



        public LeaveTransactionNewService(
              IRepositoryAsync<LeaveTransaction> PreRecruitmentEmpReferenceRepositor
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            , IPlantService plantService
            , ISignatureService signatrueService
            , ILeaveTransactionDetailsService leaveTransactionDetailsService
            , IEmployeeInformationService employeeinformationService) :
            base(PreRecruitmentEmpReferenceRepositor, unitOfWork, pkGeneratorService)
        {
            _pk = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _plantService = plantService;
            _signatrueService = signatrueService;
            _leaveTransactionDetailsService = leaveTransactionDetailsService;
            _employeeinformationService = employeeinformationService;
        }

        #endregion Constructor

        public IEnumerable<object> EmployeeInfo()
        {
            try
            {
                var _sql = @"SELECT TOP (100) *
                                            FROM (
                                             SELECT E.SystemID
                                              ,convert (int, E.EmployeeCode)EmployeeCode
                                              ,E.BudgetCode
                                              ,pr.UserName Position
                                              ,E.EmployeeName
                                              ,Dsg.StandardName AS Designation
                                              ,DsgGiv.UserName AS GivenDesignation
                                              ,Ent.UserName as Entity
                                              ,U.StandardName AS Unit
                                              ,Dv.StandardName AS Division
                                              ,De.StandardName AS Department
                                              ,Se.StandardName AS Section
                                              ,SuS.StandardName SubSection
                                              ,REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
                                              ,REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ
                                              ,REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC
                                              ,E.EmpType
                                              ,E.EmploymentType EmploymentNature
                                              ,E.NationalID
                                              ,E.GenderID GenderName
                                              ,EC.StandardName EmployeeType
                                              ,E.GroupID,E.CompanyID,E.PlantId
                                              ,E.LVPolicyMasterSystemID
                                             FROM EmployeeInformation AS E
                                             LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON pmb.Id = e.BudgetCode
                                             LEFT OUTER JOIN [ORG].[Position] PR ON PR.Id = PMB.PositionId
                                             LEFT OUTER JOIN [ORG].[Entity] Ent ON PMB.EntityId=Ent.Id
                                             LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.ID
                                             LEFT OUTER JOIN [ORG].[Unit] AS U ON U.ID = E.UnitID
                                             LEFT OUTER JOIN [ORG].Division AS Dv ON Dv.ID = E.DivisionID
                                             LEFT OUTER JOIN [ORG].Department AS De ON De.ID = E.DepartmentID
                                             LEFT OUTER JOIN [HKP].Designation AS Dsg ON Dsg.ID = E.DesignationSystemID
                                             LEFT OUTER JOIN [HKP].Designation AS DsgGiv ON DsgGiv.ID = E.GivenDesignationId
                                             LEFT OUTER JOIN [ORG].Section AS Se ON Se.ID = E.SectionID
                                             LEFT OUTER JOIN [ORG].SubSection AS SuS ON SuS.ID = E.SubSectionID) A ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetShortLeaveSettings(string plantid)
        {
            try
            {
                //string _sql = @"SELECT * FROM PlantWiseShortLeaveSetting WHERE PlantID = '" + plantid + "'";
                string _sql = @"SELECT * FROM PlantWiseHRMSSetting WHERE PlantID = '" + plantid + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet GetLeaveBalanceType(string employeeId, string calanderYearId)
        {
            try
            {
                GridParameter parameters = null;
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT	els.CalanderYearID,
										 els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         --ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear,
                                         els.DaysCanBeSanctioned, els.EncashedInbetween,
                                         ltd.IsAvailExceptionAllowedOnSpecialAppeal,
										 0.00 Balance,
                                         ISNULL(els.CurrentYearAllocation, 0) CurrentAllocation,
                                         ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
                                         ISNULL(ltrn.ldays, 0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
                                         ISNULL(tav.av, 0) Availed,
										 ISNULL(acApl.ldays,0) ldays
                                         FROM trn.EmployeeLeaveSummary els
										 left join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
										 left join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m group by EmpSystemID,LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left  join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																			select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 LEFT  JOIN (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where	IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         LEFT JOIN (SELECT * FROM dbo.LeavePolicyDetail
                                                 where LPMSystemID =
                                                 (select LeavePolicyMasterId  FROM MST.DesignationMaster DM
																			  LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
																			  WHERE DC.PlantId=(select PlantId from dbo.EmployeeInformation where SystemId='" + employeeId + @"') and dm.DesignationId =
                                                    (select givendesignationId from dbo.EmployeeInformation where SystemId='" + employeeId + @"'))
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE els.EmployeeID = '" + employeeId + @"'
                                              --AND PlantID = '20188'
                                              --AND GroupID = 'CG20181'
                                              AND els.CalanderYearID = '" + calanderYearId + @"'"
                };

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet GetLeaveBalanceType(string sGroupID, string sPlantID, string EmpSystemID, string calYearId)
        {

            try
            {
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;

                // var esic = GetESICEligibleEmployee(EmpSystemID);
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {
                    throw new Exception("No Year found...");
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
                                         ---BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         BroughtForward=
										 CASE WHEN LT.LeaveType='Earn' THEN
										 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END
										 
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END


                                         ,ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
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
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
															group by LT.EmpSystemID,LT.LTSystemID
														 )ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (														
															Select Sum(LTD.LeaveDuration) av,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"' and LTD.IsAvailed=1
															group by LT.EmpSystemID,LT.LTSystemID
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

                                            				                                    )--IN

"
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
                                         ---BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
                                         BroughtForward=
										 CASE WHEN LT.LeaveType='Earn' THEN
										 
												 CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												 ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END
										 
										  ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END


                                         ,ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
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

        public DataSet GetLeaveBalanceTypeNew(string sGroupID, string sPlantID, string EmpSystemID, string calYearId)
        {
            try
            {
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;

                // var esic = GetESICEligibleEmployee(EmpSystemID);
                DataSet dsCalYear = GetCalYearInfo(calYearId);
                DataSet dsCalYearNo = GetCalYearInfoByYearNo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();

                }
                else if (dsCalYearNo.Tables[0].Rows.Count > 0)
                {
                    calYearId= dsCalYearNo.Tables[0].Rows[0]["Id"].ToString();
                    _FromDate = dsCalYearNo.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYearNo.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {
                    throw new Exception("No Year found...");
                }
                var esic = GetESICEligibleEmployeeFromEnum(EmpSystemID, DateTime.Now.ToString("dd-MMM-yyyy"));

                var lastYear = Convert.ToDateTime(_FromDate).AddYears(-1);

                var _LFromDate = Convert.ToDateTime(_FromDate).AddYears(-1);
                var  _LToDate = Convert.ToDateTime(_ToDate).AddYears(-1);

                if (esic.Tables[0].Rows.Count > 0)
                {
                    GridParameter parameters = null;
                    parameters = new GridParameter
                    {
                        ExportType = "DATASET",
                        CmdText = @"SELECT els.CalanderYearID,ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed
                                        ,FromDate=FORMAT(ELS.FromDate,'dd-MMM-yyyy')
										,ToDate=FORMAT(ELS.ToDate,'dd-MMM-yyyy')
										 ,els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID,
                                         --ltd.IsProrataPreviousyear,
                                         ltd.IsProratacurrentyear
                                       ,DaysCanBeSanctioned=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END
,CurrentAllocationDCBS= case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END

                                        ,els.EncashedInbetween
                                        ,ltd.IsAvailExceptionAllowedOnSpecialAppeal,
                                        CurrentAllocation=ISNULL(els.CurrentYearAllocation, 0),
                                        ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,                                        

                                         LeaveDays=ISNULL(els.DaysCanBeSanctioned, 0),
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
                                            ISNULL(R.Rejected,0)Rejected,
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
                                            ,Earned=CAST (0 AS decimal(18,2))
,Balance=(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END)-ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0)
                                            ----------------------------------------------------------------------------------------------------------------------



                                          FROM (
                                                        select S.* from trn.EmployeeLeaveSummary S
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where CalanderYearId='" + calYearId + @"' and S.EmployeeId ='" + EmpSystemID + @"' AND lp.EncashmentBasis='CalanderYear') els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
                                        LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId


        LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemID,LT.LTSystemID)R ON R.EmpSystemID = els.EmployeeId  and R.LTSystemId = els.LeaveTypeId
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
															group by LT.EmpSystemID,LT.LTSystemID
														 )ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (														
															Select Sum(LTD.LeaveDuration) av,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
															Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
															Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"' and LTD.IsAvailed=1
															group by LT.EmpSystemID,LT.LTSystemID
														  )tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId

										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = els.EmployeeId and acApl.LTSystemId = els.LeaveTypeId
                                         left outer join (select * from dbo.LeavePolicyDetail
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																										LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
																						where dc.plantid='" + sPlantID + @"'
																		 ) dm where dm.DesignationId =(select givendesignationId from dbo.EmployeeInformation where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
                                                WHERE els.EmployeeID = '" + EmpSystemID + @"'
                                              --AND CalanderYearID = '" + calYearId + @"'
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
                                                      WHERE EI.SystemID='" + EmpSystemID + @"' AND EI.GroupID='"+ sGroupID + @"' AND EI.PlantID='" + sPlantID + @"'
                                                       )
                                                    AND
                                                    EPLT.ESICPolicyMasterID IN (
                                                     SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId='" + sPlantID + @"') DM
                                                     WHERE DM.DesignationId IN (SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + EmpSystemID + @"'))) AND LT.UserName NOT LIKE '%Maternity%'

UNION ALL
 Select A.CalanderYearID,CAST (0 AS BIT) IsExceptionAllowed,A.FromDate,A.ToDate
 ,a.SystemID,A.LTSystemID,A.EmployeeId EmployeeID,A.LeaveName, A.LeaveDescription,ltd.SystemID LvPolDetailsSystemID,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
 ,DaysCanBeSanctioned=CAST (B.CarryForward AS decimal(18,2))
 ,CurrentAllocationDCBS=CAST (B.CarryForward AS decimal(18,2)),0 EncashedInbetween,CAST (0 AS BIT) IsAvailExceptionAllowedOnSpecialAppeal
 ,CurrentAllocation=CASE WHEN Masterx.LeaveType='Earn' THEN(select ((SELECT DATEDIFF(day,  cast(YEAR('" + _FromDate + @"') as char(4)),  cast(YEAR('" + _FromDate + @"')+1 as char(4))))-COUNT(d.OffDayDate))/ltd.EncashWorkingDaysQty
		                                from scs.OffDayDetail d
		                                inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('H','W') and m.PlantId='" + sPlantID + @"'
		                                where d.PlantId='" + sPlantID + @"'
		                                and d.OffDayDate between '" + _FromDate + @"' AND '" + _ToDate + @"')ELSE ltd.LeaveDays END  
,CAST (0 AS decimal(18,2)) PreviousYearCarryForward
 --,BroughtForward=CASE WHEN A.Opening>B.CarryForward THEN A.Opening WHEN B.BroughtForward>B.CarryForward THEN B.BroughtForward ELSE B.CarryForward END
 ,BroughtForward=CAST (A.Opening AS decimal(18,2))
 
  ,LeaveDays=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 ,ISNULL(ltrn.ldays, 0) Applied,ISNULL(tav.av, 0) Availed,ISNULL(R.Rejected,0) Rejected,ISNULL(acApl.ldays,0) ldays,A.LeaveType,CAST (0 AS BIT) IsBroughtForwardAdd
 ,(round((CAST(case when ltd.EncashWorkingDaysQty >0 then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty else 0 END AS decimal(18,2))) * 2, 0)/2) as Earned

,Balance=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 from (
 select A.LeaveYearId CalanderYearID,0 IsExceptionAllowed,a.Id SystemID,A.LeaveTypeId LTSystemID,A.EmployeeId EmployeeID,lt.UserName LeaveName, lt.Description LeaveDescription,lt.LeaveType
 ,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate,A.Opening 
 from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId --AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between'" + _FromDate + @"' AND '" + _ToDate + @"' 
										  AND LY.ToDate between'" + _FromDate + @"' AND '" + _ToDate + @"'
                                           AND A.EmployeeId='" + EmpSystemID + @"'
										  ) A  
LEFT JOIN (
select A.EmployeeId,A.LeaveTypeId,lt.UserName LeaveName,A.CarryForward from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                                        LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										 Where LY.FromDate between '" + _LFromDate + @"' AND '" + _LToDate + @"' 
										  AND LY.ToDate between '" + _LFromDate + @"' AND '" + _LToDate + @"' AND A.EmployeeId='" + EmpSystemID + @"')B ON B.EmployeeId=A.EmployeeId  AND A.LTSystemID=B.LeaveTypeId

left outer join (select ltd.* from dbo.LeavePolicyDetail ltd
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																			SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																			LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId where dc.plantid='" + sPlantID + @"'
																		 ) dm where dm.DesignationId =(select givendesignationId  from dbo.EmployeeInformation where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = A.LTSystemID

LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' AND '" + _ToDate + @"' AND IsCancel=1
                                                            group by LT.EmpSystemID,LT.LTSystemID)R ON R.EmpSystemID = A.EmployeeId  and R.LTSystemId = A.LTSystemID
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                                                            group by LT.EmpSystemID,LT.LTSystemID
														)ltrn on ltrn.EmpSystemID = A.EmployeeId and ltrn.LTSystemId = A.LTSystemID
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
														)tav on tav.EmpSystemID = A.EmployeeId and tav.LTSystemId = A.LTSystemID
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = A.EmployeeId and acApl.LTSystemId = A.LTSystemID
left join (SELECT EmpSystemID,SUM(l.EarnValue)EarnDays,T.Id as LeaveId,ei.PlantId,t.LeaveType
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + _FromDate + @"' and '" + _ToDate + @"'
                and EI.PlantID='" + sPlantID + @"' and t.LeaveType='Earn'
                group by EmpSystemID,t.Id,ei.plantid,t.LeaveType
                ) as Masterx on Masterx.EmpSystemID=A.EmployeeId and Masterx.PlantId='" + sPlantID + @"' and Masterx.LeaveId=A.LTSystemID
				left join (Select top(1) md.* from  ManualLeaveData md
				JOIN LeaveType T ON t.Id=md.LeaveTypeId AND t.LeaveType='Earn'
				Where md.EmployeeId='" + EmpSystemID + @"'order by md.addeddate desc
				)med on med.EmployeeId=A.EmployeeId
Where A.EmployeeId='" + EmpSystemID + "'"
                    };
                    parameters.sort = "LeaveName";
                    parameters.order = "ASC";
                        return _sqlRepository.GetGridData(parameters).Source; 
                }
                else
                {
                    GridParameter parameters = null;
                    parameters = new GridParameter
                    {
                        ExportType = "DATASET",
                        CmdText = @"SELECT els.CalanderYearID, ISNULL(ltd.IsExceptionAllowed,0) IsExceptionAllowed
                                        ,FromDate=FORMAT(ELS.FromDate,'dd-MMM-yyyy')
										,ToDate=FORMAT(ELS.ToDate,'dd-MMM-yyyy')
										 ,els.Id SystemID,
                                         els.LeaveTypeId LTSystemID,
                                         els.EmployeeID,
										 lt.UserName LeaveName,
										 lt.Description LeaveDescription,
                                         ltd.SystemID LvPolDetailsSystemID
                                         ,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
                                     ,DaysCanBeSanctioned= case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END
,CurrentAllocationDCBS=case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END
										,els.EncashedInbetween
                                         ,CAST (ISNULL(ltd.IsAvailExceptionAllowedOnSpecialAppeal,0) AS BIT)IsAvailExceptionAllowedOnSpecialAppeal,
                                        CurrentAllocation=ISNULL(els.CurrentYearAllocation, 0),
                                         ISNULL(els.PreviousYearCarryForward, 0) PreviousYearCarryForward,
										 --all carry forward
                                         --ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) BroughtForward,
                                         BroughtForward=CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,
            --                             BroughtForward=CASE WHEN LT.LeaveType='Earn' THEN	
												--ISNULL(ALP.PBroughtForward, 
												-- CASE WHEN els.IsEncashed =1 THEN ISNULL(els.CarryForward, 0)+ISNULL(els.EncashedInbetween, 0) 
												-- ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END)													   
										  --ELSE ISNULL(els.BroughtForward, 0)+isnull(els.CarryForwardOpeningBalance,0) END,


                                         ISNULL(els.DaysCanBeSanctioned, 0) LeaveDays,
-- LeaveDays=ISNULL(CASE WHEN LT.LeaveType='Earn' THEN ALD.Opening ELSE ISNULL(els.DaysCanBeSanctioned, 0) END,0),
										 --applied +applied ob
                                         ISNULL(ltrn.ldays, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Applied,
										 --(ISNULL(tav.av, 0)+ ISNULL(acApl.ldays,0)) Applied,
                                         --0 Availed,
										  --Availed +Availed ob
                                         ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0) Availed,
                                         ISNULL(R.Rejected,0)Rejected,
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
,Earned=CAST (0 AS decimal(18,2))
,Balance=(case when ltd.LvAvailedOnFixedOrPercentage='Fixed' then  Isnull(ltd.LvCanAvailQuantity,0)
																   when ltd.LvAvailedOnFixedOrPercentage='Percentage' then  (Isnull(ltd.LvCanAvailQuantity,0) * Isnull(els.DaysCanBeSanctioned,0))/100
																   else Isnull(els.DaysCanBeSanctioned,0) END)-ISNULL(tav.av, 0)+isnull(CurrentYearAvailedOpeningBalance,0)
----------------------------------------------------------------------------------------------------------------------
                                          FROM (    select S.* from trn.EmployeeLeaveSummary S
                                                        LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=s.EmployeeId
                                                        LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                                        LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                                        LEFT JOIN LeavePolicyDetail AS lp ON lp.LPMSystemID=dmc.LeavePolicyMasterId AND s.LeaveTypeId=lp.LTSystemID
                                                        where CalanderYearId='" + calYearId + @"' and S.EmployeeId ='" + EmpSystemID + @"' AND lp.EncashmentBasis='CalanderYear'
														) els
										 left outer join dbo.LeaveType lt on lt.Id = els.LeaveTypeId
        LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate+@"' AND '"+_ToDate+@"' AND IsCancel=1
                                                            group by LT.EmpSystemID,LT.LTSystemID)R ON R.EmpSystemID = els.EmployeeId  and R.LTSystemId = els.LeaveTypeId
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '"+_FromDate+@"' and '"+_ToDate+@"'
                                                            group by LT.EmpSystemID,LT.LTSystemID
														)ltrn on ltrn.EmpSystemID = els.EmployeeId and ltrn.LTSystemId = els.LeaveTypeId
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '"+_FromDate+@"' and '"+_ToDate+@"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = els.EmployeeId and tav.LTSystemId = els.LeaveTypeId
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
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
																						where dc.plantid='" + sPlantID+ @"'

																		 ) dm where dm.DesignationId =(select givendesignationId 
																									 from dbo.EmployeeInformation 
																									 where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = lt.Id
LEFT JOIN EmployeeInformation AS emp ON emp.SystemId  = els.EmployeeId
                                                WHERE els.EmployeeID = '" + EmpSystemID + @"'
                                              
                                              AND els.LeaveTypeId not IN (select id from LeaveType where IsESIC=1 and IsGeneral=0) AND LT.UserName NOT LIKE '%Maternity%'

UNION ALL
 Select A.CalanderYearID,CAST (0 AS BIT) IsExceptionAllowed,A.FromDate,A.ToDate
 ,a.SystemID,A.LTSystemID,A.EmployeeId EmployeeID,A.LeaveName, A.LeaveDescription,ltd.SystemID LvPolDetailsSystemID,ISNULL(ltd.IsProratacurrentyear,0)IsProratacurrentyear
 ,DaysCanBeSanctioned=CAST (B.CarryForward AS decimal(18,2))
 ,CurrentAllocationDCBS=CAST (B.CarryForward AS decimal(18,2)),0 EncashedInbetween,CAST (0 AS BIT) IsAvailExceptionAllowedOnSpecialAppeal
 ,CurrentAllocation=CASE WHEN Masterx.LeaveType='Earn' THEN (select ((SELECT DATEDIFF(day,  cast(YEAR('" + _FromDate + @"') as char(4)),  cast(YEAR('" + _FromDate + @"')+1 as char(4))))-COUNT(d.OffDayDate))/ltd.EncashWorkingDaysQty
		                                from scs.OffDayDetail d
		                                inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('H','W') and m.PlantId='" + sPlantID + @"'
		                                where d.PlantId='" + sPlantID + @"'
		                                and d.OffDayDate between '" + _FromDate + @"' AND '" + _ToDate + @"') ELSE ltd.LeaveDays END
,CAST (0 AS decimal(18,2)) PreviousYearCarryForward
 --,BroughtForward=CASE WHEN A.Opening>B.CarryForward THEN A.Opening WHEN B.BroughtForward>B.CarryForward THEN B.BroughtForward ELSE B.CarryForward END
 ,BroughtForward=CAST (A.Opening AS decimal(18,2))
 
  ,LeaveDays=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 ,ISNULL(ltrn.ldays, 0) Applied,ISNULL(tav.av, 0) Availed,ISNULL(R.Rejected,0) Rejected,ISNULL(acApl.ldays,0) ldays,A.LeaveType,CAST (0 AS BIT) IsBroughtForwardAdd
 ,(round((CAST(case when ltd.EncashWorkingDaysQty >0 then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty else 0 END AS decimal(18,2))) * 2, 0)/2) as Earned

,Balance=ROUND(CAST (A.Opening +CAST(case when ltd.EncashWorkingDaysQty >0 
then (isnull(Masterx.EarnDays,'0')+ isnull(med.Earned,'0'))/ltd.EncashWorkingDaysQty 
else 0 END AS decimal(18,2))-ISNULL(tav.av, 0)AS decimal(18,2)),0)
 from (
 select A.LeaveYearId CalanderYearID,0 IsExceptionAllowed,a.Id SystemID,A.LeaveTypeId LTSystemID,A.EmployeeId EmployeeID,lt.UserName LeaveName, lt.Description LeaveDescription,lt.LeaveType
 ,FORMAT(LY.FromDate,'dd-MMM-yyyy')FromDate,FORMAT(LY.ToDate,'dd-MMM-yyyy')ToDate,A.Opening 
 from dbo.AnnualLeaveDataCurrent A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId --AND LeaveType='Earn'
										  LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between'" + _FromDate+@"' AND '"+_ToDate+@"' 
										  AND LY.ToDate between'"+_FromDate+ @"' AND '"+_ToDate+ @"'
										  ) A  
LEFT JOIN (
select BroughtForward=CASE WHEN A.Adjustment=0 THEN A.Opening ELSE A.Adjustment END,A.EmployeeId,A.LeaveTypeId,lt.UserName LeaveName,A.CarryForward from dbo.AnnualLeaveDataPast A
										left outer join dbo.LeaveType lt on lt.Id = A.LeaveTypeId AND LeaveType='Earn'
                                        LEFT JOIN dbo.LeaveYearDefination LY  ON LY.Id=A.LeaveYearId
										  Where LY.FromDate between '" + _LFromDate + @"' AND '"+_LToDate+@"' 
										  AND LY.ToDate between '" + _LFromDate + @"' AND '"+_LToDate+@"')B ON B.EmployeeId=A.EmployeeId  AND A.LTSystemID=B.LeaveTypeId

left outer join (select ltd.* from dbo.LeavePolicyDetail ltd
																 where LPMSystemID =
																 (--w
																 select LeavePolicyMasterId from 
																		 (
																			SELECT DC.LeavePolicyMasterId,dm.DesignationId FROM MST.DesignationMaster DM
																			LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId where dc.plantid='" + sPlantID+ @"'
																		 ) dm where dm.DesignationId =(select givendesignationId  from dbo.EmployeeInformation where SystemId='" + EmpSystemID + @"')
																	)--w
                                                 ) ltd on ltd.LTSystemID = A.LTSystemID

LEFT JOIN (Select Sum(LTD.LeaveDuration) Rejected,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '" + _FromDate+@"' AND '"+_ToDate+@"' AND IsCancel=1
                                                            group by LT.EmpSystemID,LT.LTSystemID)R ON R.EmpSystemID = A.EmployeeId  and R.LTSystemId = A.LTSystemID
										 left outer join (
															Select Sum(LTD.LeaveDuration) ldays,LT.EmpSystemID,LT.LTSystemID  from LeaveTransaction LT
                                                            Left Join LeaveTransactionDetails LTD on LT.SystemID=LTD.LvTrnsSystemID
                                                            Where WorkDate between '"+_FromDate+@"' and '"+_ToDate+@"'
                                                            group by LT.EmpSystemID,LT.LTSystemID
														)ltrn on ltrn.EmpSystemID = A.EmployeeId and ltrn.LTSystemId = A.LTSystemID
										 left outer join (
																select sum(c) av,EmpSystemID,LTSystemID from
																(
																	select m.EmpSystemID,m.LTSystemID,c from dbo.LeaveTransaction m
																	left outer join
																		(
																		Select SUM(d.LeaveDuration) c,d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where
																			IsAvailed = 1 and WorkDate between '"+_FromDate+@"' and '"+_ToDate+ @"'
                                                                        group by LvTrnsSystemID
																		) ltrnDt on ltrnDt.LvTrnsSystemID = m.SystemID
																)x group by EmpSystemID,LTSystemID
														)tav on tav.EmpSystemID = A.EmployeeId and tav.LTSystemId = A.LTSystemID
										 left outer join (
															select sum(m.LeaveDays) ldays,m.EmpSystemID,m.LTSystemID from dbo.LeaveTransaction m
																where m.SystemID not in(select d.LvTrnsSystemID from dbo.LeaveTransactionDetails d where IsAvailed = 1 or WorkDate<=CONVERT(date, getdate()))
																group by EmpSystemID,LTSystemID
														  )acApl  on acApl.EmpSystemID = A.EmployeeId and acApl.LTSystemId = A.LTSystemID
left join (SELECT EmpSystemID,SUM(l.EarnValue)EarnDays,T.Id as LeaveId,ei.PlantId,t.LeaveType
				FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + _FromDate + @"' and '" + _ToDate + @"'
                and EI.PlantID='" + sPlantID + @"' and t.LeaveType='Earn'
                group by EmpSystemID,t.Id,ei.plantid,t.LeaveType
                ) as Masterx on Masterx.EmpSystemID=A.EmployeeId and Masterx.PlantId='" + sPlantID + @"' and Masterx.LeaveId=A.LTSystemID
				left join (Select top(1) md.* from  ManualLeaveData md
				JOIN LeaveType T ON t.Id=md.LeaveTypeId AND t.LeaveType='Earn'
				Where md.EmployeeId='" + EmpSystemID + @"' order by md.addeddate desc
				)med on med.EmployeeId=A.EmployeeId
Where A.EmployeeId='" + EmpSystemID+"'"
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



        public IEnumerable<object> LoadLvPolicyWiseLeaveTypeCmb(string sPlantID, string strLvPolSysID, string employeeId)
        {
            var CompanyGroupId = _employeeinformationService.Query(t => t.SystemId == employeeId).Select(t => t.GroupID).FirstOrDefault();

            var esic = GetESICEligibleEmployeeFromEnum(employeeId, DateTime.Now.ToString("dd-MMM-yyyy"));
            //var esic = GetESICEligibleEmployee(employeeId);

            string _sql;
            try
            {
                if (esic.Tables[0].Rows.Count > 0)
                {
                    _sql = @"SELECT LT.ID, LT.UserName LeaveName,LT.IsESIC,LT.IsGeneral, EPLT.ESICPolicyMasterID FROM dbo.ESICPolicyLeaveType AS EPLT
                  LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                  WHERE
                  EPLT.LeaveTypeID IN
                   (
                     SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                  LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                  LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                  WHERE DC.PlantId='" + sPlantID + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                  LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                  WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + CompanyGroupId + @"' AND EI.PlantID='" + sPlantID + @"'
                   )
                AND
                EPLT.ESICPolicyMasterID IN (
                 SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                 LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                 WHERE DC.PlantId='" + sPlantID + @"') DM
                 WHERE DM.DesignationId IN (
                  SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + employeeId + @"'
                  )
                )";
                }
                else
                {
                    _sql = @"SELECT LT.ID, LT.UserName LeaveName FROM LeaveType LT
                                    LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                    LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
                                    LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId='" + sPlantID + @"') DM ON DM.LeavePolicyMasterId=LPM.SystemID
                                    LEFT JOIN EmployeeInformation EI ON EI.GivenDesignationId=DM.DesignationId
                                    LEFT JOIN ESICEligibleEmployee EE ON EE.EmpSystemID=EI.SystemId
                                    WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + CompanyGroupId + @"' AND EI.PlantID='" + sPlantID + @"' AND LT.IsGeneral = 1";
                }
                //_sql = @"SELECT LT.Id, LT.UserName LeaveName FROM LeavePolicyDetail LPD
                //                LEFT JOIN LeaveType LT ON LPD.LTSystemID = LT.Id
                //            WHERE LPD.PlantID = '" + sPlantID + @"' AND LPD.LPMSystemID = '" + strLvPolSysID + @"' AND LPD.IsActive = 1
                //            --UNION
                //            --(SELECT Id, UserName LeaveName FROM LeaveType
                //            --        WHERE LeaveType IN ('Leave Without Pay','Earn'))
                //            --ORDER BY LT.UserName
                //            ";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        public IEnumerable<object> xxxLoadLvPolicyWiseLeaveTypeCmb(string sPlantID, string strLvPolSysID, string employeeId)
        {
            var CompanyGroupId = _employeeinformationService.Query(t => t.SystemId == employeeId).Select(t => t.GroupID).FirstOrDefault();

            var esic = GetESICEligibleEmployeeFromEnum(employeeId, DateTime.Now.ToString("dd-MMM-yyyy"));
            //var esic = GetESICEligibleEmployee(employeeId);

            string _sql;
            try
            {
                if (esic.Tables[0].Rows.Count > 0)
                {
                    _sql = @"SELECT LT.ID, LT.UserName LeaveName,LT.IsESIC,LT.IsGeneral, EPLT.ESICPolicyMasterID FROM dbo.ESICPolicyLeaveType AS EPLT
                  LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                  WHERE
                  EPLT.LeaveTypeID IN
                   (
                     SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                  LEFT JOIN MST.DesignationMaster AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                  LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                  WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + CompanyGroupId + @"' AND EI.PlantID='" + sPlantID + @"'
                   )
                AND
                EPLT.ESICPolicyMasterID IN (
                 SELECT DM.ESICPolicyMasterID FROM MST.DesignationMaster DM
                 WHERE DM.DesignationId IN (
                  SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + employeeId + @"'
                  )
                )";
                }
                else
                {
                    _sql = @"SELECT LT.ID, LT.UserName LeaveName FROM LeaveType LT
                                    LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                    LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
                                    LEFT JOIN MST.DesignationMaster DM ON DM.LeavePolicyMasterId=LPM.SystemID
                                    LEFT JOIN EmployeeInformation EI ON EI.GivenDesignationId=DM.DesignationId
                                    LEFT JOIN ESICEligibleEmployee EE ON EE.EmpSystemID=EI.SystemId
                                    WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + CompanyGroupId + @"' AND EI.PlantID='" + sPlantID + @"' AND LT.IsGeneral = 1";
                }
                //_sql = @"SELECT LT.Id, LT.UserName LeaveName FROM LeavePolicyDetail LPD
                //                LEFT JOIN LeaveType LT ON LPD.LTSystemID = LT.Id
                //            WHERE LPD.PlantID = '" + sPlantID + @"' AND LPD.LPMSystemID = '" + strLvPolSysID + @"' AND LPD.IsActive = 1
                //            --UNION
                //            --(SELECT Id, UserName LeaveName FROM LeaveType
                //            --        WHERE LeaveType IN ('Leave Without Pay','Earn'))
                //            --ORDER BY LT.UserName
                //            ";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        public IEnumerable<ComboModel> LoadLeaveTypeCbo(string sPlantID, string employeeId)
        {
            var CompanyGroupId = _employeeinformationService.Query(t => t.SystemId == employeeId).Select(t => t.GroupID).FirstOrDefault();

            var esic = GetESICEligibleEmployeeFromEnum(employeeId, DateTime.Now.ToString("dd-MMM-yyyy"));
            //var esic = GetESICEligibleEmployee(employeeId);

            string _sql;
            try
            {
                if (esic.Tables[0].Rows.Count > 0)
                {
                    _sql = @"SELECT LT.ID, LT.UserName,LT.IsESIC,LT.IsGeneral, EPLT.ESICPolicyMasterID FROM dbo.ESICPolicyLeaveType AS EPLT
                  LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                  WHERE
                  EPLT.LeaveTypeID IN
                   (
                     SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                  LEFT JOIN  (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId WHERE DC.PlantId='" + sPlantID + @"') AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                  LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                  WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + CompanyGroupId + @"' AND EI.PlantID='" + sPlantID + @"'
                   )
                AND
                EPLT.ESICPolicyMasterID IN (
                 SELECT DM.ESICPolicyMasterID FROM (SELECT DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
WHERE DC.PlantId='" + sPlantID + @"') DM
                 WHERE DM.DesignationId IN (
                  SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + employeeId + @"'
                  )
                ) AND LT.UserName NOT like'%Maternity%'";
                }
                else
                {
                    _sql = @"SELECT LT.ID, LT.UserName FROM LeaveType LT
                                    LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                    LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
                                    LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId='" + sPlantID + @"') DM ON DM.LeavePolicyMasterId=LPM.SystemID
                                    LEFT JOIN EmployeeInformation EI ON EI.GivenDesignationId=DM.DesignationId
                                    LEFT JOIN ESICEligibleEmployee EE ON EE.EmpSystemID=EI.SystemId
                                    WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + CompanyGroupId + @"' AND EI.PlantID='" + sPlantID + @"' AND LT.IsGeneral = 1 AND LT.UserName NOT like'%Maternity%'";
                }

                return _sqlRepository.GetCombo(_sql, "ID", "UserName");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        public IEnumerable<ComboModel> XLoadLeaveTypeCbo(string sPlantID, string employeeId)
        {
            var CompanyGroupId = _employeeinformationService.Query(t => t.SystemId == employeeId).Select(t => t.GroupID).FirstOrDefault();

            var esic = GetESICEligibleEmployeeFromEnum(employeeId, DateTime.Now.ToString("dd-MMM-yyyy"));
            //var esic = GetESICEligibleEmployee(employeeId);

            string _sql;
            try
            {
                if (esic.Tables[0].Rows.Count > 0)
                {
                    _sql = @"SELECT LT.ID, LT.UserName,LT.IsESIC,LT.IsGeneral, EPLT.ESICPolicyMasterID FROM dbo.ESICPolicyLeaveType AS EPLT
                  LEFT JOIN dbo.LeaveType AS LT ON LT.Id = EPLT.LeaveTypeID
                  WHERE
                  EPLT.LeaveTypeID IN
                   (
                     SELECT LTSystemID FROM dbo.LeavePolicyDetail AS LPD
                  LEFT JOIN MST.DesignationMaster AS DM ON DM.LeavePolicyMasterId=LPD.LPMSystemID
                  LEFT JOIN dbo.EmployeeInformation AS EI ON EI.GivenDesignationId=DM.DesignationId
                  WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + CompanyGroupId + @"' AND EI.PlantID='" + sPlantID + @"'
                   )
                AND
                EPLT.ESICPolicyMasterID IN (
                 SELECT DM.ESICPolicyMasterID FROM MST.DesignationMaster DM
                 WHERE DM.DesignationId IN (
                  SELECT GivenDesignationId FROM dbo.EmployeeInformation WHERE SystemID='" + employeeId + @"'
                  )
                )";
                }
                else
                {
                    _sql = @"SELECT LT.ID, LT.UserName FROM LeaveType LT
                                    LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                    LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
                                    LEFT JOIN MST.DesignationMaster DM ON DM.LeavePolicyMasterId=LPM.SystemID
                                    LEFT JOIN EmployeeInformation EI ON EI.GivenDesignationId=DM.DesignationId
                                    LEFT JOIN ESICEligibleEmployee EE ON EE.EmpSystemID=EI.SystemId
                                    WHERE EI.SystemID='" + employeeId + @"' AND EI.GroupID='" + CompanyGroupId + @"' AND EI.PlantID='" + sPlantID + @"' AND LT.IsGeneral = 1";
                }

                return _sqlRepository.GetCombo(_sql, "ID", "UserName");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        public IEnumerable<ComboModel> LoadYearCbo(string plantId)
        {
            try
            {
                var sql = @"SELECT Id, YearNo FROM dbo.YearlyCalendar WHERE PlantId='" + plantId + "'";
                return _sqlRepository.GetCombo(sql, "Id", "YearNo");
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
        }//End Function

        public DataSet GetCalYearInfoByYearNo(string CalYearId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select * from YearlyCalendar WHERE YearNo='" + CalYearId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }//End Function

        public DataSet xGetESICEligibleEmployee(string empSystemId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM ESICEligibleEmployee WHERE EmpSystemID='" + empSystemId + "' AND IsActive=1"
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
            };//and EffectiveDate<='" + FromDate + @"'
              //var data = _sqlRepository.GetDataCollection(CmdText);
            return _sqlRepository.GetGridData(parameters).Source;
        }
        public DataSet GetESICEligibleEmployeeFromEnumNew(string empSystemId, string FromDate)
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
            };//and EffectiveDate<='" + FromDate + @"'
              //var data = _sqlRepository.GetDataCollection(CmdText);
            return _sqlRepository.GetGridData(parameters).Source;
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

        public DataSet GetEmpWeekendData(string empSystemId, string fromDate, string toDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT DayType FROM EmpDateWiseShiftAssign WHERE EmpSystemID='" + empSystemId + @"' and DayType='W' and WorkDate between '" + fromDate + @"' and '" + toDate + "' "
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetEmpHoliDayData(string sGroupID, string sPlantID, string fromDate, string toDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT OFM.CldDescription, OFM.FromDate, OFM.ToDate, OFM.OffDayType, OFM.TotalDay, OFD.DayName, OFM.PlantID  
	                            FROM scs.OffDayMaster OFM
			                            INNER JOIN scs.OffDayDetail OFD ON OFM.Id = OFD.OffDayMasterId 
                                                                    AND OFD.OffDayDate between '" + fromDate + @"' AND '" + toDate + @"'
                                WHERE OFM.CompanyGroupId = '" + sGroupID + @"' AND OFM.PlantID = '" + sPlantID + @"'
									  AND OFM.OffDayType = 'H'"
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

        public DataSet GetYear(string plantId, string yearId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT Year(FromDate) YearNo FROM [dbo].[YearlyCalendar] WHERE PlantId=" + plantId + @" AND Id=" + yearId + ""
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetYearValue(string plantId, string yearId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT IsYearEndClosed FROM [dbo].[YearlyCalendar] WHERE PlantId=" + plantId + @" AND Id=" + yearId + ""
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }
        public DataSet GetLeavePolicyDetailBackDatePosting(string empSystemId, string fromDate, string lTSystemID)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT IsBackDatePosting,CAST( GETDATE()-BackDatePostingAllowedDays AS date) BackDatePostingDate,DATEDIFF(DAY, CAST( GETDATE()-BackDatePostingAllowedDays AS date), '" + fromDate + @"') AS DateDiff
                             FROM  [dbo].[LeavePolicyDetail] LPD
							INNER JOIN (SELECT E.SystemId, E.EmployeeId, E.EmployeeCode,E.EmployeeName,DGM.EmployeeCategoryId,DMC.LeavePolicyMasterId
												FROM EmployeeInformation e
												LEFT JOIN hkp.Designation egdsg ON egdsg.id = e.GivenDesignationId
												LEFT JOIN HKP.LegalDesignation ld ON ld.Id = e.LegalDesignationId
												LEFT JOIN MST.DesignationMasterLegalDesignation DMLD ON DMLD.LegalDesignationId=E.LegalDesignationId
												LEFT JOIN mst.DesignationMaster DGM ON DGM.Id = DMLD.DesignationMasterId
												LEFT JOIN SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DGM.Id and DMC.PlantId=e.PlantId
												WHERE E.SystemId = '" + empSystemId + @"'  )E ON E.EmployeeCategoryId=LPD.EmpCatId AND E.LeavePolicyMasterId=LPD.LPMSystemID
                            WHERE  LPD.LTSystemID='" + lTSystemID + @"' "
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public IEnumerable<object> GetLvTransInfo(string strSysTemID)
        {
            string _sql;

            try
            {
                _sql = @"SELECT SystemID, ComAssignLvSystemID, LTSystemID, REPLACE(CONVERT(VARCHAR(11), FromDate, 113), ' ', '-') FromDate, lt.UserName,ltrn.LTSystemID,
                                  REPLACE(CONVERT(VARCHAR(11), ToDate, 113), ' ', '-') ToDate, LeaveDays, LvReason AS Reason, ApprovedDate,
                                  REPLACE(CONVERT(VARCHAR(11), AppliedDate, 113), ' ', '-')  AppliedDate
                            FROM dbo.LeaveTransaction ltrn
							left outer join dbo.LeaveType lt on lt.Id = ltrn.LTSystemID
                            WHERE  SystemID = '" + strSysTemID + @"'";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        private string GetPK()
        {
            return _signatrueService.GetAutoNumber("LT-", DateTime.Now).ToString();
        }

        public void SaveData(LeaveTransaction leaveTransaction)
        {
            var flag = false;
            decimal duration = 0.0m;
            var halfDay = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                AttendanceProcessAplos ob = new AttendanceProcessAplos();
                ob.LockValidation(leaveTransaction.PlantID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), Convert.ToDateTime(leaveTransaction.ToDate).ToString("dd-MMM-yyyy"), leaveTransaction.EmpSystemID);




                if (!string.IsNullOrEmpty(leaveTransaction.SystemID))
                {
                    var approved = base.Query(t => t.SystemID == leaveTransaction.SystemID).Select(t => t.IsApproved).FirstOrDefault();
                    if (approved == false)
                    {
                        _leaveTransactionDetailsService.ExecuteSqlCommand(@"DELETE FROM [dbo].LeaveTransactionDetails WHERE LvTrnsSystemID ='" + leaveTransaction.SystemID + "'");
                    }
                    else
                    {
                        throw new CustomException("Approved data can not be updated.");
                    }

                    var cancel = base.Query(t => t.SystemID == leaveTransaction.SystemID).Select(t => t.IsCancel).FirstOrDefault();
                    if (cancel)
                    {
                        throw new CustomException("Reject leave cannot be modify...");
                    }
                }

                var restEmployee = GetRestEmployee(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), leaveTransaction.ToDate.ToString());

                if (restEmployee.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This employee is in rest.");
                }

                var getOdData = GetEmpODData(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), leaveTransaction.ToDate.ToString());
                if (getOdData.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This employee is on duty.");
                }



                _unitOfWork.BeginTransaction();
                flag = true;

                var dtFmDate = Convert.ToDateTime(leaveTransaction.FromDate);
                var dtToDate = Convert.ToDateTime(leaveTransaction.ToDate);

                TimeSpan difference = dtToDate - dtFmDate;
                var leaveDays_t = Convert.ToInt32(difference.Days + 1);
                decimal leaveDays = Convert.ToDecimal(leaveDays_t);

                var cgId = _plantService.Query(t => t.Id == leaveTransaction.PlantID).Select(t => t.CompanyGroupId).FirstOrDefault();
                var LVPolicyMasterSystemID = _employeeinformationService.Query(t => t.SystemId == leaveTransaction.EmpSystemID).Select(t => t.LVPolicyMasterSystemID).FirstOrDefault();


                //for sandwich W/H
                PolicySandwichVM _sandwichVM = new PolicySandwichVM();
                clsOffDayList _clsOffDayList = new clsOffDayList();
                List<string> _list_W = new List<string>();
                List<string> _list_H = new List<string>();
                _clsOffDayList.createOffDayList(identity.PlantId, leaveTransaction, _list_H, _list_W);
                //for sandwich W/H

                leaveTransaction.LeaveStatus = LeaveStatus.Pending.ToString();
                leaveTransaction.GroupID = cgId;

                //if (leaveTransaction.LeaveDayType == "FirstHalfDay" || leaveTransaction.LeaveDayType == "SecondHalfDay")
                //{
                //    leaveTransaction.LeaveDays = 0.5m;
                //}
                //else
                //{
                //    if (_offDayList.Count > 0)
                //    {
                //        leaveDays = leaveDays - _offDayList.Count;
                //    }

                //    if (leaveDays <= 0)
                //    {
                //        throw new Exception("Define Off Day setting for this employee");
                //    }

                //    leaveTransaction.LeaveDays = leaveDays;
                //}

                //CheckMaxLeaveataTime(cgId, leaveTransaction.PlantID, LVPolicyMasterSystemID, leaveTransaction.LTSystemID, leaveDays, leaveTransaction.EmpSystemID, Convert.ToDateTime(leaveTransaction.AppliedDate), Convert.ToDateTime(leaveTransaction.ToDate), leaveTransaction.FromDate);

                #region leave days validation
                decimal _leave_days = leaveDays;
                if (leaveTransaction.LeaveDayType == "FirstHalfDay" || leaveTransaction.LeaveDayType == "SecondHalfDay")
                {
                    _leave_days = 0.5m;
                    leaveDays = 0.5m;
                }
                else
                {
                    //if (_offDayList.Count > 0)
                    //{
                    //    leaveDays = leaveDays - _offDayList.Count;
                    //}
                    clsEmpWiseLeavePolicyInfo _obj_POD = new clsEmpWiseLeavePolicyInfo(leaveTransaction.PlantID);
                    _obj_POD.GetLeaveCount(leaveTransaction.EmpSystemID, leaveTransaction.LTSystemID, _list_H.Count, _list_W.Count, ref leaveDays, out _sandwichVM);

                    if (leaveDays <= 0)
                    {
                        throw new Exception("Define OffDay setting for this employee");
                    }
                }


                CheckMaxLeaveataTime(leaveTransaction, LVPolicyMasterSystemID, leaveDays);

                if (leaveTransaction.LeaveDayType == "FirstHalfDay" || leaveTransaction.LeaveDayType == "SecondHalfDay")
                {
                    leaveTransaction.LeaveDays = 0.5m;
                    leaveTransaction.ToDate = leaveTransaction.FromDate;
                }
                else
                {
                    leaveTransaction.LeaveDays = leaveDays;
                }
                #endregion

                if (string.IsNullOrEmpty(leaveTransaction.SystemID))
                {

                    var employeePreviousData = GetEmpPreviousData(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), leaveTransaction.ToDate.ToString());

                    if (employeePreviousData.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Another leave has applied in this days.");
                    }


                    var pk = GetPK();
                    leaveTransaction.SystemID = "LT" + DateTime.Now.ToString("yy") + "-" + pk;
                    leaveTransaction.DateAdded = DateTime.Now;
                    Insert(leaveTransaction);
                }
                else
                {
                    leaveTransaction.DateUpdated = DateTime.Now;
                    Update(leaveTransaction);
                }
                LeaveTransactionDetails details = new LeaveTransactionDetails
                {
                    LvTrnsSystemID = leaveTransaction.SystemID,
                    AddedBy = leaveTransaction.AddedBy,
                    DateAdded = leaveTransaction.DateAdded
                };

                if (leaveTransaction.LeaveDayType == "FullDay")
                {
                    duration = 1m;
                    halfDay = false;
                }
                else if (leaveTransaction.LeaveDayType == "FirstHalfDay")
                {
                    duration = 0.5m;
                    halfDay = true;
                }
                else
                {
                    duration = 0.5m;
                    halfDay = false;
                }
                _leaveTransactionDetailsService.InsertGraph(_sandwichVM, _list_H, _list_W, details, leaveTransaction.FromDate, Convert.ToDateTime(leaveTransaction.ToDate), duration, halfDay);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();




            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, leaveTransaction.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void SaveLeaveData(LeaveTransaction leaveTransaction)
        {
            var flag = false;
            decimal duration = 0.0m;
            var halfDay = false;
            try
            {

                AttendanceProcessAplos ob = new AttendanceProcessAplos();
                ob.LockValidation(leaveTransaction.PlantID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), Convert.ToDateTime(leaveTransaction.ToDate).ToString("dd-MMM-yyyy"), leaveTransaction.EmpSystemID);

                if (!string.IsNullOrEmpty(leaveTransaction.SystemID))
                {
                    var approved = base.Query(t => t.SystemID == leaveTransaction.SystemID).Select(t => t.IsApproved).FirstOrDefault();
                    if (approved == false)
                    {
                        _leaveTransactionDetailsService.ExecuteSqlCommand(@"DELETE FROM [dbo].LeaveTransactionDetails WHERE LvTrnsSystemID ='" + leaveTransaction.SystemID + "'");
                    }
                    else
                    {
                        throw new CustomException("Approved data can not be updated.");
                    }

                    var cancel = base.Query(t => t.SystemID == leaveTransaction.SystemID).Select(t => t.IsCancel).FirstOrDefault();
                    if (cancel)
                    {
                        throw new CustomException("Reject leave cannot be modify...");
                    }
                }

                var restEmployee = GetRestEmployee(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), leaveTransaction.ToDate.ToString());

                if (restEmployee.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This employee is in rest.");
                }

                var getOdData = GetEmpODData(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), leaveTransaction.ToDate.ToString());
                if (getOdData.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This employee is on duty.");
                }



                _unitOfWork.BeginTransaction();
                flag = true;

                var dtFmDate = Convert.ToDateTime(leaveTransaction.FromDate);
                var dtToDate = Convert.ToDateTime(leaveTransaction.ToDate);

                TimeSpan difference = dtToDate - dtFmDate;
                var leaveDays = Convert.ToDecimal(difference.Days + 1);

                var cgId = _plantService.Query(t => t.Id == leaveTransaction.PlantID).Select(t => t.CompanyGroupId).FirstOrDefault();
                var LVPolicyMasterSystemID = _employeeinformationService.Query(t => t.SystemId == leaveTransaction.EmpSystemID).Select(t => t.LVPolicyMasterSystemID).FirstOrDefault();


                //for sandwich W/H
                PolicySandwichVM _sandwichVM = new PolicySandwichVM();
                clsOffDayList _obj = new clsOffDayList();
                List<string> _list_W = new List<string>();
                List<string> _list_H = new List<string>();
                _obj.createOffDayList(leaveTransaction.PlantID, leaveTransaction, _list_H, _list_W);
                //for sandwich W/H

                leaveTransaction.LeaveStatus = LeaveStatus.Pending.ToString();
                leaveTransaction.GroupID = cgId;

                //if (leaveTransaction.LeaveDayType == "FirstHalfDay" || leaveTransaction.LeaveDayType == "SecondHalfDay")
                //{
                //    leaveTransaction.LeaveDays = 0.5m;
                //}
                //else
                //{
                //    if (_offDayList.Count > 0)
                //    {
                //        leaveDays = leaveDays - _offDayList.Count;
                //    }

                //    if (leaveDays <= 0)
                //    {
                //        throw new Exception("Define Off Day setting for this employee");
                //    }

                //    leaveTransaction.LeaveDays = leaveDays;
                //}

                //CheckMaxLeaveataTime(cgId, leaveTransaction.PlantID, LVPolicyMasterSystemID, leaveTransaction.LTSystemID, leaveDays, leaveTransaction.EmpSystemID, Convert.ToDateTime(leaveTransaction.AppliedDate), Convert.ToDateTime(leaveTransaction.ToDate), leaveTransaction.FromDate);

                #region leave days validation
                decimal _leave_days = leaveDays;
                if (leaveTransaction.LeaveDayType == "FirstHalfDay" || leaveTransaction.LeaveDayType == "SecondHalfDay")
                {
                    _leave_days = 0.5m;
                }
                else
                {
                    //if (_offDayList.Count > 0)
                    //{
                    //    leaveDays = leaveDays - _offDayList.Count;
                    //}
                    clsEmpWiseLeavePolicyInfo _obj_POD = new clsEmpWiseLeavePolicyInfo(leaveTransaction.PlantID);
                    _obj_POD.GetLeaveCount(leaveTransaction.EmpSystemID, leaveTransaction.LTSystemID, _list_H.Count, _list_W.Count, ref leaveDays, out _sandwichVM);

                    if (leaveDays <= 0)
                    {
                        throw new Exception("Define Off Day setting for this employee");
                    }
                }


                CheckMaxLeaveataTime(leaveTransaction, LVPolicyMasterSystemID, leaveDays);

                if (leaveTransaction.LeaveDayType == "FirstHalfDay" || leaveTransaction.LeaveDayType == "SecondHalfDay")
                {
                    leaveTransaction.LeaveDays = 0.5m;
                }
                else
                {
                    leaveTransaction.LeaveDays = leaveDays;
                }
                #endregion

                if (string.IsNullOrEmpty(leaveTransaction.SystemID))
                {

                    var employeePreviousData = GetEmpPreviousData(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), leaveTransaction.ToDate.ToString());

                    if (employeePreviousData.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Another leave has been applied on this date.");
                    }


                    var pk = GetPK();
                    leaveTransaction.SystemID = "LT" + DateTime.Now.ToString("yy") + "-" + pk;
                    leaveTransaction.DateAdded = DateTime.Now;
                    Insert(leaveTransaction);
                }
                else
                {
                    leaveTransaction.DateUpdated = DateTime.Now;
                    Update(leaveTransaction);
                }
                LeaveTransactionDetails details = new LeaveTransactionDetails
                {
                    LvTrnsSystemID = leaveTransaction.SystemID,
                    AddedBy = leaveTransaction.AddedBy,
                    DateAdded = leaveTransaction.DateAdded
                };
                if (leaveTransaction.LeaveDayType == "FullDay")
                {
                    duration = 1m;
                    halfDay = false;
                }
                else if (leaveTransaction.LeaveDayType == "FirstHalfDay")
                {
                    duration = 0.5m;
                    halfDay = true;
                }
                else
                {
                    duration = 0.5m;
                    halfDay = false;
                }
                if (!string.IsNullOrEmpty(leaveTransaction.SystemID))
                {
                    _leaveTransactionDetailsService.InsertGraph(_sandwichVM, _list_H, _list_W, details, leaveTransaction.FromDate, Convert.ToDateTime(leaveTransaction.ToDate), duration, halfDay);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, leaveTransaction.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void SaveAndUpdateData(LeaveTransaction leaveTransaction, string yearId)
        {

            var flag = false;
            decimal duration = 0.0m;
            var halfDay = false;
            try
            {
                #region valida
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                AttendanceProcessAplos ob = new AttendanceProcessAplos();
                ob.LockValidation(leaveTransaction.PlantID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), Convert.ToDateTime(leaveTransaction.ToDate).ToString("dd-MMM-yyyy"), leaveTransaction.EmpSystemID);

                var year = GetYear(identity.PlantId, yearId);
                var fd = leaveTransaction.FromDate.Year.ToString();
                var td = Convert.ToDateTime(leaveTransaction.ToDate).Year.ToString();

                if (year.Tables[0].Rows[0]["YearNo"].ToString() != fd || year.Tables[0].Rows[0]["YearNo"].ToString() != td)
                {
                    throw new CustomException("Calendar year and apply year must be same.");
                }

                if (fd != td)
                {
                    throw new CustomException("FromDate and ToDate must be in the same year.");
                }

                var getYearEnd = GetYearValue(identity.PlantId, yearId);
                if (Convert.ToBoolean(getYearEnd.Tables[0].Rows[0]["IsYearEndClosed"].ToString()) == true)
                {
                    throw new CustomException("Year end process has been done, So leave apply is not allowed on this year.");
                }

                var getIsBackDatePosting = GetLeavePolicyDetailBackDatePosting(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("yyyy-MM-dd"), leaveTransaction.LTSystemID);
                if (getIsBackDatePosting.Tables[0].Rows.Count > 0)
                {
                    if (Convert.ToBoolean(getIsBackDatePosting.Tables[0].Rows[0]["IsBackDatePosting"].ToString()) == true)
                    {
                        if (Convert.ToInt32(getIsBackDatePosting.Tables[0].Rows[0]["DateDiff"]) < 0)
                        {
                            throw new CustomException("Back Date Posting Not Allowed!.");
                        }
                    }
                }

                var restEmployee = GetRestEmployee(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), leaveTransaction.ToDate.ToString());

                if (restEmployee.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This employee is in rest.");
                }

                var getOdData = GetEmpODData(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), leaveTransaction.ToDate.ToString());
                if (getOdData.Tables[0].Rows.Count > 0)
                {
                    if (leaveTransaction.LeaveDays >= 1)
                    {
                        throw new CustomException("This employee is on duty.");
                    }
                }
                #endregion

                var dtFmDate = Convert.ToDateTime(leaveTransaction.FromDate);
                var dtToDate = Convert.ToDateTime(leaveTransaction.ToDate);

                DataTable CAWD = CheckAttdnByWD(leaveTransaction.EmpSystemID, dtFmDate.ToString(), dtToDate.ToString());
                if (CAWD.Rows.Count > 0)
                {
                    throw new CustomException("This " + dtFmDate.ToString() + " work date is not allowed because of attendance.");
                }

                _unitOfWork.BeginTransaction();
                #region master
                flag = true;



                TimeSpan difference = dtToDate - dtFmDate;
                var leaveDays = Convert.ToDecimal(difference.Days + 1);

                var cgId = _plantService.Query(t => t.Id == leaveTransaction.PlantID).Select(t => t.CompanyGroupId).FirstOrDefault();
                var LVPolicyMasterSystemID = _employeeinformationService.Query(t => t.SystemId == leaveTransaction.EmpSystemID).Select(t => t.LVPolicyMasterSystemID).FirstOrDefault();

                //for sandwich W/H
                PolicySandwichVM _policyVM = null;
                clsOffDayList _obj = new clsOffDayList();
                List<string> _list_W = new List<string>();
                List<string> _list_H = new List<string>();
                _obj.createOffDayList(identity.PlantId, leaveTransaction, _list_H, _list_W);
                //for sandwich W/H
                //get policy




                leaveTransaction.LeaveStatus = LeaveStatus.Pending.ToString();
                leaveTransaction.GroupID = cgId;

                #region leave days validation
                decimal _leave_days = leaveDays;
                if (leaveTransaction.LeaveDayType == "FirstHalfDay" || leaveTransaction.LeaveDayType == "SecondHalfDay")
                {
                    _leave_days = 0.5m;
                    leaveDays = 0.5m;
                }
                else
                {
                    clsEmpWiseLeavePolicyInfo _obj_POD = new clsEmpWiseLeavePolicyInfo(identity.PlantId);
                    _obj_POD.GetLeaveCount(leaveTransaction.EmpSystemID, leaveTransaction.LTSystemID, _list_H.Count, _list_W.Count, ref leaveDays, out _policyVM);

                    if (leaveDays <= 0)
                    {
                        throw new Exception("Define Off Day setting for this employee");
                    }
                }


                CheckMaxLeaveataTime(leaveTransaction, LVPolicyMasterSystemID, leaveDays, yearId);//77

                if (leaveTransaction.LeaveDayType == "FirstHalfDay" || leaveTransaction.LeaveDayType == "SecondHalfDay")
                {
                    leaveTransaction.LeaveDays = 0.5m;
                    leaveTransaction.ToDate = leaveTransaction.FromDate;
                }
                else
                {
                    leaveTransaction.LeaveDays = leaveDays;
                }
                #endregion



                if (!string.IsNullOrEmpty(leaveTransaction.SystemID))
                {
                    var cancel = base.Query(t => t.SystemID == leaveTransaction.SystemID).Select(t => t.IsCancel).FirstOrDefault();
                    if (cancel)
                    {
                        throw new CustomException("Reject leave cannot be modify...");
                    }

                    var approved = base.Query(t => t.SystemID == leaveTransaction.SystemID).Select(t => t.IsApproved).FirstOrDefault();
                    if (approved == false)
                    {
                        _leaveTransactionDetailsService.ExecuteSqlCommand(@"DELETE FROM [dbo].LeaveTransactionDetails WHERE LvTrnsSystemID ='" + leaveTransaction.SystemID + "'");
                    }
                    else
                    {
                        throw new CustomException("Approved data can not be updated.");
                    }


                }
                if (string.IsNullOrEmpty(leaveTransaction.SystemID))
                {

                    var employeePreviousData = GetEmpPreviousData(leaveTransaction.EmpSystemID, leaveTransaction.FromDate.ToString("dd-MMM-yyyy"), leaveTransaction.ToDate.ToString());

                    if (employeePreviousData.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Another leave has been applied on this date.");
                    }


                    var pk = GetPK();
                    leaveTransaction.SystemID = "LT" + DateTime.Now.ToString("yy") + "-" + pk;
                    leaveTransaction.DateAdded = DateTime.Now;

                    leaveTransaction.FirstApprovingStatus = true;

                    Insert(leaveTransaction);
                }
                else
                {
                    leaveTransaction.DateUpdated = DateTime.Now;
                    Update(leaveTransaction);
                }
                #endregion

                #region LeaveDetail

                LeaveTransactionDetails details = new LeaveTransactionDetails
                {
                    LvTrnsSystemID = leaveTransaction.SystemID,
                    AddedBy = leaveTransaction.AddedBy,
                    DateAdded = leaveTransaction.DateAdded
                };
                if (leaveTransaction.LeaveDayType == "FullDay")
                {
                    duration = 1m;
                    halfDay = false;
                }
                else if (leaveTransaction.LeaveDayType == "FirstHalfDay")
                {
                    duration = 0.5m;
                    halfDay = true;
                }
                else
                {
                    duration = 0.5m;
                    halfDay = false;
                }
                if (!string.IsNullOrEmpty(leaveTransaction.SystemID))//77
                {
                    _leaveTransactionDetailsService.InsertGraph(_policyVM, _list_H, _list_W, details, leaveTransaction.FromDate, Convert.ToDateTime(leaveTransaction.ToDate), duration, halfDay);
                }
                //_leaveTransactionDetailsService.InsertGraph(details, leaveTransaction.FromDate, Convert.ToDateTime(leaveTransaction.ToDate), duration, halfDay); 
                #endregion

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                //************  New Leave Work Code


                ////Getting the Employees Data from the APD Table

                //ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                //var sqlx = @"select * from AttdnProcessData where WorkDate between '" + Convert.ToDateTime(leaveTransaction.FromDate) + "' and '"+ Convert.ToDateTime(leaveTransaction.ToDate) + @"' 
                //            and EmpSystemID ='" + leaveTransaction.EmpSystemID + "' ";

                //objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                ////Getting the Leave Code
                //var strCode = "Select Code from dbo.LeaveType where Id = '"+leaveTransaction.LTSystemID+"'";
                //DataTable ddt = _sqlRepository.GetDataTable(strCode);
                //string LeaveCode = ddt.Rows[0]["Code"].ToString();


                //DateTime Ftd = Convert.ToDateTime(leaveTransaction.FromDate);
                //DateTime Tld = Convert.ToDateTime(leaveTransaction.ToDate);

                //while ( Ftd <= Tld )
                //{
                //    string newformat = Convert.ToDateTime(Ftd).ToString("yyyyMMdd");
                //    if (Ftd <= DateTime.Now.Date)
                //    {
                //        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + leaveTransaction.EmpSystemID + "' ";
                //        DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                //        dr.BeginEdit();
                //        dr["LeaveStatus"] = LeaveCode;
                //        dr["UpdatedBy"] = "Schedule";
                //        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);

                //        dr.EndEdit();
                //    }

                //    Ftd = Ftd.AddDays(1);
                //}
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, leaveTransaction.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #region LV W LV
        //void createOffDayList(string plantid,LeaveTransaction master, List<string> listoffday)
        //{
        //    DataSet dsLeavePolicy = null;
        //    DataSet dsLeavePolicyMaster = null;
        //    string fromDate = string.Empty;
        //    string toDate = string.Empty;
        //    string LVPolicyMasterSystemID = string.Empty;
        //    try
        //    {
        //        List<string> listoffdayTotal = new List<string>();
        //        fromDate = master.FromDate.ToString("dd-MMM-yyyy");
        //        toDate = master.ToDate.ToString();
        //        //get leave policydetail leavetypeid
        //        //get H/W list 
        //        //update master
        //        //update detail

        //        _getLeavePolicyMaster(master.EmpSystemID, plantid, out dsLeavePolicyMaster);
        //        if(dsLeavePolicyMaster.Tables[0].Rows.Count>0)
        //        {
        //            LVPolicyMasterSystemID = dsLeavePolicyMaster.Tables[0].Rows[0]["LeavePolicyMasterId"].ToString();
        //        }
        //        else
        //        {
        //            throw new Exception("Leave policy is not configured...");
        //        }

        //        _getLeavePolicy(LVPolicyMasterSystemID, master.LTSystemID, out dsLeavePolicy);

        //        //if (dsLeavePolicy.Tables[0].Rows.Count > 0)
        //        //{
        //        //    if (Convert.ToBoolean(dsLeavePolicy.Tables[0].Rows[0]["InBetweenHoliday"].ToString()))//if false sandwich applicable on Holiday
        //        //    {
        //                _getHolidays(plantid, fromDate, toDate, listoffdayTotal);
        //            //}

        //            //if (Convert.ToBoolean(dsLeavePolicy.Tables[0].Rows[0]["InBetweenWeekoff"].ToString()))//sandwich applicable on Weekoff
        //            //{
        //                _getWeekOffdate(master.EmpSystemID, plantid, fromDate, toDate, listoffdayTotal);
        //        //    }
        //        //}




        //        DateTime _fd = Convert.ToDateTime(fromDate);
        //        DateTime _td = Convert.ToDateTime(toDate);
        //        bool IsOffDayAvailable = false;
        //        while (_fd < _td)
        //        {
        //            IsOffDayAvailable= listoffdayTotal.Contains(_fd.ToString("dd-MMM-yyyy"));
        //           if(IsOffDayAvailable)
        //            {
        //                listoffday.Add(_fd.ToString("dd-MMM-yyyy"));
        //            }
        //            _fd = _fd.AddDays(1);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //void _getLeavePolicyMaster(string empid, string plantid, out System.Data.DataSet dsRef)
        //{
        //    string strSQL = string.Empty;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"select c.LeavePolicyMasterId from EmployeeInformation e
        //                        inner join mst.DesignationMasterLegalDesignation m on e.LegalDesignationId=m.LegalDesignationId
        //                        inner join scs.DesignationMasterConfiguration c on c.DesignationMasterId=m.DesignationMasterId and c.PlantId='"+ plantid + @"'
        //                        where e.SystemId='"+ empid + "'";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function
        //void _getLeavePolicy(string LPMSystemID,string LTSystemID, out System.Data.DataSet dsRef)
        //{
        //    string strSQL = string.Empty;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"select isnull(InBetweenHoliday,0) InBetweenHoliday, isnull(InBetweenWeekoff,0) InBetweenWeekoff from LeavePolicyDetail where LPMSystemID='" + LPMSystemID + @"' and LTSystemID='"+ LTSystemID + @"' ";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function
        //void _getHolidays(string plantid, string fromDate,string toDate, List<string> listoffday)
        //{
        //    string strSQL = string.Empty;
        //    DataSet dsRef = null;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"		select d.OffDayDate
        //                          from scs.OffDayDetail d
        //                          inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('H') and m.PlantId='"+plantid+@"'
        //                          where d.PlantId='"+ plantid + @"'
        //                          and d.OffDayDate between '"+ fromDate + @"' and '"+toDate+@"' ";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

        //        for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
        //        {
        //            string dt = Convert.ToDateTime(dsRef.Tables[0].Rows[i]["OffDayDate"].ToString()).ToString("dd-MMM-yyyy");
        //            listoffday.Add(dt);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function
        //void _getWeekoff(string plantid, string fromDate, string toDate, out System.Data.DataSet dsRef)
        //{
        //    string strSQL = string.Empty;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"		select d.OffDayDate
        //                          from scs.OffDayDetail d
        //                          inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('W') and m.PlantId='" + plantid + @"'
        //                          where d.PlantId='" + plantid + @"'
        //                          and d.OffDayDate between '" + fromDate + @"' and '" + toDate + @"' ";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function
        //void _getoffDayIndividual(string empid,string toDate, out System.Data.DataSet dsRef)
        //{
        //    string strSQL = string.Empty;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"		select max(EffectiveDate)EffectiveDate,FstOffDay from EmployeeWeekOffByDay where EmpSystemID ='"+ empid + @"'
        //                            and EffectiveDate<='"+toDate+@"' and IndividualWeekOff=1
        //                            group by FstOffDay ";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function
        //void _getWeekOffdate(string empid,string plantid, string fromDate, string toDate,List<string> list)
        //{
        //    DataSet dsOffdayIndi = null;
        //    DataSet dsWeekOff = null;
        //    try
        //    {
        //        _getoffDayIndividual(empid, toDate, out dsOffdayIndi);
        //        if(dsOffdayIndi.Tables[0].Rows.Count>0)
        //        {
        //            string offday = dsOffdayIndi.Tables[0].Rows[0]["FstOffDay"].ToString();//e.g. friday

        //            DateTime _fd = Convert.ToDateTime(fromDate);
        //            DateTime _td = Convert.ToDateTime(toDate);
        //            while(_fd<_td)
        //            {
        //                if(_fd.ToString("dddd").ToUpper()== offday.ToUpper())
        //                {
        //                    list.Add(_fd.ToString("dd-MMM-yyyy"));
        //                }
        //                _fd = _fd.AddDays(1);
        //            }
        //        }
        //        else
        //        {
        //            _getWeekoff(plantid, fromDate, toDate, out dsWeekOff);
        //            for (int i = 0; i < dsWeekOff.Tables[0].Rows.Count; i++)
        //            {
        //                string dt = Convert.ToDateTime(dsWeekOff.Tables[0].Rows[i]["OffDayDate"].ToString()).ToString("dd-MMM-yyyy");
        //                list.Add(dt);
        //                //list.Add(dsWeekOff.Tables[0].Rows[i]["OffDayDate"].ToString());
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        //objCon = null;
        //    }
        //}//End Function 
        #endregion

        public DataTable CheckAttdnByWD(string EmployeeId, string fromDate, string toDate)
        {
            string sql = @"Select * from dbo.AttdnProcessData Where EmpSystemID='" + EmployeeId + "' AND WorkDate between '" + fromDate + "' AND '" + toDate + "' AND InTime<>'' AND OutTime<>'' AND DATEDIFF(Hour,InTime,OutTime)>=8";
            var list = _sqlRepository.GetDataTable(sql);

            return list;
        }

        public void CheckMaxLeaveataTime(LeaveTransaction leaveTransaction, string leavepolicymasterId, decimal leaveDays, string yearId = "")
        {
            DataSet dsLeavePolicyMaster = null;
            clsOffDayList _odl = null;
            try
            {
                DateTime doc = new DateTime();
                DateTime doj = new DateTime();
                DateTime dateAfterAllow = new DateTime();
                string plantId = leaveTransaction.PlantID;
                string companyGroupId = leaveTransaction.GroupID;
                string leaveTypeId = leaveTransaction.LTSystemID;
                string EmpSystemID = leaveTransaction.EmpSystemID;
                DateTime fromDate = Convert.ToDateTime(leaveTransaction.FromDate);
                DateTime applyDate = Convert.ToDateTime(leaveTransaction.AppliedDate);
                DateTime toDate = Convert.ToDateTime(leaveTransaction.ToDate);
                _odl = new clsOffDayList();

                _odl._getLeavePolicyMaster(EmpSystemID, plantId, out dsLeavePolicyMaster);
                if (dsLeavePolicyMaster.Tables[0].Rows.Count > 0)
                {
                    leavepolicymasterId = dsLeavePolicyMaster.Tables[0].Rows[0]["LeavePolicyMasterId"].ToString();
                }
                else
                {
                    throw new Exception("Leave policy is not configured...");
                }

                var leaveData = GetMaxLeaveAtaTime(companyGroupId, plantId, leavepolicymasterId, leaveTypeId);

                var docanddoj = _employeeinformationService.Query(i => i.SystemId == EmpSystemID).Select(i => new { i.DOC, i.DOJ }).FirstOrDefault();

                if (leaveData.Tables[0].Rows.Count > 0)
                {
                    int maxDayAllowed = (int)(leaveData.Tables[0].Rows[0]["MaxAllocationLimit"]);
                    int minDayAllowed = (int)(leaveData.Tables[0].Rows[0]["MinAllocationLimit"]);
                    bool isMaxAtaRowExp = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsExcessAllow"].ToString());
                    bool subToApproval = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsSubjectToApproval"].ToString());

                    bool IsAllowedInProbationalPeriod = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsAllowed"].ToString());
                    bool isAllowedInSpecialCase = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsAllowedonspecialappeal"].ToString());
                    int allowAfterDays = Convert.ToInt32(leaveData.Tables[0].Rows[0]["AllowedAfterDays"].ToString());
                    bool isPostAppAllow = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsPostApplicationAllowed"].ToString());//
                    bool _IsAvailExceptionAllowedOnSpecialAppeal = Convert.ToBoolean(leaveData.Tables[0].Rows[0]["IsAvailExceptionAllowedOnSpecialAppeal"].ToString());//IsAvailExceptionAllowedOnSpecialAppeal

                    decimal applied = leaveDays;
                    if (minDayAllowed!=0)
                    {
                        if (applied < minDayAllowed)
                        {
                            throw new Exception("Minimum leave at a time should greater or equal " + minDayAllowed + "  days.");
                        } 
                    }
                    if (applied > maxDayAllowed && isMaxAtaRowExp == false)
                    {
                        throw new Exception("Max leave at a time cannot be greater then " + maxDayAllowed + "  day");
                    }
                    if (applied > maxDayAllowed && isMaxAtaRowExp == true && subToApproval == false)
                    {
                        throw new Exception("Max leave at a time cannot be greater then " + maxDayAllowed + "  day");
                    }

                    string stDoc = docanddoj.DOC.ToString();
                    if (!string.IsNullOrEmpty(docanddoj.DOC.ToString()) && !string.IsNullOrEmpty(docanddoj.DOJ.ToString()))
                    {
                        if (stDoc != null && stDoc != "")
                        {
                            doc = Convert.ToDateTime(stDoc);
                        }
                        if (docanddoj.DOJ.ToString() != null)
                        {
                            doj = Convert.ToDateTime(docanddoj.DOJ.ToString());
                        }
                        dateAfterAllow = doj.AddDays(allowAfterDays);
                    }

                    #region check post applicatiton allowed

                    if (isPostAppAllow == false && Convert.ToDateTime(applyDate.ToString().Trim()) > Convert.ToDateTime(toDate.ToString("dd-MMM-yyyy")))
                    {
                        throw new Exception("Post Leave application is not allowed for this Leave Policy");
                    }

                    #endregion check post applicatiton allowed

                    if (stDoc != null && stDoc != "")
                    {
                        doc = Convert.ToDateTime(stDoc);

                        if (IsAllowedInProbationalPeriod == true && Convert.ToDateTime(fromDate.ToString("dd-MMM-yyyy")) < doc && isAllowedInSpecialCase != true)
                        {
                            throw new Exception("Leave application is not allowed for this employee before DOC");
                        }
                    }
                    if (IsAllowedInProbationalPeriod == false && (stDoc == null || stDoc == "") && isAllowedInSpecialCase == false)
                    {
                        throw new Exception("Leave application is not allowed for this employee before DOC");
                    }
                    if (Convert.ToDateTime(fromDate.ToString("dd-MMM-yyyy")) < doj)
                    {

                    }
                    if (IsAllowedInProbationalPeriod == true && Convert.ToDateTime(fromDate.ToString("dd-MMM-yyyy")) < dateAfterAllow)
                    {
                        throw new Exception("Leave application is not allowed for this employee before " + dateAfterAllow.ToString("dd-MMM-yyyy"));
                    }

                    //check balance
                    if (yearId.Length > 0)
                    {
                        // List<LeaveTransactionVM> list_balance = (List<LeaveTransactionVM>)LoadGrdAllocatedLvDetails(companyGroupId, plantId, EmpSystemID, yearId);
                        clsLeaveBalanceToDate leave = new clsLeaveBalanceToDate();
                       // List<Dictionary<string, object>> list_balance = leave.GetLeaveBalanceType(EmpSystemID, yearId);
                        List<Dictionary<string, object>> list_balance = leave.GetLeaveBalanceTypeNew(EmpSystemID, yearId, plantId);
                        foreach (var item in list_balance)
                        {
                            if (item["LeaveTypeId"].ToString() == leaveTypeId)
                            {
                                if (_IsAvailExceptionAllowedOnSpecialAppeal == false)
                                {
                                    if (string.IsNullOrEmpty(leaveTransaction.SystemID) == false)//edit
                                    {
                                        item["ClosingBalance"] = (decimal)(clsStaticInfo.dbl(item["ClosingBalance"].ToString()) - clsStaticInfo.dbl(item["AllFutureAppliedLeave"].ToString())) + leaveTransaction.LeaveDays;
                                    }

                                    if ((decimal)(clsStaticInfo.dbl(item["ClosingBalance"].ToString())) < leaveDays)
                                    {
                                        throw new Exception("Can't apply more than Balance...");
                                    }
                                }
                            }//leave type
                        }//foreach
                    }//yearid
                     ////balance check

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
                CmdText = @"SELECT ISNULL(IsExcessAllow,0) IsExcessAllow, ISNULL(IsSubjectToApproval,0)IsSubjectToApproval, ISNULL(MaxAllocationLimit,0)MaxAllocationLimit, ISNULL(MinAllocationLimit,0)MinAllocationLimit
                            ,LvAvailedOnDOC IsAllowed,IsAllowedonspecialappeal,AllowedAfterDays,IsPostApplicationAllowed,isnull(IsAvailExceptionAllowedOnSpecialAppeal,0) IsAvailExceptionAllowedOnSpecialAppeal
                            FROM dbo.LeavePolicyDetail
                            WHERE  (LPMSystemID = '" + strLPMSystemID + @"') AND (LTSystemID = '" + strLvTypeId + @"')"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }
        public GridModel QueryGetLeaveListForDelete(GridParameter parameters, string companyGroupId, string companyId, string plantId, string employeeId, string yearNo)
        {
            try
            {

                var fromDate = string.Empty;
                var toDate = string.Empty;
                DataSet dsYear = null;
                GetYearlyCalendarDetails(yearNo, out dsYear);
                if (dsYear.Tables[0].Rows.Count > 0)
                {
                    fromDate = dsYear.Tables[0].Rows[0]["FromDate"].ToString();
                    toDate = dsYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                //if (yearNo != "null")
                //{
                //    var year = GetYear(plantId, yearNo);

                //    fromDate = "01-Jan-" + year.Tables[0].Rows[0]["YearNo"];
                //    toDate = "31-Dec-" + year.Tables[0].Rows[0]["YearNo"];
                //}
                else
                {
                    var newyear = DateTime.Now.Year;
                    fromDate = "01-Jan-" + newyear;
                    toDate = "31-Dec-" + newyear;
                }
                parameters.CmdText = @"SELECT LT.*, e.SectionId , L.UserName  AS leaveTypeName,e.EmployeeName,e.EmployeeCode
                                    FROM [dbo].[LeaveTransaction] AS LT
                                    LEFT JOIN [dbo].[LeaveType] AS L ON L.Id=LT.LTSystemID
									LEFT JOIN EmployeeInformation E ON E.SystemId=LT.EmpSystemID
                                    WHERE LT.GroupID='" + companyGroupId + @"' AND E.CompanyId='" + companyId + @"' --AND LT.PlantID='" + plantId + @"' 
                                    AND LT.EmpSystemID='" + employeeId + @"'
                                    AND ((LT.FromDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
                                    OR LT.ToDate BETWEEN '" + fromDate + @"' AND '" + toDate + "') OR L.LeaveType ='Maternity')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, string employeeId, string yearNo)
        {
            try
            {
                var fromDate = string.Empty;
                var toDate = string.Empty;
                DataSet dsYear = null;
                GetYearlyCalendarDetails(yearNo, out dsYear);
                if (dsYear.Tables[0].Rows.Count > 0)
                {
                    fromDate = dsYear.Tables[0].Rows[0]["FromDate"].ToString();
                    toDate = dsYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                //if (yearNo != "null")
                //{
                //    var year = GetYear(plantId, yearNo);

                //    fromDate = "01-Jan-" + year.Tables[0].Rows[0]["YearNo"];
                //    toDate = "31-Dec-" + year.Tables[0].Rows[0]["YearNo"];
                //}
                else
                {
                    var newyear = DateTime.Now.Year;
                    fromDate = "01-Jan-" + newyear;
                    toDate = "31-Dec-" + newyear;
                }
                parameters.CmdText = @"SELECT LT.*, e.SectionId , L.UserName  AS leaveTypeName,e.EmployeeName,e.EmployeeCode,FORMAT(DOJ,'dd-MMM-yyyy')DOJ,format(DOC,'dd-MMM-yyyy')DOC
                                    FROM [dbo].[LeaveTransaction] AS LT
                                    LEFT JOIN [dbo].[LeaveType] AS L ON L.Id=LT.LTSystemID
									LEFT JOIN EmployeeInformation E ON E.SystemId=LT.EmpSystemID
                                    WHERE LT.GroupID='" + companyGroupId + @"' AND E.CompanyId='" + companyId + @"' -- AND LT.PlantID='" + plantId + @"'
                                    AND LT.EmpSystemID='" + employeeId + @"'
                                    AND L.LeaveType<>'Maternity' AND (LT.FromDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
                                    OR LT.ToDate BETWEEN '" + fromDate + @"' AND '" + toDate + "')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> LoadGrdAllocatedLvDetails(string companyGroupId, string plantId, string employeeId, string calanderYearId)
        {
            //DataSet dsLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            try
            {
                var dsLvAllo = GetLeaveBalanceTypeNew(companyGroupId, plantId, employeeId, calanderYearId);

                dvLocal = new DataView();
                dvLocal.Table = dsLvAllo.Tables[0];
                bool proDataPrevYear = false;
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
                        //proDataPrevYear = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsProrataPreviousyear"].ToString());
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

                                //drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim());
                                //drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());
                                if (IsBroughtForwardAdd)
                                {

                                    drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) + BroughtForward;
                                    drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) + BroughtForward - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());
                                    //TotalEarn = BroughtForward + DaysCanBeSanctioned;
                                }
                                else
                                {
                                    //TotalEarn = DaysCanBeSanctioned;
                                    drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim());
                                    drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());
                                }


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
                //dsLvAllo = null;
            }
        }//End Function

        public IEnumerable<object> LoadGrdAllocatedLvDetailsNew_(string companyGroupId, string plantId, string employeeId, string calanderYearId)
        {
            //DataSet dsLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            try
            {
                var dsLvAllo = GetLeaveBalanceTypeNew(companyGroupId, plantId, employeeId, calanderYearId);
//                var dsLvAllo = GetLeaveBalanceType(companyGroupId, plantId, employeeId, calanderYearId);

                dvLocal = new DataView();
                dvLocal.Table = dsLvAllo.Tables[0];
                bool proDataPrevYear = false;
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
                        //proDataPrevYear = Convert.ToBoolean(dsLvAllo.Tables[0].Rows[i]["IsProrataPreviousyear"].ToString());
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
                        if (dsLvAllo.Tables[0].Rows[i]["LeaveType"].ToString().Trim().ToUpper() == "EARN")
                        {
                            IsBroughtForwardAdd = true;
                        }
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

                                //drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim());
                                //drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim());
                                if (IsBroughtForwardAdd)
                                {

                                    drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) + BroughtForward;
                                    drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) + BroughtForward - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Availed"].ToString().Trim());
                                    //TotalEarn = BroughtForward + DaysCanBeSanctioned;
                                }
                                else
                                {
                                    //TotalEarn = DaysCanBeSanctioned;
                                    drLocal["LeaveDays"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim());
                                    drLocal["Balance"] = Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["CurrentAllocationDCBS"].ToString().Trim()) - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Availed"].ToString().Trim());
                                }


                                #endregion
                            }
                            else
                            {
                                #region 02

                                drLocal["LeaveDays"] = TotalEarn;
                                drLocal["Balance"] = TotalEarn - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Availed"].ToString().Trim());

                                #endregion
                            }
                        }
                        else
                        {
                            drLocal["LeaveDays"] = TotalEarn;
                            //drLocal["Balance"] = TotalEarn - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Applied"].ToString().Trim()) - EncashedInbetween;
                            drLocal["Balance"] = TotalEarn - Convert.ToDecimal(dsLvAllo.Tables[0].Rows[i]["Availed"].ToString().Trim()) - EncashedInbetween;

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
                //dsLvAllo = null;
            }
        }//End Function

        public IEnumerable<object> LoadGrdAllocatedLvDetailsNew(string companyGroupId, string plantId, string employeeId, string calanderYearId)
        {
            DataRow drLocal = null;
            DataView dvLocal = null;
            try
            {
                var dsLvAllo = GetLeaveBalanceTypeNew(companyGroupId, plantId, employeeId, calanderYearId);

                dvLocal = new DataView();
                dvLocal.Table = dsLvAllo.Tables[0];
                List<object> ss = new List<object>();

                object ob = new object { };              

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
                //dsLvAllo = null;
            }
        }//End Function

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

        public void DeleteGraph(string id)
        {
            var from_db = Find(id);

            var detailsData = _leaveTransactionDetailsService.Query(r => r.LvTrnsSystemID == id).Select().ToList();
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                if (from_db.IsApproved)
                {
                    throw new CustomException("Leave already approved.");
                }

                foreach (var item in detailsData)
                {
                    _leaveTransactionDetailsService.Delete(item.SystemID);
                }

                base.DeleteGraph(from_db);
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteApprovedLeaveGraph(string id, string EmpSystemid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var from_db = Find(id);

            var detailsData = _leaveTransactionDetailsService.Query(r => r.LvTrnsSystemID == id).Select().ToList();
            var flag = false;
            try
            {
                //New Code
                DateTime FD = Convert.ToDateTime(from_db.FromDate.ToString());
                DateTime TD = Convert.ToDateTime(from_db.ToDate.ToString());

                DataSet PlantLock;
                PlantLockCheck(FD.ToString("dd-MMM-yyyy"), TD.ToString("dd-MMM-yyyy"), out PlantLock, identity.PlantId);
                string pl = "";
                if (PlantLock.Tables[0].Rows.Count > 0)
                {
                    for (var i = 0; i < PlantLock.Tables[0].Rows.Count; i++)
                    {
                        pl = pl + " " + PlantLock.Tables[0].Rows[i]["LockedDate"].ToString() + ", ";
                    }

                    throw new Exception("The Plant is Locked for - " + pl);
                }

                //new Code Ends


                DateTime FromDateV = Convert.ToDateTime(from_db.FromDate.ToString());
                DateTime ToDateV = Convert.ToDateTime(from_db.ToDate.ToString());




                _unitOfWork.BeginTransaction();

                foreach (var item in detailsData)
                {
                    _leaveTransactionDetailsService.Delete(item.SystemID);
                }

                base.DeleteGraph(from_db);
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();


                // New Code

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from AttdnProcessData where WorkDate between '" + FD + @"' and '" + TD + @"'AND PlantID='" + identity.PlantId + @"' and EmpSystemID='" + EmpSystemid + "'", out dsMaster, false, "1");
                string RowsEdit = "''";

                while (FD <= TD)
                {
                    string newformat = Convert.ToDateTime(FD).ToString("yyyyMMdd");

                    if (FD <= DateTime.Now)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "RowId='" + newformat + EmpSystemid + "'";
                        int j = dsMaster.Tables[0].DefaultView.Count;
                        if (j > 0)
                        {
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["LeaveStatus"] = DBNull.Value;
                            dr["LTSystemID"] = DBNull.Value;
                            dr["ManualFlag"] = true;
                            dr["IsLock"] = false;
                            dr["LockedBy"] = DBNull.Value;
                            dr["LockedDate"] = DBNull.Value;
                            dr["OTComfirmBy"] = DBNull.Value;
                            dr["DateOTComfirm"] = DBNull.Value;
                            dr["IsOTComfirm"] = false;

                            #region OT Columns Nullified

                            dr["TargetOT"] = DBNull.Value;
                            dr["PlanOT"] = DBNull.Value;
                            dr["AppliedOTLimit"] = DBNull.Value;
                            dr["AllowedOTLimit"] = DBNull.Value;
                            dr["StandardOT"] = DBNull.Value;
                            dr["AdditionalOt"] = DBNull.Value;

                            #endregion

                            dr.EndEdit();
                            RowsEdit = RowsEdit + ",'" + dr["RowId"].ToString() + "'";
                        }
                    }
                    FD = FD.AddDays(1);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                NewAttendanceProcessService ap = new NewAttendanceProcessService();
                ap.ManualScheduler(identity.PlantId, RowsEdit);

                //New Code Ends

            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }



        public void GetYearlyCalendarDetails(string YearId, out DataSet dsYear)
        {

            try
            {

                ConnectionManager.DAL.ConManager objCon;
                string sql = @"SELECT YearNo,FORMAT(FromDate,'dd-MMM-yyyy')  FromDate,FORMAT(ToDate,'dd-MMM-yyyy')  ToDate, IsYearEndClosed, PlantId, Id FROM YearlyCalendar WHERE Id='" + YearId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsYear, false, "1");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void PlantLockCheck(string FDate, string TDate, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string From = Convert.ToDateTime(FDate).ToString("dd-MMM-yyyy");
                string To = Convert.ToDateTime(TDate).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate between '" + From + "' and '" + To + "' and IsActive='1'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}
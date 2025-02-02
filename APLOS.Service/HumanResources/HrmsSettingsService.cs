#region Using

using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Model.Employees;
using Library.Model.Setups;
using Library.Service.Currencies;
using Library.Service.Employees;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using static Library.Service.Helpers.ReportUtility;

#endregion Using

namespace Library.Service.HumanResources
{
    public class HrmsSettingsService : IHrmsSettingsService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        // private readonly IRepositoryAsync<AttdnProcessData> attdnProcessData;
        private readonly IRepositoryAsync<AttdnProcessData> _recruitmentPlanningProcessSetRepository;

        public HrmsSettingsService(

             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            // , IRepositoryAsync<AttdnProcessData> _attdnProcessData
            , IRepositoryAsync<AttdnProcessData> recruitmentPlanningProcessSetRepository

            )
        {

            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            // _attdnProcessData = attdnProcessData;
            _recruitmentPlanningProcessSetRepository = recruitmentPlanningProcessSetRepository;

        }

        #endregion

        #region Attendance Lock and Un-lock
        #region Validation Method
        public IEnumerable<object> GetUnApprovedEmployeeListData(string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"Select EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section
                                From EmployeeInformation EI
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                Left join HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId
                                Where ---DOJ between    DATEFROMPARTS(year('" + lockDate + "'),month('" + lockDate + "'),1)   and '" + lockDate + @"'  AND 
                                EI.isApproved=0 AND EI.EmployeeStatus !='Separated'  AND DOJ <= '" + lockDate + @"' AND  EI.PlantId='" + identity.PlantId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetUnApprovedEmployeeListData(string EmpSystemId, string[] lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"Select EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section
                                From EmployeeInformation EI
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                Left join HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId
                                Where ---DOJ between    DATEFROMPARTS(year('" + lockDate + "'),month('" + lockDate + "'),1)   and '" + lockDate + @"'  AND 
                                EI.isApproved=0 AND EI.EmployeeStatus !='Separated'  AND DOJ <= '" + lockDate.Last() + @"' AND  EI.PlantId='" + identity.PlantId + @"'  AND EI.SystemID='" + EmpSystemId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetOTConfirmationData(string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"select AP.EmpSystemId, FORMAT(AP.WorkDate,'dd-MMM-yyyy') WorkDate
                                ,EI.EmployeeCode,EI.EmployeeName 
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation
                                from AttdnProcessData AP
                                INNER JOIN EmployeeInformation EI ON EI.SystemId=AP.EmpSystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                Left join HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                Where AP.WorkDate ='" + lockDate + "' and AP.IsOTComfirm=0 and  AP.DayStatus in (select DayType from DayType where category in ('Present','Late')) and AP.DayStatus not in ('RST')  and AP.IsOTEntitled=1 and EI.PlantID='" + identity.PlantId + "' order by AP.WorkDate,EI.EmployeeCode";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetOTConfirmationDataForZeroAuto(string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"select AP.EmpSystemId, FORMAT(AP.WorkDate,'dd-MMM-yyyy') WorkDate
                                ,EI.EmployeeCode,EI.EmployeeName 
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation
                                from AttdnProcessData AP
                                INNER JOIN EmployeeInformation EI ON EI.SystemId=AP.EmpSystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                Left join HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                Where AP.WorkDate ='" + lockDate + "' AND AP.OTHr>0 and AP.IsOTComfirm=0 and  AP.DayStatus in (select DayType from DayType where category in ('Present','Late')) and AP.DayStatus not in ('RST')  and AP.IsOTEntitled=1 and EI.PlantID='" + identity.PlantId + "' order by AP.WorkDate,EI.EmployeeCode";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetOutPunchMissingData(string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"select AP.EmpSystemId, FORMAT(AP.WorkDate,'dd-MMM-yyyy') WorkDate
                                ,EI.EmployeeCode,EI.EmployeeName 
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation, se.UserName Section
                                , Sus.UserName SubSection
                                from AttdnProcessData AP
                                INNER JOIN EmployeeInformation EI ON EI.SystemId=AP.EmpSystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                Left join HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                --LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                --LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                                LEFT join [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                                left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                                LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
								LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                Where AP.WorkDate ='" + lockDate + "' and  AP.outtime IS  NULL and  AP.DayStatus in (select DayType from DayType where category in ('Present','Late')) and AP.DayStatus not in ('RST')   and EI.PlantID='" + identity.PlantId + "' order by AP.WorkDate,EI.EmployeeCode";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetOutPunchMissingDataForAlert(string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @" Select AP.EmpSystemId, FORMAT(AP.WorkDate,'dd-MMM-yyyy') WorkDate
                                ,FORMAT(AP.InTime,'HH:mm tt') InTime
                                ,FORMAT(AP.OutTime,'HH:mm tt') OutTime
                                ,AP.DayStatus
                                ,EI.EmployeeCode,EI.EmployeeName 
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation
                                ,se.UserName Section
                                ,Sus.UserName SubSection
                                from AttdnProcessData AP
                                INNER JOIN EmployeeInformation EI ON EI.SystemId=AP.EmpSystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                Left join HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                --LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                --LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId


                                LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                                LEFT join [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                                left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                                left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                                LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                WHERE AP.OutTime is not null and DayStatus='A' and WorkDate='" + lockDate + "' and AP.PlantID='" + identity.PlantId + "'";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetOTConfirmationData(string EmpSystemId, string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"select AP.EmpSystemId, FORMAT(AP.WorkDate,'dd-MMM-yyyy') WorkDate
                                ,EI.EmployeeCode,EI.EmployeeName 
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation
                                from AttdnProcessData AP
                                INNER JOIN EmployeeInformation EI ON EI.SystemId=AP.EmpSystemId
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                Left join HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                Where AP.WorkDate IN(" + lockDate + @") and AP.IsOTComfirm=0 and  AP.DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                AND AP.DayStatus not in ('RST') and AP.IsOTEntitled=1  AND EI.SystemID='" + EmpSystemId + @"' 
                                order by AP.WorkDate,EI.EmployeeCode";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetShiftNotAssignData(string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"  SELECT DISTINCT E.SystemId
	                            ,E.EmployeeName
	                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
	                            ,E.EmployeeCode
	                            --,LTRIM(RIGHT(CONVERT(VARCHAR(25), EDWSA.ShiftInTime, 100), 7)) ShiftInTime
	                            ,cg.Id CompanyGroupId
	                            ,cg.UserName GroupName
	                            ,ISNULL(Line.UserName, '-') Line
	                            ,C.Id AS CompanyId
	                            ,C.UserName CompanyName,ld.UserName LegalDesignation
                           FROM EmployeeInformation E left join org.Company C on c.Id=e.CompanyId
                            left JOIN ORG.CompanyGroup CG ON CG.Id = c.CompanyGroupId
                         
                            LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
                            LEFT JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
                            LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            Left join HKP.LegalDesignation ld on ld.Id=E.LegalDesignationId
                            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            LEFT JOIN DayType DT ON DT.DayType = APD.DayStatus
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            LEFT JOIN ORG.Line Line ON Line.Id = MB.LineId
                            WHERE E.PlantID = '" + identity.PlantId + @"'
                            and E.SystemId NOT IN (
							SELECT DISTINCT EmpSystemID
			                            FROM EmployeeShiftAssign)
                                AND (
		                            E.EmployeeStatus != 'Separated'
		                            OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate + @"')
		                            )
	                            AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate + @"')";







                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetShiftNotAssignData(string EmpSystemId, string[] lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"  SELECT DISTINCT E.SystemId
	                            ,E.EmployeeName
	                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
	                            ,E.EmployeeCode
	                            --,LTRIM(RIGHT(CONVERT(VARCHAR(25), EDWSA.ShiftInTime, 100), 7)) ShiftInTime
	                            ,cg.Id CompanyGroupId
	                            ,cg.UserName GroupName
	                            ,ISNULL(Line.UserName, '-') Line
	                            ,C.Id AS CompanyId
	                            ,C.UserName CompanyName,ld.UserName LegalDesignation
                            FROM ORG.CompanyGroup CG
                            INNER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                            INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id
	                            AND c.Id = E.CompanyId
                            INNER JOIN (
	                            --*
	                            SELECT SystemID
	                            FROM EmployeeInformation EI
	                            WHERE EI.SystemID NOT IN (
			                            --**
			                            SELECT DISTINCT EmpSystemID
			                            FROM EmployeeShiftAssign
				                            --WHERE CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + lockDate + @"')
			                            ) --**
	                            ) --*
	                            ESA ON E.SystemId = ESA.SystemId
                            LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
                            LEFT JOIN ShiftDefination SD ON SD.SystemID = APD.ShiftSystemID
                            LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            Left join HKP.LegalDesignation ld on ld.Id=E.LegalDesignationId
                            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            LEFT JOIN DayType DT ON DT.DayType = APD.DayStatus
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            LEFT JOIN ORG.Line Line ON Line.Id = MB.LineId
                            WHERE E.GroupID = '" + identity.CompanyGroupId + @"'

                              --  AND (
		                       --     E.EmployeeStatus != 'Separated'
		                       --     OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate.Last() + @"')
		                      --      )
	                           --- AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate.First() + @"')  
                            AND E.SystemID='" + EmpSystemId + "'";







                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAttdencenotNotProcData(string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                var _sql = @"SELECT DISTINCT E.SystemId
	                            ,E.EmployeeName
	                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
	                            ,E.EmployeeCode
	                            ,cg.Id CompanyGroupId
	                            ,cg.UserName GroupName
	                            ,ISNULL(SD.ShiftDefinationName, '') ShiftDefinationName
	                            ,LTRIM(RIGHT(CONVERT(VARCHAR(25), SD.InTime, 100), 7)) ShiftInTime
	                            ,C.Id AS CompanyId
	                            ,C.UserName CompanyName
	                            ,ISNULL(Line.UserName, '-') Line
	                            ,Plant.UserName Plant
	                            ,Division.UserName Division
	                            ,Unit.UserName Unit
	                            ,Department.UserName Department
	                            ,Section.UserName Section
	                            ,SubSection.UserName SubSection ,ld.UserName LegalDesignation
                       FROM EmployeeInformation E left join org.Company C on c.Id=e.CompanyId
                            left JOIN ORG.CompanyGroup CG ON CG.Id = c.CompanyGroupId
                           
                            LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
                            LEFT JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = E.SystemID AND CONVERT(DATE, EDWSA.WorkDate) = CONVERT(DATE, '" + lockDate + @"')
	
                            LEFT JOIN ShiftDefination SD ON SD.SystemID = EDWSA.ShiftSystemID
                            LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            Left join HKP.LegalDesignation ld on ld.Id=E.LegalDesignationId
                            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            LEFT JOIN [ORG].[Plant] ON Plant.Id = ENT.PlantId
                            LEFT JOIN ORG.Line Line ON Line.Id = MB.LineId
                            LEFT JOIN [ORG].[Division] ON Division.Id = POS.DivisionId
                            LEFT JOIN [ORG].[Unit] ON Unit.Id = ENT.UnitId
                            LEFT JOIN [ORG].[Department] ON Department.Id = POS.DepartmentId
                            LEFT JOIN [ORG].[Section] ON Section.Id = POS.SectionId
                            LEFT JOIN [ORG].[SubSection] ON SubSection.Id = POS.SubSectionId
                            WHERE E.PlantId = '" + identity.PlantId + @"'
	                       
							and isnull(E.SystemId,'') IN (	SELECT distinct es.EmpSystemID
	                          FROM EmployeeShiftAssign es
							  left join EmployeeInformation ex on ex.SystemId=es.EmpSystemID
	                            WHERE EffectiveDate <= GETDATE() and ex.PlantId='" + identity.PlantId + @"'
		                            AND isnull(EmpSystemID,'') NOT IN (
			                          
			                            SELECT DISTINCT isnull(EmpSystemID,'')
			                            FROM AttdnProcessData p
										left join EmployeeInformation ex on ex.SystemId=p.EmpSystemID
			                            WHERE CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + lockDate + @"')
										and ex.PlantId='" + identity.PlantId + @"'
			                            )  
										)                
						  
                            
								 AND (
		                            E.EmployeeStatus != 'Separated'
		                            OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate + @"')
		                            )
	                            AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate + @"')";


                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetAttdencenotNotProcData(string EmpSystemId, string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"SELECT DISTINCT E.SystemId
	                            ,E.EmployeeName
	                            ,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 106), ' ', '-') DOJ
	                            ,E.EmployeeCode
	                            ,cg.Id CompanyGroupId
	                            ,cg.UserName GroupName
	                            ,ISNULL(SD.ShiftDefinationName, '') ShiftDefinationName
	                            ,LTRIM(RIGHT(CONVERT(VARCHAR(25), SD.InTime, 100), 7)) ShiftInTime
	                            ,C.Id AS CompanyId
	                            ,C.UserName CompanyName
	                            ,ISNULL(Line.UserName, '-') Line
	                            ,Plant.UserName Plant
	                            ,Division.UserName Division
	                            ,Unit.UserName Unit
	                            ,Department.UserName Department
	                            ,Section.UserName Section
	                            ,SubSection.UserName SubSection ,ld.UserName LegalDesignation
                            FROM ORG.CompanyGroup CG
                            INNER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                            INNER JOIN EmployeeInformation E ON e.GroupID = CG.Id 	AND c.Id = E.CompanyId
                            INNER JOIN (
	                            --*
	                            SELECT TOP 1
	                            WITH TIES *
	                            FROM EmployeeShiftAssign
	                            WHERE EffectiveDate <= GETDATE()
		                            AND EmpSystemID NOT IN (
			                            --**
			                            SELECT DISTINCT EmpSystemID
			                            FROM AttdnProcessData
			                            WHERE WorkDate IN (" + lockDate + @")                      ---CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + lockDate + @"')
			                            )
	                            ORDER BY ROW_NUMBER() OVER (
			                            PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC
			                            )
	                            ) -- *
	                            ESA ON E.SystemId = ESA.EmpSystemID
                            LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = ESA.EmpSystemID
                            LEFT JOIN EmpDateWiseShiftAssign EDWSA ON EDWSA.EmpSystemID = E.SystemID AND  EDWSA.WorkDate IN (" + lockDate + @")
	
                            LEFT JOIN ShiftDefination SD ON SD.SystemID = EDWSA.ShiftSystemID
                            LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            Left join HKP.LegalDesignation ld on ld.Id=E.LegalDesignationId
                            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            LEFT JOIN ORG.Line Line ON Line.Id = MB.LineId
                            LEFT JOIN [ORG].[Plant] ON Plant.Id = ENT.PlantId
                            LEFT JOIN [ORG].[Division] ON Division.Id = ENT.DivisionId
                            LEFT JOIN [ORG].[Unit] ON Unit.Id = ENT.UnitId
                            LEFT JOIN [ORG].[Department] ON Department.Id = POS.DepartmentId
                            LEFT JOIN [ORG].[Section] ON Section.Id = POS.SectionId
                            LEFT JOIN [ORG].[SubSection] ON SubSection.Id = POS.SubSectionId
                            WHERE E.GroupID = '" + identity.CompanyGroupId + @"'  AND E.SystemID='" + EmpSystemId + @"'

                                AND E.CompanyId = '" + identity.CompanyId + @"'
	                          ---  AND (
		                         --   E.EmployeeStatus != 'Separated'
		                           -- OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate + @"')
		                        --    )
	                         --   AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate + @"')";















                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public IEnumerable<object> GetUnLockEmployeeListData(string lockDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"Select EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section,Al.IsActive
                                From EmployeeInformation EI
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId
                                LEFT JOIN [PlantWiseAttendanceLock] al ON al.EmpSystemId=EI.SystemId AND Al.LockedDate='" + lockDate + @"'
                                Where ---DOJ between    DATEFROMPARTS(year('" + lockDate + "'),month('" + lockDate + "'),1)   and '" + lockDate + @"'  AND 
                                EI.isApproved=0 AND EI.EmployeeStatus !='Separated'  AND DOJ <= '" + lockDate + @"' AND  EI.PlantId='" + identity.PlantId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetAllEmployeeListData(string fromdate, string todate, string plantId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"Select EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section---,Al.IsActive
                                From EmployeeInformation EI
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId
                                --LEFT JOIN [PlantWiseAttendanceLock] al ON al.EmpSystemId=EI.SystemId
                                Where
                                DOJ<='" + todate + @"' AND (DOS is null OR DOS>= '" + fromdate + @"')
                                AND  EI.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetEmployeeWiseLockData(string empsystemid, string fromdate, string todate, string plantId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"  SELECT  0 CheckBoxSelect ,EmpSystemID,FORMAT(WorkDate,'dd-MMM-yyyy')  WorkDate
                                , IsLock,LockedBy
                                ,FORMAT(lockedDate,'dd-MMM-yyyy HH:mm:ss') lockedDate
                                ,IsCheckBoxEnable=CASE WHEN ISNULL(IsLock,0)=1 THEN CONVERT(BIT, 0) ELSE CONVERT(BIT, 1) END
                                 FROM AttdnProcessData 
                               where ----PlantId='" + plantId + @"' AND
                               WorkDate between '" + fromdate + @"' and '" + todate + @"' 
                               AND EmpSystemID='" + empsystemid + @"'";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetLockEmployeeList(string FromDate, string ToDate, CustomIdentity identity)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"SELECT DISTINCT  0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                            ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                            ,FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                            ,DG.UserName GivenDesignation
                            ,DP.UserName Department
                            ,PR.UserName PositionName
                            ,DSG.UserName Designation
                            ,PR.DesignationId
                            ,PG.StandardName PayRollGroupName
                            ,PG.Id PayRollGroupId
                            ,ld.UserName LegalDesignation
                            ,Section.UserName Section,J.JobLocation
                            From AttdnProcessData AS apd                            
                            LEFT JOIN ExceptionEmployeeAttendanceUnlock AS eeau ON eeau.EmpSystemId = apd.EmpSystemId AND eeau.WorkDate=apd.WorkDate
                            INNER JOIN  EmployeeInformation EI ON EI.SystemId = apd.EmpSystemID
                            LEFT JOIN JobLocation J ON J.systemid = EI.JobLocationID
                            LEFT JOIN [PlantWiseAttendanceLock] al ON al.PlantID = EI.PlantID AND  al.LockedDate = apd.WorkDate
                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                            LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                            LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                            LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                            LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                            LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                            LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                            LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                            LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId

                            WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' ---AND apd.PlantId='" + identity.PlantId + @"'
                            AND al.IsActive=1  
                            AND eeau.EmpSystemId  IS NULL
                            AND DOJ<='" + ToDate + @"' AND (DOS is null OR DOS>= '" + FromDate + @"') AND  EI.PlantId='" + identity.PlantId + @"'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetReLockEmployeeList(string FromDate, string ToDate, CustomIdentity identity)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @"SELECT DISTINCT  0 CheckBoxSelect, EI.SystemID,EI.EmployeeCode
                            ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                            ,FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                            ,DG.UserName GivenDesignation
                            ,DP.UserName Department
                            ,PR.UserName PositionName
                            ,DSG.UserName Designation
                            ,PR.DesignationId
                            ,PG.StandardName PayRollGroupName
                            ,PG.Id PayRollGroupId
                            ,ld.UserName LegalDesignation
                            ,Section.UserName Section,J.JobLocation
                            From AttdnProcessData AS apd                            
                            LEFT JOIN ExceptionEmployeeAttendanceUnlock AS eeau ON eeau.EmpSystemId = apd.EmpSystemId AND eeau.WorkDate=apd.WorkDate
                            INNER JOIN  EmployeeInformation EI ON EI.SystemId = apd.EmpSystemID
                            LEFT JOIN JobLocation J ON J.systemid = EI.JobLocationID
                            LEFT JOIN [PlantWiseAttendanceLock] al ON al.PlantID = EI.PlantID AND  al.LockedDate = apd.WorkDate
                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                            LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                            LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                            LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                            LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                            LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                            LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                            LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                            LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId

                            WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' ---AND apd.PlantId='" + identity.PlantId + @"'
                            AND al.IsActive=1  
                            AND eeau.EmpSystemId  IS NOT NULL
                            AND DOJ<='" + ToDate + @"' AND (DOS is null OR DOS>= '" + FromDate + @"') AND  EI.PlantId='" + identity.PlantId + @"'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetDailyAllowanceTransactionPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyAllowanceTransaction", out idFromDB);
            systemID = "DAT-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        public void CreateLockData(string lockDate)
        {

            //if (ProcessUtility.ProcessLocked(ProcessFlag.AttendanceLock) == true)
            //    throw new Exception("Another process is running. Please try again later");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsDailyAllowance odailyAllowance = new clsDailyAllowance();
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsOTUnConfirmed;
            DataSet dsUnApprovedList;
            //DataSet dsMaster;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            bool IsOTConfirmationAuto = false;
            bool IsOTConfirmationAutoForZeroAuto = false;
            bool IsOTConfirmationAutoException = false;
            bool IsOTConfirmationAfterLock = false;



            try
            {
                objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim()))
                    {
                        IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());

                    }
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                    {
                        IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                    }
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                    {
                        IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                    }
                }




                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }
                //GetUnApprovedEmployeeListData(lockDate, out dsUnApprovedList);
                //if (dsUnApprovedList.Tables[0].Rows.Count > 0)
                //{
                //    string UnApprovedEmpList = string.Empty;
                //    foreach (DataRow item in dsUnApprovedList.Tables[0].Rows)
                //    {
                //        UnApprovedEmpList = UnApprovedEmpList + item["EmployeeCode"].ToString() + " - " + item["EmployeeName"].ToString() + "</br>";
                //    }


                //    throw new Exception(UnApprovedEmpList);

                //}
                var UnApprovedEmployeeListData = GetUnApprovedEmployeeListData(lockDate);
                if (UnApprovedEmployeeListData.Count() > 0)
                {

                    throw new Exception("Please Confirmed all Employees  Approved.");

                }


                if (IsOTConfirmationAfterLock == false)
                {
                    var OTConfirmationData = GetOTConfirmationData(lockDate);
                    if (OTConfirmationData.Count() > 0)
                    {

                        throw new Exception("Please Confirmed all Employees OT.");

                    }

                }






                var ShiftNotAssignData = GetShiftNotAssignData(lockDate);
                if (ShiftNotAssignData.Count() > 0)
                {

                    throw new Exception("Please Confirmed all Employees Shift Assign.");

                }
                var AttdencenotNotProcData = GetAttdencenotNotProcData(lockDate);
                if (AttdencenotNotProcData.Count() > 0)
                {

                    throw new Exception("Please Confirmed all Employees Attdence Proc.");

                }

                SaveDailyAllowanceTransaction(lockDate);
                //odailyAllowance.UpdateDailyAllowanceSummaryData(identity, lockDate);
                string sql = @"SELECT [Id]
                                    ,[PlantId]
                                    ,[LockedDate]
                                    ,[AddedBy]
                                    ,[AddedDate]
                                    ,[AddedFromIP]
                                    ,[UpdatedBy]
                                    ,[UpdatedDate]
                                    ,[UpdatedFromIP],[IsActive]
                                     FROM [PlantWiseAttendanceLock] where LockedDate='" + lockDate + "' AND PlantId='" + identity.PlantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PLANTWISEATTENDANCELOCK", out sID);
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "PAL" + sID;
                    dr["PlantId"] = identity.PlantId;
                    dr["IsActive"] = true;
                    dr["LockedDate"] = lockDate;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);


                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["PlantId"] = identity.PlantId;
                    dr["IsActive"] = true;
                    dr["LockedDate"] = lockDate;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);



                string sqld = @"DELETE FROM ExceptionEmployeeAttendanceUnlock where WorkDate='" + lockDate + "' AND  EmpSystemId IN (select SystemID from EmployeeInformation where PlantID='" + identity.PlantId + "')";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqld, out dsMaster, false, "1");
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {
                //ProcessUtility.ProcessUnlock(ProcessFlag.AttendanceLock);
            }
        }

        public void DeleteDailyAllowanceTransaction(string lockDate, string plantId)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[DailyAllowanceTransaction] WHERE EmpSystemId IN (select SystemID from EmployeeInformation where PlantID='" + plantId + "') AND WorkDate='" + lockDate + "'";
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
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        public void DeleteDailyAllowanceTransactionEmpWise(string lockDate, string plantId, string EmpIds)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[DailyAllowanceTransaction] WHERE  WorkDate='" + lockDate + "' and EmpSystemId IN (" + EmpIds + @")";
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
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function
        public void GetSalaryHead(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryHead";

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
        public void GetMultipleEmployeeSalaryData(string PlantId, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM ( SELECT (x.EffectiveDate) EffectiveDate,m.SystemID,m.EmpInfoSystemID from (
												select max(	EffectiveDate) 	EffectiveDate,EmpInfoSystemID FROM (
																	SELECT   EffectiveDate   ,EmpInfoSystemID
																	FROM SalaryInfoDefineMaster  
																	WHERE IsApproved =1 AND EffectiveDate<= '" + sEffectiveDate + @"' AND EmpInfoSystemID IN (select SystemID from EmployeeInformation where  PlantID='" + PlantId + @"')  
																	union
																	SELECT  EffectiveDate  ,EmpInfoSystemID
																	FROM SalaryInfoBackMaster  
																	WHERE IsApproved =1 AND EffectiveDate<= '" + sEffectiveDate + @"' AND EmpInfoSystemID IN (select SystemID from EmployeeInformation where  PlantID='" + PlantId + @"')  
 	 												) zz GROUP BY EmpInfoSystemID		
											) x
						
						INNER JOIN (
							 SELECT  EffectiveDate,SystemID,EmpInfoSystemID
							   FROM SalaryInfoDefineMaster  
							  WHERE    IsApproved =1 
                        union
                        SELECT  EffectiveDate,SystemID ,EmpInfoSystemID
								FROM SalaryInfoBackMaster  
                                WHERE IsApproved =1 
						) m ON m.EffectiveDate=x.EffectiveDate AND m.EmpInfoSystemID= x.EmpInfoSystemID ) mas
						INNER JOIN (
						SELECT s.SystemID,s.SalaryID,s.SalaryHeadID,s.EntryCurrencyID,s.EntryAmount,s.DefineCurrencyID,s.DefineAmount,s.AmtDefinitionCurrencyID,s.AmtDefinitionRate,s.SequenceNo,s.SalaryCategory
                        ,sh.HeadCategory,sh.SalaryHead  FROM SalaryInfoDefine s
						LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID 
						UNION
						SELECT sb.SystemID,sb.SalaryID,sb.SalaryHeadID,sb.EntryCurrencyID,sb.EntryAmount,sb.DefineCurrencyID,sb.DefineAmount,sb.AmtDefinitionCurrencyID,sb.AmtDefinitionRate,sb.SequenceNo,sb.SalaryCategory
                        ,sh.HeadCategory,sh.SalaryHead FROM  SalaryInfoBack sb
						LEFT JOIN SalaryHead AS sh on sb.SalaryHeadID=sh.SalaryHeadID
                        ) d ON mas.SystemID=d.SalaryID   ORDER BY mas.EmpInfoSystemID ";

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
        public void ReLoadFormulaValueNew(string strFormulaID, string sLocalCurrencyID, string sForeignCurRate,
        out string sFormulaValue, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSlrHd)
        {
            DataSet dsLocal = null;
            //DataView dvLocal = null;
            //DataView dvSlrHd = null;
            string strTemp = "";

            try
            {

                dsLocal = new DataSet();
                string strFormulaIDTemp = strFormulaID.Trim();
                //string sLocalCurrencyID = para.lblLocalCurrencyID;
                //string sForeignCurRate = para.lblLocalCurRate;

                if (sForeignCurRate == "")
                { sForeignCurRate = "1"; }

                sFormulaValue = "";

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
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == ">" || strTemp.Trim() == "<" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {

                        var dtv = dtValue.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                        if (dtv.Count() > 0)
                        {

                            if (dtv[0].EntryCurrencyID == sLocalCurrencyID)
                            {
                                strTemp = dtv[0].EntryAmount;
                                strTemp = GetAbsValue(strTemp);
                            }
                            else
                            {
                                strTemp = (Convert.ToDecimal(dtv[0].EntryAmount) * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                strTemp = " " + GetAbsValue(strTemp) + " ";
                            }


                        }
                        else
                        {
                            var dicsh = dicSlrHd.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                            if (dicsh.Count() > 0)
                            {
                                strTemp = "0.00";
                            }
                        }


                    }


                    sFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 


        string GetAbsValue(string strTemp)
        {
            try
            {
                var vv = Math.Abs(Convert.ToDecimal(strTemp.Trim()));
                string _vv = vv.ToString();
                return _vv;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //static public string ReplaceWholeWord(this string original, string wordToFind, string replacement, RegexOptions regexOptions = RegexOptions.None)
        //{
        //    string pattern = String.Format(@"\b{0}\b", wordToFind);
        //    string ret = Regex.Replace(original, pattern, replacement, regexOptions);
        //    return ret;
        //}
        public void SaveDailyAllowanceTransaction(string lockDate)
        {
            try
            {
                int DaysInaMonth = DateTime.DaysInMonth(Convert.ToDateTime(lockDate).Year, Convert.ToDateTime(lockDate).Month);
                //string input = "doin' some replacement";
                string pattern = @"DaysInaMonth";
                string replace = DaysInaMonth.ToString();
                //string result = Regex.Replace(input, pattern, replace);




                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsSalaryDataEmpWise = null;
                GetMultipleEmployeeSalaryData(identity.PlantId, lockDate, out dsSalaryDataEmpWise);
                Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                string _empId = "";
                List<DataRow> _data = new List<DataRow>();
                for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                {
                    if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                        _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    }
                    _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                }

                DataSet dsSalHd = null;
                //DataTable dtSlrHd = null;
                string _formulaValue = string.Empty;
                string sFormulaResult = string.Empty;
                clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
                List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                GetSalaryHead(out dsSalHd);
                DataView dvsh = new DataView(dsSalHd.Tables[0]);
                DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                if (dtSalHdx.Rows.Count > 0)
                    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();







                DeleteDailyAllowanceTransaction(lockDate, identity.PlantId);



                ConnectionManager.DAL.ConManager objCon;

                string _sql = "SELECT * FROM [dbo].[DailyAllowanceTransaction] WHERE  WorkDate='" + lockDate + "' AND  EmpSystemId IN (select SystemID from EmployeeInformation where PlantID='" + identity.PlantId + "')";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(_sql, out DataSet dsMaster, false, "1");



                var dailyAll = GetAllowanceDaily(lockDate, identity.PlantId);
                foreach (var data in dailyAll)
                {
                    var dailyAllowanceData = GetDailyAllowanceEmployeeList(lockDate, identity.PlantId, data.Id, data.Catagory);

                    DataSet dsRateFromSalaryRange = null;
                    GetRateFromSalaryRange(data.Id, identity.PlantId, out dsRateFromSalaryRange);


                    foreach (var item in dailyAllowanceData)
                    {


                        List<SPvalueHeadWise> dtValue = null;
                        decimal Totalvalue = 0;
                        decimal Ratevalue = 0;
                        decimal Quantity = 0;
                        string FormulaDesID = string.Empty;

                        if (Convert.ToBoolean(item.IsAllDesignation) == true)
                        {

                            if (Convert.ToBoolean(item.IsRateBasedOnSalaryRange) == true)///////RateBasedOnSalaryRang
                            {
                                //FormulaDesID = item.SalaryRangeBasedOnSalaryHeadId.ToString();
                                FormulaDesID = Regex.Replace(item.SalaryRangeBasedOnSalaryHeadId.ToString(), pattern, replace);
                                //FormulaDesID.ReplaceWholeWord("DaysInaMonth", DaysInaMonth.ToString());
                                if (DicAllEmpSalaryInfo.ContainsKey(item.EmpSystemId.ToString()) == false)
                                    continue;

                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[item.EmpSystemId.ToString()];


                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table




                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                }
                                try
                                {
                                    ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();



                                    if (dsRateFromSalaryRange.Tables[0].Rows.Count > 0)
                                    {
                                        DataView dvRateFromSalaryRange = new DataView(dsRateFromSalaryRange.Tables[0]);
                                        dvRateFromSalaryRange.RowFilter = "SalaryRangeLowerLimit<=" + sFormulaResult + " and " + sFormulaResult + "<=SalaryRangeUpperLimit";



                                        if (dvRateFromSalaryRange.Count > 0)
                                        {

                                            Ratevalue = Convert.ToDecimal(dvRateFromSalaryRange[0]["Rate"].ToString());
                                        }
                                        dvRateFromSalaryRange.RowFilter = null;
                                    }








                                    if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                    {

                                        Quantity = 1;

                                    }
                                    if (item.Catagory == "HourlyOffDuty")
                                    {

                                        Quantity = Convert.ToDecimal(item.OTDuration) / 60;

                                    }
                                    if (item.Catagory == "HourlyOffDutyDeduction")
                                    {

                                        Quantity = Convert.ToDecimal(item.DurationInMin) / 60;

                                    }
                                    Totalvalue = Ratevalue * Quantity;
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }///////RateBasedOnSalaryRang
                            }
                            else
                            {



                                if (Convert.ToBoolean(item.IsFixed) == true)
                                {
                                    Ratevalue = Convert.ToDecimal(item.Rate);
                                    if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                    {

                                        Quantity = 1;

                                    }
                                    if (item.Catagory == "HourlyOffDuty")
                                    {
                                        Quantity = Convert.ToDecimal(item.OTDuration) / 60;


                                    }
                                    if (item.Catagory == "HourlyOffDutyDeduction")
                                    {


                                        Quantity = Convert.ToDecimal(item.DurationInMin) / 60;

                                    }
                                    Totalvalue = Ratevalue * Quantity;
                                }
                                else
                                {
                                    //FormulaDesID = item.FormulaDesID.ToString();
                                    FormulaDesID = Regex.Replace(item.FormulaDesID.ToString(), pattern, replace);
                                    if (DicAllEmpSalaryInfo.ContainsKey(item.EmpSystemId.ToString()) == false)
                                        continue;

                                    List<DataRow> salaryStructure = DicAllEmpSalaryInfo[item.EmpSystemId.ToString()];


                                    #region Create Table                    
                                    dtValue = new List<SPvalueHeadWise>();
                                    #endregion Create Table




                                    for (int j = 0; j < salaryStructure.Count; j++)
                                    {
                                        SPvalueHeadWise sp = new SPvalueHeadWise();
                                        sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                        sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                        sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                        dtValue.Add(sp);
                                    }
                                    try
                                    {
                                        ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                        sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();



                                        //Totalvalue = Convert.ToDecimal(sFormulaResult) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString().Trim());
                                        Ratevalue = Convert.ToDecimal(sFormulaResult);
                                        if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                        {

                                            Quantity = 1;

                                        }
                                        if (item.Catagory == "HourlyOffDuty")
                                        {

                                            Quantity = Convert.ToDecimal(item.OTDuration) / 60;

                                        }
                                        if (item.Catagory == "HourlyOffDutyDeduction")
                                        {

                                            Quantity = Convert.ToDecimal(item.DurationInMin) / 60;

                                        }
                                        Totalvalue = Ratevalue * Quantity;
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToBoolean(item.DARIsFixed) == true)
                            {
                                //Totalvalue = Convert.ToDecimal(item.DARRate) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString());
                                Ratevalue = Convert.ToDecimal(item.DARRate);
                                if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                {

                                    Quantity = 1;

                                }
                                if (item.Catagory == "HourlyOffDuty")
                                {
                                    Quantity = Convert.ToDecimal(item.DurationInMin) / 60;


                                }
                                if (item.Catagory == "HourlyOffDutyDeduction")
                                {

                                    Quantity = Convert.ToDecimal(item.OTDuration) / 60;

                                }
                                Totalvalue = Ratevalue * Quantity;
                            }
                            else
                            {
                                //FormulaDesID = item.DARFormulaDesID.ToString();
                                FormulaDesID = Regex.Replace(item.DARFormulaDesID.ToString(), pattern, replace);
                                if (DicAllEmpSalaryInfo.ContainsKey(item.EmpSystemId.ToString()) == false)
                                    continue;

                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[item.EmpSystemId];


                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table




                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                }
                                try
                                {
                                    ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                                    //Totalvalue = Convert.ToDecimal(sFormulaResult) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString().Trim());

                                    Ratevalue = Convert.ToDecimal(sFormulaResult);
                                    if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                    {

                                        Quantity = 1;

                                    }
                                    if (item.Catagory == "HourlyOffDuty")
                                    {


                                        Quantity = Convert.ToDecimal(item.DurationInMin) / 60;
                                    }
                                    if (item.Catagory == "HourlyOffDutyDeduction")
                                    {
                                        Quantity = Convert.ToDecimal(item.OTDuration) / 60;


                                    }
                                    Totalvalue = Ratevalue * Quantity;

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }


                        }



                        //string _sql = "SELECT * FROM [dbo].[DailyAllowanceTransaction] WHERE EmpSystemId='" + item.EmpSystemId + "' AND WorkDate='" + lockDate + "' AND AllowanceDailyId='" + item.AllowanceDailyId + "'";
                        //objCon = new ConnectionManager.DAL.ConManager("1");
                        //objCon.OpenDataSetThroughAdapter(_sql, out DataSet dsMaster, false, "1");



                        DataView dvMastert = new DataView(dsMaster.Tables[0]);
                        dvMastert.RowFilter = "EmpSystemId='" + item.EmpSystemId + "' AND WorkDate='" + lockDate + "' AND AllowanceDailyId='" + item.AllowanceDailyId + "'";



                        if (dvMastert.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = GetDailyAllowanceTransactionPK();
                            dr["PlantId"] = identity.PlantId;
                            dr["WorkDate"] = lockDate;
                            dr["EmpSystemId"] = item.EmpSystemId;
                            dr["AllowanceDailyId"] = item.AllowanceDailyId;
                            dr["Quantity"] = Quantity;
                            dr["Rate"] = Ratevalue;
                            dr["Amount"] = Totalvalue;
                            dr["SalaryHeadId"] = item.SalaryHeadId;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;


                            dsMaster.Tables[0].Rows.Add(dr);
                        }


                        dvMastert.RowFilter = null;
                        // }


                    }
                }
                clsStaticInfo objt = new clsStaticInfo();
                objt.SaveDataSets(dsMaster);



            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SaveDailyAllowanceTransactionEmpWise(string lockDate, string EmpIds)
        {
            try
            {



                int DaysInaMonth = DateTime.DaysInMonth(Convert.ToDateTime(lockDate).Year, Convert.ToDateTime(lockDate).Month);
                //string input = "doin' some replacement";
                string pattern = @"DaysInaMonth";
                string replace = DaysInaMonth.ToString();
                //string result = Regex.Replace(input, pattern, replace);

                //================
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsSalaryDataEmpWise = null;
                GetMultipleEmployeeSalaryData(identity.PlantId, lockDate, out dsSalaryDataEmpWise);
                Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                string _empId = "";
                List<DataRow> _data = new List<DataRow>();
                for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                {
                    if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                        _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    }
                    _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                }

                DataSet dsSalHd = null;
                //DataTable dtSlrHd = null;
                string _formulaValue = string.Empty;
                string sFormulaResult = string.Empty;
                clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
                List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                GetSalaryHead(out dsSalHd);
                DataView dvsh = new DataView(dsSalHd.Tables[0]);
                DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                if (dtSalHdx.Rows.Count > 0)
                    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();







                DeleteDailyAllowanceTransactionEmpWise(lockDate, identity.PlantId, EmpIds);



                ConnectionManager.DAL.ConManager objCon;

                string _sql = "SELECT * FROM [dbo].[DailyAllowanceTransaction] WHERE EmpSystemId IN (" + EmpIds + ") AND  WorkDate='" + lockDate + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(_sql, out DataSet dsMaster, false, "1");



                var dailyAll = GetAllowanceDaily(lockDate, identity.PlantId);
                foreach (var data in dailyAll)
                {
                    var dailyAllowanceData = GetDailyAllowanceEmployeeWise(lockDate, identity.PlantId, data.Id, data.Catagory, EmpIds);

                    DataSet dsRateFromSalaryRange = null;
                    GetRateFromSalaryRange(data.Id, identity.PlantId, out dsRateFromSalaryRange);



                    foreach (var item in dailyAllowanceData)
                    {


                        List<SPvalueHeadWise> dtValue = null;
                        decimal Totalvalue = 0;
                        decimal Ratevalue = 0;
                        decimal Quantity = 0;
                        string FormulaDesID = string.Empty;







                        if (Convert.ToBoolean(item.IsAllDesignation) == true)
                        {




                            if (Convert.ToBoolean(item.IsRateBasedOnSalaryRange) == true)///////RateBasedOnSalaryRang
                            {
                                //FormulaDesID = item.SalaryRangeBasedOnSalaryHeadId.ToString();
                                FormulaDesID = Regex.Replace(item.SalaryRangeBasedOnSalaryHeadId.ToString(), pattern, replace);
                                if (DicAllEmpSalaryInfo.ContainsKey(item.EmpSystemId.ToString()) == false)
                                    continue;

                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[item.EmpSystemId.ToString()];


                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table




                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                }
                                try
                                {
                                    ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();



                                    if (dsRateFromSalaryRange.Tables[0].Rows.Count > 0)
                                    {
                                        DataView dvRateFromSalaryRange = new DataView(dsRateFromSalaryRange.Tables[0]);
                                        dvRateFromSalaryRange.RowFilter = "SalaryRangeLowerLimit<=" + sFormulaResult + " and " + sFormulaResult + "<=SalaryRangeUpperLimit";



                                        if (dvRateFromSalaryRange.Count > 0)
                                        {

                                            Ratevalue = Convert.ToDecimal(dvRateFromSalaryRange[0]["Rate"].ToString());
                                        }
                                        dvRateFromSalaryRange.RowFilter = null;
                                    }








                                    if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                    {

                                        Quantity = 1;

                                    }
                                    if (item.Catagory == "HourlyOffDuty")
                                    {

                                        Quantity = Convert.ToDecimal(item.OTDuration) / 60;

                                    }
                                    if (item.Catagory == "HourlyOffDutyDeduction")
                                    {

                                        Quantity = Convert.ToDecimal(item.DurationInMin) / 60;

                                    }
                                    Totalvalue = Ratevalue * Quantity;
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }///////RateBasedOnSalaryRang
                            }
                            else
                            {
                                if (Convert.ToBoolean(item.IsFixed) == true)
                                {
                                    Ratevalue = Convert.ToDecimal(item.Rate);
                                    if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                    {

                                        Quantity = 1;

                                    }
                                    if (item.Catagory == "HourlyOffDuty")
                                    {
                                        Quantity = Convert.ToDecimal(item.OTDuration) / 60;


                                    }
                                    if (item.Catagory == "HourlyOffDutyDeduction")
                                    {


                                        Quantity = Convert.ToDecimal(item.DurationInMin) / 60;

                                    }
                                    Totalvalue = Ratevalue * Quantity;
                                }
                                else
                                {
                                    //FormulaDesID = item.FormulaDesID.ToString();
                                    FormulaDesID = Regex.Replace(item.FormulaDesID.ToString(), pattern, replace);
                                    if (DicAllEmpSalaryInfo.ContainsKey(item.EmpSystemId.ToString()) == false)
                                        continue;

                                    List<DataRow> salaryStructure = DicAllEmpSalaryInfo[item.EmpSystemId.ToString()];


                                    #region Create Table                    
                                    dtValue = new List<SPvalueHeadWise>();
                                    #endregion Create Table




                                    for (int j = 0; j < salaryStructure.Count; j++)
                                    {
                                        SPvalueHeadWise sp = new SPvalueHeadWise();
                                        sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                        sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                        sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                        dtValue.Add(sp);
                                    }
                                    try
                                    {
                                        ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                        sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();



                                        //Totalvalue = Convert.ToDecimal(sFormulaResult) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString().Trim());
                                        Ratevalue = Convert.ToDecimal(sFormulaResult);
                                        if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                        {

                                            Quantity = 1;

                                        }
                                        if (item.Catagory == "HourlyOffDuty")
                                        {

                                            Quantity = Convert.ToDecimal(item.OTDuration) / 60;

                                        }
                                        if (item.Catagory == "HourlyOffDutyDeduction")
                                        {

                                            Quantity = Convert.ToDecimal(item.DurationInMin) / 60;

                                        }
                                        Totalvalue = Ratevalue * Quantity;
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (Convert.ToBoolean(item.DARIsFixed) == true)
                            {
                                //Totalvalue = Convert.ToDecimal(item.DARRate) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString());
                                Ratevalue = Convert.ToDecimal(item.DARRate);
                                if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                {

                                    Quantity = 1;

                                }
                                if (item.Catagory == "HourlyOffDuty")
                                {
                                    Quantity = Convert.ToDecimal(item.DurationInMin) / 60;


                                }
                                if (item.Catagory == "HourlyOffDutyDeduction")
                                {

                                    Quantity = Convert.ToDecimal(item.OTDuration) / 60;

                                }
                                Totalvalue = Ratevalue * Quantity;
                            }
                            else
                            {
                                //FormulaDesID = item.DARFormulaDesID.ToString();
                                FormulaDesID = Regex.Replace(item.DARFormulaDesID.ToString(), pattern, replace);
                                if (DicAllEmpSalaryInfo.ContainsKey(item.EmpSystemId.ToString()) == false)
                                    continue;

                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[item.EmpSystemId];


                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table




                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                }
                                try
                                {
                                    ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                                    //Totalvalue = Convert.ToDecimal(sFormulaResult) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString().Trim());

                                    Ratevalue = Convert.ToDecimal(sFormulaResult);
                                    if (item.Catagory == "DailyAllowanceTimeBased" || item.Catagory == "WeekOffAllowance" || item.Catagory == "HolidayAllowance")
                                    {

                                        Quantity = 1;

                                    }
                                    if (item.Catagory == "HourlyOffDuty")
                                    {


                                        Quantity = Convert.ToDecimal(item.DurationInMin) / 60;
                                    }
                                    if (item.Catagory == "HourlyOffDutyDeduction")
                                    {
                                        Quantity = Convert.ToDecimal(item.OTDuration) / 60;


                                    }
                                    Totalvalue = Ratevalue * Quantity;

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }


                        }



                        //string _sql = "SELECT * FROM [dbo].[DailyAllowanceTransaction] WHERE EmpSystemId='" + item.EmpSystemId + "' AND WorkDate='" + lockDate + "' AND AllowanceDailyId='" + item.AllowanceDailyId + "'";
                        //objCon = new ConnectionManager.DAL.ConManager("1");
                        //objCon.OpenDataSetThroughAdapter(_sql, out DataSet dsMaster, false, "1");



                        DataView dvMastert = new DataView(dsMaster.Tables[0]);
                        dvMastert.RowFilter = "EmpSystemId='" + item.EmpSystemId + "' AND WorkDate='" + lockDate + "' AND AllowanceDailyId='" + item.AllowanceDailyId + "'";



                        if (dvMastert.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = GetDailyAllowanceTransactionPK();
                            dr["PlantId"] = identity.PlantId;
                            dr["WorkDate"] = lockDate;
                            dr["EmpSystemId"] = item.EmpSystemId;
                            dr["AllowanceDailyId"] = item.AllowanceDailyId;
                            dr["Quantity"] = Quantity;
                            dr["Rate"] = Ratevalue;
                            dr["Amount"] = Totalvalue;
                            dr["SalaryHeadId"] = item.SalaryHeadId;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;


                            dsMaster.Tables[0].Rows.Add(dr);
                        }


                        dvMastert.RowFilter = null;
                        // }


                    }
                }
                clsStaticInfo objt = new clsStaticInfo();
                objt.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetRateFromSalaryRange(string DailyAllowanceId, string PlantId, out DataSet dsResult)
        {

            ConnectionManager.DAL.ConManager objCon;

            //DataSet dsResult = null;
            string sql = @"SELECT *
                            FROM [DailyAllowanceRateBasedOnSalaryRange] AS R 
                            WHERE  R.DailyAllowanceId='" + DailyAllowanceId + @"' AND R.PlantID='" + PlantId + @"'  ";

            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsResult, false, "1");

        }

        bool IsEligible(string pit, string pot, string pt, string shiftInTime, string ProcessDate)
        {
            bool _isEligible = false;
            try
            {
                var _punchInTime = Convert.ToDateTime(pit);
                var _punchOutTime = Convert.ToDateTime(pot);
                var _policyTime = Convert.ToDateTime(pt);
                var _shiftintime = Convert.ToDateTime(shiftInTime);
                string policyDateTime = _punchInTime.ToString("dd-MMM-yyyy") + " " + _policyTime.ToString("HH:mm");
                string shiftDateTime = _punchInTime.ToString("dd-MMM-yyyy") + " " + _shiftintime.ToString("HH:mm");

                if (Convert.ToDateTime(shiftDateTime) > Convert.ToDateTime(policyDateTime))//night shift so add one day
                {
                    string fpdt = Convert.ToDateTime(ProcessDate).AddDays(1) + " " + _policyTime.ToString("HH:mm");
                    if (Convert.ToDateTime(fpdt) >= _punchInTime && Convert.ToDateTime(fpdt) <= _punchOutTime)
                    {
                        _isEligible = true;
                    }
                }
                else //same date
                {
                    string fpdt = ProcessDate + " " + _policyTime.ToString("HH:mm");
                    if (Convert.ToDateTime(fpdt) >= _punchInTime && Convert.ToDateTime(fpdt) <= _punchOutTime)
                    {
                        _isEligible = true;
                    }
                }
                return _isEligible;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateLockDataEmpWise(string EmpSystemId, string[] lockDateList, string user, string lockEntryDate)
        {


            string result = "";
            for (int i = 0; i < lockDateList.Length; i++)
            {

                if (result == "")
                    result = "'" + lockDateList[i].ToString() + "'";
                else
                    result = result + ",'" + lockDateList[i].ToString() + "'";
            }



            try
            {
                //if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                //{
                //    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                //}
                if (lockDateList.Length == 0)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }
                //GetUnApprovedEmployeeListData(lockDate, out dsUnApprovedList);
                //if (dsUnApprovedList.Tables[0].Rows.Count > 0)
                //{
                //    string UnApprovedEmpList = string.Empty;
                //    foreach (DataRow item in dsUnApprovedList.Tables[0].Rows)
                //    {
                //        UnApprovedEmpList = UnApprovedEmpList + item["EmployeeCode"].ToString() + " - " + item["EmployeeName"].ToString() + "</br>";
                //    }


                //    throw new Exception(UnApprovedEmpList);

                //}
                var UnApprovedEmployeeListData = GetUnApprovedEmployeeListData(EmpSystemId, lockDateList);
                if (UnApprovedEmployeeListData.Count() > 0)
                {

                    throw new Exception("Please Confirmed this Employee is Approved.");

                }
                var OTConfirmationData = GetOTConfirmationData(EmpSystemId, result);

                if (OTConfirmationData.Count() > 0)
                {

                    throw new Exception("Please Confirmed this Employees OT.");

                }
                var ShiftNotAssignData = GetShiftNotAssignData(EmpSystemId, lockDateList);
                if (ShiftNotAssignData.Count() > 0)
                {

                    throw new Exception("Please Confirmed this Employees Shift Assign.");

                }
                var AttdencenotNotProcData = GetAttdencenotNotProcData(EmpSystemId, result);
                if (AttdencenotNotProcData.Count() > 0)
                {

                    throw new Exception("Please Confirmed this Employees Attdence Proc.");

                }
                //var attdnData= _attdnProcessData.qu
                //var d= _recruitmentPlanningProcessSetRepository.Query(t=>t.IsLockD)
                string sql = @"UPDATE AttdnProcessData SET IsLock = 1,LockedBy = '" + user + @"'
                                ,lockedDate = '" + lockEntryDate + @"' 
                                WHERE WorkDate IN (" + result + @") 
                                AND EmpSystemID='" + EmpSystemId + @"'";
                _sqlRepository.ExecuteSqlCommand(sql);




            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public void CreateLockDataDateWise(string lockDate, string[] LockDateWiseEmployeeList, string user, string lockEntryDate)
        {

            string result = "";
            for (int i = 0; i < LockDateWiseEmployeeList.Length; i++)
            {

                if (result == "")
                    result = "'" + LockDateWiseEmployeeList[i].ToString() + "'";
                else
                    result = result + ",'" + LockDateWiseEmployeeList[i].ToString() + "'";
            }



            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }


                var UnApprovedEmployeeListData = GetUnApprovedEmployeeListData(lockDate);
                if (UnApprovedEmployeeListData.Count() > 0)
                {

                    throw new Exception("Please Confirmed this Employee is Approved.");

                }
                var OTConfirmationData = GetOTConfirmationData(lockDate);

                if (OTConfirmationData.Count() > 0)
                {

                    throw new Exception("Please Confirmed this Employees OT.");

                }
                var ShiftNotAssignData = GetShiftNotAssignData(lockDate);
                if (ShiftNotAssignData.Count() > 0)
                {

                    throw new Exception("Please Confirmed this Employees Shift Assign.");

                }
                var AttdencenotNotProcData = GetAttdencenotNotProcData(lockDate);
                if (AttdencenotNotProcData.Count() > 0)
                {

                    throw new Exception("Please Confirmed this Employees Attdence Proc.");

                }
                //var attdnData= _attdnProcessData.qu
                //var d= _recruitmentPlanningProcessSetRepository.Query(t=>t.IsLockD)
                string sql = @"UPDATE AttdnProcessData SET IsLock = 1,LockedBy = '" + user + @"'
                                ,lockedDate = '" + lockEntryDate + @"' 
                                WHERE EmpSystemID IN (" + result + @") 
                                AND WorkDate='" + lockDate + @"'";
                _sqlRepository.ExecuteSqlCommand(sql);




            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public IEnumerable<DailyAllowanceTransaction> GetDailyAllowanceEmployeeList(string lockdate, string plantId, string allowanceId, string allowanceCatagory)
        {


            #region AD
           
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsAdditionalPolicy;
            bool IsAbsentApplicable = false;
            decimal AbsentFromValue = 0;
            decimal AbsentToValue = 0;
            bool IsLateApplicable = false;
            decimal LateFromValue = 0;
            decimal LateToValue = 0;
            bool IsLeaveApplicable = false;
            decimal LeaveFromValue = 0;
            decimal LeaveToValue = 0;
            bool IsLeaveWithOutPayApplicable = false;
            decimal LeaveWithOutPayFromValue = 0;
            decimal LeaveWithOutPayToValue = 0;


            string JOINLeaveApplicable = "";
            string JOINLeaveWithOutPayApplicable = "";


            string WhereAbsentApplicable = "";
            string WhereLateApplicable = "";
            string WhereLeaveApplicable = "";
            string WhereLeaveWithOutPayApplicable = "";
            try
            {
              


                string sqlAdditionalPolicy = @"select IsAbsentApplicable,AbsentFromValue,AbsentToValue
                                ,IsLateApplicable,LateFromValue,LateToValue
                                ,IsLeaveApplicable,LeaveFromValue,LeaveToValue 
                                ,IsLeaveWithOutPayApplicable,LeaveWithOutPayFromValue,LeaveWithOutPayToValue,Id
                                from DailyAllowanceAdditionalPolicy ap
                                where ap.DailyAllowanceId='"+ allowanceId+@"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlAdditionalPolicy, out dsAdditionalPolicy, false, "1");
                if (dsAdditionalPolicy.Tables[0].Rows.Count>0)
                {
                    IsAbsentApplicable = Convert.ToBoolean(dsAdditionalPolicy.Tables[0].Rows[0]["IsAbsentApplicable"]);
                    IsLateApplicable = Convert.ToBoolean(dsAdditionalPolicy.Tables[0].Rows[0]["IsLateApplicable"]);
                    IsLeaveApplicable = Convert.ToBoolean(dsAdditionalPolicy.Tables[0].Rows[0]["IsLeaveApplicable"]);
                    IsLeaveWithOutPayApplicable = Convert.ToBoolean(dsAdditionalPolicy.Tables[0].Rows[0]["IsLeaveWithOutPayApplicable"]);

                    AbsentFromValue = Convert.ToDecimal(dsAdditionalPolicy.Tables[0].Rows[0]["AbsentFromValue"]);
                    AbsentToValue = Convert.ToDecimal(dsAdditionalPolicy.Tables[0].Rows[0]["AbsentToValue"]);
                    LateFromValue = Convert.ToDecimal(dsAdditionalPolicy.Tables[0].Rows[0]["LateFromValue"]);
                    LateToValue = Convert.ToDecimal(dsAdditionalPolicy.Tables[0].Rows[0]["LateToValue"]);
                    LeaveFromValue = Convert.ToDecimal(dsAdditionalPolicy.Tables[0].Rows[0]["LeaveFromValue"]);
                    LeaveToValue = Convert.ToDecimal(dsAdditionalPolicy.Tables[0].Rows[0]["LeaveToValue"]);
                    LeaveWithOutPayFromValue = Convert.ToDecimal(dsAdditionalPolicy.Tables[0].Rows[0]["LeaveWithOutPayFromValue"]);
                    LeaveWithOutPayToValue = Convert.ToDecimal(dsAdditionalPolicy.Tables[0].Rows[0]["LeaveWithOutPayToValue"]);

                    JOINLeaveApplicable = @" LEFT JOIN 
                                                    (SELECT
                                                        ap.EmpSystemID,
                                                        SUM(LeaveDuration)[Leave]
                                                    from AttdnProcessData ap
                                                    where Month(WorkDate) = MONTH('" + lockdate + @"') and year(WorkDate) = year('" + lockdate + @"')
                                                    and LTSystemID in (select LeaveTypeId from DailyAllowanceLeaveType where DailyAllowanceAdditionalPolicyId='"+ dsAdditionalPolicy.Tables[0].Rows[0]["Id"] + @"')
                                                    --and ap.EmpSystemID = 2000001
                                                    group by ap.EmpSystemID) Leave on Leave.EmpSystemID = apd.EmpSystemID";


                    JOINLeaveWithOutPayApplicable = @" LEFT JOIN 
                                                    (SELECT
                                                        ap.EmpSystemID,
                                                        SUM(LeaveDuration)[LeaveWithoutPay]
                                                    from AttdnProcessData ap
                                                    where Month(WorkDate) = MONTH('" + lockdate + @"') and year(WorkDate) = year('" + lockdate + @"')
                                                    and LTSystemID in (Select Id From LeaveType where LeaveType = 'Leave Without Pay')
                                                    --and ap.EmpSystemID = 2000001
                                                    group by ap.EmpSystemID) LeaveWithoutPay on LeaveWithoutPay.EmpSystemID == apd.EmpSystemID";





                     WhereAbsentApplicable = " AND LateAbsent.Absent between "+ AbsentFromValue + @" and " + AbsentToValue;
                     WhereLateApplicable = " AND LateAbsent.Late between " + LateFromValue + @" and " + LateToValue; 
                     WhereLeaveApplicable = " AND Leave.Leave between " + LeaveFromValue + @" and " + LeaveToValue;
                     WhereLeaveWithOutPayApplicable = " AND LeaveWithoutPay.LeaveWithoutPay between " + LeaveWithOutPayFromValue + @" and " + LeaveWithOutPayToValue;


                }


            }
            catch (Exception ex)
            {

                throw (ex);
            }

            #endregion


            try
            {
                string sql = string.Empty;
                if (allowanceCatagory == "DailyAllowanceTimeBased")
                {
                    sql = @" SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType=''
								 ,DurationInMin=''
								 ,OTDuration='',ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                 left outer join DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id                              
                                  
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                 ---------------------------------------
                              LEFT JOIN (SELECT  ho.WorkDate ,ho.EmpSystemId,max(ho.ToDate) ExtraOTOutTime
											FROM HourlyOT AS ho 
											WHERE ho.OTType='EXTRAOT' AND ho.WorkDate='" + lockdate + @"' 
											GROUP BY  ho.WorkDate ,ho.EmpSystemId,ho.ToDate) AS extraot 
											ON extraot.EmpSystemID = apd.EmpSystemID AND extraot.WorkDate = apd.WorkDate
                            ----------------------------------------------------
                            LEFT JOIN 
                            (SELECT 
	                            ap.EmpSystemID,
	                            SUM(CASE WHEN DayStatus ='L' THEN 1 ELSE 0 END) [Late],
	                            SUM(CASE WHEN DayStatus ='A' THEN 1 ELSE 0 END) [Absent]
	
                            from AttdnProcessData ap
                            where Month(WorkDate)=MONTH('" + lockdate + @"') and year(WorkDate)=year('" + lockdate + @"')  
                            ---and ap.EmpSystemID=2000001
                            group by ap.EmpSystemID ) LateAbsent on LateAbsent.EmpSystemID = apd.EmpSystemID

                            "+ (IsLeaveApplicable==true? JOINLeaveApplicable : " ")+ @"
                            " + (IsLeaveWithOutPayApplicable == true ? JOINLeaveWithOutPayApplicable : " ") + @"

                            WHERE apd.WorkDate='" + lockdate + @"' AND 
                                 1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') 
                                          ----AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
                                              AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(ISNULL(extraot.ExtraOTOutTime, apd.OutTime),'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') 
                                           ---AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
                                              AND CONVERT(DATETIME,format(ISNULL(extraot.ExtraOTOutTime, apd.OutTime),'dd-MMM-yyyy'))+''+FORMAT(ISNULL(extraot.ExtraOTOutTime, apd.OutTime),'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                
                                
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  )
                                    " + (IsAbsentApplicable == true ? WhereAbsentApplicable : " ") + @"
                                    " + (IsLateApplicable == true ? WhereLateApplicable : " ") + @"
                                    " + (IsLeaveApplicable == true ? WhereLeaveApplicable : " ") + @"
                                    " + (IsLeaveWithOutPayApplicable == true ? WhereLeaveWithOutPayApplicable : " ") + @"





                                  AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 
                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')";
                }
                if (allowanceCatagory == "WeekOffAllowance")
                {
                    sql = @"   SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType=''---CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
										       --WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											   --ELSE edwsa.DayType END
								 ,DurationInMin=''
								 ,OTDuration='',ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 LEFT JOIN   ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                  LEFT JOIN  DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id            
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                  ---LEFT JOIN  EmpDateWiseShiftAssign AS edwsa ON edwsa.EmpSystemID = apd.EmpSystemID AND edwsa.WorkDate = apd.WorkDate
                                  ---LEFT JOIN
								  ---(SELECT odm.OffDayType,d.PlantId,d.OffDayDate FROM scs.OffDayDetail  d
								  ---LEFT JOIN scs.OffDayMaster AS odm ON odm.Id = d.OffDayMasterId 
								   ---WHERE odm.OffDayType='H' AND d.PlantId='" + plantId + @"' AND d.OffDayDate = '" + lockdate + @"'
								  ---) AS odd ON odd.PlantId=apd.PlantID AND odd.OffDayDate = apd.WorkDate
								  ---LEFT JOIN [MST].[ExceptionForHolidayEmpList] AS efhel ON efhel.EmpSystemId =apd.EmpSystemID AND efhel.WorkDate =apd.WorkDate
								  
                                LEFT JOIN  DayType dt on dt.DayType=apd.DayStatus 
-------------------------------------------------------------
                            LEFT JOIN 
                            (SELECT 
	                            ap.EmpSystemID,
	                            SUM(CASE WHEN DayStatus ='L' THEN 1 ELSE 0 END) [Late],
	                            SUM(CASE WHEN DayStatus ='A' THEN 1 ELSE 0 END) [Absent]
	
                            from AttdnProcessData ap
                            where Month(WorkDate)=MONTH('" + lockdate + @"') and year(WorkDate)=year('" + lockdate + @"')  
                            ---and ap.EmpSystemID=2000001
                            group by ap.EmpSystemID ) LateAbsent on LateAbsent.EmpSystemID = apd.EmpSystemID
                            " + (IsLeaveApplicable == true ? JOINLeaveApplicable : " ") + @"
                            " + (IsLeaveWithOutPayApplicable == true ? JOINLeaveWithOutPayApplicable : " ") + @"



                                WHERE apd.WorkDate='" + lockdate + @"' AND 
                                1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                 
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  ) 
                                 ---AND 'W' = CASE WHEN ISNULL(odd.OffDayType ,'')= 'H' AND ISNULL(efhel.EmpSystemId,'')!= '' THEN edwsa.DayType
                                                                  ---WHEN ISNULL(odd.OffDayType, '') = 'H' AND ISNULL(efhel.EmpSystemId,'')= '' THEN odd.OffDayType
                                                                    ---ELSE edwsa.DayType END
                                  ----AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 

                                  AND dt.OriginalDayType='W'
                                  AND apd.InTime is not null  

                                    " + (IsAbsentApplicable == true ? WhereAbsentApplicable : " ") + @"
                                    " + (IsLateApplicable == true ? WhereLateApplicable : " ") + @"
                                    " + (IsLeaveApplicable == true ? WhereLeaveApplicable : " ") + @"
                                    " + (IsLeaveWithOutPayApplicable == true ? WhereLeaveWithOutPayApplicable : " ") + @"


                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')  
                                  AND apd.IsOTEntitled=0";
                }
                if (allowanceCatagory == "HolidayAllowance")
                {
                    sql = @" SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType=''---CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
										       --WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											   ---ELSE edwsa.DayType END
								 ,DurationInMin=''
								 ,OTDuration='',ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                 left outer join DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id            
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                  --LEFT JOIN  EmpDateWiseShiftAssign AS edwsa ON edwsa.EmpSystemID = apd.EmpSystemID AND edwsa.WorkDate = apd.WorkDate
                                  --LEFT JOIN
								  --(SELECT odm.OffDayType,d.PlantId,d.OffDayDate FROM scs.OffDayDetail  d
								   --LEFT JOIN scs.OffDayMaster AS odm ON odm.Id = d.OffDayMasterId 
								  -- WHERE odm.OffDayType='H' AND d.PlantId='" + plantId + @"' AND d.OffDayDate = '" + lockdate + @"'
								  --) AS odd ON odd.PlantId=apd.PlantID AND odd.OffDayDate = apd.WorkDate
								 --- LEFT JOIN [MST].[ExceptionForHolidayEmpList] AS efhel ON efhel.EmpSystemId =apd.EmpSystemID AND efhel.WorkDate =apd.WorkDate
								  
                             LEFT JOIN  DayType dt on dt.DayType=apd.DayStatus 
-----------------------------------------------------------------------------------------------------
                            LEFT JOIN 
                            (SELECT 
	                            ap.EmpSystemID,
	                            SUM(CASE WHEN DayStatus ='L' THEN 1 ELSE 0 END) [Late],
	                            SUM(CASE WHEN DayStatus ='A' THEN 1 ELSE 0 END) [Absent]
	
                            from AttdnProcessData ap
                            where Month(WorkDate)=MONTH('" + lockdate + @"') and year(WorkDate)=year('" + lockdate + @"')  
                            ---and ap.EmpSystemID=2000001
                            group by ap.EmpSystemID ) LateAbsent on LateAbsent.EmpSystemID = apd.EmpSystemID

                            " + (IsLeaveApplicable == true ? JOINLeaveApplicable : " ") + @"
                            " + (IsLeaveWithOutPayApplicable == true ? JOINLeaveWithOutPayApplicable : " ") + @"

                                WHERE apd.WorkDate='" + lockdate + @"' AND 
                                1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                 
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  ) 
                                  ---AND 'H' = CASE WHEN ISNULL(odd.OffDayType ,'')= 'H' AND ISNULL(efhel.EmpSystemId,'')!= '' THEN edwsa.DayType
                                                                 --- WHEN ISNULL(odd.OffDayType, '') = 'H' AND ISNULL(efhel.EmpSystemId,'')= '' THEN odd.OffDayType
                                                                   --- ELSE edwsa.DayType END
                                  ---AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 

                                  AND dt.OriginalDayType='H'
                                  AND apd.InTime is not null 
                                    " + (IsAbsentApplicable == true ? WhereAbsentApplicable : " ") + @"
                                    " + (IsLateApplicable == true ? WhereLateApplicable : " ") + @"
                                    " + (IsLeaveApplicable == true ? WhereLeaveApplicable : " ") + @"
                                    " + (IsLeaveWithOutPayApplicable == true ? WhereLeaveWithOutPayApplicable : " ") + @"
                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')  
                                  AND apd.IsOTEntitled=0";
                }
                if (allowanceCatagory == "HourlyOffDutyDeduction")
                {
                    sql = @"   SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType=''
								 ,HOD.DurationInMin
								 ,OTDuration='',ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                 left outer join DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id                              
                                  
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                  
								  INNER JOIN (
                            	  SELECT EmpSystemId,WorkDate,sum(DurationInMin) DurationInMin
										FROM HourlyOffDuty WHERE IsApprove=1 AND ApproveType='Deducation' GROUP BY EmpSystemId,WorkDate
                                   ) AS HOD ON HOD.EmpSystemId = apd.EmpSystemId AND HOD.WorkDate = apd.WorkDate
                            
                                  
                            
                                WHERE apd.WorkDate='" + lockdate + @"' AND 
                                1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                 
                                 
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  ) 
                                  AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 
                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')";
                }
                if (allowanceCatagory == "HourlyOffDuty")
                {
                    sql = @"   SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType=''
								 ,DurationInMin=''
								 ,HO.OTDuration,ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                 left outer join DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id                              
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                  
								  
                            
                                  INNER JOIN (
                                 	SELECT EmpSystemId,WorkDate,sum(Duration) OTDuration
                                     FROM [dbo].[HourlyOT]   GROUP BY EmpSystemId,WorkDate
                                 ) AS HO ON HO.EmpSystemId = apd.EmpSystemId AND HO.WorkDate = apd.WorkDate
                            
                            
                                WHERE apd.WorkDate='" + lockdate + @"' AND 
                                1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                 
                                 
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  ) 
                                  ---AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 
                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')";
                }


                return _sqlRepository.GetModelCollection<DailyAllowanceTransaction>(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<DailyAllowanceTransaction> GetDailyAllowanceEmployeeWise(string lockdate, string plantId, string allowanceId, string allowanceCatagory, string EmpIds)
        {
            try
            {
                string sql = string.Empty;
                if (allowanceCatagory == "DailyAllowanceTimeBased")
                {
                    sql = @" SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType=''
								 ,DurationInMin=''
								 ,OTDuration='',ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                 left outer join DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id                              
                                  
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                 
                               ---------------------------------------
                              LEFT JOIN (SELECT  ho.WorkDate ,ho.EmpSystemId,max(ho.ToDate) ExtraOTOutTime
											FROM HourlyOT AS ho 
											WHERE ho.OTType='EXTRAOT' AND ho.WorkDate='" + lockdate + @"' 
											GROUP BY  ho.WorkDate ,ho.EmpSystemId,ho.ToDate) AS extraot 
											ON extraot.EmpSystemID = apd.EmpSystemID AND extraot.WorkDate = apd.WorkDate
                            ----------------------------------------------------
                             LEFT JOIN 
                            (SELECT 
	                            ap.EmpSystemID,
	                            SUM(CASE WHEN DayStatus ='L' THEN 1 ELSE 0 END) [Late],
	                            SUM(CASE WHEN DayStatus ='A' THEN 1 ELSE 0 END) [Absent]
	
                            from AttdnProcessData ap
                            where Month(WorkDate)=MONTH('" + lockdate + @"') and year(WorkDate)=year('" + lockdate + @"')  
                            ---and ap.EmpSystemID=2000001
                            group by ap.EmpSystemID ) LateAbsent on LateAbsent.EmpSystemID = apd.EmpSystemID
                                WHERE apd.EmpSystemId IN (" + EmpIds + @") AND apd.WorkDate='" + lockdate + @"' AND 
                                 1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') 
                                          ----AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
                                              AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(ISNULL(extraot.ExtraOTOutTime, apd.OutTime),'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') 
                                          ----AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
                                          ----AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(ISNULL(extraot.ExtraOTOutTime, apd.OutTime),'hh:mm tt')
                                              AND CONVERT(DATETIME,format(ISNULL(extraot.ExtraOTOutTime, apd.OutTime),'dd-MMM-yyyy'))+''+FORMAT(ISNULL(extraot.ExtraOTOutTime, apd.OutTime),'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                
                                
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  ) 
                                  AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 
                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')";
                }
                if (allowanceCatagory == "WeekOffAllowance")
                {
                    sql = @"   SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType=dt.OriginalDayType --CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
										       --WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											   --ELSE edwsa.DayType END
								 ,DurationInMin=''
								 ,OTDuration='',ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                 left outer join DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id            
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                  ---LEFT JOIN  EmpDateWiseShiftAssign AS edwsa ON edwsa.EmpSystemID = apd.EmpSystemID AND edwsa.WorkDate = apd.WorkDate
                                  ---LEFT JOIN
								 --- (SELECT odm.OffDayType,d.PlantId,d.OffDayDate FROM scs.OffDayDetail  d
								  --- LEFT JOIN scs.OffDayMaster AS odm ON odm.Id = d.OffDayMasterId 
								  --- WHERE odm.OffDayType='H' AND d.PlantId='" + plantId + @"' AND d.OffDayDate = '" + lockdate + @"'
								 ---- ) AS odd ON odd.PlantId=apd.PlantID AND odd.OffDayDate = apd.WorkDate
								 --- LEFT JOIN [MST].[ExceptionForHolidayEmpList] AS efhel ON efhel.EmpSystemId =apd.EmpSystemID AND efhel.WorkDate =apd.WorkDate
								  
                                LEFT JOIN  DayType dt on dt.DayType=apd.DayStatus 
                            LEFT JOIN 
                            (SELECT 
	                            ap.EmpSystemID,
	                            SUM(CASE WHEN DayStatus ='L' THEN 1 ELSE 0 END) [Late],
	                            SUM(CASE WHEN DayStatus ='A' THEN 1 ELSE 0 END) [Absent]
	
                            from AttdnProcessData ap
                            where Month(WorkDate)=MONTH('" + lockdate + @"') and year(WorkDate)=year('" + lockdate + @"')  
                            ---and ap.EmpSystemID=2000001
                            group by ap.EmpSystemID ) LateAbsent on LateAbsent.EmpSystemID = apd.EmpSystemID
                                WHERE apd.EmpSystemId IN (" + EmpIds + @") AND apd.WorkDate='" + lockdate + @"' AND 
                                1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                 
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  ) 
                                 ---AND 'W' = CASE WHEN ISNULL(odd.OffDayType ,'')= 'H' AND ISNULL(efhel.EmpSystemId,'')!= '' THEN edwsa.DayType
                                                                ---  WHEN ISNULL(odd.OffDayType, '') = 'H' AND ISNULL(efhel.EmpSystemId,'')= '' THEN odd.OffDayType
                                                                 ---   ELSE edwsa.DayType END
                                 --- AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 
                                  AND dt.OriginalDayType='W'
                                  AND apd.InTime is not null 
                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')  
                                  AND apd.IsOTEntitled=0";
                }
                if (allowanceCatagory == "HolidayAllowance")
                {
                    sql = @" SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType= dt.OriginalDayType    ---CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
										       ---WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											   ---ELSE edwsa.DayType END
								 ,DurationInMin=''
								 ,OTDuration='',ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                 left outer join DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id            
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                 --- LEFT JOIN  EmpDateWiseShiftAssign AS edwsa ON edwsa.EmpSystemID = apd.EmpSystemID AND edwsa.WorkDate = apd.WorkDate
                                 --- LEFT JOIN
								 --- (SELECT odm.OffDayType,d.PlantId,d.OffDayDate FROM scs.OffDayDetail  d
								 ---  LEFT JOIN scs.OffDayMaster AS odm ON odm.Id = d.OffDayMasterId 
								  --- WHERE odm.OffDayType='H' AND d.PlantId='" + plantId + @"' AND d.OffDayDate = '" + lockdate + @"'
								 --- ) AS odd ON odd.PlantId=apd.PlantID AND odd.OffDayDate = apd.WorkDate
								  ---LEFT JOIN [MST].[ExceptionForHolidayEmpList] AS efhel ON efhel.EmpSystemId =apd.EmpSystemID AND efhel.WorkDate =apd.WorkDate
								  
                                LEFT JOIN  DayType dt on dt.DayType=apd.DayStatus 
                            LEFT JOIN 
                            (SELECT 
	                            ap.EmpSystemID,
	                            SUM(CASE WHEN DayStatus ='L' THEN 1 ELSE 0 END) [Late],
	                            SUM(CASE WHEN DayStatus ='A' THEN 1 ELSE 0 END) [Absent]
	
                            from AttdnProcessData ap
                            where Month(WorkDate)=MONTH('" + lockdate + @"') and year(WorkDate)=year('" + lockdate + @"')  
                            ---and ap.EmpSystemID=2000001
                            group by ap.EmpSystemID ) LateAbsent on LateAbsent.EmpSystemID = apd.EmpSystemID
                                WHERE apd.EmpSystemId IN (" + EmpIds + @") AND apd.WorkDate='" + lockdate + @"' AND 
                                1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                 
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  ) 
                                  ---AND 'H' = CASE WHEN ISNULL(odd.OffDayType ,'')= 'H' AND ISNULL(efhel.EmpSystemId,'')!= '' THEN edwsa.DayType
                                                                ---  WHEN ISNULL(odd.OffDayType, '') = 'H' AND ISNULL(efhel.EmpSystemId,'')= '' THEN odd.OffDayType
                                                                  ---  ELSE edwsa.DayType END
                                  ---AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 
                                  AND dt.OriginalDayType='H'
                                  AND apd.InTime is not null 
                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                  AND apd.IsOTEntitled=0";
                }
                if (allowanceCatagory == "HourlyOffDutyDeduction")
                {
                    sql = @"   SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType=''
								 ,HOD.DurationInMin
								 ,OTDuration='',ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                 left outer join DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id                              
                                  
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                  
								  INNER JOIN (
                            	  SELECT EmpSystemId,WorkDate,sum(DurationInMin) DurationInMin
										FROM HourlyOffDuty WHERE IsApprove=1 AND ApproveType='Deducation' GROUP BY EmpSystemId,WorkDate
                                   ) AS HOD ON HOD.EmpSystemId = apd.EmpSystemId AND HOD.WorkDate = apd.WorkDate
                            
                                  
                            
                                WHERE apd.EmpSystemId IN (" + EmpIds + @") AND apd.WorkDate='" + lockdate + @"' AND 
                                1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                 
                                 
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  ) 
                                  AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 
                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')";
                }
                if (allowanceCatagory == "HourlyOffDuty")
                {
                    sql = @"   SELECT apd.EmpSystemID
                                 ,ad.id AllowanceDailyId
                                 ,ad.Catagory
                                 ,ad.SalaryHeadId 
                                 ,ad.IsAllDesignation
								 ,ad.IsFixed
                                 ,ad.Rate
                                 ,ad.FormulaDesID
                                 ,DAR.IsFixed DARIsFixed 
                                 ,DAR.Rate DARRate 
                                 ,DAR.FormulaDesID DARFormulaDesID
								 ,DayType=''
								 ,DurationInMin=''
								 ,HO.OTDuration,ISNULL(ad.IsRateBasedOnSalaryRange,0) IsRateBasedOnSalaryRange,ad.SalaryRangeBasedOnSalaryHeadId,ISNULL(ad.IsVoucherPayment,0) IsVoucherPayment
                                 FROM AttdnProcessData AS apd
                                 LEFT JOIN hkp.AllowanceDaily AS ad ON apd.PlantID=ad.PlantId AND ad.Active=1 AND  ad.Id='" + allowanceId + @"'
                                 left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                                 left outer join DailyAllowanceSetting ST on st.ShiftSystemID=apd.ShiftSystemID and st.PlantID=apd.PlantID AND ST.DailyAllowanceID = ad.Id                              
                                  LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = apd.EmpSystemId
                                  LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=ad.Id AND dar.DesignationId=ei.GivenDesignationId 
                                  
								  
                            
                                  INNER JOIN (
                                 	SELECT EmpSystemId,WorkDate,sum(Duration) OTDuration
                                     FROM [dbo].[HourlyOT]   GROUP BY EmpSystemId,WorkDate
                                 ) AS HO ON HO.EmpSystemId = apd.EmpSystemId AND HO.WorkDate = apd.WorkDate
                            
                            
                                WHERE apd.EmpSystemId IN (" + EmpIds + @") AND apd.WorkDate='" + lockdate + @"' AND 
                                1=  (
  	                                CASE 
  	                                WHEN ad.IsAllShift=1  THEN
  		                                CASE 
  		                                WHEN  ad.IsSpecificTime=0 THEN 1
  		                                WHEN  ad.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(ad.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(ad.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   
  	                                WHEN ad.IsAllShift=0  THEN 
  	                                    CASE 
  		                                WHEN  st.IsSpecificTime=0 THEN 1
  		                                WHEN  st.IsSpecificTime=1 AND   		                                	
  		                                    CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                                              THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                                              ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                                              BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
  		                                    THEN 1 ELSE 0 END 
  	                                   ELSE 0 END  
                                  ) 
                                 
                                 
                                   AND
                                 1 = (
                                      CASE

                                      WHEN ad.IsAllDesignation = 1  THEN  1

                                      WHEN ad.IsAllDesignation = 0 AND dar.DesignationId = ei.GivenDesignationId THEN 1

                                      ELSE  0 END
                                  ) 
                                  AND apd.DayStatus IN(SELECT DayType FROM DayType WHERE Category IN('Present','Late')) 
                                  AND apd.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')";
                }


                return _sqlRepository.GetModelCollection<DailyAllowanceTransaction>(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }







        public IEnumerable<DailyAllowanceTransaction> GetDailyAllowanceDataEmployeeWise(string lockdate, string plantId, string allowanceId, string EmpIds)
        {
            try
            {

                string sql = @"
                            SELECT apd.EmpSystemID,st.DailyAllowanceID AllowanceDailyId FROM AttdnProcessData AS apd
                            left outer join ShiftDefination SD ON sd.SystemID=apd.ShiftSystemID
                            left outer join DailyAllowanceSetting ST 
                            on st.ShiftSystemID=apd.ShiftSystemID 
                            and apd.WorkDate between st.FromDate and st.ToDate 
                            and st.DailyAllowanceID='" + allowanceId + @"' and st.PlantID=apd.PlantID
                            
                            WHERE apd.WorkDate='" + lockdate + @"' AND 
                            CONVERT(DATETIME,(CASE WHEN CONVERT(DATETIME,format(apd.WorkDate,'dd-MMM-yyyy') +' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))<CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt')
                            THEN DATEADD(DAY,1,CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt'))) 
                            ELSE CONVERT(DATETIME,apd.WorkDate+ ' '+ FORMAT(st.EffectiveTime,'hh:mm tt')) END ))
                            BETWEEN CONVERT(DATETIME,format(apd.InTime,'dd-MMM-yyyy'))+''+FORMAT(apd.InTime,'hh:mm tt') AND CONVERT(DATETIME,format(apd.OutTime,'dd-MMM-yyyy'))+''+FORMAT(apd.OutTime,'hh:mm tt')
                            AND apd.DayStatus IN (SELECT DayType FROM DayType WHERE Category IN('Present','Late')) and apd.EmpSystemID IN (" + EmpIds + @")
                            ";
                return _sqlRepository.GetModelCollection<DailyAllowanceTransaction>(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<AllowanceDaily> GetAllowanceDaily(string Workdate, string PlantId)
        {
            try
            {


                var sql = @"SELECT Id,Catagory,FromEffectiveDate,ToEffectiveDate  FROM [HKP].[AllowanceDaily] WHERE Active=1 AND '" + Workdate + @"' BETWEEN FromEffectiveDate AND ToEffectiveDate AND PlantId='" + PlantId + @"'";
                return _sqlRepository.GetModelCollection<AllowanceDaily>(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetLockEmployeeListData(string lockdate, string plantId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @" Select EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section,apd.LockedBy,FORMAT(apd.lockedDate,'dd-MMM-yyyy hh:mm:ss')  lockedDate
                                From AttdnProcessData AS apd
                                INNER JOIN EmployeeInformation EI ON EI.SystemId = apd.EmpSystemID
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId
                                --LEFT JOIN [PlantWiseAttendanceLock] al ON al.EmpSystemId=EI.SystemId
                                WHERE apd.WorkDate='" + lockdate + @"' AND apd.IsLock=1                           
                                AND  EI.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetTobeLockEmployeeListData(string lockdate, string plantId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = @" Select   CONVERT(BIT, 0) CheckBoxSelect , EI.SystemID,EI.EmployeeCode
                                ,EI.EmployeeName,FORMAT(EI.DOJ,'dd-MMM-yyyy')   DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS   
                                ,DG.UserName GivenDesignation
                                ,DP.UserName Department
                                ,PR.UserName PositionName
                                ,DSG.UserName Designation
                                ,PR.DesignationId
                                ,PG.StandardName PayRollGroupName
                                ,PG.Id PayRollGroupId
                                ,ld.UserName LegalDesignation,Section.UserName Section,apd.LockedBy,FORMAT(apd.lockedDate,'dd-MMM-yyyy hh:mm:ss')  lockedDate
                                From AttdnProcessData AS apd
                                INNER JOIN EmployeeInformation EI ON EI.SystemId = apd.EmpSystemID
                                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                LEFT JOIN HKP.LegalDesignation ld on ld.Id=EI.LegalDesignationId
                                LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                                LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                                LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                                LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
                                LEFT JOIN [ORG].[Section] ON Section.Id = PR.SectionId
                                --LEFT JOIN [PlantWiseAttendanceLock] al ON al.EmpSystemId=EI.SystemId
                                WHERE apd.WorkDate='" + lockdate + @"' AND apd.IsLock=0                           
                                AND  EI.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateUnLockData(string lockDate, string plantId)
        {



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Un Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }

                string sqlvalidation = @" select * from SalaryLock where IsLocked=1 and MonthNo=MONTH('" + lockDate + @"') and YearNo=YEAR('" + lockDate + @"') AND EmpSystemId IN (select SystemId from EmployeeInformation where PlantId='" + plantId + @"') ";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlvalidation, out DataSet dsMasterValidation, false, "1");

                if (dsMasterValidation.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Salary is Locked.");

                }



                string sql = @"SELECT [Id]
                                    ,[PlantId]
                                    ,[LockedDate]
                                    ,[AddedBy]
                                    ,[AddedDate]
                                    ,[AddedFromIP]
                                    ,[UpdatedBy]
                                    ,[UpdatedDate]
                                    ,[UpdatedFromIP],[IsActive]
                                     FROM [PlantWiseAttendanceLock] where LockedDate='" + lockDate + "' AND PlantId='" + plantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                //if (dsMaster.Tables[0].Rows.Count > 0)
                //{

                //    string sql2 = @"Delete
                //                    FROM [PlantWiseAttendanceLock] where LockedDate='" + lockDate + "' AND PlantId='" + plantId + "'";


                //    objCon = new ConnectionManager.DAL.ConManager("1");
                //    objCon.OpenConnection("1");
                //    objCon.BeginTransaction();

                //    objCon.ExecuteNonQueryWrapper(sql2, true, "1");

                //    objCon.CommitTransaction();







                //}
                //else
                //{

                //    throw new Exception("Data not found");
                //}
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("Data not found");

                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["IsActive"] = false;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                }





                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);





            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public void CreateUnLockDataEmployeeWise(string lockDate, string EmpSystemId, string plantId)
        {



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }

                //string sqlvalidation = @" SELECT * FROM SalaryProcMaster AS spm 
                //                          WHERE EXISTS(SELECT * FROM SalaryProcChild AS spc WHERE spc.SlrProcMstSystemID=spm.SystemID AND spc.IsApproved=1 AND spc.PlantID='" + plantId + @"') 
                //                          AND spm.YearNo=YEAR('" + lockDate + @"') AND spm.MonthNo=MONTH('" + lockDate + @"')";


                string sqlvalidation = @" select * from SalaryLock where IsLocked=1 and MonthNo=MONTH('" + lockDate + @"') and YearNo=YEAR('" + lockDate + @"') AND EmpSystemId IN (select SystemId from EmployeeInformation where PlantId='" + plantId + @"') ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlvalidation, out DataSet dsMasterValidation, false, "1");

                if (dsMasterValidation.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }



                string sql = @"SELECT [Id]
                                    ,[PlantId]
                                    ,[LockedDate]
                                    ,[AddedBy]
                                    ,[AddedDate]
                                    ,[AddedFromIP]
                                    ,[UpdatedBy]
                                    ,[UpdatedDate]
                                    ,[UpdatedFromIP]
                                     FROM [PlantWiseAttendanceLock] where LockedDate='" + lockDate + "' AND PlantId='" + plantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                //if (dsMaster.Tables[0].Rows.Count > 0)
                //{

                //    string sql2 = @"Delete
                //                    FROM [PlantWiseAttendanceLock] where LockedDate='" + lockDate + "' AND PlantId='" + plantId + "'";


                //    objCon = new ConnectionManager.DAL.ConManager("1");
                //    objCon.OpenConnection("1");
                //    objCon.BeginTransaction();

                //    objCon.ExecuteNonQueryWrapper(sql2, true, "1");

                //    objCon.CommitTransaction();







                //}
                //else
                //{

                //    throw new Exception("Data not found");
                //}
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("Data not found");

                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["IsActive"] = false;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                }





                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);





            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public string GetLastLockDate()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            try
            {

                string sql = @"SELECT FORMAT(Max([LockedDate]),'dd-MMM-yyyy') [LockedDate]
                               FROM [PlantWiseAttendanceLock] where  PlantId='" + identity.PlantId + "' and IsActive=1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                return dsMaster.Tables[0].Rows[0]["LockedDate"].ToString();


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public string[] GetLockDateList()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;


            try
            {

                string sql = @"SELECT TOP 60 FORMAT([LockedDate],'dd-MMM-yyyy') [LockedDates]
                               FROM [PlantWiseAttendanceLock] where  PlantId='" + identity.PlantId + "' and IsActive=1  order by LockedDate desc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                string[] result = new string[dsMaster.Tables[0].Rows.Count];


                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    result[i] = dsMaster.Tables[0].Rows[i]["LockedDates"].ToString();
                    //if (result == "")
                    //    result = "'" + dsMaster.Tables[0].Rows[i]["LockedDate"].ToString() + "'";
                    //else
                    //    result = result + ",'" + dsMaster.Tables[0].Rows[i]["LockedDate"].ToString() + "'";
                }


                return result;


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public string[] GetUnLockDateList()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;


            try
            {

                string sql = @"SELECT TOP 100 FORMAT([LockedDate],'dd-MMM-yyyy') [LockedDates]
                               FROM [PlantWiseAttendanceLock] where  PlantId='" + identity.PlantId + "' order by LockedDate desc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                string[] result = new string[dsMaster.Tables[0].Rows.Count];
                //string[] result2 = new string[];

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    result[i] = dsMaster.Tables[0].Rows[i]["LockedDates"].ToString();

                }

                string[] newResult = new string[120];
                DateTime dtTo = DateTime.Now;
                DateTime dtFrom = dtTo.AddMonths(-2);
                for (int i = 0; i < 120; i++)
                {
                    string nDate = dtFrom.AddDays(i).ToString("dd-MMM-yyyy");
                    if (result.Contains(nDate))
                        continue;

                    newResult[i] = nDate;
                }





                return newResult;


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public void GetUnApprovedEmployeeListData(string lockDate, out DataSet dsMaster)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            //DataSet dsMaster;
            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }


                string sql = @"Select SystemID,EmployeeCode,EmployeeName From EmployeeInformation 
                                Where  EmployeeStatus='Active' and DOJ between    DATEFROMPARTS(year('" + lockDate + "'),month('" + lockDate + "'),1)   and '" + lockDate + @"'  AND 
                                isApproved=0  AND PlantId='" + identity.PlantId + "'";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public void CheckAttdenceProcAndShiftAssignData(string lockDate, out DataSet dsMaster)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            //DataSet dsMaster;
            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }


                string sql = @"SELECT DISTINCT ISNULL(ShiftNotAssignedEmployee.totalShiftNotAssignedEmployee, 0) ShiftNotAssignedEmployee
	                                ,ISNULL(AttdnNotProcessedToday.totalAttdnNotProcessedToday, 0) totalAttdnNotProcessedToday
                                FROM (
	                                SELECT COUNT(E.SystemId) totalEmployee
		                                ,C.UserName
		                                ,cg.Id CompanyGroupId
		                                ,c.Id CompanyId
		                                ,c.UserName CompanyName
		                                ,cg.UserName GroupName
	                                FROM ORG.CompanyGroup CG
	                                LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
	                                LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id
		                                AND c.Id = E.CompanyId
	                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
	                                LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
	                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                                WHERE GroupID = '" + identity.CompanyGroupId + @"'
		                                AND (
			                                E.EmployeeStatus != 'Separated'
			                                OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate + @"')
			                                )
		                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate + @"')

                                    GROUP BY C.UserName
		                                ,cg.Id
		                                ,c.Id
		                                ,c.UserName
		                                ,cg.UserName
	                                ) OnRoleEmployee
                                LEFT JOIN (
	                                SELECT COUNT(E.SystemId) totalShiftNotAssignedEmployee
		                                ,cg.Id CompanyGroupId
		                                ,cg.UserName GroupName
		                                ,C.Id AS CompanyId
		                                ,C.UserName CompanyName
	                                FROM ORG.CompanyGroup CG
	                                LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
	                                LEFT JOIN (
		                                --*
		                                SELECT *
		                                FROM EmployeeInformation
		                                WHERE SystemId NOT IN (
				                                --**
				                                SELECT DISTINCT EmpSystemID
				                                FROM EmployeeShiftAssign
				                                ) -- * *
		                                ) -- *
		                                E ON e.GroupID = CG.Id
		                                AND c.Id = E.CompanyId
	                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
	                                LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
	                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                                WHERE GroupID = '" + identity.CompanyGroupId + @"'
		                                AND (
			                                E.EmployeeStatus != 'Separated'
			                                OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate + @"')
			                                )
		                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate + @"')

                                    GROUP BY C.UserName
		                                ,cg.UserName
		                                ,C.Id
		                                ,cg.Id
		                                ,cg.UserName
	                                ) ShiftNotAssignedEmployee ON OnRoleEmployee.CompanyGroupId = ShiftNotAssignedEmployee.CompanyGroupId
	                                AND OnRoleEmployee.CompanyId = ShiftNotAssignedEmployee.CompanyId
                                LEFT JOIN (
	                                SELECT count(E.SystemID) totalAttdnNotProcessedToday
		                                ,cg.Id CompanyGroupId
		                                ,cg.UserName GroupName
		                                ,C.Id AS CompanyId
		                                ,C.UserName UId
	                                FROM ORG.CompanyGroup CG
	                                LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
	                                INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id
		                                AND c.Id = E.CompanyId
	                                INNER JOIN (
		                                --*
		                                SELECT TOP 1
		                                WITH TIES *
		                                FROM EmployeeShiftAssign
		                                WHERE EffectiveDate <= GETDATE()
			                                AND EmpSystemID NOT IN (
				                                --**
				                                SELECT DISTINCT EmpSystemID
				                                FROM AttdnProcessData
				                                WHERE CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + lockDate + @"')
				                                )
		                                ORDER BY ROW_NUMBER() OVER (
				                                PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC
				                                )
		                                ) -- *
		                                ESA ON E.SystemId = ESA.EmpSystemID
	                                LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
	                                LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
	                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
	                                WHERE GroupID = '" + identity.CompanyGroupId + @"'
		                                AND (
			                                E.EmployeeStatus != 'Separated'
			                                OR CONVERT(DATE, E.DOS) >= CONVERT(DATE, '" + lockDate + @"')
			                                )
		                                AND CONVERT(DATE, E.DOJ) <= CONVERT(DATE, '" + lockDate + @"')

                                    GROUP BY C.UserName
		                                ,cg.UserName
		                                ,C.Id
		                                ,cg.Id
		                                ,cg.UserName
	                                ) AttdnNotProcessedToday ON OnRoleEmployee.CompanyGroupId = AttdnNotProcessedToday.CompanyGroupId
	                                AND OnRoleEmployee.CompanyId = AttdnNotProcessedToday.CompanyId";




                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public void CheckOTConfirmationData(string lockDate, out DataSet dsMaster)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            //DataSet dsMaster;
            try
            {
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");

                }


                string sql = @"select AP.EmpSystemId, FORMAT(AP.WorkDate,'dd-MMM-yyyy') WorkDate
                                ,E.EmployeeCode,E.EmployeeName from AttdnProcessData AP
                                LEFT JOIN EmployeeInformation E ON E.SystemId=AP.EmpSystemId
                                Where AP.WorkDate=''" + lockDate + @"'' and AP.IsOTComfirm=0 and AP.IsOTEntitled=1 and E.PlantID='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }


        public void CheckAttdanLockSettingData(out DataSet dsMaster)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            //DataSet dsMaster;
            try
            {



                string sql = @"select * from PlantWiseHRMSSetting Where PlantID='" + identity.CompanyGroupId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public void CreateUnLockDataRangeWise(string FromDate, string ToDate, string[] LockDateWiseEmployeeList, CustomIdentity identity)
        {


            ConnectionManager.DAL.ConManager objCon;
            string EmployeeIds = string.Empty;

            try
            {
                if (Convert.ToDateTime(FromDate).Month == Convert.ToDateTime(ToDate).Month && Convert.ToDateTime(FromDate).Year == Convert.ToDateTime(ToDate).Year)
                {
                    //
                }
                else
                {
                    throw new Exception("From date and To Date must be same month.");
                }



                for (int i = 0; i < LockDateWiseEmployeeList.Length; i++)
                {
                    if (string.IsNullOrEmpty(EmployeeIds))
                    {
                        EmployeeIds = "'" + LockDateWiseEmployeeList[i].ToString() + "'";
                    }
                    else
                    {
                        EmployeeIds += ",'" + LockDateWiseEmployeeList[i].ToString() + "'";
                    }
                }



                string sql = @"SELECT * FROM ExceptionEmployeeAttendanceUnlock where WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' AND EmpSystemId IN (" + EmployeeIds + @") ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsExceptionEmployeeAttendanceUnlock, false, "1");




                string sqlvalidation = @" SELECT * FROM SalaryLock WHERE EmpSystemId IN (" + EmployeeIds + @") AND YearNo=YEAR('" + FromDate + @"') AND MonthNo=MONTH('" + FromDate + @"') AND IsLocked=1 ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlvalidation, out DataSet dsMasterValidation, false, "1");

                string sqlIsSalaryProc = @" SELECT spm.*,spc.EmpInfoSystemID FROM SalaryProcMaster AS spm 
                                            LEFT JOIN SalaryProcChild AS spc ON spc.SlrProcMstSystemID = spm.SystemID
                                            WHERE   spc.EmpInfoSystemID IN (" + EmployeeIds + @")
                                            AND spm.YearNo=YEAR('" + FromDate + @"') AND spm.MonthNo=MONTH('" + FromDate + @"')";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlIsSalaryProc, out DataSet dsIsSalaryProc, false, "1");

                string sqlvalidationLog = @"SELECT * FROM ExceptionEmployeeSalaryReprocess WHERE  EmpSystemId IN (" + EmployeeIds + @") AND MonthNo=MONTH('" + FromDate + @"') AND YearNo=YEAR('" + FromDate + @"')";




                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlvalidationLog, out DataSet dsMasterValidationLog, false, "1");









                string sqlAttdnProcess = @"SELECT apd.EmpSystemID,apd.WorkDate  ,apd.PlantID                      
                                                From AttdnProcessData AS apd
                                                LEFT JOIN ExceptionEmployeeAttendanceUnlock AS eeau ON eeau.EmpSystemId = apd.EmpSystemId AND eeau.WorkDate=apd.WorkDate
                                                LEFT JOIN  EmployeeInformation EI ON EI.SystemId = apd.EmpSystemID
												LEFT JOIN [PlantWiseAttendanceLock] al ON al.PlantID = ei.PlantID AND  al.LockedDate = apd.WorkDate
                                                WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' ---AND apd.PlantId='" + identity.PlantId + @"'
                                                AND al.IsActive=1  
                                                AND eeau.EmpSystemId  IS NULL
                                                AND DOJ<='" + ToDate + @"' AND (DOS is null OR DOS>= '" + FromDate + @"') AND  EI.PlantId='" + identity.PlantId + @"'";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlAttdnProcess, out DataSet dsAttdnProcess, false, "1");

                for (int i = 0; i < LockDateWiseEmployeeList.Length; i++)
                {


                    int ExceptionEmployeeAttendanceUnlock = 0;
                    string ExceptionEmployeeAttendanceUnlockPK = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployeeAttendanceUnlock", out ExceptionEmployeeAttendanceUnlockPK);


                    int ExceptionEmployeeSalaryReprocess = 0;
                    string ExceptionEmployeeSalaryReprocessPK = string.Empty;
                    //bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployeeSalaryReprocess", out ExceptionEmployeeSalaryReprocessPK);




                    DateTime sFromDate = Convert.ToDateTime(FromDate.ToString());
                    DateTime sToDate = Convert.ToDateTime(ToDate.ToString());
                    while (sFromDate <= sToDate)//date wise loop
                    {
                        //===================
                        DataView dvAttdnProcess = new DataView(dsAttdnProcess.Tables[0])
                        {
                            RowFilter = "EmpSystemId='" + LockDateWiseEmployeeList[i].ToString() + "'  AND WorkDate='" + sFromDate + "'"
                        };
                        if (dvAttdnProcess.Count > 0)
                        {

                            DataView dvExceptionEmployeeAttendanceUnlock = new DataView(dsExceptionEmployeeAttendanceUnlock.Tables[0])
                            {
                                RowFilter = "EmpSystemId='" + LockDateWiseEmployeeList[i].ToString() + "'  AND WorkDate='" + sFromDate + "'"
                            };
                            if (dvExceptionEmployeeAttendanceUnlock.Count == 0)
                            {
                                //string sID = string.Empty;
                                //bplib.clsGenID objGenID = new bplib.clsGenID();
                                //objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployeeAttendanceUnlock", out sID);
                                DataRow dr = dsExceptionEmployeeAttendanceUnlock.Tables[0].NewRow();
                                dr["Id"] = "AU" + ExceptionEmployeeAttendanceUnlockPK + "_" + ExceptionEmployeeAttendanceUnlock;
                                dr["EmpSystemId"] = LockDateWiseEmployeeList[i].ToString();
                                dr["PlantId"] = identity.PlantId;
                                dr["IsActive"] = true;
                                dr["WorkDate"] = sFromDate;
                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dsExceptionEmployeeAttendanceUnlock.Tables[0].Rows.Add(dr);


                            }
                            else
                            {
                                //edit
                                DataRow dr = dvExceptionEmployeeAttendanceUnlock[0].Row;

                                dr.BeginEdit();
                                dr["PlantId"] = identity.PlantId;
                                dr["EmpSystemId"] = LockDateWiseEmployeeList[i].ToString();
                                dr["IsActive"] = true;
                                dr["WorkDate"] = sFromDate;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dr.EndEdit();

                            }
                            //dsIsSalaryProc.Tables[0].DefaultView.RowFilter = "EmpInfoSystemID='" + LockDateWiseEmployeeList[i].ToString() + "'";
                            DataView dvIsSalaryProc = new DataView(dsIsSalaryProc.Tables[0])
                            {
                                RowFilter = "EmpInfoSystemID='" + LockDateWiseEmployeeList[i].ToString() + "'"
                            };

                            if (dvIsSalaryProc.Count > 0)
                            {
                                //dsMasterValidation.Tables[0].DefaultView.RowFilter = "EmpInfoSystemID='" + LockDateWiseEmployeeList[i].ToString() + "'";
                                DataView dvMasterValidation = new DataView(dsMasterValidation.Tables[0])
                                {
                                    RowFilter = "EmpSystemID='" + LockDateWiseEmployeeList[i].ToString() + "'"
                                };
                                if (dvMasterValidation.Count > 0)
                                {
                                    throw new Exception("Salary is approved. Employee Code [" + GetEmpCode(dvMasterValidation[0]["EmpSystemID"].ToString()) + "]");

                                }
                                else
                                {
                                    DataView dv = new DataView(dsMasterValidationLog.Tables[0])
                                    {
                                        RowFilter = "EmpSystemId='" + LockDateWiseEmployeeList[i].ToString() + "'"
                                    };

                                    if (dv.Count == 0)
                                    {
                                        //string sID = string.Empty;
                                        //bplib.clsGenID objGenID = new bplib.clsGenID();
                                        //objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployeeSalaryReprocess", out sID);
                                        DataRow dr = dsMasterValidationLog.Tables[0].NewRow();
                                        dr["Id"] = "SU" + ExceptionEmployeeSalaryReprocessPK + "_" + ExceptionEmployeeSalaryReprocess;
                                        dr["EmpSystemId"] = LockDateWiseEmployeeList[i].ToString();
                                        dr["PlantId"] = identity.PlantId;
                                        dr["IsActive"] = true;
                                        DateTime date = Convert.ToDateTime(sFromDate);
                                        dr["YearNo"] = date.Year.ToString();
                                        dr["MonthNo"] = date.Month.ToString();
                                        dr["AddedBy"] = identity.Name;
                                        dr["AddedDate"] = System.DateTime.Now.ToString();
                                        dr["AddedFromIP"] = identity.IPAddress;
                                        dr["UpdatedBy"] = identity.Name;
                                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                        dr["UpdatedFromIP"] = identity.IPAddress;
                                        dsMasterValidationLog.Tables[0].Rows.Add(dr);


                                    }
                                    else
                                    {
                                        //edit
                                        DataRow dr = dv[0].Row;

                                        dr.BeginEdit();
                                        dr["PlantId"] = identity.PlantId;
                                        dr["EmpSystemId"] = LockDateWiseEmployeeList[i].ToString();
                                        dr["IsActive"] = true;
                                        DateTime date = Convert.ToDateTime(sFromDate);
                                        dr["YearNo"] = date.Year.ToString();
                                        dr["MonthNo"] = date.Month.ToString();
                                        dr["UpdatedBy"] = identity.Name;
                                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                        dr["UpdatedFromIP"] = identity.IPAddress;
                                        dr.EndEdit();

                                    }
                                    dv.RowFilter = null;
                                }
                                dvMasterValidation.RowFilter = null;
                            }




                            dvExceptionEmployeeAttendanceUnlock.RowFilter = null;
                            dvIsSalaryProc.RowFilter = null;


                        }



                        //====================




                        //date increment
                        ExceptionEmployeeAttendanceUnlock++;
                        ExceptionEmployeeSalaryReprocess++;
                        sFromDate = sFromDate.AddDays(1);
                    }



                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsExceptionEmployeeAttendanceUnlock, dsMasterValidationLog);


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }




        public void CreateUnLockDataEmployeeWise(string lockDate, string[] LockDateWiseEmployeeList, CustomIdentity identity)
        {

            string EmployeeIds = string.Empty;
            ConnectionManager.DAL.ConManager objCon;

            for (int i = 0; i < LockDateWiseEmployeeList.Length; i++)
            {
                if (string.IsNullOrEmpty(EmployeeIds))
                {
                    EmployeeIds = "'" + LockDateWiseEmployeeList[i].ToString() + "'";
                }
                else
                {
                    EmployeeIds += ",'" + LockDateWiseEmployeeList[i].ToString() + "'";
                }
            }

            try
            {
                string sql = @"SELECT * FROM ExceptionEmployeeAttendanceUnlock where WorkDate='" + lockDate + "' AND EmpSystemId IN (" + EmployeeIds + @")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsExceptionEmployeeAttendanceUnlock, false, "1");

                string sqlvalidation = @"SELECT * FROM ExceptionEmployeeSalaryReprocess WHERE  EmpSystemId IN (" + EmployeeIds + @") AND MonthNo=MONTH('" + lockDate + @"') AND YearNo=YEAR('" + lockDate + @"')";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlvalidation, out DataSet dsMasterValidation, false, "1");

                string sqlIsSalaryProc = @" SELECT spm.*,spc.EmpInfoSystemID FROM SalaryProcMaster AS spm 
                                            LEFT JOIN SalaryProcChild AS spc ON spc.SlrProcMstSystemID = spm.SystemID
                                            WHERE   spc.EmpInfoSystemID IN (" + EmployeeIds + @")
                                            AND spm.YearNo=YEAR('" + lockDate + @"') AND spm.MonthNo=MONTH('" + lockDate + @"')";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlIsSalaryProc, out DataSet dsIsSalaryProc, false, "1");

                string sqlvalidationLog = @"SELECT * FROM ExceptionEmployeeSalaryReprocess WHERE EmpSystemId IN (" + EmployeeIds + @") AND MonthNo=MONTH('" + lockDate + @"') AND YearNo=YEAR('" + lockDate + @"')";




                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlvalidationLog, out DataSet dsMasterValidationLog, false, "1");

                //dsMasterValidation.Tables[0].DefaultView.RowFilter = "EmpSystemId='" + LockDateWiseEmployeeList[i].ToString() + "' AND PlantId='" + identity.PlantId + "'";
                //if (dsMasterValidation.Tables[0].Rows.Count > 0)
                //{
                //    throw new Exception("Salary is approved.");

                //}



                for (int i = 0; i < LockDateWiseEmployeeList.Length; i++)
                {
                    DataView dvExceptionEmployeeAttendanceUnlock = new DataView(dsExceptionEmployeeAttendanceUnlock.Tables[0])
                    {
                        RowFilter = "EmpSystemId='" + LockDateWiseEmployeeList[i].ToString() + "'  AND WorkDate='" + lockDate + "'"
                    };
                    if (dvExceptionEmployeeAttendanceUnlock.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployeeAttendanceUnlock", out sID);
                        DataRow dr = dsExceptionEmployeeAttendanceUnlock.Tables[0].NewRow();
                        dr["Id"] = "AU" + sID;
                        dr["EmpSystemId"] = LockDateWiseEmployeeList[i].ToString();
                        dr["PlantId"] = identity.PlantId;
                        dr["IsActive"] = true;
                        dr["WorkDate"] = lockDate;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsExceptionEmployeeAttendanceUnlock.Tables[0].Rows.Add(dr);


                    }
                    else
                    {
                        //edit
                        DataRow dr = dvExceptionEmployeeAttendanceUnlock[0].Row;

                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId;
                        dr["EmpSystemId"] = LockDateWiseEmployeeList[i].ToString();
                        dr["IsActive"] = true;
                        dr["WorkDate"] = lockDate;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                    }
                    //dsIsSalaryProc.Tables[0].DefaultView.RowFilter = "EmpInfoSystemID='" + LockDateWiseEmployeeList[i].ToString() + "'";
                    DataView dvIsSalaryProc = new DataView(dsIsSalaryProc.Tables[0])
                    {
                        RowFilter = "EmpInfoSystemID='" + LockDateWiseEmployeeList[i].ToString() + "'"
                    };

                    if (dvIsSalaryProc.Count > 0)
                    {
                        //dsMasterValidation.Tables[0].DefaultView.RowFilter = "EmpInfoSystemID='" + LockDateWiseEmployeeList[i].ToString() + "'";
                        DataView dvMasterValidation = new DataView(dsMasterValidation.Tables[0])
                        {
                            RowFilter = "EmpInfoSystemID='" + LockDateWiseEmployeeList[i].ToString() + "'"
                        };
                        if (dvMasterValidation.Count > 0)
                        {
                            throw new Exception("Salary is approved. Employee Code [" + GetEmpCode(dvMasterValidation[0]["EmpInfoSystemID"].ToString()) + "]");

                        }
                        else
                        {
                            DataView dv = new DataView(dsMasterValidationLog.Tables[0])
                            {
                                RowFilter = "EmpSystemId='" + LockDateWiseEmployeeList[i].ToString() + "'"
                            };

                            if (dv.Count == 0)
                            {
                                string sID = string.Empty;
                                bplib.clsGenID objGenID = new bplib.clsGenID();
                                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployeeSalaryReprocess", out sID);
                                DataRow dr = dsMasterValidationLog.Tables[0].NewRow();
                                dr["Id"] = "SU" + sID;
                                dr["EmpSystemId"] = LockDateWiseEmployeeList[i].ToString();
                                dr["PlantId"] = identity.PlantId;
                                dr["IsActive"] = true;
                                DateTime date = Convert.ToDateTime(lockDate);
                                dr["YearNo"] = date.Year.ToString();
                                dr["MonthNo"] = date.Month.ToString();
                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dsMasterValidationLog.Tables[0].Rows.Add(dr);


                            }
                            else
                            {
                                //edit
                                DataRow dr = dv[0].Row;

                                dr.BeginEdit();
                                dr["PlantId"] = identity.PlantId;
                                dr["EmpSystemId"] = LockDateWiseEmployeeList[i].ToString();
                                dr["IsActive"] = true;
                                DateTime date = Convert.ToDateTime(lockDate);
                                dr["YearNo"] = date.Year.ToString();
                                dr["MonthNo"] = date.Month.ToString();
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dr.EndEdit();

                            }
                            dv.RowFilter = null;
                        }
                        dvMasterValidation.RowFilter = null;
                    }




                    dvExceptionEmployeeAttendanceUnlock.RowFilter = null;
                    //dsMasterValidationLog.Tables[0].DefaultView.RowFilter = null;
                    //dsIsSalaryProc.Tables[0].DefaultView.RowFilter = null;
                    dvIsSalaryProc.RowFilter = null;

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsExceptionEmployeeAttendanceUnlock, dsMasterValidationLog);


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public void CreateReLockDataEmployeeWise(string lockDate, string[] LockDateWiseEmployeeList, CustomIdentity identity)
        {
            clsDailyAllowance odailyAllowance = new clsDailyAllowance();

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            bool IsOTConfirmationAfterLock = false;



            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
            if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
            {
                //IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());
                //IsOutMissingValidationRequired = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOutMissingValidationRequired"].ToString().Trim());
                //if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                //{
                //    IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                //}
                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                {
                    IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                }
            }

            try
            {

                List<Dictionary<string, object>> OTConfirmationData = (List<Dictionary<string, object>>)GetOTConfirmationData(lockDate);

                List<Dictionary<string, object>> UnApprovedEmployeeListData = (List<Dictionary<string, object>>)GetUnApprovedEmployeeListData(lockDate);

                List<Dictionary<string, object>> ShiftNotAssignData = (List<Dictionary<string, object>>)GetShiftNotAssignData(lockDate);

                List<Dictionary<string, object>> AttdencenotNotProcData = (List<Dictionary<string, object>>)GetAttdencenotNotProcData(lockDate);


                for (int i = 0; i < LockDateWiseEmployeeList.Length; i++)
                {


                    if (IsOTConfirmationAfterLock == false)
                    {
                        //List<Dictionary<string, object>> OTConfirmationData = (List<Dictionary<string, object>>)GetOTConfirmationData(lockDate);
                        if (OTConfirmationData.Count() > 0)
                        {
                            List<Dictionary<string, object>> OT = OTConfirmationData.Where(ee => ee["EmpSystemId"].ToString() == LockDateWiseEmployeeList[i].ToString()).ToList();
                            if (OT.Count() > 0)
                            {
                                throw new Exception("Please Confirmed  OT. Employee Code [ " + GetEmpCode(LockDateWiseEmployeeList[i].ToString()) + " ].");
                            }
                        }
                    }




                    //List<Dictionary<string, object>> UnApprovedEmployeeListData = (List<Dictionary<string, object>>)GetUnApprovedEmployeeListData(lockDate);
                    if (UnApprovedEmployeeListData.Count() > 0)
                    {


                        List<Dictionary<string, object>> UAE = UnApprovedEmployeeListData.Where(ee => ee["SystemID"].ToString() == LockDateWiseEmployeeList[i].ToString()).ToList();
                        if (UAE.Count() > 0)
                        {
                            throw new Exception("Please Confirmed all Employees  Approved. Employee Code [ " + GetEmpCode(LockDateWiseEmployeeList[i].ToString()) + " ].");
                        }


                    }

                    //List<Dictionary<string, object>> ShiftNotAssignData = (List<Dictionary<string, object>>)GetShiftNotAssignData(lockDate);
                    if (ShiftNotAssignData.Count() > 0)
                    {
                        List<Dictionary<string, object>> SNA = ShiftNotAssignData.Where(ee => ee["SystemId"].ToString() == LockDateWiseEmployeeList[i].ToString()).ToList();
                        if (SNA.Count() > 0)
                        {
                            throw new Exception("Please Confirmed all Employees Shift Assign. Employee Code [ " + GetEmpCode(LockDateWiseEmployeeList[i].ToString()) + " ].");
                        }


                    }
                    //List<Dictionary<string, object>> AttdencenotNotProcData = (List<Dictionary<string, object>>)GetAttdencenotNotProcData(lockDate);
                    if (AttdencenotNotProcData.Count() > 0)
                    {

                        List<Dictionary<string, object>> ANP = AttdencenotNotProcData.Where(ee => ee["SystemId"].ToString() == LockDateWiseEmployeeList[i].ToString()).ToList();
                        if (ANP.Count() > 0)
                        {
                            throw new Exception("Please Confirmed all Employees Attdence Proc. Employee Code [ " + GetEmpCode(LockDateWiseEmployeeList[i].ToString()) + " ].");
                        }


                    }


                }




                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");
                }






                string result = "";
                for (int i = 0; i < LockDateWiseEmployeeList.Length; i++)
                {

                    if (result == "")
                        result = "'" + LockDateWiseEmployeeList[i].ToString() + "'";
                    else
                        result = result + ",'" + LockDateWiseEmployeeList[i].ToString() + "'";
                }

                SaveDailyAllowanceTransactionEmpWise(lockDate, result);
                //odailyAllowance.UpdateDailyAllowanceSummaryData(identity, lockDate,result);
                if (bplib.clsWebLib.IsDateOK(lockDate) == false)
                {
                    throw new Exception("Invalid date format for Lock. Expected date format is 'dd-MMM-yyyy' (eg. 01-Jan-2019)");
                }
                string sqld = @"DELETE FROM ExceptionEmployeeAttendanceUnlock where WorkDate='" + lockDate + "' AND EmpSystemId IN (" + result + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqld, out DataSet dsMaster, false, "1");
                //objCon.ExecuteNonQueryWrapper(sqld, true, "1");




            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }




        public void CreateEmployeeIndividualAttendanceLock(string EmpSystemId, string[] LockDateList, string LockType, CustomIdentity identity)
        {


            ConnectionManager.DAL.ConManager objCon;


            try
            {

                DataSet dsLocalHRMSSetting = null;
                clsStaticInfo objStatic = null;
                objStatic = new clsStaticInfo();
                //bool IsOTConfirmationAuto = false;
                ////bool IsOTConfirmationAutoException = false;
                //bool IsOutMissingValidationRequired = false;
                //bool IsOTConfirmationAutoForZeroAuto = false;
                bool IsOTConfirmationAfterLock = false;



                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());
                    //IsOutMissingValidationRequired = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOutMissingValidationRequired"].ToString().Trim());
                    //if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                    //{
                    //    IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                    //}
                    if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                    {
                        IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                    }
                }




                foreach (var lockDate in LockDateList)
                {
                    List<Dictionary<string, object>> OTConfirmationData = (List<Dictionary<string, object>>)GetOTConfirmationData(lockDate);

                    List<Dictionary<string, object>> UnApprovedEmployeeListData = (List<Dictionary<string, object>>)GetUnApprovedEmployeeListData(lockDate);

                    List<Dictionary<string, object>> ShiftNotAssignData = (List<Dictionary<string, object>>)GetShiftNotAssignData(lockDate);

                    List<Dictionary<string, object>> AttdencenotNotProcData = (List<Dictionary<string, object>>)GetAttdencenotNotProcData(lockDate);


                    if (!string.IsNullOrEmpty(EmpSystemId))
                    {

                        if (IsOTConfirmationAfterLock == false)
                        {
                            if (OTConfirmationData.Count() > 0)
                            {
                                List<Dictionary<string, object>> OT = OTConfirmationData.Where(ee => ee["EmpSystemId"].ToString() == EmpSystemId.ToString()).ToList();
                                if (OT.Count() > 0)
                                {
                                    throw new Exception("Please Confirmed  OT. Employee Code [ " + GetEmpCode(EmpSystemId.ToString()) + " ].");
                                }
                            }
                        }



                        if (UnApprovedEmployeeListData.Count() > 0)
                        {

                            List<Dictionary<string, object>> OT = UnApprovedEmployeeListData.Where(ee => ee["SystemID"].ToString() == EmpSystemId.ToString()).ToList();
                            if (OT.Count() > 0)
                            {
                                throw new Exception("Please Confirmed all Employees  Approved.");
                            }



                        }

                        //List<Dictionary<string, object>> ShiftNotAssignData = (List<Dictionary<string, object>>)GetShiftNotAssignData(lockDate);
                        if (ShiftNotAssignData.Count() > 0)
                        {
                            List<Dictionary<string, object>> OT = ShiftNotAssignData.Where(ee => ee["SystemId"].ToString() == EmpSystemId.ToString()).ToList();
                            if (OT.Count() > 0)
                            {
                                throw new Exception("Please Confirmed all Employees Shift Assign.");
                            }


                        }
                        //List<Dictionary<string, object>> AttdencenotNotProcData = (List<Dictionary<string, object>>)GetAttdencenotNotProcData(lockDate);
                        if (AttdencenotNotProcData.Count() > 0)
                        {
                            List<Dictionary<string, object>> OT = AttdencenotNotProcData.Where(ee => ee["SystemId"].ToString() == EmpSystemId.ToString()).ToList();
                            if (OT.Count() > 0)
                            {
                                throw new Exception("Please Confirmed all Employees Attdence Proc.");
                            }


                        }


                    }
                }





                string dates = string.Empty;
                foreach (var item in LockDateList)
                {
                    if (dates == "")
                        dates = "'" + item.ToString() + "'";
                    else
                        dates = dates + ",'" + item.ToString() + "'";
                }

                foreach (var lockDate in LockDateList)
                {
                    SaveDailyAllowanceTransactionEmpWise(lockDate, EmpSystemId);
                }



                string sql = @"SELECT * FROM IndividualEmployeeAttendancelock where WorkDate IN (" + dates + ") AND LockType='" + LockType + @"' AND EmpSystemId='" + EmpSystemId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsExceptionEmployeeAttendanceUnlock, false, "1");




                for (int i = 0; i < LockDateList.Length; i++)
                {
                    DataView dvExceptionEmployeeAttendanceUnlock = new DataView(dsExceptionEmployeeAttendanceUnlock.Tables[0])
                    {
                        RowFilter = "EmpSystemId='" + EmpSystemId + "'  AND WorkDate='" + LockDateList[i].ToString() + "'"
                    };
                    if (dvExceptionEmployeeAttendanceUnlock.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "IndividualEmployeeAttendancelock", out sID);
                        DataRow dr = dsExceptionEmployeeAttendanceUnlock.Tables[0].NewRow();
                        dr["Id"] = "IAL" + sID;
                        dr["EmpSystemId"] = EmpSystemId;
                        dr["PlantId"] = identity.PlantId;
                        dr["IsActive"] = true;
                        dr["WorkDate"] = LockDateList[i].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["LockType"] = LockType;
                        dsExceptionEmployeeAttendanceUnlock.Tables[0].Rows.Add(dr);


                    }
                    else
                    {
                        //edit
                        DataRow dr = dvExceptionEmployeeAttendanceUnlock[0].Row;

                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId;
                        dr["EmpSystemId"] = EmpSystemId;
                        dr["IsActive"] = true;
                        dr["WorkDate"] = LockDateList[i].ToString();
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        //dr["LockType"] = "SEPARATED";
                        dr["LockType"] = LockType;
                        dr.EndEdit();

                    }




                    dvExceptionEmployeeAttendanceUnlock.RowFilter = null;


                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsExceptionEmployeeAttendanceUnlock);


            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
    
        public void CreateEmployeeIndividualAttendanceUnLock(string EmpSystemId, string[] lockDateList, CustomIdentity identity)
        {

            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string FromDate = "";

                string result = "";
                for (int i = 0; i < lockDateList.Length; i++)
                {

                    if (result == "")
                    {
                        result = "'" + lockDateList[i].ToString() + "'";
                        FromDate = lockDateList[i].ToString();
                    }
                    else
                        result = result + ",'" + lockDateList[i].ToString() + "'";
                }







                string sqlvalidation = @" SELECT * FROM SalaryLock WHERE EmpSystemId =" + EmpSystemId + @" AND YearNo=YEAR('" + FromDate + @"') AND MonthNo=MONTH('" + FromDate + @"') AND IsLocked=1 ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlvalidation, out DataSet dsMasterValidation, false, "1");


                if (dsMasterValidation.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Salary is approved.");

                }



                string sqld = @"DELETE FROM IndividualEmployeeAttendancelock where WorkDate   IN (" + result + ")  AND EmpSystemId ='" + EmpSystemId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqld, out DataSet dsMaster, false, "1");
                //objCon.ExecuteNonQueryWrapper(sqld, true, "1");




            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public string GetEmpCode(string EmpSystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string Result = string.Empty;
            ConnectionManager.DAL.ConManager objCon;



            string sqld = @"SELECT EmployeeCode FROM EmployeeInformation WHERE SystemId='" + EmpSystemId + "' AND PlantId='" + identity.PlantId + "' ";

            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sqld, out DataSet dsMaster, false, "1");
            if (dsMaster.Tables[0].Rows.Count > 0)
            {
                Result = dsMaster.Tables[0].Rows[0]["EmployeeCode"].ToString();
            }
            return Result;

        }
        #endregion
        public void CheckSalaryLock(string FromDate, string ToDate, string[] LockDateWiseEmployeeList, CustomIdentity identity)
        {


            ConnectionManager.DAL.ConManager objCon;
            string EmployeeIds = string.Empty;

            try
            {
                if (Convert.ToDateTime(FromDate).Month == Convert.ToDateTime(ToDate).Month && Convert.ToDateTime(FromDate).Year == Convert.ToDateTime(ToDate).Year)
                {
                    //
                }
                else
                {
                    throw new Exception("From date and To Date must be same month.");
                }



                for (int i = 0; i < LockDateWiseEmployeeList.Length; i++)
                {
                    if (string.IsNullOrEmpty(EmployeeIds))
                    {
                        EmployeeIds = "'" + LockDateWiseEmployeeList[i].ToString() + "'";
                    }
                    else
                    {
                        EmployeeIds += ",'" + LockDateWiseEmployeeList[i].ToString() + "'";
                    }
                }







                string sqlvalidation = @" SELECT * FROM SalaryLock WHERE EmpSystemId IN (" + EmployeeIds + @") AND YearNo=YEAR('" + FromDate + @"') AND MonthNo=MONTH('" + FromDate + @"') AND IsLocked=1 ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlvalidation, out DataSet dsMasterValidation, false, "1");



                for (int i = 0; i < LockDateWiseEmployeeList.Length; i++)
                {



                    DateTime sFromDate = Convert.ToDateTime(FromDate.ToString());
                    DateTime sToDate = Convert.ToDateTime(ToDate.ToString());
                    while (sFromDate <= sToDate)//date wise loop
                    {


                        DataView dvMasterValidation = new DataView(dsMasterValidation.Tables[0])
                        {
                            RowFilter = "EmpSystemID='" + LockDateWiseEmployeeList[i].ToString() + "'"
                        };
                        if (dvMasterValidation.Count > 0)
                        {
                            throw new Exception("Salary is approved. Employee Code [" + GetEmpCode(dvMasterValidation[0]["EmpSystemID"].ToString()) + "]");

                        }


               ;
                        sFromDate = sFromDate.AddDays(1);
                    }



                }



            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

    }
    public class AllowanceDaily : BaseModel
    {
        public string Id { get; set; }
        public string Catagory { get; set; }
    }
    public class DailyAllowanceRateBasedOnSalaryRange
    {
        public string Id { get; set; }
        public string PlantID { get; set; }
        public string DailyAllowanceId { get; set; }
        public decimal SalaryRangeUpperLimit { get; set; }
        public decimal SalaryRangeLowerLimit { get; set; }
        public decimal Rate { get; set; }



    }

    public class DailyAllowanceTransaction : BaseModel
    {
        public string Id { get; set; }
        public string PlantId { get; set; }
        public DateTime WorkDate { get; set; }
        public string EmpSystemId { get; set; }
        public string AllowanceDailyId { get; set; }
        public string Quantity { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public DateTime EffectiveTime { get; set; }
        public DateTime PunchInTime { get; set; }
        public DateTime PunchOutTime { get; set; }
        public DateTime InTime { get; set; }

        public string SalaryHeadId { get; set; }
        public bool IsAllDesignation { get; set; }
        public bool IsFixed { get; set; }
        public decimal Rate { get; set; } = 0;
        public string FormulaDesID { get; set; }
        public bool DARIsFixed { get; set; }
        public decimal DARRate { get; set; } = 0;
        public string DARFormulaDesID { get; set; }
        public string DayType { get; set; }
        public string DurationInMin { get; set; }
        public string OTDuration { get; set; }
        public string Catagory { get; set; }
        public bool IsRateBasedOnSalaryRange { get; set; }
        public string SalaryRangeBasedOnSalaryHeadId { get; set; }
        //public bool IsVoucherPayment { get; set; }
    }





}
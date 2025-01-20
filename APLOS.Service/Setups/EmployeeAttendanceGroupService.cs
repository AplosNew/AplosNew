#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Setups;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class EmployeeAttendanceGroupService : Service<EmployeeAttendanceGroup >, IEmployeeAttendanceGroupService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<EmployeeAttendanceGroup> _EmployeeAttendanceGroupRepository;
        private readonly IRepositoryAsync<EmployeeLeaveSummary> _EmployeeLeaveSummaryRepository;

        public EmployeeAttendanceGroupService(
            IRepositoryAsync<EmployeeAttendanceGroup> EmployeeAttendanceGroupRepository,
            IRepositoryAsync<EmployeeLeaveSummary> EmployeeLeaveSummaryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            ISqlRepository sqlRepository) : base(EmployeeAttendanceGroupRepository, unitOfWork, pkGeneratorService)
        {
            _EmployeeAttendanceGroupRepository = EmployeeAttendanceGroupRepository;
            _EmployeeLeaveSummaryRepository = EmployeeLeaveSummaryRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor


        public void InsertOrUpdateGraph(IEnumerable<EmployeeAttendanceGroup> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please Select Attendance Group and Employee");
                _unitOfWork.BeginTransaction();
                flag = true;
                //var pk = GetMaxNumber(nameof(EmployeeAttendanceGroup), PKGeneratorEnum.Yearly, null, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        //pk.MaxNumber++;
                        item.Id = item.EmployeeId;
                        InsertGraph(item);
                    }
                    else
                    {
                        UpdateGraph(item);
                    }
                }
               
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InSertOrUpdate(EmployeeAttendanceGroup entity)
        {
            var data = Query(t => t.EmployeeId == entity.EmployeeId).Select().FirstOrDefault();
            //var pk = GetMaxNumber(nameof(EmployeeAttendanceGroup), PKGeneratorEnum.Yearly, null, DateTime.Now);
            if (data == null)
            {
                if (string.IsNullOrEmpty(entity.Id))
                {
                   // pk.MaxNumber++;
                    entity.Id = entity.EmployeeId;
                    Insert(entity);
                }
            }
            else
            {
                data.AttendanceGroupId = entity.AttendanceGroupId;
                Update(data);
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                var dbList = base.Query(t => t.Id == id).Select().AsEnumerable();
                if (dbList != null)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    foreach (var item in dbList)
                    {
                        base.DeleteGraph(item);
                    }
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(EmployeeAttendanceGroup), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public GridModel Query(GridParameter parameters, string companyGroupId,string attendanceGroupId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT PG.*,E.EmployeeName,E.EmployeeCode,E.GivenDesignationId,PR.DepartmentId,PR.DivisionId,PR.SectionId
                                           ,EC.Id EmployeeCategoryId,EC.UserName EmployeeCategory
                                           ,GD.UserName GivenDesignation,D.UserName Department,DV.UserName Division
                                           ,S.UserName Section FROM 
                                         EmployeeInformation E 
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
										LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                        LEFT JOIN ORG.Department D ON PR.DepartmentId=D.Id
                                        LEFT JOIN ORG.Division DV ON PR.DivisionId=DV.Id
                                        LEFT JOIN ORG.Section S ON PR.SectionId= S.Id
										LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
										LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                        LEFT JOIN EmployeeAttendanceGroup AG ON AG.EmployeeId=E.SystemId AND AG.AttendanceGroupId='" + attendanceGroupId + @"' 
                                        WHERE  E.GroupID='"+companyGroupId+@"' AND E.PlantId = '"+ plantId + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IEnumerable<object> AttendanceGroupQuery(string companyGroupId, string attendanceGroupId, string plantId)
        {
            try
            {
                string CmdText = @"SELECT AG.*,E.EmployeeName,E.EmployeeCode,E.GivenDesignationId,PR.DepartmentId,PR.DivisionId,PR.SectionId
                                           ,EC.Id EmployeeCategoryId,EC.UserName EmployeeCategory
                                           ,GD.UserName GivenDesignation,D.UserName Department,DV.UserName Division
                                           ,S.UserName Section FROM 
                                         EmployeeInformation E 
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
										LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                        LEFT JOIN ORG.Department D ON PR.DepartmentId=D.Id
                                        LEFT JOIN ORG.Division DV ON PR.DivisionId=DV.Id
                                        LEFT JOIN ORG.Section S ON PR.SectionId= S.Id
										LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
										LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                        LEFT JOIN [dbo].[EmployeeAttendanceGroup] AG ON AG.EmployeeId=E.SystemId AND AG.AttendanceGroupId='" + attendanceGroupId + @"' 
                                        WHERE  E.GroupID='" + companyGroupId + @"' AND E.PlantId = '" + plantId + @"' AND AG.AttendanceGroupId='" + attendanceGroupId + @"' AND E.EmployeeStatus='Active'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public GridModel QueryWithEmployee(GridParameter parameters, string companyGroupId, string employeeId, string[] attendanceGroupIds)
        {
            try
            {
                var wc = string.Empty;
                if (attendanceGroupIds.Length >0)
                {
                     wc = "AND PG.PayrollGroupId NOT IN(" + ReturnStringArray(attendanceGroupIds) + ")";
                }
                parameters.CmdText = @"SELECT P.Id,PG.EmployeeId,P.[Sequence], P.Code, P.UserName, P.ShortName, P.StandardName
                                    FROM [MST].[EmployeeAttendanceGroup] PG
                                    LEFT JOIN HKP.PayrollGroup p on PG.PayrollGroupId=p.Id
                                    WHERE PG.EmployeeId='" + employeeId + "' AND PG.CompanyGroupId='" + companyGroupId + "' " + wc + "";


                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel QueryWithUser(GridParameter parameters, string companyGroupId, string userId)
        {
            try
            {
              
                parameters.CmdText = @"SELECT P.Id,P.[Sequence], P.Code, P.UserName, P.ShortName, P.StandardName
                                    FROM  HKP.PayrollGroup p 
                                    WHERE   p.Id NOT IN (SELECT PayrollGroupId from SEC.UserPayrollGroup WHERE UserId ='" + userId+@"')
                                     ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public void SalaryProcessDelete( string employeesId, string month, string year)
        {
            try
            {


                string sqlQry = @"DELETE FROM SalaryProcChild
			                                WHERE SlrProcMstSystemID IN (
										                                SELECT SystemID
										                                FROM SalaryProcMaster
										                                WHERE MonthNo = "+ month + @"
											                                AND YearNo = " + year + @"
										                                )
				                                AND IsApproved = 0
				                                AND IsDisbursed = 0
				                                AND EmpInfoSystemID IN (" + employeesId + @")

                                DELETE FROM BonusPolicyMonthlyRetainDistributionPmt
			                                WHERE BnsPlyMntRetainID IN (
										                                SELECT  id
										                                FROM BonusPolicyMonthlyRetainEmpWiseCalculation
										                                WHERE MonthNo = " + month + @"
											                                AND YearNo = " + year + @"
											                                AND EmpSystemID IN (" + employeesId + @")
										                                )
                                DELETE FROM BonusPolicyMonthlyRetainEmpWiseCalculation
			                                WHERE SlrProcMstSystemID IN (
										                                SELECT SystemID
										                                FROM SalaryProcMaster
										                                WHERE MonthNo = " + month + @"
											                                AND YearNo = " + year + @"
											                                AND EmpSystemID IN (" + employeesId + @")
										                                )
			                                AND EmpSystemID IN (" + employeesId + @")


                                DELETE FROM BonusPolicyMonthlyRetainDistributionStrcPmt
			                                WHERE BnsPlyMntRetainID IN (
										                                SELECT Id
										                                FROM BonusPolicyMonthlyRetainStrcEmpWiseCalculation
										                                WHERE MonthNo = " + month + @"
											                                AND YearNo = " + year + @"
											                                AND EmpSystemID IN (" + employeesId + @")
										                                )

                                DELETE FROM BonusPolicyMonthlyRetainStrcEmpWiseCalculation
			                                WHERE SlrProcMstSystemID IN (
										                                SELECT SystemID
										                                FROM SalaryProcMaster
										                                WHERE MonthNo = " + month + @"
											                                AND YearNo = " + year + @"
											                                AND EmpSystemID IN (" + employeesId + @")
										                                )
			                                AND 
			
			                                EmpSystemID IN (" + employeesId + @")
                                DELETE FROM SalaryProceAttdnData
			                                WHERE SlrProcMstSystemId IN (
					                                SELECT SystemID
					                                FROM SalaryProcMaster
					                                WHERE MonthNo = " + month + @"
						                                AND YearNo = " + year + @"
						                                AND EmpSystemID IN (" + employeesId + @")
					                                )
			                                AND EmpSystemID IN (" + employeesId + @")
                              
                                UPDATE TaxDeductionInfoMonthWise
                                SET SlrProcMstSystemID = NULL
                                WHERE SlrProcMstSystemID IN (
		                                SELECT SystemID
		                                FROM SalaryProcMaster
		                                WHERE MonthNo = " + month + @"
			                                AND YearNo = " + year + @"
		                                )
		                                AND EmpInfoSystemID IN (" + employeesId + @")";
                _sqlRepository.ExecuteSqlCommand(sqlQry);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
    }
}
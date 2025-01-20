#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Payrolls;
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

namespace Library.Service.Payrolls
{
    public class ParollsReportService : IParollsReportService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ParollsReportService(
            IUnitOfWork unitOfWork,
            ISqlRepository sqlRepository) 
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
        
        
        public GridModel Query(GridParameter parameters, string companyGroupId,string payrollGroupId,string plantId)
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
                                        LEFT JOIN [MST].[PayrollGroupMaster] PG ON PG.EmployeeId=E.SystemId AND PG.PayrollGroupId='" + payrollGroupId + @"' 
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

        public IEnumerable<object> PayRollGroupQuery(string companyGroupId, string payrollGroupId, string plantId)
        {
            try
            {
                var sql = @"SELECT PG.*,E.EmployeeName,E.EmployeeCode,E.GivenDesignationId,PR.DepartmentId,PR.DivisionId,PR.SectionId
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
                                        LEFT JOIN [MST].[PayrollGroupMaster] PG ON PG.EmployeeId=E.SystemId AND PG.PayrollGroupId='" + payrollGroupId + @"' 
                                        WHERE  E.GroupID='" + companyGroupId + @"' AND E.PlantId = '" + plantId + @"' AND PG.PayrollGroupId='" + payrollGroupId + @"' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public GridModel QueryWithEmployee(GridParameter parameters, string companyGroupId, string employeeId, string[] payrollGroupIds)
        {
            try
            {
                var wc = string.Empty;
                if (payrollGroupIds.Length >0)
                {
                     //wc = "AND PG.PayrollGroupId NOT IN(" + ReturnStringArray(payrollGroupIds) + ")";
                }
                parameters.CmdText = @"SELECT P.Id,PG.EmployeeId,P.[Sequence], P.Code, P.UserName, P.ShortName, P.StandardName
                                    FROM [MST].[PayrollGroupMaster] PG
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
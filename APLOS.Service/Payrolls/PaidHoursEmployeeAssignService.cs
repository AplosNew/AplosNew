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
    public class PaidHoursEmployeeAssignService : Service<PaidHoursEmployeeAssign>, IPaidHoursEmployeeAssignService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PaidHoursEmployeeAssign> _PaidHoursEmployeeAssignRepository;
        private readonly IRepositoryAsync<EmployeeLeaveSummary> _EmployeeLeaveSummaryRepository;

        public PaidHoursEmployeeAssignService(
            IRepositoryAsync<PaidHoursEmployeeAssign> PaidHoursEmployeeAssignRepository,
            IRepositoryAsync<EmployeeLeaveSummary> EmployeeLeaveSummaryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            ISqlRepository sqlRepository) : base(PaidHoursEmployeeAssignRepository, unitOfWork, pkGeneratorService)
        {
            _PaidHoursEmployeeAssignRepository = PaidHoursEmployeeAssignRepository;
            _EmployeeLeaveSummaryRepository = EmployeeLeaveSummaryRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor


        public void InsertOrUpdateGraph(IEnumerable<PaidHoursEmployeeAssign> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please select paid hours employee assign");
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(PaidHoursEmployeeAssign), PKGeneratorEnum.Yearly, null, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        InsertGraph(item);
                    }
                    else
                    {
                        UpdateGraph(item);
                    }
                }
                //string payrollGroupId = entities.First().PayrollGroupId;
                //string companyGroupId = entities.First().CompanyGroupId;
                //var dbList = base.Query(t => t.PayrollGroupId == payrollGroupId && t.CompanyGroupId == companyGroupId).Select().ToList();
                //if (dbList != null && dbList.Count() > 0)
                //{
                //    if (entities == null)
                //    {
                //        foreach (var item in dbList)
                //        {
                //            base.DeleteGraph(item);
                //        }
                //    }
                //    else
                //    {
                //        foreach (var item in dbList)
                //        {
                //            if (!entities.Any(t => t.Id == item.Id))
                //            {
                //                base.DeleteGraph(item);
                //            }
                //        }
                //    }
                //}
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
            return base.GetMaxNumber(nameof(PaidHoursEmployeeAssign), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public GridModel Query(GridParameter parameters, string companyGroupId, string paidHours,string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT PG.*,E.EmployeeName,E.EmployeeCode,E.GivenDesignationId,PR.DepartmentId,PR.DivisionId,PR.SectionId,EC.Id EmployeeCategoryId,EC.UserName EmployeeCategory,GD.UserName GivenDesignation,D.UserName Department,DV.UserName Division,S.UserName Section FROM [MST].[PaidHoursEmployeeAssign] PG
                                        LEFT JOIN EmployeeInformation E ON PG.EmployeeId=E.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
										LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                        LEFT JOIN ORG.Department D ON pr.DepartmentId=D.Id
                                        LEFT JOIN ORG.Division DV ON PR.DivisionId=DV.Id
                                        LEFT JOIN ORG.Section S ON PR.SectionId= S.Id
										LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
										LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                        WHERE PG.PaidHours='" + paidHours + "' and PG.CompanyGroupId='" + companyGroupId + "' AND PG.PlantId='"+plantId+"'";
                return _sqlRepository.GetGridData(parameters);
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
                parameters.CmdText = @"SELECT P.Id,PG.EmployeeId,P.[Sequence], P.Code, P.UserName, P.ShortName, P.StandardName
                                    FROM [MST].[PaidHoursEmployeeAssign] PG
                                    LEFT JOIN HKP.PayrollGroup p on PG.PayrollGroupId=p.Id
                                    WHERE PG.EmployeeId='" + employeeId + "' AND PG.CompanyGroupId='" + companyGroupId + "' AND PG.PayrollGroupId NOT IN(" + ReturnStringArray(payrollGroupIds) + ")";
                return _sqlRepository.GetGridData(parameters);
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
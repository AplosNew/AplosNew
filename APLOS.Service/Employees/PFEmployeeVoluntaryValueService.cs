#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class PFEmployeeVoluntaryValueService : Service<PFEmployeeVoluntaryValue>, IPFEmployeeVoluntaryValueService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPFEmployeeAppliedService _pFEmployeeAppliedService;

        private readonly IEmployeeInformationService _employeeInformationService;

        public PFEmployeeVoluntaryValueService(
            IRepositoryAsync<PFEmployeeVoluntaryValue> PFEmployeeVoluntaryValueRepository,
            IPKGeneratorService pkGeneratorService,
            IPFEmployeeAppliedService pFEmployeeAppliedService,
            IEmployeeInformationService employeeInformationService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(PFEmployeeVoluntaryValueRepository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _pFEmployeeAppliedService = pFEmployeeAppliedService;
            _employeeInformationService = employeeInformationService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region InsertUpdate

        /// <summary>
        /// CompanyFYPeriod Insert.
        /// </summary>
        /// <param name="entity"></param>
        public void InsertOrUpdate(IEnumerable<PFEmployeeVoluntaryValue> entities)
        {
            bool flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = _pkGeneratorService.GetMaxNumber(nameof(PFEmployeeVoluntaryValue), PKGeneratorEnum.Auto, null, DateTime.Now);
                foreach (var item in entities)
                {
                    var pfOb = _pFEmployeeAppliedService.Query(r => r.ID == item.PFEligibleEmpId).Select().FirstOrDefault();
                    var pfVUser = base.Query(r => r.EmpSystemId == item.EmpSystemId).Select().ToList().FirstOrDefault();
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.ModelState = ModelState.Added;
                        AuditService.Log(item);
                        pfOb.IsVoluntaryPF = item.IsVoluntaryPF;
                        _pFEmployeeAppliedService.UpdateGraph(pfOb);
                        InsertGraph(item);
                    }
                    else if ((pfVUser.EmpSystemId == item.EmpSystemId && pfVUser.EffectiveDate.Month == item.EffectiveDate.Month && pfVUser.EffectiveDate.Year == item.EffectiveDate.Year))
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                        UpdateGraph(item);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException cx)
            {
                throw cx;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(PFEmployeeVoluntaryValue), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(PFEmployeeVoluntaryValue entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
        }

        #endregion InsertUpdate

        public void DeleteGraph(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                PFEmployeeVoluntaryValue PFEmployeeVoluntaryValue = Find(key);
                base.DeleteGraph(PFEmployeeVoluntaryValue);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.CmdText = @"SELECT A.*,E.EmployeeName,D.UserName Department,DE.UserName Designation FROM dbo.PFEmployeeVoluntaryValue A
                LEFT JOIN DBO.EmployeeInformation E ON A.EmpSystemId=E.SystemId
				LEFT JOIN ORG.Department D ON E.DepartmentId=D.Id
				LEFT JOIN HKP.Designation DE ON E.GivenDesignationId=DE.Id
                ORDER BY EffectiveDate DESC";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public GridModel QueryPFEmpVoluntaryValue(GridParameter parameters, string plantId, string effectiveDate)
        {
            try
            {
                var effectSql = string.Empty;
                if (effectiveDate != "") effectSql = "WHERE Month(EffectiveDate) = Month('" + effectiveDate + "') AND Year(EffectiveDate) = Year('" + effectiveDate + "')";

                parameters.CmdText = @"SELECT CASE ISNULL(PFV.Id,'') when '' then CAST('False' as bit)
                else CAST('TRUE' as bit) end Flag,PFV.Id,EI.SystemId EmployeeId,CONVERT (int, EI.EmployeeCode) EmployeeCode,EI.EmployeeName
				,D.UserName EmpDesignation,DP.UserName EMPDepartment,S.UserName EMPSection,SS.UserName EMPSubSection
				,PD.EmpVolunValPer
                ,PFV.AddedBy
                ,PFV.AddedDate
                ,PFV.AddedFromIP
                ,PFV.UpdatedBy
                ,PFV.UpdatedDate
                ,PFV.UpdatedFromIP
				,PFV.[EffectiveDate]
				,PFV.[VoluntaryPFValue]
				,PE.PFMstID,PE.IsMandatory,PE.IsActive
				,PE.IsApproved,PD.IsVoluntaryPF
				,PE.AlwnSlrHd
                ,PE.EmpSystemID
                ,PE.ID PFEligibleEmpId
				FROM PFEligibleEmployee PE
                LEFT JOIN EmployeeInformation EI ON PE.EmpSystemID=EI.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									LEFT JOIN HKP.Designation D ON pr.DesignationID = D.Id
									LEFT JOIN ORG.Department DP ON pr.DepartmentId = DP.Id
									LEFT JOIN ORG.Section S ON pr.SectionId=S.Id
									LEFT JOIN ORG.SubSection SS ON pr.SubSectionId = SS.Id
									LEFT JOIN PFPolicyMaster PM ON PE.PFMstID = PM.ID
									LEFT JOIN PFPolicyDetails PD ON PM.ID=PD.PFPolicyMasterID
									LEFT JOIN (
											   SELECT * FROM [dbo].[PFEmployeeVoluntaryValue]
												 " + effectSql + @"
											  ) PFV ON PE.ID= PFV.[PFEligibleEmpId]
				WHERE EI.PlantId='" + plantId + "' AND PE.IsActive =1 AND PD.IsVoluntaryPF=1  and PFV.Id is null";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }
        public GridModel QueryPFEmpVoluntaryValueChecked(GridParameter parameters, string plantId, string effectiveDate)
        {
            try
            {
                var effectSql = string.Empty;
                if (effectiveDate != "") effectSql = "WHERE Month(EffectiveDate) = Month('" + effectiveDate + "') AND Year(EffectiveDate) = Year('" + effectiveDate + "')";

                parameters.CmdText = @"SELECT CASE ISNULL(PFV.Id,'') when '' then CAST('False' as bit)
                else CAST('TRUE' as bit) end Flag,PFV.Id,EI.SystemId EmployeeId,CONVERT (int, EI.EmployeeCode) EmployeeCode,EI.EmployeeName
				,D.UserName EmpDesignation,DP.UserName EMPDepartment,S.UserName EMPSection,SS.UserName EMPSubSection
				,PD.EmpVolunValPer
                ,PFV.AddedBy
                ,PFV.AddedDate
                ,PFV.AddedFromIP
                ,PFV.UpdatedBy
                ,PFV.UpdatedDate
                ,PFV.UpdatedFromIP
				,PFV.[EffectiveDate]
				,PFV.[VoluntaryPFValue]
				,PE.PFMstID,PE.IsMandatory,PE.IsActive
				,PE.IsApproved,PD.IsVoluntaryPF
				,PE.AlwnSlrHd
                ,PE.EmpSystemID
                ,PE.ID PFEligibleEmpId
				FROM PFEligibleEmployee PE
                LEFT JOIN EmployeeInformation EI ON PE.EmpSystemID=EI.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									LEFT JOIN HKP.Designation D ON pr.DesignationID = D.Id
									LEFT JOIN ORG.Department DP ON pr.DepartmentId = DP.Id
									LEFT JOIN ORG.Section S ON pr.SectionId=S.Id
									LEFT JOIN ORG.SubSection SS ON pr.SubSectionId = SS.Id
									LEFT JOIN PFPolicyMaster PM ON PE.PFMstID = PM.ID
									LEFT JOIN PFPolicyDetails PD ON PM.ID=PD.PFPolicyMasterID
									LEFT JOIN (
											   SELECT * FROM [dbo].[PFEmployeeVoluntaryValue]
												 " + effectSql + @"
											  ) PFV ON PE.ID= PFV.[PFEligibleEmpId]
				WHERE EI.PlantId='" + plantId + "' AND PE.IsActive =1 AND PD.IsVoluntaryPF=1  and PFV.Id is not null";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }
    }
}
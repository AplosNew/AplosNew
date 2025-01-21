#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Payrolls;
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
    public class BonusPolicyMonthlyRetainEligibleEmployeeService : Service<BonusPolicyMonthlyRetainEligibleEmployee>, IBonusPolicyMonthlyRetainEligibleEmployeeService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<BonusPolicyMonthlyRetainEmpWiseCalculation> _bonusPolicyMonthlyRetainEmpWiseCalculation;
        private readonly IRepositoryAsync<BonusPolicyMonthlyRetainDistributionPmt> _bonusPolicyMonthlyRetainDistributionPmt;
        private readonly IRepositoryAsync<BonusPolicyMonthlyRetainStrcEmpWiseCalculation> _bonusPolicyMonthlyRetainStrcEmpWiseCalculation;
        private readonly IRepositoryAsync<BonusPolicyMonthlyRetainDistributionStrcPmt> _bonusPolicyMonthlyRetainDistributionStrcPmt;

        public BonusPolicyMonthlyRetainEligibleEmployeeService(
              IRepositoryAsync<BonusPolicyMonthlyRetainEmpWiseCalculation> bonusPolicyMonthlyRetainEmpWiseCalculation,
        IRepositoryAsync<BonusPolicyMonthlyRetainDistributionPmt> bonusPolicyMonthlyRetainDistributionPmt,
              IRepositoryAsync<BonusPolicyMonthlyRetainStrcEmpWiseCalculation> bonusPolicyMonthlyRetainStrcEmpWiseCalculation,
        IRepositoryAsync<BonusPolicyMonthlyRetainDistributionStrcPmt> bonusPolicyMonthlyRetainDistributionStrcPmt,
            IRepositoryAsync<BonusPolicyMonthlyRetainEligibleEmployee> BonusPolicyMonthlyRetainEmpWiseCalculationRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(BonusPolicyMonthlyRetainEmpWiseCalculationRepository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _bonusPolicyMonthlyRetainEmpWiseCalculation = bonusPolicyMonthlyRetainEmpWiseCalculation;
            _bonusPolicyMonthlyRetainDistributionPmt = bonusPolicyMonthlyRetainDistributionPmt;
            _bonusPolicyMonthlyRetainStrcEmpWiseCalculation = bonusPolicyMonthlyRetainStrcEmpWiseCalculation;
            _bonusPolicyMonthlyRetainDistributionStrcPmt = bonusPolicyMonthlyRetainDistributionStrcPmt;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region InsertUpdate

        public void InsertOrUpdate(IEnumerable<BonusPolicyMonthlyRetainEligibleEmployee> entities)
        {
            bool flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = _pkGeneratorService.GetMaxNumber(nameof(BonusPolicyMonthlyRetainEligibleEmployee), PKGeneratorEnum.Auto, null, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.ID))
                    {
                        pk.MaxNumber++;
                        item.ID = pk.MaxNumber.ToString();
                        item.ModelState = ModelState.Added;
                        AuditService.Log(item);
                        base.InsertGraph(item);
                    }
                    else if (!string.IsNullOrEmpty(item.ID) && item.IsActive == false)
                    {
                        int ui_monthNo = Convert.ToDateTime(item.EndDate).Month;
                        int ui_yearNo = Convert.ToDateTime(item.EndDate).Year;
                        if (!item.IsActive)
                        {
                            var b = _bonusPolicyMonthlyRetainEmpWiseCalculation.Query(r => r.EmpSystemID == item.EmpSystemID  && r.MonthNo >= ui_monthNo && r.YearNo >= ui_yearNo).Select().ToList();
                            if (b.Count > 0)
                            {
                                foreach (var a in b)
                                {
                                    var bchild = _bonusPolicyMonthlyRetainDistributionPmt.Query(r => r.BnsPlyMntRetainID == a.ID).Select().ToList();
                                    if (bchild != null)
                                    {
                                        foreach (var ac in bchild)
                                        {
                                            _bonusPolicyMonthlyRetainDistributionPmt.Delete(ac);
                                        }
                                    }
                                    _bonusPolicyMonthlyRetainEmpWiseCalculation.Delete(a);
                                }
                            }
                            var c = _bonusPolicyMonthlyRetainStrcEmpWiseCalculation.Query(r => r.EmpSystemID == item.EmpSystemID  && r.MonthNo >= ui_monthNo && r.YearNo >= ui_yearNo).Select().ToList();
                            if (c.Count > 0)
                            {
                                foreach (var a in c)
                                {
                                    var cchild = _bonusPolicyMonthlyRetainDistributionStrcPmt.Query(r => r.BnsPlyMntRetainID == a.ID).Select().ToList();
                                    if (cchild != null)
                                    {
                                        foreach (var ac in cchild)
                                        {
                                            _bonusPolicyMonthlyRetainDistributionStrcPmt.Delete(ac);
                                        }
                                    }
                                    _bonusPolicyMonthlyRetainStrcEmpWiseCalculation.Delete(a);
                                }
                            }
                        }
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                        UpdateGraph(item);
                    }
                    else
                    {
                        UpdateGraph(item);
                    }

                    //InsertOrUpdateGraph(item);
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
            return _pkGeneratorService.GetAutoNumber(nameof(BonusPolicyMonthlyRetainEligibleEmployee), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(BonusPolicyMonthlyRetainEligibleEmployee entity)
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
                BonusPolicyMonthlyRetainEligibleEmployee BonusPolicyMonthlyRetainEmpWiseCalculation = Find(key);
                base.DeleteGraph(BonusPolicyMonthlyRetainEmpWiseCalculation);
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
                parameters.CmdText = @"SELECT A.*,E.EmployeeName,D.UserName Department,DE.UserName Designation FROM dbo.BonusPolicyMonthlyRetainEmpWiseCalculation A
                LEFT JOIN DBO.EmployeeInformation E ON A.EmpSystemId=E.SystemId
				LEFT JOIN ORG.Department D ON E.DepartmentId=D.Id
				LEFT JOIN HKP.Designation DE ON E.GivenDesignationId=DE.Id";
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

        public GridModel QueryForMandatoryBonusEmployee(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.EmployeeId,EI.EmployeeCode,EI.EmployeeName,D.UserName EmpDesignation,DP.UserName EMPDepartment,S.UserName EMPSection,SS.UserName EMPSubSection, PE.* FROM [BonusPolicyMonthlyRetainEligibleEmployee] PE
                                    LEFT JOIN EmployeeInformation EI ON PE.EmpSystemID=EI.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									LEFT JOIN HKP.Designation D ON EI.DesignationSystemID = D.Id
									LEFT JOIN ORG.Department DP ON pr.DepartmentId = DP.Id
									LEFT JOIN ORG.Section S ON pr.SectionId=S.Id
									LEFT JOIN ORG.SubSection SS ON pr.SubSectionId = SS.Id
                                    WHERE EI.PlantId='" + plantId + "' AND PE.IsMandatory =1  AND PE.IsApproved =1";
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

        public GridModel QueryForOptionalBonusEmployee(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.EmployeeId, EI.EmployeeCode, EI.EmployeeName, D.UserName EmpDesignation, DP.UserName EMPDepartment, S.UserName EMPSection,
                                    SS.UserName EMPSubSection,BPM.IsIndividual,PE.*
                                    FROM [BonusPolicyMonthlyRetainEligibleEmployee] PE
                                    INNER JOIN EmployeeInformation EI ON PE.EmpSystemID=EI.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                                    INNER JOIN (
                                    SELECT DC.LeavePolicyMasterId,DC.BnsPlcMthRetainID, DC.PFPolicyMasterID, D.DesignationId
                                    FROM MST.DesignationMaster D
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON D.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId = '" + plantId + @"'
                                    ) DM ON EI.GivenDesignationId = DM.DesignationId
                                    LEFT JOIN HKP.Designation D ON pr.DesignationID = D.Id
                                    LEFT JOIN ORG.Department DP ON pr.DepartmentId = DP.Id
                                    LEFT JOIN ORG.Section S ON pr.SectionId=S.Id
                                    LEFT JOIN ORG.SubSection SS ON pr.SubSectionId = SS.Id
                                    left join [dbo].[BonusPolicyMonthlyRetainMaster] BPM ON DM.BnsPlcMthRetainID=BPM.ID
                                    WHERE EI.PlantId = '" + plantId + "' AND PE.IsApproved =1 AND PE.IsMandatory =0";
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
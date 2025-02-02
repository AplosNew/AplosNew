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
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class PFEmployeeAppliedService : Service<PFEligibleEmployee>, IPFEmployeeAppliedService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public PFEmployeeAppliedService(
            IRepositoryAsync<PFEligibleEmployee> PFEmployeeAppliedRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(PFEmployeeAppliedRepository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region InsertUpdate

        /// <summary>
        /// CompanyFYPeriod Insert.
        /// </summary>
        /// <param name="entity"></param>
        public void InsertOrUpdate(IEnumerable<PFEligibleEmployee> entities)
        {
            bool flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = _pkGeneratorService.GetMaxNumber(nameof(PFEligibleEmployee), PKGeneratorEnum.Auto, null, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.ID))
                    {
                        pk.MaxNumber++;
                        item.ID = pk.MaxNumber.ToString();
                        item.ModelState = ModelState.Added;
                        AuditService.Log(item);
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                    }
                    InsertOrUpdateGraph(item);
                }
                //IEnumerable<PFEligibleEmployee> dbList = base.Query().Select();
                //if (dbList != null && dbList.Count() > 0)
                //{
                //    if (entities == null)
                //    {
                //        foreach (var item in dbList)
                //        {
                //            base.Delete(item);
                //        }
                //    }
                //    else
                //    {
                //        foreach (var item in dbList)
                //        {
                //            if (!entities.Any(t => t.ID == item.ID))
                //            {
                //                base.Delete(item);
                //            }
                //        }
                //    }
                //}
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
            return _pkGeneratorService.GetAutoNumber(nameof(PFEmployeeApplied), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(PFEligibleEmployee entity)
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
                PFEligibleEmployee PFEmployeeApplied = Find(key);
                base.DeleteGraph(PFEmployeeApplied);
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
                parameters.CmdText = @"SELECT A.*,E.EmployeeName,D.UserName Department,DE.UserName Designation FROM dbo.PFEmployeeApplied A
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

        public GridModel QueryForPFMandatoryEmployee(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.EmployeeId,EI.EmployeeCode,EI.EmployeeName,D.UserName EmpDesignation,DP.UserName EMPDepartment,S.UserName EMPSection,SS.UserName EMPSubSection,PE.* FROM PFEligibleEmployee PE
                                    LEFT JOIN EmployeeInformation EI ON PE.EmpSystemID=EI.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
									LEFT JOIN HKP.Designation D ON pr.DesignationID = D.Id
									LEFT JOIN ORG.Department DP ON pr.DepartmentId = DP.Id
									LEFT JOIN ORG.Section S ON pr.SectionId=S.Id
									LEFT JOIN ORG.SubSection SS ON pr.SubSectionId = SS.Id
                                    WHERE EI.PlantId='" + plantId + "' AND PE.IsMandatory =1 AND PE.IsActive=1 AND PE.IsApproved =1";
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

        public GridModel QueryForPFOptionalEmployee(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT A.ID,A.EmployeeId,A.EmployeeCode,A.EmployeeName,A.Checked
				,IsPFNotEntitleGetAllownce = CASE WHEN A.IsActive = 0 AND A.IsApproved = 0 THEN IsPFNotEntitleGetAllownceCS
												  ELSE A.IsNotEntGetEmplrAlwn
												  END
                ,IsNotEntGetEmplrAlwnDetail
				--,IsIndividualAlwnDetail = CASE WHEN A.IsActive = 0 AND A.IsApproved = 0 THEN IsIndividualAlwnDetail
				--								  ELSE IsIndividualAlwn
				--								  END
                ,IsIndividualAlwnDetail
				--,AlwnSlrHdDetail = CASE WHEN A.IsActive = 0 AND A.IsApproved = 0 THEN AlwnSlrHdDetail
				--								  ELSE AlwnSlrHd
				--								  END
                ,AlwnSlrHdDetail
				,EmpDesignation, EMPDepartment,EMPSection,EMPSubSection
				,A.EmpSystemID,A.PFMstID,A.StartDate,A.MaturityDate,A.PFDtlID,A.IsMandatory,A.IsActive,A.AddedBy,A.AddedDate
				,A.AddedFromIP,A.UpdatedBy,A.UpdatedDate,A.UpdatedFromIP,A.IsApproved,A.IsVoluntaryPF,A.IsNotEntGetEmplrAlwn
				,A.AlwnSlrHd
				 FROM
        (SELECT EI.SystemId EmployeeId,EI.EmployeeCode,EI.EmployeeName,EI.PlantId
                , Checked = CASE WHEN PE.IsActive = 0 AND PE.IsApproved = 0 THEN CONVERT(bit, 'True')
				                 WHEN PE.IsActive = 1 AND PE.IsApproved = 1 THEN CONVERT(bit, 'True')
				                 ELSE CONVERT(bit, 'False')
				                 END
				,IsPFNotEntitleGetAllownceCS = CASE WHEN PD.IsNotEntGetEmplrAlwn=0 AND PD.IsIndividualAlwn =0 THEN CONVERT(BIT,'False')
									   WHEN PD.IsNotEntGetEmplrAlwn=1 AND PD.IsIndividualAlwn =0 THEN CONVERT(BIT,'True')
									   WHEN PD.IsNotEntGetEmplrAlwn=1 AND PD.IsIndividualAlwn =1 THEN CONVERT(BIT,'True')
									   END
                ,PD.IsNotEntGetEmplrAlwn IsNotEntGetEmplrAlwnDetail,PD.IsIndividualAlwn IsIndividualAlwnDetail,PD.AlwnSlrHd AlwnSlrHdDetail
				,D.UserName EmpDesignation,DP.UserName EMPDepartment,S.UserName EMPSection,SS.UserName EMPSubSection
				,PE.*
				 FROM PFEligibleEmployee PE
                LEFT JOIN EmployeeInformation EI ON PE.EmpSystemID=EI.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
													LEFT JOIN HKP.Designation D ON pr.DesignationID = D.Id
									LEFT JOIN ORG.Department DP ON pr.DepartmentId = DP.Id
									LEFT JOIN ORG.Section S ON pr.SectionId=S.Id
									LEFT JOIN ORG.SubSection SS ON pr.SubSectionId = SS.Id
                LEFT JOIN PFPolicyMaster PM ON PE.PFMstID = PM.ID
                LEFT JOIN PFPolicyDetails PD ON PM.ID=PD.PFPolicyMasterID) A
				WHERE A.PlantId='" + plantId + "' AND	A.IsMandatory=0";
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
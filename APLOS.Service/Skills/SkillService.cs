#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Skills
{
    public class SkillService : Service<Skill>, ISkillService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ISkillProcessService _skillProcessService;
        private readonly IRepositoryAsync<Skill> _skillRepository;

        public SkillService(
            IRepositoryAsync<Skill> SkillRepository
            ,IPKGeneratorService pkGeneratorService
            ,ISkillProcessService skillProcessService
            ,IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(SkillRepository, unitOfWork, pkGeneratorService)
        {
            _skillRepository = SkillRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _skillProcessService = skillProcessService;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT SK.[Sequence]
		                                        ,SK.Code
		                                        ,SK.ShortName
		                                        ,SK.StandardName
		                                        ,SK.UserName
                                                ,SK.CompanyGroupId
		                                        ,SK.SkillCategoryId
		                                        ,SC.UserName AS SkillCategoryName
		                                        , IsMachineApplicable = CASE WHEN SK.IsMachineApplicable='1' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
		                                        ,SK.[Description]
		                                        ,SK.Remarks
		                                        ,SK.Active
		                                        ,SK.Id,SK.SkillGroupId,SK.OperationApplicable,SK.DashboardApplicable,SG.UserName SkillGroup,SK.OperationActivityId,OA.UserName OperationActivity
                                       FROM [" + DbSchema.HKP + @"].[Skill] AS SK
                                       LEFT OUTER JOIN [" + DbSchema.HKP + @"].[SkillCategory] AS SC ON SK.SkillCategoryId=SC.Id
                                       LEFT JOIN [SCS].[SkillGrouping] SG ON SG.Id=SK.SkillGroupId
                                       LEFT JOIN HKP.OperationActivity OA ON OA.Id=SK.OperationActivityId
                                       WHERE SK.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public GridModel GetIsMachineSkillList(GridParameter parameters, string companyGroupId, string[] skillProcessIds)
        {
            try
            {
                parameters.CmdText = @"SELECT CAST(0 AS BIT) Flag, SKC.UserName AS SkillCategoryName, SKP.SkillId,SK.UserName AS SkillName, SKP.ProcessId, P.UserName AS ProcessName, SKP.Id AS SkillProcessId
                     FROM HKP.Skill AS SK INNER JOIN HKP.SkillCategory AS SKC ON SK.SkillCategoryId=SKC.Id
                     INNER JOIN HKP.SkillProcess AS SKP ON SKP.SkillId=SK.Id INNER JOIN HKP.Process AS P ON SKP.ProcessId=P.Id
                     WHERE SK.CompanyGroupId='" + companyGroupId + "' AND SK.IsMachineApplicable=1 AND SKP.Id NOT IN (" + ReturnStringArray(skillProcessIds) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public GridModel GetCommonSkillListByProcess(GridParameter parameters, string companyGroupId, string[] processIds)
        {
            parameters.CmdText = @";WITH CTE AS
                    (
                       SELECT SP.SkillId, SK.UserName SkillCategoryName, SK.[Sequence], SK.Code, SK.ShortName, SK.StandardName, SK.UserName--,SP.ProcessId
		                    , COUNT(*) OVER (PARTITION BY SP.SkillId) AS RN
	                    FROM [HKP].[SkillProcess] AS SP
	                    JOIN [HKP].[Skill] AS SK ON SP.SkillId=SK.Id
	                    LEFT JOIN [HKP].[SkillCategory] AS SKC ON SK.SkillCategoryId=SKC.Id
	                    WHERE SP.ProcessId IN (" + ReturnStringArray(processIds) + ") AND SK.CompanyGroupId='"+ companyGroupId + @"' AND SK.IsMachineApplicable=0 AND SK.Active=1
                    ) SELECT DISTINCT *, COUNT(*) OVER () AS TotalRows FROM CTE";
            return _sqlRepository.GetDifferentGridData(parameters);
        }
        
        #region Get Cbo

        /// <summary>
        /// Goes for operation.which skill not declare for machine type.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public IEnumerable<object> GetCboWithoutMachineType(string processId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT SKP.SkillId, SK.UserName
                                FROM HKP.SkillProcess AS SKP
                                LEFT OUTER JOIN HKP.Skill AS SK ON SKP.SkillId=SK.Id
                                WHERE SK.IsMachineApplicable=0 AND SKP.ProcessId='" + processId + "' AND SK.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetCombo(_sql, "SkillId", "UserName");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        /// <summary>
        /// Goes for machine.which skill declare for machine type.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="companyGroupId"></param>
        /// <returns></returns>
        public IEnumerable<object> GetCboByProcess(string companyGroupId, string[] processIds)
        {
            try
            {
                var _sql = @";WITH CTE AS
                            (
	                            SELECT SKP.SkillId AS [Value], SK.UserName AS [Text], COUNT(*) OVER (PARTITION BY SKP.SkillId) AS RN
	                            FROM HKP.Skill AS SK
	                            LEFT JOIN HKP.SkillProcess AS SKP ON SKP.SkillId=SK.Id
	                            WHERE SK.CompanyGroupId='" + companyGroupId + @"' AND SK.IsMachineApplicable=1 AND SKP.ProcessId IN(" + ReturnStringArray(processIds) + @")
                            )
                            SELECT DISTINCT * FROM CTE ORDER BY CTE.[Text]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        /// <summary>
        /// Get cbo by machine type id.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="matchineTypeId">todo: describe matchineTypeId parameter on GetCboByMachineTypeId</param>
        public IEnumerable<ComboModel> GetCboByMachineTypeId(string processId, string matchineTypeId)
        {
            try
            {
                var _sql = @"SELECT DISTINCT MTP.SkillId,SK.UserName
                                    FROM [MSt].[AssetItemProcess] AS MTP
                                    LEFT OUTER JOIN HKP.[SkillProcess] AS SKP ON MTP.SkillId=SKP.SkillId
                                    LEFT OUTER JOIN HKP.[Skill] AS SK ON SKP.SkillId=SK.Id
                                    WHERE MTP.ProcessId='" + processId + "' AND MTP.AssetItemId='" + matchineTypeId + "'";
                return _sqlRepository.GetCombo(_sql, "SkillId", "UserName");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        #endregion Get Cbo

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber("Skill", PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(Skill entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.Active && r.CompanyGroupId == identity.CompanyGroupId && r.SkillCategoryId == entity.SkillCategoryId);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.Active && r.CompanyGroupId == identity.CompanyGroupId && r.SkillCategoryId == entity.SkillCategoryId);
        }

        public void InsertGraph(Skill entity, IEnumerable<SkillProcess> skillProcess)
        {
            var flag = false;
            try
            {
                Check(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                _skillProcessService.InsertUpdateOrDeleteGraph(entity.Id, skillProcess);
                base.InsertGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateGraph(Skill entity, IEnumerable<SkillProcess> skillProcess)
        {
            var flag = false;
            try
            {
                IsMachineAplicableChange(entity.Id, entity.IsMachineApplicable);
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                _skillProcessService.InsertUpdateOrDeleteGraph(entity.Id, skillProcess);
                base.UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
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
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "Skill Id"));
                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                _skillProcessService.DeleteGraph(entity.Id);
                DeleteGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void IsMachineAplicableChange(string id, bool newIsMachineApplicable)
        {
            if (CheckSkillIdUse(id))
            {
                var oldIsMachineApplicable = base.Query(t => t.Id == id).Select(t => t.IsMachineApplicable).FirstOrDefault();
                if (oldIsMachineApplicable != newIsMachineApplicable)
                {
                    if (oldIsMachineApplicable)
                        throw new CustomException(string.Format(ServiceResources.AlreadyExistAnotherTable, "unchecked", "Machine"));
                    else
                        throw new CustomException(string.Format(ServiceResources.AlreadyExistAnotherTable, "checked", "Operation"));
                }
            }
        }

        /// <summary>
        /// if skill id use in machine or operation then IsMachineApplicable field can not be change in skill.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CheckSkillIdUse(string id)
        {
            try
            {
                var sql = @"IF EXISTS(SELECT 1 FROM (
	                        SELECT SkillId AS CheckingColumn FROM MST.Operation UNION ALL
	                        SELECT SkillId AS CheckingColumn FROM MST.MaterialMaster
                        ) A
                        WHERE CheckingColumn ='" + id + "') SELECT 1 ELSE SELECT 0 RETURN";
                return Convert.ToBoolean(_skillRepository.SqlQuery<int>(sql).Single());
            }
            catch
            {
                throw;
            }
        }
    }
}
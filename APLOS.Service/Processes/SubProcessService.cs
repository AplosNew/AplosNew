#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Processes;
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

namespace Library.Service.Processes
{
    public partial class SubProcessService : Service<SubProcess>, ISubProcessService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public SubProcessService(
            IRepositoryAsync<SubProcess> subProcessRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(subProcessRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(SubProcess entity)
        {
            try
            {
                CheckUnique(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(SubProcess), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void Update(SubProcess entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        private void CheckUnique(SubProcess entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && r.ProcessId == entity.ProcessId && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && r.ProcessId == entity.ProcessId && !r.Archive);
        }

        public decimal GetAutoSequence(string processId)
        {
            try
            {
                return Query(t => t.ProcessId == processId).Select().Max(t => t.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = $"SELECT p.Id AS Value, p.UserName AS Text FROM {DbSchema.HKP}.[{DbTable.SubProcess}] AS p  WHERE CompanyGroupId='{identity.CompanyGroupId}' AND p.Active=1 AND p.Archive=0 ORDER BY UserName ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public GridModel GetCbo(string processid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = $"SELECT p.Id AS [Value], p.UserName AS [Text] FROM {DbSchema.HKP}.[{DbTable.SubProcess}] AS p " +
                    $"  left outer join (select * from mst.CompanySubProcess where CompanyId = '{identity.CompanyId}' and CompanyGroupId='{identity.CompanyGroupId}' and Archive=0) cp " +
                    $" on cp.SubProcessId=p.Id " +
                    $" where p.ProcessId='{processid}' " +
                    $"  AND p.Active=1 AND p.Archive=0 ";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel Query(GridParameter parameters, string processId, string companyGroupId)
        {
            try
            {
                parameters.order = "asc";
                parameters.sort = "Sequence";
                parameters.CmdText = @"SELECT SP.*,SPC.UserName AS SubProcessCategoryName
                                        FROM HKP.[SubProcess] AS SP
                                        LEFT OUTER JOIN HKP.[SubProcessCategory] AS SPC ON SP.SubProcessCategoryId=SPC.Id
                                        WHERE CompanyGroupId='" + companyGroupId + "' AND ProcessId='" + processId + "' AND Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        /// <summary>
        /// Get sub-process for company sub process.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="companyId"></param>
        /// <param name="processId"></param>
        /// <returns></returns>

        public GridModel GetListForCompanySubProcess(GridParameter parameters, string companyId, string processId, string[] subProcessIds)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var subProcess = "";
                if (subProcessIds.Length > 0)
                    subProcess = string.Join(",", subProcessIds.Select(item => "'" + item + "'"));
                else
                    subProcess = "' '";
                parameters.order = "asc";
                parameters.sort = "Sequence";
                parameters.CmdText = "SELECT SP.Id, " +
                                               "SP.Code, " +
                                               "SP.UserName, " +
                                               "SP.Sequence," +
                                               "SP.Active, " +
                                               "SP.Archive, " +
                                               "SPC.UserName AS SubProcessCategoryName," +
                                               "'' AS Flag " +
                                        $"FROM {DbSchema.HKP}.[{DbTable.SubProcess}] AS SP  " +
                                        $"LEFT OUTER JOIN HKP.[SubProcessCategory] AS SPC ON SP.SubProcessCategoryId=SPC.Id " +
                                        $"WHERE SP.CompanyGroupId='{identity.CompanyGroupId}' AND SP.Archive=0 AND SP.ProcessId='{processId}' " +
                                        $"AND SP.Id NOT IN ({subProcess}) AND SP.Id NOT IN (SELECT SubProcessId FROM MST.CompanySubProcess)";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public GridModel GetListSubProcess(GridParameter parameters, string companyId, string processId, string[] subProcessIds)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.order = "asc";
                parameters.sort = "Sequence";
                parameters.CmdText = @"SELECT SP.Id
                                              , SP.Code
                                              , SP.UserName
                                              , SP.Sequence
                                              , SP.Active
                                              , SP.Archive
                                              , SPC.UserName AS SubProcessCategoryName
                                              , '' AS Flag
                                        FROM [" + DbSchema.HKP + @"].[" + DbTable.SubProcess + @"] AS SP
                                        LEFT OUTER JOIN [" + DbSchema.HKP + @"].[SubProcessCategory] AS SPC ON SP.SubProcessCategoryId=SPC.Id
                                        WHERE SP.CompanyGroupId='" + identity.CompanyGroupId + @"' AND SP.Archive=0 AND SP.ProcessId='" + processId + @"'
                                        AND SP.Id NOT IN (" + ReturnStringArray(subProcessIds) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

		public GridModel GetSubProcessListByProductionProcess(GridParameter parameters, string companyGroupId, string processId)
		{
			try
			{
				parameters.CmdText = @"SELECT SP.Id AS SubProcessId
								, SP.CompanyGroupId
								, SP.ProcessId
								, SP.Archive
								, SP.Code
								, SP.UserName AS SubProcessName
								, SPC.UserName AS SubProcessCategoryName
						FROM [HKP].[SubProcess] AS SP
						LEFT JOIN [HKP].[SubProcessCategory] AS SPC ON SP.SubProcessCategoryId=SPC.Id
						JOIN [HKP].[Process] AS P ON SP.ProcessId=P.Id
						WHERE SP.CompanyGroupId='"+ companyGroupId + "' AND SP.ProcessId='"+ processId + "' AND SP.Archive=0 AND P.IsProductionProcess=1";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
			}
		}
	}
}
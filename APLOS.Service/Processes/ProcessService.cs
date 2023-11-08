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
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Processes
{
	public partial class ProcessService : Service<Process>, IProcessService
	{
		#region Constructor

		private readonly IUnitOfWork _unitOfWork;
		private readonly ISqlRepository _sqlRepository;
		private readonly IPKGeneratorService _pkGeneratorService;

		public ProcessService(
			IRepositoryAsync<Process> processRepository
			, IPKGeneratorService pkGeneratorService
			, ISqlRepository sqlRepository
			, IUnitOfWork unitOfWork) : base(processRepository, unitOfWork)
		{
			_unitOfWork = unitOfWork;
			_pkGeneratorService = pkGeneratorService;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		private string GetPK()
		{
			return _pkGeneratorService.GetAutoNumber(nameof(Process), PKGeneratorEnum.Yearly, null, DateTime.Now);
		}

		public override void Insert(Process entity)
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

		private void CheckUnique(Process entity)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
			CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive);
		}

		public override void Update(Process entity)
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

		public void DeleteGraph(string id)
		{
			try
			{
				if (string.IsNullOrEmpty(id))
					throw new CustomException(string.Format(ResourcesCore.IsNull, "Process Id"));
				var entity = Find(id);
				Delete(entity);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
			}
		}

		#region GetSequence

		///-------------------------------------------------------------------------------------------------
		/// <summary>   Gets automatic sequence. </summary>
		/// <returns>   The automatic sequence. </returns>
		///-------------------------------------------------------------------------------------------------

		public decimal GetAutoSequence()
		{
			try
			{
				return Query().Select().Max(r => r.Sequence + 1);
			}
			catch
			{
				return 1.00M;
			}
		}

		#endregion GetSequence

		#region Get List

		public GridModel Query(GridParameter parameters, string companyGroupId, string[] processIds)
		{
			try
			{
				parameters.CmdText = @"SELECT P.Id, P.CompanyGroupId, P.Code
	                                    , P.[Sequence], P.ShortName, P.StandardName, P.UserName
	                                    , P.IsProductionProcess, P.IsProcessRouting, P.IsLocked
	                                    , P.IsAppApplicable, P.IsChecked, P.IsValueAdded
	                                    , P.MaterialTypeId, MT.[Description] AS MaterialType
	                                    , P.ProcessGroupId, PG.UserName AS ProcessGroupName
	                                    , P.Remarks,P.POControlMilestoneSequence
	                                    , P.Active, P.Archive, P.IsFirst,P.IsLast,P.IsCrossAllowed,Convert(bit,0) AS Flag
                                    FROM [HKP].[Process] AS P
                                    LEFT JOIN [HKP].[MaterialType] AS MT ON P.MaterialTypeId=MT.Id
                                    LEFT JOIN [HKP].[ProcessGroup] AS PG ON P.ProcessGroupId=PG.Id
							        WHERE P.CompanyGroupId='" + companyGroupId + "' AND P.Id NOT IN(" + ReturnStringArray(processIds) + ") AND P.Archive=0";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
			}
		}

		public GridModel GetProductionProcessList(GridParameter parameters, string companyGroupId, string CompanyId, string productionOrderId, string EntityId)
		{
			try
			{
				parameters.CmdText = @"SELECT DISTINCT P.Id, P.CompanyGroupId, P.Code
								, P.[Sequence], P.ShortName, P.StandardName, P.UserName
								, P.IsProductionProcess, P.IsProcessRouting, P.IsLocked
								, P.IsAppApplicable, IsChecked, P.IsValueAdded
								, P.MaterialTypeId, MT.[Description] AS MaterialType
								, P.Remarks,TG.ProductionBookingLevel
								, P.Active, P.Archive, Convert(bit,0) AS Flag,IsInventory=CAST(CASE WHEN M.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT)
							FROM [HKP].[Process] AS P
							LEFT JOIN HKP.MaterialType AS MT ON P.MaterialTypeId=MT.Id
							LEFT JOIN [HKP].[EntityProcessTag] TG ON TG.ProcessId=P.Id AND TG.EntityId='"+ EntityId + @"' 
							LEFT JOIN [dbo].[EntityConfig] M ON M.ConsumptionProcessId=P.Id AND M.EntityId='" + EntityId + @"' 
							WHERE P.CompanyGroupId='" + companyGroupId + @"' AND P.IsProductionProcess=1 AND P.Archive=0
							AND P.Id NOT IN(Select ProcessId from TRN.ProductionOrderProcessSet Where  ProductionOrderId='"+ productionOrderId + "')";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
			}
		}

		#endregion Get List

		#region Cbo

		public IEnumerable<object> GetCbo(string companyGroupId)
		{
			try
			{
				var sql = $"SELECT p.Id AS Value, p.UserName AS Text FROM {DbSchema.HKP}.[{DbTable.Process}] AS p  WHERE CompanyGroupId='{companyGroupId}' AND p.Active=1 AND p.Archive=0 ORDER BY UserName ";
				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
			}
		}

		public IEnumerable<object> GetCboByIsValueAdded(string groupId)
		{
			try
			{
				var sql = $"SELECT p.Id AS Value, p.UserName AS Text FROM {DbSchema.HKP}.[{DbTable.Process}] AS p  WHERE CompanyGroupId='{groupId}' AND p.IsValueAdded=1 AND p.Active=1 AND p.Archive=0 ORDER BY UserName ";
				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
			}
		}

		public IEnumerable<ComboModel> GetProductionProcessCbo(string companyGroupId)
		{
			var _sql = @"SELECT P.Id, P.UserName FROM [HKP].[Process] AS P
						WHERE P.CompanyGroupId='" + companyGroupId + @"'
						AND P.IsProductionProcess = 1 AND P.Archive = 0 ORDER BY P.UserName";
			return _sqlRepository.GetCombo(_sql, "Id", "UserName");
		}

		#endregion Cbo

		public GridModel GetLoadProcessWithSubProcess(GridParameter parameters, string companyGroupId)
		{
			try
			{
				parameters.CmdText = @"SELECT P.Id,P.UserName ProcessName,SP.Id SubProcessId,SP.UserName SubProcessName FROM HKP.Process AS p
                                      Left JOIN HKP.SubProcess SP ON P.Id=SP.ProcessId
                                      WHERE p.CompanyGroupId='" + companyGroupId + "' AND p.IsValueAdded=1 AND p.Active=1 AND p.Archive=0";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
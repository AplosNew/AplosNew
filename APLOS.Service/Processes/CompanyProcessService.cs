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
	public class CompanyProcessService : Service<CompanyProcess>, ICompanyProcessService
	{
		#region Constructor

		private readonly IUnitOfWork _unitOfWork;
		private readonly IProcessService _processService;
		private readonly ISqlRepository _sqlRepository;
		private readonly IRepositoryAsync<CompanyProcess> _companyProcessRepository;

		public CompanyProcessService(
			IRepositoryAsync<CompanyProcess> companyProcessRepository,
			IProcessService processService,
			IPKGeneratorService pkGeneratorService,
			IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			) : base(companyProcessRepository, unitOfWork, pkGeneratorService)
		{
			_companyProcessRepository = companyProcessRepository;
			_processService = processService;
			_unitOfWork = unitOfWork;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		public void InsertUpdateOrDeleteGraph(IEnumerable<CompanyProcess> entities, string companyGroupId)
		{
			var flag = false;
			try
			{
				if (entities != null)
				{
					_unitOfWork.BeginTransaction();
					flag = true;
					var companyId = entities.FirstOrDefault().CompanyId;
					var dbList = base.Query(t => t.CompanyId == companyId).Select().ToList();
					var pk = GetMaxNumber(nameof(CompanyProcess), PKGeneratorEnum.Auto, null, DateTime.Now);
					foreach (var item in entities)
					{
						if (string.IsNullOrEmpty(item.Id))
						{
							pk.MaxNumber++;
							item.Id = pk.MaxNumber.ToString();
							item.CompanyGroupId = companyGroupId;
							InsertGraph(item);
						}
					}
					if (dbList.IsNotNull() && dbList.Count > 0)
					{
						if (entities == null)
						{
							foreach (var item in dbList)
							{
								base.DeleteGraph(item);
							}
						}
						else
						{
							foreach (var item in dbList)
							{
								if (!entities.Any(t => t.Id == item.Id))
									base.DeleteGraph(item);
							}
						}
					}
					_unitOfWork.SaveChanges();
					flag = false;
					_unitOfWork.Commit();
				}
			}
			catch (CustomException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		private string GetPK()
		{
			return GetAutoNumber(nameof(CompanyProcess), PKGeneratorEnum.Auto, null, DateTime.Now);
		}

		public GridModel Query(GridParameter parameters, string companyId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			parameters.CmdText = "SELECT CP.*, " +
										 "P.Code, " +
										 "P.UserName AS ProcessName, " +
										 "P.StandardName, " +
										 "P.ShortName, " +
										 "P.IsProductionProcess, " +
										 "P.IsProcessRouting, " +
										 "P.IsLocked " +
								 $"FROM {DbSchema.Masters}.[{DbTable.CompanyProcess}] AS CP " +
								 $"LEFT OUTER JOIN {DbSchema.HKP}.[{DbTable.Process}] AS P ON CP.ProcessId=P.Id " +
								 $"WHERE CP.CompanyGroupId='{identity.CompanyGroupId}' AND CP.CompanyId='{companyId}' AND CP.Archive=0 ";
			return _sqlRepository.GetGridData(parameters);
		}

		public GridModel GetCompanyProductionProcessList(GridParameter parameters, string companyGroupId, string companyId, string[] processIds)
		{
			try
			{
				parameters.CmdText = @"SELECT P.Id
                            , P.[Sequence], P.Code
                            , P.ShortName, P.StandardName, P.UserName
                            , MT.[Description] AS MaterialType
                            , P.Active, '' AS Flag
                    FROM HKP.Process AS P
                    JOIN MST.CompanyProcess CP ON CP.ProcessId=p.Id
                    LEFT JOIN HKP.MaterialType AS MT ON P.MaterialTypeId=MT.Id
                    WHERE P.CompanyGroupId='" + companyGroupId + "' AND CP.CompanyId='" + companyId + @"'AND P.IsProductionProcess=1 
					AND P.Id NOT IN(" + ReturnStringArray(processIds) + ") AND P.Archive=0";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception)
			{
				throw;
			}
		}

		public GridModel GetCompanyProcessList(GridParameter parameters, string companyId, string[] processIds)
		{
			try
			{
				parameters.CmdText = @"SELECT  P.Id, P.[Sequence], P.Code
	                                           , P.ShortName, P.StandardName, P.UserName
	                                           , MT.[Description] AS MaterialType
                                               , P.IsProductionProcess
	                                           , P.Active, '' AS Flag
                                        FROM MST.CompanyProcess AS CP
                                        LEFT OUTER JOIN HKP.Process AS P ON CP.ProcessId=P.Id
                                        LEFT OUTER JOIN HKP.MaterialType AS MT ON P.MaterialTypeId=MT.Id
                                        WHERE CP.CompanyId='" + companyId + "' AND P.Id NOT IN(" + ReturnStringArray(processIds) + ") AND CP.Archive=0 AND P.Archive=0 AND P.IsProductionProcess=1";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception)
			{
				throw;
			}
		}

		public IEnumerable<ComboModel> GetCompanyProductionProcessCbo(string companyId)
		{
			var _sql = @"SELECT P.Id, P.UserName FROM [MST].[CompanyProcess] CP JOIN [HKP].[Process] AS P ON CP.ProcessId=p.Id
						WHERE CP.CompanyId='" + companyId + "'AND P.IsProductionProcess=1 AND P.Archive=0 ORDER BY P.UserName";
			return _sqlRepository.GetCombo(_sql, "Id", "UserName");
		}

		/// <summary>
		/// Get cbo by company process for company sub process
		/// </summary>
		/// <returns></returns>
		public IEnumerable<object> GetCompanyProcessCbo(string companyId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = "SELECT CP.ProcessId AS Value, P.UserName AS Text " +
							$"FROM MST.[CompanyProcess] AS CP " +
							$"LEFT OUTER JOIN HKP.[Process] AS P ON CP.ProcessId=P.Id  " +
							$"WHERE CP.CompanyId='{companyId}' AND CP.Archive=0 ";
				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
			}
		}

		private bool CheckIdUse(string companyId, string[] processIds)
		{
			try
			{
				var process = "";
				if (processIds.Length > 0)
					process = string.Join(",", processIds.Select(item => "'" + item + "'"));
				else
					process = "' '";
				var sql = @"IF EXISTS(SELECT 1 FROM(
                                SELECT A.CheckingColumn1,B.CheckingColumn2 FROM
                                (SELECT Id,CompanyId AS CheckingColumn1 FROM HKP.ProcessSet) AS A LEFT OUTER JOIN
                                (SELECT ProcessSetId,ProcessId AS CheckingColumn2 FROM HKP.ProcessSetDetail ) AS B ON A.Id=B.ProcessSetId
                               ) AA WHERE CheckingColumn1 ='" + companyId + "' AND CheckingColumn2 IN (" + process + ")) SELECT 1 ELSE SELECT 0 RETURN";
				return Convert.ToBoolean(_companyProcessRepository.SqlQuery<int>(sql).Single());
			}
			catch
			{
				throw;
			}
		}
	}
}
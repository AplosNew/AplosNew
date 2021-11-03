#region Using

using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Library.ViewModel.Materials;

#endregion Using

namespace Library.Service.Materials
{
    public class MaterialAttributeMasterService : Service<MaterialAttributeMaster>, IMaterialAttributeMasterService
	{
		#region Constructor

		private readonly IUnitOfWork _unitOfWork;
		private readonly IPKGeneratorService _pkGeneratorService;
		private readonly ISqlRepository _sqlRepository;
		private readonly IRepositoryAsync<MaterialAttributeMaster> _charaterRepository;

		public MaterialAttributeMasterService(
			IRepositoryAsync<MaterialAttributeMaster> charaterRepository,
			IPKGeneratorService pkGeneratorService,
			IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			) : base(charaterRepository, unitOfWork, pkGeneratorService)
		{
			_charaterRepository = charaterRepository;
			_unitOfWork = unitOfWork;
			_pkGeneratorService = pkGeneratorService;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		public IEnumerable<object> Query(string materialGroupMasterId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var _sql = @"SELECT MAM.Id, MAM.MaterialGroupMasterId, MAM.MaterialAttributeId, MA.UserName AS MaterialAttributeName
                                    , MAM.[Sequence], MA.IsFixedNoOfCharacter, MA.NoOfCharacter, MA.ValueAssignmentLevel
                                    , MA.IsFreeField, MA.IsPreDefinedField, MA.IsMandatory, MAM.Active, MAM.Archive
                                FROM [" + DbSchema.Masters + @"].[" + DbTable.MaterialAttributeMaster + @"] as MAM
                                INNER JOIN [" + DbSchema.HKP + @"].[" + DbTable.MaterialAttribute + @"] as MA ON MA.Id = MAM.MaterialAttributeId
                                Where MAM.MaterialGroupMasterId='" + materialGroupMasterId + "' ORDER BY MAM.MaterialGroupMasterId, MAM.[Sequence]";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
								null, ErrorType.ServiceError, null,
								ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
		}

		public IEnumerable<object> QueryForMaterialMaster(string materialGroupMasterId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var _sql = @"SELECT  NULL AS Id, NULL MaterialMasterId, MAM.MaterialGroupMasterId, MAM.MaterialAttributeId, MA.UserName
                                    , MAM.[Sequence], MA.IsFixedNoOfCharacter, MA.NoOfCharacter, MA.ValueAssignmentLevel
                                    , MA.IsFreeField, MA.IsPreDefinedField, MA.IsMandatory, MAM.Active, MAM.Archive
                                FROM [" + DbSchema.Masters + @"].[" + DbTable.MaterialAttributeMaster + @"] as MAM
                                INNER JOIN [" + DbSchema.HKP + @"].[" + DbTable.MaterialAttribute + @"] as MA ON MA.Id = MAM.MaterialAttributeId
                                Where MAM.MaterialGroupMasterId='" + materialGroupMasterId + "' ORDER BY MAM.MaterialGroupMasterId, MAM.[Sequence]";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
								null, ErrorType.ServiceError, null,
								ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
		}

		public void Save(IEnumerable<MaterialAttributeMaster> entites)
		{
			var flag = false;
			try
			{
				_unitOfWork.BeginTransaction();
				flag = true;
				var pk = GetMaxNumber(nameof(MaterialAttributeMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
				if (entites != null)
				{
					var materialGroupMasterId = entites.First().MaterialGroupMasterId;
					var dbList = base.Query(r => r.MaterialGroupMasterId == materialGroupMasterId && !r.Archive).Select().ToList();
					var materialAttributeIds = entites.Select(t => t.MaterialAttributeId).ToArray();
					var materialMasterAttributeList = GetMaterialMasterAttribute(materialGroupMasterId, materialAttributeIds);
					//if (dbList.Count() <= 6 && entites.Count() <= 20)
					if (entites.Count() <= 20)
					{
						foreach (var item in entites)
						{
							//var data = dbList.FirstOrDefault(r => r.MaterialAttributeId == item.MaterialAttributeId);
							if (string.IsNullOrEmpty(item.Id))
							{
								var id = pk.MaxNumber++;
								item.Id = id.ToString();
								InsertGraph(item);
							}
							else
								UpdateGraph(item);
						}
					}
					else
						throw new CustomException("Total no of material attribute can not be more than 20.");
					if (dbList != null)
					{
						foreach (var item in dbList)
						{
							if (!entites.Any(t => t.Id == item.Id))
								DeleteGraph(item);
						}
					}
					_unitOfWork.SaveChanges();
					flag = false;
					_unitOfWork.Commit();
				}
				else
					throw new CustomException("Please select at least one attribute.");
			}
			catch (CustomException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		//private string GetPK()
		//{
		//    return base.GetMaxNumber("MaterialAttributeMaster", PKGeneratorEnum.Auto, null, DateTime.Now);
		//}

		public override void Archive(string materialGroupMasterId)
		{
			var flag = false;
			try
			{
				CheckIdUse(materialGroupMasterId);
				_unitOfWork.BeginTransaction();
				flag = true;
				var data = base.Query(r => r.MaterialGroupMasterId == materialGroupMasterId && !r.Archive).Select().AsEnumerable();
				if (data != null)
				{
					foreach (var item in data)
					{
						DeleteGraph(item);
					}
				}
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
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
			finally
			{
				if (flag)
					_unitOfWork.Rollback();
			}
		}

		private void CheckIdUse(string id)
		{
			string sql = $"IF EXISTS(SELECT 1 FROM( " +
							$"SELECT MaterialGroupMasterId AS CheckingColumn FROM [{DbSchema.Masters}].[{DbTable.MaterialMaster}] WHERE Archive=0 " +
							$") A WHERE CheckingColumn = '{id}') SELECT 1 ELSE SELECT 0 RETURN ";
			var data = Convert.ToBoolean(_charaterRepository.SqlQuery<int>(sql).Single());
			if (data)
				throw new CustomException("Already grid exist in material master, you can't delete....!");
		}

		private IEnumerable<MaterialViewModel> GetMaterialMasterAttribute(string materialGroupMasterId, string[] materialAttributeId)
		{
			try
			{
				var _sql = @"SELECT DISTINCT MMA.MaterialAttributeId FROM MST.MaterialMasterAttribute AS MMA
                            WHERE MMA.MaterialGroupMasterId='" + materialGroupMasterId + "' AND MMA.MaterialAttributeId IN(" + ReturnStringArray(materialAttributeId) + ")";
				return _charaterRepository.SqlQuery<MaterialViewModel>(_sql).ToList();
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
		}
	}
}
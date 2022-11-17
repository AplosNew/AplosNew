#region using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
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

#endregion using

namespace Library.Service.Materials
{
	public class MaterialMasterAttributeValueService : Service<MaterialMasterAttributeValue>, IMaterialMasterAttributeValueService
	{
		#region Constructor

		private readonly IUnitOfWork _unitOfWork;
		private readonly IPKGeneratorService _pkGeneratorService;
		private readonly ISqlRepository _sqlRepository;

		public MaterialMasterAttributeValueService(
			IRepositoryAsync<MaterialMasterAttributeValue> materialMasterAttributeValueRepository,
			IPKGeneratorService pkGeneratorService,
			IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			) : base(materialMasterAttributeValueRepository, unitOfWork, pkGeneratorService)
		{
			_unitOfWork = unitOfWork;
			_pkGeneratorService = pkGeneratorService;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		public decimal GetAutoSequence()
		{
			try
			{
				return base.Query().Select().Max(r => r.Sequence + 1);
			}
			catch (Exception)
			{
				return 1.00m;
			}
		}

		public IEnumerable<object> Query(string masterId)
		{
			try
			{
				var _sql = @"SELECT A.Id, A.CompanyGroupId, A.MaterialAttributeId, A.MaterialMasterId, A.SourceType, A.Sequence, A.Code, A.ShortName, A.StandardName, A.UserName, A.IsDefault, A.Remarks, A.Description, A.Active, A.Archive
                            FROM [HKP].[MaterialAttributeValue] AS A WHERE A.MaterialMasterId='" + masterId + "' ORDER BY A.Sequence";
				return _sqlRepository.GetDataCollection(_sql, null);
			}
			catch (Exception)
			{
				throw;
			}
		}

		public GridModel GetAttributeValueList(GridParameter parameters, string assignment, string materialMasterId, string attributeId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				if (ValueAssignmentEnum.General.ToString() == assignment)
				{
					parameters.CmdText = @"SELECT Id AS MaterialAttributeValueId, NULL AS MaterialMasterAttributeValueId, [Sequence], Code, ShortName, StandardName, UserName
                                           FROM HKP.MaterialAttributeValue WHERE CompanyGroupId='" + identity.CompanyGroupId + @"' AND MaterialAttributeId = '" + attributeId + "'";
				}
				else
				{
					parameters.CmdText = @"SELECT Id AS MaterialAttributeValueId, NULL AS MaterialMasterAttributeValueId, [Sequence], Code, ShortName, StandardName, UserName
						FROM HKP.MaterialAttributeValue WHERE CompanyGroupId='" + identity.CompanyGroupId + "' AND MaterialAttributeId = '" + attributeId + "' AND MaterialMasterId='" + materialMasterId + "'";
				}
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
		}

		public void InsertOrUpdateGraph(MaterialMasterAttribute attribute, IEnumerable<MaterialMasterAttributeValue> entities, IEnumerable<MaterialMasterAttributeValue> dbList)
		{
			if (entities != null)
			{
				Check(entities);
				foreach (var item in entities)
				{
					CheckPropertiesAndCharLength(item, attribute);
					if (item.Id == 0)
					{
						item.MaterialMasterId = attribute.MaterialMasterId;
						item.MaterialMasterAttributeId = attribute.Id;
						InsertGraph(item);
					}
					else
						UpdateGraph(item);
				}
			}
			if (dbList != null)
			{
				var deleteList = dbList.Where(t => t.MaterialMasterAttributeId == attribute.Id).ToList();
				foreach (var item in deleteList)
				{
					if (!entities.Any(t => t.Id == item.Id))
						base.DeleteGraph(item);
				}
			}
		}

		public void DeleteGraph(IEnumerable<MaterialMasterAttributeValue> attributeValueList)
		{
			if (attributeValueList != null)
			{
				foreach (var item in attributeValueList)
				{
					base.DeleteGraph(item);
				}
			}
		}

		private void Check(IEnumerable<MaterialMasterAttributeValue> entities)
		{
			// Duplicate Budget activity checking.
			var duplicateCode = entities.GroupBy(x => new { x.Code }).Where(x => x.Skip(1).Any());
			if (duplicateCode.Any())
				throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "Code (" + duplicateCode.FirstOrDefault().Key + ")"));
			var duplicateUserName = entities.GroupBy(x => new { x.Code }).Where(x => x.Skip(1).Any());
			if (duplicateUserName.Any())
				throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "UserName (" + duplicateUserName.FirstOrDefault().Key + ")"));
			var duplicateIsDefault = entities.Where(t => t.IsDefault && t.Active);
			if (duplicateIsDefault != null && duplicateIsDefault.Count() > 1)
				throw new CustomException("Default value already set.");
		}

		private static void CheckPropertiesAndCharLength(MaterialMasterAttributeValue entity, MaterialMasterAttribute attribute)
		{
			if (attribute != null)
			{
				if (attribute.AttributeProperty == AttributePropertiesEnum.Integer.ToString())
				{
					if (!int.TryParse(entity.UserName, out int userName))
						throw new CustomException("User name is not integer");
				}
				else if (attribute.AttributeProperty == AttributePropertiesEnum.Decimal.ToString())
				{
					if (!decimal.TryParse(entity.UserName, out decimal userName))
						throw new CustomException("User name is not decimal");
				}
				else
				{
					if (attribute.IsFixedNoOfCharacter &&
					   (attribute.NoOfCharacter < entity.UserName.Count() || attribute.NoOfCharacter > entity.UserName.Count()))
						throw new Exception("User name must be [" + attribute.NoOfCharacter + "] character");
				}
			}
		}

	}
}
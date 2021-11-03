#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Reflection;

#endregion Using

namespace Library.Service.Productions
{
    public class RecipeConfigService : Service<RecipeConfig>, IRecipeConfigService
	{
		#region Constructor

		private readonly IUnitOfWork _unitOfWork;
		private readonly ISqlRepository _sqlRepository;

		public RecipeConfigService(
			IRepositoryAsync<RecipeConfig> RecipeConfigRepository,
			IPKGeneratorService pkGeneratorService,
			IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			) :
			base(RecipeConfigRepository, unitOfWork, pkGeneratorService)
		{
			_unitOfWork = unitOfWork;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		private void Check(RecipeConfig entity)
		{
			if (base.Any(t => t.Id != entity.Id && t.PlantId == entity.PlantId && t.ProcessId == entity.ProcessId))
				throw new CustomException("This process already exist in this plant.");
		}

		public override void Insert(RecipeConfig entity)
		{
			try
			{
				Check(entity);
				entity.Id = GetAutoNumber(nameof(RecipeConfig), PKGeneratorEnum.Yearly, null, DateTime.Now);
				base.Insert(entity);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
				Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
				ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
			}
		}

		public override void Update(RecipeConfig entity)
		{
			try
			{
				Check(entity);
				base.Update(entity);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
			}
		}

		public void Delete(string id)
		{
			try
			{
				var entity = Find(id);
				base.Delete(entity);
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
		}

		public GridModel Query(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText = @"SELECT BRC.Id, BRC.CompanyGroupId, BRC.CompanyId, BRC.PlantId
								, BRC.ProcessId, PR.UserName AS ProcessName
								, BRC.OutputDependAttributeId, A.UserName AS OutputDependAttribute
								, BRC.OutputDependCharacteristicsId, H.UserName AS OutputDependCharacteristics
								, BRC.OutputDependSubprocessId, B.UserName AS OutputDependSubprocess
								, BRC.OutPutUoMId, F.UserName AS OutPutUoM, BRC.OutputLevel
								, BRC.RawMaterialConsumptionAattributeId, C.UserName AS RawMaterialConsumptionAattribute
								, BRC.RawMaterialConsumptionCharacteristicsId, I.UserName AS RawMaterialConsumptionCharacteristics
								, BRC.RmConsumptionUoMId, G.UserName AS RmConsumptionUoM, BRC.ConsumptionLevel
								, BRC.RecipeDependAttributeId, E.UserName AS RecipeDependAttribute
								, BRC.RecipeDependCharacteristicsId, J.UserName AS RecipeDependCharacteristics
								, BRC.RecipeDependonSubprocessId, D.UserName AS RecipeDependonSubprocess
								, BRC.RecipeLevel 
                                ,BRC.SpecificationLevel1 as SpecificationLevel1
								,BRC.SpecificationAttributeId1 as SpecificationAttributeId1
								,BRC.SpecificationCharacteristicId1 as SpecificationCharacteristicId1
                                ,BRC.SpecificationLevel2 as SpecificationLevel2
								,BRC.SpecificationAttributeId2 as SpecificationAttributeId2
								,BRC.SpecificationCharacteristicId2 as SpecificationCharacteristicId2
						FROM [SCS].[RecipeConfig] AS BRC
						LEFT JOIN [HKP].[Process] AS PR ON BRC.ProcessId=PR.Id
						LEFT JOIN [HKP].[MaterialAttribute] AS A ON BRC.OutputDependAttributeId=A.Id
						LEFT JOIN [HKP].[Characteristics] AS H ON BRC.OutputDependCharacteristicsId=H.Id
						LEFT JOIN [HKP].[SubProcess] AS B ON BRC.OutputDependSubprocessId=B.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS F ON BRC.OutPutUoMId=F.Id
						LEFT JOIN [HKP].[MaterialAttribute] AS C ON BRC.RawMaterialConsumptionAattributeId=C.Id
						LEFT JOIN [HKP].[Characteristics] AS I ON BRC.RawMaterialConsumptionCharacteristicsId=I.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS G ON BRC.RmConsumptionUoMId=G.Id
						LEFT JOIN [HKP].[MaterialAttribute] AS E ON BRC.RecipeDependAttributeId=E.Id
						LEFT JOIN [HKP].[Characteristics] AS J ON BRC.RecipeDependCharacteristicsId=J.Id
						LEFT JOIN [HKP].[SubProcess] AS D ON BRC.RecipeDependonSubprocessId=D.Id
						WHERE BRC.PlantId='" + plantId + "'";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
			}
		}
	}
}
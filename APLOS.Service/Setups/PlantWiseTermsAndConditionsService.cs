#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class PlantWiseTermsAndConditionsService : Service<PlantWiseTermsAndConditions>, IPlantWiseTermsAndConditionsService
	{
		#region Constructor

		private readonly ISqlRepository _sqlRepository;

		public PlantWiseTermsAndConditionsService(
			IRepositoryAsync<PlantWiseTermsAndConditions> PlantWiseTermsAndConditionsRepository
			, IPKGeneratorService pkGeneratorService
			, IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			) :
			base(PlantWiseTermsAndConditionsRepository, unitOfWork, pkGeneratorService)
		{
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		private string GetPK()
		{
			return GetAutoNumber(nameof(PlantWiseTermsAndConditions), PKGeneratorEnum.Auto, null, DateTime.Now);
		}

		public override void Insert(PlantWiseTermsAndConditions entity)
		{
			try
			{
				entity.Id = "PTC-" + GetPK();
				base.Insert(entity);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
				Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
				ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public override void Update(PlantWiseTermsAndConditions entity)
		{
			try
			{
				base.Update(entity);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
				Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
				ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public GridModel Query(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText = @"SELECT PTC.*,P.UserName Plant FROM [SCS].[PlantWiseTermsAndConditions] PTC
										LEFT JOIN ORG.Plant AS P ON PTC.PlantId=P.Id Where PTC.PlantId='" + plantId + "'";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public IEnumerable<object> GetTermsAndConditionsByPreRecruitmentEmployee(string preRecruitmentEmployeeId)
		{
			try
			{
				var sql = @"SELECT * FROM [TRN].[EmployeeWiseTermsAndConditions] WHERE PreRecruitmentEmployeeId ='" + preRecruitmentEmployeeId + "'";
				return _sqlRepository.GetDataCollection(sql, null);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
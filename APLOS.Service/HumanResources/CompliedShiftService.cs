using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.HumanResources;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.HumanResources
{
	public class CompliedShiftService : Service<CompliedShift>, ICompliedShiftService
	{
		#region Constructor

		private readonly ISqlRepository _sqlRepository;
		private readonly IUnitOfWork _unitOfWork;

		public CompliedShiftService(
			IRepositoryAsync<CompliedShift> compliedShiftRepository
			, IPKGeneratorService pkGeneratorService
			, ISqlRepository sqlRepository
			, IUnitOfWork unitOfWork) : base(compliedShiftRepository, unitOfWork, pkGeneratorService)
		{
			_unitOfWork = unitOfWork;
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		private void Check(CompliedShift entity)
		{
			if (base.Any(t => t.Id != entity.Id && t.Code == entity.Code && t.PlantId == entity.PlantId))
				throw new CustomException("Code ("+entity.Code + ") already Exist in this plant");
			if (base.Any(t => t.Id != entity.Id && t.ShiftName == entity.ShiftName && t.PlantId == entity.PlantId))
				throw new CustomException("Name (" + entity.ShiftName + ") already Exist in this plant");
		}

		public override void Insert(CompliedShift entity)
		{
			try
			{
				Check(entity);
                entity.Id = GetAutoNumber(nameof(CompliedShift), PKGeneratorEnum.Auto, null, DateTime.Now);
				base.Insert(entity);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
				Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
				ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public override void Update(CompliedShift entity)
		{
			try
			{
				Check(entity);
                //if (entity.IsNight && entity.OutTime > entity.InTime)
                //{
                //    throw new CustomException("Please rectify time");
                //}
                //if (!entity.IsNight && entity.InTime > entity.OutTime)
                //{
                //    throw new CustomException("Please rectify time");
                //}
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
				parameters.CmdText = @"SELECT [Id],[CompanyGroupId],[PlantId],[Code],[ShiftName],CONVERT(VARCHAR(5),InTime, 108) InTime,CONVERT(VARCHAR(5),OutTime, 108) [OutTime],[IsNight],[AddedBy],[AddedFromIP],[AddedDate],[UpdatedBy],[UpdatedDate] ,[UpdatedFromIP]
                                       FROM [HKP].[CompliedShift] WHERE PlantId='" + plantId + "'";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public IEnumerable<object> GetCbo(string plantId)
		{
			try
			{
				return from m in base.Query(r => r.PlantId == plantId).Select().OrderBy(r => r.ShiftName)
					   select new { Text = m.ShiftName, Value = m.Id };
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
	}
}
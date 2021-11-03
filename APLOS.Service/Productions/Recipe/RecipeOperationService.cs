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
using System.Linq;
using System.Reflection;

namespace Library.Service.Productions
{
    public class RecipeOperationService : Service<RecipeOperation>, IRecipeOperationService
	{
		#region Constructor

		private readonly ISqlRepository _sqlRepository;
		private readonly IUnitOfWork _unitOfWork;

		public RecipeOperationService(
			IRepositoryAsync<RecipeOperation> recipeOperationRepository
			, IPKGeneratorService pkGeneratorService
			, ISqlRepository sqlRepository
			, IUnitOfWork unitOfWork) : base(recipeOperationRepository, unitOfWork, pkGeneratorService)
		{
			_unitOfWork = unitOfWork;
			_sqlRepository = sqlRepository;
		}

        #endregion Constructor

        private void Check(RecipeOperation entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }

        public override void Insert(RecipeOperation entity)
		{
			try
			{
				Check(entity);
                entity.Id = GetAutoNumber(nameof(RecipeOperation), PKGeneratorEnum.Auto, null, DateTime.Now);
				base.Insert(entity);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
				Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
				ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public override void Update(RecipeOperation entity)
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
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public GridModel Query(GridParameter parameters)
		{
			try
			{
				parameters.CmdText = @"SELECT *  FROM [HKP].[RecipeOperation]";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

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
    }
}
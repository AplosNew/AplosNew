#region Using

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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Productions
{
    public class ProductionSettingsService : Service<ProductionSettings>, IProductionSettingsService
    {
        #region Constructor

        private readonly IProcessCapacityUOMService _processCapacityUOMService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ProductionSettingsService(
            IRepositoryAsync<ProductionSettings> ProductionSettingsRepository,
            IProcessCapacityUOMService processCapacityUOMService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(ProductionSettingsRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _processCapacityUOMService = processCapacityUOMService;
        }

        #endregion Constructor

        public IEnumerable<object> Query(string plantId)
        {
            try
            {
                string _sql = @"SELECT PS.Id
		                                ,PS.PlantId
		                                ,PS.SAMUomId
		                                ,PS.NeoclearProcessId
		                                ,PS.BomOrRecipe
		                                ,PS.IsMultipleOrderAllowedInBatch
                                FROM TRN.ProductionSettings AS PS
                                WHERE PS.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProductionSettings), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public void InsertGraph(ProductionSettings productionSettings, IEnumerable<ProcessCapacityUOM> processCapacityUOM)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                productionSettings.Id = GetPK();
                _processCapacityUOMService.InsertUpdateOrDeleteGraph(processCapacityUOM, productionSettings.PlantId);
                base.InsertGraph(productionSettings);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, productionSettings.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateGraph(ProductionSettings productionSettings, IEnumerable<ProcessCapacityUOM> processCapacityUOM)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                _processCapacityUOMService.InsertUpdateOrDeleteGraph(processCapacityUOM, productionSettings.PlantId);
                base.UpdateGraph(productionSettings);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, productionSettings.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string plantId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                ProductionSettings entity = base.Query(t => t.PlantId == plantId).Select().FirstOrDefault();
                _processCapacityUOMService.DeleteGraph(plantId);
                base.DeleteGraph(entity);
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
    }
}
#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Materials
{
    public class MaterialGroupAlternativeUoMService : Service<MaterialGroupAlternativeUoM>, IMaterialGroupAlternativeUoMService
    {
        #region Constructor

        /// <summary>   The unit of work. </summary>
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public MaterialGroupAlternativeUoMService(
            IRepositoryAsync<MaterialGroupAlternativeUoM> materialMasterAlternativeUOMRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialMasterAlternativeUOMRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetAltUomListMasterId(string masterId)
        {
            try
            {
                string _sql = @"SELECT MMAU.Id
	                                  ,MMAU.MaterialGroupMasterId
	                                  ,MMAU.AlternativeUoMId
	                                  ,MMAU.BaseUoMId
	                                  ,MMAU.AlternativeUoMFactor
	                                  ,MMAU.BaseUoMFactor
	                                  ,UOMA.UserName AS AlternativeUoMName
	                                  ,UOMB.UserName AS BaseUoMName
                                FROM MST.MaterialGroupAlternativeUoM AS MMAU
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOMA ON MMAU.AlternativeUoMId=UOMA.Id
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOMB ON MMAU.BaseUoMId=UOMB.Id
                                WHERE MMAU.MaterialGroupMasterId='" + masterId + "'";
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

        public IEnumerable<object> GetAltUomList()
        {
            try
            {
                string _sql = @"SELECT MMAU.Id
	                                  ,MMAU.MaterialGroupMasterId
	                                  ,MMAU.AlternativeUoMId
	                                  ,MMAU.BaseUoMId
	                                  ,MMAU.AlternativeUoMFactor
	                                  ,MMAU.BaseUoMFactor
	                                  ,UOMA.UserName AS AlternativeUoMName
	                                  ,UOMB.UserName AS BaseUoMName
                                FROM MST.MaterialGroupAlternativeUoM AS MMAU
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOMA ON MMAU.AlternativeUoMId=UOMA.Id
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOMB ON MMAU.BaseUoMId=UOMB.Id";
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

        public void InsertOrUpdateGraph(IEnumerable<MaterialGroupAlternativeUoM> entities, string masterId)
        {
            try
            {
                if (entities != null)
                {
                    var pk = _pkGeneratorService.GetMaxNumber(nameof(MaterialGroupAlternativeUoM), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = pk.MaxNumber++.ToString();
                            item.MaterialGroupMasterId = masterId;
                            InsertGraph(item);
                        }
                        else
                            UpdateGraph(item);
                    }
                }
                var dbList = Query(m => m.MaterialGroupMasterId == masterId).Select();
                if (dbList != null)
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
                            {
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraph(string masterId)
        {
            var dbList = Query(m => m.MaterialGroupMasterId == masterId).Select();
            if (dbList != null)
            {
                foreach (var item in dbList)
                {
                    base.DeleteGraph(item);
                }
            }
        }
    }
}
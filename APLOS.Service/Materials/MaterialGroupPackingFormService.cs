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
    public class MaterialGroupPackingFormService : Service<MaterialGroupPackingForm>, IMaterialGroupPackingFormService
    {
        #region Constructor

        /// <summary>   The unit of work. </summary>
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public MaterialGroupPackingFormService(
            IRepositoryAsync<MaterialGroupPackingForm> repository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(repository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> Query(string masterId)
        {
            try
            {
                string _sql = @"SELECT MP.Id
                                      ,MP.MaterialGroupMasterId
                                      ,MP.PackingFormId
                                      ,P.UserName AS PackingFormName
                                      ,MP.Sequence
                                      ,MP.IsSingleEntry
                               FROM MST.MaterialGroupPackingForm AS MP
                               INNER JOIN HKP.PackingForm AS P ON MP.PackingFormId=P.Id
                               WHERE MP.MaterialGroupMasterId='" + masterId + "' ORDER BY MP.Sequence";
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

        public void InsertOrUpdateGraph(IEnumerable<MaterialGroupPackingForm> entities, string masterId)
        {
            try
            {
                if (entities != null)
                {
                    var pk = _pkGeneratorService.GetMaxNumber(nameof(MaterialGroupPackingForm), PKGeneratorEnum.Auto, null, DateTime.Now);
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
                var dbList = base.Query(m => m.MaterialGroupMasterId == masterId).Select();
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
            var dbList = base.Query(m => m.MaterialGroupMasterId == masterId).Select();
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
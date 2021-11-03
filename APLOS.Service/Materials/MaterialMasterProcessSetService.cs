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

namespace Library.Service.Materials
{
    public partial class MaterialMasterProcessSetService : Service<MaterialMasterProcessSet>, IMaterialMasterProcessSetService
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaterialTypeService _materialTypeService;

        public MaterialMasterProcessSetService(
            IRepositoryAsync<MaterialMasterProcessSet> masterProcessSetRepository
            , IPKGeneratorService pkGeneratorService
            , IMaterialTypeService materialTypeService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(masterProcessSetRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _materialTypeService = materialTypeService;
            _sqlRepository = sqlRepository;
        }

        #endregion

        public IEnumerable<object> Query(string materialMasterId)
        {
            try
            {
                var _sql = @"SELECT MMPS.Id
								, MMPS.MaterialMasterId
								, MMPS.ProcessId, p.UserName AS ProcessName
								, MMPS.[Sequence], MMPS.IsBaseProcess, MMPS.[Days], MMPS.Symbol
								, MMPS.ProductionCycleTime, MMPS.JobWorkApplicable, MMPS.JobWorkType
								, MMPS.EntityIdWithinCompany, MMPS.EntityIdWithinGroup, MMPS.PartyId
								, EntityOrVendorName= CASE WHEN MMPS.EntityIdWithinCompany<>'' THEN EWC.UserName 
											WHEN MMPS.EntityIdWithinGroup<>'' THEN EWG.UserName
											WHEN MMPS.PartyId<>'' THEN PRT.UserName
											ELSE PRT.UserName END
						FROM [MST].[MaterialMasterProcessSet] AS MMPS
						LEFT OUTER JOIN HKP.Process AS P ON MMPS.ProcessId=P.Id
						LEFT OUTER JOIN ORG.Entity AS EWC ON MMPS.EntityIdWithinCompany=EWC.Id
						LEFT OUTER JOIN ORG.Entity AS EWG ON MMPS.EntityIdWithinGroup=EWG.Id
						LEFT OUTER JOIN HKP.Party AS PRT ON MMPS.PartyId=PRT.Id
						WHERE MMPS.MaterialMasterId='" + materialMasterId + "' ORDER BY MMPS.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public void InsertGraph(string materialMasterId, IEnumerable<MaterialMasterProcessSet> entity)
        {
            try
            {
                if (entity != null)
                {
                    var pk = GetMaxNumber(nameof(MaterialMasterProcessSet), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entity)
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.MaterialMasterId = materialMasterId;
                        base.InsertGraph(item);
                    }
                }
                //else
                //{
                //    var processMandatory = _materialTypeService.Query(t => t.Id == materialTypeId).Select(t => t.IsProcessMandatory).FirstOrDefault();
                //    if (processMandatory)
                //        throw new CustomException("Insert process set!");
                //}
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
        }

        public void InsertOrUpdateGraph(string materialMasterId, IEnumerable<MaterialMasterProcessSet> entity)
        {
            try
            {
                if (entity != null)
                {
                    var pk = GetMaxNumber(nameof(MaterialMasterProcessSet), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entity)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.MaterialMasterId = materialMasterId;
                            base.InsertGraph(item);
                        }
                        else
                            UpdateGraph(item);
                    }
                }
                //else
                //{
                //    var processMandatory = _materialTypeService.Query(t => t.Id == materialTypeId).Select(t => t.IsProcessMandatory).FirstOrDefault();
                //    if (processMandatory)
                //        throw new CustomException("insert at least one process!");
                //}
                var dbList = base.Query(t => t.MaterialMasterId == materialMasterId).Select().AsEnumerable();
                if (dbList.Count() > 0)
                {
                    if (entity == null)
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
                            if (!entity.Any(t => t.Id == item.Id))
                                base.DeleteGraph(item);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraph(string materialMasterId)
        {
            try
            {
                var dbList = base.Query(t => t.MaterialMasterId == materialMasterId).Select().AsEnumerable();
                if (dbList != null)
                {
                    foreach (var item in dbList)
                    {
                        base.DeleteGraph(item);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}
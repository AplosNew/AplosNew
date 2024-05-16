#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Processes;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Processes
{
    public class EntityProcessTagService : Service<EntityProcessTag>, IEntityProcessTagService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IProcessService _processService;
        private readonly ISqlRepository _sqlRepository;

        public EntityProcessTagService(
            IRepositoryAsync<EntityProcessTag> EntityProcessTagRepository,
            IProcessService processService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(EntityProcessTagRepository, unitOfWork, pkGeneratorService)
        {
            _processService = processService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(EntityProcessTag), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private bool CheckUniqueRow(EntityProcessTag entity)
        {
            try
            {
                return Any(r => r.Id != entity.Id && r.EntityId == entity.EntityId && r.IsDispatchSKURequired == entity.IsDispatchSKURequired && r.ProcessNature==entity.ProcessNature);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CheckUnique(EntityProcessTag entity)
        {
            try
            {
                return Any(r => r.Id != entity.Id && r.EntityId == entity.EntityId  && r.IsPackingSKURequired == entity.IsPackingSKURequired && r.ProcessNature == entity.ProcessNature);
            }
            catch (Exception)
            {
                throw;
            }
        }
      
        public void InsertUpdateOrDelete(IEnumerable<EntityProcessTag> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var entityId = entities.FirstOrDefault().EntityId;
                var dbList = base.Query(t => t.EntityId == entityId).Select().ToList();

                if (entities != null)
                {
                    var pk = GetMaxNumber(nameof(EntityProcessTag), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            if (CheckUniqueRow(item) || CheckUnique(item))
                            {
                                item.IsFinishGoods = false;
                                item.ProcessNature = string.Empty;
                                item.PackingForm = string.Empty;
                                item.IsPackingSKURequired = false;
                                item.DispatchForm = string.Empty;
                                item.IsDispatchSKURequired = false;
                                item.DispatchType = string.Empty;
                                
                            }
                            InsertGraph(item);
                        }
                        else
                        {
                            item.ModelState = ModelState.Modified;
                            if (CheckUniqueRow(item) || CheckUnique(item))
                            {
                                item.IsFinishGoods = false;
                                item.ProcessNature = string.Empty;
                                item.PackingForm = string.Empty;
                                item.IsPackingSKURequired = false;
                                item.DispatchForm = string.Empty;
                                item.IsDispatchSKURequired = false;
                                item.DispatchType = string.Empty;
                            }
                            UpdateGraph(item);
                        }

                        if (!CheckUniqueRow(item) || !CheckUnique(item))
                        {

                           
                        }
                    }
                }
                if (dbList.IsNotNull() && dbList.Count > 0)
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
                                base.DeleteGraph(item);
                        }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string entityId)
        {
            parameters.sort = "Sequence";
            parameters.order = "ASC";
            parameters.CmdText = @"SELECT DISTINCT EPT.Id, EPT.EntityId, EPT.ProcessId,EPT.IsFinishGoods,EPT.ProcessNature,EPT.LotNumberCapture,EPT.LotNumberMandatory
                                ,EPT.IsPackingSKURequired,EPT.PackingForm,EPT.IsDispatchSKURequired,EPT.DispatchForm,EPT.DispatchType
								, P.[Sequence], P.Code, P.UserName, P.ShortName
								, P.StandardName, MT.[Description] AS MaterialType, P.Active
                                , P.IsProductionProcess,ept.IsParameterBased
								--, PP.UserName ProductionProcessGroup
                                --, EPT.ProductionProcessGroupId
                                , EPT.ProductionBookingLevel
								, CAST(0 as BIT) AS Archive,EPT.IsSKU1,EPT.IsSKU2,EPT.IsSKU3,EPT.ToCloseAllowed,EPT.IsScanApplicable
					FROM [HKP].[EntityProcessTag] AS EPT
					LEFT JOIN HKP.Process AS P ON EPT.ProcessId=P.Id
					LEFT JOIN HKP.MaterialType AS MT ON P.MaterialTypeId=MT.Id
					--LEFT JOIN HKP.ProductionProcessGroup PP ON EPT.ProductionProcessGroupId=PP.Id
					WHERE EPT.EntityId='" + entityId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetProcessListByEntity(GridParameter parameters, string entityId)
        {
            parameters.CmdText = @"SELECT distinct EPT.Id, EPT.EntityId, EPT.ProcessId
									, P.[Sequence], P.Code, P.UserName, P.ShortName
									, P.StandardName, P.Active
							FROM HKP.EntityProcessTag AS EPT
							JOIN HKP.Process AS P ON EPT.ProcessId=P.Id
							WHERE EPT.EntityId='" + entityId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public void DeleteGraph(string entityId, string productionProcessGroupId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var dbList = base.Query(t => t.EntityId == entityId).Select().ToList();
                foreach (var item in dbList)
                {
                    base.DeleteGraph(dbList);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void Delete(string id)
        {
            try
            {
                var data = base.Find(id);
                base.Delete(data);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public GridModel GetEntityProcessCbo(bool cadmin, bool sadmin, string userId, string entityId)
        {
            if (cadmin || sadmin)
            {
                string _sql = @"SELECT DISTINCT P.Id AS [Value], P.UserName AS [Text],
--ISNULL(PS.ProductionBookingLevel,EP.ProductionBookingLevel)ProductionBookingLevel,
EP.ProductionBookingLevel,
EP.LotNumberMandatory,EP.LotNumberCapture,EP.IsSKU1,EP.IsSKU2,EP.IsSKU3,P.IsFirst,EP.IsParameterBased,EP.ToCloseAllowed
FROM HKP.EntityProcessTag AS EP
JOIN HKP.Process AS P ON EP.ProcessId=P.Id 
LEFT JOIN (Select S.ProductionBookingLevel,P.EntityId,S.ProcessId,IsBaseProcess from TRN.ProductionOrderProcessSet S
LEFT JOIN TRN.ProductionOrder P ON P.Id=ProductionOrderId
) PS ON PS.ProcessId=EP.ProcessId AND PS.EntityId=EP.EntityId
WHERE EP.EntityId='" + entityId + "' AND P.Active=1";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
            else
            {
                string _sql = @"SELECT distinct P.Id AS [Value], P.UserName AS [Text],
--ISNULL(PS.ProductionBookingLevel,EPT.ProductionBookingLevel)ProductionBookingLevel,
EPT.ProductionBookingLevel,EPT.LotNumberMandatory,EPT.LotNumberCapture
,EPT.IsSKU1,EPT.IsSKU2,EPT.IsSKU3,P.IsFirst,EPT.IsParameterBased,EPT.ToCloseAllowed FROM HKP.EntityProcessTag EPT
INNER JOIN HKP.Process AS P ON P.Id=EPT.ProcessId
LEFT JOIN (Select S.ProductionBookingLevel,P.EntityId,S.ProcessId,IsBaseProcess from TRN.ProductionOrderProcessSet S
LEFT JOIN TRN.ProductionOrder P ON P.Id=ProductionOrderId
) PS ON PS.ProcessId=EPT.ProcessId AND PS.EntityId=EPT.EntityId
INNER JOIN [SEC].[UserProcess] UP ON UP.ProcessId=P.Id
						        WHERE EPT.EntityId='" + entityId + @"' AND UP.UserId='"+ userId + "' AND P.Active=1";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
        }

    }
}
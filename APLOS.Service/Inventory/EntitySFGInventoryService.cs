#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
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

namespace Library.Service.Inventory
{
    public class EntitySFGInventoryService : Service<EntitySFGInventory>, IEntitySFGInventoryService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public EntitySFGInventoryService(
            IRepositoryAsync<EntitySFGInventory> SFGMovementEntityRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(SFGMovementEntityRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(EntitySFGInventory), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertUpdateOrDelete(IEnumerable<EntitySFGInventory> entities)
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
                    var pk = GetMaxNumber(nameof(EntitySFGInventory), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            InsertGraph(item);
                        }
                        else
                        {
                            item.ModelState = ModelState.Modified;
                            UpdateGraph(item);
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
            parameters.CmdText = @"SELECT A.*,B.[Sequence], B.Code,B.UserName,B.ShortName, B.StandardName
                                 FROM MST.EntitySFGInventory A
                                 LEFT JOIN [HKP].[SFGInventory] B ON B.Id=A.SFGInventoryId
                                 WHERE A.EntityId='" + entityId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetProcessListByEntity(GridParameter parameters, string entityId)
        {
            parameters.CmdText = @"SELECT distinct EPT.Id, EPT.EntityId, EPT.ProcessId
									, P.[Sequence], P.Code, P.UserName, P.ShortName
									, P.StandardName, P.Active
							FROM HKP.SFGMovementEntity AS EPT
							JOIN HKP.Process AS P ON EPT.ProcessId=P.Id
							WHERE EPT.EntityId='" + entityId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public void DeleteGraph(string entityId)
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
                string _sql = @"SELECT DISTINCT P.Id AS [Value], P.UserName AS [Text],EP.ProductionBookingLevel FROM HKP.SFGMovementEntity AS EP
                            JOIN HKP.Process AS P ON EP.ProcessId=P.Id WHERE EP.EntityId='" + entityId + "' AND P.Active=1";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
            else
            {
                string _sql = @"SELECT P.Id AS [Value], P.UserName AS [Text],EPT.ProductionBookingLevel FROM HKP.SFGMovementEntity EPT
						        INNER JOIN HKP.Process AS P ON P.Id=EPT.ProcessId
						        INNER JOIN [SEC].[UserProcess] UP ON UP.ProcessId=P.Id
						        WHERE EPT.EntityId='" + entityId + @"' AND UP.UserId='"+ userId + "' AND P.Active=1";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
        }

    }
}
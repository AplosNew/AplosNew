#region Using

using Library.Core;
using Library.Crosscutting.Security;
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
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Setups
{
    public class IntermediateItemEntityService : Service<IntermediateItemEntity>, IIntermediateItemEntityService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public IntermediateItemEntityService(
            IRepositoryAsync<IntermediateItemEntity> IntermediateItemEntityRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(IntermediateItemEntityRepository, unitOfWork, pkGeneratorService)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region GetDayList

        public GridModel GetCbo(string buyerId, string companyGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                //var sql = $"SELECT um.Id AS Value, um.UserName as Text FROM {DbSchema.Setup}.[{DbTable.IntermediateItemEntity}] AS um " +
                //          $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseUOM}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgu " +
                //          $"ON um.Id = cgu.UOMId  WHERE ISNULL(cgu.Id, '')<> '' AND  um.Archive=0 ";

                var sql = @"SELECT B.Id Value,B.UserName Text FROM [HKP].[IntermediateItemEntity] B
                            LEFT JOIN HKP.CompanyGroupIntermediateItemEntity CB ON B.Id=CB.IntermediateItemEntityId
                            WHERE B.BuyerId='" + buyerId + "' AND B.Archive =0 AND B.Active=1 AND CB.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
        }

        #endregion GetDayList

        #region InsertUpdate

        public void InsertORUpdate(IEnumerable<IntermediateItemEntity> entities)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(IntermediateItemEntity), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                //Check(entities);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.CompanyGroupId = identity.CompanyGroupId;
                        InsertGraph(item);
                    }
                    else
                    {
                        UpdateGraph(item);
                    }
                }
                string entityId = entities.First().EntityId;
                string intermediateItemId = entities.First().IntermediateItemId;
                IEnumerable<IntermediateItemEntity> dbList = base.Query(r => r.EntityId == entityId).Select();
                if (dbList != null && dbList.Count() > 0)
                {
                    if (entities == null)
                    {
                        foreach (var x in dbList)
                        {
                            Delete(x);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id && t.EntityId == item.EntityId && t.IntermediateItemId == item.IntermediateItemId))
                            {
                                Delete(item);
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null, ErrorType.ServiceError,
    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void Check(IEnumerable<IntermediateItemEntity> entities)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            foreach (var entity in entities)
            {
                var db_Data = base.Query(t => t.Id != entity.Id && t.IntermediateItemId == entity.IntermediateItemId && t.EntityId == entity.EntityId && entity.Archive).Select().FirstOrDefault();
                if (db_Data != null)
                    throw new CustomException("item duplicate found!");
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(IntermediateItemEntity), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private string GetAutoComGBDepartmentId()
        {
            return _pkGeneratorService.GetAutoNumber("CompanyGroupIntermediateItemEntity", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        #endregion InsertUpdate

        public void DeleteGraph(string key)
        {
            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                IntermediateItemEntity IntermediateItemEntity = Find(key);
                base.DeleteGraph(IntermediateItemEntity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string entityId, string companyGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.CmdText = @"SELECT I.*,it.Code,it.ShortName,it.StandardName,it.UserName FROM [HKP].[IntermediateItemEntity] I
                LEFT JOIN HKP.IntermediateItem it ON I.IntermediateItemId=it.Id
                WHERE I.EntityId='" + entityId + "' AND I.Archive =0 AND I.Active=1 AND I.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }
    }
}
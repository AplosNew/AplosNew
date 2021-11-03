#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Machines;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Machines
{
    public partial class EntityOperationSettingsService : Service<EntityOperationSettings>, IEntityOperationSettingsService
    {
        #region Constructor

        private readonly IRepositoryAsync<EntityOperationSettings> _baseRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public EntityOperationSettingsService(
            IRepositoryAsync<EntityOperationSettings> baseRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(baseRepository, unitOfWork)
        {
            _baseRepository = baseRepository;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertGraph(IEnumerable<EntityOperationSettings> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var currentId = _baseRepository.SqlQuery<int>(@"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [SCS].[EntityOperationSettings] WHERE EntityId='" + entities.First().EntityId + "'").First();
                foreach (var item in entities)
                {
                    if (item.NoOfEmployee == 0) throw new CustomException("Please input employee number.");
                    currentId++;
                    item.Id = MakePK(entities.First().EntityId + 2, currentId, 2);
                    base.InsertGraph(item);
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void UpdateGraph(IEnumerable<EntityOperationSettings> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var currentId = _baseRepository.SqlQuery<int>(@"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [SCS].[EntityOperationSettings] WHERE EntityId='" + entities.First().EntityId + "'").First();
                foreach (var item in entities)
                {
                    if (item.NoOfEmployee == 0) throw new CustomException("Please input employee number.");

                    if (string.IsNullOrEmpty(item.Id))
                    {
                        currentId++;
                        item.Id = MakePK(entities.First().EntityId + 2, currentId, 2);
                        base.InsertGraph(item);
                    }
                    else
                        base.UpdateGraph(item);
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }


        public IEnumerable<object> Query(string entityId)
        {
            try
            {
                var sql = @"SELECT EOS.Id, EOS.PlantId, EOS.EntityId, EOS.OperationId, o.UserName AS OperationName
                                , Process=STUFF((SELECT DISTINCT ',' + P.UserName FROM MST.[OperationMachineSkill] AS OPMT
					                            LEFT OUTER JOIN HKP.[Process] AS P ON OPMT.ProcessId=P.Id
					                            WHERE OPMT.OperationId=o.Id  GROUP BY P.UserName FOR XML PATH ('') ),1,1,'')
                                , ot.UserName AS OperationTypeCode, oc.UserName AS OperationCategoryName
                                , OA.UserName AS OperationActivityName, EOS.NoOfEmployee, o.IsMachineRequired
                        FROM [SCS].[EntityOperationSettings] AS EOS
                        JOIN [MST].[Operation] as o ON EOS.OperationId=o.Id
                        LEFT JOIN HKP.[OperationType] as ot ON o.OperationTypeId = ot.Id
                        LEFT JOIN HKP.[OperationCategory] as oc ON o.OperationCategoryId = oc.Id
                        LEFT JOIN HKP.[OperationActivity] AS OA ON o.OperationActivityId=OA.Id
                        WHERE EOS.EntityId='" + entityId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }
    }
}
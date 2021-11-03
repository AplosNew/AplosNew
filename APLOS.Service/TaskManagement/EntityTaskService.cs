#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.TaskManagement;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.TaskManagement
{
    public class EntityTaskService : Service<EntityTask>, IEntityTaskService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public EntityTaskService(
            IRepositoryAsync<EntityTask> EntityTaskRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(EntityTaskRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(EntityTask), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertUpdateOrDelete(IEnumerable<EntityTask> entities)
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
                    var pk = GetMaxNumber(nameof(EntityTask), PKGeneratorEnum.Auto, null, DateTime.Now);
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
            parameters.CmdText = @"SELECT A.*,B.[Sequence], B.Code,B.UserDefineTask, B.StandardName,EI.EmployeeName TaskEmployeeName
                                 FROM [dbo].[EntityTask] A
                                 LEFT JOIN [dbo].[TaskMaster] B ON B.Id=A.TaskMasterId
                                 LEFT JOIN EmployeeInformation EI ON EI.SystemId=A.EmpSystemId
                                 WHERE A.EntityId='" + entityId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetTaskMasterData(GridParameter parameters)
        {
            parameters.CmdText = @"SELECT * FROM [dbo].[TaskMaster] WHERE ResponsiblePersonCategory='Entity'";
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


    }
}
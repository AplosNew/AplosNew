#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.External;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.External
{
    public class ActivityService : Service<ActivityEmp>, IActivityService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDocumentActivityService _documentActivityService;
        private readonly IKPIService _kpiService;
        private readonly IRepositoryAsync<ActivityEmp> _activityRepository;

        public ActivityService(
            IRepositoryAsync<ActivityEmp> activityRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , IDocumentActivityService documentActivityService
            , IKPIService kpiService
            , ISqlRepository sqlRepository
            ) : base(activityRepository, unitOfWork, pkGeneratorService)
        {
            _activityRepository = activityRepository;
            _unitOfWork = unitOfWork;
            _documentActivityService = documentActivityService;
            _kpiService = kpiService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Operation

        private void Check(ActivityEmp entity)
        {
            var ck = base.Query(a => a.Id != entity.Id && a.EmployeeId == entity.EmployeeId && a.Name == entity.Name).Select().FirstOrDefault();
            if (ck != null && ck.Name == entity.Name)
            {
                throw new CustomException("[" + entity.Name + "] already exists.");
            }
        }

        public void InsertOrUpdate(ActivityEmp entity)
        {
            try
            {
                if (entity != null)
                {
                    Check(entity);
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = GetAutoNumber(nameof(ActivityEmp), PKGeneratorEnum.Auto, null, DateTime.Now);
                        entity.AddedDateTime = DateTime.Now;
                        Insert(entity);
                    }
                    else
                    {
                        var dbdata = Find(entity.Id);
                        if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))

                            throw new CustomException("The record no longer exists.");
                        entity.AddedDateTime = DateTime.Now;
                        Update(entity);
                    }
                }
                else
                    throw new CustomException("Incomplete data.");
            }
            catch (CustomException)
            {
                throw;
            }
            //catch (Exception ex)
            //{
            //    throw new CustomException(ex.Message, ex,
            //       Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
            //       ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            //}
        }

        public void UpdateActivity(string id, string fieldName)
        {
            try
            {
                if (!string.IsNullOrEmpty(fieldName))
                {
                    var entity = Find(id);

                    if (fieldName == "IsDocument")
                        entity.Documents = true;
                    else if (fieldName == "IsKpi")
                        entity.KPI = true;
                    Update(entity);
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
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public void InsertOrUpdateDocument(DocumentActivity entity, string docPk)
        {
            var flag = false;
            try
            {
                if (!string.IsNullOrEmpty(entity.FileName))
                {
                    if (_documentActivityService.Any(t => t.Id != entity.Id && t.ActivityId == entity.ActivityId && t.FileName == entity.FileName))
                        throw new CustomException("This file is already exists!!!");
                }
                if (entity == null)
                    throw new CustomException("Incomplete data.");
                var dbdata = Find(entity.ActivityId);
                if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))
                    throw new CustomException("The record no longer exists.");

                flag = true;
                _unitOfWork.BeginTransaction();

                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = docPk;
                    entity.FileId = entity.Id;
                    if (string.IsNullOrEmpty(entity.FileName))
                    {
                        entity.FileId = null;
                    }
                    entity.AddedDateTime = DateTime.Now;
                    _documentActivityService.InsertGraph(entity);
                }
                else
                {
                    var db_data = _documentActivityService.Find(entity.Id);
                    if (db_data == null || string.IsNullOrEmpty(db_data.Id))
                        throw new CustomException("The record no longer exists.");
                    entity.FileId = entity.Id;
                    if (string.IsNullOrEmpty(entity.FileName))
                        entity.FileId = null;
                    entity.AddedDateTime = DateTime.Now;
                    _documentActivityService.UpdateGraph(entity);
                }
                ActivityEmp activity = Find(entity.ActivityId);
                if (!activity.Documents)
                {
                    activity.Documents = true;
                    UpdateGraph(activity);
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
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public string GetPk(DocumentActivity entity)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.Id))
                {
                    return entity.ActivityId + "-" + _documentActivityService.GetAutoNumber(nameof(DocumentActivity), PKGeneratorEnum.Auto, null, DateTime.Now);
                }
                else
                {
                    return entity.Id;
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
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public void InsertOrUpdateKPI(KPI entity)
        {
            var flag = false;
            try
            {
                flag = true;
                _unitOfWork.BeginTransaction();

                if (entity == null)
                    throw new CustomException("Incomplete data.");
                var dbdata = Find(entity.ActivityId);
                if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))
                    throw new CustomException("The record no longer exists.");

                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = _kpiService.GetAutoNumber(nameof(KPI), PKGeneratorEnum.Auto, null, DateTime.Now);
                    entity.AddedDateTime = DateTime.Now;
                    _kpiService.InsertGraph(entity);
                }
                else
                {
                    var db_data = _kpiService.Find(entity.Id);
                    if (db_data == null || string.IsNullOrEmpty(db_data.Id))
                        throw new CustomException("The record no longer exists.");
                    entity.AddedDateTime = DateTime.Now;
                    _kpiService.UpdateGraph(entity);
                }
                ActivityEmp activity = Find(entity.ActivityId);
                if (!activity.KPI)
                {
                    activity.KPI = true;
                    UpdateGraph(activity);
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
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string employeeId)

        {
            try
            {
                parameters.CmdText = @"SELECT E.*, AC.Name ActivityCategory, P.Name Period, AI.Name ActivityImportance FROM dbo.ActivityEmp E
                                       LEFT OUTER JOIN [dbo].[ActivityCategory] AC ON E.ActivityCategoryId=AC.Id
                                       LEFT OUTER JOIN [dbo].[Period] P ON E.PeriodId=P.Id
                                       LEFT OUTER JOIN [dbo].[ActivityImportance] AI ON E.ActivityImportanceId=AI.Id
                                       Where E.EmployeeId='" + employeeId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetCbo(string employeeId)
        {
            try
            {
                var sql = @"Select A.Id AS [Value], A.Name AS [Text] From dbo.ActivityEmp AS A Where A.EmployeeId='" + employeeId + "'";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetKPICbo(string employeeId)
        {
            try
            {
                var sql = @"Select A.Id AS [Value], A.Name AS [Text] From dbo.ActivityEmp AS A Where A.EmployeeId='" + employeeId + "'";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public void Delete(string id)
        {
            try
            {
                var documentData = _documentActivityService.Query(t => t.ActivityId == id).Select().ToList();
                var kpiData = _kpiService.Query(t => t.ActivityId == id).Select().ToList();
                var msg = "";
                if (documentData != null && documentData.Count() > 0)
                {
                    msg += documentData.Count() + " Document ";
                    throw new CustomException("First Delete '" + msg + "'");
                }

                if (kpiData != null && kpiData.Count() > 0)
                {
                    msg += "'" + kpiData.Count() + "KPI ";
                    throw new CustomException("First Delete '" + msg + "'");
                }

                //DelMaster(id, out from_db);
                ActivityEmp from_db = Find(id);
                if (from_db == null || string.IsNullOrEmpty(from_db.Id))
                    throw new Exception("The record no longer exists");
                base.Delete(from_db);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelMaster(string id, out ActivityEmp from_db)
        {
            from_db = null;
            try
            {
                from_db = GetMaster(id);

                if (from_db.Id == null || from_db.Id == "")
                    throw new Exception("No Activity found against Id: [" + id + "]");
                else
                    from_db.ModelState = ModelState.Deleted;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActivityEmp GetMaster(string PK)//TBT
        {
            try
            {
                string _sql = "select * from dbo.ActivityEmp where Id='" + PK + "'";
                return _activityRepository.SelectQuery(_sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Operation
    }
}
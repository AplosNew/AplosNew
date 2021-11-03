#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
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

namespace Library.Service.Employees
{
    public class SOPActivityService : Service<SOPActivity>, ISOPActivityService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISOPActivityDocumentService _documentActivityService;
        private readonly ISOPActivityKPIService _kpiService;
        private readonly IRepositoryAsync<SOPActivity> _activityRepository;
        private readonly IRepositoryAsync<SOPActivityDocument> _activityDocRepository;

        public SOPActivityService(
            IRepositoryAsync<SOPActivity> activityRepository,
            IRepositoryAsync<SOPActivityDocument> activityDocRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ISOPActivityDocumentService documentActivityService
            , ISOPActivityKPIService kpiService
            , ISqlRepository sqlRepository
            ) : base(activityRepository, unitOfWork, pkGeneratorService)
        {
            _activityRepository = activityRepository;
            _activityDocRepository = activityDocRepository;
            _unitOfWork = unitOfWork;
            _documentActivityService = documentActivityService;
            _kpiService = kpiService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Operation

        private void Check(SOPActivity entity)
        {
            var ck = base.Query(a => a.Id != entity.Id && a.Name == entity.Name).Select().FirstOrDefault();
            if (ck != null && ck.Name == entity.Name)
            {
                throw new CustomException("[" + entity.Name + "] already exists.");
            }
        }

        public void InsertOrUpdate(SOPActivity entity)
        {
            try
            {
                if (entity != null)
                {
                    Check(entity);
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = GetAutoNumber(nameof(SOPActivity), PKGeneratorEnum.Auto, null, DateTime.Now);
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        entity.AddedBy = identity.EmployeeId;
                        entity.AddedFromIP = identity.IPAddress;
                        entity.AddedDate = DateTime.Now;
                        Insert(entity);
                    }
                    else
                    {
                        var dbdata = Find(entity.Id);
                        if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))

                            throw new CustomException("The record no longer exists.");
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        entity.UpdatedBy = identity.EmployeeId;
                        entity.UpdatedFromIP = identity.IPAddress;
                        entity.UpdatedDate = DateTime.Now;
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

        public void InsertOrUpdateDocument(IEnumerable<SOPActivityDocument> entities)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(SOPActivityDocument), PKGeneratorEnum.Auto, null, DateTime.Now);
                //Check(entities);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.AddedBy = identity.EmployeeId;
                        item.AddedFromIP = identity.IPAddress;
                        item.AddedDate = DateTime.Now;
                        _documentActivityService.InsertGraph(item);
                    }
                }
                string sopActivityId = entities.First().SOPActivityId;
                string sopDocumentId = entities.First().SOPDocumentId;
                IEnumerable<SOPActivityDocument> dbList = _activityDocRepository.Query(r => r.SOPActivityId == sopActivityId).Select();
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
                            if (!entities.Any(t => t.Id == item.Id && t.SOPActivityId == item.SOPActivityId && t.SOPDocumentId == item.SOPDocumentId))
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

        public string GetPk(SOPActivityDocument entity)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.Id))
                {
                    return entity.SOPActivityId + "-" + _documentActivityService.GetAutoNumber(nameof(SOPActivityDocument), PKGeneratorEnum.Auto, null, DateTime.Now);
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

        public void InsertOrUpdateKPI(SOPActivityKPI entity)
        {
            var flag = false;
            try
            {
                flag = true;
                _unitOfWork.BeginTransaction();

                if (entity == null)
                    throw new CustomException("Incomplete data.");
                var dbdata = Find(entity.SOPActivityId);
                if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))
                    throw new CustomException("The record no longer exists.");

                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = _kpiService.GetAutoNumber(nameof(SOPActivityKPI), PKGeneratorEnum.Auto, null, DateTime.Now);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    entity.AddedBy = identity.EmployeeId;
                    entity.AddedFromIP = identity.IPAddress;
                    entity.AddedDate = DateTime.Now;
                    _kpiService.InsertGraph(entity);
                }
                else
                {
                    var db_data = _kpiService.Find(entity.Id);
                    if (db_data == null || string.IsNullOrEmpty(db_data.Id))
                        throw new CustomException("The record no longer exists.");
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    entity.UpdatedBy = identity.EmployeeId;
                    entity.UpdatedFromIP = identity.IPAddress;
                    entity.AddedDate = DateTime.Now;
                    _kpiService.UpdateGraph(entity);
                }
                SOPActivity activity = Find(entity.SOPActivityId);
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

        public IEnumerable<object> Query(string sopItemId)

        {
            try
            {
                 string CmdText = @"SELECT E.*,P.UserName AS PositionName FROM HKP.SOPActivity E
                                       LEFT OUTER JOIN ORG.Position AS P ON P.Id=E.PositionId
                                       Where E.SOPItemId='" + sopItemId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetCbo(string sopItemId)
        {
            try
            {
                var sql = @"Select A.Id AS [Value], A.Name AS [Text] From HKP.SOPActivity AS A Where A.SOPItemId='" + sopItemId + "'";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetKPICbo(string sopItemId)
        {
            try
            {
                var sql = @"Select A.Id AS [Value], A.Name AS [Text] From HKP.SOPActivity AS A Where A.SOPItemId='" + sopItemId + "'";

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
                var documentData = _documentActivityService.Query(t => t.SOPActivityId == id).Select().ToList();
                var kpiData = _kpiService.Query(t => t.SOPActivityId == id).Select().ToList();
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
                SOPActivity from_db = Find(id);
                if (from_db == null || string.IsNullOrEmpty(from_db.Id))
                    throw new Exception("The record no longer exists");
                base.Delete(from_db);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelMaster(string id, out SOPActivity from_db)
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

        public SOPActivity GetMaster(string PK)//TBT
        {
            try
            {
                string _sql = "select * from dbo.SOPActivity where Id='" + PK + "'";
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
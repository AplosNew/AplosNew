#region Using

using Library.Core;
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

#endregion Using

namespace Library.Service.Employees
{
    public class RouteStoppageService : Service<RouteStoppage>, IRouteStoppageService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RouteStoppageService(
            IRepositoryAsync<RouteStoppage> routeStoppageRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(routeStoppageRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        public void InsertOrUpdateGraph(IEnumerable<RouteStoppage> routeStoppageList, string routeId, out List<RouteStoppage> routeStoppageDb_list)
        {
            try
            {
                routeStoppageDb_list = base.Query(r => r.RouteId == routeId).Select().ToList<RouteStoppage>();
                if (routeStoppageDb_list == null)
                {
                    routeStoppageDb_list = new List<RouteStoppage>();
                }

                foreach (var item in routeStoppageDb_list)
                {
                    var db = routeStoppageList.Where(a => a.Id == item.Id).FirstOrDefault();
                    if (db == null || string.IsNullOrEmpty(db.Id))
                    {
                        item.ModelState = ModelState.Deleted;
                        AuditService.Log(item);
                    }
                }

                var pk = GetAutoNumber(nameof(RouteStoppage), PKGeneratorEnum.Auto, null, DateTime.Now);
                var count = 0;
                foreach (RouteStoppage routeStoppage in routeStoppageList)
                {
                    var routeStoppageDb = routeStoppageDb_list.FirstOrDefault(r => r.Id == routeStoppage.Id);
                    if (routeStoppageDb == null || string.IsNullOrEmpty(routeStoppageDb.Id))
                    {
                        count++;
                        routeStoppageDb = new RouteStoppage
                        {
                            Id = pk + "-" + count,
                            RouteId = routeId,
                            StoppageId = routeStoppage.StoppageId,
                            UpInTime = routeStoppage.UpInTime,
                            UpDuration = routeStoppage.UpDuration,
                            DownInTime = routeStoppage.DownInTime,
                            DownDuration = routeStoppage.DownDuration,
                            Sequence = routeStoppage.Sequence,
                            ModelState = ModelState.Added
                        };
                        AuditService.AddedLog(routeStoppageDb);
                        routeStoppageDb_list.Add(routeStoppageDb);
                    }
                    else
                    {
                        routeStoppageDb.UpInTime = routeStoppage.UpInTime;
                        routeStoppageDb.UpDuration = routeStoppage.UpDuration;
                        routeStoppageDb.DownInTime = routeStoppage.DownInTime;
                        routeStoppageDb.DownDuration = routeStoppage.DownDuration;
                        routeStoppageDb.Sequence = routeStoppage.Sequence;
                        routeStoppageDb.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(routeStoppageDb);
                    }
                }

                //_unitOfWork.SaveChanges();
                //flag = false;
                //_unitOfWork.Commit();
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
            //finally
            //{
            //    if (flag)
            //    {
            //        _unitOfWork.Rollback();
            //    }
            //}
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM [MST].[RouteStoppage]";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            throw new NotImplementedException();
        }

        public void DeleteGraph(string routeId)
        {
            var flag = false;
            try
            {
                var data = base.Query(t => t.RouteId == routeId).Select().ToList();

                if (data != null && data.Count() > 0)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    foreach (var item in data)
                    {
                        base.DeleteGraph(item);
                    }
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
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
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }
    }
}
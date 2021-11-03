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
    public class RouteService : Service<Route>, IRouteService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRouteStoppageService _routeStoppageService;
        private readonly IUnitOfWork _unitOfWork;

        public RouteService(
            IRepositoryAsync<Route> RouteRepository
            , IRouteStoppageService routeStoppageService
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(RouteRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _routeStoppageService = routeStoppageService;
            _unitOfWork = unitOfWork;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(Route), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(Route entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }

        public void Insert(Route entity, IEnumerable<RouteStoppage> routeStoppages)
        {
            var flag = false;
            List<RouteStoppage> routeStoppageDb_list = null;
            try
            {
                Check(entity);
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    entity.ModelState = ModelState.Added;
                    AuditService.AddedLog(entity);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(entity);
                }
                InsertOrUpdateGraph(entity);
                _routeStoppageService.InsertOrUpdateGraph(routeStoppages, entity.Id, out routeStoppageDb_list);
                foreach (var item in routeStoppageDb_list)
                {
                    _routeStoppageService.InsertOrUpdateGraph(item);
                }

                _unitOfWork.BeginTransaction();
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel GetAllStoppage(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"select s.*,c.UserName as City from HKP.Stoppage as s
                                        left outer join SCS.City as c on s.CityId = c.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetAllDriver(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //      parameters.CmdText = @"SELECT EMP.*,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName
                //,DEG.UserName GivenDesignation,DEPT.UserName Department,
                //                           '' UpInTime,0 UpDuration,'' DownInTime,0 DownDuration
                //FROM EmployeeInformation EMP
                //LEFT OUTER JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                //LEFT OUTER JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                //LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id
                //LEFT OUTER JOIN HKP.Designation D ON PR.DesignationId=D.Id
                //LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                //LEFT OUTER JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                //where EMP.GroupID='" + identity.CompanyGroupId + "' and EMP.CompanyId='" + identity.CompanyId + "'";

                parameters.CmdText = @"SELECT EMP.*
                                    	,E.UserName EntityName
                                        , D.UserName Designation
                                         , PR.UserName PositionName
                                          , DEG.UserName GivenDesignation
                                           , DEPT.UserName Department
                                            ,'' UpInTime
                                    	,0 UpDuration
                                    	,'' DownInTime
                                    	,0 DownDuration
                                    FROM EmployeeInformation EMP
                                    LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                    LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                                    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
                                    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                                    LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId = DEG.Id
                                    WHERE EMP.GroupID = '" + identity.CompanyGroupId + "' AND EMP.CompanyId = '" + identity.CompanyId + "'  AND DEG.UserName = 'Driver'";
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
            try
            {
                return from m in base.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //public IEnumerable<object> GetRouteStoppageData(string routeId)
        //{
        //    try
        //    {
        //        var sql = @"select RS.*,S.UserName,C.UserName as City from MST.RouteStoppage RS
        //                    left outer join HKP.Stoppage as S on RS.StoppageId=S.Id
        //                    left outer join SCS.City as C on S.CityId=C.Id
        //                    where RS.RouteId='" + routeId + "'order by Sequence ";
        //        return _sqlRepository.GetDataCollection(sql, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}

    }
}
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
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class RouteEmployeeService : Service<RouteEmployee>, IRouteEmployeeService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RouteEmployeeService(
            IRepositoryAsync<RouteEmployee> routeEmployeeRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(routeEmployeeRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(RouteEmployee), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public GridModel Query(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"select R.UserName as Route from [MST].[Route] as R
								left outer join ORG.Plant as P on R.PlantId = p.Id
								where R.PlantId = '" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetSavedData(string plantId, string routeId)
        {
            try
            {
                var sql = @"SELECT  RE.Id,Replace(CONVERT(VARCHAR(11), RE.EffectiveDate, 106), ' ', '-') EffectiveDate
							,RE.RouteId,RE.EmployeeId,RE.PlantId,RE.PickStoppageId,RE.DropStoppageId
							,EMP.EmployeeCode,EMP.EmployeeName
							,EMP.DOJ,D.UserName Designation,DEPT.UserName Department,EMP.CellPhnNo
        					 FROM TRN.RouteEmployee RE
							 LEFT OUTER JOIN dbo.EmployeeInformation EMP on RE.EmployeeId=EMP.SystemId
        					 LEFT OUTER JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
        					 LEFT OUTER JOIN ORG.Position PR ON PMB.PositionId=PR.Id
        					 LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id
        					 LEFT OUTER JOIN HKP.Designation D ON PR.DesignationId=D.Id
        					 LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
        					 LEFT OUTER JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
        					 WHERE RE.PlantId='" + plantId + "' and RE.RouteId='" + routeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetAllEmployee(GridParameter parameters, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT 0 Flag, EMP.*,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName
        					 ,DEG.UserName GivenDesignation,DEPT.UserName Department
        					 FROM EmployeeInformation EMP
        					 LEFT OUTER JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
        					 LEFT OUTER JOIN ORG.Position PR ON PMB.PositionId=PR.Id
        					 LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id
        					 LEFT OUTER JOIN HKP.Designation D ON PR.DesignationId=D.Id
        					 LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
        					 LEFT OUTER JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
        					 where EMP.GroupID='" + identity.CompanyGroupId + "' and EMP.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertOrUpdateGraph(IEnumerable<RouteEmployee> uilist, string plantId, string routeId)
        {
            var flag = false;
            try
            {
                ValidationEmpInRoute(uilist, plantId, routeId);
                List<RouteEmployee> dbList = base.Query(r => r.RouteId == routeId).Select().ToList();
                if (dbList == null)
                    dbList = new List<RouteEmployee>();

                foreach (var item in dbList)
                {
                    var db = uilist.Where(a => a.Id == item.Id).FirstOrDefault();
                    //if (db == null || string.IsNullOrEmpty(db.Id))
                    if (db == null)
                    {
                        item.ModelState = ModelState.Deleted;
                        AuditService.Log(item);
                    }
                }

                var pk = GetAutoNumber(nameof(RouteEmployee), PKGeneratorEnum.Auto, null, DateTime.Now);
                var count = 0;
                foreach (RouteEmployee routeEmployee in uilist)
                {
                    var routeEmployeeDb = dbList.FirstOrDefault(r => r.Id == routeEmployee.Id);
                    if (routeEmployeeDb == null || string.IsNullOrEmpty(routeEmployeeDb.Id))
                    {
                        count++;
                        routeEmployeeDb = new RouteEmployee
                        {
                            Id = pk + "-" + count,
                            RouteId = routeEmployee.RouteId,
                            EmployeeId = routeEmployee.EmployeeId,
                            PlantId = routeEmployee.PlantId,
                            EffectiveDate = routeEmployee.EffectiveDate,
                            PickStoppageId = routeEmployee.PickStoppageId,
                            DropStoppageId = routeEmployee.DropStoppageId,
                            ModelState = ModelState.Added
                        };
                        AuditService.AddedLog(routeEmployeeDb);
                        dbList.Add(routeEmployeeDb);
                    }
                    else
                    {
                        routeEmployeeDb.RouteId = routeEmployee.RouteId;
                        routeEmployeeDb.EmployeeId = routeEmployee.EmployeeId;
                        routeEmployeeDb.PlantId = routeEmployee.PlantId;
                        routeEmployeeDb.EffectiveDate = routeEmployee.EffectiveDate;
                        routeEmployeeDb.PickStoppageId = routeEmployee.PickStoppageId;
                        routeEmployeeDb.DropStoppageId = routeEmployee.DropStoppageId;
                        routeEmployeeDb.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(routeEmployeeDb);
                    }
                }
                foreach (var item in dbList)
                {
                    base.InsertOrUpdateGraph(item);
                }
                _unitOfWork.BeginTransaction();
                flag = true;
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
                {
                    _unitOfWork.Rollback();
                }
            }
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

        private void ValidationEmpInRoute(IEnumerable<RouteEmployee> uilist, string plantId, string routeId)
        {
            try
            {
                DataSet ds = GetEmpDataset(routeId, plantId);
                foreach (var item in uilist)
                {
                    DataView dv = new DataView(ds.Tables[0])
                    {
                        RowFilter = "EmployeeId='" + item.EmployeeId + "' and Id<>'" + item.Id + "'"
                    };
                    if (dv.Count > 0)
                    {
                        //throw new Exception("Employee [" + item.EmployeeName + "] has already been added in this route");
                        throw new Exception("Employee [" + item.EmployeeId + "] has already been added in this route");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet GetEmpDataset(string routeId, string plantId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                //string _sql = @"SELECT R.*,E.EmployeeName
                //               FROM TRN.RouteEmployee R
                //               LEFT JOIN dbo.EmployeeInformation E ON R.EmployeeId = E.SystemId
                //               WHERE R.RouteId='" + routeId + "'  and R.PlantId='" + plantId + "'";

                parameters.CmdText = @"SELECT * FROM [TRN].[RouteEmployee] WHERE RouteId='" + routeId + "' AND PlantId='" + plantId + "'";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
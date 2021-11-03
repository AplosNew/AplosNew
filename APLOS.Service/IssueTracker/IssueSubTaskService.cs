#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.IssueTracker
{
    public class IssueSubTaskService : Service<IssueSubTask>, IIssueSubTaskService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public IssueSubTaskService(
            IRepositoryAsync<IssueSubTask> IssueSubTaskRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(IssueSubTaskRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor

        //public decimal GetAutoSequence()
        //{
        //    try
        //    {
        //        return base.Query().Select().Max(r => r.Sequence + 1);
        //    }
        //    catch
        //    {
        //        return 1.00M;
        //    }
        //}
        
        public string GetPK()
        {
            return GetAutoNumber(nameof(IssueSubTask), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        //private void Check(IssueSubTask entity)
        //{
        //    CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
        //    CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        //}
        public override void Insert(IssueSubTask entity)
        {
            try
            {
                //Check(entity);
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(IssueSubTask entity)
        {
            try
            {
                //Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public void UpdateSubTask(List<TaskManagerSubTasks> SubTasks)
        {
            try
            {
               
                DataSet dsData;
                string sql = "Select * from TaskManagerSubTask where TaskManagerMasterId='" + SubTasks[0].TaskManagerMasterId + "'";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsData, false, "1");

                for (int i = 0; i < SubTasks.Count; i++)
                {
                    dsData.Tables[0].DefaultView.RowFilter = "Id='"+ SubTasks[i].Id + "'";
                    if (dsData.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = dsData.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["IsDone"] = SubTasks[i].IsDone;
                        dr.EndEdit();
                    }

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsData);
            }
            catch (Exception)
            {

                throw;
            }


        }

        

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT ist.*
                                        ,its.Issue
                                        ,emp.EmployeeName

                                         FROM [dbo].[IssueSubTask] AS ist
                                        LEFT JOIN [dbo].[IssueTransaction] AS its
                                        ON its.Id = ist.IssueTransactionId
                                        LEFT JOIN [dbo].[EmployeeInformation] AS emp
                                        ON emp.SystemId = ist.ResponsiblePersonId ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        
        public void Delete(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                base.Delete(Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public List<Dictionary<string, object>> GetSubTaskByIssueTransactionId(string issueTransactionId)
        {
            try
            {
                var sql = @"SELECT * FROM [dbo].[IssueSubTask] WHERE IssueTransactionId ='" + issueTransactionId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Logs.ToString()));
            }
        }


    }
}
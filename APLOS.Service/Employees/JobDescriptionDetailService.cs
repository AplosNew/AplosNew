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
    public class JobDescriptionDetailService : Service<JobDescriptionDetail>, IJobDescriptionDetailService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public JobDescriptionDetailService(
            IRepositoryAsync<JobDescriptionDetail> JobDescriptionDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(JobDescriptionDetailRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void DeleteGraphByJobDescription(string jobDescriptionId)
        {
            var db_List = base.Query(t => t.JobDescriptionId == jobDescriptionId).Select(t => t.Id).ToList();
            if (null != db_List)
            {
                foreach (var item in db_List)
                {
                    DeleteGraph(item);
                }
            }
        }

        public void InsertGraph(IEnumerable<JobDescriptionDetail> entities, string jobDescriptionId)
        {
            try
            {
                if (entities != null)
                {
                    string id = CreatePk(jobDescriptionId);
                    var count = id.ToInt();
                    foreach (var item in entities)
                    {
                        item.Id = jobDescriptionId + "-" + count; count++;
                        item.FileId = item.Id;
                        item.JobDescriptionId = jobDescriptionId;
                        base.InsertGraph(item);
                    }
                }
                //else
                //    throw new CustomException("Please select at least one attachment...........!");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private string CreatePk(string jobDescriptionId)
        {
            try
            {
                string id = string.Empty;
                var Db_Pk = base.Query(t => t.JobDescriptionId == jobDescriptionId).Select(t => t.Id).AsEnumerable();
                if (Db_Pk.Count() != 0)
                {
                    id = Db_Pk.Max(x => Convert.ToInt32(x.Substring(jobDescriptionId.Length + 1)) + 1).ToString();
                }
                else
                {
                    id = "1";
                }
                return id;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateGraph(IEnumerable<JobDescriptionDetail> entities, string jobDescriptionId)
        {
            try
            {
                if (jobDescriptionId != null)
                {
                    string id = CreatePk(jobDescriptionId);
                    var count = id.ToInt();
                    foreach (var item in entities)
                    {
                        item.Id = jobDescriptionId + "-" + count;
                        item.JobDescriptionId = jobDescriptionId;
                        count++;
                        base.InsertGraph(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string jobDescriptionId)
        {
            try
            {
                parameters.CmdText = $"SELECT JDD.Id, JDD.FileName AS 'name', JDD.FileName, JDD.FileId, JDD.JobDescriptionId FROM [HKP].[JobDescriptionDetail] AS JDD" +
                    $" WHERE JDD.JobDescriptionId='{jobDescriptionId}' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
    }
}
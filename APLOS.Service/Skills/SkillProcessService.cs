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
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Skills
{
    public class SkillProcessService : Service<SkillProcess>, ISkillProcessService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        //private readonly IMaterialMasterMachineProcessService _machineTypeProcessService;
        private readonly IRepositoryAsync<SkillProcess> _skillProcessRepository;

        public SkillProcessService(
            IRepositoryAsync<SkillProcess> skillProcessRepository,
            IPKGeneratorService pkGeneratorService,
            //IMaterialMasterMachineProcessService machineTypeProcessService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(skillProcessRepository, unitOfWork, pkGeneratorService)
        {
            //_machineTypeProcessService = machineTypeProcessService;
            _sqlRepository = sqlRepository;
            _skillProcessRepository = skillProcessRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string skillId)
        {
            try
            {
                parameters.CmdText = @"SELECT SP.Id
                                                ,P.[Sequence]
                                                ,P.Code
                                                ,P.UserName
                                                ,P.StandardName
                                                ,P.ShortName
                                                ,MT.[Description] AS MaterialType
                                                ,P.Active
                                                ,SP.SkillId
                                                ,SP.ProcessId
                                                ,CAST(0 as BIT) AS Archive
                                        FROM HKP.SkillProcess AS SP
                                        LEFT OUTER JOIN HKP.Process AS P ON SP.ProcessId=P.Id
                                        LEFT OUTER JOIN HKP.MaterialType AS MT ON P.MaterialTypeId=MT.Id
                                        WHERE SP.SkillId='" + skillId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void InsertUpdateOrDeleteGraph(string skillId, IEnumerable<SkillProcess> entities)
        {
            try
            {
                var dbList = base.Query(t => t.SkillId == skillId).Select().ToList();
                if (entities != null)
                {
                    string id = CreatePk(skillId);
                    var count = id.ToInt();
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = skillId + "-" + count;
                            item.SkillId = skillId;
                            count++;
                            InsertGraph(item);
                        }
                    }
                }
                if (dbList != null)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            //if (CheckProcessIdUseInMachineType(new[] { item.ProcessId }, skillId))
                            //    throw new CustomException(string.Format(ServiceResources.AlreadyExistAnotherTableNotUpOrDel, "process", "machine type", "delete this!"));
                            //if (CheckProcessIdUseInOperation(new[] { item.ProcessId }, skillId))
                            //    throw new CustomException(string.Format(ServiceResources.AlreadyExistAnotherTableNotUpOrDel, "process", "operation", "delete this!"));
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                //if (CheckProcessIdUseInMachineType(new[] { item.ProcessId }, skillId))
                                //    throw new CustomException(string.Format(ServiceResources.AlreadyExistAnotherTableNotUpOrDel, "process", "machine type", "delete this!"));
                                //if (CheckProcessIdUseInOperation(new[] { item.ProcessId }, skillId))
                                //    throw new CustomException(string.Format(ServiceResources.AlreadyExistAnotherTableNotUpOrDel, "process", "operation", "delete this!"));
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        private string CreatePk(string skillId)
        {
            try
            {
                string id = string.Empty;
                var Db_Pk = base.Query(t => t.SkillId == skillId).Select(t => t.Id).AsEnumerable();
                if (Db_Pk.Count() != 0)
                    id = Db_Pk.Max(x => Convert.ToInt32(x.Substring(skillId.Length + 1)) + 1).ToString();
                else
                    id = "1";
                return id;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void DeleteGraph(string skillId)
        {
            var data_Db = base.Query(r => r.SkillId == skillId).Select().AsEnumerable();
            if (data_Db != null)
            {
                //TODO: Check
                //var data_DbProcessIds = data_Db.Select(t => t.ProcessId);
                //string[] machineTypeProcess = _machineTypeProcessService.Query(t => data_DbProcessIds.Contains(t.ProcessId)
                //                                && t.SkillId == skillId).Select(t => t.ProcessId).ToArray();
                //CheckProcessIdUseInOperation(machineTypeProcess, skillId);
                //CheckProcessIdUseInMachineType(machineTypeProcess, skillId);
                foreach (var item in data_Db)
                {
                    base.DeleteGraph(item);
                }
            }
        }

        /// <summary>
        /// if process id use in operation then process or skill can not delete.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="processIds">todo: describe processIds parameter on CheckProcessIdUseInOperation</param>
        /// <param name="skillId">todo: describe skillId parameter on CheckProcessIdUseInOperation</param>
        private bool CheckProcessIdUseInOperation(string[] processIds, string skillId)
        {
            try
            {
                string sql = @"IF EXISTS(SELECT 1 FROM(
                                                SELECT A.CheckingColumn,A.CheckingColumn2 FROM
                                                (SELECT OperationId,ProcessId AS CheckingColumn,SkillId AS CheckingColumn2 FROM MST.OperationAssetItem ) AS A
                                                ) AA WHERE CheckingColumn IN (" + ReturnStringArray(processIds) + ") AND CheckingColumn2='" + skillId + "') SELECT 1 ELSE SELECT 0 RETURN ";
                return Convert.ToBoolean(_skillProcessRepository.SqlQuery<int>(sql).Single());
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// if process id use in process then process or skill can not delete.
        /// </summary>
        /// <param name="processId"></param>
        private bool CheckProcessIdUseInMachineType(string[] processIds, string skillId)
        {
            try
            {
                return false;
                //string sql = @"IF EXISTS(SELECT 1 FROM(
                //                    SELECT ProcessId AS CheckingColumn,SkillId AS CheckingColumn2 FROM MST.AssetItemProcess
                //                    ) A WHERE CheckingColumn IN (" + ReturnStringArray(processIds) + ") AND CheckingColumn2='" + skillId + "') SELECT 1 ELSE SELECT 0 RETURN ";
                //return Convert.ToBoolean(_skillProcessRepository.SqlQuery<int>(sql).Single());
            }
            catch
            {
                throw;
            }
        }
    }
}
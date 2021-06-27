#region Using

using Library.Core;
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
    public partial class OperationFgComponentService : Service<OperationFgComponent>, IOperationFgComponentService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<OperationFgComponent> _fgComponentRepository;

        public OperationFgComponentService(
            IRepositoryAsync<OperationFgComponent> fgComponentRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(fgComponentRepository, unitOfWork)
        {
            _fgComponentRepository = fgComponentRepository;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> Query(string operationId)
        {
            try
            {
                var sql = @"SELECT OPFGC.Id
                                          ,OPFGC.OperationId
                                          ,OPFGC.FGComponentId
                                          ,FGC.[Sequence]
	                                      ,FGC.Code
	                                      ,FGC.ShortName
	                                      ,FGC.StandardName
	                                      ,FGC.UserName
	                                      ,FGC.Active
	                                      ,CAST(0 as BIT) AS Archive
                                    FROM MST.OperationFgComponent AS OPFGC
                                    LEFT OUTER JOIN HKP.FGComponent AS FGC ON OPFGC.FGComponentId=FGC.Id
                                    WHERE OPFGC.OperationId='" + operationId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertUpdateOrDeleteGraph(string operationId, IEnumerable<OperationFgComponent> entities)
        {
            try
            {
                if (entities != null)
                {
                    var count = _fgComponentRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [MST].[OperationFgComponent] WHERE OperationId='{operationId}'").First();
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.Id = MakePK(operationId, count, 2);
                            item.OperationId = operationId;
                            InsertGraph(item);
                        }
                    }
                }
                var dbList = base.Query(t => t.OperationId == operationId).Select().ToList();
                if (dbList != null)
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
                            {
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private string CreatePk(string operationId)
        {
            try
            {
                string id = string.Empty;
                var Db_Pk = base.Query(t => t.OperationId == operationId).Select(t => t.Id).AsEnumerable();
                if (Db_Pk.Count() != 0)
                    id = Db_Pk.Max(x => Convert.ToInt32(x.Substring(operationId.Length + 1)) + 1).ToString();
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

        public void DeleteGraph(string operationId)
        {
            try
            {
                var db_Data = base.Query(t => t.OperationId == operationId).Select().AsEnumerable();
                if (db_Data != null)
                {
                    foreach (var item in db_Data)
                    {
                        base.DeleteGraph(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }
    }
}
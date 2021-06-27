#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Productions
{
    public class ProcessCapacityUOMService : Service<ProcessCapacityUOM>, IProcessCapacityUOMService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ProcessCapacityUOMService(
            IRepositoryAsync<ProcessCapacityUOM> ProcessCapacityUOMRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(ProcessCapacityUOMRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> Query(string plantId)
        {
            try
            {
                string _sql = @"SELECT PS.Id
		                                ,PS.PlantId
                                        ,PS.ProcessId
                                        ,PRS.UserName AS ProcessName
                                        ,PS.CapacityUOMId
		                                ,UOMC.UserName AS CapacityUOM
                                        ,PS.UOM1Id
		                                ,UOM1.UserName AS UOM1
                                        ,PS.UOM2Id
		                                ,UOM2.UserName AS UOM2
		                                ,CAST(0 as BIT) AS Archive
                                FROM SCS.ProcessCapacityUOM AS PS
                                LEFT OUTER JOIN HKP.Process AS PRS ON PS.ProcessId=PRS.Id
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOMC ON PS.CapacityUOMId=UOMC.Id
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM1 ON PS.UOM1Id=UOM1.Id
                                LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM2 ON PS.UOM2Id=UOM2.Id
                                WHERE PS.PlantId='" + plantId + "' ORDER BY PRS.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void InsertUpdateOrDeleteGraph(IEnumerable<ProcessCapacityUOM> entity, string plantId)
        {
            try
            {
                if (entity != null)
                {
                    string id = CreatePk(plantId);
                    var count = id.ToInt();
                    foreach (var item in entity)
                    {
                        if (string.IsNullOrEmpty(item.Id) && !item.Archive)
                        {
                            item.Id = plantId + "-" + count;
                            count++;
                            InsertGraph(item);
                        }
                        else if (!string.IsNullOrEmpty(item.Id) && item.Archive)
                        {
                            base.DeleteGraph(item);
                        }
                        else
                        {
                            UpdateGraph(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void DeleteGraph(string plantId)
        {
            try
            {
                IEnumerable<ProcessCapacityUOM> entity = base.Query(t => t.PlantId == plantId).Select();
                foreach (var item in entity)
                {
                    base.DeleteGraph(item);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
        }

        private string CreatePk(string plantId)
        {
            try
            {
                string id = string.Empty;
                var Db_Pk = base.Query(t => t.PlantId == plantId).Select(t => t.Id).AsEnumerable();
                if (Db_Pk.Count() != 0)
                {
                    id = Db_Pk.Max(x => Convert.ToInt32(x.Substring(plantId.Length + 1)) + 1).ToString();
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }
    }
}
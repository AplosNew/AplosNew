#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Enums;
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

namespace Library.Service.WorkCenters
{
    public partial class WorkStationDailyService : Service<WorkStationDaily>, IWorkStationDailyService
    {
        private string tWorkStationDaily = " " + DbSchema.Transaction + ".[WorkStationDaily] ";

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;//
        private readonly IRepositoryAsync<WorkStationDaily> _workStationDailyRepository;

        public WorkStationDailyService(
            IRepositoryAsync<WorkStationDaily> workStationDailyRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(workStationDailyRepository, unitOfWork, pkGeneratorService)
        {
            _workStationDailyRepository = workStationDailyRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetWorkStation(string entityId, string workcenterId)
        {
            try
            {
                var sql = @"Select Id  From [TRN].[WorkStationDaily] WSD Where WSD.EntityId ='" + entityId + "' AND WSD.WorkCenterId='" + workcenterId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetPk()
        {
            try
            {
                var pk = GetMaxNumber(nameof(WorkStationDaily), PKGeneratorEnum.Auto, null, DateTime.Now);
                pk.MaxNumber++;
                return pk.MaxNumber.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CheckEmployee(WorkStationDaily entity)
        {
            var data = Query(t => t.Id != entity.Id && t.EntryDate == entity.EntryDate && t.EmployeeId == entity.EmployeeId).Select().FirstOrDefault();
            if (data != null)
                throw new CustomException("This Employee [" + entity.EmployeeId + "] is  already exist in this Work Center [" + entity.WorkCenterId + "]!");
        }

        private void CheckWorkStation(WorkStationDaily entity)
        {
            var wsdata = Query(t => t.Id != entity.Id && t.WorkCenterId == entity.WorkCenterId && t.WorkStation == entity.WorkStation).Select().FirstOrDefault();
            if (wsdata != null)
                throw new CustomException("This Work Station is  already exist in this Work Center!");
        }

        public override void Insert(WorkStationDaily entity)
        {
            try
            {
                CheckEmployee(entity);
                CheckWorkStation(entity);
                entity.Id = GetPk();
                base.Insert(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.WorkCenter.ToString()));
            }
        }

        public override void Update(WorkStationDaily entity)
        {
            try
            {
                CheckEmployee(entity);
                CheckWorkStation(entity);
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.WorkCenter.ToString()));
            }
        }

        public WorkStationDaily GetMaster(string PK)
        {
            try
            {
                var _sql = "select * from " + tWorkStationDaily + " where Id='" + PK + "'";
                return _workStationDailyRepository.SelectQuery(_sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Delete(string id)
        {
            try
            {
                var entity = base.Find(id);
                base.Delete(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.WorkCenter.ToString()));
            }
        }

        public GridModel GetOperationList(GridParameter parameters, string entityId, string processId)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT O.Id, O.OperationTypeId, O.OperationCategoryId,--O.OperationActionId, 
                                        O.Sequence, O.Code, O.ShortName, O.StandardName, O.UserName,OMT.ProcessId
                                               FROM   MST.Operation O
                                                      INNER JOIN(SELECT * FROM   MST.OperationProcess
                                                      WHERE  ProcessId = (SELECT ProcessId FROM   [HKP].[EntityProcessTag]
                                                      WHERE  EntityId = '" + entityId + @"' And ProcessId =
                                                      (SELECT ProcessId FROM   [SCS].[WorkCenterMaster] Where  Id = '" + processId + @"'))) OMT ON O.Id = OMT.OperationId ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.WorkCenter.ToString()));
            }
        }

        public GridModel GetList(GridParameter parameters, string entityId, string workCenterId, string entryDate)
        {
            try
            {
                parameters.CmdText = @"SELECT WSD.Id, WSD.EntityId,WSD.WorkCenterId,WSD.OperationId,WSD.EmployeeId,WSD.WorkStation, O.UserName
                                    , WSD.ArticleId, FAR.StandardName AS ArticleName
                                    , Replace(CONVERT(VARCHAR(11), WSD.EntryDate, 106), ' ', '-') EntryDate
                                    , E.UserName Entity, WCM.UserName WorkCenter, O.UserName Operation, EM.EmployeeName
                                    FROM TRN.WorkStationDaily WSD
                                    LEFT JOIN ORG.Entity E ON WSD.EntityId=E.Id
                                    LEFT JOIN SCS.WorkCenterMaster WCM ON WSD.WorkCenterId=WCM.Id
                                    LEFT JOIN MST.Operation O ON WSD.OperationId=O.Id
                                    LEFT JOIN dbo.EmployeeInformation EM ON WSD.EmployeeId=EM.SystemId
                                    LEFT JOIN MST.MaterialMasterArticle AS FAR ON WSD.ArticleId=FAR.Id
                                    --LEFT JOIN MST.MaterialMaster AS MM ON FAR.MaterialMasterId=MM.Id
                                    Where WSD.EntityId='" + entityId + "' AND WSD.WorkCenterId='" + workCenterId + "' AND EntryDate='" + entryDate + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.WorkCenter.ToString()));
            }
        }

        public GridModel GetMachineList(GridParameter parameters, string operationId, string processId)
        {
            try
            {
                parameters.CmdText = @"SELECT FAR.Id AS FixedAssetRegisterId, FA.UserName AS FixedAssetMaster --,MM.UserName AS MaterialMasterName, P.UserName AS VendorName
	                                     , FAR.LifeTime, FAR.Model, FAR.SerialNo, FAR.InvoiceNo, FAR.InvoiceDate
                                    FROM TRN.FixedAssetRegister AS FAR
                                    INNER JOIN MST.MaterialMaster AS MM  ON FAR.MaterialMasterId=MM.Id
                                    INNER JOIN MST.MaterialMasterArticle AS ART ON FAR.MaterialMasterId=MM.Id AND FAR.MaterialMasterArticleId=ART.Id
                                    INNER JOIN MST.OperationMachineSkill AS OMS ON OMS.MaterialMasterArticleId=ART.Id
                                    LEFT JOIN MST.FixedAssetMaster AS FA ON FAR.FixedAssetMasterId=FA.Id
                                    LEFT JOIN HKP.Party AS P ON FAR.VendorId=P.Id
                                        WHERE OMS.OperationId='" + operationId + "' AND OMS.ProcessId='" + processId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.WorkCenter.ToString()));
            }
        }
    }
}
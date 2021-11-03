using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.OrderManagements;
using Library.Service.Systems;
using Library.Service.WorkCenters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Productions
{
    public class MainProcessPlanningService : Service<MainProcessPlanning>, IMainProcessPlanningService
    {
        private DataSet prodDtSet;
        private DataSet blockFromDb;
        private DataSet daysFromDb;
        private DataSet calendarFromDb;
        private DataSet wcPreferenceDs;

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IProductionOrderService _productionBatchMasterService;
        private readonly IWorkCenterMasterService _workCenterMasterService;

        public MainProcessPlanningService(
            IRepositoryAsync<MainProcessPlanning> mainProcessPlanningRepository,
            IPKGeneratorService pkGeneratorService,
            IProductionOrderService productionBatchMasterService,
            IWorkCenterMasterService workCenterMasterService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(mainProcessPlanningRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _productionBatchMasterService = productionBatchMasterService;
            _workCenterMasterService = workCenterMasterService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetList(string plantId, DateTime toDate, string companyId, string processId)
        {
            try
            {
                var result = GetAllDataFromMP(plantId, processId);
                return result;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> Process(string plantId, DateTime toDate, string companyId, string processId)
        {
            try
            {
                prodDtSet = GetProdReferenceData(plantId, companyId, toDate, processId);
                blockFromDb = GetBlockData(plantId, toDate, processId);

                var wcWithAdditionalDs = GetWorkCenterWithAdditional(plantId, companyId, processId);
                wcPreferenceDs = GetWorkCenterByLinePreference(plantId, companyId, toDate, processId);
                calendarFromDb = GetWeekendCalendar(plantId);
                var ids = GetIds(blockFromDb.Tables[0]);
                RowDeleteFromMainDt(ref blockFromDb, ids);
                var dtBlock = SetLsdCD(prodDtSet, blockFromDb, ref daysFromDb, wcWithAdditionalDs, toDate, processId);
                SetOffDays(dtBlock, plantId, toDate, calendarFromDb.Tables[0], processId);
                InsertOrUpdateRange(dtBlock, ids);
                var result = GetAllDataFromMP(plantId, processId);
                return result;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private static void RowDeleteFromMainDt(ref DataSet dsBlock, string[] ids)
        {
            try
            {
                var dt = dsBlock.Tables[0];
                foreach (var item in ids)
                {
                    var dv = new DataView(dt) { RowFilter = "Id='" + item + "' AND IsFreeze=0" };
                    if (dv.Count > 0)
                    {
                        dv[0].Row.Delete();
                        dt.AcceptChanges();
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(null, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private static string[] GetIds(DataTable dtBlock)
        {
            try
            {
                string[] ids;
                var rows = dtBlock.Select();
                ids = Array.ConvertAll(rows, row => row["Id"].ToString());
                return ids;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(null, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void InsertOrUpdateRange(DataTable dt, string[] ids)
        {
            var flag = false;
            try
            {
                var dtBlockDv = new DataView(dt) { RowFilter = "LineId<>''" };
                var entities = new List<MainProcessPlanning>();
                entities = ConvertDataTable<MainProcessPlanning>(dtBlockDv.ToTable());
                var pk = GetMaxNumber(nameof(MainProcessPlanning), PKGeneratorEnum.Auto, null, DateTime.Now);

                var dbData = Query(t => ids.Contains(t.Id)).Select().AsEnumerable();
                if (dbData.Count() > 0 || dbData != null)
                {
                    foreach (var delItem in dbData)
                    {
                        DeleteGraph(delItem);
                    }
                }
                foreach (var item in entities)
                {
                    pk.MaxNumber++;
                    item.Id = pk.MaxNumber.ToString();
                    if (string.IsNullOrEmpty(item.EntityId))
                        item.EntityId = null;
                    if (string.IsNullOrEmpty(item.OurStyleId))
                        item.OurStyleId = null;
                    if (string.IsNullOrEmpty(item.ProductionBatchMasterId))
                        item.ProductionBatchMasterId = null;
                    InsertGraph(item);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private IEnumerable<object> GetAllDataFromMP(string plantId, string processId)
        {
            try
            {
                var _sql = @"SELECT MPPB.Id, MPPB.MinAllocatedLine, MPPB.IncrementType, MPPB.IncrementValue, MPPB.StandardTime
	                                , MPPB.DaysToGetTheTarget, MPPB.FirstDayOutPut, MPPB.MinRequiredTargetHourly, MPPB.EntityId
	                                , MPPB.PlantId, MPPB.LineId, WC.Code AS Line, MPPB.ProductionBatchMasterId
	                                , MPPB.OurStyleId, OS.UserName AS OurStyle, MPPB.RunningDay, MPPB.MinWorkingDays
	                                , MPPB.TotalQty, MPPB.DailyOutPut, MPPB.StandardDailyOutPut, MPPB.LearningCurveOutPut
	                                , MPPB.HasLearningCurve, MPPB.IsFreeze, MPPB.OffDayType, MPPB.OffDay, MPPB.Lsd
	                                , MPPB.CommitmentDate, MPPB.[Date], MPPB.[Sequence], BM.ProductionPriority, SO.FileNo
	                                , CAST(1 as BIT) AS IsDb, MPPB.Color, CAST(0 as BIT) AS FontColor, '' AS Msg
                                    , ActualAllocatedLine=
                                        (SELECT  CONVERT(varchar(max), COUNT(DISTINCT P.LineId))+ ' [' + STUFF((SELECT DISTINCT ',' +  WC.Code FROM TRN.MainProcessPlanning AS PL
					                     INNER JOIN SCS.WorkCenterMaster AS WC  ON WC.Id=PL.LineId WHERE PL.OffDay=0 AND PL.ProductionBatchMasterId=MPPB.ProductionBatchMasterId FOR XML PATH('')),1,1,'')+']'
		                                 FROM TRN.MainProcessPlanning AS P
		                                 WHERE P.OffDay=0 AND P.ProductionBatchMasterId=MPPB.ProductionBatchMasterId GROUP BY P.ProductionBatchMasterId)
                                    , QtyVariance=CASE WHEN MPPB.TotalQty <> BM.Qty AND MPPB.OffDay=0 THEN CAST(1 as BIT) ELSE CAST(0 as BIT) END
                           FROM TRN.MainProcessPlanning AS MPPB
                           LEFT OUTER JOIN SCS.WorkCenterMaster AS WC ON MPPB.LineId=WC.Id
                           LEFT OUTER JOIN HKP.OurStyle AS OS ON MPPB.OurStyleId=OS.Id
                           LEFT OUTER JOIN TRN.ProductionBatchMaster AS BM ON MPPB.ProductionBatchMasterId=BM.Id
                           LEFT OUTER JOIN TRN.SalesOrderMaster AS SO ON BM.ProductionOrderMasterId=SO.Id
                           WHERE MPPB.PlantId='" + plantId + "' AND MPPB.[Date]>='" + DateTime.Now.Date + "'  AND MPPB.ProcessId='" + processId + "' ORDER BY WC.EntityId,WC.Code";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        /// <summary>
        /// Set color in prf(Lsd CD wise)
        /// </summary>
        /// <param name="dt"></param>
        private void SetColor(DataTable dt)
        {
            try
            {
                var color = "";
                var dtPrf = new DataView(dt).ToTable(true, "ProductionBatchMasterId");
                for (int i = 0; i < dtPrf.Rows.Count; i++)
                {
                    var dvByPrf = new DataView(dt)
                    {
                        RowFilter = "ProductionBatchMasterId='" + dtPrf.Rows[i]["ProductionBatchMasterId"] + "' AND LineId<> ''",
                    };
                    if (dvByPrf.Count > 0)
                    {
                        var dtByPrf = dvByPrf.ToTable();
                        var getCD = Convert.ToDateTime(dtByPrf.Compute("MAX(CommitmentDate)", null));
                        var maxDate = Convert.ToDateTime(dtByPrf.Compute("MAX(Date)", null));
                        if (getCD == maxDate)
                        {
                            color = "#15ed32";// #64fc79 #f40202
                            SetRowWiseColor(dt, color, dtByPrf);
                        }
                        else if (getCD > maxDate)
                        {
                            color = "#64fc79";// #15ed32 #f40202
                            SetRowWiseColor(dt, color, dtByPrf);
                        }
                        else if (getCD < maxDate)
                        {
                            color = "#f40202";// #15ed32 #64fc79
                            SetRowWiseColor(dt, color, dtByPrf);
                        }
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void SetOffDays(DataTable dtBlock, string plantId, DateTime toDate, DataTable offDays, string processId)
        {
            try
            {
                var dvOffDays = new DataView(offDays) { RowFilter = "OffDayDate>='" + DateTime.Now.Date + "' AND OffDayDate<='" + toDate.Date + "'" };
                var dtOffDays = dvOffDays.ToTable();

                var dv = new DataView(dtBlock) { RowFilter = "LineId<>''" };
                var dtLines = dv.ToTable(true, "LineId");
                var currentOffDate = DateTime.Now.Date;
                DataRow newDr = null;
                for (int i = 0; i < dtOffDays.Rows.Count; i++)
                {
                    currentOffDate = Convert.ToDateTime(dtOffDays.Rows[i]["OffDayDate"]).Date;
                    for (int j = 0; j < dtLines.Rows.Count; j++)
                    {
                        dv.RowFilter = null;
                        dv.RowFilter = "LineId='" + dtLines.Rows[j]["LineId"] + "' AND Date='" + currentOffDate + "'";
                        if (dv.Count == 0)
                        {
                            newDr = dtBlock.NewRow();
                            newDr["Id"] = Guid.NewGuid();
                            newDr["MinAllocatedLine"] = 0;
                            newDr["IncrementType"] = DBNull.Value;
                            newDr["IncrementValue"] = 0.00;
                            newDr["StandardTime"] = 0;
                            newDr["DaysToGetTheTarget"] = 0;
                            newDr["FirstDayOutPut"] = 0;
                            newDr["MinRequiredTargetHourly"] = 0;
                            newDr["ProductionBatchMasterId"] = DBNull.Value;
                            newDr["EntityId"] = DBNull.Value;
                            newDr["PlantId"] = plantId;
                            newDr["ProcessId"] = processId;
                            newDr["LineId"] = dtLines.Rows[j]["LineId"];
                            newDr["OurStyleId"] = DBNull.Value;
                            newDr["RunningDay"] = 0;
                            newDr["MinWorkingDays"] = 0;
                            newDr["TotalQty"] = 0;
                            newDr["DailyOutPut"] = 0;
                            newDr["StandardDailyOutPut"] = 0;
                            newDr["LearningCurveOutPut"] = 0;
                            newDr["HasLearningCurve"] = false;
                            newDr["IsFreeze"] = false;
                            newDr["OffDayType"] = dtOffDays.Rows[i]["OffDayType"];
                            newDr["OffDay"] = true;
                            newDr["Lsd"] = dtOffDays.Rows[i]["OffDayDate"];
                            newDr["CommitmentDate"] = dtOffDays.Rows[i]["OffDayDate"];
                            newDr["Date"] = dtOffDays.Rows[i]["OffDayDate"];
                            newDr["Sequence"] = 0;
                            newDr["IsDb"] = false;
                            dtBlock.Rows.Add(newDr);
                        }
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private static void SetRowWiseColor(DataTable dt, string color, DataTable dtByPrf)
        {
            DataView dv = null;
            DataRow dr = null;
            for (int j = 0; j < dtByPrf.Rows.Count; j++)
            {
                var id = dtByPrf.Rows[j]["Id"].ToString();
                dv = new DataView(dt)
                {
                    RowFilter = "Id='" + id + "'"
                };
                if (dv.Count > 0)
                {
                    dr = dv[0].Row;
                    dr.BeginEdit();
                    dr["Color"] = color;
                    dr.EndEdit();
                }
            }
        }

        public void SaveFreezing(string[] idList)
        {
            var flag = false;
            try
            {
                if (idList.Length > 0)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var dbData = Query(t => idList.Contains(t.ProductionBatchMasterId)).Select().AsEnumerable();
                    foreach (var item in dbData)
                    {
                        item.IsFreeze = true;
                        UpdateGraph(item);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #region DataSetToModel

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            var data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                var item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            var temp = typeof(T);
            var obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        if (dr[column.ColumnName] == DBNull.Value)
                            dr[column.ColumnName] = "";
                        pro.SetValue(obj, dr[column.ColumnName], null);
                        break;
                    }
                }
            }
            return obj;
        }

        #endregion DataSetToModel

        #region DbData

        private DataSet GetWeekendCalendar(string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT ODD.PlantId, ODD.OffDayMasterId
	                                  , CONVERT(DATE, ODD.OffDayDate) AS OffDayDate, ODD.DayName, ODD.IsIncentiveLock
	                                  , ODD.DayLengthType, CONVERT(DATE, OM.FromDate) AS FromDate
	                                  , CONVERT(DATE, OM.ToDate) AS ToDate, OM.OffDayType, OM.TotalDay
                                FROM SCS.OffDayDetail AS ODD
                                INNER JOIN SCS.OffDayMaster AS OM ON ODD.OffDayMasterId=OM.Id
                                WHERE OM.PlantId='" + plantId + @"' AND OM.CompanyGroupId='" + identity.CompanyGroupId + @"'
                                AND '" + DateTime.Now.Date + "' BETWEEN OM.FromDate AND OM.ToDate"
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        /// <summary>
        /// Get workcenter by plant and company.
        /// </summary>
        /// <param name="plantId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        private DataSet GetWorkCenterWithAdditional(string plantId, string companyId, string processId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT DISTINCT WC.Id,PE.ProductionBatchMasterId
                                      , WC.CompanyId, WC.PlantId, WC.EntityId, WC.WorkCenterCategoryId, WC.WorkCenterSubcategoryId
                                      , WC.ProcessId, WC.Code, WC.UserName, WC.StandardName, WC.Description
                                      , WC.CapacityUoMId, WC.UoMId, WC.Capacity, WC.PlanEfficiency, WC.MaxTimePerDay
                                      , WC.StandardTimePerDay, WC.PlanBudgetCapacityPerDay, WC.DailyFixedCost
                                      , WC.VariableCost, WC.VariableCostTimeUoMId, WC.CurrencyId
                                FROM SCS.WorkCenterMaster AS Wc
                                INNER JOIN TRN.ProductionBatchEntity AS PE ON PE.EntityId=WC.EntityId
                                INNER JOIN TRN.ProductionBatchMaster AS PBM ON PBM.Id=PE.ProductionBatchMasterId
                                INNER JOIN HKP.ProductionStatus AS PRDS ON PRDS.Id=PBM.ProductionStatusId
                                WHERE PBM.PlantId='" + plantId + "' AND PBM.CompanyId='" + companyId + "' AND PRDS.PlanningGroupPriority=1 AND WC.ProcessId='" + processId + "'"
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        /// <summary>
        /// Get prodreference by plant and company.
        /// </summary>
        /// <param name="plantId"></param>
        /// <param name="companyId"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        private DataSet GetProdReferenceData(string plantId, string companyId, DateTime toDate, string processId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    order = "asc",
                    sort = "Lsd,ProductionPriority",
                    CmdText = @"SELECT PBM.Id, PBM.RecipeMasterId, PBM.ProductionOrderMasterId, PBM.MaterialMasterId, PBM.FirstInputDate
                                    , PBM.TargetCommitmentDate, CONVERT(DATE, PBM.Lsd) AS Lsd, PBM.LsdRemark, PBM.TargetLsd
                                    , CONVERT(DATE, PBM.CommitmentDate) AS CommitmentDate, PBM.CommitmentDateRemarks, PBM.CalculationBasis
                                    , PBM.SPT, PBM.NoOfWorkStation, PBM.BuyerId, PBM.Cm, PBM.Qty, PBM.CmCurrencyId, PBM.Efficiency, PBM.FirstDayOutPut
                                    , PBM.IncrementType, PBM.IncrementValue, PBM.MinAllocatedLine, PBM.ProductionStatusId, PBM.CompanyGroupId
                                    , PBM.CompanyId, PBM.PlantId, PBM.BulletinMasterId, PBM.StandardTime, PBM.MinWorkingDays, PBM.ProductionPriority
                                    , PBM.DaysToGetTheTarget, PBM.MinRequiredTargetHourly, PBM.EntityId, MM.OurStyleId, CAST(0 as INT) AS Sort
                                FROM TRN.ProductionBatchMaster AS PBM
                                LEFT OUTER JOIN HKP.ProductionStatus AS PRDS ON PRDS.Id=PBM.ProductionStatusId
                                LEFT OUTER JOIN TRN.ProductionBatchProcessSet AS PS ON PS.ProductionBatchMasterId=PBM.Id
                                LEFT OUTER JOIN TRN.SalesOrderMaster AS SOM ON SOM.Id=PBM.ProductionOrderMasterId
                                LEFT OUTER JOIN MST.MaterialMaster AS MM ON MM.Id=PBM.MaterialMasterId
                                WHERE PBM.PlantId='" + plantId + @"' AND PBM.CompanyId='" + companyId + @"' AND PRDS.PlanningGroupPriority=1 AND PS.IsBaseProcess=1 AND PS.ProcessId='" + processId + @"'
                                AND PBM.Id NOT IN (SELECT ProductionBatchMasterId FROM TRN.MainProcessPlanning WHERE IsFreeze=1)"
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        /// <summary>
        /// Get workcenter by line preference
        /// </summary>
        /// <param name="plantId"></param>
        /// <param name="companyId"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        private DataSet GetWorkCenterByLinePreference(string plantId, string companyId, DateTime toDate, string processId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT PWC.*
                                FROM TRN.ProductionBatchWorkCenter AS PWC
                                LEFT OUTER JOIN TRN.ProductionBatchMaster AS PBM ON PWC.ProductionBatchMasterId=PBM.Id
                                LEFT OUTER JOIN HKP.ProductionStatus AS PRDS ON PRDS.Id=PBM.ProductionStatusId
                                LEFT OUTER JOIN TRN.ProductionBatchProcessSet AS PS ON PS.ProductionBatchMasterId=PBM.Id
                                WHERE PBM.PlantId='" + plantId + @"' AND PBM.CompanyId='" + companyId + "' AND PRDS.PlanningGroupPriority=1 AND PS.IsBaseProcess=1 AND PS.ProcessId='" + processId + @"'"
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private DataSet GetBlockData(string plantId, DateTime toDate, string processId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT MPPB.Id, MPPB.MinAllocatedLine, MPPB.IncrementType
                                    , MPPB.IncrementValue, MPPB.StandardTime, MPPB.DaysToGetTheTarget, MPPB.FirstDayOutPut
	                                , MPPB.MinRequiredTargetHourly, MPPB.EntityId, MPPB.PlantId, MPPB.LineId, MPPB.ProductionBatchMasterId
	                                , MPPB.OurStyleId, MPPB.RunningDay, MPPB.MinWorkingDays, MPPB.TotalQty, MPPB.DailyOutPut
	                                , MPPB.StandardDailyOutPut, MPPB.LearningCurveOutPut, MPPB.HasLearningCurve, MPPB.IsFreeze, MPPB.OffDayType
	                                , MPPB.OffDay, MPPB.Lsd, MPPB.CommitmentDate, MPPB.[Date], MPPB.[Sequence], CAST(1 as BIT) AS IsDb
                                FROM TRN.MainProcessPlanning AS MPPB
                                WHERE MPPB.PlantId='" + plantId + "' AND MPPB.ProcessId='" + processId + "' --AND MPPB.Date>='" + DateTime.Now + "'"
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #endregion DbData

        private DataTable SetLsdCD(DataSet prodRef, DataSet DbBlockDs, ref DataSet daysDs, DataSet workCenter, DateTime toDate, string processId)
        {
            try
            {
                DataTable dtBlock = null;
                var dld = new DistributedLineDate(prodDtSet, calendarFromDb, DbBlockDs, wcPreferenceDs.Tables[0], workCenter.Tables[0], processId);
                dld.CreateDtTable();
                dtBlock = dld.SteLineMain(toDate);
                return dtBlock;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void DetailRowCreation(DataRow drBlock)
        {
            var runnigDays = Convert.ToInt32(drBlock["RunningDay"].ToString());
            var lsd = Convert.ToDateTime(drBlock["Lsd"].ToString());
            for (int i = 0; i < runnigDays; i++)
            {
                lsd = lsd.AddDays(i);
                var newDr = daysFromDb.Tables[0].NewRow();
                newDr["Id"] = DateTime.Now.Ticks + i;
                newDr["MainProcessPlanningId"] = drBlock["MainProcessPlanningId"];
                newDr["MainProcessPlanningBlockId"] = drBlock["Id"];
                newDr["Date"] = lsd;
                //TODO: Calculation Stander Time & Total hr output.
                newDr["TotalOutPut"] = drBlock["MinRequiredTargetHourly"];
                daysFromDb.Tables[0].Rows.Add(newDr);
            }
        }

        private void ResetDailyOP(DataTable dt, DataTable dtWC)
        {
            try
            {
                for (int i = 0; i < dtWC.Rows.Count; i++)
                {
                    var productionRef = dtWC.Rows[i]["Id"].ToString();
                    var dv = new DataView(dt)
                    {
                        RowFilter = "ProductionBatchMasterId='" + productionRef + "'",
                        Sort = "Date"
                    };
                    if (dv.Count > 0)
                    {
                        var targetDay = (dv[1]["DaysToGetTheTarget"].ToString()).ToInt();
                        if (targetDay > 0)
                        {
                            var dailyOutput = Convert.ToInt32(dv[1]["StandardTime"].ToString()) * Convert.ToInt32(dv[1]["MinRequiredTargetHourly"].ToString());
                            var lcCounter = 0;
                            for (int j = 0; j < dv.Count; j++)
                            {
                                var dr = dv[j].Row;
                                if (!Convert.ToBoolean(dr["OffDay"].ToString()))
                                {
                                    lcCounter++;
                                    if (targetDay >= lcCounter)
                                    {
                                        dr.BeginEdit();
                                        dr["DailyOutPut"] = ResetDailyOutPut(dr, lcCounter, targetDay);
                                        dr["HasLearningCurve"] = true;
                                        dr.EndEdit();
                                    }
                                    else//other daily out put
                                    {
                                        dr.BeginEdit();
                                        dr["DailyOutPut"] = dailyOutput;
                                        dr.EndEdit();
                                    }
                                }
                                else//Off day wise edit
                                {
                                    dr.BeginEdit();
                                    dr["RunningDay"] = 0;
                                    dr["TotalQty"] = 0;
                                    dr["DailyOutPut"] = 0;
                                    dr["StandardDailyOutPut"] = 0;
                                    dr["LearningCurveOutPut"] = 0;
                                    dr.EndEdit();
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < dv.Count; j++)
                            {
                                var dr = dv[j].Row;
                                if (Convert.ToBoolean(dr["OffDay"].ToString()))//Off day wise edit
                                {
                                    dr.BeginEdit();
                                    dr["RunningDay"] = 0;
                                    dr["TotalQty"] = 0;
                                    dr["DailyOutPut"] = 0;
                                    dr["StandardDailyOutPut"] = 0;
                                    dr["LearningCurveOutPut"] = 0;
                                    dr.EndEdit();
                                }
                            }
                        }
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public int ResetDailyOutPut(DataRow dr, int lcCounter, int targetDay)
        {
            try
            {
                var dailyOutput = Convert.ToInt32(dr["StandardTime"].ToString()) * Convert.ToInt32(dr["MinRequiredTargetHourly"].ToString());
                var firstDayOutPut = Convert.ToInt32(dr["StandardTime"].ToString()) * Convert.ToInt32(dr["FirstDayOutPut"].ToString());
                var incrementValue = Convert.ToInt32(Convert.ToDecimal(dr["IncrementValue"].ToString()));
                if (lcCounter == 1)
                    return firstDayOutPut;
                var iv = incrementValue;
                if (dr["IncrementType"].ToString() != "Fixed")
                {
                    var c = firstDayOutPut;
                    iv = iv * c / 100;
                }
                iv = (iv * Convert.ToInt32(dr["StandardTime"].ToString()));//daily iv
                var totalCumulative = firstDayOutPut;
                for (int i = 0; i < lcCounter; i++)
                {
                    totalCumulative = i == 0 ? firstDayOutPut : totalCumulative + iv;
                }
                return totalCumulative;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }
    }

    public class Block : CommonFunction
    {
        private DataSet dsPrdRef;
        private DataSet _offDays;
        private DataSet _dbBlockDs;
        private DataTable dtBlock;
        private DataTable _dtBlankBlock;
        private DataTable _dtWcPreference;
        private DataTable _dtWorkCenter;

        public DataTable DtBlock
        {
            get
            {
                return dtBlock;
            }
        }

        public Block(DataSet Source, DataSet offDays, DataSet dbBlockDs, DataTable dtWcPreference, DataTable dtWorkCenter)
        {
            dsPrdRef = Source;
            _offDays = offDays;
            _dbBlockDs = dbBlockDs;
            _dtWcPreference = dtWcPreference;
            _dtWorkCenter = dtWorkCenter;
        }

        private int GetLineDays(int ordQty, int dailyOutPut)
        {
            try
            {
                return ordQty / dailyOutPut;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private int GetBlock(int lineDays, int line)
        {
            try
            {
                return lineDays / line;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private DataTable CreateTable(params string[] columnName)
        {
            DataTable dt = null;
            try
            {
                dt = new DataTable();
                foreach (var item in columnName)
                {
                    dt.Columns.Add(new DataColumn(item));
                }
                dt.Columns.Add(new DataColumn("RunningDay", typeof(int)));
                dt.Columns.Add(new DataColumn("MinAllocatedLine", typeof(int)));
                dt.Columns.Add(new DataColumn("IncrementValue", typeof(decimal)));
                dt.Columns.Add(new DataColumn("StandardTime", typeof(int)));
                dt.Columns.Add(new DataColumn("DaysToGetTheTarget", typeof(int)));
                dt.Columns.Add(new DataColumn("FirstDayOutPut", typeof(int)));
                dt.Columns.Add(new DataColumn("MinRequiredTargetHourly", typeof(int)));
                dt.Columns.Add(new DataColumn("MinWorkingDays", typeof(int)));
                dt.Columns.Add(new DataColumn("HasLearningCurve", typeof(bool)));
                dt.Columns.Add(new DataColumn("OffDay", typeof(bool)));
                dt.Columns.Add(new DataColumn("IsFreeze", typeof(bool)));
                dt.Columns.Add(new DataColumn("IsDb", typeof(bool)));

                dt.Columns.Add(new DataColumn("TotalQty", typeof(int)));
                dt.Columns.Add(new DataColumn("DailyOutPut", typeof(int)));
                dt.Columns.Add(new DataColumn("StandardDailyOutPut", typeof(int)));
                dt.Columns.Add(new DataColumn("LearningCurveOutPut", typeof(int)));

                dt.Columns.Add(new DataColumn("Lsd", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("CommitmentDate", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Date", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Sequence", typeof(Int32)));
                return dt;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void CreateBlock(DataRow dr, ref DataTable dtBlock)
        {
            try
            {
                //int minAllocatedLine = 1;//Convert.ToInt32(dr["MinAllocatedLine"].ToString());
                // TODO: function return how many line required.

                var qty = Convert.ToInt32(dr["Qty"].ToString());
                var incrementValue = Convert.ToDecimal(dr["IncrementValue"].ToString());
                var daysToGetTarget = Convert.ToInt32(dr["DaysToGetTheTarget"].ToString());
                var ourStyleId = dr["OurStyleId"].ToString();
                var standardTime = Convert.ToInt32(dr["StandardTime"].ToString()); //TODO:Standard time come from settings
                var firstDayOutPut = standardTime * Convert.ToInt32(dr["FirstDayOutPut"].ToString());
                var minRequiredTargetHourly = Convert.ToInt32(dr["MinRequiredTargetHourly"].ToString());
                var daysToGetTheTarget = Convert.ToInt32(dr["DaysToGetTheTarget"].ToString());
                //int lineDays = GetLineDays(qty, (standardTime * minRequiredTargetHourly));
                //int block = GetBlock(lineDays, minAllocatedLine);
                var dvPRefLine = new DataView(_dtWorkCenter) { RowFilter = "ProductionBatchMasterId='" + dr["Id"] + "'" };
                var minAllocatedLine = GetMinmumLine(dvPRefLine.ToTable(), Convert.ToInt32(dr["MinAllocatedLine"].ToString()));
                var block = RunningDays(qty, CalculateLearningCurveOutPut(dr), daysToGetTheTarget, (standardTime * minRequiredTargetHourly), minAllocatedLine);
                DataRow newDr = null;
                for (int i = 0; i < block; i++)
                {
                    newDr = dtBlock.NewRow();
                    newDr["Id"] = Guid.NewGuid();
                    newDr["PlantId"] = dr["PlantId"];
                    newDr["LineId"] = "";//TODO:NEED STYLEWISE LINE.
                    newDr["EntityId"] = dr["EntityId"];
                    newDr["ProductionBatchMasterId"] = dr["Id"];
                    newDr["OurStyleId"] = dr["OurStyleId"];
                    newDr["Lsd"] = dr["Lsd"];
                    newDr["CommitmentDate"] = dr["CommitmentDate"];
                    newDr["RunningDay"] = block;//block - offDays;
                    newDr["Date"] = DBNull.Value;
                    newDr["TotalQty"] = qty;
                    newDr["DailyOutPut"] = qty / block;//(block - offDays);
                    newDr["StandardDailyOutPut"] = qty / block;// (block - offDays);
                    newDr["LearningCurveOutPut"] = CalculateLearningCurveOutPut(dr);
                    newDr["HasLearningCurve"] = false;
                    newDr["IsFreeze"] = false;
                    newDr["OffDay"] = false;
                    newDr["OffDayType"] = DBNull.Value;
                    newDr["Sequence"] = i;
                    newDr["MinAllocatedLine"] = Convert.ToInt32(dr["MinAllocatedLine"]);
                    newDr["IncrementType"] = dr["IncrementType"];
                    newDr["IncrementValue"] = Convert.ToDecimal(dr["IncrementValue"]);
                    newDr["StandardTime"] = Convert.ToInt32(dr["StandardTime"]);
                    newDr["MinRequiredTargetHourly"] = Convert.ToInt32(dr["MinRequiredTargetHourly"]);
                    newDr["DaysToGetTheTarget"] = Convert.ToInt32(dr["DaysToGetTheTarget"]);
                    newDr["FirstDayOutPut"] = Convert.ToInt32(dr["FirstDayOutPut"]);
                    newDr["MinWorkingDays"] = Convert.ToInt32(dr["MinWorkingDays"]);
                    newDr["IsDb"] = false;
                    newDr["Color"] = null;

                    dtBlock.Rows.Add(newDr);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void CreateBlock()
        {
            try
            {
                string[] col = { "Id", "EntityId", "PlantId", "LineId", "ProductionBatchMasterId", "OurStyleId", "OffDayType", "IncrementType", "Color" };
                dtBlock = CreateTable(col);
                if (_dbBlockDs != null && _dbBlockDs.Tables[0].Rows.Count > 0)
                {
                    DataRow newDr = null;
                    for (int i = 0; i < _dbBlockDs.Tables[0].Rows.Count; i++)
                    {
                        //if (!Convert.ToBoolean(_dbBlockDs.Tables[0].Rows[i]["IsFreeze"]))
                        //{
                        //    _dbBlockDs.Tables[0].Rows[i]["LineId"] = "";
                        //    _dbBlockDs.Tables[0].Rows[i]["Date"] = DBNull.Value;
                        //}
                        newDr = dtBlock.NewRow();
                        newDr["Id"] = _dbBlockDs.Tables[0].Rows[i]["Id"];
                        newDr["MinAllocatedLine"] = _dbBlockDs.Tables[0].Rows[i]["MinAllocatedLine"];
                        newDr["IncrementType"] = _dbBlockDs.Tables[0].Rows[i]["IncrementType"];
                        newDr["IncrementValue"] = _dbBlockDs.Tables[0].Rows[i]["IncrementValue"];
                        newDr["StandardTime"] = _dbBlockDs.Tables[0].Rows[i]["StandardTime"];
                        newDr["DaysToGetTheTarget"] = _dbBlockDs.Tables[0].Rows[i]["DaysToGetTheTarget"];
                        newDr["FirstDayOutPut"] = _dbBlockDs.Tables[0].Rows[i]["FirstDayOutPut"];
                        newDr["MinRequiredTargetHourly"] = _dbBlockDs.Tables[0].Rows[i]["MinRequiredTargetHourly"];
                        newDr["EntityId"] = _dbBlockDs.Tables[0].Rows[i]["EntityId"];
                        newDr["PlantId"] = _dbBlockDs.Tables[0].Rows[i]["PlantId"];
                        newDr["LineId"] = _dbBlockDs.Tables[0].Rows[i]["LineId"];
                        newDr["ProductionBatchMasterId"] = _dbBlockDs.Tables[0].Rows[i]["ProductionBatchMasterId"];
                        newDr["OurStyleId"] = _dbBlockDs.Tables[0].Rows[i]["OurStyleId"];
                        newDr["RunningDay"] = _dbBlockDs.Tables[0].Rows[i]["RunningDay"];
                        newDr["MinWorkingDays"] = _dbBlockDs.Tables[0].Rows[i]["MinWorkingDays"];
                        newDr["TotalQty"] = _dbBlockDs.Tables[0].Rows[i]["TotalQty"];
                        newDr["DailyOutPut"] = _dbBlockDs.Tables[0].Rows[i]["DailyOutPut"];
                        newDr["StandardDailyOutPut"] = _dbBlockDs.Tables[0].Rows[i]["StandardDailyOutPut"];
                        newDr["LearningCurveOutPut"] = _dbBlockDs.Tables[0].Rows[i]["LearningCurveOutPut"];
                        newDr["HasLearningCurve"] = _dbBlockDs.Tables[0].Rows[i]["HasLearningCurve"];
                        newDr["IsFreeze"] = _dbBlockDs.Tables[0].Rows[i]["IsFreeze"];
                        newDr["OffDayType"] = _dbBlockDs.Tables[0].Rows[i]["OffDayType"];
                        newDr["OffDay"] = _dbBlockDs.Tables[0].Rows[i]["OffDay"];
                        newDr["Lsd"] = _dbBlockDs.Tables[0].Rows[i]["Lsd"];
                        newDr["CommitmentDate"] = _dbBlockDs.Tables[0].Rows[i]["CommitmentDate"];
                        newDr["Date"] = _dbBlockDs.Tables[0].Rows[i]["Date"];
                        newDr["Sequence"] = _dbBlockDs.Tables[0].Rows[i]["Sequence"];
                        newDr["IsDb"] = _dbBlockDs.Tables[0].Rows[i]["IsDb"];
                        dtBlock.Rows.Add(newDr);
                    }
                }
                for (int i = 0; i < dsPrdRef.Tables[0].Rows.Count; i++)
                {
                    CreateBlock(dsPrdRef.Tables[0].Rows[i], ref dtBlock);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private int BlankBlock()
        {
            var dv = new DataView(dtBlock)
            {
                RowFilter = "LineId=''",
                Sort = "ProductionBatchMasterId"
            };
            _dtBlankBlock = dv.ToTable();
            return _dtBlankBlock.Rows.Count;
        }

        public void SetLineInBlock(DataTable dtWC)
        {
            try
            {
                BlankBlock();
                DataRow row = null;
                BlockLineSetup blcSet = null;
                for (int i = 0; i < _dtBlankBlock.Rows.Count; i++)
                {
                    row = _dtBlankBlock.Rows[i];
                    /// Get pref id
                    /// Check in dt block
                    /// Check lsd  and cd
                    /// if ok set
                    /// or get style id
                    /// Check in dtblock

                    var productionRef = row["ProductionBatchMasterId"].ToString();
                    var ourStyleId = row["OurStyleId"].ToString();
                    var dvWC = new DataView(dtWC)
                    {
                        RowFilter = "ProductionBatchMasterId='" + productionRef + "'"
                    };
                    var dtWorkCenter = dvWC.ToTable();
                    var minLine = GetMinmumLine(dtWorkCenter, Convert.ToInt32(row["MinAllocatedLine"].ToString()));
                    var minWorkingDays = Convert.ToInt32(row["MinWorkingDays"].ToString());
                    var id = row["Id"].ToString();
                    var lsd = row["Lsd"].ToString();
                    var dv = new DataView(dtBlock)
                    {
                        RowFilter = "ProductionBatchMasterId='" + productionRef + "' AND LineId <> ''",
                        Sort = "Sequence"
                    };
                    if (dv.Count > 0)//Give all block which is the same pr id;
                    {
                        if (HasNoMinWorkDaysInLine(dv.ToTable(), minWorkingDays))//if crossing min working days
                        {
                            //check has more line
                            if (HasNoMoreLine(dv.ToTable(), minLine))
                            {
                                var dtLine = dv.ToTable(true, "LineId");
                                var remainingLine = minLine - dtLine.Rows.Count;
                                var remainingDays = RemainingWorkingDdays(dv.ToTable(), minLine, remainingLine);//return remaining days
                                if (remainingDays > (minWorkingDays * remainingLine))// if has more working days without ((minLine - totalGivenline) * minWorkingDays)
                                    ReturnLine(id, dv);//no more line so add block in the same line
                                else
                                    blcSet = FirstTimeReturnLine(row, productionRef, ourStyleId, dtWorkCenter, id);
                            }
                            else
                                ReturnLine(id, dv);//no more line so add block in the same line
                        }
                        else
                            ReturnLine(id, dv);//min working days still not crossing so add block in the same line
                    }//same pr id
                    else//different pr id
                        blcSet = FirstTimeReturnLine(row, productionRef, ourStyleId, dtWorkCenter, id);
                }//for
                //SetLineCdExceed(dtWC, dtBlock, _dtWcPreference);//for exceed cd
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private BlockLineSetup FirstTimeReturnLine(DataRow row, string productionRef, string ourStyleId, DataTable dtWorkCenter, string id)
        {
            BlockLineSetup blcSet;
            //filter line preference list by p.Ref
            var preferenceWC = new DataView(_dtWcPreference) { RowFilter = "ProductionBatchMasterId='" + productionRef + "'" };
            var prfWcDt = preferenceWC.ToTable();
            var dv = new DataView(dtBlock) { RowFilter = "ProductionBatchMasterId='" + productionRef + "' AND LineId <> ''" };

            var dtTable = dv.ToTable(true, "LineId");
            string[] dtLines = null;
            var rows = dtTable.Select();
            if (rows.Count() > 0)
                dtLines = Array.ConvertAll(rows, t => t["LineId"].ToString());

            blcSet = new BlockLineSetup(dtWorkCenter, prfWcDt, row, dtBlock, _offDays, _dtBlankBlock);
            blcSet.GetLineByCD(dtLines);

            var line = blcSet.Line;
            var maxDate = blcSet.Maxdate;
            var offDay = blcSet.OffDay;
            var offDayType = blcSet.OffDayType;
            var dailyOutput = 0;
            if (!offDay)
                dailyOutput = GetPerDayLCPrd(dtBlock, productionRef, line);

            dv.RowFilter = null;
            dv.RowFilter = "ProductionBatchMasterId<>'" + productionRef + "' AND LineId <> ''";
            if (dv.Count > 0)
            {
                dv.RowFilter = "ProductionBatchMasterId<>'" + productionRef + "' AND OurStyleId='" + ourStyleId + "' AND LineId <> ''";
                if (dv.Count > 0)
                    SetLineInBlock(id, line, maxDate, offDayType, offDay, dailyOutput);
                else
                    SetLineInBlock(id, line, maxDate, offDayType, offDay, dailyOutput);
            }
            else//different pr very first.
                SetLineInBlock(id, line, maxDate, offDayType, offDay, dailyOutput);
            return blcSet;
        }

        private void ReturnLine(string id, DataView dv)
        {
            //pr found
            var dailyOutput = 0;
            var dt = GetLastBlock(dv.ToTable());
            if (dt.Rows.Count > 0)
            {
                var offDay = false;
                string offDayType = null;
                var nextWorkingDay = Convert.ToDateTime(dt.Rows[0]["Date"].ToString()).AddDays(1);
                var odDr = ResetDateWithOffDay(nextWorkingDay, _offDays);
                if (odDr != null)
                {
                    offDay = true;
                    offDayType = odDr["OffDayType"].ToString();
                }
                else
                    dailyOutput = GetPerDayLCPrd(dtBlock, dt.Rows[0]["ProductionBatchMasterId"].ToString(), dt.Rows[0]["LineId"].ToString());
                SetLineInBlock(id, dt.Rows[0]["LineId"].ToString(), nextWorkingDay, offDayType, offDay, dailyOutput);
            }
            else//For first time block set in line
            {
                //string ids = GetLineIds(dtWorkCenter);
                //DataView dataView = new DataView(dtBlock)
                //{
                //    RowFilter = "LineId IN (" + ids + ")"
                //};
                //if (dataView.Count == 0)//Very first time
                //{
                //    DateTime _maxDate = GetMaxDate(dv.ToTable());
                //    DateTime date = BlockStartingBasedOnLsd(_maxDate, row);
                //    SetLineInBlock(id, dtWorkCenter.Rows[0]["Id"].ToString(), date,);
                //}
            }
        }

        private void SetLineInBlock(string id, string lineFound, DateTime date, string offDayType, bool offDay, int dailyOutput)
        {
            try
            {
                var dv = new DataView(dtBlock)
                {
                    RowFilter = "Id='" + id + "'"
                };
                if (dv.Count > 0)
                {
                    var dr = dv[0].Row;
                    dr.BeginEdit();
                    dr["LineId"] = lineFound;
                    dr["OffDayType"] = offDayType;
                    dr["OffDay"] = offDay;
                    dr["Date"] = date;
                    dr["DailyOutPut"] = dailyOutput.ToString();
                    //TODO: Child
                    dr.EndEdit();
                    DateSetToDic(lineFound, Convert.ToInt32(Convert.ToDateTime(date).ToString("yyMMdd")));
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        /// <summary>
        /// Line set which is exceed cd.
        /// </summary>
        /// <param name="dtBlock"></param>
        /// <param name="dtWithoutLine"></param>
        private void SetLineCdExceed(DataTable dtWC, DataTable dtBlock, DataTable _dtWcPreference)
        {
            try
            {
                var dtWithoutLinekDv = new DataView(dtBlock) { RowFilter = "ISNULL (LineId, '')=''" };
                if (dtWithoutLinekDv.Count > 0)
                {
                    var dtPrf = dtWithoutLinekDv.ToTable(true, "ProductionBatchMasterId");
                    if (dtPrf.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtPrf.Rows.Count; i++)
                        {
                            var productionRef = dtPrf.Rows[i]["ProductionBatchMasterId"].ToString();
                            var dvByPrf = new DataView(dtBlock)
                            {
                                RowFilter = "ProductionBatchMasterId='" + dtPrf.Rows[i]["ProductionBatchMasterId"] + "'",
                            };
                            for (int j = 0; j < dvByPrf.Count; j++)
                            {
                                var ourStyleId = dvByPrf[j]["OurStyleId"].ToString();
                                var id = dvByPrf[j]["Id"].ToString();
                                var lsd = dvByPrf[j]["Lsd"].ToString();
                                var sequence = Convert.ToInt32(dvByPrf[j]["Sequence"].ToString());
                                if (sequence != 0)//Give all block which is the same pr id;
                                {
                                    //pr found
                                    var date = Convert.ToDateTime(dvByPrf[j - 1]["Date"].ToString());
                                    //bool offDay = false;
                                    string offDayType = null;
                                    var nextWorkingDay = date.AddDays(1);
                                    var odDr = ResetDateWithOffDay(nextWorkingDay, _offDays);
                                    if (odDr != null)
                                    {
                                        //offDay = true;
                                        offDayType = odDr["OffDayType"].ToString();
                                    }
                                    //SetLineInBlock(id, dvByPrf[j - 1]["LineId"].ToString(), nextWorkingDay, offDayType, offDay);
                                }//same pr id
                                else//different pr id
                                {
                                    var dv = new DataView(dtBlock)
                                    {
                                        RowFilter = "ProductionBatchMasterId='" + productionRef + "' AND LineId<>''"
                                    };

                                    var dtTable = dv.ToTable(true, "LineId");
                                    string[] dtLines = null;
                                    var rows = dtTable.Select();
                                    if (rows.Count() > 0)
                                        dtLines = Array.ConvertAll(rows, t => t["LineId"].ToString());

                                    var blcSet = new BlockLineSetup(dtWC, _dtWcPreference, null, dtBlock, _offDays, _dtBlankBlock);
                                    blcSet.GetLineForCDExceed(productionRef, ourStyleId, dtLines);
                                    var line = blcSet.Line;
                                    var maxDate = blcSet.Maxdate;
                                    var offDay = blcSet.OffDay;
                                    var offDayType = blcSet.OffDayType;
                                    //SetLineInBlock(id, line, maxDate, offDayType, offDay);
                                }
                            }
                        }
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void DateSetToDic(string line, int date)
        {
            try
            {
                if (dicDateList.ContainsKey(line))
                {
                    if (!dicDateList[line].Contains(date))
                    {
                        dicDateList[line].Add(date);
                    }
                }
                else
                {
                    var list = new List<int>
                    {
                        date
                    };
                    dicDateList.Add(line, list);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private string GetLineIds(DataTable dtWorkCenter)
        {
            var lineIds = "' '";
            for (int ri = 0; ri < dtWorkCenter.Rows.Count; ri++)
            {
                if (lineIds == "' '")
                    lineIds = "'" + dtWorkCenter.Rows[ri]["Id"]+ "'";
                else
                    lineIds += ",'" + dtWorkCenter.Rows[ri]["Id"]+ "'";
            }
            return lineIds;
        }

        private DataTable GetLastBlock(DataTable dt)
        {
            try
            {
                var maxSequence = Convert.ToInt32(dt.Compute("MAX(Sequence)", null));
                var dv = new DataView(dt)
                {
                    RowFilter = "Sequence=" + maxSequence
                };

                return dv.ToTable();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }
    }

    public class BlockLineSetup : CommonFunction
    {
        private DataTable _dtWC;
        private DataRow _drCurrentBlock;
        private DataTable _dtBlock;
        private DataSet _offDays;
        private DataTable _dtBlankBlock;
        private DataTable _dtWcPreference;
        public string Line { get; set; } = "";
        public DateTime Maxdate { get; set; } = DateTime.Now.Date;
        public bool OffDay { get; set; }
        public string OffDayType { get; set; }

        public BlockLineSetup(DataTable dtWC, DataTable dtWcPreference, DataRow drCurrentBlock, DataTable dtBlock, DataSet offDays, DataTable dtBlankBlock)
        {
            _dtWC = dtWC;
            _drCurrentBlock = drCurrentBlock;
            _dtBlock = dtBlock;
            _offDays = offDays;
            _dtBlankBlock = dtBlankBlock;
            _dtWcPreference = dtWcPreference;
        }

        #region Line Setup

        public void GetLineByCD(string[] dtLines)
        {
            try
            {
                if (_dtWcPreference != null && _dtWcPreference.Rows.Count > 0)
                    SetLineByPreference(dtLines);
                else
                    SetLine(dtLines);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void SetLineByPreference(string[] dtLines)
        {
            for (int i = 0; i < _dtWcPreference.Rows.Count; i++)//prf different same line
            {
                if (!HasLineExist(dtLines, _dtWcPreference.Rows[i]["WorkCenterMasterId"].ToString()))
                {
                    var dv = new DataView(_dtBlock)
                    {
                        RowFilter = "LineId='" + _dtWcPreference.Rows[i]["WorkCenterMasterId"] + "'"
                    };
                    if (dv.Count > 0)
                    {
                        ReturnLineWithDate(dv);
                        break;
                    }
                    else
                    {
                        ReturnLineWithDate(dv, _dtWcPreference.Rows[i]["WorkCenterMasterId"].ToString());
                        break;
                    }
                }
            }// for
            if (string.IsNullOrEmpty(Line))//prf different new line
                SetLine(dtLines);
        }

        private void SetLine(string[] dtLines)
        {
            for (int i = 0; i < _dtWC.Rows.Count; i++)//prf different same line
            {
                if (!HasLineExist(dtLines, _dtWC.Rows[i]["Id"].ToString()))
                {
                    var dv = new DataView(_dtBlock) { RowFilter = "LineId='" + _dtWC.Rows[i]["Id"] + "'" };
                    if (dv.Count > 0)
                    {
                        ReturnLineWithDate(dv);
                        break;
                    }
                    else
                    {
                        ReturnLineWithDate(dv, _dtWC.Rows[i]["Id"].ToString());
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(Line))//prf different new line
            {
                for (int i = 0; i < _dtWC.Rows.Count; i++)
                {
                    var dv = new DataView(_dtBlock)
                    {
                        RowFilter = "LineId='" + _dtWC.Rows[i]["Id"] + "'"
                    };
                    if (dv.Count == 0)
                    {
                        ReturnLineWithDate(dv, _dtWC.Rows[i]["Id"].ToString());
                        break;
                    }
                }
            }
        }

        private void ReturnLineWithDate(DataView dv)
        {
            try
            {
                var remainingDays = RemainingDaysCalculatedForLine();
                var blockMaxDate = GetMaxDate(dv.ToTable());
                var getBlockLsd = BlockStartingBasedOnLsd(blockMaxDate, Convert.ToDateTime(_drCurrentBlock["Lsd"].ToString()));
                var calculatedCD = getBlockLsd.AddDays(remainingDays - 1);
                var totalOffDay = TotalOffDay(getBlockLsd.ToString(), calculatedCD.ToString(), _offDays);
                calculatedCD = CDincreaseByOffDays(calculatedCD, totalOffDay, _offDays);
                if (calculatedCD <= Convert.ToDateTime(_drCurrentBlock["CommitmentDate"].ToString()))
                {
                    CreateNewBlockByOffDay(getBlockLsd, calculatedCD, _offDays, _drCurrentBlock);
                    var odDr = ResetDateWithOffDay(getBlockLsd, _offDays);
                    if (odDr != null)
                    {
                        Maxdate = getBlockLsd;
                        OffDay = true;
                        OffDayType = odDr[nameof(OffDayType)].ToString();
                        Line = (dv[0]["LineId"]).ToString();
                    }
                    else
                    {
                        Maxdate = getBlockLsd;
                        OffDay = false;
                        OffDayType = null;
                        Line = (dv[0]["LineId"]).ToString();
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private int RemainingDaysCalculatedForLine()
        {
            try
            {
                var totalRunningDays = (_drCurrentBlock["RunningDay"].ToString()).ToInt();
                var minLines = GetMinmumLine(_dtWC, (_drCurrentBlock["MinAllocatedLine"].ToString()).ToInt());
                var minWorkingDays = (_drCurrentBlock["MinWorkingDays"].ToString()).ToInt();
                var dvCurrentAllPrf = new DataView(_dtBlock)
                {
                    RowFilter = "ProductionBatchMasterId='" + _drCurrentBlock["ProductionBatchMasterId"]+ "' AND LineId<>''"
                };
                var dtLines = new DataView(dvCurrentAllPrf.ToTable()).ToTable(true, "LineId");
                var previusLineDays = dvCurrentAllPrf.Count;
                var givenLine = 0;
                givenLine = dtLines.Rows.Count;
                return totalRunningDays - (previusLineDays + ((minLines - givenLine - 1) * minWorkingDays));
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void ReturnLineWithDate(DataView dv, string lineId)
        {
            try
            {
                var remainingDays = RemainingDaysCalculatedForLine();
                var blockMaxDate = GetMaxDate(dv.ToTable());
                var getBlockLsd = BlockStartingBasedOnLsd(blockMaxDate, Convert.ToDateTime(_drCurrentBlock["Lsd"].ToString()));
                //DateTime getBlockLsd = BlockStartingBasedOnLsd(blockMaxDate, _drCurrentBlock);
                //DateTime calculatedCD = getBlockLsd.AddDays((_drCurrentBlock["RunningDay"].ToString()).ToInt() - 1);
                var calculatedCD = getBlockLsd.AddDays(remainingDays - 1);
                var totalOffDay = TotalOffDay(getBlockLsd.ToString(), calculatedCD.ToString(), _offDays);
                calculatedCD = CDincreaseByOffDays(calculatedCD, totalOffDay, _offDays);
                if (calculatedCD <= Convert.ToDateTime(_drCurrentBlock["CommitmentDate"].ToString()))
                {
                    CreateNewBlockByOffDay(getBlockLsd, calculatedCD, _offDays, _drCurrentBlock);
                    var odDr = ResetDateWithOffDay(getBlockLsd, _offDays);
                    if (odDr != null)
                    {
                        Maxdate = getBlockLsd;
                        OffDay = true;
                        OffDayType = odDr[nameof(OffDayType)].ToString();
                        Line = lineId;
                    }
                    else
                    {
                        Maxdate = getBlockLsd;
                        OffDay = false;
                        OffDayType = null;
                        Line = lineId;
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private bool HasLineExist(string[] dtLines, string lineId)
        {
            var hasLine = false;
            if (dtLines != null)
            {
                foreach (var item in dtLines)
                {
                    if (item == lineId)
                    {
                        hasLine = true;
                        break;
                    }
                }
            }
            return hasLine;
        }

        //************//
        public void GetLineForCDExceed(string productionRef, string ourStyleId, string[] dtLines)
        {
            try
            {
                var wcPreference = new DataView(_dtWcPreference)
                {
                    RowFilter = "ProductionBatchMasterId='" + productionRef + "'"
                };
                var _wcPreference = wcPreference.ToTable();
                if (_wcPreference != null && _wcPreference.Rows.Count > 0)
                    SetLineByPreferenceForCDExceed(_wcPreference, productionRef, ourStyleId);
                else
                {
                    SetLineForCDExceed(productionRef, ourStyleId);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void SetLineByPreferenceForCDExceed(DataTable _wcPreference, string productionRef, string ourStyleId)
        {
            for (int i = 0; i < _wcPreference.Rows.Count; i++)// prf different same line
            {
                var dv = new DataView(_dtBlock)
                {
                    RowFilter = "LineId='" + _wcPreference.Rows[i]["WorkCenterMasterId"] + "' AND OurStyleId='" + ourStyleId + "'"
                };
                if (dv.Count > 0)
                {
                    ReturnPreferdLineForCDExceed(dv);
                    break;
                }
                else
                {
                    dv.RowFilter = null;
                    dv = new DataView(_dtBlock)
                    {
                        RowFilter = "LineId='" + _wcPreference.Rows[i]["WorkCenterMasterId"] + "'"
                    };
                    if (dv.Count > 0)
                    {
                        ReturnPreferdLineForCDExceed(dv);
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(Line))
            {
                var dv = new DataView(_dtBlock) { RowFilter = "ProductionBatchMasterId='" + productionRef + "'" };

                //DataTable dtLines = dv.ToTable(true, "LineId");
                SetLineForCDExceed(productionRef, ourStyleId);
            }
        }

        private void ReturnPreferdLineForCDExceed(DataView dv)
        {
            var _drCurrentBlock = dv[0].Row;
            var blockMaxDate = GetMaxDate(dv.ToTable());
            var getBlockLsd = BlockStartingBasedOnLsd(blockMaxDate, Convert.ToDateTime(_drCurrentBlock["Lsd"].ToString()));
            //DateTime getBlockLsd = BlockStartingBasedOnLsd(blockMaxDate, _drCurrentBlock);
            var calculatedCD = getBlockLsd.AddDays((_drCurrentBlock["RunningDay"].ToString()).ToInt() - 1);
            var totalOffDay = TotalOffDay(getBlockLsd.ToString(), calculatedCD.ToString(), _offDays);
            calculatedCD = CDincreaseByOffDays(calculatedCD, totalOffDay, _offDays);
            CreateNewBlockByOffDay(getBlockLsd, calculatedCD, _offDays, _drCurrentBlock);
            var odDr = ResetDateWithOffDay(getBlockLsd, _offDays);
            if (odDr != null)
            {
                Maxdate = getBlockLsd;
                OffDay = true;
                OffDayType = odDr[nameof(OffDayType)].ToString();
                Line = (dv[0]["LineId"]).ToString();
            }
            else
            {
                Maxdate = getBlockLsd;
                OffDay = false;
                OffDayType = null;
                Line = (dv[0]["LineId"]).ToString();
            }
        }

        private void SetLineForCDExceed(string productionRef, string ourStyleId)
        {
            try
            {
                var dv = new DataView(_dtBlock)
                {
                    RowFilter = "OurStyleId='" + ourStyleId + "' AND LineId<>''"
                };
                var dtLineWise = new DataView(dv.ToTable()).ToTable(true, "LineId");
                for (int i = 0; i < dtLineWise.Rows.Count; i++)
                {
                    dv.RowFilter = null;
                    dv = new DataView(_dtBlock)
                    {
                        RowFilter = "OurStyleId='" + ourStyleId + "' AND LineId='" + dtLineWise.Rows[i]["LineId"] + "' AND LineId<>''"
                    };
                    if (dv.Count > 0)
                        ReturnPreferdLineForCDExceed(dv);

                    var dvWC = new DataView(_dtWC)
                    {
                        RowFilter = "ProductionBatchMasterId='" + productionRef + "' AND Id='" + Line + "'"
                    };
                    if (dvWC.Count > 0)
                        break;
                    else
                        Line = "";
                }
                if (string.IsNullOrEmpty(Line))
                {
                    dv.RowFilter = null;
                    var dvWC = new DataView(_dtWC) { RowFilter = "ProductionBatchMasterId='" + productionRef + "'" };
                    dv = new DataView(_dtBlock) { RowFilter = "LineId<>''" };
                    var dtLineId = new DataView(dv.ToTable()).ToTable(true, "LineId");
                    for (int i = 0; i < dvWC.Count; i++)
                    {
                        for (int j = 0; j < dtLineId.Rows.Count; j++)
                        {
                            if (dvWC[i]["Id"].ToString() == dtLineId.Rows[j]["LineId"].ToString())
                            {
                                dv.RowFilter = null;
                                dv = new DataView(_dtBlock) { RowFilter = "LineId='" + dvWC[i]["Id"]+ "'" };
                                ReturnPreferdLineForCDExceed(dv);
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(Line))
                            break;
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #endregion Line Setup

        public void CreateNewBlockByOffDay(DateTime blockStratDate, DateTime blockRunningDate, DataSet _offDays, DataRow lastDr)
        {
            try
            {
                var offDayDv = new DataView(_offDays.Tables[0])
                {
                    RowFilter = "OffDayDate >='" + blockStratDate + "' AND OffDayDate <='" + blockRunningDate + "'"
                };
                var seqDv = new DataView(_dtBlock)
                {
                    RowFilter = "ProductionBatchMasterId='" + lastDr["ProductionBatchMasterId"] + "'"
                };
                var seqDt = seqDv.ToTable();
                var getLastSequence = Convert.ToInt32(seqDt.Rows[seqDt.Rows.Count - 1]["Sequence"].ToString());
                if (offDayDv.Count > 0)
                {
                    DataRow newDr = null;
                    for (int i = 0; i < offDayDv.Count; i++)
                    {
                        newDr = _dtBlock.NewRow();
                        newDr["Id"] = Guid.NewGuid();
                        newDr["PlantId"] = lastDr["PlantId"];
                        newDr["LineId"] = "";
                        newDr["EntityId"] = lastDr["EntityId"];
                        newDr["ProductionBatchMasterId"] = lastDr["ProductionBatchMasterId"];
                        newDr["OurStyleId"] = lastDr["OurStyleId"];
                        newDr["Lsd"] = lastDr["Lsd"];
                        newDr["CommitmentDate"] = lastDr["CommitmentDate"];
                        newDr["RunningDay"] = lastDr["RunningDay"];
                        newDr["Date"] = DBNull.Value;
                        newDr["TotalQty"] = lastDr["TotalQty"];
                        newDr["DailyOutPut"] = lastDr["DailyOutPut"];
                        newDr["StandardDailyOutPut"] = lastDr["StandardDailyOutPut"];
                        newDr["LearningCurveOutPut"] = lastDr["LearningCurveOutPut"];
                        newDr["HasLearningCurve"] = false;
                        newDr["IsFreeze"] = false;
                        newDr[nameof(OffDay)] = false;
                        newDr[nameof(OffDayType)] = DBNull.Value;
                        newDr["Sequence"] = getLastSequence + 1 + i;
                        newDr["MinAllocatedLine"] = lastDr["MinAllocatedLine"];
                        newDr["IncrementType"] = lastDr["IncrementType"];
                        newDr["IncrementValue"] = lastDr["IncrementValue"];
                        newDr["StandardTime"] = lastDr["StandardTime"];
                        newDr["MinRequiredTargetHourly"] = lastDr["MinRequiredTargetHourly"];
                        newDr["DaysToGetTheTarget"] = lastDr["DaysToGetTheTarget"];
                        newDr["FirstDayOutPut"] = lastDr["FirstDayOutPut"];
                        newDr["MinWorkingDays"] = lastDr["MinWorkingDays"];
                        newDr["IsDb"] = false;
                        newDr["Color"] = null;
                        _dtBlankBlock.Rows.Add(newDr.ItemArray);
                        _dtBlock.Rows.Add(newDr);
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        //private bool IsGapExist(DataRow currentBlock, string line)
        //{
        //    try
        //    {
        //        int addDays = 0;
        //        DateTime currentLsd = Convert.ToDateTime(_drCurrentBlock["Lsd"].ToString());
        //        DateTime currentCD = Convert.ToDateTime(_drCurrentBlock["CommitmentDate"].ToString());
        //        int runningDay = (_drCurrentBlock["RunningDay"].ToString()).ToInt();
        //        while (currentLsd <= (currentCD.AddDays(-runningDay)))
        //        {
        //            if (dicDateList.ContainsKey(line))//if line exist
        //            {
        //                List<int> dateList = dicDateList[line];
        //                int lsdFromList = Convert.ToInt32(currentLsd.ToString("yyMMdd"));
        //                if (!dateList.Contains(lsdFromList))//date is blank
        //                {
        //                    if (addDays == 0)
        //                        maxdate = currentLsd;
        //                    addDays++;
        //                    currentLsd.AddDays(1);
        //                }
        //            }
        //        }//while
        //        if (addDays >= runningDay)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch (CustomException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
        //    }
        //}
    }

    public class CommonFunction
    {
        public Dictionary<string, List<int>> dicDateList = new Dictionary<string, List<int>>();

        public DateTime GetMaxDate(DataTable dt)
        {
            try
            {
                var maxDate = DateTime.Now;
                if (dt.Rows.Count > 0)
                {
                    maxDate = Convert.ToDateTime(dt.Compute("MAX(Date)", null));
                    maxDate = maxDate.AddDays(1).Date;
                }
                return maxDate.Date;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public static DateTime BlockStartingBasedOnLsd(DateTime maxDateInLine, DateTime blocRowkLsd)
        {
            try
            {
                return maxDateInLine == blocRowkLsd || maxDateInLine > blocRowkLsd ? maxDateInLine.Date : blocRowkLsd.Date;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(null, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public int CalculateLearningCurveOutPut(DataRow drPref)
        {
            try
            {
                var dailyOutput = Convert.ToInt32(drPref["StandardTime"].ToString()) * Convert.ToInt32(drPref["MinRequiredTargetHourly"].ToString());
                var firstDayOutPut = Convert.ToInt32(drPref["StandardTime"].ToString()) * Convert.ToInt32(drPref["FirstDayOutPut"].ToString());
                var incrementValue = Convert.ToInt32(Convert.ToDecimal(drPref["IncrementValue"].ToString()));
                var daysToGetTheTarget = Convert.ToInt32(drPref["DaysToGetTheTarget"].ToString());
                if (daysToGetTheTarget > 0)
                {
                    var iv = incrementValue;
                    if (drPref["IncrementType"].ToString() != "Fixed")
                    {
                        var c = firstDayOutPut;
                        iv = iv * c / 100;
                    }
                    else
                        iv = (iv * Convert.ToInt32(drPref["StandardTime"].ToString()));//daily iv in fixed
                    //iv = (iv * Convert.ToInt32(drPref["StandardTime"].ToString()));//daily iv
                    var _days = 1;
                    var totalCumulative = firstDayOutPut;
                    var _cumi_output = firstDayOutPut;
                    while (_cumi_output < dailyOutput)
                    {
                        _cumi_output += iv;
                        _days++;
                        if (_cumi_output > dailyOutput)
                            totalCumulative += dailyOutput;
                        else
                            totalCumulative += _cumi_output;
                        if (iv <= 0)
                        {
                            _days = 0;
                            break;
                        }
                    }
                    return totalCumulative;
                }
                else
                    return 0;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public int RunningDays(int totalQty, int learningCurveQty, int daysToGetTheTarget, int dailyOutput, int minLine)
        {
            try
            {
                decimal qty = (totalQty - (learningCurveQty * minLine));
                var remainingQty = qty / dailyOutput;
                var remainingDays = (int)Math.Ceiling(remainingQty) + (daysToGetTheTarget * minLine);
                return remainingDays;
                //int newDays = (int)Math.Ceiling((Convert.ToDecimal(remainingDays)));
                //var iv = (Convert.ToDecimal(dueDays) - newDays);
                //if (iv > Convert.ToDecimal(.0099))
                //    return newDays + 1;
                //else
                //    return newDays;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public DataRow ResetDateWithOffDay(DateTime blocKLsd, DataSet _offDays)
        {
            try
            {
                var offDayDv = new DataView(_offDays.Tables[0])
                {
                    RowFilter = "OffDayDate ='" + blocKLsd.ToString() + "'"
                };
                return offDayDv.Count > 0 ? offDayDv[0].Row : null;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public int TotalOffDay(string lsd, string cd, DataSet _offDays)
        {
            try
            {
                var offDayDv = new DataView(_offDays.Tables[0])
                {
                    RowFilter = "OffDayDate >='" + lsd + "' AND OffDayDate <='" + cd + "'"
                };
                return offDayDv.Count > 0 ? offDayDv.Count : 0;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public bool HasNoMinWorkDaysInLine(DataTable _dt, int minWorkingDays)
        {
            try
            {
                var hasWorkingDays = false;
                var maxSequence = Convert.ToInt32(_dt.Compute("MAX(Sequence)", null));
                var _dv = new DataView(_dt) { RowFilter = "Sequence=" + maxSequence };
                var lineId = _dv[0]["LineId"].ToString();
                //int minWorkingDays = Convert.ToInt32(_dv[0]["MinWorkingDays"].ToString());
                var _dv2 = new DataView(_dt) { RowFilter = "LineId='" + lineId + "'" };
                if (_dv2.Count >= minWorkingDays)
                    hasWorkingDays = true;
                return hasWorkingDays;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public bool HasNoMoreLine(DataTable _dt, int minLine)
        {
            try
            {
                var noLines = false;
                var maxSequence = Convert.ToInt32(_dt.Compute("MAX(Sequence)", null));
                var _dv = new DataView(_dt) { RowFilter = "Sequence=" + maxSequence };
                var lineId = _dv[0]["LineId"].ToString();
                var _dv2 = new DataView(_dt) { RowFilter = "LineId='" + lineId + "'" };//already line set
                if (_dv2.Count >= minLine)
                    noLines = true;
                return noLines;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public int RemainingWorkingDdays(DataTable _dt, int minLine, int remainingLine)
        {
            try
            {
                var totalDOP = Convert.ToInt32(_dt.Compute("SUM(DailyOutPut)", null));
                var totalQty = Convert.ToInt32(_dt.Rows[0]["TotalQty"].ToString());
                var stdTime = Convert.ToInt32(_dt.Rows[0]["StandardTime"].ToString());
                var hrsTarget = Convert.ToInt32(_dt.Rows[0]["MinRequiredTargetHourly"].ToString());
                var daysTarget = Convert.ToInt32(_dt.Rows[0]["DaysToGetTheTarget"].ToString());
                decimal lcOutPut = Convert.ToInt32(_dt.Rows[0]["LearningCurveOutPut"].ToString());//each line lc prd
                var remainingLCQty = (int)Math.Ceiling(lcOutPut) * remainingLine;//each remaining line lc prd
                var remainingLCDays = daysTarget * remainingLine;//total lc days in remaining line
                double remainingQty = totalQty - totalDOP - remainingLCQty;
                //int remainingDays = Convert.ToInt32(Math.Round(remainingQty / (hrsTarget * stdTime), 0));
                var remainingDays = (int)Math.Ceiling(remainingQty / (hrsTarget * stdTime));
                remainingDays += remainingLCDays;
                return remainingDays;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public int GetMinmumLine(DataTable wc, int minAllocatedLine)
        {
            try
            {
                var minLines = minAllocatedLine;
                if (wc.Rows.Count < minAllocatedLine)// if all wc (for this p.Ref) less then min allocated lines
                    minLines = wc.Rows.Count;
                return minLines;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        /// <summary>
        /// lcOPHrs=1stOP + individualOP
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int GetPerDayLCPrd(DataTable dtBlock, string prf, string line)//each day lc prd n line
        {
            try
            {
                var dailyOutput = 0;
                var dv = new DataView(dtBlock) { RowFilter = "ProductionBatchMasterId='" + prf + "'" };
                var _dtBlock = dv.ToTable();

                var dvLine = new DataView(dtBlock) { RowFilter = "ProductionBatchMasterId='" + prf + "' AND LineId='" + line + "'AND OffDay=0" };
                var standardTime = _dtBlock.Rows[0]["StandardTime"].ToString().ToInt();
                var firstDayOP = standardTime * _dtBlock.Rows[0]["FirstDayOutPut"].ToString().ToInt();
                var incrementType = _dtBlock.Rows[0]["IncrementType"].ToString();
                var incrementValue = Convert.ToInt32(Convert.ToDecimal(_dtBlock.Rows[0]["incrementValue"].ToString()));
                var dtCount = 0;

                var daysToGetTheTarget = Convert.ToInt32(_dtBlock.Rows[0]["DaysToGetTheTarget"].ToString());
                if ((dvLine.Count + 1) <= daysToGetTheTarget)
                {
                    dtCount = dvLine.Count;

                    //if (dtCount != 0)
                    //    dtCount -= 1;
                    if (incrementType != "Fixed")
                    {
                        var dailyIncrement = (incrementValue / 100.0M) * standardTime;
                        dailyOutput = firstDayOP + (dtCount * (firstDayOP * Convert.ToInt32(Convert.ToDecimal(dailyIncrement))));
                        return dailyOutput;
                    }
                    else
                    {
                        dailyOutput = firstDayOP + (dtCount * (incrementValue * standardTime));
                        return dailyOutput;
                    }
                }
                else
                {
                    dailyOutput = Convert.ToInt32(_dtBlock.Rows[0]["StandardTime"].ToString()) * Convert.ToInt32(_dtBlock.Rows[0]["MinRequiredTargetHourly"].ToString());
                    return dailyOutput;
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        /// <summary>
        /// if get off day cd will be increase
        /// </summary>
        /// <param name="cd"></param>
        /// <param name="totalOffday"></param>
        /// <param name="_offDays"></param>
        /// <returns></returns>
        public DateTime CDincreaseByOffDays(DateTime cd, int totalOffday, DataSet _offDays)
        {
            try
            {
                if (totalOffday > 0)
                {
                    for (int i = 0; i < totalOffday; i++)
                    {
                        var isOffDay = true;
                        while (isOffDay)
                        {
                            cd = cd.AddDays(1);
                            var offDayDv = new DataView(_offDays.Tables[0])
                            {
                                RowFilter = "OffDayDate='" + cd + "'"
                            };
                            if (offDayDv.Count == 0)
                                isOffDay = false;
                        }
                    }
                }
                return cd.Date;//cd increase
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        //************************************************************************************//

        #region Table Create

        private DataTable CreateTable()
        {
            DataTable dt = null;
            try
            {
                string[] columnName = { "Id", "EntityId", "PlantId", "LineId", "ProcessId", "ProductionBatchMasterId", "OurStyleId", "OffDayType", "IncrementType", "Color" };
                dt = new DataTable();
                foreach (var item in columnName)
                {
                    dt.Columns.Add(new DataColumn(item));
                }
                dt.Columns.Add(new DataColumn("RunningDay", typeof(int)));
                dt.Columns.Add(new DataColumn("MinAllocatedLine", typeof(int)));
                dt.Columns.Add(new DataColumn("IncrementValue", typeof(decimal)));
                dt.Columns.Add(new DataColumn("StandardTime", typeof(int)));
                dt.Columns.Add(new DataColumn("DaysToGetTheTarget", typeof(int)));
                dt.Columns.Add(new DataColumn("FirstDayOutPut", typeof(int)));
                dt.Columns.Add(new DataColumn("MinRequiredTargetHourly", typeof(int)));
                dt.Columns.Add(new DataColumn("MinWorkingDays", typeof(int)));
                dt.Columns.Add(new DataColumn("HasLearningCurve", typeof(bool)));
                dt.Columns.Add(new DataColumn("OffDay", typeof(bool)));
                dt.Columns.Add(new DataColumn("IsFreeze", typeof(bool)));
                dt.Columns.Add(new DataColumn("IsDb", typeof(bool)));

                dt.Columns.Add(new DataColumn("TotalQty", typeof(int)));
                dt.Columns.Add(new DataColumn("DailyOutPut", typeof(int)));
                dt.Columns.Add(new DataColumn("StandardDailyOutPut", typeof(int)));
                dt.Columns.Add(new DataColumn("LearningCurveOutPut", typeof(int)));

                dt.Columns.Add(new DataColumn("Lsd", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("CommitmentDate", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Date", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("Sequence", typeof(Int32)));
                return dt;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public DataTable CreateBlock(DataSet _dbBlockDs, string processId)
        {
            try
            {
                var dtBlock = CreateTable();
                if (_dbBlockDs != null && _dbBlockDs.Tables[0].Rows.Count > 0)
                {
                    DataRow newDr = null;
                    for (int i = 0; i < _dbBlockDs.Tables[0].Rows.Count; i++)
                    {
                        newDr = dtBlock.NewRow();
                        newDr["Id"] = _dbBlockDs.Tables[0].Rows[i]["Id"];
                        newDr["MinAllocatedLine"] = _dbBlockDs.Tables[0].Rows[i]["MinAllocatedLine"];
                        newDr["IncrementType"] = _dbBlockDs.Tables[0].Rows[i]["IncrementType"];
                        newDr["IncrementValue"] = _dbBlockDs.Tables[0].Rows[i]["IncrementValue"];
                        newDr["StandardTime"] = _dbBlockDs.Tables[0].Rows[i]["StandardTime"];
                        newDr["DaysToGetTheTarget"] = _dbBlockDs.Tables[0].Rows[i]["DaysToGetTheTarget"];
                        newDr["FirstDayOutPut"] = _dbBlockDs.Tables[0].Rows[i]["FirstDayOutPut"];
                        newDr["MinRequiredTargetHourly"] = _dbBlockDs.Tables[0].Rows[i]["MinRequiredTargetHourly"];
                        newDr["EntityId"] = _dbBlockDs.Tables[0].Rows[i]["EntityId"];
                        newDr["ProcessId"] = processId;
                        newDr["PlantId"] = _dbBlockDs.Tables[0].Rows[i]["PlantId"];
                        newDr["LineId"] = _dbBlockDs.Tables[0].Rows[i]["LineId"];
                        newDr["ProductionBatchMasterId"] = _dbBlockDs.Tables[0].Rows[i]["ProductionBatchMasterId"];
                        newDr["OurStyleId"] = _dbBlockDs.Tables[0].Rows[i]["OurStyleId"];
                        newDr["RunningDay"] = _dbBlockDs.Tables[0].Rows[i]["RunningDay"];
                        newDr["MinWorkingDays"] = _dbBlockDs.Tables[0].Rows[i]["MinWorkingDays"];
                        newDr["TotalQty"] = _dbBlockDs.Tables[0].Rows[i]["TotalQty"];
                        newDr["DailyOutPut"] = _dbBlockDs.Tables[0].Rows[i]["DailyOutPut"];
                        newDr["StandardDailyOutPut"] = _dbBlockDs.Tables[0].Rows[i]["StandardDailyOutPut"];
                        newDr["LearningCurveOutPut"] = _dbBlockDs.Tables[0].Rows[i]["LearningCurveOutPut"];
                        newDr["HasLearningCurve"] = _dbBlockDs.Tables[0].Rows[i]["HasLearningCurve"];
                        newDr["IsFreeze"] = _dbBlockDs.Tables[0].Rows[i]["IsFreeze"];
                        newDr["OffDayType"] = _dbBlockDs.Tables[0].Rows[i]["OffDayType"];
                        newDr["OffDay"] = _dbBlockDs.Tables[0].Rows[i]["OffDay"];
                        newDr["Lsd"] = _dbBlockDs.Tables[0].Rows[i]["Lsd"];
                        newDr["CommitmentDate"] = _dbBlockDs.Tables[0].Rows[i]["CommitmentDate"];
                        newDr["Date"] = _dbBlockDs.Tables[0].Rows[i]["Date"];
                        newDr["Sequence"] = _dbBlockDs.Tables[0].Rows[i]["Sequence"];
                        newDr["IsDb"] = _dbBlockDs.Tables[0].Rows[i]["IsDb"];
                        dtBlock.Rows.Add(newDr);
                    }
                }
                return dtBlock;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #endregion Table Create

        public string ReturnStringArray(ArrayList arrayList)
        {
            var str = "";
            str = arrayList != null && arrayList.Count > 0 ? string.Join(",", arrayList.ToArray().Select(item => "'" + item + "'")) : "' '";
            return str;
        }

        public void IfDateLessThenCDInLine(DateTime cd, int prefTotalQty, ref DataTable dt2Temp, int dailyQty, ArrayList countCD, ref int tQty, DataSet _offDays)
        {
            var isQtyLess = true;
            while (isQtyLess)
            {
                for (int i = 0; i < dt2Temp.Rows.Count; i++)
                {
                    var blockEndDate = Convert.ToDateTime(dt2Temp.Rows[i]["EndDate"].ToString());
                    var calculatedCD = CDincreaseByOffDays(blockEndDate, 1, _offDays);
                    if (calculatedCD <= cd)
                    {
                        var dr = dt2Temp.Rows[i];
                        dr.BeginEdit();
                        dr["EndDate"] = calculatedCD;
                        dr["TotalQty"] = Convert.ToInt32(dt2Temp.Rows[i]["TotalQty"]) + dailyQty;
                        dr["IsSetable"] = 0;
                        dr.EndEdit();
                        tQty += dailyQty;
                    }
                    else
                    {
                        if (!countCD.Contains(dt2Temp.Rows[i]["LineId"]))
                            countCD.Add(dt2Temp.Rows[i]["LineId"]);
                        if (dt2Temp.Rows.Count == countCD.Count)
                        {
                            isQtyLess = false;
                            break;
                        }
                    }
                    if (tQty >= prefTotalQty)
                    {
                        isQtyLess = false;
                        break;
                    }
                }
            }
        }

        public void IfDateCrossThenCDInLine(int prefTotalQty, ref DataTable dt2Temp, int dailyQty, ref int tQty, DataSet _offDays)
        {
            var isQtyLess = true;
            while (isQtyLess)
            {
                for (int i = 0; i < dt2Temp.Rows.Count; i++)
                {
                    var blockEndDate = Convert.ToDateTime(dt2Temp.Rows[i]["EndDate"].ToString());
                    var calculatedCD = CDincreaseByOffDays(blockEndDate, 1, _offDays);
                    var dr = dt2Temp.Rows[i];
                    dr.BeginEdit();
                    dr["EndDate"] = calculatedCD;
                    dr["TotalQty"] = Convert.ToInt32(dt2Temp.Rows[i]["TotalQty"]) + dailyQty;
                    dr["IsSetable"] = 0;
                    dr.EndEdit();
                    tQty += dailyQty;
                    if (tQty >= prefTotalQty)
                    {
                        isQtyLess = false;
                        break;
                    }
                }
            }
        }

        //public int TotalLCinTemp(DataTable dtTemp)
        //{
        //    try
        //    {
        //        DataView dv = new DataView(dtTemp) { RowFilter = "IsLC=true" };
        //        return dv.Count;
        //    }
        //    catch (CustomException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
        //    }
        //}
        public void GetTopNRowFromDataTable(ref DataTable dtNewTemp, DataTable dtFilterTemp, int lineInHand, DataRow drPref)
        {
            try
            {
                if (dtNewTemp.Rows.Count == 0)
                    dtNewTemp = dtFilterTemp.Clone();
                var cd = Convert.ToDateTime(drPref["CommitmentDate"]);
                var dtEndDate = DateTime.Now.Date;
                var count = 0;
                var loop = 0;
                loop = dtFilterTemp.Rows.Count >= lineInHand ? lineInHand : dtFilterTemp.Rows.Count;
                for (int i = 0; i < dtFilterTemp.Rows.Count; i++)
                {
                    count++;
                    dtEndDate = Convert.ToDateTime(dtFilterTemp.Rows[i]["EndDate"]);
                    if (dtEndDate <= cd)
                        dtNewTemp.ImportRow(dtFilterTemp.Rows[i]);
                    if (count == loop) break;
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void GetTopAnyNRow(ref DataTable dtNewTemp, DataTable dtFilterTemp, int lineInHand, DataRow drPref)
        {
            try
            {
                dtNewTemp = dtFilterTemp.Clone();
                var cd = Convert.ToDateTime(drPref["CommitmentDate"]);
                var dtEndDate = DateTime.Now.Date;
                var loop = 0;
                loop = dtFilterTemp.Rows.Count >= lineInHand ? lineInHand : dtFilterTemp.Rows.Count;
                for (int i = 0; i < dtFilterTemp.Rows.Count; i++)
                {
                    dtEndDate = Convert.ToDateTime(dtFilterTemp.Rows[i]["EndDate"]);
                    dtNewTemp.ImportRow(dtFilterTemp.Rows[i]);
                    if (dtNewTemp.Rows.Count == loop) break;
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public int GetPerDayPrd(DataTable dtBlock, DataRow drPref, string line, bool isLc)//each day prd n line
        {
            try
            {
                var dailyOutput = 0;
                var standardTime = drPref["StandardTime"].ToString().ToInt();
                var minHourlyTarget = drPref["MinRequiredTargetHourly"].ToString().ToInt();
                var firstDayOP = standardTime * drPref["FirstDayOutPut"].ToString().ToInt();
                var incrementType = drPref["IncrementType"].ToString();
                var incrementValue = Convert.ToInt32(Convert.ToDecimal(drPref["incrementValue"].ToString()));
                var daysToGetTheTarget = Convert.ToInt32(drPref["DaysToGetTheTarget"].ToString());
                var dtCount = 0;
                if (!isLc)
                {
                    dailyOutput = standardTime * minHourlyTarget;
                    return dailyOutput;
                }
                var dvLine = new DataView(dtBlock) { RowFilter = "ProductionBatchMasterId='" + drPref["Id"] + "' AND LineId='" + line + "'AND OffDay=0" };
                if ((dvLine.Count + 1) <= daysToGetTheTarget)
                {
                    dtCount = dvLine.Count;// return total days
                    if (incrementType != "Fixed")
                    {
                        var dailyIncrement = (incrementValue / 100.0M) * standardTime;
                        dailyOutput = firstDayOP + (dtCount * (drPref["FirstDayOutPut"].ToString().ToInt() * Convert.ToInt32(Convert.ToDecimal(dailyIncrement))));
                        if (dailyOutput > (standardTime * minHourlyTarget))
                            return (standardTime * minHourlyTarget);
                        return dailyOutput;
                    }
                    else
                    {
                        dailyOutput = firstDayOP + (dtCount * (incrementValue * standardTime));
                        if (dailyOutput > (standardTime * minHourlyTarget))
                            return (standardTime * minHourlyTarget);
                        return dailyOutput;
                    }
                }
                else
                {
                    var dv2 = new DataView(dtBlock) { RowFilter = "ProductionBatchMasterId='" + drPref["Id"] + "' AND OffDay=0" };
                    var tQty = Convert.ToInt32(dv2.ToTable().Compute("SUM(DailyOutPut)", null));
                    var remaningQty = drPref["Qty"].ToString().ToInt() - tQty;
                    dailyOutput = remaningQty > (standardTime * minHourlyTarget) ? standardTime * minHourlyTarget : remaningQty;
                    return dailyOutput;
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }
    }

    public class DistributedLineDate : CommonFunction
    {
        private DataSet dsPrdRef;
        private DataSet _offDays;
        private DataSet _dbBlockDs;
        private DataTable dtBlock;
        private DataTable _dtWcPreference;
        private DataTable _dtWorkCenter;
        private string processId;
        private readonly ISqlRepository _sqlRepository;

        public DataTable DtBlock
        {
            get { return dtBlock; }
        }

        public DistributedLineDate(
            DataSet Source
            , DataSet offDays
            , DataSet dbBlockDs
            , DataTable dtWcPreference
            , DataTable dtWorkCenter
            , string _processId
            )
        {
            dsPrdRef = Source;
            _offDays = offDays;
            _dbBlockDs = dbBlockDs;
            _dtWcPreference = dtWcPreference;
            _dtWorkCenter = dtWorkCenter;
            processId = _processId;
            _sqlRepository = new SqlRepository();
        }

        public void CreateDtTable()
        {
            try
            {
                dtBlock = CreateBlock(_dbBlockDs, processId);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public DataTable SteLineMain(DateTime toDate)
        {
            try
            {
                var dtPref = dsPrdRef.Tables[0];
                if (dtPref.Rows.Count > 0)
                {
                    var dtTemp = new DataTable();
                    var dtNewTemp = new DataTable();
                    for (int i = 0; i < dtPref.Rows.Count; i++)
                    {
                        dtTemp = null;
                        dtNewTemp = null;
                        dtTemp = FilterRelatedLineWithMaxDate(dtPref.Rows[i]);
                        dtNewTemp = SetLineInTempList(dtTemp, dtPref.Rows[i]);
                        if (dtNewTemp.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtNewTemp.Rows.Count; j++)
                            {
                                BlockCreate(dtNewTemp.Rows[j], dtPref.Rows[i]);
                            }
                        }
                        var dv = new DataView(dtBlock)
                        { RowFilter = "ProductionBatchMasterId='" + dtPref.Rows[i]["Id"] + "' AND OffDay=0" };
                        if (dv.Count > 0)
                        {
                            for (int k = 0; k < dv.Count; k++)
                            {
                                var dr = dv[k].Row;
                                dr.BeginEdit();
                                dr["RunningDay"] = dv.Count;
                                dr.EndEdit();
                            }
                        }
                    }
                }
                HasWcSetInDtBlock(dtBlock, toDate);// if more wc present but not in dtBlock.Then add them in dtBlock.
                return dtBlock;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void BlockCreate(DataRow drTemp, DataRow drPref)
        {
            try
            {
                var sDate = Convert.ToDateTime(drTemp["StartDate"]);
                var eDate = Convert.ToDateTime(drTemp["EndDate"]);
                var isLc = Convert.ToBoolean(drTemp["IsLC"]);
                var standardTime = drPref["StandardTime"].ToString().ToInt();
                var hourlyTarget = Convert.ToInt32(drPref["MinRequiredTargetHourly"].ToString());
                var isOffDay = false;
                var OffDayType = "";
                //int totalOffDay = TotalOffDay(sDate.ToString(), eDate.ToString(), _offDays);
                var dateDiff = Convert.ToInt32((eDate - sDate.AddDays(-1)).TotalDays);
                for (int i = 0; i < dateDiff; i++)
                {
                    IsOffDay(sDate, out isOffDay, out OffDayType);
                    BlockAddInMainList(drTemp, drPref, sDate, isOffDay, OffDayType, i);
                    sDate = sDate.AddDays(1);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private void BlockAddInMainList(DataRow drTemp, DataRow drPref, DateTime sDate, bool isOffDay, string OffDayType, int sequence)
        {
            var dailyOutPut = 0;
            if (!isOffDay)
                dailyOutPut = GetPerDayPrd(dtBlock, drPref, drTemp["LineId"].ToString(), Convert.ToBoolean(drTemp["IsLC"]));
            var hasLcCurve = false;
            if (Convert.ToBoolean(drTemp["IsLC"]))
            {
                var dvLine = new DataView(dtBlock)
                { RowFilter = "ProductionBatchMasterId='" + drPref["Id"] + "' AND LineId='" + drTemp["LineId"]+ "'AND OffDay=0" };
                if ((dvLine.Count + 1) <= Convert.ToInt32(drPref["DaysToGetTheTarget"].ToString()))
                    hasLcCurve = true;
            }
            var newDr = dtBlock.NewRow();
            newDr["Id"] = Guid.NewGuid();
            newDr["PlantId"] = drPref["PlantId"];
            newDr["LineId"] = drTemp["LineId"];
            newDr["EntityId"] = drPref["EntityId"];
            newDr["ProcessId"] = processId;
            newDr["ProductionBatchMasterId"] = drPref["Id"];
            newDr["OurStyleId"] = drPref["OurStyleId"];
            newDr["Lsd"] = drPref["Lsd"];
            newDr["CommitmentDate"] = drPref["CommitmentDate"];
            newDr["RunningDay"] = 0;
            newDr["Date"] = sDate;
            newDr["TotalQty"] = drPref["Qty"];
            newDr["DailyOutPut"] = dailyOutPut;
            newDr["StandardDailyOutPut"] = Convert.ToInt32(drPref["StandardTime"]) * Convert.ToInt32(drPref["MinRequiredTargetHourly"]);
            newDr["LearningCurveOutPut"] = CalculateLearningCurveOutPut(drPref);
            newDr["HasLearningCurve"] = hasLcCurve;
            newDr["IsFreeze"] = false;
            newDr["OffDay"] = isOffDay;
            newDr[nameof(OffDayType)] = OffDayType;
            newDr["Sequence"] = sequence;
            newDr["MinAllocatedLine"] = drPref["MinAllocatedLine"];
            newDr["IncrementType"] = drPref["IncrementType"];
            newDr["IncrementValue"] = drPref["IncrementValue"];
            newDr["StandardTime"] = drPref["StandardTime"];
            newDr["MinRequiredTargetHourly"] = drPref["MinRequiredTargetHourly"];
            newDr["DaysToGetTheTarget"] = drPref["DaysToGetTheTarget"];
            newDr["FirstDayOutPut"] = drPref["FirstDayOutPut"];
            newDr["MinWorkingDays"] = drPref["MinWorkingDays"];
            newDr["IsDb"] = false;
            newDr["Color"] = null;
            dtBlock.Rows.Add(newDr);
        }

        /// <summary>
        /// has another wc which is absence in dtBlock.
        /// </summary>
        /// <param name="dtBlock"></param>
        private void HasWcSetInDtBlock(DataTable dtBlock, DateTime toDate)
        {
            try
            {
                var plantId = dtBlock.Rows[0]["PlantId"].ToString();
                var arrayList = new ArrayList();
                for (int i = 0; i < dtBlock.Rows.Count; i++)
                {
                    if (!arrayList.Contains(dtBlock.Rows[i]["LineId"]))
                        arrayList.Add(dtBlock.Rows[i]["LineId"].ToString());
                }
                var dsBlankWc = GetBlankWorkCenter(plantId, arrayList, processId); // get another wc from wc master which is absence in dtBlock.
                var dtBlankWc = dsBlankWc.Tables[0];
                if (dtBlankWc.Rows.Count > 0)
                {
                    var dv = new DataView(_offDays.Tables[0]) { RowFilter = "OffDayDate>='" + DateTime.Now.Date + "' AND OffDayDate<='" + toDate.Date + "'" };
                    for (int i = 0; i < dv.Count; i++)
                    {
                        for (int j = 0; j < dtBlankWc.Rows.Count; j++)
                        {
                            //wc and off day wise block in dtBlock
                            BlankWcBlockAddInMainList(dtBlankWc.Rows[j]["Id"].ToString(), plantId, Convert.ToDateTime(dv[i]["OffDayDate"]), dv[i]["OffDayType"].ToString(), 0);
                        }
                    }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        /// <summary>
        /// get another wc from wc master which is absence in dtBlock.
        /// </summary>
        /// <param name="dtBlock"></param>
        private DataSet GetBlankWorkCenter(string plantId, ArrayList arrayList, string processId)
        {
            try
            {
                var parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT Id FROM SCS.WorkCenterMaster AS WC
                            WHERE WC.Id NOT IN (" + ReturnStringArray(arrayList) + @") AND WC.PlantId='" + plantId + "' AND WC.ProcessId='" + processId + "'"
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void BlankWcBlockAddInMainList(string lineId, string plantId, DateTime sDate, string OffDayType, int sequence)
        {
            var newDr = dtBlock.NewRow();
            newDr["Id"] = Guid.NewGuid();
            newDr["PlantId"] = plantId;
            newDr["LineId"] = lineId;
            newDr["EntityId"] = "";
            newDr["ProcessId"] = processId;
            newDr["ProductionBatchMasterId"] = "";
            newDr["OurStyleId"] = "";
            newDr["Lsd"] = DateTime.Now.Date;
            newDr["CommitmentDate"] = DateTime.Now.Date;
            newDr["RunningDay"] = 0;
            newDr["Date"] = sDate;
            newDr["TotalQty"] = 0;
            newDr["DailyOutPut"] = 0;
            newDr["StandardDailyOutPut"] = 0;
            newDr["LearningCurveOutPut"] = 0;
            newDr["HasLearningCurve"] = false;
            newDr["IsFreeze"] = false;
            newDr["OffDay"] = true;
            newDr[nameof(OffDayType)] = OffDayType;
            newDr["Sequence"] = sequence;
            newDr["MinAllocatedLine"] = 0;
            newDr["IncrementType"] = "";
            newDr["IncrementValue"] = 0;
            newDr["StandardTime"] = 0;
            newDr["MinRequiredTargetHourly"] = 0;
            newDr["DaysToGetTheTarget"] = 0;
            newDr["FirstDayOutPut"] = 0;
            newDr["MinWorkingDays"] = 0;
            newDr["IsDb"] = false;
            newDr["Color"] = null;
            dtBlock.Rows.Add(newDr);
        }

        #region Distributed Date

        private DataTable WorKCenterInHand(string pRef)//return all line by pRef
        {
            try
            {
                var dvPreferenceLine = new DataView(_dtWcPreference) { RowFilter = "ProductionBatchMasterId='" + pRef + "'" };
                DataTable dtPrefMergeLine = null;
                DataRow newDr = null;
                string[] col = { "WorkCenterMasterId", "ProductionBatchMasterId", "PlantId" };
                dtPrefMergeLine = new DataTable();
                foreach (var item in col)
                {
                    dtPrefMergeLine.Columns.Add(new DataColumn(item));
                }
                dtPrefMergeLine.Columns.Add(new DataColumn("IsPreference", typeof(bool)));
                if (dvPreferenceLine.Count > 0)
                {
                    for (int p = 0; p < dvPreferenceLine.Count; p++)
                    {
                        newDr = dtPrefMergeLine.NewRow();
                        newDr["WorkCenterMasterId"] = dvPreferenceLine[p]["WorkCenterMasterId"];
                        newDr["ProductionBatchMasterId"] = dvPreferenceLine[p]["ProductionBatchMasterId"];
                        newDr["IsPreference"] = 1;
                        dtPrefMergeLine.Rows.Add(newDr);
                    }
                }
                var dvOthersLine = new DataView(_dtWorkCenter) { RowFilter = "ProductionBatchMasterId='" + pRef + "'" };
                if (dvOthersLine.Count > 0)
                {
                    DataView dvFilter = null;
                    for (int o = 0; o < dvOthersLine.Count; o++)
                    {
                        dvFilter = new DataView(dtPrefMergeLine) { RowFilter = "WorkCenterMasterId='" + dvOthersLine[o]["Id"] + "'" };
                        if (dvFilter.Count == 0)
                        {
                            newDr = dtPrefMergeLine.NewRow();
                            newDr["WorkCenterMasterId"] = dvOthersLine[o]["Id"];
                            newDr["ProductionBatchMasterId"] = dvOthersLine[o]["ProductionBatchMasterId"];
                            newDr["PlantId"] = dvOthersLine[o]["PlantId"];
                            newDr["IsPreference"] = 0;
                            dtPrefMergeLine.Rows.Add(newDr);
                        }
                    }
                }
                return dtPrefMergeLine;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private DataTable FilterRelatedLineWithMaxDate(DataRow drPref)//find line and line max date by in hand lines
        {
            try
            {
                var style = 0;
                var pRef = drPref["Id"].ToString();
                var ourStyle = drPref["OurStyleId"].ToString();
                CreateTempDt(out DataTable dtTemp, out DataRow newDr);//create temp dt for pref
                var dvBlock = new DataView(dtBlock);
                var dtWc = WorKCenterInHand(pRef);
                var lineMaxDate = DateTime.Now.Date;
                if (dvBlock.Count > 0)
                {
                    for (int i = 0; i < dtWc.Rows.Count; i++)
                    {
                        dvBlock.RowFilter = "LineId='" + dtWc.Rows[i]["WorkCenterMasterId"] + "'";// filter by in hand wc
                        if (dvBlock.Count > 0)//line found in dt
                        {
                            lineMaxDate = GetMaxDate(dvBlock.ToTable());///return line max date.
                            dvBlock.RowFilter = null;
                            dvBlock.RowFilter = "LineId='" + dtWc.Rows[i]["WorkCenterMasterId"] + "' AND Date='" + lineMaxDate.AddDays(-1) + "'";
                            if (dvBlock.Count > 0)
                            {
                                style = dvBlock[0]["OurStyleId"].ToString() == ourStyle ? 1 : 0;
                                newDr = NewRowAddInTemp(style, dtTemp, dvBlock, dtWc.Rows[i], lineMaxDate, drPref);
                                dtTemp.Rows.Add(newDr);
                            }
                        }
                        else//line not found in dt
                        {
                            newDr = NewRowAddInTemp(0, dtTemp, dvBlock, dtWc.Rows[i], DateTime.Now.Date, drPref);
                            dtTemp.Rows.Add(newDr);
                        }
                    }
                }
                else// very first time
                {
                    for (int i = 0; i < dtWc.Rows.Count; i++)
                    {
                        newDr = NewRowAddInTemp(style, dtTemp, dvBlock, dtWc.Rows[i], DateTime.Now.Date, drPref);
                        dtTemp.Rows.Add(newDr);
                    }
                }

                return dtTemp;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void CreateTempDt(out DataTable dtTemp, out DataRow newDr)
        {
            try
            {
                dtTemp = new DataTable();
                newDr = null;
                string[] col = { "LineId", "MaxDate", "IsLC", "StartDate", "EndDate", "MinWorkingDays" };
                foreach (var item in col)//temp dt create
                {
                    dtTemp.Columns.Add(new DataColumn(item));
                }
                dtTemp.Columns.Add(new DataColumn("IsPreference", typeof(int)));
                dtTemp.Columns.Add(new DataColumn("OurStyleId", typeof(int)));
                dtTemp.Columns.Add(new DataColumn("MaxDateMargine", typeof(int)));
                dtTemp.Columns.Add(new DataColumn("IsDateMarginePositive", typeof(int)));
                dtTemp.Columns.Add(new DataColumn("IsSetable", typeof(int)));
                dtTemp.Columns.Add(new DataColumn("TotalQty", typeof(int)));
                dtTemp.Columns.Add(new DataColumn(nameof(TotalOffDay), typeof(int)));
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private DataRow NewRowAddInTemp(int style, DataTable dtTemp, DataView dvBlock, DataRow drWc, DateTime lineMaxDate, DataRow drPref)
        {
            try
            {
                var isLc = false;
                if (style == 0) isLc = true;
                var newLineMaxDate = DateTime.Now.Date;
                if (lineMaxDate != newLineMaxDate)
                    newLineMaxDate = lineMaxDate.AddDays(-1);
                var startDate = BlockStartingBasedOnLsd(lineMaxDate, Convert.ToDateTime(drPref["Lsd"]));
                var margineDays = (newLineMaxDate.Date - Convert.ToDateTime(drPref["Lsd"])).TotalDays;
                var minWorkDays = Convert.ToInt32(drPref["MinWorkingDays"]);
                var daysToTarget = Convert.ToInt32(drPref["DaysToGetTheTarget"]);
                var standardTime = drPref["StandardTime"].ToString().ToInt();
                var hourlyTarget = Convert.ToInt32(drPref["MinRequiredTargetHourly"].ToString());
                var cd = CDincreaseByOffDays(startDate.AddDays(minWorkDays - 1), TotalOffDay(startDate.ToString(), startDate.AddDays(minWorkDays - 1).ToString(), _offDays), _offDays);
                var newDr = dtTemp.NewRow();
                newDr["LineId"] = drWc["WorkCenterMasterId"];
                newDr["OurStyleId"] = style;
                newDr["IsPreference"] = drWc["IsPreference"];
                newDr["MaxDate"] = newLineMaxDate;
                newDr["IsLC"] = isLc;
                newDr["StartDate"] = startDate;
                newDr["EndDate"] = cd;
                newDr["TotalQty"] = (CalculateTempQty(drPref, isLc));// + ((minWorkDays - daysToTarget) * (standardTime * hourlyTarget));
                newDr["IsSetable"] = 0;
                newDr["MaxDateMargine"] = Math.Abs(margineDays);
                newDr["IsDateMarginePositive"] = margineDays >= 0 ? 1 : 0;
                newDr[nameof(TotalOffDay)] = (cd - startDate).TotalDays + 1 - minWorkDays;
                newDr["MinWorkingDays"] = minWorkDays;
                return newDr;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(null, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private int CalculateTempQty(DataRow drPref, bool isLc)
        {
            try
            {
                var standardTime = drPref["StandardTime"].ToString().ToInt();
                var daysToGetTheTarget = Convert.ToInt32(drPref["DaysToGetTheTarget"].ToString());
                var hourlyTarget = Convert.ToInt32(drPref["MinRequiredTargetHourly"].ToString());
                //int totalQty = Convert.ToInt32(drPref["Qty"].ToString());
                var minWorkDays = Convert.ToInt32(drPref["MinWorkingDays"]);
                var remainingDays = 0;
                var remainingQty = 0;
                if (isLc)
                {
                    var lcQty = CalculateLearningCurveOutPut(drPref);
                    remainingDays = minWorkDays - daysToGetTheTarget;
                    remainingQty = ((standardTime * hourlyTarget) * remainingDays) + lcQty;
                }
                else
                    remainingQty = (standardTime * hourlyTarget) * minWorkDays;
                return remainingQty;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(null, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private DataTable SetLineInTempList(DataTable dtTemp, DataRow drPref)
        {
            try
            {
                #region Data get from p.Ref

                var minAllocatedLine = Convert.ToInt32(drPref["MinAllocatedLine"]);
                var minWorkingDays = Convert.ToInt32(drPref["MinWorkingDays"]);
                var daysToGetTheTarget = Convert.ToInt32(drPref["DaysToGetTheTarget"]);
                var lsd = Convert.ToDateTime(drPref["Lsd"]);
                var cd = Convert.ToDateTime(drPref["CommitmentDate"]);
                var prefTotalQty = Convert.ToInt32(drPref["Qty"]);
                var standardTime = drPref["StandardTime"].ToString().ToInt();
                var hourlyTarget = Convert.ToInt32(drPref["MinRequiredTargetHourly"].ToString());

                #endregion Data get from p.Ref

                var dvPreference = new DataView(dtTemp) { Sort = "IsPreference DESC, OurStyleId DESC, MaxDateMargine, IsDateMarginePositive" };//Filter by preference
                var dtFilterTemp = dvPreference.ToTable();
                var lineInHand = GetMinmumLine(dtTemp, minAllocatedLine);
                var dailyQty = standardTime * hourlyTarget;
                var countCD = new ArrayList();
                var dt2Temp = new DataTable();
                GetTopNRowFromDataTable(ref dt2Temp, dtFilterTemp, lineInHand, drPref);//get top n rows from dt
                if (dt2Temp.Rows.Count > 0)
                {
                    var tQty = Convert.ToInt32(dt2Temp.Compute("SUM(TotalQty)", null));
                    IfDateLessThenCDInLine(cd, prefTotalQty, ref dt2Temp, dailyQty, countCD, ref tQty, _offDays);
                    while (tQty < prefTotalQty)//is qty has  //but line not found for cd cross
                    {
                        var remainingDays = (prefTotalQty - tQty) / dailyQty;
                        if (remainingDays >= minWorkingDays)//if remainingDays greater than or equal minWorkingDays
                        {
                            var dv = new DataView(dtFilterTemp) { RowFilter = "LineId NOT IN(" + ReturnStringArray(countCD) + ")" };
                            if (dv.Count > 0)// if another line in hand
                            {
                                GetTopNRowFromDataTable(ref dt2Temp, dv.ToTable(), 1, drPref);
                                var dv2 = new DataView(dt2Temp) { RowFilter = "LineId NOT IN(" + ReturnStringArray(countCD) + ")" };
                                if (dv2.Count > 0)// if return another line (mwd <= cd)// then distribute in new line
                                {
                                    tQty = Convert.ToInt32(dt2Temp.Compute("SUM(TotalQty)", null));
                                    var newCountCD = new ArrayList();
                                    IfDateLessThenCDInLine(cd, prefTotalQty, ref dt2Temp, dailyQty, newCountCD, ref tQty, _offDays);//set date and qty in temp dt
                                    foreach (var item in newCountCD)
                                    {
                                        if (!countCD.Contains(item))
                                            countCD.Add(item);
                                    }
                                }
                                else// if return another line (mwd > cd) // then distribute in previous line
                                {
                                    tQty = Convert.ToInt32(dt2Temp.Compute("SUM(TotalQty)", null));
                                    IfDateCrossThenCDInLine(prefTotalQty, ref dt2Temp, dailyQty, ref tQty, _offDays);
                                }
                            }// if another line in hand
                            else// if another line in not hand return previous line
                            {
                                tQty = Convert.ToInt32(dt2Temp.Compute("SUM(TotalQty)", null));
                                IfDateCrossThenCDInLine(prefTotalQty, ref dt2Temp, dailyQty, ref tQty, _offDays);//set date and qty in temp dt
                            }
                        }
                        else// if remainingDays less then minWorkingDays return previous line
                        {
                            tQty = Convert.ToInt32(dt2Temp.Compute("SUM(TotalQty)", null));
                            IfDateCrossThenCDInLine(prefTotalQty, ref dt2Temp, dailyQty, ref tQty, _offDays);//set date and qty in temp dt
                        }
                    }
                }
                else// if mwd cross cd then get top any min line //TODO: If min line is not get (for cd crossing).
                {
                    GetTopAnyNRow(ref dt2Temp, dtFilterTemp, lineInHand, drPref);
                    if (dt2Temp.Rows.Count > 0)
                    {
                        var tQty = Convert.ToInt32(dt2Temp.Compute("SUM(TotalQty)", null));
                        while (tQty < prefTotalQty)
                        {
                            tQty = Convert.ToInt32(dt2Temp.Compute("SUM(TotalQty)", null));
                            IfDateCrossThenCDInLine(prefTotalQty, ref dt2Temp, dailyQty, ref tQty, _offDays);
                        }
                    }
                }
                return dt2Temp;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private void IsOffDay(DateTime sDate, out bool isOffDay, out string OffDayType)
        {
            try
            {
                var dv = new DataView(_offDays.Tables[0]) { RowFilter = "OffDayDate='" + sDate + "'" };
                if (dv.Count > 0)
                {
                    isOffDay = true;
                    OffDayType = dv[0][nameof(OffDayType)].ToString();
                }
                else
                {
                    isOffDay = false;
                    OffDayType = "";
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        #endregion Distributed Date
    }
}
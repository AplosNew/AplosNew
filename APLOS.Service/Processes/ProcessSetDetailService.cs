#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Processes;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Processes
{
    public partial class ProcessSetDetailService : Service<ProcessSetDetail>, IProcessSetDetailService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public ProcessSetDetailService(
            IRepositoryAsync<ProcessSetDetail> ProcessSetDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(ProcessSetDetailRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> Query(string processSetId)
        {
            try
            {
                string _sql = @"SELECT PSD.Id
										, PSD.ProcessSetId
										, PSD.ProcessId, p.UserName AS ProcessName
										, PSD.[Sequence], PSD.IsBaseProcess, PSD.[Days], PSD.Symbol
										, PSD.ProductionCycleTime, PSD.JobWorkApplicable, PSD.JobWorkType
										, PSD.EntityIdWithinCompany, PSD.EntityIdWithinGroup, PSD.PartyId
										, EntityOrVendorName= CASE WHEN PSD.EntityIdWithinCompany<>'' THEN EWC.UserName 
													WHEN PSD.EntityIdWithinGroup<>'' THEN EWG.UserName
													WHEN PSD.PartyId<>'' THEN PRT.UserName
													ELSE PRT.UserName END
										, PSD.Archive, PSD.MaterialMasterId, MM.UserName AS MaterialMasterName
	                                    , PSD.ArticleId, ART.StandardName AS ArticleName,PSD.Qty,PSD.UOMId

								FROM HKP.ProcessSetDetail AS PSD
								LEFT OUTER JOIN HKP.ProcessSet AS PS ON PSD.ProcessSetId=PS.Id
								LEFT OUTER JOIN HKP.Process AS P ON PSD.ProcessId=P.Id
								LEFT OUTER JOIN ORG.Entity AS EWC ON PSD.EntityIdWithinCompany=EWC.Id
								LEFT OUTER JOIN ORG.Entity AS EWG ON PSD.EntityIdWithinGroup=EWG.Id
								LEFT OUTER JOIN HKP.Party AS PRT ON PSD.PartyId=PRT.Id
								LEFT JOIN MST.MaterialMaster AS MM ON PSD.MaterialMasterId=MM.Id
                                LEFT JOIN MST.MaterialMasterArticle AS ART ON PSD.ArticleId=ART.Id
								LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=PSD.UOMId
                                WHERE PSD.ProcessSetId='" + processSetId + "' ORDER BY PSD.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        /// <summary>
        /// in material master
        /// </summary>
        /// <param name="processSetId"></param>
        /// <returns></returns>
        public IEnumerable<object> GetProcessSetList(string processSetId,string entityId)
        {
            try
            {
                string _sql = @"SELECT DISTINCT PSD.ProcessSetId
										, PSD.ProcessId, p.UserName AS ProcessName
										, PSD.[Sequence], PSD.IsBaseProcess, PSD.[Days], PSD.Symbol
										, PSD.ProductionCycleTime, PSD.JobWorkApplicable, PSD.JobWorkType
										, PSD.EntityIdWithinCompany, PSD.EntityIdWithinGroup, PSD.PartyId
										, EntityOrVendorName= CASE WHEN PSD.EntityIdWithinCompany<>'' THEN EWC.UserName 
													WHEN PSD.EntityIdWithinGroup<>'' THEN EWG.UserName
													WHEN PSD.PartyId<>'' THEN PRT.UserName
													ELSE PRT.UserName END
										, PSD.Archive, PSD.MaterialMasterId, MM.UserName AS MaterialMasterName
	                                    , PSD.ArticleId, ART.StandardName AS ArticleName,Qty=CASE WHEN PSD.Qty=0 THEN 100 ELSE PSD.Qty END,PSD.UOMId
										, P.IsProductionProcess,TG.ProductionBookingLevel
										,IsInventory=CAST(CASE WHEN M.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT)
								FROM HKP.ProcessSetDetail AS PSD
								LEFT JOIN HKP.ProcessSet AS PS ON PSD.ProcessSetId=PS.Id
								LEFT JOIN HKP.Process AS P ON PSD.ProcessId=P.Id
								LEFT JOIN ORG.Entity AS EWC ON PSD.EntityIdWithinCompany=EWC.Id
								LEFT JOIN ORG.Entity AS EWG ON PSD.EntityIdWithinGroup=EWG.Id
								LEFT JOIN HKP.Party AS PRT ON PSD.PartyId=PRT.Id
                                LEFT JOIN MST.MaterialMaster AS MM ON PSD.MaterialMasterId=MM.Id
                                LEFT JOIN MST.MaterialMasterArticle AS ART ON PSD.ArticleId=ART.Id
								LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=PSD.UOMId
								LEFT JOIN [HKP].[EntityProcessTag] TG ON TG.ProcessId=P.Id AND TG.EntityId='" + entityId + @"' 
							    LEFT JOIN [dbo].[EntityConfig] M ON M.ConsumptionProcessId=P.Id AND M.EntityId='"+ entityId + @"' 
                                WHERE PSD.ProcessSetId='" + processSetId + "' ORDER BY PSD.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public void InsertGraph(string processSetId, IEnumerable<ProcessSetDetail> processSetDetail)
        {
            try
            {
                if (processSetDetail != null)
                {
                    var id = CreatePk(processSetId);
                    var count = id.ToInt();
                    foreach (var item in processSetDetail)
                    {
                        //insert
                        item.Id = processSetId + "-" + count;
                        item.ProcessSetId = processSetId;
                        count++;
                        base.InsertGraph(item);
                    }
                }
                else
                    throw new CustomException("Please select at least one process!");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public void InsertUpdateOrDeleteGraph(string processSetId, IEnumerable<ProcessSetDetail> processSetDetail)
        {
            try
            {
                if (processSetDetail != null)
                {
                    var id = CreatePk(processSetId);
                    var count = id.ToInt();
                    foreach (var item in processSetDetail)
                    {
                        if (item.Id.StartsWith("new"))
                        {
                            //insert
                            item.Id = processSetId + "-" + count;
                            item.ProcessSetId = processSetId;
                            count++;
                            base.InsertGraph(item);
                        }
                        else
                        {
                            //update
                            UpdateGraph(item);
                        }
                    }
                    var dbList = base.Query(t => t.ProcessSetId == processSetId).Select().AsEnumerable();
                    if (dbList != null)
                    {
                        if (processSetDetail == null)
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
                                if (!processSetDetail.Any(t => t.Id == item.Id))
                                {
                                    base.DeleteGraph(item);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        private string CreatePk(string processSetId)
        {
            try
            {
                string id = string.Empty;
                var Db_Pk = base.Query(t => t.ProcessSetId == processSetId).Select(t => t.Id).AsEnumerable();
                if (Db_Pk.Count() != 0)
                {
                    id = Db_Pk.Max(x => Convert.ToInt32(x.Substring(processSetId.Length + 1)) + 1).ToString();
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public void DeleteGraph(string processSetId)
        {
            try
            {
                var entity = base.Query(t => t.ProcessSetId == processSetId).Select().AsEnumerable();
                if (entity != null)
                {
                    foreach (var item in entity)
                    {
                        base.DeleteGraph(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }
    }
}
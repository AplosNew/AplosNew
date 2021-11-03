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
    public partial class SubProcessSetDetailService : Service<SubProcessSetDetail>, ISubProcessSetDetailService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public SubProcessSetDetailService(
            IRepositoryAsync<SubProcessSetDetail> subProcessSetDetailRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(subProcessSetDetailRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> Query(string subProcessSetId)
        {
            try
            {
                string _sql = @"SELECT PSD.Id
                                        ,PSD.SubProcessSetId
                                        ,PSD.SubProcessId
                                        ,p.UserName AS SubProcessName
		                                ,PSD.[Sequence]
                                        ,PSD.IsBaseProcess
                                        ,PSD.[Days]
                                        ,PSD.Symbol
                                        ,PSD.ProductionCycleTime
                                        ,PSD.JobWorkApplicable
                                        ,PSD.JobWorkType
                                        ,PSD.EntityIdWithinCompany
                                        ,PSD.EntityIdWithinGroup
                                        ,PSD.VendorId
                                        ,EntityOrVendorName =
                                           CASE ISNULL(PSD.EntityIdWithinCompany, '')
                                                 WHEN '' THEN ''
                                                 ELSE EWC.UserName
                                                 END
                                         + CASE ISNULL(PSD.EntityIdWithinGroup, '')
                                                  WHEN '' THEN ''
                                                  ELSE EWG.UserName
                                                  END
                                         + CASE ISNULL(PSD.VendorId, '')
                                                  WHEN '' THEN ''
                                                  ELSE PRT.UserName
                                                  END
                                FROM HKP.SubProcessSetDetail AS PSD
                                LEFT OUTER JOIN HKP.SubProcessSet AS PS ON PSD.SubProcessSetId = PS.Id
                                LEFT OUTER JOIN HKP.SubProcess AS P ON PSD.SubProcessId = P.Id
                                LEFT OUTER JOIN ORG.Entity AS EWC ON PSD.EntityIdWithinCompany = EWC.Id
                                LEFT OUTER JOIN ORG.Entity AS EWG ON PSD.EntityIdWithinGroup = EWG.Id
                                LEFT OUTER JOIN HKP.Party AS PRT ON PSD.VendorId = PRT.Id
                                WHERE PSD.SubProcessSetId = '" + subProcessSetId + "' ORDER BY PSD.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public void InsertGraph(string subProcessSetId, IEnumerable<SubProcessSetDetail> subProcessSetDetail)
        {
            try
            {
                if (subProcessSetDetail != null)
                {
                    string id = CreatePk(subProcessSetId);
                    var count = id.ToInt();
                    foreach (var item in subProcessSetDetail)
                    {
                        //insert
                        item.Id = subProcessSetId + "-" + count;
                        item.SubProcessSetId = subProcessSetId;
                        count++;
                        base.InsertGraph(item);
                    }
                }
                else
                    throw new CustomException("Please select at least one sub process...........!");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public void InsertUpdateOrDeleteGraph(string subProcessSetId, IEnumerable<SubProcessSetDetail> subProcessSetDetail)
        {
            try
            {
                if (subProcessSetDetail != null)
                {
                    string id = CreatePk(subProcessSetId);
                    var count = id.ToInt();
                    foreach (var item in subProcessSetDetail)
                    {
                        if (item.Id.StartsWith("new"))
                        {
                            //insert
                            item.Id = subProcessSetId + "-" + count;
                            item.SubProcessSetId = subProcessSetId;
                            count++;
                            base.InsertGraph(item);
                        }
                        else
                        {
                            //update
                            UpdateGraph(item);
                        }
                    }
                    var dbList = base.Query(t => t.SubProcessSetId == subProcessSetId).Select().AsEnumerable();
                    if (dbList != null)
                    {
                        if (subProcessSetDetail == null)
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
                                if (!subProcessSetDetail.Any(t => t.Id == item.Id))
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

        private string CreatePk(string subProcessSetId)
        {
            try
            {
                string id = string.Empty;
                var Db_Pk = base.Query(t => t.SubProcessSetId == subProcessSetId).Select(t => t.Id).AsEnumerable();
                if (Db_Pk.Count() != 0)
                {
                    id = Db_Pk.Max(x => Convert.ToInt32(x.Substring(subProcessSetId.Length + 1)) + 1).ToString();
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

        public void DeleteGraph(string subProcessSetId)
        {
            try
            {
                var entity = base.Query(t => t.SubProcessSetId == subProcessSetId).Select().AsEnumerable();
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
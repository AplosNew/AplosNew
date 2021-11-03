#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Documents;
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
    public class ComplianceDocumentSetProofTypeAssignService : Service<ComplianceDocumentSetProofTypeAssign>, IComplianceDocumentSetProofTypeAssignService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ComplianceDocumentSetProofTypeAssignService(
            IRepositoryAsync<ComplianceDocumentSetProofTypeAssign> ComplianceDocumentSetProofTypeAssign
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            ) :
            base(ComplianceDocumentSetProofTypeAssign, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(ComplianceDocumentSetProofTypeAssign), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertOrUpdate(IEnumerable<ComplianceDocumentSetProofTypeAssign> entities, string masterId)
        {
            try
            {
                var pk = GetMaxNumber(nameof(ComplianceDocumentSetProofTypeAssign), PKGeneratorEnum.Auto, null, DateTime.Now);
                var dbList = GetGbList(masterId);
                if (entities != null)
                {
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.ComplianceDocumentSetId = masterId;
                            InsertGraph(item);
                        }
                        else
                        {
                            if (dbList.Any(r => r.ComplianceDocumentSetId == item.ComplianceDocumentSetId && r.Id == item.Id))
                                UpdateGraph(item);
                            //else
                            //    throw new CustomException(ServiceResources.RecordNoLonger);
                        }
                    }
                }
                if (dbList.Count() > 0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                DeleteGraph(item);
                            }
                        }
                    }
                }
                //if (entities != null)
                //{
                //    foreach (var item in entities)
                //    {
                //        if (item.Id ==null)
                //        {
                //            pk.MaxNumber++;
                //            item.ModelState = ModelState.Added;
                //            AuditService.Log(item);
                //            item.Id = pk.MaxNumber.ToString();
                //            item.ComplianceDocumentSetId = masterId;
                //            base.InsertOrUpdateGraph(item);
                //        }
                //    }
                //}
                //IEnumerable<ComplianceDocumentSetProofTypeAssign> dbList = GetGbList(masterId);
                //if (dbList != null && dbList.Count() > 0)
                //{
                //    if (entities == null)
                //    {
                //        foreach (var item in dbList)
                //        {
                //            base.Delete(item);
                //        }
                //    }
                //    else
                //    {
                //        foreach (var item in dbList)
                //        {
                //            var db_c = entities.Where(a => a.ComplianceDocumentSetId == item.ComplianceDocumentSetId && a.ComplianceDocumentProofTypeId == item.ComplianceDocumentProofTypeId).FirstOrDefault();
                //            if (db_c == null || db_c.Id == null)
                //            {
                //                base.Delete(item);
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
            }
        }

        private IEnumerable<ComplianceDocumentSetProofTypeAssign> GetGbList(string materId)
        {
            try
            {
                var _sql = @"SELECT * FROM [HKP].[ComplianceDocumentSetProofTypeAssign]
                                WHERE  ComplianceDocumentSetId ='" + materId + "'";
                return _sqlRepository.GetModelCollection<ComplianceDocumentSetProofTypeAssign>(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel QueryGraph(GridParameter parameters, string complianceDocumentSetId)
        {
            try
            {
                //var sql = @"SELECT SELECT AP.Id
                //           ,CASE ISNULL(AP.Id,'') when '' then 0
                //           else 1 end Flag,P.Id ComplianceDocumentProofTypeId,P.UserName FROM [HKP].[ComplianceDocumentProofType] P
                //            LEFT JOIN ( SELECT Id,ComplianceDocumentProofTypeId,ComplianceDocumentSetId FROM [HKP].[ComplianceDocumentSetProofTypeAssign] WHERE ComplianceDocumentSetId='" + complianceDocumentSetId + "' )AP ON P.Id=AP.ComplianceDocumentProofTypeId";
                //return _sqlRepository.GetDataCollection(sql);

                parameters.CmdText = @"SELECT AP.Id,CASE ISNULL(AP.Id,'') when ''
                                        then CAST('False' as bit)
                                        else CAST('TRUE' as bit)
                                        end Flag, P.Id ComplianceDocumentProofTypeId,P.Sequence,P.UserName FROM [HKP].[ComplianceDocumentProofType] P
                                        LEFT JOIN ( SELECT Id,ComplianceDocumentProofTypeId,ComplianceDocumentSetId FROM [HKP].[ComplianceDocumentSetProofTypeAssign] WHERE ComplianceDocumentSetId='" + complianceDocumentSetId + "' )AP ON P.Id=AP.ComplianceDocumentProofTypeId";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void DeleteWithMaster(string Id)
        {
            try
            {
                var data = Query(r => r.ComplianceDocumentSetId == Id).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count(); i++)
                    {
                        Delete(data[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}
#region Using

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
    public class ComplianceDocumentSetDetailService : Service<ComplianceDocumentSetDetail>, IComplianceDocumentSetDetailService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ComplianceDocumentSetDetailService(
            IRepositoryAsync<ComplianceDocumentSetDetail> ComplianceDocumentSetDetailRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(ComplianceDocumentSetDetailRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ComplianceDocumentSetDetail> entities, string masterId)
        {
            try
            {
                if (entities != null)
                {
                    var pk = GetMaxNumber(nameof(ComplianceDocumentSetDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
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
                            UpdateGraph(item);
                        }
                    }
                }
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

        public IEnumerable<object> Query(string masterId)
        {
            try
            {
                var sql = @"SELECT CD.UserName,CDC.UserName AS ComplianceDocumentCategoryName,CDSC.UserName AS ComplianceDocumentSubCategoryName,CD.DocumentType,CD.Importance,CD.EmploymentStage,DSD.* FROM [HKP].[ComplianceDocumentSetDetail] AS DSD
                            LEFT OUTER JOIN [HKP].[ComplianceDocument] AS CD ON DSD.ComplianceDocumentId=CD.Id
                            LEFT OUTER JOIN [HKP].[ComplianceDocumentCategory] AS CDC ON CD.ComplianceDocumentCategoryId=CDC.Id
                            LEFT OUTER JOIN [HKP].[ComplianceDocumentSubCategory] AS CDSC ON CD.ComplianceDocumentSubCategoryId=CDSC.Id
                            WHERE DSD.ComplianceDocumentSetId='" + masterId + @"' Order By CD.UserName";
                return _sqlRepository.GetDataCollection(sql);
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
                var data = base.Query(r => r.ComplianceDocumentSetId == Id).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count(); i++)
                    {
                        DeleteGraph(data[i]);
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
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
    public class ComplianceDocumentPostRecruitmentService : Service<ComplianceDocumentPostRecruitment>, IComplianceDocumentPostRecruitmentService
    {
        #region Constructor

        private readonly IRepositoryAsync<ComplianceDocumentPostRecruitment> _complianceDocumentPostRecruitmentRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ComplianceDocumentPostRecruitmentService(
            IRepositoryAsync<ComplianceDocumentPostRecruitment> complianceDocumentPostRecruitmentRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(complianceDocumentPostRecruitmentRepository, unitOfWork, pkGeneratorService)
        {
            _complianceDocumentPostRecruitmentRepository = complianceDocumentPostRecruitmentRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<ComplianceDocumentPostRecruitment> entities, string masterId)
        {
            try
            {
                IEnumerable<ComplianceDocumentPostRecruitment> DbList = GetDBList(masterId);
                ///Delete
                foreach (var item in DbList)
                {
                    //var db_c = entities.Where(a => a.ComplianceDocumentId==item.ComplianceDocumentId).FirstOrDefault();
                    if (item != null)
                    {
                        Delete(item);
                    }
                }
                if (entities != null)
                {
                    var count = 0;
                    foreach (var item in entities)
                    {
                        count++;
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = masterId + "-" + count;
                            item.ComplianceDocumentId = masterId;
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

        private IEnumerable<ComplianceDocumentPostRecruitment> GetDBList(string masterId)
        {
            try
            {
                string _sql = @" SELECT * FROM [HKP].[ComplianceDocumentPostRecruitment] A WHERE A.ComplianceDocumentId='" + masterId + "'";
                return _complianceDocumentPostRecruitmentRepository.SqlQuery<ComplianceDocumentPostRecruitment>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> Query(string complianceDocumentId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.PostRecruitment AS Text,A.PostRecruitment AS Value FROM [HKP].[ComplianceDocumentPostRecruitment] AS A
                            WHERE A.ComplianceDocumentId='" + complianceDocumentId + "'";
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
                var data = base.Query(r => r.ComplianceDocumentId == Id).Select().ToList();
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
#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class DocumentSetAssignDetailService : Service<DocumentSetAssignDetail>, IDocumentSetAssignDetailService
    {
        #region Constructor

        private readonly IRepositoryAsync<DocumentSetAssignDetail> _documentSetAssignDetailRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DocumentSetAssignDetailService(
            IRepositoryAsync<DocumentSetAssignDetail> documentSetAssignDetailRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(documentSetAssignDetailRepository, unitOfWork, pkGeneratorService)
        {
            _documentSetAssignDetailRepository = documentSetAssignDetailRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InsertOrUpdate(IEnumerable<DocumentSetAssignDetail> entities, string masterId, bool flag)//if flag false then delete
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (entities != null)
                {
                    var count = 0;
                    string complianceDocumentSetId = string.Empty;
                    foreach (var item in entities)
                    {
                        complianceDocumentSetId = item.ComplianceDocumentSetId;
                        break;
                    }
                    IEnumerable<DocumentSetAssignDetail> DbList = GetDBList(identity.CompanyGroupId, masterId, complianceDocumentSetId);
                    ///Delete
                    if (flag)
                    {
                        foreach (var item in DbList)
                        {
                            var db_c = entities.Where(a => a.CompanyGroupId == item.CompanyGroupId && a.DocumentConfigurationDesignationGroupId == item.DocumentConfigurationDesignationGroupId && a.ResponsiblePersonId == item.ResponsiblePersonId).FirstOrDefault();
                            if (db_c == null)
                            {
                                Delete(item);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in DbList)
                        {
                            Delete(item);
                        }
                    }
                    foreach (var item in entities)
                    {
                        count++;
                        if (item.ResponsiblePersonId != null)
                        {
                            var db_c = DbList.Where(a => a.CompanyGroupId == item.CompanyGroupId && a.DocumentConfigurationDesignationGroupId == item.DocumentConfigurationDesignationGroupId && a.ResponsiblePersonId == item.ResponsiblePersonId).FirstOrDefault();
                            if (db_c == null || db_c.Id == null)
                            //if (string.IsNullOrEmpty(item.Id))
                            {
                                item.Id = masterId + "-" + count;
                                item.DocumentConfigurationDesignationGroupId = masterId;
                                item.CompanyGroupId = identity.CompanyGroupId;
                                InsertGraph(item);
                            }
                            else
                            {
                                UpdateGraph(item);
                            }
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

        private IEnumerable<DocumentSetAssignDetail> GetDBList(string companyGroupId, string masterId, string complianceDocumentSetId)
        {
            try
            {
                string _sql = @" SELECT * FROM [HKP].[DocumentSetAssignDetail] A WHERE A.CompanyGroupId='" + companyGroupId + "' AND A.DocumentConfigurationDesignationGroupId='" + masterId + @"' AND A.ComplianceDocumentSetId='" + complianceDocumentSetId + @"'";
                return _documentSetAssignDetailRepository.SqlQuery<DocumentSetAssignDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> Query(string documentSetId, string plantId, string employeeTypeId)
        {
            try
            {
                var sql = @"  SELECT  D.Id, a.Id AS ComplianceDocumentSetId,
                              D.DocumentConfigurationDesignationGroupId,
                              D.ResponsiblePersonId,
                              D.CompanyGroupId,
                              E.EmployeeName AS ResponsiblePersonName,
                              C.Id           AS ComplianceDocumentId,
                              C.UserName AS ComplianceDocumentName,
                              C.DocumentType
                              FROM   hkp.ComplianceDocumentSet AS A
                              LEFT OUTER JOIN [HKP].[ComplianceDocumentSetDetail] AS CD
                                           ON A.Id = CD.ComplianceDocumentSetId
                              LEFT OUTER JOIN [HKP].[ComplianceDocument] AS C
                                           ON CD.ComplianceDocumentId = C.Id
                              LEFT OUTER JOIN (SELECT B.Id,
                               B.DocumentConfigurationDesignationGroupId,
                               B.ComplianceDocumentId,
                               B.ResponsiblePersonId,
                               B.CompanyGroupId,
                               B.ComplianceDocumentSetId
                                FROM   [HKP].DocumentConfigurationDesignationGroup as a
                               LEFT OUTER JOIN [HKP].DocumentSetAssignDetail AS B ON A.Id =  B.DocumentConfigurationDesignationGroupId
                        WHERE  A.PlantId = '" + plantId + "'  AND A.EmployeeCategoryId = '" + employeeTypeId + "'   AND B.ComplianceDocumentSetId = '" + documentSetId + @"')AS D  ON C.Id = D.ComplianceDocumentId
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS E  ON D.ResponsiblePersonId = E.SystemId
                            WHERE  ISNULL(c.Id, '') <> '' AND A.Id ='" + documentSetId + "'";
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
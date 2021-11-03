#region Using

using Library.Core;
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
    public class DocumentConfigurationDesignationGroupService : Service<DocumentConfigurationDesignationGroup>, IDocumentConfigurationDesignationGroupService
    {
        #region Constructor

        private readonly IRepositoryAsync<DocumentConfigurationDesignationGroup> _dcumentConfigurationDesignationGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IDocumentSetAssignDetailService _documentSetAssignDetailService;
        private readonly IComplianceDocumentSetDetailService _complianceDocumentSetDetailService;

        public DocumentConfigurationDesignationGroupService(
            IRepositoryAsync<DocumentConfigurationDesignationGroup> dcumentConfigurationDesignationGroupRepository,
            IPKGeneratorService pkGeneratorService,
            IDocumentSetAssignDetailService documentSetAssignDetailService,
            IComplianceDocumentSetDetailService complianceDocumentSetDetailService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(dcumentConfigurationDesignationGroupRepository, unitOfWork, pkGeneratorService)
        {
            _dcumentConfigurationDesignationGroupRepository = dcumentConfigurationDesignationGroupRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _complianceDocumentSetDetailService = complianceDocumentSetDetailService;
            _documentSetAssignDetailService = documentSetAssignDetailService;
        }

        #endregion Constructor

        public void InsertORUpdateGraph(DocumentConfigurationDesignationGroup entity, IEnumerable<DocumentSetAssignDetail> entities)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                CheckUniqeCombineRow(entity);
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    entity.CompanyGroupId = identity.CompanyGroupId;

                    if (entity.ResponsiblePersonBy == "Document")
                    {
                        _documentSetAssignDetailService.InsertOrUpdate(entities, entity.Id, true);
                    }
                    else
                    {
                        var doc = base.Query(t => t.Id == entity.Id).Select(t => t.ResponsiblePersonBy).FirstOrDefault();
                        var isDoc = (doc == entity.ResponsiblePersonBy) ? true : false;
                        if (isDoc)
                        {
                            var DbList = GetDBList(identity.CompanyGroupId, entity.Id, entity.ComplianceDocumentSetId);
                            foreach (var item in DbList)
                            {
                                _documentSetAssignDetailService.Delete(item);
                            }
                        }

                        var dataList = _complianceDocumentSetDetailService.Query(t => t.ComplianceDocumentSetId == entity.ComplianceDocumentSetId).Select(t => t.ComplianceDocumentId).ToList();
                        InsertGraph(entity);
                        var count = 0;
                        foreach (var item in dataList)
                        {
                            count++;
                            DocumentSetAssignDetail documentSetAssignDetail = new DocumentSetAssignDetail
                            {
                                Id = entity.Id + "-" + count,
                                CompanyGroupId = identity.CompanyGroupId,
                                DocumentConfigurationDesignationGroupId = entity.Id,
                                ComplianceDocumentSetId = entity.ComplianceDocumentSetId,
                                ComplianceDocumentId = item,
                                ResponsiblePersonId = entity.ResponsiblePersonId
                            };
                            _documentSetAssignDetailService.Insert(documentSetAssignDetail);
                        }
                    }
                }
                else
                {
                    var doc = base.Query(t => t.Id == entity.Id).Select(t => t.ResponsiblePersonBy).FirstOrDefault();
                    var isDoc = (doc == entity.ResponsiblePersonBy) ? true : false;

                    if (entities != null)
                    {
                        _documentSetAssignDetailService.InsertOrUpdate(entities, entity.Id, isDoc);
                    }
                    else
                    {
                        if (isDoc)
                        {
                            var DbList = GetDBList(identity.CompanyGroupId, entity.Id, entity.ComplianceDocumentSetId);
                            foreach (var item in DbList)
                            {
                                _documentSetAssignDetailService.Delete(item);
                            }
                        }

                        var dataList = _complianceDocumentSetDetailService.Query(t => t.ComplianceDocumentSetId == entity.ComplianceDocumentSetId).Select(t => t.ComplianceDocumentId).ToList();
                        var count = 0;
                        foreach (var item in dataList)
                        {
                            count++;
                            DocumentSetAssignDetail documentSetAssignDetail = new DocumentSetAssignDetail
                            {
                                Id = entity.Id + "-" + count,
                                CompanyGroupId = identity.CompanyGroupId,
                                DocumentConfigurationDesignationGroupId = entity.Id,
                                ComplianceDocumentSetId = entity.ComplianceDocumentSetId,
                                ComplianceDocumentId = item,
                                ResponsiblePersonId = entity.ResponsiblePersonId
                            };
                            _documentSetAssignDetailService.Insert(documentSetAssignDetail);
                        }
                    }
                    UpdateGraph(entity);
                }
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(DocumentConfigurationDesignationGroup), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUniqeCombineRow(DocumentConfigurationDesignationGroup entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var db_Data = base.Query(t => t.Id != entity.Id && t.PlantId == entity.PlantId && t.EmployeeCategoryId == entity.EmployeeCategoryId && t.ComplianceDocumentSetId == entity.ComplianceDocumentSetId && t.CompanyGroupId == identity.CompanyGroupId
                                        && t.EmploymentType == entity.EmploymentType).Select().FirstOrDefault();
            if (db_Data != null)
                throw new CustomException("This combination already exist....!");
        }

        public void Delete(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                base.Delete(Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public GridModel Query(GridParameter parameters, string companyId, string plantId)
        {
            try
            {
                parameters.searchBy = "ComplianceDocumentSet";
                parameters.sort = "ComplianceDocumentSet";
                parameters.order = "ASC";
                parameters.CmdText = @"SELECT DG.*,CDS.UserName ComplianceDocumentSet,EC.UserName EmployeeCategory, EI.EmployeeName ResponsiblePersonName FROM [HKP].[DocumentConfigurationDesignationGroup] DG
                                     LEFT JOIN HKP.ComplianceDocumentSet CDS ON DG.ComplianceDocumentSetId=CDS.Id
                                     LEFT JOIN HKP.EmployeeCategory EC ON DG.EmployeeCategoryId=EC.Id
                                     LEFT JOIN EmployeeInformation EI ON DG.ResponsiblePersonId=EI.SystemId
                                     WHERE  DG.CompanyId='" + companyId + "' AND DG.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel QueryAssign(GridParameter parameters, string companyGroupId, string plantId, string employeeTypeId, string employmentType)
        {
            try
            {
                parameters.searchBy = "ComplianceDocumentSetName";
                parameters.sort = "ComplianceDocumentSetName";
                parameters.order = "ASC";
                var sql = @"SELECT E.EmployeeName AS ResponsiblePersonName,B.UserName AS ComplianceDocumentSetName, A.* FROM [HKP].[DocumentConfigurationDesignationGroup] AS A
                               LEFT OUTER JOIN [HKP].[ComplianceDocumentSet] AS B ON A.ComplianceDocumentSetId= B.Id
                               LEFT OUTER JOIN  [dbo].[EmployeeInformation] AS E ON A.ResponsiblePersonId= E.SystemId
                               WHERE A.PlantId='" + plantId + "' AND A.EmployeeCategoryId='" + employeeTypeId + "' AND A.CompanyGroupId='" + companyGroupId + "' AND A.EmploymentType='" + employmentType + "' ";

                parameters.CmdText = sql;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel GetDocumentList(GridParameter parameters, string companyGroupId, string plantId, string employeeTypeId, string documentSetType)
        {
            try
            {
                string sql = "";
                if (documentSetType == "DocumentSet")
                {
                    parameters.searchBy = "ComplianceDocumentSetName";
                    parameters.sort = "ComplianceDocumentSetName";
                    parameters.order = "ASC";
                    sql = @"SELECT   D.Id,D.ResponsiblePersonId,D.CompanyGroupId,E.EmployeeName AS ResponsiblePersonName,A.Id AS ComplianceDocumentSetId,A.UserName AS ComplianceDocumentSetName FROM [HKP].[ComplianceDocumentSet] AS A
                            LEFT OUTER JOIN [HKP].[ComplianceDocumentSetDetail] AS B ON A.Id=B.ComplianceDocumentSetId
							LEFT OUTER JOIN (SELECT * FROM  [HKP].[DocumentConfigurationDesignationGroup] WHERE PlantId='" + plantId + "' AND EmployeeCategoryId='" + employeeTypeId + @"' )AS D ON A.Id = D.ComplianceDocumentSetId
							LEFT OUTER JOIN [dbo].[EmployeeInformation] AS E ON D.ResponsiblePersonId = E.SystemId
                            WHERE A.CompanyGroupId='" + companyGroupId + "' ";
                }
                else
                {
                    parameters.searchBy = "ComplianceDocumentName";
                    parameters.sort = "ComplianceDocumentName";
                    parameters.order = "ASC";
                    sql = @"SELECT  D.Id,D.ResponsiblePersonId,D.CompanyGroupId,E.EmployeeName AS ResponsiblePersonName, C.Id AS ComplianceDocumentId,C.UserName AS ComplianceDocumentName,C.DocumentType FROM [HKP].[ComplianceDocument] AS C
							LEFT OUTER JOIN (SELECT * FROM  [HKP].[DocumentConfigurationDesignationGroup] WHERE PlantId='" + plantId + "' AND EmployeeCategoryId='" + employeeTypeId + @"' )AS D ON C.Id= D.ComplianceDocumentId
							LEFT OUTER JOIN [dbo].[EmployeeInformation] AS E ON D.ResponsiblePersonId = E.SystemId
                            WHERE C.CompanyGroupId='" + companyGroupId + "'";
                }

                parameters.CmdText = sql;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPC.Id AS Value, PPC.UserName AS Text FROM ORG.DocumentConfigurationDesignationGroup AS PPC
                                 ORDER BY PPC.UserName ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<DocumentConfigurationDesignationGroup> GetDbDocumentList(string plantId, string employeeTypeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT * FROM HKP.DocumentConfigurationDesignationGroup AS D
                WHERE D.CompanyGroupId='" + identity.CompanyGroupId + "' AND D.PlantId='" + plantId + "' AND D.EmployeeCategoryId='" + employeeTypeId + "' AND D.ComplianceDocumentId <>''";
                return _dcumentConfigurationDesignationGroupRepository.SqlQuery<DocumentConfigurationDesignationGroup>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<DocumentConfigurationDesignationGroup> GetDbDocumentSetList(string plantId, string employeeTypeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT * FROM HKP.DocumentConfigurationDesignationGroup AS D
                WHERE D.CompanyGroupId='" + identity.CompanyGroupId + "' AND D.PlantId='" + plantId + "' AND D.EmployeeCategoryId='" + employeeTypeId + "' AND D.ComplianceDocumentSetId <>''";
                return _dcumentConfigurationDesignationGroupRepository.SqlQuery<DocumentConfigurationDesignationGroup>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDesignationGroupDateList(string companyGroupId, string plantId, string employeeTypeId)
        {
            try
            {
                string _sql = @"SELECT DS.UserName AS ComplianceDocumentName,EI.EmployeeName AS ResponsiblePersonName,CDG.* FROM [HKP].[DocumentConfigurationDesignationGroup] AS CDG
                                LEFT OUTER JOIN  [dbo].[EmployeeInformation] AS EI ON CDG.ResponsiblePersonId=EI.SystemId
                                LEFT OUTER JOIN [HKP].[ComplianceDocumentSet] AS DS ON CDG.ComplianceDocumentSetId=DS.Id
                                WHERE CDG.PlantId='" + plantId + "' AND CDG.EmployeeCategoryId='" + employeeTypeId + "' AND CDG.CompanyGroupId='" + companyGroupId + @"'
                                ORDER BY DS.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<DocumentSetAssignDetail> GetDBList(string companyGroupId, string masterId, string complianceDocumentSetId)
        {
            try
            {
                string _sql = @" SELECT * FROM [HKP].[DocumentSetAssignDetail] A WHERE A.CompanyGroupId='" + companyGroupId + "' AND A.DocumentConfigurationDesignationGroupId='" + masterId + @"' AND A.ComplianceDocumentSetId='" + complianceDocumentSetId + @"'";
                return _sqlRepository.GetModelCollection<DocumentSetAssignDetail>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
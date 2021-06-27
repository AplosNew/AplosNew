#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Documents;
using Library.Service.Accounts;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class ComplianceDocumentService : Service<ComplianceDocument>, IComplianceDocumentService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IComplianceDocumentPositonCodeService _complianceDocumentPositonCodeService;
        private readonly IComplianceDocumentPostRecruitmentService _complianceDocumentPostRecruitmentService;
        private readonly IComplianceDocumentProofTypeAssignService _complianceDocumentProofTypeAssignService;
        private readonly IEmployeeDocumentService _employeeDocumentService;
        private readonly IDocumentSetAssignDetailService _documentSetAssignDetailService;
        private readonly IComplianceDocumentSetDetailService _complianceDocumentSetDetailService;

        public ComplianceDocumentService(
            IRepositoryAsync<ComplianceDocument> ComplianceDocumentRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , IComplianceDocumentPositonCodeService complianceDocumentPositonCodeService
            , ISqlRepository sqlRepository
              , IComplianceDocumentProofTypeAssignService complianceDocumentProofTypeAssignService
            , IComplianceDocumentPostRecruitmentService complianceDocumentPostRecruitmentService
            , IEmployeeDocumentService employeeDocumentService
            , IDocumentSetAssignDetailService documentSetAssignDetailService
            , IComplianceDocumentSetDetailService complianceDocumentSetDetailService) :
            base(ComplianceDocumentRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _complianceDocumentPositonCodeService = complianceDocumentPositonCodeService;
            _complianceDocumentProofTypeAssignService = complianceDocumentProofTypeAssignService;
            _complianceDocumentPostRecruitmentService = complianceDocumentPostRecruitmentService;
            _employeeDocumentService = employeeDocumentService;
            _documentSetAssignDetailService = documentSetAssignDetailService;
            _complianceDocumentSetDetailService = complianceDocumentSetDetailService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(ComplianceDocument), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private void Check(ComplianceDocument entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }

        public void InsertGraph(ComplianceDocument entity, IEnumerable<ComplianceDocumentPositonCode> complianceDocumentPositon, IEnumerable<ComplianceDocumentPostRecruitment> complianceDocumentPostRecruitment, IEnumerable<ComplianceDocumentProofTypeAssign> complianceDocumentProofTypeAssign)
        {
            var flag = false;
            try
            {
                Check(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                entity.Active = true;
                entity.CompanyGroupId = identity.CompanyGroupId;
                _complianceDocumentPositonCodeService.InsertOrUpdate(complianceDocumentPositon, entity.Id);
                _complianceDocumentPostRecruitmentService.InsertOrUpdate(complianceDocumentPostRecruitment, entity.Id);
                _complianceDocumentProofTypeAssignService.InsertOrUpdate(complianceDocumentProofTypeAssign, entity.Id);
                base.InsertGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateGraph(ComplianceDocument entity, IEnumerable<ComplianceDocumentPositonCode> complianceDocumentPositon, IEnumerable<ComplianceDocumentPostRecruitment> complianceDocumentPostRecruitment, IEnumerable<ComplianceDocumentProofTypeAssign> complianceDocumentProofTypeAssign)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                _complianceDocumentPositonCodeService.InsertOrUpdate(complianceDocumentPositon, entity.Id);
                _complianceDocumentPostRecruitmentService.InsertOrUpdate(complianceDocumentPostRecruitment, entity.Id);
                _complianceDocumentProofTypeAssignService.InsertOrUpdate(complianceDocumentProofTypeAssign, entity.Id);
                base.UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId, string type)
        {
            try
            {
                parameters.CmdText = @"SELECT CD.Id,CD.ProfileType,CD.QualificationLevelId,MB.Code AS ResponsiblePersonCode,CD.ComplianceDocumentCategoryId,CD.ComplianceDocumentSubCategoryId,CD.CompanyGroupId,CD.DocumentType,CD.Importance,
                                        CD.EmploymentStage,CD.DependateDate,CD.Sequence,CD.Description,CD.Remarks,CD.[Type],CD.ReNewAble,
										CD.ReNewAfterEvery,CD.ReNewUOM,CD.DocumentExpirable,CD.MailAlert,CD.DaysBeforeExpiry,CD.DocNumberRequired,CD.DocDateRequired
                                        ,CD.UserName,CD.Code,CD.ShortName,CD.StandardName,CD.EmpType,CD.IsGlobalDocument,CD.DocumentationBy,PR.UserName AS PositionName
                                        ,CD.IsSkillBased,CD.LeadOrLagDays,CD.OptionalOrMandatory,CD.ResponsiblePersonId,CD.Active,CDC.UserName AS ComplianceDocumentCategoryName,CDSC.UserName AS ComplianceDocumentSubCategoryName
                                        FROM [HKP].[ComplianceDocument] AS CD
                                        LEFT OUTER JOIN [HKP].[ComplianceDocumentCategory] AS CDC ON CD.ComplianceDocumentCategoryId= CDC.Id
                                        LEFT OUTER JOIN [HKP].[ComplianceDocumentSubCategory] AS CDSC ON CD.ComplianceDocumentSubCategoryId= CDSC.Id
										LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON CD.ResponsiblePersonId= MB.Id
				LEFT OUTER JOIN [ORG].[Position] AS PR ON MB.PositionId=PR.Id
                                         WHERE CD.CompanyGroupId='" + companyGroupId + "' AND CD.Type='" + type + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void DeleteGraph(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = base.Query(r => r.Id == Id).Select().FirstOrDefault();
                if (data != null)
                {
                    _complianceDocumentPositonCodeService.DeleteWithMaster(Id);
                    _complianceDocumentProofTypeAssignService.DeleteWithMaster(Id);

                    _complianceDocumentPostRecruitmentService.ExecuteSqlCommand("DELETE FROM HKP.ComplianceDocumentPostRecruitment WHERE ComplianceDocumentId='" + Id + "'");
                    var empDoc = _employeeDocumentService.Query(r => r.ComplianceDocumentId == Id).Select();
                    if (empDoc != null)
                    {
                        _employeeDocumentService.ExecuteSqlCommand("DELETE FROM EmployeeDocument WHERE ComplianceDocumentId='" + Id + @"' AND ISNULL(FileName,'')=''");
                    }

                    var setDetail = _complianceDocumentSetDetailService.Query(r => r.ComplianceDocumentId == Id).Select().FirstOrDefault();
                    if (setDetail != null)
                    {
                        _complianceDocumentSetDetailService.ExecuteSqlCommand("DELETE FROM HKP.ComplianceDocumentSetDetail WHERE ComplianceDocumentId='" + Id + "'");
                    }

                    var setAssignDetail = _documentSetAssignDetailService.Query(r => r.ComplianceDocumentId == Id).Select().FirstOrDefault();
                    if (setAssignDetail != null)
                    {
                        _documentSetAssignDetailService.ExecuteSqlCommand("DELETE FROM HKP.DocumentSetAssignDetail WHERE ComplianceDocumentId='" + Id + "'");
                    }

                    base.DeleteGraph(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IWorkbook GetComplianceDocumentReport(string documentLevel, string plantId)
        {
            try
            {
                ReportGeneralVoucher obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IWorkbook workbook = obj.ComplianceDocument_Report(excelEngine, documentLevel, plantId);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
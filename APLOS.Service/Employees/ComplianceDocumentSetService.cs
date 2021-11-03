#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Documents;
using Library.Model.Systems;
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
    public class ComplianceDocumentSetService : Service<ComplianceDocumentSet>, IComplianceDocumentSetService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IComplianceDocumentSetDetailService _complianceDocumentSetDetailService;

        //private readonly IComplianceDocumentPositonCodeService _complianceDocumentPositonCodeService;
        private readonly IComplianceDocumentSetProofTypeAssignService _complianceDocumentSetProofTypeAssignService;

        public ComplianceDocumentSetService(
            IRepositoryAsync<ComplianceDocumentSet> complianceDocumentSetRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IComplianceDocumentSetDetailService complianceDocumentSetDetailService
            , IComplianceDocumentSetProofTypeAssignService complianceDocumentSetProofTypeAssignService
            ) : base(complianceDocumentSetRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _complianceDocumentSetDetailService = complianceDocumentSetDetailService;
            _complianceDocumentSetProofTypeAssignService = complianceDocumentSetProofTypeAssignService;
        }

        #endregion Constructor

        private void Check(ComplianceDocumentSubCategory entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.CompanyGroupId != identity.CompanyGroupId);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.CompanyGroupId != identity.CompanyGroupId);
        }

        public void InsertGraph(ComplianceDocumentSet entity, IEnumerable<ComplianceDocumentSetDetail> complianceDocumentSetDetail, IEnumerable<ComplianceDocumentSetProofTypeAssign> complianceDocumentSetProofTypeAssign)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                Check(entity);
                entity.Id = GetPK();
                entity.Active = true;
                entity.CompanyGroupId = identity.CompanyGroupId;
                _complianceDocumentSetDetailService.InsertOrUpdate(complianceDocumentSetDetail, entity.Id);
                _complianceDocumentSetProofTypeAssignService.InsertOrUpdate(complianceDocumentSetProofTypeAssign, entity.Id);
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

        public void UpdateGraph(ComplianceDocumentSet entity, IEnumerable<ComplianceDocumentSetDetail> complianceDocumentSetDetail, IEnumerable<ComplianceDocumentSetProofTypeAssign> complianceDocumentSetProofTypeAssign)
        {
            var flag = false;
            try
            {
                //Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                _complianceDocumentSetDetailService.InsertOrUpdate(complianceDocumentSetDetail, entity.Id);
                _complianceDocumentSetProofTypeAssignService.InsertOrUpdate(complianceDocumentSetProofTypeAssign, entity.Id);
                base.UpdateGraph(entity);
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

        //public void UpdateGraph(ComplianceDocument entity, IEnumerable<ComplianceDocumentPositonCode> complianceDocumentPositon, IEnumerable<ComplianceDocumentPostRecruitment> complianceDocumentPostRecruitment, IEnumerable<ComplianceDocumentProofTypeAssign> complianceDocumentProofTypeAssign)
        //{
        //    bool flag = false;
        //    try
        //    {
        //        _unitOfWork.BeginTransaction();
        //        flag = true;
        //        _complianceDocumentPositonCodeService.InsertOrUpdate(complianceDocumentPositon, entity.Id);
        //        _complianceDocumentPostRecruitmentService.InsertOrUpdate(complianceDocumentPostRecruitment, entity.Id);
        //        _complianceDocumentProofTypeAssignService.InsertOrUpdate(complianceDocumentProofTypeAssign, entity.Id);
        //        base.UpdateGraph(entity);
        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
        //    }
        //}

        public GridModel GetComplianceDocumentList(GridParameter parameters, string companyGroupId)
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
                                         WHERE CD.CompanyGroupId='" + companyGroupId + "' AND CD.Type='EmployeeRelated'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(ComplianceDocumentSet), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ComplianceDocumentSet), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(ComplianceDocumentSet entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.CompanyGroupId != identity.CompanyGroupId);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.CompanyGroupId != identity.CompanyGroupId);
        }

        public decimal GetAutoSequence()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return base.Query(r => r.CompanyGroupId == identity.CompanyGroupId).Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
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
                    _complianceDocumentSetDetailService.DeleteWithMaster(Id);
                    _complianceDocumentSetProofTypeAssignService.DeleteWithMaster(Id);
                    var setDetail = _complianceDocumentSetDetailService.Query(r => r.ComplianceDocumentSetId == Id).Select().FirstOrDefault();
                    if (setDetail != null)
                    {
                        _complianceDocumentSetDetailService.ExecuteSqlCommand("DELETE FROM HKP.ComplianceDocumentSetDetail WHERE ComplianceDocumentSetId='" + Id + "'");
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM HKP.ComplianceDocumentSet WHERE CompanyGroupId='" + companyGroupId + "' AND Archive='0'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return from m in base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && !r.Archive).Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
    }
}
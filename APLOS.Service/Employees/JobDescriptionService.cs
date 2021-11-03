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
    public class JobDescriptionService : Service<JobDescription>, IJobDescriptionService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IJobDescriptionDetailService _jobDescriptionDetailService;
        private readonly IRepositoryAsync<JobDescription> _jobDescriptionRepository;

        public JobDescriptionService(
            IRepositoryAsync<JobDescription> jobDescriptionRepository
            , IPKGeneratorService pkGeneratorService
            , IJobDescriptionDetailService jobDescriptionDetailService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            ) : base(jobDescriptionRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _jobDescriptionDetailService = jobDescriptionDetailService;
            _sqlRepository = sqlRepository;
            _jobDescriptionRepository = jobDescriptionRepository;
        }

        #endregion Constructor

        public override void Delete(object id)
        {
            var flag = false;
            try
            {
                UseChecking(id);
                _unitOfWork.BeginTransaction();
                flag = true;
                _jobDescriptionDetailService.DeleteGraphByJobDescription(id.ToString());
                DeleteGraph(id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void UseChecking(object id)
        {
            if (_jobDescriptionRepository.FKDependency("[HKP].[JobDescription]", id.ToString(), "[HKP].[JobDescriptionDetail]"))
                throw new CustomException("Delete is not allowed after transaction.");
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(JobDescription), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertGraph(JobDescription entity, IEnumerable<JobDescriptionDetail> jobDescriptionDetail)
        {
            var flag = false;
            try
            {
                if (CheckUniqueRow(entity))
                    throw new CustomException("This combination already exists!");
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                _jobDescriptionDetailService.InsertGraph(jobDescriptionDetail, entity.Id);
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

        public void UpdateGraph(JobDescription entity, IEnumerable<JobDescriptionDetail> jobDescriptionDetail)
        {
            var flag = false;
            try
            {
                if (CheckUniqueRow(entity))
                    throw new CustomException("This combination already exists!");
                _unitOfWork.BeginTransaction();
                flag = true;
                base.UpdateGraph(entity);
                _jobDescriptionDetailService.InsertGraph(jobDescriptionDetail, entity.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
                parameters.CmdText = @"SELECT  JD.Id
		                                        ,JD.CompanyGroupId
		                                        ,JD.JobDescriptionCategoryId
	                                            ,JDC.UserName AS JobDescriptionCategoryName
		                                        ,JD.JobDescriptionSubCategoryId
		                                        ,JDSC.UserName AS JobDescriptionSubCategoryName
		                                        ,JD.JobDescriptionItemId
		                                        ,JDI.UserName AS JobDescriptionItemName
		                                        ,JD.JobLevel
		                                        ,JD.PrimaryOrSecondary
		                                        ,JD.Frequency
		                                        ,JD.NatureOfActivity
		                                        ,JD.SystemOrManual
		                                        ,JD.DocumentApplicable
		                                        ,JD.EstimatedTimeRequired
                                                ,JAM.TotalAttachment
                                        FROM [HKP].[JobDescription] AS JD
                                        LEFT OUTER JOIN [HKP].[JobDescriptionCategory] JDC ON JD.JobDescriptionCategoryId = JDC.Id
                                        LEFT OUTER JOIN [HKP].[JobDescriptionSubCategory] JDSC ON JD.JobDescriptionSubCategoryId = JDSC.Id
                                        LEFT OUTER JOIN [HKP].[JobDescriptionItem] JDI ON JD.JobDescriptionItemId = JDI.Id
                                        LEFT OUTER JOIN (SELECT COUNT(Id) TotalAttachment,JobDescriptionId FROM [HKP].[JobDescriptionDetail]    group by JobDescriptionId) JAM on jam.JobDescriptionId=jd.Id
                                        WHERE JD.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        /// <summary>
        /// This list data show without grid existing jobDescriptionId
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="companyGroupId"></param>
        /// <param name="jobDescriptionIds"></param>
        /// <returns></returns>
        public GridModel Query(GridParameter parameters, string companyGroupId, string[] jobDescriptionIds)
        {
            try
            {
                var jobDescriptionId = "";
                if (jobDescriptionIds.Length > 0)
                    jobDescriptionId = string.Join(",", jobDescriptionIds.Select(item => "'" + item + "'"));
                else
                    jobDescriptionId = "' '";
                parameters.CmdText = @"SELECT  JD.Id
		                                        ,JD.CompanyGroupId
		                                        ,JD.JobDescriptionCategoryId
	                                            ,JDC.UserName AS JobDescriptionCategoryName
		                                        ,JD.JobDescriptionSubCategoryId
		                                        ,JDSC.UserName AS JobDescriptionSubCategoryName
		                                        ,JD.JobDescriptionItemId
		                                        ,JDI.UserName AS JobDescriptionItemName
		                                        ,JD.JobLevel
		                                        ,JD.PrimaryOrSecondary
		                                        ,JD.Frequency
		                                        ,JD.NatureOfActivity
		                                        ,JD.SystemOrManual
		                                        ,JD.DocumentApplicable
		                                        ,JD.EstimatedTimeRequired
                                                ,JAM.TotalAttachment
                                        FROM [HKP].[JobDescription] AS JD
                                        LEFT OUTER JOIN [HKP].[JobDescriptionCategory] JDC ON JD.JobDescriptionCategoryId = JDC.Id
                                        LEFT OUTER JOIN [HKP].[JobDescriptionSubCategory] JDSC ON JD.JobDescriptionSubCategoryId = JDSC.Id
                                        LEFT OUTER JOIN [HKP].[JobDescriptionItem] JDI ON JD.JobDescriptionItemId = JDI.Id
                                        LEFT OUTER JOIN (SELECT COUNT(Id) TotalAttachment,JobDescriptionId FROM [HKP].[JobDescriptionDetail]    group by JobDescriptionId) JAM on jam.JobDescriptionId=jd.Id
                                        WHERE JD.CompanyGroupId='" + companyGroupId + "'  AND JD.Id NOT IN (" + jobDescriptionId + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private bool CheckUniqueRow(JobDescription jobDescription)
        {
            try
            {
                CustomIdentity identiy = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Any(r => r.Id != jobDescription.Id && r.CompanyGroupId == identiy.CompanyGroupId && r.JobDescriptionCategoryId == jobDescription.JobDescriptionCategoryId
                  && r.JobDescriptionSubCategoryId == jobDescription.JobDescriptionSubCategoryId && r.JobDescriptionItemId == jobDescription.JobDescriptionItemId && r.NatureOfActivity == jobDescription.NatureOfActivity
                  && r.PrimaryOrSecondary == jobDescription.PrimaryOrSecondary && r.SystemOrManual == jobDescription.SystemOrManual && r.Frequency == jobDescription.Frequency && r.JobLevel == jobDescription.JobLevel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        
        public IEnumerable<object> GetEmployeeJobDescription(string employeeId)
        {
            try
            {
                //string _sql = @"SELECT EJD.Id EmployeeJobDescriptionId
                //                       	   ,EJD.EmployeeId
                //                       	   ,JDC.UserName JobDescriptionCategory
                //                       	   ,JDSC.UserName JobDescriptionSubCategory
                //                       	   ,JDI.UserName JobDescriptionItem
                //                       	   ,JD.Id JobDescriptionId
                //                       	   ,JD.JobLevel
                //                       	   ,JD.PrimaryOrSecondary
                //                       	   ,JD.Frequency
                //                       	   ,JD.NatureOfActivity
                //                       	   ,JD.SystemOrManual
                //                       	   ,JD.DocumentApplicable
                //                       	   ,JD.EstimatedTimeRequired
                //                       FROM [TRN].[EmployeeJobDescription] EJD
                //                       LEFT OUTER JOIN  [HKP].[JobDescription] JD ON EJD.JobDescriptionId=JD.Id
                //                       LEFT OUTER JOIN  [HKP].[JobDescriptionCategory] JDC ON JD.JobDescriptionCategoryId=JDC.Id
                //                       LEFT OUTER JOIN  [HKP].[JobDescriptionSubCategory] JDSC ON JD.JobDescriptionSubCategoryId=JDSC.Id
                //                       LEFT OUTER JOIN  [HKP].[JobDescriptionItem] JDI ON JD.JobDescriptionItemId=JDI.Id
                //                       WHERE EJD.EmployeeId='" + employeeId + "'";
                string _sql = @"SELECT  SA.Id,SC.UserName SOPCategory, SA.[Name] ActivityName, SA.ActivityImportanceId, SA.AverageTime, ISNULL(SAD.SOPDocument,0) SOPDocument, ISNULL(SOPAD.SOPItemDocument,0) SOPItemDocument, SOPAD.SOPItemId, SI.UserName SOPItem
                              ,MB.Code, P.UserName Position,EN.UserName Entity
                              FROM  HKP.SOPActivity SA
                              LEFT JOIN HKP.SOPItem SI ON SI.Id=SA.SOPItemId
                              LEFT JOIN EmployeeInformation E ON E.PositionID=SA.PositionId
                              LEFT JOIN HKP.SOPCategory SC ON SC.Id=SI.SOPCategoryId
                              LEFT JOIN (SELECT SOPActivityId,COUNT(SOPDocumentId) SOPDocument FROM [HKP].[SOPActivityDocument] Group By SOPActivityId) SAD ON SAD.SOPActivityId=SA.Id
                              LEFT JOIN (SELECT SOPItemId,COUNT(Id) SOPItemDocument FROM [HKP].[SOPAttachmentDetail] Group By SOPItemId) SOPAD ON SOPAD.SOPItemId=SI.Id
                              LEFT JOIN MST.ManpowerBudget MB ON MB.Id=E.BudgetCode
                              LEFT JOIN ORG.Position P ON P.Id=E.PositionID
                              LEFT JOIN ORG.Entity EN ON EN.Id=MB.EntityId
                              WHERE E.SystemId='" + employeeId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetFileByJDId(string jdId)
        {
            try
            {
                string _sql = @"SELECT JDD.Id,JDD.JobDescriptionId,JDD.FileName,JDD.FileId From  [HKP].[JobDescriptionDetail] JDD
                                WHERE JDD.JobDescriptionId='" + jdId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetActivityDocumentList(string SOPActivityId)
        {
            try
            {
                string _sql = @"SELECT SD.Code,SD.ShortName,SD.StandardName,SD.UserName,SD.DataSourceCategory,SD.FileName,sd.FileId from [HKP].[SOPActivityDocument] SAD
                                LEFT JOIN [HKP].[SOPDocument] SD ON SD.Id=SAD.SOPDocumentId
                                WHERE SAD.SOPActivityId='" + SOPActivityId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetSOPDocumentList(string SOPItemId)
        {
            try
            {
                string _sql = @"SELECT * FROM [HKP].[SOPAttachmentDetail] where SOPItemId='"+ SOPItemId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
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
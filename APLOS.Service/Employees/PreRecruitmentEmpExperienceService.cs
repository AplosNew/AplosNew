#region Using

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

#endregion Using

namespace Library.Service.Employees
{
    public class PreRecruitmentEmpExperienceService : Service<PreRecruitmentEmpExperience>, IPreRecruitmentEmpExperienceService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IComplianceDocumentService _complianceDocumentService;
        private readonly IPreRecruitmentDocumentService _preDocumentService;

        public PreRecruitmentEmpExperienceService(
            IRepositoryAsync<PreRecruitmentEmpExperience> PreRecruitmentEmpExperienceRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , IComplianceDocumentService complianceDocumentService
            , ISqlRepository sqlRepository
            , IPreRecruitmentDocumentService preDocumentService) :
            base(PreRecruitmentEmpExperienceRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _complianceDocumentService = complianceDocumentService;
            _preDocumentService = preDocumentService;
        }

        #endregion Constructor

        public IEnumerable<object> GetData(string empSystemID)
        {
            try
            {
                var sql = @"SELECT
                            	 Ex.SystemID
                            	,Ex.PreRecruitmentEmployeeId
                            	,Ex.Employer
                            	,Ex.Designation
                            	,Replace(CONVERT(VARCHAR(11),Ex.StartDate, 106), ' ', '-') StartDate
                            	,Replace(CONVERT(VARCHAR(11),Ex.EndDate, 106), ' ', '-') EndDate
                            	,Ex.JobDescription
                            	,Ex.Achievement
                            	,Ex.IsPartTime
                            	,Ex.IsCurrentJob
                            	,Ex.DurationYear
                            	,Ex.DurationMonth
                            	,Ex.AddedBy
                                ,Ex.FileId
								,Ex.[FileName]
                            	,Replace(CONVERT(VARCHAR(11), Ex.AddedDate, 106), ' ', '-') AddedDate
                            	,Ex.AddedFromIP
                            	,Ex.UpdatedBy
                            	,Replace(CONVERT(VARCHAR(11), Ex.UpdatedDate, 106), ' ', '-') UpdatedDate
                            	,Ex.UpdatedFromIP
								,Ex.IsExperienceApproved
                            FROM PreRecruitmentEmpExperience Ex where Ex.PreRecruitmentEmployeeId = '" + empSystemID + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void Check(PreRecruitmentEmpExperience entity)
        {
            try
            {
                var dbData = Query(t => t.SystemID != entity.SystemID && t.PreRecruitmentEmployeeId == entity.PreRecruitmentEmployeeId
                && t.Designation == entity.Designation && t.JobDescription == entity.JobDescription).Select().FirstOrDefault();
                if (dbData != null)
                    throw new Exception("This Designation and Job Description already exists !!!");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region PreRecruitmentEmpExperience

        public void InsertORUpdateMaster(PreRecruitmentEmpExperience entity)
        {
            var flag = false;
            try
            {
                flag = true;
                _unitOfWork.BeginTransaction();
                if (!string.IsNullOrEmpty(entity.FileName))
                {
                    var id = Query(t => t.SystemID != entity.SystemID && t.PreRecruitmentEmployeeId == entity.PreRecruitmentEmployeeId && t.FileName == entity.FileName).Select(t => t.SystemID).FirstOrDefault();
                    if (id != null) throw new CustomException("This file is already exists!!!");
                }

                if (entity != null)
                {
                    if (string.IsNullOrEmpty(entity.SystemID))
                    {
                        entity.SystemID = "EX" + GetAutoNumber(nameof(PreRecruitmentEmpExperience), PKGeneratorEnum.Auto, null, DateTime.Now);
                        //var complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Experience").Select(r => r.Id).FirstOrDefault();
                        var d = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Experience").Select()
                                 join t in _preDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                 select new { complianceDocumentid = a.Id }).FirstOrDefault();

                        if (d != null)
                        {
                            entity.ComplianceDocumentId = d.complianceDocumentid;
                        }
                        else
                        {
                            entity.ComplianceDocumentId = null;
                        }
                        entity.FileId = entity.SystemID;
                        if (string.IsNullOrEmpty(entity.FileName))
                        {
                            entity.FileId = null;
                        }
                        entity.AddedDate = DateTime.Now;
                        Insert(entity);

                        if (!string.IsNullOrEmpty(entity.ComplianceDocumentId))
                        {
                            var predocdata = _preDocumentService.Query(t => t.PreRecruitmentEmployeeId == entity.PreRecruitmentEmployeeId && t.ComplianceDocumentId == entity.ComplianceDocumentId).Select().FirstOrDefault();
                            if (predocdata != null)
                            {
                                predocdata.FileId = entity.FileId;
                                predocdata.FileName = entity.FileName;
                                predocdata.UpdatedDate = entity.AddedDate;

                                _preDocumentService.Update(predocdata);
                            }
                        }
                    }
                    else
                    {
                        var dbdata = Find(entity.SystemID);
                        if (dbdata == null || string.IsNullOrEmpty(dbdata.SystemID))
                            throw new CustomException("The record no longer exists.");
                        //var complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Experience").Select(r => r.Id).FirstOrDefault();

                        var d = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Experience").Select()
                                 join t in _preDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                 select new { complianceDocumentid = a.Id }).FirstOrDefault();
                        if (d != null)
                        {
                            entity.ComplianceDocumentId = d.complianceDocumentid;
                        }
                        else
                        {
                            entity.ComplianceDocumentId = null;
                        }
                        entity.FileId = entity.SystemID;
                        if (string.IsNullOrEmpty(entity.FileName))
                        {
                            entity.FileId = null;
                        }
                        entity.UpdatedDate = DateTime.Now;
                        Update(entity);

                        if (!string.IsNullOrEmpty(entity.ComplianceDocumentId))
                        {
                            var predocdata = _preDocumentService.Query(t => t.PreRecruitmentEmployeeId == entity.PreRecruitmentEmployeeId && t.ComplianceDocumentId == entity.ComplianceDocumentId).Select().FirstOrDefault();
                            if (predocdata != null)
                            {
                                predocdata.FileId = entity.FileId;
                                predocdata.FileName = entity.FileName;
                                predocdata.UpdatedDate = entity.UpdatedDate;

                                _preDocumentService.Update(predocdata);
                            }
                        }
                    }
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
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public Dictionary<string, object> GetExperienceFile(string systemId)
        {
            try
            {
                var sql = @"Select FileId, FileName From [dbo].[PreRecruitmentEmpExperience]  Where SystemID='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion PreRecruitmentEmpExperience
    }
}
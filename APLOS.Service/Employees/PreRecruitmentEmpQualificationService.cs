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
    public class PreRecruitmentEmpQualificationService : Service<PreRecruitmentEmpQualification>, IPreRecruitmentEmpQualificationService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IComplianceDocumentService _complianceDocumentService;
        private readonly IPreRecruitmentDocumentService _preDocumentService;

        public PreRecruitmentEmpQualificationService(
            IRepositoryAsync<PreRecruitmentEmpQualification> PreRecruitmentEmpQualificationRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , IComplianceDocumentService complianceDocumentService
            , ISqlRepository sqlRepository
            , IPreRecruitmentDocumentService preDocumentService) :
            base(PreRecruitmentEmpQualificationRepository, unitOfWork, pkGeneratorService)
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
                var sql = @"SELECT Qu.*,C.UserName AS Country, QL.UserName AS EducationLevel, QS.UserName AS Stream from PreRecruitmentEmpQualification Qu
                            LEFT OUTER JOIN SCS.Country C ON Qu.CountryId=C.Id
                            LEFT OUTER JOIN SCS.QualificationLevel QL ON Qu.EductLevelSystemID=QL.Id
                            LEFT OUTER JOIN SCS.QualificationStream QS ON Qu.StreamId=QS.Id
                            WHERE PreRecruitmentEmployeeId='" + empSystemID + "' ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void Check(PreRecruitmentEmpQualification entity)
        {
            try
            {
                var dbData = Query(t => t.SystemID != entity.SystemID && t.EductLevelSystemID == entity.EductLevelSystemID && t.StreamId == entity.StreamId
                    && t.ExamDegreeType == entity.ExamDegreeType && t.YearOfPass == entity.YearOfPass).Select().FirstOrDefault();
                if (dbData != null)
                    throw new Exception("This combination already exists !!!");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region PreRecruitmentEmpQualification

        public void InsertORUpdateMaster(PreRecruitmentEmpQualification entity)
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
                        entity.SystemID = "Q" + GetAutoNumber(nameof(PreRecruitmentEmpQualification), PKGeneratorEnum.Auto, null, DateTime.Now);
                        //var complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == entity.EductLevelSystemID).Select(r => r.Id).FirstOrDefault();

                        var d = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == entity.EductLevelSystemID).Select()
                                 join t in _preDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                 select new { complianceDocumentid = a.Id }).FirstOrDefault();

                        if (d == null)
                        {
                            //complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == null).Select(r => r.Id).FirstOrDefault();
                            var c = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == null).Select()
                                     join t in _preDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                     select new { complianceDocumentid = a.Id }).FirstOrDefault();
                            entity.ComplianceDocumentId = c.complianceDocumentid;
                        }
                        else
                            entity.ComplianceDocumentId = d.complianceDocumentid;
                        entity.FileId = entity.SystemID;
                        if (string.IsNullOrEmpty(entity.FileName))
                        {
                            entity.FileId = null;
                        }
                        entity.AddedDate = DateTime.Now;
                        Insert(entity);

                        var predocdata = _preDocumentService.Query(t => t.PreRecruitmentEmployeeId == entity.PreRecruitmentEmployeeId && t.ComplianceDocumentId == entity.ComplianceDocumentId).Select().FirstOrDefault();
                        if (predocdata != null)
                        {
                            predocdata.FileId = entity.FileId;
                            predocdata.FileName = entity.FileName;
                            predocdata.UpdatedDate = entity.AddedDate;

                            _preDocumentService.Update(predocdata);
                        }
                    }
                    else
                    {
                        var dbdata = Find(entity.SystemID);
                        if (dbdata == null || string.IsNullOrEmpty(dbdata.SystemID))
                            throw new CustomException("The record no longer exists.");
                        //var complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == entity.EductLevelSystemID).Select(r => r.Id).FirstOrDefault();

                        var d = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == entity.EductLevelSystemID).Select()
                                 join t in _preDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                 select new { complianceDocumentid = a.Id }).FirstOrDefault();
                        if (d == null)
                        {
                            //complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == null).Select(r => r.Id).FirstOrDefault();
                            var c = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == null).Select()
                                     join t in _preDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                     select new { complianceDocumentid = a.Id }).FirstOrDefault();
                            entity.ComplianceDocumentId = c.complianceDocumentid;
                        }
                        else
                            entity.ComplianceDocumentId = d.complianceDocumentid;

                        entity.FileId = entity.SystemID;
                        if (string.IsNullOrEmpty(entity.FileName))
                        {
                            entity.FileId = null;
                        }
                        entity.UpdatedDate = DateTime.Now;
                        Update(entity);

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

        public Dictionary<string, object> GetQualificationFile(string systemId)
        {
            try
            {
                var sql = @"Select FileId, FileName From [dbo].[PreRecruitmentEmpQualification]  Where SystemID='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion PreRecruitmentEmpQualification
    }
}
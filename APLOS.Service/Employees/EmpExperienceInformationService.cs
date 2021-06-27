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
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class EmpExperienceInformationService : Service<EmpExperienceInformation>, IEmpExperienceInformationService
    {
        #region Constructor

        private readonly IRepositoryAsync<PreRecruitmentEmpExperience> _preRecruitmentEmpExperience;
        private readonly IRepositoryAsync<EmpExperienceInformation> _empExperienceInformationRepository;
        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IComplianceDocumentService _complianceDocumentService;
        private readonly IEmployeeDocumentService _empDocumentService;

        public EmpExperienceInformationService(
            IRepositoryAsync<EmpExperienceInformation> empExperienceInformationRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , IRepositoryAsync<PreRecruitmentEmpExperience> preRecruitmentEmpExperience
            , IComplianceDocumentService complianceDocumentService
            , IEmployeeDocumentService empDocumentService
            , ISqlRepository sqlRepository
            ) : base(empExperienceInformationRepository, unitOfWork, pkGeneratorService)
        {
            _preRecruitmentEmpExperience = preRecruitmentEmpExperience;
            _empExperienceInformationRepository = empExperienceInformationRepository;
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _complianceDocumentService = complianceDocumentService;
            _empDocumentService = empDocumentService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return _signatrueService.GetAutoNumber("EMP_EXPERIENCE", DateTime.Now).ToString();
        }

        private IEnumerable<EmpExperienceInformation> Getlist(string empid)
        {
            string _sql = "SELECT * FROM EmpExperienceInformation WHERE EmpSystemID ='" + empid + "'";
            return _empExperienceInformationRepository.SelectQuery(_sql).AsEnumerable();
        }

        private IEnumerable<PreRecruitmentEmpExperience> GetOldlist(string empIdOld)
        {
            try
            {
                string _sql = "SELECT * FROM PreRecruitmentEmpExperience WHERE PreRecruitmentEmployeeId ='" + empIdOld + "'";
                return _preRecruitmentEmpExperience.SelectQuery(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitData(string empid, string empIdOld, out List<EmpExperienceInformation> from_db)
        {
            IEnumerable<PreRecruitmentEmpExperience> from_ui = null;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_ui = GetOldlist(empIdOld);
                from_db = Getlist(empid).ToList<EmpExperienceInformation>();
                foreach (var db in from_db)
                {
                    var ui = from_ui.Where(a => a.SystemID == db.SystemID).FirstOrDefault();
                    if (ui == null || ui.SystemID == null)
                    {
                        db.ModelState = ModelState.Deleted;
                    }
                }
                var _pk = GetPK();
                var pkCount = 0;
                foreach (var ui in from_ui)
                {
                    var db = from_db.Where(a => a.SystemID == ui.SystemID).FirstOrDefault();
                    if (db == null || db.SystemID == null)
                    {
                        pkCount++;
                        db = new EmpExperienceInformation
                        {
                            ModelState = ModelState.Added
                        };
                        AuditService.Log(db);
                        db.SystemID = "EE" + DateTime.Now.ToString("yy") + "-" + _pk + "-" + pkCount;//set pk
                        from_db.Add(db);
                    }
                    else
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                    }
                    MoveImage(ui.SystemID, ui.FileName);
                    db.FileId = ui.FileId;
                    db.FileName = ui.FileName;
                    db.EmpSystemID = empid;
                    db.Employer = ui.Employer;
                    db.Designation = ui.Designation;
                    db.StartDate = ui.StartDate;
                    db.EndDate = ui.EndDate;
                    db.JobDescription = ui.JobDescription;
                    db.Achievement = ui.Achievement;
                    db.IsPartTime = ui.IsPartTime;
                    db.IsCurrentJob = ui.IsCurrentJob;
                    db.DurationYear = ui.DurationYear;
                    db.DurationMonth = ui.DurationMonth;
                    db.ComplianceDocumentId = ui.ComplianceDocumentId;
                    db.DateAdded = DateTime.Now;
                    db.DateUpdated = DateTime.Now; ;
                    db.AddedBy = identity.Name;
                    db.UpdatedBy = identity.Name;
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveList(string empid, string empidOld)
        {
            List<EmpExperienceInformation> from_db = null;
            //IEnumerable<EmpExperienceInformation> from_db = null;
            //var flag = false;
            try
            {
                InitData(empid, empidOld, out from_db);

                foreach (var item in from_db)
                {
                    InsertOrUpdateGraph(item);
                }

                //_unitOfWork.BeginTransaction();
                //flag = true;
                //_unitOfWork.SaveChanges();
                //flag = false;
                //_unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<PreRecruitmentEmpExperience> GetPreRecruitmentEmpExperienceList(string PKs)
        {
            try
            {
                string _sql = "SELECT * FROM PreRecruitmentEmpExperience WHERE PreRecruitmentEmployeeId IN (" + PKs + ")";
                return _preRecruitmentEmpExperience.SelectQuery(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void MoveImage(string fromName, string toName)
        {
            try
            {
                var Fromdirectory = ResourcesPathReader.GetExperienceSourcePath();

                //new AppSettingsReader().GetValue("USEREXP_SOURCE", typeof(string)).ToString(); //get pic from web config
                var Todirectory = ResourcesPathReader.GetExperienceDestinationPath();
                //new AppSettingsReader().GetValue("USEREXP_DESTINATION", typeof(string)).ToString();

                string path = Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName));
                if (File.Exists(path))
                {
                    File.Copy(Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName)), Path.Combine(Todirectory, fromName + Path.GetExtension(toName)), true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetData(string empSystemID)
        {
            try
            {
                var sql = @"SELECT
                            	 Ex.SystemID
                            	,Ex.EmpSystemID
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
                            	,Replace(CONVERT(VARCHAR(11), Ex.DateAdded, 106), ' ', '-') AddedDate
                            	,Ex.UpdatedBy
                            	,Replace(CONVERT(VARCHAR(11), Ex.DateUpdated, 106), ' ', '-') UpdatedDate
								,Ex.IsExperienceApproved
                            FROM EmpExperienceInformation Ex where Ex.EmpSystemID  = '" + empSystemID + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertORUpdateMaster(EmpExperienceInformation entity)
        {
            var flag = false;
            try
            {
                flag = true;
                _unitOfWork.BeginTransaction();
                if (!string.IsNullOrEmpty(entity.FileName))
                {
                    var id = Query(t => t.SystemID != entity.SystemID && t.EmpSystemID == entity.EmpSystemID && t.FileName == entity.FileName).Select(t => t.SystemID).FirstOrDefault();
                    if (id != null) throw new CustomException("This file is already exists!!!");
                }

                if (entity != null)
                {
                    if (string.IsNullOrEmpty(entity.SystemID))
                    {
                        entity.SystemID = "EX" + GetAutoNumber(nameof(EmpExperienceInformation), PKGeneratorEnum.Auto, null, DateTime.Now);
                        //var complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Experience").Select(r => r.Id).FirstOrDefault();
                        var d = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Experience").Select()
                                 join t in _empDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
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
                        entity.DateAdded = DateTime.Now;
                        Insert(entity);

                        if (!string.IsNullOrEmpty(entity.ComplianceDocumentId))
                        {
                            var predocdata = _empDocumentService.Query(t => t.EmpSystemID == entity.EmpSystemID && t.ComplianceDocumentId == entity.ComplianceDocumentId).Select().FirstOrDefault();
                            if (predocdata != null)
                            {
                                predocdata.FileId = entity.FileId;
                                predocdata.FileName = entity.FileName;
                                predocdata.UpdatedDate = entity.DateAdded;

                                _empDocumentService.Update(predocdata);
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
                                 join t in _empDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
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
                        entity.DateUpdated = DateTime.Now;
                        Update(entity);

                        if (!string.IsNullOrEmpty(entity.ComplianceDocumentId))
                        {
                            var predocdata = _empDocumentService.Query(t => t.EmpSystemID == entity.EmpSystemID && t.ComplianceDocumentId == entity.ComplianceDocumentId).Select().FirstOrDefault();
                            if (predocdata != null)
                            {
                                predocdata.FileId = entity.FileId;
                                predocdata.FileName = entity.FileName;
                                predocdata.UpdatedDate = entity.DateUpdated;

                                _empDocumentService.Update(predocdata);
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
                var sql = @"Select FileId, FileName From [dbo].[EmpExperienceInformation]  Where SystemID='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
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
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
    public class EmpTrainingInformationService : Service<EmpTrainingInformation>, IEmpTrainingInformationService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IComplianceDocumentService _complianceDocumentService;
        private readonly IEmployeeDocumentService _empDocumentService;

        public EmpTrainingInformationService(
            IRepositoryAsync<EmpTrainingInformation> PreRecruitmentEmpTrainingRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IComplianceDocumentService complianceDocumentService
            , IEmployeeDocumentService empDocumentService) :
            base(PreRecruitmentEmpTrainingRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _signatrueService = signatrueService;
            _sqlRepository = sqlRepository;
            _complianceDocumentService = complianceDocumentService;
            _empDocumentService = empDocumentService;
        }

        #endregion Constructor

        private string GetPK()
        {
            //return base.GetAutoNumber("EmpTrainingInformation", PKGeneratorEnum.Auto, null, DateTime.Now);
            return _signatrueService.GetAutoNumber("EMP_TRAI", DateTime.Now).ToString();
        }

        private IEnumerable<EmpTrainingInformation> Getlist(string empid)//TBT
        {
            try
            {
                string _sql = "SELECT * FROM EmpTrainingInformation WHERE EmpSystemID ='" + empid + "'";
                return _sqlRepository.GetModelCollection<EmpTrainingInformation>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<PreRecruitmentEmpTraining> GetOldlist(string empIdOld)//TBT
        {
            try
            {
                string _sql = "SELECT * FROM PreRecruitmentEmpTraining WHERE PreRecruitmentEmployeeId ='" + empIdOld + "'";
                return _sqlRepository.GetModelCollection<PreRecruitmentEmpTraining>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitData(string empid, string empIdOld, out List<EmpTrainingInformation> from_db)
        {
            IEnumerable<PreRecruitmentEmpTraining> from_ui = null;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_ui = GetOldlist(empIdOld);
                from_db = Getlist(empid).ToList<EmpTrainingInformation>();
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
                        db = new EmpTrainingInformation
                        {
                            ModelState = ModelState.Added
                        };
                        AuditService.Log(db);
                        db.SystemID = "ET" + DateTime.Now.ToString("yy") + "-" + _pk + "-" + pkCount;//set pk
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
                    db.TrainingTitle = ui.TrainingTitle;
                    db.TopicCovered = ui.TopicCovered;
                    db.InstituteName = ui.InstituteName;
                    db.CountrySystemID = ui.CountrySystemID;
                    db.Location = ui.Location;
                    db.TrainingYear = ui.TrainingYear;
                    db.Duration = Convert.ToInt32(ui.Duration);
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
            List<EmpTrainingInformation> from_db = null;
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
            finally
            {
                //if (flag)
                //    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<PreRecruitmentEmpTraining> GetPreRecruitmentEmpTrainingList(string PKs)//TBT
        {
            try
            {
                string _sql = "SELECT * FROM PreRecruitmentEmpTraining WHERE PreRecruitmentEmployeeId IN (" + PKs + ")";
                return _sqlRepository.GetModelCollection<PreRecruitmentEmpTraining>(_sql, null);
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
                var Fromdirectory = ResourcesPathReader.GetTrainingSourcePath();//new AppSettingsReader().GetValue("USERTRA_SOURCE", typeof(string)).ToString(); //get pic from web config
                var Todirectory = ResourcesPathReader.GetTrainingDestinationPath();// new AppSettingsReader().GetValue("USERTRA_DESTINATION", typeof(string)).ToString();

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
                var sql = @"Select * from EmpTrainingInformation where EmpSystemID ='" + empSystemID + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertORUpdateMaster(EmpTrainingInformation entity)
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
                        entity.SystemID = "TR" + GetAutoNumber(nameof(PreRecruitmentEmpTraining), PKGeneratorEnum.Auto, null, DateTime.Now);
                        //var complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Training").Select(r => r.Id).FirstOrDefault();
                        var d = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Training").Select()
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
                        entity.DateUpdated = DateTime.Now;

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
                        //var dbdata = base.Find(entity.SystemID);
                        //if (dbdata == null || string.IsNullOrEmpty(dbdata.SystemID))
                        //	throw new CustomException("The record no longer exists.");
                        //var complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Training").Select(r => r.Id).FirstOrDefault();
                        var d = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Training").Select()
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

        public Dictionary<string, object> GetTrainingFile(string systemId)
        {
            try
            {
                var sql = @"Select FileId, FileName From [dbo].[EmpTrainingInformation]  Where SystemID='" + systemId + "'";
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
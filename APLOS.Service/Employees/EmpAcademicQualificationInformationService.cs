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
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class EmpAcademicQualificationInformationService : Service<EmpAcademicQualificationInformation>, IEmpAcademicQualificationInformationService
    {
        #region Constructor

        private readonly IRepositoryAsync<EmpAcademicQualificationInformation> _preRecruitmentEmpQualificationRepository;
        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IComplianceDocumentService _complianceDocumentService;
        private readonly IEmployeeDocumentService _empDocumentService;

        public EmpAcademicQualificationInformationService(
            IRepositoryAsync<EmpAcademicQualificationInformation> preRecruitmentEmpQualificationRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , IComplianceDocumentService complianceDocumentService
            , ISqlRepository sqlRepository
            , IEmployeeDocumentService empDocumentService) :
            base(preRecruitmentEmpQualificationRepository, unitOfWork, pkGeneratorService)
        {
            _preRecruitmentEmpQualificationRepository = preRecruitmentEmpQualificationRepository;
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _complianceDocumentService = complianceDocumentService;
            _empDocumentService = empDocumentService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return _signatrueService.GetAutoNumber("EMP_ACADE", DateTime.Now).ToString();
            //return base.GetAutoNumber("EmpAcademicQualificationInformation", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private IEnumerable<EmpAcademicQualificationInformation> Getlist(string empid)//TBT
        {
            try
            {
                string _sql = "SELECT * FROM EmpAcademicQualificationInformation WHERE EmpSystemID ='" + empid + "'";
                return _preRecruitmentEmpQualificationRepository.SqlQuery<EmpAcademicQualificationInformation>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<PreRecruitmentEmpQualification> GetOldlist(string empIdOld)
        {
            try
            {
                string _sql = "SELECT * FROM PreRecruitmentEmpQualification WHERE PreRecruitmentEmployeeId ='" + empIdOld + "'";
                return _preRecruitmentEmpQualificationRepository.SqlQuery<PreRecruitmentEmpQualification>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitData(string empid, string empIdOld, out List<EmpAcademicQualificationInformation> from_db)
        {
            IEnumerable<PreRecruitmentEmpQualification> from_ui = null;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_ui = GetOldlist(empIdOld);
                from_db = Getlist(empid).ToList<EmpAcademicQualificationInformation>();
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
                        db = new EmpAcademicQualificationInformation
                        {
                            ModelState = ModelState.Added
                        };
                        AuditService.Log(db);
                        db.SystemID = "EQ" + DateTime.Now.ToString("yy") + "-" + _pk + "-" + pkCount;//set pk
                        from_db.Add(db);
                    }
                    else
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                    }

                    MoveImage(ui.SystemID, ui.FileName);
                    db.EmpSystemID = empid;
                    db.FileId = ui.FileId;
                    db.FileName = ui.FileName;
                    db.TypeIsAcademic = ui.TypeIsAcademic;
                    db.EductLevelSystemID = ui.EductLevelSystemID;
                    db.IsEnglishMedium = ui.IsEnglishMedium;
                    db.HasDistinction = ui.HasDistinction;
                    db.ExamDegreeType = ui.ExamDegreeType;
                    db.StreamId = ui.StreamId;
                    db.InstituteName = ui.InstituteName;
                    db.CountryId = ui.CountryId;
                    db.YearOfPass = ui.YearOfPass.ToString();
                    db.Session = ui.Session;
                    db.Achievement = ui.Achievement;
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
            List<EmpAcademicQualificationInformation> from_db = null;
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

        public IEnumerable<PreRecruitmentEmpQualification> GetPreRecruitmentEmpQualificationList(string PKs)//TBT
        {
            try
            {
                string _sql = "SELECT * FROM PreRecruitmentEmpQualification WHERE PreRecruitmentEmployeeId IN (" + PKs + ")";
                return _preRecruitmentEmpQualificationRepository.SqlQuery<PreRecruitmentEmpQualification>(_sql).AsEnumerable();
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
                var Fromdirectory = ResourcesPathReader.GetQualificationSourcePath();
                //new AppSettingsReader().GetValue("USERQUA_SOURCE", typeof(string)).ToString(); //get pic from web config
                var Todirectory = ResourcesPathReader.GetQualificationDestinationPath();
                //new AppSettingsReader().GetValue("USERQUA_DESTINATION", typeof(string)).ToString();

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
                var sql = @"SELECT Qu.*,C.UserName AS Country, QL.UserName AS EducationLevel, QS.UserName AS Stream from EmpAcademicQualificationInformation Qu
                            LEFT OUTER JOIN SCS.Country C ON Qu.CountryId=C.Id
                            LEFT OUTER JOIN SCS.QualificationLevel QL ON Qu.EductLevelSystemID=QL.Id
                            LEFT OUTER JOIN SCS.QualificationStream QS ON Qu.StreamId=QS.Id
                            WHERE Qu.EmpSystemID='" + empSystemID + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertORUpdateMaster(EmpAcademicQualificationInformation entity)
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
                        entity.SystemID = "Q" + GetAutoNumber(nameof(EmpAcademicQualificationInformation), PKGeneratorEnum.Auto, null, DateTime.Now);
                        //var complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == entity.EductLevelSystemID).Select(r => r.Id).FirstOrDefault();
                        var empdoc= _empDocumentService.Query(t => t.EmpSystemID == entity.EmpSystemID).Select().FirstOrDefault();
                        if (empdoc!=null)
                        {
                            var d = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == entity.EductLevelSystemID).Select()
                                     join t in _empDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                     select new { complianceDocumentid = a.Id }).FirstOrDefault();

                            if (d == null)
                            {
                                //complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == null).Select(r => r.Id).FirstOrDefault();
                                var c = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == null).Select()
                                         join t in _empDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                         select new { complianceDocumentid = a.Id }).FirstOrDefault();
                                if (c!=null)
                                {
                                    entity.ComplianceDocumentId = c.complianceDocumentid; 
                                }
                            }
                            else
                                entity.ComplianceDocumentId = d.complianceDocumentid;

                            var predocdata = _empDocumentService.Query(t => t.EmpSystemID == entity.EmpSystemID && t.ComplianceDocumentId == entity.ComplianceDocumentId).Select().FirstOrDefault();
                            if (predocdata != null)
                            {
                                predocdata.FileId = entity.FileId;
                                predocdata.FileName = entity.FileName;
                                predocdata.UpdatedDate = entity.DateAdded;

                                _empDocumentService.Update(predocdata);
                            } 
                        }


                        entity.FileId = entity.SystemID;
                        if (string.IsNullOrEmpty(entity.FileName))
                        {
                            entity.FileId = null;
                        }
                        entity.DateAdded = DateTime.Now;

                        Insert(entity);

                        
                    }
                    else
                    {
                        var dbdata = Find(entity.SystemID);
                        if (dbdata == null || string.IsNullOrEmpty(dbdata.SystemID))
                            throw new CustomException("The record no longer exists.");
                        //var complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == entity.EductLevelSystemID).Select(r => r.Id).FirstOrDefault();
                        var empdoc = _empDocumentService.Query(t => t.EmpSystemID == entity.EmpSystemID).Select().FirstOrDefault();

                        if (empdoc!=null)
                        {
                            var d = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == entity.EductLevelSystemID).Select()
                                     join t in _empDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                     select new { complianceDocumentid = a.Id }).FirstOrDefault();
                            if (d == null)
                            {
                                //complianceDocumentid = _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == null).Select(r => r.Id).FirstOrDefault();
                                var c = (from a in _complianceDocumentService.Query(r => r.ProfileType == "Qualification" && r.QualificationLevelId == null).Select()
                                         join t in _empDocumentService.Query().Select() on a.Id equals t.ComplianceDocumentId
                                         select new { complianceDocumentid = a.Id }).FirstOrDefault();
                                if (c!=null)
                                {
                                    entity.ComplianceDocumentId = c.complianceDocumentid; 
                                }
                            }
                            else
                                entity.ComplianceDocumentId = d.complianceDocumentid;

                            var predocdata = _empDocumentService.Query(t => t.EmpSystemID == entity.EmpSystemID && t.ComplianceDocumentId == entity.ComplianceDocumentId).Select().FirstOrDefault();
                            if (predocdata != null)
                            {
                                predocdata.FileId = entity.FileId;
                                predocdata.FileName = entity.FileName;
                                predocdata.UpdatedDate = entity.DateUpdated;
                                _empDocumentService.Update(predocdata);
                            } 
                        }

                        entity.FileId = entity.SystemID;
                        if (string.IsNullOrEmpty(entity.FileName))
                        {
                            entity.FileId = null;
                        }
                        entity.DateUpdated = DateTime.Now;
                        Update(entity);

                        
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

        private DataSet EmployeeDocFile(string empSystemID)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM EmployeeDocument WHERE ComplianceDocumentId=(SELECT Id FROM HKP.ComplianceDocument WHERE ProfileType ='Qualification') AND EmpSystemId ='" + empSystemID + @"'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public Dictionary<string, object> GetQualificationFile(string systemId)
        {
            try
            {
                var sql = @"Select FileId, FileName From [dbo].[EmpAcademicQualificationInformation]  Where SystemID='" + systemId + "'";
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
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
using Library.ViewModel.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class EmpReferenceInformationService : Service<EmpReferenceInformation>, IEmpReferenceInformationService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EmpReferenceInformationService(
            IRepositoryAsync<EmpReferenceInformation> PreRecruitmentEmpReferenceRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) :
            base(PreRecruitmentEmpReferenceRepository, unitOfWork, pkGeneratorService)
        {
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return _signatrueService.GetAutoNumber("EMP_REFERANCE", DateTime.Now).ToString();
            //return base.GetAutoNumber("EmpReferenceInformation", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private IEnumerable<EmpReferenceInformation> Getlist(string empid)
        {
            try
            {
                string _sql = "SELECT * FROM EmpReferenceInformation WHERE EmpSystemID ='" + empid + "'";
                return _sqlRepository.GetModelCollection<EmpReferenceInformation>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<PreRecruitmentEmpReference> GetOldlist(string empIdOld)//TBT
        {
            try
            {
                string _sql = "SELECT * FROM PreRecruitmentEmpReference WHERE PreRecruitmentEmployeeId ='" + empIdOld + "'";
                return _sqlRepository.GetModelCollection<PreRecruitmentEmpReference>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitData(string empid, string empIdOld, out List<EmpReferenceInformation> from_db)
        {
            IEnumerable<PreRecruitmentEmpReference> from_ui = null;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_ui = GetOldlist(empIdOld);
                from_db = Getlist(empid).ToList<EmpReferenceInformation>();
                foreach (var db in from_db)
                {
                    var ui = from_ui.Where(a => a.SystemID == db.SystemID).FirstOrDefault();
                    if (ui == null || ui.SystemID == null)
                    {
                        db.ModelState = ModelState.Deleted;
                    }
                }

                foreach (var ui in from_ui)
                {
                    var db = from_db.Where(a => a.SystemID == ui.SystemID).FirstOrDefault();
                    if (db == null || db.SystemID == null)
                    {
                        db = new EmpReferenceInformation
                        {
                            ModelState = ModelState.Added
                        };
                        AuditService.Log(db);
                        db.SystemID = "ER" + DateTime.Now.ToString("yy") + "-" + GetPK();//set pk
                        from_db.Add(db);
                    }
                    else
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                    }

                    db.EmpSystemID = empid;
                    db.Ref1Name = ui.Ref1Name;
                    db.Ref1EmployerName = ui.Ref1EmployerName;
                    db.Ref1EmployerAddress = ui.Ref1EmployerAddress;
                    db.Ref1Designation = ui.Ref1Designation;
                    db.Ref1CellPhnNo = ui.Ref1CellPhnNo;
                    db.Ref1TelePhnNo = ui.Ref1TelePhnNo;
                    db.Ref1Email = ui.Ref1Email;
                    db.Ref1Address = ui.Ref1Address;

                    db.Ref2Name = ui.Ref2Name;
                    db.Ref2EmployerName = ui.Ref2EmployerName;
                    db.Ref2EmployerAddress = ui.Ref2EmployerAddress;
                    db.Ref2Designation = ui.Ref2Designation;
                    db.Ref2CellPhnNo = ui.Ref2CellPhnNo;
                    db.Ref2TelePhnNo = ui.Ref2TelePhnNo;
                    db.Ref2Email = ui.Ref2Email;
                    db.Ref2Address = ui.Ref2Address;

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
            List<EmpReferenceInformation> from_db = null;
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

        public IEnumerable<PreRecruitmentEmpReference> GetPreReferenceList(string PKs)//TBT
        {
            try
            {
                string _sql = "SELECT * FROM PreRecruitmentEmpReference WHERE PreRecruitmentEmployeeId IN (" + PKs + ")";
                return _sqlRepository.GetModelCollection<PreRecruitmentEmpReference>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<EmployeeProfileVM> xGetEmployeeInformation(string PK)//TBT
        {
            try
            {
                string _sql = @"SELECT
                                        e.SystemId,e.EmployeeCode,e.EmployeeName
                                        --,e.DOJ,e.DOB,e.DOC,e.DOS
                                        ,e.EmployeeStatus
                                        ,Replace(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                        ,Replace(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                        ,Replace(CONVERT(VARCHAR(11), e.DOC, 106), ' ', '-') DOC
                                        ,Replace(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-') DOS

                                        ,d.UserName Designation, gd.UserName GivenDesignation, dp.UserName Department
                                          , p.UserName Plant, c.UserName Company
                                        FROM EmployeeInformation e
                                        left outer join hkp.Designation d on e.DesignationSystemID = d.id
                                        left outer join hkp.Designation gd on e.GivenDesignationId = gd.id
                                        left outer join org.Department dp on dp.id = e.DepartmentId
                                        left outer join org.Plant p on p.Id = e.PlantId
                                        left outer join org.Company c on c.id = e.CompanyId
                                        where e.Systemid = '" + PK + "'";
                return _sqlRepository.GetModelCollection<EmployeeProfileVM>(_sql, null);
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
                var sql = @"Select REF.*,REI.EmployeeCode Ref1NameCode from EmpReferenceInformation REF
                        LEFT JOIN dbo.Employeeinformation REI ON REI.SystemId=REF.RefEmpSystemID
                        WHERE REF.EmpSystemID='" + empSystemID + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertOrUpdate(EmpReferenceInformation entity)
        {
            try
            {
                if (entity != null)
                {
                    if (string.IsNullOrEmpty(entity.SystemID))
                    {
                        entity.SystemID = GetAutoNumber(nameof(EmpReferenceInformation), PKGeneratorEnum.Auto, null, DateTime.Now);
                        entity.DateAdded = DateTime.Now;
                        Insert(entity);
                    }
                    else
                    {
                        var dbdata = Find(entity.SystemID);
                        if (dbdata == null || string.IsNullOrEmpty(dbdata.SystemID))
                            throw new CustomException("The record no longer exists.");
                        entity.DateUpdated = DateTime.Now;
                        Update(entity);
                    }
                }
                else
                    throw new CustomException("Incomplete data.");
            }
            catch (CustomException)
            {
                throw;
            }
        }
    }
}
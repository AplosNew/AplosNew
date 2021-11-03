#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.HumanResources;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.HumanResources
{
    public class SalaryFixationMailService : Service<SalaryFixationMail>, ISalaryFixationMailService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<SalaryFixationMail> _salaryFixationRepository;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;

        public SalaryFixationMailService(
            IRepositoryAsync<SalaryFixationMail> salaryFixationMailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            ) : base(salaryFixationMailRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
            _salaryFixationRepository = salaryFixationMailRepository;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
        }

        #endregion Constructor

        public string GetPK()
        {
            return GetAutoNumber(nameof(SalaryFixationMail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertOrUpdateSFMail(string PreReceuitmentEmployeeId, string PlantId)
        {
            var flag = false;
            try
            {
                SaveSFM(PreReceuitmentEmployeeId, PlantId);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void SaveSFM(string PreReceuitmentEmployeeId, string PlantId)
        {
            //SalaryFixationMail fromDB = null;
            try
            {
                var submit = _preRecruitmentEmployeeService.Query(t => t.Id == PreReceuitmentEmployeeId).Select(t => t.Submitted).FirstOrDefault();
                if (submit)
                {
                    //get from db fromDB
                    //if not in db
                    var fromDB = GetSalaryFixationMail(PreReceuitmentEmployeeId).FirstOrDefault();
                    if (fromDB == null || fromDB.Id == null)
                    {
                        fromDB = new SalaryFixationMail();
                        fromDB.Id = "FM" + GetPK();
                        fromDB.PreReceuitmentEmployeeId = PreReceuitmentEmployeeId;
                        fromDB.PlantId = PlantId;
                        fromDB.IsMailSent = false;
                        fromDB.ModelState = ModelState.Added;
                        AuditService.Log(fromDB);
                        base.InsertOrUpdateGraph(fromDB);
                    }
                    else
                    {
                        fromDB.PreReceuitmentEmployeeId = PreReceuitmentEmployeeId;
                        fromDB.PlantId = PlantId;
                        fromDB.IsMailSent = false;
                        fromDB.ModelState = ModelState.Modified;
                        AuditService.Log(fromDB);
                        base.InsertOrUpdateGraph(fromDB);
                    }
                }
                else
                {
                    throw new CustomException("This candidate doesn't submitted his profile.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public PreRecruitmentEmployee GetSubmittedCandidate(string preRecruitmentEmployeeId)
        {
            try
            {
                var sql = @"SELECT Submitted FROM PreRecruitmentEmployee WHERE Id='" + preRecruitmentEmployeeId + @"'";
                return _sqlRepository.GetModelCollection<PreRecruitmentEmployee>(sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<SalaryFixationMail> GetSalaryFixationMail(string PreReceuitmentEmployeeId)
        {
            try
            {
                string _sql = "SELECT * FROM SCS.SalaryFixationMail where PreReceuitmentEmployeeId='" + PreReceuitmentEmployeeId + "' and isnull(IsMailSent,0)=0";
                return _salaryFixationRepository.SqlQuery<SalaryFixationMail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
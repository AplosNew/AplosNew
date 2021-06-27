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
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class PreRecruitmentEmpReferenceService : Service<PreRecruitmentEmpReference>, IPreRecruitmentEmpReferenceService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<PreRecruitmentEmpReference> _preRecruitmentEmpReferenceRepository;

        public PreRecruitmentEmpReferenceService(
            IRepositoryAsync<PreRecruitmentEmpReference> preRecruitmentEmpReferenceRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(preRecruitmentEmpReferenceRepository, unitOfWork, pkGeneratorService)
        {
            _preRecruitmentEmpReferenceRepository = preRecruitmentEmpReferenceRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetData(string empSystemID)
        {
            try
            {
                var sql = @"Select * from PreRecruitmentEmpReference where PreRecruitmentEmployeeId='" + empSystemID + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region PreRecruitmentEmpReference

        public void InsertOrUpdate(PreRecruitmentEmpReference entity)
        {
            try
            {
                if (entity != null)
                {
                    if (string.IsNullOrEmpty(entity.SystemID))
                    {
                        entity.SystemID = GetAutoNumber(nameof(PreRecruitmentEmpReference), PKGeneratorEnum.Auto, null, DateTime.Now);
                        entity.AddedDate = DateTime.Now;
                        Insert(entity);
                    }
                    else
                    {
                        var dbdata = Find(entity.SystemID);
                        if (dbdata == null || string.IsNullOrEmpty(dbdata.SystemID))
                            throw new CustomException("The record no longer exists.");
                        entity.UpdatedDate = DateTime.Now;
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

        #endregion PreRecruitmentEmpReference
    }
}
#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class RetentionAllowanceService : Service<RetentionAllowanceMaster>, IRetentionAllowanceService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<RetentionAllowanceDetail> _retentionAllowanceDetailRepository;

        public RetentionAllowanceService(
            IRepositoryAsync<RetentionAllowanceMaster> retentionAllowanceMasterRepository,
            IRepositoryAsync<RetentionAllowanceDetail> retentionAllowanceDetailRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            ISqlRepository sqlRepository) : base(retentionAllowanceMasterRepository, unitOfWork, pkGeneratorService)
        {
            _retentionAllowanceDetailRepository = retentionAllowanceDetailRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK(string Id)
        {
            return GetAutoNumber(nameof(RetentionAllowanceMaster), PKGeneratorEnum.Yearly, Id, DateTime.Now);
        }

        public void InsertUpdate(RetentionAllowanceMaster model, IEnumerable<RetentionAllowanceDetail> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var ob = base.Query(r => r.PlantId == model.PlantId && r.EffectiveDate == model.EffectiveDate).Select().FirstOrDefault();
                if (ob != null)
                {
                    model.Id = ob.Id;
                }
                if (string.IsNullOrEmpty(model.Id))
                {
                    model.Id = GetAutoNumber(nameof(RetentionAllowanceMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
                    InsertGraph(model);
                }
                else
                {
                    UpdateGraph(model);
                }
                InsertOrUpdateChild(entities, model.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                model.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        //RetentionAllowanceMaster model, IEnumerable<RetentionAllowanceDetail> entities
        private void InsertOrUpdateChild(IEnumerable<RetentionAllowanceDetail> entities, string pk)
        {
            //var dbList = _retentionAllowanceDetailRepository.Query(t => t.RetentionAllowanceMasterId == pk).Select().AsEnumerable();
            if (entities != null)
            {
                var count = _retentionAllowanceDetailRepository.CreateChildPk(t => t.RetentionAllowanceMasterId == pk, x => x.Id, pk).ToInt();
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = pk + "-" + count;
                        item.RetentionAllowanceMasterId = pk;
                        AuditService.AddedLog(item);
                        _retentionAllowanceDetailRepository.Insert(item);
                        count++;
                    }
                    else
                    {
                        AuditService.UpdatedLog(item);
                        _retentionAllowanceDetailRepository.Update(item);
                    }
                }
            }
        }

        //
        public void UpdateGraph(RetentionAllowanceMaster model, IEnumerable<RetentionAllowanceDetail> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please insert Retention Allowance");
                _unitOfWork.BeginTransaction();
                flag = true;
                InsertOrUpdateChild(entities, model.Id);
                base.UpdateGraph(model);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                model.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(RetentionAllowanceMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "GL Mapping Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                RetentionAllowanceMaster entity = Find(id);
                // If section row inactive
                base.DeleteGraph(entity);
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
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name,
                    MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string plantId)
        {
            try
            {
                //           parameters.CmdText = @"Select A.*,m2.EffectiveDate,m2.PlantId,m2.IsAbsentismApplicable,LG.UserName LegalSalaryGradeName
                //from
                //           (SELECT PlantId, MAX(EffectiveDate) EffectiveDate FROM MST.RetentionAllowanceMaster
                //            WHERE EffectiveDate <= '" + date + @"' AND PlantId = '" + plantId + @"'
                //            GROUP BY PlantId
                //            ) m
                //            left join MST.RetentionAllowanceMaster m2 on m2.EffectiveDate = m.EffectiveDate and m2.PlantId = m.PlantId
                //            LEFT JOIN SCS.RetentionAllowanceDetail A ON m2.Id = A.RetentionAllowanceMasterId
                //            LEFT JOIN SCS.LegalSalaryGrade LG ON A.LegalSalaryGradeId = LG.Id";

                parameters.CmdText = @"SELECT  Id,EffectiveDate,IsAbsentismApplicable,PlantId,AddedBy,AddedDate,AddedFromIP,UpdatedBy,UpdatedDate,UpdatedFromIP FROM MST.RetentionAllowanceMaster";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel QueryWithMaster(GridParameter parameters, string masterId)
        {
            try
            {
                parameters.CmdText = @"SELECT A.Id,A.RetentionAllowanceMasterId,A.LegalSalaryGradeId,A.ExperienceSpan,A.Amount,LG.UserName LegalSalaryGradeName
                                 ,A.AddedBy,A.AddedDate,A.AddedFromIP,A.UpdatedBy,A.UpdatedDate,A.UpdatedFromIP FROM [SCS].[RetentionAllowanceDetail] A
                                LEFT JOIN SCS.LegalSalaryGrade LG ON A.LegalSalaryGradeId = LG.Id WHERE A.RetentionAllowanceMasterId='" + masterId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
    }
}
#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Machines;
using Library.Model.Processes;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion Using

namespace Library.Service.Machines
{
    public class PersonalAllowanceService : Service<PersonalAllowance>, IPersonalAllowanceService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PersonalAllowanceDetails> _personalAllowanceDetailsRepository;
        public PersonalAllowanceService(
            IRepositoryAsync<PersonalAllowance> PersonalAllowanceRepository,
            IRepositoryAsync<PersonalAllowanceDetails> personalAllowanceDetailsRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(PersonalAllowanceRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _personalAllowanceDetailsRepository = personalAllowanceDetailsRepository;
        }

        #endregion Constructor

        private void Check(PersonalAllowance entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Id != entity.Id && t.CompanyId == entity.CompanyId && t.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.Id != entity.Id && t.CompanyId == entity.CompanyId && t.UserName == entity.UserName);
        }

        public IEnumerable<object> GetCbo(string companyId)
        {
            try
            {
                return (from m in base.Query(t => t.CompanyId == companyId).Select().OrderBy(t => t.UserName)
                        select new { Text = m.UserName, Value = m.Id }).Distinct();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Menu.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM [HKP].[PersonalAllowance] WHERE CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public void InsertAndUpdate(IEnumerable<PersonalAllowance> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please insert personal allowance");
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(PersonalAllowance), PKGeneratorEnum.Yearly, null, DateTime.Now);
                var cpk = GetMaxNumber(nameof(PersonalAllowanceDetails), PKGeneratorEnum.Yearly, null, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        foreach (var citem in item.PersonalAllowanceDetails)
                        {
                            if (string.IsNullOrEmpty(citem.Id))
                            {
                                cpk.MaxNumber++;
                                citem.Id = cpk.MaxNumber.ToString();
                                citem.PersonalAllowanceId = item.Id;
                                citem.ModelState = ModelState.Added;
                                AuditService.Log(citem);
                            }
                            else
                            {
                                citem.ModelState = ModelState.Modified;
                                AuditService.Log(citem);
                            }
                            _personalAllowanceDetailsRepository.InsertOrUpdateGraph(citem);

                        }
                        InsertGraph(item);
                    }
                    else
                    {
                        UpdateGraph(item);
                    }
                }
                //string payrollGroupId = entities.First().PayrollGroupId;
                //string companyGroupId = entities.First().CompanyGroupId;
                //var dbList = base.Query(t => t.PayrollGroupId == payrollGroupId && t.CompanyGroupId == companyGroupId).Select().ToList();
                //if (dbList != null && dbList.Count() > 0)
                //{
                //    if (entities == null)
                //    {
                //        foreach (var item in dbList)
                //        {
                //            base.DeleteGraph(item);
                //        }
                //    }
                //    else
                //    {
                //        foreach (var item in dbList)
                //        {
                //            if (!entities.Any(t => t.Id == item.Id))
                //            {
                //                base.DeleteGraph(item);
                //            }
                //        }
                //    }
                //}
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public override void Update(PersonalAllowance entity)
        {
            try
            {
                Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        

       
    }
}
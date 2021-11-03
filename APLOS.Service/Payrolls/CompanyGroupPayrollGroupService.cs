using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Payrolls;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Payrolls
{
    public class CompanyGroupPayrollGroupService : Service<CompanyGroupPayrollGroup>, ICompanyGroupPayrollGroupService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public CompanyGroupPayrollGroupService(
            IRepositoryAsync<CompanyGroupPayrollGroup> companyGroupPayrollGroupRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companyGroupPayrollGroupRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PG.[Sequence]
		                                            ,PG.Code
		                                            ,PG.ShortName
		                                            ,PG.StandardName
		                                            ,PG.UserName
		                                            ,PG.[Description]
		                                            ,PG.Remarks
		                                            ,PG.Active
		                                            ,PG.Id
                                            FROM [" + DbSchema.HKP + @"].[CompanyGroupPayrollGroup] AS CGPG
                                            INNER JOIN [" + DbSchema.HKP + @"].[PayrollGroup] AS PG ON PG.Id=CGPG.PayrollGroupId
                                            WHERE CGPG.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<object> GetCbo(string companyGroupId)
        {
            try
            {
                return from m in base.Query(r => r.CompanyGroupId == companyGroupId && r.Active).Include(r => r.PayrollGroup).Select().OrderBy(r => r.PayrollGroup.Sequence)
                       select new { Text = m.PayrollGroup.UserName, Value = m.PayrollGroupId };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(CompanyGroupPayrollGroup), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void InsertGraph(CompanyGroupPayrollGroup entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.InsertGraph(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
        }

        public void UpdateGraph(string payrollGroupId, bool active)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.PayrollGroupId == payrollGroupId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                data_Db.Active = active;
                data_Db.ModelState = ModelState.Modified;
                AuditService.Log(data_Db);
                base.UpdateGraph(data_Db);
            }
        }

        public void DeleteGraph(string payrollGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.PayrollGroupId == payrollGroupId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                base.DeleteGraph(data_Db);
            }
        }
    }
}
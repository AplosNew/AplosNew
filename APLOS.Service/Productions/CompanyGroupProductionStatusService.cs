#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Productions;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Productions
{
    public class CompanyGroupProductionStatusService : Service<CompanyGroupProductionStatus>, ICompanyGroupProductionStatusService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public CompanyGroupProductionStatusService(
            IRepositoryAsync<CompanyGroupProductionStatus> companyGroupProductionStatusRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companyGroupProductionStatusRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT SC.[PlanningGroupPriority]
		                                            ,SC.Code
		                                            ,SC.ShortName
		                                            ,SC.StandardName
		                                            ,SC.UserName
		                                            ,SC.[Description]
		                                            ,SC.Remarks
		                                            ,SC.Active
		                                            ,SC.Id
                                                    ,SC.MasterPlanApplicable
                                            FROM [" + DbSchema.HKP + @"].[" + DbTable.CompanyGroupProductionStatus + @"] AS CGSC
                                            INNER JOIN [" + DbSchema.HKP + @"].[" + DbTable.ProductionStatus + @"] AS SC ON SC.Id=CGSC.ProductionStatusId
                                            WHERE CGSC.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<object> GetStatusCbo(string companyGroupId)
        {
            try
            {
                var _sql = @"select p.Id [Value],p.UserName [Text] from [HKP].[ProductionStatus] p
                                left join (select * from [HKP].[CompanyGroupProductionStatus] where CompanyGroupId='" + companyGroupId + @"' and Active=1) g on g.ProductionStatusId=p.Id
                                where p.Active=1 order By p.UserName";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return from m in base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.Active).Include(r => r.ProductionStatus).Select().OrderBy(r => r.ProductionStatus.UserName)
                       select new { Text = m.ProductionStatus.UserName, Value = m.ProductionStatusId };
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
            return GetAutoNumber(nameof(CompanyGroupProductionStatus), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void InsertGraph(CompanyGroupProductionStatus entity)
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

        public void UpdateGraph(string productionStatusId, bool active)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.ProductionStatusId == productionStatusId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                data_Db.Active = active;
                data_Db.ModelState = ModelState.Modified;
                AuditService.Log(data_Db);
                base.UpdateGraph(data_Db);
            }
        }

        public void DeleteGraph(string productionStatusId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.ProductionStatusId == productionStatusId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                base.DeleteGraph(data_Db);
            }
        }
    }
}
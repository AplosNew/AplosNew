#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
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

namespace Library.Service.OrderManagements
{
    public class CompanyGroupPortService : Service<CompanyGroupPort>, ICompanyGroupPortService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public CompanyGroupPortService(
            IRepositoryAsync<CompanyGroupPort> companyGroupPortRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companyGroupPortRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT SC.[Sequence]
                                                ,SC.Code
                                                ,SC.ShortName
                                                ,SC.StandardName
                                                ,SC.UserName
                                                ,SC.[Description]
                                                ,SC.Remarks
                                                ,SC.Active
                                                ,SC.CountryId
                                                ,C.UserName AS CountryName
                                                ,SC.ShipModeId
                                                ,SM.UserName AS ShipModeName
                                                ,SC.Id
                                        FROM MST.CompanyGroupPort AS CGSC
                                        INNER JOIN MST.Port AS SC ON SC.Id=CGSC.PortId
                                        LEFT OUTER JOIN MST.ShipMode AS SM ON SC.ShipModeId=Sm.Id
                                        LEFT OUTER JOIN SCS.Country AS C ON SC.CountryId=C.Id
                                        WHERE CGSC.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return from m in base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.Active).Include(r => r.Port).Select().OrderBy(r => r.Port.UserName)
                       select new { Text = m.Port.UserName, Value = m.PortId };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(CompanyGroupPort), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void InsertGraph(CompanyGroupPort entity)
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void UpdateGraph(string portId, bool active)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CompanyGroupPort data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.PortId == portId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                data_Db.Active = active;
                data_Db.ModelState = ModelState.Modified;
                AuditService.Log(data_Db);
                base.UpdateGraph(data_Db);
            }
        }

        public void DeleteGraph(string portId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                CompanyGroupPort data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.PortId == portId).Select().FirstOrDefault();
                if (data_Db != null)
                {
                    base.DeleteGraph(data_Db);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
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
    public class CompanyGroupDestinationService : Service<CompanyGroupDestination>, ICompanyGroupDestinationService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;//

        public CompanyGroupDestinationService(
            IRepositoryAsync<CompanyGroupDestination> companyGroupDestinationRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(companyGroupDestinationRepository, unitOfWork, pkGeneratorService)
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
                                                ,SC.Id
                                        FROM MST.CompanyGroupDestination AS CGSC
                                        INNER JOIN MST.Destination AS SC ON SC.Id=CGSC.DestinationId
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
                return from m in base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.Active).Include(r => r.Destination).Select().OrderBy(r => r.Destination.UserName)
                       select new { Text = m.Destination.UserName, Value = m.DestinationId };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GeDestinationCbo(string portId, string groupId)
        {
            try
            {
                string _sql = @"SELECT d.Id [Value], d.UserName [Text] FROM [MST].[Destination] d
                                LEFT JOIN (select * from [MST].[CompanyGroupDestination] WHERE CompanyGroupId='" + groupId + @"') g on g.DestinationId=d.Id
                                WHERE d.Active=1 AND d.CountryId=(SELECT CountryId FROM [MST].[Port] WHERE Id='" + portId + "')";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GeDestinationCbobyCountry(string CountryId, string groupId)
        {
            try
            {
                string _sql = @"SELECT d.Id [Value], d.UserName [Text] FROM [MST].[Destination] d
                                LEFT JOIN (select * from [MST].[CompanyGroupDestination] WHERE CompanyGroupId='" + groupId + @"') g on g.DestinationId=d.Id
                                WHERE d.Active=1 AND d.CountryId='"+ CountryId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(CompanyGroupDestination), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void InsertGraph(CompanyGroupDestination entity)
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

        public void UpdateGraph(string destinationtId, bool active)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CompanyGroupDestination data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.DestinationId == destinationtId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                data_Db.Active = active;
                data_Db.ModelState = ModelState.Modified;
                AuditService.Log(data_Db);
                base.UpdateGraph(data_Db);
            }
        }

        public void DeleteGraph(string destinationtId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                CompanyGroupDestination data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.DestinationId == destinationtId).Select().FirstOrDefault();
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
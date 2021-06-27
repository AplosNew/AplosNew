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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class StoppageService : Service<Stoppage>, IStoppageService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public StoppageService(
            IRepositoryAsync<Stoppage> StoppageRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(StoppageRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(Stoppage), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(Stoppage entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }
        
        public GridModel GetCbo(string routeId)
        {
            try
            {
                var sql = @"select S.Id as [Value],S.UserName as [Text] from HKP.Stoppage S
							left outer join MST.RouteStoppage RS on S.Id=RS.StoppageId
							where RS.RouteId='" + routeId + "'";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IEnumerable<object> GetCityByCompanyCbo(string companyId)
        {
            try
            {
                var sql = @"Select  C.Id AS [Value], C.UserName AS [Text] From SCS.City AS C
                            Left Outer Join MST.AddressMaster AS AM ON C.CountryId=AM.CountryId
                            Left Outer Join ORG.Company AS CO ON AM.Id=CO.AddressMasterId
                            Where CO.Id='" + companyId + "' order by C.UserName ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                 Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null,
                    ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }
    }
}
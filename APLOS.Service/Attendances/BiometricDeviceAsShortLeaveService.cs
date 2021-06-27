#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Attendances
{
    public class BiometricDeviceAsShortLeaveService : Service<BiometricDeviceAsShortLeave>, IBiometricDeviceAsShortLeaveService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public IRepositoryAsync<BiometricDeviceAsShortLeave> BiometricDeviceAsShortLeaveRepository { get; }

        public BiometricDeviceAsShortLeaveService(
            IRepositoryAsync<BiometricDeviceAsShortLeave> BiometricDeviceAsShortLeaveRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(BiometricDeviceAsShortLeaveRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            this.BiometricDeviceAsShortLeaveRepository = BiometricDeviceAsShortLeaveRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(BiometricDeviceAsShortLeave), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(BiometricDeviceAsShortLeave entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.GroupID = identity.CompanyGroupId;
                entity.SystemID = "SL" + GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(BiometricDeviceAsShortLeave entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT B.*,P.UserName AS Plant FROM dbo.BiometricDeviceAsShortLV AS B
										Left Outer Join ORG.Plant AS P ON B.PlantId = P.Id
										Where B.GroupID ='" + identity.CompanyGroupId + "' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //public IEnumerable<BiometricDeviceAsShortLeave> LoadAttdnRawData(string plantid, string ip)//TBT
        //{
        //    try
        //    {
        //        string _sql = @"SELECT * FROM [dbo].[BiometricDeviceAsShortLV]
        //                        WHERE PlantID = '" + plantid + @"' AND MachineIP = '" + ip + @"'";

        //        //string _sql = "SELECT * FROM AttdnRawData WHERE PlantId ='"+ sPlantid + "'";
        //        return _sqlRepository.GetModelCollection<BiometricDeviceAsShortLeave>(_sql, null);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
    }
}
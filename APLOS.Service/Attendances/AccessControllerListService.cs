#region Using

using Library.Core;
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
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Attendances
{
    public class AccessControllerListService : Service<AccessControllerList>, IAccessControllerListService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public IRepositoryAsync<AccessControllerList> AccessControllerListRepository { get; }

        public AccessControllerListService(
            IRepositoryAsync<AccessControllerList> AccessControllerListRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(AccessControllerListRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            this.AccessControllerListRepository = AccessControllerListRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(AccessControllerList), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Insert(AccessControllerList entity, string companyGroupId)
        {
            try
            {
                entity.CompanyGroupId = companyGroupId;
                entity.Id = "AC" + GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(AccessControllerList entity)
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

        public GridModel Query(GridParameter parameters, string companyGroupId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT B.*,P.UserName AS Plant,Z.UserName AS Zone FROM MST.AccessControllerList AS B
										Left Outer Join ORG.Plant AS P ON B.PlantId = P.Id
										LEFT OUTER JOIN hkp.AttendanceDeviceZone AS Z ON z.Id=B.AttendanceDeviceZoneid
										Where B.PlantId='" + plantId + "' AND B.CompanyGroupId ='" + companyGroupId + "' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<AccessControllerList> LoadAttdnRawData(string plantid, string ip)//TBT
        {
            var _sql = @"SELECT * FROM [MST].[AccessControllerList]
                            WHERE PlantID = '" + plantid + @"' AND MachineIP = '" + ip + @"'";

            return _sqlRepository.GetModelCollection<AccessControllerList>(_sql, null);
        }

        public IEnumerable<ComboModel> GetCbo(string plantId)
        {
            string _sql = @"SELECT Id,(MachineIP+'-'+Description) UserName FROM [MST].[AccessControllerList] where PlantId='" + plantId + "'";
            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }
    }
}
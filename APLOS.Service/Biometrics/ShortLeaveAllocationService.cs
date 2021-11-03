#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Biometrics
{
    public class ShortLeaveAllocationService : Service<ShortLeaveAllocation>, IShortLeaveAllocationService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlantService _plantService;
        private readonly IRepositoryAsync<LeaveTransaction> _leaveTransactionRepository;

        public ShortLeaveAllocationService(
            IRepositoryAsync<ShortLeaveAllocation> shortLeaveAllocationRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IPlantService plantService
            , IRepositoryAsync<LeaveTransaction> leaveTransactionRepository) :
            base(shortLeaveAllocationRepository, unitOfWork, pkGeneratorService)
        {
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _plantService = plantService;
            _leaveTransactionRepository = leaveTransactionRepository;
        }

        #endregion Constructor

        #region Operation

        private string GetPK()
        {
            return GetAutoNumber(nameof(ShortLeaveAllocation), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void SaveData(ShortLeaveAllocation entity)
        {
            try
            {
                var cgId = _plantService.Query(t => t.Id == entity.PlantID).Select(t => t.CompanyGroupId).FirstOrDefault();
                entity.SystemID = GetPK();
                entity.GroupID = cgId;
                Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Operation
    }
}
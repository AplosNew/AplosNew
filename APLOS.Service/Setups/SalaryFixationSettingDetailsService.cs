#region Using

using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Core;
using Library.Service.Systems;

#endregion Using

namespace Library.Service.Setups
{
    public class SalaryFixationSettingDetailsService : Service<SalaryFixationSettingDetails>, ISalaryFixationSettingDetailsService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public SalaryFixationSettingDetailsService(
            IRepositoryAsync<SalaryFixationSettingDetails> salaryFixationSettingDetailsRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) : base(salaryFixationSettingDetailsRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
    }
}
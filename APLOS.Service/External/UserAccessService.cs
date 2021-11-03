#region Using

using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.External;
using Library.Service.Core;

#endregion Using

namespace Library.Service.External
{
    public class UserAccessService : Service<EmployeeLink>, IUserAccessService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeService _employeeService;

        public UserAccessService(
              IRepositoryAsync<EmployeeLink> baseRepository
            , IEmployeeService employeeService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(baseRepository, unitOfWork)
        {
            _employeeService = employeeService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
    }
}
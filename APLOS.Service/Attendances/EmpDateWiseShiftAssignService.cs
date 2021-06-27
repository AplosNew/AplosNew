#region Using

using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Systems;

#endregion Using

namespace Library.Service.Attendances
{
    public class EmpDateWiseShiftAssignService : Service<EmpDateWiseShiftAssign>, IEmpDateWiseShiftAssignService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IAccessControllerDeleteRequestService _d;

        public EmpDateWiseShiftAssignService(
            IRepositoryAsync<EmpDateWiseShiftAssign> PreRecruitmentEmpReferenceRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IAccessControllerDeleteRequestService d
            , IEmployeeInformationService employeeInformationService) :
            base(PreRecruitmentEmpReferenceRepository, unitOfWork, pkGeneratorService)
        {
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _d = d;
            _employeeInformationService = employeeInformationService;
        }

        #endregion Constructor
    }
}
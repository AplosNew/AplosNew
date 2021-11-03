
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Service.Core;
using Library.Service.Systems;

namespace Library.Service.Attendances
{
    public class AttdnDataMonthlySummaryService : Service<AttdnDataMonthlySummary>, IAttdnDataMonthlySummaryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;

        public AttdnDataMonthlySummaryService(
            IRepositoryAsync<AttdnDataMonthlySummary> attdnDataDownLoadLogRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(attdnDataDownLoadLogRepository, unitOfWork, pkGeneratorService)
        {
            _pk = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }
    }
}
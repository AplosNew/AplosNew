using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Systems;
using System;

namespace Library.Service.Employees
{
    public class PreRecruitmentProofTypeService : Service<PreRecruitmentProofType>, IPreRecruitmentProofTypeService
    {
        #region Constuctor

        private readonly IRepositoryAsync<PreRecruitmentProofType> _preRecruitmentProofTypeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PreRecruitmentProofTypeService(
            IRepositoryAsync<PreRecruitmentProofType> preRecruitmentProofTypeRepository
            , IPKGeneratorService pKGeneratorService
            , IUnitOfWork unitOfWork
            ) : base(preRecruitmentProofTypeRepository, unitOfWork, pKGeneratorService)
        {
            _preRecruitmentProofTypeRepository = preRecruitmentProofTypeRepository;
            _unitOfWork = unitOfWork;
        }

        #endregion Constuctor

        private string GetPK()
        {
            return GetAutoNumber(nameof(PreRecruitmentProofType), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
    }
}
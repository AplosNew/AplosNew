#region Using

using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Biometrics
{
    public class EmployeeFPBackUpService : Service<EmployeeFPBackUp>, IEmployeeFPBackUpService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeFPBackUpService(
            IRepositoryAsync<EmployeeFPBackUp> PreRecruitmentEmpReferenceRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(PreRecruitmentEmpReferenceRepository, unitOfWork, pkGeneratorService)
        {
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void InitData(EmployeeFPInformationService ui, ref List<EmployeeFPBackUp> from_db)
        {
            //IEnumerable<AccessControllerEmployeeTag> from_ui = null;
            //from_db = null;
            try
            {
                //string _pks = GetPks(from_ui);
                //from_db = new List<AccessControllerDeleteRequest>();

                var db = new EmployeeFPBackUp
                {
                    //db.DeletedBy=
                    //db.DateAdded = DateTime.Now;
                    UpdatedDate = DateTime.Now
                };
                //db.AddedBy = identity.UserId;
                db.UpdatedBy = "schedule";
                db.ModelState = ModelState.Added;
                AuditService.Log(db);
                from_db.Add(db);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
#region Using

using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Attendances
{
    public class AccessControllerDeleteRequestService : Service<AccessControllerDeleteRequest>, IAccessControllerDeleteRequestService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AccessControllerDeleteRequestService(
            IRepositoryAsync<AccessControllerDeleteRequest> PreRecruitmentEmpReferenceRepository
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

        private string GetPK()
        {
            return GetAutoNumber(nameof(AccessControllerDeleteRequest), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InitData(AccessControllerEmployeeTag ui, ref List<AccessControllerDeleteRequest> from_db)
        {
            //IEnumerable<AccessControllerEmployeeTag> from_ui = null;
            //from_db = null;
            try
            {
                //from_db = new List<AccessControllerDeleteRequest>();

                var db = new AccessControllerDeleteRequest();

                db.Id = "DR" + GetPK();
                db.EmpInfoSystemID = ui.EmpInfoSystemID;
                db.DeviceSystemID = ui.DeviceSystemID;
                db.GroupID = ui.GroupID;
                db.PlantID = ui.PlantID;
                //db.UpdatedDate = DateTime.Now;
                //db.UpdatedBy = "schedule";
                db.ModelState = ModelState.Added;
                AuditService.Log(db);
                from_db.Add(db);

                foreach (var item in from_db)
                {
                    InsertOrUpdateGraph(item);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
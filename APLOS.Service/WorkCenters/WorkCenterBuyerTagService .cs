#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.WorkCenters;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.WorkCenters
{
    public partial class WorkCenterBuyerTagService : Service<WorkCenterBuyerTag>, IWorkCenterBuyerTagService
    {
        private string tWorkCenterBuyerTag = " " + DbSchema.HKP + ".[WorkCenterBuyerTag] ";

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<WorkCenterBuyerTag> _workCenterBuyerTagRepository;

        public WorkCenterBuyerTagService(
            IRepositoryAsync<WorkCenterBuyerTag> workCenterBuyerTagRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(workCenterBuyerTagRepository, unitOfWork, pkGeneratorService)
        {
            _workCenterBuyerTagRepository = workCenterBuyerTagRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public override void Insert(WorkCenterBuyerTag entity)
        {
            try
            {
                Check(entity);
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private void Check(WorkCenterBuyerTag entity)
        {
            var data = base.Query(t => t.Id != entity.Id && t.WorkCenterMasterId == entity.WorkCenterMasterId
                                  && t.BuyerId == entity.BuyerId && t.DMMId == entity.DMMId && t.MaterialMasterId == entity.MaterialMasterId
                                  && t.Active).Select().FirstOrDefault();
            if (data != null)
                throw new CustomException("this combination already exist..........!");
        }

        private string GetPK()
        {
            return GetAutoNumber("WorkCenterBuyerTag", PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void Update(WorkCenterBuyerTag entity)
        {
            try
            {
                Check(entity);
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public WorkCenterBuyerTag GetMaster(string PK)
        {
            try
            {
                var _sql = "select * from " + tWorkCenterBuyerTag + " where Id='" + PK + "'";
                return _workCenterBuyerTagRepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Delete(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "WorkCenterBuyerTag Id"));

                var entity = GetMaster(id);
                entity.ModelState = ModelState.Deleted;
                // If section row inactive
                base.Delete(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string plantId, string unitId)
        {
            try
            {
                parameters.CmdText = @"
                                     SELECT WCBT.*,
                                            WCM.UserName AS WorkCenterMaster,
                                            B.UserName   AS Buyer,
                                            D.UserName   AS DMM,
                                            MM.UserName  AS MaterialMaster
                                            FROM    " + DbSchema.HKP + @".[WorkCenterBuyerTag] WCBT
                                            LEFT JOIN  " + DbSchema.SystemConfigurationAndSetup + @".[WorkCenterMaster] WCM ON WCM.Id = WCBT.WorkCenterMasterId
                                            LEFT JOIN  " + DbSchema.HKP + @".[Buyer] B ON B.Id = WCBT.BuyerId
                                            LEFT JOIN " + DbSchema.HKP + ".[" + DbTable.DMM + @"] D ON D.Id = WCBT.DMMId
                                            LEFT JOIN " + DbSchema.Masters + ".[" + DbTable.MaterialMaster + @"] MM ON MM.Id = WCBT.MaterialMasterId
                                            Where WCBT.PlantId = '" + plantId + "' AND WCBT.UnitId = '" + unitId + "' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
    }
}
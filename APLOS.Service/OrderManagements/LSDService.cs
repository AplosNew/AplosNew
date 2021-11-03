#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class LSDService : Service<LSD>, ILSDService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public LSDService(
            IRepositoryAsync<LSD> LSDRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(LSDRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string buyerId)
        {
            try
            {
                parameters.CmdText = @"SELECT L.Id
                                              ,L.BuyerId
                                              ,B.UserName AS BuyerName
                                              ,L.ShipModeId
                                              ,SM.UserName AS ShipModeName
                                              ,L.OrderLeadTime
                                              ,L.ProductionLeadTime
                                              ,L.FinishingLeadTime
                                              ,L.ExFactoryLeadTime
                                              ,L.MainRawMaterialInhouseLeadTime
                                              ,L.OtherRawMaterialInhouseLeadTime
                                              ,L.Weekend
                                        FROM MST.LSD AS L
                                        LEFT OUTER JOIN HKP.Buyer AS B ON L.BuyerId=B.Id
                                        LEFT OUTER JOIN MST.ShipMode AS SM ON L.ShipModeId=SM.Id
                                        WHERE L.BuyerId='" + buyerId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> LsdList(string buyerId)
        {
            try
            {
                string _sql = @"SELECT L.Id
                                              ,L.BuyerId
                                              ,B.UserName AS BuyerName
                                              ,L.ShipModeId
                                              ,SM.UserName AS ShipModeName
                                              ,L.OrderLeadTime
                                              ,L.ProductionLeadTime
                                              ,L.FinishingLeadTime
                                              ,L.ExFactoryLeadTime
                                              ,L.MainRawMaterialInhouseLeadTime
                                              ,L.OtherRawMaterialInhouseLeadTime
                                              ,L.Weekend
                                        FROM MST.LSD AS L
                                        LEFT OUTER JOIN HKP.Buyer AS B ON L.BuyerId=B.Id
                                        LEFT OUTER JOIN MST.ShipMode AS SM ON L.ShipModeId=SM.Id
                                        WHERE L.BuyerId='" + buyerId + "'";
                //return _sqlRepository.GetGridData(parameters);
                // return _sqlRepository.GetDataCollection(_sql, null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(LSD), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(LSD entity)
        {
            var db_Data = base.Query(t => t.Id != entity.Id && t.BuyerId == entity.BuyerId && t.ShipModeId == entity.ShipModeId).Select().FirstOrDefault();
            if (db_Data != null)
            {
                throw new CustomException("This shipment mode already exist for this buyer.........!");
            }
        }

        public override void Insert(LSD entity)
        {
            try
            {
                Check(entity);
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public override void Update(LSD entity)
        {
            try
            {
                Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void DeleteGraph(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "LSD Id"));
                LSD entity = Find(id);
                Delete(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }
    }
}
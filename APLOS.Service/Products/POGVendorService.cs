#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Model.Products;
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

namespace Library.Service.Products
{
    public class POGVendorService : Service<POGVendor>, IPOGVendorService
    {
        #region Constructor
        private readonly IRepositoryAsync<PurchaseOrderGroupDetails> _receiveDetailRepository;
        private readonly IRepositoryAsync<POGVendor> _POGVendorRepository;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public POGVendorService(
            IRepositoryAsync<PurchaseOrderGroupDetails> receiveDetailRepository
            , IRepositoryAsync<POGVendor> POGVendorRepository
            , IRepositoryAsync<POGVendor> materialRequsitionMaster
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<PurchaseOrderGroupDetails> materialRequsitionDetailsRepository
            ) : base(materialRequsitionMaster, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _POGVendorRepository = POGVendorRepository;
            _unitOfWork = unitOfWork;
            _receiveDetailRepository = receiveDetailRepository;
   
        }
        public string CompanyGroupId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Id => throw new NotImplementedException();

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber( nameof(POGVendor), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public void InsertOrUpdateGraphPOGVendor(POGVendor entity, string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                // Insert in receive detail
                if (string.IsNullOrEmpty(entity.Id))
                {
                    var NewId = entity.PurchaseOrderGroupId + "-";
                    var receiveDetail = new POGVendor
                    {
                        Id = GetPK(),
                    PurchaseOrderGroupId = entity.PurchaseOrderGroupId,
                        PartyId = entity.PartyId,
                        PartyPreference=entity.PartyPreference
                    };
                    AuditService.AddedLog(receiveDetail);
                    InsertGraph(receiveDetail);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }

        }


        public void POGVendorDelete(string id)
        {
            try
            {
                //var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.MaterialRequsitionDetails WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                ////var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //if (!detail)
                //{

                var data = _POGVendorRepository.Find(id);
                if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                _POGVendorRepository.Delete(data.Id);
                _unitOfWork.SaveChanges();
                //}
                //else throw new CustomException("Please delete first line item.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

    }
}
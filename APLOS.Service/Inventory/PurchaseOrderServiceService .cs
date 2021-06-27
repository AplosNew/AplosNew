using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.Inventory
{
	public class PurchaseOrderServiceService : Service<POService>, IPurchaseOrderServiceService
    {
		#region Constructor

		private readonly IRepositoryAsync<PurchaseOrderTax> _receiveTaxRepository;
		private readonly IRepositoryAsync<POService> _inventoryServiceRepository;
		private readonly IRepositoryAsync<PurchaseOrderDetail> _invRecDetailRepository;
		private readonly IPurchaseOrderService _inventoryReceiveService;
		private readonly ISqlRepository _sqlRepository;
		private readonly IUnitOfWork _unitOfWork;

		public PurchaseOrderServiceService(
			IRepositoryAsync<POService> inventoryServiceRepository
			, IRepositoryAsync<PurchaseOrderTax> receiveTaxRepository
			, IRepositoryAsync<PurchaseOrderDetail> invRecDetailRepository
			, IPurchaseOrderService inventoryReceiveService
			, IPKGeneratorService pkGeneratorService
			, IUnitOfWork unitOfWork
			, ISqlRepository sqlRepository
			) : base(inventoryServiceRepository, unitOfWork, pkGeneratorService)
		{
			_inventoryServiceRepository = inventoryServiceRepository;
			_invRecDetailRepository = invRecDetailRepository;
			_receiveTaxRepository = receiveTaxRepository;
			_inventoryReceiveService = inventoryReceiveService;
			_unitOfWork = unitOfWork;
			_sqlRepository = sqlRepository;
		}

        #endregion Constructor
        public IEnumerable<object> GetTerms(string Id)
        {
            try
            {
                var sql = @"select DeliveryInstruction,SpecialInstruction,CheckedBy from TRN.purchaseOrder where Id='" + Id + @"'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #region InventoryService

        public void InsertGraph(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
		{
			if (Convert.ToBoolean(_inventoryServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.POService WHERE InventoryReceiveId='" + entity.InventoryReceiveId + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
				throw new CustomException("This service already taken."); ;

			var flag = false;
			try
			{
				_unitOfWork.BeginTransaction();
				flag = true;
              
                    if (entity.IsNotNull())
                    {
                    entity.ToCurrencyRate = entity.ToCurrencyRate == 0 ? 1 : entity.ToCurrencyRate;
                        var currentId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[POService] WHERE InventoryReceiveId='{entity.InventoryReceiveId}'").First();
                        currentId++;
                        var service = new POService
                        {
                            Id = MakePK(entity.InventoryReceiveId + 2, currentId, 2),
                            InventoryReceiveId = entity.InventoryReceiveId,
                            ServiceMasterId = entity.ServiceMasterId,
                            //Amount = Convert.ToDecimal(entity.TransactionAmount*entity.ToCurrencyRate),
                            Amount = Convert.ToDecimal(entity.TransactionAmount),
                            TotalTaxAmount = Convert.ToDecimal(entity.TotalTaxAmount),
                            GRNServiceAmount = 0,
                            AmountStatus = false,
                            Description=entity.Description
                        };
                        AuditService.AddedLog(service);
                        InsertGraph(service);
                        if (taxCategoryList.IsNotNull())
                        {
                            var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryServiceId='{service.Id}'").First();
                            foreach (var item in taxCategoryList)
                            {
                                crrId++;
                                item.Id = MakePK(service.Id, crrId, 2);
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryReceiveDetailId = null;
                                item.InventoryServiceId = service.Id;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                            }
                        }
                        //var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == service.InventoryReceiveId).Select(t => t.IsNonCreditable).FirstOrDefault();
                        //var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                        //if (entity.CurrencyId != entity.BaseCurrencyId)
                        //    UpdateInventoryDetail(service, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                        //else if (entity.CurrencyId == entity.BaseCurrencyId)
                        //    UpdateInventoryDetail(service, ratio, 1, entity.IsNonCreditable);
                    }
              
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
        public void InsertGraphFG(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            //if (Convert.ToBoolean(_inventoryServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.POService WHERE InventoryReceiveId='" + entity.InventoryReceiveId + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
            //    throw new CustomException("This service already taken."); ;

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var itemDetail in entity)
                {
                    if (entity.IsNotNull())
                    {
                        itemDetail.ToCurrencyRate = itemDetail.ToCurrencyRate == 0 ? 1 : itemDetail.ToCurrencyRate;
                        var currentId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[POService] WHERE InventoryReceiveId='{itemDetail.InventoryReceiveId}'").First();
                        currentId++;
                        var service = new POService
                        {
                            Id = MakePK(itemDetail.InventoryReceiveId + 2, currentId, 2),
                            InventoryReceiveId = itemDetail.InventoryReceiveId,
                            ServiceMasterId = itemDetail.ServiceMasterId,
                            //Amount = Convert.ToDecimal(entity.TransactionAmount*entity.ToCurrencyRate),
                            Amount = Convert.ToDecimal(itemDetail.TransactionAmount),
                            TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                            GRNServiceAmount = 0,
                            AmountStatus = false,
                        };
                        AuditService.AddedLog(service);
                        InsertGraph(service);
                        if (taxCategoryList.IsNotNull())
                        {
                            var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryServiceId='{service.Id}'").First();
                            foreach (var item in taxCategoryList)
                            {
                                crrId++;
                                item.Id = MakePK(service.Id, crrId, 2);
                                item.InventoryReceiveId = itemDetail.InventoryReceiveId;
                                item.InventoryReceiveDetailId = null;
                                item.InventoryServiceId = service.Id;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                            }
                        }
                        var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == service.InventoryReceiveId).Select(t => t.IsNonCreditable).FirstOrDefault();
                        var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                        if (itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                            UpdateInventoryDetail(service, ratio, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);
                        else if (itemDetail.CurrencyId == itemDetail.BaseCurrencyId)
                            UpdateInventoryDetail(service, ratio, 1, itemDetail.IsNonCreditable);
                    }
                }
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
        private void UpdateInventoryDetail(POService service, decimal ratio, decimal currencyRate, bool isNonCreditable)
		{
			var detailList = _invRecDetailRepository.Query(t => t.InventoryReceiveId == service.InventoryReceiveId).Select().ToList();
			if (detailList.IsNotNull())
			{
				foreach (var item in detailList)
				{
					var chamnt = item.ChargesAmount;
					item.ChargesAmount = item.TransactionAmount * ratio;
					item.WithInvoiceRate = isNonCreditable ? (item.TransactionAmount + item.TotalTaxAmount + item.ChargesAmount) / item.TransactionQty
												 : (item.TransactionAmount + item.ChargesAmount) / item.TransactionQty;
					item.AfterInvoiceRate = item.WithInvoiceRate;
					item.BaseAmount = (item.BaseAmount - (chamnt * currencyRate)) + item.ChargesAmount * currencyRate;
					item.ModelState = ModelState.Modified;
					AuditService.UpdatedLog(item);
					_invRecDetailRepository.Update(item);
				}
			}
		}

		public void Delete(string serviceId)
		{
			var flag = false;
			try
			{
				var isNonCreditable = _inventoryServiceRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[PurchaseOrder] AS A JOIN [TRN].[POService] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + serviceId + "'").First();
				var service = Find(serviceId);
				if (!service.IsNotNull()) throw new CustomException("Data not found");
				_unitOfWork.BeginTransaction();
				flag = true;

				var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryServiceId == serviceId).Select().ToList();
				if (taxCategoryList.IsNotNull())
				{
					foreach (var item in taxCategoryList)
					{
						item.ModelState = ModelState.Deleted;
						_receiveTaxRepository.Delete(item);
					}
				}
				var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, 0, isNonCreditable);
				UpdateInventoryDetail(service, ratio, 1, isNonCreditable);
				base.DeleteGraph(service);
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

		#endregion InventoryService

		public IEnumerable<object> Query(string receiveId)
		{
			try
			{
                //var sql = @"SELECT A.Id, A.InventoryReceiveId, A.ServiceMasterId, B.UserName AS ServiceMasterName
                //                        , A.Amount, A.TotalTaxAmount,null ChargeTaxList
                //                        FROM [TRN].[POService] AS A JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                //                        WHERE A.InventoryReceiveId='" + receiveId + "'";
                var sql = @"SELECT A.Id, A.InventoryReceiveId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.Amount
                            --, A.TotalTaxAmount
                            ,POT.TaxAmount As TotalTaxAmount
                            --,TaxAmount
                            ,null ChargeTaxList
                            ,A.Description 
                            FROM 
                            [TRN].[POService] AS A 
                            INner JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                            left JOIN (select InventoryServiceId,Sum(TaxAmount) as TaxAmount  from TRN.PurchaseOrderTax group by InventoryServiceId) AS POT on A.id=POT.InventoryServiceId
                            WHERE A.InventoryReceiveId='" + receiveId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}






           

            

    


        }

        #region po BY Requisition
        public void InsertGraphPOByReq(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            if (Convert.ToBoolean(_inventoryServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.POService WHERE InventoryReceiveId='" + entity.InventoryReceiveId + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                throw new CustomException("This service already taken."); ;

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                if (entity.IsNotNull())
                {
                    entity.ToCurrencyRate = entity.ToCurrencyRate == 0 ? 1 : entity.ToCurrencyRate;
                    var currentId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[POService] WHERE InventoryReceiveId='{entity.InventoryReceiveId}'").First();
                    currentId++;
                    var service = new POService
                    {
                        Id = MakePK(entity.InventoryReceiveId + 2, currentId, 2),
                        InventoryReceiveId = entity.InventoryReceiveId,
                        ServiceMasterId = entity.ServiceMasterId,
                        //Amount = Convert.ToDecimal(entity.TransactionAmount*entity.ToCurrencyRate),
                        Amount = Convert.ToDecimal(entity.TransactionAmount),
                        TotalTaxAmount = Convert.ToDecimal(entity.TotalTaxAmount),
                        GRNServiceAmount = 0,
                        AmountStatus = false,
                        Description = entity.Description
                    };
                    AuditService.AddedLog(service);
                    InsertGraph(service);
                    if (taxCategoryList.IsNotNull())
                    {
                        var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryServiceId='{service.Id}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            crrId++;
                            item.Id = MakePK(service.Id, crrId, 2);
                            item.InventoryReceiveId = entity.InventoryReceiveId;
                            item.InventoryReceiveDetailId = null;
                            item.InventoryServiceId = service.Id;
                            AuditService.AddedLog(item);
                            _receiveTaxRepository.Insert(item);
                        }
                    }
                    var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == service.InventoryReceiveId).Select(t => t.IsNonCreditable).FirstOrDefault();
                    var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                    if (entity.CurrencyId != entity.BaseCurrencyId)
                        UpdateInventoryDetail(service, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                    else if (entity.CurrencyId == entity.BaseCurrencyId)
                        UpdateInventoryDetail(service, ratio, 1, entity.IsNonCreditable);
                }

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
        public IEnumerable<object> GetServicePOTerms(string Id)
        {
            try
            {
                var sql = @"select DeliveryInstruction,SpecialInstruction,CheckedBy from TRN.ServicePOMaster where Id='" + Id + @"'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetServiceChargePOServiceList(string Id)
        {
            try
            {
                var sql = @"select A.Id,B.UserName ServiceName,Amount,Tax.TotalTaxAmount ,A.Description,B.Id ServiceMasterId, ISNULL(A.Qty,0) Qty, ISNULL(A.Rate,0) Rate,UOM.ShortName UoM
                            from [TRN].[ServicePODetail] A
                            LEFt JOIN hkp.ServiceMaster B ON A.ServiceMasterId=B.Id
                            LEFT JOIN (select ServicePODetailId, sum(taxamount) TotalTaxAmount 
			                from trn.ServicePOTax group by ServicePODetailId
                            ) Tax ON Tax.ServicePODetailId=A.Id
                            Left JOin [SCS].[UnitOfMeasurement] UOM ON UOM.Id=A.TransactionUoMId
                            where A.ServicePOMasterId='" + Id + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> LoadServicePoDetails(string Id) 
        {
            try
            {
                var sql = @"select A.Id,A.ServicePOMasterId,B.UserName ServiceName,Amount,Tax.TotalTaxAmount,ISNULL(A.Qty,0) Qty,ISNULL(A.Rate,0) Rate,UOM.ShortName UoM
                            from [TRN].[ServicePODetail] A
                            LEFt JOIN hkp.ServiceMaster B ON A.ServiceMasterId=B.Id
                            LEFT JOIN (select ServicePODetailId, sum(taxamount) TotalTaxAmount 
			                from trn.ServicePOTax group by ServicePODetailId
                            ) Tax ON Tax.ServicePODetailId=A.Id
                            	Left JOin [SCS].[UnitOfMeasurement] UOM ON UOM.Id=A.TransactionUoMId
                            ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> LoadTaxById(string Id)
        {
            try
            {
                var sql = @"SELECT [Id]
                              ,[ServicePOMasterId]
                              ,[ServicePODetailId]
                              ,[TaxCategoryId] 
                              ,[HSNCodeId]
                              ,[Percentage]
                              ,[TaxAmount]
                              ,[AddedBy]
                              ,[AddedDate]
                              ,[AddedFromIP]
                              ,[UpdatedBy]
                              ,[UpdatedDate]
                              ,[UpdatedFromIP]
                          FROM [TRN].[ServicePOTax]
                          Where ServicePODetailId='"+ Id + "'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion
    }
}
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Materials;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.MaterialManagement.Inventory
{
    public class MaterialRequsitionDetailsServiceService : Service<MaterialRequsitionDetails>, IMaterialRequsitionDetailsServiceService 
    {
        #region Constructor
        private readonly IPKGeneratorService _pkGeneratorService;

        private readonly IRepositoryAsync<MaterialRequsitionDetails> _receiveDetailRepository;
        private readonly IRepositoryAsync<InventoryReceive> _inventoryReceiveRepository; 
        private readonly IRepositoryAsync<PurchaseOrderTax> _receiveTaxRepository;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IPOMaterialService _inventoryMaterialMasterService;
        private readonly IPurchaseOrderService _inventoryReceiveService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPurchaseOrderService _inventoryReveiveService;
        public MaterialRequsitionDetailsServiceService(
             IRepositoryAsync<MaterialRequsitionDetails> receiveDetailRepository
            ,IRepositoryAsync<InventoryReceive> inventoryReceiveRepository
            , IRepositoryAsync<PurchaseOrderTax> receiveTaxRepository
            , IMaterialMasterService materialMasterService
            , IPOMaterialService inventoryMaterialMasterService
            , IPurchaseOrderService inventoryReceiveService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ,IPurchaseOrderService inventoryReveiveService

            ) : base(receiveDetailRepository, unitOfWork, pkGeneratorService)
        {
            _receiveDetailRepository = receiveDetailRepository;
            _receiveTaxRepository = receiveTaxRepository;
            _materialMasterService = materialMasterService;
            _inventoryMaterialMasterService = inventoryMaterialMasterService;
            _inventoryReceiveService = inventoryReceiveService;
            _inventoryReceiveRepository = inventoryReceiveRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
            _inventoryReveiveService = inventoryReveiveService;
        }

        #endregion Constructor

        public void InsertOrUpdateGraph(MaterialRequisitionDetailViewModel entity)
        {


            var flag = false;
            try
            {
               
                _unitOfWork.BeginTransaction();
                flag = true;
                
                // Insert in receive detail
                if (string.IsNullOrEmpty(entity.Id))
                {
                    var NewId = entity.MaterialReqqusitionMasterId + "-";
                    //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();
                   var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id))    AS INT)), 0) Id FROM[TRN].[MaterialRequsitionDetails] WHERE MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();

                    
                    currentId++;
                    var receiveDetail = new MaterialRequsitionDetails
                    {
                        
                        Id = NewId + currentId, //MakePK(NewId + currentId, 0,0),
                        CompanyGroupId = entity.CompanyGroupId,
                        MaterialReqqusitionMasterId = entity.MaterialReqqusitionMasterId,
                        ActivityId = entity.ActivityId,
                        MaterialMasterId = entity.MaterialMasterId,
                        ArticleId = entity.ArticleId,
                        FirstCharacteristicsId = entity.FirstCharacteristicsId,
                        FirstCharacteristicsValueId = entity.FirstCharacteristicsValueId,
                        SecondCharacteristicsId = entity.SecondCharacteristicsId,
                        SecondCharacteristicsValueId = entity.SecondCharacteristicsValueId,
                        ThirdCharacteristicsId = entity.ThirdCharacteristicsId,
                        ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId,
                        MaterialDetail = entity.MaterialDetail,
                        TransactionUoMId = entity.TransactionUoMId,
                        CurrencyId = entity.CurrencyId,
                        TransactionQty = Convert.ToDecimal(entity.TransactionQty),
                        EstimatedRate = Convert.ToDecimal(entity.EstimatedRate),
                        TotalAmount = Convert.ToDecimal(entity.TotalAmount),
                        BudgetType = entity.BudgetType,
                        Reason = entity.Reason,
                        Remarks = entity.Remarks,
                        QualityApprovalResponsiblePersonId = entity.QualityApprovalResponsiblePersonId,
                        NeedSpecialAppId = entity.NeedSpecialAppId,
                        FutureReqApp = entity.FutureReqApp,
                        DeliveryDate = Convert.ToDateTime(entity.DeliveryDate),
                        LocalImported = entity.LocalImported,
                        CommitmentDate = entity.CommitmentDate,
						OrginalQty= Convert.ToDecimal(entity.TransactionQty),
						BudgetMasterId=entity.BudgetMasterId,
						GLGeneralInfoId=entity.GLGeneralInfoId
					};
                    InsertGraph(receiveDetail);
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
		public void InsertOrUpdateGraphApprovedQty(IEnumerable<MaterialRequisitionDetailViewModel>  entity) 
		{


			var flag = false;
			try
			{

				_unitOfWork.BeginTransaction();
				flag = true;

				
				foreach (var item in entity)
				{
					if(item.ApprovedQty>0)
					{
						string query = "update TRN.MaterialRequsitionDetails set TransactionQty='" + item.ApprovedQty + "',TotalAmount='" + item.TotalAmount + "' where Id='" + item.Id + "'";
						_receiveDetailRepository.ExecuteSqlCommand(query);
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
		public void InsertOrUpdateGraphEdit(MaterialRequisitionDetailViewModel entity)
        {


            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                // Insert in receive detail
                if (!string.IsNullOrEmpty(entity.Id))
                {
                    //var NewId = entity.MaterialReqqusitionMasterId + "-";
                    //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();
                   // currentId++;
                    var receiveDetail = new MaterialRequsitionDetails
                    {

                        Id = entity.Id,
                        CompanyGroupId = entity.CompanyGroupId,
                        MaterialReqqusitionMasterId = entity.MaterialReqqusitionMasterId,
                        ActivityId = entity.ActivityId,
                        MaterialMasterId = entity.MaterialMasterId,
                        ArticleId = entity.ArticleId,
                        FirstCharacteristicsId = entity.FirstCharacteristicsId,
                        FirstCharacteristicsValueId = entity.FirstCharacteristicsValueId,
                        SecondCharacteristicsId = entity.SecondCharacteristicsId,
                        SecondCharacteristicsValueId = entity.SecondCharacteristicsValueId,
                        ThirdCharacteristicsId = entity.ThirdCharacteristicsId,
                        ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId,
                        MaterialDetail = entity.MaterialDetail,
                        TransactionUoMId = entity.TransactionUoMId,
                        CurrencyId = entity.CurrencyId,
                        TransactionQty = Convert.ToDecimal(entity.TransactionQty),
                        EstimatedRate = Convert.ToDecimal(entity.EstimatedRate),
                        TotalAmount = Convert.ToDecimal(entity.TotalAmount),
                        BudgetType = entity.BudgetType,
                        Reason = entity.Reason,
                        Remarks = entity.Remarks,
                        QualityApprovalResponsiblePersonId = entity.QualityApprovalResponsiblePersonId,
                        NeedSpecialAppId = entity.NeedSpecialAppId,
                        FutureReqApp = entity.FutureReqApp,
                        DeliveryDate = Convert.ToDateTime(entity.DeliveryDate),
                        LocalImported = entity.LocalImported,
                        CommitmentDate = entity.CommitmentDate
                    };
                    UpdateGraph(receiveDetail);
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




        
        public void InsertOrUpdateGraphFGForMasterOrder(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> Materialentity, IEnumerable<PurchaseOrderTax> taxCategoryList, IEnumerable<InventoryMaterialViewModel> ServiceEntity, IEnumerable<PurchaseOrderTax> ServicetaxCategoryList)
        {


            var flag = false;
            try
            {
                //if (CheckItemExist(entity))
                //    throw new CustomException(entity.MaterialMasterName + " already received");

               
                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.POType = "FGPO";
                entity.MasterOrderId = Materialentity.Select(r=>r.MasterOrderId).FirstOrDefault();

                _inventoryReveiveService.Insert(entity);
                //foreach (var itemDetail in Materialentity)
                //{
                //    ResetCurrencyRate(itemDetail);
                //    if (entity.IsNotNull())
                //    {
                //        itemDetail.CompanyGroupId = identity.CompanyGroupId;
                //        itemDetail.CompanyId = identity.CompanyId;
                //        itemDetail.PlantId = identity.PlantId;
                //        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                //        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                //        ///TODO : Get total qyt and amount by country and issue qty
                //        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != entity.Id).Select(t => t.BaseQty).Sum();
                //        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != entity.Id).Select(t => t.BaseAmount).Sum();

                //        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                //        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                //        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                //        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId
                //             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                //        {
                //            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                //            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                //            itemDetail.BaseAmount = itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;

                //            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                //            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                //        }
                //        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId)
                //        {
                //            itemDetail.BaseQty = itemDetail.TransactionQty;
                //            itemDetail.BaseUoMFactor = itemDetail.TransactionQty;
                //            //entity.BaseAmount = entity.TransactionAmount * entity.ToCurrencyRate;
                //            itemDetail.BaseAmount = itemDetail.TransactionAmount;

                //            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                //            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                //        }
                //        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == entity.BaseCurrencyId
                //            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                //        {
                //            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                //            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                //            itemDetail.BaseAmount = itemDetail.TransactionAmount;

                //            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                //            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                //        }
                //        else
                //        {
                //            itemDetail.BaseUoMFactor = itemDetail.TransactionQty;
                //            itemDetail.BaseQty = itemDetail.TransactionQty;
                //            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                //        }
                //        // Insert in receive detail
                //        if (string.IsNullOrEmpty(entity.Id))
                //        {
                //            var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId='{itemDetail.InventoryReceiveId}'").First();
                //            currentId++;
                //            var receiveDetail = new PurchaseOrderDetail
                //            {
                //                Id = MakePK(itemDetail.InventoryReceiveId + 1, currentId, 2),
                //                MaterialStorageId = itemDetail.MaterialStorageId,
                //                InventoryReceiveId = itemDetail.InventoryReceiveId,
                //                //InventoryMaterialId = entity.InventoryMaterialId,
                //                TransactionQty = itemDetail.TransactionQty,
                //                TransactionUoMId = itemDetail.TransactionUoMId,
                //                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                //                BaseUOMId = itemDetail.BaseUOMId,
                //                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                //                TransactionRate = Convert.ToDecimal(itemDetail.TransactionRate),
                //                TransactionAmount = Convert.ToDecimal(itemDetail.TransactionAmount),
                //                BaseAmount = Convert.ToDecimal(itemDetail.BaseAmount),
                //                TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                //                IssueQty = null,
                //                GRNRcvQty = 0,
                //                QtyStatus = false,
                //                CountryId = itemDetail.CountryId,
                //                MasterOrderId = itemDetail.MasterOrderId,
                //                MasterOrderDetailId = itemDetail.MasterOrderDetailId
                //            };
                //            itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                //            AuditService.AddedLog(receiveDetail);
                //            var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.TransactionAmount, null, 0, entity.IsNonCreditable);

                //            receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;
                //            receiveDetail.WithInvoiceRate = entity.IsNonCreditable ? (receiveDetail.TransactionAmount + receiveDetail.TotalTaxAmount + receiveDetail.ChargesAmount) / receiveDetail.TransactionQty
                //                                     : (receiveDetail.TransactionAmount + receiveDetail.ChargesAmount) / receiveDetail.TransactionQty;
                //            receiveDetail.AfterInvoiceRate = receiveDetail.WithInvoiceRate;

                //            //receiveDetail.BaseAmount += entity.IsNonCreditable ? Convert.ToDecimal(entity.TotalTaxAmount + receiveDetail.ChargesAmount) * Convert.ToDecimal(entity.ToCurrencyRate) :
                //            //     Convert.ToDecimal(receiveDetail.ChargesAmount) * Convert.ToDecimal(entity.ToCurrencyRate);
                //            receiveDetail.BaseAmount = entity.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + receiveDetail.BaseAmount) * Convert.ToDecimal(entity.ToCurrencyRate) :
                //                 Convert.ToDecimal(receiveDetail.BaseAmount);//* Convert.ToDecimal(entity.ToCurrencyRate);

                //            itemDetail.TotalQty = Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty);
                //            itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.BaseAmount) / itemDetail.TotalQty);

                //            _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                //            receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                //            InsertGraph(receiveDetail);
                //            UpdateInventoryDetail(receiveDetail, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                //        }
                //        // insert in receive tax
                //        if (taxCategoryList.IsNotNull())
                //        {
                //            var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                //            foreach (var item in taxCategoryList)
                //            {
                //                currentId++;
                //                item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                //                item.InventoryReceiveId = itemDetail.InventoryReceiveId;
                //                item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                //                item.InventoryServiceId = null;
                //                AuditService.AddedLog(item);
                //                _receiveTaxRepository.Insert(item);
                //            }
                //        }
                //    }
                //}
          
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


        
        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(PurchaseOrderTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public void InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            var flag = false;
            try
            {
                if (CheckItemExist(entity))
                    throw new CustomException(entity.MaterialMasterName + " already received");

                ResetCurrencyRate(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                var dbList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == entity.InventoryReceiveDetailId).Select().AsEnumerable();
                if (entity.IsNotNull())
                {
                    // insert in PO Item tax
                    if (taxCategoryList.IsNotNull())
                    {
                        var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            if (item.Id == null)
                            {
                                currentId++;
                                item.Id = MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                                //_receiveTaxRepository.InsertOrUpdateGraph(item);
                            }
                            else
                            {
                                item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Update(item);
                            }
                        }
                        if (dbList != null)
                        {
                            var deleteList = dbList.Where(t => t.InventoryReceiveDetailId == entity.InventoryReceiveDetailId).ToList();
                            foreach (var item in deleteList)
                            {
                                if (!taxCategoryList.Any(t => t.Id == item.Id))
                                    _receiveTaxRepository.Delete(item);
                            }
                        }
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
        public void InsertserviceTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList, string ServiceId)
        {
            var flag = false;
            try
            {
                if (CheckItemExist(entity))
                    throw new CustomException(entity.MaterialMasterName + " already received");
                var dbList = _receiveTaxRepository.Query(t => t.InventoryServiceId == ServiceId && t.InventoryReceiveId == entity.InventoryReceiveId).Select().AsEnumerable();
                ResetCurrencyRate(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entity.IsNotNull())
                {

                    // insert in receive tax
                    if (taxCategoryList.IsNotNull())
                    {
                        var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            if (item.Id == null)
                            {
                                currentId++;
                                item.Id = GetPK();//MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryReceiveDetailId = null; //entity.InventoryReceiveDetailId;
                                item.InventoryServiceId = ServiceId;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                                //_receiveTaxRepository.InsertOrUpdateGraph(item);
                            }
                            else
                            {
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryServiceId = ServiceId;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Update(item);
                            }
                        }
                        if (dbList != null)
                        {
                            var deleteList = dbList.Where(t => t.InventoryServiceId == ServiceId && t.InventoryReceiveId == entity.InventoryReceiveId).ToList();
                            foreach (var item in deleteList)
                            {
                                if (!taxCategoryList.Any(t => t.Id == item.Id))
                                    _receiveTaxRepository.Delete(item);

                            }
                        }
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


        private static void ResetCurrencyRate(InventoryMaterialViewModel entity)
        {
            if (string.IsNullOrEmpty(entity.ToCurrencyRate.ToString()))
            {
                if (entity.BaseCurrencyId != entity.CurrencyId)
                    throw new CustomException("Please input currency rate.");
                else
                    entity.ToCurrencyRate = 1;
            }
            else if (entity.ToCurrencyRate == 0)
            {
                if (entity.BaseCurrencyId != entity.CurrencyId)
                    throw new CustomException("Please input currency rate.");
                else
                    entity.ToCurrencyRate = 1;
            }
            else
            {
                if (entity.BaseCurrencyId == entity.CurrencyId)
                    entity.ToCurrencyRate = 1;
            }
        }

        private void UpdateInventoryDetail(PurchaseOrderDetail detail, decimal ratio, decimal currencyRate, bool isNonCreditable)
        {
            //var detailList = base.Query(t => t.InventoryReceiveId == detail.InventoryReceiveId && t.Id != detail.Id).Select().ToList();
            //if (detailList.IsNotNull())
            //{
            //    foreach (var item in detailList)
            //    {
            //        //var chamnt = item.ChargesAmount;
            //        var chamnt = item.BaseAmount;
            //        item.ChargesAmount = item.TransactionAmount * ratio;
            //        //item.WithInvoiceRate = isNonCreditable ? (item.TransactionAmount + item.TotalTaxAmount + item.ChargesAmount) / item.TransactionQty
            //        //                             : (item.TransactionAmount + item.ChargesAmount) / item.TransactionQty;
            //        item.WithInvoiceRate = isNonCreditable ? (item.TransactionAmount + item.TotalTaxAmount + item.BaseAmount) / item.TransactionQty
            //                                     : (item.TransactionAmount + item.BaseAmount) / item.TransactionQty;
            //        item.AfterInvoiceRate = item.WithInvoiceRate;
            //        item.BaseAmount = (item.BaseAmount - (chamnt * currencyRate)) + item.BaseAmount * currencyRate;

            //        item.ModelState = ModelState.Modified;
            //        AuditService.UpdatedLog(item);
            //        UpdateGraph(item);
            //    }
            //}
        }

        public void Delete(string receiveDetailId)
        {
            var flag = false;
            try
            {
                //var isNonCreditable = _receiveDetailRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[PurchaseOrder] AS A JOIN [TRN].[PurchaseOrderDetail] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + receiveDetailId + "'").First();
                //var data = Find(receiveDetailId);
                //if (data.IsNotNull())
                //{
                //    _unitOfWork.BeginTransaction();
                //    flag = true;
                //    _inventoryMaterialMasterService.UpdateFromReceive(data.InventoryMaterialId, receiveDetailId);
                //    var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                //    if (taxCategoryList.Count > 0)
                //    {
                //        foreach (var item in taxCategoryList)
                //        {
                //            item.ModelState = ModelState.Deleted;
                //            _receiveTaxRepository.Delete(item);
                //        }
                //    }
                //    var ratio = _inventoryReceiveService.GetChargesRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                //    UpdateInventoryDetail(data, ratio, 1, isNonCreditable);
                //    var res = _inventoryReceiveRepository.SqlQuery<int>(@"Select POId=Case when IR.POId IS NULL then 0 else 1 end from [TRN].PurchaseOrder PO Left JOIN [TRN].[InventoryReceive]  IR On IR.POId=PO.Id where PO.Id= '" + data.InventoryReceiveId + "'").FirstOrDefault();
                //    if(res==1)
                //    {
                //        throw new CustomException("Already Received In PO");

                //    }
                //    base.DeleteGraph(data);
                //    _unitOfWork.SaveChanges();
                //    flag = false;
                //    _unitOfWork.Commit();
                //}
                //else
                //    throw new CustomException("Data not found");
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

        private bool CheckItemExist(InventoryMaterialViewModel entity)
        {
            try
            {
                var sql = @"IF EXISTS(SELECT 1 FROM(
                                SELECT IRD.InventoryMaterialId FROM [TRN].[InventoryReceiveDetail] AS IRD
                                JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                                WHERE IRD.InventoryReceiveId='" + entity.InventoryReceiveId + @"'
                                AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                                AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + @"'
                                AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + @"'
                                AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                                AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'
                            ) AS TBL ) SELECT 1 ELSE SELECT 0 RETURN";
                var d = Convert.ToBoolean(_receiveDetailRepository.SqlQuery<int>(sql).First());
                return d;
            }
            catch
            {
                throw;
            }
        }
        public void UpdateMaterial(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            try
            {
                //int currentId = 0;
                //foreach (var item1 in inventoryMaterialList)
                //{
                //    if (string.IsNullOrEmpty(item1.Id))
                //    {
                //        currentId++;
                //        item1.Id = MakePK(item1.Id, currentId, 2);                       
                //        AuditService.AddedLog(item1);
                //        _receiveTaxRepository.Insert(item1);
                //    }
                //    else
                //    {
                //        AuditService.UpdatedLog(item1);
                //        _receiveTaxRepository.Update(item1);
                //    }
                //}

                if (entity.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item1 in entity)
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        var ip = identity.IPAddress;
                        var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                        var UpdatedBy = identity.Name;
                        var TaxAutoId = item1.InventoryReceiveDetailId;
                        if(item1.BaseTaxAmount == null)
                        {
                            item1.BaseTaxAmount = "0.00";
                        }
                        string _sql = "Update TRN.purchaseOrderDetail set TransactionQty='" + item1.TransactionQty + "', BaseQty='" + item1.TransactionQty + "',BaseUOMFactor='" + item1.TransactionQty + "', TransactionRate='" + item1.TransactionRate + "', TransactionAmount='" + item1.TrnAmount + "',TotalTaxAmount='" + item1.BaseTaxAmount + "' ,BaseAmount='" + item1.TrnAmount + "',Description ='" + item1.Description + "',UpdatedBy='" + UpdatedBy + "',UpdatedDate='" + updatedDate + "',UpdatedFromIP='" + ip + "'  where id='" + TaxAutoId + "'";
                        _sqlRepository.ExecuteSqlCommand(_sql);
                    }
                }
                if (receiveTaxList.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item in receiveTaxList)
                    {
                        // currentId++;
                        //item.Id = MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                        //item.InventoryReceiveId = entity.InventoryReceiveId;
                        //item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                        //item.InventoryServiceId = null;

                        //AuditService.AddedLog(item);
                        //_receiveTaxRepository.Insert(item);

                        var TaxAutoId = item.Id;
                        string _sql1 = "Update TRN.PurchaseOrderTax set TaxAmount='" + item.TaxAmount + "' where id='" + TaxAutoId + "' ";
                        _sqlRepository.ExecuteSqlCommand(_sql1);
                        //AuditService.UpdatedLog(item);
                        //_receiveTaxRepository.Update(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void UpdateServiceAndTax(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            try
            {
                if (entity.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item1 in entity)
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        var ip = identity.IPAddress;
                        var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                        var UpdatedBy = identity.Name;
                        var TaxAutoId = item1.Id;
                        if(item1.TotalTaxAmount==null)
                        {
                            item1.TotalTaxAmount = 0;
                        }
                        // string _sql = "Update TRN.purchaseOrderDetail set TransactionQty='" + item1.TransactionQty + "', BaseQty='" + item1.TransactionQty + "',BaseUOMFactor='" + item1.TransactionQty + "', TransactionRate='" + item1.TransactionRate + "', TransactionAmount='" + item1.TrnAmount + "',TotalTaxAmount='" + item1.BaseTaxAmount + "' ,BaseAmount='" + item1.BaseAmount + "',UpdatedBy='" + UpdatedBy + "',UpdatedDate='" + updatedDate + "',UpdatedFromIP='" + ip + "'  where id='" + TaxAutoId + "'";
                        string _sql = "Update TRN.POService set Amount='" + item1.Amount + "', TotalTaxAmount='" + item1.TotalTaxAmount + "', Description='" + item1.Description + "',UpdatedBy='" + UpdatedBy + "',UpdatedDate='" + updatedDate + "',UpdatedFromIP='" + ip + "'  where id='" + TaxAutoId + "'";
                        _sqlRepository.ExecuteSqlCommand(_sql);
                    }
                }

                //AuditService.AddedLog(voucher);
                // 
                // _sqlRepository.ExecuteSqlCommand(_sql);
                if (receiveTaxList.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item in receiveTaxList)
                    {
                        // currentId++;
                        //item.Id = MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                        //item.InventoryReceiveId = entity.InventoryReceiveId;
                        //item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                        //item.InventoryServiceId = null;

                        //AuditService.AddedLog(item);
                        //_receiveTaxRepository.Insert(item);
                        var TaxAutoId = item.Id;
                        string _sql1 = "Update TRN.PurchaseOrderTax set TaxAmount='" + item.TaxAmount + "' where id='" + TaxAutoId + "' ";
                        _sqlRepository.ExecuteSqlCommand(_sql1);
                    }
                }
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
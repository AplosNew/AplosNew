using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Products;
using Library.Model.Taxations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Library.Service.Inventory
{
    public class IssueRequestMasterService : Service<IssueRequestMaster>, IIssueRequestMasterService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<IssueRequestMaster> _inventoryReceiveRepository;
        private readonly IRepositoryAsync<InventoryReceiveDetail> _inventoryReceiveDetailRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;

        public IssueRequestMasterService(
            IRepositoryAsync<IssueRequestMaster> inventoryReceiveRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<InventoryReceiveDetail> inventoryReceiveDetailRepository
            ) : base(inventoryReceiveRepository, unitOfWork, pkGeneratorService)
        {
            _inventoryReceiveRepository = inventoryReceiveRepository;
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _inventoryReceiveDetailRepository = inventoryReceiveDetailRepository;
        }

        #endregion Constructor

        #region InventoryReceive

        //private string GetPK()
        //{
        //    return GetAutoNumber(nameof(InventoryReceive), PKGeneratorEnum.Yearly, null, DateTime.Now);
        //}

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(InventoryReceive), out sID);
            return sID;
        }

        public override void Insert(IssueRequestMaster entity)
        {
            try
            {
                //entity.FixedAssetOrInventory = "Inventory";
                //var leastDate = base.Query(t => t.Id != entity.Id && t.PlantId == entity.PlantId).Select(t => t.GRNDate).OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.Date).FirstOrDefault();
                //if (Convert.ToDateTime(entity.GRNDate) < leastDate) throw new CustomException("GRN date can't less then " + leastDate.ToString("dd/MMM/yyyy"));
                //ResetCurrencyRate(entity);
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void Insert(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMaterial)
        {
            var flag = false;
            try
            {
                //_companyParallelCurrencyService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                //_companyFiscalYearService.CheckingFiscalYearPeriod(voucherVM);
                //_companyTaxYearService.CheckingTaxYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;
                entity.ModelState = ModelState.Added;
                entity.Id = _pkGeneratorService.GetAutoNumber(nameof(entity), PKGeneratorEnum.Yearly, null, DateTime.Now);
                AuditService.AddedLog(entity);
               // _inventoryReceiveRepository.Insert(entity);

                var currentInventoryReceiveId = 0;
                //var currentSalesServiceId = 0;
                //var currentSalesTaxId = 0;
                //if (entityMaterial != null)
                //{
                foreach (var entityMaterialVM in entityMaterial)
                {
                    currentInventoryReceiveId++;
                    var inventoryReceiveDetail = new InventoryReceiveDetail
                    {
                        Id = _pkGeneratorService.MakePK(entity.Id, currentInventoryReceiveId, 2),
                        InventoryReceiveId = entity.Id,
                        InventoryMaterialId = entityMaterialVM.MaterialMasterId,
                        MaterialStorageId = entity.MaterialStorageId,
                        TransactionQty = entityMaterialVM.TransactionQty,
                        TransactionUoMId = entityMaterialVM.TransactionUoMId,
                        BaseQty = Convert.ToDecimal(entityMaterialVM.BaseQty),
                        BaseUOMId = entityMaterialVM.BaseUOMId,
                        BaseUoMFactor = Convert.ToDecimal(entityMaterialVM.BaseUoMFactor),
                        MaterialTranRate = Convert.ToDecimal(entityMaterialVM.MaterialTranRate),
                        MaterialTranAmount = Convert.ToDecimal(entityMaterialVM.MaterialTranAmount),
                        IssueQty = Convert.ToDecimal(entityMaterialVM.IssueQty),
                        AddedBy = entity.AddedBy,
                        AddedDate = entity.AddedDate,
                        AddedFromIP = entity.AddedFromIP,
                        UpdatedBy = entity.UpdatedBy,
                        UpdatedDate = entity.UpdatedDate,
                        UpdatedFromIP = entity.UpdatedFromIP,
                        TotalTaxAmount = Convert.ToDecimal(entityMaterialVM.TotalTaxAmount),
                        TotalMaterialTranAmount = Convert.ToDecimal(entityMaterialVM.TotalMaterialTranAmount),
                        ChargesTranAmount = Convert.ToDecimal(entityMaterialVM.ChargesTranAmount), //entityMaterialVM.ChargesTranAmount,
                        TrnCurrencyBaseRate = Convert.ToDecimal(entityMaterialVM.WithInvoiceRate),
                        BooksCurrencyBaseRate = Convert.ToDecimal(entityMaterialVM.AfterInvoiceRate),
                        CountryId = entityMaterialVM.CountryId,

                    };
                    _inventoryReceiveDetailRepository.Insert(inventoryReceiveDetail);

                    //if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                    //{
                    //    foreach (var taxVM in salesMaterialVM.TaxList)
                    //    {
                    //        if (taxVM.TaxCategoryId == null)
                    //            throw new CustomException("Please Select Tax Category !");

                    //        currentSalesTaxId++;
                    //        var salesTax = new SalesTax
                    //        {
                    //            Id = _pkGeneratorService.MakePK(salesMaterial.Id, currentSalesTaxId, 2),
                    //            AddedBy = salesMaterial.AddedBy,
                    //            AddedDate = salesMaterial.AddedDate,
                    //            AddedFromIP = salesMaterial.AddedFromIP,
                    //            Amount = taxVM.TotalAmount,
                    //            HSNCodeId = taxVM.HSNCodeId,
                    //            Percentage = taxVM.Percentage,
                    //            SalesId = sales.Id,
                    //            SalesMaterialId = salesMaterial.Id,
                    //            TaxCategoryId = taxVM.TaxCategoryId,
                    //            SalesServiceId = null,
                    //            ModelState = ModelState.Added,
                    //            UpdatedBy = null,
                    //            UpdatedDate = null,
                    //            UpdatedFromIP = null
                    //        };
                    //        _salesTaxRepository.Insert(salesTax);
                    //    }
                    //}
                }
                //}
                //if (salesServiceVMList != null)
                //{
                //    foreach (var salesServiceVM in salesServiceVMList)
                //    {

                //        currentSalesServiceId++;
                //        var salesService = new Model.SalesManagements.SalesService
                //        {
                //            AddedBy = sales.AddedBy,
                //            AddedDate = sales.AddedDate,
                //            AddedFromIP = sales.AddedFromIP,
                //            Amount = salesServiceVM.Amount,
                //            Id = _pkGeneratorService.MakePK(sales.Id, currentSalesServiceId, 2),
                //            ModelState = ModelState.Added,
                //            NetAmount = salesServiceVM.NetAmount,
                //            SalesId = sales.Id,
                //            ServiceMasterId = salesServiceVM.ServiceMasterId,
                //            TaxAmount = salesServiceVM.TaxAmount,
                //            UpdatedBy = null,
                //            UpdatedDate = null,
                //            UpdatedFromIP = null
                //        };
                //        _salesServiceRepository.Insert(salesService);

                //        if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                //        {
                //            foreach (var taxVM in salesServiceVM.ServiceTaxList)
                //            {
                //                if (taxVM.TaxCategoryId == null)
                //                    throw new CustomException("Please Select Tax Category !");

                //                currentSalesTaxId++;
                //                var salesTax = new SalesTax
                //                {
                //                    Id = _pkGeneratorService.MakePK(salesService.Id, currentSalesTaxId, 2),
                //                    AddedBy = salesService.AddedBy,
                //                    AddedDate = salesService.AddedDate,
                //                    AddedFromIP = salesService.AddedFromIP,
                //                    Amount = taxVM.TotalAmount,
                //                    HSNCodeId = taxVM.HSNCodeId,
                //                    Percentage = taxVM.Percentage,
                //                    SalesId = sales.Id,
                //                    SalesMaterialId = null,
                //                    SalesServiceId = salesService.Id,
                //                    TaxCategoryId = taxVM.TaxCategoryId,
                //                    ModelState = ModelState.Added,
                //                    UpdatedBy = null,
                //                    UpdatedDate = null,
                //                    UpdatedFromIP = null
                //                };
                //                _salesTaxRepository.Insert(salesTax);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public override void Update(IssueRequestMaster entity)
        {
            try
            {
                //var leastDate = base.Query(t => t.Id != entity.Id && t.PlantId == entity.PlantId).Select(t => t.GRNDate).OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenByDescending(t => t.Date).FirstOrDefault();
                //if (Convert.ToDateTime(entity.GRNDate) < leastDate) throw new CustomException("GRN date can't less then " + leastDate.ToString("dd/MMM/yyyy"));
               // ResetCurrencyRate(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        private static void ResetCurrencyRate(InventoryReceive entity)
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
            }
            else
            {
                if (entity.BaseCurrencyId == entity.CurrencyId)
                    entity.ToCurrencyRate = 1;
            }
        }

        public GridModel Query(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.sort = "GRNDate";
                parameters.order = "DESC";

                parameters.CmdText = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    --, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IsApproved = 0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> QueryGetListForMasterData(string plantId)

        {
            try
            {
                //parameters.sort = "GRNDate";
                //parameters.order = "DESC";

                var sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IsApproved = 0 order by IR.GRNDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public GridModel GetListByGrnno(GridParameter parameters, string plantId, int GRN)
        {
            try
            {
                parameters.sort = "GRNDate";
                parameters.order = "DESC";
                var _wc = string.Empty;

                #region Approve/Unapprove Count
                //if (GRN == 1)
                //{
                //    _wc = "APPROVED";

                //}
                //else
                //{
                //    _wc = "UNAPPROVED";

                //}
                #endregion Approve/Unapprove Count

                parameters.CmdText = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    --, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId 
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL  And IR.IsApproved ='" + GRN + "' and GAG.CtnId is not null";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public GridModel GetPostingList(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
			                        , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable
                                    , COUNT(*) OVER () AS TotalRows
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId=@plantId AND IR.[Status]='Posting' AND IR.IsPaymentHold=0 AND CP.PlantId=@plantId AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel GetListForHold(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsApproved=1 AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel GetEmployeePurchaseList(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName, IR.EmployeeId, EI.EmployeeName, EI.EmployeeCode
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId<>''";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel GetListForInvPayable(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.UserName AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                            , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                            , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                            , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate
                    FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                    JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                     LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBaseAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0 AND CP.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL AND IR.IsApproved=1";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListForInvShortagePayable( string plantId)
        {
            try
            {
                var Sql = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.UserName AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                            , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                            , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                            , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.ShortageQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.ShortageAmount, IRD.ShortageBaseAmount
								,IsWrittenOff=case when IV.IsWrittenOff IS NULL then 0 else IV.IsWrittenOff end
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate, IV.Id AS InvoiceId, IV.IsWrittenOff
                                FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                   
                                LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                                JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                                LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                                LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                                LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
								LEFT JOIN [TRN].Invoice AS IV ON IV.InventoryReceiveId=IR.Id
                                LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.ShortageQty) AS ShortageQty, SUM( A.MaterialTranRate * A.ShortageQty) AS ShortageAmount, SUM(A.BooksCurrencyBaseRate * A.ShortageQty) AS ShortageBaseAmount 
                                FROM [TRN].[InventoryReceiveDetail] AS A
                                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                                LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
                                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                                WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[ShortageStatus],'')<>'Posting' AND IR.IsPaymentHold=0 AND CP.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' 
                                AND IR.OpeningBalanceId IS NULL AND IR.IsApproved=1 AND IRD.ShortageQty > 0";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListForInvRejectPayable(string plantId)
        {
            try
            {
                var Sql = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.UserName AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                            , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                            , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                            , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.RejectionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.RejectionAmount, IRD.RejectionBaseAmount
								,IsWrittenOff=case when IV.IsWrittenOff IS NULL then 0 else IV.IsWrittenOff end
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate, IV.Id AS InvoiceId, IV.IsWrittenOff
                                FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                   
                                LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                                JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                                LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                                LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                                LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
								LEFT JOIN [TRN].Invoice AS IV ON IV.InventoryReceiveId=IR.Id
                                LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.RejectionQty) AS RejectionQty, SUM( A.MaterialTranRate * A.RejectionQty) AS RejectionAmount, SUM(A.BooksCurrencyBaseRate * A.RejectionQty) AS RejectionBaseAmount 
                                FROM [TRN].[InventoryReceiveDetail] AS A
                                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                                LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
                                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                                WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[ShortageStatus],'')<>'Posting' AND IR.IsPaymentHold=0 AND CP.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' 
                                AND IR.OpeningBalanceId IS NULL AND IR.IsApproved=1 AND  IRD.RejectionQty > 0";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId)
        {
            try
            {
                decimal toCurrencyRate = 0;
                if (currencyId != baseCurrencyId)
                {
                    var sql = @"SELECT ISNULL((SELECT TOP(1) ISNULL(A.ToCurrencyBankSelling,0) FROM SCS.ExchangeRate AS A WHERE
                                            FromCurrencyCode='" + currencyId + "' AND ToCurrencyCode='" + baseCurrencyId + "' AND A.FromDate<='" + docDate + @"' AND A.CompanyId='" + companyId + "' ORDER BY CAST(FromDate AS DATE) DESC), 0)";
                    toCurrencyRate = _inventoryReceiveRepository.SqlQuery<decimal>(sql).First();
                }
                return toCurrencyRate;
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
        }

        public void Delete(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryReceiveDetail WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                if (!detail && !service)
                {
                    var data = base.Find(id);
                    if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                    base.Delete(data);
                }
                else throw new CustomException("Please delete first line item.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListForHold(string plantId)
        {
            try
            {
                //var Sql = @"select ROW_NUMBER() OVER (ORDER BY po.Id DESC) AS SiNo,po.Id,Po.PODate,po.PartyId,po.DocRefNo,po.DocDate
                //           ,po.CurrencyId,po.ToCurrencyRate
                //           ,PO.PaymentTermId,Po.BaseOnDueDate,PO.BaseNoOfDays,PO.MatureDate,PO.IsNonCreditable,p.StandardName PartyName
                //           ,sum(CONVERT(DECIMAL(10,2),pod.TransactionQty)) TransactionQty 
                //              from TRN.PurchaseOrder po
                //Inner Join TRN.PurchaseOrderDetail pod on pod.InventoryReceiveId=po.Id 
                //                  INNER JOIN [HKP].[Party] p on po.PartyId=p.Id
                //                  where po.IsApproved=1 and isnull(po.Isclosed,0)=0
                //                   and pod.QtyStatus=0
                //--AND PO.Id=201964
                //--AND PO.Id=201964
                //group by  po.Id,Po.PODate,po.PartyId,po.DocRefNo,po.DocDate,po.CurrencyId,po.ToCurrencyRate
                //                  ,PO.PaymentTermId,Po.BaseOnDueDate,PO.BaseNoOfDays,PO.MatureDate,PO.IsNonCreditable
                //                  ,p.StandardName";

                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                                   SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
                                     , CP.UserName AS PartyAccountGroupName
                                           , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                                           --, IR.GateEntryNo
                                              --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                              , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
                                           , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
                                           , IR.FixedAssetOrInventory, IR.PODepended
                                              --, IR.AlongwithInvoice
                                              --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
                                           , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
                                           , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                              , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
                			, IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                            ,IPP.UserName As InvoicingByName
                            ,pgl.CtnId
                                  FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                                  LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                     ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                  JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                  LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                  LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                                  LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                                  LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                  LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                                  LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                                  LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                  LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                                  LEFT JOIN (SELECT A.InventoryReceiveId,A.QtyStatus, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
                                        JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId,A.QtyStatus) AS IRD ON IRD.InventoryReceiveId=IR.Id
                                  LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id
                                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                                  LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                                  LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approved' group by POID) as pgl  on pgl.POID=IR.Id
                                  WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting'  AND IR.EmployeeId IS NULL AND IR.IsApproved=1 
                                  AND IR.IsClosed=0 and IRD.QtyStatus=0 AND pgl.CtnId is not null
                                  Order by IR.PODate DESC, IR.ID DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListOfPOGateEntry(string CompanyGroupId, string CompanyId, string PlantId,string partyCode)
        {
            try
            {

                var Sql = @"Select 
	                                GE.Id
	                                ,REPLACE(CONVERT(CHAR(11), GE.EntryDate , 106),' ','-') AS EntryDate
	                                ,GE.PartyCode
	                                ,GE.InvoicingPartyPlantId
	                                ,GE.InvoicingByAddress
	                                ,GE.DeliveryPartyPlantId
	                                ,GE.DeliveryByAddress
	                                ,GE.Description
	                                ,GE.PackageQty
	                                ,GE.ModeofTransport
	                                ,GE.Bill
	                                ,GE.PersonName
	                                ,MobileNo
	                                ,GE.Remarks
	                                ,GE.AddedBy
	                                ,p.UserName
	                                ,p.Id
                                FROM TRN.GateEntry GE
                                left Join hkp.Party p on p.Id=GE.PartyCode
                                Where GE.CompanyGroupId='" + CompanyGroupId + "' AND GE.CompanyId='" + CompanyId + "' AND GE.PlantId='" + PlantId + "' and p.Code='" + partyCode + "' " +
                                
                                " Order By GE.EntryDate DESC";
                //AND GE.Id not in(select GateEntryNo from trn.InventoryReceive)
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }




        #endregion InventoryReceive

        #region Tax

        public IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                                    JOIN TRN.InventoryReceive AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id
                                    JOIN TRN.InventoryReceive AS IR ON IR.InvoicingPartyPlantId=PP.Id WHERE IR.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId) AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetReceiveTaxList(string receiveDetailId)
        {
            try
            {
                //var sql = @"SELECT A.Id,A.InventoryReceiveDetailId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount,d.id As PODetailId
                //            FROM [TRN].[InventoryReceiveTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                //            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                //            WHERE A.InventoryReceiveDetailId='" + receiveDetailId + "' AND A.InventoryServiceId IS NULL ORDER BY TC.[Sequence]";
                var sql = @"SELECT A.Id,A.InventoryReceiveDetailId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount,d.id As PODetailId
                            FROM [TRN].[InventoryReceiveTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            left join TRN.InventoryReceiveDetail d on d.id= A.InventoryReceiveDetailId
                            WHERE A.InventoryReceiveId='" + receiveDetailId + "' AND A.InventoryServiceId IS NULL ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetReceiveTaxListPO(string receiveDetailId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.InventoryReceiveDetailId,A.InventoryReceiveId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount,d.id As PODetailId
                            FROM [TRN].[PurchaseOrderTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
							left join TRN.PurchaseOrderDetail d on d.id= A.InventoryReceiveDetailId
                            WHERE A.InventoryReceiveId='" + receiveDetailId + "' AND A.InventoryServiceId IS NULL ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetTotalReceiveTaxList(string receiveId)
        {
            try
            {
                var sql = @"SELECT A.TaxCategoryId, TC.UserName AS TaxCategory, SUM(A.TaxAmount) AS TaxAmount, TC.[Sequence]
                            FROM [TRN].[InventoryReceiveTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            WHERE A.InventoryReceiveId='" + receiveId + "' AND A.InventoryServiceId IS NULL GROUP BY A.TaxCategoryId, TC.UserName, TC.[Sequence] ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetServiceTaxList(string serviceId)
        {
            try
            {
                //var sql = @"SELECT A.Id,A.InventoryServiceId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                //            FROM [TRN].[InventoryReceiveTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                //            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                //            WHERE A.InventoryServiceId='" + serviceId + "' AND A.InventoryReceiveDetailId IS NULL ORDER BY TC.[Sequence]";
                var sql = @"SELECT A.Id,A.InventoryServiceId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM [TRN].[InventoryReceiveTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.InventoryReceiveId='" + serviceId + "' AND A.InventoryReceiveDetailId IS NULL ORDER BY TC.[Sequence]";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetServiceTaxListPO(string serviceId)
        {
            try
            {
                var sql = @"SELECT A.Id,A.InventoryServiceId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM [TRN].[PurchaseOrderTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.InventoryReceiveId='" + serviceId + "' AND A.InventoryReceiveDetailId IS NULL ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #endregion Tax

        public decimal GetChargesRatio(string receiveId, string detailId, decimal detailTotalAmnt, string serviceId, decimal svcTotalAmnt, bool isNonCreditable)
        {
            try
            {
                decimal svcAmount = 0;
                if (isNonCreditable)
                    svcAmount = _inventoryReceiveRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0)+ISNULL(SUM(TotalTaxAmount), 0) FROM TRN.InventoryService WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                else
                    svcAmount = _inventoryReceiveRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(Amount), 0) FROM TRN.InventoryService WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + serviceId + "'").First();
                if (svcTotalAmnt > 0) svcAmount += svcTotalAmnt;
                else svcAmount -= svcTotalAmnt;

                var detailAmount = _inventoryReceiveRepository.SqlQuery<decimal>("SELECT ISNULL(SUM(MaterialTranAmount), 1) FROM TRN.InventoryReceiveDetail WHERE InventoryReceiveId='" + receiveId + "' AND ISNULL(Id, '')<>'" + detailId + "'").First();
                if (detailTotalAmnt > 0)
                {
                    detailAmount += detailTotalAmnt;
                }

                else
                {
                    //detailAmount -= detailTotalAmnt;
                    detailAmount = 1;
                }

                return svcAmount == 0 && detailAmount == 0 ? 0 : svcAmount / detailAmount;
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
        }

        public void GRNApproved(IEnumerable<InventoryReceive> entities, string GRNStatus)
        {
            var flag = false;
            try
            {

                if (entities.IsNull()) throw new CustomException("Select GRN");
                var ids = entities.Select(t => t.Id).ToArray();
                var dbList = base.Query(t => ids.Contains(t.Id)).Select().ToList();
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var entity in entities)
                {

                    foreach (var item in dbList)
                    {
                        var Status = "";


                        if (entity.Id == item.Id)
                        {
                            if (GRNStatus == "1")
                            {
                                Status = "UnApproved";
                                entity.IsApproved = false;
                                //item.IsApproved = entity.IsApproved;
                            }
                            else
                            {
                                Status = "Approved";
                                entity.IsApproved = true;
                               // item.IsApproved = entity.IsApproved;
                            }
                            base.UpdateGraph(item);

                            var PoValue = "0";
                            var Id = GetPK();
                            var GRNId = entity.Id;


                            var UpdatedBy = "";
                            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                            var ip = identity.IPAddress;
                            var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                            var AddedBy = identity.Name;
                            var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                            var CompanyGroupId = identity.CompanyGroupId;
                            var CompanyId = identity.CompanyId;
                            var PlantId = identity.PlantId;

                            string _sql1 = "Insert into TRN.GRNApprovalLogTbl(Id," +
                            "CompanyGroupId," +
                            "CompanyId," +
                            "PlantId," +
                            "ApprovedBy," +
                            "Date," +
                            "POValue," +
                            "Status," +
                            "AddedBy," +
                            "AddedDate," +
                            "AddedFromIp," +
                            "UpdatedBy," +
                            "UpdatedDate," +
                            "UpdatedFromIp,GRNId) " +
                            "values ('" + Id + "'," +
                            "'" + CompanyGroupId + "'," +
                            "'" + CompanyId + "'," +
                            "'" + PlantId + "'," +
                            "'" + AddedBy + "'," +
                            "'" + AddedDate + "'," +
                            "'" + PoValue + "'," +
                            "'" + Status + "'," +
                            "'" + AddedBy + "'," +
                            "'" + AddedDate + "'," +
                            "'" + ip + "'," +
                            "'" + UpdatedBy + "'," +
                            "'" + updatedDate + "', " +
                            "'" + ip + "','" + GRNId + "')";
                            _sqlRepository.ExecuteSqlCommand(_sql1);

                        }
                    }
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
        public void GRNApproved1(IEnumerable<InventoryReceive> entities, string GRNStatus, string GRNNo,string AuthorizedByStatus)
        {
            var flag = false;
            try
            {

                //if (entities.IsNull()) throw new CustomException("Select GRN");
                //var ids = entities.Select(t => t.Id).ToArray();
                //var dbList = base.Query(t => ids.Contains(t.Id)).Select().ToList();
                //_unitOfWork.BeginTransaction();
                //flag = true;
                //foreach (var entity in entities)
                //{

                //    foreach (var item in dbList)
                //    {
                //        var Status = "";

                //        if (entity.Id == item.Id)
                //        {
                //        }
                //    }
                //}

                //_unitOfWork.SaveChanges();
                //flag = false;
                //_unitOfWork.Commit();
                bool IsApproved;
                var Status = "";
                if (GRNStatus == "1")
                {
                    Status = "UnApproved";
                    IsApproved = false;

                }
                else
                {
                    Status = "Approved";
                    IsApproved = true;

                }
                //base.UpdateGraph(item);

                var PoValue = "0";
                var Id = GetPK();
                var GRNId = GRNNo;


                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql2 = "Update TRN.InventoryReceive set IsApproved='" + IsApproved + "' ,AuthorizedByStatus='" + AuthorizedByStatus + "' where Id='" + GRNNo + "'";
                _sqlRepository.ExecuteSqlCommand(_sql2);

                string _sql1 = "Insert into TRN.GRNApprovalLogTbl(Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,GRNId) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + GRNId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);


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
        public void PaymentHold(IEnumerable<InventoryReceive> entities)
        {
            var flag = false;
            try
            {
                if (entities.IsNull()) throw new CustomException("Select GRN");
                var ids = entities.Select(t => t.Id).ToArray();
                var dbList = base.Query(t => ids.Contains(t.Id)).Select().ToList();
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var entity in entities)
                {
                    foreach (var item in dbList)
                    {
                        if (entity.Id == item.Id)
                        {
                            //item.IsPaymentHold = entity.IsPaymentHold;
                            base.UpdateGraph(item);
                        }
                    }
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

        #region GRNReport

        public void InventoryReceive(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId)
        {

            var fileName = "";
            var strPath = "";

            var File = "";

            ReportUtility ru = new ReportUtility();

            //tempId = dtLangName.Rows[0]["UserName"].ToString();
            fileName = "GRNReport" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            //makeDictionary();
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                WSection section = document.Sections[0];

                DataTable dtOrderMaster;


                dtOrderMaster = loadGRNMaterialMaster(grnId);

                var invoicePartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dtOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);

                var vendorPartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);

                Dictionary<string, string> columns = new Dictionary<string, string>();

                var poApprovedStatus = "";
                if (string.IsNullOrEmpty(dtOrderMaster.Rows[0]["IsApproved"].ToString()) == false)
                {
                    if (Convert.ToBoolean(dtOrderMaster.Rows[0]["IsApproved"]) == false)
                    {
                        poApprovedStatus = "Unapproved";
                        document.Replace("{GRNApprovedStatus}", poApprovedStatus, true, true);
                    }
                    else
                    {
                        var poApprovedDT = _sqlRepository.GetDataTable(@"select Count(*) ApproveNumber from trn.GRNApprovalLogTbl where GRNID = '" + grnId + @"' and [Status] = 'Approved'");

                        poApprovedStatus = "Approved(" + poApprovedDT.Rows[0]["ApproveNumber"] + ")";
                        document.Replace("{GRNApprovedStatus}", poApprovedStatus, true, true);
                    }
                }


                foreach (DataColumn item in dtOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var dsServiceItems = loadGRNServiceMaster(grnId);
                var materialTotal = makeOrderDetailsTable(document, dtOrderMaster, grnId);//Material Details 
                var serviceTotal = 0.00;
                if (dsServiceItems.Rows.Count > 0)
                {
                    serviceTotal = makeOrderServiceTable(document, dsServiceItems, grnId);//Service Details 
                    document.Replace("{ServiceDetails}", "Service Details", true, true);
                }
                //{TotalInWords}
                document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2") + " " + dtOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord((materialTotal + serviceTotal), dtOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();

                StringCollection strColDistinct = new StringCollection();

                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());             //For Same Name Use
                    string text = strReplace[i].ToUpper();

                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }

        public DataTable loadGRNMaterialMaster(string OrderMasterID)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IR.Id grnNumber
                                ,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,ir.PODepended 
								,IR.POId PONumber                                 
                                ,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS PODate
                                ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
		                        ,IR.InvoicingPartyPlantId
		                       
		                             ,INVPARTYPL.UserName InvoicingPartyName
                                                ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                                                ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                                                ,ISNULL(IR.InvoicingByAddress,'') InvoicingByAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
		                        ,IR.DeliveryPartyPlantId		
		                        ,IOM.MaterialMasterId
		                        ,IR.DocRefNo
		                        ,IR.DocDate
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
		                        ,IR.IsApproved
		                        ,IR.PartyType
		                        ,Party.UserName VendorName
                                ,Party.AddressMasterId VendorAddressMasterId
                                ,Party.TINNO VendorGSTIN
                                ,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
		                        ,IR.IsNonCreditable
		                        ,IR.CurrencyId
	                            ,CRNC.Code AS CurrencyName
	                            ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
		                        ,PayTerm.UserName PaymentTerm
	                          ,MM.UserName MaterialMaster
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IOM.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS FirstCharacteristicsValue
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRD.TransactionQty, 2) POTransactionQty
	                          ,ROUND(IRD.MaterialTranRate, 2) TransactionRate
	                          ,ROUND((IRD.TransactionQty * IRD.MaterialTranRate), 2) AS TrnAmount
	                          ,IRD.TotalMaterialTranAmount BaseAmount
	                          ,IRD.TotalTaxAmount AS BaseTaxAmount
	                          ,TaxAmount = (
		                            SELECT SUM(TaxAmount)
		                            FROM [TRN].[PurchaseOrderTax]
		                            WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
		                            SELECT SUM(TotalTaxAmount)
		                            FROM [TRN].[POService]
		                            WHERE InventoryReceiveId = IOM.Id
		                            )
	                          ,IRD.ChargesTranAmount
	                          ,IRD.CountryId

	                          ,IRD.TransactionUoMId
	                          ,TUoM.UserName AS TransactionUoM
                              ,IRD.Id InventoryReceiveDetailId
                              FROM TRN.InventoryReceive IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.BaseCurrencyId
                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId 
                         LEFT JOIN trn.inventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId
						 LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId                                        
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                         WHERE IR.Id ='" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable loadGRNServiceMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TotalTaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                               FROM TRN.InventoryReceive   IR
                            INNER join trn.inventoryservice IOS ON IOS.InventoryReceiveId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                            where IR.Id = '" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable loadGRNServiceMasterTex(string OrderMasterID)
        {
            string strSQL;
            try
            {
                strSQL = @"select InventoryServiceId,IR.Id PurchaseOrderId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount
                    from TRN.InventoryReceive IR
                              INNER JOIN trn.InventoryService ISER ON ISER.InventoryReceiveId = IR.Id
                              Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = IR.Id and IRT.InventoryServiceId = ISER.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                WHERE IR.Id='" + OrderMasterID + @"'
								and InventoryServiceId  is not null and   InventoryReceiveDetailId is null 
								 ORDER BY tg.[Sequence]";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public double makeOrderDetailsTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{materialItems}";

            DataTable dsOrderItems, dsTax;

            dsOrderItems = loadOrderMasterItems(grnId);
            dsTax = loadOrderMasterTax(grnId);

            int LasColumnIndex = 8;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }


            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            //wTable.Description = "This table shows the price details of PI";
            //wTable.IndentFromLeft = 10;


            //string UOM = dsOrderMaster.Tables[0].Rows[0]["UOM"].ToString();
            //string Currency = dsOrderMaster.Tables[0].Rows[0]["Currency"].ToString();
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Char1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Char2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Char3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL;


            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);

                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Value");
            }


            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("F2"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString().ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("F2"));

                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));

                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));

                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colRate || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colQty || C == colRate || C == colUoM || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable

            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }

            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return total;
        }

        public double makeOrderServiceTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{ServiceItems}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadGRNServiceMasterTex(grnId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("ServiceName");
            range.ApplyCharacterFormat(FontBold);
            int colServiceName = COL; //COL++;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Service"].ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Amount"].ToString()).ToString("F2"));

                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsOrderMaster.Rows[i]["ServiceId"] + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));
                        }
                    }
                }
            }

            ROW++;

            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;

            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2"));
            }


            #endregion Total


            ROW++;


            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(Amount)", "").ToString())
//- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());



            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;

            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            myStyle2.CharacterFormat.TextColor = Color.Black;
            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle2");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;


            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public DataTable loadOrderMasterItems(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
                                  from [TRN].[MasterOrderItem] T
                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
                                where MasterOrderId='" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable loadOrderMasterTax(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select InventoryServiceId,PO.Id PurchaseOrderId,InventoryReceiveDetailId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount from TRN.InventoryReceive PO
                               INNER JOIN trn.inventoryReceiveDetail IRD ON IRD.InventoryReceiveId = PO.Id
                               Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = PO.Id and IRT.InventoryReceiveDetailId = IRD.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                 WHERE PO.Id='" + OrderMasterID + @"' 
								 and InventoryReceiveDetailId  is not null and  InventoryServiceId is null 
								 ORDER BY tg.[Sequence] ";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        class clsStdLib
        {
            public static string passWord = "prodDisplay";
            public clsStdLib()
            {

            }
            public enum mType
            {
                Error,
                Success,
                Information
            }
            public static bool passwordGet = true;
            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            public static string DataRankNames(int dayNo)
            {

                if (dayNo <= 0)
                    return "";

                if (dayNo.ToString().Length > 1)
                {
                    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
                    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
                        return dayNo + "th";
                }

                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
                switch (RightString)
                {
                    case "1":
                        return dayNo + "st";
                    case "2":
                        return dayNo + "nd";
                    case "3":
                        return dayNo + "rd";
                    default:
                        return dayNo + "th";

                }




            }

            #region date related
            public static readonly string dateFormat = "dd-MMM-yyyy";
            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
            public static bool IsDateOK(string strdate)
            {
                try
                {
                    if (strdate.Length != 11)
                    {
                        return false;
                    }
                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                    {
                        return false;
                    }
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            private static bool DateOkCheck(string strdate)
            {
                try
                {
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            public static object chk_NullDateData(object dateValue)
            {
                if (DateOkCheck("" + dateValue.ToString()) == false)
                {
                    dateValue = "";
                }

                if (("" + dateValue.ToString()) == "")
                {
                    System.DateTime dt = new System.DateTime(1901, 1, 1);
                    dateValue = (object)dt;
                }
                return (object)dateValue;
            }
            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
            {
                string strDate = null;
                dateValue = chk_NullDateData(dateValue);
                strDate = dateValue.ToString();
                if (strDate != "")
                {
                    if (input_date_format.Trim() != "")
                    {
                        if (output_date_format.Trim() != "")
                        {
                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                            InputFormat.ShortDatePattern = input_date_format;
                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                            strDate = myDt.ToString(output_date_format);
                        }
                    }
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
            {
                if (string.IsNullOrEmpty((string)dateValue))
                    return DBNull.Value;

                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
                }

                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


            }// End of function
            public static System.DateTime DateData_DBToApp(object dateValue)
            {
                string strDate = null;
                strDate = dateValue.ToString();

                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
                return System.Convert.ToDateTime(strDate);
            }// End function
            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static String makeBaseBlank(object dateValue)
            {
                System.DateTime dt;
                dt = System.Convert.ToDateTime(dateValue.ToString());
                if (dt.Year == 1901)
                {
                    return "";
                }
                else
                {
                    return dateValue.ToString();
                }
            }// End of function
             ///<summary>
             ///return day difference in integer. 
             ///    Example 1: firstDate[Less Than]lastDate returns positive value
             ///    Example 2: firstDate>lastDate returns negative value
             ///    Example 3: firstDate=lastDate returns 0 [zero]**/
             /// </summary>
            public static int dateDiff(string firstDate, string lastDate)
            {

                int difference = 0;
                try
                {
                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                    if (IsDateOK(firstDate) == false)
                    {
                        Exception ex = new Exception("Invalid [First Date]");
                        throw (ex);
                    }
                    if (IsDateOK(lastDate) == false)
                    {
                        Exception ex = new Exception("Invalid [Last Date]");
                        throw (ex);
                    }
                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                    difference = TimeSpan.Days;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                return difference;
            }



            public static string getSqliteDate(string standardDate)
            {
                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
            }
            public static string getStandardDateFromSqliteDate(string SqliteDate)
            {
                if (SqliteDate.Length != 10)
                    return "";
                if (SqliteDate.Split('-').Length != 3)
                    return "";
                //many things to validate 
                //but i have less time :)
                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
            }
            #endregion date related

            #region numeric
            public static bool IsNumeric(string strNumber)
            {
                Double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Length == 0)
                {
                    return false;
                }
                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
            } // End Function
            public static string GetNumericData(string strNumber)
            {
                double d;
                strNumber = strNumber.Replace(",", "");
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0"; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return strNumber;
                }
                else
                {
                    return "0";
                }
            }// end function
            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
            {
                if (precision < 1)
                    return strNumber;

                string s_precision = new String('0', precision);

                double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0." + s_precision; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return string.Format("{0:0." + s_precision + "}", d);
                }
                else
                {
                    return "0." + s_precision;
                }
            }// end function
            public static double dbl(string d)
            {
                return Convert.ToDouble(GetNumericData(d));

            }
            public static int Percentage(int total, double percentage)
            {
                return (int)(total * (percentage / 100));

            }
            //validation
            public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
            {

                try
                {



                    if (isMandatory == true)
                    {
                        if (value.Trim() == "")
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }
                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }

                        if (value.Trim() != "")
                        {
                            if (IsNumeric(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                        if (isInteger == true)
                        {

                            if (isInt(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                                throw (ex);
                            }

                        }
                        if (negativeAllowed == false)
                        {
                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                            {
                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }


            }

            ///<summary>
            ///check whether a value is integer or not returns true if integer, 
            ///false if floating or string containing alpahnumeric
            ///</summary>
            public static bool isInt(string num)
            {

                bool isInt;
                int number;
                try
                {
                    isInt = System.Int32.TryParse(num, out number);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
                return isInt;
            }


            #endregion numeric

            #region string

            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
            public static readonly string NumberFormatStringText = "@"; //format cell data as text


            public static object ValidLength(string str)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");

                return (object)removechar.Trim();

            }
            public static object ValidLength(string str, int length)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");


                int strLen = removechar.Length;
                if (strLen > length)
                    removechar = removechar.Substring(0, length);

                return (object)removechar.Trim();

            }
            public static string FileNameLegalChar(string fileName)
            {
                string illegalChar = @"~`!@#$%^&*=/\|>,<";
                foreach (char c in illegalChar)
                {
                    fileName = fileName.Replace(c.ToString(), " ");
                }

                return fileName;
            }
            private StringCollection getTableColumns(ref DataSet dsLocal)
            {
                StringCollection strcol = new StringCollection();
                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
                {
                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
                }

                return strcol;

            }
            public static string emptyString(string str)
            {
                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
                if (str == "&nbsp;")
                    str = "";
                if (string.IsNullOrEmpty(str) == true)
                    str = "";


                return str;
            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
            #endregion string


            #region others
            public void copyDataset(DataSet source, ref DataSet destination)
            {
                StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
                DataRow drLocal = null;
                for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
                {
                    drLocal = destination.Tables[0].NewRow();
                    for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
                    {
                        if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
                        {
                            drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
                        }
                    }
                    destination.Tables[0].Rows.Add(drLocal);
                }


            }
            public static string GetxlsCol(int intCol)
            {
                //returns excel columns based on column number. tested 1 to 256 column numbers
                try
                {
                    if (intCol < 1 || intCol > 256)
                    {
                        System.Exception ex = new Exception("Invalid Column Value");
                        throw (ex);
                    }
                    intCol = intCol - 1;
                    int intFirstLetter = ((intCol) / 512) + 64;
                    int intSecondLetter = ((intCol % 512) / 26) + 64;
                    int intThirdLetter = (intCol % 26) + 65;
                    char FirstLetter;
                    char SecondLetter;
                    if (intFirstLetter > 64)
                        FirstLetter = (char)intFirstLetter;
                    else
                        FirstLetter = ' ';

                    if (intSecondLetter > 64)
                        SecondLetter = (char)intSecondLetter;
                    else
                        SecondLetter = ' ';

                    char ThirdLetter = (char)intThirdLetter;
                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
            }//returns excel columns based on column number. tested 1 to 256 column numbers
            #endregion others

            public static object RetValidLen(string Data)
            {
                if (string.IsNullOrEmpty(Data))
                    return DBNull.Value;

                return Data;
            }
            public static double sum(string columnName, DataTable dtLocal, string criteria)
            {
                double total = 0;
                DataRow[] dr = dtLocal.Select(criteria);
                foreach (DataRow d in dr)
                {
                    total += dbl(d[columnName].ToString());
                }


                return total;
            }
        }

        #region GRN Approval Bye Taufik


        public IEnumerable<object> getListForGRNUnchecked(string plantId)

        {
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    --, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId,IR.PartyId,IR.AddedBy,IR.CheckedBy,IR.CheckedByStatus, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM
                                    , IRD.TransactionAmount
                                    , IRD.BaseAmount, IR.ToCurrencyRate
                                    ,IRD.TotalMaterialTranAmount
                                    ,IRD.TotalMaterialBaseAmount	
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId 
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount ,SUM(A.TotalMaterialTranAmount) AS TotalMaterialTranAmount, sum(A.TotalMaterialBaseAmount) As TotalMaterialBaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL  And IR.IsApproved =0 AND IR.CheckedBy is not null AND IR.CheckedByStatus='ForChecked' Order by IR.GRNDate DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IEnumerable<object> getListForGRNChecked(string plantId)

        {
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    --, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId,IR.AddedBy,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM
                                    , IRD.TransactionAmount
                                    , IRD.BaseAmount, IR.ToCurrencyRate
                                    ,IRD.TotalMaterialTranAmount
                                    ,IRD.TotalMaterialBaseAmount	
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId 
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount ,SUM(A.TotalMaterialTranAmount) AS TotalMaterialTranAmount, sum(A.TotalMaterialBaseAmount) As TotalMaterialBaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL  And IR.IsApproved =0 AND IR.CheckedBy is not null AND IR.CheckedByStatus='Checked' Order by IR.GRNDate DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IEnumerable<object> GetListForGRNAp(string plantId)
        {
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    --, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM
                                    , IRD.TransactionAmount
                                    , IRD.BaseAmount, IR.ToCurrencyRate
                                    ,IRD.TotalMaterialTranAmount
                                    ,IRD.TotalMaterialBaseAmount	
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId 
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount ,SUM(A.TotalMaterialTranAmount) AS TotalMaterialTranAmount, sum(A.TotalMaterialBaseAmount) As TotalMaterialBaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL  And IR.IsApproved =0  And IR.CheckedByStatus='Checked'  Order by IR.GRNDate DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }






        public void PoApproved(string PoId, string PoValue)
        {
            try
            {
                PoValue = "0";
                var Id = GetPK();

                var Status = "Approved";
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql = "Update TRN.PurchaseOrder set IsApproved='1' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,POID) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion

        #region un Approval for post

        public IEnumerable<object> GetListForGRNUNApproval(string plantId)
        {
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    --, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId 
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL  And IR.IsApproved =1 And IR.CheckedByStatus='Checked'And IR.AuthorizedByStatus='Approval'   and GAG.CtnId <> 0 Order by IR.GRNDate DESC";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void PoApproved1(string PoId, string PoValue)
        {
            try
            {
                PoValue = "0";
                var Id = GetPK();

                var Status = "Approved";
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql = "Update TRN.PurchaseOrder set IsApproved='1' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,POID) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion
        #endregion

        #region Inventory Issue Report


        public void InventoryIssueReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId)
        {

            var fileName = "";
            var strPath = "";

            var File = "";

            ReportUtility ru = new ReportUtility();

            //tempId = dtLangName.Rows[0]["UserName"].ToString();
            fileName = "IssueReport" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            //makeDictionary();
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                WSection section = document.Sections[0];

                DataTable dtOrderMaster;


                dtOrderMaster = loadIssueMaterialMaster(grnId);

                var invoicePartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dtOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);

                var vendorPartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);

                Dictionary<string, string> columns = new Dictionary<string, string>();

                //var poApprovedStatus = "";
                //if (string.IsNullOrEmpty(dtOrderMaster.Rows[0]["IsApproved"].ToString()) == false)
                //{
                //    if (Convert.ToBoolean(dtOrderMaster.Rows[0]["IsApproved"]) == false)
                //    {
                //        poApprovedStatus = "Unapproved";
                //        document.Replace("{GRNApprovedStatus}", poApprovedStatus, true, true);
                //    }
                //    else
                //    {
                //        var poApprovedDT = _sqlRepository.GetDataTable(@"select Count(*) ApproveNumber from trn.GRNApprovalLogTbl where GRNID = '" + grnId + @"' and [Status] = 'Approved'");

                //        poApprovedStatus = "Approved(" + poApprovedDT.Rows[0]["ApproveNumber"] + ")";
                //        document.Replace("{GRNApprovedStatus}", poApprovedStatus, true, true);
                //    }
                //}


                foreach (DataColumn item in dtOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var dsServiceItems = loadIssueServiceMaster(grnId);
                var materialTotal = makeIssueOrderDetailsTable(document, dtOrderMaster, grnId);                //Material Details 

                var serviceTotal = 0.00;
                if (dsServiceItems.Rows.Count > 0)
                {
                    serviceTotal = makeIssueOrderServiceTable(document, dsServiceItems, grnId);                //Service Details 
                    document.Replace("{ServiceDetails}", "Service Details", true, true);
                }

                //{ TotalInWords}

                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2") + " " + dtOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{TotalInWords}", ru.InWord((materialTotal +serviceTotal), dtOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();

                StringCollection strColDistinct = new StringCollection();

                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());             //For Same Name Use
                    string text = strReplace[i].ToUpper();

                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }

        public DataTable loadIssueMaterialMaster(string OrderMasterID)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IR.Id grnNumber
                                ,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,null PODepended 
								,IR.Id PONumber                                 
                                ,REPLACE(Convert(VARCHAR(11), IR.IssueDate, 106), ' ', '-') AS PODate
                                ,null BaseOnDueDate
                                ,NULL AS MatureDate
		                        ,null InvoicingPartyPlantId
		                       
		                             ,null InvoicingPartyName
                                                ,null InvoicePartyAddressMasterId
                                                ,null InvoicingPartyGSTIN
                                                ,null InvoicingByAddress
		                        ,null DeliveryByAddress
		                        ,null DeliveryParty
		                        ,null DeliveryPartyPlantId		
		                        ,IOM.MaterialMasterId
		                        ,null DocRefNo 
		                        ,null DocDate
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
		                        ,null IsApproved
		                        ,null PartyType
		                        ,null VendorName
                                ,null VendorAddressMasterId
                                ,null VendorGSTIN
                                --,Case When null IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
		                        ,null IsNonCreditable
		                        ,null CurrencyId
	                            ,null AS CurrencyName
	                            ,null as ToCurrencyRate
		                        ,null AS BaseCurrencyName
		                        ,NULL PaymentTerm
	                          ,MM.UserName MaterialMaster
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IOM.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS FirstCharacteristicsValue
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRD.TransactionQty, 2) POTransactionQty
	                          ,ROUND(IRD.AvgRate, 2) TransactionRate
	                          ,ROUND((IRD.TransactionQty * IRD.AvgRate), 2) AS TrnAmount
	                          ,null BaseAmount
	                          ,null AS BaseTaxAmount
	                          ,TaxAmount = (
		                            SELECT SUM(TaxAmount)
		                            FROM [TRN].[PurchaseOrderTax]
		                            WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
		                            SELECT SUM(TotalTaxAmount)
		                            FROM [TRN].[POService]
		                            WHERE InventoryReceiveId = IOM.Id
		                            )
	                          ,null ChargesTranAmount
	                          ,null CountryId
	                          ,IRD.BaseUOMId
	                          ,TUoM.UserName AS TransactionUoM
                              ,IRD.Id InventoryReceiveDetailId
                              FROM TRN.InventoryIssue IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN trn.InventoryIssueDetail IRD ON IR.Id = IRD.InventoryIssueId						                                   
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
                         WHERE IR.Id ='" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable loadIssueServiceMaster(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TotalTaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                               FROM TRN.InventoryReceive   IR
                            INNER join trn.inventoryservice IOS ON IOS.InventoryReceiveId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                            where IR.Id = '" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable loadIssueServiceMasterTex(string OrderMasterID)
        {
            string strSQL;
            try
            {
                strSQL = @"select InventoryServiceId,IR.Id PurchaseOrderId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount
                    from TRN.InventoryReceive IR
                              INNER JOIN trn.InventoryService ISER ON ISER.InventoryReceiveId = IR.Id
                              Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = IR.Id and IRT.InventoryServiceId = ISER.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                WHERE IR.Id='" + OrderMasterID + @"'
								and InventoryServiceId  is not null and   InventoryReceiveDetailId is null 
								 ORDER BY tg.[Sequence]";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public double makeIssueOrderDetailsTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{materialItems}";

            DataTable dsOrderItems, dsTax;

            dsOrderItems = loadIssueOrderMasterItems(grnId);
            dsTax = loadIssueOrderMasterTax(grnId);

            int LasColumnIndex = 8;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }


            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            //wTable.Description = "This table shows the price details of PI";
            //wTable.IndentFromLeft = 10;


            //string UOM = dsOrderMaster.Tables[0].Rows[0]["UOM"].ToString();
            //string Currency = dsOrderMaster.Tables[0].Rows[0]["Currency"].ToString();
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Char1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Char2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Char3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL;


            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);

                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Value");
            }


            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("F2"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString().ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("F2"));

                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));

                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));

                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colRate || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colQty || C == colRate || C == colUoM || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable

            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }

            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return total;
        }

        public double makeIssueOrderServiceTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{ServiceItems}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadIssueServiceMasterTex(grnId);

            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("ServiceName");
            range.ApplyCharacterFormat(FontBold);
            int colServiceName = COL; //COL++;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                IParagraphItem p = TROW.Cells[colServiceName].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Service"].ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Amount"].ToString()).ToString("F2"));

                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryServiceId='" + dsOrderMaster.Rows[i]["ServiceId"] + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));
                        }
                    }
                }
            }

            ROW++;

            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;

            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2"));
            }


            #endregion Total


            ROW++;


            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(Amount)", "").ToString())
            //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
            + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());



            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total


            ROW++;

            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle2 = document.AddParagraphStyle("MyStyle2");
            //Sets the formatting of the style
            myStyle2.CharacterFormat.FontSize = 8f;
            myStyle2.CharacterFormat.TextColor = Color.Black;
            myStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle2");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style2 = document.AddParagraphStyle("SubTotalStyle2");
            style2.CharacterFormat.Bold = true;
            style2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;


            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle2");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public DataTable loadIssueOrderMasterItems(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
                                  from [TRN].[MasterOrderItem] T
                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
                                where MasterOrderId='" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable loadIssueOrderMasterTax(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select InventoryServiceId,PO.Id PurchaseOrderId,InventoryReceiveDetailId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount from TRN.InventoryReceive PO
                               INNER JOIN trn.inventoryReceiveDetail IRD ON IRD.InventoryReceiveId = PO.Id
                               Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = PO.Id and IRT.InventoryReceiveDetailId = IRD.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                 WHERE PO.Id='" + OrderMasterID + @"' 
         and InventoryReceiveDetailId  is not null and  InventoryServiceId is null 
         ORDER BY tg.[Sequence] ";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        #endregion

        #region Material Ledger 

        public IEnumerable<object> GetOperationMaster()
        {
            try
            {
                var sql = @"SELECT  
                                 OM.Id
                                ,CG.StandardName AS CompanyGroup
                                ,OM.Sequence
                                ,OM.Code
                                ,OM.ShortName
                                ,OM.StandardName
                                ,OM.UserName
                                ,OA.UserName AS OperationActivity
                                ,OT.UserName AS OperationType
                                ,OC.UserName AS OperationCategory
                                ,S.UserName AS Skill
                                ,OM.Type 
                                ,MM.UserName AS MachineMaster
                                ,SG.UserName AS SkillGroup
                                ,LD.UserName AS LegalDesignation
                                ,p.UserName As Process
                                ,OM.ProposedSalary
                                ,OM.Remarks
                                ,OM.Active
                                From [MST].[OperationMaster] OM
                                LEFT JOIN [ORG].CompanyGroup CG ON CG.Id=OM.CompanyGroupId
                                LEFT JOIN [HKP].[OperationActivity] OA ON OA.Id=OM.OperationActivityId
                                LEFT JOIN [HKP].[OperationType] OT ON OT.Id=OM.OperationTypeId
                                LEFT JOIN [HKP].[OperationCategory] OC ON OC.Id=OM.OperationCategoryId
                                LEFT JOIN [HKP].[Skill] S On S.Id=OM.SkillId
                                LEFT JOIN [MST].[MachineMaster] MM ON MM.Id=OM.MachineMasterId
                                LEFT JOIN [SCS].[SkillGrouping] SG ON SG.Id=OM.SkillGroupId
                                LEFT JOIN [HKP].[LegalDesignation] LD ON LD.Id=OM.LegalDesignationId
                                LEFT JOIN [HKP].[Process] P ON P.Id=OM.ProcessId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //public IEnumerable<object> GetListForGRNCheck(string plantId)
        //{
        //    throw new NotImplementedException();
        //}

        //public IEnumerable<object> GetListForGRNUnCheck(string plantId)
        //{
        //    throw new NotImplementedException();
        //}

        //public void GRNCheck(string PoId, string PoValue)
        //{
        //    throw new NotImplementedException();
        //}

        //public void GRNUnCheck(string PoId, string PoValue)
        //{
        //    throw new NotImplementedException();
        //}


        public void GRNChecked(string GRNId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            try
            {
                var AuthorizedById = "";

                PoValue = "0";
                var Id = GetPK();
                if (CheckedStataus == "Checked")
                {
                    if (AuthorizedBy == null || AuthorizedBy == "")
                    {
                        throw new CustomException("Select Approved By");
                    }
                    AuthorizedById = AuthorizedBy;

                }
                else
                {
                    AuthorizedById = null;

                }
                var Status = CheckedStataus;
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                // string _sql = "Update TRN.PurchaseOrder set IsApproved='0',CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "' where id='" + PoId + "'";
                //_sqlRepository.ExecuteSqlCommand(_sql);
                string _sql2 = "Update TRN.InventoryReceive set IsApproved='0',CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "' where Id='" + GRNId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql2);

                string _sql1 = "Insert into TRN.GRNApprovalLogTbl(Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,GRNId) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + GRNId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion

        #region Grn IssueRequest and Issue

        public IEnumerable<object> IssueSlipFilter()
        {
            try
            {
                var sql = @"Select 
                           
                            En.Username As EntityName
                            ,MRM.EntityId
                            ,Bu.Code
                            ,Bu.UserName
                            ,Us.FullName AddedBy
                            ,MRM.Id RequisitionNo
                            ,MRD.ArticleId
                            ,Dp.UserName DepartmentName
							,MGM.UserName MaterialMasterGroupName
							,mm.UserName MaterialMasterName
	                        ,ART.StandardName
							,MT.UserName MaterialType
	                        ,MRD.FirstCharacteristicsId
	                        ,FC.UserName AS FirstCharacteristics
	                        ,MRD.FirstCharacteristicsValueId
	                        ,FCV.UserName AS FirstCharacteristicsValue
	                        ,MRD.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,MRD.SecondCharacteristicsValueId
	                        ,SCV.UserName AS SecondCharacteristicsValue
	                        ,MRD.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,MRD.ThirdCharacteristicsValueId
	                        ,TCV.UserName AS ThirdCharacteristicsValue
                            ,MRD.Id AS RequisitionDetailId
                         from [TRN].[MaterialRequsitionDetails] As MRD
                        Left Join  [TRN].[MaterialRequsitionMaster] As MRM On MRD.MaterialReqqusitionMasterId=MRM.Id
                        Left Join [ORG].[Entity] As En On MRM.EntityId=En.Id
                        Left Join [HKP].[Budget] As Bu On Bu.Id=MRD.ActivityId
                        Left JOIN MST.MaterialMaster AS MM ON MRD.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON MRD.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON MRD.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON MRD.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON MRD.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON MRD.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON MRD.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON MRD.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MRD.TransactionUoMId = TUoM.Id
                        LEFT JOIN [SEC].[User] As Us On MRM.AddedBy=Us.UserId
                        LEFT JOIN dbo.EmployeeInformation As Em On Us.EmployeeId=Em.SystemId
                        LEFT JOIN [ORG].[Department] AS Dp On Dp.Id=Em.DepartmentId
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion



        #region IssueRequestReport

        public void IssueRequestReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string issueId)
        {

            var fileName = "";
            var strPath = "";

            var File = "";

            ReportUtility ru = new ReportUtility();

            //tempId = dtLangName.Rows[0]["UserName"].ToString();
            fileName = "IssueRequestReport" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            //makeDictionary();
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                WSection section = document.Sections[0];

                DataTable dtOrderMaster;


                dtOrderMaster = loadIssueRequestMaster(issueId);

                //var invoicePartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dtOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                //document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);

                //var vendorPartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                //document.Replace("{VendorAddress}", vendorPartyAddress, false, false);

                Dictionary<string, string> columns = new Dictionary<string, string>();

                //var poApprovedStatus = "";
                //if (string.IsNullOrEmpty(dtOrderMaster.Rows[0]["IsApproved"].ToString()) == false)
                //{
                //    if (Convert.ToBoolean(dtOrderMaster.Rows[0]["IsApproved"]) == false)
                //    {
                //        poApprovedStatus = "Unapproved";
                //        document.Replace("{GRNApprovedStatus}", poApprovedStatus, true, true);
                //    }
                //    else
                //    {
                //        var poApprovedDT = _sqlRepository.GetDataTable(@"select Count(*) ApproveNumber from trn.GRNApprovalLogTbl where GRNID = '" + issueId + @"' and [Status] = 'Approved'");

                //        poApprovedStatus = "Approved(" + poApprovedDT.Rows[0]["ApproveNumber"] + ")";
                //        document.Replace("{GRNApprovedStatus}", poApprovedStatus, true, true);
                //    }
                //}


                foreach (DataColumn item in dtOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                //var dsServiceItems = loadGRNServiceMaster(issueId);
                var materialTotal = makeOrderDetailsTable(document, dtOrderMaster, issueId);//Material Details 
                var serviceTotal = 0.00;
                //if (dsServiceItems.Rows.Count > 0)
                //{
                //    serviceTotal = makeOrderServiceTable(document, dsServiceItems, issueId);//Service Details 
                //    document.Replace("{ServiceDetails}", "Service Details", true, true);
                //}
                //{TotalInWords}
                document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2") + " " + dtOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord((materialTotal + serviceTotal), dtOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();

                StringCollection strColDistinct = new StringCollection();

                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());             //For Same Name Use
                    string text = strReplace[i].ToUpper();

                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }

        public DataTable loadIssueRequestMaster(string issueId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IR.Id grnNumber
                                ,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,ir.PODepended 
								,IR.POId PONumber                                 
                                ,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS PODate
                                ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
		                        ,IR.InvoicingPartyPlantId
		                       
		                             ,INVPARTYPL.UserName InvoicingPartyName
                                                ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                                                ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                                                ,ISNULL(IR.InvoicingByAddress,'') InvoicingByAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
		                        ,IR.DeliveryPartyPlantId		
		                        ,IOM.MaterialMasterId
		                        ,IR.DocRefNo
		                        ,IR.DocDate
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
		                        ,IR.IsApproved
		                        ,IR.PartyType
		                        ,Party.UserName VendorName
                                ,Party.AddressMasterId VendorAddressMasterId
                                ,Party.TINNO VendorGSTIN
                                ,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
		                        ,IR.IsNonCreditable
		                        ,IR.CurrencyId
	                            ,CRNC.Code AS CurrencyName
	                            ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
		                        ,PayTerm.UserName PaymentTerm
	                          ,MM.UserName MaterialMaster
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IOM.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS FirstCharacteristicsValue
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRD.TransactionQty, 2) POTransactionQty
	                          ,ROUND(IRD.MaterialTranRate, 2) TransactionRate
	                          ,ROUND((IRD.TransactionQty * IRD.MaterialTranRate), 2) AS TrnAmount
	                          ,IRD.TotalMaterialTranAmount BaseAmount
	                          ,IRD.TotalTaxAmount AS BaseTaxAmount
	                          ,TaxAmount = (
		                            SELECT SUM(TaxAmount)
		                            FROM [TRN].[PurchaseOrderTax]
		                            WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
		                            SELECT SUM(TotalTaxAmount)
		                            FROM [TRN].[POService]
		                            WHERE InventoryReceiveId = IOM.Id
		                            )
	                          ,IRD.ChargesTranAmount
	                          ,IRD.CountryId

	                          ,IRD.TransactionUoMId
	                          ,TUoM.UserName AS TransactionUoM
                              ,IRD.Id InventoryReceiveDetailId
                              FROM TRN.InventoryReceive IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.BaseCurrencyId
                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId 
                         LEFT JOIN trn.inventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId
						 LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId                                        
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                         WHERE IR.Id ='" + issueId + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }



        

        public double makeIssueDetailsTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{IssueSlipDetails}";

            DataTable dsOrderItems, dsTax;

            //dsOrderItems = loadOrderMasterItems(grnId);
            //dsTax = loadOrderMasterTax(grnId);

            int LasColumnIndex = 8;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            //DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            //if (dv.Count > 0)
            //{
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        LasColumnIndex++;
            //        dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
            //        LasColumnIndex++;
            //    }
            //}


            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            //wTable.Title = "Material Details";
            //wTable.Description = "This table shows the price details of PI";
            //wTable.IndentFromLeft = 10;


            //string UOM = dsOrderMaster.Tables[0].Rows[0]["UOM"].ToString();
            //string Currency = dsOrderMaster.Tables[0].Rows[0]["Currency"].ToString();
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Char1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Char2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Char3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL;


            int colTotalTaxableAmount = COL;
            //if (dv.Count > 0)
            //{
            //    COL++;
            //    colTotalTaxableAmount = COL;
            //    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
            //    range.ApplyCharacterFormat(FontBold);
            //    //COL++;
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        //two columns required for tax
            //        COL++;
            //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
            //        range.ApplyCharacterFormat(FontBold);

            //        COL++;
            //        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            //    }
            //}
            //else
            //{
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Value");
            //}


            wTable.Rows.Add(TemplateRow);
            ROW++;

            //if (dv.Count > 0)
            //{
            //    for (int i = 0; i < dv.Count; i++)
            //    {

            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
            //        range.ApplyCharacterFormat(FontBold);
            //        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
            //        range.ApplyCharacterFormat(FontBold);

            //    }
            //}
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("F2"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString().ToString());

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("F2"));

                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString());

                //if (dv.Count > 0)
                //{
                //    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                //    //double totalTax = 0;

                //    for (int T = 0; T < dv.Count; T++)
                //    {
                //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                //        if (dvtax.Count > 0)
                //        {
                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));

                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));

                //        }
                //    }
                //}
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colRate || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || C == colQty || C == colRate || C == colUoM || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString());
                //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                //+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable

            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }

            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return total;
        }

       
        public DataTable loadIssueMasterItems(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
                                  from [TRN].[MasterOrderItem] T
                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
                                where MasterOrderId='" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }


        #endregion

    }
}
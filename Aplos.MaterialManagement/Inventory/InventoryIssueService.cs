using Aplos.MaterialManagement.MaterialQuery;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Materials;
using Library.Model.Organizations;
using Library.Model.Products;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.SalesManagements;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.MaterialManagement.Inventory
{
    public class InventoryIssueService : Service<InventoryIssue>, IInventoryIssueService
    {
        #region Constructor

        private readonly IRepositoryAsync<InventoryIssue> _issueRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;
        private readonly IInventoryIssueDetailService _issueDetailService;
        private readonly IInventoryMaterialService _inventoryMaterialService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVoucherService _voucherService;
        private readonly IRepositoryAsync<Voucher> _voucherRepository;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<VoucherDetailCurrency> _voucherDetailCurrencyRepository;

        private readonly IRepositoryAsync<InventoryMaterial> _inventoryMaterialRepository;
        private readonly IRepositoryAsync<InventoryReceiveDetail> _receiveDetailRepository;
        private readonly IRepositoryAsync<InventoryIssueHistory> _issueHistoryRepository;
        private readonly IRepositoryAsync<InventoryIssueHistoryBOQ> _issueHistoryBOQRepository;
        private readonly IRepositoryAsync<RequisitionIssueDetail> _requisitionIssueDetailRepository;
        private readonly IRepositoryAsync<InventoryIssueReturn> _InventoryIssueReturnRepository;
        private readonly IRepositoryAsync<InventoryIssueReturnHistory> _InventoryIssueReturnHistoryRepository;

        private readonly IRepositoryAsync<PhysicalStockAdjustmentMaster> _PhysicalStockAdjustmentMasterRepository;
        private readonly IRepositoryAsync<PhysicalStockAdjustmentDetail> _PhysicalStockAdjustmentDetailRepository;
        private readonly IRepositoryAsync<PhysicalStockAdjustmentHistory> _PhysicalStockAdjustmentHistoryRepository;

        private readonly IRepositoryAsync<InventorySales> _InventorySalesRepository;
        private readonly IRepositoryAsync<InventorySalesDetail> _InventorySalesDetailRepository;
        private readonly IRepositoryAsync<InventorySalesHistory> _InventorySalesHistoryRepository;
        private readonly IRepositoryAsync<InventorySalesTax> _InventorySalesTaxRepository;
        private readonly IRepositoryAsync<InventorySalesService> _InventorySalesServiceRepository;


        private readonly IRepositoryAsync<InventoryScrap> _InventoryScrapRepository;
        private readonly IRepositoryAsync<InventoryScrapDetail> _InventoryScrapDetailRepository;
        private readonly IRepositoryAsync<InventoryScrapHistory> _InventoryScrapHistoryRepository;
        private readonly IRepositoryAsync<InventoryReceive> _InventoryReceiveRepository;

        private readonly IRepositoryAsync<InventoryTransferHistory> _InventoryTransferHistoryRepository;
        private readonly IRepositoryAsync<IssueDetailAndIssueRequestMap> _IssueDetailAndIssueRequestMapRepository;

        private readonly IRepositoryAsync<InventorySalesReturn> _InventorySalesReturnRepository;
        private readonly IRepositoryAsync<InventorySalesReturnDetail> _InventorySalesReturnDetailRepository;

        private readonly IRepositoryAsync<InventorySalesReturnTax> _InventorySalesReturnTaxRepository;
        private readonly IRepositoryAsync<InventorySalesReturnService> _InventorySalesReturnServiceRepository;
        private readonly IInventoryReceiveService _inventoryReveiveService;
        private readonly IRepositoryAsync<InventoryReceiveTax> _receiveTaxRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        public InventoryIssueService(
            IRepositoryAsync<InventoryIssue> issueRepository
            , IRepositoryAsync<Company> companyRepository
            , IInventoryIssueDetailService issueDetailService
            , IInventoryMaterialService inventoryMaterialService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IVoucherService voucherService
            , IRepositoryAsync<Voucher> voucherRepository
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IRepositoryAsync<VoucherDetailCurrency> voucherDetailCurrencyRepository
            , IRepositoryAsync<InventoryIssueDetail> issueDetailRepository
            , IRepositoryAsync<InventoryMaterial> inventoryMaterialRepository
            , IRepositoryAsync<InventoryReceiveDetail> receiveDetailRepository
            , IRepositoryAsync<InventoryIssueHistory> issueHistoryRepository
            , IRepositoryAsync<InventoryIssueHistoryBOQ> issueHistoryRepositoryBOQ
            , IRepositoryAsync<RequisitionIssueDetail> requisitionIssueDetailRepository
            , IRepositoryAsync<InventoryIssueReturn> InventoryIssueReturnRepository
            , IRepositoryAsync<InventoryIssueReturnHistory> InventoryIssueReturnHistoryRepository
            , IRepositoryAsync<PhysicalStockAdjustmentMaster> PhysicalStockAdjustmentMasterRepository
            , IRepositoryAsync<PhysicalStockAdjustmentDetail> PhysicalStockAdjustmentDetailRepository
            , IRepositoryAsync<PhysicalStockAdjustmentHistory> PhysicalStockAdjustmentHistoryRepository

            , IRepositoryAsync<InventorySales> InventorySalesRepository
            , IRepositoryAsync<InventorySalesDetail> InventorySalesDetailRepository
            , IRepositoryAsync<InventorySalesHistory> InventorySalesHistoryRepository
            , IRepositoryAsync<InventorySalesTax> InventorySalesTaxRepository
            , IRepositoryAsync<InventorySalesService> InventorySalesServiceRepository
            , IRepositoryAsync<InventoryScrap> InventoryScrapRepository
            , IRepositoryAsync<InventoryScrapDetail> InventoryScrapDetailRepository
            , IRepositoryAsync<InventoryScrapHistory> InventoryScrapHistoryRepository
            , IRepositoryAsync<InventoryReceive> InventoryReceiveRepository
            , IRepositoryAsync<InventoryTransferHistory> InventoryTransferHistoryRepository
            , IRepositoryAsync<IssueDetailAndIssueRequestMap> IssueDetailAndIssueRequestMapRepository
            , IRepositoryAsync<InventorySalesReturn> InventorySalesReturnRepository
            , IRepositoryAsync<InventorySalesReturnDetail> InventorySalesReturnDetailRepository

            , IRepositoryAsync<InventorySalesReturnTax> InventorySalesReturnTaxRepository
            , IRepositoryAsync<InventorySalesReturnService> InventorySalesReturnServiceRepository
            , IInventoryReceiveService inventoryReveiveService
            , IRepositoryAsync<InventoryReceiveTax> receiveTaxRepository
            ) : base(issueRepository, unitOfWork, pkGeneratorService)
        {
            _issueRepository = issueRepository;
            _issueDetailService = issueDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _unitOfWork = unitOfWork;
            _companyRepository = companyRepository;
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
            _voucherRepository = voucherRepository;
            _voucherDetailRepository = voucherDetailRepository;
            _voucherDetailCurrencyRepository = voucherDetailCurrencyRepository;
            _issueHistoryRepository = issueHistoryRepository;
            _issueHistoryBOQRepository = issueHistoryRepositoryBOQ;
            _requisitionIssueDetailRepository = requisitionIssueDetailRepository;
            _receiveDetailRepository = receiveDetailRepository;
            _inventoryMaterialRepository = inventoryMaterialRepository;
            _InventoryIssueReturnRepository = InventoryIssueReturnRepository;
            _InventoryIssueReturnHistoryRepository = InventoryIssueReturnHistoryRepository;
            _PhysicalStockAdjustmentMasterRepository = PhysicalStockAdjustmentMasterRepository;
            _PhysicalStockAdjustmentDetailRepository = PhysicalStockAdjustmentDetailRepository;
            _PhysicalStockAdjustmentHistoryRepository = PhysicalStockAdjustmentHistoryRepository;
            _InventorySalesRepository = InventorySalesRepository;
            _InventorySalesDetailRepository = InventorySalesDetailRepository;
            _InventorySalesHistoryRepository = InventorySalesHistoryRepository;
            _InventorySalesTaxRepository = InventorySalesTaxRepository;
            _InventorySalesServiceRepository = InventorySalesServiceRepository;

            _InventoryScrapRepository = InventoryScrapRepository;
            _InventoryScrapDetailRepository = InventoryScrapDetailRepository;
            _InventoryScrapHistoryRepository = InventoryScrapHistoryRepository;
            _InventoryReceiveRepository = InventoryReceiveRepository;
            _InventoryTransferHistoryRepository = InventoryTransferHistoryRepository;
            _IssueDetailAndIssueRequestMapRepository = IssueDetailAndIssueRequestMapRepository;

            _InventorySalesReturnRepository = InventorySalesReturnRepository;
            _InventorySalesReturnDetailRepository = InventorySalesReturnDetailRepository;
            _InventorySalesReturnServiceRepository = InventorySalesReturnServiceRepository;
            _InventorySalesReturnTaxRepository = InventorySalesReturnTaxRepository;
            _inventoryReveiveService = inventoryReveiveService;
            _pkGeneratorService = pkGeneratorService;
            _receiveTaxRepository = receiveTaxRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return base.GetAutoNumber(nameof(InventoryIssue), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetPK3()
        {
            return base.GetAutoNumber(nameof(InventorySales), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetPK2()
        {
            return base.GetAutoNumber(nameof(PhysicalStockAdjustmentMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetPK1()
        {
            return base.GetAutoNumber(nameof(InventoryIssueReturn), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetPK4()
        {
            return base.GetAutoNumber(nameof(InventoryScrap), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetInventorySalesTaxPK()
        {
            return base.GetAutoNumber(nameof(InventorySalesTax), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetInventorySalesReturnPK()
        {
            return base.GetAutoNumber(nameof(InventoryIssueReturn), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetIssueDetailAndIssueRequestMapPK()
        {
            return base.GetAutoNumber(nameof(IssueDetailAndIssueRequestMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public void InsertGraph(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll)
        {
            var flag = false;
            bool FlagIsAsset = false;
            if (IssueTypeStatus.ToString() == "Inventory")
            {
                FlagIsAsset = false;
            }
            else
            {
                FlagIsAsset = true;
            }
            try
            {


                var GRNCalculateList = new List<InventoryIssueHistory>();
                if (entities.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var _pk = GetPK();
                    var inventoryMaterialList = _inventoryMaterialService.GetInventoryMaterialListByUpToSku(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                    var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
                    foreach (var item in entities)// update view model (inventory material field)
                    {
                        var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                //&& t.FirstCharacteristicsId == item.FirstCharacteristicsId 
                                && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                //&& t.SecondCharacteristicsId == item.SecondCharacteristicsId 
                                && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                                //&& t.ThirdCharacteristicsId == item.ThirdCharacteristicsId 
                                && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                                && t.CountryId == item.CountryId
                                && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId // && t.CountryId == item.CountryId
                               );
                        if (im.IsNotNull())
                        {

                            //if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                            item.InventoryIssueId = _pk;
                            item.InventoryMaterialId = im.Id;
                            item.CompanyGroupId = im.CompanyGroupId;
                            item.CompanyId = inventoryIssue.CompanyId;
                            item.PlantId = inventoryIssue.PlantId;
                            item.CurrencyId = currencyId;
                            item.MaterialStorageId = null;
                            item.MaterialMasterId = im.MaterialMasterId;
                            item.ArticleId = im.ArticleId;
                            item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                            item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                            item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                            item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                            item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                            item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                            item.TotalQty = im.TotalQty;
                            item.AvgRate = im.AvgRate;

                        }
                    }// update view model (inventory material field)
                    inventoryIssue.CurrencyId = currencyId;
                    inventoryIssue.ProductionOrderId = inventoryIssue.ProductionOrderId;
                    inventoryIssue.ContractId = inventoryIssue.ContractId;
                    inventoryIssue.OrderRefNo = inventoryIssue.OrderRefNo;


                    inventoryIssue.Id = _pk;
                    InsertGraph(inventoryIssue);
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    //_issueDetailService.InsertRange(entities, specificStockList, inventoryIssue);


                    #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======
                    try
                    {

                        var uiList = entities.ToList();
                        var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                        var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                        var specificInvaterialIds = new string[] { };
                        var maIds = new string[] { };
                        if (specificStockList.IsNotNull())
                        {
                            specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                            maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                            for (int i = uiList.Count() - 1; i >= 0; i--)
                            {
                                var row = uiList.ElementAt(i);
                                if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                    uiList.RemoveAt(i);
                            }
                        }
                        else maIds = inventoryMaterialIds;
                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty,IRD.InventorySalesQty,IRD.InventoryScrapQty,IRD.PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,IRD.ReductionByAdjustmentQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
									      AND IR.Status='Posting' 
										  AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.IssueDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                        //if (receiveDetailList.IsNotNull())
                        if (specificStockList.IsNull())
                        {
                            foreach (var issue in uiList)
                            {

                                var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;
                                /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* (TotalMmaterialTrnAmount/MaterialTrnRate))*/
                                /*Rate= Amount/Sum GRN Qty */

                                if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                                    //input.BaseRate = receiveDetailRow.BaseAmount / receiveDetailRow.BaseQty;
                                    issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                if (issue.TransactionUoMId != issue.BaseUOMId)
                                    issue.BaseQty = Convert.ToDecimal(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);

                                decimal IssueTransactionQty = issue.TransactionQty;
                                foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                {

                                    if (IssueTransactionQty <= 0)
                                        break;

                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(IIH.TotalAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(ISH.TotalBaseAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                    //																						   FROM trn.InventoryReceiveDetail IRD  
                                    //																							left JOIN [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                    //																						   WHERE  IIH.InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                    decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;


                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }

                                    //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                    //if (item.TransactionUoMId == issue.TransactionUoMId)
                                    if (item.BaseUOMId == issue.TransactionUoMId)
                                    {

                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.BaseQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty

                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                    }
                                    else
                                    {
                                        //detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty+ item.InventoryTransferQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialTranAmount / item.BaseQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            //TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialTranAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor);

                                    }
                                    //}
                                }

                                if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                currentId++;
                                //totalGRNQty = issue.TransactionQty;
                                if (issue.BaseQty == null)
                                    issue.BaseQty = totalGRNQty;
                                var detail = new InventoryIssueDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    InventoryIssueId = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,
                                                          //InventoryIssue = inventoryIssue,
                                    InventoryMaterialId = issue.InventoryMaterialId,
                                    TransactionQty = totalGRNQty,//issue.TransactionQty,
                                    BaseQty = issue.BaseQty,
                                    BaseUOMId = issue.BaseUOMId,
                                    TransactionUoMId = issue.TransactionUoMId,

                                    //TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                    AvgRate = Math.Round(issue.AvgRate, 4),
                                    AvgAmount = Math.Round((issue.TransactionQty * issue.AvgRate), 2),
                                    Policy = receiveDetailRow.Policy,


                                    PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
                                    PolicyAmount = Math.Round(detailtrnAmount, 2),

                                    //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                                    //PolicyRate = detailtrnAmount / totalGRNQty,
                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    Comments = issue.Comments,
                                    CostCenterId = issue.CostCenterId,
                                    ModelState = ModelState.Added

                                    //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                                    //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                                };
                                var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{detail.Id}'").First();
                                // single entry (history)
                                //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                //if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                {
                                    historyId++;
                                    var history = new InventoryIssueHistory
                                    {
                                        Id = MakePK(detail.Id, historyId, 2),
                                        InventoryIssueDetailId = detail.Id,
                                        InventoryReceiveDetailId = receiveDetailRow.Id,
                                        Qty = SelectedGRN.Qty,
                                        //Rate = Math.Round(Convert.ToDecimal(issue.BaseRate),4),
                                        //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                        //Rate = detailtrnAmount / totalGRNQty,
                                        //TotalAmount = Math.Round((Convert.ToDecimal(issue.BaseRate)* SelectedGRN.Qty),2),
                                        Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                        TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                        IsCapitalize = false,
                                        IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                        IssueReturnQty = 0,
                                        //BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BooksCurrencyBaseRate), 4),
                                        //TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * issue.BooksCurrencyBaseRate), 2)
                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BaseRate), 4),
                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * Math.Round(Convert.ToDecimal(issue.BaseRate), 4)), 2)
                                    };
                                    //detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                                    //detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);

                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(SelectedGRN.Qty)) + @"'
									 , BaseIssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(SelectedGRN.Qty)) + "' WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";//issue.TransactionQty
                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _issueHistoryRepository.Insert(history);


                                }
                                // multiple entry (history)
                                else
                                {
                                    var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                    var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                               - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                                    //if (tqty < input.TransactionQty) throw new CustomException("Stock 0");
                                    if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                    decimal policyAmount = 0;
                                    //decimal qtyDifference = input.TransactionQty;
                                    decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                    foreach (var item in rdList)
                                    {
                                        historyId++;
                                        if (item.TransactionUoMId != item.BaseUOMId)
                                            //input.BaseRate = item.BaseAmount / item.BaseQty;
                                            issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                        //else input.BaseRate = item.TransactionRate;
                                        else issue.BaseRate = item.MaterialTranRate;

                                        //var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty);
                                        var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty - item.PurchaseReturnQty - item.ReductionByAdjustmentQty - item.InventorySalesQty - item.InventoryScrapQty) + Convert.ToDecimal(item.IssueReturnQty);
                                        // (10 - 3)//Issueable Qty
                                        //if (issueQty != 0)
                                        //{

                                        if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                        {
                                            policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                            qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                        }
                                        else // (6 < 7) (qtyDifference < issueQty)
                                        {
                                            //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                            policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                            qtyDifference = 0;
                                        }
                                        SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                        var history = new InventoryIssueHistory
                                        {
                                            Id = MakePK(detail.Id, historyId, 2),
                                            InventoryIssueDetailId = detail.Id,
                                            InventoryReceiveDetailId = item.Id,
                                            Qty = SelectedGRN.Qty,//Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO
                                                                  //Qty = Convert.ToDecimal(issueQty),//TODO
                                                                  // Qty = Convert.ToDecimal(qtyDifference),//TODO
                                                                  //Rate = Convert.ToInt32(issue.BaseRate),
                                                                  //Rate = Convert.ToDecimal(issue.BaseRate),
                                                                  //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                                  //Rate = detailtrnAmount / totalGRNQty,
                                                                  //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                            Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                            //Rate = Math.Round(Convert.ToDecimal(issue.BaseRate),4),
                                            TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                                                                 //TotalAmount = Math.Round((Convert.ToDecimal(issue.BaseRate)* SelectedGRN.Qty),2),//Convert.ToDecimal(detailtrnAmount),
                                            IsCapitalize = false,
                                            IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                            // BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                            //TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * item.BooksCurrencyBaseRate), 2)
                                            BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BaseRate), 4),
                                            TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * Math.Round(Convert.ToDecimal(issue.BaseRate), 4)), 2)
                                        };

                                        AuditService.AddedLog(history);
                                        _issueHistoryRepository.Insert(history);

                                        builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET BaseIssueQty='" + Convert.ToDecimal(issueQty) + "',IssueQty='" + Convert.ToDecimal(issueQty) + "'  WHERE Id='" + item.Id + "'";
                                        rdBuilder.Append(builderSql);
                                        if (qtyDifference == 0)
                                            break;
                                        //}
                                    }

                                    //detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                                    //detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                                }
                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - issue.BaseQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";//issue.TransactionQty
                                rdBuilder.Append(builderSql);
                                AuditService.AddedLog(detail);
                                _issueDetailService.InsertGraph(detail);

                                foreach (var itemAll in entitiesAll)
                                {
                                    //Mapping Data=========================================================
                                    var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + itemAll.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                    if (receiveDetailList1.IsNotNull())
                                    {
                                        bool isQtyAlocated = true;
                                        decimal temp = 0;
                                        int count = 0;
                                        foreach (var receiveDetailListNew in receiveDetailList1)
                                        {


                                            count++;
                                            if (count == 1)
                                            {
                                                if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > detail.TransactionQty)
                                                {

                                                    detail.TransactionQty = detail.TransactionQty;
                                                    //temp += itemDetail.TransactionQty;
                                                    isQtyAlocated = false;

                                                }
                                                else if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < detail.TransactionQty)
                                                {
                                                    //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                    temp = (detail.TransactionQty - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                    detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                    isQtyAlocated = true;

                                                }
                                                else
                                                {
                                                    //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                    detail.TransactionQty = detail.TransactionQty;
                                                    isQtyAlocated = true;

                                                }
                                            }
                                            if (count > 1)
                                            {
                                                if (isQtyAlocated == true)
                                                {
                                                    if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > temp)
                                                    {
                                                        //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                        detail.TransactionQty = detail.TransactionQty;
                                                        isQtyAlocated = false;
                                                    }
                                                    if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < temp)
                                                    {
                                                        //temp = temp - issue.TransactionQtyForPO;
                                                        temp = (temp - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                        //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                        detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                        isQtyAlocated = true;
                                                    }
                                                    else
                                                    {
                                                        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                        detail.TransactionQty = temp;
                                                        isQtyAlocated = true;

                                                    }

                                                }
                                                else
                                                {
                                                    detail.TransactionQty = 0;
                                                }
                                            }


                                            var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                            {
                                                Id = GetIssueDetailAndIssueRequestMapPK(),
                                                InventoryIssueDetailId = detail.Id,
                                                IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                Qty = detail.TransactionQty
                                                //AutoAllocate = true

                                            };
                                            AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                            _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                        }
                                    }

                                    //===================

                                }



                            }

                        }
                        if (specificStockList.IsNotNull())
                        {


                            foreach (var invMaterialId in specificInvaterialIds)
                            {
                                var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                var totalReqQty = 0M;

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;
                                /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* TrnRate)*/
                                /*Rate= Amount/Sum GRN Qty */

                                foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                {
                                    if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");
                                    decimal IssueTransactionQty = item.RequisitionQty;
                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																														FROM (
																																SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD
																																left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalMaterialBooksCurrencyAmount,0) IIHTotalAmount,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																																UNION All
																																SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																														)x
																														WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;


                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }

                                    if (item.TransactionUoMId == item.BaseUOMId) //entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault())
                                    {
                                        // detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                        if (RemainingGRNQty == IssueTransactionQty)
                                        {
                                            detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount + item.AdditionalChargesAmount - totalIssuedAmount);
                                        }
                                        else
                                        {
                                            detailtrnAmount += Math.Round(Convert.ToDecimal(item.RequisitionQty * (item.BooksCurrencyBaseRate)), 4);
                                        }
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = (RemainingGRNQty == IssueTransactionQty) ? Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount + item.AdditionalChargesAmount - totalIssuedAmount) : Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate),
                                            //TotalAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);

                                        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                    }
                                    else
                                    {
                                        //detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        if (RemainingGRNQty == IssueTransactionQty)
                                        {
                                            detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount + item.AdditionalChargesAmount - totalIssuedAmount);
                                        }
                                        else
                                        {
                                            detailtrnAmount += Math.Round(Convert.ToDecimal(item.RequisitionQty * (item.TrnCurrencyBaseRate*item.BaseUoMFactor)), 4);
                                        }
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = (RemainingGRNQty == IssueTransactionQty) ? Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount + item.AdditionalChargesAmount - totalIssuedAmount) : Math.Round(Convert.ToDecimal((item.RequisitionQty * item.BooksCurrencyBaseRate) * item.BaseUoMFactor), 4) * Convert.ToDecimal((item.GRNBaseUoMFactor / item.BaseUoMFactor)),
                                            //TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            //TotalAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty * item.BaseUoMFactor);
                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor);
                                    }
                                    item.IssueRequest = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.IssueRequest).FirstOrDefault();
                                }

                                currentId++;
                                var issueDetail = new InventoryIssueDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    InventoryIssueId = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,
                                                          //InventoryIssue = inventoryIssue,
                                    InventoryMaterialId = invMaterialId,
                                    BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                    TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                                    AvgRate = Math.Round(Convert.ToDecimal(invMaterial.AvgRate), 4),
                                    Policy = "N/A",

                                    TransactionQty = stockList.Sum(r => r.RequisitionQty),//Math.Round(Convert.ToDecimal(totalGRNQty), 2), //stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),

                                    PolicyRate = Math.Round((Convert.ToDecimal(detailtrnAmount / stockList.Sum(r => r.RequisitionQty))), 4),
                                    PolicyAmount = Math.Round(Convert.ToDecimal(detailtrnAmount), 2),
                                    BaseQty = Math.Round(Convert.ToDecimal(totalGRNQty), 2),//stockList.Sum(r => r.RequisitionQty),
                                    AvgAmount = Math.Round((Convert.ToDecimal(totalGRNQty * invMaterial.AvgRate)), 2),
                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
                                    Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),
                                    ModelState = ModelState.Added
                                };

                                var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                                foreach (var item in stockList)
                                {
                                    var historyTotalCal = 0M;
                                    if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                                    if (item.TransactionUoMId != item.BaseUOMId)
                                    {
                                        totalReqQty = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BaseUoMFactor), 4);
                                        historyTotalCal = Math.Round(Convert.ToDecimal(totalReqQty * Convert.ToDecimal(item.BooksCurrencyBaseRate)), 2) * Convert.ToDecimal((item.GRNBaseUoMFactor / item.BaseUoMFactor));

                                    }
                                    else
                                    {
                                        totalReqQty = Math.Round(item.RequisitionQty, 4);
                                        historyTotalCal=Math.Round(Convert.ToDecimal(totalReqQty * Convert.ToDecimal(item.BooksCurrencyBaseRate)), 2);
                                    }
                                    historyId++;
                                    var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                    var history = new InventoryIssueHistory
                                    {
                                        Id = MakePK(issueDetail.Id, historyId, 2),
                                        InventoryIssueDetailId = issueDetail.Id,
                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                        MaterialStorageId=string.IsNullOrEmpty(item.MaterialStorageId)? inventoryIssue.MaterialStorageId: item.MaterialStorageId,
                                        Qty = Math.Round(totalReqQty, 4), //item.RequisitionQty,
                                                                          //Rate = Convert.ToDecimal(item.BaseRate),
                                        Rate = Math.Round((SelectedGRN.TotalAmount / totalReqQty), 4),//totalGRNQty
                                        TotalAmount = (item.BaseQty == (item.IssueQty + item.RequisitionQty - item.IssueReturnQty)) ? Math.Round(Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount + item.AdditionalChargesAmount - item.TotalIssueAmount + item.TotalIssueReturnAmount), 2) : Math.Round(historyTotalCal, 2),//Convert.ToDecimal(detailtrnAmount),
                                        IssueRequestDetailId = item.IssueRequest,
                                        IssueReturnQty = 0,
                                        BooksCurrencyBaseRate = (item.TransactionUoMId == item.BaseUOMId)? Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4) : Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4) * Convert.ToDecimal((item.BaseUoMFactor / item.GRNBaseUoMFactor)),
                                        TotalMaterialBooksCurrencyAmount = (item.BaseQty == (item.IssueQty + item.RequisitionQty-item.IssueReturnQty)) ? Math.Round(Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount + item.AdditionalChargesAmount - item.TotalIssueAmount+item.TotalIssueReturnAmount), 2) : Math.Round(historyTotalCal, 2)//totalReqQty item.RequisitionQty
                                    };
                                    //policyAmmount += history.Qty * history.Rate;



                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' ,IssueReturnQty='" + Convert.ToDecimal(item.IssueReturnQty) + @"'
										,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _issueHistoryRepository.Insert(history);
                                    //Mapping Data=========================================================
                                    if (entitiesAll.IsNotNull())
                                    {
                                        foreach (var itemall in entitiesAll.Where(q => q.MaterialMasterId == item.MaterialMasterId && q.ArticleId == item.ArticleId && q.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId && q.SecondCharacteristicsId == item.SecondCharacteristicsValueId))
                                        {
                                            var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + itemall.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                            if (receiveDetailList1.IsNotNull())
                                            {
                                                foreach (var receiveDetailListNew in receiveDetailList1)
                                                {
                                                    var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                    {
                                                        Id = GetIssueDetailAndIssueRequestMapPK(),
                                                        InventoryIssueDetailId = issueDetail.Id,
                                                        IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                        Qty = Convert.ToDecimal(issueDetail.BaseQty),
                                                        //AutoAllocate = true

                                                    };
                                                    AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                    _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                                }
                                            }
                                        }
                                    }
                                }

                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";//
                                rdBuilder.Append(builderSql);

                                AuditService.AddedLog(issueDetail);
                                _issueDetailService.InsertGraph(issueDetail);
                            }
                        }

                    }
                    catch (CustomException)
                    {
                        throw;
                    }
                    #endregion

                    _unitOfWork.SaveChanges();
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    flag = false;
                    _unitOfWork.Commit();
                }
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

        public void UpdateIssueMaster(InventoryIssue inventoryIssue)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var builder = new System.Text.StringBuilder();
                var sql = "";
                sql = @"UPDATE  [TRN].[InventoryIssue] set IssueType='"+ inventoryIssue.IssueType + "' WHERE Id='" + inventoryIssue.Id + "'";
                builder.Append(sql);
                
                _sqlRepository.ExecuteSqlCommand(builder.ToString());
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

        public void InsertGraphBOQ(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll, List<InventoryIssueHistoryBOQ> BoqAllocationListVM)
        {
            var flag = false;
            bool FlagIsAsset = false;
            if (IssueTypeStatus.ToString() == "Inventory")
            {
                FlagIsAsset = false;
            }
            else
            {
                FlagIsAsset = true;
            }
            try
            {


                var GRNCalculateList = new List<InventoryIssueHistory>();
                if (entities.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var _pk = GetPK();
                    var inventoryMaterialList = _inventoryMaterialService.GetInventoryMaterialListByUpToSku(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                    var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
                    foreach (var item in entities)// update view model (inventory material field)
                    {
                        var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                //&& t.FirstCharacteristicsId == item.FirstCharacteristicsId 
                                && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                //&& t.SecondCharacteristicsId == item.SecondCharacteristicsId 
                                && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                                //&& t.ThirdCharacteristicsId == item.ThirdCharacteristicsId 
                                && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                                && t.CountryId == item.CountryId
                                && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId // && t.CountryId == item.CountryId
                               );
                        if (im.IsNotNull())
                        {

                            //if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                            item.InventoryIssueId = _pk;
                            item.InventoryMaterialId = im.Id;
                            item.CompanyGroupId = im.CompanyGroupId;
                            item.CompanyId = inventoryIssue.CompanyId;
                            item.PlantId = inventoryIssue.PlantId;
                            item.CurrencyId = currencyId;
                            item.MaterialStorageId = null;
                            item.MaterialMasterId = im.MaterialMasterId;
                            item.ArticleId = im.ArticleId;
                            item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                            item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                            item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                            item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                            item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                            item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                            item.TotalQty = im.TotalQty;
                            item.AvgRate = im.AvgRate;

                        }
                    }// update view model (inventory material field)
                    inventoryIssue.CurrencyId = currencyId;
                    inventoryIssue.ProductionOrderId = inventoryIssue.ProductionOrderId;
                    inventoryIssue.ContractId = inventoryIssue.ContractId;
                    inventoryIssue.OrderRefNo = inventoryIssue.OrderRefNo;
                    inventoryIssue.Types = "InventoryBOQIssue";

                    inventoryIssue.Id = _pk;
                    InsertGraph(inventoryIssue);
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    //_issueDetailService.InsertRange(entities, specificStockList, inventoryIssue);


                    #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======
                    try
                    {

                        var uiList = entities.ToList();
                        var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                        var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                        var specificInvaterialIds = new string[] { };
                        var maIds = new string[] { };
                        if (specificStockList.IsNotNull())
                        {
                            specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                            maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                            for (int i = uiList.Count() - 1; i >= 0; i--)
                            {
                                var row = uiList.ElementAt(i);
                                if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                    uiList.RemoveAt(i);
                            }
                        }
                        else maIds = inventoryMaterialIds;
                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty,IRD.InventorySalesQty,IRD.InventoryScrapQty,IRD.PurchaseReturnQty,IRD.IssueReturnQty,IRD.ReductionByAdjustmentQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
									      AND IR.Status='Posting' 
										  AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.IssueDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                        //if (receiveDetailList.IsNotNull())
                        if (specificStockList.IsNull())
                        {
                            foreach (var issue in uiList)
                            {

                                var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;
                                /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* (TotalMmaterialTrnAmount/MaterialTrnRate))*/
                                /*Rate= Amount/Sum GRN Qty */

                                if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                                    //input.BaseRate = receiveDetailRow.BaseAmount / receiveDetailRow.BaseQty;
                                    issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                if (issue.TransactionUoMId != issue.BaseUOMId)
                                    issue.BaseQty = Convert.ToDecimal(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);

                                decimal IssueTransactionQty = issue.TransactionQty;
                                foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                {

                                    if (IssueTransactionQty <= 0)
                                        break;

                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(IIH.TotalAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(ISH.TotalBaseAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                    //																						   FROM trn.InventoryReceiveDetail IRD  
                                    //																							left JOIN [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                    //																						   WHERE  IIH.InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                    decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;


                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }

                                    //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                    //if (item.TransactionUoMId == issue.TransactionUoMId)
                                    if (item.BaseUOMId == issue.TransactionUoMId)
                                    {

                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.BaseQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty

                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                    }
                                    else
                                    {
                                        //detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty+ item.InventoryTransferQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialTranAmount / item.BaseQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            //TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialTranAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor);

                                    }
                                    //}
                                }

                                if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                currentId++;
                                //totalGRNQty = issue.TransactionQty;
                                if (issue.BaseQty == null)
                                    issue.BaseQty = totalGRNQty;
                                var detail = new InventoryIssueDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    InventoryIssueId = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,
                                                          //InventoryIssue = inventoryIssue,
                                    InventoryMaterialId = issue.InventoryMaterialId,
                                    TransactionQty = totalGRNQty,//issue.TransactionQty,
                                    BaseQty = issue.BaseQty,
                                    BaseUOMId = issue.BaseUOMId,
                                    TransactionUoMId = issue.TransactionUoMId,

                                    //TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                    AvgRate = Math.Round(issue.AvgRate, 4),
                                    AvgAmount = Math.Round((issue.TransactionQty * issue.AvgRate), 2),
                                    Policy = receiveDetailRow.Policy,


                                    PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
                                    PolicyAmount = Math.Round(detailtrnAmount, 2),

                                    //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                                    //PolicyRate = detailtrnAmount / totalGRNQty,
                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    Comments = issue.Comments,
                                    CostCenterId = issue.CostCenterId,
                                    ModelState = ModelState.Added

                                    //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                                    //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                                };
                                var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{detail.Id}'").First();
                                // single entry (history)
                                //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                //if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                {
                                    historyId++;
                                    var history = new InventoryIssueHistory
                                    {
                                        Id = MakePK(detail.Id, historyId, 2),
                                        InventoryIssueDetailId = detail.Id,
                                        InventoryReceiveDetailId = receiveDetailRow.Id,
                                        Qty = SelectedGRN.Qty,
                                        //Rate = Math.Round(Convert.ToDecimal(issue.BaseRate),4),
                                        //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                        //Rate = detailtrnAmount / totalGRNQty,
                                        //TotalAmount = Math.Round((Convert.ToDecimal(issue.BaseRate)* SelectedGRN.Qty),2),
                                        Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                        TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                        IsCapitalize = false,
                                        IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                        IssueReturnQty = 0,
                                        //BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BooksCurrencyBaseRate), 4),
                                        //TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * issue.BooksCurrencyBaseRate), 2)
                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BaseRate), 4),
                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * Math.Round(Convert.ToDecimal(issue.BaseRate), 4)), 2)
                                    };
                                    //detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                                    //detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);

                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(SelectedGRN.Qty)) + @"'
									 , BaseIssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(SelectedGRN.Qty)) + "' WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";//issue.TransactionQty
                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _issueHistoryRepository.Insert(history);


                                }
                                // multiple entry (history)
                                else
                                {
                                    var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                    var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                               - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                                    //if (tqty < input.TransactionQty) throw new CustomException("Stock 0");
                                    if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                    decimal policyAmount = 0;
                                    //decimal qtyDifference = input.TransactionQty;
                                    decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                    foreach (var item in rdList)
                                    {
                                        historyId++;
                                        if (item.TransactionUoMId != item.BaseUOMId)
                                            //input.BaseRate = item.BaseAmount / item.BaseQty;
                                            issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                        //else input.BaseRate = item.TransactionRate;
                                        else issue.BaseRate = item.MaterialTranRate;

                                        //var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty);
                                        var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty - item.PurchaseReturnQty - item.ReductionByAdjustmentQty - item.InventorySalesQty - item.InventoryScrapQty) + Convert.ToDecimal(item.IssueReturnQty);
                                        // (10 - 3)//Issueable Qty
                                        //if (issueQty != 0)
                                        //{

                                        if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                        {
                                            policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                            qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                        }
                                        else // (6 < 7) (qtyDifference < issueQty)
                                        {
                                            //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                            policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                            qtyDifference = 0;
                                        }
                                        SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                        var history = new InventoryIssueHistory
                                        {
                                            Id = MakePK(detail.Id, historyId, 2),
                                            InventoryIssueDetailId = detail.Id,
                                            InventoryReceiveDetailId = item.Id,
                                            Qty = SelectedGRN.Qty,//Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO
                                                                  //Qty = Convert.ToDecimal(issueQty),//TODO
                                                                  // Qty = Convert.ToDecimal(qtyDifference),//TODO
                                                                  //Rate = Convert.ToInt32(issue.BaseRate),
                                                                  //Rate = Convert.ToDecimal(issue.BaseRate),
                                                                  //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                                  //Rate = detailtrnAmount / totalGRNQty,
                                                                  //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                            Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                            //Rate = Math.Round(Convert.ToDecimal(issue.BaseRate),4),
                                            TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                                                                 //TotalAmount = Math.Round((Convert.ToDecimal(issue.BaseRate)* SelectedGRN.Qty),2),//Convert.ToDecimal(detailtrnAmount),
                                            IsCapitalize = false,
                                            IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                            // BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                            //TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * item.BooksCurrencyBaseRate), 2)
                                            BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BaseRate), 4),
                                            TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * Math.Round(Convert.ToDecimal(issue.BaseRate), 4)), 2)
                                        };

                                        AuditService.AddedLog(history);
                                        _issueHistoryRepository.Insert(history);

                                        builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET BaseIssueQty='" + Convert.ToDecimal(issueQty) + "',IssueQty='" + Convert.ToDecimal(issueQty) + "'  WHERE Id='" + item.Id + "'";
                                        rdBuilder.Append(builderSql);
                                        if (qtyDifference == 0)
                                            break;
                                        //}
                                    }

                                    //detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                                    //detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                                }
                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - issue.BaseQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";//issue.TransactionQty
                                rdBuilder.Append(builderSql);
                                AuditService.AddedLog(detail);
                                _issueDetailService.InsertGraph(detail);

                                foreach (var itemAll in entitiesAll)
                                {
                                    //Mapping Data=========================================================
                                    var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + itemAll.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                    if (receiveDetailList1.IsNotNull())
                                    {
                                        bool isQtyAlocated = true;
                                        decimal temp = 0;
                                        int count = 0;
                                        foreach (var receiveDetailListNew in receiveDetailList1)
                                        {


                                            count++;
                                            if (count == 1)
                                            {
                                                if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > detail.TransactionQty)
                                                {

                                                    detail.TransactionQty = detail.TransactionQty;
                                                    //temp += itemDetail.TransactionQty;
                                                    isQtyAlocated = false;

                                                }
                                                else if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < detail.TransactionQty)
                                                {
                                                    //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                    temp = (detail.TransactionQty - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                    detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                    isQtyAlocated = true;

                                                }
                                                else
                                                {
                                                    //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                    detail.TransactionQty = detail.TransactionQty;
                                                    isQtyAlocated = true;

                                                }
                                            }
                                            if (count > 1)
                                            {
                                                if (isQtyAlocated == true)
                                                {
                                                    if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > temp)
                                                    {
                                                        //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                        detail.TransactionQty = detail.TransactionQty;
                                                        isQtyAlocated = false;
                                                    }
                                                    if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < temp)
                                                    {
                                                        //temp = temp - issue.TransactionQtyForPO;
                                                        temp = (temp - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                        //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                        detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                        isQtyAlocated = true;
                                                    }
                                                    else
                                                    {
                                                        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                        detail.TransactionQty = temp;
                                                        isQtyAlocated = true;

                                                    }

                                                }
                                                else
                                                {
                                                    detail.TransactionQty = 0;
                                                }
                                            }


                                            var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                            {
                                                Id = GetIssueDetailAndIssueRequestMapPK(),
                                                InventoryIssueDetailId = detail.Id,
                                                IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                Qty = detail.TransactionQty
                                                //AutoAllocate = true

                                            };
                                            AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                            _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                        }
                                    }

                                    //===================

                                }



                            }

                        }
                        if (specificStockList.IsNotNull())
                        {


                            foreach (var invMaterialId in specificInvaterialIds)
                            {
                                var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                var totalReqQty = 0M;

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;
                                /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* TrnRate)*/
                                /*Rate= Amount/Sum GRN Qty */

                                foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                {
                                    if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");
                                    decimal IssueTransactionQty = item.RequisitionQty;
                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																														FROM (
																																SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD
																																left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalMaterialBooksCurrencyAmount,0) IIHTotalAmount,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																																UNION All
																																SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																														)x
																														WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;
                                    decimal RemainingQty = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"Select BaseQty-ISNULL(IIH.IssueQty,0) BalanceQty from TRN.InventoryReceiveDetail ird 
                                                        left join (select SUM(Qty) IssueQty,InventoryReceiveDetailId from  trn.InventoryIssueHistory Group By InventoryReceiveDetailId) iih on iih.InventoryReceiveDetailId=ird.Id where ird.Id in ('"+ item.InventoryReceiveDetailId + @"')").FirstOrDefault());
                                    if (RemainingQty < 0 || RemainingQty==0)
                                    {
                                        throw new CustomException("There is no available stock for Issue!!!.");
                                    }
                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }

                                    if (item.TransactionUoMId == item.BaseUOMId) //entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault())
                                    {
                                        // detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                        if (RemainingGRNQty == IssueTransactionQty)
                                        {
                                            detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount);
                                        }
                                        else
                                        {
                                            detailtrnAmount += Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate);
                                        }
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = (RemainingGRNQty == IssueTransactionQty) ? Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) : Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate),
                                            //TotalAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);

                                        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                    }
                                    else
                                    {
                                        //detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        if (RemainingGRNQty == IssueTransactionQty)
                                        {
                                            detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount);
                                        }
                                        else
                                        {
                                            
                                            detailtrnAmount += Math.Round(Convert.ToDecimal((item.RequisitionQty*item.BaseUoMFactor) * item.TrnCurrencyBaseRate), 4);
                                        }
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            //TotalAmount = (RemainingGRNQty == IssueTransactionQty) ? Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) : Math.Round(Convert.ToDecimal((item.RequisitionQty * item.BooksCurrencyBaseRate) * item.BaseUoMFactor), 4) * Convert.ToDecimal((item.GRNBaseUoMFactor / item.BaseUoMFactor)),
                                            TotalAmount = (RemainingGRNQty == IssueTransactionQty) ? Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) : Math.Round(Convert.ToDecimal((item.RequisitionQty * item.BooksCurrencyBaseRate)), 4),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);// requitionQty is given as base uom so no need to multiple with uomfactor.
                                    }
                                    item.IssueRequest = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.IssueRequest).FirstOrDefault();
                                }

                                currentId++;
                                var issueDetail = new InventoryIssueDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    InventoryIssueId = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,
                                                          //InventoryIssue = inventoryIssue,
                                    InventoryMaterialId = invMaterialId,
                                    BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                    TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                    AvgRate = Math.Round(Convert.ToDecimal(invMaterial.AvgRate), 4),
                                    Policy = "N/A",

                                    TransactionQty = stockList.Sum(r => r.RequisitionQty),//Math.Round(Convert.ToDecimal(totalGRNQty), 2), //stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),

                                    PolicyRate = Math.Round((Convert.ToDecimal(detailtrnAmount / stockList.Sum(r => r.RequisitionQty))), 4),
                                    PolicyAmount = Math.Round(Convert.ToDecimal(detailtrnAmount), 2),
                                    BaseQty = Math.Round(Convert.ToDecimal(totalGRNQty), 2),//stockList.Sum(r => r.RequisitionQty),
                                    AvgAmount = Math.Round((Convert.ToDecimal(totalGRNQty * invMaterial.AvgRate)), 2),
                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
                                    Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),
                                    ModelState = ModelState.Added
                                };

                                var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                                foreach (var item in stockList)
                                {

                                    if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                                    //if (item.TransactionUoMId != item.BaseUOMId)
                                    //    totalReqQty = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BaseUoMFactor), 4);
                                    //else
                                        totalReqQty = Math.Round(item.RequisitionQty, 4);
                                    historyId++;
                                    var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                    var history = new InventoryIssueHistory
                                    {
                                        Id = MakePK(issueDetail.Id, historyId, 2),
                                        InventoryIssueDetailId = issueDetail.Id,
                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                        Qty = Math.Round(totalReqQty, 4), //item.RequisitionQty,
                                                                          //Rate = Convert.ToDecimal(item.BaseRate),
                                        Rate = Math.Round((SelectedGRN.TotalAmount / totalReqQty), 4),//totalGRNQty
                                        TotalAmount = (item.BaseQty == (item.IssueQty + item.RequisitionQty)) ? Math.Round(Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - item.TotalIssueAmount), 2) : Math.Round(Convert.ToDecimal(totalReqQty * Convert.ToDecimal(item.BooksCurrencyBaseRate)), 2) * Convert.ToDecimal((item.GRNBaseUoMFactor / item.BaseUoMFactor)),//Convert.ToDecimal(detailtrnAmount),
                                        IssueRequestDetailId = item.IssueRequest,
                                        IssueReturnQty = 0,
                                        MaterialStorageId=item.MaterialStorageId,
                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4) * Convert.ToDecimal((item.BaseUoMFactor / item.GRNBaseUoMFactor)),
                                        TotalMaterialBooksCurrencyAmount = (item.BaseQty == (item.IssueQty + item.RequisitionQty)) ? Math.Round(Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - item.TotalIssueAmount), 2) : Math.Round(Convert.ToDecimal(totalReqQty * Convert.ToDecimal(item.BooksCurrencyBaseRate)), 2) * Convert.ToDecimal((item.GRNBaseUoMFactor / item.BaseUoMFactor))//totalReqQty item.RequisitionQty
                                    };
                                    //policyAmmount += history.Qty * history.Rate;



                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
										,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _issueHistoryRepository.Insert(history);
                                    var BOQcount = 0;
                                    foreach (var boqItem in BoqAllocationListVM.Where(r => r.InventoryReceiveDetailId == history.InventoryReceiveDetailId))
                                    {
                                        BOQcount++;
                                        var historyBOQ = new InventoryIssueHistoryBOQ
                                        {
                                            Id = history.Id +"_"+ BOQcount,
                                            InventoryIssueHistoryId = history.Id,
                                            InventoryReceiveDetailId = boqItem.InventoryReceiveDetailId,
                                            Qty = Math.Round(boqItem.RequisitionQty, 4),
                                            Rate = Math.Round((boqItem.Rate), 4),
                                            BOQDetailId=boqItem.BOQDetailId
                                        };
                                        AuditService.AddedLog(historyBOQ);
                                        _issueHistoryBOQRepository.Insert(historyBOQ);
                                    }

                                    //Mapping Data=========================================================
                                    if (entitiesAll.IsNotNull())
                                    {
                                        foreach (var itemall in entitiesAll.Where(q => q.MaterialMasterId == item.MaterialMasterId && q.ArticleId == item.ArticleId && q.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId && q.SecondCharacteristicsId == item.SecondCharacteristicsValueId))
                                        {
                                            var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + itemall.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                            if (receiveDetailList1.IsNotNull())
                                            {
                                                foreach (var receiveDetailListNew in receiveDetailList1)
                                                {
                                                    var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                    {
                                                        Id = GetIssueDetailAndIssueRequestMapPK(),
                                                        InventoryIssueDetailId = issueDetail.Id,
                                                        IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                        Qty = itemall.TransactionQty,
                                                        //AutoAllocate = true

                                                    };
                                                    AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                    _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                                }
                                            }
                                        }
                                    }
                                }

                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";//
                                rdBuilder.Append(builderSql);

                                AuditService.AddedLog(issueDetail);
                                _issueDetailService.InsertGraph(issueDetail);
                            }
                        }

                    }
                    catch (CustomException)
                    {
                        throw;
                    }
                    #endregion

                    _unitOfWork.SaveChanges();
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    flag = false;
                    _unitOfWork.Commit();
                }
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



        public void InsertGraphIssueReturn(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssueReturn inventoryIssue, string IssueTypeStatus)
        {
            var flag = false;

            try
            {
                if (string.IsNullOrEmpty(inventoryIssue.Id))
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var _pk = GetPK1();
                    inventoryIssue.Id = _pk;
                    AuditService.AddedLog(inventoryIssue);
                    _InventoryIssueReturnRepository.Insert(inventoryIssue);
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    try
                    {
                        var historyId = _InventoryIssueReturnHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueReturnHistory] WHERE InventoryIssueReturnId='{inventoryIssue.Id}'").First();

                        foreach (var issue in specificStockList)
                        {


                            var Newid = inventoryIssue.Id + '-';
                            historyId++;
                            var history = new InventoryIssueReturnHistory
                            {
                                Id = Newid + historyId,
                                InventoryIssueReturnId = inventoryIssue.Id,
                                InventoryMaterialId = issue.InventoryMaterialId,
                                InventoryReceiveDetailId = issue.InventoryReceiveDetailId,
                                CostCenterId = issue.CostCenterId,
                                StorageLocationId = issue.MaterialStorageId,
                                Qty = issue.TransactionQty,
                                Rate = Math.Round(Convert.ToDecimal(issue.BaseRate), 4),
                                TotalAmount = Math.Round((issue.TransactionQty * Convert.ToDecimal(issue.BaseRate)), 2),
                                IsCapitalize = false,
                                BaseUOMId = issue.BaseUOMId,
                                TransactionUoMId = issue.TransactionUoMId,
                                IssueRequestDetailId = issue.IssueRequestDetailId,
                                InventoryIssueId = issue.InventoryIssueId,
                                InventoryIssueHistoryId = issue.InventoryIssueHistoryId,
                                InventoryIssueDetailId = issue.InventoryIssueDetailId,

                            };
                            AuditService.AddedLog(history);
                            _InventoryIssueReturnHistoryRepository.Insert(history);
                            var invMaterial = _InventoryIssueReturnHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + issue.InventoryMaterialId + "'").FirstOrDefault();
                            var invMaterial12 = _InventoryIssueReturnHistoryRepository.SqlQuery<InventoryIssueHistory>(@"SELECT * FROM [TRN].[InventoryIssueHistory] WHERE Id='" + issue.InventoryIssueHistoryId + "'").FirstOrDefault();
                            var invMaterial1 = _InventoryIssueReturnRepository.SqlQuery<InventoryReceiveDetail>(@"SELECT * FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + issue.InventoryReceiveDetailId + "'").FirstOrDefault();

                            builderSql = @"UPDATE trn.InventoryIssueHistory SET IssueReturnQty='" + Convert.ToDecimal(Convert.ToDecimal(invMaterial12.IssueReturnQty + issue.TransactionQty)) + "' WHERE Id='" + issue.InventoryIssueHistoryId + "'";
                            rdBuilder.Append(builderSql);

                            builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueReturnQty='" + Convert.ToDecimal(Convert.ToDecimal(invMaterial1.IssueReturnQty + issue.TransactionQty)) + "',BaseIssueQty='"+ Convert.ToDecimal(invMaterial1.BaseIssueQty - issue.TransactionQty) + "',IssueQty='" + Convert.ToDecimal(invMaterial1.IssueQty - issue.TransactionQty) + "' WHERE Id='" + issue.InventoryReceiveDetailId + "'";
                            rdBuilder.Append(builderSql);

                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty + issue.TransactionQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                            rdBuilder.Append(builderSql);

                        }



                    }
                    catch (CustomException)
                    {
                        throw;
                    }



                    _unitOfWork.SaveChanges();
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    //var _pk = GetPK1();
                    //inventoryIssue.Id = _pk;
                    AuditService.UpdatedLog(inventoryIssue);
                    _InventoryIssueReturnRepository.Update(inventoryIssue);
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    try
                    {
                        foreach (var issue in specificStockList)
                        {
                            var Newid = inventoryIssue.Id + '-';


                            //var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueReturnHistory] WHERE InventoryIssueReturnId='{inventoryIssue.Id}'").First();


                            //historyId++;
                            var history = new InventoryIssueReturnHistory
                            {
                                Id = issue.IssueREturnHistoryId,
                                InventoryIssueReturnId = inventoryIssue.Id,
                                InventoryMaterialId = issue.InventoryMaterialId,
                                InventoryReceiveDetailId = issue.InventoryReceiveDetailId,
                                CostCenterId = issue.CostCenterId,
                                StorageLocationId = issue.MaterialStorageId,
                                Qty = issue.TransactionQty,
                                Rate = Math.Round(Convert.ToDecimal(issue.BaseRate), 4),
                                IsCapitalize = false,
                                BaseUOMId = issue.BaseUOMId,
                                TransactionUoMId = issue.TransactionUoMId,
                                IssueRequestDetailId = issue.IssueRequestDetailId
                            };
                            AuditService.UpdatedLog(history);
                            _InventoryIssueReturnHistoryRepository.Update(history);
                            var invMaterial = _InventoryIssueReturnHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + issue.InventoryMaterialId + "'").FirstOrDefault();
                            var invMaterial1 = _InventoryIssueReturnRepository.SqlQuery<InventoryReceiveDetail>(@"SELECT * FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + issue.InventoryReceiveDetailId + "'").FirstOrDefault();
                            var invMaterial12 = _InventoryIssueReturnHistoryRepository.SqlQuery<InventoryIssueHistory>(@"SELECT * FROM [TRN].[InventoryIssueHistory] WHERE Id='" + issue.InventoryIssueHistoryId + "'").FirstOrDefault();

                            builderSql = @"UPDATE trn.InventoryIssueHistory SET IssueReturnQty='" + Convert.ToDecimal((invMaterial12.IssueReturnQty - issue.oldReturnQty) + Convert.ToDecimal(issue.TransactionQty)) + "' WHERE Id='" + issue.InventoryIssueHistoryId + "'";
                            rdBuilder.Append(builderSql);

                            builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueReturnQty='" + Convert.ToDecimal((invMaterial1.IssueReturnQty - issue.oldReturnQty) + issue.TransactionQty) + "' WHERE Id='" + issue.InventoryReceiveDetailId + "'";
                            rdBuilder.Append(builderSql);

                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal((invMaterial.TotalQty - issue.oldReturnQty) + issue.TransactionQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                            rdBuilder.Append(builderSql);

                        }



                    }
                    catch (CustomException)
                    {
                        throw;
                    }



                    _unitOfWork.SaveChanges();
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    flag = false;
                    _unitOfWork.Commit();
                }
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

        public void DeleteIssueDetail(string issueDetailId,string voucherId)
        {
            var flag = false;
            try
            {
                
                if ( voucherId!="null")
                {
                    throw new CustomException("Posted voucher  have to delete first!");
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                var builder = new System.Text.StringBuilder();
                var sql = "";
                sql = @"UPDATE A SET A.TotalQty=A.TotalQty+B.TransactionQty FROM [TRN].[InventoryMaterial] AS A JOIN [TRN].[InventoryIssueDetail] AS B ON B.InventoryMaterialId=A.Id WHERE B.Id='" + issueDetailId + "'";
                builder.Append(sql);
                //sql = @"UPDATE A SET A.IssueQty=A.IssueQty-B.Qty FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryIssueHistory] AS B ON B.InventoryReceiveDetailId=A.Id WHERE B.InventoryIssueDetailId='" + issueDetailId + "'";
                //builder.Append(sql);
                sql = @"UPDATE A SET  A.BaseIssueQty=A.BaseIssueQty-B.Qty FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryIssueHistory] AS B ON B.InventoryReceiveDetailId=A.Id WHERE B.InventoryIssueDetailId='" + issueDetailId + "'";
                builder.Append(sql);
                sql = @"DELETE [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='" + issueDetailId + "'";
                builder.Append(sql);
                sql = @"DELETE [TRN].[IssueDetailAndIssueRequestMap] WHERE InventoryIssueDetailId='" + issueDetailId + "'";
                builder.Append(sql);
                sql = @"DELETE [TRN].[InventoryIssueDetail]  WHERE Id='" + issueDetailId + "'";
                builder.Append(sql);
                _sqlRepository.ExecuteSqlCommand(builder.ToString());
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

        public void DeleteIssueDetailBOQ(string issueDetailId, string voucherId)
        {
            var flag = false;
            try
            {

                if (voucherId != "null")
                {
                    throw new CustomException("Posted voucher  have to delete first!");
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                var builder = new System.Text.StringBuilder();
                var sql = "";
                sql = @"UPDATE A SET A.TotalQty=A.TotalQty+B.TransactionQty FROM [TRN].[InventoryMaterial] AS A JOIN [TRN].[InventoryIssueDetail] AS B ON B.InventoryMaterialId=A.Id WHERE B.Id='" + issueDetailId + "'";
                builder.Append(sql);
                //sql = @"UPDATE A SET A.IssueQty=A.IssueQty-B.Qty FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryIssueHistory] AS B ON B.InventoryReceiveDetailId=A.Id WHERE B.InventoryIssueDetailId='" + issueDetailId + "'";
                //builder.Append(sql);
                sql = @"UPDATE A SET  A.BaseIssueQty=A.BaseIssueQty-B.Qty FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryIssueHistory] AS B ON B.InventoryReceiveDetailId=A.Id WHERE B.InventoryIssueDetailId='" + issueDetailId + "'";
                builder.Append(sql);
                sql = @"DELETE [TRN].[InventoryIssueHistoryBOQ] WHERE  InventoryIssueHistoryId in ( select id from trn.InventoryIssueHistory where InventoryIssueDetailId='" + issueDetailId + "')";
                builder.Append(sql);
                sql = @"DELETE [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='" + issueDetailId + "'";
                builder.Append(sql);
                sql = @"DELETE [TRN].[IssueDetailAndIssueRequestMap] WHERE InventoryIssueDetailId='" + issueDetailId + "'";
                builder.Append(sql);
                sql = @"DELETE [TRN].[InventoryIssueDetail]  WHERE Id='" + issueDetailId + "'";
                builder.Append(sql);
                _sqlRepository.ExecuteSqlCommand(builder.ToString());
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
        public void DeleteSalesDetail(string issueDetailId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var builder = new System.Text.StringBuilder();
                var sql = "";
                sql = @"UPDATE A SET A.TotalQty=A.TotalQty+B.TransactionQty FROM [TRN].[InventoryMaterial] AS A JOIN [TRN].[InventorySalesDetail] AS B ON B.InventoryMaterialId=A.Id WHERE B.Id='" + issueDetailId + "'";
                builder.Append(sql);
                sql = @"UPDATE A SET A.InventorySalesQty=A.InventorySalesQty-B.Qty FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventorySalesHistory] AS B ON B.InventoryReceiveDetailId=A.Id WHERE B.InventorySalesDetailId='" + issueDetailId + "'";
                builder.Append(sql);

                sql = @"DELETE trn.inventorySalesTax 
						FROM trn.inventorySalesTax 
						INNER JOIN trn.InventorySalesHistory ON trn.InventorySalesHistory.Id = trn.inventorySalesTax.InventorySalesHistoryId
						WHERE trn.InventorySalesHistory.InventorySalesDetailId='" + issueDetailId + "'";
                builder.Append(sql);

                sql = @"DELETE [TRN].[InventorySalesHistory] WHERE InventorySalesDetailId='" + issueDetailId + "'";
                builder.Append(sql);
                sql = @"DELETE [TRN].[InventorySalesDetail]  WHERE Id='" + issueDetailId + "'";
                builder.Append(sql);
                _sqlRepository.ExecuteSqlCommand(builder.ToString());
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
        public GridModel Query(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT E.UserName AS Entity ,II.IssueType, II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate,'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 ,EI.EmployeeCode+' - '+EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount,II.Remarks,II.Id AS IssueId
                                FROM [TRN].[InventoryIssue] AS II
                                JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId=II.Id AND IID.IsAsset=0
                                JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId=MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId=II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id=II.EntityId
                                WHERE II.PlantId='" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        
        public IEnumerable<object> GetIssueList(string column, string value, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select  top (500) Temp.* from (SELECT II.Id,II.Id IssueNo, II.IssueDate,II.Remarks,II.EntityId,E.UserName  EntityName,II.IssueType
                                    ,EI.EmployeeCode+' - '+EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount
                                    ,ii.OrderRefNo, IsOrderSpecificy=  CASE WHEN ii.OrderRefNo <> '' THEN 1 ELSE 0 END,II.[Types]
									,SourceNo=II.JWContractId,JW.ContractId,LC.LCRef,Customer=P.Code+' '+P.UserName 
									,MaterialStorage= STUFF((select distinct ','+XPD.UserName from
									[HKP].[MaterialStorage] XPD Left join TRN.inventoryIssueHistory AS XV ON XV.MaterialStorageId=XPD.Id
									LEFT JOIN TRN.InventoryIssueDetail XIID ON XIID.Id=xv.InventoryIssueDetailId
									where XIID.InventoryIssueId=II.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    FROM [TRN].[InventoryIssue] AS II
							        JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId=II.Id
								    left join dbo.EmployeeInformation AS EI ON EI.SystemId=II.EmployeeId
                                    left join org.Entity E ON E.Id=II.EntityId
									LEFT JOIN [dbo].[OSTransformationPO] JW ON JW.Id=II.JWContractId
									left join dbo.[Contract] CN ON CN.Id=JW.ContractId
									LEFT JOIN dbo.MasterLC LC ON LC.Id=CN.MasterLCId
									LEFT JOIN HKP.Party P ON P.Id=LC.CustomerId
                            WHERE II.PlantId='" + plantId + @"' AND ISNULL(II.[Status],'')<>'Posting' 
                            AND Isnull(IID.IsAsset,0)=0 AND II.IsPostingRequired=1
                            GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 , II.IssueDate
									 ,EI.EmployeeCode,EI.EmployeeName,II.Remarks,II.EntityId,E.UserName,II.IssueType
									 , ii.OrderRefNo,II.[Types],II.JWContractId,JW.ContractId,LC.LCRef,P.Code,p.UserName)  AS TEMP WHERE " + strkey + @"
                                     order by TEMP.IssueDate desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetInventoryIssueReturnListForPosting(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT E.UserName AS Entity ,isnull(II.IssueType,'') IssueType, II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 --,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
									 ,SUM(IID.Qty) Qty,
									  SUM(IID.TotalAmount) Amount,
									 II.Remarks,II.Id AS IssueId,II.OrderRefNo
                                    FROM[TRN].[InventoryIssueReturn] AS II
                                left JOIN [TRN].InventoryIssueReturnHistory AS IID ON IID.InventoryIssueReturnId= II.Id -- AND IID.IsAsset= 0
                                left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                                left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                                Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                                WHERE II.PlantId= '" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void RequisitionIssueInsert(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
            , InventoryIssue inventoryIssue, IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails)
        {
            var flag = false;
            try
            {
                if (entities.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var pk = GetPK();

                    inventoryIssue.Id = GetPK();
                    inventoryIssue.IssueDate = DateTime.Now;
                    inventoryIssue.EntityId = entities.Select(r => r.EntityId).FirstOrDefault();
                    inventoryIssue.MaterialStorageId = entities.Select(r => r.MaterialStorageId).FirstOrDefault();
                    InsertGraph(inventoryIssue);
                    _issueDetailService.RequisitionIssueDetailInsert(entities, specificStockList, inventoryIssue, requisitionIssueDetails);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
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

        public void RequisitionIssueUpdate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
           , InventoryIssue inventoryIssue, IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails)
        {
            var flag = false;
            try
            {
                if (entities.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var inventoryIssueUpdateData = base.Find(inventoryIssue.Id);
                    UpdateGraph(inventoryIssueUpdateData);
                    _issueDetailService.RequisitionIssueDetailUpdate(entities, specificStockList, inventoryIssueUpdateData, requisitionIssueDetails);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
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

        #region Issue Register Excel and Pdf Report

        public IWorkbook CreateIssueRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Library.Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                //var sheet2 = workbook.Worksheets[1];               
                //var Head = "Stores Issue Register";// + " " + fromDate + " " + "To" + " " + toDate;

                var Head = "";
                if (Type == "Posted")
                {
                    Head = "Stores Issue Register(Posted)";
                }
                else 
                {
                    Head = "Stores Issue Register(Non-Posted)";
                }

                CreateIssueRegisterReportSheet(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;




            }
            catch (Exception)
            {
                throw;
            }
        }

        // Out Source

        public IWorkbook CreateOSIssueRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Library.Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                //var sheet2 = workbook.Worksheets[1];               
                //var Head = "Stores Issue Register";// + " " + fromDate + " " + "To" + " " + toDate;

                var Head = "";
                if (Type == "Posted")
                {

                    Head = "Out Source Issue Register(Posted)";

                }

                else if (Type == "NonPosted")
                {

                    Head = "Out Source Issue Register(Non-Posted)";

                }

                CreateOSIssueRegisterReportSheet(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;

            }
            catch (Exception)
            {
                throw;
            }
        }

        // Outsourcing

        private void CreateOSIssueRegisterReportSheet(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {


            var cmdText = "";
            if (Type == "Posted")
            {
                cmdText = @"SELECT II.Id AS IssueId
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate
	                        --,II.CompanyGroupId
	                        --,II.CompanyId
	                        --,II.PlantId
	                        -- ,II.EntityId  ---userName as Entityname 
	                        ,En.UserName AS Entityname
	                        --,II.AddedBy
	                        --,II.AddedDate
	                        --,II.AddedFromIP
	                        --,II.UpdatedBy
	                        --,II.UpdatedDate
	                        --,II.UpdatedFromIP
	                        -- ,II.MaterialStorageId
	                        ,MS.UserName AS MaterialStorageName
	                        ,II.STATUS
							,HSNC.Code HSNCode
                             ,II.Remarks
	                        -- ,II.VoucherId 
	                        --,VoucherNo=CASE WHEN II.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
	                        ,v.VoucherNo
	                        --,Posted=CASE WHEN II.Status <>'' then 'Yes' else 'No' END						
	                        --,PostingDate= CASE WHEN II.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
	                        --,PostedBy=CASE WHEN II.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,II.EmployeeId
	                        ,IID.Id IssueDetailId
	                        ,IID.InventoryIssueId
	                        --,IID.InventoryMaterialId
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName
	                        -- , IM.ArticleId
	                        ,ART.StandardName ArticleName
	                        ,IsAsset = CASE 
		                        WHEN MM.IsAsset = 0
			                        THEN 'No'
		                        ELSE 'Yes'
		                        END
	                        --, IM.FirstCharacteristicsId
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
	                        ,Round(IID.TransactionQty,2) TransactionQty
	                        --,IID.BaseUOMId
	                        ,TUoM.UserName AS UOM
	                        ,BUoM.UserName AS BaseUOM
	                        ,Round(IID.AvgRate,4) AvgRate
	                        ,Round(IID.AvgAmount,2) AvgAmount
	                        ,Round(IID.PolicyRate,4) PolicyRate
	                        ,Round(IID.PolicyAmount,2) PolicyAmount
	                        ,IID.Policy
	                        --,IID.AddedBy
	                        --,IID.AddedDate
	                        --,IID.AddedFromIP
	                        --,IID.UpdatedBy
	                        --,IID.UpdatedDate
	                        --,IID.UpdatedFromIP
	                        ,IID.BaseQty
	                        ,IID.InventoryReceiveId
	                        ,IID.InventoryReceiveDetailId
                            ,ISNULL(IGL.UserName,'') AS GL
							,ISNULL(IA.UserName,'') Activity
							,isnull(B.UserName,'') AS Budget
							,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget
                           ,CC.UserName CostCenterName,EI.EmployeeName
                            ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName--,MLC.LCRef
                            ,PLC.LCRef as PurchaseLCNo,ospo.Id as PONumber,pod.ReferenceNo
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId
					    LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId = MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.TransactionUoMId = TUoM.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IID.BaseUOMId = BUoM.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        --LEFT JOIN trn.Invoice AS I ON I.InventoryReceiveId = II.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
                        --left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=II.Id					
                        --left join trn.Voucher V1 on V1.Id=ep.VoucherId 
                        LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                       LEFT join dbo.EmployeeInformation EI ON EI.SystemId=II.EmployeeId
                       left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
						left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
						left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
					--	LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
						left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
                        left join dbo.OSTransformationPODetail pod on pod.OSTransformationPOId=ospo.Id and pod.Id=IID.OSTransformationPOId
                    where v.VoucherNo is not null ANd II.PlantId='" + plantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";

            }
            else
            {
                cmdText = @"SELECT II.Id AS IssueId
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate
	                        --,II.CompanyGroupId
	                        --,II.CompanyId
	                        --,II.PlantId
	                        -- ,II.EntityId  ---userName as Entityname 
	                        ,En.UserName AS Entityname
							,HSNC.Code HSNCode
	                        --,II.AddedBy
	                        --,II.AddedDate
	                        --,II.AddedFromIP
	                        --,II.UpdatedBy
	                        --,II.UpdatedDate
	                        --,II.UpdatedFromIP
	                        -- ,II.MaterialStorageId
	                        ,MS.UserName AS MaterialStorageName
	                        ,II.STATUS
                              ,II.Remarks
	                        -- ,II.VoucherId 
	                        --,VoucherNo=CASE WHEN II.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
	                        ,v.VoucherNo
	                        --,Posted=CASE WHEN II.Status <>'' then 'Yes' else 'No' END						
	                        --,PostingDate= CASE WHEN II.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
	                        --,PostedBy=CASE WHEN II.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,II.EmployeeId
	                        ,IID.Id IssueDetailId
	                        ,IID.InventoryIssueId
	                        --,IID.InventoryMaterialId
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName
	                        -- , IM.ArticleId
	                        ,ART.StandardName ArticleName
	                        ,IsAsset = CASE 
		                        WHEN MM.IsAsset = 0
			                        THEN 'No'
		                        ELSE 'Yes'
		                        END
	                        --, IM.FirstCharacteristicsId
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
	                        ,Round(IID.TransactionQty,2) TransactionQty
	                        --,IID.BaseUOMId
	                        ,TUoM.UserName AS UOM
	                        ,BUoM.UserName AS BaseUOM
	                        ,Round(IID.AvgRate,4) AvgRate
	                        ,Round(IID.AvgAmount,2) AvgAmount
	                        ,Round(IID.PolicyRate,4) PolicyRate
	                        ,Round(IID.PolicyAmount,2) PolicyAmount
	                        ,IID.Policy
	                        --,IID.AddedBy
	                        --,IID.AddedDate
	                        --,IID.AddedFromIP
	                        --,IID.UpdatedBy
	                        --,IID.UpdatedDate
	                        --,IID.UpdatedFromIP
	                        ,IID.BaseQty
	                        ,IID.InventoryReceiveId
	                        ,IID.InventoryReceiveDetailId
							,ISNULL(IGL.UserName,'') AS GL
							,ISNULL(IA.UserName,'') Activity
							,isnull(B.UserName,'') AS Budget
							,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget
                            ,CC.UserName CostCenterName,EI.EmployeeName
                            ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName--,MLC.LCRef
                            ,PLC.LCRef as PurchaseLCNo,ospo.Id as PONumber,pod.ReferenceNo
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.TransactionUoMId = TUoM.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IID.BaseUOMId = BUoM.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        --LEFT JOIN trn.Invoice AS I ON I.InventoryReceiveId = II.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
                        --left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=II.Id					
                        --left join trn.Voucher V1 on V1.Id=ep.VoucherId 
						LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                        LEFT join dbo.EmployeeInformation EI ON EI.SystemId=II.EmployeeId
                        left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
						left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
						left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
					--	LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
						left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
                        left join dbo.OSTransformationPODetail pod on pod.OSTransformationPOId=ospo.Id and pod.Id=IID.OSTransformationPOId
                    where v.VoucherNo is null ANd II.PlantId='" + plantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";
            }
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");


            var _rowd = 4;
            var colTransactionQtyTotal = 0.00;
            var colAvgAmountTotal = 0.00;
            var colPolicyAmountTotal = 0.00;
            var colBaseQtyTotal = 0.00;


            if (fromDate != "" && toDate != "")
            {

                sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;

                sheet1[_rowd, 4].CellStyle.Font.Size = 8;
                sheet1[_rowd, 4].CellStyle.Font.Bold = false;
                sheet1[_rowd, 4].CellStyle.Font.Size = 8;
                sheet1.Range[_rowd, 3, _rowd, 6].Merge();


            }

            var _rows = 5;
            sheet1[_rows, 6].Text = "Report Ref No:";
            sheet1[_rows, 6].CellStyle.Font.Size = 8;
            sheet1.Range[_rows, 3, _rows, 6].Merge();
            sheet1[_rows, 6].CellStyle.Font.Bold = false;
            var _row = 6;



            sheet1[_row, 33].Text = "Posted (Dr.)";
            sheet1[_row, 33].CellStyle.Font.Size = 10;
            sheet1[_row, 33].CellStyle.Font.Bold = true;
            sheet1[_row, 33].WrapText = true;
            sheet1[_row, 33].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1[_row, 33].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_row, 33, _row, 35].BorderAround(ExcelLineStyle.Hair);
            sheet1.Range[_row, 33, _row, 35].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[_row, 33, _row, 35].Merge();
            sheet1.Range[_row, 33, _row, 35].CellStyle.ColorIndex = ExcelKnownColors.Tan;

            sheet1[_row, 36].Text = "Posted (Cr.)";
            sheet1[_row, 36].CellStyle.Font.Size = 10;
            sheet1[_row, 36].CellStyle.Font.Bold = true;
            sheet1[_row, 36].WrapText = true;
            sheet1[_row, 36].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1[_row, 36].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_row, 36, _row, 38].BorderAround(ExcelLineStyle.Hair);
            sheet1.Range[_row, 36, _row, 38].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[_row, 36, _row, 38].Merge();
            sheet1.Range[_row, 36, _row, 38].CellStyle.ColorIndex = ExcelKnownColors.Tan;



            var _rowL = _row;
            var row = _row + 1;
            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Id");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            // report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Entity name");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Number";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Contract No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Customer";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Ref No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase LC No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "UDNo";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Entity name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Cost Center Name");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Cost Center Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Person Name");
            //sheet1headreColIndex++;



            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Person Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Storage Name");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Status");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Status";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Detail Id");
            //sheet1headreColIndex++;



            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Detail Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Qty");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn. Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTransactionQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UoM");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn. UoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Avg Rate");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Avg Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Avg Amount");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Avg Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colAvgAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Policy Rate");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Books Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Policy Amount");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Books Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colPolicyAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Policy");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Policy";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Base Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colBaseQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base UoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colBaseUOMTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Remarks");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Remarks";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Budget");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "BUdget");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "BUdget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

            //sheet1headreColIndex++;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;

                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["IssueId"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["IssueDate"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["PONumber"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ContractNo"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["CustomerName"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["ReferenceNo"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["PurchaseLCNo"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["UDNo"].ToString());

                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["Entityname"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CostCenterName"].ToString());
                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["EmployeeName"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["MaterialStorageName"].ToString());
                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["Status"].ToString());
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["IssueDetailId"].ToString());

                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 20, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 21, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 22, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
                report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 24, inventoryMaterialList.Rows[n]["UOM"].ToString());
                report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AvgRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AvgAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PolicyRate"].ToString()), 4);
                report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PolicyAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 29, inventoryMaterialList.Rows[n]["Policy"].ToString());
                report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseQty"].ToString()));
                report.SetText(ref sheet1, _rowL, colBaseUOMTotal, inventoryMaterialList.Rows[n]["BaseUOM"].ToString());
                report.SetText(ref sheet1, _rowL, 32, inventoryMaterialList.Rows[n]["Remarks"].ToString());
                report.SetText(ref sheet1, _rowL, 33, inventoryMaterialList.Rows[n]["GL"].ToString());
                report.SetText(ref sheet1, _rowL, 34, inventoryMaterialList.Rows[n]["Budget"].ToString());
                report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["Activity"].ToString());
                report.SetText(ref sheet1, _rowL, 36, inventoryMaterialList.Rows[n]["CGL"].ToString());
                report.SetText(ref sheet1, _rowL, 37, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
                report.SetText(ref sheet1, _rowL, 38, inventoryMaterialList.Rows[n]["CActivity"].ToString());


            }

            _rowL++;

            if (fromDate != "" && toDate != "")
            {
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, "Total");
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal) - 1].CellStyle.Font.Bold = true;
                //sheet1.Range[1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, _rowL].Merge();
                object sumObject;
                sumObject = inventoryMaterialList.Compute("Sum(TransactionQty)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionQtyTotal), Convert.ToDouble(sumObject).ToString("0,##.00"));
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(AvgAmount)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colAvgAmountTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colAvgAmountTotal), Convert.ToDouble(sumObject).ToString("0,##.00"));
                sheet1.Range[_rowL, Convert.ToInt32(colAvgAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colAvgAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;


                sumObject = inventoryMaterialList.Compute("Sum(PolicyAmount)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colPolicyAmountTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colPolicyAmountTotal), Convert.ToDouble(sumObject).ToString("0,##.00"));
                sheet1.Range[_rowL, Convert.ToInt32(colPolicyAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colPolicyAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(BaseQty)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colBaseQtyTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colBaseQtyTotal), Convert.ToDouble(sumObject).ToString("0,##.00"));
                sheet1.Range[_rowL, Convert.ToInt32(colBaseQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colBaseQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
            }
            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);




            //#endregion Signature

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);




        }


        private void CreateIssueRegisterReportSheet(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            InventoryIssueQueryService inventoryIssueQueryService = new InventoryIssueQueryService(_sqlRepository);
            DataTable cmdText = inventoryIssueQueryService.GetIssueRegister(fromDate, toDate, Type);

            var inventoryMaterialList = cmdText;
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");


            var _rowd = 4;
            var colTransactionQtyTotal = 0.00;
            var colAvgAmountTotal = 0.00;
            var colPolicyAmountTotal = 0.00;
            var colBaseQtyTotal = 0.00;


            if (fromDate != "" && toDate != "")
            {

                sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;

                sheet1[_rowd, 4].CellStyle.Font.Size = 8;
                sheet1[_rowd, 4].CellStyle.Font.Bold = false;
                sheet1[_rowd, 4].CellStyle.Font.Size = 8;
                sheet1.Range[_rowd, 3, _rowd, 6].Merge();


            }

            var _rows = 5;
            sheet1[_rows, 6].Text = "Report Ref No:";
            sheet1[_rows, 6].CellStyle.Font.Size = 8;
            sheet1.Range[_rows, 3, _rows, 6].Merge();
            sheet1[_rows, 6].CellStyle.Font.Bold = false;
            var _row = 6;



            sheet1[_row, 34].Text = "Posted (Dr.)";
            sheet1[_row, 34].CellStyle.Font.Size = 10;
            sheet1[_row, 34].CellStyle.Font.Bold = true;
            sheet1[_row, 34].WrapText = true;
            sheet1[_row, 34].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1[_row, 34].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_row, 34, _row, 38].BorderAround(ExcelLineStyle.Hair);
            sheet1.Range[_row, 34, _row, 38].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[_row, 34, _row, 38].Merge();
            sheet1.Range[_row, 34, _row, 38].CellStyle.ColorIndex = ExcelKnownColors.Tan;

            sheet1[_row, 39].Text = "Posted (Cr.)";
            sheet1[_row, 39].CellStyle.Font.Size = 10;
            sheet1[_row, 39].CellStyle.Font.Bold = true;
            sheet1[_row, 39].WrapText = true;
            sheet1[_row, 39].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1[_row, 39].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_row, 39, _row, 43].BorderAround(ExcelLineStyle.Hair);
            sheet1.Range[_row, 39, _row, 43].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[_row, 39, _row, 43].Merge();
            sheet1.Range[_row, 39, _row, 43].CellStyle.ColorIndex = ExcelKnownColors.Tan;



            var _rowL = _row;
            var row = _row + 1;
            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Id");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            // report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Entity name");
            //sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Voucher No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Entity name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Cost Center Name");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Cost Center";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Person Name");
            //sheet1headreColIndex++;



            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue By";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Department";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Storage Name");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Storage Name";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Status");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Status";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Detail Id");
            //sheet1headreColIndex++;



            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Detail Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Qty");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn. Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colTransactionQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UoM");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn. UoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Avg Rate");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Avg Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Avg Amount");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Avg Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colAvgAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Policy Rate");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Books Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Policy Amount");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Books Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colPolicyAmountTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Policy");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Policy";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Base Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            colBaseQtyTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base UoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            int colBaseUOMTotal = sheet1headreColIndex;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Remarks");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Is Park";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posting Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posted By";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Remarks";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GLCode";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Budget");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "BudgetRefNo";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGLCode";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "BUdget");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CBUdget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CActivity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CBudgetRefNo";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;

                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["IssueId"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["IssueDate"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["VoucherNo"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["Entityname"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["CostCenterName"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["EmployeeName"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["DepartmentName"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["MaterialStorageName"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["Status"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["IssueDetailId"].ToString());
                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IssueType"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
                report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 21, inventoryMaterialList.Rows[n]["UOM"].ToString());
                report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AvgRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AvgAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PolicyRate"].ToString()), 4);
                report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PolicyAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 26, inventoryMaterialList.Rows[n]["Policy"].ToString());
                report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseQty"].ToString()));
                report.SetText(ref sheet1, _rowL, colBaseUOMTotal, inventoryMaterialList.Rows[n]["BaseUOM"].ToString());
                report.SetText(ref sheet1, _rowL, 29, inventoryMaterialList.Rows[n]["IsPark"].ToString());
                report.SetText(ref sheet1, _rowL, 30, inventoryMaterialList.Rows[n]["PostingDate"].ToString());
                report.SetText(ref sheet1, _rowL, 31, inventoryMaterialList.Rows[n]["PostedBy"].ToString());
                report.SetText(ref sheet1, _rowL, 32, inventoryMaterialList.Rows[n]["Remarks"].ToString());
                report.SetText(ref sheet1, _rowL, 33, inventoryMaterialList.Rows[n]["PONo"].ToString());
                report.SetText(ref sheet1, _rowL, 34, inventoryMaterialList.Rows[n]["GLCode"].ToString());
                report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["GL"].ToString());
                report.SetText(ref sheet1, _rowL, 36, inventoryMaterialList.Rows[n]["Budget"].ToString());
                report.SetText(ref sheet1, _rowL, 37, inventoryMaterialList.Rows[n]["Activity"].ToString());
                report.SetText(ref sheet1, _rowL, 38, inventoryMaterialList.Rows[n]["BudgetRefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 39, inventoryMaterialList.Rows[n]["CGLCode"].ToString());
                report.SetText(ref sheet1, _rowL, 40, inventoryMaterialList.Rows[n]["CGL"].ToString());
                report.SetText(ref sheet1, _rowL, 41, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
                report.SetText(ref sheet1, _rowL, 42, inventoryMaterialList.Rows[n]["CActivity"].ToString());
                report.SetText(ref sheet1, _rowL, 43, inventoryMaterialList.Rows[n]["CBudgetRefNo"].ToString());
                
            }

            _rowL++;

            if (fromDate != "" && toDate != "")
            {
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, "Total");
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal) - 1].CellStyle.Font.Bold = true;
                //sheet1.Range[1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, _rowL].Merge();
                object sumObject;
                sumObject = inventoryMaterialList.Compute("Sum(TransactionQty)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionQtyTotal), Convert.ToDouble(sumObject).ToString("##.00"));
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(AvgAmount)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colAvgAmountTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colAvgAmountTotal), Convert.ToDouble(sumObject).ToString("0,##.00"));
                sheet1.Range[_rowL, Convert.ToInt32(colAvgAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colAvgAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;


                sumObject = inventoryMaterialList.Compute("Sum(PolicyAmount)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colPolicyAmountTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colPolicyAmountTotal), Convert.ToDouble(sumObject).ToString("0,##.00"));
                sheet1.Range[_rowL, Convert.ToInt32(colPolicyAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colPolicyAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

                sumObject = inventoryMaterialList.Compute("Sum(BaseQty)", "");
                sheet1.Range[_rowL, Convert.ToInt32(colBaseQtyTotal)].CellStyle.Font.Bold = true;
                report.SetText(ref sheet1, _rowL, Convert.ToInt32(colBaseQtyTotal), Convert.ToDouble(sumObject).ToString("##.00"));
                sheet1.Range[_rowL, Convert.ToInt32(colBaseQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[_rowL, Convert.ToInt32(colBaseQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
            }
            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);




            //#endregion Signature

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);




        }

        #endregion Material Stock Ledeger 



        #region Issue Return Register Excel and Pdf Report




        public IWorkbook CreateIssueReturnRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Library.Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                //var sheet2 = workbook.Worksheets[1];               
                //var Head = "Inventory Issue Return Register";// + " " + fromDate + " " + "To" + " " + toDate;


                var Head = "";
                if (Type == "Posted")
                {

                    Head = "Inventory Issue Return Register(Posted)";


                }

                else if (Type == "Non Posted")
                {

                    Head = "Inventory Issue Return Register(Non-Posted)";


                }



                CreateIssueReturnRegisterReportSheet(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private void CreateIssueReturnRegisterReportSheet(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {


            var cmdText = "";
            if (Type == "Posted")
            {
                cmdText = @"SELECT II.Id AS IssueId
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate
	                        ,En.UserName AS Entityname
	                        ,MS.UserName AS MaterialStorageName
	                        ,II.STATUS
	                        ,v.VoucherNo
	                        ,IRH.Id IssueDetailId
	                        ,IRH.InventoryIssueReturnId
	                        --,IID.InventoryMaterialId
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName
	                        -- , IM.ArticleId
	                        ,ART.StandardName ArticleName
	                        ,IsAsset = CASE 
		                        WHEN MM.IsAsset = 0
			                        THEN 'No'
		                        ELSE 'Yes'
		                        END
	                        --, IM.FirstCharacteristicsId
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
	                        ,IRH.InventoryReceiveDetailId,TUOM .UserName AS UOM
                            ,CC.UserName CostCenterName,EI.EmployeeName
							,IIH.StandardName StorageLocation,IRH.Qty,IRH.IssueRequestDetailId
                        FROM TRN.[InventoryIssueReturn] II
                        LEFT JOIN [TRN].InventoryIssueReturnHistory IRH ON II.Id = IRH.InventoryIssueReturnId	
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IRH.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IRH.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
                        LEFT join dbo.EmployeeInformation EI ON EI.SystemId=II.EmployeeId
						LEFT JOIN [HKP].[MaterialStorage] IIH ON IIH.id = IRH.StorageLocationId
                        LEFT JOIN [SCS].[UnitOfMeasurement] TUOM ON TUOM.id = IRH.BaseUOMId
                    where v.VoucherNo is not null ANd II.PlantId='" + plantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";

            }
            else
            {
                cmdText = @"SELECT II.Id AS IssueId
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate
	                        ,En.UserName AS Entityname
	                        ,MS.UserName AS MaterialStorageName
	                        ,II.STATUS
	                        ,v.VoucherNo
	                        ,IRH.Id IssueDetailId
	                        ,IRH.InventoryIssueReturnId
	                        --,IID.InventoryMaterialId
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName
	                        -- , IM.ArticleId
	                        ,ART.StandardName ArticleName
	                        ,IsAsset = CASE 
		                        WHEN MM.IsAsset = 0
			                        THEN 'No'
		                        ELSE 'Yes'
		                        END
	                        --, IM.FirstCharacteristicsId
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
	                        ,IRH.InventoryReceiveDetailId
                            ,CC.UserName CostCenterName,EI.EmployeeName,TUOM .UserName AS UOM
							,IIH.StandardName StorageLocation,IRH.Qty,IRH.IssueRequestDetailId
                        FROM TRN.[InventoryIssueReturn] II
                        LEFT JOIN [TRN].InventoryIssueReturnHistory IRH ON II.Id = IRH.InventoryIssueReturnId	
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IRH.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IRH.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
                        LEFT join dbo.EmployeeInformation EI ON EI.SystemId=II.EmployeeId
						LEFT JOIN [HKP].[MaterialStorage] IIH ON IIH.id = IRH.StorageLocationId
                        LEFT JOIN [SCS].[UnitOfMeasurement] TUOM ON TUOM.id = IRH.BaseUOMId
                    where v.VoucherNo is null ANd II.PlantId='" + plantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";
            }
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");



            var _rowd = 4;

            if (fromDate != "" && toDate != "")
            {


                sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;

                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.UsedRange.CellStyle.Font.Bold = true;
                sheet1.Range[_rowd, 3, _rowd, 6].Merge();


            }

            var _rows = 6;
            sheet1[_rows, 6].Text = "Report Ref No: ";
            sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.Range[_rows, 3, _rows, 6].Merge();
            sheet1.Range[_rows, 3, _rows, 6].CellStyle.Font.Bold = false;


            var _row = 8;

            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _row += 1;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Return Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Return Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Return Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Cost Center Name");
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1headreColIndex++;


            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Storage Name");
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1headreColIndex++;


            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Detail Id");
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UOM");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "UOM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, " Qty");
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1headreColIndex++;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "IssueRequestDetailId");

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "IssueRequestDetailId";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;

                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["IssueId"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["IssueDate"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["CostCenterName"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["MaterialStorageName"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["IssueDetailId"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["UOM"].ToString());
                report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Qty"].ToString()));
                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["IssueRequestDetailId"].ToString());



            }

            //_rowL++;

            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
            sheet1.Range[(_row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(_row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);



            //#endregion Signature

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);


        }

        #endregion Material Stock Ledeger 

        #region GRN Issue  Excel and Pdf Report




        public IWorkbook CreateIssueRegisterGRNIssueReport(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Library.Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                //var sheet2 = workbook.Worksheets[1];               
                //var Head = "GRN Wise Stores Issue Register" + " " + fromDate + " " + "To" + " " + toDate;
                var Head = "GRN Wise Stores Issue Register";

                CreateIssueRegisterGRNIssueReport(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Out Source

        public IWorkbook CreateOSIssueRegisterGRNIssueReport(string companyId, string plantId, string fromDate, string toDate, string Type)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Library.Service.Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                //var sheet2 = workbook.Worksheets[1];               
                //var Head = "GRN Wise Stores Issue Register" + " " + fromDate + " " + "To" + " " + toDate;
                var Head = "GRN Wise Out Source Issue Register";

                CreateOSIssueRegisterGRNIssueReport(ref sheet1, report, Head, "Summary", companyId, plantId, fromDate, toDate, Type);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateOSIssueRegisterGRNIssueReport(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {


            var cmdText = "";
            if (Type == "Posted")
            {
                //          cmdText = @"SELECT II.Id AS IssueId
                //                   ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate	 
                //                   ,MT.UserName MaterialType
                //                   ,MGM.UserName AS MaterialGroupMasterName
                //	,HSNC.Code HSNCode
                //                   ,IM.MaterialMasterId
                //                   ,MM.UserName MaterialMasterName	                      
                //                   ,ART.StandardName ArticleName	                        
                //                   ,FC.UserName AS FirstCharacteristics
                //                   ,IM.FirstCharacteristicsValueId
                //                   ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
                //                   ,IM.SecondCharacteristicsId
                //                   ,SC.UserName AS SecondCharacteristics
                //                   ,IM.SecondCharacteristicsValueId
                //                   ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
                //                   ,IM.ThirdCharacteristicsId
                //                   ,TC.UserName AS ThirdCharacteristics
                //                   ,IM.ThirdCharacteristicsValueId
                //                   ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
                //	,IIH.InventoryReceiveDetailId 
                //	,IRD.Id GRNDetailId
                //	,IRD.TransactionQty GRNQty
                //	,TUoM1.UserName AS GRNUOM
                //	,IRD.MaterialTranRate GRNRate
                //	,isnull(IIH1.Qty,0) OtherIssuedQty
                //	,isnull(IIH.Qty,0) CurrentIssueQty
                //	,TUoM.UserName AS IssueUOM							
                //                   ,TotalIssued=(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0))						
                //	,Balance=(Isnull(IRD.TransactionQty,0)-(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0)))
                //                   ,ISNULL(IGL.UserName,'') AS GL
                //	,ISNULL(IA.UserName,'') Activity
                //	,isnull(B.UserName,'') AS Budget
                //	,isnull(IGL1.UserName,'') AS CGL
                //	,isnull(IA1.UserName,'') AS CActivity
                //	,isnull(B1.UserName,'') AS CBUdget
                //                      ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,PLC.LCRef as PurchaseLCNo,ospo.Id as PONumber
                //                  FROM trn.InventoryIssue II
                //                  LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId						
                //                  LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                //                  LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                //                  LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                //                  LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                //LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                //                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                //                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                //                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                //                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                //                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                //                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                //                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                //                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                //                  LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                //                  --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                //                  --LEFT JOIN trn.Invoice AS I ON I.InventoryReceiveId = II.Id
                //                  LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
                //LEFT JOIN trn.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
                //LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
                //LEFT JOIN(select Sum(Qty) Qty,InventoryIssueDetailId from  trn.InventoryIssueHistory group by InventoryIssueDetailId) IIH1 ON IIH1.InventoryIssueDetailId=IID.Id AND  IID.InventoryIssueId !=II.Id
                //                  LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
                //  LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM1 ON IRD.BaseUOMId = TUoM1.Id
                //LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
                //LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
                //LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
                //Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
                //LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
                //LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
                //LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
                //Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                //                  left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
                //left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
                //left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
                //LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
                //left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
                //              where v.VoucherNo is not null ANd II.PlantId='" + plantId + "'AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";


                cmdText = @"SELECT II.Id AS IssueId,IID.Id as IssueDetailId
                            ,OSPOType=case when ospo.POType='OSValueAddedPO' then 'ValueAdded' else 'Transformation' End
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate	 
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName	                      
	                        ,ART.StandardName ArticleName	                        
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
							,IIH.InventoryReceiveDetailId 
							,IRD.Id GRNDetailId
							,IRD.TransactionQty GRNQty
							--,TUoM1.UserName AS GRNUOM
							,GRNUOM=case when IRD.BaseUOMId is not null then TUoM1.UserName else TUoM2.UserName End
							,TUoM2.UserName as TrnUoM
							,IRD.MaterialTranRate GRNRate
							,C.Code AS TransactionCurrency
							,Ir.ToCurrencyRate CurrencyConvRate
							,IRD.TotalMaterialBooksCurrencyAmount TrnAmtBDT
							,IRD.BaseQty GRNBaseQty
                        	,round(IRD.BooksCurrencyBaseRate,4) BaseRate
                        	,(IRD.BaseQty * IRD.BooksCurrencyBaseRate) BaseAmtBDT
							,MS.UserName as MaterialStorage
							,isnull(IIH1.Qty,0) OtherIssuedQty
							,isnull(IIH.Qty,0) CurrentIssueQty
							,TUoM.UserName AS IssueUOM							
	                        ,TotalIssued=(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0))						
							,Balance=(Isnull(IRD.TransactionQty,0)-(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0)))
	                        

                           ,ISNULL(IGL.UserName,'') AS GL
							,ISNULL(IA.UserName,'') Activity
							,isnull(B.UserName,'') AS Budget
							,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget
                            ,CC.UserName CostCenterName
                            ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName--,MLC.LCRef
                            ,PLC.LCRef as PurchaseLCNo,ospo.Id as PONumber,pod.ReferenceNo
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId	
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                       
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        --LEFT JOIN trn.Invoice AS I ON I.InventoryReceiveId = II.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
						LEFT JOIN trn.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
						LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
						LEFT JOIN(select Sum(Qty) Qty,InventoryIssueDetailId from  trn.InventoryIssueHistory group by InventoryIssueDetailId) IIH1 ON IIH1.InventoryIssueDetailId=IID.Id --AND  IID.InventoryIssueId !=II.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
					   LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM1 ON IRD.BaseUOMId = TUoM1.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM2 ON IRD.TransactionUoMId = TUoM2.Id


                      LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId


						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                        left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
						left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
						left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
					--	LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
						left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id = IRD.InventoryReceiveId
						LEFT JOIN SCS.Currency C ON C.Id = IR.CurrencyId
					--	left join dbo.OSTransformationPO PO on PO.Id=II.JWContractId
                        left join dbo.OSTransformationPODetail pod on pod.OSTransformationPOId=ospo.Id and pod.Id=IID.OSTransformationPOId
                    where v.VoucherNo is not null ANd II.PlantId='" + plantId + "'AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";
            }
            else
            {
                //          cmdText = @"SELECT II.Id AS IssueId
                //                   ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate	 
                //                   ,MT.UserName MaterialType
                //                   ,MGM.UserName AS MaterialGroupMasterName
                //                   ,IM.MaterialMasterId
                //	,HSNC.Code HSNCode
                //                   ,MM.UserName MaterialMasterName	                      
                //                   ,ART.StandardName ArticleName	                        
                //                   ,FC.UserName AS FirstCharacteristics
                //                   ,IM.FirstCharacteristicsValueId
                //                   ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
                //                   ,IM.SecondCharacteristicsId
                //                   ,SC.UserName AS SecondCharacteristics
                //                   ,IM.SecondCharacteristicsValueId
                //                   ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
                //                   ,IM.ThirdCharacteristicsId
                //                   ,TC.UserName AS ThirdCharacteristics
                //                   ,IM.ThirdCharacteristicsValueId
                //                   ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
                //	,IIH.InventoryReceiveDetailId 
                //	,IRD.Id GRNDetailId
                //	,IRD.TransactionQty GRNQty
                //	,TUoM1.UserName AS GRNUOM
                //	,IRD.MaterialTranRate GRNRate
                //	,isnull(IIH1.Qty,0) OtherIssuedQty
                //	,isnull(IIH.Qty,0) CurrentIssueQty
                //	,TUoM.UserName AS IssueUOM							
                //                   ,TotalIssued=(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0))						
                //	,Balance=(Isnull(IRD.TransactionQty,0)-(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0)))
                //                   ,ISNULL(IGL.UserName,'') AS GL
                //	,ISNULL(IA.UserName,'') Activity
                //	,isnull(B.UserName,'') AS Budget
                //	,isnull(IGL1.UserName,'') AS CGL
                //	,isnull(IA1.UserName,'') AS CActivity
                //	,isnull(B1.UserName,'') AS CBUdget
                //                      ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,PLC.LCRef as PurchaseLCNo,ospo.Id as PONumber
                //                  FROM trn.InventoryIssue II
                //                  LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId						
                //                  LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                //                  LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                //                  LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                //                  LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                //LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                //                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                //                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                //                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                //                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                //                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                //                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                //                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                //                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                //                  LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                //                  --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                //                  --LEFT JOIN trn.Invoice AS I ON I.InventoryReceiveId = II.Id
                //                  LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
                //LEFT JOIN trn.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
                //LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
                //LEFT JOIN(select Sum(Qty) Qty,InventoryIssueDetailId from  trn.InventoryIssueHistory group by InventoryIssueDetailId) IIH1 ON IIH1.InventoryIssueDetailId=IID.Id AND  IID.InventoryIssueId !=II.Id
                //                  LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
                //  LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM1 ON IRD.BaseUOMId = TUoM1.Id
                //LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
                //LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
                //LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
                //Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
                //LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
                //LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
                //LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
                //Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                //                  left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
                //left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
                //left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
                //LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
                //left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
                //              where v.VoucherNo is null ANd II.PlantId='" + plantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";


                cmdText = @"SELECT II.Id AS IssueId,IID.Id as IssueDetailId
                            ,OSPOType=case when ospo.POType='OSValueAddedPO' then 'ValueAdded' else 'Transformation' End
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate	 
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName	                      
	                        ,ART.StandardName ArticleName	                        
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
							,IIH.InventoryReceiveDetailId 
							,IRD.Id GRNDetailId
							,IRD.TransactionQty GRNQty
							--,TUoM1.UserName AS GRNUOM
							,GRNUOM=case when IRD.BaseUOMId is not null then TUoM1.UserName else TUoM2.UserName End
							,TUoM2.UserName as TrnUoM
							,IRD.MaterialTranRate GRNRate
							,C.Code AS TransactionCurrency
							,Ir.ToCurrencyRate CurrencyConvRate
							,IRD.TotalMaterialBooksCurrencyAmount TrnAmtBDT
							,IRD.BaseQty GRNBaseQty
                        	,round(IRD.BooksCurrencyBaseRate,4) BaseRate
                        	,(IRD.BaseQty * IRD.BooksCurrencyBaseRate) BaseAmtBDT
							,MS.UserName as MaterialStorage
							,isnull(IIH1.Qty,0) OtherIssuedQty
							,isnull(IIH.Qty,0) CurrentIssueQty
							,TUoM.UserName AS IssueUOM							
	                        ,TotalIssued=(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0))						
							,Balance=(Isnull(IRD.TransactionQty,0)-(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0)))
	                        

                           ,ISNULL(IGL.UserName,'') AS GL
							,ISNULL(IA.UserName,'') Activity
							,isnull(B.UserName,'') AS Budget
							,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget
                            ,CC.UserName CostCenterName
                            ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName--,MLC.LCRef
                            ,PLC.LCRef as PurchaseLCNo,ospo.Id as PONumber,pod.ReferenceNo
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId	
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                       
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        --LEFT JOIN trn.Invoice AS I ON I.InventoryReceiveId = II.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
						LEFT JOIN trn.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
						LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
						LEFT JOIN(select Sum(Qty) Qty,InventoryIssueDetailId from  trn.InventoryIssueHistory group by InventoryIssueDetailId) IIH1 ON IIH1.InventoryIssueDetailId=IID.Id --AND  IID.InventoryIssueId !=II.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
					   LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM1 ON IRD.BaseUOMId = TUoM1.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM2 ON IRD.TransactionUoMId = TUoM2.Id


                      LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId


						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                        left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
						left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
						left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
					--	LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
						left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id = IRD.InventoryReceiveId
						LEFT JOIN SCS.Currency C ON C.Id = IR.CurrencyId
					--	left join dbo.OSTransformationPO PO on PO.Id=II.JWContractId
                        left join dbo.OSTransformationPODetail pod on pod.OSTransformationPOId=ospo.Id and pod.Id=IID.OSTransformationPOId
                    where v.VoucherNo is null ANd II.PlantId='" + plantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";

            }
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");
            var _rowd = 4;
            if (fromDate != "" && toDate != "")
            {

                sheet1[_rowd, 3].Text = fromDate + " " + "To" + " " + toDate;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.UsedRange.CellStyle.Font.Bold = true;
                sheet1.Range[_rowd, 3, _rowd, 6].Merge();

            }

            var _rows = 6;
            sheet1[_rows, 5].Text = "Report Ref No: ";
            sheet1.Range[_rows, 3, _rows, 6].Merge();
            sheet1.UsedRange.CellStyle.Font.Bold = false;
            var _row = 7;

            sheet1[_row, 35].Text = "Posted Dr.";
            sheet1.UsedRange.CellStyle.Font.Size = 10;
            sheet1.UsedRange.CellStyle.Font.Bold = true;
            sheet1.UsedRange.WrapText = true;
            sheet1[_row, 35].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1[_row, 35].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_row, 35, _row, 37].BorderAround(ExcelLineStyle.Hair);
            //sheet1.Range[_row, 18, _row, 20].CellStyle.Color="LightYellow";
            sheet1.Range[_row, 35, _row, 37].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[_row, 35, _row, 37].Merge();
            sheet1.Range[_row, 35, _row, 37].CellStyle.ColorIndex = ExcelKnownColors.Tan;

            sheet1[_row, 38].Text = "Posted (Cr.)";
            sheet1.UsedRange.CellStyle.Font.Size = 10;
            sheet1.UsedRange.CellStyle.Font.Bold = true;
            sheet1.UsedRange.WrapText = true;
            sheet1[_row, 38].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1[_row, 38].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_row, 38, _row, 40].BorderAround(ExcelLineStyle.Hair);
            sheet1.Range[_row, 38, _row, 40].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[_row, 38, _row, 40].Merge();
            sheet1.Range[_row, 38, _row, 40].CellStyle.ColorIndex = ExcelKnownColors.Tan;

            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Id");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Detail Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Date");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Detail Id");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Number";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Contract No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Customer";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Ref No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase LC No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "UDNo";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Detail Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Storage Location";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRNQty");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Qty";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRNUOM");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN UOM";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Rate");
            //sheet1headreColIndex++;

            //sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Rate";
            //sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            //sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            //sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Other Issued Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue UOM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Current Issue Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Current Issue Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base UoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Current Issue Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Other Issued Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue UOM");
            //sheet1headreColIndex++;




            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Balance");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Balance";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn UoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn Currency";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Currency Conversion Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn Amount BDT";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base UoM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Amount BDT";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Budget");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;



            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "BUdget");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "BUdget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

            //sheet1headreColIndex++;

            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;
            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["IssueId"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["IssueDetailId"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["IssueDate"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["PONumber"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["OSPOType"].ToString());

                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["ContractNo"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["CustomerName"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ReferenceNo"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["PurchaseLCNo"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UDNo"].ToString());

                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["GRNDetailId"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["MaterialStorage"].ToString());

                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                //report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());

                report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                //report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
                //report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNQty"].ToString()));
                //report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["GRNUOM"].ToString());
                //report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNRate"].ToString()));

                report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["IssueUOM"].ToString());
                report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CurrentIssueQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 21, inventoryMaterialList.Rows[n]["GRNUOM"].ToString());
                report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CurrentIssueQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OtherIssuedQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Balance"].ToString()));

                report.SetText(ref sheet1, _rowL, 25, inventoryMaterialList.Rows[n]["TrnUoM"].ToString());
                report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 27, inventoryMaterialList.Rows[n]["TransactionCurrency"].ToString());
                report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNRate"].ToString()));

                report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CurrencyConvRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TrnAmtBDT"].ToString()));
                report.SetText(ref sheet1, _rowL, 31, inventoryMaterialList.Rows[n]["GRNUOM"].ToString());
                report.SetText(ref sheet1, _rowL, 32, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNBaseQty"].ToString()));

                report.SetText(ref sheet1, _rowL, 33, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 34, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseAmtBDT"].ToString()));


                report.SetText(ref sheet1, _rowL, 35, inventoryMaterialList.Rows[n]["GL"].ToString());
                report.SetText(ref sheet1, _rowL, 36, inventoryMaterialList.Rows[n]["Budget"].ToString());
                report.SetText(ref sheet1, _rowL, 37, inventoryMaterialList.Rows[n]["Activity"].ToString());
                report.SetText(ref sheet1, _rowL, 38, inventoryMaterialList.Rows[n]["CGL"].ToString());
                report.SetText(ref sheet1, _rowL, 39, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
                report.SetText(ref sheet1, _rowL, 40, inventoryMaterialList.Rows[n]["CActivity"].ToString());
            }

            //#endregion sumCalc
            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

        }

        public DataTable GetIssueRegisterBYGRN(string fromDate, string toDate, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var temp = "";
                var sql = "";

                if (Type == "Posted")
                {
                    temp = "and v.VoucherNo is not null";
                }
                if (Type == "NonPosted")
                {
                    temp = "and v.VoucherNo is null";
                }

                sql = @"SELECT II.Id AS IssueId,IID.Id IssueDetailId
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate	 
	                        ,MT.UserName MaterialType,II.IssueType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
                            ,HSNC.Code HSNCode
	                        ,MM.UserName MaterialMasterName	                      
	                        ,ART.StandardName ArticleName	                        
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
							,IIH.InventoryReceiveDetailId 
							,IRD.Id GRNDetailId
							,IRD.TransactionQty GRNQty
							,TUoM1.UserName AS GRNUOM
							,IRD.MaterialTranRate GRNRate
							,isnull(IIH1.Qty,0) OtherIssuedQty
							,isnull(IIH.Qty,0) CurrentIssueQty
                            ,isnull(IIH.TotalMaterialBooksCurrencyAmount,0) IssueAmount
							,TUoM.UserName AS IssueUOM							
	                        ,TotalIssued=(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0))						
							,Balance=(Isnull(IRD.TransactionQty,0)-(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0)))

                            ,GLCode=case when v.Id <>'' then  ISNULL(IGL.AccountCode,'') else ISNULL(IGLNP.AccountCode,'') end
                            ,GL=case when v.Id <>'' then  ISNULL(IGL.UserName,'') else ISNULL(IGLNP.UserName,'') end  
							,Activity=case when v.Id <>'' then ISNULL(IA.UserName,'') else ISNULL(IANP.UserName,'') end
							,Budget=case when v.Id <>'' then isnull(B.UserName,'') else ISNULL(BNP.UserName,'') end

							,CGLCode=case when v.Id <>'' then  ISNULL(IGL1.AccountCode,'') else ISNULL(IGLCNP.AccountCode,'') end
							,CGL=case when v.Id <>'' then isnull(IGL1.UserName,'') else ISNULL(IGLCNP.UserName,'') end
							,CActivity=case when v.Id <>'' then isnull(IA1.UserName,'') else ISNULL(IACNP.UserName,'') end
							,CBUdget=case when v.Id <>'' then isnull(B1.UserName,'') else ISNULL(BNPC.UserName,'') end


	                        ,isnull(IBM.RefNo,'') BudgetRefNo,ISNULL(IBM1.RefNo ,'') CBudgetRefNo
                            ,CC.UserName CostCenterName
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId		
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId = MM.Id
                        LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                       
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        --LEFT JOIN trn.Invoice AS I ON I.InventoryReceiveId = II.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
						LEFT JOIN trn.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
						LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
						LEFT JOIN(select Sum(Qty) Qty,InventoryIssueDetailId from  trn.InventoryIssueHistory group by InventoryIssueDetailId) IIH1 ON IIH1.InventoryIssueDetailId=IID.Id AND  IID.InventoryIssueId !=II.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
					   LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM1 ON IRD.BaseUOMId = TUoM1.Id


                        LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
						--NonPosted
						--dr
						LEFT JOIN MST.BudgetMaster IBMNP ON IBMNP.Id=IID.BudgetMasterId
						LEFT JOIN HKP.GLGeneralInfo IGLNP ON IGLNP.Id=IBMNP.GLGeneralInfoId 
						LEFT JOIN HKP.Activity IANP ON IANP.Id=IID.ActivityId
						Left JOIN hkp.Budget BNP On BNP.Id=IBMNP.BudgetProductionOrderId
						--cr
						LEFT JOIN HKP.GLGeneralInfo IGLCNP ON IGLCNP.Id=IRD.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBMCNP ON IBMCNP.Id=IRD.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IACNP ON IACNP.Id=IRD.PostDrActivityId
						Left JOIN hkp.Budget BNPC On BNPC.Id=IBMCNP.BudgetId

                    where II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' " + temp + "";

                return _sqlRepository.GetDataTable(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        private void CreateIssueRegisterGRNIssueReport(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
        {

            DataTable cmdText = GetIssueRegisterBYGRN(fromDate, toDate, Type);
            
            var inventoryMaterialList =cmdText;
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");
            var _rowd = 4;
            if (fromDate != "" && toDate != "")
            {

                sheet1[_rowd, 3].Text = fromDate + " " + "To" + " " + toDate;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.UsedRange.CellStyle.Font.Bold = true;
                sheet1.Range[_rowd, 3, _rowd, 6].Merge();

            }

            var _rows = 6;
            sheet1[_rows, 5].Text = "Report Ref No: ";
            sheet1.Range[_rows, 3, _rows, 6].Merge();
            sheet1.UsedRange.CellStyle.Font.Bold = false;
            var _row = 7;

            sheet1[_row, 22].Text = "Posted Dr.";
            sheet1.UsedRange.CellStyle.Font.Size = 10;
            sheet1.UsedRange.CellStyle.Font.Bold = true;
            sheet1.UsedRange.WrapText = true;
            sheet1[_row, 22].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1[_row, 22].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_row, 22, _row, 26].BorderAround(ExcelLineStyle.Hair);
            //sheet1.Range[_row, 18, _row, 20].CellStyle.Color="LightYellow";
            sheet1.Range[_row, 22, _row, 26].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[_row, 22, _row, 26].Merge();
            sheet1.Range[_row, 22, _row, 26].CellStyle.ColorIndex = ExcelKnownColors.Tan;

            sheet1[_row, 27].Text = "Posted (Cr.)";
            sheet1.UsedRange.CellStyle.Font.Size = 10;
            sheet1.UsedRange.CellStyle.Font.Bold = true;
            sheet1.UsedRange.WrapText = true;
            sheet1[_row, 27].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1[_row, 27].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_row, 27, _row, 31].BorderAround(ExcelLineStyle.Hair);
            sheet1.Range[_row, 27, _row, 31].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[_row, 27, _row, 31].Merge();
            sheet1.Range[_row, 27, _row, 31].CellStyle.ColorIndex = ExcelKnownColors.Tan;

            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Id");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue Date");
            //sheet1headreColIndex++;
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Detail Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 14;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Detail Id");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Date";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Detail Id";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 14;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Type");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRNQty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRNUOM");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN UOM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRN Rate");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Rate";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Other Issued Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Other Issued Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Current Issue Qty");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Current Issue Qty";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Issue UOM");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue UOM";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Amount";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Balance");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Balance";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL Code";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Budget");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;
            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");
            //sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Budget Ref No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
            //sheet1headreColIndex++;
            
            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGL Code";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "GL";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "BUdget");
            //sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "BUdget";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;


            sheet1.Range[_rowL, sheet1headreColIndex].Text = "Activity";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;

            sheet1.Range[_rowL, sheet1headreColIndex].Text = "CBudget Ref No";
            sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
            sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
            sheet1headreColIndex++;

            //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Activity");

            //sheet1headreColIndex++;

            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
            sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;
            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["IssueId"].ToString());
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["IssueDetailId"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["IssueDate"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["IssueType"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["GRNDetailId"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
                report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
                report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["GRNUOM"].ToString());
                report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNRate"].ToString()));
                report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OtherIssuedQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CurrentIssueQty"].ToString()));
                report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["IssueUOM"].ToString());
                report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueAmount"].ToString()));
                report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Balance"].ToString()));
                report.SetText(ref sheet1, _rowL, 22, inventoryMaterialList.Rows[n]["GLCode"].ToString());
                report.SetText(ref sheet1, _rowL, 23, inventoryMaterialList.Rows[n]["GL"].ToString());
                report.SetText(ref sheet1, _rowL, 24, inventoryMaterialList.Rows[n]["Budget"].ToString());
                report.SetText(ref sheet1, _rowL, 25, inventoryMaterialList.Rows[n]["Activity"].ToString());
                report.SetText(ref sheet1, _rowL, 26, inventoryMaterialList.Rows[n]["BudgetRefNo"].ToString());
                report.SetText(ref sheet1, _rowL, 27, inventoryMaterialList.Rows[n]["CGLCode"].ToString());
                report.SetText(ref sheet1, _rowL, 28, inventoryMaterialList.Rows[n]["CGL"].ToString());
                report.SetText(ref sheet1, _rowL, 29, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
                report.SetText(ref sheet1, _rowL, 30, inventoryMaterialList.Rows[n]["CActivity"].ToString());
                report.SetText(ref sheet1, _rowL, 31, inventoryMaterialList.Rows[n]["CBudgetRefNo"].ToString());
            }

            //#endregion sumCalc
            sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

        }


        #endregion Material Stock Ledeger 

        #region AssetInventoryIssue

        public GridModel GetAssetInventoryIssue(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate,'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 ,EI.EmployeeCode+' - '+EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount,II.OrderRefNo
                                FROM [TRN].[InventoryIssue] AS II
                                JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId=II.Id AND IID.IsAsset=1
                                JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId=MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId=II.EmployeeId
                                WHERE II.PlantId='" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' AND II.IssueType='Capital'
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 , II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.OrderRefNo";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public void InsertAssetInventoryIssue(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue)
        {
            var flag = false;
            try
            {
                if (entities.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var inventoryMaterialList = _inventoryMaterialService.GetInventoryMaterialListByUpToSku(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                    var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;

                    foreach (var item in entities)// update view model (inventory material field)
                    {
                        var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                && t.FirstCharacteristicsId == item.FirstCharacteristicsId && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                && t.SecondCharacteristicsId == item.SecondCharacteristicsId && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                               && t.ThirdCharacteristicsId == item.ThirdCharacteristicsId && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                               && t.CountryId == item.CountryId
                               && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId// && t.CountryId == item.CountryId
                               );
                        if (im.IsNotNull())
                        {
                            //if (im.TotalQty < item.RequisitionQty) throw new CustomException(@"Stock is limited {" + item.RequisitionQty + "}");
                            //if (im.TotalQty < item.RequisitionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");

                            item.InventoryMaterialId = im.Id;
                            item.CompanyGroupId = im.CompanyGroupId;
                            item.CompanyId = inventoryIssue.CompanyId;
                            item.PlantId = inventoryIssue.PlantId;
                            item.CurrencyId = currencyId;
                            item.MaterialStorageId = null;
                            item.MaterialMasterId = im.MaterialMasterId;
                            item.ArticleId = im.ArticleId;
                            item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                            item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                            item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                            item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                            item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                            item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                            item.TotalQty = im.TotalQty;
                            item.AvgRate = im.AvgRate;
                        }
                    }// update view model (inventory material field)
                    inventoryIssue.CurrencyId = currencyId;
                    inventoryIssue.Id = GetPK();
                    InsertGraph(inventoryIssue);
                    _issueDetailService.InsertAssetIssueDetail(entities, specificStockList, inventoryIssue);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
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

        
        #endregion

        public void NonPostedIssueDelete(string issueId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var issue = _issueRepository.Find(issueId);
                var issueDetail = _issueDetailService.Query(r => r.InventoryIssueId == issueId).Select().ToList();
                var inventoryissueDetailIds = issueDetail.Select(t => t.Id).ToArray();
                var issueDetailHistory = _sqlRepository.GetModelCollection<InventoryIssueHistory>(@"SELECT *  FROM TRN.InventoryIssueHistory WHERE InventoryIssueDetailId IN(" + ReturnStringArray(inventoryissueDetailIds) + @")").ToList();
                var inventoryReceiveDetailIds = issueDetailHistory.Select(t => t.InventoryReceiveDetailId).ToArray();
                var inventoryReceiveDetail = _sqlRepository.GetModelCollection<InventoryReceiveDetail>(@"SELECT *  FROM TRN.InventoryReceiveDetail WHERE Id IN(" + ReturnStringArray(inventoryReceiveDetailIds) + @")").ToList();

                foreach (var item in issueDetailHistory)
                {
                    var inventoryReceiveDetailRow = inventoryReceiveDetail.Where(r => r.Id == item.InventoryReceiveDetailId).FirstOrDefault();
                    decimal Qty = _issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty),0) Qty   FROM TRN.InventoryIssueHistory WHERE Id<>'" + item.Id + @"' AND InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + @"'").FirstOrDefault();

                    inventoryReceiveDetailRow.BaseIssueQty = Qty;
                    inventoryReceiveDetailRow.IssueQty = Qty;
                    _receiveDetailRepository.Update(inventoryReceiveDetailRow);

                    var inventoryMaterial = _inventoryMaterialRepository.Find(inventoryReceiveDetailRow.InventoryMaterialId);
                    inventoryMaterial.TotalQty = inventoryMaterial.TotalQty + item.Qty;
                    _inventoryMaterialRepository.Update(inventoryMaterial);
                    _issueHistoryRepository.Delete(item.Id);
                }
                foreach (var item in issueDetail)
                {
                    _issueDetailService.Delete(item.Id);
                }
                _issueRepository.Delete(issueId);
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

        public void PostedIssueDelete(string issueId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var issue = _issueRepository.Find(issueId);
                var issueDetail = _issueDetailService.Query(r => r.InventoryIssueId == issueId).Select().ToList();
                var inventoryissueDetailIds = issueDetail.Select(t => t.Id).ToArray();
                var issueDetailHistory = _sqlRepository.GetModelCollection<InventoryIssueHistory>(@"SELECT *  FROM TRN.InventoryIssueHistory WHERE InventoryIssueDetailId IN(" + ReturnStringArray(inventoryissueDetailIds) + @")").ToList();
                var inventoryReceiveDetailIds = issueDetailHistory.Select(t => t.InventoryReceiveDetailId).ToArray();
                var inventoryReceiveDetail = _sqlRepository.GetModelCollection<InventoryReceiveDetail>(@"SELECT *  FROM TRN.InventoryReceiveDetail WHERE Id IN(" + ReturnStringArray(inventoryReceiveDetailIds) + @")").ToList();

                var voucher = _voucherService.FindVoucher(issue.VoucherId);
                var voucherdetail = _voucherDetailRepository.Query(r => r.VoucherId == issue.VoucherId).Select().ToList();
                var voucherdetailcurrency = _voucherDetailCurrencyRepository.Query(r => r.VoucherId == issue.VoucherId).Select().ToList();


                foreach (var item in issueDetailHistory)
                {
                    var inventoryReceiveDetailRow = inventoryReceiveDetail.Where(r => r.Id == item.InventoryReceiveDetailId).FirstOrDefault();
                    decimal Qty = _issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty),0) Qty   FROM TRN.InventoryIssueHistory WHERE Id<>'" + item.Id + @"' AND InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + @"'").FirstOrDefault();


                    inventoryReceiveDetailRow.BaseIssueQty = Convert.ToDecimal(Qty);
                    inventoryReceiveDetailRow.IssueQty = Convert.ToDecimal(Qty);

                    _receiveDetailRepository.Update(inventoryReceiveDetailRow);

                    var inventoryMaterial = _inventoryMaterialRepository.Find(inventoryReceiveDetailRow.InventoryMaterialId);
                    inventoryMaterial.TotalQty = inventoryMaterial.TotalQty + item.Qty;
                    _inventoryMaterialRepository.Update(inventoryMaterial);
                    _issueHistoryRepository.Delete(item.Id);
                }
                foreach (var item in voucherdetailcurrency)
                {
                    _voucherDetailCurrencyRepository.Delete(item.Id);
                }
                foreach (var item in voucherdetail)
                {
                    _voucherDetailRepository.Delete(item.Id);
                }
                foreach (var item in issueDetail)
                {
                    _issueDetailService.Delete(item.Id);
                }
                _issueRepository.Delete(issueId);
                _voucherRepository.Delete(voucher.Id);
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


        #region Physical Stock Adjustment 

        public void InsertPhysicalStockAdjustment(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, PhysicalStockAdjustmentMaster inventoryIssue, string IssueTypeStatus)
        {
            var flag = false;
            bool FlagIsAsset = false;
            var GRNCalculateList = new List<InventoryIssueHistory>();
            if (IssueTypeStatus.ToString() == "Inventory")
            {
                FlagIsAsset = false;
            }
            else
            {
                FlagIsAsset = true;
            }
            try
            {
                if (entities.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var _pk = GetPK2();
                    var inventoryMaterialList = _inventoryMaterialService.GetInventoryMaterialListByUpToSku(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                    var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
                    foreach (var item in entities)// update view model (inventory material field)
                    {
                        var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                && t.FirstCharacteristicsId == item.FirstCharacteristicsId && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                && t.SecondCharacteristicsId == item.SecondCharacteristicsId && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                               && t.ThirdCharacteristicsId == item.ThirdCharacteristicsId && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                               && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId// && t.CountryId == item.CountryId
                               );
                        if (im.IsNotNull())
                        {
                            //if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited {" + item.TransactionQty + "}");
                            if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                            item.InventoryIssueId = _pk;
                            item.InventoryMaterialId = im.Id;
                            item.CompanyGroupId = im.CompanyGroupId;
                            item.CompanyId = inventoryIssue.CompanyId;
                            item.PlantId = inventoryIssue.PlantId;
                            item.CurrencyId = currencyId;
                            item.MaterialStorageId = null;
                            item.MaterialMasterId = im.MaterialMasterId;
                            item.ArticleId = im.ArticleId;
                            item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                            item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                            item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                            item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                            item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                            item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                            item.TotalQty = im.TotalQty;
                            item.AvgRate = im.AvgRate;
                        }
                    }// update view model (inventory material field)
                    inventoryIssue.CurrencyId = currencyId;
                    inventoryIssue.Id = _pk;
                    AuditService.AddedLog(inventoryIssue);
                    _PhysicalStockAdjustmentMasterRepository.Insert(inventoryIssue);
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    //_issueDetailService.InsertRange(entities, specificStockList, inventoryIssue);


                    #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======
                    try
                    {

                        var uiList = entities.ToList();
                        var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PhysicalStockAdjustmentDetail] WHERE PhysicalStockAdjustmentMasterID='{inventoryIssue.Id}'").First();
                        var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                        var specificInvaterialIds = new string[] { };
                        var maIds = new string[] { };
                        if (specificStockList.IsNotNull())
                        {
                            specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                            maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                            for (int i = uiList.Count() - 1; i >= 0; i--)
                            {
                                var row = uiList.ElementAt(i);
                                if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                    uiList.RemoveAt(i);
                            }
                        }
                        else maIds = inventoryMaterialIds;
                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty, ISNULL(IRD.IssueReturnQty,0) IssueReturnQty, ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,isnull(IRD.InventoryTransferQty,0) InventoryTransferQty
                                       ,Isnull(IRD.InventorySalesQty,0) InventorySalesQty,Isnull(IRD.InventoryScrapQty,0) InventoryScrapQty, IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
										  AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
										  AND IR.Status='Posting' 
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.IssueDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                        if (receiveDetailList.IsNotNull())
                        {
                            foreach (var issue in uiList)
                            {

                                var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;
                                /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* (TotalMmaterialTrnAmount/MaterialTrnRate))*/
                                /*Rate= Amount/Sum GRN Qty */
                                if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                                    //input.BaseRate = receiveDetailRow.BaseAmount / receiveDetailRow.BaseQty;
                                    issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                if (issue.TransactionUoMId != issue.BaseUOMId)
                                    issue.BaseQty = Convert.ToInt16(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);
                                decimal IssueTransactionQty = issue.TransactionQty;
                                //foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                //    {
                                //    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                //    //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                //    if (item.TransactionUoMId == issue.TransactionUoMId)
                                //        {
                                //        detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount/item.TransactionQty)));
                                //        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                //        }
                                //        else
                                //        {
                                //        detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                //        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                //    }
                                //    }
                                foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                {

                                    if (IssueTransactionQty <= 0)
                                        break;

                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(ISH.TotalBaseAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(IIH.TotalAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                    //																							FROM TRN.InventoryReceiveDetail IRD
                                    //																							left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                    //																						    WHERE  IRD.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																													FROM (
																															SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																															FROM TRN.InventoryReceiveDetail IRD
																															left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																															UNION All
																															SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																															FROM TRN.InventoryReceiveDetail IRD	
																															LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																															UNION All
																															SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																															FROM TRN.InventoryReceiveDetail IRD	
																															LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																															UNION All
																															SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																															FROM TRN.InventoryReceiveDetail IRD	
																															LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																															UNION All
																															SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																															FROM TRN.InventoryReceiveDetail IRD	
																															LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																															UNION All
																															SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																															FROM TRN.InventoryReceiveDetail IRD	
																															LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																															UNION All
																															SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																															FROM TRN.InventoryReceiveDetail IRD	
																															LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																													)x
																													WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                    decimal RemainingGRNQty = Convert.ToDecimal((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;


                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }

                                    //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                    if (item.TransactionUoMId == issue.TransactionUoMId)
                                    {

                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty

                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                    }
                                    else
                                    {
                                        detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                    }
                                    //}
                                }
                                if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                currentId++;
                                totalGRNQty = issue.TransactionQty;
                                var detail = new PhysicalStockAdjustmentDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    PhysicalStockAdjustmentMasterID = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,
                                                          //InventoryIssue = inventoryIssue,
                                    InventoryMaterialId = issue.InventoryMaterialId,
                                    TransactionQty = issue.TransactionQty,
                                    BaseQty = issue.BaseQty,
                                    BaseUOMId = issue.BaseUOMId,
                                    TransactionUoMId = issue.TransactionUoMId,
                                    AvgRate = issue.AvgRate,
                                    AvgAmount = issue.TransactionQty * issue.AvgRate,
                                    Policy = receiveDetailRow.Policy,
                                    //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                                    //PolicyRate = detailtrnAmount / totalGRNQty,
                                    PolicyAmount = detailtrnAmount,
                                    PolicyRate = detailtrnAmount / totalGRNQty,
                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),

                                    CostCenterId = issue.CostCenterId,
                                    ModelState = ModelState.Added

                                    //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                                    //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                                };
                                var historyId = _PhysicalStockAdjustmentHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PhysicalStockAdjustmentHistory] WHERE PhysicalStockAdjustmentDetailId='{detail.Id}'").First();
                                // single entry (history)
                                //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                //if (issue.BaseQty <= (((receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty) - receiveDetailRow.PurchaseReturnQty) + receiveDetailRow.IssueReturnQty) - receiveDetailRow.ReductionByAdjustmentQty)
                                //{
                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                {
                                    historyId++;
                                    var history = new PhysicalStockAdjustmentHistory
                                    {
                                        Id = MakePK(detail.Id, historyId, 2),
                                        PhysicalStockAdjustmentDetailId = detail.Id,
                                        InventoryReceiveDetailId = receiveDetailRow.Id,
                                        //Qty = issue.TransactionQty,
                                        //Rate = Convert.ToDecimal(issue.BaseRate),
                                        Qty = issue.TransactionQty,
                                        Rate = SelectedGRN.TotalAmount / SelectedGRN.Qty,
                                        TotalAmount = SelectedGRN.TotalAmount,
                                        IsCapitalize = false,
                                        IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                        IssueReturnQty = 0
                                    };
                                    //detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                                    //detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);
                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET ReductionByAdjustmentQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.ReductionByAdjustmentQty) + Convert.ToDecimal(issue.TransactionQty)) + @"'	  WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";
                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _PhysicalStockAdjustmentHistoryRepository.Insert(history);


                                }
                                // multiple entry (history)
                                else
                                {
                                    var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                    var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                               - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                                    //if (tqty < input.TransactionQty) throw new CustomException("Stock 0");
                                    if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                    decimal policyAmount = 0;
                                    //decimal qtyDifference = input.TransactionQty;
                                    decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                    foreach (var item in rdList)
                                    {
                                        historyId++;
                                        if (item.TransactionUoMId != item.BaseUOMId)
                                            //input.BaseRate = item.BaseAmount / item.BaseQty;
                                            issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                        //else input.BaseRate = item.TransactionRate;
                                        else issue.BaseRate = item.MaterialTranRate;

                                        var issueQty = Convert.ToDecimal(item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + Convert.ToDecimal(item.IssueReturnQty));
                                        // (10 - 3)//Issueable Qty
                                        //if (issueQty != 0)
                                        //{

                                        if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                        {
                                            policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                            qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                        }
                                        else // (6 < 7) (qtyDifference < issueQty)
                                        {
                                            //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                            policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                            qtyDifference = 0;
                                        }
                                        SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                        var history = new PhysicalStockAdjustmentHistory
                                        {
                                            Id = MakePK(detail.Id, historyId, 2),
                                            PhysicalStockAdjustmentDetailId = detail.Id,
                                            InventoryReceiveDetailId = item.Id,
                                            //Qty = Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO
                                            //Qty = Convert.ToDecimal(issueQty),//TODO
                                            // Qty = Convert.ToDecimal(qtyDifference),//TODO
                                            //Rate = Convert.ToInt32(issue.BaseRate),
                                            Qty = SelectedGRN.Qty,
                                            Rate = SelectedGRN.TotalAmount / SelectedGRN.Qty,
                                            TotalAmount = SelectedGRN.TotalAmount,
                                            IsCapitalize = false,
                                            IssueRequestDetailId = receiveDetailRow.IssueRequest
                                        };

                                        AuditService.AddedLog(history);
                                        _PhysicalStockAdjustmentHistoryRepository.Insert(history);

                                        builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET ReductionByAdjustmentQty='" + Convert.ToDecimal(SelectedGRN.Qty) + "'  WHERE Id='" + item.Id + "'";
                                        rdBuilder.Append(builderSql);
                                        if (qtyDifference == 0)
                                            break;
                                        //}
                                    }

                                    detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                                    detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                                }
                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - SelectedGRN.Qty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                                rdBuilder.Append(builderSql);
                                AuditService.AddedLog(detail);
                                _PhysicalStockAdjustmentDetailRepository.Insert(detail);
                            }
                        }
                        if (specificStockList.IsNotNull())
                        {

                            foreach (var invMaterialId in specificInvaterialIds)
                            {
                                var invMaterial = _PhysicalStockAdjustmentHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                var totalReqQty = 0M;

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;
                                /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* TrnRate)*/
                                /*Rate= Amount/Sum GRN Qty */

                                foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                {
                                    //	decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[PhysicalStockAdjustmentHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                    //	if (item.TransactionUoMId == entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                    //	{
                                    //		detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.ReductionByAdjustmentQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                    //		totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                    //	}
                                    //	else
                                    //	{
                                    //		detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                    //		totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                    //	}
                                    //}
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*BaseRate),0) FROM [TRN].[InventorySalesHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_PhysicalStockAdjustmentHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(ISH.TotalBaseAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(IIH.TotalAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                    //																								FROM TRN.InventoryReceiveDetail IRD
                                    //																								left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                    //																								LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                    //																								LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                    //																								LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                    //																								LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                    //																								LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                    //																							    WHERE  IRD.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                    if (item.TransactionUoMId == entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                    {
                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);


                                        totalGRNQty += Convert.ToDecimal(item.RequisitionQty);

                                    }
                                    else
                                    {
                                        detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                    }
                                }

                                currentId++;
                                var issueDetail = new PhysicalStockAdjustmentDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    PhysicalStockAdjustmentMasterID = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,
                                                          //InventoryIssue = inventoryIssue,
                                    InventoryMaterialId = invMaterialId,
                                    BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                    TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                                    AvgRate = invMaterial.AvgRate,
                                    Policy = "N/A",
                                    ModelState = ModelState.Added,
                                    TransactionQty = stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
                                    PolicyAmount = detailtrnAmount,
                                    PolicyRate = detailtrnAmount / totalGRNQty,
                                    BaseQty = totalGRNQty,
                                    AvgAmount = totalGRNQty * invMaterial.AvgRate,
                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),


                                };

                                var historyId = _PhysicalStockAdjustmentHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PhysicalStockAdjustmentHistory] WHERE PhysicalStockAdjustmentDetailId='{issueDetail.Id}'").First();
                                foreach (var item in stockList)
                                {

                                    if (item.RequisitionQty > item.StockQty) throw new CustomException("Adjustment qty can't greater stock qty.");

                                    if (item.TransactionUoMId != item.BaseUOMId)
                                        totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                                    else
                                        totalReqQty = item.RequisitionQty;
                                    historyId++;
                                    var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                    var history = new PhysicalStockAdjustmentHistory
                                    {
                                        Id = MakePK(issueDetail.Id, historyId, 2),
                                        PhysicalStockAdjustmentDetailId = issueDetail.Id,
                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                        //Qty = item.RequisitionQty,
                                        //Rate = Convert.ToDecimal(item.BaseRate),
                                        Qty = item.RequisitionQty,//item.RequisitionQty,
                                        Rate = SelectedGRN.TotalAmount / item.RequisitionQty,
                                        TotalAmount = SelectedGRN.TotalAmount,
                                        IssueRequestDetailId = item.IssueRequest,
                                        IssueReturnQty = 0
                                    };
                                    //policyAmmount += history.Qty * history.Rate;
                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET ReductionByAdjustmentQty='" + Convert.ToDecimal(SelectedGRN.Qty + item.ReductionByAdjustmentQty) + "'  WHERE Id = '" + item.InventoryReceiveDetailId + "'";//+ item.IssueQty

                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _PhysicalStockAdjustmentHistoryRepository.Insert(history);

                                }

                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
                                rdBuilder.Append(builderSql);
                                AuditService.AddedLog(issueDetail);
                                _PhysicalStockAdjustmentDetailRepository.Insert(issueDetail);

                            }
                        }

                        //_sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                    }
                    catch (CustomException)
                    {
                        throw;
                    }
                    #endregion



                    _unitOfWork.SaveChanges();
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    flag = false;
                    _unitOfWork.Commit();
                }
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
        
        #endregion Physical Stock Adjustment 



        #region Inventory sales
        public IEnumerable<object> GetDataByInventorySales(string plantId, string tabType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
               
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

       
        public IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy)
        {

            var sql = "";
            try
            {
                //var DailySendMailRequisition = _notificationSetting.SqlQuery<bool>(@"Select NotificationAfterCreation  from NotificationSetting Where BusinessFlow = 'MaterialRequistion'").FirstOrDefault();
                //if (DailySendMailRequisition == true)
                //{
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (CheckedBy == "true" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventorySalesCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventorySalesApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "false")
                {
                    sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventorySalesApproveBy' and  A.ActionStatus='InventorySalesCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCheckedByAndApprovedBYScrap(string CheckedBy, string ApprovedBy)
        {

            var sql = "";
            try
            {
                //var DailySendMailRequisition = _notificationSetting.SqlQuery<bool>(@"Select NotificationAfterCreation  from NotificationSetting Where BusinessFlow = 'MaterialRequistion'").FirstOrDefault();
                //if (DailySendMailRequisition == true)
                //{
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (CheckedBy == "true" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventoryScrapCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventoryScrapApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "false")
                {

                }
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public void InsertGraphInventorySales(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventorySales inventoryIssue, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<InventorySalesTax> taxCategoryList, string productNewId, decimal ToCurrencyRate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var historyIdSaved = "";
            var historyIdSavedInventoryReceiveDetailId = "";
            var InventoryReceiveDetailId = "";
            var flag = false;
            bool FlagIsAsset = false;
            var MAterialGUID = "";
            var GRNCalculateList = new List<InventorySalesHistory>();
            inventoryIssue.ToCurrencyRate = ToCurrencyRate;
            if (IssueTypeStatus.ToString() == "Inventory")
            {
                FlagIsAsset = false;
            }
            else
            {
                FlagIsAsset = true;
            }
            try
            {
                if (identity.EmployeeId == inventoryIssue.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {

                    inventoryIssue.ApprovedBy = inventoryIssue.CheckedBy;
                    inventoryIssue.ApprovedByStatus = "For Approval";
                    inventoryIssue.CheckedBy = null;
                    inventoryIssue.CheckedByStatus = null;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    inventoryIssue.CheckedByStatus = null;
                    inventoryIssue.ApprovedByStatus = null;
                    inventoryIssue.CheckedBy = null;
                    inventoryIssue.ApprovedBy = null;
                }
                else
                {
                    inventoryIssue.CheckedBy = inventoryIssue.CheckedBy;
                    inventoryIssue.CheckedByStatus = "For Checking";
                    inventoryIssue.ApprovedBy = null;
                    inventoryIssue.ApprovedByStatus = null;

                }
                //item.RequisitionStatus = null;
                //item.ReqEmpId = identity.EmployeeId;
                //item.InActive = false;
                if (entities.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var _pk = GetPK3();
                    var inventoryMaterialList = _inventoryMaterialService.GetInventoryMaterialListByUpToSkuSales(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                    //var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
                    foreach (var item in entities)// update view model (inventory material field)
                    {
                        if (!string.IsNullOrEmpty(productNewId))
                        {
                            inventoryIssue.MaterialStorageId = item.MaterialStorageId;
                            inventoryIssue.SalesDate = Convert.ToDateTime(item.IssueDate);
                            inventoryIssue.Id = item.InventoryIssueId;
                        }

                        var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                && t.FirstCharacteristicsId == item.FirstCharacteristicsId && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                && t.SecondCharacteristicsId == item.SecondCharacteristicsId && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                                && t.ThirdCharacteristicsId == item.ThirdCharacteristicsId && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                                && t.CountryId == item.CountryId
                                && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId // && t.CountryId == item.CountryId
                               );
                        if (im.IsNotNull())
                        {

                            if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                            item.InventoryIssueId = _pk;
                            item.InventoryMaterialId = im.Id;
                            item.CompanyGroupId = im.CompanyGroupId;
                            item.CompanyId = inventoryIssue.CompanyId;
                            item.PlantId = inventoryIssue.PlantId;
                            item.CurrencyId = inventoryIssue.CurrencyId;
                            item.MaterialStorageId = null;
                            item.MaterialMasterId = im.MaterialMasterId;
                            item.ArticleId = im.ArticleId;
                            item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                            item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                            item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                            item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                            item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                            item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                            item.TotalQty = im.TotalQty;
                            item.AvgRate = im.AvgRate;

                        }


                    }// update view model (inventory material field)
                     //foreach (var item1 in entities)// update view model (inventory material field)
                     //{

                    //	//inventoryIssue.CustomerId = inventoryIssue.PlantId;
                    //	if (string.IsNullOrEmpty(item1.InventoryIssueId))
                    //	{
                    //		inventoryIssue.CurrencyId = currencyId;
                    //		inventoryIssue.Id = _pk;
                    //		AuditService.AddedLog(inventoryIssue);
                    //		_InventorySalesRepository.Insert(inventoryIssue);
                    //	}
                    //	else
                    //	{

                    //	}
                    //}

                    if (string.IsNullOrEmpty(productNewId))
                    {
                        inventoryIssue.CurrencyId = inventoryIssue.CurrencyId;
                        inventoryIssue.Id = _pk;
                        AuditService.AddedLog(inventoryIssue);
                        _InventorySalesRepository.Insert(inventoryIssue);
                    }

                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    //_issueDetailService.InsertRange(entities, specificStockList, inventoryIssue);


                    #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======
                    try
                    {

                        var uiList = entities.ToList();
                        var currentId = _InventorySalesHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesDetail] WHERE InventorySalesId='{inventoryIssue.Id}'").First();
                        var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                        var specificInvaterialIds = new string[] { };
                        var maIds = new string[] { };
                        if (specificStockList.IsNotNull())
                        {
                            specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                            maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                            for (int i = uiList.Count() - 1; i >= 0; i--)
                            {
                                var row = uiList.ElementAt(i);
                                if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                    uiList.RemoveAt(i);
                            }
                        }
                        else maIds = inventoryMaterialIds;
                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.InventorySalesQty,IRD.InventoryScrapQty,IRD.PurchaseReturnQty,IRD.IssueReturnQty,IRD.ReductionByAdjustmentQty,isnull(IRD.InventoryTransferQty,0) InventoryTransferQty
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
                                          AND IR.Status='Posting' 
                                          AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.GRNDate AS DATE)<=CAST('" + inventoryIssue.SalesDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                        if (receiveDetailList.IsNotNull())
                        {
                            foreach (var issue in uiList)
                            {

                                var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;
                                /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* (TotalMmaterialTrnAmount/MaterialTrnRate))*/
                                /*Rate= Amount/Sum GRN Qty */

                                if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                                    //input.BaseRate = receiveDetailRow.BaseAmount / receiveDetailRow.BaseQty;
                                    issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                if (issue.TransactionUoMId != issue.BaseUOMId)
                                    issue.BaseQty = Convert.ToInt16(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);
                                decimal IssueTransactionQty = issue.TransactionQty;
                                //foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                //    {
                                //    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                //    //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                //    if (item.TransactionUoMId == issue.TransactionUoMId)
                                //        {
                                //        detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount/item.TransactionQty)));
                                //        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                //        }
                                //        else
                                //        {
                                //        detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                //        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                //    }
                                //    }
                                foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                {

                                    if (IssueTransactionQty <= 0)
                                        break;

                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(ISH.TotalBaseAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(IIH.TotalAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                    //																							FROM TRN.InventoryReceiveDetail IRD
                                    //																							left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                    //																						    WHERE  IRD.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																							FROM (
																									SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																									FROM TRN.InventoryReceiveDetail IRD
																									left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																									UNION All
																									SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																									FROM TRN.InventoryReceiveDetail IRD	
																									LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																									UNION All
																									SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																									FROM TRN.InventoryReceiveDetail IRD	
																									LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																									UNION All
																									SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																									FROM TRN.InventoryReceiveDetail IRD	
																									LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																									UNION All
																									SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																									FROM TRN.InventoryReceiveDetail IRD	
																									LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																									UNION All
																									SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																									FROM TRN.InventoryReceiveDetail IRD	
																									LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																									UNION All
																									SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																									FROM TRN.InventoryReceiveDetail IRD	
																									LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																							)x
																							WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                    decimal RemainingGRNQty = Convert.ToDecimal((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;


                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }

                                    //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                    if (item.TransactionUoMId == issue.TransactionUoMId)
                                    {

                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventorySalesHistory
                                        {
                                            TotalBaseAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty

                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                    }
                                    else
                                    {
                                        //detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventorySalesHistory
                                        {
                                            //TotalBaseAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty + item.InventoryTransferQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            TotalBaseAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                    }
                                    //}
                                }
                                if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                currentId++;
                                //totalGRNQty = issue.TransactionQty;
                                var detail = new InventorySalesDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    InventorySalesId = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,
                                                          //InventoryIssue = inventoryIssue,
                                    InventoryMaterialId = issue.InventoryMaterialId,
                                    TransactionQty = issue.TransactionQty,
                                    BaseQty = issue.BaseQty,
                                    BaseUOMId = issue.BaseUOMId,
                                    TransactionUoMId = issue.TransactionUoMId,
                                    AvgRate = Math.Round(issue.AvgRate, 4),
                                    AvgAmount = Math.Round(issue.TransactionQty * issue.AvgRate, 2),
                                    Policy = receiveDetailRow.Policy,
                                    //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                                    //PolicyRate = detailtrnAmount / totalGRNQty,

                                    PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
                                    PolicyAmount = Math.Round(detailtrnAmount, 2),

                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    Comments = issue.Comments,
                                    CostCenterId = issue.CostCenterId,
                                    SalesRate = issue.SalesRate,
                                    TotalSalesAmount = Math.Round((issue.TransactionQty * issue.SalesRate), 2),
                                    BooksCurrencyTransactionAmount = Math.Round((inventoryIssue.ToCurrencyRate * Math.Round((issue.TransactionQty * issue.SalesRate), 2)), 2),
                                    ModelState = ModelState.Added

                                    //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                                    //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                                };
                                var historyId = _InventorySalesHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesHistory] WHERE InventorySalesDetailId='{detail.Id}'").First();
                                // single entry (history)
                                //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                {
                                    historyId++;
                                    var history = new InventorySalesHistory
                                    {
                                        Id = MakePK(detail.Id, historyId, 2),
                                        InventorySalesDetailId = detail.Id,
                                        InventoryReceiveDetailId = receiveDetailRow.Id,
                                        Qty = SelectedGRN.Qty,//issue.TransactionQty,
                                        BaseRate = SelectedGRN.TotalBaseAmount / SelectedGRN.Qty,//Convert.ToDecimal(issue.BaseRate),
                                        TotalBaseAmount = SelectedGRN.TotalBaseAmount,//Convert.ToDecimal(detailtrnAmount),
                                                                                      //SalesRate = Convert.ToDecimal(issue.SalesRate),
                                                                                      //TotalAmount = Convert.ToDecimal(Convert.ToDecimal(issue.TransactionQty) * Convert.ToDecimal(issue.SalesRate)), //Convert.ToDecimal(issue.TotalAmount),
                                                                                      //Rate = SelectedGRN.TotalAmount / SelectedGRN.Qty,										
                                        IsCapitalize = false,
                                        IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                        IssueReturnQty = 0,
                                        BooksCurrencyBaseAmount = Math.Round((Math.Round(SelectedGRN.TotalBaseAmount)), 2)
                                    };
                                    //detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                                    //detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);

                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET InventorySalesQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.InventorySalesQty) + Convert.ToDecimal(issue.TransactionQty)) + @"'
									  WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";
                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _InventorySalesHistoryRepository.Insert(history);
                                    if (issue.FirstCharacteristicsValueId == null) issue.FirstCharacteristicsValueId = "undefined";
                                    if (issue.SecondCharacteristicsValueId == null) issue.SecondCharacteristicsValueId = "undefined";
                                    if (issue.ThirdCharacteristicsValueId == null) issue.ThirdCharacteristicsValueId = "undefined";
                                    MAterialGUID = issue.MaterialMasterId + issue.ArticleId + issue.FirstCharacteristicsValueId + issue.SecondCharacteristicsValueId + issue.ThirdCharacteristicsValueId;
                                    if (taxCategoryList.IsNotNull())
                                    {
                                        var currentTaxId = _InventorySalesTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesTax] WHERE InventoryReceiveDetailId='{history.InventoryReceiveDetailId}'").First();
                                        foreach (var itemTax in taxCategoryList)
                                        {
                                            if (MAterialGUID == itemTax.Id)
                                            {
                                                currentId++;
                                                itemTax.Id = GetInventorySalesTaxPK();
                                                itemTax.InventorySalesHistoryId = history.Id;
                                                itemTax.InventoryReceiveDetailId = history.InventoryReceiveDetailId;
                                                itemTax.TaxCategoryId = itemTax.TaxCategoryId;
                                                itemTax.HSNCodeId = itemTax.HSNCodeId;
                                                itemTax.Percentage = itemTax.Percentage;
                                                itemTax.TaxAmount = itemTax.TaxAmount;
                                                itemTax.InventorySalesId = inventoryIssue.Id;

                                                itemTax.BooksCurrencyTaxAmount = Math.Round((inventoryIssue.ToCurrencyRate * itemTax.TaxAmount), 2);

                                                AuditService.AddedLog(itemTax);
                                                _InventorySalesTaxRepository.Insert(itemTax);
                                            }//sk
                                        }
                                    }

                                }
                                // multiple entry (history)
                                else
                                {
                                    var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                    var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                               - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                                    //if (tqty < input.TransactionQty) throw new CustomException("Stock 0");
                                    if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                    decimal policyAmount = 0;
                                    //decimal qtyDifference = input.TransactionQty;
                                    decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                    foreach (var item in rdList)
                                    {
                                        historyId++;
                                        if (item.TransactionUoMId != item.BaseUOMId)
                                            //input.BaseRate = item.BaseAmount / item.BaseQty;
                                            issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                        //else input.BaseRate = item.TransactionRate;
                                        else issue.BaseRate = item.MaterialTranRate;

                                        var issueQty = Convert.ToDecimal(item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + Convert.ToDecimal(item.IssueReturnQty));
                                        // (10 - 3)//Issueable Qty
                                        //if (issueQty != 0)
                                        //{

                                        if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                        {
                                            policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                            qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                        }
                                        else // (6 < 7) (qtyDifference < issueQty)
                                        {
                                            //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                            policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                            qtyDifference = 0;
                                        }
                                        SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                        var history = new InventorySalesHistory
                                        {
                                            Id = MakePK(detail.Id, historyId, 2),
                                            InventorySalesDetailId = detail.Id,
                                            InventoryReceiveDetailId = item.Id,
                                            Qty = SelectedGRN.Qty,//Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO			
                                                                  //BaseRate = SelectedGRN.TotalBaseAmount / SelectedGRN.Qty,//Convert.ToInt32(issue.BaseRate),
                                                                  //TotalBaseAmount = Math.Round(SelectedGRN.TotalBaseAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                                  //														 //SalesRate = Convert.ToDecimal(issue.SalesRate),
                                                                  //														 //TotalAmount = Convert.ToDecimal(Convert.ToDecimal(issueQty) * Convert.ToDecimal(issue.SalesRate)),//Convert.ToDecimal(issue.TotalAmount),											
                                                                  //IssueReturnQty = 0,
                                                                  //IsCapitalize = false,
                                                                  //IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                                                  //BooksCurrencyBaseAmount = Math.Round((inventoryIssue.ToCurrencyRate * SelectedGRN.TotalBaseAmount), 2)
                                            BaseRate = SelectedGRN.TotalBaseAmount / SelectedGRN.Qty,//Convert.ToDecimal(issue.BaseRate),
                                            TotalBaseAmount = Math.Round(SelectedGRN.TotalBaseAmount, 2), //SelectedGRN.TotalBaseAmount,//Convert.ToDecimal(detailtrnAmount),
                                                                                                          //SalesRate = Convert.ToDecimal(issue.SalesRate),
                                                                                                          //TotalAmount = Convert.ToDecimal(Convert.ToDecimal(issue.TransactionQty) * Convert.ToDecimal(issue.SalesRate)), //Convert.ToDecimal(issue.TotalAmount),
                                                                                                          //Rate = SelectedGRN.TotalAmount / SelectedGRN.Qty,										
                                            IsCapitalize = false,
                                            IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                            IssueReturnQty = 0,
                                            BooksCurrencyBaseAmount = Math.Round((SelectedGRN.TotalBaseAmount), 2)
                                        };

                                        AuditService.AddedLog(history);
                                        _InventorySalesHistoryRepository.Insert(history);

                                        builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET InventorySalesQty='" + Convert.ToDecimal(Convert.ToDecimal(item.InventorySalesQty) + Convert.ToDecimal(SelectedGRN.Qty)) + "'  WHERE Id='" + item.Id + "'";
                                        rdBuilder.Append(builderSql);
                                        if (qtyDifference == 0)
                                            break;
                                        //}

                                        historyIdSaved = history.Id;
                                        InventoryReceiveDetailId = history.InventoryReceiveDetailId;

                                    }

                                    //detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                                    //detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                                }

                                if (issue.FirstCharacteristicsValueId == null) issue.FirstCharacteristicsValueId = "undefined";
                                if (issue.SecondCharacteristicsValueId == null) issue.SecondCharacteristicsValueId = "undefined";
                                if (issue.ThirdCharacteristicsValueId == null) issue.ThirdCharacteristicsValueId = "undefined";
                                MAterialGUID = issue.MaterialMasterId + issue.ArticleId + issue.FirstCharacteristicsValueId + issue.SecondCharacteristicsValueId + issue.ThirdCharacteristicsValueId;

                                if (taxCategoryList.IsNotNull())
                                {
                                    var currentTaxId = _InventorySalesTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesTax] WHERE InventoryReceiveDetailId='{InventoryReceiveDetailId}'").First();
                                    foreach (var itemTax in taxCategoryList)
                                    {
                                        if (MAterialGUID == itemTax.Id)
                                        {
                                            currentId++;
                                            itemTax.Id = GetInventorySalesTaxPK();
                                            itemTax.InventorySalesHistoryId = historyIdSaved;
                                            itemTax.InventoryReceiveDetailId = InventoryReceiveDetailId;
                                            itemTax.TaxCategoryId = itemTax.TaxCategoryId;
                                            itemTax.HSNCodeId = itemTax.HSNCodeId;
                                            itemTax.Percentage = itemTax.Percentage;
                                            itemTax.TaxAmount = itemTax.TaxAmount;
                                            itemTax.InventorySalesId = inventoryIssue.Id;
                                            itemTax.BooksCurrencyTaxAmount = Math.Round((inventoryIssue.ToCurrencyRate * itemTax.TaxAmount), 2);
                                            AuditService.AddedLog(itemTax);
                                            _InventorySalesTaxRepository.Insert(itemTax);
                                        }
                                    }
                                }
                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - SelectedGRN.Qty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                                rdBuilder.Append(builderSql);
                                AuditService.AddedLog(detail);
                                _InventorySalesDetailRepository.Insert(detail);


                            }
                        }
                        if (specificStockList.IsNotNull())
                        {
                            //foreach (var invMaterialId in specificStockList)
                            //{
                            //    var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId.InventoryMaterialId + "'").FirstOrDefault();
                            //    var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId.InventoryMaterialId).ToList();
                            //    var totalReqQty = 0M;
                            //    decimal policyAmmount = 0;
                            //    currentId++;
                            //    var issueDetail = new InventoryIssueDetail();
                            //    issueDetail.Id = MakePK(inventoryIssue.Id, currentId, 2);
                            //    issueDetail.InventoryIssueId = inventoryIssue.Id;
                            //    //issueDetail.//InventoryIssue = inventoryIssue,
                            //    issueDetail.InventoryMaterialId = invMaterialId.InventoryMaterialId;
                            //    issueDetail.BaseUOMId = stockList.Select(t => t.BaseUOMId).FirstOrDefault();
                            //    issueDetail.AvgRate = invMaterial.AvgRate;
                            //    issueDetail.Policy = "N/A";
                            //    issueDetail.ModelState = ModelState.Added;
                            //    issueDetail.InventoryReceiveId = stockList.Select(t => t.InventoryReceiveId).FirstOrDefault();
                            //    issueDetail.InventoryReceiveDetailId = stockList.Select(t => t.InventoryReceiveDetailId).FirstOrDefault();
                            //    issueDetail.TransactionQty = stockList.Select(t => t.RequisitionQty).FirstOrDefault();
                            //    issueDetail.BaseQty = stockList.Select(t => t.BaseIssueQty).FirstOrDefault();

                            //    var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                            //    foreach (var item in stockList)
                            //    {
                            //        if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                            //        if (item.TransactionUoMId != item.BaseUOMId)
                            //            item.RequisitionQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                            //        totalReqQty += item.RequisitionQty;
                            //        historyId++;
                            //        var history = new InventoryIssueHistory
                            //        {
                            //            Id = MakePK(issueDetail.Id, historyId, 2),
                            //            InventoryIssueDetailId = issueDetail.Id,
                            //            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                            //            Qty = item.RequisitionQty,
                            //            Rate = Convert.ToDecimal(item.BaseRate)
                            //        };
                            //        policyAmmount += history.Qty * history.Rate;
                            //        builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + "' WHERE Id='" + item.InventoryReceiveDetailId + "'";
                            //        rdBuilder.Append(builderSql);
                            //        AuditService.AddedLog(history);
                            //        _issueHistoryRepository.Insert(history);
                            //    }
                            //    issueDetail.PolicyRate = Convert.ToDecimal(policyAmmount / totalReqQty);
                            //    issueDetail.PolicyAmount = Convert.ToDecimal(policyAmmount);
                            //    issueDetail.TransactionQty = totalReqQty;
                            //    issueDetail.AvgAmount = totalReqQty * invMaterial.AvgRate;

                            //    builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - totalReqQty) + "' WHERE Id='" + invMaterialId + "'";
                            //    rdBuilder.Append(builderSql);
                            //    AuditService.AddedLog(issueDetail);
                            //    InsertGraph(issueDetail);
                            //}
                            foreach (var invMaterialId in specificInvaterialIds)
                            {
                                var invMaterial = _InventorySalesHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                var totalReqQty = 0M;
                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;
                                /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* TrnRate)*/
                                /*Rate= Amount/Sum GRN Qty */

                                foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                {
                                    decimal IssueTransactionQty = item.RequisitionQty;
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*BaseRate),0) FROM [TRN].[InventorySalesHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(ISH.TotalBaseAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(IIH.TotalAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                    //																							FROM TRN.InventoryReceiveDetail IRD
                                    //																							left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                    //																						    WHERE  IRD.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                    //if (item.TransactionUoMId == entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                    //{
                                    //	detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                    //	totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                    //}
                                    //else
                                    //{
                                    //	detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                    //	totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                    //}

                                    decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;


                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }
                                    if (item.TransactionUoMId == item.BaseUOMId) //entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                    {
                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));



                                        var newgrn = new InventorySalesHistory
                                        {
                                            TotalBaseAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);


                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                    }
                                    else
                                    {
                                        //detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty * item.BaseUoMFactor) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));



                                        var newgrn = new InventorySalesHistory
                                        {
                                            //TotalBaseAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            TotalBaseAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) + item.IssueReturnQty) - IssueDeduactionQty * item.BaseUoMFactor) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor);
                                    }
                                }

                                currentId++;
                                var issueDetail = new InventorySalesDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    InventorySalesId = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,
                                                          //InventoryIssue = inventoryIssue,
                                    InventoryMaterialId = invMaterialId,
                                    BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                    TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),

                                    Policy = "N/A",
                                    //Policy = receiveDetailRow.Policy,

                                    TransactionQty = Math.Round(totalGRNQty, 2),//stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
                                                                                //PolicyAmount = detailtrnAmount,
                                                                                //PolicyRate = detailtrnAmount / totalGRNQty,
                                    PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
                                    PolicyAmount = Math.Round(detailtrnAmount, 2),//Math.Round((detailtrnAmount / totalGRNQty) * stockList.Sum(r => r.RequisitionQty), 2),
                                    BaseQty = Math.Round(totalGRNQty, 2),//stockList.Sum(r => r.RequisitionQty),
                                    AvgRate = Math.Round(invMaterial.AvgRate, 4),
                                    AvgAmount = Math.Round(totalGRNQty * invMaterial.AvgRate, 2),


                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
                                    Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),

                                    //SalesRate = issue.SalesRate,
                                    //TotalSalesAmount = Math.Round((issue.TransactionQty * issue.SalesRate), 2),
                                    //BooksCurrencyTransactionAmount = Math.Round((inventoryIssue.ToCurrencyRate * Math.Round((issue.TransactionQty * issue.SalesRate), 2)), 2),

                                    SalesRate = Math.Round(entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId && r.FirstCharacteristicsValueId == invMaterial.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == invMaterial.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == invMaterial.ThirdCharacteristicsValueId).Select(t => t.SalesRate).FirstOrDefault(), 4),//Math.Round((stockList.Sum(r => r.SalesRate)), 4),///item.SalesRate,

                                    TotalSalesAmount = Math.Round(entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId && r.FirstCharacteristicsValueId == invMaterial.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == invMaterial.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == invMaterial.ThirdCharacteristicsValueId).Select(t => t.SalesRate).FirstOrDefault() * stockList.Sum(r => r.RequisitionQty), 2), //Math.Round((stockList.Sum(r => r.RequisitionQty) * Math.Round((stockList.Sum(r => r.SalesRate)), 4)), 2),
                                    BooksCurrencyTransactionAmount = Math.Round(inventoryIssue.ToCurrencyRate * (entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId && r.FirstCharacteristicsValueId == invMaterial.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == invMaterial.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == invMaterial.ThirdCharacteristicsValueId).Select(t => t.SalesRate).FirstOrDefault() * stockList.Sum(r => r.RequisitionQty)), 2), //Math.Round(inventoryIssue.ToCurrencyRate * Math.Round((stockList.Sum(r => r.RequisitionQty) * Math.Round((stockList.Sum(r => r.SalesRate)), 4)), 2), 2),
                                    ModelState = ModelState.Added
                                };
                                var SalesDetailId = issueDetail.Id;
                                var historyId = _InventorySalesHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesHistory] WHERE InventorySalesDetailId='{issueDetail.Id}'").First();
                                foreach (var item in stockList)
                                {

                                    if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                                    if (item.TransactionUoMId != item.BaseUOMId)
                                        totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                                    else
                                        totalReqQty = item.RequisitionQty;
                                    historyId++;
                                    var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                    var history = new InventorySalesHistory
                                    {
                                        Id = MakePK(issueDetail.Id, historyId, 2),
                                        InventorySalesDetailId = issueDetail.Id,
                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,

                                        Qty = totalReqQty,//item.RequisitionQty,
                                        BaseRate = Math.Round((SelectedGRN.TotalBaseAmount / totalReqQty), 4), //Math.Round(SelectedGRN.TotalBaseAmount / totalGRNQty, 4),//Math.Round(Convert.ToDecimal(item.BaseRate), 4),//SelectedGRN.TotalBaseAmount / item.RequisitionQty,//Convert.ToDecimal(item.BaseRate),
                                        TotalBaseAmount = Math.Round(SelectedGRN.TotalBaseAmount, 2), //SelectedGRN.TotalBaseAmount,//Convert.ToDecimal(detailtrnAmount),
                                                                                                      //SalesRate = Convert.ToDecimal(item.SalesRate),
                                                                                                      //TotalAmount = Convert.ToDecimal(item.TotalAmount),									
                                                                                                      //Rate = Convert.ToDecimal(item.BaseRate),	
                                        IssueRequestDetailId = item.IssueRequest,
                                        IssueReturnQty = 0,
                                        BooksCurrencyBaseAmount = Math.Round(SelectedGRN.TotalBaseAmount, 2),
                                    };
                                    //policyAmmount += history.Qty * history.BaseRate;
                                    historyIdSaved = history.Id;
                                    historyIdSavedInventoryReceiveDetailId = history.InventoryReceiveDetailId;
                                    var invMaterial1 = _InventorySalesRepository.SqlQuery<InventoryReceiveDetail>(@"SELECT * FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault();


                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET InventorySalesQty='" + Convert.ToDecimal(item.RequisitionQty + invMaterial1.InventorySalesQty) + @"' 
										 WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _InventorySalesHistoryRepository.Insert(history);

                                    if (invMaterial.FirstCharacteristicsValueId == null) invMaterial.FirstCharacteristicsValueId = "undefined";
                                    if (invMaterial.SecondCharacteristicsValueId == null) invMaterial.SecondCharacteristicsValueId = "undefined";
                                    if (invMaterial.ThirdCharacteristicsValueId == null) invMaterial.ThirdCharacteristicsValueId = "undefined";
                                    MAterialGUID = invMaterial.MaterialMasterId + invMaterial.ArticleId + invMaterial.FirstCharacteristicsValueId + invMaterial.SecondCharacteristicsValueId + invMaterial.ThirdCharacteristicsValueId;


                                }
                                if (taxCategoryList.IsNotNull())
                                {
                                    var currentTaxId = _InventorySalesTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesTax] WHERE InventoryReceiveDetailId='{historyIdSavedInventoryReceiveDetailId}'").First();
                                    foreach (var itemTax in taxCategoryList)
                                    {
                                        if (MAterialGUID == itemTax.Id)
                                        {
                                            currentId++;
                                            itemTax.Id = GetInventorySalesTaxPK();
                                            itemTax.InventorySalesHistoryId = historyIdSaved;
                                            itemTax.InventoryReceiveDetailId = historyIdSavedInventoryReceiveDetailId;
                                            itemTax.TaxCategoryId = itemTax.TaxCategoryId;
                                            itemTax.HSNCodeId = itemTax.HSNCodeId;
                                            itemTax.Percentage = itemTax.Percentage;
                                            itemTax.TaxAmount = itemTax.TaxAmount;
                                            itemTax.InventorySalesId = inventoryIssue.Id;
                                            itemTax.InventorySalesDetailId = SalesDetailId;
                                            itemTax.BooksCurrencyTaxAmount = Math.Round((inventoryIssue.ToCurrencyRate * itemTax.TaxAmount), 2);
                                            AuditService.AddedLog(itemTax);
                                            _InventorySalesTaxRepository.Insert(itemTax);
                                        }
                                    }
                                }

                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
                                rdBuilder.Append(builderSql);

                                AuditService.AddedLog(issueDetail);
                                _InventorySalesDetailRepository.Insert(issueDetail);

                            }

                        }

                        //_sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                    }
                    catch (CustomException)
                    {
                        throw;
                    }
                    #endregion



                    _unitOfWork.SaveChanges();
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    flag = false;
                    _unitOfWork.Commit();
                }
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


        public void InsertGraphInventoryScrap(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryScrap inventoryIssue, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var GRNCalculateList = new List<InventoryIssueHistory>();
            var flag = false;
            bool FlagIsAsset = false;
            if (IssueTypeStatus.ToString() == "Inventory")
            {
                FlagIsAsset = false;
            }
            else
            {
                FlagIsAsset = true;
            }
            try
            {
                if (identity.EmployeeId == inventoryIssue.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {

                    inventoryIssue.ApprovedBy = inventoryIssue.CheckedBy;
                    inventoryIssue.ApprovedByStatus = "For Approval";
                    inventoryIssue.CheckedBy = null;
                    inventoryIssue.CheckedByStatus = null;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    inventoryIssue.CheckedByStatus = null;
                    inventoryIssue.ApprovedByStatus = null;
                    inventoryIssue.CheckedBy = null;
                    inventoryIssue.ApprovedBy = null;
                }
                else
                {
                    inventoryIssue.CheckedBy = inventoryIssue.CheckedBy;
                    inventoryIssue.CheckedByStatus = "For Checking";
                    inventoryIssue.ApprovedBy = null;
                    inventoryIssue.ApprovedByStatus = null;

                }

                if (entities.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var _pk = GetPK4();
                    var inventoryMaterialList = _inventoryMaterialService.GetInventoryMaterialListByUpToSkuScrap(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                    var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
                    foreach (var item in entities)// update view model (inventory material field)
                    {
                        var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                && t.FirstCharacteristicsId == item.FirstCharacteristicsId && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                && t.SecondCharacteristicsId == item.SecondCharacteristicsId && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                                && t.ThirdCharacteristicsId == item.ThirdCharacteristicsId && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                                && t.CountryId == item.CountryId
                                && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId // && t.CountryId == item.CountryId
                               );
                        if (im.IsNotNull())
                        {

                            if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                            item.InventoryIssueId = _pk;
                            item.InventoryMaterialId = im.Id;
                            item.CompanyGroupId = im.CompanyGroupId;
                            item.CompanyId = inventoryIssue.CompanyId;
                            item.PlantId = inventoryIssue.PlantId;
                            item.CurrencyId = currencyId;
                            item.MaterialStorageId = null;
                            item.MaterialMasterId = im.MaterialMasterId;
                            item.ArticleId = im.ArticleId;
                            item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                            item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                            item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                            item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                            item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                            item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                            item.TotalQty = im.TotalQty;
                            item.AvgRate = im.AvgRate;

                        }


                    }// update view model (inventory material field)
                    inventoryIssue.CurrencyId = currencyId;
                    inventoryIssue.Id = _pk;

                    AuditService.AddedLog(inventoryIssue);
                    _InventoryScrapRepository.Insert(inventoryIssue);

                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";



                    #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======
                    try
                    {

                        var uiList = entities.ToList();
                        var currentId = _InventoryScrapHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryScrapDetail] WHERE InventoryScrapId='{inventoryIssue.Id}'").First();
                        var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                        var specificInvaterialIds = new string[] { };
                        var maIds = new string[] { };
                        if (specificStockList.IsNotNull())
                        {
                            specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                            maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                            for (int i = uiList.Count() - 1; i >= 0; i--)
                            {
                                var row = uiList.ElementAt(i);
                                if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                    uiList.RemoveAt(i);
                            }
                        }
                        else maIds = inventoryMaterialIds;
                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty
										, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor,IRD.InventorySalesQty,IRD.InventoryScrapQty,IRD.PurchaseReturnQty,IRD.IssueReturnQty,IRD.ReductionByAdjustmentQty,isnull(IRD.InventoryTransferQty,0) InventoryTransferQty
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty
										, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
                                          AND IR.Status='Posting' 
                                          AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.ScrapDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                        if (receiveDetailList.IsNotNull())
                        {
                            foreach (var issue in uiList)
                            {

                                var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;

                                if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                                    issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                if (issue.TransactionUoMId != issue.BaseUOMId)
                                    issue.BaseQty = Convert.ToInt16(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);

                                decimal IssueTransactionQty = issue.TransactionQty;
                                foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                {

                                    if (IssueTransactionQty <= 0)
                                        break;

                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(ISH.TotalBaseAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(IIH.TotalAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                    //																							FROM TRN.InventoryReceiveDetail IRD
                                    //																							left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                    //																						    WHERE  IRD.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                    decimal RemainingGRNQty = Convert.ToDecimal((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;

                                    //if (RemainingGRNQty != 0)
                                    //{
                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }

                                    //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                    if (item.TransactionUoMId == issue.TransactionUoMId)
                                    {

                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty

                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                    }
                                    else
                                    {
                                        detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) + item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                    }
                                    //}
                                    //}
                                }

                                if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                currentId++;
                                //totalGRNQty = issue.TransactionQty;
                                var detail = new InventoryScrapDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    InventoryScrapId = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,
                                                          //InventoryIssue = inventoryIssue,
                                    InventoryMaterialId = issue.InventoryMaterialId,
                                    TransactionQty = issue.TransactionQty,
                                    BaseQty = issue.BaseQty,
                                    BaseUOMId = issue.BaseUOMId,
                                    TransactionUoMId = issue.TransactionUoMId,
                                    AvgRate = issue.AvgRate,
                                    AvgAmount = issue.TransactionQty * issue.AvgRate,
                                    Policy = receiveDetailRow.Policy,
                                    //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                                    //PolicyRate = detailtrnAmount / totalGRNQty,
                                    PolicyAmount = detailtrnAmount,
                                    PolicyRate = detailtrnAmount / totalGRNQty,
                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    Comments = issue.Comments,
                                    CostCenterId = issue.CostCenterId,
                                    ModelState = ModelState.Added

                                    //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                                    //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                                };
                                var historyId = _InventoryScrapHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryScrapHistory] WHERE InventoryScrapDetailId='{detail.Id}'").First();
                                // single entry (history)
                                //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                //if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                {

                                    historyId++;
                                    var history = new InventoryScrapHistory
                                    {
                                        Id = MakePK(detail.Id, historyId, 2),
                                        InventoryScrapDetailId = detail.Id,
                                        InventoryReceiveDetailId = receiveDetailRow.Id,
                                        //Qty = issue.TransactionQty,
                                        //Rate = Convert.ToInt32(issue.BaseRate),
                                        Qty = SelectedGRN.Qty,//issue.TransactionQty,
                                        Rate = SelectedGRN.TotalAmount / SelectedGRN.Qty,
                                        TotalAmount = SelectedGRN.TotalAmount,
                                        IsCapitalize = false,
                                        IssueRequestDetailId = receiveDetailRow.IssueRequest

                                    };
                                    //detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                                    //detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);

                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET InventoryScrapQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.InventoryScrapQty) + Convert.ToDecimal(SelectedGRN.Qty)) + @"'
									  WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";
                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _InventoryScrapHistoryRepository.Insert(history);


                                }
                                // multiple entry (history)
                                else
                                {
                                    var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                    var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                               - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                                    if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                    decimal policyAmount = 0;
                                    decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                    foreach (var item in rdList)
                                    {
                                        historyId++;
                                        if (item.TransactionUoMId != item.BaseUOMId)
                                            issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                        //else input.BaseRate = item.TransactionRate;
                                        else issue.BaseRate = item.MaterialTranRate;

                                        var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty);


                                        if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                        {
                                            policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                            qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                        }
                                        else // (6 < 7) (qtyDifference < issueQty)
                                        {
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                            policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                            qtyDifference = 0;
                                        }
                                        SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                        var history = new InventoryScrapHistory
                                        {
                                            Id = MakePK(detail.Id, historyId, 2),
                                            InventoryScrapDetailId = detail.Id,
                                            InventoryReceiveDetailId = item.Id,
                                            //Qty = Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO																								 																					  
                                            //Rate = Convert.ToInt32(issue.BaseRate),

                                            Qty = SelectedGRN.Qty,
                                            Rate = SelectedGRN.TotalAmount / SelectedGRN.Qty,
                                            TotalAmount = SelectedGRN.TotalAmount,
                                            IsCapitalize = false,
                                            IssueRequestDetailId = receiveDetailRow.IssueRequest
                                        };

                                        AuditService.AddedLog(history);
                                        _InventoryScrapHistoryRepository.Insert(history);

                                        builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET InventoryScrapQty='" + Convert.ToDecimal(SelectedGRN.Qty) + "'  WHERE Id='" + item.Id + "'";
                                        rdBuilder.Append(builderSql);
                                        if (qtyDifference == 0)
                                            break;
                                        //}


                                    }

                                    //detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                                    //detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                                }
                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - SelectedGRN.Qty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                                rdBuilder.Append(builderSql);
                                AuditService.AddedLog(detail);
                                _InventoryScrapDetailRepository.Insert(detail);


                            }
                        }
                        if (specificStockList.IsNotNull())
                        {

                            foreach (var invMaterialId in specificInvaterialIds)
                            {
                                var invMaterial = _InventorySalesHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                var totalReqQty = 0M;

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;

                                foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                {
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryScrapHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                                    //if (item.TransactionUoMId == entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                    //{
                                    //	detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                    //	totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                    //}
                                    //else
                                    //{
                                    //	detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                    //	totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                    //}
                                    //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(ISH.TotalBaseAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(IIH.TotalAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                    //																							FROM TRN.InventoryReceiveDetail IRD
                                    //																							left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                    //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                    //																						    WHERE  IRD.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                    if (item.TransactionUoMId == entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                    {
                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);


                                        totalGRNQty += Convert.ToDecimal(item.RequisitionQty);

                                    }
                                    else
                                    {
                                        detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryIssueHistory
                                        {
                                            TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                    }
                                }

                                currentId++;
                                var issueDetail = new InventoryScrapDetail
                                {
                                    Id = MakePK(inventoryIssue.Id, currentId, 2),
                                    InventoryScrapId = inventoryIssue.Id,
                                    IsAsset = FlagIsAsset,//false,														 
                                    InventoryMaterialId = invMaterialId,
                                    BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                    TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                                    AvgRate = invMaterial.AvgRate,
                                    Policy = "N/A",

                                    TransactionQty = stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
                                    PolicyAmount = detailtrnAmount,
                                    PolicyRate = detailtrnAmount / totalGRNQty,
                                    BaseQty = totalGRNQty,
                                    AvgAmount = totalGRNQty * invMaterial.AvgRate,
                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
                                    Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),

                                    ModelState = ModelState.Added
                                };

                                var historyId = _InventorySalesHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryScrapHistory] WHERE InventoryScrapDetailId='{issueDetail.Id}'").First();
                                foreach (var item in stockList)
                                {

                                    if (item.RequisitionQty > item.StockQty) throw new CustomException("Scrap sales qty can't greater stock qty.");

                                    if (item.TransactionUoMId != item.BaseUOMId)
                                        totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                                    else
                                        totalReqQty = item.RequisitionQty;
                                    historyId++;
                                    var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                    var history = new InventoryScrapHistory
                                    {
                                        Id = MakePK(issueDetail.Id, historyId, 2),
                                        InventoryScrapDetailId = issueDetail.Id,
                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                        //Qty = item.RequisitionQty,
                                        //Rate = Convert.ToDecimal(item.Rate)
                                        Qty = item.RequisitionQty,
                                        Rate = SelectedGRN.TotalAmount / item.RequisitionQty,
                                        TotalAmount = SelectedGRN.TotalAmount,
                                    };
                                    //policyAmmount += history.Qty * history.Rate;

                                    var invMaterial1 = _InventoryScrapRepository.SqlQuery<InventoryReceiveDetail>(@"SELECT * FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault();


                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET InventoryScrapQty='" + Convert.ToDecimal(item.RequisitionQty + invMaterial1.InventoryScrapQty) + @"' 
										 WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _InventoryScrapHistoryRepository.Insert(history);


                                }


                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
                                rdBuilder.Append(builderSql);

                                AuditService.AddedLog(issueDetail);
                                _InventoryScrapDetailRepository.Insert(issueDetail);

                            }

                        }

                        //_sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                    }
                    catch (CustomException)
                    {
                        throw;
                    }
                    #endregion



                    _unitOfWork.SaveChanges();
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    flag = false;
                    _unitOfWork.Commit();
                }
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
        public void InsertGraph(InventoryMaterialViewModel entity, IEnumerable<InventorySalesTax> taxCategoryList)
        {
            if (Convert.ToBoolean(_InventorySalesServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.InventorySalesService WHERE InventorySalesId='" + entity.InventorySalesId + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                throw new CustomException("This service already taken."); ;

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                if (entity.IsNotNull())
                {
                    entity.TotalTaxAmount = taxCategoryList.Sum(r => r.TaxAmount);
                    entity.ToCurrencyRate = entity.ToCurrencyRate == 0 ? 1 : entity.ToCurrencyRate;
                    var currentId = _InventorySalesServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesService] WHERE InventorySalesId='{entity.InventorySalesId}'").First();
                    currentId++;
                    var service = new InventorySalesService
                    {
                        Id = MakePK(entity.InventorySalesId + 2, currentId, 2),
                        InventorySalesId = entity.InventorySalesId,
                        ServiceMasterId = entity.ServiceMasterId,
                        //Amount = Convert.ToDecimal(entity.TransactionAmount*entity.ToCurrencyRate),
                        Amount = Math.Round(Convert.ToDecimal(entity.TransactionAmount), 2),
                        TotalTaxAmount = Convert.ToDecimal(entity.TotalTaxAmount),
                        BooksCurrencyTransactionAmount = Math.Round(Convert.ToDecimal(entity.ToCurrencyRate) * Convert.ToDecimal(entity.TransactionAmount), 2),
                        BooksCurrencyTaxAmount = Math.Round(Convert.ToDecimal(entity.ToCurrencyRate) * Convert.ToDecimal(entity.TotalTaxAmount), 2),
                        //GRNServiceAmount = 0,
                        //AmountStatus = false,
                        Description = entity.Description,
                    };
                    AuditService.AddedLog(service);
                    _InventorySalesServiceRepository.Insert(service);
                    if (taxCategoryList.IsNotNull())
                    {
                        var crrId = _InventorySalesTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesTax] WHERE InventorySalesServiceId='{service.Id}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            crrId++;
                            item.Id = MakePK(service.Id, crrId, 2);
                            item.InventorySalesId = entity.InventorySalesId;
                            item.InventoryReceiveDetailId = null;
                            item.InventorySalesServiceId = service.Id;
                            item.BooksCurrencyTaxAmount = Math.Round(Convert.ToDecimal(entity.ToCurrencyRate * item.TaxAmount), 2);
                            AuditService.AddedLog(item);
                            _InventorySalesTaxRepository.Insert(item);
                        }
                    }

                    //var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == service.InventoryReceiveId).Select(t => t.IsNonCreditable).FirstOrDefault();
                    //var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                    //if (entity.CurrencyId != entity.BaseCurrencyId)
                    //	UpdateInventoryDetail(service, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                    //else if (entity.CurrencyId == entity.BaseCurrencyId)
                    //UpdateInventoryDetail(service, ratio, 1, entity.IsNonCreditable);
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


        public void ServiceChargesDelete(string serviceId)
        {
            var flag = false;
            try
            {
                //var isNonCreditable = _InventorySalesServiceRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[PurchaseOrder] AS A JOIN [TRN].[POService] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + serviceId + "'").First();
                var service = _InventorySalesServiceRepository.Find(serviceId);
                if (!service.IsNotNull())
                    throw new CustomException("Data not found");
                _unitOfWork.BeginTransaction();
                flag = true;

                var taxCategoryList = _InventorySalesTaxRepository.Query(t => t.InventorySalesServiceId == serviceId).Select().ToList();
                if (taxCategoryList.IsNotNull())
                {
                    foreach (var item in taxCategoryList)
                    {
                        //item.ModelState = ModelState.Deleted;
                        _InventorySalesTaxRepository.Delete(item);
                    }
                }
                //var ratio = _InventorySalesServiceRepository.GetChargesRatio(service.InventorysalesId, null, 0, service.Id, 0, isNonCreditable);
                //UpdateInventoryDetail(service, ratio, 1, isNonCreditable);
                //base.DeleteGraph(service);
                _InventorySalesServiceRepository.Delete(service);
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
        #endregion

        #region Material Transfer
        private string GetGRNPK()
        {
            return base.GetAutoNumber(nameof(InventoryReceive), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public void MaterialTransferCreateInsertGraph(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryReceive inventoryReceive, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {

            var flag = false;
            var grndId = "";
            bool FlagIsAsset = false;
            var NewListTemp = new List<InventoryReceiveDetail>();
            if (IssueTypeStatus.ToString() == "Inventory")
            {
                FlagIsAsset = false;
            }
            else
            {
                FlagIsAsset = true;
            }
            try
            {
                var GRNCalculateList = new List<InventoryTransferHistory>();
                if (entities.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var _pk = GetGRNPK();
                    var inventoryMaterialList = _inventoryMaterialService.GetInventoryMaterialListByUpToSku(entities, inventoryReceive.CompanyId, inventoryReceive.PlantId);
                    var currencyId = _companyRepository.Find(inventoryReceive.CompanyId).BaseCurrencyId;
                    var currentId1 = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{inventoryReceive.Id}'").First();
                    foreach (var item in entities)// update view model (inventory material field)
                    {
                        var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                && t.FirstCharacteristicsId == item.FirstCharacteristicsId && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                && t.SecondCharacteristicsId == item.SecondCharacteristicsId && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                                && t.ThirdCharacteristicsId == item.ThirdCharacteristicsId && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                                && t.CountryId == item.CountryId
                                && t.CompanyId == inventoryReceive.CompanyId && t.PlantId == inventoryReceive.PlantId // && t.CountryId == item.CountryId
                               );
                        if (im.IsNotNull())
                        {

                            if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                            item.InventoryIssueId = _pk;
                            item.InventoryMaterialId = im.Id;
                            item.CompanyGroupId = im.CompanyGroupId;
                            item.CompanyId = inventoryReceive.CompanyId;
                            item.PlantId = inventoryReceive.PlantId;
                            item.CurrencyId = currencyId;
                            item.MaterialStorageId = null;
                            item.MaterialMasterId = im.MaterialMasterId;
                            item.ArticleId = im.ArticleId;
                            item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                            item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                            item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                            item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                            item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                            item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                            item.TotalQty = im.TotalQty;
                            item.AvgRate = im.AvgRate;
                        }
                    }// update view model (inventory material field)
                    inventoryReceive.CurrencyId = currencyId;
                    inventoryReceive.Id = GetGRNPK();
                    //InsertGraph(inventoryIssue);
                    inventoryReceive.GRNType = "MaterialTransfer";
                    AuditService.AddedLog(inventoryReceive);
                    inventoryReceive.GRNDate = inventoryReceive.AddedDate;
                    inventoryReceive.EntryDate = inventoryReceive.AddedDate;
                    inventoryReceive.DocDate = inventoryReceive.AddedDate;
                    _InventoryReceiveRepository.Insert(inventoryReceive);
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    var rdBuilder1 = new System.Text.StringBuilder();
                    var builderSql1 = "";
                    var FromPlant = FromPlat(inventoryReceive.FromMaterialStorageId);
                    var ToPlant = FromPlat(inventoryReceive.MaterialStorageId);
                    if (FromPlant.Rows[0]["PlantId"].ToString() != ToPlant.Rows[0]["PlantId"].ToString())
                    {
                        inventoryReceive.RequiredPosting = true;
                    }
                    else
                    {
                        inventoryReceive.RequiredPosting = false;
                    }
                    //_issueDetailService.InsertRange(entities, specificStockList, inventoryIssue);


                    #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======
                    try
                    {

                        var uiList = entities.ToList();
                        var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryReceive.Id}'").First();
                        var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                        var specificInvaterialIds = new string[] { };
                        var maIds = new string[] { };
                        if (specificStockList.IsNotNull())
                        {
                            specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                            maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                            for (int i = uiList.Count() - 1; i >= 0; i--)
                            {
                                var row = uiList.ElementAt(i);
                                if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                    uiList.RemoveAt(i);
                            }
                        }
                        else maIds = inventoryMaterialIds;
                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty,IRD.InventorySalesQty,IRD.InventoryScrapQty,IRD.PurchaseReturnQty,IRD.IssueReturnQty,IRD.ReductionByAdjustmentQty,isnull(IRD.InventoryTransferQty,0) InventoryTransferQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryReceive.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryReceive.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
									      AND IR.Status='Posting' 
										  AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryReceive.GRNDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                        if (receiveDetailList.IsNotNull())
                        {
                            foreach (var issue in uiList)
                            {

                                var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;


                                if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)

                                    issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                if (issue.TransactionUoMId != issue.BaseUOMId)
                                    issue.BaseQty = Convert.ToDecimal(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);

                                decimal IssueTransactionQty = issue.TransactionQty;
                                foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                {

                                    if (IssueTransactionQty <= 0)
                                        break;


                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                    decimal RemainingGRNQty = Convert.ToDecimal((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;


                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }

                                    if (item.TransactionUoMId == issue.TransactionUoMId)
                                    {

                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryTransferHistory
                                        {
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty

                                        };
                                        GRNCalculateList.Add(newgrn);
                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                    }
                                    else
                                    {
                                        detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                        var newgrn = new InventoryTransferHistory
                                        {
                                            TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                            Qty = IssueDeduactionQty
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                    }

                                }

                                if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                currentId++;

                                if (issue.BaseQty == null) issue.BaseQty = totalGRNQty;
                                var detail = new InventoryIssueDetail
                                {
                                    Id = MakePK(inventoryReceive.Id, currentId, 2),
                                    InventoryIssueId = inventoryReceive.Id,
                                    IsAsset = FlagIsAsset,//false,									
                                    InventoryMaterialId = issue.InventoryMaterialId,
                                    TransactionQty = issue.TransactionQty,
                                    BaseQty = issue.BaseQty,
                                    BaseUOMId = issue.BaseUOMId,
                                    TransactionUoMId = issue.TransactionUoMId,
                                    AvgRate = issue.AvgRate,
                                    AvgAmount = issue.TransactionQty * issue.AvgRate,
                                    Policy = receiveDetailRow.Policy,

                                    PolicyAmount = detailtrnAmount,
                                    PolicyRate = detailtrnAmount / totalGRNQty,
                                    BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                    ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                    Comments = issue.Comments,
                                    CostCenterId = issue.CostCenterId,
                                    ModelState = ModelState.Added
                                };
                                var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{detail.Id}'").First();
                                // single entry (history)								
                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                {
                                    historyId++;
                                    var history = new InventoryIssueHistory
                                    {
                                        Id = MakePK(detail.Id, historyId, 2),
                                        InventoryIssueDetailId = detail.Id,
                                        InventoryReceiveDetailId = receiveDetailRow.Id,
                                        Qty = SelectedGRN.Qty,
                                        Rate = SelectedGRN.TotalAmount / SelectedGRN.Qty,
                                        TotalAmount = SelectedGRN.TotalAmount,
                                        IsCapitalize = false,
                                        IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                        IssueReturnQty = 0
                                    };

                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + @"'
									 , BaseIssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + "' WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";
                                    rdBuilder.Append(builderSql);
                                    AuditService.AddedLog(history);
                                    _issueHistoryRepository.Insert(history);


                                }
                                // multiple entry (history)
                                else
                                {
                                    var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                    var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                               - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();

                                    if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                    decimal policyAmount = 0;

                                    decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                    foreach (var item in rdList)
                                    {
                                        historyId++;
                                        if (item.TransactionUoMId != item.BaseUOMId)
                                            issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                        else issue.BaseRate = item.MaterialTranRate;
                                        var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty - item.PurchaseReturnQty - item.ReductionByAdjustmentQty - item.InventorySalesQty - item.InventoryScrapQty) + Convert.ToDecimal(item.IssueReturnQty);
                                        // (10 - 3)//Issueable Qty

                                        if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                        {
                                            policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                            qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                        }
                                        else // (6 < 7) (qtyDifference < issueQty)
                                        {
                                            //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                            issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                            policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                            qtyDifference = 0;
                                        }
                                        SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                        var history = new InventoryIssueHistory
                                        {
                                            Id = MakePK(detail.Id, historyId, 2),
                                            InventoryIssueDetailId = detail.Id,
                                            InventoryReceiveDetailId = item.Id,
                                            Qty = SelectedGRN.Qty,
                                            Rate = SelectedGRN.TotalAmount / SelectedGRN.Qty,
                                            TotalAmount = SelectedGRN.TotalAmount,
                                            IsCapitalize = false,
                                            IssueRequestDetailId = receiveDetailRow.IssueRequest
                                        };

                                        AuditService.AddedLog(history);
                                        _issueHistoryRepository.Insert(history);

                                        builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET BaseIssueQty='" + Convert.ToDecimal(issueQty) + "',IssueQty='" + Convert.ToDecimal(issueQty) + "'  WHERE Id='" + item.Id + "'";
                                        rdBuilder.Append(builderSql);
                                        if (qtyDifference == 0)
                                            break;

                                    }

                                }
                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - issue.TransactionQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                                rdBuilder.Append(builderSql);
                                AuditService.AddedLog(detail);
                                _issueDetailService.InsertGraph(detail);
                            }
                        }
                        if (specificStockList.IsNotNull())
                        {

                            foreach (var invMaterialId in specificInvaterialIds)
                            {

                                var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                var totalReqQty = 0M;
                                decimal detailtrnAmount = 0;
                                decimal totalGRNQty = 0;
                                foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                {
                                    decimal IssueTransactionQty = item.RequisitionQty;
                                    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                    //}
                                    decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                    decimal IssueDeduactionQty = 0;


                                    if (RemainingGRNQty <= IssueTransactionQty)
                                    {
                                        IssueDeduactionQty = RemainingGRNQty;
                                        IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                        RemainingGRNQty = 0;

                                    }
                                    else
                                    {
                                        IssueDeduactionQty = IssueTransactionQty;
                                        RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                        IssueTransactionQty = 0;
                                    }

                                    if (item.TransactionUoMId == item.BaseUOMId) //entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault())
                                    {
                                        detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                        var newgrn = new InventoryTransferHistory
                                        {
                                            TotalAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        // totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                    }
                                    else
                                    {
                                        detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                        var newgrn = new InventoryTransferHistory
                                        {
                                            TotalAmount = Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                            InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                        };
                                        GRNCalculateList.Add(newgrn);
                                        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                        totalGRNQty += Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor);
                                    }

                                    currentId++;
                                    var NewId = inventoryReceive.Id + "-";
                                    currentId1++;
                                    grndId = NewId + currentId1;
                                    //decimal BaseQtytemp = stockList.Sum(r => r.RequisitionQty);
                                    var a = NewListTemp.Where(r => r.InventoryMaterialId == invMaterialId).Select(r => r.InventoryMaterialId).ToList();
                                    var grnId = "";
                                    decimal grnTransactionqty = 0;
                                    decimal TotalMaterialBooksCurrencyAmount = 0;
                                    if (a.Count > 0)
                                    { }
                                    else
                                    {
                                        var recvDetail = new InventoryReceiveDetail
                                        {
                                            Id = NewId + currentId1,
                                            MaterialStorageId = inventoryReceive.MaterialStorageId,
                                            InventoryReceiveId = inventoryReceive.Id,
                                            TransactionQty = stockList.Sum(r => r.RequisitionQty),
                                            TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                                            BaseQty = stockList.Sum(r => r.RequisitionQty * Convert.ToDecimal(item.BaseUoMFactor)),//totalGRNQty,//Convert.ToDecimal(BaseQtytemp*item.BaseUoMFactor),//stockList.Sum(r => r.RequisitionQty),
                                            BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                            BaseUoMFactor = Convert.ToDecimal(entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUoMFactor).FirstOrDefault()),
                                            //Convert.ToDecimal(itemDetail.BaseUoMFactor),									
                                            MaterialTranRate = Math.Round(detailtrnAmount / totalGRNQty, 4),
                                            MaterialTranAmount = Math.Round(Convert.ToDecimal(detailtrnAmount), 2),
                                            TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(detailtrnAmount), 2),
                                            TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(detailtrnAmount), 2),
                                            POID = null,
                                            PODetailsID = null,
                                            TotalTaxAmount = 0,
                                            ChargesTranAmount = 0,
                                            ChargesTaxTranAmount = Convert.ToDecimal(0),
                                            IssueQty = 0,
                                            BaseIssueQty = 0,
                                            ShortageQty = Convert.ToDecimal(0),
                                            RejectionQty = Convert.ToDecimal(0),
                                            ApprovedQty = Convert.ToDecimal(stockList.Sum(r => r.RequisitionQty)),
                                            ShortageRatePercent = Convert.ToDecimal(0),
                                            ShortageValue = Convert.ToDecimal(0),
                                            RejectRatePercent = Convert.ToDecimal(0),
                                            RejectValue = Convert.ToDecimal(0),
                                            RejectClamPercent = Convert.ToDecimal(0),
                                            TrnCurrencyBaseRate = Math.Round(detailtrnAmount / totalGRNQty, 4),
                                            BooksCurrencyBaseRate = Math.Round(detailtrnAmount / totalGRNQty, 4),
                                            PurchaseDocumentAcceptanceId = null,
                                            PurchaseDocumentAcceptanceDetailId = null,
                                            PurchaseReturnQty = 0,
                                            IssueReturnQty = 0,
                                            InventorySalesQty = 0,
                                            InventoryScrapQty = 0,
                                            MaterialMasterOpeningBalanceDetailId = null,
                                            LotNumber = null,
                                            Diameter = null,
                                            Type = null,
                                            TransferedFromGrnId = item.InventoryReceiveDetailId
                                        };
                                        grnId = recvDetail.Id;
                                        grnTransactionqty = recvDetail.TransactionQty;
                                        NewListTemp.Add(recvDetail);
                                        AuditService.AddedLog(recvDetail);
                                        recvDetail.InventoryMaterialId = item.InventoryMaterialId;
                                        TotalMaterialBooksCurrencyAmount = recvDetail.TotalMaterialBooksCurrencyAmount;
                                        _receiveDetailRepository.Insert(recvDetail);

                                    }

                                    var historyId = _InventoryTransferHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryTransferHistory] WHERE InventoryReceiveDetailId='{grndId}'").First();
                                    foreach (var item1 in stockList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId))
                                    {

                                        if (item1.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");
                                        if (item1.TransactionUoMId != item1.BaseUOMId)
                                            totalReqQty = Convert.ToInt32(item1.RequisitionQty * item1.BaseUoMFactor);
                                        else
                                            totalReqQty = item1.RequisitionQty;
                                        historyId++;
                                        var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item1.InventoryReceiveDetailId).FirstOrDefault();//sk
                                        var history = new InventoryTransferHistory
                                        {
                                            Id = MakePK(grnId, historyId, 2),
                                            InventoryReceiveDetailId = item1.InventoryReceiveDetailId,
                                            Qty = item1.RequisitionQty,
                                            Rate = Math.Round(SelectedGRN.TotalAmount / item.RequisitionQty, 4),
                                            TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),
                                        };
                                        builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET InventoryTransferQty='" + Convert.ToDecimal(item1.RequisitionQty + item.InventoryTransferQty) + @"'  WHERE Id = '" + item1.InventoryReceiveDetailId + "'";
                                        rdBuilder.Append(builderSql);
                                        AuditService.AddedLog(history);

                                        _InventoryTransferHistoryRepository.Insert(history);

                                    }


                                    //var FromCGCPlant = _materialStorageRepository.SqlQuery<MaterialStorage>(@"select CompanyGroupId,CompanyId,PlantId from [HKP].[MaterialStorage] where Id='" + inventoryReceive.FromMaterialStorageId + "'").FirstOrDefault();
                                    //var ToCGCPlant = _materialStorageRepository.SqlQuery<MaterialStorage>(@"select CompanyGroupId,CompanyId,PlantId from [HKP].[MaterialStorage] where Id='" + inventoryReceive.MaterialStorageId + "'").FirstOrDefault();
                                    //var Frompalnt = (@"select CompanyGroupId,CompanyId,PlantId from [HKP].[MaterialStorage] where Id='" + inventoryReceive.MaterialStorageId + "'").ToList();
                                    //var Topalnt = (@"select CompanyGroupId,CompanyId,PlantId from [HKP].[MaterialStorage] where Id='" + inventoryReceive.MaterialStorageId + "'").ToList();

                                    // var dtGeneralVoucher = advanceDataList;
                                    // var tranCurrencyId = dtGeneralVoucher.Rows[0]["CurrencyId"].ToString();
                                    // var tranCurrencyCode = dtGeneralVoucher.Rows[0]["CurrencyCode"].ToString();
                                    if (FromPlant.Rows[0]["PlantId"].ToString() == ToPlant.Rows[0]["PlantId"].ToString())
                                    {

                                    }
                                    else
                                    {
                                        //foreach (var specificStockListNew in specificStockList)
                                        //{
                                        builderSql1 = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - grnTransactionqty) + "' WHERE Id='" + invMaterialId + "'";//recvDetail.TransactionQty
                                        rdBuilder1.Append(builderSql1);



                                        //item.TotalQty = Convert.ToDecimal(invMaterial.TotalQty - recvDetail.TransactionQty);
                                        item.CompanyGroupId = ToPlant.Rows[0]["CompanyGroupId"].ToString();
                                        item.CompanyId = ToPlant.Rows[0]["CompanyId"].ToString();
                                        item.PlantId = ToPlant.Rows[0]["PlantId"].ToString();
                                        item.TotalQty = grnTransactionqty;//recvDetail.TransactionQty;
                                        item.AvgRate = Math.Round(TotalMaterialBooksCurrencyAmount / grnTransactionqty, 4);//
                                                                                                                           //item.InventoryMaterialId = GetPKInventoryMaterial();
                                        DataTable InventoryMaterialDT = ToFindInventoryMaterialId(item.CompanyGroupId, item.CompanyId, item.PlantId, item.MaterialMasterId, item.ArticleId, item.FirstCharacteristicsValueId, item.SecondCharacteristicsValueId, item.ThirdCharacteristicsValueId, item.FirstCharacteristicsId, item.SecondCharacteristicsId, item.ThirdCharacteristicsId, item.CountryId);
                                        if (InventoryMaterialDT.Rows.Count > 0)
                                        {
                                            item.InventoryMaterialId = InventoryMaterialDT.Rows[0]["Id"].ToString();
                                        }
                                        else
                                        {
                                            item.InventoryMaterialId = null;
                                        }
                                        //var InventoryMaterialDT1 = _inventoryMaterialRepository.SqlQuery<InventoryMaterial>(@"select Id  from TRN.InventoryMaterial where CompanyGroupId='" + item.CompanyGroupId + "' AND CompanyId='" + item.CompanyId + "' AND PlantId='" + item.PlantId + "' AND MaterialMasterId='" + item.MaterialMasterId + "' AND ArticleId='" + item.ArticleId + "' AND  FirstCharacteristicsValueId='" + item.FirstCharacteristicsValueId + "' AND  SecondCharacteristicsValueId='" + item.SecondCharacteristicsValueId + "' AND ThirdCharacteristicsValueId='" + item.ThirdCharacteristicsValueId + "' AND FirstCharacteristicsId='" + item.FirstCharacteristicsId + "' AND SecondCharacteristicsId='" + item.SecondCharacteristicsId + "' AND ThirdCharacteristicsId='" + item.ThirdCharacteristicsId + "' AND  CountryId='" + item.CountryId + "'").FirstOrDefault();
                                        //item.InventoryMaterialId = InventoryMaterialDT1.ToString();
                                        InsertOrUpdateFromReceive(item);



                                        //}
                                    }


                                    //AuditService.AddedLog(recvDetail);
                                    //recvDetail.InventoryMaterialId = item.InventoryMaterialId;
                                    //_receiveDetailRepository.Insert(recvDetail);
                                    //}
                                }

                            }
                        }
                    }



                    catch (CustomException)
                    {
                        throw;
                    }
                    #endregion



                    _unitOfWork.SaveChanges();
                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    if (FromPlant.Rows[0]["PlantId"].ToString() != ToPlant.Rows[0]["PlantId"].ToString())
                    {
                        _sqlRepository.ExecuteSqlCommand(rdBuilder1.ToString());
                    }

                    flag = false;
                    _unitOfWork.Commit();
                }
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
        private string GetPKInventoryMaterial()
        {

            return base.GetAutoNumber(nameof(InventoryMaterial), PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public void InsertOrUpdateFromReceive(InventoryMaterialViewModel entity)
        {
            try
            {
                var rdBuilder11 = new System.Text.StringBuilder();
                var flag = false;


                if (string.IsNullOrEmpty(entity.InventoryMaterialId)) //&& string.IsNullOrEmpty(entity.ArticleId) && string.IsNullOrEmpty(entity.CountryId) && string.IsNullOrEmpty(entity.FirstCharacteristicsValueId) && string.IsNullOrEmpty(entity.SecondCharacteristicsValueId) && string.IsNullOrEmpty(entity.ThirdCharacteristicsValueId))
                {
                    entity.InventoryMaterialId = GetPKInventoryMaterial();
                    var material = ValueAssignInventoryMaterial(entity);
                    AuditService.AddedLog(material);
                    _inventoryMaterialRepository.Insert(material);

                }
                else
                {
                    try
                    {


                        //_unitOfWork.BeginTransaction();
                        //flag = true;
                        var material = ValueAssignInventoryMaterial(entity);
                        AuditService.UpdatedLog(material);

                        DataTable PrevTotalQty = PreviousQty(entity.InventoryMaterialId);
                        decimal PrevTotalQtyNew = Convert.ToDecimal(PrevTotalQty.Rows[0]["TotalQty"]);
                        //builderSql11 = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(PrevTotalQtyNew + entity.TotalQty) + "' WHERE Id='" + entity.InventoryMaterialId + "'";
                        //rdBuilder11.Append(builderSql11);
                        //_sqlRepository.ExecuteSqlCommand(rdBuilder11.ToString());
                        material.TotalQty = PrevTotalQtyNew + entity.TotalQty;
                        _inventoryMaterialRepository.Update(material);

                        //_unitOfWork.SaveChanges();

                        //flag = false;
                        //_unitOfWork.Commit();

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
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        private static InventoryMaterial ValueAssignInventoryMaterial(InventoryMaterialViewModel entity)
        {
            return new InventoryMaterial
            {
                Id = entity.InventoryMaterialId,
                CountryId = entity.CountryId,
                CompanyGroupId = entity.CompanyGroupId,
                CompanyId = entity.CompanyId,
                PlantId = entity.PlantId,
                MaterialStorageId = null,
                OpeningBalanceId = entity.OpeningBalanceId,
                MaterialMasterId = entity.MaterialMasterId,
                ArticleId = entity.ArticleId,
                FirstCharacteristicsId = entity.FirstCharacteristicsId,
                FirstCharacteristicsValueId = entity.FirstCharacteristicsValueId,
                SecondCharacteristicsId = entity.SecondCharacteristicsId,
                SecondCharacteristicsValueId = entity.SecondCharacteristicsValueId,
                ThirdCharacteristicsId = entity.ThirdCharacteristicsId,
                ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId,
                TotalQty = entity.TotalQty,
                AvgRate = entity.AvgRate,
                ShortageQty = Convert.ToDecimal(entity.ShortageQty),
                RejectionQty = Convert.ToDecimal(entity.RejectionQty),
                ApprovedQty = Convert.ToDecimal(entity.ApprovedQty)
            };
        }

        private DataTable FromPlat(string MaterialStorageId)
        {
            var cmdText = @"select CompanyGroupId,CompanyId,PlantId from [HKP].[MaterialStorage] where Id='" + MaterialStorageId + "'";
            return _sqlRepository.GetDataTable(cmdText);
        }
        private DataTable ToPlat(string MaterialStorageId)
        {
            var cmdText = @"select CompanyGroupId,CompanyId,PlantId from [HKP].[MaterialStorage] where Id='" + MaterialStorageId + "'";
            return _sqlRepository.GetDataTable(cmdText);
        }
        private DataTable ToFindInventoryMaterialId(string CompanyGroupId, string CompanyId, string PlantId, string MaterialMasterId, string ArticleId, string FirstCharacteristicsValueId, string SecondCharacteristicsValueId, string ThirdCharacteristicsValueId, string FirstCharacteristicsId, string SecondCharacteristicsId, string ThirdCharacteristicsId, string CountryId)
        {
            var cmdText = @"select Id  from TRN.InventoryMaterial where CompanyGroupId='" + CompanyGroupId + "' AND CompanyId='" + CompanyId + "' AND PlantId='" + PlantId + "' AND MaterialMasterId='" + MaterialMasterId + "' AND ArticleId='" + ArticleId + "' AND  ISNULL(FirstCharacteristicsValueId,'')='" + FirstCharacteristicsValueId + "' AND  ISNULL(SecondCharacteristicsValueId,'')='" + SecondCharacteristicsValueId + "' AND ISNULL(ThirdCharacteristicsValueId,'')='" + ThirdCharacteristicsValueId + "' AND ISNULL(FirstCharacteristicsId,'')='" + FirstCharacteristicsId + "' AND ISNULL(SecondCharacteristicsId,'')='" + SecondCharacteristicsId + "' AND ISNULL(ThirdCharacteristicsId,'')='" + ThirdCharacteristicsId + "' AND  ISNULL(CountryId,'')='" + CountryId + "'";
            return _sqlRepository.GetDataTable(cmdText);
        }
        private DataTable PreviousQty(string InventoryMaterialId)
        {
            var cmdText = @"select TotalQty  from TRN.InventoryMaterial where Id='" + InventoryMaterialId + "'";
            return _sqlRepository.GetDataTable(cmdText);
        }
        #endregion

        

        public void JWInsertGraph(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll, string TabType)
        {
            var flag = false;
            bool FlagIsAsset = false;
            if (IssueTypeStatus.ToString() == "Inventory")
            {
                FlagIsAsset = false;
            }
            else
            {
                FlagIsAsset = true;
            }

            string JWArtId = null;
            try
            {
                if (inventoryIssue.Id.IsNull())
                {

                    var GRNCalculateList = new List<InventoryIssueHistory>();
                    if (entities.IsNotNull())
                    {
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        var _pk = GetPK();
                        var inventoryMaterialList = _inventoryMaterialService.GetJWInventoryMaterialListByUpToSku(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                        var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
                        foreach (var item in entities)// update view model (inventory material field)
                        {
                            //  JWArtId += ",'" + item.ArticleId + "' ";
                            if (item.ArticleId.IsNotNull())
                            {
                                if (string.IsNullOrEmpty(JWArtId))
                                {
                                    JWArtId = item.ArticleId;
                                }

                                var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                    && t.FirstCharacteristicsId == item.FirstCharacteristicsId
                                    && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                    && t.SecondCharacteristicsId == item.SecondCharacteristicsId
                                    && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                                    && t.ThirdCharacteristicsId == item.ThirdCharacteristicsId
                                    && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                                    //&& t.CountryId == item.CountryId
                                    && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId // && t.CountryId == item.CountryId
                                   );

                                if (im.IsNotNull())
                                {

                                    if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                                    item.InventoryIssueId = _pk;
                                    item.InventoryMaterialId = im.Id;
                                    item.CompanyGroupId = im.CompanyGroupId;
                                    item.CompanyId = inventoryIssue.CompanyId;
                                    item.PlantId = inventoryIssue.PlantId;
                                    item.CurrencyId = currencyId;
                                    item.MaterialStorageId = null;
                                    item.MaterialMasterId = im.MaterialMasterId;
                                    item.ArticleId = im.ArticleId;
                                    item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                                    item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                                    item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                                    item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                                    item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                                    item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                                    item.TotalQty = im.TotalQty;
                                    item.AvgRate = im.AvgRate;

                                }
                            }

                        }// update view model (inventory material field)
                        inventoryIssue.CurrencyId = currencyId;
                        inventoryIssue.ProductionOrderId = inventoryIssue.ProductionOrderId;
                        inventoryIssue.ContractId = inventoryIssue.ContractId;
                        inventoryIssue.OrderRefNo = inventoryIssue.OrderRefNo;

                        inventoryIssue.JWContractId = inventoryIssue.JWContractId;
                        inventoryIssue.ContractType = inventoryIssue.ContractType;
                        inventoryIssue.Types = inventoryIssue.Types;

                        inventoryIssue.RefferenceNo = inventoryIssue.RefferenceNo;
                        inventoryIssue.IssueType = inventoryIssue.IssueType;
                        inventoryIssue.EmployeeId = inventoryIssue.EmployeeId;

                        inventoryIssue.MaterialStorageId = inventoryIssue.MaterialStorageId;
                        inventoryIssue.EmployeeId = inventoryIssue.EmployeeId;

                        inventoryIssue.IssueDate = inventoryIssue.IssueDate;
                        inventoryIssue.EntityId = inventoryIssue.EntityId;
                        inventoryIssue.PlantId = inventoryIssue.PlantId;

                        inventoryIssue.CompanyGroupId = inventoryIssue.CompanyGroupId;
                        inventoryIssue.CompanyId = inventoryIssue.CompanyId;

                        inventoryIssue.Id = _pk;
                        InsertGraph(inventoryIssue);
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = "";
                        //_issueDetailService.InsertRange(entities, specificStockList, inventoryIssue);


                        #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======


                        if (!string.IsNullOrEmpty(JWArtId))
                        {
                            try
                            {
                                //    var inventoryMaterialIds = new string[] { };

                                var uiList = entities.ToList();
                                var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                                var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();
                                //    inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).Distinct().ToArray();

                                var specificInvaterialIds = new string[] { };
                                //        var specificInventoryReceiveDetailIds = new string[] { };
                                var maIds = new string[] { };
                                if (specificStockList.IsNotNull())
                                {
                                    specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                                    //          specificInventoryReceiveDetailIds = specificStockList.Select(t => t.InventoryReceiveDetailId).Distinct().ToArray();
                                    maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                                    for (int i = uiList.Count() - 1; i >= 0; i--)
                                    {
                                        var row = uiList.ElementAt(i);
                                        if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                            uiList.RemoveAt(i);
                                    }
                                }
                                else maIds = inventoryMaterialIds;
                                var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty,IRD.InventorySalesQty,IRD.InventoryScrapQty,IRD.PurchaseReturnQty,IRD.IssueReturnQty,IRD.ReductionByAdjustmentQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
									      AND IR.Status='Posting' 
										  AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.IssueDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                                if (receiveDetailList.IsNotNull())
                                {
                                    foreach (var issue in uiList)
                                    {
                                        if (issue.ArticleId.IsNotNull())
                                        {
                                            var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                            decimal detailtrnAmount = 0;
                                            decimal totalGRNQty = 0;
                                            /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* (TotalMmaterialTrnAmount/MaterialTrnRate))*/
                                            /*Rate= Amount/Sum GRN Qty */

                                            if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                                                //input.BaseRate = receiveDetailRow.BaseAmount / receiveDetailRow.BaseQty;
                                                issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                            else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                            if (issue.TransactionUoMId != issue.BaseUOMId)
                                                issue.BaseQty = Convert.ToDecimal(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);

                                            decimal IssueTransactionQty = issue.TransactionQty;
                                            foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                            {

                                                if (IssueTransactionQty <= 0)
                                                    break;

                                                //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                                //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(IIH.TotalAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(ISH.TotalBaseAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                                //																						   FROM trn.InventoryReceiveDetail IRD  
                                                //																							left JOIN [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                                //																						   WHERE  IIH.InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                                decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                                decimal RemainingGRNQty = Convert.ToDecimal((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty)) + item.IssueReturnQty);
                                                decimal IssueDeduactionQty = 0;


                                                if (RemainingGRNQty <= IssueTransactionQty)
                                                {
                                                    IssueDeduactionQty = RemainingGRNQty;
                                                    IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                                    RemainingGRNQty = 0;

                                                }
                                                else
                                                {
                                                    IssueDeduactionQty = IssueTransactionQty;
                                                    RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                                    IssueTransactionQty = 0;
                                                }

                                                //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                                if (item.TransactionUoMId == issue.TransactionUoMId)
                                                {

                                                    detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                                    var newgrn = new InventoryIssueHistory
                                                    {
                                                        TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = IssueDeduactionQty

                                                    };
                                                    GRNCalculateList.Add(newgrn);
                                                    //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                                    totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                                }
                                                else
                                                {
                                                    detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                                    var newgrn = new InventoryIssueHistory
                                                    {
                                                        TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = IssueDeduactionQty
                                                    };
                                                    GRNCalculateList.Add(newgrn);
                                                    //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                                    totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                                }
                                                //}
                                            }

                                            if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                            currentId++;
                                            //totalGRNQty = issue.TransactionQty;
                                            if (issue.BaseQty == null) issue.BaseQty = totalGRNQty;
                                            var detail = new InventoryIssueDetail
                                            {
                                                Id = MakePK(inventoryIssue.Id, currentId, 2),
                                                InventoryIssueId = inventoryIssue.Id,
                                                IsAsset = FlagIsAsset,//false,
                                                                      //InventoryIssue = inventoryIssue,
                                                InventoryMaterialId = issue.InventoryMaterialId,
                                                TransactionQty = issue.TransactionQty,
                                                BaseQty = issue.BaseQty,
                                                BaseUOMId = issue.BaseUOMId,
                                                TransactionUoMId = issue.TransactionUoMId,

                                                //TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                                AvgRate = Math.Round(issue.AvgRate, 4),
                                                AvgAmount = Math.Round((issue.TransactionQty * issue.AvgRate), 2),
                                                Policy = receiveDetailRow.Policy,

                                                PolicyAmount = Math.Round(detailtrnAmount, 2),
                                                PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),

                                                //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                                                //PolicyRate = detailtrnAmount / totalGRNQty,
                                                BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                                ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                                Comments = issue.Comments,
                                                CostCenterId = issue.CostCenterId,
                                                // OSTransformationPOId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.OSTransformationPOId).FirstOrDefault(),
                                                OSTransformationPOId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.OSTransformationPODetailId).FirstOrDefault(),
                                                ModelState = ModelState.Added

                                                //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                                                //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                                            };
                                            var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{detail.Id}'").First();
                                            // single entry (history)
                                            //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                            //if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                            var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                            if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                            {
                                                historyId++;
                                                var history = new InventoryIssueHistory
                                                {
                                                    Id = MakePK(detail.Id, historyId, 2),
                                                    InventoryIssueDetailId = detail.Id,
                                                    InventoryReceiveDetailId = receiveDetailRow.Id,
                                                    Qty = SelectedGRN.Qty,
                                                    //Rate = Convert.ToDecimal(issue.BaseRate),
                                                    //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                    //Rate = detailtrnAmount / totalGRNQty,
                                                    //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                                    Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                                    TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                    IsCapitalize = false,
                                                    IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                                    IssueReturnQty = 0,
                                                    BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BooksCurrencyBaseRate), 4),
                                                    TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * issue.BooksCurrencyBaseRate), 2)
                                                };
                                                //detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                                                //detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);

                                                builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + @"'
									 , BaseIssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + "' WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";
                                                rdBuilder.Append(builderSql);
                                                AuditService.AddedLog(history);
                                                _issueHistoryRepository.Insert(history);


                                            }
                                            // multiple entry (history)
                                            else
                                            {
                                                var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                                var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                                           - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                                                //if (tqty < input.TransactionQty) throw new CustomException("Stock 0");
                                                if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                                decimal policyAmount = 0;
                                                //decimal qtyDifference = input.TransactionQty;
                                                decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                                foreach (var item in rdList)
                                                {
                                                    historyId++;
                                                    if (item.TransactionUoMId != item.BaseUOMId)
                                                        //input.BaseRate = item.BaseAmount / item.BaseQty;
                                                        issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                                    //else input.BaseRate = item.TransactionRate;
                                                    else issue.BaseRate = item.MaterialTranRate;

                                                    //var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty);
                                                    var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty - item.PurchaseReturnQty - item.ReductionByAdjustmentQty - item.InventorySalesQty - item.InventoryScrapQty) + Convert.ToDecimal(item.IssueReturnQty);
                                                    // (10 - 3)//Issueable Qty
                                                    //if (issueQty != 0)
                                                    //{

                                                    if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                                    {
                                                        policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                                        qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                                        issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                                    }
                                                    else // (6 < 7) (qtyDifference < issueQty)
                                                    {
                                                        //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                                        issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                                        policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                                        qtyDifference = 0;
                                                    }
                                                    SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                                    var history = new InventoryIssueHistory
                                                    {
                                                        Id = MakePK(detail.Id, historyId, 2),
                                                        InventoryIssueDetailId = detail.Id,
                                                        InventoryReceiveDetailId = item.Id,
                                                        Qty = SelectedGRN.Qty,//Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO
                                                                              //Qty = Convert.ToDecimal(issueQty),//TODO
                                                                              // Qty = Convert.ToDecimal(qtyDifference),//TODO
                                                                              //Rate = Convert.ToInt32(issue.BaseRate),
                                                                              //Rate = Convert.ToDecimal(issue.BaseRate),
                                                                              //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                                              //Rate = detailtrnAmount / totalGRNQty,
                                                                              //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                                        Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                                        TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                        IsCapitalize = false,
                                                        IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * item.BooksCurrencyBaseRate), 2)

                                                    };

                                                    AuditService.AddedLog(history);
                                                    _issueHistoryRepository.Insert(history);

                                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET BaseIssueQty='" + Convert.ToDecimal(issueQty) + "',IssueQty='" + Convert.ToDecimal(issueQty) + "'  WHERE Id='" + item.Id + "'";
                                                    rdBuilder.Append(builderSql);
                                                    if (qtyDifference == 0)
                                                        break;
                                                    //}
                                                }

                                                //detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                                                //detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                                            }
                                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - issue.TransactionQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                                            rdBuilder.Append(builderSql);
                                            AuditService.AddedLog(detail);
                                            _issueDetailService.InsertGraph(detail);

                                            //Mapping Data=========================================================
                                            var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + issue.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                            if (receiveDetailList1.Count > 0)
                                            {
                                                bool isQtyAlocated = true;
                                                decimal temp = 0;
                                                int count = 0;
                                                foreach (var receiveDetailListNew in receiveDetailList1)
                                                {


                                                    count++;
                                                    if (count == 1)
                                                    {
                                                        if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > detail.TransactionQty)
                                                        {

                                                            detail.TransactionQty = detail.TransactionQty;
                                                            //temp += itemDetail.TransactionQty;
                                                            isQtyAlocated = false;

                                                        }
                                                        else if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < detail.TransactionQty)
                                                        {
                                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                            temp = (detail.TransactionQty - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                            detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                            isQtyAlocated = true;

                                                        }
                                                        else
                                                        {
                                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                            detail.TransactionQty = detail.TransactionQty;
                                                            isQtyAlocated = true;

                                                        }
                                                    }
                                                    if (count > 1)
                                                    {
                                                        if (isQtyAlocated == true)
                                                        {
                                                            if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > temp)
                                                            {
                                                                //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                                detail.TransactionQty = detail.TransactionQty;
                                                                isQtyAlocated = false;
                                                            }
                                                            if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < temp)
                                                            {
                                                                //temp = temp - issue.TransactionQtyForPO;
                                                                temp = (temp - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                                //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                                detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                                isQtyAlocated = true;
                                                            }
                                                            else
                                                            {
                                                                //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                detail.TransactionQty = temp;
                                                                isQtyAlocated = true;

                                                            }

                                                        }
                                                        else
                                                        {
                                                            detail.TransactionQty = 0;
                                                        }
                                                    }


                                                    var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                    {
                                                        Id = GetIssueDetailAndIssueRequestMapPK(),
                                                        InventoryIssueDetailId = detail.Id,
                                                        IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                        Qty = detail.TransactionQty
                                                        //AutoAllocate = true

                                                    };
                                                    AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                    _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                                }
                                            }

                                            //===================

                                        }
                                    }

                                }

                                if (specificStockList.IsNotNull())
                                {

                                    //foreach (var RecId in specificInventoryReceiveDetailIds)
                                    //{
                                    foreach (var invMaterialId in specificInvaterialIds)
                                    {
                                        var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                        var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                        var totalReqQty = 0M;
                                        decimal detailtrnAmount = 0;
                                        decimal totalGRNQty = 0;

                                        foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                        {
                                            decimal IssueTransactionQty = item.RequisitionQty;
                                            decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																														FROM (
																																SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD
																																left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																																UNION All
																																SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																														)x
																														WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                            decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                            decimal IssueDeduactionQty = 0;


                                            if (RemainingGRNQty <= IssueTransactionQty)
                                            {
                                                IssueDeduactionQty = RemainingGRNQty;
                                                IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                                RemainingGRNQty = 0;

                                            }
                                            else
                                            {
                                                IssueDeduactionQty = IssueTransactionQty;
                                                RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                                IssueTransactionQty = 0;
                                            }
                                            if (item.TransactionUoMId == item.BaseUOMId) //entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                            {
                                                detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                                var newgrn = new InventoryIssueHistory
                                                {
                                                    TotalAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                                };
                                                GRNCalculateList.Add(newgrn);
                                                //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                                totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                            }
                                            else
                                            {
                                                detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                                var newgrn = new InventoryIssueHistory
                                                {
                                                    TotalAmount = Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                                };
                                                GRNCalculateList.Add(newgrn);
                                                //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                                totalGRNQty += Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor);
                                            }
                                            item.IssueRequest = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.IssueRequest).FirstOrDefault();
                                        }


                                        currentId++;
                                        var issueDetail = new InventoryIssueDetail
                                        {
                                            Id = MakePK(inventoryIssue.Id, currentId, 2),
                                            InventoryIssueId = inventoryIssue.Id,
                                            IsAsset = FlagIsAsset,//false,
                                                                  //InventoryIssue = inventoryIssue,
                                            InventoryMaterialId = invMaterialId,
                                            BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                            TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                                            AvgRate = Math.Round(invMaterial.AvgRate, 4),
                                            Policy = "N/A",

                                            TransactionQty = Math.Round(totalGRNQty, 2), //stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
                                            PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
                                            PolicyAmount = Math.Round(detailtrnAmount, 2),
                                            BaseQty = Math.Round(totalGRNQty, 2),//stockList.Sum(r => r.RequisitionQty),
                                            AvgAmount = Math.Round((totalGRNQty * invMaterial.AvgRate), 2),
                                            BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                            ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                            CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
                                            Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),
                                            // OSTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.OSTransformationPOId).FirstOrDefault(),
                                            OSTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.OSTransformationPODetailId).FirstOrDefault(),
                                            OSTransformationPOInputMaterialId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.OSTransformationPOInputMaterialId).FirstOrDefault(),
                                            //JWTCInputId = entities.Where(r => r.MaterialMasterId != invMaterial.MaterialMasterId && r.ArticleId != invMaterial.ArticleId).Select(t => t.JWInputItemId).FirstOrDefault(),
                                            //  JWTCInputId = entities.Where(r => r.MaterialMasterId == null && r.ArticleId == null).Select(t => t.JWInputItemId).FirstOrDefault(),
                                            ModelState = ModelState.Added
                                        };
                                        decimal tempPolicyAmount = 0;
                                        if (invMaterial.ArticleId.IsNotNull())
                                        {
                                            // start

                                            var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                                            foreach (var item in stockList)
                                            {

                                                if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                                                if (item.TransactionUoMId != item.BaseUOMId)
                                                    // totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                                                    totalReqQty = Convert.ToDecimal(item.RequisitionQty * item.BaseUoMFactor);
                                                else
                                                    totalReqQty = item.RequisitionQty;
                                                historyId++;
                                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                                var history = new InventoryIssueHistory
                                                {
                                                    Id = MakePK(issueDetail.Id, historyId, 2),
                                                    InventoryIssueDetailId = issueDetail.Id,
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                    Qty = totalReqQty, //item.RequisitionQty,
                                                                       //Rate = Convert.ToDecimal(item.BaseRate),
                                                                       //Rate = Math.Round((SelectedGRN.TotalAmount / item.RequisitionQty), 4),
                                                                       //TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                                       //Rate = Math.Round((SelectedGRN.TotalAmount / totalReqQty), 4),//Old calculation
                                                    Rate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),//totalGRNQty
                                                                                                                        //TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                    TotalAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2),
                                                    IssueRequestDetailId = item.IssueRequest,
                                                    IssueReturnQty = 0,
                                                    BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                    TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2)
                                                };
                                                //policyAmmount += history.Qty * history.Rate;

                                                tempPolicyAmount += Math.Round(Convert.ToDecimal(history.TotalMaterialBooksCurrencyAmount), 4);

                                                builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
										,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                                rdBuilder.Append(builderSql);
                                                AuditService.AddedLog(history);
                                                _issueHistoryRepository.Insert(history);



                                                //Mapping Data=========================================================
                                                if (entitiesAll.IsNotNull())
                                                {
                                                    foreach (var itemall in entitiesAll)
                                                    {
                                                        var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + itemall.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                                        if (receiveDetailList1.IsNotNull())
                                                        {
                                                            foreach (var receiveDetailListNew in receiveDetailList1)
                                                            {


                                                                //count++;
                                                                //if (count == 1)
                                                                //{
                                                                //    if (((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) > issueDetail.TransactionQty)
                                                                //    {

                                                                //        issueDetail.TransactionQty =Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //        //temp += itemDetail.TransactionQty;
                                                                //        isQtyAlocated = false;

                                                                //    }
                                                                //    else if (((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) < issueDetail.TransactionQty)
                                                                //    {
                                                                //        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //        temp = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //        issueDetail.TransactionQty = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //        isQtyAlocated = true;

                                                                //    }
                                                                //    else
                                                                //    {
                                                                //        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //        issueDetail.TransactionQty = Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //        isQtyAlocated = true;

                                                                //    }
                                                                //}
                                                                //if (count > 1)
                                                                //{
                                                                //    if (isQtyAlocated == true)
                                                                //    {
                                                                //        if ((Convert.ToDecimal(receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) > temp)
                                                                //        {
                                                                //            //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //            isQtyAlocated = false;
                                                                //        }
                                                                //        if ((Convert.ToDecimal(receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) < temp)
                                                                //        {
                                                                //            //temp = temp - issue.TransactionQtyForPO;
                                                                //            temp = Convert.ToDecimal(temp - ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor));
                                                                //            //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //            isQtyAlocated = true;
                                                                //        }
                                                                //        else
                                                                //        {
                                                                //            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = temp;
                                                                //            isQtyAlocated = true;

                                                                //        }

                                                                //    }
                                                                //    else
                                                                //    {
                                                                //        issueDetail.TransactionQty = 0;
                                                                //    }
                                                                //}


                                                                var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                                {
                                                                    Id = GetIssueDetailAndIssueRequestMapPK(),
                                                                    InventoryIssueDetailId = issueDetail.Id,
                                                                    IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                                    Qty = receiveDetailListNew.IssueRequestBOQMapQty,
                                                                    //AutoAllocate = true

                                                                };
                                                                AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                                _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                                            }
                                                        }


                                                    }
                                                }


                                            }


                                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
                                            rdBuilder.Append(builderSql);

                                            // End

                                        }
                                        issueDetail.PolicyAmount = tempPolicyAmount;
                                        issueDetail.PolicyRate = Math.Round(tempPolicyAmount / issueDetail.TransactionQty, 4);
                                        AuditService.AddedLog(issueDetail);
                                        _issueDetailService.InsertGraph(issueDetail);
                                        tempPolicyAmount = 0;

                                        //===================

                                    }
                                    //       }
                                }


                                //        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                            }
                            catch (CustomException)
                            {
                                throw;
                            }
                            #endregion
                        }


                        _unitOfWork.SaveChanges();
                        if (!string.IsNullOrEmpty(JWArtId))
                        {
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        }

                        flag = false;
                        _unitOfWork.Commit();
                        if (TabType == "Transformation")
                        {
                            SaveIssueTransformationChild(entities, _pk);
                        }
                        else
                        {
                            SaveIssueValAddedChild(entities, _pk);
                        }

                    }

                }
                else
                {
                    var GRNCalculateList = new List<InventoryIssueHistory>();
                    if (entities.IsNotNull())
                    {
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        //     var _pk = GetPK();
                        var inventoryMaterialList = _inventoryMaterialService.GetJWInventoryMaterialListByUpToSku(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                        var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
                        foreach (var item in entities)// update view model (inventory material field)
                        {
                            //  JWArtId += ",'" + item.ArticleId + "' ";
                            if (item.ArticleId.IsNotNull())
                            {
                                if (string.IsNullOrEmpty(JWArtId))
                                {
                                    JWArtId = item.ArticleId;
                                }

                                var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                    && t.FirstCharacteristicsId == item.FirstCharacteristicsId
                                    && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                    && t.SecondCharacteristicsId == item.SecondCharacteristicsId
                                    && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                                    && t.ThirdCharacteristicsId == item.ThirdCharacteristicsId
                                    && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                                    //&& t.CountryId == item.CountryId
                                    && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId // && t.CountryId == item.CountryId
                                   );

                                if (im.IsNotNull())
                                {

                                    if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                                    //     item.InventoryIssueId = _pk;
                                    item.InventoryIssueId = inventoryIssue.Id;
                                    item.InventoryMaterialId = im.Id;
                                    item.CompanyGroupId = im.CompanyGroupId;
                                    item.CompanyId = inventoryIssue.CompanyId;
                                    item.PlantId = inventoryIssue.PlantId;
                                    item.CurrencyId = currencyId;
                                    item.MaterialStorageId = null;
                                    item.MaterialMasterId = im.MaterialMasterId;
                                    item.ArticleId = im.ArticleId;
                                    item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                                    item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                                    item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                                    item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                                    item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                                    item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                                    item.TotalQty = im.TotalQty;
                                    item.AvgRate = im.AvgRate;

                                }
                            }

                        }// update view model (inventory material field)
                        inventoryIssue.CurrencyId = currencyId;
                        inventoryIssue.ProductionOrderId = inventoryIssue.ProductionOrderId;
                        inventoryIssue.ContractId = inventoryIssue.ContractId;
                        inventoryIssue.OrderRefNo = inventoryIssue.OrderRefNo;

                        inventoryIssue.JWContractId = inventoryIssue.JWContractId;
                        inventoryIssue.ContractType = inventoryIssue.ContractType;
                        inventoryIssue.Types = inventoryIssue.Types;

                        inventoryIssue.RefferenceNo = inventoryIssue.RefferenceNo;
                        inventoryIssue.IssueType = inventoryIssue.IssueType;
                        inventoryIssue.EmployeeId = inventoryIssue.EmployeeId;

                        inventoryIssue.MaterialStorageId = inventoryIssue.MaterialStorageId;
                        inventoryIssue.EmployeeId = inventoryIssue.EmployeeId;

                        inventoryIssue.IssueDate = inventoryIssue.IssueDate;
                        inventoryIssue.EntityId = inventoryIssue.EntityId;
                        inventoryIssue.PlantId = inventoryIssue.PlantId;

                        inventoryIssue.CompanyGroupId = inventoryIssue.CompanyGroupId;
                        inventoryIssue.CompanyId = inventoryIssue.CompanyId;

                        // inventoryIssue.Id = _pk;
                        inventoryIssue.Id = inventoryIssue.Id;
                        //  InsertGraph(inventoryIssue);
                        UpdateGraph(inventoryIssue);
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = "";
                        //_issueDetailService.InsertRange(entities, specificStockList, inventoryIssue);


                        #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======


                        if (!string.IsNullOrEmpty(JWArtId))
                        {
                            try
                            {
                                //    var inventoryMaterialIds = new string[] { };

                                var uiList = entities.ToList();
                                var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                                var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();
                                //    inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).Distinct().ToArray();

                                var specificInvaterialIds = new string[] { };
                                //       var specificInventoryReceiveDetailIds = new string[] { };
                                var maIds = new string[] { };
                                if (specificStockList.IsNotNull())
                                {
                                    specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                                    //          specificInventoryReceiveDetailIds = specificStockList.Select(t => t.InventoryReceiveDetailId).Distinct().ToArray();
                                    maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                                    for (int i = uiList.Count() - 1; i >= 0; i--)
                                    {
                                        var row = uiList.ElementAt(i);
                                        if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                            uiList.RemoveAt(i);
                                    }
                                }
                                else maIds = inventoryMaterialIds;
                                var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty,IRD.InventorySalesQty,IRD.InventoryScrapQty,IRD.PurchaseReturnQty,IRD.IssueReturnQty,IRD.ReductionByAdjustmentQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
									      AND IR.Status='Posting' 
										  AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.IssueDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                                if (receiveDetailList.IsNotNull())
                                {
                                    foreach (var issue in uiList)
                                    {
                                        if (issue.ArticleId.IsNotNull())
                                        {
                                            var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                            decimal detailtrnAmount = 0;
                                            decimal totalGRNQty = 0;
                                            /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* (TotalMmaterialTrnAmount/MaterialTrnRate))*/
                                            /*Rate= Amount/Sum GRN Qty */

                                            if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                                                //input.BaseRate = receiveDetailRow.BaseAmount / receiveDetailRow.BaseQty;
                                                issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                            else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                            if (issue.TransactionUoMId != issue.BaseUOMId)
                                                issue.BaseQty = Convert.ToDecimal(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);

                                            decimal IssueTransactionQty = issue.TransactionQty;
                                            foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                            {

                                                if (IssueTransactionQty <= 0)
                                                    break;

                                                //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                                //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(IIH.TotalAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(ISH.TotalBaseAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                                //																						   FROM trn.InventoryReceiveDetail IRD  
                                                //																							left JOIN [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                                //																						   WHERE  IIH.InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                                decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                                decimal RemainingGRNQty = Convert.ToDecimal((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty)) + item.IssueReturnQty);
                                                decimal IssueDeduactionQty = 0;


                                                if (RemainingGRNQty <= IssueTransactionQty)
                                                {
                                                    IssueDeduactionQty = RemainingGRNQty;
                                                    IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                                    RemainingGRNQty = 0;

                                                }
                                                else
                                                {
                                                    IssueDeduactionQty = IssueTransactionQty;
                                                    RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                                    IssueTransactionQty = 0;
                                                }

                                                //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                                if (item.TransactionUoMId == issue.TransactionUoMId)
                                                {

                                                    detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                                    var newgrn = new InventoryIssueHistory
                                                    {
                                                        TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = IssueDeduactionQty

                                                    };
                                                    GRNCalculateList.Add(newgrn);
                                                    //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                                    totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                                }
                                                else
                                                {
                                                    detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                                    var newgrn = new InventoryIssueHistory
                                                    {
                                                        TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = IssueDeduactionQty
                                                    };
                                                    GRNCalculateList.Add(newgrn);
                                                    //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                                    totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                                }
                                                //}
                                            }

                                            if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                            currentId++;
                                            //totalGRNQty = issue.TransactionQty;
                                            if (issue.BaseQty == null) issue.BaseQty = totalGRNQty;
                                            var detail = new InventoryIssueDetail
                                            {
                                                Id = MakePK(inventoryIssue.Id, currentId, 2),
                                                InventoryIssueId = inventoryIssue.Id,
                                                IsAsset = FlagIsAsset,//false,
                                                                      //InventoryIssue = inventoryIssue,
                                                InventoryMaterialId = issue.InventoryMaterialId,
                                                TransactionQty = issue.TransactionQty,
                                                BaseQty = issue.BaseQty,
                                                BaseUOMId = issue.BaseUOMId,
                                                TransactionUoMId = issue.TransactionUoMId,

                                                //TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                                AvgRate = Math.Round(issue.AvgRate, 4),
                                                AvgAmount = Math.Round((issue.TransactionQty * issue.AvgRate), 2),
                                                Policy = receiveDetailRow.Policy,

                                                PolicyAmount = Math.Round(detailtrnAmount, 2),
                                                PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),

                                                //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                                                //PolicyRate = detailtrnAmount / totalGRNQty,
                                                BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                                ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                                Comments = issue.Comments,
                                                CostCenterId = issue.CostCenterId,
                                                // OSTransformationPOId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.OSTransformationPOId).FirstOrDefault(),
                                                OSTransformationPOId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.OSTransformationPODetailId).FirstOrDefault(),
                                                ModelState = ModelState.Added

                                                //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                                                //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                                            };
                                            var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{detail.Id}'").First();
                                            // single entry (history)
                                            //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                            //if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                            var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                            if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                            {
                                                historyId++;
                                                var history = new InventoryIssueHistory
                                                {
                                                    Id = MakePK(detail.Id, historyId, 2),
                                                    InventoryIssueDetailId = detail.Id,
                                                    InventoryReceiveDetailId = receiveDetailRow.Id,
                                                    Qty = SelectedGRN.Qty,
                                                    //Rate = Convert.ToDecimal(issue.BaseRate),
                                                    //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                    //Rate = detailtrnAmount / totalGRNQty,
                                                    //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                                    Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                                    TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                    IsCapitalize = false,
                                                    IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                                    IssueReturnQty = 0,
                                                    BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BooksCurrencyBaseRate), 4),
                                                    TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * issue.BooksCurrencyBaseRate), 2)
                                                };
                                                //detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                                                //detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);

                                                builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + @"'
									 , BaseIssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + "' WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";
                                                rdBuilder.Append(builderSql);
                                                AuditService.AddedLog(history);
                                                _issueHistoryRepository.Insert(history);


                                            }
                                            // multiple entry (history)
                                            else
                                            {
                                                var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                                var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                                           - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                                                //if (tqty < input.TransactionQty) throw new CustomException("Stock 0");
                                                if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                                decimal policyAmount = 0;
                                                //decimal qtyDifference = input.TransactionQty;
                                                decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                                foreach (var item in rdList)
                                                {
                                                    historyId++;
                                                    if (item.TransactionUoMId != item.BaseUOMId)
                                                        //input.BaseRate = item.BaseAmount / item.BaseQty;
                                                        issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                                    //else input.BaseRate = item.TransactionRate;
                                                    else issue.BaseRate = item.MaterialTranRate;

                                                    //var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty);
                                                    var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty - item.PurchaseReturnQty - item.ReductionByAdjustmentQty - item.InventorySalesQty - item.InventoryScrapQty) + Convert.ToDecimal(item.IssueReturnQty);
                                                    // (10 - 3)//Issueable Qty
                                                    //if (issueQty != 0)
                                                    //{

                                                    if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                                    {
                                                        policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                                        qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                                        issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                                    }
                                                    else // (6 < 7) (qtyDifference < issueQty)
                                                    {
                                                        //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                                        issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                                        policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                                        qtyDifference = 0;
                                                    }
                                                    SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                                    var history = new InventoryIssueHistory
                                                    {
                                                        Id = MakePK(detail.Id, historyId, 2),
                                                        InventoryIssueDetailId = detail.Id,
                                                        InventoryReceiveDetailId = item.Id,
                                                        Qty = SelectedGRN.Qty,//Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO
                                                                              //Qty = Convert.ToDecimal(issueQty),//TODO
                                                                              // Qty = Convert.ToDecimal(qtyDifference),//TODO
                                                                              //Rate = Convert.ToInt32(issue.BaseRate),
                                                                              //Rate = Convert.ToDecimal(issue.BaseRate),
                                                                              //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                                              //Rate = detailtrnAmount / totalGRNQty,
                                                                              //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                                        Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                                        TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                        IsCapitalize = false,
                                                        IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * item.BooksCurrencyBaseRate), 2)

                                                    };

                                                    AuditService.AddedLog(history);
                                                    _issueHistoryRepository.Insert(history);

                                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET BaseIssueQty='" + Convert.ToDecimal(issueQty) + "',IssueQty='" + Convert.ToDecimal(issueQty) + "'  WHERE Id='" + item.Id + "'";
                                                    rdBuilder.Append(builderSql);
                                                    if (qtyDifference == 0)
                                                        break;
                                                    //}
                                                }

                                                //detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                                                //detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                                            }
                                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - issue.TransactionQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                                            rdBuilder.Append(builderSql);
                                            AuditService.AddedLog(detail);
                                            _issueDetailService.InsertGraph(detail);

                                            //Mapping Data=========================================================
                                            var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + issue.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                            if (receiveDetailList1.Count > 0)
                                            {
                                                bool isQtyAlocated = true;
                                                decimal temp = 0;
                                                int count = 0;
                                                foreach (var receiveDetailListNew in receiveDetailList1)
                                                {


                                                    count++;
                                                    if (count == 1)
                                                    {
                                                        if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > detail.TransactionQty)
                                                        {

                                                            detail.TransactionQty = detail.TransactionQty;
                                                            //temp += itemDetail.TransactionQty;
                                                            isQtyAlocated = false;

                                                        }
                                                        else if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < detail.TransactionQty)
                                                        {
                                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                            temp = (detail.TransactionQty - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                            detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                            isQtyAlocated = true;

                                                        }
                                                        else
                                                        {
                                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                            detail.TransactionQty = detail.TransactionQty;
                                                            isQtyAlocated = true;

                                                        }
                                                    }
                                                    if (count > 1)
                                                    {
                                                        if (isQtyAlocated == true)
                                                        {
                                                            if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > temp)
                                                            {
                                                                //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                                detail.TransactionQty = detail.TransactionQty;
                                                                isQtyAlocated = false;
                                                            }
                                                            if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < temp)
                                                            {
                                                                //temp = temp - issue.TransactionQtyForPO;
                                                                temp = (temp - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                                //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                                detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                                isQtyAlocated = true;
                                                            }
                                                            else
                                                            {
                                                                //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                detail.TransactionQty = temp;
                                                                isQtyAlocated = true;

                                                            }

                                                        }
                                                        else
                                                        {
                                                            detail.TransactionQty = 0;
                                                        }
                                                    }


                                                    var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                    {
                                                        Id = GetIssueDetailAndIssueRequestMapPK(),
                                                        InventoryIssueDetailId = detail.Id,
                                                        IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                        Qty = detail.TransactionQty
                                                        //AutoAllocate = true

                                                    };
                                                    AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                    _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                                }
                                            }

                                            //===================

                                        }
                                    }

                                }
                                if (specificStockList.IsNotNull())
                                {
                                    //foreach (var RecId in specificInventoryReceiveDetailIds)
                                    //{

                                    foreach (var invMaterialId in specificInvaterialIds)
                                    {
                                        var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                        var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                        var totalReqQty = 0M;
                                        decimal detailtrnAmount = 0;
                                        decimal totalGRNQty = 0;

                                        foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                        {
                                            decimal IssueTransactionQty = item.RequisitionQty;
                                            decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																														FROM (
																																SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD
																																left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																																UNION All
																																SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																														)x
																														WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                            decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                            decimal IssueDeduactionQty = 0;


                                            if (RemainingGRNQty <= IssueTransactionQty)
                                            {
                                                IssueDeduactionQty = RemainingGRNQty;
                                                IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                                RemainingGRNQty = 0;

                                            }
                                            else
                                            {
                                                IssueDeduactionQty = IssueTransactionQty;
                                                RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                                IssueTransactionQty = 0;
                                            }
                                            if (item.TransactionUoMId == item.BaseUOMId) //entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                            {
                                                detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                                var newgrn = new InventoryIssueHistory
                                                {
                                                    TotalAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                                };
                                                GRNCalculateList.Add(newgrn);

                                                //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                                totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                            }
                                            else
                                            {
                                                detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                                var newgrn = new InventoryIssueHistory
                                                {
                                                    TotalAmount = Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                                };
                                                GRNCalculateList.Add(newgrn);
                                                //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                                totalGRNQty += Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor);
                                            }
                                            item.IssueRequest = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.IssueRequest).FirstOrDefault();
                                        }


                                        currentId++;
                                        var issueDetail = new InventoryIssueDetail
                                        {
                                            //   Id = MakePK(inventoryIssue.Id, currentId, 2),
                                            Id = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.InventoryIssueDetailId).FirstOrDefault(),
                                            InventoryIssueId = inventoryIssue.Id,
                                            IsAsset = FlagIsAsset,//false,
                                                                  //InventoryIssue = inventoryIssue,
                                            InventoryMaterialId = invMaterialId,
                                            BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                            TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                                            AvgRate = Math.Round(invMaterial.AvgRate, 4),
                                            Policy = "N/A",

                                            TransactionQty = Math.Round(totalGRNQty, 2), //stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
                                            PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
                                            PolicyAmount = Math.Round(detailtrnAmount, 2),
                                            BaseQty = Math.Round(totalGRNQty, 2),//stockList.Sum(r => r.RequisitionQty),
                                            AvgAmount = Math.Round((totalGRNQty * invMaterial.AvgRate), 2),
                                            BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                            ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                            CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
                                            Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),
                                            // OSTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.OSTransformationPOId).FirstOrDefault(),
                                            OSTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.OSTransformationPODetailId).FirstOrDefault(),
                                            //JWTCInputId = entities.Where(r => r.MaterialMasterId != invMaterial.MaterialMasterId && r.ArticleId != invMaterial.ArticleId).Select(t => t.JWInputItemId).FirstOrDefault(),
                                            //  JWTCInputId = entities.Where(r => r.MaterialMasterId == null && r.ArticleId == null).Select(t => t.JWInputItemId).FirstOrDefault(),
                                            ModelState = ModelState.Added
                                        };
                                        decimal tempPolicyAmount = 0;
                                        if (invMaterial.ArticleId.IsNotNull())
                                        {
                                            // start

                                            // var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                                            foreach (var item in stockList)
                                            {
                                                var IRHUPId = "";
                                                var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}' and InventoryReceiveDetailId='{item.InventoryReceiveDetailId}'").First();
                                                if (historyId != 0)
                                                {
                                                    IRHUPId = _issueHistoryRepository.SqlQuery<string>($"SELECT Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}' and InventoryReceiveDetailId='{item.InventoryReceiveDetailId}'").First();
                                                }
                                                if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                                                if (item.TransactionUoMId != item.BaseUOMId)
                                                    // totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                                                    totalReqQty = Convert.ToDecimal(item.RequisitionQty * item.BaseUoMFactor);
                                                else
                                                    totalReqQty = item.RequisitionQty;
                                                // historyId++;
                                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                                if (historyId == 0)
                                                {
                                                    var history = new InventoryIssueHistory
                                                    {

                                                        Id = MakePK(issueDetail.Id, historyId, 2),
                                                        InventoryIssueDetailId = issueDetail.Id,
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = totalReqQty, //item.RequisitionQty,
                                                                           //Rate = Convert.ToDecimal(item.BaseRate),
                                                                           //Rate = Math.Round((SelectedGRN.TotalAmount / item.RequisitionQty), 4),
                                                                           //TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                        Rate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),//totalGRNQty
                                                        TotalAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2),//Convert.ToDecimal(detailtrnAmount),
                                                        IssueRequestDetailId = item.IssueRequest,
                                                        IssueReturnQty = 0,
                                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2)
                                                    };
                                                    //policyAmmount += history.Qty * history.Rate;

                                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
										,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                                    rdBuilder.Append(builderSql);
                                                    AuditService.AddedLog(history);
                                                    _issueHistoryRepository.Insert(history);

                                                    //AuditService.UpdatedLog(history);
                                                    //_issueHistoryRepository.Update(history);
                                                }
                                                else
                                                {
                                                    var history = new InventoryIssueHistory
                                                    {

                                                        Id = IRHUPId,
                                                        InventoryIssueDetailId = issueDetail.Id,
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = totalReqQty, //item.RequisitionQty,
                                                                           //Rate = Convert.ToDecimal(item.BaseRate),
                                                                           //Rate = Math.Round((SelectedGRN.TotalAmount / item.RequisitionQty), 4),
                                                                           //TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                        Rate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),//totalGRNQty
                                                        TotalAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2),//Convert.ToDecimal(detailtrnAmount),
                                                        IssueRequestDetailId = item.IssueRequest,
                                                        IssueReturnQty = 0,
                                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2)
                                                    };
                                                    //policyAmmount += history.Qty * history.Rate;

                                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
										,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                                    rdBuilder.Append(builderSql);
                                                    //AuditService.AddedLog(history);
                                                    //_issueHistoryRepository.Insert(history);

                                                    AuditService.UpdatedLog(history);
                                                    _issueHistoryRepository.Update(history);
                                                }



                                                //                                      builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
                                                //,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                                //                                      rdBuilder.Append(builderSql);
                                                //                                      //AuditService.AddedLog(history);
                                                //                                      //_issueHistoryRepository.Insert(history);

                                                //                                      AuditService.UpdatedLog(history);
                                                //                                      _issueHistoryRepository.Update(history);



                                                //Mapping Data=========================================================
                                                if (entitiesAll.IsNotNull())
                                                {
                                                    foreach (var itemall in entitiesAll)
                                                    {
                                                        var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + itemall.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                                        if (receiveDetailList1.IsNotNull())
                                                        {
                                                            foreach (var receiveDetailListNew in receiveDetailList1)
                                                            {

                                                                //count++;
                                                                //if (count == 1)
                                                                //{
                                                                //    if (((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) > issueDetail.TransactionQty)
                                                                //    {

                                                                //        issueDetail.TransactionQty =Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //        //temp += itemDetail.TransactionQty;
                                                                //        isQtyAlocated = false;

                                                                //    }
                                                                //    else if (((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) < issueDetail.TransactionQty)
                                                                //    {
                                                                //        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //        temp = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //        issueDetail.TransactionQty = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //        isQtyAlocated = true;

                                                                //    }
                                                                //    else
                                                                //    {
                                                                //        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //        issueDetail.TransactionQty = Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //        isQtyAlocated = true;

                                                                //    }
                                                                //}
                                                                //if (count > 1)
                                                                //{
                                                                //    if (isQtyAlocated == true)
                                                                //    {
                                                                //        if ((Convert.ToDecimal(receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) > temp)
                                                                //        {
                                                                //            //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //            isQtyAlocated = false;
                                                                //        }
                                                                //        if ((Convert.ToDecimal(receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) < temp)
                                                                //        {
                                                                //            //temp = temp - issue.TransactionQtyForPO;
                                                                //            temp = Convert.ToDecimal(temp - ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor));
                                                                //            //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //            isQtyAlocated = true;
                                                                //        }
                                                                //        else
                                                                //        {
                                                                //            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = temp;
                                                                //            isQtyAlocated = true;

                                                                //        }

                                                                //    }
                                                                //    else
                                                                //    {
                                                                //        issueDetail.TransactionQty = 0;
                                                                //    }
                                                                //}


                                                                var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                                {
                                                                    Id = GetIssueDetailAndIssueRequestMapPK(),
                                                                    InventoryIssueDetailId = issueDetail.Id,
                                                                    IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                                    Qty = receiveDetailListNew.IssueRequestBOQMapQty,
                                                                    //AutoAllocate = true

                                                                };
                                                                //AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                                //_IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);

                                                                AuditService.UpdatedLog(IssueDetailAndIssueRequestMapNew);
                                                                _IssueDetailAndIssueRequestMapRepository.Update(IssueDetailAndIssueRequestMapNew);
                                                            }
                                                        }


                                                    }
                                                }


                                            }


                                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
                                            rdBuilder.Append(builderSql);

                                            // End

                                        }

                                        //AuditService.AddedLog(issueDetail);
                                        //_issueDetailService.InsertGraph(issueDetail);
                                        issueDetail.PolicyAmount = tempPolicyAmount;
                                        issueDetail.PolicyRate = Math.Round(tempPolicyAmount / issueDetail.TransactionQty, 4);
                                        AuditService.UpdatedLog(issueDetail);
                                        _issueDetailService.UpdateGraph(issueDetail);

                                        tempPolicyAmount = 0;
                                        //===================

                                    }
                                    //        }
                                }


                                //        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                            }
                            catch (CustomException)
                            {
                                throw;
                            }
                            #endregion
                        }


                        _unitOfWork.SaveChanges();
                        if (!string.IsNullOrEmpty(JWArtId))
                        {
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        }

                        flag = false;
                        _unitOfWork.Commit();
                        if (TabType == "Transformation")
                        {
                            //       SaveIssueTransformationChild(entities, _pk);
                            SaveIssueTransformationChild(entities, inventoryIssue.Id);
                        }
                        else
                        {
                            //       SaveIssueValAddedChild(entities, _pk);
                            SaveIssueValAddedChild(entities, inventoryIssue.Id);
                        }

                    }
                }
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

        // Save Issue Tranformation Wihtout Material

        private string GetTransformationChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryIssueDetail", out sID);
            return sID;
        }

        public void SaveIssueTransformationChild(IEnumerable<InventoryMaterialViewModel> entities, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var JWItemId = "' '";
                var OtMatId = "' '";

                foreach (var empitem in entities)
                {
                    if (empitem.ArticleId.IsNull())
                    {
                        JWItemId += ",'" + empitem.JWInputItemId + "' ";
                        //        OtMatId += ",'" + empitem.OSTransformationPOId + "' ";
                        OtMatId += ",'" + empitem.OSTransformationPODetailId + "' ";
                    }


                }
                con.OpenDataSetThroughAdapter("select * from TRN.InventoryIssueDetail where OSTransformationPOId IN ( " + OtMatId + ") and JWTCInputId IN (" + JWItemId + ") and InventoryIssueId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in entities)
                {
                    if (item.ArticleId.IsNull())
                    {

                        ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + item.OSTransformationPODetailId + "' and JWTCInputId='" + item.JWInputItemId + "' ";

                        if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = ExistOrNot.Tables[0].NewRow();
                            dr["Id"] = GetTransformationChildPK();

                            dr["InventoryIssueId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseUOMId"] = item.BaseUOMId;
                            dr["CostCenterId"] = item.CostCenterId;
                            dr["OSTransformationPOId"] = item.OSTransformationPODetailId;
                            dr["JWTCInputId"] = item.JWInputItemId;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            //dr["UpdatedBy"] = identity.Name;
                            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            //dr["UpdatedFromIP"] = identity.IPAddress;

                            ExistOrNot.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + item.OSTransformationPODetailId + "' and JWTCInputId='" + item.JWInputItemId + "' ";

                            if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = ExistOrNot.Tables[0].NewRow();
                                dr["Id"] = GetTransformationChildPK();

                                dr["InventoryIssueId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.BaseUOMId;
                                dr["CostCenterId"] = item.CostCenterId;
                                dr["OSTransformationPOId"] = item.OSTransformationPODetailId;
                                dr["JWTCInputId"] = item.JWInputItemId;

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                ExistOrNot.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                //edit
                                DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["InventoryIssueId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.BaseUOMId;
                                dr["CostCenterId"] = item.CostCenterId;
                                dr["OSTransformationPOId"] = item.OSTransformationPODetailId;
                                dr["JWTCInputId"] = item.JWInputItemId;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;


                                dr.EndEdit();
                            }


                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                //         return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveIssueValAddedChild(IEnumerable<InventoryMaterialViewModel> entities, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var JWOrderWiseId = "' '";
                var OtMatId = "' '";

                foreach (var empitem in entities)
                {
                    if (empitem.ArticleId.IsNull())
                    {
                        if (empitem.JWOrderWiseId.IsNotNull())
                        {
                            JWOrderWiseId += ",'" + empitem.JWOrderWiseId + "' ";
                            //       OtMatId += ",'" + empitem.OSTransformationPOId + "' ";
                            OtMatId += ",'" + empitem.OSTransformationPODetailId + "' ";
                        }
                        else
                        {
                            OtMatId += ",'" + empitem.OSTransformationPODetailId + "' ";
                        }

                    }
                }

                if (JWOrderWiseId.IsNotNull())
                {
                    con.OpenDataSetThroughAdapter("select * from TRN.InventoryIssueDetail where OSTransformationPOId IN ( " + OtMatId + ") and JWOrderWiseId IN (" + JWOrderWiseId + ") and InventoryIssueId='" + MasterId + "'  ", out ExistOrNot, false, "1");
                }
                else
                {
                    con.OpenDataSetThroughAdapter("select * from TRN.InventoryIssueDetail where OSTransformationPOId IN ( " + OtMatId + ") and InventoryIssueId='" + MasterId + "'  ", out ExistOrNot, false, "1");
                }


                foreach (var item in entities)
                {
                    if (item.ArticleId.IsNull())
                    {
                        if (item.JWOrderWiseId.IsNotNull())
                        {
                            ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + item.OSTransformationPODetailId + "' and JWOrderWiseId='" + item.JWOrderWiseId + "' ";
                        }
                        else
                        {
                            ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + item.OSTransformationPODetailId + "' ";
                        }



                        if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = ExistOrNot.Tables[0].NewRow();
                            dr["Id"] = GetTransformationChildPK();

                            dr["InventoryIssueId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseUOMId"] = item.BaseUOMId;
                            dr["CostCenterId"] = item.CostCenterId;
                            dr["OSTransformationPOId"] = item.OSTransformationPODetailId;
                            dr["JWOrderWiseId"] = item.JWOrderWiseId;
                            dr["Comments"] = item.Comments;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            //dr["UpdatedBy"] = identity.Name;
                            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            //dr["UpdatedFromIP"] = identity.IPAddress;

                            ExistOrNot.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            if (item.JWOrderWiseId.IsNotNull())
                            {
                                ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + item.OSTransformationPODetailId + "' and JWOrderWiseId='" + item.JWOrderWiseId + "' ";
                            }
                            else
                            {
                                ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + item.OSTransformationPODetailId + "' ";
                            }

                            if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = ExistOrNot.Tables[0].NewRow();
                                dr["Id"] = GetTransformationChildPK();

                                dr["InventoryIssueId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.BaseUOMId;
                                dr["CostCenterId"] = item.CostCenterId;
                                dr["OSTransformationPOId"] = item.OSTransformationPODetailId;
                                dr["JWOrderWiseId"] = item.JWOrderWiseId;

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                ExistOrNot.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                //edit
                                DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["InventoryIssueId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.BaseUOMId;
                                dr["CostCenterId"] = item.CostCenterId;
                                dr["OSTransformationPOId"] = item.OSTransformationPODetailId;
                                dr["JWOrderWiseId"] = item.JWOrderWiseId;
                                dr["Comments"] = item.Comments;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;


                                dr.EndEdit();
                            }


                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                //         return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Job Work Issue Saving

        public void JobWorkIssueCreate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll, string TabType)
        {
            var flag = false;
            bool FlagIsAsset = false;
            if (IssueTypeStatus.ToString() == "Inventory")
            {
                FlagIsAsset = false;
            }
            else
            {
                FlagIsAsset = true;
            }

            string JWArtId = null;
            try
            {
                if (inventoryIssue.Id.IsNull())
                {

                    var GRNCalculateList = new List<InventoryIssueHistory>();
                    if (entities.IsNotNull())
                    {
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        var _pk = GetPK();
                        var inventoryMaterialList = _inventoryMaterialService.GetJWInventoryMaterialListByUpToSku(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                        var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
                        foreach (var item in entities)// update view model (inventory material field)
                        {
                            //  JWArtId += ",'" + item.ArticleId + "' ";
                            if (item.ArticleId.IsNotNull())
                            {
                                if (string.IsNullOrEmpty(JWArtId))
                                {
                                    JWArtId = item.ArticleId;
                                }

                                var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                    && t.FirstCharacteristicsId == item.FirstCharacteristicsId
                                    && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                    && t.SecondCharacteristicsId == item.SecondCharacteristicsId
                                    && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                                    && t.ThirdCharacteristicsId == item.ThirdCharacteristicsId
                                    && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                                    //&& t.CountryId == item.CountryId
                                    && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId // && t.CountryId == item.CountryId
                                   );

                                if (im.IsNotNull())
                                {

                                    if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                                    item.InventoryIssueId = _pk;
                                    item.InventoryMaterialId = im.Id;
                                    item.CompanyGroupId = im.CompanyGroupId;
                                    item.CompanyId = inventoryIssue.CompanyId;
                                    item.PlantId = inventoryIssue.PlantId;
                                    item.CurrencyId = currencyId;
                                    item.MaterialStorageId = null;
                                    item.MaterialMasterId = im.MaterialMasterId;
                                    item.ArticleId = im.ArticleId;
                                    item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                                    item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                                    item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                                    item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                                    item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                                    item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                                    item.TotalQty = im.TotalQty;
                                    item.AvgRate = im.AvgRate;

                                }
                            }

                        }// update view model (inventory material field)
                        inventoryIssue.CurrencyId = currencyId;
                        inventoryIssue.ProductionOrderId = inventoryIssue.ProductionOrderId;
                        inventoryIssue.ContractId = inventoryIssue.ContractId;
                        inventoryIssue.OrderRefNo = inventoryIssue.OrderRefNo;

                        //      inventoryIssue.JWContractId = inventoryIssue.JWContractId;
                        inventoryIssue.JobWorkContractId = inventoryIssue.JobWorkContractId;
                        inventoryIssue.ContractType = inventoryIssue.ContractType;
                        inventoryIssue.Types = inventoryIssue.Types;

                        inventoryIssue.RefferenceNo = inventoryIssue.RefferenceNo;
                        inventoryIssue.IssueType = inventoryIssue.IssueType;
                        inventoryIssue.EmployeeId = inventoryIssue.EmployeeId;

                        inventoryIssue.MaterialStorageId = inventoryIssue.MaterialStorageId;
                        inventoryIssue.EmployeeId = inventoryIssue.EmployeeId;

                        inventoryIssue.IssueDate = inventoryIssue.IssueDate;
                        inventoryIssue.EntityId = inventoryIssue.EntityId;
                        inventoryIssue.PlantId = inventoryIssue.PlantId;

                        inventoryIssue.CompanyGroupId = inventoryIssue.CompanyGroupId;
                        inventoryIssue.CompanyId = inventoryIssue.CompanyId;

                        inventoryIssue.Id = _pk;
                        InsertGraph(inventoryIssue);
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = "";
                        //_issueDetailService.InsertRange(entities, specificStockList, inventoryIssue);


                        #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======


                        if (!string.IsNullOrEmpty(JWArtId))
                        {
                            try
                            {
                                //    var inventoryMaterialIds = new string[] { };

                                var uiList = entities.ToList();
                                var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                                var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();
                                //    inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).Distinct().ToArray();

                                var specificInvaterialIds = new string[] { };
                                //        var specificInventoryReceiveDetailIds = new string[] { };
                                var maIds = new string[] { };
                                if (specificStockList.IsNotNull())
                                {
                                    specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                                    //          specificInventoryReceiveDetailIds = specificStockList.Select(t => t.InventoryReceiveDetailId).Distinct().ToArray();
                                    maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                                    for (int i = uiList.Count() - 1; i >= 0; i--)
                                    {
                                        var row = uiList.ElementAt(i);
                                        if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                            uiList.RemoveAt(i);
                                    }
                                }
                                else maIds = inventoryMaterialIds;
                                var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty,IRD.InventorySalesQty,IRD.InventoryScrapQty,IRD.PurchaseReturnQty,IRD.IssueReturnQty,IRD.ReductionByAdjustmentQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
									      AND IR.Status='Posting' 
										  AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.IssueDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                                if (receiveDetailList.IsNotNull())
                                {
                                    foreach (var issue in uiList)
                                    {
                                        if (issue.ArticleId.IsNotNull())
                                        {
                                            var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                            decimal detailtrnAmount = 0;
                                            decimal totalGRNQty = 0;
                                            /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* (TotalMmaterialTrnAmount/MaterialTrnRate))*/
                                            /*Rate= Amount/Sum GRN Qty */

                                            if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                                                //input.BaseRate = receiveDetailRow.BaseAmount / receiveDetailRow.BaseQty;
                                                issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                            else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                            if (issue.TransactionUoMId != issue.BaseUOMId)
                                                issue.BaseQty = Convert.ToDecimal(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);

                                            decimal IssueTransactionQty = issue.TransactionQty;
                                            foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                            {

                                                if (IssueTransactionQty <= 0)
                                                    break;

                                                //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                                //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(IIH.TotalAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(ISH.TotalBaseAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                                //																						   FROM trn.InventoryReceiveDetail IRD  
                                                //																							left JOIN [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                                //																						   WHERE  IIH.InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                                decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                                decimal RemainingGRNQty = Convert.ToDecimal((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty)) + item.IssueReturnQty);
                                                decimal IssueDeduactionQty = 0;


                                                if (RemainingGRNQty <= IssueTransactionQty)
                                                {
                                                    IssueDeduactionQty = RemainingGRNQty;
                                                    IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                                    RemainingGRNQty = 0;

                                                }
                                                else
                                                {
                                                    IssueDeduactionQty = IssueTransactionQty;
                                                    RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                                    IssueTransactionQty = 0;
                                                }

                                                //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                                if (item.TransactionUoMId == issue.TransactionUoMId)
                                                {

                                                    detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                                    var newgrn = new InventoryIssueHistory
                                                    {
                                                        TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = IssueDeduactionQty

                                                    };
                                                    GRNCalculateList.Add(newgrn);
                                                    //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                                    totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                                }
                                                else
                                                {
                                                    detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                                    var newgrn = new InventoryIssueHistory
                                                    {
                                                        TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = IssueDeduactionQty
                                                    };
                                                    GRNCalculateList.Add(newgrn);
                                                    //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                                    totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                                }
                                                //}
                                            }

                                            if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                            currentId++;
                                            //totalGRNQty = issue.TransactionQty;
                                            if (issue.BaseQty == null) issue.BaseQty = totalGRNQty;
                                            var detail = new InventoryIssueDetail
                                            {
                                                Id = MakePK(inventoryIssue.Id, currentId, 2),
                                                InventoryIssueId = inventoryIssue.Id,
                                                IsAsset = FlagIsAsset,//false,
                                                                      //InventoryIssue = inventoryIssue,
                                                InventoryMaterialId = issue.InventoryMaterialId,
                                                TransactionQty = issue.TransactionQty,
                                                BaseQty = issue.BaseQty,
                                                BaseUOMId = issue.BaseUOMId,
                                                TransactionUoMId = issue.TransactionUoMId,

                                                //TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                                AvgRate = Math.Round(issue.AvgRate, 4),
                                                AvgAmount = Math.Round((issue.TransactionQty * issue.AvgRate), 2),
                                                Policy = receiveDetailRow.Policy,

                                                PolicyAmount = Math.Round(detailtrnAmount, 2),
                                                PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),

                                                //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                                                //PolicyRate = detailtrnAmount / totalGRNQty,
                                                BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                                ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                                Comments = issue.Comments,
                                                CostCenterId = issue.CostCenterId,
                                                // OSTransformationPOId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.OSTransformationPOId).FirstOrDefault(),
                                                OSTransformationPOId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.OSTransformationPODetailId).FirstOrDefault(),
                                                ModelState = ModelState.Added

                                                //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                                                //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                                            };
                                            var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{detail.Id}'").First();
                                            // single entry (history)
                                            //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                            //if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                            var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                            if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                            {
                                                historyId++;
                                                var history = new InventoryIssueHistory
                                                {
                                                    Id = MakePK(detail.Id, historyId, 2),
                                                    InventoryIssueDetailId = detail.Id,
                                                    InventoryReceiveDetailId = receiveDetailRow.Id,
                                                    Qty = SelectedGRN.Qty,
                                                    //Rate = Convert.ToDecimal(issue.BaseRate),
                                                    //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                    //Rate = detailtrnAmount / totalGRNQty,
                                                    //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                                    Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                                    TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                    IsCapitalize = false,
                                                    IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                                    IssueReturnQty = 0,
                                                    BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BooksCurrencyBaseRate), 4),
                                                    TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * issue.BooksCurrencyBaseRate), 2)
                                                };
                                                //detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                                                //detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);

                                                builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + @"'
									 , BaseIssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + "' WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";
                                                rdBuilder.Append(builderSql);
                                                AuditService.AddedLog(history);
                                                _issueHistoryRepository.Insert(history);


                                            }
                                            // multiple entry (history)
                                            else
                                            {
                                                var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                                var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                                           - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                                                //if (tqty < input.TransactionQty) throw new CustomException("Stock 0");
                                                if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                                decimal policyAmount = 0;
                                                //decimal qtyDifference = input.TransactionQty;
                                                decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                                foreach (var item in rdList)
                                                {
                                                    historyId++;
                                                    if (item.TransactionUoMId != item.BaseUOMId)
                                                        //input.BaseRate = item.BaseAmount / item.BaseQty;
                                                        issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                                    //else input.BaseRate = item.TransactionRate;
                                                    else issue.BaseRate = item.MaterialTranRate;

                                                    //var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty);
                                                    var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty - item.PurchaseReturnQty - item.ReductionByAdjustmentQty - item.InventorySalesQty - item.InventoryScrapQty) + Convert.ToDecimal(item.IssueReturnQty);
                                                    // (10 - 3)//Issueable Qty
                                                    //if (issueQty != 0)
                                                    //{

                                                    if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                                    {
                                                        policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                                        qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                                        issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                                    }
                                                    else // (6 < 7) (qtyDifference < issueQty)
                                                    {
                                                        //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                                        issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                                        policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                                        qtyDifference = 0;
                                                    }
                                                    SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                                    var history = new InventoryIssueHistory
                                                    {
                                                        Id = MakePK(detail.Id, historyId, 2),
                                                        InventoryIssueDetailId = detail.Id,
                                                        InventoryReceiveDetailId = item.Id,
                                                        Qty = SelectedGRN.Qty,//Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO
                                                                              //Qty = Convert.ToDecimal(issueQty),//TODO
                                                                              // Qty = Convert.ToDecimal(qtyDifference),//TODO
                                                                              //Rate = Convert.ToInt32(issue.BaseRate),
                                                                              //Rate = Convert.ToDecimal(issue.BaseRate),
                                                                              //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                                              //Rate = detailtrnAmount / totalGRNQty,
                                                                              //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                                        Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                                        TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                        IsCapitalize = false,
                                                        IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * item.BooksCurrencyBaseRate), 2)

                                                    };

                                                    AuditService.AddedLog(history);
                                                    _issueHistoryRepository.Insert(history);

                                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET BaseIssueQty='" + Convert.ToDecimal(issueQty) + "',IssueQty='" + Convert.ToDecimal(issueQty) + "'  WHERE Id='" + item.Id + "'";
                                                    rdBuilder.Append(builderSql);
                                                    if (qtyDifference == 0)
                                                        break;
                                                    //}
                                                }

                                                //detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                                                //detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                                            }
                                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - issue.TransactionQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                                            rdBuilder.Append(builderSql);
                                            AuditService.AddedLog(detail);
                                            _issueDetailService.InsertGraph(detail);

                                            //Mapping Data=========================================================
                                            var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + issue.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                            if (receiveDetailList1.Count > 0)
                                            {
                                                bool isQtyAlocated = true;
                                                decimal temp = 0;
                                                int count = 0;
                                                foreach (var receiveDetailListNew in receiveDetailList1)
                                                {


                                                    count++;
                                                    if (count == 1)
                                                    {
                                                        if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > detail.TransactionQty)
                                                        {

                                                            detail.TransactionQty = detail.TransactionQty;
                                                            //temp += itemDetail.TransactionQty;
                                                            isQtyAlocated = false;

                                                        }
                                                        else if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < detail.TransactionQty)
                                                        {
                                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                            temp = (detail.TransactionQty - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                            detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                            isQtyAlocated = true;

                                                        }
                                                        else
                                                        {
                                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                            detail.TransactionQty = detail.TransactionQty;
                                                            isQtyAlocated = true;

                                                        }
                                                    }
                                                    if (count > 1)
                                                    {
                                                        if (isQtyAlocated == true)
                                                        {
                                                            if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > temp)
                                                            {
                                                                //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                                detail.TransactionQty = detail.TransactionQty;
                                                                isQtyAlocated = false;
                                                            }
                                                            if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < temp)
                                                            {
                                                                //temp = temp - issue.TransactionQtyForPO;
                                                                temp = (temp - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                                //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                                detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                                isQtyAlocated = true;
                                                            }
                                                            else
                                                            {
                                                                //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                detail.TransactionQty = temp;
                                                                isQtyAlocated = true;

                                                            }

                                                        }
                                                        else
                                                        {
                                                            detail.TransactionQty = 0;
                                                        }
                                                    }


                                                    var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                    {
                                                        Id = GetIssueDetailAndIssueRequestMapPK(),
                                                        InventoryIssueDetailId = detail.Id,
                                                        IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                        Qty = detail.TransactionQty
                                                        //AutoAllocate = true

                                                    };
                                                    AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                    _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                                }
                                            }

                                            //===================

                                        }
                                    }

                                }

                                if (specificStockList.IsNotNull())
                                {

                                    //foreach (var RecId in specificInventoryReceiveDetailIds)
                                    //{
                                    foreach (var invMaterialId in specificInvaterialIds)
                                    {
                                        var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                        var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                        var totalReqQty = 0M;
                                        decimal detailtrnAmount = 0;
                                        decimal totalGRNQty = 0;

                                        foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                        {
                                            decimal IssueTransactionQty = item.RequisitionQty;
                                            decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																														FROM (
																																SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD
																																left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																																UNION All
																																SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																														)x
																														WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                            decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                            decimal IssueDeduactionQty = 0;


                                            if (RemainingGRNQty <= IssueTransactionQty)
                                            {
                                                IssueDeduactionQty = RemainingGRNQty;
                                                IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                                RemainingGRNQty = 0;

                                            }
                                            else
                                            {
                                                IssueDeduactionQty = IssueTransactionQty;
                                                RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                                IssueTransactionQty = 0;
                                            }
                                            if (item.TransactionUoMId == item.BaseUOMId) //entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                            {
                                                detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                                var newgrn = new InventoryIssueHistory
                                                {
                                                    TotalAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                                };
                                                GRNCalculateList.Add(newgrn);
                                                //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                                totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                            }
                                            else
                                            {
                                                detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                                var newgrn = new InventoryIssueHistory
                                                {
                                                    TotalAmount = Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                                };
                                                GRNCalculateList.Add(newgrn);
                                                //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                                totalGRNQty += Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor);
                                            }
                                            item.IssueRequest = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.IssueRequest).FirstOrDefault();
                                        }


                                        currentId++;
                                        var issueDetail = new InventoryIssueDetail
                                        {
                                            Id = MakePK(inventoryIssue.Id, currentId, 2),
                                            InventoryIssueId = inventoryIssue.Id,
                                            IsAsset = FlagIsAsset,//false,
                                                                  //InventoryIssue = inventoryIssue,
                                            InventoryMaterialId = invMaterialId,
                                            BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                            TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                                            AvgRate = Math.Round(invMaterial.AvgRate, 4),
                                            Policy = "N/A",

                                            TransactionQty = Math.Round(totalGRNQty, 2), //stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
                                            PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
                                            PolicyAmount = Math.Round(detailtrnAmount, 2),
                                            BaseQty = Math.Round(totalGRNQty, 2),//stockList.Sum(r => r.RequisitionQty),
                                            AvgAmount = Math.Round((totalGRNQty * invMaterial.AvgRate), 2),
                                            BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                            ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                            CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
                                            Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),
                                            // OSTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.OSTransformationPOId).FirstOrDefault(),
                                            //   OSTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.OSTransformationPODetailId).FirstOrDefault(),
                                            JWTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.JWTransformationPODetailId).FirstOrDefault(),
                                            //JWTCInputId = entities.Where(r => r.MaterialMasterId != invMaterial.MaterialMasterId && r.ArticleId != invMaterial.ArticleId).Select(t => t.JWInputItemId).FirstOrDefault(),
                                            //  JWTCInputId = entities.Where(r => r.MaterialMasterId == null && r.ArticleId == null).Select(t => t.JWInputItemId).FirstOrDefault(),
                                            ModelState = ModelState.Added
                                        };
                                        decimal tempPolicyAmount = 0;
                                        if (invMaterial.ArticleId.IsNotNull())
                                        {
                                            // start

                                            var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                                            foreach (var item in stockList)
                                            {

                                                if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                                                if (item.TransactionUoMId != item.BaseUOMId)
                                                    // totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                                                    totalReqQty = Convert.ToDecimal(item.RequisitionQty * item.BaseUoMFactor);
                                                else
                                                    totalReqQty = item.RequisitionQty;
                                                historyId++;
                                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                                var history = new InventoryIssueHistory
                                                {
                                                    Id = MakePK(issueDetail.Id, historyId, 2),
                                                    InventoryIssueDetailId = issueDetail.Id,
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                    Qty = totalReqQty, //item.RequisitionQty,
                                                                       //Rate = Convert.ToDecimal(item.BaseRate),
                                                                       //Rate = Math.Round((SelectedGRN.TotalAmount / item.RequisitionQty), 4),
                                                                       //TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                                       //Rate = Math.Round((SelectedGRN.TotalAmount / totalReqQty), 4),//Old calculation
                                                    Rate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),//totalGRNQty
                                                                                                                        //TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                    TotalAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2),
                                                    IssueRequestDetailId = item.IssueRequest,
                                                    IssueReturnQty = 0,
                                                    BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                    TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2)
                                                };
                                                //policyAmmount += history.Qty * history.Rate;

                                                tempPolicyAmount += Math.Round(Convert.ToDecimal(history.TotalMaterialBooksCurrencyAmount), 4);

                                                builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
										,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                                rdBuilder.Append(builderSql);
                                                AuditService.AddedLog(history);
                                                _issueHistoryRepository.Insert(history);



                                                //Mapping Data=========================================================
                                                if (entitiesAll.IsNotNull())
                                                {
                                                    foreach (var itemall in entitiesAll)
                                                    {
                                                        var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + itemall.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                                        if (receiveDetailList1.IsNotNull())
                                                        {
                                                            foreach (var receiveDetailListNew in receiveDetailList1)
                                                            {


                                                                //count++;
                                                                //if (count == 1)
                                                                //{
                                                                //    if (((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) > issueDetail.TransactionQty)
                                                                //    {

                                                                //        issueDetail.TransactionQty =Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //        //temp += itemDetail.TransactionQty;
                                                                //        isQtyAlocated = false;

                                                                //    }
                                                                //    else if (((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) < issueDetail.TransactionQty)
                                                                //    {
                                                                //        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //        temp = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //        issueDetail.TransactionQty = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //        isQtyAlocated = true;

                                                                //    }
                                                                //    else
                                                                //    {
                                                                //        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //        issueDetail.TransactionQty = Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //        isQtyAlocated = true;

                                                                //    }
                                                                //}
                                                                //if (count > 1)
                                                                //{
                                                                //    if (isQtyAlocated == true)
                                                                //    {
                                                                //        if ((Convert.ToDecimal(receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) > temp)
                                                                //        {
                                                                //            //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //            isQtyAlocated = false;
                                                                //        }
                                                                //        if ((Convert.ToDecimal(receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) < temp)
                                                                //        {
                                                                //            //temp = temp - issue.TransactionQtyForPO;
                                                                //            temp = Convert.ToDecimal(temp - ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor));
                                                                //            //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //            isQtyAlocated = true;
                                                                //        }
                                                                //        else
                                                                //        {
                                                                //            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = temp;
                                                                //            isQtyAlocated = true;

                                                                //        }

                                                                //    }
                                                                //    else
                                                                //    {
                                                                //        issueDetail.TransactionQty = 0;
                                                                //    }
                                                                //}


                                                                var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                                {
                                                                    Id = GetIssueDetailAndIssueRequestMapPK(),
                                                                    InventoryIssueDetailId = issueDetail.Id,
                                                                    IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                                    Qty = receiveDetailListNew.IssueRequestBOQMapQty,
                                                                    //AutoAllocate = true

                                                                };
                                                                AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                                _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                                            }
                                                        }


                                                    }
                                                }


                                            }


                                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
                                            rdBuilder.Append(builderSql);

                                            // End

                                        }
                                        issueDetail.PolicyAmount = tempPolicyAmount;
                                        issueDetail.PolicyRate = Math.Round(tempPolicyAmount / issueDetail.TransactionQty, 4);
                                        AuditService.AddedLog(issueDetail);
                                        _issueDetailService.InsertGraph(issueDetail);
                                        tempPolicyAmount = 0;

                                        //===================

                                    }
                                    //       }
                                }


                                //        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                            }
                            catch (CustomException)
                            {
                                throw;
                            }
                            #endregion
                        }


                        _unitOfWork.SaveChanges();
                        if (!string.IsNullOrEmpty(JWArtId))
                        {
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        }

                        flag = false;
                        _unitOfWork.Commit();
                        if (TabType == "Transformation")
                        {
                            SaveJWIssueTransformationChild(entities, _pk);
                        }
                        else
                        {
                            SaveJWIssueValAddedChild(entities, _pk);
                        }

                    }

                }
                else
                {
                    var GRNCalculateList = new List<InventoryIssueHistory>();
                    if (entities.IsNotNull())
                    {
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        //     var _pk = GetPK();
                        var inventoryMaterialList = _inventoryMaterialService.GetJWInventoryMaterialListByUpToSku(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
                        var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
                        foreach (var item in entities)// update view model (inventory material field)
                        {
                            //  JWArtId += ",'" + item.ArticleId + "' ";
                            if (item.ArticleId.IsNotNull())
                            {
                                if (string.IsNullOrEmpty(JWArtId))
                                {
                                    JWArtId = item.ArticleId;
                                }

                                var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
                                    && t.FirstCharacteristicsId == item.FirstCharacteristicsId
                                    && t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
                                    && t.SecondCharacteristicsId == item.SecondCharacteristicsId
                                    && t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
                                    && t.ThirdCharacteristicsId == item.ThirdCharacteristicsId
                                    && t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
                                    //&& t.CountryId == item.CountryId
                                    && t.CompanyId == inventoryIssue.CompanyId && t.PlantId == inventoryIssue.PlantId // && t.CountryId == item.CountryId
                                   );

                                if (im.IsNotNull())
                                {

                                    if (im.TotalQty < item.TransactionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");
                                    //     item.InventoryIssueId = _pk;
                                    item.InventoryIssueId = inventoryIssue.Id;
                                    item.InventoryMaterialId = im.Id;
                                    item.CompanyGroupId = im.CompanyGroupId;
                                    item.CompanyId = inventoryIssue.CompanyId;
                                    item.PlantId = inventoryIssue.PlantId;
                                    item.CurrencyId = currencyId;
                                    item.MaterialStorageId = null;
                                    item.MaterialMasterId = im.MaterialMasterId;
                                    item.ArticleId = im.ArticleId;
                                    item.FirstCharacteristicsId = im.FirstCharacteristicsId;
                                    item.FirstCharacteristicsValueId = im.FirstCharacteristicsValueId;
                                    item.SecondCharacteristicsId = im.SecondCharacteristicsId;
                                    item.SecondCharacteristicsValueId = im.SecondCharacteristicsValueId;
                                    item.ThirdCharacteristicsId = im.ThirdCharacteristicsId;
                                    item.ThirdCharacteristicsValueId = im.ThirdCharacteristicsValueId;
                                    item.TotalQty = im.TotalQty;
                                    item.AvgRate = im.AvgRate;

                                }
                            }

                        }// update view model (inventory material field)
                        inventoryIssue.CurrencyId = currencyId;
                        inventoryIssue.ProductionOrderId = inventoryIssue.ProductionOrderId;
                        inventoryIssue.ContractId = inventoryIssue.ContractId;
                        inventoryIssue.OrderRefNo = inventoryIssue.OrderRefNo;

                        inventoryIssue.JobWorkContractId = inventoryIssue.JobWorkContractId;
                        inventoryIssue.ContractType = inventoryIssue.ContractType;
                        inventoryIssue.Types = inventoryIssue.Types;

                        inventoryIssue.RefferenceNo = inventoryIssue.RefferenceNo;
                        inventoryIssue.IssueType = inventoryIssue.IssueType;
                        inventoryIssue.EmployeeId = inventoryIssue.EmployeeId;

                        inventoryIssue.MaterialStorageId = inventoryIssue.MaterialStorageId;
                        inventoryIssue.EmployeeId = inventoryIssue.EmployeeId;

                        inventoryIssue.IssueDate = inventoryIssue.IssueDate;
                        inventoryIssue.EntityId = inventoryIssue.EntityId;
                        inventoryIssue.PlantId = inventoryIssue.PlantId;

                        inventoryIssue.CompanyGroupId = inventoryIssue.CompanyGroupId;
                        inventoryIssue.CompanyId = inventoryIssue.CompanyId;

                        // inventoryIssue.Id = _pk;
                        inventoryIssue.Id = inventoryIssue.Id;
                        //  InsertGraph(inventoryIssue);
                        UpdateGraph(inventoryIssue);
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = "";
                        //_issueDetailService.InsertRange(entities, specificStockList, inventoryIssue);


                        #region ===========IssueDetail And IssueHistory And Update GRN And Stock=======


                        if (!string.IsNullOrEmpty(JWArtId))
                        {
                            try
                            {
                                //    var inventoryMaterialIds = new string[] { };

                                var uiList = entities.ToList();
                                var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                                var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();
                                //    inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).Distinct().ToArray();

                                var specificInvaterialIds = new string[] { };
                                //       var specificInventoryReceiveDetailIds = new string[] { };
                                var maIds = new string[] { };
                                if (specificStockList.IsNotNull())
                                {
                                    specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                                    //          specificInventoryReceiveDetailIds = specificStockList.Select(t => t.InventoryReceiveDetailId).Distinct().ToArray();
                                    maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                                    for (int i = uiList.Count() - 1; i >= 0; i--)
                                    {
                                        var row = uiList.ElementAt(i);
                                        if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                                            uiList.RemoveAt(i);
                                    }
                                }
                                else maIds = inventoryMaterialIds;
                                var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty,IRD.InventorySalesQty,IRD.InventoryScrapQty,IRD.PurchaseReturnQty,IRD.IssueReturnQty,IRD.ReductionByAdjustmentQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @") AND  IRD.BaseQty !=IRD.BaseIssueQty
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
									      AND IR.Status='Posting' 
										  AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0)-isnull(IRD.InventoryTransferQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.IssueDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();

                                if (receiveDetailList.IsNotNull())
                                {
                                    foreach (var issue in uiList)
                                    {
                                        if (issue.ArticleId.IsNotNull())
                                        {
                                            var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                                            decimal detailtrnAmount = 0;
                                            decimal totalGRNQty = 0;
                                            /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* (TotalMmaterialTrnAmount/MaterialTrnRate))*/
                                            /*Rate= Amount/Sum GRN Qty */

                                            if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                                                //input.BaseRate = receiveDetailRow.BaseAmount / receiveDetailRow.BaseQty;
                                                issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                                            else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                                            if (issue.TransactionUoMId != issue.BaseUOMId)
                                                issue.BaseQty = Convert.ToDecimal(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);

                                            decimal IssueTransactionQty = issue.TransactionQty;
                                            foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                                            {

                                                if (IssueTransactionQty <= 0)
                                                    break;

                                                //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                                //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(IIH.TotalAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(ISH.TotalBaseAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
                                                //																						   FROM trn.InventoryReceiveDetail IRD  
                                                //																							left JOIN [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
                                                //																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
                                                //																						   WHERE  IIH.InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                                decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																FROM (
																		SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD
																		left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																		UNION All
																		SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																		UNION All
																		SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																		FROM TRN.InventoryReceiveDetail IRD	
																		LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																)x
																WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                                                decimal RemainingGRNQty = Convert.ToDecimal((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty)) + item.IssueReturnQty);
                                                decimal IssueDeduactionQty = 0;


                                                if (RemainingGRNQty <= IssueTransactionQty)
                                                {
                                                    IssueDeduactionQty = RemainingGRNQty;
                                                    IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                                    RemainingGRNQty = 0;

                                                }
                                                else
                                                {
                                                    IssueDeduactionQty = IssueTransactionQty;
                                                    RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                                    IssueTransactionQty = 0;
                                                }

                                                //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                                                if (item.TransactionUoMId == issue.TransactionUoMId)
                                                {

                                                    detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                                    var newgrn = new InventoryIssueHistory
                                                    {
                                                        TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = IssueDeduactionQty

                                                    };
                                                    GRNCalculateList.Add(newgrn);
                                                    //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                                    totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                                }
                                                else
                                                {
                                                    detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                                                    var newgrn = new InventoryIssueHistory
                                                    {
                                                        TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - IssueDeduactionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = IssueDeduactionQty
                                                    };
                                                    GRNCalculateList.Add(newgrn);
                                                    //detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));

                                                    totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);
                                                }
                                                //}
                                            }

                                            if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                                            currentId++;
                                            //totalGRNQty = issue.TransactionQty;
                                            if (issue.BaseQty == null) issue.BaseQty = totalGRNQty;
                                            var detail = new InventoryIssueDetail
                                            {
                                                Id = MakePK(inventoryIssue.Id, currentId, 2),
                                                InventoryIssueId = inventoryIssue.Id,
                                                IsAsset = FlagIsAsset,//false,
                                                                      //InventoryIssue = inventoryIssue,
                                                InventoryMaterialId = issue.InventoryMaterialId,
                                                TransactionQty = issue.TransactionQty,
                                                BaseQty = issue.BaseQty,
                                                BaseUOMId = issue.BaseUOMId,
                                                TransactionUoMId = issue.TransactionUoMId,

                                                //TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                                AvgRate = Math.Round(issue.AvgRate, 4),
                                                AvgAmount = Math.Round((issue.TransactionQty * issue.AvgRate), 2),
                                                Policy = receiveDetailRow.Policy,

                                                PolicyAmount = Math.Round(detailtrnAmount, 2),
                                                PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),

                                                //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                                                //PolicyRate = detailtrnAmount / totalGRNQty,
                                                BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                                ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                                Comments = issue.Comments,
                                                CostCenterId = issue.CostCenterId,
                                                // OSTransformationPOId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.OSTransformationPOId).FirstOrDefault(),
                                                OSTransformationPOId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.OSTransformationPODetailId).FirstOrDefault(),
                                                ModelState = ModelState.Added

                                                //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                                                //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                                            };
                                            var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{detail.Id}'").First();
                                            // single entry (history)
                                            //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                            //if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                                            var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == receiveDetailRow.Id).FirstOrDefault();
                                            if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty - receiveDetailRow.PurchaseReturnQty - receiveDetailRow.ReductionByAdjustmentQty - receiveDetailRow.InventorySalesQty - receiveDetailRow.InventoryScrapQty) + receiveDetailRow.IssueReturnQty)
                                            {
                                                historyId++;
                                                var history = new InventoryIssueHistory
                                                {
                                                    Id = MakePK(detail.Id, historyId, 2),
                                                    InventoryIssueDetailId = detail.Id,
                                                    InventoryReceiveDetailId = receiveDetailRow.Id,
                                                    Qty = SelectedGRN.Qty,
                                                    //Rate = Convert.ToDecimal(issue.BaseRate),
                                                    //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                    //Rate = detailtrnAmount / totalGRNQty,
                                                    //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                                    Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                                    TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                    IsCapitalize = false,
                                                    IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                                    IssueReturnQty = 0,
                                                    BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(issue.BooksCurrencyBaseRate), 4),
                                                    TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * issue.BooksCurrencyBaseRate), 2)
                                                };
                                                //detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                                                //detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);

                                                builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + @"'
									 , BaseIssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + "' WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";
                                                rdBuilder.Append(builderSql);
                                                AuditService.AddedLog(history);
                                                _issueHistoryRepository.Insert(history);


                                            }
                                            // multiple entry (history)
                                            else
                                            {
                                                var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                                                var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                                           - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                                                //if (tqty < input.TransactionQty) throw new CustomException("Stock 0");
                                                if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                                                decimal policyAmount = 0;
                                                //decimal qtyDifference = input.TransactionQty;
                                                decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);

                                                foreach (var item in rdList)
                                                {
                                                    historyId++;
                                                    if (item.TransactionUoMId != item.BaseUOMId)
                                                        //input.BaseRate = item.BaseAmount / item.BaseQty;
                                                        issue.BaseRate = item.MaterialTranAmount / item.BaseQty;
                                                    //else input.BaseRate = item.TransactionRate;
                                                    else issue.BaseRate = item.MaterialTranRate;

                                                    //var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty);
                                                    var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty - item.PurchaseReturnQty - item.ReductionByAdjustmentQty - item.InventorySalesQty - item.InventoryScrapQty) + Convert.ToDecimal(item.IssueReturnQty);
                                                    // (10 - 3)//Issueable Qty
                                                    //if (issueQty != 0)
                                                    //{

                                                    if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                                    {
                                                        policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                                        qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                                        issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                                    }
                                                    else // (6 < 7) (qtyDifference < issueQty)
                                                    {
                                                        //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                                        issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                                        policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                                        qtyDifference = 0;
                                                    }
                                                    SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.Id).FirstOrDefault();
                                                    var history = new InventoryIssueHistory
                                                    {
                                                        Id = MakePK(detail.Id, historyId, 2),
                                                        InventoryIssueDetailId = detail.Id,
                                                        InventoryReceiveDetailId = item.Id,
                                                        Qty = SelectedGRN.Qty,//Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO
                                                                              //Qty = Convert.ToDecimal(issueQty),//TODO
                                                                              // Qty = Convert.ToDecimal(qtyDifference),//TODO
                                                                              //Rate = Convert.ToInt32(issue.BaseRate),
                                                                              //Rate = Convert.ToDecimal(issue.BaseRate),
                                                                              //Rate = Convert.ToDecimal(issue.ToCurrencyRate),
                                                                              //Rate = detailtrnAmount / totalGRNQty,
                                                                              //TotalAmount = Convert.ToDecimal(detailtrnAmount),
                                                        Rate = Math.Round((SelectedGRN.TotalAmount / SelectedGRN.Qty), 4),
                                                        TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                        IsCapitalize = false,
                                                        IssueRequestDetailId = receiveDetailRow.IssueRequest,
                                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(SelectedGRN.Qty * item.BooksCurrencyBaseRate), 2)

                                                    };

                                                    AuditService.AddedLog(history);
                                                    _issueHistoryRepository.Insert(history);

                                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET BaseIssueQty='" + Convert.ToDecimal(issueQty) + "',IssueQty='" + Convert.ToDecimal(issueQty) + "'  WHERE Id='" + item.Id + "'";
                                                    rdBuilder.Append(builderSql);
                                                    if (qtyDifference == 0)
                                                        break;
                                                    //}
                                                }

                                                //detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                                                //detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                                            }
                                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - issue.TransactionQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                                            rdBuilder.Append(builderSql);
                                            AuditService.AddedLog(detail);
                                            _issueDetailService.InsertGraph(detail);

                                            //Mapping Data=========================================================
                                            var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + issue.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                            if (receiveDetailList1.Count > 0)
                                            {
                                                bool isQtyAlocated = true;
                                                decimal temp = 0;
                                                int count = 0;
                                                foreach (var receiveDetailListNew in receiveDetailList1)
                                                {


                                                    count++;
                                                    if (count == 1)
                                                    {
                                                        if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > detail.TransactionQty)
                                                        {

                                                            detail.TransactionQty = detail.TransactionQty;
                                                            //temp += itemDetail.TransactionQty;
                                                            isQtyAlocated = false;

                                                        }
                                                        else if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < detail.TransactionQty)
                                                        {
                                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                            temp = (detail.TransactionQty - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                            detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                            isQtyAlocated = true;

                                                        }
                                                        else
                                                        {
                                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                            detail.TransactionQty = detail.TransactionQty;
                                                            isQtyAlocated = true;

                                                        }
                                                    }
                                                    if (count > 1)
                                                    {
                                                        if (isQtyAlocated == true)
                                                        {
                                                            if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > temp)
                                                            {
                                                                //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                                detail.TransactionQty = detail.TransactionQty;
                                                                isQtyAlocated = false;
                                                            }
                                                            if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < temp)
                                                            {
                                                                //temp = temp - issue.TransactionQtyForPO;
                                                                temp = (temp - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
                                                                //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                                detail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
                                                                isQtyAlocated = true;
                                                            }
                                                            else
                                                            {
                                                                //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                detail.TransactionQty = temp;
                                                                isQtyAlocated = true;

                                                            }

                                                        }
                                                        else
                                                        {
                                                            detail.TransactionQty = 0;
                                                        }
                                                    }


                                                    var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                    {
                                                        Id = GetIssueDetailAndIssueRequestMapPK(),
                                                        InventoryIssueDetailId = detail.Id,
                                                        IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                        Qty = detail.TransactionQty
                                                        //AutoAllocate = true

                                                    };
                                                    AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                    _IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
                                                }
                                            }

                                            //===================

                                        }
                                    }

                                }
                                if (specificStockList.IsNotNull())
                                {
                                    //foreach (var RecId in specificInventoryReceiveDetailIds)
                                    //{

                                    foreach (var invMaterialId in specificInvaterialIds)
                                    {
                                        var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                                        var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                                        var totalReqQty = 0M;
                                        decimal detailtrnAmount = 0;
                                        decimal totalGRNQty = 0;


                                        foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                                        {
                                            decimal IssueTransactionQty = item.RequisitionQty;
                                            decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(x.ISHTotalBaseAmount),0)+isnull(sum(x.PRTotalMaterialTranAmount),0)+isnull(sum(x.PSAHTotalAmount),0)+isnull(sum(x.IIHTotalAmount),0) +isnull(sum(x.InvSTotalAmount),0) +isnull(sum(x.ITHTotalAmount),0)) -isnull(sum(x.IIRTotalAmount),0))  
																														FROM (
																																SELECT 	IRD.Id,0 ITHTotalAmount, ISNULL(ISH.TotalBaseAmount,0) ISHTotalBaseAmount,0 PRTotalMaterialTranAmount,0 PSAHTotalAmount,0 IIHTotalAmount,0 InvSTotalAmount,0 IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD
																																left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,isnull(PR.TotalMaterialTranAmount,0) PRTotalMaterialTranAmount,0,0,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,isnull(PSAH.TotalAmount,0) PSAHTotalAmount,0,0,0			
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,isnull(IIH.TotalAmount,0) IIHTotalAmount,0,0		
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,isnull(InvS.TotalAmount,0) InvSTotalAmount,0	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id	
																																UNION All
																																SELECT IRD.Id,0 ,0,0,0,0,0,isnull(IIR.TotalAmount,0)	IIRTotalAmount	
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id		
																																UNION All
																																SELECT IRD.Id,isnull(ITH.TotalAmount,0) ITHTotalAmount,0,0,0,0,0,0
																																FROM TRN.InventoryReceiveDetail IRD	
																																LEFT join TRN.InventoryTransferHistory ITH ON ITH.InventoryReceiveDetailId=IRD.Id		
																														)x
																														WHERE x.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                                            decimal RemainingGRNQty = Convert.ToDecimal((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty)) + item.IssueReturnQty);
                                            decimal IssueDeduactionQty = 0;


                                            if (RemainingGRNQty <= IssueTransactionQty)
                                            {
                                                IssueDeduactionQty = RemainingGRNQty;
                                                IssueTransactionQty = IssueTransactionQty - RemainingGRNQty;
                                                RemainingGRNQty = 0;

                                            }
                                            else
                                            {
                                                IssueDeduactionQty = IssueTransactionQty;
                                                RemainingGRNQty = RemainingGRNQty - IssueTransactionQty;
                                                IssueTransactionQty = 0;
                                            }
                                            if (item.TransactionUoMId == item.BaseUOMId) //entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                                            {
                                                detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                                var newgrn = new InventoryIssueHistory
                                                {
                                                    TotalAmount = Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - IssueDeduactionQty) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                                };
                                                GRNCalculateList.Add(newgrn);

                                                //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                                totalGRNQty += Convert.ToDecimal(IssueDeduactionQty);

                                            }
                                            else
                                            {
                                                detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty)));
                                                var newgrn = new InventoryIssueHistory
                                                {
                                                    TotalAmount = Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.BaseQty - (item.BaseIssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor)) * (item.TotalMaterialBooksCurrencyAmount / item.BaseQty))),
                                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                                };
                                                GRNCalculateList.Add(newgrn);
                                                //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                                                totalGRNQty += Convert.ToDecimal(IssueDeduactionQty * item.BaseUoMFactor);
                                            }
                                            item.IssueRequest = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.IssueRequest).FirstOrDefault();
                                        }


                                        currentId++;
                                        var issueDetail = new InventoryIssueDetail
                                        {
                                            //   Id = MakePK(inventoryIssue.Id, currentId, 2),
                                            Id = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.InventoryIssueDetailId).FirstOrDefault(),
                                            InventoryIssueId = inventoryIssue.Id,
                                            IsAsset = FlagIsAsset,//false,
                                                                  //InventoryIssue = inventoryIssue,
                                            InventoryMaterialId = invMaterialId,
                                            BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                            TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                                            AvgRate = Math.Round(invMaterial.AvgRate, 4),
                                            Policy = "N/A",

                                            TransactionQty = Math.Round(totalGRNQty, 2), //stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
                                            PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
                                            PolicyAmount = Math.Round(detailtrnAmount, 2),
                                            BaseQty = Math.Round(totalGRNQty, 2),//stockList.Sum(r => r.RequisitionQty),
                                            AvgAmount = Math.Round((totalGRNQty * invMaterial.AvgRate), 2),
                                            BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                                            ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                                            CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
                                            Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),
                                            // OSTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.OSTransformationPOId).FirstOrDefault(),
                                            //     OSTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.OSTransformationPODetailId).FirstOrDefault(),
                                            JWTransformationPOId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId && r.ArticleId == invMaterial.ArticleId).Select(t => t.JWTransformationPODetailId).FirstOrDefault(),
                                            //JWTCInputId = entities.Where(r => r.MaterialMasterId != invMaterial.MaterialMasterId && r.ArticleId != invMaterial.ArticleId).Select(t => t.JWInputItemId).FirstOrDefault(),
                                            //  JWTCInputId = entities.Where(r => r.MaterialMasterId == null && r.ArticleId == null).Select(t => t.JWInputItemId).FirstOrDefault(),
                                            ModelState = ModelState.Added
                                        };
                                        decimal tempPolicyAmount = 0;
                                        if (invMaterial.ArticleId.IsNotNull())
                                        {
                                            // start

                                            // var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                                            foreach (var item in stockList)
                                            {
                                                var IRHUPId = "";
                                                var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}' and InventoryReceiveDetailId='{item.InventoryReceiveDetailId}'").First();
                                                if (historyId != 0)
                                                {
                                                    IRHUPId = _issueHistoryRepository.SqlQuery<string>($"SELECT Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}' and InventoryReceiveDetailId='{item.InventoryReceiveDetailId}'").First();
                                                }
                                                if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                                                if (item.TransactionUoMId != item.BaseUOMId)
                                                    // totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                                                    totalReqQty = Convert.ToDecimal(item.RequisitionQty * item.BaseUoMFactor);
                                                else
                                                    totalReqQty = item.RequisitionQty;
                                                // historyId++;
                                                var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                                                if (historyId == 0)
                                                {
                                                    var history = new InventoryIssueHistory
                                                    {

                                                        Id = MakePK(issueDetail.Id, historyId, 2),
                                                        InventoryIssueDetailId = issueDetail.Id,
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = totalReqQty, //item.RequisitionQty,
                                                                           //Rate = Convert.ToDecimal(item.BaseRate),
                                                                           //Rate = Math.Round((SelectedGRN.TotalAmount / item.RequisitionQty), 4),
                                                                           //TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                        Rate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),//totalGRNQty
                                                        TotalAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2),//Convert.ToDecimal(detailtrnAmount),
                                                        IssueRequestDetailId = item.IssueRequest,
                                                        IssueReturnQty = 0,
                                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2)
                                                    };
                                                    //policyAmmount += history.Qty * history.Rate;

                                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
										,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                                    rdBuilder.Append(builderSql);
                                                    AuditService.AddedLog(history);
                                                    _issueHistoryRepository.Insert(history);

                                                    //AuditService.UpdatedLog(history);
                                                    //_issueHistoryRepository.Update(history);
                                                }
                                                else
                                                {
                                                    var history = new InventoryIssueHistory
                                                    {

                                                        Id = IRHUPId,
                                                        InventoryIssueDetailId = issueDetail.Id,
                                                        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                                        Qty = totalReqQty, //item.RequisitionQty,
                                                                           //Rate = Convert.ToDecimal(item.BaseRate),
                                                                           //Rate = Math.Round((SelectedGRN.TotalAmount / item.RequisitionQty), 4),
                                                                           //TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
                                                        Rate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),//totalGRNQty
                                                        TotalAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2),//Convert.ToDecimal(detailtrnAmount),
                                                        IssueRequestDetailId = item.IssueRequest,
                                                        IssueReturnQty = 0,
                                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate), 4),
                                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate), 2)
                                                    };
                                                    //policyAmmount += history.Qty * history.Rate;

                                                    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
										,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                                    rdBuilder.Append(builderSql);
                                                    //AuditService.AddedLog(history);
                                                    //_issueHistoryRepository.Insert(history);

                                                    AuditService.UpdatedLog(history);
                                                    _issueHistoryRepository.Update(history);
                                                }



                                                //                                      builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
                                                //,BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                                                //                                      rdBuilder.Append(builderSql);
                                                //                                      //AuditService.AddedLog(history);
                                                //                                      //_issueHistoryRepository.Insert(history);

                                                //                                      AuditService.UpdatedLog(history);
                                                //                                      _issueHistoryRepository.Update(history);



                                                //Mapping Data=========================================================
                                                if (entitiesAll.IsNotNull())
                                                {
                                                    foreach (var itemall in entitiesAll)
                                                    {
                                                        var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + itemall.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
                                                        if (receiveDetailList1.IsNotNull())
                                                        {
                                                            foreach (var receiveDetailListNew in receiveDetailList1)
                                                            {


                                                                //count++;
                                                                //if (count == 1)
                                                                //{
                                                                //    if (((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) > issueDetail.TransactionQty)
                                                                //    {

                                                                //        issueDetail.TransactionQty =Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //        //temp += itemDetail.TransactionQty;
                                                                //        isQtyAlocated = false;

                                                                //    }
                                                                //    else if (((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) < issueDetail.TransactionQty)
                                                                //    {
                                                                //        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //        temp = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //        issueDetail.TransactionQty = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //        isQtyAlocated = true;

                                                                //    }
                                                                //    else
                                                                //    {
                                                                //        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //        issueDetail.TransactionQty = Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //        isQtyAlocated = true;

                                                                //    }
                                                                //}
                                                                //if (count > 1)
                                                                //{
                                                                //    if (isQtyAlocated == true)
                                                                //    {
                                                                //        if ((Convert.ToDecimal(receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) > temp)
                                                                //        {
                                                                //            //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = Convert.ToDecimal(issueDetail.TransactionQty * item.BaseUoMFactor);
                                                                //            isQtyAlocated = false;
                                                                //        }
                                                                //        if ((Convert.ToDecimal(receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor) < temp)
                                                                //        {
                                                                //            //temp = temp - issue.TransactionQtyForPO;
                                                                //            temp = Convert.ToDecimal(temp - ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor));
                                                                //            //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = Convert.ToDecimal((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) * item.BaseUoMFactor);
                                                                //            isQtyAlocated = true;
                                                                //        }
                                                                //        else
                                                                //        {
                                                                //            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                                                //            issueDetail.TransactionQty = temp;
                                                                //            isQtyAlocated = true;

                                                                //        }

                                                                //    }
                                                                //    else
                                                                //    {
                                                                //        issueDetail.TransactionQty = 0;
                                                                //    }
                                                                //}


                                                                var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
                                                                {
                                                                    Id = GetIssueDetailAndIssueRequestMapPK(),
                                                                    InventoryIssueDetailId = issueDetail.Id,
                                                                    IssueRequestBOQMapId = receiveDetailListNew.Id,
                                                                    Qty = receiveDetailListNew.IssueRequestBOQMapQty,
                                                                    //AutoAllocate = true

                                                                };
                                                                //AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
                                                                //_IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);

                                                                AuditService.UpdatedLog(IssueDetailAndIssueRequestMapNew);
                                                                _IssueDetailAndIssueRequestMapRepository.Update(IssueDetailAndIssueRequestMapNew);
                                                            }
                                                        }


                                                    }
                                                }


                                            }


                                            builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
                                            rdBuilder.Append(builderSql);

                                            // End

                                        }

                                        //AuditService.AddedLog(issueDetail);
                                        //_issueDetailService.InsertGraph(issueDetail);
                                        issueDetail.PolicyAmount = tempPolicyAmount;
                                        issueDetail.PolicyRate = Math.Round(tempPolicyAmount / issueDetail.TransactionQty, 4);
                                        AuditService.UpdatedLog(issueDetail);
                                        _issueDetailService.UpdateGraph(issueDetail);

                                        tempPolicyAmount = 0;
                                        //===================

                                    }
                                    //        }
                                }


                                //        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                            }
                            catch (CustomException)
                            {
                                throw;
                            }
                            #endregion
                        }


                        _unitOfWork.SaveChanges();
                        if (!string.IsNullOrEmpty(JWArtId))
                        {
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                        }

                        flag = false;
                        _unitOfWork.Commit();
                        if (TabType == "Transformation")
                        {
                            SaveJWIssueTransformationChild(entities, inventoryIssue.Id);
                        }
                        else
                        {
                            SaveJWIssueValAddedChild(entities, inventoryIssue.Id);
                        }

                    }
                }
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

        // Save Issue Tranformation Wihtout Material

        public void SaveJWIssueTransformationChild(IEnumerable<InventoryMaterialViewModel> entities, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var JWItemId = "' '";
                var OtMatId = "' '";

                foreach (var empitem in entities)
                {
                    if (empitem.ArticleId.IsNull())
                    {
                        JWItemId += ",'" + empitem.JWInputItemId + "' ";
                        //        OtMatId += ",'" + empitem.OSTransformationPOId + "' ";
                        OtMatId += ",'" + empitem.JWTransformationPODetailId + "' ";
                    }


                }
                con.OpenDataSetThroughAdapter("select * from TRN.InventoryIssueDetail where JWTransformationPOId IN ( " + OtMatId + ") and JobWorkTCInputId IN (" + JWItemId + ") and InventoryIssueId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in entities)
                {
                    if (item.ArticleId.IsNull())
                    {

                        ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPOId='" + item.JWTransformationPODetailId + "' and JobWorkTCInputId='" + item.JWInputItemId + "' ";

                        if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = ExistOrNot.Tables[0].NewRow();
                            dr["Id"] = GetTransformationChildPK();

                            dr["InventoryIssueId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseUOMId"] = item.BaseUOMId;
                            dr["CostCenterId"] = item.CostCenterId;
                            dr["JWTransformationPOId"] = item.JWTransformationPODetailId;
                            dr["JobWorkTCInputId"] = item.JWInputItemId;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            //dr["UpdatedBy"] = identity.Name;
                            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            //dr["UpdatedFromIP"] = identity.IPAddress;

                            ExistOrNot.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPOId='" + item.JWTransformationPODetailId + "' and JobWorkTCInputId='" + item.JWInputItemId + "' ";

                            if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = ExistOrNot.Tables[0].NewRow();
                                dr["Id"] = GetTransformationChildPK();

                                dr["InventoryIssueId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.BaseUOMId;
                                dr["CostCenterId"] = item.CostCenterId;
                                dr["JWTransformationPOId"] = item.JWTransformationPODetailId;
                                dr["JobWorkTCInputId"] = item.JWInputItemId;

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                ExistOrNot.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                //edit
                                DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["InventoryIssueId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.BaseUOMId;
                                dr["CostCenterId"] = item.CostCenterId;
                                dr["JWTransformationPOId"] = item.JWTransformationPODetailId;
                                dr["JobWorkTCInputId"] = item.JWInputItemId;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;


                                dr.EndEdit();
                            }


                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                //         return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveJWIssueValAddedChild(IEnumerable<InventoryMaterialViewModel> entities, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var JWOrderWiseId = "' '";
                var OtMatId = "' '";

                foreach (var empitem in entities)
                {
                    if (empitem.ArticleId.IsNull())
                    {
                        if (empitem.JWOrderWiseId.IsNotNull())
                        {
                            JWOrderWiseId += ",'" + empitem.JWOrderWiseId + "' ";
                            //       OtMatId += ",'" + empitem.OSTransformationPOId + "' ";
                            OtMatId += ",'" + empitem.JWTransformationPODetailId + "' ";
                        }
                        else
                        {
                            OtMatId += ",'" + empitem.JWTransformationPODetailId + "' ";
                        }

                    }
                }

                if (JWOrderWiseId.IsNotNull())
                {
                    con.OpenDataSetThroughAdapter("select * from TRN.InventoryIssueDetail where JWTransformationPOId IN ( " + OtMatId + ") and JobWorkOrderWiseId IN (" + JWOrderWiseId + ") and InventoryIssueId='" + MasterId + "'  ", out ExistOrNot, false, "1");
                }
                else
                {
                    con.OpenDataSetThroughAdapter("select * from TRN.InventoryIssueDetail where JWTransformationPOId IN ( " + OtMatId + ") and InventoryIssueId='" + MasterId + "'  ", out ExistOrNot, false, "1");
                }


                foreach (var item in entities)
                {
                    if (item.ArticleId.IsNull())
                    {
                        if (item.JWOrderWiseId.IsNotNull())
                        {
                            ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPOId='" + item.JWTransformationPODetailId + "' and JobWorkOrderWiseId='" + item.JWOrderWiseId + "' ";
                        }
                        else
                        {
                            ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPOId='" + item.JWTransformationPODetailId + "' ";
                        }



                        if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = ExistOrNot.Tables[0].NewRow();
                            dr["Id"] = GetTransformationChildPK();

                            dr["InventoryIssueId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseUOMId"] = item.BaseUOMId;
                            dr["CostCenterId"] = item.CostCenterId;
                            dr["JWTransformationPOId"] = item.JWTransformationPODetailId;
                            dr["JobWorkOrderWiseId"] = item.JWOrderWiseId;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            //dr["UpdatedBy"] = identity.Name;
                            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            //dr["UpdatedFromIP"] = identity.IPAddress;

                            ExistOrNot.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            if (item.JWOrderWiseId.IsNotNull())
                            {
                                ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPOId='" + item.JWTransformationPODetailId + "' and JobWorkOrderWiseId='" + item.JWOrderWiseId + "' ";
                            }
                            else
                            {
                                ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPOId='" + item.JWTransformationPODetailId + "' ";
                            }

                            if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = ExistOrNot.Tables[0].NewRow();
                                dr["Id"] = GetTransformationChildPK();

                                dr["InventoryIssueId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.BaseUOMId;
                                dr["CostCenterId"] = item.CostCenterId;
                                dr["JWTransformationPOId"] = item.JWTransformationPODetailId;
                                dr["JobWorkOrderWiseId"] = item.JWOrderWiseId;

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                ExistOrNot.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                //edit
                                DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["InventoryIssueId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.BaseUOMId;
                                dr["CostCenterId"] = item.CostCenterId;
                                dr["JWTransformationPOId"] = item.JWTransformationPODetailId;
                                dr["JobWorkOrderWiseId"] = item.JWOrderWiseId;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;


                                dr.EndEdit();
                            }


                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                //         return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region InventorySalesReturn
        public void GetAvgRate(string InventorySalesId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"Select AvgRate=FORMAT(SUM(TotalBaseAmount)/SUM(Qty),'N4'),SUM(TotalBaseAmount) TotalBaseAmount,SUM(Qty)Qty FROM [TRN].[InventorySalesHistory] where InventorySalesDetailId IN (Select Id FROM [TRN].[InventorySalesDetail]  where InventorySalesId='" + InventorySalesId + "')";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void SalesReturnInsert(InventorySalesReturn inventoryIssue, IEnumerable<InventorySalesReturnDetailViewModel> entities, IEnumerable<SalesReturnTaxViewModel> salesReturnTaxList, IEnumerable<InventorySalesReturnServiceViewModel> salesServiceVMList)
        {
            var flag = false;
            bool FlagIsAsset = false;
            int currentSalesTaxId = 0;
            int inventoryReceiveTaxId = 0;
            int currentSalesServiceId = 0;
            decimal avgRate = 0;
            decimal totalReturnQty = 0;
            decimal totalGRNTax = 0;

            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                inventoryIssue.Id = GetInventorySalesReturnPK();
                AuditService.AddedLog(inventoryIssue);


                InventoryReceive inventoryReceive = new InventoryReceive
                {
                    Id = _pkGeneratorService.GetAutoNumber(nameof(InventoryReceive), PKGeneratorEnum.Yearly, null, DateTime.Now),
                    CompanyGroupId = inventoryIssue.CompanyGroupId,
                    CompanyId = inventoryIssue.CompanyId,
                    PlantId = inventoryIssue.PlantId,
                    MaterialStorageId = inventoryIssue.MaterialStorageId,
                    CurrencyId = inventoryIssue.CurrencyId,
                    //PartyId = inventoryIssue.CustomerId,
                    PartyId = null,
                    DocRefNo = inventoryIssue.DocRefNo,
                    DocDate = inventoryIssue.DocDate,
                    GateEntryNo = null,
                    EntryDate = DateTime.Now,
                    FixedAssetOrInventory = "Inventory",
                    PODepended = false,
                    AlongwithInvoice = true,
                    InvoiceNo = null,
                    InvoiceDate = null,
                    AddedBy = inventoryIssue.AddedBy,
                    AddedDate = inventoryIssue.AddedDate,
                    AddedFromIP = inventoryIssue.AddedFromIP,
                    UpdatedBy = null,
                    UpdatedDate = null,
                    UpdatedFromIP = null,
                    PaymentTermId = inventoryIssue.PaymentTermId,
                    BaseOnDueDate = inventoryIssue.BaseOnDueDate,
                    BaseNoOfDays = inventoryIssue.BaseNoOfDays,
                    MatureDate = inventoryIssue.MatureDate,
                    Status = null,
                    BaseCurrencyId = inventoryIssue.CurrencyId,
                    InvoicingPartyPlantId = inventoryIssue.InvoicingPartyPlantId,
                    DeliveryPartyPlantId = inventoryIssue.DeliveryPartyPlantId,
                    EntityId = inventoryIssue.EntityId,
                    GRNDate = DateTime.Now,
                    IsNonCreditable = false,
                    InvoicingByAddress = null,
                    DeliveryByAddress = null,
                    OpeningBalanceId = null,
                    ToCurrencyRate = inventoryIssue.ToCurrencyRate,
                    IsTaxApplicable = false,
                    PartyType = null,
                    EmployeeId = null,
                    IsApproved = false,
                    IsPaymentHold = false,
                    POId = null,
                    CheckedBy = null,
                    CheckedByStatus = null,
                    AuthorizedBy = null,
                    AuthorizedByStatus = null,
                    GRNType = "InventorySalesReturn",
                    IsNonVendor = false,
                    Reason = null,
                    ApprovedHoldRejectReason = null,
                    CheckedHoldRejectReason = null,
                    NoteForAccounts = inventoryIssue.NoteForAccounts,
                    VoucherId = null,
                    PurchaseDocumentAcceptanceId = null,
                    IsFOC = false,
                    IsInvoice = true,
                    ByWhomEmployeeId = null,
                    ToPlantId = null,
                    ToVoucherId = null,
                    TransformationContractId = null,
                    JWWIPVoucherId = null,
                    JWChangeInInvVoucherId = null,
                    JWGRIRVoucherId = null
                };
                _inventoryReveiveService.InsertGraph(inventoryReceive);

                inventoryIssue.InventoryReceiveId = inventoryReceive.Id;
                _InventorySalesReturnRepository.Insert(inventoryIssue);

                var currentId = _InventorySalesReturnDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesReturnDetail] WHERE InventorySalesReturnId='{inventoryIssue.Id}'").First();

                var receiveDetailcurrentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{inventoryReceive.Id}'").First();


                DataSet AR = null;
                GetAvgRate(inventoryIssue.InventorySalesId, out AR);
                if (AR.Tables[0].Rows.Count > 0)
                {
                    avgRate = Convert.ToDecimal(AR.Tables[0].Rows[0]["AvgRate"].ToString());
                }
                if (entities != null)
                {
                    foreach (var issue in entities.Where(r => r.TransactionQty > 0))
                    {
                        currentId++;
                        var detail = new InventorySalesReturnDetail
                        {
                            Id = MakePK(inventoryIssue.Id, currentId, 2),
                            InventorySalesReturnId = inventoryIssue.Id,
                            InventorySalesDetailId = issue.InventorySalesDetailId,
                            IsAsset = FlagIsAsset,
                            InventoryMaterialId = issue.InventoryMaterialId,
                            TransactionQty = issue.TransactionQty,
                            BaseQty = issue.TransactionQty,
                            BaseUOMId = issue.BaseUOMId,
                            TransactionUoMId = issue.TransactionUoMId,
                            AvgRate = issue.AvgRate,
                            AvgAmount = issue.TransactionQty * issue.AvgRate,

                            BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                            ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                            Comments = issue.Comments,
                            CostCenterId = issue.CostCenterId,
                            SalesRate = issue.SalesRate,
                            TotalSalesAmount = Math.Round((issue.TransactionQty * issue.SalesRate), 2),
                            BooksCurrencyTransactionAmount = Math.Round((inventoryIssue.ToCurrencyRate * Math.Round((issue.TransactionQty * issue.SalesRate), 2)), 2),
                            ModelState = ModelState.Added,
                            AddedBy = inventoryIssue.AddedBy,
                            AddedDate = inventoryIssue.AddedDate,
                            AddedFromIP = inventoryIssue.AddedFromIP,
                        };
                        totalReturnQty = detail.TransactionQty;
                        _InventorySalesReturnDetailRepository.Insert(detail);


                        var inventoryMaterial = _inventoryMaterialService.Find(issue.InventoryMaterialId);
                        if (inventoryMaterial != null)
                        {
                            inventoryMaterial.TotalQty += issue.TransactionQty;
                            inventoryMaterial.ModelState = ModelState.Modified;
                            _inventoryMaterialService.UpdateGraph(inventoryMaterial);
                        }



                        //InventoryReceiveDetail & Tax
                        receiveDetailcurrentId++;
                        InventoryReceiveDetail receiveDetail = new InventoryReceiveDetail
                        {
                            Id = inventoryReceive.Id + "-" + receiveDetailcurrentId,
                            InventoryReceiveId = inventoryReceive.Id,
                            InventoryMaterialId = detail.InventoryMaterialId,
                            MaterialStorageId = inventoryIssue.MaterialStorageId,
                            TransactionQty = totalReturnQty,
                            TransactionUoMId = detail.TransactionUoMId,
                            BaseQty = totalReturnQty,
                            BaseUOMId = entities.Where(r => r.MaterialMasterId == inventoryMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                            BaseUoMFactor = Convert.ToDecimal(entities.Where(r => r.MaterialMasterId == inventoryMaterial.MaterialMasterId).Select(t => t.BaseUoMFactor).FirstOrDefault()),
                            MaterialTranRate = avgRate,
                            MaterialTranAmount = avgRate * totalReturnQty,
                            IssueQty = null,
                            AddedDate = inventoryReceive.AddedDate,
                            AddedBy = inventoryReceive.AddedBy,
                            AddedFromIP = inventoryReceive.AddedFromIP,
                            UpdatedBy = inventoryReceive.UpdatedBy,
                            UpdatedDate = inventoryReceive.UpdatedDate,
                            UpdatedFromIP = inventoryReceive.UpdatedFromIP,

                            //  TotalTaxAmount = 0,

                            TotalMaterialTranAmount = avgRate * totalReturnQty,
                            TotalMaterialBooksCurrencyAmount = avgRate * totalReturnQty * inventoryIssue.ToCurrencyRate,
                            ChargesTranAmount = 0,
                            ChargesTaxTranAmount = 0,
                            TrnCurrencyBaseRate = 0,
                            BooksCurrencyBaseRate = 0,
                            CountryId = null,
                            BaseIssueQty = 0,
                            ShortageQty = 0,
                            RejectionQty = 0,
                            ApprovedQty = 0,
                            ShortageRatePercent = 0,
                            ShortageValue = 0,
                            RejectRatePercent = 0,
                            RejectValue = 0,
                            RejectClamPercent = 0,
                            Description = null,
                            ShortRejFlag = false,
                            PostDrGLGeneralInfoId = null,
                            PostDrBudgetMasterId = null,
                            PostDrActivityId = null,
                            CapitalizeVoucherDetailId = null,
                            IsAsset = false,
                            PurchaseDocumentAcceptanceId = null,
                            PurchaseDocumentAcceptanceDetailId = null,
                            PurchaseReturnQty = 0,
                            IssueReturnQty = 0,
                            MaterialMasterOpeningBalanceDetailId = null,
                            ReductionByAdjustmentQty = null,
                            InventorySalesQty = 0,
                            InventoryScrapQty = 0,
                            LotNumber = null,
                            Diameter = null,
                            Type = null,
                            InventoryTransferQty = 0,
                            TransferedFromGrnId = null,
                            GRNQty = 0,
                            GRNTotalAmount = 0,
                            QualityStatus = null,
                            GrossAmount = avgRate * totalReturnQty,
                            DiscountAmount = 0,
                            MasterOrderItemId = null,
                            OSTransformationPOId = null,
                            OSTransformationPODetailId = null,
                            OSTransformationPOInputMaterialId = null,
                            OSTransformationPOByProductId = null,
                            MaterialFor = null
                        };

                        if (salesReturnTaxList != null)
                        {
                            var salesTaxList = salesReturnTaxList.Where(r => r.InventorySalesDetailId == issue.InventorySalesDetailId).ToList();
                            if (salesTaxList != null)
                            {
                                foreach (var taxVM in salesTaxList)
                                {
                                    if (taxVM.TaxCategoryId == null)
                                        throw new CustomException("Please Select Tax Category !");

                                    currentSalesTaxId++;
                                    var salesTax = new InventorySalesReturnTax
                                    {
                                        Id = _pkGeneratorService.MakePK(detail.Id, currentSalesTaxId, 2),
                                        AddedBy = detail.AddedBy,
                                        AddedDate = detail.AddedDate,
                                        AddedFromIP = detail.AddedFromIP,
                                        TaxAmount = taxVM.TaxAmount,
                                        BooksCurrencyTaxAmount = Math.Round(taxVM.TaxAmount * inventoryIssue.ToCurrencyRate, 2),
                                        HSNCodeId = taxVM.HSNCodeId,
                                        Percentage = taxVM.Percentage,
                                        InventorySalesReturnId = inventoryIssue.Id,
                                        InventorySalesReturnDetailId = detail.Id,
                                        InventorySalesTaxId = taxVM.InventorySalesTaxId,
                                        //SalesMaterialId = detail.Id,
                                        TaxCategoryId = taxVM.TaxCategoryId,
                                        InventorySalesReturnServiceId = null,
                                        ModelState = ModelState.Added,
                                        UpdatedBy = null,
                                        UpdatedDate = null,
                                        UpdatedFromIP = null
                                    };
                                    _InventorySalesReturnTaxRepository.Insert(salesTax);


                                    inventoryReceiveTaxId++;
                                    var inventoryReceiveTax = new InventoryReceiveTax
                                    {
                                        Id = _pkGeneratorService.MakePK(receiveDetail.Id, inventoryReceiveTaxId, 2),
                                        AddedBy = detail.AddedBy,
                                        AddedDate = detail.AddedDate,
                                        AddedFromIP = detail.AddedFromIP,
                                        TaxAmount = taxVM.TaxAmount,

                                        HSNCodeId = taxVM.HSNCodeId,
                                        Percentage = taxVM.Percentage,
                                        InventoryReceiveDetailId = receiveDetail.Id,
                                        TaxCategoryId = taxVM.TaxCategoryId,
                                        InventoryReceiveId = inventoryReceive.Id,
                                        ModelState = ModelState.Added,
                                        UpdatedBy = null,
                                        UpdatedDate = null,
                                        UpdatedFromIP = null
                                    };
                                    totalGRNTax += inventoryReceiveTax.TaxAmount;
                                    _receiveTaxRepository.Insert(inventoryReceiveTax);
                                }
                            }
                        }

                        receiveDetail.TotalTaxAmount = totalGRNTax;

                        _receiveDetailRepository.Insert(receiveDetail);
                        detail.InventoryReceiveDetailId = receiveDetail.Id;
                        detail.InventoryReceiveId = inventoryReceive.Id;
                    }
                }


                if (salesServiceVMList != null)
                {
                    foreach (var salesServiceVM in salesServiceVMList.Where(r => r.Amount > 0))
                    {

                        currentSalesServiceId++;
                        var salesService = new InventorySalesReturnService
                        {
                            AddedBy = inventoryIssue.AddedBy,
                            AddedDate = inventoryIssue.AddedDate,
                            AddedFromIP = inventoryIssue.AddedFromIP,
                            Amount = salesServiceVM.Amount,
                            BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * inventoryIssue.ToCurrencyRate, 2),
                            Id = _pkGeneratorService.MakePK(inventoryIssue.Id, currentSalesServiceId, 2),
                            ModelState = ModelState.Added,
                            InventorySalesReturnId = inventoryIssue.Id,
                            InventorySalesServiceId = salesServiceVM.InventorySalesServiceId,
                            ServiceMasterId = salesServiceVM.ServiceMasterId,
                            TotalTaxAmount = salesServiceVM.TotalTaxAmount,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null
                        };
                        _InventorySalesReturnServiceRepository.Insert(salesService);

                        if (salesServiceVM.ChargeTaxList != null && salesServiceVM.ChargeTaxList.Count > 0)
                        {
                            foreach (var taxVM in salesServiceVM.ChargeTaxList)
                            {
                                if (taxVM.TaxCategoryId == null)
                                    throw new CustomException("Please Select Tax Category !");

                                currentSalesTaxId++;
                                var salesTax = new InventorySalesReturnTax
                                {
                                    Id = _pkGeneratorService.MakePK(salesService.Id, currentSalesTaxId, 2),
                                    AddedBy = salesService.AddedBy,
                                    AddedDate = salesService.AddedDate,
                                    AddedFromIP = salesService.AddedFromIP,
                                    TaxAmount = taxVM.TaxAmount,
                                    BooksCurrencyTaxAmount = Math.Round(taxVM.TaxAmount * inventoryIssue.ToCurrencyRate, 2),
                                    HSNCodeId = taxVM.HSNCodeId,
                                    Percentage = taxVM.Percentage,
                                    InventorySalesReturnId = inventoryIssue.Id,
                                    InventorySalesReturnServiceId = salesService.Id,
                                    InventorySalesTaxId = taxVM.InventorySalesTaxId,
                                    TaxCategoryId = taxVM.TaxCategoryId,
                                    ModelState = ModelState.Added,
                                    UpdatedBy = null,
                                    UpdatedDate = null,
                                    UpdatedFromIP = null
                                };
                                _InventorySalesReturnTaxRepository.Insert(salesTax);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }

        }

        public void SalesReturnUpdate(InventorySalesReturn inventoryIssue, IEnumerable<InventorySalesReturnDetailViewModel> entities, IEnumerable<SalesReturnTaxViewModel> salesReturnTaxList, IEnumerable<InventorySalesReturnServiceViewModel> salesServiceVMList)
        {
            var flag = false;
            bool FlagIsAsset = false;
            int currentSalesTaxId = 0;
            int inventoryReceiveTaxId = 0;
            int currentSalesServiceId = 0;
            decimal avgRate = 0;
            decimal totalGRNTax = 0;

            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                var inventorySaleReturn = _InventorySalesReturnRepository.Find(inventoryIssue.Id);
                AuditService.UpdatedLog(inventorySaleReturn);
                _InventorySalesReturnRepository.Update(inventorySaleReturn);


                var inventoryreceive = _inventoryReveiveService.Find(inventoryIssue.InventoryReceiveId);
                //AuditService.UpdatedLog(inventoryreceive);
                //_inventoryReveiveService.Update(inventoryreceive);

                var currentId = _InventorySalesReturnDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesReturnDetail] WHERE InventorySalesReturnId='{inventoryIssue.Id}'").First();
                var receiveDetailcurrentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{inventoryreceive.Id}'").First();


                DataSet AR = null;
                GetAvgRate(inventoryIssue.InventorySalesId, out AR);
                if (AR.Tables[0].Rows.Count > 0)
                {
                    avgRate = Convert.ToDecimal(AR.Tables[0].Rows[0]["AvgRate"].ToString());
                }
                if (entities != null)
                {
                    foreach (var issue in entities.Where(r => r.TransactionQty > 0))
                    {

                        currentId++;

                        var detail = new InventorySalesReturnDetail
                        {
                            InventorySalesReturnId = inventoryIssue.Id,
                            InventorySalesDetailId = issue.InventorySalesDetailId,
                            IsAsset = FlagIsAsset,
                            InventoryMaterialId = issue.InventoryMaterialId,
                            TransactionQty = issue.TransactionQty,
                            BaseQty = issue.TransactionQty,
                            BaseUOMId = issue.BaseUOMId,
                            TransactionUoMId = issue.TransactionUoMId,
                            AvgRate = issue.AvgRate,
                            AvgAmount = issue.TransactionQty * issue.AvgRate,
                            BudgetMasterId = issue.BudgetMasterId,
                            ActivityId = issue.ActivityId,
                            Comments = issue.Comments,
                            CostCenterId = issue.CostCenterId,
                            SalesRate = issue.SalesRate,
                            TotalSalesAmount = Math.Round((issue.TransactionQty * issue.SalesRate), 2),
                            BooksCurrencyTransactionAmount = Math.Round((inventoryIssue.ToCurrencyRate * Math.Round((issue.TransactionQty * issue.SalesRate), 2)), 2),
                            ModelState = ModelState.Added,
                            AddedBy = inventoryIssue.AddedBy,
                            AddedDate = inventoryIssue.AddedDate,
                            AddedFromIP = inventoryIssue.AddedFromIP,
                        };
                        var inventoryMaterial = _inventoryMaterialService.Find(issue.InventoryMaterialId);
                        if (issue.Id != null)
                        {
                            if (inventoryMaterial != null)
                            {
                                inventoryMaterial.TotalQty += issue.TransactionQty - issue.TempReturnQty;
                                inventoryMaterial.ModelState = ModelState.Modified;
                                _inventoryMaterialService.UpdateGraph(inventoryMaterial);
                            }
                            detail.Id = issue.Id;
                            _InventorySalesReturnDetailRepository.Update(detail);
                            var invdetail = _receiveDetailRepository.Find(issue.InventoryReceiveDetailId);
                            invdetail.TransactionQty = issue.TransactionQty;
                            invdetail.UpdatedBy = inventoryreceive.UpdatedBy;
                            invdetail.UpdatedDate = inventoryreceive.UpdatedDate;
                            invdetail.UpdatedFromIP = inventoryreceive.UpdatedFromIP;
                            invdetail.GrossAmount = avgRate * issue.TransactionQty;
                            invdetail.TransactionQty = issue.TransactionQty;
                            invdetail.BaseQty = issue.TransactionQty;
                            invdetail.MaterialTranRate = avgRate;
                            invdetail.MaterialTranAmount = avgRate * issue.TransactionQty;
                            invdetail.TotalMaterialTranAmount = avgRate * issue.TransactionQty;
                            invdetail.TotalMaterialBooksCurrencyAmount = avgRate * issue.TransactionQty * inventoryIssue.ToCurrencyRate;


                            if (salesReturnTaxList != null)
                            {
                                totalGRNTax = 0;
                                var salesTaxList = salesReturnTaxList.Where(r => r.InventorySalesDetailId == issue.InventorySalesDetailId).ToList();
                                var invenReceiveTax = _receiveTaxRepository.Query(r => r.InventoryReceiveDetailId == issue.InventoryReceiveDetailId).Select().ToList();
                                if (salesTaxList != null)
                                {
                                    foreach (var taxVM in salesTaxList)
                                    {
                                        if (taxVM.TaxCategoryId == null)
                                            throw new CustomException("Please Select Tax Category !");

                                        var salesTax = new InventorySalesReturnTax
                                        {
                                            Id = taxVM.Id,
                                            TaxAmount = taxVM.TaxAmount,
                                            BooksCurrencyTaxAmount = Math.Round(taxVM.TaxAmount * inventoryIssue.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            InventorySalesReturnId = inventoryIssue.Id,
                                            InventorySalesReturnDetailId = detail.Id,
                                            InventorySalesTaxId = taxVM.InventorySalesTaxId,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            InventorySalesReturnServiceId = null,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = detail.UpdatedBy,
                                            UpdatedDate = detail.UpdatedDate,
                                            UpdatedFromIP = detail.UpdatedFromIP
                                        };
                                        _InventorySalesReturnTaxRepository.Update(salesTax);
                                        foreach (var intrecTax in invenReceiveTax.Where(r => r.TaxCategoryId == salesTax.TaxCategoryId))
                                        {
                                            intrecTax.TaxAmount = salesTaxList.Where(r => r.TaxCategoryId == intrecTax.TaxCategoryId).Select(s => s.TaxAmount).FirstOrDefault();
                                            totalGRNTax += intrecTax.TaxAmount;
                                            _receiveTaxRepository.Update(intrecTax);

                                        }
                                    }

                                }
                            }
                            invdetail.TotalTaxAmount = totalGRNTax;
                            _receiveDetailRepository.Update(invdetail);
                        }
                        else
                        {
                            detail.Id = MakePK(inventoryIssue.Id, currentId, 2);
                            _InventorySalesReturnDetailRepository.Insert(detail);
                            if (inventoryMaterial != null)
                            {
                                inventoryMaterial.TotalQty += issue.TransactionQty;
                                inventoryMaterial.ModelState = ModelState.Modified;
                                _inventoryMaterialService.UpdateGraph(inventoryMaterial);
                            }

                            receiveDetailcurrentId++;
                            InventoryReceiveDetail receiveDetail = new InventoryReceiveDetail
                            {
                                Id = inventoryreceive.Id + "-" + receiveDetailcurrentId,
                                InventoryReceiveId = inventoryreceive.Id,
                                InventoryMaterialId = detail.InventoryMaterialId,
                                MaterialStorageId = inventoryIssue.MaterialStorageId,
                                TransactionQty = issue.TransactionQty,
                                TransactionUoMId = detail.TransactionUoMId,
                                BaseQty = issue.TransactionQty,
                                BaseUOMId = entities.Where(r => r.MaterialMasterId == inventoryMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                                BaseUoMFactor = Convert.ToDecimal(entities.Where(r => r.MaterialMasterId == inventoryMaterial.MaterialMasterId).Select(t => t.BaseUoMFactor).FirstOrDefault()),
                                MaterialTranRate = avgRate,
                                MaterialTranAmount = avgRate * issue.TransactionQty,
                                IssueQty = null,
                                AddedDate = inventoryreceive.AddedDate,
                                AddedBy = inventoryreceive.AddedBy,
                                AddedFromIP = inventoryreceive.AddedFromIP,
                                UpdatedBy = inventoryreceive.UpdatedBy,
                                UpdatedDate = inventoryreceive.UpdatedDate,
                                UpdatedFromIP = inventoryreceive.UpdatedFromIP,
                                TotalMaterialTranAmount = avgRate * issue.TransactionQty,
                                TotalMaterialBooksCurrencyAmount = avgRate * issue.TransactionQty * inventoryIssue.ToCurrencyRate,
                                ChargesTranAmount = 0,
                                ChargesTaxTranAmount = 0,
                                TrnCurrencyBaseRate = 0,
                                BooksCurrencyBaseRate = 0,
                                CountryId = null,
                                BaseIssueQty = 0,
                                ShortageQty = 0,
                                RejectionQty = 0,
                                ApprovedQty = 0,
                                ShortageRatePercent = 0,
                                ShortageValue = 0,
                                RejectRatePercent = 0,
                                RejectValue = 0,
                                RejectClamPercent = 0,
                                Description = null,
                                ShortRejFlag = false,
                                PostDrGLGeneralInfoId = null,
                                PostDrBudgetMasterId = null,
                                PostDrActivityId = null,
                                CapitalizeVoucherDetailId = null,
                                IsAsset = false,
                                PurchaseDocumentAcceptanceId = null,
                                PurchaseDocumentAcceptanceDetailId = null,
                                PurchaseReturnQty = 0,
                                IssueReturnQty = 0,
                                MaterialMasterOpeningBalanceDetailId = null,
                                ReductionByAdjustmentQty = null,
                                InventorySalesQty = 0,
                                InventoryScrapQty = 0,
                                LotNumber = null,
                                Diameter = null,
                                Type = null,
                                InventoryTransferQty = 0,
                                TransferedFromGrnId = null,
                                GRNQty = 0,
                                GRNTotalAmount = 0,
                                QualityStatus = null,
                                GrossAmount = avgRate * issue.TransactionQty,
                                DiscountAmount = 0,
                                MasterOrderItemId = null,
                                OSTransformationPOId = null,
                                OSTransformationPODetailId = null,
                                OSTransformationPOInputMaterialId = null,
                                OSTransformationPOByProductId = null,
                                MaterialFor = null
                            };
                            if (salesReturnTaxList != null)
                            {
                                var salesTaxList = salesReturnTaxList.Where(r => r.InventorySalesDetailId == issue.InventorySalesDetailId).ToList();
                                if (salesTaxList != null)
                                {
                                    foreach (var taxVM in salesTaxList)
                                    {
                                        if (taxVM.TaxCategoryId == null)
                                            throw new CustomException("Please Select Tax Category !");

                                        currentSalesTaxId++;
                                        var salesTax = new InventorySalesReturnTax
                                        {
                                            Id = _pkGeneratorService.MakePK(detail.Id, currentSalesTaxId, 2),
                                            AddedBy = detail.AddedBy,
                                            AddedDate = detail.AddedDate,
                                            AddedFromIP = detail.AddedFromIP,
                                            TaxAmount = taxVM.TaxAmount,
                                            BooksCurrencyTaxAmount = Math.Round(taxVM.TaxAmount * inventoryIssue.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            InventorySalesReturnId = inventoryIssue.Id,
                                            InventorySalesReturnDetailId = detail.Id,
                                            InventorySalesTaxId = taxVM.InventorySalesTaxId,
                                            //SalesMaterialId = detail.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            InventorySalesReturnServiceId = null,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _InventorySalesReturnTaxRepository.Insert(salesTax);


                                        inventoryReceiveTaxId++;
                                        var inventoryReceiveTax = new InventoryReceiveTax
                                        {
                                            Id = _pkGeneratorService.MakePK(receiveDetail.Id, inventoryReceiveTaxId, 2),
                                            AddedBy = detail.AddedBy,
                                            AddedDate = detail.AddedDate,
                                            AddedFromIP = detail.AddedFromIP,
                                            TaxAmount = taxVM.TaxAmount,

                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            InventoryReceiveDetailId = receiveDetail.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            InventoryReceiveId = inventoryreceive.Id,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        totalGRNTax += inventoryReceiveTax.TaxAmount;
                                        _receiveTaxRepository.Insert(inventoryReceiveTax);
                                    }
                                }
                            }

                            receiveDetail.TotalTaxAmount = totalGRNTax;

                            _receiveDetailRepository.Insert(receiveDetail);
                            detail.InventoryReceiveDetailId = receiveDetail.Id;
                            detail.InventoryReceiveId = inventoryreceive.Id;
                        }


                    }
                }

                if (salesServiceVMList != null)
                {
                    foreach (var salesServiceVM in salesServiceVMList.Where(r => r.Amount > 0))
                    {
                        if (salesServiceVM.Id != null)
                        {
                            var salesService = new InventorySalesReturnService
                            {
                                Id = salesServiceVM.Id,
                                Amount = salesServiceVM.Amount,
                                BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * inventoryIssue.ToCurrencyRate, 2),
                                ModelState = ModelState.Modified,
                                InventorySalesReturnId = inventoryIssue.Id,
                                InventorySalesServiceId = salesServiceVM.InventorySalesServiceId,
                                ServiceMasterId = salesServiceVM.ServiceMasterId,
                                TotalTaxAmount = salesServiceVM.TotalTaxAmount,
                                UpdatedBy = inventorySaleReturn.UpdatedBy,
                                UpdatedDate = inventorySaleReturn.UpdatedDate,
                                UpdatedFromIP = inventorySaleReturn.UpdatedFromIP
                            };
                            _InventorySalesReturnServiceRepository.Update(salesService);

                            if (salesServiceVM.ChargeTaxList != null && salesServiceVM.ChargeTaxList.Count > 0)
                            {
                                foreach (var taxVM in salesServiceVM.ChargeTaxList)
                                {
                                    if (taxVM.TaxCategoryId == null)
                                        throw new CustomException("Please Select Tax Category !");
                                    if (taxVM.Id != null)
                                    {
                                        var salesTax = new InventorySalesReturnTax
                                        {
                                            Id = taxVM.Id,
                                            TaxAmount = taxVM.TaxAmount,
                                            BooksCurrencyTaxAmount = Math.Round(taxVM.TaxAmount * inventoryIssue.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            InventorySalesReturnId = inventoryIssue.Id,
                                            InventorySalesReturnServiceId = salesService.Id,
                                            InventorySalesTaxId = taxVM.InventorySalesTaxId,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = salesService.UpdatedBy,
                                            UpdatedDate = salesService.UpdatedDate,
                                            UpdatedFromIP = salesService.UpdatedFromIP
                                        };
                                        _InventorySalesReturnTaxRepository.Update(salesTax);
                                    }
                                    else
                                    {
                                        currentSalesTaxId++;
                                        var salesTax = new InventorySalesReturnTax
                                        {
                                            Id = _pkGeneratorService.MakePK(salesService.Id, currentSalesTaxId, 2),
                                            AddedBy = salesService.AddedBy,
                                            AddedDate = salesService.AddedDate,
                                            AddedFromIP = salesService.AddedFromIP,
                                            TaxAmount = taxVM.TaxAmount,
                                            BooksCurrencyTaxAmount = Math.Round(taxVM.TaxAmount * inventoryIssue.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            InventorySalesReturnId = inventoryIssue.Id,
                                            InventorySalesReturnServiceId = salesService.Id,
                                            InventorySalesTaxId = taxVM.InventorySalesTaxId,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _InventorySalesReturnTaxRepository.Insert(salesTax);
                                    }

                                }
                            }

                        }
                        else
                        {
                            currentSalesServiceId++;
                            var salesService = new InventorySalesReturnService
                            {
                                AddedBy = inventoryIssue.AddedBy,
                                AddedDate = inventoryIssue.AddedDate,
                                AddedFromIP = inventoryIssue.AddedFromIP,
                                Amount = salesServiceVM.Amount,
                                BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * inventoryIssue.ToCurrencyRate, 2),
                                Id = _pkGeneratorService.MakePK(inventoryIssue.Id, currentSalesServiceId, 2),
                                ModelState = ModelState.Added,
                                InventorySalesReturnId = inventoryIssue.Id,
                                InventorySalesServiceId = salesServiceVM.InventorySalesServiceId,
                                ServiceMasterId = salesServiceVM.ServiceMasterId,
                                TotalTaxAmount = salesServiceVM.TotalTaxAmount,
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null
                            };
                            _InventorySalesReturnServiceRepository.Insert(salesService);

                            if (salesServiceVM.ChargeTaxList != null && salesServiceVM.ChargeTaxList.Count > 0)
                            {
                                foreach (var taxVM in salesServiceVM.ChargeTaxList)
                                {
                                    if (taxVM.TaxCategoryId == null)
                                        throw new CustomException("Please Select Tax Category !");

                                    currentSalesTaxId++;
                                    var salesTax = new InventorySalesReturnTax
                                    {
                                        Id = _pkGeneratorService.MakePK(salesService.Id, currentSalesTaxId, 2),
                                        AddedBy = salesService.AddedBy,
                                        AddedDate = salesService.AddedDate,
                                        AddedFromIP = salesService.AddedFromIP,
                                        TaxAmount = taxVM.TaxAmount,
                                        BooksCurrencyTaxAmount = Math.Round(taxVM.TaxAmount * inventoryIssue.ToCurrencyRate, 2),
                                        HSNCodeId = taxVM.HSNCodeId,
                                        Percentage = taxVM.Percentage,
                                        InventorySalesReturnId = inventoryIssue.Id,
                                        InventorySalesReturnServiceId = salesService.Id,
                                        InventorySalesTaxId = taxVM.InventorySalesTaxId,
                                        TaxCategoryId = taxVM.TaxCategoryId,
                                        ModelState = ModelState.Added,
                                        UpdatedBy = null,
                                        UpdatedDate = null,
                                        UpdatedFromIP = null
                                    };
                                    _InventorySalesReturnTaxRepository.Insert(salesTax);
                                }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }

        }


        #endregion

    }
}
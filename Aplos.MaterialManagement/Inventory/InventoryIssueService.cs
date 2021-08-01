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
		private readonly IRepositoryAsync<MaterialStorage> _materialStorageRepository;

		private readonly IRepositoryAsync<IssueDetailAndIssueRequestMap> _IssueDetailAndIssueRequestMapRepository;
		
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
			, IRepositoryAsync<MaterialStorage> materialStorageRepository
			, IRepositoryAsync<IssueDetailAndIssueRequestMap> IssueDetailAndIssueRequestMapRepository
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
			_materialStorageRepository = materialStorageRepository;
			_IssueDetailAndIssueRequestMapRepository = IssueDetailAndIssueRequestMapRepository;
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
		private string GetIssueDetailAndIssueRequestMapPK() 
		{
			return base.GetAutoNumber(nameof(IssueDetailAndIssueRequestMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
		}
		public void InsertGraph(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus)
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
												temp = (detail.TransactionQty-(receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
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
								var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
								var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
								var totalReqQty = 0M;
								decimal policyAmmount = 0;

								decimal detailtrnAmount = 0;
								decimal totalGRNQty = 0;
								/*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* TrnRate)*/
								/*Rate= Amount/Sum GRN Qty */
								var IssueRequestDetailIdnew = "";
								foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
								{
									//decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
									//decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(IIH.TotalAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(ISH.TotalBaseAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
									//																						   FROM trn.InventoryReceiveDetail IRD  
									//																							left JOIN [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
									//																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
									//																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
									//																							LEFT join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
									//																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
									//																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
									//																							WHERE  IRD.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
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

									TransactionQty = stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
									PolicyAmount = Math.Round(detailtrnAmount, 2),
									PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
									BaseQty = totalGRNQty,
									AvgAmount = Math.Round((totalGRNQty * invMaterial.AvgRate), 2),
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

									if (item.TransactionUoMId != item.BaseUOMId)
										totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
									else
										totalReqQty = item.RequisitionQty;
									historyId++;
									var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
									var history = new InventoryIssueHistory
									{
										Id = MakePK(issueDetail.Id, historyId, 2),
										InventoryIssueDetailId = issueDetail.Id,
										InventoryReceiveDetailId = item.InventoryReceiveDetailId,
										Qty = item.RequisitionQty,
										//Rate = Convert.ToDecimal(item.BaseRate),
										Rate = Math.Round((SelectedGRN.TotalAmount / item.RequisitionQty), 4),
										TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
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



									//Mapping Data=========================================================
									var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + item.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
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
												if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > issueDetail.TransactionQty)
												{

													issueDetail.TransactionQty = issueDetail.TransactionQty;
													//temp += itemDetail.TransactionQty;
													isQtyAlocated = false;

												}
												else if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < issueDetail.TransactionQty)
												{
													//temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
													temp = receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty;
													issueDetail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
													isQtyAlocated = true;

												}
												else
												{
													//temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
													issueDetail.TransactionQty = issueDetail.TransactionQty;
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
														issueDetail.TransactionQty = issueDetail.TransactionQty;
														isQtyAlocated = false;
													}
													if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < temp)
													{
														//temp = temp - issue.TransactionQtyForPO;
														temp = (temp - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
														//itemDetail.TransactionQty = issue.TransactionQtyForPO;
														issueDetail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
														isQtyAlocated = true;
													}
													else
													{
														//temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
														issueDetail.TransactionQty = temp;
														isQtyAlocated = true;

													}

												}
												else
												{
													issueDetail.TransactionQty = 0;
												}
											}


											var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
											{
												Id = GetIssueDetailAndIssueRequestMapPK(),
												InventoryIssueDetailId = issueDetail.Id,
												IssueRequestBOQMapId = receiveDetailListNew.Id,
												Qty = issueDetail.TransactionQty
												//AutoAllocate = true

											};
											AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
											_IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
										}
									}


								}


								builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
								rdBuilder.Append(builderSql);

								AuditService.AddedLog(issueDetail);
								_issueDetailService.InsertGraph(issueDetail);

								
								//===================

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

							};
							AuditService.AddedLog(history);
							_InventoryIssueReturnHistoryRepository.Insert(history);
							var invMaterial = _InventoryIssueReturnHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + issue.InventoryMaterialId + "'").FirstOrDefault();
							var invMaterial12 = _InventoryIssueReturnHistoryRepository.SqlQuery<InventoryIssueHistory>(@"SELECT * FROM [TRN].[InventoryIssueHistory] WHERE Id='" + issue.InventoryIssueHistoryId + "'").FirstOrDefault();
							var invMaterial1 = _InventoryIssueReturnRepository.SqlQuery<InventoryReceiveDetail>(@"SELECT * FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + issue.InventoryReceiveDetailId + "'").FirstOrDefault();

							builderSql = @"UPDATE trn.InventoryIssueHistory SET IssueReturnQty='" + Convert.ToDecimal(Convert.ToDecimal(invMaterial12.IssueReturnQty + issue.TransactionQty)) + "' WHERE Id='" + issue.InventoryIssueHistoryId + "'";
							rdBuilder.Append(builderSql);

							builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueReturnQty='" + Convert.ToDecimal(Convert.ToDecimal(invMaterial1.IssueReturnQty + issue.TransactionQty)) + "' WHERE Id='" + issue.InventoryReceiveDetailId + "'";
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

		public void DeleteIssueDetail(string issueDetailId)
		{
			var flag = false;
			try
			{
				_unitOfWork.BeginTransaction();
				flag = true;
				var builder = new System.Text.StringBuilder();
				var sql = "";
				sql = @"UPDATE A SET A.TotalQty=A.TotalQty+B.TransactionQty FROM [TRN].[InventoryMaterial] AS A JOIN [TRN].[InventoryIssueDetail] AS B ON B.InventoryMaterialId=A.Id WHERE B.Id='" + issueDetailId + "'";
				builder.Append(sql);
				sql = @"UPDATE A SET A.IssueQty=A.IssueQty-B.Qty FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryIssueHistory] AS B ON B.InventoryReceiveDetailId=A.Id WHERE B.InventoryIssueDetailId='" + issueDetailId + "'";
				builder.Append(sql);
				sql = @"DELETE [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='" + issueDetailId + "'";
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



		public IEnumerable<object> GetDataByInventoryIssue(string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				//var sql = @"SELECT E.UserName AS Entity ,isnull(II.IssueType,'') issuetype, II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//                              ,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
				//					 ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount,II.Remarks,II.Id AS IssueId,II.OrderRefNo
				//                                FROM[TRN].[InventoryIssue]
				//    AS II
				//                            JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId= II.Id AND IID.IsAsset= 0
				//                            JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id

				//                            left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId

				//                            Left JOIN [ORG].[Entity] E On E.id= II.EntityId
				//                            WHERE II.PlantId= '" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' 
				//                            GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//                              ,II.IssueDate, MS.UserName
				//					 ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  Order BY II.Id DESC";

				var sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,SUM(IIH.qty) Qty
							,SUM(Round(IIH.qty*IIH.Rate,2)) Amount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							,C.Id CountryId,c.UserName CountryName,II.ContractId,II.ProductionOrderId,Con.ContractNo
							FROM[TRN].[InventoryIssue] AS II
							left JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join (Select InventoryIssueDetailId,IssueRequestDetailId,qty, Rate from trn.InventoryIssueHistory ) IIH ON IIH.InventoryIssueDetailId=IID.Id
							left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
							left JOIN SCS.Country c ON C.Id=IR.CountryId
							left join dbo.Contract Con On Con.Id=II.ContractId
						WHERE II.PlantId= '" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' AND IID.IsAsset= 0
						GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
						,II.IssueDate, MS.UserName
						,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
						,C.Id ,c.UserName ,II.ContractId ,II.ProductionOrderId,Con.ContractNo
						Order BY II.IssueDate DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		public IEnumerable<object> GetDataByInventoryReturnIssue(string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"SELECT E.UserName AS Entity ,isnull(II.IssueType,'') issuetype, II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 --,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
									 ,SUM(IID.Qty) Qty,
									 --SUM(IID.PolicyAmount) Amount
									 II.Remarks,II.Id AS IssueId,II.OrderRefNo
                                    FROM[TRN].[InventoryIssueReturn] AS II
                                left JOIN [TRN].InventoryIssueReturnHistory AS IID ON IID.InventoryIssueReturnId= II.Id -- AND IID.IsAsset= 0
                                left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                                left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                                Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                                WHERE II.PlantId= '" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  Order BY II.Id DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}





		public GridModel GetIssueList(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText = @"SELECT II.Id,II.Id IssueNo, II.IssueDate,II.Remarks, MS.UserName AS MaterialStorage,II.EntityId,E.UserName  EntityName,II.IssueType
                                    ,EI.EmployeeCode+' - '+EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount
                                    ,ii.OrderRefNo, IsOrderSpecificy=  CASE WHEN ii.OrderRefNo <> '' THEN 1 ELSE 0 END,II.[Types]
                                    FROM [TRN].[InventoryIssue] AS II
                                    JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId=MS.Id 
							        JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId=II.Id
								    left join dbo.EmployeeInformation AS EI ON EI.SystemId=II.EmployeeId
                                    left join org.Entity E ON E.Id=II.EntityId
                            WHERE II.PlantId='" + plantId + @"' AND ISNULL(II.[Status],'')<>'Posting' 
                            AND IID.IsAsset=0
                            GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 , II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.Remarks,II.EntityId,E.UserName,II.IssueType, ii.OrderRefNo,II.[Types]";
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


		public IEnumerable<object> GetIssueRegister(string fromDate, string toDate, string Type)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (Type == "Posted")
				{
					sql = @"SELECT II.Id AS IssueId
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
	                        ,IID.TransactionQty
	                        --,IID.BaseUOMId
	                        ,TUoM.UserName AS UOM
	                        ,Round(IID.AvgRate,2) AvgRate
	                        ,Round(IID.AvgAmount,2) AvgAmount
	                        ,Round(IID.PolicyRate,2) PolicyRate
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
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
                         --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                       -- LEFT JOIN trn.Invoice AS I ON I.InventoryReceiveId = II.Id
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
                    where v.VoucherNo is not null ANd II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";

				}
				else
				{
					sql = @"SELECT II.Id AS IssueId
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
	                        ,IID.TransactionQty
	                        --,IID.BaseUOMId
	                        ,TUoM.UserName AS UOM
	                        ,Round(IID.AvgRate,2) AvgRate
	                        ,Round(IID.AvgAmount,2) AvgAmount
	                        ,Round(IID.PolicyRate,2) PolicyRate
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
                            ,CC.UserName CostCenterName
                            ,EI.EmployeeName
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
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
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
                    where v.VoucherNo is null ANd II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";

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
		public IEnumerable<object> GetIssueRegisterBYGRN(string fromDate, string toDate, string Type)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (Type == "Posted")
				{
					sql = @"SELECT II.Id AS IssueId
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
							,TUoM1.UserName AS GRNUOM
							,IRD.MaterialTranRate GRNRate
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


                    where v.VoucherNo is not null ANd II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";

				}
				else
				{
					sql = @"SELECT II.Id AS IssueId
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
							,TUoM1.UserName AS GRNUOM
							,IRD.MaterialTranRate GRNRate
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

                    where v.VoucherNo is null ANd II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";

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

		public IEnumerable<object> GetIssueRegisterDetail(string id)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @"select Id,InventoryIssueDetailId,InventoryReceiveDetailId,Qty,Round(Rate,4) Rate,AddedBy,REPLACE(CONVERT(CHAR(11), AddedDate, 106), ' ', '-') AddedDate  from trn.InventoryIssueHistory where InventoryIssueDetailId='" + id + "'";


				return _sqlRepository.GetDataCollection(sql);
			}

			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}





		#region Issue Register Excel and Pdf Report




		public IWorkbook CreateIssueRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Type)
		{
			try
			{
				var excelEngine = new ExcelEngine();
				var report = new Library.Service.Helpers.ReportUtility();
				var workbook = report.GetWorkbook(ref excelEngine, 2);
				var sheet1 = workbook.Worksheets[0];
				//var sheet2 = workbook.Worksheets[1];               
				//var Head = "Stores Issue Register";// + " " + fromDate + " " + "To" + " " + toDate;

				var Head = "";
				if (Type == "Posted")
				{

					Head = "Stores Issue Register(Posted)";


				}

				else if (Type == "NonPosted")
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



		private void CreateIssueRegisterReportSheet(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
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
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
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
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
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



			sheet1[_row, 26].Text = "Posted (Dr.)";
			sheet1[_row, 26].CellStyle.Font.Size = 10;
			sheet1[_row, 26].CellStyle.Font.Bold = true;
			sheet1[_row, 26].WrapText = true;
			sheet1[_row, 26].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1[_row, 26].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_row, 26, _row, 28].BorderAround(ExcelLineStyle.Hair);
			sheet1.Range[_row, 26, _row, 28].BorderInside(ExcelLineStyle.Hair);
			sheet1.Range[_row, 26, _row, 28].Merge();
			sheet1.Range[_row, 26, _row, 28].CellStyle.FillBackground = ExcelKnownColors.Tan;

			sheet1[_row, 29].Text = "Posted (Cr.)";
			sheet1[_row, 29].CellStyle.Font.Size = 10;
			sheet1[_row, 29].CellStyle.Font.Bold = true;
			sheet1[_row, 29].WrapText = true;
			sheet1[_row, 29].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1[_row, 29].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_row, 29, _row, 31].BorderAround(ExcelLineStyle.Hair);
			sheet1.Range[_row, 29, _row, 31].BorderInside(ExcelLineStyle.Hair);
			sheet1.Range[_row, 29, _row, 31].Merge();
			sheet1.Range[_row, 29, _row, 31].CellStyle.FillBackground = ExcelKnownColors.Tan;



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


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTransactionQtyTotal = sheet1headreColIndex;
			sheet1headreColIndex++;
			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UoM");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "UoM";
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


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Policy Rate";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//         report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Policy Amount");
			//sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Policy Amount";
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
			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;

			var Row_Total_Start = _rowL + 1;
			for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
			{
				_rowL++;

				report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["IssueId"].ToString());
				report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["IssueDate"].ToString());
				report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["Entityname"].ToString());
				report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["CostCenterName"].ToString());
				report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["EmployeeName"].ToString());
				report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["MaterialStorageName"].ToString());
				report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["Status"].ToString());
				report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["IssueDetailId"].ToString());
				report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
				report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
				report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
				report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
				report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
				report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["UOM"].ToString());
				report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AvgRate"].ToString()));
				report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AvgAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PolicyRate"].ToString()));
				report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PolicyAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 23, inventoryMaterialList.Rows[n]["Policy"].ToString());
				report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 25, inventoryMaterialList.Rows[n]["Remarks"].ToString());
				report.SetText(ref sheet1, _rowL, 26, inventoryMaterialList.Rows[n]["GL"].ToString());
				report.SetText(ref sheet1, _rowL, 27, inventoryMaterialList.Rows[n]["Budget"].ToString());
				report.SetText(ref sheet1, _rowL, 28, inventoryMaterialList.Rows[n]["Activity"].ToString());
				report.SetText(ref sheet1, _rowL, 29, inventoryMaterialList.Rows[n]["CGL"].ToString());
				report.SetText(ref sheet1, _rowL, 30, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
				report.SetText(ref sheet1, _rowL, 31, inventoryMaterialList.Rows[n]["CActivity"].ToString());


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
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(AvgAmount)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colAvgAmountTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colAvgAmountTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colAvgAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colAvgAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;


				sumObject = inventoryMaterialList.Compute("Sum(PolicyAmount)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colPolicyAmountTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colPolicyAmountTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colPolicyAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colPolicyAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(BaseQty)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colBaseQtyTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colBaseQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
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

			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
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



		private void CreateIssueRegisterGRNIssueReport(ref IWorksheet sheet1, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId, string fromDate, string toDate, string Type)
		{


			var cmdText = "";
			if (Type == "Posted")
			{
				cmdText = @"SELECT II.Id AS IssueId
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate	 
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
							,HSNC.Code HSNCode
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
							,TUoM1.UserName AS GRNUOM
							,IRD.MaterialTranRate GRNRate
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
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId						
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
                    where v.VoucherNo is not null ANd II.PlantId='" + plantId + "'AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";
			}
			else
			{
				cmdText = @"SELECT II.Id AS IssueId
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate	 
	                        ,MT.UserName MaterialType
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
							,TUoM.UserName AS IssueUOM							
	                        ,TotalIssued=(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0))						
							,Balance=(Isnull(IRD.TransactionQty,0)-(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0)))
	                        ,ISNULL(IGL.UserName,'') AS GL
							,ISNULL(IA.UserName,'') Activity
							,isnull(B.UserName,'') AS Budget
							,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId						
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

			sheet1[_row, 19].Text = "Posted Dr.";
			sheet1.UsedRange.CellStyle.Font.Size = 10;
			sheet1.UsedRange.CellStyle.Font.Bold = true;
			sheet1.UsedRange.WrapText = true;
			sheet1[_row, 19].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1[_row, 19].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_row, 19, _row, 21].BorderAround(ExcelLineStyle.Hair);
			//sheet1.Range[_row, 18, _row, 20].CellStyle.Color="LightYellow";
			sheet1.Range[_row, 19, _row, 21].BorderInside(ExcelLineStyle.Hair);
			sheet1.Range[_row, 19, _row, 21].Merge();
			sheet1.Range[_row, 19, _row, 21].CellStyle.FillBackground = ExcelKnownColors.Tan;

			sheet1[_row, 22].Text = "Posted (Cr.)";
			sheet1.UsedRange.CellStyle.Font.Size = 10;
			sheet1.UsedRange.CellStyle.Font.Bold = true;
			sheet1.UsedRange.WrapText = true;
			sheet1[_row, 22].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			sheet1[_row, 22].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_row, 22, _row, 24].BorderAround(ExcelLineStyle.Hair);
			sheet1.Range[_row, 22, _row, 24].BorderInside(ExcelLineStyle.Hair);
			sheet1.Range[_row, 22, _row, 24].Merge();
			sheet1.Range[_row, 22, _row, 24].CellStyle.FillBackground = ExcelKnownColors.Tan;

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

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Detail Id";
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
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
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

			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;
			var Row_Total_Start = _rowL + 1;
			for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
			{
				_rowL++;
				report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["IssueId"].ToString());
				report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["IssueDate"].ToString());
				report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["GRNDetailId"].ToString());
				report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
				report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
				report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
				report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
				report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
				report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["GRNUOM"].ToString());
				report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNRate"].ToString()));
				report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OtherIssuedQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CurrentIssueQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueUOM"].ToString()));
				report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Balance"].ToString()));
				report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["GL"].ToString());
				report.SetText(ref sheet1, _rowL, 20, inventoryMaterialList.Rows[n]["Budget"].ToString());
				report.SetText(ref sheet1, _rowL, 21, inventoryMaterialList.Rows[n]["Activity"].ToString());
				report.SetText(ref sheet1, _rowL, 22, inventoryMaterialList.Rows[n]["CGL"].ToString());
				report.SetText(ref sheet1, _rowL, 23, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
				report.SetText(ref sheet1, _rowL, 24, inventoryMaterialList.Rows[n]["CActivity"].ToString());
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
							if (im.TotalQty < item.RequisitionQty) throw new CustomException(@"Stock is limited for {" + item.MaterialMasterName + "} {" + item.ArticleName + "} {" + item.TransactionQty + "} . Available stock is {" + im.TotalQty + "}");

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

		public IEnumerable<object> GetGRNFixedAssetList(string plantId, string materialStorageId)
		{
			try
			{
				var sql = @"SELECT IR.Id GRNNo,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END, IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName,
                                     FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate, P.Code AS PartyCode, P.UserName AS PartyName, UoM.UserName AS TransactionUoM,CU.Code AS CurrencyCode
                                     , IR.IsNonCreditable, FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate, FORMAT(IR.EntryDate,'dd-MMM-yyyy') EntryDate, FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate, IR.DeliveryByAddress
                                     , DPP.UserName AS DeliveryBy, CP.UserName AS PartyAccountGroupName, PT.UserName AS PaymentTermName, IR.GateEntryNo, IR.InvoicingByAddress, IPP.UserName AS InvoicingBy,IR.ToCurrencyRate
                                     , MGM.UserName AS MaterialGroupMasterName,IRD.BaseUoMFactor
									 ,IR.POId, IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId
									  ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty,IRD.BaseUOMId,IRD.TransactionUoMId
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						,ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,Isnull(InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-Isnull(InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
						 , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, CU.Code AS TCurrency, IRD.MaterialTranAmount,IR.ToCurrencyRate
                       , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                           , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
							,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						    ,GL.UserName GLName,GL.Id GLGeneralInfoId,IRD.PostDrBudgetMasterId BudgetMasterId,B.UserName BudgetName,IRD.PostDrActivityId ActivityId,A.UserName ActivityName
							, IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue, isnull(C.UserName,'') CountryName, C.Id CountryId
							, ISNULL(PO1.PoId,'') PurchaseOrderId,REPLACE(CONVERT(CHAR(11), PO1.PODate, 106),' ','-')  PurchaseOrderDate,ISNULL(PLC.LCRef,'') LCRef,ISNULL(BMTbl.AccountTitle,'') AccountTitle,ISNULL(PDA.AcceptanceNo,'') AcceptanceNo,PDA.AcceptanceDate,PO.DocRefNo PODocRefNo,PO.PODate,IR.DocRefNo GRNDocRefNo,IR.GRNDate
								FROM TRN.InventoryReceiveDetail IRD 
								LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
								LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
								LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
								LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
							    JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							    LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							    LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							    LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IRD.TransactionUoMId=UoM.Id
								LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
								LEFT JOIN [MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
								LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
								LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
                                JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                     			            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                left JOIN SCS.Country C On C.Id=IM.countryId
								LEFT JOIN(
										select PDAMAP.GRNId, REPLACE(Convert(VARCHAR(11), IR.PODate, 106), ' ', '-') AS PODate
										,PoId=STUFF((select distinct ','+xpo.Id from
										trn.PurchaseOrder xpo
										INNER JOin TRN.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.PoId
										where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

										from TRN.POGGRNMap PDAMAP
										LEFT JOIN [TRN].PurchaseOrder IR ON IR.Id = PDAMAP.PoId
										
										group by PDAMAP.GRNId, IR.podate
										)PO1 ON PO1.GRNId = IRD.InventoryReceiveId
								 --LEFT JOIN [TRN].[GateEntry] GTE ON GTE.ID= IR.GateEntryNo

								Left Join TRN.PurchaseOrder PO ON PO.Id=IRD.POId
								Left join dbo.PurchaseLC PLC ON PLC.Id=PO.PurchaseLCId
								LEFT JOIN [MST].[BankMaster] BMTbl ON BMTbl.Id=PLC.OpeningBankMasterId
								Left Join TRN.GRNAcceptanceMap APOMap ON APOMap.GRNId=IR.Id
								left JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=APOMap.PurchaseDocumentAcceptanceId
                                     WHERE IRD.IsAsset=1 AND IRD.CapitalizeVoucherDetailId IS NULL AND IR.PlantId='" + plantId + @"' 
                                    --AND (ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))>0 
									AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0))+isnull(IRD.IssueReturnQty,0))>0
								    AND IRD.MaterialStorageId='" + materialStorageId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		#endregion


		#region Asset Issue Slip

		public IEnumerable<object> GetAssetIssueSlipWithGRN(string plantId, string materialStorageId)
		{
			try
			{
				var sql = @"SELECT IR.Id GRNNo,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END, IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName,
                                     FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate, P.Code AS PartyCode, P.UserName AS PartyName, UoM.UserName AS TransactionUoM,CU.Code AS CurrencyCode
                                     , IR.IsNonCreditable, FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate, FORMAT(IR.EntryDate,'dd-MMM-yyyy') EntryDate, FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate, IR.DeliveryByAddress
                                     , DPP.UserName AS DeliveryBy, CP.UserName AS PartyAccountGroupName, PT.UserName AS PaymentTermName, IR.GateEntryNo, IR.InvoicingByAddress, IPP.UserName AS InvoicingBy,IR.ToCurrencyRate
                                     , MGM.UserName AS MaterialGroupMasterName
									 ,IR.POId, IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId
									  ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty,IRD.BaseUOMId,IRD.TransactionUoMId
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						 ,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
						 , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, CU.Code AS TCurrency, IRD.MaterialTranAmount
                       , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                           , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
							,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						    ,GL.UserName GLName,GL.Id GLGeneralInfoId,IRD.PostDrBudgetMasterId BudgetMasterId,B.UserName BudgetName,IRD.PostDrActivityId ActivityId,A.UserName ActivityName
							, IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                            ,IReq.Id IssueRequestDatailId,IReq.IssueRequestMasterId
									 FROM TRN.InventoryReceiveDetail IRD 
                                     LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
                                     LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                                     LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
								LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
							    JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							    LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							    LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							    LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                     LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IRD.TransactionUoMId=UoM.Id
									 LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
                                     JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                     LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                      LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                     			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                     JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                     LEFT JOIN [TRN].[IssueRequest] AS IReq ON MM.Id=IReq.MaterialMasterId 
                                        AND IReq.ArticleId=ART.Id 
                                     LEFT JOIN [TRN].[IssueRequestMaster] AS IRM ON IRM.Id =IReq.IssueRequestMasterId 
                                     LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                     WHERE IRD.IsAsset=1 AND IRD.CapitalizeVoucherDetailId IS NULL AND IR.PlantId='" + plantId + @"' 
                                    AND IRM.IssueSlipType='AssetSlip' 
                                    AND (ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))>0 AND IRD.MaterialStorageId='" + materialStorageId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		#endregion
		public IEnumerable<object> GetApprovedIssueSlip()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty,Orderspecific=CASE WHEN Orderspecific='Yes' Then 'Yes' else 'No' End from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty,IRM.Orderspecific
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                             
                           Where IRM.CheckedBy IS NOT NULL 
						   AND IRM.CheckedByStatus='Checked' 
						   AND IRM.AuthorizedByStatus='Approved' 
						   AND IRM.AuthorizedBy IS NOT null  
						   AND IRM.IssueSlipType='InventorySlip'
						   AND IRM.PlantId='" + identity.PlantId + @"'
                           --Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='Checked' OR IRM.CheckedByStatus='Approval'AND IRM.AuthorizedByStatus IS Not NULL  AND IRM.AuthorizedBy IS null OR IRM.AuthorizedBy IS NOT null And IRM.PreparedBy='" + identity.EmployeeId + @"'
                           --Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='ForChecked' AND IRM.AuthorizedByStatus IS NULL AND IRM.IssueSlipType='AssetSlip' AND IRM.AuthorizedBy IS null --And IRM.PreparedBy='" + identity.EmployeeId + @"'
                           UNION ALL
						   SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty,IRM.Orderspecific
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                             
                           Where IRM.CheckedBy IS  NULL 
						   AND IRM.CheckedByStatus IS NULL
						   AND IRM.AuthorizedByStatus='Approved' 
						   AND IRM.AuthorizedBy IS NOT null  
						   AND IRM.IssueSlipType='InventorySlip'
						   AND IRM.PlantId='" + identity.PlantId + @"'
                           )x 
                            Group by Id ,x.PreparedBy,x.AddedDate ,Orderspecific                             
                          ";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}




		public IEnumerable<object> GetAssetIssueSlip()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                             
                           Where IRM.CheckedBy IS NOT NULL 
						   AND IRM.CheckedByStatus='Checked' 
						   AND IRM.AuthorizedByStatus='Approval' 
						   AND IRM.AuthorizedBy IS NOT null  
						   AND IRM.IssueSlipType='AssetSlip'
						   AND IRM.PlantId='" + identity.PlantId + @"'
                           --Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='Checked' OR IRM.CheckedByStatus='Approval'AND IRM.AuthorizedByStatus IS Not NULL  AND IRM.AuthorizedBy IS null OR IRM.AuthorizedBy IS NOT null And IRM.PreparedBy='" + identity.EmployeeId + @"'
                           --Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='ForChecked' AND IRM.AuthorizedByStatus IS NULL AND IRM.IssueSlipType='AssetSlip' AND IRM.AuthorizedBy IS null --And IRM.PreparedBy='" + identity.EmployeeId + @"'
                           )x 
                            Group by Id ,x.PreparedBy,x.AddedDate                             
                          ";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		public IEnumerable<object> GetApprovedIssueSlipDetails(string Id, string StorageLocationId, string OrderSpecific)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			var sql = "";
				try
				{
				if (OrderSpecific == "Yes")
				{

					sql = @"Select 
							MGM.UserName MaterialMasterGroupName
                            ,IR.MaterialMasterId
							,mm.UserName Material
	                        ,IR.ArticleId
							,ART.StandardName ArticleName
							,MT.UserName MaterialType
							,IR.FirstCharacteristicsId
							,FC.UserName AS FirstCharacteristics
							,IR.FirstCharacteristicsValueId
							,FCV.UserName AS Sku1
							,IR.SecondCharacteristicsId
							,SC.UserName AS SecondCharacteristics
							,IR.SecondCharacteristicsValueId
							,SCV.UserName AS Sku2
							,IR.ThirdCharacteristicsId
							,TC.UserName AS ThirdCharacteristics
							,IR.ThirdCharacteristicsValueId
							,TCV.UserName AS Sku3,C.UserName CountryName,C.Id CountryId
							,TUoM.Id BaseUOMId
							,TUoM.Id TransactionUoMId
							,TUoM.UserName UOM
							 --,isnull(PostingQty.UoM,'') UOM
							,TUoM.UserName TransactionUoM
							,IR.CostCenterId
							,CC.UserName AS CostCenterName
							,IR.GLGeneralInfoId 
							,IGL1.UserName GLName									
							,IR.BudgetMasterId									
							,B1.UserName BudgetName
							,IR.ExpenseActivityId
							,IA1.UserName ActivityName	
							,IRM.Id IssueRequestMasterId
							,IR.Id IssueRequest								
                           --,RequestedQty=Isnull(IR.RequestedQty,0)-ISNULL(ABC.Qty,0)							
                            , PostingQty.MaterialStorageId ,Convert(bit,0)  'check'
							,sum(IR.RequestedQty) RequestedQty
							,sum(IDRM.Qty) IssuedQty
							,sum(Isnull(ApprovedQty.ApprovedQty,0)) ApprovedQty
							,sum(Isnull(UnApprovedQty.UnApprovedQty,0)) UnApprovedQty
							,TotalStock=Sum((isnull(ApprovedQty.ApprovedQty,0) + ISNULL(UnApprovedQty.UnApprovedQty,0)))
							,Sum(Isnull(PostingQty.PostingQty,0)) PostingQty
							,BalanceQty=Sum(Isnull(IR.RequestedQty,0)-ISNULL(IDRM.Qty,0))
							FROM trn.IssueRequest IR									
							LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id=IR.IssueRequestMasterId                                    
							Left JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
							LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
							LEFT JOIN [SCS].[Country] AS C ON C.Id = IR.CountryId
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON Ir.TransactionUoMId = TUoM.Id
							LEFT JOIN [SEC].[User] As Us On IR.AddedBy=Us.UserId                                   
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id

							Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
							LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IR.GLGeneralInfoId 
							LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IR.BudgetMasterId
							Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
							LEFT JOIN HKP.Activity IA1 ON IA1.Id=IR.ExpenseActivityId
							LEFT JOIN(
										SELECT TUoM.Id UoM,0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
											,IM.MaterialMasterId
												,IM.ArticleId
												,IM.FirstCharacteristicsValueId
												, IM.SecondCharacteristicsValueId
												,IM.ThirdCharacteristicsValueId
												, IRD.MaterialStorageId,IM.PlantId
											FROM [TRN].[InventoryReceiveDetail] AS IRD
												LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
												LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
												LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
												LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
												LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
												LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
											WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
												AND IR.IsApproved=1						
												Group BY 
												IM.MaterialMasterId
												,IM.ArticleId
												,IM.FirstCharacteristicsValueId
												, IM.SecondCharacteristicsValueId
												,IM.ThirdCharacteristicsValueId
													, IRD.MaterialStorageId,TUoM.Id,IM.PlantId
									)ApprovedQty ON 
												ApprovedQty.MaterialMasterId=IR.MaterialMasterId 
												AND ApprovedQty.ArticleId=IR.ArticleId
												AND ISNULL(ApprovedQty.FirstCharacteristicsValueId,'')=ISNULL(IR.FirstCharacteristicsValueId,'')
												AND ISNULL(ApprovedQty.SecondCharacteristicsValueId,'')=ISNULL(IR.SecondCharacteristicsValueId,'')
												AND ISNULL(ApprovedQty.ThirdCharacteristicsValueId,'')=ISNULL(IR.ThirdCharacteristicsValueId,'')
												AND ApprovedQty.MaterialStorageId='" + StorageLocationId + @"'
												AND  ApprovedQty.PlantId=IRM.PlantId
												AND  ApprovedQty.UoM=IR.TransactionUoMId

                            LEFT JOIN(
										  SELECT TUoM.Id UoM,0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
										  ,IM.MaterialMasterId
																,IM.ArticleId
																,IM.FirstCharacteristicsValueId
																, IM.SecondCharacteristicsValueId
																,IM.ThirdCharacteristicsValueId
																, IRD.MaterialStorageId,IM.PlantId
												FROM [TRN].[InventoryReceiveDetail] AS IRD
												JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
												JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
												LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
												JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
												JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
												JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
												WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
												AND IR.IsApproved=0						
										Group BY 
												IM.MaterialMasterId
												,IM.ArticleId
												,IM.FirstCharacteristicsValueId
												, IM.SecondCharacteristicsValueId
												,IM.ThirdCharacteristicsValueId
												, IRD.MaterialStorageId,TUoM.Id,IM.PlantId
								)UnApprovedQty ON UnApprovedQty.MaterialMasterId=IR.MaterialMasterId 
												AND UnApprovedQty.ArticleId=IR.ArticleId
												AND ISNULL(UnApprovedQty.FirstCharacteristicsValueId,'')=ISNULL(IR.FirstCharacteristicsValueId,'')
												AND ISNULL(UnApprovedQty.SecondCharacteristicsValueId,'')=ISNULL(IR.SecondCharacteristicsValueId,'')
												AND ISNULL(UnApprovedQty.ThirdCharacteristicsValueId,'')=ISNULL(IR.ThirdCharacteristicsValueId,'')
												AND ApprovedQty.MaterialStorageId='" + StorageLocationId + @"'
												AND  UnApprovedQty.PlantId=IRM.PlantId
												AND  UnApprovedQty.UoM=IR.TransactionUoMId

								Left JOIN(
										SELECT TUoM.Id UoM,0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
										,IM.MaterialMasterId
																,IM.ArticleId
																,IM.FirstCharacteristicsValueId
																, IM.SecondCharacteristicsValueId
																,IM.ThirdCharacteristicsValueId
																, IRD.MaterialStorageId
																,IM.PlantId
												FROM [TRN].[InventoryReceiveDetail] AS IRD
												JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
												JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
												LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
												JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
												JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
												JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
												WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
												AND IR.[Status]='Posting' 						
												Group BY 
																IM.MaterialMasterId
																,IM.ArticleId
																,IM.FirstCharacteristicsValueId
																, IM.SecondCharacteristicsValueId
																,IM.ThirdCharacteristicsValueId
																, IRD.MaterialStorageId,TUoM.Id,IM.PlantId
												)PostingQty  ON PostingQty.MaterialMasterId=IR.MaterialMasterId 
												AND PostingQty.ArticleId=IR.ArticleId
												AND ISNULL(PostingQty.FirstCharacteristicsValueId,'')=ISNULL(IR.FirstCharacteristicsValueId,'')
												AND ISNULL(PostingQty.SecondCharacteristicsValueId,'')=ISNULL(IR.SecondCharacteristicsValueId,'')
												AND ISNULL(PostingQty.ThirdCharacteristicsValueId,'')=ISNULL(IR.ThirdCharacteristicsValueId,'')
                                                AND PostingQty.MaterialStorageId='" + StorageLocationId + @"'
												AND  PostingQty.PlantId=IRM.PlantId
												AND  PostingQty.UoM=IR.TransactionUoMId
                                                LEFT JOIN(
															SELECT isnull(sum(c.Qty),0) Qty ,c.IssueRequestDetailId	 
															from trn.InventoryIssue  a          
															LEFT JOIN trn.InventoryIssueDetail b ON b.InventoryIssueId=a.id
															Left JOIN  trn.InventoryIssueHistory c On c.InventoryIssueDetailId=b.Id 
															Left JOIN trn.IssueRequest IR	ON IR.Id=c.IssueRequestDetailId	
															LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id=IR.IssueRequestMasterId   
															--where IRM.Id='2067'				
															GROUp BY c.IssueRequestDetailId
															)ABC ON ABC.IssueRequestDetailId=IR.Id
                                                Left Join (select aa.Id, sum(cc.Qty) Qty  
															from trn.IssueRequest aa
															left join trn.IssueRequestMaster dd on dd.id=aa.IssueRequestMasterId
															left join [TRN].[IssueRequestBOQMap] bb on bb.IssueRequestDetailId=aa.id
															left join  [TRN].[IssueDetailAndIssueRequestMap] cc on cc.IssueRequestBOQMapId=bb.Id 
															where cc.IssueRequestBOQMapId is not null --and  dd.Id='2150'
															group by aa.Id
												) IDRM ON IDRM.Id=IR.id
								                Where IRM.Id='" + Id + @"' Group BY
												MGM.UserName
                            ,IR.MaterialMasterId
							,mm.UserName
	                        ,IR.ArticleId
							,ART.StandardName
							,MT.UserName
							,IR.FirstCharacteristicsId
							,FC.UserName
							,IR.FirstCharacteristicsValueId
							,FCV.UserName
							,IR.SecondCharacteristicsId
							,SC.UserName
							,IR.SecondCharacteristicsValueId
							,SCV.UserName
							,IR.ThirdCharacteristicsId
							,TC.UserName
							,IR.ThirdCharacteristicsValueId
							,TCV.UserName ,C.UserName ,C.Id
							,TUoM.Id
							,TUoM.Id
							--,TUoM.UserName UOM
							 --, PostingQty.UoM
							,TUoM.UserName
							,IR.CostCenterId
							,CC.UserName
							,IR.GLGeneralInfoId
							,IGL1.UserName
							,IR.BudgetMasterId
							,B1.UserName
							,IR.ExpenseActivityId
							,IA1.UserName
							,IRM.Id
							,IR.Id
                            , PostingQty.MaterialStorageId";
				}
				else
				{

					sql = @"Select 
							MGM.UserName MaterialMasterGroupName
                            ,IR.MaterialMasterId
							,mm.UserName Material
	                        ,IR.ArticleId
							,ART.StandardName ArticleName
							,MT.UserName MaterialType
							,IR.FirstCharacteristicsId
							,FC.UserName AS FirstCharacteristics
							,IR.FirstCharacteristicsValueId
							,FCV.UserName AS Sku1
							,IR.SecondCharacteristicsId
							,SC.UserName AS SecondCharacteristics
							,IR.SecondCharacteristicsValueId
							,SCV.UserName AS Sku2
							,IR.ThirdCharacteristicsId
							,TC.UserName AS ThirdCharacteristics
							,IR.ThirdCharacteristicsValueId
							,TCV.UserName AS Sku3,C.UserName CountryName,C.Id CountryId
							,TUoM.Id BaseUOMId
							,TUoM.Id TransactionUoMId
							,TUoM.UserName UOM
							 --,isnull(PostingQty.UoM,'') UOM
							,TUoM.UserName TransactionUoM
							,IR.CostCenterId
							,CC.UserName AS CostCenterName
							,IR.GLGeneralInfoId 
							,IGL1.UserName GLName									
							,IR.BudgetMasterId									
							,B1.UserName BudgetName
							,IR.ExpenseActivityId
							,IA1.UserName ActivityName	
							,IRM.Id IssueRequestMasterId
							,IR.Id IssueRequest								
                           --,RequestedQty=Isnull(IR.RequestedQty,0)-ISNULL(ABC.Qty,0)							
                            , PostingQty.MaterialStorageId ,Convert(bit,0)  'check'
							,sum(IR.RequestedQty) RequestedQty
							,sum(IDRM.Qty) IssuedQty
							,sum(Isnull(ApprovedQty.ApprovedQty,0)) ApprovedQty
							,sum(Isnull(UnApprovedQty.UnApprovedQty,0)) UnApprovedQty
							,TotalStock=Sum((isnull(ApprovedQty.ApprovedQty,0) + ISNULL(UnApprovedQty.UnApprovedQty,0)))
							,Sum(Isnull(PostingQty.PostingQty,0)) PostingQty
							,BalanceQty=Sum(Isnull(IR.RequestedQty,0)-ISNULL(IDRM.Qty,0))
							FROM trn.IssueRequest IR									
							LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id=IR.IssueRequestMasterId                                    
							Left JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
							LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
							LEFT JOIN [SCS].[Country] AS C ON C.Id = IR.CountryId
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON Ir.TransactionUoMId = TUoM.Id
							LEFT JOIN [SEC].[User] As Us On IR.AddedBy=Us.UserId                                   
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id

							Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
							LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IR.GLGeneralInfoId 
							LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IR.BudgetMasterId
							Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
							LEFT JOIN HKP.Activity IA1 ON IA1.Id=IR.ExpenseActivityId
							LEFT JOIN(
										SELECT TUoM.Id UoM,0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
											,IM.MaterialMasterId
												,IM.ArticleId
												,IM.FirstCharacteristicsValueId
												, IM.SecondCharacteristicsValueId
												,IM.ThirdCharacteristicsValueId
												, IRD.MaterialStorageId,IM.PlantId
											FROM [TRN].[InventoryReceiveDetail] AS IRD
												LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
												LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
												LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
												LEFT JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
												LEFT JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
												LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
											WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
												AND IR.IsApproved=1						
												Group BY 
												IM.MaterialMasterId
												,IM.ArticleId
												,IM.FirstCharacteristicsValueId
												, IM.SecondCharacteristicsValueId
												,IM.ThirdCharacteristicsValueId
													, IRD.MaterialStorageId,TUoM.Id,IM.PlantId
									)ApprovedQty ON 
												ApprovedQty.MaterialMasterId=IR.MaterialMasterId 
												AND ApprovedQty.ArticleId=IR.ArticleId
												AND ISNULL(ApprovedQty.FirstCharacteristicsValueId,'')=ISNULL(IR.FirstCharacteristicsValueId,'')
												AND ISNULL(ApprovedQty.SecondCharacteristicsValueId,'')=ISNULL(IR.SecondCharacteristicsValueId,'')
												AND ISNULL(ApprovedQty.ThirdCharacteristicsValueId,'')=ISNULL(IR.ThirdCharacteristicsValueId,'')
												AND ApprovedQty.MaterialStorageId='" + StorageLocationId + @"'
												AND  ApprovedQty.PlantId=IRM.PlantId
												AND  ApprovedQty.UoM=IR.TransactionUoMId

                            LEFT JOIN(
										  SELECT TUoM.Id UoM,0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
										  ,IM.MaterialMasterId
																,IM.ArticleId
																,IM.FirstCharacteristicsValueId
																, IM.SecondCharacteristicsValueId
																,IM.ThirdCharacteristicsValueId
																, IRD.MaterialStorageId,IM.PlantId
												FROM [TRN].[InventoryReceiveDetail] AS IRD
												JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
												JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
												LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
												JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
												JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
												JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
												WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
												AND IR.IsApproved=0						
										Group BY 
												IM.MaterialMasterId
												,IM.ArticleId
												,IM.FirstCharacteristicsValueId
												, IM.SecondCharacteristicsValueId
												,IM.ThirdCharacteristicsValueId
												, IRD.MaterialStorageId,TUoM.Id,IM.PlantId
								)UnApprovedQty ON UnApprovedQty.MaterialMasterId=IR.MaterialMasterId 
												AND UnApprovedQty.ArticleId=IR.ArticleId
												AND ISNULL(UnApprovedQty.FirstCharacteristicsValueId,'')=ISNULL(IR.FirstCharacteristicsValueId,'')
												AND ISNULL(UnApprovedQty.SecondCharacteristicsValueId,'')=ISNULL(IR.SecondCharacteristicsValueId,'')
												AND ISNULL(UnApprovedQty.ThirdCharacteristicsValueId,'')=ISNULL(IR.ThirdCharacteristicsValueId,'')
												AND ApprovedQty.MaterialStorageId='" + StorageLocationId + @"'
												AND  UnApprovedQty.PlantId=IRM.PlantId
												AND  UnApprovedQty.UoM=IR.TransactionUoMId

								Left JOIN(
										SELECT TUoM.Id UoM,0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
										,IM.MaterialMasterId
																,IM.ArticleId
																,IM.FirstCharacteristicsValueId
																, IM.SecondCharacteristicsValueId
																,IM.ThirdCharacteristicsValueId
																, IRD.MaterialStorageId
																,IM.PlantId
												FROM [TRN].[InventoryReceiveDetail] AS IRD
												JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
												JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
												LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
												JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
												JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
												JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
												WHERE IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
												AND IR.[Status]='Posting' 						
												Group BY 
																IM.MaterialMasterId
																,IM.ArticleId
																,IM.FirstCharacteristicsValueId
																, IM.SecondCharacteristicsValueId
																,IM.ThirdCharacteristicsValueId
																, IRD.MaterialStorageId,TUoM.Id,IM.PlantId
												)PostingQty  ON PostingQty.MaterialMasterId=IR.MaterialMasterId 
												AND PostingQty.ArticleId=IR.ArticleId
												AND ISNULL(PostingQty.FirstCharacteristicsValueId,'')=ISNULL(IR.FirstCharacteristicsValueId,'')
												AND ISNULL(PostingQty.SecondCharacteristicsValueId,'')=ISNULL(IR.SecondCharacteristicsValueId,'')
												AND ISNULL(PostingQty.ThirdCharacteristicsValueId,'')=ISNULL(IR.ThirdCharacteristicsValueId,'')
                                                AND PostingQty.MaterialStorageId='" + StorageLocationId + @"'
												AND  PostingQty.PlantId=IRM.PlantId
												AND  PostingQty.UoM=IR.TransactionUoMId
                                                LEFT JOIN(
															SELECT isnull(sum(c.Qty),0) Qty ,c.IssueRequestDetailId	 
															from trn.InventoryIssue  a          
															LEFT JOIN trn.InventoryIssueDetail b ON b.InventoryIssueId=a.id
															Left JOIN  trn.InventoryIssueHistory c On c.InventoryIssueDetailId=b.Id 
															Left JOIN trn.IssueRequest IR	ON IR.Id=c.IssueRequestDetailId	
															LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id=IR.IssueRequestMasterId   
															--where IRM.Id='2067'				
															GROUp BY c.IssueRequestDetailId
															)ABC ON ABC.IssueRequestDetailId=IR.Id
                                                Left Join (select aa.Id, sum(cc.Qty) Qty  
															from trn.IssueRequest aa
															left join trn.IssueRequestMaster dd on dd.id=aa.IssueRequestMasterId
															left join [TRN].[IssueRequestBOQMap] bb on bb.IssueRequestDetailId=aa.id
															left join  [TRN].[IssueDetailAndIssueRequestMap] cc on cc.IssueRequestBOQMapId=bb.Id 
															where cc.IssueRequestBOQMapId is not null --and  dd.Id='2150'
															group by aa.Id
												) IDRM ON IDRM.Id=IR.id
								                Where IRM.Id='" + Id + @"' Group BY
												MGM.UserName
                            ,IR.MaterialMasterId
							,mm.UserName
	                        ,IR.ArticleId
							,ART.StandardName
							,MT.UserName
							,IR.FirstCharacteristicsId
							,FC.UserName
							,IR.FirstCharacteristicsValueId
							,FCV.UserName
							,IR.SecondCharacteristicsId
							,SC.UserName
							,IR.SecondCharacteristicsValueId
							,SCV.UserName
							,IR.ThirdCharacteristicsId
							,TC.UserName
							,IR.ThirdCharacteristicsValueId
							,TCV.UserName ,C.UserName ,C.Id
							,TUoM.Id
							,TUoM.Id
							--,TUoM.UserName UOM
							 --, PostingQty.UoM
							,TUoM.UserName
							,IR.CostCenterId
							,CC.UserName
							,IR.GLGeneralInfoId
							,IGL1.UserName
							,IR.BudgetMasterId
							,B1.UserName
							,IR.ExpenseActivityId
							,IA1.UserName
							,IRM.Id
							,IR.Id
                            , PostingQty.MaterialStorageId";
				}
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}



		public GridModel GetDeletableIssueList(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText = @"SELECT E.UserName AS Entity ,II.IssueType, II.Id,V.VoucherNo,II.VoucherId, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate,'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 ,EI.EmployeeCode+' - '+EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount,II.Remarks,II.Id AS IssueId
                                FROM [TRN].[InventoryIssue] AS II
                                JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId=II.Id AND IID.IsAsset=0
                                JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId=MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId=II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id=II.EntityId
								Left JOIN TRN.Voucher V on V.Id=II.VoucherId
                                WHERE II.PlantId='" + plantId + @"' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,V.VoucherNo,II.VoucherId";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

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

		public IEnumerable<object> MaterialIssueDetailsData1(string inveReveiveId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @" SELECT IR.Id IssueNo
                                ,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,null PODepended 
								,IR.Id PONumber  
	                            ,IR.IssueRequestMasterId  
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
		                       -- ,'' CurrencyId
                                ,IR.CurrencyId
	                            --,null AS CurrencyName
                                ,CUR.Code CurrencyName
	                            ,null as ToCurrencyRate
		                        ,null AS BaseCurrencyName
		                        ,NULL PaymentTerm
	                          ,MM.UserName Materials
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IOM.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS SKU1
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SKU2
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS SKU3
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                             ,ROUND(IIH.Qty, 2) Qty
	                          ,ROUND(IIH.Rate,2) TransactionRate
	                          ,ROUND((IIH.Qty*IIH.Rate), 2) AS TrnAmount
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
	                          ,TUoM.UserName AS UOM
                              ,IRD.Id InventoryReceiveDetailId
							  ,IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter,IRD.Comments
                              FROM TRN.InventoryIssue IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN trn.InventoryIssueDetail IRD ON IR.Id = IRD.InventoryIssueId		
                          LEFT JOIN (Select InventoryIssueDetailId,IssueRequestDetailId,Sum(Qty) Qty,sum(qty*rate)/Sum(Qty) Rate, sum(qty*rate) TrnAmount from trn.InventoryIssueHistory group by InventoryIssueDetailId,IssueRequestDetailId)IIH ON IIH.InventoryIssueDetailId = IRD.Id		
						 				                                   
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
                         LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						 Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						 LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						 LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
						--WHERE IR.Id IS NULL
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


		public IEnumerable<object> MaterialIssueDetailsData(string inveReveiveId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @"  SELECT IR.Id IssueNo
                                ,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,null PODepended 
								,IR.Id PONumber  
	                            ,IR.IssueRequestMasterId  
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
                                ,IR.CurrencyId
                                ,CUR.Code CurrencyName
	                            ,null as ToCurrencyRate
		                        ,null AS BaseCurrencyName
		                        ,NULL PaymentTerm
	                          ,MM.UserName Materials
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IOM.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS SKU1
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SKU2
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS SKU3
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRD.Qty, 2) Qty
	                          ,null ChargesTranAmount
	                          ,null CountryId
                              ,IRD.Id InventoryReceiveDetailId,IRD.IsCapitalize,TUOM.UserName UOM,
							  IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter, IIH.StandardName StorageLocation
                              FROM TRN.[InventoryIssueReturn] IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN [TRN].InventoryIssueReturnHistory IRD ON IR.Id = IRD.InventoryIssueReturnId		
						LEFT JOIN [TRN].[VoucherDetail]	VD ON VD.Id=IRD.CapitalizeVoucherDetailId
						 LEFT JOIN [HKP].[MaterialStorage] IIH ON IIH.id = IRD.StorageLocationId
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
                         LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						 Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						 LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
						 LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
                         LEFT JOIN [SCS].[UnitOfMeasurement] TUOM ON TUOM.id = IRD.BaseUOMId
						--WHERE IR.Id IS NULL
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


		public IEnumerable<object> GetIssueReturnRegister(string fromDate, string toDate, string Type)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				//if (Type == "Posted")
				//{

				//}
				var sql = "";


				sql = @"SELECT II.Id AS IssueId
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
                            ,CC.UserName CostCenterName,EI.EmployeeName,TUOM.UserName AS UOM
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
                        where II.PlantId='" + identity.PlantId + "'AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";





				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}



		#region Physical Stock Adjustment 


		public IEnumerable<object> GetDataByPhysicalStockAdjustment(string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"SELECT E.UserName AS Entity ,isnull(II.IssueType,'') issuetype, II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount,II.Remarks,II.Id AS IssueId,II.OrderRefNo
                                    FROM[TRN].PhysicalStockAdjustmentMaster
        AS II
                                JOIN TRN.PhysicalStockAdjustmentDetail AS IID ON IID.PhysicalStockAdjustmentMasterID= II.Id AND IID.IsAsset= 0
                                JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                                left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                                Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                                WHERE II.PlantId= '" + identity.PlantId + @"' AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  Order BY II.Id DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}
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
								decimal policyAmmount = 0;

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
		public IEnumerable<object> MaterialAdjustmentDetailsData(string inveReveiveId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @"   SELECT IR.Id IssueNo
                                ,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,null PODepended 
								,IR.Id PONumber  
	                            ,IR.IssueRequestMasterId  
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
		                       -- ,'' CurrencyId
                                ,IR.CurrencyId
	                            --,null AS CurrencyName
                                ,CUR.Code CurrencyName
	                            ,null as ToCurrencyRate
		                        ,null AS BaseCurrencyName
		                        ,NULL PaymentTerm
	                          ,MM.UserName Materials
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IOM.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS SKU1
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SKU2
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS SKU3
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                            ,ROUND(IRD.BaseQty, 2) Qty
	                          ,ROUND(IRD.PolicyRate, 2) TransactionRate
	                          ,ROUND((IRD.PolicyAmount), 2) AS TrnAmount
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
	                          ,TUoM.UserName AS UOM
                              ,IRD.Id InventoryReceiveDetailId
							  ,IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter
                              FROM TRN.PhysicalStockAdjustmentMaster IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN trn.PhysicalStockAdjustmentDetail IRD ON IR.Id = IRD.PhysicalStockAdjustmentMasterID	
                         LEFT JOIN trn.PhysicalStockAdjustmentHistory IIH ON IIH.PhysicalStockAdjustmentDetailId = IRD.Id	
						 				                                   
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
                         LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						 Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						 LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						 LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
						--WHERE IR.Id IS NULL
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

		#endregion Physical Stock Adjustment 



		#region Inventory sales
		public IEnumerable<object> GetDataByInventorySales(string plantId, string tabType)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				//if (tabType == "1")
				//{
				//	sql = @"SELECT * FROM(SELECT E.UserName AS Entity 
				//			,isnull(II.IssueType,'') issuetype
				//			, II.Id, II.CompanyGroupId
				//			, II.CompanyId, II.PlantId
				//			, II.EntityId, II.MaterialStorageId
				//			,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
				//			, MS.UserName AS MaterialStorage 
				//			,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
				//			,SUM(IID.TransactionQty) Qty
				//			,SUM(IID.PolicyAmount) Amount
				//			,II.Remarks,II.Id AS IssueId
				//			,II.OrderRefNo
				//			,C.Id CountryId,c.UserName CountryName
				//                       ,PPI.UserName BillTo
				//			,PPI1.UserName ShipTo
				//			FROM[TRN].[InventorySales] AS II
				//			left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
				//			left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
				//			left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
				//			Left JOIN [ORG].[Entity] E On E.id= II.EntityId
				//			left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
				//			left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
				//			left JOIN SCS.Country c ON C.Id=IR.CountryId
				//                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
				//			LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
				//			WHERE II.PlantId= '" + plantId + @"' 
				//                        AND II.CheckedByStatus='For Checking'
				//			AND ISNULL(II.[Status],'') <>'Posting' 
				//			GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//			,II.SalesDate, MS.UserName
				//			,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
				//			,C.Id ,c.UserName,PPI.UserName ,PPI1.UserName  

				//                        UNION ALL
				//                        SELECT E.UserName AS Entity 
				//			,isnull(II.IssueType,'') issuetype
				//			, II.Id, II.CompanyGroupId
				//			, II.CompanyId, II.PlantId
				//			, II.EntityId, II.MaterialStorageId
				//			,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
				//			, MS.UserName AS MaterialStorage 
				//			,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
				//			,SUM(IID.TransactionQty) Qty
				//			,SUM(IID.PolicyAmount) Amount
				//			,II.Remarks,II.Id AS IssueId
				//			,II.OrderRefNo
				//			,C.Id CountryId,c.UserName CountryName
				//                       ,PPI.UserName BillTo
				//			,PPI1.UserName ShipTo
				//			FROM[TRN].[InventorySales] AS II
				//			left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
				//			left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
				//			left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
				//			Left JOIN [ORG].[Entity] E On E.id= II.EntityId
				//			left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
				//			left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
				//			left JOIN SCS.Country c ON C.Id=IR.CountryId
				//                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
				//			LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
				//			WHERE II.PlantId= '" + plantId + @"' 
				//                        AND II.CheckedByStatus IS NULL
				//                        AND II.ApprovedByStatus IS NULL
				//			AND ISNULL(II.[Status],'') <>'Posting' 
				//			GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//			,II.SalesDate, MS.UserName
				//			,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
				//			,C.Id ,c.UserName,PPI.UserName ,PPI1.UserName  
				//			UNION ALL
				//                        SELECT E.UserName AS Entity 
				//			,isnull(II.IssueType,'') issuetype
				//			, II.Id, II.CompanyGroupId
				//			, II.CompanyId, II.PlantId
				//			, II.EntityId, II.MaterialStorageId
				//			,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
				//			, MS.UserName AS MaterialStorage 
				//			,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
				//			,SUM(IID.TransactionQty) Qty
				//			,SUM(IID.PolicyAmount) Amount
				//			,II.Remarks,II.Id AS IssueId
				//			,II.OrderRefNo
				//			,C.Id CountryId,c.UserName CountryName
				//                       ,PPI.UserName BillTo
				//			,PPI1.UserName ShipTo
				//			FROM[TRN].[InventorySales] AS II
				//			left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
				//			left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
				//			left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
				//			Left JOIN [ORG].[Entity] E On E.id= II.EntityId
				//			left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
				//			left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
				//			left JOIN SCS.Country c ON C.Id=IR.CountryId
				//                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
				//			LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
				//			WHERE II.PlantId= '" + plantId + @"' 
				//                        AND II.CheckedByStatus IS NULL
				//                        AND II.ApprovedByStatus ='For Approval'
				//			AND ISNULL(II.[Status],'') <>'Posting' 
				//			GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//			,II.SalesDate, MS.UserName
				//			,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
				//			,C.Id ,c.UserName,PPI.UserName ,PPI1.UserName)x  
				//			Order BY IssueDate DESC";
				//}
				//else if (tabType == "2")
				//{
				//	sql = @"SELECT E.UserName AS Entity 
				//			,isnull(II.IssueType,'') issuetype
				//			, II.Id, II.CompanyGroupId
				//			, II.CompanyId, II.PlantId
				//			, II.EntityId, II.MaterialStorageId
				//			,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
				//			, MS.UserName AS MaterialStorage 
				//			,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
				//			,SUM(IID.TransactionQty) Qty
				//			,SUM(IID.PolicyAmount) Amount
				//			,II.Remarks,II.Id AS IssueId
				//			,II.OrderRefNo
				//			,C.Id CountryId,c.UserName CountryName
				//                       ,PPI.UserName BillTo
				//			,PPI1.UserName ShipTo
				//			FROM[TRN].[InventorySales] AS II
				//			left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
				//			left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
				//			left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
				//			Left JOIN [ORG].[Entity] E On E.id= II.EntityId
				//			left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
				//			left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
				//			left JOIN SCS.Country c ON C.Id=IR.CountryId
				//                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
				//			LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
				//			WHERE II.PlantId= '" + plantId + @"' 
				//			AND (II.CheckedByStatus='Hold' OR II.CheckedByStatus='Reject')                           
				//			AND ISNULL(II.[Status],'') <>'Posting' 
				//			GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//			,II.SalesDate, MS.UserName
				//			,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
				//			,C.Id ,c.UserName,PPI.UserName ,PPI1.UserName  
				//			Order BY II.SalesDate DESC";
				//}
				//else if (tabType == "3")
				//{
				//	sql = @"SELECT E.UserName AS Entity 
				//			,isnull(II.IssueType,'') issuetype
				//			, II.Id, II.CompanyGroupId
				//			, II.CompanyId, II.PlantId
				//			, II.EntityId, II.MaterialStorageId
				//			,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
				//			, MS.UserName AS MaterialStorage 
				//			,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
				//			,SUM(IID.TransactionQty) Qty
				//			,SUM(IID.PolicyAmount) Amount
				//			,II.Remarks,II.Id AS IssueId
				//			,II.OrderRefNo
				//			,C.Id CountryId,c.UserName CountryName
				//                       ,PPI.UserName BillTo
				//			,PPI1.UserName ShipTo
				//			FROM[TRN].[InventorySales] AS II
				//			left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
				//			left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
				//			left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
				//			Left JOIN [ORG].[Entity] E On E.id= II.EntityId
				//			left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
				//			left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
				//			left JOIN SCS.Country c ON C.Id=IR.CountryId
				//                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
				//			LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
				//			WHERE II.PlantId= '" + plantId + @"' 
				//			AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='For Approval'    
				//			AND ISNULL(II.[Status],'') <>'Posting' 
				//			GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//			,II.SalesDate, MS.UserName
				//			,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
				//			,C.Id ,c.UserName,PPI.UserName ,PPI1.UserName  
				//			Order BY II.SalesDate DESC";
				//}
				//else if (tabType == "4")
				//{
				//	sql = @"SELECT E.UserName AS Entity 
				//			,isnull(II.IssueType,'') issuetype
				//			, II.Id, II.CompanyGroupId
				//			, II.CompanyId, II.PlantId
				//			, II.EntityId, II.MaterialStorageId
				//			,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
				//			, MS.UserName AS MaterialStorage 
				//			,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
				//			,SUM(IID.TransactionQty) Qty
				//			,SUM(IID.PolicyAmount) Amount
				//			,II.Remarks,II.Id AS IssueId
				//			,II.OrderRefNo
				//			,C.Id CountryId,c.UserName CountryName
				//                       ,PPI.UserName BillTo
				//			,PPI1.UserName ShipTo
				//			FROM[TRN].[InventorySales] AS II
				//			left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
				//			left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
				//			left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
				//			Left JOIN [ORG].[Entity] E On E.id= II.EntityId
				//			left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
				//			left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
				//			left JOIN SCS.Country c ON C.Id=IR.CountryId
				//                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
				//			LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
				//			WHERE II.PlantId= '" + plantId + @"' 
				//			AND II.CheckedByStatus='Checked' AND (II.ApprovedByStatus='Hold' OR II.ApprovedByStatus='Reject')
				//			AND ISNULL(II.[Status],'') <>'Posting' 
				//			GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//			,II.SalesDate, MS.UserName
				//			,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
				//			,C.Id ,c.UserName,PPI.UserName ,PPI1.UserName  
				//			Order BY II.SalesDate DESC";
				//}
				//else if (tabType == "5")
				//{
				//	sql = @"SELECT E.UserName AS Entity 
				//			,isnull(II.IssueType,'') issuetype
				//			, II.Id, II.CompanyGroupId
				//			, II.CompanyId, II.PlantId
				//			, II.EntityId, II.MaterialStorageId
				//			,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
				//			, MS.UserName AS MaterialStorage 
				//			,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
				//			,SUM(IID.TransactionQty) Qty
				//			,SUM(IID.PolicyAmount) Amount
				//			,II.Remarks,II.Id AS IssueId
				//			,II.OrderRefNo
				//			,C.Id CountryId,c.UserName CountryName
				//                       ,PPI.UserName BillTo
				//			,PPI1.UserName ShipTo
				//			FROM[TRN].[InventorySales] AS II
				//			left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
				//			left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
				//			left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
				//			Left JOIN [ORG].[Entity] E On E.id= II.EntityId
				//			left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
				//			left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
				//			left JOIN SCS.Country c ON C.Id=IR.CountryId
				//                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
				//			LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
				//			WHERE II.PlantId= '" + plantId + @"' 
				//			AND II.CheckedByStatus='Checked' AND (II.ApprovedByStatus='Hold' OR II.ApprovedByStatus='Reject')
				//			AND ISNULL(II.[Status],'') <>'Posting' 
				//			GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//			,II.SalesDate, MS.UserName
				//			,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
				//			,C.Id ,c.UserName,PPI.UserName ,PPI1.UserName  
				//			Order BY II.SalesDate DESC";
				//}
				//if (tabType == "6")
				//{
				//	sql = @"SELECT E.UserName AS Entity 
				//			,isnull(II.IssueType,'') issuetype
				//			, II.Id, II.CompanyGroupId
				//			, II.CompanyId, II.PlantId
				//			, II.EntityId, II.MaterialStorageId
				//			,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
				//			, MS.UserName AS MaterialStorage 
				//			,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
				//			,SUM(IID.TransactionQty) Qty
				//			,SUM(IID.PolicyAmount) Amount
				//			,II.Remarks,II.Id AS IssueId
				//			,II.OrderRefNo
				//			,C.Id CountryId,c.UserName CountryName
				//                       ,PPI.UserName BillTo
				//			,PPI1.UserName ShipTo
				//			FROM[TRN].[InventorySales] AS II
				//			left JOIN TRN.InventorySalesDetail AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
				//			left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
				//			left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
				//			Left JOIN [ORG].[Entity] E On E.id= II.EntityId
				//			left join trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId=IID.Id
				//			left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
				//			left JOIN SCS.Country c ON C.Id=IR.CountryId
				//                        LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
				//			LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId
				//			WHERE II.PlantId= '" + plantId + @"' 
				//			AND II.CheckedByStatus='Checked' AND (II.ApprovedByStatus='Hold' OR II.ApprovedByStatus='Reject')
				//			AND ISNULL(II.[Status],'') <>'Posting' 
				//			GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
				//			,II.SalesDate, MS.UserName
				//			,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
				//			,C.Id ,c.UserName,PPI.UserName ,PPI1.UserName  
				//			Order BY II.SalesDate DESC";
				//}
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		public IEnumerable<object> MaterialSalesDetails(string inveReveiveId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @"SELECT IR.Id IssueNo
							,IR.CompanyGroupId
							,IR.CompanyId
							,Plant.GSTIN 
							,null PODepended 
							,IR.Id PONumber  
							,IR.IssueRequestMasterId  
							,REPLACE(Convert(VARCHAR(11), IR.SalesDate, 106), ' ', '-') AS PODate
							,null BaseOnDueDate
							,NULL AS MatureDate
							,null InvoicingPartyPlantId		                       
							,null InvoicingPartyName
							,null InvoicePartyAddressMasterId
							,null InvoicingPartyGSTIN
							,null InvoicingByAddress
							,null DeliveryByAddress
							,null DeliveryPartya
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
							-- ,'' CurrencyId
							,IR.CurrencyId
							--,null AS CurrencyName
							,CUR.Code CurrencyName
							,null as ToCurrencyRate
							,null AS BaseCurrencyName
							,NULL PaymentTerm
							,MM.UserName Materials
							,MM.MaterialGroupMasterId
							,MGM.UserName MaterialGroupMaster
							,IOM.ArticleId
							,MMA.StandardName Article
							,FC.Id FirstCharId
							,FC.UserName FirstChar
							,IOM.FirstCharacteristicsValueId
							,FCV.UserName AS SKU1
							,IOM.SecondCharacteristicsValueId
							,SCV.UserName AS SKU2
							,IOM.ThirdCharacteristicsValueId
							,TCV.UserName AS SKU3
							,SC.Id SecondCharId
							,SC.UserName SecondChar
							,TC.Id ThirdCharId
							,TC.UserName ThirdChar
							--,ROUND(IRD.BaseQty, 2) Qty
							--,ROUND(IRD.PolicyRate, 2) TransactionRate
							--,ROUND((IRD.PolicyAmount), 2) AS TrnAmount
							,ROUND(ISH.Qty, 2) Qty	
							,ROUND(ISH.SalesRate, 2) TransactionRate
							,ROUND(ISH.TotalAmount,2) TrnAmount
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
							,TUoM.UserName AS UOM
							,IRD.Id InventoryReceiveDetailId
							,IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter,IRD.Comments,ROUND(ISH.Qty, 2) Qty,ISH.TotalAmount,ISH.TotalAmount/Qty SalesRate
							FROM TRN.InventorySales IR
						LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
						LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
						LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
						LEFT JOIN trn.InventorySalesDetail IRD ON IR.Id = IRD.InventorySalesId		
						--LEFT JOIN trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId = IRD.Id
                         --LEFT JOIN (select InventorySalesDetailId,ROUND(sum(Qty), 2) Qty,
								--ROUND(sum(SalesRate), 2) SalesRate,
								--ROUND((sum(TotalAmount)), 2) AS TotalAmount 
							--from  TRN.InventorySalesHistory 
							--group by InventorySalesDetailId
							--)  ISH ON ISH.InventorySalesDetailId=IRD.Id
						 				                                   
						 LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IRD.Id
			                                   
						LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
						left JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
						left JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
						left JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
						LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
						LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
						--WHERE IR.Id IS NULL";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> MaterialScrapDetails(string inveReveiveId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @" SELECT IR.Id IssueNo
							,IR.CompanyGroupId
							,IR.CompanyId
							,Plant.GSTIN 
							,null PODepended 
							,IR.Id PONumber  
							,IR.IssueRequestMasterId  
							,REPLACE(Convert(VARCHAR(11), IR.ScrapDate, 106), ' ', '-') AS PODate
							,null BaseOnDueDate
							,NULL AS MatureDate
							,null InvoicingPartyPlantId
		                       
									,null InvoicingPartyName
											,null InvoicePartyAddressMasterId
											,null InvoicingPartyGSTIN
											,null InvoicingByAddress
							,null DeliveryByAddress
							,null DeliveryPartya
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
							,null IsNonCreditable
							-- ,'' CurrencyId
							,IR.CurrencyId
							--,null AS CurrencyName
							,CUR.Code CurrencyName
							,null as ToCurrencyRate
							,null AS BaseCurrencyName
							,NULL PaymentTerm
							,MM.UserName Materials
							,MM.MaterialGroupMasterId
							,MGM.UserName MaterialGroupMaster
							,IOM.ArticleId
							,MMA.StandardName Article
							,FC.Id FirstCharId
							,FC.UserName FirstChar
							,IOM.FirstCharacteristicsValueId
							,FCV.UserName AS SKU1
							,IOM.SecondCharacteristicsValueId
							,SCV.UserName AS SKU2
							,IOM.ThirdCharacteristicsValueId
							,TCV.UserName AS SKU3
							,SC.Id SecondCharId
							,SC.UserName SecondChar
							,TC.Id ThirdCharId
							,TC.UserName ThirdChar
							--,ROUND(IRD.BaseQty, 2) Qty
							--,ROUND(IRD.PolicyRate, 2) TransactionRate
							--,ROUND((IRD.PolicyAmount), 2) AS TrnAmount
							,null BaseAmount
							,null AS BaseTaxAmount
							--,TaxAmount = (
							--	SELECT SUM(TaxAmount)
							--	FROM [TRN].[PurchaseOrderTax]
							--	WHERE InventoryReceiveDetailId = IRD.Id
							--	)
							--,ServiceTaxAmount = (
							--	SELECT SUM(TotalTaxAmount)
							--	FROM [TRN].[POService]
							--	WHERE InventoryReceiveId = IOM.Id
							--	)
							,null ChargesTranAmount
							,null CountryId
							--,IRD.BaseUOMId
							,TUoM.UserName AS UOM
							--,IRD.Id InventoryReceiveDetailId
							--,IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter,IRD.Comments,IIH.SalesRate,IIH.TotalAmount
                           ,IIH.Qty
						FROM TRN.InventoryScrap IR
						LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
						LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
						LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
						LEFT JOIN trn.InventoryScrapDetail IRD ON IR.Id = IRD.InventoryScrapId		
						LEFT JOIN trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId = IRD.Id	
						 				                                   
						LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
						left JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
						left JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
						left JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
						LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
						LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						--LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						--LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
						--WHERE IR.Id IS NULL";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
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
                          where  A.ActionStatus='InventorySalesCheckedBy'";//A.PlantId='" + identity.PlantId + "' AND
				}
				else if (CheckedBy == "false" && ApprovedBy == "true")
				{
					sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventorySalesApproveBy'";//A.PlantId='" + identity.PlantId + "' AND
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
                          where  A.ActionStatus='InventoryScrapCheckedBy'";//A.PlantId='" + identity.PlantId + "' AND
				}
				else if (CheckedBy == "false" && ApprovedBy == "true")
				{
					sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='InventoryScrapApproveBy'";//A.PlantId='" + identity.PlantId + "' AND
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
		public void InsertGraphInventorySales(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventorySales inventoryIssue, string IssueTypeStatus, string CheckedByStatusForNoti, string ApprovedByStatusForNoti, IEnumerable<InventorySalesTax> taxCategoryList, string productNewId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var historyIdSaved = "";
			var InventoryReceiveDetailId = "";
			var flag = false;
			bool FlagIsAsset = false;
			var MAterialGUID = "";
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
					var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
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
						inventoryIssue.CurrencyId = currencyId;
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
										BaseRate = SelectedGRN.TotalAmount / SelectedGRN.Qty,//Convert.ToDecimal(issue.BaseRate),
										TotalBaseAmount = SelectedGRN.TotalAmount,//Convert.ToDecimal(detailtrnAmount),
																				  //SalesRate = Convert.ToDecimal(issue.SalesRate),
																				  //TotalAmount = Convert.ToDecimal(Convert.ToDecimal(issue.TransactionQty) * Convert.ToDecimal(issue.SalesRate)), //Convert.ToDecimal(issue.TotalAmount),
																				  //Rate = SelectedGRN.TotalAmount / SelectedGRN.Qty,										
										IsCapitalize = false,
										IssueRequestDetailId = receiveDetailRow.IssueRequest,
										IssueReturnQty = 0,
										BooksCurrencyBaseAmount = Math.Round((inventoryIssue.ToCurrencyRate * SelectedGRN.TotalAmount), 2)
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
											BaseRate = SelectedGRN.TotalAmount / SelectedGRN.Qty,//Convert.ToInt32(issue.BaseRate),
											TotalBaseAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
																									 //SalesRate = Convert.ToDecimal(issue.SalesRate),
																									 //TotalAmount = Convert.ToDecimal(Convert.ToDecimal(issueQty) * Convert.ToDecimal(issue.SalesRate)),//Convert.ToDecimal(issue.TotalAmount),											
											IssueReturnQty = 0,
											IsCapitalize = false,
											IssueRequestDetailId = receiveDetailRow.IssueRequest,
											BooksCurrencyBaseAmount = Math.Round((inventoryIssue.ToCurrencyRate * SelectedGRN.TotalAmount), 2)
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
								decimal policyAmmount = 0;
								decimal detailtrnAmount = 0;
								decimal totalGRNQty = 0;
								/*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* TrnRate)*/
								/*Rate= Amount/Sum GRN Qty */

								foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
								{
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
								var issueDetail = new InventorySalesDetail
								{
									Id = MakePK(inventoryIssue.Id, currentId, 2),
									InventorySalesId = inventoryIssue.Id,
									IsAsset = FlagIsAsset,//false,
														  //InventoryIssue = inventoryIssue,
									InventoryMaterialId = invMaterialId,
									BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
									TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
									AvgRate = invMaterial.AvgRate,
									Policy = "N/A",
									//Policy = receiveDetailRow.Policy,

									TransactionQty = stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
									PolicyAmount = detailtrnAmount,
									PolicyRate = detailtrnAmount / totalGRNQty,
									BaseQty = totalGRNQty,
									AvgAmount = totalGRNQty * invMaterial.AvgRate,
									BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
									ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
									CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
									Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),

									//SalesRate = item.SalesRate, 
									//TotalSalesAmount = Math.Round((stockList.Sum(r => r.RequisitionQty) * item.SalesRate), 2), 
									//BooksCurrencyTransactionAmount = (inventoryIssue.ToCurrencyRate * Math.Round((issue.TransactionQty * item.SalesRate), 2)),
									ModelState = ModelState.Added






								};

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

										Qty = item.RequisitionQty,
										BaseRate = SelectedGRN.TotalAmount / item.RequisitionQty,//Convert.ToDecimal(item.BaseRate),
										TotalBaseAmount = SelectedGRN.TotalAmount,//Convert.ToDecimal(detailtrnAmount),
																				  //SalesRate = Convert.ToDecimal(item.SalesRate),
																				  //TotalAmount = Convert.ToDecimal(item.TotalAmount),									
																				  //Rate = Convert.ToDecimal(item.BaseRate),	
										IssueRequestDetailId = item.IssueRequest,
										IssueReturnQty = 0,
										BooksCurrencyBaseAmount = Math.Round((inventoryIssue.ToCurrencyRate * SelectedGRN.TotalAmount), 2)
									};
									//policyAmmount += history.Qty * history.BaseRate;

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

									if (taxCategoryList.IsNotNull())
									{
										var currentTaxId = _InventorySalesTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventorySalesTax] WHERE InventoryReceiveDetailId='{item.InventoryReceiveDetailId}'").First();
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
											}
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
								decimal policyAmmount = 0;

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
						Amount = Math.Round(Convert.ToDecimal(entity.TransactionAmount),2),
						TotalTaxAmount = Convert.ToDecimal(entity.TotalTaxAmount),
						BooksCurrencyTransactionAmount=Math.Round(Convert.ToDecimal(entity.ToCurrencyRate)* Convert.ToDecimal(entity.TransactionAmount),2),
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
							item.BooksCurrencyTaxAmount = Math.Round(Convert.ToDecimal(entity.ToCurrencyRate* item.TaxAmount),2);
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
								decimal policyAmmount = 0;
								decimal detailtrnAmount = 0;
								decimal totalGRNQty = 0;
								foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
								{

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
										detailtrnAmount += Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
										var newgrn = new InventoryTransferHistory
										{
											TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
											InventoryReceiveDetailId = item.InventoryReceiveDetailId
										};
										GRNCalculateList.Add(newgrn);
										totalGRNQty += Convert.ToDecimal(item.RequisitionQty);

									}
									else
									{
										detailtrnAmount += Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
										var newgrn = new InventoryTransferHistory
										{
											TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty + item.InventoryTransferQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
											InventoryReceiveDetailId = item.InventoryReceiveDetailId
										};
										GRNCalculateList.Add(newgrn);
										totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
									}
									//}

									currentId++;
									var NewId = inventoryReceive.Id + "-";
									currentId1++;
									grndId = NewId + currentId1;
									var recvDetail = new InventoryReceiveDetail
									{
										Id = NewId + currentId1,
										MaterialStorageId = inventoryReceive.MaterialStorageId,
										InventoryReceiveId = inventoryReceive.Id,
										TransactionQty = stockList.Sum(r => r.RequisitionQty),
										TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
										BaseQty = stockList.Sum(r => r.RequisitionQty),
										BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
										BaseUoMFactor = Convert.ToDecimal(entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUoMFactor).FirstOrDefault()),
										//Convert.ToDecimal(itemDetail.BaseUoMFactor),									
										MaterialTranRate = Math.Round(detailtrnAmount / totalGRNQty,4),
										MaterialTranAmount = Math.Round(Convert.ToDecimal(detailtrnAmount),2),
										TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(detailtrnAmount),2),
										TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(detailtrnAmount),2),
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
										TrnCurrencyBaseRate = Math.Round(detailtrnAmount / totalGRNQty,4),
										BooksCurrencyBaseRate = Math.Round(detailtrnAmount / totalGRNQty,4),
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


									var historyId = _InventoryTransferHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryTransferHistory] WHERE InventoryReceiveDetailId='{recvDetail.Id}'").First();
									foreach (var item1 in stockList)
									{

										if (item1.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

										if (item1.TransactionUoMId != item1.BaseUOMId)
											totalReqQty = Convert.ToInt32(item1.RequisitionQty * item1.BaseUoMFactor);
										else
											totalReqQty = item1.RequisitionQty;
										historyId++;
										var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item1.InventoryReceiveDetailId).FirstOrDefault();
										var history = new InventoryTransferHistory
										{
											Id = MakePK(recvDetail.Id, historyId, 2),
											InventoryReceiveDetailId = item1.InventoryReceiveDetailId,
											Qty = item1.RequisitionQty,
											Rate =Math.Round(SelectedGRN.TotalAmount / item.RequisitionQty,4),
											TotalAmount = Math.Round(SelectedGRN.TotalAmount,2),
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
										builderSql1 = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - recvDetail.TransactionQty) + "' WHERE Id='" + invMaterialId + "'";
										rdBuilder1.Append(builderSql1);
										//item.TotalQty = Convert.ToDecimal(invMaterial.TotalQty - recvDetail.TransactionQty);
										item.CompanyGroupId = ToPlant.Rows[0]["CompanyGroupId"].ToString();
										item.CompanyId = ToPlant.Rows[0]["CompanyId"].ToString();
										item.PlantId = ToPlant.Rows[0]["PlantId"].ToString();
										item.TotalQty = recvDetail.TransactionQty;
										item.AvgRate =Math.Round(recvDetail.TotalMaterialBooksCurrencyAmount / recvDetail.TransactionQty,4);
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

									AuditService.AddedLog(recvDetail);
									recvDetail.InventoryMaterialId = item.InventoryMaterialId;
									_receiveDetailRepository.Insert(recvDetail);
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
				var builderSql11 = "";
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

		public GridModel Querywithoutpo(GridParameter parameters, string inveReveiveId)
		{
			try
			{
				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
				                             , @totalReceiveAmount DECIMAL(18, 4)=0
				                             , @totalServiceAmount DECIMAL(18, 4)=0
				                             , @totalSvcTaxAmount DECIMAL(18, 4)=0
				                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
				                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
				                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
				                  SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
				                      , MGM.UserName AS MaterialGroupMasterName
				                      , IM.MaterialMasterId, MM.UserName MaterialMasterName
				                      , IM.ArticleId, ART.StandardName ArticleName

				                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristicText
				                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue

				                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristicText
				                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue

				                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristicText
				                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue

				                      , IRD.TransactionQty
				                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
				                      , IRD.MaterialTranRate AS TransactionRate
				                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
				                       , (IRD.MaterialTranAmount) AS TrnAmount
				                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                         
				                      , IRD.TotalTaxAmount AS BaseTaxAmount
				                   , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
				                   , IRD.ChargesTranAmount AS ChargesAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                               , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   , IRD.CountryId
				                    , IRD.TotalMaterialTranAmount AS TotalMaterialTranAmount  
                                    ,null TaxList
                                    ,IRD.InventoryReceiveId,IRD.BaseUOMId,IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,isnull(IRD.ShortageQty,0) ShortageQty,isnull(IRD.RejectionQty,0) RejectionQty,isnull(IRD.ApprovedQty,0) ApprovedQty
                                    ,(IRD.TransactionQty-isnull(IRD.ShortageQty,0)) AS NetQty
                                    ,IRD.BaseQty
									,IRD.BaseUoMFactor,IRD.Description
                                    ,null DataChangeFlag,IRD.ShortRejFlag
                                   ,IRD.ShortageRatePercent ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent RejectionRate,IRD.RejectValue RejectionValue,IRD.RejectClamPercent RejectionClamRate
                                   ,C.Id CountryId,C.UserName CountryName,IRD.LotNumber,IRD.Diameter,IRD.Type
				                  FROM TRN.InventoryMaterial AS IM
				                  JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
				                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
				                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
				                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
				                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
				                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
				                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
				                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
				                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
				                  JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
				                  JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
				                  JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
				                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                  left Join [SCS].[Country] AS C ON C.Id=IM.CountryId
				                  WHERE IRD.InventoryReceiveId=@inventoryReceiveId And Im.MaterialMasterId is not null --Order BY IRD.Id ASC

                                  UNION ALL

								SELECT  IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
				                      , '' AS MaterialGroupMasterName
				                      , '' MaterialMasterId
									  ,'' UserName
				                      , '' ArticleId
									  , '' StandardName
				                      , IM.FirstCharacteristicsId, '' AS FirstCharacteristics
				                      , IM.FirstCharacteristicsValueId, '' AS FirstCharacteristicsValue
				                      , IM.SecondCharacteristicsId, '' AS SecondCharacteristics
				                      , IM.SecondCharacteristicsValueId, '' AS SecondCharacteristicsValue
				                      , IM.ThirdCharacteristicsId, '' AS ThirdCharacteristics
				                      , IM.ThirdCharacteristicsValueId, '' AS ThirdCharacteristicsValue
				                      , IRD.TransactionQty
				                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
				                      , IRD.MaterialTranRate AS TransactionRate
				                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
				                       , (IRD.MaterialTranAmount) AS TrnAmount
				                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                         
				                      , IRD.TotalTaxAmount AS BaseTaxAmount
				                   , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
				                   , IRD.ChargesTranAmount AS ChargesAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                               , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   , IRD.CountryId
				                    , IRD.TotalMaterialTranAmount AS TotalMaterialTranAmount  
                                    ,null TaxList
                                    ,IRD.InventoryReceiveId,IRD.BaseUOMId,IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,isnull(IRD.ShortageQty,0) ShortageQty,isnull(IRD.RejectionQty,0) RejectionQty,isnull(IRD.ApprovedQty,0) ApprovedQty
                                    ,(IRD.TransactionQty-isnull(IRD.ShortageQty,0)) AS NetQty
                                    ,IRD.BaseQty
									,IRD.BaseUoMFactor,IRD.Description
                                    ,null DataChangeFlag,IRD.ShortRejFlag
                                    ,IRD.ShortageRatePercent ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent RejectionRate,IRD.RejectValue RejectionValue,IRD.RejectClamPercent RejectionClamRate
                                    ,C.Id CountryId,C.UserName CountryName,IRD.LotNumber,IRD.Diameter,IRD.Type
				                  FROM TRN.InventoryMaterial AS IM
				                  --JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
				                  --LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
				                  --LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
				                  --LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
				                  --LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
				                  --LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
				                  --LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
				                  --LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
				                  --LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
				                  JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
				                  JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
				                  JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
				                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                  left Join [SCS].[Country] AS C ON C.Id=IM.CountryId
				                  WHERE IRD.InventoryReceiveId=@inventoryReceiveId And Im.MaterialMasterId is null Order BY IRD.Id ASC";

				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public void JWInsertGraph(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus)
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
					var inventoryMaterialList = _inventoryMaterialService.GetJWInventoryMaterialListByUpToSku(entities, inventoryIssue.CompanyId, inventoryIssue.PlantId);
					var currencyId = _companyRepository.Find(inventoryIssue.CompanyId).BaseCurrencyId;
					foreach (var item in entities)// update view model (inventory material field)
					{
						var im = inventoryMaterialList.FirstOrDefault(t => t.MaterialMasterId == item.MaterialMasterId && t.ArticleId == item.ArticleId
								//&& t.FirstCharacteristicsId == item.FirstCharacteristicsId 
								//&& t.FirstCharacteristicsValueId == item.FirstCharacteristicsValueId
								//&& t.SecondCharacteristicsId == item.SecondCharacteristicsId 
								//&& t.SecondCharacteristicsValueId == item.SecondCharacteristicsValueId
								//&& t.ThirdCharacteristicsId == item.ThirdCharacteristicsId 
								//&& t.ThirdCharacteristicsValueId == item.ThirdCharacteristicsValueId
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
								if (receiveDetailList1.Count>0)
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
								var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
								var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
								var totalReqQty = 0M;
								decimal policyAmmount = 0;

								decimal detailtrnAmount = 0;
								decimal totalGRNQty = 0;
								/*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* TrnRate)*/
								/*Rate= Amount/Sum GRN Qty */
								var IssueRequestDetailIdnew = "";
								foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
								{
									//decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
									//decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(IIH.TotalAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(ISH.TotalBaseAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
									//																						   FROM trn.InventoryReceiveDetail IRD  
									//																							left JOIN [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
									//																							LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
									//																							LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
									//																							LEFT join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
									//																							LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
									//																							LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
									//																							WHERE  IRD.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
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

									TransactionQty = stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
									PolicyAmount = Math.Round(detailtrnAmount, 2),
									PolicyRate = Math.Round((detailtrnAmount / totalGRNQty), 4),
									BaseQty = totalGRNQty,
									AvgAmount = Math.Round((totalGRNQty * invMaterial.AvgRate), 2),
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

									if (item.TransactionUoMId != item.BaseUOMId)
										totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
									else
										totalReqQty = item.RequisitionQty;
									historyId++;
									var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
									var history = new InventoryIssueHistory
									{
										Id = MakePK(issueDetail.Id, historyId, 2),
										InventoryIssueDetailId = issueDetail.Id,
										InventoryReceiveDetailId = item.InventoryReceiveDetailId,
										Qty = item.RequisitionQty,
										//Rate = Convert.ToDecimal(item.BaseRate),
										Rate = Math.Round((SelectedGRN.TotalAmount / item.RequisitionQty), 4),
										TotalAmount = Math.Round(SelectedGRN.TotalAmount, 2),//Convert.ToDecimal(detailtrnAmount),
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



									//Mapping Data=========================================================
									var receiveDetailList1 = _sqlRepository.GetModelCollection<IssueRequestViewModel>(@"select IRBM.Id,IRBM.IssueRequestDetailId,IRBM.BOQID,Isnull(IRBM.Qty,0) IssueRequestBOQMapQty,Isnull(IDRM.Qty,0) AllocatedIssueSlipQty
															from [TRN].[IssueRequestBOQMap] IRBM
															Left Join (Select IssueRequestBOQMapId, sum(Qty) Qty from [TRN].[IssueDetailAndIssueRequestMap]  group by IssueRequestBOQMapId) IDRM ON IDRM.IssueRequestBOQMapId=IRBM.BOQID
															where IssueRequestDetailId='" + item.IssueRequest + @"' Order By IRBM.Qty ASC").ToList();
									if (receiveDetailList1.Count>0)
									{
										bool isQtyAlocated = true;
										decimal temp = 0;
										int count = 0;
										foreach (var receiveDetailListNew in receiveDetailList1)
										{


											count++;
											if (count == 1)
											{
												if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) > issueDetail.TransactionQty)
												{

													issueDetail.TransactionQty = issueDetail.TransactionQty;
													//temp += itemDetail.TransactionQty;
													isQtyAlocated = false;

												}
												else if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < issueDetail.TransactionQty)
												{
													//temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
													temp = receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty;
													issueDetail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
													isQtyAlocated = true;

												}
												else
												{
													//temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
													issueDetail.TransactionQty = issueDetail.TransactionQty;
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
														issueDetail.TransactionQty = issueDetail.TransactionQty;
														isQtyAlocated = false;
													}
													if ((receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty) < temp)
													{
														//temp = temp - issue.TransactionQtyForPO;
														temp = (temp - (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty));
														//itemDetail.TransactionQty = issue.TransactionQtyForPO;
														issueDetail.TransactionQty = (receiveDetailListNew.IssueRequestBOQMapQty - receiveDetailListNew.AllocatedIssueSlipQty);
														isQtyAlocated = true;
													}
													else
													{
														//temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
														issueDetail.TransactionQty = temp;
														isQtyAlocated = true;

													}

												}
												else
												{
													issueDetail.TransactionQty = 0;
												}
											}


											var IssueDetailAndIssueRequestMapNew = new IssueDetailAndIssueRequestMap
											{
												Id = GetIssueDetailAndIssueRequestMapPK(),
												InventoryIssueDetailId = issueDetail.Id,
												IssueRequestBOQMapId = receiveDetailListNew.Id,
												Qty = issueDetail.TransactionQty
												//AutoAllocate = true

											};
											AuditService.AddedLog(IssueDetailAndIssueRequestMapNew);
											_IssueDetailAndIssueRequestMapRepository.Insert(IssueDetailAndIssueRequestMapNew);
										}
									}


								}


								builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
								rdBuilder.Append(builderSql);

								AuditService.AddedLog(issueDetail);
								_issueDetailService.InsertGraph(issueDetail);


								//===================

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



	}
}//end
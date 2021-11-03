using System;
using System.Collections.Generic;

namespace Library.ViewModel.SalesManagements
{
    public class InventorySalesReturnDetailViewModel
    {
        public string Id { get; set; }
        public string DocRefNo { get; set; }
        public string DocDate { get; set; }
        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        public string EntityId { get; set; }
        public string MaterialStorageId { get; set; }
        public string OpeningBalanceId { get; set; }
        public string MaterialMasterId { get; set; }
        public string MaterialMasterName { get; set; }
        public string ArticleId { get; set; }
        public string FirstCharacteristicsId { get; set; }
        public string FirstCharacteristicsValueId { get; set; }
        public string SecondCharacteristicsId { get; set; }
        public string SecondCharacteristicsValueId { get; set; }
        public string ThirdCharacteristicsId { get; set; }
        public string ThirdCharacteristicsValueId { get; set; }
        public decimal TotalQty { get; set; }
        public decimal AvgRate { get; set; }
        public decimal RequisitionQty { get; set; }
        public decimal IssueQty { get; set; }
        public decimal BaseIssueQty { get; set; }
        public decimal TransactionQty { get; set; }
        public decimal TempReturnQty { get; set; }
        public decimal StockQty { get; set; }
        public string TransactionUoMId { get; set; }
        public decimal? BaseQty { get; set; }
        public string BaseUOMId { get; set; }
        public decimal? BaseUoMFactor { get; set; }
        public decimal? TransactionRate { get; set; }
        public decimal? BaseRate { get; set; }
        public decimal? ToCurrencyRate { get; set; }
        public decimal? WithInvoiceRate { get; set; }
        public decimal? AfterInvoiceRate { get; set; }
        public decimal? TransactionAmount { get; set; }
        public decimal? BaseAmount { get; set; }
        public string InventorySalesDetailId { get; set; }
        public string InventorySalesId { get; set; }
        public string InventoryReceiveId { get; set; }
        public string InventoryReceiveDetailId { get; set; }
        public string InventoryMaterialId { get; set; }
        public string InventoryIssueId { get; set; }
        public string InventoryIssueDetailId { get; set; }
        public string InventoryIssueHistoryId { get; set; }
        public decimal? AvgAmount { get; set; }
        public decimal? PolicyRate { get; set; }
        public decimal? PolicyAmount { get; set; }
        public string Policy { get; set; }
        public DateTime? IssueDate { get; set; }
        public decimal? TotalTaxAmount { get; set; }
        public string CurrencyId { get; set; }
        public string BaseCurrencyId { get; set; }
        public string CountryId { get; set; }
        public string VendorArticulationId { get; set; }
        public string InventoryServiceId { get; set; }
        public string ServiceMasterId { get; set; }
        public bool IsNonCreditable { get; set; }
        public bool IsTaxApplicableChangeable { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPaymentHold { get; set; }
        public bool QtyStatus { get; set; }
        public bool check { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal GRNRcvQty { get; set; }
        public decimal TaxAmount { get; set; }
        public string UOMId { get; set; }
        public string POID { get; set; }
        public string PODetailsID { get; set; }
        public decimal BaseTaxAmount { get; set; }
        public string POServiceId { get; set; }
        public decimal? TrnAmount { get; set; }
        public decimal? ChargesAmount { get; set; }
        public decimal? MaterialTranRate { get; set; }
        public decimal? MaterialTranAmount { get; set; }
        public decimal? TotalMaterialTranAmount { get; set; }
        public decimal? TotalMaterialBooksCurrencyAmount { get; set; }
        public decimal? ChargesTranAmount { get; set; }
        public decimal? ChargesTaxTranAmount { get; set; }
        public decimal? TrnCurrencyBaseRate { get; set; }
        public decimal? BooksCurrencyBaseRate { get; set; }
        public decimal? ServiceCharge { get; set; }
        public decimal? ServiceTax { get; set; }
        public string MasterOrderId { get; set; }
        public string MasterOrderDetailId { get; set; }

        public string Description { get; set; }
        public DateTime? DeliveryDate { get; set; }

        public string RequisitionId { get; set; }
        public string RequisitionDetailId { get; set; }
        public decimal RequisitionRcvQty { get; set; }
        public bool RequisitionQtyStatus { get; set; }
        public decimal ShortageQty { get; set; }
        public decimal RejectionQty { get; set; }
        public decimal ApprovedQty { get; set; }
        public decimal PreviousQty { get; set; }
        public decimal PreviousShortQty { get; set; }
        public decimal PreviousRejectionQty { get; set; }
        public decimal PreviousApprovedQty { get; set; }
        public decimal NetQty { get; set; }
        public string HSNCodeId { get; set; }

        public string GRNID { get; set; }
        public string POReqDetailsID { get; set; }
        public decimal Qty { get; set; }


        public decimal ShortageRate { get; set; }
        public decimal ShortageValue { get; set; }
        public decimal RejectionRate { get; set; }
        public decimal RejectionValue { get; set; }
        public decimal RejectionClamRate { get; set; }
        public string IssueDetailId { get; set; }
        public string IssueMasterId { get; set; }
        public string FixedAssetMasterId { get; set; }
        public bool ShortRejFlag { get; set; }
        public string Remarks { get; set; }
        /// <summary>
        /// Used in case of inventory issue. for tracking employee.
        /// </summary>
        public string EmployeeId { get; set; }
        public string GLGeneralInfoId { get; set; }
        public string BudgetMasterId { get; set; }

        public string ActivityId { get; set; }

        public string PostDrGLGeneralInfoId { get; set; }

        public string PostDrBudgetMasterId { get; set; }

        public string PostDrActivityId { get; set; }

        public string PostCrGLGeneralInfoId { get; set; }

        public string PostCrBudgetMasterId { get; set; }

        public string PostCrActivityId { get; set; }
        public string PurchaseDocumentAcceptanceId { get; set; }
        public string PurchaseDocumentAcceptanceDetailId { get; set; }
        public string CostCenterId { get; set; }
        public string NoteForAccounts { get; set; }

        public decimal AcceptanceRcvQty { get; set; }
        public bool AcceptanceRcvStatusQty { get; set; }
        public string IssueRequest { get; set; }
        public bool POQtyStatus { get; set; }

        public bool WantToClose { get; set; }
        public string AccessQtyReason { get; set; }
        public bool POClosStatus { get; set; }
        public string IssueRequestDetailId { get; set; }

        public string IssueREturnHistoryId { get; set; }


        public decimal oldReturnQty { get; set; }
        public bool Active { get; set; }
        public decimal ReductionByAdjustmentQty { get; set; }

        public decimal IssueReturnQty { get; set; }
        public decimal PurchaseReturnQty { get; set; }


        public string ArticleName { get; set; }

        public bool IsSpecific { get; set; }
        public string Comments { get; set; }
        public string CheckedBy { get; set; }

        public string CheckedByStatus { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByStatus { get; set; }
        public string AuthorizedBy { get; set; }
        public string AuthorizedByStatus { get; set; }
        public decimal InventorySalesQty { get; set; }
        public decimal InventoryScrapQty { get; set; }
        public string PartyId { get; set; }
        public decimal SalesRate { get; set; }
        public decimal TotalAmount { get; set; }

        public string LotNumber { get; set; }
        public string Diameter { get; set; }
        public string Type { get; set; }


        public string RefferenceNo { get; set; }
        public decimal InventoryTransferQty { get; set; }

        public string FromMaterialStorageId { get; set; }
        public decimal GRNQty { get; set; }
        public decimal GRNTotalAmount { get; set; }
        public bool IsAsset { get; set; }


        public string BOQId { get; set; }
        public string MasterOrderItemId { get; set; }
        public string SalesOrderId { get; set; }

        public decimal BaseQtyNew { get; set; }
        public decimal POBOQQty { get; set; }
        public string POUoMId { get; set; }
        public string LotNo { get; set; }
        public string QualityStatus { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal RejectBaseQty { get; set; }
        public decimal RejectQty { get; set; }
        public string POBOQMapId { get; set; }
        public decimal Tolerance { get; set; }

        public string SavedPOBOQId { get; set; }
        public decimal TransactionQtyForPO { get; set; }
        public decimal POQty { get; set; }
        public decimal AllocatedSOQty { get; set; }

        public ICollection<SalesReturnTaxViewModel> TaxList { get; set; }

    }
}
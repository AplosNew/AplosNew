using System;
using System.Collections.Generic;

namespace Library.ViewModel.SalesManagements
{
    public class SalesMaterialViewModel
    {
        public string Id { get; set; }
        public string DocRefNo { get; set; }
        public string DocDate { get; set; }
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
        public int RequisitionQty { get; set; }
        public int IssueQty { get; set; }
        public decimal TransactionQty { get; set; }
        public int StockQty { get; set; }
        public int BaseQty { get; set; }
        public decimal BaseUoMFactor { get; set; }
        public decimal TransactionRate { get; set; }
        public decimal BaseRate { get; set; }
        public decimal? ToCurrencyRate { get; set; }
        public decimal? WithInvoiceRate { get; set; }
        public decimal? AfterInvoiceRate { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal SalesQty { get; set; }
        public decimal BaseAmount { get; set; }
        public string InventoryReceiveId { get; set; }
        public string InventoryReceiveDetailId { get; set; }
        public string InventoryMaterialId { get; set; }
        public string InventoryIssueId { get; set; }
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
        public string SalesMasterId { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string BaseUOMId { get; set; }
        public string TransactionUoMId { get; set; }
        public string SalesOrderId { get; set; }

        public string GLGeneralInfoId { get; set; }
        public string GLGeneralInfoCode { get; set; }
        public string GLGeneralInfoName { get; set; }

        public string BudgetMasterId { get; set; }
        public string BudgetId { get; set; }
        public string BudgetCode { get; set; }
        public string BudgetName { get; set; }

        public string ActivityId { get; set; }
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
        public string TrnType { get; set; }
        public string OtherName { get; set; }
        public decimal Amount { get; set; }
        public string VoucherId { get; set; }
        public string VoucherDetailId { get; set; }
        public string TaxCategoryId { get; set; }
        public string TaxCodeId { get; set; }

        public decimal BooksCurrencyTransactionAmount { get; set; }
        public decimal BooksCurrencyBaseRate { get; set; }
        public decimal BooksCurrencyTaxAmount { get; set; }
        public string GoodsDescription { get; set; }
        public string PackingId { get; set; }

        public ICollection<SalesTaxViewModel> TaxList { get; set; }

        #region SalesMaterialSO
        public string SalesId { get; set; }

        public string SalesMaterialId { get; set; }

        //public string MasterOrderId { get; set; }

        //public string MasterOrderItemId { get; set; }

        //
        public decimal ExistSalesQty { get; set; }
        public decimal TempSalesQty { get; set; }
        #endregion
    }
}
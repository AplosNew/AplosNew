using Library.Core;
using System;

namespace Library.ViewModel.OrderManagements
{
    public class POMaterialViewModel : BaseModel
    {
        //public InventoryMaterialViewModel();

        public decimal? WithInvoiceRate { get; set; }
        public decimal? AfterInvoiceRate { get; set; }
        public decimal? TransactionAmount { get; set; }
        public decimal? BaseAmount { get; set; }
        public string InventoryReceiveId { get; set; }
        public string InventoryReceiveDetailId { get; set; }
        public string InventoryMaterialId { get; set; }
        public string InventoryIssueId { get; set; }
        public decimal? AvgAmount { get; set; }
        public decimal? PolicyRate { get; set; }
        public decimal? PolicyAmount { get; set; }
        public string Policy { get; set; }
        public decimal? ToCurrencyRate { get; set; }
        public DateTime? IssueDate { get; set; }
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
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal? TotalTaxAmount { get; set; }
        public decimal? BaseRate { get; set; }
        public decimal? TransactionRate { get; set; }
        public decimal? BaseUoMFactor { get; set; }
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
        public decimal TransactionQty { get; set; }
        public decimal StockQty { get; set; }
        public string TransactionUoMId { get; set; }
        public decimal? BaseQty { get; set; }
        public string BaseUOMId { get; set; }
        public decimal TaxAmount { get; set; }
        public string UOMId { get; set; }       
        public string BaseTaxAmount { get; set; }
        public string Description { get; set; }
        public decimal? TrnAmount { get; set; }
        public string RefferenceNo { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal? Tolerance { get; set; } 
        

    }
}
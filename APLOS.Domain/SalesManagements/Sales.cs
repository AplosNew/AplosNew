using Library.Core;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.SalesManagements
{
    public class Sales : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string PartyType { get; set; }
        public string DocRefNo { get; set; }
       // public DateTime DocDate { get; set; }
        public DateTime EntryDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? BaseOnDueDate { get; set; }
        public int BaseNoOfDays { get; set; }
        public DateTime? MatureDate { get; set; }
        public string RowState { get; set; }
        public decimal ToCurrencyRate { get; set; }
        public string Narration { get; set; }
        public string InvoicingByAddress { get; set; }
        public string DeliveryByAddress { get; set; }
        public string ComercialInvoiceNo { get; set; }
        public string BLNumber { get; set; }
        public string ItemDescription { get; set; }
        public string EXPFromNo { get; set; }
        public DateTime? BLDate { get; set; }
        public DateTime? EXPDate { get; set; }
        public string SourceType { get; set; }
        public bool IsAdditionalInfoApplicable { get; set; }
        public bool IsIncentiveApplicable { get; set; }
        public string InvoiceStatus { get; set; }
        public string PaymentToReceiveBankId { get; set; }
        public string AdditionalFrieght { get; set; }
        public string Incoterms { get; set; }
        public decimal AdditionalFrieghtValue { get; set; }
        public decimal IncotermsValue { get; set; }
       
        public string CancelStatus { get; set; }
        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }

        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        [NeverUpdate]
        public string AddedFromIP { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string CancelBy { get; set; }

        public DateTime? CancelDate { get; set; }

        #endregion Audit Properties

        #region Navigation Properties

        [NeverUpdate, XmlIgnore]
        public string CompanyGroupId { get; set; }

        [NeverUpdate]
        public string CompanyId { get; set; }

        [NeverUpdate]
        public string PlantId { get; set; }

        public string EntityId { get; set; }
        public string CurrencyId { get; set; }

        public string PartyId { get; set; }

        public string PaymentTermId { get; set; }

        public string InvoicingPartyPlantId { get; set; }

        public string DeliveryPartyPlantId { get; set; }
        public Voucher Voucher { get; set; }

        public string VoucherId { get; set; }

        #endregion Navigation Properties
    }
}
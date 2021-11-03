using Library.Core;
using Library.Model.Invoices;
using Library.Model.Vouchers;
using System;

namespace Library.Model.Accounts
{
    public class OtherInvoice : BaseModel
    {

        #region Scalar Properties

        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string SourceType { get; set; }
        public bool IsPark { get; set; }

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

        #endregion Audit Properties

        #region Navigation Properties

        public virtual Invoice Invoice { get; set; }
        public string InvoiceId { get; set; }
      
        public Voucher Voucher { get; set; }
        public string VoucherId { get; set; }
       
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }
        public string GLGeneralInfoId { get; set; }
        public string BudgetMasterId { get; set; }
        public string ActivityId { get; set; }
      
       
        #endregion Navigation Properties

    }
}
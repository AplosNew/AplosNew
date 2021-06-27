using Library.Core;
using Library.Model.Vouchers;
using System;

namespace Library.Model.SalesManagements
{
    public class SalesService : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal BooksCurrencyTransactionAmount { get; set; }
        public decimal BooksCurrencyTaxAmount { get; set; }
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

        public Sales Sales { get; set; }
        public string SalesId { get; set; }
        public string ServiceMasterId { get; set; }
        public VoucherDetail VoucherDetail { get; set; }

        public string VoucherDetailId { get; set; }

        #endregion Navigation Properties
    }
}
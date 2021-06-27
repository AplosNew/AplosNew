using Library.Core;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Model.Vouchers;
using System;

namespace Library.Model.Commercial
{
    public class LoanAgainstAcceptance : BaseModel
    {

        #region Scalar Properties

        public string Id { get; set; }
        public string PartyType { get; set; }

        public string PaymentSource { get; set; }
        public DateTime LoanDate { get; set; }
        public string LoanNo { get; set; }
        public decimal Amount { get; set; }

        public bool IsPark { get; set; }
        public string TransactionType { get; set; }

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

        [NeverUpdate]
        public string CompanyGroupId { get; set; }

        [NeverUpdate]
        public string CompanyId { get; set; }

        [NeverUpdate]
        public string PlantId { get; set; }

        public string EntityId { get; set; }
        public string VoucherTypeId { get; set; }
        public string CurrencyId { get; set; }
        public Voucher Voucher { get; set; }
        public string VoucherId { get; set; }
        public virtual Party Party { get; set; }
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }
        public PurchaseDocAcceptance PurchaseDocAcceptance { get; set; }
        public string PurchaseDocAcceptanceId { get; set; }
        public string BankMasterId { get; set; }

        #endregion Navigation Properties

    }
}
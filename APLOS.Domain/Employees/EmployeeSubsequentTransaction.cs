using Library.Core;
using Library.Model.Advances;
using Library.Model.Vouchers;
using System;

namespace Library.Model.Employees
{
    public class EmployeeSubsequentTransaction : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string PartyType { get; set; }

        public string SourceType { get; set; }

        public string PaymentSource { get; set; }
        public DateTime VoucherDate { get; set; }
        public DateTime PostingDate { get; set; }
        public DateTime DocDate { get; set; }
        public string DocRefNo { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }

        public bool IsPark { get; set; }
        public string JournalType { get; set; }
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
        public string EmployeeTransactionTypeId { get; set; }
        public string EmployeeSalaryAdvanceId { get; set; }
       

        // public  EmployeeInformation Employee { get; set; }
        public string EmployeeId { get; set; }
        public Advance Advance { get; set; }
        public string AdvanceId { get; set; }
        public VoucherDetail VoucherDetail { get; set; }

        public string VoucherDetailId { get; set; }
        public AdvanceWriteOff AdvanceWriteOff { get; set; }
        public string AdvanceWriteOffId { get; set; }
        public EmployeePayable EmployeePayable { get; set; }
        public string EmployeePayableId { get; set; }
        public EmployeePayableWriteOff EmployeePayableWriteOff { get; set; }
        public string EmployeePayableWriteOffId { get; set; }
        public string EmployeeAdvanceDetailId { get; set; }

        #endregion Navigation Properties
    }
}
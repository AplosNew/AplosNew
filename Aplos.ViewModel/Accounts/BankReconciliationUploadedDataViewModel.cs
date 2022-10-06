using System;

namespace Library.ViewModel.Accounts
{
    public class BankReconciliationUploadedDataViewModel
    {
        public string Id { get; set; }
        public string BankReconciliationUploadId { get; set; }
        public string BankStatementDate { get; set; }
        public string BankRefNo { get; set; }
        public string BankParticulars { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public string OwnRefNo { get; set; }
        public string Remarks { get; set; }

        public string BankReconciliationUploadedDataId { get; set; }
        public string VoucherDetailId { get; set; }
        public string GLTransactionDetailId { get; set; }



    }
}
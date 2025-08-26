using Library.Core;
using Library.Model.Employees;
using Library.Model.Finances;
using Library.Model.Inventory;
using Library.Model.Invoices;
using Library.Model.Taxations;
using Library.Model.Vouchers;
using System;

namespace Library.Model.Accounts
{
    public class AdditionalTax : BaseModel
    {

        #region Scalar Properties

        public string Id { get; set; }
        public bool Archive { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxAutoAmount { get; set; }
        public string SourceType { get; set; }
        public decimal WrittenOffAmount { get; set; }
        public bool IsWrittenOff { get; set; }

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
        public virtual InvoiceWriteOff InvoiceWriteOff { get; set; }
        public string InvoiceWriteOffId { get; set; }
        public virtual EmployeePayable EmployeePayable { get; set; }
        public string EmployeePayableId { get; set; }

        public virtual ServiceAcknowledgementMaster ServiceAcknowledgementMaster { get; set; }
        public string ServiceAcknowledgementMasterId { get; set; }

        public Voucher Voucher { get; set; }
        public string VoucherId { get; set; }
     
        public string TaxYearId { get; set; }
        public string TaxYearPeriodId { get; set; }
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }
        public InventoryReceive InventoryReceive { get; set; }
        public string InventoryReceiveId { get; set; }
        public AdjustmentNote AdjustmentNote { get; set; }
        public string AdjustmentNoteId { get; set; }

        public FinancingWriteOff FinancingWriteOff { get; set; }
        public string FinancingWriteOffId { get; set; }

        #endregion Navigation Properties

    }
}
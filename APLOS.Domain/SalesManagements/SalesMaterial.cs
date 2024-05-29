using Library.Core;
using Library.Model.Vouchers;
using System;

namespace Library.Model.SalesManagements
{
    public class SalesMaterial : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal BaseRate { get; set; }
        public decimal BaseQty { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal BaseUoMFactor { get; set; }
        public decimal TransactionRate { get; set; }
        public decimal TransactionQty { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal BooksCurrencyTransactionAmount { get; set; }
        public decimal BooksCurrencyBaseRate { get; set; }
        public decimal BooksCurrencyTaxAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }

        public string PostDrGLGeneralInfoId { get; set; }

        public string PostDrBudgetMasterId { get; set; }

        public string PostDrActivityId { get; set; }

        public string PostCrGLGeneralInfoId { get; set; }

        public string PostCrBudgetMasterId { get; set; }

        public string PostCrActivityId { get; set; }
        public string GoodsDescription { get; set; }
        public bool IsCanceled { get; set; }
        public string Remark { get; set; }
        public string CanceledBy { get; set; }
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

        public Sales Sales { get; set; }
        public string SalesId { get; set; }

        public string SalesOrderId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string BaseUOMId { get; set; }
        public string TransactionUoMId { get; set; }
        public VoucherDetail VoucherDetail { get; set; }
        
        public string VoucherDetailId { get; set; }
        public string FirstCharacteristicsId { get; set; }

        public string FirstCharacteristicsValueId { get; set; }

        public string SecondCharacteristicsId { get; set; }

        public string SecondCharacteristicsValueId { get; set; }

        public string ThirdCharacteristicsId { get; set; }

        public string ThirdCharacteristicsValueId { get; set; }

        #endregion Navigation Properties
    }
}
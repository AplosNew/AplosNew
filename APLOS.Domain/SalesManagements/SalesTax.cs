using Library.Core;
using Library.Model.Taxations;
using System;

namespace Library.Model.SalesManagements
{
    public class SalesTax : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public decimal BooksCurrencyTransactionAmount { get; set; }
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

      //  public Sales Sales { get; set; }
        public string SalesId { get; set; }
       // public SalesMaterial SalesMaterial { get; set; }
        public string SalesMaterialId { get; set; }
       // public SalesService SalesService { get; set; }
        public string SalesServiceId { get; set; }
        public string TaxCategoryId { get; set; }
        public string HSNCodeId { get; set; }

        #endregion Navigation Properties
    }
}
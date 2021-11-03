using Library.Core;
using System;

namespace Library.Model.SalesManagements
{
    public class SalesPacking : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ProductLibraryId { get; set; }
        public decimal Amount { get; set; }
        public decimal Qty { get; set; }
       

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

       // public Sales Sales { get; set; }
        public string SalesId { get; set; }

        public string PackingId { get; set; }
        public string VoucherId { get; set; }

      


        #endregion Navigation Properties
    }
}
using Library.Core;
using Library.Model.Inventory;
using System;

namespace Library.Model.Products
{
    public class RequisitionIssueDetail : BaseModel
    {
        #region Scalar Properties
 
        public string Id { get; set; }
        public decimal IssueQty { get; set; }
        public decimal IssueRejectedQty { get; set; }

        
        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        [NeverUpdate]
        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }



        #endregion Audit Properties

        #region Navigation
        public InventoryIssue IssueMaster { get; set; }

        public string IssueMasterId { get; set; }

        public string IssueRequestId { get; set; }
        public IssueRequestMaster IssueRequestMaster { get; set; }

        public string IssueRequestMasterId { get; set; }
        public InventoryIssueDetail IssueDetail { get; set; }
        public string IssueDetailId { get; set; }


       
        #endregion
    }
}
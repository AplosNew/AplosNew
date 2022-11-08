using Library.Core;
using System;

namespace Library.Model.Products
{
    public class IssueRequestMaster : BaseModel
    {
        #region Scalar Properties
 
        public string Id { get; set; }
        
        public string Preparedby { get; set; }

        public string CheckedBy { get; set; }

        public string CheckedByStatus { get; set; }

        public string AuthorizedBy { get; set; }

        public string AuthorizedByStatus { get; set; }


        public string IssueSlipType { get; set; }

        public string CompanyGroupId { get; set; }


        public string CompanyId { get; set; }

        public string PlantId { get; set; }
        public string ProductionOrderId { get; set; }

        public string Orderspecific { get; set; } 
        

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
    }
}
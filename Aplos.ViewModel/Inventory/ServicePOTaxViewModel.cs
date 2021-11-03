using Library.Core;
using System;

namespace Library.ViewModel.Inventory
{
    public class ServicePOTaxViewModel : BaseModel  
    {
        public string Id { get; set; }
        public string ServicePOMasterId { get; set; }
        public string ServicePODetailId { get; set; }
        public string TaxCategoryId { get; set; }
        public string HSNCodeId { get; set; }
        public decimal Percentage { get; set; }

        public decimal TaxAmount { get; set; }


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

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }
        #endregion

    }
}
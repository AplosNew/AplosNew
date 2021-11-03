using Library.Core;
using System;

namespace Library.Model.Products
{
    public class GateEntry : BaseModel
    {
        #region Scalar Properties
        public string Id	{ get; set; }
        public string CompanyGroupId { get; set; }
        public DateTime EntryDate { get; set; }
       
        public string Description { get; set; }
        public decimal PackageQty { get; set; }
        public string ModeofTransport { get; set; }
        public string Bill { get; set; }
        public string PersonName { get; set; }
        public string MobileNo { get; set; }
        public string Remarks { get; set; }
        public string InvoicingPartyPlantId { get; set; }
        public string InvoicingByAddress { get; set; }
        public string DeliveryPartyPlantId { get; set; }
        public string DeliveryByAddress { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
		public string EmployeeId { get; set; } 

		public DateTime GateEntryTime { get; set; }
		public string FlagStatus { get; set; }
		public string EmployeeIdForGateEntry { get; set; }

		public string GateEntryType { get; set; }
		public string PlantWiseGateId { get; set; }

        public string LocalImported { get; set; }


        public string PartyId { get; set; } 


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

        #endregion Audit Properties
    }
}
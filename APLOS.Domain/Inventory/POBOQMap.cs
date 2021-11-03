using Library.Core;
using Library.Model.OpeningBalances;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class POBOQMap : BaseModel  
    {
        #region Scalar Properties

        public string Id { get; set; }
        public  PurchaseOrderDetail PODetail { get; set; }     
        public string PODetailId { get; set; }
      
        public string BOQDetailId { get; set; }
        public decimal TransactionQty { get; set; }
        public decimal BaseQty { get; set; }
        public string TransactionUoMId { get; set; }
        public string BaseUoMId { get; set; }

        public decimal POBOQQty { get; set; } 
        public string POUoMId { get; set; }        

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

  //      #region Navigation Properties

  //      [NeverUpdate, XmlIgnore]
  //      public string CompanyGroupId { get; set; }

		//[NeverUpdate]
  //      public string CompanyId { get; set; }	

		//[NeverUpdate]
  //      public string PlantId { get; set; }


        
  //      #endregion Navigation Properties
    }
}
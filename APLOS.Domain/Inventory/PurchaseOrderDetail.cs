using Library.Core;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class PurchaseOrderDetail : BaseModel 
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal TransactionQty { get; set; }
        public decimal BaseQty { get; set; }
        public decimal BaseUoMFactor { get; set; }
        public decimal TransactionRate { get; set; }
        public decimal WithInvoiceRate { get; set; }
        public decimal AfterInvoiceRate { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal? IssueQty { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal ChargesAmount { get; set; }
        public string MasterOrderId { get; set; } 
        public string MasterOrderDetailId { get; set; }
        public string Description { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string RequisitionId { get; set; }
        public string RequisitionDetailId { get; set; }
        //public bool RequisitionQtyStatus { get; set; }
        //public decimal RequisitionRcvQty { get; set; }  


        public string ArticleId { get; set; }

        public string FirstCharacteristicsId { get; set; }

        public string FirstCharacteristicsValueId { get; set; }

        public string SecondCharacteristicsId { get; set; }

        public string SecondCharacteristicsValueId { get; set; }

        public string ThirdCharacteristicsId { get; set; }

        public string ThirdCharacteristicsValueId { get; set; }

        public decimal AcceptanceRcvQty { get; set; }
        public bool AcceptanceRcvStatusQty { get; set; }

        public string RefferenceNo { get; set; }
        public decimal Tolerance { get; set; }
        

        #endregion Scalar Properties



        #region Navigation Properties

        //[XmlIgnore]
        // public PurchaseOrder PurchaseOrder { get; set; }

        public string InventoryReceiveId { get; set; }
        public POMaterial InventoryMaterial { get; set; }
        public string InventoryMaterialId { get; set; }
        public string TransactionUoMId { get; set; }
        public string BaseUOMId { get; set; }
        public string MaterialStorageId { get; set; }
        public string CountryId { get; set; }
        public bool QtyStatus { get; set; }
        public decimal GRNRcvQty { get; set; }

        #endregion Navigation Properties

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
        //public int TotalAmount { get; set; }

        #endregion Audit Properties
    }
}
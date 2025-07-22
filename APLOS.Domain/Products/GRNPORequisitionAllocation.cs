using Library.Core;
using Library.Model.Inventory;
using Library.Model.Setups;
using System;

namespace Library.Model.Products
{
    public class GRNPORequisitionAllocation : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }

        public InventoryReceiveDetail InventoryReceiveDetail { get; set; }
        public string InventoryReceiveDetailId { get; set; }
        public POBOQMap POBOQMap { get; set; } 
        public string POBOQMapId { get; set; }
        public string POReqDetailsID { get; set; }
       
      
        public decimal TransactionQty { get; set; }
        public string TransactionUoMId { get; set; }
        public decimal BaseQty { get; set; }
        public UnitOfMeasurement BaseUoM { get; set; }
        public string BaseUoMId { get; set; }

        public decimal POBOQQty { get; set; }

        public UnitOfMeasurement POUoM { get; set; }         
        public string POUoMId { get; set; }

        
        public decimal RejectQty { get; set; }
        public decimal RejectBaseQty { get; set; }
        public decimal? ReturnQty { get; set; }
        public string SalesOrderId { get; set; }
        public string BOQDetailId { get; set; }
        //public bool AutoAllocate { get; set; } 
        
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
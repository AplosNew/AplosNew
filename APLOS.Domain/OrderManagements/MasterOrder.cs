using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class MasterOrder : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string MasterOrderNo { get; set; }
        public decimal TotalQty { get; set; } = 0;
        public int NoOfLineItem { get; set; } = 0;
        public string OrderYear { get; set; }
        public string OrderType { get; set; }
        public string Type { get; set; }
        public bool IsReplacement { get; set; }
        public string SpecialTaxId { get; set; }
        public string Remarks { get; set; }
        public string InvoicingByAddress { get; set; }
        public string DeliveryByAddress { get; set; }

        public bool IsExtraOrderPercentage { get; set; }
        public string BuyerReferenceNo { get; set; }
        public string OwnReferenceNo { get; set; }
        public string OrderStatusId { get; set; }
        public string PaymentTermId { get; set; }
        public int PaymentTermDays { get; set; }
        public DateTime? BaseOnDueDate { get; set; }
        public DateTime? MatureDate { get; set; }
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

        #region Navigation Properties

        public string CompanyId { get; set; }
        public string CommitmentId { get; set; }
        public string PlantId { get; set; }
        public string EntityId { get; set; }
        public string PartyId { get; set; }
        public string BuyerId { get; set; }
        public string BuyerBrandId { get; set; }
        public string BuyerDivisionId { get; set; }
        public string BuyerDepartmentId { get; set; }
        public string TestingStandardId { get; set; }
        public string OrderCategoryId { get; set; }
        public string SeasonId { get; set; }
        public string CurrencyId { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string InvoicingPartyPlantId { get; set; }
        public string DeliveryPartyPlantId { get; set; }
        public decimal OrderWastagePercentage { get; set; } = 0;
        public decimal ExtraOrderPercentage { get; set; } = 0;
        public string TotalQtyUOMId { get; set; }
        public string TaskTemplateMasterId { get; set; }
        public string ExceptionalProcessId { get; set; }
        public string ExceptionalSubProcessId { get; set; }
        #endregion Navigation Properties
    }
}



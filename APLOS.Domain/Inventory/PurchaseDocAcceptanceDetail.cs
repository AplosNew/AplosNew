using Library.Core;
using System;

namespace Library.Model.Inventory
{
    public class PurchaseDocAcceptanceDetail : BaseModel 
    {
        #region Scalar Properties

        public string Id { get; set; }      
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string FirstCharacteristicsId { get; set; }
        public string FirstCharacteristicsValueId { get; set; }
        public string SecondCharacteristicsId { get; set; }
        public string SecondCharacteristicsValueId { get; set; }
        public string ThirdCharacteristicsId { get; set; }
        public string ThirdCharacteristicsValueId { get; set; }
        public decimal TransactionQty { get; set; }
        public string TransactionUoMId { get; set; }
        public decimal MaterialTranRate { get; set; }
        public decimal MaterialTranAmount { get; set; }
        public decimal TotalMaterialTranAmount { get; set; }
        public decimal ChargesTranAmount { get; set; }
        public decimal ChargesTaxTranAmount { get; set; }
        public decimal TaxAmount { get; set; }


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
        public string PurchaseDocAcceptanceId { get; set; }
        public string POId { get; set; }
        public string PODetailId { get; set; }
        public string GLGeneralInfoId { get; set; }
        public string BudgetMasterId { get; set; }
        public string ActivityId { get; set; }
        public decimal AcceptanceRate { get; set; }
        public string ServicePOMasterId { get; set; }
        public string ServicePODetailId { get; set; }

        #endregion Navigation Properties
    }
}
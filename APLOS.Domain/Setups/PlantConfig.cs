using Library.Core;
using Library.Model.Organizations;
using Library.Model.Processes;
using System;

namespace Library.Model.Setups
{
    public class PlantConfig : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string FabRollPrefix { get; set; }
        public bool BuyerApplicable { get; set; }
        public decimal BlanketDefaultLength { get; set; }
        public decimal BlanketDefaultWidth { get; set; }
        public bool IsBlanketDefaultLengthValuesChangeable { get; set; }
        public bool IsBlanketDefaultWidthValuesChangeable { get; set; }
        public bool IsAfterWashShrinkageOnActual { get; set; }
        public string Operation { get; set; }
        public string WeekendforProductionOrder { get; set; }
        public bool IsMachineChangeableinBulletinTemplate { get; set; }
        public bool OperationInProductionBookingWillBeCapturebyBulletin { get; set; }
        
        public string MachineBudgetLevel { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        ///
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        ///
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        ///
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

        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public virtual Plant Plant { get; set; }
        public string PlantId { get; set; }

        #endregion Navigation Properties
    }
}
using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class LineProductionBooking : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string PlantName { get; set; }
        public DateTime ProductionDate { get; set; }
        public string Line { get; set; }
        public string ProductionShift { get; set; }
        public string SalesOrder { get; set; }
        public string Fabrication { get; set; }
        public string Style { get; set; }
        public decimal ProductionQty { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalManPower { get; set; }
        public decimal PlanRunMC { get; set; }
        public decimal ActualRunMC { get; set; }
        public decimal ExtraMC { get; set; }
        public decimal TrimCheckPress { get; set; }
        public decimal SewingSMV { get; set; }
        public decimal TotalSMV { get; set; }
        public decimal MCMINAvailable { get; set; }
        public decimal NonMCMINAvailable { get; set; }
        public decimal TotalMINAvailable { get; set; }
        public decimal ActualMINWorked { get; set; }
        public decimal MCSAMProd { get; set; }
        public decimal TotalSAMProd { get; set; }
        public decimal MCEfficiency { get; set; }
        public decimal OrderQty { get; set; }
        public decimal TargetQuantity { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialDesc { get; set; }
		public bool NoApplicablePcsRate { get; set; }

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
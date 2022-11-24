using Library.Core;
using System;

namespace Library.Model.Products
{
    public class IssueRequest : BaseModel
    {
        #region Scalar Properties
 
        public string Id { get; set; }
        public string RequisitionId { get; set; }

        public string RequisitionDetailId { get; set; }
        public string CostCenterId { get; set; }

        public string ExpenseActivityId { get; set; }
        
        public decimal RequestedQty { get; set; }
        public decimal RejectedQty { get; set; }
      
        public string BudgetMasterId { get; set; }
        public string GLGeneralInfoId { get; set; }

		public string MaterialMasterId { get; set; }
		public string ArticleId { get; set; }
		public string FirstCharacteristicsId { get; set; }
		public string FirstCharacteristicsValueId { get; set; }
		public string SecondCharacteristicsId { get; set; }
		public string SecondCharacteristicsValueId { get; set; }

		public string ThirdCharacteristicsId { get; set; }
		public string ThirdCharacteristicsValueId { get; set; }
		public string TransactionUoMId { get; set; }

		public string InventoryMaterialId { get; set; }

		public string CountryId { get; set; } 

		public IssueRequestMaster IssueRequestMaster { get; set; } 

        public string IssueRequestMasterId { get; set; }
        public string MaterialIssueControlDetailId { get; set; }

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
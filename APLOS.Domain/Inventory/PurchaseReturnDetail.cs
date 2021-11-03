using Library.Core;
using System;

namespace Library.Model.Inventory
{
	public class PurchaseReturnDetail : BaseModel
	{
		#region Scalar Properties


		public string Id { get; set; }
		
		public string PurchaseReturnId { get; set; }
		public string MaterialMasterId { get; set; }


		public string ArticleId { get; set; }

		public string FirstCharacteristicsId { get; set; }


		public string FirstCharacteristicsValueId { get; set; }

		public string SecondCharacteristicsId { get; set; }


		public string SecondCharacteristicsValueId { get; set; }

		public string ThirdCharacteristicsId { get; set; }


		public string ThirdCharacteristicsValueId { get; set; }

		public string MaterialStorageId { get; set; }


		public decimal TransactionQty { get; set; }

		public string TransactionUoMId { get; set; }


		public decimal BaseQty { get; set; }

		public string BaseUOMId { get; set; }


		public decimal BaseUoMFactor { get; set; }

		public decimal MaterialTranRate { get; set; }


		public decimal MaterialTranAmount { get; set; }

		//public decimal IssueQty { get; set; }

		public decimal TotalTaxAmount { get; set; }

		public decimal TotalMaterialTranAmount { get; set; }


		public decimal TotalMaterialBooksCurrencyAmount { get; set; }

		public decimal ChargesTranAmount { get; set; }


		public decimal ChargesTaxTranAmount { get; set; }

		public decimal TrnCurrencyBaseRate { get; set; }


		public decimal BooksCurrencyBaseRate { get; set; }

		public string CountryId { get; set; }


		//public string POId { get; set; }

		//public string PODetailsId { get; set; }

		//public decimal BaseIssueQty { get; set; }

		//public decimal ShortageQty { get; set; }
		//public decimal RejectionQty { get; set; }
		//public decimal ApprovedQty { get; set; }
		//public decimal ShortageRatePercent { get; set; }
		//public decimal ShortageValue { get; set; }
		//public decimal RejectRatePercent { get; set; }
		//public decimal RejectValue { get; set; }
		//public decimal RejectClamPercent { get; set; }
		//public string RequisitionId { get; set; }
		//public string RequisitionDetailId { get; set; }
		public string Description { get; set; }
		//public bool ShortRejFlag { get; set; }
		public string PostDrGLGeneralInfoId { get; set; }
		public string PostDrBudgetMasterId { get; set; }
		public string PostDrActivityId { get; set; }
		public string PostCRGLGeneralInfoId { get; set; }
		public string PostCRBudgetMasterId { get; set; }
		public string PostCRActivityId { get; set; }
		public string CapitalizeVoucherDetailId { get; set; }
		public bool IsAsset { get; set; }
		public string InventoryReceiveId { get; set; }
		public string InventoryReceiveDetailId { get; set; }
		//public string PurchaseDocumentAcceptanceId { get; set; }
		//public string PurchaseDocumentAcceptanceDetailId { get; set; }
		public string InventoryMaterialId { get; set; } 

		
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

		#endregion Navigation Properties
	}
}
using Library.Core;
using System;

namespace Library.Model.Products
{
	public class GatePassDetails : BaseModel
	{
		#region Scalar Properties

		public string Id { get; set; }
		public string GatePassMasterId { get; set; }
		public string MaterialMasterId { get; set; }
		public string ArticleId { get; set; }
		public string FirstCharacteristicsId { get; set; }
		public string FirstCharacteristicsValueId { get; set; }
		public string SecondCharacteristicsId { get; set; }
		public string SecondCharacteristicsValueId { get; set; }
		public string ThirdCharacteristicsId { get; set; }
		public string ThirdCharacteristicsValueId { get; set; }
		public string MaterialDetail { get; set; }
		public decimal TransactionQty { get; set; }
		public string TransactionUoMId { get; set; }
		public string Remarks { get; set; }
		public bool IsReturnable { get; set; }
		public DateTime? ReturnableDate { get; set; }
		public bool IsMutilated { get; set; }
		public decimal Rate { get; set; }
		public string ChallanNo { get; set; }
		public string ChallanNoDetailId { get; set; } 



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
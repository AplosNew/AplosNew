using Library.Core;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
	public class IssueRequestSKUMap : BaseModel
	{

		#region Scalar Properties

		public string Id { get; set; }
		public string IssueRequestMasterId { get; set; }
		public string FirstCharacteristicsValueId { get; set; }
		public string SecondCharacteristicsValueId { get; set; }
		public string ThirdCharacteristicsValueId { get; set; }
		public decimal RequisitionForQty { get; set; }


		public string MaterialMasterId { get; set; }
		public string ArticleId { get; set; }
		public string SalesOrderId { get; set; }
		public decimal OrderQty { get; set; }
		public decimal PlanOrderQty { get; set; }
		public string Destination { get; set; }
		public string PONumber { get; set; }
		public DateTime PODate { get; set; }

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
using Library.Core;
using System;

namespace Library.Model.Attendances
{
	public class EmpDateWiseShiftAssign : BaseModel
	{
		#region Scalar Properties

		public string EmpSystemID { get; set; }
		public DateTime WorkDate { get; set; }
		public string GroupID { get; set; }
		public string PlantID { get; set; }
		public string EmpSftAssiSystemID { get; set; }
		public string ShiftSystemID { get; set; }
		public int RosterShiftDayCount { get; set; }
		public bool AttdnLock { get; set; }
		public string DayType { get; set; }
		public string ToReprocess { get; set; }

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
		public DateTime DateAdded { get; set; }

		/// <summary>
		/// Record updated user name.
		/// </summary>
		public string UpdatedBy { get; set; }

		/// <summary>
		/// Record updated by user date and time.
		/// </summary>
		public DateTime? DateUpdated { get; set; }

		#endregion Audit Properties
	}
}
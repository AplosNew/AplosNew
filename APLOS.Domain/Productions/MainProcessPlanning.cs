using Library.Core;
using Library.Model.Processes;
using System;

namespace Library.Model.Productions
{
    public class MainProcessPlanning : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public int Sequence { get; set; }
        public DateTime Date { get; set; }
        public DateTime Lsd { get; set; }
        public DateTime CommitmentDate { get; set; }
        public int RunningDay { get; set; }
        public int TotalQty { get; set; }
        public int DailyOutPut { get; set; }
        public int StandardDailyOutPut { get; set; }
        public int LearningCurveOutPut { get; set; }
        public bool HasLearningCurve { get; set; }
        public bool IsFreeze { get; set; }
        public string OffDayType { get; set; }
        public bool OffDay { get; set; }

        public int MinAllocatedLine { get; set; }
        public string IncrementType { get; set; }
        public decimal IncrementValue { get; set; }
        public int StandardTime { get; set; }
        public int DaysToGetTheTarget { get; set; }
        public int FirstDayOutPut { get; set; }
        public int MinRequiredTargetHourly { get; set; }
        public int MinWorkingDays { get; set; }
        public bool IsDb { get; set; }
        public string Color { get; set; }

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

        public string ProductionBatchMasterId { get; set; }

        //public string MainProcessPlaningId { get; set; }
        public string EntityId { get; set; }

        public string OurStyleId { get; set; }
        public string PlantId { get; set; }
        public string LineId { get; set; }
        public virtual Process Process { get; set; }
        public string ProcessId { get; set; }

        #endregion Navigation Properties
    }
}
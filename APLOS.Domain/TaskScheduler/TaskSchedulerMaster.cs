using Library.Core;
using System;

namespace Library.Model.TaskScheduler
{
    public class TaskSchedulerMaster : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string RepeatType { get; set; }
        public int EveryInterval { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsNever { get; set; }
        public bool IsAfter { get; set; }
        public bool IsOn { get; set; }

        public bool isRepeatByDay { get; set; }
        public bool isRepeatByTheNthWeekForMonthly { get; set; }
        
        //public bool isRepeatByTheNthWeek { get; set; }
        public bool isRepeatByTheMonth { get; set; }

        public int AfterNoOfAccurence { get; set; }
        public string WeeklyRepeatationBycommaSepDayName { get; set; }
        public int RepeatByDayNumber { get; set; }
        public string RepeatbyNthWeek { get; set; }
        public string RepeatByMonth { get; set; }
        public string RepeatbyOfEarly { get; set; }
        public string RepeatByWeek { get; set; }
        public string Details { get; set; }
        public bool OnPreviousAccomplishment { get; set; } = true;
        

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

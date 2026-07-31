using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Model.OrderManagements
{
    public class ProductionOrder : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string PlanningTypeProcessId { get; set; }
        //public DateTime? TargetCommitmentDate { get; set; }
        public DateTime? Lsd { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string OrderType { get; set; }
        //public DateTime? TargetLsd { get; set; }
        public DateTime? CommitmentDate { get; set; }
        //public string CommitmentDateRemarks { get; set; }
        //public string CalculationBasis { get; set; }
        //public decimal SPT { get; set; } = 0;
        //public decimal Cm { get; set; } = 0;
        //public int NoOfWorkStation { get; set; } = 0;
        //public decimal Efficiency { get; set; } = 0;
        //public int FirstDayOutPut { get; set; } = 0;
        //public string IncrementType { get; set; }
        //public decimal IncrementValue { get; set; } = 0;
        //public int MinAllocatedLine { get; set; } = 0;
        public double Qty { get; set; } = 0;
        public double PlannedQty { get; set; } = 0;
        //public int StandardTime { get; set; } = 0;
        //public int MinWorkingDays { get; set; } = 0;
        //public int DaysToGetTheTarget { get; set; } = 0;
        //public int MinRequiredTargetHourly { get; set; } = 0;
        //public decimal ProductionPriority { get; set; } = 0;
        public string Remarks { get; set; }
        public string RequiredTimeUnit { get; set; }
        public string OrderLevel { get; set; }
        public string UserDefineLotNo { get; set; }
        public bool IsPreDefineLotApplicable { get; set; }
        public bool IsWorkCenterValidateApplicable { get; set; }

        //public string ProductionStage { get; set; }
        //public string color { get; set; } = "#ffffff";
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

        #region Navigation Property

        [NeverUpdate]
        public string PlantId { get; set; }
        public string EntityId { get; set; }
        public string ProductionStatusId { get; set; }
        //public string RecipeId { get; set; }
        //public string CmCurrencyId { get; set; }

        #endregion Navigation Property
    }
}
using System;

namespace Library.Service.Productions
{
    public class ProcessBlock
    {
        public string Id { get; set; }
        public string MinAllocatedLine { get; set; }
        public string Qty { get; set; }
        public string IncrementType { get; set; }
        public string IncrementValue { get; set; }
        public string StandardTime { get; set; }
        public string DaysToGetTheTarget { get; set; }
        public string FirstDayOutPut { get; set; }
        public string MinRequiredTarget { get; set; }
        public string EntityId { get; set; }
        public string PlantId { get; set; }
        public string LineId { get; set; }
        public string ProductionBatchMasterId { get; set; }
        public string OurStyleId { get; set; }
        public string TotalHour { get; set; }
        public string RunningDay { get; set; }
        public string MinWorkingDays { get; set; }
        public string TotalQty { get; set; }
        public string DailyOutPut { get; set; }
        public string StandardDailyOutPut { get; set; }
        public string LearningCurveOutPut { get; set; }
        public string HasLearningCurve { get; set; }
        public string IsFreeze { get; set; }
        public string OffDayType { get; set; }
        public bool OffDay { get; set; }
        public DateTime Lsd { get; set; }
        public DateTime CommitmentDate { get; set; }
        public DateTime Date { get; set; }
        public int Sequence { get; set; }
    }
}
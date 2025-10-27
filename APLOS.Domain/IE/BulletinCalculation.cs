using Library.Core;
using System;

namespace Library.Model.IE
{
    public class BulletinCalculation : BaseModel 
    {
        #region Scalar Properties
        public int Id { get; set; }
        public string BulletinTemplateMasterId { get; set; }
        public string TotalSPT { get; set; }
        public string TotalManpower { get; set; }
        public string TotalWorkStation { get; set; }
        public string MCtotalspt { get; set; }
        public string MCtotalMP { get; set; }
        public string NonMCtotalspt { get; set; }
        public string NonMCtotalMP { get; set; }
        public string PitchTime { get; set; }
        public string ProductionEfficiencyPerHour { get; set; }
        public string MaxAllottedTime { get; set; }
        public string ProductionEfficiencyPerDay { get; set; }
        public string OrganizationEfficiency { get; set; }
        public string LineTargetPerHour { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}
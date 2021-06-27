using Library.Core;
using Library.Model.Organizations;
using System;

namespace Library.Model.HumanResources
{
    public class MaternityLeavePolicy : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public int ChildNo { get; set; }
        public bool IsBefore { get; set; }
        public bool IsAfter { get; set; }
        public bool IsMonthly { get; set; }
        public decimal BeforePercentage { get; set; }
        public decimal AfterPercentage { get; set; }        
        public int MaternityStartDay { get; set; }
        public int MaternityEndDay { get; set; }
        public int MaternityLeaveStartDay { get; set; }
        public int MaternityLeaveEndDay { get; set; }
        public int CanAvailAfterDOJ { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int GapeBetweenConsecutiveIssue { get; set; }
        public bool IsNoBenefit { get; set; }

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

        #region Navigation Properties

        public string CompanyId { get; set; }

        public string PlantId { get; set; }

        #endregion Navigation Properties
    }
}
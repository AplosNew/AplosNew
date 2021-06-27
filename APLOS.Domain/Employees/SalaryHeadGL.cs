using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Model.ManagementChartOfAccounts;
using Library.Model.Organizations;
using System;

namespace Library.Model.Employees
{
    public class SalaryHeadGL : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string SalaryPayableGroup { get; set; }
        public string DrDirectOtherGLCode { get; set; }
        public string DrDirectOtherGL { get; set; }
        public string CrDirectOtherGLCode { get; set; }
        public string CrDirectOtherGL { get; set; }
        public string DrInDirectOtherGLCode { get; set; }
        public string DrInDirectOtherGL { get; set; }
        public string CrInDirectOtherGLCode { get; set; }
        public string CrInDirectOtherGL { get; set; }

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

        public virtual Company Company { get; set; }
        public string CompanyId { get; set; }
        public virtual Plant Plant { get; set; }
        public string PlantId { get; set; }
        public string SalaryHeadId { get; set; }
        
        public string DrDirectGLId { get; set; }
        public string DrDirectBudgetMasterId { get; set; }
        public string DrDirectActivityId { get; set; }
        public string CrDirectGLId { get; set; }
        public string CrDirectBudgetMasterId { get; set; }
        public string CrDirectActivityId { get; set; }
        public string DrInDirectGLId { get; set; }
        public string DrInDirectBudgetMasterId { get; set; }
        public string DrInDirectActivityId { get; set; }
        public string CrInDirectGLId { get; set; }
        public string CrInDirectBudgetMasterId { get; set; }
        public string CrInDirectActivityId { get; set; }

        #endregion Navigation Properties
    }
}
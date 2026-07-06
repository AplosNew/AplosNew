using Library.Core;
using System;

namespace Library.Model.IE
{
    public class OperationMaster : BaseModel 
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string CompanyGroupID { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string OperationActivityId { get; set; }
        public string OperationTypeId { get; set; }
        public string OperationCategoryId { get; set; }
        public string Skillid { get; set; }
        public string DesignationGroupId { get; set; }
        public string Type { get; set; }
        public string MachineMasterId { get; set; }        
        public string SkillGroupId { get; set; }
        public string LegalDesignationId { get; set; }
        public string ProcessId { get; set; }
        public decimal ProposedSalary { get; set; }
        public string Remarks { get; set; }
        public string SkillLevel { get; set; }
        public bool Active { get; set; }

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
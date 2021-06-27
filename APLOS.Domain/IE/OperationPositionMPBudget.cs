using Library.Core;
using System;

namespace Library.Model.IE
{
    public class OperationPositionMPBudget : BaseModel 
    {
        #region Scalar Properties


        

        public string Id { get; set; }

        public string CompanyGroupID { get; set; }
        /// <summary>
        /// This is used for Is delete active or not.
        /// </summary>
        public string OperationMasterId { get; set; }
        public string EntityId { get; set; }

        public string ShiftId { get; set; }
        public string PositionId { get; set; }

        public string Caption { get; set; }
        public decimal ManpowerBudget { get; set; }

        public bool Active { get; set; }
        public decimal Sequence { get; set; }





        //public string Code { get; set; }

        ///// <summary>
        ///// This is Short Name.
        ///// </summary>
        //public string ShortName { get; set; }

        ///// <summary>
        ///// This is Standard Name.
        ///// </summary>
        //public string StandardName { get; set; }

        ///// <summary>
        ///// This is User Name.
        ///// </summary>
        //public string UserName { get; set; }

        //public string OperationActivityId { get; set; }
        //public string OperationTypeId { get; set; }
        //public string OperationCategoryId { get; set; }
        //public string Skillid { get; set; }



        //public string Type { get; set; }

        //public string MachineMasterId { get; set; }

        //public string SkillGroupId { get; set; }

        //public string LegalDesignationId { get; set; }
        //public string ProcessId { get; set; }


        //public decimal ProposedSalary { get; set; }
        //public string Remarks { get; set; }


        //public bool Active { get; set; }

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
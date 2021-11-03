using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class SOPSubCategory : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// This is used for Is delete active or not.
        /// </summary>
        public bool Archive { get; set; }

        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets or sets the name of the standard. </summary>
        ///
        /// <value> The name of the standard. </value>
        ///-------------------------------------------------------------------------------------------------

        public string StandardName { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets or sets the name of the user. </summary>
        ///
        /// <value> The name of the user. </value>
        ///-------------------------------------------------------------------------------------------------
        public string UserName { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets or sets the description. </summary>
        ///
        /// <value> The description. </value>
        ///-------------------------------------------------------------------------------------------------
        public string Description { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets or sets the remarks. </summary>
        ///
        /// <value> The remarks. </value>
        ///-------------------------------------------------------------------------------------------------
        public string Remarks { get; set; }

        public string FileName { get; set; }
        public string FileId { get; set; }

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
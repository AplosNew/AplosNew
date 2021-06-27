using Library.Core;
using System;

namespace Library.Model.IE
{
    public class OperationTimeCaptureMaster : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public bool Active { get; set; }
        public bool Archive { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string Line { get; set; }
        public string Unit { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string NoOfVariant { get; set; }
        public string FirstVariant { get; set; }
        public string SecondVariant { get; set; }
        public string ThirdVariant { get; set; }

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

        [NeverUpdate]
        public string CompanyGroupId { get; set; }
        public string OperationId { get; set; }
        public string OperationVideoUploadId { get; set; }
        public string MaterialMasterArticleId { get; set; }

        #endregion Navigation Properties
    }
}
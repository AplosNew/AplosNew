using Library.Core;
using System;

namespace Library.Model.Productions
{
    public class AuthorizationConfig : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string GroupID { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        public string EmployeeId { get; set; }
        public string ActionStatus { get; set; }
       

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

        
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}
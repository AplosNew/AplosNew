#region Using

using Library.Core;
using System;
using System.Xml.Serialization;

#endregion Using

namespace Library.Model.Machines
{
    public class OperationAttributeValue : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }

        public decimal Sequence { get; set; }

        public string Code { get; set; }

        public string ShortName { get; set; }

        public string StandardName { get; set; }

        public string UserName { get; set; }


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

        [XmlIgnore]
        public virtual Operation Operation { get; set; }
        public string OperationId { get; set; }

        [XmlIgnore]
        public virtual OperationAttribute OperationAttribute { get; set; }
        public string OperationAttributeId { get; set; }

        #endregion Navigation Properties
    }
}
using Library.Core;
using Library.Model.Materials;
using System;
using System.Xml.Serialization;

namespace Library.Model.OrderManagements
{
    public class SampleOrderSubMaterialValue : BaseModel
    {
        #region Scalar Properties

        public int Id { get; set; }
        public string MaterialAttributeValueFreeText { get; set; }

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
        public SampleOrder SampleOrder { get; set; }
        public string SampleOrderId { get; set; }

        //[XmlIgnore]
        //public SampleOrderSubMaterial SampleOrderSubMaterial { get; set; }
        public string SampleOrderSubMaterialId { get; set; }

        [XmlIgnore]
        public MaterialAttribute MaterialAttribute { get; set; }
        public string MaterialAttributeId { get; set; }

        [XmlIgnore]
        public MaterialAttributeValue MaterialAttributeValue { get; set; }
        public string MaterialAttributeValueId { get; set; }

        #endregion Navigation Properties
    }
}
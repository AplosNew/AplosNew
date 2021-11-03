using Library.Core;
using Library.Model.Currencies;
using Library.Model.Materials;
using Library.Model.Setups;
using System;
using System.Collections.Generic;

namespace Library.Model.OrderManagements
{
    public class SampleOrderSubMaterial : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public int Qty { get; set; }
        public decimal Rate { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public DateTime DeliveryDate { get; set; }
        public bool IsConfirmed { get; set; }

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

        public virtual UnitOfMeasurement UoM { get; set; }
        public string UoMId { get; set; }
        public virtual Currency Currency { get; set; }
        public string CurrencyId { get; set; }
        public virtual SampleOrder SampleOrder { get; set; }
        public string SampleOrderId { get; set; }
        public virtual MaterialGroupMaster MaterialGroupMaster { get; set; }
        public string MaterialGroupMasterId { get; set; }
        public string TestingStandardId { get; set; }
        public virtual MaterialMaster MaterialMaster { get; set; }
        public string MaterialMasterId { get; set; }
        public virtual MaterialMasterArticle Article { get; set; }
        public string ArticleId { get; set; }
        public string FirstCharacteristicsId { get; set; }
        public string FirstCharacteristicsValueId { get; set; }
        public string SecondCharacteristicsId { get; set; }
        public string SecondCharacteristicsValueId { get; set; }
        public string ThirdCharacteristicsId { get; set; }
        public string ThirdCharacteristicsValueId { get; set; }
        public ICollection<SampleOrderSubMaterialValue> MaterialAttributeValues { get; set; }

        #endregion Navigation Properties
    }
}
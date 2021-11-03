using Library.Core;
using Library.Model.Payments;
using Library.Model.Setups;

namespace Library.Model.Products
{
    public class ItemMaster : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchive { get; set; }

        public string StandardName { get; set; }
        public string Description { get; set; }
        public string ShortName { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
        public string Sequence { get; set; }
        public string Remarks { get; set; }
        public string AT73C18 { get; set; }
        public string AT73C19 { get; set; }

        #endregion Scalar Properties

        #region Navigation Properties

        public string ItemTypeId { get; set; }

        public string ProcurementCategoryId { get; set; }
        public string ProcurementBaseId { get; set; }

        public string ProcurementFrequencyId { get; set; }

        public string PaymentPolicyId { get; set; }

        public virtual PaymentTerm PaymentTerm { get; set; }

        public string PaymentTermId { get; set; }

        public string DependentDateId { get; set; }

        public virtual ItemCategory ItemCategory { get; set; }

        public string ItemCategoryId { get; set; }

        public virtual ItemSubCategory ItemSubCategory { get; set; }

        public string ItemSubCategoryId { get; set; }

        public virtual Item Item { get; set; }
        public string ItemId { get; set; }

        #endregion Navigation Properties
    }
}
using Library.Core;
using Library.Model.Currencies;
using System;

namespace Library.Model.Products
{
    public class ProductMaster : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public bool Active { get; set; }
        public bool Archive { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public decimal CostAndManufacture { get; set; }
        public int FirstdayOutPut { get; set; }
        public int DaysToReachTheTarget { get; set; }
        public string IsFixed { get; set; }
        public int IncrementValue { get; set; }
        public int TargetQty { get; set; }
        public string BaseProcessId { get; set; }
        public string CostingType { get; set; }
        public string PlanningType { get; set; }
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets or sets the type of the item. </summary>
        /// <value> The type of the item. </value>
        ///-------------------------------------------------------------------------------------------------
        public virtual ProductCategory ProductCategory { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets or sets the identifier of the ProductCategory. </summary>
        /// <value> The identifier of the ProductCategory. </value>
        ///-------------------------------------------------------------------------------------------------

        public string ProductCategoryId { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets or sets the category the expenses belongs to. </summary>
        /// <value> The ProductSubCategory . </value>
        ///-------------------------------------------------------------------------------------------------

        public virtual ProductSubCategory ProductSubCategory { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets or sets the identifier of the expenses category. </summary>
        /// <value> The identifier of the expenses category. </value>
        ///-------------------------------------------------------------------------------------------------

        public string ProductSubCategoryId { get; set; }

        public virtual Product Product { get; set; }
        public string ProductId { get; set; }

        public string CompanyGroupId { get; set; }

        public virtual Currency CostAndManufactureCurrency { get; set; }
        public string CostAndManufactureCurrencyId { get; set; }
        #endregion Navigation Properties
    }
}
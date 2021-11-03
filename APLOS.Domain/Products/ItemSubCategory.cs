using Library.Core;
using System;

namespace Library.Model.Products
{
    public class ItemSubCategory : BaseModel
    {
        #region Constructor

        public ItemSubCategory()
        {
            //this.ProductMasters = new HashSet<ProductMaster>();
        }

        #endregion Constructor

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// This is used for Is delete active or not.
        /// </summary>
        public bool IsArchive { get; set; }

        /// <summary>
        /// Interface Field S#              : 7
        /// Field No                        : F7
        /// Interface Field Code            : A7IF29F7
        /// Table Column Ref Code           : A7T29C7
        /// Table Code                      : AplosTb_29
        /// Column Title                    : Seq
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is Sequence for sorting.
        /// </summary>
        public decimal Sequence { get; set; }

        /// <summary>
        /// Interface Field S#              : 6
        /// Field No                        : F6
        /// Interface Field Code            : A7IF29F6
        /// Table Column Ref Code           : A7T29C6
        /// Table Code                      : AplosTb_29
        /// Column Title                    : Itm_Cd
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is Item Code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Interface Field S#              : 4
        /// Field No                        : F4
        /// Interface Field Code            : A7IF29F4
        /// Table Column Ref Code           : A7T29C4
        /// Table Code                      : AplosTb_29
        /// Column Title                    : Sht_Nm
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is Short Name.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Interface Field S#              : 2
        /// Field No                        : F2
        /// Interface Field Code            : A7IF29F2
        /// Table Column Ref Code           : A7T29C2
        /// Table Code                      : AplosTb_29
        /// Column Title                    : Std_Nm
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is Standard Name.
        /// </summary>
        public string StandardName { get; set; }

        /// <summary>
        /// Interface Field S#              : 5
        /// Field No                        : F5
        /// Interface Field Code            : A7IF29F5
        /// Table Column Ref Code           : A7T29C5
        /// Table Code                      : AplosTb_29
        /// Column Title                    : Usr_Nm
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is User Name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Interface Field S#              : 3
        /// Field No                        : F3
        /// Interface Field Code            : A7IF29F3
        /// Table Column Ref Code           : A7T29C3
        /// Table Code                      : AplosTb_29
        /// Column Title                    : Disc
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Interface Field S#              : 8
        /// Field No                        : F8
        /// Interface Field Code            : A7IF29F8
        /// Table Column Ref Code           : A7T29C8
        /// Table Code                      : AplosTb_29
        /// Column Title                    : Remks
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// Remarks for comments
        /// </summary>
        public string Remarks { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
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

        /// <summary>
        /// ProductMaster collections.
        /// </summary>
        //public virtual ICollection<ProductMaster> ProductMasters { get; set; }

        #endregion Navigation Properties
    }
}
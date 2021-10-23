using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class SeparationType : BaseModel
    {
        #region Constructor

        public SeparationType()
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
        /// Interface Field Code            : A7IF27F7
        /// Table Column Ref Code           : A7T27C7
        /// Table Code                      : AplosTb_27
        /// Column Title                    : Seq
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is Sequence for sorting.
        /// </summary>
        public decimal Sequence { get; set; }

        /// <summary>
        /// Interface Field S#              : 6
        /// Field No                        : F6
        /// Interface Field Code            : A7IF27F6
        /// Table Column Ref Code           : A7T27C6
        /// Table Code                      : AplosTb_27
        /// Column Title                    : Itm_Cd
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is Item Code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Interface Field S#              : 4
        /// Field No                        : F4
        /// Interface Field Code            : A7IF27F4
        /// Table Column Ref Code           : A7T27C4
        /// Table Code                      : AplosTb_27
        /// Column Title                    : Sht_Nm
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is Short Name.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Interface Field S#              : 2
        /// Field No                        : F2
        /// Interface Field Code            : A7IF27F2
        /// Table Column Ref Code           : A7T27C2
        /// Table Code                      : AplosTb_27
        /// Column Title                    : Std_Nm
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is Standard Name.
        /// </summary>
        public string StandardName { get; set; }

        /// <summary>
        /// Interface Field S#              : 5
        /// Field No                        : F5
        /// Interface Field Code            : A7IF27F5
        /// Table Column Ref Code           : A7T27C5
        /// Table Code                      : AplosTb_27
        /// Column Title                    : Usr_Nm
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// This is User Name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Interface Field S#              : 3
        /// Field No                        : F3
        /// Interface Field Code            : A7IF27F3
        /// Table Column Ref Code           : A7T27C3
        /// Table Code                      : AplosTb_27
        /// Column Title                    : Disc
        /// Entry Type                      : Entry (D)
        /// Interface / Data Field Purpose  :
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Interface Field S#              : 8
        /// Field No                        : F8
        /// Interface Field Code            : A7IF27F8
        /// Table Column Ref Code           : A7T27C8
        /// Table Code                      : AplosTb_27
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
        public string FormulaDes { get; set; }
        public string FormulaDesID { get; set; }
        public string PlantID { get; set; }
        public bool IsGratuityApplicable { get; set; }
        public bool IsFixedDayAmountApplicable { get; set; }
        public bool IsNetPayWithFinalSattlement { get; set; }
        #endregion Audit Properties

        #region Navigation Properties

        /// <summary>
        /// ProductMaster collections.
        /// </summary>
        //public virtual ICollection<ProductMaster> ProductMasters { get; set; }

        #endregion Navigation Properties
    }
}
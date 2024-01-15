using Library.Core;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class InventoryIssue : BaseModel
    {
      
        #region Scalar Properties

        public string Id { get; set; }



        public string IssueRequestMasterId { get; set; }

        [NeverUpdate]
        public string CompanyGroupId { get; set; }

        [NeverUpdate]
        public string CompanyId { get; set; }

        [NeverUpdate]
        public string PlantId { get; set; }

        [NeverUpdate]
        public string EntityId { get; set; }

        public DateTime IssueDate { get; set; }

        public string MaterialStorageId { get; set; }


        public string Status { get; set; }

        public  Voucher Voucher { get; set; }
        public string VoucherId { get; set; }
        public Voucher CapitalizeVoucher { get; set; }

        public string CapitalizeVoucherId { get; set; }
        public string EmployeeId { get; set; }
        public string CurrencyId { get; set; }
        public string Remarks { get; set; }
        public string IssueType { get; set; }
        public string OrderRefNo { get; set; }
        public string ProductionOrderId { get; set; }
        public string SlipWisePRNo { get; set; }
        public string ContractId { get; set; }
        public string RefferenceNo { get; set; }
        public string Types { get; set; }

        public string JWContractId { get; set; }
        public string ContractType { get; set; }
        public string Orderspecific { get; set; }

        public string JobWorkContractId { get; set; }
        public bool IsPostingRequired { get; set; }
        public string IssueCategory { get; set; }

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
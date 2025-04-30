using Library.Core;
using System;

namespace Library.Model.IE
{
    public class MachineMasterUI : BaseModel 
    {
        #region Scalar Properties


        public string Id { get; set; }

        public string CompanyGroupId { get; set; }
     
        public string MachineCategoryId { get; set; }
        public string MachineSubCategoryId { get; set; }
        public decimal Sequence { get; set; }

        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// This is Standard Name.
        /// </summary>
        public string StandardName { get; set; }

        /// <summary>
        /// This is User Name.
        /// </summary>
        public string UserName { get; set; }
        public string Description { get; set; }

        public string Remarks { get; set; }
        public string MachineMake { get; set; }
        public string MachineModel { get; set; }
        public string MachinePerticulars { get; set; }

        public string SkillId { get; set; }
        public string MachineGroupId { get; set; }
        public decimal ProductionMachineQty { get; set; }
        public decimal SampleMachineQty { get; set; }
        public decimal TrainingMachineQty { get; set; }
        public decimal RentMachineQty { get; set; }

        public decimal OtherMachineQty { get; set; }

        public string ConnectedPower { get; set; }
        public string RunningLoad { get; set; }
        public string ConnectedSteam { get; set; }
        public string RunningSteam { get; set; }
        public string ConnectedAir { get; set; }
        public string RunningAir { get; set; }
        public bool MaintanenceScheduleApplicable { get; set; }

        public bool Active { get; set; }

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
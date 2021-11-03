using Library.Core;
using Library.Model.Organizations;
using System;

namespace Library.Model.Employees
{
    public class PreRecruitmentEmployee : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string NationalID { get; set; }
        public DateTime? SubmitDateTime { get; set; }
        public DateTime? SelectionDateTime { get; set; }
        public string SelectedBy { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? DOJ { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string EmployeeName { get; set; }
        public string EmpType { get; set; }
        public string SelectionStatus { get; set; }
        public string ConfirmationStatus { get; set; }
        public string TIN { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string SpouseName { get; set; }
        public string SpouseNationalID { get; set; }
        public string SpouseOccupation { get; set; }
        public int? NoOfChildren { get; set; }
        public string PresentAddress1 { get; set; }
        public string PresentAddress2 { get; set; }
        public string ParmanentAddress1 { get; set; }
        public string ParmanentAddress2 { get; set; }
        public string PresZipCode { get; set; }
        public string ParmZipCode { get; set; }
        public bool ReadyForCandidateAccess { get; set; }
        public bool Submitted { get; set; }
        public bool Completed { get; set; }
        public bool Active { get; set; }
        public string LegalDesignationId { get; set; }
        public DateTime? BirthdayCelebrationDate { get; set; }
        public DateTime? MarriagedayCelebrationDate { get; set; }
        public DateTime? AgreedDOJ { get; set; }
        public decimal TotalSalary { get; set; }
        public int SpecialReviewDuration { get; set; }
        public decimal SpecialReviewAmount { get; set; }
        public bool IsFirstlogin { get; set; }
        public string InitialPIN { get; set; }
        public string NewPIN { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public int ExpiredDays { get; set; }
        public string AppAddedBy { get; set; }
        public DateTime? AppAddedDateTime { get; set; }
        public string AppUpdatedBy { get; set; }
        public DateTime? AppUpdatedDateTime { get; set; }
        public string ConfirmationBy { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public string ParmanentArea { get; set; }
        public string PresentArea { get; set; }
        public bool IsKnownPerson { get; set; }
        public int NumberOfKnownPerson { get; set; }
        public int IsExceptionalDesigApplicable { get; set; }
        public bool IsApproved { get; set; }
        public bool IsImage { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedFromIP { get; set; }
        public int ConfirmAfterDays { get; set; }
        public bool IsDepartmentSubmit { get; set; }
        public string DeptDocumentBy { get; set; }
        public DateTime? DeptDocumentDateTime { get; set; }
        public bool ApplyingAsFresher { get; set; }
        public string Status { get; set; }

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

        public Designation GivenDesignation { get; set; }
        public string GivenDesignationId { get; set; }

        public string InterviewRankingId { get; set; }
        public string EmployeeId { get; set; }
        public string BudgetId { get; set; }
        public string EmployeeCode { get; set; }
        public string GroupID { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        public string PositionID { get; set; }
        public string CitizenID { get; set; }
        public string ReligionID { get; set; }
        public string CivilStatusID { get; set; }
        public string BloodGroupID { get; set; }
        public string PresThanaID { get; set; }
        public string ParmThanaID { get; set; }
        public string PresPostOfficeID { get; set; }
        public string ParmPostOfficeID { get; set; }
        public string PresDistrictID { get; set; }
        public string ParmDistrictID { get; set; }
        public string PresCountryID { get; set; }
        public string ParmCountryID { get; set; }
        public string PresCityID { get; set; }
        public string ParmCityID { get; set; }
        public string PresAreaID { get; set; }
        public string ParmAreaID { get; set; }
        public string EmrCntPer1Name { get; set; }
        public string EmrCntPer2Name { get; set; }
        public string EmrCntPer1CellNo { get; set; }
        public string EmrCntPer1CellNo2 { get; set; }
        public string EmrCntPer1CellNo3 { get; set; }
        public string EmrCntPer2CellNo { get; set; }
        public string EmrCntPer2CellNo2 { get; set; }
        public string EmrCntPer2CellNo3 { get; set; }
        public string Image { get; set; }
        public string PresStateId { get; set; }
        public string ParmStateId { get; set; }
        public string OperationMasterID { get; set; }
        #endregion Navigation Properties
    }
}
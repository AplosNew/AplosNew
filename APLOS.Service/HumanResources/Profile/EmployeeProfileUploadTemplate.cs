using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.HumanResources.Profile
{
    public class EmployeeProfileUploadTemplate
    {
        //
        //public string SystemId { get; set; }
        //public string EmployeeId { get; set; }
        public string EmployeeCodeType { get; set; }
        public string EmployeeCode { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string MaritalStatus { get; set; }

        public string SpouseName { get; set; }
        public string PresentAddress1 { get; set; }
        public string PresentAddress2 { get; set; }
        public string PermanentAddress1 { get; set; }
        public string PermanentAddress2 { get; set; }

        public string EmpType { get; set; }
        public string EmploymentType { get; set; }
        public string Gender { get; set; }
        public string Religion { get; set; }
        public string BloodGroup { get; set; }

        public string PhoneNo { get; set; }
        public string CardNumber { get; set; }
        public string NID { get; set; }
        public string DOB { get; set; }
        public string CelebrationDOB { get; set; }

        public string DOJ { get; set; }
        public string PPeriod_Date { get; set; }
        //public string ShiftEffectiveDate { get; set; }
        //public string RosterShiftName { get; set; }
        //public string AssignShiftName { get; set; }

        //public string WeekOffEffectiveDate { get; set; }
        //public string AlignWithCompany { get; set; }
        //public string IndividualWeekOff { get; set; }
        public string JobLocation { get; set; }
        public string LegalDesignation { get; set; }

        public string BudgetCode { get; set; }
        public string PaymentMode { get; set; }
        public string Country_permanent { get; set; }
        public string Citizen { get; set; }
        public string State_Division { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string TrainingType { get; set; }
        public string EntryLevel { get; set; }
        public string IsConfirmed { get; set; }

        //public string Remarks { get; set; }
    }
}

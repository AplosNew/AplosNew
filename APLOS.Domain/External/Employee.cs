using Library.Core;
using System;

namespace Library.Model.External
{
    public class Employee : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ReportingOfficerId { get; set; }
        public string CompanyId { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string Name { get; set; }
        public string NewPIN { get; set; }
        public string InitialPIN { get; set; }
        public string Col1 { get; set; }
        public string Col2 { get; set; }
        public string Col3 { get; set; }
        public string Col4 { get; set; }
        public string Col5 { get; set; }
        public string Col6 { get; set; }
        public string Col7 { get; set; }
        public string Col8 { get; set; }
        public string Col9 { get; set; }
        public string Col10 { get; set; }
        public string Col11 { get; set; }
        public string Col12 { get; set; }
        public string Col13 { get; set; }
        public string Col14 { get; set; }
        public string Col15 { get; set; }
        public string Col16 { get; set; }
        public string Col17 { get; set; }
        public string Col18 { get; set; }
        public string Col19 { get; set; }
        public string Col20 { get; set; }
        public bool Submit { get; set; }
        public bool IsFirstlogin { get; set; }
        public bool IsAccessRestricted { get; set; }
        public string SalutationId { get; set; }
        public int TimesSend { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime DOJ { get; set; }
        public DateTime? BirthdayCelebrationDate { get; set; }
        public DateTime? FirstLoginTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? SubmitTime { get; set; }
        public bool AccessUser { get; set; }
        public DateTime? AccessUserDateTime { get; set; }
        public DateTime? PinChangeDateTime { get; set; }

        #endregion Scalar Properties
    }
}
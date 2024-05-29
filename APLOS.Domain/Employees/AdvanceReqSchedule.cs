using Library.Core;
using Library.Model.Advances;
using System;

namespace Library.Model.Employees
{
    public class AdvanceReqSchedule : BaseModel
    {       
        public string UpdatedFromIP { get; set; }//
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedBy { get; set; }
        public bool IsDeferred { get; set; }
        public bool IsReScheduled { get; set; }
        public bool IsRepaid { get; set; }
        public int DeferredAdjustmentNumber { get; set; }
        //public virtual Financing Financing { get; set; }
        public int ScheduleNo { get; set; }
        public DateTime? OldInstallmentDate { get; set; }
        public DateTime InstallmentDate { get; set; }
        public decimal Arrear { get; set; }
        public decimal Balance { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal OtherAmount { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal ProfitAmount { get; set; }
        public decimal InstallmentAmount { get; set; }
        public string Id { get; set; }
        public int InstallmentNo { get; set; }
        public int YearNo { get; set; }
        public int MonthNo { get; set; }
        public string RequisitionId { get; set; }
        public EmployeeSalaryAdvance EmployeeSalaryAdvance { get; set; }

        public string EmployeeSalaryAdvanceId { get; set; }
        public string EmployeeAdvanceDetailId { get; set; }

    }
}
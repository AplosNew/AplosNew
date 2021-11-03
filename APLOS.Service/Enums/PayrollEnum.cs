using System.ComponentModel;

namespace Library.Service.Enums
{

    public enum SalaryHeadEnum
    {
        AttendanceBonus,
        Absenteeism,
        BonusRetain,
        ESIC,
        OT,
        PF
    }
    public enum PayRegisterCofigEnum
    {
        [Description("Exclude Father Name")]
        ExcludeFatherName,
        [Description("Exclude Non Payable(Notional)")]
        ExcludeNonPayableNotional,
        [Description("Exclude Total Gross")]
        ExcludeTotalGross,
        [Description("Exclude CTC")]
        ExcludeCTC
    }
    public enum PayRegisterSettingsPerPage
    {
        [Description("Structure And Earning Except Attendance")]
        StructreAndEarningExceptAttendance,
        [Description("Earning Except Attendance")]
        EarningExceptAttendance,
        [Description("Structure And Earning With Attendance")]
        StructureAndEarningWithAttendance,
        [Description("Earning With Attendance")]
        EarningWithAttendance
    }
    public enum DailyAllowanceCatagoryEnum
    {
        [Description("Hourly OffDuty")]
        HourlyOffDuty,
        [Description("Daily Allowance Time Based")]
        DailyAllowanceTimeBased,
        [Description("Week Off Allowance")]
        WeekOffAllowance,
        [Description("Holiday Allowance")]
        HolidayAllowance,
        [Description("Hourly Off Duty Deduction")]
        HourlyOffDutyDeduction
    }
}
using System.ComponentModel;

namespace Library.Service.Enums
{
    public enum EmployeeSeprationSetupEnum
    {
        [Description("Joining Date")]
        JoiningDate,
        [Description("Confirmation Date")]
        ConfirmationDate,
        [Description("Resign Date")]
        ResignDate,
        [Description("Separation Date")]
        SeparationDate,
        [Description("Earn Leave")]
        EarnLeave,
        [Description("Basic")]
        Basic,
        [Description("Gross")]
        Gross,
        [Description("Leave Encashment")]
        LeaveEncashment,
        [Description("Notice Period")]
        NoticePeriod,
        [Description("Served Notice Period")]
        ServedNoticePeriod,
        [Description("Short Notice Period")]
        ShortNoticePeriod,
        [Description("Notice Pay")]
        NoticePay,
        [Description("Additional Deduction")]
        AdditionalDeduction,
        [Description("Additional Earning")]
        AdditionalEarning
    }
}
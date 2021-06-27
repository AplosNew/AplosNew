using Library.Core;

namespace Library.ViewModel.HR.Attendance
{
    public class JobcardVM : BaseModel
    {
        public string WorkingDate { get; set; }
        public string ShiftName { get; set; }
        public string ShiftInTime { get; set; }
        public string ShfitOutTime { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string Duration { get; set; }
        public string DayStatus { get; set; }
        public string LateBy { get; set; }
        public string Leave { get; set; }
        public string ShortLeave { get; set; }
        public string InDeviceId { get; set; }
        public string OutDeviceId { get; set; }

        // Date Shift Name Shift InTime Shift OutTime InTime  OutTime Duration    
        //Day Status  Late By LV Short Leave In Device ID    Out Device ID

    }
}
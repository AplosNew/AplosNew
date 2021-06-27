using Library.Core;
using System;



namespace Library.ViewModel.Setup
{
    public class NotificationSetting : BaseModel
    {
        public string Id { get; set; }
        public string PlantId { get; set; }
        public string BusinessFlow { get; set; }
        public bool NotificationAfterCreation { get; set; }
        public bool RequiredChecking { get; set; }
        public bool NotificationAfterChecking { get; set; }
        public bool RequiredApproval { get; set; }
        public bool NotificationAfterApproval { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

    }
}






using Library.Core;

namespace Library.Model.External
{
    public class EmployeeLink : BaseModel
    {
        #region Scalar Properties

        public string CompanyGroupId { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }

        #endregion Scalar Properties
    }
}
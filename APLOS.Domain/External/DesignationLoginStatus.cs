using Library.Core;

namespace Library.Model.External
{
    public class DesignationLoginStatus : BaseModel
    {
        #region Scalar Properties

        public string Plant { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public int TotalEmployee { get; set; }
        public int NotLoggedIn { get; set; }
        public int Submitted { get; set; }
        public int NotSubmitted { get; set; }

        #endregion Scalar Properties
    }
}
using Library.Core;

namespace Library.Model.External
{
    public class CompanyEmp : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }

        #endregion Scalar Properties
    }
}
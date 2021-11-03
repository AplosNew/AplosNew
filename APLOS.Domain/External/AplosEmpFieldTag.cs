using Library.Core;

namespace Library.Model.External
{
    public class AplosEmpFieldTag : BaseModel
    {
        #region Scalar Properties

        public int Id { get; set; }
        public string ColumnName { get; set; }
        public bool IsAplicable { get; set; }
        public int Sequence { get; set; }
        public int? AplosEmpFieldId { get; set; }
        public string ClientColumnId { get; set; }
        public string ClinetColumnName { get; set; }
        public int CompanyGroupId { get; set; }

        #endregion Scalar Properties
    }
}
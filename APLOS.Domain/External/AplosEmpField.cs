using Library.Core;

namespace Library.Model.External
{
    public class AplosEmpField : BaseModel
    {
        #region Scalar Properties

        public int Id { get; set; }
        public string InterfaceIdField { get; set; }
        public string InterfaceFieldName { get; set; }
        public string AplosColumnId { get; set; }
        public string AplosColumnName { get; set; }
        public bool Active { get; set; }

        #endregion Scalar Properties
    }
}
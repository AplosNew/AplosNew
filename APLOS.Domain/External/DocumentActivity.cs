using Library.Core;
using System;

namespace Library.Model.External
{
    public class DocumentActivity : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ActivityId { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string FileId { get; set; }
        public int DataSourceCategoryId { get; set; }
        public int DocumentFormateId { get; set; }
        public string ApplicationName { get; set; }
        public string PreparedBy { get; set; }
        public string Remarks { get; set; }
        public string PreparedByInCaseOfOther { get; set; }
        public DateTime? AddedDateTime { get; set; }

        #endregion Scalar Properties
    }
}
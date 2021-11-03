using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class EmployeeDependantInfo : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string EmpSystemId { get; set; }
        public string Name { get; set; }
        public string LocalName { get; set; }
        public string RelationId { get; set; }
        public DateTime? DOB { get; set; }
        public string ProfessionId { get; set; }
        public string Remarks { get; set; }       
        #endregion Scalar Properties

        #region Audit Properties
        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        #endregion Audit Properties
    }
}
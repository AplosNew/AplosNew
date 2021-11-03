using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Core;

namespace Library.Model.EmployeeServices
{
    public class EmployeeData : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeStatus { get; set; }
        public string EmployeeCode { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string ShiftId { get; set; }
        public string Shift { get; set; }
        public string EmployeeServiceCategoryId { get; set; }
        public string Category { get; set; }
        public string Chargeable { get; set; }
        public string IsProcessed { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Quantity { get; set; }
        public string Particulars { get; set; }
        public string BillOtherReferenceNo { get; set; }
        public string Amount { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

    }



}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Core;

namespace Library.Model.QMS
{
    public class QMSRejection : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string Date { get; set; }
        public string ShiftMasterId { get; set; }
        public string EmployeeId { get; set; }
        public string ProcessId { get; set; }
        public string LocationId { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string ProductionReferenceId { get; set; }
        public string Remarks { get; set; }
        public string EmployeeStatus { get; set; }
        public string EmpIStatus { get; set; }
        public string Customer { get; set; }


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

    public class QMSRejectionChild : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string QMSRejectionMasterId { get; set; }
        public string StockKeepingUnitId { get; set; }
        public string QMSDefectMasterId { get; set; }
        public string GradeMasterId { get; set; }
      
        public decimal NoOfPics { get; set; }
        public decimal RepairablePics { get; set; }

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

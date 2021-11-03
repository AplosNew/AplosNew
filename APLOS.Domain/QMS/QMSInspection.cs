using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Core;

namespace Library.Model.QMS
{
    public class QMSInspection : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string InspectionMasterId { get; set; }
        public string InspectionTypeId { get; set; }
        public string InspectionLevelId { get; set; }
        public string EmployeeId { get; set; }
        public string ProcessId { get; set; }
        public string LocationId { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string ProductionReferenceId { get; set; }
        public string BatchReferenceNo { get; set; }
        public decimal BatchSize { get; set; }
        public decimal SampleSize { get; set; }
        public decimal NoOfDefectiveUnit { get; set; }  
        public string StatusId { get; set; }
        public string Remarks { get; set; }
        public string Date { get; set; }
        public string ShiftMasterId { get; set; }
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

    public class QMSInspectionChild : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string QMSInspectionId { get; set; }
        public string QMSDefectMasterId { get; set; }
        public string QMSDefectZoneId { get; set; }
        public string MajorMinor { get; set; }
        public string SkillId { get; set; }
        public string DefectResponsiblePersonId { get; set; }
        public decimal NoOfDefect { get; set; }

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

    public class EmployeeInformation : BaseModel
    {
        #region Scalar Properties
        public string isSelected { get; set; }
        public string SystemID { get; set; }
        public string EmployeeStatus { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeCodePreFix { get; set; }
        public string EmployeeCodeNumeric { get; set; }
        public string EmpPicPath { get; set; }
        public string BudgetCode { get; set; }
        public string EntityName { get; set; }
        public string Designation { get; set; }
        public string PositionName { get; set; }
        public string DepartmentName { get; set; }
        public string Section { get; set; }
        public string SectionId { get; set; }
        public string SubSection { get; set; }
        public string Plant { get; set; }

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

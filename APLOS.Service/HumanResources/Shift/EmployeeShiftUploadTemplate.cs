using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.HumanResources.Shift
{
    public class EmployeeShiftUploadTemplate
    {//
        public string SystemId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string JobLocation { get; set; }        
        public string ShiftSystemId { get; set; }
        public string EffectiveDate { get; set; }
        public string IsRoster { get; set; }
        public string RosterSystemID { get; set; }
        public string RosterStartShiftID { get; set; }
    }
    public class EmployeeShiftUploadTemplateVM
    {//
        public string SystemId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string JobLocation { get; set; }
        public string ShiftSystemId { get; set; }
       
        public string IsRoster { get; set; }
        public string RosterSystemID { get; set; }
        public string RosterStartShiftID { get; set; }
    }
}

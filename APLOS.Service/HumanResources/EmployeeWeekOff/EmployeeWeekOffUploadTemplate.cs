using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.HumanResources.Shift
{
    public class EmployeeWeekOffUploadTemplate
    {//
        public string EmpSystemID { get; set; }
        public string EmployeeCode { get; set; }
        public string EffectiveDate { get; set; }
        public string AlignWithCC { get; set; }
        public string IndividualWeekOff { get; set; }        
        public string FstOffDay { get; set; }
        //public string FstDayLengthType { get; set; }
    }
    public class EmployeeWeekOffUploadTemplateVM
    {//
        public string EmpSystemID { get; set; }
        public string EmployeeCode { get; set; }        
        public string AlignWithCC { get; set; }
        public string IndividualWeekOff { get; set; }
        public string FstOffDay { get; set; }
        //public string FstDayLengthType { get; set; }
    }

}

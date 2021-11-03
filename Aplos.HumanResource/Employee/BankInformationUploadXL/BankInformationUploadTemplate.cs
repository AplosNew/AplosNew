using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee.BankInformationUploadXL
{
    public class BankInformationUploadTemplate
    {

        public string EmpSystemID { get; set; }
        public string EmployeeCode { get; set; }
        public string BankName { get; set; }
        public string BankAccNo { get; set; }
        //public string SalaryPercentage { get; set; }
        public string IFSCCode { get; set; }
        public string MICRCode { get; set; }
      
    }
    public class BankInformationUploadTemplateModel
    {

        public string EmpSystemID { get; set; }
        public string EmployeeCode { get; set; }
        public string BankName { get; set; }
        public string BankAccNo { get; set; }
        //public string SalaryPercentage { get; set; }
        public string IFSCCode { get; set; }
        public string MICRCode { get; set; }
        public string BankSystemID { get; set; }
        public string BankBranchId { get; set; }
    }
}

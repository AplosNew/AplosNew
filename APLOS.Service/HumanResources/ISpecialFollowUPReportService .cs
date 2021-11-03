#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;
using System.Web;
using static Library.Service.HumanResources.PayRegisterBDReportService;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface ISpecialFollowUPReportService
    {
        // IWorkbook EmployeeSalaryRegister(PayRegisterParamList PayRegisterParam);
        //IWorkbook EmployeeSalaryRegisterBangla(PayRegisterParamList PayRegisterParam);
        //  IEnumerable<ComboModel> GetSalaryprocessIdCbo(string compnayGroupId, string companyId, string plantId, string MonthNo, string YearNo, string IsCompleteMonth);

        IWorkbook GetSpecialFollowUPReportSummaryExcel(string PlantId ,string fromDate,string toDate);

        //IEnumerable<ComboModel> GetPayGroupCbo(bool sa, bool ca, string userId);
    }
}
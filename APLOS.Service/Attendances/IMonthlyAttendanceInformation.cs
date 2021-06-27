using Library.Model.Attendances;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.Attendances
{
    public interface IMonthlyAttendanceInformation : IService<AttdnDataMonthlySummary>
    {
        IWorkbook XlsMonthlyAttendanceSummaryReport(string companyId, string plantId, string Month, string Year, string userName, string DayStatus, Dictionary<string, string> empParameters, bool withColor, bool includeCurrentDate, bool withSummary, bool isActive, bool isSeperated, bool isMaternity);
       // IWorkbook XlsMonthlyAttendanceSummaryReport(string companyId, string plantId, string Month, string Year, string userName, string DayStatus, Dictionary<string, string> empParameters, bool withColor,bool includeCurrentDate, bool withSummary);
    }
}
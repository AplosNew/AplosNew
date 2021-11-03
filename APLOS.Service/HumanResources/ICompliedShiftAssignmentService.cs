using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;

namespace Library.Service.HumanResources
{
    public interface ICompliedShiftAssignmentService : IService<CompliedShiftAssignment>
    {
        IEnumerable<ComboModel> GetCompliedRosterCbo(string plantId);
        GridModel GetUnAssignEmployee(GridParameter parameters, string plantId);
        GridModel GetCbo(string empId);
        IWorkbook GetDailyComplianceReport(string plantId, string plantName, string companyId, string workDate);
        void InsertOrUpdateGraph(IEnumerable<CompliedShiftAssignment> entities);
        IEnumerable<ComboModel> GetSectionCbo(string sGroupID, bool sysId, string userId);
        GridModel Query(GridParameter parameters, string workDate, string compliedShiftId, string plantId);
        IEnumerable<ComboModel> GetCompliedShiftCbo(string plantId);
        IEnumerable<ComboModel> GetActualShiftCbo(string plantId);
        IEnumerable<ComboModel> GetCompliedShiftGroupingCbo(string plantId);
        GridModel GetAllEmployee(GridParameter parameters, string plantId, string sectionId, string workDate, string compliedShiftGroupId);
        IEnumerable<object> GetEmployeeFixedShift(string empId, string plantId, string fromDate, string toDate);
        void CompliedshiftChange(string addedBy, string ip, string appVersion, DateTime rotationDate, string request);

        IWorkbook GetMonthlyDailyShiftReport(string PlantId, string PlantName, string CompanyId, string userName, string yearId, string monthId, string complianceShiftList);
        IWorkbook GetEmployeeJobCardReport(string PlantId, string PlantName, string CompanyId, string userName, string fromDate, string toDate, string emp);
        //IWorkbook GetMonthlyDailyAttendanceReport(string PlantId, string PlantName, string CompanyId, string userName, string yearId, string monthId);
        IWorkbook GetMonthlyDailyAttendanceReport(string PlantId, string PlantName, string CompanyId, string userName, string yearId, string monthId, string dayStatusReportType);
    }
}
#region Using

using Library.Core;
using Library.Model.External;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.External
{
    public interface IEmployeeService : IService<Employee>
    {
        IWorkbook IndividualInfo(ReportParam status);

        IWorkbook ExceptionInfo(ReportParam status);

        IWorkbook ActivityInfo(ReportParam status, string fromdate, string todate);

        IWorkbook EmployeeInfo(ReportParam status);

        Dictionary<string, object> GetDocumentFolder(string id);

        Dictionary<string, object> QueryEmployeeAccess(string id, string initialpin);

        void UpdateEmployeeSubmit(Employee entity);

        void UpdateUserAccess(Employee entity);

        void UpdatePIN(string id, string newPin);

        void UpdateSubmit(Employee entity);

        void UpdateAccessRestriction(IEnumerable<Employee> list);

        Employee Login(string id, string initialpin);

        Dictionary<string, object> Query(string id);

        IEnumerable<object> QueryList(string id);

        GridModel QueryReportingOfficer(GridParameter parameters, string companyGroupId, string id);

        IEnumerable<object> GetCbo();

        GridModel GetCboList(string companyGroupId);

        GridModel GetNameCboList();

        GridModel GetActivityCategoryCboList();

        GridModel GetActivityImportanceCboList();

        GridModel GetPeriodCboList();

        GridModel GetDocumentFormateCboList();

        GridModel GetDataSourceCategoryCboList();

        GridModel GetEmployeeListByCompanyGroup(GridParameter parameters, string companyGroupId);

        Dictionary<string, object> GetEmployee(string employeeId);

        IEnumerable<object> GetDynamicData(string employeeId);

        GridModel GetEmployeeByCompanyGroup(GridParameter parameters, string companyGroupId);

        GridModel GetEmployeeDataForRestriction(GridParameter parameters, string companyGroupId);

        GridModel GetEmployeeByCompanyGroupAndSubmit(GridParameter parameters, string companyGroupId);

        IEnumerable<ActivityEmp> GetActivityList(string employeeId);

        IEnumerable<DocumentActivity> GetDocumentActivityList(string employeeId);

        IEnumerable<KPI> GetKPIList(string employeeId);
    }
}
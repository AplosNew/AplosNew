#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IEmployeeProbationalPeriodService : IService<EmployeeProbationalPeriod>
    {
        void UpdateStatusActive(string Id);
        GridModel GetInActivemployeeData(GridParameter parameters, string plantId);
        IEnumerable<ComboModel> GetCbo(string plantId);
        GridModel GetConfirmedEmployeeData(GridParameter parameters, string plantId);

        GridModel EmployeeQuery(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyId, string employeeId, string plantId);

        GridModel EmployeeColorQueryByDate(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyId, string employeeId, bool a, bool t, bool f, string plantId);

        void Save(EmployeeProbationalPeriod ui, out string masterid);

        void ProbationalUpdate(IEnumerable<EmployeeProbationalPeriod> entities);

        void ConfirmedEmployeeInfo(IEnumerable<EmployeeInformation> empInfoList);
        void UpdateStatus(string Id);
        IEnumerable<object> ProbationQueryByID(string empID);

        IWorkbook EmployeeConfirmation(string companyGroupId, string companyId, string plantId, string empId, string empType, string tempId);//, string templatePathHindi, string templatePathEnglish, string templatePathBangla);
    }
}
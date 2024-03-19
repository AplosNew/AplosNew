#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPreRecruitmentApprovalService : IService<EmployeeInformation>
    {
        IEnumerable<object> GetLegalDesignationCbobyGivenDesignation(string givenDesignationpId);
        GridModel GetData(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);

        void InsertORUpdate(IEnumerable<PreRecruitmentEmployee> entities);

        IEnumerable<object> GetGivenDesignationCbo(string GroupId);
        GridModel GetLegalDesignationCbo(GridParameter parameters, string companyGroupId, string plantId, string BudgetCode);
        GridModel GetDesignationCbo(GridParameter parameters, string companyGroupId, string plantId, string BudgetCode);
    }
}
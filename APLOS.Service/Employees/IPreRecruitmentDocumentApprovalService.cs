#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPreRecruitmentDocumentApprovalService : IService<PreRecruitmentEmployee>
    {
        void Insert(PreRecruitmentEmployee preRecruitmentEmployee,
            IEnumerable<PreRecruitmentEmpQualification> preRecruitmentEmpQualification,
            IEnumerable<PreRecruitmentEmpExperience> preRecruitmentEmpExperience,
            IEnumerable<PreRecruitmentEmpTraining> preRecruitmentEmpTraining,
            IEnumerable<PreRecruitmentDocument> preRecruitmentDocument);

        IEnumerable<object> GetEmployeeData(string eId);

        IEnumerable<object> GetEmployeeDocumentData(string eId);

        GridModel GetAllSubmittedEmployee(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);

        IEnumerable<object> GetAllDocumentData(string companyGroupId, string budgetId, string plantId, string empType, string pId);
    }
}
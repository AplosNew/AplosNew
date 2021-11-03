#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IApprovedEmployeeService : IService<EmployeeInformation>
    {
        void Insert(EmployeeInformation employeeInformation,
                          IEnumerable<EmpAcademicQualificationInformation> empAcademicQualificationInformations,
                          IEnumerable<EmpExperienceInformation> empExperienceInformations,
                          IEnumerable<EmpTrainingInformation> empTrainingInformations,
                          IEnumerable<EmployeeDocument> employeeDocuments);

        IEnumerable<object> GetEmployeeData(string eId);

        IEnumerable<object> GetEmployeeDocumentData(string eId);

        GridModel GetAllEmployee(GridParameter parameters);

        IEnumerable<object> GetExperienceData(string empSystemID);

        IEnumerable<object> GetQualificationData(string empSystemID);

        IEnumerable<object> GetTrainingData(string empSystemID);
    }
}
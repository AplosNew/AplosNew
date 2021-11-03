#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IPostRecruitmentDocumentService : IService<EmployeeDocument>
    {
        void InsertGraph(IEnumerable<EmployeeDocument> entities, string PreRecruitmentEmployeeId);

        IEnumerable<EmployeeDocument> GetDocumentFile(string id);

        Dictionary<string, object> GetDocFile(string id);

        IEnumerable<object> GetDocumentData(string companyGroupId, string budgetId, string plantId, string pId);

        GridModel GetAllEmployee(GridParameter parameter, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);

        void UpdatePostRecruitmentDocument(string id);

        void InsertORUpdate(EmployeeDocument entity);
    }
}
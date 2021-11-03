#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IDocumentConfigurationDesignationGroupService : IService<DocumentConfigurationDesignationGroup>
    {
        void InsertORUpdateGraph(DocumentConfigurationDesignationGroup entity, IEnumerable<DocumentSetAssignDetail> entities);

        IEnumerable<object> GetCbo();

        IEnumerable<object> GetDesignationGroupDateList(string companyGroupId, string plantId, string employeeTypeId);

        GridModel GetDocumentList(GridParameter parameters, string companyGroupId, string plantId, string employeeTypeId, string documentSetType);

        GridModel QueryAssign(GridParameter parameters, string companyGroupId, string plantId, string employeeTypeId, string employmentType);

        void Delete(string Id);

        GridModel Query(GridParameter parameters, string companyId, string plantId);
    }
}
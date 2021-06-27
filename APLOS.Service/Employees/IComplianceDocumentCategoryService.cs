#region Using

using Library.Core;
using Library.Model.Documents;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IComplianceDocumentCategoryService : IService<ComplianceDocumentCategory>
    {
        new void InsertGraph(ComplianceDocumentCategory entity);

        new void UpdateGraph(ComplianceDocumentCategory entity);

        GridModel Query(GridParameter parameters, string companyGroupId);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();
    }
}
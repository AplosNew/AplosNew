#region Using

using Library.Core;
using Library.Model.Documents;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IComplianceDocumentSubCategoryService : IService<ComplianceDocumentSubCategory>
    {
        new void InsertGraph(ComplianceDocumentSubCategory entity);

        new void UpdateGraph(ComplianceDocumentSubCategory entity);

        GridModel Query(GridParameter parameters, string companyGroupId);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();
    }
}
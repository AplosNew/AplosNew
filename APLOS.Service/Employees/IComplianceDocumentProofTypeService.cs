#region Using

using Library.Core;
using Library.Model.Documents;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Employees
{
    public interface IComplianceDocumentProofTypeService : IService<ComplianceDocumentProofType>
    {
        decimal GetAutoSequence();

        new void InsertGraph(ComplianceDocumentProofType entity);

        new void UpdateGraph(ComplianceDocumentProofType entity);

        GridModel Query(GridParameter parameters);
    }
}
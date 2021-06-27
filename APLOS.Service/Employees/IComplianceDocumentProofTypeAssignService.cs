#region Using

using Library.Core;
using Library.Model.Documents;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IComplianceDocumentProofTypeAssignService : IService<ComplianceDocumentProofTypeAssign>
    {
        void InsertOrUpdate(IEnumerable<ComplianceDocumentProofTypeAssign> entities, string masterId);

        GridModel QueryGraph(GridParameter parameters, string complianceDocumentId);

        void DeleteWithMaster(string Id);
    }
}
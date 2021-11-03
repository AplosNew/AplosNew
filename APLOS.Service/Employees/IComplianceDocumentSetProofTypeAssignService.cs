#region Using

using Library.Core;
using Library.Model.Documents;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IComplianceDocumentSetProofTypeAssignService : IService<ComplianceDocumentSetProofTypeAssign>
    {
        void InsertOrUpdate(IEnumerable<ComplianceDocumentSetProofTypeAssign> entities, string masterId);

        GridModel QueryGraph(GridParameter parameters, string complianceDocumentSetId);

        void DeleteWithMaster(string Id);
    }
}
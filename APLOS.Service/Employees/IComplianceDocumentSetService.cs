#region Using

using Library.Core;
using Library.Model.Documents;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IComplianceDocumentSetService : IService<ComplianceDocumentSet>
    {
        void InsertGraph(ComplianceDocumentSet entity, IEnumerable<ComplianceDocumentSetDetail> complianceDocumentSetDetail, IEnumerable<ComplianceDocumentSetProofTypeAssign> complianceDocumentProofTypeAssign);

        void UpdateGraph(ComplianceDocumentSet entity, IEnumerable<ComplianceDocumentSetDetail> complianceDocumentSetDetail, IEnumerable<ComplianceDocumentSetProofTypeAssign> complianceDocumentProofTypeAssign);

        void DeleteGraph(string id);

        GridModel Query(GridParameter parameters, string companyGroupId);

        //IEnumerable<object> QueryGraph(string complianceDocumentSetId);
        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();

        GridModel GetComplianceDocumentList(GridParameter parameters, string companyGroupId);
    }
}
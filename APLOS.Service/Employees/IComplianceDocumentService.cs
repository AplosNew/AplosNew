#region Using

using Library.Core;
using Library.Model.Documents;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IComplianceDocumentService : IService<ComplianceDocument>
    {
        void InsertGraph(ComplianceDocument entity, IEnumerable<ComplianceDocumentPositonCode> complianceDocumentPositon, IEnumerable<ComplianceDocumentPostRecruitment> complianceDocumentPostRecruitment, IEnumerable<ComplianceDocumentProofTypeAssign> complianceDocumentProofTypeAssign);

        void UpdateGraph(ComplianceDocument entity, IEnumerable<ComplianceDocumentPositonCode> complianceDocumentPositon, IEnumerable<ComplianceDocumentPostRecruitment> complianceDocumentPostRecruitment, IEnumerable<ComplianceDocumentProofTypeAssign> complianceDocumentProofTypeAssign);

        GridModel Query(GridParameter parameters, string companyGroupId, string type);

        void DeleteGraph(string id);

        IWorkbook GetComplianceDocumentReport(string documentLevel, string plantId);

        decimal GetAutoSequence();
    }
}
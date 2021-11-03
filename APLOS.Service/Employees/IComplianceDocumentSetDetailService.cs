#region Using

using Library.Model.Documents;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IComplianceDocumentSetDetailService : IService<ComplianceDocumentSetDetail>
    {
        void InsertOrUpdate(IEnumerable<ComplianceDocumentSetDetail> entities, string masterId);

        IEnumerable<object> Query(string masterId);

        void DeleteWithMaster(string Id);
    }
}
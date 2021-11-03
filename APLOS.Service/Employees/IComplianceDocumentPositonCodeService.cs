#region Using

using Library.Model.Documents;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IComplianceDocumentPositonCodeService : IService<ComplianceDocumentPositonCode>
    {
        void InsertOrUpdate(IEnumerable<ComplianceDocumentPositonCode> entities, string masterId);

        IEnumerable<object> Query(string complianceDocumentId);

        void DeleteWithMaster(string Id);
    }
}
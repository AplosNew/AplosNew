#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IDocumentSetAssignDetailService : IService<DocumentSetAssignDetail>
    {
        void InsertOrUpdate(IEnumerable<DocumentSetAssignDetail> entities, string masterId, bool flag);

        IEnumerable<object> Query(string documentSetId, string plantId, string employeeTypeId);

        void DeleteWithMaster(string Id);
    }
}
#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISOPActivityDocumentService : IService<SOPActivityDocument>
    {
        IEnumerable<object> GetDocumentListMain(string sopItemId);

        IEnumerable<object> GetDocumentList(string activityId);
    }
}
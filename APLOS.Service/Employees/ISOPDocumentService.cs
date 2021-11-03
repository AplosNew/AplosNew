#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISOPDocumentService : IService<SOPDocument>
    {
        IEnumerable<object> GetCbo();

        decimal GetAutoSequence();

        void DeleteGraph(string id);

        Dictionary<string, object> GetSOPDocumentFile(string systemId);

        GridModel Query(GridParameter parameters);
    }
}
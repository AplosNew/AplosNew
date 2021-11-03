#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISOPCategoryService : IService<SOPCategory>
    {
        decimal GetAutoSequence();

        Dictionary<string, object> GetSOPCategoryFile(string systemId);

        void DeleteGraph(string id);
    }
}
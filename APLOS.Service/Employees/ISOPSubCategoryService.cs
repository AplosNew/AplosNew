#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISOPSubCategoryService : IService<SOPSubCategory>
    {
        decimal GetAutoSequence();

        Dictionary<string, object> GetSOPSubCategoryFile(string systemId);

        void DeleteGraph(string id);
    }
}
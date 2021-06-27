#region Using

using Library.Model.Employees;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISOPDocumentCategoryService : IService<SOPDocumentCategory>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
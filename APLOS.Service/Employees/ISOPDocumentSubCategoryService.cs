#region Using

using Library.Model.Employees;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISOPDocumentSubCategoryService : IService<SOPDocumentSubCategory>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
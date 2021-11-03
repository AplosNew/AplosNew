#region Using

using Library.Model.OrderManagements;
using Library.Service.Core;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IOrderCategoryService : IService<OrderCategory>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
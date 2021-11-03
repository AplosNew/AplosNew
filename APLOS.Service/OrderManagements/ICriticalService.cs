#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.Core;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ICriticalService : IService<Critical>
    {
        GridModel GetCbo(string companyGroupId);

        GridModel Query(GridParameter parameters, string companyGroupId);

        decimal GetAutoSequence();

        void DeleteGraph(string key);
    }
}
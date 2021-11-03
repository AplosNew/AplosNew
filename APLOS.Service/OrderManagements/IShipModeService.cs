#region Using

using Library.Model.OrderManagements;
using Library.Service.Core;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IShipModeService : IService<ShipMode>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
#region Using

using Library.Model.Productions;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Productions
{
    public interface IProductionStatusService : IService<ProductionStatus>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
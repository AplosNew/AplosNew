#region Using

using Library.Model.Productions;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Productions
{
    public interface IDMMService : IService<DMM>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
#region Using

using Library.Model.Machines;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Machines
{
    public interface IMachineActivityService : IService<MachineMaster>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
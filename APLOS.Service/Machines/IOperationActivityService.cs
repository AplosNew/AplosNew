#region Using

using Library.Model.Machines;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Machines
{
    public interface IOperationActivityService : IService<OperationActivity>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
#region Using

using Library.Model.Biometrics;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Biometrics
{
    public interface IShortLeaveAllocationService : IService<ShortLeaveAllocation>
    {
        void SaveData(ShortLeaveAllocation entity);
    }
}
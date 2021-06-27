#region Using

using Library.Core;
using Library.Model.Attendances;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Attendances
{
    public interface IBiometricDeviceAsShortLeaveService : IService<BiometricDeviceAsShortLeave>
    {
        GridModel Query(GridParameter parameters);
    }
}
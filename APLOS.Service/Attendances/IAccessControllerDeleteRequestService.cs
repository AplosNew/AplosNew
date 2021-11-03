#region Using

//using Library.Model.Biometics;
using Library.Model.Attendances;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Attendances
{
    public interface IAccessControllerDeleteRequestService : IService<AccessControllerDeleteRequest>
    {
        void InitData(AccessControllerEmployeeTag ui, ref List<AccessControllerDeleteRequest> from_db);
    }
}
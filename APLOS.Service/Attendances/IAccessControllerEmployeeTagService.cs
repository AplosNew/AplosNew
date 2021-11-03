#region Using

using Library.Core;
using Library.Model.Attendances;

//using Library.Model.Biometics;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Attendances
{
    public interface IAccessControllerEmployeeTagService : IService<AccessControllerEmployeeTag>
    {
        GridModel GetAllEmployee(GridParameter parameters, string plantId);

        IEnumerable<object> GetEmployeeRelatedDevices(string systemId);

        void SaveList(List<AccessControllerEmployeeTag> fromui);
        void InsertOrUpdateEmployeeDevice(IEnumerable<AccessControllerEmployeeTag> uilist, bool registerProximate, bool registerFP, string deviceId);
        void InsertOrUpdateGraph(IEnumerable<AccessControllerEmployeeTag> uilist, string empId, bool registerProximate, bool registerFP);
        IEnumerable<object> GetEmployeeDevicesList(string deviceId);
        void DeleteAndUpdateList(List<AccessControllerEmployeeTagDelete> fromui);


    }
}
#region Using

using Library.Model.Attendances;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Attendances
{
    public interface IAttdnRawDataFromAppService : IService<AttdnRawDataFromApp>
    {
        void SaveAttdnRawDataFromApp(AttdnRawDataFromApp entity);
        IEnumerable<object> GetAttnd(string EmpId);
        string SaveData(IEnumerable<AttdnRawDataFromApp> DataToSave);
    }
}
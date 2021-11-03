#region Using

using Library.Model.Attendances;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Attendances
{
    public interface IAttdnRawDataService : IService<AttdnRawData>
    {
        IEnumerable<object> AttendanceProximityInfo(string sPlantID, string sAttnDate);

        void SaveAttdnRawData(string plantid, string deviceid, string sMinDate, string sMaxDate, string _groupid, List<AttdnRawData> fromui);
    }
}
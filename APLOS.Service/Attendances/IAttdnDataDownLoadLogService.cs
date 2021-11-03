using Library.Model.Attendances;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Attendances
{
    public interface IAttdnDataDownLoadLogService : IService<AttdnDataDownLoadLog>
    {
        IEnumerable<object> AttendanceLogMaxDate(string sPlantID);

        void SaveAttdnDataDownLoadLog(string plantid, List<AttdnDataDownLoadLog> fromui);
    }
}
using Library.Model.Biometrics;
using Library.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Biometrics
{
    public interface IAttendanceDeviceZoneService : IService<AttendanceDeviceZone>
    {
        List<AttendanceDeviceZone> GetAllZone();
        List<AttendanceDeviceZone> GetSpecificZone(string ID);
        List<AttendanceDeviceZone> SearchSpecificZone(string strkey);

        void Save(AttendanceDeviceZone data);
        void Delete(string id);


    }
}

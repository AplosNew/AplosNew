using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.HumanResources
{
    public interface IRestDetailsService : IService<AttendanceRestDetail>
    {
        void DeleteDetail(string id);
        void InsertOrUpdateGraph(IEnumerable<AttendanceRestDetail> restDetailsList, string plantId, string restId, out List<AttendanceRestDetail> restDetailsDb_list);
    }
}
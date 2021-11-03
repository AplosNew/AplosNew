using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.HumanResources
{
    public interface ICompliedShiftGroupingService : IService<CompliedShiftGrouping>
    {
        void InsertOrUpdateGraph(CompliedShiftGrouping entity, IEnumerable<CompliedShiftGroupDetail> details);
        GridModel Query(GridParameter parameters, string companyGroupId, string plantId);
		IEnumerable<object> QueryDetail(string compliedShiftGroupId);
        GridModel QueryshiftDefination(GridParameter parameters, string groupId,string plantId);
        IEnumerable<object> GetCbo(string plantId);
        void DeleteGraph(string id);
        void DeleteDetail(string id);
    }
}
#region Using

using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface ICompliedShiftGroupDetailService : IService<CompliedShiftGroupDetail>
    {
        void InsertOrUpdate(IEnumerable<CompliedShiftGroupDetail> entity, string masterId);
        void DeleteWithMaster(string Id);
        void DeleteWithChild(string Id);

    }
}
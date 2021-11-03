#region Using

using Library.Model.Productions;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Productions
{
    public interface IProcessCapacityUOMService : IService<ProcessCapacityUOM>
    {
        IEnumerable<object> Query(string plantId);

        void InsertUpdateOrDeleteGraph(IEnumerable<ProcessCapacityUOM> entity, string plantId);

        void DeleteGraph(string plantId);
    }
}
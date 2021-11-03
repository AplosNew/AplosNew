using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    public interface IEntityOperationSettingsService : IService<EntityOperationSettings>
    {
        void InsertGraph(IEnumerable<EntityOperationSettings> entities);
        void UpdateGraph(IEnumerable<EntityOperationSettings> entities);
        IEnumerable<object> Query(string entityId);
    }
}
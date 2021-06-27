#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IServiceTypeService : IService<ServiceType>
    {
        decimal GetAutoSequence();

        GridModel Query(GridParameter parameters);

        IEnumerable<object> GetCbo();

        void Delete(string Id);
    }
}
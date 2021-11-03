using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IFGZoneService : IService<FGZone>
    {
        IEnumerable<object> GetFGZoneCbo();

        decimal GetAutoSequence();

        GridModel Query(GridParameter parameters);
    }
}
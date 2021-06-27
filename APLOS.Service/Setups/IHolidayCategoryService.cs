#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IHolidayCategoryService : IService<HolidayCategory>
    {
        GridModel QueryGraph(GridParameter parameters, string companyGroupId);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo(string companyGroupId);
    }
}
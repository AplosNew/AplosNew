using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IFGComponentService : IService<FGComponent>
    {
        /// <summary>
        /// Search for multiple add
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        GridModel GetFgComponentList(GridParameter parameters, string companyGroupId, string[] id);

        IEnumerable<object> GetFGComponentCbo(string companyGroupId);

        decimal GetAutoSequence();

        GridModel Query(GridParameter parameters);
    }
}
using Library.Core;
using Library.Model.Machines;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Machines
{
    public interface IThirdPartyOperationService : IService<ThirdPartyOperation>
    {
        /// <summary>
        /// GetThirdPartyList
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetCbo();

        void DeleteGraph(string key);

        GridModel Query(GridParameter parameters);
    }
}
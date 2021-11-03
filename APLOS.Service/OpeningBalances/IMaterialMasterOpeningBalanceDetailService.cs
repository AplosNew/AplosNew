using Library.Core;
using Library.Model.OpeningBalances;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.OpeningBalances
{
    public interface IMaterialMasterOpeningBalanceDetailService : IService<MaterialMasterOpeningBalanceDetail>
    {
        void DeleteGraph(string id);

        void Post(IEnumerable<MaterialMasterOpeningBalanceDetail> entities);

        void Park(IEnumerable<MaterialMasterOpeningBalanceDetail> entities);

        GridModel GetFixedAssetOpeningBalanceById(GridParameter parameters, string id, string companyId);

        GridModel Query(GridParameter parameters, string companyId);
    }
}
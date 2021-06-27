using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetRegisterCharacteristicsValueService : IService<FixedAssetRegisterCharacteristicsValue>
    {
        void InsertOrUpdateGraph(FixedAssetRegister fixedAssetRegister, IEnumerable<FixedAssetRegisterCharacteristicsValue> entity);

        IEnumerable<object> GetMaterialMasterCharacteristicsList(string materialMasterId, string registerId);

        GridModel GetCharacteristicsValueList(GridParameter parameters, string assignment, string mMasterId, string charateristicsId);
    }
}
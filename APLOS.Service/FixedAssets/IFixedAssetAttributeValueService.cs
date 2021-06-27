#region

using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;

#endregion

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetAttributeValueService : IService<FixedAssetAttributeValue>
    {
        decimal GetAutoSequence();

        GridModel Query(GridParameter parameters, string fixedAssetAttributeId);

        void Delete(string id);
    }
}
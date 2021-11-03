#region Using

using Library.Model.FixedAssets;
using Library.Service.Core;

#endregion Using

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetClassService : IService<FixedAssetClass>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
#region Using

using Library.Model.FixedAssets;
using Library.Service.Core;

#endregion Using

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetSubClassService : IService<FixedAssetSubClass>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
#region Using

using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.FixedAssets
{
    public interface ISubAssetTypeService : IService<SubAssetType>
    {
        GridModel Query(GridParameter parameters);
        decimal GetAutoSequence();
        IEnumerable<object> GetCbo();
    }
}
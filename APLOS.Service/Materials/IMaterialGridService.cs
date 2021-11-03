using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialGridService : IService<MaterialGrid>
    {
        IEnumerable<object> GetMaterialGridList();

        decimal GetAutoSequence();

        GridModel GetMaterialGridListWithoutExisting(GridParameter parameters, string companyGroupId, string[] ids);

        void Insert(MaterialGrid charater,
            IEnumerable<MaterialGridCharacteristics> materialGridCharacteristics, string[] deletedItems);

        void DeleteGraph(string key);

        GridModel Query(GridParameter parameters);
    }
}
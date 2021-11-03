using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialGridCharacteristicsService : IService<MaterialGridCharacteristics>
    {
        IEnumerable<object> GetCbo();

        IEnumerable<object> GetByMatrialGridList(string id);

        IEnumerable<object> Query(string materialGridId);

        void Insert(IEnumerable<MaterialGridCharacteristics> entity, string materialGridId, string[] deletedItems);

        void DeleteGraph(string materialGridId);
    }
}
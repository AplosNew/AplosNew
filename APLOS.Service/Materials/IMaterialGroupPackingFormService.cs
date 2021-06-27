using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialGroupPackingFormService : IService<MaterialGroupPackingForm>
    {
        void InsertOrUpdateGraph(IEnumerable<MaterialGroupPackingForm> entities, string masterId);

        IEnumerable<object> Query(string masterId);

        void DeleteGraph(string masterId);
    }
}
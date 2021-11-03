using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using Library.ViewModel.Materials;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialAttributeService : IService<MaterialAttribute>
    {
        decimal GetAutoSequence();
        IEnumerable<object> GetMaterialAttributeCbo(string groupId, string valueAssignment);
        GridModel GetMaterialAttributeData(GridParameter parameters);
        GridModel Query(GridParameter parameters);

        void InsertOrUpdate(MaterialAttributeViewModel viewModel);
        void DeleteGraph(string key);
    }
}

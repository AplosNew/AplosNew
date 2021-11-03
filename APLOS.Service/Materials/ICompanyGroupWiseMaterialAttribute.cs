using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface ICompanyGroupWiseMaterialAttributeService : IService<CompanyGroupMaterialAttribute>
    {
        IEnumerable<object> GetCompanyGroupWiseMaterialAttributeList();

        GridModel GetSearchData(GridParameter parameters);

        void DeleteGraph(string materialAttributeId);
    }
}
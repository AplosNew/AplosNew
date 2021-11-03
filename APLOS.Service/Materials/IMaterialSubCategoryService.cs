#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialSubCategoryService : IService<MaterialSubCategory>
    {
        IEnumerable<object> GetCbo(string companyGroupId);

        decimal GetAutoSequence();

        GridModel Query(GridParameter parameters, string companyGroupId);

        /// <summary>
        /// This cbo go to fabric roll management.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ComboModel> GetCboByMaterialMaster(string companyGroupId);
    }
}
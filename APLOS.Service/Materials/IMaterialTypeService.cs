#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IMaterialTypeService : IService<MaterialType>
    {
        IEnumerable<object> GetCbo();

        decimal GetAutoSequence();

        IEnumerable<object> GetMaterialTypeNatureListCbo();

        IEnumerable<object> GetCboFilterBySFG();

        //void InsertGraph(MaterialType entity, IEnumerable<MaterialTypeNature> materialTypeNatureList);

        //void UpdateGraph(MaterialType entity, IEnumerable<MaterialTypeNature> materialTypeNatureList);

        void DeleteGraph(string id);

        //IEnumerable<object> GetMaterialTypeNatureList(string masterId);

        /// <summary>
        /// This cbo go to fabric roll management.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ComboModel> GetCboByMaterialMaster(string companyGroupId);

        GridModel Query(GridParameter parameters);
    }
}
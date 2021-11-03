using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Materials
{
    public interface IMaterialMasterMachineProcessService : IService<MaterialMasterMachineProcess>
    {
        IEnumerable<object> GetDetailList(string masterId);

        GridModel GetMaterialMasterList(GridParameter parameters, string companyGroupId);

        void InsertGraph(string materialMasterId, IEnumerable<MaterialMasterMachineProcess> entities);

        void InsertUpdateOrDeleteGraph(string materialMasterId, string skillId, IEnumerable<MaterialMasterMachineProcess> entities, IEnumerable<MaterialMasterArticle> articleList);

        /// <summary>
        /// For material master
        /// </summary>
        /// <param name="materialMasterId"></param>
        /// <param name="entities"></param>
        void InsertUpdateDeleteGraph(string materialMasterId, IEnumerable<MaterialMasterMachineProcess> entities);

        void DeleteGraph(string materialMasterId);
    }
}
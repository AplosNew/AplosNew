using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Setups
{
    public interface IPlantConfigService : IService<PlantConfig>
    {
        PlantConfig GetPlantConfigByPlantId(string PlantId);

        GridModel GetMasterSearchData(GridParameter parameters);

        IEnumerable<object> GetCboList();

        GridModel GetPlantList(string CompanyId);

        GridModel GetProcessList();

        //void SaveMaster(PlantConfig from_ui, out string MasterID);
        void SaveMaster(PlantConfig from_ui, out string masterID, IEnumerable<PrdOrdSetting> prdOrdSetting);

        IEnumerable<PlantConfig> GetMaster(string Id);

        IEnumerable<object> GetMasterDataById(string MasterId);

        IEnumerable<object> GetPlantWiseDuplicateData(string Id, string CompanyGroupId, string CompanyId, string PlantId);

        void DeleteMaster(string masterid);

        IEnumerable<object> GetPlantConfigByPlant(string PlantId);

        GridModel Query(GridParameter parameters);
    }
}
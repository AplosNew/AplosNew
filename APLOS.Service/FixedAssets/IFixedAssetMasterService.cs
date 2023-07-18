using Library.Core;
using Library.Model.FixedAssets;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.FixedAssets
{
    public interface IFixedAssetMasterService : IService<FixedAssetMaster>
    {
        GridModel GetSearch(GridParameter parameters);

        IEnumerable<object> GetCbo();

        IWorkbook GetFixedAssetMaster();

        void InsertUpdateFixAssetMaster(FixedAssetMaster fixedAssetMaster);

        void DeleteItem(string masterId);

        GridModel GetFixedAssetDetermineByMasterId(GridParameter parameters, string assetMasterId);

        GridModel GetFixedAssetMasterDeterminateGL(GridParameter parameters, string companyId);

        IEnumerable<object> CheckMasterIsRegisterApplyByMasterId(string fxmasterId);

        GridModel Query(GridParameter parameters, string companyGroupId, string[] ids);

        GridModel QueryWithType(GridParameter parameters, string type);

        GridModel QueryWithTypeGl(GridParameter parameters, string type);

        GridModel QueryAsMaterialMaster(GridParameter parameters);

        GridModel GetMaterialMasterAssetTypeList(GridParameter parameters, string companyGroupId);
        GridModel GetFixedAssetMasterData(GridParameter parameters);
        IEnumerable<object> GetFixedAssetMasterPoPUpData();
        GridModel GetFAMISearch(GridParameter parameters);
        string GetFixedAssetMasterReport(string ReportHeader, string reportFileName,string CompanyGroupId);
        string GetFixedAssetMasterIndividualReport(string FAMId, string ReportHeader, string reportFileName, string CompanyGroupId);
        string GetFixedAssetMasterItemReport(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName,string PlantId);

    }
}
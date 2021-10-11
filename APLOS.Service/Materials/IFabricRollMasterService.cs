#region Using

using Library.Core;
using Library.Model.Materials;
using Library.Service.Core;
using Library.ViewModel.Materials;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Materials
{
    public interface IFabricRollMasterService : IService<FabricRollMaster>
    {
        void InsertOrUpdateGraph(IEnumerable<FabricRollMaster> entities);
        void UpdateFabricRoll(List<Dictionary<string, object>> FabricRollData, string PackingForm);
        void CreateRoll(int NoofRolls, Dictionary<string, object> SelectedRow,double Width, string PackingForm);
        
        int InsertOrUpdateGraphIncrement();
        IEnumerable<object> QueryList(string value);

        void UpdateFabricInitial(FabricRollMaster entity);

        void UpdateFabricInsPectionWithDefect(FabricRollMaster entity, FabricRollMasterDefect fabricRollMasterDefect);
        void UpdateFabricInsPection(FabricRollMaster entity);
        object QueryFriPlantConfigInfo();

        //  IEnumerable<object> GetCbo();
        IEnumerable<object> GetDefectCodeList();

        IEnumerable<FabricRollMasterDefectViewModel> QueryRollMasterDefectList(string value);
        GridModel Query(GridParameter parameters, string companyGroupId, string paidHours, string plantId);
        GridModel GetGRNList(GridParameter parameters, string fabricRoll);
        GridModel GetGRNDetailList(GridParameter parameters, string inventoryReceiveId, string fabricRoll);
        GridModel GetFABRollList(GridParameter parameters, string inventoryReceiveDetailId);
        IEnumerable<object> GetBarCideList(string inventoryReceiveDetailId);
    }
}
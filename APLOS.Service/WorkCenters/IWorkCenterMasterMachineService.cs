using Library.Core;
using Library.Model.WorkCenters;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.WorkCenters
{
    public interface IWorkCenterMasterMachineService : IService<WorkCenterMasterMachine>
    {
        GridModel GetSearchData(GridParameter parameters);

        IEnumerable<object> GetList(string masterid);

        void InsertORUpdatedetail(string masterid, string PlantId, IEnumerable<WorkCenterMasterMachine> from_ui);

        void DeleteMaster(string masterid);

        IEnumerable<object> GetDetailListByPlant(string plantid, string currentMasterId);
    }
}
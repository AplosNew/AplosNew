#region Using
using Library.Core;
using Library.Model.Productions;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Productions
{
    public interface IPlanningTypesService : IService<PlanningTypes>
    {
        void DeleteGraph(string Id);
        GridModel Query(GridParameter parameters);
        GridModel GetShiftList(GridParameter parameters, string sGroupID, string sPlantID, string[] ShiftDefinationIDs, string wcids);
    }
}
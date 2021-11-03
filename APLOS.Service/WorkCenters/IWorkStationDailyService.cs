using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.WorkCenters
{
    public interface IWorkStationDailyService : IService<WorkStationDaily>
    {
        GridModel GetMachineList(GridParameter parameters, string operationId, string processId);

        GridModel GetList(GridParameter parameters, string entityId, string workCenterId, string entryDate);

        GridModel GetOperationList(GridParameter parameters, string entity, string processId);

        void Delete(string id);

        IEnumerable<object> GetWorkStation(string entityId, string workcenterId);

        string GetPk();
    }
}
using Library.Model.Productions;
using Library.Service.Core;
using System;
using System.Collections.Generic;

namespace Library.Service.Productions
{
    public interface IMainProcessPlanningService : IService<MainProcessPlanning>
    {
        IEnumerable<object> GetList(string plantId, DateTime toDate, string companyId, string processId);

        IEnumerable<object> Process(string plantId, DateTime toDate, string companyId, string processId);

        void SaveFreezing(string[] idList);
    }
}
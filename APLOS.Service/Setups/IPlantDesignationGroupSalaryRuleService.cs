#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IPlantDesignationGroupSalaryRuleService : IService<PlantDesignationGroupSalaryRule>
    {
        void InsertORUpdate(IEnumerable<PlantDesignationGroupSalaryRule> entityList);

        void DeleteGraph(string plantId, string salaryRuleMasterId);

        IEnumerable<object> QueryGraph(string plantId, string salaryRuleMasterId);

        IEnumerable<object> GetSalaryRuleMasterWithPlantCbo(string plantId);

        GridModel QueryDesignationWithoutExisting(GridParameter parameters, string designationIds, string salaryRuleMasterId);

        IWorkbook GetDesignationMaster(string plantId);
    }
}
#region Using

using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IPlantDesignationGroupSalaryRuleDetailService : IService<PlantDesignationGroupSalaryRuleDetail>
    {
        void InsertORUpdate(string masterId, IEnumerable<PlantDesignationGroupSalaryRuleDetail> PlantDesignationGroupSalaryRuleDetail);

        void Delete(string Id);
    }
}
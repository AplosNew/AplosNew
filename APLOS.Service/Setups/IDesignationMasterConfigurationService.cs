#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface IDesignationMasterConfigurationService : IService<DesignationMasterConfiguration>
    {
        IEnumerable<object> GetLegalDesignationbyDesignationMaster(string designationMasterId);
        GridModel Query(GridParameter parameters);

        void InsertORUpdate(IEnumerable<DesignationMasterConfiguration> entityList);

        void Delete(string Id);

        IEnumerable<object> QueryGraph(string plantId, string companyGroupId);

        IEnumerable<object> QueryDesignation(string designationGroupId, string plantId, string companyGroupId);

        IEnumerable<object> GetLeavePolicyCbo(string plantId);

        IEnumerable<object> GetBonusPolicyMasterCbo(string plantId, string companyGroupId);

        IEnumerable<object> GetAttdnBonusPmtPolicyMasterCbo(string plantId);
        IEnumerable<object> GetBonusPolicyMonthlyRetainMasterCbo(string plantId);
        IEnumerable<object> GetSalaryRuleMstHead();

        IEnumerable<object> GetAttdnBonusPmtPolicyHead();

        IEnumerable<object> GetPFPolicyMasterCbo(string plantId);

        IEnumerable<object> GetESICPolicyMasterCbo(string plantId);
        IEnumerable<object> OverTimePmtPolicyMasterCbo(string plantId);
    }
}
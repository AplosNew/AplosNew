#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ISalaryFixationSettingService : IService<SalaryFixationSetting>
    {
        GridModel Query(GridParameter parameters);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();

        GridModel GetSalaryHeads(GridParameter parameters);

        GridModel GetSalaryHeadsAnCash(GridParameter parameters);

        GridModel GetLeaveTypes(GridParameter parameters);

        IEnumerable<object> GetSavedChildMasterWise(string salFixSetId);

        IEnumerable<object> GetAnnualCashChild(string salFixSetId);

        void Insert(SalaryFixationSetting entity, string companyGroupId);

        IEnumerable<object> GetNonCashChild(string salFixSetId);

        IEnumerable<object> GetSavedLeaveChild(string salFixSetId);

        GridModel GetAnnualNonCash(GridParameter parameters);

        void InsertOrUpdate(IEnumerable<SalaryFixationSettingDetails> leaveTypeList
            , IEnumerable<SalaryFixationSettingDetails> monthlyList
            , IEnumerable<SalaryFixationSettingDetails> annualCashList
            , IEnumerable<SalaryFixationSettingDetails> annualCashNonList, string salFixSetId);

        IEnumerable<SalaryFixationSettingDetails> GetleaveDataList(string salFixSetId);

        void DeleteMaster(string id);
    }
}
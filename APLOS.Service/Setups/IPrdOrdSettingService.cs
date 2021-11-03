using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Setups
{
    public interface IPrdOrdSettingService : IService<PrdOrdSetting>
    {
        IEnumerable<PrdOrdSetting> GetList(string groupId, string companyId, string plantId);

        void InsertOrUpdateGraph(IEnumerable<PrdOrdSetting> entities);
    }
}
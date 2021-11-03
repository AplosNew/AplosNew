using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Setups
{
    public interface IPrerecruitmentUrlService : IService<PrerecruitmentUrl>
    {
        List<Dictionary<string, object>> Query(string companyGroupId, string companyId);

        void Save(IEnumerable<PrerecruitmentUrl> entities);
    }
}
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.IEnumerable
{
    public interface IBulletinDetailService : IService<BulletinDetail>
    {
        IEnumerable<BulletinDetail> GetBulletinDetailList(string MasterId);

        IEnumerable<object> GetList(string companyGroupId, string masterId, string processId);

        string GetPK();
    }
}
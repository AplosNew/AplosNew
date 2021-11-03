#region Using

using Library.Core;
using Library.Model.External;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.External
{
    public interface IAplosEmpFieldTagService : IService<AplosEmpFieldTag>
    {
        GridModel Query(GridParameter parameters, int companyGroupId);

        void Insert(IEnumerable<AplosEmpFieldTag> entity);

        GridModel GetCompanyGroupCbo();
    }
}
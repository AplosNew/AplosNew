using Library.Core;
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.IEnumerable
{
    public interface ISubsectionStructureDetailService : IService<SubsectionStructureDetail>
    {
        GridModel GetSearchData(GridParameter parameters);

        SubsectionStructureDetail GetDetail(string PK);

        IEnumerable<SubsectionStructureDetail> GetDetailList(string MasterId);

        IEnumerable<object> GetList(string MasterId);

        string GetPK();
    }
}
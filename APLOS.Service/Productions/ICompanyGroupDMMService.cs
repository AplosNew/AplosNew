using Library.Core;
using Library.Model.Productions;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Productions
{
    public interface ICompanyGroupDMMService : IService<CompanyGroupDMM>
    {
        IEnumerable<object> GetCbo();

        void UpdateGraph(string dmmId, bool active);

        void DeleteGraph(string dmmId);

        GridModel Query(GridParameter parameters);
    }
}
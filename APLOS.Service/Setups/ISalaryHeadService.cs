using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Setups
{
    public interface ISalaryHeadService : IService<SalaryHead>
    {
        GridModel Query(GridParameter parameters, string companyGroupId);
        IEnumerable<object> GetSalaryHeadQuery();

        IEnumerable<object> GetLeaveTypeQuery();

        IEnumerable<object> QueryLocalLanguage();


        IEnumerable<object> GetSalaryHeadQueryWithLocalLanguage(string LanguageId, string flag);

        
    }
}
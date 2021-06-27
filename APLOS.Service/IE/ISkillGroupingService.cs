#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.IE
{
    public interface ISkillGroupingService : IService<SkillGrouping>
    {
        GridModel Query(GridParameter parameters);

        decimal GetAutoSequence();

        IEnumerable<object> GetCbo();

        IEnumerable<object> GetSkillgrouping();


        IEnumerable<object> GetDataBySkillGroupingId(string id);
        void Check(SkillGrouping model);
    }
}
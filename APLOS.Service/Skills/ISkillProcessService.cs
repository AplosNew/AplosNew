using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Skills
{
    public interface ISkillProcessService : IService<SkillProcess>
    {
        GridModel Query(GridParameter parameters, string skillId);

        void InsertUpdateOrDeleteGraph(string skillId, IEnumerable<SkillProcess> entity);

        void DeleteGraph(string skillId);
    }
}
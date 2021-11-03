#region Using

using Library.Model.Employees;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Skills
{
    public interface ISkillCategoryService : IService<SkillCategory>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
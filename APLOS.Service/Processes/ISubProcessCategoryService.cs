#region Using

using Library.Model.Processes;

using Library.Service.Core;

#endregion Using

namespace Library.Service.Processes
{
    public interface ISubProcessCategoryService : IService<SubProcessCategory>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
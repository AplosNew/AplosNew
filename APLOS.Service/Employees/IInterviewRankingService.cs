#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Employees
{
    public interface IInterviewRankingService : IService<InterviewRanking>
    {
        GridModel Query(GridParameter parameters);

        decimal GetAutoSequence();
    }
}
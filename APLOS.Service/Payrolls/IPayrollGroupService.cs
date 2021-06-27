#region Using

using Library.Model.Payrolls;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Payrolls
{
    public interface IPayrollGroupService : IService<PayrollGroup>
    {
        decimal GetAutoSequence();

        void DeleteGraph(string id);
    }
}
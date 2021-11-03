#region Using

using Library.Model.Employees;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Employees
{
    public interface ITempDocDashboardService : IService<TempDocDashboard>
    {
        void DataInsertInTemTable();
    }
}
using Library.Model.HumanResources;
using Library.Service.Core;

namespace Library.Service.HumanResources
{
    public interface ICompliedEmployeeRosterService : IService<CompliedEmployeeRoster>
    {
        void InsertOrUpdateRoster(CompliedEmployeeRoster entity);
    }
}
using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;

namespace Library.Service.Setups
{
    public interface IEmployeeWiseTermsAndConditionsService : IService<EmployeeWiseTermsAndConditions>
    {
        GridModel Query(GridParameter parameters);
    }
}
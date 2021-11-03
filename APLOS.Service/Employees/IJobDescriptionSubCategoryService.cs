#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IJobDescriptionSubCategoryService : IService<JobDescriptionSubCategory>
    {
        GridModel Query(GridParameter parameters, string companyGroupId);

        decimal GetAutoSequence(string companyGroupId);

        IEnumerable<object> GetCbo(string companyGroupId);
    }
}
#region Using

using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;
using Library.Model.Setups;
using Library.Core;

#endregion Using

namespace Library.Service.Employees
{
    public interface IRptConfigTemplateService : IService<RptConfigTemplate>
    {
        GridModel GetConfigTemplate(GridParameter parameters);
        IEnumerable<ComboModel> GetPlantCbo();
        IEnumerable<ComboModel> GetLanguageCbo();
    }



}
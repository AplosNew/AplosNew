#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface IDisciplinaryActionCategoryService : IService<DisciplinaryActionCategory>
    {
         //decimal GetAutoSequence();
        //GridModel Query(GridParameter parameters);      
        IEnumerable<object> GetCbo();

    }
}
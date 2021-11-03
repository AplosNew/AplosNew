using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Setups
{
    public interface IPlantWiseLetterTemplateService : IService<PlantWiseLetterTemplate>
    {
        IEnumerable<object> GetTermsAndConditionsByPreRecruitmentEmployee(string preRecruitmentEmployeeId);

        GridModel Query(GridParameter parameters, string plantId,string letterType);
    }
}
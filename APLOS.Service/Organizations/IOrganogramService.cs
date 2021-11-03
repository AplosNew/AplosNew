using System.Collections.Generic;

namespace Library.Service.Organizations
{
    public interface IOrganogramService
    {
        List<Dictionary<string, object>> GetList(string companyGroupId);
    }
}
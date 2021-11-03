#region Using

using Library.Model.Payrolls;
using Library.Service.Core;

#endregion Using

namespace Library.Service.Setups
{
    public interface ISalaryFixationSettingDetailsService : IService<SalaryFixationSettingDetails>
    {
        //void InsertORUpdate(IEnumerable<SalaryFixationSetting> entityList);
        //void Delete(string Id);
        //IEnumerable<object> QueryGraph(string plantId, string companyGroupId);
        //IEnumerable<object> GetSalaryHead(string companyGroupId);
    }
}
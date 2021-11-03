using Library.Model.Logs;
using Library.Service.Core;

namespace Library.Service.Invoices
{
    public interface ISyncSalesDataService : IService<SyncRegister>
    {
        void Sync(string companyGroupId, string companyId, string plantId, string entityId);
    }
}
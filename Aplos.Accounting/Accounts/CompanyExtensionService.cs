using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Accounting.Accounts
{
    public class CompanyExtensionService
    {
        private readonly ISqlRepository _sqlRepository;
        public CompanyExtensionService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        public Dictionary<string, object> GetCompanyConfiguration(string companyId)
        {
            var cmdText = @"SELECT Id,IsCostCenterApplicable,IsProfitCenterApplicable,IsVoucherFromBudget,IsBudgetPeriod,COAId,AddressMasterId,IsInventorySalesBook 
                            ,IsAccountModuleRunning,IsFixedAssetSalesBook,IsInboundInvoiceServiceApplicable FROM ORG.Company WHERE Id='"+ companyId + "' ";
            return _sqlRepository.GetData(cmdText);
        }
    }
}

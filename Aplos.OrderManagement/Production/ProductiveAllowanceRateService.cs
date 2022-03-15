using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.OrderManagement.Production
{
    public class ProductiveAllowanceRateService
    {
        ISqlRepository _sqlRepository;
        public ProductiveAllowanceRateService()
        {
            _sqlRepository = new SqlRepository();
        }

        
    }
}

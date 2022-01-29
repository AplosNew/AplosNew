using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;


namespace Library.OrderManagement.Production
{
    public class StocksAgeingReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        public StocksAgeingReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }


    } 

}




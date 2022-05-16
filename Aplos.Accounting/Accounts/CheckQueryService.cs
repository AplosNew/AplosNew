using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Banks;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Library.Accounting.Accounts
{
    public class CheckQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public CheckQueryService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
       
    }
}

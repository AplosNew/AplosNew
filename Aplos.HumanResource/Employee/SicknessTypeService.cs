#region LIB
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion LIB

namespace Library.HumanResource.Employee
{
   public class SicknessTypeService
    {
        private readonly SqlRepository _sqlRepository;
        #region const
        public SicknessTypeService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion const
    }
}

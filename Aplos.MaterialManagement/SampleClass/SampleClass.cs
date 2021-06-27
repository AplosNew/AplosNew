using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.MaterialManagement.SampleClass
{
    public class SampleClass
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        CustomIdentity identity;
        #region Constructor
        public SampleClass()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
            identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        }
        #endregion Constructor



        public string SampleMethod(string PlantId)
        {

            return PlantId;
        }
    }
}

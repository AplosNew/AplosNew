using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.NewAttendanceProcess { 
    public class NewAttendanceProcessService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        
        public NewAttendanceProcessService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }

    }    
}

#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Crosscutting.Security;
using System.Data;
using Library.Security.Core;
using System.Threading;
using Library.MaterialManagement.Material;
using System.Web;
using Newtonsoft.Json;
using Library.Service.Helpers;
using System.IO;
using Library.Core;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class CutPlanController : BaseController
    {
        #region Constructor
        
        private readonly ISqlRepository _sqlRepository;
        public CutPlanController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }
    }   
}

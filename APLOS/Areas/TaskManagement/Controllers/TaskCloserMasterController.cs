using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskCloserMasterController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        public TaskCloserMasterController(SqlRepository R)
        {
            _sqlRepository = R;
        }

        public ActionResult Aplos()
        {
            return View();
        }
    }
}
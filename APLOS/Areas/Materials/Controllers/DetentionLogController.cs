using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Materials
{
    public class DetentionLogController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        public DetentionLogController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        [Authorize, AllowAnonymous]
        public ActionResult Aplos()
        {
            return View();
        }
    }
}
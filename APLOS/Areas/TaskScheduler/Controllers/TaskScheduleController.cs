#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.TaskManagement;

#endregion

namespace Aplos.Areas.TaskScheduler.Controllers
{
    public class TaskScheduleController : BaseController
    {
        #region Constructor


        public TaskScheduleController()
        {

        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion


    }
}
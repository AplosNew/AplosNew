#region Using
using Aplos.Properties;
using Library.Core;
using Library.Model.Machines;
using Library.Service.Machines;
using System.Web.Mvc;
#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class ThirdPartyOperationController : Controller
    {
        #region Constructor
        /// <summary>   The skillCategoryService service. </summary>
        private readonly IThirdPartyOperationService _thirdPartyService;

        public ThirdPartyOperationController(IThirdPartyOperationService thirdPartyService)
        {
            this._thirdPartyService = thirdPartyService;
        }
        #endregion

        #region -- Pages
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_thirdPartyService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_thirdPartyService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetThirdParty(string id)
        {
            return Json(_thirdPartyService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ThirdPartyOperation thirdParty)
        {
            _thirdPartyService.Insert(thirdParty);
            return Json(new { ThirdPartyOperation = thirdParty, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ThirdPartyOperation thirdParty)
        {
            _thirdPartyService.Update(thirdParty);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _thirdPartyService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
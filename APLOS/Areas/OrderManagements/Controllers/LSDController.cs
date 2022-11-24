#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class LSDController : BaseController
    {
        #region Constructor
        /// <summary>   The lsdService service. </summary>
        private readonly ILSDService _lsdService;

        public LSDController(ILSDService lsdService)
        {
            _lsdService = lsdService;
        }
        #endregion

        #region -- Pages
      
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string buyerId)
        {
            return Json(_lsdService.Query(parameters, buyerId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLSDBuyerWise(string buyerid)
        {
            return Json(_lsdService.LsdList(buyerid), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(LSD lsd)
        {
            _lsdService.Insert(lsd);
            return Json(new { LSD= lsd, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(LSD lsd)
        {
            _lsdService.Update(lsd);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _lsdService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
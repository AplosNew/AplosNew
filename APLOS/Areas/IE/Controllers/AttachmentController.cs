#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.IE;
using Library.Model.Setups;
using Library.Service.IE;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IE.Controllers
{
    public class AttachmentController : BaseController
    {
        #region Constructor

        private readonly IAttachmentService _attachmentService;

        public AttachmentController(
            IAttachmentService attachmentService
            )
        {
            _attachmentService = attachmentService;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_attachmentService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_attachmentService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_attachmentService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Attachment model)
        {
            _attachmentService.Insert(model);
            return Json(new { SizeGroup = model, Sequence = _attachmentService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(Attachment model)
        {
            _attachmentService.Update(model);
            return Json(new { Sequence = _attachmentService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _attachmentService.Delete(id);
            return Json(new { Sequence = _attachmentService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
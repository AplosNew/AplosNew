#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Securites;
using Library.Service.Properties;
using Library.Service.Securites;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    public class ControlAdminController : BaseController
    {
        #region Constructor

        private readonly IControlAdminService _controlAdminService;
        private readonly IUserService _userService;

        public ControlAdminController(
            IControlAdminService controlAdminService
            , IUserService userService)
        {
            _userService = userService;
            _controlAdminService = controlAdminService;
        }

        #endregion Constructor

        [Authorize, HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_controlAdminService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(ControlAdmin controlAdmin)
        {
            if (_userService.Check(controlAdmin.UserId))
                throw new CustomException(string.Format(ResourcesCore.UsernameInvalid, controlAdmin.UserId));
            _controlAdminService.Insert(controlAdmin);
            return Json(new { ControlAdmin = controlAdmin, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ControlAdmin controlAdmin)
        {
            if (_userService.Check(controlAdmin.UserId))
                throw new CustomException(string.Format(ResourcesCore.UsernameInvalid, controlAdmin.UserId));
            _controlAdminService.Update(controlAdmin);
            return Json(new { ControlAdmin = controlAdmin, Message = AplosMessage.Updated });
        }

        #region Reset

        [HttpGet]
        public ActionResult Reset()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Reset(ControlAdmin controlAdmin)
        {
            _controlAdminService.PasswordChange(controlAdmin);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion Reset

        [Authorize, HttpGet]
        public ActionResult Get(string id)
        {
            var data = _controlAdminService.Find(id);
            if (data == null)
                throw new CustomException(Resources.ControlAdminNotFound);
            using (var embeddedTool = new EmbeddedTool())
            {
                data.Password = embeddedTool.Decrypt(data.Password);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        #region Change

        [HttpGet]
        public ActionResult Change()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Change(ControlAdmin clientAdmin)
        {
            _controlAdminService.PasswordChange(clientAdmin);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion Change

        [HttpPost]
        public JsonResult Delete(string userId)
        {
            _controlAdminService.Delete(userId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #region EncryptDecrypt

        [HttpGet]
        public ActionResult EncryptDecrypt()
        {
            return View();
        }

        [HttpGet]
        public string EncryptText(string txt)
        {
            using (var embeddedTool = new EmbeddedTool())
            {
                return embeddedTool.Encrypt(txt);
            }
        }

        [HttpGet]
        public string DecryptText(string decryptTxt)
        {
            using (var embeddedTool = new EmbeddedTool())
            {
                return embeddedTool.Decrypt(decryptTxt);
            }
        }

        #endregion EncryptDecrypt
    }
}
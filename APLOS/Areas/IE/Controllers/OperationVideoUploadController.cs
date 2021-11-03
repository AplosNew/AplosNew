using Library.Core;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Data;
using Library.Service.Materials;
using System.Web.Mvc;
using System.Web;
using System.IO;
using System;

namespace Aplos.Areas.IE.Controllers
{
    public class OperationVideoUploadController : Controller
    {
        #region Constructor

        /// <summary>   The CharacteristicsValueController service. </summary>
        private readonly ICharacteristicsValueService _characteristicsValueService;

        public OperationVideoUploadController(ICharacteristicsValueService characteristicsValueService)
        {
            this._characteristicsValueService = characteristicsValueService;
        }

        #endregion Constructor

        #region -- Pages

        /// <summary>
        /// Indexes this instance.
        /// </summary>
   
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize]
        public JsonResult GetOplerationVideoUploadControllerCbo()
        {
            return Json(new SelectList(_characteristicsValueService.GetCharacteristicsValueList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetOplerationVideoUploadListCbo()
        {
            return Json(new SelectList(_characteristicsValueService.GetCharacteristicsValueList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_characteristicsValueService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetOplerationVideoUploadController(string id)
        {
            return Json(_characteristicsValueService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult Create()
        {
            try
            {
                var file = System.Web.HttpContext.Current.Request.Files["file"];
                if (file.ContentLength > 0)
                {
                }

                HttpPostedFileBase vfile = Request.Files["Image"];
                //if (ModelState.IsValid)
                //{
                if (file != null && file.ContentLength > 0)
                {
                    string extension = Path.GetExtension(file.FileName);
                    if (extension.ToUpper() == ".MP4")
                    {
                        var _fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + Path.GetExtension(file.FileName);
                        file.SaveAs(Path.Combine(Server.MapPath("~/Images/Company/"), _fileName));
                    }
                    else
                        throw new Exception("Please upload .mp4 file only.");
                }
                // _companyService.Insert(company);
                ViewBag.Success = "Data saved successfully";
                return Json(new { Id = "007", Message = AplosMessage.Insert });
                //}
                //else
                //    ViewBag.Error = "Some fields are required.";
            }
            catch (Exception ex)
            {
                if (ex.InnerException.InnerException.Message.Contains("IX_Company_Code"))
                    ViewBag.Error = "This code(007) is already exists.";
                else if (ex.InnerException.InnerException.Message.Contains("IX_Company_Sequence"))
                    ViewBag.Error = "This Sequence(007) is already exists.";
                else
                    ViewBag.Error = ex.Message;
                throw new CustomException(Resources.RequiredFieldMessage);
            }
        }

        [HttpPost, Authorize]
        public JsonResult Edit(CharacteristicsValue characteristicsvalue)
        {
            if (ModelState.IsValid)
            {
                _characteristicsValueService.Update(characteristicsvalue);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _characteristicsValueService.Archive(id);
                return Json(new { Sequence = _characteristicsValueService.GetAutoSequence(null, null), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
            //if (!string.IsNullOrEmpty(id))
            //{
            //    _characteristicsValueService.Archive(id);
            //    return Json(new { });
            //}
            //else
            //    throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}
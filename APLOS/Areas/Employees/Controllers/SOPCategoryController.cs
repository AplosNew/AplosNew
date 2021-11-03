#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using System.Web;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.IO;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class SOPCategoryController : BaseController
    {
        #region Constructor
        /// <summary>   The SOPCategoryService service. </summary>
        private readonly ISOPCategoryService _SOPCategoryService;
        private readonly ICompanyGroupSOPCategoryService _companyGroupSOPCategoryService;

        public SOPCategoryController(
              ISOPCategoryService SOPCategoryService
            , ICompanyGroupSOPCategoryService companyGroupSOPCategoryService
            )
        {
            _SOPCategoryService = SOPCategoryService;
            _companyGroupSOPCategoryService = companyGroupSOPCategoryService;
        }
        #endregion

        #region -- Pages
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
            return Json(new SelectList(_companyGroupSOPCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupSOPCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_SOPCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {
            SOPCategory sopCategory = new JavaScriptSerializer().Deserialize<SOPCategory>(form["SOPCategory"]);

            _SOPCategoryService.Insert(sopCategory);

            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetSOPCategoryPath();
                string path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = _SOPCategoryService.GetSOPCategoryFile(sopCategory.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["FileId"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != sopCategory.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + sopCategory.Id + Path.GetExtension(item.FileName));
                    }
                }

            }
            
            return Json(new { SOPCategory = sopCategory, Sequence = _SOPCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        public JsonResult Edit(FormCollection form, HttpPostedFileBase[] file)
        {
            SOPCategory sopCategory = new JavaScriptSerializer().Deserialize<SOPCategory>(form["SOPCategory"]);

            _SOPCategoryService.Update(sopCategory);

            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetSOPCategoryPath();
                string path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = _SOPCategoryService.GetSOPCategoryFile(sopCategory.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["FileId"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != sopCategory.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + sopCategory.Id + Path.GetExtension(item.FileName));
                    }
                }

            }

            return Json(new { SOPCategory = sopCategory, Sequence = _SOPCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }


        public JsonResult Delete(string id)
        {
            var directory = ResourcesPathReader.GetSOPCategoryPath();
            string path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _SOPCategoryService.GetSOPCategoryFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _SOPCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _SOPCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion
    }
}
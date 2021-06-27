#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Helpers;
using System.IO;
using System.Web.Script.Serialization;
using System.Web;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class SOPSubCategoryController : BaseController
    {
        #region Constructor
        /// <summary>   The SOPSubCategoryService service. </summary>
        private readonly ISOPSubCategoryService _SOPSubCategoryService;
        private readonly ICompanyGroupSOPSubCategoryService _companyGroupSOPSubCategoryService;

        public SOPSubCategoryController(
              ISOPSubCategoryService SOPSubCategoryService
            , ICompanyGroupSOPSubCategoryService companyGroupSOPSubCategoryService
            )
        {
            _SOPSubCategoryService = SOPSubCategoryService;
            _companyGroupSOPSubCategoryService = companyGroupSOPSubCategoryService;
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
            return Json(new SelectList(_companyGroupSOPSubCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupSOPSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_SOPSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }


        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {
            SOPSubCategory sopSubCategory = new JavaScriptSerializer().Deserialize<SOPSubCategory>(form["SOPSubCategory"]);

            _SOPSubCategoryService.Insert(sopSubCategory);

            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetSOPSubCategoryPath();
                string path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = _SOPSubCategoryService.GetSOPSubCategoryFile(sopSubCategory.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["FileId"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != sopSubCategory.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + sopSubCategory.Id + Path.GetExtension(item.FileName));
                    }
                }

            }

            return Json(new { SOPSubCategory = sopSubCategory, Sequence = _SOPSubCategoryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        public JsonResult Edit(FormCollection form, HttpPostedFileBase[] file)
        {
            SOPSubCategory sopSubCategory = new JavaScriptSerializer().Deserialize<SOPSubCategory>(form["SOPSubCategory"]);

            _SOPSubCategoryService.Update(sopSubCategory);

            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetSOPSubCategoryPath();
                string path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = _SOPSubCategoryService.GetSOPSubCategoryFile(sopSubCategory.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["FileId"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["FileId"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != sopSubCategory.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + sopSubCategory.Id + Path.GetExtension(item.FileName));
                    }
                }

            }

            return Json(new { SOPSubCategory = sopSubCategory, Sequence = _SOPSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }


        public JsonResult Delete(string id)
        {
            var directory = ResourcesPathReader.GetSOPSubCategoryPath();
            string path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _SOPSubCategoryService.GetSOPSubCategoryFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["FileId"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["FileId"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _SOPSubCategoryService.DeleteGraph(id);
            return Json(new { Sequence = _SOPSubCategoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion
    }
}
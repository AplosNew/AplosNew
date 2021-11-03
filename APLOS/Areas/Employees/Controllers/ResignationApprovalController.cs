#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class ResignationApprovalController : BaseController
    {
        #region Constructor
        private readonly IResignationService _ResignationService;
        
        public ResignationApprovalController(
              IResignationService ResignationService        
            )
        {
            _ResignationService = ResignationService;
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
        //[AllowAnonymous]
        //public JsonResult GetCbo()
        //{
        //    return Json(new SelectList(_ResignationService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}


        //[HttpGet]
        //public ActionResult GetList(GridParameter parameters,string plantId)
        //{
        //    return Json(_ResignationService.ResignationQueryByPlantId(parameters,plantId), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult GetResignationList(GridParameter parameters, string plantId)
        {
            return Json(_ResignationService.ResignationApprovalQueryByPlantId(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetExperience(string EmpId,string reginDate)
        {
            int tYear = 0;
            int tMonth = 0;
           _ResignationService.GetExperience(EmpId,out tYear,out tMonth);
            //_ResignationService.getd
            return Json(new {DurationY= tYear,DurationM= tMonth, JsonRequestBehavior.AllowGet });
        }

        //public JsonResult GetList(GridParameter parameters)
        //{
        //    return Json(_ResignationService.Query(parameters), JsonRequestBehavior.AllowGet);
        //}


        [HttpPost]
        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {

            var pre = form["Resignation"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            Resignation reg = JsonConvert.DeserializeObject<Resignation>(pre, settings);
            

            var directory = new AppSettingsReader().GetValue("EMPLOYEEPROFILE", typeof(string)).ToString() + "Resignation" + "/"; //get pic url from web config
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string path = Path.Combine((Server.MapPath(directory)));


            string _id = "";
            _ResignationService.Save(reg, out _id);

            var fileName = "";
            var filedata = _ResignationService.GetFile(reg.Id);

            if (filedata.Count > 0)
            {
                if (
                    !string.IsNullOrEmpty(filedata["AttachLetter"].ToString()))
                    fileName = filedata["AttachLetter"].ToString();

                if (fileName != reg.AttachLetter)
                    if (System.IO.File.Exists(path + _id + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + _id + Path.GetExtension(fileName));
            }

            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + _id + Path.GetExtension(item.FileName));
                        item.SaveAs(path + _id + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { Resignation = reg, Message = AplosMessage.Success });
        }

       

        [HttpPost]
        public JsonResult Edit(Resignation model)
        {
            _ResignationService.Update(model);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _ResignationService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
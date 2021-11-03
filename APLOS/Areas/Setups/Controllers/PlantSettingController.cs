#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Setups;
using Library.Service.Helpers;
using Library.Service.Setups;
using Newtonsoft.Json;
using System.IO;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class PlantSettingController : BaseController
    {
        #region Constructor

        private readonly IPlantSettingService _plantSettingService;

        public PlantSettingController(IPlantSettingService plantSettingService)
        {
            _plantSettingService = plantSettingService;
        }

        #endregion Constructor

        
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_plantSettingService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_plantSettingService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form)
        {
            var pre = form["PlantSetting"];
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var plantSetting = JsonConvert.DeserializeObject<PlantSetting>(pre, settings);
            var file = Request.Files["file"];
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                {
                    plantSetting.AuthorizedSignature = Path.GetExtension(file.FileName);
                    if (!string.IsNullOrEmpty(plantSetting.AuthorizedSignature.ToString()))
                        plantSetting.AuthorizedSignature = file.FileName;
                }
                else
                    throw new CustomException(Resources.ImageUploadError);
            }
            _plantSettingService.Insert(plantSetting);
            if (file != null)
            {
                var path = Path.Combine(ResourcesPathReader.GetAuthorizedSignaturePath(), plantSetting.AuthorizedSignature);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                {
                    file.SaveAs(path);
                }
            }
            return Json(new { PlantSetting = plantSetting, Message = AplosMessage.Success });
        }
        [HttpPost]
        public JsonResult Edit(PlantSetting plantSetting)
        {
            _plantSettingService.Update(plantSetting);
            return Json(new { Sequence = _plantSettingService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            var directory = ResourcesPathReader.GetAuthorizedSignaturePath();
            var path = Path.Combine(directory);
            var authorizedSignature = "";
            var data = _plantSettingService.GetGetAuthorizedSignatureFile(id);
            if (data.Count > 0)
            {
                if (
                !string.IsNullOrEmpty(data["AuthorizedSignature"].ToString()))
                authorizedSignature = data["AuthorizedSignature"].ToString();
                if (System.IO.File.Exists(path + authorizedSignature))
                    System.IO.File.Delete(path + authorizedSignature);
            }

            _plantSettingService.Delete(id);

            return Json(new { Sequence = _plantSettingService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}
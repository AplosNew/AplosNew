#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Materials;
using Library.Service.Helpers;
using Library.Service.Materials;
using Library.ViewModel.Materials;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialMasterController : BaseController
    {
        #region -- Constructor

        private readonly IMaterialMasterService _materialMasterService;
        private readonly IMaterialMasterAlternativeUOMService _materialMasterAlternativeUOMService;
        private readonly IMaterialMasterProcessRoutingService _materialMasterProcessRoutingService;
        private readonly IMaterialMasterUsageService _materialMasterUsageService;
        private readonly IMaterialMasterAttributeValueService _materialMasterAttributeValueService;
        private readonly IMaterialAttributeValueService _materialValueService;
        private readonly IMaterialMasterCharacteristicsValueService _materialMasterCharacteristicsValueService;
        private readonly IMaterialMasterProcessSetService _materialMasterProcessService;
        private readonly IMaterialMasterMachineProcessService _assetItemProcessService;

        public MaterialMasterController(
            IMaterialMasterService materialMasterService
            , IMaterialMasterAlternativeUOMService materialMasterAlternativeUOMService
            , IMaterialMasterProcessRoutingService materialMasterProcessRoutingService
            , IMaterialMasterUsageService materialMasterUsageService
            , IMaterialMasterAttributeValueService materialMasterAttributeValueService
            , IMaterialMasterCharacteristicsValueService materialMasterCharacteristicsValueService
            , IMaterialMasterProcessSetService materialMasterProcessService
            , IMaterialMasterMachineProcessService assetItemProcessService
            , IMaterialAttributeValueService materialValueService
            )
        {
            _materialMasterService = materialMasterService;
            _materialMasterAlternativeUOMService = materialMasterAlternativeUOMService;
            _materialMasterProcessRoutingService = materialMasterProcessRoutingService;
            _materialMasterUsageService = materialMasterUsageService;
            _materialMasterAttributeValueService = materialMasterAttributeValueService;
            _materialMasterCharacteristicsValueService = materialMasterCharacteristicsValueService;
            _materialMasterProcessService = materialMasterProcessService;
            _assetItemProcessService = assetItemProcessService;
            _materialValueService = materialValueService;
        }

        #endregion -- Constructor

        #region Pages

      
        public ActionResult Aplos()
        {
            return View();
        }

     
        public ActionResult MaterialMasterReportPage()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public ActionResult GetMaterialUsedData(string masterId)
        {
            var PO = _materialMasterService.GetPOList(masterId);
            var GRN = _materialMasterService.GetGRNList(masterId);
            var BOM = _materialMasterService.GetBOMList(masterId);
            var CharacteristicsUsingInBOM = _materialMasterService.GetMaterialMasterCharacteristicsUsingInBOM(masterId);
            

            return Json(new { PO, GRN, BOM, CharacteristicsUsingInBOM }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAssetItemProcessByMaterialMaster(string materialMasterId)
        {
            return Json(_assetItemProcessService.GetDetailList(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult MaterialMasterSearch(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.MaterialMasterSearch(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// use in inventory (receive,issue)
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult MaterialSearchByBusinessProcess(GridParameter parameters, string type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.MaterialSearchByBusinessProcess(parameters, identity.CompanyGroupId, type), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetNonAssetMaterialList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_materialMasterService.GetNonAssetMaterialList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// use in btn recipe
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="materialType"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult GetMaterialListByMaterialType(GridParameter parameters, string materialType)
        {
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_materialMasterService.GetMaterialListByMaterialType(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(materialType)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialListByMaterialTypeBOM(GridParameter parameters, string materialType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.GetMaterialListByMaterialTypeBOM(parameters, identity.CompanyGroupId, materialType), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Use in Operation
        /// </summary>
        /// <param name="processIds"></param>
        /// <returns></returns>
        [Authorize]
        public JsonResult GetCommonMachineListByProcess(GridParameter parameters, string processIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.GetCommonMachineListByProcess(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(processIds)), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// use -(Commitment,)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult GetUomCboByMaterialMaster(string id)
        {
            return Json(_materialMasterService.GetUomCboByMaterialMaster(new JavaScriptSerializer().Deserialize<string[]>(id)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult MaterialMasterCbo(string materialmasterid)
        {
            return Json(_materialMasterService.GetMaterialMaster(materialmasterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetOurStyle(string materialmasterid)
        {
            return Json(_materialMasterService.GetOurStyle(materialmasterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMMDefaultSetting(string materialmasterid)
        {
            return Json(_materialMasterService.GetMMDefaultSetting(materialmasterid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterActiveItemPopUp(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.GetMaterialMasterActiveItemPopUp(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsByMaterialMasterId(string materialMasterId)
        {
            var charData = _materialMasterService.GetCharacteristicsByMaterialMasterId(materialMasterId);
            return Json(new { charData, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRevenueBudget(string materialMasterId)
        {
            return Json(_materialMasterService.GetRevenueBudget(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterAltUomList(string materialMasterId)
        {
            return Json(_materialMasterAlternativeUOMService.GetMaterialMasterAltUomList(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetValidation(string materialTypeId)
        //{
        //    return Json(_materialMasterService.ValidationByMaterialType(materialTypeId), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetProcessRoutingList(string materialMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterProcessRoutingService.GetProcessRoutingList(identity.CompanyGroupId, materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterUsage(string materialMasterId)
        {
            return Json(_materialMasterUsageService.Get(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMstProcess(string materialMasterId)
        {
            return Json(_materialMasterProcessService.Query(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMaster()
        {
            return Json(_materialMasterService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form
            // MaterialMaster materialMaster , IEnumerable<MaterialMasterAlternativeUOM> materialMasterAlternativeUOM
            //, IEnumerable<MaterialMasterAttribute> materialMasterAttribute, IEnumerable<MaterialMasterCharacteristics> materialMasterCharacteristics
            //, IEnumerable<MaterialMasterProcessRouting> materialMasterProcessRouting, IEnumerable<masterProcessSetList> masterProcessSetList, IEnumerable<MaterialMasterBusinessProcess> businessProcesses, IEnumerable<MaterialMasterRevenueBudget> revenuList, IEnumerable<AssetItemProcess> skillProcessList
            )
        {
            GetFormCollection(form, out var materialMasterAlternativeUOM
                , out var materialMasterAttribute, out var attributeValueList, out var materialMasterCharacteristics, out var characteristicsValue
                , out var materialMasterProcessRouting, out var masterProcessSetList
                , out var businessProcesses, out var revenuList
                , out var materialMaster, out var file);
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if ((extension.ToLower() == ".jpg" || extension.ToLower() == ".png") && !string.IsNullOrEmpty(file.FileName))
                    materialMaster.Image = ".jpg";
                else
                    throw new CustomException(Resources.ImageUploadError);
            }

            _materialMasterService.InsertGraph(materialMaster, materialMasterAlternativeUOM, materialMasterAttribute, attributeValueList, materialMasterCharacteristics
                        , characteristicsValue, materialMasterProcessRouting, masterProcessSetList, businessProcesses, revenuList);
            if (file != null)
            {
                //To Do change path
                var path = Path.Combine(ResourcesPathReader.GetMaterialsImagePath(), materialMaster.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                    file.SaveAs(path);
            }
            return Json(new { MaterialMaster = materialMaster, Sequence = _materialMasterService.GetAutoSequence(materialMaster.CompanyGroupId), Message = AplosMessage.Insert });
        }

        private void GetFormCollection(FormCollection form, out List<MaterialMasterAlternativeUOM> materialMasterAlternativeUOM
            , out IEnumerable<MaterialAttributeViewModel> materialMasterAttribute, out IEnumerable<MaterialAttributeValue> attributeValueList
            , out IList<MaterialMasterCharacteristics> materialMasterCharacteristics, out IEnumerable<CharacteristicsValue> characteristicsValue
            , out List<MaterialMasterProcessRouting> materialMasterProcessRouting, out List<MaterialMasterProcessSet> masterProcessSetList
            , out List<MaterialMasterBusinessProcess> businessProcesses, out List<MaterialMasterRevenueBudget> revenuList
            , out MaterialMaster materialMaster, out HttpPostedFileBase file)
        {
            materialMasterAlternativeUOM = null;
            materialMasterAttribute = null;
            attributeValueList = null;
            materialMasterCharacteristics = null;
            characteristicsValue = null;
            materialMasterProcessRouting = null;
            masterProcessSetList = null;
            businessProcesses = null;
            revenuList = null;
            materialMaster = new JavaScriptSerializer().Deserialize<MaterialMaster>(form["materialMaster"]);
            if (form["materialMasterAlternativeUOM"] != null)
                materialMasterAlternativeUOM = new JavaScriptSerializer().Deserialize<List<MaterialMasterAlternativeUOM>>(form["materialMasterAlternativeUOM"]);
            if (form["materialMasterAttribute"] != null)
                materialMasterAttribute = JsonConvert.DeserializeObject<IList<MaterialAttributeViewModel>>(form["materialMasterAttribute"]);
            if (form["attributeValueList"] != null)
                attributeValueList = JsonConvert.DeserializeObject<IList<MaterialAttributeValue>>(form["attributeValueList"]);
            //materialMasterAttribute = new JavaScriptSerializer().Deserialize<IEnumerable<MaterialMasterAttribute>>(form["materialMasterAttribute"]);
            if (form["materialMasterCharacteristics"] != null)
                materialMasterCharacteristics = JsonConvert.DeserializeObject<IList<MaterialMasterCharacteristics>>(form["materialMasterCharacteristics"]);
            if (form["characteristicsValue"] != null)
                characteristicsValue = JsonConvert.DeserializeObject<IList<CharacteristicsValue>>(form["characteristicsValue"]);
            if (form["materialMasterProcessRouting"] != null)
                materialMasterProcessRouting = new JavaScriptSerializer().Deserialize<List<MaterialMasterProcessRouting>>(form["materialMasterProcessRouting"]);
            if (form["masterProcessSetList"] != null)
                masterProcessSetList = new JavaScriptSerializer().Deserialize<List<MaterialMasterProcessSet>>(form["masterProcessSetList"]);
            if (form["businessProcesses"] != null)
                businessProcesses = new JavaScriptSerializer().Deserialize<List<MaterialMasterBusinessProcess>>(form["businessProcesses"]);
            if (form["revenuList"] != null)
                revenuList = new JavaScriptSerializer().Deserialize<List<MaterialMasterRevenueBudget>>(form["revenuList"]);
           
            file = Request.Files["file"];
        }

        [HttpPost]
        public JsonResult Edit(FormCollection form)
        {
            GetFormCollection(form, out var materialMasterAlternativeUOM
                , out var materialMasterAttribute, out var attributeValueList, out var materialMasterCharacteristics, out var characteristicsValue
                , out var materialMasterProcessRouting, out var masterProcessSetList
                , out var businessProcesses, out var revenuList
                , out var materialMaster, out var file);
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                {
                    if (!string.IsNullOrEmpty(file.FileName))
                        materialMaster.Image = materialMaster.Id + ".jpg";
                }
                else
                    throw new CustomException(Resources.ImageUploadError);
            }
            else
            {
                if (!string.IsNullOrEmpty(materialMaster.Image))
                {
                }
            }
            _materialMasterService.UpdateGraph(materialMaster, materialMasterAlternativeUOM, materialMasterAttribute, attributeValueList
                , materialMasterCharacteristics, characteristicsValue, materialMasterProcessRouting, masterProcessSetList, businessProcesses, revenuList);
            if (file != null && !string.IsNullOrEmpty(materialMaster.Image))
            {
                var path = Path.Combine(ResourcesPathReader.GetMaterialsImagePath(), materialMaster.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                    file.SaveAs(path);
            }
            return Json(new { Sequence = _materialMasterService.GetAutoSequence(materialMaster.CompanyGroupId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _materialMasterService.Archive(id);
            return Json(new { Sequence = _materialMasterService.GetAutoSequence(identity.CompanyId), Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetMaterialMasterDeterminateGL(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialMasterService.GetMaterialMasterDeterminateGL(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMaterialMasterWithFixedAssetMasterId(GridParameter parameters, string fixedAssetMasterId)
        {
            return Json(_materialMasterService.GetMaterialMasterWithFixedAssetMasterId(parameters, fixedAssetMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMaterialMasterWithFixedAssetMaster(GridParameter parameters)
        {
            return Json(_materialMasterService.GetMaterialMasterWithFixedAssetMaster(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetFixedAssetMasterBudgetTagForRegister(GridParameter parameters,string budgetMasterId,string activityId)
        {
            return Json(_materialMasterService.GetFixedAssetMasterBudgetTagForRegister(parameters, budgetMasterId, activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMaterialMasterAUCFixedAssetMaster(GridParameter parameters)
        {
            return Json(_materialMasterService.GetMaterialMasterAUCFixedAssetMaster(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult DeleteDocument(string id)
        {
            var directory = ResourcesPathReader.GetMaterialsImagePath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = _materialMasterService.GetDocFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                !string.IsNullOrEmpty(data["Image"].ToString()))
                    fileId = data["Id"].ToString();
                fileName = data["Image"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            _materialMasterService.UpdateDocument(id);

            return Json(new { Message = "File detach successfully." });
        }

        [HttpGet, Authorize]
        public ActionResult CheckMaterialCVUsingInBOM(string characteristicsValueId)
        {
            try
            {
                return Json(_materialMasterService.CheckMaterialCVUsingInBOM(characteristicsValueId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion -- Operations

        #region MaterialAttributeMaster

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterAttribute(string masterId)
        {
            return Json(_materialMasterService.GetMaterialMasterAttribute(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterAttributeList(string materialMasterId)
        {
            return Json(_materialMasterService.GetMaterialMasterAttributeList(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialValueSequence()
        {
            return Json(_materialMasterAttributeValueService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterAttributeValue(string masterId)
        {
            return Json(_materialMasterAttributeValueService.Query(masterId), JsonRequestBehavior.AllowGet);
        }

        #endregion MaterialAttributeMaster

        #region SKU

        [HttpPost]
        public JsonResult DeleteCharacteristicsValues(string id)
        {
            DeleteCharacteristicsValueData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteCharacteristicsValueData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM hkp.CharacteristicsValue WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterCharacteristics(string masterId)
        {
            return Json(_materialMasterService.GetMaterialMasterCharacteristics(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterCharacteristicsValue(string masterId)
        {
            return Json(_materialMasterCharacteristicsValueService.Query(masterId), JsonRequestBehavior.AllowGet);
        }

        #endregion SKU

        #region Material attribute(mst) and value

        [HttpPost, Authorize]
        public JsonResult MaterialAttributeMasterCreate(IEnumerable<MaterialAttributeMaster> materialAttributeMasters)
        {
            var controller = DependencyResolver.Current.GetService<MaterialAttributeMasterController>();
            //controller.ControllerContext = new ControllerContext(Request.RequestContext, controller);
            controller.Create(materialAttributeMasters);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult CreateValue(MaterialAttributeValue entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _materialValueService.InsertOrUpdate(entity);
            return Json(new { entity.Id, Sequence = _materialValueService.GetAutoSequence(entity.MaterialAttributeId, entity.MaterialMasterId), Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteValue(string valueId)
        {
            var data = _materialValueService.Query(t => t.Id == valueId).Select().FirstOrDefault();
            _materialValueService.Delete(data);
            return Json(new { Sequence = _materialValueService.GetAutoSequence(data.MaterialAttributeId, data.MaterialMasterId), Message = AplosMessage.Deleted });
        }

        #endregion Material attribute(mst) and value

        #region Report

        public ActionResult MaterialMasterReport(string materialTypeId, bool withSubmaterial)
        {
            var fileName = "Material Master Report " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _materialMasterService.GetMaterialMasterReport(materialTypeId, withSubmaterial);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult MaterialMasterReport2(string MaterialTypeId,bool Article )
        {
            try
            {
                _materialMasterService.MaterialMasterReport2(MaterialTypeId,Article);


                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        #endregion Report

       
    }
}
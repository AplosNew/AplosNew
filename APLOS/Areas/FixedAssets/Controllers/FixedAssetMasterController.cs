using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.FixedAssets;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
using Library.Security.Core;
using Library.Service.FixedAssets;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.FixedAssets.Controllers
{
    public class FixedAssetMasterController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFixedAssetMasterService _fixedAssetMasterService;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        public FixedAssetMasterController(IUnitOfWork unitOfWork, IFixedAssetMasterService fixedAssetMasterService, IPKGeneratorService pkGeneratorService, ISqlRepository sqlRepository)
        {
            _unitOfWork = unitOfWork;
            _fixedAssetMasterService = fixedAssetMasterService;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetMaster.cshtml");
        }

        [Authorize]
        public ActionResult FixedAssetMasterBudgetTag()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetMasterBudgetTag.cshtml");
        }

        [Authorize]
        public ActionResult FixedAssetMasterGL()
        {
            return View("~/Areas/FixedAssets/Views/FixedAssetMasterGL.cshtml");
        }
        
        public ActionResult AdditionalInfoUpdate()
        {
            return View("~/Areas/FixedAssets/Views/AdditionalInfoUpdate.cshtml");
        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_fixedAssetMasterService.GetSearch(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_fixedAssetMasterService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFixedAssetMasterDeterminateGL(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetMasterService.GetFixedAssetMasterDeterminateGL(parameters, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListForDynamicPopup(GridParameter parameters, string ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetMasterService.Query(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithMaterialMaster(GridParameter parameters)
        {
            return Json(_fixedAssetMasterService.QueryAsMaterialMaster(parameters), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Use In:Budget Master
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="budgetMasterId"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public ActionResult QueryAsMaterialMasterBudgetMaster(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fixedAssetMasterService.GetMaterialMasterAssetTypeList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListForDynamicPopupWithType(GridParameter parameters, string type)
        {
            return Json(_fixedAssetMasterService.QueryWithType(parameters, type), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListForDynamicPopupWithTypeGL(GridParameter parameters, string type)
        {
            return Json(_fixedAssetMasterService.QueryWithTypeGl(parameters, type), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFixedAssetDetermineByMasterId(GridParameter parameters, string fxmasterId)
        {
            return Json(_fixedAssetMasterService.GetFixedAssetDetermineByMasterId(parameters, fxmasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult CheckMasterIsRegisterApplyByMasterId(string fxmasterId)
        {
            return Json(_fixedAssetMasterService.CheckMasterIsRegisterApplyByMasterId(fxmasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FixedAssetMaster fixedAssetMaster)
        {
            _fixedAssetMasterService.InsertUpdateFixAssetMaster(fixedAssetMaster);
            return Json(new { FixedAssetMaster = fixedAssetMaster, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(FixedAssetMaster fixedAssetClass)
        {
            _fixedAssetMasterService.Update(fixedAssetClass);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _fixedAssetMasterService.DeleteItem(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public ActionResult FixedAssetMasterXls(string reportFileName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string fileName = "";
                fileName = _fixedAssetMasterService.GetFixedAssetMasterReport("", reportFileName, identity.CompanyGroupId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult FixedAssetMasterIndividualXls(string FAMId, string reportFileName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string fileName = "";
                fileName = _fixedAssetMasterService.GetFixedAssetMasterIndividualReport(FAMId, "", reportFileName, identity.CompanyGroupId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult FixedAssetMasterReport()
        {
            var fileName = "Fixed Asset Master Report " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _fixedAssetMasterService.GetFixedAssetMaster();
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, Authorize]
        public ActionResult GetFixedAssetMasterData(GridParameter parameters)
        {
            return Json(_fixedAssetMasterService.GetFixedAssetMasterData(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetFixedAssetMaster()
        {
            return Json(_fixedAssetMasterService.GetFixedAssetMasterPoPUpData(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateChild(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [MST].[FixedAssetItem] where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [MST].[FixedAssetItem] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FixedAssetItem", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update 
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpGet, Authorize]
        public ActionResult getFAMIlist(GridParameter parameters)
        {
            return Json(_fixedAssetMasterService.GetFAMISearch(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteFAMI(string Id)
        {
            try
            {

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[FixedAssetItem] where Id ='" + Id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public ActionResult FixedAssetMasterItemXls(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string fileName = "";
                fileName = _fixedAssetMasterService.GetFixedAssetMasterItemReport(data, "", reportFileName, identity.CompanyGroupId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region AdditionalInfo
        [HttpPost]
        public JsonResult CreateAdditional(List<Dictionary<string, object>> AdditionalList, string masterId)
        {
            try
            {
                SaveAdditionalData(AdditionalList, masterId);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }


        private void SaveAdditionalData(List<Dictionary<string, object>> AdditionalList, string masterId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsChild;
            int c = 0;
            try
            {
                #region AdditionalList 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[AssetItemAdditionalInfoMap] where  FixedAssetItemId='" + masterId + "'", out dsChild, false, "1");
                if (AdditionalList != null)
                {
                    foreach (var item in AdditionalList)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            c++;

                            item["Id"] = _pkGeneratorService.MakePK(masterId, c, 2);

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsChild);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet]
        public ActionResult GetAdditionalData(string masterId)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetAdditionalData(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAdditionalDataByAssetId(string masterId, string headerId)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetAdditionalDataByAssetId(masterId, headerId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region AdditionalInfoUpdate

        [HttpPost]
        public JsonResult CreateAdditionalInfoUpdate(Dictionary<string, object> data, List<Dictionary<string, object>> detailData, List<Dictionary<string, object>> SelectedAssetRegisterList)
        {
            try
            {
                SaveAdditionalInfoUpdate(data, detailData, out string Id, SelectedAssetRegisterList);
                return Json(new { Id, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        public void SaveAdditionalInfoUpdate(Dictionary<string, object> data, List<Dictionary<string, object>> detailData, out string Id, List<Dictionary<string, object>> SelectedAssetRegisterList)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
         
                DataSet dsMaster, dsDetail = null;
                DataSet dsAssetRegister = null;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [TRN].[AdditionalInfoUpdate] where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [TRN].[AdditionalInfoUpdate] where Id='" + data["Id"] + "'", out dsMaster, false, "1");


                string _Id = "", detailId = "";
                string id = string.Empty;
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AdditionalInfoUpdate", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update 

                #region Additionall Info Update Detail
                if (detailData != null)
                {
                    con.OpenDataSetThroughAdapter("select * from [TRN].[AdditionalInfoUpdateDetail]", out dsDetail, false, "1");
                    foreach (var item in detailData)
                    {
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AdditionalInfoUpdateDetail", out detailId);

                            item["Id"] = detailId;
                            item["AdditionalInfoUpdateId"] = _Id;

                            AddNewRow(dsDetail.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }


                #endregion Additionall Info Update Detail

                #region AssetRegister

                foreach (var item in SelectedAssetRegisterList)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }
                string mosql = "SELECT * FROM TRN.AssetRegister WHERE Id IN (" + id + ")";
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(mosql, out dsAssetRegister, false, "1");

                foreach (var item in SelectedAssetRegisterList)
                {
                    DataView dv = new DataView(dsAssetRegister.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["AdditionalInfoUpdateId"] = _Id;
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }
                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail, dsAssetRegister);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult GetAdditionallInfoUpdateData()
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetAdditionallInfoUpdateData(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteAdditionallInfoUpdate(string Id)
        {
            _unitOfWork.BeginTransaction();
            var vendorAdWr = new System.Text.StringBuilder();
            var vendorAdWrsql = "";

            vendorAdWrsql = "delete from [TRN].[AdditionalInfoUpdateDetail] Where AdditionalInfoUpdateId='" + Id + "'";
            vendorAdWr.Append(vendorAdWrsql);
            vendorAdWrsql = "delete from [TRN].[AdditionalInfoUpdate] Where Id='" + Id + "'";
            vendorAdWr.Append(vendorAdWrsql);

            _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
            _unitOfWork.SaveChanges();
            _unitOfWork.Commit();

            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public ActionResult GetAssetRegisterData(string fixedAssetItemId)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetAssetRegisterData(fixedAssetItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTaggedAssetRegisterData(string headerId)
        {
            FixedAssetQueryService _fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
            return Json(_fixedAssetQueryService.GetTaggedAssetRegisterData(headerId), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
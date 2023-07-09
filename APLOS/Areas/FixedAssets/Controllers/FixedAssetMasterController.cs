using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.FixedAssets;
using Library.Security.Core;
using Library.Service.FixedAssets;
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
        private readonly IFixedAssetMasterService _fixedAssetMasterService;

        public FixedAssetMasterController(IFixedAssetMasterService fixedAssetMasterService)
        {
            _fixedAssetMasterService = fixedAssetMasterService;
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

                con.OpenDataSetThroughAdapter("select * from [MST].[FixedAssetMasterItem] where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from [MST].[FixedAssetMasterItem] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FixedAssetMasterItem", out _Id);

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
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from [MST].[FixedAssetMasterItem] where Id = '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");


                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[FixedAssetMasterItem] where Id ='" + Id + "'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}
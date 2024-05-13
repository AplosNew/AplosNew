using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Setups.Controllers
{
    public class ServiceMasterController : BaseController
    {
        #region -- Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IServiceMasterService _serviceMasterService;

        public ServiceMasterController(IServiceMasterService serviceMasterService, ISqlRepository R)
        {
            this._serviceMasterService = serviceMasterService;
            _sqlRepository = R;
        }

        #endregion -- Constructor

        #region Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

      
        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters, string ids)
        {
            return Json(_serviceMasterService.Query(parameters, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceMasterList(GridParameter parameters)
        {
            return Json(_serviceMasterService.QueryServiceMaster(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetHSNCodeByServiceGroupId(string groupId)
        {
            var sql = @"SELECT Code FROM HKP.HSNCode WHERE Id =(SELECT HSNCodeId FROM [HKP].[ServiceGroup] WHERE Id='"+ groupId + "')";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_serviceMasterService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ServiceMaster serviceMaster)
        {
            _serviceMasterService.Insert(serviceMaster);
            return Json(new { ServiceMaster = serviceMaster, Sequence = _serviceMasterService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ServiceMaster serviceMaster)
        {
            _serviceMasterService.Update(serviceMaster);
            return Json(new { Sequence = _serviceMasterService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _serviceMasterService.Delete(id);
                return Json(new { Sequence = _serviceMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations

        #region Service Control

        public ActionResult ServiceControl()
        {
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult CreateServiceControlHeader(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [MST].[ServiceControl] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _detaliId = null;
                string _Id = "";
                bplib.clsGenID genid = new bplib.clsGenID();

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenerateIDYearly(DateTime.Now.ToString(), "ServiceControl", out _Id);

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

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [MST].[ServiceControl]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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
        public ActionResult DeleteServiceControl(string id)
        {
            string sqlChild = @"SELECT * FROM [HKP].[ServiceControlServiceMaster] WHERE ServiceControlId = '" + id + "'";
            string sql = @"SELECT * FROM [HKP].[ServiceControl] WHERE Id = '" + id + "'";
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[ServiceControlServiceMaster] WHERE ServiceControlId='" + id + "'");
                con.executeQuery("DELETE FROM [HKP].[ServiceControl] WHERE Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetServiceControlList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM [MST].[ServiceControl]) AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetServiceMasterList(string serviceControlId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                sql = @"SELECT  CheckBoxSelect=cast(CASE WHEN SC.ServiceMasterId<>'' THEN 1  ELSE 0 END as bit),
                                    SG.UserName AS ServiceGroup,SM.UserName ServiceMaster,SM.IsPO,SM.IsApproved,SC.BudgetLimit
                                    FROM [HKP].[ServiceMaster] SM
									 LEFT JOIN [HKP].[ServiceGroup] AS SG ON SG.Id=SM.ServiceGroupId
									left join(select * from  [MST].[ServiceControlServiceMaster] where ServiceControlId='" + serviceControlId + @"') SC on SC.ServiceMasterId=SM.Id
                                    --where SM.CompanyId='"+identity.CompanyId+"'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult CreateServiceControlServiceMaster(List<Dictionary<string, object>> data, string serviceControl, string TabName)
        {
            try
            {
                DataSet dsDr, dsCr;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string Id = "";
                #region data update

                con.OpenDataSetThroughAdapter("select * from [HKP].[ServiceControlServiceMaster] where ServiceControl='" + serviceControl + "'", out dsDr, false, "1");
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ServiceControlServiceMaster", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsDr.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ServiceControl"] = serviceControl;

                        AddNewRow(dsDr.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false && item["Id"].ToString() != null)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDr);

                #endregion data update 


                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public JsonResult CreateServiceControlActionBy(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsAB;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[ServiceControlActionBy] where ServiceControlId='" + GlManagementId + "'", out dsAB, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementActionBy", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsAB.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ActionById"] = item["SystemID"];
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsAB.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsAB);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public JsonResult CreateGlManagementApproveBy(List<Dictionary<string, object>> data, string GlManagementId)
        {
            try
            {
                DataSet dsAPB;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[GLManagementApproveBy] where GlManagementId='" + GlManagementId + "'", out dsAPB, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GLManagementApproveBy", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsAPB.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ApproveById"] = item["SystemID"];
                        item["GlManagementId"] = GlManagementId;

                        AddNewRow(dsAPB.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsAPB);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        #endregion
    }
}
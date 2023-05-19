#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Outsourcing.Controllers
{
    public class JWTransformationMasterController : BaseController
    {
        string TableName = "JWTransformationMaster";
        //authentication for
        //GetList Create Delete
        Library.MaterialManagement.JobWork.OSCommon JobWorkCommon = null;

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public JWTransformationMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from JWActivity where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize, HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT JWTM.Id, JWTM.Sequence, JWTM.JWActivityId, JWA.UserName JWActivity, JWTM.ResponsiblePersonId,ISNULL(JWTM.ProcessId,'') ProcessId
                               , Process.UserName Process,MM.UserName Material
                            ,EEI.EmployeeName ResponsiblePersonName, 
                            JWTM.OutputMaterialId, OUM.UserName OutputMaterial, JWTM.OutputMaterialUOMId,UOM.ShortName UOM, JWTM.RateApplicableOn, 
                            JWTM.CurrencyId, CURR.Code CURR, JWTM.MinRate, JWTM.MaxRate, 
                            JWTM.CycleTimeDays, JWTM.ByProductApplicable, JWTM.Remarks
                            FROM dbo.JWTransformationMaster JWTM
                            LEFT JOIN JWActivity JWA ON JWA.Id = JWTM.JWActivityId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JWTM.ResponsiblePersonId
                            LEFT JOIN JWItem OUM ON OUM.Id = JWTM.OutputMaterialId
                            LEFT JOIN [SCS].[UnitOfMeasurement] UOM  oN UOM.Id = JWTM.OutputMaterialUOMId
                            LEFT JOIN [HKP].[Process] Process  oN Process.Id = JWTM.ProcessId
 LEFT JOIN MST.MaterialMaster MM ON MM.Id = OUM.MaterialMasterId
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = JWTM.CurrencyId
                             WHERE " + strkey + " ";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetJobWorkActivityList()
        {
            string strSql = @"SELECT * FROM HKP.JobWorkActivity WHERE Type = '" + JobWorkType.Transformation.ToString() + "'";
            return Json(_sqlRepository.GetDataCollection(strSql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCurrencyList()
        {
            string strSql = @"SELECT C.Id CurrencyCode, C.Code AS Currency 
                                FROM scs.Currency C";
            return Json(_sqlRepository.GetDataCollection(strSql, null), JsonRequestBehavior.AllowGet);
        }
       

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> InputMaterialList, List<Dictionary<string, object>> ByProductList)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string JWTransformationId = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                con.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + " WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = "JWT" + _Id;
                    JWTransformationId = data["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    JWTransformationId = _Id;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }


                #region JW Transformation Input
                DataSet dsInputmaterial = null;
                string sql = "";
                string _inputMaterailId = "";
                sql = "SELECT * FROM JWInputMaterial WHERE JWTransformationMasterId = '" + JWTransformationId + "'";
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsInputmaterial, false, "1");

                if (InputMaterialList != null)
                {
                    for (int i = 0; i < dsInputmaterial.Tables[0].Rows.Count; i++)
                    {
                        var containsActivity = InputMaterialList.FirstOrDefault(x => x.ContainsKey("JWTransformationMasterId")).Values.Contains(dsInputmaterial.Tables[0].Rows[i]["JWTransformationMasterId"].ToString());
                        if (containsActivity)
                            continue;
                        else
                            dsInputmaterial.Tables[0].Rows[i].Delete();
                    }
                    for (int i = 0; i < InputMaterialList.Count; i++)
                    {
                        if (dsInputmaterial.Tables[0].DefaultView.Count == 0)
                        {
                            if (_inputMaterailId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("JWInputMaterial", out _inputMaterailId);
                                _inputMaterailId = "IM" + _inputMaterailId;
                            }
                            DataRow dr = dsInputmaterial.Tables[0].NewRow();
                            dr["Id"] = _inputMaterailId + "-" + (i + 1).ToString();

                            dr["JWTransformationMasterId"] = JWTransformationId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dsInputmaterial.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsInputmaterial.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["JWActivityId"] = bplib.clsWebLib.RetValidLen(InputMaterialList[i]["JWActivityId"]);

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dr.EndEdit();

                        }
                    }
                }
                #endregion


                #region JW Transformation ByProduct
                DataSet dsByProduct = null;
                sql = "SELECT * FROM JWByProduct WHERE JWTransformationMasterId = '" + JWTransformationId + "'";
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsByProduct, false, "1");
                // dsByProduct = dsInputmaterial;

                string _byProductId = "";

                if (ByProductList != null)
                {
                    for (int i = 0; i < dsByProduct.Tables[0].Rows.Count; i++)
                    {
                        var containsActivity = ByProductList.FirstOrDefault(x => x.ContainsKey("JWTransformationMasterId")).Values.Contains(dsByProduct.Tables[0].Rows[i]["JWTransformationMasterId"].ToString());
                        if (containsActivity)
                            continue;
                        else
                            dsByProduct.Tables[0].Rows[i].Delete();
                    }
                    for (int i = 0; i < ByProductList.Count; i++)
                    {
                        if (dsByProduct.Tables[0].DefaultView.Count == 0)
                        {

                            if (_byProductId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("JWByProduct", out _byProductId);
                                _byProductId = "BP" + _byProductId;
                            }
                            DataRow dr = dsByProduct.Tables[0].NewRow();
                            dr["Id"] = _byProductId + "-" + (i + 1).ToString();

                            dr["JWTransformationMasterId"] = JWTransformationId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsByProduct.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsByProduct.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();

                        }
                    }
                }
                #endregion


                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Data = data, Error = false, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Data = data, Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateInputMaterial(Dictionary<string, object> data)
        {
            try
            {
                if (string.IsNullOrEmpty(data["MaterialId"].ToString()))
                {
                    throw new Exception("Please select Material");
                }
                TableName = "JWInputMaterial";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string JWTransformationId = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                con.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + " WHERE ID='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = "IM" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    JWTransformationId = _Id;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateByProduct(Dictionary<string, object> data)
        {
            try
            {

                if(data["MaterialId"] == null)
                {
                    throw new Exception("Please select Material"); 
                }

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string JWTransformationId = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                con.OpenDataSetThroughAdapter("SELECT * FROM JWByProduct WHERE ID='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("JWByProduct", out _Id);
                    data["Id"] = "BP" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    JWTransformationId = _Id;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }




        public ActionResult Delete(string id)
        {
            // string sql = @"select * from [HKP].[HourlyLeaveReason] where CostingGroupId = '" + id + "'";


            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from JWByProduct where JWTransformationMasterId='" + id + "'");
                con.executeQuery("delete from JWInputMaterial where JWTransformationMasterId='" + id + "'");
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        [Authorize]
        public ActionResult DeleteByPrduct(string id)
        {
            // string sql = @"select * from [HKP].[HourlyLeaveReason] where CostingGroupId = '" + id + "'";


            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from  JWByProduct where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        [Authorize]
        public ActionResult DeleteInputMaterial(string id)
        {
            // string sql = @"select * from [HKP].[HourlyLeaveReason] where CostingGroupId = '" + id + "'";


            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from JWInputMaterial where  id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

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
                    dr[item] = bplib.clsWebLib.RetValidLen(sourceData[item]);
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
                    dr[item] = bplib.clsWebLib.RetValidLen(sourceData[item]);
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [Authorize, HttpPost]
        public ActionResult GetItemList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT JWI.Id,JWI.MaterialMasterId,JWI.ResponsiblePersonId,JWI.UOMId UOMId,JWI.Code
                            ,JWI.Sequence,JWI.ShortName,JWI.StandardName,JWI.UserName,JWI.Remarks,MM.UserName MaterialMaster
                            ,UOM.ShortName UOM,EEI.EmployeeName ResponsiblePersonName FROM JWItem JWI 
                            LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id = JWI.MaterialMasterId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = JWI.ResponsiblePersonId
                            LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id = JWI.UOMId WHERE " + strkey + " order by JWI.sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetInputMaterialItemList(string JWTransformationId)
        {
            string strkey = "1=1";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 
                            JWIM.Id,JWIM.JWTransformationMasterId
                            ,JWIM.UOMId,JWIM.MaterialId,ISNULL(JWIM.ResponsiblePersonId,'') ResponsiblePersonId
                            ,ISNULL(UOM.ShortName,'') UOM ,ISNULL(Material.UserName,'') Material,ISNULL(EEI.EmployeeName,'') ResponsiblePerson
                            ,ISNULL(MaterialSpecification,'') MaterialSpecification, ISNULL(WastagePercentage,0) WastagePercentage
                            , ISNULL(NetConsumptionOROutputUnit,0) NetConsumptionOROutputUnit, ISNULL(Rejection,0) Rejection
                            , ISNULL(ValueLoss,0) ValueLoss, ISNULL(GrossConsumption,0) GrossConsumption,Isnull(JWIM.Remarks,'') Remarks
                            from JWInputMaterial JWIM 
                            LEFT JOIN JWItem Material oN Material.Id = JWIM.MaterialId
                            LEFT JOIN [SCS].[UnitOfMeasurement] UOM  oN UOM.Id = JWIM.UOMId
                            LEFT JOIN EmployeeInformation EEI  oN EEI.SystemId = JWIM.ResponsiblePersonId                            
                            WHERE JWIM.JWTransformationMasterId = '" + JWTransformationId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetByProductItemList(string JWTransformationId)
        {
            string strkey = "1=1";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 
	                        JWBP.Id,JWBP.JWTransformationMasterId
	                        ,JWBP.UOMId,JWBP.MaterialId,JWBP.ResponsiblePersonId
	                        ,ISNULL(UOM.ShortName,'') UOM ,ISNULL(Material.UserName,'') Material
	                        ,ISNULL(EEI.EmployeeName,'') ResponsiblePerson
	                        ,ISNULL(MaterialSpecification,'') MaterialSpecification
	                        , ISNULL(WastagePercentage,0) WastagePercentage,ISNULl(Curr.Code,'') Currency,ISNULL(JWBP.CurrencyId,'')  CurrencyId
	                        , ISNULL(NetConsumptionOROutputUnit,0) NetConsumptionOROutputUnit, ISNULL(Rejection,0) Rejection
	                        , ISNULL(ValueLoss,0) ValueLoss, ISNULL(GrossQuantityOrInputUnit,0) GrossQuantityOrInputUnit
	                        ,Isnull(StandardRateORUnit,0) StandardRateORUnit ,Isnull(StandardQtyORInputUnit,0) StandardQtyORInputUnit,Isnull(JWBP.Remarks,'') Remarks 
	                        FROM JWByProduct JWBP 
                            LEFT JOIN JWItem Material oN Material.Id = JWBP.MaterialId
	                        left join scs.Currency Curr on Curr.Id = JWBP.CurrencyId
                            LEFT JOIN [SCS].[UnitOfMeasurement] UOM  oN UOM.Id = JWBP.UOMId
                            LEFT JOIN EmployeeInformation EEI  oN EEI.SystemId = JWBP.ResponsiblePersonId                            
                            WHERE JWBP.JWTransformationMasterId = '" + JWTransformationId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
    }
}
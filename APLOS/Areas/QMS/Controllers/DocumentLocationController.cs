#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.QMS.Controllers
{
    public class DocumentLocationController : BaseController
    {
        string TableName = "hkp.DocumentLocation";
        string TableName1 = "hkp.documentsublocation";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public DocumentLocationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from hkp.DocumentLocation where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                DataSet dsMaster1;
                DataSet dsMaster2;
                DataSet dsMaster3;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (data["Id"] == null)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code= '" + data["Code"] + "' ", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName= '" + data["UserName"] + "' ", out dsMaster1, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0 && dsMaster1.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Code and User Name already exists!!!");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Code already exists!!!");
                    if (dsMaster1.Tables[0].Rows.Count > 0)
                        throw new Exception("User Name already exists!!!");
                }

                else
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code= '" + data["Code"] + "' ", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code= '" + data["Code"] + "' and Id='" + data["Id"] + "' ", out dsMaster1, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName= '" + data["UserName"] + "' ", out dsMaster2, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName= '" + data["UserName"] + "' and Id='" + data["Id"] + "' ", out dsMaster3, false, "1");
                 
                    if (dsMaster.Tables[0].Rows.Count > 0 && dsMaster1.Tables[0].Rows.Count == 0)
                        throw new Exception("Code already exists!!!");

                    if (dsMaster2.Tables[0].Rows.Count > 0 && dsMaster3.Tables[0].Rows.Count == 0)
                        throw new Exception("User Name already exists!!!");

                }

                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same code already exists!!!");

                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same user name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0 && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "DL" + _Id;
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

        public ActionResult Delete(string id)
        {
            string sql = @"select * from [HKP].[DocumentLocation] where CostingGroupId = '" + id + "'";


            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where DocumentLocationId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Document SubLocation.");
                    }
                }


                // ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        // Sub Location

        [AllowAnonymous]
        public JsonResult GetCboDsl()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName1 + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDsl(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from hkp.documentsublocation where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetListDsl(string column, string value, string DocumentLocationId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM " + TableName1 + " where DocumentLocationId= '" + DocumentLocationId + "') AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetAutoSequenceDsl(string DocumentLocationId)
        {

            string sql = @"SELECT (ISNULL((MAX(ISNULL(Sequence,0))),0)+1) Sequence FROM hkp.documentsublocation Where DocumentLocationId='" + DocumentLocationId + "'";
            return Json(_sqlRepository.GetModelCollection<DocumentSubLocation>(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateDsl(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                DataSet dsMaster1;
                DataSet dsMaster2;
                DataSet dsMaster3;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (data["Id"] == null)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Code= '" + data["Code"] + "' and DocumentLocationId='" + data["DocumentLocationId"] + "' ", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where UserName= '" + data["UserName"] + "' and DocumentLocationId='" + data["DocumentLocationId"] + "' ", out dsMaster1, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0 && dsMaster1.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Code and User Name already exists!!!");
                    if (dsMaster.Tables[0].Rows.Count>0)
                        throw new Exception("Same Code already exists!!!");
                    if (dsMaster1.Tables[0].Rows.Count > 0)
                        throw new Exception("Same User Name already exists!!!");
                }

                else
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Code= '" + data["Code"] + "' and DocumentLocationId='" + data["DocumentLocationId"] + "'  ", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Code= '" + data["Code"] + "' and DocumentLocationId='" + data["DocumentLocationId"] + "'  and Id='" + data["Id"] + "' ", out dsMaster1, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where UserName= '" + data["UserName"] + "' and DocumentLocationId='" + data["DocumentLocationId"] + "'  ", out dsMaster2, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where UserName= '" + data["UserName"] + "' and DocumentLocationId='" + data["DocumentLocationId"] + "'  and Id='" + data["Id"] + "' ", out dsMaster3, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0 && dsMaster1.Tables[0].Rows.Count == 0)
                        throw new Exception("Code already exists!!!");

                    if (dsMaster2.Tables[0].Rows.Count > 0 && dsMaster3.Tables[0].Rows.Count == 0)
                        throw new Exception("User Name already exists!!!");
                }

                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same code already exists!!!");

                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same user name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0 && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "DSL" + _Id;
                    AddNewRowDsl(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRowDsl(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeleteDsl(string id)
        {
            string sql = @"select * from [HKP].[DocumentSubLocation] where CostingGroupId = '" + id + "'";


            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[DocumentSubLocation] WHERE Id='" + id + "' ");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }


        private void AddNewRowDsl(DataTable dt, Dictionary<string, object> sourceData)
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
        private void EditRowDsl(DataRow dr, Dictionary<string, object> sourceData)
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



    }
    public class DocumentSubLocation: BaseModel
        {

        #region Scalar Properties
        
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool Active { get; set; }
        #endregion Scalar Properties

        #region Audit Properties
        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}
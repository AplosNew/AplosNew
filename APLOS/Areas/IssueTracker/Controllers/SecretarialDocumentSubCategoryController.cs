#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.TaskManagement;
using System.Collections.Generic;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using System;
using Library.Data.Sql;

#endregion

namespace Aplos.Areas.IssueTracker.Controllers
{
    public class SecretarialDocumentSubCategoryController : BaseController
    {
        // string TableName = "";
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public SecretarialDocumentSubCategoryController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM hkp.SecretarialDocumentSubCategory"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetSDSubCagtegory(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from hkp.SecretarialDocumentSubCategory where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM hkp.SecretarialDocumentSubCategory) AS TEMP WHERE " + strkey + " order by sequence";



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
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from hkp.SecretarialDocumentSubCategory where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from hkp.SecretarialDocumentSubCategory where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from hkp.SecretarialDocumentSubCategory where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SecretarialDocumentSubCategory", out _Id);

                    data["Id"] = "SDSC" + _Id;
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
            string sql = @"select * from hkp.SecretarialDocumentSubCategory where Id = '" + id + "'";


            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from hkp.SecretarialDocumentSubCategory where id='" + id + "'");
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM hkp.SecretarialDocumentSubCategory");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        //Recurring
        [HttpPost, Authorize]
        public ActionResult CreateTaskSchedule(Dictionary<string, object> taskSchedule, string masterId)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select * from dbo.TaskSchedulerMaster where 1=2", out dsMaster, false, "1");

            string _Id = "";

            #region data update
            if (dsMaster.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("Dbo.TaskSchedulerMaster", out _Id);

                taskSchedule["Id"] = "TC" + _Id;
                AddNewRow(dsMaster.Tables[0], taskSchedule);

            }
            else
            {
                _Id = taskSchedule["Id"].ToString();
                EditRow(dsMaster.Tables[0].Rows[0], taskSchedule);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster);

            return Json(new { TaskSchedule = taskSchedule, Message = AplosMessage.Updated });

        }

        [HttpPost, Authorize]
        public ActionResult EditTaskSchedule(string auditTaskSchedulerMasterId, Dictionary<string, object> taskSchedule)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter(@"select * from Dbo.TaskSchedulerMaster where Id = '" + auditTaskSchedulerMasterId + "'", out dsMaster, false, "1");

            string _Id = "";

            #region data update
            if (dsMaster.Tables[0].Rows.Count == 0)
            {
                //bplib.clsGenID genid = new bplib.clsGenID();
                //genid.GenID("Dbo.TaskSchedulerMaster", out _Id);

                //taskSchedule["Id"] = "TC" + _Id;
                //AddNewRow(dsMaster.Tables[0], taskSchedule);
            }
            else
            {
                _Id = taskSchedule["Id"].ToString();
                EditRow(dsMaster.Tables[0].Rows[0], taskSchedule);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsMaster);

            return Json(new { TaskSchedule = taskSchedule, Message = AplosMessage.Updated });

        }




        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            //string strkey = "1=1";
            //if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
            //    strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from SecretarialDocumentRecurringSubCategory";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection(@"select * from SecretarialDocumentRecurringSubCategory where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}
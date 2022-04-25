using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Parties;
using Library.Security.Core;
using Library.Service.Parties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class DocumentCategoryController : BaseController
    {
        private readonly IBuyerService _buyerService;
        private SqlRepository _sqlRepository = new SqlRepository();
        public DocumentCategoryController(IBuyerService buyerService)
        {
            _buyerService = buyerService;
        }


        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetAutoSequence()
        //{
        //    return Json(_buyerService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetBuyer()
        {
            return Json(_buyerService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost, Authorize]
        //public JsonResult Create(Dictionary<string, object> data)
        //{
        //    try
        //    {
        //        DataSet dsMaster;
        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

        //        //con.OpenDataSetThroughAdapter("select * from HKP.DocumentCategory where Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
        //        //if (dsMaster.Tables[0].Rows.Count > 0)
        //        //    throw new Exception("Activity already exists!!!");

        //        con.OpenDataSetThroughAdapter("select * from HKP.DocumentCategory where Id='" + data["Id"] + "'", out dsMaster, false, "1");

        //        string _Id = "";
        //        #region data update
        //        if (dsMaster.Tables[0].Rows.Count == 0)
        //        {
        //            bplib.clsGenID genid = new bplib.clsGenID();
        //            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HKP.DocumentCategory", out _Id);
        //            // genid.GenID("EmpUnderstandingActivity", out _Id);
        //            _Id = "DC" + _Id;
        //            data["Id"] = _Id;

        //            AddNewRow(dsMaster.Tables[0], data);
        //        }
        //        else
        //        {
        //            _Id = data["Id"].ToString();
        //            EditRow(dsMaster.Tables[0].Rows[0], data);
        //        }
        //        #endregion data update
        //        clsStaticInfo _info = new clsStaticInfo();
        //        _info.SaveDataSets(dsMaster);
        //        return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Insert });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, Message = ex.Message });
        //    }
        //}


        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from HKP.DocumentCategory where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("UserName already exists!!!");

                con.OpenDataSetThroughAdapter("select * from HKP.DocumentCategory where   Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Code already exists!!!");



                con.OpenDataSetThroughAdapter("select * from HKP.DocumentCategory where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HKP.DocumentCategory", out _Id);

                    data["Id"] = "DC" + _Id;
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


                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Delete(string id)
        {
            string sql = @"select * from HKP.DocumentCategory where Id = '" + id + "'";
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from HKP.DocumentCategory where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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
            string sql = @"select top 100 * from (SELECT * FROM HKP.DocumentCategory) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
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
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.DocumentCategory");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
    }
}
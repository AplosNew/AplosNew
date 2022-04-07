using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
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

namespace Aplos.Areas.Materials.Controllers
{
    public class DetentionMasterController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public DetentionMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        public JsonResult StorageSql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_sqlRepository.GetDataCollection("select s.Id,s.PlantId,s.UserName as Storage from HKP.MaterialStorage s where s.plantId = '" + identity.PlantId + @"'"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetList(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM dbo.Rack where PlantId='" + plantId + "' order by sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> DetentionData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from DetentionMaster where Id<>'" + DetentionData["Id"] + "'", out DataSet dsDetentionMasterValidation, false, "1");

                //if (dsDetentionMaster.Tables[0].Rows.Count>0)
                //{
                //    throw new Exception("Code Already Exist.");
                //}
                
                DataSet dsDetentionMaster;

                 conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from DetentionMaster where Id='" + DetentionData["Id"] + "'", out dsDetentionMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsDetentionMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("DetentionMaster", out _Id);
                    _Id = "DM" + _Id;
                    DetentionData["Id"] = _Id;
                    AddNewRow(dsDetentionMaster.Tables[0], DetentionData);
                }
                else
                {
                    _Id = DetentionData["Id"].ToString();
                    EditRow(dsDetentionMaster.Tables[0].Rows[0], DetentionData);
                }
                #endregion data update


              
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDetentionMaster);

                return Json(new { Error = false, Data = DetentionData, Sequence = GetSequence(), Message = AplosMessage.Insert });

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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM  dbo.Rack ");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }


        [Authorize, HttpGet]
        public ActionResult LoadDetentionList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM DetentionMaster";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult LoadEditData(string DetentionID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
          
            string sql = @"select * from DetentionMaster where Id='" + DetentionID + @"'";
            return Json(new { detention=_sqlRepository.GetDataCollection(sql, null)}, JsonRequestBehavior.AllowGet);
        }


        //public ActionResult Delete(string RackID)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    string Deletesql = @"delete from Bin where RackId ='" + RackID + @"'";
        //    string Deletesql1 = @"delete from Rack where Id='" + RackID + @"'";
        //    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        //   // return Json(new { rack = _sqlRepository.GetDataCollection(Deletesql1, null), bin = _sqlRepository.GetDataCollection(Deletesql, null) }, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult Delete(string Id)
        {
            DeleteData(Id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string RackID)
        {
            string strSQL, strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strCSQL = @"delete from Bin where RackId ='" + RackID + @"'";
                strSQL = @"delete from Rack where Id='" + RackID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
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
        #endregion -- Operations
    }
}
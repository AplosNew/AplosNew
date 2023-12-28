#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.OrderManagement.ShipmentControl;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.OrderManagement.TermsAndConditions;
#endregion Using


namespace Aplos.Areas.OrderManagements.Controllers
{
    public class TermsAndConditionsController : BaseController
    {
        //abcd
        //this is my code from tarek
        string TableName = "hkp.TermsAndConditions";
        //authentication for
        //GetList Create Delete

        #region Constructor
        TermsAndConditionsService tg = new TermsAndConditionsService();
        private readonly ISqlRepository _sqlRepository;

        public TermsAndConditionsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor    

        ShipmentControl control = new ShipmentControl();
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult Group()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(tg.GetCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = tg.Get(Id);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value,string CompanyId)
        {
            return Json(tg.GetList(column, value, CompanyId), JsonRequestBehavior.AllowGet);
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
                string ret = tg.Create(data);
                if (ret == "Success")
                {
                    return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });
                }
                else
                {
                    return Json(new { Error = true, Message = ret });
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        public ActionResult Delete(string id)
        {
            try
            {

                string ret = tg.Delete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }
        
        public ActionResult DeleteTitle(string id)
        {
            try
            {

                string ret = tg.DeleteTitle(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        public ActionResult DeletePopup(string id)
        {
            try
            {

                string ret = tg.DeletePopUp(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }


        [HttpPost]
        public JsonResult SaveData(Dictionary<string, object> GridData, string titleId)
        {
            try
            {

                DataSet dsGrid;

                ConnectionManager.DAL.ConManager conBin = new ConnectionManager.DAL.ConManager("1");
                conBin.OpenDataSetThroughAdapter("select top 1 Sequence from dbo.TermsAndConditionsDetails where TermsAndConditionsChildId='" + titleId + "' order by AddedDate desc", out DataSet dsGridSeq, false, "1");
                conBin.OpenDataSetThroughAdapter("select * from dbo.TermsAndConditionsDetails where TermsAndConditionsChildId='" + titleId + "'", out dsGrid, false, "1");
                string DetailId = "";
                int count = 0;
                int seq=0;
                DataView dv = new DataView(dsGrid.Tables[0]);
                dv.RowFilter = "Id='" + GridData["Id"] + "'";

                if (dv.Count == 0)
                {
                    if (DetailId == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.TermsAndConditionsDetails", out DetailId);
                    }
                    if (dsGridSeq.Tables[0].Rows.Count==0)
                    {
                        count++;
                    }
                    else
                    {
                        count =(int) clsStaticInfo.dbl( dsGridSeq.Tables[0].Rows[0]["Sequence"].ToString())+1;
                    }
                    DataRow dr = dsGrid.Tables[0].NewRow();

                    GridData["Id"] = "TD-" + DetailId;
                    GridData["TermsAndConditionsChildId"] = titleId;
                    GridData["Sequence"] = count;

                    AddNewRow(dsGrid.Tables[0], GridData);
                }
                else
                {
                    DataRow drmo = dv[0].Row;
                    EditRow(drmo, GridData);
                }




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsGrid);


                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }


        [HttpPost]
        public JsonResult SaveTitle(Dictionary<string, object> TitleData, string TitleId)
        {
            try
            {
                ConnectionManager.DAL.ConManager conTitle = new ConnectionManager.DAL.ConManager("1");
                conTitle.OpenDataSetThroughAdapter("select * from dbo.TermsAndConditionsChild where TermsAndConditionsMasterId='" + TitleData["TermsAndConditionsMasterId"] + "' and Id='" + TitleData["Id"] + "' ", out DataSet dsTitle, false, "1");
                string _Id = "";

                //conTitle.OpenDataSetThroughAdapter("select * from  dbo.TermsAndConditionsChild where Title='" + TitleData["Title"] + "' AND  Id<>'" + TitleData["Id"] + "'", out DataSet dsValid, false, "1");
                //if (dsValid.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same title already exists!!!");

                #region data update
                if (dsTitle.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("dbo.TermsAndConditionsChild", out _Id);
                    _Id = "TC" + _Id;
                    TitleData["Id"] = _Id;
                    AddNewRow(dsTitle.Tables[0], TitleData);
                }
                else
                {
                    _Id = TitleData["Id"].ToString();
                    EditRow(dsTitle.Tables[0].Rows[0], TitleData);
                }
                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTitle);
                //_info.SaveDataSets(dsTitle);

                return Json(new { Error = false, Data = TitleData, Message = AplosMessage.Insert });

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
        public JsonResult GetPopUp(string TermsAndConditionsDetailId)
        {
            try
            {
                return Json(control.GetTermsAndConditionPopUp(TermsAndConditionsDetailId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpGet, Authorize]
        public JsonResult GetTitle(string masterID)
        {
            try
            {
                return Json(control.Title(masterID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpPost, Authorize]
        public ActionResult UpdateMaterialSequence(List<string> data)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data changed!!!");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                for (int i = 0; i < data.Count; i++)
                {
                    con.executeQuery("UPDATE TermsAndConditionsDetails SET Sequence=" + (i + 1) + " where id='" + data[i] + "'");
                }

                con.CommitTransaction();

                return Json(new { Error = false, Message = "Sequence updated successfully" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetGroupList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM dbo.TermsandConditionGroup) AS TEMP WHERE " + strkey + " order by sequence";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateGroup(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.TermsandConditionGroup where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from dbo.TermsandConditionGroup where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

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

                return Json(new { Error = false, Data = data, Sequence = GetGroupAutoSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public JsonResult GetGroupAutoSequence()
        {
            return Json(GetGroupSequence(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteGroup(string id)
        {
            string sql = @"select * from dbo.TermsandConditionGroup where Id = '" + id + "'";

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.TermsandConditionGroup where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetGroupAutoSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        private double GetGroupSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM dbo.TermsandConditionGroup");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public ActionResult GetGroupNameList()
        {
             string sql = "SELECT Id Value, UserName Text FROM dbo.TermsandConditionGroup";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

    }
}
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
    public class RackController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public RackController(ISqlRepository R)
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


        [HttpPost]
        public JsonResult Create(Dictionary<string, object> RackData, List<Dictionary<string, object>> BinData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from dbo.Rack where Id<>'" + RackData["Id"] + "' AND Code='"+ RackData["Code"] + "'", out DataSet dsRackValidation, false, "1");

                if (dsRackValidation.Tables[0].Rows.Count>0)
                {
                    throw new Exception("Code Already Exist.");
                }
                
                DataSet dsRack;

                 conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from dbo.Rack where Id='" + RackData["Id"] + "'", out dsRack, false, "1");

                if (RackData["StorageLocationId"] == null)
                {
                    throw new Exception("Please Select Storage Location");
                }

                string _Id = "";

                #region data update
                if (dsRack.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("dbo.Rack", out _Id);
                    _Id = "R" + _Id;
                    RackData["Id"] = _Id;
                    AddNewRow(dsRack.Tables[0], RackData);
                }
                else
                {
                    _Id = RackData["Id"].ToString();
                    EditRow(dsRack.Tables[0].Rows[0], RackData);
                }
                #endregion data update


                DataSet dsBin;

                ConnectionManager.DAL.ConManager conBin = new ConnectionManager.DAL.ConManager("1");
                conBin.OpenDataSetThroughAdapter("select * from dbo.Bin where RackId='" + _Id + "'", out dsBin, false, "1");

                string binId = "";
                for (int i = 0; i < BinData.Count; i++)
                {
                    dsBin.Tables[0].DefaultView.RowFilter = "Id='" + BinData[i]["Id"] + @"'";
                    if (dsBin.Tables[0].DefaultView.Count > 0)
                    {
                        //edit
                        DataRow dr = dsBin.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["UserName"] = BinData[i]["UserName"];                      
                        dr.EndEdit();
                    }
                    else
                    {
                        //addnew
                        if (binId == "")
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("dbo.Bin", out binId);
                        }
                        DataRow dr = dsBin.Tables[0].NewRow();

                        dr["Id"] = "B-" + binId + "-" + (i + 1);
                        dr["RackId"] = _Id;
                        dr["Code"] = BinData[i]["Code"];
                        dr["Row"] = BinData[i]["Row"];
                        dr["Column"] = BinData[i]["Column"];
                        dr["UserName"] = BinData[i]["UserName"];

                        dsBin.Tables[0].Rows.Add(dr);

                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRack, dsBin);

                return Json(new { Error = false, Data = RackData, Sequence = GetSequence(), Message = AplosMessage.Insert });

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
        public ActionResult LoadRackList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select r.*,s.UserName StorageLocation ,b.TotalBin
from Rack r 
left outer join 
(
select COUNT(Id) TotalBin,RackId from Bin 
group by RackId
)b on b.RackId=r.Id
left outer join hkp.MaterialStorage s on s.Id=r.StorageLocationId";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult LoadEditData(string RackID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select  * from Bin where RackId='" + RackID +@"'";
            string sql1 = @"select  * from Rack where Id='" + RackID +@"'";
            return Json(new { rack=_sqlRepository.GetDataCollection(sql1, null),bin= _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
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
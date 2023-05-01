using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Security.Core;
using Library.Service.EmployeeServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class WebBasedPackingController : Controller
    {
        private readonly ISqlRepository _sqlRepository;

        public WebBasedPackingController(SqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        // GET: Packing/WebBasedPacking
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetEntity()
        {
            string sqlEntity = @"select Id Value, UserName Text from ORG.Entity where Id !=111";
            return Json(_sqlRepository.GetDataCollection(sqlEntity), JsonRequestBehavior.AllowGet);
        }

        public JsonResult FromLoc(string Entity, string Purpose)
        {
            try
            {
                var _sql = @"select distinct m.FromLocation as Text
                from mst.MaterialMovementMaster m
                where PurposeId='" + Purpose + "' and EntityId='" + Entity + "'";
                return Json(_sqlRepository.GetDataCollection(_sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult ToLoc(string Entity, string Purpose, string FromLoc)
        {
            try
            {
                var sql = @"select distinct m.ToLocation as Text,m.Id as Value
                from mst.MaterialMovementMaster m
                where PurposeId='" + Purpose + "' and EntityId='" + Entity + "' and FromLocation='" + FromLoc + "'";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult GetPurpose(string Entity)
        {
            try
            {
                var _sql = @"select distinct PurposeId as Value,mp.UserName as Text 
                from mst.MaterialMovementMaster m
                left join hkp.MaterialMovementPurpose mp on mp.Id=m.PurposeId
                where m.EntityId='" + Entity + "'and mp.Active='1'";
                return Json(_sqlRepository.GetDataCollection(_sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult Save(Dictionary<string, object> datas)
        {
            try
            {

                string TableName = "dbo.ItemScan";
                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + datas["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenIDYearly(DateTime.Now.ToShortDateString(), "Item Scan", out _Id);

                    //genid.GenID(TableName, out _Id);

                    datas["Id"] = _Id;

                    AddNewRow(dsMaster.Tables[0], datas);
                }
                else
                {
                    _Id = datas["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Data = datas, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public string SaveHeader(IEnumerable<ItemScanData> DataToSave,  Dictionary<string, object> datas)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<ItemScanData> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.ItemScan where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (ItemScanData item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID id = new bplib.clsGenID();
                        id.GenIDYearly(DateTime.Now.ToShortDateString(), "Item Scan", out string NewId);



                        dr["Id"] = NewId;
                        dr["WorkDate"] = item.WorkDate;
                        dr["Time"] = item.Time;
                        dr["ShiftId"] = item.ShiftId;
                        dr["Grade"] = item.Grade;
                        dr["LocMasterId"] = item.LocMasterId;
                        dr["PurposeId"] = item.PurposeId;
                        dr["Remarks"] = item.Remarks;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dsMaster.Tables[0].Rows.Add(dr);


                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["WorkDate"] = item.WorkDate;
                        dr["Time"] = item.Time;
                        dr["ShiftId"] = item.ShiftId;
                        dr["Grade"] = item.Grade;
                        dr["LocMasterId"] = item.LocMasterId;
                        dr["PurposeId"] = item.PurposeId;
                        dr["Remarks"] = item.Remarks;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = DateTime.Now.ToString();

                        dr.EndEdit();
                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MasterId;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public JsonResult GetShiftMaster()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"SELECT distinct SystemID as Value,UserName AS Text FROM [dbo].[ShiftDefination] where isnull(PlantID,'')='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(_sql, null), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                throw;
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
            dr["AddedDate"] = DateTime.Now.ToString();
            //dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
           // dr["UpdatedFromIP"] = identity.IPAddress;

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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        public void Scanner_Clicked()
        {
            try
            {
                
                SerialPort scanner = new SerialPort("RS-232");

                scanner.Open();
                string command = "...";
                scanner.Write(command);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
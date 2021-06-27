#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Costings.Controllers
{
    public class BOQCriteriaController : BaseController
    {
        //authentication for
        //GetList Create


        #region Constructor
        string TableName = "HKP.BOQCriteria";
        private readonly ISqlRepository _sqlRepository;
        public BOQCriteriaController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }
    

        [HttpPost, Authorize]
        public ActionResult GetList()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM " + TableName;
            DataTable dt = _sqlRepository.GetDataTable(sql);
            int Sequence = 0;
            foreach (BOQCriteria _data in Enum.GetValues(typeof(BOQCriteria)))
            {
                Sequence++;
                dt.DefaultView.RowFilter = "Criteria='" + _data.ToString() + "'";
                if (dt.DefaultView.Count == 0)
                {
                    DataRow dr = dt.NewRow();
                    dr["Criteria"] = _data.ToString();
                    dr["UserName"] = _data.ToString();
                    dr["Sequence"] = Sequence.ToString();
                    dr["Active"] = true;
                    dt.Rows.Add(dr);

                }

            }
            dt.DefaultView.RowFilter = null;




            return Json(Helpers.CustomJsonResult.DataTableToJson(dt), JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult Create(List<Dictionary<string, object>> data)
        {
            try
            {

                for (int i = 0; i < data.Count; i++)
                {
                    if (bplib.clsWebLib.RetValidLen(data[i]["UserName"]).ToString() != "")
                    {
                        if (bplib.clsWebLib.RetValidLen(data[i]["Code"]).ToString() == "")
                            throw new Exception("Please enter Code");

                        if (bplib.clsWebLib.RetValidLen(data[i]["ShortName"]).ToString() == "")
                            throw new Exception("Please enter short name");
                    }
                    else
                    {
                        throw new Exception("Please fill all the information for each line item");
                    }

                    if (bplib.clsWebLib.RetValidLen(data[i]["Code"]).ToString() != "")
                    {
                        var k = data.Where(m => m["Code"].ToString().ToUpper() == bplib.clsWebLib.RetValidLen(data[i]["Code"]).ToString().ToUpper() &&
                          m["Criteria"].ToString().ToUpper() != bplib.clsWebLib.RetValidLen(data[i]["Criteria"]).ToString().ToUpper()).ToList();
                        if (k != null)
                        {
                            if (k.Count > 0)
                                throw new Exception("Same code already exists!!!");
                        }

                        k = data.Where(m => m["UserName"].ToString().ToUpper() == bplib.clsWebLib.RetValidLen(data[i]["UserName"]).ToString().ToUpper() &&
                         m["Criteria"].ToString().ToUpper() != bplib.clsWebLib.RetValidLen(data[i]["Criteria"]).ToString().ToUpper()).ToList();
                        if (k != null)
                        {
                            if (k.Count > 0)
                                throw new Exception("Same User Name already exists!!!");
                        }
                    }
                }



                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName, out dsMaster, false, "1");

                string _Id = "";




                #region data update

                bplib.clsGenID genid;
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i]["UserName"] != null)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "Criteria='" + data[i]["Criteria"].ToString() + "'";
                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            if (_Id == "")
                            {
                                genid = new bplib.clsGenID();
                                genid.GenID(TableName, out _Id);
                            }
                            data[i]["Id"] = "A" + _Id + (i + 1).ToString();
                            AddNewRow(dsMaster.Tables[0], data[i]);

                            //dsMaster.Tables[0].Rows[dsMaster.Tables[0].Rows.Count - 1]["Code"] = data[i]["Criteria"];
                            //dsMaster.Tables[0].Rows[dsMaster.Tables[0].Rows.Count - 1]["UserName"] = data[i]["Criteria"];
                            //dsMaster.Tables[0].Rows[dsMaster.Tables[0].Rows.Count - 1]["ShortName"] = data[i]["Criteria"];
                            //dsMaster.Tables[0].Rows[dsMaster.Tables[0].Rows.Count - 1]["StandardName"] = data[i]["Criteria"];
                            //dsMaster.Tables[0].Rows[dsMaster.Tables[0].Rows.Count - 1]["Active"] = true;
                        }
                        else
                        {

                            EditRow(dsMaster.Tables[0].DefaultView[0].Row, data[i]);
                        }
                    }

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

    }
}
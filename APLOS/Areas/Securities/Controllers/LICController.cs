#region Using

using Aplos.Controllers;
using Aplos.Properties;
using aplosLicenseService;
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
using System.Globalization;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    public class LICController : BaseController
    {
        string TableName = "SEC.LIC";
        //authentication for
        //GetList Create Delete


        #region Constructor
        EncDec encDec = new EncDec();
        private readonly ISqlRepository _sqlRepository;
        public LICController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

      
        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            DataSet dsData;
            string key = "@plosGlobalL1cens1ingServ1ce";

            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM SEC.LIC", out dsData, false, "1");
            if (dsData.Tables[0].Rows.Count > 0)
            {

                for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                {
                    //dsData.Tables[0].Rows[i]["LicKey1"] = Convert.ToDateTime(EncDec.Decrypt(dsData.Tables[0].Rows[i]["LicKey1"].ToString(), key));
                    // dsData.Tables[0].Rows[i]["LicKey2"] = Convert.ToDateTime(EncDec.Decrypt(dsData.Tables[0].Rows[i]["LicKey2"].ToString(), key));



                    string[] formats =
                    {
    "M/d/yyyy h:mm:ss tt",
    "M/d/yyyy H:mm:ss",
    "M/d/yyyy",
    "yyyy-MM-dd",
    "dd-MM-yyyy"
};

                    var date1Str = EncDec.Decrypt(dsData.Tables[0].Rows[i]["LicKey1"].ToString(), key);
                    var date2Str = EncDec.Decrypt(dsData.Tables[0].Rows[i]["LicKey2"].ToString(), key);

                    DateTime parsed1, parsed2;

                    if (!DateTime.TryParseExact(date1Str, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed1))
                        throw new Exception("Invalid LicKey1 date: " + date1Str);

                    if (!DateTime.TryParseExact(date2Str, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed2))
                        throw new Exception("Invalid LicKey2 date: " + date2Str);

                    dsData.Tables[0].Rows[i]["LicKey1"] = parsed1;
                    dsData.Tables[0].Rows[i]["LicKey2"] = parsed2;
                }
            }

            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dsData.Tables[0]);

            return Json(NewData, JsonRequestBehavior.AllowGet);
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
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["id"] + "'", out dsMaster, false, "1");

                string key = "@plosGlobalL1cens1ingServ1ce";

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

                    DateTime license1 = Convert.ToDateTime(EncDec.Encrypt(data["LicKey1"].ToString(), key));
                    DateTime license2 = Convert.ToDateTime(EncDec.Encrypt(data["LicKey2"].ToString(), key));

                    data["id"] = _Id;
                    data["LicKey1"] = license1;
                    data["LicKey2"] = license2;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    DateTime dateTime1 = Convert.ToDateTime(data["LicKey1"].ToString());
                    DateTime dateTime2 = Convert.ToDateTime(data["LicKey2"].ToString());

                    _Id = data["id"].ToString();
                    var license1 = EncDec.Encrypt(dateTime1.ToString(), key);
                    var license2 = EncDec.Encrypt(dateTime2.ToString(), key);
                    data["LicKey1"] = license1;
                    data["LicKey2"] = license2;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
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

        public ActionResult Delete(string id)
        {
            
                try
                {
                    if (string.IsNullOrEmpty(id))
                        throw new Exception("Select entry first");

                    ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
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

            dr.EndEdit();
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

       

    }
}
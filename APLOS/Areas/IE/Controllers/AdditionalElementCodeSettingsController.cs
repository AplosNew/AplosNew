using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.IE.Controllers
{
    public class AdditionalElementCodeSettingsController : Controller
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public AdditionalElementCodeSettingsController(ISqlRepository sqlRepository)
        {

            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
        // GET: IE/BartackCode
        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion -- Pages

        #region Forhad Code
        [HttpPost, Authorize]
        public JsonResult getData()
        {

            string sql0 = @"SELECT* FROM AddtionalElementCodeSettings";
            string sql1 = @"SELECT* FROM AddtionalElementCodeStopAccuracy";
            string sql2 = @"SELECT* FROM AddtionalElementCodeHandlingFactor";

            return Json(
                new
                {
                    GeneralSettings = _sqlRepository.GetDataCollection(sql0),
                    StoppingAccuracy = _sqlRepository.GetDataCollection(sql1),
                    HandlingFactor = _sqlRepository.GetDataCollection(sql2),
                },
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveGeneralSettings(Dictionary<string, object> data)
        {


            saveGeneralSettings(data, out DataSet dsLocal);
            OTSBD.clsStaticInfo info = new OTSBD.clsStaticInfo();
            info.SaveDataSets(dsLocal);


            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult SaveStoppingAccuracy(Dictionary<string, object> Masterdata, Dictionary<string, object> data)
        {


            saveStoppingAccuracy(Masterdata, data);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult SaveHandlingFactor(Dictionary<string, object> Masterdata, Dictionary<string, object> data)
        {


            saveHandlingFactor(Masterdata, data);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
       

        private void saveGeneralSettings(Dictionary<string, object> data, out DataSet dsLocal)
        {
            try
            {
                string sql0 = @"SELECT* FROM AddtionalElementCodeSettings";
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet(sql0, out dsLocal);

                if (dsLocal.Tables[0].Rows.Count == 0)
                {

                    DataRow dr = dsLocal.Tables[0].NewRow();


                    dr["Id"] = System.DateTime.Now.Ticks;
                    dr["EachStartTMU"] = OTSBD.clsStaticInfo.dbl(data["EachStartTMU"]);
                    dr["EachStopTMU"] = OTSBD.clsStaticInfo.dbl(data["EachStopTMU"]);


                    dsLocal.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dsLocal.Tables[0].Rows[0];

                    dr.BeginEdit();
                    dr["EachStartTMU"] = OTSBD.clsStaticInfo.dbl(data["EachStartTMU"]);
                    dr["EachStopTMU"] = OTSBD.clsStaticInfo.dbl(data["EachStopTMU"]);
                    dr.EndEdit();

                }

                data["Id"] = dsLocal.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        private void saveStoppingAccuracy(Dictionary<string, object> Masterdata, Dictionary<string, object> data)
        {
            try
            {
                DataSet dsLocal;
                string sql0 = @"SELECT* FROM AddtionalElementCodeStopAccuracy Where Id<>'" + data["Id"] + "' AND Code='" + data["Code"] + "'";
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet(sql0, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code exists!!!");


                saveGeneralSettings(Masterdata, out DataSet dsMaster);

                sql0 = @"SELECT* FROM AddtionalElementCodeStopAccuracy Where Id='" + data["Id"] + "'";
                con = new ConnectionManager.clsConnection();
                con.getDataSet(sql0, out dsLocal);

                if (dsLocal.Tables[0].Rows.Count == 0)
                {

                    DataRow dr = dsLocal.Tables[0].NewRow();


                    dr["Id"] = System.DateTime.Now.Ticks;
                    dr["Code"] = data["Code"];
                    dr["Description"] = data["Description"];
                    dr["ValueInTMU"] = OTSBD.clsStaticInfo.dbl(data["ValueInTMU"]);

                    dsLocal.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dsLocal.Tables[0].Rows[0];

                    dr.BeginEdit();
                    dr["Code"] = data["Code"];
                    dr["Description"] = data["Description"];
                    dr["ValueInTMU"] = OTSBD.clsStaticInfo.dbl(data["ValueInTMU"]);
                    dr.EndEdit();

                }

                data["Id"] = dsLocal.Tables[0].Rows[0]["Id"].ToString();


                OTSBD.clsStaticInfo info = new OTSBD.clsStaticInfo();
                info.SaveDataSets(dsMaster, dsLocal);
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        private void saveHandlingFactor(Dictionary<string, object> Masterdata, Dictionary<string, object> data)
        {
            try
            {
                DataSet dsLocal;
                string sql0 = @"SELECT * FROM AddtionalElementCodeHandlingFactor Where Id<>'" + data["Id"] + "' AND Code='" + data["Code"] + "'";
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet(sql0, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code exists!!!");


                saveGeneralSettings(Masterdata, out DataSet dsMaster);


                sql0 = @"SELECT* FROM AddtionalElementCodeHandlingFactor Where Id='" + data["Id"] + "'";
                con = new ConnectionManager.clsConnection();
                con.getDataSet(sql0, out dsLocal);

                if (dsLocal.Tables[0].Rows.Count == 0)
                {

                    DataRow dr = dsLocal.Tables[0].NewRow();


                    dr["Id"] = System.DateTime.Now.Ticks;
                    dr["Code"] = data["Code"];
                    dr["Description"] = data["Description"];
                    dr["DegreeOfDifficulty"] = data["DegreeOfDifficulty"];
                    dr["AdditionRate"] = OTSBD.clsStaticInfo.dbl(data["AdditionRate"]);

                    dsLocal.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dsLocal.Tables[0].Rows[0];

                    dr.BeginEdit();
                    dr["Code"] = data["Code"];
                    dr["Description"] = data["Description"];
                    dr["DegreeOfDifficulty"] = data["DegreeOfDifficulty"];
                    dr["AdditionRate"] = OTSBD.clsStaticInfo.dbl(data["AdditionRate"]);
                    dr.EndEdit();

                }

                data["Id"] = dsLocal.Tables[0].Rows[0]["Id"].ToString();


                OTSBD.clsStaticInfo info = new OTSBD.clsStaticInfo();
                info.SaveDataSets(dsMaster, dsLocal);
            }
            catch (Exception ex)
            {

                throw;
            }

        }



        [HttpGet]
        public ActionResult DeleteStoppingAccuracy(string Id)
        {
            try
            {
                if (Id == "null")
                    throw new Exception("Select entry first");

                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM AddtionalElementCodeStopAccuracy Where Id='" + Id.ToString() + "'");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet]
        public ActionResult DeleteHandlingFactor(string Id)
        {
            try
            {
                if (Id == "null")
                    throw new Exception("Select entry first");

                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM AddtionalElementCodeHandlingFactor Where Id='" + Id.ToString() + "'");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion
    }
}
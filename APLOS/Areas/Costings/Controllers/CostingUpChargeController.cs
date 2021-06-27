#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Model.Costings;
using Library.Service.Costings;
using System.Data;
using Library.Data.Sql;
using Aplos.Helpers;
using System;
using System.Collections.Generic;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Costings.Controllers
{
    public class CostingUpChargeController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public CostingUpChargeController(ISqlRepository s)
        {
            _sqlRepository = s;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        public JsonResult getData(string CostingType)
        {
            DataTable dtTemp;
            try
            {
                string sql = "Select * from hkp.CostingUpChargeMatrix where CostingType='" + CostingType + "'";
                DataTable dt = _sqlRepository.GetDataTable(sql);

                dtTemp = dt.Clone();
                for (int i = 1; i <= 100; i++)
                {
                    dt.DefaultView.RowFilter = "WorkCenterDays=" + i.ToString();
                    if (dt.DefaultView.Count > 0)
                    {
                        dtTemp.ImportRow(dt.DefaultView[0].Row);
                    }
                    else
                    {
                        DataRow dr = dtTemp.NewRow();
                        dr["WorkCenterDays"] = i.ToString();
                        dtTemp.Rows.Add(dr);
                    }
                }


            }
            catch (System.Exception ex)
            {

                throw;
            }
            return Json(CustomJsonResult.DataTableToJson(dtTemp), JsonRequestBehavior.AllowGet);
        }
        private string GetPK(string TableName)
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), TableName, out sID);
            return sID;
        }
        [HttpPost]
        public JsonResult UpdateData(List<Dictionary<string, object>> MatrixData, string CostingType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsRef;
                string sql = "Select * from hkp.CostingUpChargeMatrix where CostingType='" + CostingType + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
                while (dsRef.Tables[0].DefaultView.Count > 0)
                {
                    dsRef.Tables[0].DefaultView[0].Delete();
                }
                string id = GetPK("hkp.CostingUpChargeMatrix");
                for (int i = 0; i < MatrixData.Count; i++)
                {
                    //dsRef.Tables[0].DefaultView.RowFilter = "WorkCenterDays=" + (i + 1).ToString();
                    //if (dsRef.Tables[0].DefaultView.Count > 0)
                    //{
                    //    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                    //    dr.BeginEdit();

                    //    //dr["CostingType"] = CostingType;
                    //    dr["Basic"] = clsStaticInfo.dbl(MatrixData[i]["Basic"]);
                    //    dr["SemiCritical"] = clsStaticInfo.dbl(MatrixData[i]["SemiCritical"]);
                    //    dr["Critical"] = clsStaticInfo.dbl(MatrixData[i]["Critical"]);
                    //    dr["HighlyCritical"] = clsStaticInfo.dbl(MatrixData[i]["HighlyCritical"]);

                    //    dr["UpdatedBy"] = identity.Name;
                    //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    //    dr["UpdatedFromIP"] = identity.IPAddress;

                    //    dr.EndEdit();
                    //}
                    //else
                    //{
                    DataRow dr = dsRef.Tables[0].NewRow();

                    dr["Id"] = id + "-" + (i + 1).ToString();
                    dr["WorkCenterDays"] = (i + 1).ToString();
                    dr["CostingType"] = CostingType;
                    dr["Basic"] = clsStaticInfo.dbl(MatrixData[i]["Basic"]);
                    dr["SemiCritical"] = clsStaticInfo.dbl(MatrixData[i]["SemiCritical"]);
                    dr["Critical"] = clsStaticInfo.dbl(MatrixData[i]["Critical"]);
                    dr["HighlyCritical"] = clsStaticInfo.dbl(MatrixData[i]["HighlyCritical"]);


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;


                    dsRef.Tables[0].Rows.Add(dr);
                    //}
                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);


                return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
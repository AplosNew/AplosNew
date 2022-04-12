#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;
using System;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using Library.Data.Sql;
using Library.Security.Core;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class CostingSOTemplateController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public CostingSOTemplateController(ISqlRepository R)
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



        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  ISNULL(Max(Sequence),0) AS Sequence FROM dbo.CostingSOTemplate");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {

            string sql = @"select N.* from [dbo].[CostingSOTemplate] N";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(string CostingSOTemplateId)
        {

            string sql = @"SELECT D.Sequence,D.SalaryHeadID
                        ,SalaryHead= CASE WHEN ISNULL(SD.SalaryHead,'')<>'' THEN SD.SalaryHead ELSE D.Component END,D.Component,D.CostingSOTemplateId
                        FROM [dbo].[FormulaDetail] D
                        LEFT JOIN dbo.SalaryHead SD ON SD.SalaryHeadID=D.SalaryHeadID
                        WHERE CostingSOTemplateId='" + CostingSOTemplateId + @"' Order By D.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> data/*, IEnumerable<CostingSOTemplateFormulaDetail> details*/)
        {
            try
            {
                SaveCostingSOTemplateData(data/*, details*/);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }


        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "CostingSOTemplate", out sID);
            return sID;
        }


        private void SaveCostingSOTemplateData(Dictionary<string, object> data/*, IEnumerable<CostingSOTemplateFormulaDetail> details*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    string _Id = "";

                    DataSet dsMaster, dsDestination;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.CostingSOTemplate WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    // con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where CostingSOTemplateId='" + data.Id + "'", out dsDestination, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(CostingSOTemplate), out _Id);

                        data["Id"] =  _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }

                    string Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                    #region NoticePeriodFormulaDetail 
                    //DataRow drF;
                    //while (dsDestination.Tables[0].DefaultView.Count > 0)
                    //    dsDestination.Tables[0].DefaultView[0].Delete();

                    //int count = 0;
                    //if (details != null)
                    //{

                    //    foreach (var item in details)
                    //    {
                    //        drF = dsDestination.Tables[0].NewRow();
                    //        count++;
                    //        string pk = _Id + "_" + count;
                    //        drF["Id"] = pk;
                    //        drF["CostingSOTemplateId"] = _Id;
                    //        drF["Sequence"] = item.Sequence;
                    //        drF["Component"] = item.Component;

                    //        dsDestination.Tables[0].Rows.Add(drF);
                    //    }

                    //}
                    #endregion NoticePeriodFormulaDetail 

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster/*, dsDestination*/);


                }
            }
            catch (Exception ex)
            {
                throw (ex);
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
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

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

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string SystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.CostingSOTemplate WHERE Id = '" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper("Delete [dbo].FormulaDetail where CostingSOTemplateId= '" + SystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function



        #endregion

       
    }

    public class CostingSOTemplate
    {
        public string Id { get; set; }
        public string PlantId { get; set; }
        public string FormulaDes { get; set; }
        public string FormulaDesID { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

    }

    public class CostingSOTemplateFormulaDetail
    {
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string NoticePeriodSettingId { get; set; }
        public string SalaryHeadID { get; set; }
        public string Component { get; set; }
    }
}
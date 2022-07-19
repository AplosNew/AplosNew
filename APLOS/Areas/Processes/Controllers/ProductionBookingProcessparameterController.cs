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

namespace Aplos.Areas.Processes.Controllers
{
    public class ProductionBookingProcessparameterController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        string TableName = "dbo.ProductionBookingProcessparameter";
        public ProductionBookingProcessparameterController(ISqlRepository R)
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

        [HttpGet, Authorize]
        public JsonResult GetHeaderItemCbo(string id)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id AS Value, UserName AS Text FROM [dbo].[ProductionBookingProcessparameter] WHERE Id<>'" + id + "'"), JsonRequestBehavior.AllowGet);
        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  ISNULL(Max(Sequence),0) AS Sequence FROM dbo.ProductionBookingProcessparameter");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {

            string sql = @"SELECT N.* from [dbo].[ProductionBookingProcessparameter] N ORDER BY N.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(string OrderLineCostingItemId)
        {

            string sql = @"SELECT D.Sequence,D.OrderLineHeadId
                            ,SalaryHead= CASE WHEN ISNULL(SD.UserName,'')<>'' THEN SD.UserName ELSE D.Component END,D.Component,D.OrderLineCostingItemId
                            FROM [dbo].[FormulaDetail] D
                            LEFT JOIN dbo.OrderLineCostingItem SD ON SD.Id=D.OrderLineHeadId
                            WHERE OrderLineCostingItemId='"+ OrderLineCostingItemId + "' Order By D.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingComponentByCostingType(string costingType)
        {
            string sql = @"SELECT * FROM  HKP.CostingComponent WHERE Id IN(SELECT CostingComponentId  FROM [dbo].[CostingTypeComponent] WHERE CostingType='"+ costingType + "') ORDER BY Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> data, IEnumerable<ProductionBookingProcessparameterFormulaDetail> details)
        {
            try
            {
                SaveCostingSOTemplateData(data, details);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void SaveCostingSOTemplateData(Dictionary<string, object> data, IEnumerable<ProductionBookingProcessparameterFormulaDetail> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    string _Id = "";

                    DataSet dsMaster, dsDestination;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("UserName already exists!!!");


                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductionBookingProcessparameter WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where ProductionBookingProcessparameterId='" + data["Id"] + "'", out dsDestination, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ProductionBookingProcessparameter), out _Id);

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
                    DataRow drF;
                    while (dsDestination.Tables[0].DefaultView.Count > 0)
                        dsDestination.Tables[0].DefaultView[0].Delete();

                    int count = 0;
                    if (details != null)
                    {

                        foreach (var item in details)
                        {
                            drF = dsDestination.Tables[0].NewRow();
                            count++;
                            string pk = _Id + "_" + count;
                            drF["Id"] = pk;
                            drF["ProductionBookingProcessparameterId"] = _Id;
                            drF["Sequence"] = item.Sequence;
                            drF["ProductionBookingProcessparameterHeadId"] = item.ProductionBookingProcessparameterHeadId;
                            drF["Component"] = item.Component;

                            dsDestination.Tables[0].Rows.Add(drF);
                        }

                    }
                    #endregion NoticePeriodFormulaDetail 

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsDestination);


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
                strSQL = "DELETE FROM dbo.OrderLineCostingItem WHERE Id = '" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

               // objCon.ExecuteNonQueryWrapper("Delete [dbo].FormulaDetail where CostingSOTemplateId= '" + SystemID + "'", true, "1");
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

    public class ProductionBookingProcessparameter
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

    public class ProductionBookingProcessparameterFormulaDetail
    {
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string NoticePeriodSettingId { get; set; }
        public string SalaryHeadID { get; set; }
        public string Component { get; set; }
        public string ProductionBookingProcessparameterId { get; set; }
        public string ProductionBookingProcessparameterHeadId { get; set; }

    }
}
#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Commercial;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Commercial.Controllers
{
    public class LCFundUtilizationController : BaseController
    {
        string TableName = "dbo.LCFundUtilization";

        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public LCFundUtilizationController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpPost, Authorize]
        public ActionResult GetFundUtilizationList(string column, string value)
        {
            
            string sql = @"SELECT * FROM " + TableName + " WHERE UtilizationSourceType='"+ UtilizationSourceType.FundUtilization + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost,Authorize]
        public ActionResult GetBuyerDeductionList(string column, string value)
        {

            string sql = @"SELECT * FROM " + TableName + " WHERE UtilizationSourceType='" + UtilizationSourceType.BuyerDeduction + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(LCFundUtilization), out sID);
            return sID;
        }

        [HttpPost]
        public ActionResult Create(IEnumerable<LCFundUtilization> data)
        {
            try
            {
                SaveFundUtilizationData(data);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }
        private void SaveFundUtilizationData(IEnumerable<LCFundUtilization> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM "+TableName+ " WHERE FundUtilization='" + item.FundUtilization + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["UtilizationSourceType"] = UtilizationSourceType.FundUtilization;
                            dr["FundUtilization"] = item.FundUtilization;
                            dr["FundUtilizationText"] = item.FundUtilizationText;
                            dr["Percentage"] =item.Percentage;
                            dr["CurrencyId"] = item.CurrencyId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();
                            dr["UtilizationSourceType"] = UtilizationSourceType.FundUtilization;
                            dr["FundUtilization"] = item.FundUtilization;
                            dr["FundUtilizationText"] = item.FundUtilizationText;
                            dr["Percentage"] = item.Percentage;
                            dr["CurrencyId"] = item.CurrencyId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public ActionResult CreateBuyerDeduction(IEnumerable<LCFundUtilization> data)
        {
            try
            {
                SaveBuyerDeductionData(data);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }
        private void SaveBuyerDeductionData(IEnumerable<LCFundUtilization> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM " + TableName + " WHERE FundUtilization='" + item.FundUtilization + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["UtilizationSourceType"] = UtilizationSourceType.BuyerDeduction;
                            dr["FundUtilization"] = item.FundUtilization;
                            dr["FundUtilizationText"] = item.FundUtilizationText;
                            dr["Percentage"] = item.Percentage;
                            dr["CurrencyId"] = item.CurrencyId;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();
                            dr["UtilizationSourceType"] = UtilizationSourceType.BuyerDeduction;
                            dr["FundUtilization"] = item.FundUtilization;
                            dr["FundUtilizationText"] = item.FundUtilizationText;
                            dr["Percentage"] = item.Percentage;
                            dr["CurrencyId"] = item.CurrencyId;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public ActionResult Delete(string id)
        {
            string sql = @"select * from '"+TableName+"' where Id = '"+ id + "'";
                try
                {
                    if (string.IsNullOrEmpty(id))
                        throw new Exception("Select entry first");

                    ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                    con.BeginTransaction();
                    con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                    con.CommitTransaction();

                    return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

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
        
    }

    public class LCFundUtilization
    {
        public string UtilizationSourceType { get; set; }
        public string FundUtilization { get; set; }
        public string FundUtilizationText { get; set; }
        public decimal Percentage { get; set; }
        public string CurrencyId { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}
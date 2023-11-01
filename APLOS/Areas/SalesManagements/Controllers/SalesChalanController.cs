#region Using

using Aplos.Controllers;
using Aplos.Properties;
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
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.SalesManagements.Controllers
{
    public class SalesChalanController : BaseController
    {
        //abcd
        //this is my code from tarek
        string TableName = "dbo.SalesChalan";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public SalesChalanController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


    
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet,Authorize]
        public ActionResult GetVehicleNoCbo()
        {
            try
            {
                string sql = @"SELECT DISTINCT TransportVehicleNo AS Value,TransportVehicleNo AS Text FROM [dbo].[PostSalesInvoice] Where TransportVehicleNo IS NOT NULL";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (Select SC.*,WE.EmployeeName ByWhom,SE.EmployeeName SecurityInCharge,RE.EmployeeName ResponsiblePerson,CE.EmployeeName CheckBy,AE.EmployeeName ApproveBy
from [dbo].[SalesChalan] SC
LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=SC.ByWhomId
LEFT JOIN dbo.EmployeeInformation SE ON SE.SystemId=SC.SecurityInChargeId
LEFT JOIN dbo.EmployeeInformation RE ON RE.SystemId=SC.ResponsiblePersonId
LEFT JOIN dbo.EmployeeInformation CE ON CE.SystemId=SC.CheckById
LEFT JOIN dbo.EmployeeInformation AE ON AE.SystemId=SC.ApproveById) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetInvoiceData(string fromDate,string toDate,string vehicleno)
        {
            try
            {
                string sql = @"SELECT Checked=CAST(0 AS bit),FORMAT(S.InvoiceDate,'dd-MMM-yyyy')InvoiceDate, S.Id InvoiceId,P.UserName Customer,DT.UserName Destination,BKD.NoOfPackage,BKD.NetWeight,BKD.GrossWeight
FROM TRN.Sales S
LEFT JOIN HKP.Party P ON P.Id=S.PartyId
LEFT JOIN (select  sum(isc.NetWeight) NetWeight ,sum(isc.Gweight) GrossWeight , Count(isc.RefNo) NoOfPackage , isc.SalesId 
                from itemscanchild isc
                left join trn.POLotReference PLR on PLR.Id = isc.PackingId
                left join trn.PackingLineItem pli on pli.PackingLineItemId = PLR.PackingLineItemId
				group by  isc.salesId) BKD on BKD.salesId = s.Id
left join PostSalesInvoice PSI on PSI.SalesId = BKD.SalesId
left join MST.Addressmaster AM on Am.Id = P.AddressmasterId
left join scs.District Dt on DT.Id = AM.DistrictId
Where FORMAT(S.AddedDate,'dd-MMM-yyyy') between '" + fromDate + @"' AND '"+ toDate + @"' AND PSI.Transportvehicleno='"+ vehicleno + @"'
AND S.Id NOT IN(Select  InvoiceId from dbo.SalesChalanDetail)
ORDER BY S.Id";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet,Authorize]
        public ActionResult GetInvoiceDataByChalan(string masterId)
        {
            try
            {
                string sql = @"Select SCD.*,P.UserName Customer,BKD.NoOfPackage,BKD.NetWeight,BKD.GrossWeight,DT.UserName Destination,FORMAT(S.InvoiceDate,'dd-MMM-yyyy')InvoiceDate 
from dbo.SalesChalanDetail SCD
LEFT JOIN TRN.Sales S ON S.Id=SCD.InvoiceId
LEFT JOIN HKP.Party P ON P.Id=S.PartyId
left join (select  sum(isc.NetWeight) NetWeight ,sum(isc.Gweight) GrossWeight , Count(isc.RefNo) NoOfPackage,isc.SalesId 
                from itemscanchild isc
                left join trn.POLotReference PLR on PLR.Id = isc.PackingId
                left join trn.PackingLineItem pli on pli.PackingLineItemId = PLR.PackingLineItemId
				group by  isc.salesId) BKD on BKD.salesId = s.Id
left join PostSalesInvoice PSI on PSI.SalesId = BKD.SalesId
left join MST.Addressmaster AM on Am.Id = P.AddressmasterId
left join scs.District DT on DT.Id = AM.DistrictId
Where SCD.SalesChalanId='" + masterId + "'";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> details)
        {
            try
            {
                DataSet dsMaster, dsChild;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                string _cId = "";
                bplib.clsGenID genid = new bplib.clsGenID();
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                   
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

                #region SalesChalanDetail 

                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SalesChalanDetail WHERE  SalesChalanId='" + data["Id"] + "'", out dsChild, false, "1");
                
                if (details != null)
                {
                    genid.GenID("SalesChalanDetail", out _cId);
                    int c = 0;
                    foreach (var item in details)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            c++;
                            item["Id"] = _cId + " - " + c;
                            item["SalesChalanId"] = data["Id"];

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsChild);

                return Json(new { Error = false, Data= data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            string sql = @"select * from '"+TableName+"' where Id = '" + id + "'";

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
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
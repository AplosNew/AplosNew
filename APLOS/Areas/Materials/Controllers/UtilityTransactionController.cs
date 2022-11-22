#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Library.ViewModel.Materials;
using Syncfusion.DocIO.DLS;
using Library.Security.Core;
using System.Data;
using System;

#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class UtilityTransactionController : BaseController
    {
        string TableName = "dbo.UtilityTransaction";

        #region -- Constructor

        private readonly IFabricRollMasterService _fabricRollMasterService;
        private SqlRepository _sqlRepository = new SqlRepository();
        public UtilityTransactionController()
        {

        }

        #endregion -- Constructor

        #region Pages

        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages


        #region Operation

        [HttpPost, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct UT.Id,FORMAT(UT.Date,'dd-MMM-yyyy') [Date],UM.Id UtilityMasterId,UM.UserName UtilityMaster,UT.Quantity
							            ,UT.Reading,UOM.Id UoMId,UOM.UserName UoM,UT.Quantity,UT.Reading,UT.Remarks
							            from dbo.UtilityTransaction UT
										left join UtilityMaster UM on UM.Id=UT.UtilityMasterId
										left join SCS.UnitOfMeasurement UOM on UOM.Id=UM.UoMId";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetUtilityMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select distinct UM.UserName as Text,UM.Id as [Value],UOM.UserName UoM,UM.IsReadingApplicable,C.LastReading

                                from UtilityMaster UM
                                left join SCS.UnitOfMeasurement UOM on UOM.Id=UM.UoMId
                                --left join (Select MAX(FORMAT(AddedDate,'dd-MMM-yyyy'))LastReadingDate,UtilityMasterId from UtilityTransaction group by AddedDate,UtilityMasterId) A on A.UtilityMasterId=UM.Id
                                --left join (Select MAX(CONVERT(varchar(5),AddedDate,108))LastReadingTime,UtilityMasterId from UtilityTransaction group by AddedDate,UtilityMasterId) B on B.UtilityMasterId=UM.Id
                                left join (Select TOP(1) Quantity as LastReading,UtilityMasterId from UtilityTransaction ORDER by AddedDate,UtilityMasterId DESC) C on C.UtilityMasterId=UM.Id";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetReadingList(string utilityMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select TOP(1) MAX(FORMAT(AddedDate,'dd-MMM-yyyy'))LastReadingDate,MAX(CONVERT(varchar(5),AddedDate,108))LastReadingTime,Quantity LastReading
                                    from UtilityTransaction 
					                Where UtilityMasterId='" + utilityMasterId + @"'
                                    group by AddedDate,Quantity";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEditReadingList(string utilityMasterId,string utilityTransactionId)
        {
            if (utilityTransactionId=="null"|| utilityTransactionId == "undefind")
            {
                utilityTransactionId = "";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select LastReading=(select top(1) Reading from UtilityTransaction  order by Id desc)
									, LastReadingDate=(select top(1) FORMAT([Date],'dd-MMM-yyyy') from UtilityTransaction  order by Id desc)
									, LastReadingTime=(select top(1) CONVERT(varchar(5),[AddedDate],108) from UtilityTransaction  order by Id desc)
                                    from UtilityTransaction
                                    Where UtilityMasterId='" + utilityMasterId + @"' and Id='" + utilityTransactionId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMasterOrder;
                string id = string.Empty;
                try
                {
                    objCon = new ConnectionManager.DAL.ConManager("1");

                    objCon.OpenDataSetThroughAdapter("select * from dbo.UtilityTransaction where Id='" + data["Id"] + "'", out dsMasterOrder, false, "1");

                    string UtilityTransactionId = "";

                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + data["Id"] + "'";

                    if (dsMasterOrder.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "UtilityTransaction", out UtilityTransactionId);

                        data["Id"] = UtilityTransactionId;
                        AddNewRow(dsMasterOrder.Tables[0], data);
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMasterOrder);

                    return Json(new { Error = false, Message = AplosMessage.Insert });

                }
                catch (Exception ex)
                {
                    return Json(new { Error = true, Message = ex.Message });
                }
            }

        }

        [HttpPost]
        public JsonResult Update(Dictionary<string, object> data)
        {

            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMasterOrder;
                string id = string.Empty;
                try
                {
                    objCon = new ConnectionManager.DAL.ConManager("1");

                    objCon.OpenDataSetThroughAdapter("select * from dbo.UtilityTransaction where Id='" + data["Id"] + "'", out dsMasterOrder, false, "1");

                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + data["Id"] + "'";

                    if (dsMasterOrder.Tables[0].Rows.Count > 0)
                    {
                        EditRow(dsMasterOrder.Tables[0].Rows[0], data);
                    }
                    
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMasterOrder);

                    return Json(new { Error = false, Message = AplosMessage.Insert });

                }
                catch (Exception ex)
                {
                    return Json(new { Error = true, Message = ex.Message });
                }
            }

        }

        public ActionResult Delete(string id)
        {
            try
            {
                //if (string.IsNullOrEmpty(id))
                //    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from UtilityTransaction where Id='" + id + "'");
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

        #endregion

    }
}
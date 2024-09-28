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
using Library.Data;
using Library.Service.Logs;
using System.Reflection;

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
            string sql = @"select top(1000) UT.Id,FORMAT(UT.Date,'dd-MMM-yyyy') [Date],UM.Id UtilityMasterId,UM.UserName UtilityMaster,UT.Quantity
							            ,UT.Reading,UOM.Id UoMId,UOM.UserName UoM,UT.Quantity,UT.Reading
                                        ,UT.LastReading,FORMAT(UT.LastReadingDate,'dd-MMM-yyyy')LastReadingDate,CONVERT(varchar(5),UT.LastReadingTime,108) LastReadingTime
                                        ,UT.Remarks, UM.MultiplyingFactor,UT.InputSourceId
							            from dbo.UtilityTransaction UT
										left join UtilityMaster UM on UM.Id=UT.UtilityMasterId
										left join SCS.UnitOfMeasurement UOM on UOM.Id=UM.UoMId
                                        order by UT.Date Desc";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        //[HttpGet, Authorize]
        //public JsonResult GetUtilityMasterList(GridParameter parameters)
        //{
        //    return Json(GetUtilityMasterData(parameters), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost, Authorize]
        public JsonResult GetUtilityMasterList(string column, string value)
        {
            //var res = GetCompanyPartyListNew(column, value);
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 1000 * from (select distinct UM.Id as UtilityMasterId, UM.UserName as UtilityMaster,UOM.UserName UoM,UM.IsReadingApplicable,C.LastReading
								,UG.UserName UtilityGroup,UM.UtilitySubGroup,UM.UtilityCategory,UM.UtilitySubCategory,UM.MultiplyingFactor,UM.UoMId,UM.InPutSourceId

                                from UtilityMaster UM
                                left join SCS.UnitOfMeasurement UOM on UOM.Id=UM.UoMId
                                --left join (Select MAX(FORMAT(AddedDate,'dd-MMM-yyyy'))LastReadingDate,UtilityMasterId from UtilityTransaction group by AddedDate,UtilityMasterId) A on A.UtilityMasterId=UM.Id
                                --left join (Select MAX(CONVERT(varchar(5),AddedDate,108))LastReadingTime,UtilityMasterId from UtilityTransaction group by AddedDate,UtilityMasterId) B on B.UtilityMasterId=UM.Id
                                left join (Select TOP(1) Quantity as LastReading,UtilityMasterId from UtilityTransaction ORDER by AddedDate,UtilityMasterId DESC) C on C.UtilityMasterId=UM.Id
								left join HKP.UtilityGroup UG on UG.Id=UM.UtilityGroupId
                                Where UM.Active=1
                                    ) AS TEMP WHERE " + strkey + " ";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

                //return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            //var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            //jsondata.MaxJsonLength = int.MaxValue;
            //return jsondata;
        }

        //public List<Dictionary<string, object>> GetCompanyPartyListNew(string column, string value)
        //{
        //    try
        //    { 
        //        string strkey = "1=1";
        //        if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
        //            strkey = column + " like '%" + value + "%'";
        //        var sql = @"select top 100 * from (select distinct UM.Id as UtilityMasterId, UM.UserName as UtilityMaster,UOM.UserName UoM,UM.IsReadingApplicable,C.LastReading
								//,UG.UserName UtilityGroup,UM.UtilitySubGroup,UM.UtilityCategory,UM.UtilitySubCategory

        //                        from UtilityMaster UM
        //                        left join SCS.UnitOfMeasurement UOM on UOM.Id=UM.UoMId
        //                        --left join (Select MAX(FORMAT(AddedDate,'dd-MMM-yyyy'))LastReadingDate,UtilityMasterId from UtilityTransaction group by AddedDate,UtilityMasterId) A on A.UtilityMasterId=UM.Id
        //                        --left join (Select MAX(CONVERT(varchar(5),AddedDate,108))LastReadingTime,UtilityMasterId from UtilityTransaction group by AddedDate,UtilityMasterId) B on B.UtilityMasterId=UM.Id
        //                        left join (Select TOP(1) Quantity as LastReading,UtilityMasterId from UtilityTransaction ORDER by AddedDate,UtilityMasterId DESC) C on C.UtilityMasterId=UM.Id
								//left join HKP.UtilityGroup UG on UG.Id=UM.UtilityGroupId
        //                            ) AS TEMP WHERE " + strkey + " order by Code ";
        //        return _sqlRepository.GetDataCollection(sql);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
        //    }
        //}

        //public GridModel GetUtilityMasterData(GridParameter parameters)
        //{
        //    parameters.CmdText = @"select distinct UM.Id as UtilityMasterId, UM.UserName as UtilityMaster,UOM.UserName UoM,UM.IsReadingApplicable,C.LastReading
								//,UG.UserName UtilityGroup,UM.UtilitySubGroup,UM.UtilityCategory,UM.UtilitySubCategory

        //                        from UtilityMaster UM
        //                        left join SCS.UnitOfMeasurement UOM on UOM.Id=UM.UoMId
        //                        --left join (Select MAX(FORMAT(AddedDate,'dd-MMM-yyyy'))LastReadingDate,UtilityMasterId from UtilityTransaction group by AddedDate,UtilityMasterId) A on A.UtilityMasterId=UM.Id
        //                        --left join (Select MAX(CONVERT(varchar(5),AddedDate,108))LastReadingTime,UtilityMasterId from UtilityTransaction group by AddedDate,UtilityMasterId) B on B.UtilityMasterId=UM.Id
        //                        left join (Select TOP(1) Quantity as LastReading,UtilityMasterId from UtilityTransaction ORDER by AddedDate,UtilityMasterId DESC) C on C.UtilityMasterId=UM.Id
								//left join HKP.UtilityGroup UG on UG.Id=UM.UtilityGroupId";
        //    return _sqlRepository.GetGridData(parameters);
        //}


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
        public JsonResult GetEditReadingList(string utilityMasterId, string utilityTransactionId)
        {
            if (utilityTransactionId == "null" || utilityTransactionId == "undefind")
            {
                utilityTransactionId = "";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"Select TOP(1)* from (select LastReading=(select LastReading=(select top(1) Reading from UtilityTransaction Where UtilityMasterId ='" + utilityMasterId + @"' order by Date desc))
									, LastReadingDate=(select top(1) FORMAT([Date],'dd-MMM-yyyy') from UtilityTransaction Where UtilityMasterId='" + utilityMasterId +@"' order by Date desc)
                                    , LastReadingTime=(select top(1) CONVERT(varchar(5),[AddedDate],108) from UtilityTransaction Where UtilityMasterId = '" + utilityMasterId + @"' order by Date desc)
                                    , MultiplyingFactor = (select top(1)  MultiplyingFactor from UtilityMaster where UtilityMasterId = '"+ utilityMasterId + @"' order by Date desc)

                                    from UtilityTransaction
                                    Where UtilityMasterId='" + utilityMasterId + "')A";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCalculatedValue(string utilityMasterId)
        {
            string sql = @"select * from UtilityMaster where Id='" + utilityMasterId + "'";

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
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

namespace Aplos.Areas.Farming.Controllers
{
    public class PurchaseBookingSodaController : BaseController
    {
        string TableName = "TRN.PurchaseBookingSoda";
        string TableName1 = "TRN.PurchaseBookingSodaChild";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PurchaseBookingSodaController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
       


     
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM "+ TableName +"  "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getlocation()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName AS Text FROM HKP.CropRateLocation"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult geticsmaster()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,Name AS Text FROM [MST].[ICSMaster]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getcropplanning(string ICSMasterId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM TRN.CropPlanning where ICSMasterID='"+ ICSMasterId + "'"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getvalidationenumvalue()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,EnumValue as Text from dbo.FarmingBusinessProcess"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult getcustomer()
        {
            return Json(_sqlRepository.GetDataCollection("select p.Id as Value,p.UserName as Text from HKP.Party p left join HKP.CompanyParty cp on p.Code= cp.PartyId where cp.PartyType='Customer' order by Text "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getcrop()
        {
            return Json(_sqlRepository.GetDataCollection("select distinct cm.Id as Value,cm.UserName as Text from MST.CropMaster cm inner join TRN.CropPlanningChild cpc on cm.Id=cpc.CropId "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult getcroptype(string CropNameId)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct ct.Id as Value,ct.UserName as Text from HKP.CropType ct inner join TRN.CropPlanningChild cpc on ct.Id=cpc.CropTypeId where cpc.CropId='"+ CropNameId + "' "), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getfarmer(string ICSMasterID, string CropPlanningId)
        {
            
            string sql;
            sql = @"select distinct fm.Id as Value,fm.FarmerName as Text from MST.FarmerMaster fm inner join TRN.CropPlanningChild cpc
                                                            on fm.Id=cpc.FarmerId inner join TRN.CropPlanning cp on cpc.CropPlanningMasterId=cp.Id
															where cp.Id='"+ CropPlanningId + "' or cp.ICSMasterID='"+ ICSMasterID + "' order by Text";
            
            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpGet]
        public JsonResult getfarmerfather(string FarmerID)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct Id as Value, FarmerFatherHusbandName as Text from MST.FarmerMaster where Id='"+ FarmerID + "' "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getfarmerregistrationid(string FarmerFatherID)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct Id as Value, FarmerRegistrationID as Text from MST.FarmerMaster where Id='"+ FarmerFatherID + "' "), JsonRequestBehavior.AllowGet);
        }



        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from TRN.PurchaseBookingSoda where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           

            string sql = @"select top 100 * from (select distinct pbs.*,FORMAT(pbs.Date,'dd-MMM-yyyy') as BookingDate,FORMAT(pbs.ValidationDate,'dd-MMM-yyyy') as ValidateDate,CONVERT(varchar(5),pbs.[Time],108)[BookingTime],crl.UserName as Location,cp.UserName as CropPlanning,p.UserName as Customer,ics.Name as IcsMaster,ics.Id as ICSMasterID,fm.Id as Farmer,fm.FarmerName,fm.Id as FarmerFatherHusbandNameId,fm.FarmerFatherHusbandName,fm.Id as FarmerRegId,fm.FarmerRegistrationID as FarmerRegistration,0 BookDays,kk.BookingStatus                                                              
                                                                from TRN.PurchaseBookingSoda pbs
                                                                left join HKP.CropRateLocation crl on crl.Id=pbs.LocationId
																left join TRN.CropPlanning cp on cp.Id=pbs.CropPlanningId
																left join HKP.Party p on p.Id=pbs.CustomerId
																left join MST.ICSMaster ics on ics.Id=cp.ICSMasterID
																left join TRN.CropPlanning on cp.Id=pbs.CropPlanningId
																left join TRN.CropPlanningChild cpc on cpc.CropPlanningMasterId=pbs.CropPlanningId
																left join MST.FarmerMaster fm on fm.Id=cpc.FarmerId
																left join (
													select BookingStatus='Booked',PurchaseBookingSodaMasterId FROM TRN.PurchaseBookingSodaChild
													) kk on kk.PurchaseBookingSodaMasterId=pbs.Id
													where pbs.IsConfirmed=0) AS TEMP WHERE " + strkey + " order by Date desc ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private string GetSBPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PurchaseBookingSoda", out sID);
            return sID;
        }


        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
             
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0  && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "PBS" + GetSBPK();
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

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
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where PurchaseBookingSodaMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("After Soda Booking you can't delete it.");
                    }
                }

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



        // *************** Purchase Booking Soda Child Tab ***************************

        [HttpPost]
        public JsonResult SavePurchaseBookingSodaChild(Dictionary<string, object> data, IEnumerable<PurchaseBookingSodaChild> PBSChildTabData,string PurchaseBookingSodaChildMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                foreach (var item in PBSChildTabData)
                {
                    item.Id = null;
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + item.Id + "'", out dsMaster, false, "1");
                    

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = GetPBSCPK();

                        dr["PurchaseBookingSodaMasterId"] = PurchaseBookingSodaChildMasterId;
                        dr["CropPlanningChildId"] = item.CPCId;
                        dr["Quantity"] = item.Quantity;
                        dr["Rate"] = item.Rate;
                        dr["TargetRate"] = item.TargetRatee;
                        dr["Remarks"] = item.Remarks;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
         
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["Quantity"] = item.Quantity;
                        dr["Rate"] = item.Rate;
      
                        dr["Remarks"] = item.Remarks;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
                }

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllPBSChildTabForSelection(string PurchaseBookingSodaMasterId,string CropPlanningId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            
            string sql = @"select distinct cpc.Id as CPCId,cm.UserName as Crop,kk.TotalQuantity,CQ.TotalConfirmedQuantity, 0 Active,cm.Id as CMId,ct.UserName as CropType,ct.Id as CTId,cpc.PlanQuantity,cpc.CropPlanningMasterId,dcr.Id as DCRId,dcr.TargetRate as TargetRatee,0 Amount 
                                                   ,BalanceBook=cpc.PlanQuantity-(ISNULL(kk.TotalQuantity,'0')),BalancePurchase=cpc.PlanQuantity-(ISNULL(CQ.TotalConfirmedQuantity,'0'))
                                                    from TRN.PurchaseBookingSodaChild pbsc
                                                    full join TRN.CropPlanningChild cpc on cpc.Id=pbsc.CropPlanningChildId
                                                    full join MST.CropMaster cm on cm.Id=cpc.CropId
													full join HKP.CropType ct on ct.Id=cpc.CropTypeId
													inner join MST.DailyCroprate dcr on dcr.CropId=cpc.CropId and dcr.CropTypeId=cpc.CropTypeId
													left join (
													select SUM(quantity) as TotalQuantity,CropPlanningChildId FROM TRN.PurchaseBookingSodaChild group by CropPlanningChildId
													) kk on kk.CropPlanningChildId=cpc.id
													left join (
													select SUM(ConfirmationQuantity) as TotalConfirmedQuantity,CropPlanningChildId FROM TRN.PurchaseBookingSodaChild group by CropPlanningChildId
													) CQ on CQ.CropPlanningChildId=cpc.id
													where cpc.CropPlanningMasterId='" + CropPlanningId + @"' or pbsc.PurchaseBookingSodaMasterId='" + PurchaseBookingSodaMasterId + @"' ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet]
        public JsonResult LoadAllSelectedPBSChildTab(string PurchaseBookingSodaMasterId)
        {
            string sql = @"select distinct pbsc.*,cm.UserName as CropName,ct.UserName as CropType from TRN.PurchaseBookingSodaChild pbsc
                                                      left join TRN.CropPlanningChild cpc on cpc.Id=pbsc.CropPlanningChildId
                                                      left join MST.CropMaster cm on cm.Id=cpc.CropId
													  left join HKP.CropType ct on ct.Id=cpc.CropTypeId
                                                      where PurchaseBookingSodaMasterId= '" + PurchaseBookingSodaMasterId + "' order by CropName ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        private string GetPBSCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseBookingSodaChild), out sID);
            return sID;
        }

    }

    public class PurchaseBookingSodaChild : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string PurchaseBookingSodaMasterId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string CropPlanningChildId { get; set; }
        public string CPCId { get; set; }
        public string Quantity { get; set; }
        public string Rate { get; set; }
        public string TargetRate { get; set; }
  //      public string Ratee { get; set; }
        public string TargetRatee { get; set; }
        public string DCRId { get; set; }
        public string BalanceBook { get; set; }
        public string BalancePurchase { get; set; }
        public string Remarks { get; set; }
        public string Amount { get; set; }
        public string ConfirmedQuantity { get; set; }
        public string ConfirmedRate { get; set; }
        public string ApproveQuantity { get; set; }
        public string ApproveRate { get; set; }
        public string PaidQuantity { get; set; }
        public string PaidRate { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

}
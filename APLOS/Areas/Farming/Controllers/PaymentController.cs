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
    public class PaymentController : BaseController
    {
        string TableName = "TRN.PurchaseBookingSoda";
        string TableName1 = "TRN.PurchaseBookingSodaChild";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PaymentController(ISqlRepository R)
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
            return Json(_sqlRepository.GetDataCollection("select distinct Id as Value, FarmerFatherHusbandName as Text from MST.FarmerMaster where Id='" + FarmerID + "' "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getfarmerregistrationid(string FarmerFatherID)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct Id as Value, FarmerRegistrationID as Text from MST.FarmerMaster where Id='" + FarmerFatherID + "' "), JsonRequestBehavior.AllowGet);
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
													select BookingStatus='To be Paid',PurchaseBookingSodaMasterId FROM TRN.PurchaseBookingSodaChild
													) kk on kk.PurchaseBookingSodaMasterId=pbs.Id
													where kk.PurchaseBookingSodaMasterId=pbs.Id and pbs.IsApproved=1 and (pbs.IsPayment=0 or pbs.IsPayment=1) and pbs.IsVoucher=0) AS TEMP WHERE " + strkey + " order by Date desc ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<PurchaseBookingSodaChild> HeaderConfirmedChildTabData, string IsPaymentData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet dsMaster;
                DataSet dsMaster1;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                foreach (var item in HeaderConfirmedChildTabData)
                {
               
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where PurchaseBookingSodaMasterId='" + item.Id + "'", out dsMaster, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                       
                    }
                    else
                    {
                
                        //edit
                      
                        for(int i=0;i<dsMaster.Tables[0].Rows.Count;i++)
                        {
                            DataRow dr = dsMaster.Tables[0].DefaultView[i].Row;

                            dr.BeginEdit();

                            dr["PaymentQuantity"] = dsMaster.Tables[0].Rows[i]["ApprovedQuantity"].ToString();
                            dr["PaymentRate"] = dsMaster.Tables[0].Rows[i]["ApprovedRate"].ToString();

                            dr.EndEdit();
                        }
                     
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + item.Id + "'", out dsMaster1, false, "1");
                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        //edit
                        DataRow dr = dsMaster1.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();
                        dr["IsPayment"] = IsPaymentData;
                        dr["PaymentBy"] = identity.Name;
                        dr["PaymentDate"] = System.DateTime.Now.ToString();
                        dr["PaymentIP"] = identity.IPAddress;


                        dr.EndEdit();
                    }
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsMaster1);
                }

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

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
        public JsonResult SavePurchaseBookingSodaChild(IEnumerable<PurchaseBookingSodaChild> PBSChildTabData, string PurchaseBookingSodaChildMasterId, string IsPaymentData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet dsMaster;
                DataSet dsMaster1;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                foreach (var item in PBSChildTabData)
                {
    
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + item.Id + "'", out dsMaster, false, "1");
                    

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                       
                    }
                    else
                    {
               
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["PaymentQuantity"] = item.PaidQuantity;
                        dr["PaymentRate"] = item.PaidRate;
       

                        dr.EndEdit();
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + PurchaseBookingSodaChildMasterId + "'", out dsMaster1, false, "1");
                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        //edit
                        DataRow dr = dsMaster1.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();
                        dr["IsPayment"] = IsPaymentData;
                        dr["PaymentBy"] = identity.Name;
                        dr["PaymentDate"] = System.DateTime.Now.ToString();
                        dr["PaymentIP"] = identity.IPAddress;
                        

                        dr.EndEdit();
                    }
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsMaster1);
                }

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllPBSChildTabForSelection(string PurchaseBookingSodaMasterId, string CropPlanningId)
        {

            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "";
                if (!string.IsNullOrEmpty(PurchaseBookingSodaMasterId))
                {

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id= '" + PurchaseBookingSodaMasterId + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0 && dsMaster.Tables[0].Rows[0]["IsPayment"].ToString() == "True")
                    {
                        sql = @"select distinct pbsc.*, cpc.Id as CPCId,cm.UserName as Crop,kk.TotalQuantity, 0 Active,cm.Id as CMId,ct.UserName as CropType,ct.Id as CTId,cpc.PlanQuantity,cpc.CropPlanningMasterId,dcr.Id as DCRId,dcr.TargetRate as TargetRatee,0 Amount,
                                                    PaidQuantity=(pbsc.PaymentQuantity),PaidRate=(pbsc.PaymentRate)
                                                   ,BalanceBook=cpc.PlanQuantity-kk.TotalQuantity,BalancePurchase=cpc.PlanQuantity-PQ.TotalPaidQuantity,PQ.TotalPaidQuantity,pbsc.PurchaseBookingSodaMasterId,ApproveAmount=pbsc.ApprovedQuantity*pbsc.ApprovedRate
                                                    from TRN.PurchaseBookingSodaChild pbsc
                                                    full join TRN.CropPlanningChild cpc on cpc.Id=pbsc.CropPlanningChildId
                                                    full join MST.CropMaster cm on cm.Id=cpc.CropId
													full join HKP.CropType ct on ct.Id=cpc.CropTypeId
													inner join MST.DailyCroprate dcr on dcr.CropId=cpc.CropId and dcr.CropTypeId=cpc.CropTypeId
													left join (
													select SUM(quantity) as TotalQuantity,CropPlanningChildId FROM TRN.PurchaseBookingSodaChild group by CropPlanningChildId
													) kk on kk.CropPlanningChildId=cpc.id
													left join (
													select SUM(PaymentQuantity) as TotalPaidQuantity,CropPlanningChildId FROM TRN.PurchaseBookingSodaChild group by CropPlanningChildId
													) PQ on PQ.CropPlanningChildId=cpc.id
													where pbsc.PurchaseBookingSodaMasterId='" + PurchaseBookingSodaMasterId + @"' order by Crop ";

                    }
                    else
                    {
                        sql = @"select distinct pbsc.*, cpc.Id as CPCId,cm.UserName as Crop,kk.TotalQuantity, 0 Active,cm.Id as CMId,ct.UserName as CropType,ct.Id as CTId,cpc.PlanQuantity,cpc.CropPlanningMasterId,dcr.Id as DCRId,dcr.TargetRate as TargetRatee,0 Amount,
                                                    PaidQuantity=(pbsc.ApprovedQuantity),PaidRate=(pbsc.ApprovedRate)
                                                   ,BalanceBook=cpc.PlanQuantity-kk.TotalQuantity,BalancePurchase=cpc.PlanQuantity-PQ.TotalPaidQuantity,PQ.TotalPaidQuantity,pbsc.PurchaseBookingSodaMasterId,ApproveAmount=pbsc.ApprovedQuantity*pbsc.ApprovedRate
                                                    from TRN.PurchaseBookingSodaChild pbsc
                                                    full join TRN.CropPlanningChild cpc on cpc.Id=pbsc.CropPlanningChildId
                                                    full join MST.CropMaster cm on cm.Id=cpc.CropId
													full join HKP.CropType ct on ct.Id=cpc.CropTypeId
													inner join MST.DailyCroprate dcr on dcr.CropId=cpc.CropId and dcr.CropTypeId=cpc.CropTypeId
													left join (
													select SUM(quantity) as TotalQuantity,CropPlanningChildId FROM TRN.PurchaseBookingSodaChild group by CropPlanningChildId
													) kk on kk.CropPlanningChildId=cpc.id
													left join (
													select SUM(PaymentQuantity) as TotalPaidQuantity,CropPlanningChildId FROM TRN.PurchaseBookingSodaChild group by CropPlanningChildId
													) PQ on PQ.CropPlanningChildId=cpc.id
													where pbsc.PurchaseBookingSodaMasterId='" + PurchaseBookingSodaMasterId + @"' order by Crop ";
                    }

                }

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

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

}
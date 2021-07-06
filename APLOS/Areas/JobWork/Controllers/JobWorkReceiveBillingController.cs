using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using OTSBD;
using Library.MaterialManagement.JobWork;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Data;

namespace Aplos.Areas.JobWork.Controllers
{
    public class JobWorkReceiveBillingController : BaseController
    {
        JobWorkReceiptValueAdded R = new JobWorkReceiptValueAdded();

        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkReceiveBillingController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
            R = new JobWorkReceiptValueAdded();
        }
        #endregion
        #region Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region Load Data


        [Authorize, HttpGet]
        public JsonResult GetReceiptTransChildData(string PKId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(R.GetReceiptTransChildData(PKId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetContractList(string column, string value, string Type)
        {
            string sql = "";
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            try
            {
                sql = @"select vac.Id,TabType='Value Added', vac.EntityId,vac.VendorPartyId,vac.Remarks,FORMAT(vac.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),vac.[Time],108)[VACTime]
			,FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
			FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
			e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
			from dbo.JobWorkValueAddedContract vac left join ORG.Entity e on e.Id=vac.EntityId
			left join HKP.Party p on p.Id=vac.VendorPartyId
 
			union
			select tc.Id,TabType='Transformation', tc.EntityId,tc.VendorPartyId,tc.Remarks,FORMAT(tc.Date,'dd-MMM-yyyy') as ValueAddedDate
			,CONVERT(varchar(5),tc.[Time],108)[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
			FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
			e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
			from dbo.JobWorkTransformationContract tc left join ORG.Entity e on e.Id=tc.EntityId
			left join HKP.Party p on p.Id=tc.VendorPartyId";

                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetInventoryReceiveByTransformationContractId(string contractId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
                return Json(obj.GetInventoryReceiveByTransformationContractId(identity.PlantId, contractId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetInventoryReceiveDetailByOutSourcePO(string contractId)
        {
            try
            {
                string sql = @"SELECT B.Id,CTC.Id JWTransformationContractChildId
                            ,CTC.MaterialMasterId,MM.UserName MaterialName
                            ,ART.Id ArticleId,ART.StandardName Article,CTC.FirstCharacteristicsId,FC.UserName AS SKU1 ,CTC.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue
                            ,CTC.SecondCharacteristicsId,SC.UserName AS SKU2,CTC.SecondCharacteristicsValueId,SCV.UserName AS SecondCharacteristicsValue
                            ,CTC.ThirdCharacteristicsId,TC.UserName AS SKU3,CTC.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue
                            ,CTC.Quantity OrderQty,IRD.TransactionQty ReceiveQty,ISNULL(B.BillingQty,0) BillingQty,(IRD.TransactionQty-ISNULL(B.BillingQty,0)) BalanceQty
                            from [dbo].[JobWorkTransformationContractChild] CTC 
                            LEFT JOIN dbo.JobWorkTransformationContract JWTC ON JWTC.Id=CTC.JobWorkTransformationContractMasterId
                            LEFT JOIN MST.MaterialMaster AS MM ON CTC.MaterialMasterId = MM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON CTC.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON CTC.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON CTC.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON CTC.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON CTC.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON CTC.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON CTC.ThirdCharacteristicsValueId = TCV.Id
                            LEFT JOIN (select SUM(TransactionQty) TransactionQty,JWTCMId from TRN.InventoryReceiveDetail GROUP BY JWTCMId) IRD ON IRD.JWTCMId=CTC.JobWorkTransformationContractMasterId
                            LEFT JOIN (Select JWTransformationContractChildId,BillingQty,Id from dbo.JWReceiveBilling) B ON B.JWTransformationContractChildId=CTC.Id
                            WHERE  CTC.JobWorkTransformationContractMasterId ='"+ contractId + "'";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                R.Create(data);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

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

        #endregion

       

        

    }
}
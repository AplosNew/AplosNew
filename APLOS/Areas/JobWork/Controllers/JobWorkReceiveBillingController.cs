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

        #region Operations


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

                sql = @"SELECT '' Id,tc.Id JWTransformationPurchaseOrderId,TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate
			            ,FORMAT(tc.[Time],'hh:mm tt')[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
			            FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
			            e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName,tc.CurrencyId,CU.Code Currency,TC.PurchaseLCId,ISNULL(LC.LCRef,'')LCRef,PT.PaymentMode,ISNULL(CN.ContractNo,'')ContractNo
			            from [dbo].[JWTransformationPurchaseOrder] tc
			            left join ORG.Entity e on e.Id=tc.EntityId
			            left join HKP.Party p on p.Id=tc.PartyId
			            LEFT JOIN [SCS].[Currency] AS CU ON tc.CurrencyId=CU.Id
			            LEFT JOIN dbo.PurchaseLC LC ON LC.Id=TC.PurchaseLCId
			            LEFT JOIN MST.PaymentTerm PT ON PT.Id=TC.PaymentTermId
						LEFT JOIN dbo.[Contract] CN ON CN.Id=TC.ContractId
                        WHERE tc.PlantId='" + identity .PlantId+ "'";

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
        public ActionResult GetJWReceiveBillingDetailData(string masterId)
        {
            try
            {
                string sql = @"SELECT RBD.*,CTC.Id JWTransformationContractChildId,CTC.MaterialMasterId,MM.UserName MaterialName
                            ,ART.Id ArticleId,ART.StandardName Article,CTC.FirstCharacteristicsId,FC.UserName AS SKU1 ,CTC.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue
                            ,CTC.SecondCharacteristicsId,SC.UserName AS SKU2,CTC.SecondCharacteristicsValueId,SCV.UserName AS SecondCharacteristicsValue
                            ,CTC.ThirdCharacteristicsId,TC.UserName AS SKU3,CTC.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue
                            ,CTC.Quantity OrderQty,IRD.TransactionQty ReceiveQty,ISNULL(B.BillingQty,0) OtherBillingQty,(IRD.TransactionQty-ISNULL(B.BillingQty,0)) BalanceQty
                            from  dbo.JWReceiveBillingDetail RBD 
                            LEFT JOIN [dbo].[JobWorkTransformationContractChild] CTC ON CTC.Id=RBD.JWTransformationContractChildId
                            --LEFT JOIN dbo.JobWorkTransformationContract JWTC ON JWTC.Id=CTC.JobWorkTransformationContractMasterId
                            --LEFT JOIN [dbo].[JWTransformationPurchaseOrder] JWPO ON JWPO.Id=CTC.JobWorkTransformationContractMasterId
                            LEFT JOIN MST.MaterialMaster AS MM ON CTC.MaterialMasterId = MM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON CTC.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON CTC.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON CTC.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON CTC.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON CTC.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON CTC.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON CTC.ThirdCharacteristicsValueId = TCV.Id
                            LEFT JOIN (select SUM(TransactionQty) TransactionQty,JWTCMDId,MaterialTranRate from TRN.InventoryReceiveDetail GROUP BY JWTCMDId,MaterialTranRate) IRD ON IRD.JWTCMDId=CTC.Id
                            LEFT JOIN (Select JWTransformationContractChildId,SUM(BillingQty) BillingQty from dbo.JWReceiveBillingDetail WHERE JWReceiveBillingId<>'" + masterId + @"' GROUP BY JWTransformationContractChildId ) B ON B.JWTransformationContractChildId=CTC.Id
                            WHERE RBD.JWReceiveBillingId='" + masterId + "'";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetJWReceiveBillingData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT RB.*,TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate
			                ,FORMAT(tc.[Time],'hh:mm tt')[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
			                FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
			                e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName,tc.CurrencyId,CU.Code Currency
                            ,TC.PurchaseLCId,ISNULL(LC.LCRef,'')LCRef,ISNULL(CN.ContractNo,'')ContractNo
			                from [dbo].[JWReceiveBilling] RB
			                LEFT JOIN [dbo].[JWTransformationPurchaseOrder] tc ON tc.Id=RB.JWTransformationPurchaseOrderId
			                LEFT JOIN ORG.Entity e on e.Id=tc.EntityId
			                LEFT JOIN HKP.Party p on p.Id=tc.PartyId
                            LEFT JOIN [SCS].[Currency] AS CU ON tc.CurrencyId=CU.Id
							LEFT JOIN dbo.PurchaseLC LC ON LC.Id=TC.PurchaseLCId
							LEFT JOIN dbo.[Contract] CN ON CN.Id=TC.ContractId 
                            Where RB.PlantId='" + identity.PlantId + "'";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetInventoryReceiveDetailByOutSourcePO(string contractId)
        {
            try
            {
                string sql = @"SELECT NULL Id,CTC.Id JWTransformationContractChildId,CTC.MaterialMasterId,MM.UserName MaterialName
                            ,ART.Id ArticleId,ART.StandardName Article,CTC.FirstCharacteristicsId,FC.UserName AS SKU1 ,CTC.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue
                            ,CTC.SecondCharacteristicsId,SC.UserName AS SKU2,CTC.SecondCharacteristicsValueId,SCV.UserName AS SecondCharacteristicsValue
                            ,CTC.ThirdCharacteristicsId,TC.UserName AS SKU3,CTC.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue
                            ,CTC.Quantity OrderQty,IRD.TransactionQty ReceiveQty,ISNULL(B.BillingQty,0) OtherBillingQty,0 BillingQty,(IRD.TransactionQty-ISNULL(B.BillingQty,0)) BalanceQty,IRD.MaterialTranRate
                            from [dbo].[JobWorkTransformationContractChild] CTC 
                            --LEFT JOIN dbo.JobWorkTransformationContract JWTC ON JWTC.Id=CTC.JobWorkTransformationContractMasterId
                            LEFT JOIN [dbo].[JWTransformationPurchaseOrder] JWPO ON JWPO.Id=CTC.JobWorkTransformationContractMasterId
                            LEFT JOIN MST.MaterialMaster AS MM ON CTC.MaterialMasterId = MM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON CTC.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON CTC.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON CTC.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON CTC.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON CTC.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON CTC.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON CTC.ThirdCharacteristicsValueId = TCV.Id
                            LEFT JOIN (select SUM(TransactionQty) TransactionQty,JWTCMDId,MaterialTranRate,MaterialFor from TRN.InventoryReceiveDetail GROUP BY JWTCMDId,MaterialTranRate,MaterialFor) IRD ON IRD.JWTCMDId=CTC.Id
                            LEFT JOIN (Select JWTransformationContractChildId,SUM(BillingQty) BillingQty from dbo.JWReceiveBillingDetail GROUP BY JWTransformationContractChildId) B ON B.JWTransformationContractChildId=CTC.Id
                            WHERE  CTC.JobWorkTransformationContractMasterId ='" + contractId + "' AND IRD.MaterialFor='JWOUTPUTMaterial'";

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
        public JsonResult Create(Dictionary<string, object> master, List<Dictionary<string, object>> data)
        {
            try
            {
                SaveData(master, data);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

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
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JWReceiveBilling", out sID);
            return sID;
        }

        private void SaveData(Dictionary<string, object> master, List<Dictionary<string, object>> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            objCon = new ConnectionManager.DAL.ConManager("1");
            DataSet dsMaster, dsBills;
            try
            {
                string _Id = "";
                string masterId = "";
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.JWReceiveBilling Where Id='" + master["Id"] + "'", out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.JWReceiveBillingDetail Where JWReceiveBillingId='" + master["Id"] + "'", out dsBills, false, "1");

                if (master != null)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JWReceiveBilling", out _Id);

                        master["Id"] = _Id;
                        master["PlantId"] = identity.PlantId;
                        AddNewRow(dsMaster.Tables[0], master);
                    }
                    else
                    {
                        _Id = master["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], master);
                    }

                    masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    if (data != null)
                    {


                        foreach (var item in data)
                        {
                            DataView dv = new DataView(dsBills.Tables[0]);
                            dv.RowFilter = "Id='" + item["Id"] + "'";

                            if (dv.Count == 0)
                            {
                                item["Id"] = GetPK();
                                item["JWReceiveBillingId"] = masterId;
                                AddNewRow(dsBills.Tables[0], item);
                            }
                            else
                            {
                                DataRow drmo = dv[0].Row;
                                EditRow(drmo, item);
                            }
                        }
                    }

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsBills);


            }
            catch (Exception ex)
            {
                throw ex;
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

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL, strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strCSQL = "DELETE FROM [dbo].[JWReceiveBillingDetail] WHERE JWReceiveBillingId='" + Id + "'";
                strSQL = "DELETE FROM [dbo].[JWReceiveBilling] WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        #endregion





    }
}
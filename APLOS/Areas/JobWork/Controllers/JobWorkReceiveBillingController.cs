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
using System.Globalization;

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
        public ActionResult GetJWReceiveBillingDetailData(string masterId, string contractId, string inventoryReceiveIds)
        {
            try
            {
                string sql = @"SELECT B.Id,CTC.Id JWTransformationContractChildId,CTC.MaterialMasterId,MM.UserName MaterialName,ART.Id ArticleId
	                        ,ART.StandardName Article,CTC.FirstCharacteristicsId,FC.UserName AS SKU1,CTC.FirstCharacteristicsValueId
	                        ,FCV.UserName AS FirstCharacteristicsValue,CTC.SecondCharacteristicsId,SC.UserName AS SKU2,CTC.SecondCharacteristicsValueId
	                        ,SCV.UserName AS SecondCharacteristicsValue,CTC.ThirdCharacteristicsId,TC.UserName AS SKU3
	                        ,CTC.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue,IRD.MaterialFor
	                         ,CTC.Quantity OrderQty,SUM(IRD.TransactionQty) ReceiveQty,OB.OtherBillingQty,B.BillingQty,(SUM(IRD.TransactionQty)-OB.OtherBillingQty) BalanceQty, CTC.RatePerUnit MaterialTranRate,B.Amount
                        FROM TRN.InventoryReceiveDetail IRD
                        JOIN [TRN].[InventoryReceive] IR ON IR.Id = IRD.InventoryReceiveId
                        JOIN [dbo].[JWTransformationPurchaseOrder] JWPO ON IR.TransformationContractId = JWPO.Id
                        JOIN [dbo].[JobWorkTransformationContractChild] CTC ON JWPO.Id = CTC.JobWorkTransformationContractMasterId
                        LEFT JOIN MST.MaterialMaster AS MM ON CTC.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON CTC.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON CTC.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON CTC.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON CTC.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON CTC.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON CTC.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON CTC.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN dbo.JWReceiveBillingDetail B ON B.JWTransformationContractChildId=CTC.Id AND B.JWReceiveBillingId='" + masterId +@"'
                        LEFT JOIN (SELECT JWTransformationContractChildId,ISNULL(SUM(BillingQty),0) OtherBillingQty FROM dbo.JWReceiveBillingDetail 
                        WHERE JWReceiveBillingId<> '"+masterId+@"'
                        GROUP BY JWTransformationContractChildId) OB ON OB.JWTransformationContractChildId=CTC.Id
                        WHERE JWTCMId = '"+ contractId + @"' AND InventoryReceiveId "+ inventoryReceiveIds + @" AND IRD.MaterialFor = 'JWOUTPUTMaterial'
                        GROUP BY B.Id,CTC.Id,CTC.MaterialMasterId,MM.UserName,ART.Id,ART.StandardName,CTC.FirstCharacteristicsId,FC.UserName,CTC.FirstCharacteristicsValueId
                        ,FCV.UserName,CTC.SecondCharacteristicsId,SC.UserName,CTC.SecondCharacteristicsValueId,SCV.UserName,CTC.ThirdCharacteristicsId
                        ,TC.UserName,CTC.ThirdCharacteristicsValueId,TCV.UserName,CTC.Quantity, CTC.RatePerUnit,IRD.MaterialFor,OB.OtherBillingQty,B.BillingQty,B.Amount";

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
        public ActionResult GetInventoryReceiveDetailByOutSourcePO(string masterId,string contractId, string inventoryReceiveIds)
        {
            try
            {
                string sql = @"SELECT B.Id,CTC.Quantity OrderQty,SUM(IRD.TransactionQty) ReceiveQty,B.BillingQty,ISNULL(BIL.OtherBillQty,0) OtherBillQty,IRD.InventoryMaterialId
                            ,IM.MaterialMasterId,MM.UserName MaterialName,ART.Id ArticleId,ART.StandardName Article
                            ,IM.FirstCharacteristicsId,FC.UserName AS SKU1,IM.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue
                            ,IM.SecondCharacteristicsId,SC.UserName AS SKU2,IM.SecondCharacteristicsValueId
                            ,SCV.UserName AS SecondCharacteristicsValue,IM.ThirdCharacteristicsId,TC.UserName AS SKU3
                            ,IM.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue
                            FROM TRN.InventoryReceiveDetail IRD
                            LEFT JOIN [dbo].[JWTransformationPurchaseOrder] JWPO ON IRD.JWTCMId = JWPO.Id
                            JOIN (SELECT SUM(Quantity)Quantity,JobWorkTransformationContractMasterId from [dbo].[JobWorkTransformationContractChild] 
                            GROUP BY JobWorkTransformationContractMasterId) CTC ON JWPO.Id = CTC.JobWorkTransformationContractMasterId
                            LEFT JOIN TRN.InventoryMaterial IM ON IM.Id = IRD.InventoryMaterialId
                            LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                            LEFT JOIN dbo.JWReceiveBillingDetail B ON B.JWTransformationContractChildId = IRD.JWTCMDId AND B.JWReceiveBillingId='"+ masterId + @"'
                            LEFT JOIN (
                            SELECT JWTransformationContractChildId,Sum(BillingQty) OtherBillQty FROM dbo.JWReceiveBillingDetail 
                            WHERE JWReceiveBillingId<>'"+ masterId + @"' GROUP BY JWTransformationContractChildId
                            ) BIL ON BIL.JWTransformationContractChildId=IRD.JWTCMDId
                            where IRD.InventoryReceiveId "+ inventoryReceiveIds + @" AND IRD.JWTCMId='"+ contractId + @"' AND IRD.MaterialFor = 'JWOUTPUTMaterial'
                            GROUP BY CTC.Quantity,IRD.InventoryMaterialId,IM.MaterialMasterId,MM.UserName,ART.Id
                            ,ART.StandardName,IM.FirstCharacteristicsId,FC.UserName,IM.FirstCharacteristicsValueId
                            ,FCV.UserName,IM.SecondCharacteristicsId,SC.UserName,IM.SecondCharacteristicsValueId
                            ,SCV.UserName,IM.ThirdCharacteristicsId,TC.UserName
                            ,IM.ThirdCharacteristicsValueId,TCV.UserName,B.BillingQty,BIL.OtherBillQty,B.Id";

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
                            Where RB.PlantId='" + identity.PlantId + "' ORDER BY RB.AddedDate DESC";

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
        public ActionResult GetSavedGRNList(string masterId)
        {
            try
            {
                string sql = @"SELECT * FROM JWReceiveBillingGRN WHERE JWReceiveBillingId='" + masterId+"'";

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
        public JsonResult Create(Dictionary<string, object> master, List<Dictionary<string, object>> grnList, List<Dictionary<string, object>> data)
        {
            try
            {
                SaveData(master, grnList,data);
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
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JWReceiveBillingDetail", out sID);
            return sID;
        }
        private string GetGRNPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JWReceiveBillingGRN", out sID);
            return sID;
        }

        private void SaveData(Dictionary<string, object> master, List<Dictionary<string, object>> grnList, List<Dictionary<string, object>> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            objCon = new ConnectionManager.DAL.ConManager("1");
            DataSet dsMaster, dsGRNBills, dsBills;
            try
            {
                string _Id = "";
                string masterId = "";
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.JWReceiveBilling Where Id='" + master["Id"] + "'", out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.JWReceiveBillingGRN Where JWReceiveBillingId='" + master["Id"] + "'", out dsGRNBills, false, "1");
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
                    if (grnList != null)
                    {
                        foreach (var item in grnList)
                        {
                            DataView dv = new DataView(dsGRNBills.Tables[0]);
                            dv.RowFilter = "Id='" + item["Id"] + "'";

                            if (dv.Count == 0)
                            {
                                item["Id"] = GetGRNPK();
                                item["JWReceiveBillingId"] = masterId;
                                AddNewRow(dsGRNBills.Tables[0], item);
                            }
                            else
                            {
                                DataRow drmo = dv[0].Row;
                                EditRow(drmo, item);
                            }
                        }
                    }

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
                obj.SaveDataSets(dsMaster, dsGRNBills,dsBills);


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
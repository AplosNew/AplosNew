#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Service.OrderManagements;
using Library.Service.Parties;
using Library.Service.Productions;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Library.Model.Enums;
using OTSBD;
using Library.Data.Sql;
using System.Linq;
using Library.Service.Core;
using Library.Service.Enums;
using Aplos.Helpers;
using Newtonsoft.Json;
#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class InquiryMasterController : BaseController
    {
        #region -- Constructor

        private readonly IMasterOrderService _masterOrderService;
        private readonly IPartyService _partyService;
        private readonly ICustomerPOService _customerPOService;
        private readonly ISqlRepository _sqlRepository;

        public InquiryMasterController(IMasterOrderService masterOrderService, IPartyService partyService, ICustomerPOService customerPOService, ISqlRepository sqlRepository)
        {
            _masterOrderService = masterOrderService;
            _partyService = partyService;
            _customerPOService = customerPOService;
            _sqlRepository = sqlRepository;

        }

        #endregion

        #region -- Pages
        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetSalesOrderTaxCategoryList(string salesOrderId)
        {
            return Json(_masterOrderService.GetSalesOrderTaxCategoryList(salesOrderId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string masterOrderId, string plantId, string hsnCodeId, string specialTaxId, string PODate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId) || plantId == "null") plantId = identity.PlantId;
            return Json(_masterOrderService.GetTaxCategoryList(identity.CompanyGroupId, masterOrderId, plantId, hsnCodeId, specialTaxId, PODate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeListResponsible(GridParameter parameters, string plantId, string partyAccountGroupId, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(_masterOrderService.GetEmployeeListResponsible(parameters, identity.CompanyId, plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFirstSkuList(string salesOrderId)
        {
            return Json(_masterOrderService.GetFirstSkuSalesOrderId(salesOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllSkuSalesOrderId(string salesOrderId)
        {
            var firstData = _masterOrderService.GetFirstSkuSalesOrderId(salesOrderId);
            var secondtData = _masterOrderService.GetSecondSkuSalesOrderId(salesOrderId);
            var thirdData = _masterOrderService.GetThirdSkuSalesOrderId(salesOrderId);
            return Json(new { firstData, secondtData, thirdData }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetCriticalLevelData()
        {
            string query = @"select Id,UserName from hkp.Critical";
            return Json(_sqlRepository.GetDataCollection(query), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetInquiryTypeData()
        {
            string query = @"select Id,UserName from hkp.InquiryType";
            return Json(_sqlRepository.GetDataCollection(query), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetChValueCbo(string materialId)
        {
            return Json(_masterOrderService.GetChValueCbo(materialId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsByMaterialMasterId(string materialMasterId)
        {
            return Json(_masterOrderService.GetCharacteristicsByMaterialMasterId(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSOandItemList(string masterItemId)
        {
            return Json(_masterOrderService.GetSOList(masterItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult GetList(GridParameter parameters, string companyId)
        {
            return Json(_masterOrderService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAttributeListByMaterialMasterId(string materialMasterId)
        {
            var sql = @"SELECT NULL AS Id
                            , MMA.MaterialMasterId
                            , MMA.MaterialAttributeId AS AttributeId
                            , MA.UserName AS AttributeName
                            , MMA.IsFreeField
                            , MMA.IsPreDefinedField
                            , MMA.IsMandatory
		                    , MAV.Id AS AttributeValueId
		                    , ValueFreeText=MAV.UserName
                            , MA.ValueAssignmentLevel
		                    , MAV.SourceType
                    FROM MST.MaterialMasterAttribute AS MMA
                    LEFT JOIN HKP.MaterialAttribute AS MA ON MMA.MaterialAttributeId = MA.Id
                    LEFT JOIN (SELECT * FROM HKP.MaterialAttributeValue WHERE Active=1 AND IsDefault=1) AS MAV 
		                    ON MAV.MaterialAttributeId=MMA.MaterialAttributeId AND MAV.SourceType=MA.ValueAssignmentLevel
                    WHERE MMA.MaterialMasterId='" + materialMasterId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOrderAttributeListByMasterId(string masterItemId, string materialMasterId)
        {
            var sql = @"SELECT MOV.Id
                                , MMA.MaterialMasterId
                                , MOV.InquiryItemId
                                , MMA.MaterialAttributeId AS AttributeId
                                , MA.UserName AS AttributeName
                                , MMA.IsFreeField
                                , MMA.IsPreDefinedField
                                , MMA.IsMandatory
		                        , AttributeValueId=CASE WHEN (MOV.Id IS NULL AND MAV.IsDefault=1) THEN MAV.Id 
								                        WHEN MOV.Id<>'' THEN MOV.AttributeValueId END
		                        , ValueFreeText =CASE WHEN (MOV.Id IS NULL AND MAV.IsDefault=1) THEN MAV.UserName
							                          WHEN MOV.AttributeValueId<>'' THEN MAV.UserName ELSE MOV.ValueFreeText END
                                , MOV.ValueRemarks, MA.ValueAssignmentLevel, MAV.SourceType, MOV.ReferenceSampleandRemarks
                        FROM MST.MaterialMasterAttribute AS MMA
                        LEFT JOIN HKP.MaterialAttribute AS MA ON MMA.MaterialAttributeId = MA.Id
                        LEFT JOIN (SELECT A.*, B.MaterialMasterId FROM TRN.InquiryAttributeValue AS A
			                        JOIN TRN.InquiryItem AS B ON A.InquiryItemId=B.Id WHERE B.Id='" + masterItemId + @"') AS MOV 
			                        ON MOV.AttributeId=MMA.MaterialAttributeId AND MMA.MaterialMasterId=MOV.MaterialMasterId
                        LEFT JOIN (SELECT * FROM HKP.MaterialAttributeValue WHERE Active=1) AS MAV 
		                        ON MAV.MaterialAttributeId=MMA.MaterialAttributeId AND MOV.AttributeValueId=MAV.Id AND MAV.SourceType=MA.ValueAssignmentLevel
                        WHERE MMA.MaterialMasterId='" + materialMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetArticleCodeList(string materialMasterId, string articleCode)
        {
            return Json(_masterOrderService.GetArticleCodeList(materialMasterId, articleCode), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSpecialTaxList(string plantId)
        {
            return Json(_masterOrderService.GetSpecialTaxList(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskList(string buyerId, string buyerDepartmentId, string buyerDivisionId, string moId)
        {
            return Json(_masterOrderService.GetTaskList(buyerId, buyerDepartmentId, buyerDivisionId, moId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCompanyPartyDataList(GridParameter parameters, string companyId, string plantId, string partyType)
        {
            if (plantId == "null") plantId = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_masterOrderService.GetCompanyPartyList(parameters, identity.CompanyGroupId, companyId, plantId, partyType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GeInquiryItemList(string InquiryMasterId)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT MOI.Id, MOI.InquiryMasterId, MOI.NoOfSample,MOI.Remarks,MOI.InquiryProcess
	                         , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                         , MOI.ArticleId, ART.StandardName AS ArticleName
	                         , MOI.BuyerReferenceNo, MOI.OwnReferenceNo, MOI.ProjectedQty
	                         , null AS InquiryProcessList 
							 , ISNULL(HART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute,MOI.CostingRequired,MOI.Particulars
                             , ISNULL((select sum(SO.Qty) from TRN.SalesOrder SO where So.MasterOrderItemId = MOI.Id),0) as SOQty,MOI.Type,MOI.IsRepeat, PM.UserName AS ProductMaster
                        FROM TRN.InquiryItem AS MOI
                        JOIN MST.MaterialMaster AS MM ON MOI.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON MOI.ArticleId=ART.Id
						LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0 THEN COUNT(MaterialMasterId) ELSE 0 END
                                                , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
                                            FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS HART ON HART.MaterialMasterId=MM.Id
                        LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId= MM.Id
						LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                        WHERE MOI.InquiryMasterId='" + InquiryMasterId + @"'";

            string SqlInProcess = "select * from InquiryProcess where InquiryMasterId='" + InquiryMasterId + @"'";

            List<Dictionary<string, object>> inProcData = _sqlRepository.GetDataCollection(SqlInProcess, null);

            List<Dictionary<string, object>> inItemData = _sqlRepository.GetDataCollection(sql, null);

            for (int i = 0; i < inItemData.Count; i++)
            {
                List<Dictionary<string, object>> itemProcData = inProcData.Where(ip => ip["InquiryItemId"] == inItemData[i]["Id"]).ToList();

                inItemData[i]["InquiryProcessList"] = itemProcData;
            }

            return Json(inItemData, JsonRequestBehavior.AllowGet);
        }

        private bool isEmpty(object value)
        {
            if (value == null)
                return true;


            return string.IsNullOrEmpty(value.ToString());
        }

        [HttpPost, ChaildAction(ParentActionName = "Edit")]
        public JsonResult CreateAttributeValue(string masterItemId, List<Dictionary<string, object>> attributeValueList)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsAttribute;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from TRN.InquiryAttributeValue where InquiryItemId='" + masterItemId + "'", out dsAttribute, false, "1");
                if (attributeValueList == null)
                {
                    while (dsAttribute.Tables[0].DefaultView.Count > 0)
                    {
                        dsAttribute.Tables[0].DefaultView[0].Row.Delete();
                    }
                }

                for (int i = 0; i < dsAttribute.Tables[0].Rows.Count; i++)
                {
                    try
                    {
                        List<Dictionary<string, object>> k = attributeValueList.Where(kk => kk["Id"].ToString() == dsAttribute.Tables[0].Rows[i]["id"].ToString()).ToList();
                        if (k.Count == 0)
                        {
                            dsAttribute.Tables[0].Rows[i].Delete();
                        }
                    }
                    catch (Exception)
                    {

                    }

                }
                string newId = "";
                for (int i = 0; i < attributeValueList.Count; i++)
                {
                    if (isEmpty(attributeValueList[i]["AttributeValueId"]) && isEmpty(attributeValueList[i]["ValueFreeText"]) && isEmpty(attributeValueList[i]["ValueRemarks"]))
                        continue;

                    dsAttribute.Tables[0].DefaultView.RowFilter = "Id = '" + attributeValueList[i]["Id"] + @"'";

                    if (dsAttribute.Tables[0].DefaultView.Count == 0)
                    {
                        if (newId == "")
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("TRN.InquiryItem", out newId);
                        }
                        DataRow dr = dsAttribute.Tables[0].NewRow();

                        foreach (var item in attributeValueList[i].Keys)
                        {
                            try
                            {
                                dr[item] = attributeValueList[i][item];
                            }
                            catch (Exception)
                            {
                            }
                        }
                        dr["Id"] = newId + (i + 1).ToString();
                        dr["InquiryItemId"] = masterItemId;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsAttribute.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsAttribute.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        foreach (var item in attributeValueList[i].Keys)
                        {
                            try
                            {
                                dr[item] = attributeValueList[i][item];
                            }
                            catch (Exception)
                            {
                            }
                        }
                        dr["InquiryItemId"] = masterItemId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                }



                clsStaticInfo _info = new clsStaticInfo();


                _info.SaveDataSets(dsAttribute);

                return Json(new { Message = AplosMessage.Updated, Error = false });
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true });
            }
            //_masterOrderService.InsertOrUpdateGraphInquiryAttributeValues(masterItemId, attributeValueList);

        }
        //[HttpGet, Authorize]
        //public ActionResult GetDepartmentPersonList(string plantId, string partyAccountGroupId, string partyId, bool flag)
        //{
        //    return Json(_masterOrderService.GetDepartmentPersonList(plantId, partyAccountGroupId, partyId, flag), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public ActionResult GetResponsiblePersonList(string masterId)
        {
            return Json(_masterOrderService.GetResponsiblePersonList(masterId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetDepartmentPersonCbo(string plantId, string partyAccountGroupId, string partyId)
        //{
        //    return Json(_masterOrderService.GetDepartmentPersonCbo(plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
        //}
        [HttpPost,Authorize]
        public ActionResult GetList(string column, string value, string companyId)
        {
            string strkey = "1 =1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT A.Id, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId	
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId	
                                    , A.SeasonId, A.OrderYear, A.ProjectedQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' )
								    
								    ,A.BuyerDepartmentId
									,A.InquiryClosingDate,A.InquirySource,A.InquiryDate,CRT.Id CriticalLevelId
								    ,A.ProjectQtyUOMId,PL.UserName,A.Type,A.SpecialTaxId,PM.UserName ProductMaster
                                    ,A.OwnReferenceNo,A.BuyerReferenceNo
                            FROM [TRN].[InquiryMaster] AS A
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN Hkp.Critical AS CRT ON A.CriticalLevelId=CRT.Id
                            LEFT JOIN Hkp.InquiryType AS INQTYPE ON A.InquiryTypeId=INQTYPE.Id                        
                            LEFT JOIN TRN.Commitment COM ON COM.Id=A.CommitmentId
							LEFT JOIN [MST].[ProductMaster] PM ON COM.ProductMasterId=PM.Id
                             WHERE A.CompanyId='" + companyId + @"'
                                ) AS TEMP WHERE " + strkey;



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateORUpdate(Dictionary<string, object> data, string itemData)
        {
            try
            {
                List<Dictionary<string, object>> itemLists = null;
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                itemLists = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(itemData, settings);


                string rowitem = "";
                try
                {
                    rowitem = itemLists[0]["InquiryProcessList"].ToString();
                }
                catch (Exception)
                {

                }


                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from TRN.InquiryMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("TRN.InquiryMaster", out _Id);
                    _Id = DateTime.Now.ToString("yy") + _Id;
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                DataSet dsItem = new DataSet();
                DataSet dsInquiryProcess = new DataSet();

                CreateORUpdateInquiryItem(_Id, itemLists, out dsItem, out dsInquiryProcess);

                _info.SaveDataSets(dsMaster, dsItem, dsInquiryProcess);
                return Json(new { Error = false, Id = _Id, Message = AplosMessage.Updated });

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

                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery(@"DELETE FROM trn.InquiryAttributeValue WHERE InquiryItemId IN (SELECT Id FROM trn.InquiryItem WHERE InquiryMasterId='" + id + @"')");
                connection.executeQuery(@"DELETE FROM trn.InquiryItem WHERE InquiryMasterId='" + id + "'");
                connection.executeQuery(@"DELETE FROM trn.InquiryMaster WHERE Id='" + id + @"'");
                connection.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        private void CreateORUpdateInquiryItem(string masterId, List<Dictionary<string, object>> itemList, out DataSet dsMaster, out DataSet dsInProcMaster)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = new DataSet();
            dsInProcMaster = new DataSet();

            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            string sqlText = "SELECT * FROM TRN.InquiryItem WHERE InquiryMasterId = '" + masterId + @"'";
            con.OpenDataSetThroughAdapter(sqlText, out dsMaster, false, "1");

            string sqlInProcText = "SELECT * FROM InquiryProcess WHERE InquiryMasterId = '" + masterId + @"'";
            con.OpenDataSetThroughAdapter(sqlInProcText, out dsInProcMaster, false, "1");

            //while (dsInProcMaster.Tables[0].DefaultView.Count > 0)
            //{
            //    dsInProcMaster.Tables[0].DefaultView[0].Row.Delete();
            //}



            #region Delete
            if (itemList == null)
            {
                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }
                return;
            }
            DataTable dtMaster = dsMaster.Tables[0];
            for (int i = 0; i < dtMaster.Rows.Count; i++)
            {
                try
                {
                    List<Dictionary<string, object>> k = itemList.Where(kk => kk["Id"].ToString() == dtMaster.Rows[i]["id"].ToString()).ToList();
                    if (k.Count == 0)
                    {
                        dtMaster.Rows[i].Delete();
                    }
                }
                catch (Exception)
                {
                }

            }
            #endregion

            #region SaveEdit
            string newId = "";
            string processNewId = "";
            string itemCurrentId = "";


            for (int i = 0; i < itemList.Count; i++)
            {
                dsMaster.Tables[0].DefaultView.RowFilter = "Id = '" + itemList[i]["Id"] + @"'";

                if (dsMaster.Tables[0].DefaultView.Count == 0)
                {
                    if (newId == "")
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.InquiryItem", out newId);
                    }
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = masterId + newId + (i + 1).ToString();
                    itemCurrentId = dr["Id"].ToString();
                    dr["InquiryMasterId"] = masterId;
                    dr["MaterialMasterId"] = itemList[i]["MaterialMasterId"];
                    dr["ArticleId"] = itemList[i]["ArticleId"];
                    dr["BuyerReferenceNo"] = itemList[i]["BuyerReferenceNo"];
                    dr["OwnReferenceNo"] = itemList[i]["OwnReferenceNo"];
                    dr["ProjectedQty"] = itemList[i]["ProjectedQty"];
                    dr["Type"] = itemList[i]["Type"];
                    dr["IsRepeat"] = itemList[i]["IsRepeat"];
                    dr["NoOfSample"] = itemList[i]["NoOfSample"];
                    dr["Remarks"] = itemList[i]["Remarks"];
                    dr["InquiryProcess"] = itemList[i]["InquiryProcess"];
                    dr["Particulars"] = itemList[i]["Particulars"];
                    dr["CostingRequired"] = itemList[i]["CostingRequired"];


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
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    itemCurrentId = dr["Id"].ToString();
                    dr["MaterialMasterId"] = itemList[i]["MaterialMasterId"];
                    dr["ArticleId"] = itemList[i]["ArticleId"];
                    dr["BuyerReferenceNo"] = itemList[i]["BuyerReferenceNo"];
                    dr["OwnReferenceNo"] = itemList[i]["OwnReferenceNo"];
                    dr["ProjectedQty"] = itemList[i]["ProjectedQty"];
                    dr["Type"] = itemList[i]["Type"];
                    dr["IsRepeat"] = itemList[i]["IsRepeat"];
                    dr["NoOfSample"] = itemList[i]["NoOfSample"];
                    dr["Remarks"] = itemList[i]["Remarks"];
                    dr["InquiryProcess"] = itemList[i]["InquiryProcess"];
                    dr["Particulars"] = itemList[i]["Particulars"];
                    dr["CostingRequired"] = itemList[i]["CostingRequired"];

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                try
                {

                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    List<Dictionary<string, object>> processList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(itemList[i]["InquiryProcessList"].ToString(), settings);

                    for (int ip = 0; ip < processList.Count; ip++)
                    {
                        if (processNewId == "")
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("InquiryProcess", out processNewId);
                        }

                        DataRow dr = dsInProcMaster.Tables[0].NewRow();
                        dr["Id"] = processNewId + i.ToString() + ip.ToString();
                        dr["ProcessName"] = processList[ip]["ProcessName"].ToString();
                        dr["IsApplicable"] = processList[ip]["IsApplicable"];


                        dr["InquiryItemId"] = itemCurrentId;
                        dr["InquiryMasterId"] = masterId;

                        dsInProcMaster.Tables[0].Rows.Add(dr);


                    }
                }
                catch (Exception)
                {

                }

            }
            #endregion

            // return dsMaster;
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

        [HttpGet, Authorize]
        public JsonResult GetInquiryProcessCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<InquiryProcessEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        #region -- Customer Po

        [HttpGet, Authorize]
        public JsonResult GetListByMasterOrder(string companyId, string masterOrderId)
        {
            return Json(_customerPOService.GetListByMasterOrder(companyId, masterOrderId).Rows, JsonRequestBehavior.AllowGet);
        }


        #endregion -- Customer Po

        #region Report

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderReport(string masterOrderId)
        {
            try
            {
                // ReportFormat reportFormat = "pdf";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                _masterOrderService.GetProformaInvoiceReportService(identity.CompanyId, identity.PlantId, masterOrderId);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
        #endregion

        #region Inquriy Process
        [HttpPost, Authorize]
        public ActionResult GetInquiryProcessList(string inquiryMasterId, string inquiryItemId)
        {
            DataTable dtInquiryProcess = null;

            string sql = @"SELECT * FROM InquiryProcess where InquiryMasterId = '" + inquiryMasterId + @"' and InquiryItemId = '" + inquiryItemId + @"' ";
            dtInquiryProcess = _sqlRepository.GetDataTable(sql);
            dtInquiryProcess.Columns.Add("ProcessDesc");

            foreach (var item in Enum.GetValues(typeof(InquiryProcessEnum)))
            {
                dtInquiryProcess.DefaultView.RowFilter = "ProcessName='" + item.ToString() + "'";
                if (dtInquiryProcess.DefaultView.Count == 0)
                {
                    DataRow dr = dtInquiryProcess.NewRow();
                    dr["ProcessName"] = item.ToString();
                    dr["ProcessDesc"] = item.GetDescription().ToString();
                    dr["IsApplicable"] = 0;
                    dr["InquiryItemId"] = inquiryItemId;
                    dr["InquiryMasterId"] = inquiryMasterId;


                    dtInquiryProcess.Rows.Add(dr);
                }
                else
                {
                    dtInquiryProcess.DefaultView[0].Row["ProcessDesc"] = item.GetDescription().ToString();
                }
            }


            //SavePayRegisterReportConfig(null);
            return Json(CustomJsonResult.DataTableToJson(dtInquiryProcess), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveInquiryProcess(List<Dictionary<string, object>> data, string inquiryMasterId, string inquiryItemId)
        {

            try
            {

                DataTable dtCheckConfig = null;
                //string sql = @"SELECT distinct FieldName FROM PayRegisterReportConfig ";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string newId = "";
                bplib.clsGenID genid = new bplib.clsGenID();
                con.OpenDataSetThroughAdapter("SELECT * FROM InquiryProcess where InquiryMasterId = '" + inquiryMasterId + @"' and InquiryItemId = '" + inquiryItemId + @"'", out dsMaster, false, "1");
                foreach (var item in Enum.GetValues(typeof(InquiryProcessEnum)))
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "ProcessName='" + item.ToString() + "'";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {

                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["ProcessName"] = item.ToString();
                        if (data != null)
                        {
                            genid.GenID("InquiryProcess", out newId);
                            dr["Id"] = "IP" + newId;

                            List<Dictionary<string, object>> curValue = data.Where(dic => dic["ProcessName"].ToString() == item.ToString()).ToList();
                            foreach (var val in curValue)
                            {

                                dr["IsApplicable"] = bplib.clsWebLib.GetBoolData(val["IsApplicable"].ToString());
                            }
                        }
                        dr["InquiryItemId"] = inquiryItemId;
                        dr["InquiryMasterId"] = inquiryMasterId;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        if (data != null)
                        {
                            List<Dictionary<string, object>> curValue = data.Where(dic => dic["FieldName"].ToString() == item.ToString()).ToList();
                            foreach (var val in curValue)
                            {
                                dr["IsApplicable"] = bplib.clsWebLib.GetBoolData(val["IsApplicable"].ToString());
                            }
                        }
                        dr.EndEdit();

                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Error = false, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
}
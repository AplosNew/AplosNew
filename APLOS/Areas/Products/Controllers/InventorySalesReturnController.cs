using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Service.Invoices;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data;
using System.Linq;
using Library.Data.Sql;
using Library.Accounting.Accounts;
using Library.Core;
using System;
using System.Data;
using Library.Security.Core;
using Library.MaterialManagement.Inventory;
using Library.ViewModel.Materials;
using Library.Model.Inventory;
using Library.ViewModel.SalesManagements;

namespace Aplos.Areas.Products.Controllers
{
    public class InventorySalesReturnController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly AccountsSalesService _accountsSalesService;
        private readonly IInventoryIssueService _inventoryIssueService;
        public InventorySalesReturnController(
             ISqlRepository sqlRepository
            , AccountsSalesService accountsSalesService
            ,IInventoryIssueService inventoryIssueService
            )
        {
            _sqlRepository = sqlRepository;
            _accountsSalesService = accountsSalesService;
            _inventoryIssueService = inventoryIssueService;
        }

        

        #region Inventory Sales Posting
        
        public ActionResult Aplos()
        {
            return View();
        }

      
        [Authorize, HttpGet]
        public JsonResult GetInventorySaleDetailGLList(string inventorySalesId, string customerId)
        {
            AccountsInventorySalesService accountsInventorySalesService = new AccountsInventorySalesService(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(accountsInventorySalesService.GetInventorySaleDetailGLListData(identity.CompanyId, identity.PlantId, inventorySalesId, customerId), JsonRequestBehavior.AllowGet);
        }


        //[HttpPost]
        //public JsonResult Create(Dictionary<string, object> entity, List<Dictionary<string, object>> attributes)
        //{
        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        if (entity != null)
        //        {

        //            DataRow dr;

        //            DataSet dsMaster;
        //            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

        //            con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductLibrary WHERE Id='" + entity["Id"] + "'", out dsMaster, false, "1");
        //            con.OpenDataSetThroughAdapter("select * from dbo.ProductLibrary where Code='" + entity["Code"] + "' AND  Id<>'" + entity["Id"] + "'", out DataSet dsCodeMaster, false, "1");
        //            if (dsCodeMaster.Tables[0].Rows.Count > 0)
        //                throw new Exception("Same Code already exists!!!");

        //            con.OpenDataSetThroughAdapter("select * from dbo.ProductLibrary where UserName='" + entity["UserName"] + "' AND  Id<>'" + entity["Id"] + "'", out DataSet dsUserMaster, false, "1");
        //            if (dsUserMaster.Tables[0].Rows.Count > 0)
        //                throw new Exception("Same User Name already exists!!!");


        //            string _Id = "";
        //            string _DId = "";

        //            #region data update
        //            if (dsMaster.Tables[0].Rows.Count == 0)
        //            {
        //                bplib.clsGenID genid = new bplib.clsGenID();
        //                genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductLibrary", out _Id);

        //                entity["CompanyGroupId"] = identity.CompanyGroupId;

        //                entity["AddedBy"] = identity.Name;
        //                entity["AddedDate"] = System.DateTime.Now.ToString();
        //                entity["AddedFromIP"] = identity.IPAddress;

        //                entity["Id"] = "PL" + _Id;
        //                _Id = entity["Id"].ToString();
        //                AddNewRow(dsMaster.Tables[0], entity);
        //            }
        //            else
        //            {
        //                _Id = entity["Id"].ToString();
        //                EditRow(dsMaster.Tables[0].Rows[0], entity);
        //            }

        //            #endregion data update

        //            #region Child 

        //            DataSet dsChild;


        //            con.OpenDataSetThroughAdapter("select * from  where  ProductLibraryId='" + _Id + "'", out dsChild, false, "1");
        //            #region data update


        //            if (attributes != null)
        //            {
        //                foreach (var item in attributes)
        //                {
        //                    bplib.clsGenID genid = new bplib.clsGenID();
        //                    genid.GenID("", out _DId);

        //                    DataView dv = new DataView(dsChild.Tables[0]);
        //                    dv.RowFilter = "Id='" + item["Id"] + "'";

        //                    if (dv.Count == 0)
        //                    {
        //                        item["Id"] = _DId;
        //                        item["ProductLibraryId"] = _Id;
        //                        AddNewRow(dsChild.Tables[0], item);
        //                    }
        //                    else
        //                    {
        //                        DataRow drmo = dv[0].Row;
        //                        EditRow(drmo, item);

        //                    }
        //                }
        //            }
        //            #endregion

        //            #endregion


        //            clsStaticInfo _info = new clsStaticInfo();
        //            _info.SaveDataSets(dsMaster, dsChild);



        //        }
        //        return Json(new { Error = false, Data = entity, Message = AplosMessage.Insert });
        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message });

        //    }
        //}

        //private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    DataRow dr = dt.NewRow();

        //    foreach (var item in sourceData.Keys)
        //    {
        //        try
        //        {
        //            dr[item] = sourceData[item];
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }

        //    dr["AddedBy"] = identity.Name;
        //    dr["AddedDate"] = System.DateTime.Now.ToString();
        //    dr["AddedFromIP"] = identity.IPAddress;

        //    dr["UpdatedBy"] = identity.Name;
        //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
        //    dr["UpdatedFromIP"] = identity.IPAddress;

        //    dt.Rows.Add(dr);
        //}
        //private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    dr.BeginEdit();

        //    foreach (var item in sourceData.Keys)
        //    {
        //        try
        //        {
        //            dr[item] = sourceData[item];
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }


        //    dr["UpdatedBy"] = identity.Name;
        //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
        //    dr["UpdatedFromIP"] = identity.IPAddress;

        //    dr.EndEdit();
        //}


        [HttpPost]
        public JsonResult Create(InventorySalesReturn inventoryIssue,IEnumerable<InventorySalesReturnDetailViewModel> entities,  IEnumerable<InventorySalesReturnService> salesServiceVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
            inventoryIssue.CompanyId = identity.CompanyId;
            inventoryIssue.PlantId = identity.PlantId;
            _inventoryIssueService.SalesReturnInsert(inventoryIssue,entities, salesServiceVMList);
            return Json(new { inventoryIssue, Message = AplosMessage.Success + "Sales No=" + inventoryIssue.Id }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesDetailByIssueId(string issueId)
        {
            return Json(GetSalesDetailDataByIssueId(issueId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(Query(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> Query(string plantId)
        {
            try
            {
                string CmdText = @"SELECT E.UserName AS Entity , II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                                , MS.UserName AS MaterialStorage,SUM(IID.TransactionQty) Qty,II.Remarks,II.InventorySalesId
                                FROM [TRN].[InventorySalesReturn] AS II
                                JOIN TRN.InventorySalesReturnDetail AS IID ON IID.InventorySalesReturnId=II.Id
                                JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId=MS.Id
                                Left JOIN [ORG].[Entity] E On E.id=II.EntityId
                                WHERE II.PlantId='"+ plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                                , MS.UserName,E.UserName,II.Remarks,II.Id,II.InventorySalesId";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSalesDetailDataByIssueId(string issueId)
        {
            try
            {
                string sql = @"SELECT ISH.Id HistotyId,''Id, IID.InventorySalesId InventoryIssueId, IID.InventoryMaterialId, II.MaterialStorageId
		                        , IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                        , IM.FirstCharacteristicsId, CH1.UserName AS FirstCharacteristics, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicText--FirstCharacteristicsValue
		                        , IM.SecondCharacteristicsId, CH2.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicText--SecondCharacteristicsValue
		                        , IM.ThirdCharacteristicsId, CH3.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicText--ThirdCharacteristicsValue
		                        , IID.TransactionQty, IID.BaseUOMId, UoM.UserName AS TransactionUoM, IID.AvgRate, IID.AvgAmount, IID.PolicyRate, IID.PolicyAmount, IID.[Policy]
                                ,CC.UserName CostCenter,C.UserName CountryName,c.Id CountryId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.NoteForAccounts
                                 ,ISD.SalesRate,ISD.TotalAmount,IST.TaxAmount,NULL TaxList
                        FROM [TRN].[InventorySalesDetail] AS IID
                        LEFT JOIN [TRN].[InventorySales] AS II ON IID.InventorySalesId=II.Id
                        LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IID.InventoryMaterialId=IM.Id
                        LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IM.ArticleId=AR.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH1 ON IM.FirstCharacteristicsId=CH1.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IM.FirstCharacteristicsValueId=CHV1.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON IM.SecondCharacteristicsId=CH2.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IM.SecondCharacteristicsValueId=CHV2.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH3 ON IM.ThirdCharacteristicsId=CH3.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IM.ThirdCharacteristicsValueId=CHV3.Id
						LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IID.CostCenterId
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IID.BaseUOMId=UoM.Id
                        LEFT JOIN scs.country C On C.Id=IM.CountryId
                        LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=IID.Id
                        JOIN (select InventorySalesHistoryId,Sum(TaxAmount) TaxAmount from trn.inventorySalesTax group by InventorySalesHistoryId) IST ON IST.InventorySalesHistoryId =ISH.Id
                        LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id) ISD ON ISD.Id=IID.Id

                        WHERE IID.InventorySalesId='" + issueId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
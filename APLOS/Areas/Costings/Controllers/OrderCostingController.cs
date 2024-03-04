#region Using

using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Costings;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


#endregion Using

namespace Aplos.Areas.Costings.Controllers
{
    public class OrderCostingController : BaseController
    {
        string TableName = "dbo.OrderCostingMasterTemplate";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        Library.Service.Materials.MaterialMasterService _Materialservice;
        public OrderCostingController(ISqlRepository R, Library.Service.Materials.MaterialMasterService M)
        {
            _sqlRepository = R;
            _Materialservice = M;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            string sql = @"select * from '" + TableName + "' wher Id = '" + Id + "' ";
            try
            {
                var _master = _sqlRepository.GetDataCollection(sql);


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetInquiryList(string column, string value)

        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (SELECT im.Id,im.InquirySource,FORMAT( im.InquiryDate,'dd-MMM-yyyy') AS InquiryDate, im.OrderYear,im.ProjectedQty,uom.UserName AS UOM,p.UserName AS Party,b.UserName AS Buyer,bb.UserName AS BuyerBrand,bd.UserName AS BuyerDivision,
                                                    bd2.UserName AS BuyerDepartment,s.UserName AS Season,ei.EmployeeName AS ResponsiblePerson,im.ResponsiblePersonId
                                                      FROM trn.InquiryMaster AS im
                                                    LEFT OUTER JOIN hkp.Party AS p ON p.Id=im.PartyId
                                                    LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=im.BuyerId
                                                    LEFT OUTER JOIN hkp.BuyerBrand AS bb ON bb.Id=im.BuyerBrandId
                                                    LEFT OUTER JOIN hkp.BuyerDivision AS bd ON bd.Id=im.BuyerDivisionId
                                                    LEFT OUTER JOIN hkp.BuyerDepartment AS bd2 ON bd2.Id=im.BuyerDepartmentId
                                                    LEFT OUTER JOIN hkp.Season AS s ON s.Id=im.SeasonId
                                                    LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=im.ResponsiblePersonId
                                                    LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=im.ProjectQtyUOMId
                                                    WHERE im.id IN (SELECT ii.InquiryMasterId FROM trn.InquiryItem AS ii WHERE ii.CostingRequired=1 AND isnull(ii.Id,'') NOT IN (SELECT isnull(InquiryItemId,'')
                                                                                                                                                                   FROM OrderCostingMasterTemplate))
                                                    AND ei.PlantId='" + identity.PlantId + @"'
                            ) AS TEMP WHERE 1=1 AND " + strkey + @" ORDER BY convert(datetime,InquiryDate) DESC";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);


            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetInquiryItemList(string Id)

        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT ii.Id,ii.BuyerReferenceNo,mm.UserName AS Material,mma.StandardName AS Article, ii.OwnReferenceNo, ii.ProjectedQty, ii.NoOfSample,
                            pd.ProductMasterId,p.UserName AS Product,
                                       ii.[Type]
                                  FROM trn.InquiryItem AS ii
                                LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=ii.MaterialMasterId
                                LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=ii.ArticleId
                                LEFT JOIN [TRN].[ProductDefinition] PD ON pd.MaterialMasterId=mm.Id
                                LEFT JOIN mst.ProductMaster AS pm ON pm.Id=pd.ProductMasterId
                                LEFT JOIN hkp.Product AS p ON p.Id=pm.ProductId


                                WHERE ii.InquiryMasterId='" + Id + @"' AND ii.CostingRequired=1";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);


            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetMasterOrderList(string column, string value)

        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (SELECT im.Id,FORMAT( im.AddedDate,'dd-MMM-yyyy') AS MasterOrderDate, 
im.OrderYear,im.TotalQty,uom.UserName AS UOM,p.UserName AS Party,b.UserName AS Buyer,
bb.UserName AS BuyerBrand,bd.UserName AS BuyerDivision,
ContractNo=STUFF((select distinct ','+cx.ContractNo from  trn.SalesOrder SO
																INNER JOIN trn.MasterOrderItem XMOI ON SO.MasterOrderItemId=XMOI.Id
								                                INNER JOIN [Contract] AS cx ON cx.Id=SO.ContractId                                               
							                                where XMOI.masterOrderId=IM.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

LCRef=STUFF((select distinct ','+mlx.LCRef from  trn.SalesOrder SO
																INNER JOIN trn.MasterOrderItem XMOI ON SO.MasterOrderItemId=XMOI.Id
								                                INNER JOIN [Contract] AS cx ON cx.Id=SO.ContractId 
								                                INNER JOIN MasterLC AS mlx ON mlx.Id=cx.MasterLCId                                              
							                                where XMOI.masterOrderId=IM.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
    bd2.UserName AS BuyerDepartment,s.UserName AS Season,ei.EmployeeName AS ResponsiblePerson,im.ResponsiblePersonId
        FROM trn.MasterOrder AS im
    LEFT OUTER JOIN hkp.Party AS p ON p.Id=im.PartyId
    LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=im.BuyerId
    LEFT OUTER JOIN hkp.BuyerBrand AS bb ON bb.Id=im.BuyerBrandId
    LEFT OUTER JOIN hkp.BuyerDivision AS bd ON bd.Id=im.BuyerDivisionId
    LEFT OUTER JOIN hkp.BuyerDepartment AS bd2 ON bd2.Id=im.BuyerDepartmentId
    LEFT OUTER JOIN hkp.Season AS s ON s.Id=im.SeasonId
    LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=im.ResponsiblePersonId
    LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=im.TotalQtyUOMId
    WHERE im.id IN (SELECT ii.MasterOrderId FROM trn.MasterOrderItem AS ii WHERE isnull(ii.Id,'') NOT IN (SELECT isnull(MasterOrderItemId,'')
                                                                                                                    FROM OrderCostingMasterTemplate))
   AND im.PlantId='" + identity.PlantId + @"'
                            ) AS TEMP WHERE 1=1 AND " + strkey + @" ORDER BY convert(datetime,MasterOrderDate) DESC";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);


            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetMasterOrderItemList(string Id)

        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT convert(bit,0) AS isChecked, convert(bit,CASE WHEN isnull(ii.OrderCostingMasterTemplateId,'')='' THEN 0 ELSE 1 END) AS TakenForCosting,
                            ii.Id, ii.Id AS MasterOrderItemId,ii.BuyerReferenceNo,mm.UserName AS Material,mma.StandardName AS Article, ii.OwnReferenceNo, ii.TotalQty, 
                            pd.ProductMasterId,p.UserName AS Product,pm.Id as ProductMasterId,ii.[Type]
									,ContractNo=STUFF((select distinct ','+cx.ContractNo from [Contract] AS cx
								                                INNER JOIN  trn.SalesOrder SO ON cx.Id=SO.ContractId                                               
							                                where SO.MasterOrderItemId=II.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

LCRef=STUFF((select distinct ','+mlx.LCRef from MasterLC AS mlx 
																
								                                INNER JOIN [Contract] AS cx ON mlx.Id=cx.MasterLCId  
																INNER JOIN  trn.SalesOrder SO ON cx.Id=SO.ContractId                                             
							                                where SO.MasterOrderItemId=II.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                  FROM trn.MasterOrderItem AS ii
                                LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=ii.MaterialMasterId
                                LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=ii.ArticleId
                                LEFT JOIN [TRN].[ProductDefinition] PD ON pd.MaterialMasterId=mm.Id
                                LEFT JOIN mst.ProductMaster AS pm ON pm.Id=pd.ProductMasterId
                                LEFT JOIN hkp.Product AS p ON p.Id=pm.ProductId
                                WHERE ii.MasterOrderId='" + Id + "'";

            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);
            return Json(new { DATA = data }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)

        {
            string strkey = "1=1  order by TEMP.AddedDate Desc";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory,CUR.Code AS Currency,ct.UserName AS CostingTypeName
							,psc.UserName as ProductSubCategory
                             ,pm.CostingType,qcm.CostingStage AS CurrentCostStage
							 ,TotalQty=(select sum(TotalQty) from  trn.MasterOrderItem where OrderCostingMasterTemplateId=qcm.Id)
							  ,MOIId=STUFF((select distinct ','+moi.Id from   trn.MasterOrderItem MOI 
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							 ,BuyerReferenceNo=STUFF((select distinct ','+moi.BuyerReferenceNo from   trn.MasterOrderItem MOI 
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,OwnReferenceNo=STUFF((select distinct ','+moi.OwnReferenceNo from   trn.MasterOrderItem MOI 
								                             where moi.OrderCostingMasterTemplateId=qcm.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							from OrderCostingMasterTemplate qcm 
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join scs.Currency CUR on CUR.Id=qcm.CurrencyId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType 
                            ) AS TEMP WHERE 1=1 AND " + strkey ;


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);
            Dictionary<string, string> CostingType = new Dictionary<string, string>();
            foreach (var item in Enum.GetValues(typeof(CostingType)))
            {
                CostingType.Add(item.ToString(), AccessInfo.GetEnumDescription((CostingType)(int)item));
            }

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    data[i]["CostingType"] = CostingType[data[i]["CostingType"].ToString()];
                }
                catch (Exception)
                {


                }
            }

            string sqlPurchasegroup = "SELECT Id,pg.UserName  FROM org.PurchaseGroup AS pg";



            return Json(new { DATA = data, PurchaseGroup = _sqlRepository.GetDataCollection(sqlPurchasegroup) }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetListOrderCostingForCopy(string column, string value, string ProductMasterId)

        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            if (string.IsNullOrEmpty(ProductMasterId) == false)
                strkey += " AND ProductMasterId='" + ProductMasterId + "'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory,CUR.Code AS Currency,ct.UserName AS CostingTypeName
							,psc.UserName as ProductSubCategory
                             ,pm.CostingType,qcm.CostingStage AS CurrentCostStage
							from OrderCostingMasterTemplate qcm 
							
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join scs.Currency CUR on CUR.Id=qcm.CurrencyId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
                            ) AS TEMP WHERE 1=1 AND " + strkey;


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);
            Dictionary<string, string> CostingType = new Dictionary<string, string>();
            foreach (var item in Enum.GetValues(typeof(CostingType)))
            {
                CostingType.Add(item.ToString(), AccessInfo.GetEnumDescription((CostingType)(int)item));
            }

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    data[i]["CostingType"] = CostingType[data[i]["CostingType"].ToString()];
                }
                catch (Exception)
                {


                }
            }

            string sqlPurchasegroup = "SELECT Id,pg.UserName  FROM org.PurchaseGroup AS pg";



            return Json(new { DATA = data, PurchaseGroup = _sqlRepository.GetDataCollection(sqlPurchasegroup) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetListCostingTemplateForCopy(string column, string value, string ProductMasterId)

        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            if (string.IsNullOrEmpty(ProductMasterId) == false)
                strkey += " AND ProductMasterId='" + ProductMasterId + "'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory,CUR.Code AS Currency,ct.UserName AS CostingTypeName
							,psc.UserName as ProductSubCategory
                             ,pm.CostingType
							from CostingMasterTemplate qcm 
							
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join scs.Currency CUR on CUR.Id=qcm.CurrencyId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
                            ) AS TEMP WHERE 1=1 AND " + strkey;


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);
            Dictionary<string, string> CostingType = new Dictionary<string, string>();
            foreach (var item in Enum.GetValues(typeof(CostingType)))
            {
                CostingType.Add(item.ToString(), AccessInfo.GetEnumDescription((CostingType)(int)item));
            }

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    data[i]["CostingType"] = CostingType[data[i]["CostingType"].ToString()];
                }
                catch (Exception)
                {


                }
            }

            string sqlPurchasegroup = "SELECT Id,pg.UserName  FROM org.PurchaseGroup AS pg";

            return Json(new { DATA = data, PurchaseGroup = _sqlRepository.GetDataCollection(sqlPurchasegroup) }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost, Authorize]
        public ActionResult GetListItem(string Id)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory,ct.UserName AS CostingTypeName
                             ,pm.CostingType,eff.StandardWorkingHours AS StandardWorkingHoursForProduct
							from OrderCostingMasterTemplate qcm 
							
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT JOIN [TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId=qcm.ProductMasterId AND EfficencyName='Costing'  
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
                            WHERE QCM.ID='" + Id + @"'";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);
            Dictionary<string, string> CostingType = new Dictionary<string, string>();
            foreach (var item in Enum.GetValues(typeof(CostingType)))
            {
                CostingType.Add(item.ToString(), AccessInfo.GetEnumDescription((CostingType)(int)item));
            }

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    data[i]["CostingType"] = CostingType[data[i]["CostingType"].ToString()];
                }
                catch (Exception)
                {


                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult OrderBudgetReport(string OrderCostingId, string orderBudget, string preCosting, string ProcurementCosting, string MOIId)
        {
            try
            {
                Library.OrderManagement.Costing.CostingReport Report = new Library.OrderManagement.Costing.CostingReport();
                Report.OrderBudgetReport(OrderCostingId, orderBudget, preCosting, ProcurementCosting, MOIId); 
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            } 
        }

        [HttpGet, Authorize]
        public ActionResult GetOrderCostingReport(string OrderCostingId,string orderBudget,string preCosting, string ProcurementCosting, string MOIId)
        {
            try
            {
                Library.OrderManagement.Costing.CostingReport Report = new Library.OrderManagement.Costing.CostingReport();
                Report.GetOrderCostingReport(OrderCostingId, orderBudget, preCosting, ProcurementCosting, MOIId); 
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            } 
        }

        [HttpPost, Authorize]
        public ActionResult GetSOList(string column, string value, string TemplateId, string MasterOrderItemId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            if (string.IsNullOrEmpty(MasterOrderItemId) == false)
            {
                strkey = "1=1";
                MasterOrderItemId = " MOI.Id='" + MasterOrderItemId + "' ";
            }
            else
            {

                MasterOrderItemId = " isnull(MOI.OrderCostingMasterTemplateId,'')='' ";
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (
                                SELECT convert(bit,0) AS isChecked,MOI.Id AS MasterOrderItemId,
                                convert(bit,CASE WHEN isnull(MOI.OrderCostingMasterTemplateId,'')='' THEN 0 ELSE 1 END) AS TakenForCosting,mm.UserName AS Material,mma.StandardName AS Article,
                                  moi.MasterOrderId,p.Id AS PartyId,pm.UserName AS Product, p.UserName AS Customer, 
                                pm.Id
                                  FROM trn.MasterOrder AS mo
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId
                                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                                left outer join hkp.Party AS p ON p.Id=mo.PartyId 
                               where  " + MasterOrderItemId + @"
                               
                            ) AS TEMP WHERE 1=1 AND " + strkey + " ORDER BY TEMP.MasterOrderId, TEMP.Product";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);


            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetSOListForTemplate(string TemplateId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT convert(bit,0) AS isChecked,MOI.Id AS MasterOrderItemId ,mm.UserName AS Material,mma.StandardName AS Article, moi.MasterOrderId,p.Id AS                                      PartyId,pm.UserName AS Product, p.UserName AS Customer, pm.Id
                                 ,ISNULL(moi.TotalQty,0) TotalQty,moi.BuyerReferenceNo,moi.OwnReferenceNo
									
                                 FROM trn.MasterOrder AS mo
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId
                                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                left outer join hkp.Party AS p ON p.Id=mo.PartyId

                                WHERE isnull(moi.OrderCostingMasterTemplateId,'')='" + TemplateId + @"'
                                ORDER BY mo.Id,pm.UserName
                            ";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult UpdateSOData(string TemplateId, List<Dictionary<string, object>> SOList)
        {

            try
            {
                UpdateSalesOrders(TemplateId, SOList);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Message = "SO Updated Successfully" });

        }

        private void UpdateSalesOrders(string TemplateId, List<Dictionary<string, object>> SOList)
        {
            try
            {


                string _soList = "''";
                for (int i = 0; i < SOList.Count; i++)
                    _soList += ",'" + SOList[i]["MasterOrderItemId"].ToString() + "'";


                string sql = "update trn.MasterOrderItem set OrderCostingMasterTemplateId='" + TemplateId + "' Where Id in (" + _soList + ")";

                _sqlRepository.ExecuteSqlCommand(sql);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteSOData(string TemplateId, string SOId)
        {

            try
            {

                string sql = "update trn.MasterOrderItem set OrderCostingMasterTemplateId=NULL Where OrderCostingMasterTemplateId='" + TemplateId + "' AND Id ='" + SOId + "'";

                _sqlRepository.ExecuteSqlCommand(sql);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Message = "SO Updated Successfully" });

        }


        [HttpPost, Authorize]
        public ActionResult GetCostingItemForSelection(string CostingStage, string OrderCostingMasterTemplateId, string costingComponentId, string Segment)
        {
            string TableName = "";
            string aND = "";
            if (CostingStage == "PRE")
            {
                if (Segment == CostingSegment.DirectMaterial.ToString())
                {
                    TableName = "OrderPreCostingDirectMaterial";
                    aND = "AND ci.IsSubMaterial = 0";
                }
                else if (Segment == CostingSegment.DirectProcess.ToString())
                    TableName = "OrderPreCostingDirectProcess";
                else if (Segment == CostingSegment.Operation.ToString())
                    TableName = "OrderPreCostingOperation";
                else if (Segment == CostingSegment.Profit.ToString())
                    TableName = "OrderPreCostingProfit";
                else if (Segment == CostingSegment.SalesExpense.ToString())
                    TableName = "OrderPreCostingSalesExpense";
                else if (Segment == CostingSegment.ValueLoss.ToString())
                    TableName = "OrderPreCostingValueLoss";
            }
            if (CostingStage == "PROCUREMENT")
            {

                if (Segment == CostingSegment.DirectMaterial.ToString())
                {
                    TableName = "OrderProcurementCostingDirectMaterial";
                    aND = "AND ci.IsSubMaterial = 0";
                }
                else if (Segment == CostingSegment.DirectProcess.ToString())
                    TableName = "OrderProcurementCostingDirectProcess";
                else if (Segment == CostingSegment.Operation.ToString())
                    TableName = "OrderProcurementCostingOperation";
                else if (Segment == CostingSegment.Profit.ToString())
                    TableName = "OrderProcurementCostingProfit";
                else if (Segment == CostingSegment.SalesExpense.ToString())
                    TableName = "OrderProcurementCostingSalesExpense";
                else if (Segment == CostingSegment.ValueLoss.ToString())
                    TableName = "OrderProcurementCostingValueLoss";
            }
            string sql = @"SELECT ci.ShortName,cat.UserName AS CostingCategory, CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END) AS Selected, ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                        o.OrderCostingMasterTemplateId,
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                            ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            LEFT OUTER JOIN hkp.CostingCategory AS cat ON cat.Id=ci.CostingCategoryId
                            LEFT join " + TableName + @" o on o.CostingItemId = ci.Id AND o.OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + @"'
                            WHERE ci.CostingComponentId='" + costingComponentId + @"' " + aND + @"
                            ORDER BY CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END), ci.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }



        [HttpPost, Authorize]
        public ActionResult SaveCostingItemsForCostingComponent(string CostingStage, List<Dictionary<string, object>> itemList, string OrderCostingMasterTemplateId, string costingComponentId, string Segment)
        {
            try
            {
                List<string> _stage = new List<string>();
                _stage.Add("PROCUREMENT");
                if (CostingStage == "PRE")
                {
                    _stage.Add("PRE");
                }



                if (itemList == null)
                    throw new Exception("No Data Found");

                string CostingItemIds = "''";
                foreach (var item in itemList)
                    CostingItemIds += ",'" + item["CostingItemId"].ToString() + "'";

                for (int lst = 0; lst < _stage.Count; lst++)
                {


                    string Consumption = "";
                    string TableName = "";
                    if (_stage[lst] == "PRE")
                    {
                        if (Segment == CostingSegment.DirectMaterial.ToString())
                            TableName = "OrderPreCostingDirectMaterial";
                        else if (Segment == CostingSegment.DirectProcess.ToString())
                            TableName = "OrderPreCostingDirectProcess";
                        else if (Segment == CostingSegment.Operation.ToString())
                            TableName = "OrderPreCostingOperation";
                        else if (Segment == CostingSegment.Profit.ToString())
                            TableName = "OrderPreCostingProfit";
                        else if (Segment == CostingSegment.SalesExpense.ToString())
                            TableName = "OrderPreCostingSalesExpense";
                        else if (Segment == CostingSegment.ValueLoss.ToString())
                            TableName = "OrderPreCostingValueLoss";


                    }
                    else if (_stage[lst] == "PROCUREMENT")
                    {
                        if (Segment == CostingSegment.DirectMaterial.ToString())
                            TableName = "OrderProcurementCostingDirectMaterial";
                        else if (Segment == CostingSegment.DirectProcess.ToString())
                            TableName = "OrderProcurementCostingDirectProcess";
                        else if (Segment == CostingSegment.Operation.ToString())
                            TableName = "OrderProcurementCostingOperation";
                        else if (Segment == CostingSegment.Profit.ToString())
                            TableName = "OrderProcurementCostingProfit";
                        else if (Segment == CostingSegment.SalesExpense.ToString())
                            TableName = "OrderProcurementCostingSalesExpense";
                        else if (Segment == CostingSegment.ValueLoss.ToString())
                            TableName = "OrderProcurementCostingValueLoss";
                    }

                    ConnectionManager.DAL.ConManager objCon;

                    DataSet dsConsumption;
                    if (Segment == CostingSegment.DirectMaterial.ToString())
                    {
                        Consumption = "Select * from " + TableName + @"Consumption where CostingItemId in (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";

                    }
                    else
                    {
                        Consumption = "Select * from OrderProcurementCostingDirectMaterialConsumption where 1=2";
                    }
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(Consumption, out dsConsumption, false, "1");


                    DataSet dsMaster;



                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;




                    string sql = "Select * from " + TableName + " where CostingItemId in (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    double MaxSequence = 0;
                    for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                    {
                        if (MaxSequence < clsStaticInfo.dbl(dsMaster.Tables[0].Rows[i]["Sequence"].ToString()))
                            MaxSequence = clsStaticInfo.dbl(dsMaster.Tables[0].Rows[i]["Sequence"].ToString());
                    }

                    DataTable dtMainItems = _sqlRepository.GetDataTable(@"SELECT * FROM hkp.CostingItem AS ci WHERE ci.Id IN  (" + CostingItemIds + ") ");




                    string ConsumptionReference = @"SELECT m.ProductMasterId, m.CostingItemId, m.GSMValue,co.ComponentName,CO.AreaType,CO.NoOfParts,icc.ParameterName,
                                               icc.Parameter, icc.Actual, icc.Allowance, icc.Number AS NoOfParameter, icc.Total
                                                 from ItemConsumtionMaster M
                                               join ItemConsumtionComponent CO ON  m.Id=co.ItemConsumtionMasterId
                                               JOIN ItemConsumtionChild AS icc ON icc.ItemConsumtionComponentId=co.Id AND m.Id=icc.ItemConsumtionMasterId
                                               WHERE m.ProductMasterId=(SELECT top 1 cmt.ProductMasterId
                                               FROM OrderCostingMasterTemplate AS cmt WHERE cmt.Id='" + OrderCostingMasterTemplateId + @"')
                                               AND m.CostingItemId IN (" + CostingItemIds + ") ORDER BY m.CostingItemId, co.ComponentName,CO.AreaType";



                    DataTable dtConsumptionReference = _sqlRepository.GetDataTable(ConsumptionReference);

                    int Index = 0;
                    foreach (var item in itemList)
                    {
                        Index++;
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item["CostingItemId"].ToString() + "'";
                        if (bplib.clsWebLib.GetBoolData(item["Selected"].ToString()))
                        {
                            if (dsMaster.Tables[0].DefaultView.Count > 0)
                                continue;
                        }
                        else
                        {
                            //while (dsMaster.Tables[0].DefaultView.Count > 0)
                            //    dsMaster.Tables[0].DefaultView[0].Delete();
                            while (dsMaster.Tables[0].DefaultView.Count > 0)
                            {
                                dsConsumption.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dsMaster.Tables[0].DefaultView[0]["CostingItemId"].ToString() + "'";
                                while (dsConsumption.Tables[0].DefaultView.Count > 0)
                                {
                                    dsConsumption.Tables[0].DefaultView[0].Delete();
                                }
                                dsMaster.Tables[0].DefaultView[0].Delete();
                            }
                            continue;
                        }
                        if (_id == "")
                        {
                            _id = "" + GetPK("OrderCosting");
                        }

                        MaxSequence++;

                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = _id + Index;
                        dr["CostingItemId"] = item["CostingItemId"];
                        dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                        dr["Sequence"] = MaxSequence;


                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;


                        //setting direct process value loss percentage as default value
                        if (Segment == CostingSegment.DirectProcess.ToString())
                        {
                            dtMainItems.DefaultView.RowFilter = "Id='" + item["CostingItemId"].ToString() + "'";
                            if (dtMainItems.DefaultView.Count > 0)
                            {
                                dr["Value"] = dtMainItems.DefaultView[0]["ValueLossPercentage"];
                            }
                        }


                        if (Segment == CostingSegment.ValueLoss.ToString())
                        {
                            dr["Type"] = "Percentage";
                            DataTable dtProductConfig = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[ProductMasterEfficency] WHERE ProductMasterId=(SELECT ProductMasterId FROM OrderCostingMasterTemplate WHERE Id='" + OrderCostingMasterTemplateId + "') AND EfficencyName='Costing'");
                            dtMainItems.DefaultView.RowFilter = "Code='VLS'";
                            if (dtMainItems.DefaultView.Count > 0)
                            {
                                if (dtProductConfig.Rows.Count > 0)
                                    dr["Value"] = dtProductConfig.Rows[0]["ValueLossPercentage"];
                            }
                        }

                        if (Segment == CostingSegment.DirectMaterial.ToString())
                        {
                            dtConsumptionReference.DefaultView.RowFilter = "CostingItemId='" + item["CostingItemId"] + "'";
                            for (int CONS = 0; CONS < dtConsumptionReference.DefaultView.Count; CONS++)
                            {
                                DataRow drConsumption = dsConsumption.Tables[0].NewRow();
                                CopyRow(dtConsumptionReference.DefaultView[CONS].Row, drConsumption);
                                if (_stage[lst] == "PRE")
                                    drConsumption["OrderPreCostingDirectMaterialId"] = dr["Id"];
                                else
                                    drConsumption["OrderProcurementCostingDirectMaterialId"] = dr["Id"];

                                drConsumption["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                dsConsumption.Tables[0].Rows.Add(drConsumption);
                            }

                            //calculate Consumption
                            dr["Consumption"] = CalculateConsumption(dtConsumptionReference.DefaultView.ToTable());
                        }


                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsConsumption);
                }

                CalculateFormula(OrderCostingMasterTemplateId);
                RecalculateValues(OrderCostingMasterTemplateId);
                RecalculateProcurementValues(OrderCostingMasterTemplateId);
                return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        private double CalculateConsumption(DataTable dt)
        {
            if (dt.Rows.Count == 0)
                return 0;

            int CentimeterFactor = 100;//to Meter
            double GSM = clsStaticInfo.dbl(dt.Rows[0]["GSMValue"].ToString());


            DataTable dtSummary = dt.AsEnumerable().GroupBy(x => new
            {
                AreaType = x["AreaType"],
                ComponentName = x["ComponentName"],
                ParameterName = x["ParameterName"],
                NoOfParts = x["NoOfParts"],
            })
                                        .Select(x =>
                                        {
                                            DataRow row = dt.NewRow();
                                            row["ComponentName"] = x.Key.ComponentName;
                                            row["ParameterName"] = x.Key.ParameterName;
                                            row["AreaType"] = x.Key.AreaType;
                                            row["NoOfParts"] = x.Key.NoOfParts;
                                            row["Total"] = x.Sum(r => (decimal)r["Total"]);
                                            return row;
                                        }
                                        ).CopyToDataTable();

            string ComponentName = "";
            double TotalArea = 0;
            for (int i = 0; i < dtSummary.Rows.Count; i++)
            {
                if (ComponentName == dtSummary.Rows[i]["ComponentName"].ToString())
                {
                    ComponentName = dtSummary.Rows[i]["ComponentName"].ToString();
                    continue;
                }

                double Parameter1 = clsStaticInfo.dbl(dtSummary.Compute("SUM(Total)", "ComponentName='" + dtSummary.Rows[i]["ComponentName"].ToString() + "' AND ParameterName='Height'").ToString());
                double Parameter2 = clsStaticInfo.dbl(dtSummary.Compute("SUM(Total)", "ComponentName='" + dtSummary.Rows[i]["ComponentName"].ToString() + "' AND ParameterName='Width'").ToString());
                double NoOfParts = clsStaticInfo.dbl(dtSummary.Rows[i]["NoOfParts"].ToString());

                Parameter1 = Parameter1 / CentimeterFactor;
                Parameter2 = Parameter2 / CentimeterFactor;

                if (dtSummary.Rows[i]["AreaType"].ToString().ToUpper() == "RECTANGULAR")
                    TotalArea += Parameter1 * Parameter2 * NoOfParts;
                else if (dtSummary.Rows[i]["AreaType"].ToString().ToUpper() == "TRIANGLE")
                    TotalArea += 0.5 * Parameter1 * Parameter2 * NoOfParts;
                else if (dtSummary.Rows[i]["AreaType"].ToString().ToUpper() == "CIRCLE")
                    TotalArea += Math.PI * Parameter1 * Parameter1 * NoOfParts;


                ComponentName = dtSummary.Rows[i]["ComponentName"].ToString();
            }

            return TotalArea * GSM;

        }
        private void CopyRow(DataRow drSource, DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {

                    drDestination[drSource.Table.Columns[COL].ColumnName] = bplib.clsWebLib.RetValidLen(drSource[drSource.Table.Columns[COL].ColumnName].ToString());

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }


        [HttpPost, Authorize]
        public ActionResult GetDirectProcessRateValue(string CostingItemId)
        {
            try
            {
                var dtMainItems = _sqlRepository.GetDataCollection(@"SELECT * FROM hkp.CostingItem AS ci WHERE ci.Id ='" + CostingItemId + "' ");

                return Json(dtMainItems, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetVersionByVersionId(string versionId)
        {
            string sql = @"select * from CostingVersionMasterTemplate where Id = '" + versionId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetVersion(string OrderCostingMasterTemplateId)
        {
            string sql = @"select  qcm.*  from OrderCostingMasterTemplate qcm 
                                where qcm.Id = '" + OrderCostingMasterTemplateId + "'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOrderCostingByMasterId(string VersionId)
        {
            try
            {

                string sql = @"select  A.* from (
                        select um.UserName UOM, qcd.*,ISNULL(ctc.CostingType,'0') as CostingType, csc.UserName,  csc.Code, csc.StandardName,csc.ShortName,  0 as Status from dbo.OrderCostingDetailTemplate qcd 
                        left join [HKP].[CostingComponent] csc ON csc.Id = qcd.CostingComponentId
                        left join CostingTypeComponent as ctc on ctc.CostingComponentId = csc.Id

						left join hkp.CostingItem ci on ci.CostingComponentId = csc.Id
						left join scs.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                        where ISNULL(OrderCostingMasterTemplateId,'null')='" + VersionId + @"'
                        union 
                        select um.UserName UOM, qcvd.*,ISNULL(ctc.CostingType,'0') as CostingType, csc.UserName,  csc.Code, csc.StandardName,csc.ShortName, 1 as Status  from dbo.CostingVersionDetailTemplate qcvd 
						left join [HKP].[CostingComponent] csc ON csc.Id = qcvd.CostingComponentId
						left join CostingTypeComponent as ctc on ctc.CostingComponentId = csc.Id

						left join hkp.CostingItem ci on ci.CostingComponentId = csc.Id
						left join scs.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                         where ISNULL(OrderCostingMasterTemplateId,'null')='" + VersionId + "') as A order by A.Sequence";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public DataTable GetOrderCostingByVersionMasterId(string VersionId)
        {
            try
            {
                string sql = @"select A.* from (select qcd.*, csc.UserName as CostingSubCategory, 0 as Status from dbo.OrderCostingDetailTemplate qcd left join HKP.CostingSubCategory csc ON csc.Id = qcd.CostingSubCategoryId
                        where CostingVersionMasterTemplateId='" + VersionId + @"'
                        union 
                        select qcvd.*, csc.UserName as CostingSubCategory, 1 as Status  from dbo.CostingVersionDetailTemplate qcvd left join hkp.CostingSubCategory csc ON csc.Id = qcvd.CostingSubCategoryId
                         where CostingVersionMasterTemplateId='" + VersionId + "') as A order by Sequence ";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetQuickCostinDetailByuickCostingVersionMasterId(string VersionId)
        {
            string sql = @"select * from OrderCostingDetailTemplate where CostingVersionMasterTemplateId = '" + VersionId + "'";
            try
            {
                var _data = _sqlRepository.GetDataCollection(sql);


                return Json(new { data = _data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        private DataSet GetCostingDetail(string OrderCostingMasterTemplateId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select qcvm.* from CostingVersionMasterTemplate qcvm
                            left join OrderCostingMasterTemplate qcm ON qcm.Id = qcvm.OrderCostingMasterTemplateId 
                            where OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet GetCostingVersionData(string versionId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select * from CostingVersionMasterTemplate where Id = '" + versionId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private string GetPK(string TableName)
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), TableName, out sID);
            return sID;
        }

        private void SaveData(string versionId, out string NewId, string versionDescription)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                NewId = string.Empty;
                DataSet dsDetail = null;
                string sql = "SELECT * FROM [dbo].[CostingVersionMasterTemplate] WHERE Id=''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                DataSet dsversion = GetCostingVersionData(versionId);

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    //bplib.clsGenID genid = new bplib.clsGenID();
                    //genid.GenID("dbo.OrderCostingVersionMaster", out _Id);
                    _Id = GetPK("CostingVersionMasterTemplate");
                    dr["Id"] = "VQCM" + _Id;

                    dr["OrderCostingMasterTemplateId"] = dsversion.Tables[0].Rows[0]["OrderCostingMasterTemplateId"].ToString();

                    dr["Version"] = Convert.ToDouble(dsversion.Tables[0].Rows[0]["Version"]) + 1;
                    dr["Description"] = versionDescription;

                    dr["AddedBy"] = dsversion.Tables[0].Rows[0]["AddedBy"];
                    dr["AddedDate"] = dsversion.Tables[0].Rows[0]["AddedDate"];
                    dr["AddedFromIP"] = dsversion.Tables[0].Rows[0]["AddedFromIP"];

                    //dr["UpdatedBy"] = dsversion.Tables[0].Rows[0]["UpdatedBy"];
                    //dr["UpdatedDate"] = dsversion.Tables[0].Rows[0]["UpdatedDate"];
                    //dr["UpdatedFromIP"] = dsversion.Tables[0].Rows[0]["UpdatedFromIP"];

                    dsMaster.Tables[0].Rows.Add(dr);
                }


                NewId = dsMaster.Tables[0].Rows[0]["Id"].ToString();


                SaveDetailData(versionId, NewId, out dsDetail);

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveDetailData(string versionId, string NewId, out DataSet dsDetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            GetCostingDeatilByVersionId(versionId, out DataSet detailData);
            dsDetail = detailData;
            if (dsDetail.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsDetail.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = dsDetail.Tables[0].Rows[i];

                    dr.BeginEdit();

                    dr["CostingVersionMasterTemplateId"] = NewId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
            }
        }


        #region CostingVersionDetailTemplate

        public void GetCostingDeatilByVersionId(string versionId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM OrderCostingDetailTemplate where CostingVersionMasterTemplateId = '" + versionId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function


        [HttpPost, Authorize]
        public ActionResult CreateCostingVersionDetail(string versionId, List<OrderCostingDetailTemplate> data, string versionDescription)
        {
            string NewId = string.Empty;
            GetCostingDeatilByVersionId(versionId, out DataSet dsCostingDetail);

            SaveCostingVersionCopyDetail(versionId, dsCostingDetail);
            SaveData(versionId, out NewId, versionDescription);
            string newVersionId = NewId;
            //SaveCostingDetail(newVersionId, data);
            return Json(new { Message = AplosMessage.Insert });
        }

        private void SaveCostingVersionCopyDetail(string versionId, DataSet dsCostingDetail)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "select * from CostingVersionDetailTemplate where CostingVersionMasterTemplateId = '" + versionId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsCostingDetail.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsCostingDetail.Tables[0].Rows.Count; i++)
                    {
                        string _Id = "";
                        //bplib.clsGenID genid = new bplib.clsGenID();
                        //genid.GenID("dbo.CostingVersionDetailTemplate", out _Id);
                        _Id = GetPK("CostingVersionDetailTemplate");

                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = "VD" + _Id;
                        //dr["CostingTypeComponentId"] = dsCostingDetail.Tables[0].Rows[i]["CostingTypeComponentId"];
                        dr["CostingComponentId"] = dsCostingDetail.Tables[0].Rows[i]["CostingComponentId"];

                        dr["CostingVersionMasterTemplateId"] = dsCostingDetail.Tables[0].Rows[i]["CostingVersionMasterTemplateId"];
                        dr["Sequence"] = dsCostingDetail.Tables[0].Rows[i]["Sequence"];
                        dr["CostingValue"] = dsCostingDetail.Tables[0].Rows[i]["CostingValue"];
                        dr["BuyerTarget"] = dsCostingDetail.Tables[0].Rows[i]["BuyerTarget"];

                        dr["AddedBy"] = dsCostingDetail.Tables[0].Rows[i]["AddedBy"];
                        dr["AddedDate"] = dsCostingDetail.Tables[0].Rows[i]["AddedDate"];
                        dr["AddedFromIP"] = dsCostingDetail.Tables[0].Rows[i]["AddedFromIP"];

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion CostingVersionDetailTemplate

        #region OrderCostingDetailTemplate


        public void GetOrderCosting(string CostingVersionMasterTemplateId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM [dbo].[OrderCostingDetailTemplate] WHERE OrderCostingMasterTemplateId= '" + CostingVersionMasterTemplateId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        //End of function
        private void SaveCostingDetail(string masterid, List<OrderCostingDetailTemplate> data, out DataSet dsdetail)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GetOrderCosting(masterid, out dsdetail);
            try
            {
                if (data == null)
                    return;

                for (int i = 0; i < dsdetail.Tables[0].Rows.Count; i++)
                {
                    string ownid = dsdetail.Tables[0].Rows[i]["Id"].ToString();
                    List<OrderCostingDetailTemplate> FilterData = data.Where(a => a.Id == ownid).ToList();
                    if (FilterData.Count == 0)
                        dsdetail.Tables[0].Rows[i].Delete();
                }


                if (data != null)
                {

                    DataView dv = null;


                    dv = new DataView(dsdetail.Tables[0]);

                    string _Id = string.Empty;

                    _Id = GetPK("OrderCostingDetailTemplate");

                    int count = 0;
                    foreach (var item in data)
                    {
                        dv.RowFilter = "Id='" + item.Id + "'";
                        if (dv.Count == 0)
                        {
                            count++;


                            DataRow dr = dsdetail.Tables[0].NewRow();
                            dr["Id"] = "QD" + _Id + "_" + count;

                            //dr["CostingTypeComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingTypeComponentId));
                            dr["CostingComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingComponentId));
                            dr["OrderCostingMasterTemplateId"] = masterid;
                            dr["Sequence"] = clsStaticInfo.dbl(clsStaticInfo.nullrecorder(item.Sequence));
                            dr["CostingValue"] = item.CostingValue;
                            dr["BuyerTarget"] = item.BuyerTarget;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsdetail.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dv[0].Row;

                            dr.BeginEdit();
                            //dr["CostingTypeComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingTypeComponentId));
                            dr["CostingComponentId"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.nullrecorder(item.CostingComponentId));
                            dr["OrderCostingMasterTemplateId"] = masterid;
                            dr["Sequence"] = clsStaticInfo.dbl(clsStaticInfo.nullrecorder(item.Sequence));
                            dr["CostingValue"] = Convert.ToDouble(item.CostingValue);
                            dr["BuyerTarget"] = item.BuyerTarget;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr["UpdatedDate"] = DateTime.Now;

                            dr.EndEdit();
                        }
                    }
                    //clsStaticInfo obj = new clsStaticInfo();
                    // obj.SaveDataSets(dsdetail);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public ActionResult CreateCostingBuyer(OrderCostingBuyer data)
        {
            DataSet dsCostingBuyer;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            //con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[CostingBuyer] where Id= '" + data.Id.ToString()+ "'", out dsCostingBuyer, false, "1");
            con.OpenDataSetThroughAdapter("select * from [dbo].[CostingBuyer] where Id='" + data.Id + "'", out dsCostingBuyer, false, "1");

            string _Id = "";

            #region data update
            if (dsCostingBuyer.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("dbo.OrderCostingDetail", out _Id);

                data.Id = "CB" + _Id;
                AddNewCostingBuyerRow(dsCostingBuyer.Tables[0], data);
            }
            else
            {
                _Id = data.Id.ToString();
                EditNewCostingBuyerRow(dsCostingBuyer.Tables[0].Rows[0], data);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsCostingBuyer);


            return Json(new { data = Helpers.CustomJsonResult.DataTableToJson(dsCostingBuyer.Tables[0]), Message = AplosMessage.Insert });
        }
        #endregion OrderCostingDetailTemplate


        [HttpGet, Authorize]
        public ActionResult GetCostingSubCategory()
        {
            string sql = @"select 0 as flag,c.* from hkp.CostingComponent c where c.Active = 1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetCostingComponents()
        //{
        //    string sql = @"select ci.CostingComponentId,ci.Code,ci.StandardName from hkp.CostingItem ci
        //            left JOIN  hkp.CostingComponent cc ON ci.CostingComponentId=cc.Id";
        //    return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        //}



        public void SaveCostingItemsForCostingComponent(DataSet dsTemplateMaster, out DataSet dsMaster)
        {
            try
            {

                DataSet dsItem;

                ConnectionManager.DAL.ConManager objCon;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _id = string.Empty;



                string sql = @"Select * from OrderPreCostingOperation where CostingItemId in (SELECT I.Id FROM hkp.CostingItem I 
                                        INNER JOIN  hkp.CostingComponent AS cc  ON cc.Id=i.CostingComponentId
                                        WHERE cc.Code='OPN') AND OrderCostingMasterTemplateId='" + dsTemplateMaster.Tables[0].Rows[0]["Id"].ToString() + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                sql = @"SELECT I.* FROM hkp.CostingItem I 
                        INNER JOIN  hkp.CostingComponent AS cc  ON cc.Id=i.CostingComponentId
                        WHERE cc.Code='OPN' ORDER BY cc.Code,cc.Sequence";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsItem, false, "1");


                string ProductConfigSql = @"SELECT * FROM [TRN].[ProductMasterEfficency] EFF WHERE 
                                    eff.ProductMasterId='" + dsTemplateMaster.Tables[0].Rows[0]["ProductMasterId"].ToString() + @"' AND EfficencyName='Costing'  ";

                DataTable dtTempProductConfig = _sqlRepository.GetDataTable(ProductConfigSql);

                int Index = 0;
                double CMValue = 0;
                foreach (DataRow item in dsItem.Tables[0].Rows)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item["Id"].ToString() + "'";


                    Index++;
                    DataRow dr;
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        if (_id == "")
                        {
                            _id = "" + GetPK("OrderCosting");
                        }

                        dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = _id + Index;
                        dr["CostingItemId"] = item["Id"];
                        dr["OrderCostingMasterTemplateId"] = dsTemplateMaster.Tables[0].Rows[0]["Id"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr["Value"] = "0";
                        dsMaster.Tables[0].Rows.Add(dr);


                        if (item["Code"].ToString().ToUpper() == "CM")
                        {
                            double additionalCost = 0;
                            double StandardWorkingHours = clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHours"].ToString());
                            double StandardWorkingHoursForProduct = clsStaticInfo.dbl(dtTempProductConfig.Rows[0]["StandardWorkingHours"].ToString());


                            if (StandardWorkingHours > StandardWorkingHoursForProduct)
                            {
                                additionalCost = (StandardWorkingHours - StandardWorkingHoursForProduct) * clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["AdditionalWorkingHourCostPerHour"].ToString());
                            }

                            CMValue = ((clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHourCost"].ToString()) + additionalCost) /
                               clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHours"].ToString())) /
                               clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["MKTTargetPerHour"].ToString());
                            dr["Value"] = CMValue;
                        }
                        else if (item["Code"].ToString().ToUpper() == "UPC")
                        {
                            double _workdays = clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["OrderSize"].ToString()) /
                                (clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHours"].ToString()) *
                                 clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["MKTTargetPerHour"].ToString()));

                            int WorkDysRequired = Convert.ToInt32(Math.Ceiling(_workdays));

                            DataTable UpChargeMatrix = _sqlRepository.GetDataTable("SELECT TOP 1 * FROM hkp.CostingUpchargeMatrix AS cum WHERE cum.WorkCenterDays=" + WorkDysRequired.ToString());
                            if (UpChargeMatrix.Rows.Count == 0)
                                UpChargeMatrix = _sqlRepository.GetDataTable("SELECT TOP 1 * FROM hkp.CostingUpchargeMatrix AS cum WHERE cum.WorkCenterDays<=" + WorkDysRequired.ToString() + " ORDER BY WorkCenterDays desc");

                            if (UpChargeMatrix.Rows.Count > 0)
                            {
                                dr["Value"] = CMValue * clsStaticInfo.dbl(UpChargeMatrix.Rows[0][dsTemplateMaster.Tables[0].Rows[0]["CriticalLevel"].ToString()].ToString()) / 100;
                            }

                        }


                    }
                    else
                    {
                        dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["CostingItemId"] = item["Id"];
                        dr["OrderCostingMasterTemplateId"] = dsTemplateMaster.Tables[0].Rows[0]["Id"].ToString();

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        //dr["Value"] = "0";

                        dr.EndEdit();
                    }



                  
                }


            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public void SaveProcurementCostingItemsForCostingComponent(DataSet dsTemplateMaster, out DataSet dsMaster)
        {
            try
            {

                DataSet dsItem;

                ConnectionManager.DAL.ConManager objCon;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _id = string.Empty;



                string sql = @"Select * from OrderProcurementCostingOperation where CostingItemId in (SELECT I.Id FROM hkp.CostingItem I 
                                        INNER JOIN  hkp.CostingComponent AS cc  ON cc.Id=i.CostingComponentId
                                        WHERE cc.Code='OPN') AND OrderCostingMasterTemplateId='" + dsTemplateMaster.Tables[0].Rows[0]["Id"].ToString() + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                sql = @"SELECT I.* FROM hkp.CostingItem I 
                        INNER JOIN  hkp.CostingComponent AS cc  ON cc.Id=i.CostingComponentId
                        WHERE cc.Code='OPN' ORDER BY cc.Code,cc.Sequence";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsItem, false, "1");


                string ProductConfigSql = @"SELECT * FROM [TRN].[ProductMasterEfficency] EFF WHERE 
                                    eff.ProductMasterId='" + dsTemplateMaster.Tables[0].Rows[0]["ProductMasterId"].ToString() + @"' AND EfficencyName='Costing'  ";

                DataTable dtTempProductConfig = _sqlRepository.GetDataTable(ProductConfigSql);

                int Index = 0;
                double CMValue = 0;
                foreach (DataRow item in dsItem.Tables[0].Rows)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item["Id"].ToString() + "'";


                    Index++;
                    DataRow dr;
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        if (_id == "")
                        {
                            _id = "" + GetPK("OrderCosting");
                        }

                        dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = _id + Index;
                        dr["CostingItemId"] = item["Id"];
                        dr["OrderCostingMasterTemplateId"] = dsTemplateMaster.Tables[0].Rows[0]["Id"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr["Value"] = "0";
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["CostingItemId"] = item["Id"];
                        dr["OrderCostingMasterTemplateId"] = dsTemplateMaster.Tables[0].Rows[0]["Id"].ToString();

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr["Value"] = "0";

                        dr.EndEdit();
                    }



                    if (item["Code"].ToString().ToUpper() == "CM")
                    {
                        double additionalCost = 0;
                        double StandardWorkingHours = clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHours"].ToString());
                        double StandardWorkingHoursForProduct = clsStaticInfo.dbl(dtTempProductConfig.Rows[0]["StandardWorkingHours"].ToString());


                        if (StandardWorkingHours > StandardWorkingHoursForProduct)
                        {
                            additionalCost = (StandardWorkingHours - StandardWorkingHoursForProduct) * clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["AdditionalWorkingHourCostPerHour"].ToString());
                        }

                        CMValue = ((clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHourCost"].ToString()) + additionalCost) /
                           clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHours"].ToString())) /
                           clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["MKTTargetPerHour"].ToString());
                        dr["Value"] = CMValue;
                    }
                    else if (item["Code"].ToString().ToUpper() == "UPC")
                    {
                        double _workdays = clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["OrderSize"].ToString()) /
                            (clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["StandardWorkingHours"].ToString()) *
                             clsStaticInfo.dbl(dsTemplateMaster.Tables[0].Rows[0]["MKTTargetPerHour"].ToString()));

                        int WorkDysRequired = Convert.ToInt32(Math.Ceiling(_workdays));

                        DataTable UpChargeMatrix = _sqlRepository.GetDataTable("SELECT TOP 1 * FROM hkp.CostingUpchargeMatrix AS cum WHERE cum.WorkCenterDays=" + WorkDysRequired.ToString());
                        if (UpChargeMatrix.Rows.Count == 0)
                            UpChargeMatrix = _sqlRepository.GetDataTable("SELECT TOP 1 * FROM hkp.CostingUpchargeMatrix AS cum WHERE cum.WorkCenterDays<=" + WorkDysRequired.ToString() + " ORDER BY WorkCenterDays desc");

                        if (UpChargeMatrix.Rows.Count > 0)
                        {
                            dr["Value"] = CMValue * clsStaticInfo.dbl(UpChargeMatrix.Rows[0][dsTemplateMaster.Tables[0].Rows[0]["CriticalLevel"].ToString()].ToString()) / 100;
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        [HttpPost]
        public JsonResult Create(FormCollection form, List<Dictionary<string, object>> SalesOrderData)
        {
            try
            {


                var pre = form["modelNew"];
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var cost = JsonConvert.DeserializeObject<OrderCostingMasterTemplate>(pre, settings);
                if (string.IsNullOrEmpty(cost.FileName) == false)
                    if (cost.FileName.Length > 50)
                        throw new Exception("File name should be less than 50 characters");

                DataSet dsMaster;
                DataSet dsItems = new DataSet();
                DataSet dsItemsProcurement = new DataSet();
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + cost.Code + "' AND  Id<>'" + cost.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Code is already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + cost.UserName + "'  AND  Id<>'" + cost.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("UserName is already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + cost.Id + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    //bplib.clsGenID genid = new bplib.clsGenID();
                    //genid.GenID(TableName, out _Id);
                    _Id = GetPK(TableName);

                    cost.Id = _Id;
                    _Id = cost.Id;
                    AddNewRow(dsMaster.Tables[0], cost);


                    //SaveCostingItemsForCostingComponent(dsMaster, out dsItems);
                }
                else
                {
                    _Id = cost.Id.ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], cost);

                    //SaveCostingItemsForCostingComponent(dsMaster, out dsItems);
                }

                DataTable dtProductParams = _sqlRepository.GetDataTable(ProductParameters(dsMaster.Tables[0].Rows[0]["ProductMasterId"].ToString()));
                if (dtProductParams.Rows.Count > 0)
                {
                    dsMaster.Tables[0].Rows[0]["StandardWorkingHourCost"] = dtProductParams.Rows[0]["StandardWorkingHourCost"].ToString();
                    dsMaster.Tables[0].Rows[0]["AdditionalWorkingHourCostPerHour"] = dtProductParams.Rows[0]["AdditionalWorkingHourCostPerHour"].ToString();
                }
                SaveCostingItemsForCostingComponent(dsMaster, out dsItems);
                SaveProcurementCostingItemsForCostingComponent(dsMaster, out dsItemsProcurement);
                #endregion data update

                DataSet dsCostingDetail = null;

                var _OrderCostingData = form["OrderCostingData"];
                var OrderCostingData = JsonConvert.DeserializeObject<List<OrderCostingDetailTemplate>>(_OrderCostingData, settings);
                SaveCostingDetail(_Id, OrderCostingData, out dsCostingDetail);




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsCostingDetail, dsItems, dsItemsProcurement);

                try
                {
                    SalesOrderData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(form["SalesOrderData"], settings);
                    UpdateSalesOrders(_Id, SalesOrderData);
                }
                catch (Exception ex)
                {
                }


                var file = Request.Files["file"];
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".jpg" || extension.ToLower() == ".png")
                    {
                        cost.FileName = extension;
                        if (!string.IsNullOrEmpty(cost.FileName))
                            cost.FileName = cost.Id.ToString() + cost.FileName;
                    }
                    else
                        throw new CustomException(Resources.ImageUploadError);
                }

                if (file != null)
                {
                    var path = Path.Combine(ResourcesPathReader.GetCostingPicPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeImage, typeof(string)).ToString())*/, cost.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }


                return Json(new { Error = false, data = cost, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public Dictionary<string, object> GetDocumentFile(string OrderCostingMasterTemplateId)
        {
            try
            {
                var sql = @"Select Id, FileName From " + TableName + "  Where Id='" + OrderCostingMasterTemplateId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
                //throw new CustomException(ex.Message, ex,
                // Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public ActionResult DeleteCostingDetail(string costingDetailId)
        {
            try
            {
                if (string.IsNullOrEmpty(costingDetailId))
                    throw new Exception("costingDetailId Not Found");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderCostingDetailTemplate where id='" + costingDetailId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Delete(string id)
        {
            //string sql = @"select * from [HKP].[CostingGroupGL] where CostingGroupId = '"+ id + "'";
            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery("delete from [OrderCostingDetailTemplate] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingDirectProcess] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingOperation] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingValueLoss] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingSalesExpense] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingDirectMaterialConsumption] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingDirectMaterial] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderPreCostingProfit] where OrderCostingMasterTemplateId='" + id + "'");

                con.executeQuery("delete from [OrderProcurementCostingDirectProcess] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderProcurementCostingOperation] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderProcurementCostingValueLoss] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderProcurementCostingSalesExpense] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderProcurementCostingDirectMaterialConsumption] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderProcurementCostingDirectMaterial] where OrderCostingMasterTemplateId='" + id + "'");
                con.executeQuery("delete from [OrderProcurementCostingProfit] where OrderCostingMasterTemplateId='" + id + "'");

                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
        private void AddNewRow(DataTable dt, OrderCostingMasterTemplate sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            //foreach (var item in sourceData)
            //{
            //    try
            //    {
            //        dr[item] = sourceData[item];
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
            dr["Id"] = sourceData.Id;
            dr["Code"] = sourceData.Code;
            dr["Active"] = sourceData.Active;
            dr["CustomerId"] = sourceData.CustomerId;
            dr["Description"] = sourceData.Description;
            dr["FileName"] = sourceData.FileName;
            dr["MKTTargetPerHour"] = sourceData.MKTTargetPerHour;
            dr["OrderSize"] = sourceData.OrderSize;
            dr["PackingType"] = sourceData.PackingType;
            dr["PaymentDays"] = sourceData.PaymentDays;
            dr["ProductionAvailableDays"] = sourceData.ProductionAvailableDays;
            dr["ProductMasterId"] = sourceData.ProductMasterId;
            dr["Remarks"] = sourceData.Remarks;
            //     dr["Sequence"] = sourceData.Sequence;
            dr["ShortName"] = sourceData.ShortName;
            dr["SpecifyTo"] = sourceData.SpecifyTo;
            dr["StandardName"] = sourceData.StandardName;
            dr["TargetSellingPrice"] = sourceData.TargetSellingPrice;
            dr["UserName"] = sourceData.UserName;
            dr["EstNoOfPackingList"] = sourceData.EstNoOfPackingList;
            dr["ExcessShipmentPer"] = sourceData.ExcessShipmentPer;
            dr["CurrencyId"] = sourceData.CurrencyId;

            dr["CostingStage"] = sourceData.CostingStage;
            dr["MasterOrderItemId"] = sourceData.MasterOrderItemId;
            dr["InquiryItemId"] = sourceData.InquiryItemId;

            dr["UOM"] = sourceData.UOM;
            dr["TargetOrSPT"] = sourceData.TargetOrSPT;
            dr["CriticalLevel"] = sourceData.CriticalLevel;


            dr["SPT"] = sourceData.SPT;
            dr["NoOfWorkstation"] = sourceData.NoOfWorkstation;
            dr["EfficiencyPercentage"] = sourceData.EfficiencyPercentage;
            dr["StandardWorkingHours"] = sourceData.StandardWorkingHours;
            dr["WorkCenterTargetPerDay"] = sourceData.WorkCenterTargetPerDay;
            dr["StandardWorkingHourCost"] = sourceData.StandardWorkingHourCost;
            dr["AdditionalWorkingHourCostPerHour"] = sourceData.AdditionalWorkingHourCostPerHour;
            dr["isDirectApproval"] = sourceData.isDirectApproval;

            dr["QuickCostingCheckStatus"] = DBNull.Value;
            dr["PreCostingCheckStatus"] = DBNull.Value;
            dr["ProcurementCostingCheckStatus"] = DBNull.Value;
            dr["QuickCostingCheckRemarks"] = DBNull.Value;
            dr["PreCostingCheckRemarks"] = DBNull.Value;
            dr["ProcurementCostingCheckRemarks"] = DBNull.Value;
            dr["QuickCostingApprovalStatus"] = DBNull.Value;
            dr["PreCostingApprovalStatus"] = DBNull.Value;
            dr["ProcurementCostingApprovalStatus"] = DBNull.Value;
            dr["QuickCostingApprovalRemarks"] = DBNull.Value;
            dr["PreCostingApprovalRemarks"] = DBNull.Value;
            dr["ProcurementCostingApprovalRemarks"] = DBNull.Value;


            dr["TargetCM"] = sourceData.TargetCM;
            dr["TargetProfit"] = sourceData.TargetProfit;
            dr["IsPercentage"] = sourceData.IsPercentage;

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }


        private void EditRow(DataRow dr, OrderCostingMasterTemplate sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            //foreach (var item in sourceData)
            //{
            //    try
            //    {
            //        dr[item] = sourceData[item];
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
            dr["Id"] = sourceData.Id;
            dr["Code"] = sourceData.Code;
            dr["Active"] = sourceData.Active;
            dr["CustomerId"] = sourceData.CustomerId;
            dr["Description"] = sourceData.Description;
            dr["FileName"] = sourceData.FileName;
            dr["MKTTargetPerHour"] = sourceData.MKTTargetPerHour;
            dr["OrderSize"] = sourceData.OrderSize;
            dr["PackingType"] = sourceData.PackingType;
            dr["PaymentDays"] = sourceData.PaymentDays;
            dr["ProductionAvailableDays"] = sourceData.ProductionAvailableDays;
            dr["ProductMasterId"] = sourceData.ProductMasterId;
            dr["Remarks"] = sourceData.Remarks;
            //  dr["Sequence"] = sourceData.Sequence;
            dr["ShortName"] = sourceData.ShortName;
            dr["SpecifyTo"] = sourceData.SpecifyTo;
            dr["StandardName"] = sourceData.StandardName;
            dr["TargetSellingPrice"] = sourceData.TargetSellingPrice;
            dr["UserName"] = sourceData.UserName;
            dr["EstNoOfPackingList"] = sourceData.EstNoOfPackingList;
            dr["ExcessShipmentPer"] = sourceData.ExcessShipmentPer;
            dr["CurrencyId"] = sourceData.CurrencyId;
            dr["CostingStage"] = sourceData.CostingStage;
            dr["isDirectApproval"] = sourceData.isDirectApproval;

            dr["SPT"] = sourceData.SPT;
            dr["NoOfWorkstation"] = sourceData.NoOfWorkstation;
            dr["EfficiencyPercentage"] = sourceData.EfficiencyPercentage;
            dr["StandardWorkingHours"] = sourceData.StandardWorkingHours;
            dr["WorkCenterTargetPerDay"] = sourceData.WorkCenterTargetPerDay;
            dr["StandardWorkingHourCost"] = sourceData.StandardWorkingHourCost;
            dr["AdditionalWorkingHourCostPerHour"] = sourceData.AdditionalWorkingHourCostPerHour;

            dr["MasterOrderItemId"] = sourceData.MasterOrderItemId;
            dr["InquiryItemId"] = sourceData.InquiryItemId;


            dr["UOM"] = sourceData.UOM;
            dr["TargetOrSPT"] = sourceData.TargetOrSPT;
            dr["CriticalLevel"] = sourceData.CriticalLevel;

            dr["TargetCM"] = sourceData.TargetCM;
            dr["TargetProfit"] = sourceData.TargetProfit;
            dr["IsPercentage"] = sourceData.IsPercentage;
            //dr["IsApprovalApplicable"] = sourceData.IsApprovalApplicable;
            //dr["ApproveByWhomId"] = sourceData.ApproveByWhomId;

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        [Authorize]
        public ActionResult GetProductByProductMasterId(string ProductMasterId)
        {
            string sql = @"select pm.*, pc.UserName as ProductCategory,psc.UserName as ProductSubCategory ,
                                NoOfWorkstation,	EfficencyPercentage AS EfficiencyPercentage,
                                StandardWorkingHours,SPT
							
							from [MST].[ProductMaster] pm 
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT OUTER JOIN [TRN].[ProductMasterEfficency] EF ON ef.ProductMasterId=pm.Id AND ef.EfficencyName='Costing'
							where pm.Id = '" + ProductMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private string ProductParameters(string ProductMasterId)
        {
            return @"select  pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory
                             ,pm.CostingType ,ct.UserName AS CostingTypeName,
                                NoOfWorkstation,	EfficencyPercentage AS EfficiencyPercentage,StandardWorkingHourCost,AdditionalWorkingHourCostPerHour,
                                StandardWorkingHours,SPT
							from  [MST].[ProductMaster] pm 
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
                            LEFT OUTER JOIN [TRN].[ProductMasterEfficency] EF ON ef.ProductMasterId=pm.Id AND ef.EfficencyName='Costing'
                            LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
							where pm.Id = '" + ProductMasterId + "'";
        }
        [HttpGet, Authorize]
        public ActionResult ProductMasterDetail(string ProductMasterId)
        {
            string sql = ProductParameters(ProductMasterId);

            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);
            Dictionary<string, string> CostingType = new Dictionary<string, string>();
            foreach (var item in Enum.GetValues(typeof(CostingType)))
            {
                CostingType.Add(item.ToString(), AccessInfo.GetEnumDescription((CostingType)(int)item));
            }

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    data[i]["CostingType"] = CostingType[data[i]["CostingType"].ToString()];
                }
                catch (Exception)
                {


                }
            }

            return Json(new { Product = data }, JsonRequestBehavior.AllowGet);


            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingComponentByProductMasterId(string ProductMasterId)
        {

            string sql = @"SELECT 
                        cc.Id as CostingComponentId
                     ,cc.Code
                     ,cc.ShortName
                     ,cc.UserName
                        ,ctc.Sequence
                     ,cc.StandardName
                     ,ctc.CostingType
                     ,cc.CostingSegment
                    ,ProcurementCostingSavingsPercentage, PreCostingSavingsPercentage
                    FROM [dbo].[CostingTypeComponent] AS ctc
                    inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                    WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + "') order by ctc.Sequence";

            //string itemSql = @"select * from hkp.CostingItemselect * from hkp.CostingItem";
            // return Json(new { data = _sqlRepository.GetDataCollection(sql, null), items = _sqlRepository.GetDataCollection(itemSql,null) }, JsonRequestBehavior.AllowGet);
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOrderCostingDetailByProductMaster(string ProductMasterId, string CostingVersionMasterTemplateId)
        {
            string sql = "";

            sql = @" select isnull(d.id,'New') isNewId, case when isnull(d.Id,'')<>'' THEN isnull(TEMPLATE.CostingComponentId,'DELETE') ELSE '' END AS isToBeDeleted,
                         d.Id
                        ,0 as Status,CC.CalculationMethod
	                    ,d.CostingValue
	                    ,d.BuyerTarget
	                    --,d.CostingVersionMasterTemplateId
                        ,cc.Id as CostingComponentId
	                    ,cc.Code
	                    ,cc.ShortName
	                    ,cc.UserName
                        ,ctc.Sequence
	                    ,cc.StandardName
	                    ,ctc.CostingType
                        ,cc.CostingSegment
                        ,isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount 
                        ,isnull(itemvalp.TotalGrossAmount,0) AS TotalProcurementGrossAmount 
                        ,CC.ProcurementCostingSavingsPercentage, CC.PreCostingSavingsPercentage
						 from hkp.CostingComponent CC
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')
                        left outer join OrderCostingDetailTemplate D on cc.id=d.CostingComponentId and d.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'
                         LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderPreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + CostingVersionMasterTemplateId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM OrderPreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                  )AS ITEMVAL ON  itemval.CostingComponentId=d.CostingComponentId
                        LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + CostingVersionMasterTemplateId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM OrderProcurementCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	GROUP BY i.CostingComponentId
                                  )AS ITEMVALP ON  ITEMVALP.CostingComponentId=d.CostingComponentId
                        left outer join  (
                        select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')) AS TEMPLATE 
					    on template.CostingComponentId=d.CostingComponentId


                        where   cc.Id IN (
                            select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')

					    UNION

					    select CostingComponentId from OrderCostingDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')='" + CostingVersionMasterTemplateId + @"'

					--union

					--select CostingComponentId from CostingVersionDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')= '" + CostingVersionMasterTemplateId + @"'
                    )  order by isnull(ctc.Sequence,999999),cc.Description";

            string sqlAllItem = @"  SELECT  ci.Id,CI.Code,CC.CalculationMethod, ctc.Sequence AS ComponentSequence,ci.Sequence AS ItemSequnce, ci.CostingCategoryId, ci.CostingComponentId,cc.CostingSegment,upper(isnull(itemval.ValueType,'FIXED')) AS ValueType,
                        isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount,isnull(itemval.Value,0) AS Value,isnull(itemval.Rate,0) AS Rate,
                        isnull(itemvalp.TotalGrossAmount,0) AS TotalProcurementGrossAmount,isnull(itemvalp.Value,0) AS ProcurementValue,isnull(itemvalp.Rate,0) AS ProcurementRate,
						upper(isnull(itemval.ValueType,'FIXED')) AS ProcurementValueType,CC.ProcurementCostingSavingsPercentage, CC.PreCostingSavingsPercentage
						 from hkp.CostingComponent CC
						 INNER JOIN hkp.CostingItem AS ci ON ci.CostingComponentId=cc.Id
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')
                        left outer join OrderCostingDetailTemplate D on cc.id=d.CostingComponentId and d.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'
                         LEFT OUTER JOIN (        SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.GrossAmount AS TotalGrossAmount FROM OrderPreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + CostingVersionMasterTemplateId + @"' 
                                            UNION ALL SELECT 'PERCENTAGE' AS ValueType, PC.Value,PC.Rate, i.Id,pc.Amount AS TotalGrossAmount FROM OrderPreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.[Value]  AS TotalGrossAmount FROM OrderPreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,          pc.Amount AS TotalGrossAmount FROM OrderPreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderPreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderPreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                  )AS ITEMVAL ON  itemval.Id=ci.Id
                        LEFT OUTER JOIN (        SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.GrossAmount AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + CostingVersionMasterTemplateId + @"' 
                                            UNION ALL SELECT 'PERCENTAGE' AS ValueType, PC.Value,PC.Rate, i.Id,pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.[Value]  AS TotalGrossAmount FROM OrderProcurementCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,          pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + CostingVersionMasterTemplateId + @"'	
                                  )AS ITEMVALP ON  itemvalp.Id=ci.Id
                        left outer join  (
                        select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')) AS TEMPLATE 
					    on template.CostingComponentId=d.CostingComponentId
                        where   cc.Id IN (
                            select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')

					    UNION

					    select CostingComponentId from OrderCostingDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')='" + CostingVersionMasterTemplateId + @"'

                    ) order by ctc.Sequence,ci.Sequence --order by isnull(ctc.Sequence,999999),cc.Description";

            string sqlUpChargeMatrix = "SELECT * FROM hkp.CostingUpchargeMatrix AS cum WHERE cum.CostingType=(SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"') ORDER BY WorkCenterDays desc";
            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            return Json(new { UpChargeMatrix = _sqlRepository.GetDataCollection(sqlUpChargeMatrix), ComponentList = _sqlRepository.GetDataCollection(sql, null), ItemList = _sqlRepository.GetDataCollection(sqlAllItem, null) }, JsonRequestBehavior.AllowGet);
        }

        private string GetChargesPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(CostingItem), out sID);
            return sID;
        }

        private void AddNewOrderCostingDetailRow(DataTable dt, Dictionary<string, object> sourceData)
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
        private void EditNewOrderCostingDetailRow(DataRow dr, Dictionary<string, object> sourceData)
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


        private void AddNewCostingBuyerRow(DataTable dt, OrderCostingBuyer sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();


            dr["Id"] = sourceData.Id;
            dr["OrderCostingMasterTemplateId"] = sourceData.OrderCostingMasterTemplateId;
            dr["BuyerId"] = sourceData.BuyerId;
            dr["BuyerStyleRefNo"] = sourceData.BuyerStyleRefNo;
            dr["OwnStyleRefNo"] = sourceData.OwnStyleRefNo;

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditNewCostingBuyerRow(DataRow dr, OrderCostingBuyer sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            dr["Id"] = sourceData.Id;
            dr["OrderCostingMasterTemplateId"] = sourceData.OrderCostingMasterTemplateId;
            dr["BuyerId"] = sourceData.BuyerId;
            dr["BuyerStyleRefNo"] = sourceData.BuyerStyleRefNo;
            dr["OwnStyleRefNo"] = sourceData.OwnStyleRefNo;

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        private void SaveCostingItems(IEnumerable<CostingItem> data, out DataSet dsMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [HKP].[CostingItem] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = GetChargesPK();

                            dr["Sequence"] = item.Sequence;
                            dr["Code"] = item.Code;
                            dr["ShortName"] = item.ShortName;
                            dr["StandardName"] = item.StandardName;
                            dr["UserName"] = item.UserName;
                            dr["Description"] = item.Description;
                            dr["Remarks"] = item.Remarks;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["CostingCategoryId"] = item.CostingCategoryId;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["UnitOfMeasurementId"] = item.UnitOfMeasurementId;
                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["Wastage"] = item.Wastage;
                            dr["ProcessId"] = item.ProcessId;
                            dr["BudgetMasterId"] = item.BudgetMasterId;
                            dr["ActivityId"] = item.ActivityId;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;
                            //dr["CostingGroupId"] = item.CostingGroupId;
                            //dr["CostingItemType"] = item.CostingItemType;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["Id"] = item.Id;
                            dr["Sequence"] = item.Sequence;
                            dr["Code"] = item.Code;
                            dr["ShortName"] = item.ShortName;
                            dr["StandardName"] = item.StandardName;
                            dr["UserName"] = item.UserName;
                            dr["Description"] = item.Description;
                            dr["Remarks"] = item.Remarks;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["CostingCategoryId"] = item.CostingCategoryId;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["CostingComponentId"] = item.CostingComponentId;
                            dr["UnitOfMeasurementId"] = item.UnitOfMeasurementId;
                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["Wastage"] = item.Wastage;
                            dr["ProcessId"] = item.ProcessId;
                            dr["BudgetMasterId"] = item.BudgetMasterId;
                            dr["ActivityId"] = item.ActivityId;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;
                            //dr["CostingGroupId"] = item.CostingGroupId;
                            //dr["CostingItemType"] = item.CostingItemType;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void SaveOrderCostingDetail(Dictionary<string, object> OrderCostingDetail, out DataSet dsCostingDetail)
        {

            dsCostingDetail = null;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[OrderCostingDetail] where Id= '" + OrderCostingDetail["Id"] + "'", out dsCostingDetail, false, "1");

            string _Id = "";

            #region data update
            if (dsCostingDetail.Tables[0].Rows.Count == 0)
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID("dbo.OrderCostingDetail", out _Id);

                OrderCostingDetail["Id"] = "PCD" + _Id;
                AddNewOrderCostingDetailRow(dsCostingDetail.Tables[0], OrderCostingDetail);
            }
            else
            {
                _Id = OrderCostingDetail["Id"].ToString();
                EditNewOrderCostingDetailRow(dsCostingDetail.Tables[0].Rows[0], OrderCostingDetail);
            }
            #endregion data update

            // Save to Database 
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsCostingDetail);
        }
        [HttpPost, Authorize]
        public ActionResult SaveCostingItemsIncludingComponent(IEnumerable<CostingItem> costingItems, Dictionary<string, object> OrderCostingDetail)
        {

            SaveCostingItems(costingItems, out DataSet dsMaster);
            SaveOrderCostingDetail(OrderCostingDetail, out DataSet dsCostingDetail);

            return Json(new { costingItems = dsMaster, OrderCostingDetail = dsCostingDetail, Message = AplosMessage.Updated });
        }

        [Authorize]
        public ActionResult GetBuyerDataByCostingMasterId(string costingMasterId)
        {
            string sql = @"select cb.*, b.UserName as Buyer from [dbo].[CostingBuyer] cb
                            left join hkp.Buyer b on b.Id = cb.BuyerId where OrderCostingMasterTemplateId = '" + costingMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult DeleteCostingBuyer(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[CostingBuyer] where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemByComponentId(string costingComponentId)
        {
            //string sql = @"select ci.CostingComponentId,ci.Code,ci.StandardName from hkp.CostingItem ci
            //        left JOIN  hkp.CostingComponent cc ON ci.CostingComponentId=cc.Id";

            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,um.UserName as UnitOfMeasurement,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, pcdm.Consumption,
                            pcdm.UOM, pcdm.Rate,pcdm.Description as dmDescription, pcdm.ValueLoss,pcdm.GrossConsumption,pcdm.GrossAmount, pcdm.Id  ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            left join OrderPreCostingDirectMaterial pcdm on pcdm.CostingItemId = ci.Id 
                            left join SCS.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                           where CostingComponentId = '" + costingComponentId + "' ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveOrderPreCostingDirectMaterial(IEnumerable<OrderPreCostingDirectMaterial> data, string OrderCostingMasterTemplateId, string cs)
        {
            DataSet dsMaster, dsProMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingDirectMaterial] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    string sqlPro = "SELECT * FROM [dbo].[OrderProcurementCostingDirectMaterial] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsProMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";
                        dsProMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Consumption > 0 && item.Rate > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DM" + GetPK("OrderPreCostingDirectMaterial");
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["Consumption"] = item.Consumption;
                            dr["UOM"] = item.UOM;
                            dr["Rate"] = item.Rate;
                            dr["ValueLoss"] = item.ValueLoss;
                            dr["GrossConsumption"] = item.GrossConsumption;
                            dr["GrossAmount"] = item.GrossAmount;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["SourcingType"] = item.SourcingType;
                            dr["Usage"] = item.Usage;
                            dr["POCriteria"] = item.POCriteria;
                            dr["IsUDApplicable"] = item.IsUDApplicable;
                            dr["IsGeneric"] = item.IsGeneric;
                            dr["IsMandatory"] = item.IsMandatory;
                            dr["MaterialMasterId"] = item.MaterialMasterId;
                            dr["ArticleId"] = item.ArticleId;
                            dr["VendorId"] = item.VendorId;


                            dr["ProcurementLevel"] = item.ProcurementLevel;
                            dr["BOQDays"] = item.BOQDays;
                            dr["BOQCriteria"] = item.BOQCriteria;
                            dr["DependentDate"] = item.DependentDate;

                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["Particulars"] = item.Particulars;
                            dr["Remarks"] = item.Remarks;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;




                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();


                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["Consumption"] = item.Consumption;
                            dr["UOM"] = item.UOM;
                            dr["Rate"] = item.Rate;
                            dr["ValueLoss"] = item.ValueLoss;
                            dr["GrossConsumption"] = item.GrossConsumption;

                            dr["GrossAmount"] = item.GrossAmount;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["SourcingType"] = item.SourcingType;
                            dr["Usage"] = item.Usage;
                            dr["POCriteria"] = item.POCriteria;

                            dr["IsUDApplicable"] = item.IsUDApplicable;
                            dr["IsGeneric"] = item.IsGeneric;
                            dr["IsMandatory"] = item.IsMandatory;
                            dr["MaterialMasterId"] = item.MaterialMasterId;
                            dr["ArticleId"] = item.ArticleId;
                            dr["VendorId"] = item.VendorId;


                            dr["ProcurementLevel"] = item.ProcurementLevel;
                            dr["BOQDays"] = item.BOQDays;
                            dr["BOQCriteria"] = item.BOQCriteria;
                            dr["DependentDate"] = item.DependentDate;

                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["Particulars"] = item.Particulars;
                            dr["Remarks"] = item.Remarks;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();



                            if (cs == "PreCosting")
                            {
                                if (dsProMaster.Tables[0].DefaultView.Count > 0)
                                {
                                    DataRow drpro = dsProMaster.Tables[0].DefaultView[0].Row;
                                    drpro.BeginEdit();

                                    drpro["CostingItemId"] = item.CostingItemId;
                                    drpro["Sequence"] = item.Sequence;
                                    drpro["Consumption"] = item.Consumption;
                                    drpro["UOM"] = item.UOM;
                                    drpro["Rate"] = item.Rate;
                                    drpro["ValueLoss"] = item.ValueLoss;
                                    drpro["GrossConsumption"] = item.GrossConsumption;
                                    drpro["GrossAmount"] = item.GrossAmount;
                                    drpro["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                    drpro["ResponsiblePersonId"] = item.ResponsiblePersonId;
                                    drpro["SourcingType"] = item.SourcingType;
                                    drpro["Usage"] = item.Usage;
                                    drpro["POCriteria"] = item.POCriteria;
                                    drpro["IsUDApplicable"] = item.IsUDApplicable;
                                    drpro["IsGeneric"] = item.IsGeneric;
                                    drpro["IsMandatory"] = item.IsMandatory;
                                    drpro["MaterialMasterId"] = item.MaterialMasterId;
                                    drpro["ArticleId"] = item.ArticleId;
                                    drpro["VendorId"] = item.VendorId;
                                    drpro["ProcurementLevel"] = item.ProcurementLevel;
                                    drpro["BOQDays"] = item.BOQDays;
                                    drpro["BOQCriteria"] = item.BOQCriteria;
                                    drpro["DependentDate"] = item.DependentDate;
                                    drpro["MinimumOfQuantity"] = item.MinimumOfQuantity;
                                    drpro["POIssueDeadLine"] = item.POIssueDeadLine;
                                    drpro["Particulars"] = item.Particulars;
                                    drpro["Remarks"] = item.Remarks;
                                    drpro["PurchaseGroupId"] = item.PurchaseGroupId;
                                    drpro["UpdatedBy"] = identity.Name;
                                    drpro["UpdatedDate"] = System.DateTime.Now.ToString();
                                    drpro["UpdatedFromIP"] = identity.IPAddress;

                                    drpro.EndEdit();
                                }
                            }
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsProMaster);

                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public ActionResult GetDirectCostingMaterialWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sqlPre = @"select ci.CostingComponentId,m.Sequence,m.Consumption,m.VendorId,p.UserName AS Vendor,CI.Id AS CostingItemId,m.OrderCostingMasterTemplateId,m.GrossAmount,m.GrossConsumption,m.Id,m.Rate,m.ResponsiblePersonId,m.UOM
                        ,isnull(m.ValueLoss,ci.Wastage) AS ValueLoss,M.Remarks,M.ProcurementLevel,M.BOQDays,M.DependentDate,M.BOQCriteria
                        ,m.SourcingType, ISNULL(m.IsUDApplicable,0) AS IsUDApplicable, m.Usage, ISNULL(m.IsGeneric,0) AS IsGeneric,ISNULL(m.IsMandatory,0) AS  IsMandatory
						,m.MaterialMasterId, m.ArticleId, mm.UserName as MaterialMasterName, mma.StandardName as ArticleName
                        ,e.EmployeeName as ResponsiblePerson, e.SystemId as ResponsiblePersonId,um.UserName as UnitOfMeasurement, um.Id as UoMId, ci.UserName
                        ,ISNULL(m.MinimumOfQuantity,ci.MinimumOfQuantity) AS MinimumOfQuantity,ISNULL(m.POIssueDeadLine,ci.POIssueDeadLine)POIssueDeadLine
                        ,ISNULL(m.PurchaseGroupId,ci.PurchaseGroupId) AS PurchaseGroupId,ISNULL(m.Particulars,ci.UserName) AS Particulars
                        ,M.FileName,M.FileOriginalName,m.POCriteria
						 from hkp.CostingItem ci
                        JOIN [dbo].[OrderPreCostingDirectMaterial] m on m.CostingItemId = ci.Id  and m.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"'
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                        left join [SCS].[UnitOfMeasurement] um on um.Id = ci.UnitOfMeasurementId
						left join dbo.EmployeeInformation e on e.SystemId = m.ResponsiblePersonId
						left join mst.MaterialMaster mm on mm.Id = m.MaterialMasterId 
						left join [MST].[MaterialMasterArticle] mma on mma.Id = m.ArticleId 
						LEFT JOIN hkp.Party AS p ON p.Id=m.VendorId
						WHERE ci.CostingComponentId='" + costingComponentId + @"'  Order By m.Sequence";

            string sqlProcurement = @"select ci.CostingComponentId,m.Sequence,m.Consumption,m.VendorId,p.UserName AS Vendor,CI.Id AS CostingItemId,m.OrderCostingMasterTemplateId,m.GrossAmount,m.GrossConsumption,m.Id,m.Rate,m.ResponsiblePersonId,m.UOM
                        ,isnull(m.ValueLoss,ci.Wastage) AS ValueLoss,M.Remarks,M.ProcurementLevel,M.BOQDays,M.DependentDate,M.BOQCriteria
                        ,m.SourcingType, ISNULL(m.IsUDApplicable,0) AS IsUDApplicable, m.Usage, ISNULL(m.IsGeneric,0) AS IsGeneric,ISNULL(m.IsMandatory,0) AS  IsMandatory
						,m.MaterialMasterId, m.ArticleId, mm.UserName as MaterialMasterName, mma.StandardName as ArticleName
                        ,e.EmployeeName as ResponsiblePerson, e.SystemId as ResponsiblePersonId,um.UserName as UnitOfMeasurement, um.Id as UoMId, ci.UserName
                        ,ISNULL(m.MinimumOfQuantity,ci.MinimumOfQuantity) AS MinimumOfQuantity,ISNULL(m.POIssueDeadLine,ci.POIssueDeadLine)POIssueDeadLine
                        ,ISNULL(m.PurchaseGroupId,ci.PurchaseGroupId) AS PurchaseGroupId,ISNULL(m.Particulars,ci.UserName) AS Particulars
                      ,M.FileName,M.FileOriginalName,m.POCriteria
						 from hkp.CostingItem ci
                        JOIN [dbo].[OrderProcurementCostingDirectMaterial] m on m.CostingItemId = ci.Id  and m.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"'
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                        left join [SCS].[UnitOfMeasurement] um on um.Id = ci.UnitOfMeasurementId
						left join dbo.EmployeeInformation e on e.SystemId = m.ResponsiblePersonId
						left join mst.MaterialMaster mm on mm.Id = m.MaterialMasterId 
						left join [MST].[MaterialMasterArticle] mma on mma.Id = m.ArticleId 
						LEFT JOIN hkp.Party AS p ON p.Id=m.VendorId
						WHERE ci.CostingComponentId='" + costingComponentId + @"'  Order By m.Sequence";

            return Json(new { Pre = _sqlRepository.GetDataCollection(sqlPre, null), Procurement = _sqlRepository.GetDataCollection(sqlProcurement, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilter(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @" select m.*,e.EmployeeName as ResponsiblePerson,um.UserName as UnitOfMeasurement, um.Id as UoMId, ci.UserName, m.Description
                        ,m.MaterialMasterId, m.ArticleId, mm.UserName as MaterialMasterName, mma.StandardName as ArticleName from hkp.CostingItem ci
                        inner join [dbo].[OrderPreCostingDirectMaterial] m on m.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                        left join [SCS].[UnitOfMeasurement] um on um.Id = ci.UnitOfMeasurementId
						left join EmployeeInformation e on e.SystemId = m.ResponsiblePersonId
						left join mst.MaterialMaster mm on mm.Id = m.MaterialMasterId 
						left join [MST].[MaterialMasterArticle] mma on mma.Id = m.ArticleId 
                        where m.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductUOMOrderCosting(string ProductMasterId)
        {
            string sql = @"Select P.Id ProductMasterId,BUoM.Id AS Value,BUoM.UserName AS Text from [MST].[ProductMaster] P
                            LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=P.BaseUOMId
                            Where ISNULL(BUoM.Id,'')<>'' and p.Id='" + ProductMasterId + @"'
                            UNION ALL
                            Select AUom.ProductMasterId,BUoM.Id,BUoM.UserName from MST.ProductMasterAlternativeUoM AUoM 
                            LEFT JOIN SCS.UnitOfMeasurement BUoM ON BUoM.Id=AUom.AlternativeUOMId
                            Where ISNULL(BUoM.Id,'')<>'' and AUom.ProductMasterId='" + ProductMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteDirectMaterial(string DirectMaterialId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingDirectMaterial where id='" + DirectMaterialId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("REFERENCE"))
                    return Json(new { Error = true, Message = "Selected Issue Group has been used in Issue therefor cannot delete." }, JsonRequestBehavior.AllowGet);

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        #region Pre Costing Operation
        [HttpGet, Authorize]
        public ActionResult GetOperationWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sqlPre = @"select ci.CostingComponentId,o.Sequence,o.Id,CI.Id AS CostingItemId, o.[Value], o.[Description],e.EmployeeName as ResponsiblePerson, e.SystemId as ResponsiblePersonId,  ci.UserName, o.Description
                        ,O.FileName,O.FileOriginalName                           
                        from hkp.CostingItem ci
						 join [dbo].[OrderPreCostingOperation] o on o.CostingItemId = ci.Id  and o.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = o.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By o.Sequence";

            string sqlProcurement = @"select ci.CostingComponentId,o.Sequence,o.Id,CI.Id AS CostingItemId, o.[Value], o.[Description],e.EmployeeName as ResponsiblePerson, e.SystemId as ResponsiblePersonId,  ci.UserName, o.Description
                          ,O.FileName,O.FileOriginalName      
                        from hkp.CostingItem ci
						 join [dbo].[OrderProcurementCostingOperation] o on o.CostingItemId = ci.Id  and o.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = o.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By o.Sequence";

            return Json(new { Pre = _sqlRepository.GetDataCollection(sqlPre, null), Procurement = _sqlRepository.GetDataCollection(sqlProcurement, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveOperation(IEnumerable<OrderPreCostingOperation> data, string OrderCostingMasterTemplateId, string cs)
        {
            DataSet dsMaster, dsProMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingOperation] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    string sqlPro = "SELECT * FROM [dbo].[OrderProcurementCostingOperation] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsProMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";
                        dsProMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";
                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "CO" + GetPK("OrderPreCostingOperation");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Value"] = item.Value;
                            dr["Description"] = item.Description;


                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                            dr["Value"] = item.Value;
                            dr["Description"] = item.Description;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();

                            if (cs == "PreCosting")
                            {
                                if (dsProMaster.Tables[0].DefaultView.Count > 0)
                                {
                                    DataRow drpro = dsProMaster.Tables[0].DefaultView[0].Row;
                                    drpro.BeginEdit();

                                    drpro["CostingItemId"] = item.CostingItemId;
                                    drpro["Sequence"] = item.Sequence;
                                    drpro["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                    drpro["ResponsiblePersonId"] = item.ResponsiblePersonId;
                                    drpro["Value"] = item.Value;
                                    drpro["Description"] = item.Description;

                                    drpro["UpdatedBy"] = identity.Name;
                                    drpro["UpdatedDate"] = System.DateTime.Now.ToString();
                                    drpro["UpdatedFromIP"] = identity.IPAddress;

                                    drpro.EndEdit();
                                }
                            }
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsProMaster);
                }

                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public ActionResult GetCostingItemWithOperationByComponentId(string costingComponentId)
        {
            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, o.OrderCostingMasterTemplateId,
                                 ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                                 o.Description as dmDescription,o.Value ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                                 from hkp.CostingItem ci 
                                 left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
            left join [dbo].[OrderPreCostingOperation] o on o.CostingItemId = ci.Id 
                                where CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DeleteOperation(string operationId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingOperation where id='" + operationId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForOperation(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select o.*,e.EmployeeName as ResponsiblePerson, ci.UserName, o.Description from hkp.CostingItem ci
                        inner join  [dbo].[OrderPreCostingOperation] o on o.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = o.ResponsiblePersonId 
                            where o.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion Operation

        #region OrderCosting Direct Process
        [HttpGet, Authorize]
        public ActionResult GetDirectProcessWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sqlPre = @"select ci.CostingComponentId,p.Sequence,ci.UserName,p.Id,CI.Id AS CostingItemId, p.OrderCostingMasterTemplateId, p.ExecutionType,
       p.[Value], p.Rate, p.Amount, p.[Description],e.SystemId as ResponsiblePersonId, e.EmployeeName as ResponsiblePerson
                     ,P.FileName,P.FileOriginalName   
                        from hkp.CostingItem ci
                        join [dbo].[OrderPreCostingDirectProcess] p on CostingItemId = ci.Id and p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By p.Sequence";

            string sqlProcurement = @"select ci.CostingComponentId,p.Sequence,ci.UserName,p.Id,CI.Id AS CostingItemId, p.OrderCostingMasterTemplateId, p.ExecutionType,
       p.[Value], p.Rate, p.Amount, p.[Description],e.SystemId as ResponsiblePersonId, e.EmployeeName as ResponsiblePerson
                        ,P.FileName,P.FileOriginalName   
                        from hkp.CostingItem ci
                        join [dbo].[OrderProcurementCostingDirectProcess] p on CostingItemId = ci.Id and p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By p.Sequence";

            return Json(new { Pre = _sqlRepository.GetDataCollection(sqlPre, null), Procurement = _sqlRepository.GetDataCollection(sqlProcurement, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveDirectProcess(IEnumerable<OrderPreCostingDirectProcess> data, string OrderCostingMasterTemplateId, string cs)
        {
            DataSet dsMaster, dsProMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingDirectProcess] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    string sqlPro = "SELECT * FROM [dbo].[OrderProcurementCostingDirectProcess] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsProMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";
                        dsProMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Amount > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DP" + GetPK("OrderPreCostingDirectProcess");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["ExecutionType"] = item.ExecutionType;
                            dr["Value"] = item.Value;
                            dr["Rate"] = item.Rate;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["ExecutionType"] = item.ExecutionType;
                            dr["Value"] = item.Value;
                            dr["Rate"] = item.Rate;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dr.EndEdit();

                            if (cs == "PreCosting")
                            {
                                if (dsProMaster.Tables[0].DefaultView.Count > 0)
                                {
                                    DataRow drpro = dsProMaster.Tables[0].DefaultView[0].Row;
                                    drpro.BeginEdit();


                                    drpro["CostingItemId"] = item.CostingItemId;
                                    drpro["Sequence"] = item.Sequence;
                                    drpro["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                    drpro["ResponsiblePersonId"] = item.ResponsiblePersonId;
                                    drpro["ExecutionType"] = item.ExecutionType;
                                    drpro["Value"] = item.Value;
                                    drpro["Rate"] = item.Rate;
                                    drpro["Amount"] = item.Amount;
                                    drpro["Description"] = item.Description;

                                    drpro["UpdatedBy"] = identity.Name;
                                    drpro["UpdatedDate"] = System.DateTime.Now.ToString();
                                    drpro["UpdatedFromIP"] = identity.IPAddress;

                                    drpro.EndEdit();
                                }
                            }
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsProMaster);

                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemWithDirectProcessByComponentId(string costingComponentId)
        {
            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,um.UserName as UnitOfMeasurement,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                             p.Id , p.ExecutionType, p.Value, p.Rate, p.Amount,p.Description
							 ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            left join [dbo].[OrderPreCostingDirectProcess] p on p.CostingItemId = ci.Id 
                            left join SCS.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                            where CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteDirectProcess(string directProcessId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingDirectProcess where id='" + directProcessId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForDirectProcess(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select p.*,e.EmployeeName as ResponsiblePerson, ci.UserName
						from hkp.CostingItem ci
                        inner join [dbo].[OrderPreCostingDirectProcess]  p on p.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId 
                        where p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion Pre Costing Direct Process

        #region OrderCosting SalesExpense
        [HttpGet, Authorize]
        public ActionResult GetSalesExpenseWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sqlPre = @"select ci.CostingComponentId,s.Sequence,s.Id,CI.Id AS CostingItemId, s.OrderCostingMasterTemplateId, s.[Type], s.[Value],
       s.Amount, s.[Description],e.SystemId as ResponsiblePersonId, e.EmployeeName as ResponsiblePerson,ci.UserName,
                        S.FileName,S.FileOriginalName
                        from hkp.CostingItem ci
                        join [dbo].[OrderPreCostingSalesExpense] s on CostingItemId = ci.Id  and s.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = s.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By s.Sequence";

            string sqlProcurement = @"select ci.CostingComponentId,s.Sequence,s.Id,CI.Id AS CostingItemId, s.OrderCostingMasterTemplateId, s.[Type], s.[Value],
       s.Amount, s.[Description],e.SystemId as ResponsiblePersonId, e.EmployeeName as ResponsiblePerson,ci.UserName,
                        S.FileName,S.FileOriginalName
                        from hkp.CostingItem ci
                        join [dbo].[OrderProcurementCostingSalesExpense] s on CostingItemId = ci.Id  and s.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = s.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By s.Sequence";


            return Json(new { Pre = _sqlRepository.GetDataCollection(sqlPre, null), Procurement = _sqlRepository.GetDataCollection(sqlProcurement, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveSalesExpense(IEnumerable<OrderPreCostingSalesExpense> data, string OrderCostingMasterTemplateId, string cs)
        {
            DataSet dsMaster, dsProMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingSalesExpense] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    string sqlPro = "SELECT * FROM [dbo].[OrderProcurementCostingSalesExpense] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsProMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";
                        dsProMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DP" + GetPK("OrderPreCostingSalesExpense");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();


                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dr.EndEdit();

                            if (cs == "PreCosting")
                            {
                                if (dsProMaster.Tables[0].DefaultView.Count > 0)
                                {
                                    DataRow drpro = dsProMaster.Tables[0].DefaultView[0].Row;
                                    drpro.BeginEdit();


                                    drpro["CostingItemId"] = item.CostingItemId;
                                    drpro["Sequence"] = item.Sequence;
                                    drpro["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                    drpro["ResponsiblePersonId"] = item.ResponsiblePersonId;
                                    drpro["Type"] = item.Type;
                                    drpro["Value"] = item.Value;
                                    drpro["Amount"] = item.Amount;
                                    drpro["Description"] = item.Description;

                                    drpro["UpdatedBy"] = identity.Name;
                                    drpro["UpdatedDate"] = System.DateTime.Now.ToString();
                                    drpro["UpdatedFromIP"] = identity.IPAddress;

                                    drpro.EndEdit();
                                }
                            }

                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsProMaster);


                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemWithSalesExpenseByComponentId(string costingComponentId)
        {
            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,um.UserName as UnitOfMeasurement,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId,
                            s.Type, s.Value,s.Amount,s.Description as dmDescription,s.Id  ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                            
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            left join [dbo].[OrderPreCostingSalesExpense] s on s.CostingItemId = ci.Id 
                            left join SCS.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                            where CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteSalesExpense(string salesExpenseId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingSalesExpense where id='" + salesExpenseId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForSalesExpense(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select s.*,e.EmployeeName as ResponsiblePerson, ci.UserName
						from hkp.CostingItem ci
                        inner join [dbo].[OrderPreCostingSalesExpense]  s on s.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = s.ResponsiblePersonId
                        where s.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion OrderCosting SalesExpense

        #region OrderCosting ValueLoss
        [HttpGet, Authorize]
        public ActionResult GetValueLossWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sqlPre = @"select ci.CostingComponentId,p.Sequence,p.Id,CI.Id AS CostingItemId, p.OrderCostingMasterTemplateId, p.[Type], p.[Value],
                    p.Amount, p.[Description],e.EmployeeName as ResponsiblePerson,p.ResponsiblePersonId,ci.UserName,
                        P.FileName,P.FileOriginalName
                        from hkp.CostingItem ci
                        join [dbo].[OrderPreCostingValueLoss] p on CostingItemId = ci.Id  and p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By p.Sequence";

            string sqlProcurement = @"select ci.CostingComponentId,p.Sequence,p.Id,CI.Id AS CostingItemId, p.OrderCostingMasterTemplateId, p.[Type], p.[Value],
                    p.Amount, p.[Description],e.EmployeeName as ResponsiblePerson,p.ResponsiblePersonId,ci.UserName,
                        P.FileName,P.FileOriginalName
                        from hkp.CostingItem ci
                        join [dbo].[OrderProcurementCostingValueLoss] p on CostingItemId = ci.Id  and p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By p.Sequence";

            return Json(new { Pre = _sqlRepository.GetDataCollection(sqlPre, null), Procurement = _sqlRepository.GetDataCollection(sqlProcurement, null) }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProfitWithItemByComponentId(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sqlPre = @"select ci.CostingComponentId,p.Sequence,p.Id,CI.Id AS CostingItemId, p.OrderCostingMasterTemplateId, p.[Type], p.[Value],
                    p.Amount, p.[Description],e.EmployeeName as ResponsiblePerson,p.ResponsiblePersonId,ci.UserName,
                        P.FileName,P.FileOriginalName
                        from hkp.CostingItem ci
                        join [dbo].[OrderPreCostingProfit] p on CostingItemId = ci.Id  and p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By p.Sequence";
            string sqlProcurement = @"select ci.CostingComponentId,p.Sequence,p.Id,CI.Id AS CostingItemId, p.OrderCostingMasterTemplateId, p.[Type], p.[Value],
                    p.Amount, p.[Description],e.EmployeeName as ResponsiblePerson,p.ResponsiblePersonId,ci.UserName,
                        P.FileName,P.FileOriginalName
                        from hkp.CostingItem ci
                        join [dbo].[OrderProcurementCostingProfit] p on CostingItemId = ci.Id  and p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"' 
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where ci.CostingComponentId = '" + costingComponentId + "'  Order By p.Sequence";
            return Json(new { Pre = _sqlRepository.GetDataCollection(sqlPre, null), Procurement = _sqlRepository.GetDataCollection(sqlProcurement, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveValueLoss(IEnumerable<OrderPreCostingValueLoss> data, string OrderCostingMasterTemplateId, string cs)
        {
            DataSet dsMaster, dsProMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingValueLoss] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    string sqlPro = "SELECT * FROM [dbo].[OrderProcurementCostingValueLoss] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsProMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";
                        dsProMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DP" + GetPK("OrderPreCostingSalesExpense");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();


                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();

                            if (cs == "PreCosting")
                            {
                                if (dsProMaster.Tables[0].DefaultView.Count > 0)
                                {
                                    DataRow drpro = dsProMaster.Tables[0].DefaultView[0].Row;
                                    drpro.BeginEdit();


                                    drpro["CostingItemId"] = item.CostingItemId;
                                    drpro["Sequence"] = item.Sequence;
                                    drpro["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                    drpro["ResponsiblePersonId"] = item.ResponsiblePersonId;
                                    drpro["Type"] = item.Type;
                                    drpro["Value"] = item.Value;
                                    drpro["Amount"] = item.Amount;
                                    drpro["Description"] = item.Description;

                                    drpro["UpdatedBy"] = identity.Name;
                                    drpro["UpdatedDate"] = System.DateTime.Now.ToString();
                                    drpro["UpdatedFromIP"] = identity.IPAddress;

                                    drpro.EndEdit();
                                }
                            }
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsProMaster);

                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost, Authorize]
        public ActionResult SaveProfit(IEnumerable<OrderPreCostingProfit> data, string OrderCostingMasterTemplateId, string cs)
        {
            DataSet dsMaster, dsProMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderPreCostingProfit] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    string sqlPro = "SELECT * FROM [dbo].[OrderProcurementCostingProfit] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsProMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";
                        dsProMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "P" + GetPK("OrderPreCostingProfit");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["Sequence"] = item.Sequence;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();


                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();

                            if (cs == "PreCosting")
                            {
                                if (dsProMaster.Tables[0].DefaultView.Count > 0)
                                {
                                    DataRow drpro = dsProMaster.Tables[0].DefaultView[0].Row;
                                    drpro.BeginEdit();


                                    drpro["CostingItemId"] = item.CostingItemId;
                                    drpro["Sequence"] = item.Sequence;
                                    drpro["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                                    drpro["ResponsiblePersonId"] = item.ResponsiblePersonId;
                                    drpro["Type"] = item.Type;
                                    drpro["Value"] = item.Value;
                                    drpro["Amount"] = item.Amount;
                                    drpro["Description"] = item.Description;

                                    drpro["UpdatedBy"] = identity.Name;
                                    drpro["UpdatedDate"] = System.DateTime.Now.ToString();
                                    drpro["UpdatedFromIP"] = identity.IPAddress;

                                    drpro.EndEdit();
                                }
                            }
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsProMaster);

                }
                RecalculateValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemWithValueLossByComponentId(string costingComponentId)
        {
            string sql = @"select ci.CostingComponentId,ci.Id as CostingItemId,um.UserName as UnitOfMeasurement,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.Description,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId,
                            s.Type, s.Value,s.Amount,s.Description as dmDescription,s.Id  ,ci.POIssueDeadLine, ci.Wastage,ci.Description
                            
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            left join [dbo].[OrderPreCostingSalesExpense] s on s.CostingItemId = ci.Id 
                            left join SCS.UnitOfMeasurement um on um.Id = ci.UnitOfMeasurementId
                            where CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteValueLoss(string ValueLossId)
        {

            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingValueLoss where id='" + ValueLossId + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingItemsWithoutFilterForValueLoss(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sql = @"select p.*, ci.UserName, e.EmployeeName ResponsiblePerson
						from hkp.CostingItem ci
                        inner join [dbo].[OrderPreCostingValueLoss]  p on p.CostingItemId = ci.Id
                        left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
						left join EmployeeInformation e on e.SystemId = p.ResponsiblePersonId
                        where p.OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + "' and ci.CostingComponentId = '" + costingComponentId + "' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Costing formula 

        [HttpGet, Authorize]
        public ActionResult CalculateFormula(string OrderCostingMasterTemplateId)
        {
            string sql = @"select sum(D.DirectMaterialCost) AS TotalDirectMaterial, sum(D.OperationCost) as TotalOperation,sum(D.ProcessCost) TotalProcess from 
                    (
                    select  sum(GrossAmount) AS DirectMaterialCost,0 AS OperationCost,0 AS ProcessCost from OrderPreCostingDirectMaterial  where OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"'
                    union all 
                    select 0, sum(Value) as OperationCost, 0 as ProcessCost from [dbo].[OrderPreCostingOperation] where OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"'
                    union all
                    select 0 , 0, sum(Value) as TotalOperation from [dbo].[OrderPreCostingDirectProcess] where OrderCostingMasterTemplateId = '" + OrderCostingMasterTemplateId + @"'
                    ) AS D";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        #endregion end Costing formula 
        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            try
            {
                string CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE  EMP.EmployeeStatus='Active' 
                                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void RecalculateValues_backup(string TemplateMasterId)
        {
            try
            {
                //recalculate Direct Process
                string sql = @"UPDATE [OrderPreCostingDirectProcess] SET amount=isnull(Rate,0)+((MM.GrossAmount)*VALUE/100)
                            FROM [OrderPreCostingDirectProcess] AS K
                            LEFT JOIN (
                            SELECT OrderCostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.OrderCostingMasterTemplateId, M.GrossAmount
							                              FROM [OrderPreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.OrderCostingMasterTemplateId,M.[Value] 
								                        FROM [OrderPreCostingOperation] M) AS K GROUP BY K.OrderCostingMasterTemplateId
                            ) AS MM ON mm.OrderCostingMasterTemplateId=k.OrderCostingMasterTemplateId

                            WHERE k.OrderCostingMasterTemplateId='" + TemplateMasterId + "'";

                _sqlRepository.ExecuteSqlCommand(sql);

                sql = @"UPDATE [OrderPreCostingSalesExpense] SET amount=case when k.[Type]='FIXED' THEN Value ELSE ((MM.GrossAmount)*VALUE/100) END
                            FROM [OrderPreCostingSalesExpense] AS K
                            LEFT JOIN (
                            SELECT OrderCostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.OrderCostingMasterTemplateId, M.GrossAmount
								                            FROM [OrderPreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.OrderCostingMasterTemplateId,M.[Value] 
								                        FROM [OrderPreCostingOperation] M
							                            UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM [OrderPreCostingDirectProcess] M

								                            ) AS K GROUP BY K.OrderCostingMasterTemplateId
                            ) AS MM ON mm.OrderCostingMasterTemplateId=k.OrderCostingMasterTemplateId

                            WHERE k.OrderCostingMasterTemplateId='" + TemplateMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(sql);

                sql = @"UPDATE [OrderPreCostingValueLoss] SET amount=case when k.[Type]='FIXED' THEN Value ELSE ((MM.GrossAmount)*VALUE/100) END
                            FROM [OrderPreCostingValueLoss] AS K
                            LEFT JOIN (
                            SELECT OrderCostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.OrderCostingMasterTemplateId, M.GrossAmount
								                            FROM [OrderPreCostingDirectMaterial] M
							                            UNION ALL
							                           SELECT m.OrderCostingMasterTemplateId,M.[Value] 
								                        FROM [OrderPreCostingOperation] M
							                            UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM [OrderPreCostingDirectProcess] M

								                            ) AS K GROUP BY K.OrderCostingMasterTemplateId
                            ) AS MM ON mm.OrderCostingMasterTemplateId=k.OrderCostingMasterTemplateId

                            WHERE k.OrderCostingMasterTemplateId='" + TemplateMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(sql);

                sql = @"UPDATE [OrderPreCostingProfit] SET amount=case when k.[Type]='FIXED' THEN Value ELSE ((MM.GrossAmount)*VALUE/100) END
                            FROM [OrderPreCostingProfit] AS K
                            LEFT JOIN (
                            SELECT OrderCostingMasterTemplateId, SUM(GrossAmount) AS GrossAmount FROM (
								                            SELECT m.OrderCostingMasterTemplateId, M.GrossAmount
								                            FROM [OrderPreCostingDirectMaterial] M
							                                UNION ALL
							                                SELECT m.OrderCostingMasterTemplateId,M.[Value] 
								                            FROM [OrderPreCostingOperation] M
							                                UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM [OrderPreCostingDirectProcess] M
								                             UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM OrderPreCostingValueLoss M
								                            
								                               UNION ALL
								                            SELECT m.OrderCostingMasterTemplateId,M.Amount 
								                            FROM OrderPreCostingSalesExpense AS M
								                            ) AS K GROUP BY K.OrderCostingMasterTemplateId
                            ) AS MM ON mm.OrderCostingMasterTemplateId=k.OrderCostingMasterTemplateId

                            WHERE k.OrderCostingMasterTemplateId='" + TemplateMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(sql);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        public void RecalculateValues(string TemplateMasterId)
        {
            try
            {
                //recalculate Direct Process
                string sql = @" SELECT  ci.Id,CC.CalculationMethod, ctc.Sequence AS ComponentSequence,ci.Sequence AS ItemSequnce, ci.CostingCategoryId, ci.CostingComponentId,cc.CostingSegment,upper(isnull(itemval.ValueType,'FIXED')) AS ValueType,
                        isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount,isnull(itemval.Value,0) AS Value,isnull(itemval.Rate,0) AS Rate
						  from  OrderCostingDetailTemplate D 
						 INNER JOIN OrderCostingMasterTemplate AS cmt ON cmt.Id=d.OrderCostingMasterTemplateId
						 inner join hkp.CostingComponent CC on cc.id=d.CostingComponentId
						 INNER JOIN hkp.CostingItem AS ci ON ci.CostingComponentId=cc.Id
                         left outer join [dbo].[CostingTypeComponent] AS ctc  
                         ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster 
                                                                                  WHERE Id = cmt.ProductMasterId)

                         LEFT OUTER JOIN (SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.GrossAmount AS TotalGrossAmount FROM OrderPreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + TemplateMasterId + @"' 
                                            UNION ALL SELECT 'PERCENTAGE' AS ValueType, PC.Value,PC.Rate, i.Id,pc.Amount AS TotalGrossAmount FROM OrderPreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                            UNION ALL SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.[Value]  AS TotalGrossAmount FROM OrderPreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + TemplateMasterId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,          pc.Amount AS TotalGrossAmount FROM OrderPreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderPreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderPreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                  )AS ITEMVAL ON  itemval.Id=ci.Id
                         WHERE d.OrderCostingMasterTemplateId='" + TemplateMasterId + @"'
                          order by ctc.Sequence,ci.Sequence";

                DataTable dtReference = _sqlRepository.GetDataTable(sql);

                for (int i = 0; i < dtReference.Rows.Count; i++)
                {
                    if (dtReference.Rows[i]["CostingSegment"].ToString().ToUpper() != "DIRECTPROCESS" && (clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString()) > 0 || dtReference.Rows[i]["ValueType"].ToString().ToUpper() == "PERCENTAGE"))
                    {
                        double TotalFixedValue = getFixedAmount(dtReference, clsStaticInfo.dbl(dtReference.Rows[i]["ComponentSequence"].ToString()));
                        double TotalCurrentFixed = getCurrentFixedAmount(dtReference, clsStaticInfo.dbl(dtReference.Rows[i]["ComponentSequence"].ToString()));
                        double CurrentPercent = getCurrentPercent(dtReference, clsStaticInfo.dbl(dtReference.Rows[i]["ComponentSequence"].ToString()));

                        double CurrentGrossValue = 0;
                        double Percentage = 0;
                        if (clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString()) > 0)
                        {
                            //because we have both fixed value and a rate (e.g. DirectProcess) which will sum up to TotalGrossValue
                            CurrentGrossValue = clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString());
                        }
                        Percentage = clsStaticInfo.dbl(dtReference.Rows[i]["Value"].ToString());


                        //now add percentage portion with the CurrentGrossValue
                        if (dtReference.Rows[i]["CalculationMethod"].ToString().ToUpper() == "CUMULATIVE")
                            CurrentGrossValue += TotalFixedValue * (Percentage / 100);
                        else
                            CurrentGrossValue += ((TotalFixedValue + TotalCurrentFixed) / ((100 - CurrentPercent) / 100)) * (Percentage / 100);
                        //CurrentGrossValue += TotalFixedValue * (Percentage / 100);
                        dtReference.Rows[i]["TotalGrossAmount"] = CurrentGrossValue;
                    }
                    else if (dtReference.Rows[i]["CostingSegment"].ToString().ToUpper() == "DIRECTPROCESS")
                    {
                        double TotalFixedValue = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "CostingSegment='DirectMaterial'").ToString());
                        double CurrentGrossValue = 0;
                        double Percentage = 0;
                        if (clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString()) > 0)
                        {
                            //because we have both fixed value and a rate (e.g. DirectProcess) which will sum up to TotalGrossValue
                            CurrentGrossValue = clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString());
                        }
                        Percentage = clsStaticInfo.dbl(dtReference.Rows[i]["Value"].ToString());


                        //now add percentage portion with the CurrentGrossValue
                        CurrentGrossValue += (TotalFixedValue / ((100 - Percentage) / 100)) - TotalFixedValue; //TotalFixedValue * (Percentage / 100);TotalFixedValue * (Percentage / 100);

                        dtReference.Rows[i]["TotalGrossAmount"] = CurrentGrossValue;
                    }
                }


                //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                dtReference.DefaultView.RowFilter = null;
                DataTable dvDistinctSegment = dtReference.DefaultView.ToTable(true, "CostingSegment");
                for (int i = 0; i < dvDistinctSegment.Rows.Count; i++)
                {
                    if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.DirectMaterial.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderPreCostingDirectMaterial", CostingSegment.DirectMaterial.ToString(), "GrossAmount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.Operation.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderPreCostingOperation", CostingSegment.Operation.ToString(), "Value", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.DirectProcess.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderPreCostingDirectProcess", CostingSegment.DirectProcess.ToString(), "Amount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.SalesExpense.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderPreCostingSalesExpense", CostingSegment.SalesExpense.ToString(), "Amount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.ValueLoss.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderPreCostingValueLoss", CostingSegment.ValueLoss.ToString(), "Amount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.Profit.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderPreCostingProfit", CostingSegment.Profit.ToString(), "Amount", dtReference);


                }

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        public void RecalculateProcurementValues(string TemplateMasterId)
        {
            try
            {
                //recalculate Direct Process
                string sql = @" SELECT  ci.Id,CC.CalculationMethod, ctc.Sequence AS ComponentSequence,ci.Sequence AS ItemSequnce, ci.CostingCategoryId, ci.CostingComponentId,cc.CostingSegment,upper(isnull(itemval.ValueType,'FIXED')) AS ValueType,
                        isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount,isnull(itemval.Value,0) AS Value,isnull(itemval.Rate,0) AS Rate
						  from  OrderCostingDetailTemplate D 
						 INNER JOIN OrderCostingMasterTemplate AS cmt ON cmt.Id=d.OrderCostingMasterTemplateId
						 inner join hkp.CostingComponent CC on cc.id=d.CostingComponentId
						 INNER JOIN hkp.CostingItem AS ci ON ci.CostingComponentId=cc.Id
                         left outer join [dbo].[CostingTypeComponent] AS ctc  
                         ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster 
                                                                                  WHERE Id = cmt.ProductMasterId)

                         LEFT OUTER JOIN (SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.GrossAmount AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + TemplateMasterId + @"' 
                                            UNION ALL SELECT 'PERCENTAGE' AS ValueType, PC.Value,PC.Rate, i.Id,pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                            UNION ALL SELECT 'FIXED' AS ValueType, 0 AS Value,0 AS Rate, i.Id,pc.[Value]  AS TotalGrossAmount FROM OrderProcurementCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + TemplateMasterId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,          pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                            UNION ALL SELECT pc.[Type],            PC.Value,0 AS Rate,           i.Id,           pc.Amount AS TotalGrossAmount FROM OrderProcurementCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateMasterId + @"'	
                                  )AS ITEMVAL ON  itemval.Id=ci.Id
                         WHERE d.OrderCostingMasterTemplateId='" + TemplateMasterId + @"'
                          order by ctc.Sequence,ci.Sequence";

                DataTable dtReference = _sqlRepository.GetDataTable(sql);

                for (int i = 0; i < dtReference.Rows.Count; i++)
                {
                    if (dtReference.Rows[i]["CostingSegment"].ToString().ToUpper() != "DIRECTPROCESS" && (clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString()) > 0 || dtReference.Rows[i]["ValueType"].ToString().ToUpper() == "PERCENTAGE"))
                    {
                        double TotalFixedValue = getFixedAmount(dtReference, clsStaticInfo.dbl(dtReference.Rows[i]["ComponentSequence"].ToString()));
                        double TotalCurrentFixed = getCurrentFixedAmount(dtReference, clsStaticInfo.dbl(dtReference.Rows[i]["ComponentSequence"].ToString()));
                        double CurrentPercent = getCurrentPercent(dtReference, clsStaticInfo.dbl(dtReference.Rows[i]["ComponentSequence"].ToString()));

                        double CurrentGrossValue = 0;
                        double Percentage = 0;
                        if (clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString()) > 0)
                        {
                            //because we have both fixed value and a rate (e.g. DirectProcess) which will sum up to TotalGrossValue
                            CurrentGrossValue = clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString());
                        }
                        Percentage = clsStaticInfo.dbl(dtReference.Rows[i]["Value"].ToString());


                        //now add percentage portion with the CurrentGrossValue
                        if (dtReference.Rows[i]["CalculationMethod"].ToString().ToUpper() == "CUMULATIVE")
                            CurrentGrossValue += TotalFixedValue * (Percentage / 100);
                        else
                            CurrentGrossValue += ((TotalFixedValue + TotalCurrentFixed) / ((100 - CurrentPercent) / 100)) * (Percentage / 100);
                        //CurrentGrossValue += TotalFixedValue * (Percentage / 100);
                        dtReference.Rows[i]["TotalGrossAmount"] = CurrentGrossValue;
                    }
                    else if (dtReference.Rows[i]["CostingSegment"].ToString().ToUpper() == "DIRECTPROCESS")
                    {
                        double TotalFixedValue = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "CostingSegment='DirectMaterial'").ToString());
                        double CurrentGrossValue = 0;
                        double Percentage = 0;
                        if (clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString()) > 0)
                        {
                            //because we have both fixed value and a rate (e.g. DirectProcess) which will sum up to TotalGrossValue
                            CurrentGrossValue = clsStaticInfo.dbl(dtReference.Rows[i]["Rate"].ToString());
                        }
                        Percentage = clsStaticInfo.dbl(dtReference.Rows[i]["Value"].ToString());


                        //now add percentage portion with the CurrentGrossValue
                        CurrentGrossValue += (TotalFixedValue / ((100 - Percentage) / 100)) - TotalFixedValue; //TotalFixedValue * (Percentage / 100);TotalFixedValue * (Percentage / 100);

                        dtReference.Rows[i]["TotalGrossAmount"] = CurrentGrossValue;
                    }
                }


                //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                dtReference.DefaultView.RowFilter = null;
                DataTable dvDistinctSegment = dtReference.DefaultView.ToTable(true, "CostingSegment");
                for (int i = 0; i < dvDistinctSegment.Rows.Count; i++)
                {
                    if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.DirectMaterial.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderProcurementCostingDirectMaterial", CostingSegment.DirectMaterial.ToString(), "GrossAmount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.Operation.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderProcurementCostingOperation", CostingSegment.Operation.ToString(), "Value", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.DirectProcess.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderProcurementCostingDirectProcess", CostingSegment.DirectProcess.ToString(), "Amount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.SalesExpense.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderProcurementCostingSalesExpense", CostingSegment.SalesExpense.ToString(), "Amount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.ValueLoss.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderProcurementCostingValueLoss", CostingSegment.ValueLoss.ToString(), "Amount", dtReference);

                    else if (dvDistinctSegment.Rows[i]["CostingSegment"].ToString() == CostingSegment.Profit.ToString())
                        UpdateCostingItems(TemplateMasterId, "OrderProcurementCostingProfit", CostingSegment.Profit.ToString(), "Amount", dtReference);


                }

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

        private void UpdateCostingItems(string OrderCostingMasterTemplateId, string TableName, string SegmentName, string UpdateColumnName, DataTable dtReference)
        {

            string strSql = "Select* from " + TableName + " Where OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
            DataSet dsData;
            ConnectionManager.DAL.ConManager objCon;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(strSql, out dsData, false, "1");

            if (dsData.Tables[0].Rows.Count == 0)
                return;


            dtReference.DefaultView.RowFilter = "CostingSegment='" + SegmentName + "'";
            for (int i = 0; i < dtReference.DefaultView.Count; i++)
            {
                dsData.Tables[0].DefaultView.RowFilter = "CostingItemId='" + dtReference.DefaultView[i]["Id"].ToString() + "'";
                if (dsData.Tables[0].DefaultView.Count > 0)
                {
                    DataRow dr = dsData.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr[UpdateColumnName] = clsStaticInfo.dbl(dtReference.DefaultView[i]["TotalGrossAmount"].ToString());
                    dr.EndEdit();
                }
            }


            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsData);
        }

        private double getFixedAmount(DataTable dtReference, double CurrentSequence)
        {
            double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "ComponentSequence<" + CurrentSequence).ToString());
            //totalPrevious += clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "(Rate=0 OR ValueType<>'PERCENTAGE') AND ComponentSequence=" + CurrentSequence).ToString());
            return totalPrevious;
        }
        private double getCurrentFixedAmount(DataTable dtReference, double CurrentSequence)
        {
            //double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "ComponentSequence<" + CurrentSequence).ToString());
            double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "(ValueType<>'PERCENTAGE') AND ComponentSequence=" + CurrentSequence).ToString());
            totalPrevious += clsStaticInfo.dbl(dtReference.Compute("SUM(Rate)", "(Rate>0 AND ValueType='PERCENTAGE') AND ComponentSequence=" + CurrentSequence).ToString());
            return totalPrevious;
        }
        private double getCurrentPercent(DataTable dtReference, double CurrentSequence)
        {
            //double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(TotalGrossAmount)", "ComponentSequence<" + CurrentSequence).ToString());
            double totalPrevious = clsStaticInfo.dbl(dtReference.Compute("SUM(Value)", "ValueType='PERCENTAGE' AND ComponentSequence=" + CurrentSequence).ToString());
            return totalPrevious;
        }

        #region Remove document 
        //public ActionResult DeleteDocumentPosition(string id)
        //{
        //    //_complianceDocumentPositonCodeService.Delete(id);
        //    return Json(new { Message = AplosMessage.Deleted });
        //}
        #endregion Remove document 


        public ActionResult CopyCostingTemplate(Dictionary<string, object> CopyData, List<Dictionary<string, object>> SalesOrderList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string NewId = "";
            try
            {
                DataSet OrderCostingMasterTemplate
                , OrderCostingDetailTemplate
                , OrderPreCostingDirectMaterial
                , OrderPreCostingDirectMaterialConsumption
                , OrderPreCostingDirectMaterialChild
                , OrderPreCostingOperation
                , OrderPreCostingDirectProcess
                , OrderPreCostingValueLoss
                , OrderPreCostingSalesExpense
                , OrderPreCostingProfit


                , OrderProcurementCostingDirectMaterial
                , OrderProcurementCostingDirectMaterialConsumption
                , OrderProcurementCostingDirectMaterialChild
                , OrderProcurementCostingOperation
                , OrderProcurementCostingDirectProcess
                , OrderProcurementCostingValueLoss
                , OrderProcurementCostingSalesExpense
                , OrderProcurementCostingProfit;

                string SourceId = CopyData["CostingMasterTemplateId"].ToString();

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + CopyData["Code"].ToString() + "'", out OrderCostingMasterTemplate, false, "1");
                if (OrderCostingMasterTemplate.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + CopyData["UserName"].ToString() + "'", out OrderCostingMasterTemplate, false, "1");
                if (OrderCostingMasterTemplate.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists");

                con.OpenDataSetThroughAdapter("select * from OrderCostingDetailTemplate where 1=2", out OrderCostingDetailTemplate, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectMaterial where 1=2", out OrderPreCostingDirectMaterial, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectMaterialConsumption where 1=2", out OrderPreCostingDirectMaterialConsumption, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectMaterialChild where 1=2", out OrderPreCostingDirectMaterialChild, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingOperation where 1=2", out OrderPreCostingOperation, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectProcess where 1=2", out OrderPreCostingDirectProcess, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingValueLoss where 1=2", out OrderPreCostingValueLoss, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingSalesExpense where 1=2", out OrderPreCostingSalesExpense, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingProfit where 1=2", out OrderPreCostingProfit, false, "1");

                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectMaterial where 1=2", out OrderProcurementCostingDirectMaterial, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectMaterialConsumption where 1=2", out OrderProcurementCostingDirectMaterialConsumption, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectMaterialChild where 1=2", out OrderProcurementCostingDirectMaterialChild, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingOperation where 1=2", out OrderProcurementCostingOperation, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectProcess where 1=2", out OrderProcurementCostingDirectProcess, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingValueLoss where 1=2", out OrderProcurementCostingValueLoss, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingSalesExpense where 1=2", out OrderProcurementCostingSalesExpense, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingProfit where 1=2", out OrderProcurementCostingProfit, false, "1");


                DataTable CostingMasterTemplate = _sqlRepository.GetDataTable("select * from [dbo].[CostingMasterTemplate] WHERE Id='" + SourceId + "'");
                DataTable CostingDetailTemplate = _sqlRepository.GetDataTable("select * from [dbo].[CostingDetailTemplate] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectMaterial = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingDirectMaterial] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectMaterialConsumption = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingDirectMaterialConsumption] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectMaterialChild = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingDirectMaterialChild] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingOperation = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingOperation] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectProcess = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingDirectProcess] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingValueLoss = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingValueLoss] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingSalesExpense = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingSalesExpense] WHERE CostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingProfit = _sqlRepository.GetDataTable("select * from [dbo].[PreCostingProfit] WHERE CostingMasterTemplateId='" + SourceId + "'");



                NewId = GetPK(TableName);
                CopyDataTable(CostingMasterTemplate, OrderCostingMasterTemplate.Tables[0], "");
                CopyDataTable(CostingDetailTemplate, OrderCostingDetailTemplate.Tables[0], NewId);
                CopyDataTable(PreCostingDirectMaterial, OrderPreCostingDirectMaterial.Tables[0], NewId);
                CopyDataTable(PreCostingDirectMaterialConsumption, OrderPreCostingDirectMaterialConsumption.Tables[0], NewId);
                CopyDataTable(PreCostingDirectMaterialChild, OrderPreCostingDirectMaterialChild.Tables[0], NewId);
                CopyDataTable(PreCostingOperation, OrderPreCostingOperation.Tables[0], NewId);
                CopyDataTable(PreCostingDirectProcess, OrderPreCostingDirectProcess.Tables[0], NewId);
                CopyDataTable(PreCostingValueLoss, OrderPreCostingValueLoss.Tables[0], NewId);
                CopyDataTable(PreCostingSalesExpense, OrderPreCostingSalesExpense.Tables[0], NewId);
                CopyDataTable(PreCostingProfit, OrderPreCostingProfit.Tables[0], NewId);

                CopyDataTable(PreCostingDirectMaterial, OrderProcurementCostingDirectMaterial.Tables[0], NewId);
                CopyDataTable(PreCostingDirectMaterialConsumption, OrderProcurementCostingDirectMaterialConsumption.Tables[0], NewId);
                CopyDataTable(PreCostingDirectMaterialChild, OrderProcurementCostingDirectMaterialChild.Tables[0], NewId);
                CopyDataTable(PreCostingOperation, OrderProcurementCostingOperation.Tables[0], NewId);
                CopyDataTable(PreCostingDirectProcess, OrderProcurementCostingDirectProcess.Tables[0], NewId);
                CopyDataTable(PreCostingValueLoss, OrderProcurementCostingValueLoss.Tables[0], NewId);
                CopyDataTable(PreCostingSalesExpense, OrderProcurementCostingSalesExpense.Tables[0], NewId);
                CopyDataTable(PreCostingProfit, OrderProcurementCostingProfit.Tables[0], NewId);


                OrderCostingMasterTemplate.Tables[0].Rows[0]["CostingMasterTemplateId"] = SourceId;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["Id"] = NewId;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["Code"] = CopyData["Code"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["UserName"] = CopyData["UserName"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ShortName"] = CopyData["ShortName"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["StandardName"] = CopyData["StandardName"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["CostingStage"] = CopyData["CostingStage"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProductMasterId"] = CopyData["ProductMasterId"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["InquiryItemId"] = CopyData["InquiryItemId"];
                //OrderCostingMasterTemplate.Tables[0].Rows[0]["MasterOrderItemId"] = CopyData["MasterOrderItemId"];
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PlantId"] = identity.PlantId;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["Version"] = "1";
                OrderCostingMasterTemplate.Tables[0].Rows[0]["isDirectApproval"] = false;

                OrderCostingMasterTemplate.Tables[0].Rows[0]["QuickCostingCheckStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PreCostingCheckStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProcurementCostingCheckStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["QuickCostingCheckRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PreCostingCheckRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProcurementCostingCheckRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["QuickCostingApprovalStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PreCostingApprovalStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProcurementCostingApprovalStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["QuickCostingApprovalRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PreCostingApprovalRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProcurementCostingApprovalRemarks"] = DBNull.Value;

                SetForeignKey(OrderCostingDetailTemplate, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectMaterial, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectMaterialConsumption, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectMaterialChild, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingOperation, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectProcess, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingValueLoss, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingSalesExpense, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingProfit, "OrderCostingMasterTemplateId", NewId);
                for (int i = 0; i < OrderPreCostingDirectMaterial.Tables[0].Rows.Count; i++)
                {
                    OrderPreCostingDirectMaterialConsumption.Tables[0].DefaultView.RowFilter = "CostingItemId='" + OrderPreCostingDirectMaterial.Tables[0].Rows[i]["CostingItemId"].ToString() + "'";
                    for (int k = 0; k < OrderPreCostingDirectMaterialConsumption.Tables[0].DefaultView.Count; k++)
                        OrderPreCostingDirectMaterialConsumption.Tables[0].DefaultView[k]["OrderPreCostingDirectMaterialId"] = OrderPreCostingDirectMaterial.Tables[0].Rows[i]["Id"];

                    OrderPreCostingDirectMaterialChild.Tables[0].DefaultView.RowFilter = "ParentCostingItemId='" + OrderPreCostingDirectMaterial.Tables[0].Rows[i]["CostingItemId"].ToString() + "'";
                    for (int k = 0; k < OrderPreCostingDirectMaterialChild.Tables[0].DefaultView.Count; k++)
                        OrderPreCostingDirectMaterialChild.Tables[0].DefaultView[k]["OrderPreCostingDirectMaterialId"] = OrderPreCostingDirectMaterial.Tables[0].Rows[i]["Id"];

                }

                SetForeignKey(OrderProcurementCostingDirectMaterial, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingDirectMaterialConsumption, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingDirectMaterialChild, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingOperation, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingDirectProcess, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingValueLoss, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingSalesExpense, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingProfit, "OrderCostingMasterTemplateId", NewId);
                for (int i = 0; i < OrderProcurementCostingDirectMaterial.Tables[0].Rows.Count; i++)
                {
                    OrderProcurementCostingDirectMaterialConsumption.Tables[0].DefaultView.RowFilter = "CostingItemId='" + OrderProcurementCostingDirectMaterial.Tables[0].Rows[i]["CostingItemId"].ToString() + "'";
                    for (int k = 0; k < OrderProcurementCostingDirectMaterialConsumption.Tables[0].DefaultView.Count; k++)
                        OrderProcurementCostingDirectMaterialConsumption.Tables[0].DefaultView[k]["OrderProcurementCostingDirectMaterialId"] = OrderProcurementCostingDirectMaterial.Tables[0].Rows[i]["Id"];

                    OrderProcurementCostingDirectMaterialChild.Tables[0].DefaultView.RowFilter = "ParentCostingItemId='" + OrderProcurementCostingDirectMaterial.Tables[0].Rows[i]["CostingItemId"].ToString() + "'";
                    for (int k = 0; k < OrderProcurementCostingDirectMaterialChild.Tables[0].DefaultView.Count; k++)
                        OrderProcurementCostingDirectMaterialChild.Tables[0].DefaultView[k]["OrderProcurementCostingDirectMaterialId"] = OrderProcurementCostingDirectMaterial.Tables[0].Rows[i]["Id"];
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(OrderCostingMasterTemplate, OrderCostingDetailTemplate,
                    OrderPreCostingDirectMaterial, OrderPreCostingDirectMaterialConsumption, OrderPreCostingDirectMaterialChild, OrderPreCostingOperation, OrderPreCostingDirectProcess, OrderPreCostingValueLoss, OrderPreCostingSalesExpense, OrderPreCostingProfit,
                    OrderProcurementCostingDirectMaterial, OrderProcurementCostingDirectMaterialConsumption, OrderProcurementCostingDirectMaterialChild, OrderProcurementCostingOperation, OrderProcurementCostingDirectProcess, OrderProcurementCostingValueLoss, OrderProcurementCostingSalesExpense, OrderProcurementCostingProfit);

                try
                {

                    UpdateSalesOrders(NewId, SalesOrderList);
                }
                catch (Exception ex)
                {
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Id = NewId, Message = "Template copied successfully" });
        }


        public ActionResult CopyOrderCosting(Dictionary<string, object> CopyData, List<Dictionary<string, object>> SalesOrderList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string NewId = "";
            try
            {
                DataSet OrderCostingMasterTemplate
                , OrderCostingDetailTemplate
                , OrderPreCostingDirectMaterial
                , OrderPreCostingDirectMaterialConsumption
                , OrderPreCostingDirectMaterialChild
                , OrderPreCostingOperation
                , OrderPreCostingDirectProcess
                , OrderPreCostingValueLoss
                , OrderPreCostingSalesExpense
                , OrderPreCostingProfit


                , OrderProcurementCostingDirectMaterial
                , OrderProcurementCostingDirectMaterialConsumption
                , OrderProcurementCostingDirectMaterialChild
                , OrderProcurementCostingOperation
                , OrderProcurementCostingDirectProcess
                , OrderProcurementCostingValueLoss
                , OrderProcurementCostingSalesExpense
                , OrderProcurementCostingProfit;

                string SourceId = CopyData["CostingMasterTemplateId"].ToString();

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + CopyData["Code"].ToString() + "'", out OrderCostingMasterTemplate, false, "1");
                if (OrderCostingMasterTemplate.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + CopyData["UserName"].ToString() + "'", out OrderCostingMasterTemplate, false, "1");
                if (OrderCostingMasterTemplate.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists");

                con.OpenDataSetThroughAdapter("select * from OrderCostingDetailTemplate where 1=2", out OrderCostingDetailTemplate, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectMaterial where 1=2", out OrderPreCostingDirectMaterial, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectMaterialConsumption where 1=2", out OrderPreCostingDirectMaterialConsumption, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectMaterialChild where 1=2", out OrderPreCostingDirectMaterialChild, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingOperation where 1=2", out OrderPreCostingOperation, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectProcess where 1=2", out OrderPreCostingDirectProcess, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingValueLoss where 1=2", out OrderPreCostingValueLoss, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingSalesExpense where 1=2", out OrderPreCostingSalesExpense, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingProfit where 1=2", out OrderPreCostingProfit, false, "1");

                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectMaterial where 1=2", out OrderProcurementCostingDirectMaterial, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectMaterialConsumption where 1=2", out OrderProcurementCostingDirectMaterialConsumption, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectMaterialChild where 1=2", out OrderProcurementCostingDirectMaterialChild, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingOperation where 1=2", out OrderProcurementCostingOperation, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectProcess where 1=2", out OrderProcurementCostingDirectProcess, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingValueLoss where 1=2", out OrderProcurementCostingValueLoss, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingSalesExpense where 1=2", out OrderProcurementCostingSalesExpense, false, "1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingProfit where 1=2", out OrderProcurementCostingProfit, false, "1");


                DataTable CostingMasterTemplate = _sqlRepository.GetDataTable("select * from " + TableName + " WHERE Id='" + SourceId + "'");
                DataTable CostingDetailTemplate = _sqlRepository.GetDataTable("select * from [dbo].[OrderCostingDetailTemplate] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectMaterial = _sqlRepository.GetDataTable("select * from [dbo].[OrderPreCostingDirectMaterial] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectMaterialConsumption = _sqlRepository.GetDataTable("select * from [dbo].[OrderPreCostingDirectMaterialConsumption] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectMaterialChild = _sqlRepository.GetDataTable("select * from [dbo].[OrderPreCostingDirectMaterialChild] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingOperation = _sqlRepository.GetDataTable("select * from [dbo].[OrderPreCostingOperation] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingDirectProcess = _sqlRepository.GetDataTable("select * from [dbo].[OrderPreCostingDirectProcess] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingValueLoss = _sqlRepository.GetDataTable("select * from [dbo].[OrderPreCostingValueLoss] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingSalesExpense = _sqlRepository.GetDataTable("select * from [dbo].[OrderPreCostingSalesExpense] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable PreCostingProfit = _sqlRepository.GetDataTable("select * from [dbo].[OrderPreCostingProfit] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");

                DataTable OrderCostingDirectMaterial = _sqlRepository.GetDataTable("select * from [dbo].[OrderProcurementCostingDirectMaterial] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable OrderCostingDirectMaterialConsumption = _sqlRepository.GetDataTable("select * from [dbo].[OrderProcurementCostingDirectMaterialConsumption] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable OrderCostingDirectMaterialChild = _sqlRepository.GetDataTable("select * from [dbo].[OrderProcurementCostingDirectMaterialChild] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable OrderCostingOperation = _sqlRepository.GetDataTable("select * from [dbo].[OrderProcurementCostingOperation] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable OrderCostingDirectProcess = _sqlRepository.GetDataTable("select * from [dbo].[OrderProcurementCostingDirectProcess] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable OrderCostingValueLoss = _sqlRepository.GetDataTable("select * from [dbo].[OrderProcurementCostingValueLoss] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable OrderCostingSalesExpense = _sqlRepository.GetDataTable("select * from [dbo].[OrderProcurementCostingSalesExpense] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");
                DataTable OrderCostingProfit = _sqlRepository.GetDataTable("select * from [dbo].[OrderProcurementCostingProfit] WHERE OrderCostingMasterTemplateId='" + SourceId + "'");


                NewId = GetPK(TableName);
                CopyDataTable(CostingMasterTemplate, OrderCostingMasterTemplate.Tables[0], "");
                CopyDataTable(CostingDetailTemplate, OrderCostingDetailTemplate.Tables[0], NewId);
                CopyDataTable(PreCostingDirectMaterial, OrderPreCostingDirectMaterial.Tables[0], NewId);
                CopyDataTable(PreCostingDirectMaterialConsumption, OrderPreCostingDirectMaterialConsumption.Tables[0], NewId);
                CopyDataTable(PreCostingDirectMaterialChild, OrderPreCostingDirectMaterialChild.Tables[0], NewId);
                CopyDataTable(PreCostingOperation, OrderPreCostingOperation.Tables[0], NewId);
                CopyDataTable(PreCostingDirectProcess, OrderPreCostingDirectProcess.Tables[0], NewId);
                CopyDataTable(PreCostingValueLoss, OrderPreCostingValueLoss.Tables[0], NewId);
                CopyDataTable(PreCostingSalesExpense, OrderPreCostingSalesExpense.Tables[0], NewId);
                CopyDataTable(PreCostingProfit, OrderPreCostingProfit.Tables[0], NewId);

                CopyDataTable(OrderCostingDirectMaterial, OrderProcurementCostingDirectMaterial.Tables[0], NewId);
                CopyDataTable(OrderCostingDirectMaterialConsumption, OrderProcurementCostingDirectMaterialConsumption.Tables[0], NewId);
                CopyDataTable(OrderCostingDirectMaterialChild, OrderProcurementCostingDirectMaterialChild.Tables[0], NewId);
                CopyDataTable(OrderCostingOperation, OrderProcurementCostingOperation.Tables[0], NewId);
                CopyDataTable(OrderCostingDirectProcess, OrderProcurementCostingDirectProcess.Tables[0], NewId);
                CopyDataTable(OrderCostingValueLoss, OrderProcurementCostingValueLoss.Tables[0], NewId);
                CopyDataTable(OrderCostingSalesExpense, OrderProcurementCostingSalesExpense.Tables[0], NewId);
                CopyDataTable(OrderCostingProfit, OrderProcurementCostingProfit.Tables[0], NewId);


                OrderCostingMasterTemplate.Tables[0].Rows[0]["OrderCostingMasterTemplateId"] = SourceId;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["Id"] = NewId;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["Code"] = CopyData["Code"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["UserName"] = CopyData["UserName"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ShortName"] = CopyData["ShortName"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["StandardName"] = CopyData["StandardName"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["CostingStage"] = CopyData["CostingStage"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProductMasterId"] = CopyData["ProductMasterId"].ToString();
                OrderCostingMasterTemplate.Tables[0].Rows[0]["InquiryItemId"] = CopyData["InquiryItemId"];
                //OrderCostingMasterTemplate.Tables[0].Rows[0]["MasterOrderItemId"] = CopyData["MasterOrderItemId"];
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PlantId"] = identity.PlantId;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["Version"] = "1";

                OrderCostingMasterTemplate.Tables[0].Rows[0]["QuickCostingCheckStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PreCostingCheckStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProcurementCostingCheckStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["QuickCostingCheckRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PreCostingCheckRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProcurementCostingCheckRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["QuickCostingApprovalStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PreCostingApprovalStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProcurementCostingApprovalStatus"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["QuickCostingApprovalRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["PreCostingApprovalRemarks"] = DBNull.Value;
                OrderCostingMasterTemplate.Tables[0].Rows[0]["ProcurementCostingApprovalRemarks"] = DBNull.Value;


                SetForeignKey(OrderCostingDetailTemplate, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectMaterial, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectMaterialConsumption, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectMaterialChild, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingOperation, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingDirectProcess, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingValueLoss, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingSalesExpense, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderPreCostingProfit, "OrderCostingMasterTemplateId", NewId);
                for (int i = 0; i < OrderPreCostingDirectMaterial.Tables[0].Rows.Count; i++)
                {
                    OrderPreCostingDirectMaterialConsumption.Tables[0].DefaultView.RowFilter = "CostingItemId='" + OrderPreCostingDirectMaterial.Tables[0].Rows[i]["CostingItemId"].ToString() + "'";
                    for (int k = 0; k < OrderPreCostingDirectMaterialConsumption.Tables[0].DefaultView.Count; k++)
                        OrderPreCostingDirectMaterialConsumption.Tables[0].DefaultView[k]["OrderPreCostingDirectMaterialId"] = OrderPreCostingDirectMaterial.Tables[0].Rows[i]["Id"];

                    OrderPreCostingDirectMaterialChild.Tables[0].DefaultView.RowFilter = "ParentCostingItemId='" + OrderPreCostingDirectMaterial.Tables[0].Rows[i]["CostingItemId"].ToString() + "'";
                    for (int k = 0; k < OrderPreCostingDirectMaterialChild.Tables[0].DefaultView.Count; k++)
                        OrderPreCostingDirectMaterialChild.Tables[0].DefaultView[k]["OrderPreCostingDirectMaterialId"] = OrderPreCostingDirectMaterial.Tables[0].Rows[i]["Id"];
                }

                SetForeignKey(OrderProcurementCostingDirectMaterial, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingDirectMaterialConsumption, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingDirectMaterialChild, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingOperation, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingDirectProcess, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingValueLoss, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingSalesExpense, "OrderCostingMasterTemplateId", NewId);
                SetForeignKey(OrderProcurementCostingProfit, "OrderCostingMasterTemplateId", NewId);
                for (int i = 0; i < OrderProcurementCostingDirectMaterial.Tables[0].Rows.Count; i++)
                {
                    OrderProcurementCostingDirectMaterialConsumption.Tables[0].DefaultView.RowFilter = "CostingItemId='" + OrderProcurementCostingDirectMaterial.Tables[0].Rows[i]["CostingItemId"].ToString() + "'";
                    for (int k = 0; k < OrderProcurementCostingDirectMaterialConsumption.Tables[0].DefaultView.Count; k++)
                        OrderProcurementCostingDirectMaterialConsumption.Tables[0].DefaultView[k]["OrderProcurementCostingDirectMaterialId"] = OrderProcurementCostingDirectMaterial.Tables[0].Rows[i]["Id"];

                    OrderProcurementCostingDirectMaterialChild.Tables[0].DefaultView.RowFilter = "ParentCostingItemId='" + OrderProcurementCostingDirectMaterial.Tables[0].Rows[i]["CostingItemId"].ToString() + "'";
                    for (int k = 0; k < OrderProcurementCostingDirectMaterialChild.Tables[0].DefaultView.Count; k++)
                        OrderProcurementCostingDirectMaterialChild.Tables[0].DefaultView[k]["OrderProcurementCostingDirectMaterialId"] = OrderProcurementCostingDirectMaterial.Tables[0].Rows[i]["Id"];
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(OrderCostingMasterTemplate, OrderCostingDetailTemplate,
                    OrderPreCostingDirectMaterial, OrderPreCostingDirectMaterialConsumption, OrderPreCostingDirectMaterialChild, OrderPreCostingOperation, OrderPreCostingDirectProcess, OrderPreCostingValueLoss, OrderPreCostingSalesExpense, OrderPreCostingProfit,
                    OrderProcurementCostingDirectMaterial, OrderProcurementCostingDirectMaterialConsumption, OrderProcurementCostingDirectMaterialChild, OrderProcurementCostingOperation, OrderProcurementCostingDirectProcess, OrderProcurementCostingValueLoss, OrderProcurementCostingSalesExpense, OrderProcurementCostingProfit);


                try
                {

                    UpdateSalesOrders(NewId, SalesOrderList);
                }
                catch (Exception ex)
                {
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Id = NewId, Message = "Template copied successfully" });
        }
        private void SetForeignKey(DataSet ds, string ColumnName, string KeyValue)
        {
            foreach (DataRow drSource in ds.Tables[0].Rows)
            {
                drSource[ColumnName] = KeyValue;

            }
        }
        private void CopyDataTable(DataTable dtSource, DataTable dtDestination, string PK)
        {
            int Index = 0;
            foreach (DataRow drSource in dtSource.Rows)
            {
                Index++;
                DataRow drDestination = dtDestination.NewRow();
                CopyRow(drSource, ref drDestination);
                if (PK != "")
                    drDestination["Id"] = PK + Index;
                dtDestination.Rows.Add(drDestination);
            }
        }
        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }

        [HttpPost, Authorize]
        public ActionResult SaveOrderProcurementCostingDirectMaterial(IEnumerable<OrderProcurementCostingDirectMaterial> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderProcurementCostingDirectMaterial] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Consumption > 0 && item.Rate > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DM" + GetPK("OrderProcurementCostingDirectMaterial");
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["Consumption"] = item.Consumption;
                            dr["UOM"] = item.UOM;
                            dr["Rate"] = item.Rate;
                            dr["ValueLoss"] = item.ValueLoss;
                            dr["GrossConsumption"] = item.GrossConsumption;
                            dr["GrossAmount"] = item.GrossAmount;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["SourcingType"] = item.SourcingType;
                            dr["Usage"] = item.Usage;
                            dr["POCriteria"] = item.POCriteria;
                            dr["IsUDApplicable"] = item.IsUDApplicable;
                            dr["IsGeneric"] = item.IsGeneric;
                            dr["IsMandatory"] = item.IsMandatory;
                            dr["MaterialMasterId"] = item.MaterialMasterId;
                            dr["ArticleId"] = item.ArticleId;
                            dr["VendorId"] = item.VendorId;


                            dr["ProcurementLevel"] = item.ProcurementLevel;
                            dr["BOQDays"] = item.BOQDays;
                            dr["BOQCriteria"] = item.BOQCriteria;
                            dr["DependentDate"] = item.DependentDate;

                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["Particulars"] = item.Particulars;
                            dr["Remarks"] = item.Remarks;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;




                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();


                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["Consumption"] = item.Consumption;
                            dr["UOM"] = item.UOM;
                            dr["Rate"] = item.Rate;
                            dr["ValueLoss"] = item.ValueLoss;
                            dr["GrossConsumption"] = item.GrossConsumption;

                            dr["GrossAmount"] = item.GrossAmount;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["SourcingType"] = item.SourcingType;
                            dr["Usage"] = item.Usage;
                            dr["POCriteria"] = item.POCriteria;

                            dr["IsUDApplicable"] = item.IsUDApplicable;
                            dr["IsGeneric"] = item.IsGeneric;
                            dr["IsMandatory"] = item.IsMandatory;
                            dr["MaterialMasterId"] = item.MaterialMasterId;
                            dr["ArticleId"] = item.ArticleId;
                            dr["VendorId"] = item.VendorId;

                            dr["ProcurementLevel"] = item.ProcurementLevel;
                            dr["BOQDays"] = item.BOQDays;
                            dr["BOQCriteria"] = item.BOQCriteria;
                            dr["DependentDate"] = item.DependentDate;

                            dr["MinimumOfQuantity"] = item.MinimumOfQuantity;
                            dr["POIssueDeadLine"] = item.POIssueDeadLine;
                            dr["Particulars"] = item.Particulars;
                            dr["Remarks"] = item.Remarks;
                            dr["PurchaseGroupId"] = item.PurchaseGroupId;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateProcurementValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost, Authorize]
        public ActionResult SaveProcurementCostingOperation(IEnumerable<OrderProcurementCostingOperation> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderProcurementCostingOperation] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "CO" + GetPK("OrderProcurementCostingOperation");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Value"] = item.Value;
                            dr["Description"] = item.Description;


                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                            dr["Value"] = item.Value;
                            dr["Description"] = item.Description;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }

                RecalculateProcurementValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost, Authorize]
        public ActionResult SaveProcurementCostingDirectProcess(IEnumerable<OrderProcurementCostingDirectProcess> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderProcurementCostingDirectProcess] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Amount > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DP" + GetPK("OrderProcurementCostingDirectProcess");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["ExecutionType"] = item.ExecutionType;
                            dr["Value"] = item.Value;
                            dr["Rate"] = item.Rate;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["ExecutionType"] = item.ExecutionType;
                            dr["Value"] = item.Value;
                            dr["Rate"] = item.Rate;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dr.EndEdit();
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateProcurementValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost, Authorize]
        public ActionResult SaveProcurementCostingSalesExpense(IEnumerable<OrderProcurementCostingSalesExpense> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderProcurementCostingSalesExpense] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DP" + GetPK("OrderProcurementCostingSalesExpense");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();


                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dr.EndEdit();
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);


                }
                RecalculateProcurementValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost, Authorize]
        public ActionResult SaveProcurementCostingValueLoss(IEnumerable<OrderProcurementCostingValueLoss> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderProcurementCostingValueLoss] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "DP" + GetPK("OrderProcurementCostingSalesExpense");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();


                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateProcurementValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost, Authorize]
        public ActionResult SaveProcurementCostingProfit(IEnumerable<OrderProcurementCostingProfit> data, string OrderCostingMasterTemplateId)
        {
            DataSet dsMaster = null;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    string _id = string.Empty;


                    string CostingItemIds = "''";
                    foreach (var item in data)
                        CostingItemIds += ",'" + item.CostingItemId + "'";

                    string sql = "SELECT * FROM [dbo].[OrderProcurementCostingProfit] WHERE CostingItemId IN (" + CostingItemIds + ") AND OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId='" + item.CostingItemId + "'";

                        //if (item.Value > 0)
                        //{

                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            _id = "P" + GetPK("OrderProcurementCostingProfit");
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = _id;
                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();


                            dr["CostingItemId"] = item.CostingItemId;
                            dr["Sequence"] = item.Sequence;
                            dr["OrderCostingMasterTemplateId"] = OrderCostingMasterTemplateId;
                            dr["ResponsiblePersonId"] = item.ResponsiblePersonId;

                            dr["Type"] = item.Type;
                            dr["Value"] = item.Value;
                            dr["Amount"] = item.Amount;
                            dr["Description"] = item.Description;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }

                        //}
                        //else
                        //{

                        //    while (dsMaster.Tables[0].DefaultView.Count > 0)
                        //        dsMaster.Tables[0].DefaultView[0].Delete();

                        //}


                    }



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                RecalculateProcurementValues(OrderCostingMasterTemplateId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost, Authorize]
        public ActionResult UploadAttachment(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\\", "");
                DataTable AdditionalData = CustomJsonResult.ToDataTable(UploadDefault_data);

                //var settings = new JsonSerializerSettings
                //{
                //    NullValueHandling = NullValueHandling.Ignore,
                //    MissingMemberHandling = MissingMemberHandling.Ignore
                //};
                //List<Dictionary<string, string>> AdditionalData.Rows[0]1 = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(UploadDefault_data, settings);

                //Dictionary<string, string> AdditionalData.Rows[0] = JsonConvert.DeserializeObject<Dictionary<string, string>>(UploadDefault_data, settings);


                AdditionalData.Rows[0]["Id"] = AdditionalData.Rows[0]["Id"].ToString().Replace("\"", "");
                if (string.IsNullOrEmpty(AdditionalData.Rows[0]["Id"].ToString()))
                    throw new Exception("Save the item first");



                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                foreach (var file in UploadDefault)
                {

                    string _Id = AdditionalData.Rows[0]["TableName"].ToString() + AdditionalData.Rows[0]["Id"].ToString();

                    var fileName = Path.GetFileName(_Id + new FileInfo(file.FileName).Extension);
                    var destinationPath = Path.Combine(ResourcesPathReader.GetOrderCostingPath(), _Id + new FileInfo(file.FileName).Extension);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetOrderCostingPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetOrderCostingPath());
                        }
                        catch (Exception ex)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from " + AdditionalData.Rows[0]["TableName"] + " where Id='" + AdditionalData.Rows[0]["Id"].ToString() + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();




                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        #region Task data update
                        if (dsLocal.Tables[0].Rows[0]["FileName"].ToString() != "")
                        {
                            //try to delete the existing file
                            try
                            {
                                var _Path = Path.Combine(ResourcesPathReader.GetToDoPath(), dsLocal.Tables[0].Rows[0]["FileName"].ToString());
                                if (System.IO.File.Exists(_Path))
                                    System.IO.File.Delete(_Path);
                            }
                            catch (Exception)
                            {

                            }

                        }

                        DataRow dr = dsLocal.Tables[0].Rows[0];

                        dr.BeginEdit();

                        dr["FileName"] = fileName;
                        dr["FileOriginalName"] = file.FileName;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();


                        #endregion data update





                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id, string TableName)
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("select * from " + TableName + "  where Id='" + Id + "'"), JsonRequestBehavior.AllowGet);



            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult DeleteFile(string Id, string TableName)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + "  where Id='" + Id + "'", out dsMaster, false, "1");



                var destinationPath = Path.Combine(ResourcesPathReader.GetOrderCostingPath(), dsMaster.Tables[0].Rows[0]["FileName"].ToString());
                if (System.IO.File.Exists(destinationPath))
                    System.IO.File.Delete(destinationPath);

                #region Task data update


                DataRow dr = dsMaster.Tables[0].Rows[0];
                dr.BeginEdit();

                dr["FileName"] = DBNull.Value;
                dr["FileOriginalName"] = DBNull.Value;
                dr.EndEdit();


                #endregion data update




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public ActionResult ChangeCostingStage(string TemplateId, string CostingStage)
        {

            try
            {


                string sql = "update OrderCostingMasterTemplate set CostingStage='" + CostingStage + "' Where Id='" + TemplateId + "'";

                _sqlRepository.ExecuteSqlCommand(sql);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Message = "Costing Stage Updated Successfully" });

        }

        [HttpPost, Authorize]
        public ActionResult ChanageVersion(string TemplateId)
        {

            try
            {


                CreateSnapshotAndVersion(TemplateId);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Message = "SO Updated Successfully" });

        }

        private void CreateSnapshotAndVersion(string TemplateId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string NewId = "";
            try
            {
                DataSet OrderCostingMasterTemplate;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from OrderCostingMasterTemplate where Id='" + TemplateId + "'", out OrderCostingMasterTemplate, false, "1");


                string ProductMasterId = OrderCostingMasterTemplate.Tables[0].Rows[0]["ProductMasterId"].ToString();

                string sql = "";

                sql = @" select isnull(d.id,'New') isNewId, case when isnull(d.Id,'')<>'' THEN isnull(TEMPLATE.CostingComponentId,'DELETE') ELSE '' END AS isToBeDeleted,
                         d.Id
                        ,0 as Status
	                    ,d.CostingValue
	                    ,d.BuyerTarget
	                    --,d.CostingVersionMasterTemplateId
                        ,cc.Id as CostingComponentId
	                    ,cc.Code
	                    ,cc.ShortName
	                    ,cc.UserName
                        ,ctc.Sequence
	                    ,cc.StandardName
	                    ,ctc.CostingType
                        ,cc.CostingSegment
                        ,isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount 
						 from hkp.CostingComponent CC
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')
                        left outer join OrderCostingDetailTemplate D on cc.id=d.CostingComponentId and d.OrderCostingMasterTemplateId='" + TemplateId + @"'
                         LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + TemplateId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM OrderCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderCostingPurchaseExpense AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                  )AS ITEMVAL ON  itemval.CostingComponentId=d.CostingComponentId
                        left outer join  (
                        select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')) AS TEMPLATE 
					    on template.CostingComponentId=d.CostingComponentId


                        where   cc.Id IN (
                            select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')

					    UNION

					    select CostingComponentId from OrderCostingDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')='" + TemplateId + @"'

					--union

					--select CostingComponentId from CostingVersionDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')= '" + TemplateId + @"'
                    )  order by isnull(ctc.Sequence,999999),cc.Description";


                sql = @" select isnull(d.id,'New') isNewId, case when isnull(d.Id,'')<>'' THEN isnull(TEMPLATE.CostingComponentId,'DELETE') ELSE '' END AS isToBeDeleted,
                         d.Id
                        ,0 as Status
	                    ,d.CostingValue
	                    ,d.BuyerTarget
	                    --,d.CostingVersionMasterTemplateId
                        ,cc.Id as CostingComponentId
	                    ,cc.Code
	                    ,cc.ShortName
	                    ,cc.UserName
                        ,ctc.Sequence
	                    ,cc.StandardName
	                    ,ctc.CostingType
                        ,cc.CostingSegment
                        ,isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount 
                        ,isnull(itemvalp.TotalGrossAmount,0) AS TotalProcurementGrossAmount 
						 from hkp.CostingComponent CC
                        left outer join [dbo].[CostingTypeComponent] AS ctc  ON cc.Id = ctc.CostingComponentId and ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')
                        left outer join OrderCostingDetailTemplate D on cc.id=d.CostingComponentId and d.OrderCostingMasterTemplateId='" + TemplateId + @"'
                         LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderPreCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + TemplateId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM OrderPreCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderPreCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                  )AS ITEMVAL ON  itemval.CostingComponentId=d.CostingComponentId
                        LEFT OUTER JOIN ( SELECT i.CostingComponentId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=    '" + TemplateId + @"' GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingDirectProcess AS pc   INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.[Value]) AS TotalGrossAmount FROM OrderProcurementCostingOperation AS pc       INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId='" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingSalesExpense AS pc    INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingValueLoss AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                            UNION ALL SELECT i.CostingComponentId,SUM(pc.Amount)AS TotalGrossAmount FROM OrderProcurementCostingProfit AS pc INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId and pc.OrderCostingMasterTemplateId=  '" + TemplateId + @"'	GROUP BY i.CostingComponentId
                                  )AS ITEMVALP ON  ITEMVALP.CostingComponentId=d.CostingComponentId
                        left outer join  (
                        select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')) AS TEMPLATE 
					    on template.CostingComponentId=d.CostingComponentId


                        where   cc.Id IN (
                            select ctc.CostingComponentId FROM [dbo].[CostingTypeComponent] AS ctc
                        inner JOIN [HKP].[CostingComponent] AS cc ON cc.Id = ctc.CostingComponentId
                        WHERE ctc.CostingType = (SELECT CostingType FROM MST.ProductMaster WHERE Id = '" + ProductMasterId + @"')

					    UNION

					    select CostingComponentId from OrderCostingDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')='" + TemplateId + @"'

					--union

					--select CostingComponentId from CostingVersionDetailTemplate where  ISNULL(OrderCostingMasterTemplateId,'')= '" + TemplateId + @"'
                    )  order by isnull(ctc.Sequence,999999),cc.Description";
                DataTable dtBackupDetail = _sqlRepository.GetDataTable(sql);

                NewId = "B" + GetPK("OrderCostingVersionMasterTemplate");
                DataSet OrderCostingVersionMasterTemplate, OrderCostingVersionDetailTemplate;
                con.OpenDataSetThroughAdapter("SELECT* FROM OrderCostingVersionMasterTemplate AS cvmt where 1=2", out OrderCostingVersionMasterTemplate, false, "1");
                con.OpenDataSetThroughAdapter("SELECT* FROM OrderCostingVersionDetailTemplate AS cvmt where 1=2", out OrderCostingVersionDetailTemplate, false, "1");


                DataRow dr = OrderCostingVersionMasterTemplate.Tables[0].NewRow();
                dr["Id"] = NewId;
                dr["OrderCostingMasterTemplateId"] = TemplateId;
                dr["Version"] = OrderCostingMasterTemplate.Tables[0].Rows[0]["Version"];
                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = DateTime.Now;
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedFromIP"] = identity.IPAddress;
                dr["UpdatedDate"] = DateTime.Now;
                OrderCostingVersionMasterTemplate.Tables[0].Rows.Add(dr);


                for (int i = 0; i < dtBackupDetail.Rows.Count; i++)
                {
                    dr = OrderCostingVersionDetailTemplate.Tables[0].NewRow();
                    dr["Id"] = NewId + (i + 1).ToString();
                    dr["OrderCostingVersionMasterTemplateId"] = NewId;

                    dr["CostingComponentId"] = dtBackupDetail.Rows[i]["CostingComponentId"];
                    dr["Sequence"] = dtBackupDetail.Rows[i]["Sequence"];

                    dr["BuyerTarget"] = clsStaticInfo.dbl(dtBackupDetail.Rows[i]["BuyerTarget"].ToString());
                    dr["CostingValue"] = clsStaticInfo.dbl(dtBackupDetail.Rows[i]["CostingValue"].ToString());
                    dr["PreCostingValue"] = clsStaticInfo.dbl(dtBackupDetail.Rows[i]["TotalGrossAmount"].ToString());
                    dr["ProcurementCostingValue"] = clsStaticInfo.dbl(dtBackupDetail.Rows[i]["TotalProcurementGrossAmount"].ToString());


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = DateTime.Now;
                    OrderCostingVersionDetailTemplate.Tables[0].Rows.Add(dr);
                }

                OrderCostingMasterTemplate.Tables[0].Rows[0]["Version"] = clsStaticInfo.dbl(OrderCostingMasterTemplate.Tables[0].Rows[0]["Version"].ToString()) + 1;

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(OrderCostingMasterTemplate, OrderCostingVersionMasterTemplate, OrderCostingVersionDetailTemplate);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #region --update part by saad --

        [HttpPost, Authorize]
        public ActionResult GetHeightData(string PreCostMaterialId, string ParameterName)
        {
            string sql = @"select Id,AreaType,ParameterName,Parameter,Actual,Allowance,(Actual+Allowance)WithAllowance,NoOfParameter,Total from [dbo].[OrderPreCostingDirectMaterialConsumption] where OrderPreCostingDirectMaterialId='" + PreCostMaterialId + "' and ParameterName='" + ParameterName + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetHeightDataPro(string PreCostMaterialId, string ParameterName)
        {
            string sql = @"select Id,AreaType,ParameterName,Parameter,Actual,Allowance,(Actual+Allowance)WithAllowance,NoOfParameter,Total from [dbo].[OrderProcurementCostingDirectMaterialConsumption] where OrderProcurementCostingDirectMaterialId='" + PreCostMaterialId + "' and ParameterName='" + ParameterName + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetDataFromItemCon(string ProductId, string MaterialId)
        {
            string sql = @"select m.Id,m.Description
                            from ItemConsumtionMaster m
                            where m.ProductMasterId='" + ProductId + "' and m.CostingItemId='" + MaterialId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveNewItemConsumptionDataOrderProcurement(string PreCostingDirectMaterialId, string ItemConsumtionId, string CostingMasterTemplateId)
        {
            try
            {
                string _id = string.Empty;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string Consumption = "Select * from OrderProcurementCostingDirectMaterialConsumption where OrderProcurementCostingDirectMaterialId ='" + PreCostingDirectMaterialId + "'";

                string PreCostingDirectMaterial = "Select * from OrderProcurementCostingDirectMaterial where Id ='" + PreCostingDirectMaterialId + "'";

                DataSet dsChild;
                con.OpenDataSetThroughAdapter(" ", out dsChild, false, "1");
                string ConsumptionReference = @"SELECT m.ProductMasterId, m.CostingItemId, m.GSMValue,co.ComponentName,CO.AreaType,CO.NoOfParts,icc.ParameterName,icc.Parameter, icc.Actual, icc.Allowance, icc.Number AS NoOfParameter, icc.Total from ItemConsumtionMaster M
                                               join ItemConsumtionComponent CO ON  m.Id = co.ItemConsumtionMasterId
                                               JOIN ItemConsumtionChild AS icc ON icc.ItemConsumtionComponentId = co.Id AND m.Id = icc.ItemConsumtionMasterId
                                               WHERE m.Id = '" + ItemConsumtionId + "'";

                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(Consumption, out DataSet dsConsumption, false, "1");
                con.OpenDataSetThroughAdapter(PreCostingDirectMaterial, out DataSet dsPreCostingDirectMaterial, false, "1");
                DataTable dtConsumptionReference = _sqlRepository.GetDataTable(ConsumptionReference);



                while (dsConsumption.Tables[0].DefaultView.Count > 0)
                {
                    dsConsumption.Tables[0].DefaultView[0].Delete();
                }

                for (int CONS = 0; CONS < dtConsumptionReference.DefaultView.Count; CONS++)
                {
                    DataRow drConsumption = dsConsumption.Tables[0].NewRow();
                    CopyRow(dtConsumptionReference.DefaultView[CONS].Row, drConsumption);
                    drConsumption["OrderProcurementCostingDirectMaterialId"] = PreCostingDirectMaterialId;
                    drConsumption["OrderCostingMasterTemplateId"] = CostingMasterTemplateId;
                    dsConsumption.Tables[0].Rows.Add(drConsumption);
                }

                //calculate Consumption
                dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"] = CalculateConsumption(dtConsumptionReference.DefaultView.ToTable());


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsConsumption, dsPreCostingDirectMaterial);
                return Json(new { Error = false, Consumption = clsStaticInfo.dbl(dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"].ToString()), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult SaveNewItemConsumptionData(string PreCostingDirectMaterialId, string ItemConsumtionId, string CostingMasterTemplateId)
        {
            try
            {
                string _id = string.Empty;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string Consumption = "Select * from OrderPreCostingDirectMaterialConsumption where OrderPreCostingDirectMaterialId ='" + PreCostingDirectMaterialId + "'";

                string PreCostingDirectMaterial = "Select * from OrderPreCostingDirectMaterial where Id ='" + PreCostingDirectMaterialId + "'";

                DataSet dsChild;
                con.OpenDataSetThroughAdapter(" ", out dsChild, false, "1");
                string ConsumptionReference = @"SELECT m.ProductMasterId, m.CostingItemId, m.GSMValue,co.ComponentName,CO.AreaType,CO.NoOfParts,icc.ParameterName,icc.Parameter, icc.Actual, icc.Allowance, icc.Number AS NoOfParameter, icc.Total from ItemConsumtionMaster M
                                               join ItemConsumtionComponent CO ON  m.Id = co.ItemConsumtionMasterId
                                               JOIN ItemConsumtionChild AS icc ON icc.ItemConsumtionComponentId = co.Id AND m.Id = icc.ItemConsumtionMasterId
                                               WHERE m.Id = '" + ItemConsumtionId + "'";

                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(Consumption, out DataSet dsConsumption, false, "1");
                con.OpenDataSetThroughAdapter(PreCostingDirectMaterial, out DataSet dsPreCostingDirectMaterial, false, "1");
                DataTable dtConsumptionReference = _sqlRepository.GetDataTable(ConsumptionReference);



                while (dsConsumption.Tables[0].DefaultView.Count > 0)
                {
                    dsConsumption.Tables[0].DefaultView[0].Delete();
                }

                for (int CONS = 0; CONS < dtConsumptionReference.DefaultView.Count; CONS++)
                {
                    DataRow drConsumption = dsConsumption.Tables[0].NewRow();
                    CopyRow(dtConsumptionReference.DefaultView[CONS].Row, drConsumption);
                    drConsumption["OrderPreCostingDirectMaterialId"] = PreCostingDirectMaterialId;
                    drConsumption["OrderCostingMasterTemplateId"] = CostingMasterTemplateId;
                    dsConsumption.Tables[0].Rows.Add(drConsumption);
                }

                //calculate Consumption
                dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"] = CalculateConsumption(dtConsumptionReference.DefaultView.ToTable());


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsConsumption, dsPreCostingDirectMaterial);
                return Json(new { Error = false, Consumption = clsStaticInfo.dbl(dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"].ToString()), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveUpdate(string PreCostingDirectMaterialId, List<UpdatedModel> ChildData)
        {
            try
            {
                for (int i = 0; i < ChildData.Count; i++)
                {
                    if (OTSBD.clsStaticInfo.dbl(ChildData[i].Allowance) < 0)
                        throw new Exception("Allowance data cannot be negative");
                    if (OTSBD.clsStaticInfo.dbl(ChildData[i].Actual) <= 0)
                        throw new Exception("Actual data cannot be less or equal zero");

                    var xy = ChildData.Where(parameter => parameter.Parameter == ChildData[i].Parameter).ToList();
                    if (xy.Count > 1)
                    {
                        throw new Exception("Duplicate Parameter");
                    }
                }
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsChild;
                con.OpenDataSetThroughAdapter("select * from [dbo].[OrderPreCostingDirectMaterialConsumption] where  OrderPreCostingDirectMaterialId='" + PreCostingDirectMaterialId + "'", out dsChild, false, "1");
                string PreCostingDirectMaterial = "Select * from OrderPreCostingDirectMaterial where Id ='" + PreCostingDirectMaterialId + "'";
                con.OpenDataSetThroughAdapter(PreCostingDirectMaterial, out DataSet dsPreCostingDirectMaterial, false, "1");
                foreach (var item in ChildData)
                {
                    dsChild.Tables[0].DefaultView.RowFilter = "Id='" + item.Id + "'";
                    if (dsChild.Tables[0].DefaultView.Count == 1)
                    {
                        DataRow dr = dsChild.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["Parameter"] = item.Parameter;
                        dr["Actual"] = item.Actual;
                        dr["Allowance"] = item.Allowance;
                        dr["Parameter"] = item.Parameter;
                        dr["NoOfParameter"] = item.NoOfParameter;
                        dr["Total"] = item.Total;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                }

                dsChild.Tables[0].DefaultView.RowFilter = null;
                dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"] = CalculateConsumption(dsChild.Tables[0]);
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild, dsPreCostingDirectMaterial);

                return Json(new { Error = false, Consumption = clsStaticInfo.dbl(dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"].ToString()), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult SaveUpdateOrderProcurement(string PreCostingDirectMaterialId, List<UpdatedModel> ChildData)
        {
            try
            {
                for (int i = 0; i < ChildData.Count; i++)
                {
                    if (OTSBD.clsStaticInfo.dbl(ChildData[i].Allowance) < 0)
                        throw new Exception("Allowance data cannot be negative");
                    if (OTSBD.clsStaticInfo.dbl(ChildData[i].Actual) <= 0)
                        throw new Exception("Actual data cannot be less or equal zero");

                    var xy = ChildData.Where(parameter => parameter.Parameter == ChildData[i].Parameter).ToList();
                    if (xy.Count > 1)
                    {
                        throw new Exception("Duplicate Parameter");
                    }
                }
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsChild;
                con.OpenDataSetThroughAdapter("select * from [dbo].[OrderProcurementCostingDirectMaterialConsumption] where  OrderProcurementCostingDirectMaterialId='" + PreCostingDirectMaterialId + "'", out dsChild, false, "1");
                string PreCostingDirectMaterial = "Select * from OrderProcurementCostingDirectMaterial where Id ='" + PreCostingDirectMaterialId + "'";
                con.OpenDataSetThroughAdapter(PreCostingDirectMaterial, out DataSet dsPreCostingDirectMaterial, false, "1");
                foreach (var item in ChildData)
                {
                    dsChild.Tables[0].DefaultView.RowFilter = "Id='" + item.Id + "'";
                    if (dsChild.Tables[0].DefaultView.Count == 1)
                    {
                        DataRow dr = dsChild.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["Parameter"] = item.Parameter;
                        dr["Actual"] = item.Actual;
                        dr["Allowance"] = item.Allowance;
                        dr["Parameter"] = item.Parameter;
                        dr["NoOfParameter"] = item.NoOfParameter;
                        dr["Total"] = item.Total;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                }

                dsChild.Tables[0].DefaultView.RowFilter = null;
                dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"] = CalculateConsumption(dsChild.Tables[0]);
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild, dsPreCostingDirectMaterial);

                return Json(new { Error = false, Consumption = clsStaticInfo.dbl(dsPreCostingDirectMaterial.Tables[0].Rows[0]["Consumption"].ToString()), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveSubMaterial(List<Dictionary<string, object>> itemList, Dictionary<string, object> PreCDMaterial)
        {
            try
            {
                if (itemList == null)
                {
                    throw new Exception("Nothing to update");
                }
                DataSet dsMaster; DataRow drMSave; var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity; int count = 0;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "OrderPreCostingDirectMaterialChild", out string seed_detail);
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectMaterialChild where OrderPreCostingDirectMaterialId='" + PreCDMaterial["Id"] + "' ", out dsMaster, false, "1");

                foreach (var item in itemList)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId = '" + item["CostingItemId"] + "' ";

                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                        continue;

                    count++;
                    string pk = "MC" + seed_detail + "_" + count;
                    drMSave = dsMaster.Tables[0].NewRow();
                    drMSave["Id"] = pk;
                    drMSave["OrderPreCostingDirectMaterialId"] = PreCDMaterial["Id"];
                    drMSave["CostingItemId"] = item["CostingItemId"];
                    drMSave["OrderCostingMasterTemplateId"] = item["OrderCostingMasterTemplateId"];
                    drMSave["ParentCostingItemId"] = PreCDMaterial["CostingItemId"];

                    drMSave["Consumption"] = 0;
                    drMSave["Rate"] = 0;
                    drMSave["ValueLoss"] = 0;
                    drMSave["GrossConsumption"] = 0;
                    drMSave["GrossAmount"] = 0;

                    drMSave["AddedBy"] = identity.Name;
                    drMSave["AddedDate"] = DateTime.Now;
                    drMSave["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(drMSave);

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetSubMaterialData(string MasterId)
        {
            string sql = @"SELECT  pcdmc.*,ci.UserName CostingItemName,cmt.StandardName  CostingMasterTemplate,pcdm.Id PCDMCID
                              FROM OrderPreCostingDirectMaterialChild AS pcdmc 
                            LEFT JOIN HKP.CostingItem AS ci ON ci.Id = pcdmc.CostingItemId
                            LEFT JOIN OrderCostingMasterTemplate AS cmt ON cmt.Id = pcdmc.OrderCostingMasterTemplateId
                            LEFT JOIN OrderPreCostingDirectMaterial AS pcdm ON pcdm.Id = pcdmc.OrderPreCostingDirectMaterialId
                            where OrderPreCostingDirectMaterialId ='" + MasterId + "'";
            return Json(new { data = _sqlRepository.GetDataCollection(sql) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult UpdatePreCostingChild(List<Dictionary<string, object>> subMaterilaList, string MasterId)
        {
            try
            {
                DataSet dsMaster; DataRow drMSave; var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from OrderPreCostingDirectMaterialChild where OrderPreCostingDirectMaterialId='" + MasterId + "' ", out dsMaster, false, "1");

                foreach (var item in subMaterilaList)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "Id = '" + item["Id"] + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {
                        drMSave = dsMaster.Tables[0].DefaultView[0].Row;
                        drMSave.BeginEdit();
                        drMSave["Consumption"] = clsStaticInfo.dbl(item["Consumption"]);
                        drMSave["Rate"] = clsStaticInfo.dbl(item["Rate"]);
                        drMSave["ValueLoss"] = clsStaticInfo.dbl(item["ValueLoss"]);
                        drMSave["GrossConsumption"] = clsStaticInfo.dbl(item["GrossConsumption"]);
                        drMSave["GrossAmount"] = clsStaticInfo.dbl(item["GrossAmount"]);

                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = DateTime.Now;
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        drMSave.EndEdit();
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetSubMaterialSelection(string CostingMasterTemplateId, string costingComponentId, string Segment)
        {

            string sql = @"SELECT ci.ShortName,cat.UserName AS CostingCategory, CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END) AS Selected, ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                        o.CostingMasterTemplateId,
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                            ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            LEFT OUTER JOIN hkp.CostingCategory AS cat ON cat.Id=ci.CostingCategoryId
                            LEFT join PreCostingDirectMaterial o on o.CostingItemId = ci.Id AND o.CostingMasterTemplateId='" + CostingMasterTemplateId + @"'
                            WHERE ci.CostingComponentId='" + costingComponentId + @"'
                            ORDER BY CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END), ci.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DeleteSubMaterial(string SubMaterialId)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderPreCostingDirectMaterialChild where id='" + SubMaterialId + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetCostingItemForSubMaterial(string CostingStage, string OrderCostingMasterTemplateId, string costingComponentId, string Segment)
        {
            string TableName = "";
            string aND = "";
            if (CostingStage == "PRE")
            {
                if (Segment == CostingSegment.DirectMaterial.ToString())
                {
                    TableName = "OrderPreCostingDirectMaterial";
                    aND = "AND ci.IsSubMaterial = 1";
                }
                else if (Segment == CostingSegment.DirectProcess.ToString())
                    TableName = "OrderPreCostingDirectProcess";
                else if (Segment == CostingSegment.Operation.ToString())
                    TableName = "OrderPreCostingOperation";
                else if (Segment == CostingSegment.Profit.ToString())
                    TableName = "OrderPreCostingProfit";
                else if (Segment == CostingSegment.SalesExpense.ToString())
                    TableName = "OrderPreCostingSalesExpense";
                else if (Segment == CostingSegment.ValueLoss.ToString())
                    TableName = "OrderPreCostingValueLoss";
            }
            if (CostingStage == "PROCUREMENT")
            {

                if (Segment == CostingSegment.DirectMaterial.ToString())
                {
                    TableName = "OrderProcurementCostingDirectMaterial";
                    aND = "AND ci.IsSubMaterial = 1";
                }
                else if (Segment == CostingSegment.DirectProcess.ToString())
                    TableName = "OrderProcurementCostingDirectProcess";
                else if (Segment == CostingSegment.Operation.ToString())
                    TableName = "OrderProcurementCostingOperation";
                else if (Segment == CostingSegment.Profit.ToString())
                    TableName = "OrderProcurementCostingProfit";
                else if (Segment == CostingSegment.SalesExpense.ToString())
                    TableName = "OrderProcurementCostingSalesExpense";
                else if (Segment == CostingSegment.ValueLoss.ToString())
                    TableName = "OrderProcurementCostingValueLoss";
            }


            string sql = @"SELECT ci.ShortName,cat.UserName AS CostingCategory, CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END) AS Selected, ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
                        o.OrderCostingMasterTemplateId,
                            ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
                            ci.POIssueDeadLine, ci.Wastage,ci.Description
                            from hkp.CostingItem ci 
                            left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
                            LEFT OUTER JOIN hkp.CostingCategory AS cat ON cat.Id=ci.CostingCategoryId
                            LEFT join " + TableName + @" o on o.CostingItemId = ci.Id AND o.OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + @"'
                            WHERE ci.CostingComponentId='" + costingComponentId + @"' " + aND + @"
                            ORDER BY CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END), ci.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetSubMaterialDataPro(string MasterId)
        {
            string sql = @"SELECT  pcdmc.*,ci.UserName CostingItemName,cmt.StandardName  CostingMasterTemplate,pcdm.Id PCDMCID
                              FROM OrderProcurementCostingDirectMaterialChild AS pcdmc 
                            LEFT JOIN HKP.CostingItem AS ci ON ci.Id = pcdmc.CostingItemId
                            LEFT JOIN OrderCostingMasterTemplate AS cmt ON cmt.Id = pcdmc.OrderCostingMasterTemplateId
                            LEFT JOIN OrderProcurementCostingDirectMaterial AS pcdm ON pcdm.Id = pcdmc.OrderProcurementCostingDirectMaterialId
                            where OrderProcurementCostingDirectMaterialId ='" + MasterId + "'";
            return Json(new { data = _sqlRepository.GetDataCollection(sql) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult SaveSubMaterialPro(List<Dictionary<string, object>> itemList, Dictionary<string, object> PreCDMaterial)
        {
            try
            {
                if (itemList == null)
                {
                    throw new Exception("Nothing to update");
                }
                DataSet dsMaster; DataRow drMSave; var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity; int count = 0;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "OrderProcurementCostingDirectMaterialChild", out string seed_detail);
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectMaterialChild where OrderProcurementCostingDirectMaterialId='" + PreCDMaterial["Id"] + "' ", out dsMaster, false, "1");

                foreach (var item in itemList)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "CostingItemId = '" + item["CostingItemId"] + "' ";

                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                        continue;

                    count++;
                    string pk = "OP" + seed_detail + "_" + count;
                    drMSave = dsMaster.Tables[0].NewRow();
                    drMSave["Id"] = pk;
                    drMSave["OrderProcurementCostingDirectMaterialId"] = PreCDMaterial["Id"];
                    drMSave["CostingItemId"] = item["CostingItemId"];
                    drMSave["OrderCostingMasterTemplateId"] = item["OrderCostingMasterTemplateId"];
                    drMSave["ParentCostingItemId"] = PreCDMaterial["CostingItemId"];
                    drMSave["Consumption"] = 0;
                    drMSave["Rate"] = 0;
                    drMSave["ValueLoss"] = 0;
                    drMSave["GrossConsumption"] = 0;
                    drMSave["GrossAmount"] = 0;

                    drMSave["AddedBy"] = identity.Name;
                    drMSave["AddedDate"] = DateTime.Now;
                    drMSave["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(drMSave);

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult UpdatePreCostingChildPro(List<Dictionary<string, object>> subMaterilaList, string MasterId)
        {
            try
            {
                if (subMaterilaList == null)
                    throw new Exception("Nothing to update");

                DataSet dsMaster; DataRow drMSave; var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from OrderProcurementCostingDirectMaterialChild where OrderProcurementCostingDirectMaterialId='" + MasterId + "' ", out dsMaster, false, "1");

                foreach (var item in subMaterilaList)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "Id = '" + item["Id"] + "' ";
                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {
                        drMSave = dsMaster.Tables[0].DefaultView[0].Row;
                        drMSave.BeginEdit();
                        drMSave["Consumption"] = clsStaticInfo.dbl(item["Consumption"]);
                        drMSave["Rate"] = clsStaticInfo.dbl(item["Rate"]);
                        drMSave["ValueLoss"] = clsStaticInfo.dbl(item["ValueLoss"]);
                        drMSave["GrossConsumption"] = clsStaticInfo.dbl(item["GrossConsumption"]);
                        drMSave["GrossAmount"] = clsStaticInfo.dbl(item["GrossAmount"]);

                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = DateTime.Now;
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        drMSave.EndEdit();
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult DeleteSubMaterialPro(string SubMaterialId)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from OrderProcurementCostingDirectMaterialChild where id='" + SubMaterialId + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        [Authorize, HttpGet]
        public JsonResult GetApprovedBY()
        {
            string sql = @"SELECT E.SystemId As Value, E.EmployeeName As Text, A.ActionStatus from dbo.AuthorizationConfig A 
                          INNER JOIN dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='OrderCostingApproveBy' AND E.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeletePreCosting(string OrderPreCostingDirectMaterialId,string cs)
        {
            string strSQL, strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strCSQL = @"delete from OrderPreCostingDirectMaterial where Id='" + OrderPreCostingDirectMaterialId + @"'";
                strSQL = @"delete from OrderProcurementCostingDirectMaterial where Id='" + OrderPreCostingDirectMaterialId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                if (cs == "PreCosting")
                {
                    objCon.ExecuteNonQueryWrapper(strSQL, true, "1"); 
                }
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


            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteProcurementCosting(string OrderProcurementCostingDirectMaterialId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsOPRCDM;
            try
            {
                string sqlStopage = @"delete from OrderProcurementCostingDirectMaterial where Id='" + OrderProcurementCostingDirectMaterialId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsOPRCDM, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteOrderPreCostingDirectProces(string OrderPreCostingDirectProcessId,string cs)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsOPRCDM, dsPro;
            try
            {
                string sqlStopage = @"delete from OrderPreCostingDirectProcess where Id='" + OrderPreCostingDirectProcessId + @"'";
                string sqlPro = @"delete from OrderProcurementCostingDirectProcess where Id='" + OrderPreCostingDirectProcessId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsOPRCDM, false, "1");
                if (cs == "PreCosting")
                {
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsPro, false, "1"); 
                }

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteDirectProcessProcurementCosting(string DirectProcessProcurementCostingId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsOPRCDM;
            try
            {
                string sqlStopage = @"delete from OrderProcurementCostingDirectProcess where Id='" + DirectProcessProcurementCostingId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsOPRCDM, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteOperationListPreCosting(string OperationListPreCostingId, string cs)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsOLPC, dsPro;
            try
            {
                string sqlStopage = @"delete from OrderPreCostingOperation where Id='" + OperationListPreCostingId + @"'";
                string sqlPro = @"delete from OrderProcurementCostingOperation where Id='" + OperationListPreCostingId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsOLPC, false, "1");
                if (cs == "PreCosting")
                {
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsPro, false, "1"); 
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult DeleteOperationListProcurementCosting(string OperationListProcurementCostingId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsOLPRC;
            try
            {
                string sqlStopage = @"delete from OrderProcurementCostingOperation where Id='" + OperationListProcurementCostingId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsOLPRC, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteValueLossPreCosting(string ValueLossPreCostingId, string cs)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsVLPC,dsPro;
            try
            {
                string sqlStopage = @"delete from OrderPreCostingValueLoss where Id='" + ValueLossPreCostingId + @"'";
                string sqlPro = @"delete from OrderProcurementCostingValueLoss where Id='" + ValueLossPreCostingId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsVLPC, false, "1");
                if (cs == "PreCosting")
                {
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsPro, false, "1"); 
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteValueLossProcurementCosting(string ValueLossProcurementCostingId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsVLPC;
            try
            {
                string sqlStopage = @"delete from OrderProcurementCostingValueLoss where Id='" + ValueLossProcurementCostingId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsVLPC, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteOrderPreCostingProfit(string OrderPreCostingProfitId, string cs)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsVLPC, dsPro;
            try
            {
                string sqlStopage = @"delete from OrderPreCostingProfit where Id='" + OrderPreCostingProfitId + @"'";
                string sqlPro = @"delete from OrderProcurementCostingProfit where Id='" + OrderPreCostingProfitId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsVLPC, false, "1");
                if (cs == "PreCosting")
                {
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsPro, false, "1"); 
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteOrderProcurementCostingProfit(string OrderProcurementCostingProfitId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsVLPC;
            try
            {
                string sqlStopage = @"delete from OrderProcurementCostingProfit where Id='" + OrderProcurementCostingProfitId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsVLPC, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteOrderPreCostingSalesExpense(string OrderPreCostingSalesExpenseId, string cs)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsVLPC, dsPro;
            try
            {
                string sqlStopage = @"delete from OrderPreCostingSalesExpense where Id='" + OrderPreCostingSalesExpenseId + @"'";
                string sqlPro = @"delete from OrderProcurementCostingSalesExpense where Id='" + OrderPreCostingSalesExpenseId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsVLPC, false, "1");
                if (cs == "PreCosting")
                {
                    objCon.OpenDataSetThroughAdapter(sqlPro, out dsPro, false, "1"); 
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteOrderProcurementCostingSalesExpense(string OrderProcurementCostingSalesExpenseId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsVLPC;
            try
            {
                string sqlStopage = @"delete from OrderProcurementCostingSalesExpense where Id='" + OrderProcurementCostingSalesExpenseId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsVLPC, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOrderDirectMaterialBudget(string OrderCostingMasterTemplateId)
        {
            string sqlPre = @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,UOM.Code as UOM,pc.Particulars,I.UserName as CostingItems,I.CostingComponentId
					,CC.CostingSegment,cc.UserName as CostingComponentName,ISNULL(pc.Consumption,0) AS Consumption,ISNULL(pc.Rate,0) AS Rate
					,ISNULL(pc.ValueLoss,0) AS ValueLoss,pc.MinimumOfQuantity
					,ISNULL(pc.GrossConsumption,0) AS GrossConsumption
					,C.Code as Currency,OCMT.Id as OrderCostingMasterTemplateId
					,EI.EmployeeName as ResponsiblePerson,pc.SourcingType,MM.UserName as Material,MMA.StandardName as Article,pc.VendorId
					--,ISNULL(MOI.TotalQty,0) TotalQty
					,TotalQty=(select sum(TotalQty) from  trn.MasterOrderItem where OrderCostingMasterTemplateId=PC.OrderCostingMasterTemplateId)
					--,TotalMaterialRequirement=(ISNULL(MOI.TotalQty,0) * ISNULL(pc.GrossConsumption,0))
					,TotalMaterialRequirement=sum(ISNULL(TotalQty,0) * ISNULL(pc.GrossConsumption,0))
					,ISNULL(pc.GrossAmount,0) AS GrossAmount
                    ,TotalOrderCost=ISNULL(pc.GrossAmount,0)*(select sum(TotalQty) from  trn.MasterOrderItem where OrderCostingMasterTemplateId=PC.OrderCostingMasterTemplateId)

					FROM OrderPreCostingDirectMaterial AS pc  
					LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
					LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
                    LEFT JOIN SCS.UnitOfMeasurement as UOM on UOM.Id=I.UnitOfMeasurementId
					LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId
					LEFT JOIN TRN.MasterOrderItem MOI on MOI.OrderCostingMasterTemplateId=OCMT.Id
					LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
					LEFT JOIN EmployeeInformation EI on EI.SystemId=pc.ResponsiblePersonId
					LEFT JOIN MST.MaterialMasterArticle MMA on MMA.Id=pc.ArticleId
					LEFT JOIN MST.MaterialMaster MM on MM.Id=pc.MaterialMasterId
					LEFT JOIN HKP.Party P on P.Id=pc.VendorId
					
					where pc.OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + @"' and I.Id is not null
                    group by pc.Id,I.Id,pc.Sequence,UOM.Code,pc.Particulars,I.UserName,I.CostingComponentId	,CC.CostingSegment
					,cc.UserName,pc.Consumption,pc.Rate,pc.ValueLoss,pc.MinimumOfQuantity,pc.GrossConsumption,pc.GrossAmount
					,C.Code,OCMT.Id,EI.EmployeeName,pc.SourcingType,MM.UserName,MMA.StandardName,pc.VendorId,PC.OrderCostingMasterTemplateId
					order by pc.Sequence"; 

            return Json(new { Pre = _sqlRepository.GetDataCollection(sqlPre, null)}, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOrderBudgetDirectProcess(string OrderCostingMasterTemplateId, string costingComponentId)
        {
            string sqlPre = @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItems,I.CostingComponentId
            ,ISNULL(pc.ExecutionType,'Fixed') as [Type]
			,OCMT.Id as OrderCostingMasterTemplateId,cc.UserName as CostingComponentName
			,ISNULL(pc.Value,0) AS ValueLoss,ISNULL(pc.Rate,0) AS Rate,ISNULL(pc.Amount,0) AS Amount
			,C.Code as Currency ,moi.TotalQty OrderQty
			,TotalOrderCost=ISNULL(pc.Amount,0)*moi.TotalQty
			
			FROM OrderPreCostingDirectProcess AS pc   
			LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
			LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId
			LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
            LEFT JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=ocmt.Id

			where pc.OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + @"' and I.CostingComponentId ='" + costingComponentId + @"'
			order by pc.Sequence";

            return Json(new { Pre = _sqlRepository.GetDataCollection(sqlPre, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOrderBudgetOperation(string costingComponentId, string OrderCostingMasterTemplateId)
        {
            string sql = @"SELECT pc.Id,I.Id as CostingId,I.UserName as CostingItems,I.CostingComponentId,pc.Sequence
				,ISNULL(pc.Value,0) AS Value,OCMT.Id as OrderCostingMasterTemplateId
				,cc.UserName as CostingComponentName,c.Code as Currency,moi.TotalQty OrderQty
				,TotalOrderCost=ISNULL(pc.value,0)*moi.TotalQty						
				
				FROM OrderPreCostingOperation AS pc       
				LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId 
				LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
				LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
				LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
				LEFT JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=ocmt.Id

			where pc.OrderCostingMasterTemplateId='" + OrderCostingMasterTemplateId + @"'
			order by pc.Sequence";
 
            return Json(new { Pre = _sqlRepository.GetDataCollection(sql, null)}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOrderBudgetValueLoss(string OrderCostingId)
        {
            string sql = @"SELECT pc.Id,I.Id as CostingId,I.UserName as CostingItems,I.CostingComponentId,pc.Sequence
			                        ,OCMT.Id as OrderCostingMasterTemplateId,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value,ISNULL(pc.Amount,0) AS Amount
			                        ,C.Code as Currency,cc.UserName as CostingComponentName ,moi.TotalQty OrderQty
				                        ,TotalOrderCost=ISNULL(pc.amount,0)*moi.TotalQty			


			                        FROM OrderPreCostingValueLoss AS pc 
			                        LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
			                        LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			                        LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
			                        LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId 
			                        LEFT JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=ocmt.Id
                        
                                    where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
			                        order by pc.Sequence";

            return Json(new { Pre = _sqlRepository.GetDataCollection(sql, null)}, JsonRequestBehavior.AllowGet);
        }
         

        [HttpGet, Authorize]
        public ActionResult GetOrderBudgetProfit(string OrderCostingId)
        {
            string sqlPre = @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItems,I.CostingComponentId
			                            ,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value,ISNULL(pc.Amount,0) AS Amount,C.Code as Currency
			                            ,OCMT.CurrencyId,PC.OrderCostingMasterTemplateId,cc.UserName as CostingComponentName  
			                            ,moi.TotalQty OrderQty,TotalOrderCost=ISNULL(pc.Amount,0)*moi.TotalQty	
					
			                            FROM OrderPreCostingProfit AS pc 
			                            LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
			                            LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
			                            LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
			                            LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId 
			                            LEFT JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=ocmt.Id

                                    where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
			                        order by pc.Sequence";
            return Json(new { Pre = _sqlRepository.GetDataCollection(sqlPre, null)}, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetOrderBudgetSalesExpense(string OrderCostingId)
        {
            string sql = @"SELECT pc.Id,I.Id as CostingId,pc.Sequence,I.UserName as CostingItems,I.CostingComponentId
		                            ,ISNULL(pc.Type,'Fixed') as [Type],ISNULL(pc.Value,0) AS Value,ISNULL(pc.Amount,0) AS Amount,C.Code as Currency
		                            ,cc.UserName as CostingComponentName,moi.TotalQty OrderQty,TotalOrderCost=ISNULL(pc.Amount,0)*moi.TotalQty	

		                            FROM OrderPreCostingSalesExpense AS pc    
		                            LEFT JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
		                            LEFT JOIN HKP.CostingComponent CC on CC.Id=I.CostingComponentId
		                            LEFT JOIN OrderCostingMasterTemplate OCMT on OCMT.Id=PC.OrderCostingMasterTemplateId 
		                            LEFT JOIN SCS.Currency C on C.Id=OCMT.CurrencyId
		                            LEFT JOIN EmployeeInformation EI on EI.SystemId=pc.ResponsiblePersonId
		                            LEFT JOIN trn.MasterOrderItem AS moi ON moi.OrderCostingMasterTemplateId=ocmt.Id
                                    
                                    where pc.OrderCostingMasterTemplateId='" + OrderCostingId + @"'
			                        order by pc.Sequence";             
            return Json(new { Pre = _sqlRepository.GetDataCollection(sql, null)}, JsonRequestBehavior.AllowGet);
        }

    }


    public class OrderCostingBuyer
    {
        public string Id { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public string BuyerId { get; set; }
        public string BuyerStyleRefNo { get; set; }
        public string OwnStyleRefNo { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

    }
    public class OrderCostingDetailTemplate
    {
        public string Id { get; set; }
        public string CostingComponentId { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public string CostingVersionMasterTemplateId { get; set; }
        public decimal Sequence { get; set; }
        public decimal CostingValue { get; set; }
        public decimal BuyerTarget { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }


    }

    public class OrderCostingMasterTemplate : BaseModel
    {
        public string Id { get; set; }
        public string ProductMasterId { get; set; }
        public string CustomerId { get; set; }
        public string CostingMasterTemplateId { get; set; }
        public int Version { get; set; }
        public string Code { get; set; }
        public string SpecifyTo { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public decimal OrderSize { get; set; }
        public int ProductionAvailableDays { get; set; }

        public decimal TargetSellingPrice { get; set; }
        public decimal PaymentDays { get; set; }
        public string PackingType { get; set; }
        public int EstNoOfPackingList { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool Active { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public decimal ExcessShipmentPer { get; set; }
        public string CurrencyId { get; set; }
        public string UOM { get; set; }
        public string TargetOrSPT { get; set; }
        public string CriticalLevel { get; set; }
        public decimal MKTTargetPerHour { get; set; }

        public string CostingStage { get; set; }

        public string InquiryItemId { get; set; }
        public string MasterOrderItemId { get; set; }

        public decimal SPT { get; set; }
        public int NoOfWorkstation { get; set; }
        public decimal EfficiencyPercentage { get; set; }
        public decimal StandardWorkingHours { get; set; }
        public decimal WorkCenterTargetPerDay { get; set; }

        public decimal StandardWorkingHourCost { get; set; }
        public decimal AdditionalWorkingHourCostPerHour { get; set; }
        public bool isDirectApproval { get; set; } = false;
        public decimal TargetCM { get; set; }
        public decimal TargetProfit { get; set; }
        public bool IsPercentage { get; set; }
        //public bool IsApprovalApplicable { get; set; }
        //public string ApproveByWhomId { get; set; }
    }
    public class OrderOrderCostingDetailTemplate
    {
        public string Id { get; set; }
        public string CostingComponentId { get; set; }
        public string OrderCostingVersionMasterTemplateId { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public decimal Sequence { get; set; }
        public decimal CostingValue { get; set; }
        public decimal BuyerTarget { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public decimal ExcessShipmentPer { get; set; }

    }

    public class OrderPreCostingDirectMaterial
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }
        public decimal Consumption { get; set; }
        public decimal UOM { get; set; }
        public decimal Rate { get; set; }
        public decimal ValueLoss { get; set; }
        public decimal GrossConsumption { get; set; }
        public decimal GrossAmount { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string SourcingType { get; set; }
        public string Usage { get; set; }
        public string POCriteria { get; set; }
        public bool IsUDApplicable { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsMandatory { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string VendorId { get; set; }

        public string ProcurementLevel { get; set; }
        public decimal BOQDays { get; set; }
        public string BOQCriteria { get; set; }
        public string DependentDate { get; set; }

        public decimal MinimumOfQuantity { get; set; }
        public int POIssueDeadLine { get; set; }
        public string PurchaseGroupId { get; set; }
        public string Particulars { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class OrderPreCostingSalesExpense
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }

        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class OrderPreCostingValueLoss
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class OrderPreCostingProfit
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }

        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class OrderPreCostingDirectProcess
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }

        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string ExecutionType { get; set; }
        public decimal Value { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class OrderPreCostingOperation
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }

        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public decimal Value { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }


    public class OrderProcurementCostingDirectMaterial
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }
        public decimal Consumption { get; set; }
        public decimal UOM { get; set; }
        public decimal Rate { get; set; }
        public decimal ValueLoss { get; set; }
        public decimal GrossConsumption { get; set; }
        public decimal GrossAmount { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string SourcingType { get; set; }
        public string Usage { get; set; }
        public string POCriteria { get; set; }
        public bool IsUDApplicable { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsMandatory { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string VendorId { get; set; }

        public string ProcurementLevel { get; set; }
        public decimal BOQDays { get; set; }
        public string BOQCriteria { get; set; }
        public string DependentDate { get; set; }

        public decimal MinimumOfQuantity { get; set; }
        public int POIssueDeadLine { get; set; }
        public string PurchaseGroupId { get; set; }
        public string Particulars { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class OrderProcurementCostingSalesExpense
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class OrderProcurementCostingValueLoss
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class OrderProcurementCostingProfit
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }

        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string Type { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class OrderProcurementCostingDirectProcess
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public string ExecutionType { get; set; }
        public decimal Value { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
    public class OrderProcurementCostingOperation
    {
        public string Id { get; set; }
        public string CostingItemId { get; set; }
        public decimal Sequence { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }
        public string ResponsiblePersonId { get; set; }

        public decimal Value { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }


        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}
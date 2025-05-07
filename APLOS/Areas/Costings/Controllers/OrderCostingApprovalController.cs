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
    public class OrderCostingApprovalController : BaseController
    {


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public OrderCostingApprovalController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            try
            {
                var sql = @"SELECT MO.PartyId,P.UserName Customer,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,MO.Id MasterOrderId,MOI.Id LineItemId
  FROM TRN.MasterOrderItem AS moi 
LEFT JOIN TRN.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
LEFT JOIN HKP.Party P ON P.Id=MO.PartyId
WHERE MO.OrderStatusId='Active'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        public ActionResult ApproveQuickCosting(string TemplateId)
        {

            try
            {


                string sql = "update OrderCostingMasterTemplate set isQuickCostingApproved=1 Where Id='" + TemplateId + "'";

                _sqlRepository.ExecuteSqlCommand(sql);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Message = "SO Updated Successfully" });

        }
        [HttpPost]
        public ActionResult ApprovePreCosting(string TemplateId)
        {

            try
            {


                string sql = "update OrderCostingMasterTemplate set isPreCostingApproved=1 Where Id='" + TemplateId + "'";

                _sqlRepository.ExecuteSqlCommand(sql);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Message = "SO Updated Successfully" });

        }
        [HttpPost]
        public ActionResult ApproveProcurementCosting(string TemplateId)
        {

            try
            {


                string sql = "update OrderCostingMasterTemplate set isProcurementCostingApproved=1 Where Id='" + TemplateId + "'";

                _sqlRepository.ExecuteSqlCommand(sql);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

            return Json(new { Error = false, Message = "SO Updated Successfully" });

        }

     
       

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value, Dictionary<string, string> parameters)

        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory,CUR.Code AS Currency,ct.UserName AS CostingTypeName
							,psc.UserName as ProductSubCategory
                            ,pm.CostingType,qcm.CostingStage AS CurrentCostStage
                            ,QApproved=CASE WHEN ISNULL(qcm.isQuickCostingApproved, 0)=0 THEN 'No' ELSE 'Yes' END
							,PreApproved=CASE WHEN ISNULL(qcm.isPreCostingApproved, 0)=0 THEN 'No' ELSE 'Yes' END
							,ProcApproved=CASE WHEN ISNULL(qcm.isProcurementCostingApproved, 0)=0 THEN 'No' ELSE 'Yes' END
							from OrderCostingMasterTemplate qcm 
							
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join scs.Currency CUR on CUR.Id=qcm.CurrencyId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT OUTER JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType
                          WHERE QCM.PlantId='" + identity.PlantId + @"' AND qcm.Id IN(SELECT OrderCostingMasterTemplateId FROM TRN.MasterOrderItem WHERE Id IN(" + parameters["LineItemId"] + @")) and (isnull(isPreCostingApproved,0)=0 OR isnull(isQuickCostingApproved,0)=0) ) AS TEMP WHERE 1=1 AND " + strkey;


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
        [HttpPost, Authorize]
        public ActionResult GetListItem(string Id)

        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory
							,psc.UserName as ProductSubCategory
							--,CostingType=case  when pm.CostingType = 'CostingType1' then 'Garment' 
							--	else   'Fabric' end
                             ,pm.CostingType
							from OrderCostingMasterTemplate qcm 
							
                            left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
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


        [HttpPost, Authorize]
        public ActionResult GetSOListForTemplate(string TemplateId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT convert(bit,0) AS isChecked,mm.UserName AS Material,mma.StandardName AS Article, moi.MasterOrderId,p.Id AS PartyId,pm.UserName AS Product, p.UserName AS Customer, moi.Id ItemId,SO.Id AS SalesOrder, FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,pm.Id
								,b.UserName Buyer,moi.BuyerReferenceNo
                                  FROM trn.MasterOrder AS mo
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.MasterOrderId=mo.Id
                                INNER JOIN hkp.Buyer b ON b.Id=mo.BuyerId
                                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id
                                left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId
                                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                                left outer join hkp.Party AS p ON p.Id=mo.PartyId

                                WHERE isnull(moi.OrderCostingMasterTemplateId,'')='" + TemplateId + @"'
                                ORDER BY mo.Id,pm.UserName,so.Id
                            ";


            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQuickCostingDetailByProductMaster(string ProductMasterId, string CostingVersionMasterTemplateId)
        {
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
                        ,isnull(itemvalp.TotalGrossAmount,0) AS TotalProcurementGrossAmount 
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

            string sqlAllItem = @"  SELECT  ci.Id,CI.Code, ctc.Sequence AS ComponentSequence,ci.Sequence AS ItemSequnce, ci.CostingCategoryId, ci.CostingComponentId,cc.CostingSegment,upper(isnull(itemval.ValueType,'FIXED')) AS ValueType,
                        isnull(itemval.TotalGrossAmount,0) AS TotalGrossAmount,isnull(itemval.Value,0) AS Value,isnull(itemval.Rate,0) AS Rate,
                        isnull(itemvalp.TotalGrossAmount,0) AS TotalProcurementGrossAmount,isnull(itemvalp.Value,0) AS ProcurementValue,isnull(itemvalp.Rate,0) AS ProcurementRate,
						upper(isnull(itemval.ValueType,'FIXED')) AS ProcurementValueType
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


            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            return Json(new { ComponentList = _sqlRepository.GetDataCollection(sql, null), ItemList = _sqlRepository.GetDataCollection(sqlAllItem, null) }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBuyerDataByCostingMasterId(string costingMasterId)
        {
            string sql = @"select cb.*, b.UserName as Buyer from [dbo].[OrderCostingBuyer] cb
                            left join hkp.Buyer b on b.Id = cb.BuyerId where OrderCostingMasterTemplateId = '" + costingMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

    }

}
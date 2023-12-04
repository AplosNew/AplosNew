#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Crosscutting.Security;
using System.Data;
using Library.Security.Core;
using System.Threading;
using Library.MaterialManagement.Material;
using System.Web;
using Newtonsoft.Json;
using Library.Service.Helpers;
using System.IO;
using Library.Core;
using Library.MaterialManagement.CutPlan;
using Library.Service.OrderManagements;
using System.Linq;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class CutPlanController : BaseController
    {
        #region Constructor
        private readonly IProductionOrderService _productionOrderService;
        clsCutPlan cp = new clsCutPlan();
		private readonly ISqlRepository _sqlRepository;
        public CutPlanController(ISqlRepository R, IProductionOrderService productionOrderService)
        {
            _productionOrderService = productionOrderService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region Operation

        [Authorize, HttpPost]
        public ActionResult GetUserName()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadCutPlanList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * ,(select E.EmployeeName from EmployeeInformation E where E.SystemId=CP.UserId) as UserName,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=CP.ResponsiblePersonId) as ResponsiblePerson
                            FROM [MST].[CutPlan] CP";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadCutPlanEditData(string CutPlanId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select E.EmployeeName from EmployeeInformation E where E.SystemId=CP.UserId) as UserName,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=CP.ResponsiblePersonId) as ResponsiblePerson
                            FROM [MST].[CutPlan] CP where CP.Id='" + CutPlanId + @"'";
            return Json(new { cutplan = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderDataList(string entityId)
        {
            return Json(cp.GetProductionOrderData(entityId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetLineItemData(string entityId, string processId, string productionOrderId, string masterId)
        {
            return Json(cp.GetLineItemData(entityId, processId, productionOrderId, masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionRecipeMaterialList(string productionOrderId)
        {
            return Json(_productionOrderService.GetProductionRecipeMaterialList(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCutPlanDetailsList(string FromDate, string ToDate, string ProductionEntityId, string PlanId)
        {
            string FilterDate = string.Empty;
            string FilterEntity = string.Empty;
            string FilterPlan = string.Empty;

            if (FromDate != "null" && ToDate != "null" && FromDate != "undefined" && ToDate != "undefined")
            {
                FilterDate = " and SO.AddedDate between '" + FromDate + "' and '" + ToDate + "'";
            }
            if (ProductionEntityId != "null" && ProductionEntityId != "undefined")
            {
                FilterEntity = " and POS.EntityId='" + ProductionEntityId + "'";
            }
            if (PlanId != "null" && PlanId != "undefined")
            {
                FilterPlan = " ORDER BY CPD.CutPlanId desc";
            }
            else
            {
                FilterPlan = " ORDER BY MOI.ProductionGrouping,MOI.OwnReferenceNo";
            }
            string sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping,MOI.OwnReferenceNo, isnull(PO.PONumber,'') AS PONumber,PS.UserName ProductionStatus,
OS.UserName AS OrderStatusName,SO.Id SONo,SO.Qty,isnull(CPD.PlanPercentage,MO.ExtraOrderPercentage) PlanPercentage,isnull(CPD.SOPlanQty,SO.Qty + (MO.ExtraOrderPercentage*SO.Qty / 100)) as SOPlanQty,CPD.CutPlanId,CPD.Id,CPD.Status,MOI.MaterialMasterId, MM.UserName AS MaterialMasterName, MOI.ArticleId, 
ART.StandardName AS ArticleName,P.UserName AS Customer,MOI.BuyerReferenceNo,MOI.Id LineItemNo
                                ,MO.Type,isnull(moi.Consignment,0) AS Consignment
                                ,CASE WHEN ISNULL(eout.Id,'')<>'' OR ISNULL(TOUT.Id,'')<>'' THEN CONCAT(POWN.UserName,'(',EOWN.UserName,')') ELSE '' END AS OrderOwner
                                ,POD.Id, POD.ProductionOrderId, MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId
	                            , SO.Id AS SalesOrderId,B.UserName AS Buyer,PM.Id AS ProductID
	                            ,PM.UserName AS ProductName
	                            ,MO.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), SO.CommitmentDate, 106),' ','-')
                                , LSD = REPLACE(CONVERT(CHAR(11), SO.LSD, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , OC.UserName AS OrderCategoryName
	                            , SO.CM, SO.Rate,ISNULL(SO.Description,'')Description
	                            , Flag = CAST(0 AS BIT),ISNULL(SO.DestinationDescription,'')DestinationDescription
							
                       FROM 
                       [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[ProductionOrderDetail] AS POD ON pod.SalesOrderId=so.Id
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
					   LEFT JOIN TRN.ProductionOrder POS ON POS.Id=POD.ProductionOrderId
					   LEFT JOIN HKP.ProductionStatus PS ON PS.Id=POS.ProductionStatusId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                      LEFT JOIN [MST].[CutPlanSODetails] CPD on CPD.SalesOrderId=SO.Id and CPD.CutPlanId='" + PlanId + @"'
					  
							LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId

                      where SO.OrderStatusId not in ('Closed','Cancelled') " + FilterDate + @" " + FilterEntity + @"
                     " + FilterPlan + @"";
                    
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createCutPlan(Dictionary<string, object> CutPlanData, List<Dictionary<string, object>> DataList)
        {
            string TableName = "[MST].[CutPlanSODetails]";
            string contId = string.Empty;
            string _CPId = "";
            DataSet dsCutPlanData, dsProdBooked, dsChildId;

            try
            {
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
              

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[CutPlan] where Id='" + CutPlanData["Id"] + "'", out dsCutPlanData, false, "1");
                string _Id = "", Id = string.Empty;

                #region data update
                if (dsCutPlanData.Tables[0].Rows.Count == 0)
                {
                     bplib.clsGenID genid = new bplib.clsGenID();
                     genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[MST].[CutPlan]", out _Id);
                     CutPlanData["Id"] = _Id;
                    _CPId = CutPlanData["Id"].ToString();
                    AddNewRow(dsCutPlanData.Tables[0], CutPlanData);
                }
                else
                {
                    _Id = CutPlanData["Id"].ToString();
                    _CPId = CutPlanData["Id"].ToString();
                    EditRow(dsCutPlanData.Tables[0].Rows[0], CutPlanData);
                }
                #endregion data update

                #region  
                conRack.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  CutPlanId='" + _CPId + "'", out dsProdBooked, false, "1");
                conRack.OpenDataSetThroughAdapter("select count(Id) + 1 as CPDId from [MST].[CutPlanSODetails] where CutPlanId='" + _CPId + "'", out dsChildId, false, "1");
                int CPDId = Convert.ToInt32(dsChildId.Tables[0].Rows[0]["CPDId"].ToString());
               
                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            item["Id"] = _CPId + "-" + CPDId++;
                            item["CutPlanId"] = _CPId;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else if(dv.Count > 0 && Convert.ToBoolean(item["Status"].ToString()) == false)
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                    }
                }

                #endregion


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsCutPlanData, dsProdBooked);

                return Json(new { Error = true, Data = dsCutPlanData, Message = AplosMessage.Insert });

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

        [HttpGet, Authorize]
        public JsonResult GetMarker(string MaterialId)
        {
            return Json(cp.getMarkerList(MaterialId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMarkerDetails(string MarkerId)
        {
            return Json(cp.GetMarkerDetailList(MarkerId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSkuDetails(string OtherSku,string SOId, string Sequence,string CharacteristicsValueId)
        {
            return Json(cp.GetOtherSkuDetailList(OtherSku, SOId, Sequence, CharacteristicsValueId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetSeceondIterationData(string MasterId)
        {
            var _sql = "";
            var _sql1 = "";
            var _sql2 = "";
            _sql = @"SELECT distinct cv.UserName,cv.Sequence,cpf.MarkerRatio
                            FROM CutPlanFormation AS cpf
                            JOIN CutPlanChild AS cpc ON cpc.Id = cpf.CutPlanChildId
                            JOIN CutPlanMarkerDetails AS cpmd ON cpmd.Id = cpc.CutPlanMarkerDetailsId
                            JOIN hkp.CharacteristicsValue AS cv ON cv.Id = cpf.MarkerCharacteristicsValueId
                            WHERE cpmd.CutPlanMasterId='" + MasterId + @"' 
                            ORDER BY cv.Sequence";

            _sql1 = @"SELECT cpf.QtyForCalculation,cpf.CalculatedQty,(cpf.QtyForCalculation-cpf.CalculatedQty) CurrentQty,cpc.CharacteristicsValueId
                            FROM CutPlanFormation AS cpf
                            JOIN CutPlanChild AS cpc ON cpc.Id = cpf.CutPlanChildId
                            JOIN CutPlanMarkerDetails AS cpmd ON cpmd.Id = cpc.CutPlanMarkerDetailsId
                            JOIN hkp.CharacteristicsValue AS cv ON cv.Id = cpf.MarkerCharacteristicsValueId
                            JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id = cpf.MarkerCharacteristicsValueId
                            WHERE cpmd.CutPlanMasterId='" + MasterId + @"' 
                            ORDER BY CV1.Sequence,cv.Sequence";

            _sql2 = @"SELECT DISTINCT cv.UserName,cpf.QtyForCalculation,cv.Sequence,cpc.CharacteristicsValueId, null as Details
                            FROM CutPlanFormation AS cpf
                            JOIN CutPlanChild AS cpc ON cpc.Id = cpf.CutPlanChildId
                            JOIN CutPlanMarkerDetails AS cpmd ON cpmd.Id = cpc.CutPlanMarkerDetailsId
                            JOIN hkp.CharacteristicsValue AS cv ON cv.Id = cpc.CharacteristicsValueId
                            WHERE cpmd.CutPlanMasterId='" + MasterId + @"' 
                            ORDER BY cv.Sequence";

            var ColorData = _sqlRepository.GetDataCollection(_sql2);
            var SizeData = _sqlRepository.GetDataCollection(_sql1);
            for (int i = 0; i < ColorData.Count; i++)
            {
                var TempData = SizeData.Where(x=>x["CharacteristicsValueId"].ToString() == ColorData[i]["CharacteristicsValueId"].ToString() && x["QtyForCalculation"].ToString() == ColorData[i]["QtyForCalculation"].ToString()).ToList();
                ColorData[i]["Details"] = TempData;
            }

            var jsondata = Json(new { HeaderData = _sqlRepository.GetDataCollection(_sql), MaintData = ColorData }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public JsonResult Create( List<CutPlantCalculate> CalculatedValueList, List<Dictionary<string, object>> FGCharacteristicsValueList, CutPlanMaster MasterData, CutPlanMarkerDetails CPMarkerDetails,List<Dictionary<string,object>> SkuValueList)
        {
            try
            {
                cp.Save(CalculatedValueList, FGCharacteristicsValueList,MasterData, CPMarkerDetails, SkuValueList);
                return Json(new { Error = false, data = MasterData, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion
    }
}

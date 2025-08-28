using Library.Model.IE;
using Aplos.Properties;
using Library.Data;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Service.IE;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Service.Enums;
using Library.Planning.OrderManagement;
using System.Data;
using Library.Security.Core;
using Library.Data.Sql;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class OS3DashboardController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        OS3Dashboard os3 = new OS3Dashboard();
        public OS3DashboardController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }


        #endregion -- Pages

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            return Json(os3.filters(), JsonRequestBehavior.AllowGet);
        }

        public void GetProductionOrderMaster(out DataTable dtOrderMaster)
        {
            try
            {
                string sql = @"Select * from(Select row_number() over (partition by po.Id order by po.Id,A.Date) as Seq
,po.Id POId,sc.ID ScheduleId,PS.UserName POStatus,FORMAT(PO.AddedDate,'dd-MMM-yyyy')POCreationDate ,FORMAT(BASEP.BaseProcProdStartDate,'dd-MMM-yyyy')BaseProcProdStartDate,FORMAT(BASEP.BaseProductionEndDate,'dd-MMM-yyyy')BaseProductionEndDate
,FORMAT(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')BaseProcPlanStartDate,FORMAT(Type1.BaseProcPlanEndDate,'dd-MMM-yyyy')BaseProcPlanEndDate
,POStartDate=FORMAT(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  <  Type1.BaseProcPlanStartDate then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end,'dd-MMM-yyyy')
,POCompletionDate=FORMAT((case when Type1.BaseProcPlanEndDate is null or BASEP.BaseProductionEndDate  > Type1.BaseProcPlanEndDate then BASEP.BaseProductionEndDate else Type1.BaseProcPlanEndDate end ),'dd-MMM-yyyy')
,COUNT(SO.id) NoOfSO
,FORMAT(A.Date,'dd-MMM-yyyy') Date

,PlanningStatus=CASE WHEN FORMAT(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  <  Type1.BaseProcPlanStartDate then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end,'dd-MMM-yyyy') IS NULL 
OR FORMAT((case when Type1.BaseProcPlanEndDate is null or BASEP.BaseProductionEndDate  > Type1.BaseProcPlanEndDate then BASEP.BaseProductionEndDate else Type1.BaseProcPlanEndDate end ),'dd-MMM-yyyy') IS NULL OR SC.Id IS NULL THEN 'Schedule Missing' ELSE 'Schedule' END
,POCompletion= CASE WHEN A.Date<= GETDATE() Then 'Complete' else 'Scheduled' END 
,A.ProdQty,A.PlanQty,AvailableQty= CASE WHEN ISNULL(A.ProdQty,0)>0 THEN A.ProdQty ELSE A.PlanQty END

,CumProdQty=SUM(CASE WHEN ISNULL(A.ProdQty,0)>0 THEN A.ProdQty ELSE A.PlanQty END) OVER(PARTITION BY PO.ID ORDER BY A.Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)


FROM trn.SalesOrder SO
LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=so.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcProdStartDate,MAX(ProductionDate)BaseProductionEndDate,A.ProductionOrderId 
FROM TRN.ProductionSummary A
LEFT JOIN HKP.Process B ON B.Id=A.ProcessId
Group By A.ProductionOrderId) BASEP ON BASEP.ProductionOrderId=POD.ProductionOrderId

LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanEndDate,ProductionOrderId 
From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=POD.ProductionOrderId
LEFT JOIN TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN dbo.ProductionOrderSchedulingParametersType1 SC ON Sc.ProductionOrderID=PO.Id
LEFT JOIN(
Select B.* from
(
Select PS.ProductionOrderId POId,PS.ProductionDate Date,SUM(Quantity)ProdQty,0 PlanQty from TRN.ProductionOrder PO
LEFT JOIN TRN.ProductionSummary PS ON PS.ProductionOrderId=PO.Id
left join TRN.ProductionOrderProcessSet A ON A.ProductionOrderId=PS.ProductionOrderId  AND PS.ProcessId=A.ProcessId Where A.IsBaseProcess=1 Group BY PS.ProductionOrderId,PS.ProductionDate
UNION
Select DISTINCT PO.Id POId,T1.ProductionDate Date, 0 ProdQty,SUM(T1.Quantity) PlanQty 
from TRN.ProductionOrder PO
LEFT JOIN dbo.ProductionPlanningType1 T1 ON T1.ProductionOrderID=PO.Id
Group BY PO.Id,T1.ProductionDate
)B Where ISNULL(B.Date,'')<>'' 
)A ON A.POId=PO.Id

Where SO.OrderStatusId NOT IN('Cancelled','Closed') AND SO.ShipmentFromStock=0 and pod.ProductionOrderId<>''
GROUP BY po.Id,BASEP.BaseProcProdStartDate,BASEP.BaseProductionEndDate,Type1.BaseProcPlanStartDate,Type1.BaseProcPlanEndDate
,A.Date,sc.ID,PS.UserName,PO.AddedDate,A.ProdQty,A.PlanQty)x
";
                dtOrderMaster = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetSOCompletionData(out DataTable dt)
        {
            try
            {
                string sql = @"SELECT row_number() over (partition by POD.ProductionOrderId order by POD.ProductionOrderId,SO.DeliveryDate) as Seq,
POD.ProductionOrderId,SO.OrderStatusId SOStatus,m.[Days]
,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,SO.Id SOId,SO.Qty SOQty
,SoCommqty=SUM(SO.Qty) OVER (PARTITION BY POD.ProductionOrderId ORDER BY SO.DeliveryDate ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)
,P.UserName Customer,MOI.BuyerReferenceNo,moi.OwnReferenceNo,moi.Id LineitemId,MMA.StandardName Article,PL.Code ProductCode
,ProductLibraryDetail=STUFF((select distinct ','+MA.Code+'-'+MA.AttributeValue from
												[dbo].ProductLibraryAttribute MA												
												where MA.ProductLibraryId=PL.Id for xml path('') ), 1, 1, '')

,PS.UserName POStatus,FORMAT(SO.PlanExFactoryDate,'dd-MMM-yyyy')ExFactoryDate,FORMAT(SO.CommitmentDate,'dd-MMM-yyyy')CommitmentDate,RP.EmployeeName ResponsiblePerson,E.UserName Entity,CP.PartyType,DiffComEx=CASE  WHEN SO.CommitmentDate IS NULL THEN DATEDIFF(DAY,PlanExFactoryDate,GETDATE()) ELSE DATEDIFF(DAY,SO.CommitmentDate,GETDATE()) END,'' ExDate,''EarlyOrLateBy,''Months,''Years,so.OrderStatusId
,''LN30,''LN30T20,''LN20T10,''LN10T5,''LN5T0,''E0,''G0T5,''G5T10,''G10T15,''G15T20,''G20T30,''G30,''nodates,''NotAlotted,''daysthree,so.AddedDate
from trn.SalesOrder SO
left join TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
left join TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet M ON m.ProductionOrderId=POD.ProductionOrderId
AND m.Id=(SELECT TOP 1 ID FROM TRN.ProductionOrderProcessSet EII WHERE EII.ProductionOrderId=POD.ProductionOrderId ORDER BY EII.Sequence DESC)
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON MO.Id=MOI.MasterOrderId
LEFT JOIN HKP.Party P ON P.Id=MO.PartyId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
LEFT JOIN [dbo].[ProductLibrary] PL ON PL.Id=MOI.ProductLibraryId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN dbo.EmployeeInformation RP ON RP.SystemId=SO.ResponsiblePersonId
LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'
Where  SO.OrderStatusId NOT IN('Cancelled','Closed') AND SO.ShipmentFromStock=0  AND POD.ProductionOrderId<>''";

                dt = _sqlRepository.GetDataTable(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult getSlabData(Dictionary<string, string> parameters, string group, string value, string analysis, string type)
        {
            //string ExpectedDate = "";
            //DataTable dtSOComplete, dtOrderMaster;
            //GetProductionOrderMaster(out dtOrderMaster);
            //GetSOCompletionData(out dtSOComplete);
            //DateTime currentDate = DateTime.Now;
            //DateTime newDate = currentDate.AddDays(-3);
            //for (int i = 0; i < dtSOComplete.Rows.Count; i++)
            //{
            //    DataRow dr = GetExpectedSOCompletionDate(clsStaticInfo.dbl(dtSOComplete.Rows[i]["SoCommqty"].ToString()), dtSOComplete.Rows[i]["ProductionOrderId"].ToString(), dtOrderMaster);

            //    if (dr != null)
            //    {
            //        ExpectedDate = GetDate(dr["Date"].ToString());
            //        dtSOComplete.Rows[i]["ExDate"] = ExpectedDate;

            //        TimeSpan dts = Convert.ToDateTime(ExpectedDate) - Convert.ToDateTime(dtSOComplete.Rows[i]["DeliveryDate"].ToString());
            //        dtSOComplete.Rows[i]["EarlyOrLateBy"] = dts.Days;
            //        dtSOComplete.Rows[i]["Months"] = Convert.ToDateTime(dtSOComplete.Rows[i]["DeliveryDate"].ToString()).ToString("MMMM");
            //        DateTime date = DateTime.Parse(dtSOComplete.Rows[i]["DeliveryDate"].ToString());
            //        dtSOComplete.Rows[i]["Years"] = date.Year;
            //        if (dts.Days < -30)
            //            dtSOComplete.Rows[i]["LN30"] = 1;
            //        dtSOComplete.Rows[i]["LN30"] = 0;

            //        if (dts.Days > -31 && dts.Days < -20)
            //            dtSOComplete.Rows[i]["LN30T20"] = 1;
            //        dtSOComplete.Rows[i]["LN30T20"] = 0;

            //        if (dts.Days > -21 && dts.Days < -10)
            //            dtSOComplete.Rows[i]["LN20T10"] = 1;
            //        dtSOComplete.Rows[i]["LN20T10"] = 0;

            //        if (dts.Days > -11 && dts.Days < -5)
            //            dtSOComplete.Rows[i]["LN10T5"] = 1;
            //        dtSOComplete.Rows[i]["LN10T5"] = 0;

            //        if (dts.Days > -6 && dts.Days <0)
            //            dtSOComplete.Rows[i]["LN5T0"] = 1;
            //        dtSOComplete.Rows[i]["LN5T0"] = 0;

            //        if (dts.Days==0)
            //            dtSOComplete.Rows[i]["E0"] = 1;
            //        dtSOComplete.Rows[i]["E0"] = 0;

            //        if (dts.Days > 0 && dts.Days < 6)
            //            dtSOComplete.Rows[i]["G0T5"] = 1;
            //        dtSOComplete.Rows[i]["G0T5"] = 0;


            //        if (dts.Days > 5 && dts.Days < 11)
            //            dtSOComplete.Rows[i]["G5T10"] = 1;
            //        dtSOComplete.Rows[i]["G5T10"] = 0;

            //        if (dts.Days > 10 && dts.Days < 16)
            //            dtSOComplete.Rows[i]["G10T15"] = 1;
            //        dtSOComplete.Rows[i]["G10T15"] = 0;

            //        if (dts.Days > 15 && dts.Days < 21)
            //            dtSOComplete.Rows[i]["G15T20"] = 1;
            //        dtSOComplete.Rows[i]["G15T20"] = 0;

            //        if (dts.Days > 20 && dts.Days < 31)
            //            dtSOComplete.Rows[i]["G20T30"] = 1;
            //        dtSOComplete.Rows[i]["G20T30"] = 0;

            //        if (dts.Days > 30)
            //            dtSOComplete.Rows[i]["G30"] = 1;
            //        dtSOComplete.Rows[i]["G30"] = 0;

            //        if (ExpectedDate=="")
            //            dtSOComplete.Rows[i]["nodates"] = 1;
            //        dtSOComplete.Rows[i]["nodates"] = 0;

            //        if (string.IsNullOrEmpty(dtSOComplete.Rows[i]["ProductionOrderId"].ToString()))
            //            dtSOComplete.Rows[i]["NotAlotted"] = 1;
            //        dtSOComplete.Rows[i]["NotAlotted"] = 0;

                    

            //        if (Convert.ToDateTime(dtSOComplete.Rows[i]["AddedDate"].ToString()) >= newDate)
            //            dtSOComplete.Rows[i]["daysthree"] = 1;
            //        dtSOComplete.Rows[i]["daysthree"] = 0;

            //    }
            //}
            //var dd= Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtSOComplete);

            var data = os3.getSlabData(parameters, group, out List<Object> totalArr, out List<double[]> chart, value, analysis, type);
            return Json(new { DATA = data, Total = totalArr, Chart = chart }, JsonRequestBehavior.AllowGet);
        }

        private string GetDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            try
            {
                return Convert.ToDateTime(s).ToString("dd-MMM-yyyy");
            }
            catch (Exception)
            {
                return "";
            }
        }

        private DataRow GetExpectedSOCompletionDate(double RequiredQty, string POId, DataTable Data)
        {
            for (int i = 0; i < Data.Rows.Count; i++)
            {
                if (Data.Rows[i]["POId"].ToString() == POId)
                {

                    if (clsStaticInfo.dbl(Data.Rows[i]["CumProdQty"].ToString()) >= RequiredQty)
                    {
                        return Data.Rows[i];
                    }
                }
            }


            return null;
        }

        [HttpPost, Authorize]
        public ActionResult getClickData(Dictionary<string, string> parameters, string group, string col, string range, string analysis, string type, string entityId)
        {
            return Json(os3.getClickData(parameters, group, col, range, analysis, type, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getControlList(string pr)
        {
            return Json(os3.getControlList(pr), JsonRequestBehavior.AllowGet);
        }
    }

}
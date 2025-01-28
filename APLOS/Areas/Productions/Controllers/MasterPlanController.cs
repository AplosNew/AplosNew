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
using Library.Service.OrderManagements;
using System.Linq;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class MasterPlanController : BaseController
    {
        #region Constructor
       
        private readonly ISqlRepository _sqlRepository;
        public MasterPlanController(ISqlRepository R)
        {
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
            string str = @"SELECT EI.SystemId as SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionId
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and EI.EmployeeCode is not null";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcessList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select P.Id as Value,P.UserName as Text from [HKP].[Process] P where P.MasterPlanApplicable=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetMasterPlanFieldStatusList(string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select LineItem,SKU1,SKU2 from hkp.Process where Id='"+ ProcessId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEntityList(string ProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select E.Id as Value,E.UserName as Text from  hkp.EntityProcessTag EPT
left join ORG.Entity E ON E.Id=EPT.EntityId
where EPT.ProcessId='" + ProcessId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMasterPlanList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT * ,(select E.EmployeeName from EmployeeInformation E where E.SystemId=CP.UserId) as UserName,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=CP.ResponsiblePersonId) as ResponsiblePerson,
                            (select UserName from hkp.Process where id=Cp.ProcessId) as Process,(select UserName from org.entity where id=Cp.EntityId) as Entity FROM [MST].[MasterPlan] CP";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMasterPlanEditData(string MasterPlanId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select E.EmployeeName from EmployeeInformation E where E.SystemId=CP.UserId) as UserName,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=CP.ResponsiblePersonId) as ResponsiblePerson,
                           (select UserName from hkp.Process where id=Cp.ProcessId) as Process,(select UserName from org.entity where id=Cp.EntityId) as Entity FROM [MST].[MasterPlan] CP where CP.Id='" + MasterPlanId + @"'";
            return Json(new { MasterPlan = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetMasterPlanDetailsList(string ProcessId, string PlanId)
        {
            try
            {
                if (ProcessId == "null")
                {
                    throw new Exception("Please select Process and proceed..");
                }
                else
                {
                    string FilterPlan = string.Empty;
                    if (PlanId != "null" && PlanId != "undefined")
                    {
                        FilterPlan = " and SO.Id in (select SalesOrderId from [MST].[MasterPlanSODetails] where MasterPlanId='"+ PlanId + @"')";
                    }
                    else
                    {
                        FilterPlan = " and SO.Id Not in (select SalesOrderId from [MST].[MasterPlanSODetails])";
                    }
                    string sql = @"select isnull(MOI.ProductionGrouping,'') AS ProductionGrouping,MOI.OwnReferenceNo, isnull(PO.Id,'') AS PONumber,
PS.UserName ProductionStatus,OS.UserName AS OrderStatusName,SO.Id SONo,SO.Qty,isnull((select PlanPercentage from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id),
MO.ExtraOrderPercentage) PlanPercentage,isnull((select SOPlanQty from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id),SO.Qty + (MO.ExtraOrderPercentage*SO.Qty / 100)) as SOPlanQty,
(select PlanStatus from MST.MasterPlan where id=(select MasterPlanId from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id)) as MasterPlanStatus,
(case when (select MasterPlanId from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) is null then 0 else 1 end) IsMasterPlan,
(select MasterPlanId from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) as MasterPlanId,
(select Id from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) as Id,
(select Status from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) as Status,
MOI.MaterialMasterId, MM.UserName AS MaterialMasterName, MOI.ArticleId, 
ART.StandardName AS ArticleName,P.UserName AS Customer,MOI.BuyerReferenceNo,MOI.Id LineItemNo,SO.Id AS SalesOrderId,MO.MasterOrderNo,
E.UserName POEntity,PPS.JobWorkApplicable IsJW,PPS.JobWorkType JWType,(Case when PPS.JobWorkType='EntityWithinCompany' then (select UserName from ORG.Entity where Id=PPS.EntityIdWithinCompany) 
when PPS.JobWorkType='EntityWithinGroup' then (select UserName from ORG.Entity where Id=PPS.EntityIdWithinCompany)
when PPS.JobWorkType='Party' then (select UserName from hkp.Party where Id=PPS.PartyId) end ) EntityVendor
from TRN.ProductionOrder PO
left join TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderId=PO.Id
left join TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=PO.Id
left join TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
left join [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
left join [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
left join [HKP].[Party] AS P ON MO.PartyId = P.Id
left join [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
left join [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
left join [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
left join [TRN].[CustomerPO] AS CP ON SO.CustomerPOId = CP.Id
left join [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
left join [ORG].[Entity]  AS E ON E.Id=PO.EntityId
LEFT JOIN [MST].[MasterPlanSODetails] CPD on CPD.SalesOrderId=SO.Id and CPD.MasterPlanId='" + PlanId + @"'
where PPS.ProcessId = '" + ProcessId + @"'  " + FilterPlan + @"
and SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1)
and PO.ProductionStatusId in (select Id from HKP.ProductionStatus where MasterPlanApplicable=1)
ORDER BY MOI.ProductionGrouping,MOI.OwnReferenceNo";

                    return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult CreateData(Dictionary<string, object> data, List<Dictionary<string, object>> DataList)
        {
            SaveMasterPlanData(data, DataList, out string masterId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        public void SaveMasterPlanData(Dictionary<string, object> data, List<Dictionary<string, object>> DataList, out string masterId)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsId;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [MST].[MasterPlan] where PlanName='" + data["PlanName"] + "'", out DataSet dsMasterPlanNameValidation, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[MasterPlan] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    if (dsMasterPlanNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Plan Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MasterPlan", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[MasterPlanSODetails] WHERE MasterPlanId ='" + masterId + "'", out dsDetail, false, "1");
                con.OpenDataSetThroughAdapter("SELECT COUNT(Id)Id FROM [MST].[MasterPlanSODetails] WHERE MasterPlanId ='" + masterId + "'", out dsId, false, "1");

                int count = Convert.ToInt32(dsId.Tables[0].Rows[0]["Id"].ToString());


                foreach (var item in DataList)
                {

                    DataView dv = new DataView(dsDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        count++;

                        item["Id"] = masterId + "-" + count;
                        item["MasterPlanId"] = masterId;

                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["Status"].ToString()) == false)
                    {
                        DataRow drpb = dv[0].Row;
                        EditRow(drpb, item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);

            }
            catch (Exception ex)
            {
                throw (ex);
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

#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Data;
using System.Reflection;
using Library.Service.Logs;
using Library.Service.Processes;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class WasteIssueController : BaseController
    {
        private readonly IProcessService _processService;
        WasteMasterService ws = new WasteMasterService();
        string TableName = "dbo.WasteMaster";
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public WasteIssueController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }
        
        
        [Authorize, HttpPost]
        public ActionResult getProcess()
        {
            return Json(ws.getProcess(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getUOM()
        {
            return Json(ws.getUOM(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getBudget()
        {
            return Json(ws.getBudgetId(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM dbo.WasteMaster"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetWaste(string entityId,string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select Cast(0 as bit) Active,WID.Id,WTD.Id WasteTransactionDataId,WM.Sequence,WM.Category WasteCategory,WM.SubCategory WasteSubCategory,WM.ItemName WasteName
				                    ,UOM.UserName UOM,WTD.Quantity StockQty,WM.StandardRate StdRate,(WTD.Quantity*WM.StandardRate) StdValue
				                    ,ISNULL(WID.IssueQty,0) IssueQty,ISNULL(WID.Rate,0) Rate,WID.ProcessId,P.UserName Process,WID.Remarks,(ISNULL(WID.IssueQty,0) * ISNULL(WID.Rate,0))as IssueValue
									,ISNULL((WTD.Quantity-(WID.IssueQty+ISNULL(WIDS.OtherQty,0))),0) as BalanceStock
									,((ISNULL(WTD.Quantity,0)*ISNULL(WM.StandardRate,0))-(ISNULL(WID.IssueQty,0)*ISNULL(WID.Rate,0))) as BalanceStkValue,ISNULL(WIDS.OtherQty,0) OtherQty
				                    from WasteTransactionData WTD
				                    left join WasteMaster WM on WM.Id=WTD.WasteMasterId
				                    left join SCS.UnitOfMeasurement UOM on UOM.Id=WM.UOMId
									LEFT JOIN WasteIssueDetails WID ON WID.WasteTransactionDataId=WTD.Id AND WID.WasteIssueId='"+Id+@"'
									LEFT JOIN (select sum(IssueQty) OtherQty,WasteTransactionDataId,WasteIssueId from WasteIssueDetails group by WasteTransactionDataId,WasteIssueId) WIDS ON WIDS.WasteTransactionDataId=WTD.Id
									AND WIDS.WasteIssueId<> '"+Id+@"'

                                    left join HKP.Process P on P.Id=WID.ProcessId
				                    where EntityId='" + entityId + "' ";
              
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

       

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            return Json(ws.GetList(strkey), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
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

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> WasteData)
        {
            

            try
            {
                DataSet dsWasteDetail, dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from WasteIssue where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";
                string MasterId = string.Empty;
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "WasteIssue", out _Id);

                    data["Id"] = _Id;
                    MasterId = data["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    MasterId = _Id;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                string id = "";
                for (int i = 0; i < WasteData.Count; i++)
                {
                    if (id == "")
                    {
                        id = "'" + WasteData[i]["Id"] + "'";
                    }
                    else
                    {
                        id += ",'" + WasteData[i]["Id"] + "'";
                    }
                }

                con.OpenDataSetThroughAdapter("select * from WasteIssueDetails where Id in (" + id + ")", out dsWasteDetail, false, "1");

                string WasteId = "";
                for (int i = 0; i < WasteData.Count; i++)
                {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    dsWasteDetail.Tables[0].DefaultView.RowFilter = "Id='" + WasteData[i]["Id"] + @"'";
                    if (dsWasteDetail.Tables[0].DefaultView.Count > 0)
                    {
                        //edit
                        DataRow dr = dsWasteDetail.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["WasteIssueId"] = MasterId;
                        dr["WasteTransactionDataId"] = WasteData[i]["WasteTransactionDataId"];
                        dr["WasteMasterId"] = WasteData[i]["Id"];
                        dr["IssueQty"] = WasteData[i]["IssueQty"];
                        dr["Rate"] = WasteData[i]["Rate"];
                        dr["IssueValue"] = WasteData[i]["IssueValue"];
                        dr["ProcessId"] = WasteData[i]["ProcessId"];
                        dr["Remarks"] = WasteData[i]["Remarks"];

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                    else
                    {
                        //addnew

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("WasteIssueDetails", out WasteId);

                        DataRow dr = dsWasteDetail.Tables[0].NewRow();

                        dr["Id"] = "M-" + WasteId + "-" + (i + 1);
                        dr["WasteIssueId"] = MasterId;
                        dr["WasteTransactionDataId"] = WasteData[i]["WasteTransactionDataId"];
                        dr["IssueQty"] = WasteData[i]["IssueQty"];
                        dr["Rate"] = WasteData[i]["Rate"];
                        dr["IssueValue"] = WasteData[i]["IssueValue"];
                        dr["ProcessId"] = WasteData[i]["ProcessId"];
                        dr["Remarks"] = WasteData[i]["Remarks"];

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsWasteDetail.Tables[0].Rows.Add(dr);

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsWasteDetail);
                return Json(new { Error = false, Message = AplosMessage.Updated, Id = _Id });

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
                ws.Delete(id);

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }


        
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"Select e.Id as EntityId, e.UserName as EntityName , p.UserName as Plant, c.UserName as Company from org.Entity e
                                left join org.Plant p on p.Id = e.PlantId
                                left join org.Company c on c.Id = p.CompanyId";
           
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeListByWhom(GridParameter parameters, string plantId, string partyAccountGroupId, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(GetEmployeeListByWhom(parameters, identity.CompanyId, plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
        }

        public GridModel GetEmployeeListByWhom(GridParameter parameters, string companyId, string plantId, string partyAccountGroupId, string partyId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [Designation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.DesignationSystemID
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            WHERE EI.CompanyId='" + companyId + "' AND EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

    }
}
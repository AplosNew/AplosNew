using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.QMS.Controllers
{
    public class CustomerQualityAndTechnicalSupportController : Controller
    {
        private readonly SqlRepository _sqlRepository;

        public CustomerQualityAndTechnicalSupportController()
        {
            _sqlRepository = new SqlRepository();
        }

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetData()
        {
            string sql = @"select CQT.Id, P.UserName Party,format(CQT.ComplaintDate, 'dd-MMM-yyyy')ComplaintDate,format(CQT.ToCloseDate,'dd-MMM-yyyy')ToCloseDate, EI.EmployeeName ResponsiblePerson
,BW.EmployeeName ByWhom, CQT.ByWhomId, CQT.ResponsiblePersonId, CQT.CustomerId, CQT.ArticleId, MMA.StandardName MaterialArticle
from [TRN].[CustomerQATechSupport] CQT
LEFT JOIN EmployeeInformation EI on EI.SystemId = CQT.ResponsiblePersonId
left join EmployeeInformation BW on BW.SystemId = CQT.ByWhomId
left join HKP.Party p on P.Id = CQT.CustomerId 
left join MST.MaterialMasterArticle MMA on MMA.Id = CQT.ArticleId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetArticle(string salesId)
        {
            try
            {
                string sql = @"select distinct MMA.Id Value, MMA.StandardName Text from MST.MaterialMasterArticle MMA
left join TRN.SalesMaterial SM on SM.ArticleId = MMA.Id
left join TRN.Sales S on S.Id = SM.SalesId and S.AddedDate between dateadd(year,datediff(year,0,getdate()),0)
and  dateadd(day,-1,dateadd(year,datediff(year,-1,getdate()),0))
where S.PartyId = '"+ salesId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public JsonResult GetInvoiceNumber(string articleId)
        {
            try
            {
                //                string sql = @"select * from TRN.SalesMaterial SM
                //left join TRN.Sales on Sales.Id  = SM.SalesId
                //where SM.ArticleId = '"+ articleId + "' ";
                string sql = @"select '' Id, S.Id InvoiceId, s.InvoiceNo, S.InvoiceDate, MA.StandardName Article, SM.ArticleId, PO.Qty POQuantity from TRN.Sales S
                                left join TRN.SalesMaterial SM on SM.SalesId = S.Id
left join MST.MaterialMasterArticle MA on MA.Id = SM.ArticleId
left join TRN.SalesOrder SO on SO.Id = SM.SalesOrderId
left join TRN.ProductionOrderDetail POD on POD.SalesOrderId = SO.Id
left join TRN.ProductionOrder PO on PO.Id = POD.ProductionOrderId
where SM.ArticleId = '" + articleId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public GridModel GetEmployeeListByWhom(GridParameter parameters, string companyId, string plantId, string partyAccountGroupId, string partyId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId, MB.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [Designation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.GivenDesignationID
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

        public JsonResult GetMultiInvoiceNo()
        {
            try
            {
                string sql = @"select s.InvoiceNo, S.InvoiceDate, MA.StandardName, SM.ArticleId from TRN.Sales S
                                left join TRN.SalesMaterial SM on SM.SalesId = S.Id
                                left join MST.MaterialMasterArticle MA on MA.Id = SM.ArticleId";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public JsonResult GetComplaint()
        {
            string sql = @"select Id Value, UserName Text from HKP.ComplaintMaster";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCustomerStatus()
        {
            string sql = @"select ''Id, Id Value, UserName Text from [HKP].[CustomerQtyTechSupportStatus]";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(Dictionary<string, object> datas)
        {
            try
            {

                string TableName = "[TRN].[CustomerQATechSupport]";
                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");



                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + datas["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    datas["Id"] = _Id;

                    AddNewRow(dsMaster.Tables[0], datas);
                }
                else
                {
                    _Id = datas["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Data = datas, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public void SaveData(Dictionary<string, object> actiontakenObj, string headerid, out string contId, List<Dictionary<string, object>> invoicelist)
        {
            string TableName = "TRN.HRReportMasterChild";
            
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsMaster, dsChild;
            string id = string.Empty;

            string _Id = "";
            string _UserGroupId = string.Empty;


            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.OpenDataSetThroughAdapter("select * from [TRN].[CustomerQATechInvoice]  where CustomerQATechSupportId = '" + headerid + "'", out dsChild, false, "1");
                foreach (var item in invoicelist)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["CustomerQATechSupportId"] = headerid;
                        dr["InvoiceId"] = item["InvoiceId"];
                        dr["ComplaintId"] = item["ComplaintId"];
                        dr["IsActive"] = 0;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.CustomerQATechInvoice", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["CustomerQATechSupportId"] = headerid;
                        dr["InvoiceId"] = item["InvoiceId"];
                        dr["ComplaintId"] = item["CompaintId"];
                        dr["IsActive"] = item["isSelected"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }
                contId = dsChild.Tables[0].Rows[0]["Id"].ToString();
                string sql = "SELECT * FROM [TRN].[CustomerQATechSupportStatus] WHERE CustomerQATechInvoiceId='" + contId + "'";

                
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                

                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    actiontakenObj["Id"] = _Id;                   
                    actiontakenObj["CustomerQATechInvoiceId"] = contId;                   

                    AddNewRow(dsMaster.Tables[0], actiontakenObj);

                }
                else
                {
                  
                    EditRow(dsMaster.Tables[0].Rows[0], actiontakenObj);
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> actiontakenObj, string headerid, List<Dictionary<string, object>> invoicelist)
        {
            try
            {
                SaveData(actiontakenObj, headerid, out string contractId, invoicelist);


                return Json(new { Id = contractId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        #region AddEdit
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
            dr["AddedDate"] = DateTime.Now.ToString();
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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

      
        #endregion AddEdit
    }

    
}
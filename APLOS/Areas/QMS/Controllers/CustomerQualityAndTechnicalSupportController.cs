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
                string sql = @"select s.InvoiceNo, S.InvoiceDate, MA.StandardName Article, SM.ArticleId, PO.Qty POQuantity from TRN.Sales S
                                left join TRN.SalesMaterial SM on SM.SalesId = S.Id
left join MST.MaterialMasterArticle MA on MA.Id = SM.ArticleId
left join TRN.SalesOrder SO on SO.Id = SM.SalesOrderId
left join TRN.ProductionOrderDetail POD on POD.SalesOrderId = SO.Id
left join TRN.ProductionOrder PO on PO.Id = POD.ProductionOrderId";
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

        public JsonResult GetCoplaint()
        {
            string sql = @"select Id Value, UserName Text from HKP.ComplaintMaster";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(Dictionary<string, object> datas)
        {
            try
            {

                string TableName = "HKP.HRReportMaster";
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

        public void SaveData(Dictionary<string, object> chkBgtList, string headerId, out string contId, List<Dictionary<string, object>> usergroup)
        {
            string TableName = "TRN.HRReportMasterChild";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsMaster, dsChild;


            try
            {

                string sql = "SELECT * FROM [TRN].[HRReportMasterChild] WHERE Id='" + chkBgtList["Id"] + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                string id = string.Empty;

                string _Id = "";
                string _UserGroupId = string.Empty;

                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    chkBgtList["Id"] = _Id;
                    chkBgtList["HRReportMasterId"] = headerId;
                    chkBgtList["Active"] = chkBgtList["isSelected"];


                    AddNewRow(dsMaster.Tables[0], chkBgtList);

                }
                else
                {
                    chkBgtList["Active"] = 0;
                    EditRow(dsMaster.Tables[0].Rows[0], chkBgtList);
                }

                contId = dsMaster.Tables[0].Rows[0]["Id"].ToString();



                objCon.OpenDataSetThroughAdapter("select * from TRN.HRReportMasterBudgetUserGroup  where HRReportMasterChildId = '" + contId + "'", out dsChild, false, "1");
                foreach (var item in usergroup)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["Grade"] = item["Grade"];

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.HRReportMasterBudgetUserGroup", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["HRReportMasterChildId"] = contId;
                        dr["UserGroupId"] = item["UserGroupId"];
                        dr["Grade"] = item["Grade"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
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
        public JsonResult Create(Dictionary<string, object> chkBgtList, string headerId, List<Dictionary<string, object>> usergroup)
        {
            try
            {
                SaveData(chkBgtList, headerId, out string contractId, usergroup);


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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.HRReportMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        #endregion AddEdit
    }
}
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
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
    }
}
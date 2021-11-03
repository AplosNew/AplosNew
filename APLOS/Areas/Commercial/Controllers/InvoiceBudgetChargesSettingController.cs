#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Employees;
using Library.Crosscutting.Security;
using System.Threading;
using System;
using Library.Service.Systems;
using System.Collections.Generic;
using Library.Service.Enums;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using OTSBD;
using System.Data;
using Library.Data.Repositories;
using Library.ViewModel.Setup;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class InvoiceBudgetChargesSettingController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public InvoiceBudgetChargesSettingController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InvoiceBudgetChargesSetting", out idFromDB);
            systemID = idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        [HttpGet]
        public ActionResult GetList(string companyId)
        {
            var sql = @"Select I.Id,I.CompanyId,I.Process,I.Type,I.ExpenseTypeId,I.DrGLGeneralInfoId,I.DrActivityId,I.DrBudgetMasterId,
                        I.CrGLGeneralInfoId,I.CrActivityId,I.CrBudgetMasterId,I.Days,I.PaymentTerms,I.EstimatedPercentageValue,I.EstimatedMaxValue
                        ,I.AddedBy,FORMAT(I.AddedDate,'dd-MMM-yyyy')AddedDate,I.AddedFromIP,I.UpdatedBy,FORMAT(I.UpdatedDate,'dd-MMM-yyyy')UpdatedDate,I.UpdatedFromIP
						, GLGIDr.AccountCode AS DrGLGeneralInfoCode, GLGIDr.UserName AS DrGLGeneralInfoName
                                   , B.BudgetCode DrBudgetCode, B.BudgetName DrBudgetName, A.ActivityCode DrActivityCode, A.ActivityName DrActivityName
						, GLGICr.AccountCode AS CrGLGeneralInfoCode, GLGICr.UserName AS CrGLGeneralInfoName
                                    ,C.BudgetCode CrBudgetCode, C.BudgetName CrBudgetName, F.ActivityCode CrActivityCode, F.ActivityName CrActivityName,O.UserName ExpenseType
                        FROM dbo.InvoiceBudgetChargesSetting I 
						LEFT JOIN [HKP].[GLGeneralInfo] AS GLGIDr ON GLGIDr.Id=I.DrGLGeneralInfoId
						LEFT JOIN [HKP].[GLGeneralInfo] AS GLGICr ON GLGICr.Id=I.CrGLGeneralInfoId

						LEFT JOIN (SELECT BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.GLGeneralInfoId, BM.RefNo
	                                    FROM [HKP].[Budget] AS B
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=B.Id
                                    ) AS B ON B.GLGeneralInfoId=GLGIDr.Id
                        LEFT JOIN (SELECT A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, BA.BudgetMasterId
	                                    FROM [HKP].[Activity] AS A
	                                    LEFT JOIN [MST].[BudgetMasterActivity] AS BA ON BA.ActivityId=A.Id
                                    ) AS A ON A.BudgetMasterId=B.BudgetMasterId

                        LEFT JOIN (SELECT BM.Id AS BudgetMasterId, E.Code AS BudgetCode, E.UserName AS BudgetName, BM.GLGeneralInfoId, BM.RefNo
	                                    FROM [HKP].[Budget] AS E
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=E.Id
                                    ) AS C ON C.GLGeneralInfoId=GLGICr.Id
                        LEFT JOIN (SELECT D.Id AS ActivityId, D.Code AS ActivityCode, D.UserName AS ActivityName, BA.BudgetMasterId
	                                    FROM [HKP].[Activity] AS D
	                                    LEFT JOIN [MST].[BudgetMasterActivity] AS BA ON BA.ActivityId=D.Id
                                    ) AS F ON F.BudgetMasterId=C.BudgetMasterId
                        LEFT JOIN [HKP].[OverHeadType] AS O ON O.Id=I.ExpenseTypeId
                        WHERE I.CompanyId='" + companyId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(InvoiceBudgetChargesSetting entity)
        {
            try
            {
                SaveData(entity);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }

        private void SaveData(InvoiceBudgetChargesSetting item)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (item != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    string sql = "SELECT * FROM [dbo].[InvoiceBudgetChargesSetting] WHERE Id='" + item.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {

                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = GetPK();

                        dr["Process"] = item.Process;
                        dr["CompanyId"] = item.CompanyId;
                        dr["Type"] = item.Type;
                        dr["ExpenseTypeId"] = item.ExpenseTypeId;
                        dr["DrGLGeneralInfoId"] = item.DrGLGeneralInfoId;
                        dr["DrActivityId"] = item.DrActivityId;
                        dr["DrBudgetMasterId"] = item.DrBudgetMasterId;
                        dr["CrGLGeneralInfoId"] = item.CrGLGeneralInfoId;
                        dr["CrActivityId"] = item.CrActivityId;
                        dr["CrBudgetMasterId"] = item.CrBudgetMasterId;
                        dr["Days"] = item.Days;
                        dr["PaymentTerms"] = item.PaymentTerms;
                        dr["EstimatedPercentageValue"] = item.EstimatedPercentageValue;
                        dr["EstimatedMaxValue"] = item.EstimatedMaxValue;

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

                        dr["Process"] = item.Process;
                        dr["CompanyId"] = item.CompanyId;
                        dr["Type"] = item.Type;
                        dr["ExpenseTypeId"] = item.ExpenseTypeId;
                        dr["DrGLGeneralInfoId"] = item.DrGLGeneralInfoId;
                        dr["DrActivityId"] = item.DrActivityId;
                        dr["DrBudgetMasterId"] = item.DrBudgetMasterId;
                        dr["CrGLGeneralInfoId"] = item.CrGLGeneralInfoId;
                        dr["CrActivityId"] = item.CrActivityId;
                        dr["CrBudgetMasterId"] = item.CrBudgetMasterId;
                        dr["Days"] = item.Days;
                        dr["PaymentTerms"] = item.PaymentTerms;
                        dr["EstimatedPercentageValue"] = item.EstimatedPercentageValue;
                        dr["EstimatedMaxValue"] = item.EstimatedMaxValue;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
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

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.InvoiceBudgetChargesSetting WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
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
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        #endregion
    }

    public class InvoiceBudgetChargesSetting : BaseModel
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Process { get; set; }
        public string Type { get; set; }
        public string ExpenseTypeId { get; set; }
        public string DrGLGeneralInfoId { get; set; }
        public string DrBudgetMasterId { get; set; }
        public string DrActivityId { get; set; }
        public string CrGLGeneralInfoId { get; set; }
        public string CrBudgetMasterId { get; set; }
        public string CrActivityId { get; set; }
        public int Days { get; set; }
        public string PaymentTerms { get; set; }
        public decimal EstimatedPercentageValue { get; set; }
        public decimal EstimatedMaxValue { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}
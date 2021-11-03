#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.ViewModel.Invoices;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Vouchers;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Organizations.Controllers
{
    public class DesignationBudgetController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public DesignationBudgetController(IUnitOfWork U
            , ISqlRepository R
            , IInvoiceWriteOffService invoiceWriteOffService
            , IInvoiceReportService invoiceReportService
            )
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations


        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"
                        SELECT MB.Code,LG.UserName LegalDesignation,DB.*  FROM [dbo].[DesignationBudget] DB
                        LEFT JOIN MST.ManpowerBudget MB ON MB.Id=DB.BudgetCodeId
                        LEFT JOIN HKP.LegalDesignation LG ON LG.Id=DB.LegalDesignationId ORDER BY LG.UserName";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool CheckDesignationBudgetCombination(Dictionary<string, object> data)
        {
            try
            {
                string _sql = "";

                _sql = @"SELECT * FROM [dbo].[DesignationBudget] where BudgetCodeId='" + data["BudgetCodeId"] + "' AND LegalDesignationId='" + data["LegalDesignationId"] + "' AND  id<>'" + data["Id"] + "' AND Activity='"+data["Activity"] +"' ";

                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    var IsDuplicateEntryAllowed = CheckDesignationBudgetCombination(data);
                    if (!IsDuplicateEntryAllowed)
                    {
                        throw new Exception("This combiation has been taken...");
                    }

                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[DesignationBudget] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                    string _Id = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("DesignationBudget", out _Id);

                        data["Id"] = "MB" + _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data update

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpPost]
        public JsonResult Edit(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    var IsDuplicateEntryAllowed = CheckDesignationBudgetCombination(data);
                    if (!IsDuplicateEntryAllowed)
                    {
                        throw new Exception("This combiation has been taken...");
                    }

                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[DesignationBudget] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                    string _Id = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("DesignationBudget", out _Id);

                        data["Id"] = "MB" + _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data update

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
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
                strSQL = "DELETE FROM dbo.DesignationBudget WHERE Id = '" + Id + "'";
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


}
#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Enums;
using Library.Service.Payrolls;
using Library.Service.Properties;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class ContractualEmployeeCodeController : BaseController
    {
        string TableName = "dbo.AccountsGroup";
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public ContractualEmployeeCodeController(IUnitOfWork U, ISqlRepository R)
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

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetContractCodeList(string Level)
        {
            if (Level == "CompanyGroup")
            {
                return Json(_sqlRepository.GetDataCollection(@"Select EC.Id,CG.Id CompanyGroupId,CG.UserName CompanyGroup, EC.StartValue from ORG.CompanyGroup CG
LEFT JOIN ContractualEmployeeCode EC ON EC.CompanyGroupId = CG.Id Where ISNULL(EC.EmployeeCodeLevel,'" + Level + "')='" + Level + "'"), JsonRequestBehavior.AllowGet);
            }
            else if (Level == "Company")
            {
                return Json(_sqlRepository.GetDataCollection(@"Select EC.Id,CG.Id CompanyGroupId,CG.UserName CompanyGroup,C.Id CompanyId,C.UserName Company, EC.StartValue from ORG.Company C
LEFT JOIN ORG.CompanyGroup CG ON CG.Id=C.CompanyGroupId 
LEFT JOIN ContractualEmployeeCode EC ON EC.CompanyId=C.Id
Where ISNULL(EC.EmployeeCodeLevel,'" + Level + "')='" + Level + "'"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(_sqlRepository.GetDataCollection(@"Select EC.Id,CG.Id CompanyGroupId,CG.UserName CompanyGroup,C.Id CompanyId,C.UserName Company,P.Id PlantId,P.UserName Plant, EC.StartValue from ORG.Plant P
LEFT JOIN ORG.Company C ON C.Id=P.CompanyId
LEFT JOIN ORG.CompanyGroup CG ON CG.Id=C.CompanyGroupId
LEFT JOIN ContractualEmployeeCode EC ON EC.PlantId=P.Id Where ISNULL(EC.EmployeeCodeLevel,'" + Level + "')='" + Level + "'"), JsonRequestBehavior.AllowGet);
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ContractualEmployeeCode", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult Create(List<Dictionary<string, object>> data, string Level)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ContractualEmployeeCode", out dsMaster, false, "1");
                string _Id = GetPK();
                int count = 0;
                foreach (var item in data)
                {
                    count++;
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = _Id+count;
                        item["EmployeeCodeLevel"] = Level;

                        AddNewRow(dsMaster.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Message = AplosMessage.Updated });

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
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
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
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }
    }
}
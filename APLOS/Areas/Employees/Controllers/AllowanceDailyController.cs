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
    public class AllowanceDailyController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public AllowanceDailyController(IUnitOfWork U, ISqlRepository R)
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

        [HttpGet, Authorize]
        public ActionResult GetAutoSequence()
        {
            string sql = @"SELECT ISNULL((MAX(Sequence)+1 ),1) Sequence FROM [HKP].[AllowanceDaily]";
            return Json(_sqlRepository.GetModelCollection<AllowanceDailyModel>(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList()
        {
            string sql = @"SELECT * FROM [HKP].[AllowanceDaily] ORDER BY [Sequence]";
            return Json(_sqlRepository.GetModelCollection<AllowanceDailyModel>(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var sql = @"SELECT Id, UserName FROM [HKP].[AllowanceDaily] ORDER BY UserName";
            return Json(_sqlRepository.GetCombo(sql, "Id", "UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AllowanceDailyModel entity)
        {
            try
            {
                SaveData(entity);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string Id = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AllowanceDaily", out idFromDB);
            Id = "AD" + idFromDB;
            sID = Id.Trim();
            return sID;

        }

        private DataSet CheckCode(string Code, string id)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT Code FROM [HKP].[AllowanceDaily] WHERE Code='" + Code + "' and Id <>'" + id + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;

        }
        private DataSet CheckUserName(string UserName, string id)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT UserName FROM [HKP].[AllowanceDaily] WHERE UserName='" + UserName + "' and Id <>'" + id + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;

        }
        private void SaveData(AllowanceDailyModel data)
        {
            var code = CheckCode(data.Code, data.Id);
            if (code.Tables[0].Rows.Count > 0)
            {
                throw new Exception("Code already exist.");
            }

            var UserName = CheckUserName(data.UserName, data.Id);
            if (UserName.Tables[0].Rows.Count > 0)
            {
                throw new Exception("UserName already exist.");
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [HKP].[AllowanceDaily] WHERE ID='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetPK();
                    dr["Code"] = data.Code;
                    dr["ShortName"] = data.ShortName;
                    dr["StandardName"] = data.StandardName;
                    dr["UserName"] = data.UserName;
                    dr["Description"] = data.Description;
                    dr["Remarks"] = data.Remarks;
                    dr["Sequence"] = data.Sequence;
                    dr["Active"] = data.Active;
                    dr["SalaryHeadId"] = data.SalaryHeadId;
                    dr["FormulaDescription"] = data.FormulaDescription;
                    dr["FormulaDesID"] = data.FormulaDesID;
                    dr["CalculationBasics"] = data.CalculationBasics;
                    dr["Catagory"] = data.Catagory;
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

                    dr["Code"] = data.Code;
                    dr["Sequence"] = data.Sequence;
                    dr["ShortName"] = data.ShortName;
                    dr["StandardName"] = data.StandardName;
                    dr["UserName"] = data.UserName;
                    dr["Description"] = data.Description;
                    dr["Remarks"] = data.Remarks;
                    dr["Active"] = data.Active;
                    dr["SalaryHeadId"] = data.SalaryHeadId;
                    dr["FormulaDescription"] = data.FormulaDescription;
                    dr["FormulaDesID"] = data.FormulaDesID;
                    dr["CalculationBasics"] = data.CalculationBasics;
                    dr["Catagory"] = data.Catagory;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
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
                strSQL = "DELETE FROM [HKP].[AllowanceDaily] WHERE Id = '" + Id + "'";
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
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function
       
        [HttpGet, Authorize]
        public JsonResult GetSalaryHeadCbo()
        {
            var sql = @"SELECT DISTINCT H.SalaryHeadID, H.SalaryHead,HasGL=CASE WHEN G.Id IS NOT NULL THEN 1 ELSE 0 END FROM SalaryHead H
LEFT JOIN MST.SalaryHeadGL G ON G.SalaryHeadID=H.SalaryHeadID
ORDER BY H.SalaryHead";
            return Json(_sqlRepository.GetCombo(sql, "SalaryHeadID", "SalaryHead"), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }


    public class AllowanceDailyModel : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string SalaryHeadId { get; set; }
        public bool Active { get; set; }
        public string FormulaDescription { get; set; }
        public string FormulaDesID { get; set; }
        public string CalculationBasics { get; set; }
        public string Catagory { get; set; }
        [NeverUpdate]
        public string AddedBy { get; set; }

        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        [NeverUpdate]
        public string AddedFromIP { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        #endregion
    }
}
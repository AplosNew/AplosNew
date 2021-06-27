#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Payrolls;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeWorkTypeController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public EmployeeWorkTypeController(IUnitOfWork U, ISqlRepository R)
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
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT ISNULL((MAX(Sequence)+1 ),1) Sequence FROM [dbo].[EmployeeWorkType]";
            return Json(_sqlRepository.GetModelCollection<EmployeeWorkType>(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM [dbo].[EmployeeWorkType] ORDER BY [Sequence]";
            return Json(_sqlRepository.GetModelCollection<EmployeeWorkType>(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var sql = @"SELECT Id AS Value, UserName AS Text FROM EmployeeWorkType ORDER BY UserName";
            return Json(_sqlRepository.GetCombo(sql, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EmployeeWorkType entity)
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
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(EmployeeWorkType), out sID);
            return sID;

        }

        private void SaveData(EmployeeWorkType data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {

                string sql = "SELECT * FROM [dbo].[EmployeeWorkType] WHERE Id='" + data.Id + "'";
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
            return Json(new {Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.EmployeeWorkType WHERE Id = '" + Id + "'";
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
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        #endregion
    }

    public class EmployeeWorkType : BaseModel
    {
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool Active { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}
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

namespace Aplos.Areas.Payrolls.Controllers
{
    public class SalaryHeadController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public SalaryHeadController(IUnitOfWork U, ISqlRepository R)
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
            string sql = @"SELECT (ISNULL((MAX(ISNULL(Sequence,0))),0)+1) Sequence FROM [dbo].[SalaryHead] Where GroupID='" + identity .CompanyGroupId+ "'";
            return Json(_sqlRepository.GetModelCollection<SalaryHeadModel>(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT SalaryHeadID
                                 ,SalaryHead
                                 ,Description
                                 ,HeadType = CASE WHEN HeadType = 'D' THEN 'Deduction' 
                           				  WHEN HeadType = 'E' THEN 'Earning'  ELSE '' END
                                 ,HeadCategory
                                 ,ISNULL(ExtDataUpload, 0) ExtDataUpload
                                 ,GroupID
                                 ,AddedBy
                           	     ,FORMAT(DateAdded,'dd-MMM-yyyy') DateAdded
                                 ,UpdatedBy
                           	     ,FORMAT(DateUpdated,'dd-MMM-yyyy')
                                 ,Sequence
                                 ,PartOfNetPay
                                ,IsGrossComponent
                                ,IsRetained
                                ,TransactionType,IsApplicableInFinalSettlement
                             FROM [dbo].[SalaryHead] WHERE GroupID = '" + identity .CompanyGroupId+ @"'  ORDER BY [Sequence]";
            return Json(_sqlRepository.GetModelCollection<SalaryHeadModel>(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSalaryHeadCategoryCbo()
        {
            var sql = @"SELECT SystemId, SalaryHeadCategory FROM SalaryHeadCategory ORDER BY SalaryHeadCategory";
            return Json(_sqlRepository.GetCombo(sql, "SalaryHeadCategory", "SalaryHeadCategory"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SalaryHeadModel entity)
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

        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SALARYHEAD", out idFromDB);
            systemID = "SHD" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private void SaveData(SalaryHeadModel data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string headType = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                string sql = "SELECT * FROM [dbo].[SalaryHead] WHERE SalaryHeadID='" + data.SalaryHeadID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (data.HeadType == "Earning")
                {
                    headType = "E";
                }
                else if (data.HeadType == "Deduction")
                {
                    headType = "D";
                }
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["SalaryHeadID"] = GetPK();
                    dr["SalaryHead"] = data.SalaryHead;
                    dr["Description"] = data.Description;
                    dr["HeadType"] = headType;
                    dr["HeadCategory"] = data.HeadCategory;
                    dr["ExtDataUpload"] = data.ExtDataUpload;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;
                    dr["Sequence"] = data.Sequence;
                    dr["PartOfNetPay"] = data.PartOfNetPay;
                    dr["IsGrossComponent"] = data.IsGrossComponent;
                    dr["IsRetained"] = data.IsRetained;
                    dr["TransactionType"] = data.TransactionType;
                    dr["IsApplicableInFinalSettlement"] = data.IsApplicableInFinalSettlement;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["SalaryHead"] = data.SalaryHead;
                    dr["Description"] = data.Description;
                    dr["HeadType"] = headType;
                    dr["HeadCategory"] = data.HeadCategory;
                    dr["ExtDataUpload"] = data.ExtDataUpload;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["Sequence"] = data.Sequence;
                    dr["PartOfNetPay"] = data.PartOfNetPay;
                    dr["IsGrossComponent"] = data.IsGrossComponent;
                    dr["IsRetained"] = data.IsRetained;
                    dr["TransactionType"] = data.TransactionType;
                    dr["IsApplicableInFinalSettlement"] = data.IsApplicableInFinalSettlement;
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

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

        public void DeleteData(string strSalaryHeadID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM SalaryHead WHERE SalaryHeadID = '" + strSalaryHeadID + "'";
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

        #endregion
    }

    public class SalaryHeadModel : BaseModel
    {
        public string SalaryHeadID { get; set; }
        public string SalaryHead { get; set; }
        public string Description { get; set; }
        public string HeadType { get; set; } 
        public string HeadCategory { get; set; }
        public bool ExtDataUpload { get; set; }
        public string GroupID { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsCTCComponent { get; set; }
        public bool IsGrossComponent { get; set; }
        public bool IsApplicableInFinalSettlement { get; set; }
        public bool IsRetained { get; set; }
        public decimal Sequence { get; set; }
        public bool PartOfNetPay { get; set; }
        public string TransactionType { get; set; }
    }
}
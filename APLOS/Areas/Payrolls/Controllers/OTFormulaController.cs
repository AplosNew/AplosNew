#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
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
    public class OTFormulaController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public OTFormulaController(IUnitOfWork U, ISqlRepository R)
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
        public JsonResult GetAutoSequence(string CompanyId)
        {
            return Json(GetSequence(CompanyId), JsonRequestBehavior.AllowGet);
        }

        private double GetSequence(string CompanyId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM dbo.OTFormula Where CompanyId='"+ CompanyId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string CompanyId)
        {
           
            string sql = @"select N.*,P.UserName Company from [dbo].[OTFormula] N
                        LEFT JOIN ORG.Company P ON P.Id=N.CompanyId Where N.CompanyId='" + CompanyId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(string OTFormulaId)
        {
           
            string sql = @"SELECT D.Sequence,D.SalaryHeadID
                        ,SalaryHead= CASE WHEN ISNULL(SD.SalaryHead,'')<>'' THEN SD.SalaryHead ELSE D.Component END,D.Component,D.OTFormulaId
                        FROM [dbo].[FormulaDetail] D
                        LEFT JOIN dbo.SalaryHead SD ON SD.SalaryHeadID=D.SalaryHeadID
                        WHERE OTFormulaId='"+ OTFormulaId + @"' Order By D.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(OTFormula data, IEnumerable<NoticePeriodFormulaDetail> details)
        {
            try
            {
                SaveOTFormulaData(data, details);
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
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OTFormula", out sID);
            return sID;
        }


        private void SaveOTFormulaData(OTFormula data, IEnumerable<NoticePeriodFormulaDetail> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {

                    DataSet dsMaster, dsDestination;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.OTFormula WHERE Id='" + data.Id + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where OTFormulaId='" + data.Id + "'", out dsDestination, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {

                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = "OTF" + GetPK();
                        dr["CompanyId"] = data.CompanyId;
                        dr["Sequence"] = data.Sequence;
                        dr["Code"] = data.Code;
                        dr["ShortName"] = data.ShortName;
                        dr["StandardName"] = data.StandardName;
                        dr["UserName"] = data.UserName;
                        dr["Description"] = data.Description;
                        dr["Remarks"] = data.Remarks;
                        dr["Active"] = data.Active;

                        dr["FormulaDes"] = data.FormulaDes;
                        dr["FormulaDesID"] = data.FormulaDesID;

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

                        dr["CompanyId"] = data.CompanyId;

                        dr["Sequence"] = data.Sequence;
                        dr["Code"] = data.Code;
                        dr["ShortName"] = data.ShortName;
                        dr["StandardName"] = data.StandardName;
                        dr["UserName"] = data.UserName;
                        dr["Description"] = data.Description;
                        dr["Remarks"] = data.Remarks;
                        dr["Active"] = data.Active;

                        dr["FormulaDes"] = data.FormulaDes;
                        dr["FormulaDesID"] = data.FormulaDesID;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }

                   string  _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                    #region NoticePeriodFormulaDetail 
                    DataRow drF;
                    while (dsDestination.Tables[0].DefaultView.Count > 0)
                        dsDestination.Tables[0].DefaultView[0].Delete();

                    int count = 0;
                    if (details != null)
                    {
                      
                        foreach (var item in details)
                        {
                            drF = dsDestination.Tables[0].NewRow();
                            count++;
                            string pk = _Id + "_" + count;
                            drF["Id"] = pk;
                            drF["OTFormulaId"] = _Id;
                            drF["Sequence"] = item.Sequence;
                            drF["SalaryHeadID"] = item.SalaryHeadID;
                            drF["Component"] = item.Component;

                            dsDestination.Tables[0].Rows.Add(drF);
                        }

                    }
                    #endregion NoticePeriodFormulaDetail 

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsDestination);


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

        public void DeleteData(string SystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.OTFormula WHERE Id = '" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                
                objCon.ExecuteNonQueryWrapper("Delete [dbo].FormulaDetail where OTFormulaId= '" + SystemID + "'", true, "1");
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

    public class OTFormula
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string FormulaDes { get; set; }
        public string FormulaDesID { get; set; }

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
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

    }


}
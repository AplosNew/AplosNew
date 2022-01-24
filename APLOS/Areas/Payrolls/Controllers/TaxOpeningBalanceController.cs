using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Payrolls.Setting;
using static Library.Service.Payrolls.Setting.clsCurrencyRule;
using Library.HumanResource.Payroll.Setting;
using Library.HumanResource.Payroll.Tax;
using System.Reflection;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Web;
using Aplos.Helpers;
using System.IO;
using Library.Service.Helpers;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class TaxOpeningBalanceController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeProfileService _employeeProfileService;
        
        TaxOpeningBalanceService tob = new TaxOpeningBalanceService();


        public TaxOpeningBalanceController(ISqlRepository R, IEmployeeProfileService employeeProfileService)
        {
            _sqlRepository = R;
            _employeeProfileService = employeeProfileService;
            tob = new TaxOpeningBalanceService();
        }
       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region 
        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(tob.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxYear()
        {
            try
            {
                TaxPolicyMasterService tm = new TaxPolicyMasterService();
                return Json(tm.getTaxYearList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetTabValue(string Doj, string TaxYeadId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetTab(Doj, TaxYeadId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxType()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(tob.GetIncomeTaxType(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

      

     
              
        [HttpPost, Authorize]
        public ActionResult GetList(string TaxYear, string TaxType, string empid)
        {
            try
            {
                if (string.IsNullOrEmpty(TaxYear))
                {
                    throw new Exception("No Tax Year is selected...");
                }
                if (string.IsNullOrEmpty(TaxType))
                {
                    throw new Exception("No Tax Type is selected...");
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                TaxOB ep = new TaxOB();
                return Json(ep.GetList(TaxYear, TaxType, empid), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion

     
        #region

        [HttpPost, Authorize]
        public ActionResult UploadAttachment(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\\", "");
                DataTable AdditionalData = CustomJsonResult.ToDataTable(UploadDefault_data);


                AdditionalData.Rows[0]["Id"] = AdditionalData.Rows[0]["Id"].ToString().Replace("\"", "");
                if (string.IsNullOrEmpty(AdditionalData.Rows[0]["Id"].ToString()))
                    throw new Exception("Save the item first");



                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                foreach (var file in UploadDefault)
                {

                    string _Id = "IncomeTax_" + AdditionalData.Rows[0]["Id"].ToString();

                    var fileName = Path.GetFileName(_Id + new FileInfo(file.FileName).Extension);
                    var destinationPath = Path.Combine(ResourcesPathReader.TaxOpeningBalancePath(), _Id + new FileInfo(file.FileName).Extension);

                    if (System.IO.Directory.Exists(ResourcesPathReader.TaxOpeningBalancePath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.TaxOpeningBalancePath());
                        }
                        catch (Exception ex)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from " + AdditionalData.Rows[0]["TableName"] + " where Id='" + AdditionalData.Rows[0]["Id"].ToString() + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();




                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        #region Task data update
                        if (dsLocal.Tables[0].Rows[0]["FileName"].ToString() != "")
                        {
                            //try to delete the existing file
                            try
                            {
                                var _Path = Path.Combine(ResourcesPathReader.GetToDoPath(), dsLocal.Tables[0].Rows[0]["FileName"].ToString());
                                if (System.IO.File.Exists(_Path))
                                    System.IO.File.Delete(_Path);
                            }
                            catch (Exception)
                            {

                            }

                        }

                        DataRow dr = dsLocal.Tables[0].Rows[0];

                        dr.BeginEdit();

                        dr["FileName"] = fileName;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();


                        #endregion data update





                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id /*taxyear, string taxtype,string empsysteid*/)
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("select FileName from IncomeTaxItemTransaction  where Id='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetTaxableIncomeFileInfo(string Id )
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("select FileName from TaxableIncomeparameter  where Id='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult DeleteFile(string Id, string TableName)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + "  where Id='" + Id + "'", out dsMaster, false, "1");



                var destinationPath = Path.Combine(ResourcesPathReader.TaxOpeningBalancePath(), dsMaster.Tables[0].Rows[0]["FileName"].ToString());
                if (System.IO.File.Exists(destinationPath))
                    System.IO.File.Delete(destinationPath);

                #region Task data update


                DataRow dr = dsMaster.Tables[0].Rows[0];
                dr.BeginEdit();

                dr["FileName"] = DBNull.Value;
                dr.EndEdit();


                #endregion data update




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        #endregion
    }
}
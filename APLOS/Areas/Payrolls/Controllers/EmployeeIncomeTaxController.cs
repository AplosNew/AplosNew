using Library.Service.Employees;
using System;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Tax;
using System.Collections.Generic;
using Aplos.Properties;
using System.Web;
using Library.Security.Core;
using Library.Service.Helpers;
using System.IO;
using Aplos.Helpers;
using System.Data;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class EmployeeIncomeTaxController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        EmployeeIncomeTaxService eit = new EmployeeIncomeTaxService();


        public EmployeeIncomeTaxController(ISqlRepository R, IEmployeeProfileService employeeProfileService)
        {
            _sqlRepository = R;
            eit = new EmployeeIncomeTaxService();
        }
       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region Employee Header Saving Functions
        
        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(eit.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
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
        public ActionResult GetTaxType()
        {
            try
            {
                return Json(eit.GetIncomeTaxType(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetTaxPolicy(string Residence, string YearId, string Gender)
        {
            try
            {
                return Json(eit.GetTaxPolicy(Residence,YearId,Gender), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
              
        #endregion

        #region Investment/Deduction Tab Functions

        [HttpPost, Authorize]
        public ActionResult GetInvestDeductList(string PolicyHeaderId,string EmpId)
        {
            try
            {
                return Json(eit.InvestDeductGridData(PolicyHeaderId,EmpId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
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

                    if (Directory.Exists(ResourcesPathReader.TaxOpeningBalancePath()) == false)
                    {
                        try
                        {
                            Directory.CreateDirectory(ResourcesPathReader.TaxOpeningBalancePath());
                        }
                        catch (Exception)
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
                        dr["UpdatedDate"] = DateTime.Now.ToString();
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
        public ActionResult SaveInvestDeduction(Dictionary<string, object> Masterdata, IEnumerable<InvestDeductModelClass> ChildData)
        {
            try
            {
                if (Masterdata["TaxTypeId"] == null)
                {
                    throw new Exception("Please Select Tax Type !!");
                }
                if (Masterdata["TaxYearId"] == null)
                {
                    throw new Exception("Please Select Tax Year !!");
                }

                eit.SaveInvestDeduction(Masterdata, ChildData);
                return Json(new { Error = false, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion

    }
}
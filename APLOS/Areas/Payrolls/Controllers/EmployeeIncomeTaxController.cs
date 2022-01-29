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

        [HttpPost, Authorize]
        public ActionResult DeleteFile(string Id, string TableName)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + "  where Id='" + Id + "'", out dsMaster, false, "1");

                var destinationPath = Path.Combine(ResourcesPathReader.EmployeeIncomeTax(), dsMaster.Tables[0].Rows[0]["FileName"].ToString());
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

        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                //UploadDefault_data = UploadDefault_data.Replace("\"", "");
                //if (string.IsNullOrEmpty(UploadDefault_data))
                //    throw new Exception("Save the order first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.EmployeeIncomeTax(), fileName);

                    var directory = ResourcesPathReader.EmployeeIncomeTax();
                    var path = Path.Combine(directory);

                    if (Directory.Exists(ResourcesPathReader.EmployeeIncomeTax()) == false)
                    {
                        try
                        {
                            Directory.CreateDirectory(ResourcesPathReader.EmployeeIncomeTax());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM EmployeeInvestmentDeduction WHERE Id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

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


        #endregion

        #region Earning Tab Functions

        [HttpPost, Authorize]
        public ActionResult GetEarningGridData(string PolicyId, string EmpId,string From,string To)
        {
            try
            {
                return Json(eit.EarningGridData(PolicyId,EmpId,From,To), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion

    }
}
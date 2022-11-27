using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.Service.Administration.Contract;
using Aplos.Properties;
using System.Data;
using Library.Security.Core;
using System.IO;
using Library.Service.Helpers;

namespace Aplos.Areas.Administration.Controllers
{
    public class GeneralContractController : BaseController
    {
        GeneralContractService gc = new GeneralContractService();
        ContractItemDetailService ci = new ContractItemDetailService();

        SqlRepository _sqlRepository;
        public GeneralContractController()
        {
            _sqlRepository = new SqlRepository();
        }
        public ActionResult Aplos()
        {
            return View();
        }

        #region GetFunction

        [HttpGet, Authorize]
        public ActionResult GetHeaderList()
        {
            return Json(ci.GetHeaderList(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetContractItemDetail(string gcId)
        {
            return Json(ci.GetContractItemDetail(gcId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCheckByList(string gcId)
        {
            return Json(ci.GetCheckByList(gcId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetApproveByList(string gcId)
        {
            return Json(ci.GetApproveByList(gcId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSaveEntityList(string gcId)
        {
            return Json(ci.GetSaveEntityList(gcId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetContractMaster()
        {
            return Json(gc.GetContractMaster(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetContractorList()
        {
            return Json(gc.GetContractorList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetForCheckedByList()
        {
            return Json(gc.GetForCheckedByList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetForApprovedByByList()
        {
            return Json(gc.GetForApprovedByByList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEntity()
        {
            return Json(gc.GetEntity(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetVendorBasedEmployee(string vendorId)
        {
            return Json(gc.GetVendorBasedEmployee(vendorId), JsonRequestBehavior.AllowGet);
        }

        #endregion GetFunction

        #region SAVE
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> data, List<Dictionary<string, object>> contractItemDetail, List<Dictionary<string, object>> checkby, List<Dictionary<string, object>> approveby, List<Dictionary<string, object>> entity)
        {
            try
            {
                return Json(new { Error = false, Data = ci.Save(data, contractItemDetail, checkby, approveby, entity), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveVendorEmployee(List<Dictionary<string, object>> vendoremployee, string headerId)
        {
            try
            {
                return Json(new { Error = false, Data = ci.SaveVendorEmployee(vendoremployee, headerId), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion SAVE

        #region FileUpload
        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<System.Web.HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the order first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetGeneralContractPath(), fileName);

                    var directory = ResourcesPathReader.GetGeneralContractPath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetGeneralContractPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetGeneralContractPath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM [MST].[GeneralContract] WHERE Id='" + UploadDefault_data + "'";
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

       
        #endregion  FileUpload
    }
}
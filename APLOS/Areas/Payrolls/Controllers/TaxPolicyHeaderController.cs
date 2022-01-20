using Aplos.Controllers;
using Aplos.Properties;
using Library.HumanResource.Payroll.Tax;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class TaxPolicyHeaderController : BaseController
    {
        #region Constructor

        TaxPolicyMasterService ds = new TaxPolicyMasterService();
        public TaxPolicyHeaderController()
        {
            ds = new TaxPolicyMasterService();
        }

        #endregion Constructor

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

     
        #region PlantChild Actions

        [HttpPost, Authorize]
        public ActionResult getChildData (string MasterId)
        {
            return Json(ds.getChildData(MasterId), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost, Authorize]
        public ActionResult DeleteChild(string id)
        {
            string jj = ds.DeleteChild(id);
            if (jj == "Success")
            {
                return Json(new { Error = false, Data = id, Message = AplosMessage.Updated });
            }
            else
            {
                return Json(new { Error = true, Data = id, Message = jj });
            }
        }

        [HttpPost]
        public ActionResult saveChild(Dictionary<string, object> Child)
        {
            try
            {
                var id = ds.saveChild(Child);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion

        #region Header Functions

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(ds.GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getMaster()
        {
            return Json(ds.getMaster(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getHeader()
        {
            return Json(ds.getHeader(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequenceHeader()
        {
            return Json(ds.GetSequenceHeader(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult saveHeader(Dictionary<string, object> Header)
        {
            try
            {
                var id = ds.saveHeader(Header);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion

        #region Earning Screen Functions

        [HttpPost, Authorize]
        public ActionResult GetEarningMasterList(string Id)
        {
            return Json(ds.GetEarningMasterList(Id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getSalaryHeadList()
        {
            try
            {
                return Json(ds.getSalaryHeadList(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveEarningMaster(Dictionary<string, object> EarningMasterData)
        {
            try
            {
                var id = ds.SaveEarningMasterChild(EarningMasterData);
                return Json(new { Error = false, Data = id, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion

        #region Formula Rules Functions

        [HttpPost, Authorize]
        public JsonResult DeleteFormula(string ID)
        {
            try
            {
                ds.DeleteFormula(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetGeneralFormula(string TaxEarnChildId)
        {
            try
            {
                return Json(ds.GetGeneralFormula(TaxEarnChildId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveGeneralFormula(TaxExemptionFormula TaxExemptionFormula, IEnumerable<TaxExemptionFormulaDetail> details)
        {
            try
            {
                #region Validations

                if (string.IsNullOrEmpty(TaxExemptionFormula.Description))
                {
                    throw new Exception("Enter Description..");
                }
                if (TaxExemptionFormula.Formula == null)
                {
                    throw new Exception("Select Formula..");
                }
              
                DataSet dsExemptionFormula;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaxExemptionApplicableChild where TaxEarningMasterChildId='" + TaxExemptionFormula.TaxEarningMasterChildId + "' AND  " +
                    "Id<>'" + TaxExemptionFormula.Id + "' AND  Description='" + TaxExemptionFormula.Description + "'", out dsExemptionFormula, false, "1");
               
                if (dsExemptionFormula.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Description already exists!!!");

                #endregion

                #region Service Calling
               
                TaxPolicyMasterService p = new TaxPolicyMasterService();
                p.SaveGeneralFormula(TaxExemptionFormula, details);
             
                #endregion

                return Json(new { Error = false, Data = TaxExemptionFormula, Message = AplosMessage.Updated });
          
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetFormulaList(string FormulaId)
        {
            try
            {
                return Json(ds.GetFormulaList(FormulaId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        #endregion

        #region Investment Deduction Functions
       
        [HttpGet, Authorize]
        public ActionResult getTaxSavingGroup()
        {
            try
            {
                return Json(ds.getTaxSavingGroup(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult getTaxSavingItem()
        {
            try
            {
                return Json(ds.getTaxSavingItem(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetList(string HeaderId)
        {
            try
            {
                return Json(ds.GetList(HeaderId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [Authorize, HttpGet]
        public ActionResult getChildList(string id)
        {
            try
            {
                return Json(ds.getChildList(id), JsonRequestBehavior.AllowGet);
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
                return Json(ds.GetTaxType(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try {
                if (data["TaxTypeId"] == null)
                {
                    throw new Exception("Select Tax Type");
                }
                if (data["TaxSavingGroupId"] == null)
                {
                    throw new Exception("Select Tax Saving Group");
                }
                var datax = ds.Create(data);
                return Json(new { Error = false, Data = datax, Message = AplosMessage.Updated });
            }catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }            
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequenceItemChild()
        {
            try
            {
                return Json(ds.GetSequenceItemChild(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
       
        #endregion

    }
}
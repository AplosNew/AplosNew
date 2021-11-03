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

namespace Aplos.Areas.Payrolls.Controllers
{

    public class TaxPolicyController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public TaxPolicyController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region View

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region Get All
        [HttpGet, Authorize]
        public ActionResult GetMaster()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetMaster(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPlantWisePolicy()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetPlantWisePolicy(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPlantTaxPolicy(string plantID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetPlantTaxPolicy(plantID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxMonth(string Year)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetTaxMonth(Year), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetGeneral(string Master)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetGeneral(Master), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string MasterID)
        {
            return Json(GetSequence(MasterID), JsonRequestBehavior.AllowGet);
        }
        private double GetSequence(string MasterId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM TaxPolicyGeneral where TaxPolicyMstID='" + MasterId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }
        [HttpGet, Authorize]
        public ActionResult GetGeneralFormula(string GeneralID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetGeneralFormula(GeneralID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxYear()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetCompTaxYear(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetValidationForPlant(string TPId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetValidationForPlant(TPId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxGroup()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetTaxType(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetPro(string Master, string YearId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetPro(Master, YearId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetRebate(string Master)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetRebate(Master), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetIncome(string Master)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetIncome(Master), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetFormulaList(string GeneralFormulaId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetFormulaList(GeneralFormulaId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxRebate(string Master)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetTaxRebate(Master), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetTaxRebateMaster(string Master)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetTaxRebateMaster(Master), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetTaxSurchargeMaster(string Master)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetTaxSurchargeMaster(Master), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetTaxSurcharge(string Master)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetTaxSurcharge(Master), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        #endregion

        #region Save
        [HttpPost]
        public JsonResult Create(TaxPolicyMaster master)
        {
            string _id = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (master.CalculationBasis == "" || string.IsNullOrEmpty(master.CalculationBasis))
                {
                    throw new Exception("Select Calculation Basis..");
                }
                if (master.AgeFrom < 18)
                {
                    throw new Exception("Age from cannot be less than 18");
                }
                if (master.AgeTo > 80)
                {
                    throw new Exception("Age Cannot exceed more than 80");
                }
                if (master.AgeFrom > master.AgeTo)
                {
                    throw new Exception("Age To cannot be less than Age From");
                }
                if (string.IsNullOrEmpty(master.AgeFrom.ToString()))
                {
                    throw new Exception("Age From Cannot be empty..");
                }
                if (string.IsNullOrEmpty(master.AgeTo.ToString()))
                {
                    throw new Exception("Age To Cannot be empty..");
                }
                if (master.Male == false && master.Female == false)
                {
                    throw new Exception("At least one Gender should be ticked..");
                }

                master.GroupID = identity.CompanyGroupId;
                master.AddedBy = identity.Name;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.Save(master);
                return Json(new { Error = false, Data = master, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveTaxPolicyPlantWise(List<TaxPolicyPlantWise> BP, string plantID)
        {
            string _id = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();

                for (int i = 0; i < BP.Count; i++)
                {
                    if (BP[i].IsSelectPolicy == false)
                    {
                        if (BP[i].IsDefaultPolicy == true)
                        {
                            throw new Exception("Please Select [" + BP[i].TaxPolicyName + "] to save as Default Policy..");
                        }
                    }
                }

                p.SaveTPPW(BP, plantID);
                return Json(new { Error = false, Data = BP, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveRebate(List<TaxRebateSlabDefine> Rebate, string MasterID, InvestmentCredits InvestmentCredit)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                for (int i = 0; i < Rebate.Count; i++)
                {
                    if (Rebate[i].TaxAbleIncomeUpperForRebate < Rebate[i].TaxAbleIncomeLowerForRebate)
                    {
                        throw new Exception("Maximum cannot be less than Minimum");
                    }
                }

                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.SaveRebate(Rebate, MasterID, InvestmentCredit);
                return Json(new { Error = false, Data = Rebate, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveGeneral(TaxPolicyGeneral GeneralTax)
        {
            string _id = string.Empty;
            try
            {
                if (GeneralTax.SalaryHeadID == null)
                {
                    throw new Exception("Select Taxable Salary Head");
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //GeneralTax.GroupID = identity.CompanyGroupId;
                GeneralTax.AddedBy = identity.Name;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.SaveGeneral(GeneralTax);
                return Json(new { Error = false, Data = GeneralTax.SystemID, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public JsonResult SaveGeneralFormula(TaxGeneralFormula GeneralFormula, IEnumerable<TaxGeneralFormulaDetail> details)
        {
            try
            {
                if (string.IsNullOrEmpty(GeneralFormula.Description))
                {
                    throw new Exception("Enter Description..");
                }
                if (GeneralFormula.Formula == null)
                {
                    throw new Exception("Select Formula..");
                }
                if (GeneralFormula.IsOptionBased)
                {
                    if (string.IsNullOrEmpty(GeneralFormula.OptionBasedValue))
                    {
                        throw new Exception("Select Option Based Value");
                    }
                }
                DataSet dsGeneralFormula;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from TaxPolicyGeneralFormula where TaxPolicyGeneralId='" + GeneralFormula.TaxPolicyGeneralId + "' AND  Id<>'" + GeneralFormula.Id + "' AND  Description='" + GeneralFormula.Description + "'", out dsGeneralFormula, false, "1");
                if (dsGeneralFormula.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Description already exists!!!");
                if (GeneralFormula.IsOptionBaseDefault)
                {
                    con.OpenDataSetThroughAdapter("select * from TaxPolicyGeneralFormula where TaxPolicyGeneralId='" + GeneralFormula.TaxPolicyGeneralId + "' AND  OptionBasedValue='" + GeneralFormula.OptionBasedValue + "' and IsOptionBaseDefault=1 ", out dsGeneralFormula, false, "1");
                    if (dsGeneralFormula.Tables[0].Rows.Count > 0)
                        throw new Exception("Defaul already set for this group");
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.SaveGeneralFormula(GeneralFormula, details);
                return Json(new { Error = false, Data = GeneralFormula, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveProfessionalTax(TaxSlabDefineProfessional ProTax)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ProTax.AddedBy = identity.Name;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.SaveProfessionalTax(ProTax);
                return Json(new { Error = false, Data = ProTax.Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveIncomeSlab(List<Dictionary<string, object>> IncomeSlab, string Master, TaxSlabDefinee Slab)
        {
            IncomeTaxPolicy p = new IncomeTaxPolicy();
            p.SaveIncome(Slab, Master);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string DetailsId = string.Empty;
                string sql = "SELECT * FROM [dbo].[TaxSlabDefine] WHERE TaxPolicyMstID='" + Master + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < IncomeSlab.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[TaxSlabDefine]", out sID);
                    DetailsId = "ITS" + sID;
                    dr["SystemID"] = DetailsId;
                    dr["TaxPolicyMstID"] = Master;
                    dr["Minimum"] = clsStaticInfo.dbl(IncomeSlab[i]["Minimum"]);
                    dr["Maximum"] = clsStaticInfo.dbl(IncomeSlab[i]["Maximum"]);
                    dr["TaxRate"] = clsStaticInfo.dbl(IncomeSlab[i]["TaxRate"]);
                    //dr["SlabType"] = (IncomeSlab[i]["SlabType"]);


                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Error = false, Data = IncomeSlab, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult SaveTaxRebate(List<Dictionary<string, object>> TaxRebateList, string Master, TaxRebate Slab)
        {
            try
            {
                for (int i = 0; i < TaxRebateList.Count; i++)
                {
                    if (TaxRebateList[i]["Maximum"] == null && TaxRebateList[i]["Minimum"] == null)
                    {
                        throw new Exception("Inset Details..");
                    }
                    if (clsStaticInfo.dbl(TaxRebateList[i]["Maximum"]) < clsStaticInfo.dbl(TaxRebateList[i]["Minimum"]))
                    {
                        throw new Exception("Maximum Cannot be smaller than Minimum");
                    }
                }
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.SaveTaxRebate(Slab, Master, TaxRebateList);

                return Json(new { Error = false, Data = TaxRebateList, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult SaveTaxSurcharge(List<Dictionary<string, object>> TaxSurchargeList, string Master, TaxRebate Slab)
        {
            try
            {
                for (int i = 0; i < TaxSurchargeList.Count; i++)
                {
                    if (TaxSurchargeList[i]["Maximum"] == null && TaxSurchargeList[i]["Minimum"] == null)
                    {
                        throw new Exception("Inset Details..");
                    }
                    if (clsStaticInfo.dbl(TaxSurchargeList[i]["Maximum"]) < clsStaticInfo.dbl(TaxSurchargeList[i]["Minimum"]))
                    {
                        throw new Exception("Maximum Cannot be smaller than Minimum");
                    }
                }

                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.SaveTaxSurcharge(Slab, Master, TaxSurchargeList);

                return Json(new { Error = false, Data = TaxSurchargeList, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        #endregion

        #region Delete

        [HttpPost]
        public JsonResult DeleteMaster(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteMaster(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteGeneral(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteGeneral(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult DeleteIncomeSlab(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteIncomeSlab(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult DeleteGeneralFormula(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteGeneralFormula(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult DeleteProfessionalTax(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteProfessionalTax(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteRebate(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteRebate(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteIncome(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteIncome(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult DeleteTaxRebateSlab(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteTaxRebateSlab(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult DeleteTaxRebateDetails(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteTaxRebateDetail(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult DeleteTaxSurchargeDetails(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteTaxSurchargeDetail(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult DeleteTaxSurchargeSlab(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy p = new IncomeTaxPolicy();
                p.DeleteTaxSurchargeSlab(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}
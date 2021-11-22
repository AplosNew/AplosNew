using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class GratuityPolicyController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;

        public GratuityPolicyController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult getGratuitylist(string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select GratuityPolicyMaster.*, p.CompanyId from GratuityPolicyMaster LEFT JOIN ORG.Plant p on p.Id=GratuityPolicyMaster.plantId where plantId= '" + PlantId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult getGratuityDetailslist(string MasterID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from GratuityPolicyDetails where GratuityPolicyMasterId='"+MasterID+@"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(GratuityPolicyMaster GratuityPolicyMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string MasterId = string.Empty;
            MasterId = SaveGratuityPolicyMaster(GratuityPolicyMaster);
            return Json(new { MasterId,Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        public string SaveGratuityPolicyMaster(GratuityPolicyMaster GratuityPolicyMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string Id = string.Empty;
                string sql = "SELECT * FROM GratuityPolicyMaster WHERE ID='" + GratuityPolicyMaster.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GratuityPolicyMaster", out sID);
                    Id = "GPM" + sID;
                    dr["Id"] = Id;
                    //dr["IsFirstMaturityRoudingSixMonth"] = GratuityPolicyMaster.IsFirstMaturityRoudingSixMonth;
                    dr["IsRoudingSixMonth"] = GratuityPolicyMaster.IsRoudingSixMonth;
                    dr["UserName"] = GratuityPolicyMaster.UserName;
                    dr["Active"] = GratuityPolicyMaster.Active;

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["plantId"] = GratuityPolicyMaster.plantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }

                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["ID"].ToString();
                    //dr["IsFirstMaturityRoudingSixMonth"] = GratuityPolicyMaster.IsFirstMaturityRoudingSixMonth;
                    dr["IsRoudingSixMonth"] = GratuityPolicyMaster.IsRoudingSixMonth;
                    dr["UserName"] = GratuityPolicyMaster.UserName;
                    dr["Active"] = GratuityPolicyMaster.Active;

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["plantId"] = GratuityPolicyMaster.plantId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public ActionResult SaveDetails(GratuityPolicyDetails GratuityPolicyDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SaveGratuityPolicyDetails(GratuityPolicyDetails);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        public void SaveGratuityPolicyDetails(GratuityPolicyDetails GratuityPolicyDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsValidation;
            try
            {

                string sql1 = "select * from GratuityPolicyDetails where GratuityPolicyMasterId='" + GratuityPolicyDetails.GratuityPolicyMasterId + @"' and id <> '" + GratuityPolicyDetails.Id + @"' 
                            and(" + GratuityPolicyDetails.MaturityFromYear + @" between MaturityFromYear AND MaturityToYear or " + GratuityPolicyDetails.MaturityToYear + @" between MaturityFromYear AND MaturityToYear)";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsValidation, false, "1");
                if (dsValidation.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Already Have This Year Duration....");
                    throw (ex);
                }

                string sql = "SELECT * FROM GratuityPolicyDetails WHERE ID='" + GratuityPolicyDetails.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "GratuityPolicyDetails", out sID);
                    dr["Id"] = "GPD" + sID;
                    dr["GratuityPolicyMasterId"] = GratuityPolicyDetails.GratuityPolicyMasterId;
                    dr["MaturityFromYear"] = GratuityPolicyDetails.MaturityFromYear;
                    dr["MaturityToYear"] = GratuityPolicyDetails.MaturityToYear;
                    dr["MaturityFormulaDesID"] = GratuityPolicyDetails.MaturityFormulaDesID;
                    dr["MaturityFormulaDescription"] = GratuityPolicyDetails.MaturityFormulaDescription;
                    dr["YearOrDayBasis"] = GratuityPolicyDetails.YearOrDayBasis;
                    if (GratuityPolicyDetails.YearOrDayBasis == "Day")
                    {
                        dr["NoOfDays"] = GratuityPolicyDetails.NoOfDays;
                    }
                    else
                    {
                    dr["NoOfDays"] = DBNull.Value; 
                    }

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["plantId"] = GratuityPolicyDetails.plantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }

                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["GratuityPolicyMasterId"] = GratuityPolicyDetails.GratuityPolicyMasterId;
                    dr["MaturityFromYear"] = GratuityPolicyDetails.MaturityFromYear;
                    dr["MaturityToYear"] = GratuityPolicyDetails.MaturityToYear;
                    dr["MaturityFormulaDesID"] = GratuityPolicyDetails.MaturityFormulaDesID;
                    dr["MaturityFormulaDescription"] = GratuityPolicyDetails.MaturityFormulaDescription;
                    dr["YearOrDayBasis"] = GratuityPolicyDetails.YearOrDayBasis;
                    if (GratuityPolicyDetails.YearOrDayBasis == "Day")
                    {
                        dr["NoOfDays"] = GratuityPolicyDetails.NoOfDays;
                    }
                    else
                    {
                        dr["NoOfDays"] = DBNull.Value;
                    }
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["plantId"] = GratuityPolicyDetails.plantId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet]
        public ActionResult Delete(string SystemID)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            DataSet dsMaster;
            try
            {
                string sql1 = "SELECT * FROM GratuityPolicyDetails WHERE GratuityPolicyMasterId='" + SystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Please Delete Details First....");
                    throw (ex);
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM GratuityPolicyMaster WHERE Id='" + SystemID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteDetails(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM GratuityPolicyDetails WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        public class GratuityPolicyMaster : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
           // public bool IsFirstMaturityRoudingSixMonth { get; set; }
            public bool IsRoudingSixMonth { get; set; }

            public string plantId { get; set; }
            public string CompanyGroupId { get; set; }
            public string UserName { get; set; }
            public bool Active { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string AddedFromIP { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }
        public class GratuityPolicyDetails : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string GratuityPolicyMasterId { get; set; }

            public decimal MaturityFromYear { get; set; }
            public decimal MaturityToYear { get; set; }
            public string MaturityFormulaDesID { get; set; }
            public string MaturityFormulaDescription { get; set; }
            public string plantId { get; set; }
            public string CompanyGroupId { get; set; }
            public string YearOrDayBasis { get; set; }
            public double NoOfDays { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string AddedFromIP { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }
    }
}
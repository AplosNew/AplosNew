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
    public class SalaryRuleController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<SalaryHeadSetting> _salaryHeadSettingRepository;

        public SalaryRuleController(IUnitOfWork U, ISqlRepository R, IRepositoryAsync<SalaryHeadSetting> salaryHeadSettingRepository)
        {
            _unitOfWork = U;
            _sqlRepository = R;
            _salaryHeadSettingRepository = salaryHeadSettingRepository;
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
        public ActionResult GetSalaryHeadCbo(string currencyRuleSystemID)
        {
            try
            {
                var sql = @"SELECT DISTINCT A.SalaryHeadID Value, A.SalaryHead Text,A.HasGL FROM (
SELECT DISTINCT SH.SalaryHeadID , SH.SalaryHead,HasGL=CASE WHEN G.Id IS NOT NULL THEN 1 WHEN SH.TransactionType='NotApplicable' THEN 1 ELSE 0 END FROM SalaryHead SH
                            INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
							LEFT JOIN(Select * from MST.SalaryHeadGL Where (DrDirectGLId IS NOT NULL OR CrDirectGLId IS NOT NULL OR DrInDirectGLId IS NOT NULL OR CrInDirectGLId IS NOT NULL)) G ON G.SalaryHeadID=SH.SalaryHeadID   
                            Where CRC.MstSystemID = '" + currencyRuleSystemID + @"')A";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult getSalaryRuleList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT SR.*, CRM.CurrencyRuleName, TG.TaxGroupName IncomeTaxGroup
                           FROM  SalaryRuleMaster SR
                           LEFT JOIN CurrencyRuleMaster CRM ON SR.CurrencyRuleSystemID = CRM.SystemID AND CRM.GroupID = '" + identity.CompanyGroupId + @"' AND CRM.PlantID = '" + identity.PlantId + @"' 
                           LEFT JOIN TaxGroup TG ON TG.SystemID=SR.TaxGroupID
                           WHERE SR.GroupID = '" + identity.CompanyGroupId + @"' AND SR.PlantID = '" + identity.PlantId + "' AND SR.IsActive=1 ORDER BY SR.SalaryRuleName";
            return Json(_sqlRepository.GetModelCollection<SalaryRuleMaster>(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult LoadSalaryRuleGeneral(string strSalaryRuleID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT SG.SalaryRuleGeneralSystemID, SG.SalaryHeadID, SH.SalaryHead, SG.IsOpen, SG.IsFixed, SG.FixedValue,SG.IsNA,
                                 SG.IsFormula, SG.FormulaDes, SG.FormulaDesID,
                                 SG.IsFixedMonthDay, SG.FixedMonthDayValue, SG.IsMonthDay, SG.IsMonthWorkDay, 
                                 SG.IsWorkDaysInAMonthIncHold, ISNULL(SG.IsFixedDisbus,0) IsFixedDisbus, ISNULL(SG.SequenceNo, 0) SequenceNo, SG.IsDisbusted,
                                 ISNULL(SG.BaseOnNetPay, 0) BaseOnNetPay, ISNULL(SG.RefAbsentism, 0) RefAbsentism, ISNULL(SG.IsGNRBaseOthSlrHD, 0) IsGNRBaseOthSlrHD,
                                 --ISNULL(SH.IsCTCComponent, 0) IsCTCComponent, ISNULL(SH.IsGrossComponent, 0) IsGrossComponent,
                                 SG.GNRBaseOthSlrHDFormula, SG.GNRApplicableMonthNo,
                                 ISNULL(SG.IsRetain, 0) IsRetain, ISNULL(SG.IsMinWages, 0) IsMinWages, ISNULL(SG.IsGNRWhichEverLess, 0) IsGNRWhichEverLess     
                                 ,HasMaxLimit,FixedMaxLimit,MaxLimitValue,PercentageMaxLimit,PercentageMaxLimitSalaryHeadId
								 ,HasMinLimit,FixedMinLimit,MinLimitValue,PercentageMinLimit,PercentageMinLimitSalaryHeadId,IsPolicyDerived
                                    , ISNULL(SG.IsPayOnWeekoffForFixedMonthDay, 0) IsPayOnWeekoffForFixedMonthDay, ISNULL(SG.IsPayOnHolidayForFixedMonthDay, 0) IsPayOnHolidayForFixedMonthDay--,SG.IsSlabBased
                               FROM SalaryRuleGeneral SG
                               LEFT JOIN SalaryHead SH ON SG.SalaryHeadID = SH.SalaryHeadID  
                               WHERE SG.SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"'
                               ORDER BY SG.SequenceNo";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryRuleESIC(string strSalaryRuleID, string currencyRuleSystemID)
        {
            string sql = @"SELECT Active = CAST (CASE WHEN SREE.SalaryRuleESICSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                         ,SH.SalaryHeadID, SH.SalaryHead,SREE.SalaryRuleESICSystemID FROM SalaryHead SH
                         INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                         OUTER APPLY (SELECT * FROM [dbo].[SalaryRuleESIC] SRE
                         WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                         WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND HeadCategory LIKE '%ESIC%' ORDER BY SH.SalaryHead";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryRuleRetentionBonus(string strSalaryRuleID, string currencyRuleSystemID)
        {
            string sql = @"SELECT distinct SH.SalaryHeadID
                         , Active = CAST (CASE WHEN SREE.SalaryRuleRetentionPmtSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                         , SH.SalaryHead,SREE.SalaryRuleRetentionPmtSystemID FROM SalaryHead SH
                         INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                         OUTER APPLY (SELECT * FROM [dbo].[SalaryRuleRetentionPmtMaster] SRE
                         WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                         WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND SalaryHead like 'Bonus%' or SalaryHead like 'Ex-Gratia%' or SalaryHead like 'Statutory Bonus%'
                         ORDER BY SH.SalaryHead";


            var SalaryRuleRetentionBonus = _sqlRepository.GetDataCollection(sql);


            string sql1 = @"SELECT * FROM [dbo].[SalaryRuleGovtGrd] WHERE SalaryRuleMasterSystemID= '" + strSalaryRuleID + @"'";


            var SalaryRuleGovtGrd = _sqlRepository.GetDataCollection(sql1);


            return Json(new { SalaryRuleRetentionBonus, SalaryRuleGovtGrd }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryRuleAttdnBonus(string strSalaryRuleID, string currencyRuleSystemID)
        {
            string sql = @"SELECT distinct SH.SalaryHeadID
                        , Active = CAST (CASE WHEN SREE.SalaryRuleAttdnBonusPmtSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                        , SH.SalaryHead,SREE.SalaryRuleAttdnBonusPmtSystemID FROM SalaryHead SH
                        INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                        OUTER APPLY (SELECT * FROM [dbo].[SalaryRuleAttdnBonusPmtMaster] SRE
                        WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                        WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND HeadCategory='Attendance Bonus'
                        ORDER BY SH.SalaryHead
                        ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryRuleAbsenteeism(string strSalaryRuleID, string currencyRuleSystemID)
        {
            string sql = @"SELECT  SH.SalaryHeadID, SH.SalaryHead FROM SalaryHead SH
                         INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                         WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND HeadCategory ='Absenteeism' ORDER BY SH.SalaryHead";
            return Json(_sqlRepository.GetCombo(sql, "SalaryHeadID", "SalaryHead"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedSalaryRuleAbsenteeism(string strSalaryRuleID)
        {
            string sql = @"SELECT SA.*,SH.SalaryHead FROM [dbo].[SalaryRuleAbsenteeism] SA
                         LEFT JOIN dbo.SalaryHead SH ON SH.SalaryHeadID=SA.SalaryHeadID
                         WHERE SA.SalaryRuleMasterSystemID='" + strSalaryRuleID + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryRuleOT(string strSalaryRuleID, string currencyRuleSystemID)
        {
            string sql = @"SELECT Active = CAST (CASE WHEN SREE.SalaryRuleOTSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                         ,SH.SalaryHeadID, SH.SalaryHead,SREE.SalaryRuleOTSystemID FROM SalaryHead SH
                         INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                         OUTER APPLY (SELECT * FROM [dbo].[SalaryRuleOT] SRE
                         WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                         WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND HeadCategory= 'OverTime' ORDER BY SH.SalaryHead";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetSalaryRulePF(string strSalaryRuleID, string currencyRuleSystemID)
        {
            string sql = @"SELECT distinct SH.SalaryHeadID
                        , Active = CAST (CASE WHEN SREE.SalaryRulePFSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                        , SH.SalaryHead,SREE.SalaryRulePFSystemID FROM SalaryHead SH
                        INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                        OUTER APPLY (SELECT * FROM [dbo].[SalaryRulePF] SRE
                        WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                        WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND HeadCategory like 'PF Employee Contribution%' or HeadCategory like 'PF Employer Contribution%' or HeadCategory like 'PF Voluntary%' or HeadCategory like 'Pension%'
                        ORDER BY SH.SalaryHead";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult LoadSalaryHeadSetting(string strSalaryRuleID, string currencyRuleSystemID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            //==================================

            string GetSalaryRuleESICsql = @"SELECT Active = CAST (CASE WHEN SREE.SalaryRuleESICSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                         ,SH.SalaryHeadID, SH.SalaryHead,SREE.SalaryRuleESICSystemID FROM SalaryHead SH
                         INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                         OUTER APPLY (SELECT * FROM [dbo].[SalaryRuleESIC] SRE
                         WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                         WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND HeadCategory LIKE '%ESIC%' ORDER BY SH.SalaryHead";
            var SalaryRuleESIC = _sqlRepository.GetDataCollection(GetSalaryRuleESICsql);



            string GetSalaryRuleRetentionBonussql = @"SELECT distinct SH.SalaryHeadID
                         , Active = CAST (CASE WHEN SREE.SalaryRuleRetentionPmtSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                         , SH.SalaryHead,SREE.SalaryRuleRetentionPmtSystemID FROM SalaryHead SH
                         INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                         OUTER APPLY (SELECT * FROM [dbo].[SalaryRuleRetentionPmtMaster] SRE
                         WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                         WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND SalaryHead like 'Bonus%' or SalaryHead like 'Ex-Gratia%' or SalaryHead like 'Statutory Bonus%'
                         ORDER BY SH.SalaryHead";
            var SalaryRuleRetentionBonus = _sqlRepository.GetDataCollection(GetSalaryRuleRetentionBonussql);



            string GetSalaryRuleAttdnBonussql = @"SELECT distinct SH.SalaryHeadID
                        , Active = CAST (CASE WHEN SREE.SalaryRuleAttdnBonusPmtSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                        , SH.SalaryHead,SREE.SalaryRuleAttdnBonusPmtSystemID FROM SalaryHead SH
                        INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                        OUTER APPLY (SELECT * FROM [dbo].[SalaryRuleAttdnBonusPmtMaster] SRE
                        WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                        WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND HeadCategory='Attendance Bonus'
                        ORDER BY SH.SalaryHead
                        ";
            var SalaryRuleAttdnBonus = _sqlRepository.GetDataCollection(GetSalaryRuleAttdnBonussql);




            string GetSalaryRuleOTsql = @"SELECT Active = CAST (CASE WHEN SREE.SalaryRuleOTSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                         ,SH.SalaryHeadID, SH.SalaryHead,SREE.SalaryRuleOTSystemID FROM SalaryHead SH
                         INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                         OUTER APPLY (SELECT * FROM [dbo].[SalaryRuleOT] SRE
                         WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                         WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND HeadCategory= 'OverTime' ORDER BY SH.SalaryHead";
            var SalaryRuleOT = _sqlRepository.GetDataCollection(GetSalaryRuleOTsql);




            string GetSalaryRulePFsql = @"SELECT distinct SH.SalaryHeadID
                        , Active = CAST (CASE WHEN SREE.SalaryRulePFSystemID IS NULL THEN 0 ELSE 1 END AS bit)
                        , SH.SalaryHead,SREE.SalaryRulePFSystemID FROM SalaryHead SH
                        INNER JOIN CurrencyRuleChild CRC ON SH.SalaryHeadID = CRC.SalaryHeadID
                        OUTER APPLY (SELECT * FROM [dbo].[SalaryRulePF] SRE
                        WHERE SRE.SalaryHeadID = SH.SalaryHeadID AND SalaryRuleMasterSystemID = '" + strSalaryRuleID + @"') SREE
                        WHERE CRC.MstSystemID = '" + currencyRuleSystemID + @"' AND HeadCategory like 'PF Employee Contribution%' or HeadCategory like 'PF Employer Contribution%' or HeadCategory like 'PF Voluntary%' or HeadCategory like 'Pension%'
                        ORDER BY SH.SalaryHead";
            var SalaryRulePF = _sqlRepository.GetDataCollection(GetSalaryRulePFsql);






            string GetSavedSalaryRuleAbsenteeismsql = @"SELECT SA.*,SH.SalaryHead FROM [dbo].[SalaryRuleAbsenteeism] SA
                         LEFT JOIN dbo.SalaryHead SH ON SH.SalaryHeadID=SA.SalaryHeadID
                         WHERE SA.SalaryRuleMasterSystemID='" + strSalaryRuleID + "'";
            var SalaryRuleAbsenteeism = _sqlRepository.GetDataCollection(GetSavedSalaryRuleAbsenteeismsql);
            //===================================

            string sql = @"SELECT* FROM [dbo].[SalaryHeadSetting] Where SalaryRuleId='" + strSalaryRuleID + "'";
            return Json(new
            {
                data = _sqlRepository.GetDataCollection(sql)
                ,
                SalaryRuleESIC,
                SalaryRuleRetentionBonus,
                SalaryRuleAttdnBonus,
                SalaryRuleOT,
                SalaryRulePF,
                SalaryRuleAbsenteeism
            },
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult SearchTaxGrpInfo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"SELECT * FROM TaxGroup WHERE GroupID = '" + identity.CompanyGroupId + @"'";
            return Json(_sqlRepository.GetModelCollection<TaxGroup>(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCurrencyRuleCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"SELECT SystemID,CurrencyRuleName FROM CurrencyRuleMaster  WHERE GroupID = '" + identity.CompanyGroupId + @"' AND PlantID = '" + identity.PlantId + @"' ";
            return Json(_sqlRepository.GetCombo(sql, "SystemID", "CurrencyRuleName"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SalaryRuleMaster entity)
        {
            try
            {
                string SystemId;
                SaveSalaryRuleMasterData(entity, out SystemId);
                // SaveSalaryRuleGeneralData(salaryRuleGenerals, SystemId);
                return Json(new { SystemId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult CreateSalaryRuleGeneral(SalaryRuleGeneral salaryRuleGenerals)
        {
            try
            {
                SaveSalaryRuleGeneralData(salaryRuleGenerals);
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
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SALARY_RULE", out idFromDB);
            systemID = "SR-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }
        private string GetGeneralPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SALARY_RULE_GENERAL", out idFromDB);
            systemID = "SR-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private string GetESICPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SALARY_RULE", out idFromDB);
            systemID = "ES-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private string GetOTPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SALARY_RULE", out idFromDB);
            systemID = "OT-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private string GetPFPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SALARY_RULE", out idFromDB);
            systemID = "PF-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private string GetBonusPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SALARY_RULE", out idFromDB);
            systemID = "B-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }
        private string GetSalaryRuleGovtGrdPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalaryRuleGovtGrd", out idFromDB);
            systemID = "B-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }
        private string GetAbsenteeismPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SALARY_RULE", out idFromDB);
            systemID = "AB-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private string GetRetentionBonusPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SALARY_RULE", out idFromDB);
            systemID = "RB-" + idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        [HttpGet, Authorize]
        public ActionResult GetAutoSequence(string SalaryRuleMasterSystemID)
        {

            string sql = @"SELECT (ISNULL((MAX(ISNULL(SequenceNo,0))),0)+1) SequenceNo FROM [dbo].[SalaryRuleGeneral] Where SalaryRuleMasterSystemID='" + SalaryRuleMasterSystemID + "'";
            return Json(_sqlRepository.GetModelCollection<SalaryRuleGeneral>(sql), JsonRequestBehavior.AllowGet);
        }

        public void GetAutoSequence(string SalaryRuleMasterSystemID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT (ISNULL(MAX(SequenceNo),0)+1 )SequenceNo FROM [dbo].[SalaryRuleGeneral] Where SalaryRuleMasterSystemID='" + SalaryRuleMasterSystemID + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetSalaryHeadID(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT SalaryHeadID FROM SalaryHead WHERE HeadCategory= 'GROSS'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetSalaryInfoDefine(string salaryRuleMasterSystemID, out DataSet dsRef)
        {
            string strSql;
            ConnectionManager.DAL.ConManager Obj;
            try
            {
                strSql = @"SELECT SystemID,Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate ,SalaryRuleMasterSystemID FROM SalaryInfoDefineMaster Where SalaryRuleMasterSystemID='" + salaryRuleMasterSystemID + @"'
                           union
                           SELECT  SystemID,Replace(CONVERT(VARCHAR(11), EffectiveDate, 106), ' ', '-') EffectiveDate ,SalaryRuleMasterSystemID FROM SalaryInfoBackMaster Where SalaryRuleMasterSystemID='" + salaryRuleMasterSystemID + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        private void SaveSalaryRuleMasterData(SalaryRuleMaster data, out string SystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SystemId = null;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsSalRul;
            try
            {
                string sqlv = "SELECT * FROM [dbo].[SalaryRuleMaster] WHERE PlantID='" + identity.PlantId + "' AND SalaryRuleName='" + data.SalaryRuleName.ToString() + "' and SystemID <>'" + data.SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlv, out dsSalRul, false, "1");
                if (dsSalRul.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Salary Rule already  exists [" + dsSalRul.Tables[0].Rows[0]["SalaryRuleName"] + "]!!!");
                    throw (ex);
                }


                string sql = "SELECT * FROM [dbo].[SalaryRuleMaster] WHERE SystemID='" + data.SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["SystemID"] = GetPK();
                    dr["IsUsed"] = 0;

                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = identity.PlantId;

                    dr["SalaryRuleName"] = data.SalaryRuleName;
                    dr["TaxGroupID"] = data.TaxGroupID;
                    dr["CurrencyRuleSystemID"] = data.CurrencyRuleSystemID;
                    dr["SalaryRuleDescription"] = data.SalaryRuleDescription;
                    dr["IsValidGovGrd"] = data.IsValidGovGrd;
                    dr["IsActive"] = data.IsActive;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["DateAdded"] = DateTime.Now;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["SalaryRuleName"] = data.SalaryRuleName;
                    dr["TaxGroupID"] = data.TaxGroupID;
                    dr["CurrencyRuleSystemID"] = data.CurrencyRuleSystemID;
                    dr["SalaryRuleDescription"] = data.SalaryRuleDescription;
                    dr["IsValidGovGrd"] = data.IsValidGovGrd;
                    dr["IsActive"] = data.IsActive;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                SystemId = dsMaster.Tables[0].Rows[0]["SystemID"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        private void SaveSalaryRuleGeneralData(SalaryRuleGeneral data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {

                    if (data.IsFormula)
                    {
                        if (string.IsNullOrEmpty(data.FormulaDesID))
                        {
                            Exception ex = new Exception("Enter Formula !!!");
                            throw (ex);
                        }

                    }


                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    DataSet dsSeq;
                    // GetAutoSequence(masterId, out dsSeq);
                    //int seq = Convert.ToInt32(dsSeq.Tables[0].Rows[0]["SequenceNo"].ToString()); ;

                    string sql = "SELECT * FROM [dbo].[SalaryRuleGeneral] WHERE SalaryRuleGeneralSystemID='" + data.SalaryRuleGeneralSystemID + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        //seq++;

                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["SalaryRuleGeneralSystemID"] = GetGeneralPK();
                        //dr["SalaryRuleGeneralSystemID"] = data.SalaryRuleMasterSystemID + data.SalaryHeadID;
                        dr["SalaryRuleMasterSystemID"] = data.SalaryRuleMasterSystemID;
                        dr["SalaryHeadID"] = data.SalaryHeadID;

                        dr["IsOpen"] = data.IsOpen;
                        dr["IsFixed"] = data.IsFixed;
                        dr["FixedValue"] = data.FixedValue;
                        dr["IsFormula"] = data.IsFormula;
                        dr["IsNA"] = data.IsNA;
                        if (data.IsFormula)
                        {
                            dr["FormulaDes"] = data.FormulaDes;
                            dr["FormulaDesID"] = data.FormulaDesID;
                        }
                        else
                        {
                            dr["FormulaDes"] = null;
                            dr["FormulaDesID"] = null;
                        }


                        dr["IsFixedMonthDay"] = data.IsFixedMonthDay;
                        dr["FixedMonthDayValue"] = data.FixedMonthDayValue;

                        dr["IsMonthDay"] = data.IsMonthDay;
                        dr["IsMonthWorkDay"] = data.IsMonthWorkDay;
                        dr["IsWorkDaysInAMonthIncHold"] = data.IsWorkDaysInAMonthIncHold;
                        dr["IsFixedDisbus"] = data.IsFixedDisbus;

                        dr["SequenceNo"] = data.SequenceNo;

                        dr["BaseOnNetPay"] = data.BaseOnNetPay;
                        dr["RefAbsentism"] = data.RefAbsentism;

                        dr["IsGNRBaseOthSlrHD"] = data.IsGNRBaseOthSlrHD;
                        dr["GNRBaseOthSlrHDFormula"] = data.GNRBaseOthSlrHDFormula;
                        dr["GNRApplicableMonthNo"] = data.GNRApplicableMonthNo;

                        dr["IsRetain"] = data.IsRetain;
                        dr["IsMinWages"] = data.IsMinWages;
                        dr["IsGNRWhichEverLess"] = data.IsGNRWhichEverLess;

                        dr["HasMaxLimit"] = data.HasMaxLimit;
                        dr["HasMinLimit"] = data.HasMinLimit;

                        dr["FixedMaxLimit"] = data.FixedMaxLimit;
                        dr["MaxLimitValue"] = data.MaxLimitValue;

                        dr["PercentageMaxLimit"] = data.PercentageMaxLimit;

                        dr["FixedMinLimit"] = data.FixedMinLimit;
                        dr["MinLimitValue"] = data.MinLimitValue;

                        dr["PercentageMinLimit"] = data.PercentageMinLimit;

                        dr["IsPayOnWeekoffForFixedMonthDay"] = data.IsPayOnWeekoffForFixedMonthDay;
                        dr["IsPayOnHolidayForFixedMonthDay"] = data.IsPayOnHolidayForFixedMonthDay;


                        dr["PercentageMaxLimitSalaryHeadId"] = data.PercentageMaxLimitSalaryHeadId;
                        dr["PercentageMinLimitSalaryHeadId"] = data.PercentageMinLimitSalaryHeadId;

                        dr["IsPolicyDerived"] = data.IsPolicyDerived;
                        dr["IsGNRNetPayEffect"] = data.IsGNRNetPayEffect;
                        dr["IsGNRTagAndUnTag"] = data.IsGNRTagAndUnTag;
                        dr["IsDisbusted"] = data.IsDisbusted;
                        dr["IsBankPayment"] = data.IsBankPayment;
                        dr["IsCashPayment"] = data.IsCashPayment;
                        dr["IsCTCComponent"] = data.IsCTCComponent;
                        dr["IsGrossComponent"] = data.IsGrossComponent;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["DateAdded"] = DateTime.Now;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["SalaryRuleMasterSystemID"] = data.SalaryRuleMasterSystemID;
                        dr["SalaryHeadID"] = data.SalaryHeadID;

                        dr["IsOpen"] = data.IsOpen;
                        dr["IsFixed"] = data.IsFixed;
                        dr["FixedValue"] = data.FixedValue;
                        dr["IsFormula"] = data.IsFormula;
                        dr["IsNA"] = data.IsNA;

                        //dr["FormulaDes"] = data.FormulaDes;
                        //dr["FormulaDesID"] = data.FormulaDesID;
                        if (data.IsFormula)
                        {
                            dr["FormulaDes"] = data.FormulaDes;
                            dr["FormulaDesID"] = data.FormulaDesID;
                        }
                        else
                        {
                            dr["FormulaDes"] = null;
                            dr["FormulaDesID"] = null;
                        }

                        dr["IsFixedMonthDay"] = data.IsFixedMonthDay;
                        dr["FixedMonthDayValue"] = data.FixedMonthDayValue;

                        dr["IsPayOnWeekoffForFixedMonthDay"] = data.IsPayOnWeekoffForFixedMonthDay;
                        dr["IsPayOnHolidayForFixedMonthDay"] = data.IsPayOnHolidayForFixedMonthDay;

                        dr["IsMonthDay"] = data.IsMonthDay;
                        dr["IsMonthWorkDay"] = data.IsMonthWorkDay;
                        dr["IsWorkDaysInAMonthIncHold"] = data.IsWorkDaysInAMonthIncHold;
                        dr["IsFixedDisbus"] = data.IsFixedDisbus;

                        //dr["SequenceNo"] = item.SequenceNo != 0 ? item.SequenceNo : Convert.ToInt32(dsSeq.Tables[0].Rows[0]["SequenceNo"].ToString());
                        dr["SequenceNo"] = data.SequenceNo;

                        dr["BaseOnNetPay"] = data.BaseOnNetPay;
                        dr["RefAbsentism"] = data.RefAbsentism;

                        dr["IsGNRBaseOthSlrHD"] = data.IsGNRBaseOthSlrHD;
                        dr["GNRBaseOthSlrHDFormula"] = data.GNRBaseOthSlrHDFormula;
                        dr["GNRApplicableMonthNo"] = data.GNRApplicableMonthNo;

                        dr["IsRetain"] = data.IsRetain;
                        dr["IsMinWages"] = data.IsMinWages;
                        dr["IsGNRWhichEverLess"] = data.IsGNRWhichEverLess;

                        dr["HasMaxLimit"] = data.HasMaxLimit;
                        dr["HasMinLimit"] = data.HasMinLimit;

                        dr["FixedMaxLimit"] = data.FixedMaxLimit;
                        dr["MaxLimitValue"] = data.MaxLimitValue;

                        dr["PercentageMaxLimit"] = data.PercentageMaxLimit;

                        dr["FixedMinLimit"] = data.FixedMinLimit;
                        dr["MinLimitValue"] = data.MinLimitValue;

                        dr["PercentageMinLimit"] = data.PercentageMinLimit;

                        dr["PercentageMaxLimitSalaryHeadId"] = data.PercentageMaxLimitSalaryHeadId;
                        dr["PercentageMinLimitSalaryHeadId"] = data.PercentageMinLimitSalaryHeadId;

                        dr["IsPolicyDerived"] = data.IsPolicyDerived;
                        dr["IsGNRNetPayEffect"] = data.IsGNRNetPayEffect;
                        dr["IsGNRTagAndUnTag"] = data.IsGNRTagAndUnTag;
                        dr["IsDisbusted"] = data.IsDisbusted;
                        dr["IsBankPayment"] = data.IsBankPayment;
                        dr["IsCashPayment"] = data.IsCashPayment;
                        dr["IsCTCComponent"] = data.IsCTCComponent;
                        dr["IsGrossComponent"] = data.IsGrossComponent;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["DateUpdated"] = DateTime.Now.ToString();

                        dr.EndEdit();
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);


                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveSalaryRuleGeneralData(IEnumerable<SalaryRuleGeneral> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    DataSet dsSeq;
                    //GetAutoSequence(masterId, out dsSeq);
                    //int seq = Convert.ToInt32(dsSeq.Tables[0].Rows[0]["SequenceNo"].ToString()); ;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[SalaryRuleGeneral] WHERE SalaryRuleGeneralSystemID='" + item.SalaryRuleGeneralSystemID + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            //seq++;

                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["SalaryRuleGeneralSystemID"] = masterId + item.SalaryHeadID;
                            dr["SalaryRuleMasterSystemID"] = masterId;
                            dr["SalaryHeadID"] = item.SalaryHeadID;

                            dr["IsOpen"] = item.IsOpen;
                            dr["IsFixed"] = item.IsFixed;
                            dr["FixedValue"] = item.FixedValue;
                            dr["IsFormula"] = item.IsFormula;
                            dr["IsNA"] = item.IsNA;

                            dr["FormulaDes"] = item.FormulaDes;
                            dr["FormulaDesID"] = item.FormulaDesID;

                            dr["IsFixedMonthDay"] = item.IsFixedMonthDay;
                            dr["FixedMonthDayValue"] = item.FixedMonthDayValue;

                            dr["IsPayOnWeekoffForFixedMonthDay"] = item.IsPayOnWeekoffForFixedMonthDay;
                            dr["IsPayOnHolidayForFixedMonthDay"] = item.IsPayOnHolidayForFixedMonthDay;


                            dr["IsMonthDay"] = item.IsMonthDay;
                            dr["IsMonthWorkDay"] = item.IsMonthWorkDay;
                            dr["IsWorkDaysInAMonthIncHold"] = item.IsWorkDaysInAMonthIncHold;
                            dr["IsFixedDisbus"] = item.IsFixedDisbus;

                            // dr["SequenceNo"] = seq;

                            dr["BaseOnNetPay"] = item.BaseOnNetPay;
                            dr["RefAbsentism"] = item.RefAbsentism;

                            dr["IsGNRBaseOthSlrHD"] = item.IsGNRBaseOthSlrHD;
                            dr["GNRBaseOthSlrHDFormula"] = item.GNRBaseOthSlrHDFormula;
                            dr["GNRApplicableMonthNo"] = item.GNRApplicableMonthNo;

                            dr["IsRetain"] = item.IsRetain;
                            dr["IsMinWages"] = item.IsMinWages;
                            dr["IsGNRWhichEverLess"] = item.IsGNRWhichEverLess;

                            dr["HasMaxLimit"] = item.HasMaxLimit;
                            dr["HasMinLimit"] = item.HasMinLimit;

                            dr["FixedMaxLimit"] = item.FixedMaxLimit;
                            dr["MaxLimitValue"] = item.MaxLimitValue;

                            dr["PercentageMaxLimit"] = item.PercentageMaxLimit;

                            dr["FixedMinLimit"] = item.FixedMinLimit;
                            dr["MinLimitValue"] = item.MinLimitValue;

                            dr["PercentageMinLimit"] = item.PercentageMinLimit;

                            dr["PercentageMaxLimitSalaryHeadId"] = item.PercentageMaxLimitSalaryHeadId;
                            dr["PercentageMinLimitSalaryHeadId"] = item.PercentageMinLimitSalaryHeadId;

                            dr["IsPolicyDerived"] = item.IsPolicyDerived;
                            //dr["IsGNRNetPayEffect"] = data.IsGNRNetPayEffect;
                            //dr["IsGNRTagAndUnTag"] = data.IsGNRTagAndUnTag;
                            //dr["IsDisbusted"] = data.IsDisbusted;
                            //dr["IsBankPayment"] = data.IsBankPayment;
                            //dr["IsCashPayment"] = data.IsCashPayment;
                            //dr["IsCTCComponent"] = data.IsCTCComponent;
                            //dr["IsGrossComponent"] = data.IsGrossComponent;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["DateAdded"] = DateTime.Now;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["SalaryRuleMasterSystemID"] = masterId;
                            dr["SalaryHeadID"] = item.SalaryHeadID;

                            dr["IsOpen"] = item.IsOpen;
                            dr["IsFixed"] = item.IsFixed;
                            dr["FixedValue"] = item.FixedValue;
                            dr["IsFormula"] = item.IsFormula;
                            dr["IsNA"] = item.IsNA;

                            dr["FormulaDes"] = item.FormulaDes;
                            dr["FormulaDesID"] = item.FormulaDesID;

                            dr["IsFixedMonthDay"] = item.IsFixedMonthDay;
                            dr["FixedMonthDayValue"] = item.FixedMonthDayValue;


                            dr["IsPayOnWeekoffForFixedMonthDay"] = item.IsPayOnWeekoffForFixedMonthDay;
                            dr["IsPayOnHolidayForFixedMonthDay"] = item.IsPayOnHolidayForFixedMonthDay;

                            dr["IsMonthDay"] = item.IsMonthDay;
                            dr["IsMonthWorkDay"] = item.IsMonthWorkDay;
                            dr["IsWorkDaysInAMonthIncHold"] = item.IsWorkDaysInAMonthIncHold;
                            dr["IsFixedDisbus"] = item.IsFixedDisbus;

                            //dr["SequenceNo"] = item.SequenceNo != 0 ? item.SequenceNo : Convert.ToInt32(dsSeq.Tables[0].Rows[0]["SequenceNo"].ToString());

                            dr["BaseOnNetPay"] = item.BaseOnNetPay;
                            dr["RefAbsentism"] = item.RefAbsentism;

                            dr["IsGNRBaseOthSlrHD"] = item.IsGNRBaseOthSlrHD;
                            dr["GNRBaseOthSlrHDFormula"] = item.GNRBaseOthSlrHDFormula;
                            dr["GNRApplicableMonthNo"] = item.GNRApplicableMonthNo;

                            dr["IsRetain"] = item.IsRetain;
                            dr["IsMinWages"] = item.IsMinWages;
                            dr["IsGNRWhichEverLess"] = item.IsGNRWhichEverLess;

                            dr["HasMaxLimit"] = item.HasMaxLimit;
                            dr["HasMinLimit"] = item.HasMinLimit;

                            dr["FixedMaxLimit"] = item.FixedMaxLimit;
                            dr["MaxLimitValue"] = item.MaxLimitValue;

                            dr["PercentageMaxLimit"] = item.PercentageMaxLimit;

                            dr["FixedMinLimit"] = item.FixedMinLimit;
                            dr["MinLimitValue"] = item.MinLimitValue;

                            dr["PercentageMinLimit"] = item.PercentageMinLimit;

                            dr["PercentageMaxLimitSalaryHeadId"] = item.PercentageMaxLimitSalaryHeadId;
                            dr["PercentageMinLimitSalaryHeadId"] = item.PercentageMinLimitSalaryHeadId;

                            dr["IsPolicyDerived"] = item.IsPolicyDerived;
                            //dr["IsGNRNetPayEffect"] = data.IsGNRNetPayEffect;
                            //dr["IsGNRTagAndUnTag"] = data.IsGNRTagAndUnTag;
                            //dr["IsDisbusted"] = data.IsDisbusted;
                            //dr["IsBankPayment"] = data.IsBankPayment;
                            //dr["IsCashPayment"] = data.IsCashPayment;
                            //dr["IsCTCComponent"] = data.IsCTCComponent;
                            //dr["IsGrossComponent"] = data.IsGrossComponent;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr["DateUpdated"] = DateTime.Now.ToString();

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateSalaryHeadSetting(IEnumerable<SalaryHeadSetting> entities)
        {

            try
            {
                SaveSalaryHeadSettingData(entities);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult CreateESICSalaryHead(IEnumerable<SalaryRuleESIC> entities, string SalaryRuleMasterSystemID)
        {
            try
            {
                SaveESICSalaryHeadData(entities, SalaryRuleMasterSystemID);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateAttnBonusSalaryHead(IEnumerable<SalaryRuleAttdnBonusPmtMaster> entities)
        {
            try
            {
                SaveAttnBonusSalaryHeadData(entities);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateRetentionBonusSalaryHead(IEnumerable<SalaryRuleRetentionPmtMaster> entities, string SalaryHeadId, List<MinimumWagesSalaryHeadModel> MinimumWagesSalaryHeadLists)
        {
            try
            {
                SaveRetentionSalaryHeadData(entities, SalaryHeadId, MinimumWagesSalaryHeadLists);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateAbsenteeismSalaryHead(SalaryRuleAbsenteeism entity, string SalaryRuleMasterSystemID)
        {
            try
            {
                SaveAbsenteeismSalaryHeadData(entity);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost, Authorize]
        public JsonResult CreateOTSalaryHead(IEnumerable<SalaryRuleOT> entities, string SalaryRuleMasterSystemID)
        {
            try
            {
                SaveOTSalaryHeadData(entities, SalaryRuleMasterSystemID);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreatePFSalaryHead(IEnumerable<SalaryRulePF> entities, string SalaryRuleMasterSystemID)
        {
            try
            {
                SavePFSalaryHeadData(entities, SalaryRuleMasterSystemID);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        private void SavePFSalaryHeadData(IEnumerable<SalaryRulePF> data, string SalaryRuleMasterSystemID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                if (data != null)
                {
                    int Seqcount = 0;

                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[SalaryRulePF] WHERE SalaryRulePFSystemID='" + item.SalaryRulePFSystemID + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        Seqcount++;

                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["SalaryRulePFSystemID"] = GetPFPK();
                            dr["SalaryRuleMasterSystemID"] = item.SalaryRuleMasterSystemID;
                            dr["SalaryHeadID"] = item.SalaryHeadID;


                            dr["IsIndividual"] = 1;
                            dr["SequenceNo"] = Seqcount;

                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = DateTime.Now;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = DateTime.Now;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
                else
                {
                    string sql = "SELECT * FROM [dbo].[SalaryRulePF] WHERE SalaryRuleMasterSystemID='" + SalaryRuleMasterSystemID + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out DataSet dsPF, false, "1");

                    while (dsPF.Tables[0].DefaultView.Count > 0)
                    {
                        dsPF.Tables[0].DefaultView[0].Delete();
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsPF);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveOTSalaryHeadData(IEnumerable<SalaryRuleOT> data, string SalaryRuleMasterSystemID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                if (data != null)
                {
                    int Seqcount = 0;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[SalaryRuleOT] WHERE SalaryRuleOTSystemID='" + item.SalaryRuleOTSystemID + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        Seqcount++;

                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            //Holiday

                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["SalaryRuleOTSystemID"] = GetOTPK();
                            dr["SalaryRuleMasterSystemID"] = item.SalaryRuleMasterSystemID;
                            dr["SalaryHeadID"] = item.SalaryHeadID;

                            dr["OverTimeDayType"] = "Holiday";

                            dr["IsIndividual"] = 1;
                            dr["SequenceNo"] = 1;

                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = DateTime.Now;

                            dsMaster.Tables[0].Rows.Add(dr);

                            //Working Day

                            DataRow dr1 = dsMaster.Tables[0].NewRow();

                            dr1["SalaryRuleOTSystemID"] = GetOTPK();
                            dr1["SalaryRuleMasterSystemID"] = item.SalaryRuleMasterSystemID;
                            dr1["SalaryHeadID"] = item.SalaryHeadID;

                            dr1["OverTimeDayType"] = "Working Day";

                            dr1["IsIndividual"] = 1;
                            dr1["SequenceNo"] = 2;

                            dr1["AddedBy"] = identity.Name;
                            dr1["DateAdded"] = DateTime.Now;

                            dsMaster.Tables[0].Rows.Add(dr1);

                            //Week Off

                            DataRow dr2 = dsMaster.Tables[0].NewRow();

                            dr2["SalaryRuleOTSystemID"] = GetOTPK();
                            dr2["SalaryRuleMasterSystemID"] = item.SalaryRuleMasterSystemID;
                            dr2["SalaryHeadID"] = item.SalaryHeadID;

                            dr2["OverTimeDayType"] = "Week Off";

                            dr2["IsIndividual"] = 1;
                            dr2["SequenceNo"] = 3;

                            dr2["AddedBy"] = identity.Name;
                            dr2["DateAdded"] = DateTime.Now;

                            dsMaster.Tables[0].Rows.Add(dr2);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = DateTime.Now;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
                else
                {
                    string sql = "SELECT * FROM [dbo].[SalaryRuleOT] WHERE SalaryRuleMasterSystemID='" + SalaryRuleMasterSystemID + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out DataSet dsPF, false, "1");

                    while (dsPF.Tables[0].DefaultView.Count > 0)
                    {
                        dsPF.Tables[0].DefaultView[0].Delete();
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsPF);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveSalaryHeadSettingData(IEnumerable<SalaryHeadSetting> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[SalaryHeadSetting] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        if (!string.IsNullOrEmpty(item.Id) && !item.IsEditable)
                        {

                            DeleteSalaryHeadSettingData(item.Id);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(item.Id) && item.IsEditable)
                            {
                                if (dsMaster.Tables[0].Rows.Count == 0)
                                {
                                    DataRow dr = dsMaster.Tables[0].NewRow();

                                    dr["Id"] = GetPK();
                                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                                    dr["PlantId"] = identity.PlantId;
                                    dr["SalaryRuleId"] = item.SalaryRuleId;
                                    dr["SalaryHeadEnum"] = item.SalaryHeadEnum;
                                    dr["IsEditable"] = item.IsEditable;

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = DateTime.Now;
                                    dr["AddedFromIp"] = identity.IPAddress;

                                    dsMaster.Tables[0].Rows.Add(dr);
                                }

                            }
                            else if (!string.IsNullOrEmpty(item.Id))
                            {
                                //edit
                                DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["CompanyGroupId"] = identity.CompanyGroupId;
                                dr["PlantId"] = identity.PlantId;
                                dr["SalaryRuleId"] = item.SalaryRuleId;
                                dr["SalaryHeadEnum"] = item.SalaryHeadEnum;
                                dr["IsEditable"] = item.IsEditable;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = DateTime.Now.ToString();
                                dr["UpdatedFromIp"] = identity.IPAddress;

                                dr.EndEdit();
                            }
                            clsStaticInfo obj = new clsStaticInfo();
                            obj.SaveDataSets(dsMaster);
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveESICSalaryHeadData(IEnumerable<SalaryRuleESIC> data, string SalaryRuleMasterSystemID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                if (data != null)
                {
                    int Seqcount = 0;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[SalaryRuleESIC] WHERE SalaryRuleESICSystemID='" + item.SalaryRuleESICSystemID + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        Seqcount++;

                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["SalaryRuleESICSystemID"] = GetESICPK();
                            dr["SalaryRuleMasterSystemID"] = item.SalaryRuleMasterSystemID;
                            dr["SalaryHeadID"] = item.SalaryHeadID;

                            dr["IsIndividual"] = 1;
                            dr["SequenceNo"] = Seqcount;

                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = DateTime.Now;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = DateTime.Now;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
                else
                {
                    string sql = "SELECT * FROM [dbo].[SalaryRuleESIC] WHERE SalaryRuleMasterSystemID='" + SalaryRuleMasterSystemID + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out DataSet dsPF, false, "1");

                    while (dsPF.Tables[0].DefaultView.Count > 0)
                    {
                        dsPF.Tables[0].DefaultView[0].Delete();
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsPF);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveAttnBonusSalaryHeadData(IEnumerable<SalaryRuleAttdnBonusPmtMaster> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    int Seqcount = 0;
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[SalaryRuleAttdnBonusPmtMaster] WHERE SalaryRuleAttdnBonusPmtSystemID='" + item.SalaryRuleAttdnBonusPmtSystemID + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        Seqcount++;

                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["SalaryRuleAttdnBonusPmtSystemID"] = GetBonusPK();
                            dr["SalaryRuleMasterSystemID"] = item.SalaryRuleMasterSystemID;
                            dr["SalaryHeadID"] = item.SalaryHeadID;

                            dr["IsIndividual"] = 1;
                            dr["SequenceNo"] = Seqcount;

                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = DateTime.Now;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = DateTime.Now;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveRetentionSalaryHeadData(IEnumerable<SalaryRuleRetentionPmtMaster> data, string SalaryHeadId, List<MinimumWagesSalaryHeadModel> MinimumWagesSalaryHeadLists)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    clsStaticInfo obj = new clsStaticInfo();
                    int Seqcount = 0;
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[SalaryRuleRetentionPmtMaster] WHERE SalaryRuleRetentionPmtSystemID='" + item.SalaryRuleRetentionPmtSystemID + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        Seqcount++;

                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["SalaryRuleRetentionPmtSystemID"] = GetRetentionBonusPK();
                            dr["SalaryRuleMasterSystemID"] = item.SalaryRuleMasterSystemID;
                            dr["SalaryHeadID"] = item.SalaryHeadID;

                            dr["IsIndividual"] = 1;
                            dr["SequenceNo"] = Seqcount;

                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = DateTime.Now;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = DateTime.Now;

                            dr.EndEdit();
                        }




                        try
                        {
                            string strSQL = "Delete FROM [dbo].[SalaryRuleGovtGrd] WHERE SalaryRuleMasterSystemID='" + item.SalaryRuleMasterSystemID + "'";

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




                        if (item.IsBonusRetainMinimumWages == true)
                        {
                            if (MinimumWagesSalaryHeadLists != null)
                            {
                                int Seqcount1 = 0;
                                foreach (var item1 in MinimumWagesSalaryHeadLists)
                                {
                                    DataSet dsSalaryRuleGovtGrd;
                                    string sqlM = "SELECT * FROM [dbo].[SalaryRuleGovtGrd] WHERE SalaryRuleMasterSystemID='" + item.SalaryRuleMasterSystemID + "' AND SalaryHeadID='" + SalaryHeadId + "' AND GovtSalaryHeadID='" + item1.MinimumWagesSalaryHead + "'";
                                    objCon = new ConnectionManager.DAL.ConManager("1");
                                    objCon.OpenDataSetThroughAdapter(sqlM, out dsSalaryRuleGovtGrd, false, "1");

                                    Seqcount1++;

                                    if (dsSalaryRuleGovtGrd.Tables[0].Rows.Count == 0)
                                    {
                                        DataRow dr = dsSalaryRuleGovtGrd.Tables[0].NewRow();

                                        dr["SalaryRuleGovtGrdSystemID"] = GetSalaryRuleGovtGrdPK();
                                        dr["SalaryRuleMasterSystemID"] = item.SalaryRuleMasterSystemID;
                                        dr["SalaryHeadID"] = SalaryHeadId;
                                        dr["GovtSalaryHeadID"] = item1.MinimumWagesSalaryHeadId;

                                        dr["SequenceNo"] = Seqcount1;

                                        dr["AddedBy"] = identity.Name;
                                        dr["DateAdded"] = DateTime.Now;

                                        dsSalaryRuleGovtGrd.Tables[0].Rows.Add(dr);
                                    }
                                    else
                                    {
                                        //edit
                                        DataRow dr = dsSalaryRuleGovtGrd.Tables[0].DefaultView[0].Row;

                                        dr.BeginEdit();
                                        dr["SalaryRuleMasterSystemID"] = item.SalaryRuleMasterSystemID;
                                        dr["SalaryHeadID"] = SalaryHeadId;
                                        dr["GovtSalaryHeadID"] = item1.MinimumWagesSalaryHeadId;

                                        dr["SequenceNo"] = Seqcount1;
                                        dr["UpdatedBy"] = identity.Name;
                                        dr["DateUpdated"] = DateTime.Now;

                                        dr.EndEdit();
                                    }
                                    obj.SaveDataSets(dsSalaryRuleGovtGrd);
                                }

                            }
                        }


                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveAbsenteeismSalaryHeadData(SalaryRuleAbsenteeism data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                int seq = 0;
                DataSet dsSeq, dsSalaryHeadIdGross = null;
                if (data != null)
                {
                    GetAutoSequence(data.SalaryRuleMasterSystemID, out dsSeq);
                    if (dsSeq.Tables[0].Rows.Count > 0)
                    {
                        seq = Convert.ToInt32(dsSeq.Tables[0].Rows[0]["SequenceNo"].ToString());
                    }

                    if (data.IsDeductionOnGross)
                    {
                        GetSalaryHeadID(out dsSalaryHeadIdGross);
                        if (dsSalaryHeadIdGross.Tables[0].Rows.Count > 0)
                        {
                            data.FormulaDesID_NewJoin = dsSalaryHeadIdGross.Tables[0].Rows[0]["SalaryHeadID"].ToString();
                        }
                    }
                    else
                    {
                        data.FormulaDesID_NewJoin = DBNull.Value.ToString();
                    }
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    string sql = "SELECT * FROM [dbo].[SalaryRuleAbsenteeism] WHERE SalaryRuleAbsenteeismSystemID='" + data.SalaryRuleAbsenteeismSystemID + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");



                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {

                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["SalaryRuleAbsenteeismSystemID"] = GetBonusPK();
                        dr["SalaryRuleMasterSystemID"] = data.SalaryRuleMasterSystemID;
                        dr["SalaryHeadID"] = data.SalaryHeadID;

                        dr["IsAbsNetPayEffect"] = data.IsAbsNetPayEffect;
                        dr["IsAbsTagAndUnTag"] = data.IsAbsTagAndUnTag;
                        dr["IsFixed"] = data.IsFixed;
                        dr["FixedValue"] = data.FixedValue;
                        dr["IsFormula"] = data.IsFormula;
                        dr["FormulaDes"] = data.FormulaDes;
                        dr["FormulaDesID"] = data.FormulaDesID;
                        dr["IsFixedMonthDay"] = data.IsFixedMonthDay;
                        dr["FixedMonthDayValue"] = data.FixedMonthDayValue;
                        dr["IsMonthDay"] = data.IsMonthDay;
                        dr["IsMonthWorkDay"] = data.IsMonthWorkDay;
                        dr["IsFixedDisbus"] = data.IsFixedDisbus;
                        dr["IsFixedDisbus"] = data.IsFixedDisbus;
                        dr["BaseOnNetPay"] = data.BaseOnNetPay;
                        dr["IsDeductionOnGross"] = data.IsDeductionOnGross;
                        dr["FormulaDesID_NewJoin"] = data.FormulaDesID_NewJoin;
                        dr["IsDisbusted"] = data.IsDisbusted;
                        dr["SequenceNo"] = seq;

                        dr["AddedBy"] = identity.Name;
                        dr["DateAdded"] = DateTime.Now;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["SalaryHeadID"] = data.SalaryHeadID;
                        dr["IsAbsNetPayEffect"] = data.IsAbsNetPayEffect;
                        dr["IsAbsTagAndUnTag"] = data.IsAbsTagAndUnTag;
                        dr["IsFixed"] = data.IsFixed;
                        dr["FixedValue"] = data.FixedValue;
                        dr["IsFormula"] = data.IsFormula;
                        dr["FormulaDes"] = data.FormulaDes;
                        dr["FormulaDesID"] = data.FormulaDesID;
                        dr["IsFixedMonthDay"] = data.IsFixedMonthDay;
                        dr["FixedMonthDayValue"] = data.FixedMonthDayValue;
                        dr["IsMonthDay"] = data.IsMonthDay;
                        dr["IsMonthWorkDay"] = data.IsMonthWorkDay;
                        dr["IsFixedDisbus"] = data.IsFixedDisbus;
                        dr["IsFixedDisbus"] = data.IsFixedDisbus;
                        dr["BaseOnNetPay"] = data.BaseOnNetPay;
                        dr["IsDisbusted"] = data.IsDisbusted;
                        dr["IsDeductionOnGross"] = data.IsDeductionOnGross;
                        dr["FormulaDesID_NewJoin"] = data.FormulaDesID_NewJoin;
                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = DateTime.Now;

                        dr.EndEdit();
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

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
                strSQL = "DELETE FROM dbo.SalaryRuleMaster WHERE SystemID = '" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from dbo.SalaryHeadSetting where SalaryRuleId=  '" + SystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete [dbo].[SalaryRuleAbsenteeism] where SalaryRuleMasterSystemID= '" + SystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete [dbo].[SalaryRuleAttdnBonusPmtMaster] where SalaryRuleMasterSystemID= '" + SystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete [dbo].[SalaryRuleOT] where SalaryRuleMasterSystemID= '" + SystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete [dbo].[SalaryRuleESIC] where SalaryRuleMasterSystemID= '" + SystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete [dbo].[SalaryRulePF] where SalaryRuleMasterSystemID= '" + SystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete [dbo].[SalaryRuleRetentionPmtMaster] where SalaryRuleMasterSystemID= '" + SystemID + "'", true, "1");
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


        public void DeleteSalaryHeadSettingData(string SystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.SalaryHeadSetting WHERE Id = '" + SystemID + "'";
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

        [HttpPost, Authorize]
        public JsonResult DeleteSalaryRuleGeneral(string id)
        {
            DeleteSalaryRuleGeneralData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteSalaryRuleGeneralData(string SystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.SalaryRuleGeneral WHERE SalaryRuleGeneralSystemID = '" + SystemID + "'";
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


        [HttpPost, Authorize]
        public JsonResult DeleteSalaryRuleAbsent(string id)
        {
            DeleteSalaryRuleAbsentData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteSalaryRuleAbsentData(string SystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.SalaryRuleAbsenteeism WHERE SalaryRuleAbsenteeismSystemID = '" + SystemID + "'";
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

    public class SalaryRuleMaster : BaseModel
    {
        public string SystemID { get; set; }
        public string SalaryRuleName { get; set; }
        public string SalaryRuleDescription { get; set; }
        public string CurrencyRuleSystemID { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public bool IsUsed { get; set; }
        public string TotalSalaryId { get; set; }
        public string TaxGroupID { get; set; }
        public bool IsValidGovGrd { get; set; }
        public bool IsActive { get; set; }
        public string AddedBy { get; set; }
        public string AddedFromIP { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedFromIP { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CurrencyRuleName { get; set; }
        public string IncomeTaxGroup { get; set; }

    }

    public class TaxGroup : BaseModel
    {
        public string SystemID { get; set; }
        public string TaxGroupName { get; set; }
        public string Description { get; set; }
        public bool DefaultGroup { get; set; }
        public string GroupID { get; set; }
    }

    public class SalaryRuleESIC : BaseModel
    {
        public string SalaryRuleESICSystemID { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsIndividual { get; set; }
        public int SequenceNo { get; set; }
    }

    public class SalaryRuleOT : BaseModel
    {
        public string SalaryRuleOTSystemID { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string OverTimeDayType { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsIndividual { get; set; }
        public int SequenceNo { get; set; }
    }

    public class SalaryRulePF : BaseModel
    {
        public string SalaryRulePFSystemID { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsIndividual { get; set; }
        public int SequenceNo { get; set; }
    }

    public class SalaryRuleAbsenteeism : BaseModel
    {
        public string SalaryRuleAbsenteeismSystemID { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public bool IsAbsNetPayEffect { get; set; }
        public bool IsAbsTagAndUnTag { get; set; }
        public bool IsFixed { get; set; }
        public decimal FixedValue { get; set; }
        public bool IsFormula { get; set; }
        public string FormulaDes { get; set; }
        public string FormulaDesID { get; set; }
        public bool IsFixedMonthDay { get; set; }
        public decimal FixedMonthDayValue { get; set; }
        public bool IsMonthDay { get; set; }
        public bool IsMonthWorkDay { get; set; }
        public bool IsFixedDisbus { get; set; }
        public int SequenceNo { get; set; }
        public bool IsDisbusted { get; set; }
        public bool BaseOnNetPay { get; set; }
        public bool IsDeductionOnGross { get; set; }
        public string FormulaDesID_NewJoin { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class SalaryRuleAttdnBonusPmtMaster : BaseModel
    {
        public string SalaryRuleAttdnBonusPmtSystemID { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsIndividual { get; set; }
        public int SequenceNo { get; set; }
    }

    public class SalaryRuleRetentionPmtMaster : BaseModel
    {
        public string SalaryRuleRetentionPmtSystemID { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool IsIndividual { get; set; }
        public int SequenceNo { get; set; }
        public bool IsBonusRetainMinimumWages { get; set; } = false;
    }

    public class SalaryHeadSetting : BaseModel
    {
        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string PlantId { get; set; }
        public string SalaryRuleId { get; set; }
        public string SalaryHeadEnum { get; set; }
        public bool IsEditable { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIp { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIp { get; set; }
    }


    public class MinimumWagesSalaryHeadModel

    {
        public bool CheckBoxSelect { get; set; }
        public string SalaryHeadId { get; set; }

        public string MinimumWagesSalaryHeadId { get; set; }
        public string MinimumWagesSalaryHead { get; set; }

    }

    public class SalaryRuleGeneral : BaseModel
    {
        public string SalaryRuleGeneralSystemID { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public bool IsOpen { get; set; }
        public bool IsFixed { get; set; }
        public bool IsNA { get; set; }
        public decimal FixedValue { get; set; }
        public bool IsFormula { get; set; }
        public string FormulaDes { get; set; }
        public string FormulaDesID { get; set; }
        public bool IsFixedMonthDay { get; set; }
        public decimal FixedMonthDayValue { get; set; }
        public bool IsMonthDay { get; set; }
        public bool IsMonthWorkDay { get; set; }
        public bool IsFixedDisbus { get; set; }
        public int SequenceNo { get; set; }
        public bool IsDisbusted { get; set; }
        public bool BaseOnNetPay { get; set; }
        public bool RefAbsentism { get; set; }
        public string AddedBy { get; set; }
        public string AddedFromIP { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedFromIP { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string GNRBaseOthSlrHDFormula { get; set; }
        public string GNRApplicableMonthNo { get; set; }
        public bool IsGNRBaseOthSlrHD { get; set; }
        public bool IsRetain { get; set; }
        public bool IsMinWages { get; set; }
        public bool IsGNRWhichEverLess { get; set; }
        public bool IsWorkDaysInAMonthIncHold { get; set; }
        public bool HasMaxLimit { get; set; }
        public bool FixedMaxLimit { get; set; }
        public int MaxLimitValue { get; set; }
        public bool PercentageMaxLimit { get; set; }
        public string PercentageMaxLimitSalaryHeadId { get; set; }
        public bool HasMinLimit { get; set; }
        public bool FixedMinLimit { get; set; }
        public bool PercentageMinLimit { get; set; }
        public int MinLimitValue { get; set; }
        public string PercentageMinLimitSalaryHeadId { get; set; }
        public bool IsPolicyDerived { get; set; }
        public bool IsSlabBased { get; set; }
        public bool IsPayOnWeekoffForFixedMonthDay { get; set; }
        public bool IsPayOnHolidayForFixedMonthDay { get; set; }
        public bool IsGNRNetPayEffect { get; set; }
        public bool IsGNRTagAndUnTag { get; set; }
        public bool IsBankPayment { get; set; }
        public bool IsCashPayment { get; set; }
        public bool IsCTCComponent { get; set; }
        public bool IsGrossComponent { get; set; }


    }
}
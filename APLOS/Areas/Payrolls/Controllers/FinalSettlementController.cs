using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Employees;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class FinalSettlementController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;
        public FinalSettlementController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion



        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations


        [HttpGet, Authorize]
        public ActionResult GetSeparationTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,UserName FROM hkp.[SeparationType] ORDER BY Sequence";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetEmployeeFinalSettlementlist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT  EI.SystemId
                          ,EI.EmployeeCode
                         ,EI.EmployeeName
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         , DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         , E.UserName EntityName
                         , FS.Id, FS.SeparationTypeId
                            , st.UserName SeparationTypeName
                            , FORMAT(FS.FinalSettlementDate,'dd-MMM-yyyy') FinalSettlementDate
                            , FS.FormulaDes, FS.PolicyYearNo
                            , FS.PolicyDayNo
                            , FS.SeparationTypeAmount
                            , FS.GratuityAmount
                            , FS.LvEncashmentAmount
                            ---, FS.OthersAmount
                           --- , FS.DeductionAmount
                            , FS.TenureDayNo
                            , FS.TenureMonthNo
                            , FS.TenureYearNo
                            , FS.Remarks
                         FROM [dbo].[EmployeeFinalSettlement] AS FS
                         LEFT JOIN HKP.SeparationType AS st ON st.Id = FS.SeparationTypeId
                         INNER JOIN dbo.Employeeinformation EI ON EI.SystemId = FS.EmpSystemId
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id 
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId	
                         WHERE EI.PlantId='" + identity.PlantId + @"'  ORDER BY  CONVERT(DATETIME,FS.FinalSettlementDate) DESC";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult LoadEmployeelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"SELECT  EI.SystemId
                         ,EI.EmployeeCode
                         ,EI.EmployeeName
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         , DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                         , E.UserName EntityName
                        
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId				
                              WHERE EI.SystemId IN (SELECT EmployeeId FROM TRN.Resignation WHERE ApprovalStatus='Approved' ) AND EI.SystemId NOT IN (SELECT EmpSystemId FROM EmployeeFinalSettlement ) AND
                                    EI.PlantId='" + identity.PlantId + @"'   ORDER BY  ei.EmployeeCodePreFix,ei.EmployeeCodeNumeric";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult SeparationTypeSelectedChange(string EmpSystemId)
        {
            string DOS = string.Empty;
            DataSet dsSalary = null;
            clsFinalSettlement ob = new clsFinalSettlement();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            EmployeeFinalSettlement data = ob.CalculateFinalSettlementValue(EmpSystemId, identity.PlantId, out DOS);
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [Id]
                          ,[Sequence]
                         -- ,[Code]
                         -- ,[ShortName]
                         -- ,[StandardName]
                          ,[UserName]
                         -- ,[Description]
                         -- ,[Remarks]
                         -- ,[Active]
                          ,[DeductionAmount]=0.0
                           FROM [dbo].[FinalSettlementDeductionHead] WHERE Active = 1 And Category='Deduction' ORDER BY Sequence";

            var FinalSettlementDeduction = _sqlRepository.GetDataCollection(sql);


            string sqle = @"SELECT [Id]
                          ,[Sequence]
                         -- ,[Code]
                         -- ,[ShortName]
                         -- ,[StandardName]
                          ,[UserName]
                         -- ,[Description]
                         -- ,[Remarks]
                         -- ,[Active]
                          ,[Amount]=0.0
                           FROM [dbo].[FinalSettlementDeductionHead] WHERE Active = 1 And Category='Earning' ORDER BY Sequence";

            var FinalSettlementEarning = _sqlRepository.GetDataCollection(sqle);
            //var FinalSettlementDeduction = ob.GetFinalSettlementDeduction();
            string sqlRetained = @"SELECT spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID
                         
                            ,SUM(spc.DisbusmentAmount) DisbusmentAmount
                          
                              
                             FROM  SalaryProcChild spc
                             LEFT JOIN SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                             LEFT JOIN SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                             LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID							 
                             LEFT JOIN SalaryDisbursementInAcc sd on sd.MonthNo=spm.MonthNo and sd.YearNo=spm.YearNo and sd.SalaryHeadId=spc.SalaryHeadID and sd.EmpSystemId=spc.EmpInfoSystemID
                             WHERE   sl.IsLocked=1 and sh.IsRetained=1 
							 AND spc.DisbusmentAmount>0 AND spc.EmpInfoSystemID='" + EmpSystemId + @"'
                            and (spm.YearNo<=year('" + DOS + @"') or (spm.YearNo<=year('" + DOS + @"') and spm.MonthNo<=month('" + DOS + @"')))
                            and spc.PlantID='" + identity.PlantId + @"'
	                        and ISNULL( sd.Id,'')=''
 
                            group by spc.EmpInfoSystemID ,sh.SalaryHead,spc.SalaryHeadID
                            order by spc.EmpInfoSystemID";

            var FinalSettlementRetainedHead = _sqlRepository.GetDataCollection(sqlRetained);


            return Json(new { data, FinalSettlementDeduction, FinalSettlementEarning, FinalSettlementRetainedHead }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SaveFinalSettlement(EmployeeFinalSettlement FinalSettlementData, List<DeductionModel> DeductionData, List<DeductionModel> EarningData, List<FinalSettlementRetainedHeadModel> FinalSettlementRetainedHead)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string YearlyCalendarId = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsEmployeeFinalSettlement = null; ;
            DataSet dsleaveEncashment = null;
            DataSet dsLvDetails = null;
            DataSet dsFinalSettlementDeductionDetails = null;
            string FinalSettlementId = string.Empty;


            List<DeductionModel> FinalSettlementDeductionDetailsData = new List<DeductionModel>();
            List<FinalSettlementRetainedHeadModel> FinalSettlementRetainedHeadDetailsData = new List<FinalSettlementRetainedHeadModel>();


            //clsLeaveEncashment olv = new clsLeaveEncashment();
            try
            {

                if (EarningData != null)
                {
                    if (EarningData.Count > 0)
                    {
                        foreach (var item in EarningData)
                        {
                            if (item.EarningAmount > 0)
                            {
                                DeductionModel o = new DeductionModel();
                                o.Id = item.Id;
                                o.Sequence = item.Sequence;
                                o.UserName = item.UserName;
                                o.Amount = item.EarningAmount;
                                FinalSettlementDeductionDetailsData.Add(o);

                            }
                        }
                    }
                }
                if (DeductionData != null)
                {
                    if (DeductionData.Count > 0)
                    {
                        foreach (var item in DeductionData)
                        {
                            if (item.DeductionAmount > 0)
                            {
                                DeductionModel o = new DeductionModel();
                                o.Id = item.Id;
                                o.Sequence = item.Sequence;
                                o.UserName = item.UserName;
                                o.Amount = item.DeductionAmount;
                                FinalSettlementDeductionDetailsData.Add(o);

                            }
                        }
                    }
                }

                if (FinalSettlementRetainedHead != null)
                {
                    if (FinalSettlementRetainedHead.Count > 0)
                    {
                        foreach (var item in FinalSettlementRetainedHead)
                        {
                            if (item.DisbusmentAmount > 0)
                            {
                                FinalSettlementRetainedHeadModel o = new FinalSettlementRetainedHeadModel();
                                o.EmpInfoSystemID = item.EmpInfoSystemID;
                                o.SalaryHeadID = item.SalaryHeadID;
                                o.SalaryHead = item.SalaryHead;
                                o.DisbusmentAmount = item.DisbusmentAmount;
                                FinalSettlementRetainedHeadDetailsData.Add(o);

                            }
                        }
                    }
                }




                string sql = @"SELECT * FROM [dbo].[EmployeeFinalSettlement] WHERE  EmpSystemId='" + FinalSettlementData.EmpSystemId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsEmployeeFinalSettlement, false, "1");



                DataView dvEmployeeFinalSettlement = new DataView(dsEmployeeFinalSettlement.Tables[0]);
                dvEmployeeFinalSettlement.RowFilter = "EmpSystemId='" + FinalSettlementData.EmpSystemId + @"'";

                if (dvEmployeeFinalSettlement.Count == 0)
                {
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EmployeeFinalSettlement", out sID);
                    DataRow dr = dsEmployeeFinalSettlement.Tables[0].NewRow();
                    FinalSettlementId = "FS" + DateTime.Now.ToString("yy") + sID;
                    dr["Id"] = FinalSettlementId;
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                    dr["SeparationTypeId"] = FinalSettlementData.SeparationTypeId;
                    dr["FinalSettlementDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                    dr["PolicyYearNo"] = FinalSettlementData.PolicyYearNo;
                    dr["PolicyDayNo"] = FinalSettlementData.PolicyDayNo;
                    dr["SeparationTypeAmount"] = FinalSettlementData.SeparationTypeAmount;

                    dr["GratuityAmount"] = FinalSettlementData.GratuityAmount;
                    dr["LvEncashmentAmount"] = FinalSettlementData.LvEncashmentAmount;
                    dr["EarningAmount"] = FinalSettlementData.EarningAmount;
                    //dr["DeductionAmount"] = FinalSettlementData.DeductionAmount;
                    dr["TenureDayNo"] = FinalSettlementData.TenureDayNo;
                    dr["TenureMonthNo"] = FinalSettlementData.TenureMonthNo;
                    dr["TenureYearNo"] = FinalSettlementData.TenureYearNo;
                    dr["Remarks"] = FinalSettlementData.Remarks;
                    dr["FormulaDes"] = FinalSettlementData.FormulaDes;
                    dr["GrossAmount"] = FinalSettlementData.GrossAmount;
                    dr["BasicAmount"] = FinalSettlementData.BasicAmount;
                    dr["SalaryRate"] = FinalSettlementData.SalaryRate;
                    dr["OTRate"] = FinalSettlementData.OTRate;

                    dr["PolicyFixedDayNo"] = FinalSettlementData.PolicyFixedDayNo;
                    dr["FixedDayAmount"] = FinalSettlementData.FixedDayAmount;
                    dr["LvEncashmentDayNo"] = FinalSettlementData.LvEncashmentDayNo;
                    dr["LvEncashmentRateAmount"] = FinalSettlementData.LvEncashmentRate;
                    dr["LastMonthProcDay"] = FinalSettlementData.LastMonthProcDay;

                    dr["LastMonthAbsentDay"] = FinalSettlementData.LastMonthAbsentDay;
                    dr["LastMonthOTHour"] = FinalSettlementData.LastMonthOTHour;
                    //dr["StampAmount"] = FinalSettlementData.StampAmount;
                    dr["LastMonthGrossAmount"] = FinalSettlementData.LastMonthGrossAmount;
                    dr["LastMonthAbsenteeismAmount"] = FinalSettlementData.LastMonthAbsenteeismAmount;
                    dr["LastMonthOTAmount"] = FinalSettlementData.LastMonthOTAmount;
                    dr["TotalPayableAmount"] = FinalSettlementData.TotalPayableAmount + FinalSettlementData.GratuityAmount;
                    dr["TotalDeductionAmount"] = FinalSettlementData.TotalDeductionAmount;
                    dr["NetPayAmount"] = FinalSettlementData.TotalNetPayAmount;
                    dr["LastMonthNetPayAmount"] = FinalSettlementData.LastMonthNetPayAmount;

                    dr["EarnLvDeductionDayNo"] = FinalSettlementData.EarnLvDeductionDayNo;
                    dr["EarnLvDeductionAmount"] = FinalSettlementData.EarnLvDeductionAmount;
                    dr["TotalRetainedAmount"] = FinalSettlementData.TotalRetainedAmount;
                    dr["NoticePeriodDayNo"] = FinalSettlementData.NoticePeriodDayNo;
                    dr["NoticePeriodAmount"] = FinalSettlementData.NoticePeriodAmount;
                    dr["NoticePeriodRate"] = FinalSettlementData.NoticePeriodRate;
                    dr["NoticePeriodType"] = FinalSettlementData.NoticePeriodType;









                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsEmployeeFinalSettlement.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dvEmployeeFinalSettlement[0].Row;
                    dr.BeginEdit();
                    FinalSettlementId = dr["Id"].ToString();
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                    dr["SeparationTypeId"] = FinalSettlementData.SeparationTypeId;
                    dr["FinalSettlementDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                    dr["PolicyYearNo"] = FinalSettlementData.PolicyYearNo;
                    dr["PolicyDayNo"] = FinalSettlementData.PolicyDayNo;
                    dr["SeparationTypeAmount"] = FinalSettlementData.SeparationTypeAmount;

                    dr["GratuityAmount"] = FinalSettlementData.GratuityAmount;
                    dr["LvEncashmentAmount"] = FinalSettlementData.LvEncashmentAmount;
                    dr["EarningAmount"] = FinalSettlementData.EarningAmount;
                    //dr["DeductionAmount"] = FinalSettlementData.DeductionAmount;
                    dr["TenureDayNo"] = FinalSettlementData.TenureDayNo;
                    dr["TenureMonthNo"] = FinalSettlementData.TenureMonthNo;
                    dr["TenureYearNo"] = FinalSettlementData.TenureYearNo;
                    dr["Remarks"] = FinalSettlementData.Remarks;
                    dr["FormulaDes"] = FinalSettlementData.FormulaDes;
                    dr["GrossAmount"] = FinalSettlementData.GrossAmount;
                    dr["BasicAmount"] = FinalSettlementData.BasicAmount;
                    dr["SalaryRate"] = FinalSettlementData.SalaryRate;
                    dr["OTRate"] = FinalSettlementData.OTRate;
                    dr["PolicyFixedDayNo"] = FinalSettlementData.PolicyFixedDayNo;
                    dr["FixedDayAmount"] = FinalSettlementData.FixedDayAmount;
                    dr["LvEncashmentDayNo"] = FinalSettlementData.LvEncashmentDayNo;
                    dr["LvEncashmentRateAmount"] = FinalSettlementData.LvEncashmentRate;
                    dr["LastMonthProcDay"] = FinalSettlementData.LastMonthProcDay;

                    dr["LastMonthAbsentDay"] = FinalSettlementData.LastMonthAbsentDay;
                    dr["LastMonthOTHour"] = FinalSettlementData.LastMonthOTHour;
                    //dr["StampAmount"] = FinalSettlementData.StampAmount;
                    dr["LastMonthGrossAmount"] = FinalSettlementData.LastMonthGrossAmount;
                    dr["LastMonthAbsenteeismAmount"] = FinalSettlementData.LastMonthAbsenteeismAmount;
                    dr["LastMonthOTAmount"] = FinalSettlementData.LastMonthOTAmount;
                    dr["TotalPayableAmount"] = FinalSettlementData.TotalPayableAmount + FinalSettlementData.GratuityAmount;
                    dr["TotalDeductionAmount"] = FinalSettlementData.TotalDeductionAmount;
                    dr["NetPayAmount"] = FinalSettlementData.TotalNetPayAmount;
                    dr["LastMonthNetPayAmount"] = FinalSettlementData.LastMonthNetPayAmount;


                    dr["EarnLvDeductionDayNo"] = FinalSettlementData.EarnLvDeductionDayNo;
                    dr["EarnLvDeductionAmount"] = FinalSettlementData.EarnLvDeductionAmount;
                    dr["TotalRetainedAmount"] = FinalSettlementData.TotalRetainedAmount;
                    dr["NoticePeriodDayNo"] = FinalSettlementData.NoticePeriodDayNo;
                    dr["NoticePeriodAmount"] = FinalSettlementData.NoticePeriodAmount;
                    dr["NoticePeriodRate"] = FinalSettlementData.NoticePeriodRate;
                    dr["NoticePeriodType"] = FinalSettlementData.NoticePeriodType;


                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();




                }


                dvEmployeeFinalSettlement.RowFilter = null;


                //Leave Encashment
                if (FinalSettlementData.LvEncashmentDayNo > 0)
                {
                    clsFinalSettlement ob = new clsFinalSettlement();
                    DataSet dsYearlyCalendar = null;
                    ob.GetYearlyCalendarIdByDOS(FinalSettlementData.EmpDOS, identity.PlantId, out dsYearlyCalendar);
                    if (dsYearlyCalendar.Tables[0].Rows.Count > 0)
                    {
                        YearlyCalendarId = dsYearlyCalendar.Tables[0].Rows[0]["Id"].ToString();
                    }


                    string sqll = @"SELECT * FROM LeaveEncashmentTransaction 
                                            WHERE ---PlantId='" + identity.PlantId + @"' AND
                                             EmpSystemId='" + FinalSettlementData.EmpSystemId + @"' AND EncashmentDate='" + Convert.ToDateTime(FinalSettlementData.FinalSettlementDate).ToString("dd-MMM-yyyy") + @"' 
                                            AND LeaveEncashmentType='Final Settlement Encashment' AND YearlyCalendarId='" + YearlyCalendarId + @"'";

                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sqll, out dsleaveEncashment, false, "1");





                    GetLeaveBalance(FinalSettlementData.EmpSystemId, YearlyCalendarId, identity.PlantId, out dsLvDetails);
                    DataView dvleaveEncashment = new DataView(dsleaveEncashment.Tables[0]);


                    dvleaveEncashment.RowFilter = "EmpSystemId='" + FinalSettlementData.EmpSystemId + @"' AND YearlyCalendarId='" + YearlyCalendarId + "'  AND EncashmentDate='" + Convert.ToDateTime(FinalSettlementData.FinalSettlementDate).ToString("dd-MMM-yyyy") + @"'  AND LeaveEncashmentType='Final Settlement Encashment'";

                    if (dvleaveEncashment.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MultipleaveEncashment", out sID);
                        DataRow dr = dsleaveEncashment.Tables[0].NewRow();
                        dr["Id"] = "LE" + sID;
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                        dr["LeaveEncashmentType"] = "Final Settlement Encashment";
                        dr["EncashmentDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = FinalSettlementData.LvEncashmentDayNo;
                        //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                        dr["Rate"] = FinalSettlementData.LvEncashmentRate;
                        dr["LeaveTypeSystemId"] = FinalSettlementData.LeaveTypeId;



                        dr["BasicAmmount"] = FinalSettlementData.BasicAmount;
                        dr["GrossAmmount"] = FinalSettlementData.GrossAmount;


                        //dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;




                        if (dsLvDetails.Tables[0].Rows.Count > 0)
                        {


                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["LegalDesignationId"].ToString()))
                            {
                                dr["LegalDesignationId"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["LegalDesignationId"].ToString());
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BroughtForward"].ToString()))
                            {
                                dr["BroughtForward"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["BroughtForward"].ToString());
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString()))
                            {
                                dr["DaysCanBeSanctioned"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString());
                            }
                            //if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["CurrentYearAllocation"].ToString()))
                            //{
                            //    ob.CurrentYearAllocation = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["CurrentYearAllocation"].ToString());
                            //}
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["CarryForward"].ToString()))
                            {
                                dr["CarryForward"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["CarryForward"].ToString());
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["AvailedLeave"].ToString()))
                            {
                                dr["AvailedLeave"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["AvailedLeave"].ToString());
                            }



                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString()))
                            {
                                dr["PaymentMode"] = dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString()))
                            {
                                dr["BankSystemID"] = dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString()))
                            {
                                dr["BankBranchId"] = dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString()))
                            {
                                dr["BankAccNo"] = dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString();
                            }

                            //dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                            //dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                            //dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                            //dr["BankAccNo"] = leaveEncashment[i].BankAccNo;
                        }
                        //dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        //dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        //dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        //dr["CarryForward"] = leaveEncashment[i].CarryForward;
                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;




                        dsleaveEncashment.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvleaveEncashment[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = FinalSettlementData.EmpSystemId;
                        dr["LeaveEncashmentType"] = "Final Settlement Encashment";
                        dr["EncashmentDate"] = FinalSettlementData.FinalSettlementDate;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["Days"] = FinalSettlementData.LvEncashmentDayNo;
                        //dr["Rest"] = leaveEncashment.Balance-(leaveEncashment.AvailedEncashment+ leaveEncashment.Days);
                        dr["Rate"] = FinalSettlementData.LvEncashmentRate;
                        dr["LeaveTypeSystemId"] = FinalSettlementData.LeaveTypeId;
                        //dr["PaymentMode"] = leaveEncashment[i].PaymentMode;
                        dr["BasicAmmount"] = FinalSettlementData.BasicAmount;
                        dr["GrossAmmount"] = FinalSettlementData.GrossAmount;

                        //dr["BankSystemID"] = leaveEncashment[i].BankSystemID;
                        //dr["BankBranchId"] = leaveEncashment[i].BankBranchId;
                        //dr["BankAccNo"] = leaveEncashment[i].BankAccNo;

                        //dr["LegalDesignationId"] = leaveEncashment[i].LegalDesignationId;



                        if (dsLvDetails.Tables[0].Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["LegalDesignationId"].ToString()))
                            {
                                dr["LegalDesignationId"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["LegalDesignationId"].ToString());
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BroughtForward"].ToString()))
                            {
                                dr["BroughtForward"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["BroughtForward"].ToString());
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString()))
                            {
                                dr["DaysCanBeSanctioned"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["DaysCanBeSanctioned"].ToString());
                            }

                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["CarryForward"].ToString()))
                            {
                                dr["CarryForward"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["CarryForward"].ToString());
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["AvailedLeave"].ToString()))
                            {
                                dr["AvailedLeave"] = Convert.ToDecimal(dsLvDetails.Tables[0].Rows[0]["AvailedLeave"].ToString());
                            }

                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString()))
                            {
                                dr["PaymentMode"] = dsLvDetails.Tables[0].Rows[0]["PaymentMode"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString()))
                            {
                                dr["BankSystemID"] = dsLvDetails.Tables[0].Rows[0]["BankSystemID"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString()))
                            {
                                dr["BankBranchId"] = dsLvDetails.Tables[0].Rows[0]["BankBranchId"].ToString();
                            }
                            if (!string.IsNullOrEmpty(dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString()))
                            {
                                dr["BankAccNo"] = dsLvDetails.Tables[0].Rows[0]["BankAccNo"].ToString();
                            }
                        }

                        //dr["BroughtForward"] = leaveEncashment[i].BroughtForward;
                        //dr["DaysCanBeSanctioned"] = leaveEncashment[i].DaysCanBeSanctioned;
                        //dr["AvailedLeave"] = leaveEncashment[i].AvailedLeave;
                        //dr["CarryForward"] = leaveEncashment[i].CarryForward;



                        dr["Isdisburse"] = true;
                        dr["YearlyCalendarId"] = YearlyCalendarId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                    }
                    dvleaveEncashment.RowFilter = null;

                }

                ///deduction

                string sqld = @"select * from FinalSettlementDeductionDetails where EmployeeFinalSettlementId='" + FinalSettlementId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqld, out dsFinalSettlementDeductionDetails, false, "1");



                DataView dvFinalSettlementDeductionDetails = new DataView(dsFinalSettlementDeductionDetails.Tables[0]);
                if (FinalSettlementDeductionDetailsData.Count > 0)
                {
                    foreach (var item in FinalSettlementDeductionDetailsData)
                    {
                        dvFinalSettlementDeductionDetails.RowFilter = "EmployeeFinalSettlementId='" + FinalSettlementId + @"' and FinalSettlementDeductionHeadId='" + item.Id + @"'";

                        if (dvFinalSettlementDeductionDetails.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "FinalSettlementDeductionDetails", out sID);
                            DataRow dr = dsFinalSettlementDeductionDetails.Tables[0].NewRow();

                            dr["Id"] = "FD" + DateTime.Now.ToString("yy") + sID;
                            dr["FinalSettlementDeductionHeadId"] = item.Id.ToString();
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.Amount;



                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsFinalSettlementDeductionDetails.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            DataRow dr = dvEmployeeFinalSettlement[0].Row;
                            dr.BeginEdit();

                            dr["FinalSettlementDeductionHeadId"] = item.Id;
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.Amount;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();




                        }

                        dvFinalSettlementDeductionDetails.RowFilter = null;
                    }

                }







                ///Retained
                DataSet dsFinalSettlementRetainedDetails = null;
                string sqlRetained = @"select * from [dbo].[FinalSettlementRetainedDetails] where EmployeeFinalSettlementId='" + FinalSettlementId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlRetained, out dsFinalSettlementRetainedDetails, false, "1");



                DataView dvFinalSettlementRetainedDetails = new DataView(dsFinalSettlementRetainedDetails.Tables[0]);
                if (FinalSettlementRetainedHeadDetailsData.Count > 0)
                {
                    foreach (var item in FinalSettlementRetainedHeadDetailsData)
                    {
                        dvFinalSettlementRetainedDetails.RowFilter = "EmployeeFinalSettlementId='" + FinalSettlementId + @"' and SalaryHeadId='" + item.SalaryHeadID + @"'";

                        if (dvFinalSettlementRetainedDetails.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "FinalSettlementRetainedDetails", out sID);
                            DataRow dr = dsFinalSettlementRetainedDetails.Tables[0].NewRow();

                            dr["Id"] = "FD" + DateTime.Now.ToString("yy") + sID;
                            dr["SalaryHeadId"] = item.SalaryHeadID.ToString();
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.DisbusmentAmount;



                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsFinalSettlementRetainedDetails.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            DataRow dr = dvFinalSettlementRetainedDetails[0].Row;
                            dr.BeginEdit();

                            dr["SalaryHeadId"] = item.SalaryHeadID;
                            dr["EmployeeFinalSettlementId"] = FinalSettlementId;
                            dr["Amount"] = item.DisbusmentAmount;


                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();




                        }

                        dvFinalSettlementRetainedDetails.RowFilter = null;
                    }

                }

                clsStaticInfo obj = new clsStaticInfo();


                if (FinalSettlementData.LvEncashmentDayNo > 0)
                {
                    obj.SaveDataSets(dsEmployeeFinalSettlement, dsleaveEncashment, dsFinalSettlementDeductionDetails, dsFinalSettlementRetainedDetails);
                }
                else
                {
                    obj.SaveDataSets(dsEmployeeFinalSettlement, dsFinalSettlementDeductionDetails, dsFinalSettlementRetainedDetails);
                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }
        public void GetLeaveBalance(string EmpSystemId, string YearId, string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType,s.LeaveTypeId,e.LegalDesignationId,    BI.BankSystemID,  BI.BankBranchId,  BI.BankAccNo
                            ,e.GivenDesignationId,e.PaymentMode,e.LegalDesignationId,e.BudgetCode
                            ,s.BroughtForward,s.CarryForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed,s.EncashedInbetween ,s.YearEndEncash
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave

                            --,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)
                            -----------------------------------Is Brought Forward Add to balance -----------------------------------------------------------                                       
							,Balance=CASE WHEN t.LeaveType='Earn' THEN  
								CASE WHEN
								-----------------------------------DOJorDOC start -----------------------------------------------------------
															CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
							---------------------------------------DOJorDOC start  end-------------------------------------------------------
	
								> GETDATE() then 
									  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0)------No
									ELSE  
                        ---isnull(s.BroughtForward,0) 
                        CASE WHEN s.IsEncashed =1 THEN ISNULL(s.CarryForward, 0)+ISNULL(s.EncashedInbetween, 0) ELSE ISNULL(s.BroughtForward, 0)+isnull(s.CarryForwardOpeningBalance,0) END
                        +isnull(s.DaysCanBeSanctioned,0)
                        -isnull(kk.LeaveDuration,0)-
                        isnull(s.EncashedInbetween,0) END---Yes
							ELSE  isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(s.EncashedInbetween,0) END  ---No,
	                        ,DOJorDOC=CASE WHEN ltd.LvAvailedOnDOJ=1 THEN                            										 
                            																	 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  e.DOJ )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  e.DOJ ) END
																	   WHEN  ltd.LvAvailedOnDOC=1 THEN 										   
										   														 CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter,  	e.DOC  )
																									  WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter,  	e.DOC  )
										   													END
																   END
                            from trn.EmployeeLeaveSummary s 
                            INNER join LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            left join EmployeeInformation e on e.SystemId=s.EmployeeId
                            left join (
                            select 
                            tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
                            from 
                            LeaveTransaction t 
                            left join 
                            (--detail
                            select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                            where IsAvailed=1
                            and WorkDate between
                            (select FromDate from YearlyCalendar where Id=" + YearId + @" and PlantId='" + PlantId + @"')
                            and (select ToDate from YearlyCalendar where Id= " + YearId + @" and PlantId='" + PlantId + @"')
                            group by LvTrnsSystemID
                            )--detail 
                            d on t.SystemID=d.LvTrnsSystemID

                            left join LeaveType tt on tt.id=t.LTSystemID
                            where t.IsApproved=1  
                            group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                        -----------------------------------------------------------
						   left outer join (select * from dbo.LeavePolicyDetail
											where LPMSystemID =
											(--w
											select LeavePolicyMasterId from 
													(
														SELECT DC.LeavePolicyMasterId,dm.DesignationId 
																				FROM MST.DesignationMaster DM
																				LEFT JOIN SCS.DesignationMasterConfiguration DC 
																							ON DM.Id=DC.DesignationMasterId
																where dc.plantid='" + PlantId + @"'

													) dm where dm.DesignationId =(select givendesignationId 
																				from dbo.EmployeeInformation 
																				where SystemId='" + EmpSystemId + @"')
											)--w
							) ltd on ltd.LTSystemID = t.Id
                            LEFT JOIN EmployeeBankInfo  AS BI ON BI.EmpSystemID=e.SystemId
						   --------------------------------------------------------------------------
                            where s.CalanderYearId=(select id from YearlyCalendar where Id=" + YearId + @" and PlantId='" + PlantId + @"') AND E.SystemId ='" + EmpSystemId + @"'
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                            ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function






        [HttpGet, Authorize]
        public ActionResult GetDataForEdit(string Id)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string FinalSettlementSql = @"SELECT FS.*,ST.UserName SeparationTypeName FROM [dbo].[EmployeeFinalSettlement] AS FS
                                          LEFT JOIN [HKP].[SeparationType] AS ST ON ST.Id = FS.SeparationTypeId
                                          WHERE FS.PlantId='" + identity.PlantId + @"' AND FS.Id='" + Id + @"'";
            string EmployeeInfosql = @"SELECT  EI.SystemId
                                                 ,EI.EmployeeCode
                                                 ,EI.EmployeeName
                                                 , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                                                 , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                                                 , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                                                 , DG.UserName LegalDesignation
                                                 , DP.UserName Department
                                                 , PMB.Code,PR.UserName PositionName
                                                 , E.UserName EntityName
                        
                                                 FROM dbo.Employeeinformation EI
                                                 LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                                                 LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                                                 LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                                                 LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                                 LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                                 LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                                                 LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                                                 LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId				
                                                      WHERE EI.SystemId IN (SELECT EmpSystemId FROM [dbo].[EmployeeFinalSettlement] WHERE id='" + Id + @"') AND 
                                                            EI.PlantId='" + identity.PlantId + @"' ORDER BY CONVERT(INT, ei.EmployeeCode) ";

            var FinalSettlement = _sqlRepository.GetDataCollection(FinalSettlementSql);
            var EmployeeInfo = _sqlRepository.GetDataCollection(EmployeeInfosql);

            return Json(new { FinalSettlement, EmployeeInfo }, JsonRequestBehavior.AllowGet);
        }





        [HttpGet, Authorize]
        public ActionResult GetEmploymentTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT UserName  FROM EmploymentTypeEnum  ";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT max(Sequence)+1 Sequence FROM hkp.[SeparationType] WHERE PlantID='" + identity.PlantId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        public void Caculate()
        {

        }
        #endregion
    }




}
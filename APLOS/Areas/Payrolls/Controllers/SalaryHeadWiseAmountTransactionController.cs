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
using Library.HumanResource.Payroll.Allowance;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class SalaryHeadWiseAmountTransactionController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;
        public SalaryHeadWiseAmountTransactionController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion



        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //SalaryHeadWiseAmountTransaction o = new SalaryHeadWiseAmountTransaction();
            //o.SalaryHeadWiseAmountCalculation(identity, "01-Aug-2020", "31 - Aug - 2020", "206714");

            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadWiseAmountSettinglist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM SalaryHeadWiseAmountSetting WHERE PlantId='" + identity.PlantId + @"'";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadWiseAmountSettingDetails(string SalaryHeadWiseAmountSettingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM SalaryHeadWiseAmountSetting WHERE PlantId='" + identity.PlantId + @"' AND Id='" + SalaryHeadWiseAmountSettingId + "'";

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
                         LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId				
                         WHERE EI.EmployeeStatus='Active' AND EI.PlantId='" + identity.PlantId + @"'   ORDER BY  ei.EmployeeCodePreFix,ei.EmployeeCodeNumeric";

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



        [HttpGet]
        public ActionResult GetSalaryHeadWiseAmountTransaction(string EmpSystemId, string SalaryHeadWiseAmountSettingId, string DurationType)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsLastUnlockDate = null;
            string sql = @"SELECT TOP 1 FORMAT( DATEADD(MONTH,1, str(MonthNo) + '/1/'+ str(YearNo)),'dd-MMM-yyyy') LastUnLockDate,Month(DATEADD(MONTH,1, str(MonthNo) + '/1/'+ str(YearNo))) LastUnLockMonthNo,YEAR(DATEADD(MONTH,1, str(MonthNo) + '/1/'+ str(YearNo))) LastUnLockYearNo FROM SalaryLock WHERE EmpSystemId='" + EmpSystemId + @"' AND IsLocked=1 ORDER BY YearNo DESC, MonthNo DESC ";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsLastUnlockDate, false, "1");

            string sqlSalaryHeadWiseAmountTransaction = @"SELECT SHWT.[Id]
                                                              ,SHWT.[PlantId]
                                                              ,FORMAT(SHWT.[WorkDate],'dd-MMM-yyyy' ) WorkDate
                                                              ,FORMAT(SHWT.[EffectiveDate],'dd-MMM-yyyy' ) EffectiveDate
                                                              ,SHWT.[EmpSystemId],SHWT.[Particulars]
                                                              ,SHWT.[SalaryHeadWiseAmountSettingId]
                                                              ,SHWT.[YearNo]
                                                              ,SHWT.[MonthNo]
                                                              ,FORMAT(SHWT.[FromDate],'dd-MMM-yyyy' ) FromDate
                                                              ,FORMAT(SHWT.[ToDate],'dd-MMM-yyyy' ) ToDate
                                                              ,SHWT.[Amount]
                                                              ,shwas.AllowanceComponent
                                                        FROM SalaryHeadWiseAmountTransaction SHWT
                                                        LEFT JOIN SalaryHeadWiseAmountSetting AS shwas ON shwas.Id = SHWT.SalaryHeadWiseAmountSettingId
                                                        WHERE SHWT.SalaryHeadWiseAmountSettingId IN ( SELECT Id FROM SalaryHeadWiseAmountSetting WHERE DurationType IN (
                                                        SELECT DurationType FROM SalaryHeadWiseAmountSetting WHERE Id='" + SalaryHeadWiseAmountSettingId + @"'))
                                                        AND SHWT.EmpSystemId='" + EmpSystemId + @"'";

            if (DurationType == "DateSpecific")
            {
                if (dsLastUnlockDate.Tables[0].Rows.Count > 0)
                {
                    sqlSalaryHeadWiseAmountTransaction += " AND SHWT.WorkDate>='" + dsLastUnlockDate.Tables[0].Rows[0]["LastUnLockDate"].ToString() + "' ORDER BY CONVERT(DATETIME,SHWT.WorkDate) DESC";
                }
                else
                {
                    sqlSalaryHeadWiseAmountTransaction += " ORDER BY CONVERT(DATETIME,SHWT.WorkDate) DESC";

                }
            }
            if (DurationType == "Monthly")
            {
                if (dsLastUnlockDate.Tables[0].Rows.Count > 0)
                {
                    sqlSalaryHeadWiseAmountTransaction += " AND SHWT.MonthNo>=" + dsLastUnlockDate.Tables[0].Rows[0]["LastUnLockMonthNo"].ToString() + " AND SHWT.YearNo>=" + dsLastUnlockDate.Tables[0].Rows[0]["LastUnLockYearNo"].ToString() + " ORDER BY YearNo DESC,SHWT.MonthNo DESC";
                }
                else
                {
                    sqlSalaryHeadWiseAmountTransaction += " ORDER BY YearNo DESC,SHWT.MonthNo DESC";

                }
            }
            if (DurationType == "Recurring")
            {
                if (dsLastUnlockDate.Tables[0].Rows.Count > 0)
                {
                    sqlSalaryHeadWiseAmountTransaction += " AND SHWT.ToDate>='" + dsLastUnlockDate.Tables[0].Rows[0]["LastUnLockDate"].ToString() + "' ORDER BY CONVERT(DATETIME,SHWT.ToDate) DESC";
                }
                else
                {
                    sqlSalaryHeadWiseAmountTransaction += "  ORDER BY CONVERT(DATETIME,SHWT.ToDate) DESC";

                }
            }
            var data = _sqlRepository.GetDataCollection(sqlSalaryHeadWiseAmountTransaction);

            return Json(data, JsonRequestBehavior.AllowGet);
        }






        [HttpPost]
        public JsonResult SaveSalaryHeadWiseAmountTransaction(string EmpSystemId, string SalaryHeadWiseAmountSettingId, SalaryHeadWiseAmountTransactionVM SalaryHeadWiseAmountTransactionData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string YearlyCalendarId = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsSalaryHeadWiseAmountTransaction = null;
            DataSet dsSalaryLockData = null;

            string MonthNo = string.Empty;
            string YearNo = string.Empty;










            //clsLeaveEncashment olv = new clsLeaveEncashment();
            try
            {






                if (SalaryHeadWiseAmountTransactionData.DurationType == "DateSpecific")
                {

                    YearNo = Convert.ToDateTime(SalaryHeadWiseAmountTransactionData.WorkDate).Year.ToString();
                    MonthNo = Convert.ToDateTime(SalaryHeadWiseAmountTransactionData.WorkDate).Month.ToString();




                }
                if (SalaryHeadWiseAmountTransactionData.DurationType == "Monthly")
                {

                    YearNo = SalaryHeadWiseAmountTransactionData.YearNo.ToString();
                    MonthNo = SalaryHeadWiseAmountTransactionData.MonthNo.ToString();

                }
                if (SalaryHeadWiseAmountTransactionData.DurationType == "Recurring")
                {

                    YearNo = Convert.ToDateTime(SalaryHeadWiseAmountTransactionData.FromDate).Year.ToString();
                    MonthNo = Convert.ToDateTime(SalaryHeadWiseAmountTransactionData.FromDate).Month.ToString();

                }



                string sqlV = @"SELECT EmpSystemId,	YearNo,	MonthNo,	IsLocked,DateName( month , DateAdd( month , MonthNo , -1 )) AS LockMonthName FROM SalaryLock WHERE EmpSystemId='" + EmpSystemId + @"' AND YearNo='" + YearNo + @"' AND MonthNo='" + MonthNo + @"' AND IsLocked=1 ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlV, out dsSalaryLockData, false, "1");

                if (dsSalaryLockData.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("This Employee Salary is Locked on[" + dsSalaryLockData.Tables[0].Rows[0]["LockMonthName"].ToString() + "-" + dsSalaryLockData.Tables[0].Rows[0]["YearNo"].ToString() + "]");

                }



                string sql = @"SELECT * FROM SalaryHeadWiseAmountTransaction WHERE SalaryHeadWiseAmountSettingId='" + SalaryHeadWiseAmountSettingId + @"' AND EmpSystemId='" + EmpSystemId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsSalaryHeadWiseAmountTransaction, false, "1");



                DataView dvEmployeeFinalSettlement = new DataView(dsSalaryHeadWiseAmountTransaction.Tables[0]);


                if (SalaryHeadWiseAmountTransactionData.DurationType == "DateSpecific")
                {
                    dvEmployeeFinalSettlement.RowFilter = "EmpSystemId='" + EmpSystemId + @"' AND PlantId='" + identity.PlantId + @"' and WorkDate='" + Convert.ToDateTime(SalaryHeadWiseAmountTransactionData.WorkDate).ToString("dd-MMM-yyyy") + @"'";
                    if (dvEmployeeFinalSettlement.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SalaryHeadWiseAmountTransaction", out sID);
                        DataRow dr = dsSalaryHeadWiseAmountTransaction.Tables[0].NewRow();
                        dr["Id"] = "SHWAT" + sID;
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = EmpSystemId;
                        dr["WorkDate"] = SalaryHeadWiseAmountTransactionData.WorkDate;
                        dr["SalaryHeadWiseAmountSettingId"] = SalaryHeadWiseAmountSettingId;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        //dr["YearNo"] = SalaryHeadWiseAmountTransactionData.YearNo;
                        //dr["MonthNo"] = SalaryHeadWiseAmountTransactionData.MonthNo;
                        dr["Amount"] = SalaryHeadWiseAmountTransactionData.Amount;

                        if (SalaryHeadWiseAmountTransactionData.EffectiveDate !=null)
                        {
                            dr["EffectiveDate"] = SalaryHeadWiseAmountTransactionData.EffectiveDate;
                        }
                        

                        dr["Particulars"] = SalaryHeadWiseAmountTransactionData.Particulars;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsSalaryHeadWiseAmountTransaction.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvEmployeeFinalSettlement[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = EmpSystemId;
                        dr["WorkDate"] = SalaryHeadWiseAmountTransactionData.WorkDate;
                        dr["SalaryHeadWiseAmountSettingId"] = SalaryHeadWiseAmountSettingId;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        //dr["YearNo"] = SalaryHeadWiseAmountTransactionData.YearNo;
                        //dr["MonthNo"] = SalaryHeadWiseAmountTransactionData.MonthNo;
                        dr["Amount"] = SalaryHeadWiseAmountTransactionData.Amount;

                        if (SalaryHeadWiseAmountTransactionData.EffectiveDate != null)
                        {
                            dr["EffectiveDate"] = SalaryHeadWiseAmountTransactionData.EffectiveDate;
                        }

                        dr["Particulars"] = SalaryHeadWiseAmountTransactionData.Particulars;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();




                    }


                    dvEmployeeFinalSettlement.RowFilter = null;
                }
                if (SalaryHeadWiseAmountTransactionData.DurationType == "Monthly")
                {
                    dvEmployeeFinalSettlement.RowFilter = "EmpSystemId='" + EmpSystemId + @"' AND PlantId='" + identity.PlantId + @"' AND YearNo='" + SalaryHeadWiseAmountTransactionData.YearNo + "' AND MonthNo='" + SalaryHeadWiseAmountTransactionData.MonthNo + "'";
                    if (dvEmployeeFinalSettlement.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SalaryHeadWiseAmountTransaction", out sID);
                        DataRow dr = dsSalaryHeadWiseAmountTransaction.Tables[0].NewRow();
                        dr["Id"] = "SHWAT" + sID;
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = EmpSystemId;
                        //dr["WorkDate"] = SalaryHeadWiseAmountTransactionData.WorkDate;
                        dr["SalaryHeadWiseAmountSettingId"] = SalaryHeadWiseAmountSettingId;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["YearNo"] = SalaryHeadWiseAmountTransactionData.YearNo;
                        dr["MonthNo"] = SalaryHeadWiseAmountTransactionData.MonthNo;
                        dr["Amount"] = SalaryHeadWiseAmountTransactionData.Amount;

                        if (SalaryHeadWiseAmountTransactionData.EffectiveDate != null)
                        {
                            dr["EffectiveDate"] = SalaryHeadWiseAmountTransactionData.EffectiveDate;
                        }

                        dr["Particulars"] = SalaryHeadWiseAmountTransactionData.Particulars;


                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsSalaryHeadWiseAmountTransaction.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvEmployeeFinalSettlement[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = EmpSystemId;
                        //dr["WorkDate"] = SalaryHeadWiseAmountTransactionData.WorkDate;
                        dr["SalaryHeadWiseAmountSettingId"] = SalaryHeadWiseAmountSettingId;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["YearNo"] = SalaryHeadWiseAmountTransactionData.YearNo;
                        dr["MonthNo"] = SalaryHeadWiseAmountTransactionData.MonthNo;
                        dr["Amount"] = SalaryHeadWiseAmountTransactionData.Amount;

                        if (SalaryHeadWiseAmountTransactionData.EffectiveDate != null)
                        {
                            dr["EffectiveDate"] = SalaryHeadWiseAmountTransactionData.EffectiveDate;
                        }

                        dr["Particulars"] = SalaryHeadWiseAmountTransactionData.Particulars;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();




                    }


                    dvEmployeeFinalSettlement.RowFilter = null;
                }
                if (SalaryHeadWiseAmountTransactionData.DurationType == "Recurring")
                {
                    dvEmployeeFinalSettlement.RowFilter = "EmpSystemId='" + EmpSystemId + @"' AND PlantId='" + identity.PlantId + @"' AND FromDate='" + SalaryHeadWiseAmountTransactionData.FromDate + "' AND ToDate='" + SalaryHeadWiseAmountTransactionData.ToDate + "'";
                    if (dvEmployeeFinalSettlement.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SalaryHeadWiseAmountTransaction", out sID);
                        DataRow dr = dsSalaryHeadWiseAmountTransaction.Tables[0].NewRow();
                        dr["Id"] = "SHWAT" + sID;
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = EmpSystemId;
                        //dr["WorkDate"] = SalaryHeadWiseAmountTransactionData.WorkDate;
                        dr["SalaryHeadWiseAmountSettingId"] = SalaryHeadWiseAmountSettingId;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["FromDate"] = SalaryHeadWiseAmountTransactionData.FromDate;
                        dr["ToDate"] = SalaryHeadWiseAmountTransactionData.ToDate;
                        dr["Amount"] = SalaryHeadWiseAmountTransactionData.Amount;


                        if (SalaryHeadWiseAmountTransactionData.EffectiveDate != null)
                        {
                            dr["EffectiveDate"] = SalaryHeadWiseAmountTransactionData.EffectiveDate;
                        }

                        dr["Particulars"] = SalaryHeadWiseAmountTransactionData.Particulars;


                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsSalaryHeadWiseAmountTransaction.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvEmployeeFinalSettlement[0].Row;
                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId.ToString();
                        dr["EmpSystemId"] = EmpSystemId;
                        //dr["WorkDate"] = SalaryHeadWiseAmountTransactionData.WorkDate;
                        dr["SalaryHeadWiseAmountSettingId"] = SalaryHeadWiseAmountSettingId;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                        dr["FromDate"] = SalaryHeadWiseAmountTransactionData.FromDate;
                        dr["ToDate"] = SalaryHeadWiseAmountTransactionData.ToDate;
                        dr["Amount"] = SalaryHeadWiseAmountTransactionData.Amount;

                        if (SalaryHeadWiseAmountTransactionData.EffectiveDate != null)
                        {
                            dr["EffectiveDate"] = SalaryHeadWiseAmountTransactionData.EffectiveDate;
                        }


                        dr["Particulars"] = SalaryHeadWiseAmountTransactionData.Particulars;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();




                    }


                    dvEmployeeFinalSettlement.RowFilter = null;
                }








                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsSalaryHeadWiseAmountTransaction);


            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }


        [HttpPost, Authorize]
        public JsonResult DeleteSalaryHeadWiseAmountTransaction(string Id)
        {


            //clsLeaveEncashment olv = new clsLeaveEncashment();
            try
            {



                //throw new Exception("test");
                bool IsTransactionStarted = false;
                ConnectionManager.DAL.ConManager objCon = null;
                try
                {
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenConnection("1");
                    objCon.BeginTransaction();
                    IsTransactionStarted = true;
                    objCon.ExecuteNonQueryWrapper("Delete FROM SalaryHeadWiseAmountTransaction WHERE  Id='" + Id + "'", true, "1");


                    objCon.CommitTransaction();
                    IsTransactionStarted = false;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                    objCon = null;
                }


            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted });
        }


        #endregion
    }
    public class SalaryHeadWiseAmountTransactionVM
    {
        public string Id { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? WorkDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SalaryHeadWiseAmountSettingId { get; set; }
        public string EmpSystemId { get; set; }
        public string YearNo { get; set; }
        public string MonthNo { get; set; }
        public decimal Amount { get; set; }
        public string DurationType { get; set; }
        public string Particulars { get; set; }
    }
}
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

    public class EmployeeFixedServicTransactionController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>

        private readonly ISqlRepository _sqlRepository;
        public EmployeeFixedServicTransactionController(ISqlRepository sqlRepository)
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
            string sql = @"SELECT * FROM [dbo].[EmployeeFixedServiceMaster] WHERE CompanyGroupId='" + identity.CompanyGroupId + @"'";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetSalaryHeadWiseAmountSettingDetails(string SalaryHeadWiseAmountSettingId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    string sql = @"SELECT * FROM SalaryHeadWiseAmountSetting WHERE PlantId='" + identity.PlantId + @"' AND Id='" + SalaryHeadWiseAmountSettingId + "'";

        //    var data = _sqlRepository.GetDataCollection(sql);

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}


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
                         , E.UserName EntityName,EI.PlantId
                        
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



        //[HttpGet]
        //public ActionResult GetSalaryHeadWiseAmountTransaction(string EmpSystemId, string SalaryHeadWiseAmountSettingId, string DurationType)
        //{
        //    ConnectionManager.DAL.ConManager objCon;
        //    DataSet dsLastUnlockDate = null;
        //    string sql = @"SELECT TOP 1 FORMAT( DATEADD(MONTH,1, str(MonthNo) + '/1/'+ str(YearNo)),'dd-MMM-yyyy') LastUnLockDate,Month(DATEADD(MONTH,1, str(MonthNo) + '/1/'+ str(YearNo))) LastUnLockMonthNo,YEAR(DATEADD(MONTH,1, str(MonthNo) + '/1/'+ str(YearNo))) LastUnLockYearNo FROM SalaryLock WHERE EmpSystemId='" + EmpSystemId + @"' AND IsLocked=1 ORDER BY YearNo DESC, MonthNo DESC ";
        //    objCon = new ConnectionManager.DAL.ConManager("1");
        //    objCon.OpenDataSetThroughAdapter(sql, out dsLastUnlockDate, false, "1");

        //    string sqlSalaryHeadWiseAmountTransaction = @"SELECT SHWT.[Id]
        //                                                      ,SHWT.[PlantId]
        //                                                      ,FORMAT(SHWT.[WorkDate],'dd-MMM-yyyy' ) WorkDate
        //                                                      ,FORMAT(SHWT.[EffectiveDate],'dd-MMM-yyyy' ) EffectiveDate
        //                                                      ,SHWT.[EmpSystemId],SHWT.[Particulars]
        //                                                      ,SHWT.[SalaryHeadWiseAmountSettingId]
        //                                                      ,SHWT.[YearNo]
        //                                                      ,SHWT.[MonthNo]
        //                                                      ,FORMAT(SHWT.[FromDate],'dd-MMM-yyyy' ) FromDate
        //                                                      ,FORMAT(SHWT.[ToDate],'dd-MMM-yyyy' ) ToDate
        //                                                      ,SHWT.[Amount]
        //                                                      ,shwas.AllowanceComponent
        //                                                FROM SalaryHeadWiseAmountTransaction SHWT
        //                                                LEFT JOIN SalaryHeadWiseAmountSetting AS shwas ON shwas.Id = SHWT.SalaryHeadWiseAmountSettingId
        //                                                WHERE SHWT.SalaryHeadWiseAmountSettingId IN ( SELECT Id FROM SalaryHeadWiseAmountSetting WHERE DurationType IN (
        //                                                SELECT DurationType FROM SalaryHeadWiseAmountSetting WHERE Id='" + SalaryHeadWiseAmountSettingId + @"'))
        //                                                AND SHWT.EmpSystemId='" + EmpSystemId + @"'";

        //    if (DurationType == "DateSpecific")
        //    {
        //        if (dsLastUnlockDate.Tables[0].Rows.Count > 0)
        //        {
        //            sqlSalaryHeadWiseAmountTransaction += " AND SHWT.WorkDate>='" + dsLastUnlockDate.Tables[0].Rows[0]["LastUnLockDate"].ToString() + "' ORDER BY CONVERT(DATETIME,SHWT.WorkDate) DESC";
        //        }
        //        else
        //        {
        //            sqlSalaryHeadWiseAmountTransaction += " ORDER BY CONVERT(DATETIME,SHWT.WorkDate) DESC";

        //        }
        //    }
        //    if (DurationType == "Monthly")
        //    {
        //        if (dsLastUnlockDate.Tables[0].Rows.Count > 0)
        //        {
        //            sqlSalaryHeadWiseAmountTransaction += " AND SHWT.MonthNo>=" + dsLastUnlockDate.Tables[0].Rows[0]["LastUnLockMonthNo"].ToString() + " AND SHWT.YearNo>=" + dsLastUnlockDate.Tables[0].Rows[0]["LastUnLockYearNo"].ToString() + " ORDER BY YearNo DESC,SHWT.MonthNo DESC";
        //        }
        //        else
        //        {
        //            sqlSalaryHeadWiseAmountTransaction += " ORDER BY YearNo DESC,SHWT.MonthNo DESC";

        //        }
        //    }
        //    if (DurationType == "Recurring")
        //    {
        //        if (dsLastUnlockDate.Tables[0].Rows.Count > 0)
        //        {
        //            sqlSalaryHeadWiseAmountTransaction += " AND SHWT.ToDate>='" + dsLastUnlockDate.Tables[0].Rows[0]["LastUnLockDate"].ToString() + "' ORDER BY CONVERT(DATETIME,SHWT.ToDate) DESC";
        //        }
        //        else
        //        {
        //            sqlSalaryHeadWiseAmountTransaction += "  ORDER BY CONVERT(DATETIME,SHWT.ToDate) DESC";

        //        }
        //    }
        //    var data = _sqlRepository.GetDataCollection(sqlSalaryHeadWiseAmountTransaction);

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}


        [HttpGet]
        public ActionResult GetSalaryHeadWiseAmountTransaction(string EmpSystemId , string EmpServiceId)
        {
           
        
            

            string sqlSalaryHeadWiseAmountTransaction = @"SELECT SHWT.[Id]
                                                              ,SHWT.[PlantId]                                                              
                                                              ,FORMAT(SHWT.[EffectiveDate],'dd-MMM-yyyy' ) EffectiveDate
                                                              ,SHWT.[EmpSystemId],SHWT.[Particulars],SHWT.[Remarks],SHWT.[Active]
                                                              ,SHWT.EmployeeFixedServicId                                                              
                                                              ,SHWT.[Amount]
                                                              ,shwas.ServicComponent
                                                        FROM EmployeeFixedServiceTransaction SHWT
                                                        LEFT JOIN EmployeeFixedServiceMaster AS shwas ON shwas.Id = SHWT.EmployeeFixedServicId
                                                        WHERE  SHWT.EmpSystemId='" + EmpSystemId + @"' and SHWT.EmployeeFixedServicId = '"+EmpServiceId+"' AND SHWT.Active=1";


            var data = _sqlRepository.GetDataCollection(sqlSalaryHeadWiseAmountTransaction);

            return Json(data, JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        public JsonResult SaveSalaryHeadWiseAmountTransaction(EmployeeFixedServicTransactionVM SalaryHeadWiseAmountTransactionData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string YearlyCalendarId = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsSalaryHeadWiseAmountTransaction = null;
           









            //clsLeaveEncashment olv = new clsLeaveEncashment();
            try
            {






              



                string sql = @"SELECT * FROM EmployeeFixedServiceTransaction WHERE   EmpSystemId='" + SalaryHeadWiseAmountTransactionData.EmpSystemId + @"' and EmployeeFixedServicId='" + SalaryHeadWiseAmountTransactionData.EmployeeFixedServicId + @"' and EffectiveDate>='" + SalaryHeadWiseAmountTransactionData.EffectiveDate + @"' and Active=1";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsSalaryHeadWiseAmountTransaction, false, "1");



                DataView dvEmployeeFinalSettlement = new DataView(dsSalaryHeadWiseAmountTransaction.Tables[0]);
                dvEmployeeFinalSettlement.RowFilter = "EmpSystemId='" + SalaryHeadWiseAmountTransactionData.EmpSystemId + @"' AND EmployeeFixedServicId='" + SalaryHeadWiseAmountTransactionData.EmployeeFixedServicId + @"' and EffectiveDate='" + Convert.ToDateTime(SalaryHeadWiseAmountTransactionData.EffectiveDate).ToString("dd-MMM-yyyy") + @"'";
                if (dvEmployeeFinalSettlement.Count == 0)
                {
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EmployeeFixedServicTransaction", out sID);
                    DataRow dr = dsSalaryHeadWiseAmountTransaction.Tables[0].NewRow();
                    dr["Id"] = "SHWAT" + sID;
                    dr["PlantId"] = SalaryHeadWiseAmountTransactionData.PlantId.ToString();
                    dr["EmpSystemId"] = SalaryHeadWiseAmountTransactionData.EmpSystemId;
                    dr["EffectiveDate"] = SalaryHeadWiseAmountTransactionData.EffectiveDate;
                    dr["EmployeeFixedServicId"] = SalaryHeadWiseAmountTransactionData.EmployeeFixedServicId;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                  
                    dr["Amount"] = SalaryHeadWiseAmountTransactionData.Amount;

                    dr["Active"] = SalaryHeadWiseAmountTransactionData.Active;
                    dr["Remarks"] = SalaryHeadWiseAmountTransactionData.Remarks;

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
                    dr["PlantId"] = SalaryHeadWiseAmountTransactionData.PlantId.ToString();
                    dr["EmpSystemId"] = SalaryHeadWiseAmountTransactionData.EmpSystemId;
                    dr["EffectiveDate"] = SalaryHeadWiseAmountTransactionData.EffectiveDate;
                    dr["EmployeeFixedServicId"] = SalaryHeadWiseAmountTransactionData.EmployeeFixedServicId;  /// System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();

                    dr["Amount"] = SalaryHeadWiseAmountTransactionData.Amount;

                    dr["Active"] = SalaryHeadWiseAmountTransactionData.Active;
                    dr["Remarks"] = SalaryHeadWiseAmountTransactionData.Remarks;

                    dr["Particulars"] = SalaryHeadWiseAmountTransactionData.Particulars;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();




                }


                dvEmployeeFinalSettlement.RowFilter = null;

               
               
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsSalaryHeadWiseAmountTransaction);


            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }


        [HttpPost,Authorize]
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
                    objCon.ExecuteNonQueryWrapper("Delete FROM EmployeeFixedServicTransaction WHERE  Id='" + Id + "'", true, "1");


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
    public class EmployeeFixedServicTransactionVM
    {
        public string Id { get; set; }
        public DateTime? EffectiveDate { get; set; }        
        public string EmployeeFixedServicId { get; set; }
        public string EmpSystemId { get; set; }
        public string PlantId { get; set; }
        public bool Active { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public string Particulars { get; set; }
    }
}
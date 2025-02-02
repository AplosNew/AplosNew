#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using System.Collections.Generic;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Web.Script.Serialization;
using System;
using clsAttendance;
using Library.Data.Sql;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class AttendanceRawDataDeleteController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public AttendanceRawDataDeleteController(
               ISqlRepository sqlRepository
            )
        {

            _sqlRepository = sqlRepository;
        }
        #endregion

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region Employee wise
        [HttpGet, Authorize]
        public ActionResult GetAllEmploteeList()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [CheckBoxSelect] = Convert(BIT, 'False') 
	                            ,E.SystemId
	                            ,e.EmployeeCode
	                            ,e.EmployeeName
	                            ,FORMAT(e.DOJ, 'dd-MMM-yyyy') DOJ
	                            ,EC.UserName EmpCategoryName
	                            ,ld.UserName Designation
	                            ,U.UserName Unit
	                            ,Dv.UserName Division
	                            ,Dp.UserName Department
	                            ,Se.UserName Section
	                            ,SB.UserName SubSection
	                            ,L.UserName Line
                           FROM  EmployeeInformation e 
                            LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                            LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON PR.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON PR.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON PR.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON PR.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON MB.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON PR.DesignationID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld ON E.LegalDesignationId = ld.Id
                            LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                            WHERE e.PlantId= '" + identity.PlantId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpGet, Authorize]
        public ActionResult GetAttendanceRawDataEmployeeWise(string FromDate, string ToDate, string EmpSystemId)

        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [CheckBoxSelect] = Convert(BIT, 'False')
	                            ,FORMAT(ard.PDate, 'dd-MMM-yyyy') PDate
	                            ,ard.PType
	                            ,ard.Id
	                            ,FORMAT(ard.PTime, 'hh:mm tt') PTime
	                            ,ard.ProcessedFlag
	                            ,E.SystemId
	                            ,e.EmployeeCode
	                            ,e.EmployeeName
	                            ,FORMAT(e.DOJ, 'dd-MMM-yyyy') DOJ
	                            ,EC.UserName EmpCategoryName
	                            ,ld.UserName Designation
	                            ,U.UserName Unit
	                            ,Dv.UserName Division
	                            ,Dp.UserName Department
	                            ,Se.UserName Section
	                            ,SB.UserName SubSection
	                            ,L.UserName Line
	                            ,InTimeRowID= CASE WHEN apd.InTimeRowID = ard.RowID THEN 'YES' ELSE 'NO' END
	                            ,OutTimeRowID= CASE WHEN apd.OutTimeRowID = ard.RowID THEN 'YES' ELSE 'NO' end
                            FROM AttdnRawData ard
                            INNER JOIN EmployeeInformation e ON e.SystemId = ard.LogDownLoadNum
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld ON E.LegalDesignationId = ld.Id
                            LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
							LEFT JOIN AttdnProcessData AS apd ON apd.EmpSystemID = ard.LogDownLoadNum AND apd.WorkDate = ard.PDate
                            WHERE ard.PDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' AND ard.LogDownLoadNum='" + EmpSystemId + @"' AND ard.PlantID='" + identity.PlantId + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpPost, Authorize]
        public ActionResult SaveAttendanceRawDataEmployeeWise(List<AttendanceRawDataVM> AttendanceRawData, string pFromDate, string pToDate)
        {
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string AttendanceRawDataId = "";
            string EmpSytemId = "";
            DataSet dsRef = null;
            DataSet dsGetdataRef = null;
            DataSet dsSaveddataRef = null;
            DataRow drSaveSummary = null;
            string strSQL;
            string strSQL1;
            string strSQL2;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                for (int i = 0; i < AttendanceRawData.Count; i++)
                {
                    if (AttendanceRawDataId == "")
                        AttendanceRawDataId = "'" + AttendanceRawData[i].Id.ToString() + "'";
                    else
                        AttendanceRawDataId = AttendanceRawDataId + ",'" + AttendanceRawData[i].Id.ToString() + "'";
                }

                for (int i = 0; i < AttendanceRawData.Count; i++)
                {
                    if (EmpSytemId == "")
                        EmpSytemId = "'" + AttendanceRawData[i].SystemId.ToString() + "'";
                    //else
                    //    EmpSytemId = EmpSytemId + ",'" + AttendanceRawData[i].SystemId.ToString() + "'";
                }
                clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                DateTime FromDate = Convert.ToDateTime(pFromDate);
                DateTime ToDate = Convert.ToDateTime(pToDate);
                try
                {

                    if (EmpSytemId != "")
                    {
                        obj.LockValidation(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), ToDate.ToString("dd-MMM-yyyy"), EmpSytemId);
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }


                strSQL1 = @"SELECT * FROM AttdnRawData WHERE Id IN (" + AttendanceRawDataId + ") AND PlantID='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL1, out dsGetdataRef, false, "1");




                strSQL2 = @"SELECT * FROM AttdnRawDataBackUp WHERE Id IN (" + AttendanceRawDataId + ") AND PlantID='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL2, out dsSaveddataRef, false, "1");





                DataView dvSaveSummary = new DataView(dsSaveddataRef.Tables[0]);
                for (int i = 0; i < dsGetdataRef.Tables[0].Rows.Count; i++)
                {


                    dvSaveSummary.RowFilter = " Id ='" + dsGetdataRef.Tables[0].Rows[i]["Id"] + "' AND PlantID = '" + identity.PlantId + @"'";

                    if (dvSaveSummary.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AttdnRawDataBackUp", out sID);
                        DataRow dr = dsSaveddataRef.Tables[0].NewRow();
                        dr["Id"] = "AB" + sID;
                        dr["DeviceID"] = dsGetdataRef.Tables[0].Rows[i]["DeviceID"];
                        dr["DevSystemID"] = dsGetdataRef.Tables[0].Rows[i]["DevSystemID"];
                        dr["LogDownLoadNum"] = dsGetdataRef.Tables[0].Rows[i]["LogDownLoadNum"];
                        dr["PDate"] = dsGetdataRef.Tables[0].Rows[i]["PDate"];
                        dr["PTime"] = dsGetdataRef.Tables[0].Rows[i]["PTime"];
                        dr["PType"] = dsGetdataRef.Tables[0].Rows[i]["PType"];
                        dr["ProcessedFlag"] = dsGetdataRef.Tables[0].Rows[i]["ProcessedFlag"];
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["PlantID"] = identity.PlantId.ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["DateAdded"] = System.DateTime.Now.ToString();
                        dr["BackupType"] = "RAWDATADELETE";
                        dsSaveddataRef.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvSaveSummary[0].Row;
                        dr.BeginEdit();
                        dr["DeviceID"] = dsGetdataRef.Tables[0].Rows[i]["DeviceID"];
                        dr["DevSystemID"] = dsGetdataRef.Tables[0].Rows[i]["DevSystemID"];
                        dr["LogDownLoadNum"] = dsGetdataRef.Tables[0].Rows[i]["LogDownLoadNum"];
                        dr["PDate"] = dsGetdataRef.Tables[0].Rows[i]["PDate"];
                        dr["PTime"] = dsGetdataRef.Tables[0].Rows[i]["PTime"];
                        dr["PType"] = dsGetdataRef.Tables[0].Rows[i]["PType"];
                        dr["ProcessedFlag"] = dsGetdataRef.Tables[0].Rows[i]["ProcessedFlag"];
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["PlantID"] = identity.PlantId.ToString();
                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = System.DateTime.Now.ToString();
                        dr["BackupType"] = "RAWDATADELETE";
                        dr.EndEdit();

                    }
                    dvSaveSummary.RowFilter = null;
                    //Old year insert 
                }
                SaveAttendanceRawDataBackupDataSetsAndDelete(AttendanceRawDataId, dsSaveddataRef);






                while (FromDate <= ToDate)
                {

                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    ReturnType r = obj.SaveTotal(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), EmpSytemId, false);//laila                 
                    FromDate = FromDate.AddDays(1);
                }


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }





            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Date wise data delete
        [HttpPost]
        public ActionResult SaveAttendanceRawDataDateWise(List<AttendanceRawDataVM> AttendanceRawData, string WDate)
        {
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string AttendanceRawDataId = "";
            string EmpSytemId = "";
            DataSet dsRef = null;
            DataSet dsGetdataRef = null;
            DataSet dsSaveddataRef = null;
            DataRow drSaveSummary = null;
            string strSQL;
            string strSQL1;
            string strSQL2;

            ConnectionManager.DAL.ConManager objCon;
            try
            {

                for (int i = 0; i < AttendanceRawData.Count; i++)
                {
                    if (AttendanceRawDataId == "")
                        AttendanceRawDataId = "'" + AttendanceRawData[i].Id.ToString() + "'";
                    else
                        AttendanceRawDataId = AttendanceRawDataId + ",'" + AttendanceRawData[i].Id.ToString() + "'";
                }

                for (int i = 0; i < AttendanceRawData.Count; i++)
                {
                    if (EmpSytemId == "")
                        EmpSytemId = "'" + AttendanceRawData[i].SystemId.ToString() + "'";
                    else
                        EmpSytemId = EmpSytemId + ",'" + AttendanceRawData[i].SystemId.ToString() + "'";
                }
                clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                DateTime ToDate = Convert.ToDateTime(WDate);
                obj.LockValidation(identity.PlantId, ToDate.ToString("dd-MMM-yyyy"), ToDate.ToString("dd-MMM-yyyy"), EmpSytemId);



                strSQL1 = @"SELECT * FROM AttdnRawData WHERE Id IN (" + AttendanceRawDataId + ") AND PlantID='" + identity.PlantId + @"' AND PDate='" + WDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL1, out dsGetdataRef, false, "1");




                strSQL2 = @"SELECT * FROM AttdnRawDataBackUp WHERE Id IN (" + AttendanceRawDataId + ") AND PlantID='" + identity.PlantId + @"' AND  PDate='" + WDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL2, out dsSaveddataRef, false, "1");





                DataView dvSaveSummary = new DataView(dsSaveddataRef.Tables[0]);
                for (int i = 0; i < dsGetdataRef.Tables[0].Rows.Count; i++)
                {


                    dvSaveSummary.RowFilter = " Id ='" + dsGetdataRef.Tables[0].Rows[i]["Id"] + "' AND PlantID = '" + identity.PlantId + @"' AND PDate = '" + WDate + @"'";

                    if (dvSaveSummary.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AttdnRawDataBackUp", out sID);
                        DataRow dr = dsSaveddataRef.Tables[0].NewRow();
                        dr["Id"] = "AB" + sID;
                        dr["DeviceID"] = dsGetdataRef.Tables[0].Rows[i]["DeviceID"];
                        dr["DevSystemID"] = dsGetdataRef.Tables[0].Rows[i]["DevSystemID"];
                        dr["LogDownLoadNum"] = dsGetdataRef.Tables[0].Rows[i]["LogDownLoadNum"];
                        dr["PDate"] = dsGetdataRef.Tables[0].Rows[i]["PDate"];
                        dr["PTime"] = dsGetdataRef.Tables[0].Rows[i]["PTime"];
                        dr["PType"] = dsGetdataRef.Tables[0].Rows[i]["PType"];
                        dr["ProcessedFlag"] = dsGetdataRef.Tables[0].Rows[i]["ProcessedFlag"];
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["PlantID"] = identity.PlantId.ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["DateAdded"] = System.DateTime.Now.ToString();
                        dr["BackupType"] = "RAWDATADELETE";
                        dsSaveddataRef.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dvSaveSummary[0].Row;
                        dr.BeginEdit();
                        dr["DeviceID"] = dsGetdataRef.Tables[0].Rows[i]["DeviceID"];
                        dr["DevSystemID"] = dsGetdataRef.Tables[0].Rows[i]["DevSystemID"];
                        dr["LogDownLoadNum"] = dsGetdataRef.Tables[0].Rows[i]["LogDownLoadNum"];
                        dr["PDate"] = dsGetdataRef.Tables[0].Rows[i]["PDate"];
                        dr["PTime"] = dsGetdataRef.Tables[0].Rows[i]["PTime"];
                        dr["PType"] = dsGetdataRef.Tables[0].Rows[i]["PType"];
                        dr["ProcessedFlag"] = dsGetdataRef.Tables[0].Rows[i]["ProcessedFlag"];
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["PlantID"] = identity.PlantId.ToString();
                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = System.DateTime.Now.ToString();
                        dr["BackupType"] = "RAWDATADELETE";
                        dr.EndEdit();

                    }
                    dvSaveSummary.RowFilter = null;
                    //Old year insert 
                }
                SaveAttendanceRawDataBackupDataSetsAndDelete(AttendanceRawDataId, dsSaveddataRef);






                AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                ReturnType r = obj.SaveTotal(identity.PlantId, ToDate.ToString("dd-MMM-yyyy"), EmpSytemId, false);//laila                 


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }





            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAttendanceRawDataDateWise(string WDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [CheckBoxSelect] = Convert(BIT, 'False')
	                            ,FORMAT(ard.PDate, 'dd-MMM-yyyy') PDate
	                            ,ard.PType
	                            ,ard.Id
	                            ,FORMAT(ard.PTime, 'hh:mm tt') PTime
	                            ,ard.ProcessedFlag
	                            ,E.SystemId
	                            ,e.EmployeeCode
	                            ,e.EmployeeName
	                            ,FORMAT(e.DOJ, 'dd-MMM-yyyy') DOJ
	                            ,EC.UserName EmpCategoryName
	                            ,ld.UserName Designation
	                            ,U.UserName Unit
	                            ,Dv.UserName Division
	                            ,Dp.UserName Department
	                            ,Se.UserName Section
	                            ,SB.UserName SubSection
	                            ,L.UserName Line
 	                            ,InTimeRowID= CASE WHEN apd.InTimeRowID = ard.RowID THEN 'YES' ELSE 'NO' END
	                            ,OutTimeRowID= CASE WHEN apd.OutTimeRowID = ard.RowID THEN 'YES' ELSE 'NO' end
                            FROM AttdnRawData ard
                            INNER JOIN EmployeeInformation e ON e.SystemId = ard.LogDownLoadNum
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld ON E.LegalDesignationId = ld.Id
                            LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
							LEFT JOIN AttdnProcessData AS apd ON apd.EmpSystemID = ard.LogDownLoadNum AND apd.WorkDate = ard.PDate
                            WHERE ard.PDate = '" + WDate + @"' AND ard.PlantID='" + identity.PlantId + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        #endregion


        public void SaveAttendanceRawDataBackupDataSetsAndDelete(string AttendanceRawDataId, params DataSet[] dsRef)
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
                objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnRawData WHERE Id IN (" + AttendanceRawDataId + ")", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon = null;
            }
        }//End Function
    }

    public class AttendanceRawDataVM
    {
        public bool CheckBoxSelect { get; set; }
        public string PDate { get; set; }
        public string PType { get; set; }
        public string Id { get; set; }
        public string PTime { get; set; }
        public bool ProcessedFlag { get; set; }
        public string SystemId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DOJ { get; set; }
        public string EmpCategoryName { get; set; }
        public string Designation { get; set; }
        public string Unit { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public string Line { get; set; }
    }
}
#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LongAbsenteeismAssignController : BaseController
    {


        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public LongAbsenteeismAssignController(IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor
        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        private string LongAbsenteeismQuery(out int days)
        {

            try
            {
                days = AbsenteeismDays();
                if (days == 0)
                {
                    throw new Exception("No absenteeism policy found for the plant");

                }

                string FromDate = System.DateTime.Now.AddDays((days * 2 + 30) * -1).ToString("dd-MMM-yyyy");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                return @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,e.EmpPicPath,
                                DEP.UserName AS Department,de.UserName AS designation,
                                sec.UserName AS Section,ss.UserName AS SubSection,
                                D.DayStatus,COUNT(d.DayStatus) AS AbsentCount,ab.AbsentDays,format(ab.FirstAbsentDate,'dd-MMM-yyyy') as FirstAbsentDate
                                FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM AttdnProcessData AS P
		                                WHERE p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) 
	                                ) AS D
                                INNER JOIN EmployeeInformation AS E ON e.SystemId=d.EmpSystemID 
                            LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT OUTER JOIN org.Department AS DEP ON dep.Id=PR.DepartmentId
                                LEFT OUTER JOIN hkp.LegalDesignation AS DE ON de.Id=e.LegalDesignationId
                                LEFT OUTER JOIN org.section sec ON sec.Id=PR.SectionId
                                LEFT OUTER JOIN org.SubSection AS ss ON ss.Id=PR.SubSectionId

                                LEFT OUTER JOIN (select K.EmpSystemID,COUNT(*)AbsentDays,MIN(k.WorkDate) AS FirstAbsentDate
                                  from (SELECT *,RANK() OVER(PARTITION BY EmpSystemID,dayStatustemp ORDER BY EmpSystemID,seq) AS SQ FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,CASE WHEN daystatus IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) THEN 'A' ELSE daystatus END AS dayStatustemp,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM AttdnProcessData AS P 
		                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=p.EmpSystemID
                                        where EI.PlantId='" + identity.PlantId + @"'  AND p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) AND ei.EmployeeStatus='Active'
                                ) AS K WHERE K.dayStatustemp='A') AS K 
                                WHERE K.SEQ=K.SQ
                                GROUP BY K.EmpSystemID
                                HAVING COUNT(*)>=" + days + @") AS AB ON ab.EmpSystemID=E.SystemId


                                WHERE  e.EmployeeStatus='Active' AND isnull(e.EmployeeCurrentStatus,'')='' AND D.SEQ<=" + days + @" AND D.DayStatus='A'  AND E.PlantId='" + identity.PlantId + @"'
                                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,E.EmployeeName,D.DayStatus,
                                DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath,ab.FirstAbsentDate
                                HAVING COUNT(d.DayStatus)>=" + days + @" ORDER BY AB.AbsentDays DESC";
            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

        [HttpPost]
        public ActionResult GetAbsenteeismList(string plantid)
        {
            try
            {
                int days;
                string sql = LongAbsenteeismQuery(out days);
                var _data = _sqlRepository.GetDataCollection(sql);

                return Json(new { DATA = _data, Policy = "(Minimum Absenteeism : " + days.ToString() + " Days Consecutively)", Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private int AbsenteeismDays()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                string SQL = "select * from [dbo].[PlantWiseHRMSSetting] where plantid='" + identity.PlantId + "'";
                DataTable dt = _sqlRepository.GetDataTable(SQL);
                if (dt.Rows.Count > 0)
                    return (int)Convert.ToDouble(bplib.clsWebLib.GetNumData(dt.Rows[0]["LongTermAbesnteeism"].ToString()));
            }
            catch (Exception ex)
            {

                return 0;
            }

            return 0;
        }
        

        [HttpPost, Authorize]
        public ActionResult GetAbsenteeismAssignedList(string plantid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                int days = AbsenteeismDays();

                //string sql = @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,e.EmpPicPath,
                //            DEP.UserName AS Department,de.UserName AS designation,
                //            sec.UserName AS Section,ss.UserName AS SubSection,
                //            D.DayStatus,COUNT(d.DayStatus) AS AbsentCount 
                //            FROM (
                //              SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,
                //              dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
                //              FROM AttdnProcessData AS P
                //              WHERE p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) 
                //             ) AS D
                //            INNER JOIN EmployeeInformation AS E ON e.SystemId=d.EmpSystemID 
                //            LEFT OUTER JOIN org.Department AS DEP ON dep.Id=e.DepartmentId
                //            LEFT OUTER JOIN hkp.Designation AS DE ON de.Id=e.DesignationSystemID
                //            LEFT OUTER JOIN org.section sec ON sec.Id=e.SectionId
                //            LEFT OUTER JOIN org.SubSection AS ss ON ss.Id=e.SubSectionId
                //            WHERE  e.EmployeeStatus='LONG ABSENTEEISM' AND D.SEQ<=" + days.ToString() + @" AND D.DayStatus='A'  AND E.PlantId='" + identity.PlantId + @"'
                //            GROUP BY e.SystemId, e.EmployeeCode,E.EmployeeName,D.DayStatus,
                //            DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath
                //            HAVING COUNT(d.DayStatus)>=" + days.ToString();
                //string sql = @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,e.EmpPicPath,
                //                DEP.UserName AS Department,de.UserName AS designation,
                //                sec.UserName AS Section,ss.UserName AS SubSection,case when isnull(ab.AbsentDays,0)=0 then 'Can be revoked' else '' end as Status, 
                //                D.DayStatus,COUNT(d.DayStatus) AS AbsentCount,isnull(ab.AbsentDays,0) AS AbsentDays,AB.FirstAbsentDate
                //                FROM (
                //                  SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,
                //                  dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
                //                  FROM AttdnProcessData AS P
                //                  WHERE p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) 
                //                 ) AS D
                //                INNER JOIN EmployeeInformation AS E ON e.SystemId=d.EmpSystemID 
                //                LEFT OUTER JOIN org.Department AS DEP ON dep.Id=e.DepartmentId
                //                --LEFT OUTER JOIN hkp.Designation AS DE ON de.Id=e.DesignationSystemID
                //                LEFT OUTER JOIN hkp.LegalDesignation AS DE ON de.Id=e.LegalDesignationId
                //                LEFT OUTER JOIN org.section sec ON sec.Id=e.SectionId
                //                LEFT OUTER JOIN org.SubSection AS ss ON ss.Id=e.SubSectionId

                //                LEFT OUTER JOIN (select K.EmpSystemID,COUNT(*)AbsentDays,MIN(k.WorkDate) AS FirstAbsentDate
                //                  from (SELECT *,RANK() OVER(PARTITION BY EmpSystemID,dayStatustemp ORDER BY EmpSystemID,seq) AS SQ FROM (
                //                  SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,CASE WHEN daystatus IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) THEN 'A' ELSE daystatus END AS dayStatustemp,
                //                  dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
                //                  FROM AttdnProcessData AS P 
                //                  INNER JOIN EmployeeInformation AS ei ON ei.SystemId=p.EmpSystemID
                //                        where p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) AND ei.EmployeeStatus='Active' AND ei.EmployeeCurrentStatus='LONG ABSENTEEISM'
                //                ) AS K WHERE K.dayStatustemp='A') AS K -- AND K.SEQ<30
                //                WHERE K.SEQ=K.SQ
                //                GROUP BY K.EmpSystemID
                //                HAVING COUNT(*)>=" + days + @") AS AB ON ab.EmpSystemID=E.SystemId


                //                WHERE  e.EmployeeStatus='Active' AND e.EmployeeCurrentStatus='LONG ABSENTEEISM'  AND D.SEQ<=" + days + @" AND D.DayStatus='A' AND E.PlantId='" + identity.PlantId + @"'
                //                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,E.EmployeeName,D.DayStatus,AB.FirstAbsentDate,
                //                DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath
                //                --HAVING COUNT(d.DayStatus)>=" + days + @" 
                //                ORDER BY AB.AbsentDays DESC";

                string sql = @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,e.EmpPicPath
                               , e.SystemId AS EmpSystemId, DEP.UserName AS Department,de.UserName AS designation,
                                sec.UserName AS Section,ss.UserName AS SubSection,case when isnull(ab.AbsentDays,0)=0 then 'Can be revoked' else '' end as Status, 
                               isnull(ab.AbsentDays,0) AS AbsentDays,Format(ab.FirstAbsentDate,'dd-MMM-yyyy') FirstAbsentDate
                                FROM EmployeeInformation AS E 
LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT OUTER JOIN org.Department AS DEP ON dep.Id=PR.DepartmentId
                                LEFT OUTER JOIN hkp.LegalDesignation AS DE ON de.Id=e.LegalDesignationId
                                LEFT OUTER JOIN org.section sec ON sec.Id=PR.SectionId
                                LEFT OUTER JOIN org.SubSection AS ss ON ss.Id=PR.SubSectionId

                                LEFT OUTER JOIN (select K.EmpSystemID,COUNT(*)AbsentDays,MIN(k.WorkDate) AS FirstAbsentDate
                                  from (SELECT *,RANK() OVER(PARTITION BY EmpSystemID,dayStatustemp ORDER BY EmpSystemID,seq) AS SQ FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,CASE WHEN daystatus IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) THEN 'A' ELSE daystatus END AS dayStatustemp,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM AttdnProcessData AS P 
		                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=p.EmpSystemID
                                        where p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) AND ei.EmployeeCurrentStatus='LONG ABSENTEEISM'
                                ) AS K WHERE K.dayStatustemp='A') AS K 
                                WHERE K.SEQ=K.SQ
                                GROUP BY K.EmpSystemID
                                HAVING COUNT(*)>=" + days + @") AS AB ON ab.EmpSystemID=E.SystemId


                                WHERE e.EmployeeStatus='Active' AND e.EmployeeCurrentStatus='LONG ABSENTEEISM'   AND E.PlantId='" + identity.PlantId + @"'
                                   and isnull(e.EmployeeDisciplinaryActionIdForLA,'')= ''
                                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,E.EmployeeName,ab.FirstAbsentDate,
                                DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath
                                ORDER BY AB.AbsentDays DESC
                                ";


                var _data = _sqlRepository.GetDataCollection(sql);

                return Json(new { DATA = _data, Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost]
        public ActionResult UpdateEmployeeStatus(string[] empids, string flag)
        {
            try
            {

                if (empids == null || empids.Length == 0)
                    throw new Exception("No data found!!!");
                string ids = "''";
                foreach (string item in empids)
                {
                    ids += ",'" + item + "'";
                }

                DataSet dsRef;
                string strSql = @"select * from employeeinformation where systemid in (" + ids + ")";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");



                int days;
                string sql = LongAbsenteeismQuery(out days);
                DataTable dt = _sqlRepository.GetDataTable(sql);

                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    dt.DefaultView.RowFilter = "Id='" + dsRef.Tables[0].Rows[i]["SystemId"].ToString() + "'";

                    dsRef.Tables[0].Rows[i].BeginEdit();
                    dsRef.Tables[0].Rows[i]["EmployeeCurrentStatus"] = flag;
                    dsRef.Tables[0].Rows[i]["EmployeeCurrentStatusEffectiveDate"] = System.DateTime.Now.ToString("dd-MMM-yyyy");

                    if (dt.DefaultView.Count > 0)
                        dsRef.Tables[0].Rows[i]["EmployeeCurrentStatusEffectiveDate"] = Convert.ToDateTime(dt.DefaultView[0]["FirstAbsentDate"].ToString()).AddDays(-1).ToString("dd-MMM-yyyy");

                    if (flag.ToUpper() == "ACTIVE")
                    {
                        dsRef.Tables[0].Rows[i]["EmployeeCurrentStatusEffectiveDate"] = DBNull.Value;
                        dsRef.Tables[0].Rows[i]["EmployeeCurrentStatus"] = DBNull.Value;
                    }
                    dsRef.Tables[0].Rows[i].EndEdit();
                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsRef);

                return Json(new { Message = "Data updated successfully", Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true, }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult ViewEmployeeStatus(string empid, string firstabsentdate)
        {
            try
            {

                if (string.IsNullOrEmpty(empid))
                    throw new Exception("No data found!!!");

                string sql = "";
                string fromDate = System.DateTime.Now.AddDays(-30).ToString("dd-MMM-yyyy");
                if (string.IsNullOrEmpty(firstabsentdate) == false)
                {
                    fromDate = Convert.ToDateTime(firstabsentdate).AddDays(-1).ToString("dd-MMM-yyyy");
                }

                sql = @"SELECT FORMAT(apd.WorkDate,'dd-MMM-yyyy') AS WorkDate,FORMAT( apd.InTime,'dd-MMM-yyyy hh:mm:ss tt') AS InTime,FORMAT( apd.OutTime,'dd-MMM-yyyy hh:mm:ss tt') AS OutTime,apd.DayStatus,apd.IsManualInTime,apd.IsManualOutTime,apd.IsOTEntitled,
                                   dt.Category,sd.UserName AS ShiftDesc
                              FROM AttdnProcessData AS apd 
                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID
                            LEFT OUTER JOIN DayType AS dt ON dt.DayType=apd.DayStatus
                            WHERE apd.EmpSystemID='" + empid + @"' AND apd.WorkDate>='" + fromDate + @"'
                            ORDER BY apd.WorkDate DESC";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true, }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpGet, Authorize]
        public ActionResult GetLABS(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select Id,EmpSystemId,DisciplinaryActionCategoryId,Description,format(EntryDate,'dd-MMM-yyyy')as EntryDate,ActionType
								from [HKP].[EmployeeDisciplinaryAction] where EmpSystemId='" + Id+@"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetActionCategory()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,UserName FROM [HKP].[DisciplinaryActionCategory]";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetAllDescription(string DisciplinaryActionCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  select DAS.Id,DAS.DisciplinaryActionCategoryId,DAS.Sequence
							,DAS.LetterIssueDay,DAS.IsSeparable,DAS.Description as UserName
							,Separable=case when DAS.IsSeparable=1 then 'Yes' else 'No' END 
                            ,Count=(select Count(Id) from   DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"')
                            ,NextLetterDueDate=(select LetterIssueDay from   DisciplinaryActionSettingDetails  where DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"' and Sequence=das.Sequence+1)
                            From  DisciplinaryActionSettingDetails  DAS
							where das.DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId+ @"' and das.IsActive = 1 order by Sequence asc";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLetterFormet(string LetterFormetId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select Id,LetterFormat,LetterName,IsDefault From DisciplinaryActionSettingChild where DisciplinaryActionSettingDetailsId='" + LetterFormetId+ @"' and IsActive=1 ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public void ExecuteRawSQL(string sql1)
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
                objCon.ExecuteNonQueryWrapper(sql1, true, "1");
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
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End Function

        [HttpPost]
        public ActionResult Save(LABS longAbsenteeism, DisciplinaryActionDetails disciplinaryActionDetails)
        {
            try
            {               
                string LBSID = string.Empty;
                string DADID = string.Empty;
                LBSID = SaveLBS(longAbsenteeism);
                SaveLetterInformation(disciplinaryActionDetails, LBSID,out DADID);
                string sql = @"update EmployeeInformation SET EmployeeCurrentStatus='TBS',EmployeeCurrentStatusEffectiveDate='"+ System.DateTime.Now.ToString("dd-MMM-yyyy")+@"' ,EmployeeDisciplinaryActionIdForLA = '" + LBSID+@"' where SystemId='" + longAbsenteeism.EmpSystemId + @"'";
                ExecuteRawSQL(sql);
                return Json(new { DADID, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string SaveLBS(LABS longAbsenteeism)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string LBSID = string.Empty;

            try
            {
                string sql = "SELECT * FROM [HKP].[EmployeeDisciplinaryAction] WHERE Id='" + longAbsenteeism.Id + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                string Id = string.Empty;

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[HKP].[EmployeeDisciplinaryAction]", out sID);
                    Id = "LA" + sID;
                    dr["Id"]=Id ;
                    dr["EmpSystemId"] = longAbsenteeism.EmpSystemId;
                    dr["DisciplinaryActionCategoryId"] = longAbsenteeism.DisciplinaryActionCategoryId;
                    dr["Description"] = longAbsenteeism.Description;
                    dr["EntryDate"] = longAbsenteeism.EntryDate;
                    dr["ActionType"] = "LA";

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
                    dr["EmpSystemId"] = longAbsenteeism.EmpSystemId;
                    dr["DisciplinaryActionCategoryId"] = longAbsenteeism.DisciplinaryActionCategoryId;
                    dr["Description"] = longAbsenteeism.Description;
                    dr["EntryDate"] = longAbsenteeism.EntryDate;
                    dr["ActionType"] = "LA";

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

        public void SaveLetterInformation(DisciplinaryActionDetails disciplinaryActionDetails,string LBSID,out string DADID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DADID = string.Empty;
            try
            {
                string sql = "SELECT * FROM EmployeeDisciplinaryActionDetails WHERE Id='" + disciplinaryActionDetails.DADID + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeDisciplinaryActionDetails", out sID);
                    dr["Id"] = "DAD" + sID;
                    DADID = dr["Id"].ToString();
                    dr["EmployeeDisciplinaryActionId"] = LBSID;
                    dr["LetterFormetId"] = disciplinaryActionDetails.LettersFormat;

                    dr["NextLetterDueDate"] = disciplinaryActionDetails.NextLetterDueDate;
                    dr["LetterIssueDate"] = disciplinaryActionDetails.LetterIssueDate;
                    dr["DisciplinaryActionSettingDetailsId"] = disciplinaryActionDetails.DisciplinaryActionSettingDetailsId;
                    dr["DisciplinaryActionCategoryId"] = disciplinaryActionDetails.DisciplinaryActionCategoryId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    DADID = dr["ID"].ToString();

                    dr["EmployeeDisciplinaryActionId"] = LBSID;
                    dr["LetterFormetId"] = disciplinaryActionDetails.LettersFormat;

                    dr["NextLetterDueDate"] = disciplinaryActionDetails.NextLetterDueDate;
                    dr["LetterIssueDate"] = disciplinaryActionDetails.LetterIssueDate;
                    dr["DisciplinaryActionSettingDetailsId"] = disciplinaryActionDetails.DisciplinaryActionSettingDetailsId;
                    dr["DisciplinaryActionCategoryId"] = disciplinaryActionDetails.DisciplinaryActionCategoryId;

                  
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
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
     
        #endregion -- Operations

        public class LABS : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string EmpSystemId { get; set; }
            public string DisciplinaryActionCategoryId { get; set; }
            public string Description { get; set; }
            public DateTime? EntryDate { get; set; }
            public string ActionType { get; set; }
          
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

        public class DisciplinaryActionDetails : BaseModel
        {
            #region Scalar Properties            
            public string DADID { get; set; }
            public string EmployeeDisciplinaryActionId { get; set; }
            public string LetterFormetId { get; set; }     
            public string LettersFormat { get; set; }


          
            public string NextLetterDueDate { get; set; }
            public string LetterIssueDate { get; set; }
            public string DisciplinaryActionSettingDetailsId { get; set; }
            public string DisciplinaryActionCategoryId { get; set; }


            #endregion Scalar Properties
        }

    }
}
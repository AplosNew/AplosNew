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
using System.Linq;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class TBSAssignController : BaseController
    {


        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public TBSAssignController(IUnitOfWork U, ISqlRepository R)
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
                                D.DayStatus,COUNT(d.DayStatus) AS AbsentCount,ab.AbsentDays,ab.FirstAbsentDate
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


                                WHERE  e.EmployeeStatus='Active' AND isnull(e.EmployeeCurrentStatus,'')<>'TBS' AND D.SEQ<=" + days + @" AND D.DayStatus='A'  AND E.PlantId='" + identity.PlantId + @"'
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
                    return (int)Convert.ToDouble(bplib.clsWebLib.GetNumData(dt.Rows[0]["tbsDays"].ToString()));
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
                //            WHERE  e.EmployeeStatus='TBS' AND D.SEQ<=" + days.ToString() + @" AND D.DayStatus='A'  AND E.PlantId='" + identity.PlantId + @"'
                //            GROUP BY e.SystemId, e.EmployeeCode,E.EmployeeName,D.DayStatus,
                //            DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath
                //            HAVING COUNT(d.DayStatus)>=" + days.ToString();
                //string sql = @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,e.EmpPicPath,
                //                DEP.UserName AS Department,de.UserName AS designation,
                //                sec.UserName AS Section,ss.UserName AS SubSection,case when isnull(ab.AbsentDays,0)=0 then 'Can be revoked' else '' end as Status, 
                //                D.DayStatus,COUNT(d.DayStatus) AS AbsentCount,isnull(ab.AbsentDays,0) AS AbsentDays,ab.FirstAbsentDate
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
                //                        where p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) AND ei.EmployeeStatus='Active' AND ei.EmployeeCurrentStatus='TBS'
                //                ) AS K WHERE K.dayStatustemp='A') AS K -- AND K.SEQ<30
                //                WHERE K.SEQ=K.SQ
                //                GROUP BY K.EmpSystemID
                //                HAVING COUNT(*)>=" + days + @") AS AB ON ab.EmpSystemID=E.SystemId


                //                WHERE  e.EmployeeStatus='Active' AND e.EmployeeCurrentStatus='TBS' AND D.SEQ<=" + days + @" AND D.DayStatus='A' AND E.PlantId='" + identity.PlantId + @"'
                //                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,E.EmployeeName,D.DayStatus,ab.FirstAbsentDate,
                //                DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath
                //                --HAVING COUNT(d.DayStatus)>=" + days + @" 
                //                ORDER BY AB.AbsentDays DESC";

                string Xsql = @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,e.EmpPicPath,
                                DEP.UserName AS Department,de.UserName AS designation,
                                sec.UserName AS Section,ss.UserName AS SubSection,case when isnull(ab.AbsentDays,0)=0 then 'Can be revoked' else '' end as Status, 
                                FROM EmployeeInformation AS E 
LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT OUTER JOIN org.Department AS DEP ON dep.Id=PR.DepartmentId
                                --LEFT OUTER JOIN hkp.Designation AS DE ON de.Id=e.DesignationSystemID
                                LEFT OUTER JOIN hkp.LegalDesignation AS DE ON de.Id=e.LegalDesignationId
                                LEFT OUTER JOIN org.section sec ON sec.Id=PR.SectionId
                                LEFT OUTER JOIN org.SubSection AS ss ON ss.Id=PR.SubSectionId

                                LEFT OUTER JOIN (select K.EmpSystemID,COUNT(*)AbsentDays,MIN(k.WorkDate) AS FirstAbsentDate
                                  from (SELECT *,RANK() OVER(PARTITION BY EmpSystemID,dayStatustemp ORDER BY EmpSystemID,seq) AS SQ FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,CASE WHEN daystatus IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) THEN 'A' ELSE daystatus END AS dayStatustemp,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM AttdnProcessData AS P 
		                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=p.EmpSystemID
                                        where p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) AND ei.EmployeeCurrentStatus='TBS'
                                ) AS K WHERE K.dayStatustemp='A') AS K 
                                WHERE K.SEQ=K.SQ
                                GROUP BY K.EmpSystemID
                                HAVING COUNT(*)>=" + days + @") AS AB ON ab.EmpSystemID=E.SystemId


                                WHERE  e.EmployeeCurrentStatus='TBS'   AND E.PlantId='" + identity.PlantId + @"'
                                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,E.EmployeeName,ab.FirstAbsentDate,
                                DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath
                                ORDER BY AB.AbsentDays DESC
                                ";
                string sql = @"SELECT convert(bit,0) AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,e.EmpPicPath,
                                DEP.UserName AS Department,de.UserName AS designation,
                                sec.UserName AS Section,ss.UserName AS SubSection,case when isnull(ab.AbsentDays,0)=0 then 'Can be revoked' else '' end as Status, 
                               isnull(ab.AbsentDays,0) AS AbsentDays,ab.FirstAbsentDate,ab.EntryDate,ab.CaseNo,ab.DisciplinaryActionCategory,ab.Description,ab.Sequence,Isnull(ab.IsFromDA,0) IsFromDA, '' Remarks
                                FROM EmployeeInformation AS E 
LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                LEFT OUTER JOIN org.Department AS DEP ON dep.Id=PR.DepartmentId
                                --LEFT OUTER JOIN hkp.Designation AS DE ON de.Id=e.DesignationSystemID
                                LEFT OUTER JOIN hkp.LegalDesignation AS DE ON de.Id=e.LegalDesignationId
                                LEFT OUTER JOIN org.section sec ON sec.Id=PR.SectionId
                                LEFT OUTER JOIN org.SubSection AS ss ON ss.Id=PR.SubSectionId

                                LEFT OUTER JOIN (


                                --==============================================================================================

                                select K.EmpSystemID,COUNT(*)AbsentDays,MIN(k.WorkDate) AS FirstAbsentDate, '' EntryDate,'' CaseNo,'' DisciplinaryActionCategory,'' Description,'' Sequence, convert(bit,0) IsFromDA
                                  from (SELECT *,RANK() OVER(PARTITION BY EmpSystemID,dayStatustemp ORDER BY EmpSystemID,seq) AS SQ FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,CASE WHEN daystatus IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) THEN 'A' ELSE daystatus END AS dayStatustemp,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM AttdnProcessData AS P 
		                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=p.EmpSystemID
                                        where p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) AND ei.EmployeeCurrentStatus='TBS' and ei.EmployeeDisciplinaryActionIdForLA is  null
                                ) AS K WHERE K.dayStatustemp='A') AS K 
                                WHERE K.SEQ=K.SQ
                                GROUP BY K.EmpSystemID
                                HAVING COUNT(*)>=" + days + @"				
								
								--=============================================================================================
								Union
								--=============================================================================================
								SELECT DA.EmpSystemID,'' AbsentDays,'' FirstAbsentDate,FORMAT( MIN(DA.EntryDate),'dd-MMM-yyyy' ) AS EntryDate,DA.CaseNo,DA.UserName DisciplinaryActionCategory,DA.Description,DA.Sequence, convert(bit,1) IsFromDA
									FROM (SELECT * ,RANK() OVER(PARTITION BY EmpSystemID,EntryDate,Sequence ORDER BY EmpSystemID,EntryDate,Sequence desc) AS SQ 
									  FROM (  
										   SELECT DAC.UserName,EDA.EmpSystemId,EDA.EntryDate,EDA.DisciplinaryActionCategoryId,EDA.ActionType,EDA.Id CaseNo,DASD.Description,DASD.Sequence
										   ,dense_rank() OVER (PARTITION BY EDA.EmpSystemID ORDER BY EDA.EntryDate,DASD.Sequence DESC) AS SEQ
										   FROM hkp.EmployeeDisciplinaryAction EDA 
                                            INNER JOIN EmployeeInformation AS ei ON ei.SystemId=EDA.EmpSystemID
										  LEFT JOIN HKP.DisciplinaryActionCategory DAC ON DAC.Id =EDA.DisciplinaryActionCategoryId
										  left join EmployeeDisciplinaryActionDetails EDAD on EDAD.EmployeeDisciplinaryActionId=EDA.Id
										  LEFT JOIN DisciplinaryActionSettingDetails DASD on DASD.Id=EDAD.DisciplinaryActionSettingDetailsId
										   WHERE EDA.ActionType='LA' AND ei.EmployeeCurrentStatus='TBS'
									) AS DA WHERE DA.ActionType='LA') AS DA 
									WHERE DA.SEQ=DA.SQ
									GROUP BY DA.EmpSystemID,DA.CaseNo,DA.UserName,DA.Description,DA.Sequence
								--=============================================================================================
                                ) AS AB ON ab.EmpSystemID=E.SystemId


                                WHERE  e.EmployeeCurrentStatus='TBS'   AND E.PlantId='" + identity.PlantId + @"'
                                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,E.EmployeeName,ab.FirstAbsentDate,
                                DEP.UserName,de.UserName,sec.UserName,ss.UserName,e.EmpPicPath,ab.EntryDate,ab.CaseNo,ab.DisciplinaryActionCategory,ab.Description,ab.Sequence,ab.IsFromDA
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
        public ActionResult UpdateEmployeeStatus(List<TBSUnAssignModel> empids, string flag)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;

                if (empids == null || empids.Count == 0)
                    throw new Exception("No data found!!!");
                string ids = "''";
                foreach (TBSUnAssignModel item in empids)
                {
                    ids += ",'" + item.EmpSystemId + "'";
                }




                #region DA
                string DAIds = "''";


                foreach (TBSUnAssignModel item in empids.Where(x => x.IsFromDA = true))
                {
                    DAIds += ",'" + item.CaseNo + "'";
                }




                DataSet dsDA;
                string strSqlDA = @"select * from hkp.EmployeeDisciplinaryAction where id in (" + DAIds + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSqlDA, out dsDA, false, "1");

                foreach (TBSUnAssignModel item in empids.Where(x => x.IsFromDA = true))
                {
                    DataView dv = new DataView(dsDA.Tables[0]);
                    dv.RowFilter = "Id='" + item.CaseNo + "'";
                    if (dv.Count == 0)
                    {

                    }
                    else
                    {
                        DataRow dr = dsDA.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["DACompeletionRemark"] = item.Remarks;
                        dr["IsDACompleted"] = true;
                        dr["DACompeletedBy"] = identity.Name;
                        dr["DACompeletionDate"] = System.DateTime.Now.ToString();

                        dr.EndEdit();
                    }
                    dv.RowFilter = null;
                }



                #endregion






                DataSet dsRef;
                string strSql = @"select * from employeeinformation where systemid in (" + ids + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
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

                        dsRef.Tables[0].Rows[i]["EmployeeDisciplinaryActionIdForLA"] = DBNull.Value;
                        dsRef.Tables[0].Rows[i]["EmployeeCurrentStatusEffectiveDate"] = DBNull.Value;
                        dsRef.Tables[0].Rows[i]["EmployeeCurrentStatus"] = DBNull.Value;
                    }
                    dsRef.Tables[0].Rows[i].EndEdit();
                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsRef, dsDA);


                return Json(new { Message = "Data updated successfully", Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true, }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost, Authorize]
        public ActionResult ViewEmployeeStatus_backup(string empid)
        {
            try
            {

                if (string.IsNullOrEmpty(empid))
                    throw new Exception("No data found!!!");

                int days;
                string sql = LongAbsenteeismQuery(out days);
                DataTable dt = _sqlRepository.GetDataTable(sql);

                dt.DefaultView.RowFilter = "Id='" + empid + "'";

                if (dt.DefaultView.Count > 0)
                {

                    if (clsStaticInfo.dbl(dt.DefaultView[0]["AbsentDays"].ToString()) > 0)
                    {
                        string fromDate = Convert.ToDateTime(dt.DefaultView[0]["FirstAbsentDate"].ToString()).AddDays(-1).ToString("dd-MMM-yyyy");
                        sql = @"SELECT FORMAT(apd.WorkDate,'dd-MMM-yyyy') AS WorkDate,FORMAT( apd.InTime,'dd-MMM-yyyy hh:mm:ss tt') AS InTime,FORMAT( apd.OutTime,'dd-MMM-yyyy hh:mm:ss tt') AS OutTime,apd.DayStatus,apd.IsManualInTime,apd.IsManualOutTime,apd.IsOTEntitled,
                                   dt.Category,sd.UserName AS ShiftDesc
                              FROM AttdnProcessData AS apd 
                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID
                            LEFT OUTER JOIN DayType AS dt ON dt.DayType=apd.DayStatus
                            WHERE apd.EmpSystemID='" + empid + @"' AND apd.WorkDate>='" + fromDate + @"'
                            ORDER BY apd.WorkDate DESC";

                        return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        string fromDate = System.DateTime.Now.AddDays(-30).ToString("dd-MMM-yyyy");

                        sql = @"SELECT FORMAT(apd.WorkDate,'dd-MMM-yyyy') AS WorkDate,FORMAT( apd.InTime,'dd-MMM-yyyy hh:mm:ss tt') AS InTime,FORMAT( apd.OutTime,'dd-MMM-yyyy hh:mm:ss tt') AS OutTime,apd.DayStatus,apd.IsManualInTime,apd.IsManualOutTime,apd.IsOTEntitled,
                                   dt.Category,sd.UserName AS ShiftDesc
                              FROM AttdnProcessData AS apd 
                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID
                            LEFT OUTER JOIN DayType AS dt ON dt.DayType=apd.DayStatus
                            WHERE apd.EmpSystemID='" + empid + @"' AND apd.WorkDate>='" + fromDate + @"'
                            ORDER BY apd.WorkDate DESC";

                        return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                    }




                }
                else
                {
                    string fromDate = System.DateTime.Now.AddDays(-30).ToString("dd-MMM-yyyy");

                    sql = @"SELECT FORMAT(apd.WorkDate,'dd-MMM-yyyy') AS WorkDate,FORMAT( apd.InTime,'dd-MMM-yyyy hh:mm:ss tt') AS InTime,FORMAT( apd.OutTime,'dd-MMM-yyyy hh:mm:ss tt') AS OutTime,apd.DayStatus,apd.IsManualInTime,apd.IsManualOutTime,apd.IsOTEntitled,
                                   dt.Category,sd.UserName AS ShiftDesc
                              FROM AttdnProcessData AS apd 
                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID
                            LEFT OUTER JOIN DayType AS dt ON dt.DayType=apd.DayStatus
                            WHERE apd.EmpSystemID='" + empid + @"' AND apd.WorkDate>='" + fromDate + @"'
                            ORDER BY apd.WorkDate DESC";

                    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


                }

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

        #endregion -- Operations
    }
    public class TBSUnAssignModel
    {
        public string EmpSystemId { get; set; }
        public bool IsFromDA { get; set; } = false;
        public string CaseNo { get; set; }
        public string Remarks { get; set; }
    }
}
using Library.Service.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance.Compliance
{
    public class AttendanceProcessFinalAplos
    {
        string sEmpSystemIDColl = string.Empty;
        string lblAttdnProcBase = string.Empty;

        private void GetDayType(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.DayType";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        private void GetUpdatedCompliedEmpShiftAssignBeforeFromDate(string sEmpSystemIDColl, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT A.* FROM dbo.CompliedShiftAssignment A
                            INNER JOIN (
                                         SELECT EmpSystemID, MAX(WorkDate) WorkDate FROM dbo.CompliedShiftAssignment
                                            WHERE WorkDate <= '" + sAttnDate + @"' AND EmpSystemID IN (" + sEmpSystemIDColl + @")
                                            GROUP BY EmpSystemID
                                        ) B ON A.EmpSystemID = B.EmpSystemID AND A.WorkDate = B.WorkDate";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetCompliedShiftDateWiseWithDateRange(string sEmpSystemIDColl, string dtLastDt, string sDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQL = @"SELECT *
                            FROM dbo.CompliedShiftDateWise 
                             WHERE EmpSystemID IN (" + sEmpSystemIDColl + @") 
                                   AND WorkDate BETWEEN '" + dtLastDt + @"' AND '" + sDate + @"'";

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

        private void GetEmployeeCompliedShiftAssignInDateRange(string sEmpSystemIDColl, string lstDate, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.CompliedShiftAssignment 
                                WHERE WorkDate between  '" + lstDate + @"' and '" + sAttnDate + @"'
                                            AND EmpSystemID IN (" + sEmpSystemIDColl + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void GetCompliedShiftDefination(string sPlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Id, CompanyGroupId, PlantId, Code, ShiftName, CONVERT(VARCHAR(8), InTime) InTime, 
	                              CONVERT(VARCHAR(8), OutTime) OutTime, IsNight 
                            FROM [HKP].[CompliedShift] 
                            WHERE PlantID = '" + sPlantID + @"'";

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
        public void ShiftProcess(string _plantid, string sAttnDate)
        {
            #region DataSet Declare

            DataSet dsAttdnProc = null;

            DataSet dsEmpDtWiseCmpSftAss = null;
            DataTable dtEmpDtWiseCmpSftAss = null;
            DataRow drEmpDtWiseCmpSftAss = null;
            DataView dvEmpDtWiseCmpSftAss = null;

            DataSet dsEmpCmpSftAssBfrFmDt = null;
            DataTable dtEmpCmpSftAssBfrFmDt = null;
            DataView dvEmpCmpSftAssBfrFmDt = null;

            DataSet dsDayType = null;
            DataTable dtDayType = null;
            DataView dvDayType = null;

            DataSet dsEmpCmpSftAss = null;
            DataTable dtEmpCmpSftAss = null;
            DataView dvEmpCmpSftAss = null;

            DataSet dsSftDft = null;

            string sEmpSysIDCollForSft = "";

            #endregion DataSet Declare

            try
            {
                GetAttdnProcessData(sAttnDate, _plantid, out dsAttdnProc);
                if (dsAttdnProc.Tables[0].Rows.Count > 0)
                {
                    for (int iEmpCnt = 0; iEmpCnt < dsAttdnProc.Tables[0].Rows.Count; iEmpCnt++)
                    {
                        if (sEmpSysIDCollForSft.Trim() == "")
                        {
                            sEmpSysIDCollForSft = "'" + dsAttdnProc.Tables[0].Rows[iEmpCnt]["EmpSystemID"].ToString().Trim() + "'";
                        }
                        else
                        {
                            sEmpSysIDCollForSft += ",'" + dsAttdnProc.Tables[0].Rows[iEmpCnt]["EmpSystemID"].ToString().Trim() + "'";
                        }
                    }
                }

                if (dsAttdnProc.Tables[0].Rows.Count > 0)
                {
                    #region DataSet

                    string dtLastDt = Convert.ToDateTime(sAttnDate).AddDays(-1).ToString("dd-MMM-yyyy");
                    GetDayType(out dsDayType);
                    dtDayType = dsDayType.Tables[0];
                    dvDayType = new DataView();

                    List<dicCmpShiftDft> dicCmpShiftDft = new List<global::dicCmpShiftDft>();
                    GetCompliedShiftDefination(_plantid, out dsSftDft);
                    if (dsSftDft.Tables[0].Rows.Count > 0)
                        dicCmpShiftDft = dsSftDft.Tables[0].ToList<dicCmpShiftDft>();

                    GetCompliedShiftDateWiseWithDateRange(sEmpSysIDCollForSft.Trim(), dtLastDt, sAttnDate.Trim(), out dsEmpDtWiseCmpSftAss);
                    dtEmpDtWiseCmpSftAss = dsEmpDtWiseCmpSftAss.Tables[0];
                    dvEmpDtWiseCmpSftAss = new DataView();

                    GetUpdatedCompliedEmpShiftAssignBeforeFromDate(sEmpSysIDCollForSft.Trim(), sAttnDate.Trim(), out dsEmpCmpSftAssBfrFmDt);
                    dtEmpCmpSftAssBfrFmDt = dsEmpCmpSftAssBfrFmDt.Tables[0];
                    dvEmpCmpSftAssBfrFmDt = new DataView();

                    GetEmployeeCompliedShiftAssignInDateRange(sEmpSysIDCollForSft.Trim(), dtLastDt, sAttnDate.Trim(), out dsEmpCmpSftAss);
                    dtEmpCmpSftAss = dsEmpCmpSftAss.Tables[0];
                    dvEmpCmpSftAss = new DataView();

                    #endregion DataSet

                    for (int i = 0; i < dsAttdnProc.Tables[0].Rows.Count; i++)
                    {
                        #region Declare Variable

                        string sEmpSystemID = dsAttdnProc.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                        if (sEmpSystemID == "1800005")
                        {

                        }
                        string sPlantID = _plantid;

                        string sEmpSftAssCurntSysID = "";
                        string sEmpSftAssTempSysID = "";

                        string sSfTime = "00:00:00";

                        DateTime dtStDt = Convert.ToDateTime(sAttnDate);
                        DateTime dtFrmD = Convert.ToDateTime(sAttnDate);
                        DateTime dtToD = Convert.ToDateTime(sAttnDate);

                        #endregion Declare Variable

                        while (dtStDt <= dtToD)
                        {//check in the table 'EmpDateWiseShiftAssign', EmpSystemID and WorkDate are already available
                            #region Initialize

                            string strStDt = dtStDt.ToString("dd-MMM-yyyy");
                            sSfTime = "";

                            #endregion Initialize

                            dvEmpDtWiseCmpSftAss.Table = dtEmpDtWiseCmpSftAss;
                            dvEmpDtWiseCmpSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate = '" + strStDt + "'";
                            if (dvEmpDtWiseCmpSftAss.Count > 0)
                            {
                                #region EmpSystemID and WorkDate are already available in the table 'EmpDateWiseShiftAssign'

                                dvEmpCmpSftAss.Table = dtEmpCmpSftAss;
                                dvEmpCmpSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate <= '" + strStDt + "'";

                                if (dtStDt == dtFrmD || dvEmpCmpSftAss.Count == 0)
                                {

                                    #region  FromDate & Shift start Date Same and After fromdate to todate not found shift assignment

                                    #region Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                    dvEmpCmpSftAssBfrFmDt.Table = dtEmpCmpSftAssBfrFmDt;
                                    dvEmpCmpSftAssBfrFmDt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate <= '" + strStDt + "'";
                                    if (dvEmpCmpSftAssBfrFmDt.Count > 0)
                                    {
                                        var dicShiftDft_Sub = dicCmpShiftDft.Find(x => x.Id == dvEmpCmpSftAssBfrFmDt[0]["CompliedShiftId"].ToString().Trim());
                                        if (dicShiftDft_Sub != null)
                                        {
                                            sSfTime = strStDt + " " + dicShiftDft_Sub.InTime/*((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss")*/;
                                        }
                                        #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                        if (sSfTime.Trim().Length > 0)
                                        {
                                            drEmpDtWiseCmpSftAss = dvEmpDtWiseCmpSftAss[0].Row;
                                            drEmpDtWiseCmpSftAss.BeginEdit();

                                            drEmpDtWiseCmpSftAss["EmpSystemId"] = sEmpSystemID.Trim();
                                            drEmpDtWiseCmpSftAss["WorkDate"] = strStDt.Trim();
                                            drEmpDtWiseCmpSftAss["CompliedShiftId"] = dvEmpCmpSftAssBfrFmDt[0]["CompliedShiftId"].ToString().Trim();

                                            drEmpDtWiseCmpSftAss["PlantId"] = sPlantID.Trim();

                                            drEmpDtWiseCmpSftAss["UpdatedBy"] = "Schedule";
                                            drEmpDtWiseCmpSftAss["UpdatedDate"] = DateTime.Now;
                                            drEmpDtWiseCmpSftAss["UpdatedFromIP"] = "";

                                            drEmpDtWiseCmpSftAss.EndEdit();
                                        }
                                        #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'

                                    }
                                    #endregion Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                    #endregion  FromDate & Shift start Date Same and After fromdate to todate not found shift assignment
                                    sEmpSftAssTempSysID = sEmpSftAssCurntSysID;
                                }
                                else if (dvEmpCmpSftAss.Count > 0)
                                {
                                    string strActuEffDt = "";
                                    string strActuEffDtTmp = "";

                                    if (dvEmpCmpSftAss.Count > 1)
                                    {
                                        for (int efDt = 0; efDt < dvEmpCmpSftAss.Count; efDt++)
                                        {
                                            if (Convert.ToDateTime(dvEmpCmpSftAss[efDt]["WorkDate"].ToString().Trim()) <= Convert.ToDateTime(strStDt))
                                            {
                                                strActuEffDtTmp = dvEmpCmpSftAss[efDt]["WorkDate"].ToString().Trim();
                                            }
                                            if (strActuEffDt == "")
                                            { strActuEffDt = strActuEffDtTmp; }

                                            if (Convert.ToDateTime(strActuEffDtTmp) > Convert.ToDateTime(strActuEffDt))
                                            {
                                                strActuEffDt = strActuEffDtTmp;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        strActuEffDt = dvEmpCmpSftAss[0]["WorkDate"].ToString().Trim();
                                    }
                                }

                                #endregion EmpSystemID and WorkDate are already available in the table 'EmpDateWiseShiftAssign'
                            }
                            else
                            {
                                #region EmpSystemID and WorkDate not found in the table 'EmpDateWiseShiftAssign'

                                dvEmpCmpSftAss.Table = dtEmpCmpSftAss;
                                dvEmpCmpSftAss.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate <= '" + strStDt + "'";
                                if (dtStDt == dtFrmD || dvEmpCmpSftAss.Count == 0)
                                {
                                    dtLastDt = dtStDt.AddDays(-1).ToString("dd-MMM-yyyy");

                                    #region FromDate & Shift start Date Same and After fromdate to todate not found shift assignment

                                    #region Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                    dvEmpCmpSftAssBfrFmDt.Table = dtEmpCmpSftAssBfrFmDt;
                                    dvEmpCmpSftAssBfrFmDt.RowFilter = "EmpSystemID = '" + sEmpSystemID.Trim() + "' AND WorkDate <= '" + strStDt + "'";
                                    if (dvEmpCmpSftAssBfrFmDt.Count > 0)
                                    {
                                        var dicShiftDft_Sub = dicCmpShiftDft.Find(x => x.Id == dvEmpCmpSftAssBfrFmDt[0]["CompliedShiftId"].ToString().Trim());
                                        if (dicShiftDft_Sub != null)
                                        {
                                            sSfTime = strStDt + " " + dicShiftDft_Sub.InTime/*((DateTime)dicShiftDft_Sub.InTime).ToString("HH:mm:ss")*/;
                                        }
                                        #region If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                        if (sSfTime.Trim().Length > 0)
                                        {
                                            drEmpDtWiseCmpSftAss = dtEmpDtWiseCmpSftAss.NewRow();
                                            //									
                                            drEmpDtWiseCmpSftAss["EmpSystemId"] = sEmpSystemID.Trim();
                                            drEmpDtWiseCmpSftAss["WorkDate"] = strStDt.Trim();
                                            drEmpDtWiseCmpSftAss["CompliedShiftId"] = dvEmpCmpSftAssBfrFmDt[0]["CompliedShiftId"].ToString().Trim();

                                            drEmpDtWiseCmpSftAss["AddedBy"] = "Schedule";
                                            drEmpDtWiseCmpSftAss["AddedDate"] = DateTime.Now;
                                            drEmpDtWiseCmpSftAss["AddedFromIP"] = "";

                                            drEmpDtWiseCmpSftAss["PlantId"] = sPlantID.Trim();

                                            drEmpDtWiseCmpSftAss["UpdatedBy"] = "Schedule";
                                            drEmpDtWiseCmpSftAss["UpdatedDate"] = DateTime.Now;
                                            drEmpDtWiseCmpSftAss["UpdatedFromIP"] = "";

                                            dtEmpDtWiseCmpSftAss.Rows.Add(drEmpDtWiseCmpSftAss);
                                        }
                                        #endregion If Last updated shift in table 'EmployeeShiftAssign' is fix shift then just update the shiftSystemID in the table 'EmpDateWiseShiftAssign'
                                    }

                                    #endregion Check Last updated shift in table 'EmployeeShiftAssign' before fromdate

                                    #endregion FromDate & Shift start Date Same and After fromdate to todate not found shift assignment
                                }

                                #endregion EmpSystemID and WorkDate not found in the table 'EmpDateWiseShiftAssign'
                            }

                            dtStDt = dtStDt.AddDays(1);
                        }
                        //}
                    }
                    clsStaticInfo obs = new clsStaticInfo();
                    obs.SaveDataSets(dsEmpDtWiseCmpSftAss);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dsEmpCmpSftAssBfrFmDt = null;
            }
        }//End Function  

        public void GetAttdnProcessDataForFinalProcess(string sFromDate, string sToDate, string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                //190203 by monir
                //      strSql = @"SELECT A.EmpSystemID, A.WorkDate, A.GroupID, A.PlantID, A.ShiftSystemID, (A.WorkDate + CONVERT(VARCHAR(8), S.InTime, 108)) ShiftInTime,
                //                     (A.WorkDate + CONVERT(VARCHAR(8), S.OutTime, 108)) ShiftOutTime, A.InTime, A.OutTime,
                //                     (A.WorkDate + CONVERT(VARCHAR(8), CS.InTime, 108)) ComShiftInTime, 
                //                     (A.WorkDate + CONVERT(VARCHAR(8), CS.OutTime, 108)) ComShiftOutTime, CS.IsNight, ST.CompliedShiftId, 
                //                     DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.InTime, 108)), A.InTime) DifferentTimeMinute, A.DayStatus,
                //                     DATEADD(MINUTE, DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.InTime, 108)), A.InTime), (A.WorkDate + CONVERT(VARCHAR(8), CS.InTime, 108))) ComInTime, 
                //DATEADD(MINUTE, -A.OTHr, (DATEADD(MINUTE, DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.OutTime, 108)), A.OutTime), 
                //(A.WorkDate + CONVERT(VARCHAR(8), CS.OutTime, 108))))) ComOutTime
                //                  FROM [dbo].[AttdnProcessData] A
                //                    LEFT JOIN [dbo].[ShiftDefination] S ON A.ShiftSystemID = S.SystemID
                //                    INNER JOIN [dbo].[CompliedShiftDateWise] ST ON A.EmpSystemID = ST.EmpSystemId AND A.WorkDate = ST.WorkDate
                //                    LEFT JOIN [HKP].[CompliedShift] CS ON ST.CompliedShiftId = CS.Id
                //                  WHERE A.WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"' AND A.PlantID = '" + sPlantID + @"'
                //                  ORDER BY A.EmpSystemID, A.WorkDate";


                strSql = @"select * from
(--tt
select *
--,DATEADD(MINUTE,DifferentTimeMinuteOutRight,ComShiftOutTime) ComOutTime
,ComOutTime=case when DifferentTimeMinuteOutRight>=15 then DATEADD(MINUTE,ABS(CHECKSUM(NewId())) % 15,ComShiftOutTime)
else DATEADD(MINUTE,DifferentTimeMinuteOutRight,ComShiftOutTime) end
from 
                                (
                                SELECT A.EmpSystemID, A.WorkDate, A.GroupID, A.PlantID, A.ShiftSystemID, (A.WorkDate + CONVERT(VARCHAR(8), S.InTime, 108)) ShiftInTime,
	                                                              (A.WorkDate + CONVERT(VARCHAR(8), S.OutTime, 108)) ShiftOutTime, A.InTime, A.OutTime,
	                                                              (A.WorkDate + CONVERT(VARCHAR(8), CS.InTime, 108)) ComShiftInTime, 
	                                                              (A.WorkDate + CONVERT(VARCHAR(8), CS.OutTime, 108)) ComShiftOutTime, CS.IsNight, ST.CompliedShiftId, 
	                                                              DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.InTime, 108)), A.InTime) DifferentTimeMinute,
								                                  DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.OutTime, 108)), A.OutTime) DifferentTimeMinuteOut
								                                  ,DifferentTimeMinuteOutRight=case when DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.OutTime, 108)), A.OutTime)>59 then (DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.OutTime, 108)), A.OutTime)%60)
								                                  else DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.OutTime, 108)), A.OutTime) end 
								                                  , A.DayStatus,
	                                                              DATEADD(MINUTE, DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.InTime, 108)), A.InTime), (A.WorkDate + CONVERT(VARCHAR(8), CS.InTime, 108))) ComInTime, 
								                                  DATEADD(MINUTE, -A.OTHr, (DATEADD(MINUTE, DATEDIFF(MI, (A.WorkDate + CONVERT(VARCHAR(8), S.OutTime, 108)), A.OutTime), (A.WorkDate + CONVERT(VARCHAR(8), CS.OutTime, 108))))) xComOutTime
								 
								                                  ,A.OTHr
                                                            FROM [dbo].[AttdnProcessData] A
		                                                            LEFT JOIN [dbo].[ShiftDefination] S ON A.ShiftSystemID = S.SystemID
		                                                            INNER JOIN [dbo].[CompliedShiftDateWise] ST ON A.EmpSystemID = ST.EmpSystemId AND A.WorkDate = ST.WorkDate
		                                                            LEFT JOIN [HKP].[CompliedShift] CS ON ST.CompliedShiftId = CS.Id
                                                            WHERE
							                                 A.WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"' AND A.PlantID = '" + sPlantID + @"'
                            
							                                ) x


union

select
				st.EmpSystemID,	st.WorkDate,	c.CompliedShiftId GroupID,	c.PlantId PlantID,'' ShiftSystemID,	null ShiftInTime,	null ShiftOutTime
					,cs.InTime	,cs.OutTime	
					,(st.WorkDate + CONVERT(VARCHAR(8), CS.InTime, 108)) ComShiftInTime
					,(st.WorkDate + CONVERT(VARCHAR(8), CS.OutTime, 108)) ComShiftOutTime
					,	IsNight	,st.CompliedShiftId	,0 DifferentTimeMinute	,0 DifferentTimeMinuteOut	,0 DifferentTimeMinuteOutRight
					,'A'	DayStatus	,null ComInTime	,'' xComOutTime	,0 OTHr	,null ComOutTime


from CompliedShiftAssignment c 
inner join   [dbo].[CompliedShiftDateWise] ST on c.EmpSystemId=st.EmpSystemId and c.CompliedShiftId=st.CompliedShiftId
inner join   [dbo].EmpDateWiseShiftAssign ns on c.EmpSystemId=ns.EmpSystemId and ns.WorkDate=st.WorkDate
LEFT JOIN ShiftDefination sd ON ns.ShiftSystemID = sd.SystemID


LEFT JOIN [HKP].[CompliedShift] CS ON ST.CompliedShiftId = CS.Id
WHERE
	st.WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"' AND st.PlantID = '" + sPlantID + @"'
and
(--1
'" + DateTime.Now + @"'<CONVERT(datetime,('" + DateTime.Now.ToString("dd-MMM-yyyy") + @" ' + CONVERT(VARCHAR(8), sd.InTime, 108)))
and  '" + DateTime.Now + @"'>=CONVERT(datetime,('" + DateTime.Now.ToString("dd-MMM-yyyy") + @" ' + CONVERT(VARCHAR(8), cs.InTime, 108)))
)--1

)--tt
TT
where isnull(TT.ShiftInTime ,'')<>'' 

ORDER BY EmpSystemID, WorkDate";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetAttdnProcessFinalData(string sFromDate, string sToDate, string sPlantID, string sEmpSystemIDColl, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.AttdnProcessFinalData
                            WHERE WorkDate BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"' AND PlantID = '" + sPlantID + @"'
                            ";

                if (sEmpSystemIDColl.Trim() != "")
                {
                    strSql += @"
                                  AND EmpSystemID IN (" + sEmpSystemIDColl + @")
                                  ";
                }
                strSql += @"ORDER BY EmpSystemID, WorkDate";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetAttdnProcessData(string sWorkDate, string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                //strSql = @"SELECT *
                //            FROM [dbo].[AttdnProcessData]
                //            WHERE WorkDate = '" + sWorkDate + @"'";

                //by monir
                strSql = @"SELECT *
                            FROM [dbo].[EmpDateWiseShiftAssign]
                            WHERE WorkDate = '" + sWorkDate + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
    }
}

public class dicCmpShiftDft
{
    public string Id { get; set; } = string.Empty;
    public string CompanyGroupId { get; set; } = string.Empty;
    public string PlantId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
    //public DateTime? InTime { get; set; }
    //public DateTime? OutTime { get; set; }
    public string InTime { get; set; } = string.Empty;
    public string OutTime { get; set; } = string.Empty;
    public bool IsNight { get; set; } = false;
}

using AplosShiftProcess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBS;

namespace SetINOUT
{
    public class clsSetInOut
    {
        public void GetPlant(string CompanyGroupId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT CompanyGroupId, Id FROM ORG.Plant WHERE CompanyGroupId = '" + CompanyGroupId + @"' AND  Active = 1 AND Archive = 0";

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
        public void SetRawINOUT(string plantid, string GroupSysID, string workdate, string sEmpSystemIDColl)
        {
            DataSet dsEmpShift = null;
            DataSet dsRaw = null;
            try
            {
                #region ShiftProcess
                GetTypeLessRawData(plantid, workdate, out dsRaw);
                if (dsRaw.Tables[0].Rows.Count > 0)
                {
                    //ShiftProcess sp = new ShiftProcess();
                    //FixedShiftProcess sp2 = new FixedShiftProcess();
                    ////clsAttendance.AttendanceProcessAplos sp2 = new clsAttendance.AttendanceProcessAplos();
                    //sp2.ShiftProcess(plantid, workdate, GroupSysID, sEmpSystemIDColl);
                    //sp.ShiftProcessStart(plantid, workdate, GroupSysID, sEmpSystemIDColl);//_emplist

                    GenericShiftProcess sp = new GenericShiftProcess();
                    sp.Process(plantid, workdate, GroupSysID, sEmpSystemIDColl);
                    #endregion
                    GetEmpDateWise(plantid, workdate, out dsEmpShift);
                    //calculation
                    for (int i = 0; i < dsRaw.Tables[0].Rows.Count; i++)
                    {
                        string _rid = dsRaw.Tables[0].Rows[i]["Id"].ToString();
                        if (_rid == "TX238610")
                        {

                        }
                        DataView dvRaw = new DataView(dsRaw.Tables[0]);
                        dvRaw.RowFilter = "Id='" + _rid + "'";
                        if (dvRaw.Count > 0)
                        {
                            string _Type = GetINOUTType(workdate, dsEmpShift, dvRaw[0]);
                            if (string.IsNullOrEmpty(_Type) == false)
                            {
                                DataRow drRaw = dvRaw[0].Row;
                                drRaw.BeginEdit();
                                drRaw["PType"] = _Type;
                                drRaw.EndEdit();
                            }//type found                      
                        }//if
                    }//for
                    SaveDataSets(dsRaw);
                }//dsRaw.Tables[0].Rows.Count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function   
        public void SaveDataSets(params DataSet[] dsRef)
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

        }//End Function 
        public void GetHRSetting(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT PlantID,ShiftBasedPunchFlag FROM PlantWiseHRMSSetting WHERE PlantId = '" + PlantId + @"'  ";

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
        string GetINOUTType(string _workDate, DataSet dsEmpShift, DataRowView drRaw)
        {
            string res = string.Empty;
            try
            {
                string empid = drRaw["LogDownLoadNum"].ToString();
                string workdate = drRaw["pdate"].ToString();
                string worktime = drRaw["ptime"].ToString();
                if (empid == "205167")
                {

                }

                DataView dvEmpShift = new DataView(dsEmpShift.Tables[0]);
                dvEmpShift.RowFilter = "EmpSystemId='" + empid + "' and workdate='" + _workDate + "'";
                if (dvEmpShift.Count > 0)
                {
                    res = GetTypeINOUT(_workDate, workdate, worktime, dvEmpShift[0]);
                }//if

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetTypeINOUT(string workDate, string Punchworkdate, string worktime, DataRowView drEmpShift)
        {
            string res = string.Empty;
            try
            {
                string punchDT = Convert.ToDateTime(Punchworkdate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(worktime).ToString("HH:mm");
                DateTime _punchDT = Convert.ToDateTime(punchDT);

                string ShiftInTime = drEmpShift["InTime"].ToString();
                string ShiftOutTime = drEmpShift["OutTime"].ToString();
                int RawINDefinitionFrom = Convert.ToInt32(drEmpShift["RawINDefinitionFrom"].ToString());
                int RawINDefinitionTo = Convert.ToInt32(drEmpShift["RawINDefinitionTo"].ToString());
                int RawOUTDefinitionFrom = Convert.ToInt32(drEmpShift["RawOUTDefinitionFrom"].ToString());
                int RawOUTDefinitionTo = Convert.ToInt32(drEmpShift["RawOUTDefinitionTo"].ToString());

                string ShiftIn = Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(ShiftInTime).ToString("HH:mm");
                string ShiftOut = Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(ShiftOutTime).ToString("HH:mm");//TBD

                string _in = Convert.ToDateTime(ShiftInTime).ToString("HH:mm");
                string _out = Convert.ToDateTime(ShiftOutTime).ToString("HH:mm");//TBD

                if (Convert.ToDateTime(_out) < Convert.ToDateTime(_in))
                {
                    ShiftOut = Convert.ToDateTime(ShiftOut).AddDays(1).ToString("dd-MMM-yyyy HH:mm");
                }

                string IN_From = Convert.ToDateTime(ShiftIn).AddMinutes(-RawINDefinitionFrom).ToString("dd-MMM-yyyy HH:mm");
                string IN_To = Convert.ToDateTime(ShiftIn).AddMinutes(RawINDefinitionTo).ToString("dd-MMM-yyyy HH:mm");

                string OUT_From = Convert.ToDateTime(ShiftOut).AddMinutes(-RawOUTDefinitionFrom).ToString("dd-MMM-yyyy HH:mm");
                string OUT_To = Convert.ToDateTime(ShiftOut).AddMinutes(RawOUTDefinitionTo).ToString("dd-MMM-yyyy HH:mm");

                if (_punchDT >= Convert.ToDateTime(IN_From) && _punchDT <= Convert.ToDateTime(IN_To))
                {
                    res = "IN";
                }

                if (_punchDT >= Convert.ToDateTime(OUT_From) && _punchDT <= Convert.ToDateTime(OUT_To))
                {
                    res = "OUT";
                }

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string xGetTypeINOUT(string workDate, string workdate, string worktime, DataRowView drEmpShift)
        {
            string res = string.Empty;
            try
            {
                string punchDT = Convert.ToDateTime(workdate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(worktime).ToString("HH:mm");
                DateTime _punchDT = Convert.ToDateTime(punchDT);

                string ShiftInTime = drEmpShift["InTime"].ToString();
                string ShiftOutTime = drEmpShift["OutTime"].ToString();
                int RawINDefinitionFrom = Convert.ToInt32(drEmpShift["RawINDefinitionFrom"].ToString());
                int RawINDefinitionTo = Convert.ToInt32(drEmpShift["RawINDefinitionTo"].ToString());
                int RawOUTDefinitionFrom = Convert.ToInt32(drEmpShift["RawOUTDefinitionFrom"].ToString());
                int RawOUTDefinitionTo = Convert.ToInt32(drEmpShift["RawOUTDefinitionTo"].ToString());

                string ShiftIn = Convert.ToDateTime(workdate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(ShiftInTime).ToString("HH:mm");
                string ShiftOut = Convert.ToDateTime(workdate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(ShiftOutTime).ToString("HH:mm");//TBD
                //if (Convert.ToDateTime(ShiftOut) < Convert.ToDateTime(ShiftIn))
                //{
                //    ShiftOut = Convert.ToDateTime(ShiftOut).AddDays(-1).ToString("dd-MMM-yyyy HH:mm");
                //}

                string IN_From = Convert.ToDateTime(ShiftIn).AddMinutes(-RawINDefinitionFrom).ToString("dd-MMM-yyyy HH:mm");
                string IN_To = Convert.ToDateTime(ShiftIn).AddMinutes(RawINDefinitionTo).ToString("dd-MMM-yyyy HH:mm");

                string OUT_From = Convert.ToDateTime(ShiftOut).AddMinutes(-RawOUTDefinitionFrom).ToString("dd-MMM-yyyy HH:mm");
                string OUT_To = Convert.ToDateTime(ShiftOut).AddMinutes(RawOUTDefinitionTo).ToString("dd-MMM-yyyy HH:mm");

                if (_punchDT >= Convert.ToDateTime(IN_From) && _punchDT <= Convert.ToDateTime(IN_To))
                {
                    res = "IN";
                }

                if (_punchDT >= Convert.ToDateTime(OUT_From) && _punchDT <= Convert.ToDateTime(OUT_To))
                {
                    res = "OUT";
                }

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void GetEmpDateWise(string plantid, string workdate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select
                       	InTime = CASE WHEN ISNULL(C.InTime, '') != '' THEN C.InTime  ELSE S.InTime END
						,OutTime = CASE WHEN ISNULL(C.OutTime, '') != '' THEN C.OutTime  ELSE S.OutTime END

						,RawINDefinitionFrom = CASE WHEN ISNULL(C.RawINDefinitionFrom, '') != '' THEN C.RawINDefinitionFrom  ELSE S.RawINDefinitionFrom END
						,RawINDefinitionTo = CASE WHEN ISNULL(C.RawINDefinitionTo, '') != '' THEN C.RawINDefinitionTo  ELSE S.RawINDefinitionTo END
						,RawOUTDefinitionFrom = CASE WHEN ISNULL(C.RawOUTDefinitionFrom, '') != '' THEN C.RawOUTDefinitionFrom  ELSE S.RawOUTDefinitionFrom END
						,RawOUTDefinitionTo = CASE WHEN ISNULL(C.RawOUTDefinitionTo, '') != '' THEN C.RawOUTDefinitionTo  ELSE S.RawOUTDefinitionTo END
                              --,s.[RawINDefinitionFrom]
                              --,s.[RawINDefinitionTo]
                              --,s.[RawOUTDefinitionFrom]
                              --,s.[RawOUTDefinitionTo]
	                          ,a.EmpSystemID
                              ,a.WorkDate
                         from EmpDateWiseShiftAssign a
                        left join ShiftDefination s on s.SystemID = a.ShiftSystemID
                        left join EmployeeInformation e on e.SystemId = a.EmpSystemID
	                    LEFT JOIN (
									SELECT SCM.*, SCC.ShiftDate FROM [dbo].[ShiftTimeChgMaster] SCM
											INNER JOIN [dbo].[ShiftTimeChgChild] SCC ON SCM.SystemID = STCMasterSystemID
									WHERE SCC.ShiftDate ='" + workdate + @"'
									) C ON a.ShiftSystemID = C.ShiftDefinationID
                        where WorkDate = '" + workdate + @"'  and e.plantid='" + plantid + "'";

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
        private void GetTypeLessRawData(string plantid, string workdate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            DateTime _prv = Convert.ToDateTime(workdate);
            DateTime _nxt = Convert.ToDateTime(workdate).AddDays(1);
            try
            {
                strSql = "select * from AttdnRawData where pdate between '" + _prv + "' and '" + _nxt + "' and plantid='" + plantid + "'  and isnull(ptype,'')=''";

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


        private void GetAllRawData(string plantid, string sEmpSystemIDColl, string workdate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            DateTime _prv = Convert.ToDateTime(workdate);
            DateTime _nxt = Convert.ToDateTime(workdate).AddDays(1);
            try
            {
                strSql = "select * from AttdnRawData where pdate between '" + _prv + "' and '" + _nxt + "' and LogDownLoadNum in (" + sEmpSystemIDColl + ") and plantid='" + plantid + "' ";



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
        private void GetPlantWiseHRMSSettingData(string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = "Select * from PlantWiseHRMSSetting where PlantID ='" + plantid + "' ";

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
        private void GetEmpDateWise(string plantid, string emps, string workdate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select
                       	InTime = CASE WHEN ISNULL(C.InTime, '') != '' THEN C.InTime  ELSE S.InTime END
						,OutTime = CASE WHEN ISNULL(C.OutTime, '') != '' THEN C.OutTime  ELSE S.OutTime END

						,RawINDefinitionFrom = CASE WHEN ISNULL(C.RawINDefinitionFrom, '') != '' THEN C.RawINDefinitionFrom  ELSE S.RawINDefinitionFrom END
						,RawINDefinitionTo = CASE WHEN ISNULL(C.RawINDefinitionTo, '') != '' THEN C.RawINDefinitionTo  ELSE S.RawINDefinitionTo END
						,RawOUTDefinitionFrom = CASE WHEN ISNULL(C.RawOUTDefinitionFrom, '') != '' THEN C.RawOUTDefinitionFrom  ELSE S.RawOUTDefinitionFrom END
						,RawOUTDefinitionTo = CASE WHEN ISNULL(C.RawOUTDefinitionTo, '') != '' THEN C.RawOUTDefinitionTo  ELSE S.RawOUTDefinitionTo END
                              --,s.[RawINDefinitionFrom]
                              --,s.[RawINDefinitionTo]
                              --,s.[RawOUTDefinitionFrom]
                              --,s.[RawOUTDefinitionTo]
	                          ,a.EmpSystemID
                              ,a.WorkDate
                         from EmpDateWiseShiftAssign a
                        left join ShiftDefination s on s.SystemID = a.ShiftSystemID
                        left join EmployeeInformation e on e.SystemId = a.EmpSystemID
	                    LEFT JOIN (
									SELECT SCM.*, SCC.ShiftDate FROM [dbo].[ShiftTimeChgMaster] SCM
											INNER JOIN [dbo].[ShiftTimeChgChild] SCC ON SCM.SystemID = STCMasterSystemID
									WHERE SCC.ShiftDate ='" + workdate + @"'
									) C ON a.ShiftSystemID = C.ShiftDefinationID
                        where WorkDate = '" + workdate + @"' and e.systemid in (" + emps + ")  and e.plantid='" + plantid + "'";

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

        public void SetRawINOUTonShiftAssignment(string plantid, string GroupSysID, string workdate, string sEmpSystemIDColl)
        {
            DataSet dsEmpShift = null;
            DataSet dsRaw = null;
            //DataSet dsPlantWiseHRMSSetting = null;
            bool ShiftBasedPunchFlag = false;
            try
            {

                GetPlantWiseHRMSSettingData(plantid, out DataSet dsPlantSetting);
                if (bplib.clsWebLib.GetBoolData(dsPlantSetting.Tables[0].Rows[0]["ShiftBasedPunchFlag"].ToString()) == false)
                    return;
                //GetPlantWiseHRMSSettingData(plantid, out dsPlantWiseHRMSSetting);
                //if (dsPlantWiseHRMSSetting.Tables[0].Rows.Count > 0)
                //{
                //    ShiftBasedPunchFlag = Convert.ToBoolean(dsPlantWiseHRMSSetting.Tables[0].Rows[0]["ShiftBasedPunchFlag"]);
                //}
                //if (ShiftBasedPunchFlag)
                //{
                GetAllRawData(plantid, sEmpSystemIDColl, workdate, out dsRaw);
                #region ShiftProcess
                //ShiftProcess sp = new ShiftProcess();
                //FixedShiftProcess sp2 = new FixedShiftProcess();
                ////clsAttendance.AttendanceProcessAplos sp2 = new clsAttendance.AttendanceProcessAplos();
                //sp2.ShiftProcess(plantid, workdate, GroupSysID, sEmpSystemIDColl);
                //sp.ShiftProcessStart(plantid, workdate, GroupSysID, sEmpSystemIDColl);//_emplist

                GenericShiftProcess gsp = new GenericShiftProcess();
                gsp.Process(plantid, workdate, GroupSysID, sEmpSystemIDColl);
                #endregion

                if (dsRaw.Tables[0].Rows.Count > 0)
                {



                    GetEmpDateWise(plantid, sEmpSystemIDColl, workdate, out dsEmpShift);
                    //calculation
                    for (int i = 0; i < dsRaw.Tables[0].Rows.Count; i++)
                    {
                        string _rid = dsRaw.Tables[0].Rows[i]["Id"].ToString();
                        if (_rid == "TX238610")
                        {

                        }
                        DataView dvRaw = new DataView(dsRaw.Tables[0]);
                        dvRaw.RowFilter = "Id='" + _rid + "'";
                        if (dvRaw.Count > 0)
                        {
                            
                            if (bplib.clsWebLib.GetBoolData(dvRaw[0]["FlagSetByProcess"]) == true
                            || string.IsNullOrEmpty(dvRaw[0]["PType"].ToString()) == true)
                            {
                                string _Type = GetINOUTType(workdate, dsEmpShift, dvRaw[0]);
                                if (string.IsNullOrEmpty(_Type) == false)
                                {
                                    DataRow drRaw = dvRaw[0].Row;
                                    drRaw.BeginEdit();
                                    drRaw["FlagSetByProcess"] = true;
                                    drRaw["PType"] = _Type;
                                    drRaw["dateupdated"] = DateTime.Now;
                                    drRaw.EndEdit();
                                }//type found
                            }
                        }//if
                    }//for
                    SaveDataSets(dsRaw);
                }//dsRaw.Tables[0].Rows.Count
                 //}

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//End Function   
    }
}

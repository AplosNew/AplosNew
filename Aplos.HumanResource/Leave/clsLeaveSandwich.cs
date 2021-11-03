using System;
using System.Data;

namespace Library.HumanResource.Leave
{
   public class clsLeaveSandwich
    {
        string _companyGroupId, _plantId, _userName = string.Empty;
        public clsLeaveSandwich(string CompanyGroupId,string PlantId,string UserName)
        {
            this._companyGroupId = CompanyGroupId;
            this._plantId = PlantId;
            this._userName = UserName;
        }
        public void ProcessSandwich(string sFromDate, string sToDate, string CurrDate,string empids)
        {
            DataSet dsEmpList = null;

            DataSet dsSave_sandwichlog = null;
            DataSet dsSave_leavemaster = null;
            DataSet dsSave_leavedetail = null;

            DataSet ds_log_todelete = null;

            bool _IsDataOk = false;
            string _leaveids = string.Empty;
            try
            {
                CustomIdentityLocal identity = new CustomIdentityLocal(this._companyGroupId, this._plantId, this._userName);

                DateTime dtf = Convert.ToDateTime(sFromDate).AddDays(-7);
                DateTime dtt = Convert.ToDateTime(sToDate).AddDays(7);
                if (dtt > Convert.ToDateTime(CurrDate))
                {
                    dtt = Convert.ToDateTime(CurrDate);
                }

                _GetEmpList(empids, identity.PlantId, dtf.ToString("dd-MMM-yyyy"), dtt.ToString("dd-MMM-yyyy"), out dsEmpList);
               

                string _seed_log = string.Empty;
                string _seed_master = string.Empty;
                string _seed_detail = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "LEAVE_SANDWICH_LOG", out _seed_log);
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "LEAVE_MASTER_SAND", out _seed_master);
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "LEAVE_DETAIL_SAND", out _seed_detail);
                int _count = 0;
                if (dsEmpList.Tables[0].Rows.Count > 0)
                {
                    //call delete
                    _GetSandwichLeaveLog_Selete(empids, dtf.ToString("dd-MMM-yyyy"), dtt.ToString("dd-MMM-yyyy"), identity.PlantId, out ds_log_todelete);
                    _getLeaveIds(ds_log_todelete, out _leaveids);
                    _DeleteSandwichLeaveLog(_leaveids);
                    ///delete leave master and detail tbd
                    _GetSandwichLeaveLog(empids, dtf.ToString("dd-MMM-yyyy"), dtt.ToString("dd-MMM-yyyy"), identity.PlantId, out dsSave_sandwichlog);
                    _GetLeaveTransaction(identity.PlantId, out dsSave_leavemaster);
                    _GetLeaveTransactionDetails(out dsSave_leavedetail);

                    for (int i = 0; i < dsEmpList.Tables[0].Rows.Count; i++)
                    {
                        _Validation(dsEmpList.Tables[0].Rows[i], out _IsDataOk); 
                        if (_IsDataOk)
                        {
                            _count++;
                            string _masterpk = string.Empty;
                            _LeaveMaster(ref dsSave_leavemaster, dsEmpList.Tables[0].Rows[i], identity, _count, _seed_master,out _masterpk);//tbd 
                            if (_masterpk.Length > 0)
                            {
                                _LeaveDetail(ref dsSave_leavedetail, dsEmpList.Tables[0].Rows[i]["AttdnProcDate"].ToString(), identity, _count, _seed_detail, _masterpk);//tbd
                                _SandwichLog(ref dsSave_sandwichlog, dsEmpList.Tables[0].Rows[i], identity, _count, _seed_log, _masterpk);//done
                            }
                        }//IsDataOk
                    }//for
                    _Save(dsSave_leavemaster,dsSave_sandwichlog, dsSave_leavedetail);
                }//count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //sandwich log
        void _getLeaveIds(DataSet ds,out string _leaveids)
        {
            _leaveids = "''";
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    _leaveids += ",'" + ds.Tables[0].Rows[i]["LeaveMasterid"].ToString() + "'";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _Validation(DataRow dr,out bool _IsDataOk)
        {
            try
            {
                _IsDataOk = false;
                string _Suc_LV_W = dr["Suc_LV_W"].ToString();
                string _Suc_LV_H = dr["Suc_LV_H"].ToString();
                string _Post_LV_W = dr["Post_LV_W"].ToString();
                string _Post_LV_H = dr["Post_LV_H"].ToString();

                if (string.IsNullOrEmpty(_Suc_LV_W) && string.IsNullOrEmpty(_Suc_LV_H))
                {
                    _IsDataOk = false;
                }
                else if (string.IsNullOrEmpty(_Post_LV_W) && string.IsNullOrEmpty(_Post_LV_H))
                {
                    _IsDataOk = false;
                }
                else
                {
                    _IsDataOk = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _SandwichLog(ref DataSet dsSave, DataRow drSource, CustomIdentityLocal _identity, int _count, string _seed,string _masterpk)
        {
            DataView _dvSave = null;
            try
            {
                string _WorkDate = drSource["AttdnProcDate"].ToString();
                string _EmpSystemID = drSource["EmpSystemID"].ToString();
                _dvSave = new DataView(dsSave.Tables[0]);
                _dvSave.RowFilter = "EmpSystemID='" + _EmpSystemID + "' AND WorkDate='" + _WorkDate + "' ";
                if (_dvSave.Count == 0)
                {
                    DataRow _dr = dsSave.Tables[0].NewRow();
                    _AddRow(ref _dr, _identity, _WorkDate, _EmpSystemID, _seed, _count, _masterpk);
                    dsSave.Tables[0].Rows.Add(_dr);
                }
                _dvSave.RowFilter = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _AddRow(ref DataRow _dr, CustomIdentityLocal _identity, string _WorkDate, string _EmpSystemID, string _seed, int _count,string _masterpk)
        {
            try
            {
               // _dr["Id"] = "S" + _seed + "-" + _count;
                _dr["PlantID"] = _identity.PlantId;
                _dr["EmpSystemID"] = _EmpSystemID;
                _dr["WorkDate"] = _WorkDate;
                _dr["LeaveMasterId"] = _masterpk;
                _dr["AddedBy"] = _identity.Name;
                _dr["DateAdded"] = System.DateTime.Now.ToString();
               // _dr["AddedFromIP"] = _identity.IPAddress;
                _dr["UpdatedBy"] = _identity.Name;
                _dr["DateUpdated"] = System.DateTime.Now.ToString();
               // _dr["UpdatedFromIP"] = _identity.IPAddress;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _GetEmpList(string empids, string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string _wc = string.Empty;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (empids.Length > 0)
                {
                    _wc = " and apd.empsystemid in (" + empids + @")";
                }

                strSQL = @" DECLARE @FromDate DATE =  '"+ sFromDate + @"'
                            DECLARE      @ToDate DATE = '" + sToDate + @"'
                            DECLARE		@Plantid varchar(10)
		                            set @Plantid='"+ sPlantID + @"'

                     SELECT 0 CheckBoxSelect 
                     , APD.EmpSystemID
                     , FORMAT(APD.WorkDate ,'dd-MMM-yyyy') AttdnProcDate
                     , APD.DayStatus,apd.Category
                     , FORMAT( x.WorkDate  ,'dd-MMM-yyyy') BeforeWorkDate
                     , x.DayStatus BeforeDayStatus
                     , FORMAT(y.WorkDate,'dd-MMM-yyyy') AfterWorkDate
                     , y.DayStatus AfterDayStatus
                     , EI.EmployeeCode
                     , EI.EmployeeName
                     , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                     , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
					 ---------------
					 ,xs.DayStatus PreDaystatus
					 ,ys.DayStatus PostDaystatus
					 ,S_L_W.LTSystemID Suc_LV_W
					 ,S_L_H.LTSystemID Suc_LV_H

					 ,P_L_W.LTSystemID Post_LV_W
					 ,P_L_H.LTSystemID Post_LV_H
                     ,y.LTSystemID postLeaveid
					 ,x.LTSystemID preLeaveid
 
                    FROM ( select apd.EmpSystemID, apd.WorkDate, apd.DayStatus,dt.Category
	                       FROM AttdnProcessData AS apd
                           left join daytype dt on dt.daytype=apd.daystatus
	                       WHERE apd.WorkDate BETWEEN @FromDate AND @ToDate
                           AND apd.PlantID=@Plantid
	                       AND isnull(apd.DayStatus,'') IN (
                                                            ---DayType
                                                            SELECT DayType  FROM DayType WHERE Category='Weekend' 
                                                            UNION
                                                            SELECT DayType FROM DayType WHERE Category='Holiday' 

                                                           ) 
                           AND apd.EmpSystemID NOT IN (select EmpSystemID  FROM [SCS].[WeeklyAbsentismAssignment] WHERE EmpSystemID=apd.EmpSystemID AND WorkingDate=apd.WorkDate)
                    ) AS APD
                    LEFT OUTER JOIN AttdnProcessData AS X ON apd.EmpSystemID=x.EmpSystemID 
															AND isnull(x.WorkDate,'')=(--1
																						SELECT TOP 1 isnull(WorkDate,'') AS PK 
                                                                                        FROM AttdnProcessData WHERE WorkDate<apd.WorkDate 
																						AND EmpSystemID=APD.EmpSystemID	
																						AND DayStatus NOT IN (
                                                                                                            ---DayType
                                                                                                            SELECT DayType  FROM DayType WHERE Category='Weekend' 
                                                                                                            UNION
                                                                                                            SELECT DayType FROM DayType WHERE Category='Holiday' 
                                                                                                            )
                                                                                        ORDER BY WorkDate  DESC
																						)--1
					--get leave policy for X
					left join (select * from AttdnProcessData ) xs on xs.WorkDate=DATEADD(DAY,1, x.WorkDate) and xs.EmpSystemID=x.EmpSystemID
					left join
					(
					select 
							d.IsPrecedingHoliday,d.IsPrecedingWeekoff,d.IsSucceedignHoliday,d.IsSucceedignWeekoff,e.SystemId,d.LTSystemID
							from EmployeeInformation e
							left join mst.DesignationMasterLegalDesignation Ld on Ld.LegalDesignationId=e.LegalDesignationId
							left join scs.DesignationMasterConfiguration c on c.DesignationMasterId=ld.DesignationMasterId and c.PlantId=@Plantid
							left join LeavePolicyDetail d on d.LPMSystemID=c.LeavePolicyMasterId
							where d.IsSucceedignHoliday=1 
					) S_L_H on S_L_H.SystemId=x.EmpSystemID and S_L_H.LTSystemID=x.LTSystemID and xs.DayStatus in (SELECT DayType FROM DayType WHERE Category='Holiday' )

					left join
					(
					select 
							d.IsPrecedingHoliday,d.IsPrecedingWeekoff,d.IsSucceedignHoliday,d.IsSucceedignWeekoff,e.SystemId,d.LTSystemID
							from EmployeeInformation e
							left join mst.DesignationMasterLegalDesignation Ld on Ld.LegalDesignationId=e.LegalDesignationId
							left join scs.DesignationMasterConfiguration c on c.DesignationMasterId=ld.DesignationMasterId and c.PlantId=@Plantid
							left join LeavePolicyDetail d on d.LPMSystemID=c.LeavePolicyMasterId
							where d.IsSucceedignWeekoff=1 
					) S_L_W on S_L_W.SystemId=x.EmpSystemID and S_L_W.LTSystemID=x.LTSystemID and xs.DayStatus in (SELECT DayType FROM DayType WHERE Category='Weekend' )
					--end leave policy for X

					


                    LEFT OUTER JOIN AttdnProcessData AS Y ON apd.EmpSystemID=Y.EmpSystemID AND isnull(Y.WorkDate,'')=(SELECT TOP 1 isnull(WorkDate,'') AS PK 
                                                                                                                      FROM AttdnProcessData WHERE WorkDate>apd.WorkDate 
																								                      AND EmpSystemID=APD.EmpSystemID	
																													  AND DayStatus NOT IN (
                                                                                                                                            ---DayType
                                                                                                                                        SELECT DayType  FROM DayType WHERE Category='Weekend' 
                                                                                                                                        UNION
                                                                                                                                        SELECT DayType FROM DayType WHERE Category='Holiday' 
                                                                                                                                        ) 
                                                                                                                      ORDER BY WorkDate  ASC)
                                                                                                  
                                                                                                  
                           --get leave policy for Y
					left join (select * from AttdnProcessData ) ys on ys.WorkDate=DATEADD(DAY,-1, y.WorkDate) and ys.EmpSystemID=y.EmpSystemID

					left join
					(
					select 
							d.IsPrecedingHoliday,d.IsPrecedingWeekoff,d.IsSucceedignHoliday,d.IsSucceedignWeekoff,e.SystemId,d.LTSystemID
							from EmployeeInformation e
							left join mst.DesignationMasterLegalDesignation Ld on Ld.LegalDesignationId=e.LegalDesignationId
							left join scs.DesignationMasterConfiguration c on c.DesignationMasterId=ld.DesignationMasterId and c.PlantId=@Plantid
							left join LeavePolicyDetail d on d.LPMSystemID=c.LeavePolicyMasterId
							where d.IsSucceedignHoliday=1 
					) P_L_H on P_L_H.SystemId=y.EmpSystemID and P_L_H.LTSystemID=y.LTSystemID and ys.DayStatus in (SELECT DayType FROM DayType WHERE Category='Holiday' )

					left join
					(
					select 
							d.IsPrecedingHoliday,d.IsPrecedingWeekoff,d.IsSucceedignHoliday,d.IsSucceedignWeekoff,e.SystemId,d.LTSystemID
							from EmployeeInformation e
							left join mst.DesignationMasterLegalDesignation Ld on Ld.LegalDesignationId=e.LegalDesignationId
							left join scs.DesignationMasterConfiguration c on c.DesignationMasterId=ld.DesignationMasterId and c.PlantId=@Plantid
							left join LeavePolicyDetail d on d.LPMSystemID=c.LeavePolicyMasterId
							where d.IsSucceedignWeekoff=1 
					) P_L_W on P_L_W.SystemId=y.EmpSystemID and P_L_W.LTSystemID=y.LTSystemID and ys.DayStatus in (SELECT DayType FROM DayType WHERE Category='Weekend' )

					--get leave policy for Y
					
                     LEFT JOIN dbo.Employeeinformation EI ON EI.SystemId = apd.EmpSystemID
                    WHERE
					
						(ISNULL(x.DayStatus,'')='LV'  AND ISNULL(Y.DayStatus,'')='LV' )
" + _wc + @"
				--and apd.EmpSystemID='1900941'
				--and (isnull(S_L_H.LTSystemID ,'')<>'' OR isnull(S_L_W.LTSystemID ,'')<>'')
                    ORDER BY APD.EmpSystemID,APD.WorkDate  ";

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
        }//End 
        void _GetSandwichLeaveLog(string empids, string sFromDate, string sToDate,string plantid, out System.Data.DataSet dsRef)
        {
            string _wc = string.Empty;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (empids.Length > 0)
                {
                    _wc = " and empsystemid in (" + empids + @")";
                }
                strSQL = @" SELECT *  FROM SandwichLeaveLog where  plantid='"+ plantid + "'  " + _wc + @"  and WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' ";
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
        }//End 
        void _GetSandwichLeaveLog_Selete(string empids, string sFromDate, string sToDate, string plantid, out System.Data.DataSet dsRef)
        {
            string _wc = string.Empty;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                
                strSQL = @" SELECT *  FROM SandwichLeaveLog where  plantid='" + plantid + "'  " + _wc + @"  and WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' ";
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
        }//End 
        void _Save(params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();//

                //objCon.ExecuteNonQueryWrapper("DELETE FROM SalaryProcChild WHERE MonthNo = " + intMonthNo + " AND YearNo = " + intYearNo + " AND IsDisbursed = 0 AND (" + strEmp + ")", true, "1");

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
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception ex2)
                {
                    throw ex;
                }

            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }

        }//End Function
        void _DeleteSandwichLeaveLog(string _leaveids)
        {
            string _wc = string.Empty;
            ConnectionManager.DAL.ConManager objCon = null;
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            string sql3 = string.Empty;
            try
            {                
                sql1 = @"delete from LeaveTransactionDetails  where LvTrnsSystemID in (" + _leaveids + @")"; 
                sql2 = @"delete  from SandwichLeaveLog where  LeaveMasterid in ("+_leaveids+@")";
                sql3 = @"delete  from LeaveTransaction where  systemid in ("+ _leaveids + @")";            

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(sql1, true, "1");
                objCon.ExecuteNonQueryWrapper(sql2, true, "1");
                objCon.ExecuteNonQueryWrapper(sql3, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception ex2)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function 
        ///leave master///
        void _GetLeaveTransaction(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from LeaveTransaction where plantid='" + plantid + "'";
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
        void _LeaveMaster(ref DataSet dsSaveLeaveMaster,DataRow drSource,CustomIdentityLocal _identity,int _count,string _seed,out string _masterpk)
        {
            DataView _dvSave = null;
            _masterpk = string.Empty;
            try
            {
                string _WorkDate = drSource["AttdnProcDate"].ToString();
                string _EmpSystemID = drSource["EmpSystemID"].ToString();
                string _LTSystemID = drSource["postLeaveid"].ToString();
                _dvSave = new DataView(dsSaveLeaveMaster.Tables[0]);
                _dvSave.RowFilter = "EmpSystemID='" + _EmpSystemID + "' and LTSystemID='"+_LTSystemID+@"' AND FromDate='" + _WorkDate + "' and ToDate='"+_WorkDate+@"' ";
                if (_dvSave.Count == 0)
                {
                    _masterpk= "W" + _seed + "-" + _count;
                    DataRow _dr = dsSaveLeaveMaster.Tables[0].NewRow();
                    _AddRowLeaveMaster(ref _dr, _identity, _WorkDate, _EmpSystemID, _LTSystemID, _masterpk);
                    dsSaveLeaveMaster.Tables[0].Rows.Add(_dr);
                }
                else
                {
                    _masterpk = _dvSave[0]["Systemid"].ToString();
                }
                _dvSave.RowFilter = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _AddRowLeaveMaster(ref DataRow _dr, CustomIdentityLocal _identity, string _WorkDate, string _EmpSystemID,string _LTSystemID,string _masterpk)
        {
            try
            {
                _dr["SystemId"] = _masterpk;
                _dr["PlantID"] = _identity.PlantId;
                _dr["EmpSystemID"] = _EmpSystemID;
                _dr["LTSystemID"] = _LTSystemID;
                _dr["FromDate"] = _WorkDate;
                _dr["ToDate"] = _WorkDate;
                _dr["LeaveDays"] = 1.00;                
                _dr["LeaveDayType"] = "FullDay";
                _dr["IsApproved"] =1; 
                _dr["FirstApprovingStatus"] =1;
                 _dr["LvReason"] ="Sandwich";

                _dr["GroupID"] = _identity.CompanyGroupId;
                _dr["AddedBy"] = _identity.Name;
                _dr["DateAdded"] = System.DateTime.Now.ToString();
                _dr["UpdatedBy"] = _identity.Name;
                _dr["DateUpdated"] = System.DateTime.Now.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //=============leave detail
        void _GetLeaveTransactionDetails(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from LeaveTransactionDetails where SystemID=''";
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
        void _LeaveDetail(ref DataSet dsSaveLeaveDetail, string _WorkDate, CustomIdentityLocal _identity, int _count, string _seed,string _masterpk)
        {
            DataView _dvSave = null;
            try
            {
                _dvSave = new DataView(dsSaveLeaveDetail.Tables[0]);
                _dvSave.RowFilter = "LvTrnsSystemID='" + _masterpk + "' and workdate='"+_WorkDate+"'";
                if (_dvSave.Count == 0)
                {
                    string pk= "SD" + _seed + "-" + _count;
                    DataRow _dr = dsSaveLeaveDetail.Tables[0].NewRow();
                    _AddRowLeaveDetail(ref _dr, _identity, _WorkDate, _masterpk, pk);
                    dsSaveLeaveDetail.Tables[0].Rows.Add(_dr);
                }
                _dvSave.RowFilter = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _AddRowLeaveDetail(ref DataRow _dr, CustomIdentityLocal _identity, string _WorkDate, string _masterpk, string pk)
        {
            try
            {
                _dr["SystemId"] = pk;
                _dr["LvTrnsSystemID"] = _masterpk;
                _dr["WorkDate"] = _WorkDate;
                _dr["DayType"] = "NW"; 
                _dr["LeaveStatus"] = "LV"; 
                _dr["IsAvailed"] = 1; 
                _dr["LeaveDuration"] = 1; 

                 _dr["AddedBy"] = _identity.Name;
                _dr["DateAdded"] = System.DateTime.Now.ToString();
                _dr["UpdatedBy"] = _identity.Name;
                _dr["DateUpdated"] = System.DateTime.Now.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

    public class CustomIdentityLocal
    {
        public string Name { get; set; }
        public string PlantId { get; set; }
        public string CompanyGroupId { get; set; }
       public CustomIdentityLocal(string CompanyGroupId, string PlantId, string UserName)
        {
            this.CompanyGroupId = CompanyGroupId;
            this.PlantId = PlantId;
            this.Name = UserName;
        }

    }
}

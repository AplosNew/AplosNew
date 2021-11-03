using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance
{
   public class clsWeekOffProcess
    {
        public void Process(string _empids, string sAttnDate, string _plantid, string GroupSysID,string username)
        {
            DataSet dsComAssWkOff1 = null;
            DataSet dsCurrWeekOff1 = null;
            string sEmpSysIDCollForSft = string.Empty;
            List<EmpShiftInfo> _empList = null;
            try
            {
                _getEmpList(_plantid, sAttnDate, _empids, out sEmpSysIDCollForSft, out _empList);

                _getEmpCurrWeekOff(sAttnDate, sEmpSysIDCollForSft.Trim(), out dsCurrWeekOff1);
                List<OffDayIndividual> _listIndiOffDay = new List<OffDayIndividual>();
                if (dsCurrWeekOff1.Tables[0].Rows.Count > 0)
                {
                    _listIndiOffDay = dsCurrWeekOff1.Tables[0].ToList<OffDayIndividual>();
                }
                _getCompanyAssignWeekOffDateRangeWise(GroupSysID, _plantid, sAttnDate.Trim(), out dsComAssWkOff1);
                List<OffDayCompanyWise> _listCompanyOffDay = new List<OffDayCompanyWise>();
                if (dsComAssWkOff1.Tables[0].Rows.Count > 0)
                {
                    _listCompanyOffDay = dsComAssWkOff1.Tables[0].ToList<OffDayCompanyWise>();
                }


                for (int i = 0; i < _empList.Count; i++)
                {
                    string sEmpSystemID = _empList[i].SystemID;
                    string sDayType = "NW";
                    _getOffDay(_listCompanyOffDay, _listIndiOffDay, sAttnDate, sEmpSystemID, out sDayType);
                    if(sDayType.ToUpper()=="W")
                    {
                    _save(sEmpSystemID, sAttnDate, username);
                    }
                }//for

                //update
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       public void _updateWeekoff(string _empsystemid, string _fromdate,string _todate, string _user)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();//

                objCon.ExecuteNonQueryWrapper("update EmpDateWiseShiftAssign set DayType='NW',UpdatedBy='" + _user + "',DateUpdated='" + DateTime.Now + "' where EmpSystemID='" + _empsystemid + "' and WorkDate between '" + _fromdate + "' and '" + _todate + "'", true, "1");
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
        void _save(string _empsystemid,string _workdate,string _user)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();//

                objCon.ExecuteNonQueryWrapper("update EmpDateWiseShiftAssign set DayType='W',UpdatedBy='"+_user+ "',DateUpdated='"+DateTime.Now+"' where EmpSystemID='" + _empsystemid + "' and WorkDate='" + _workdate + "'", true, "1");
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
        void _getEmpList(string _plantid, string sAttnDate, string _emplist, out string empIds, out List<EmpShiftInfo> _list)
        {
            empIds = string.Empty;
            DataSet dsEmpInfoForShiftProc = null;
            _list = new List<EmpShiftInfo>();
            try
            {
                if (_emplist.Length == 0)
                {
                    _getEmployeeInformationForShiftProcess(_plantid, "", sAttnDate.Trim(), out dsEmpInfoForShiftProc);
                }
                else
                {
                    _getEmployeeInformationForShiftProcess(_plantid, _emplist, sAttnDate.Trim(), out dsEmpInfoForShiftProc);
                }

                if (dsEmpInfoForShiftProc.Tables[0].Rows.Count > 0)
                {
                    if (_emplist.Length == 0)
                    {
                        for (int i = 0; i < dsEmpInfoForShiftProc.Tables[0].Rows.Count; i++)
                        {
                            if (empIds.Trim() == "")
                            {
                                empIds = "'" + dsEmpInfoForShiftProc.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                            else
                            {
                                empIds = empIds.Trim() + ", '" + dsEmpInfoForShiftProc.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                        }

                    }//_emplist
                    else
                    {
                        empIds = _emplist;
                    }
                    _list = dsEmpInfoForShiftProc.Tables[0].ToList<EmpShiftInfo>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _getEmployeeInformationForShiftProcess(string sPlantID, string sEmpSysIdColl, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                string sEmpId = "";

                if (sEmpSysIdColl.Trim() != "")
                {
                    sEmpId = " and SystemID IN (" + sEmpSysIdColl + @") ";
                }

                strSql = @"
                                        SELECT * FROM 
                                        (
                                         SELECT SystemID,EmployeeCode,PlantID FROM EmployeeInformation WHERE PlantID = '" + sPlantID + @"'  " + sEmpId + @"	                                        
                                         ) A  
                                         ORDER BY EmployeeCode";

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
        void _getOffDay(List<OffDayCompanyWise> dtComAssWkOff, List<OffDayIndividual> dtEmpWkOff, string Workdate, string _empSystemid, out string sDayType)
        {
            sDayType = "NW";
            //DataView dvComAssWkOff = null;
            bool IsIndividualWeekOff = false;
            bool AlignWithCC = false;
            bool isindi = false;
            try
            {
                //DataView dvWeekInfo = new DataView(dtEmpWkOff);
                //dvWeekInfo.RowFilter = "EmpSystemID='" + _empSystemid + "'";

                var _obj_indi = dtEmpWkOff.Where(r => r.EmpSystemID == _empSystemid).FirstOrDefault();
                if (_obj_indi != null && string.IsNullOrEmpty(_obj_indi.EmpSystemID) == false)
                {
                    isindi = true;
                    IsIndividualWeekOff = _obj_indi.IndividualWeekOff;// Convert.ToBoolean(dvWeekInfo[0]["IndividualWeekOff"].ToString().Trim());
                    AlignWithCC = _obj_indi.AlignWithCC;// Convert.ToBoolean(dvWeekInfo[0]["AlignWithCC"].ToString().Trim());
                }

                if (IsIndividualWeekOff == false)
                {
                    //dvComAssWkOff = new DataView();
                    //dvComAssWkOff.Table = dtComAssWkOff;
                    //dvComAssWkOff.RowFilter = "OffDayDate = '" + Workdate + "' ";

                    var _obj = dtComAssWkOff.Where(r => r.OffDayDate == Convert.ToDateTime(Workdate)).FirstOrDefault();

                    if (_obj != null && string.IsNullOrEmpty(_obj.DayLengthType) == false)
                    {
                        var sDayLengthType = _obj.DayLengthType;
                        if (sDayLengthType.ToUpper() == "FULL DAY" || sDayLengthType.ToUpper() == "FULLDAY")
                        {
                            sDayType = "W";
                        }
                    }
                }
                else
                {
                    if (isindi)
                    {
                        var _FstOffDay = _obj_indi.FstOffDay;// dvWeekInfo[0]["FstOffDay"].ToString().Trim();
                        var d = Convert.ToDateTime(Workdate).ToString("dddd");
                        if (_FstOffDay.ToUpper() == d.ToUpper())
                        {
                            sDayType = "W";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void _getEmpCurrWeekOff(string sAttnDate, string empids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT 
                                      [EmpSystemID]
                                      ,[FixSystemID]
                                      ,max(EffectiveDate) EffectiveDate
                                      ,[AlignWithCC]
                                      ,[IndividualWeekOff]
                                      ,[FstOffDay]
                                      ,[FstDayLengthType]
                                      ,[SndOffDay]
                                      ,[SndDayLengthType]     
                                  FROM
                                  --------------tables starts
                                   [dbo].[EmployeeWeekOffByDay] d
                                  inner join 
                                  (--1
                                  select max(EffectiveDate) ed,EmpSystemID emp from [EmployeeWeekOffByDay] 
                                  where EmpSystemID in 
                                  (
                                  " + empids + @"
                                  )
                                    and EffectiveDate<='" + sAttnDate + @"'
                                  group by EmpSystemID
                                  )--1 
                                  m on m.ed=d.EffectiveDate and m.emp=d.EmpSystemID
                                ------------tables ends

                                  where EmpSystemID in (
                                   " + empids + @"
                                  )
                                  group by 
                                  EmpSystemID,FixSystemID,AlignWithCC,IndividualWeekOff
                                  ,FstOffDay,FstDayLengthType,SndOffDay,SndDayLengthType";

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

        void _getCompanyAssignWeekOffDateRangeWise(string sGroupID, string sPlantID, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT
                                    --A.* 
                                    OffDayDate,DayLengthType
                                    FROM scs.OffDayDetail A
			                            INNER JOIN (SELECT * FROM scs.OffDayMaster WHERE OffDayType = 'W') B ON A.OffDayMasterId = B.Id
                            WHERE A.OffDayDate = '" + sAttnDate + @"'
                                  AND A.CompanyGroupId = '" + sGroupID + @"' AND A.PlantID = '" + sPlantID + "'";

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
    class EmpShiftInfo
    {
        public string PlantID { get; set; }
        public string SystemID { get; set; }
    }
}

using Library.Model.Biometrics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.HumanResource.Leave
{
   public class clsOffDayList
    {
       public void createOffDayList(string plantid, LeaveTransaction master, List<string> listH, List<string> listW)
        {
            DataSet dsLeavePolicy = null;
            DataSet dsLeavePolicyMaster = null;
            string fromDate = string.Empty;
            string toDate = string.Empty;
            string LVPolicyMasterSystemID = string.Empty;
            try
            {
                List<string> list_W_Total = new List<string>();
                List<string> list_H_Total = new List<string>();
                fromDate = master.FromDate.ToString("dd-MMM-yyyy");
                DateTime _dtTD = Convert.ToDateTime(master.ToDate);
                toDate = _dtTD.ToString("dd-MMM-yyyy");
                //get leave policydetail leavetypeid
                //get H/W list 
                //update master
                //update detail

                _getLeavePolicyMaster(master.EmpSystemID, plantid, out dsLeavePolicyMaster);
                if (dsLeavePolicyMaster.Tables[0].Rows.Count > 0)
                {
                    LVPolicyMasterSystemID = dsLeavePolicyMaster.Tables[0].Rows[0]["LeavePolicyMasterId"].ToString();
                }
                else
                {
                    throw new Exception("Leave policy is not configured...");
                }

                _getLeavePolicy(LVPolicyMasterSystemID, master.LTSystemID, out dsLeavePolicy);

                //if (dsLeavePolicy.Tables[0].Rows.Count > 0)
                //{
                //    if (Convert.ToBoolean(dsLeavePolicy.Tables[0].Rows[0]["InBetweenHoliday"].ToString()))//if false sandwich applicable on Holiday
                //    {
                _getHolidays(master.EmpSystemID, plantid, fromDate, toDate, list_H_Total);
                //}

                //if (Convert.ToBoolean(dsLeavePolicy.Tables[0].Rows[0]["InBetweenWeekoff"].ToString()))//sandwich applicable on Weekoff
                //{
                _getWeekOffdate(master.EmpSystemID, plantid, fromDate, toDate, list_W_Total);
                //    }
                //}

                clsOffDayListGenerate _odl = new clsOffDayListGenerate(plantid, fromDate, toDate, list_W_Total, list_H_Total);
                _odl.GenerateList(listW, listH);
                //get hr setting for H/W
                //DataSet dsHRSetting = null;
                //_getHRSettingForHW_Priority(plantid, out dsHRSetting);

                //DateTime _fd = Convert.ToDateTime(fromDate);
                //DateTime _td = Convert.ToDateTime(toDate);
                //while (_fd < _td)
                //{
                //    if (list_W_Total.Contains(_fd.ToString("dd-MMM-yyyy")) && list_H_Total.Contains(_fd.ToString("dd-MMM-yyyy")))
                //    {
                //        if(dsHRSetting.Tables[0].Rows.Count>0)
                //        {
                //            listH.Add(_fd.ToString("dd-MMM-yyyy"));
                //        }
                //        else
                //        {
                //            listW.Add(_fd.ToString("dd-MMM-yyyy"));
                //        }
                //    }
                //    else
                //    {
                //        //W
                //        if (list_W_Total.Contains(_fd.ToString("dd-MMM-yyyy")))
                //        {
                //            listW.Add(_fd.ToString("dd-MMM-yyyy"));
                //        }
                //        //H
                //        if (list_H_Total.Contains(_fd.ToString("dd-MMM-yyyy")))
                //        {
                //            listH.Add(_fd.ToString("dd-MMM-yyyy"));
                //        }
                //    }
                //    _fd = _fd.AddDays(1);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public void _getLeavePolicyMaster(string empid, string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select c.LeavePolicyMasterId from EmployeeInformation e
                                inner join mst.DesignationMasterLegalDesignation m on e.LegalDesignationId=m.LegalDesignationId
                                inner join scs.DesignationMasterConfiguration c on c.DesignationMasterId=m.DesignationMasterId and c.PlantId='" + plantid + @"'
                                where e.SystemId='" + empid + "'";
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
        public void _getLeavePolicy(string LPMSystemID, string LTSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select isnull(InBetweenHoliday,0) InBetweenHoliday, isnull(InBetweenWeekoff,0) InBetweenWeekoff from LeavePolicyDetail where LPMSystemID='" + LPMSystemID + @"' and LTSystemID='" + LTSystemID + @"' ";
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
        public void _getHolidays(string empsystemid,string plantid, string fromDate, string toDate, List<string> listoffday)
        {
            string strSQL = string.Empty;
            DataSet dsRef = null;
            DataSet dsCompenNW = null;
            DataSet dsCompenH = null;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"		select d.OffDayDate
		                                from scs.OffDayDetail d
		                                inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('H') and m.PlantId='" + plantid + @"'
		                                where d.PlantId='" + plantid + @"'
		                                and d.OffDayDate between '" + fromDate + @"' and '" + toDate + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                //start
                List<string> _listNW = new List<string>();
                List<string> _listH = new List<string>();
                _getCompensatoryH(empsystemid, fromDate, toDate, out dsCompenH);
                _listH = _getList(dsCompenH);
                _getCompensatoryGeneralH(empsystemid, fromDate, toDate, out dsCompenNW);
                _listNW = _getList(dsCompenNW);
                //ds
                //also checking compensatory off day
                //var _isW = _listH.Contains(_fd);
                foreach (var item in _listH)
                {
                    string _fd = Convert.ToDateTime(item).ToString("dd-MMM-yyyy");
                    if (listoffday.Contains(_fd) == false)
                    {
                        listoffday.Add(_fd);
                    }
                }

                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    string _fd = Convert.ToDateTime(dsRef.Tables[0].Rows[i]["OffDayDate"].ToString()).ToString("dd-MMM-yyyy");

                    var _isNW = _listNW.Contains(_fd);
                    if (_isNW == false)//offday (not NW)
                    {
                        if (listoffday.Contains(_fd) == false)
                        {
                            listoffday.Add(_fd);
                        }
                    }
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
        }//End Function
        public void _getWeekoff(string plantid, string fromDate, string toDate, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"		select d.OffDayDate
		                                from scs.OffDayDetail d
		                                inner join scs.OffDayMaster m on d.offdaymasterid=m.id and OffDayType in ('W') and m.PlantId='" + plantid + @"'
		                                where d.PlantId='" + plantid + @"'
		                                and d.OffDayDate between '" + fromDate + @"' and '" + toDate + @"' ";
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
        void _getoffDayIndividual(string empid, string toDate, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {             
                strSQL= @"select d.EffectiveDate,FstOffDay from EmployeeWeekOffByDay d 
                                inner join
                                (
                                select max(EffectiveDate) EffectiveDate, EmpSystemID from EmployeeWeekOffByDay where EmpSystemID = '" + empid + @"'
                                                                    and EffectiveDate<='" + toDate + @"'
                                                                    group by EmpSystemID
									                                ) x on x.EffectiveDate = d.EffectiveDate and x.EmpSystemID = d.EmpSystemID
                                        where IndividualWeekOff=1 ";
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
        void _getCompensatoryWeekoff(string empid,string fromDate, string toDate, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select m.CompensatoryDate WDate from mst.CompensatoryOffEmpList e 
                                    left join mst.CompensatoryOff m on m.id=e.CompensatoryOffId
                                    where e.EmpSystemId='" + empid + @"'
                                    and CompensatoryDate between  '" + fromDate + @"' and '" + toDate + @"' and m.CompensatoryDateTreatmentType='W' ";
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
        void _getCompensatoryH(string empid, string fromDate, string toDate, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select m.CompensatoryDate WDate from mst.CompensatoryOffEmpList e 
                                    left join mst.CompensatoryOff m on m.id=e.CompensatoryOffId
                                    where e.EmpSystemId='" + empid + @"'
                                    and CompensatoryDate between  '" + fromDate + @"' and '" + toDate + @"' and m.CompensatoryDateTreatmentType='H' ";
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
        void _getCompensatoryGeneral(string empid, string fromDate, string toDate, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select m.OriginalDate WDate from mst.CompensatoryOffEmpList e 
                                    left join mst.CompensatoryOff m on m.id=e.CompensatoryOffId
                                    where e.EmpSystemId='"+ empid + @"'
                                    and OriginalDate between '"+fromDate+@"' and '"+toDate+@"' and m.CompensatoryDateTreatmentType='W'";
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
        void _getCompensatoryGeneralH(string empid, string fromDate, string toDate, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select m.OriginalDate WDate from mst.CompensatoryOffEmpList e 
                                    left join mst.CompensatoryOff m on m.id=e.CompensatoryOffId
                                    where e.EmpSystemId='" + empid + @"'
                                    and OriginalDate between '" + fromDate + @"' and '" + toDate + @"' and m.CompensatoryDateTreatmentType='H'";
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
        List<string> _getList(DataSet ds)
        {
            List<string> _list = new List<string>();
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    _list.Add(Convert.ToDateTime(ds.Tables[0].Rows[i]["WDate"].ToString()).ToString("dd-MMM-yyyy"));
                }
                return _list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void _getWeekOffdate(string empid, string plantid, string fromDate, string toDate, List<string> list)
        {
            DataSet dsOffdayIndi = null;
            DataSet dsCompenW = null;
            DataSet dsCompenNW = null;
            DataSet dsWeekOff = null;
            DataSet dsCompenHNW = null;
            try
            {
                List<string> _listNW = new List<string>();
                List<string> _listNW_H = new List<string>();
                List<string> _listW = new List<string>();
                _getCompensatoryWeekoff(empid, fromDate, toDate, out dsCompenW);
                _listW = _getList(dsCompenW);
                _getCompensatoryGeneral(empid, fromDate, toDate, out dsCompenNW);
                _listNW = _getList(dsCompenNW);
                //-------
                _getCompensatoryGeneralH(empid, fromDate, toDate, out dsCompenHNW);
                _listNW_H = _getList(dsCompenHNW);

                _getoffDayIndividual(empid, toDate, out dsOffdayIndi);
                if (dsOffdayIndi.Tables[0].Rows.Count > 0)
                {

                    string offday = dsOffdayIndi.Tables[0].Rows[0]["FstOffDay"].ToString();//e.g. friday

                    DateTime _fd = Convert.ToDateTime(fromDate);
                    DateTime _td = Convert.ToDateTime(toDate);
                    while (_fd < _td)
                    {
                        var _isNW = _listNW.Contains(_fd.ToString("dd-MMM-yyyy"));
                        var _isNWH = _listNW_H.Contains(_fd.ToString("dd-MMM-yyyy"));
                        if (_isNW == false && _isNWH==false)//offday (not NW)
                        {
                            if (_fd.ToString("dddd").ToUpper() == offday.ToUpper())
                            {
                                if (list.Contains(_fd.ToString("dd-MMM-yyyy")) == false)
                                {
                                    list.Add(_fd.ToString("dd-MMM-yyyy"));
                                }
                            }
                        }

                        //also checking compensatory off day
                        var _isW = _listW.Contains(_fd.ToString("dd-MMM-yyyy"));
                        if (_isW)//offday
                        {
                            if (_fd.ToString("dddd").ToUpper() == offday.ToUpper())
                            {
                                if (list.Contains(_fd.ToString("dd-MMM-yyyy")) == false)
                                {
                                    list.Add(_fd.ToString("dd-MMM-yyyy"));
                                }
                            }
                        }

                        _fd = _fd.AddDays(1);
                    }
                }
                else
                {
                    _getWeekoff(plantid, fromDate, toDate, out dsWeekOff);
                   

                    for (int i = 0; i < dsWeekOff.Tables[0].Rows.Count; i++)
                    {
                        string dt = Convert.ToDateTime(dsWeekOff.Tables[0].Rows[i]["OffDayDate"].ToString()).ToString("dd-MMM-yyyy");
                        var _isNWH = _listNW_H.Contains(dt);
                        var _isNW = _listNW.Contains(dt);
                        if (_isNW == false && _isNWH == false)//offday (not NW) //dont add it is now NH
                        {
                            if (list.Contains(dt) == false)
                            {
                                list.Add(dt);
                            }
                        }
                    }

                    foreach (var item in _listW)//include all compensatory offday
                    {
                        string _dt = Convert.ToDateTime(item).ToString("dd-MMM-yyyy");
                        if (list.Contains(_dt) == false)
                        {
                            list.Add(_dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function
    }
}

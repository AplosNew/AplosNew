using Library.Crosscutting.Security;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Leave.LeaveUploadXL
{
    public class clsTemplateSaveLeave
    {
        public void SaveLeave(CustomIdentity identity, List<LeaveUploadTemplate> emplist, out DataSet dsSave_leavemaster, out DataSet dsSave_leavedetail)
        {
            dsSave_leavemaster = null;
            dsSave_leavedetail = null;
            bool _IsDataOk = false;
            string _leaveids = string.Empty;
            try
            {
                string _seed_master = string.Empty;
                string _seed_detail = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
               
                int _count = 0;
                if (emplist.Count() > 0)
                {
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "LEAVE_MASTER_XL", out _seed_master);
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "LEAVE_DETAIL_XL", out _seed_detail);

                    _GetLeaveTransaction(identity.PlantId, out dsSave_leavemaster);
                    _GetLeaveTransactionDetails(out dsSave_leavedetail);
                    foreach (var emp in emplist)
                    {
                        var sl = GetPK(emp.LTSystemID);
                        if (sl == string.Empty)
                        {
                            _IsDataOk = false;
                        }
                        else
                        {
                            _IsDataOk = true;
                        }

                        _IsDataOk = true;
                        //_Validation(dsEmpList.Tables[0].Rows[i], out _IsDataOk);
                        if (_IsDataOk)
                        {
                            _count++;
                            string _masterpk = string.Empty;
                            _LeaveMaster(ref dsSave_leavemaster, emp, identity, _count, _seed_master, out _masterpk);//tbd 
                            if (_masterpk.Length > 0)
                            {
                                _LeaveDetail(ref dsSave_leavedetail, emp.LvDate, identity, _count, _seed_detail, _masterpk);//tbd
                            }
                        }//IsDataOk
                    }//for
                }//count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetPK(string colvalue)
        {
            string r = string.Empty;
            string token = "_#";
            try
            {
                //var k = colvalue;
                if (colvalue != null)
                {
                    var _index = colvalue.IndexOf(token);
                    if (_index != -1)
                    {
                        r = colvalue.Substring(_index + token.Length).Trim().Replace("\n", "").Replace("\r", "");
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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
        void _LeaveMaster(ref DataSet dsSaveLeaveMaster, LeaveUploadTemplate _emp, CustomIdentity _identity, int _count, string _seed, out string _masterpk)
        {
            DataView _dvSave = null;
            _masterpk = string.Empty;
            try
            {
                //string _WorkDate = _emp.FromDate;// drSource["AttdnProcDate"].ToString();
                //string _EmpSystemID = _emp.EmpSystemID;// drSource["EmpSystemID"].ToString();
                //string _LTSystemID = _emp.LTSystemID;// drSource["postLeaveid"].ToString();
                _dvSave = new DataView(dsSaveLeaveMaster.Tables[0]);
                _dvSave.RowFilter = "EmpSystemID='" + _emp.EmpSystemID + "' and LTSystemID='" + GetPK(_emp.LTSystemID) + @"' AND FromDate='" + _emp.LvDate + "' and ToDate='" + _emp.LvDate + @"' ";
                if (_dvSave.Count == 0)
                {
                    _masterpk = "XM"+DateTime.Now.ToString("yy") + _seed + "-" + _count;
                    DataRow _dr = dsSaveLeaveMaster.Tables[0].NewRow();
                    _AddRowLeaveMaster(ref _dr, _identity, _emp, _masterpk);
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
        void _AddRowLeaveMaster(ref DataRow _dr, CustomIdentity _identity, LeaveUploadTemplate emp, string _masterpk)
        {
            try
            {
                _dr["SystemId"] = _masterpk;
                _dr["PlantID"] = _identity.PlantId;
                _dr["EmpSystemID"] = emp.EmpSystemID;
                _dr["LTSystemID"] = GetPK(emp.LTSystemID);
                _dr["FromDate"] = emp.LvDate;
                _dr["ToDate"] = emp.LvDate;
                _dr["LeaveDays"] = 1.00;
                _dr["LeaveDayType"] = "FullDay";
                _dr["IsApproved"] = 1;
                _dr["LvReason"] = emp.LvReason;

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
        void _LeaveDetail(ref DataSet dsSaveLeaveDetail, string _WorkDate, CustomIdentity _identity, int _count, string _seed, string _masterpk)
        {
            DataView _dvSave = null;
            try
            {
                _dvSave = new DataView(dsSaveLeaveDetail.Tables[0]);
                _dvSave.RowFilter = "LvTrnsSystemID='" + _masterpk + "' and workdate='" + _WorkDate + "'";
                if (_dvSave.Count == 0)
                {
                    string pk = "XD"+DateTime.Now.ToString("yy") + _seed + "-" + _count;
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
        void _AddRowLeaveDetail(ref DataRow _dr, CustomIdentity _identity, string _WorkDate, string _masterpk, string pk)
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

        public void SaveData(CustomIdentity identity, List<LeaveUploadTemplate> epList)
        {
            DataSet dsLeaveMaster = null;
            DataSet dsLeaveDetail = null;
            try
            {
                clsStaticInfo objS = new clsStaticInfo();
                foreach (var item in epList)
                {
                   // CheckField(item.EmployeeCode, "EmployeeCode");
                }
                SaveLeave(identity, epList, out dsLeaveMaster, out dsLeaveDetail);
                objS.SaveDataSets(dsLeaveMaster, dsLeaveDetail);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//EOF            
    }
}

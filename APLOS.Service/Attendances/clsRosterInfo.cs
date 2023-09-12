using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Attendances
{
    public class clsRosterInfo
    {
        ISqlRepository isqlrepository = null;
        public clsRosterInfo(ISqlRepository _ISqlRepository)
        {
            isqlrepository = _ISqlRepository;
        }
        public void SaveData(ShiftRosterMaster m, List<ShiftRosterDetail> detail,out string id)
        {
            #region DataSet
            id = string.Empty;
            //DataSet dsGrd = null;
            //DataTable dtGrd = null;
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            DataSet dsIdLast = null;
            DataSet dsSFTDetails = null;
            DataTable dtSFTDetails = null;
            DataRow drSFTDetails = null;
            DataView dvSFTDetails = null;
            DataSet dsPntCmpAsg = null;
            clsStaticInfo objStatic = null;
            bool DATA_OK = false;

            #endregion DataSet

            try
            {
                objStatic = new clsStaticInfo();

                if (DATA_OK == false)
                {
                    #region VALIDATION CHECK

                    if (m.ShiftRosterName == "" || m.ShiftRosterName.Trim().Length > 50)
                    {
                        Exception ex = new Exception("Shift Name cannot be blank and max(50)");
                        throw (ex);
                    }

                    if (m.SystemID != null && m.SystemID.Length > 0)
                    {
                        if (CheckShiftRoster(m.GroupID, m.PlantID, m.SystemID, m.ShiftRosterName) == false)
                        {
                            Exception ex = new Exception("This Roster Name already exist.........'Roster Name' must be unique");
                            throw (ex);
                        }
                    }

                    DATA_OK = true;
                    #endregion
                }
                if (DATA_OK == true)
                {
                    bplib.clsGenID objGenID = null;
                    //string strResCode = null;
                    #region Save Shift Roster
                    // for add new a new auto Shift Roster Id will be added

                    // string strShiftDetailsID = "";

                    if (m.SystemID == null || m.SystemID.Length == 0)
                    {
                        string strShiftID;
                        objGenID = new bplib.clsGenID();
                        objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SHIFT_ROSTER", out strShiftID);
                        //strShiftDetailsID = strShiftID;
                        //strShiftID = "R" + strShiftID;
                        m.SystemID = "R" + strShiftID;
                    }
                    //else
                    //{
                    //    strShiftDetailsID = TextShiftRosterID.Text.ToString().Substring(2);
                    //}

                    //GetShiftRoster(lblGroupID.Text.Trim(), ddlPlant.SelectedValue.ToString().Trim(), TextShiftRosterID.Text.Trim(), out dsLocal);
                    GetShiftRoster(m.GroupID, m.PlantID, m.SystemID, out dsLocal);
                    dtLocal = dsLocal.Tables[0];
                    dvLocal = new DataView();
                    dvLocal.Table = dtLocal;
                    dvLocal.RowFilter = "SystemID = '" + m.SystemID + "'";

                    if (dvLocal.Count == 0)
                    { // Add new block

                        //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);

                        drLocal = dtLocal.NewRow();
                        UpdateTheDataRow("ADDNEW", m, ref drLocal);
                        dtLocal.Rows.Add(drLocal);
                    }
                    else
                    {//edit block
                     //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);

                        drLocal = dvLocal[0].Row;
                        drLocal.BeginEdit();
                        UpdateTheDataRow("EDIT", m, ref drLocal);
                        drLocal.EndEdit();
                    }

                    #endregion

                    #region Save Shift Roster Details
                    // for add new a new auto Shift Roster Details Id will be added
                    GetShiftRosterDetails(m.SystemID, out dsSFTDetails);
                    dtSFTDetails = dsSFTDetails.Tables[0];
                    dvSFTDetails = new DataView();
                    dvSFTDetails.Table = dtSFTDetails;

                    //LoadDataSetFromDataGrid(ref dgShiftRosterDetail, out dsGrd);

                    int _Count = 0;

                    string _detailid;
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "ROSTER_DETAIL", out _detailid);
                    //m.SystemID = "RD" + _detailid;
                    for (int i = dsSFTDetails.Tables[0].Rows.Count - 1; i >= 0; i--)
                    {
                        string _shiftid = dsSFTDetails.Tables[0].Rows[i]["ShiftDefinationID"].ToString();
                        string _SystemId = dsSFTDetails.Tables[0].Rows[i]["SystemId"].ToString();
                        bool IsFound = false;
                        foreach (var item in detail)
                        {
                            if(item.ShiftDefinationID== _shiftid)
                            {
                                IsFound = true;
                                break;
                            }
                        }//for item
                        if(IsFound==false)
                        {
                            DataView dv = new DataView(dsSFTDetails.Tables[0]);
                            dv.RowFilter = "SystemId = '" + _SystemId + "'";
                            if (dv.Count > 0)
                            {
                                DataRow dr = dv[0].Row;
                                dr.BeginEdit();
                                dr.Delete();
                                dr.EndEdit();
                            }
                        }

                    }//for del
                    
                    foreach (var d in detail)
                    {
                        _Count++;
                        dvSFTDetails.RowFilter = "ShiftDefinationID = '" + d.ShiftDefinationID + "'";
                        if (dvSFTDetails.Count == 0)
                        { // Add new block                            
                            drSFTDetails = dtSFTDetails.NewRow();
                            drSFTDetails["SystemID"] = "RD" + _detailid + "-" + _Count;
                            drSFTDetails["SRMasterSystemID"] = m.SystemID;
                            drSFTDetails["AddedBy"] = m.AddedBy;
                            drSFTDetails["DateAdded"] = DateTime.Now;
                            drSFTDetails["ShiftDefinationID"] = d.ShiftDefinationID;// d.ShiftDefinationID;
                            //drSFTDetails["ShiftSequence"] = _Count;
                            drSFTDetails["ShiftSequence"] = d.ShiftSequence;
                            drSFTDetails["PlantID"] = m.PlantID;
                            //drSFTDetails["EffectiveDate"] = m.EffectiveDate;

                            drSFTDetails["GroupID"] = m.GroupID;
                            drSFTDetails["UpdatedBy"] = m.UpdatedBy;
                            drSFTDetails["DateUpdated"] = DateTime.Now;
                            dtSFTDetails.Rows.Add(drSFTDetails);
                        }
                        else
                        {//edit block
                            drSFTDetails = dvSFTDetails[0].Row;
                            drSFTDetails.BeginEdit();
                            drSFTDetails["ShiftDefinationID"] = d.ShiftDefinationID;

                            //drSFTDetails["ShiftSequence"] = _Count;
                            drSFTDetails["ShiftSequence"] = d.ShiftSequence;
                            drSFTDetails["UpdatedBy"] = m.UpdatedBy;
                            drSFTDetails["DateUpdated"] = DateTime.Now;
                            drSFTDetails.EndEdit();
                        }
                    }
                    #endregion

                    objStatic.SaveDataSets(dsLocal, dsSFTDetails);
                    id = m.SystemID;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                drLocal = null;
                dvLocal = null;
                dtLocal = null;
                dsLocal = null;
            }

        }///End Function
        public void GetShiftRosterDetails(string strAGID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "Select * from ShiftRosterChild where SRMasterSystemID = '" + strAGID + "'";
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
        }//End of function
        public bool CheckShiftRoster(string sGroupID, string sPlantID, string strShiftID, string strShiftName)
        {
            DataSet dsRef = null;
            string strSQl;
            bool blnStatus = false;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQl = "SELECT *  FROM ShiftRosterMaster WHERE SystemID != '" + strShiftID + "' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' AND ShiftRosterName='" + strShiftName + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End of function
        public void GetShiftRoster(string sGroupID, string sPlantID, string strAGID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM ShiftRosterMaster WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' AND SystemID='" + strAGID + "'";
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
        }//End of function

        private void UpdateTheDataRow(string OPN_FLAG, ShiftRosterMaster m, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = m.SystemID;
                    drLocal["AddedBy"] = m.AddedBy;
                    drLocal["DateAdded"] = DateTime.Now;
                }
                drLocal["ShiftRosterName"] = m.ShiftRosterName;
                drLocal["ShiftRosterDescription"] = m.ShiftRosterDescription;               
                drLocal["ChangeAfterDayLength"] = m.ChangeAfterDayLength;

                if (m.EffectiveDate != null)
                {
                    drLocal["EffectiveDate"] = m.EffectiveDate;
                }
                else
                {
                    drLocal["EffectiveDate"] = DBNull.Value;
                }

                // drLocal["EffectiveDate"] = m.EffectiveDate != string.isNullOrEmpty;

                drLocal["RosteringPattern"] = m.RosteringPattern;
                drLocal["WeekDays"] = m.WeekDays;
                drLocal["MultiDate"] = m.MultiDate;
                drLocal["PlantID"] = m.PlantID;
                drLocal["GroupID"] = m.GroupID;
                drLocal["UpdatedBy"] = m.AddedBy;
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        public List<Dictionary<string, object>> ShiftDefinationSearch(string sGroupID, string sPlantID)
        {
            //ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {                
                    strSql = @"SELECT SystemID ShiftDefinationID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo ShiftSequence, CONVERT(VARCHAR(10), InTime, 108) AS InTime,
                                        InTimeStartMargin, LateMargin, AbsentEndMargin, CONVERT(VARCHAR(10), OutTime, 108) AS OutTime,
                                        OutTimeEndMargin, OTStartTime, CONVERT(VARCHAR(10), BreakStratTime, 108) AS BreakStratTime,
                                        CONVERT(VARCHAR(10), BreakEndTime, 108) AS BreakEndTime, BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                                FROM ShiftDefination WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' Order By ShiftDefinationName";
                
               return isqlrepository.GetDataCollection(strSql);                
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//end of function

 public List<Dictionary<string, object>> RosterLoad(string sPlantID)
        {
            string strSql = "";
            try
            {             
                    strSql = @"select 
                                m.SystemId ,m.ShiftRosterName,m.ShiftRosterDescription,m.ChangeAfterDayLength
                                ,m.RosteringPattern,m.WeekDays,m.MultiDate,Format(m.EffectiveDate,'dd-MMM-yyyy')as EffectiveDate
                                from ShiftRosterMaster m
                                where m.PlantID='" + sPlantID + "'";                
               return isqlrepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//end of function
        public List<Dictionary<string, object>> RosterChildLoad(string RosterId, string sPlantID)
        {
            //ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                //strSql = @"SELECT SystemID ShiftDefinationID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo ShiftSequence, CONVERT(VARCHAR(10), InTime, 108) AS InTime,
                //                        InTimeStartMargin, LateMargin, AbsentEndMargin, CONVERT(VARCHAR(10), OutTime, 108) AS OutTime,
                //                        OutTimeEndMargin, OTStartTime, CONVERT(VARCHAR(10), BreakStratTime, 108) AS BreakStratTime,
                //                        CONVERT(VARCHAR(10), BreakEndTime, 108) AS BreakEndTime, BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                //                FROM ShiftDefination WHERE PlantID = '" + sPlantID + @"' and systemid in (select ShiftDefinationID from ShiftRosterChild where SRMasterSystemID='" + RosterId + @"') 
                //                Order By ShiftDefinationName";
                strSql = @"SELECT ShiftDefination.SystemID ShiftDefinationID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, C.ShiftSequence ShiftSequence, CONVERT(VARCHAR(10), InTime, 108) AS InTime,
                             InTimeStartMargin, LateMargin, AbsentEndMargin, CONVERT(VARCHAR(10), OutTime, 108) AS OutTime,
                             OutTimeEndMargin, OTStartTime, CONVERT(VARCHAR(10), BreakStratTime, 108) AS BreakStratTime,
                             CONVERT(VARCHAR(10), BreakEndTime, 108) AS BreakEndTime, BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                     FROM ShiftDefination
                     JOIN dbo.ShiftRosterChild C ON C.ShiftDefinationID=ShiftDefination.SystemID AND SRMasterSystemID='"+ RosterId + @"'
                      WHERE ShiftDefination.PlantID = '"+sPlantID+@"' and ShiftDefination.systemid in (select ShiftDefinationID from ShiftRosterChild where SRMasterSystemID='"+RosterId+ @"') 
                     Order By C.ShiftSequence,ShiftDefinationName";

                return isqlrepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//end of function

        public List<Dictionary<string, object>> SelectedShiftDefination(string strSystemID)
        {
            string strSql = "";
            try
            {
                strSql = @"SELECT SystemID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo, CONVERT(VARCHAR(10), InTime, 108) AS InTime,
                                        InTimeStartMargin, LateMargin, AbsentEndMargin, CONVERT(VARCHAR(10), OutTime, 108) AS OutTime,
                                        OutTimeEndMargin, OTStartTime, CONVERT(VARCHAR(10), BreakStratTime, 108) AS BreakStratTime,
                                        CONVERT(VARCHAR(10), BreakEndTime, 108) AS BreakEndTime, BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                                      ,EarlyIn, LateIn,LateOut,  EarlyOut
                                        , EarlyOutMargin,EarlyInMargin,LateInMargin,LateOutMargin
                                        ,LateOutRoundMargin
                                        ,LateInRoundMargin
                                        ,EarlyOutRoundMargin
                                        ,EarlyInRoundMargin
                                        ,LateOutRoundMarginType
                                        ,LateInRoundMarginType
                                        ,EarlyOutRoundMarginType
                                        ,EarlyInRoundMarginType
                                        ,IncludeBreakTimeInOT
                                        ,ShortLeaveMaxLimit
                                        ,HalfDayAbsentMaxLimit
                              FROM ShiftDefination
                                WHERE  SystemID = '" + strSystemID + @"'
                                ORDER BY ShiftDefinationName";
                return isqlrepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//end of function
    }
    public class ShiftRosterMaster
    {
        public string SystemID { get; set; }
        public string ShiftRosterName { get; set; }
        public string ShiftRosterDescription { get; set; }
        public string WeekDays { get; set; }
        public string RosteringPattern { get; set; }
        public string MultiDate { get; set; }
        public int ChangeAfterDayLength { get; set; }
        public string PlantID { get; set; }
        public string GroupID { get; set; }
        public string UpdatedBy { get; set; }
        public string AddedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateAdded { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }
    public class ShiftRosterDetail
    {
        public string SystemID { get; set; }
        public string SRMasterSystemID { get; set; }
        public string ShiftDefinationID { get; set; }
        public string ShiftSequence { get; set; }
    }
}

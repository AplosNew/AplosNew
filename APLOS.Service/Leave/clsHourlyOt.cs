using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Service.Leave
{
   public class clsHourlyOt
    {
        ISqlRepository _sqlRepository;
        private DataSet dsRef;

        public clsHourlyOt(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        public clsHourlyOt()
        {

        }
        void GetDetail(string empid, string leavetransactionid, string userid, string ip,out IEnumerable<LeavePolicyMaster> dList)
        {
            dList = null;
            try
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SetRowValue(ref DataRow dr,string Field,object v)
        {
            try
            {
                if(v is null)
                {
                    dr[Field] = DBNull.Value;
                }
                else
                {
                    dr[Field] = v;
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SetRowValue(ref DataRow dr, object v)
        {
            try
            {
                dr[nameof(v)] = v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Off Duty Hours

        public void SaveDutyHour(HourlyOt HourlyOt)
        {
            DataSet dsMaster = null;

            try
            {
                SaveDutyHourMasters(HourlyOt, out dsMaster);

                clsStaticInfo obj = new clsStaticInfo();

                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        void SaveDutyHourMasters(HourlyOt DutyHour, out DataSet dsMaster)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();
                obj.LockValidation(identity.PlantId, DutyHour.FromDate.ToString("dd-MMM-yyyy"), DutyHour.ToDate.ToString("dd-MMM-yyyy"), DutyHour.EmpSystemId);
                
                DutyHourMaster(DutyHour.Id, out dsMaster);
                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + DutyHour.Id + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                   
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HourlyOT", out sID);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    DutyHour.Id = "HO" + sID;
                    foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
                    }
                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
                    }
                    dr.EndEdit();
                    #endregion
                }
                dvMaster.RowFilter = null;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public List<Dictionary<string, object>> GetShiftInfo(string EmpSystemID, string WorkDate)
        {
            try
            {
                string wd = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy");

                var cmdText = @"   select ES.EmpSystemID,S.UserName,es.ShiftSystemID                            
                        ,Format(s.InTime,'dd-MMM-yyyy hh:mm tt')InTime,format(s.OutTime,'dd-MMM-yyyy hh:mm tt')OutTime
                                    ,ES.DayType                             
                               from [dbo].[EmpDateWiseShiftAssign] ES
                               left join ShiftDefination s on s.SystemID=es.ShiftSystemID 							                       
                               WHERE es.EmpSystemID='" + EmpSystemID + "' and es.WorkDate='" + wd + "'  ";

                return _sqlRepository.GetDataCollection(cmdText);

            }
            catch (Exception)
            {
                throw;
            }
        }//end of function

        void DutyHourMaster(string Id, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM HourlyOT where ID='" + Id + @"' ";

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


        #endregion

    }
    public class HourlyOt
    {   
         public string Id { get; set; }
         public string EmpSystemId { get; set; }
         public DateTime FromDate { get; set; }
         public DateTime ToDate { get; set; }
         public int Duration { get; set; }      
         public string PlantId { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
        public string OTType { get; set; }


        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime WorkDate { get; set; }
    }
  
}

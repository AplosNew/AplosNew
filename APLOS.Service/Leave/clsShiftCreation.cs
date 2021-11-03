using Library.Core;
using Library.Crosscutting.Security;
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
    public class clsShiftCreation
    {
        ISqlRepository _sqlRepository;
        public clsShiftCreation(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        void SetRowValue(ref DataRow dr, string Field, object v)
        {
            try
            {
                if (v is null)
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

        #region PolicyMaster

        void SaveShiftCreation(ShiftCreationMaster ShiftCreationData, out DataSet dsMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                DateTime dtBkSt = Convert.ToDateTime(ShiftCreationData.BreakStratTime);
                DateTime dtBkEd = Convert.ToDateTime(ShiftCreationData.BreakEndTime);

                DateTime dtInT = Convert.ToDateTime(ShiftCreationData.InTime);
                DateTime dOutT = Convert.ToDateTime(ShiftCreationData.OutTime);

                int minBk = 0;
                if ((ShiftCreationData.BreakEndTime.ToString() != "00:00:00") & ShiftCreationData.BreakStratTime.ToString() != "00:00:00")
                {
                    if (ShiftCreationData.ShiftType == "Night Shift" & dtBkEd < dtBkSt)
                    {
                        dtBkEd = dtBkEd.AddDays(1);
                        ShiftCreationData.BreakEndTime = dtBkEd;
                    }
                    TimeSpan tsBk = dtBkEd - dtBkSt;
                    minBk = Convert.ToInt32(tsBk.TotalMinutes);
                    ShiftCreationData.BreakPeriod = minBk;
                }
                if (minBk > 0)
                {
                    if (string.IsNullOrEmpty(ShiftCreationData.BreakPeriod.ToString()) == false & bplib.clsWebLib.IsNumeric(ShiftCreationData.BreakPeriod.ToString()) == false)
                    {
                        
                        Exception ex = new Exception("Invalid / Blank Data not allowed for Break Period. \n Please Enter Numeric data Only");
                        throw (ex);
                    }

                    if (ShiftCreationData.ShiftType == "Day Shift")
                    {
                        if (dtBkSt <= dtInT)
                        {
                            
                            Exception ex = new Exception("Define Break Start time cannot be less than or equal IN Time...");
                            throw (ex);
                        }
                        if (dtBkEd <= dtInT)
                        {
                            
                            Exception ex = new Exception("Define Break End time cannot be less than or equal IN Time...");
                            throw (ex);
                        }
                        if (dtBkSt >= dtBkEd)
                        {
                           
                            Exception ex = new Exception("Define Break Start time cannot be more than or equal Break End time...");
                            throw (ex);
                        }
                        if (dtBkEd >= dOutT)
                        {
                           
                            Exception ex = new Exception("Define Break End time cannot be more than or equal OUT Time...");
                            throw (ex);
                        }
                    }
                }
                if (ShiftCreationData.ShiftType == "Day Shift")
                {
                    if (dtInT >= dOutT)
                    {                      
                        Exception ex = new Exception("Define IN Time cannot be more than or equal OUT Time...");
                        throw (ex);
                    }
                }
                if (ShiftCreationData.ShiftType == "Night Shift" && dtInT < dOutT)
                {
                    Exception ex = new Exception("For Night Shift IN Time can't be less than or equal OUT Time...");
                    throw (ex);
                }
                if (ShiftCreationData.ShiftType == "Night Shift")
                {
                    //date same ok
                    dOutT = dOutT.AddDays(1);
                    ShiftCreationData.OutTime = dOutT;
                }
                
                int minWrkTm = 0;
                TimeSpan tsWrk = dOutT - dtInT;
                if (ShiftCreationData.IncludeBreakTimeInOT)
                {
                    minWrkTm = Convert.ToInt32(tsWrk.TotalMinutes);
                }
                else
                {
                    minWrkTm = Math.Abs(Convert.ToInt32(tsWrk.TotalMinutes) - minBk);
                    //int NewDuration = Math.Abs(minWrkTm);
                }
                ShiftCreationData.WorkingHour = minWrkTm;

                if (bplib.clsWebLib.IsNumeric(ShiftCreationData.WorkingHour.ToString()) == true)
                {
                    if (Convert.ToDecimal(ShiftCreationData.WorkingHour.ToString()) > Convert.ToDecimal("1440"))
                    {
                        Exception ex = new Exception("Working minutes is not allow more then 1440");
                        throw (ex);
                    }
                }

                if (ShiftCreationData.DefaultShift == true)
                {
                    var DefaultShift = CheckDefaultShift(ShiftCreationData.SystemID);
                    if (DefaultShift.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Shift Name:- '" + ShiftCreationData.ShiftDefinationName + ",' already define. Unable to define more than one default");
                    }
                }

                var SameShift = CheckSameShift(ShiftCreationData.SystemID, ShiftCreationData.ShiftDefinationName);
                if (SameShift.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("This Shift already exist.........Shift must be unique");
                }

                LeavePolicyMaster(ShiftCreationData.SystemID, out dsMaster);

                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "SystemID='" + ShiftCreationData.SystemID + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ShiftDefination", out sID);
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    ShiftCreationData.SystemID = "SF"+ sID;
                    ShiftCreationData.AddedBy = identity.Name;
                    ShiftCreationData.DateAdded = DateTime.Now;
                    foreach (PropertyInfo prop in ShiftCreationData.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(ShiftCreationData, null));
                    }

                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in ShiftCreationData.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(ShiftCreationData, null));
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
        #endregion

        void LeavePolicyMaster(string SystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM ShiftDefination where SystemID='" + SystemID + @"'";

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

        void Query(string SystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[LeavePolicyMaster] where SystemID='" + SystemID + @"'";

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
        }

        public void SaveShiftCreationMaster(ShiftCreationMaster ShiftCreationData)
        {
            DataSet dsMaster = null;

            try
            {
                SaveShiftCreation(ShiftCreationData, out dsMaster);
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private DataSet CheckDefaultShift(string SystemID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"   SELECT * FROM ShiftDefination
                                WHERE GroupID = '" + identity.CompanyGroupId + @"' AND PlantID = '" + identity.PlantId + @"' AND
                                SystemID != '" + SystemID + @"' AND IsActive = 1
                                AND DefaultShift = 1 "
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }
        private DataSet CheckSameShift(string SystemID, string ShiftDefinationName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"   SELECT * FROM ShiftDefination WHERE SystemID != '" + SystemID + "' AND GroupID = '" + identity.CompanyGroupId + @"' AND PlantID = '" + identity.PlantId + @"' AND ShiftDefinationName='" + ShiftDefinationName + "' "
            };
            return _sqlRepository.GetGridData(parameters).Source;

        }

    }

    public class ShiftCreationMaster
    {
        public string SystemID { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public string ShiftDefinationName { get; set; }
        public string ShiftDefinationDescription { get; set; }
        public string UserName { get; set; }
        public int SequenceNo { get; set; }
        public bool IsActive { get; set; }
        public bool DefaultShift { get; set; }
        public string ShiftType { get; set; }
        public DateTime InTime { get; set; }
        public int InTimeStartMargin { get; set; }
        public int LateMargin { get; set; }
        public int AbsentEndMargin { get; set; }
        public DateTime OutTime { get; set; }
        public int OutTimeEndMargin { get; set; }
        public int OTStartTime { get; set; }
        public bool IsGapInclude { get; set; }
        public DateTime BreakStratTime { get; set; }
        public DateTime BreakEndTime { get; set; }
        public int BreakPeriod { get; set; }
        public decimal WorkingHour { get; set; }
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime? DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool EarlyIn { get; set; }
        public bool LateIn { get; set; }
        public int LateInMargin { get; set; }
        public int LateMarginSeconds { get; set; }
        public bool EarlyOut { get; set; }
        public int EarlyOutMargin { get; set; }
        public int EarlyInMargin { get; set; }
        public int LateOutMargin { get; set; }
        public bool LateOut { get; set; }
        public int LateOutRoundMargin { get; set; }
        public int LateInRoundMargin { get; set; }
        public int EarlyOutRoundMargin { get; set; }
        public int EarlyInRoundMargin { get; set; }
        public string LateOutRoundMarginType { get; set; }
        public string LateInRoundMarginType { get; set; }
        public string EarlyOutRoundMarginType { get; set; }
        public string EarlyInRoundMarginType { get; set; }
        public bool IncludeBreakTimeInOT { get; set; }
        public bool IsLunchOutApplicable { get; set; }
        public bool IsEarlyOutApplicable { get; set; }
        public string EarlyOutMaxLimit { get; set; }
        
        public decimal HalfDayAbsentMaxLimit { get; set; }
        public int LateInMaxLimit { get; set; }
        public int EarlyOutToleranceMargin { get; set; }
        public int LateInToleranceMargin { get; set; }
        public bool IsLateInApplicable { get; set; }
        public int RawINDefinitionFrom { get; set; }
        public int RawINDefinitionTo { get; set; }
        public int RawOUTDefinitionFrom { get; set; }
        public int RawOUTDefinitionTo { get; set; }
        public bool INAfterOUTAsOTStart { get; set; }
        public decimal FullDayDuration { get; set; }
        public decimal HalfDayDuration { get; set; }
        public decimal ShortDuration { get; set; }
        public decimal MaxOutDuration { get; set; }
        public decimal ShiftDuration { get; set; }
        public decimal ShiftEarlyInMargin { get; set; }
        public decimal ShiftEarlyOutMargin { get; set; }
        public decimal ShiftLateInMargin { get; set; }
        public decimal ShiftLateOutMargin { get; set; }
        public decimal HoursWithoutOT { get; set; }

    }



}

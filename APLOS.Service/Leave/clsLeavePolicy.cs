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
   public class clsLeavePolicy
    {
        ISqlRepository _sqlRepository;
        public clsLeavePolicy(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        public clsLeavePolicy()
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
        void MaternityBenefitMaster(string pk, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[MaternityBenefitMaster] WHERE id  = '" + pk + @"'";

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
        void MaternityBenefitDetail(string MaternityBenefitMasterId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                                FROM [dbo].[MaternityBenefitDetail] WHERE MaternityBenefitMasterId  = '" + MaternityBenefitMasterId + @"'";

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

        #region PolicyMaster
        public string SaveMasterAndDetailForLeavePolicy(LeavePolicyMaster _LeavePolicyMaster)
        {
            DataSet dsMaster = null;
            string MasterId = string.Empty;
            try
            {
                SaveMaternityBenefitMasterForAfter(_LeavePolicyMaster, out dsMaster);
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                if (dsMaster.Tables[0].Rows.Count>0)
                {
                    MasterId = dsMaster.Tables[0].Rows[0]["SystemID"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return MasterId;
        }
        void SaveMaternityBenefitMasterForAfter(LeavePolicyMaster _LeavePolicyMaster, out DataSet dsMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
              
                LeavePolicyMaster(_LeavePolicyMaster.SystemID, out dsMaster);

                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "SystemID='" + _LeavePolicyMaster.SystemID + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "LeavePolicyMaster", out sID);
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    _LeavePolicyMaster.SystemID = "LPM" + identity.PlantId + sID;
                    foreach (PropertyInfo prop in _LeavePolicyMaster.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(_LeavePolicyMaster, null));
                    }

                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in _LeavePolicyMaster.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(_LeavePolicyMaster, null));
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

        #region Policy Details

        public string SaveDetailForLeavePolicy(LeavePolicyDetails _LeavePolicyDetails,string MasterId,string Id,List<LeavePolicyDayType>  _DayTypeLeavePolicy)
        {
            DataSet dsMaster = null;
            DataSet dsDayType = null;
             Id = string.Empty;

            try
            {
                var LeaveType = CheckLeaveType(_LeavePolicyDetails.SystemID, _LeavePolicyDetails.LPMSystemID, _LeavePolicyDetails.LTSystemID);
                if (LeaveType.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Leave Type already exist.");
                }

                SaveDetailPolicy(_LeavePolicyDetails, MasterId, Id,out dsMaster);
                clsStaticInfo obj = new clsStaticInfo();
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    Id = dsMaster.Tables[0].Rows[0]["SystemID"].ToString();
                }

                SaveDetailPolicyDayType(_DayTypeLeavePolicy, MasterId, Id, out dsDayType);
                obj.SaveDataSets(dsMaster, dsDayType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Id;
        }

        void SaveDetailPolicy(LeavePolicyDetails _LeavePolicyDetails,string MasterId, string Id,out DataSet dsMaster)
        {
            _LeavePolicyDetails.LPMSystemID = MasterId;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                LeavePolicyDetails(_LeavePolicyDetails.SystemID, out dsMaster);

                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "SystemID='" + _LeavePolicyDetails.SystemID + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "LeavePolicyDetail", out sID);
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    _LeavePolicyDetails.SystemID = "LPD" + identity.PlantId + sID;
                    foreach (PropertyInfo prop in _LeavePolicyDetails.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(_LeavePolicyDetails, null));
                    }

                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in _LeavePolicyDetails.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(_LeavePolicyDetails, null));
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

        void SaveDetailPolicyDayType( List<LeavePolicyDayType> _DayTypeLeavePolicy,string MasterId,string Id, out DataSet dsMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            dsMaster = null;
            try
            {
                LeavePolicyDayTypeDelete(MasterId, Id, out dsMaster);
                LeavePolicyDayType(MasterId, Id, out dsMaster);
                if (_DayTypeLeavePolicy != null)
                {
                    for (int i = 0; i < _DayTypeLeavePolicy.Count; i++)
                    {
                        _DayTypeLeavePolicy[i].LPMasterID = MasterId;
                        _DayTypeLeavePolicy[i].LPDetailID = Id;
                        
                        DataView dvMaster = new DataView(dsMaster.Tables[0]);
                        dvMaster.RowFilter = "Id='" + _DayTypeLeavePolicy[i].Id + "' ";
                        if (dvMaster.Count == 0)
                        {
                            #region add
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "LeavePolicyDetail", out sID);
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            _DayTypeLeavePolicy[i].Id = "LPWD" + identity.PlantId + sID;
                            foreach (PropertyInfo prop in _DayTypeLeavePolicy[i].GetType().GetProperties())
                            {
                                SetRowValue(ref dr, prop.Name, prop.GetValue(_DayTypeLeavePolicy[i], null));
                            }

                            dsMaster.Tables[0].Rows.Add(dr);
                            #endregion
                        }
                        else
                        {
                            #region edit

                            DataRow dr = dvMaster[0].Row;
                            dr.BeginEdit();

                            foreach (PropertyInfo prop in _DayTypeLeavePolicy[i].GetType().GetProperties())
                            {
                                SetRowValue(ref dr, prop.Name, prop.GetValue(_DayTypeLeavePolicy[i], null));
                            }
                            dr.EndEdit();
                            #endregion
                        }
                        dvMaster.RowFilter = null;
                    }
                }
                
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private DataSet CheckLeaveType(string SystemID, string LPMSystemID, string LTSystemID)
        {
            
            DataSet dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" select LTSystemID From LeavePolicyDetail WHERE LTSystemID='" + LTSystemID + "' and LPMSystemID='" + LPMSystemID + "' AND SystemID<>'" + SystemID + "'";

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

            return dsRef;
        }

        #endregion

        void LeavePolicyMaster(string SystemID, out System.Data.DataSet dsRef)
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
        }//End Function

        void LeavePolicyDetails(string SystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[LeavePolicyDetail] where SystemID='" + SystemID + @"' ";

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
        
        void LeavePolicyDayType(string LPMasterID, string LPDetailID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[LeavePolicyWorkingDays] where LPMasterID='" + LPMasterID + @"' AND LPDetailID='" + LPDetailID + @"' ";

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

        void LeavePolicyDayTypeDelete(string LPMasterID, string LPDetailID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"delete FROM [dbo].[LeavePolicyWorkingDays] where LPMasterID='" + LPMasterID + @"' AND LPDetailID='" + LPDetailID + @"' ";

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

    }

    public class LeavePolicyMaster
    {   
         public string SystemID { get; set; }
         public string PolicyCode { get; set; }
         public string PolicyName { get; set; }
         public string EarnedLeaveSystemID { get; set; }
         public string MaternityLeaveSystemID { get; set; }
         public bool DefaultPolicy { get; set; }

         public string GroupID { get; set; }
         public string PlantID { get; set; }   
        
         public string AddedBy { get; set; }
         [NeverUpdate]
         public DateTime? DateAdded { get; set; }
         public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }     
    }

    public class LeavePolicyDetails
    {
        public string SystemID { get; set; }
        public string LPMSystemID { get; set; }
        public string LTSystemID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsWithoutPay { get; set; }
        public int LeaveDays { get; set; }
        public bool IsCarryForward { get; set; }
        public int CarryForwardDay { get; set; }
        public bool IsMaxAllocation { get; set; }
        public int MaxAllocationLimit { get; set; }
        public int MinAllocationLimit { get; set; }

        public string CalculationBasis { get; set; }
        public string LvAvailedOnFixedOrPercentage { get; set; }
        public decimal LvCanAvailQuantity { get; set; }
        public string LeaveCredit { get; set; }
        public bool IsExcessAllow { get; set; }
        public bool IsPrecedingWeekoff { get; set; }
        public bool IsPrecedingHoliday { get; set; }
        public bool IsSucceedignWeekoff { get; set; }
        public bool IsSucceedignHoliday { get; set; }
        public bool InBetweenWeekoff { get; set; }
        public bool IsAsperEntryOnW { get; set; }
        public bool IsNoLeaveOnW { get; set; }
        public bool InBetweenHoliday { get; set; }
        public bool IsAsperEntryOnH { get; set; }
        public bool IsNoLeaveOnH { get; set; }
        public bool IsProrataMonthly { get; set; }
        public int LeaveInHourDaily { get; set; }

        public bool IsActive { get; set; }

        public string GroupID { get; set; }
        public bool EncasementEndDate { get; set; }
        public string PlantID { get; set; }

        public string AddedBy { get; set; }

        public DateTime DateAdded { get; set; }

        public string UpdatedBy { get; set; }
        public string EncashmentSpecificDay { get; set; }
        public string EncashmentSpecificMonth { get; set; }
        public DateTime? DateUpdated { get; set; }

        //public DateTime? EncashmentDate { get; set; }

        //public bool IsCarryForwardCumulative { get; set; }

        public string LeaveCalculationRoundOption { get; set; }

        public bool IsMaxEncashment { get; set; }
        public int MaxEncashment { get; set; }

        public bool IsMaxEncashmentLapse { get; set; }

        public int MaxEncashmentLapse { get; set; }

        public bool IsAllowed { get; set; }
        public bool IsAllowedonspecialappeal { get; set; }
        public bool IsProratacurrentyear { get; set; }
        //public bool IsProrataPreviousyear { get; set; }
        public bool IsNewlyJoined { get; set; }
        public int NewlyJoined { get; set; }
        public int EncashWorkingDaysQty { get; set; }
        public int EncashEarnLeaveQty { get; set; }

        public bool IsSubmittoApproval { get; set; }
        public bool IsAvailPreviousYearProRata { get; set; }
        public bool IsAvailCurrentYearProRata { get; set; }
        public bool IsAvailExceptionAllowedOnSpecialAppeal { get; set; }

        public int AllowedAfterDays { get; set; }
        public bool IsPostApplicationAllowed { get; set; }
        public bool IsExceptionAllowed { get; set; }
        public bool IsSubjectToApproval { get; set; }
        public bool IsProofDocRequired { get; set; }

        public int ProofDocReqAfterDays { get; set; }

        public bool LvCalculationOnDOJ { get; set; }
        public bool LvCalculationOnDOC { get; set; }
        public bool LvAvailedOnDOJ { get; set; }
        public bool LvAvailedOnDOC { get; set; }
        public int LvCanAvailAfter { get; set; }
        public string CanAvailUOM { get; set; }
        public bool IsCFFixed { get; set; }
        public bool IsCFRestFixed { get; set; }
        public bool IsCFCRestFixed { get; set; }
        public bool IsCFRestEncash { get; set; }
        public bool IsCFCRestEncash { get; set; }
        public string CarryForwardRoundupOption { get; set; }
        public string EncashmentBasis { get; set; } 
        public string LvEncashmentFormulaDesID { get; set; }
        public string FormulaDescription { get; set; }
        public bool IsBackDatePosting { get; set; }
        public int BackDatePostingAllowedDays { get; set; }
        public string EmpCatId { get; set; }
    }

    public class LeavePolicyDayType
    {
        public string Id { get; set; }
        public string LPMasterID { get; set; }
        public string LPDetailID { get; set; }
        public string DayType { get; set; }
       
    }

}

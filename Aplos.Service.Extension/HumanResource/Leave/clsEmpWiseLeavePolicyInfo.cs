using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.HumanResource.Leave
{
   public class clsEmpWiseLeavePolicyInfo
    {
        string _plantId = string.Empty;
        public clsEmpWiseLeavePolicyInfo(string _plantId)
        {
            this._plantId = _plantId;
        }
       public PolicySandwichVM _getSandwichInfo(string empSystemid,string leaveTypeId)
        {
            DataSet dsLeavePolicyMaster = null;
            DataSet dsSandwich = null;
            string leavepolicymasterId = string.Empty;
            try
            {
              var  _odl = new clsOffDayList();
               PolicySandwichVM _r = new PolicySandwichVM();

                _odl._getLeavePolicyMaster(empSystemid, this._plantId, out dsLeavePolicyMaster);
                if (dsLeavePolicyMaster.Tables[0].Rows.Count > 0)
                {
                    leavepolicymasterId = dsLeavePolicyMaster.Tables[0].Rows[0]["LeavePolicyMasterId"].ToString();
                }
                else
                {
                    throw new Exception("Leave policy is not configured...");
                }
               _getSandwichInfo(leavepolicymasterId, leaveTypeId,out dsSandwich);
                if(dsSandwich.Tables[0].Rows.Count>0)
                {
                    _r = dsSandwich.Tables[0].ToList<PolicySandwichVM>()[0];
                }
                return _r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetLeaveCount(string empSystemid, string leaveTypeId,int HCount,int WCount,ref decimal leaveDays,out PolicySandwichVM _policyVM)
        {
            try
            {
                 _policyVM = _getSandwichInfo(empSystemid, leaveTypeId);
                if (WCount > 0)//77
                {
                    if (_policyVM.IsAsperEntryOnW)
                    {
                        
                    }
                    else if (_policyVM.IsNoLeaveOnW)
                    {
                        leaveDays = leaveDays - WCount;
                    }
                    else if (_policyVM.InBetweenWeekoff)
                    {
                        leaveDays = leaveDays - WCount;
                    }
                }//OffDayCount

                if (HCount > 0)//77
                {
                   // var _policyVM = _getSandwichInfo(empSystemid, leaveTypeId);
                    if (_policyVM.IsAsperEntryOnH)
                    {

                    }
                    else if (_policyVM.IsNoLeaveOnH)
                    {
                        leaveDays = leaveDays - HCount;
                    }
                    else if (_policyVM.InBetweenHoliday)
                    {
                        leaveDays = leaveDays - HCount;
                    }
                }//OffDayCount
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        void _getSandwichInfo(string strLPMSystemID, string strLvTypeId, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT 
                                CONVERT(bit,InBetweenHoliday) InBetweenHoliday
                                ,CONVERT(bit,InBetweenWeekoff) InBetweenWeekoff
                                ,CONVERT(bit,IsAsperEntryOnH) IsAsperEntryOnH
                                ,CONVERT(bit,IsNoLeaveOnH) IsNoLeaveOnH
                                ,CONVERT(bit,IsAsperEntryOnW) IsAsperEntryOnW
                                ,CONVERT(bit,IsNoLeaveOnW) IsNoLeaveOnW
                            FROM dbo.LeavePolicyDetail
                            WHERE  (LPMSystemID = '" + strLPMSystemID + @"') AND (LTSystemID = '" + strLvTypeId + @"')";
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
    }
    public class PolicySandwichVM
    {
        public bool IsAsperEntryOnW { get; set; }
        public bool IsNoLeaveOnW { get; set; }
        public bool InBetweenWeekoff { get; set; }

        public bool IsNoLeaveOnH { get; set; }
        public bool IsAsperEntryOnH { get; set; }
        public bool InBetweenHoliday { get; set; }
    }
}

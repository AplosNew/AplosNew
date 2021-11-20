using Library.Crosscutting.Security;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Setting
{
    public class clsBonusPolicyMonthlyRetain
    {
        public void Save(BnsPlcMthRetain master, List<BnsPlcMthRetainMthNo> months, List<BonusPolicyMonthlyRetainMasterSalaryHead> HeadList)
        {

            try
            {
                //---------------------
                DeleteBnsPlcMthRetainMonthNo(master.ID);
                //---------------------
                DataSet dsMaster;
                GetBankCashPercentageSettinng(master.ID, out dsMaster);
                _BonusMaster(ref dsMaster, master);

                DataSet dsMonth;
                GetBonusPolicyMonthlyRetainMonthNo(master.ID, out dsMonth);
                _BonusMonth(ref dsMonth, master.ID, months);

                #region Bonus Policy Salary Head Save Part
                if (HeadList != null)
                {
                    DeleteHead(master.ID);
                    GetHead(master.ID, out DataSet dsHead);
                    _Head(ref dsHead, master.ID, HeadList);

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsMonth, dsHead);
                }

                #endregion
                else
                {
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsMonth);
                }

                //clsStaticInfo _info = new clsStaticInfo();
                //_info.SaveDataSets(dsMaster, dsMonth);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _Head(ref DataSet dsSaveBonusMonths, string MasterID, List<BonusPolicyMonthlyRetainMasterSalaryHead> HeadList)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                dtMSave = dsSaveBonusMonths.Tables[0];
                int count = 0;
                foreach (var item in HeadList)
                {
                    dvMSave = new DataView();
                    dvMSave.Table = dtMSave;
                    dvMSave.RowFilter = "BonusPolicyMonthlyRetainMasterId ='" + item.BonusPolicyMonthlyRetainMasterId + "' and SalaryHeadID='" + item.SalaryHeadID + "'";
                    if (dvMSave.Count == 0)
                    {
                        count++;
                        drMSave = dtMSave.NewRow();
                        drMSave["Id"] = MasterID + count;
                        drMSave["BonusPolicyMonthlyRetainMasterId"] = MasterID;
                        drMSave["SalaryHeadID"] = item.SalaryHeadID;
                        drMSave["SalaryHeadID"] = item.SalaryHeadID;
                        drMSave["Sequence"] = count;
                        drMSave["AddedBy"] = identity.Name;
                        drMSave["AddedDate"] = DateTime.Now;
                        drMSave["AddedFromIP"] = identity.IPAddress;
                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = System.DateTime.Now.ToString();
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        dtMSave.Rows.Add(drMSave);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteHead(string sMstID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM BonusPolicyMonthlyRetainMasterSalaryHead WHERE BonusPolicyMonthlyRetainMasterId = '" + sMstID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetHead(string sMstID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sMstID != "")
                {
                    strSQL = "SELECT * FROM BonusPolicyMonthlyRetainMasterSalaryHead WHERE BonusPolicyMonthlyRetainMasterId = '" + sMstID + "'";
                }
                else
                {
                    strSQL = "SELECT * FROM BonusPolicyMonthlyRetainMasterSalaryHead ";
                }

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

        public void SaveDetails(BnsPlcMthRetainDetail details)
        {

            try
            {
                DataSet dsDetails;
                GetBonusPolicyMonthlyRetainDetails(details, out dsDetails);
                _BonusDetail(ref dsDetails, details);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDetails);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveDistribution(BnsPlcMthRetainDistribution distribution)
        {

            try
            {
                DataSet dsDistribution;
                GetDistribution(distribution, out dsDistribution);
                _BonusDistribution(ref dsDistribution, distribution);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsDistribution);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region S a v e Bonus Policy Monthly Retain Master
        void _BonusMaster(ref DataSet dsSaveBonusMaster, BnsPlcMthRetain ui_master)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);
                _dvSave.RowFilter = "ID ='" + ui_master.ID + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveBonusMaster.Tables[0].NewRow();
                    _BonusMasterCol("ADDNEW", ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _BonusMasterCol("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _BonusMasterCol(string OPN_FLAG, BnsPlcMthRetain ui_master, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;

            string idFromDB = "";
            string systemID = "";

            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "BOUNS_POLICY_MONTHLY_RETAIN", out idFromDB);
                    systemID = "BPMR-" + idFromDB;
                    ui_master.ID = systemID.Trim();

                    drLocal["ID"] = bplib.clsWebLib.RetValidLen(ui_master.ID);
                    drLocal["BnsPlcMthRetainName"] = ui_master.BnsPlcMthRetainName;
                    drLocal["BnsPlcMthRetainDescription"] = ui_master.BnsPlcMthRetainDescription;
                    drLocal["IsAllEmpApplocable"] = ui_master.IsAllEmpApplocable;
                    drLocal["PlantID"] = ui_master.PlantID;
                    drLocal["GroupID"] = ui_master.GroupID;
                    drLocal["IsIndividual"] = ui_master.IsIndividual;
                    drLocal["AddedBy"] = ui_master.AddedBy;
                    drLocal["AddedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                    drLocal["AddedFromIP"] = ui_master.AddedFromIP;
                }
                else
                {
                    drLocal["BnsPlcMthRetainName"] = ui_master.BnsPlcMthRetainName;
                    drLocal["BnsPlcMthRetainDescription"] = ui_master.BnsPlcMthRetainDescription;
                    drLocal["IsAllEmpApplocable"] = ui_master.IsAllEmpApplocable;
                    drLocal["IsIndividual"] = ui_master.IsIndividual;
                    drLocal["UpdatedBy"] = ui_master.AddedBy;
                    drLocal["UpdatedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                    drLocal["UpdatedFromIP"] = ui_master.AddedFromIP;
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
        #endregion S a v e Bonus Policy Monthly Retain Master

        #region S a v e Bonus Policy Monthly Retain Month
        void _BonusMonth(ref DataSet dsSaveBonusMonths, string MasterID, List<BnsPlcMthRetainMthNo> ui_monthList)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            try
            {
                dtMSave = dsSaveBonusMonths.Tables[0];
                int count = 0;
                foreach (var item in ui_monthList)
                {
                    dvMSave = new DataView();
                    dvMSave.Table = dtMSave;
                    dvMSave.RowFilter = "BnsPlcMthRetainMstID ='" + item.BnsPlcMthRetainMstID + "' and MonthNo='" + item.MonthNo + "'";
                    if (dvMSave.Count == 0)
                    {
                        count++;
                        drMSave = dtMSave.NewRow();
                        drMSave["BnsPlcMthRetainMstID"] = MasterID;
                        drMSave["MonthName"] = item.MonthName;
                        string MonthNo;
                        getMonth(item.MonthName, out MonthNo);
                        drMSave["MonthNo"] = MonthNo;
                        dtMSave.Rows.Add(drMSave);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion S a v e Bonus Policy Monthly Retain Master

        #region S a v e Bonus Policy Monthly Retain Details
        void _BonusDetail(ref DataSet dsSaveBonusDetail, BnsPlcMthRetainDetail ui_monthDetails)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            try
            {
                bplib.clsGenID objGenID = null;
                string idFromDB = "";
                string systemID = "";
                dtMSave = dsSaveBonusDetail.Tables[0];
                int count = 0;
                dvMSave = new DataView();
                dvMSave.Table = dtMSave;
                dvMSave.RowFilter = "ID ='" + ui_monthDetails.ID + "' ";
                if (dvMSave.Count == 0)
                {
                    count++;
                    drMSave = dtMSave.NewRow();
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "BOUNS_POLICY_MONTHLY_RETAIN", out idFromDB);
                    systemID = "BPMRD-" + idFromDB;
                    ui_monthDetails.ID = systemID.Trim();
                    drMSave["ID"] = bplib.clsWebLib.RetValidLen(ui_monthDetails.ID);
                    drMSave["BnsPlcMthRetainID"] = ui_monthDetails.BnsPlcMthRetainID;
                    drMSave["FormulaDesEarning"] = ui_monthDetails.FormulaDesEarning;
                    drMSave["FormulaDesIDEarning"] = ui_monthDetails.FormulaDesIDEarning;
                    drMSave["SalaryHeadIDEarning"] = ui_monthDetails.SalaryHeadIDEarning;
                    drMSave["EarningValueRangeFrom"] = ui_monthDetails.EarningValueRangeFrom;
                    drMSave["EarningValueRangeTo"] = ui_monthDetails.EarningValueRangeTo;
                    drMSave["IsMandatory"] = ui_monthDetails.IsMandatory;
                    drMSave["IsFixed"] = ui_monthDetails.IsFixed;
                    if (string.IsNullOrEmpty(ui_monthDetails.FixedValue))
                    {
                        drMSave["FixedValue"] = 0;
                    }
                    else
                    {
                        drMSave["FixedValue"] = ui_monthDetails.FixedValue;
                    }
                    drMSave["IsFormula"] = ui_monthDetails.IsFormula;
                    if (string.IsNullOrEmpty(ui_monthDetails.IsDependOnEarning))
                    {
                        drMSave["IsDependOnEarning"] = false;
                    }
                    else
                    {
                        drMSave["IsDependOnEarning"] = ui_monthDetails.IsDependOnEarning;
                    }
                    if (string.IsNullOrEmpty(ui_monthDetails.IsMinWages))
                    {
                        drMSave["IsMinWages"] = false;
                    }
                    else
                    {
                        drMSave["IsMinWages"] = ui_monthDetails.IsMinWages;
                    }
                    drMSave["CompMinWagesAndOrginal"] = ui_monthDetails.CompMinWagesAndOrginal;
                    drMSave["SalaryHeadId"] = ui_monthDetails.SalaryHeadIdFormula;
                    drMSave["GroupID"] = ui_monthDetails.GroupID;
                    drMSave["PlantID"] = ui_monthDetails.PlantID;
                    drMSave["FormulaDes"] = ui_monthDetails.FormulaDescription;
                    drMSave["FormulaDesID"] = ui_monthDetails.FormulaIDDescription;
                    drMSave["AddedBy"] = ui_monthDetails.AddedBy;
                    drMSave["AddedFromIP"] = ui_monthDetails.AddedFromIP;
                    drMSave["AddedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                    drMSave["UpdatedBy"] = ui_monthDetails.AddedBy;
                    drMSave["UpdatedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                    drMSave["UpdatedFromIP"] = ui_monthDetails.AddedFromIP;
                    dtMSave.Rows.Add(drMSave);
                }
                else
                {
                    drMSave = dvMSave[0].Row;
                    drMSave.BeginEdit();
                    drMSave["FormulaDesEarning"] = ui_monthDetails.FormulaDesEarning;
                    drMSave["FormulaDesIDEarning"] = ui_monthDetails.FormulaDesIDEarning;
                    drMSave["SalaryHeadIDEarning"] = ui_monthDetails.SalaryHeadIDEarning;
                    drMSave["EarningValueRangeFrom"] = ui_monthDetails.EarningValueRangeFrom;
                    drMSave["EarningValueRangeTo"] = ui_monthDetails.EarningValueRangeTo;
                    drMSave["IsMandatory"] = ui_monthDetails.IsMandatory;
                    drMSave["IsFixed"] = ui_monthDetails.IsFixed;
                    if (string.IsNullOrEmpty(ui_monthDetails.FixedValue))
                    {
                        drMSave["FixedValue"] = 0;
                    }
                    else
                    {
                        drMSave["FixedValue"] = ui_monthDetails.FixedValue;
                    }
                    drMSave["IsFormula"] = ui_monthDetails.IsFormula;
                    if (string.IsNullOrEmpty(ui_monthDetails.IsDependOnEarning))
                    {
                        drMSave["IsDependOnEarning"] = false;
                    }
                    else
                    {
                        drMSave["IsDependOnEarning"] = ui_monthDetails.IsDependOnEarning;
                    }
                    if (string.IsNullOrEmpty(ui_monthDetails.IsMinWages))
                    {
                        drMSave["IsMinWages"] = false;
                    }
                    else
                    {
                        drMSave["IsMinWages"] = ui_monthDetails.IsMinWages;
                    }
                    drMSave["CompMinWagesAndOrginal"] = ui_monthDetails.CompMinWagesAndOrginal;
                    drMSave["SalaryHeadId"] = ui_monthDetails.SalaryHeadIdFormula;
                    drMSave["GroupID"] = ui_monthDetails.GroupID;
                    drMSave["PlantID"] = ui_monthDetails.PlantID;
                    drMSave["FormulaDes"] = ui_monthDetails.FormulaDescription;
                    drMSave["FormulaDesID"] = ui_monthDetails.FormulaIDDescription;
                    drMSave["UpdatedBy"] = ui_monthDetails.AddedBy;
                    drMSave["UpdatedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                    drMSave["UpdatedFromIP"] = ui_monthDetails.AddedFromIP;
                    drMSave.EndEdit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion S a v e Bonus Policy Monthly Retain Master

        #region S a v e Bonus Policy Monthly Retain Distribution
        void _BonusDistribution(ref DataSet dsSaveBonusDetail, BnsPlcMthRetainDistribution distribution)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            try
            {
                bplib.clsGenID objGenID = null;
                string idFromDB = "";
                string systemID = "";
                dtMSave = dsSaveBonusDetail.Tables[0];
                dvMSave = new DataView();
                dvMSave.Table = dtMSave;
                dvMSave.RowFilter = "ID ='" + distribution.ID + "' ";
                if (dvMSave.Count == 0)
                {
                    //count++;
                    drMSave = dtMSave.NewRow();
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "BOUNS_POLICY_MONTHLY_RETAIN", out idFromDB);
                    systemID = "BPMR_D_D-" + idFromDB;
                    distribution.ID = systemID.Trim();
                    drMSave["ID"] = bplib.clsWebLib.RetValidLen(distribution.ID);
                    drMSave["BonusPolicyDetailsID"] = distribution.BonusPolicyDetailsID;
                    drMSave["FstValue"] = distribution.FstValue;
                    drMSave["FstSalaryHeadID"] = distribution.FstSalaryHeadID;

                    if (string.IsNullOrEmpty(distribution.SndValue))
                    {
                        drMSave["SndValue"] = 0;
                    }
                    else
                    {
                        drMSave["SndValue"] = distribution.SndValue;
                    }

                    if (string.IsNullOrEmpty(distribution.SndSalaryHeadID))
                    {
                        drMSave["SndSalaryHeadID"] = DBNull.Value;
                    }
                    else
                    {
                        drMSave["SndSalaryHeadID"] = distribution.SndSalaryHeadID;
                    }

                    dtMSave.Rows.Add(drMSave);
                }
                else
                {
                    DataRow dr = dvMSave[0].Row;
                    dr.BeginEdit();
                    distribution.ID = dr["ID"].ToString();
                    dr["FstValue"] = distribution.FstValue;
                    dr["FstSalaryHeadID"] = distribution.FstSalaryHeadID;
                    if (string.IsNullOrEmpty(distribution.SndValue))
                    {

                        dr["SndValue"] = "0";
                    }
                    else
                    {
                        dr["SndValue"] = distribution.SndValue;
                    }

                    if (string.IsNullOrEmpty(distribution.SndSalaryHeadID))
                    {
                        dr["SndSalaryHeadID"] = DBNull.Value;
                    }
                    else
                    {
                        dr["SndSalaryHeadID"] = distribution.SndSalaryHeadID;
                    }
                    dr.EndEdit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion S a v e Bonus Policy Monthly Retain Distribution

        void getMonth(string MonthName, out string monthNo)
        {
            monthNo = string.Empty;
            try
            {
                switch (MonthName)
                {
                    case "January":
                        monthNo = "01";
                        break;
                    case "February":
                        monthNo = "02";
                        break;
                    case "March":
                        monthNo = "03";
                        break;
                    case "April":
                        monthNo = "04";
                        break;
                    case "May":
                        monthNo = "05";
                        break;
                    case "June":
                        monthNo = "06";
                        break;
                    case "July":
                        monthNo = "07";
                        break;
                    case "August":
                        monthNo = "08";
                        break;
                    case "September":
                        monthNo = "09";
                        break;
                    case "October":
                        monthNo = "10";
                        break;
                    case "November":
                        monthNo = "11";
                        break;
                    case "December":
                        monthNo = "12";
                        break;
                    default:
                        monthNo = "01";
                        break;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void GetBonusPolicyMonthlyRetainMonthNo(string sMstID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sMstID != "")
                {
                    strSQL = "SELECT * FROM BonusPolicyMonthlyRetainMonthNo WHERE BnsPlcMthRetainMstID = '" + sMstID + "'";
                }
                else
                {
                    strSQL = "SELECT * FROM BonusPolicyMonthlyRetainMonthNo ";
                }

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
        public void GetBonusPolicyMonthlyRetainDetails(BnsPlcMthRetainDetail details, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (details.BnsPlcMthRetainID != "")
                {
                    strSQL = @"SELECT *
                                FROM [dbo].[BonusPolicyMonthlyRetainDetails]
                                WHERE BnsPlcMthRetainID = '" + details.BnsPlcMthRetainID + "'";
                }
                else
                {
                    strSQL = @"SELECT *
                                FROM [dbo].[BonusPolicyMonthlyRetainDetails]";
                }

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
        public void DeleteBnsPlcMthRetainMonthNo(string sMstID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM BonusPolicyMonthlyRetainMonthNo WHERE BnsPlcMthRetainMstID = '" + sMstID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetDistribution(BnsPlcMthRetainDistribution distribution, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM BonusPolicyMonthlyRetainDistribution WHERE BonusPolicyDetailsID= '" + distribution.BonusPolicyDetailsID + @"'";
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
        public void GetBankCashPercentageSettinng(string PlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM BonusPolicyMonthlyRetainMaster WHERE ID = '" + PlantID + @"'";
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

        public void DeleteMaster(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[BonusPolicySalaryHead] where BonusPolicyMasterId='" + ID + "'");
                con.executeQuery("delete from BonusPolicyMonthlyRetainMaster where ID='" + ID + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteMonth(string ID, string monthno)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from BonusPolicyMonthlyRetainMonthNo where BnsPlcMthRetainMstID='" + ID + "' and MonthNo= '" + monthno + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteDetails(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                {
                    throw new Exception("Select Id first");
                }
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from BonusPolicyMonthlyRetainDetails where ID='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteDistribution(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                {
                    throw new Exception("Select Id first");
                }
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from BonusPolicyMonthlyRetainDistribution where ID='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

public class BnsPlcMthRetain
{
    public string ID { get; set; }
    public string BnsPlcMthRetainName { get; set; }
    public string BnsPlcMthRetainDescription { get; set; }
    public string PlantID { get; set; }
    public string GroupID { get; set; }
    public bool IsAllEmpApplocable { get; set; }
    public bool IsIndividual { get; set; }
    public string AddedBy { get; set; }
    public string AddedFromIP { get; set; }
    public string AddedDate { get; set; }
    public string UpdatedFromIP { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedDate { get; set; }
}
public class BnsPlcMthRetainMthNo
{
    public string BnsPlcMthRetainMstID { get; set; }
    public string MonthNo { get; set; }
    public string MonthName { get; set; }
}
public class BnsPlcMthRetainDetail
{
    public string ID { get; set; }
    public string BnsPlcMthRetainID { get; set; }
    public string FormulaDesEarning { get; set; }
    public string FormulaDesIDEarning { get; set; }
    public string SalaryHeadIDEarning { get; set; }
    public string EarningValueRangeFrom { get; set; }
    public string EarningValueRangeTo { get; set; }
    public string IsMandatory { get; set; }
    public string IsFixed { get; set; }
    public string FixedValue { get; set; }
    public string IsFormula { get; set; }
    public string IsDependOnEarning { get; set; }
    public string IsMinWages { get; set; }
    public string CompMinWagesAndOrginal { get; set; }
    public string GroupID { get; set; }
    public string PlantID { get; set; }
    public string FormulaDescription { get; set; }
    public string FormulaIDDescription { get; set; }
    public string SalaryHeadID { get; set; }
    public string SalaryHeadIdFormula { get; set; }
    public string AddedBy { get; set; }
    public string AddedDate { get; set; }
    public string AddedFromIP { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedDate { get; set; }
    public string UpdatedFromIP { get; set; }
}

public class BnsPlcMthRetainDistribution
{
    public string ID { get; set; }
    public string BonusPolicyDetailsID { get; set; }
    public string FstValue { get; set; }
    public string FstSalaryHeadID { get; set; }
    public string SndValue { get; set; }
    public string SndSalaryHeadID { get; set; }
}
public class BonusPolicyMonthlyRetainMasterSalaryHead
{
    #region Scalar Properties            
    public string Id { get; set; }
    public string BonusPolicyMonthlyRetainMasterId { get; set; }
    public string SalaryHeadID { get; set; }
    #endregion Scalar Properties

    #region Audit Properties

    public string AddedBy { get; set; }

    public DateTime? AddedDate { get; set; }

    public string AddedFromIP { get; set; }

    public string UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string UpdatedFromIP { get; set; }

    #endregion Audit Properties
}
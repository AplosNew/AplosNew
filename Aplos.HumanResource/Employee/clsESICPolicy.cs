using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee
{
    public class clsESICPolicy
    {
        ISqlRepository _sqlRepository;
        public clsESICPolicy()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> GetLeaveList(string masterID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT IsSelectESICLeaveType = Case WHEN SRL.ESICPolicyMasterID IS NULL THEN Convert(bit, 'False')
                            ELSE Convert(bit, 'True') END, L.ID LeaveTypeID, L.Code LeaveCode, L.UserName LeaveName,
                            SRL.ESICPolicyMasterID
                            FROM LeaveType L
                                        LEFT JOIN ESICPolicyLeaveType SRL ON L.ID = SRL.LeaveTypeID 
                                                        AND SRL.ESICPolicyMasterID = '" + masterID + @"'
                            WHERE L.IsESIC = 1";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public void SaveMaster(ESICPolicyMaster master, List<ESICPolicyMonthNo> months, List<ESICPolicyLeaveType> LeaveList, List<ESICPolicySalaryHead> HeadList)
        {

            try
            {
                DataSet dsHead;
                //---------------------
                DeleteMonth(master.ID);
                //---------------------
                DataSet dsMaster;
                GetMaster(master.ID, out dsMaster);
                _Master(ref dsMaster, master);

                DataSet dsMonth;
                GetMonth(master.ID, out dsMonth);
                _Month(ref dsMonth, master.ID, months);

                #region Save ESICPolicySalaryHead Part

                DeleteHead(master.ID);
                GetHead(master.ID, out dsHead);
                _Head(ref dsHead, master.ID, HeadList);

                #endregion

                if (LeaveList == null)
                {
                    clsStaticInfo _infos = new clsStaticInfo();
                    _infos.SaveDataSets(dsMaster, dsMonth, dsHead);
                }
                else
                {
                    DataSet dsLeave;
                    GetESICLeaveType(master.ID, out dsLeave);
                    _LeaveType(ref dsLeave, master.ID, LeaveList);
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsMonth, dsLeave, dsHead);
                }


                //clsStaticInfo _info = new clsStaticInfo();
                //_info.SaveDataSets(dsMaster, dsMonth, dsLeave);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveDetails(ESICPolicyDetails details)
        {

            try
            {
                DataSet dsMaster;
                GetDetails(details.ID, details.ESICPolicyMasterID, out dsMaster);
                _Details(ref dsMaster, details);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region S a v e ESIC Policy Master
        void _Master(ref DataSet dsSaveBonusMaster, ESICPolicyMaster ui_master)
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
                    _Master("ADDNEW", ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _Master("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _Master(string OPN_FLAG, ESICPolicyMaster ui_master, ref DataRow drLocal)
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
                    systemID = "EPM-" + idFromDB;
                    ui_master.ID = systemID.Trim();

                    drLocal["ID"] = bplib.clsWebLib.RetValidLen(ui_master.ID);
                    drLocal["ESICPolicyName"] = ui_master.ESICPolicyName;
                    drLocal["ESICPolicyDescription"] = ui_master.ESICPolicyDescription;

                    drLocal["PlantID"] = ui_master.PlantID;
                    drLocal["GroupID"] = ui_master.GroupID;

                    drLocal["AddedBy"] = ui_master.AddedBy;
                    drLocal["AddedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                    drLocal["AddedFromIP"] = ui_master.AddedFromIP;
                }
                else
                {
                    drLocal["ESICPolicyName"] = ui_master.ESICPolicyName;
                    drLocal["ESICPolicyDescription"] = ui_master.ESICPolicyDescription;
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
        #endregion S a v e ESIC Policy Master

        #region S a v e ESIC Policy Month No
        void _Month(ref DataSet dsSaveBonusMonths, string MasterID, List<ESICPolicyMonthNo> ui_monthList)
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
                    dvMSave.RowFilter = "ESICPolicyMasterID ='" + item.ESICPolicyMasterID + "' and MonthNo='" + item.MonthNo + "'";
                    if (dvMSave.Count == 0)
                    {
                        count++;
                        drMSave = dtMSave.NewRow();
                        drMSave["ESICPolicyMasterID"] = MasterID;
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
        #endregion S a v e ESIC Policy Month No

        #region S a v e ESIC Leave Type
        void _LeaveType(ref DataSet dsSaveBonusMonths, string MasterID, List<ESICPolicyLeaveType> LeaveType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                DataTable dtBp = null;
                DataSet dsBp = null;
                DataView dvBp = null;
                DataRow drBp = null;
                string BPId = string.Empty;
                string sql = "SELECT * FROM [dbo].[ESICPolicyLeaveType] ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");

                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "Bonus_POLICY_P", out BPId);
                int count = 0;

                for (int i = dsBp.Tables[0].Rows.Count - 1; i >= 0; i--)
                {
                    string policyID = dsBp.Tables[0].Rows[i]["ESICPolicyMasterID"].ToString();
                    foreach (var item in LeaveType)
                    {
                        if (item.ESICPolicyMasterID == policyID && item.IsSelectESICLeaveType == false)
                        {
                            DataView dv = new DataView(dsBp.Tables[0]);
                            dv.RowFilter = "ESICPolicyMasterID='" + MasterID + "'";
                            if (dv.Count > 0)
                            {
                                //var ID = dv[0]["ID"].ToString();
                                //drBp = dv[0].Row;
                                //drBp.BeginEdit();
                                Delete(item.ESICPolicyMasterID);
                                //drBp.EndEdit();
                                //dsBp.Tables[0].AcceptChanges();
                            }
                        }
                    }
                }


                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");

                foreach (var item in LeaveType)
                {

                    if (item.IsSelectESICLeaveType == true)
                    {
                        dvBp = new DataView(dsBp.Tables[0]);
                        //dvBp.Table = ;
                        dvBp.RowFilter = " ESICPolicyMasterID='" + MasterID + "' and LeaveTypeID='" + item.LeaveTypeID + "' ";

                        if (dvBp.Count == 0)
                        {
                            count++;
                            //string pk = "B_P_P" + BPId + "_" + count;
                            drBp = dsBp.Tables[0].NewRow();
                            //drBp["ID"] = pk;
                            drBp["ESICPolicyMasterID"] = MasterID;
                            drBp["LeaveTypeID"] = item.LeaveTypeID;

                            //drBp["AddedBy"] = identity.Name;
                            //drBp["AddedDate"] = DateTime.Now;
                            //drBp["AddedFromIP"] = identity.IPAddress;

                            dsBp.Tables[0].Rows.Add(drBp);
                        }

                    }
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsBp);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Delete(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                {
                    throw new Exception("Select Id first");
                }
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[ESICPolicyLeaveType] where ESICPolicyMasterID ='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion S a v e ESIC Policy Month No

        #region S a v e ESIC Policy Details
        void _Details(ref DataSet dsSaveBonusMaster, ESICPolicyDetails ui_master)
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
                    _DetailsCol("ADDNEW", ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _DetailsCol("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _DetailsCol(string OPN_FLAG, ESICPolicyDetails ui_master, ref DataRow drLocal)
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
                    systemID = "EPD-" + idFromDB;
                    ui_master.ID = systemID.Trim();

                    drLocal["ID"] = bplib.clsWebLib.RetValidLen(ui_master.ID);
                    drLocal["ESICPolicyMasterID"] = ui_master.ESICPolicyMasterID;
                    drLocal["FormulaDesEarning"] = ui_master.FormulaDesEarning;
                    drLocal["FormulaDesIDEarning"] = ui_master.FormulaDesIDEarning;
                    drLocal["SalaryHeadIDEarning"] = ui_master.SalaryHeadIDEarning;
                    drLocal["EarningValueRangeFrom"] = ui_master.EarningValueRangeFrom;
                    drLocal["EarningValueRangeTo"] = ui_master.EarningValueRangeTo;
                    drLocal["IsMandatory"] = ui_master.IsMandatory;
                    drLocal["IsFixedEmp"] = ui_master.IsFixedEmp;
                    drLocal["FixedValueEmp"] = ui_master.FixedValueEmp;
                    drLocal["IsFormulaEmp"] = ui_master.IsFormulaEmp;
                    drLocal["IsContributionSlrHDdependOnEarningEmp"] = ui_master.IsContributionSlrHDdependOnEarningEmp;
                    //drLocal["FormulaDes"] = ui_master.FormulaDes;
                    //drLocal["FormulaDesID"] = ui_master.FormulaDesID;
                    drLocal["FormulaDesEmp"] = ui_master.FormulaDescription;
                    drLocal["FormulaDesIDEmp"] = ui_master.FormulaIDDescription;
                    drLocal["SalaryHeadIDEmp"] = ui_master.SalaryHeadIdFormula;
                    //drLocal["SalaryHeadID"] = ui_master.SalaryHeadID;
                    drLocal["IsFixedEmployer"] = ui_master.IsFixedEmployer;
                    drLocal["FixedValueEmployer"] = ui_master.FixedValueEmployer;
                    drLocal["IsFormulaEmployer"] = ui_master.IsFormulaEmployer;
                    drLocal["IsContributionSlrHDdependOnEarningEmployer"] = ui_master.IsContributionSlrHDdependOnEarningEmployer;
                    drLocal["FormulaDesEmployer"] = ui_master.FormulaDesEmployer;
                    drLocal["FormulaDesIDEmployer"] = ui_master.FormulaDesIDEmployer;
                    drLocal["SalaryHeadIDEmployer"] = ui_master.SalaryHeadIDEmployer;

                }
                else
                {
                    drLocal["FormulaDesEarning"] = ui_master.FormulaDesEarning;
                    drLocal["FormulaDesIDEarning"] = ui_master.FormulaDesIDEarning;
                    drLocal["SalaryHeadIDEarning"] = ui_master.SalaryHeadIDEarning;
                    drLocal["EarningValueRangeFrom"] = ui_master.EarningValueRangeFrom;
                    drLocal["EarningValueRangeTo"] = ui_master.EarningValueRangeTo;
                    drLocal["IsMandatory"] = ui_master.IsMandatory;
                    drLocal["IsFixedEmp"] = ui_master.IsFixedEmp;
                    drLocal["FixedValueEmp"] = ui_master.FixedValueEmp;
                    drLocal["IsFormulaEmp"] = ui_master.IsFormulaEmp;
                    drLocal["IsContributionSlrHDdependOnEarningEmp"] = ui_master.IsContributionSlrHDdependOnEarningEmp;
                    //drLocal["FormulaDes"] = ui_master.FormulaDes;
                    //drLocal["FormulaDesID"] = ui_master.FormulaDesID;
                    drLocal["FormulaDesEmp"] = ui_master.FormulaDescription;
                    drLocal["FormulaDesIDEmp"] = ui_master.FormulaIDDescription;
                    drLocal["SalaryHeadIDEmp"] = ui_master.SalaryHeadIdFormula;
                    //drLocal["SalaryHeadID"] = ui_master.SalaryHeadID;
                    drLocal["IsFixedEmployer"] = ui_master.IsFixedEmployer;
                    drLocal["FixedValueEmployer"] = ui_master.FixedValueEmployer;
                    drLocal["IsFormulaEmployer"] = ui_master.IsFormulaEmployer;
                    drLocal["IsContributionSlrHDdependOnEarningEmployer"] = ui_master.IsContributionSlrHDdependOnEarningEmployer;
                    drLocal["FormulaDesEmployer"] = ui_master.FormulaDesEmployer;
                    drLocal["FormulaDesIDEmployer"] = ui_master.FormulaDesIDEmployer;
                    drLocal["SalaryHeadIDEmployer"] = ui_master.SalaryHeadIDEmployer;
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
        #endregion S a v e ESIC Policy Details

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

        public void DeleteMonth(string sMstID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM ESICPolicyMonthNo WHERE ESICPolicyMasterID = '" + sMstID + "'", true, "1");
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

        public void GetMonth(string sMstID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sMstID != "")
                {
                    strSQL = "SELECT * FROM ESICPolicyMonthNo WHERE ESICPolicyMasterID = '" + sMstID + "'";
                }
                else
                {
                    strSQL = "SELECT * FROM ESICPolicyMonthNo ";
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

        public void GetESICLeaveType(string sMstID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sMstID != "")
                {
                    strSQL = "SELECT * FROM ESICPolicyLeaveType WHERE ESICPolicyMasterID = '" + sMstID + "'";
                }
                else
                {
                    strSQL = "SELECT * FROM ESICPolicyLeaveType ";
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

        public void GetMaster(string PlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM ESICPolicyMaster WHERE ID = '" + PlantID + @"'";
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

        public void GetDetails(string ID, string masterID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM ESICPolicyDetails WHERE ESICPolicyMasterID = '" + masterID + @"' and ID='" + ID + "'";
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

        public IEnumerable<object> GetMaster(string PlantID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select m.*,p.CompanyID from [dbo].[ESICPolicyMaster] m
                        left join org.plant p on p.Id = m.plantID
                        where plantID = '" + PlantID + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetMonths(string MasterID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select *  from [dbo].[ESICPolicyMonthNo]
                            where ESICPolicyMasterID ='" + MasterID + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetHeadList(string MasterID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select p.*,s.SalaryHead SalaryHeadName from ESICPolicySalaryHead p
                                left join SalaryHead s on s.SalaryHeadID=p.SalaryHeadID
                            where ESICPolicyMasterID ='" + MasterID + "' Order By Sequence";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetDetails(string masterID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select ID,ESICPolicyMasterID,FormulaDesEarning,FormulaDesIDEarning,SalaryHeadIDEarning,EarningValueRangeFrom,
                            EarningValueRangeTo,IsMandatory,IsFixedEmp,FixedValueEmp,IsFormulaEmp,IsContributionSlrHDdependOnEarningEmp,
                            FormulaDesEmp FormulaDescription,FormulaDesIDEmp FormulaIDDescription,SalaryHeadIDEmp SalaryHeadIdFormula,
                            IsFixedEmployer,FixedValueEmployer, IsFormulaEmployer, IsContributionSlrHDdependOnEarningEmployer, FormulaDesEmployer,
                            FormulaDesIDEmployer,SalaryHeadIDEmployer
                            from ESICPolicyDetails 
                            where ESICPolicyMasterID ='" + masterID + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public void DeleteMonth(string ID, string monthno)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from ESICPolicyMonthNo where ESICPolicyMasterID='" + ID + "' and MonthNo= '" + monthno + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteMaster(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from ESICPolicyLeaveType where ESICPolicyMasterID = '" + ID + "'");
                con.executeQuery("delete from ESICPolicySalaryHead where ESICPolicyMasterID = '" + ID + "'");
                con.executeQuery("delete from ESICPolicyMaster where ID='" + ID + "'");

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
                con.executeQuery("delete from ESICPolicyDetails where ID='" + ID + "'");

                con.CommitTransaction();

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
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM ESICPolicySalaryHead WHERE ESICPolicyMasterID = '" + sMstID + "'", true, "1");
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
                    strSQL = "SELECT * FROM ESICPolicySalaryHead WHERE ESICPolicyMasterID = '" + sMstID + "'";
                }
                else
                {
                    strSQL = "SELECT * FROM ESICPolicySalaryHead ";
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
        void _Head(ref DataSet dsSaveBonusMonths, string MasterID, List<ESICPolicySalaryHead> HeadList)
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
                    dvMSave.RowFilter = "ESICPolicyMasterId ='" + item.ESICPolicyMasterId + "' and SalaryHeadID='" + item.SalaryHeadID + "'";
                    if (dvMSave.Count == 0)
                    {
                        count++;
                        drMSave = dtMSave.NewRow();
                        drMSave["Id"] = MasterID + count;
                        drMSave["ESICPolicyMasterId"] = MasterID;
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
    }
}


public class ESICPolicyMaster
{
    public string ID { get; set; }
    public string ESICPolicyName { get; set; }
    public string ESICPolicyDescription { get; set; }
    public string PlantID { get; set; }
    public string GroupID { get; set; }
    public string AddedBy { get; set; }
    public string AddedFromIP { get; set; }
    public string AddedDate { get; set; }
    public string UpdatedFromIP { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedDate { get; set; }
}

public class ESICPolicyMonthNo
{
    public string ESICPolicyMasterID { get; set; }
    public string MonthNo { get; set; }
    public string MonthName { get; set; }
}

public class ESICPolicyLeaveType
{
    public string ESICPolicyMasterID { get; set; }
    public string LeaveTypeID { get; set; }
    public bool IsSelectESICLeaveType { get; set; }
}

public class ESICPolicySalaryHead
{
    #region Scalar Properties            
    public string Id { get; set; }
    public string ESICPolicyMasterId { get; set; }
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

public class ESICPolicyDetails
{
    public string ID { get; set; }
    public string ESICPolicyMasterID { get; set; }
    public string FormulaDesEarning { get; set; }
    public string FormulaDesIDEarning { get; set; }
    public string SalaryHeadIDEarning { get; set; }
    public string EarningValueRangeFrom { get; set; }
    public string EarningValueRangeTo { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsFixedEmp { get; set; }
    public string FixedValueEmp { get; set; }
    public bool IsFormulaEmp { get; set; }
    public bool IsContributionSlrHDdependOnEarningEmp { get; set; }
    //public string FormulaDes { get; set; }
    //public string FormulaDesID { get; set; }
    public string FormulaDescription { get; set; }
    public string FormulaIDDescription { get; set; }
    public string SalaryHeadIdFormula { get; set; }
    //public string SalaryHeadID { get; set; }
    public bool IsFixedEmployer { get; set; }
    public string FixedValueEmployer { get; set; }
    public bool IsFormulaEmployer { get; set; }
    public bool IsContributionSlrHDdependOnEarningEmployer { get; set; }
    public string FormulaDesEmployer { get; set; }
    public string FormulaDesIDEmployer { get; set; }
    public string SalaryHeadIDEmployer { get; set; }
}
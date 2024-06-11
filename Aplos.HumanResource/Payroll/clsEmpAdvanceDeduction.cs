using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Extension;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll
{
    public class clsEmpAdvanceDeduction
    {
        ISqlRepository _sqlRepository;
        public clsEmpAdvanceDeduction()
        {
            _sqlRepository = new SqlRepository();
        }
        public void SaveAdvance(List<SalaryAdvance> data, string Year, string Month, List<SalaryHeadAD> ExtraSlrHd, string Advance, string Interest, List<SalaryAdvance> DataToBeDelete)
        {
            try
            {
                DataSet dsMWESAMst = null;
                DataTable dtMWESAMst = null;
                DataRow drMWESAMst = null;
                DataView dvMWESAMst = null;

                DataSet dsMWESAChd = null;
                DataTable dtMWESAChd = null;
                DataRow drMWESAChd = null;
                DataView dvMWESAChd = null;

                DataSet dsEmpInfo = null;
                DataSet dsSalaryLock = null;
                DataTable dtEmpInfo = null;
                DataView dvEmpInfo = null;

                clsEmpExtraSalaryAmt objEmpExtAmt = new clsEmpExtraSalaryAmt();

                var MWESAMasterSystemID = "";
                string ChdSystemID = "";
                string sameEmp = null;
                int count = 0;
                double total = 0;
                double totalInterest = 0;

                DataSet dsChild;
                string BPId = string.Empty;
                DataRow drBp = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //string sql = "SELECT * FROM [TRN].[EmployeeAdvanceDeduction] where YearNo= '" + Year + "' and MonthNo='" + Month + "' ";
                con.getDataSet("SELECT * FROM [TRN].[EmployeeAdvanceDeduction] where YearNo= '" + Year + "' and MonthNo='" + Month + "'", out dsChild);

               // con.OpenDataSetThroughAdapter(sql, out dsChild, false, "1");

                string empids = string.Empty;
                string deleteEmpId = string.Empty;
                foreach (SalaryAdvance Item in data)
                {
                    if (empids == "")
                    {
                        empids = "'" + Item.EmployeeId + "'";
                    }
                    else
                    {
                        empids += ",'" + Item.EmployeeId + "'";
                    }

                }
                if (DataToBeDelete != null)
                {
                    foreach (SalaryAdvance Item in DataToBeDelete)
                    {
                        if (deleteEmpId == "")
                        {
                            deleteEmpId = "'" + Item.EmployeeId + "'";
                        }
                        else
                        {
                            deleteEmpId += ",'" + Item.EmployeeId + "'";
                        }

                    }
                }
                string SalaryHeads = string.Empty;
                foreach (SalaryHeadAD Item in ExtraSlrHd)
                {
                    if (SalaryHeads == "")
                    {
                        SalaryHeads = "'" + Advance + "'";
                    }
                    else
                    {
                        SalaryHeads += ",'" + Interest + "'";
                    }

                }
                if (DataToBeDelete != null)
                {
                    DeletedEmplist(deleteEmpId, Convert.ToInt32(Year), Convert.ToInt32(Month), identity.PlantId, SalaryHeads);
                }

                SalaryLockCheck(empids, Convert.ToInt32(Year), Convert.ToInt32(Month), out dsSalaryLock);

                while (dsSalaryLock.Tables[0].DefaultView.Count > 0)
                {
                    throw new Exception("Salary Locked..");
                }

                LoadExternalUploadFromExcelOnGrid(empids, SalaryHeads, Convert.ToInt32(Year), Convert.ToInt32(Month), out dsEmpInfo);
                dtEmpInfo = dsEmpInfo.Tables[0];
                dvEmpInfo = new DataView();

                objEmpExtAmt.GetMthWiseExtSalAmtMaster(empids, Convert.ToInt32(Year), Convert.ToInt32(Month), out dsMWESAMst);

                dtMWESAMst = dsMWESAMst.Tables[0];
                dvMWESAMst = new DataView();
                dvMWESAMst.Table = dtMWESAMst;

                GetMthWiseExtSalAmtChild(identity.PlantId, empids, Convert.ToInt32(Year), Convert.ToInt32(Month), SalaryHeads, out dsMWESAChd);

                dtMWESAChd = dsMWESAChd.Tables[0];
                dvMWESAChd = new DataView();
                dvMWESAChd.Table = dtMWESAChd;

                #region NEW ID GENERATE

                string xstrCurCode;
                string _MasterPK = string.Empty;
                int CountM = 0;
                bplib.clsGenID objGenIDs = new bplib.clsGenID();
                objGenIDs.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmt", out _MasterPK);

                int CountC = 0;
                string ChildPK = string.Empty;
                objGenIDs.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmtChild", out ChildPK);

                #endregion End ID Generate

                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EmployeeAdvanceDeduction", out BPId);
                foreach (var item in data)
                {
                    if (item.IsSelected == true)
                    {
                         if (item.EmployeeAdvanceDetailId !=null)
                        {
                            dsChild.Tables[0].DefaultView.RowFilter = "YearNo='" + Year + "' and EmployeeId='" + item.EmployeeId + "' and MonthNo='" + Month + "' and EmployeeAdvanceDetailId='" + item.EmployeeAdvanceDetailId + "'";

                        }
                        //else if (item.EmployeeSalaryAdvanceId != null && item.AdvanceId == null) {

                        //    dsChild.Tables[0].DefaultView.RowFilter = "YearNo='" + Year + "' and EmployeeId='" + item.EmployeeId + "' and MonthNo='" + Month + "'  and EmployeeSalaryAdvanceId='" + item.EmployeeSalaryAdvanceId + "' ";
                        //}
                        
                        //else
                        //{
                        //    dsChild.Tables[0].DefaultView.RowFilter = "YearNo='" + Year + "' and EmployeeId='" + item.EmployeeId + "' and MonthNo='" + Month + "' and AdvanceId='" + item.AdvanceId + "'";

                        //}
                        if (dsChild.Tables[0].DefaultView.Count == 1)
                        {
                            DataRow dr = dsChild.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["AdvanceId"] = item.AdvanceId;
                            dr["AdvanceReqScheduleId"] = item.AdvanceReqScheduleId;
                            dr["EmployeeSalaryAdvanceId"] = item.EmployeeSalaryAdvanceId;
                            dr["EmployeeAdvanceDetailId"] = item.EmployeeAdvanceDetailId;
                            dr["YearNo"] = item.YearNo;
                            dr["MonthNo"] = item.MonthNo;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();
                        }
                        else
                        {
                            count++;
                            string pk = "EAD" + BPId + "_" + count;
                            drBp = dsChild.Tables[0].NewRow();
                            drBp["Id"] = pk;
                            drBp["CompanyGroupId"] = identity.CompanyGroupId;
                            drBp["EmployeeId"] = item.EmployeeId;
                            drBp["AdvanceId"] = item.AdvanceId;
                            drBp["AdvanceReqScheduleId"] = item.AdvanceReqScheduleId;
                            drBp["EmployeeSalaryAdvanceId"] = item.EmployeeSalaryAdvanceId;
                            drBp["EmployeeAdvanceDetailId"] = item.EmployeeAdvanceDetailId;
                            drBp["YearNo"] = item.YearNo;
                            drBp["MonthNo"] = item.MonthNo;

                            drBp["AddedBy"] = identity.Name;
                            drBp["AddedDate"] = System.DateTime.Now.ToString();
                            drBp["AddedFromIP"] = identity.IPAddress;
                            dsChild.Tables[0].Rows.Add(drBp);
                        }
                    }
                }//for 


                for (int i = 0; i < data.Count; i++)
                {
                    var InterestFlag = "xx";
                    string strMstSysID = data[i].MWESAMasterSystemID;
                    string empid = data[i].EmployeeId;
                    var xy = data.Where(parameter => parameter.EmployeeId == data[i].EmployeeId).ToList();
                    if (xy.Count > 1)
                    {
                        if (data[i].EmployeeId != sameEmp)
                        {
                            total = data.Where(x => x.EmployeeId == data[i].EmployeeId).Sum(x => Convert.ToDouble(x.PrincipalAmount));
                            totalInterest = data.Where(x => x.EmployeeId == data[i].EmployeeId).Sum(x => Convert.ToDouble(x.InterestAmount));
                            sameEmp = data[i].EmployeeId;

                            dvMWESAMst.Table = dtMWESAMst;
                            dvMWESAMst.RowFilter = "EmpInfoSystemID = '" + data[i].EmployeeId + "' AND MonthNo='" + Month + "' and YearNo='" + Year + "'";
                            if (dvMWESAMst.Count == 0)
                            {
                                CountM++;
                                strMstSysID = "XM" + _MasterPK + "-" + CountM;
                                MWESAMasterSystemID = strMstSysID;
                                drMWESAMst = dtMWESAMst.NewRow();
                                drMWESAMst["SystemID"] = bplib.clsWebLib.RetValidLen(strMstSysID.Trim(), 50);
                                drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(data[i].EmployeeId, 50);

                                drMWESAMst["AddedBy"] = identity.Name;
                                drMWESAMst["DateAdded"] = DateTime.Now;

                                drMWESAMst["PlantID"] = identity.PlantId;
                                drMWESAMst["MonthNo"] = Month;
                                drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(Year);

                                drMWESAMst["UpdatedBy"] = identity.Name;
                                drMWESAMst["DateUpdated"] = DateTime.Now;
                                dtMWESAMst.Rows.Add(drMWESAMst);
                            }
                            else
                            {
                                drMWESAMst = dvMWESAMst[0].Row;
                                drMWESAMst.BeginEdit();
                                drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(data[i].EmployeeId, 50);

                                drMWESAMst["PlantId"] = identity.PlantId;
                                drMWESAMst["MonthNo"] = Month;
                                drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(Year);
                                MWESAMasterSystemID = drMWESAMst["SystemID"].ToString();
                                drMWESAMst["UpdatedBy"] = identity.Name;
                                drMWESAMst["DateUpdated"] = DateTime.Now;

                                drMWESAMst.EndEdit();
                            }

                            ChdSystemID = "";
                            ChdSystemID = data[i].MWESAChildSystemID;
                            dvEmpInfo.Table = dtEmpInfo;
                            dvEmpInfo.RowFilter = "SystemId = '" + data[i].EmployeeId + "'";
                            //dvMWESAChd.RowFilter = "MWESAMasterSystemID = '" + MWESAMasterSystemID + "'";
                            foreach (var item in ExtraSlrHd)
                            {
                                if (data[i].InterestAmount.ToString() != InterestFlag)
                                {
                                    dvMWESAChd.RowFilter = "MWESAMasterSystemID = '" + MWESAMasterSystemID + "' and SalaryHeadID = '" + item.SalaryHead + "'";
                                    if (dvMWESAChd.Count == 0)
                                    {
                                        CountC++;

                                        ChdSystemID = "XC" + ChildPK + "-" + CountC;


                                        drMWESAChd = dtMWESAChd.NewRow();

                                        drMWESAChd["SystemID"] = bplib.clsWebLib.RetValidLen(ChdSystemID, 50);
                                        drMWESAChd["MWESAMasterSystemID"] = bplib.clsWebLib.RetValidLen(strMstSysID.Trim(), 50);

                                        drMWESAChd["AddedBy"] = identity.Name;
                                        drMWESAChd["DateAdded"] = DateTime.Now;
                                        if (totalInterest == 0)
                                        {
                                            InterestFlag = totalInterest.ToString();
                                        }
                                        if (item.SalaryHead == Advance)
                                        {
                                            drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Advance, 50);
                                        }
                                        else
                                        {
                                            drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Interest, 50);
                                        }
                                        drMWESAChd["CurrencyRuleSystemID"] = dvEmpInfo[0]["CurrencyRuleSystemID"].ToString().Trim();
                                        drMWESAChd["EntryCurrencyID"] = dvEmpInfo[0]["EntryCurrencyID"].ToString().Trim();
                                        if (item.SalaryHead == Advance)
                                        {
                                            drMWESAChd["EntryAmount"] = total;
                                            drMWESAChd["DefineAmount"] = total;

                                        }
                                        else
                                        {
                                            drMWESAChd["EntryAmount"] = totalInterest;
                                            drMWESAChd["DefineAmount"] = totalInterest;
                                        }
                                        drMWESAChd["DefineCurrencyID"] = dvEmpInfo[0]["DefinitionCurrencyID"].ToString().Trim();
                                        drMWESAChd["AmtDefinitionCurrencyID"] = dvEmpInfo[0]["AmtDefinationCurrencyID"].ToString().Trim();
                                        drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(data[i].AmtDefinationRate);
                                        drMWESAChd["ExtDataUploadApp"] = "XL";

                                        drMWESAChd["UpdatedBy"] = identity.Name;
                                        drMWESAChd["DateUpdated"] = DateTime.Now;

                                        dtMWESAChd.Rows.Add(drMWESAChd);
                                    }
                                    else
                                    {
                                        drMWESAChd = dvMWESAChd[0].Row;
                                        drMWESAChd.BeginEdit();

                                        if (item.SalaryHead == Advance)
                                        {
                                            drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Advance, 50);

                                        }
                                        else
                                        {
                                            drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Interest, 50);
                                        }
                                        drMWESAChd["CurrencyRuleSystemID"] = dvEmpInfo[0]["CurrencyRuleSystemID"].ToString().Trim();
                                        drMWESAChd["EntryCurrencyID"] = dvEmpInfo[0]["EntryCurrencyID"].ToString().Trim();
                                        if (item.SalaryHead == Advance)
                                        {
                                            drMWESAChd["EntryAmount"] = total;
                                            drMWESAChd["DefineAmount"] = total;

                                        }
                                        else
                                        {
                                            drMWESAChd["EntryAmount"] = totalInterest;
                                            drMWESAChd["DefineAmount"] = totalInterest;
                                        }
                                        drMWESAChd["DefineCurrencyID"] = dvEmpInfo[0]["DefinitionCurrencyID"].ToString().Trim();
                                        drMWESAChd["AmtDefinitionCurrencyID"] = dvEmpInfo[0]["AmtDefinationCurrencyID"].ToString().Trim();
                                        drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(data[i].AmtDefinationRate);
                                        drMWESAChd["ExtDataUploadApp"] = "XL";

                                        drMWESAChd["UpdatedBy"] = identity.Name;
                                        drMWESAChd["DateUpdated"] = DateTime.Now;
                                        drMWESAChd.EndEdit();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        total = Convert.ToDouble(data[i].PrincipalAmount);
                        totalInterest = Convert.ToDouble(data[i].InterestAmount);
                        dvMWESAMst.Table = dtMWESAMst;
                        dvMWESAMst.RowFilter = "EmpInfoSystemID = '" + data[i].EmployeeId + "' AND MonthNo='" + Month + "' and YearNo='" + Year + "'";
                        if (dvMWESAMst.Count == 0)
                        {
                            CountM++;
                            strMstSysID = "XM" + _MasterPK + "-" + CountM;
                            MWESAMasterSystemID = strMstSysID;
                            drMWESAMst = dtMWESAMst.NewRow();
                            drMWESAMst["SystemID"] = bplib.clsWebLib.RetValidLen(strMstSysID.Trim(), 50);
                            drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(data[i].EmployeeId, 50);

                            drMWESAMst["AddedBy"] = identity.Name;
                            drMWESAMst["DateAdded"] = DateTime.Now;

                            drMWESAMst["PlantID"] = identity.PlantId;
                            drMWESAMst["MonthNo"] = Month;
                            drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(Year);

                            drMWESAMst["UpdatedBy"] = identity.Name;
                            drMWESAMst["DateUpdated"] = DateTime.Now;
                            dtMWESAMst.Rows.Add(drMWESAMst);
                        }
                        else
                        {
                            drMWESAMst = dvMWESAMst[0].Row;
                            drMWESAMst.BeginEdit();
                            drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(data[i].EmployeeId, 50);

                            drMWESAMst["PlantId"] = identity.PlantId;
                            drMWESAMst["MonthNo"] = Month;
                            drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(Year);
                            MWESAMasterSystemID = drMWESAMst["SystemID"].ToString();
                            drMWESAMst["UpdatedBy"] = identity.Name;
                            drMWESAMst["DateUpdated"] = DateTime.Now;

                            drMWESAMst.EndEdit();
                        }

                        ChdSystemID = "";
                        ChdSystemID = data[i].MWESAChildSystemID;

                        dvEmpInfo.Table = dtEmpInfo;
                        dvEmpInfo.RowFilter = "SystemId = '" + data[i].EmployeeId + "'";

                        foreach (var item in ExtraSlrHd)
                        {
                            if (data[i].InterestAmount.ToString() != InterestFlag)
                            {
                                dvMWESAChd.RowFilter = "MWESAMasterSystemID = '" + MWESAMasterSystemID + "' and SalaryHeadID = '" + item.SalaryHead + "'";
                                if (dvMWESAChd.Count == 0)
                                {
                                    CountC++;
                                    ChdSystemID = "XC" + ChildPK + "-" + CountC;
                                    drMWESAChd = dtMWESAChd.NewRow();
                                    drMWESAChd["SystemID"] = bplib.clsWebLib.RetValidLen(ChdSystemID, 50);

                                    if (string.IsNullOrEmpty(strMstSysID))
                                    {
                                        drMWESAChd["MWESAMasterSystemID"] = MWESAMasterSystemID; 
                                    }
                                    else
                                    {
                                        drMWESAChd["MWESAMasterSystemID"] = bplib.clsWebLib.RetValidLen(strMstSysID.Trim(), 50);
                                    }

                                    drMWESAChd["AddedBy"] = identity.Name;
                                    drMWESAChd["DateAdded"] = DateTime.Now;

                                    if (totalInterest == 0)
                                    {
                                        InterestFlag = totalInterest.ToString();
                                    }

                                    if (item.SalaryHead == Advance)
                                    {
                                        drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Advance, 50);
                                    }
                                    else
                                    {
                                        drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Interest, 50);
                                    }
                                    if (!string.IsNullOrEmpty(dvEmpInfo[0]["CurrencyRuleSystemID"].ToString().Trim()))
                                    {
                                        drMWESAChd["CurrencyRuleSystemID"] = dvEmpInfo[0]["CurrencyRuleSystemID"].ToString().Trim();
                                        drMWESAChd["EntryCurrencyID"] = dvEmpInfo[0]["EntryCurrencyID"].ToString().Trim();
                                    }
                                    else
                                    {
                                       throw new  Exception("Salary Information is not found for EmployeeId '"+ data[i].EmployeeId + "'");
                                    }
                                  
                                    if (item.SalaryHead == Advance)
                                    {
                                        drMWESAChd["EntryAmount"] = total;
                                        drMWESAChd["DefineAmount"] = total;
                                    }
                                    else
                                    {
                                        drMWESAChd["EntryAmount"] = totalInterest;
                                        drMWESAChd["DefineAmount"] = totalInterest;
                                    }

                                    drMWESAChd["DefineCurrencyID"] = dvEmpInfo[0]["DefinitionCurrencyID"].ToString().Trim();

                                    drMWESAChd["AmtDefinitionCurrencyID"] = dvEmpInfo[0]["AmtDefinationCurrencyID"].ToString().Trim();
                                    drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(data[i].AmtDefinationRate);
                                    drMWESAChd["ExtDataUploadApp"] = "XL";

                                    drMWESAChd["UpdatedBy"] = identity.Name;
                                    drMWESAChd["DateUpdated"] = DateTime.Now;

                                    dtMWESAChd.Rows.Add(drMWESAChd);
                                }
                                else
                                {
                                    drMWESAChd = dvMWESAChd[0].Row;
                                    drMWESAChd.BeginEdit();
                                    if (item.SalaryHead == Advance)
                                    {
                                        drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Advance, 50);

                                    }
                                    else
                                    {
                                        drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Interest, 50);
                                    }
                                    if (totalInterest == 0)
                                    {
                                        InterestFlag = totalInterest.ToString();
                                    }
                                    drMWESAChd["CurrencyRuleSystemID"] = dvEmpInfo[0]["CurrencyRuleSystemID"].ToString().Trim();
                                    drMWESAChd["EntryCurrencyID"] = dvEmpInfo[0]["EntryCurrencyID"].ToString().Trim();

                                    if (item.SalaryHead == Advance)
                                    {
                                        drMWESAChd["EntryAmount"] = total;
                                        drMWESAChd["DefineAmount"] = total;

                                    }
                                    else
                                    {
                                        drMWESAChd["EntryAmount"] = totalInterest;
                                        drMWESAChd["DefineAmount"] = totalInterest;
                                    }

                                    drMWESAChd["DefineCurrencyID"] = dvEmpInfo[0]["DefinitionCurrencyID"].ToString().Trim();
                                    drMWESAChd["AmtDefinitionCurrencyID"] = dvEmpInfo[0]["AmtDefinationCurrencyID"].ToString().Trim();
                                    drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(data[i].AmtDefinationRate);
                                    drMWESAChd["ExtDataUploadApp"] = "XL";

                                    drMWESAChd["UpdatedBy"] = identity.Name;
                                    drMWESAChd["DateUpdated"] = DateTime.Now;
                                    drMWESAChd.EndEdit();
                                }
                            }
                        }
                    }
                }
                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsMWESAMst, dsMWESAChd, dsChild);

               // objEmpExtAmt.SaveDataSets(dsMWESAMst, dsMWESAChd, dsChild);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void LoadExternalUploadFromExcelOnGrid(string sEntityID, string strSalaryHdID, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT   E.SystemId,CR.MstSystemID CurrencyRuleSystemID, 		                            
		                            CR.AmtEntryCurrency EntryCurrencyID, CR.AmtDefinitionCurrency 
		                            DefinitionCurrencyID, CR.AmtDisbusmentCurrency 
		                            AmtDefinationCurrencyID
                            FROM dbo.EmployeeInformation E
		                            LEFT JOIN dbo.SalaryRuleMaster SR ON E.SalaryRuleMasterSystemID = SR.SystemID
		                            LEFT JOIN dbo.CurrencyRuleChild CR ON SR.CurrencyRuleSystemID = CR.MstSystemID AND CR.SalaryHeadID in (" + strSalaryHdID + @")
		                            LEFT JOIN dbo.SalaryHead SD ON CR.SalaryHeadID = SD.SalaryHeadID
		                            --LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster MWESMat ON E.SystemID = MWESMat.EmpInfoSystemID 
															                            ---AND MWESMat.MonthNo = '" + MonthNo + @"' 
															                            ---AND MWESMat.YearNo = '" + YearNo + @"'
		                            --LEFT JOIN dbo.MonthWiseExtraSalaryAmtChild MWESChd ON MWESMat.SystemID = MWESChd.MWESAMasterSystemID 
															                           --- AND SD.SalaryHeadID = MWESChd.SalaryHeadID
															                           --- AND CR.MstSystemID = MWESChd.CurrencyRuleSystemID
		                            ---LEFT JOIN SCS.Currency MWESChdCr On MWESChd.EntryCurrencyID = MWESChdCr.ID
		                            LEFT JOIN SCS.Currency CrEn ON CR.AmtEntryCurrency = CrEn.ID
		                            LEFT JOIN SCS.Currency CrDe ON CR.AmtDefinitionCurrency = CrDe.ID
                            WHERE E.SystemId in ( " + sEntityID + @")
                                 
                            ORDER BY E.EmployeeCode";


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

        public void SalaryLockCheck(string EmpId, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from SalaryLock where EmpSystemId in (" + EmpId + ") and MonthNo='" + MonthNo + "' and YearNo='" + YearNo + "' and IsLocked=1";
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

        public void GetMthWiseExtSalAmtChild(string plantid, string empids, int YearNo, int MonthNo, string SalaryHeadId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID in (" + SalaryHeadId + @") and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                                                                    WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and plantid='" + plantid + @"' and EmpInfoSystemID in (" + empids + @"))";

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
        public void DeletedEmplist(string EmpId, int YearNo, int MonthNo, string plantid, string SalaryHeadId)
        {
            try
            {

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("Delete FROM MonthWiseExtraSalaryAmtChild WHERE SalaryHeadID in (" + SalaryHeadId + @") and MWESAMasterSystemID IN(SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and plantid = '" + plantid + @"' and EmpInfoSystemID in (" + EmpId + @"))");
                //con.executeQuery("Delete FROM MonthWiseExtraSalaryAmtMaster WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and EmpInfoSystemID in (" + EmpId + @")");
                con.executeQuery("Delete FROM [TRN].[EmployeeAdvanceDeduction] where YearNo= '" + YearNo + "' and MonthNo='" + MonthNo + "' and EmployeeId in (" + EmpId + ")");

                con.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }//End Function
    }
}

public class SalaryHeadAD
{
    public string SalaryHead { get; set; }
}
public class SalaryAdvance
{
    public bool IsSelected { get; set; }
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string AdvanceId { get; set; }
    public string AdvanceReqScheduleId { get; set; }
    public string EmployeeSalaryAdvanceId { get; set; }
    public string YearNo { get; set; }
    public string MonthNo { get; set; }
    public string MWESAMasterSystemID { get; set; }
    public string DefineAmount { get; set; }
    public string MWESAChildSystemID { get; set; }
    //
    public string EntryAmount { get; set; }
    public string CurrencyRuleSystemID { get; set; }
    public string EntryCurrencyID { get; set; }
    public string DefinitionCurrencyID { get; set; }
    public string AmtDefinationCurrencyID { get; set; }
    public string AmtDefinationRate { get; set; }
    public string CurrentInstallment { get; set; }
    public string PrincipalAmount { get; set; }
    public double InterestAmount { get; set; }
    public double SalaryHead { get; set; }
    public string EmployeeAdvanceDetailId { get; set; }
}
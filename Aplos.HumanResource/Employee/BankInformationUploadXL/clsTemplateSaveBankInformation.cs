using Library.Crosscutting.Security;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee.BankInformationUploadXL
{
    public class clsTemplateSaveBankInformation
    {


        public void SaveData(CustomIdentity identity, List<BankInformationUploadTemplate> epList)
        {
            DataSet dsEmployeeBankInfo = null;
            DataSet dsEmployeeBankInfoBackUp = null;
            DataSet dsBankDetails = null;
            string EmpSystemIds = string.Empty;
            try
            {
                List<BankInformationUploadTemplateModel> data = new List<BankInformationUploadTemplateModel>();
                clsStaticInfo objS = new clsStaticInfo();

                GetBankInformation(out dsBankDetails);
                foreach (BankInformationUploadTemplate item in epList)
                {
                    BankInformationUploadTemplateModel ob = new BankInformationUploadTemplateModel();
                    ob.EmpSystemID = item.EmpSystemID;
                    ob.EmployeeCode = item.EmployeeCode;
                    ob.BankName = item.BankName;
                    ob.BankAccNo = item.BankAccNo;
                    //ob.SalaryPercentage = item.SalaryPercentage;
                    ob.IFSCCode = item.IFSCCode;
                    ob.MICRCode = item.MICRCode;
                    ob.BankBranchId = GetPK(item.BankName);
                    ob.BankSystemID = GetBankId(GetPK(item.BankName),  dsBankDetails) ;
                    data.Add(ob);
                    if (string.IsNullOrEmpty(EmpSystemIds))
                    {
                        EmpSystemIds = "'" + item.EmpSystemID + "'";
                    }
                    else
                    {
                        EmpSystemIds += ",'" + item.EmpSystemID + "'";
                    }
                    // CheckField(item.EmployeeCode, "EmployeeCode");
                }


                SaveBankInformation(identity, data, EmpSystemIds, out dsEmployeeBankInfo, out dsEmployeeBankInfoBackUp);
                objS.SaveDataSets(dsEmployeeBankInfo, dsEmployeeBankInfoBackUp);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//EOF  

        public void SaveBankInformation(CustomIdentity identity, List<BankInformationUploadTemplateModel> datalist,string EmpSystemIds ,out DataSet dsSave_EmployeeBankInfo, out DataSet dsSave_EmployeeBankInfoBackUp)
        {
            dsSave_EmployeeBankInfo = null;
            dsSave_EmployeeBankInfoBackUp = null;
            string DeleteRowIDs = string.Empty;
            try
            {
                bplib.clsGenID objGenID = new bplib.clsGenID();

                GetEmployeeBankInfo(EmpSystemIds, out dsSave_EmployeeBankInfo);
                EmployeeBankInfoBackUp(EmpSystemIds, out dsSave_EmployeeBankInfoBackUp);

                DataView dvEmployeeBankInfo = new DataView(dsSave_EmployeeBankInfo.Tables[0]);
                DataView dvEmployeeBankInfo2 = new DataView(dsSave_EmployeeBankInfo.Tables[0]);
                DataView dvEmployeeBankInfoBackUp = new DataView(dsSave_EmployeeBankInfoBackUp.Tables[0]);



                if (datalist.Count() > 0)
                {
                   
                    foreach (BankInformationUploadTemplateModel emp in datalist)
                    {
                        dvEmployeeBankInfo.RowFilter = " EmpSystemID = '"+ emp.EmpSystemID + @"'";// and BankBranchId = '" + emp.BankBranchId + @"' and BankSystemID = '" + emp.BankSystemID + @"' and BankAccNo = '" + emp.BankAccNo + @"' and SalaryPercentage = '" + emp.SalaryPercentage + @"'";
                        if (dvEmployeeBankInfo.Count>0)
                        {

                            dvEmployeeBankInfo2.RowFilter = " EmpSystemID = '" + emp.EmpSystemID + @"' and BankBranchId = '" + emp.BankBranchId + @"' and BankSystemID = '" + emp.BankSystemID + @"' and BankAccNo = '" + emp.BankAccNo + @"'";
                            if (dvEmployeeBankInfo2.Count > 0)
                            {
                                //same data, need not to change
                            }
                            else//back
                            {
                                //New
                                DataRow drNew = dsSave_EmployeeBankInfo.Tables[0].NewRow();
                                //dr["RowID"] = dsGetdataRef.Tables[0].Rows[i]["RowID"];
                                drNew["EmpSystemID"] = emp.EmpSystemID;
                                drNew["BankSystemID"] = emp.BankSystemID;
                                drNew["BankBranchId"] = emp.BankBranchId;
                                drNew["BankAccNo"] = emp.BankAccNo;
                                drNew["SalaryPercentage"] = "100";
                                //dr["IsApproved"] = dsGetdataRef.Tables[0].Rows[i]["IsApproved"];
                                //dr["ApprovedDateTime"] = dsGetdataRef.Tables[0].Rows[i]["ApprovedDateTime"];
                                //dr["ApprovedBy"] = dsGetdataRef.Tables[0].Rows[i]["ApprovedBy"];
                                drNew["AddedBy"] = identity.Name;
                                drNew["DateAdded"] = System.DateTime.Now.ToString();
                                drNew["UpdatedBy"] = identity.Name;
                                drNew["DateUpdated"] = System.DateTime.Now.ToString();
                                drNew["IFSCCode"] = emp.IFSCCode;
                                drNew["MICRCode"] = emp.MICRCode;

                                dsSave_EmployeeBankInfo.Tables[0].Rows.Add(drNew);


                                //backup
                                DataRow dr = dsSave_EmployeeBankInfoBackUp.Tables[0].NewRow();
                                //dr["RowID"] = dsGetdataRef.Tables[0].Rows[i]["RowID"];
                                dr["EmpSystemID"] = dvEmployeeBankInfo[0]["EmpSystemID"].ToString();
                                dr["BankSystemID"] = dvEmployeeBankInfo[0]["BankSystemID"].ToString();
                                dr["BankBranchId"] = dvEmployeeBankInfo[0]["BankBranchId"].ToString();
                                dr["BankAccNo"] = dvEmployeeBankInfo[0]["BankAccNo"].ToString();
                                dr["SalaryPercentage"] = dvEmployeeBankInfo[0]["SalaryPercentage"].ToString();
                                dr["IsApproved"] = dvEmployeeBankInfo[0]["IsApproved"];
                                dr["ApprovedDateTime"] = dvEmployeeBankInfo[0]["ApprovedDateTime"];
                                dr["ApprovedBy"] = dvEmployeeBankInfo[0]["ApprovedBy"];
                                dr["AddedBy"] = identity.Name;
                                dr["DateAdded"] = System.DateTime.Now.ToString();
                                dr["UpdatedBy"] = identity.Name;
                                dr["DateUpdated"] = System.DateTime.Now.ToString();
                                dr["IFSCCode"] = dvEmployeeBankInfo[0]["IFSCCode"].ToString();
                                dr["MICRCode"] = dvEmployeeBankInfo[0]["MICRCode"].ToString();
                                dsSave_EmployeeBankInfoBackUp.Tables[0].Rows.Add(dr);

                                //if (string.IsNullOrEmpty(DeleteRowIDs))
                                //{
                                //    DeleteRowIDs = "'" + dvEmployeeBankInfo[0]["RowID"].ToString();
                                //}


                                DataRow drEB = dvEmployeeBankInfo[0].Row;
                                drEB.BeginEdit();
                                drEB.Delete();
                                drEB.EndEdit();
                            }
                        }
                        else
                        {
                            DataRow dr = dsSave_EmployeeBankInfo.Tables[0].NewRow();
                            //dr["RowID"] = dsGetdataRef.Tables[0].Rows[i]["RowID"];
                            dr["EmpSystemID"] = emp.EmpSystemID;
                            dr["BankSystemID"] = emp.BankSystemID;
                            dr["BankBranchId"] = emp.BankBranchId;
                            dr["BankAccNo"] = emp.BankAccNo;
                            dr["SalaryPercentage"] = "100";
                            //dr["IsApproved"] = dsGetdataRef.Tables[0].Rows[i]["IsApproved"];
                            //dr["ApprovedDateTime"] = dsGetdataRef.Tables[0].Rows[i]["ApprovedDateTime"];
                            //dr["ApprovedBy"] = dsGetdataRef.Tables[0].Rows[i]["ApprovedBy"];
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr["IFSCCode"] = emp.IFSCCode;
                            dr["MICRCode"] = emp.MICRCode;
                           
                            dsSave_EmployeeBankInfo.Tables[0].Rows.Add(dr);
                        }
                        dvEmployeeBankInfo.RowFilter = null;
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
        string GetBankId(string BankBranchId,DataSet dsBankDetails)
        {
            string r = string.Empty;
           
            try
            {
                DataView dv = new DataView(dsBankDetails.Tables[0]);
                dv.RowFilter = "BankBranchId='" + BankBranchId + "'";
                if (dv.Count>0)
                {
                    r = dv[0]["BankId"].ToString();
                }
                dv.RowFilter = null;
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetBankInformation(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT bb.Id BankBranchId,bb.BankId,bb.UserName BankBranchName,b.UserName BankName ,bb.UserName+' ('+b.UserName+')_#'+bb.Id BankBranchList from hkp.Bank b
                           LEFT JOIN  hkp.BankBranch bb on bb.BankId=b.Id
                           ---where bb.Active=1 and b.Active=1  
                           ORDER BY bb.UserName";
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

        public void GetEmployeeBankInfo(string EmpSystemIDs ,out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from EmployeeBankInfo where EmpSystemID IN ("+ EmpSystemIDs + ")";
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
        public void EmployeeBankInfoBackUp(string EmpSystemIDs, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from EmployeeBankInfoBackUp where EmpSystemID IN (" + EmpSystemIDs + ")";
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
}

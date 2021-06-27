using Library.Crosscutting.Security;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;


namespace Library.Service.HumanResources.Shift
{
   public class clsTemplateSaveEmployeeWeekOff
    {//
        public void GetEmployeeWeeKOffInformation( out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from [dbo].[EmployeeWeekOffByDay]  WHERE EmpSystemID=''";
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



        public void GetHRSetting(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select DOCBaseON,DOCCount,IsCityMandatory from PlantWiseHRMSSetting where PlantID='" + plantid + "'";
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
        public void GetCivilStatus(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"  select Id,UserName from hkp.CivilStatus where HasPartner=1";
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
        public void GetLegalDesignations(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select B.DesignationId, C.UserName,a.LegalDesignationId LegalDesignation from [MST].[DesignationMasterLegalDesignation] A
                            inner join  [MST].[DesignationMaster] B ON B.Id=A.DesignationMasterId
                            inner join HKP.Designation C ON C.Id=B.DesignationId";

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
        public void GetPlantPrefix(string PlantID, out string pfx)
        {
            string strSQl;
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            pfx = string.Empty;
            try
            {
                strSQl = "SELECT PlantPrefix FROM [dbo].[PlantWiseHRMSSetting] WHERE PlantID = '" + PlantID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, false, "", "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    pfx = dsRef.Tables[0].Rows[0]["PlantPrefix"].ToString();

                    if (pfx.Trim().Length == 0)
                    {
                        throw new Exception("No prefix found for this plant...");
                    }
                }
                else
                {
                    throw new Exception("No prefix found for this plant...");
                }
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function
        private string GetPadding(string iv)
        {
            while (iv.Length < bplib.clsWebLib.EMP_BASIC_PK_PAD)
            {
                iv = "0" + iv;
            }
            return iv;
        }
        public string SplitAlphaString(string str)
        {
            string a = "";
            StringBuilder alpha = new StringBuilder();
            StringBuilder num = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsDigit(str[i]))
                    num.Append(str[i]).ToString();
                else if ((str[i] >= 'A' &&
                         str[i] <= 'Z') ||
                         (str[i] >= 'a' &&
                          str[i] <= 'z') || str[i] == '-')
                    a = alpha.Append(str[i]).ToString();
                if (num.Length > 0)
                {
                    break;
                }
            }

            return a;
        }
        private void UpdateEmployeeInformationDataRow(DataSet dsBudgetCodeInfo, string groupid, string companyid, string plantid,string user, string OPN_FLAG, string systemid, string EmployeeId, EmployeeShiftUploadTemplate ep, string GivenDesignation, string _DOCBaseON, string _DOCCount, ref DataRow drLocal,ref DataSet dsEmpJoblocation,string emp_job)
        {
            try
            {
                string _plantid = plantid;
                string _groupid = groupid;
                string _companyid = companyid;
                //clsValidation clsValidation = new clsValidation();
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = systemid;
                    drLocal["EmployeeId"] = EmployeeId;
                    drLocal["EmployeeCode"] = ep.EmployeeCode;
                    drLocal["EmployeeStatus"] = "Active";
                    drLocal["AddedBy"] = user;
                    drLocal["DateAdded"] = DateTime.Now;
                }
                string eca = SplitAlphaString(ep.EmployeeCode);
                string LastCode = ep.EmployeeCode.Trim().Substring(eca.Length);
                drLocal["EmployeeCodePreFix"] = eca;
                if (LastCode.Length > 0)
                {
                    drLocal["EmployeeCodeNumeric"] = ep.EmployeeCode.Trim().Substring(eca.Length);
                }
                else
                {
                    drLocal["EmployeeCodeNumeric"] = 0;
                }

                

                var JobLocationID = GetPK(ep.JobLocation);
                if (JobLocationID == string.Empty)
                {
                    drLocal["JobLocationID"] = DBNull.Value;
                }
                else
                {
                    drLocal["JobLocationID"] = JobLocationID;

                }

                //drLocal["JobLocationID"] = GetPK(ep.JobLocation);
                drLocal["GroupID"] = _groupid;
                drLocal["CompanyID"] = _companyid;
                drLocal["PlantID"] = _plantid;
                drLocal["UpdatedBy"] = user;
                drLocal["DateUpdated"] = DateTime.Now;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //
            }
        }//End Function
        public static bool GetBool(string inputData)
        {

            try
            {
                if (string.IsNullOrEmpty(inputData) == true)
                {
                    return false;
                }                
                else if (string.Compare(inputData.Trim(), "NO", true) == 0)
                {
                    return false;
                }
                else if (string.IsNullOrEmpty(inputData.Trim()) == true)
                {
                    return false;
                }
                else if (string.Compare(inputData.Trim(), "0", true) == 0)
                {
                    return false;
                }
                else if (string.Compare(inputData.Trim(), "FALSE", true) == 0)
                {
                    return false;
                }
                else if (Convert.ToDouble(bplib.clsWebLib.GetNumData(inputData.Trim())) < 0)
                    return false;


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } // End Function
        private void SetFieldValue(bool HasBudgetCode, ref DataRow drLocal)
        {
            try
            {
                if (HasBudgetCode)
                {
                    //checkNull(ref drLocal, lblEmployeeGroupId.Text, "EmployeeGroupSystemID");
                    //checkNull(ref drLocal, lblUnitId.Text, "UnitID");
                    //checkNull(ref drLocal, lblDivisionId.Text, "DivisionID");
                    //checkNull(ref drLocal, lblSubdivisionId.Text, "SubdivisionID");
                    //drLocal["DepartmentID"] = lblDepartmentId.Text.Trim();
                    //checkNull(ref drLocal, lblSectionId.Text, "SectionID");
                    //checkNull(ref drLocal, lblSubSectionId.Text, "SubSectionID");
                    //checkNull(ref drLocal, lblLineId.Text, "LineID");
                    //checkNull(ref drLocal, lblBudgetCategoryId.Text, "BudgetCategoryID");
                    //checkNull(ref drLocal, lblEmployeeCategoryId.Text, "EmployeeCategorySystemID");
                    //drLocal["DesignationGroupID"] = lblDesignationGroupId.Text.Trim();
                    //drLocal["DesignationSystemID"] = lblDesignationId.Text.Trim();
                }
                else
                {
                    //drLocal["EmployeeGroupSystemID"] = bplib.clsWebLib.RetValidLen(ddlEmployeeGroup.SelectedValue.Trim());
                    //drLocal["UnitID"] = bplib.clsWebLib.RetValidLen(ddlUnit.SelectedValue.ToString().Trim());
                    //drLocal["DivisionID"] = bplib.clsWebLib.RetValidLen(ddlDivision.SelectedValue.ToString().Trim());
                    //drLocal["SubdivisionID"] = bplib.clsWebLib.RetValidLen(ddlSubdivision.SelectedValue.ToString().Trim());
                    //drLocal["DepartmentID"] = bplib.clsWebLib.RetValidLen(ddlDepartment.SelectedValue.ToString().Trim());
                    //drLocal["SectionID"] = bplib.clsWebLib.RetValidLen(ddlSection.SelectedValue.ToString().Trim());
                    //drLocal["SubSectionID"] = bplib.clsWebLib.RetValidLen(ddlSubSection.SelectedValue.ToString().Trim());//this.ddlLine
                    //drLocal["LineID"] = bplib.clsWebLib.RetValidLen(ddlLine.SelectedValue.ToString().Trim());
                    //drLocal["BudgetCategoryID"] = bplib.clsWebLib.RetValidLen(ddlBudgetCategory.SelectedValue.ToString().Trim());
                    //drLocal["EmployeeCategorySystemID"] = bplib.clsWebLib.RetValidLen(ddlEmpCategor.SelectedValue.ToString().Trim());
                    //drLocal["DesignationGroupID"] = bplib.clsWebLib.RetValidLen(ddlDesignationGroup.SelectedValue.ToString().Trim());
                    //drLocal["DesignationSystemID"] = bplib.clsWebLib.RetValidLen(ddlDesignation.SelectedValue.ToString().Trim());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetGivenDesignation(string legaldesig, DataSet dsLegal, out string givenDesig)
        {
            givenDesig = string.Empty;
            try
            {
                DataView dv = new DataView(dsLegal.Tables[0]);
                dv.RowFilter = "LegalDesignation='" + GetPK(legaldesig) + "'";
                if (dv.Count > 0)
                {
                    givenDesig = dv[0]["DesignationId"].ToString();
                }
                else
                {
                    throw new Exception("No 'given designation' is found for Legal designaion [" + dv[0]["UserName"].ToString() + "]");
                }
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
        private void CheckField(string L, string FieldName)
        {
            try
            {
                if (string.IsNullOrEmpty(L))
                {
                    throw new Exception(FieldName + " can not be blank...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void getCountry(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT AM.CountryId, C.UserName Country From ORG.Plant P
                            LEFT OUTER JOIN  MST.AddressMaster AM ON P.AddressMasterId=AM.Id
                            LEFT OUTER JOIN SCS.Country C ON AM.CountryId=C.Id
                            WHERE P.Id='" + plantId + "'";

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
        private void CountryValidation(string countryId,string country, string Paracountryid,string emptype)
        {           
            try
            {
                if (string.IsNullOrEmpty(countryId) == false && string.IsNullOrEmpty(Paracountryid) == false)
                {
                    if (emptype.ToUpper() == "LOCAL")
                    {
                        if (Paracountryid != countryId)
                        {
                            throw new Exception("Local Employee must have the same country in permanent address as that:[" + country + "] of Plant");
                        }
                    }
                    else
                    {
                        if (Paracountryid == countryId)
                        {
                            throw new Exception("Expatriate Employee can not have local country:[" + country + "] in permanent address");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetEmployeeCityMandatory(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                ///170613
                strSQL = @"Select IsCityMandatory from [dbo].[PlantWiseHRMSSetting] Where PlantID='" + plantId + "'";

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
        public void getCutOffDate(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT [Id]     
                              ,[PlantId]
                              ,[ModuleName]
                              ,[CutOffDate]     
                          FROM [SCS].[OpeningBalanceCutOffDate] where PlantId='" + PlantId + "' and ModuleName='HR'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        bool IsSpouseRequired(DataSet ds,string civilStatusid)
        {
            bool r = false;
            try
            {
                DataView dv = new DataView(ds.Tables[0]);
                dv.RowFilter = "id='" + civilStatusid + "'";
                if(dv.Count>0)
                {
                    r = true;
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetSystemId(string plantid,out string  syspad,out  string pad,out string _jlpk)
        {
            pad = string.Empty;
            syspad = string.Empty;
            _jlpk = string.Empty;
            try
            {               
                string _seed = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_BASIC", out _seed);
                syspad = GetPadding(_seed);
                syspad = DateTime.Now.ToString("yy") + syspad;

                string Prefix = null;
                string _seed2 = string.Empty;
                GetPlantPrefix(plantid, out Prefix);
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), plantid + "EMP_BASIC", out _seed2);
                pad = GetPadding(_seed2);
                pad = Prefix + DateTime.Now.ToString("yy") + pad;

                
                string _seed_jl = string.Empty;
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_JOB_LOC", out _seed_jl);
                _jlpk = GetPadding(_seed_jl);
                _jlpk = DateTime.Now.ToString("yy") + _jlpk;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetBudgetCodeInfo(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"select b.id,b.Code,en.UnitId
                                ,p.DivisionId,p.SubDivisionId,p.DesignationId
                                ,p.DepartmentId,p.SectionId,p.SubSectionId,p.IsDirect,p.PaymentLink
                                from mst.ManpowerBudget b
                                left join org.Position p on p.id=b.PositionId
                                left join org.Entity en on en.id=b.EntityId
                                where en.PlantId='" + PlantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        void GetBudgetCodeInfoByCode(DataSet ds, string bCode,ref DataRow dr)
        {
            try
            {
                DataView dv = new DataView(ds.Tables[0]);
                dv.RowFilter = "id='" + bCode + "'";
                if (dv.Count > 0)
                {
                    //DivisionId,SubdivisionID,UnitId,DepartmentId,SectionId,SubSectionId,IsDirect
                    dr["UnitId"] = dv[0]["UnitId"];
                    dr["DivisionId"] = dv[0]["DivisionId"];
                    dr["SubdivisionID"] = dv[0]["SubDivisionId"];
                    dr["DepartmentId"] = dv[0]["DepartmentId"];
                    dr["SectionId"] = dv[0]["SectionId"];
                    dr["SubSectionId"] = dv[0]["SubSectionId"];
                    dr["DesignationSystemID"] = dv[0]["designationid"];
                    dr["IsDirect"] = dv[0]["IsDirect"];
                    //dr["ss"] = dv[0]["PaymentLink"].ToString();
                }else
                {
                    throw new Exception("No budgetCode found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetEmpDateWiseJobLocation(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"select * from  EmpDateWiseJobLocation";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void SaveData(CustomIdentity Identity, List<EmployeeWeekOffUploadTemplate> epList)
        {
            DataSet dsEmployeeWeeKOffInformation = null;
            DataSet dsDOJ = null;
            DataSet dsCOD = null;
            DateTime COD = DateTime.Now;
            
            try
            {




                #region Validation on Required fields
                getDOJ(Identity.PlantId, out dsDOJ);
                getCutOffDate(Identity.PlantId, out dsCOD);
              
                
                //COD                    
                if (dsCOD.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("No cutt-of-Date is defined for this plant...");
                }
                else
                {
                    COD = Convert.ToDateTime(dsCOD.Tables[0].Rows[0]["CutOffDate"].ToString());
                }

              

               
                DataView dvDOJ = new DataView(dsDOJ.Tables[0]);

                foreach (var item in epList)
                {
                    


                    dvDOJ.RowFilter = "SystemId='" + item.EmpSystemID + "'";
                    if (dvDOJ.Count > 0)
                    {
                        DateTime DOJ = Convert.ToDateTime(dvDOJ[0]["DOJ"]);
                        if (DOJ > COD)
                        {
                           
                            item.EffectiveDate = DOJ.ToString("dd-MMM-yyyy");

                        }
                        else
                        {
                           
                            item.EffectiveDate = COD.ToString("dd-MMM-yyyy");
                        }


                    }
                    else
                    {
                        throw new Exception("Employee [" + item.EmployeeCode + "] has No DOJ.");

                    }
                    dvDOJ.RowFilter = null;






                 

                   
                }





                #endregion
                #region Validation on Required fields

                GetEmployeeWeeKOffInformation(out dsEmployeeWeeKOffInformation);


                DataView dvEmployeeWeeKOffInformation = new DataView(dsEmployeeWeeKOffInformation.Tables[0]);
                string sID = string.Empty;               
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EmployeeWeekOffByDayUpload", out sID);
                int count_master = 0;
                foreach (EmployeeWeekOffUploadTemplate item in epList)
                {
                    count_master++;
                    dvEmployeeWeeKOffInformation.RowFilter = " EmpSystemID ='" + item.EmpSystemID + @"' AND EffectiveDate = '" + item.EffectiveDate + @"'";

                    if (dvEmployeeWeeKOffInformation.Count == 0)
                    {
                        DataRow dr = dsEmployeeWeeKOffInformation.Tables[0].NewRow();
                      
                        dr["SystemID"] = "WU"+DateTime.Now.ToString("yy") + sID + "_" + count_master; 
                        dr["EmpSystemID"] = item.EmpSystemID;
                        dr["EffectiveDate"] = item.EffectiveDate;
                        if (!string.IsNullOrEmpty(item.AlignWithCC))
                        {
                            if (item.AlignWithCC.ToUpper()=="YES")
                            {
                                dr["AlignWithCC"] = true;
                            }
                            else
                            {
                                dr["AlignWithCC"] = false;
                            }
                        }
                        else
                        {
                            dr["AlignWithCC"] = false;
                        }




                        if (!string.IsNullOrEmpty(item.IndividualWeekOff))
                        {
                            if (item.IndividualWeekOff.ToUpper() == "YES")
                            {
                                dr["IndividualWeekOff"] = true;
                            }
                            else
                            {
                                dr["IndividualWeekOff"] = false;
                            }
                        }
                        else
                        {
                            dr["IndividualWeekOff"] =  false;
                        }

                        if (!string.IsNullOrEmpty(item.FstOffDay))
                        {
                            dr["FstOffDay"] = item.FstOffDay;
                            dr["FstDayLengthType"] = "Full Day";
                        }

                      




                       
                      

                        dr["AddedBy"] = Identity.Name;
                        dr["DateAdded"] = DateTime.Now;
                        dr["UpdatedBy"] = Identity.Name;
                        dr["DateUpdated"] = DateTime.Now;
                        dsEmployeeWeeKOffInformation.Tables[0].Rows.Add(dr);
                             

                    }
                    dvEmployeeWeeKOffInformation.RowFilter = null;
                }
                #endregion

            
               
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsEmployeeWeeKOffInformation);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//EOF
        void SaveLocationEmp(ref DataSet dsEmpJoblocation, string emppk,string effectiveDate,string joblocationid,string empjobpk)
        {
            try
            {
                DataView dvEmpJoblocation = new DataView(dsEmpJoblocation.Tables[0]);
                dvEmpJoblocation.RowFilter = " EmpSystemID ='" + emppk + "' ";
                if (dvEmpJoblocation.Count > 0)
                {
                    DataRow dr = dvEmpJoblocation[0].Row;
                    dr.BeginEdit();
                    dr["JobLcSystemID"] = joblocationid;
                    dr["EmpSystemID"] = emppk;
                    dr["EffectiveDate"] = effectiveDate;                   
                    dr["UpdatedBy"] = "uploaded";
                    dr["DateUpdated"] = DateTime.Now;
                    dr.EndEdit();
                }
                else
                {
                    DataRow dr = dsEmpJoblocation.Tables[0].NewRow();
                    dr["SystemID"] = empjobpk;
                    dr["EmpSystemID"] = emppk;
                    dr["JobLcSystemID"] = joblocationid;
                    dr["EffectiveDate"] = effectiveDate;
                    dr["AddedBy"] = "uploaded";
                    dr["DateAdded"] = DateTime.Now;                   
                    dsEmpJoblocation.Tables[0].Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //public void getCutOffDate(string PlantId, out DataSet dsRef)
        //{
        //    ConnectionManager.DAL.ConManager objCon;
        //    string strSql = "";

        //    try
        //    {
        //        strSql = @" SELECT [Id]     
        //                      ,[PlantId]
        //                      ,[ModuleName]
        //                      ,[CutOffDate]     
        //                  FROM [SCS].[OpeningBalanceCutOffDate] where PlantId='" + PlantId + "' and ModuleName='HR'";

        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function


        public void getDOJ(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT SystemId,FORMAT(DOJ,'dd-MMM-yyyy') DOJ FROM EmployeeInformation where PlantId='" + PlantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

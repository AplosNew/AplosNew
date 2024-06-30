using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;


namespace Library.Service.HumanResources.Profile
{
    public class clsTemplateSaveProfile
    {
        public void GetEmployeeInformation(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from EmployeeInformation where plantid=''";
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
        private void UpdateEmployeeInformationDataRow(DataSet dsBudgetCodeInfo, string groupid, string companyid, string plantid, string user, string OPN_FLAG, string systemid, string EmployeeId, EmployeeProfileUploadTemplate ep, string GivenDesignation, string _DOCBaseON, string _DOCCount, ref DataRow drLocal, ref DataSet dsEmpJoblocation, ref DataSet dsAppsAuthorization, string emp_job, string auth_pk)
        {

          
            clsEmployeeLoad objEL = new clsEmployeeLoad();
            try
            {
                DataSet  dsEmpCodeGenSetting, dsMaxEmpCode = null;
                string _plantid = plantid;
                string _groupid = groupid;
                string _companyid = companyid;
                //clsValidation clsValidation = new clsValidation();
                

                #region empCode new
                string strEmpSystemID = "";
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_BASIC", out strEmpSystemID);
                string syspad = GetPadding(strEmpSystemID);
               
                systemid = DateTime.Now.ToString("yy") + syspad;

                string Prefix = null;

                var _EmployeeCodeTypeId = GetPK(ep.EmployeeCodeType);
                if (_EmployeeCodeTypeId == string.Empty)
                {
                    drLocal["EmployeeCodeTypeId"] = DBNull.Value;
                }
                else
                {
                    drLocal["EmployeeCodeTypeId"] = _EmployeeCodeTypeId;

                }


                objEL.GetEmpCodeGenSetting(plantid, _EmployeeCodeTypeId, out Prefix, out dsEmpCodeGenSetting);
                 EmployeeId = Prefix + systemid;
                if (Convert.ToBoolean(dsEmpCodeGenSetting.Tables[0].Rows[0]["IsEmployeeCodeOpenField"]) == false)
                {
                    ep.EmployeeCode = null;
                }

                if (string.IsNullOrEmpty(ep.EmployeeCode))
                {
                    objEL.GetMaxEmpCode(plantid, _EmployeeCodeTypeId, out dsMaxEmpCode);

                    if (dsEmpCodeGenSetting.Tables[0].Rows.Count > 0)
                    {
                        if (Convert.ToBoolean(dsEmpCodeGenSetting.Tables[0].Rows[0]["IsEmployeeCodeOpenField"]) == false)
                        {
                            if (dsEmpCodeGenSetting.Tables[0].Rows[0]["EmpCodeGenType"].ToString() == "AutoIncrement")
                            {
                                if (dsMaxEmpCode.Tables[0].Rows.Count > 0)
                                {
                                    int v = Convert.ToInt32(bplib.clsWebLib.GetNumData(dsMaxEmpCode.Tables[0].Rows[0]["EmployeeCode"].ToString())) + 1;
                                    if (v == 1)
                                    {
                                        if (Convert.ToInt32(bplib.clsWebLib.GetNumData(dsEmpCodeGenSetting.Tables[0].Rows[0]["EmpCodeStartValue"].ToString())) != 0)
                                        {
                                            int code = Convert.ToInt32(bplib.clsWebLib.GetNumData(dsEmpCodeGenSetting.Tables[0].Rows[0]["EmpCodeStartValue"].ToString())) + 1;
                                            ep.EmployeeCode = code.ToString();
                                        }
                                        else
                                        {
                                            Exception ex = new Exception("Employee code start value doesn't define in Employee Code Generation...");
                                            throw ex;
                                        }
                                    }
                                    else
                                    {
                                        ep.EmployeeCode = v.ToString();
                                    }
                                }
                            }
                            else
                            {
                                ep.EmployeeCode = systemid;
                            }
                        }
                        if (dsEmpCodeGenSetting.Tables[0].Rows[0]["IsAutoEmpCodeWithPrefix"].ToString() == "True")
                        {
                            ep.EmployeeCode = Prefix + ep.EmployeeCode;
                        }

                    }
                }
                #endregion






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

                var sl = GetPK(ep.Salutation);
                if (sl == string.Empty)
                {
                    drLocal["Salutation"] = DBNull.Value;
                }
                else
                {
                    drLocal["Salutation"] = sl;

                }
                drLocal["FirstName"] = ep.FirstName;
                //drLocal["MiddleName"] = "";
                drLocal["LastName"] = ep.LastName;
                drLocal["EmployeeName"] = ep.FirstName + " " + ep.LastName;
                //drLocal["EmployeeNameLocal"] = bplib.clsWebLib.RetValidLen(txtLocalEmployeeName.Text.Trim());
                //drLocal["NickName"] = bplib.clsWebLib.RetValidLen(txtNickName.Text.Trim().ToUpper());
                drLocal["EmpType"] = ep.EmpType;

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
                drLocal["PaymentMode"] = ep.PaymentMode;

                drLocal["DOB"] = bplib.clsWebLib.DateData_AppToDB(ep.DOB, bplib.clsWebLib.DB_DATE_FORMAT);
                drLocal["BirthdayCelebrationDate"] = bplib.clsWebLib.DateData_AppToDB(ep.DOB.Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                drLocal["DOJ"] = bplib.clsWebLib.DateData_AppToDB(ep.DOJ.Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                drLocal["IssueDate"] = drLocal["DOJ"];
                //drLocal["IssueDate"] = bplib.clsWebLib.DateData_AppToDB(txtIssueDate.Text.ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);

                //clsEmployeeLoad objEmpLoad = new clsEmployeeLoad();
                //DataSet dsplant = null;
                //objEmpLoad.GetHRMSSettings(_plantid, out dsplant);
                //if (dsplant.Tables[0].Rows.Count > 0)
                //{
                drLocal["DOCIsDay"] = (_DOCBaseON.ToUpper() == "DAY" ? true : false);
                drLocal["DOCDay"] = (_DOCBaseON.ToUpper() == "DAY" ? _DOCCount : "0");
                drLocal["DOCIsMonth"] = (_DOCBaseON.ToUpper() == "DAY" ? false : true);
                drLocal["DOCMonth"] = (_DOCBaseON.ToUpper() != "DAY" ? _DOCCount : "0");

                if (string.IsNullOrEmpty(ep.PPeriod_Date))
                {
                    if (_DOCBaseON.ToUpper() == "MONTH")
                    {
                        drLocal["DOC"] = Convert.ToDateTime(ep.DOJ).AddMonths(Convert.ToInt32(_DOCCount)).ToString("dd-MMM-yyyy");
                    }
                    else
                    {
                        drLocal["DOC"] = Convert.ToDateTime(ep.DOJ).AddDays(Convert.ToInt32(_DOCCount)).ToString("dd-MMM-yyyy");
                    }
                }
                else
                {
                    drLocal["DOC"] = Convert.ToDateTime(ep.PPeriod_Date).ToString("dd-MMM-yyyy");
                }
                //drLocal["IsConfirmed"] = bplib.clsWebLib.RetValidLen(this.chkIsConfirmed.Checked.ToString().Trim());

                //}

                ///===============required==========================================
                drLocal["IsDirect"] = true;
                drLocal["SalaryPercentage"] = 0;
                drLocal["RegisterFP"] = 0;
                drLocal["RegisterProximate"] = 0;
                drLocal["IsAttdnProcBaseOnDeviceData"] = true;
                drLocal["IsImage"] = 0;
                drLocal["ApplyingAsFresher"] = 0;
                drLocal["IsAccessible"] = 0;
                drLocal["NumberOfKnownPerson"] = 0;
                drLocal["PreviouslyWorkedHere"] = 0;
                drLocal["AnyRelativeWorkedHere"] = 0;
                ///================================================


                //drLocal["ReActiveDate"] = bplib.clsWebLib.DateData_AppToDB(lblReActive.Text.ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                //drLocal["EmpPicPath"] = Session["CMP_SHORT_NAME"] + "-" + this.txtEmpCode.Text + ".jpg";

                drLocal["FatherName"] = ep.FatherName;
                //drLocal["FatherNameLocal"] = "";
                drLocal["MotherName"] = ep.MotherName;
                //drLocal["MotherNameLocal"] = bplib.clsWebLib.RetValidLen(txtMotherNameLocal.Text.Trim());
                drLocal["NationalID"] = ep.NID;
                //drLocal["TIN"] = "";

                //drLocal["IdentificationMark"] = bplib.clsWebLib.RetValidLen(txtIdentificationMark.Text.Trim());
                //drLocal["LocalIdentificationMark"] = bplib.clsWebLib.RetValidLen(txtIdentificationMarkLocal.Text.Trim());
                //drLocal["Height"] = bplib.clsWebLib.RetValidLen(txtHeight.Text.Trim());
                //drLocal["Weight"] = bplib.clsWebLib.RetValidLen(txtWeight.Text.Trim());

                var ReligionID = GetPK(ep.Religion);
                if (ReligionID == string.Empty)
                {
                    drLocal["ReligionID"] = DBNull.Value;
                }
                else
                {
                    drLocal["ReligionID"] = ReligionID;

                }

                var CitizenID = GetPK(ep.Citizen);
                if (CitizenID == string.Empty)
                {
                    drLocal["CitizenID"] = DBNull.Value;
                }
                else
                {
                    drLocal["CitizenID"] = CitizenID;
                    drLocal["PresCountryID"] = CitizenID;
                    drLocal["ParmCountryID"] = CitizenID;

                }

                var CivilStatusID = GetPK(ep.MaritalStatus);
                if (CivilStatusID == string.Empty)
                {
                    drLocal["CivilStatusID"] = DBNull.Value;
                }
                else
                {
                    drLocal["CivilStatusID"] = CivilStatusID;

                }

                var BloodGroupID = GetPK(ep.BloodGroup);
                if (BloodGroupID == string.Empty)
                {
                    drLocal["BloodGroupID"] = DBNull.Value;
                }
                else
                {
                    drLocal["BloodGroupID"] = BloodGroupID;

                }

                var City = GetPK(ep.City);
                if (City == string.Empty)
                {
                    drLocal["PresCityID"] = DBNull.Value;
                }
                else
                {
                    drLocal["PresCityID"] = City;

                }
                drLocal["IsConfirmed"] = GetBool(ep.IsConfirmed);
                //drLocal["CitizenID"] = GetPK(ep.Country);
                //drLocal["CivilStatusID"] = GetPK(ep.MaritalStatus);

                //if (ep.BloodGroup == null)
                //{
                //    drLocal["BloodGroupID"] = DBNull.Value;
                //}
                //else
                //{
                //    drLocal["BloodGroupID"] =GetPK(ep.BloodGroup);
                //}
                drLocal["GenderID"] = ep.Gender;

                //if (!string.IsNullOrEmpty(txtMarriageDayDate.Text.ToString().Trim()))
                //{
                //    drLocal["MarriagedayCelebrationDate"] = bplib.clsWebLib.DateData_AppToDB(txtMarriageDayDate.Text.ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                //}
                //else
                //{
                //    drLocal["MarriagedayCelebrationDate"] = DBNull.Value;
                //}
                drLocal["SpouseName"] = ep.SpouseName;
                //drLocal["SpouseNameLocal"] = bplib.clsWebLib.RetValidLen(txtEmpSpouseNameLocal.Text.Trim());
                //drLocal["SpouseNationalID"] = bplib.clsWebLib.RetValidLen(txtEmpSpouseNationalIDCard.Text.Trim());
                //drLocal["SpouseOccupation"] = bplib.clsWebLib.RetValidLen(txtEmpSpouseOccupation.Text.Trim());
                //drLocal["NoOfChildren"] = bplib.clsWebLib.GetNumData(txtEmpNoOfChildren.Text.Trim());

                drLocal["PresentAddress1"] = ep.PresentAddress1;
                drLocal["PresentAddress2"] = ep.PresentAddress2;

                //drLocal["PresentAddress1Local"] = bplib.clsWebLib.RetValidLen(txtPresentAddress1Local.Text.Trim());
                //drLocal["PresentAddress2Local"] = bplib.clsWebLib.RetValidLen(txtPresentAddress2Local.Text.Trim());

                //drLocal["PresThanaID"] = bplib.clsWebLib.RetValidLen(ddlPresThana.SelectedValue.Trim());
                //drLocal["PresPostOfficeID"] = bplib.clsWebLib.RetValidLen(ddlPresPostOffice.SelectedValue.Trim());
                //drLocal["PresZipCode"] = bplib.clsWebLib.RetValidLen(txtPresZipCode.Text.Trim());
                var PresDistrictID = GetPK(ep.District);
                if (PresDistrictID == string.Empty)
                {
                    drLocal["PresDistrictID"] = DBNull.Value;
                }
                else
                {
                    drLocal["PresDistrictID"] = PresDistrictID;

                }

                var ParmCountryID = GetPK(ep.Country_permanent);
                if (ParmCountryID == string.Empty)
                {
                    drLocal["ParmCountryID"] = DBNull.Value;
                }
                else
                {
                    drLocal["ParmCountryID"] = ParmCountryID;

                }

                var PresStateId = GetPK(ep.State_Division);
                if (PresStateId == string.Empty)
                {
                    drLocal["PresStateId"] = DBNull.Value;
                }
                else
                {
                    drLocal["PresStateId"] = PresStateId;

                }
                //drLocal["PresDistrictID"] = GetPK(ep.District);
                //drLocal["PresCountryID"] = GetPK(ep.Country);
                //drLocal["PresStateId"] = GetPK(ep.State_Division);
                drLocal["ParmanentAddress1"] = ep.PermanentAddress1;
                drLocal["ParmanentAddress2"] = ep.PermanentAddress2;




                var LegalDesignationId = GetPK(ep.LegalDesignation);
                if (LegalDesignationId == string.Empty)
                {
                    drLocal["LegalDesignationId"] = DBNull.Value;
                }
                else
                {
                    drLocal["LegalDesignationId"] = LegalDesignationId;
                }
                drLocal["LegalDesignationId"] = GetPK(ep.LegalDesignation);
                drLocal["GivenDesignationId"] = GivenDesignation;
                drLocal["BudgetCode"] = GetPK(ep.BudgetCode);

                GetBudgetCodeInfoByCode(dsBudgetCodeInfo, GetPK(ep.BudgetCode), ref drLocal);

                var EmploymentType = ep.EmploymentType;// GetPK(ep.EmploymentType);
                if (EmploymentType == string.Empty)
                {
                    drLocal["EmploymentType"] = DBNull.Value;
                }
                else
                {
                    drLocal["EmploymentType"] = EmploymentType;
                }
                drLocal["UpdatedBy"] = user;
                drLocal["DateUpdated"] = DateTime.Now;

                SaveLocationEmp(ref dsEmpJoblocation, systemid, ep.DOJ, JobLocationID, emp_job);
                SaveAppsAuthorization(ref dsAppsAuthorization, systemid, auth_pk);

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
                    throw new Exception("No 'given designation' is found for Legal designaion [" + legaldesig + "]");
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
        private void CountryValidation(string countryId, string country, string Paracountryid, string emptype)
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
        bool IsSpouseRequired(DataSet ds, string civilStatusid)
        {
            bool r = false;
            try
            {
                DataView dv = new DataView(ds.Tables[0]);
                dv.RowFilter = "id='" + civilStatusid + "'";
                if (dv.Count > 0)
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
        void GetSystemId(string plantid, int Count, out string syspad, out string pad, out string _jlpk, out string AuthPK)
        {
            pad = string.Empty;
            syspad = string.Empty;
            _jlpk = string.Empty;
            AuthPK = string.Empty;
            try
            {
                string _seed = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_BASIC", Count, out _seed);
                //syspad = GetPadding(_seed);
                //syspad = DateTime.Now.ToString("yy") + _seed;
                syspad = _seed;

                string Prefix = null;
                string _seed2 = string.Empty;
                GetPlantPrefix(plantid, out Prefix);
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), plantid + "EMP_BASIC", out _seed2);
                pad = GetPadding(_seed2);
                pad = Prefix + pad;
                //pad = Prefix + DateTime.Now.ToString("yy") + pad;


                string _seed_jl = string.Empty;
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_JOB_LOC", out _seed_jl);
                //_jlpk = GetPadding(_seed_jl);
                _jlpk = _seed_jl;
                //_jlpk = DateTime.Now.ToString("yy") + _jlpk;

                string _seed_Auth = string.Empty;
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_PIN", out _seed_Auth);
                //_jlpk = GetPadding(_seed_jl);
                AuthPK = _seed_Auth;
                //AuthPK = DateTime.Now.ToString("yy") + _seed_Auth;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetCurrentPlantJobLocationData(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT JobLocation+'_#'+systemid UserName FROM JobLocation where PlantID='" + PlantId + "' order by UserName";

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
        public void GetLegalDesignationData(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"select g.UserName+'_#'+g.Id UserName from hkp.LegalDesignation g
                                    inner join mst.LegalSalaryGradeDesignation d on d.LegalDesignationId=g.id
                                    where d.PlantID='" + PlantId + "' and g.Active=1 order by UserName";

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
        public void GetBudgetCodeData(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"select b.Code+'_#'+b.id UserName from mst.ManpowerBudget b
                              inner join org.Entity en on en.id=b.EntityId
                              where en.PlantId='" + PlantId + "' and b.active=1 order by UserName";

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

        public void GetBudgetCodeInfo(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"select b.id,b.Code,en.UnitId
                                ,p.DivisionId,p.SubDivisionId,p.DesignationId
                                ,p.DepartmentId,p.SectionId,p.SubSectionId,p.IsDirect,p.PaymentLink,dm.DesignationGroupId
                                from mst.ManpowerBudget b
                                left join org.Position p on p.id=b.PositionId
                                left join org.Entity en on en.id=b.EntityId
                                left join mst.DesignationMaster dm on dm.DesignationId=p.DesignationId
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
        void GetBudgetCodeInfoByCode(DataSet ds, string bCode, ref DataRow dr)
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
                    //var vv = dv[0]["DesignaionGroupId"].ToString();//DesignationGroupId
                    dr["DesignationGroupId"] = dv[0]["DesignationGroupId"];
                    //dr["ss"] = dv[0]["PaymentLink"].ToString();
                }
                else
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
        public void GetEmpAppsAuthorization(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"select * from   hkp.EmployeeMobileAppsAuthorization";

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

        public void SaveData(string groupid, string companyid, string plantid, string user, List<EmployeeProfileUploadTemplate> epList)
        {
            DataSet dsEmpProfile = null;
            DataSet dsHRSetting;
            DataSet dsLegalDesignations;
            //DataSet dsCity = null;
            DataSet dsCOD = null;
            DataSet dsCountry = null;
            DataSet dsBudgetCodeInfo = null;
            DataSet dsData = null;
            DataSet dsBudgetCodeData = null;
            DataSet dsLegalDesData = null;
            DataSet dsCivilStatus = null;
            DataSet dsEmpJoblocation = null;
            DataSet dsAppsAuthorization = null;
            string _DOCBaseON = string.Empty;
            string _DOCCount = string.Empty;
            string _gdesignation = string.Empty;
            string countryId = string.Empty;
            string country = string.Empty;
            string syspad = string.Empty;
            string pad = string.Empty;
            string _jlpk = string.Empty;
            string _systemid = string.Empty;
            string _employeeid = string.Empty;
            string _emp_job = string.Empty;
            string _emp_app_auth = string.Empty;
            try
            {
                #region Validation For Save 

                GetCurrentPlantJobLocationData(plantid, out dsData);
                var JobLocationlist = new List<JobLocationData>();
                JobLocationlist = dsData.Tables[0].ToList<JobLocationData>();

                GetLegalDesignationData(plantid, out dsLegalDesData);
                var LegalDesignationList = new List<LegalDesignationData>();
                LegalDesignationList = dsLegalDesData.Tables[0].ToList<LegalDesignationData>();

                GetBudgetCodeData(plantid, out dsBudgetCodeData);
                List<BudgetCodeData> BudgetCodeList = new List<BudgetCodeData>();
                BudgetCodeList = dsBudgetCodeData.Tables[0].ToList<BudgetCodeData>();

                #endregion

                #region Validation on Required fields

                GetEmpDateWiseJobLocation(out dsEmpJoblocation);
                GetEmpAppsAuthorization(out dsAppsAuthorization);
                getCutOffDate(plantid, out dsCOD);
                GetCivilStatus(out dsCivilStatus);
                GetHRSetting(plantid, out dsHRSetting);
                getCountry(plantid, out dsCountry);

                //COD                    
                if (dsCOD.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("No cutt-of-Date is defined for this plant...");
                }
                //HR Setting
                if (dsHRSetting.Tables[0].Rows.Count > 0)
                {
                    _DOCBaseON = dsHRSetting.Tables[0].Rows[0]["DOCBaseON"].ToString();
                    _DOCCount = dsHRSetting.Tables[0].Rows[0]["DOCCount"].ToString();
                }
                else
                {
                    throw new Exception("No plant setting is found!!!");
                }
                //Country
                if (dsCountry.Tables[0].Rows.Count > 0)
                {
                    countryId = dsCountry.Tables[0].Rows[0]["CountryId"].ToString();
                    country = dsCountry.Tables[0].Rows[0]["Country"].ToString();
                }

                //foreach (var kk in epList)
                //{
                    
                //}

                foreach (var item in epList)
                {
                    CheckField(item.EmployeeCodeType, "EmployeeCodeType");
                    CheckField(item.EmployeeCode, "EmployeeCode");
                    CheckField(item.Salutation, "Salutation");
                    CheckField(item.FirstName, "First Name");
                    CheckField(item.EmpType, "EmpType");

                    CheckField(item.EmploymentType, "Employment Type");
                    CheckField(item.Gender, "Gender");
                    CheckField(item.Religion, "Religion");
                    CheckField(item.NID, "NID");
                    CheckField(item.DOB, "DOB");
                    CheckField(item.DOJ, "DOJ");

                    //CheckField(item.ShiftEffectiveDate, "Shift EffectiveDate");
                    //CheckField(item.AssignShiftName, "Shift Name");
                    CheckField(item.JobLocation, "Job Location");
                    CheckField(item.LegalDesignation, "Legal Designation");

                    CheckField(item.BudgetCode, "Budget Code");
                    CheckField(item.PaymentMode, "Payment Mode");
                    CheckField(item.Citizen, "Citizen");
                    CheckField(item.State_Division, "State_Division");
                    CheckField(item.District, "District");
                    CheckField(item.PresentAddress1, "PresentAddress1");
                    //DOJ
                    if (Convert.ToDateTime(item.DOJ) > Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                    {
                        throw new Exception("Future Date of Join is not allowed");
                    }
                    //CIVIL STATUS--------------
                    if (IsSpouseRequired(dsCivilStatus, GetPK(item.MaritalStatus)))
                    {
                        CheckField(item.SpouseName, "Spouse Name");
                    }
                    //City-------------------
                    if (Convert.ToInt32(dsHRSetting.Tables[0].Rows[0]["IsCityMandatory"]) > 0)
                    {
                        CheckField(GetPK(item.District), "Present City");
                    }
                    //DOB
                    DateTime NewDate = Convert.ToDateTime(item.DOB).AddYears(18).AddDays(-1);
                    if (NewDate > Convert.ToDateTime(item.DOJ))
                    {
                        throw new Exception("Employee [" + item.EmployeeCode + "] is under 18 years of old...");
                    }
                    //var dob2 = Convert.ToDateTime(item.DOJ).Subtract(Convert.ToDateTime(item.DOB)).Days / 365;
                    //if (dob2 < 18)
                    //{
                    //    Exception ex = new Exception("This Employee Below 18 Years...");
                    //    throw ex;
                    //}
                    //Country
                    //CountryValidation(countryId, country, GetPK(item.Country_permanent), item.EmpType);
                    //DOJ
                    //if (item.JobLocation==)
                    //{
                    //    throw new Exception("Future Date of Join is not allowed");
                    //}

                    var _BudgetCodeList = BudgetCodeList.Where(r => r.UserName == item.BudgetCode);
                    if (_BudgetCodeList == null || _BudgetCodeList.Count() == 0)
                    {
                        throw new Exception( "Budget Code ["+item.BudgetCode+"] is not found for EmployeeCode ["+item.EmployeeCode+"]");
                    }
                    var _LegalDesignationList = LegalDesignationList.Where(r => r.UserName == item.LegalDesignation);
                    if (_LegalDesignationList == null || _LegalDesignationList.Count() == 0)
                    {
                        throw new Exception("Legal Designation [" + item.LegalDesignation + "] is not found for EmployeeCode [" + item.EmployeeCode + "]");
                    }
                    var _JobLocationlist = JobLocationlist.Where(r => r.UserName == item.JobLocation);
                    if (_JobLocationlist == null || _JobLocationlist.Count() == 0)
                    {
                        throw new Exception("Job Location [" + item.JobLocation + "] is not found for EmployeeCode [" + item.EmployeeCode + "]");
                    }
                }

                #endregion
                string _Auth_seed = string.Empty;

                GetBudgetCodeInfo(plantid, out dsBudgetCodeInfo);
                GetEmployeeInformation(plantid, out dsEmpProfile);
                GetLegalDesignations(plantid, out dsLegalDesignations);

                DataView dvEmpProfile = new DataView(dsEmpProfile.Tables[0]);
                GetSystemId(plantid, epList.Count, out syspad, out pad, out _jlpk, out _Auth_seed);

                int _count = 0;
                for (int i = 0; i < epList.Count; i++)
                {
                    _count++;
                    int ids = Convert.ToInt32(syspad) + Convert.ToInt32(_count);
                    //var ids = syspad+"" + (_count.ToString());
                    _systemid = DateTime.Now.ToString("yy") + ids.ToString();
                    _employeeid = DateTime.Now.ToString("yy") + pad + "_" + _count;
                    _emp_job = "EJ" + DateTime.Now.ToString("yy") + _jlpk + "_" + _count;
                    _emp_app_auth = "A" + DateTime.Now.ToString("yy") + _Auth_seed + "_" + _count;
                    dvEmpProfile.RowFilter = " systemid ='' AND PlantID = '" + plantid + @"'";

                    if (dvEmpProfile.Count == 0)
                    {
                        var ep = epList[i];
                        //dsLegalDesignations
                        GetGivenDesignation(ep.LegalDesignation, dsLegalDesignations, out _gdesignation);
                        DataRow dr = dsEmpProfile.Tables[0].NewRow();
                        UpdateEmployeeInformationDataRow(dsBudgetCodeInfo, groupid, companyid, plantid, user, "ADDNEW", _systemid, _employeeid, ep, _gdesignation, _DOCBaseON, _DOCCount, ref dr, ref dsEmpJoblocation, ref dsAppsAuthorization, _emp_job, _emp_app_auth);
                        dsEmpProfile.Tables[0].Rows.Add(dr);
                    }
                    //else
                    //{
                    //    DataRow dr = dvSaveSummary[0].Row;
                    //    dr.BeginEdit();                    
                    //    dr.EndEdit();
                    //}
                    //dvEmpProfile.RowFilter = null;
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsEmpProfile, dsEmpJoblocation, dsAppsAuthorization);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//EOF
        void SaveLocationEmp(ref DataSet dsEmpJoblocation, string emppk, string effectiveDate, string joblocationid, string empjobpk)
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
        void SaveAppsAuthorization(ref DataSet dsAppsAuthorization, string emppk, string pk)
        {
            try
            {
                Random r = new Random();
                int _pin = r.Next(100000, 999999);
                // string _pin = string.Empty;
                DataView dvEmpJoblocation = new DataView(dsAppsAuthorization.Tables[0]);
                dvEmpJoblocation.RowFilter = " EmployeeId ='" + emppk.Trim() + "' ";
                if (dvEmpJoblocation.Count > 0)
                {
                    DataRow dr = dvEmpJoblocation[0].Row;
                    dr.BeginEdit();
                    //dr["Id"] = joblocationid;
                    dr["EmployeeId"] = emppk.Trim();
                    dr["PIN"] = _pin;

                    dr["IsSalaryStructure"] = false;
                    dr["IsPaySlip"] = false;
                    dr["IsMonthlyAttendance"] = false;

                    dr["IsDailyAttendanceNotification"] = false;
                    dr["IsSalaryProcessConfirmationNotification"] = false;
                    dr["IsSalaryDisbursementNotification"] = false;

                    dr["IsIncrementNotification"] = false;
                    dr["IsPromotionNotification"] = false;
                    dr["IsLeaveNotification"] = false;

                    dr["UpdatedBy"] = "uploaded";
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = "::1";
                    dr.EndEdit();
                }
                else
                {
                    DataRow dr = dsAppsAuthorization.Tables[0].NewRow();
                    dr["Id"] = pk;
                    dr["EmployeeId"] = emppk.Trim();
                    dr["PIN"] = _pin;

                    dr["IsSalaryStructure"] = false;
                    dr["IsPaySlip"] = false;
                    dr["IsMonthlyAttendance"] = false;

                    dr["IsDailyAttendanceNotification"] = false;
                    dr["IsSalaryProcessConfirmationNotification"] = false;
                    dr["IsSalaryDisbursementNotification"] = false;

                    dr["IsIncrementNotification"] = false;
                    dr["IsPromotionNotification"] = false;
                    dr["IsLeaveNotification"] = false;

                    dr["AddedBy"] = "uploaded";
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = "::1";
                    dsAppsAuthorization.Tables[0].Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

public class JobLocationData
{
    public string UserName { get; set; }

}

public class LegalDesignationData
{
    public string UserName { get; set; }

}

public class BudgetCodeData
{
    public string UserName { get; set; }

}
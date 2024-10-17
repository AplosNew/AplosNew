using bplib;
using clsAttendance;
using ConnectionManager.DAL;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Aplos.HumanResource
{
    public class EmployeeProfile
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        private ConManager objCon;
        clsEmployeeLoad objEL = new clsEmployeeLoad();
        public EmployeeProfile()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }
        string txtEmpFixShiftEffectiveDate = "";
        string strEmpJbLcSystemID = "";
        string strShiftAssSystemID = "";
        string strWorkOffSystemID = "";
        public string GetEmployeeProfile()
        {
            return "";
        }

        #region Employee Information
        public IEnumerable<object> GetOnRollByBudget(string budgetId)
        {
            try
            {
                string sql = @"SELECT A.TotalNumber,B.OnRoll,TA.TACount,OnRollManPwr=B.OnRoll-ISNULL(TA.TACount,0) FROM MST.ManpowerBudgetDetail A
LEFT JOIN(SELECT COUNT(SystemId) OnRoll,BudgetCode FROM EmployeeInformation WHERE EmployeeStatus = 'Active' AND BudgetCode='" + budgetId + @"' GROUP BY BudgetCode) B ON B.BudgetCode=A.ManpowerBudgetId
LEFT JOIN (SELECT COUNT(BudgetCode) TACount,BudgetCode FROM EmployeeInformation WHERE EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'') IN ('TBS','LONG ABSENTEEISM')  AND BudgetCode='" + budgetId + @"' GROUP BY BudgetCode) TA ON TA.BudgetCode=A.ManpowerBudgetId
Where A.ManpowerBudgetId='" + budgetId + @"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> Query(string column, string value, string companyGroupId, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                string sql = @"SELECT TOP(1000) * FROM (SELECT EI.*,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
                             ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                             ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
                             ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
                             ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
                             ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName BudgetedDesignation,PR.DesignationId
                             ,EAG.AttendanceGroupId,PGM.PayrollGroupId, OM.Code OperationMasterCode , OV.Code OperationVariationCode,LD.UserName LegalDesignation
	                         ,EC.UserName EmpCategoryName, U.UserName Unit,Dv.UserName Division,SD.UserName SubDivision,Se.UserName Section, SuS.UserName SubSection,Ln.UserName Line
	                         , EBC.StandardName BudgetCategoryName--,ESHIFT.FixSystemID
							 ,PT.UserName PartyName,ECT.UserName EmployeeCodeType,ECT.IsOutSider
							 ,ShiftDf.UserName ShiftDefination
							 ,FORMAT(EI.DOJ,'dd-MMM-yyyy') DateOfJoin 
                             ,TenureDay=DATEDIFF(day, FORMAT(EI.DOJ,'dd-MMM-yyyy'),FORMAT(GetDate(),'dd-MMM-yyyy')),REI.EmployeeCode RelativeCode,REI.EmployeeName RelativeName
                            FROM dbo.Employeeinformation EI
                            LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
                            LEFT JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
                            LEFT JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
                            LEFT JOIN SCS.District D ON EI.PresDistrictID = D.Id
                            LEFT JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
                            LEFT JOIN SCS.Country C ON EI.PresCountryID = C.ID
                            LEFT JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
                            LEFT JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
                            LEFT JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                            LEFT JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
                            LEFT JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                            LEFT JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
                            LEFT JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
                            LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
                            LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
                            LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
                            LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                            LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                            LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                            LEFT JOIN dbo.EmployeeAttendanceGroup EAG on EAG.EmployeeId=EI.SystemId
                            LEFT JOIN MST.PayrollGroupMaster PGM on PGM.EmployeeId=EI.SystemId
                            LEFT JOIN dbo.EmployeeCodeType ECT on ECT.Id=EI.EmployeeCodeTypeId
                            
                            LEFT JOIN MST.OperationMaster OM ON OM.Id=EI.OperationMasterID
                            LEFT JOIN MST.OperationVariation OV ON OV.Id=EI.OperationVariationId
                            LEFT JOIN [HKP].[LegalDesignation] LD ON LD.Id=EI.LegalDesignationId
                            LEFT JOIN [HKP].[Party] PT ON PT.Id=EI.VendorId
                             LEFT OUTER JOIN (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
				                            LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
				                            )EC ON EC.DesignationId=EI.GivenDesignationId
                            LEFT OUTER JOIN [ORG].[Unit] AS U ON U.ID = E.UnitID
                            LEFT OUTER JOIN [ORG].Division AS Dv ON Dv.ID = PR.DivisionID
                            LEFT OUTER JOIN	[ORG].Section AS Se ON Se.ID = PR.SectionID
                            LEFT OUTER JOIN	[ORG].SubSection AS SuS ON SuS.ID = PR.SubSectionID
                            LEFT OUTER JOIN [ORG].Line AS Ln ON Ln.ID = PMB.LineID
                            LEFT OUTER JOIN [ORG].SubDivision AS SD ON SD.Id = PR.SubdivisionID
                            LEFT OUTER JOIN [HKP].[EmployeeBudgetCategory] EBC ON EI.BudgetCategoryID = EBC.ID
                            LEFT JOIN EmployeeShiftAssign ESA ON ESA.EmpSystemID=EI.SystemId 
							 AND ESA.SystemId=(Select top(1) SystemId from dbo.EmployeeShiftAssign ES Where ES.EmpSystemID=EI.SystemId Order by EffectiveDate desc)
							 LEFT JOIN ShiftDefination ShiftDf on ShiftDf.SystemID=ESA.FixSystemID
                            LEFT JOIN dbo.Employeeinformation REI ON REI.SystemId=EI.RelativeSystemId
                            WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + "' AND  EI.GroupId='" + companyGroupId + "') AS TEMP WHERE " + strkey + " Order By DateAdded DESC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetEmployeeAssignShift(string EmpSystemID)
        {
            try
            {
                var sql = @" Select ESHIFT.FixSystemID,SD.UserName from EmployeeInformation E
				          LEFT JOIN(
                             SELECT M.EmpSystemID,M.FixSystemID FROM EmployeeShiftAssign M
                             JOIN (
                             SELECT Max(EII.EffectiveDate)EffectiveDate,EII.EmpSystemID 
                             FROM EmployeeShiftAssign EII 
                             WHERE EII.EffectiveDate<=FORMAT(GETDATE(),'dd-MMM-yyyy')
                             GROUP BY EII.EmpSystemID) S ON S.EmpSystemID=M.EmpSystemID AND S.EffectiveDate=M.EffectiveDate
                            ) ESHIFT ON ESHIFT.EmpSystemID=E.SystemId
							left join ShiftDefination SD ON SD.SystemId=ESHIFT.FixSystemID
							Where E.SystemId='" + EmpSystemID + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetPlantWiseHRMSSetting(string PlantId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT HRP.*,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength,CNT.Id CountryId,PC.Operation 
                            FROM PlantWiseHRMSSetting HRP 
                            LEFT JOIN ORG.Plant PL ON HRP.PlantId = PL.Id
                            LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
                            LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id 
                            LEFT JOIN [SCS].[PlantConfig] PC ON PC.PlantId=HRP.PlantID WHERE HRP.PlantId='" + PlantId + @"'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetIsOTEntitled(string PlantId, string designationId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT C.IsOTEntitled FROM SCS.DesignationMasterConfiguration C
                            LEFT JOIN MST.DesignationMaster M ON M.Id=C.DesignationMasterId
                            LEFT JOIN HKP.Designation D ON D.Id=M.DesignationId
                            WHERE D.Id='" + designationId + "' AND C.PlantId='" + PlantId + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmpCodeGenSetting(string PlantId, string employeeCodeTypeId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"Select A.IsEmployeeCodeOpenField,A.EmpCodeGenType,A.EmpCodeStartValue,A.IsAutoEmpCodeWithPrefix,A.Prefix from [dbo].[EmployeeCodeGenGroup] A
                            LEFT JOIN [dbo].[EmployeeCodeGenGroupDetail] B ON B.EmployeeCodeGenGroupId=A.Id
                            where B.PlantId='" + PlantId + "' and B.EmployeeCodeTypeId='" + employeeCodeTypeId + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void GetEmployeeComplianceDocument(string empSystemId, string plantid, string givenDesignationId, string budgetId, string empType)
        {
            string strSQL;
            //ConnectionManager.DAL.ConManager objCon;cls
            clsStaticInfo clsStaticInfo = null;
            try
            {
                strSQL = @"DECLARE @employeeId varchar(20)='" + empSystemId + @"';
									DECLARE @plantId varchar(20)='" + plantid + @"';
									DECLARE @manpowerBudgetId varchar(20)='" + budgetId + @"';
									DECLARE @givenDesignationId varchar(20)='" + givenDesignationId + @"';
									DECLARE @empType varchar(20)='" + empType + @"';
									DELETE FROM EmployeeDocument WHERE EmpSystemID=@employeeId AND FileName IS NULL;
									SELECT  @ManpowerBudgetId=BudgetCode, @givenDesignationId=GivenDesignationId, @empType=EmpType FROM EmployeeInformation WHERE SystemId=@employeeId;
									INSERT INTO EmployeeDocument (Id, EmpSystemID, AddedBy, AddedDate, ComplianceDocumentId, OptionalOrMandatory, ComplianceDocumentSetId, ResponsiblePersonId)
									SELECT @employeeId+'-'+ X.ComplianceDocumentId, @employeeId, '" + empSystemId + @"', GETDATE(), X.ComplianceDocumentId, X.OptionalOrMandatory, X.ComplianceDocumentSetId, X.ResponsiblePersonId from (
									SELECT CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM
								(
								SELECT DISTINCT
										P.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM EmployeeInformation P
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId=@givenDesignationId
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id = PC.ComplianceDocumentId
								LEFT OUTER JOIN ORG.Position PO ON PC.PositionId = PO.Id
								LEFT JOIN MST.ManpowerBudget MB ON MB.PositionId=PO.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId =@plantId AND CD.IsSkillBased = 1
								AND MB.Id=@manpowerBudgetId AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
							UNION
									SELECT  CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM (
							SELECT DISTINCT
										P.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM EmployeeInformation P
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId=@givenDesignationId
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId = @plantId AND CD.IsSkillBased = 0 AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
								)X  WHERE X.ComplianceDocumentId NOT IN(SELECT ComplianceDocumentId from EmployeeDocument ED WHERE ED.EmpSystemID=@employeeId)";
                clsStaticInfo = new clsStaticInfo();

                clsStaticInfo.SaveEmployeeDocument(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        private string SetDOC(string value, string doj, string radiobtn)
        {
            string EmpDateOC = "";
            if (radiobtn == "Month")
            {
                EmpDateOC = Convert.ToDateTime(doj).AddMonths(Convert.ToInt32(value)).ToString("dd-MMM-yyyy");
            }
            if (radiobtn == "Day")
            {
                EmpDateOC = Convert.ToDateTime(doj).AddDays(Convert.ToInt32(value)).ToString("dd-MMM-yyyy");
            }
            return EmpDateOC;
        }

        public IEnumerable<object> GetJobLocationCbo(string flag, string CompanyGroupId, string PlantId)
        {
            try
            {
                string strSQL = string.Empty;

                if (flag == "Load All")
                {
                    strSQL = @"SELECT j.SystemID,(j.JobLocation+' '+p.username) JobLocation,j.PlantID  FROM JobLocation j
                              LEFT JOIN ORG.Plant p on p.Id=j.plantid
                              WHERE CompanyGroupId ='" + CompanyGroupId + @"' ORDER BY JobLocation";
                }
                else if (flag == "Load Less")
                {
                    strSQL = @"SELECT j.SystemID,j.JobLocation+' '+p.username JobLocation,j.PlantID  FROM JobLocation j
                              LEFT JOIN ORG.Plant p on p.Id=j.plantid
                              WHERE j.plantid ='" + PlantId + @"' ORDER BY JobLocation";
                }
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetAllJobLocationCbo(string CompanyGroupId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT j.SystemID,(j.JobLocation+' '+p.username) JobLocation,j.PlantID  FROM JobLocation j
                              LEFT JOIN ORG.Plant p on p.Id=j.plantid
                              WHERE CompanyGroupId ='" + CompanyGroupId + @"' ORDER BY JobLocation";

                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function


        public void SaveData(EmployeeInformation data, IdentityParameter para, string EmployeeCodeCheckLevel, EmpReferenceInformation empRef, Dictionary<string, object> empBank)
        {
            // , Dictionary<string, object> WeekOff, Dictionary<string, object> OT
            #region DataSet Declare



            DataSet dsLocal, dsPlantLock, dsEmpCodeGenSetting, dsMaxEmpCode = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;

            DataSet dsShiftAssign = null;
            DataTable dtShiftAssign = null;
            DataRow drShiftAssign = null;
            DataView dvShiftAssign = null;

            DataSet dsWeekOffByDay = null;
            DataTable dtWeekOffByDay = null;
            DataRow drWeekOffByDay = null;
            DataView dvWeekOffByDay = null;

            DataSet dsEmpJbLc = null;
            DataTable dtEmpJbLc = null;
            DataRow drEmpJbLc = null;
            DataView dvEmpJbLc = null;

            DataSet dsEmpPin = null;
            DataTable dtEmpPin = null;
            DataRow drEmpPin = null;
            DataView dvEmpPin = null;

            DataSet dsEmpRef = null;
            DataTable dtEmpRef = null;
            DataRow drEmpRef = null;
            DataView dvEmpRef = null;


            clsStaticInfo objApp = null;
            clsEmployeeLoad objEmpLoad = null;

            #endregion DataSet Declare

            bool DATA_OK = false;
            bool IsEdit = true;
            DataSet dsHRsettin = null;
            try
            {
                objApp = new clsStaticInfo();
                objEmpLoad = new clsEmployeeLoad();

                //#endregion //End CHECK EDIT/UPDATE ACCESS

                if (DATA_OK == false)
                {
                    #region Validation


                    if (!string.IsNullOrEmpty(data.EmployeeCode))
                    {
                        string eca = SplitAlphaString(data.EmployeeCode);
                        string LastCode = data.EmployeeCode.Substring(eca.Length);

                        if (LastCode.Length > 0 && bplib.clsWebLib.IsNumeric(LastCode) == false)
                        {
                            throw new Exception("Character is accepted only as prefix...");
                        }

                    }


                    GetDefaultPlantWiseHRMSSetting(para.CompanyGroupId, para.PlantId, out dsHRsettin);

                    DataSet dsCOD = null;
                    GetCutOffDate(para.PlantId, out dsCOD);
                    if (dsCOD.Tables[0].Rows.Count == 0)
                    {
                        throw new Exception("No cutt-of-Date is defined for this plant...");
                    }


                    clsEmployeeLoad employeeLoad = new clsEmployeeLoad();

                    #region Employee Basic Info

                    if (dsCOD.Tables[0].Rows.Count > 0)
                    {
                        if (Convert.ToDateTime(dsCOD.Tables[0].Rows[0]["CutOffDate"].ToString()) < data.DOJ)
                        {
                            objEmpLoad.GetDOJSettingPlantWise(para.PlantId, out dsLocal);
                            if (dsLocal.Tables[0].Rows.Count > 0)
                            {
                                //past DOJ allowed
                            }
                            else
                            {
                                if (Convert.ToDateTime(data.DOJ) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                                {
                                    Exception ex = new Exception("Past 'Date of Join'[" + data.DOJ + "] is not allowed.");
                                    throw ex;
                                }
                            }

                            DataSet dsDaysLocal = null;
                            PlantWiseDOJDays(para.PlantId, out dsDaysLocal);
                            if (Convert.ToDateTime(data.DOJ) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                            {
                                if (dsLocal.Tables[0].Rows.Count > 0)
                                {
                                    var start = DateTime.Now;
                                    var end = Convert.ToDateTime(data.DOJ);

                                    TimeSpan difference = start - end;
                                    var days = Convert.ToInt32(difference.Days);
                                    var date = Convert.ToInt32(dsDaysLocal.Tables[0].Rows[0]["PastDOJDaysAllowed"]);
                                    if (date < days)
                                    {
                                        throw new Exception("Maximum  " + dsDaysLocal.Tables[0].Rows[0]["PastDOJDaysAllowed"] + " days back is allowed for DOJ.");
                                    }
                                    //allowed
                                }
                                else
                                {
                                    throw new Exception("Previous Date of Join is not allowed");
                                }
                            }
                            else if (Convert.ToDateTime(data.DOJ) > Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                            {
                                //future
                                throw new Exception("Future Date of Join is not allowed");
                            }
                            else
                            {
                                //Current
                            }
                        }
                    }

                    //if (dsCOD.Tables[0].Rows.Count > 0)
                    //{
                    //    if (Convert.ToDateTime(dsCOD.Tables[0].Rows[0]["CutOffDate"].ToString()) < data.DOJ)
                    //    {
                    //        objEmpLoad.GetDOCSettingPlantWise(para.PlantId, out dsLocal);
                    //        if (dsLocal.Tables[0].Rows.Count > 0)
                    //        {
                    //            //past DOC allowed
                    //        }
                    //        else
                    //        {
                    //            if (Convert.ToDateTime(data.DOC) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                    //            {
                    //                Exception ex = new Exception("Past 'Date of Confirmation'[" + data.DOC + "] is not allowed.");
                    //                throw ex;
                    //            }
                    //        }
                    //    }
                    //}

                    //if (txtEmpCode.Text.Trim() == "" || txtEmpCode.Text.Trim().Length > 30)
                    //{
                    //    txtEmpCode.Focus();
                    //    System.Exception ex = new Exception("Define the Employee Code...(Max length allowed 30)");
                    //    throw (ex);
                    //}


                    string sEmpName = "";
                    sEmpName = data.FirstName + " " + data.MiddleName + " " + data.LastName;
                    if (string.IsNullOrEmpty(sEmpName.Trim()) == false)
                    {
                        data.EmployeeName = sEmpName;
                    }
                    if (data.EmployeeName == "" || data.EmployeeName.Length > 100)
                    {
                        Exception ex = new Exception("Define the Employee Name...(Max length allowed 100)");
                        throw ex;
                    }

                    //if (objEmpLoad.DuplicateCardNumber(data.SystemId, data.CardNumber, out dsLocal) == false)
                    //{

                    //    Exception ex = new Exception("This Proximity Number already define with EmployeeCode: " + dsLocal.Tables[0].Rows[0]["EmployeeCode"].ToString() + "...Proximity Number must be unique");
                    //    throw ex;
                    //}


                    #endregion Employee Basic Info

                    #region Employee DOB

                    if (string.IsNullOrEmpty(data.DOJ.ToString()))
                    {

                        DateTime dtDOB = bplib.clsWebLib.DateData_DBToApp(data.DOJ, bplib.clsWebLib.DB_DATE_FORMAT);
                        DateTime dtToDay = bplib.clsWebLib.DateData_DBToApp(DateTime.Now.ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                        TimeSpan ts = dtDOB - dtToDay;
                        int days = ts.Days;
                        if (days >= 0)
                        {
                            Exception ex = new Exception("Please check the DOB, cannot more than ToDay Date......");
                            throw ex;
                        }
                    }

                    if (!string.IsNullOrEmpty(data.BirthdayCelebrationDate.ToString()))
                    {
                        DateTime dtBCD = bplib.clsWebLib.DateData_DBToApp(data.BirthdayCelebrationDate.ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                        DateTime dtToDay = bplib.clsWebLib.DateData_DBToApp(DateTime.Now.ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                        TimeSpan ts = dtBCD - dtToDay;
                        int days = ts.Days;
                        if (days >= 0)
                        {
                            Exception ex = new Exception("Please check the Birthday Celebration Date, cannot more than ToDay Date......");
                            throw ex;
                        }
                    }
                    if (!string.IsNullOrEmpty(data.MarriagedayCelebrationDate.ToString()))
                    {
                        DateTime dtBCD = bplib.clsWebLib.DateData_DBToApp(data.MarriagedayCelebrationDate, bplib.clsWebLib.DB_DATE_FORMAT);
                        DateTime dtToDay = bplib.clsWebLib.DateData_DBToApp(DateTime.Now.ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                        TimeSpan ts = dtBCD - dtToDay;
                        int days = ts.Days;
                        if (days >= 0)
                        {
                            Exception ex = new Exception("Please check the Marriage day Celebration Date, cannot more than ToDay Date......");
                            throw ex;
                        }
                    }

                    #endregion Employee DOB

                    //if (string.IsNullOrEmpty(this.ddlGender.SelectedValue.Trim()) == true)
                    //{
                    //    this.ddlGender.Focus();
                    //    System.Exception ex = new Exception("Please Select Gender...");
                    //    throw (ex);
                    //}

                    if (data.NoOfChildren.ToString() == "")
                    { data.NoOfChildren = 0; }
                    if (data.NoOfChildren.ToString().Length > 10 || bplib.clsWebLib.IsNumeric(data.NoOfChildren.ToString()) == false)
                    {
                        Exception ex = new Exception("Please Enter Numeric data Only");
                        throw ex;
                    }

                    #region Employee DOJ, DOS, RDOS

                    //string _alock = dsHRsettin.Tables[0].Rows[0]["IsAttendanceLockApplicable"].ToString();
                    string _alock = "True";
                    if (Convert.ToBoolean(_alock))
                    {
                        if (Convert.ToDateTime(dsCOD.Tables[0].Rows[0]["CutOffDate"].ToString()) < data.DOJ)
                        {
                            PlantWiseLock(para.PlantId, Convert.ToDateTime(data.DOJ).ToString("dd-MMM-yyyy"), out dsPlantLock);
                        }
                        else
                        {
                            PlantWiseLock(para.PlantId, Convert.ToDateTime(dsCOD.Tables[0].Rows[0]["CutOffDate"].ToString()).ToString("dd-MMM-yyyy"), out dsPlantLock);
                        }


                        if (dsPlantLock.Tables[0].Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(dsPlantLock.Tables[0].Rows[0]["LockedDate"].ToString()))
                            {
                                if (Convert.ToDateTime(dsCOD.Tables[0].Rows[0]["CutOffDate"].ToString()) < data.DOJ)
                                {
                                    throw new Exception("Date of Join: " + Convert.ToDateTime(data.DOJ).ToString("dd-MMM-yyyy") + " cann't be less than Max-Attendance-Locked-Date : " + Convert.ToDateTime(dsPlantLock.Tables[0].Rows[0]["LockedDate"]).ToString("dd-MMM-yyyy") + "");
                                }
                                else
                                {
                                    throw new Exception("Cut of Date: " + Convert.ToDateTime(dsCOD.Tables[0].Rows[0]["CutOffDate"]).ToString("dd-MMM-yyyy") + " cann't be less than Max-Attendance-Locked-Date : " + Convert.ToDateTime(dsPlantLock.Tables[0].Rows[0]["LockedDate"]).ToString("dd-MMM-yyyy") + "");
                                }
                            }
                        }
                    }//alock

                    DateTime dtDOJ = bplib.clsWebLib.DateData_DBToApp(data.DOJ, bplib.clsWebLib.DB_DATE_FORMAT);
                    DateTime dtAftOneMonth = bplib.clsWebLib.DateData_DBToApp(DateTime.Now.AddMonths(1).ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                    TimeSpan tsCheck = dtDOJ - dtAftOneMonth;
                    int daysCheck = tsCheck.Days;

                    var dob3 = Convert.ToDateTime(data.DOB).AddYears(18);
                    if (dob3 > data.DOJ)
                    {
                        Exception ex = new Exception("This Employee Below 18 Years...");
                        throw ex;
                    }

                    #endregion Employee DOJ, DOS, RDOS

                    var ep = "'" + data.SystemId + "'";
                    // lock validation

                    DateTime _doj_ = Convert.ToDateTime(data.DOJ);

                    AttendanceProcessAplos ob = new AttendanceProcessAplos();

                    ob.LockValidation(para.PlantId, data.DOJ.ToString(), data.DOJ.ToString(), ep);

                    #region PaymentLink
                    string lblPaymentLink = string.Empty;
                    employeeLoad.getSkill(data.BudgetCode, out dsLocal);
                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        lblPaymentLink = dsLocal.Tables[0].Rows[0]["PaymentLink"].ToString();
                    }
                    else
                    {
                        lblPaymentLink = string.Empty;
                    }

                    if (data.BudgetCode.Length > 0)
                    {
                        if (lblPaymentLink.ToUpper() == "SKILL")
                        {
                            employeeLoad.getPlantConfig(para.PlantId, out DataSet dsPC);
                            if (dsPC.Tables[0].Rows.Count > 0)
                            {
                                if (dsPC.Tables[0].Rows[0]["Operation"].ToString() == "Operation Master")
                                {
                                    if (string.IsNullOrEmpty(data.OperationMasterID))
                                    {
                                        throw new Exception("Operation Master is required as Payment Link is Skill in position.");
                                    }
                                }
                                if (dsPC.Tables[0].Rows[0]["Operation"].ToString() == "Operation Variation")
                                {
                                    if (string.IsNullOrEmpty(data.OperationVariationId))
                                    {
                                        throw new Exception("Operation Variation is required as Payment Link is Skill in position.");
                                    }
                                }
                            }
                        }

                    }
                    #endregion

                    #region Shift

                    if (!string.IsNullOrEmpty(data.FixSystemID))
                    {
                        #region Fix Shift
                        if (Convert.ToDateTime(data.DOJ) < Convert.ToDateTime(dsCOD.Tables[0].Rows[0]["CutOffDate"]))
                        {
                            txtEmpFixShiftEffectiveDate = dsCOD.Tables[0].Rows[0]["CutOffDate"].ToString();
                        }
                        else
                        {
                            txtEmpFixShiftEffectiveDate = Convert.ToDateTime(data.DOJ).ToString("dd-MMM-yyyy");
                        }

                        #endregion Fix Shift
                    }


                    #endregion Shift


                    DATA_OK = true;

                    #endregion Validation
                }
                if (DATA_OK == true)
                {


                    //for add new a new auto Employee ID will be added

                    #region NEW ID GENERATE

                    string strEmpSystemID = "";
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_BASIC", out strEmpSystemID);
                    string syspad = GetPadding(strEmpSystemID);
                    data.SystemId = DateTime.Now.ToString("yy") + syspad;

                    //string Prefix = null;
                    //string incrementvalue = string.Empty;
                    //objApp.GetPlantPrefix(para.PlantId, out Prefix);
                    //objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), para.PlantId + "EMP_BASIC", out incrementvalue);
                    //string pad = GetPadding(incrementvalue);
                    //data.EmployeeId = Prefix + DateTime.Now.ToString("yy") + pad;



                    #region empCode new
                    string Prefix = null;
                    objEL.GetEmpCodeGenSetting(para.PlantId, data.EmployeeCodeTypeId, out Prefix, out dsEmpCodeGenSetting);
                    data.EmployeeId = Prefix + data.SystemId;


                    if (string.IsNullOrEmpty(data.EmployeeCode))
                    {
                        objEL.GetMaxEmpCode(para.PlantId, data.EmployeeCodeTypeId, out dsMaxEmpCode);

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
                                                data.EmployeeCode = code.ToString();
                                            }
                                            else
                                            {
                                                Exception ex = new Exception("Employee code start value doesn't define in Employee Code Generation...");
                                                throw ex;
                                            }
                                        }
                                        else
                                        {
                                            data.EmployeeCode = v.ToString();
                                        }
                                    }
                                }
                                else
                                {
                                    data.EmployeeCode = data.SystemId;
                                }
                            }
                            if (dsEmpCodeGenSetting.Tables[0].Rows[0]["IsAutoEmpCodeWithPrefix"].ToString() == "True")
                            {
                                data.EmployeeCode = Prefix + data.EmployeeCode;
                            }

                        }
                    }
                    #endregion

                    #region empCode old
                    ///empCode old
                    //if (string.IsNullOrEmpty(data.EmployeeCode))
                    //{
                    //    DataSet dsEC = null;
                    //    DataSet dsECSV = null;

                    //    objEL.getEmpCodeAuto(para.PlantId, out dsEC);
                    //    objEL.GetEmpCodeStartValue(para.PlantId, out dsECSV);
                    //    objEmpLoad.GetDefaultPlantWiseHRMSSetting(para.CompanyGroupId, para.PlantId, out dsHRsettin);

                    //    if (dsHRsettin.Tables[0].Rows.Count > 0)
                    //    {
                    //        if (Convert.ToBoolean(dsHRsettin.Tables[0].Rows[0]["IsEmployeeCodeOpenField"]) == false)
                    //        {
                    //            if (dsHRsettin.Tables[0].Rows[0]["EmployeeCodeStart"].ToString() == "AutoIncrement")
                    //            {
                    //                if (dsEC.Tables[0].Rows.Count > 0)
                    //                {
                    //                    int v = Convert.ToInt32(bplib.clsWebLib.GetNumData(dsEC.Tables[0].Rows[0]["c"].ToString())) + 1;
                    //                    if (v == 1)
                    //                    {
                    //                        if (Convert.ToInt32(bplib.clsWebLib.GetNumData(dsECSV.Tables[0].Rows[0]["EmpCodeStartValue"].ToString())) != 0)
                    //                        {
                    //                            int code = Convert.ToInt32(bplib.clsWebLib.GetNumData(dsECSV.Tables[0].Rows[0]["EmpCodeStartValue"].ToString())) + 1;
                    //                            data.EmployeeCode = code.ToString();
                    //                        }
                    //                        else
                    //                        {
                    //                            Exception ex = new Exception("Employee code start value doesn't define in plant wise setting...");
                    //                            throw ex;
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        data.EmployeeCode = v.ToString();
                    //                    }
                    //                }
                    //            }
                    //            else
                    //            {
                    //                data.EmployeeCode = data.SystemId;
                    //            }
                    //        }
                    //        if (dsHRsettin.Tables[0].Rows[0]["IsAutoEmpCodeWithPrefix"].ToString() == "True")
                    //        {
                    //            data.EmployeeCode = Prefix + data.EmployeeCode;
                    //        }

                    //    }
                    //}
                    #endregion



                    //if (objEmpLoad.DuplicateEmployeeCode(para.CompanyGroupId, para.CompanyId, para.PlantId, data.SystemId, data.EmployeeCode, EmployeeCodeCheckLevel) == false)
                    //{
                    //    Exception ex = new Exception("This EmployeeCode already exist.........EmployeeCode must be unique");
                    //    throw ex;
                    //}

                    if (objEmpLoad.DuplicateEmployeeCodeWithInGroup(para.PlantId, data.SystemId, data.EmployeeCode, data.EmployeeCodeTypeId) == false)
                    {
                        Exception ex = new Exception("This EmployeeCode already exist.........EmployeeCode must be unique");
                        throw ex;
                    }

                    //if (objEmpLoad.DuplicateEmployeeCode(para.CompanyGroupId, para.CompanyId, para.PlantId, data.SystemId, data.EmployeeCode, EmployeeCodeCheckLevel) == false)
                    //{
                    //    Exception ex = new Exception("This EmployeeCode already exist.........EmployeeCode must be unique");
                    //    throw ex;
                    //}

                    string strShiftAssSystemID = "";
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_SHIFT_ASSIGN", out strShiftAssSystemID);
                    strShiftAssSystemID = "S" + "-" + strShiftAssSystemID;

                    string strWorkOffSystemID = "";
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_WEEKOFF_BYDAY", out strWorkOffSystemID);
                    strWorkOffSystemID = "W" + "-" + strWorkOffSystemID;

                    string strEmpPinSysId = "";
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_PIN", out strEmpPinSysId);


                    string strEmpJbLcSystemID = "";
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_JOB_LOC", out strEmpJbLcSystemID);
                    strEmpJbLcSystemID = "J" + "-" + strEmpJbLcSystemID;


                    #endregion NEW ID GENERATE

                    #region DataSet

                    objEmpLoad.SaveEmployeeInformation(para.CompanyGroupId, para.CompanyId, para.PlantId, data.SystemId, out dsLocal);
                    dtLocal = dsLocal.Tables[0];
                    dvLocal = new DataView();
                    dvLocal.Table = dtLocal;
                    dvLocal.RowFilter = "SystemID = '" + data.SystemId + "'";


                    objEmpLoad.SaveEmpDateWiseJobLocation(data.SystemId, strEmpJbLcSystemID, out dsEmpJbLc);
                    dtEmpJbLc = dsEmpJbLc.Tables[0];
                    dvEmpJbLc = new DataView();
                    dvEmpJbLc.Table = dtEmpJbLc;
                    dvEmpJbLc.RowFilter = "SystemID = '" + strEmpJbLcSystemID + "'";



                    objEmpLoad.SaveEmployeeShiftAssign(data.SystemId, strShiftAssSystemID, out dsShiftAssign);
                    dtShiftAssign = dsShiftAssign.Tables[0];
                    dvShiftAssign = new DataView();
                    dvShiftAssign.Table = dtShiftAssign;
                    dvShiftAssign.RowFilter = "SystemID = '" + strShiftAssSystemID + "'";

                    //if (radFixShift.Checked == true)
                    //{
                    objEmpLoad.SaveEmployeeWeekOffByDay(data.SystemId, strWorkOffSystemID, out dsWeekOffByDay);
                    dtWeekOffByDay = dsWeekOffByDay.Tables[0];
                    dvWeekOffByDay = new DataView();
                    dvWeekOffByDay.Table = dtWeekOffByDay;
                    dvWeekOffByDay.RowFilter = "SystemID = '" + strWorkOffSystemID + "'";
                    // }

                    objEmpLoad.SaveEmployeePIN(data.SystemId, out dsEmpPin);
                    dtEmpPin = dsEmpPin.Tables[0];
                    dvEmpPin = new DataView();
                    dvEmpPin.Table = dtEmpPin;
                    dvEmpPin.RowFilter = "EmployeeId = '" + data.SystemId + "'";


                    objEmpLoad.SaveEmployeeRef(data.SystemId, out dsEmpRef);
                    dtEmpRef = dsEmpRef.Tables[0];
                    dvEmpRef = new DataView();
                    dvEmpRef.Table = dtEmpRef;
                    dvEmpRef.RowFilter = "EmpSystemID = '" + data.SystemId + "'";

                    #endregion DataSet

                    #region Employee Information

                    if (dvLocal.Count == 0)
                    {// Add new block

                        drLocal = dtLocal.NewRow();
                        UpdateEmployeeInformationDataRow("ADDNEW", data, para, ref drLocal);
                        dtLocal.Rows.Add(drLocal);
                    }
                    else
                    {//edit block

                        drLocal = dvLocal[0].Row;
                        drLocal.BeginEdit();
                        UpdateEmployeeInformationDataRow("EDIT", data, para, ref drLocal);
                        drLocal.EndEdit();
                        IsEdit = false;
                    }
                    dvLocal.RowFilter = null;

                    #endregion Employee Information

                    #region Employee JOB Location

                    if (dvEmpJbLc.Count == 0)
                    {// Add new block
                        drEmpJbLc = dtEmpJbLc.NewRow();
                        UpdateEmpDateWiseJobLocation("ADDNEW", strEmpJbLcSystemID, data, para, ref drEmpJbLc);
                        dtEmpJbLc.Rows.Add(drEmpJbLc);
                    }
                    else
                    {//edit block
                        drEmpJbLc = dvEmpJbLc[0].Row;
                        drEmpJbLc.BeginEdit();
                        UpdateEmpDateWiseJobLocation("EDIT", strEmpJbLcSystemID, data, para, ref drEmpJbLc);
                        drEmpJbLc.EndEdit();
                    }
                    dvEmpJbLc.RowFilter = null;

                    #endregion Employee JOB Location

                    #region Employee Shift Assign

                    if (dvShiftAssign.Count == 0)
                    {// Add new block
                        drShiftAssign = dtShiftAssign.NewRow();
                        UpdateEmployeeShiftAssignDataRow("ADDNEW", strShiftAssSystemID, data, para, ref drShiftAssign);
                        dtShiftAssign.Rows.Add(drShiftAssign);
                    }
                    //else
                    //{//edit block
                    //    drShiftAssign = dvShiftAssign[0].Row;
                    //    drShiftAssign.BeginEdit();
                    //    UpdateEmployeeShiftAssignDataRow("EDIT", ref drShiftAssign);
                    //    drShiftAssign.EndEdit();
                    //}
                    dvShiftAssign.RowFilter = null;

                    #endregion Employee Shift Assign

                    #region Employee Week Off By Day

                    //if (radFixShift.Checked == true)
                    //{
                    if (dvWeekOffByDay.Count == 0)
                    {// Add new block
                        drWeekOffByDay = dtWeekOffByDay.NewRow();
                        UpdateEmployeeWeekOffByDayDataRow("ADDNEW", strWorkOffSystemID, data, para, ref drWeekOffByDay);
                        dtWeekOffByDay.Rows.Add(drWeekOffByDay);
                    }
                    else
                    {//edit block
                        drWeekOffByDay = dvWeekOffByDay[0].Row;
                        drWeekOffByDay.BeginEdit();
                        UpdateEmployeeWeekOffByDayDataRow("EDIT", strWorkOffSystemID, data, para, ref drWeekOffByDay);
                        drWeekOffByDay.EndEdit();
                    }
                    dvWeekOffByDay.RowFilter = null;
                    //}

                    #endregion Employee Week Off By Day

                    #region Employee PIN


                    if (dvEmpPin.Count == 0)
                    {// Add new block
                        drEmpPin = dtEmpPin.NewRow();
                        UpdateEmployeePIN("ADDNEW", data.SystemId, para, ref drEmpPin);
                        dtEmpPin.Rows.Add(drEmpPin);
                    }
                    else
                    {//edit block
                        drEmpPin = dvEmpPin[0].Row;
                        drEmpPin.BeginEdit();
                        UpdateEmployeePIN("EDIT", data.SystemId, para, ref drEmpPin);
                        drEmpPin.EndEdit();
                    }
                    dvEmpPin.RowFilter = null;


                    #endregion Employee PIN

                    #region Employee Ref


                    if (dvEmpRef.Count == 0)
                    {// Add new block
                        drEmpRef = dtEmpRef.NewRow();
                        UpdateEmployeeRef("ADDNEW", data.SystemId, empRef, para, ref drEmpRef);
                        dtEmpRef.Rows.Add(drEmpRef);
                    }
                    else
                    {//edit block
                        drEmpRef = dvEmpRef[0].Row;
                        drEmpRef.BeginEdit();
                        UpdateEmployeeRef("EDIT", data.SystemId, empRef, para, ref drEmpRef);
                        drEmpRef.EndEdit();
                    }
                    dvEmpRef.RowFilter = null;


                    #endregion Employee Ref

                    #region Employee Weekly Off

                    //DataSet dsCTOD = null;
                    //GetCutOffDate(para.PlantId, out dsCTOD);
                    //WeekOff["EmpSystemId"] = data.SystemId;

                    //if (Convert.ToDateTime(dsCTOD.Tables[0].Rows[0]["CutOffDate"].ToString()) < data.DOJ)
                    //{
                    //    WeekOff["EffectiveDate"] = data.DOJ;
                    //}
                    //else
                    //{
                    //    WeekOff["EffectiveDate"] = Convert.ToDateTime(dsCTOD.Tables[0].Rows[0]["CutOffDate"].ToString());
                    //}

                    //string TableName = "dbo.EmployeeWeeklyOff";
                    //DataSet dsWeeklyOff;
                    //ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    //con.OpenDataSetThroughAdapter("Select * from dbo.EmployeeWeeklyOff where Id = '" + WeekOff["Id"] + "'", out dsWeeklyOff, false, "1");

                    //string _Id = "";
                    //if (dsWeeklyOff.Tables[0].Rows.Count == 0)
                    //{
                    //    bplib.clsGenID genid = new bplib.clsGenID();
                    //    genid.GenID(TableName, out _Id);

                    //    WeekOff["Id"] = _Id;
                    //    AddNewRow(dsWeeklyOff.Tables[0], WeekOff);
                    //}


                    #endregion

                    #region Employee Non Eligible OT
                    //DataSet dsCTOD = null;
                    //GetCutOffDate(para.PlantId, out dsCTOD);
                    //OT["EmpSystemId"] = data.SystemId;
                    //if (Convert.ToDateTime(dsCTOD.Tables[0].Rows[0]["CutOffDate"].ToString()) < data.DOJ)
                    //{
                    //    OT["EffectiveDate"] = data.DOJ;
                    //}
                    //else
                    //{
                    //    OT["EffectiveDate"] = Convert.ToDateTime(dsCTOD.Tables[0].Rows[0]["CutOffDate"].ToString());
                    //}

                    //string TableName1 = "dbo.EmployeeWeeklyOff";
                    //DataSet dsNonOT;
                    //ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                    //conn.OpenDataSetThroughAdapter("Select * from dbo.NonEligibleOT where Id = '" + OT["Id"] + "'", out dsNonOT, false, "1");

                    //if (Boolean.Parse(OT["Exclude"].ToString()) == true)
                    //{
                    //    string _Id1 = "";
                    //    if (dsNonOT.Tables[0].Rows.Count == 0)
                    //    {
                    //        bplib.clsGenID genid = new bplib.clsGenID();
                    //        genid.GenID(TableName1, out _Id1);

                    //        OT["Id"] = _Id1;
                    //        AddNewRow(dsNonOT.Tables[0], OT);
                    //    }
                    //}

                    #endregion

                    #region EmpBank
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsEmpBankMaster;
                    string sql = "SELECT * FROM [dbo].[EmployeeBankInfo] WHERE RowID='" + empBank["RowID"] + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsEmpBankMaster, false, "1");
                    if (dsEmpBankMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsEmpBankMaster.Tables[0].NewRow();

                        dr["EmpSystemID"] = data.SystemId;
                        dr["BankSystemID"] = empBank["BankSystemID"];
                        dr["BankBranchId"] = empBank["BankBranchId"];
                        dr["BankAccNo"] = empBank["BankAccNo"];
                        dr["SalaryPercentage"] = empBank["SalaryPercentage"];
                        dr["IFSCCode"] = empBank["IFSCCode"];
                        dr["IsApproved"] = false;
                        dr["ApprovedDateTime"] = DBNull.Value;

                        dr["AddedBy"] = para.AddedBy;
                        dr["DateAdded"] = DateTime.Now;

                        dsEmpBankMaster.Tables[0].Rows.Add(dr);
                    }


                    #endregion EmpBank

                    objApp.SaveDataSets(dsLocal, dsShiftAssign, dsEmpJbLc, dsWeekOffByDay, dsEmpPin, dsEmpRef, dsEmpBankMaster); // , dsWeeklyOff, dsNonOT


                    #region att process only for new emp


                    if (dsHRsettin.Tables[0].Rows.Count > 0)
                    {
                        string EmpId = "'" + data.SystemId.Replace(" ", "','") + "'";//replaced with ""
                        string _ap = dsHRsettin.Tables[0].Rows[0]["CallAttendanceAfterProfileEntry"].ToString();
                        if (Convert.ToBoolean(_ap))
                        {
                            string _ed = txtEmpFixShiftEffectiveDate;
                            clsAttendance.AttendanceProcessAplos objAttdn = new clsAttendance.AttendanceProcessAplos();
                            DateTime FromDate = Convert.ToDateTime(_ed);
                            DateTime ToDate = DateTime.Now;
                            while (FromDate <= ToDate)
                            {
                                AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                                objAttdn.SaveTotal(para.PlantId, FromDate.ToString("dd-MMM-yyyy"), EmpId, false);
                                FromDate = FromDate.AddDays(1);
                            }
                        }//ap 

                    }//xx//insert
                    #endregion
                    #region Year and From To Date Finding

                    DataSet YearFinding;
                    string YearId = "";
                    string date = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                    FindLeaveYear(date, out YearFinding, para.PlantId);
                    if (YearFinding.Tables[0].Rows.Count > 0)
                    {
                        YearId = YearFinding.Tables[0].Rows[0][@"Id"].ToString();
                    }

                    DataTable DateTbl;
                    var str = @"select FromDate,ToDate from LeaveYearDefination where id='" + YearId + "'";
                    DateTbl = _sqlRepository.GetDataTable(str);
                    string From = "", To = "";
                    if (DateTbl.Rows.Count > 0)
                    {
                        From = DateTbl.Rows[0]["FromDate"].ToString();
                        To = DateTbl.Rows[0]["ToDate"].ToString();
                    }

                    #endregion

                    #region Saving Logic 

                    DataSet dsRef, dsSource;
                    var sqlx = @"select * from AnnualLeaveDataCurrent where PlantId='" + para.PlantId + "'and LeaveYearId='" + YearId + "' AND EmployeeId='" + data.SystemId + "'";
                    objCon.OpenDataSetThroughAdapter(sqlx, out dsRef, false, false, "", "1");

                    string LvTypeId = "";
                    LeaveSourceDataGeneration(From, To, out dsSource, para.PlantId, YearId, data.SystemId);
                    if (dsSource.Tables[0].Rows.Count > 0)
                    {
                        clsGenID genid = new clsGenID();
                        genid.GenID("AnnualLeaveDataCurrent", out string _Idx);

                        for (int i = 0; i < dsSource.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"EmpId"]).ToString();
                            string LvYearId = clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"LeaveYearId"]).ToString();
                            LvTypeId = clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"LeaveTypeId"]).ToString();
                            decimal Availed = Convert.ToDecimal(clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"Availed"]).ToString());
                            decimal Earned = Convert.ToDecimal(clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"Earned"]).ToString());

                            dsRef.Tables[0].DefaultView.RowFilter = @"EmployeeId='" + EmpId + "' AND LeaveTypeId='" + LvTypeId + "' AND LeaveYearId='" + LvYearId + "'";
                            if (dsRef.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow drx = dsRef.Tables[0].NewRow();
                                drx["Id"] = "AC" + _Idx + "-" + i;
                                drx["EmployeeId"] = EmpId;
                                drx["LeaveYearId"] = LvYearId;
                                drx["PlantId"] = para.PlantId;
                                drx["LeaveTypeId"] = LvTypeId;
                                drx["Opening"] = 0;
                                drx["Availed"] = Availed;
                                drx["Earned"] = Earned;
                                drx["AddedBy"] = para.AddedBy;
                                drx["AddedDate"] = Convert.ToDateTime(DateTime.Now);
                                drx["AddedFromIP"] = para.AddedFromIP;
                                dsRef.Tables[0].Rows.Add(drx);
                            }
                        }
                        objApp.SaveDataSets(dsRef);
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objApp = null;
                drLocal = null;
                dvLocal = null;
                dtLocal = null;
                dsLocal = null;

            }
        }//End Function

        #region Leave Process Source Data

        public void FindLeaveYear(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select ld.* from LeaveYearDefination ld left 
                join LeaveYearDefinationPlantChild pc on
                pc.LeaveYearDefinationId=ld.Id where 
                FromDate<='" + Date + "' and '" + Date + "'<=ToDate and pc.PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public void LeaveSourceDataGeneration(string From, string To, out DataSet ds, string Plant, string YearId, string empId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select dd.*,case when lpd.EncashWorkingDaysQty > 0 
                then dd.EarnDays/lpd.EncashWorkingDaysQty else 
				0 END as Earned	 
			    from (select e.SystemId as EmpId,e.EmployeeCode,ld.Id as 
                LeaveYearId,ld.UserName as LeaveYear,p.UserName as Plant,
                lt.UserName as LeaveType,lt.Id as LeaveTypeId,lt.Code,
                isnull(Masterx.EarnDays,'0')+ isnull(md.Earned,'0')EarnDays,
                Availed= (isnull(Info.AvailedLeave,'0')+isnull(md.Availed,'0')),
			    Info.EmpTypeId,Info.LeavePolicyMasterId
                from LeaveYearDefination ld 
                left join LeaveYearDefinationPlantChild pc on 
				pc.LeaveYearDefinationId=ld.Id and pc.PlantId='" + Plant + @"'
                left join org.Plant p on p.Id=pc.PlantId
				left join org.Company c on c.Id=p.CompanyId
                left join org.CompanyGroup cg on cg.Id=c.CompanyGroupId
                left join LeaveType lt on lt.CompanyGroupId=cg.Id 
                left join EmployeeInformation e on e.PlantId=p.Id
                left join ManualLeaveData md on md.EmployeeId=e.SystemId
				and md.LeaveYearId=ld.Id and 
				md.LeaveTypeId=lt.Id and md.PlantId='" + Plant + @"'
                left join AnnualLeaveDataCurrent ac on ac.EmployeeId=e.SystemId
				and ac.LeaveYearId=ld.Id and ac.LeaveTypeId=lt.Id and ac.PlantId='" + Plant + @"'
				left join
				(
				select a.EmpSystemID,SUM(a.LvValue)AvailedLeave,A.DayStatus,a.PlantID,dc.EmpTypeId,
                dxc.LeavePolicyMasterId				
				from AttdnProcessData a left join EmployeeInformation ei on a.EmpSystemID=ei.SystemId
				left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = 
		        ei.LegalDesignationId
				left join mst.DesignationMaster 
				dm on dm.Id = ddm.DesignationMasterId
				left join scs.DesignationMasterConfiguration dxc on dxc.DesignationMasterId=dm.Id
				and dxc.PlantId=ei.PlantId
				left join DayStatusPlantChild 
				dc on dc.EmpTypeId=dm.EmployeeCategoryId
				and dc.PlantId=ei.PlantId
				left join DayStatusHeader dh on dh.Id=dc.headerId
				left join DayTypeWithValues dt on dt.HeaderId=dh.Id
				and dt.DayType=a.DayStatus				
				where dt.HeaderId is not null and 
				a.LvValue<>0 and ei.EmployeeStatus='Active'
				and 
				a.workdate between '" + From + @"' and '" + To + @"'
				and ei.PlantId='" + Plant + @"'
				group by A.EmpSystemID,a.DayStatus,a.PlantID,dc.EmpTypeId,
                dxc.LeavePolicyMasterId ) as Info
				on Info.EmpSystemID=e.SystemId and Info.PlantID=e.PlantId 
				and Info.DayStatus=lt.Code
                left join (SELECT EmpSystemID,SUM(l.EarnValue)EarnDays,T.Id as LeaveId,ei.PlantId
                FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd   ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId
                AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + From + "' and '" + To + @"'
                and EI.PlantID='" + Plant + @"' and t.LeaveType='Earn'
                group by EmpSystemID,t.Id,ei.plantid
                ) as Masterx on Masterx.EmpSystemID=e.SystemId 
				and e.PlantId=Masterx.PlantId and
                Masterx.LeaveId=lt.Id          
                where p.Id='" + Plant + "' and ld.Id='" + YearId + @"' and			
                e.EmployeeStatus='Active' and e.SystemId='" + empId + @"') as dd
                left join LeavePolicyDetail lpd on lpd.LPMSystemID=dd.LeavePolicyMasterId
				and lpd.LTSystemID=dd.LeaveTypeId	
                order by dd.EmpId,dd.LeaveTypeId";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        #endregion

        public void GetCutOffDate(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT Id, PlantId, ModuleName, FORMAT(CutOffDate,'dd-MMM-yyyy') CutOffDate FROM [SCS].[OpeningBalanceCutOffDate] WHERE PlantId='" + PlantId + "' and ModuleName='HR'";

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

        public void GetDefaultPlantWiseHRMSSetting(string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM PlantWiseHRMSSetting WHERE GroupID = '" + sGroupID + @"'
                            AND PlantID = '" + sPlantID + @"'";

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

        public void PlantWiseDOJDays(string plantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT  PastDOJDaysAllowed FROM dbo.PlantWiseHRMSSetting WHERE PlantId='" + plantId + @"' AND IsPastDOJAllowed=1";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PlantWiseLock(string plantId, string workDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT FORMAT(MAX(LockedDate),'dd-MMM-yyyy') LockedDate FROM PlantWiseAttendanceLock where PlantId='" + plantId + "' And LockedDate>'" + workDate + "' and isactive=1";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateEmployeeInformationDataRow(string OPN_FLAG, EmployeeInformation data, IdentityParameter para, ref DataRow drLocal)
        {
            try
            {
                DataSet dsLocal = null;
                clsEmployeeLoad objEmpBasic = new clsEmployeeLoad();
                objEmpBasic.GetDesignationGroupByDesignationId(para.CompanyGroupId, data.DesignationSystemID, data.PlantID, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    data.DesignationGroupID = dsLocal.Tables[0].Rows[0]["ID"].ToString();

                }

                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = data.SystemId;
                    drLocal["EmployeeId"] = bplib.clsWebLib.RetValidLen(data.EmployeeId);

                    drLocal["EmployeeCode"] = bplib.clsWebLib.RetValidLen(data.EmployeeCode);
                    drLocal["EmployeeStatus"] = "Active";

                    drLocal["AddedBy"] = para.AddedBy;
                    drLocal["DateAdded"] = DateTime.Now;

                }

                if (OPN_FLAG == "EDIT")// Change Employee Code
                {
                    drLocal["Employeecode"] = bplib.clsWebLib.RetValidLen(data.EmployeeCode);
                }
                string eca = SplitAlphaString(data.EmployeeCode);
                string LastCode = data.EmployeeCode.Substring(eca.Length);
                drLocal["EmployeeCodePreFix"] = eca;
                if (LastCode.Length > 0)
                {
                    drLocal["EmployeeCodeNumeric"] = data.EmployeeCode.Substring(eca.Length);
                }
                else
                {
                    drLocal["EmployeeCodeNumeric"] = 0;
                }
                drLocal["Salutation"] = bplib.clsWebLib.RetValidLen(data.Salutation);
                drLocal["FirstName"] = bplib.clsWebLib.RetValidLen(data.FirstName.Trim().ToUpper());
                SetColumnValue(ref drLocal, "MiddleName", data.MiddleName, true);
                SetColumnValue(ref drLocal, "LastName", data.LastName, true);
                drLocal["EmployeeName"] = bplib.clsWebLib.RetValidLen(data.EmployeeName.Trim().ToUpper());
                drLocal["EmpType"] = bplib.clsWebLib.RetValidLen(data.EmpType);

                drLocal["JobLocationID"] = bplib.clsWebLib.RetValidLen(data.JobLocationID);
                drLocal["GroupID"] = bplib.clsWebLib.RetValidLen(para.CompanyGroupId);
                drLocal["CompanyID"] = bplib.clsWebLib.RetValidLen(para.CompanyId);
                drLocal["PlantID"] = bplib.clsWebLib.RetValidLen(para.PlantId);
                drLocal["PaymentMode"] = bplib.clsWebLib.RetValidLen(data.PaymentMode);

                drLocal["DOB"] = bplib.clsWebLib.DateData_AppToDB(data.DOB, bplib.clsWebLib.DB_DATE_FORMAT);
                drLocal["BirthdayCelebrationDate"] = bplib.clsWebLib.DateData_AppToDB(data.BirthdayCelebrationDate, bplib.clsWebLib.DB_DATE_FORMAT);
                drLocal["DOJ"] = bplib.clsWebLib.DateData_AppToDB(data.DOJ, bplib.clsWebLib.DB_DATE_FORMAT);
                drLocal["SalaryPercentage"] = 0;
                drLocal["RegisterFP"] = false;
                drLocal["IsAttdnProcBaseOnDeviceData"] = false;
                drLocal["IsEntryComplete"] = false;
                drLocal["FirstTimeLock"] = false;
                drLocal["RegisterProximate"] = false;
                drLocal["IsApproved"] = false;
                drLocal["ApplyingAsFresher"] = false;
                drLocal["IsKnownPerson"] = false;
                drLocal["IsImage"] = false;
                drLocal["IsAccessible"] = false;
                drLocal["PreviouslyWorkedHere"] = false;
                drLocal["AnyRelativeWorkedHere"] = false;
                drLocal["NumberOfKnownPerson"] = 0;
                drLocal["EntryLevel"] = data.EntryLevel;
                //drLocal["Ref1CellPhnNo"] = data.Ref1CellPhnNo;
                drLocal["DOCIsDay"] = data.DOCIsDay;
                drLocal["DOCDay"] = data.DOCDay;
                drLocal["DOCIsMonth"] = data.DOCIsMonth;
                drLocal["DOCMonth"] = data.DOCMonth;
                drLocal["DOC"] = bplib.clsWebLib.DateData_AppToDB(data.DOC, bplib.clsWebLib.DB_DATE_FORMAT);
                drLocal["IsConfirmed"] = data.IsConfirmed;

                drLocal["NationalID"] = bplib.clsWebLib.RetValidLen(data.NationalID.Trim());

                drLocal["CitizenID"] = data.CitizenID;
                drLocal["CivilStatusID"] = data.CivilStatusID;
                drLocal["GenderID"] = data.GenderID;

                drLocal["PositionId"] = data.PositionId;
                drLocal["IsDirect"] = data.IsDirect;

                drLocal["BudgetCode"] = data.BudgetCode;

                drLocal["GivenDesignationId"] = data.GivenDesignationId;
                drLocal["LegalDesignationId"] = data.LegalDesignationId;
                drLocal["EmployeeCodeTypeId"] = data.EmployeeCodeTypeId;
                drLocal["VendorId"] = data.VendorId;

                drLocal["CardNumber"] = data.CardNumber;
                drLocal["ApprovalAuthorityId"] = data.ApprovalAuthorityId;

                //BudgetCode related column data 

                drLocal["EmployeeGroupSystemID"] = data.EmployeeGroupSystemID;
                drLocal["UnitID"] = data.UnitID;
                drLocal["DivisionID"] = data.DivisionID;
                drLocal["SubdivisionID"] = data.SubdivisionID;
                drLocal["DepartmentID"] = data.DepartmentID;
                drLocal["SectionID"] = data.SectionID;
                drLocal["SubSectionID"] = data.SubSectionID;
                drLocal["LineID"] = data.LineID;
                drLocal["BudgetCategoryID"] = data.BudgetCategoryID;
                drLocal["EmployeeCategorySystemID"] = data.EmployeeCategorySystemID;
                drLocal["DesignationGroupID"] = data.DesignationGroupID;
                drLocal["DesignationSystemID"] = data.DesignationSystemID;
                drLocal["RelativeSystemId"] = data.RelativeSystemId;
                drLocal["ProfileShiftId"] = data.FixSystemID;
                drLocal["ResidenceGroupId"] = data.ResidenceGroupId;
                drLocal["TransportGroupId"] = data.TransportGroupId;
                drLocal["EmploymentType"] = data.EmploymentType;
                drLocal["ExcludeOT"] = false;
                drLocal["isLeaveOnDOC"] = false;

                if (!string.IsNullOrEmpty(data.RelativeSystemId))
                {
                    drLocal["AnyRelativeWorkedHere"] = true;
                }
                else
                {
                    drLocal["AnyRelativeWorkedHere"] = false;
                }

                objEmpBasic.getPlantConfig(para.PlantId, out DataSet dsPC);
                if (dsPC.Tables[0].Rows.Count > 0)
                {
                    if (dsPC.Tables[0].Rows[0]["Operation"].ToString() == "Operation Master")
                    {
                        if (string.IsNullOrEmpty(data.OperationMasterID) || string.IsNullOrWhiteSpace(data.OperationMasterID))
                        {
                            drLocal["OperationMasterID"] = DBNull.Value;
                        }
                        else
                        {
                            drLocal["OperationMasterID"] = data.OperationMasterID;
                        }
                        drLocal["OperationVariationId"] = DBNull.Value;
                    }
                    if (dsPC.Tables[0].Rows[0]["Operation"].ToString() == "Operation Variation")
                    {
                        if (string.IsNullOrEmpty(data.OperationVariationId) || string.IsNullOrWhiteSpace(data.OperationVariationId))
                        {
                            drLocal["OperationVariationId"] = DBNull.Value;
                        }
                        else
                        {
                            drLocal["OperationVariationId"] = data.OperationVariationId;
                        }

                        drLocal["OperationMasterID"] = DBNull.Value;
                    }
                }


                drLocal["UpdatedBy"] = para.UpdatedBy;
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
        void SetColumnValue(ref DataRow drLocal, string colname, string colvalue, bool IsUpper = false)
        {
            if (string.IsNullOrEmpty(colvalue))
            {
                drLocal[colname] = DBNull.Value;
            }
            else
            {
                drLocal[colname] = IsUpper ? colvalue.Trim().ToUpper() : colvalue.Trim();
            }
        }
        private void UpdateEmployeePIN(string OPN_FLAG, string lblEmpSystemId, IdentityParameter para, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["Id"] = bplib.clsWebLib.RetValidLen(lblEmpSystemId.ToString().Trim());
                    drLocal["EmployeeId"] = bplib.clsWebLib.RetValidLen(lblEmpSystemId.ToString().Trim());

                    drLocal["PIN"] = new Random().Next(111111, 999999).ToString();

                    drLocal["AddedBy"] = para.AddedBy;
                    drLocal["AddedDate"] = DateTime.Now;
                }

                drLocal["IsSalaryStructure"] = false;
                drLocal["IsPaySlip"] = false;
                drLocal["IsMonthlyAttendance"] = false;
                drLocal["IsDailyAttendanceNotification"] = false;
                drLocal["IsSalaryProcessConfirmationNotification"] = false;
                drLocal["IsSalaryDisbursementNotification"] = false;
                drLocal["IsIncrementNotification"] = false;
                drLocal["IsPromotionNotification"] = false;
                drLocal["IsLeaveNotification"] = false;

                drLocal["UpdatedBy"] = para.UpdatedBy;
                drLocal["UpdatedDate"] = DateTime.Now;
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

        private void UpdateEmployeeRef(string OPN_FLAG, string lblEmpSystemId, EmpReferenceInformation data, IdentityParameter para, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(lblEmpSystemId.ToString().Trim());
                    drLocal["EmpSystemID"] = bplib.clsWebLib.RetValidLen(lblEmpSystemId.ToString().Trim());


                    drLocal["AddedBy"] = para.AddedBy;
                    drLocal["DateAdded"] = DateTime.Now;
                }
                drLocal["RefEmpSystemID"] = data.RefEmpSystemID;
                drLocal["Ref1Name"] = data.Ref1Name;
                drLocal["Ref1EmployerName"] = data.Ref1EmployerName;
                drLocal["Ref1EmployerAddress"] = data.Ref1EmployerAddress;
                drLocal["Ref1Designation"] = data.Ref1Designation;
                drLocal["Ref1CellPhnNo"] = data.Ref1CellPhnNo;
                drLocal["Ref1TelePhnNo"] = data.Ref1TelePhnNo;
                drLocal["Ref1Email"] = data.Ref1Email;
                drLocal["Ref1Address"] = data.Ref1Address;
                drLocal["Ref2Name"] = data.Ref2Name;
                drLocal["Ref2EmployerName"] = data.Ref2EmployerName;
                drLocal["Ref2EmployerAddress"] = data.Ref2EmployerAddress;
                drLocal["Ref2Designation"] = data.Ref2Designation;
                drLocal["Ref2CellPhnNo"] = data.Ref2CellPhnNo;
                drLocal["Ref2TelePhnNo"] = data.Ref2TelePhnNo;
                drLocal["Ref2Email"] = data.Ref2Email;
                drLocal["Ref2Address"] = data.Ref2Address;



                drLocal["UpdatedBy"] = para.UpdatedBy;
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


        private string GetPadding(string iv)
        {
            while (iv.Length < bplib.clsWebLib.EMP_BASIC_PK_PAD)
            {
                iv = "0" + iv;
            }
            return iv;
        }

        private void UpdateEmpDateWiseJobLocation(string OPN_FLAG, string strEmpJbLcSystemID, EmployeeInformation data, IdentityParameter para, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(strEmpJbLcSystemID.ToString().Trim());
                    drLocal["EmpSystemID"] = bplib.clsWebLib.RetValidLen(data.SystemId);

                    drLocal["AddedBy"] = para.AddedBy;
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["JobLcSystemID"] = bplib.clsWebLib.RetValidLen(data.JobLocationID);
                drLocal["EffectiveDate"] = bplib.clsWebLib.DateData_AppToDB(txtEmpFixShiftEffectiveDate, bplib.clsWebLib.DB_DATE_FORMAT);

                drLocal["UpdatedBy"] = para.UpdatedBy;
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

        private void UpdateEmployeeShiftAssignDataRow(string OPN_FLAG, string strShiftAssSystemID, EmployeeInformation data, IdentityParameter para, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(strShiftAssSystemID);
                    drLocal["EmpSystemID"] = bplib.clsWebLib.RetValidLen(data.SystemId);

                    drLocal["AddedBy"] = para.AddedBy;
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["IsFix"] = true;
                drLocal["IsRoster"] = false;

                drLocal["EffectiveDate"] = txtEmpFixShiftEffectiveDate;
                drLocal["FixSystemID"] = data.FixSystemID;

                drLocal["UpdatedBy"] = para.UpdatedBy;
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

        private void UpdateEmployeeWeekOffByDayDataRow(string OPN_FLAG, string strWorkOffSystemID, EmployeeInformation data, IdentityParameter para, ref DataRow drLocal)
        {
            try
            {

                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = strWorkOffSystemID;
                    drLocal["EmpSystemID"] = data.SystemId;

                    drLocal["AddedBy"] = para.AddedBy;
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["FixSystemID"] = data.FixSystemID;
                drLocal["EffectiveDate"] = txtEmpFixShiftEffectiveDate;

                drLocal["AlignWithCC"] = true;
                drLocal["IndividualWeekOff"] = false;

                drLocal["UpdatedBy"] = para.UpdatedBy; ;
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


        public IEnumerable<object> GetApprovedAndFirtTimeLockEmployeeList(string column, string value, string companyGroupId, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                string sql = @"SELECT * FROM (SELECT 
								   EI.SystemID,EI.EmployeeCode, EI.EmployeeName,
								   Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOB,
								   Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
   								   ,DP.UserName Department, 
								   DeG.UserName Designation, 
								   se.UserName Section, Sus.UserName SubSection,
								   LGD.userName LegalDesignation
								   ,U.UserName Unit, L.UserName Line,EI.EmployeeCodePreFix, EI.EmployeeCodeNumeric
                                   ,FirstTimeLock=CASE WHEN EI.FirstTimeLock=1 THEN 'Yes' ELSE 'No' END
                              FROM dbo.Employeeinformation EI
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id						
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
							  LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                              LEFT JOIN ORG.Section AS Se ON Se.Id= EI.SectionID 
							  LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= EI.SubSectionID 
							  LEFT JOIN ORG.Unit AS U ON U.Id= EI.UnitId
							  LEFT JOIN ORG.Line AS L ON L.Id= EI.LineId
                WHERE EI.EmployeeStatus ='Active' AND EI.IsApproved =0 AND  EI.FirstTimeLock=1 AND EI.PlantId='" + plantId + "' AND  EI.GroupId='" + companyGroupId + "') AS TEMP WHERE " + strkey + " Order By ISNULL(EmployeeCodePreFix,''), EmployeeCodeNumeric";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UnlockEmployee(EmployeeInformation employeeInformation, IdentityParameter para)
        {
            try
            {
                if (employeeInformation != null)
                {

                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    string sql = "SELECT * FROM [dbo].[EmployeeInformation] WHERE SystemId='" + employeeInformation.SystemId + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["FirstTimeLock"] = false;
                        dr["UpdatedBy"] = para.UpdatedBy;
                        dr["DateUpdated"] = DateTime.Now;

                        dr.EndEdit();

                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public IEnumerable<object> GetApprovalAuthority(string plantId)
        {
            var sql = "";
            try
            {
                sql = @"SELECT E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          INNER JOIN dbo.EmployeeInformation E On E.SystemId=A.EmployeeId 
                          WHERE  A.ActionStatus='EmployeeApprovalAuthority' AND E.EmployeeStatus='Active' And A.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmpAllDocumentDataList(string companyGroupId, string pId, string plantId)
        {
            try
            {
                var sql = @"SELECT  DISTINCT ED.*,
									CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,CD.ProfileType,CD.DocNumberRequired,CD.DocDateRequired
									,E.UserName AS EmployeeCategory
									,CD.DependateDate
								FROM dbo.EmployeeDocument ED
								LEFT JOIN hkp.ComplianceDocument CD ON ED.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CD.Id = CDSD.ComplianceDocumentId
								LEFT JOIN (SELECT  * FROM HKP.DocumentConfigurationDesignationGroup
								Where PlantId='" + plantId + @"' and EmployeeCategoryId = (
										SELECT D.EmployeeCategoryId
										FROM (SELECT * FROM MST.DesignationMaster WHERE CompanyGroupId = '" + companyGroupId + @"'
											) AS D
										LEFT JOIN EmployeeInformation EI ON D.DesignationId = EI.GivenDesignationId
										WHERE EI.SystemId = '" + pId + @"'
										)
								)DD ON CDSD.ComplianceDocumentSetId = DD.ComplianceDocumentSetId
								LEFT JOIN HKP.EmployeeCategory AS E ON DD.EmployeeCategoryId = E.Id
								WHERE ED.EmpSystemID = '" + pId + @"' 
									--AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND E.UserName IS NOT NULL ORDER BY CDSD.OptionalOrMandatory,DocumentName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmployeeList(string plantId, string companyId)
        {
            try
            {
                string CmdText = @"SELECT CAST (0 AS bit) Flag,Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,DeM.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,SE.UserName Section,EMP.SectionId,SuS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation,isnull( L.UserName,'') Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,DeM.UserName DesignationGroup,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,EJ.JobLcSystemID,FORMAT(EJ.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
                                        ,C.UserName Company,AM.Address1,EMP.PresentAddress1,EMP.CellPhnNo,EC.UserName EmployeeCategory,LPM.PolicyName
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN ORG.Company C ON C.Id=EMP.CompanyId
                                        LEFT JOIN MST.AddressMaster AM ON AM.Id=C.AddressMasterId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        left join ORG.Section SE on SE.Id=PR.SectionId
										LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN MST.DesignationMasterLegalDesignation DML ON DML.LegalDesignationId = EMP.LegalDesignationId
										Left join  MST.DesignationMaster DeM on DeM.Id = DML.DesignationMasterId
										left join HKP.Designation DeG on DeG.Id=DeM.DesignationId
                                        left join [MST].[DesignationMaster] DM on DM.DesignationId=EMP.GivenDesignationId
										left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.Id and DMC.PlantId=emp.PlantId                
										left join [dbo].[LeavePolicyMaster] LPM on LPM.SystemID=DMC.LeavePolicyMasterId and LPM.PlantID=emp.PlantID
                                        left join [HKP].[EmployeeCategory] EC on EC.Id=DM.EmployeeCategoryId
                                        LEFT JOIN dbo.EmpDateWiseJobLocation EJ ON EJ.EmpsystemId=EMP.SystemId
										 AND EJ.SystemId=(Select top(1) SystemId from dbo.EmpDateWiseJobLocation JB Where JB.EmpSystemID=EMP.SystemId Order by EffectiveDate desc)
                                        WHERE emp.PlantID='" + plantId + @"'  and EMP.CompanyId='" + companyId + @"' and EMP.EmployeeStatus='Active' ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmployeeCbo(string GroupId, string companyId, string plantId)
        {
            try
            {
                var sql = @"SELECT SystemId AS Value, (EmployeeCode+'-'+EmployeeName) AS Text FROM EmployeeInformation WHERE GroupID='" + GroupId + @"' AND CompanyId='" + companyId + @"' AND PlantId='" + plantId + @"' AND EmployeeStatus='Active' ORDER BY EmployeeName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmployeeNomineeInfo(string empId)
        {
            try
            {
                var sql = @"select N.Name,Format(N.DOB,'dd-MMM-yyyy')DOB,N.NationalID,N.CellNo,N.LocalName
                            , R.UserName Relativie,N.Id,N.AddressLocal,N.[Address],N.Relation
                                    from EmployeeNomineeInfo N
                          LEFT JOIN [SCS].[Relationship] R ON R.Id=N.Relation
                          Where N.EmpSystemId='" + empId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmployeeDependantInfo(string empId)
        {
            try
            {
                var sql = @"select ed.Id,Name,ed.RelationId,ed.ProfessionId, ed.Remarks,ed.LocalName,FORMAT(ED.DOB,'dd-MMM-yyyy')DOB, r.UserName Relation,p.UserName  Profession
                                from [dbo].[EmployeeDependantInfo] ed
                                left join [SCS].[Relationship] r on r.Id=ed.RelationId
                                left join [SCS].[Profession] p on p.Id=ed.ProfessionId
                             where ED.EmpSystemId='" + empId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmployeeLandLoardInfo(string empId)
        {
            try
            {
                var sql = @"select * from [dbo].[EmployeeLandLordInfo] where EmpSystemId='" + empId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmpDocumentDataList(string companyGroupId, string pId, string plantId)
        {
            try
            {
                var sql = @"SELECT  DISTINCT ED.*,
									CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,CD.ProfileType,CD.DocNumberRequired,CD.DocDateRequired
									,E.UserName AS EmployeeCategory
									,CD.DependateDate
								FROM dbo.EmployeeDocument ED
								LEFT JOIN hkp.ComplianceDocument CD ON ED.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CD.Id = CDSD.ComplianceDocumentId
								LEFT JOIN (SELECT  * FROM HKP.DocumentConfigurationDesignationGroup
								Where PlantId='" + plantId + @"' and EmployeeCategoryId = (
										SELECT D.EmployeeCategoryId
										FROM (SELECT * FROM MST.DesignationMaster WHERE CompanyGroupId = '" + companyGroupId + @"'
											) AS D
										LEFT JOIN EmployeeInformation EI ON D.DesignationId = EI.GivenDesignationId
										WHERE EI.SystemId = '" + pId + @"'
										)
								)DD ON CDSD.ComplianceDocumentSetId = DD.ComplianceDocumentSetId
								LEFT JOIN HKP.EmployeeCategory AS E ON DD.EmployeeCategoryId = E.Id
								WHERE ED.EmpSystemID = '" + pId + @"' AND DocumentationBy='Self'
									--AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND E.UserName IS NOT NULL ORDER BY CDSD.OptionalOrMandatory,DocumentName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object GetData(string companyGroupId, string companyId, string plantId, string employeeId)
        {
            try
            {
                var sql = @"SELECT EI.*
								  ,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
								  ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                                  ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
								  ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
								  ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
                              FROM dbo.Employeeinformation EI
                              LEFT OUTER JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
							  LEFT OUTER JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
							  LEFT OUTER JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
							  LEFT OUTER JOIN SCS.District D ON EI.PresDistrictID = D.Id
							  LEFT OUTER JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
		                      LEFT OUTER JOIN SCS.Country C ON EI.PresCountryID = C.ID
		                      LEFT OUTER JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
		                      LEFT OUTER JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
		                      LEFT OUTER JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                              LEFT OUTER JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
		                      LEFT OUTER JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                              LEFT OUTER JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
							  LEFT OUTER JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
							  LEFT OUTER JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT OUTER JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							  LEFT OUTER JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
							  LEFT OUTER JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                              WHERE EI.GroupId='" + companyGroupId + @"' AND EI.CompanyId='" + companyId + @"' AND EI.PlantId='" + plantId + "' AND EI.SystemId='" + employeeId + "'";
                return _sqlRepository.GetData(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetJobData(string empid)
        {
            try
            {
                var sql = @"SELECT JDI.Id, JDI.UserName JobDescription from [MST].[ManpowerBudgetJobDescription] PMBJD
                            LEFT OUTER JOIN [HKP].[JobDescription] JD ON PMBJD.JobDescriptionId=JD.Id
                            LEFT OUTER JOIN [HKP].[JobDescriptionItem] JDI ON JD.JobDescriptionItemId=JDI.Id
                             Where PMBJD.ManpowerBudgetId=(Select EI.BudgetCode From [dbo].[EmployeeInformation] EI Where EI.SystemId='" + empid + "') AND PMBJD.Archive=0";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        #endregion

        #region  EmployeeOperation

        public IEnumerable<object> GetSavedOperationData(string empsystemId)
        {
            try
            {
                var sql = @"SELECT EO.*,ISNULL(OM.Code,OV.Code) Code,ISNULL(OM.ShortName,OV.ShortName) ShortName
                            ,ISNULL(OM.StandardName,OV.StandardName) StandardName,ISNULL(OM.UserName,OV.UserName) UserName
                            ,ISNULL(MM.UserName,MMA.StandardName) MachineMaster,ISNULL(S.UserName,VS.UserName) Skill
                            FROM EmployeeOperation EO
                            LEFT JOIN  MST.OperationMaster OM ON EO.OperationMasterId=OM.Id
                            LEFT JOIN  MST.OperationVariation OV ON EO.OperationVariationId=OV.Id
                            LEFT JOIN MST.MachineMaster MM ON MM.Id=OM.MachineMasterId 
                            LEFT JOIN HKP.Skill S ON S.Id=OM.SkillId
                            LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=OV.ArticleId
                            LEFT JOIN HKP.Skill VS ON VS.Id=OV.SkillId
                            Where EO.EmpSystemId='" + empsystemId + "' ORDER BY EO.Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetOperationSequence(string empSystemId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT ISNULL((MAX(Sequence)+1),0) Sequence FROM [dbo].[EmployeeOperation] Where EmpSystemId='" + empSystemId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveOperation(List<EmployeeOperation> data, IdentityParameter para, string EmpSystemId)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsSeq;
                    GetOperationSequence(EmpSystemId, out dsSeq);
                    decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Sequence"].ToString());
                    if (seq != 0)
                    {
                        seq--;
                    }

                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    string _Id = "";
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[EmployeeOperation] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            seq++;
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeOperation", out _Id);

                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = _Id;
                            dr["Sequence"] = seq;
                            dr["EmpSystemId"] = item.EmpSystemId;
                            dr["OperationMasterId"] = item.OperationMasterId;
                            dr["OperationVariationId"] = item.OperationVariationId;
                            dr["CycleTime"] = item.CycleTime;

                            dr["AddedBy"] = para.AddedBy;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = para.AddedFromIP;

                            dr["UpdatedBy"] = para.UpdatedBy;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = para.UpdatedFromIP;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();
                            dr["Sequence"] = item.Sequence;
                            dr["OperationMasterId"] = item.OperationMasterId;
                            dr["OperationVariationId"] = item.OperationVariationId;
                            dr["CycleTime"] = item.CycleTime;

                            dr["UpdatedBy"] = para.UpdatedBy;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = para.UpdatedFromIP;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void DeleteEmployeeOperation(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.EmployeeOperation WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        public IEnumerable<object> GetUnApprovedEmployeeList(string companyGroupId, string plantId, bool IsSysAdmin, string UserId)
        {
            try
            {
                string sql = "";
                if (IsSysAdmin)
                {
                    sql = @"SELECT  CheckBoxSelect = Convert(bit, 'False'),
								   EI.SystemID,EI.EmployeeCode, EI.EmployeeName,
								   Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,
								   Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs
   								  ,DP.UserName Department, 
								  PR.UserName PositionName,
								  E.UserName EntityName,
								  DSG.UserName Designation, 
								  se.UserName Section, Sus.UserName SubSection,
								  LGD.userName LegalDesignation,PMB.Code,PR.UserName PositionName,E.UserName EntityName,ISNULL(PG.UserName,'') PayrollGroup,EC.UserName EmployeeCategory
                                  ,AE.EmployeeName ApprovalAuthority
                              FROM dbo.Employeeinformation EI
                             
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id						
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
							  LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                              LEFT JOIN ORG.Section AS Se ON Se.Id= EI.SectionID 
							  LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= EI.SubSectionID 
                              LEFT JOIN  [MST].[PayrollGroupMaster] PGM ON PGM.EmployeeId=EI.SystemId
                              LEFT JOIN  [HKP].[PayrollGroup] PG ON PG.Id=PGM.PayrollGroupId
                              LEFT JOIN dbo.Employeeinformation AE ON AE.SystemId=EI.ApprovalAuthorityId
                              LEFT JOIN (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
				                            LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
				                    )EC ON EC.DesignationId=EI.GivenDesignationId
                              WHERE EI.EmployeeStatus !='Separated' AND EI.IsApproved =0 AND  
                                    EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + @"'";
                }
                else
                {
                    sql = @"SELECT  CheckBoxSelect = Convert(bit, 'False'),
                                    EI.SystemID,EI.EmployeeCode, EI.EmployeeName,
                                    Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,
                                    Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs
                                    ,DP.UserName Department,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation, 
                                    se.UserName Section, Sus.UserName SubSection,LGD.userName LegalDesignation,PMB.Code,PR.UserName PositionName,E.UserName EntityName,ISNULL(PG.UserName,'') PayrollGroup,EC.UserName EmployeeCategory,AE.EmployeeName ApprovalAuthority
                                    FROM dbo.Employeeinformation EI                             
                                    LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
                                    LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id						
                                    LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                    LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                    LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
                                    LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                    LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                                    LEFT JOIN ORG.Section AS Se ON Se.Id= EI.SectionID 
                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= EI.SubSectionID 
                                    LEFT JOIN  [MST].[PayrollGroupMaster] PGM ON PGM.EmployeeId=EI.SystemId
                                    LEFT JOIN  [HKP].[PayrollGroup] PG ON PG.Id=PGM.PayrollGroupId
                                    LEFT JOIN dbo.Employeeinformation AE ON AE.SystemId=EI.ApprovalAuthorityId
                                    LEFT JOIN (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
				                            LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
				                    )EC ON EC.DesignationId=EI.GivenDesignationId
                                    WHERE EI.EmployeeStatus !='Separated' AND EI.IsApproved =0 AND  
                                    EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + @"' 
                                    AND (
                                    EI.SystemId IN ((Select EmployeeId from [MST].[PayrollGroupMaster] Where PayrollGroupId IN 
                                    (Select PayrollGroupId from [SEC].[UserPayrollGroup] where UserId='" + UserId + @"')
                                    ))
                                    OR EI.SystemId NOT IN (Select EmployeeId from [MST].[PayrollGroupMaster])
                                    )";
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetApprovedEmployeeList(string companyGroupId, string plantId, bool IsSysAdmin, string UserId)
        {
            try
            {
                string sql = "";
                if (IsSysAdmin)
                {
                    sql = @"SELECT  CheckBoxSelect = Convert(bit, 'False'),
								   EI.SystemID,EI.EmployeeCode, EI.EmployeeName,
								   Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,
								   Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs
   								  ,DP.UserName Department, 
								  PR.UserName PositionName,
								  E.UserName EntityName,
								  DSG.UserName Designation, 
								  se.UserName Section, Sus.UserName SubSection,
								  LGD.userName LegalDesignation,PMB.Code,PR.UserName PositionName,E.UserName EntityName,ISNULL(PG.UserName,'') PayrollGroup,EC.UserName EmployeeCategory
                              FROM dbo.Employeeinformation EI
                             
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id						
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
							  LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                              LEFT JOIN ORG.Section AS Se ON Se.Id= EI.SectionID 
							  LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= EI.SubSectionID 
                              LEFT JOIN  [MST].[PayrollGroupMaster] PGM ON PGM.EmployeeId=EI.SystemId
                              LEFT JOIN  [HKP].[PayrollGroup] PG ON PG.Id=PGM.PayrollGroupId
                              LEFT JOIN (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
				                            LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
				                    )EC ON EC.DesignationId=EI.GivenDesignationId
                              WHERE EI.EmployeeStatus !='Separated' AND EI.IsApproved =1 AND 
                                    EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + @"'";
                }
                else
                {
                    sql = @"SELECT  CheckBoxSelect = Convert(bit, 'True'),
                                    EI.SystemID,EI.EmployeeCode, EI.EmployeeName,
                                    Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,
                                    Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs
                                    ,DP.UserName Department,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation, 
                                    se.UserName Section, Sus.UserName SubSection,LGD.userName LegalDesignation,PMB.Code,PR.UserName PositionName,E.UserName EntityName,ISNULL(PG.UserName,'') PayrollGroup,EC.UserName EmployeeCategory
                                    FROM dbo.Employeeinformation EI                             
                                    LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
                                    LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id						
                                    LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                    LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                                    LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
                                    LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                                    LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                                    LEFT JOIN ORG.Section AS Se ON Se.Id= EI.SectionID 
                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= EI.SubSectionID 
                                    LEFT JOIN  [MST].[PayrollGroupMaster] PGM ON PGM.EmployeeId=EI.SystemId
                                    LEFT JOIN  [HKP].[PayrollGroup] PG ON PG.Id=PGM.PayrollGroupId
                                    LEFT JOIN (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
				                            LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
				                    )EC ON EC.DesignationId=EI.GivenDesignationId
                                    WHERE EI.EmployeeStatus !='Separated' AND EI.IsApproved =1 AND  
                                    EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + @"' 
                                    AND (
                                    EI.SystemId IN ((Select EmployeeId from [MST].[PayrollGroupMaster] Where PayrollGroupId IN 
                                    (Select PayrollGroupId from [SEC].[UserPayrollGroup] where UserId='" + UserId + @"')
                                    ))
                                    OR EI.SystemId NOT IN (Select EmployeeId from [MST].[PayrollGroupMaster])
                                    )";
                }

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region AddOnsFor Week OFf And OT Non Eligible
        //public IEnumerable<object> getWeekOff()
        //{
        //    try
        //    {
        //        var str = @"Select Id as Value, UserName as Text from dbo.WeekOffHeader";
        //        return _sqlRepository.GetDataCollection(str);
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        public IEnumerable<object> getNonEligibleOT(string DesgId, string PlantId)
        {
            try
            {
                var str = @"Select distinct PlantId, dm.DesignationGroupId , dg.Id, dc.IsOTEntitled from scs.DesignationMasterConfiguration dc
                            left join mst.DesignationMaster dm on dm.Id = dc.DesignationMasterId
                            left join hkp.Designation dg on dg.Id = dm.DesignationId
                            where dg.Id = '" + DesgId + @"' and PlantId = '" + PlantId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }

        #endregion

        #region EmployeeCodeGeneration
        public IEnumerable<object> GetAllEmployeeCodeGenerationPlantData(string masterId)
        {
            try
            {
                var sql = @"SELECT Flag= CAST(CASE WHEN D.Id IS NULL THEN 0 ELSE 1 END AS bit),ET.EmploymentType,D.Id,CG.UserName CompanyGroup,C.UserName Company,P.Id PlantId,P.UserName Plant,ET.Id EmployeeCodeTypeId
                                        FROM 
                                        (SELECT UserName AS EmploymentType,Id  From dbo.EmployeeCodeType) ET
                                        LEFT JOIN ORG.Plant P ON 1=1
                                        LEFT JOIN ORG.Company C ON C.Id=P.CompanyId
                                        LEFT JOIN ORG.CompanyGroup CG ON CG.Id=C.CompanyGroupId
                                        LEFT JOIN [dbo].[EmployeeCodeGenGroupDetail] D ON D.PlantId=P.Id and D.EmployeeCodeTypeId=ET.Id
                                        WHERE ISNULL(D.EmployeeCodeGenGroupId,'')=''
                                        UNION ALL
                                        SELECT Flag= CAST(CASE WHEN D.Id IS NULL THEN 0 ELSE 1 END AS bit),ET.EmploymentType,D.Id,CG.UserName CompanyGroup,C.UserName Company,P.Id PlantId,P.UserName Plant,ET.Id EmployeeCodeTypeId
                                        FROM 
                                        (SELECT UserName AS EmploymentType,Id  From dbo.EmployeeCodeType) ET
                                        LEFT JOIN ORG.Plant P ON 1=1
                                        LEFT JOIN ORG.Company C ON C.Id=P.CompanyId
                                        LEFT JOIN ORG.CompanyGroup CG ON CG.Id=C.CompanyGroupId
                                        LEFT JOIN [dbo].[EmployeeCodeGenGroupDetail] D ON D.PlantId=P.Id and D.EmployeeCodeTypeId=ET.Id
                                        WHERE ISNULL(D.EmployeeCodeGenGroupId,'')='" + masterId + @"'
                                        ORDER BY CG.UserName,C.UserName,P.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAvailableBudgetCode(string budgetCode)
        {
            try
            {
                string sql = @"SELECT COUNT(E.SystemId) OnRoll,BudgetCode,mbd.TotalNumber,CA= mbd.TotalNumber-COUNT(E.SystemId)+ISNULL(TBS.TBSEmp,0)
FROM EmployeeInformation E
LEFT JOIN MST.ManpowerBudgetDetail AS mbd ON mbd.ManpowerBudgetId= E.BudgetCode
LEFT JOIN (SELECT COUNT(BudgetCode) TBSEmp,SystemId FROM EmployeeInformation WHERE BudgetCode='" + budgetCode + @"' AND EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'') IN ('TBS') GROUP BY SystemId) TBS ON TBS.SystemId=E.SystemId
WHERE E.EmployeeStatus = 'Active' AND E.BudgetCode='" + budgetCode + @"' GROUP BY E.BudgetCode,mbd.TotalNumber,TBS.TBSEmp";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Guest User
        public IEnumerable<object> GetGuestEmployee(string CompanyGroupId)
        {
            try
            {
                var sql = @"SELECT E.SystemId,E.EmployeeCode,E.GroupID,E.DivisionId,E.DepartmentId,E.SectionId,E.SubSectionId,E.DesignationGroupId,E.DesignationSystemID,E.BudgetCode,E.PositionID
	                        ,E.CardNumber,E.Salutation,E.FirstName,E.MiddleName,E.LastName,E.EmployeeName,E.NickName,E.EmpPicPath,FORMAT(E.DOB,'dd-MMM-yyyy') DOB ,E.GenderID,E.GivenDesignationId	,E.LegalDesignationId
	                        ,E.EmailId,E.EmpType,D.UserName Division,DPT.UserName Department, S.UserName Section, SS.UserName SubSection,DG.UserName GivenDesignation,LDG.UserName Designation,E.IsAccessible,MB.PIN,FORMAT(E.TentativeExpiryDate,'dd-MMM-yyyy') TentativeExpiryDate
                        FROM EmployeeInformation E
                        LEFT JOIN ORG.Division D ON D.Id = E.DivisionId
                        LEFT JOIN ORG.Department DPT ON DPT.Id = E.DepartmentId
                        LEFT JOIN ORG.Section S ON S.Id = E.SectionId
                        LEFT JOIN ORG.SubSection SS ON SS.Id = E.SubSectionId
                        LEFT JOIN HKP.Designation DG ON DG.Id = E.GivenDesignationId
                        LEFT JOIN HKP.LegalDesignation LDG ON LDG.Id = E.LegalDesignationId
                        LEFT JOIN HKP.EmployeeMobileAppsAuthorization MB ON MB.EmployeeId = E.SystemId
                        WHERE E.GroupID = '" + CompanyGroupId + "' AND EmpType='Guest' order by DateAdded desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetGusetList(string CompanyGroupId)
        {
            try
            {
                var sql = @"SELECT MB.Id,E.SystemId EmployeeId,E.EmployeeCode,E.GroupID,E.DivisionId,E.DepartmentId,E.SectionId,E.SubSectionId,E.DesignationGroupId,E.DesignationSystemID,E.BudgetCode,E.PositionID
	                        ,E.CardNumber,E.Salutation,E.FirstName,E.MiddleName,E.LastName,E.EmployeeName,E.NickName,E.EmpPicPath,FORMAT(E.DOB,'dd-MMM-yyyy') DOB ,E.GenderID,E.GivenDesignationId	,E.LegalDesignationId
	                        ,E.EmailId,E.EmpType,D.UserName Division,DPT.UserName Department, S.UserName Section, SS.UserName SubSection,DG.UserName GivenDesignation,LDG.UserName Designation,E.IsAccessible,MB.PIN
                        FROM HKP.EmployeeMobileAppsAuthorization MB  
						LEFT JOIN EmployeeInformation E ON MB.EmployeeId = E.SystemId
                        LEFT JOIN ORG.Division D ON D.Id = E.DivisionId
                        LEFT JOIN ORG.Department DPT ON DPT.Id = E.DepartmentId
                        LEFT JOIN ORG.Section S ON S.Id = E.SectionId
                        LEFT JOIN ORG.SubSection SS ON SS.Id = E.SubSectionId
                        LEFT JOIN HKP.Designation DG ON DG.Id = E.GivenDesignationId
                        LEFT JOIN HKP.LegalDesignation LDG ON LDG.Id = E.LegalDesignationId
                        WHERE E.GroupID = '" + CompanyGroupId + "' AND EmpType='Guest'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAllLegalDesignationCbo(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT A.Id AS [Value], A.UserName AS [Text], B.DesignationGroupId FROM [HKP].[LegalDesignation] A
                            LEFT OUTER JOIN (SELECT * FROM [MST].[DesignationMasterLegalDesignation]) DL ON A.Id=DL.LegalDesignationId
                            LEFT OUTER JOIN (SELECT * FROM [MST].[DesignationMaster] where CompanyGroupId='" + companyGroupId + @"')B ON DL.DesignationMasterId = B.Id
                            Order By A.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        #endregion
        public DataTable GetEmpInfoData(string empId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT EI.SystemId,EI.EmployeeName,EI.EmpPicPath EmployeePic,EI.NationalID UIDNO,EI.CellPhnNo MobileNo,EI.FatherName,EI.MotherName,EI.SpouseName,EI.GenderID Gender,FORMAT(EI.DOB,'dd-MMM-yyyy') DOB,DATEDIFF(YEAR,EI.DOB,GETDate()) Age
,CS.UserName MaritalStatus,R.UserName Caste,EN.Name NomineeName,NomineeAge=DATEDIFF(YEAR,EN.DOB,GETDate()),RS.UserName NomineeRelation,EI.PresentAddress1 PresentAddress,EI.ParmanentAddress1 PermanentAddress
,EXI.Employer NameofCompany,Years=CAST((EXI.DurationYear)AS varchar(20))+' Y '+CAST((EXI.DurationMonth)AS varchar(20))+' M',EXI.Designation, RDP.UserName RefDepartment,EI.EmployeeName NameofCandidate,FORMAT(EI.DOJ,'dd-MMM-yyyy')DOJ, EDP.UserName Department
,ESINo=(Select ED.DocNumber from dbo.EmployeeDocument ED
LEFT JOIN HKP.ComplianceDocument CD ON CD.Id=ED.ComplianceDocumentId
Where EmpSystemID=EI.SystemId AND CD.UserName='Declaration Form (ESIC)')
,UAN=(Select ED.DocNumber from dbo.EmployeeDocument ED
LEFT JOIN HKP.ComplianceDocument CD ON CD.Id=ED.ComplianceDocumentId
Where EmpSystemID=EI.SystemId AND CD.UserName='Nomination Form (PF)')
,REF.Ref1Name RefName,REMP.EmployeeCode RefCode,LD.UserName LegalDesignation,EQ.ExamDegreeType Qualification,S.UserName Section
,RG.UserName ResidenceGroup,TG.UserName TransportGroup,RM.ResidenceNumber,RT.StandardName [Route]
FROM dbo.EmployeeInformation EI
LEFT JOIN HKP.CivilStatus CS ON CS.Id=EI.CivilStatusID
LEFT JOIN SCS.Religion R ON R.Id=EI.ReligionId
LEFT JOIN dbo.EmployeeNomineeInfo EN ON EN.EmpSystemId=EI.SystemId
LEFT JOIN [SCS].[Relationship] RS ON RS.Id=EN.Relation
LEFT JOIN dbo.EmpExperienceInformation EXI ON  EXI.EmpSystemID=EI.SystemId
AND EXI.SystemID=(select top(1) SystemID from dbo.EmpExperienceInformation Where SystemId=EXI.SystemID AND EmpSystemID=EI.SystemId Order By DateAdded DESC)
LEFT JOIN dbo.EmpAcademicQualificationInformation EQ ON  EQ.EmpSystemID=EI.SystemId
AND EQ.SystemID=(select top(1) SystemID from dbo.EmpAcademicQualificationInformation Q
LEFT JOIN [SCS].[QualificationLevel] L ON L.Id=Q.EductLevelSystemID
Where Q.SystemID=EQ.SystemID AND Q.EmpSystemID=EI.SystemId Order By L.Sequence DESC)
LEFT JOIN dbo.EmpReferenceInformation REF ON REF.EmpSystemId=EI.SystemId
LEFT JOIN dbo.EmployeeInformation REMP ON REMP.SystemId=REF.RefEmpSystemID
LEFT JOIN ORG.Department RDP ON RDP.Id=REMP.DepartmentId
LEFT JOIN ORG.Department EDP ON EDP.Id=EI.DepartmentId
LEFT JOIN HKP.LegalDesignation LD ON LD.Id=EI.LegalDesignationId
LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
LEFT JOIN dbo.ResidenceGroup RG ON RG.Id=EI.ResidenceGroupId
LEFT  JOIN dbo.ResidenceAllocatedEmployees RA ON RA.EmployeeSystemId=EI.SystemId
LEFT JOIN ResidenceMaster RM ON RM.Id=RA.ResidenceId
LEFT JOIN dbo.TransportGroup TG ON TG.Id=EI.TransportGroupId
LEFT  JOIN dbo.EmployeeTransportAllocation ETA ON EI.SystemId = ETA.EmployeeSystemId AND ETA.AssignStatus = 1
left join RouteSchedule RSC on RSC.Id = ETA.TripId
left join MST.Route RT on RT.Id = RSC.RouteId
Where EI.SystemId='" + empId + "'";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable GetFamiliInfo(string empId)
        {
            string strSQL;
            try
            {
                strSQL = @"select D.Name,Relation=RS.UserName,Age=DATEDIFF(YEAR,D.DOB,GETDate()) from dbo.EmployeeDependantInfo D
LEFT JOIN [SCS].[Relationship] RS ON RS.Id=D.RelationId
Where EmpSystemId='" + empId + "'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public void GetAPPLICATIONFORMFORRECRUITMENT(string empId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "APPLICATIONFORMFORRECRUITMENT.docx";

            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                WSection section = document.Sections[0];

                DataTable dsData, dsFamiliInfo;

                dsData = GetEmpInfoData(empId);
                dsFamiliInfo = GetFamiliInfo(empId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsData.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeEmpInfoData(empId, document, dsData);
                var addInfo = makefamilyInfo(empId, document, dsFamiliInfo);
                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        document.Replace(text, dsData.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                if (!string.IsNullOrEmpty(dsData.Rows[0]["EmployeePic"].ToString()))
                {
                    var pic = dsData.Rows[0]["EmployeePic"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 120, 120);

                            section.Tables[0].Rows[0].Cells[0].Paragraphs[0].AppendPicture(newImage);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                fileName = "APPLICATIONFORMFORRECRUITMENT-" + empId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }
        }

        public double makefamilyInfo(string empId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{familyInfo}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 3;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Name");
            range.ApplyCharacterFormat(FontBold);
            int colName = COL; COL++;
            wTable.Rows[ROW].Cells[colName].Width = 200;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Relation");
            range.ApplyCharacterFormat(FontBold);
            int colRelation = COL; COL++;
            wTable.Rows[ROW].Cells[colRelation].Width = 150;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Age");
            range.ApplyCharacterFormat(FontBold);
            int colAge = COL;
            wTable.Rows[ROW].Cells[colAge].Width = 100;


            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colName].AddParagraph().AppendText(dsaddInfo.Rows[i]["Name"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colRelation].AddParagraph().AppendText(dsaddInfo.Rows[i]["Relation"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colAge].AddParagraph().AppendText(dsaddInfo.Rows[i]["Age"].ToString()).ApplyCharacterFormat(DFontSize);

            }
            ROW++;
            ROW++;
            #region paragrpath formats

            IWParagraphStyle myaddStyle = document.AddParagraphStyle("AddinfoStyle");
            //Sets the formatting of the style
            myaddStyle.CharacterFormat.FontSize = 8f;
            myaddStyle.CharacterFormat.TextColor = Color.Black;
            myaddStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            #endregion paragrpath formats

            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;
            ROW++;
            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return 0;
        }

        public Image resizeImage(Image image, int new_height, int new_width)
        {
            Bitmap new_image = new Bitmap(new_width, new_height);
            Graphics g = Graphics.FromImage((Image)new_image);
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, new_width, new_height);
            return new_image;
        }
        public double makeEmpInfoData(string empId, WordDocument document, DataTable dsData)
        {

            int LasColumnIndex = 6;

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            FontBold.FontSize = 8.5f;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("ARTICLE");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 200;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PRODUCT DETAILS");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("LOT NO");
            range.ApplyCharacterFormat(FontBold);
            int colLot = COL; COL++;
            wTable.Rows[ROW].Cells[colLot].Width = 80;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("NET WEIGHT (KGS.)");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("No of Cartons");
            range.ApplyCharacterFormat(FontBold);
            int colCartons = COL; COL++;
            wTable.Rows[ROW].Cells[colCartons].Width = 50;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("GROSS WEIGHT (KGS.)");
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colGW = COL;
            wTable.Rows[ROW].Cells[colQty].Width = 60;

            #endregion column headers

            ROW++;

            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;



            #endregion paragrpath formats

            #region merging section

            ROW = 0;

            ROW++;

            #endregion merging section

            //TextBodyPart textBodyPart = new TextBodyPart(document);
            //textBodyPart.BodyItems.Add(wTable);
            //document.Replace(replaceString, textBodyPart, true, true);


            return 0;
        }

    }
    public class EmployeeOperation
    {
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string EmpSystemId { get; set; }
        public string OperationMasterId { get; set; }
        public string OperationVariationId { get; set; }

        public decimal CycleTime { get; set; } = 0;
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}

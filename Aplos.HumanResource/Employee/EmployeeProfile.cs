using bplib;
using clsAttendance;
using ConnectionManager.DAL;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Model.HumanResources;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
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

                string sql = @"SELECT TOP(500) * FROM (SELECT EI.*,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
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
                             ,TenureDay=DATEDIFF(day, FORMAT(EI.DOJ,'dd-MMM-yyyy'),FORMAT(GetDate(),'dd-MMM-yyyy')),REI.EmployeeCode RelativeCode,REI.EmployeeName RelativeName,EC.DesignationGroup
							 ,PolicyName=(Select distinct LPM.PolicyName from MST.DesignationMaster DM 
                                            LEFT JOIN  HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId            
                            left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.Id and DMC.PlantId=EI.PlantId    
                            left join [dbo].[LeavePolicyMaster] LPM on LPM.SystemID=DMC.LeavePolicyMasterId and LPM.PlantID=EI.PlantID
							Where DM.DesignationId=EI.GivenDesignationId)
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
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId,DG.UserName DesignationGroup FROM [HKP].[EmployeeCategory] ECT
				                            LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
                                            LEFT JOIN  HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId
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


        public void SaveData(EmployeeInformation data, IdentityParameter para, string EmployeeCodeCheckLevel, EmpReferenceInformation empRef, Dictionary<string, object> empBank, EmployeeWeekOffByDay employeeWeek)
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

                    //string strWorkOffSystemID = "";
                    //objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_WEEKOFF_BYDAY", out strWorkOffSystemID);
                    //strWorkOffSystemID = "W" + "-" + strWorkOffSystemID;

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
                    //objEmpLoad.SaveEmployeeWeekOffByDay(data.SystemId, strWorkOffSystemID, out dsWeekOffByDay);
                    //dtWeekOffByDay = dsWeekOffByDay.Tables[0];
                    //dvWeekOffByDay = new DataView();
                    //dvWeekOffByDay.Table = dtWeekOffByDay;
                    //dvWeekOffByDay.RowFilter = "SystemID = '" + strWorkOffSystemID + "'";
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
                    //if (dvWeekOffByDay.Count == 0)
                    //{// Add new block
                    //    drWeekOffByDay = dtWeekOffByDay.NewRow();
                    //    UpdateEmployeeWeekOffByDayDataRow("ADDNEW", strWorkOffSystemID, data, para, ref drWeekOffByDay);
                    //    dtWeekOffByDay.Rows.Add(drWeekOffByDay);
                    //}
                    //else
                    //{//edit block
                    //    drWeekOffByDay = dvWeekOffByDay[0].Row;
                    //    drWeekOffByDay.BeginEdit();
                    //    UpdateEmployeeWeekOffByDayDataRow("EDIT", strWorkOffSystemID, data, para, ref drWeekOffByDay);
                    //    drWeekOffByDay.EndEdit();
                    //}
                    //dvWeekOffByDay.RowFilter = null;
                    ////}
                    string sql = "";
                    DataSet dsWeekOff=null;

                    if (employeeWeek!=null)
                    {
                        sql = "SELECT * FROM EmployeeWeekOffByDay WHERE EmpSystemID='" + data.SystemId + "'";
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsWeekOff, false, "1");

                        if (dsWeekOff.Tables[0].Rows.Count == 0)
                        {
                            string systemid = "";
                            bplib.clsGenID id = new bplib.clsGenID();
                            id.GenID(DateTime.Now.ToShortDateString(), "WeekoffAssign", out systemid);

                            DataRow dr = dsWeekOff.Tables[0].NewRow();

                            dr["SystemID"] = "WN-" + systemid;
                            dr["EmpSystemID"] = data.SystemId;
                            dr["EffectiveDate"] = data.DOJ;


                            dr["AlignWithCC"] = false;
                            dr["IndividualWeekOff"] = false;

                            dr["FstOffDay"] = DBNull.Value;
                            dr["FstDayLengthType"] = DBNull.Value;
                            dr["SndOffDay"] = DBNull.Value;
                            dr["SndDayLengthType"] = DBNull.Value;

                            dr["AlignWithCC"] = employeeWeek.AlignWithCC;

                            if (employeeWeek.AlignWithCC == false)
                            {
                                dr["IndividualWeekOff"] = true;
                                if (string.IsNullOrEmpty(employeeWeek.FstOffDay) == false)
                                {
                                    dr["FstOffDay"] = employeeWeek.FstOffDay;
                                    dr["FstDayLengthType"] = employeeWeek.FstDayLengthType;
                                }
                                if (string.IsNullOrEmpty(employeeWeek.SndOffDay) == false)
                                {
                                    dr["SndOffDay"] = employeeWeek.SndOffDay;
                                    dr["SndDayLengthType"] = employeeWeek.SndDayLengthType;
                                }
                            }

                            dr["AddedBy"] = para.AddedBy;
                            dr["DateAdded"] = DateTime.Now;

                            dsWeekOff.Tables[0].Rows.Add(dr);

                        }

                    }
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
                    DataSet dsEmpBankMaster;
                    sql = "SELECT * FROM [dbo].[EmployeeBankInfo] WHERE RowID='" + empBank["RowID"] + "'";
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

                    objApp.SaveDataSets(dsLocal, dsShiftAssign, dsEmpJbLc, dsWeekOffByDay, dsEmpPin, dsEmpRef, dsEmpBankMaster, dsWeekOff); // , dsWeeklyOff, dsNonOT


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
                left join LeaveType lt on lt.CompanyGroupId=cg.Id AND lt.LeaveType='Earn'
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
                drLocal["CTC"] = 0;
                drLocal["Basic"] = 0;
                drLocal["Gross"] = 0;
                if (string.IsNullOrEmpty(data.TrainingTypeId))
                {
                    drLocal["TrainingTypeId"] = DBNull.Value; 
                }
                else
                {
                    drLocal["TrainingTypeId"] = data.TrainingTypeId;
                }
                drLocal["IsGlobalEmployee"] = false;

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
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
							  LEFT JOIN ORG.Entity EN ON EN.Id=PMB.EntityId
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                              LEFT JOIN ORG.Section AS Se ON Se.Id= PR.SectionID 
							  LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID 
							  LEFT JOIN ORG.Unit AS U ON U.Id= EN.UnitId
							  LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
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
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,SE.UserName Section,PR.SectionId,SuS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation,isnull( L.UserName,'') Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,DG.UserName DesignationGroup,
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
LEFT JOIN  HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId            
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
                var sql = @"SELECT SystemId AS Value, (EmployeeCode+'-'+EmployeeName) AS Text FROM EmployeeInformation WHERE GroupID='" + GroupId + @"' AND CompanyId='" + companyId + @"' AND PlantId='" + plantId + @"' AND EmployeeStatus='Active' AND EmpType<>'Guest' ORDER BY EmployeeName";
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
                var sql = @"SELECT EO.*,OM.Code,OM.ShortName,OM.StandardName,OM.UserName,MM.UserName MachineMaster,S.UserName Skill
                            FROM EmployeeOperation EO
                            LEFT JOIN  MST.OperationMaster OM ON EO.OperationMasterId=OM.Id
                            LEFT JOIN MST.MachineMaster MM ON MM.Id=OM.MachineMasterId 
                            LEFT JOIN HKP.Skill S ON S.Id=OM.SkillId
                            Where EO.EmpSystemId='" + empsystemId + "' ORDER BY EO.Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAllEmployeeOperationData()
        {
            try
            {
                var sql = @"SELECT EO.EmpSystemId,E.EmployeeCode,E.EmployeeName,EO.Sequence,OM.Code,OM.ShortName,OM.StandardName,OM.UserName,S.UserName Skill,EO.CycleTime,EN.UserName Entity
                            FROM EmployeeOperation EO
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=EO.EmpSystemId
							LEFT JOIN MST.ManpowerBudget MB ON MB.Id=E.BudgetCode
							LEFT JOIN ORG.Entity EN ON EN.Id=MB.EntityId
                            LEFT JOIN  MST.OperationMaster OM ON EO.OperationMasterId=OM.Id
                            LEFT JOIN HKP.Skill S ON S.Id=OM.SkillId
                            WHERE EO.Archive=0 ORDER BY EO.EmpSystemId,EO.Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAllEmployeeOperationArchiveData()
        {
            try
            {
                var sql = @"SELECT EO.EmpSystemId,E.EmployeeCode,E.EmployeeName,EO.Sequence,OM.Code,OM.ShortName,OM.StandardName,OM.UserName,S.UserName Skill,EO.CycleTime,EN.UserName Entity
                            FROM EmployeeOperation EO
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=EO.EmpSystemId
							LEFT JOIN MST.ManpowerBudget MB ON MB.Id=E.BudgetCode
							LEFT JOIN ORG.Entity EN ON EN.Id=MB.EntityId
                            LEFT JOIN  MST.OperationMaster OM ON EO.OperationMasterId=OM.Id
                            LEFT JOIN HKP.Skill S ON S.Id=OM.SkillId
                            WHERE EO.Archive=1 ORDER BY EO.EmpSystemId,EO.Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAllEmployeeOperationVariationData()
        {
            try
            {
                var sql = @"SELECT EO.EmpSystemId,E.EmployeeCode,E.EmployeeName,EO.Sequence,OV.Code,OV.ShortName,OV.StandardName,OV.UserName,MMA.StandardName MachineName,EN.UserName Entity    
                            FROM EmployeeOperationVariation EO
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=EO.EmpSystemId
							LEFT JOIN MST.ManpowerBudget MB ON MB.Id=E.BudgetCode
							LEFT JOIN ORG.Entity EN ON EN.Id=MB.EntityId
                            LEFT JOIN  MST.OperationVariation OV ON EO.OperationVariationId=OV.Id
                            LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=OV.ArticleId
                            WHERE EO.Archive=0 ORDER BY EO.EmpSystemId,EO.Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetAllEmployeeOperationVariationArchiveData()
        {
            try
            {
                var sql = @"SELECT EO.EmpSystemId,E.EmployeeCode,E.EmployeeName,EO.Sequence,OV.Code,OV.ShortName,OV.StandardName,OV.UserName,MMA.StandardName MachineName,EN.UserName Entity        
                            FROM EmployeeOperationVariation EO
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=EO.EmpSystemId
							LEFT JOIN MST.ManpowerBudget MB ON MB.Id=E.BudgetCode
							LEFT JOIN ORG.Entity EN ON EN.Id=MB.EntityId
                            LEFT JOIN  MST.OperationVariation OV ON EO.OperationVariationId=OV.Id
                            LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=OV.ArticleId
                            WHERE EO.Archive=1 ORDER BY EO.EmpSystemId,EO.Sequence";
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
                            //bplib.clsGenID genid = new bplib.clsGenID();
                            //genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeOperation", out _Id);

                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = item.EmpSystemId+"-"+seq.ToString();
                            //dr["Id"] = _Id;
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
								  LGD.userName LegalDesignation,PMB.Code,E.UserName EntityName,ISNULL(PG.UserName,'') PayrollGroup,EC.UserName EmployeeCategory
                                  ,AE.EmployeeName ApprovalAuthority
                              FROM dbo.Employeeinformation EI
                             
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id						
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                              LEFT JOIN ORG.Section AS Se ON Se.Id= PR.SectionID 
							  LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID 
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
                                    se.UserName Section, Sus.UserName SubSection,LGD.userName LegalDesignation,PMB.Code,E.UserName EntityName,ISNULL(PG.UserName,'') PayrollGroup,EC.UserName EmployeeCategory,AE.EmployeeName ApprovalAuthority
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
                                    LEFT JOIN ORG.Section AS Se ON Se.Id= PR.SectionID 
                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID 
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
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                              LEFT JOIN ORG.Section AS Se ON Se.Id= PR.SectionID 
							  LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID 
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
                                    LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                              LEFT JOIN ORG.Section AS Se ON Se.Id= PR.SectionID 
							  LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID 
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
LEFT JOIN MST.ManpowerBudget emb ON emb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON eMB.PositionId=PR.Id
                        LEFT JOIN ORG.Division D ON D.Id = PR.DivisionId
                        LEFT JOIN ORG.Department DPT ON DPT.Id = PR.DepartmentId
                        LEFT JOIN ORG.Section S ON S.Id = PR.SectionId
                        LEFT JOIN ORG.SubSection SS ON SS.Id = PR.SubSectionId
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
                strSQL = @"SELECT EI.SystemId,EI.EmployeeCode,EI.EmployeeName,EI.EmpPicPath EmployeePic,EI.NationalID UIDNO,EI.CellPhnNo MobileNo,EI.FatherName,EI.MotherName,EI.SpouseName,EI.GenderID Gender,FORMAT(EI.DOB,'dd-MMM-yyyy') DOB,DATEDIFF(YEAR,EI.DOB,GETDate()) Age
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
LEFT JOIN MST.ManpowerBudget emb ON emb.Id = EI.BudgetCode
LEFT JOIN ORG.Position EPR ON EMB.PositionId=EPR.Id
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
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = REMP.BudgetCode
LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
LEFT JOIN ORG.Department RDP ON RDP.Id=PR.DepartmentId
LEFT JOIN ORG.Department EDP ON EDP.Id=EPR.DepartmentId
LEFT JOIN HKP.LegalDesignation LD ON LD.Id=EI.LegalDesignationId
LEFT JOIN ORG.Section S ON S.Id=EPR.SectionId
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

        public List<string> GetOSMasterColumnList()
        {
            string strSql = @"select A.SalesOrderId,A.UserName ,A.Value,A.MasterOrderNo,A.ExpectedCompletionDate,A.SODistributedQty,A.PlantId,A.Plant,A.MasterOrderEntityId,A.MasterOrderEntity,
A.ProductionOrderEntityId,A.ProductionOrderEntity,A.Customer,A.PartyType,A.PaymentTerm,A.PaymentTermDays,A.BaseOnDueDate,A.MatureDate,A.Buyer,A.Season,
A.TotalPlanQty,A.ProducedQty,A.RemainingPlanQuantity,A.BuyerDepartment,A.BuyerDivision,A.ResponsiblePerson,A.MasterOrderQty,
A.MOCreationDate,A.MOOrderCategory,A.MOOrderStatus,A.Type,A.BuyerOrderNo,A.OwnOrderNo,
A.LineItemId,A.BuyerReferenceNo,A.ProductionGrouping,A.MasterOrderItemCreationDate,A.ItemCategory,A.Remark,
A.Material,A.Article, A.ProductCategory, A.Product,A.ItemQty,A.UOM,A.ProductLibrayId,A.ProductCode,A.OrderRemarks,A.OrderControlStatus,
A.CriticalityLevel,A.MainRMInhouseRemarks,A.MainRMInhouseStatus,A.OtherRMInhouseRemarks,A.OtherRMInhouseStatus,A.InputRemarks,
A.InputStatus,A.ReviseDate ,A.DestinationId,A.Destination,
A.ShipmentModeId,A.ShipMode, A.SalesOrderCategoryId,A.SalesOrderCategory,
A.SalseOrderStatusId,A.SalseOrderStatus, A.SOQty,A.CM,A.[Gain & Loss],A.[Other Cost] ,A.Rate
,A.SalesExpense,A.Discount,A.NetSalesRealization,A.DirectMaterialCost,A.DirectProcessCost,A.ValueLoss,A.DeliveryGroup,A.DeliveryDate, A.CommitmentDate,A.PlanExFactoryDate
, A.SOMainRawMaterialInhouseDate,A.SOOtherRawMaterialInhouseDate,A.SOLSD
,A.PONumber,A.Description,A.LineItemReference,A.SalesOrderCreationDate,
A.ProductionOrderCreationDate,A.ScheduleId,A.ProductionOrderID,A.ProductionStatus
,A.FirstProcessWorkCenter
,A.NoOfWorkStation, A.Efficiency,
A.[S.SPT], A.PlanWorkingHoursPerDay, A.FirstDayOutPut,
A.PlanTargetPerHour, A.IncrementValue, A.IncrementType,
A.DayToReachTheTarget, A.POCommitmentDate,
A.ProductionPriority, A.TargetPerHour, A.TargetPerDay,
A.MinimumLineDays, A.RequiredLineDays,
A.RequiredNoOfLines, A.AllocatedLines, A.ExplicitProductionQty,
A.PRLSD, A.PRMainRawMaterialInhouseDate, A.PROtherRawMaterialInhouseDate,
A.RunningOrderBlockSize,A.ProductionCompletionDate,A.ActiveOrderLinePreference,A.RunningOrderLinePreference
,A.PlannedLinePreference,A.ProductionType,A.ShipmentFromStock,A.PackingType,A.InvoiceNo
,A.InvoiceDate,A.SalesQty,A.SalesAmount,A.DispatchUpdateQty,A.DispatchStatus,A.DispatchPercentage
,A.ProductionCompletionStatus,A.ExchangeRate,A.ApprovedStatus,A.ApproveBy,A.ApprovalDate,A.CustomerArticle,A.OwnItemRef,A.OrderCostingMasterTemplateId
,A.CSPT,A.BulletinID, A.[B.Spt] , A.[B.M.Spt]
,A.OrderStatusUpdateDate,A.OderStatusUpdateBy
from
(
SELECT mo.MasterOrderNo,format(XCOM.ExpectedCompletionDate,'dd-MMM-yyyy') AS ExpectedCompletionDate,XCOM.Quantity SODistributedQty
,p2.Id PlantId,p2.UserName AS Plant,
e.Id AS MasterOrderEntityId,e.UserName AS MasterOrderEntity,
e2.Id AS ProductionOrderEntityId,e2.UserName AS ProductionOrderEntity,
p.UserName AS Customer
,PartyType=(select distinct PAG.UserName PartyType from HKP.CompanyParty CP
LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId
Where CP.PartyType='Customer' AND mo.PartyId=CP.PartyId),PTR.UserName PaymentTerm,MO.PaymentTermDays,FORMAT(MO.BaseOnDueDate,'dd-MMM-yyyy')BaseOnDueDate,FORMAT(MO.MatureDate,'dd-MMM-yyyy')MatureDate
,b.UserName AS Buyer,ss.UserName AS Season,
ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0) AS TotalPlanQty,
ISNULL(PRODPR.ProductionQtyAtPR,0) AS ProducedQty,
ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS RemainingPlanQuantity,
 
BDEP.UserName AS BuyerDepartment,bd.UserName AS BuyerDivision, ei.EmployeeName AS ResponsiblePerson,MO.TotalQty MasterOrderQty,
FORMAT(MO.AddedDate,'dd-MMM-yyyy') MOCreationDate,OC.UserName AS MOOrderCategory,os.UserName AS MOOrderStatus,MO.Type, mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo,
MOI.Id AS LineItemId,MOI.BuyerReferenceNo,moi.ProductionGrouping,FORMAT(MOI.AddedDate,'dd-MMM-yyyy') MasterOrderItemCreationDate,MOI.ItemCategory,MOI.Remark,
mm.UserName AS Material,mma.StandardName AS Article, pc.UserName AS ProductCategory, pm.UserName AS Product,MOI.TotalQty AS ItemQty,uom.UserName AS UOM,
PL.Id ProductLibrayId,PL.Code ProductCode
,MOI.CSPT 
,OrderRemarks=REPLACE(REPLACE(
   STUFF((select distinct ', '+FORMAT(AMTR.AddedDate,'dd-MMM-yyyy')+'-'+AMTR.Remarks 
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'ShipmentControl' AND B.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,OrderControlStatus=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.Status
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'ShipmentControl' AND B.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,CriticalityLevel=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.CriticalityLevel
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'ShipmentControl' AND B.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,MainRMInhouseRemarks=REPLACE(REPLACE(
   STUFF((select distinct ', '+FORMAT(AMTR.AddedDate,'dd-MMM-yyyy')+'-'+AMTR.Remarks 
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'MainRMInhouse' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,MainRMInhouseStatus=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.Status  
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'MainRMInhouse' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,OtherRMInhouseRemarks=REPLACE(REPLACE(
   STUFF((select distinct ', '+FORMAT(AMTR.AddedDate,'dd-MMM-yyyy')+'-'+AMTR.Remarks  
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'OtherRMInhouse' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,OtherRMInhouseStatus=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.Status  
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'OtherRMInhouse' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
 
,InputRemarks=REPLACE(REPLACE(
   STUFF((select distinct ', '+FORMAT(AMTR.AddedDate,'dd-MMM-yyyy')+'-'+AMTR.Remarks 
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'BaseProcessInput' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,InputStatus=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.Status  
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'BaseProcessInput' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
 
,so.Id AS SalesOrderId,FORMAT(SO.ReviseDate,'dd-MMM-yyyy')ReviseDate ,so.DestinationId,dest.UserName AS Destination,
so.ShipmentModeId,smo.UserName AS ShipMode, OCS.Id SalesOrderCategoryId,OCS.UserName AS SalesOrderCategory,
OSS.Id SalseOrderStatusId,osS.UserName AS SalseOrderStatus, ISNULL(so.Qty,0) SOQty,SCCCM.SOValue CM, SCC.SOValue 'Gain & Loss',SO.Rate
,SO.SalesExpense,SO.Discount,NetSalesRealization=ISNULL((SO.SalesExpense - SO.Discount ),0),SO.DirectMaterialCost,SO.DirectProcessCost,SO.ValueLoss,SO.Other [Other Cost]
,SO.DeliveryGroup,FORMAT(so.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, FORMAT(so.CommitmentDate,'dd-MMM-yyyy') CommitmentDate, FORMAT(so.PlanExFactoryDate,'dd-MMM-yyyy') PlanExFactoryDate
, FORMAT(so.MainRawMaterialInhouseDate,'dd-MMM-yyyy') SOMainRawMaterialInhouseDate,
FORMAT(so.OtherRawMaterialInhouseDate,'dd-MMM-yyyy') SOOtherRawMaterialInhouseDate,FORMAT(so.LSD,'dd-MMM-yyyy') SOLSD
,CP.PONumber,SO.Description,SO.LineItemReference,FORMAT(so.AddedDate,'dd-MMM-yyyy') SalesOrderCreationDate,FORMAT(so.OrderStatusChangedDate,'dd-MMM-yyyy') OrderStatusUpdateDate
,SO.OrderStatusChangedBy OderStatusUpdateBy,FORMAT(PO.AddedDate,'dd-MMM-yyyy')ProductionOrderCreationDate,t.Id ScheduleId,PO.Id ProductionOrderID,ps.UserName AS ProductionStatus
,FirstProcessWorkCenter=STUFF((select distinct ','+xw.UserName from
ProductionOrderFirstProcessWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, '')
,t.NoOfWorkStation, t.Efficiency,
t.SPT 'S.SPT', t.PlanWorkingHoursPerDay, t.FirstDayOutPut,
t.PlanTargetPerHour, t.IncrementValue, t.IncrementType,
t.DayToReachTheTarget, t.CommitmentDate POCommitmentDate,
t.ProductionPriority, t.TargetPerHour, t.TargetPerDay,
t.MinimumLineDays, t.RequiredLineDays,
t.RequiredNoOfLines, t.AllocatedLines, t.Qty AS ExplicitProductionQty,
t.LSD AS PRLSD, t.MainRawMaterialInhouseDate AS PRMainRawMaterialInhouseDate, t.OtherRawMaterialInhouseDate AS PROtherRawMaterialInhouseDate,
t.RunningOrderBlockSize,l.LastProcessDate AS ProductionCompletionDate
,PBT.Id BulletinID,PBTD.Totalspt 'B.SPT',PBTD.mctotalspt 'B.M.SPT'
,ActiveOrderLinePreference=STUFF((select distinct ','+xw.UserName from
trn.ProductionOrderWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, '')
 
,RunningOrderLinePreference=STUFF((select distinct ','+xw.UserName from
trn.RunningOrderWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, '')
 
,PlannedLinePreference=STUFF((select distinct ','+xw.UserName from
ProductionPlanningType1 AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, '')
,SO.ProductionType,ShipmentFromStock= CASE WHEN SO.ShipmentFromStock=1 THEN 'Yes' ELSE 'No' END,PT.UserName PackingType
 
,InvoiceNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+SM.SalesId  
                                        from TRN.SalesMaterial SM
										LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId
                                        WHERE SM.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
,InvoiceDate=REPLACE(REPLACE(
										STUFF((select distinct ', '+FORMAT(S.EntryDate,'dd-MMM-yyyy')
                                        from TRN.SalesMaterial SM
										LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId
                                        WHERE SM.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
,SalesQty=(Select SUM(SM.TransactionQty) from TRN.SalesMaterial SM LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId Where SM.SalesOrderId=SO.Id GROUP BY SM.SalesOrderId)
 
,SalesAmount=(Select SUM(SM.TransactionAmount) from TRN.SalesMaterial SM LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId Where SM.SalesOrderId=SO.Id GROUP BY SM.SalesOrderId)
 ,DispatchUpdateQty=ISNULL(DPC.DispatchPlanQty,0)
,DispatchStatus= CASE WHEN (Select SUM(SM.TransactionQty) from TRN.SalesMaterial SM LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId Where SM.SalesOrderId=SO.Id GROUP BY SM.SalesOrderId)>=ISNULL(so.Qty,0) THEN 'Completed' ELSE osS.UserName END
,DispatchPercentage=ISNULL(((Select SUM(SM.TransactionQty) from TRN.SalesMaterial SM LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId Where SM.SalesOrderId=SO.Id GROUP BY SM.SalesOrderId)/ISNULL(so.Qty,0))*100,0)
 
,ProductionCompletionStatus=CASE WHEN ISNULL(PRODPR.ProductionQtyAtPR,0)>= ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0) THEN 'Completed' ELSE ps.UserName END
,isnull(MOER.ExchangeRate,1) ExchangeRate,SO.ApprovedStatus,ESO.EmployeeName ApproveBy,FORMAT(SO.ApproveByDate,'dd-MMM-yyyy')ApprovalDate,AA.ArticlePartyName CustomerArticle,MOI.OwnReferenceNo OwnItemRef,MOI.OrderCostingMasterTemplateId
,SA.UserName,SA.Value
FROM trn.MasterOrder MO
left join MasterOrderExchangeRates MOER on MOER.TransactionId = MO.Id
LEFT JOIN org.Plant AS p2 ON p2.id=mo.PlantId
LEFT JOIN org.Entity AS e ON e.Id=mo.EntityId
left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
LEFT join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
left join [dbo].[SOCostingConfirmation] SCC on SCC.SalesOrderId = SO.Id and SCC.UserName = 'Gain & Loss'
left join [dbo].[SOCostingConfirmation] SCCCM on SCCCM.SalesOrderId = SO.Id and SCCCM.UserName = 'CM'
left join   (Select Sum(DispatchPlanQty) DispatchPlanQty , SOId from  [dbo].[DispatchPlanChild] group by SOId ) DPC ON DPC.SOId=SO.Id
LEFT JOIN dbo.EmployeeInformation ESO ON ESO.SystemId=SO.ApproveBy
LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
LEFT OUTER JOIN hkp.Season SS ON ss.Id=mo.SeasonId
LEFT OUTER JOIN hkp.PackingType PT ON PT.Id=SO.PackingTypeId
LEFT JOIN MST.PaymentTerm PTR ON PTR.Id=MO.PaymentTermId
 
LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
LEFT OUTER JOIN [TRN].[ProductionBulletinTemplate] AS PBT ON po.Id=PBT.ProductionOrderId
LEFT OUTER JOIN [TRN].[ProductionBulletinTemplateMaster] AS PBTM ON PBT.Id=PBTM.ProductionBulletinTemplateId AND ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
--left join (Select ProductionBulletinTemplateMasterId , Sum(TotalSpt)TotalSpt  from [TRN].[ProductionBulletinTemplateDetail] Group by ProductionBulletinTemplateMasterId) PBTD on PBTD.ProductionBulletinTemplateMasterId = PBTM.Id
left join (Select * from [dbo].[ProducitonBulletinCalculation] Where Totalspt  IS NOT NULL AND mctotalspt IS NOT NULL) PBTD on  PBTD.ProductionBulletinTemplateMasterId = PBTM.Id
LEFT OUTER JOIN [ExpectedSOWiseProductionCompletion] XCOM on XCOM.SalesOrderId=SO.Id 
LEFT JOIN org.Entity AS e2 ON e2.Id=po.EntityId
LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=po.Id
 
LEFT OUTER JOIN (
SELECT K.ProductionOrderID,max(K.LastProcessDate) AS LastProcessDate FROM (
SELECT ppt.ProductionOrderID,ppt.ProductionDate AS LastProcessDate
FROM ProductionPlanningType1 AS ppt
UNION ALL
SELECT ppt.ProductionOrderID,ppt.ProductionDate AS LastProcessDate
FROM trn.ProductionSummary AS ppt
) AS K GROUP BY K.ProductionOrderID
) AS L ON l.ProductionOrderID=po.Id
 
--production at PR Level
LEFT OUTER JOIN (
SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
FROM trn.ProductionSummary S
WHERE CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<=CONVERT(DATETIME, format(getdate(),'dd-MMM-yyyy'))
GROUP BY s.ProductionOrderId,s.ProcessId
) AS PRODPR ON PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
left outer join (SELECT pod.ProductionOrderId,
sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
FROM trn.SalesOrder AS so
INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
GROUP BY pod.ProductionOrderId
) AS PRDQ ON PRDQ.ProductionOrderId=po.Id
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
left outer join mst.MaterialMasterArticle AS mma on mma.id=moi.ArticleId
left join dbo.ArticleAlias AA ON AA.MasterOrderItemId=MOI.Id
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
 
left outer join [HKP].[Party] p on P.Id=MO.PartyId
left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
 
left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerDivisionId
left outer join [HKP].[BuyerDEPARTMENT] BDEP on BDEP.id=mo.BuyerDepartmentId
left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
left outer join mst.Destination DEST on dest.Id=so.DestinationId
left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
 
left outer join [HKP].[OrderCategory] OCS on ocS.id=So.OrderCategoryId
left outer join [HKP].[OrderStatus] OSS on OSS.id=So.OrderStatusId
 
left outer join hkp.Season S on s.id=mo.SeasonId
left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
 
LEFT JOIN (
select SA.SalesOrderId CId,AI.UserName,SA.Value from [dbo].[SalesAdditionalInfo] SA
LEFT JOIN HKP.AdditionalInfo AI ON AI.Id=SA.AdditionalInfoId
Where SA.SalesOrderId<>''
UNION ALL
select SA.LineItemId CId,AI.UserName,SA.Value from [dbo].[SalesAdditionalInfo] SA
LEFT JOIN HKP.AdditionalInfo AI ON AI.Id=SA.AdditionalInfoId
Where SA.LineItemId<>''

) SA ON SA.CId=SO.Id OR CId=MOI.Id
)A WHERE 1=2";

            DataTable dt = _sqlRepository.GetDataTable(strSql);

            var columns = new List<string>();
            foreach (DataColumn c in dt.Columns)
            {
                // UserName ও Value pivot এর জন্য ব্যবহৃত, ড্রপডাউনে দরকার নেই
                if (c.ColumnName == "UserName" || c.ColumnName == "Value") continue;
                columns.Add(c.ColumnName);
            }
            return columns;
        }
        public DataTable GetOS1MasterData(string column, string value)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // প্রতিটা ColumnName এর সর্বশেষ setting
            con.OpenDataSetThroughAdapter(
                "select ColumnName,Operator,OperatorValue from (" +
                "select ColumnName,Operator,OperatorValue, " +
                "ROW_NUMBER() over(partition by ColumnName order by StartDate desc) rn " +
                "from operationstatusSetting where DataName='Master' and StartDate <= '" + now + "'" +
                ") x where rn=1", out dsMaster, false, "1");

            string masterFilter = BuildMasterFilter(dsMaster);

            string strSql = @"SELECT B.ScheduleId,B.ProductionOrderID,B.MasterOrderNo,B.ExpectedCompletionDate,B.SODistributedQty,B.PlantId,B.Plant,B.MasterOrderEntityId,B.MasterOrderEntity,
B.ProductionOrderEntityId,B.ProductionOrderEntity,B.Customer,B.PartyType,B.PaymentTerm,B.PaymentTermDays,B.BaseOnDueDate,B.MatureDate,B.Buyer,B.Season,
B.TotalPlanQty,B.ProducedQty,B.RemainingPlanQuantity,B.BuyerDepartment,B.BuyerDivision,B.ResponsiblePerson,B.MasterOrderQty,
B.MOCreationDate,B.MOOrderCategory,B.MOOrderStatus,B.Type,B.BuyerOrderNo,B.OwnOrderNo,
B.LineItemId,B.BuyerReferenceNo,B.ProductionGrouping,B.MasterOrderItemCreationDate,B.ItemCategory,B.Remark,
B.Material,B.Article, B.ProductCategory, B.Product,B.ItemQty,B.UOM,B.ProductLibrayId,B.ProductCode,B.OrderRemarks,B.OrderControlStatus,
B.CriticalityLevel,B.MainRMInhouseRemarks,B.MainRMInhouseStatus,B.OtherRMInhouseRemarks,B.OtherRMInhouseStatus,B.InputRemarks,
B.InputStatus,B.SalesOrderId,B.ReviseDate ,B.DestinationId,B.Destination,
B.ShipmentModeId,B.ShipMode, B.SalesOrderCategoryId,B.SalesOrderCategory,
B.SalseOrderStatusId,B.SalseOrderStatus, B.SOQty
,B.DirectMaterialCost,B.DirectProcessCost,B.ValueLoss,B.SalesExpense,B.CM,B.[Gain & Loss],B.[Other Cost],B.Rate,B.Discount,B.NetSalesRealization
,B.DeliveryGroup,B.DeliveryDate, B.CommitmentDate,B.PlanExFactoryDate
,B.SOMainRawMaterialInhouseDate,B.SOOtherRawMaterialInhouseDate,B.SOLSD
,B.PONumber,B.Description,B.LineItemReference,B.SalesOrderCreationDate,
B.ProductionOrderCreationDate,B.ProductionStatus
,B.FirstProcessWorkCenter
,B.NoOfWorkStation, B.Efficiency,
B.[S.SPT], B.PlanWorkingHoursPerDay, B.FirstDayOutPut,
B.PlanTargetPerHour, B.IncrementValue, B.IncrementType,
B.DayToReachTheTarget, B.POCommitmentDate,
B.ProductionPriority, B.TargetPerHour, B.TargetPerDay,
B.MinimumLineDays, B.RequiredLineDays,
B.RequiredNoOfLines, B.AllocatedLines, B.ExplicitProductionQty,
B.PRLSD, B.PRMainRawMaterialInhouseDate, B.PROtherRawMaterialInhouseDate,
B.RunningOrderBlockSize,B.ProductionCompletionDate,B.ActiveOrderLinePreference,B.RunningOrderLinePreference
,B.PlannedLinePreference,B.ProductionType,B.ShipmentFromStock,B.PackingType,B.InvoiceNo
,B.InvoiceDate,B.SalesQty,B.SalesAmount,B.DispatchUpdateQty,B.DispatchStatus,B.DispatchPercentage
,B.ProductionCompletionStatus,B.ExchangeRate,B.ApprovedStatus,B.ApproveBy,B.ApprovalDate,B.CustomerArticle,B.OwnItemRef,B.OrderCostingMasterTemplateId,B.UserName,B.Value
,B.CSPT,B.BulletinID, B.[B.Spt] AS BSPT , B.[B.M.Spt] AS MBSPT
,B.OrderStatusUpdateDate,B.OderStatusUpdateBy
into #tempOT from
(select A.SalesOrderId,A.UserName ,A.Value,A.MasterOrderNo,A.ExpectedCompletionDate,A.SODistributedQty,A.PlantId,A.Plant,A.MasterOrderEntityId,A.MasterOrderEntity,
A.ProductionOrderEntityId,A.ProductionOrderEntity,A.Customer,A.PartyType,A.PaymentTerm,A.PaymentTermDays,A.BaseOnDueDate,A.MatureDate,A.Buyer,A.Season,
A.TotalPlanQty,A.ProducedQty,A.RemainingPlanQuantity,A.BuyerDepartment,A.BuyerDivision,A.ResponsiblePerson,A.MasterOrderQty,
A.MOCreationDate,A.MOOrderCategory,A.MOOrderStatus,A.Type,A.BuyerOrderNo,A.OwnOrderNo,
A.LineItemId,A.BuyerReferenceNo,A.ProductionGrouping,A.MasterOrderItemCreationDate,A.ItemCategory,A.Remark,
A.Material,A.Article, A.ProductCategory, A.Product,A.ItemQty,A.UOM,A.ProductLibrayId,A.ProductCode,A.OrderRemarks,A.OrderControlStatus,
A.CriticalityLevel,A.MainRMInhouseRemarks,A.MainRMInhouseStatus,A.OtherRMInhouseRemarks,A.OtherRMInhouseStatus,A.InputRemarks,
A.InputStatus,A.ReviseDate ,A.DestinationId,A.Destination,
A.ShipmentModeId,A.ShipMode, A.SalesOrderCategoryId,A.SalesOrderCategory,
A.SalseOrderStatusId,A.SalseOrderStatus, A.SOQty,A.CM,A.[Gain & Loss],A.[Other Cost] ,A.Rate
,A.SalesExpense,A.Discount,A.NetSalesRealization,A.DirectMaterialCost,A.DirectProcessCost,A.ValueLoss,A.DeliveryGroup,A.DeliveryDate, A.CommitmentDate,A.PlanExFactoryDate
, A.SOMainRawMaterialInhouseDate,A.SOOtherRawMaterialInhouseDate,A.SOLSD
,A.PONumber,A.Description,A.LineItemReference,A.SalesOrderCreationDate,
A.ProductionOrderCreationDate,A.ScheduleId,A.ProductionOrderID,A.ProductionStatus
,A.FirstProcessWorkCenter
,A.NoOfWorkStation, A.Efficiency,
A.[S.SPT], A.PlanWorkingHoursPerDay, A.FirstDayOutPut,
A.PlanTargetPerHour, A.IncrementValue, A.IncrementType,
A.DayToReachTheTarget, A.POCommitmentDate,
A.ProductionPriority, A.TargetPerHour, A.TargetPerDay,
A.MinimumLineDays, A.RequiredLineDays,
A.RequiredNoOfLines, A.AllocatedLines, A.ExplicitProductionQty,
A.PRLSD, A.PRMainRawMaterialInhouseDate, A.PROtherRawMaterialInhouseDate,
A.RunningOrderBlockSize,A.ProductionCompletionDate,A.ActiveOrderLinePreference,A.RunningOrderLinePreference
,A.PlannedLinePreference,A.ProductionType,A.ShipmentFromStock,A.PackingType,A.InvoiceNo
,A.InvoiceDate,A.SalesQty,A.SalesAmount,A.DispatchUpdateQty,A.DispatchStatus,A.DispatchPercentage
,A.ProductionCompletionStatus,A.ExchangeRate,A.ApprovedStatus,A.ApproveBy,A.ApprovalDate,A.CustomerArticle,A.OwnItemRef,A.OrderCostingMasterTemplateId
,A.CSPT,A.BulletinID, A.[B.Spt] , A.[B.M.Spt]
,A.OrderStatusUpdateDate,A.OderStatusUpdateBy
from
(
SELECT mo.MasterOrderNo,format(XCOM.ExpectedCompletionDate,'dd-MMM-yyyy') AS ExpectedCompletionDate,XCOM.Quantity SODistributedQty
,p2.Id PlantId,p2.UserName AS Plant,
e.Id AS MasterOrderEntityId,e.UserName AS MasterOrderEntity,
e2.Id AS ProductionOrderEntityId,e2.UserName AS ProductionOrderEntity,
p.UserName AS Customer
,PartyType=(select distinct PAG.UserName PartyType from HKP.CompanyParty CP
LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId
Where CP.PartyType='Customer' AND mo.PartyId=CP.PartyId),PTR.UserName PaymentTerm,MO.PaymentTermDays,FORMAT(MO.BaseOnDueDate,'dd-MMM-yyyy')BaseOnDueDate,FORMAT(MO.MatureDate,'dd-MMM-yyyy')MatureDate
,b.UserName AS Buyer,ss.UserName AS Season,
ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0) AS TotalPlanQty,
ISNULL(PRODPR.ProductionQtyAtPR,0) AS ProducedQty,
ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS RemainingPlanQuantity,
 
BDEP.UserName AS BuyerDepartment,bd.UserName AS BuyerDivision, ei.EmployeeName AS ResponsiblePerson,MO.TotalQty MasterOrderQty,
FORMAT(MO.AddedDate,'dd-MMM-yyyy') MOCreationDate,OC.UserName AS MOOrderCategory,os.UserName AS MOOrderStatus,MO.Type, mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo,
MOI.Id AS LineItemId,MOI.BuyerReferenceNo,moi.ProductionGrouping,FORMAT(MOI.AddedDate,'dd-MMM-yyyy') MasterOrderItemCreationDate,MOI.ItemCategory,MOI.Remark,
mm.UserName AS Material,mma.StandardName AS Article, pc.UserName AS ProductCategory, pm.UserName AS Product,MOI.TotalQty AS ItemQty,uom.UserName AS UOM,
PL.Id ProductLibrayId,PL.Code ProductCode
,MOI.CSPT 
,OrderRemarks=REPLACE(REPLACE(
   STUFF((select distinct ', '+FORMAT(AMTR.AddedDate,'dd-MMM-yyyy')+'-'+AMTR.Remarks 
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'ShipmentControl' AND B.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,OrderControlStatus=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.Status
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'ShipmentControl' AND B.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,CriticalityLevel=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.CriticalityLevel
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'ShipmentControl' AND B.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,MainRMInhouseRemarks=REPLACE(REPLACE(
   STUFF((select distinct ', '+FORMAT(AMTR.AddedDate,'dd-MMM-yyyy')+'-'+AMTR.Remarks 
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'MainRMInhouse' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,MainRMInhouseStatus=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.Status  
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'MainRMInhouse' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,OtherRMInhouseRemarks=REPLACE(REPLACE(
   STUFF((select distinct ', '+FORMAT(AMTR.AddedDate,'dd-MMM-yyyy')+'-'+AMTR.Remarks  
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'OtherRMInhouse' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,OtherRMInhouseStatus=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.Status  
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'OtherRMInhouse' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
 
,InputRemarks=REPLACE(REPLACE(
   STUFF((select distinct ', '+FORMAT(AMTR.AddedDate,'dd-MMM-yyyy')+'-'+AMTR.Remarks 
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'BaseProcessInput' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
,InputStatus=REPLACE(REPLACE(
   STUFF((select distinct ', '+B.Status  
   FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'BaseProcessInput' AND B.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')
 
 
,so.Id AS SalesOrderId,FORMAT(SO.ReviseDate,'dd-MMM-yyyy')ReviseDate ,so.DestinationId,dest.UserName AS Destination,
so.ShipmentModeId,smo.UserName AS ShipMode, OCS.Id SalesOrderCategoryId,OCS.UserName AS SalesOrderCategory,
OSS.Id SalseOrderStatusId,osS.UserName AS SalseOrderStatus, ISNULL(so.Qty,0) SOQty,SCCCM.SOValue CM, SCC.SOValue 'Gain & Loss',SO.Rate
,SO.SalesExpense,SO.Discount,NetSalesRealization=ISNULL((SO.SalesExpense - SO.Discount ),0),SO.DirectMaterialCost,SO.DirectProcessCost,SO.ValueLoss,SO.Other [Other Cost]
,SO.DeliveryGroup,FORMAT(so.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, FORMAT(so.CommitmentDate,'dd-MMM-yyyy') CommitmentDate, FORMAT(so.PlanExFactoryDate,'dd-MMM-yyyy') PlanExFactoryDate
, FORMAT(so.MainRawMaterialInhouseDate,'dd-MMM-yyyy') SOMainRawMaterialInhouseDate,
FORMAT(so.OtherRawMaterialInhouseDate,'dd-MMM-yyyy') SOOtherRawMaterialInhouseDate,FORMAT(so.LSD,'dd-MMM-yyyy') SOLSD
,CP.PONumber,SO.Description,SO.LineItemReference,FORMAT(so.AddedDate,'dd-MMM-yyyy') SalesOrderCreationDate,FORMAT(so.OrderStatusChangedDate,'dd-MMM-yyyy') OrderStatusUpdateDate
,SO.OrderStatusChangedBy OderStatusUpdateBy,FORMAT(PO.AddedDate,'dd-MMM-yyyy')ProductionOrderCreationDate,t.Id ScheduleId,PO.Id ProductionOrderID,ps.UserName AS ProductionStatus
,FirstProcessWorkCenter=STUFF((select distinct ','+xw.UserName from
ProductionOrderFirstProcessWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, '')
,t.NoOfWorkStation, t.Efficiency,
t.SPT 'S.SPT', t.PlanWorkingHoursPerDay, t.FirstDayOutPut,
t.PlanTargetPerHour, t.IncrementValue, t.IncrementType,
t.DayToReachTheTarget, t.CommitmentDate POCommitmentDate,
t.ProductionPriority, t.TargetPerHour, t.TargetPerDay,
t.MinimumLineDays, t.RequiredLineDays,
t.RequiredNoOfLines, t.AllocatedLines, t.Qty AS ExplicitProductionQty,
t.LSD AS PRLSD, t.MainRawMaterialInhouseDate AS PRMainRawMaterialInhouseDate, t.OtherRawMaterialInhouseDate AS PROtherRawMaterialInhouseDate,
t.RunningOrderBlockSize,l.LastProcessDate AS ProductionCompletionDate
,PBT.Id BulletinID,PBTD.Totalspt 'B.SPT',PBTD.mctotalspt 'B.M.SPT'
,ActiveOrderLinePreference=STUFF((select distinct ','+xw.UserName from
trn.ProductionOrderWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, '')
 
,RunningOrderLinePreference=STUFF((select distinct ','+xw.UserName from
trn.RunningOrderWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, '')
 
,PlannedLinePreference=STUFF((select distinct ','+xw.UserName from
ProductionPlanningType1 AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, '')
,SO.ProductionType,ShipmentFromStock= CASE WHEN SO.ShipmentFromStock=1 THEN 'Yes' ELSE 'No' END,PT.UserName PackingType
 
,InvoiceNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+SM.SalesId  
                                        from TRN.SalesMaterial SM
										LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId
                                        WHERE SM.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
,InvoiceDate=REPLACE(REPLACE(
										STUFF((select distinct ', '+FORMAT(S.EntryDate,'dd-MMM-yyyy')
                                        from TRN.SalesMaterial SM
										LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId
                                        WHERE SM.SalesOrderId=SO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
,SalesQty=(Select SUM(SM.TransactionQty) from TRN.SalesMaterial SM LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId Where SM.SalesOrderId=SO.Id GROUP BY SM.SalesOrderId)
 
,SalesAmount=(Select SUM(SM.TransactionAmount) from TRN.SalesMaterial SM LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId Where SM.SalesOrderId=SO.Id GROUP BY SM.SalesOrderId)
 ,DispatchUpdateQty=ISNULL(DPC.DispatchPlanQty,0)
,DispatchStatus= CASE WHEN (Select SUM(SM.TransactionQty) from TRN.SalesMaterial SM LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId Where SM.SalesOrderId=SO.Id GROUP BY SM.SalesOrderId)>=ISNULL(so.Qty,0) THEN 'Completed' ELSE osS.UserName END
,DispatchPercentage=ISNULL(((Select SUM(SM.TransactionQty) from TRN.SalesMaterial SM LEFT JOIN TRN.Sales S ON S.Id=SM.SalesId Where SM.SalesOrderId=SO.Id GROUP BY SM.SalesOrderId)/ISNULL(so.Qty,0))*100,0)
 
,ProductionCompletionStatus=CASE WHEN ISNULL(PRODPR.ProductionQtyAtPR,0)>= ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0) THEN 'Completed' ELSE ps.UserName END
,isnull(MOER.ExchangeRate,1) ExchangeRate,SO.ApprovedStatus,ESO.EmployeeName ApproveBy,FORMAT(SO.ApproveByDate,'dd-MMM-yyyy')ApprovalDate,AA.ArticlePartyName CustomerArticle,MOI.OwnReferenceNo OwnItemRef,MOI.OrderCostingMasterTemplateId
,SA.UserName,SA.Value
FROM trn.MasterOrder MO
left join MasterOrderExchangeRates MOER on MOER.TransactionId = MO.Id
LEFT JOIN org.Plant AS p2 ON p2.id=mo.PlantId
LEFT JOIN org.Entity AS e ON e.Id=mo.EntityId
left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
LEFT join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
left join [dbo].[SOCostingConfirmation] SCC on SCC.SalesOrderId = SO.Id and SCC.UserName = 'Gain & Loss'
left join [dbo].[SOCostingConfirmation] SCCCM on SCCCM.SalesOrderId = SO.Id and SCCCM.UserName = 'CM'
left join   (Select Sum(DispatchPlanQty) DispatchPlanQty , SOId from  [dbo].[DispatchPlanChild] group by SOId ) DPC ON DPC.SOId=SO.Id
LEFT JOIN dbo.EmployeeInformation ESO ON ESO.SystemId=SO.ApproveBy
LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
LEFT OUTER JOIN hkp.Season SS ON ss.Id=mo.SeasonId
LEFT OUTER JOIN hkp.PackingType PT ON PT.Id=SO.PackingTypeId
LEFT JOIN MST.PaymentTerm PTR ON PTR.Id=MO.PaymentTermId
 
LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
LEFT OUTER JOIN [TRN].[ProductionBulletinTemplate] AS PBT ON po.Id=PBT.ProductionOrderId
LEFT OUTER JOIN [TRN].[ProductionBulletinTemplateMaster] AS PBTM ON PBT.Id=PBTM.ProductionBulletinTemplateId AND ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
--left join (Select ProductionBulletinTemplateMasterId , Sum(TotalSpt)TotalSpt  from [TRN].[ProductionBulletinTemplateDetail] Group by ProductionBulletinTemplateMasterId) PBTD on PBTD.ProductionBulletinTemplateMasterId = PBTM.Id
left join (Select * from [dbo].[ProducitonBulletinCalculation] Where Totalspt  IS NOT NULL AND mctotalspt IS NOT NULL) PBTD on  PBTD.ProductionBulletinTemplateMasterId = PBTM.Id
LEFT OUTER JOIN [ExpectedSOWiseProductionCompletion] XCOM on XCOM.SalesOrderId=SO.Id 
LEFT JOIN org.Entity AS e2 ON e2.Id=po.EntityId
LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=po.Id
 
LEFT OUTER JOIN (
SELECT K.ProductionOrderID,max(K.LastProcessDate) AS LastProcessDate FROM (
SELECT ppt.ProductionOrderID,ppt.ProductionDate AS LastProcessDate
FROM ProductionPlanningType1 AS ppt
UNION ALL
SELECT ppt.ProductionOrderID,ppt.ProductionDate AS LastProcessDate
FROM trn.ProductionSummary AS ppt
) AS K GROUP BY K.ProductionOrderID
) AS L ON l.ProductionOrderID=po.Id
 
--production at PR Level
LEFT OUTER JOIN (
SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
FROM trn.ProductionSummary S
WHERE CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<=CONVERT(DATETIME, format(getdate(),'dd-MMM-yyyy'))
GROUP BY s.ProductionOrderId,s.ProcessId
) AS PRODPR ON PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
left outer join (SELECT pod.ProductionOrderId,
sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
FROM trn.SalesOrder AS so
INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
GROUP BY pod.ProductionOrderId
) AS PRDQ ON PRDQ.ProductionOrderId=po.Id
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
left outer join mst.MaterialMasterArticle AS mma on mma.id=moi.ArticleId
left join dbo.ArticleAlias AA ON AA.MasterOrderItemId=MOI.Id
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
 
left outer join [HKP].[Party] p on P.Id=MO.PartyId
left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
 
left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerDivisionId
left outer join [HKP].[BuyerDEPARTMENT] BDEP on BDEP.id=mo.BuyerDepartmentId
left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
left outer join mst.Destination DEST on dest.Id=so.DestinationId
left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
 
left outer join [HKP].[OrderCategory] OCS on ocS.id=So.OrderCategoryId
left outer join [HKP].[OrderStatus] OSS on OSS.id=So.OrderStatusId
 
left outer join hkp.Season S on s.id=mo.SeasonId
left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
 
LEFT JOIN (
select SA.SalesOrderId CId,AI.UserName,SA.Value from [dbo].[SalesAdditionalInfo] SA
LEFT JOIN HKP.AdditionalInfo AI ON AI.Id=SA.AdditionalInfoId
Where SA.SalesOrderId<>''
UNION ALL
select SA.LineItemId CId,AI.UserName,SA.Value from [dbo].[SalesAdditionalInfo] SA
LEFT JOIN HKP.AdditionalInfo AI ON AI.Id=SA.AdditionalInfoId
Where SA.LineItemId<>''

) SA ON SA.CId=SO.Id OR CId=MOI.Id

  

)A  " + masterFilter + @"
)B 
DECLARE @sql nvarchar(max), @col nvarchar(max)
                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(REPLACE(CONVERT(VARCHAR(40), UserName, 113), '', ''))    
                                FROM #tempOT
                                FOR XML PATH ('')
                            )   SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([Value]) FOR [UserName] IN ('+STUFF(@col,1,1,'')+')
                            ) as pvt)' 
                            EXEC sp_executesql @sql
                            drop table #tempOT
";
            return _sqlRepository.GetDataTable(strSql);

        }
        // alias নাম -> ভেতরের আসল কলাম। D = date, N = number, T = text
        private static readonly Dictionary<string, string> MasterColMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
    // Date
    {"MasterOrderItemCreationDate", "D|MOI.AddedDate"},
    {"MOCreationDate",              "D|MO.AddedDate"},
    {"SalesOrderCreationDate",      "D|SO.AddedDate"},
    {"ProductionOrderCreationDate", "D|PO.AddedDate"},
    {"ReviseDate",                  "D|SO.ReviseDate"},
    {"DeliveryDate",                "D|SO.DeliveryDate"},
    {"CommitmentDate",              "D|SO.CommitmentDate"},
    {"PlanExFactoryDate",           "D|SO.PlanExFactoryDate"},
    {"SOLSD",                       "D|SO.LSD"},
    {"SOMainRawMaterialInhouseDate","D|SO.MainRawMaterialInhouseDate"},
    {"SOOtherRawMaterialInhouseDate","D|SO.OtherRawMaterialInhouseDate"},
    {"OrderStatusUpdateDate",       "D|SO.OrderStatusChangedDate"},
    {"ApprovalDate",                "D|SO.ApproveByDate"},
    {"ExpectedCompletionDate",      "D|XCOM.ExpectedCompletionDate"},
    {"BaseOnDueDate",               "D|MO.BaseOnDueDate"},
    {"MatureDate",                  "D|MO.MatureDate"},
    {"ProductionCompletionDate",    "D|L.LastProcessDate"},
    {"PRLSD",                       "D|t.LSD"},
    {"POCommitmentDate",            "D|t.CommitmentDate"},
    // Number
    {"SOQty",           "N|ISNULL(so.Qty,0)"},
    {"MasterOrderQty",  "N|MO.TotalQty"},
    {"ItemQty",         "N|MOI.TotalQty"},
    {"Rate",            "N|SO.Rate"},
    // Text
    {"MasterOrderNo",   "T|MO.MasterOrderNo"},
    {"Customer",        "T|p.UserName"},
    {"Buyer",           "T|b.UserName"},
    {"Season",          "T|ss.UserName"},
        };

        private string BuildMasterFilter(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return "";

            var conditions = new List<string>();

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                string col = Convert.ToString(r["ColumnName"]).Trim();
                string op = Convert.ToString(r["Operator"]).Trim();
                string val = Convert.ToString(r["OperatorValue"]).Trim();
                if (col == "" || op == "" || val == "") continue;

                switch (op)
                {
                    case "=>": case ">=": op = ">="; break;
                    case "=<": case "<=": op = "<="; break;
                    case ">": case "<": case "=": case "<>": break;
                    default: throw new Exception("Invalid operator: " + op);
                }

                string colSql = "A.[" + col.Replace("]", "]]") + "]";
                DateTime dt; decimal num;

                if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
                    conditions.Add("TRY_CONVERT(decimal(38,6), " + colSql + ") " + op + " " + num.ToString(CultureInfo.InvariantCulture));
                else if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    conditions.Add("TRY_CONVERT(date, " + colSql + ") " + op + " '" + dt.ToString("yyyy-MM-dd") + "'");
                else
                    conditions.Add(colSql + " " + op + " N'" + val.Replace("'", "''") + "'");
            }

            return conditions.Count == 0 ? "" : " WHERE " + string.Join(" AND ", conditions) + " ";
        }

        private string BuildMasterFilterBlank(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return "";

            var conditions = new List<string>();

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                string col = Convert.ToString(r["ColumnName"]).Trim();
                string op = Convert.ToString(r["Operator"]).Trim();
                string val = Convert.ToString(r["OperatorValue"]).Trim();
                if (col == "" || op == "" || val == "") continue;

                switch (op)
                {
                    case "=>": case ">=": op = ">="; break;
                    case "=<": case "<=": op = "<="; break;
                    case ">": case "<": case "=": case "<>": break;
                    default: throw new Exception("Invalid operator: " + op);
                }

                string colSql = "[" + col.Replace("]", "]]") + "]";
                DateTime dt; decimal num;

                if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
                    conditions.Add("TRY_CONVERT(decimal(38,6), " + colSql + ") " + op + " " + num.ToString(CultureInfo.InvariantCulture));
                else if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    conditions.Add("TRY_CONVERT(date, " + colSql + ") " + op + " '" + dt.ToString("yyyy-MM-dd") + "'");
                else
                    conditions.Add(colSql + " " + op + " N'" + val.Replace("'", "''") + "'");
            }

            return conditions.Count == 0 ? "" : " WHERE " + string.Join(" AND ", conditions) + " ";
        }

        public List<string> GetOS2ColumnList()
        {
            string strSql = @"select
isnull(WC.GroupingData, 'Internal') AS GroupingData, WC.Id as WC_Id,WC.Code, WC.UserName as WorkCenter,p1.ProductionDate AS TargetDate,pp.UserName PlanningProcess, po.Id AS ProductionOrderID,
p1.Quantity AS PlannedQtyForTheDay,t1.SPT,t1.ProductionPriority,pm.UserName AS Product,
pc.UserName AS ProductCategory,mm.UserName AS Material,ord.Article,
BuyerRefNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
   trn.MasterOrder XMOI
   INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId = XMOI.Id
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = moi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



OwnRefNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
   trn.MasterOrder XMOI
   INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId = XMOI.Id
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = moi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



StyleNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = XMOI.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),

OwnStyleNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = XMOI.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



SONo = STUFF((select distinct ',' + sox.Id from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



SODesc = STUFF((select distinct ',' + sox.[Description] from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



buyer = STUFF((select distinct ',' + XB.UserName from
    trn.SalesOrder XSO
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
left outer join[HKP].Buyer XB on XB.Id = XMO.BuyerId
where T1.ProductionOrderId = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),




Customer = STUFF((select distinct ',' + XP.UserName from
    trn.SalesOrder XSO
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
left outer join[HKP].[Party] Xp on XP.Id = XMO.PartyId
where T1.ProductionOrderId = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),

StyleGroup = STUFF((select distinct ',' + xmoi.ProductionGrouping from
    trn.SalesOrder XSO
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

where PO.Id = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),

MasterOrderNo = STUFF((select distinct ',' + XMO.MasterOrderNo from
    trn.SalesOrder XSO
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
where PO.Id = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),


ord.FOB,ord.CM,CEILING(t1.TargetPerDay) AS LineTargetPerDay, ord.OrderQty,ord.PlannedQty AS PlanOrderQty,prodWC.ProductionQtyAtWC,prodpr.ProductionQtyAtPR,u.UserName AS Unit,
ACCI.EmployeeName AS AccountIncharge,acch.EmployeeName AS AccountHolder,ord.FirstDeliveryDate,ord.LastDeliveryDate,ord.ProductionCompletionDate,
DATEADD(DD, ISNULL(PRDTIME.LastDayOfProduction, 0) * -1, ord.LastDeliveryDate) AS BaseProcessCompletionDate,
    ps.UserName AS ProductionStatus,ORD.Currency,ORD.UOM,E.UserName AS Entity,PLN.UserName AS Plant,



ord.CM* p1.Quantity AS BookedCM,t1.NoOfWorkStation,t1.LSD,t1.CommitmentDate,t1.MainRawMaterialInhouseDate,
t1.OtherRawMaterialInhouseDate,
--ord.OrderStatus,
PRODWC.LineStartDateAtWC,PRODWC.LineEndDateAtWC,
PRODPR.LineStartDateAtPR,PRODPR.LineEndDateAtPR,

p3.PlanningStartDateAtWC,p3.PlanningEndDateAtWC,p3.PlanningQtyAtWC,
p2.PlanningStartDateAtPR,p2.PlanningEndDateAtPR, p2.PlanningQtyAtPR,




--targetdate,earlyStartDate,
p1.ProductionHours AS WorkingHours,t1.PlanWorkingHoursPerDay,
t1.TargetPerDay / t1.PlanWorkingHoursPerDay AS PlanTargetPerHour,
  ppc.WorkingHours AS CalendarWorkingHours,p1.isBuildUp,
WC.StandardTimePerDay AS StandardWorkingHours, eff.StandardWorkingHourCost,eff.AdditionalWorkingHourCostPerHour,
wc.NoOfWorkStation AS StandardWorkStations,wc.DailyFixedCost,wc.VariableCost AS VariableCostPerHour
,bul.Id BulletinId, bul.BulletinTemplateId, bul.MachineSPT, bul.NonMachineSPT,
bul.MachineManpower, bul.NonMachineManpower,bul.TotalSPT,wcm.UserName FirstProcessWC, P.UserName FirstProcess


from trn.ProductionOrder PO
inner
join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID = po.Id
INNER
join ProductionPlanningType1 p1 on p1.ProductionOrderID = t1.ProductionOrderID and ProcessID = (select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = po.Id)



LEFT JOIN ProductionPlanningCalendar AS ppc ON ppc.EntityID = po.EntityId AND ppc.ProcessID = p1.ProcessID AND ppc.WorkingDate = p1.ProductionDate

left outer join scs.WorkCenterMaster WC on wc.id = p1.WorkCenterMasterId
LEFT OUTER JOIN EmployeeInformation AS ACCI ON ACCI.SystemId = wc.AccountInCharge
LEFT OUTER JOIN EmployeeInformation AS ACCH ON ACCH.SystemId = wc.AccountHolder



LEFT OUTER JOIN(SELECT MAX(days) AS LastDayOfProduction, pops.ProductionOrderId
FROM trn.ProductionOrderProcessSet AS pops WHERE pops.Symbol = '+'
GROUP BY pops.ProductionOrderId) AS PRDTIME ON prdtime.ProductionOrderId = po.Id



left outer join mst.MaterialMaster mm on mm.id = p1.MaterialMasterId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId = mm.Id
left outer join[MST].[ProductMaster] PM on pm.id = pd.ProductMasterId
left outer join[HKP].[ProductCategory] PC on pc.Id = pm.ProductCategoryId
LEFT JOIN[TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId = PM.Id AND EFF.EfficencyName = 'Costing'


left outer join org.Entity E on e.Id = p1.EntityID
LEFT OUTER JOIN org.Unit AS u ON u.Id = e.UnitId
left outer join org.Plant PLN on pln.Id = PO.PlantId
LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId

--Bulletin Data

                        LEFT JOIN(SELECT T.Id, T.BulletinTemplateId, T.ProductionOrderId, TM.ProcessId, SUM(D.TotalSPT) AS TotalSPT,
                            SUM(CASE WHEN ISNULL(D.MachineVarientId,'')<> '' THEN D.TotalSPT ELSE 0 END) AS MachineSPT,
                             SUM(CASE WHEN ISNULL(D.MachineVarientId, '') = '' THEN D.TotalSPT ELSE 0 END) AS NonMachineSPT,
                                SUM(CASE WHEN ISNULL(D.MachineVarientId, '') <> '' THEN D.AllotedManpower ELSE 0 END) AS MachineManpower,
                                   SUM(CASE WHEN ISNULL(D.MachineVarientId, '') = '' THEN D.AllotedManpower ELSE 0 END) AS NonMachineManpower

                            FROM trn.ProductionBulletinTemplate T

                            JOIN trn.ProductionBulletinTemplateMaster AS  TM ON tm.ProductionBulletinTemplateId = t.Id

                            JOIN trn.ProductionBulletinTemplateDetail AS D ON d.ProductionBulletinTemplateMasterId = TM.Id

                                       group by T.Id, T.BulletinTemplateId, T.ProductionOrderId,TM.ProcessId
						) BUL ON bul.ProductionOrderId = po.Id AND p1.ProcessID = BUL.ProcessId

  --planning at PR Level
left outer join(
SELECT P1.ProductionOrderID, P1.ProcessID,
MIN(ProductionDate) AS PlanningStartDateAtPR, SUM(Quantity) AS PlanningQtyAtPR, max(ProductionDate) AS PlanningEndDateAtPR
FROM ProductionPlanningType1 p1
group by P1.ProductionOrderID, P1.ProcessID
) as p2
on p2.ProductionOrderID = t1.ProductionOrderID
and p2.ProcessID = (select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = p1.ProductionOrderID)
left join hkp.process PP on pp.id = p2.ProcessID
--planning at WC Level
left outer join(
SELECT P1.ProductionOrderID, P1.ProcessID, p1.WorkCenterMasterId,
MIN(ProductionDate) AS PlanningStartDateAtWC, SUM(Quantity)AS PlanningQtyAtWC, max(ProductionDate) AS PlanningEndDateAtWC FROM ProductionPlanningType1 p1
group by P1.ProductionOrderID, P1.ProcessID, p1.WorkCenterMasterId
) as p3
on p1.WorkCenterMasterId = p3.WorkCenterMasterId
AND p3.ProductionOrderID = t1.ProductionOrderID
and p3.ProcessID = (select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = p1.ProductionOrderID)





--production at WC Level
LEFT OUTER JOIN(
SELECT s.EntityId, s.WorkCenterMasterId, s.ProductionOrderId, s.ProcessId, SUM(s.Quantity) AS ProductionQtyAtWC, MIN(s.ProductionDate) AS LineStartDateAtWC, MAX(s.ProductionDate) AS LineENDDateAtWC
FROM trn.ProductionSummary S
GROUP BY s.ProductionOrderId, s.WorkCenterMasterId, s.EntityId, s.ProcessId
) AS PRODWC ON p1.EntityID = PRODWC.EntityId AND PRODWC.WorkCenterMasterId = p1.WorkCenterMasterId AND p1.ProcessID = PRODWC.ProcessId AND PRODWC.ProductionOrderId = p1.ProductionOrderID



      --production at PR Level
LEFT OUTER JOIN(
SELECT s.ProductionOrderId, s.ProcessId, SUM(s.Quantity) AS ProductionQtyAtPR, MIN(s.ProductionDate) AS LineStartDateAtPR, MAX(s.ProductionDate) AS LineEndDateAtPR
FROM trn.ProductionSummary S
GROUP BY s.ProductionOrderId, s.ProcessId
) AS PRODPR ON PRODPR.ProductionOrderId = p1.ProductionOrderID AND p1.ProcessID = PRODPR.ProcessId
LEFT JOIN[dbo].[ProductionOrderFirstProcessWorkCenter] FP ON FP.ProductionOrderId = p1.ProductionOrderID
                            LEFT JOIN SCS.WorkCenterMaster wcm ON wcm.Id = FP.WorkCenterMasterId
                            LEFT JOIN HKP.Process P ON P.Id = FP.ProcessId
left outer join(
select POD.ProductionOrderId, ma.StandardName AS Article,
SUM(CEILING((isnull(SO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0))))) AS PlannedQty,
--SUM((so.Qty * (1 + (moi.ExtraOrderPercentage / 100))) * (1 + (moi.OrderWastagePercentage / 100))) AS PlannedQty,
min(so.DeliveryDate) AS FirstDeliveryDate, cur.Code AS Currency, uom.UserName AS UOM,
max(so.DeliveryDate) AS LastDeliveryDate,
MAX(so.CommitmentDate) AS ProductionCompletionDate,
SUM(so.Rate * so.Qty) / SUM(so.Qty) AS FOB, SUM(so.CM * so.Qty) / SUM(so.Qty) AS CM,
sum(SO.Qty) AS OrderQty from trn.ProductionOrderDetail POD
left outer join trn.SalesOrder SO on so.id = pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
LEFT OUTER JOIN[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
left outer join trn.MasterOrder MO on mo.Id = moi.MasterOrderId
left outer join[HKP].[Party] p on P.Id = MO.plantID
left outer join[HKP].[PartyPlant] PPI on ppi.id = mo.InvoicingPartyPlantId
left outer join[HKP].[PartyPlant] PPD on ppd.id = mo.DeliveryPartyPlantId
left outer join[HKP].[Buyer] B on b.id = mo.BuyerId
left outer join[HKP].[BuyerBrand] BB on bb.id = mo.BuyerBrandId
left outer join[HKP].[BuyerDivision] BD on bd.id = mo.BuyerBrandId
left outer join[HKP].[OrderCategory] OC on oc.id = mo.OrderCategoryId
--left outer join[HKP].[OrderStatus] OS on OS.id = mo.OrderStatusId
LEFT OUTER JOIN trn.Commitment AS c ON c.Id = mo.CommitmentId
left outer join mst.Destination DEST on dest.Id = so.DestinationId
left outer join[TRN].[CustomerPO] CPO ON CPO.Id = so.CustomerPOId
left outer join[MST].[ShipMode] SMO on SMO.Id = so.ShipmentModeId
left outer join hkp.Season S on s.id = mo.SeasonId
left outer join EmployeeInformation EI on ei.SystemId = MO.ResponsiblePersonId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id = MO.TotalQtyUOMId
LEFT OUTER JOIN scs.Currency AS cur ON cur.Id = mo.CurrencyId
group by POD.ProductionOrderId, ma.StandardName, cur.Code, uom.UserName--, MO.MasterOrderNo, b.UserName, os.UserName
) AS ORD on ord.ProductionOrderID = PO.Id

WHERE 1=2";

            DataTable dt = _sqlRepository.GetDataTable(strSql);

            var columns = new List<string>();
            foreach (DataColumn c in dt.Columns)
            {
                // UserName ও Value pivot এর জন্য ব্যবহৃত, ড্রপডাউনে দরকার নেই
                if (c.ColumnName == "UserName" || c.ColumnName == "Value") continue;
                columns.Add(c.ColumnName);
            }
            return columns;
        }
        public DataTable GetOS2Data(string column, string value)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // প্রতিটা ColumnName এর সর্বশেষ setting
            con.OpenDataSetThroughAdapter(
                "select ColumnName,Operator,OperatorValue from (" +
                "select ColumnName,Operator,OperatorValue, " +
                "ROW_NUMBER() over(partition by ColumnName order by StartDate desc) rn " +
                "from operationstatusSetting where DataName='OS2.1' and StartDate <= '" + now + "'" +
                ") x where rn=1", out dsMaster, false, "1");

            string masterFilter = BuildMasterFilter(dsMaster);
            string strSql = @" SELECT A.* FROM (
select
isnull(WC.GroupingData, 'Internal') AS GroupingData, WC.Id as WC_Id,WC.Code, WC.UserName as WorkCenter,p1.ProductionDate AS TargetDate,pp.UserName PlanningProcess, po.Id AS ProductionOrderID,
p1.Quantity AS PlannedQtyForTheDay,t1.SPT,t1.ProductionPriority,pm.UserName AS Product,
pc.UserName AS ProductCategory,mm.UserName AS Material,ord.Article,
BuyerRefNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
   trn.MasterOrder XMOI
   INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId = XMOI.Id
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = moi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



OwnRefNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
   trn.MasterOrder XMOI
   INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId = XMOI.Id
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = moi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



StyleNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = XMOI.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),

OwnStyleNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = XMOI.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



SONo = STUFF((select distinct ',' + sox.Id from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



SODesc = STUFF((select distinct ',' + sox.[Description] from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = T1.ProductionOrderId for xml path('') ), 1, 1, ''),



buyer = STUFF((select distinct ',' + XB.UserName from
    trn.SalesOrder XSO
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
left outer join[HKP].Buyer XB on XB.Id = XMO.BuyerId
where T1.ProductionOrderId = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),




Customer = STUFF((select distinct ',' + XP.UserName from
    trn.SalesOrder XSO
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
left outer join[HKP].[Party] Xp on XP.Id = XMO.PartyId
where T1.ProductionOrderId = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),

StyleGroup = STUFF((select distinct ',' + xmoi.ProductionGrouping from
    trn.SalesOrder XSO
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

where PO.Id = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),

MasterOrderNo = STUFF((select distinct ',' + XMO.MasterOrderNo from
    trn.SalesOrder XSO
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
where PO.Id = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),


ord.FOB,ord.CM,CEILING(t1.TargetPerDay) AS LineTargetPerDay, ord.OrderQty,ord.PlannedQty AS PlanOrderQty,prodWC.ProductionQtyAtWC,prodpr.ProductionQtyAtPR,u.UserName AS Unit,
ACCI.EmployeeName AS AccountIncharge,acch.EmployeeName AS AccountHolder,ord.FirstDeliveryDate,ord.LastDeliveryDate,ord.ProductionCompletionDate,
DATEADD(DD, ISNULL(PRDTIME.LastDayOfProduction, 0) * -1, ord.LastDeliveryDate) AS BaseProcessCompletionDate,
    ps.UserName AS ProductionStatus,ORD.Currency,ORD.UOM,E.UserName AS Entity,PLN.UserName AS Plant,



ord.CM* p1.Quantity AS BookedCM,t1.NoOfWorkStation,t1.LSD,t1.CommitmentDate,t1.MainRawMaterialInhouseDate,
t1.OtherRawMaterialInhouseDate,
--ord.OrderStatus,
PRODWC.LineStartDateAtWC,PRODWC.LineEndDateAtWC,
PRODPR.LineStartDateAtPR,PRODPR.LineEndDateAtPR,

p3.PlanningStartDateAtWC,p3.PlanningEndDateAtWC,p3.PlanningQtyAtWC,
p2.PlanningStartDateAtPR,p2.PlanningEndDateAtPR, p2.PlanningQtyAtPR,




--targetdate,earlyStartDate,
p1.ProductionHours AS WorkingHours,t1.PlanWorkingHoursPerDay,
t1.TargetPerDay / t1.PlanWorkingHoursPerDay AS PlanTargetPerHour,
  ppc.WorkingHours AS CalendarWorkingHours,p1.isBuildUp,
WC.StandardTimePerDay AS StandardWorkingHours, eff.StandardWorkingHourCost,eff.AdditionalWorkingHourCostPerHour,
wc.NoOfWorkStation AS StandardWorkStations,wc.DailyFixedCost,wc.VariableCost AS VariableCostPerHour
,bul.Id BulletinId, bul.BulletinTemplateId, bul.MachineSPT, bul.NonMachineSPT,
bul.MachineManpower, bul.NonMachineManpower,bul.TotalSPT,wcm.UserName FirstProcessWC, P.UserName FirstProcess


from trn.ProductionOrder PO
inner
join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID = po.Id
INNER
join ProductionPlanningType1 p1 on p1.ProductionOrderID = t1.ProductionOrderID and ProcessID = (select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = po.Id)



LEFT JOIN ProductionPlanningCalendar AS ppc ON ppc.EntityID = po.EntityId AND ppc.ProcessID = p1.ProcessID AND ppc.WorkingDate = p1.ProductionDate

left outer join scs.WorkCenterMaster WC on wc.id = p1.WorkCenterMasterId
LEFT OUTER JOIN EmployeeInformation AS ACCI ON ACCI.SystemId = wc.AccountInCharge
LEFT OUTER JOIN EmployeeInformation AS ACCH ON ACCH.SystemId = wc.AccountHolder



LEFT OUTER JOIN(SELECT MAX(days) AS LastDayOfProduction, pops.ProductionOrderId
FROM trn.ProductionOrderProcessSet AS pops WHERE pops.Symbol = '+'
GROUP BY pops.ProductionOrderId) AS PRDTIME ON prdtime.ProductionOrderId = po.Id



left outer join mst.MaterialMaster mm on mm.id = p1.MaterialMasterId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId = mm.Id
left outer join[MST].[ProductMaster] PM on pm.id = pd.ProductMasterId
left outer join[HKP].[ProductCategory] PC on pc.Id = pm.ProductCategoryId
LEFT JOIN[TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId = PM.Id AND EFF.EfficencyName = 'Costing'


left outer join org.Entity E on e.Id = p1.EntityID
LEFT OUTER JOIN org.Unit AS u ON u.Id = e.UnitId
left outer join org.Plant PLN on pln.Id = PO.PlantId
LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId

--Bulletin Data

                        LEFT JOIN(SELECT T.Id, T.BulletinTemplateId, T.ProductionOrderId, TM.ProcessId, SUM(D.TotalSPT) AS TotalSPT,
                            SUM(CASE WHEN ISNULL(D.MachineVarientId,'')<> '' THEN D.TotalSPT ELSE 0 END) AS MachineSPT,
                             SUM(CASE WHEN ISNULL(D.MachineVarientId, '') = '' THEN D.TotalSPT ELSE 0 END) AS NonMachineSPT,
                                SUM(CASE WHEN ISNULL(D.MachineVarientId, '') <> '' THEN D.AllotedManpower ELSE 0 END) AS MachineManpower,
                                   SUM(CASE WHEN ISNULL(D.MachineVarientId, '') = '' THEN D.AllotedManpower ELSE 0 END) AS NonMachineManpower

                            FROM trn.ProductionBulletinTemplate T

                            JOIN trn.ProductionBulletinTemplateMaster AS  TM ON tm.ProductionBulletinTemplateId = t.Id

                            JOIN trn.ProductionBulletinTemplateDetail AS D ON d.ProductionBulletinTemplateMasterId = TM.Id

                                       group by T.Id, T.BulletinTemplateId, T.ProductionOrderId,TM.ProcessId
						) BUL ON bul.ProductionOrderId = po.Id AND p1.ProcessID = BUL.ProcessId

  --planning at PR Level
left outer join(
SELECT P1.ProductionOrderID, P1.ProcessID,
MIN(ProductionDate) AS PlanningStartDateAtPR, SUM(Quantity) AS PlanningQtyAtPR, max(ProductionDate) AS PlanningEndDateAtPR
FROM ProductionPlanningType1 p1
group by P1.ProductionOrderID, P1.ProcessID
) as p2
on p2.ProductionOrderID = t1.ProductionOrderID
and p2.ProcessID = (select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = p1.ProductionOrderID)
left join hkp.process PP on pp.id = p2.ProcessID
--planning at WC Level
left outer join(
SELECT P1.ProductionOrderID, P1.ProcessID, p1.WorkCenterMasterId,
MIN(ProductionDate) AS PlanningStartDateAtWC, SUM(Quantity)AS PlanningQtyAtWC, max(ProductionDate) AS PlanningEndDateAtWC FROM ProductionPlanningType1 p1
group by P1.ProductionOrderID, P1.ProcessID, p1.WorkCenterMasterId
) as p3
on p1.WorkCenterMasterId = p3.WorkCenterMasterId
AND p3.ProductionOrderID = t1.ProductionOrderID
and p3.ProcessID = (select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = p1.ProductionOrderID)





--production at WC Level
LEFT OUTER JOIN(
SELECT s.EntityId, s.WorkCenterMasterId, s.ProductionOrderId, s.ProcessId, SUM(s.Quantity) AS ProductionQtyAtWC, MIN(s.ProductionDate) AS LineStartDateAtWC, MAX(s.ProductionDate) AS LineENDDateAtWC
FROM trn.ProductionSummary S
GROUP BY s.ProductionOrderId, s.WorkCenterMasterId, s.EntityId, s.ProcessId
) AS PRODWC ON p1.EntityID = PRODWC.EntityId AND PRODWC.WorkCenterMasterId = p1.WorkCenterMasterId AND p1.ProcessID = PRODWC.ProcessId AND PRODWC.ProductionOrderId = p1.ProductionOrderID



      --production at PR Level
LEFT OUTER JOIN(
SELECT s.ProductionOrderId, s.ProcessId, SUM(s.Quantity) AS ProductionQtyAtPR, MIN(s.ProductionDate) AS LineStartDateAtPR, MAX(s.ProductionDate) AS LineEndDateAtPR
FROM trn.ProductionSummary S
GROUP BY s.ProductionOrderId, s.ProcessId
) AS PRODPR ON PRODPR.ProductionOrderId = p1.ProductionOrderID AND p1.ProcessID = PRODPR.ProcessId
LEFT JOIN[dbo].[ProductionOrderFirstProcessWorkCenter] FP ON FP.ProductionOrderId = p1.ProductionOrderID
                            LEFT JOIN SCS.WorkCenterMaster wcm ON wcm.Id = FP.WorkCenterMasterId
                            LEFT JOIN HKP.Process P ON P.Id = FP.ProcessId
left outer join(
select POD.ProductionOrderId, ma.StandardName AS Article,
SUM(CEILING((isnull(SO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0))))) AS PlannedQty,
--SUM((so.Qty * (1 + (moi.ExtraOrderPercentage / 100))) * (1 + (moi.OrderWastagePercentage / 100))) AS PlannedQty,
min(so.DeliveryDate) AS FirstDeliveryDate, cur.Code AS Currency, uom.UserName AS UOM,
max(so.DeliveryDate) AS LastDeliveryDate,
MAX(so.CommitmentDate) AS ProductionCompletionDate,
SUM(so.Rate * so.Qty) / SUM(so.Qty) AS FOB, SUM(so.CM * so.Qty) / SUM(so.Qty) AS CM,
sum(SO.Qty) AS OrderQty from trn.ProductionOrderDetail POD
left outer join trn.SalesOrder SO on so.id = pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
LEFT OUTER JOIN[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
left outer join trn.MasterOrder MO on mo.Id = moi.MasterOrderId
left outer join[HKP].[Party] p on P.Id = MO.plantID
left outer join[HKP].[PartyPlant] PPI on ppi.id = mo.InvoicingPartyPlantId
left outer join[HKP].[PartyPlant] PPD on ppd.id = mo.DeliveryPartyPlantId
left outer join[HKP].[Buyer] B on b.id = mo.BuyerId
left outer join[HKP].[BuyerBrand] BB on bb.id = mo.BuyerBrandId
left outer join[HKP].[BuyerDivision] BD on bd.id = mo.BuyerBrandId
left outer join[HKP].[OrderCategory] OC on oc.id = mo.OrderCategoryId
--left outer join[HKP].[OrderStatus] OS on OS.id = mo.OrderStatusId
LEFT OUTER JOIN trn.Commitment AS c ON c.Id = mo.CommitmentId
left outer join mst.Destination DEST on dest.Id = so.DestinationId
left outer join[TRN].[CustomerPO] CPO ON CPO.Id = so.CustomerPOId
left outer join[MST].[ShipMode] SMO on SMO.Id = so.ShipmentModeId
left outer join hkp.Season S on s.id = mo.SeasonId
left outer join EmployeeInformation EI on ei.SystemId = MO.ResponsiblePersonId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id = MO.TotalQtyUOMId
LEFT OUTER JOIN scs.Currency AS cur ON cur.Id = mo.CurrencyId
group by POD.ProductionOrderId, ma.StandardName, cur.Code, uom.UserName--, MO.MasterOrderNo, b.UserName, os.UserName
) AS ORD on ord.ProductionOrderID = PO.Id
) A
 " + masterFilter + @" ";
            return _sqlRepository.GetDataTable(strSql);
        }


        public List<string> GetOS2SecondColumnList()
        {
            string strSql = @"select
isnull(WC.GroupingData, 'Internal') AS GroupingData,wc.Code, WC.Id as WC_Id, WC.UserName as WorkCenter,
WorkCenterGroup = STUFF((select distinct ',' + WCG.UserName
from HKP.WorkCenterGroup WCG
LEFT JOIN HKP.WorkCenterWiseGroup WG ON WG.WorkCenterGroupId = WCG.Id
where WG.WorkCenterMasterId = WC.Id for xml path('') ), 1, 1, ''),
p1.ProductionDate AS TargetDate,pp.UserName PlanningProcess, po.Id AS ProductionOrderID,t1.ID SubPOId,
 p1.Quantity AS PlannedQtyForTheDay,t1.SPT,t1.ProductionPriority,pm.UserName AS Product,
pc.UserName AS ProductCategory,mm.UserName AS Material,ord.Article,
BuyerRefNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
   trn.MasterOrder XMOI
   INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId = XMOI.Id
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = moi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
OwnRefNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
   trn.MasterOrder XMOI
   INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId = XMOI.Id
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = moi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
StyleNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = XMOI.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
OwnStyleNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = XMOI.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
SONo = STUFF((select distinct ',' + sox.Id from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
LineItemReference = STUFF((select distinct ',' + sox.LineItemReference from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
SODesc = STUFF((select distinct ',' + sox.[Description] from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
buyer = STUFF((select distinct ',' + XB.UserName from
    trn.SalesOrder XSO
    JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
left outer join[HKP].Buyer XB on XB.Id = XMO.BuyerId
where t1.ProductionOrderId = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),
Customer = STUFF((select distinct ',' + XP.UserName from
    trn.SalesOrder XSO
    JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
left outer join[HKP].[Party] Xp on XP.Id = XMO.PartyId
where t1.ProductionOrderId = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),
StyleGroup = STUFF((select distinct ',' + xmoi.ProductionGrouping from
    trn.SalesOrder XSO
    JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
where PO.Id = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),
MasterOrderNo = STUFF((select distinct ',' + XMO.MasterOrderNo from
    trn.SalesOrder XSO
    JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
where PO.Id = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),
ord.FOB,ord.CM,CEILING(t1.TargetPerDay) AS LineTargetPerDay, ord.OrderQty,ord.PlannedQty AS PlanOrderQty,prodWC.ProductionQtyAtWC,prodpr.ProductionQtyAtPR,u.UserName AS Unit,
ACCI.EmployeeName AS AccountIncharge,acch.EmployeeName AS AccountHolder,ord.FirstDeliveryDate,ord.LastDeliveryDate,ord.ProductionCompletionDate,
DATEADD(DD, ISNULL(PRDTIME.LastDayOfProduction, 0) * -1, ord.LastDeliveryDate) AS BaseProcessCompletionDate,
    ps.UserName AS ProductionStatus,ORD.Currency,ORD.UOM,E.UserName AS Entity,PLN.UserName AS Plant,
ord.CM* p1.Quantity AS BookedCM,t1.NoOfWorkStation,t1.LSD,t1.CommitmentDate,t1.MainRawMaterialInhouseDate,
t1.OtherRawMaterialInhouseDate,
--ord.OrderStatus,
PRODWC.LineStartDateAtWC,PRODWC.LineEndDateAtWC,
PRODPR.LineStartDateAtPR,PRODPR.LineEndDateAtPR,
p3.PlanningStartDateAtWC,p3.PlanningEndDateAtWC,p3.PlanningQtyAtWC,
p2.PlanningStartDateAtPR,p2.PlanningEndDateAtPR, p2.PlanningQtyAtPR,
--targetdate,earlyStartDate,
p1.ProductionHours AS WorkingHours,t1.PlanWorkingHoursPerDay,
t1.TargetPerDay / t1.PlanWorkingHoursPerDay AS PlanTargetPerHour,
  ppc.WorkingHours AS CalendarWorkingHours,p1.isBuildUp,
WC.StandardTimePerDay AS StandardWorkingHours, eff.StandardWorkingHourCost,eff.AdditionalWorkingHourCostPerHour,
wc.NoOfWorkStation AS StandardWorkStations,wc.DailyFixedCost,wc.VariableCost AS VariableCostPerHour
,bul.Id BulletinId, bul.MachineSPT, bul.NonMachineSPT,
bul.MachineManpower, bul.NonMachineManpower,bul.TotalSPT,wcm.UserName FirstProcessWC, P.UserName FirstProcess, PS1.UserName SubPOStatus, t1.ProductionPriority SubPOPriority
   , FCV.UserName SKUColor, SCV.UserName SKUSize
from ProductionOrderSchedulingParametersType2 T1
inner
join trn.ProductionOrder PO on t1.ProductionOrderID = po.Id AND PO.OrderType = 'PlanningType2'
INNER join ProductionPlanningType2 p1 on p1.ProductionOrderID = t1.ID and ProcessID = (select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = po.Id)
LEFT JOIN ProductionPlanningType2Calendar AS ppc ON ppc.EntityID = po.EntityId AND ppc.ProcessID = p1.ProcessID AND ppc.WorkingDate = p1.ProductionDate
LEFT JOIN HKP.CharacteristicsValue FCV ON FCV.Id = T1.SKU1Id
LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id = T1.SKU2Id


left outer join scs.WorkCenterMaster WC on wc.id = p1.WorkCenterMasterId
LEFT OUTER JOIN EmployeeInformation AS ACCI ON ACCI.SystemId = wc.AccountInCharge
LEFT OUTER JOIN EmployeeInformation AS ACCH ON ACCH.SystemId = wc.AccountHolder
LEFT OUTER JOIN(SELECT MAX(days) AS LastDayOfProduction, pops.ProductionOrderId
FROM trn.ProductionOrderProcessSet AS pops WHERE pops.Symbol = '+'
GROUP BY pops.ProductionOrderId) AS PRDTIME ON prdtime.ProductionOrderId = po.Id
left outer join mst.MaterialMaster mm on mm.id = p1.MaterialMasterId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId = mm.Id
left outer join[MST].[ProductMaster] PM on pm.id = pd.ProductMasterId
left outer join[HKP].[ProductCategory] PC on pc.Id = pm.ProductCategoryId
LEFT JOIN[TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId = PM.Id AND EFF.EfficencyName = 'Costing'
left outer join org.Entity E on e.Id = p1.EntityID
LEFT OUTER JOIN org.Unit AS u ON u.Id = e.UnitId
left outer join org.Plant PLN on pln.Id = PO.PlantId
LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
LEFT OUTER JOIN hkp.ProductionStatus AS ps1 ON ps1.Id = t1.ProductionStatusId
--Bulletin Data

                        LEFT JOIN(SELECT T.Id, T.ProductionOrderId, TM.ProcessId, SUM(D.TotalSPT) AS TotalSPT,
                            SUM(CASE WHEN ISNULL(D.MachineVarientId,'')<> '' THEN D.TotalSPT ELSE 0 END) AS MachineSPT,
                             SUM(CASE WHEN ISNULL(D.MachineVarientId, '') = '' THEN D.TotalSPT ELSE 0 END) AS NonMachineSPT,
                                SUM(CASE WHEN ISNULL(D.MachineVarientId, '') <> '' THEN D.AllotedManpower ELSE 0 END) AS MachineManpower,
                                   SUM(CASE WHEN ISNULL(D.MachineVarientId, '') = '' THEN D.AllotedManpower ELSE 0 END) AS NonMachineManpower

                            FROM trn.ProductionBulletinTemplate T

                            JOIN trn.ProductionBulletinTemplateMaster AS  TM ON tm.ProductionBulletinTemplateId = t.Id

                            JOIN trn.ProductionBulletinTemplateDetail AS D ON d.ProductionBulletinTemplateMasterId = TM.Id

                                       group by T.Id, T.ProductionOrderId,TM.ProcessId
						) BUL ON bul.ProductionOrderId = po.Id AND p1.ProcessID = BUL.ProcessId
  --planning at PR Level
left outer join(
SELECT P1.ProductionOrderID, P1.ProcessID,
MIN(ProductionDate) AS PlanningStartDateAtPR, SUM(Quantity) AS PlanningQtyAtPR, max(ProductionDate) AS PlanningEndDateAtPR
FROM ProductionPlanningType2 p1
group by P1.ProductionOrderID, P1.ProcessID
) as p2
on p2.ProductionOrderID = t1.ID
and p2.ProcessID = (select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = po.Id)
left join hkp.process PP on pp.id = p2.ProcessID
--planning at WC Level
left outer join(
SELECT P1.ProductionOrderID, P1.ProcessID, p1.WorkCenterMasterId,
MIN(ProductionDate) AS PlanningStartDateAtWC, SUM(Quantity)AS PlanningQtyAtWC, max(ProductionDate) AS PlanningEndDateAtWC FROM ProductionPlanningType2 p1
group by P1.ProductionOrderID, P1.ProcessID, p1.WorkCenterMasterId
) as p3
on p1.WorkCenterMasterId = p3.WorkCenterMasterId
AND p3.ProductionOrderID = t1.ID
and p3.ProcessID = (select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = po.Id)
--production at WC Level
LEFT OUTER JOIN(
SELECT s.EntityId, s.WorkCenterMasterId, s.ProductionOrderId, s.ProcessId, SUM(s.Quantity) AS ProductionQtyAtWC, MIN(s.ProductionDate) AS LineStartDateAtWC, MAX(s.ProductionDate) AS LineENDDateAtWC
FROM trn.ProductionSummary S
GROUP BY s.ProductionOrderId, s.WorkCenterMasterId, s.EntityId, s.ProcessId
) AS PRODWC ON p1.EntityID = PRODWC.EntityId AND PRODWC.WorkCenterMasterId = p1.WorkCenterMasterId AND p1.ProcessID = PRODWC.ProcessId AND PRODWC.ProductionOrderId = po.ID

      --production at PR Level
LEFT OUTER JOIN(
SELECT s.ProductionOrderId, s.ProcessId, SUM(s.Quantity) AS ProductionQtyAtPR, MIN(s.ProductionDate) AS LineStartDateAtPR, MAX(s.ProductionDate) AS LineEndDateAtPR
FROM trn.ProductionSummary S
GROUP BY s.ProductionOrderId, s.ProcessId
) AS PRODPR ON PRODPR.ProductionOrderId = po.ID AND p1.ProcessID = PRODPR.ProcessId
LEFT JOIN[dbo].[ProductionOrderFirstProcessWorkCenter] FP ON FP.ProductionOrderId = p1.ProductionOrderID

                            LEFT JOIN SCS.WorkCenterMaster wcm ON wcm.Id = FP.WorkCenterMasterId

                            LEFT JOIN HKP.Process P ON P.Id = FP.ProcessId
left outer join(
select POD.ProductionOrderId, ma.StandardName AS Article,
SUM(CEILING((isnull(SO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0))))) AS PlannedQty,
--SUM((so.Qty * (1 + (moi.ExtraOrderPercentage / 100))) * (1 + (moi.OrderWastagePercentage / 100))) AS PlannedQty,
min(so.DeliveryDate) AS FirstDeliveryDate, cur.Code AS Currency, uom.UserName AS UOM,
max(so.DeliveryDate) AS LastDeliveryDate,
MAX(so.CommitmentDate) AS ProductionCompletionDate,
SUM(so.Rate * so.Qty) / SUM(so.Qty) AS FOB, SUM(so.CM * so.Qty) / SUM(so.Qty) AS CM,
sum(SO.Qty) AS OrderQty from TRN.ProductionOrderDetail POD
left outer join trn.SalesOrder SO on so.id = pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
LEFT OUTER JOIN[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
left outer join trn.MasterOrder MO on mo.Id = moi.MasterOrderId
left outer join[HKP].[Party] p on P.Id = MO.plantID
left outer join[HKP].[PartyPlant] PPI on ppi.id = mo.InvoicingPartyPlantId
left outer join[HKP].[PartyPlant] PPD on ppd.id = mo.DeliveryPartyPlantId
left outer join[HKP].[Buyer] B on b.id = mo.BuyerId
left outer join[HKP].[BuyerBrand] BB on bb.id = mo.BuyerBrandId
left outer join[HKP].[BuyerDivision] BD on bd.id = mo.BuyerBrandId
left outer join[HKP].[OrderCategory] OC on oc.id = mo.OrderCategoryId
--left outer join[HKP].[OrderStatus] OS on OS.id = mo.OrderStatusId
LEFT OUTER JOIN trn.Commitment AS c ON c.Id = mo.CommitmentId
left outer join mst.Destination DEST on dest.Id = so.DestinationId
left outer join[TRN].[CustomerPO] CPO ON CPO.Id = so.CustomerPOId
left outer join[MST].[ShipMode] SMO on SMO.Id = so.ShipmentModeId
left outer join hkp.Season S on s.id = mo.SeasonId
left outer join EmployeeInformation EI on ei.SystemId = MO.ResponsiblePersonId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id = MO.TotalQtyUOMId
LEFT OUTER JOIN scs.Currency AS cur ON cur.Id = mo.CurrencyId
group by POD.ProductionOrderId, ma.StandardName, cur.Code, uom.UserName--, MO.MasterOrderNo, b.UserName, os.UserName
) AS ORD on ord.ProductionOrderID = PO.Id

WHERE 1=2";

            DataTable dt = _sqlRepository.GetDataTable(strSql);

            var columns = new List<string>();
            foreach (DataColumn c in dt.Columns)
            {
                // UserName ও Value pivot এর জন্য ব্যবহৃত, ড্রপডাউনে দরকার নেই
                if (c.ColumnName == "UserName" || c.ColumnName == "Value") continue;
                columns.Add(c.ColumnName);
            }
            return columns;
        }
        public DataTable GetOS2SecondData(string column, string value)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // প্রতিটা ColumnName এর সর্বশেষ setting
            con.OpenDataSetThroughAdapter(
                "select ColumnName,Operator,OperatorValue from (" +
                "select ColumnName,Operator,OperatorValue, " +
                "ROW_NUMBER() over(partition by ColumnName order by StartDate desc) rn " +
                "from operationstatusSetting where DataName='OS2.2' and StartDate <= '" + now + "'" +
                ") x where rn=1", out dsMaster, false, "1");

            string masterFilter = BuildMasterFilterBlank(dsMaster);

            string strSql = @"select isnull(WC.GroupingData, 'Internal') AS GroupingData,wc.Code, WC.Id as WC_Id, WC.UserName as WorkCenter,
WorkCenterGroup = STUFF((select distinct ',' + WCG.UserName
from HKP.WorkCenterGroup WCG
LEFT JOIN HKP.WorkCenterWiseGroup WG ON WG.WorkCenterGroupId = WCG.Id
where WG.WorkCenterMasterId = WC.Id for xml path('') ), 1, 1, ''),
p1.ProductionDate AS TargetDate,pp.UserName PlanningProcess, po.Id AS ProductionOrderID,t1.ID SubPOId,
 p1.Quantity AS PlannedQtyForTheDay,t1.SPT,t1.ProductionPriority,pm.UserName AS Product,
pc.UserName AS ProductCategory,mm.UserName AS Material,ord.Article,
BuyerRefNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
   trn.MasterOrder XMOI
   INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId = XMOI.Id
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = moi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
OwnRefNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
   trn.MasterOrder XMOI
   INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId = XMOI.Id
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = moi.Id
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
StyleNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = XMOI.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
OwnStyleNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = XMOI.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
SONo = STUFF((select distinct ',' + sox.Id from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
LineItemReference = STUFF((select distinct ',' + sox.LineItemReference from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
SODesc = STUFF((select distinct ',' + sox.[Description] from
    trn.MasterOrderItem XMOI
    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId = xmoi.Id
INNER JOIN TRN.ProductionOrderDetail AS podx ON podx.SalesOrderId = sox.Id
where podx.ProductionOrderId = t1.ProductionOrderId for xml path('') ), 1, 1, ''),
buyer = STUFF((select distinct ',' + XB.UserName from
    trn.SalesOrder XSO
    JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
left outer join[HKP].Buyer XB on XB.Id = XMO.BuyerId
where t1.ProductionOrderId = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),
Customer = STUFF((select distinct ',' + XP.UserName from
    trn.SalesOrder XSO
    JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
left outer join[HKP].[Party] Xp on XP.Id = XMO.PartyId
where t1.ProductionOrderId = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),
StyleGroup = STUFF((select distinct ',' + xmoi.ProductionGrouping from
    trn.SalesOrder XSO
    JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
where PO.Id = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),
MasterOrderNo = STUFF((select distinct ',' + XMO.MasterOrderNo from
    trn.SalesOrder XSO
    JOIN TRN.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
where PO.Id = Xpod.ProductionOrderId for xml path('') ), 1, 1, ''),
ord.FOB,ord.CM,CEILING(t1.TargetPerDay) AS LineTargetPerDay, ord.OrderQty,ord.PlannedQty AS PlanOrderQty,prodWC.ProductionQtyAtWC,prodpr.ProductionQtyAtPR,u.UserName AS Unit,
ACCI.EmployeeName AS AccountIncharge,acch.EmployeeName AS AccountHolder,ord.FirstDeliveryDate,ord.LastDeliveryDate,ord.ProductionCompletionDate,
DATEADD(DD, ISNULL(PRDTIME.LastDayOfProduction, 0) * -1, ord.LastDeliveryDate) AS BaseProcessCompletionDate,
    ps.UserName AS ProductionStatus,ORD.Currency,ORD.UOM,E.UserName AS Entity,PLN.UserName AS Plant,
ord.CM* p1.Quantity AS BookedCM,t1.NoOfWorkStation,t1.LSD,t1.CommitmentDate,t1.MainRawMaterialInhouseDate,
t1.OtherRawMaterialInhouseDate,
--ord.OrderStatus,
PRODWC.LineStartDateAtWC,PRODWC.LineEndDateAtWC,
PRODPR.LineStartDateAtPR,PRODPR.LineEndDateAtPR,
p3.PlanningStartDateAtWC,p3.PlanningEndDateAtWC,p3.PlanningQtyAtWC,
p2.PlanningStartDateAtPR,p2.PlanningEndDateAtPR, p2.PlanningQtyAtPR,
--targetdate,earlyStartDate,
p1.ProductionHours AS WorkingHours,t1.PlanWorkingHoursPerDay,
t1.TargetPerDay / t1.PlanWorkingHoursPerDay AS PlanTargetPerHour,
  ppc.WorkingHours AS CalendarWorkingHours,p1.isBuildUp,
WC.StandardTimePerDay AS StandardWorkingHours, eff.StandardWorkingHourCost,eff.AdditionalWorkingHourCostPerHour,
wc.NoOfWorkStation AS StandardWorkStations,wc.DailyFixedCost,wc.VariableCost AS VariableCostPerHour
,bul.Id BulletinId, bul.MachineSPT, bul.NonMachineSPT,
bul.MachineManpower, bul.NonMachineManpower,bul.TotalSPT,wcm.UserName FirstProcessWC, P.UserName FirstProcess, PS1.UserName SubPOStatus, t1.ProductionPriority SubPOPriority
   , FCV.UserName SKUColor, SCV.UserName SKUSize
from ProductionOrderSchedulingParametersType2 T1
inner
join trn.ProductionOrder PO on t1.ProductionOrderID = po.Id AND PO.OrderType = 'PlanningType2'
INNER join ProductionPlanningType2 p1 on p1.ProductionOrderID = t1.ID and ProcessID = (select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = po.Id)
LEFT JOIN ProductionPlanningType2Calendar AS ppc ON ppc.EntityID = po.EntityId AND ppc.ProcessID = p1.ProcessID AND ppc.WorkingDate = p1.ProductionDate
LEFT JOIN HKP.CharacteristicsValue FCV ON FCV.Id = T1.SKU1Id
LEFT JOIN HKP.CharacteristicsValue SCV ON SCV.Id = T1.SKU2Id


left outer join scs.WorkCenterMaster WC on wc.id = p1.WorkCenterMasterId
LEFT OUTER JOIN EmployeeInformation AS ACCI ON ACCI.SystemId = wc.AccountInCharge
LEFT OUTER JOIN EmployeeInformation AS ACCH ON ACCH.SystemId = wc.AccountHolder
LEFT OUTER JOIN(SELECT MAX(days) AS LastDayOfProduction, pops.ProductionOrderId
FROM trn.ProductionOrderProcessSet AS pops WHERE pops.Symbol = '+'
GROUP BY pops.ProductionOrderId) AS PRDTIME ON prdtime.ProductionOrderId = po.Id
left outer join mst.MaterialMaster mm on mm.id = p1.MaterialMasterId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId = mm.Id
left outer join[MST].[ProductMaster] PM on pm.id = pd.ProductMasterId
left outer join[HKP].[ProductCategory] PC on pc.Id = pm.ProductCategoryId
LEFT JOIN[TRN].[ProductMasterEfficency] EFF ON eff.ProductMasterId = PM.Id AND EFF.EfficencyName = 'Costing'
left outer join org.Entity E on e.Id = p1.EntityID
LEFT OUTER JOIN org.Unit AS u ON u.Id = e.UnitId
left outer join org.Plant PLN on pln.Id = PO.PlantId
LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id = po.ProductionStatusId
LEFT OUTER JOIN hkp.ProductionStatus AS ps1 ON ps1.Id = t1.ProductionStatusId
--Bulletin Data

                        LEFT JOIN(SELECT T.Id, T.ProductionOrderId, TM.ProcessId, SUM(D.TotalSPT) AS TotalSPT,
                            SUM(CASE WHEN ISNULL(D.MachineVarientId,'')<> '' THEN D.TotalSPT ELSE 0 END) AS MachineSPT,
                             SUM(CASE WHEN ISNULL(D.MachineVarientId, '') = '' THEN D.TotalSPT ELSE 0 END) AS NonMachineSPT,
                                SUM(CASE WHEN ISNULL(D.MachineVarientId, '') <> '' THEN D.AllotedManpower ELSE 0 END) AS MachineManpower,
                                   SUM(CASE WHEN ISNULL(D.MachineVarientId, '') = '' THEN D.AllotedManpower ELSE 0 END) AS NonMachineManpower

                            FROM trn.ProductionBulletinTemplate T

                            JOIN trn.ProductionBulletinTemplateMaster AS  TM ON tm.ProductionBulletinTemplateId = t.Id

                            JOIN trn.ProductionBulletinTemplateDetail AS D ON d.ProductionBulletinTemplateMasterId = TM.Id

                                       group by T.Id, T.ProductionOrderId,TM.ProcessId
						) BUL ON bul.ProductionOrderId = po.Id AND p1.ProcessID = BUL.ProcessId
  --planning at PR Level
left outer join(
SELECT P1.ProductionOrderID, P1.ProcessID,
MIN(ProductionDate) AS PlanningStartDateAtPR, SUM(Quantity) AS PlanningQtyAtPR, max(ProductionDate) AS PlanningEndDateAtPR
FROM ProductionPlanningType2 p1
group by P1.ProductionOrderID, P1.ProcessID
) as p2
on p2.ProductionOrderID = t1.ID
and p2.ProcessID = (select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = po.Id)
left join hkp.process PP on pp.id = p2.ProcessID
--planning at WC Level
left outer join(
SELECT P1.ProductionOrderID, P1.ProcessID, p1.WorkCenterMasterId,
MIN(ProductionDate) AS PlanningStartDateAtWC, SUM(Quantity)AS PlanningQtyAtWC, max(ProductionDate) AS PlanningEndDateAtWC FROM ProductionPlanningType2 p1
group by P1.ProductionOrderID, P1.ProcessID, p1.WorkCenterMasterId
) as p3
on p1.WorkCenterMasterId = p3.WorkCenterMasterId
AND p3.ProductionOrderID = t1.ID
and p3.ProcessID = (select top 1 ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess = 1 and ProductionOrderID = po.Id)
--production at WC Level
LEFT OUTER JOIN(
SELECT s.EntityId, s.WorkCenterMasterId, s.ProductionOrderId, s.ProcessId, SUM(s.Quantity) AS ProductionQtyAtWC, MIN(s.ProductionDate) AS LineStartDateAtWC, MAX(s.ProductionDate) AS LineENDDateAtWC
FROM trn.ProductionSummary S
GROUP BY s.ProductionOrderId, s.WorkCenterMasterId, s.EntityId, s.ProcessId
) AS PRODWC ON p1.EntityID = PRODWC.EntityId AND PRODWC.WorkCenterMasterId = p1.WorkCenterMasterId AND p1.ProcessID = PRODWC.ProcessId AND PRODWC.ProductionOrderId = po.ID

      --production at PR Level
LEFT OUTER JOIN(
SELECT s.ProductionOrderId, s.ProcessId, SUM(s.Quantity) AS ProductionQtyAtPR, MIN(s.ProductionDate) AS LineStartDateAtPR, MAX(s.ProductionDate) AS LineEndDateAtPR
FROM trn.ProductionSummary S
GROUP BY s.ProductionOrderId, s.ProcessId
) AS PRODPR ON PRODPR.ProductionOrderId = po.ID AND p1.ProcessID = PRODPR.ProcessId
LEFT JOIN[dbo].[ProductionOrderFirstProcessWorkCenter] FP ON FP.ProductionOrderId = p1.ProductionOrderID

                            LEFT JOIN SCS.WorkCenterMaster wcm ON wcm.Id = FP.WorkCenterMasterId

                            LEFT JOIN HKP.Process P ON P.Id = FP.ProcessId
left outer join(
select POD.ProductionOrderId, ma.StandardName AS Article,
SUM(CEILING((isnull(SO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0))))) AS PlannedQty,
--SUM((so.Qty * (1 + (moi.ExtraOrderPercentage / 100))) * (1 + (moi.OrderWastagePercentage / 100))) AS PlannedQty,
min(so.DeliveryDate) AS FirstDeliveryDate, cur.Code AS Currency, uom.UserName AS UOM,
max(so.DeliveryDate) AS LastDeliveryDate,
MAX(so.CommitmentDate) AS ProductionCompletionDate,
SUM(so.Rate * so.Qty) / SUM(so.Qty) AS FOB, SUM(so.CM * so.Qty) / SUM(so.Qty) AS CM,
sum(SO.Qty) AS OrderQty from TRN.ProductionOrderDetail POD
left outer join trn.SalesOrder SO on so.id = pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
LEFT OUTER JOIN[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
left outer join trn.MasterOrder MO on mo.Id = moi.MasterOrderId
left outer join[HKP].[Party] p on P.Id = MO.plantID
left outer join[HKP].[PartyPlant] PPI on ppi.id = mo.InvoicingPartyPlantId
left outer join[HKP].[PartyPlant] PPD on ppd.id = mo.DeliveryPartyPlantId
left outer join[HKP].[Buyer] B on b.id = mo.BuyerId
left outer join[HKP].[BuyerBrand] BB on bb.id = mo.BuyerBrandId
left outer join[HKP].[BuyerDivision] BD on bd.id = mo.BuyerBrandId
left outer join[HKP].[OrderCategory] OC on oc.id = mo.OrderCategoryId
--left outer join[HKP].[OrderStatus] OS on OS.id = mo.OrderStatusId
LEFT OUTER JOIN trn.Commitment AS c ON c.Id = mo.CommitmentId
left outer join mst.Destination DEST on dest.Id = so.DestinationId
left outer join[TRN].[CustomerPO] CPO ON CPO.Id = so.CustomerPOId
left outer join[MST].[ShipMode] SMO on SMO.Id = so.ShipmentModeId
left outer join hkp.Season S on s.id = mo.SeasonId
left outer join EmployeeInformation EI on ei.SystemId = MO.ResponsiblePersonId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id = MO.TotalQtyUOMId
LEFT OUTER JOIN scs.Currency AS cur ON cur.Id = mo.CurrencyId
group by POD.ProductionOrderId, ma.StandardName, cur.Code, uom.UserName--, MO.MasterOrderNo, b.UserName, os.UserName
) AS ORD on ord.ProductionOrderID = PO.Id
--order by wc.Code,p1.ProductionDate

 " + masterFilter + @" ";

            return _sqlRepository.GetDataTable(strSql);

        }


        public List<string> GetProductionColumnList()
        {
            string strSql = @"SELECT distinct PP.Id, trkp.UserName AS Plant, trke.UserName AS Entity, pp.EntityID, pp.WorkCenterMasterId, PP.ProductionOrderID, wcm.UserName AS WorkCenter, FORMAT(PP.ProductionDate, 'dd-MMM-yyyy') AS ActualDate, pp.Quantity AS ActualQty
                                        , RP.EmployeeName ResponsiblePerson, MN.EmployeeName Mentor, AH.EmployeeName AccountHolder, AC.EmployeeName AccountInCharge
                                        , (select SUM(xp.Qty * xp.CM) / sum(xp.Qty)
                                        from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id
                                        where PO.Id = PoD.ProductionOrderId) * pp.Quantity AS ActualCM
                                            , pt1.SPT AS SAM,pp.ProcessId,isnull(p.UserName, FSFG.UserName) AS Process
                                              , pops.[Sequence] ProcessSequence
							, BaseProcessFlag = case when pops.IsBaseProcess = 0 then 'No' else 'Yes' end
							,isnull(Tp.UserName, TSFG.UserName) AS ToProcess, Twcm.UserName AS ToWorkCenter
							,Material = STUFF((select distinct ',' + MA.UserName from
                                                MST.MaterialMaster MA
                                                left
                                                                                 join TRN.MasterOrderItem moi on moi.MaterialMasterId = MA.Id
                                
                                                                            left
                                                                                 join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                                
                                                                            INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                            where PO.Id = PoD.ProductionOrderId for xml path('') ), 1, 1, '')
							,Article = STUFF((select distinct ',' + MA.StandardName from
                                                MST.MaterialMasterArticle MA
    
                                                left
                                                                                    join TRN.MasterOrderItem moi on moi.ArticleId = MA.Id
                                   
                                                                               left
                                                                                    join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                                   
                                                                               INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                            where PO.Id = PoD.ProductionOrderId for xml path('') ), 1, 1, '')
                          ,Product = STUFF((select distinct ',' + PM.UserName from
                                                MST.MaterialMaster mm
    
                                                left
                                                                              join TRN.MasterOrderItem moi on moi.MaterialMasterId = mm.Id
                             
                                                                         left
                                                                              join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                             
                                                                         left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId = mm.Id
                                            left outer join[MST].[ProductMaster] PM on pm.id = pd.ProductMasterId

                                            INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                            where PO.Id = PoD.ProductionOrderId for xml path('') ), 1, 1, '')						   
								,ProductCategory = STUFF((select distinct ',' + pc.UserName from
                                [HKP].[ProductCategory] PC

                                left join[MST].[ProductMaster] PM on pc.Id = pm.ProductCategoryId

                                left join trn.ProductDefinition AS pd ON pd.ProductMasterId = pm.Id

                                left join mst.MaterialMaster mm on mm.id = pd.MaterialMasterId

                                left join TRN.MasterOrderItem moi on moi.MaterialMasterId = MM.Id

                                left join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id

                                INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                where PO.Id = PoD.ProductionOrderId for xml path('') ), 1, 1, '')
							,Format(SN.AddedDate, 'dd-MMM-yyyy') AS SnapshotDate,
                             sn.Quantity AS PlanQty
							,(select SUM(xp.Qty * xp.CM) / sum(xp.Qty)

                            from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id
                            where PO.Id = PoD.ProductionOrderId)*sn.Quantity AS PlanCM

                              , CM = (select SUM(xp.Qty * xp.CM) / sum(xp.Qty)
                            from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id
                            where PO.Id = PoD.ProductionOrderId)

                             ,CPL.UserName AS ProductionShift,so.Id AS SalesOrderIdBooking,CPL.ShiftDuration ShiftWorkingMin, so.[Description] AS SalesOrderDescBooking,
                              wcm.StandardTimePerDay AS StandardWorkingHours,  wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
                             PP.ProductionHours AS WorkingHours,SN.isBuildUp,
                             pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                            --additional info
                                 buyer = STUFF((select distinct ',' + XB.UserName from
                                            trn.SalesOrder XSO
    
                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                        left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId

                                        left outer join[HKP].Buyer XB on XB.Id = XMO.BuyerId

                                        where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,LineItemId = STUFF((select distinct ', ' + moi.Id from
                                                TRN.MasterOrderItem moi
    
                                                left
                                                                               join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                              
                                                                          INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId = xp.id

                                            where pp.ProductionOrderID = PD.ProductionOrderId for xml path('') ), 1, 1, '')
							
							
							,ProductGroup = STUFF((select distinct ', ' + moi.ProductionGrouping from
                                                TRN.MasterOrderItem moi
    
                                                left
                                                                                                 join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                                                
                                                                                            INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId = xp.id

                                            where pp.ProductionOrderID = PD.ProductionOrderId for xml path('') ), 1, 1, '')

							,ProductName = STUFF((select distinct ',' + MA.UserName from
                                                [dbo].[ProductLibrary] MA

                                                left join TRN.MasterOrderItem moi on moi.ProductLibraryId = MA.Id

                                                left join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id

                                                INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                                where pp.ProductionOrderID = PoD.ProductionOrderId for xml path('') ), 1, 1, '')

							,ProductCode = STUFF((select distinct ',' + MA.Code from
                                                [dbo].[ProductLibrary] MA

                                                left join TRN.MasterOrderItem moi on moi.ProductLibraryId = MA.Id

                                                left join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id

                                                INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                                where pp.ProductionOrderID = PoD.ProductionOrderId for xml path('') ), 1, 1, '')
                            ,SalesOrderIds = STUFF((select distinct ', ' + XSO.Id from
                                        trn.SalesOrder XSO
    
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                    where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

	                        SalesOrderDesc = STUFF((select distinct ',' + XSO.Description from
                                        trn.SalesOrder XSO
    
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                    where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                   
                                  MasterOrderNo = STUFF((select distinct ',' + XMO.Id from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                        BuyerOrderNo = STUFF((select distinct ',' + XMO.BuyerReferenceNo from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     OwnOrderNo = STUFF((select distinct ',' + XMO.OwnReferenceNo from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

			
		                                StyleNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
                                        OwnStyleNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            pt1.NoOfWorkStation,ISNULL(PPC.WorkingHours, 0) AS PlanHours
                             , ISNULL(WCS.[ProductionHours], 0) StandardProductionHours
                            ,ISNULL(ppt.ProductionHours, 0) ProductionHours,
                            ISNULL(sn.Quantity, 0) * isnull(pt1.SPT, 0) AS PlanMinutes,
                                ISNULL(sn.Quantity, 0)*isnull(pt1.SPT, 0) / (pt1.NoOfWorkStation * sn.ProductionHours * 60) AS PlanEfficiency,

                                        ISNULL(pp.Quantity, 0)*isnull(pt1.SPT, 0) AS ActualMinutes,
                                          ISNULL(pp.Quantity, 0)*isnull(pt1.SPT, 0) / (pt1.NoOfWorkStation * pp.ProductionHours * 60) AS ActualEfficiency
                                                  , isnull(MMT.[Minute], 0) DetentionInMin,0 Utilization,pp.ProductionOrderId PORefNo, pp.ProductionLineId,pp.ProductionSONo,pp.LotNumber,pp.AddedBy EntryBy, UOM.Code UOM, pp.Quantity ProductionQty, pp.Remarks
							,pp.QtyWithoutScan,pp.ScanQty,PB.POBulletinId,PB.TotalSPT,PB.AllotedWorkstation,PB.AllotedManpower,PB.AdditionalWorkstation,PB.AdditionalManpower,PB.MCSPT

                            FROM(SELECT ps.Id, ps.ProcessId, mm.UserName, ma.StandardName, ps.FromSFGInventoryId, ps.ToProcessId, ps.ToSFGInventoryId, ps.EntityId, ps.ProductionShiftId
                            , ps.ProductionOrderId, ps.ProductionDate, ps.WorkCenterMasterId, ps.ToWorkCenterMasterId, COUNT(*) AS ProductionHours, ps.Quantity, PS.ScanQty, PS.QtyWithoutScan
                            , ps.MasterOrderItemId ProductionLineId, PS.SalesOrderId ProductionSONo, PS.LotNumber
                                    , PS.AddedBy, ps.Remarks, ps.ProductLibraryId
                               FROM trn.ProductionSummary AS ps
                                  left outer join mst.MaterialMaster mm on mm.id = ps.MaterialMasterId
                                  LEFT OUTER JOIN[MST].[MaterialMasterArticle] MA ON ma.Id = ps.ArticleId

                                    --WHERE ps.ProductionDate BETWEEN @FromDate AND @ToDate
                                  GROUP BY  ps.Id, ps.ProcessId, mm.UserName, ma.StandardName, ps.FromSFGInventoryId, ps.ToProcessId, ps.ToSFGInventoryId, ps.EntityId, ps.SalesOrderId
                                  , ps.ProductionShiftId, ps.ProductionOrderId, ps.ProductionDate, ps.WorkCenterMasterId, ps.ToWorkCenterMasterId, PS.AddedBy, ps.Remarks, ps.ProductLibraryId
                                  , ps.Quantity, PS.ScanQty, PS.QtyWithoutScan, ps.MasterOrderItemId, PS.SalesOrderId, PS.LotNumber
                            ) AS pp

                            left join(Select SUM(isnull([Minute],0))[Minute], ProcessId,ShiftId,WorkCenterId,[Date] from MachineMasterTransaction Group By ProcessId,ShiftId,WorkCenterId,[Date]) MMT on MMT.ProcessId = pp.ProcessId and MMT.ShiftId = pp.ProductionShiftId and MMT.WorkCenterId = pp.WorkCenterMasterId and MMT.[Date]= pp.ProductionDate
                            LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId = pp.ProductionShiftId

                            LEFT JOIN[dbo].[ProductionPlanningCalendar] PPC ON PPC.EntityId = PP.EntityId AND PPC.ProcessId = PP.ProcessId AND PPC.WorkingDate = pp.ProductionDate

                            LEFT JOIN[dbo].[WorkCenterWiseShift] WCS ON WCS.WorkCenterMasterId = pp.WorkCenterMasterId AND WCS.ShiftDefinationID = pp.ProductionShiftId

                            LEFT JOIN trn.SalesOrder AS so ON so.Id = pp.ProductionSONo

                            left join trn.MasterOrderItem MOI on MOI.Id = so.MasterOrderItemId

                            left join SCS.UnitOfMeasurement UOM on UOM.Id = MOI.UOMId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID = pp.ProductionOrderID
                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID = pp.ProductionOrderId AND sn.ProductionDate = pp.ProductionDate AND sn.WorkCenterMasterId = pp.WorkCenterMasterId AND sn.EntityID = pp.EntityId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id = pp.WorkCenterMasterId


                            LEFT JOIN dbo.EmployeeInformation RP ON RP.SystemId = wcm.ResponsiblePersonId

                            LEFT JOIN dbo.EmployeeInformation MN ON MN.SystemId = wcm.MentorId

                            LEFT JOIN dbo.EmployeeInformation AH ON AH.SystemId = wcm.AccountHolder

                            LEFT JOIN dbo.EmployeeInformation AC ON AC.SystemId = wcm.AccountInCharge


                            LEFT JOIN(
                            Select P.Id POBulletinId, P.ProductionOrderId, SUM(D.TotalSPT) TotalSPT, SUM(D.AllotedWorkstation) AllotedWorkstation, SUM(D.AllotedManpower) AllotedManpower
                            , SUM(D.AdditionalWorkstation) AdditionalWorkstation, SUM(D.AdditionalManpower) AdditionalManpower
                            , MCSPT= PC.MCtotalspt, M.ProcessId
                            from TRN.ProductionBulletinTemplate P

                            LEFT JOIN [TRN].[ProductionBulletinTemplateMaster] M ON M.ProductionBulletinTemplateId= P.Id

                            LEFT JOIN [TRN].[ProductionBulletinTemplateDetail] D ON D.ProductionBulletinTemplateMasterId= M.Id

                            LEFT JOIN [dbo].[ProducitonBulletinCalculation] PC ON PC.ProductionBulletinTemplateMasterId= M.Id

                            Where P.Id= 2510

                            Group BY P.Id, P.ProductionOrderId, PC.MCtotalspt, M.ProcessId

                            ) PB ON PB.ProductionOrderId = PP.ProductionOrderId AND PB.ProcessId = PP.ProcessId

                            LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id = pp.FromSFGInventoryId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id = pp.ToWorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id = pp.ToSFGInventoryId
                            left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID = pp.ProductionOrderId AND ppt.WorkCenterMasterId = PP.WorkCenterMasterId AND ppt.ProcessId = PP.ProcessId AND ppt.EntityId = pp.EntityId and ppt.ProductionDate = PP.ProductionDate
                            left outer join TRN.ProductionOrder PO ON PO.Id = PP.ProductionOrderID
                            LEFT OUTER JOIN hkp.Process AS p ON p.Id = pp.ProcessId
                            LEFT OUTER JOIN hkp.Process AS Tp ON Tp.Id = pp.ToProcessId
                            left join trn.productionorderprocessset pops on pops.ProductionOrderId = po.Id AND P.Id = POPS.ProcessId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
             

WHERE 1=2";

            DataTable dt = _sqlRepository.GetDataTable(strSql);

            var columns = new List<string>();
            foreach (DataColumn c in dt.Columns)
            {
                // UserName ও Value pivot এর জন্য ব্যবহৃত, ড্রপডাউনে দরকার নেই
                if (c.ColumnName == "UserName" || c.ColumnName == "Value") continue;
                columns.Add(c.ColumnName);
            }
            return columns;
        }

        public DataTable GetProductionData(string column, string value)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // প্রতিটা ColumnName এর সর্বশেষ setting
            con.OpenDataSetThroughAdapter(
                "select ColumnName,Operator,OperatorValue from (" +
                "select ColumnName,Operator,OperatorValue, " +
                "ROW_NUMBER() over(partition by ColumnName order by StartDate desc) rn " +
                "from operationstatusSetting where DataName='Production' and StartDate <= '" + now + "'" +
                ") x where rn=1", out dsMaster, false, "1");

            string masterFilter = BuildMasterFilter(dsMaster);
            string strSql = @"  SELECT A.*from(SELECT distinct PP.Id, trkp.UserName AS Plant, trke.UserName AS Entity, pp.EntityID, pp.WorkCenterMasterId, PP.ProductionOrderID, wcm.UserName AS WorkCenter, FORMAT(PP.ProductionDate, 'dd-MMM-yyyy') AS ActualDate, pp.Quantity AS ActualQty
                                        , RP.EmployeeName ResponsiblePerson, MN.EmployeeName Mentor, AH.EmployeeName AccountHolder, AC.EmployeeName AccountInCharge
                                        , (select SUM(xp.Qty * xp.CM) / sum(xp.Qty)
                                        from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id
                                        where PO.Id = PoD.ProductionOrderId) * pp.Quantity AS ActualCM
                                            , pt1.SPT AS SAM,pp.ProcessId,isnull(p.UserName, FSFG.UserName) AS Process
                                              , pops.[Sequence] ProcessSequence
							, BaseProcessFlag = case when pops.IsBaseProcess = 0 then 'No' else 'Yes' end
							,isnull(Tp.UserName, TSFG.UserName) AS ToProcess, Twcm.UserName AS ToWorkCenter
							,Material = STUFF((select distinct ',' + MA.UserName from
                                                MST.MaterialMaster MA
                                                left
                                                                                 join TRN.MasterOrderItem moi on moi.MaterialMasterId = MA.Id
                                
                                                                            left
                                                                                 join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                                
                                                                            INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                            where PO.Id = PoD.ProductionOrderId for xml path('') ), 1, 1, '')
							,Article = STUFF((select distinct ',' + MA.StandardName from
                                                MST.MaterialMasterArticle MA
    
                                                left
                                                                                    join TRN.MasterOrderItem moi on moi.ArticleId = MA.Id
                                   
                                                                               left
                                                                                    join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                                   
                                                                               INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                            where PO.Id = PoD.ProductionOrderId for xml path('') ), 1, 1, '')
                          ,Product = STUFF((select distinct ',' + PM.UserName from
                                                MST.MaterialMaster mm
    
                                                left
                                                                              join TRN.MasterOrderItem moi on moi.MaterialMasterId = mm.Id
                             
                                                                         left
                                                                              join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                             
                                                                         left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId = mm.Id
                                            left outer join[MST].[ProductMaster] PM on pm.id = pd.ProductMasterId

                                            INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                            where PO.Id = PoD.ProductionOrderId for xml path('') ), 1, 1, '')						   
								,ProductCategory = STUFF((select distinct ',' + pc.UserName from
                                [HKP].[ProductCategory] PC

                                left join[MST].[ProductMaster] PM on pc.Id = pm.ProductCategoryId

                                left join trn.ProductDefinition AS pd ON pd.ProductMasterId = pm.Id

                                left join mst.MaterialMaster mm on mm.id = pd.MaterialMasterId

                                left join TRN.MasterOrderItem moi on moi.MaterialMasterId = MM.Id

                                left join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id

                                INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                where PO.Id = PoD.ProductionOrderId for xml path('') ), 1, 1, '')
							,Format(SN.AddedDate, 'dd-MMM-yyyy') AS SnapshotDate,
                             sn.Quantity AS PlanQty
							,(select SUM(xp.Qty * xp.CM) / sum(xp.Qty)

                            from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id
                            where PO.Id = PoD.ProductionOrderId)*sn.Quantity AS PlanCM

                              , CM = (select SUM(xp.Qty * xp.CM) / sum(xp.Qty)
                            from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id
                            where PO.Id = PoD.ProductionOrderId)

                             ,CPL.UserName AS ProductionShift,so.Id AS SalesOrderIdBooking,CPL.ShiftDuration ShiftWorkingMin, so.[Description] AS SalesOrderDescBooking,
                              wcm.StandardTimePerDay AS StandardWorkingHours,  wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
                             PP.ProductionHours AS WorkingHours,SN.isBuildUp,
                             pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                            --additional info
                                 buyer = STUFF((select distinct ',' + XB.UserName from
                                            trn.SalesOrder XSO
    
                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                        left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId

                                        left outer join[HKP].Buyer XB on XB.Id = XMO.BuyerId

                                        where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,LineItemId = STUFF((select distinct ', ' + moi.Id from
                                                TRN.MasterOrderItem moi
    
                                                left
                                                                               join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                              
                                                                          INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId = xp.id

                                            where pp.ProductionOrderID = PD.ProductionOrderId for xml path('') ), 1, 1, '')
							
							
							,ProductGroup = STUFF((select distinct ', ' + moi.ProductionGrouping from
                                                TRN.MasterOrderItem moi
    
                                                left
                                                                                                 join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id
                                                
                                                                                            INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId = xp.id

                                            where pp.ProductionOrderID = PD.ProductionOrderId for xml path('') ), 1, 1, '')

							,ProductName = STUFF((select distinct ',' + MA.UserName from
                                                [dbo].[ProductLibrary] MA

                                                left join TRN.MasterOrderItem moi on moi.ProductLibraryId = MA.Id

                                                left join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id

                                                INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                                where pp.ProductionOrderID = PoD.ProductionOrderId for xml path('') ), 1, 1, '')

							,ProductCode = STUFF((select distinct ',' + MA.Code from
                                                [dbo].[ProductLibrary] MA

                                                left join TRN.MasterOrderItem moi on moi.ProductLibraryId = MA.Id

                                                left join trn.SalesOrder AS xp on xp.MasterOrderItemId = moi.Id

                                                INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId = xp.id

                                                where pp.ProductionOrderID = PoD.ProductionOrderId for xml path('') ), 1, 1, '')
                            ,SalesOrderIds = STUFF((select distinct ', ' + XSO.Id from
                                        trn.SalesOrder XSO
    
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                    where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

	                        SalesOrderDesc = STUFF((select distinct ',' + XSO.Description from
                                        trn.SalesOrder XSO
    
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                    where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                   
                                  MasterOrderNo = STUFF((select distinct ',' + XMO.Id from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                        BuyerOrderNo = STUFF((select distinct ',' + XMO.BuyerReferenceNo from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     OwnOrderNo = STUFF((select distinct ',' + XMO.OwnReferenceNo from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

			
		                                StyleNo = STUFF((select distinct ',' + XMOI.BuyerReferenceNo from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
                                        OwnStyleNo = STUFF((select distinct ',' + XMOI.OwnReferenceNo from
                                                trn.SalesOrder XSO
    
                                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id

                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId

                                            where pp.ProductionOrderID = Xpod.ProductionOrderId   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            pt1.NoOfWorkStation,ISNULL(PPC.WorkingHours, 0) AS PlanHours
                             , ISNULL(WCS.[ProductionHours], 0) StandardProductionHours
                            ,ISNULL(ppt.ProductionHours, 0) ProductionHours,
                            ISNULL(sn.Quantity, 0) * isnull(pt1.SPT, 0) AS PlanMinutes,
                                ISNULL(sn.Quantity, 0)*isnull(pt1.SPT, 0) / (pt1.NoOfWorkStation * sn.ProductionHours * 60) AS PlanEfficiency,

                                        ISNULL(pp.Quantity, 0)*isnull(pt1.SPT, 0) AS ActualMinutes,
                                          ISNULL(pp.Quantity, 0)*isnull(pt1.SPT, 0) / (pt1.NoOfWorkStation * pp.ProductionHours * 60) AS ActualEfficiency
                                                  , isnull(MMT.[Minute], 0) DetentionInMin,0 Utilization,pp.ProductionOrderId PORefNo, pp.ProductionLineId,pp.ProductionSONo,pp.LotNumber,pp.AddedBy EntryBy, UOM.Code UOM, pp.Quantity ProductionQty, pp.Remarks
							,pp.QtyWithoutScan,pp.ScanQty,PB.POBulletinId,PB.TotalSPT,PB.AllotedWorkstation,PB.AllotedManpower,PB.AdditionalWorkstation,PB.AdditionalManpower,PB.MCSPT

                            FROM(SELECT ps.Id, ps.ProcessId, mm.UserName, ma.StandardName, ps.FromSFGInventoryId, ps.ToProcessId, ps.ToSFGInventoryId, ps.EntityId, ps.ProductionShiftId
                            , ps.ProductionOrderId, ps.ProductionDate, ps.WorkCenterMasterId, ps.ToWorkCenterMasterId, COUNT(*) AS ProductionHours, ps.Quantity, PS.ScanQty, PS.QtyWithoutScan
                            , ps.MasterOrderItemId ProductionLineId, PS.SalesOrderId ProductionSONo, PS.LotNumber
                                    , PS.AddedBy, ps.Remarks, ps.ProductLibraryId
                               FROM trn.ProductionSummary AS ps
                                  left outer join mst.MaterialMaster mm on mm.id = ps.MaterialMasterId
                                  LEFT OUTER JOIN[MST].[MaterialMasterArticle] MA ON ma.Id = ps.ArticleId

                                    WHERE ps.ProductionDate BETWEEN @FromDate AND @ToDate
                                  GROUP BY  ps.Id, ps.ProcessId, mm.UserName, ma.StandardName, ps.FromSFGInventoryId, ps.ToProcessId, ps.ToSFGInventoryId, ps.EntityId, ps.SalesOrderId
                                  , ps.ProductionShiftId, ps.ProductionOrderId, ps.ProductionDate, ps.WorkCenterMasterId, ps.ToWorkCenterMasterId, PS.AddedBy, ps.Remarks, ps.ProductLibraryId
                                  , ps.Quantity, PS.ScanQty, PS.QtyWithoutScan, ps.MasterOrderItemId, PS.SalesOrderId, PS.LotNumber
                            ) AS pp

                            left join(Select SUM(isnull([Minute],0))[Minute], ProcessId,ShiftId,WorkCenterId,[Date] from MachineMasterTransaction Group By ProcessId,ShiftId,WorkCenterId,[Date]) MMT on MMT.ProcessId = pp.ProcessId and MMT.ShiftId = pp.ProductionShiftId and MMT.WorkCenterId = pp.WorkCenterMasterId and MMT.[Date]= pp.ProductionDate
                            LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId = pp.ProductionShiftId
                            LEFT JOIN[dbo].[ProductionPlanningCalendar] PPC ON PPC.EntityId = PP.EntityId AND PPC.ProcessId = PP.ProcessId AND PPC.WorkingDate = pp.ProductionDate
                            LEFT JOIN[dbo].[WorkCenterWiseShift] WCS ON WCS.WorkCenterMasterId = pp.WorkCenterMasterId AND WCS.ShiftDefinationID = pp.ProductionShiftId
                            LEFT JOIN trn.SalesOrder AS so ON so.Id = pp.ProductionSONo
                            left join trn.MasterOrderItem MOI on MOI.Id = so.MasterOrderItemId
                            left join SCS.UnitOfMeasurement UOM on UOM.Id = MOI.UOMId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID = pp.ProductionOrderID
                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID = pp.ProductionOrderId AND sn.ProductionDate = pp.ProductionDate AND sn.WorkCenterMasterId = pp.WorkCenterMasterId AND sn.EntityID = pp.EntityId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id = pp.WorkCenterMasterId
                            LEFT JOIN dbo.EmployeeInformation RP ON RP.SystemId = wcm.ResponsiblePersonId
                            LEFT JOIN dbo.EmployeeInformation MN ON MN.SystemId = wcm.MentorId
                            LEFT JOIN dbo.EmployeeInformation AH ON AH.SystemId = wcm.AccountHolder

                            LEFT JOIN dbo.EmployeeInformation AC ON AC.SystemId = wcm.AccountInCharge


                            LEFT JOIN(
                            Select P.Id POBulletinId, P.ProductionOrderId, SUM(D.TotalSPT) TotalSPT, SUM(D.AllotedWorkstation) AllotedWorkstation, SUM(D.AllotedManpower) AllotedManpower
                            , SUM(D.AdditionalWorkstation) AdditionalWorkstation, SUM(D.AdditionalManpower) AdditionalManpower
                            , MCSPT= PC.MCtotalspt, M.ProcessId
                            from TRN.ProductionBulletinTemplate P

                            LEFT JOIN [TRN].[ProductionBulletinTemplateMaster] M ON M.ProductionBulletinTemplateId= P.Id

                            LEFT JOIN [TRN].[ProductionBulletinTemplateDetail] D ON D.ProductionBulletinTemplateMasterId= M.Id

                            LEFT JOIN [dbo].[ProducitonBulletinCalculation] PC ON PC.ProductionBulletinTemplateMasterId= M.Id

                            Where P.Id= 2510

                            Group BY P.Id, P.ProductionOrderId, PC.MCtotalspt, M.ProcessId

                            ) PB ON PB.ProductionOrderId = PP.ProductionOrderId AND PB.ProcessId = PP.ProcessId

                            LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id = pp.FromSFGInventoryId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id = pp.ToWorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id = pp.ToSFGInventoryId
                            left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID = pp.ProductionOrderId AND ppt.WorkCenterMasterId = PP.WorkCenterMasterId AND ppt.ProcessId = PP.ProcessId AND ppt.EntityId = pp.EntityId and ppt.ProductionDate = PP.ProductionDate
                            left outer join TRN.ProductionOrder PO ON PO.Id = PP.ProductionOrderID
                            LEFT OUTER JOIN hkp.Process AS p ON p.Id = pp.ProcessId
                            LEFT OUTER JOIN hkp.Process AS Tp ON Tp.Id = pp.ToProcessId
                            left join trn.productionorderprocessset pops on pops.ProductionOrderId = po.Id AND P.Id = POPS.ProcessId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
             )A  " + masterFilter + @" ";

            return _sqlRepository.GetDataTable(strSql);

        }

        public List<string> GetWorkCenterColumnList()
        {
            string strSql = @"select WCM.Id WCMId, WCM.CompanyId,WCM.PlantId,
WCM.EntityId,WCM.WorkCenterCategoryId,WCM.WorkCenterSubcategoryId,WCM.ProcessId,WCM.Code WCMCode, WCM.UserName LineName,
 WCM.Description WCMDescription, WCM.CapacityProcessUoMId, WCM.UoMId, WCM.LineId,WCM.Capacity,
WCM.PlanEfficiency, WCM.MaxTimePerDay, WCM.StandardTimePerDay, WCM.PlanBudgetCapacityPerDay,
WCM.DailyFixedCost, WCM.VariableCost, WCM.CurrencyId, WCM.AddedBy, WCM.AddedDate, WCM.AddedFromIP,
WCM.UpdatedBy, WCM.UpdatedDate, WCM.UpdatedFromIP, WCM.SPT, WCM.CM,
WCM.NoOfWorkStation, WCM.MonthlyNoOfDays, WCM.ResponsiblePersonId, WCM.MentorId,WCM.BuyerId,
WCM.Sequence, WCM.AccountHolder, WCM.AccountInCharge, WCM.GroupingData, WCM.Active WCMActive,
WCM.OperationBulletinId, WCM.IsBusinessProcess, 
L.UserName AS OrgLine,WCG.UserName AS WorkCenterGroup, 
B.UserName Buyer, AH.EmployeeName AccountHolderName
 , ProductName = isnull(STUFF((select distinct ',' + PM.UserName
 from[SCS].[WorkCenterMasterProductPriority] A
 LEFT JOIN MST.ProductMaster PM ON PM.Id = A.ProductMasterId
 where A.WorkCenterMasterId = WCM.Id   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,NoOfShift = (Select COUNT(ShiftDefinationID) from[dbo].[WorkCenterWiseShift] Where WorkCenterMasterId = WCM.Id Group by WorkCenterMasterId)
,ShiftName = isnull(STUFF((select distinct ',' + SD.UserName from[dbo].[WorkCenterWiseShift] A
LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID = A.ShiftDefinationID
where A.WorkCenterMasterId = WCM.Id   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,PS.UserName Process, P.UserName Plant, E.UserName Entity, WCM.Active
   from[SCS].[WorkCenterMaster] WCM
  LEFT JOIN HKP.Buyer B ON B.Id = WCM.BuyerId
LEFT JOIN[dbo].[EmployeeInformation] AH ON AH.SystemId = WCM.AccountHolder
LEFT JOIN HKP.Process PS ON PS.Id = WCM.Processid
LEFT JOIN ORG.Plant P ON P.id = WCM.PlantId
LEFT JOIN ORG.Entity E ON E.Id = WCM.EntityId
LEFT JOIN ORG.Line L ON L.Id = wcm.LineId
LEFT JOIN HKP.WorkCenterWiseGroup WG ON WG.WorkCenterMasterId = WCM.Id
LEFT JOIN HKP.WorkCenterGroup WCG ON WCG.Id = WG.WorkCenterGroupId

WHERE 1=2";

            DataTable dt = _sqlRepository.GetDataTable(strSql);

            var columns = new List<string>();
            foreach (DataColumn c in dt.Columns)
            {
                // UserName ও Value pivot এর জন্য ব্যবহৃত, ড্রপডাউনে দরকার নেই
                if (c.ColumnName == "UserName" || c.ColumnName == "Value") continue;
                columns.Add(c.ColumnName);
            }
            return columns;
        }

        public DataTable GetWorkCenterData(string column, string value)
        {
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // প্রতিটা ColumnName এর সর্বশেষ setting
            con.OpenDataSetThroughAdapter(
                "select ColumnName,Operator,OperatorValue from (" +
                "select ColumnName,Operator,OperatorValue, " +
                "ROW_NUMBER() over(partition by ColumnName order by StartDate desc) rn " +
                "from operationstatusSetting where DataName='WorkCenter' and StartDate <= '" + now + "'" +
                ") x where rn=1", out dsMaster, false, "1");

            string masterFilter = BuildMasterFilter(dsMaster);

            string sql = @"select WCM.Id WCMId, WCM.CompanyId,WCM.PlantId,
WCM.EntityId,WCM.WorkCenterCategoryId,WCM.WorkCenterSubcategoryId,WCM.ProcessId,WCM.Code WCMCode, WCM.UserName LineName,
 WCM.Description WCMDescription, WCM.CapacityProcessUoMId, WCM.UoMId, WCM.LineId,WCM.Capacity,
WCM.PlanEfficiency, WCM.MaxTimePerDay, WCM.StandardTimePerDay, WCM.PlanBudgetCapacityPerDay,
WCM.DailyFixedCost, WCM.VariableCost, WCM.CurrencyId, WCM.AddedBy, WCM.AddedDate, WCM.AddedFromIP,
WCM.UpdatedBy, WCM.UpdatedDate, WCM.UpdatedFromIP, WCM.SPT, WCM.CM,
WCM.NoOfWorkStation, WCM.MonthlyNoOfDays, WCM.ResponsiblePersonId, WCM.MentorId,WCM.BuyerId,
WCM.Sequence, WCM.AccountHolder, WCM.AccountInCharge, WCM.GroupingData, WCM.Active WCMActive,
WCM.OperationBulletinId, WCM.IsBusinessProcess, 
L.UserName AS OrgLine,WCG.UserName AS WorkCenterGroup, 
B.UserName Buyer, AH.EmployeeName AccountHolderName
 , ProductName = isnull(STUFF((select distinct ',' + PM.UserName
 from[SCS].[WorkCenterMasterProductPriority] A
 LEFT JOIN MST.ProductMaster PM ON PM.Id = A.ProductMasterId
 where A.WorkCenterMasterId = WCM.Id   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,NoOfShift = (Select COUNT(ShiftDefinationID) from[dbo].[WorkCenterWiseShift] Where WorkCenterMasterId = WCM.Id Group by WorkCenterMasterId)
,ShiftName = isnull(STUFF((select distinct ',' + SD.UserName from[dbo].[WorkCenterWiseShift] A
LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID = A.ShiftDefinationID
where A.WorkCenterMasterId = WCM.Id   for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,PS.UserName Process, P.UserName Plant, E.UserName Entity, WCM.Active
   from[SCS].[WorkCenterMaster] WCM
  LEFT JOIN HKP.Buyer B ON B.Id = WCM.BuyerId
LEFT JOIN[dbo].[EmployeeInformation] AH ON AH.SystemId = WCM.AccountHolder
LEFT JOIN HKP.Process PS ON PS.Id = WCM.Processid
LEFT JOIN ORG.Plant P ON P.id = WCM.PlantId
LEFT JOIN ORG.Entity E ON E.Id = WCM.EntityId
LEFT JOIN ORG.Line L ON L.Id = wcm.LineId
LEFT JOIN HKP.WorkCenterWiseGroup WG ON WG.WorkCenterMasterId = WCM.Id
LEFT JOIN HKP.WorkCenterGroup WCG ON WCG.Id = WG.WorkCenterGroupId
 " + masterFilter + @" ";

            return _sqlRepository.GetDataTable(sql);

        }
        private static string SqlEsc(object value)
        {
            return Convert.ToString(value).Replace("'", "''");
        }
    }
    public class EmployeeOperation
    {
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string EmpSystemId { get; set; }
        public string OperationMasterId { get; set; }
        public string OperationVariationId { get; set; }
        public string OperationVariation { get; set; }

        public decimal CycleTime { get; set; } = 0;
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string OperationMaster { get; set; }
        public string OperationActivity { get; set; }
        public string OperationType { get; set; }
        public string OperationCategory { get; set; }
        public string Skill { get; set; }
        public string SkillGroup { get; set; }
        public string LegalDesignation { get; set; }
        public string DesignationGroup { get; set; }
        public string MachineMaster { get; set; }
        public string Code { get; set; }
        public string Process { get; set; }
        public string Archive { get; set; }
        public string Operation { get; set; }
        public string SKillMaster { get; set; }
        public string SpecialSkill { get; set; }
        public string EmployeeRating { get; set; }
    }


}

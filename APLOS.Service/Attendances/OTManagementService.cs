#region Using

using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.UI.WebControls;

#endregion Using

namespace Library.Service.Attendances
{
    public class OTManagementService : Service<AccessControllerEmployeeTag>, IOTManagementService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IAccessControllerDeleteRequestService _d;

        public OTManagementService(
            IRepositoryAsync<AccessControllerEmployeeTag> PreRecruitmentEmpReferenceRepository
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IAccessControllerDeleteRequestService d
            , IEmployeeInformationService employeeInformationService) :
            base(PreRecruitmentEmpReferenceRepository, unitOfWork, pkGeneratorService)
        {
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _d = d;
            _employeeInformationService = employeeInformationService;
        }

        #endregion Constructor




        //un-Confirmed Employee Data For Grid
        public IEnumerable<object> LoadEmpForOTConfirmation(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons)
        {
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            clsAttnManualOverTime objManlAttn;
            objManlAttn = new clsAttnManualOverTime();
            objStatic = new clsStaticInfo();

            try
            {
                #region Variable


                string sFixShift = string.Empty;


                #endregion Variable

                #region Validation

                if (string.IsNullOrEmpty(PlantId) == true)
                {

                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (ProcDate == "" || bplib.clsWebLib.IsDateOK(ProcDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                objStatic.GetPlantWiseHRMSSetting(CompanyGroupID, PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                    IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                    if (IsPreallocationBasedOT)
                    {
                        IsPunchBasedOT = false;
                    }

                }




                #endregion Validation

                return GetEmpForOTConfirmation(CompanyGroupID, PlantId, ProcDate, sOTValCons, IsPunchBasedOT, IsPreallocationBasedOT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function
        public IEnumerable<object> GetEmpForOTConfirmation(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT * FROM ( SELECT [CheckBoxSelect] = Convert(bit, 'False'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ
                                , ES.ShiftSystemID, ES.ShiftName
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftInTime) , '00:00'),'hh:mm tt')  ShiftInTime
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftOutTime) , '00:00'),'hh:mm tt')  ShiftOutTime

                                    , De.UserName DepartmentName
                                    , EC.UserName EmpCategoryName
                                    --, Dsg.UserName DesignationName
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation 


                                    , ISNULL(Atd.IsManualInTime, 0) IsManualInTime
                                    , FORMAT(ISNULL(Convert(datetime, Atd.InTime) , '00:00'),'hh:mm tt') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, FORMAT(ISNULL(Convert(datetime, Atd.OutTime) , '00:00'),'hh:mm tt')  OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";








                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";


                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            NormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                NormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            NormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            NormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                , Atd.IsOTEntitled IsOTEntitle
                                , ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                                , ExtraOT=CASE WHEN ISNULL(Atd.OTHr,0)/60 > pl.firstSlab THEN 'YES' ELSE 'NO' END
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 

	                                LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 

                                    LEFT OUTER JOIN ORG.Line eL on eL.id=PMB.LineId
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND MaternityStatus IS NULL 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND MaternityStatus IS NULL 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))  AND MaternityStatus IS NULL 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)

                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND MaternityStatus IS NULL 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
													






                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                               (
                                                  SELECT EDSA.EmpSystemID
                                                    ,EDSA.ShiftSystemID
                                                    ,ESA.IsFix
                                                    ,ESA.FixSystemID
                                                    ,ESA.IsRoster
                                                    ,ESA.RosterSystemID
                                                    ---,EDSA.DayType
                                                    ,DayType=dt.OriginalDayType
                                                    ,S.ShiftDefinationName ShiftName
                                                    ,S.ShiftType
                                                    ,CONVERT(VARCHAR(5), S.InTime, 108) xShiftInTime
                                                    ,CONVERT(VARCHAR(5), S.OutTime, 108) xShiftOutTime
                                                    ---STC starts
                                                    , ShiftInTime = CASE
                                                    WHEN Sc.InTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.InTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.InTime , 108)
                                                    END

                                                    , ShiftOutTime = CASE
                                                    WHEN Sc.OutTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.OutTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.OutTime , 108)
                                                    END
                                                    ---STC ends
                                                    ,DATEADD(MI, - S.InTimeStartMargin, S.InTime) OfficeStartTime
                                                    ,DATEADD(MI, S.LateMargin, S.InTime) OfficeTime
                                                    ,S.InTimeStartMargin
                                                    ,S.BreakStratTime
                                                    ,S.BreakEndTime
                                                    ,DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime
                                                    ,OTStartTime = CASE 
                                                    WHEN S.IsGapInclude = 1
                                                    THEN S.OutTime
                                                    ELSE DATEADD(MI, S.OTStartTime, S.OutTime)
                                                    END
                                                    FROM dbo.EmpDateWiseShiftAssign EDSA
                                                    LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
                                                    LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
                                                    ---STC starts
                                                    LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + strAttnDate + @"' BETWEEN FromDate AND ToDate) AS sc ON EDSA.ShiftSystemID = sc.ShiftDefinationID
                                                    ---STC ends
                                                    LEFT JOIN AttdnProcessData ap on EDSA.EmpSystemID=ap.EmpSystemID and ap.WorkDate=EDSA.WorkDate
													Left JOIN DayType dt on dt.DayType=ap.DayStatus
		                                           WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                        AND EDSA.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                ) ES ON E.SystemID = ES.EmpSystemID";



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }


                strSql = strSql + @"  LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = ES.DayType AND Atd.WorkDate BETWEEN pl.FromDate AND pl.ToDate and pl.PlantID=e.PlantId";


                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x WHERE x.SystemID NOT IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )    ORDER BY X.EmployeeCode ";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Auto
        public DataSet LoadEmpForOTConfirmationAuto(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons)
        {
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            clsAttnManualOverTime objManlAttn;
            objManlAttn = new clsAttnManualOverTime();
            objStatic = new clsStaticInfo();

            try
            {
                #region Variable


                string sFixShift = string.Empty;


                #endregion Variable

                #region Validation

                if (string.IsNullOrEmpty(PlantId) == true)
                {

                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (ProcDate == "" || bplib.clsWebLib.IsDateOK(ProcDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                objStatic.GetPlantWiseHRMSSetting(CompanyGroupID, PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                    IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                    if (IsPreallocationBasedOT)
                    {
                        IsPunchBasedOT = false;
                    }

                }




                #endregion Validation

                return GetEmpForOTConfirmationAuto(CompanyGroupID, PlantId, ProcDate, sOTValCons, IsPunchBasedOT, IsPreallocationBasedOT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function
        public DataSet GetEmpForOTConfirmationAuto(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                strSql = @"SELECT * FROM ( SELECT [CheckBoxSelect] = Convert(bit, 'true'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ
                                , ES.ShiftSystemID, ES.ShiftName
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftInTime) , '00:00'),'hh:mm tt')  ShiftInTime
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftOutTime) , '00:00'),'hh:mm tt')  ShiftOutTime

                                    , De.UserName DepartmentName
                                    , EC.UserName EmpCategoryName
                                    --, Dsg.UserName DesignationName
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation 


                                    , ISNULL(Atd.IsManualInTime, 0) IsManualInTime
                                    , FORMAT(ISNULL(Convert(datetime, Atd.InTime) , '00:00'),'hh:mm tt') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, FORMAT(ISNULL(Convert(datetime, Atd.OutTime) , '00:00'),'hh:mm tt')  OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";








                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";


                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            NormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                NormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            NormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            NormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                , Atd.IsOTEntitled IsOTEntitle
                                , ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                                , ExtraOT=CASE WHEN ISNULL(Atd.OTHr,0)/60 > pl.firstSlab THEN 'YES' ELSE 'NO' END,ISNULL(Atd.OTHr, 0) OTHrInMin
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 							      

	                                LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 

                                    LEFT OUTER JOIN ORG.Line eL on eL.id=PMB.LineId
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND MaternityStatus IS NULL 
                                                   
													
													
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND MaternityStatus IS NULL
                                                    
													
													
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND MaternityStatus IS NULL 
                                                    
													
													
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND MaternityStatus IS NULL
                                                    
													
													
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                               (
                                                  SELECT EDSA.EmpSystemID
                                                    ,EDSA.ShiftSystemID
                                                    ,ESA.IsFix
                                                    ,ESA.FixSystemID
                                                    ,ESA.IsRoster
                                                    ,ESA.RosterSystemID
                                                    ---,EDSA.DayType
									                ,DayType=dt.OriginalDayType
                                                    ,S.ShiftDefinationName ShiftName
                                                    ,S.ShiftType
                                                    ,CONVERT(VARCHAR(5), S.InTime, 108) xShiftInTime
                                                    ,CONVERT(VARCHAR(5), S.OutTime, 108) xShiftOutTime
                                                    ---STC starts
                                                    , ShiftInTime = CASE
                                                    WHEN Sc.InTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.InTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.InTime , 108)
                                                    END

                                                    , ShiftOutTime = CASE
                                                    WHEN Sc.OutTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.OutTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.OutTime , 108)
                                                    END
                                                    ---STC ends
                                                    ,DATEADD(MI, - S.InTimeStartMargin, S.InTime) OfficeStartTime
                                                    ,DATEADD(MI, S.LateMargin, S.InTime) OfficeTime
                                                    ,S.InTimeStartMargin
                                                    ,S.BreakStratTime
                                                    ,S.BreakEndTime
                                                    ,DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime
                                                    ,OTStartTime = CASE 
                                                    WHEN S.IsGapInclude = 1
                                                    THEN S.OutTime
                                                    ELSE DATEADD(MI, S.OTStartTime, S.OutTime)
                                                    END
                                                    FROM dbo.EmpDateWiseShiftAssign EDSA
                                                    LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
                                                    LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
                                                    ---STC starts
                                                    LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + strAttnDate + @"' BETWEEN FromDate AND ToDate) AS sc ON EDSA.ShiftSystemID = sc.ShiftDefinationID
                                                    ---STC ends
                                                    LEFT JOIN AttdnProcessData ap on EDSA.EmpSystemID=ap.EmpSystemID and ap.WorkDate=EDSA.WorkDate
													Left JOIN DayType dt on dt.DayType=ap.DayStatus
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                        AND EDSA.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                ) ES ON E.SystemID = ES.EmpSystemID";



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }



                strSql = strSql + @"  LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = ES.DayType AND Atd.WorkDate BETWEEN pl.FromDate AND pl.ToDate and pl.PlantID=e.PlantId";
                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x WHERE x.SystemID NOT IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )    ORDER BY X.EmployeeCode ";
                //return _sqlRepository.GetDataTable(strSql);
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
                return dsRef;
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

        //Maternity with OT Auto
        public DataSet LoadEmpMaternityWithOTAuto(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons)
        {
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            clsAttnManualOverTime objManlAttn;
            objManlAttn = new clsAttnManualOverTime();
            objStatic = new clsStaticInfo();

            try
            {
                #region Variable


                string sFixShift = string.Empty;


                #endregion Variable

                #region Validation

                if (string.IsNullOrEmpty(PlantId) == true)
                {

                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (ProcDate == "" || bplib.clsWebLib.IsDateOK(ProcDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                objStatic.GetPlantWiseHRMSSetting(CompanyGroupID, PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                    IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                    if (IsPreallocationBasedOT)
                    {
                        IsPunchBasedOT = false;
                    }

                }




                #endregion Validation

                return GetEmpMaternityWithOTAuto(CompanyGroupID, PlantId, ProcDate, sOTValCons, IsPunchBasedOT, IsPreallocationBasedOT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function
        public DataSet GetEmpMaternityWithOTAuto(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");

                strSql = @"SELECT * FROM ( SELECT [CheckBoxSelect] =  Convert(bit, 'true'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ
                                , ES.ShiftSystemID, ES.ShiftName
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftInTime) , '00:00'),'hh:mm tt')  ShiftInTime
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftOutTime) , '00:00'),'hh:mm tt')  ShiftOutTime

                                    , De.UserName DepartmentName
                                    , EC.UserName EmpCategoryName
                                    --, Dsg.UserName DesignationName
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation 


                                    , ISNULL(Atd.IsManualInTime, 0) IsManualInTime
                                    , FORMAT(ISNULL(Convert(datetime, Atd.InTime) , '00:00'),'hh:mm tt') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, FORMAT(ISNULL(Convert(datetime, Atd.OutTime) , '00:00'),'hh:mm tt')  OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";








                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";


                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            NormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                NormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            NormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            NormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                , Atd.IsOTEntitled IsOTEntitle
                                , ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime
                                , ExtraOT=CASE WHEN ISNULL(Atd.OTHr,0)/60 > pl.firstSlab THEN 'YES' ELSE 'NO' END,ISNULL(Atd.OTHr, 0) OTHrInMin
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 

	                                LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 

                                    LEFT OUTER JOIN ORG.Line eL on eL.id=PMB.LineId
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"' )
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND (MaternityStatus='PRE' OR  MaternityStatus='POST')  AND MaternityStatus IS NOT NULL  
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"' )
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is not null	
                                                  and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND (MaternityStatus='PRE' OR  MaternityStatus='POST')  AND MaternityStatus IS NOT NULL  
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                   and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND (MaternityStatus='PRE' OR  MaternityStatus='POST')  AND MaternityStatus IS NOT NULL     
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"'  AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                  and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND (MaternityStatus='PRE' OR  MaternityStatus='POST')  AND MaternityStatus IS NOT NULL   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                               (
                                                  SELECT EDSA.EmpSystemID
                                                    ,EDSA.ShiftSystemID
                                                    ,ESA.IsFix
                                                    ,ESA.FixSystemID
                                                    ,ESA.IsRoster
                                                    ,ESA.RosterSystemID
                                                    ---,EDSA.DayType
									                ,DayType=dt.OriginalDayType
                                                    ,S.ShiftDefinationName ShiftName
                                                    ,S.ShiftType
                                                    ,CONVERT(VARCHAR(5), S.InTime, 108) xShiftInTime
                                                    ,CONVERT(VARCHAR(5), S.OutTime, 108) xShiftOutTime
                                                    ---STC starts
                                                    , ShiftInTime = CASE
                                                    WHEN Sc.InTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.InTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.InTime , 108)
                                                    END

                                                    , ShiftOutTime = CASE
                                                    WHEN Sc.OutTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.OutTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.OutTime , 108)
                                                    END
                                                    ---STC ends
                                                    ,DATEADD(MI, - S.InTimeStartMargin, S.InTime) OfficeStartTime
                                                    ,DATEADD(MI, S.LateMargin, S.InTime) OfficeTime
                                                    ,S.InTimeStartMargin
                                                    ,S.BreakStratTime
                                                    ,S.BreakEndTime
                                                    ,DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime
                                                    ,OTStartTime = CASE 
                                                    WHEN S.IsGapInclude = 1
                                                    THEN S.OutTime
                                                    ELSE DATEADD(MI, S.OTStartTime, S.OutTime)
                                                    END
                                                    FROM dbo.EmpDateWiseShiftAssign EDSA
                                                    LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
                                                    LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
                                                    ---STC starts
                                                    LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + strAttnDate + @"' BETWEEN FromDate AND ToDate) AS sc ON EDSA.ShiftSystemID = sc.ShiftDefinationID
                                                    ---STC ends

									                LEFT JOIN AttdnProcessData ap on EDSA.EmpSystemID=ap.EmpSystemID and ap.WorkDate=EDSA.WorkDate
									                Left JOIN DayType dt on dt.DayType=ap.DayStatus
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                       AND EDSA.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                ) ES ON E.SystemID = ES.EmpSystemID";



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }



                strSql = strSql + @"  LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = ES.DayType AND Atd.WorkDate BETWEEN pl.FromDate AND pl.ToDate and pl.PlantID=e.PlantId";

                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x WHERE x.SystemID NOT IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )    ORDER BY X.EmployeeCode ";
                //return _sqlRepository.GetDataCollection(strSql);
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
                return dsRef;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }







        //Confirmed Employee Data For Grid
        public IEnumerable<object> LoadConfirmedEmployeeDataForGrid(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons)
        {
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            clsAttnManualOverTime objManlAttn;
            objManlAttn = new clsAttnManualOverTime();
            objStatic = new clsStaticInfo();

            try
            {
                #region Variable


                string sFixShift = string.Empty;


                #endregion Variable

                #region Validation

                if (string.IsNullOrEmpty(PlantId) == true)
                {

                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (ProcDate == "" || bplib.clsWebLib.IsDateOK(ProcDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                objStatic.GetPlantWiseHRMSSetting(CompanyGroupID, PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                    IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                    if (IsPreallocationBasedOT)
                    {
                        IsPunchBasedOT = false;
                    }

                }




                #endregion Validation

                return GetEmpOTConfirmed(CompanyGroupID, PlantId, ProcDate, sOTValCons, IsPunchBasedOT, IsPreallocationBasedOT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function
        public IEnumerable<object> GetEmpOTConfirmed(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 'False'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName
      , FORMAT(ISNULL(Convert(datetime,  ES.ShiftInTime) , '00:00'),'hh:mm tt')  ShiftInTime
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftOutTime) , '00:00'),'hh:mm tt')  ShiftOutTime
, De.UserName DepartmentName, EC.UserName EmpCategoryName


---, Dsg.UserName DesignationName, 
                                   
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation 



,ISNULL(Atd.IsManualInTime, 0) IsManualInTime, FORMAT(ISNULL(Convert(datetime, Atd.InTime) , '00:00'),'hh:mm tt')  InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, FORMAT(ISNULL(Convert(datetime, Atd.OutTime) , '00:00'),'hh:mm tt')  OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), 
                                  NormalOTHrHour =  CAST((CAST(ISNULL(FOT.TotalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  NormalOTHrMinute =  CAST((CAST(ISNULL(FOT.TotalOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                  NormalOTHr = CAST((CAST(ISNULL(FOT.TotalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                      NormalOTHrInDecimal =  CAST(ISNULL(FOT.TotalOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, IsDeviation=case when Atd.OTHr=FOT.TotalOTHr then 0  else 1 end ,";


                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  CNormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  CNormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  CNormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  CNormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  CNormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  CNormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  CNormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  CNormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            CNormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            CNormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            CNormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                CNormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock,  ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            CNormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            CNormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            CNormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            CNormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                ,  Atd.IsOTEntitled IsOTEntitle
                                ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 
                                    LEFT OUTER JOIN 
							                     FinalOT AS FOT ON FOT.EmpSystemId= E.systemid AND FOT.WorkDate='" + strAttnDate + @"'
				                   
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
				                     

                                    LEFT OUTER JOIN ORG.Line eL on eL.id=PMB.LineId
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";


                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"' )
                                                    AND IsOTComfirm = 1 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 1 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm =1 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 1 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID
                                                    ,EDSA.ShiftSystemID
                                                    ,ESA.IsFix
                                                    ,ESA.FixSystemID
                                                    ,ESA.IsRoster
                                                    ,ESA.RosterSystemID
                                                    ,EDSA.DayType
                                                    ,S.ShiftDefinationName ShiftName
                                                    ,S.ShiftType
                                                    ,CONVERT(VARCHAR(5), S.InTime, 108) xShiftInTime
                                                    ,CONVERT(VARCHAR(5), S.OutTime, 108) xShiftOutTime
                                                    ---STC starts
                                                    , ShiftInTime = CASE
                                                    WHEN Sc.InTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.InTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.InTime , 108)
                                                    END

                                                    , ShiftOutTime = CASE
                                                    WHEN Sc.OutTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.OutTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.OutTime , 108)
                                                    END
                                                    ---STC ends
                                                    ,DATEADD(MI, - S.InTimeStartMargin, S.InTime) OfficeStartTime
                                                    ,DATEADD(MI, S.LateMargin, S.InTime) OfficeTime
                                                    ,S.InTimeStartMargin
                                                    ,S.BreakStratTime
                                                    ,S.BreakEndTime
                                                    ,DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime
                                                    ,OTStartTime = CASE 
                                                    WHEN S.IsGapInclude = 1
                                                    THEN S.OutTime
                                                    ELSE DATEADD(MI, S.OTStartTime, S.OutTime)
                                                    END
                                                    FROM dbo.EmpDateWiseShiftAssign EDSA
                                                    LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
                                                    LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
                                                    ---STC starts
                                                    LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + strAttnDate + @"' BETWEEN FromDate AND ToDate) AS sc ON EDSA.ShiftSystemID = sc.ShiftDefinationID
                                                    ---STC ends
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                        AND EDSA.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                ) ES ON E.SystemID = ES.EmpSystemID";




                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }




                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ORDER BY EmployeeCode";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Re-Confirmed Required
        public IEnumerable<object> LoadPostDeviationEmployeeDataForGrid(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons)
        {
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            clsAttnManualOverTime objManlAttn;
            objManlAttn = new clsAttnManualOverTime();
            objStatic = new clsStaticInfo();

            try
            {
                #region Variable


                string sFixShift = string.Empty;


                #endregion Variable

                #region Validation

                if (string.IsNullOrEmpty(PlantId) == true)
                {

                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (ProcDate == "" || bplib.clsWebLib.IsDateOK(ProcDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                objStatic.GetPlantWiseHRMSSetting(CompanyGroupID, PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                    IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                    if (IsPreallocationBasedOT)
                    {
                        IsPunchBasedOT = false;
                    }

                }




                #endregion Validation

                return GetEmpPostDeviationConfirmed(CompanyGroupID, PlantId, ProcDate, sOTValCons, IsPunchBasedOT, IsPreallocationBasedOT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function
        public IEnumerable<object> GetEmpPostDeviationConfirmed(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT * FROM ( SELECT [CheckBoxSelect] = Convert(bit, 'False'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName
      , FORMAT(ISNULL(Convert(datetime,  ES.ShiftInTime) , '00:00'),'hh:mm tt')  ShiftInTime
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftOutTime) , '00:00'),'hh:mm tt')  ShiftOutTime
, De.UserName DepartmentName, EC.UserName EmpCategoryName
--, Dsg.UserName DesignationName
                                    
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation 


, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, FORMAT(ISNULL(Convert(datetime, Atd.InTime) , '00:00'),'hh:mm tt')  InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, FORMAT(ISNULL(Convert(datetime, Atd.OutTime) , '00:00'),'hh:mm tt') OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";








                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";


                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            NormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                NormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            NormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            NormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                , Atd.IsOTEntitled IsOTEntitle
                                , ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime
                                , ExtraOT=CASE WHEN ISNULL(Atd.OTHr,0)/60 > pl.firstSlab THEN 'YES' ELSE 'NO' END
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
				                    
  
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 

                                    LEFT OUTER JOIN ORG.Line eL on eL.id=PMB.LineId
				                    

                                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and EmpSystemID IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and EmpSystemID IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )	
                                                    
                                                    and 1=2
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and EmpSystemID IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and EmpSystemID IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                               (
                                                  SELECT EDSA.EmpSystemID
                                                    ,EDSA.ShiftSystemID
                                                    ,ESA.IsFix
                                                    ,ESA.FixSystemID
                                                    ,ESA.IsRoster
                                                    ,ESA.RosterSystemID
                                                    ---,EDSA.DayType
									                ,DayType=dt.OriginalDayType
                                                    ,S.ShiftDefinationName ShiftName
                                                    ,S.ShiftType
                                                    ,CONVERT(VARCHAR(5), S.InTime, 108) xShiftInTime
                                                    ,CONVERT(VARCHAR(5), S.OutTime, 108) xShiftOutTime
                                                    ---STC starts
                                                    , ShiftInTime = CASE
                                                    WHEN Sc.InTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.InTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.InTime , 108)
                                                    END

                                                    , ShiftOutTime = CASE
                                                    WHEN Sc.OutTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.OutTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.OutTime , 108)
                                                    END
                                                    ---STC ends
                                                    ,DATEADD(MI, - S.InTimeStartMargin, S.InTime) OfficeStartTime
                                                    ,DATEADD(MI, S.LateMargin, S.InTime) OfficeTime
                                                    ,S.InTimeStartMargin
                                                    ,S.BreakStratTime
                                                    ,S.BreakEndTime
                                                    ,DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime
                                                    ,OTStartTime = CASE 
                                                    WHEN S.IsGapInclude = 1
                                                    THEN S.OutTime
                                                    ELSE DATEADD(MI, S.OTStartTime, S.OutTime)
                                                    END
                                                    FROM dbo.EmpDateWiseShiftAssign EDSA
                                                    LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
                                                    LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
                                                    ---STC starts
                                                    LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + strAttnDate + @"' BETWEEN FromDate AND ToDate) AS sc ON EDSA.ShiftSystemID = sc.ShiftDefinationID
                                                    ---STC ends
                                                    
									                LEFT JOIN AttdnProcessData ap on EDSA.EmpSystemID=ap.EmpSystemID and ap.WorkDate=EDSA.WorkDate
									                Left JOIN DayType dt on dt.DayType=ap.DayStatus
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                        AND EDSA.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                ) ES ON E.SystemID = ES.EmpSystemID";



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }



                strSql = strSql + @"  LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = ES.DayType AND Atd.WorkDate BETWEEN pl.FromDate AND pl.ToDate and pl.PlantID=e.PlantId";
                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x WHERE x.SystemID  IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )    ORDER BY X.EmployeeCode ";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> xGetEmpPostDeviationConfirmed(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT * FROM (SELECT [CheckBoxSelect] = Convert(bit, 'True'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                  ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, ISNULL(Atd.InTime, '00:00') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, ISNULL(Atd.OutTime, '00:00') OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), 
                                  NormalOTHrHour =  CAST((CAST(ISNULL(FOT.TotalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  NormalOTHrMinute =  CAST((CAST(ISNULL(FOT.TotalOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                  NormalOTHr = CAST((CAST(ISNULL(FOT.TotalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                      NormalOTHrInDecimal =  CAST(ISNULL(FOT.TotalOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, IsDeviation=case when Atd.OTHr=FOT.TotalOTHr then 0  else 1 end ,IsPostDeviation=case when Atd.DateOTComfirm >isnull(FOT.DateUpdated,FOT.DateAdded) then 1  else 0 end, ";


                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  CNormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  CNormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  CNormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  CNormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  CNormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  CNormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  CNormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  CNormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            CNormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            CNormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            CNormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                CNormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock,  ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            CNormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            CNormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            CNormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            CNormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                ,  Atd.IsOTEntitled IsOTEntitle
                                ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 
				                    LEFT OUTER JOIN 
							                    HKP.Designation AS Dsg ON Dsg.Id= E.GivenDesignationId 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 
                                    LEFT OUTER JOIN 
							                     FinalOT AS FOT ON FOT.EmpSystemId= E.systemid AND FOT.WorkDate='" + strAttnDate + @"'
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled ,DateUpdated,DateAdded,DateOTComfirm
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"' 
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))  
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled ,DateUpdated,DateAdded,DateOTComfirm
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"' 
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled ,DateUpdated,DateAdded,DateOTComfirm
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"' 
                                                        AND IsOTComfirm =0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled ,DateUpdated,DateAdded,DateOTComfirm
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"' 
                                                    AND IsOTComfirm = 0
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                        DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                        OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                        AND EDSA.PlantID = '" + plantId + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID";



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }




                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x
                                    WHERE x.SystemID IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )    ORDER BY X.EmployeeCode  ";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet LoadPostDeviationEmployeeDataForGridAuto(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons)
        {
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            clsAttnManualOverTime objManlAttn;
            objManlAttn = new clsAttnManualOverTime();
            objStatic = new clsStaticInfo();

            try
            {
                #region Variable


                string sFixShift = string.Empty;


                #endregion Variable

                #region Validation

                if (string.IsNullOrEmpty(PlantId) == true)
                {

                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (ProcDate == "" || bplib.clsWebLib.IsDateOK(ProcDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                objStatic.GetPlantWiseHRMSSetting(CompanyGroupID, PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                    IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                    if (IsPreallocationBasedOT)
                    {
                        IsPunchBasedOT = false;
                    }

                }




                #endregion Validation

                return GetEmpPostDeviationConfirmedAuto(CompanyGroupID, PlantId, ProcDate, sOTValCons, IsPunchBasedOT, IsPreallocationBasedOT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function
        public DataSet GetEmpPostDeviationConfirmedAuto(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                strSql = @"SELECT * FROM ( SELECT [CheckBoxSelect] = Convert(bit, 'true'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName
      , FORMAT(ISNULL(Convert(datetime,  ES.ShiftInTime) , '00:00'),'hh:mm tt')  ShiftInTime
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftOutTime) , '00:00'),'hh:mm tt')  ShiftOutTime
, De.UserName DepartmentName, EC.UserName EmpCategoryName
--, Dsg.UserName DesignationName
                                    
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation 


                                  , ISNULL(Atd.IsManualInTime, 0) IsManualInTime, FORMAT(ISNULL(Convert(datetime, Atd.InTime) , '00:00'),'hh:mm tt')  InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, FORMAT(ISNULL(Convert(datetime, Atd.OutTime) , '00:00'),'hh:mm tt') OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";








                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";


                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            NormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                NormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            NormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            NormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                ,  Atd.IsOTEntitled IsOTEntitle
                                ,  ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                                , ExtraOT=CASE WHEN ISNULL(Atd.OTHr,0)/60 > pl.firstSlab THEN 'YES' ELSE 'NO' END,ISNULL(Atd.OTHr, 0) OTHrInMin
                            FROM EmployeeInformation AS E  
                                   LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 
				                    LEFT OUTER JOIN 
							                    HKP.Designation AS Dsg ON Dsg.Id= E.GivenDesignationId 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
				                    
  
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 

                                    LEFT OUTER JOIN ORG.Line eL on eL.id=PMB.LineId
				                    

                                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and EmpSystemID IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and EmpSystemID IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )	
                                                    
                                                    and 1=2
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and EmpSystemID IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and EmpSystemID IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                               (
                                                  SELECT EDSA.EmpSystemID
                                                    ,EDSA.ShiftSystemID
                                                    ,ESA.IsFix
                                                    ,ESA.FixSystemID
                                                    ,ESA.IsRoster
                                                    ,ESA.RosterSystemID
                                                    ,EDSA.DayType
                                                    ,S.ShiftDefinationName ShiftName
                                                    ,S.ShiftType
                                                    ,CONVERT(VARCHAR(5), S.InTime, 108) xShiftInTime
                                                    ,CONVERT(VARCHAR(5), S.OutTime, 108) xShiftOutTime
                                                    ---STC starts
                                                    , ShiftInTime = CASE
                                                    WHEN Sc.InTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.InTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.InTime , 108)
                                                    END

                                                    , ShiftOutTime = CASE
                                                    WHEN Sc.OutTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.OutTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.OutTime , 108)
                                                    END
                                                    ---STC ends
                                                    ,DATEADD(MI, - S.InTimeStartMargin, S.InTime) OfficeStartTime
                                                    ,DATEADD(MI, S.LateMargin, S.InTime) OfficeTime
                                                    ,S.InTimeStartMargin
                                                    ,S.BreakStratTime
                                                    ,S.BreakEndTime
                                                    ,DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime
                                                    ,OTStartTime = CASE 
                                                    WHEN S.IsGapInclude = 1
                                                    THEN S.OutTime
                                                    ELSE DATEADD(MI, S.OTStartTime, S.OutTime)
                                                    END
                                                    FROM dbo.EmpDateWiseShiftAssign EDSA
                                                    LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
                                                    LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
                                                    ---STC starts
                                                    LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + strAttnDate + @"' BETWEEN FromDate AND ToDate) AS sc ON EDSA.ShiftSystemID = sc.ShiftDefinationID
                                                    ---STC ends
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                        AND EDSA.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                ) ES ON E.SystemID = ES.EmpSystemID";



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }



                strSql = strSql + @"  LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = ES.DayType AND Atd.WorkDate BETWEEN pl.FromDate AND pl.ToDate and pl.PlantID=e.PlantId";

                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x WHERE x.SystemID  IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )    ORDER BY X.EmployeeCode ";
                //return _sqlRepository.GetDataCollection(strSql);
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
                return dsRef;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Miss Punch 
        public IEnumerable<object> LoadMissPunchEmployeeDataForGrid(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons)
        {
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            clsAttnManualOverTime objManlAttn;
            objManlAttn = new clsAttnManualOverTime();
            objStatic = new clsStaticInfo();

            try
            {
                #region Variable


                string sFixShift = string.Empty;


                #endregion Variable

                #region Validation

                if (string.IsNullOrEmpty(PlantId) == true)
                {

                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (ProcDate == "" || bplib.clsWebLib.IsDateOK(ProcDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                objStatic.GetPlantWiseHRMSSetting(CompanyGroupID, PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                    IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                    if (IsPreallocationBasedOT)
                    {
                        IsPunchBasedOT = false;
                    }

                }




                #endregion Validation

                return GetEmpMissPunch(CompanyGroupID, PlantId, ProcDate, sOTValCons, IsPunchBasedOT, IsPreallocationBasedOT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function


        public IEnumerable<object> GetEmpMissPunch(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT * FROM ( SELECT [CheckBoxSelect] = Convert(bit, 'False'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName
      , FORMAT(ISNULL(Convert(datetime,  ES.ShiftInTime) , '00:00'),'hh:mm tt')  ShiftInTime
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftOutTime) , '00:00'),'hh:mm tt')  ShiftOutTime
, De.UserName DepartmentName, EC.UserName EmpCategoryName
--, Dsg.UserName DesignationName

                            
                                    --, Dsg.UserName DesignationName
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation 

, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, FORMAT(ISNULL(Convert(datetime, Atd.InTime) , '00:00'),'hh:mm tt')  InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, FORMAT(ISNULL(Convert(datetime, Atd.OutTime) , '00:00'),'hh:mm tt') OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";








                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";


                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            NormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                NormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            NormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            NormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                ,  Atd.IsOTEntitled IsOTEntitle
                                , ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                                , ExtraOT=CASE WHEN ISNULL(Atd.OTHr,0)/60 > pl.firstSlab THEN 'YES' ELSE 'NO' END
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID

 
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
				                    

                                    LEFT OUTER JOIN ORG.Line eL on eL.id=PMB.LineId
				                     
                                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 

                                   
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')     
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is  null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) and   DayStatus not in ('RST')
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is  null
                                                    and 1=2
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is  null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))  and   DayStatus not in ('RST')
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is  null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) and   DayStatus not in ('RST')
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                              (
                                                  SELECT EDSA.EmpSystemID
                                                    ,EDSA.ShiftSystemID
                                                    ,ESA.IsFix
                                                    ,ESA.FixSystemID
                                                    ,ESA.IsRoster
                                                    ,ESA.RosterSystemID
                                                    ,EDSA.DayType
                                                    ,S.ShiftDefinationName ShiftName
                                                    ,S.ShiftType
                                                    ,CONVERT(VARCHAR(5), S.InTime, 108) xShiftInTime
                                                    ,CONVERT(VARCHAR(5), S.OutTime, 108) xShiftOutTime
                                                    ---STC starts
                                                    , ShiftInTime = CASE
                                                    WHEN Sc.InTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.InTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.InTime , 108)
                                                    END

                                                    , ShiftOutTime = CASE
                                                    WHEN Sc.OutTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.OutTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.OutTime , 108)
                                                    END
                                                    ---STC ends
                                                    ,DATEADD(MI, - S.InTimeStartMargin, S.InTime) OfficeStartTime
                                                    ,DATEADD(MI, S.LateMargin, S.InTime) OfficeTime
                                                    ,S.InTimeStartMargin
                                                    ,S.BreakStratTime
                                                    ,S.BreakEndTime
                                                    ,DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime
                                                    ,OTStartTime = CASE 
                                                    WHEN S.IsGapInclude = 1
                                                    THEN S.OutTime
                                                    ELSE DATEADD(MI, S.OTStartTime, S.OutTime)
                                                    END
                                                    FROM dbo.EmpDateWiseShiftAssign EDSA
                                                    LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
                                                    LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
                                                    ---STC starts
                                                    LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + strAttnDate + @"' BETWEEN FromDate AND ToDate) AS sc ON EDSA.ShiftSystemID = sc.ShiftDefinationID
                                                    ---STC ends
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                        AND  EDSA.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                ) ES ON E.SystemID = ES.EmpSystemID";




                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }




                strSql = strSql + @"  LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = ES.DayType AND Atd.WorkDate BETWEEN pl.FromDate AND pl.ToDate and pl.PlantID=e.PlantId";

                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"'  
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x WHERE x.SystemID NOT IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )    ORDER BY X.EmployeeCode ";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> xGetEmpMissPunch(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT * FROM (SELECT [CheckBoxSelect] = Convert(bit, 'True'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                  ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, ISNULL(Atd.InTime, '00:00') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, ISNULL(Atd.OutTime, '00:00') OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), 
                                  NormalOTHrHour =  CAST((CAST(ISNULL(FOT.TotalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  NormalOTHrMinute =  CAST((CAST(ISNULL(FOT.TotalOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                  NormalOTHr = CAST((CAST(ISNULL(FOT.TotalOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                      NormalOTHrInDecimal =  CAST(ISNULL(FOT.TotalOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, IsDeviation=case when Atd.OTHr=FOT.TotalOTHr then 0  else 1 end ,IsPostDeviation=case when isnull(Atd.DateUpdated,Atd.DateAdded) >isnull(FOT.DateUpdated,FOT.DateAdded) then 1  else 0 end, ";


                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"
                                  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),

                                  CNormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  CNormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  CNormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  CNormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"    
                                  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), 

                                  CNormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  CNormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  CNormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  CNormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            CNormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            CNormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            CNormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                CNormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock,  ";
                    }
                    else
                    {
                        strSql = strSql + @"  
                                           
                                            CNormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            CNormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            CNormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            CNormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                ,  Atd.IsOTEntitled IsOTEntitle
                                ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 
                                    LEFT OUTER JOIN 
							                     FinalOT AS FOT ON FOT.EmpSystemId= E.systemid AND FOT.WorkDate='" + strAttnDate + @"'
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled ,DateUpdated,DateAdded
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"' 
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is  null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))  
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }

                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled ,DateUpdated,DateAdded
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"' 
                                                    AND IsOTComfirm =0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is  null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }

                strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                        DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                        OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                        AND EDSA.PlantID = '" + plantId + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID";



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";




                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND PlantID = '" + plantId + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }





                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x
                                    WHERE '" + IsPunchBasedOT + "'='True'  ORDER BY X.EmployeeCode  ";


                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Maternity with OT
        public IEnumerable<object> LoadEmpMaternityWithOT(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons)
        {
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            clsAttnManualOverTime objManlAttn;
            objManlAttn = new clsAttnManualOverTime();
            objStatic = new clsStaticInfo();

            try
            {
                #region Variable


                string sFixShift = string.Empty;


                #endregion Variable

                #region Validation

                if (string.IsNullOrEmpty(PlantId) == true)
                {

                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (ProcDate == "" || bplib.clsWebLib.IsDateOK(ProcDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                objStatic.GetPlantWiseHRMSSetting(CompanyGroupID, PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                    IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                    if (IsPreallocationBasedOT)
                    {
                        IsPunchBasedOT = false;
                    }

                }




                #endregion Validation

                return GetEmpMaternityWithOT(CompanyGroupID, PlantId, ProcDate, sOTValCons, IsPunchBasedOT, IsPreallocationBasedOT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function
        public IEnumerable<object> GetEmpMaternityWithOT(string companyGroupId, string plantId, string strAttnDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT * FROM ( SELECT [CheckBoxSelect] = Convert(bit, 'False'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ
                                , ES.ShiftSystemID, ES.ShiftName
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftInTime) , '00:00'),'hh:mm tt')  ShiftInTime
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftOutTime) , '00:00'),'hh:mm tt')  ShiftOutTime

                                    , De.UserName DepartmentName
                                    , EC.UserName EmpCategoryName
                                    --, Dsg.UserName DesignationName
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation 


                                    , ISNULL(Atd.IsManualInTime, 0) IsManualInTime
                                    , FORMAT(ISNULL(Convert(datetime, Atd.InTime) , '00:00'),'hh:mm tt') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, FORMAT(ISNULL(Convert(datetime, Atd.OutTime) , '00:00'),'hh:mm tt')  OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";








                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";


                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            NormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                NormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            NormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            NormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                ,  Atd.IsOTEntitled IsOTEntitle
                                , ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                                , ExtraOT=CASE WHEN ISNULL(Atd.OTHr,0)/60 > pl.firstSlab THEN 'YES' ELSE 'NO' END
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 							      

	                                LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 

                                    LEFT OUTER JOIN ORG.Line eL on eL.id=PMB.LineId
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND (MaternityStatus='PRE' OR  MaternityStatus='POST')  AND MaternityStatus IS NOT NULL  
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is not null	
                                                  and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND (MaternityStatus='PRE' OR  MaternityStatus='POST')  AND MaternityStatus IS NOT NULL  
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                   and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND (MaternityStatus='PRE' OR  MaternityStatus='POST')  AND MaternityStatus IS NOT NULL   
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) AND (MaternityStatus='PRE' OR  MaternityStatus='POST')  AND MaternityStatus IS NOT NULL 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                               (
                                                  SELECT EDSA.EmpSystemID
                                                    ,EDSA.ShiftSystemID
                                                    ,ESA.IsFix
                                                    ,ESA.FixSystemID
                                                    ,ESA.IsRoster
                                                    ,ESA.RosterSystemID
                                                    ,EDSA.DayType
                                                    ,S.ShiftDefinationName ShiftName
                                                    ,S.ShiftType
                                                    ,CONVERT(VARCHAR(5), S.InTime, 108) xShiftInTime
                                                    ,CONVERT(VARCHAR(5), S.OutTime, 108) xShiftOutTime
                                                    ---STC starts
                                                    , ShiftInTime = CASE
                                                    WHEN Sc.InTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.InTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.InTime , 108)
                                                    END

                                                    , ShiftOutTime = CASE
                                                    WHEN Sc.OutTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.OutTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.OutTime , 108)
                                                    END
                                                    ---STC ends
                                                    ,DATEADD(MI, - S.InTimeStartMargin, S.InTime) OfficeStartTime
                                                    ,DATEADD(MI, S.LateMargin, S.InTime) OfficeTime
                                                    ,S.InTimeStartMargin
                                                    ,S.BreakStratTime
                                                    ,S.BreakEndTime
                                                    ,DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime
                                                    ,OTStartTime = CASE 
                                                    WHEN S.IsGapInclude = 1
                                                    THEN S.OutTime
                                                    ELSE DATEADD(MI, S.OTStartTime, S.OutTime)
                                                    END
                                                    FROM dbo.EmpDateWiseShiftAssign EDSA
                                                    LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
                                                    LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
                                                    ---STC starts
                                                    LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + strAttnDate + @"' BETWEEN FromDate AND ToDate) AS sc ON EDSA.ShiftSystemID = sc.ShiftDefinationID
                                                    ---STC ends
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' 
                                                        AND EDSA.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                ) ES ON E.SystemID = ES.EmpSystemID";



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }




                strSql = strSql + @"  LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = ES.DayType AND Atd.WorkDate BETWEEN pl.FromDate AND pl.ToDate and pl.PlantID=e.PlantId";

                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x WHERE x.SystemID NOT IN ( SELECT EmpSystemID FROM FinalOT WHERE WorkDate='" + strAttnDate + @"' )    ORDER BY X.EmployeeCode ";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        public bool ShowOTValueFromHRMSSetting(string companyGroupId, string plantId)
        {
            DataSet dsLocal = null;
            clsStaticInfo objStatic = null;
            bool OTValueVisible = false;
            try
            {
                objStatic = new clsStaticInfo();
                objStatic.GetPlantWiseHRMSSetting(companyGroupId, plantId, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    if (/*Convert.ToBoolean(dsLocal.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim()) == true &&*/ Convert.ToBoolean(dsLocal.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim()) == true)
                    {
                        OTValueVisible = true;

                    }

                }
                else
                {
                    OTValueVisible = false;

                }
                return OTValueVisible;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dsLocal = null;
                objStatic = null;
            }
        }//End Function


        public void SaveData(string ProcDate, DataSet dsGrd)
        {
            DataSet dsLocal = null;
            //DataSet dsGrd = null;

            DataSet dsFinalOT = null;
            DataRow drFinalOT = null;
            DataTable dtFinalOT = null;
            DataView dvFinalOT = null;

            DataSet dsAttProc = null;
            DataRow drAttProc = null;
            DataTable dtAttProc = null;
            DataView dvAttProc = null;

            //DataSet dsOTSlabEmp = null;
            //DataTable dtOTSlabEmp = null;
            //DataView dvOTSlabEmp = null;

            DataSet dsOTSlabGen = null;
            DataTable dtOTSlabGen = null;

            AttendanceProcessAplos objAttdnProc;
            objAttdnProc = new AttendanceProcessAplos();

            clsAttnManualOverTime objAttdnManOT;
            objAttdnManOT = new clsAttnManualOverTime();

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            decimal dOTHour = 0;
            decimal dOTMinute = 0;
            decimal dOTDecimal = 0;

            decimal iTotalOTHr = 0;
            decimal iNormalOTHr = 0;
            decimal iExtraOTHr = 0;

            string sEmpSysID = "";
            string sOTDayType = "";
            decimal dfirstSlab = 0;
            bool bIsOTExtentNextSlab = false;




            DataSet dsLocalHRMSSetting = null;
            string MinimumOTMinute = string.Empty;
            string OTConsiderOn = string.Empty;
            string OTFractionCalculate = string.Empty;

            objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
            if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
            {
                MinimumOTMinute = dsLocalHRMSSetting.Tables[0].Rows[0]["MinimumOTMinute"].ToString().Trim();
                OTConsiderOn = dsLocalHRMSSetting.Tables[0].Rows[0]["OTConsiderOn"].ToString().Trim();
                OTFractionCalculate = dsLocalHRMSSetting.Tables[0].Rows[0]["OTFractionCalculation"].ToString().Trim();

            }

            try
            {
                #region CHECK EDIT/UPDATE ACCESS

                var ob = new clsStaticInfo();
                //ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);
                //ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);

                #endregion CHECK EDIT/UPDATE ACCESS

                if (string.IsNullOrEmpty(ProcDate) == true || bplib.clsWebLib.IsDateOK(ProcDate) == false)
                {
                    //txtProcDate.Focus();
                    Exception ex = new Exception("Please Define Data Download Date..! (allowed format is  dd-MMM-yyyy ex: '01-Jan-2014')");
                    throw (ex);
                }
                if (string.IsNullOrEmpty(MinimumOTMinute) == false & bplib.clsWebLib.IsNumeric(MinimumOTMinute) == false)
                {
                    //lblMinOT.Focus();
                    Exception ex = new Exception("Please enter numeric value...");
                    throw (ex);
                }



                //objAttdnProc.GetAttdnLock(identity.CompanyGroupId, identity.PlantId, out dsLocal);
                GetAttdnLock(identity.CompanyGroupId, identity.PlantId, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    DateTime dtDtLock = bplib.clsWebLib.DateData_DBToApp(dsLocal.Tables[0].Rows[0]["LockDate"].ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                    DateTime dtDtProc = bplib.clsWebLib.DateData_DBToApp(ProcDate, bplib.clsWebLib.DB_DATE_FORMAT);
                    TimeSpan ts = dtDtLock - dtDtProc;
                    int days = ts.Days;
                    if (days >= 0)
                    {
                        //txtProcDate.Focus();
                        Exception ex = new Exception("Please check the attendance process date, cannot less than or equal attendance lock date :- " + Convert.ToDateTime(dsLocal.Tables[0].Rows[0]["LockDate"].ToString()).ToString("dd-MMM-yyyy") + "  ...");
                        throw (ex);
                    }
                }

                //lblEmpSysIDCollForFnc.Text = "'" + EmpSystemID;
                //lblEmpSysIDCollForFnc.Text += "," + EmpSystemID;

                //objAttdnProc.GetOTSlabDefineGeneral(identity.CompanyGroupId, identity.PlantId, ProcDate, out dsOTSlabGen);
                GetOTSlabDefineGeneral(identity.CompanyGroupId, identity.PlantId, ProcDate, out dsOTSlabGen);
                dtOTSlabGen = dsOTSlabGen.Tables[0];

                //LoadDataSetFromDataGrid(ref dgAttdnProc, out dsGrd);
                string lblEmpSysIDForAttdSummry = "";
                string EmpSysIDForExtraOT = "";
                if (dsGrd.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsGrd.Tables[0].Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(dsGrd.Tables[0].Rows[i]["CheckBoxSelect"].ToString().Trim()) == true)
                        {
                            if (lblEmpSysIDForAttdSummry == "")
                            {
                                lblEmpSysIDForAttdSummry = "'" + dsGrd.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                            else
                            {
                                lblEmpSysIDForAttdSummry = lblEmpSysIDForAttdSummry + ", '" + dsGrd.Tables[0].Rows[i]["SystemID"].ToString().Trim() + "'";
                            }
                            // for ExtraOT
                            if (dsGrd.Tables[0].Rows[i]["ExtraOT"].ToString().Trim() == "YES")
                            {
                                if (EmpSysIDForExtraOT == "")
                                {
                                    EmpSysIDForExtraOT = "'" + dsGrd.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim() + "'";
                                }
                                else
                                {
                                    EmpSysIDForExtraOT = EmpSysIDForExtraOT + ", '" + dsGrd.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim() + "'";
                                }
                            }//


                        }
                    }
                }

                if (string.IsNullOrEmpty(lblEmpSysIDForAttdSummry.Trim()) == true)
                {
                    //lblEmpSysIDForAttdSummry.Focus();
                    Exception ex = new Exception("Please select employee...");
                    throw (ex);
                }
                if (string.IsNullOrEmpty(EmpSysIDForExtraOT.Trim()) == false)
                {
                    //lblEmpSysIDForAttdSummry.Focus();
                    Exception ex = new Exception("This employee exceeding the OT limit [" + EmpSysIDForExtraOT + "]");
                    throw (ex);
                }
                try
                {
                    objAttdnProc.LockValidation(identity.PlantId, ProcDate, ProcDate, lblEmpSysIDForAttdSummry);

                }
                catch (Exception ex)
                {

                    //Exception ex = new Exception("Please confirm all employees OT on ["+ ProcDate+"].");
                    throw (ex);
                }

                ///objAttdnProc.GetOTSlabDefineEmployee(identity.CompanyGroupId, lblEmpSysIDForAttdSummry, ProcDate, out dsOTSlabEmp);
                //GetOTSlabDefineEmployee(identity.CompanyGroupId, lblEmpSysIDForAttdSummry, ProcDate, out dsOTSlabEmp);

                //dtOTSlabEmp = dsOTSlabEmp.Tables[0];





                objAttdnManOT.GetFinalOT(identity.CompanyGroupId, identity.PlantId, lblEmpSysIDForAttdSummry, ProcDate, out dsFinalOT);
                dtFinalOT = dsFinalOT.Tables[0];
                dvFinalOT = new DataView();

                objAttdnManOT.GetAttdnProcessData(identity.CompanyGroupId, identity.PlantId, lblEmpSysIDForAttdSummry.Trim(), ProcDate, out dsAttProc);
                dtAttProc = dsAttProc.Tables[0];
                dvAttProc = new DataView();

                if (dsGrd.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsGrd.Tables[0].Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(dsGrd.Tables[0].Rows[i]["CheckBoxSelect"].ToString().Trim()) == true)
                        {
                            sEmpSysID = dsGrd.Tables[0].Rows[i]["SystemID"].ToString().Trim();
                            sOTDayType = dsGrd.Tables[0].Rows[i]["DayType"].ToString().Trim();
                            dOTHour = 0;
                            dOTMinute = 0;
                            dOTDecimal = 0;

                            //iTotalOTHr = Convert.ToDecimal(dsGrd.Tables[0].Rows[i]["OTHr"].ToString().Trim());
                            iExtraOTHr = 0;
                            iNormalOTHr = 0;
                            iTotalOTHr = 0;

                            if (OTConsiderOn.ToUpper() == "HOUR MINUTE VALUE")
                            {
                                dOTHour = Convert.ToDecimal(dsGrd.Tables[0].Rows[i]["NormalOTHrHour"].ToString().Trim());
                                dOTMinute = Convert.ToDecimal(dsGrd.Tables[0].Rows[i]["NormalOTHrMinute"].ToString().Trim());
                                iTotalOTHr = (dOTHour * 60) + dOTMinute;
                            }
                            else if (OTConsiderOn.ToUpper() == "DECIMAL VALUE")
                            {
                                dOTDecimal = Convert.ToDecimal(dsGrd.Tables[0].Rows[i]["NormalOTHrInDecimal"].ToString().Trim());
                                iTotalOTHr = (dOTDecimal * 60);
                            }

                            //dvOTSlabEmp = new DataView();
                            //dvOTSlabEmp.Table = dtOTSlabEmp;
                            //dvOTSlabEmp.RowFilter = "EmpSystemID = '" + sEmpSysID + "'";
                            //if (dvOTSlabEmp.Count > 0)
                            //{
                            //    dfirstSlab = (Convert.ToDecimal(dvOTSlabEmp[0].Row["firstSlab"].ToString()) * 60);
                            //    bIsOTExtentNextSlab = Convert.ToBoolean(dvOTSlabEmp[0].Row["IsOTExtentNextSlab"].ToString());
                            //}
                            if (dsOTSlabGen.Tables[0].Rows.Count > 0)
                            {
                                dfirstSlab = (Convert.ToDecimal(dsOTSlabGen.Tables[0].Rows[0]["firstSlab"].ToString()) * 60);
                                bIsOTExtentNextSlab = Convert.ToBoolean(dsOTSlabGen.Tables[0].Rows[0]["IsOTExtentNextSlab"].ToString());
                            }

                            if (iTotalOTHr > dfirstSlab)
                            {
                                //iNormalOTHr = dfirstSlab;
                                //if (bIsOTExtentNextSlab == true)
                                //{
                                //    iExtraOTHr = iTotalOTHr - iNormalOTHr;
                                //}
                                //if (iNormalOTHr == 0)
                                //{
                                //    iNormalOTHr = iTotalOTHr;
                                //}
                                iNormalOTHr = iTotalOTHr;

                                //Exception ex = new Exception("This employee exceeding the OT limit [" + dsGrd.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim() + "]");
                                //throw (ex);
                            }
                            else
                            {
                                iNormalOTHr = iTotalOTHr;
                            }

                            //if (bIsOTExtentNextSlab == true)
                            //{
                            //    iExtraOTHr = iTotalOTHr - iNormalOTHr;
                            //}

                            dvFinalOT.Table = dtFinalOT;
                            dvFinalOT.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "'";
                           
                            if (dvFinalOT.Count > 0)
                            {
                                drFinalOT = dvFinalOT[0].Row;
                                drFinalOT.BeginEdit();

                                drFinalOT["OTDayType"] = sOTDayType.Trim();
                                drFinalOT["WorkDate"] = ProcDate;
                                drFinalOT["TotalOTHr"] = iTotalOTHr;
                                drFinalOT["NormalOTHr"] = iNormalOTHr;
                                drFinalOT["ExtraOTHr"] = iExtraOTHr;

                                drFinalOT["GroupID"] = bplib.clsWebLib.RetValidLen(identity.CompanyGroupId);
                                drFinalOT["PlantID"] = bplib.clsWebLib.RetValidLen(identity.PlantId);

                                drFinalOT["UpdatedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                                drFinalOT["DateUpdated"] = DateTime.Now;
                                drFinalOT.EndEdit();
                            }
                            else
                            {
                                drFinalOT = dsFinalOT.Tables[0].NewRow();
                                drFinalOT["AddedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                                drFinalOT["DateAdded"] = DateTime.Now;

                                drFinalOT["OTDayType"] = sOTDayType.Trim();
                                drFinalOT["EmpSystemID"] = sEmpSysID.Trim();
                                drFinalOT["WorkDate"] = ProcDate;
                                drFinalOT["TotalOTHr"] = iTotalOTHr;
                                drFinalOT["NormalOTHr"] = iNormalOTHr;
                                drFinalOT["ExtraOTHr"] = iExtraOTHr;

                                drFinalOT["GroupID"] = bplib.clsWebLib.RetValidLen(identity.CompanyGroupId);
                                drFinalOT["PlantID"] = bplib.clsWebLib.RetValidLen(identity.PlantId);

                                drFinalOT["UpdatedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                                drFinalOT["DateUpdated"] = DateTime.Now;
                                dsFinalOT.Tables[0].Rows.Add(drFinalOT);
                            }

                            dvAttProc.Table = dtAttProc;
                            dvAttProc.RowFilter = "EmpSystemID = '" + sEmpSysID.Trim() + "'";
                            if (dvFinalOT.Count > 0)
                            {
                                drAttProc = dvAttProc[0].Row;
                                drAttProc.BeginEdit();

                                drAttProc["IsOTComfirm"] = true;

                                drAttProc["OTComfirmBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                                drAttProc["DateOTComfirm"] = DateTime.Now;

                                drAttProc.EndEdit();
                            }
                        }
                    }
                }

                objStatic.SaveDataSets(dsFinalOT, dsAttProc);



                //ShowMessage("Data Saved Sucessfully...");
                //Button_save.Enabled = false;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                // clean variable
            }
        }//End Function
        public void GetOTSlabDefineEmployee(string sGroupID, string sEmpSystemIDColl, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.OTSlabDefineEmployee
WHERE '" + sAttnDate + @"' BETWEEN FromDate AND ToDate
AND GroupID = '" + sGroupID + @"'
AND EmpSystemID IN (" + sEmpSystemIDColl + @"
--SELECT DISTINCT EmpSystemID FROM ftEmployeeJobLocationDateWise('" + sAttnDate + @"', '" + sAttnDate + @"', )
-- WHERE JobLcSystemID IN (
-- SELECT SystemID FROM [dbo].[JobLocation]
-- WHERE PlantID =
-- )
)";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        //======================employee wise=============================
        //un-Confirmed Employee Data For Grid
        public IEnumerable<object> LoadEmpWiseDataForOTConfirmation(string CompanyGroupID, string PlantId, string EmpId, string FDate, string TDate, string sOTValCons)
        {
            bool IsPunchBasedOT = true;
            bool IsPreallocationBasedOT = false;

            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            clsAttnManualOverTime objManlAttn;
            objManlAttn = new clsAttnManualOverTime();
            objStatic = new clsStaticInfo();

            try
            {
                #region Variable


                string sFixShift = string.Empty;


                #endregion Variable

                #region Validation

                if (string.IsNullOrEmpty(PlantId) == true)
                {

                    Exception ex = new Exception("Select Plant First...");
                    throw (ex);
                }
                if (FDate == "" || bplib.clsWebLib.IsDateOK(FDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                if (TDate == "" || bplib.clsWebLib.IsDateOK(TDate) == false)
                {

                    Exception ex = new Exception("Please define from date .... (allowed format is  dd-MMM-yyyy ex: '01-jan-2014')");
                    throw (ex);
                }
                objStatic.GetPlantWiseHRMSSetting(CompanyGroupID, PlantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    //IsPunchBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPunchBasedOT"].ToString().Trim());
                    IsPreallocationBasedOT = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsPreallocationBasedOT"].ToString().Trim());
                    if (IsPreallocationBasedOT)
                    {
                        IsPunchBasedOT = false;
                    }

                }




                #endregion Validation

                return GetEmpWiseDataForOTConfirmation(CompanyGroupID, PlantId, EmpId, FDate, TDate, sOTValCons, IsPunchBasedOT, IsPreallocationBasedOT);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function
        public IEnumerable<object> GetEmpWiseDataForOTConfirmation(string companyGroupId, string plantId, string EmpId, string FDate, string TDate, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT)
        {
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT * FROM ( SELECT [CheckBoxSelect] = Convert(bit, 'False'),  FORMAT(Atd.WorkDate,'dd-MMM-yyyy') WorkDate
								  ,E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ
                                , ES.ShiftSystemID, ES.ShiftName
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftInTime) , '00:00'),'hh:mm tt')  ShiftInTime
                                    , FORMAT(ISNULL(Convert(datetime,  ES.ShiftOutTime) , '00:00'),'hh:mm tt')  ShiftOutTime

                                    , De.UserName DepartmentName
                                    , EC.UserName EmpCategoryName
                                    --, Dsg.UserName DesignationName
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation 


                                    , ISNULL(Atd.IsManualInTime, 0) IsManualInTime
                                    , FORMAT(ISNULL(Convert(datetime, Atd.InTime) , '00:00'),'hh:mm tt') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, FORMAT(ISNULL(Convert(datetime, Atd.OutTime) , '00:00'),'hh:mm tt')  OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";








                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";


                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else
                {
                    if (IsPreallocationBasedOT == true)
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
                                            NormalOTHr = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) , 
			                                NormalOTHrInDecimal =  CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) , ISNULL(Atd.IsLock, 0) IsLock, ";
                    }
                    else
                    {
                        strSql = strSql + @"                
                                            NormalOTHrHour =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)),														
                                            NormalOTHrMinute =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),													
                                            NormalOTHr =  CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)),														
                                            NormalOTHrInDecimal =  CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)),";
                    }
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 --,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								  --                                else ISNULL(EmOT.IsOTEntitle, 0) end 
                                , Atd.IsOTEntitled IsOTEntitle
                                , ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                                , ExtraOT=CASE WHEN ISNULL(Atd.OTHr,0)/60 > pl.firstSlab THEN 'YES' ELSE 'NO' END
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 

	                                LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
                                    LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 

                                    LEFT OUTER JOIN ORG.Line eL on eL.id=PMB.LineId
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate between '" + FDate + @"' and '" + TDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))  
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate between '" + FDate + @"' and '" + TDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    --and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate between '" + FDate + @"' and '" + TDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                        AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late'))
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime , IsOTEntitled
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate between '" + FDate + @"' and '" + TDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
                                                    AND IsOTComfirm = 0 
                                                    and IsOTEntitled=1 
                                                    and OutTime is not null	
                                                    and DayStatus in (select DayType from DayType where category in ('Present','Late')) 
                                                    and 1=(
													case when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=1 and  OTHr>0 then 1
													     when (select IsOTConfirmationAutoForZero from PlantWiseHRMSSetting where PlantID='" + plantId + @"')=0 and  OTHr>=0 then 1
													else 0 end
													)
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                               (
                                                  SELECT EDSA.EmpSystemID,EDSA.WorkDate
                                                    ,EDSA.ShiftSystemID
                                                    ,ESA.IsFix
                                                    ,ESA.FixSystemID
                                                    ,ESA.IsRoster
                                                    ,ESA.RosterSystemID
                                                    ---,EDSA.DayType
									                ,DayType=dt.OriginalDayType
                                                    ,S.ShiftDefinationName ShiftName
                                                    ,S.ShiftType
                                                    ,CONVERT(VARCHAR(5), S.InTime, 108) xShiftInTime
                                                    ,CONVERT(VARCHAR(5), S.OutTime, 108) xShiftOutTime
                                                    ---STC starts
                                                    , ShiftInTime = CASE
                                                    WHEN Sc.InTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.InTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.InTime , 108)
                                                    END

                                                    , ShiftOutTime = CASE
                                                    WHEN Sc.OutTime IS not NULL
                                                    THEN CONVERT(varchar(5),sc.OutTime,108)
                                                    ELSE CONVERT(VARCHAR(5), s.OutTime , 108)
                                                    END
                                                    ---STC ends
                                                    ,DATEADD(MI, - S.InTimeStartMargin, S.InTime) OfficeStartTime
                                                    ,DATEADD(MI, S.LateMargin, S.InTime) OfficeTime
                                                    ,S.InTimeStartMargin
                                                    ,S.BreakStratTime
                                                    ,S.BreakEndTime
                                                    ,DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime
                                                    ,OTStartTime = CASE 
                                                    WHEN S.IsGapInclude = 1
                                                    THEN S.OutTime
                                                    ELSE DATEADD(MI, S.OTStartTime, S.OutTime)
                                                    END
                                                    FROM dbo.EmpDateWiseShiftAssign EDSA
                                                    LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
                                                    LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
                                                    ---STC starts
                                                    LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE FromDate='" + FDate + @"' AND ToDate='" + TDate + @"') AS sc ON EDSA.ShiftSystemID = sc.ShiftDefinationID
                                                    ---STC ends
									                LEFT JOIN AttdnProcessData ap on EDSA.EmpSystemID=ap.EmpSystemID and ap.WorkDate=EDSA.WorkDate
									                Left JOIN DayType dt on dt.DayType=ap.DayStatus
		                                          WHERE EDSA.WorkDate between '" + FDate + @"' and '" + TDate + @"' AND EDSA.GroupID = '" + companyGroupId + @"' and EDSA.EmpSystemID= '" + EmpId + @"' 
                                                        AND EDSA.EmpSystemID IN (select SystemID from EmployeeInformation where PlantID='" + plantId + @"') 
                                                ) ES ON E.SystemID = ES.EmpSystemID and  ES.WorkDate=Atd.WorkDate";



                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate between '" + FDate + @"' and '" + TDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";

                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate between '" + FDate + @"' and '" + TDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate between '" + FDate + @"' and '" + TDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate between '" + FDate + @"' and '" + TDate + @"' AND GroupID = '" + companyGroupId + @"' AND EmpSystemID in (select SystemID from EmployeeInformation where PlantID='" + plantId + @"')
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }


                strSql = strSql + @"  LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = ES.DayType AND Atd.WorkDate BETWEEN pl.FromDate AND pl.ToDate and pl.PlantID=e.PlantId";

                strSql = strSql + @" WHERE (E.DOS >= '" + FDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + TDate + @"' AND E.GroupID = '" + companyGroupId + @"' 
                                    AND E.PlantID = '" + plantId + "'";






                strSql = strSql + @" ) x WHERE x.SystemID='" + EmpId + @"'     ORDER BY X.EmployeeCode ";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //====================end========================




        private void LoadDataSetFromDataGrid(ref DataGrid dgSource, out DataSet dsDest)
        {
            Type T = null;
            DataRow drLocal = null;
            try
            {
                dsDest = new DataSet();
                dsDest.Tables.Add(new DataTable("dsFromDg"));

                //Adding Column Name To DataSource
                for (int ColCount = 0; ColCount < dgSource.Columns.Count; ColCount++)
                {
                    T = dgSource.Columns[ColCount].GetType();
                    //dsDest.Tables[0].Columns.Add(((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString());
                    if (T.Name == "BoundColumn")
                    {
                        dsDest.Tables[0].Columns.Add(((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString());
                    }
                    else if (T.Name == "TemplateColumn")
                    {
                        dsDest.Tables[0].Columns.Add(((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString());
                    }
                    //
                }

                //Adding Row To DataSource
                for (int rowCount = 0; rowCount < dgSource.Items.Count; rowCount++)
                {
                    drLocal = dsDest.Tables[0].NewRow();

                    for (int ColCount = 0; ColCount < dgSource.Columns.Count; ColCount++)
                    {
                        T = dgSource.Columns[ColCount].GetType();
                        if (T.Name == "BoundColumn")
                        {
                            if ((dgSource.Items[rowCount].Cells[ColCount].Text.ToString().Trim() != "&nbsp;") && (dgSource.Items[rowCount].Cells[ColCount].Text.ToString().Trim() != ""))
                            {
                                drLocal[((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString()] = dgSource.Items[rowCount].Cells[ColCount].Text.ToString().Trim();
                            }
                            else
                            {
                                drLocal[((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString()] = DBNull.Value;
                            }
                        }
                        else if (T.Name == "TemplateColumn")
                        {
                            if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "All")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("CheckBoxSelect")).Checked.ToString().Trim();
                            }
                            if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "InTime")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtInTime")).Text.ToString().Trim();
                            }
                            if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "OutTime")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtOutTime")).Text.ToString().Trim();
                            }
                            if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "NormalOTHrInHourMinute")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtFinalOTHM")).Text.ToString().Trim();
                            }
                            if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "NormalOTHrHour")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtFinalOTHour")).Text.ToString().Trim();
                            }
                            if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "NormalOTHrMinute")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtFinalOTMinute")).Text.ToString().Trim();
                            }
                            if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "NormalOTHrInDecimal")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtFinalOTDc")).Text.ToString().Trim();
                            }
                        }
                    }

                    dsDest.Tables[0].Rows.Add(drLocal);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                T = null;
                drLocal = null;
            }
        }//End Function



        public void xGetEmpCodeLoadForOTConfirmation(string sGroupID, string sPlantID, string strAttnDate, string sUnit, string sDevi, string sDept, string sSect, string sSbSe, string sEmpC, string sDeGr, string sDesi, string sEmpSysID, string sFixShift, string sOTValCons, bool IsPunchBasedOT, bool IsPreallocationBasedOT, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {

                strSql = @"SELECT [CheckBoxSelect] = Convert(bit, 'True'), 
								  E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ, ES.ShiftSystemID, ES.ShiftName, ES.ShiftInTime,
                                  ES.ShiftOutTime, De.UserName DepartmentName, EC.UserName EmpCategoryName, Dsg.UserName DesignationName, ISNULL(Atd.IsManualInTime, 0) IsManualInTime, ISNULL(Atd.InTime, '00:00') InTime, 
                                  ISNULL(Atd.IsManualOutTime, 0) IsManualOutTime, ISNULL(Atd.OutTime, '00:00') OutTime, Atd.DayStatus, --ISNULL(Atd.OTHr, 0) OTHr, 
                                  
                                  OTPreallocationHour = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  OTPreallocationMinute = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  OTPreallocation = CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  OTPreallocationDecimal = CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)),
                                  
								  DeviceOTHrHour = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)), 
                                  DeviceOTHrMinute = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
                                  DeviceOTHr = CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)), 
								  DeviceOTHrInDecimal = CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2)), ";

                if (sOTValCons == "Which Ever is Less")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) < ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }
                else if (sOTValCons == "Which Ever is More")
                {
                    strSql = strSql + @"                 
                                  NormalOTHrHour = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) END, 
                                  NormalOTHrMinute = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
                                  NormalOTHr = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(Atd.OTHr, 0) AS INTEGER) % 60) AS VARCHAR(2))
														ELSE CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) / 60) AS VARCHAR(2)) + ':' + CAST((CAST(ISNULL(POT.PreallocatedOTHr, 0) AS INTEGER) % 60) AS VARCHAR(2)) END, 
								  NormalOTHrInDecimal = CASE WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) != 0 AND (ISNULL(Atd.OTHr, 0) > ISNULL(POT.PreallocatedOTHr, 0)) THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
								  					    WHEN ISNULL(Atd.OTHr, 0) != 0 AND ISNULL(POT.PreallocatedOTHr, 0) = 0  THEN CAST(ISNULL(Atd.OTHr, 0) / 60 AS DECIMAL(10, 2))
														ELSE CAST(ISNULL(POT.PreallocatedOTHr, 0) / 60 AS DECIMAL(10, 2)) END, ISNULL(Atd.IsLock, 0) IsLock, ";
                }

                strSql = strSql + @"				  
								  ES.ShiftType, ES.DayType
                                 ,IsOTEntitle=case when isnull(IsOT.IsOTEntitle,0)=0 then ISNULL(dcot.IsOTEntitle, 0)
								                                  else ISNULL(EmOT.IsOTEntitle, 0) end 
                                ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
                            FROM EmployeeInformation AS E  
                                    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
				                    LEFT OUTER JOIN 
							                    ORG.Unit AS U ON U.Id= EN.UnitID 
				                    LEFT OUTER JOIN 
							                    ORG.Division AS Dv ON Dv.Id= PR.DivisionID 
				                    LEFT OUTER JOIN 
							                    ORG.Department AS De ON De.Id = PR.DepartmentID 
							        LEFT OUTER JOIN 
							                    HKP.DesignationGroup AS DsgGr ON DM.DesignationGroupID =  DsgGr.ID
				                    LEFT OUTER JOIN 
							                    ORG.Section AS Se ON Se.Id= PR.SectionID 
				                    LEFT OUTER JOIN 
							                    ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID ";
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(OTHr, 0) OTHr, 
                                                            IsLock  ,ISNULL(OTIntime, 0) OTIntime ,ISNULL(OTOuttime, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EmpSystemID, WorkDate, DayStatus, Convert(varchar(5), InTime, 108) InTime, IsManualInTime, 
															Convert(varchar(5), OutTime, 108) OutTime, IsManualOutTime, ISNULL(0, 0) OTHr, 
                                                            IsLock  ,ISNULL(0, 0) OTIntime ,ISNULL(0, 0) OTOuttime 
	                                               FROM dbo.AttdnProcessData
                                                  WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' 
                                                        --AND IsOTComfirm = 0 
                                                        AND DayStatus IN ('P','L','WP','WL','HP','HL')   
                                                ) AS Atd ON E.SystemID = Atd.EmpSystemID ";
                }
                strSql = strSql + @" INNER JOIN 
                                                (
                                                  SELECT EDSA.EmpSystemID, EDSA.ShiftSystemID, ESA.IsFix, ESA.FixSystemID, ESA.IsRoster, ESA.RosterSystemID, EDSA.DayType, S.ShiftDefinationName ShiftName, S.ShiftType, 
														CONVERT(VARCHAR(5), S.InTime, 108) ShiftInTime, CONVERT(VARCHAR(5), S.OutTime, 108) ShiftOutTime, 
				                                        DATEADD(MI, -S.InTimeStartMargin, S.InTime) OfficeStartTime, DATEADD(MI, S.LateMargin, S.InTime) OfficeTime, 
														S.InTimeStartMargin, S.BreakStratTime, S.BreakEndTime, DATEADD(MI, S.OutTimeEndMargin, S.OutTime) OfficeEndTime, 
                                                        OTStartTime = CASE WHEN S.IsGapInclude = 1 THEN S.OutTime
											                            ELSE DATEADD(MI, S.OTStartTime, S.OutTime) END
		                                          FROM dbo.EmpDateWiseShiftAssign EDSA
														LEFT JOIN dbo.EmployeeShiftAssign ESA ON EDSA.EmpSftAssiSystemID = ESA.SystemID
				                                        LEFT JOIN dbo.ShiftDefination S ON EDSA.ShiftSystemID = S.SystemID
		                                          WHERE EDSA.WorkDate = '" + strAttnDate + @"' AND EDSA.GroupID = '" + sGroupID + @"' 
                                                        AND EDSA.PlantID = '" + sPlantID + @"'
                                                ) ES ON E.SystemID = ES.EmpSystemID
                                        ----------OT
												 left JOIN
                                                (
											      SELECT IsOTEntitle,EmpSystemID FROM dbo.EmployeeOTEntitle																                   
										        ) IsOT ON E.SystemID = IsOT.EmpSystemID

                                    ---OT entitle as per individual tagging
                                    left JOIN
                                                (
											      SELECT IsOTEntitle,EmpSystemID FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 1 
																                   
										        ) EmOT ON E.SystemID = EmOT.EmpSystemID
									       ---OT entitle as per designation
									 left JOIN  (
														SELECT DC.IsOTEntitled IsOTEntitle,Dm.DesignationId,PlantId
																			FROM MST.DesignationMaster DM
                                                        LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
													) DCOT ON DCOT.DesignationId = E.GivenDesignationId AND DCOT.PlantId=E.PlantId

									LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM FinalOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS FOT ON E.SystemID = FOT.EmpSystemID ";

                if (IsPunchBasedOT == true && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == true && IsPreallocationBasedOT == true)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT * FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }
                if (IsPunchBasedOT == false && IsPreallocationBasedOT == false)
                {
                    strSql = strSql + @" LEFT OUTER JOIN 
							                    (
							                     SELECT 0 PreallocatedOTHr,EmpSystemID FROM PreallocatedOT
													    WHERE WorkDate = '" + strAttnDate + @"' AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'
											    ) AS POT ON E.SystemID = POT.EmpSystemID ";
                }




                strSql = strSql + @" WHERE (E.DOS >= '" + strAttnDate + @"' OR E.DOS IS NULL) AND E.DOJ <= '" + strAttnDate + @"' AND E.GroupID = '" + sGroupID + @"' 
                                    AND E.PlantID = '" + sPlantID + @"'
                                     and 
									(
									(	isnull(IsOT.IsOTEntitle,0)=1 and isnull(EmOT.IsOTEntitle,0)=1)
                                    or (isnull(IsOT.IsOTEntitle,0)=0 and isnull(DCOT.IsOTEntitle,0)=1)                                   
									)
                                    and e.SystemId not in (
									 SELECT EmpSystemID FROM dbo.EmployeeOTEntitle 
												  	    WHERE '" + strAttnDate + @"' BETWEEN ISNULL(OTStartDate, GETDATE()) AND ISNULL(OTEndDate, GETDATE())
                                                                AND ISNULL(IsOTEntitle, 0) = 0
									                        )";





                strSql = strSql + @"
                        ORDER BY EmployeeCode";



                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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













        private IEnumerable<AccessControllerEmployeeTag> GetTaglist(string EmpInfoSystemIDs)//TBT
        {
            try
            {
                var _sql = "SELECT * FROM AccessControllerEmployeeTag WHERE EmpInfoSystemID in (" + EmpInfoSystemIDs + ")";
                return _sqlRepository.GetModelCollection<AccessControllerEmployeeTag>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GetPks(List<AccessControllerEmployeeTag> from_ui)
        {
            var _r = "''";
            try
            {
                var builder = new System.Text.StringBuilder();
                // builder.Append(_r);
                foreach (var item in from_ui)
                {
                    if (_r == "''")
                    {
                        _r = "'" + item.EmpInfoSystemID + "'";
                        builder.Append("'" + item.EmpInfoSystemID + "'");
                    }
                    else
                    {
                        builder.Append(",'" + item.EmpInfoSystemID + "'");
                    }
                }
                _r = builder.ToString();
                return _r;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitData(List<AccessControllerEmployeeTag> from_ui, out List<AccessControllerEmployeeTag> from_db, out List<AccessControllerDeleteRequest> del_list)
        {
            del_list = new List<AccessControllerDeleteRequest>();

            from_db = null;
            try
            {
                var _pks = GetPks(from_ui);
                from_db = GetTaglist(_pks).ToList<AccessControllerEmployeeTag>();

                //foreach (var db in from_db)
                //{
                //    var ui = from_ui.FirstOrDefault(a => a.EmpInfoSystemID == db.EmpInfoSystemID && a.DeviceSystemID == db.DeviceSystemID);
                //    if (ui == null)
                //    {
                //        // _d.InitData(db, ref del_list);
                //        //if(db.DeviceSystemID==)
                //        db.ModelState = ModelState.Deleted;
                //    }
                //}

                foreach (var ui in from_ui)
                {
                    var db = from_db.FirstOrDefault(a => a.EmpInfoSystemID == ui.EmpInfoSystemID && a.DeviceSystemID == ui.DeviceSystemID);
                    if (db == null)
                    {
                        // db = new AccessControllerEmployeeTag();
                    }
                    else
                    {
                        db.RegisterStatus = "Registered";
                        //db.DateAdded = DateTime.Now;
                        db.UpdatedDate = DateTime.Now; ;
                        //db.AddedBy = identity.UserId;
                        db.UpdatedBy = "schedule";
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveList(List<AccessControllerEmployeeTag> fromui)
        {
            List<AccessControllerDeleteRequest> del_list = null;

            List<AccessControllerEmployeeTag> from_db = null;
            var flag = false;
            try
            {
                InitData(fromui, out from_db, out del_list);
                foreach (var item in from_db)
                {
                    base.InsertOrUpdateGraph(item);
                }

                foreach (var item in del_list)
                {
                    // _d.Insert(item);
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public void DeleteAndUpdateList(List<AccessControllerEmployeeTagDelete> fromui)
        {
            List<AccessControllerDeleteRequest> del_list = null;

            List<AccessControllerEmployeeTag> from_db = null;
            var flag = false;
            try
            {
                //InitData(fromui, out from_db, out del_list);
                foreach (var item in from_db)
                {
                    base.InsertOrUpdateGraph(item);
                }

                foreach (var item in del_list)
                {
                    // _d.Insert(item);
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel GetAllEmployee(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EMP.*,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName
        					 ,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection,P.UserName Plant
        					 FROM EmployeeInformation EMP
        					 LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
        					 LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
        					 LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
        					 LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
        					 LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
        					 LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                             LEFT JOIN ORG.Plant P ON P.Id=EMP.PlantId
        					 LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
        					 LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
        					 WHERE EMP.EmployeeStatus='Active' AND EMP.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmployeeRelatedDevices(string systemId)
        {
            try
            {
                var sql = @"SELECT ACE.Id
		                                    ,CASE ISNULL(ACE.Id,'') when '' then CAST('False' as bit)
		                                    else CAST('TRUE' as bit) end Flag, ACL.Id DeviceSystemID
                                            ,ACL.MachineID
                                            ,ACL.MachineIP
                                            ,ACL.Description
		                                    ,(select RegisterFP from dbo.EmployeeInformation where systemid='" + systemId + @"') RegisterFP
                                            ,(select RegisterProximate from dbo.EmployeeInformation where systemid='" + systemId + @"') RegisterProximate
                                            FROM MST.AccessControllerList ACL
                                            LEFT OUTER JOIN(Select ACT.* from dbo.AccessControllerEmployeeTag  ACT
		                                    WHERE ACT.EmpInfoSystemID = '" + systemId + "') ACE ON ACL.Id = ACE.DeviceSystemID";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetEmployeeDevicesList(string deviceId)
        {
            try
            {
                var sql = @"SELECT E.*,ACL.MachineID
                                            ,ACL.MachineIP
                                            ,ACL.Description 
											 ,EMP.RegisterFP 
                                            ,EMP.RegisterProximate
                                            ,EMP.EmployeeCode
											,EMP.EmployeeName
											,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection,P.UserName Plant
											FROM [dbo].[AccessControllerEmployeeTag] E
							  LEFT JOIN MST.AccessControllerList ACL ON ACL.Id=E.DeviceSystemID
							  LEFT JOIN EmployeeInformation EMP ON EMP.SystemId=E.EmpInfoSystemID
LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
							  LEFT JOIN ORG.Department DEPT ON DEPT.Id=PR.DepartmentId
        					  LEFT JOIN HKP.Designation DEG ON DEG.Id=EMP.GivenDesignationId
        					  LEFT JOIN ORG.Plant P ON P.Id=EMP.PlantId
        					  LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
        					  LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
							  WHERE E.DeviceSystemID='" + deviceId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertOrUpdateGraph(IEnumerable<AccessControllerEmployeeTag> uilist, string empId, bool registerProximate, bool registerFP)
        {
            var flag = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var dbList = Query(r => r.EmpInfoSystemID == empId).Select().ToList();
                if (dbList == null)
                    dbList = new List<AccessControllerEmployeeTag>();

                var delReqList = new List<AccessControllerDeleteRequest>();

                foreach (var item in dbList)
                {
                    AccessControllerEmployeeTag db = null;
                    if (uilist != null)
                    {
                        db = uilist.FirstOrDefault(a => a.Id == item.Id);
                    }

                    if (db == null)
                    {
                        _d.InitData(item, ref delReqList);

                        item.ModelState = ModelState.Deleted;
                        AuditService.Log(item);
                    }
                }

                var pk = GetAutoNumber(nameof(AccessControllerEmployeeTag), PKGeneratorEnum.Auto, null, DateTime.Now);

                if (uilist != null)
                {
                    var count = 0;
                    foreach (var AccessControllerEmployeeTags in uilist)
                    {
                        count++;
                        var Db_list = dbList.FirstOrDefault(r => r.Id == AccessControllerEmployeeTags.Id);
                        if (Db_list == null || string.IsNullOrEmpty(Db_list.Id))
                        {
                            Db_list = new AccessControllerEmployeeTag
                            {
                                Id = "EAC" + pk + "-" + count,
                                EmpInfoSystemID = AccessControllerEmployeeTags.EmpInfoSystemID,
                                DeviceSystemID = AccessControllerEmployeeTags.DeviceSystemID,
                                RegisterStatus = "Requested",
                                GroupID = identity.CompanyGroupId,
                                PlantID = AccessControllerEmployeeTags.PlantID,
                                ModelState = ModelState.Added
                            };
                            AuditService.AddedLog(Db_list);
                            dbList.Add(Db_list);
                        }
                        else
                        {
                            Db_list.EmpInfoSystemID = AccessControllerEmployeeTags.EmpInfoSystemID;
                            Db_list.DeviceSystemID = AccessControllerEmployeeTags.DeviceSystemID;
                            Db_list.RegisterStatus = "Requested";
                            Db_list.GroupID = identity.CompanyGroupId;
                            Db_list.PlantID = AccessControllerEmployeeTags.PlantID;
                            Db_list.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(Db_list);
                        }
                    }
                }

                foreach (var item in dbList)
                {
                    base.InsertOrUpdateGraph(item);
                }

                var empdata = _employeeInformationService.Find(empId);
                empdata.RegisterProximate = registerProximate;
                empdata.RegisterFP = registerFP;
                _employeeInformationService.Update(empdata);

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void InsertOrUpdateEmployeeDevice(IEnumerable<AccessControllerEmployeeTag> uilist, bool registerProximate, bool registerFP, string deviceId)
        {
            var flag = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var pk = GetAutoNumber(nameof(AccessControllerEmployeeTag), PKGeneratorEnum.Auto, null, DateTime.Now);

                if (uilist != null)
                {
                    var count = 0;
                    foreach (var item in uilist)
                    {
                        var dbDevice = base.Query(t => t.EmpInfoSystemID == item.EmpInfoSystemID && t.DeviceSystemID == deviceId).Select().FirstOrDefault();
                        if (dbDevice == null)
                        {
                            count++;
                            if (item == null || string.IsNullOrEmpty(item.Id))
                            {
                                item.Id = "EAC" + pk + "-" + count;
                                item.RegisterStatus = "Requested";
                                item.DeviceSystemID = deviceId;
                                item.GroupID = identity.CompanyGroupId;
                                item.PlantID = identity.PlantId;
                                item.ModelState = ModelState.Added;

                                AuditService.AddedLog(item);
                                Insert(item);
                            }
                            else
                            {
                                item.ModelState = ModelState.Modified;
                                AuditService.UpdatedLog(item);
                                Update(item);
                            }
                            var empdata = _employeeInformationService.Find(item.EmpInfoSystemID);
                            empdata.RegisterProximate = registerProximate;
                            empdata.RegisterFP = registerFP;
                            _employeeInformationService.Update(empdata);
                        }
                    }
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }




        public void GetOTSlabDefineGeneral(string sGroupID, string sPlantID, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.OTSlabDefineGeneral
                           WHERE '" + sAttnDate + @"' BETWEEN FromDate AND ToDate AND GroupID = '" + sGroupID + @"' 
                                 AND PlantID = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetAttdnLock(string sGroupID, string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = "SELECT * FROM AttdnLock WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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



    public class OTOTConfirmation
    {
        public bool CheckBoxSelect { get; set; }
        public string SystemID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DOJ { get; set; }
        public string ShiftSystemID { get; set; }
        public string ShiftName { get; set; }
        public string ShiftInTime { get; set; }
        public string ShiftOutTime { get; set; }
        public string DepartmentName { get; set; }
        public string EmpCategoryName { get; set; }
        public string DesignationName { get; set; }
        public bool IsManualInTime { get; set; }
        public string InTime { get; set; }
        public bool IsManualOutTime { get; set; }
        public string OutTime { get; set; }
        public string DayStatus { get; set; }
        public string OTPreallocationHour { get; set; }
        public string OTPreallocationMinute { get; set; }
        public string OTPreallocation { get; set; }
        public string OTPreallocationDecimal { get; set; }
        public string DeviceOTHrHour { get; set; }
        public string DeviceOTHrMinute { get; set; }
        public string DeviceOTHr { get; set; }
        public string DeviceOTHrInDecimal { get; set; }
        public string NormalOTHrHour { get; set; }
        public string NormalOTHrMinute { get; set; }
        public string NormalOTHr { get; set; }
        public string NormalOTHrInDecimal { get; set; }
        public string ShiftType { get; set; }
        public string DayType { get; set; }
        public bool IsOTEntitle { get; set; }
        public string OTIntime { get; set; }
        public string OTOuttime { get; set; }
        public string ExtraOT { get; set; }
        // New add
        //public string WorkDate { get; set; }

    }
}
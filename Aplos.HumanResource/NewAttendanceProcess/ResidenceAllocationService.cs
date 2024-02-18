#region lib
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;
using Library.Data;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Reflection;
#endregion lib

namespace Library.HumanResource.NewAttendanceProcess
{
    #region Residence Master
    public class ResidenceAllocationService
    {
        SqlRepository _sqlRepository;
        public ResidenceAllocationService()
        {

            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetResidenceMaster()
        {
            try
            {
                string sql = @"select rm.*, p.UserName as Plant, eg.UserName as EmployeeCategory, rg.UserName as ResidenceGroup from dbo.ResidenceMaster rm
left join ORG.Plant p on p.Id = rm.PlantId
left join dbo.ResidenceGroup rg on rg.Id = rm.ResidenceGroupId
left join hkp.EmployeeCategory eg on eg.Id = rm.EmployeeCategoryId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPlant()
        {
            try
            {
                string sql = @"select Id as Value, UserName as Text from ORG.Plant";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getResidenceGroup()
        {
            try
            {
                string sql = @"select Id as Value, UserName as Text from dbo.ResidenceGroup where Active = 1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmployeeCategory()
        {
            try
            {
                string sql = @"select Id as value, UserName as Text from hkp.EmployeeCategory";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmpServiceType()
        {
            try
            {
                string sql = @"select * from dbo.EmpServiceType";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> Save(Dictionary<string, object> data, string PlantId, string ResidenceGroupId, string Emp, string ServiceTypeId)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "dbo.ResidenceMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (data["PlantId"] == null)
                {
                    throw new Exception("Please Select Plant Id !!");
                }
                if (data["ResidenceGroupId"] == null)
                {
                    throw new Exception("Please SelectResidenceGroup Id!!");
                }
                // Unique User Validation
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ResidenceNumber='" + data["ResidenceNumber"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                    #region data Master update
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "RM" + _Id;
                    //data["PlantId"] = PlantId;
                    //data["ResidenceGroupId"] = ResidenceGroupId;
                    //data["EmployeeCategoryId"] = Emp;
                    //data["EmpServiceTypeId"] = ServiceTypeId;
                    AddNewRow(dsMaster.Tables[0], data);


                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["PlantId"] = PlantId;
                    data["ResidenceGroupId"] = ResidenceGroupId;
                    data["EmployeeCategoryId"] = Emp;
                    data["EmpServiceTypeId"] = ServiceTypeId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data Master update





                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Add & Edit Row
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

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion Add & Edit Row

        #region TAB POSITION
        public IEnumerable<object> getEntity()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select E.UserName Text, E.Id Value from ORG.Entity E
                               where E.PlantId = '" + identity.PlantId + "' and E.Active = 1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getBudgetCode(string entityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select distinct Code, Id Value from MST.ManpowerBudget
                                where EntityId = '"+ entityId + "' and Active = 1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPositionCode(string MPBudgetId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select distinct P.UserName Text, P.Id Value from ORG.Position P
                               left join MST.ManpowerBudget MB on MB.PositionId = P.Id
                               where MB.Id = '"+ MPBudgetId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPositionTabGridData()
        {
            try
            {
                string sql = @"Select P.Code PositionCode, MP.Code BudgetCode from MST.ManpowerBudget MP
                            left join ORG.Position P on P.Id = MP.PositionId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion TAB POSITION
    }
    #endregion Residence Master

    #region Residence Status Allocation
    public class ResidenceStausAllocationService
    {
        SqlRepository _sqlRepository;

        #region constructor
        public ResidenceStausAllocationService()
        {

            _sqlRepository = new SqlRepository();
        }
        #endregion constructor

        #region Actions
        public IEnumerable<object> getData()
        {
            try
            {
                var _sql = @"select * from dbo.ResidenceStatusLocation";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> getAllEmployee(string EmpCategoryId)
        {
            try
            {
                var str = @"select ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section,
                            SBC.UserName as SubSection from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                            left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                            left join SalaryRuleMaster SRM on srm.systemid = ei.salaryrulemastersystemid
                            left join ResidenceGroup RG on RG.Id = ei.ResidenceGroupId
                            left join TransportGroup TG on TG.Id = ei.TransportGroupId          
                            where x.Id = '" + EmpCategoryId + "' and ei.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmployeeCategory()
        {
            try
            {
                string sql = @"select eg.* from hkp.EmployeeCategory eg";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetViewData(Dictionary<string, string> parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"select RM.Id ResidenceMasterId,RG.Id ResidenceGroupId,RG.UserName ResidenceGroup,P.Id PlantId,P.UserName Plant,RM.[Location],EC.Id EmployeeTypeId
                                    ,EC.UserName EmployeeType,EST.[Service] ServiceType,RM.Rooms,RM.[Block],RM.ResidenceSubCategory,RM.[Floor],RM.ResidentType
									,RM.ResidenceNumber,RM.AssetName,RM.Remarks,RM.AddedBy,format(RM.AddedDate,'dd-MMM-yyyy')AddedDate
								    ,isnull(RM.Vacancy,0) Vacancy,isnull(O.Occupied,0) Occupied,Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0)
									
                                    from ResidenceMaster RM
									left join ResidenceGroup RG on RG.Id=RM.ResidenceGroupId 
									left join ORG.Plant P on P.Id=RM.PlantId
									left join HKP.EmployeeCategory EC on EC.Id=RM.EmployeeCategoryId
									left join EmpServiceType EST on EST.Id=RM.EmpServiceTypeId
                                   
									LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    ";


                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetRSAFiltersViewData(Dictionary<string, string> parameters)
        {
            try
            {
                var _sql = @"select ei.SystemId EmployeeId,DEG.UserName Designation,ei.EmployeeName,S.UserName Section,SS.UserName SubSection,D.UserName Department
                            ,RG.UserName ResidenceGroup,RM.Id ResidenceId,RM.ResidenceNumber,RM.[Block],RM.ResidentType,RM.ResidenceSubCategory
							
							from dbo.ResidenceAllocatedEmployees rae
                            left join ResidenceMaster RM on RM.Id=RAE.ResidenceId 
											left join ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                                            left join EmployeeInformation EI on EI.SystemId=RAE.EmployeeSystemId
                                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                            --left join HKP.Designation DE on DE.Id=EI.DesignationSystemID
											--left join MST.DesignationMaster DM on DM.DesignationId = DM.Id
                                            left join ORG.Department D on D.Id=EI.DepartmentId
                                            left join ORG.Section S on S.Id=EI.SectionId
                                            left join ORG.SubSection SS on SS.Id=EI.SubSectionId
                                            left join ORG.Line L on L.Id=EI.LineId 
											LEFT JOIN HKP.Designation DEG ON DEG.Id =  EI.GivenDesignationId
											LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = DEG.Id
											left join HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                                
                            where ei.SystemId in(" + parameters["EmployeeId"] + @") AND RAE.isOccupied = 1 and PR.ID <> '989' 
";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> getemployeeDataList(string plantId, string residenceGroupId, string EmployeeTypeId)
        {
            try
            {
                var Today = DateTime.Now;
                string FirstDayOfTheMonth = "01-" + Convert.ToDateTime(Today).ToString("MMM") + "-" + Convert.ToDateTime(Today).ToString("yyyy");
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                string CmdText = @"SELECT isSelected=(CAST(0 as bit)), Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, RG.UserName ResidenceGroup, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                        ,RM.Location, RM.ResidenceCategory, EMP.GenderID
										FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
										LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId
										LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
										
                              Where EMP.PlantId ='" + plantId + @"' AND EMP.EmployeeStatus='Active' AND EMP.SystemId 
							  NOT IN(Select EmployeeSystemId from dbo.ResidenceAllocatedEmployees  Where isOccupied = 1) 
								
							  AND RG.IsResidenceApplicable = 'true' and EC.Id = '" + EmployeeTypeId + @"'
                              ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        //Route Employee start
        

        

        public IEnumerable<object> getviewUnassign(string plantId)
        {
            try
            {
                string CmdText = @"select EI.SystemId EmployeeId,EI.EmployeeStatus,EI.EmployeeCurrentStatus,format(EI.DOJ,'dd-MMM-yyyy') DOJ,EI.DOS,R.StandardName [Route]
							                    ,TD.TransportUserName Transport,SD.UserName [Shift],R.[From],R.[To],RS.Id TripId,RS.TripNo,PR.PaymentLink Skill
							                    ,DEG.UserName GivenDesignation,S.UserName Section,SS.UserName SubSection,DEPT.UserName Department,E.UserName Entity,PL.UserName Plant
												,ST.Id StoppageId,ST.UserName Stoppage,ETA.AssignStatus,ETA.UnassignDate,ETA.AssignDate

							                    from EmployeeTransportAllocation ETA
							                    left join EmployeeInformation EI on EI.SystemId = ETA.EmployeeSystemId
							                    LEFT JOIN MST.ManpowerBudget PMB ON PMB.Id=EI.BudgetCode
							                    LEFT JOIN ORG.Position PR ON PR.Id=PMB.PositionId
							                    LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							                    LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id
							                    LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
							                    LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
							                    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
							                    LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
							                    left join RouteSchedule RS on RS.Id = ETA.TripId
							                    left join MST.Route R on R.Id = RS.RouteId
							                    left join TransportDetail TD on TD.Id = RS.TransportId
							                    left join ShiftDefination SD on SD.SystemID=RS.ShiftId
							                    left join HKP.Stoppage ST on ST.Id=ETA.StoppageId
                                
                                Where EI.PlantId='" + plantId + @"' and ETA.AssignStatus = 1 order by  EI.EmployeeStatus desc, case when EI.EmployeeCurrentStatus is not null then 0 else 1 end, EmployeeCurrentStatus";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void SaveUnassignData(List<Dictionary<string, object>> employeeList)
        {
            try
            {
                var id = "";
                foreach (var item in employeeList)
                {
                    if (id == "")
                        id = "'" + item["EmployeeId"] + "'";
                    else
                        id = id + ",'" + item["EmployeeId"] + "'";
                }

                //Master Table - PMSMaster
                string TableName = "dbo.EmployeeTransportAllocation";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where EmployeeSystemId In (" + id + ")", out dsMaster, false, "1");

                //string _Id = "";

                #region data Master update

                foreach (var item in employeeList)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "EmployeeSystemId='" + item["EmployeeId"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;
                        item["AssignStatus"] = 0;
                        item["UnassignDate"] = DateTime.Now;
                        EditRow(drmo, item);
                    }

                }
                #endregion data Master update

                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                //return ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Route Employee End

        public IEnumerable<object> getOccupiedemployeeDataList(string plantId, string residenceNumber)
        {
            try
            {
                var Today = DateTime.Now;
                string FirstDayOfTheMonth = "01-" + Convert.ToDateTime(Today).ToString("MMM") + "-" + Convert.ToDateTime(Today).ToString("yyyy");
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                string CmdText = @"select RAE.Id,EI.EmployeeCode,EI.SystemId EmployeeId,EI.EmployeeName,D.UserName Department,DEG.UserName Designation
                                            ,S.UserName Section,SS.UserName SubSection,L.UserName Line,format(EI.DOJ,'dd-MMM-yyyy') DOJ
                                            ,RM.AssetName ResidenceName,RAE.isOccupied, FORMAT(EI.DOS, 'dd-MMM-yyyy')DOS, EI.EmployeeStatus,
											EI.EmployeeCurrentStatus, RG.UserName ResidenceGroup, [RM].[Block], RM.ResidentType, 
											RM.ResidenceNumber, EI.DOS, DEG.UserName GivenDesignation, PR.PaymentLink Skill ,
                                            RM.Location, EC.UserName EmployeeCategory
											
                                            from ResidenceAllocatedEmployees RAE
                                            left join ResidenceMaster RM on RM.Id=RAE.ResidenceId 
											left join ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                                            left join EmployeeInformation EI on EI.SystemId=RAE.EmployeeSystemId
                                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                            --left join HKP.Designation DE on DE.Id=EI.DesignationSystemID
											--left join MST.DesignationMaster DM on DM.DesignationId = DM.Id
                                            left join ORG.Department D on D.Id=EI.DepartmentId
                                            left join ORG.Section S on S.Id=EI.SectionId
                                            left join ORG.SubSection SS on SS.Id=EI.SubSectionId
                                            left join ORG.Line L on L.Id=EI.LineId 
											LEFT JOIN HKP.Designation DEG ON DEG.Id =  EI.GivenDesignationId
											LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = DEG.Id
											left join HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                                
                                Where EI.PlantId='" + plantId + @"' and rae.isOccupied=1 and RM.ResidenceNumber = '" + residenceNumber + @"' order by  EI.EmployeeStatus desc, case when EI.EmployeeCurrentStatus is not null then 0 else 1 end, EmployeeCurrentStatus
                               -- AND EI.EmployeeStatus='Active'";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> getResidence()
        {
            try
            {
                var str = @"select Id Value, UserName Text from dbo.ResidenceGroup";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getviewUnallocation(string plantId)
        {
            try
            {
                string CmdText = @"select RAE.Id,EI.EmployeeCode,EI.SystemId EmployeeId,EI.EmployeeName,D.UserName Department,DEG.UserName Designation
                                            ,S.UserName Section,SS.UserName SubSection,L.UserName Line,format(EI.DOJ,'dd-MMM-yyyy') DOJ
                                            ,RM.AssetName ResidenceName,RAE.isOccupied, FORMAT(EI.DOS, 'dd-MMM-yyyy')DOS, EI.EmployeeStatus,
											EI.EmployeeCurrentStatus, RG.UserName ResidenceGroup, [RM].[Block], RM.ResidentType, 
											RM.ResidenceNumber, EI.DOS, DEG.UserName GivenDesignation, PR.PaymentLink Skill --, EC.UserName EmployeeCategory
                                            ,EC.UserName EmployeeCategory, RM.Location
                                            from ResidenceAllocatedEmployees RAE
                                            left join ResidenceMaster RM on RM.Id=RAE.ResidenceId 
											left join ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                                            left join EmployeeInformation EI on EI.SystemId=RAE.EmployeeSystemId
                                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                            left join HKP.Designation DE on DE.Id=EI.DesignationSystemID
											
                                            left join ORG.Department D on D.Id=EI.DepartmentId
                                            left join ORG.Section S on S.Id=EI.SectionId
                                            left join ORG.SubSection SS on SS.Id=EI.SubSectionId
                                            left join ORG.Line L on L.Id=EI.LineId 
											LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id
											left join MST.DesignationMaster DM on DM.DesignationId = DEG.Id
											LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                                
                                
                                Where EI.PlantId='" + plantId + @"' and  rae.isOccupied=1 order by  
                                case 
								when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
								when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
								when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
								when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
							
								else
								5
								 end ASC
";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> PopupEmployeeView(string fromDate, string toDate, string EmployeeCategorySystemID)
        {
            try
            {
                var str = @"select ei.SystemId, LDSG.UserName as Designation, POS.Activity, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section,
                            SBC.UserName as SubSection from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                            left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                            left join SalaryRuleMaster SRM on srm.systemid = ei.salaryrulemastersystemid
                            left join ResidenceGroup RG on RG.Id = ei.ResidenceGroupId
                            left join TransportGroup TG on TG.Id = ei.TransportGroupId          
                            where ei.DOJ BETWEEN '" + fromDate + "' and '" + toDate + "' and ei.EmployeeCategorySystemID = '" + EmployeeCategorySystemID + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Save(List<Dictionary<string, object>> EmployeeList)
        {

            try
            {
                if (EmployeeList!=null)
                {
                    //Master Table - PMSMaster
                    string TableName = "dbo.ResidenceAllocatedEmployees";
                    DataSet dsMaster = null;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");



                    string _Id = "";

                    #region data Master update
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    int count = 0;
                    foreach (var item in EmployeeList)
                    {
                        con.OpenDataSetThroughAdapter("select * from " + TableName + " where EmployeeSystemId='" + item["EmployeeSystemId"] + "'", out dsMaster, false, "1");
                        count++;
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "EmployeeSystemId='" + item["EmployeeSystemId"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + count;
                            item["Date"] = DateTime.Now;
                            item["isOccupied"] = 1;
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            item["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                            item["isOccupied"] = 1;
                            EditRow(drmo, item);
                        }
                    }
                    #endregion data Master update

                    OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                    obj.SaveDataSets(dsMaster);

                }
                //return ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveRSUnallocation(List<Dictionary<string, object>> employeeList)
        {

            try
            {
                if (employeeList!=null)
                {
                    var id = "";
                    foreach (var item in employeeList)
                    {
                        if (id == "")
                            id = "'" + item["Id"] + "'";
                        else
                            id = id + ",'" + item["Id"] + "'";
                    }

                    //Master Table - PMSMaster
                    string TableName = "dbo.ResidenceAllocatedEmployees";
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id In (" + id + ")", out dsMaster, false, "1");

                    string _Id = "";

                    #region data Master update

                    foreach (var item in employeeList)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            item["isOccupied"] = 0;
                            EditRow(drmo, item);
                        }

                    }
                    #endregion data Master update

                    OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                    obj.SaveDataSets(dsMaster); 
                }

                //return ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       

        #region Add & Edit Row
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
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion Add & Edit Row

        public IEnumerable<object> getEmployee(string PlantId, string ResidenceGroupId, string EmployeeCategoryId)
        {
            try
            {
                var str = @"select ei.EmployeeName, ei.DOJ, ei.EmployeeStatus, ei.SystemId, rm.Id ,rm.AddedDate as AllocationDate from dbo.ResidenceMaster rm                           
                            left join HKP.EmployeeCategory eg on eg.Id = rm.EmployeeCategoryId
                            left join dbo.EmployeeInformation ei on ei.EmployeeCategorySystemID = eg.Id
                            where rm.PlantId='" + PlantId + "' and rm.ResidenceGroupId='" + ResidenceGroupId + "'  and rm.EmployeeCategoryId = '" + EmployeeCategoryId + "' and ei.EmployeeStatus = 'Active'";

                ;
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getResidenceStatusLocation(string EmployeeId, string ResidenceMasterId)
        {
            try
            {
                var str = @"select ei.EmployeeName, FORMAT (rae.AddedDate, 'dd-MMM-yyyy') as Date ,rm.AssetName 
                            from dbo.EmployeeInformation ei
                            left join dbo.ResidenceAllocatedEmployees rae on rae.EmployeeSystemId = ei.SystemId
                            left join dbo.ResidenceMaster rm on rm.Id = rae.ResidenceId
                            where ei.SystemId='" + EmployeeId + "' and rm.Id = '" + ResidenceMasterId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> getSelectedEmployees(List<Dictionary<string, object>> EmpList)
        {
            try
            {

                return EmpList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #region REPORTS QUERY
        public DataTable residenceAllocationReport(Dictionary<string, string> parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"select RM.Id ResidenceMasterId,RG.Id ResidenceGroupId,RG.UserName ResidenceGroup,P.Id PlantId,P.UserName Plant,RM.[Location],EC.Id EmployeeTypeId
                                    , EC.UserName EmployeeType, EST.[Service] ServiceType,RM.Rooms,RM.[Block],RM.ResidenceSubCategory,RM.[Floor],RM.ResidentType
									,RM.ResidenceNumber,RM.AssetName,RM.Remarks,RM.AddedBy,format(RM.AddedDate, 'dd-MMM-yyyy')AddedDate
								    ,isnull(RM.Vacancy, 0) Vacancy,isnull(O.Occupied, 0) Occupied,Available = isnull(isnull(RM.Vacancy, 0) - isnull(O.Occupied, 0), 0)
                                    
                                    from ResidenceMaster RM

                                    left
                                    join ResidenceGroup RG on RG.Id = RM.ResidenceGroupId

                               left
                                    join ORG.Plant P on P.Id = RM.PlantId

                               left
                                    join HKP.EmployeeCategory EC on EC.Id = RM.EmployeeCategoryId

                               left
                                    join EmpServiceType EST on EST.Id = RM.EmpServiceTypeId
                               

                               LEFT JOIN(
                               select COUNT(A.EmployeeSystemId)Occupied, A.ResidenceId from dbo.ResidenceAllocatedEmployees A

                                 left
                                                                                       join EmployeeInformation EI on EI.SystemId = A.EmployeeSystemId
                                                  
                                                                                      Where A.isOccupied = 1 and EI.PlantId in ( " + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId = RM.Id
                                    where RM.Id in(" + parameters["ResidenceMasterId"] + @")
                                        AND RG.Id in(" + parameters["ResidenceGroupId"] + @")
                                       AND P.Id in(" + parameters["PlantId"] + @")
                                        AND EC.Id in(" + parameters["EmployeeTypeId"] + @")";


                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable allresidencemasterReport()
        {
            var sql = @"select RM.Id,ec.UserName 'Employee Category', RG.UserName 'Residence Group', RM.[Location], RM.ResidenceCategory, RM.Block
                            ,RM.Floor, RM.ResidenceNumber, RM.ResidenceSubCategory, RM.ResidentType, RM.Vacancy, EMP.EmployeeName,
                            EMP.EmployeeCode, EMP.EmployeeStatus, 
                            case when  EMP.EmployeeCurrentStatus is null then 'Regular' else EMP.EmployeeCurrentStatus end as EmployeeCurrentStatus ,
                            FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, SC.UserName 'Section', SBC.UserName 'Sub Section', 
                            LDSG.UserName 'Designation', GDSG.UserName 'Legal Designation'
                            from ResidenceMaster RM

                            left join ResidenceAllocatedEmployees RAE on RAE.ResidenceId = RM.Id
                            left join EmployeeInformation EMP on EMP.SystemId = RAE.EmployeeSystemId
                            left join ResidenceGroup RG on EMP.ResidenceGroupId = RG.Id
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                            left join hkp.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId";

            return _sqlRepository.GetDataTable(sql);
        }

        public DataTable residencemasterReport(string empCurrentStatus)
        {
            try
            {

                var sql = @"select RM.Id,ec.UserName 'Employee Category', RG.UserName 'Residence Group', RM.[Location], RM.ResidenceCategory, RM.Block
                            ,RM.Floor, RM.ResidenceNumber, RM.ResidenceSubCategory, RM.ResidentType, RM.Vacancy, EMP.EmployeeName,
                            EMP.EmployeeCode, EMP.EmployeeStatus, 
                            case when  EMP.EmployeeCurrentStatus is null then 'Regular' else EMP.EmployeeCurrentStatus end as EmployeeCurrentStatus ,
                            FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, SC.UserName 'Section', SBC.UserName 'Sub Section', 
                            LDSG.UserName 'Designation', GDSG.UserName 'Legal Designation'
                            from ResidenceMaster RM

                            left join ResidenceAllocatedEmployees RAE on RAE.ResidenceId = RM.Id
                            left join EmployeeInformation EMP on EMP.SystemId = RAE.EmployeeSystemId
                            left join ResidenceGroup RG on EMP.ResidenceGroupId = RG.Id
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                            left join hkp.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId

                            where EMP.EmployeeCurrentStatus = '" + empCurrentStatus + "'";
                return _sqlRepository.GetDataTable(sql);




            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion REPORTS QUERY

        public IEnumerable<object> employeeCurrrentStatus()
        {
            try
            {
                var sql = @"select distinct EmployeeCurrentStatus from EmployeeInformation where EmployeeCurrentStatus is not null";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> gridViewResidenceMAster()
        {
            try
            {
                var sql = @"select RM.Id,ec.UserName 'Employee Category', RG.UserName 'Residence Group', RM.[Location], RM.ResidenceCategory, RM.Block
                            ,RM.Floor, RM.ResidenceNumber, RM.ResidenceSubCategory, RM.ResidentType, RM.Vacancy, EMP.EmployeeName,
                            EMP.EmployeeCode, EMP.EmployeeStatus, 
                            case when  EMP.EmployeeCurrentStatus is null then 'Regular' else EMP.EmployeeCurrentStatus end as EmployeeCurrentStatus ,
                            FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, SC.UserName 'Section', SBC.UserName 'Sub Section', 
                            LDSG.UserName 'Designation', GDSG.UserName 'Legal Designation'
                            from ResidenceMaster RM

                            left join ResidenceAllocatedEmployees RAE on RAE.ResidenceId = RM.Id
                            left join EmployeeInformation EMP on EMP.SystemId = RAE.EmployeeSystemId
                            left join ResidenceGroup RG on EMP.ResidenceGroupId = RG.Id
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                            left join hkp.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Actions

    }
    #endregion Residence Status Allocation

    #region Residence Status Report
    public class ResudeceStatusReportService
    {
        SqlRepository _sqlRepository;
        #region constructor
        public ResudeceStatusReportService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion constructor

        #region Detail Residence Status Report
        // Detail Residence Status Report
        public DataTable detailResidenceStatusReport(string PartialVacantFullyOccupied)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "";
                if (PartialVacantFullyOccupied == "FullyOccupied")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName, 
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup
 , EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=ei.PositionID
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    --where   O.Occupied > 0 and isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0) = 0
                                      where rae.isOccupied > 0 and RM.Vacancy <= o.Occupied order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";


                }

                if (PartialVacantFullyOccupied == "PartialVacant")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName,  
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup
 , EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=ei.PositionID
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    --where   isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0) > 0 and O.Occupied > 0
                                       where rae.isOccupied > 0 and RM.Vacancy > o.Occupied order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";




                }

                if (PartialVacantFullyOccupied == "All")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName, 
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup
 , EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=ei.PositionID
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    where rae.isOccupied = 1 order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus";



                }
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

   

        // Pending For Allocation
        public DataTable pendingForAllocationReport()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                
                var sql = @"SELECT isSelected=(CAST(0 as bit)), Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    EMP.EmpPicPath,EMP.BudgetCode,E.UserName Entity,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, RG.UserName ResidenceGroup, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                        ,RM.Location, PR.Activity, RM.ResidenceCategory, RM.ResidentType, P.UserName Process
										FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
										LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId
										LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                                        left join HKP.Process P on P.Id = PR.ProcessId
										
                              Where EMP.PlantId ='" + identity.PlantId + @"' AND EMP.EmployeeStatus='Active' AND EMP.SystemId 
							  NOT IN(Select EmployeeSystemId from dbo.ResidenceAllocatedEmployees  Where isOccupied = 1) 
								
							  AND RG.IsResidenceApplicable = 'true' 
                              ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

      

        public DataTable TeamPlanReport(string todate, string fromdate, string teamName, string employeeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "";
                if (teamName == "null")
                {
                    sql = @"select TD.UserName TeamName,E.UserName Entity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,EC.UserName EmployeeCategory,DEG.UserName as Designation,
LD.UserName as LegalDesignation,SD.UserName as Shift,TDE.ResponsibilityLevel,EI.SystemId as EmpId,EI.EmployeeName,format(EI.DOJ,'dd-MMM-yyyy') as DOJ,
EI.EmployeeStatus,EI.EmployeeCurrentStatus,(select top 1 DATENAME(WEEKDAY, AD.WorkDate) from AttdnProcessData AD where AD.EmpSystemID=TDE.EmployeeId and AD.DayStatus='W') as  WeekOff,format(APD.WorkDate,'dd-MMM-yyyy') as Date,APD.DayStatus,TDE.PlanHours,isnull(FLOOR(APD.Duration/60),0) AvailableHours,
isnull(Alloted.AllotedHours,0) as AllotedHours,
isnull(Actual.ActualHours,0) as ActualHours,
TDE.Remarks,
TC.UserName as TeamCategory,
EAC.UserName EACategory
from TRN.TeamDefinition TD
left join TRN.TeamEntity TE ON TE.TeamDefinitionId=TD.Id 
left join ORG.Entity E ON E.Id=TE.EntityId
left join TRN.TeamDefinitionEmployee TDE ON TDE.TeamDefinitionId=TD.Id
left join hkp.EmployeeActivityCategory EAC ON EAC.Id=TDE.EmployeeActiviyCategory
left join EmployeeInformation EI ON EI.SystemId=TDE.EmployeeId  
left join ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
left join HKP.Designation DEG ON DEG.Id=EI.DesignationSystemID
left join ORG.Section S ON S.Id=EI.SectionId
left join ORG.SubSection SS ON SS.Id=EI.SubSectionId
left join TRN.TeamDefinitionCategory TDC ON TDC.TeamDefinitionId=TD.Id
left join hkp.TeamCategory TC ON TC.Id=TDC.TeamCategoryId
left join HKP.LegalDesignation LD ON LD.Id=EI.LegalDesignationId
left join [MST].[ManpowerBudget]  MB ON MB.Id=EI.BudgetCode
left join ShiftDefination SD ON SD.SystemID=MB.ShiftDefinationId
left join ORG.Position P on P.Id = MB.PositionId
left join MST.DesignationMaster DM on DM.DesignationId = P.DesignationId
left join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
left join AttdnProcessData APD ON APD.EmpSystemID=TDE.EmployeeId
left join (select  (sum(isnull(PlanMinutes,0))/60) AllotedHours,ResponsiblePersonId,mapd.PlannedDate from TRN.ResponsiblePlannedDetails rpd 
left join (select Id,PlannedDate,ActualDate from TRN.MachineAssetPlannedDetails )mapd on mapd.Id=rpd.PlannedId
group by rpd.ResponsiblePersonId,mapd.PlannedDate) Alloted ON Alloted.ResponsiblePersonId=TDE.EmployeeId and Alloted.PlannedDate=APD.WorkDate
left join (select  (sum(isnull(ActualMinutes,0))/60) ActualHours,ResponsiblePersonId,mapd.PlannedDate from TRN.ResponsiblePlannedDetails rpd 
left join (select Id,PlannedDate,ActualDate from TRN.MachineAssetPlannedDetails )mapd on mapd.Id=rpd.PlannedId  
group by rpd.ResponsiblePersonId,mapd.PlannedDate) Actual ON Actual.ResponsiblePersonId=TDE.EmployeeId and Actual.PlannedDate=APD.WorkDate 
where EI.EmployeeStatus='Active' and APD.WorkDate between '" + fromdate + "' and '" + todate + "'";
                }
                else
                {
                    if (employeeId == "null")
                    {
                        sql = @"select TD.UserName TeamName,E.UserName Entity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,EC.UserName EmployeeCategory,DEG.UserName as Designation,
LD.UserName as LegalDesignation,SD.UserName as Shift,TDE.ResponsibilityLevel,EI.SystemId as EmpId,EI.EmployeeName,format(EI.DOJ,'dd-MMM-yyyy') as DOJ,
EI.EmployeeStatus,EI.EmployeeCurrentStatus,(select top 1 DATENAME(WEEKDAY, AD.WorkDate) from AttdnProcessData AD where AD.EmpSystemID=TDE.EmployeeId and AD.DayStatus='W') as  WeekOff,format(APD.WorkDate,'dd-MMM-yyyy') as Date,APD.DayStatus,TDE.PlanHours,isnull(FLOOR(APD.Duration/60),0) AvailableHours,
isnull(Alloted.AllotedHours,0) as AllotedHours,
isnull(Actual.ActualHours,0) as ActualHours,
TDE.Remarks,
TC.UserName as TeamCategory,
EAC.UserName EACategory
from TRN.TeamDefinition TD
left join TRN.TeamEntity TE ON TE.TeamDefinitionId=TD.Id 
left join ORG.Entity E ON E.Id=TE.EntityId
left join TRN.TeamDefinitionEmployee TDE ON TDE.TeamDefinitionId=TD.Id
left join hkp.EmployeeActivityCategory EAC ON EAC.Id=TDE.EmployeeActiviyCategory
left join EmployeeInformation EI ON EI.SystemId=TDE.EmployeeId  
left join ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
left join HKP.Designation DEG ON DEG.Id=EI.DesignationSystemID
left join ORG.Section S ON S.Id=EI.SectionId
left join ORG.SubSection SS ON SS.Id=EI.SubSectionId
left join TRN.TeamDefinitionCategory TDC ON TDC.TeamDefinitionId=TD.Id
left join hkp.TeamCategory TC ON TC.Id=TDC.TeamCategoryId
left join HKP.LegalDesignation LD ON LD.Id=EI.LegalDesignationId
left join [MST].[ManpowerBudget]  MB ON MB.Id=EI.BudgetCode
left join ShiftDefination SD ON SD.SystemID=MB.ShiftDefinationId
left join ORG.Position P on P.Id = MB.PositionId
left join MST.DesignationMaster DM on DM.DesignationId = P.DesignationId
left join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
left join AttdnProcessData APD ON APD.EmpSystemID=TDE.EmployeeId
left join (select  (sum(isnull(PlanMinutes,0))/60) AllotedHours,ResponsiblePersonId,mapd.PlannedDate from TRN.ResponsiblePlannedDetails rpd 
left join (select Id,PlannedDate,ActualDate from TRN.MachineAssetPlannedDetails )mapd on mapd.Id=rpd.PlannedId
group by rpd.ResponsiblePersonId,mapd.PlannedDate) Alloted ON Alloted.ResponsiblePersonId=TDE.EmployeeId and Alloted.PlannedDate=APD.WorkDate
left join (select  (sum(isnull(ActualMinutes,0))/60) ActualHours,ResponsiblePersonId,mapd.PlannedDate from TRN.ResponsiblePlannedDetails rpd 
left join (select Id,PlannedDate,ActualDate from TRN.MachineAssetPlannedDetails )mapd on mapd.Id=rpd.PlannedId  
group by rpd.ResponsiblePersonId,mapd.PlannedDate) Actual ON Actual.ResponsiblePersonId=TDE.EmployeeId and Actual.PlannedDate=APD.WorkDate 
where EI.EmployeeStatus='Active' and APD.WorkDate between '" + fromdate + "' and '" + todate + "' and TD.Id='" + teamName + "'";
                    }
                    else
                    {
                        sql = @"select TD.UserName TeamName,E.UserName Entity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,EC.UserName EmployeeCategory,DEG.UserName as Designation,
LD.UserName as LegalDesignation,SD.UserName as Shift,TDE.ResponsibilityLevel,EI.SystemId as EmpId,EI.EmployeeName,format(EI.DOJ,'dd-MMM-yyyy') as DOJ,
EI.EmployeeStatus,EI.EmployeeCurrentStatus,(select top 1 DATENAME(WEEKDAY, AD.WorkDate) from AttdnProcessData AD where AD.EmpSystemID=TDE.EmployeeId and AD.DayStatus='W') as WeekOff,format(APD.WorkDate,'dd-MMM-yyyy') as Date,APD.DayStatus,TDE.PlanHours,isnull(FLOOR(APD.Duration/60),0) AvailableHours,
isnull(Alloted.AllotedHours,0) as AllotedHours,
isnull(Actual.ActualHours,0) as ActualHours,
TDE.Remarks,
TC.UserName as TeamCategory,
EAC.UserName EACategory
from TRN.TeamDefinition TD
left join TRN.TeamEntity TE ON TE.TeamDefinitionId=TD.Id 
left join ORG.Entity E ON E.Id=TE.EntityId
left join TRN.TeamDefinitionEmployee TDE ON TDE.TeamDefinitionId=TD.Id
left join hkp.EmployeeActivityCategory EAC ON EAC.Id=TDE.EmployeeActiviyCategory
left join EmployeeInformation EI ON EI.SystemId=TDE.EmployeeId  
left join ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
left join HKP.Designation DEG ON DEG.Id=EI.DesignationSystemID
left join ORG.Section S ON S.Id=EI.SectionId
left join ORG.SubSection SS ON SS.Id=EI.SubSectionId
left join TRN.TeamDefinitionCategory TDC ON TDC.TeamDefinitionId=TD.Id
left join hkp.TeamCategory TC ON TC.Id=TDC.TeamCategoryId
left join HKP.LegalDesignation LD ON LD.Id=EI.LegalDesignationId
left join [MST].[ManpowerBudget]  MB ON MB.Id=EI.BudgetCode
left join ShiftDefination SD ON SD.SystemID=MB.ShiftDefinationId
left join ORG.Position P on P.Id = MB.PositionId
left join MST.DesignationMaster DM on DM.DesignationId = P.DesignationId
left join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
left join AttdnProcessData APD ON APD.EmpSystemID=TDE.EmployeeId
left join (select  (sum(isnull(PlanMinutes,0))/60) AllotedHours,ResponsiblePersonId,mapd.PlannedDate from TRN.ResponsiblePlannedDetails rpd 
left join (select Id,PlannedDate,ActualDate from TRN.MachineAssetPlannedDetails )mapd on mapd.Id=rpd.PlannedId
group by rpd.ResponsiblePersonId,mapd.PlannedDate) Alloted ON Alloted.ResponsiblePersonId=TDE.EmployeeId and Alloted.PlannedDate=APD.WorkDate
left join (select  (sum(isnull(ActualMinutes,0))/60) ActualHours,ResponsiblePersonId,mapd.PlannedDate from TRN.ResponsiblePlannedDetails rpd 
left join (select Id,PlannedDate,ActualDate from TRN.MachineAssetPlannedDetails )mapd on mapd.Id=rpd.PlannedId  
group by rpd.ResponsiblePersonId,mapd.PlannedDate) Actual ON Actual.ResponsiblePersonId=TDE.EmployeeId and Actual.PlannedDate=APD.WorkDate 
where EI.EmployeeStatus='Active' and APD.WorkDate between '" + fromdate + "' and '" + todate + "' and TDE.EmployeeId='" + employeeId + "' and TD.Id='" + teamName + "'";
                    }
                }

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable MaintenanceStatusSummaryReport(string todate,string fromdate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select X.Id,X.Entity,X.ScheduleName,X.MachineName,X.Make,X.Model,X.ScheduleCode,X.ResponsiblePersonBudgetCode,
count(X.NoOfAsset) as NoOfAsset,sum(X.OverDue) as OverDue,sum(X.DueToday) as DueToday,sum(X.FutureDue) as FutureDue,X.Remarks,X.PlanStatus,X.Department,X.MaintenanceGroup,X.NoOfScheduleReqFP,X.ScheduleCompletedFP,X.Difference from (
select MS.Id,E.UserName Entity,MS.UserName ScheduleName,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MB.Code ResponsiblePersonBudgetCode,count(MMA.Id) NoOfAsset,
MS.ScheduleDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.Remarks,(select count(MPD.Id) from [TRN].[MachineAssetPlannedDetails] MPD where MPD.PlannedDate is null) as PlanStatus,
(select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,MS.MaintenanceGroup,
DATEDIFF(Day,'" + fromdate + @"','" + todate + @"')/MS.ScheduleDays NoOfScheduleReqFP,(select count(MPD.Id) from [TRN].[MachineAssetPlannedDetails] MPD where MPD.ActualDate is not null) ScheduleCompletedFP,DATEDIFF(day,(select count(MPD.Id) from [TRN].[MachineAssetPlannedDetails] MPD where MPD.ActualDate is not null),(DATEDIFF(Day,'" + fromdate + @"','" + todate + @"')/MS.ScheduleDays)) Difference
 from TRN.Maintenancescheduling MS
 --left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 where MMA.Id is not null and Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)) end between '" + fromdate + "' and '" + todate + "' group by MS.Id,MMA.Id,E.UserName,MS.UserName,MM.UserName,MM.MachineMake,MM.MachineModel,MS.ScheduleCode,MB.Code,MS.LastMaintenanceDate,MS.ScheduleDays,MS.Remarks,MS.DepartmentId,MS.MaintenanceGroup) X group by NoOfAsset,Id,Entity,ScheduleName,MachineName,Make,Model,ScheduleCode,ResponsiblePersonBudgetCode,Remarks,PlanStatus,Department,MaintenanceGroup,NoOfScheduleReqFP,ScheduleCompletedFP,Difference";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable MaintenanceStatusDetailsReport(string todate, string fromdate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select MS.Id,E.UserName Entity,MS.UserName ScheduleName,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MB.Code ResponsiblePersonBudgetCode,MA.AssetName,MA.AssetCode,
WC.UserName WorkCenter,MS.ScheduleDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,MS.Remarks,(select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,MS.MaintenanceGroup
,Format(MPD.PlannedDate,'dd-MMM-yyyy') as PlannedDate
 from TRN.Maintenancescheduling MS
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 left join TRN.MachineAssetPlannedDetails MPD ON MPD.AssetId=MMA.Id
 where MMA.Id is not null and Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)) end between '" + fromdate + "' and '" + todate + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable MaintenancePlanningReport(string todate, string fromdate, string Status)
        {
            try
            {
                string Filter = string.Empty;

                if (Status == "All")
                {
                    Filter = " and (MPD.ActualDate is not null or MPD.ActualDate is null) and MPD.PlannedDate is not null";
                }
                else if (Status == "Completed")
                {
                    Filter = " and MPD.ActualDate is not null and MPD.PlannedDate is not null";
                }
                else
                {
                    Filter = " and MPD.ActualDate is null and MPD.PlannedDate is not null";
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"Select  P.UserName as Process,WC.UserName WorkCenter,WC.Code WCCode,MA.AssetName,MA.AssetCode,MM.MachineMake Make,
MM.MachineModel Model,MS.UserName ScheduleName,MS.ScheduleCode,Format(MPD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,
isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'') as LMD,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CMD,MPD.Remarks
from HKP.Process P
left join SCS.WorkCenterMaster WC ON WC.ProcessId=P.Id
left join MachineMasterAsset MA ON MA.WorkCenterMasterId=WC.Id
left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
left join TRN.MaintenanceMachineAsset MMA ON MA.Id=MMA.AssetId and MMA.IsActive=1
left Join TRN.MaintenanceScheduling MS ON MMA.MaintenanceSchedulingId=MS.Id 
left join TRN.MachineAssetPlannedDetails MPD ON MPD.AssetId=MMA.Id
where P.Active=1 and Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)) end between '" + fromdate + "' and '" + todate + "' " + Filter + @" order by Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id ORDER BY APD.Id DESC),'')= '' then GETDATE() else (MS.ScheduleDays + (select top 1 ActualDate from[TRN].[MachineAssetPlannedDetails] APD where APD.AssetId = MMA.Id ORDER BY APD.Id DESC)) end";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable SpecialIssueControlDetailsReport(string FromDate, string ToDate, string Shift)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select distinct '' Id,SIC.SpecialIssueName,SII.SpecialIssueItem,format(SICU.Date,'dd-MMM-yyyy') as Date,
SICU.Shift,(select PeriodName from (select PeriodName,format(Time,'HH:mm:tt') as FromTime,isnull(lead(format(Time,'HH:mm:tt'))Over(order by Sequence),format(Time+2,'HH:mm:tt')) as ToTime from  MST.SpecialIssueDefinePeriod) P where format(SICU.Time,'HH:mm:tt') between P.FromTime and P.ToTime) Period,
format(SICU.Time,'hh:mm:tt') as Time,SII.SampleSize,SIUI.Value,SIUI.Remarks,SIUI.ConfidenceLevel,convert(decimal(18,2),(SIUI.Value/SII.SampleSize)) as Percentage
from TRN.SpecialIssueControl SIC
left join TRN.SpecialIssueItem SII ON SII.SpecialIssueControlId=SIC.Id
left join TRN.SpecialIssueControlUpdate SICU ON SICU.IssueId=SIC.Id
left join TRN.SpecialIssueUpdateItem SIUI ON SIUI.ICUId=SICU.Id and SIUI.SICItemId=SII.Id
where 
format(SICU.Date,'dd-MMM-yyyy') between '" + FromDate + "' and '" + ToDate + "' and SICU.Shift='" + Shift + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable SpecialIssueControlSummaryReport(string FromDate, string ToDate, string Shift)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select X.SpecialIssueName,X.SpecialIssueItem,Sum(X.SampleSize) as SampleSize,Sum(Value) as Value,convert(decimal(18,2),Sum(Value)/Sum(X.SampleSize)) as Percentage from (select distinct '' Id,SIC.SpecialIssueName,SII.SpecialIssueItem,format(SICU.Date, 'dd-MMM-yyyy') as Date,SICU.Shift,(select PeriodName from(select PeriodName, format(Time,'HH:mm:tt') as FromTime,isnull(lead(format(Time, 'HH:mm:tt'))Over(order by Sequence), format(Time + 2, 'HH:mm:tt')) as ToTime from MST.SpecialIssueDefinePeriod) P where format(SICU.Time, 'HH:mm:tt') between P.FromTime and P.ToTime) Period,format(SICU.Time, 'hh:mm:tt') as Time,SII.SampleSize,SIUI.Value,SIUI.Remarks,SIUI.ConfidenceLevel,convert(decimal(18,2),(SIUI.Value/SII.SampleSize)) as Percentage from TRN.SpecialIssueControl SIC left join TRN.SpecialIssueItem SII ON SII.SpecialIssueControlId = SIC.Id left join TRN.SpecialIssueControlUpdate SICU ON SICU.IssueId = SIC.Id left join TRN.SpecialIssueUpdateItem SIUI ON SIUI.ICUId = SICU.Id and SIUI.SICItemId = SII.Id where Date between '" + FromDate + "' and '" + ToDate + "' and Shift = '" + Shift + "') X group by X.SpecialIssueItem,X.SpecialIssueName";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion Detail Residence Status Report

        #region Residence Grid View
        public IEnumerable<object> detailResidenceStatusGrid(string PartialVacantFullyOccupied)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "";
                if (PartialVacantFullyOccupied == "FullyOccupied")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
                            , RM.Vacancy, O.Occupied, Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName,
                            D.UserName Department, S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity,isnull(P.Activity,'') Activity, ei.EmployeeStatus,
                             FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ,isnull(FORMAT(ei.DOS, 'dd-MMM-yyyy'),'')DOS,isnull(ei.EmployeeCurrentStatus,'') EmployeeCurrentStatus,  P.PaymentLink Skill,isnull(PR.UserName,'') Process, RG.UserName ResidenceGroup
                             , EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=ei.PositionID
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    --where   O.Occupied > 0 and isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0) = 0
                                    where rae.isOccupied > 0 and RM.Vacancy <= o.Occupied order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";


                }

                if (PartialVacantFullyOccupied == "PartialVacant")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
                            ,RM.Vacancy, O.Occupied,Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName,D.UserName Department,
                            S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity,isnull(P.Activity,'') Activity, ei.EmployeeStatus,
                            FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ,isnull(FORMAT(ei.DOS, 'dd-MMM-yyyy'),'')DOS,isnull(ei.EmployeeCurrentStatus,'') EmployeeCurrentStatus,  
                            P.PaymentLink Skill,isnull(PR.UserName,'') Process, RG.UserName ResidenceGroup,EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=ei.PositionID
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    --where   isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0) > 0 and O.Occupied > 0
                                    where rae.isOccupied > 0 and RM.Vacancy > o.Occupied order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";




                }

                if (PartialVacantFullyOccupied == "All")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
                            , RM.Vacancy, O.Occupied,Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName,   
                            D.UserName Department,S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity,isnull(P.Activity,'') Activity, ei.EmployeeStatus,
                             FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ,isnull(FORMAT(ei.DOS, 'dd-MMM-yyyy'),'')DOS,isnull(ei.EmployeeCurrentStatus,'') EmployeeCurrentStatus,  
                             P.PaymentLink Skill,isnull(PR.UserName,'') Process, RG.UserName ResidenceGroup, EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=ei.PositionID
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    where rae.isOccupied = 1 order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";



                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> pendingForAllocationGrid()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                /*var sql = @"select ei.EmployeeCode,  ei.EmployeeName, EC.UserName EmployeeCategory,  
D.UserName Department, RM.ResidentType,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
ei.EmployeeCurrentStatus, FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy') DOS,  P.PaymentLink Skill,
PR.UserName Process, RG.UserName ResidenceGroup from EmployeeInformation ei
left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
left join ResidenceGroup rg on rg.Id = ei.ResidenceGroupId
left join ResidenceAllocatedEmployees RAM on RAM.EmployeeSystemId = ei.SystemId
left join ResidenceMaster RM on RM.Id = RAM.ResidenceId
left join org.Section S on S.Id = ei.SectionId
left join org.SubSection SS on SS.Id = ei.SubSectionId
left join org.Department D on D.Id = ei.DepartmentId
left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
left join org.Entity E on E.Id =MPB.EntityId
left join ORG.Position P on P.Id=ei.PositionID
left join HKP.Process PR on PR.Id = P.ProcessId
left join MST.DesignationMaster DM on DM.DesignationId = DE.Id
LEFT JOIN HKP.EmployeeCategory ec ON ec.Id = DM.EmployeeCategoryId
where ei.ResidenceGroupId = 'RG221' and RAM.EmployeeSystemId is null and ei.EmployeeStatus = 'Active'
";*/
                var sql = @"SELECT isSelected=(CAST(0 as bit)), Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName Entity,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, RG.UserName ResidenceGroup, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                        ,RM.Location, RM.ResidentType, PR.Activity, RM.ResidenceCategory, P.UserName Process
										FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
										LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId
										LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                                        left join HKP.Process P on P.Id = PR.ProcessId
										
                              Where EMP.PlantId ='" + identity.PlantId + @"' AND EMP.EmployeeStatus='Active' AND EMP.SystemId 
							  NOT IN(Select EmployeeSystemId from dbo.ResidenceAllocatedEmployees  Where isOccupied = 1) 
								
							  AND RG.IsResidenceApplicable = 'true' 
                              ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> pendingForUnAllocationGrid()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName, DGM.EmployeeCategory,  
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup

							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=ei.PositionID
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
									
									where RAE.isOccupied = 1 and (EI.EmployeeStatus <> 'Active' 
								   or EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' or EI.EmployeeCurrentStatus = 'TBS')
 order by 
								   case 
								when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
								when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
								when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
								when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
							
								else
								5
								 end ASC
                       --where RAE.isOccupied = 1 and EI.EmployeeStatus <> 'Active' or RAE.isOccupied = 1 and EI.EmployeeCurrentStatus <> null

";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> residenceSummarGrid()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select EC.UserName EmpCategory,  RM.[Location], RM.Block, RM.ResidentType, sum(rm.vacancy)Capacity,
sum(rm.Rooms)Rooms, sum(cast(rae.Occupied as INT)) as Allotted,
case when isnull(isnull(sum(rm.vacancy),0)-isnull(sum(rae.Occupied),0),0) = 0 then '0' else isnull(isnull(sum(rm.vacancy),0)-isnull(sum(rae.Occupied),0),0)
end  Balance
from ResidenceMaster RM
left join (select distinct rae.ResidenceId, count(rae.EmployeeSystemId) Occupied from dbo.ResidenceAllocatedEmployees rae
LEFT JOIN EmployeeInformation E on E.SystemId = rae.EmployeeSystemId
where rae.isOccupied = 1
group by rae.ResidenceId) rae on rae.ResidenceId = RM.Id
left join HKP.EmployeeCategory EC on EC.Id = RM.EmployeeCategoryId
group by EC.UserName,  RM.[Location], RM.Block, RM.ResidentType";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Residence Grid View
    }
    #endregion Residence Status Report

}



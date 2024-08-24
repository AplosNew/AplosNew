using clsAttendance;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Aplos.HumanResource
{
    public class EmployeeTransport
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        clsEmployeeLoad objEL = new clsEmployeeLoad();
        public EmployeeTransport()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }
        public IEnumerable<object> GetemployeeDataListRouteEmp(string plantId)
        {
            try
            {
                string CmdText = @"SELECT isSelected=(CAST(0 as bit)), Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus
									,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,PR.PaymentLink Skill,DEG.UserName GivenDesignation
									,S.UserName Section,SS.UserName SubSection,DEPT.UserName Department,E.UserName EntityName
									,PL.UserName Plant,TG.UserName TransportGroup,'' StoppageId
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        left join [dbo].[TransportGroup] TG on TG.Id=EMP.TransportGroupId
                              Where EMP.PlantId ='" + plantId + @"' AND EMP.EmployeeStatus='Active' 
							   and TG.IsTransportApplicable=1 and EMP.SystemId not in (select EmployeeSystemId from EmployeeTransportAllocation where AssignStatus=1)
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

        public IEnumerable<object> GetemployeeListRoute(string plantId)
        {
            try
            {
                string CmdText = @"select Emp.EmployeeCode,Emp.EmployeeName,Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,MSD.UserName BudgatedShift,ESD.UserName AssignedShift
									,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ
									,Skill =isnull(OM.UserName,OV.UserName),DEG.UserName GivenDesignation
									,S.UserName Section,SS.UserName SubSection,DEPT.UserName Department,E.UserName EntityName
									,PL.UserName Plant,TG.UserName TransportGroup,'' StoppageId
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
										LEFT JOIN ShiftDefination MSD on MSD.SystemID=PMB.ShiftDefinationId 
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        left join [dbo].[TransportGroup] TG on TG.Id=EMP.TransportGroupId
										LEFT JOIN dbo.AttdnProcessData apd on apd.EmpSystemID=EMP.SystemId AND apd.WorkDate=FORMAT(GetDate(),'dd-MMM-yyyy')
										LEFT JOIN ShiftDefination ESD on ESD.SystemID=apd.ShiftSystemID
										LEFT JOIN [MST].[OperationVariation] OV on OV.Id=EMP.OperationVariationId
                                        LEFT JOIN [MST].[OperationMaster] OM on OM.Id=EMP.OperationMasterId
                              Where EMP.PlantId ='" + plantId + @"' AND EMP.EmployeeStatus='Active' 
							   and TG.IsTransportApplicable=1 and EMP.SystemId not in (select EmployeeSystemId from EmployeeTransportAllocation where AssignStatus=1)
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

        public IEnumerable<object> getviewUnassign(string plantId)
        {
            try
            {
                string CmdText = @"select ETA.Id,EI.EmployeeCode,EI.EmployeeName,EI.EmployeeStatus,EI.EmployeeCurrentStatus,format(EI.DOJ,'dd-MMM-yyyy') DOJ,EI.DOS,R.StandardName [Route]
							                    ,TD.TransportUserName Transport,SD.UserName [Shift],R.[From],R.[To],RS.Id TripId,RS.TripNo
												,Skill =isnull(OM.UserName,OV.UserName)
							                    ,DEG.UserName GivenDesignation,S.UserName Section,SS.UserName SubSection,DEPT.UserName Department,E.UserName Entity,PL.UserName Plant
												,ST.Id StoppageId,ST.UserName Stoppage,ETA.AssignStatus,format(ETA.UnassignDate,'dd-MMM-yyyy') UnassignDate
												,format(ETA.AssignDate,'dd-MMM-yyyy') AssignDate,TG.UserName TransportGroup
												,MSD.UserName BudgatedShift,ESD.UserName AssignedShift
							                    from EmployeeTransportAllocation ETA
							                    left join EmployeeInformation EI on EI.SystemId = ETA.EmployeeSystemId
							                    LEFT JOIN MST.ManpowerBudget PMB ON PMB.Id=EI.BudgetCode
							                    LEFT JOIN ORG.Position PR ON PR.Id=PMB.PositionId
							                    LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
												LEFT JOIN ShiftDefination MSD on MSD.SystemID=PMB.ShiftDefinationId 
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
                                                left join [dbo].[TransportGroup] TG on TG.Id=EI.TransportGroupId
                                                LEFT JOIN dbo.AttdnProcessData apd on apd.EmpSystemID=EI.SystemId AND apd.WorkDate=FORMAT(GetDate(),'dd-MMM-yyyy')
												LEFT JOIN ShiftDefination ESD on ESD.SystemID=apd.ShiftSystemID
												LEFT JOIN MST.OperationVariation OV on OV.Id=EI.OperationVariationId
												LEFT JOIN MST.OperationMaster OM on OM.Id=EI.OperationMasterID 
                                
                                Where EI.PlantId='" + plantId + @"' and ETA.AssignStatus = 1 ";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetBusVerificationData(string fromDate, string toDate)
        {
            try
            {
               string sql = @"SELECT ROW_NUMBER() OVER(ORDER BY BV.EmpSystemID) SrNo, BV.EmpSystemID,EI.EmployeeCode,FORMAT(BV.WorkDate,'dd-MMM-yyyy')WorkDate,ISNULL(format(BV.InTime,'dd-MMM-yyyy hh:mm tt'),'') as InTime
,ISNULL(format(BV.OutTime,'dd-MMM-yyyy hh:mm tt'),'') OutTime,BV.AddedBy,FORMAT(BV.AddedDate,'dd-MMM-yyyy')AddedDate,BV.UpdatedBy,FORMAT(BV.UpdatedDate,'dd-MMM-yyyy')UpdatedDate
,ST.UserName Stoppage,TD.TransportUserName Transport,R.StandardName [Route],S.UserName Section,SS.UserName SubSection,DEPT.UserName Department
FROM dbo.BusVerification BV
LEFT JOIN dbo.EmployeeTransportAllocation ETA ON ETA.EmployeeSystemId=EmpSystemId
LEFT JOIN HKP.Stoppage ST on ST.Id=ETA.StoppageId
LEFT JOIN RouteSchedule RS on RS.Id = ETA.TripId
LEFT JOIN MST.Route R on R.Id = RS.RouteId
LEFT JOIN TransportDetail TD on TD.Id = RS.TransportId
LEFT JOIN EmployeeInformation EI on EI.SystemId = ETA.EmployeeSystemId
LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
LEFT JOIN ORG.Department DEPT ON EI.DepartmentId=DEPT.Id
Where ETA.AssignStatus=1 AND BV.WorkDate between '" + fromDate + "' AND '"+toDate+"'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public void SaveEmployeeTransportAllocation(List<Dictionary<string, object>> EmployeeList)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "dbo.EmployeeTransportAllocation";
                DataSet dsMaster = null;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string _Id = "";

                #region data Master update
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID(TableName, out _Id);
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where AssignStatus <> 0", out dsMaster, false, "1");
                int count = 0;
                foreach (var item in EmployeeList)
                {
                    
                    count++;
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "EmployeeSystemId='" + item["SystemID"] + "' ";
                   

                    if (dv.Count == 0)
                    {
                        item["Id"] = _Id + "-" + count;
                        item["AssignDate"] = DateTime.Now;
                        item["TripId"] = item["TripId"];
                        item["EmployeeSystemId"] = item["SystemID"];
                        item["StoppageId"] = item["StoppageId"];
                        item["AssignStatus"] = 1;
                        AddNewRow(dsMaster.Tables[0], item);
                    }
                    //else
                    //{
                    //    DataRow drmo = dv[0].Row;
                    //    item["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    //    item["AssignStatus"] = 1;
                    //    EditRow(drmo, item);
                    //}
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

        //Route Employee start

        public void SaveUnassignData(List<Dictionary<string, object>> employeeList)
        {
            try
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
                string TableName = "dbo.EmployeeTransportAllocation";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id In (" + id + ")", out dsMaster, false, "1");

                //string _Id = "";

                #region data Master update

                foreach (var item in employeeList)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

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
    }
}

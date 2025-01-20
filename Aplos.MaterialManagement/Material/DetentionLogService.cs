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

namespace Library.MaterialManagement.Material
{
    #region Detention Log
    public class DetentionLogService
    {
        SqlRepository _sqlRepository;
        public DetentionLogService()
        {
            _sqlRepository = new SqlRepository();
        }
       

        // Workcenter
        public IEnumerable<object> GetWorkCenter(string processId)
        {
            try
            {
                var sql = @"select WM.StandardName Text, WM.Id Value from SCS.WorkCenterMaster WM                          
							where WM.ProcessId = '"+ processId + "'order by Text";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Get Department List
        public IEnumerable<object> GetDetentionDepartment()
        {
            try
            {
                string sql = @"select distinct DD.DepartmentId Value, D.StandardName Text from DetentionMasterDepartment DD
                              left outer join ORG.Department D on D.id = DD.DepartmentId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Responsible Person
        public IEnumerable<object> GetDetentionResponsible(string detentionTypeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string str = @"select distinct E.SystemId as ResponsiblePersonId, E.CellPhnNo ,E.EmployeeCode,E.EmployeeName as ResponsiblePerson,DEP.UserName AS Department,S.UserName as Section,
                           SS.UserName as SubSection,DEG.UserName AS [LegalDesignation]--,DR.DetentionMasterId
						   --CAST (CASE WHEN DLRP.Id IS NULL THEN 0 ELSE 1 END AS bit) chk, DLRP.isActive
						   from DetentionMasterResponsible DR
                           left join EmployeeInformation AS E ON E.SystemId=DR.ResponsibleMasterId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=E.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.id=E.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=PR.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
							--Left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.ResponsiblePersonId = E.SystemId
							left join dbo.DetentionMaster DM on DM.Id = DR.DetentionMasterId
							left join hkp.DetentionType DT on DT.Id = DM.DetentionTypeId
							where DT.Id = '" + detentionTypeId + "'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getRespPersonContactNo(string ResponsiblePersonId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string str = @"select E.CellPhnNo from  DetentionMasterResponsible DR
                           left join EmployeeInformation AS E ON E.SystemId=DR.ResponsibleMasterId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=E.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.id=E.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=PR.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
							where E.SystemId = '" + ResponsiblePersonId + "'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> getIssueByNo(string loginId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string str = @"select E.CellPhnNo IssueByNo from EmployeeInformation E
                                where E.SystemId = '" + identity.EmployeeId + "'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // Detention Type
        public IEnumerable<object> getDetentionTypeListByDepartment()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select distinct DT.UserName As Text, DT.Id As Value from DetentionMasterDepartment DD
                        left join DetentionMaster DM ON DM.Id=DD.DetentionMasterId
                        left join hkp.DetentionType DT ON DT.id=DM.DetentionTypeId
                        order by Text
            ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Detention Master based on Detention Type
        public IEnumerable<object> getDetention(string processId)
        {
            try
            {
                var sql = @"select Distinct DM.DetentionUserName as Text, DM.Id as Value 
                            from DetentionMaster DM
                            LEFT JOIN DetentionMasterProcess DMP on DMP.DetentionMasterId = DM.Id
                            LEFT JOIN HKp.Process P on P.Id = DMP.ProcessId
                            where P.Id = '" + processId + "' order by Text ASC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getProcessList()
        {
            try
            {
                var sql = @"select distinct WM.ProcessId Value, P.UserName Text from SCS.WorkCenterMaster WM
                            left join HKP.Process P on P.Id = WM.ProcessId";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMachineMasterAsset()
        {
            try
            {
                var sql = @"select distinct WM.ProcessId Value, P.UserName Text from SCS.WorkCenterMaster WM
                            left join HKP.Process P on P.Id = WM.ProcessId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetDepartment(string detentiontypeId)
        {
            try
            {
                var sql = @"select distinct DMD.DepartmentId Value, D.UserName Text from org.Department D
						left join dbo.DetentionMasterDepartment DMD on DMD.DepartmentId = D.Id
						left join dbo.DetentionMaster DM on DM.Id = DMD.DetentionMasterId
						where DM.DetentionTypeId = '"+ detentiontypeId + "'order by Text";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> Save(Dictionary<string, object> data)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "TRN.DetentionLog";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "DL" + _Id;
                    
                    data["isClose"] = 0;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    
                    data["isClose"] = 1;

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

        public List<Dictionary<string, object>> saveDtentionLogResPerson(List<Dictionary<string, object>> data, string detentionLogId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM TRN.DetentionLogResponsiblePerson WHERE DetentionLogId ='" + detentionLogId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                //string DetentionMasterDepartmentId = "";

                string _Id = "";

                int count = 0;
                foreach (var item in data)
                {
                    count++;

                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.DetentionLog", out _Id);

                        item["Id"] = "DLRP-" + _Id;
                        item["DetentionLogId"] = detentionLogId;                   
                       
                        AddNewRow(dsMasterOrder.Tables[0], item);
                    }
                    else
                    {

                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();

                       
                        dr["DetentionLogId"] = detentionLogId;
                        
                        EditRow(dr, item);
                       
                        dr.EndEdit();



                    }

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
                return data;
            }
            catch (Exception ex)
            {
                throw (ex);
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
    }
    #endregion Detention Log

    #region Detention Logout
    public class DetentionLogoutService
    {
        SqlRepository _sqlRepository;
        public DetentionLogoutService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetDetentionResponsible(string detentionTypeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string str = @"select distinct E.SystemId as ResponsiblePersonId, E.CellPhnNo ,E.EmployeeCode,E.EmployeeName as ResponsiblePerson,DEP.UserName AS Department,S.UserName as Section,
                           SS.UserName as SubSection,DEG.UserName AS [LegalDesignation],
					 DLRP.isActive
						   from DetentionMasterResponsible DR
                           left join EmployeeInformation AS E ON E.SystemId=DR.ResponsibleMasterId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=E.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.id=PR.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=PR.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
							Left join (select * from TRN.DetentionLogResponsiblePerson where Id in ('DLRP-16', 'DLRP-2114')) DLRP on DLRP.ResponsiblePersonId = E.SystemId
                            left join dbo.DetentionMaster DM on DM.Id = DR.DetentionMasterId
							left join hkp.DetentionType DT on DT.Id = DM.DetentionTypeId
							--where DT.Id = '" + detentionTypeId + "'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getByWhom()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select EmployeeName ByWhom from EmployeeInformation Where SystemId = '" + identity.EmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> getDetentionLogGrid()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                
                string sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType, format(DL.AddedDate, 'dd-MMM-yyyy hh:mm') LoginTime, DL.IssueByNo, DL.Remarks , 
WM.Id WorkCenterId ,  DT.Id DetentionTypeId, format(DL.LogoutTime, 'dd-MMM-yyyy hh:mm') CloseTime,  ISNULL(DL.isClose,0) isClose,
P.UserName Process,  DL.ProcessId, DL.DepartmentId, DP.UserName Department, DL.UpdateRemarks
from TRN.DetentionLog DL
left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                                left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
								left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                                left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
								left join HKP.Process P on P.Id = DL.ProcessId
                                left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where isClose = 0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDetentionLogResponsiblePerson(string detentionLogId)
        {
            try
            {
                string sql = @"select DLRP.Id, EI.EmployeeCode, EI.SystemId ResponsiblePersonId ,EI.EmployeeName, EI.CellPhnNo, DEP.UserName as Department, S.UserName Section, SS.UserName SubSection,
                                DEG.UserName as LegalDesignation, DLRP.isActive, DLRP.Id
                                from TRN.DetentionLogResponsiblePerson DLRP
                                LEFT JOIN TRN.DetentionLog DL on DL.Id = DLRP.DetentionLogId
								LEFT JOIN EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
								LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
								LEFT JOIN ORG.Department AS DEP ON DEP.id=EI.DepartmentId
								LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
								LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                                where DLRP.DetentionLogId  = '" + detentionLogId + "' and DLRP.isActive = 1";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetDepartment()
        {
            try
            {
                var sql = @"select distinct DMD.DepartmentId Value, D.UserName Text from org.Department D
						left join dbo.DetentionMasterDepartment DMD on DMD.DepartmentId = D.Id
						left join dbo.DetentionMaster DM on DM.Id = DMD.DetentionMasterId
						order by Text";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region update
        public Dictionary<string, object> Update(Dictionary<string, object> data, string detentionLogId)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "TRN.DetentionLog";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + detentionLogId + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenID(TableName, out _Id);

                    
                    data["Id"] = detentionLogId;
                    data["isUpdate"] = 0;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["Id"] = detentionLogId;
                    data["isUpdate"] = 1;
                   

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

        public Dictionary<string, object> UpdateResponsiblePerson(Dictionary<string, object> data, string detentionLogId)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "TRN.DetentionLog";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("update TRN.DetentionLogResponsiblePerson set  isActive = 0 where Id ='" + detentionLogId + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenID(TableName, out _Id);


                    data["Id"] = detentionLogId;
                    data["isUpdate"] = 0;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["Id"] = detentionLogId;
                    data["isUpdate"] = 1;


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
        #endregion update

        #region save detention log out
        public Dictionary<string, object> saveDtentionLogout(Dictionary<string, object> data, string detentionLogId, string logouttime)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                string TableName = "TRN.DetentionLog";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + detentionLogId + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update

                bplib.clsGenID genid = new bplib.clsGenID();

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenID(TableName, out _Id);

                    // data["Id"] = "DL" + _Id;
                    data["Id"] = detentionLogId;
                    data["isClose"] = false;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["isClose"] = true;
                    dr["LogoutTime"] = logouttime;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();

                }
                #endregion data Master update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

       

        #endregion save detention log out

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
                catch (Exception ex)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        #endregion Add & Edit Row

        #region REPORTS
        public IEnumerable<object> GetClosedDetentionGridReport(string from, string to, string departmentId, string detentionTypeId)
        {
            var sql = "";
            try
            {
                if (departmentId == "null" && detentionTypeId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime)/60, 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                            where DL.LogoutTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.isClose = 1";


                }
                else if (departmentId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime)/60, 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LogoutTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose = 1";

                }
                else if (detentionTypeId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime)/60, 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LogoutTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"'
								 and  DL.isClose = 1";
                }
                
                else
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime)/60, 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LogoutTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"'
								and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose = 1";
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region FOR EXCEL VIEW DOWNLOAD
        public void GetClosedDetentionExcelReport(string from, string to, string departmentId, string detentionTypeId, out DataTable data)
        {
            var sql = "";
            try
            {
                if (departmentId == "null" && detentionTypeId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime)/60, 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                            where DL.LogoutTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.isClose = 1";


                }
                else if (departmentId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime)/60, 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LogoutTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose = 1";

                }
                else if (detentionTypeId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime)/60, 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LogoutTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"'
								 and  DL.isClose = 1";
                }
                
                else
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName Process,  DL. ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, FORMAT(DL.LogoutTime, 'dd-MMM-yyyy')LogoutDate,
							FORMAT(DL.LogoutTime, 'hh:mm tt')LogoutTime,
isnull(DATEDIFF(MINUTE, DL.AddedDate, DL.LogoutTime)/60, 0)Duration,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LogoutTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"'
								and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose = 1";
                }
                data =  _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPendingDetentionGridView(string from, string to, string departmentId, string detentionTypeId)
        {
            var sql = "";
            try
            {
                if (departmentId == "null" && detentionTypeId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
                            FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName MachineMaster,  DL.ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, P.UserName Process,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and  DL.isClose <> 1";


                }
                else if (departmentId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
                            FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName MachineMaster,  DL.ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, P.UserName Process,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                            where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose <> 1";

                }
                else if (detentionTypeId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
                            FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName MachineMaster,  DL.ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, P.UserName Process,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"'
								 and  DL.isClose <> 1";
                }
                
                else
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
                            FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName MachineMaster,  DL.ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, P.UserName Process,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"'
								and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose <> 1";
                }

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetPendingDetentionExcelView(string from, string to, string departmentId, string detentionTypeId, out DataTable data)
        {
            try
            {
                var sql = "";
                if (departmentId == "null" && detentionTypeId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
                            FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName MachineMaster,  DL.ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, P.UserName Process,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and  DL.isClose <> 1";


                }
                else if (departmentId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
                            FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName MachineMaster,  DL.ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, P.UserName Process,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                            where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose <> 1";

                }
                else if (detentionTypeId == "null")
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
                            FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName MachineMaster,  DL.ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, P.UserName Process,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"'
								 and  DL.isClose <> 1";
                }

                else
                {
                    sql = @"select distinct DL.Id, WM.UserName WorkCenter, DT.UserName DetentionType,FORMAT(DL.AddedDate,'dd-MMM-yyyy')AddedDate,
                            FORMAT(DL.AddedDate,'hh:mm tt')AddedTime, DL.LoginTime,  DL.IssueByNo ,  DL.Remarks,
                            WM.Id WorkCenterId ,  DT.Id DetentionTypeId, DL.isClose, DL.isUpdate,
                            P.UserName MachineMaster,  DL.ProcessId, DL.AddedBy, DL.AddedFromIP
                            ,  DP.UserName Department, DL.DepartmentId, P.UserName Process,
                            STUFF((select ',' +  X.SystemId
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonId,
                            STUFF((select ',' +  X.EmployeeName
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ResponsiblePersonName,
                            STUFF((select ',' +  X.CellPhnNo
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') ContactNo,
							STUFF((select ',' +  DLR.Id
                            From TRN.DetentionLogResponsiblePerson DLR
                            left join EmployeeInformation X on X.SystemId = DLR.ResponsiblePersonId
                            where DLR.DetentionLogId = DL.Id and DLR.isActive = 1
                            FOR XML PATH('')
                            ),1,1,'') DLRPId
                            from TRN.DetentionLog DL
                            left join TRN.DetentionLogResponsiblePerson  DLR on DLR.DetentionLogId = DL.Id
                            left join SCS.WorkCenterMaster WM on WM.Id = DL.WorkCenterId
                            left join HKP.DetentionType DT on DT.Id = DL.DetentionTypeId
                            left join TRN.DetentionLogResponsiblePerson DLRP on DLRP.DetentionLogId = DL.Id
                            left join EmployeeInformation EI on EI.SystemId = DLRP.ResponsiblePersonId
                            left join HKP.Process P on P.Id = DL.ProcessId
                            left join ORG.Department DP on DP.Id = DL.DepartmentId
                                where DL.LoginTime between '" + from + " 00:00:00' and '" + to + " 12:59:59' and DL.DepartmentId = '" + departmentId + @"'
								and DL.DetentionTypeId = '" + detentionTypeId + "' and  DL.isClose <> 1";
                }
                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion FOR EXCEL VIEW DOWNLOAD
        #endregion REPORTS
    }
    #endregion Detention Logout
}

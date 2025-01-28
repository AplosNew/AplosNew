using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Linq;
using System.Data;
using OTSBD;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using Library.Data.UnitOfWorks;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Reflection;

namespace Library.Service.PerformanceManagement
{

    public class JobEvaluation
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        string TableName = "dbo.JobEvaluation";
        string TableName1 = "dbo.JobEvaluationChild";

        public JobEvaluation()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select j.*,FORMAT(j.EvaluationDate,'dd-MMM-yyyy') as JobEvalDate,p.UserName as Position, p.Code as PositionCode, div.UserName as Division, dept.UserName as Department,EMP.EmployeeStatus,EMP.EmployeeName as ResponsiblePerson,EMP.EmployeeCode
                                    ,empl.EmployeeStatus as EmpStatus,empl.EmployeeName as ApprovedByName,empl.EmployeeCode AS EmpCode
                                    from dbo.JobEvaluation j left join ORG.Position p on j.PositionCodeId=p.Id
									left join ORG.Division div on div.Id=p.DivisionId
									left join ORG.Department dept on dept.Id=p.DepartmentId
									left join dbo.EmployeeInformation EMP on EMP.SystemId=j.EvaluatorNameId
									left join dbo.EmployeeInformation empl on empl.SystemId=j.ApprovedById
                                    WHERE " + strkey + " order by p.Code";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobEvaluation", out sID);
            return sID;
        }

        public void Create(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where PositionCodeId='" + data["PositionCodeId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Position Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "JE" + GetPK();

                    dr["EvaluationDate"] = data["EvaluationDate"];
                    dr["PositionCodeId"] = data["PositionCodeId"];
                    dr["EvaluatorNameId"] = data["EvaluatorNameId"];
                    dr["ApprovedById"] = data["ApprovedById"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["EvaluationDate"] = data["EvaluationDate"];
                    dr["PositionCodeId"] = data["PositionCodeId"];
                    dr["EvaluatorNameId"] = data["EvaluatorNameId"];
                    dr["ApprovedById"] = data["ApprovedById"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
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

        public void EditRow(DataRow dr, Dictionary<string, object> sourceData)
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

        public void Delete(string Id)
        {
            try
            {
  
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where JobEvaluationId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Job Evaluation Master Data");
                    }
                }

                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + Id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> LoadAllPositionDetailsForSelection(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select p.*,div.UserName as Division, sdiv.UserName as SubDivision, desg.UserName as Designation, dept.UserName as Department, sec.UserName as Section, subsec.UserName as SubSection from ORG.Position p left join ORG.Division div on div.Id=p.DivisionId
                               left join ORG.SubDivision sdiv on sdiv.Id=p.SubDivisionId
							   left join HKP.Designation desg on desg.Id=p.DesignationId
							   left join ORG.Department dept on dept.Id=p.DepartmentId
							   left join ORG.Section sec on sec.id=p.sectionId
							   left join org.SubSection subsec on subsec.Id=p.SubSectionId
                               WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"'
                               AND isnull(p.Id,'') not in (select isnull(PositionCodeId,'') from dbo.JobEvaluation where Id='" + Id + @"')
                               order by p.Code";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> LoadAllEvaluatorDetails(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(EvaluatorNameId,'') from dbo.JobEvaluation where Id='" + Id + @"')
                  order by EMP.EmployeeCode";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> LoadApprovedbyDetails(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ApprovedById,'') from dbo.JobEvaluation where Id='" + Id + @"')
                  order by EMP.EmployeeCode";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        // Job Evaluation Child data

        public IEnumerable<object> getjobevalattributelist()
        {
            try
            {
                string sql = @"select pa.Id as Value, pa.UserName as Text from HKP.PerformanceAttribute pa inner join dbo.JobEvaluationMaster jem on pa.Id=jem.PerformanceAttributeId order by pa.UserName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> LoadAllJobEvalDetailsForSelection(string MasterId, string JobEvalAttributeId)
        {
            try
            {
                string sql = @"select je.*,jecc.Category, jecc.Criteria, jecc.Code, jecc.Points, jemc.Dimension1ControlName, jemc.Dimension1ControlLevel, jemc.Dimension1ControlCode, jemc.Dimension2ControlName, jemc.Dimension2ControlLevel, jemc.Dimension2ControlCode, jemc.Points as jemcPoints
                                           ,DimensionApp =CASE WHEN je.DimensionApplicable=1 THEN 'Yes' ELSE 'No' END
                                           from dbo.JobEvaluationMaster je
                                           left join dbo.JobEvaluationMasterChild2 jecc on jecc.JobEvaluationMasterId=je.Id
										   left join dbo.JobEvaluationMasterChild jemc on jemc.JobEvaluationMasterId=je.Id
										   where isnull(je.Id,'') not in (select isnull(JobEvaluationMasterId,'') from dbo.JobEvaluationChild where JobEvaluationId='" + MasterId + @"')
                                           and je.PerformanceAttributeId='"+ JobEvalAttributeId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        private string GetChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobEvaluationChild", out sID);
            return sID;
        }

        public void SaveJobEvalChildData(Dictionary<string, object> data, string MasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "C" + GetChildPK();

                    dr["JobEvaluationId"] = MasterId;
                    dr["JobEvaluationMasterId"] = data["JobEvaluationMasterId"];
                    dr["Factoring"] = data["Factoring"];
                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["JobEvaluationId"] = MasterId;
                    dr["JobEvaluationMasterId"] = data["JobEvaluationMasterId"];
                    dr["Factoring"] = data["Factoring"];
                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DelJobEChild(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
  
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                con.BeginTransaction();
                con.executeQuery("delete from " + TableName1 + " where Id='" + Id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> getJobEvalChildData(string MasterId)
        {
            try
            {
                string sql = @"select c.*,jm.AttributeStandardName as JobEvaluationMaster,pa.UserName as PerformanceAttribute, jmc.Dimension1ControlName, jmc.Dimension1ControlLevel, jmc.Dimension2ControlName, jmc.Dimension2ControlLevel, jmc.Points as DimPoints
                                         ,jmcc.Category, jmcc.Criteria, jmcc.Code, jmcc.Points
                                         from dbo.JobEvaluationChild c left join dbo.JobEvaluation j on j.Id=c.JobEvaluationId
                                         left join dbo.JobEvaluationMaster jm on jm.Id=c.JobEvaluationMasterId
										 left join HKP.PerformanceAttribute pa on pa.Id=jm.PerformanceAttributeId
										 left join dbo.JobEvaluationMasterChild jmc on jmc.JobEvaluationMasterId=jm.Id
										 left join dbo.JobEvaluationMasterChild2 jmcc on jmcc.JobEvaluationMasterId=jm.Id
										   where c.JobEvaluationId='" + MasterId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}

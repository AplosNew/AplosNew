#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class PostRecruitmentDocumentService : Service<EmployeeDocument>, IPostRecruitmentDocumentService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<EmployeeDocument> _postRecruitmentDocumentRepository;

        public PostRecruitmentDocumentService(
            IRepositoryAsync<EmployeeDocument> postRecruitmentDocumentRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) :
            base(postRecruitmentDocumentRepository, unitOfWork, pkGeneratorService)
        {
            _postRecruitmentDocumentRepository = postRecruitmentDocumentRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<EmployeeDocument> GetDocumentFile(string id)
        {
            try
            {
                var sql = @"Select * From [dbo].[EmployeeDocument]  Where EmpSystemId='" + id + "'";
                return _postRecruitmentDocumentRepository.SqlQuery<EmployeeDocument>(sql).AsEnumerable();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public Dictionary<string, object> GetDocFile(string id)
        {
            try
            {
                var sql = @"Select FileId, FileName From [dbo].[EmployeeDocument]  Where Id='" + id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private DataSet GetDocList(string plantId, string budgetIds, string empType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"SELECT BD.Id BudgetId,CD.Id AS ComplianceDocumentId,
                            CD.UserName DocumentName,CD.DocumentType,
                            CD.IsSkillBased,PC.PositionId,CDSD.OptionalOrMandatory,
                            CD.EmpType,E.UserName AS EmployeeCategory
                            FROM HKP.ComplianceDocumentSet AS CDS
                            LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON CDS.Id=DC.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id= CDSD.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
                            LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
                            LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id=PC.ComplianceDocumentId
                            LEFT OUTER JOIN ORG.Position PO ON PC.PositionId=PO.Id
							LEFT OUTER JOIN
			         (Select DM.DesignationId,MBD.Id, DM.EmployeeCategoryId
							From MST.DesignationMaster AS DM
							LEFT OUTER JOIN ORG.Position AS PS ON DM.DesignationId=PS.DesignationId
							LEFT OUTER JOIN (Select * From MST.ManpowerBudget Where ID IN( " + budgetIds + @")) AS MBD ON PS.Id=MBD.PositionId
							 Where MBD.Id is not null
							) BD ON BD.EmployeeCategoryId=DC.EmployeeCategoryId
                            WHERE CD.EmploymentStage='PreRecruitment'-- AND CD.DocumentationBy='Self'
							and DC.EmployeeCategoryId
							IN (Select D.EmployeeCategoryId From
                         (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + identity.CompanyGroupId + @"') AS D
                         LEFT OUTER JOIN ORG.Position AS P ON P.DesignationId = D.DesignationId
                         LEFT OUTER JOIN MST.ManpowerBudget AS M ON M.PositionId=P.Id WHERE M.Id IN (" + budgetIds + @"))
                         AND DC.PlantId='" + plantId + @"' and CD.IsSkillBased=1 AND PC.PositionId IN (select PositionId from MST.ManpowerBudget WHERE Id IN(" + budgetIds + @"))
                         AND (CD.EmpType='" + empType + @"' or CD.EmpType='Both')
						  UNION
				    SELECT
						  BD.Id BudgetId,
						  CD.Id AS ComplianceDocumentId,
                            CD.UserName DocumentName,CD.DocumentType,
                            CD.IsSkillBased,'' PositionId,CDSD.OptionalOrMandatory,
                            CD.EmpType,E.UserName AS EmployeeCategory
                            FROM HKP.ComplianceDocumentSet AS CDS
                            LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON CDS.Id=DC.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id= CDSD.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
                            LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
						   LEFT Outer JOIN
			         (Select DM.DesignationId,MBD.Id, DM.EmployeeCategoryId
							From MST.DesignationMaster AS DM
							LEFT OUTER JOIN ORG.Position AS PS ON DM.DesignationId=PS.DesignationId
							LEFT OUTER JOIN (Select * From MST.ManpowerBudget Where ID IN( " + budgetIds + @")) AS MBD ON PS.Id=MBD.PositionId
							 Where MBD.Id is not null
							) BD ON BD.EmployeeCategoryId=DC.EmployeeCategoryId
                            WHERE CD.EmploymentStage='PreRecruitment' --AND CD.DocumentationBy='Self'
							and DC.EmployeeCategoryId IN
							(Select D.EmployeeCategoryId From
                         (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + identity.CompanyGroupId + @"') AS D
                         LEFT OUTER JOIN ORG.Position AS P ON P.DesignationId = D.DesignationId
                         LEFT OUTER JOIN MST.ManpowerBudget AS M ON M.PositionId=P.Id WHERE M.Id IN( " + budgetIds + @"))
                         AND DC.PlantId='" + plantId + @"' and CD.IsSkillBased=0
                         AND (CD.EmpType='" + empType + @"' or CD.EmpType='Both')";
                var x = _sqlRepository.GetGridData(parameters).Source;
                return x;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //  public void SaveDocumentList(string plantId, string empType, List<EmployeeDocument> budgetIdList)
        //  {
        //      List<EmployeeDocument> docdb = new List<EmployeeDocument>();

        //      try
        //      {
        //          var budgetIds = "''";
        //          foreach (var item in budgetIdList)
        //          {
        //              if (budgetIds == "''")
        //              {
        //                  budgetIds = "'" + item.Bu + "'";
        //              }
        //              else
        //              {
        //                  budgetIds += ",'" + item.BudgetId + "'";
        //              }

        //          }
        //          var _pk = base.GetAutoNumber("EmployeeDocument", PKGeneratorEnum.Auto, null, DateTime.Now);
        //          int pkCount = 0;
        //          DataSet docList = GetDocList(plantId, budgetIds, empType);
        //          foreach (var item in budgetIdList)
        //          {
        //              var empId = item.PreRecruitmentEmployeeId;
        //              var budgetId = item.BudgetId;
        //              DataView dvList = new DataView(docList.Tables[0]);
        //              dvList.RowFilter = "BudgetId='" + budgetId + "'";
        //              for (int i = 0; i < dvList.Count; i++)
        //              {
        //                  pkCount++;
        //EmployeeDocument ob = new EmployeeDocument();
        //                  ob.Id = _pk + "-" + pkCount;
        //                  ob.ComplianceDocumentId = dvList[i]["ComplianceDocumentId"].ToString();
        //                  ob.PreRecruitmentEmployeeId = empId;
        //                  docdb.Add(ob);
        //              }
        //          }
        //          foreach (var item in docdb)
        //          {
        //              base.InsertGraph(item);
        //          }

        //      }
        //      catch (Exception ex)
        //      {
        //          throw new CustomException(ex.Message, ex,
        //                             Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //                             ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //      }
        //  }

        public void InsertGraph(IEnumerable<EmployeeDocument> entities, string empSystemID)
        {
            try
            {
                var pk = GetAutoNumber(nameof(EmployeeDocument), PKGeneratorEnum.Auto, null, DateTime.Now);
                var pkCount = 0;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (entities != null)
                {
                    var dbList = Query(r => r.EmpSystemID == empSystemID).Select().ToList();
                    foreach (var item in entities)
                    {
                        // var loList = dbList.FirstOrDefault(r =>r.Id == item.Id);

                        if (item == null || string.IsNullOrEmpty(item.Id))
                        {
                            pkCount++;
                            item.Id = pk + "-" + pkCount;
                            item.FileId = item.Id;
                            item.FileName = item.FileName;
                            item.UpdatedBy = identity.Name;
                            item.UpdatedDate = DateTime.Now;
                            Insert(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetDocumentData(string companyGroupId, string budgetId, string plantId, string pId)
        {
            try
            {
                var sql = @"SELECT DISTINCT ED.*,
									CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,CD.ProfileType,CD.DocNumberRequired,CD.DocDateRequired
									,E.UserName AS EmployeeCategory,CD.DocumentationBy
								FROM dbo.EmployeeDocument ED
								LEFT JOIN hkp.ComplianceDocument CD ON ED.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CD.Id = CDSD.ComplianceDocumentId
								LEFT JOIN (select  * from hkp.DocumentConfigurationDesignationGroup

								Where PlantId='" + plantId + @"' and EmployeeCategoryId = (
										SELECT D.EmployeeCategoryId
										FROM (SELECT * FROM MST.DesignationMaster WHERE CompanyGroupId = '" + companyGroupId + @"'
											) AS D
										LEFT JOIN EmployeeInformation EI ON D.DesignationId = EI.GivenDesignationId
										WHERE EI.BudgetCode = '" + budgetId + @"'
											AND EI.SystemId = '" + pId + @"'
										)
								)DD ON CDSD.ComplianceDocumentSetId = DD.ComplianceDocumentSetId
								LEFT JOIN HKP.EmployeeCategory AS E ON DD.EmployeeCategoryId = E.Id
								WHERE ED.EmpSystemID = '" + pId + @"'
									--AND CD.EmploymentStage = 'PreRecruitment'
									--AND CD.DocumentationBy = 'Department'
									AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND E.UserName IS NOT NULL";
                //AND PD.DueDate IS NOT NULL";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetAllEmployee(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                    str = @" AND EMP.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE PostRecruitmentOrgDocRP='" + employeeId + "'))";
                parameters.CmdText = @"Select EMP.*,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName
									   ,DEG.UserName GivenDesignation, DEPT.UserName AS Department
									 FROM EmployeeInformation EMP
									 LEFT OUTER JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
									 LEFT OUTER JOIN ORG.Position PR ON PMB.PositionId=PR.Id
									 LEFT OUTER JOIN HKP.Designation DEG on DEG.Id=EMP.GivenDesignationId
								     LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
									 LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id
									 LEFT OUTER JOIN HKP.Designation D ON PR.DesignationId=D.Id
									 Where EMP.GroupID='" + companyGroupId + @"' AND EMP.CompanyId='" + companyId + @"'"
                                    + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdatePostRecruitmentDocument(string id)
        {
            ExecuteSqlCommand("Update dbo.EmployeeDocument set FileName=NULL Where Id='" + id + "'");
        }

        public void InsertORUpdate(EmployeeDocument entity)
        {
            try
            {
                if (!string.IsNullOrEmpty(entity.FileName))
                {
                    var id = Query(t => t.Id != entity.Id && t.EmpSystemID == entity.EmpSystemID && t.FileName == entity.FileName).Select(t => t.Id).FirstOrDefault();
                    if (id != null) throw new CustomException("This file is already exists!!!");
                }

                if (entity != null)
                {
                    var dbdata = Find(entity.Id);
                    dbdata.FileId = entity.Id;
                    dbdata.FileName = entity.FileName;
                    dbdata.DocDate = entity.DocDate;
                    dbdata.DocNumber = entity.DocNumber;
                    dbdata.UpdatedDate = DateTime.Now;
                    Update(dbdata);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
    }
}
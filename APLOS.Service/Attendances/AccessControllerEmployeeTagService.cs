#region Using

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Attendances
{
    public class AccessControllerEmployeeTagService : Service<AccessControllerEmployeeTag>, IAccessControllerEmployeeTagService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IAccessControllerDeleteRequestService _d;

        public AccessControllerEmployeeTagService(
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
                              LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EMP.BudgetCode
						      LEFT JOIN ORG.Position p ON p.Id=MB.PositionId
							  LEFT JOIN ORG.Department DEPT ON DEPT.Id=P.DepartmentId
        					  LEFT JOIN HKP.Designation DEG ON DEG.Id=EMP.GivenDesignationId
        					  LEFT JOIN ORG.Plant P ON P.Id=EMP.PlantId
        					  LEFT JOIN ORG.Section S ON S.Id=P.SectionId
        					  LEFT JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
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

        
    }
}
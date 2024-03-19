#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Organizations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class RecruitmentApprovalService : Service<EmployeeInformation>, IPreRecruitmentApprovalService
    {
        #region Constructor

        private readonly IManpowerBudgetService _manpowerBudgetService;
        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRecruitmentSelectionService prerecruitmentemployeeservice;
        private readonly IEmpReferenceInformationService empreferenceinformationservice;
        private readonly IEmpAcademicQualificationInformationService empAcademicQualificationInformationService;
        private readonly IEmpExperienceInformationService empExperienceInformationService;
        private readonly IEmpTrainingInformationService empTrainingInformationService;
        private readonly IEmployeeDocumentService employeeDocumentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<EmployeeInformation> _recruitmentSelectionRepository;
        private readonly IRepositoryAsync<PreRecruitmentEmployee> _recruitmentEmpRepository;
        private readonly IRepositoryAsync<DesignationMaster> _designationMasterRepository;
        private readonly IRepositoryAsync<Plant> _plantRepository;
        private readonly IPlantService _plantService;
        private readonly IPreRecruitmentDocumentService _preRecruitmentDocumentService;
        private readonly IEmployeeDocumentAssignmentService _employeeDocumentAssignmentService;

        public RecruitmentApprovalService(
            IRepositoryAsync<EmployeeInformation> recruitmentSelectionRepository
            , IRepositoryAsync<PreRecruitmentEmployee> recruitmentEmpRepository
            , IPKGeneratorService pkGeneratorService
            , ISignatureService signatrueService
            , IRecruitmentSelectionService _prerecruitmentemployeeservice
            , IEmpReferenceInformationService _empreferenceinformationservice
            , IEmpAcademicQualificationInformationService _empAcademicQualificationInformationService
            , IEmpExperienceInformationService _empExperienceInformationService
            , IEmpTrainingInformationService _empTrainingInformationService
            , IManpowerBudgetService manpowerBudgetService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<DesignationMaster> designationMasterRepository
            , IEmployeeDocumentService _employeeDocumentService
            , IRepositoryAsync<Plant> plantRepository
            , IPreRecruitmentDocumentService preRecruitmentDocumentService
            , IPlantService plantService
            , IEmployeeDocumentAssignmentService employeeDocumentAssignmentService
            ) : base(recruitmentSelectionRepository, unitOfWork, pkGeneratorService)
        {
            _designationMasterRepository = designationMasterRepository;
            _recruitmentEmpRepository = recruitmentEmpRepository;
            _recruitmentSelectionRepository = recruitmentSelectionRepository;
            _unitOfWork = unitOfWork;
            _signatrueService = signatrueService;
            prerecruitmentemployeeservice = _prerecruitmentemployeeservice;
            empreferenceinformationservice = _empreferenceinformationservice;
            empAcademicQualificationInformationService = _empAcademicQualificationInformationService;
            empExperienceInformationService = _empExperienceInformationService;
            empTrainingInformationService = _empTrainingInformationService;
            _manpowerBudgetService = manpowerBudgetService;
            _sqlRepository = sqlRepository;
            employeeDocumentService = _employeeDocumentService;
            _plantRepository = plantRepository;
            _preRecruitmentDocumentService = preRecruitmentDocumentService;
            _plantService = plantService;
            _employeeDocumentAssignmentService = employeeDocumentAssignmentService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return _signatrueService.GetAutoNumber("EMP_BASIC", DateTime.Now).ToString();
        }

        private static string GetPadding(string iv)
        {
            while (iv.Length < 5)
            {
                iv = "0" + iv;
            }
            return iv;
        }

        public GridModel GetData(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId)
        {
            try
            {//AND PRE.Submitted=1  AND PRE.IsDepartmentSubmit=1 AND PRE.IsApproved=1
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                    str = @" AND PRE.BudgetId IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE RecruitmentFinalConfirmationRP='" + employeeId + "'))";
                parameters.CmdText = @"SELECT PRE.*
                                           ,PS.IsEmployeeCodeOpenField
									       ,PR.UserName Position
									       ,d.UserName GivenDesignation
										   ,DEG.UserName Designation ,PR.DesignationId,dm.UserName DesignationMaster
									       ,dm.Id DesignationMasterId,dm.DesignationGroupId,dg.UserName DesignationGroup
									       ,et.UserName EmployeeCategory
									       ,E.UserName EntityName, DEPT.UserName AS Department, 0 Active
									    FROM PreRecruitmentEmployee PRE
									    LEFT JOIN [MST].[ManpowerBudget] PMB ON PRE.BudgetId=PMB.Id
									    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
										LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
										LEFT JOIN HKP.Designation DEG ON PR.DesignationId=DEG.Id
									    LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
									    LEFT JOIN HKP.Designation d on d.Id=PRE.GivenDesignationId
									    LEFT JOIN MST.DesignationMaster dm on dm.DesignationId=d.Id
									    LEFT JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									    LEFT JOIN HKP.[EmployeeCategory] et on et.Id=dm.EmployeeCategoryId
                                        LEFT JOIN PlantWiseHRMSSetting PS on PS.PlantID=PRE.PlantId
									    Where PRE.GroupID='" + companyGroupId + @"' AND PRE.CompanyId='" + companyId + @"' AND PRE.Completed=0" + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public EmployeeInformation GetMaster(string PK)
        {
            var _sql = "SELECT * FROM EmployeeInformation WHERE SystemId='" + PK + "'";
            return _recruitmentSelectionRepository.SelectQuery(_sql, null).FirstOrDefault();
        }

        public IEnumerable<EmployeeInformation> GetMasterlist(string PKs)
        {
            var _sql = "SELECT * FROM EmployeeInformation WHERE SystemId IN (" + PKs + ")";
            return _recruitmentSelectionRepository.SqlQuery<EmployeeInformation>(_sql).AsEnumerable();
        }

        private Dictionary<string, object> GetBudgetInfo(string Id)
        {
            return _manpowerBudgetService.GetManpowerBudgetById(Id);
        }

        private static string GetValue(Dictionary<string, object> dic, string key)
        {
            string value = null;
            if (dic.ContainsKey(key))
            {
                value = dic[key].ToString();
            }


            if (string.IsNullOrEmpty(value))
            {
                value = null;
            }
            return value;
        }

        public string GetDesignationGroup(string designationId)
        {
            var _sql = "SELECT DesignationGroupId FROM mst.DesignationMaster WHERE DesignationId='" + designationId + "'";
            return _designationMasterRepository.SqlQuery<string>(_sql).FirstOrDefault();
        }

        public string GetEmployeeCategoryByDesignation(string designationId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var _sql = @" SELECT B.EmployeeCategoryId
                        FROM [HKP].DesignationGroup A
                         INNER JOIN (SELECT * FROM [MST].[DesignationMaster] where DesignationId='" + designationId + @"' and  CompanyGroupId='" + identity.CompanyGroupId + @"')B
                        ON A.Id = B.DesignationGroupId
                        left outer join HKP.EmployeeCategory t on t.Id=B.EmployeeCategoryId ";
            return _designationMasterRepository.SqlQuery<string>(_sql).FirstOrDefault();
        }

        private void InitBudgetCode(Dictionary<string, object> dic, ref EmployeeInformation bc)
        {
            bc.UnitID = GetValue(dic, "UnitId");
            bc.DivisionID = GetValue(dic, "DivisionId");
            bc.DepartmentID = GetValue(dic, "DepartmentId");
            bc.SectionID = GetValue(dic, "SectionId");
            bc.SubSectionID = GetValue(dic, "SubSectionId");
            bc.SubdivisionID = GetValue(dic, "SubdivisionId");
            bc.LineID = GetValue(dic, "LineId");
            bc.DesignationSystemID = GetValue(dic, "DesignationId");
            bc.DesignationGroupID = GetDesignationGroup(bc.DesignationSystemID);
            bc.EmployeeCategorySystemID = GetEmployeeCategoryByDesignation(bc.DesignationSystemID);
            bc.EmployeeGroupSystemID = GetValue(dic, "EmployeeGroupId");
            bc.EmploymentType = GetValue(dic, "EmploymentType");
            //bc.PaymentLink = GetValue(dic, "PaymentLink");
        }

        public static bool IsDateOK(string strdate)
        {
            try
            {
                if (strdate.Length != 11)
                {
                    return false;
                }
                if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }// end function

        private void InitData(string EmployeeId, PreRecruitmentEmployee item, out EmployeeInformation local_ob)
        {
            local_ob = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            local_ob = new EmployeeInformation
            {
                ModelState = ModelState.Added
            };
            AuditService.Log(local_ob);
            var pk = GetPK();
            var pad = GetPadding(Convert.ToInt32(pk).ToString());

            local_ob.SystemId = DateTime.Now.ToString("yy") + pad;//GetPK

            local_ob.PreRecruitmentEmployeeId = item.Id;

            local_ob.EmployeeId = EmployeeId;
            local_ob.EmployeeCode = item.EmployeeCode;

            //if (string.IsNullOrEmpty(item.EmployeeCode))
            //{
            //    var autoCode = getEmpCodeAuto(item.PlantId);
            //    var startValue = GetEmpCodeStartValue(item.PlantId);
            //    if (autoCode.Tables[0].Rows.Count > 0)
            //    {
            //        int v = Convert.ToInt32(autoCode.Tables[0].Rows[0]["c"].ToString()) + 1;
            //        if (v == 1)
            //        {
            //            if (Convert.ToInt32(startValue.Tables[0].Rows[0]["EmpCodeStartValue"].ToString()) != 0)
            //            {
            //                int code = Convert.ToInt32(startValue.Tables[0].Rows[0]["EmpCodeStartValue"].ToString()) + 1;
            //                local_ob.EmployeeCode = code.ToString();
            //            }
            //            else
            //            {
            //                Exception ex = new Exception("Employee code start value doesn't define in plant wise setting...");
            //                throw (ex);
            //            }
            //        }
            //        else
            //        {
            //            local_ob.EmployeeCode = v.ToString();
            //        }
            //    }
            //}

            //if (string.IsNullOrEmpty(item.EmployeeCode))
            //{
            //    local_ob.EmployeeCode = EmployeeId;
            //}
            //else
            //{
            //    local_ob.EmployeeCode = item.EmployeeCode;
            //}

            MoveImage(item.Image, local_ob.SystemId + ".jpg");
            local_ob.EmpPicPath = local_ob.SystemId + ".jpg";
            local_ob.GivenDesignationId = item.GivenDesignationId;
            local_ob.LegalDesignationId = item.LegalDesignationId;

            local_ob.BloodGroupID = item.BloodGroupID;
            local_ob.BudgetCode = item.BudgetId;
            local_ob.CitizenID = item.CitizenID;
            local_ob.CivilStatusID = item.CivilStatusID;
            local_ob.CompanyId = identity.CompanyId;

            Dictionary<string, object> dic;
            dic = GetBudgetInfo(item.BudgetId);

            InitBudgetCode(dic, ref local_ob);
            local_ob.DOB = item.DOB;
            local_ob.DOJ = item.DOJ;
            local_ob.IsSlvDevReg = "No";

            local_ob.EmailId = item.Email;
            local_ob.EmployeeName = item.EmployeeName;
            local_ob.EmpType = item.EmpType;
            local_ob.EmrCntPer1CellNo = item.EmrCntPer1CellNo;
            local_ob.EmrCntPer1Name = item.EmrCntPer1Name;

            local_ob.EmrCntPer2CellNo = item.EmrCntPer2CellNo;
            local_ob.EmrCntPer2Name = item.EmrCntPer2Name;
            local_ob.FatherName = item.FatherName;
            local_ob.FirstName = item.FirstName;

            local_ob.GenderID = item.Gender;
            local_ob.GroupID = item.GroupID;
            local_ob.LastName = item.LastName;

            local_ob.MiddleName = item.MiddleName;
            local_ob.MotherName = item.MotherName;
            local_ob.NationalID = item.NationalID;

            local_ob.NickName = item.NickName;
            local_ob.NoOfChildren = item.NoOfChildren;
            local_ob.ParmanentAddress1 = item.ParmanentAddress1;
            local_ob.ParmanentAddress2 = item.ParmanentAddress2;

            local_ob.ParmAreaID = item.ParmAreaID;
            local_ob.ParmCityID = item.ParmCityID;
            local_ob.ParmCountryID = item.ParmCountryID;
            local_ob.ParmDistrictID = item.ParmDistrictID;
            local_ob.ParmStateId = item.ParmStateId;
            local_ob.ParmPostOfficeID = item.ParmPostOfficeID;
            local_ob.ParmanentArea = item.ParmanentArea;

            local_ob.ParmThanaID = item.ParmThanaID;
            local_ob.ParmZipCode = item.ParmZipCode;
            local_ob.CellPhnNo = item.Phone;
            local_ob.PlantID = item.PlantId;
            local_ob.PositionId = item.PositionID;

            local_ob.PresAreaID = item.PresAreaID;
            local_ob.PresCityID = item.PresCityID;
            local_ob.PresCountryID = item.PresCountryID;
            local_ob.PresDistrictID = item.PresDistrictID;
            local_ob.PresStateId = item.PresStateId;
            local_ob.PresentAddress1 = item.PresentAddress1;
            local_ob.PresentArea = item.PresentArea;

            local_ob.PresentAddress2 = item.PresentAddress2;
            local_ob.PresPostOfficeID = item.PresPostOfficeID;
            local_ob.PresThanaID = item.PresThanaID;
            local_ob.PresZipCode = item.PresZipCode;

            local_ob.ReligionID = item.ReligionID;
            local_ob.Salutation = item.Salutation;
            local_ob.SpouseName = item.SpouseName;

            local_ob.SpouseNationalID = item.SpouseNationalID;
            local_ob.SpouseOccupation = item.SpouseOccupation;
            local_ob.EmployeeStatus = "Active";

            local_ob.TIN = item.TIN;
            local_ob.AgreedDOJ = item.AgreedDOJ;
            local_ob.TotalSalary = item.TotalSalary;
            local_ob.SpecialReviewDuration = item.SpecialReviewDuration;
            local_ob.SpecialReviewAmount = item.SpecialReviewAmount;

            local_ob.EmrCntPer1CellNo2 = item.EmrCntPer1CellNo2;
            local_ob.EmrCntPer1CellNo3 = item.EmrCntPer1CellNo3;
            local_ob.EmrCntPer2CellNo2 = item.EmrCntPer2CellNo2;
            local_ob.EmrCntPer2CellNo3 = item.EmrCntPer2CellNo3;

            local_ob.ApprovedBy = item.ApprovedBy;
            local_ob.ApprovedDateTime = item.ApprovedDateTime;
            //local_ob.IsApproved = item.IsApproved;
            local_ob.IsApproved = false;
            local_ob.IsImage = item.IsImage;
            local_ob.ApplyingAsFresher = item.ApplyingAsFresher;
            local_ob.IsKnownPerson = item.IsKnownPerson;
            local_ob.NumberOfKnownPerson = item.NumberOfKnownPerson;

            local_ob.BirthdayCelebrationDate = item.BirthdayCelebrationDate;
            local_ob.MarriagedayCelebrationDate = item.MarriagedayCelebrationDate;

            local_ob.DOCIsDay = true;
            local_ob.DOCDay = item.ConfirmAfterDays;
            local_ob.DOCIsMonth = false;
            local_ob.DOCMonth = 0;
            local_ob.OperationMasterID = item.OperationMasterID;

            local_ob.DateAdded = DateTime.Now;
            local_ob.DateUpdated = DateTime.Now;
            local_ob.AddedBy = identity.Name;
            local_ob.UpdatedBy = identity.Name;

            //local_ob.InitialPIN = new Random().Next(111111, 999999).ToString();
        }

        public DataSet getEmpCodeAuto(string plantId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter

            {
                ExportType = "DATASET",
                CmdText = @" SELECT max(CAST(EmployeeCode AS int)) c  from EmployeeInformation
                            WHERE plantid='" + plantId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet getEmpCodeAutoPreRecruitment(string plantId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter

            {
                ExportType = "DATASET",
                //CmdText = @" SELECT max(CAST(EmployeeCode AS int)) c  from PreRecruitmentEmployee
                //            WHERE plantid='" + plantId + "'"
                CmdText = @"SELECT ISNULL(c, 0 ) c from (SELECT max(CAST(EmployeeCode AS int)) c from PreRecruitmentEmployee
                            WHERE PlantId='" + plantId + @"')a"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetEmpCodeStartValue(string plantId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter

            {
                ExportType = "DATASET",
                CmdText = @"SELECT EmpCodeStartValue FROM PlantWiseHRMSSetting WHERE PlantId='" + plantId + @"'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet PlantWiseDOJ(string plantId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT  IsPastDOJAllowed FROM dbo.PlantWiseHRMSSetting WHERE PlantId='" + plantId + @"' AND IsPastDOJAllowed=1"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet PlantWiseDOJDays(string plantId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT  PastDOJDaysAllowed FROM dbo.PlantWiseHRMSSetting WHERE PlantId='" + plantId + @"' AND IsPastDOJAllowed=1"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public void InsertORUpdate(IEnumerable<PreRecruitmentEmployee> entities)
        {
            var pks = string.Empty;
            var flag = false;
            EmployeeInformation local_ob = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                prerecruitmentemployeeservice.GetPKList(entities, out pks);
                var from_dblist = prerecruitmentemployeeservice.GetMasterlist(pks);
                var from_ui_ref = empreferenceinformationservice.GetPreReferenceList(pks);
                var from_ui_qua = empAcademicQualificationInformationService.GetPreRecruitmentEmpQualificationList(pks);
                var from_ui_exp = empExperienceInformationService.GetPreRecruitmentEmpExperienceList(pks);
                var from_ui_train = empTrainingInformationService.GetPreRecruitmentEmpTrainingList(pks);
                var from_ui_doc = employeeDocumentService.GetPreRecruitmentDocumentList(pks);

                string _EmployeeId;
                var _plantId = string.Empty;
                var ids = entities.First().Id;
                foreach (var item in entities)
                {
                    _plantId = item.PlantId;
                }
                var pk = _signatrueService.GetMaxNumber(_plantId + "EMP_BASIC", DateTime.Now);
                var prefix = _plantService.GetPlantPrefix(_plantId);
                if (string.IsNullOrEmpty(prefix))
                    throw new Exception("No prefix found for this plant.");
                var count = 0;
                var plantName = _plantRepository.Query(t => t.Id == _plantId).Select(t => t.UserName).FirstOrDefault();
                var emp = PlantWiseDOJ(_plantId);
                var nodays = PlantWiseDOJDays(_plantId);
                foreach (var item in entities)
                {
                    count++;
                    if (item.Active)
                    {
                        if (item.DOJ < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                        {
                            if (emp.Tables[0].Rows.Count > 0)
                            {
                                var start = DateTime.Now;
                                var end = Convert.ToDateTime(item.DOJ);

                                TimeSpan difference = start - end;
                                var days = Convert.ToInt32(difference.Days);
                                var date = Convert.ToInt32(nodays.Tables[0].Rows[0]["PastDOJDaysAllowed"]);
                                if (date < days)
                                {
                                    throw new Exception("Maximum  " + nodays.Tables[0].Rows[0]["PastDOJDaysAllowed"] + " days back is allowed for DOJ.");
                                }
                                //allowed
                            }
                            else
                            {
                                throw new Exception("Previous  Date of Join  for candidate(" + item.Id + ") plant " + plantName + " is not allowed");
                            }
                        }
                        else if (item.DOJ > Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                        {
                            //future
                            throw new Exception("Future  Date of Join  for candidate(" + item.Id + ") plant " + plantName + " is not allowed");
                        }
                        else
                        {
                            //Current
                        }

                        #region PreRecruitmentEmployee update status completed

                        var db = from_dblist.FirstOrDefault(a => a.Id == item.Id);
                        var empList = new List<EmployeeInformation>();
                        if (db != null)
                        {
                            db.ModelState = ModelState.Modified;
                            db.DOJ = item.DOJ;
                            pk.LastNumber++;
                            var pad = GetPadding(Convert.ToInt32(pk.LastNumber).ToString());
                            _EmployeeId = prefix + DateTime.Now.ToString("yy") + pad;
                            db.EmployeeId = _EmployeeId;

                            if (string.IsNullOrEmpty(item.EmployeeCode))
                            {
                                var autoCode = getEmpCodeAutoPreRecruitment(item.PlantId);

                                var startValue = GetEmpCodeStartValue(item.PlantId);
                                if (autoCode.Tables[0].Rows.Count > 0)
                                {
                                    int v = Convert.ToInt32(autoCode.Tables[0].Rows[0]["c"].ToString()) + 1;
                                    if (v == 1)
                                    {
                                        if (Convert.ToInt32(startValue.Tables[0].Rows[0]["EmpCodeStartValue"].ToString()) != 0)
                                        {
                                            int code = Convert.ToInt32(startValue.Tables[0].Rows[0]["EmpCodeStartValue"].ToString()) + 1;
                                            item.EmployeeCode = code.ToString();
                                        }
                                        else
                                        {
                                            Exception ex = new Exception("Employee code start value doesn't define in plant wise setting...");
                                            throw (ex);
                                        }
                                    }
                                    else
                                    {
                                        item.EmployeeCode = v.ToString();
                                    }
                                }
                            }

                            //if (string.IsNullOrEmpty(item.EmployeeCode))
                            //{
                            //    db.EmployeeCode = prefix + DateTime.Now.ToString("yy") + pad;
                            //}
                            //else
                            //{
                            //    db.EmployeeCode = item.EmployeeCode;
                            //}
                            db.ConfirmationStatus = item.ConfirmationStatus;
                            db.LegalDesignationId = item.LegalDesignationId;
                            db.EmployeeCode = item.EmployeeCode;
                            if (item.ConfirmationStatus == ConfirmationStatus.Selected.ToString())
                            {
                                db.Completed = true;
                                db.GivenDesignationId = item.GivenDesignationId;
                                db.LegalDesignationId = item.LegalDesignationId;
                                db.ConfirmationStatus = item.ConfirmationStatus;
                                db.ConfirmationBy = identity.Name;
                                db.ConfirmationDate = DateTime.Now;
                                db.ConfirmAfterDays = item.ConfirmAfterDays;
                                db.UpdatedBy = identity.Name;
                                db.UpdatedDate = DateTime.Now;
                            }

                            #region EmployeeInformation insert full row

                            InitData(_EmployeeId, db, out local_ob);

                            AuditService.Log(db);
                            prerecruitmentemployeeservice.UpdateGraph(db);

                            InsertOrUpdateGraph(local_ob);

                            empList.Add(local_ob);

                            #endregion EmployeeInformation insert full row
                        }

                        #endregion PreRecruitmentEmployee update status completed

                        #region EmpReferenceInformation

                        var reflist = from_ui_ref.Where(a => a.PreRecruitmentEmployeeId == item.Id);
                        empreferenceinformationservice.SaveList(local_ob.SystemId, item.Id);

                        #endregion EmpReferenceInformation

                        #region EmpAcademicQualificationInformation

                        var qualist = from_ui_qua.Where(a => a.PreRecruitmentEmployeeId == item.Id);
                        empAcademicQualificationInformationService.SaveList(local_ob.SystemId, item.Id);

                        #endregion EmpAcademicQualificationInformation

                        #region EmpExperienceInformation

                        var explist = from_ui_exp.Where(a => a.PreRecruitmentEmployeeId == item.Id);
                        empExperienceInformationService.SaveList(local_ob.SystemId, item.Id);

                        #endregion EmpExperienceInformation

                        #region EmpTrainingInformation

                        var trainlist = from_ui_train.Where(a => a.PreRecruitmentEmployeeId == item.Id);
                        empTrainingInformationService.SaveList(local_ob.SystemId, item.Id);

                        #endregion EmpTrainingInformation

                        #region EmployeeDocument

                        var doclist = from_ui_doc.Where(a => a.PreRecruitmentEmployeeId == item.Id);
                        foreach (var doc in doclist)
                        {
                            doc.IsCopied = true;
                            _preRecruitmentDocumentService.UpdateGraph(doc);
                        }
                        employeeDocumentService.SaveList(local_ob.SystemId, item.Id);

                        //var plantId = entities.Select(t => t.PlantId).FirstOrDefault();
                        //var empType = entities.Select(t => t.EmpType).FirstOrDefault();

                        //employeeDocumentService.InitPostDocument(empList, plantId, empType);
                        _employeeDocumentAssignmentService.InsertORUpdateMaster(empList);

                        #endregion EmployeeDocument
                    }
                }
                _unitOfWork.BeginTransaction();
                flag = true;

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> GetGivenDesignationCbo(string GroupId)
        {
            try
            {
                var sql = @"SELECT A.Id AS [Value], A.UserName AS [Text] FROM [HKP].[Designation] A
                INNER JOIN (SELECT * FROM [hkp].[CompanyGroupDesignation] WHERE CompanyGroupId = '" + GroupId + @"')B
				ON A.Id = B.DesignationId
				INNER JOIN (SELECT * FROM [MST].[DesignationMaster] WHERE CompanyGroupId = '" + GroupId + @"') m
				 ON A.Id = m.DesignationId
                ORDER BY A.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        
        public GridModel GetLegalDesignationCbo(GridParameter parameters,string companyGroupId, string plantId, string BudgetCode)
        {
            try
            {
              
//                parameters.CmdText = @"SELECT A.Id, A.Sequence,A.Code,A.ShortName,A.StandardName,A.UserName FROM [HKP].[LegalDesignation] A                           
//                         WHERE a.id in
//                         (
//                             SELECT LegalDesignationId FROM [HKP].[CompanyGroupLegalDesignation] WHERE CompanyGroupId = '" + companyGroupId + @"'
//                         ) AND A.Active=1 AND A.Id in (select LegalDesignationId from [MST].[DesignationMasterLegalDesignation] where DesignationMasterId=
//(select Id from MST.DesignationMaster where 
//DesignationId=(select DesignationId from  ORG.Position where Id=(select PositionId from mst.ManpowerBudget where id='"+BudgetCode+@"'))))
//                         AND A.Id IN (SELECT LegalDesignationId FROM [MST].[LegalSalaryGradeDesignation] WHERE PlantId='" + plantId + "')";

                parameters.CmdText = @"SELECT A.Id, A.Sequence,A.Code,A.ShortName,A.StandardName,A.UserName FROM [HKP].[LegalDesignation] A                           
                         WHERE a.id in
                         (
                             SELECT LegalDesignationId FROM [HKP].[CompanyGroupLegalDesignation] WHERE CompanyGroupId = '" + companyGroupId + @"'
                         ) AND A.Active=1 
                         AND A.Id IN (SELECT LegalDesignationId FROM [MST].[LegalSalaryGradeDesignation] WHERE PlantId='" + plantId + "')";
                //return _sqlRepository.GetDataCollection(sql, null);
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public GridModel GetDesignationCbo(GridParameter parameters, string companyGroupId, string plantId, string BudgetCode)
        {
            try
            { 
                parameters.CmdText = @"SELECT Id, Sequence,Code,ShortName,StandardName,UserName FROM [HKP].[Designation]                            
                         WHERE Active=1";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }


        public IEnumerable<object> GetLegalDesignationCbobyGivenDesignation(string givenDesignationpId)
        {
            try
            {
                var sql = @"SELECT LD.Id AS [Value],LD.UserName AS [Text] FROM [HKP].[LegalDesignation] LD
                          LEFT JOIN [MST].[DesignationMasterLegalDesignation] LDM ON ldm.LegalDesignationId=LD.Id
                          LEFT JOIN MST.DesignationMaster DM ON DM.Id=LDM.DesignationMasterId
                          WHERE DM.Designationid='" + givenDesignationpId + "'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public static void MoveImage(string fromName, string toName)
        {
            var Fromdirectory = ResourcesPathReader.GetEmployeePicPath();
            //new AppSettingsReader().GetValue("USERPIC_SOURCE", typeof(string)).ToString(); //get pic from web config
            var Todirectory = ResourcesPathReader.GetEmployeeDestinationPicPath();
            //new AppSettingsReader().GetValue("USERPIC_DESTINATION", typeof(string)).ToString();
            //if (!System.IO.Directory.Exists(Fromdirectory)) //CreateDirectory
            //    System.IO.Directory.CreateDirectory(Fromdirectory);
            if (!string.IsNullOrEmpty(fromName))
            {
                var path = Path.Combine(Fromdirectory, fromName);
                if (File.Exists(path))
                {
                    File.Copy(Path.Combine(Fromdirectory, fromName), Path.Combine(Todirectory, toName), true);
                }
            }
        }
    }
}
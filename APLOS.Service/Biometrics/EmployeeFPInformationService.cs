using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Model.Employees;
using Library.Service.Attendances;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.Systems;
using Library.ViewModel.HR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Library.Service.Biometrics
{
    public class EmployeeFPInformationService : Service<EmployeeFPInformation>, IEmployeeFPInformationService
    {
        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<EmployeeFPInformation> _r;
        private readonly IRepositoryAsync<EmployeeInformation> _eir;
        private readonly IAccessControllerEmployeeTagService _tg;
        private readonly EmployeeInformationService _ei;

        public EmployeeFPInformationService(
            IRepositoryAsync<EmployeeFPInformation> PreRecruitmentEmpReferenceRepository
            , IRepositoryAsync<EmployeeInformation> eir
            , ISignatureService signatrueService
            , IPKGeneratorService pkGeneratorService
            , IAccessControllerEmployeeTagService tg
            , EmployeeInformationService ei
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) : base(PreRecruitmentEmpReferenceRepository, unitOfWork, pkGeneratorService)
        {
            _signatrueService = signatrueService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _tg = tg;
            _eir = eir;
            _ei = ei;
            _r = PreRecruitmentEmpReferenceRepository;
        }

        private string GetPK()
        {
            return _signatrueService.GetAutoNumber("E_FINGERPRINT_I", DateTime.Now).ToString();
        }

        private IEnumerable<EmployeeFPInformation> Getlist(string empid)
        {
            try
            {
                var _sql = "SELECT * FROM EmployeeFPInformation WHERE EmpSystemID ='" + empid + "'";
                return _sqlRepository.GetModelCollection<EmployeeFPInformation>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<AccessControllerEmployeeTag> GetAccessControllerEmployeeTaglist(string empid)
        {
            try
            {
                var _sql = "SELECT * FROM AccessControllerEmployeeTag WHERE EmpInfoSystemID ='" + empid + "'";
                return _sqlRepository.GetModelCollection<AccessControllerEmployeeTag>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<EmployeeInformation> GetEmployeeInformationSameCard(string empid, string cardid)
        {
            try
            {
                var _sql = "SELECT * FROM EmployeeInformation WHERE CardNumber ='" + cardid + "' and SystemId<>'" + empid + "' and EmployeeStatus='Active'";
                return _sqlRepository.GetModelCollection<EmployeeInformation>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public EmployeeFPInformation GetEmployeeFPInformation(string PK, string FingerName, bool IsLeft)
        {
            try
            {
                var sql = "select * from EmployeeFPInformation where EmpSystemId='" + PK + "' and FingerName='" + FingerName + "' and IsLeft='" + IsLeft + "'";
                return _r.SqlQuery<EmployeeFPInformation>(sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public EmployeeInformation GetEmployeeInformationValidation(string PK)
        {
            try
            {
                var sql = "select * from EmployeeInformation where SystemId='" + PK + "' ";
                return _eir.SqlQuery<EmployeeInformation>(sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private EmployeeInformation xGetEmployeeInformationValidation(string PK)
        {
            try
            {
                string sql = "select * from EmployeeInformation where SystemId='" + PK + "' ";
                return _sqlRepository.GetModelCollection<EmployeeInformation>(sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitData(EmployeeFPInformation ui, out EmployeeFPInformation db)
        {
            db = null;
            try
            {
                db = GetEmployeeFPInformation(ui.EmpSystemId, ui.FingerName, ui.IsLeft);
                if (db == null || string.IsNullOrEmpty(db.Id))
                {
                    db = new EmployeeFPInformation
                    {
                        Id = "EFP"+DateTime.Now.ToString("yy")+"-" + GetPK(),
                        EmpSystemId = ui.EmpSystemId,
                        FingerName = ui.FingerName,
                        FPId = ui.FPId,
                        FPImage = ui.FPImage,
                        FPTemplate = ui.FPTemplate,
                        IsLeft = ui.IsLeft,
                        ModelState = ModelState.Added,
                        AddedBy = "System",
                        AddedDate = DateTime.Now,
                        AddedFromIP = "::1"
                    };
                }
                else
                {
                    db.FingerName = ui.FingerName;
                    db.FPId = ui.FPId;
                    db.FPImage = ui.FPImage;
                    db.FPTemplate = ui.FPTemplate;
                    db.IsLeft = ui.IsLeft;
                    db.ModelState = ModelState.Modified;
                    db.UpdatedBy = "System";
                    db.UpdatedDate = DateTime.Now;
                    db.UpdatedFromIP = "::1";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitEmp(string Id)
        {
            //emp if fp is false avoid the below code
            var emp = GetEmployeeInformationValidation(Id);
            if (emp.RegisterFP)
            {
                //get from db
                var dblist = GetAccessControllerEmployeeTaglist(Id);
                foreach (var db in dblist)
                {
                    if (db == null || string.IsNullOrEmpty(db.Id))
                    {
                    }
                    else
                    {
                        //db.RegisterStatus = test;
                        db.RegisterStatus = "Requested";
                        db.ModelState = ModelState.Modified;
                        db.UpdatedBy = "System";
                        db.UpdatedDate = DateTime.Now;
                        db.UpdatedFromIP = "::1";
                        _tg.InsertOrUpdateGraph(db);
                    }
                }//foreach
            }//RegisterFP
        }

        private void InitEmpTagProximity(string Id)
        {
            try
            {
                //emp if fp is false avoid the below code
                var emp = GetEmployeeInformationValidation(Id);
                if (emp.RegisterProximate)
                {
                    //get from db
                    var dblist = GetAccessControllerEmployeeTaglist(Id);
                    foreach (var db in dblist)
                    {
                        if (db == null || string.IsNullOrEmpty(db.Id))
                        {
                        }
                        else
                        {
                            db.RegisterStatus = "Requested";

                            db.UpdatedBy = "System";
                            db.UpdatedDate = DateTime.Now;
                            db.UpdatedFromIP = "::1";
                            db.ModelState = ModelState.Modified;
                            _tg.UpdateGraph(db);
                        }
                    }//foreach
                }//RegisterProximate
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Save(EmployeeFPInformation ui)
        {
            var flag = false;
            EmployeeFPInformation db = null;
            try
            {
                InitData(ui, out db);

                MemoryStream stream = new MemoryStream(db.FPImage);
                System.Drawing.Image thn;
                thn = System.Drawing.Image.FromStream(stream);
                string _dir = ResourcesPathReader.GetEmployeeFingerPrintPath();
                string _path = _dir + db.FPId + ".bmp";

                _unitOfWork.BeginTransaction();
                InsertOrUpdateGraph(db);

                InitEmp(ui.EmpSystemId);

                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                thn.Save(_path, System.Drawing.Imaging.ImageFormat.Bmp);
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

        private void ProximityValidation(string empid, string cardnum)
        {
            try
            {
                var emplist = GetEmployeeInformationSameCard(empid, cardnum);
                string empname = string.Empty;
                foreach (var item in emplist)
                {
                    if (empname.Length == 0)
                    {
                        empname = item.EmployeeName;
                    }
                    else
                    {
                        empname += ", " + item.EmployeeName;
                    }
                }
                var _count = emplist.Count();
                if (_count > 0)
                {
                    throw new Exception("Employee" + (_count > 1 ? "s" : "") + " [" + empname + "] " + (_count > 1 ? "have" : "has") + " the same proximity card...");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitProximity(string Id, string CardNumber)
        {
            ProximityValidation(Id, CardNumber);
            //emp if fp is false avoid the below code
            // var emp = GetEmployeeInformationValidation(Id);
            // if (emp.RegisterProximate)
            //  {
            //get from db
            var db = GetEmployeeInformationValidation(Id);
            //var dblist = GetAccessControllerEmployeeTaglist(Id);
            //foreach (var db in dblist)
            //{
            if (db == null || string.IsNullOrEmpty(db.SystemId))
            {
                throw new Exception("No Employee found...");
            }
            else
            {
                if (string.IsNullOrEmpty(db.CardNumber))
                {
                    db.ProximityAddedBy = "System";
                    db.ProximityAddedDate = DateTime.Now;
                }
                else
                {
                    db.ProximityUpdatedBy = "System";
                    db.ProximityUpdatedDate = DateTime.Now;
                }

                db.CardNumber = CardNumber;
                db.ModelState = ModelState.Modified;
                //db.UpdatedFromIP = "::1";
                _ei.UpdateGraph(db);
            }
            // }//foreach
            //}//RegisterFP
        }

        public void SaveProximityCard(string empid, string cardid)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                InitProximity(empid, cardid);

                InitEmpTagProximity(empid);

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

        private byte[] Test(string empid)
        {
            string path = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath(), empid + ".jpg");
            if (File.Exists(path))
            {
                var webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(path);
                return imageBytes;
            }
            else
            {
                return null;
            }
        }

        private string Test2(string empid)
        {
            string path = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath(), empid + ".jpg");
            if (File.Exists(path))
            {
                var webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(path);
                return imageBytes.ToString();
            }
            else
            {
                return null;
            }
        }

        private string GetPicPath(string emppicpath)
        {
            //string path = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath(), empid + ".jpg");//GetROOT_FOLDER
            //string path = ResourcesPathReader.GetROOT_FOLDER() + "/EmployeeProfiles/EmpPic/" + empid + ".jpg";//GetROOT_FOLDER
            string path = ResourcesPathReader.GetROOT_FOLDER() + "/EmployeeProfiles/EmpPic/" + emppicpath;//GetROOT_FOLDER
                                                                                                          //if (System.IO.File.Exists(path))
                                                                                                          // path = path.Substring(path.IndexOf("/")+1);
#if DEBUG

#else
            path = path.Substring(path.IndexOf("/") + 1);
#endif
            return path;
        }

        public IEnumerable<EmployeeProfileVM> GetEmployeeInformation(string EmployeeCode, string plantid)//TBT
        {
            IEnumerable<EmployeeProfileVM> _returnlist = null;
            try
            {
                string _sql = @"SELECT
                                        e.SystemId,e.EmployeeCode,e.EmployeeName
                                        ,e.DOJ,e.DOB,e.DOC,e.DOS,e.EmpPicPath
                                        ,e.EmployeeStatus,isnull(e.CardNumber,'') CardNumber
                                        ,isnull(RegisterFP,0) RegisterFP,cg.PKPrefixField GroupPreFix
                                        ,isnull(RegisterProximate,0) RegisterProximate
                                        ,Replace(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                        ,Replace(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                        ,Replace(CONVERT(VARCHAR(11), e.DOC, 106), ' ', '-') DOC
                                        ,Replace(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-') DOS

                                        ,d.UserName Designation, gd.UserName GivenDesignation, dp.UserName Department
                                          , p.UserName Plant, c.UserName Company ,fp.FingerName,fp.FPImage,fp.IsLeft
                                        FROM EmployeeInformation e
                                        left outer join hkp.Designation d on e.DesignationSystemID = d.id
                                        left outer join hkp.Designation gd on e.GivenDesignationId = gd.id
                                        left outer join org.Department dp on dp.id = e.DepartmentId
                                        left outer join org.Plant p on p.Id = e.PlantId
                                        left outer join org.Company c on c.id = e.CompanyId
                                        left outer join org.CompanyGroup cg on cg.Id=e.GroupId
                                        left outer join [EmployeeFPInformation] fp on fp.EmpSystemId=e.SystemId
                                        where e.EmployeeCode = '" + EmployeeCode + "' and e.PlantId='" + plantid + "'";
                _returnlist = _sqlRepository.GetModelCollection<EmployeeProfileVM>(_sql, null);
                if (_returnlist.Count() > 0)
                {
                    //var v = Test2(PK); ;
                    foreach (var item in _returnlist)
                    {
                        item.ImageUrl = GetPicPath(item.EmpPicPath);
                    }
                }
                return _returnlist;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<EmployeeProfileVM> xGetAccessControllerList(string CompanyId)//TBT
        {
            IEnumerable<EmployeeProfileVM> _returnlist = null;
            try
            {
                string _sql = @"SELECT E.SystemID, ACR.DeviceSystemID, E.EmployeeCode, E.EmployeeName,
                                BDAC.MachineID, BDAC.MachineIP, ACR.RegisterStatus,
                                Remarks = CASE WHEN ACR.RegisterStatus = 'Registered' THEN 'Allready registered.'
                                                    ELSE '' END
													,E.CardNumber
													,fp.FPTemplate
													,fp.IsLeft
													,fp.FingerName

                                    ,BDAC.RegisCharacter, BDAC.RegisTypeDec, BDAC.RegisTypeHex, Isnull(E.RegisterProximate, 0) RegisterProximate, Isnull(E.RegisterFP, 0) RegisterFP
                            FROM AccessControllerEmployeeTag ACR
                                INNER JOIN EmployeeInformation E ON ACR.EmpInfoSystemID = E.SystemID
                                LEFT JOIN mst.AccessControllerList BDAC ON ACR.DeviceSystemID = BDAC.Id
								left outer join EmployeeFPInformation fp on fp.EmpSystemId=E.SystemId
                            WHERE ACR.RegisterStatus = 'Requested' AND (ACR.CompSystemID = '" + CompanyId + @"')
                            ORDER BY E.EmployeeCode, BDAC.MachineID";
                _returnlist = _sqlRepository.GetModelCollection<EmployeeProfileVM>(_sql, null);
                if (_returnlist.Count() > 0)
                {
                    var v = Test(CompanyId); ;
                    foreach (var item in _returnlist)
                    {
                        //item.FPImage = v;
                    }
                }
                return _returnlist;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetAccessControllerEmployeeUnTag(string plantid)
        {
            try
            {
                string _sql = @"SELECT E.SystemID, ACR.DeviceSystemID,
                                BDAC.MachineID, BDAC.MachineIP
                            FROM AccessControllerEmployeeTagDelete ACR
                                INNER JOIN EmployeeInformation E ON ACR.EmpInfoSystemID = E.SystemID
                                LEFT JOIN (select * from mst.AccessControllerList where isnull(IsActive,0)=1) BDAC ON ACR.DeviceSystemID = BDAC.Id
								
                            WHERE (ACR.plantid = '"+ plantid + @"')  and isnull(BDAC.MachineID,'')<>'' and acr.RegisterStatus='Requested'
                                   
                            ORDER BY E.EmployeeCode, BDAC.MachineID";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetAccessControllerEmployeeTag(string plantid)
        {
            try
            {
                string _sql = @"SELECT E.SystemID, ACR.DeviceSystemID, E.EmployeeCode, E.EmployeeName,
                                BDAC.MachineID, BDAC.MachineIP, ACR.RegisterStatus,
                                Remarks = CASE WHEN ACR.RegisterStatus = 'Registered' THEN 'Allready registered.'
                                                    ELSE '' END
													,E.CardNumber
													,fp.FPTemplate
													,ISNULL(fp.IsLeft,0) IsLeft
													,fp.FingerName

                                    ,BDAC.RegisCharacter, BDAC.RegisTypeDec, BDAC.RegisTypeHex
                                , Isnull(E.RegisterProximate, 0) RegisterProximate
                                , Isnull(E.RegisterFP, 0) RegisterFP
                            FROM AccessControllerEmployeeTag ACR
                                INNER JOIN (select * from EmployeeInformation where EmployeeStatus='Active') E ON ACR.EmpInfoSystemID = E.SystemID
                                LEFT JOIN (select * from mst.AccessControllerList where isnull(IsActive,0)=1) BDAC ON ACR.DeviceSystemID = BDAC.Id
								left outer join EmployeeFPInformation fp on fp.EmpSystemId=E.SystemId
                            WHERE ACR.RegisterStatus = 'Requested' AND (ACR.plantid = '" + plantid + @"')  and isnull(BDAC.MachineID,'')<>'' --and isnull(fp.FPTemplate,'')<>''
                                    and (Isnull(E.RegisterProximate, 0)=1 OR Isnull(E.RegisterFP, 0)=1)
                                    --and IsLeft is not null
                            ORDER BY E.EmployeeCode, BDAC.MachineID";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetGroupPrefix(string empid)
        {
            try
            {
                string _sql = @"SELECT PKPrefixField from org.CompanyGroup where Id=(select GroupId from EmployeeInformation where systemid='" + empid + "')";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetAccessControllerList(string plantid)
        {
            try
            {
                string _sql = @"SELECT  Id, MachineIP,MachineID, [Description],  Remarks, IsActive,
       IsDataAutoDownloadBySched, IsAdmin, AdminEnrollID, AdminPassword,
       AdminProxiCard, OneFlag, ZeroFlag, RegisTypeDec, RegisTypeHex,
       RegisCharacter, DownLdEnrollID, DownLdTypeDec, DownLdTypeScan,
       DownLdTypeHex, DownLdCharacter, IsDataClearAftDW, IsDeviceBasedInOut,
       DeviceInOutFlag, AttendanceDeviceZoneid, AddedBy, AddedDate, AddedFromIP,
       UpdatedBy, UpdatedDate, UpdatedFromIP,CompanyGroupId, PlantId FROM mst.AccessControllerList where plantid='" + plantid + "' and IsActive=1";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetShortLeaveSettings(string plantid)
        {
            try
            {
                //string _sql = @"SELECT * FROM PlantWiseShortLeaveSetting WHERE PlantID = '" + plantid + "'";
                string _sql = @"SELECT * FROM PlantWiseHRMSSetting WHERE PlantID = '" + plantid + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<EmployeeProfileVM> GetIndviEmployeeInformation(string plantid, string cardNumber)
        {
            IEnumerable<EmployeeProfileVM> _returnlist = null;
            try
            {
                string _sql = @"SELECT P.SystemID, P.EmployeeCode, P.EmployeeName,P.LVPolicyMasterSystemID
									,Replace(CONVERT(VARCHAR(11), P.DOB, 106), ' ', '-') DOB,
                                     Replace(CONVERT(VARCHAR(11), P.DOJ, 106), ' ', '-') DOJ,
									--EFP.IsLeft, EFP.FPImage,EFP.FPTemplate,
									P.NationalID, EmployeeStatus,p.EmpPicPath,
                                    P.CardNumber,
                                    U.UserName Unit, Dv.UserName Division, Dpt.UserName Department, S.UserName Section, SS.UserName SubSection,
                                    Desg.UserName GivenDesignation
                            FROM EmployeeInformation P
							    --LEFT JOIN EmployeeFPInformation EFP ON P.SystemID = EFP.EmpSystemID
                                LEFT JOIN ORG.Unit U ON P.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON P.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dpt ON P.DepartmentID = Dpt.Id
                                LEFT JOIN ORG.Section S ON P.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SS ON P.SubSectionID = SS.Id
                                LEFT JOIN HKP.Designation Desg ON P.GivenDesignationId = Desg.Id
                            WHERE P.EmployeeStatus='Active' AND P.IsApproved=1 AND P.PlantID = '" + plantid + @"' ";

                if (cardNumber.Trim() != "")
                {
                    _sql += @"AND (P.CardNumber = '" + cardNumber + @"')";
                }
                _returnlist = _sqlRepository.GetModelCollection<EmployeeProfileVM>(_sql, null);
                if (_returnlist.Count() > 0)
                {
                    //var v = Test2(PK); ;
                    foreach (var item in _returnlist)
                    {
                        item.ImageUrl = GetPicPath(item.EmpPicPath);
                    }
                }
                return _returnlist;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<EmployeeProfileVM> GetEmployeeInformation(string emp_pk)
        {
            IEnumerable<EmployeeProfileVM> _returnlist = null;
            try
            {
                string _sql = @"SELECT P.SystemID, P.EmployeeCode, P.EmployeeName,P.LVPolicyMasterSystemID
									,Replace(CONVERT(VARCHAR(11), P.DOB, 106), ' ', '-') DOB,
                                     Replace(CONVERT(VARCHAR(11), P.DOJ, 106), ' ', '-') DOJ,
									--EFP.IsLeft, EFP.FPImage,EFP.FPTemplate,
									P.NationalID, EmployeeStatus,
                                    P.CardNumber,
                                    U.UserName Unit, Dv.UserName Division, Dpt.UserName Department, S.UserName Section, SS.UserName SubSection,
                                    Desg.UserName GivenDesignation
                                    ,b.Code BudgetCode
									,p.EmpPicPath
                                    ,pp.UserName Plant
									,p.PlantId
									,c.UserName Company
									,p.CompanyId
                            FROM EmployeeInformation P
							    --LEFT JOIN EmployeeFPInformation EFP ON P.SystemID = EFP.EmpSystemID
                                LEFT JOIN ORG.Unit U ON P.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON P.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dpt ON P.DepartmentID = Dpt.Id
                                LEFT JOIN ORG.Section S ON P.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SS ON P.SubSectionID = SS.Id
                                LEFT JOIN HKP.Designation Desg ON P.GivenDesignationId = Desg.Id
                                left join mst.ManpowerBudget b on b.id=p.BudgetCode
                                left join org.Plant pp on pp.id=p.PlantId
								left join org.Company c on c.id=p.CompanyId
                            WHERE P.EmployeeStatus='Active'
                            --AND P.IsApproved=1
                            AND P.systemid = '" + emp_pk + @"' ";

                //if (cardNumber.Trim() != "")
                //{
                //    _sql += @"AND (P.CardNumber = '" + cardNumber + @"')";
                //}
                _returnlist = _sqlRepository.GetModelCollection<EmployeeProfileVM>(_sql, null);
                if (_returnlist.Count() > 0)
                {
                    //var v = Test2(PK); ;
                    foreach (var item in _returnlist)
                    {
                        item.ImageUrl = GetPicPath(item.EmpPicPath);
                    }
                }
                return _returnlist;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetPlantWiseShortLeaveKioskDetails(string plantid)
        {
            try
            {
                string _sql = @"SELECT * FROM PlantWiseShortLeaveKioskDetails WHERE PlantID = '" + plantid + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetFPEngineParameterForWithOutBlackListedEmpInfoViaUSBRd(string plantid)
        {
            try
            {
                string _sql = @"SELECT * FROM FPEngineParameter WHERE PlantID = '" + plantid + @"' AND (AuthenticationType = 'LeftFingerActive' OR AuthenticationType = 'RightFingerActive') ORDER BY AuthenticationType";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetSlvAvailedB4SlvApp(string plantId, string empSystemID, string slvDate)
        {
            try
            {
                string _sql = @"SELECT * FROM ShortLeaveAllocation WHERE (PlantID = '" + plantId + @"')
                            AND (EmpSystemID = '" + empSystemID + @"') AND (REPLACE((CONVERT(VARCHAR(11), SlvDate, 113)),' ','-') = '" + slvDate + @"')
                            AND IsAvailed = 0";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetCheckMultiTimeSlvINaDay(string plantId, string empSystemID, string slvDate, string strLang)
        {
            string _sql;
            try
            {
                if (strLang == "BANGLA")
                {
                    _sql = @"SELECT CONVERT(VARCHAR(5), ISNULL(OutTime, 0), 108) [যাবার সময়], CONVERT(VARCHAR(5), ISNULL(InTime, 0), 108) [ফেরার সময়],
                                ISNULL(TimeDuration, 0) [সময়সীমা] FROM ShortLeaveAllocation WHERE (PlantID = '" + plantId + @"')
                                AND (EmpSystemID = '" + empSystemID + @"') AND (REPLACE((CONVERT(VARCHAR(11), SlvDate, 113)),' ','-') = '" + slvDate + @"')
                                ORDER BY OutTime, InTime";
                }
                else
                {
                    _sql = @"SELECT CONVERT(VARCHAR(5), ISNULL(OutTime, 0), 108) OutTime, CONVERT(VARCHAR(5), ISNULL(InTime, 0), 108) InTime,
                                ISNULL(TimeDuration, 0) TimeDuration FROM ShortLeaveAllocation WHERE (PlantID = '" + plantId + @"')
                                AND (EmpSystemID = '" + empSystemID + @"') AND (REPLACE((CONVERT(VARCHAR(11), SlvDate, 113)),' ','-') = '" + slvDate + @"')
                                ORDER BY OutTime, InTime";
                }

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetCheckSameDateLeave(string plantId, string empSystemID, string fromDate, string toDate)
        {
            //AND GroupID = '" + companyGroupId + @"'
            string _sql;
            try
            {
                if (empSystemID != "")
                {
                    _sql = @"SELECT * FROM dbo.LeaveTransaction
                               WHERE PlantID = '" + plantId + @"'
                                    AND (SystemID <> '') AND (EmpSystemID = '" + empSystemID + @"')
                                        AND ((FromDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"')
                                            OR (ToDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'))";
                }
                else
                {
                    _sql = @"SELECT * FROM dbo.LeaveTransaction
                               WHERE PlantID = '" + plantId + @"'
                                        AND ((FromDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"')
                                            OR (ToDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'))";
                }

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<EmployeeProfileVM> GetIndviSupVisEmpInfo(string plantId, string cardNumber)
        {
            IEnumerable<EmployeeProfileVM> _returnlist = null;
            try
            {
                string _sql = @"SELECT P.SystemID, P.EmployeeCode, P.EmployeeName, Replace(CONVERT(VARCHAR(11), P.DOB, 106), ' ', '-') DOB,
                                Replace(CONVERT(VARCHAR(11), P.DOJ, 106), ' ', '-') DOJ, P.NationalID, EmployeeStatus,p.EmpPicPath,
                                 P.CardNumber, U.UserName Unit, Dv.UserName Division, Dpt.UserName Department, S.UserName Section, SS.UserName SubSection,
                                Desg.UserName GivenDesignation
                         FROM EmployeeInformation P
                                LEFT JOIN ORG.Unit U ON P.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON P.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dpt ON P.DepartmentID = Dpt.Id
                                LEFT JOIN ORG.Section S ON P.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SS ON P.SubSectionID = SS.Id
                                LEFT JOIN HKP.Designation Desg ON P.GivenDesignationId = Desg.Id
                          WHERE P.PlantID = '" + plantId + @"' AND P.CardNumber = '" + cardNumber + @"'  AND P.EmployeeStatus = 'Active' AND P.IsApproved=1";
                _returnlist = _sqlRepository.GetModelCollection<EmployeeProfileVM>(_sql, null);
                if (_returnlist.Count() > 0)
                {
                    foreach (var item in _returnlist)
                    {
                        item.ImageUrl = GetPicPath(item.EmpPicPath);
                    }
                }
                return _returnlist;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<AccessControllerEmployeeTag> GetAccessControllerEmployeeTagList(string plantid)//TBT
        {
            IEnumerable<AccessControllerEmployeeTag> _returnlist = null;
            try
            {
                string _sql = @"SELECT * from AccessControllerEmployeeTag WHERE RegisterStatus = 'Requested' and plantid='" + plantid + "'";
                _returnlist = _sqlRepository.GetModelCollection<AccessControllerEmployeeTag>(_sql, null);
                return _returnlist;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<AccessControllerEmployeeTag> GetAccessControllerEmployeeTagDeleteList(string plantid)//TBT
        {
            IEnumerable<AccessControllerEmployeeTag> _returnlist = null;
            try
            {
                string _sql = @"SELECT * from AccessControllerEmployeeTagDelete WHERE RegisterStatus = 'Requested' and plantid='" + plantid + "'";
                _returnlist = _sqlRepository.GetModelCollection<AccessControllerEmployeeTag>(_sql, null);
                return _returnlist;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetShortLeaveAllocation(string plantid)
        {
            try
            {
                string _sql = @"SELECT * FROM ShortLeaveAllocation WHERE PlantID = '" + plantid + "' AND SystemID=''";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetEmployeePin(string employeeid, string pin)
        {
            try
            {
                string _sql = @"SELECT * FROM HKP.EmployeeMobileAppsAuthorization Where EmployeeId='" + employeeid + "' AND PIN='" + pin + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetSLAPK()
        {
            return _signatrueService.GetAutoNumber(nameof(ShortLeaveAllocation), DateTime.Now).ToString();
        }
    }
}
#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class SalaryFixationSettingService : Service<SalaryFixationSetting>, ISalaryFixationSettingService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ISalaryFixationService _sf;
        private readonly ISalaryFixationSettingDetailsService _salaryFixationSettingDetailsService;
        private readonly IRepositoryAsync<SalaryFixationSettingDetails> _salaryFixationSettingDetailsRepository;

        public SalaryFixationSettingService(
            IRepositoryAsync<SalaryFixationSetting> salaryFixationSettingRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , ISalaryFixationSettingDetailsService salaryFixationSettingDetailsService
            , ISalaryFixationService sf
            , IRepositoryAsync<SalaryFixationSettingDetails> salaryFixationSettingDetailsRepository)
            : base(salaryFixationSettingRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _sf = sf;
            _salaryFixationSettingDetailsService = salaryFixationSettingDetailsService;
            _salaryFixationSettingDetailsRepository = salaryFixationSettingDetailsRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(SalaryFixationSetting), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM SCS.SalaryFixationSetting";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void Check(SalaryFixationSetting entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id);
        }

        public void Insert(SalaryFixationSetting entity, string companyGroupId)
        {
            try
            {
                Check(entity);
                entity.Id = "SFM" + GetPK();
                entity.CompanyGroupID = companyGroupId;
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(SalaryFixationSetting entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetSalaryHeads(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM dbo.SalaryHead WHERE HeadType='E'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetSalaryHeadsAnCash(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM dbo.SalaryHead where HeadType='E'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetSavedChildMasterWise(string salFixSetId)
        {
            try
            {
                var sql = @"SELECT SFD.*
                           	,SH.SalaryHeadID
                           	,SH.SalaryHead
                           	,SH.Description
                           	,SH.HeadType
                           	,SH.HeadCategory
                           FROM SCS.SalaryFixationSettingDetails SFD
                           LEFT JOIN SalaryHead SH ON SFD.SalaryHeadID = SH.SalaryHeadID
                           WHERE SFD.SalFixSetId = '" + salFixSetId + @"' AND SFD.IsMonthly = 1";
                return _sqlRepository.GetDataCollection(sql, null);
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
        }

        public IEnumerable<object> GetAnnualCashChild(string salFixSetId)
        {
            try
            {
                var sql = @"SELECT SFD.*
                           	,SH.SalaryHeadID
                           	,SH.SalaryHead
                           	,SH.Description
                           	,SH.HeadType
                           	,SH.HeadCategory
                           FROM SCS.SalaryFixationSettingDetails SFD
                           LEFT JOIN SalaryHead SH ON SFD.SalaryHeadID = SH.SalaryHeadID
                           WHERE SFD.SalFixSetId = '" + salFixSetId + @"' AND SFD.IsAnnualCash = 1";
                return _sqlRepository.GetDataCollection(sql, null);
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
        }

        public IEnumerable<object> GetNonCashChild(string salFixSetId)
        {
            try
            {
                var sql = @"SELECT  SFD.*
                            ,ANC.Code
                           	,ANC.ShortName
                           	,ANC.UserName
                           	,ANC.Description
                           	,ANC.Active
                           FROM SCS.AnnualNonCash ANC
                           LEFT JOIN SCS.SalaryFixationSettingDetails SFD ON ANC.Id = SFD.AnnualNonCashId
                           WHERE SFD.SalFixSetId = '" + salFixSetId + @"'
                           	AND SFD.IsAnnualNonCash = 1";
                return _sqlRepository.GetDataCollection(sql, null);
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
        }

        public void InsertOrUpdate(IEnumerable<SalaryFixationSettingDetails> monthlyList
            , IEnumerable<SalaryFixationSettingDetails> annualCashList
            , IEnumerable<SalaryFixationSettingDetails> annualCashNonList, IEnumerable<SalaryFixationSettingDetails> leaveTypeList
            , string salFixSetId)
        {
            var flag = false;

            try
            {
                var month_db = GetmonthlyDetailList(salFixSetId).ToList<SalaryFixationSettingDetails>();
                var annual_db = GetAnnualCashDetailList(salFixSetId).ToList<SalaryFixationSettingDetails>();
                var non_db = GetNonCashDetailList(salFixSetId).ToList<SalaryFixationSettingDetails>();
                var leave_db = GetleaveDataList(salFixSetId).ToList<SalaryFixationSettingDetails>();

                if (monthlyList != null)
                {
                    Save(monthlyList, month_db, salFixSetId);
                }

                if (annualCashList != null)
                {
                    Save(annualCashList, annual_db, salFixSetId);
                }

                if (annualCashNonList != null)
                {
                    Save(annualCashNonList, non_db, salFixSetId);
                }

                if (leaveTypeList != null)
                {
                    Save(leaveTypeList, leave_db, salFixSetId);
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void Save(IEnumerable<SalaryFixationSettingDetails> list, IEnumerable<SalaryFixationSettingDetails> from_db, string salFixSetId)
        {
            try
            {
                SalaryFixationSettingDetails db = null;
                var count = 0;
                var pk = _salaryFixationSettingDetailsService.GetAutoNumber(nameof(SalaryFixationSettingDetails), PKGeneratorEnum.Auto, null, DateTime.Now);
                foreach (var item in from_db)
                {
                    var del = list.Where(a => a.Id == item.Id).FirstOrDefault();
                    if (del == null || string.IsNullOrEmpty(del.Id))
                    {
                        item.ModelState = ModelState.Deleted;
                        _salaryFixationSettingDetailsService.Delete(item);
                    }
                }
                foreach (var item in list)
                {
                    count++;
                    db = from_db.FirstOrDefault(r => r.Id == item.Id);
                    if (db == null || db.Id == null)
                    {
                        db = new SalaryFixationSettingDetails();

                        db = item;
                        db.Id = "SFD" + pk + "-" + count;
                        db.ModelState = ModelState.Added;
                        AuditService.Log(db);
                        _salaryFixationSettingDetailsService.InsertGraph(db);
                    }
                    else
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                        _salaryFixationSettingDetailsService.UpdateGraph(db);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<SalaryFixationSettingDetails> GetmonthlyDetailList(string salFixSetId)
        {
            try
            {
                string _sql = "SELECT * FROM SCS.SalaryFixationSettingDetails  WHERE SalFixSetId='" + salFixSetId + "' and IsMonthly=1";
                return _salaryFixationSettingDetailsRepository.SqlQuery<SalaryFixationSettingDetails>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<SalaryFixationSettingDetails> GetAnnualCashDetailList(string salFixSetId)
        {
            try
            {
                string _sql = "SELECT * FROM SCS.SalaryFixationSettingDetails  WHERE SalFixSetId='" + salFixSetId + "' AND IsAnnualCash=1";
                return _salaryFixationSettingDetailsRepository.SqlQuery<SalaryFixationSettingDetails>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<SalaryFixationSettingDetails> GetNonCashDetailList(string salFixSetId)
        {
            try
            {
                string _sql = "SELECT * FROM SCS.SalaryFixationSettingDetails  WHERE SalFixSetId='" + salFixSetId + "' AND IsAnnualNonCash=1";
                return _salaryFixationSettingDetailsRepository.SqlQuery<SalaryFixationSettingDetails>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<SalaryFixationSettingDetails> GetleaveDataList(string salFixSetId)
        {
            try
            {
                string _sql = "SELECT * FROM SCS.SalaryFixationSettingDetails  WHERE SalFixSetId='" + salFixSetId + "' AND IsLeave=1";
                return _salaryFixationSettingDetailsRepository.SqlQuery<SalaryFixationSettingDetails>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetSavedLeaveChild(string salFixSetId)
        {
            try
            {
                var sql = @"SELECT S.*
                            	,L.LeaveType
                            	,L.Code
                            	,L.UserName
                            	,L.Description
                            FROM LeaveType L
                            LEFT JOIN SCS.SalaryFixationSettingDetails S ON L.Id = S.LeaveTypeId
                            WHERE S.SalFixSetId = '" + salFixSetId + @"'
                            AND S.IsLeave = 1";
                return _sqlRepository.GetDataCollection(sql, null);
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
        }

        public GridModel GetLeaveTypes(GridParameter parameters)
        {
            try
            {
                //parameters.CmdText = @"SELECT CASE ISNULL(SFD.Id, '')
                //                       		WHEN ''
                //                       			THEN CAST('False' AS BIT)
                //                       		ELSE CAST('TRUE' AS BIT)
                //                       		END Flag
                //                       	,SFD.Id
                //                       	,L.Id AS LeaveTypeId
                //                       	,L.CompanyGroupId
                //                       	,L.LeaveType
                //                       	,L.Code
                //                       	,L.UserName
                //                       	,L.Description
                //                       FROM [dbo].[LeaveType] L
                //                       LEFT JOIN SCS.SalaryFixationSettingDetails SFD ON L.Id = SFD.LeaveTypeId";
                parameters.CmdText = @"SELECT
                                       	L.Id AS LeaveTypeId
                                       	,L.CompanyGroupId
                                       	,L.LeaveType
                                       	,L.Code
                                       	,L.UserName
                                       	,L.Description
                                       FROM [dbo].[LeaveType] L";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetAnnualNonCash(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT Id AnnualNonCashId, Code, ShortName, UserName, Description, Active FROM [SCS].[AnnualNonCash]";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //void ValidationDelete(string masterpk)
        //{
        //    try
        //    {
        //        var _SalaryFixation = _sf.Query(t => t.FixationSetID == masterpk).Select().ToList();
        //        if(_SalaryFixation !=null && _SalaryFixation.Count>0)
        //        {
        //            throw new CustomException("This 'Salary Fixation Setting' has already been tagged with Salary Fixation...");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public void DeleteMaster(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new CustomException("Salary Fixation Setting is not found...");

            //ValidationDelete(id);
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(id);
                if (data != null)
                {
                    _salaryFixationSettingDetailsService.ExecuteSqlCommand("DELETE FROM scs.SalaryFixationSettingDetails Where SalFixSetId='" + id + "'");
                    base.Delete(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}
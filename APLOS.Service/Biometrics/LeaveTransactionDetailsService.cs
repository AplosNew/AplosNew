#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Extension.HumanResource.Leave;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

#endregion Using

namespace Library.Service.Biometrics
{
    public class LeaveTransactionDetailsService : Service<LeaveTransactionDetails>, ILeaveTransactionDetailsService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IAccessControllerDeleteRequestService _d;

        public LeaveTransactionDetailsService(
            IRepositoryAsync<LeaveTransactionDetails> PreRecruitmentEmpReferenceRepository
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

        public void InsertGraph(PolicySandwichVM _policyVM,List<string> listH, List<string> listW, LeaveTransactionDetails details, DateTime fromDate, DateTime toDate,decimal duration, bool halfDay)
        {
            try
            {
                int i = 0;

                DateTime dtFmLTD = fromDate;
                DateTime dtToLTD = toDate;

                while (dtFmLTD <= dtToLTD)
                {


                    //IsValid
                    if (IsValid(_policyVM, listH, listW, dtFmLTD))//if no W/H as per policy
                    {
                        i += 1;
                        LeaveTransactionDetails d = new LeaveTransactionDetails
                        {
                            SystemID = details.LvTrnsSystemID + "-" + i,
                            LvTrnsSystemID = details.LvTrnsSystemID,
                            WorkDate = dtFmLTD,
                            DayType = "NW",
                            LeaveStatus = null,
                            IsAvailed = false,
                            LeaveDuration = duration,
                            IsFirstHalf = halfDay,
                            AddedBy = details.AddedBy,
                            DateAdded = details.DateAdded
                        };
                        InsertGraph(d); 
                    }//if no W/H as per policy

                    dtFmLTD = dtFmLTD.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
       

        bool IsValid(PolicySandwichVM _policyVM, List<string> listH, List<string> listW,DateTime dtFmLTD)
        {
            bool _result = false;
            try
            {
                if(_policyVM ==null && listH ==null && listW==null)//MLV
                {
                    return true;
                }

                if ((listW != null && listW.Contains(dtFmLTD.ToString("dd-MMM-yyyy"))))
                {
                    if (_policyVM.IsAsperEntryOnW)
                    {
                        _result = true;
                    }
                    else if (_policyVM.IsNoLeaveOnW)
                    {
                       return   false;
                    }
                    else if (_policyVM.InBetweenWeekoff)
                    {
                        return  false;
                    }
                }else
                {
                    _result = true;
                }

                if ((listH != null && listH.Contains(dtFmLTD.ToString("dd-MMM-yyyy"))))
                {
                    if (_policyVM.IsAsperEntryOnH)
                    {
                         _result = true;
                    }
                    else if (_policyVM.IsNoLeaveOnH)
                    {
                        return   false;
                    }
                    else if (_policyVM.InBetweenHoliday)
                    {
                        return  false;
                    }
                }
                else
                {
                    _result = true;
                }

                return _result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
using Library.Core;
using Library.Crosscutting;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Logs;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Addresses;
using Library.Service.Attendances;
using Library.Service.HumanResources;
using Library.Service.OrderManagements;
using Library.Service.Organizations;
using Library.Service.Properties;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Setups
{
    public class MailSenderServiceCore : Library.Service.Setups.MailSenderService
    {
        #region Constructor
        private readonly IRepositoryAsync<MailReceiver> _mailReceiverRepository;
        private readonly IRepositoryAsync<MailReceiverDetail> _mailReceiverDetailRepository;
        private readonly IRepositoryAsync<MailReceiverServiceMapping> _mailReceiverServiceMappingRepository;
        private readonly ISMTPConfigurationService _smtpConfigurationService;
        private readonly IAttendanceManagementService _attendanceManagementService;
        private readonly IManpowerAttendanceSummary _manpowerAttendanceSummary;
        private readonly IProductionOrderService _productionOrderService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IEntityService _entityService;
        private readonly IRepositoryAsync<CompanyGroup> _companyGroupRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;
        private readonly IRepositoryAsync<MailLog> _mailLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMonthlyAttendanceInformation _monthlyAttendanceInformation;


        public MailSenderServiceCore(
              IRepositoryAsync<MailReceiverDetail> mailReceiverDetailRepository
            , ISMTPConfigurationService smtpConfigurationService
            , IRepositoryAsync<MailReceiverServiceMapping> mailReceiverServiceMappingRepository
            , IRepositoryAsync<MailReceiver> mailReceiverRepository
            , ISqlRepository sqlRepository
            , IEntityService entityService
            , IRepositoryAsync<CompanyGroup> companyGroupRepository
            , IRepositoryAsync<Company> companyRepository
            , IRepositoryAsync<MailLog> mailLogRepository
            , IUnitOfWork unitOfWork
            , IAttendanceManagementService attendanceManagementService
            , IProductionOrderService ProductionOrderService
            , IManpowerAttendanceSummary manpowerAttendanceSummary
            , IMonthlyAttendanceInformation monthlyAttendanceInformation
              ):base(mailReceiverDetailRepository, smtpConfigurationService, mailReceiverServiceMappingRepository, mailReceiverRepository,
                 sqlRepository, entityService, companyGroupRepository, companyRepository, mailLogRepository, unitOfWork,
                 attendanceManagementService, ProductionOrderService, manpowerAttendanceSummary, monthlyAttendanceInformation)
        {
            _mailReceiverDetailRepository = mailReceiverDetailRepository;
            _smtpConfigurationService = smtpConfigurationService;
            _mailReceiverServiceMappingRepository = mailReceiverServiceMappingRepository;
            _mailReceiverRepository = mailReceiverRepository;
            _sqlRepository = sqlRepository;
            _entityService = entityService;
            _companyGroupRepository = companyGroupRepository;
            _companyRepository = companyRepository;
            _mailLogRepository = mailLogRepository;
            _unitOfWork = unitOfWork;
            _attendanceManagementService = attendanceManagementService;
            _productionOrderService = ProductionOrderService;
            _manpowerAttendanceSummary = manpowerAttendanceSummary;
            _monthlyAttendanceInformation = monthlyAttendanceInformation;
        }

        #endregion Constructor
    }
}

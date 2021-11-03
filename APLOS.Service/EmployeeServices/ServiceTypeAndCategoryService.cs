using System;
using System.Collections.Generic;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.EmployeeServices;
using Library.Service.Core;
using Library.Service.Organizations;
using Library.Service.Systems;

namespace Library.Service.EmployeeServices
{
    public class ServiceTypeAndCategoryService : Service<ServiceTypeAndCategory>, IServiceTypeAndCategoryService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;
        private readonly IPlantService _plantService;
        private readonly ISignatureService _signatrueService;


        public ServiceTypeAndCategoryService(
              IRepositoryAsync<ServiceTypeAndCategory> PreRecruitmentEmpReferenceRepositor
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            , IPlantService plantService
            , ISignatureService signatrueService

           ) :
            base(PreRecruitmentEmpReferenceRepositor, unitOfWork, pkGeneratorService)
        {
            _pk = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _plantService = plantService;
            _signatrueService = signatrueService;


        }

        #endregion Constructor

        public List<ServiceTypeAndCategory> GetList(string Service)
        {
            string strSql = "";
            try
            {
                strSql = @"select distinct st.*,sc.Category,uom.UserName as UOM
                                  from dbo.EmpServiceType st left join dbo.EmpServiceCategory sc on st.Id=sc.EmpServiceTypeId
                                  left join SCS.UnitOfMeasurement uom on st.UOMId=uom.Id
                                  where isnull(st.Service,'')='" + Service + "' ";
                return _sqlRepository.GetModelCollection<ServiceTypeAndCategory>(strSql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

           
        }

        public List<ServiceCategory> GetCategoryList(string Service)
        {
            string strSql = "";
            try
            {
                strSql = @"select sc.*,st.Service from dbo.EmpServiceCategory sc 
                              left join dbo.EmpServiceType st on sc.EmpServiceTypeId=st.Id
                              where isnull(st.Service,'')='" + Service + "' ";
                return _sqlRepository.GetModelCollection<ServiceCategory>(strSql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            
        }

        public IEnumerable<object> GetAllServices()
        {
            try
            {
                var _sql = @"select * from dbo.EmpServiceType";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmpName(string Empcode)
        {
            try
            {
                var _sql = @"select emp.EmployeeName as Text from dbo.EmployeeInformation emp where isnull(EmployeeCode,'')='" + Empcode + "' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}

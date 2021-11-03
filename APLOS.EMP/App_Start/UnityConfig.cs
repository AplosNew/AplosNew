using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Service.Core;
using Library.Service.External;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using Unity;
using Unity.AspNet.Mvc;

namespace Aplos.App_Start
{
    public class UnityConfig
    {
        #region Unity Container

        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }

        #endregion Unity Container

        public static void RegisterTypes(IUnityContainer container)
        {
            container
                .RegisterType<IEfDbContext, EfDbContext>(new PerRequestLifetimeManager())
                .RegisterType<IUnitOfWorkAsync, UnitOfWork>(new PerRequestLifetimeManager())
                .RegisterType<IUnitOfWork, UnitOfWork>(new PerRequestLifetimeManager())
                .RegisterType<ISqlRepository, SqlRepository>(new PerRequestLifetimeManager())
                .RegisterType(typeof(IRepositoryAsync<>), typeof(Repository<>))
                .RegisterType(typeof(IService<>), typeof(Service<>))

            #region Employee

            .RegisterType<IPKGeneratorService, PKGeneratorService>()
            .RegisterType<IEmployeeService, EmployeeService>()
            .RegisterType<IAplosEmpFieldTagService, AplosEmpFieldTagService>()
            .RegisterType<IAplosEmpFieldService, AplosEmpFieldService>()
            .RegisterType<IActivityService, ActivityService>()
            .RegisterType<IDocumentActivityService, DocumentActivityService>()
            .RegisterType<IKPIService, KPIService>()
            .RegisterType<IEmployeeLinkService, EmployeeLinkService>()
            .RegisterType<IUserAccessService, UserAccessService>()
            .RegisterType<IChartService, ChartService>()
            .RegisterType<IEmployeeProfileFromExcelService, EmployeeProfileFromExcelService>()

            #endregion Employee

            #region Logger

            .RegisterType<ILogger, Logger>()
            .RegisterType<IActionLogService, ActionLogService>()

            #endregion Logger

            ;
        }
    }
}
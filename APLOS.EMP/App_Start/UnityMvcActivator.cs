using Aplos.App_Start;
using System.Linq;
using System.Web.Mvc;
using Unity.AspNet.Mvc;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(UnityWebActivator), "Start")]
[assembly: ApplicationShutdownMethod(typeof(UnityWebActivator), "Shutdown")]
namespace Aplos.App_Start
{
    public static class UnityWebActivator
    {
        public static void Start() 
        {
            var container = UnityConfig.GetConfiguredContainer();
            FilterProviders.Providers.Remove(FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().First());
            FilterProviders.Providers.Add(new UnityFilterAttributeFilterProvider(container));
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        public static void Shutdown()
        {
            var container = UnityConfig.GetConfiguredContainer();
            container.Dispose();
        }
    }
}
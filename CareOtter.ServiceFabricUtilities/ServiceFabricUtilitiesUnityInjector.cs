using CareOtter.ServiceFabricUtilities.Interfaces;
using CareOtter.ServiceFabricUtilities.Paging;
using CareOtter.ServiceFabricUtilities.ServiceActivation;
using CareOtter.ServiceFabricUtilities.Services;
using Microsoft.Practices.Unity;

namespace CareOtter.ServiceFabricUtilities
{
    public class ServiceFabricUtilitiesUnityInjector 
    {
        public void Inject(UnityContainer config)
        {
            config.RegisterType<ICareOtterServiceProxyFactory,CareOtterServiceProxyFactory>
                (new ContainerControlledLifetimeManager());

            config.RegisterType<IServiceActivator, FabricServiceActivator>
                (new ContainerControlledLifetimeManager());

            config.RegisterType<IPagingHelper, PagingHelper>
                (new ContainerControlledLifetimeManager(),
                new InjectionConstructor(typeof(ICareOtterServiceProxyFactory)));
        }
    }
}
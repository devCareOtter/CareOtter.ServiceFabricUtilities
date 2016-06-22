using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CareOtter.ServiceFabricUtilities.Observable
{
    /// <summary>
    /// Defines a provider for push-based notification across Service Fabric Services
    /// </summary>
    public interface IReliableServiceObservable : IService
    {
        /// <summary>
        /// Subscribes to a provider for push-based notifications.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        Task Subscribe(ServiceObserver observer);

        /// <summary>
        /// Unsubscribes to a provider for push-based notifications.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        Task Unsubscribe(ServiceObserver observer);
    }
}

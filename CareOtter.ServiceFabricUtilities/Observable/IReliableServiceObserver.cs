using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CareOtter.ServiceFabricUtilities.Observable
{
    /// <summary>
    /// Provides a mechanism for receiving push-based notifications across Service Fabric Services
    /// </summary>    
    public interface IReliableServiceObserver : IService
    {
        /// <summary>
        /// Provides the observer with new data
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        Task Notify(MessageWrapper state);
    }
}

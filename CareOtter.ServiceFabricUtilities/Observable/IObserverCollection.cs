using System.Threading.Tasks;

namespace CareOtter.ServiceFabricUtilities.Observable
{
    /// <summary>
    /// Manages observers & notification
    /// </summary>
    
    public interface IObserverCollection
    {
        /// <summary>
        /// Subscribes an observer to the collection
        /// </summary>
        /// <param name="observer">the observer to subscribe</param>
        /// <returns>task</returns>
        Task SubscribeAsync(ServiceObserver observer);
        /// <summary>
        /// Removes an observer from the collection
        /// </summary>
        /// <param name="observer">The observer to remove</param>
        /// <returns>task</returns>
        Task UnsubscribeAsync(ServiceObserver observer);
        /// <summary>
        /// Notifies the observers than a value has changed
        /// </summary>
        /// <param name="value">the value to update</param>
        /// <returns>task</returns>
        Task NotifyAsync(MessageWrapper value);
    }
}
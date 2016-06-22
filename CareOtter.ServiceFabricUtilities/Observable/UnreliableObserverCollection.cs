using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareOtter.ServiceFabricUtilities.Observable
{
    /// <summary>
    /// Simple wrapper for storing & notifying observers
    /// Stored in unreliable, but thread safe, collections
    /// </summary>
    public class UnreliableObserverCollection : IObserverCollection
    {
         private readonly ConcurrentDictionary<Uri,ServiceObserver> _observers = new ConcurrentDictionary<Uri, ServiceObserver>();

        
        public Task SubscribeAsync(ServiceObserver observer)
        {
            _observers.AddOrUpdate(observer.ServiceUri, observer, (k, v) => observer);
            return Task.FromResult(true);
        }

        public Task UnsubscribeAsync(ServiceObserver observer)
        {

            ServiceObserver outObserver;
            _observers.TryRemove(observer.ServiceUri, out outObserver);
            return Task.FromResult(true);
        }

        public async Task NotifyAsync(MessageWrapper value)
        {

            var taskList = _observers.Select(obs => obs.Value.ResolveAndNotify(value));
            await Task.WhenAll(taskList);
        }
    }
}
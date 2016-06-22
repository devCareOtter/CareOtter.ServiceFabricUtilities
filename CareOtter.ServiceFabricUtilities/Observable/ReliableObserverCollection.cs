using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Nito.AsyncEx;
using System.Threading;
using System.Collections.Generic;

namespace CareOtter.ServiceFabricUtilities.Observable
{
    /// <summary>
    /// Manages storing and notifying observers in a reliable collection
    /// NOTE: The collection key is based on the value type, so you can only have one of these per type
    /// </summary>
    public class ReliableObserverCollection : IObserverCollection
    {
        private readonly IReliableStateManager _stateManager;
        private readonly string _name;
        public ReliableObserverCollection(IReliableStateManager stateManager, string name)
        {
            _stateManager = stateManager;
            _name = name;
            _observersDictName = $"ObserversDict_{_name}";
        }

        
        public async Task SubscribeAsync(ServiceObserver observer)
        {
            var dict = await GetObserversDictionaryAsync();
            using (var tx = _stateManager.CreateTransaction())
            {
                await dict.AddOrUpdateAsync(tx, observer.ServiceUri.ToString(), k => observer, (k, v) => observer);
                await tx.CommitAsync();
            }
        }

        public async Task UnsubscribeAsync(ServiceObserver observer)
        {
            var dict = await GetObserversDictionaryAsync();
            using (var tx = _stateManager.CreateTransaction())
            {
                await dict.TryRemoveAsync(tx, observer.ServiceUri.ToString());
                await tx.CommitAsync();
            }
        }

        public async Task NotifyAsync(MessageWrapper value)
        {
            var dict = await GetObserversDictionaryAsync();
            using (var tx = this._stateManager.CreateTransaction())
            {
                var query = await dict.CreateEnumerableAsync(tx);
                using (var e = query.GetAsyncEnumerator())
                {
                    var tasks = new List<Task>();
                    while (await e.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
                    {
                        tasks.Add(e.Current.Value.ResolveAndNotify(value));
                    }
                    await Task.WhenAll(tasks);
                }
            }
        }

        private readonly AsyncLock _observersDictLock = new AsyncLock();
        private IReliableDictionary<string, ServiceObserver> _observersDict;
        private readonly string _observersDictName;
        private async Task< IReliableDictionary<string, ServiceObserver>> GetObserversDictionaryAsync()
        {
            using (await _observersDictLock.LockAsync())
            {
                if (_observersDict == null)
                {
                    using (var tx = _stateManager.CreateTransaction())
                    {
                        _observersDict =
                            await _stateManager.GetOrAddAsync<IReliableDictionary<string, ServiceObserver>>(tx,
                                _observersDictName);
                        await tx.CommitAsync();
                    }
                }
            }

            return _observersDict;
        } 
    }
}
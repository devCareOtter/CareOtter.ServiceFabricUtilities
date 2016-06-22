using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.Mocks.StateManager
{
    public class MockActorStateManager : IActorStateManager
    {
        public Dictionary<string, object> _stateDict = new Dictionary<string, object>();

        /// <summary>
        /// The chance that all TryXAsync methods will fail
        /// </summary>
        public int TransientFailureChance { get; set; }
        public MockActorStateManager(int transientFailureChance = 0)
        {
            TransientFailureChance = transientFailureChance;
        }

        public Task AddStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = new CancellationToken())
        {
            _stateDict.Add(stateName,value);
            return Task.FromResult(true);
        }

        public Task<T> GetStateAsync<T>(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult((T) _stateDict[stateName]);
        }

        public Task SetStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = new CancellationToken())
        {
            _stateDict[stateName] = value;
            return Task.FromResult(true);
        }

        public Task RemoveStateAsync(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            _stateDict.Remove(stateName);
            return Task.FromResult(true);
        }

        public Task<bool> TryAddStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = new CancellationToken())
        {
            _stateDict.Add(stateName,value);
            return Task.FromResult(true);
        }

        private Random _random = new Random();
        protected bool MaybeFail()
        {
            if (TransientFailureChance == 0)
                return false;

            return _random.Next(0, 100) > TransientFailureChance;
        }

        private Task<ConditionalValue<T>> FailConditional<T>()
        {
            return Task.FromResult(new ConditionalValue<T>(false,default(T)));
        }

        public Task<ConditionalValue<T>> TryGetStateAsync<T>(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            if (MaybeFail() || !_stateDict.ContainsKey(stateName))
                return FailConditional<T>();

            return Task.FromResult(new ConditionalValue<T>(true, (T) _stateDict[stateName]));
        }

        public Task<bool> TryRemoveStateAsync(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            if (MaybeFail() || !_stateDict.ContainsKey(stateName))
                return Task.FromResult(false);

            _stateDict.Remove(stateName);
            return Task.FromResult(true);
        }

        public Task<bool> ContainsStateAsync(string stateName, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(_stateDict.ContainsKey(stateName));
        }

        public Task<T> GetOrAddStateAsync<T>(string stateName, T value, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_stateDict.ContainsKey(stateName))
                return Task.FromResult((T) _stateDict[stateName]);

            _stateDict.Add(stateName,value);
            return Task.FromResult(value);
        }

        public Task<T> AddOrUpdateStateAsync<T>(string stateName, T addValue, Func<string, T, T> updateValueFactory,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (!_stateDict.ContainsKey(stateName))
            {
                _stateDict.Add(stateName,addValue);
                return Task.FromResult(addValue);
            }

            var oldVal = (T) _stateDict[stateName];
            var newVal = updateValueFactory(stateName, oldVal);
            _stateDict[stateName] = newVal;
            return Task.FromResult(newVal);
        }

        public Task<IEnumerable<string>>  GetStateNamesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult<IEnumerable<string>>(_stateDict.Keys);
        }

        public Task ClearCacheAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
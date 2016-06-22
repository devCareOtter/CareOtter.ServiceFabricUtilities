using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.UnitTest.Mocks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;

namespace CareOtter.ServiceFabricUtilities.Mocks.StateManager
{
    /// <summary>
    /// Mock reliable dictionary. Essentailly encapsulates a normal dictionary
    /// </summary>
    /// <typeparam name="Tkey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// TODO: Should I write logic to roll back changes if the tx isn't committed?
    public class MockReliableDictionary<Tkey,TValue> : 
        IReliableDictionary<Tkey,TValue> where Tkey : IComparable<Tkey>, IEquatable<Tkey>
    {

        private ConcurrentDictionary<Tkey,TValue> _underlyingDict = new ConcurrentDictionary<Tkey, TValue>();
        public Uri Name { get; }
        public Task<long> GetCountAsync(ITransaction tx)
        {
            return Task.FromResult((long)_underlyingDict.Count);
        }

        public Task ClearAsync()
        {
            _underlyingDict.Clear();
            return Task.FromResult(true);
        }

        public Task AddAsync(ITransaction tx, Tkey key, TValue value)
        {
            _underlyingDict.TryAdd(key, value);
            return Task.FromResult(true);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, Tkey key, Func<Tkey, TValue> addValueFactory, Func<Tkey, TValue, TValue> updateValueFactory)
        {
            if (_underlyingDict.ContainsKey(key))
            {
                _underlyingDict[key] = updateValueFactory(key, _underlyingDict[key]);
                return Task.FromResult(_underlyingDict[key]);
            }
            _underlyingDict.TryAdd(key, addValueFactory(key));
            return Task.FromResult(_underlyingDict[key]);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, Tkey key, TValue addValue, Func<Tkey, TValue, TValue> updateValueFactory)
        {
            return AddOrUpdateAsync(tx, key, k => addValue, updateValueFactory);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, Tkey key)
        {
            return Task.FromResult(_underlyingDict.ContainsKey(key));
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, Tkey key, LockMode lockMode)
        {
            return ContainsKeyAsync(tx, key);
        }
        
        public async Task<TValue> GetOrAddAsync(ITransaction tx, Tkey key, Func<Tkey, TValue> valueFactory)
        {
            if (!await ContainsKeyAsync(tx, key))
            {
                _underlyingDict.TryAdd(key, valueFactory(key));
            }

            return _underlyingDict[key];
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, Tkey key, TValue value)
        {
            return GetOrAddAsync(tx,key,k=>value);
        }

        public async Task<bool> TryAddAsync(ITransaction tx, Tkey key, TValue value)
        {
            if (await ContainsKeyAsync(tx, key))
                return false;

            return _underlyingDict.TryAdd(key, value);
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, Tkey key)
        {
            if (!await ContainsKeyAsync(tx, key))
            {
                return new ConditionalValue<TValue>(false,default(TValue));
            }

            return new ConditionalValue<TValue>(true,_underlyingDict[key]);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, Tkey key, LockMode lockMode)
        {
            return TryGetValueAsync(tx, key);
        }

        public async Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, Tkey key)
        {
            if (!await ContainsKeyAsync(tx, key))
            {
                return new ConditionalValue<TValue>(false,default(TValue));
            }
            TValue outVal;
            var success = _underlyingDict.TryRemove(key, out outVal);
            return new ConditionalValue<TValue>(success,outVal);
        }

        public async Task<bool> TryUpdateAsync(ITransaction tx, Tkey key, TValue newValue, TValue comparisonValue)
        {
            if (!await ContainsKeyAsync(tx, key))
                return false;

            if (_underlyingDict[key].Equals(comparisonValue))
            {
                _underlyingDict[key] = newValue;
                return true;
            }
            return false;
        }

        public Task SetAsync(ITransaction tx, Tkey key, TValue value)
        {
            _underlyingDict[key] = value;
            return Task.FromResult(true);
        }

        public Func<IReliableDictionary<Tkey, TValue>, NotifyDictionaryRebuildEventArgs<Tkey, TValue>, Task> RebuildNotificationAsyncCallback
        { get; set; }

#pragma warning disable 67
        public event EventHandler<NotifyDictionaryChangedEventArgs<Tkey, TValue>> DictionaryChanged;
#pragma warning restore 67
        public Task SetAsync(ITransaction tx, Tkey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return SetAsync(tx,key,value);
        }

        public Task<bool> TryUpdateAsync(ITransaction tx, Tkey key, TValue newValue, TValue comparisonValue, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            return TryUpdateAsync(tx, key, newValue, comparisonValue);
        }

        public Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, Tkey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryRemoveAsync(tx,key);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, Tkey key, LockMode lockMode, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            return TryGetValueAsync(tx,key);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, Tkey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryGetValueAsync(tx,key);
        }

        public Task<bool> TryAddAsync(ITransaction tx, Tkey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryAddAsync(tx, key, value);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, Tkey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return GetOrAddAsync(tx,key,value);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, Tkey key, Func<Tkey, TValue> valueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return GetOrAddAsync(tx, key, valueFactory);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, Tkey key, LockMode lockMode, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            return ContainsKeyAsync(tx,key);
        }

        public Task<IAsyncEnumerable<KeyValuePair<Tkey,TValue>>> CreateEnumerableAsync(ITransaction txn)
        {
            IAsyncEnumerable<KeyValuePair<Tkey,TValue>> enumer = 
                new MockAsyncEnumerable<KeyValuePair<Tkey, TValue>>(_underlyingDict.ToList());
            return Task.FromResult(enumer);
        }

        public Task<IAsyncEnumerable<KeyValuePair<Tkey, TValue>>> CreateEnumerableAsync(ITransaction txn, Func<Tkey, bool> filter, EnumerationMode enumerationMode)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<KeyValuePair<Tkey, TValue>>> CreateEnumerableAsync(ITransaction txn, EnumerationMode enumerationMode)
        {
            return CreateEnumerableAsync(txn);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, Tkey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return ContainsKeyAsync(tx, key);
        }

        public Task ClearAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return ClearAsync();
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, Tkey key, TValue addValue, Func<Tkey, TValue, TValue> updateValueFactory, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            return AddOrUpdateAsync(tx,key,addValue,updateValueFactory);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, Tkey key, Func<Tkey, TValue> addValueFactory, Func<Tkey, TValue, TValue> updateValueFactory, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            return AddOrUpdateAsync(tx, key, addValueFactory, updateValueFactory);
        }

        public Task AddAsync(ITransaction tx, Tkey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return AddAsync(tx,key,value);
        }
    }
}
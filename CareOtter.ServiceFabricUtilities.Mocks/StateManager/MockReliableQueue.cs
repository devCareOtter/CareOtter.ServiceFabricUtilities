using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.UnitTest.Mocks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace CareOtter.ServiceFabricUtilities.Mocks.StateManager
{
    public class MockReliableQueue<T> : IReliableQueue<T>
    {
        protected Queue<T> _underlyingQueue = new Queue<T>();
        public Uri Name { get; }
        public Task<long> GetCountAsync(ITransaction tx)
        {
            return Task.FromResult((long)_underlyingQueue.Count);
        }

        public Task ClearAsync()
        {
            _underlyingQueue.Clear();
            return Task.FromResult(true);
        }

        public Task EnqueueAsync(ITransaction tx, T item)
        {
            _underlyingQueue.Enqueue(item);
            return Task.FromResult(true);
        }

        public Task EnqueueAsync(ITransaction tx, T item, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return EnqueueAsync(tx,item);
        }

        public Task<ConditionalValue<T>> TryDequeueAsync(ITransaction tx)
        {
            if (_underlyingQueue.Any())
            {
                var item = _underlyingQueue.Dequeue();
                return Task.FromResult(new ConditionalValue<T>(true,item));
            }
            return Task.FromResult(new ConditionalValue<T>(false, default(T)));
        }

        public Task<ConditionalValue<T>> TryDequeueAsync(ITransaction tx, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return TryDequeueAsync(tx);
        }

        public Task<ConditionalValue<T>> TryPeekAsync(ITransaction tx)
        {
            throw new NotImplementedException();
        }

        public Task<ConditionalValue<T>> TryPeekAsync(ITransaction tx, TimeSpan timeout, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ConditionalValue<T>> TryPeekAsync(ITransaction tx, LockMode lockMode)
        {
            throw new NotImplementedException();
        }

        public Task<ConditionalValue<T>> TryPeekAsync(ITransaction tx, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<T>> CreateEnumerableAsync(ITransaction tx)
        {
            return Task.FromResult<IAsyncEnumerable<T>>(new MockAsyncEnumerable<T>(_underlyingQueue.ToList()));
        }
    }
}
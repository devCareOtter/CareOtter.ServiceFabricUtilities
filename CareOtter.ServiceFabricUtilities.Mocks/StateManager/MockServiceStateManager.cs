﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;

namespace CareOtter.ServiceFabricUtilities.Mocks.StateManager
{
    /// <summary>
    /// Provides a StateManager simulating reliable dicationary storage for testing.
    /// </summary>
    public class MockServiceStateManager : IReliableStateManagerReplica
    {
        private Dictionary<string, IReliableState> _stateDict = new Dictionary<string, IReliableState>();
        /// <summary>
        /// if tranactions generated by this should throw when disposed of and not committed
        /// </summary>
        public bool ThrowOnNonCommit { get; set; }
        public MockServiceStateManager(bool throwOnNonCommit = false)
        {
            ThrowOnNonCommit = throwOnNonCommit;
        }

        public IAsyncEnumerator<IReliableState> GetAsyncEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public bool TryAddStateSerializer<T>(IStateSerializer<T> stateSerializer)
        {
            throw new System.NotImplementedException();
        }

        public ITransaction CreateTransaction()
        {
            return new MockTransaction(ThrowOnNonCommit);
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, string name) where T : IReliableState
        {
            return GetOrAddAsync<T>(name);
        }

        public Task<T> GetOrAddAsync<T>(string name) where T : IReliableState
        {
            if (_stateDict.ContainsKey(name))
                return Task.FromResult((T)_stateDict[name]);

            var collection = FigureOutTypeAndConstruct<T>();
            _stateDict.Add(name, collection);

            return Task.FromResult((T)collection);
        }

        protected T FigureOutTypeAndConstruct<T>()
            where T : IReliableState
        {
            var typeParams = typeof(T).GenericTypeArguments;

            if (typeof(T).GetGenericTypeDefinition() == typeof(IReliableDictionary<,>))
            {
                var typeToConstruct = typeof(MockReliableDictionary<,>).MakeGenericType(typeParams);
                return (T)Activator.CreateInstance(typeToConstruct);
            }
            else if (typeof(T).GetGenericTypeDefinition() == typeof(IReliableQueue<>))
            {

                var typeToConstruct = typeof(MockReliableQueue<>).MakeGenericType(typeParams);
                return (T)Activator.CreateInstance(typeToConstruct);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Task RemoveAsync(ITransaction tx, string name)
        {
            return RemoveAsync(name);
        }

        public Task RemoveAsync(string name)
        {
            if (_stateDict.ContainsKey(name))
                _stateDict.Remove(name);

            return Task.FromResult(true);
        }

        public Task<ConditionalValue<T>> TryGetAsync<T>(string name) where T : IReliableState
        {
            if (_stateDict.ContainsKey(name))
            {
                return Task.FromResult(new ConditionalValue<T>(true, (T)_stateDict[name]));
            }
            return Task.FromResult(new ConditionalValue<T>(false, default(T)));
        }

        //ignore unused var warning. 
#pragma warning disable 67
        public event EventHandler<NotifyTransactionChangedEventArgs> TransactionChanged;
        public event EventHandler<NotifyStateManagerChangedEventArgs> StateManagerChanged;
#pragma warning restore 67


        public Task<ConditionalValue<T>> TryGetAsync<T>(Uri name) where T : IReliableState
        {
            return TryGetAsync<T>(name.ToString());
        }

        public Task RemoveAsync(string name, TimeSpan timeout)
        {
            return RemoveAsync(name);
        }

        public Task RemoveAsync(ITransaction tx, string name, TimeSpan timeout)
        {
            return RemoveAsync(name);
        }

        public Task RemoveAsync(Uri name)
        {
            return RemoveAsync(name.ToString());
        }

        public Task RemoveAsync(Uri name, TimeSpan timeout)
        {
            return RemoveAsync(name);
        }

        public Task RemoveAsync(ITransaction tx, Uri name)
        {
            return RemoveAsync(name);
        }

        public Task RemoveAsync(ITransaction tx, Uri name, TimeSpan timeout)
        {
            return RemoveAsync(name);
        }

        public Task<T> GetOrAddAsync<T>(string name, TimeSpan timeout) where T : IReliableState
        {
            return GetOrAddAsync<T>(name, timeout);
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, string name, TimeSpan timeout) where T : IReliableState
        {
            return GetOrAddAsync<T>(name);
        }

        public Task<T> GetOrAddAsync<T>(Uri name) where T : IReliableState
        {
            return GetOrAddAsync<T>(name.ToString());
        }

        public Task<T> GetOrAddAsync<T>(Uri name, TimeSpan timeout) where T : IReliableState
        {
            return GetOrAddAsync<T>(name);
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, Uri name) where T : IReliableState
        {
            return GetOrAddAsync<T>(name);
        }

        public Task<T> GetOrAddAsync<T>(ITransaction tx, Uri name, TimeSpan timeout) where T : IReliableState
        {
            return GetOrAddAsync<T>(name);
        }

        public void Initialize(StatefulServiceInitializationParameters initializationParameters)
        {
            throw new NotImplementedException();
        }

        public Task<IReplicator> OpenAsync(ReplicaOpenMode openMode, IStatefulServicePartition partition, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public Task BackupAsync(Func<BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            throw new NotImplementedException();
        }

        public Task BackupAsync(BackupOption option, TimeSpan timeout, CancellationToken cancellationToken, Func<BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            throw new NotImplementedException();
        }

        public Task RestoreAsync(string backupFolderPath)
        {
            throw new NotImplementedException();
        }

        public Task RestoreAsync(string backupFolderPath, RestorePolicy restorePolicy, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Func<CancellationToken, Task<bool>> OnDataLossAsync { get; set; }
    }
}
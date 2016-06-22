using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions;
using CareOtter.ServiceFabricUtilities.Containers;
using CareOtter.ServiceFabricUtilities.Interfaces;
using CareOtter.ServiceFabricUtilities.Paging;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CareOtter.ServiceFabricUtilities.Services
{
    public abstract class CareOtterStatefulServiceBase : StatefulService, IDataPageReciever, IDataPageSender
    {
        protected readonly ICareOtterServiceProxyFactory ServiceProxyFactory;
        protected readonly Action<Exception> ExceptionHandler;

        public CareOtterStatefulServiceBase(StatefulServiceContext context,
            ICareOtterServiceProxyFactory serviceProxyFactory, Action<Exception> exceptionHandler)
            : base(context)
        {
            if(serviceProxyFactory == null)
                throw new ArgumentNullException(nameof(serviceProxyFactory));
            if(exceptionHandler == null)
                throw new ArgumentNullException(nameof(exceptionHandler));

            ServiceProxyFactory = serviceProxyFactory;
            _pagingHelper = new PagingHelper(ServiceProxyFactory);
            _pageManager = new ReliableDataPageManager(StateManager);
            ExceptionHandler = exceptionHandler;
        }

        public CareOtterStatefulServiceBase(StatefulServiceContext context, IReliableStateManagerReplica stateManager,
            ICareOtterServiceProxyFactory serviceProxyFactory, Action<Exception> exceptionHandler)
            : base(context, stateManager)
        {
            if(serviceProxyFactory == null)
                throw new ArgumentNullException(nameof(serviceProxyFactory));
            if(exceptionHandler == null)
                throw new ArgumentNullException(nameof(exceptionHandler));

            ServiceProxyFactory = serviceProxyFactory;
            _pagingHelper = new PagingHelper(ServiceProxyFactory);
            _pageManager = new ReliableDataPageManager(StateManager);
            ExceptionHandler = exceptionHandler;
        }


        protected sealed override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await SafeRunAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            { }
            catch (Exception e)
            {
                //any unhandled exceptions in runasync are critical errors and should be rethrown
                //this will bring the app domain down, throw warning, call the police,
                //signal the air raid sirens, drop the ball, and alert us that something went wrong
                //the purpose of this is to catch it and log it to whatever so we can see the reason without having to
                //log into the boxes
                ExceptionHandler?.Invoke(e);
                throw;
            }
        }

        public abstract Task SafeRunAsync(CancellationToken cancellationToken);        

        /// <summary>
        /// returns a snapshot enumerable for a reliable queue. May be invalidated
        /// </summary>
        /// <typeparam name="T">Val type for the queue</typeparam>
        /// <param name="collection">Collection that we wish to enumerate</param>
        /// <returns>Enumerable for the collection</returns>
        /// <remarks>Do not use this helper for large collections</remarks>
        protected virtual async Task<IEnumerable<T>> GetReliableQueueEnumerableAsync<T>(IReliableQueue<T> collection)
        {
            
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumer = await GetReliableQueueEnumerableAsync(collection,tx);
                return await enumer.ToListAsync(tx);
            }
        }

        /// <summary>
        /// Creates an enumerable with a transaction
        /// </summary>
        /// <typeparam name="T">val type for the queue</typeparam>
        /// <param name="collection">collection to get the enumerable for</param>
        /// <param name="tx">transaction to use in the call</param>
        /// <returns>enumerable for the collection</returns>
        /// <remarks>Do not use this helper for large collections</remarks>
        protected virtual async Task<IAsyncEnumerable<T>> GetReliableQueueEnumerableAsync<T>(IReliableQueue<T> collection,
            ITransaction tx)
        {
            return await collection.CreateEnumerableAsync(tx);
        }

        /// <summary>
        /// returns a snapshot enumerable for the dictionary. May be invalidated
        /// </summary>
        /// <typeparam name="TKey">Key type for the dictionary</typeparam>
        /// <typeparam name="TVal">Value type for the dictionary</typeparam>
        /// <param name="collection">dictionary to enumerate</param>
        /// <returns>enumerable for the dict</returns>
        /// <remarks>Do not use this helper for large collections</remarks>
        protected virtual async Task<IEnumerable<KeyValuePair<TKey, TVal>>> GetReliableDictionaryEnumerableAsync
            <TKey, TVal>(IReliableDictionary<TKey, TVal> collection) where TKey : IComparable<TKey>,IEquatable<TKey>
        {
            
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumer = await GetReliableDictionaryEnumerableAsync(collection,tx);
                return await enumer.ToListAsync(tx);
            }
        }


        /// <summary>
        /// returns a snapshot enumerable for the dictionary. May be invalidated
        /// </summary>
        /// <typeparam name="TKey">Key type for the dictionary</typeparam>
        /// <typeparam name="TVal">Value type for the dictionary</typeparam>
        /// <param name="collection">dictionary to enumerate</param>
        /// <param name="tx">transaction to use for getting the enumerator</param>
        /// <returns>enumerable for the dict</returns>
        /// <remarks>Do not use this helper for large collections</remarks>
        protected virtual async Task<IAsyncEnumerable<KeyValuePair<TKey, TVal>>> GetReliableDictionaryEnumerableAsync
            <TKey, TVal>(IReliableDictionary<TKey, TVal> collection, ITransaction tx) where TKey : IComparable<TKey>,IEquatable<TKey>
        {
            return await collection.CreateEnumerableAsync(tx);
        }

        /// <summary>
        /// Caches reliable collections
        /// </summary>
        protected volatile ConcurrentDictionary<string,IReliableState> ReliableStateDictionary = new ConcurrentDictionary<string, IReliableState>();

        /// <summary>
        /// Gets or adds a new type of reliable collection to the state provider
        /// </summary>
        /// <typeparam name="TReliableCollectionType">The type of collection to add</typeparam>
        /// <param name="stateName">The name to reference the reliable state by</param>
        /// <returns>the reliable collection</returns>
        protected virtual async Task<TReliableCollectionType> GetOrAddCachedStateAsync<TReliableCollectionType>(string stateName) where TReliableCollectionType : IReliableState
        {
            var state = GetCachedState(stateName);
            if (state != null)
            {
                return (TReliableCollectionType) state;
            }

            var newRelColl = await StateManager.GetOrAddAsync<TReliableCollectionType>(stateName);
            ReliableStateDictionary.TryAdd(stateName, newRelColl);
            return newRelColl;
        }

        /// <summary>
        /// Attempts to get cached state from the local state manager cache
        /// </summary>
        /// <typeparam name="TReliableCollectionType">The reliable collection type</typeparam>
        /// <param name="stateName">Name of the state to get</param>
        /// <returns>A conditional result for the state we fetched</returns>
        protected virtual async Task<ConditionalValue<TReliableCollectionType>> TryGetCachedStateAsync
            <TReliableCollectionType>(string stateName) where TReliableCollectionType : IReliableState
        {
            //if it's cached return that
            var state = GetCachedState(stateName);
            if (state != null)
            {
                return new ConditionalValue<TReliableCollectionType>(true, (TReliableCollectionType) state);
            }

            //if it's not cached get it and if it's valid, cache it
            var retrievedState = await StateManager.TryGetAsync<TReliableCollectionType>(stateName);
            if (retrievedState.HasValue)
            {
                ReliableStateDictionary.TryAdd(stateName, retrievedState.Value);
            }
            return retrievedState;
        }

        /// <summary>
        /// Removes a state from state manager without checking if we have it
        /// Is faster than try remove, but it isn't exception safe 
        /// </summary>
        /// <param name="tx">Transaction for the remove </param>
        /// <param name="stateName">name of the state to remove</param>
        /// <returns></returns>
        protected virtual async Task RemoveStateAsync(ITransaction tx, string stateName)
        {
            //remove state from the local cache
            if (ReliableStateDictionary.ContainsKey(stateName))
            {
                IReliableState outValue; //we dont do anything with this, but the remove method requires it
                ReliableStateDictionary.TryRemove(stateName, out outValue);
            }

            //remove state from the state manager
            await StateManager.RemoveAsync(tx, stateName);
        }

        /// <summary>
        /// Removes state from the state manager and check if we have it first
        /// </summary>
        /// <param name="tx">transaction for the remove</param>
        /// <param name="stateName">name of the state to remove</param>
        /// <returns></returns>
        protected virtual async Task TryRemoveStateAsync(ITransaction tx, string stateName)
        {
            //see if we have this state
            var state = await TryGetCachedStateAsync<IReliableState>(stateName);
            if (state.HasValue)
            {
                //remove it if we do
                await RemoveStateAsync(tx, stateName);
            }
        }

        private IReliableState GetCachedState(string stateName)
        {
            IReliableState state = null;
            if (ReliableStateDictionary.TryGetValue(stateName, out state))
                return state;

            return null;
        }

        /// <summary>
        /// Gets a quick snapshot count of a collection
        /// </summary>
        /// <typeparam name="T">Generic type of the collection</typeparam>
        /// <param name="collection">Collection to count</param>
        /// <returns>The collection count</returns>
        protected virtual async Task<long> GetSnapshotCountTransactionalAsync<T>(IReliableCollection<T> collection)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var res = await collection.GetCountAsync(tx);
                return res;
            }
        }


        private readonly PagingHelper _pagingHelper;
        private readonly IDataPageManager _pageManager;

        public async Task<T> GetPagedResults<T>(PagingId pagingSessionId)
        {
            var data = await _pageManager.GetDataAs<T>(pagingSessionId);
            await _pageManager.EndPagingData(pagingSessionId);
            return data;
        }

        /// <summary>
        /// sends paged data to a IDataPageReciever via that reciever's uri
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataToSend">the data to send</param>
        /// <param name="recipientUri">uri of the data recipient</param>
        /// <returns></returns>
        public Task<PagingId> SendDataPaged<T>(T dataToSend, Uri recipientUri)
        {
            return _pagingHelper.SendDataPaged(dataToSend, recipientUri);
        }

        /// <summary>
        /// Sends paged data to an established IDataPageReciever proxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataToSend">the data to send</param>
        /// <param name="recipient">the recipient of the paged data</param>
        /// <returns>an id of the paged data session</returns>
        public Task<PagingId> SendDataPaged<T>(T dataToSend, IDataPageReciever recipient)
        {
            return _pagingHelper.SendDataPaged(dataToSend, recipient);
        }

        /// <summary>
        /// Notifies this service that someone is going to start paging data to us
        /// </summary>
        /// <param name="originDataLength">length of the origin array</param>
        /// <returns>Id for this paging session</returns>
        public Task<PagingId> BeginPagingData(int originDataLength)
        {
            return _pageManager.BeginPagingData(originDataLength);
        }

        /// <summary>
        /// Sends a data page to this service
        /// </summary>
        /// <param name="id">id for the paging session</param>
        /// <param name="page">byte array for the data page</param>
        /// <returns></returns>
        public Task SendDataPage(PagingId id, byte[] page)
        {
            return _pageManager.SendDataPage(id, page);
        }

        /// <summary>
        /// Gets a requested data page 
        /// </summary>
        /// <param name="sessionId">paging session id</param>
        /// <param name="pageNum">page number to get</param>
        /// <returns>CV representing the byte array for that page</returns>
        public async Task<Response<byte[]>> GetPageAsync(PagingId sessionId, int pageNum)
        {
            var res = await _pageManager.GetPage(sessionId, pageNum);
            return new Response<byte[]>()
            {
                Success = res.HasValue,
                ResultValue = res.Value
            };
        }

        /// <summary>
        /// Ends a paging session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public Task NotifyPagingComplete(PagingId sessionId)
        {
            return _pageManager.NotifyPagingSessionComplete(sessionId);
        }

        public async Task<RequestedPagedDataSession> PrepareForPaging<T>(T obj)
        {
            var ses = await _pageManager.PrepareDataForPaging(obj);
            ses.OriginUri = this.Context.ServiceName;
            return ses;
        }

        /// <summary>
        /// Gets the results for a paging session sent by a service
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <param name="session"></param>
        /// <param name="senderProxy"></param>
        /// <returns></returns>
        public Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session,
            IDataPageSender senderProxy) where TResultType : class
        {
            return _pagingHelper.GetPagedResults<TResultType>(session, senderProxy, CancellationToken.None);
        }

        /// <summary>
        /// Gets the results for a paging session sent by a service
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <param name="session"></param>
        /// <returns></returns>
        public Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session)
            where TResultType : class
        {
            return GetPagedResults<TResultType>(session, ServiceProxyFactory.Create<IDataPageSender>(session.OriginUri));
        }
    }
}
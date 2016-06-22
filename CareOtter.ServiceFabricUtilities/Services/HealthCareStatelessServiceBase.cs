using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;
using CareOtter.ServiceFabricUtilities.Interfaces;
using CareOtter.ServiceFabricUtilities.Paging;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CareOtter.ServiceFabricUtilities.Services
{
    public abstract class CareOtterStatelessServiceBase : StatelessService, IDataPageReciever, IDataPageSender
    {
        protected Action<Exception> ExceptionHandler;
        protected ICareOtterServiceProxyFactory ServiceProxyFactory;
        public CareOtterStatelessServiceBase(StatelessServiceContext context,
            ICareOtterServiceProxyFactory serviceProxyFactory,
            Action<Exception> exceptionHandler)
            : base(context)
        {
            if(serviceProxyFactory == null)
                throw new ArgumentNullException(nameof(serviceProxyFactory));
            if(exceptionHandler == null)
                throw new ArgumentNullException(nameof(exceptionHandler));

            ServiceProxyFactory = serviceProxyFactory;
            _pagingHelper = new PagingHelper(serviceProxyFactory);
            ExceptionHandler = exceptionHandler;
        }

        protected sealed override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await SafeRunAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            { }
            catch (Exception e)
            {
                ExceptionHandler?.Invoke(e);
                throw;
            }
        }

        public abstract Task SafeRunAsync(CancellationToken cancellationToken);

        private readonly PagingHelper _pagingHelper;
        private readonly IDataPageManager _pageManager = new UnreliableDataPageManager();

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
            var reqSession = new PagingRequestSession<TResultType>(senderProxy, session);
            return reqSession.ExecuteAsync(CancellationToken.None);
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
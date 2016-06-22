using System;
using System.Threading;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;
using CareOtter.ServiceFabricUtilities.Interfaces;
using CareOtter.ServiceFabricUtilities.Services;

namespace CareOtter.ServiceFabricUtilities.Paging
{
    
    public class PagingHelper : IPagingHelper
    {
        protected readonly ICareOtterServiceProxyFactory _proxyFactory;
        public PagingHelper(ICareOtterServiceProxyFactory proxyFactory)
        {
            if(proxyFactory == null)
                throw new ArgumentNullException(nameof(proxyFactory));

            _proxyFactory = proxyFactory;
        }

        public Task<PagingId> SendDataPaged<T>(T dataToSend, Uri recieverUri)
        {
            var proxy = _proxyFactory.Create<IDataPageReciever>(recieverUri);
            return SendDataPaged(dataToSend, proxy);
        }

        public async Task<PagingId> SendDataPaged<T>(T dataToSend, IDataPageReciever recieverProxy)
        {
            var senderPackage = new PagedDataSenderPackage();
            senderPackage.Initialize<T>(dataToSend,65000);
            var pagingId = await recieverProxy.BeginPagingData(senderPackage.GetOriginDataLength());
            var curPage = senderPackage.GetNextPage();
            while (curPage.HasValue)
            {
                await recieverProxy.SendDataPage(pagingId, curPage.Value);
                curPage = senderPackage.GetNextPage();
            }
            return pagingId;
        }

        public Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session,
            IDataPageSender senderProxy, CancellationToken ct) where TResultType : class
        {
            var reqSession = new PagingRequestSession<TResultType>(senderProxy, session);
            return reqSession.ExecuteAsync(ct);
        }

        public Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session,
            IDataPageSender senderProxy) where TResultType : class
        {
            return GetPagedResults<TResultType>(session, senderProxy, CancellationToken.None);
        }

        public Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session, 
             CancellationToken ct) where TResultType : class
        {
            var senderProxy = _proxyFactory.Create<IDataPageSender>(session.OriginUri);
            return GetPagedResults<TResultType>(session, senderProxy, CancellationToken.None);
        }

        public Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session) 
            where TResultType : class
        {
            return GetPagedResults<TResultType>(session, CancellationToken.None);
        }

    }
}
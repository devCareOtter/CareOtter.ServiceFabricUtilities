using System;
using System.Threading;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;

namespace CareOtter.ServiceFabricUtilities.Interfaces
{
    public interface IPagingHelper
    {
        Task<PagingId> SendDataPaged<T>(T dataToSend, Uri recieverUri);
        Task<PagingId> SendDataPaged<T>(T dataToSend, IDataPageReciever recieverProxy);

        Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session,
            IDataPageSender senderProxy, CancellationToken ct) where TResultType : class;

        Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session,
            IDataPageSender senderProxy) where TResultType : class;

        Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session,
            CancellationToken ct) where TResultType : class;

        Task<TResultType> GetPagedResults<TResultType>(RequestedPagedDataSession session)
            where TResultType : class;
    }

}
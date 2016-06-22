using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.Paging
{
    public interface IDataPageManager
    {
        Task<PagingId> BeginPagingData(int originDataLength);
        Task SendDataPage(PagingId id, byte[] page);
        Task<T> GetDataAs<T>(PagingId id);
        Task EndPagingData(PagingId id);
        Task<RequestedPagedDataSession> PrepareDataForPaging<T>(T payload);
        Task<ConditionalValue<byte[]>> GetPage(PagingId id,int pageNum);
        Task NotifyPagingSessionComplete(PagingId id);
    }
}
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CareOtter.ServiceFabricUtilities.Interfaces
{
    public interface IDataPageSender : IService
    {
        Task<Response<byte[]>> GetPageAsync(PagingId sessionId,int pageNum);
        Task NotifyPagingComplete(PagingId sessionId);
    }
}
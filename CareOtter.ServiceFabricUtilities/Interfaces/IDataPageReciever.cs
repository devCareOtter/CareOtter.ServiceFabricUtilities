using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CareOtter.ServiceFabricUtilities.Interfaces
{
    public interface IDataPageReciever : IService
    {
        Task<PagingId> BeginPagingData(int originDataLength);
        Task SendDataPage(PagingId id, byte[] page);

        
    }
}
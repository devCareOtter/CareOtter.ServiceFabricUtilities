using System;
using System.Threading;
using System.Threading.Tasks;

namespace CareOtter.ServiceFabricUtilities.Interfaces
{
    public interface IPagingRequestSession<TResultType> 
    {
        Task<TResultType> ExecuteAsync(CancellationToken ct);
    }
}
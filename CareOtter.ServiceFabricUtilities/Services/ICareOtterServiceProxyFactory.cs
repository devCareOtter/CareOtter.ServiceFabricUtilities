using System;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CareOtter.ServiceFabricUtilities.Services
{
    public interface ICareOtterServiceProxyFactory
    {
        T Create<T>(Uri serviceUri, ServicePartitionKey patitionKey = null, 
            TargetReplicaSelector replicaSelector = TargetReplicaSelector.Default, string listenerName = null)
            where T : IService;

    }
}
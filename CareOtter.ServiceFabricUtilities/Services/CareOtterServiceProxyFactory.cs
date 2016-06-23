using System;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CareOtter.ServiceFabricUtilities.Services
{
    public class CareOtterServiceProxyFactory : ICareOtterServiceProxyFactory
    {
        public T Create<T>(Uri serviceUri, ServicePartitionKey patitionKey = null,
            TargetReplicaSelector replicaSelector = TargetReplicaSelector.Default, string listenerName = null) 
            where T : IService
        {
            return ServiceProxy.Create<T>(serviceUri, patitionKey, replicaSelector, listenerName);
        }
    }
}
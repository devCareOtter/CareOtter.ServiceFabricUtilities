using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;


namespace CareOtter.ServiceFabricUtilities.Observable
{
    [DataContract]
    [KnownType("GetKnownTypes")]
    public abstract class ServiceObserver
    {
        [DataMember]
        public Uri ServiceUri { get; set; }

        public virtual async Task ResolveAndNotify(MessageWrapper state)
        {
            var observer = await Resolve();

            await observer.Notify(state);
        }

        protected abstract Task<IReliableServiceObserver> Resolve();

        private static Type[] GetKnownTypes()
        {
            return typeof(ServiceObserver).Assembly.GetTypes().Where(t =>
                t.IsClass && t.BaseType == typeof(ServiceObserver) &&
                Attribute.IsDefined(t, typeof(DataContractAttribute))).ToArray();
        }

        public override string ToString()
        {
            return ServiceUri.AbsoluteUri;
        }
    }

    [DataContract]
    public class ReliableSingletonServiceObserver : ServiceObserver
    {
        protected override Task<IReliableServiceObserver> Resolve()
        {
            return Task.FromResult(ServiceProxy.Create<IReliableServiceObserver>(ServiceUri));
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    //TODO: rewrite this when needed, had to be killed due to removing SFUtil's dependency on Common

    [DataContract]
    public class ReliablePartitionedServiceObserver : ServiceObserver
    {
        [DataMember]
        public long PartitionId { get; set; }

        protected override Task<IReliableServiceObserver> Resolve()
        {
            return Task.FromResult(ServiceProxy.Create<IReliableServiceObserver>(ServiceUri, new ServicePartitionKey(PartitionId)));
        }

        public override string ToString()
        {

            return $"{base.ToString()}-{PartitionId.ToString()}";
        }
    }
}

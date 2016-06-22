using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Health;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using CareOtter.ServiceFabricUtilities.Containers;

namespace CareOtter.ServiceFabricUtilities.ServiceActivation
{
    /// <summary>
    /// Wrapper around fabric client service managers Service constructors
    /// </summary>
    public sealed class FabricServiceActivator : IServiceActivator
    {
        private object thisLock = new object();
        private FabricClient fabricClient;
        
        public FabricServiceActivator()
        {
            fabricClient = new FabricClient();
            
            
        }

        private async Task CreateServiceInternalAsync(ServiceDescription desc)
        {
            int excCount = 0;
            try
            {
                await fabricClient.ServiceManager.CreateServiceAsync(desc);

                //Add this to the known service types while we wait for it to become healthy
                
                //The service is not successfully created until it reports a healthy state
                ServiceHealth health = null;
                do
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(20));

                        health = await fabricClient.HealthManager.GetServiceHealthAsync(desc.ServiceName);
                    }
                    catch (FabricException e)
                    {

                        excCount++;
                        if (e.ErrorCode != FabricErrorCode.FabricHealthEntityNotFound || excCount > 10) //At some point we just give up and throw it
                            throw e;
                    }

                } while (health != null && health.AggregatedHealthState != System.Fabric.Health.HealthState.Ok);

                return;
            }
            catch (Exception e)
            {
                //THIS IS DIRTY, I DONT LIKE IT
                //oh you filthy beast you
                if (e.Message != "Service already exists.")
                    throw e;
            }
        }

        public async Task<IEnumerable<Uri>> GetKnownServicesExplicitAsync(Uri applicationName)
        {
            return await RefreshKnownServices(applicationName);
        }

        private async Task<IEnumerable<Uri>>  RefreshKnownServices(Uri applicationName)
        {
            var knownServices = await fabricClient.QueryManager.GetServiceListAsync(applicationName);
            return knownServices.Select(x => x.ServiceName);
        }

        private async Task<bool> CheckServiceExistsExplicitInternalAsync(Uri applicationName, Uri serviceUri)
        {
            return (await RefreshKnownServices(applicationName)).Contains(serviceUri);
        }

        private async Task DeleteServiceInternalAsync(Uri applicationName, Uri serviceUri)
        {
            await fabricClient.ServiceManager.DeleteServiceAsync(serviceUri);   
        }
        
        /// <summary>
        /// Creates a Stateless service. Abstracts the crappy fabric constructor
        /// </summary>
        /// <param name="ServiceName">Name of this service instance. Follow the format of "fabric:/{ApplicationName}/{ServiceType}{InstanceId}</param>
        /// <param name="serviceTypeName">Type name of this service. This should match the string that's registered with the fabric runtime</param>
        /// <param name="initializationData">Serialized data sent across the wire to the initializing service</param>
        /// <example>
        /// <code>
        /// class TestClass 
        /// {
        ///     static Task Main() 
        ///     {
        ///         return FabricServiceActivator.CreateStatelessServiceAsync(
        ///             new Uri("fabric:/CareOtter"), 
        ///             new Uri("fabric:/CareOtter/IndexPatientLNS"),
        ///             "IndexType",
        ///             new byte[0]
        ///             );
        ///     }
        /// }
        /// </code>
        /// </example>
        public async Task CreateStatelessServiceAsync(FabricServiceId ServiceName, string serviceTypeName, byte[] initializationData)
        {
            var svd = new StatelessServiceDescription
            {
                ApplicationName = ServiceName.ApplicationNameUri,
                ServiceTypeName = serviceTypeName,
                PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                ServiceName = ServiceName.ServiceUri,
                InitializationData = initializationData,
            };

            await CreateServiceAsync(svd);
        }

        /// <summary>
        /// Creates a Stateful service. Abstracts the crappy fabric constructor
        /// </summary>
        /// <param name="ServiceName">Name of this service instance. Follow the format of "fabric:/{ApplicationName}/{ServiceType}{InstanceId}</param>
        /// <param name="serviceTypeName">Type name of this service. This should match the string that's registered with the fabric runtime</param>
        /// <param name="initializationData">Serialized data sent across the wire to the initializing service</param>
        /// <param name="minReplicas">Minimum replica count</param>
        /// <param name="targetReplicas">Target replica count</param>
        /// <example>
        /// <code>
        /// class TestClass 
        /// {
        ///     static Task Main() 
        ///     {
        ///         return FabricServiceActivator.CreateStatefulServiceAsync(
        ///             new Uri("fabric:/CareOtter"), 
        ///             new Uri("fabric:/CareOtter/IndexPatientLNS"),
        ///             "IndexType",
        ///             new byte[0],
        ///             5,
        ///             3
        ///             );
        ///     }
        /// }
        /// </code>
        /// </example>
        public async Task CreateStatefulServiceAsync(FabricServiceId ServiceName, string serviceTypeName, byte[] initializationData, int targetReplicas = 3, int minReplicas = 2)
        {
            var svd = new StatefulServiceDescription
            {
                ApplicationName = ServiceName.ApplicationNameUri,
                ServiceTypeName = serviceTypeName,
                PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                ServiceName = ServiceName.ServiceUri,
                InitializationData = initializationData,
                TargetReplicaSetSize = targetReplicas,
                MinReplicaSetSize = minReplicas,
                HasPersistedState = true
            };


            await CreateServiceAsync(svd);
        }

        /// <summary>
        /// Creates a Stateful service. Abstracts the crappy fabric constructor
        /// </summary>
        /// <param name="ServiceName">Name of this service instance. Follow the format of "fabric:/{ApplicationName}/{ServiceType}{InstanceId}</param>
        /// <param name="serviceTypeName">Type name of this service. This should match the string that's registered with the fabric runtime</param>
        /// <param name="initializationData">Serialized data sent across the wire to the initializing service</param>
        /// <param name="partitionCount">Number of partitions</param>
        /// <param name="minReplicas">Minimum replica count</param>
        /// <param name="targetReplicas">Target replica count</param>
        /// <example>
        /// <code>
        /// class TestClass 
        /// {
        ///     static Task Main() 
        ///     {
        ///         return FabricServiceActivator.CreateStatefulServiceAsync(
        ///             new Uri("fabric:/CareOtter"), 
        ///             new Uri("fabric:/CareOtter/IndexPatientLNS"),
        ///             "IndexType",
        ///             new byte[0],
        ///             10,
        ///             5,
        ///             3
        ///             );
        ///     }
        /// }
        /// </code>
        /// </example>
        public async Task CreateStatefulPartitionedServiceAsync(FabricServiceId ServiceName, string serviceTypeName, byte[] initializationData, int partitionCount, int targetReplicas = 3, int minReplicas = 2)
        {
            var svd = new StatefulServiceDescription
            {
                ApplicationName = ServiceName.ApplicationNameUri,
                ServiceTypeName = serviceTypeName,
                PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription
                {
                    LowKey = 0,
                    HighKey = 103482,
                    PartitionCount = partitionCount
                },
                ServiceName = ServiceName.ServiceUri,
                InitializationData = initializationData,
                TargetReplicaSetSize = targetReplicas,
                MinReplicaSetSize = minReplicas,
                HasPersistedState = true
            };

            await CreateServiceAsync(svd);
        }

        /// <summary>
        /// Creates a service given the service description. Buyer beware, you can shoot your eye out kid!
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        public async Task CreateServiceAsync(ServiceDescription desc)
        {
            if (!await ServiceExistsAsync(new FabricServiceId(desc.ServiceName)))
            {
                await CreateServiceInternalAsync(desc);
            }
        }

        public async Task<bool> ServiceExistsAsync(FabricServiceId serviceName)
        {
            return await CheckServiceExistsExplicitInternalAsync(serviceName.ApplicationNameUri, serviceName.ServiceUri);
        }

        public async Task DeleteAsync(FabricServiceId serviceUri)
        {
            await DeleteAsync(serviceUri.ApplicationNameUri,serviceUri.ServiceUri);
        }

        public async Task DeleteAsync(Uri applicationName,Uri serviceUri)
        {
            await DeleteServiceInternalAsync(applicationName,serviceUri);
        }

        public Task<bool> CheckServiceExistsExplicitAsync(Uri applicationName, Uri serviceUri)
        {
            return CheckServiceExistsExplicitInternalAsync(applicationName, serviceUri);
        }
    }
}

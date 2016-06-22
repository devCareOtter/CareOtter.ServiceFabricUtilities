using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;

namespace CareOtter.ServiceFabricUtilities.ServiceActivation
{
    public interface IServiceActivator
    {
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
        Task CreateStatelessServiceAsync(FabricServiceId ServiceName, string serviceTypeName, byte[] initializationData);
        

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
        Task CreateStatefulServiceAsync(FabricServiceId ServiceName, string serviceTypeName, byte[] initializationData,
            int targetReplicas = 3, int minReplicas = 2);

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
        Task CreateStatefulPartitionedServiceAsync(FabricServiceId ServiceName, string serviceTypeName,
            byte[] initializationData, int partitionCount, int targetReplicas = 3, int minReplicas = 2);
        
        /// <summary>
        /// Creates a service given the service description. Buyer beware, you can shoot your eye out kid!
        /// </summary>
        /// <param name="desc"></param>
        
        /// <returns></returns>
        Task CreateServiceAsync(ServiceDescription desc);

        Task<bool> ServiceExistsAsync(FabricServiceId serviceName);

        Task DeleteAsync(FabricServiceId serviceUri);

        Task DeleteAsync(Uri applicationName, Uri serviceUri);

        Task<bool> CheckServiceExistsExplicitAsync(Uri applicationName, Uri serviceUri);

        Task<IEnumerable<Uri>> GetKnownServicesExplicitAsync(Uri applicationName);
    }
}
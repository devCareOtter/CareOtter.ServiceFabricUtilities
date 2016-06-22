using System;
using System.Fabric;
using CareOtter.ServiceFabricUtilities.Mocks.StateManager;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CareOtter.ServiceFabricUtilities.UnitTest.Mocks
{
    /// <summary>
    /// Factory to create services that use mock dependencies, for testing.
    /// </summary>
    public static class TestServiceFactory
    {
        // Create a StatefulService and RompFileRepository, that use mock dependencies.
        public static T ConstructStateful<T>(byte[] initializationData,
            Func<StatefulServiceContext, IReliableStateManagerReplica, string, T> constructorFunc,
            string entityName
,            bool throwOnNonCommit = false)
            where T : StatefulService
        {
            var stateManager = new MockServiceStateManager(throwOnNonCommit);

            var serviceContext = new StatefulServiceContext(
                new NodeContext(String.Empty, new NodeId(0, 0), 0, string.Empty, string.Empty),
                new MockCodePackageActivationContext(),
                String.Empty,
                new Uri("fabric:/mock"),
                initializationData,
                Guid.Empty,
                0);
            return constructorFunc(serviceContext, stateManager, entityName);
        }

        // Create a StatefulService that uses mock dependencies.
        public static T ConstructStateful<T>(byte[] initializationData,
            Func<StatefulServiceContext,IReliableStateManagerReplica,T> constructorFunc,
            bool throwOnNonCommit = false)
            where T : StatefulService
        {
            // ersatz persistence
            var stateManager = new MockServiceStateManager(throwOnNonCommit);

            var serviceContext = new StatefulServiceContext(
                new NodeContext(String.Empty, new NodeId(0, 0), 0, string.Empty, string.Empty),
                new MockCodePackageActivationContext(),
                String.Empty,
                new Uri("fabric:/mock"),
                initializationData,
                Guid.Empty,
                0);
            return constructorFunc(serviceContext, stateManager);
        }

        public static T ConstructStateless<T>(byte[] initializationData,
            Func<StatelessServiceContext, T> constructorFunc)
            where T : StatelessService
        {
            var serviceContext = new StatelessServiceContext(
                new NodeContext(String.Empty, new NodeId(0, 0), 0, String.Empty, String.Empty),
                new MockCodePackageActivationContext(),
                String.Empty,
                new Uri("fabric:/mock"),
                initializationData,
                Guid.Empty,
                0);
            return constructorFunc(serviceContext);
        }
    }
}
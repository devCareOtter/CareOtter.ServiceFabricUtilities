using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Containers;
using CareOtter.ServiceFabricUtilities.Interfaces;
using CareOtter.ServiceFabricUtilities.Paging;
using CareOtter.ServiceFabricUtilities.Services;
using CareOtter.ServiceFabricUtilities.UnitTest.Mocks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CareOtter.ServiceFabricUtilities.UnitTest
{

    [TestClass]
    public class PagingTests
    {
        private readonly Mock<ICareOtterServiceProxyFactory> _serviceProxyFactoryMock;
        private readonly MockRepository _mockRepository;

        public PagingTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _serviceProxyFactoryMock = _mockRepository.Create<ICareOtterServiceProxyFactory>();
        }

        [TestMethod]
        public async Task PagingSerialization_SerializeDeserialize_CopyMatchesOriginal()
        {
            //arrange
            var originalData = new List<int>();
            for(int i = 0; i < 1000000; i++)
                originalData.Add(i);

            //act
            var senderPackage = new PagedDataSenderPackage();
            senderPackage.Initialize<List<int>>(originalData,100000);
            var reciever = new UnreliableDataPageManager();
            var id = await reciever.BeginPagingData(senderPackage.GetOriginDataLength());
            var page = senderPackage.GetNextPage();
            while (page.HasValue)
            {
                await reciever.SendDataPage(id,page.Value);
                page = senderPackage.GetNextPage();
            }

            var resultObject = await reciever.GetDataAs<List<int>>(id);
            await reciever.EndPagingData(id);
            //assert
            Assert.AreEqual(originalData.Count,resultObject.Count);
            Assert.IsTrue(originalData.SequenceEqual(resultObject));
        }

        [TestMethod]
        public async Task PagingBaseService_StatefulSendPagedItem_ManyItems_ThreadSafety()
        {
            //arrange
            var serviceOne = TestServiceFactory.ConstructStateful(new byte[0],
                (context, stateManager) =>
                    new PagingTestStatefulService(context, stateManager, _serviceProxyFactoryMock.Object,
                        (e) => Console.Write(e)));

            var serviceTwo = TestServiceFactory.ConstructStateful(new byte[0],
                (context, stateManager) =>
                    new PagingTestStatefulService(context, stateManager, _serviceProxyFactoryMock.Object,
                        (e) => Console.Write(e)));

            //use the data page helper on the service One to send data to serviceTwo 
            _serviceProxyFactoryMock.Setup(
                x => x.Create<IDataPageReciever>(It.IsAny<Uri>(), null, TargetReplicaSelector.Default, null))
                .Returns(serviceTwo);

            var originData = new List<TestPagingPayload>();
            for(int i = 0; i < 200; ++i)
                originData.Add(new TestPagingPayload() {PayloadNum = i,DummyData = new byte[1000000 + i]});

            //act
            var pagingTasksResults = originData.Select(x => serviceOne.SendDataPaged(x, new Uri("test:/test")));
            var pageIds = await Task.WhenAll(pagingTasksResults);

            var results = await Task.WhenAll(pageIds.Select(x => serviceTwo.GetPagedResults<TestPagingPayload>(x)));

            //assert
            var orderedResults = results.OrderBy(x => x.PayloadNum);
            Assert.IsTrue(originData.SequenceEqual(orderedResults));
        }

        [TestMethod]
        public async Task PagingBaseService_StatefulRecievePagedItem_ManyItems_ThreadSafety()
        {
            //arrange
            var serviceOne = TestServiceFactory.ConstructStateful(new byte[0],
                (context, stateManager) =>
                    new PagingTestStatefulService(context, stateManager, _serviceProxyFactoryMock.Object,
                        (e) => Console.Write(e)));

            var serviceTwo = TestServiceFactory.ConstructStateful(new byte[0],
                (context, stateManager) =>
                    new PagingTestStatefulService(context, stateManager, _serviceProxyFactoryMock.Object,
                        (e) => Console.Write(e)));

            //use the data page helper on the service One to send data to serviceTwo 
            _serviceProxyFactoryMock.Setup(
                x => x.Create<IDataPageSender>(It.IsAny<Uri>(), null, TargetReplicaSelector.Default, null))
                .Returns(serviceOne);

            var originData = new List<TestPagingPayload>();
            for (int i = 0; i < 200; ++i)
                originData.Add(new TestPagingPayload() { PayloadNum = i, DummyData = new byte[1000000 + i] });

            //act
            var requestTasks = originData.Select(x => serviceOne.PrepareForPaging(x));
            var requests = await Task.WhenAll(requestTasks);
            var resultTasks = requests.Select(x => serviceTwo.GetPagedResults<TestPagingPayload>(x));
            var results = await Task.WhenAll(resultTasks);


            //assert
            var orderedResults = results.OrderBy(x => x.PayloadNum);
            Assert.IsTrue(originData.SequenceEqual(orderedResults));
        }


        [TestMethod]
        public async Task PagingBaseService_StatelessSendPagedItem_ManyItems_ThreadSafety()
        {
            //arrange
            var serviceOne = TestServiceFactory.ConstructStateless(new byte[0],
                (context) =>
                    new PagingTestStatelessService(context, _serviceProxyFactoryMock.Object, e => Console.Write(e)));

            var serviceTwo = TestServiceFactory.ConstructStateless(new byte[0],
                (context) =>
                    new PagingTestStatelessService(context, _serviceProxyFactoryMock.Object, e => Console.Write(e)));


            //use the data page helper on the service One to send data to serviceTwo 
            _serviceProxyFactoryMock.Setup(
                x => x.Create<IDataPageReciever>(It.IsAny<Uri>(), null, TargetReplicaSelector.Default, null))
                .Returns(serviceTwo);

            var originData = new List<TestPagingPayload>();
            for (int i = 0; i < 200; ++i)
                originData.Add(new TestPagingPayload() { PayloadNum = i, DummyData = new byte[1000000 + i] });

            //act
            var pagingTasksResults = originData.Select(x => serviceOne.SendDataPaged(x, new Uri("test:/test")));
            var pageIds = await Task.WhenAll(pagingTasksResults);

            var results = await Task.WhenAll(pageIds.Select(x => serviceTwo.GetPagedResults<TestPagingPayload>(x)));

            //assert
            var orderedResults = results.OrderBy(x => x.PayloadNum);
            Assert.IsTrue(originData.SequenceEqual(orderedResults));
        }

        [TestMethod]
        public async Task PagingBaseService_StatelessRecievePagedItem_ManyItems_ThreadSafety()
        {
            //arrange
            var serviceOne = TestServiceFactory.ConstructStateless(new byte[0],
                (context) =>
                    new PagingTestStatelessService(context, _serviceProxyFactoryMock.Object, e => Console.Write(e)));


            var serviceTwo = TestServiceFactory.ConstructStateless(new byte[0],
                (context) =>
                    new PagingTestStatelessService(context, _serviceProxyFactoryMock.Object, e => Console.Write(e)));


            //use the data page helper on the service One to send data to serviceTwo 
            _serviceProxyFactoryMock.Setup(
                x => x.Create<IDataPageSender>(It.IsAny<Uri>(), null, TargetReplicaSelector.Default, null))
                .Returns(serviceOne);

            var originData = new List<TestPagingPayload>();
            for (int i = 0; i < 200; ++i)
                originData.Add(new TestPagingPayload() { PayloadNum = i, DummyData = new byte[1000000 + i] });

            //act
            var requestTasks = originData.Select(x => serviceOne.PrepareForPaging(x));
            var requests = await Task.WhenAll(requestTasks);
            var resultTasks = requests.Select(x => serviceTwo.GetPagedResults<TestPagingPayload>(x));
            var results = await Task.WhenAll(resultTasks);


            //assert
            var orderedResults = results.OrderBy(x => x.PayloadNum);
            Assert.IsTrue(originData.SequenceEqual(orderedResults));
        }


        [DataContract]
        public class TestPagingPayload : IEquatable<TestPagingPayload>
        {
            [DataMember]
            public int PayloadNum { get; set; }
            [DataMember]
            public byte[] DummyData { get; set; }

            public bool Equals(TestPagingPayload other)
            {
                return other != null && other.PayloadNum == PayloadNum && other.DummyData.Count() == DummyData.Count();
            }

            
        }
    }


    public class PagingTestStatefulService : CareOtterStatefulServiceBase
    {
        public PagingTestStatefulService(StatefulServiceContext context,
            ICareOtterServiceProxyFactory serviceProxyFactory, Action<Exception> exceptionHandler)
            : base(context, serviceProxyFactory, exceptionHandler)
        {
        }

        public PagingTestStatefulService(StatefulServiceContext context, IReliableStateManagerReplica stateManager,
            ICareOtterServiceProxyFactory serviceProxyFactory, Action<Exception> exceptionHandler)
            : base(context, stateManager, serviceProxyFactory, exceptionHandler)
        {
        }

        public override Task SafeRunAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class PagingTestStatelessService : CareOtterStatelessServiceBase
    {
        public PagingTestStatelessService(StatelessServiceContext context,
            ICareOtterServiceProxyFactory serviceProxyFactory, Action<Exception> exceptionHandler)
            : base(context, serviceProxyFactory, exceptionHandler)
        {
        }

        public override Task SafeRunAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
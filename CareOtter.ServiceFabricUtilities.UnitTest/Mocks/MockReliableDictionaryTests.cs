using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Mocks.StateManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CareOtter.ServiceFabricUtilities.UnitTest.Mocks
{
    [TestClass]
    public class MockReliableDictionaryTests
    {
        [TestMethod]
        public async Task MockReliableDict_CountAsync_ValidOutput()
        {
            var dict = new MockReliableDictionary<int,int>();
            var tx = new MockTransaction(false);

            for (int i = 0; i < 100; ++i)
                await dict.AddAsync(tx, i, i);

            var count = await dict.GetCountAsync(tx);

            Assert.AreEqual(100,count);
        }

        [TestMethod]
        public async Task MockReliableDict_AddAsync_ContainsElem()
        {
            var dict = new MockReliableDictionary<int, int>();
            var tx = new MockTransaction(false);

            await dict.AddAsync(tx, 5, 5);

            Assert.IsTrue(await dict.ContainsKeyAsync(tx,5));
        }

        [TestMethod]
        public async Task MockReliableDict_AddOrUpdateAsync_ValidOutput()
        {
            var dict = new MockReliableDictionary<int, int>();
            var tx = new MockTransaction(false);

            //add, 5=>5
            await dict.AddOrUpdateAsync(tx, 5, k => 5, (k, v) => v + 1);

            Assert.AreEqual(1,await dict.GetCountAsync(tx));
            var res = await dict.TryGetValueAsync(tx, 5);
            Assert.IsTrue(res.HasValue);
            Assert.AreEqual(5,res.Value);

            //update 5=>6
            await dict.AddOrUpdateAsync(tx, 5, k => 5, (k, v) => v + 1);

            Assert.AreEqual(1, await dict.GetCountAsync(tx));
            var res2 = await dict.TryGetValueAsync(tx, 5);
            Assert.IsTrue(res2.HasValue);
            Assert.AreEqual(6, res2.Value);
        }

        [TestMethod]
        public async Task MockReliableDict_TryGet_ValidOutput()
        {
            var dict = new MockReliableDictionary<int, int>();
            var tx = new MockTransaction(false);

            var res = await dict.TryGetValueAsync(tx, 5);
            Assert.IsFalse(res.HasValue);

            await dict.AddAsync(tx, 5, 5);
            var res2 = await dict.TryGetValueAsync(tx, 5);
            Assert.IsTrue(res2.HasValue);
            Assert.AreEqual(5,res2.Value);

        }

        [TestMethod]
        public async Task MockReliableDict_GetOrAdd_ValidOutput()
        {
            var dict = new MockReliableDictionary<int, int>();
            var tx = new MockTransaction(false);

            var res = await dict.GetOrAddAsync(tx, 5, k => 5);
            Assert.AreEqual(5,res);

            var res2 = await dict.GetOrAddAsync(tx, 5, k => 6);
            Assert.AreEqual(5,res2);
        }
    }
}
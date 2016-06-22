using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Mocks.StateManager;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CareOtter.ServiceFabricUtilities.UnitTest.Mocks
{
    [TestClass]
    public class MockStateManagerTests
    {
        [TestMethod]
        public async Task MockStateManager_GetOrAdd_ProperType_SameDict()
        {
            var stateManager = new MockServiceStateManager();

            var dict = await stateManager.GetOrAddAsync<IReliableDictionary<int, int>>("dict");
            Assert.IsTrue(dict is MockReliableDictionary<int,int>);
            using (var tx = stateManager.CreateTransaction())
            {
                await dict.AddAsync(tx, 5, 5);
                await tx.CommitAsync();
            }

            var dict2 = await stateManager.GetOrAddAsync<IReliableDictionary<int, int>>("dict");
            Assert.IsTrue(dict2 is MockReliableDictionary<int,int>);
            using (var tx = stateManager.CreateTransaction())
            {
                var res = await dict2.TryGetValueAsync(tx, 5);
                Assert.IsTrue(res.HasValue);
                Assert.AreEqual(5,res.Value);
            }
            
        }

        [TestMethod]
        public async Task MockStateManager_TryGet_DoesntExist()
        {
            var stateManager = new MockServiceStateManager();

            var cond = await stateManager.TryGetAsync<IReliableDictionary<int, int>>("dict");
            Assert.IsFalse(cond.HasValue);

            var dict = await stateManager.GetOrAddAsync<IReliableDictionary<int, int>>("dict");
            Assert.IsTrue(dict is MockReliableDictionary<int, int>);
            using (var tx = stateManager.CreateTransaction())
            {
                await dict.AddAsync(tx, 5, 5);
                await tx.CommitAsync();
            }

            var cond2 = await stateManager.TryGetAsync<IReliableDictionary<int, int>>("dict");
            Assert.IsTrue(cond2.HasValue);
        }

        [TestMethod]
        public async Task MockStateManager_RemoveAsync()
        {
            var stateManager = new MockServiceStateManager();
            
            var dict = await stateManager.GetOrAddAsync<IReliableDictionary<int, int>>("dict");
            Assert.IsTrue(dict is MockReliableDictionary<int, int>);
            using (var tx = stateManager.CreateTransaction())
            {
                await dict.AddAsync(tx, 5, 5);
                await tx.CommitAsync();
            }

            var cond2 = await stateManager.TryGetAsync<IReliableDictionary<int, int>>("dict");
            Assert.IsTrue(cond2.HasValue);

            using (var tx = stateManager.CreateTransaction())
            {
                await stateManager.RemoveAsync(tx, "dict");
            }
            
            var cond = await stateManager.TryGetAsync<IReliableDictionary<int, int>>("dict");
            Assert.IsFalse(cond.HasValue);
        }
    }
}
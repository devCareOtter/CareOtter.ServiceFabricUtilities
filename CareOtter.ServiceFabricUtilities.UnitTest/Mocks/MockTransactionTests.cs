using System;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.Mocks.StateManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CareOtter.ServiceFabricUtilities.UnitTest.Mocks
{
    [TestClass]
    public class MockTransactionTests
    {
        [TestMethod]
        public void MockTransaction_NoCommit_NoThrow_NoException()
        {
            var tx = new MockTransaction(false);

            tx.Dispose();
        }

        [TestMethod]
        public async Task MockTransaction_Commit_NoThrow_NoException()
        {
            var tx = new MockTransaction(false);

            await tx.CommitAsync();

            tx.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MockTransaction_NoCommit_Throw_ExpectedException()
        {
            var tx = new MockTransaction(true);

            tx.Dispose();
        }

        [TestMethod]
        public async Task MockTransaction_Commit_Throw_NoException()
        {
            var tx = new MockTransaction(true);

            await tx.CommitAsync();

            tx.Dispose();
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.Mocks.StateManager
{
    public class MockTransaction : ITransaction
    {
        public bool ThrowOnNonCommit { get; set; }
        private bool _hasCommitted = false;
        public MockTransaction(bool throwOnNonCommit)
        {
            ThrowOnNonCommit = throwOnNonCommit;

        }

        public void Dispose()
        {
            if(ThrowOnNonCommit && !_hasCommitted)
                throw new InvalidOperationException("Transaction not committed before disposal");
        }


        public Task CommitAsync()
        {
            _hasCommitted = true;
            return Task.FromResult(true);
        }

        public void Abort()
        {
            //not really, but the purpose of that is to test proper tx usage and this is the no-op method of doing that
            _hasCommitted = true;
        }

        public Task<long> GetVisibilitySequenceNumberAsync()
        {
            throw new System.NotImplementedException();
        }

        public long CommitSequenceNumber { get; }
        public long TransactionId { get; }
    }
}
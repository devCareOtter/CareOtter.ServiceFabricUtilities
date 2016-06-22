using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace CareOtter.ServiceFabricUtilities.AsyncEnumerable.Enumerators
{
    public class AsyncTakeWhileEnumerator<TValType> : IAsyncEnumerator<TValType>
    {
        private readonly IAsyncEnumerator<TValType> _parentEnumerator;
        private readonly Func<TValType, bool> _takeFunc;

        public AsyncTakeWhileEnumerator(IAsyncEnumerator<TValType> parentEnumerator, Func<TValType, bool> takeFunc)
        {
            _parentEnumerator = parentEnumerator;
            _takeFunc = takeFunc;
        }  

        public void Dispose()
        {
            _parentEnumerator.Dispose();
        }

        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            if (!(await _parentEnumerator.MoveNextAsync(cancellationToken).ConfigureAwait(false)))
                return false;

            if (!_takeFunc(_parentEnumerator.Current))
                return false;

            return true;
        }

        public void Reset()
        {
            _parentEnumerator.Reset();
        }

        public TValType Current => _parentEnumerator.Current;
    }
}